using System;
using System.Collections.Generic;
using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    public enum BakeoffPipeline { Urp, Hdrp }

    // Stages the Phase 3 visual-target content into the OPEN scene: the
    // true-scale Angle terrain crop plus placeholder road, stone wall, rail
    // fences, wheat, trees, 100 soldiers, smoke, and the ephemeris sun at
    // the canonical comparison time (t=8400 -> 15:20 LMT). Both pipelines
    // stage the SAME deterministic content (AngleBakeoffLayout); only the
    // shaders/sky/exposure plumbing differ, because that is what the
    // bake-off compares.
    public static class AngleBakeoffStage
    {
        public class StageResult
        {
            public Terrain terrain;
            public AngleBakeoffFigures figures;
            public Camera camera;
            public AngleTerrainMetadata meta;
        }

        public static string CropDir => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../../data/heightmap_angle"));

        [MenuItem("BattleAtlas/Bakeoff/Stage Angle Content (URP)")]
        public static void StageUrpMenu() => Stage(BakeoffPipeline.Urp);

        [MenuItem("BattleAtlas/Bakeoff/Stage Angle Content (HDRP)")]
        public static void StageHdrpMenu() => Stage(BakeoffPipeline.Hdrp);

        public static StageResult Stage(BakeoffPipeline p)
        {
            var meta = LoadMeta();
            var terrain = StageTerrain(p, meta);
            StageStatics(p, terrain);
            StageSun(p);
            StageAtmosphere(p);
            var figures = StageFigures(p, terrain);
            var camera = StageCamera(p);
            ApplyCamera(camera, terrain, AngleBakeoffLayout.Cameras[2]); // eye-level default
            return new StageResult
            {
                terrain = terrain, figures = figures, camera = camera, meta = meta,
            };
        }

        static AngleTerrainMetadata LoadMeta()
        {
            string metaPath = Path.Combine(CropDir, "heightmap.json");
            if (!File.Exists(metaPath))
                throw new InvalidOperationException(
                    $"no {metaPath}; run `uv run python -m terrain_pipeline.cli crop` first");
            var meta = JsonUtility.FromJson<AngleTerrainMetadata>(File.ReadAllText(metaPath));
            AngleTerrainFrame.ValidateTrueScale(meta);
            return meta;
        }

        // --- Terrain ---

        static Terrain StageTerrain(BakeoffPipeline p, AngleTerrainMetadata meta)
        {
            byte[] raw = File.ReadAllBytes(Path.Combine(CropDir, "heightmap.raw"));
            var td = new TerrainData();
            td.heightmapResolution = meta.resolution; // BEFORE size (size resets)
            td.size = new Vector3(
                meta.width_m,
                (meta.max_elev_m - meta.min_elev_m) * meta.vertical_exaggeration, // == 1.0
                meta.depth_m);
            td.SetHeights(0, 0, HeightmapDecoder.Decode(raw, meta.resolution));

            var layer = new TerrainLayer
            {
                diffuseTexture = NoiseTexture(
                    "angle-ground",
                    new Color(0.36f, 0.34f, 0.20f), // dry July pasture
                    new Color(0.45f, 0.43f, 0.26f)),
                tileSize = new Vector2(7f, 7f),
                // matte ground: without these the terrain shaders read gloss
                // from defaults and the field renders like wet ice
                smoothness = 0.02f,
                metallic = 0f,
            };
            td.terrainLayers = new[] { layer };

            var go = Terrain.CreateTerrainGameObject(td);
            go.name = "Angle Terrain (true scale)";
            go.transform.position = Vector3.zero;
            var terrain = go.GetComponent<Terrain>();
            terrain.materialTemplate = new Material(Shader.Find(
                p == BakeoffPipeline.Urp
                    ? "Universal Render Pipeline/Terrain/Lit"
                    : "HDRP/TerrainLit"));

            var info = go.AddComponent<BattlefieldInfo>();
            info.widthM = meta.width_m;
            info.depthM = meta.depth_m;
            info.minElevM = meta.min_elev_m;
            info.maxElevM = meta.max_elev_m;
            info.originUtmE = (float)meta.origin_utm_e;
            info.originUtmN = (float)meta.origin_utm_n;
            info.verticalExaggeration = meta.vertical_exaggeration;
            return terrain;
        }

        // --- Static placeholder geometry ---

        static void StageStatics(BakeoffPipeline p, Terrain terrain)
        {
            float Ground(Vector2 xz) =>
                terrain.transform.position.y +
                terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y));

            var parent = new GameObject("Bakeoff Statics");

            // road: terrain-conforming dirt strip
            var roadMesh = RoadMesh(Ground);
            MeshGo(parent, "Emmitsburg Road (placeholder)", roadMesh,
                Lit(p, new Color(0.44f, 0.37f, 0.28f), 0.05f));

            // stone wall
            var wall = CombineAt(InstancedMeshes.BuildWallSegment(),
                AngleBakeoffLayout.WallSegments(), Ground);
            MeshGo(parent, "Stone Wall (placeholder)", wall,
                Lit(p, new Color(0.38f, 0.37f, 0.35f), 0.08f));

            // rail fences (road flanks + approach remnant)
            var fences = CombineAt(InstancedMeshes.BuildFencePost(),
                AngleBakeoffLayout.FencePosts(), Ground);
            MeshGo(parent, "Rail Fences (placeholder)", fences,
                Lit(p, new Color(0.38f, 0.31f, 0.24f), 0.10f));

            // wheat band
            var wheat = CombineAt(InstancedMeshes.BuildCropClump(),
                AngleBakeoffLayout.WheatClumps(), Ground);
            MeshGo(parent, "Wheat (placeholder)", wheat,
                Lit(p, new Color(0.66f, 0.55f, 0.26f), 0.15f));

            // copse of trees: near-tier (two submeshes) + far-tier
            var near = AngleBakeoffLayout.CopseTrees(true);
            var canopy = CombineAt(InstancedMeshes.BuildTreeNear(), near, Ground, 0);
            MeshGo(parent, "Copse Canopy", canopy,
                Lit(p, new Color(0.22f, 0.30f, 0.14f), 0.12f));
            var understory = CombineAt(InstancedMeshes.BuildTreeNear(), near, Ground, 1);
            MeshGo(parent, "Copse Understory", understory,
                Lit(p, new Color(0.12f, 0.16f, 0.08f), 0.10f));
            var far = CombineAt(InstancedMeshes.BuildTree(),
                AngleBakeoffLayout.CopseTrees(false), Ground);
            MeshGo(parent, "Copse Far Trees", far,
                Lit(p, new Color(0.19f, 0.26f, 0.12f), 0.12f));
        }

        static Mesh RoadMesh(Func<Vector2, float> ground)
        {
            var samples = AngleBakeoffLayout.RoadSamples(10f);
            Vector2 dir = (AngleBakeoffLayout.RoadNorth - AngleBakeoffLayout.RoadSouth).normalized;
            Vector2 normal = new Vector2(dir.y, -dir.x);
            float half = AngleBakeoffLayout.RoadWidth / 2f;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i < samples.Length; i++)
            {
                Vector2 l = samples[i] - normal * half;
                Vector2 r = samples[i] + normal * half;
                verts.Add(new Vector3(l.x, ground(l) + 0.06f, l.y));
                verts.Add(new Vector3(r.x, ground(r) + 0.06f, r.y));
                uvs.Add(new Vector2(0f, i));
                uvs.Add(new Vector2(1f, i));
                if (i == 0) continue;
                int b = (i - 1) * 2;
                tris.Add(b); tris.Add(b + 2); tris.Add(b + 1);
                tris.Add(b + 1); tris.Add(b + 2); tris.Add(b + 3);
            }
            var m = new Mesh { name = "Road" };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static Mesh CombineAt(
            Mesh src, List<(Vector2 pos, float yawDeg)> placements,
            Func<Vector2, float> ground, int subMesh = 0)
        {
            var combine = new CombineInstance[placements.Count];
            for (int i = 0; i < placements.Count; i++)
            {
                var (pos, yaw) = placements[i];
                combine[i] = new CombineInstance
                {
                    mesh = src,
                    subMeshIndex = subMesh,
                    transform = Matrix4x4.TRS(
                        new Vector3(pos.x, ground(pos), pos.y),
                        Quaternion.Euler(0f, yaw, 0f),
                        Vector3.one),
                };
            }
            var m = new Mesh
            {
                name = src.name + "Combined",
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            };
            m.CombineMeshes(combine, true, true);
            m.RecalculateBounds();
            return m;
        }

        static GameObject MeshGo(GameObject parent, string name, Mesh mesh, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        // --- Sun, sky, atmosphere ---

        static void StageSun(BakeoffPipeline p)
        {
            var go = new GameObject("Sun (ephemeris 15:20 LMT)");
            var (elev, azim) = SunEphemeris.SunAngles(AngleBakeoffLayout.SecondsSinceMidnight);
            go.transform.rotation = SunEphemeris.LightRotation(elev, azim);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.color = new Color(1f, 0.94f, 0.86f); // mid-afternoon warmth

            if (p == BakeoffPipeline.Urp)
            {
                light.intensity = 1.35f;
            }
            else
            {
                var hd = go.AddComponent<HDAdditionalLightData>();
                hd.SetIntensity(95000f, LightUnit.Lux); // clear-day direct sun
                hd.angularDiameter = 0.53f;
            }
        }

        static void StageAtmosphere(BakeoffPipeline p)
        {
            if (p == BakeoffPipeline.Urp)
            {
                // procedural skybox + analytic distance haze + ACES tonemap
                var sky = new Material(Shader.Find("Skybox/Procedural"));
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 300f;
                RenderSettings.fogEndDistance = 4500f;
                RenderSettings.fogColor = new Color(0.74f, 0.75f, 0.77f);
                DynamicGI.UpdateEnvironment();

                var volGo = new GameObject("URP Volume");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                var tone = profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(true);
                tone.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.ACES;
                vol.sharedProfile = profile;
            }
            else
            {
                var volGo = new GameObject("HDRP Volume");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();

                var env = profile.Add<VisualEnvironment>(true);
                env.skyType.value = (int)SkyType.PhysicallyBased;
                env.skyAmbientMode.value = SkyAmbientMode.Dynamic;
                profile.Add<PhysicallyBasedSky>(true);

                var exposure = profile.Add<UnityEngine.Rendering.HighDefinition.Exposure>(true);
                exposure.mode.value = ExposureMode.Fixed;
                exposure.fixedExposure.value = 13.2f; // sunny-afternoon EV100

                var fog = profile.Add<UnityEngine.Rendering.HighDefinition.Fog>(true);
                fog.enabled.value = true;
                fog.meanFreePath.value = 900f;
                fog.baseHeight.value = 0f;
                fog.maximumHeight.value = 120f;

                var shadows = profile.Add<HDShadowSettings>(true);
                shadows.maxShadowDistance.value = 1200f;

                var tone = profile.Add<UnityEngine.Rendering.HighDefinition.Tonemapping>(true);
                tone.mode.value = UnityEngine.Rendering.HighDefinition.TonemappingMode.ACES;

                vol.sharedProfile = profile;
            }
        }

        // --- Figures + smoke (RenderMeshInstanced path) ---

        static AngleBakeoffFigures StageFigures(BakeoffPipeline p, Terrain terrain)
        {
            var go = new GameObject("Bakeoff Figures (instanced)");
            var figures = go.AddComponent<AngleBakeoffFigures>();
            figures.terrain = terrain;
            figures.csaMaterial = Lit(p, new Color(0.47f, 0.44f, 0.38f), 0.20f); // butternut gray
            figures.usaMaterial = Lit(p, new Color(0.17f, 0.20f, 0.33f), 0.20f); // federal blue
            figures.smokeMaterial = SmokeMaterial(p);
            figures.Build();
            return figures;
        }

        // --- Camera ---

        static Camera StageCamera(BakeoffPipeline p)
        {
            var go = new GameObject("Bakeoff Camera");
            var cam = go.AddComponent<Camera>();
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 4000f;
            cam.allowHDR = true;
            if (p == BakeoffPipeline.Urp)
            {
                var data = go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                data.renderPostProcessing = true;
                data.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
            }
            else
            {
                var hd = go.AddComponent<HDAdditionalCameraData>();
                hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
            }
            return cam;
        }

        public static void ApplyCamera(
            Camera cam, Terrain terrain, AngleBakeoffLayout.CameraDef def)
        {
            float Ground(Vector2 xz) =>
                terrain.transform.position.y +
                terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y));

            var pos = new Vector3(
                def.posXZ.x, Ground(def.posXZ) + def.heightAboveGround, def.posXZ.y);
            var look = new Vector3(
                def.lookXZ.x, Ground(def.lookXZ) + def.lookHeightAboveGround, def.lookXZ.y);
            cam.transform.position = pos;
            cam.transform.rotation = Quaternion.LookRotation(look - pos, Vector3.up);
            cam.fieldOfView = def.fovDeg;
        }

        // --- Materials and textures ---

        static Material Lit(BakeoffPipeline p, Color c, float smoothness)
        {
            var shader = Shader.Find(p == BakeoffPipeline.Urp
                ? "Universal Render Pipeline/Lit" : "HDRP/Lit");
            var m = new Material(shader);
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", smoothness);
            m.enableInstancing = true;
            return m;
        }

        static Material SmokeMaterial(BakeoffPipeline p)
        {
            var c = new Color(0.85f, 0.84f, 0.82f, 0.22f);
            if (p == BakeoffPipeline.Urp)
            {
                var m = Lit(BakeoffPipeline.Urp, c, 0f);
                m.SetFloat("_Surface", 1f); // transparent
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                return m;
            }
            else
            {
                var m = new Material(Shader.Find("HDRP/Lit"));
                m.SetColor("_BaseColor", c);
                m.SetFloat("_Smoothness", 0f);
                m.SetFloat("_SurfaceType", 1f); // transparent, alpha blend
                m.enableInstancing = true;
                HDMaterial.ValidateMaterial(m);
                return m;
            }
        }

        static Texture2D NoiseTexture(string key, Color a, Color b, int size = 128)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                name = key,
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float t = Hash01(key, x * size + y);
                    var c = Color.Lerp(a, b, t);
                    // terrain shaders read smoothness from diffuse ALPHA:
                    // alpha 1 would make the field a sky-mirror at glancing
                    // angles (reads as snow/ice at distance)
                    c.a = 0.03f;
                    pixels[y * size + x] = c;
                }
            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        // deterministic [0,1] from (key, index) — same FNV family as
        // FormationLayout.Jitter, local so the texture stays byte-stable
        static float Hash01(string key, int index)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in key) h = (h ^ c) * 16777619u;
                h = (h ^ (uint)index) * 16777619u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }
    }
}
