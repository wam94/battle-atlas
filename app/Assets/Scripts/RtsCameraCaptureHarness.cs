using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // RTS camera slice evidence harness. Inert unless the player is
    // launched with -rtscamshots; then it drives the orbit camera through
    // a scripted flight strip — theater altitude, a zoom toward the Angle
    // wall crossing down to the 5 m floor, a rotate at low altitude, a pan
    // along the Union line, a terrain-clearance shot on the Culp's Hill
    // slope, and a dedicated dynamic-near-clip-floor shot — captures a
    // screenshot per pose plus a JSON log of the poses flown, to
    // -rtscamOut <dir>, then quits. The CartographyCaptureHarness mold
    // (windowed Development standalone; the owner's editor untouched;
    // poses set directly through the pivot/yawDeg/pitchDeg/distance
    // snap-set properties — the same deterministic posing every existing
    // capture harness uses).
    public class RtsCameraCaptureHarness : MonoBehaviour
    {
        public readonly struct Pose
        {
            public readonly string Name;
            public readonly Vector3 Pivot;
            public readonly float YawDeg;
            public readonly float PitchDeg;
            public readonly float Distance;
            public readonly string Note;

            public Pose(string name, Vector3 pivot, float yawDeg,
                float pitchDeg, float distance, string note)
            {
                Name = name;
                Pivot = pivot;
                YawDeg = yawDeg;
                PitchDeg = pitchDeg;
                Distance = distance;
                Note = note;
            }
        }

        // The flight strip. Coordinates reuse the prior evidence
        // harnesses' proven views (the Angle wall crossing ~4410/4855,
        // Culp's Hill ~5950/5500) so this run is directly comparable to
        // the cartography/phase-switch capture sets.
        public static readonly Pose[] Strip =
        {
            new Pose("01-theater", new Vector3(4254f, 0f, 4600f), 0f, 55f, 7000f,
                "whole-field read, the corps-label band"),
            new Pose("02-zoom-mid", new Vector3(4415f, 0f, 4855f), 250f, 45f, 700f,
                "closing on the Angle wall crossing"),
            new Pose("03-zoom-tactical", new Vector3(4415f, 0f, 4855f), 250f, 40f, 120f,
                "inside the Soldiers/Regiments boundary"),
            new Pose("04-zoom-floor-5m", new Vector3(4415f, 0f, 4855f), 250f, 35f, 5f,
                "the 5 m distance floor over the wall — descent curve caps pitch "
                + "at 35 degrees here (Total War oblique tilt)"),
            new Pose("05-rotate-low-yaw000", new Vector3(4415f, 0f, 4855f), 0f, 35f, 40f,
                "low-altitude rotate, yaw 0"),
            new Pose("06-rotate-low-yaw090", new Vector3(4415f, 0f, 4855f), 90f, 35f, 40f,
                "low-altitude rotate, yaw 90"),
            new Pose("07-rotate-low-yaw180", new Vector3(4415f, 0f, 4855f), 180f, 35f, 40f,
                "low-altitude rotate, yaw 180"),
            new Pose("08-pan-start", new Vector3(3900f, 0f, 4850f), 0f, 50f, 900f,
                "pan sequence start — the charge corridor"),
            new Pose("09-pan-mid", new Vector3(4415f, 0f, 4855f), 0f, 50f, 900f,
                "pan sequence — the Angle"),
            new Pose("10-pan-end", new Vector3(4900f, 0f, 4900f), 0f, 50f, 900f,
                "pan sequence end — the Union right of the wall"),
            new Pose("11-slope-clearance", new Vector3(5950f, 0f, 5500f), 45f, 30f, 60f,
                "Culp's Hill slope — terrain clearance holds (>= 3 m above ground)"),
            new Pose("12-near-clip-floor", new Vector3(4415f, 0f, 4855f), 250f, 35f, 5f,
                "distance floor — dynamic near-clip eased toward 0.3 m"),
        };

        string outDir;
        string prefix;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rtscamshots") < 0)
                return;
            var go = new GameObject("RtsCameraCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<RtsCameraCaptureHarness>();
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
            outDir = ArgValue("-rtscamOut",
                Path.Combine(Application.persistentDataPath, "rts-camera-shots"));
            prefix = ArgValue("-rtscamPrefix", "rtscam");
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var orbit = FindFirstObjectByType<OrbitCameraController>();
            var director = FindFirstObjectByType<BattleDirector>();
            if (clock == null || orbit == null)
            {
                Debug.LogError("rts camera shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;
            clock.CurrentTime = 8400f; // Pickett's Charge, inside the shipped window

            var log = new System.Text.StringBuilder();
            log.Append("{\n  \"unity\": \"").Append(Application.unityVersion)
                .Append("\",\n  \"clockTime\": 8400,\n  \"poses\": [\n");
            for (int i = 0; i < Strip.Length; i++)
            {
                Pose p = Strip[i];
                orbit.followPivot = null;
                orbit.pivot = p.Pivot;
                orbit.yawDeg = p.YawDeg;
                orbit.pitchDeg = p.PitchDeg;
                orbit.distance = p.Distance;
                // renderers pose from the clock and camera every frame; a
                // few frames settle any label declutter / LOD tier latch
                yield return new WaitForSecondsRealtime(0.6f);
                yield return null;
                yield return null;

                string shot = Path.Combine(outDir, $"{prefix}-{p.Name}.png");
                ScreenCapture.CaptureScreenshot(shot);
                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.3f);

                float groundY = director != null
                    ? director.GroundHeightAt(orbit.pivot.x, orbit.pivot.z) : 0f;
                float clearance = orbit.transform.position.y - groundY;
                log.Append("    {\"name\": \"").Append(p.Name)
                    .Append("\", \"pivot\": [").Append(F(orbit.pivot.x)).Append(", ")
                    .Append(F(orbit.pivot.y)).Append(", ").Append(F(orbit.pivot.z))
                    .Append("], \"yawDeg\": ").Append(F(orbit.yawDeg))
                    .Append(", \"pitchDeg\": ").Append(F(orbit.pitchDeg))
                    .Append(", \"distance\": ").Append(F(orbit.distance))
                    .Append(", \"cameraY\": ").Append(F(orbit.transform.position.y))
                    .Append(", \"groundY\": ").Append(F(groundY))
                    .Append(", \"clearanceM\": ").Append(F(clearance))
                    .Append(", \"nearClip\": ").Append(F(Camera.main != null
                        ? Camera.main.nearClipPlane : -1f))
                    .Append(", \"note\": \"").Append(p.Note).Append("\"}")
                    .Append(i < Strip.Length - 1 ? ",\n" : "\n");
            }
            log.Append("  ]\n}\n");
            File.WriteAllText(Path.Combine(outDir, "rts-camera-poses.json"), log.ToString());
            Debug.Log($"rts camera shots written to {outDir}");
            Quit(0);
        }

        static string F(float v) =>
            v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"RtsCameraCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
