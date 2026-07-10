using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Pins the Phase 11 UI Toolkit assets the runtime bootstrap loads from
// Resources (the HdrpMigrationTests wiring pattern): panel settings, the
// visual tree, the stylesheet, and the theme — and that the retired IMGUI
// TimelineHud is actually gone from the Atlas scene file.
public class AtlasUiAssetTests
{
    [Test]
    public void PanelSettings_ExistsAndWiredToTheme()
    {
        var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(
            "Assets/Resources/UI/AtlasPanelSettings.asset");
        Assert.IsNotNull(settings, "AtlasPanelSettings.asset missing — run "
            + "BattleAtlas/Setup Atlas UI Panel Settings");
        Assert.IsNotNull(settings.themeStyleSheet, "panel has no theme");
        Assert.AreEqual(PanelScaleMode.ConstantPhysicalSize, settings.scaleMode);
    }

    [Test]
    public void VisualTree_CarriesThePlanSection10Surface()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Resources/UI/AtlasHud.uxml");
        Assert.IsNotNull(tree, "AtlasHud.uxml missing");
        VisualElement root = tree.Instantiate();
        // every element AtlasHud.Query() binds must exist in the tree
        string[] required =
        {
            "masthead", "context-title", "context-phase", "context-conditions",
            "chips", "chip-contours", "chip-reading-light", "chip-credits",
            "entry-markers", "unit-drawer", "drawer-title", "drawer-close",
            "drawer-identity", "drawer-strength", "drawer-activity",
            "drawer-confidence", "drawer-follow", "drawer-sources",
            "atlas-bar", "play-button", "speed-group", "clock-label",
            "marker-tip", "timeline-slider", "timeline-markers",
            "timeline-window-band", "sv-bar", "sv-play", "sv-speed",
            "sv-clock", "sv-title",
            "sv-proxy-badge", "sv-settling", "sv-sources", "sv-exit",
            "sv-slider", "sv-observer-line", "modal-layer", "warning-modal",
            "warning-title", "warning-body", "observer-title", "observer-body",
            "warning-acknowledge", "warning-decline", "credits-modal",
            "credits-note", "credits-list", "credits-close", "fade-overlay",
            // Phase 12 accessibility surface (§12 P12)
            "chip-options", "options-modal", "opt-master", "opt-sv-volume",
            "opt-captions", "opt-reduced-motion", "opt-motion-note",
            "options-close", "sv-captions-toggle", "sv-captions",
            "sv-motion-note",
        };
        foreach (string name in required)
            Assert.IsNotNull(root.Q(name), $"AtlasHud.uxml missing '{name}'");
    }

    [Test]
    public void StyleSheet_Exists()
    {
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Resources/UI/AtlasHud.uss"), "AtlasHud.uss missing");
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
            "Assets/Resources/UI/BattleAtlasTheme.tss"), "theme tss missing");
    }

    [Test]
    public void StyleSheet_HonorsTheReadableTextFloor()
    {
        // Phase 12 accessibility (§12 P12 "readable text"): no HUD text
        // below 12px at the panel's ConstantPhysicalSize/160dpi scale.
        // Pinned on the source text so a future style tweak cannot quietly
        // reintroduce 10-11px labels.
        string uss = System.IO.File.ReadAllText(
            UnityEngine.Application.dataPath + "/Resources/UI/AtlasHud.uss");
        foreach (System.Text.RegularExpressions.Match m in
            System.Text.RegularExpressions.Regex.Matches(
                uss, @"font-size:\s*(\d+)px"))
        {
            Assert.GreaterOrEqual(int.Parse(m.Groups[1].Value), 12,
                $"'{m.Value}' is below the 12px readability floor");
        }
    }

    [Test]
    public void AtlasScene_NoLongerReferencesTimelineHud()
    {
        // the IMGUI placeholder's component was removed by the one-shot
        // scene surgery; the class is deleted, so any lingering reference
        // would be a missing-script component. The scene text carries no
        // stale m_Script guid because the surgery saved a clean file — pin
        // the absence of ANY missing script on the scene's root objects.
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
            "Assets/Scenes/Atlas.unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);
        try
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    Assert.AreEqual(0,
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            t.gameObject),
                        $"missing script on '{t.name}' — stale TimelineHud reference?");
                }
            }
        }
        finally
        {
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }
    }
}
