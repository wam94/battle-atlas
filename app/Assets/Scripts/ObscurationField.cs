using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // Renders the obscuration field: powder smoke from authored engagement
    // events plus dust DERIVED from childless units' motion (never authored —
    // battle-format.md "Engagement events"). A thin shell over
    // ObscurationMath: every frame it recomputes the full set of live puffs
    // as a pure function of (battle data, wind, t) into persistent per-age-
    // bucket matrix buffers and draws each non-empty bucket with ONE
    // RenderMeshInstanced call — 4 smoke + 4 dust buckets, ≤ 8 draw calls,
    // zero steady-state allocation (the FormationLayout discipline). No
    // particle simulation anywhere, so scrubbing in either direction — or
    // jump-cuts — reproduces identical fields. Created and wired by
    // BattleDirector in Start (AddComponent + Init), like the formation
    // renderers — not a scene component.
    public class ObscurationField : MonoBehaviour
    {
        // per-bucket capacity: 8 buckets x 384 = 3072 total, comfortably
        // above the plan's worst-case estimate of ~1,200 live puffs at the
        // bombardment peak / advance
        public const int BucketCount = 4;
        public const int BucketCapacity = 384;
        // scratch for ONE emitter's live puffs: artillery worst case is
        // life/cadence (~23) ticks x the Seminary Ridge line's per-tick
        // spread (~1800 m / 150 = 12) ≈ 270; 512 leaves headroom
        const int ScratchCapacity = 512;
        // smoke floats above the emitter and keeps rising as it ages; dust
        // hugs the ground and flattens
        const float SmokeBaseHeight = 4f;
        const float SmokeRisePerAge = 6f;
        const float SmokeSquash = 0.8f;
        const float DustSquash = 0.45f;
        const float DustHeightFactor = 0.35f; // of radius, above ground

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // the 4 age buckets fade out; COLOR comes from the material asset's
        // _BaseColor (tuned in Task 7, Smoke.mat grey-white / Dust.mat tan),
        // only the alpha rides the per-bucket MaterialPropertyBlock
        static readonly float[] BucketAlpha = { 0.55f, 0.42f, 0.28f, 0.14f };

        // an emitter ObscurationMath can evaluate: the event plus its cached
        // position delegate (one closure per emitter built in Init — never
        // per frame) and its cached per-kind life for the overlap cull.
        // Dust emitters are synthetic events (kind "dust", full battle
        // window) over each childless unit's own track — the family rule:
        // a decomposed brigade's parent track never dusts, its children do.
        struct Emitter
        {
            public EventDto Ev;
            public System.Func<float, Vector2> PosAt; // null = fixed segment form
            public UnitTrack DustTrack; // non-null only for derived dust
            public float Life;
        }

        BattleClock clock;
        Terrain terrain;
        Material smokeMaterial;
        Material dustMaterial;
        Emitter[] smokeEmitters;
        Emitter[] dustEmitters;

        Mesh puffMesh;
        Puff[] scratch;
        Matrix4x4[][] smokeBuckets;
        Matrix4x4[][] dustBuckets;
        int[] smokeCounts;
        int[] dustCounts;
        MaterialPropertyBlock[] smokeBlocks;
        MaterialPropertyBlock[] dustBlocks;
        Bounds fieldBounds;
        Vector2 wind;
        float groundYBase;
        bool warnedNoMaterial;
        bool warnedOverflow;

        // Called by BattleDirector after it parses the battle and links
        // families. Event emitter positions resolve from each event's OWN
        // unit's track at t — NOT from what the family LOD currently
        // renders: a hidden parent or hidden child is still the attested
        // emitter (battle-format.md attach-level rule).
        public void Init(
            BattleDto battle, Dictionary<string, UnitTrack> tracksById,
            List<UnitTrack> childlessTracks, BattleClock clock, Terrain terrain,
            Material smokeMaterial, Material dustMaterial)
        {
            this.clock = clock;
            this.terrain = terrain;
            this.smokeMaterial = smokeMaterial;
            this.dustMaterial = dustMaterial;
            // asset references keep the shaders in device builds (the
            // magenta/stripping lesson); instancing flags are safe to set
            if (smokeMaterial != null) smokeMaterial.enableInstancing = true;
            if (dustMaterial != null) dustMaterial.enableInstancing = true;

            int eventCount = battle.events != null ? battle.events.Count : 0;
            smokeEmitters = new Emitter[eventCount];
            for (int i = 0; i < eventCount; i++)
            {
                EventDto ev = battle.events[i];
                System.Func<float, Vector2> posAt = null;
                if (!string.IsNullOrEmpty(ev.unitId))
                {
                    UnitTrack track = tracksById[ev.unitId]; // loader guaranteed it resolves
                    posAt = tEmit => track.StateAt(tEmit).posXZ;
                }
                smokeEmitters[i] = new Emitter
                { Ev = ev, PosAt = posAt, Life = PuffParams.For(ev.kind).life };
            }

            float dustLife = PuffParams.For("dust").life;
            dustEmitters = new Emitter[childlessTracks.Count];
            for (int i = 0; i < childlessTracks.Count; i++)
            {
                UnitTrack track = childlessTracks[i];
                // synthetic full-window event; which ticks actually shed a
                // puff is decided per frame by the unit's speed at tEmit
                var ev = new EventDto
                {
                    id = "dust:" + track.Unit.id,
                    kind = "dust",
                    t0 = battle.startTime,
                    t1 = battle.endTime,
                };
                dustEmitters[i] = new Emitter
                {
                    Ev = ev,
                    PosAt = tEmit => track.StateAt(tEmit).posXZ,
                    DustTrack = track,
                    Life = dustLife,
                };
            }

            // JsonUtility quirk (BattleData.cs): an absent environment block
            // deserializes as a zeroed instance — windMps 0 = calm = zero
            // drift, exactly the right fallback
            wind = battle.environment != null
                ? ObscurationMath.WindVector(
                    battle.environment.windTowardDeg, battle.environment.windMps)
                : Vector2.zero;

            puffMesh = InstancedMeshes.BuildPuff();
            scratch = new Puff[ScratchCapacity];
            smokeBuckets = NewBuckets();
            dustBuckets = NewBuckets();
            smokeCounts = new int[BucketCount];
            dustCounts = new int[BucketCount];
            smokeBlocks = BucketBlocks(smokeMaterial);
            dustBlocks = BucketBlocks(dustMaterial);

            // one conservative cull bounds for the whole field: the terrain
            // extent inflated for wind drift past the edges and rising smoke
            Vector3 size = terrain.terrainData.size;
            fieldBounds = new Bounds(
                terrain.transform.position + size * 0.5f,
                size + new Vector3(1000f, 400f, 1000f));
        }

        static Matrix4x4[][] NewBuckets()
        {
            var buckets = new Matrix4x4[BucketCount][];
            for (int b = 0; b < BucketCount; b++)
                buckets[b] = new Matrix4x4[BucketCapacity];
            return buckets;
        }

        // per-bucket alpha over the material's own base color, so Task 7
        // tunes the smoke/dust tint on the ASSET and the fade stays here
        static MaterialPropertyBlock[] BucketBlocks(Material material)
        {
            Color baseColor = material != null && material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId) : Color.white;
            var blocks = new MaterialPropertyBlock[BucketCount];
            for (int b = 0; b < BucketCount; b++)
            {
                baseColor.a = BucketAlpha[b];
                blocks[b] = new MaterialPropertyBlock();
                blocks[b].SetColor(BaseColorId, baseColor);
            }
            return blocks;
        }

        void Update()
        {
            if (smokeEmitters == null) return; // not initialized yet
            if (smokeMaterial == null || dustMaterial == null)
            {
                // loud once, render nothing — never a silent or magenta fake
                // (the flag-material-unset warning is the house pattern)
                if (!warnedNoMaterial)
                {
                    Debug.LogWarning(
                        "ObscurationField: smoke/dust material unset; run " +
                        "BattleAtlas/Setup Obscuration to create and wire " +
                        "Assets/Battle/Smoke.mat + Dust.mat — rendering no obscuration");
                    warnedNoMaterial = true;
                }
                return;
            }

            float t = clock.CurrentTime;
            groundYBase = terrain.transform.position.y;
            for (int b = 0; b < BucketCount; b++)
            {
                smokeCounts[b] = 0;
                dustCounts[b] = 0;
            }
            for (int i = 0; i < smokeEmitters.Length; i++)
                Accumulate(smokeEmitters[i], t, smokeBuckets, smokeCounts);
            for (int i = 0; i < dustEmitters.Length; i++)
                Accumulate(dustEmitters[i], t, dustBuckets, dustCounts);

            Render(smokeMaterial, smokeBuckets, smokeCounts, smokeBlocks);
            Render(dustMaterial, dustBuckets, dustCounts, dustBlocks);
        }

        // one emitter's live puffs -> TRS matrices in the age buckets
        void Accumulate(in Emitter em, float t, Matrix4x4[][] buckets, int[] counts)
        {
            // lookback cull: outside [t0, t1 + life] an event has no live
            // puffs — skip the scratch fill entirely
            if (t < em.Ev.t0 || t > em.Ev.t1 + em.Life) return;
            bool dust = em.DustTrack != null;
            int n = ObscurationMath.LivePuffs(em.Ev, em.PosAt, wind, t, scratch);
            for (int i = 0; i < n; i++)
            {
                Puff p = scratch[i];
                if (dust)
                {
                    // derived dust: only ticks where the unit was actually
                    // moving shed a puff. tEmit falls back out of age01, so
                    // the filter stays a pure function of t — same answer in
                    // both scrub directions.
                    float tEmit = t - p.age01 * em.Life;
                    if (ObscurationMath.DustSpeedAt(em.DustTrack, tEmit)
                        <= ObscurationMath.DustSpeedThresholdMps)
                        continue;
                }
                int b = ObscurationMath.AgeBucket(p.age01);
                if (counts[b] >= BucketCapacity)
                {
                    // deterministic clamp: fixed emitter order + LivePuffs's
                    // newest-first fill means the same t always drops the
                    // same puffs. Loud once, never silently forever.
                    if (!warnedOverflow)
                    {
                        Debug.LogWarning(
                            $"ObscurationField: age bucket {b} overflowed its " +
                            $"{BucketCapacity}-puff capacity; clamping (live puffs " +
                            "exceed the plan's worst-case estimate)");
                        warnedOverflow = true;
                    }
                    continue;
                }
                float ground = groundYBase + terrain.SampleHeight(
                    new Vector3(p.posXZ.x, 0f, p.posXZ.y));
                float y;
                Vector3 scale;
                if (dust)
                {
                    y = ground + p.radius * DustHeightFactor;
                    scale = new Vector3(p.radius, p.radius * DustSquash, p.radius);
                }
                else
                {
                    y = ground + SmokeBaseHeight + SmokeRisePerAge * p.age01;
                    scale = new Vector3(p.radius, p.radius * SmokeSquash, p.radius);
                }
                buckets[b][counts[b]++] = Matrix4x4.TRS(
                    new Vector3(p.posXZ.x, y, p.posXZ.y), Quaternion.identity, scale);
            }
        }

        void Render(
            Material material, Matrix4x4[][] buckets, int[] counts,
            MaterialPropertyBlock[] blocks)
        {
            var rp = new RenderParams(material)
            {
                // a translucent blob's shadow would be a solid box shadow —
                // and shadows are off project-wide anyway; explicit, not
                // default-On (the B1/B8 trap)
                shadowCastingMode = ShadowCastingMode.Off,
                worldBounds = fieldBounds,
            };
            for (int b = 0; b < BucketCount; b++)
            {
                if (counts[b] == 0) continue;
                rp.matProps = blocks[b];
                Graphics.RenderMeshInstanced(rp, puffMesh, 0, buckets[b], counts[b]);
            }
        }
    }
}
