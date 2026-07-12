using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Cartography-slice evidence harness. Inert unless the player is
    // launched with -cartoshots; then it drives the orbit camera through a
    // fixed set of altitude presets (theater / owner-P11-default / mid /
    // tactical) at two battle times (bombardment peak t=5400, charge
    // t=8400), captures a screenshot of each to -cartoOut <dir>, and
    // quits. The SAME presets run against the before and after builds, so
    // the owner compares like against like — the BenchmarkHarness mold
    // (windowed Development standalone, the owner's editor untouched).
    public class CartographyCaptureHarness : MonoBehaviour
    {
        // One camera pose per legibility band the slice must prove:
        //   default  — the committed scene pose (pivot 4254/4254, pitch 45,
        //              dist 4000): the angle the owner's P11/wave reviews
        //              actually saw.
        //   theater  — whole-field read, the corps-label band.
        //   mid      — the charge corridor at brigade grain.
        //   tactical — inside the Soldiers/Regiments boundary at the Angle,
        //              where formation state and battery glyphs must read.
        public readonly struct Preset
        {
            public readonly string Name;
            public readonly Vector3 Pivot;
            public readonly float YawDeg;
            public readonly float PitchDeg;
            public readonly float Distance;

            public Preset(string name, Vector3 pivot, float yawDeg,
                float pitchDeg, float distance)
            {
                Name = name;
                Pivot = pivot;
                YawDeg = yawDeg;
                PitchDeg = pitchDeg;
                Distance = distance;
            }
        }

        public static readonly Preset[] Presets =
        {
            new Preset("default", new Vector3(4254f, 0f, 4254f), 0f, 45f, 4000f),
            new Preset("theater", new Vector3(4254f, 0f, 4600f), 0f, 55f, 7000f),
            // the charge corridor: between the Emmitsburg Road crossing and
            // the Union wall (csa-garnett track ~x3200..4400 @ z~4850)
            new Preset("mid", new Vector3(3900f, 0f, 4850f), 0f, 55f, 2200f),
            new Preset("tactical", new Vector3(4405f, 0f, 4852f), 25f, 60f, 1100f),
        };

        public static readonly float[] Times = { 5400f, 8400f };

        string outDir;
        string prefix;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-cartoshots") < 0)
                return;
            var go = new GameObject("CartographyCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<CartographyCaptureHarness>();
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
            outDir = ArgValue("-cartoOut",
                Path.Combine(Application.persistentDataPath, "carto-shots"));
            prefix = ArgValue("-cartoPrefix", "carto");
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var orbit = FindFirstObjectByType<OrbitCameraController>();
            if (clock == null || orbit == null)
            {
                Debug.LogError("carto shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            foreach (float t in Times)
            {
                clock.CurrentTime = t;
                foreach (Preset p in Presets)
                {
                    orbit.followPivot = null;
                    orbit.pivot = p.Pivot;
                    orbit.yawDeg = p.YawDeg;
                    orbit.pitchDeg = p.PitchDeg;
                    orbit.distance = p.Distance;
                    // renderers pose from the clock and camera every frame;
                    // give label declutter a beat to settle its sticky state
                    yield return new WaitForSecondsRealtime(1.0f);
                    string shot = Path.Combine(outDir,
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "{0}-{1}-t{2:0}.png", prefix, p.Name, t));
                    ScreenCapture.CaptureScreenshot(shot);
                    yield return null;
                    yield return null;
                    yield return new WaitForSecondsRealtime(0.3f);
                }
            }
            Debug.Log($"carto shots written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"CartographyCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
