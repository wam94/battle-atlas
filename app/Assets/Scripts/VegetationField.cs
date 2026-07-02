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
    //
    // Two-tier LOD per SPATIAL CELL, not per tree (research doc
    // 2026-07-02-descriptive-graphics-techniques.md §2b): each SpatialBatcher
    // batch latches near/far from its cell-center camera distance with a
    // hysteresis band exactly like BattleDirector's LOD latch — a pop, never
    // a crossfade (LOD crossfade is a mobile anti-pattern, trap #6). The
    // matrices never change; only which mesh the cell draws. Per-instance
    // hue variation rides 4 pre-tinted MaterialPropertyBlock buckets,
    // hash-assigned — zero shader work.
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
        // per-batch cull bounds: the near tree mesh tops out at ~8.4 m with
        // a ~6.8 m-wide canopy (InstancedMeshes.BuildTreeNear), so 4 m
        // sideways and 9 m vertically covers either tier's mesh hanging off
        // its pivot.
        static readonly Vector3 BoundsMargin = new Vector3(4f, 9f, 4f);

        // Orchard trees render smaller than woodlot trees (planted stock vs.
        // mature woods) — a uniform scale-down of the same tree mesh.
        const float OrchardScale = 0.7f;

        // Cell-tier hysteresis: a cell resolves to the near mesh below
        // NearInDist and holds it until NearOutDist (the same sticky-band
        // idiom as BattleDirector's SoldiersInDist/SoldiersOutDist).
        const float NearInDist = 800f;
        const float NearOutDist = 1200f;

        // Woodlot trees hash into HueBucketCount pre-tinted buckets;
        // orchard-class trees get their own bucket (distinct near mesh,
        // brighter green, uniform size — planted stock reads through
        // regularity, not variation).
        public const int HueBucketCount = 4;
        public const int OrchardBucket = HueBucketCount;
        const int GroupCount = HueBucketCount + 1;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // hue variation around the old uniform FoliageColor (0.28, 0.42,
        // 0.24): small shifts read as a mixed stand, not a checkerboard
        static readonly Color[] WoodlotTints =
        {
            new Color(0.28f, 0.42f, 0.24f),
            new Color(0.25f, 0.38f, 0.20f),
            new Color(0.31f, 0.44f, 0.22f),
            new Color(0.27f, 0.40f, 0.28f),
        };
        static readonly Color OrchardColor = new Color(0.38f, 0.52f, 0.26f);
        // the understory band (BuildTreeNear submesh 1) draws darker than
        // any canopy tint — the shaded-interior concealment cue
        static readonly Color UnderstoryColor = new Color(0.16f, 0.24f, 0.14f);

        Mesh farMesh;         // box tree, both classes (InstancedMeshes.BuildTree)
        Mesh nearMesh;        // trunk + canopy blobs + understory submesh
        Mesh orchardNearMesh; // rounded lollipop
        MaterialPropertyBlock[] groupBlocks;
        MaterialPropertyBlock understoryBlock;
        Camera lodCamera;

        // Spatial-cell batches (SpatialBatcher) per bucket group: ≤1023
        // instances each, all from one 512 m grid cell, with explicit
        // per-batch cull bounds. nearLatch is the per-batch tier latch,
        // parallel to groupBatches — preallocated in Start, so Update stays
        // allocation-free.
        InstanceBatch[][] groupBatches;
        bool[][] nearLatch;

        // Pure tier latch (mirrors BattleDirector's hysteresis): near when
        // inside NearInDist, and an already-near cell stays near until the
        // camera clears NearOutDist.
        public static bool NearTier(bool wasNear, float distance) =>
            distance < NearInDist || (wasNear && distance < NearOutDist);

        // Deterministic bucket assignment: orchard-class trees always land
        // in OrchardBucket (their own mesh + tint); woodlot trees hash into
        // [0, HueBucketCount) via the shared FNV jitter.
        public static int BucketFor(string cls, int index)
        {
            if (cls == "orchard") return OrchardBucket;
            float t = (FormationLayout.Jitter("tree-hue", index, 53) + 1f) * 0.5f; // [0,1)
            return Mathf.Min(HueBucketCount - 1, (int)(t * HueBucketCount));
        }

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
            farMesh = InstancedMeshes.BuildTree();
            nearMesh = InstancedMeshes.BuildTreeNear();
            orchardNearMesh = InstancedMeshes.BuildOrchardTreeNear();
            lodCamera = Camera.main;

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
            groupBlocks = new MaterialPropertyBlock[GroupCount];
            for (int g = 0; g < HueBucketCount; g++)
            {
                groupBlocks[g] = new MaterialPropertyBlock();
                groupBlocks[g].SetColor(BaseColorId, WoodlotTints[g]);
            }
            groupBlocks[OrchardBucket] = new MaterialPropertyBlock();
            groupBlocks[OrchardBucket].SetColor(BaseColorId, OrchardColor);
            understoryBlock = new MaterialPropertyBlock();
            understoryBlock.SetColor(BaseColorId, UnderstoryColor);

            // trees don't move: precompute placement matrices once here
            // rather than every frame, then group them into 512 m spatial
            // cells so RenderMeshInstanced's per-call culling can actually
            // drop off-screen cells (file-order batches span the map and
            // never cull — see SpatialBatcher)
            List<Matrix4x4>[] groups = treesJson != null
                ? BuildTreeMatrices(treesJson.text, terrain, baseY)
                : BuildGroveMatrices(terrain, baseY);
            groupBatches = new InstanceBatch[GroupCount][];
            nearLatch = new bool[GroupCount][];
            for (int g = 0; g < GroupCount; g++)
            {
                groupBatches[g] = SpatialBatcher.Build(
                    groups[g], SpatialBatcher.CellSize, BoundsMargin, MaxInstancesPerCall);
                nearLatch[g] = new bool[groupBatches[g].Length];
            }
        }

        // Per-tree TRS matrices from baked trees.json, split into bucket
        // groups (BucketFor). Orchard trees render at OrchardScale; woodlot
        // trees carry a small deterministic scale jitter (a mixed stand);
        // every tree is height-sampled onto the terrain exactly as the
        // legacy grove path does below.
        static List<Matrix4x4>[] BuildTreeMatrices(string json, Terrain terrain, float baseY)
        {
            TreesDto dto = LandcoverData.ParseTrees(json);
            var groups = NewGroups(dto.trees.Count / GroupCount + 1);

            for (int i = 0; i < dto.trees.Count; i++)
            {
                TreeDto tree = dto.trees[i];
                float x = tree.x;
                float z = tree.z;
                float y = baseY + (terrain != null
                    ? terrain.SampleHeight(new Vector3(x, 0f, z))
                    : 0f);
                bool orchard = tree.cls == "orchard";
                float scale = orchard
                    ? OrchardScale
                    : 1f + 0.15f * FormationLayout.Jitter("tree-scale", i, 41);
                groups[BucketFor(tree.cls, i)].Add(Matrix4x4.TRS(
                    new Vector3(x, y, z), Quaternion.identity, Vector3.one * scale));
            }
            return groups;
        }

        // Per-tree TRS matrices for the hardcoded legacy Groves fallback —
        // all woodlot-class, so they hash across the hue buckets too.
        static List<Matrix4x4>[] BuildGroveMatrices(Terrain terrain, float baseY)
        {
            var groups = NewGroups(128);
            int index = 0;
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
                    float scale = 1f + 0.15f * FormationLayout.Jitter("tree-scale", index, 41);
                    groups[BucketFor("woodlot", index)].Add(Matrix4x4.TRS(
                        new Vector3(x, y, z), Quaternion.identity, Vector3.one * scale));
                    index++;
                }
            }
            return groups;
        }

        static List<Matrix4x4>[] NewGroups(int capacity)
        {
            var groups = new List<Matrix4x4>[GroupCount];
            for (int g = 0; g < GroupCount; g++) groups[g] = new List<Matrix4x4>(capacity);
            return groups;
        }

        void Update()
        {
            if (treeMaterial == null || groupBatches == null) return;
            // no camera (editor edge case): everything stays far, matching
            // BattleDirector's treat-as-far fallback
            bool hasCamera = lodCamera != null;
            Vector3 camPos = hasCamera ? lodCamera.transform.position : Vector3.zero;

            // shadows are off project-wide today, but RenderParams defaults
            // shadowCastingMode to On — left implicit, enabling main-light
            // shadows later would silently re-render every tree into each
            // cascade, so keep it explicitly Off
            var rp = new RenderParams(treeMaterial)
            {
                shadowCastingMode = ShadowCastingMode.Off,
            };
            var understoryRp = new RenderParams(treeMaterial)
            {
                matProps = understoryBlock,
                shadowCastingMode = ShadowCastingMode.Off,
            };
            for (int g = 0; g < GroupCount; g++)
            {
                rp.matProps = groupBlocks[g];
                InstanceBatch[] batches = groupBatches[g];
                bool[] latch = nearLatch[g];
                for (int b = 0; b < batches.Length; b++)
                {
                    InstanceBatch batch = batches[b];
                    // documented, not just asserted: SpatialBatcher splits cells
                    // into ≤MaxInstancesPerCall chunks, so every batch here fits
                    Debug.Assert(batch.matrices.Length <= MaxInstancesPerCall,
                        $"batch {b} exceeds RenderMeshInstanced's {MaxInstancesPerCall}-instance cap");
                    // tier per cell from the cell-center distance, latched
                    // with hysteresis: same matrices, two meshes, zero
                    // per-frame allocation
                    bool near = hasCamera && NearTier(
                        latch[b], Vector3.Distance(camPos, batch.bounds.center));
                    latch[b] = near;
                    Mesh mesh = near
                        ? (g == OrchardBucket ? orchardNearMesh : nearMesh)
                        : farMesh;
                    // per-call culling: the batch's own cell bounds, not Unity's
                    // merged default, so off-screen cells skip vertex work
                    rp.worldBounds = batch.bounds;
                    Graphics.RenderMeshInstanced(rp, mesh, 0, batch.matrices, batch.matrices.Length);
                    // near woodlot cells draw the understory band (submesh 1)
                    // darker, with the same matrices — one extra call per
                    // near cell, nothing beyond the ~1200 m ring
                    if (near && g != OrchardBucket)
                    {
                        understoryRp.worldBounds = batch.bounds;
                        Graphics.RenderMeshInstanced(
                            understoryRp, mesh, 1, batch.matrices, batch.matrices.Length);
                    }
                }
            }
        }
    }
}
