using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    public static class HeightmapImporter
    {
        // Sand-table style vertical exaggeration. Real Gettysburg relief is ~167 m
        // over 8.5 km — visually near-flat at map scale. Display-only: the raw
        // elevation data stays honest; this scales presentation, like a museum
        // terrain model. Tune to taste.
        const float VerticalExaggeration = 2.5f;

        [MenuItem("BattleAtlas/Import Heightmap")]
        public static void Import()
        {
            // repo layout: <root>/app/Assets and <root>/data/heightmap
            string dir = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../../data/heightmap"));
            byte[] raw = File.ReadAllBytes(Path.Combine(dir, "heightmap.raw"));
            var meta = JsonUtility.FromJson<HeightmapMetadata>(
                File.ReadAllText(Path.Combine(dir, "heightmap.json")));

            // fail loudly on contract drift rather than rendering garbage
            if (meta.row0 != "north")
                throw new System.InvalidOperationException(
                    $"heightmap.json row0 '{meta.row0}' != 'north'; decoder assumes north-first rows");
            if (meta.resolution < 2 || ((meta.resolution - 1) & (meta.resolution - 2)) != 0)
                throw new System.InvalidOperationException(
                    $"heightmap resolution {meta.resolution} is not 2^n+1; Unity would silently clamp it");

            var terrainData = new TerrainData();
            // resolution must be set BEFORE size (setting it resets terrain size)
            terrainData.heightmapResolution = meta.resolution;
            terrainData.size = new Vector3(
                meta.width_m,
                (meta.max_elev_m - meta.min_elev_m) * VerticalExaggeration,
                meta.depth_m);
            terrainData.SetHeights(0, 0, HeightmapDecoder.Decode(raw, meta.resolution));

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));
            // make re-imports idempotent: replace the previous asset and scene object
            const string assetPath = "Assets/Generated/GettysburgTerrain.asset";
            if (AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(terrainData, assetPath);

            var previous = GameObject.Find("Gettysburg Terrain");
            if (previous != null)
                Object.DestroyImmediate(previous);
            var go = Terrain.CreateTerrainGameObject(terrainData);
            go.name = "Gettysburg Terrain";
            var info = go.AddComponent<BattlefieldInfo>();
            info.widthM = meta.width_m;
            info.depthM = meta.depth_m;
            info.minElevM = meta.min_elev_m;
            info.maxElevM = meta.max_elev_m;
            info.originUtmE = meta.origin_utm_e;
            info.originUtmN = meta.origin_utm_n;
            info.verticalExaggeration = VerticalExaggeration;
            AssetDatabase.SaveAssets();
            Debug.Log($"Imported terrain {meta.width_m:F0}m x {meta.depth_m:F0}m, " +
                      $"elevation range {meta.max_elev_m - meta.min_elev_m:F0}m. " +
                      $"Camera pivot hint: ({meta.width_m / 2f:F0}, 0, {meta.depth_m / 2f:F0})");
        }
    }
}
