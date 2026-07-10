using System;
using System.Collections.Generic;
using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Phase 7 environment stage (plan §12 Phase 7): builds the Angle crop
    // scene from the SOURCED environment bake (data/heightmap_angle/
    // environment.json — every feature there carries claim/editorial ids;
    // pipeline tests enforce it), replacing the Phase 3 placeholder statics.
    // HDRP only (ADR 0003). Deterministic: geometry is a pure function of
    // the baked data + the shared FNV jitter.
    public static class AngleEnvironmentStage
    {
        public class StageResult
        {
            public Terrain terrain;
            public Camera camera;
            public AngleTerrainMetadata meta;
            public AngleEnvironment env;
        }

        public static string CropDir => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../../data/heightmap_angle"));

        const string TexRoot = "Assets/ThirdParty/Materials";
        const string TreeFbx =
            "Assets/ThirdParty/Models/PolyHavenIslandTree02/island_tree_02_decimated.fbx";
        const string PropDir = "Assets/ProjectOwned/Environment/Props";
        const float TreeBaseHeight = 3.41f; // measured on the derived asset

        [MenuItem("BattleAtlas/Phase7/Stage Angle Environment (HDRP)")]
        public static void StageMenu() => StageAll();

        public static StageResult StageAll()
        {
            var meta = LoadMeta();
            var env = LoadEnvironment();
            EnsureTextureImportSettings();
            var terrain = StageTerrain(meta, env);
            float Ground(Vector2 xz) =>
                terrain.transform.position.y +
                terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y));

            var parent = new GameObject("Angle Environment (P7)");
            StageWall(parent, env, Ground);
            StageFences(parent, env, Ground);
            StageTrees(parent, env, Ground);
            StageBuildings(parent, env, Ground);
            StageBattery(parent, env, Ground);
            StageWheat(parent, env, Ground);
            StageSun();
            StageAtmosphere();
            var camera = StageCamera();
            return new StageResult
            {
                terrain = terrain, camera = camera, meta = meta, env = env,
            };
        }

        public static AngleTerrainMetadata LoadMeta()
        {
            string metaPath = Path.Combine(CropDir, "heightmap.json");
            if (!File.Exists(metaPath))
                throw new InvalidOperationException(
                    $"no {metaPath}; run `uv run python -m terrain_pipeline.cli crop` first");
            var meta = JsonUtility.FromJson<AngleTerrainMetadata>(File.ReadAllText(metaPath));
            AngleTerrainFrame.ValidateTrueScale(meta);
            return meta;
        }

        public static AngleEnvironment LoadEnvironment()
        {
            string envPath = Path.Combine(CropDir, "environment.json");
            if (!File.Exists(envPath))
                throw new InvalidOperationException(
                    $"no {envPath}; run `uv run python -m terrain_pipeline.cli environment` first");
            return AngleEnvironmentData.Parse(File.ReadAllText(envPath));
        }

        // --- terrain: heights + ED-11 road carve + ED-15/17 splat ---

        static Terrain StageTerrain(AngleTerrainMetadata meta, AngleEnvironment env)
        {
            byte[] raw = File.ReadAllBytes(Path.Combine(CropDir, "heightmap.raw"));
            float[,] heights = HeightmapDecoder.Decode(raw, meta.resolution);
            CarveRoad(heights, meta, env);

            var td = new TerrainData();
            td.heightmapResolution = meta.resolution;
            td.size = new Vector3(
                meta.width_m,
                (meta.max_elev_m - meta.min_elev_m) * meta.vertical_exaggeration,
                meta.depth_m);
            td.SetHeights(0, 0, heights);
            td.terrainLayers = BuildLayers();
            ApplySplat(td, env);

            var go = Terrain.CreateTerrainGameObject(td);
            go.name = "Angle Terrain (true scale, P7 environment)";
            go.transform.position = Vector3.zero;
            var terrain = go.GetComponent<Terrain>();
            terrain.materialTemplate = new Material(Shader.Find("HDRP/TerrainLit"));

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

        // ED-11: sink the roadbed 0.45 m at the corridor centerline, easing
        // to zero at the traced fences. Stamped per centerline segment.
        static void CarveRoad(float[,] heights, AngleTerrainMetadata meta, AngleEnvironment env)
        {
            int res = meta.resolution;
            float spacing = meta.width_m / (res - 1);
            float elevRange = meta.max_elev_m - meta.min_elev_m;
            var depression = new float[res, res];
            var cl = env.road.centerline;
            for (int s = 0; s < cl.Count - 1; s++)
            {
                Vector2 a = AngleEnvironmentLayout.ToCropLocal(
                    new Vector2(cl[s].x, cl[s].z), env.crop);
                Vector2 b = AngleEnvironmentLayout.ToCropLocal(
                    new Vector2(cl[s + 1].x, cl[s + 1].z), env.crop);
                float bedHalf = Mathf.Min(cl[s].halfWidth, cl[s + 1].halfWidth)
                    - env.road.shoulderM;
                float pad = bedHalf + 2f;
                int c0 = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(a.x, b.x) - pad) / spacing));
                int c1 = Mathf.Min(res - 1, Mathf.CeilToInt((Mathf.Max(a.x, b.x) + pad) / spacing));
                int r0 = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(a.y, b.y) - pad) / spacing));
                int r1 = Mathf.Min(res - 1, Mathf.CeilToInt((Mathf.Max(a.y, b.y) + pad) / spacing));
                for (int r = r0; r <= r1; r++)
                {
                    for (int c = c0; c <= c1; c++)
                    {
                        var p = new Vector2(c * spacing, r * spacing);
                        float dist = DistToSegment(p, a, b);
                        float d = AngleEnvironmentLayout.CarveDepth(
                            dist, bedHalf, env.road.depthM);
                        if (d > depression[r, c]) depression[r, c] = d;
                    }
                }
            }
            for (int r = 0; r < res; r++)
                for (int c = 0; c < res; c++)
                    if (depression[r, c] > 0f)
                        heights[r, c] = Mathf.Max(
                            0f, heights[r, c] - depression[r, c] / elevRange);
        }

        // FormationLayout.Jitter is internal to the runtime assembly; the
        // stage uses the same shared FNV hash via AngleEnvironmentLayout.
        static float Jit(string key, int index, int salt) =>
            2f * AngleEnvironmentLayout.Hash01(key, index * 97 + salt) - 1f;

        static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 v = b - a;
            float l2 = v.sqrMagnitude;
            float t = l2 < 1e-6f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, v) / l2);
            return Vector2.Distance(p, a + t * v);
        }

        // --- terrain layers (§8.3 palette; CC0 Poly Haven via the manifest) ---

        static TerrainLayer Layer(string dir, string baseName, float tile,
            Color remap, float smoothnessCap = 0.25f)
        {
            var layer = new TerrainLayer
            {
                diffuseTexture = LoadTex($"{TexRoot}/{dir}/{baseName}_diff_{Res(dir)}.jpg"),
                normalMapTexture = LoadTex($"{TexRoot}/{dir}/{baseName}_nor_gl_{Res(dir)}.jpg"),
                maskMapTexture = MaskFromArm(
                    $"{TexRoot}/{dir}/{baseName}_arm_{Res(dir)}.jpg", smoothnessCap),
                tileSize = new Vector2(tile, tile),
                diffuseRemapMin = Color.black,
                diffuseRemapMax = remap,
                // HDRP TerrainLit mask map: R=metallic G=AO B=height A=smooth
                maskMapRemapMin = new Vector4(0f, 0f, 0f, 0f),
                maskMapRemapMax = new Vector4(0f, 1f, 0f, smoothnessCap),
            };
            return layer;
        }

        static string Res(string dir) =>
            dir.Contains("Grass") ? "1k" : "2k";

        static Texture2D LoadTex(string path)
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (t == null)
                throw new InvalidOperationException($"missing palette texture {path}");
            return t;
        }

        // Poly Haven arm = AO(R) / Roughness(G) / Metallic(B); HDRP wants
        // R=metal G=AO B=detail A=smoothness. Built transiently (readable
        // import enforced by EnsureTextureImportSettings).
        static Texture2D MaskFromArm(string path, float smoothnessCap)
        {
            var arm = LoadTex(path);
            var src = arm.GetPixels32();
            var dst = new Color32[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                byte smooth = (byte)Mathf.Clamp(
                    Mathf.RoundToInt((255 - src[i].g) * smoothnessCap), 0, 255);
                dst[i] = new Color32(src[i].b, src[i].r, 0, smooth);
            }
            var mask = new Texture2D(arm.width, arm.height, TextureFormat.RGBA32, true, true)
            {
                name = arm.name + "_mask",
                wrapMode = TextureWrapMode.Repeat,
            };
            mask.SetPixels32(dst);
            mask.Apply(true);
            return mask;
        }

        static TerrainLayer[] BuildLayers()
        {
            // order pinned by AngleEnvironmentLayout.ClassWeights
            // measured average albedos drove these remaps: leafy_grass
            // averages tan (151,131,89), so the pasture remap pulls green;
            // brown_mud_dry (115,91,62) is the lighter packed-road base and
            // the darker stony_dirt_path becomes churned/disturbed ground.
            return new[]
            {
                Layer("PolyHavenLeafyGrass", "leafy_grass", 5.5f,
                    new Color(0.72f, 1.0f, 0.58f)),             // July pasture, green pulled
                Layer("PolyHavenSparseGrass", "sparse_grass", 6.5f,
                    new Color(0.95f, 0.90f, 0.68f)),            // dry summer grass
                Layer("PolyHavenWitheredGrass", "withered_grass", 5f,
                    new Color(1.0f, 0.95f, 0.74f)),             // wheat stubble gold
                Layer("PolyHavenBrownMudDry", "brown_mud_dry", 4f,
                    new Color(1.0f, 0.97f, 0.90f)),             // packed dirt road
                Layer("PolyHavenStonyDirtPath", "stony_dirt_path", 4.5f,
                    new Color(0.95f, 0.92f, 0.88f)),            // disturbed/churned soil
            };
        }

        static void ApplySplat(TerrainData td, AngleEnvironment env)
        {
            string splatPath = Path.Combine(CropDir, env.splat.path);
            if (!File.Exists(splatPath))
                throw new InvalidOperationException($"missing {splatPath}");
            byte[] classes = File.ReadAllBytes(splatPath);
            int res = env.splat.resolution;
            if (classes.Length != res * res)
                throw new InvalidOperationException(
                    $"splat raster is {classes.Length} bytes; expected {res * res}");

            td.alphamapResolution = res;
            var maps = new float[res, res, AngleEnvironmentLayout.LayerCount];
            var w = new float[AngleEnvironmentLayout.LayerCount];
            for (int y = 0; y < res; y++)          // alphamap row 0 = south
            {
                int srcRow = res - 1 - y;          // raster row 0 = north
                for (int x = 0; x < res; x++)
                {
                    byte cls = classes[srcRow * res + x];
                    float noise = AngleEnvironmentLayout.Hash01("p7-splat", y * res + x);
                    AngleEnvironmentLayout.ClassWeights(cls, noise, w);
                    for (int l = 0; l < w.Length; l++) maps[y, x, l] = w[l];
                }
            }
            td.SetAlphamaps(0, 0, maps);
        }

        // --- imports ---

        // The palette textures come through the manifest as plain JPGs; the
        // stage (not hand editing) owns their import settings so a clean
        // checkout stages identically.
        public static void EnsureTextureImportSettings()
        {
            foreach (string guid in AssetDatabase.FindAssets(
                "t:Texture2D", new[] { TexRoot, "Assets/ThirdParty/Models" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                bool dirty = false;
                string name = Path.GetFileName(path);
                if (name.Contains("_nor_gl") &&
                    imp.textureType != TextureImporterType.NormalMap)
                {
                    imp.textureType = TextureImporterType.NormalMap;
                    dirty = true;
                }
                if (name.Contains("_arm_") && (imp.sRGBTexture || !imp.isReadable))
                {
                    imp.sRGBTexture = false;
                    imp.isReadable = true;
                    dirty = true;
                }
                if (name.Contains("stacked_stone_wall_diff") && !imp.isReadable)
                {
                    imp.isReadable = true;   // GrayShifted reads it
                    dirty = true;
                }
                if (name.Contains("leaves_diff") && !imp.alphaIsTransparency)
                {
                    imp.alphaIsTransparency = true;
                    dirty = true;
                }
                if (dirty) imp.SaveAndReimport();
            }
        }

        // --- prop materials (HDRP/Lit, palette-bound by slot name) ---

        static Material Lit(Color c, float smoothness, float metallic = 0f)
        {
            var m = new Material(Shader.Find("HDRP/Lit"));
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            m.enableInstancing = true;
            return m;
        }

        static Material TexturedLit(string dir, string baseName, Color tint,
            float smoothnessCap, Vector2 tiling)
        {
            var m = new Material(Shader.Find("HDRP/Lit"));
            m.SetTexture("_BaseColorMap", LoadTex(
                $"{TexRoot}/{dir}/{baseName}_diff_{Res(dir)}.jpg"));
            m.SetTexture("_NormalMap", LoadTex(
                $"{TexRoot}/{dir}/{baseName}_nor_gl_{Res(dir)}.jpg"));
            m.SetTexture("_MaskMap", MaskFromArm(
                $"{TexRoot}/{dir}/{baseName}_arm_{Res(dir)}.jpg", smoothnessCap));
            m.SetColor("_BaseColor", tint);
            m.SetTextureScale("_BaseColorMap", tiling);
            m.EnableKeyword("_NORMALMAP");
            m.EnableKeyword("_MASKMAP");
            m.enableInstancing = true;
            HDMaterial.ValidateMaterial(m);
            return m;
        }

        static Material StoneMaterial()
        {
            var m = TexturedLit(
                "PolyHavenStackedStoneWall", "stacked_stone_wall",
                new Color(0.92f, 0.93f, 0.95f), 0.12f, new Vector2(1.3f, 1.3f));
            // the source albedo averages warm brown (95,69,48); desaturate
            // toward gray so the wall reads as fieldstone, not a dirt berm
            // (deterministic transient texture, like the arm swizzle)
            m.SetTexture("_BaseColorMap", GrayShifted(
                $"{TexRoot}/PolyHavenStackedStoneWall/stacked_stone_wall_diff_2k.jpg",
                0.75f, 1.5f));
            return m;
        }

        static Texture2D GrayShifted(string path, float desat, float gain)
        {
            var srcTex = LoadTex(path);
            var src = srcTex.GetPixels32();
            var dst = new Color32[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                float r = src[i].r, g = src[i].g, b = src[i].b;
                float y = 0.299f * r + 0.587f * g + 0.114f * b;
                dst[i] = new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(r, y, desat) * gain), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(g, y, desat) * gain), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(b, y, desat) * gain), 0, 255),
                    255);
            }
            var tex = new Texture2D(srcTex.width, srcTex.height, TextureFormat.RGBA32, true)
            {
                name = srcTex.name + "_gray",
                wrapMode = TextureWrapMode.Repeat,
            };
            tex.SetPixels32(dst);
            tex.Apply(true);
            return tex;
        }

        static Material TimberMaterial(Color tint) => TexturedLit(
            "PolyHavenWeatheredPlanks", "weathered_planks",
            tint, 0.15f, new Vector2(0.6f, 0.6f));

        // --- the stone wall (claim-wall-structure/-profile; ED-12) ---

        static void StageWall(GameObject parent, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            var line = AngleEnvironmentData.Points(env.wall.polylineFlat);
            for (int i = 0; i < line.Count; i++)
                line[i] = AngleEnvironmentLayout.ToCropLocal(line[i], env.crop);

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            float halfW = env.wall.widthM / 2f;
            float arc = 0f;
            Vector2 prev = line[0];
            var samples = Resample(line, 2f);
            for (int i = 0; i < samples.Count; i++)
            {
                Vector2 p = samples[i];
                arc += Vector2.Distance(prev, p);
                prev = p;
                Vector2 dir = i < samples.Count - 1
                    ? (samples[i + 1] - p).normalized
                    : (p - samples[i - 1]).normalized;
                Vector2 n = new Vector2(dir.y, -dir.x);
                // deterministic irregularity: an 1863 farm wall, not a curb
                float hJit = 0.06f * Jit("p7-wall", i, 17);
                float wJit = 0.05f * Jit("p7-wall", i, 29);
                float g = ground(p);
                float top = g + env.wall.heightM + hJit;
                Vector2 l = p - n * (halfW + wJit);
                Vector2 r = p + n * (halfW + wJit);
                int b = verts.Count;
                verts.Add(new Vector3(l.x, g - 0.08f, l.y));    // 0 base L
                verts.Add(new Vector3(l.x, top, l.y));          // 1 top L
                verts.Add(new Vector3(r.x, top, r.y));          // 2 top R
                verts.Add(new Vector3(r.x, g - 0.08f, r.y));    // 3 base R
                float u = arc / 1.8f;
                uvs.Add(new Vector2(u, 0f));
                uvs.Add(new Vector2(u, 0.5f));
                uvs.Add(new Vector2(u, 0.75f));
                uvs.Add(new Vector2(u, 1.25f));
                if (i == 0) continue;
                int a4 = b - 4;
                // left face, top face, right face
                Quad(tris, a4 + 0, b + 0, b + 1, a4 + 1);
                Quad(tris, a4 + 1, b + 1, b + 2, a4 + 2);
                Quad(tris, a4 + 2, b + 2, b + 3, a4 + 3);
            }
            var mesh = new Mesh
            {
                name = "StoneWall",
                indexFormat = IndexFormat.UInt32,
            };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshGo(parent, "Stone Wall (traced)", mesh, StoneMaterial());

            // rails on top along the 69th PA front (claim-wall-rails-on-top)
            var railRails = new List<AngleEnvironmentLayout.RailSegment>();
            float z0 = env.wall.railsZRange[0] - env.crop.z0;
            float z1 = env.wall.railsZRange[1] - env.crop.z0;
            int idx = 0;
            foreach (var p in samples)
            {
                if (p.y < z0 || p.y > z1) continue;
                if (idx++ % 2 != 0) continue;
                float yaw = 12f * Jit("p7-wallrails", idx, 43);
                var dir = new Vector2(Mathf.Sin(yaw * Mathf.Deg2Rad),
                                      Mathf.Cos(yaw * Mathf.Deg2Rad));
                railRails.Add(new AngleEnvironmentLayout.RailSegment
                {
                    a = p - dir * 1.3f,
                    b = p + dir * 1.3f,
                    height = env.wall.heightM + 0.06f,
                });
            }
            var railMesh = RailsMesh(railRails, ground, 0.08f, 0.06f);
            MeshGo(parent, "Wall Top Rails (69th PA front)", railMesh,
                TimberMaterial(new Color(0.72f, 0.66f, 0.58f)));
        }

        static void Quad(List<int> tris, int a, int b, int c, int d)
        {
            tris.Add(a); tris.Add(b); tris.Add(c);
            tris.Add(a); tris.Add(c); tris.Add(d);
        }

        static List<Vector2> Resample(List<Vector2> line, float step)
        {
            var outPts = new List<Vector2> { line[0] };
            for (int s = 0; s < line.Count - 1; s++)
            {
                Vector2 a = line[s], b = line[s + 1];
                float len = Vector2.Distance(a, b);
                int n = Mathf.Max(1, Mathf.CeilToInt(len / step));
                for (int i = 1; i <= n; i++)
                    outPts.Add(Vector2.Lerp(a, b, i / (float)n));
            }
            return outPts;
        }

        // --- fences (claim-fence-*-structure, claim-fences-high-climb; ED-12) ---

        static void StageFences(GameObject parent, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            var postList = new List<AngleEnvironmentLayout.Post>();
            var railList = new List<AngleEnvironmentLayout.RailSegment>();
            var wormRails = new List<AngleEnvironmentLayout.RailSegment>();
            foreach (var fence in env.fences)
            {
                var line = AngleEnvironmentData.Points(fence.polylineFlat);
                for (int i = 0; i < line.Count; i++)
                    line[i] = AngleEnvironmentLayout.ToCropLocal(line[i], env.crop);
                if (fence.style == "worm")
                    AngleEnvironmentLayout.Worm(
                        fence.featureId, line, fence.heightM, wormRails);
                else
                    AngleEnvironmentLayout.PostAndRail(
                        fence.featureId, line, fence.postSpacingM, fence.heightM,
                        postList, railList);
            }
            var timber = TimberMaterial(new Color(0.78f, 0.72f, 0.62f));
            MeshGo(parent, "Rail Fences (traced)", RailsMesh(railList, ground, 0.09f, 0.07f), timber);
            MeshGo(parent, "Fence Posts (traced)", PostsMesh(postList, ground), timber);
            if (wormRails.Count > 0)
                MeshGo(parent, "Worm Fences (traced)",
                    RailsMesh(wormRails, ground, 0.09f, 0.08f), timber);
        }

        static Mesh RailsMesh(List<AngleEnvironmentLayout.RailSegment> rails,
            Func<Vector2, float> ground, float w, float h)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            foreach (var rail in rails)
            {
                // zero-length rails would normalize to NaN and smear a
                // garbage triangle across the sky
                if (Vector2.Distance(rail.a, rail.b) < 0.05f) continue;
                Vector2 d2 = (rail.b - rail.a).normalized;
                Vector2 n2 = new Vector2(d2.y, -d2.x) * (w / 2f);
                float ya = ground(rail.a) + rail.height;
                float yb = ground(rail.b) + rail.height;
                AddBoxBetween(verts, uvs, tris,
                    new Vector3(rail.a.x, ya, rail.a.y),
                    new Vector3(rail.b.x, yb, rail.b.y),
                    new Vector3(n2.x, 0f, n2.y), h);
            }
            var m = new Mesh { name = "Rails", indexFormat = IndexFormat.UInt32 };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static Mesh PostsMesh(List<AngleEnvironmentLayout.Post> posts,
            Func<Vector2, float> ground)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            foreach (var post in posts)
            {
                float g = ground(post.pos);
                var up = new Vector3(0f, 1.58f, 0f);
                var a = new Vector3(post.pos.x, g - 0.05f, post.pos.y);
                float rad = post.bearingDeg * Mathf.Deg2Rad;
                var n = new Vector3(Mathf.Cos(rad), 0f, -Mathf.Sin(rad)) * 0.06f;
                AddBoxBetween(verts, uvs, tris, a, a + up, n, 0.12f);
            }
            var m = new Mesh { name = "Posts", indexFormat = IndexFormat.UInt32 };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // Box from a to b: cross-section spanned by `side` (half-extent
        // vector) and thickness t perpendicular to both.
        static void AddBoxBetween(List<Vector3> verts, List<Vector2> uvs,
            List<int> tris, Vector3 a, Vector3 b, Vector3 side, float t)
        {
            Vector3 axis = (b - a).normalized;
            Vector3 upv = Vector3.Cross(axis, side.normalized).normalized * (t / 2f);
            int i0 = verts.Count;
            Vector3[] corners =
            {
                a - side - upv, a + side - upv, a + side + upv, a - side + upv,
                b - side - upv, b + side - upv, b + side + upv, b - side + upv,
            };
            float len = Vector3.Distance(a, b);
            foreach (var c in corners) verts.Add(c);
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(0.12f, 0));
            uvs.Add(new Vector2(0.12f, 0.12f)); uvs.Add(new Vector2(0, 0.12f));
            uvs.Add(new Vector2(len / 2f, 0)); uvs.Add(new Vector2(len / 2f + 0.12f, 0));
            uvs.Add(new Vector2(len / 2f + 0.12f, 0.12f)); uvs.Add(new Vector2(len / 2f, 0.12f));
            int[,] faces =
            {
                { 0, 1, 5, 4 }, { 1, 2, 6, 5 }, { 2, 3, 7, 6 },
                { 3, 0, 4, 7 }, { 1, 0, 3, 2 }, { 4, 5, 6, 7 },
            };
            for (int f = 0; f < 6; f++)
                Quad(tris, i0 + faces[f, 0], i0 + faces[f, 1],
                     i0 + faces[f, 2], i0 + faces[f, 3]);
        }

        // --- trees (claim-copse-*; ED-14) ---

        static void StageTrees(GameObject parent, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreeFbx);
            if (prefab == null)
                throw new InvalidOperationException($"missing {TreeFbx}");
            var mats = TreeMaterials();
            var root = new GameObject("Trees (traced groves + orchards)");
            root.transform.SetParent(parent.transform, false);
            foreach (var grove in env.groves) PlaceTrees(root, prefab, mats, grove, env, ground);
            foreach (var orch in env.orchards) PlaceTrees(root, prefab, mats, orch, env, ground);
        }

        static Dictionary<string, Material> TreeMaterials()
        {
            string dir = "Assets/ThirdParty/Models/PolyHavenIslandTree02";
            var trunk = new Material(Shader.Find("HDRP/Lit"));
            trunk.SetTexture("_BaseColorMap",
                LoadTex($"{dir}/island_tree_02_diff_1k.jpg"));
            trunk.SetFloat("_Smoothness", 0.12f);
            HDMaterial.ValidateMaterial(trunk);
            var branches = new Material(Shader.Find("HDRP/Lit"));
            branches.SetTexture("_BaseColorMap",
                LoadTex($"{dir}/island_tree_02_branches_diff_1k.jpg"));
            branches.SetFloat("_Smoothness", 0.15f);
            HDMaterial.ValidateMaterial(branches);
            var leaves = new Material(Shader.Find("HDRP/Lit"));
            leaves.SetTexture("_BaseColorMap",
                LoadTex($"{dir}/island_tree_02_leaves_diff_1k.png"));
            leaves.SetFloat("_Smoothness", 0.2f);
            leaves.SetColor("_BaseColor", new Color(0.66f, 0.74f, 0.55f));
            HDMaterial.SetAlphaClipping(leaves, true);
            leaves.SetFloat("_AlphaCutoff", 0.45f);
            leaves.SetFloat("_DoubleSidedEnable", 1f);
            HDMaterial.ValidateMaterial(leaves);
            return new Dictionary<string, Material>
            {
                { "island_tree_02", trunk },
                { "island_tree_02_branches", branches },
                { "island_tree_02_leaves", leaves },
            };
        }

        static void PlaceTrees(GameObject root, GameObject prefab,
            Dictionary<string, Material> mats, EnvGrove grove,
            AngleEnvironment env, Func<Vector2, float> ground)
        {
            var groveGo = new GameObject(grove.featureId);
            groveGo.transform.SetParent(root.transform, false);
            foreach (var tree in grove.trees)
            {
                Vector2 p = AngleEnvironmentLayout.ToCropLocal(
                    new Vector2(tree.x, tree.z), env.crop);
                var go = (GameObject)UnityEngine.Object.Instantiate(prefab);
                go.name = $"tree_{tree.x:F0}_{tree.z:F0}";
                go.transform.SetParent(groveGo.transform, false);
                float scale = tree.heightM / TreeBaseHeight;
                go.transform.position = new Vector3(p.x, ground(p) - 0.03f, p.y);
                // compose with the prefab's own rotation: the FBX importer's
                // axis compensation lives there (clobbering it lays scanned
                // trees flat on their sides)
                go.transform.rotation =
                    Quaternion.Euler(0f, tree.yawDeg, 0f) * prefab.transform.rotation;
                go.transform.localScale = Vector3.one * scale;
                foreach (var mr in go.GetComponentsInChildren<MeshRenderer>())
                {
                    var shared = mr.sharedMaterials;
                    var replaced = new Material[shared.Length];
                    for (int i = 0; i < shared.Length; i++)
                        replaced[i] = mats.TryGetValue(
                            shared[i] != null ? shared[i].name.Replace(" (Instance)", "") : "",
                            out var m) ? m : mats["island_tree_02"];
                    mr.sharedMaterials = replaced;
                    mr.shadowCastingMode = ShadowCastingMode.On;
                }
            }
        }

        // --- Codori buildings (claim-codori-*; ED-13) ---

        static void StageBuildings(GameObject parent, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            foreach (var b in env.buildings)
            {
                string fbx = $"{PropDir}/codori_{b.kind}.fbx";
                SpawnProp(parent, fbx, $"{b.id} (ED-13 massing)",
                    new Vector2(b.x, b.z), b.yawDeg, env, ground);
            }
        }

        // --- Cushing's battery (claim-cushing-*; ED-16) ---

        static void StageBattery(GameObject parent, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            var root = new GameObject("Cushing's Battery (ED-16)");
            root.transform.SetParent(parent.transform, false);
            foreach (var gun in env.battery.guns)
            {
                string fbx = gun.state == "intact"
                    ? $"{PropDir}/ordnance_rifle.fbx"
                    : gun.state == "disabled"
                        ? $"{PropDir}/ordnance_rifle_disabled.fbx"
                        : $"{PropDir}/ordnance_rifle_wrecked.fbx";
                SpawnProp(root, fbx, $"gun_{gun.at}_{gun.state}",
                    new Vector2(gun.x, gun.z), gun.yawDeg, env, ground);
            }
            foreach (var p in env.battery.limbers)
                SpawnProp(root, $"{PropDir}/limber.fbx", "limber",
                    new Vector2(p.x, p.z), p.yawDeg, env, ground);
            foreach (var p in env.battery.caissons)
                SpawnProp(root, $"{PropDir}/caisson.fbx", "caisson",
                    new Vector2(p.x, p.z), p.yawDeg, env, ground);
            foreach (var p in env.battery.detritus)
            {
                string fbx = p.kind == "ammo_chest"
                    ? $"{PropDir}/ammo_chest.fbx"
                    : $"{PropDir}/wheel_wreck.fbx";
                SpawnProp(root, fbx, p.kind, new Vector2(p.x, p.z),
                    p.yawDeg, env, ground);
            }
        }

        static Dictionary<string, Material> propMats;

        static Dictionary<string, Material> PropMaterials()
        {
            // P10 fix: the cache is static but the materials are
            // scene-lifetime — a NewScene between two stagings (the Gate
            // P10 determinism pair) destroys them and destroyed materials
            // render magenta. Rebuild when the cache holds corpses.
            if (propMats != null && propMats["gun_iron"] != null)
                return propMats;
            // US carriages were painted olive drab; ironwork black
            // (Ordnance Manual finish norms); timber families reuse the
            // weathered-planks palette ingredient.
            propMats = new Dictionary<string, Material>
            {
                { "gun_iron", Lit(new Color(0.08f, 0.08f, 0.09f), 0.42f, 0.85f) },
                { "carriage_wood", Lit(new Color(0.29f, 0.31f, 0.18f), 0.22f) },
                { "wheel_wood", Lit(new Color(0.27f, 0.29f, 0.17f), 0.22f) },
                { "chest_wood", Lit(new Color(0.30f, 0.32f, 0.19f), 0.22f) },
                { "dark_wood", TimberMaterial(new Color(0.42f, 0.36f, 0.28f)) },
                { "siding", TimberMaterial(new Color(0.85f, 0.82f, 0.74f)) },
                { "barn_wood", TimberMaterial(new Color(0.55f, 0.46f, 0.36f)) },
                { "roof", TimberMaterial(new Color(0.38f, 0.35f, 0.30f)) },
                { "stone", StoneMaterial() },
            };
            return propMats;
        }

        static void SpawnProp(GameObject parent, string fbxPath, string name,
            Vector2 macroXZ, float yawDeg, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (prefab == null)
                throw new InvalidOperationException($"missing prop {fbxPath}");
            Vector2 p = AngleEnvironmentLayout.ToCropLocal(macroXZ, env.crop);
            var go = (GameObject)UnityEngine.Object.Instantiate(prefab);
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.position = new Vector3(p.x, ground(p), p.y);
            go.transform.rotation =
                Quaternion.Euler(0f, yawDeg, 0f) * prefab.transform.rotation;
            var mats = PropMaterials();
            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                var shared = mr.sharedMaterials;
                var replaced = new Material[shared.Length];
                for (int i = 0; i < shared.Length; i++)
                {
                    string key = shared[i] != null
                        ? shared[i].name.Replace(" (Instance)", "") : "";
                    replaced[i] = mats.TryGetValue(key, out var m)
                        ? m : mats["dark_wood"];
                }
                mr.sharedMaterials = replaced;
                mr.shadowCastingMode = ShadowCastingMode.On;
            }
        }

        // --- standing wheat (claim-fields-west-mixed; ED-15) ---

        static void StageWheat(GameObject parent, AngleEnvironment env,
            Func<Vector2, float> ground)
        {
            string splatPath = Path.Combine(CropDir, env.splat.path);
            byte[] classes = File.ReadAllBytes(splatPath);
            int res = env.splat.resolution;
            float size = env.crop.x1 - env.crop.x0;
            byte ClassAt(Vector2 macro)
            {
                int c = Mathf.Clamp((int)((macro.x - env.crop.x0) / size * res), 0, res - 1);
                int rFromN = Mathf.Clamp(
                    (int)((env.crop.z1 - macro.y) / size * res), 0, res - 1);
                return classes[rFromN * res + c];
            }
            var clumps = AngleEnvironmentLayout.WheatClumps(env.wheat, env.crop, ClassAt);
            if (clumps.Count == 0) return;
            var combine = new CombineInstance[clumps.Count];
            var src = InstancedMeshes.BuildCropClump();
            for (int i = 0; i < clumps.Count; i++)
            {
                Vector2 p = AngleEnvironmentLayout.ToCropLocal(clumps[i].pos, env.crop);
                float sc = 0.82f + 0.28f * AngleEnvironmentLayout.Hash01("p7-wheat-s", i);
                combine[i] = new CombineInstance
                {
                    mesh = src,
                    transform = Matrix4x4.TRS(
                        new Vector3(p.x, ground(p), p.y),
                        Quaternion.Euler(0f, clumps[i].yawDeg, 0f),
                        new Vector3(sc, sc, sc)),
                };
            }
            var mesh = new Mesh
            {
                name = "WheatCombined",
                indexFormat = IndexFormat.UInt32,
            };
            mesh.CombineMeshes(combine, true, true);
            mesh.RecalculateBounds();
            MeshGo(parent, "Standing Wheat (ED-15 patches)", mesh,
                Lit(new Color(0.55f, 0.46f, 0.22f), 0.16f));
        }

        static GameObject MeshGo(GameObject parent, string name, Mesh mesh, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.On;
            return go;
        }

        // --- sun, sky, camera (ED-18: unchanged Phase 3/4 atmosphere) ---

        static void StageSun()
        {
            var go = new GameObject("Sun (ephemeris 15:20 LMT)");
            var (elev, azim) = SunEphemeris.SunAngles(AngleBakeoffLayout.SecondsSinceMidnight);
            go.transform.rotation = SunEphemeris.LightRotation(elev, azim);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.color = new Color(1f, 0.94f, 0.86f);
            var hd = go.AddComponent<HDAdditionalLightData>();
            hd.SetIntensity(95000f, LightUnit.Lux);
            hd.angularDiameter = 0.53f;
        }

        static void StageAtmosphere()
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
            exposure.fixedExposure.value = 13.2f;

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

        static Camera StageCamera()
        {
            var go = new GameObject("P7 Camera");
            var cam = go.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 4000f;
            cam.allowHDR = true;
            var hd = go.AddComponent<HDAdditionalCameraData>();
            hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
            return cam;
        }
    }
}
