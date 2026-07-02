using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    public static class VegetationPlanter
    {
        [MenuItem("BattleAtlas/Add Vegetation")]
        public static void AddVegetation()
        {
            var terrainGo = GameObject.Find("Gettysburg Terrain");
            if (terrainGo == null)
            {
                Debug.LogWarning(
                    "VegetationPlanter: no 'Gettysburg Terrain' GameObject in the scene; " +
                    "run BattleAtlas/Import Heightmap first. Aborting.");
                return;
            }

            var field = terrainGo.GetComponent<VegetationField>();
            if (field == null)
                field = terrainGo.AddComponent<VegetationField>();

            field.treeMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/UnitMarker.mat");

            EditorSceneManager.MarkSceneDirty(terrainGo.scene);
            Debug.Log("VegetationPlanter: VegetationField wired on 'Gettysburg Terrain'.");
        }
    }
}
