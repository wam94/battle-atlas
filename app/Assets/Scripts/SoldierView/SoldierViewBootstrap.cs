using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace BattleAtlas
{
    // Wires the Phase 11 UI into the running Atlas scene without editing
    // the committed scene file: at scene load, if a BattleClock exists, an
    // AtlasUI GameObject is created carrying the SoldierViewPlayer (the
    // media contract), the UIDocument (panel settings + tree from
    // Resources/UI), and the AtlasHud controller. Scenes without a clock
    // (e.g. PlayMode test scenes, which build their own rig) are untouched,
    // as are -benchmark runs (Phase 0 baselines must show pre-V2 Atlas).
    //
    // Degradation is graceful and loud: a missing viewpoints.json means an
    // Atlas with no Soldier View entry (warning logged); missing UI assets
    // mean no HUD (warning logged) but the scene still renders.
    public static class SoldierViewBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            // statics survive scene loads; nobody starts inside Soldier View
            AcousticField.SoldierViewActive = false;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-benchmark") >= 0)
                return;
            var clock = UnityEngine.Object.FindFirstObjectByType<BattleClock>();
            if (clock == null) return;

            ViewpointSet set = null;
            string path = SoldierViewPlayer.MediaPath("SoldierView/viewpoints.json");
            if (File.Exists(path))
            {
                try
                {
                    set = ViewpointSet.FromJson(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"SoldierView: viewpoints.json rejected — {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning(
                    "SoldierView: viewpoints.json missing — no Soldier View entry");
            }

            // The shipped viewpoints/media address ONE phase's clock and
            // cast (the scene's serialized battle asset — July 3
            // afternoon). The set is wired unconditionally; AtlasHud gates
            // entry PER PHASE (HudModel.ViewpointsApplyTo), so a
            // -battleFile launch hides entry and an in-HUD switch back to
            // the home phase restores it — the day-expansion slice 2
            // blanket disable is superseded by that per-phase gate.

            var go = new GameObject("AtlasUI");
            var player = go.AddComponent<SoldierViewPlayer>();
            player.clock = clock;

            var panelSettings = Resources.Load<PanelSettings>("UI/AtlasPanelSettings");
            var tree = Resources.Load<VisualTreeAsset>("UI/AtlasHud");
            if (panelSettings == null || tree == null)
            {
                Debug.LogWarning(
                    "AtlasHud: Resources/UI assets missing (AtlasPanelSettings, "
                    + "AtlasHud.uxml) — HUD not created");
                return;
            }
            var document = go.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = tree;

            var hud = go.AddComponent<AtlasHud>();
            hud.clock = clock;
            hud.player = player;
            hud.viewpoints = set;
            // director/sun/contours/orbit resolve in AtlasHud.Start via
            // FindFirstObjectByType — the scene-surgery-tolerant idiom
        }
    }
}
