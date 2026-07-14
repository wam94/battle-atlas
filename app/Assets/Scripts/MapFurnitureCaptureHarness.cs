using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Map-furniture slice evidence harness. Inert unless the player is
    // launched with -mapfurnshots; then it drives the orbit camera through
    // fixed altitude presets centered on the Diamond (where every traced
    // road spoke, the town blocks, and Stevens Run/Rock Creek converge —
    // the single view that most exercises the new layer), captures a
    // screenshot of each to -mapfurnOut <dir>, then quits. Same mold as
    // CartographyCaptureHarness/RtsCameraCaptureHarness: a windowed
    // Development standalone, the owner's editor untouched, deterministic
    // poses set directly through pivot/yawDeg/pitchDeg/distance.
    //
    // Before/after evidence: this harness's SCENE-STATE dependency is the
    // Atlas.unity scene's saved state, not the harness itself -- the same
    // binary captures a clean "before" when MapFurnitureImporter has not
    // been run (no roads/streams/town in the saved scene) and an "after"
    // once it has (docs/reconstruction/map-furniture-slice.md records the
    // exact staging commands used for each pass).
    public class MapFurnitureCaptureHarness : MonoBehaviour
    {
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

        // Diamond = manifest town-diamond tie point (4875.8, 6835.9): every
        // traced road spoke, the railroad, and the town blocks converge
        // here, so it is the single pose location that most exercises the
        // new layer at every altitude. rts-low reuses RtsCameraCaptureHarness's
        // proven floor (pitch 35, distance 40) for a directly comparable
        // low-altitude read.
        public static readonly Preset[] Presets =
        {
            new Preset("theater", new Vector3(4254f, 0f, 4600f), 0f, 55f, 7000f),
            new Preset("mid", new Vector3(4876f, 0f, 6836f), 20f, 50f, 2200f),
            new Preset("tactical", new Vector3(4876f, 0f, 6836f), 20f, 45f, 700f),
            new Preset("rts-low", new Vector3(4876f, 0f, 6836f), 20f, 35f, 40f),
        };

        string outDir;
        string prefix;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-mapfurnshots") < 0)
                return;
            var go = new GameObject("MapFurnitureCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<MapFurnitureCaptureHarness>();
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
            outDir = ArgValue("-mapfurnOut",
                Path.Combine(Application.persistentDataPath, "map-furniture-shots"));
            prefix = ArgValue("-mapfurnPrefix", "mapfurn");
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var orbit = FindFirstObjectByType<OrbitCameraController>();
            if (clock == null || orbit == null)
            {
                Debug.LogError("map-furniture shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;
            clock.CurrentTime = 8400f; // Pickett's Charge, inside the shipped window

            foreach (Preset p in Presets)
            {
                orbit.followPivot = null;
                orbit.pivot = p.Pivot;
                orbit.yawDeg = p.YawDeg;
                orbit.pitchDeg = p.PitchDeg;
                orbit.distance = p.Distance;
                yield return new WaitForSecondsRealtime(0.6f);
                yield return null;
                yield return null;

                string shot = Path.Combine(outDir, $"{prefix}-{p.Name}.png");
                ScreenCapture.CaptureScreenshot(shot);
                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.3f);
            }
            Debug.Log($"map-furniture shots written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"MapFurnitureCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
