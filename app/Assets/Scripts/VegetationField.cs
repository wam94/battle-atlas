using UnityEngine;

namespace BattleAtlas
{
    // Instanced woods at three landmark anchors. Coordinates and radii come
    // from docs/research/2026-06-13-landmark-anchors.md — this is indicative
    // vegetation for scale and silhouette, NOT surveyed 1863 tree-line
    // extents (that's the future land-cover pipeline phase).
    public class VegetationField : MonoBehaviour
    {
        public Material treeMaterial;

        // Grove: (id, center in world XZ, radius in meters, tree count).
        // Copse of Trees (High Water Mark), Spangler's Woods (Pickett's
        // Division formation area), Ziegler's Grove (Woodruff's battery /
        // Hays's right) — see docs/research/2026-06-13-landmark-anchors.md.
        static readonly (string id, Vector2 center, float radius, int count)[] Groves =
        {
            ("copse",    new Vector2(4407.3f, 4801.1f), 40f,  40),
            ("spangler", new Vector2(3118.2f, 4766.7f), 220f, 350),
            ("ziegler",  new Vector2(4583.7f, 5100.6f), 120f, 120),
        };

        // Graphics.RenderMeshInstanced caps out at 1023 instances per call.
        // The largest grove here (Spangler's Woods, 350 trees) is well under
        // that, so one call per grove is safe without batching.
        const int MaxInstancesPerCall = 1023;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color FoliageColor = new Color(0.28f, 0.42f, 0.24f);

        Mesh treeMesh;
        MaterialPropertyBlock block;
        Matrix4x4[][] groveMatrices;

        // Even-density placement of `count` points inside a disc of `radius`
        // around `center`, deterministic per (id, index) via the same
        // FNV-style hash-jitter FormationLayout uses — sqrt of the radius
        // fraction keeps density uniform across the disc instead of bunching
        // near the center.
        public static Vector2[] Placements(string id, Vector2 center, float radius, int count)
        {
            var points = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float t = (FormationLayout.Jitter(id, i, 31) + 1f) * 0.5f;      // [0,1)
                float angleT = (FormationLayout.Jitter(id, i, 37) + 1f) * 0.5f; // [0,1)
                float r = Mathf.Sqrt(t) * radius;
                float angle = angleT * Mathf.PI * 2f;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            }
            return points;
        }

        void Start()
        {
            treeMesh = InstancedMeshes.BuildTree();

            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogWarning("VegetationField: no active terrain found; trees will sit at y=0");
            }
            float baseY = terrain != null ? terrain.transform.position.y : 0f;

            if (treeMaterial != null)
            {
                // asset reference keeps the shader in device builds; the
                // instancing flag itself is not stripped, so it's safe here
                treeMaterial.enableInstancing = true;
            }
            block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, FoliageColor);

            // trees don't move: precompute each grove's placement matrices
            // once here rather than every frame
            groveMatrices = new Matrix4x4[Groves.Length][];
            for (int g = 0; g < Groves.Length; g++)
            {
                var (id, center, radius, count) = Groves[g];
                Vector2[] placements = Placements(id, center, radius, count);
                var matrices = new Matrix4x4[count];
                for (int i = 0; i < count; i++)
                {
                    float x = placements[i].x;
                    float z = placements[i].y;
                    float y = baseY + (terrain != null
                        ? terrain.SampleHeight(new Vector3(x, 0f, z))
                        : 0f);
                    matrices[i] = Matrix4x4.TRS(
                        new Vector3(x, y, z), Quaternion.identity, Vector3.one);
                }
                groveMatrices[g] = matrices;
            }
        }

        void Update()
        {
            if (treeMaterial == null || treeMesh == null || groveMatrices == null) return;
            var rp = new RenderParams(treeMaterial) { matProps = block };
            for (int g = 0; g < groveMatrices.Length; g++)
            {
                Matrix4x4[] matrices = groveMatrices[g];
                // documented, not just asserted: every grove here is well
                // under the per-call instance cap, so one call each suffices
                Debug.Assert(matrices.Length <= MaxInstancesPerCall,
                    $"grove '{Groves[g].id}' exceeds RenderMeshInstanced's {MaxInstancesPerCall}-instance cap");
                Graphics.RenderMeshInstanced(rp, treeMesh, 0, matrices, matrices.Length);
            }
        }
    }
}
