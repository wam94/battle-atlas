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

        // (A one-shot RemoveTimelineHudFromAtlas surgery method lived here
        // until it ran on 2026-07-10: it deleted the retired IMGUI
        // TimelineHud component from Atlas.unity, and was removed together
        // with the TimelineHud class itself.)
    }
}
