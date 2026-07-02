using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // Instanced wheat clumps on Field-class splat texels inside a camera
    // ring — the NEAR band of the report's three-band crop read (far = the
    // Field layer's golden tint, mid = the relief bake; research doc
    // 2026-07-02-descriptive-graphics-techniques.md §1c). Waist-high wheat
    // at soldier zoom is concealment the viewer reads the way the 8th
    // Ohio's skirmishers did — and it renders only where the traced,
    // provenance-carrying splatmap says Field.
    //
    // Own RenderMeshInstanced field cloned from the VegetationField pattern
    // (NOT Unity terrain details — placement determinism must be OUR hash,
    // not version-drifty engine internals): clump positions are FNV-jittered
    // per splat texel, the ring is rebuilt only when the camera crosses a
    // texel boundary, and everything fills one preallocated matrix buffer —
    // zero steady-state allocation. Distance fade is scale-to-zero at
    // rebuild time, never alpha (TBDR trap #4).
    //
    // Budget (report §1c): a 250 m ring at ~1 clump / 4 m² over typical
    // ~30% field coverage ≈ 5-15k clumps ≈ 0.5-1.5 ms GPU inside the ring,
    // zero outside; MaxClumps caps the worst case. Tune ClumpsPerSquareMeter
    // down first if the device says otherwise.
    public class CropField : MonoBehaviour
    {
        public Material cropMaterial;

        // Graphics.RenderMeshInstanced caps out at 1023 instances per call;
        // the flat buffer draws in ≤1023-instance chunks via startInstance.
        const int MaxInstancesPerCall = 1023;
        const int MaxBatches = 16;
        public const int MaxClumps = MaxBatches * MaxInstancesPerCall; // 16368

        // camera ring: clumps exist only inside RingRadius (the report's
        // 150-300 m near band); scale-to-zero fade runs over the outer ~28%
        const float RingRadius = 250f;

        // ~1 clump / 4 m² at full Field weight
        const float ClumpsPerSquareMeter = 0.25f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // ripe July wheat: deeper gold than the Field layer tint so clumps
        // read against their own ground
        static readonly Color WheatColor = new Color(0.72f, 0.60f, 0.30f);

        Mesh clumpMesh;
        MaterialPropertyBlock block;
        Camera ringCamera;
        Terrain terrain;
        float terrainBaseY;

        // Field-layer weights flattened from the terrain alphamaps once in
        // Start (row-major, index = z * resolution + x, z from the south
        // edge — Unity's alphamap axis order, see SplatmapDecoder).
        float[] fieldWeights;
        int resolution;
        float texelSize;

        // preallocated ring state: rebuilt in-place on texel crossing,
        // untouched (and unallocated) every other frame
        Matrix4x4[] matrices;
        int clumpCount;
        Bounds ringBounds;
        (int x, int z) builtCell;
        bool hasBuilt;
        // cached once: a fresh lambda per rebuild would allocate a closure
        System.Func<float, float, float> groundYFunc;

        // The splat texel the camera stands on — the ring rebuild trigger.
        public static (int x, int z) CameraCell(Vector3 camPos, float texelSize) =>
            (Mathf.FloorToInt(camPos.x / texelSize), Mathf.FloorToInt(camPos.z / texelSize));

        // deterministic pseudo-random in [0,1) from (texel, clump, salt) —
        // the same FNV scheme as FormationLayout.Jitter, int-keyed so
        // per-texel placement allocates no strings (OUR hash, never engine
        // details: scrubbing and version upgrades must reproduce the field)
        static float Hash01(int tx, int tz, int i, int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)tx) * 16777619u;
                h = (h ^ (uint)tz) * 16777619u;
                h = (h ^ (uint)i) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }

        // Pure ring fill: walks the splat texels within ringRadius of the
        // camera, spawns weight-proportional clump counts at FNV-jittered
        // positions, scales the outer band to zero (fade without alpha),
        // and stops at the buffer's capacity (the batch cap — excess texels
        // drop deterministically in scan order). Returns the clump count.
        // groundY is injected like UnitFormationRenderer.BuildMatrices so
        // tests run without a terrain.
        public static int Rebuild(
            float[] fieldWeights, int resolution, float texelSize,
            Vector3 camPos, float ringRadius,
            System.Func<float, float, float> groundY, Matrix4x4[] buffer)
        {
            (int camX, int camZ) = CameraCell(camPos, texelSize);
            int reach = Mathf.CeilToInt(ringRadius / texelSize);
            float clumpsPerTexel = texelSize * texelSize * ClumpsPerSquareMeter;
            float fadeStart = ringRadius * 0.72f;
            int count = 0;
            for (int tz = camZ - reach; tz <= camZ + reach; tz++)
            {
                if (tz < 0 || tz >= resolution) continue;
                for (int tx = camX - reach; tx <= camX + reach; tx++)
                {
                    if (tx < 0 || tx >= resolution) continue;
                    float weight = fieldWeights[tz * resolution + tx];
                    if (weight <= 0f) continue;
                    int clumps = Mathf.RoundToInt(weight * clumpsPerTexel);
                    for (int i = 0; i < clumps; i++)
                    {
                        float x = (tx + Hash01(tx, tz, i, 3)) * texelSize;
                        float z = (tz + Hash01(tx, tz, i, 5)) * texelSize;
                        float dx = x - camPos.x, dz = z - camPos.z;
                        float dist = Mathf.Sqrt(dx * dx + dz * dz);
                        if (dist > ringRadius) continue;
                        // scale-to-zero over the outer band, plus a small
                        // deterministic size/yaw variation per clump
                        float fade = Mathf.Clamp01((ringRadius - dist) / (ringRadius - fadeStart));
                        float scale = fade * (0.8f + 0.4f * Hash01(tx, tz, i, 7));
                        if (scale <= 0f) continue;
                        if (count >= buffer.Length) return count;
                        buffer[count++] = Matrix4x4.TRS(
                            new Vector3(x, groundY(x, z), z),
                            Quaternion.Euler(0f, Hash01(tx, tz, i, 11) * 360f, 0f),
                            Vector3.one * scale);
                    }
                }
            }
            return count;
        }

        void Start()
        {
            clumpMesh = InstancedMeshes.BuildCropClump();
            ringCamera = Camera.main;
            groundYFunc = GroundY;
            matrices = new Matrix4x4[MaxClumps];

            if (cropMaterial != null)
            {
                // asset reference keeps the shader in device builds; the
                // instancing flag itself is not stripped, so it's safe here
                cropMaterial.enableInstancing = true;
            }
            block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, WheatColor);

            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogWarning("CropField: no active terrain found; no wheat to place");
                return;
            }
            terrainBaseY = terrain.transform.position.y;

            // The Field weights live in the terrain alphamaps the importer
            // decoded from the traced splatmap (SplatmapDecoder.LayerField);
            // flatten the one layer we read so rebuilds touch a plain array.
            TerrainData data = terrain.terrainData;
            if (data.alphamapLayers <= SplatmapDecoder.LayerField)
            {
                Debug.LogWarning(
                    "CropField: terrain has no Field splat layer; run " +
                    "BattleAtlas/Import Land Cover first");
                return;
            }
            resolution = data.alphamapResolution;
            texelSize = data.size.x / resolution;
            float[,,] alphamaps = data.GetAlphamaps(0, 0, resolution, resolution);
            fieldWeights = new float[resolution * resolution];
            for (int z = 0; z < resolution; z++)
                for (int x = 0; x < resolution; x++)
                    fieldWeights[z * resolution + x] =
                        alphamaps[z, x, SplatmapDecoder.LayerField];
        }

        float GroundY(float x, float z) =>
            terrainBaseY + terrain.SampleHeight(new Vector3(x, 0f, z));

        void Update()
        {
            if (cropMaterial == null || fieldWeights == null || ringCamera == null) return;

            Vector3 camPos = ringCamera.transform.position;
            (int x, int z) cell = CameraCell(camPos, texelSize);
            // ring rebuilt ONLY when the camera crosses a splat-texel
            // boundary — steady-state frames just re-issue the draws
            if (!hasBuilt || cell != builtCell)
            {
                clumpCount = Rebuild(
                    fieldWeights, resolution, texelSize, camPos, RingRadius,
                    groundYFunc, matrices);
                ringBounds = RingBoundsOf(matrices, clumpCount);
                builtCell = cell;
                hasBuilt = true;
            }
            if (clumpCount == 0) return;

            // shadows stay explicitly Off: enabling main-light shadows later
            // must not re-render thousands of wheat quads into each cascade
            var rp = new RenderParams(cropMaterial)
            {
                matProps = block,
                shadowCastingMode = ShadowCastingMode.Off,
                // one explicit bound for the whole ring: it hugs the camera,
                // so per-chunk cell bounds would buy nothing
                worldBounds = ringBounds,
            };
            for (int start = 0; start < clumpCount; start += MaxInstancesPerCall)
            {
                int n = Mathf.Min(MaxInstancesPerCall, clumpCount - start);
                Graphics.RenderMeshInstanced(rp, clumpMesh, 0, matrices, n, start);
            }
        }

        // Min/max of the placed clump positions plus the mesh extent — the
        // rebuild-time pass keeps Update's culling bound exact without any
        // per-frame work.
        static Bounds RingBoundsOf(Matrix4x4[] matrices, int count)
        {
            if (count == 0) return new Bounds();
            Vector3 min = matrices[0].GetColumn(3);
            Vector3 max = min;
            for (int i = 1; i < count; i++)
            {
                Vector3 p = matrices[i].GetColumn(3);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
            var margin = new Vector3(1f, 1.5f, 1f); // clump is 0.9m tall, 0.7m wide
            var bounds = new Bounds();
            bounds.SetMinMax(min - margin, max + margin);
            return bounds;
        }
    }
}
