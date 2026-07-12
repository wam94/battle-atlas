using System;
using UnityEditor;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    // Cartography-slice batch staging: the FULL fresh-checkout terrain
    // preparation in one headless call — heightmap import AND landcover
    // import (splat + relief-baked layer tints), then save. Phase12Review.
    // PrepareStandaloneScene runs the heightmap step only; re-importing
    // the heightmap rebuilds TerrainData and drops the landcover splat, so
    // a worktree staged with it alone renders the bare-relief terrain the
    // owner has never seen. Evidence captures must show the owner's actual
    // Atlas.
    //
    // Usage (repo root; Unity editor closed for this project path):
    //   "$UNITY" -batchmode -quit -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.CartographyStage.PrepareScene \
    //     -logFile prepare.log
    public static class CartographyStage
    {
        public static void PrepareScene()
        {
            int exitCode = 0;
            try
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                    "Assets/Scenes/Atlas.unity",
                    UnityEditor.SceneManagement.OpenSceneMode.Single);
                HeightmapImporter.Import();
                LandcoverImporter.Import();
                if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("could not save Atlas.unity");
                Debug.Log("CartographyStage: heightmap + landcover imported into "
                    + "Atlas.unity and saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"CartographyStage failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }
    }
}
