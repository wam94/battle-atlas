using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BattleAtlas.EditorTools
{
    // Phase 11 one-shot setup, runnable headlessly:
    //
    //   "$UNITY" -batchmode -quit -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.AtlasUiSetup.CreatePanelSettings
    //
    // creates the committed PanelSettings asset the runtime bootstrap loads
    // (Resources/UI/AtlasPanelSettings) wired to the default runtime theme.
    // ConstantPhysicalSize keeps the HUD readable across retina scales —
    // the same dpi/160 intent as the retired IMGUI HudScale.
    public static class AtlasUiSetup
    {
        const string ThemePath = "Assets/Resources/UI/BattleAtlasTheme.tss";
        const string PanelPath = "Assets/Resources/UI/AtlasPanelSettings.asset";

        [MenuItem("BattleAtlas/Setup Atlas UI Panel Settings")]
        public static void CreatePanelSettings()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            if (theme == null)
            {
                Debug.LogError($"AtlasUiSetup: theme missing at {ThemePath}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, PanelPath);
            }
            settings.themeStyleSheet = theme;
            settings.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            settings.referenceDpi = 160f;
            settings.fallbackDpi = 160f;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"AtlasUiSetup: wrote {PanelPath}");
        }

        // The world-space unit labels (UnitLabelField, cartography slice)
        // are TextMeshPro components; TMP requires its Essential Resources
        // (TMP Settings + default font) imported into Assets/ or every
        // label NREs at runtime — observed in the Phase 11 standalone
        // screenshot run, and the same would happen in the owner's editor.
        // One-shot (editor menu). NOTE: AssetDatabase.ImportPackage is
        // asynchronous — under -batchmode -quit the editor exits before the
        // import lands, so the committed Assets/TextMesh Pro tree was
        // extracted directly from the .unitypackage (tar.gz) instead; this
        // menu item remains for interactive editor use.
        // Licensing: the package is Unity Companion License content; the
        // bundled LiberationSans font is SIL OFL 1.1 (license text ships
        // inside the imported folder). Flagged in the P11 gate evidence
        // for the Phase 12 license review.
        [MenuItem("BattleAtlas/Import TMP Essential Resources")]
        public static void ImportTmpEssentials()
        {
            const string dst = "Assets/TextMesh Pro";
            if (System.IO.Directory.Exists(dst))
            {
                Debug.Log("AtlasUiSetup: TMP essentials already imported");
                return;
            }
            string pkg = System.IO.Path.GetFullPath(
                "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");
            if (!System.IO.File.Exists(pkg))
            {
                Debug.LogError($"AtlasUiSetup: package not found at {pkg}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            AssetDatabase.ImportPackage(pkg, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"AtlasUiSetup: imported TMP essentials from {pkg}");
        }

        // (A one-shot RemoveTimelineHudFromAtlas surgery method lived here
        // until it ran on 2026-07-10: it deleted the retired IMGUI
        // TimelineHud component from Atlas.unity, and was removed together
        // with the TimelineHud class itself.)
    }
}
