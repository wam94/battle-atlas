using System;
using UnityEngine;

namespace BattleAtlas
{
    // Draws one unit as instanced soldier figures arranged by formation state.
    // One RenderMeshInstanced call per unit per frame; per-unit color via a
    // MaterialPropertyBlock. Created and driven by BattleDirector.
    public class UnitFormationRenderer
    {
        readonly string unitId;
        readonly float frontage;
        readonly float depth;
        readonly Mesh mesh;
        readonly Material material;
        readonly MaterialPropertyBlock block;
        readonly Matrix4x4[] matrices = new Matrix4x4[FormationLayout.MaxFigures];
        readonly Vector2[] offsetsBuffer = new Vector2[FormationLayout.MaxFigures];
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public UnitFormationRenderer(
            string unitId, float frontage, float depth, Mesh mesh, Material material, Color color)
        {
            this.unitId = unitId;
            this.frontage = frontage;
            this.depth = depth;
            this.mesh = mesh;
            this.material = material;
            block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color);
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

        public void Render(UnitState state, Func<float, float, float> groundY)
        {
            int count = BuildMatrices(
                unitId, state, frontage, depth, groundY, matrices, offsetsBuffer);
            if (count == 0) return;
            var rp = new RenderParams(material) { matProps = block };
            Graphics.RenderMeshInstanced(rp, mesh, 0, matrices, count);
        }
    }
}
