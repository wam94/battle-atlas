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
        const int TintTextureSize = 16;
        const float TileSizeM = 50f;

        // Muted, sand-table palette (docs/superpowers/plans/2026-07-01-landcover.md).
        struct LayerSpec
        {
            public string Name;
            public Color Color;
            public LayerSpec(string name, Color color) { Name = name; Color = color; }
        }

        static readonly LayerSpec[] Layers = new[]
        {
            new LayerSpec("Pasture", HexColor("#8FA05A")),
            new LayerSpec("Field", HexColor("#C9B36A")),
            new LayerSpec("Woods", HexColor("#4C6340")),
            new LayerSpec("Orchard", HexColor("#7DA05A")),
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

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));

            var terrainLayers = new TerrainLayer[Layers.Length];
            for (int i = 0; i < Layers.Length; i++)
                terrainLayers[i] = CreateTerrainLayer(Layers[i]);

            var terrainData = terrain.terrainData;
            // resolution must be set BEFORE assigning alphamaps
            terrainData.alphamapResolution = resolution;
            terrainData.terrainLayers = terrainLayers;

            Color32[] pixels = tex.GetPixels32();
            float[,,] alphamaps = SplatmapDecoder.ToAlphamaps(pixels, resolution);
            terrainData.SetAlphamaps(0, 0, alphamaps);

            Object.DestroyImmediate(tex);

            ImportTreesAndFences(dir, terrainGo);

            EditorSceneManager.MarkSceneDirty(terrainGo.scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"LandcoverImporter: imported {resolution}x{resolution} splatmap, " +
                      $"{Layers.Length} terrain layers applied to 'Gettysburg Terrain'.");
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

        static TerrainLayer CreateTerrainLayer(LayerSpec spec)
        {
            // Idempotent re-runs: replace any previous asset with the same name.
            string texAssetPath = $"{GeneratedDir}/Landcover{spec.Name}Tint.asset";
            string layerAssetPath = $"{GeneratedDir}/Landcover{spec.Name}.terrainlayer";

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(texAssetPath) != null)
                AssetDatabase.DeleteAsset(texAssetPath);
            if (AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerAssetPath) != null)
                AssetDatabase.DeleteAsset(layerAssetPath);

            // URP's terrain lit shader has no mask map here, so it reads
            // smoothness straight from the diffuse texture's alpha channel
            // (TerrainLayer.smoothness is only the fallback used when baking
            // a mask map, which we don't have). Opaque alpha=1 was rendering
            // as full-gloss "wet" terrain. Zero the tint alpha to kill that,
            // and also zero TerrainLayer.smoothness/metallic belt-and-suspenders
            // in case a mask map gets added later.
            var tex = new Texture2D(TintTextureSize, TintTextureSize, TextureFormat.RGBA32, false);
            var fill = new Color32[TintTextureSize * TintTextureSize];
            Color32 c = spec.Color;
            c.a = 0;
            for (int i = 0; i < fill.Length; i++)
                fill[i] = c;
            tex.SetPixels32(fill);
            tex.Apply();
            AssetDatabase.CreateAsset(tex, texAssetPath);

            var layer = new TerrainLayer
            {
                diffuseTexture = tex,
                tileSize = new Vector2(TileSizeM, TileSizeM),
                smoothness = 0f,
                metallic = 0f,
            };
            AssetDatabase.CreateAsset(layer, layerAssetPath);

            return layer;
        }
    }
}
