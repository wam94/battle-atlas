using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Loads the battle JSON, spawns a block marker per unit, and poses every
    // marker each frame from the clock's current time. Three LOD tiers by
    // camera distance: the brigade block beyond RegimentsInDist, instanced
    // regiment sub-blocks (one per roster entry, laid out by RegimentSlots)
    // in the middle band, and instanced soldier ranks inside SoldiersInDist
    // (with a hysteresis band at each boundary to avoid flicker). Units
    // without a regiments roster — or in scattered/routed formation, where
    // ordered sub-blocks would be a lie — keep the monolithic block at the
    // middle tier.
    public class BattleDirector : MonoBehaviour
    {
        public TextAsset battleJson;
        public Terrain terrain;
        public BattleClock clock;
        // a real material ASSET (not a runtime instance): asset references keep
        // the shader in device builds, where runtime-created materials render
        // magenta because the shader was stripped
        public Material unitMaterial;
        // material for instanced soldier figures; falls back to unitMaterial
        // when left unset in the inspector (e.g. older scenes)
        public Material soldierMaterial;

        // visible clearance of the block's top face above the highest ground in
        // its footprint (the block extends down to the lowest ground, embedding
        // in the hillside like a piece on a physical relief map)
        const float MarkerHeight = 6f;
        // LOD hysteresis: soldiers resolve in below SoldiersInDist and hold
        // until SoldiersOutDist; regiment sub-blocks resolve in below
        // RegimentsInDist and hold until RegimentsOutDist. The band at each
        // boundary (150m / 400m) prevents flicker when the camera hovers there.
        const float SoldiersInDist = 1500f;
        const float SoldiersOutDist = 1650f;
        const float RegimentsInDist = 4000f;
        const float RegimentsOutDist = 4400f;
        // regiment rosters realistically run <= 10; sized with headroom so the
        // per-unit matrix buffer never reallocates (over-long rosters clamp)
        const int MaxRegiments = 16;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // corners first (order is load-bearing for tests), then edge midpoints —
        // a denser ring catches ground rising inside the footprint, not just at
        // its extremes
        static readonly (float dx, float dz)[] CornerOffsets =
        {
            (-0.5f, -0.5f), (0.5f, -0.5f), (-0.5f, 0.5f), (0.5f, 0.5f),
            (0f, -0.5f), (0f, 0.5f), (-0.5f, 0f), (0.5f, 0f),
        };

        enum LodTier { Soldiers, Regiments, Block }

        // per-unit runtime state: the block marker plus everything needed to
        // switch it for regiment sub-blocks or soldier ranks at closer range.
        // A small class beats a growing tuple once it carries a latch and a
        // renderer.
        class UnitEntry
        {
            public UnitTrack Track;
            public Transform Marker;
            public UnitFormationRenderer FormationRenderer;
            public LodTier Tier;
            // middle-tier state, null/0 when the unit has no regiments roster
            public int RegimentCount;
            public Matrix4x4[] RegimentMatrices;
            public MaterialPropertyBlock ColorBlock;
        }

        readonly List<UnitEntry> units = new();
        readonly Vector2[] samplePoints = new Vector2[9];
        // scratch for RegimentSlots, reused across units within one Update
        readonly (Vector2 center, Vector2 size)[] slotsBuffer =
            new (Vector2, Vector2)[MaxRegiments];
        Mesh soldierMesh;
        Mesh unitBoxMesh;
        Camera lodCamera;
        // cached once in Start: avoids allocating a closure per unit per
        // frame. groundYBase is refreshed once per Update (not per unit)
        // since GroundY reads it on every call.
        System.Func<float, float, float> groundYFunc;
        float groundYBase;

        public static Color SideColor(string side)
        {
            switch (side)
            {
                case "union": return new Color(0.23f, 0.35f, 0.61f);       // deep blue
                case "confederate": return new Color(0.63f, 0.31f, 0.31f); // muted red
                default:
                    // gray keeps rendering but a typo'd side in authored data
                    // should be loud, not invisible
                    Debug.LogWarning($"unknown unit side '{side}', rendering gray");
                    return Color.gray;
            }
        }

        // Fills the buffer with world-XZ sample points under a unit: center,
        // footprint corners, and edge midpoints, rotated by facing. Update
        // stretches the block from the MIN to the MAX ground height of these,
        // embedding it in the slope.
        public static void FootprintSamplePoints(
            Vector2 centerXZ, float facingDeg, float frontage, float depth, Vector2[] buffer)
        {
            var rot = Quaternion.Euler(0f, facingDeg, 0f);
            buffer[0] = centerXZ;
            for (int i = 0; i < CornerOffsets.Length; i++)
            {
                Vector3 off = rot * new Vector3(
                    CornerOffsets[i].dx * frontage, 0f, CornerOffsets[i].dz * depth);
                buffer[i + 1] = new Vector2(centerXZ.x + off.x, centerXZ.y + off.z);
            }
        }

        // groundY: world-space terrain height under the unit. Marker pivot is
        // its center, so lift by half its height to sit on the ground.
        public static (Vector3 pos, Quaternion rot) MarkerPose(
            UnitState state, float groundY, float markerHeight)
        {
            var pos = new Vector3(state.posXZ.x, groundY + markerHeight / 2f, state.posXZ.y);
            // compass facing (0 = north = +Z) maps directly to Unity yaw
            var rot = Quaternion.Euler(0f, state.facingDeg, 0f);
            return (pos, rot);
        }

        // World-space terrain height at (x, z), offset by groundYBase (the
        // terrain object's Y, refreshed once per Update). Instance method so
        // groundYFunc can be built once in Start instead of allocating a
        // closure per unit per frame in Update.
        float GroundY(float x, float z) =>
            terrain.SampleHeight(new Vector3(x, 0f, z)) + groundYBase;

        void Start()
        {
            if (terrain == null)
            {
                // terrain re-imports replace the scene object and orphan the
                // serialized reference; fall back rather than NRE every frame
                terrain = Terrain.activeTerrain;
                Debug.LogWarning("BattleDirector.terrain was unset; using Terrain.activeTerrain");
            }
            if (soldierMaterial == null)
            {
                soldierMaterial = unitMaterial;
            }
            // asset reference keeps the shader in device builds; the
            // instancing flag itself is not stripped, so it's safe to set here
            soldierMaterial.enableInstancing = true;
            // unitMaterial also renders instanced at the regiment tier
            unitMaterial.enableInstancing = true;
            soldierMesh = InstancedMeshes.BuildSoldier();
            unitBoxMesh = InstancedMeshes.BuildUnitBox();
            lodCamera = Camera.main;
            // built once: a fresh (x, z) => ... lambda per unit per frame
            // would allocate a closure every Update
            groundYFunc = GroundY;

            BattleDto battle = BattleLoader.Parse(battleJson.text);
            clock.EndTime = battle.endTime;
            clock.StartTime = battle.startTime;
            foreach (UnitDto u in battle.units)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                marker.name = $"unit {u.id} ({u.name})";
                marker.localScale = new Vector3(u.frontage_m, MarkerHeight, u.depth_m); // y overwritten per-frame
                var renderer = marker.GetComponent<Renderer>();
                renderer.sharedMaterial = unitMaterial;
                var block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, SideColor(u.side));
                renderer.SetPropertyBlock(block);
                Object.Destroy(marker.GetComponent<Collider>()); // not clickable yet

                var formationRenderer = new UnitFormationRenderer(
                    u.id, u.frontage_m, u.depth_m, soldierMesh, soldierMaterial, SideColor(u.side));
                // clamp defensively: the schema doesn't cap roster length, and
                // the persistent matrix buffer must never grow at render time.
                // Loud, like unknown sides: authored data the renderer refuses
                // to fully show must never vanish silently.
                if (u.regiments != null && u.regiments.Count > MaxRegiments)
                    Debug.LogWarning(
                        $"unit '{u.id}' has {u.regiments.Count} regiments; rendering only the first {MaxRegiments}");
                int regimentCount = u.regiments == null
                    ? 0 : Mathf.Min(u.regiments.Count, MaxRegiments);
                units.Add(new UnitEntry
                {
                    Track = new UnitTrack(u),
                    Marker = marker,
                    FormationRenderer = formationRenderer,
                    Tier = LodTier.Block,
                    RegimentCount = regimentCount,
                    RegimentMatrices = regimentCount > 0 ? new Matrix4x4[MaxRegiments] : null,
                    ColorBlock = block,
                });
            }
        }

        void Update()
        {
            float baseY = terrain.transform.position.y;
            groundYBase = baseY;
            foreach (UnitEntry entry in units)
            {
                UnitTrack track = entry.Track;
                Transform marker = entry.Marker;
                UnitState s = track.StateAt(clock.CurrentTime);

                // one representative height sample (unit center) is enough to
                // judge camera distance; the block path does its own denser
                // footprint sampling only when it's actually the one rendering
                float centerY = baseY + terrain.SampleHeight(new Vector3(s.posXZ.x, 0f, s.posXZ.y));
                Vector3 worldPos = new Vector3(s.posXZ.x, centerY, s.posXZ.y);
                // no camera (editor edge case): treat everything as far so the
                // familiar block path keeps working
                float dist = lodCamera != null
                    ? Vector3.Distance(lodCamera.transform.position, worldPos)
                    : float.MaxValue;
                // three tiers with a sticky band at each boundary: a tier only
                // switches once the camera clears the far edge of its band
                if (dist < SoldiersInDist
                    || (entry.Tier == LodTier.Soldiers && dist < SoldiersOutDist))
                    entry.Tier = LodTier.Soldiers;
                else if (dist < RegimentsInDist
                    || (entry.Tier == LodTier.Regiments && dist < RegimentsOutDist))
                    entry.Tier = LodTier.Regiments;
                else
                    entry.Tier = LodTier.Block;

                if (entry.Tier == LodTier.Soldiers)
                {
                    marker.gameObject.SetActive(false);
                    entry.FormationRenderer.Render(s, groundYFunc);
                    continue;
                }

                // middle tier only for units with a roster holding an ordered
                // formation; scattered/routed (and roster-less units) fall
                // through to the monolithic block — honesty over uniformity
                if (entry.Tier == LodTier.Regiments && entry.RegimentCount > 0
                    && (s.formation == "line" || s.formation == "column"))
                {
                    marker.gameObject.SetActive(false);
                    RenderRegiments(entry, s, baseY);
                    continue;
                }

                marker.gameObject.SetActive(true);
                FootprintSamplePoints(s.posXZ, s.facingDeg,
                    track.Unit.frontage_m, track.Unit.depth_m, samplePoints);
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (Vector2 p in samplePoints)
                {
                    float y = terrain.SampleHeight(new Vector3(p.x, 0f, p.y));
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
                // a rigid slab can't lie on ~20 m of relief: stretch the block
                // from the lowest ground under it to MarkerHeight above the
                // highest, so it embeds in the slope instead of floating or
                // letting the crest poke through its top
                float blockHeight = (maxY - minY) + MarkerHeight;
                Vector3 scale = marker.localScale;
                scale.y = blockHeight;
                marker.localScale = scale;
                var (pos, rot) = MarkerPose(s, baseY + minY, blockHeight);
                marker.SetPositionAndRotation(pos, rot);
            }
        }

        // Middle tier: one RenderMeshInstanced of the unit box per unit, one
        // instance per roster regiment, laid out by RegimentSlots. Each
        // sub-block reuses the monolithic marker's footprint logic at its own
        // scale: sample the slot's footprint ring, stretch from the lowest
        // ground to MarkerHeight above the highest, so every sub-block embeds
        // in the slope independently. No per-frame allocations: slots and
        // matrices fill persistent buffers, RenderParams is a struct.
        void RenderRegiments(UnitEntry entry, UnitState s, float baseY)
        {
            int count = entry.RegimentCount;
            FormationLayout.RegimentSlots(s.formation, count,
                entry.Track.Unit.frontage_m, entry.Track.Unit.depth_m, slotsBuffer);
            var rot = Quaternion.Euler(0f, s.facingDeg, 0f);
            for (int i = 0; i < count; i++)
            {
                var (center, size) = slotsBuffer[i];
                // local (x=right-of-line, y=forward) -> world via facing
                Vector3 world = rot * new Vector3(center.x, 0f, center.y);
                float wx = s.posXZ.x + world.x;
                float wz = s.posXZ.y + world.z;
                FootprintSamplePoints(new Vector2(wx, wz), s.facingDeg, size.x, size.y,
                    samplePoints);
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (Vector2 p in samplePoints)
                {
                    float y = terrain.SampleHeight(new Vector3(p.x, 0f, p.y));
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
                // the unit box's base is at local y=0, so position = lowest
                // ground and scale.y = block height stands it on the terrain
                entry.RegimentMatrices[i] = Matrix4x4.TRS(
                    new Vector3(wx, baseY + minY, wz), rot,
                    new Vector3(size.x, (maxY - minY) + MarkerHeight, size.y));
            }
            var rp = new RenderParams(unitMaterial) { matProps = entry.ColorBlock };
            Graphics.RenderMeshInstanced(rp, unitBoxMesh, 0, entry.RegimentMatrices, count);
        }
    }
}
