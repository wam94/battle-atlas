using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // Instanced woods. Preferred source: baked trees.json from the land-cover
    // pipeline (docs/superpowers/plans/2026-07-01-landcover.md Task 6),
    // wired via `treesJson`. When `treesJson` is unset, falls back to three
    // hardcoded landmark groves — legacy-indicative vegetation for scale and
    // silhouette, NOT surveyed 1863 tree-line extents; coordinates and radii
    // come from docs/research/2026-06-13-landmark-anchors.md.
    public class VegetationField : MonoBehaviour
    {
        public Material treeMaterial;

        // Optional: baked land-cover tree placements (pipeline's trees.json,
        // {"trees":[{x,z,cls}]}). When set, Start() builds batches from this
        // instead of the hardcoded Groves fallback below.
        public TextAsset treesJson;

        // Legacy fallback grove: (id, center in world XZ, radius in meters,
        // tree count). Copse of Trees (High Water Mark), Spangler's Woods
        // (Pickett's Division formation area), Ziegler's Grove (Woodruff's
        // battery / Hays's right) — see
        // docs/research/2026-06-13-landmark-anchors.md. Used only when
        // treesJson is unset.
        static readonly (string id, Vector2 center, float radius, int count)[] Groves =
        {
            ("copse",    new Vector2(4407.3f, 4801.1f), 40f,  40),
            ("spangler", new Vector2(3118.2f, 4766.7f), 220f, 350),
            ("ziegler",  new Vector2(4583.7f, 5100.6f), 120f, 120),
        };

        // Graphics.RenderMeshInstanced caps out at 1023 instances per call.
        const int MaxInstancesPerCall = 1023;

        // Conservative mesh extents around each instance position for the
        // per-batch cull bounds: the tree mesh tops out at ~9 m with a
        // ~4.5 m-wide canopy (InstancedMeshes.BuildTree), so 3 m sideways
        // and 9 m vertically covers any tree hanging off its pivot.
        static readonly Vector3 BoundsMargin = new Vector3(3f, 9f, 3f);

        // Orchard trees render smaller than woodlot trees (planted stock vs.
        // mature woods) — a uniform scale-down of the same tree mesh.
        const float OrchardScale = 0.7f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color FoliageColor = new Color(0.28f, 0.42f, 0.24f);

        Mesh treeMesh;
        MaterialPropertyBlock block;

        // Spatial-cell batches (SpatialBatcher): ≤1023 instances each, all
        // from one 512 m grid cell, with explicit per-batch cull bounds.
        InstanceBatch[] batches;

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

            // trees don't move: precompute placement matrices once here
            // rather than every frame, then group them into 512 m spatial
            // cells so RenderMeshInstanced's per-call culling can actually
            // drop off-screen cells (file-order batches span the map and
            // never cull — see SpatialBatcher)
            List<Matrix4x4> matrices = treesJson != null
                ? BuildTreeMatrices(treesJson.text, terrain, baseY)
                : BuildGroveMatrices(terrain, baseY);
            batches = SpatialBatcher.Build(
                matrices, SpatialBatcher.CellSize, BoundsMargin, MaxInstancesPerCall);
        }

        // Per-tree TRS matrices from baked trees.json, in file order. Orchard
        // trees render at OrchardScale; every tree is height-sampled onto the
        // terrain exactly as the legacy grove path does below.
        static List<Matrix4x4> BuildTreeMatrices(string json, Terrain terrain, float baseY)
        {
            TreesDto dto = LandcoverData.ParseTrees(json);
            var matrices = new List<Matrix4x4>(dto.trees.Count);

            foreach (TreeDto tree in dto.trees)
            {
                float x = tree.x;
                float z = tree.z;
                float y = baseY + (terrain != null
                    ? terrain.SampleHeight(new Vector3(x, 0f, z))
                    : 0f);
                float scale = tree.cls == "orchard" ? OrchardScale : 1f;
                matrices.Add(Matrix4x4.TRS(
                    new Vector3(x, y, z), Quaternion.identity, Vector3.one * scale));
            }
            return matrices;
        }

        // Per-tree TRS matrices for the hardcoded legacy Groves fallback.
        static List<Matrix4x4> BuildGroveMatrices(Terrain terrain, float baseY)
        {
            var matrices = new List<Matrix4x4>();
            foreach (var (id, center, radius, count) in Groves)
            {
                Vector2[] placements = Placements(id, center, radius, count);
                for (int i = 0; i < count; i++)
                {
                    float x = placements[i].x;
                    float z = placements[i].y;
                    float y = baseY + (terrain != null
                        ? terrain.SampleHeight(new Vector3(x, 0f, z))
                        : 0f);
                    matrices.Add(Matrix4x4.TRS(
                        new Vector3(x, y, z), Quaternion.identity, Vector3.one));
                }
            }
            return matrices;
        }

        void Update()
        {
            if (treeMaterial == null || treeMesh == null || batches == null) return;
            // shadows are off project-wide today, but RenderParams defaults
            // shadowCastingMode to On — left implicit, enabling main-light
            // shadows later would silently re-render every tree into each
            // cascade, so keep it explicitly Off
            var rp = new RenderParams(treeMaterial)
            {
                matProps = block,
                shadowCastingMode = ShadowCastingMode.Off,
            };
            for (int b = 0; b < batches.Length; b++)
            {
                InstanceBatch batch = batches[b];
                // documented, not just asserted: SpatialBatcher splits cells
                // into ≤MaxInstancesPerCall chunks, so every batch here fits
                Debug.Assert(batch.matrices.Length <= MaxInstancesPerCall,
                    $"batch {b} exceeds RenderMeshInstanced's {MaxInstancesPerCall}-instance cap");
                // per-call culling: the batch's own cell bounds, not Unity's
                // merged default, so off-screen cells skip vertex work
                rp.worldBounds = batch.bounds;
                Graphics.RenderMeshInstanced(rp, treeMesh, 0, batch.matrices, batch.matrices.Length);
            }
        }
    }
}
