using System;
using UnityEngine;

namespace BattleAtlas
{
    // Draws one unit as instanced soldier figures arranged by formation
    // state, split into per-pose matrix buckets (standing / advancing-lean /
    // kneeling-firing): RenderMeshInstanced draws one mesh per call, so pose
    // variety costs ≤PoseCount calls per unit instead of 1 — nothing on
    // Metal. Per-unit color via a MaterialPropertyBlock. Created and driven
    // by BattleDirector.
    public class UnitFormationRenderer
    {
        public const int PoseStanding = 0;
        public const int PoseAdvancing = 1;
        public const int PoseKneeling = 2;
        public const int PoseCount = 3;

        readonly string unitId;
        readonly float frontage;
        readonly float depth;
        readonly Mesh[] poseMeshes;
        readonly Material material;
        readonly MaterialPropertyBlock block;
        // buckets preallocated at MaxFigures once (house rule: no growth at
        // render time); a figure lands in exactly one bucket per frame
        readonly Matrix4x4[][] poseBuckets;
        readonly int[] poseCounts = new int[PoseCount];
        readonly Vector2[] offsetsBuffer = new Vector2[FormationLayout.MaxFigures];
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public UnitFormationRenderer(
            string unitId, float frontage, float depth, Mesh[] poseMeshes,
            Material material, Color color)
        {
            this.unitId = unitId;
            this.frontage = frontage;
            this.depth = depth;
            this.poseMeshes = poseMeshes;
            this.material = material;
            block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color);
            poseBuckets = new Matrix4x4[PoseCount][];
            for (int p = 0; p < PoseCount; p++)
                poseBuckets[p] = new Matrix4x4[FormationLayout.MaxFigures];
        }

        // The pose-bias rule — pose is information, not garnish. The Jitter
        // hash picks within a distribution the unit's STATE selects:
        // - moving units (and routed ones, who are running) read as
        //   advancing: 80% lean / 20% standing, so a moving line ripples
        //   instead of goose-stepping;
        // - a stationary skirmish line is the one engaged/firing posture
        //   derivable from formation + movement alone (skirmishers deploy
        //   to fight): 70% kneeling-firing / 30% standing;
        // - everything else stands at shoulder arms — a posted line must
        //   not invent an engagement the data doesn't attest.
        // Deterministic per (unitId, figure): scrubbing replays the frame.
        public static int PoseFor(string unitId, int index, bool moving, string formation)
        {
            float t = (FormationLayout.Jitter(unitId, index, 61) + 1f) * 0.5f; // [0,1)
            if (moving || formation == "routed")
                return t < 0.8f ? PoseAdvancing : PoseStanding;
            if (formation == "skirmish")
                return t < 0.7f ? PoseKneeling : PoseStanding;
            return PoseStanding;
        }

        // Pure: fills matrices, returns count. groundY samples world height at (x, z).
        // offsetsBuffer is scratch space the caller owns (sized MaxFigures);
        // reused across calls to avoid a per-frame allocation.
        public static int BuildMatrices(
            string unitId, UnitState state, float frontage, float depth,
            Func<float, float, float> groundY, Matrix4x4[] matrices, Vector2[] offsetsBuffer)
        {
            int count = FormationLayout.FigureCount(state.strength);
            FormationLayout.Offsets(unitId, state.formation, count, frontage, depth, offsetsBuffer);
            var rot = Quaternion.Euler(0f, state.facingDeg, 0f);
            for (int i = 0; i < count; i++)
            {
                // local (x=right-of-line, y=forward) -> world via facing
                Vector3 world = rot * new Vector3(offsetsBuffer[i].x, 0f, offsetsBuffer[i].y);
                float wx = state.posXZ.x + world.x;
                float wz = state.posXZ.y + world.z;
                matrices[i] = Matrix4x4.TRS(
                    new Vector3(wx, groundY(wx, wz), wz), rot, Vector3.one);
            }
            return count;
        }

        // Compatibility overload matching the pre-buffer-reuse signature:
        // allocates its own scratch offsets buffer internally.
        public static int BuildMatrices(
            string unitId, UnitState state, float frontage, float depth,
            Func<float, float, float> groundY, Matrix4x4[] matrices)
        {
            return BuildMatrices(
                unitId, state, frontage, depth, groundY, matrices,
                new Vector2[FormationLayout.MaxFigures]);
        }

        // Pure pose-bucket variant of BuildMatrices: same placement math,
        // but each figure's matrix lands in poseBuckets[PoseFor(...)].
        // Returns the total figure count; poseCounts holds the per-bucket
        // split (they always sum to the return value). Buckets and counts
        // are caller-owned scratch, reused across frames.
        public static int BuildPoseMatrices(
            string unitId, UnitState state, float frontage, float depth, bool moving,
            Func<float, float, float> groundY,
            Matrix4x4[][] poseBuckets, int[] poseCounts, Vector2[] offsetsBuffer)
        {
            int count = FormationLayout.FigureCount(state.strength);
            FormationLayout.Offsets(unitId, state.formation, count, frontage, depth, offsetsBuffer);
            for (int p = 0; p < PoseCount; p++) poseCounts[p] = 0;
            var rot = Quaternion.Euler(0f, state.facingDeg, 0f);
            for (int i = 0; i < count; i++)
            {
                // local (x=right-of-line, y=forward) -> world via facing
                Vector3 world = rot * new Vector3(offsetsBuffer[i].x, 0f, offsetsBuffer[i].y);
                float wx = state.posXZ.x + world.x;
                float wz = state.posXZ.y + world.z;
                int pose = PoseFor(unitId, i, moving, state.formation);
                poseBuckets[pose][poseCounts[pose]++] = Matrix4x4.TRS(
                    new Vector3(wx, groundY(wx, wz), wz), rot, Vector3.one);
            }
            return count;
        }

        // moving: whether the unit's track position is changing around the
        // current time (BattleDirector samples the track either side of
        // now) — it biases the pose split, see PoseFor.
        public void Render(UnitState state, bool moving, Func<float, float, float> groundY)
        {
            int total = BuildPoseMatrices(
                unitId, state, frontage, depth, moving, groundY,
                poseBuckets, poseCounts, offsetsBuffer);
            if (total == 0) return;
            // shadowCastingMode deliberately left at the On default: soldiers
            // are the ONE intended shadow caster when B8 enables shadows —
            // every other instance field in the project sets Off explicitly.
            var rp = new RenderParams(material) { matProps = block };
            for (int p = 0; p < PoseCount; p++)
            {
                if (poseCounts[p] == 0) continue;
                Graphics.RenderMeshInstanced(
                    rp, poseMeshes[p], 0, poseBuckets[p], poseCounts[p]);
            }
        }
    }
}
