using System;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Wires Soldier View into the running Atlas scene without editing the
    // committed scene file: at scene load, if a BattleClock exists and
    // StreamingAssets/SoldierView/viewpoints.json parses, a SoldierView
    // GameObject (player + dev HUD) is created. Scenes without a clock
    // (e.g. PlayMode test scenes, which build their own rig) are untouched,
    // as are -benchmark runs (Phase 0 baselines must show pre-V2 Atlas).
    public static class SoldierViewBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-benchmark") >= 0)
                return;
            var clock = UnityEngine.Object.FindFirstObjectByType<BattleClock>();
            if (clock == null) return;

            string path = SoldierViewPlayer.MediaPath("SoldierView/viewpoints.json");
            if (!File.Exists(path)) return;

            ViewpointSet set;
            try
            {
                set = ViewpointSet.FromJson(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SoldierView: viewpoints.json rejected — {e.Message}");
                return;
            }

            var go = new GameObject("SoldierView");
            var player = go.AddComponent<SoldierViewPlayer>();
            player.clock = clock;
            var hud = go.AddComponent<SoldierViewHud>();
            hud.player = player;
            hud.viewpoints = set;
        }
    }
}
