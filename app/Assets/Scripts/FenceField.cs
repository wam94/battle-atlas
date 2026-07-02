using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Instanced fence posts from baked land-cover data (pipeline's
    // fences.json, {"posts":[{x,z,bearing_deg,cls}]} — see
    // pipeline/terrain_pipeline/landcover.py `fence_posts` and
    // docs/superpowers/plans/2026-07-01-landcover.md Task 6). Mirrors
    // VegetationField's structure: precompute per-post TRS matrices once in
    // Start(), batch to ≤1023 instances, draw with RenderMeshInstanced in
    // Update(). Two classes (stone_wall / rail_fence) get their own scale
    // and tint, so posts are split into class-homogeneous batches.
    public class FenceField : MonoBehaviour
    {
        public TextAsset fencesJson;
        public Material fenceMaterial;

        // Graphics.RenderMeshInstanced caps out at 1023 instances per call.
        const int MaxInstancesPerCall = 1023;

        // rail_fence posts render at the mesh's native scale (post + two
        // horizontal rails, see InstancedMeshes.BuildFencePost). stone_wall
        // reuses the same mesh but squashed/widened into a low grey block
        // that reads as a stone wall course rather than a rail fence.
        static readonly Vector3 RailScale = Vector3.one;
        static readonly Vector3 StoneScale = new Vector3(2.2f, 0.6f, 1f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color RailColor = new Color(0.35f, 0.24f, 0.15f);   // matte timber brown
        static readonly Color StoneColor = new Color(0.45f, 0.45f, 0.42f);  // matte stone grey

        Mesh postMesh;
        MaterialPropertyBlock railBlock;
        MaterialPropertyBlock stoneBlock;

        // Per-class batches of ≤1023-instance matrix arrays.
        Matrix4x4[][] railBatches;
        Matrix4x4[][] stoneBatches;

        void Start()
        {
            postMesh = InstancedMeshes.BuildFencePost();

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

            railBatches = new Matrix4x4[0][];
            stoneBatches = new Matrix4x4[0][];

            if (fencesJson == null) return;

            FencesDto dto = LandcoverData.ParseFences(fencesJson.text);

            var rail = new List<Matrix4x4>();
            var stone = new List<Matrix4x4>();
            var railList = new List<Matrix4x4[]>();
            var stoneList = new List<Matrix4x4[]>();

            foreach (PostDto post in dto.posts)
            {
                bool isStone = post.cls == "stone_wall";
                float x = post.x;
                float z = post.z;
                float y = baseY + (terrain != null
                    ? terrain.SampleHeight(new Vector3(x, 0f, z))
                    : 0f);
                Matrix4x4 m = Matrix4x4.TRS(
                    new Vector3(x, y, z),
                    Quaternion.Euler(0f, post.bearing_deg, 0f),
                    isStone ? StoneScale : RailScale);

                List<Matrix4x4> bucket = isStone ? stone : rail;
                List<Matrix4x4[]> bucketBatches = isStone ? stoneList : railList;
                bucket.Add(m);
                if (bucket.Count == MaxInstancesPerCall)
                {
                    bucketBatches.Add(bucket.ToArray());
                    bucket.Clear();
                }
            }
            if (rail.Count > 0) railList.Add(rail.ToArray());
            if (stone.Count > 0) stoneList.Add(stone.ToArray());

            railBatches = railList.ToArray();
            stoneBatches = stoneList.ToArray();
        }

        void Update()
        {
            if (fenceMaterial == null || postMesh == null) return;

            var railRp = new RenderParams(fenceMaterial) { matProps = railBlock };
            DrawBatches(railRp, railBatches);

            var stoneRp = new RenderParams(fenceMaterial) { matProps = stoneBlock };
            DrawBatches(stoneRp, stoneBatches);
        }

        void DrawBatches(RenderParams rp, Matrix4x4[][] batches)
        {
            if (batches == null) return;
            for (int b = 0; b < batches.Length; b++)
            {
                Matrix4x4[] matrices = batches[b];
                // documented, not just asserted: Start() splits into
                // ≤MaxInstancesPerCall chunks, so every batch here fits
                Debug.Assert(matrices.Length <= MaxInstancesPerCall,
                    $"fence batch {b} exceeds RenderMeshInstanced's {MaxInstancesPerCall}-instance cap");
                Graphics.RenderMeshInstanced(rp, postMesh, 0, matrices, matrices.Length);
            }
        }
    }
}
