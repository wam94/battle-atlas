using UnityEngine;

namespace BattleAtlas
{
    // Phase 1 development HUD for Soldier View, in the same IMGUI idiom as
    // TimelineHud (the retained-mode production UI arrives in Phase 11).
    // Outside Soldier View it offers an Enter button whenever the battle
    // clock sits inside a viewpoint's window; inside it offers play/pause,
    // a seek slider over the window, exit, and sync diagnostics (drift in
    // frames, last seek settle latency).
    public class SoldierViewHud : MonoBehaviour
    {
        public SoldierViewPlayer player;
        public ViewpointSet viewpoints;

        void OnGUI()
        {
            if (player == null || player.clock == null || viewpoints?.viewpoints == null)
                return;
            var clock = player.clock;

            if (!player.InSoldierView)
            {
                float y = 10f;
                foreach (var vp in viewpoints.viewpoints)
                {
                    if (!SoldierViewMath.WithinWindow(clock.CurrentTime, vp.t0, vp.t1))
                        continue;
                    if (GUI.Button(new Rect(Screen.width - 330, y, 320, 34),
                            $"Enter Soldier View: {vp.title}"))
                        player.TryEnter(vp);
                    y += 40f;
                }
                return;
            }

            var active = player.Active;
            float panelY = Screen.height - 92;
            GUI.Box(new Rect(10, panelY, Screen.width - 20, 82), GUIContent.none);

            if (GUI.Button(new Rect(20, panelY + 8, 80, 30),
                    clock.Playing ? "Pause" : "Play"))
                player.SetPlaying(!clock.Playing);
            if (GUI.Button(new Rect(110, panelY + 8, 80, 30), "Exit"))
            {
                player.Exit();
                return;
            }

            GUI.Label(new Rect(210, panelY + 12, Screen.width - 240, 24),
                $"{active.title}   battle {ClockMath.FormatClockTime(clock.StartTime, clock.CurrentTime)} " +
                $"(t={clock.CurrentTime:0.00})   video {(player.Video != null ? player.Video.time : 0):0.00}s   " +
                $"drift {player.CurrentDriftFrames:+0.0;-0.0} frames   " +
                $"seek {player.LastSeekLatencyMs:0} ms" +
                (player.SeekInProgress ? "   [settling]" : "") +
                (player.UsingProxyFallback ? "   [proxy fallback]" : ""));

            float t = GUI.HorizontalSlider(
                new Rect(20, panelY + 52, Screen.width - 60, 20),
                clock.CurrentTime, (float)active.t0, (float)active.t1);
            if (!player.SeekInProgress &&
                Mathf.Abs(t - clock.CurrentTime) * active.media.fps > 1f)
                player.Seek(t);
        }
    }
}
