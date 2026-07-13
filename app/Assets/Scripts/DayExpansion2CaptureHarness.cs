using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Day-expansion slice 2 evidence harness. Inert unless the player is
    // launched with -dayexp2shots; then it captures the requested shot SET
    // (-dayexp2set afternoon|evening — matching the July 2 phase file the
    // run loads via -battleFile) to -dayexp2Out <dir>, and quits. The
    // DayExpansionCaptureHarness mold (windowed Development standalone;
    // the owner's editor untouched).
    public class DayExpansion2CaptureHarness : MonoBehaviour
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

        // Afternoon phase (t=0 = 15:30 LMT; sunset 19:29 = 14340).
        static readonly Shot[] AfternoonShots =
        {
            // the phase timeline at theater zoom — day tabs, July 2 lit
            new Shot("timeline-1700", 5400f,
                new Vector3(4000f, 0f, 3800f), 0f, 45f, 5000f),
            // ~17:00 — the en-echelon assault's first rung: Law/Robertson
            // into the Round Tops' valley, Ward's ridge, the Wheatfield W1
            new Shot("enechelon-1700", 5400f,
                new Vector3(3700f, 0f, 2600f), 0f, 55f, 2800f),
            // ~17:30 — Little Round Top: Vincent's spur, Weed arriving,
            // the Den falling (ED-56's hands-change minute)
            new Shot("lrt-1730", 7200f,
                new Vector3(4150f, 0f, 2450f), 90f, 50f, 1800f),
            // ~18:00 — the Wheatfield at wave 3: Caldwell's counterattack
            new Shot("wheatfield-1800", 9000f,
                new Vector3(3650f, 0f, 3150f), 0f, 55f, 2200f),
            // ~18:30 — Barksdale's breakthrough at the Peach Orchard
            new Shot("barksdale-1830", 10800f,
                new Vector3(3500f, 0f, 3850f), 0f, 55f, 2400f),
            // ~19:10 — the Plum Run crisis: Wilcox vs the 1st MN, Willard
            // vs Barksdale, Wright at the gun line
            new Shot("plumrun-1910", 13200f,
                new Vector3(4150f, 0f, 4400f), 0f, 55f, 2600f),
        };

        // Evening phase (t=0 = 19:29 LMT sunset; 22:30 = 10860).
        static readonly Shot[] EveningShots =
        {
            // the evening timeline at theater zoom in twilight
            new Shot("timeline-1945", 960f,
                new Vector3(5400f, 0f, 5400f), 0f, 45f, 5000f),
            // ~20:00 — the East Cemetery Hill assault at the lodgment
            new Shot("ech-2000", 1860f,
                new Vector3(5150f, 0f, 5950f), 180f, 50f, 1700f),
            // ~20:00 — Culp's Hill: Greene's works, Steuart in the vacated
            // line, Johnson's brigades on the east face
            new Shot("culp-2000", 1860f,
                new Vector3(5950f, 0f, 5500f), 90f, 50f, 2000f),
            // ~21:00 — the recoveries and returns (Trostle haul-off,
            // Ruger's 9 P.M. column)
            new Shot("returns-2100", 5460f,
                new Vector3(5300f, 0f, 4500f), 0f, 55f, 3200f),
        };

        string outDir;
        string set;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-dayexp2shots") < 0)
                return;
            var go = new GameObject("DayExpansion2CaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<DayExpansion2CaptureHarness>();
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
            outDir = ArgValue("-dayexp2Out",
                Path.Combine(Application.persistentDataPath, "dayexp2-shots"));
            set = ArgValue("-dayexp2set", "afternoon");
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
                Debug.LogError("dayexp2 shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            Shot[] shots = set == "evening" ? EveningShots : AfternoonShots;
            foreach (Shot s in shots)
            {
                clock.CurrentTime = s.T;
                orbit.followPivot = null;
                orbit.pivot = s.Pivot;
                orbit.yawDeg = s.Yaw;
                orbit.pitchDeg = s.Pitch;
                orbit.distance = s.Dist;
                yield return new WaitForSecondsRealtime(1.0f);
                yield return Capture($"dayexp2-{set}-{s.Name}.png");
            }

            // the day panel: July 2 with its three phases — the honest
            // morning note + two reconstructed phases (the loaded one lit)
            if (hud != null)
            {
                hud.OpenDayPanel(1);
                yield return Capture($"dayexp2-{set}-day-july2-phases.png");
                hud.CloseDayPanel();
            }
            else
            {
                Debug.LogWarning("dayexp2 shots: no AtlasHud — day panel skipped");
            }

            Debug.Log($"dayexp2 shots ({set}) written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"DayExpansion2CaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
