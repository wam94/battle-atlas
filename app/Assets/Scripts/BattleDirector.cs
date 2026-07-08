using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    // middle tier. Decomposed brigades (units with a `parent`) form a
    // family: the tier is evaluated ONCE from the parent's center and the
    // whole family swaps atomically — parent block far, children (own
    // tracks) near — per battle-format.md "Parent / children".
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
        // when left unset in the inspector (e.g. older scenes). For the
        // two-tone uniforms, wire Assets/Battle/SoldierFigure.mat here — the
        // vertex-color bands need its SoldierVertexTint shader (URP Lit
        // ignores vertex color, so the fallback renders monochrome figures).
        public Material soldierMaterial;
        // material for the instanced flag layer (Assets/Battle/Flag.mat —
        // its shader carries the deterministic vertex wave). Unset: flags
        // are skipped with a warning, everything else renders as before.
        public Material flagMaterial;
        // transparent materials for the obscuration field (Assets/Battle/
        // Smoke.mat and Dust.mat, created + wired by the BattleAtlas/Setup
        // Obscuration menu item). Unset: ObscurationField warns once and
        // renders no obscuration, everything else renders as before.
        public Material smokeMaterial;
        public Material dustMaterial;

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
        // pose bias input (UnitFormationRenderer.PoseFor): a unit is
        // "moving" when its track position changes across a 1s window
        // around now — sampled symmetrically so scrub direction can't flip
        // the answer at the same t
        const float MovingSampleHalfWindow = 0.5f;
        const float MovingEpsilonM = 0.05f;
        // flag pivot above the unit-center ground: clears the 6m block
        // marker and the figures, so the flag reads at every tier
        const float FlagPoleHeight = 10f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // battle-clock seconds for the Flag shader's vertex wave — a global
        // driven from BattleClock, NOT _Time, so time-scrubbing replays the
        // identical wave (determinism even in the cloth)
        static readonly int BattleWaveTimeId = Shader.PropertyToID("_BattleWaveTime");
        // corners first (order is load-bearing for tests), then edge midpoints —
        // a denser ring catches ground rising inside the footprint, not just at
        // its extremes
        static readonly (float dx, float dz)[] CornerOffsets =
        {
            (-0.5f, -0.5f), (0.5f, -0.5f), (-0.5f, 0.5f), (0.5f, 0.5f),
            (0f, -0.5f), (0f, 0.5f), (-0.5f, 0f), (0.5f, 0f),
        };

        public enum LodTier { Soldiers, Regiments, Block }

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
            // family links, built once in Start from UnitDto.parent (the
            // loader already guaranteed validity and depth 1): a child holds
            // its parent, a parent holds its children, everyone else null
            public UnitEntry Parent;
            public List<UnitEntry> Children;
            // middle-tier state, null/0 when the unit has no regiments roster
            public int RegimentCount;
            public Matrix4x4[] RegimentMatrices;
            public MaterialPropertyBlock ColorBlock;
            public bool IsUnion; // flag color bucket
        }

        readonly List<UnitEntry> units = new();
        readonly Vector2[] samplePoints = new Vector2[9];
        // scratch for RegimentSlots, reused across units within one Update
        readonly (Vector2 center, Vector2 size)[] slotsBuffer =
            new (Vector2, Vector2)[MaxRegiments];
        Mesh[] soldierPoseMeshes;
        Mesh unitBoxMesh;
        Mesh flagMesh;
        // one flag per unit at every tier, ONE RenderMeshInstanced per side
        // across all units (side color rides the MPB, so union and
        // confederate flags are the two class-homogeneous batches);
        // preallocated at unit count in Start, refilled each Update
        Matrix4x4[] unionFlagMatrices;
        Matrix4x4[] csaFlagMatrices;
        int unionFlagCount;
        int csaFlagCount;
        MaterialPropertyBlock unionFlagBlock;
        MaterialPropertyBlock csaFlagBlock;
        bool warnedNoFlagMaterial;
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

        // The family suppression contract (battle-format.md "Parent /
        // children"): at Block tier the parent's block IS the family; at the
        // nearer tiers the children are, and the parent hides. Parentless,
        // childless units render at every tier — the pre-family behavior.
        // Pure static so the truth table is testable without a scene.
        public static bool RendersAtTier(bool isChild, bool hasChildren, LodTier tier)
        {
            if (hasChildren) return tier == LodTier.Block;
            if (isChild) return tier != LodTier.Block;
            return true;
        }

        // Three tiers with a sticky band at each boundary: a tier only
        // switches once the camera clears the far edge of its band. For a
        // family, dist is the PARENT's center distance and the latch lives on
        // the parent — evaluated once, so a family never half-swaps.
        public static LodTier EvaluateTier(float dist, LodTier current)
        {
            if (dist < SoldiersInDist
                || (current == LodTier.Soldiers && dist < SoldiersOutDist))
                return LodTier.Soldiers;
            if (dist < RegimentsInDist
                || (current == LodTier.Regiments && dist < RegimentsOutDist))
                return LodTier.Regiments;
            return LodTier.Block;
        }

        // Middle-tier sub-blocks only for units with a roster holding an
        // ordered formation; scattered/routed (and roster-less units) fall
        // through to the monolithic block — honesty over uniformity.
        public static bool RendersRegimentSubBlocks(
            int regimentCount, string formation, LodTier tier)
        {
            return tier == LodTier.Regiments && regimentCount > 0
                && (formation == "line" || formation == "column");
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
            if (flagMaterial != null) flagMaterial.enableInstancing = true;
            // pose meshes indexed by UnitFormationRenderer.Pose* — shared
            // across every unit's renderer
            soldierPoseMeshes = new Mesh[UnitFormationRenderer.PoseCount];
            soldierPoseMeshes[UnitFormationRenderer.PoseStanding] = InstancedMeshes.BuildSoldier();
            soldierPoseMeshes[UnitFormationRenderer.PoseAdvancing] = InstancedMeshes.BuildSoldierAdvancing();
            soldierPoseMeshes[UnitFormationRenderer.PoseKneeling] = InstancedMeshes.BuildSoldierKneeling();
            unitBoxMesh = InstancedMeshes.BuildUnitBox();
            flagMesh = InstancedMeshes.BuildFlag();
            unionFlagBlock = new MaterialPropertyBlock();
            unionFlagBlock.SetColor(BaseColorId, SideColor("union"));
            csaFlagBlock = new MaterialPropertyBlock();
            csaFlagBlock.SetColor(BaseColorId, SideColor("confederate"));
            lodCamera = Camera.main;
            // built once: a fresh (x, z) => ... lambda per unit per frame
            // would allocate a closure every Update
            groundYFunc = GroundY;

            BattleDto battle = BattleLoader.Parse(battleJson.text);
            clock.EndTime = battle.endTime;
            clock.StartTime = battle.startTime;
            var entriesById = new Dictionary<string, UnitEntry>(battle.units.Count);
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
                // not clickable yet. Destroy is illegal outside play mode —
                // the EditMode scale test (BattleLoaderScaleTests) drives
                // Start headlessly, so pick the mode-appropriate teardown.
                var collider = marker.GetComponent<Collider>();
                if (Application.isPlaying) Object.Destroy(collider);
                else Object.DestroyImmediate(collider);

                var formationRenderer = new UnitFormationRenderer(
                    u.id, u.frontage_m, u.depth_m, soldierPoseMeshes, soldierMaterial,
                    SideColor(u.side));
                // clamp defensively: the schema doesn't cap roster length, and
                // the persistent matrix buffer must never grow at render time.
                // Loud, like unknown sides: authored data the renderer refuses
                // to fully show must never vanish silently.
                if (u.regiments != null && u.regiments.Count > MaxRegiments)
                    Debug.LogWarning(
                        $"unit '{u.id}' has {u.regiments.Count} regiments; rendering only the first {MaxRegiments}");
                int regimentCount = u.regiments == null
                    ? 0 : Mathf.Min(u.regiments.Count, MaxRegiments);
                var entry = new UnitEntry
                {
                    Track = new UnitTrack(u),
                    Marker = marker,
                    FormationRenderer = formationRenderer,
                    Tier = LodTier.Block,
                    RegimentCount = regimentCount,
                    RegimentMatrices = regimentCount > 0 ? new Matrix4x4[MaxRegiments] : null,
                    ColorBlock = block,
                    IsUnion = u.side == "union",
                };
                units.Add(entry);
                entriesById[u.id] = entry;
            }
            // second pass: link families. Built once here so Update never
            // searches or allocates; Parse already threw on unknown parents,
            // grandparents, and roster-carrying parents.
            for (int i = 0; i < battle.units.Count; i++)
            {
                string parentId = battle.units[i].parent;
                if (string.IsNullOrEmpty(parentId))
                    continue;
                UnitEntry parent = entriesById[parentId];
                if (parent.Children == null)
                    parent.Children = new List<UnitEntry>();
                parent.Children.Add(units[i]);
                units[i].Parent = parent;
            }
            // one slot per unit — every unit flies a flag every frame
            unionFlagMatrices = new Matrix4x4[units.Count];
            csaFlagMatrices = new Matrix4x4[units.Count];

            // obscuration rides its own component but the SAME parsed battle:
            // authored events resolve emitter positions from each event's own
            // unit's track, and dust derives only from CHILDLESS units — a
            // decomposed brigade's parent track is the far-tier record of the
            // same men, so deriving from both would double-dust the family
            var tracksById = new Dictionary<string, UnitTrack>(units.Count);
            var childlessTracks = new List<UnitTrack>();
            // authored file order, not dictionary order: emitter order feeds
            // ObscurationField's deterministic overflow clamp
            foreach (UnitDto u in battle.units)
            {
                UnitEntry entry = entriesById[u.id];
                tracksById[u.id] = entry.Track;
                if (entry.Children == null)
                    childlessTracks.Add(entry.Track);
            }
            gameObject.AddComponent<ObscurationField>().Init(
                battle, tracksById, childlessTracks, clock, terrain,
                smokeMaterial, dustMaterial);
            // and the soundscape: same parsed battle, same attach-level rule
            // (emitters sound from their own track at t, whatever tier the
            // LOD ladder currently draws)
            gameObject.AddComponent<AcousticField>().Init(
                battle, tracksById, clock, terrain);
        }

        void Update()
        {
            float baseY = terrain.transform.position.y;
            groundYBase = baseY;
            // the Flag shader's wave runs on battle time, set once per frame
            Shader.SetGlobalFloat(BattleWaveTimeId, clock.CurrentTime);
            unionFlagCount = 0;
            csaFlagCount = 0;
            foreach (UnitEntry entry in units)
            {
                if (entry.Parent != null)
                    continue; // children render on their family's pass below
                UnitState s = entry.Track.StateAt(clock.CurrentTime);

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
                // the hysteresis latch lives on this entry — for a family,
                // that's the parent: one center, one tier, atomic swap
                entry.Tier = EvaluateTier(dist, entry.Tier);

                RenderMember(entry, s, entry.Tier, baseY);
                if (entry.Children == null)
                    continue;
                for (int i = 0; i < entry.Children.Count; i++)
                {
                    UnitEntry child = entry.Children[i];
                    RenderMember(child, child.Track.StateAt(clock.CurrentTime),
                        entry.Tier, baseY);
                }
            }
            RenderFlags();
        }

        // Two RenderMeshInstanced calls total — one per side across ALL
        // units (the class-homogeneous MPB pattern, like FenceField). The
        // flag material must be an ASSET (the magenta/stripping lesson);
        // when unset, say so once and keep everything else rendering.
        void RenderFlags()
        {
            if (flagMaterial == null)
            {
                if (!warnedNoFlagMaterial)
                {
                    Debug.LogWarning(
                        "BattleDirector.flagMaterial is unset; wire Assets/Battle/Flag.mat " +
                        "to fly unit flags");
                    warnedNoFlagMaterial = true;
                }
                return;
            }
            var rp = new RenderParams(flagMaterial)
            {
                // a waving flag's shadow would flicker across the whole
                // block at strategic zoom; flags never cast (the B8 audit)
                shadowCastingMode = ShadowCastingMode.Off,
            };
            if (unionFlagCount > 0)
            {
                rp.matProps = unionFlagBlock;
                Graphics.RenderMeshInstanced(
                    rp, flagMesh, 0, unionFlagMatrices, unionFlagCount);
            }
            if (csaFlagCount > 0)
            {
                rp.matProps = csaFlagBlock;
                Graphics.RenderMeshInstanced(
                    rp, flagMesh, 0, csaFlagMatrices, csaFlagCount);
            }
        }

        // Renders one unit at the given tier — or hides it when the family
        // contract says another tier's representation owns the family right
        // now. For parentless, childless units RendersAtTier is always true,
        // so this is exactly the pre-family tier dispatch.
        void RenderMember(UnitEntry entry, UnitState s, LodTier tier, float baseY)
        {
            if (!RendersAtTier(entry.Parent != null, entry.Children != null, tier))
            {
                entry.Marker.gameObject.SetActive(false);
                return;
            }

            // a rendering member flies its flag — brigade colors at the block
            // tier resolve into regiment flags exactly when the tracks do;
            // hidden family representations never double-fly
            float flagY = baseY + terrain.SampleHeight(new Vector3(s.posXZ.x, 0f, s.posXZ.y));
            var flagMatrix = Matrix4x4.TRS(
                new Vector3(s.posXZ.x, flagY + FlagPoleHeight, s.posXZ.y),
                Quaternion.Euler(0f, s.facingDeg, 0f), Vector3.one);
            if (entry.IsUnion) unionFlagMatrices[unionFlagCount++] = flagMatrix;
            else csaFlagMatrices[csaFlagCount++] = flagMatrix;

            if (tier == LodTier.Soldiers)
            {
                entry.Marker.gameObject.SetActive(false);
                // pose bias input: is the track position changing around
                // now? StateAt clamps at the track ends, so the window
                // degrades to one-sided there instead of misreading
                bool moving = Vector2.Distance(
                    entry.Track.StateAt(clock.CurrentTime - MovingSampleHalfWindow).posXZ,
                    entry.Track.StateAt(clock.CurrentTime + MovingSampleHalfWindow).posXZ)
                    > MovingEpsilonM;
                entry.FormationRenderer.Render(s, moving, groundYFunc);
                return;
            }

            if (RendersRegimentSubBlocks(entry.RegimentCount, s.formation, tier))
            {
                entry.Marker.gameObject.SetActive(false);
                RenderRegiments(entry, s, baseY);
                return;
            }

            RenderBlock(entry, s, baseY);
        }

        // Far tier (and the middle-tier fallback): the monolithic block,
        // stretched into the slope.
        void RenderBlock(UnitEntry entry, UnitState s, float baseY)
        {
            Transform marker = entry.Marker;
            marker.gameObject.SetActive(true);
            FootprintSamplePoints(s.posXZ, s.facingDeg,
                entry.Track.Unit.frontage_m, entry.Track.Unit.depth_m, samplePoints);
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
