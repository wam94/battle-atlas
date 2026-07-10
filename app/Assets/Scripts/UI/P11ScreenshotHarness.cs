using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Gate P11 evidence harness. Inert unless the player is launched with
    // -p11shots; then it drives the Phase 11 HUD through its states —
    // timeline, unit card + source drawer, entry marker, content warning,
    // Soldier View (with the in-view sources drawer), credits — captures a
    // screenshot of each to -p11out <dir>, and quits. Runs in a windowed
    // Development standalone (scripts/p11-screenshots.sh), the same mold
    // as the Phase 0 BenchmarkHarness, so the owner's editor is untouched.
    public class P11ScreenshotHarness : MonoBehaviour
    {
        string outDir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-p11shots") < 0)
                return;
            var go = new GameObject("P11ScreenshotHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<P11ScreenshotHarness>();
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
            outDir = ArgValue("-p11out",
                Path.Combine(Application.persistentDataPath, "p11-shots"));
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Shot(string name)
        {
            // two frames so the retained UI reflects the state just set
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name));
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
        }

        IEnumerator WaitUntil(Func<bool> done, float timeout, string what)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!done() && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!done()) Debug.LogWarning($"P11 shots: timed out on {what}");
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var director = FindFirstObjectByType<BattleDirector>();
            var hud = FindFirstObjectByType<AtlasHud>();
            var player = FindFirstObjectByType<SoldierViewPlayer>();
            if (clock == null || hud == null || player == null)
            {
                Debug.LogError("P11 shots: scene has no clock/hud/player");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            // 1: the timeline at the step-off phase
            clock.CurrentTime = 7800f;
            yield return Shot("p11-timeline.png");

            // 2: unit card + source drawer on Garnett's brigade
            if (director != null) director.TrySelectUnit("csa-garnett");
            clock.CurrentTime = 8300f;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Shot("p11-unit-drawer.png");
            hud.CloseDrawer();

            // 3: the entry marker inside the hero window
            clock.CurrentTime = 8400f;
            yield return Shot("p11-entry-marker.png");

            // 4: first-entry content warning (fresh state for this run)
            PlayerPrefs.DeleteKey(ContentWarningGate.PrefsKey);
            ViewpointDefinition hero = null;
            if (hud.viewpoints?.viewpoints != null)
                foreach (var vp in hud.viewpoints.viewpoints)
                    if (!vp.development) { hero = vp; break; }
            if (hero != null)
            {
                hud.RequestEnter(hero);
                yield return Shot("p11-content-warning.png");

                // 5: Soldier View playing the production media
                hud.AcknowledgeWarning();
                yield return WaitUntil(
                    () => player.InSoldierView && !hud.Transitioning
                        && !player.SeekInProgress, 30f, "enter");
                yield return new WaitForSecondsRealtime(1f);
                yield return Shot("p11-soldier-view.png");

                // 6: the source drawer from inside the viewpoint
                hud.OpenDrawerForActiveViewpoint();
                yield return Shot("p11-sv-sources.png");
                hud.RequestExit();
                yield return WaitUntil(
                    () => !player.InSoldierView && !hud.Transitioning, 15f, "exit");
            }

            // 7: credits
            hud.OpenCredits();
            yield return Shot("p11-credits.png");
            hud.CloseCredits();

            Debug.Log($"P11 shots written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"P11ScreenshotHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
