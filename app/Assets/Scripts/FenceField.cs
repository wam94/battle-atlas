using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // Instanced fence posts from baked land-cover data (pipeline's
    // fences.json, {"posts":[{x,z,bearing_deg,cls}]} — see
    // pipeline/terrain_pipeline/landcover.py `fence_posts` and
    // docs/superpowers/plans/2026-07-01-landcover.md Task 6). Mirrors
    // VegetationField's structure: precompute per-post TRS matrices once in
    // Start(), group into spatial cells and batch to ≤1023 instances
    // (SpatialBatcher), draw with RenderMeshInstanced in Update(). Two
    // classes get their own mesh and tint, so posts are split into
    // class-homogeneous batches: rail_fence keeps the post+rails mesh,
    // stone_wall draws the low irregular block strip
    // (InstancedMeshes.BuildWallSegment) — a wall the viewer must read as
    // cover, not a squashed fence.
    public class FenceField : MonoBehaviour
    {
        public TextAsset fencesJson;
        public Material fenceMaterial;

        // Graphics.RenderMeshInstanced caps out at 1023 instances per call.
        const int MaxInstancesPerCall = 1023;

        // Conservative mesh extents around each post position for the
        // per-batch cull bounds: rails and wall blocks reach 3 m along the
        // post's bearing (InstancedMeshes.BuildFencePost / BuildWallSegment)
        // and the post is ~1.4 m tall.
        static readonly Vector3 BoundsMargin = new Vector3(3f, 2f, 3f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color RailColor = new Color(0.35f, 0.24f, 0.15f);   // matte timber brown
        static readonly Color StoneColor = new Color(0.45f, 0.45f, 0.42f);  // matte stone grey

        Mesh postMesh;
        Mesh wallMesh;
        MaterialPropertyBlock railBlock;
        MaterialPropertyBlock stoneBlock;

        // Per-class spatial-cell batches (SpatialBatcher): ≤1023 instances
        // each, all from one 512 m grid cell, with explicit cull bounds.
        InstanceBatch[] railBatches;
        InstanceBatch[] stoneBatches;

        void Start()
        {
            postMesh = InstancedMeshes.BuildFencePost();
            wallMesh = InstancedMeshes.BuildWallSegment();

            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogWarning("FenceField: no active terrain found; posts will sit at y=0");
            }
            float baseY = terrain != null ? terrain.transform.position.y : 0f;

            if (fenceMaterial != null)
            {
                // asset reference keeps the shader in device builds; the
                // instancing flag itself is not stripped, so it's safe here
                fenceMaterial.enableInstancing = true;
            }
            railBlock = new MaterialPropertyBlock();
            railBlock.SetColor(BaseColorId, RailColor);
            stoneBlock = new MaterialPropertyBlock();
            stoneBlock.SetColor(BaseColorId, StoneColor);

            railBatches = new InstanceBatch[0];
            stoneBatches = new InstanceBatch[0];

            if (fencesJson == null) return;

            FencesDto dto = LandcoverData.ParseFences(fencesJson.text);

            var rail = new List<Matrix4x4>();
            var stone = new List<Matrix4x4>();

            foreach (PostDto post in dto.posts)
            {
                bool isStone = post.cls == "stone_wall";
                float x = post.x;
                float z = post.z;
                float y = baseY + (terrain != null
                    ? terrain.SampleHeight(new Vector3(x, 0f, z))
                    : 0f);
                // both meshes are built at real-world size along local +Z
                // (the pipeline's 3.0 m post spacing), so no per-class scale
                Matrix4x4 m = Matrix4x4.TRS(
                    new Vector3(x, y, z),
                    Quaternion.Euler(0f, post.bearing_deg, 0f),
                    Vector3.one);
                (isStone ? stone : rail).Add(m);
            }

            // group each class into 512 m spatial cells so per-call culling
            // can drop off-screen cells (file-order batches span the map and
            // never cull — see SpatialBatcher)
            railBatches = SpatialBatcher.Build(
                rail, SpatialBatcher.CellSize, BoundsMargin, MaxInstancesPerCall);
            stoneBatches = SpatialBatcher.Build(
                stone, SpatialBatcher.CellSize, BoundsMargin, MaxInstancesPerCall);
        }

        void Update()
        {
            if (fenceMaterial == null || postMesh == null) return;

            // shadows are off project-wide today, but RenderParams defaults
            // shadowCastingMode to On — left implicit, enabling main-light
            // shadows later would silently re-render every post into each
            // cascade, so keep it explicitly Off
            var railRp = new RenderParams(fenceMaterial)
            {
                matProps = railBlock,
                shadowCastingMode = ShadowCastingMode.Off,
            };
            DrawBatches(railRp, postMesh, railBatches);

            var stoneRp = new RenderParams(fenceMaterial)
            {
                matProps = stoneBlock,
                shadowCastingMode = ShadowCastingMode.Off,
            };
            DrawBatches(stoneRp, wallMesh, stoneBatches);
        }

        void DrawBatches(RenderParams rp, Mesh mesh, InstanceBatch[] batches)
        {
            if (batches == null) return;
            for (int b = 0; b < batches.Length; b++)
            {
                InstanceBatch batch = batches[b];
                // documented, not just asserted: SpatialBatcher splits cells
                // into ≤MaxInstancesPerCall chunks, so every batch here fits
                Debug.Assert(batch.matrices.Length <= MaxInstancesPerCall,
                    $"fence batch {b} exceeds RenderMeshInstanced's {MaxInstancesPerCall}-instance cap");
                // per-call culling: the batch's own cell bounds, not Unity's
                // merged default, so off-screen cells skip vertex work
                rp.worldBounds = batch.bounds;
                Graphics.RenderMeshInstanced(rp, mesh, 0, batch.matrices, batch.matrices.Length);
            }
        }
    }
}
