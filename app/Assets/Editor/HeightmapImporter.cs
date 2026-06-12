using System.IO;
using UnityEditor;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    public static class HeightmapImporter
    {
        [MenuItem("BattleAtlas/Import Heightmap")]
        public static void Import()
        {
            // repo layout: <root>/app/Assets and <root>/data/heightmap
            string dir = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../../data/heightmap"));
            byte[] raw = File.ReadAllBytes(Path.Combine(dir, "heightmap.raw"));
            var meta = JsonUtility.FromJson<HeightmapMetadata>(
                File.ReadAllText(Path.Combine(dir, "heightmap.json")));

            var terrainData = new TerrainData();
            // resolution must be set BEFORE size (setting it resets terrain size)
            terrainData.heightmapResolution = meta.resolution;
            terrainData.size = new Vector3(
                meta.width_m, meta.max_elev_m - meta.min_elev_m, meta.depth_m);
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
            AssetDatabase.SaveAssets();
            Debug.Log($"Imported terrain {meta.width_m:F0}m x {meta.depth_m:F0}m, " +
                      $"elevation range {meta.max_elev_m - meta.min_elev_m:F0}m. " +
                      $"Camera pivot hint: ({meta.width_m / 2f:F0}, 0, {meta.depth_m / 2f:F0})");
        }
    }
}
