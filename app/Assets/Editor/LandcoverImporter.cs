using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    public static class LandcoverImporter
    {
        const string GeneratedDir = "Assets/Generated";

        // Beyond this distance Unity samples the pre-composited basemap
        // instead of per-layer textures — per-layer work only runs near the
        // camera, essentially free fill rate at strategic zoom (research doc
        // 2026-07-02-descriptive-graphics-techniques.md §1b).
        const float BasemapDistanceM = 1000f;

        // Muted, sand-table palette (docs/superpowers/plans/2026-07-01-landcover.md).
        struct LayerSpec
        {
            public string Name;
            public Color Color;
            public LayerSpec(string name, Color color) { Name = name; Color = color; }
        }

        // EXACTLY four layers, hard constraint (research doc §1a): URP's
        // Terrain Lit packs 4 layers per pass (one RGBA control map); a 5th
        // triggers an add pass that re-rasterizes the ENTIRE terrain and
        // silently disables height-based blending. Orchard is merged into
        // the pasture base at the splat level — orchards read from their
        // baked tree ROW structure (VegetationField's regular 8 m grid), and
        // the ground under an 1863 orchard is grass. Order must match
        // SplatmapDecoder's layer indices.
        static readonly LayerSpec[] Layers = new[]
        {
            new LayerSpec("Pasture", HexColor("#8FA05A")),
            new LayerSpec("Field", HexColor("#C9B36A")),
            new LayerSpec("Woods", HexColor("#4C6340")),
            new LayerSpec("Marsh", HexColor("#6E7F72")),
        };

        static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        [MenuItem("BattleAtlas/Import Land Cover")]
        public static void Import()
        {
            var terrainGo = GameObject.Find("Gettysburg Terrain");
            if (terrainGo == null)
            {
                Debug.LogError(
                    "LandcoverImporter: no 'Gettysburg Terrain' GameObject in the scene; " +
                    "run BattleAtlas/Import Heightmap first. Aborting.");
                return;
            }
            var terrain = terrainGo.GetComponent<Terrain>();
            if (terrain == null || terrain.terrainData == null)
            {
                Debug.LogError(
                    "LandcoverImporter: 'Gettysburg Terrain' has no Terrain component/TerrainData. Aborting.");
                return;
            }

            // repo layout: <root>/app/Assets and <root>/data/landcover
            string dir = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../../data/landcover"));
            string pngPath = Path.Combine(dir, "splatmap.png");
            byte[] pngBytes = File.ReadAllBytes(pngPath);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(pngBytes))
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: failed to decode {pngPath} as a PNG.");

            int resolution = tex.width;
            if (tex.width != tex.height)
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: splatmap.png is {tex.width}x{tex.height}, must be square.");
            if (resolution < 2 || (resolution & (resolution - 1)) != 0)
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: splatmap.png resolution {resolution} is not a power of two; " +
                    "Unity's alphamapResolution requires it.");

            // The B3 relief bake goes INTO the layer tints: albedo = tint x
            // relief, so swales and dead ground darken with zero shader work
            // and zero runtime cost. relief.json is the self-describing
            // sidecar (decode constants), same pattern as heightmap.json.
            var reliefMeta = JsonUtility.FromJson<ReliefMetadata>(
                File.ReadAllText(Path.Combine(dir, "relief.json")));
            if (reliefMeta.row0 != "north")
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: relief.json row0 '{reliefMeta.row0}' != 'north'; " +
                    "ReliefDecoder assumes north-first rows");
            if (reliefMeta.resolution != resolution)
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: relief bake is {reliefMeta.resolution}px but the " +
                    $"splatmap is {resolution}px; the pipeline bakes them texel-aligned on " +
                    "purpose (cli.py) — regenerate both.");
            Color32[] relief = LoadReliefPixels(
                Path.Combine(dir, "relief.png"), reliefMeta.resolution);
            Color32[] reliefContours = LoadReliefPixels(
                Path.Combine(dir, "relief_contours.png"), reliefMeta.resolution);

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));

            var terrainData = terrain.terrainData;
            // tileSize = the FULL terrain extent: each albedo texel is one
            // fixed ground point, so the relief modulation is spatially
            // anchored (and the alpha=0 anti-gloss trick still rides in the
            // texture; see ReliefDecoder).
            var terrainExtent = new Vector2(terrainData.size.x, terrainData.size.z);
            var terrainLayers = new TerrainLayer[Layers.Length];
            var baseAlbedos = new Texture2D[Layers.Length];
            var contourAlbedos = new Texture2D[Layers.Length];
            for (int i = 0; i < Layers.Length; i++)
                terrainLayers[i] = CreateTerrainLayer(
                    Layers[i], relief, reliefContours, reliefMeta, terrainExtent,
                    out baseAlbedos[i], out contourAlbedos[i]);

            // resolution must be set BEFORE assigning alphamaps
            terrainData.alphamapResolution = resolution;
            terrainData.terrainLayers = terrainLayers;

            Color32[] pixels = tex.GetPixels32();
            float[,,] alphamaps = SplatmapDecoder.ToAlphamaps(pixels, resolution);
            terrainData.SetAlphamaps(0, 0, alphamaps);

            Object.DestroyImmediate(tex);

            // Per-pixel normal (requires Draw Instanced): distant terrain
            // keeps geometric normal detail from the 4097 heightmap instead
            // of vertex-interpolated mush — directly serves swale legibility.
            terrain.drawInstanced = true;
            terrain.basemapDistance = BasemapDistanceM;
            terrain.materialTemplate = CreateTerrainMaterial();

            // Contour toggle: the HUD chip swaps between the two baked
            // variants (both derivatives of the DEM — honest either way).
            var contourToggle = terrainGo.GetComponent<ReliefContourToggle>();
            if (contourToggle == null)
                contourToggle = terrainGo.AddComponent<ReliefContourToggle>();
            contourToggle.terrain = terrain;
            contourToggle.baseAlbedos = baseAlbedos;
            contourToggle.contourAlbedos = contourAlbedos;

            ImportTreesAndFences(dir, terrainGo);

            EditorSceneManager.MarkSceneDirty(terrainGo.scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"LandcoverImporter: imported {resolution}x{resolution} splatmap, " +
                      $"{Layers.Length} terrain layers (tint x relief, contour variant baked) " +
                      "applied to 'Gettysburg Terrain'.");
        }

        // TextAssets must live under Assets, so trees.json/fences.json get
        // copied from data/landcover/ into Assets/Generated/ (idempotent:
        // File.Copy overwrite=true + re-import) and then wired onto
        // VegetationField/FenceField (added to the terrain GameObject if
        // missing) exactly like VegetationPlanter wires treeMaterial.
        static void ImportTreesAndFences(string dataDir, GameObject terrainGo)
        {
            var vegField = terrainGo.GetComponent<VegetationField>();
            if (vegField == null) vegField = terrainGo.AddComponent<VegetationField>();
            vegField.treeMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/UnitMarker.mat");

            var fenceField = terrainGo.GetComponent<FenceField>();
            if (fenceField == null) fenceField = terrainGo.AddComponent<FenceField>();
            fenceField.fenceMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/UnitMarker.mat");

            // the wheat ring reads Field weights straight from the terrain
            // alphamaps this importer just set, so it only needs a material
            var cropField = terrainGo.GetComponent<CropField>();
            if (cropField == null) cropField = terrainGo.AddComponent<CropField>();
            cropField.cropMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/UnitMarker.mat");

            string treesSrc = Path.Combine(dataDir, "trees.json");
            if (File.Exists(treesSrc))
                vegField.treesJson = CopyJsonAsset(treesSrc, "LandcoverTrees.json");
            else
                Debug.LogWarning(
                    $"LandcoverImporter: {treesSrc} not found; VegetationField keeps its " +
                    "legacy fallback groves.");

            string fencesSrc = Path.Combine(dataDir, "fences.json");
            if (File.Exists(fencesSrc))
                fenceField.fencesJson = CopyJsonAsset(fencesSrc, "LandcoverFences.json");
            else
                Debug.LogWarning(
                    $"LandcoverImporter: {fencesSrc} not found; FenceField has no posts to render.");
        }

        static TextAsset CopyJsonAsset(string sourcePath, string assetFileName)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));
            string assetPath = $"{GeneratedDir}/{assetFileName}";
            string destPath = Path.Combine(Application.dataPath, "Generated", assetFileName);
            File.Copy(sourcePath, destPath, overwrite: true);
            AssetDatabase.ImportAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        }

        static Color32[] LoadReliefPixels(string path, int resolution)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path)))
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: failed to decode {path} as a PNG.");
            if (tex.width != resolution || tex.height != resolution)
                throw new System.InvalidOperationException(
                    $"LandcoverImporter: {path} is {tex.width}x{tex.height} but relief.json " +
                    $"says {resolution}; stale bake? rerun `terrain_pipeline.cli relief`.");
            Color32[] pixels = tex.GetPixels32();
            Object.DestroyImmediate(tex);
            return pixels;
        }

        static TerrainLayer CreateTerrainLayer(
            LayerSpec spec, Color32[] relief, Color32[] reliefContours,
            ReliefMetadata reliefMeta, Vector2 terrainExtent,
            out Texture2D baseAlbedo, out Texture2D contourAlbedo)
        {
            // Idempotent re-runs: replace any previous asset with the same name.
            string texAssetPath = $"{GeneratedDir}/Landcover{spec.Name}Tint.asset";
            string contourAssetPath = $"{GeneratedDir}/Landcover{spec.Name}TintContours.asset";
            string layerAssetPath = $"{GeneratedDir}/Landcover{spec.Name}.terrainlayer";

            foreach (string path in new[] { texAssetPath, contourAssetPath, layerAssetPath })
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                    AssetDatabase.DeleteAsset(path);

            baseAlbedo = CreateAlbedoAsset(spec, relief, reliefMeta, texAssetPath);
            contourAlbedo = CreateAlbedoAsset(spec, reliefContours, reliefMeta, contourAssetPath);

            // URP's terrain lit shader has no mask map here, so it reads
            // smoothness straight from the diffuse texture's alpha channel
            // (TerrainLayer.smoothness is only the fallback used when baking
            // a mask map, which we don't have). Opaque alpha=1 was rendering
            // as full-gloss "wet" terrain — ReliefDecoder writes alpha=0 into
            // every albedo texel; TerrainLayer.smoothness/metallic stay 0
            // belt-and-suspenders in case a mask map gets added later.
            var layer = new TerrainLayer
            {
                diffuseTexture = baseAlbedo,
                tileSize = terrainExtent,
                smoothness = 0f,
                metallic = 0f,
            };
            AssetDatabase.CreateAsset(layer, layerAssetPath);

            return layer;
        }

        static Texture2D CreateAlbedoAsset(
            LayerSpec spec, Color32[] reliefPixels, ReliefMetadata reliefMeta, string assetPath)
        {
            Color32[] albedo = ReliefDecoder.ModulatedAlbedo(
                spec.Color, reliefPixels, reliefMeta.resolution,
                reliefMeta.encode_min, reliefMeta.encode_max);
            var tex = new Texture2D(
                reliefMeta.resolution, reliefMeta.resolution, TextureFormat.RGBA32, false);
            tex.SetPixels32(albedo);
            tex.Apply();
            AssetDatabase.CreateAsset(tex, assetPath);
            return tex;
        }

        // The default terrain material lives in the read-only package, so
        // per-pixel normal needs our own material — created as an ASSET,
        // because asset references keep their shader in device builds where
        // runtime-created materials render magenta (the stripping lesson).
        //
        // Phase 4 (HDRP migration): the material is HDRP/TerrainLit now. The
        // two URP tricks this importer bakes survive verbatim: the relief
        // modulation lives in the albedo TEXTURES (shader-independent), and
        // the alpha=0 anti-gloss trick keeps working because HDRP TerrainLit
        // also derives smoothness as albedo.a * _Smoothness{i} when a layer
        // has no mask map (TerrainLit_Splatmap.hlsl, DefaultMask). The
        // per-pixel-normal property/keyword names are identical in HDRP.
        //
        // Recorded decision — the diffuse-slot conflict (plan Task B4): the
        // whole-terrain-tiled relief albedo and a 4-8 m-tiled detail albedo
        // want the same per-layer texture slot. Relief-in-albedo ships now
        // (the swale/dead-ground win, most of the phase's legibility) with
        // per-pixel normal + basemap distance carrying close-range detail.
        // IF ground-type texture still reads flat at soldier zoom after B6's
        // wheat ring, a minimal Terrain Lit variant multiplying one relief
        // sample over detail layers is the single custom-shader buy this
        // phase MAY make — Material asset only, device-verified before
        // merge. Not built speculatively.
        static Material CreateTerrainMaterial()
        {
            const string matPath = GeneratedDir + "/LandcoverTerrain.mat";
            var shader = Shader.Find("HDRP/TerrainLit");
            if (shader == null)
                throw new System.InvalidOperationException(
                    "LandcoverImporter: HDRP/TerrainLit shader not found.");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != shader)
            {
                // pre-Phase-4 generated material (URP): convert in place so
                // the GUID the scene references survives
                mat.shader = shader;
            }
            mat.SetFloat("_EnableInstancedPerPixelNormal", 1f);
            mat.EnableKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
            return mat;
        }
    }
}
