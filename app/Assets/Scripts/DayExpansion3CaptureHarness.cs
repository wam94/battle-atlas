using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Day-expansion slice 3 evidence harness. Inert unless the player is
    // launched with -dayexp3shots; then it captures the requested shot SET
    // (-dayexp3set morning|afternoon — matching the July 1 phase file the
    // run loads via -battleFile) to -dayexp3Out <dir>, and quits. The
    // DayExpansion2CaptureHarness mold (windowed Development standalone;
    // the owner's editor untouched).
    public class DayExpansion3CaptureHarness : MonoBehaviour
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

        // Morning phase (t=0 = 07:30 LMT; 13:00 = 19800).
        static readonly Shot[] MorningShots =
        {
            // the phase timeline at theater zoom — day tabs, July 1 lit
            new Shot("timeline-0900", 5400f,
                new Vector3(3600f, 0f, 7400f), 0f, 45f, 5000f),
            // ~09:00 — Buford's delay line: Gamble/Calef astride the pike,
            // Heth deploying on Herr Ridge (CA-J1M-2/4a)
            new Shot("buford-0900", 5400f,
                new Vector3(2900f, 0f, 7600f), 0f, 55f, 2600f),
            // ~10:15 — the Reynolds moment: the Iron Brigade charge at the
            // woods, Cutler's wing across the railroad (CA-J1M-4b/5)
            new Shot("reynolds-1015", 9900f,
                new Vector3(3050f, 0f, 7350f), 90f, 50f, 1900f),
            // ~10:50 — the railroad cut: Davis's trap, the 6th Wis charge
            new Shot("rrcut-1045", 12000f,
                new Vector3(3400f, 0f, 7650f), 0f, 55f, 1500f),
            // ~12:30 — the lull with Rodes arriving on Oak Hill
            new Shot("lull-1230", 18000f,
                new Vector3(4000f, 0f, 8200f), 180f, 50f, 3600f),
        };

        // Afternoon phase (t=0 = 13:00 LMT; 18:00 = 18000).
        static readonly Shot[] AfternoonShots =
        {
            // the afternoon timeline at theater zoom
            new Shot("timeline-1400", 3600f,
                new Vector3(4300f, 0f, 7800f), 0f, 45f, 5200f),
            // ~14:45 — Iverson's field: the dress-parade line before
            // Baxter's wall (CA-J1P-2, the single-spike exemplar)
            new Shot("iverson-1430", 6300f,
                new Vector3(3900f, 0f, 8450f), 270f, 50f, 1800f),
            // ~15:30 — Barlow's Knoll both-sided: Gordon's contact bar vs
            // the Ames crest line, Doles at the seam (CA-J1P-3)
            new Shot("knoll-1530", 9000f,
                new Vector3(5300f, 0f, 8350f), 0f, 50f, 2000f),
            // ~16:15 — the double collapse: Perrin at the seminary,
            // the XI Corps line folding (CA-J1P-4/5)
            new Shot("collapse-1600", 11700f,
                new Vector3(3900f, 0f, 7300f), 90f, 50f, 2800f),
            // ~16:30 — the retreat through town
            new Shot("retreat-1630", 12600f,
                new Vector3(4850f, 0f, 6900f), 180f, 50f, 2400f),
            // ~17:30 — the Cemetery Hill consolidation (CA-J1P-7/8 ground)
            new Shot("cemetery-1730", 16200f,
                new Vector3(5050f, 0f, 5700f), 180f, 50f, 2200f),
        };

        string outDir;
        string set;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-dayexp3shots") < 0)
                return;
            var go = new GameObject("DayExpansion3CaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<DayExpansion3CaptureHarness>();
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
            outDir = ArgValue("-dayexp3Out",
                Path.Combine(Application.persistentDataPath, "dayexp3-shots"));
            set = ArgValue("-dayexp3set", "morning");
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
                Debug.LogError("dayexp3 shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            Shot[] shots = set == "afternoon" ? AfternoonShots : MorningShots;
            foreach (Shot s in shots)
            {
                clock.CurrentTime = s.T;
                orbit.followPivot = null;
                orbit.pivot = s.Pivot;
                orbit.yawDeg = s.Yaw;
                orbit.pitchDeg = s.Pitch;
                orbit.distance = s.Dist;
                yield return new WaitForSecondsRealtime(1.0f);
                yield return Capture($"dayexp3-{set}-{s.Name}.png");
            }

            // the day panel: July 1 with its three phases — two
            // reconstructed (the loaded one lit) + the honest evening note.
            // With slice 3, ALL THREE DAYS carry reconstructed phases.
            if (hud != null)
            {
                hud.OpenDayPanel(0);
                yield return Capture($"dayexp3-{set}-day-july1-phases.png");
                hud.CloseDayPanel();
            }
            else
            {
                Debug.LogWarning("dayexp3 shots: no AtlasHud — day panel skipped");
            }

            Debug.Log($"dayexp3 shots ({set}) written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"DayExpansion3CaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
