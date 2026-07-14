using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // The July 3 morning slice evidence harness (Culp's Hill; the
    // july3-morning-slice.md authoring). Inert unless the player is
    // launched with -j3mShots; then it captures the timeline, Muhlenberg's
    // opening program, Steuart's charge across Pardee Field, the
    // Spangler's Meadow charge (2nd MA/27th IN, PROVISIONAL under ED-78),
    // the quiet end-state, and the July 3 day panel (now two reconstructed
    // phases) to -j3mOut <dir>, and quits. The DayExpansion3CaptureHarness
    // mold (windowed Development standalone; the owner's editor untouched).
    public class July3MorningCaptureHarness : MonoBehaviour
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

        // Phase clock: t=0 = 04:30 LMT (startTime 16200); 13:00 = 30600.
        static readonly Shot[] Shots =
        {
            // the phase timeline at theater zoom — Culp's Hill from above,
            // ~05:00, both armies in place before the Muhlenberg program
            new Shot("timeline-0500", 1800f,
                new Vector3(5900f, 0f, 5400f), 0f, 45f, 4800f),
            // ~04:32 — Muhlenberg's opening burst (CA-J3M-1's author text:
            // "fired for fifteen minutes without intermission")
            new Shot("muhlenberg-opening", 120f,
                new Vector3(5550f, 0f, 4700f), 20f, 50f, 2200f),
            // ~10:00 — Steuart's/Daniel's charge across Pardee Field
            // (CA-J3M-2 wave 3), the doomed climax of the morning
            new Shot("steuart-pardee-field", 19800f,
                new Vector3(5920f, 0f, 5300f), 340f, 48f, 1600f),
            // ~10:00 — Spangler's Meadow: the 2nd MA/27th IN charge under
            // Mudge (PROVISIONAL, ED-78/ED-64) — a separate ground from
            // Pardee Field, Colgrove's works to Smith's Virginians' line
            new Shot("spangler-meadow", 19800f,
                new Vector3(6100f, 0f, 4900f), 10f, 48f, 1300f),
            // ~13:00 — the quiet end-state: Culp's Hill spent, both armies
            // holding, the phase abutting the afternoon file's 13:00 start
            new Shot("quiet-end-1300", 30600f,
                new Vector3(5900f, 0f, 5400f), 0f, 45f, 4800f),
        };

        string outDir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-j3mShots") < 0)
                return;
            var go = new GameObject("July3MorningCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<July3MorningCaptureHarness>();
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
            outDir = ArgValue("-j3mOut",
                Path.Combine(Application.persistentDataPath, "j3m-shots"));
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
                Debug.LogError("j3m shots: scene has no clock/orbit camera");
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
                yield return Capture($"j3m-{s.Name}.png");
            }

            // the day panel: July 3 now carries TWO reconstructed phases
            // (morning + afternoon) — the loaded (morning) phase lit.
            if (hud != null)
            {
                hud.OpenDayPanel(2);
                yield return Capture("j3m-day-july3-phases.png");
                hud.CloseDayPanel();
            }
            else
            {
                Debug.LogWarning("j3m shots: no AtlasHud — day panel skipped");
            }

            Debug.Log($"j3m shots written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"July3MorningCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
