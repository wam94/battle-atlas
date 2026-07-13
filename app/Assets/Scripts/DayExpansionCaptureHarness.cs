using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Day-expansion slice 1 evidence harness. Inert unless the player is
    // launched with -dayexp1shots; then it captures the widened timeline
    // with the day tabs, the sunset window's new content (the 16:00 quiet,
    // the ~17:30 Wheatfield sweep + Benning's retirement, Taft's evening
    // fire, the 19:29 sunset light), and the honest empty-day states, to
    // -dayexp1Out <dir>, and quits. The CartographyCaptureHarness mold
    // (windowed Development standalone; the owner's editor untouched).
    public class DayExpansionCaptureHarness : MonoBehaviour
    {
        struct Shot
        {
            public string Name;
            public float T;
            public Vector3 Pivot;
            public float Yaw, Pitch, Dist;

            public Shot(string name, float t, Vector3 pivot, float yaw,
                float pitch, float dist)
            {
                Name = name; T = t; Pivot = pivot;
                Yaw = yaw; Pitch = pitch; Dist = dist;
            }
        }

        static readonly Shot[] Shots =
        {
            // the widened timeline: the hero window band now sits at ~35%
            // of a 13:00–19:29 bar, day tabs in the masthead
            new Shot("timeline-charge-1520", 8400f,
                new Vector3(4254f, 0f, 4254f), 0f, 45f, 4000f),
            // 16:00 — the field falls quiet; the column back on the ridge
            new Shot("quiet-1600", 10800f,
                new Vector3(3700f, 0f, 4900f), 0f, 55f, 2600f),
            // ~17:30 — the Wheatfield sweep at the front, Benning's brigade
            // retiring off the Den, the reformed CSA lines on the ridge
            new Shot("sweep-1730", 16200f,
                new Vector3(3700f, 0f, 2700f), 0f, 55f, 2600f),
            new Shot("ridge-rally-1730", 16200f,
                new Vector3(3250f, 0f, 5100f), 0f, 55f, 2600f),
            // ~17:45 — Taft still firing from the Baltimore Pike (his
            // report's ~6 P.M. cease; the marker said 4 P.M.)
            new Shot("taft-fire-1745", 17100f,
                new Vector3(5010f, 0f, 5600f), 0f, 50f, 1600f),
            // sunset 19:29 LMT (ED-31): the phase's last second, low sun
            new Shot("sunset-1929", 23340f,
                new Vector3(4254f, 0f, 4600f), 0f, 55f, 7000f),
        };

        string outDir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-dayexp1shots") < 0)
                return;
            var go = new GameObject("DayExpansionCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<DayExpansionCaptureHarness>();
        }

        static string ArgValue(string name, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : fallback;
        }

        void Start()
        {
            Application.runInBackground = true;
            outDir = ArgValue("-dayexp1Out",
                Path.Combine(Application.persistentDataPath, "dayexp1-shots"));
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Capture(string name)
        {
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name));
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var orbit = FindFirstObjectByType<OrbitCameraController>();
            var hud = FindFirstObjectByType<AtlasHud>();
            if (clock == null || orbit == null)
            {
                Debug.LogError("dayexp1 shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            foreach (Shot s in Shots)
            {
                clock.CurrentTime = s.T;
                orbit.followPivot = null;
                orbit.pivot = s.Pivot;
                orbit.yawDeg = s.Yaw;
                orbit.pitchDeg = s.Pitch;
                orbit.distance = s.Dist;
                yield return new WaitForSecondsRealtime(1.0f);
                yield return Capture($"dayexp1-{s.Name}.png");
            }

            // the honest empty states: July 1 (empty day) and July 3 (the
            // phase list — morning not reconstructed, afternoon active)
            if (hud != null)
            {
                clock.CurrentTime = 8400f;
                hud.OpenDayPanel(0);
                yield return Capture("dayexp1-day-july1-empty.png");
                hud.CloseDayPanel();
                hud.OpenDayPanel(2);
                yield return Capture("dayexp1-day-july3-phases.png");
                hud.CloseDayPanel();
            }
            else
            {
                Debug.LogWarning("dayexp1 shots: no AtlasHud — day panels skipped");
            }

            Debug.Log($"dayexp1 shots written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"DayExpansionCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
