using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace BattleAtlas
{
    // Phase-switching slice evidence harness. Inert unless the player is
    // launched with -phaseswitchshots; then ONE session (launched normally,
    // no -battleFile — the scene's July 3 afternoon asset) visits all five
    // reconstructed phases through AtlasHud.SwitchToPhase — the exact code
    // path the day panel's "Load this phase" buttons drive — capturing a
    // distinct battle moment and the owning day panel per phase (honest
    // not-reconstructed notes included), recording every switch's measured
    // time, and sampling steady-state FPS on the same July 3 view before
    // any switch and after returning to it. Output to -phaseswitchOut
    // <dir> (screenshots + phase-switch-summary.json), then quits. The
    // DayExpansion3CaptureHarness mold (windowed Development standalone;
    // the owner's editor untouched).
    public class PhaseSwitchCaptureHarness : MonoBehaviour
    {
        const float SwitchTimeout = 30f;
        const float FpsSampleSeconds = 4f;

        struct View
        {
            public float T;
            public Vector3 Pivot;
            public float Yaw, Pitch, Dist;

            public View(float t, Vector3 pivot, float yaw, float pitch, float dist)
            {
                T = t; Pivot = pivot; Yaw = yaw; Pitch = pitch; Dist = dist;
            }
        }

        // one distinct, citation-anchored battle moment per phase; camera
        // coordinates reuse the prior harnesses' staged views (dayexp2/3)
        // plus a Pickett's-Charge view for the shipped July 3 phase
        struct PhaseShot
        {
            public string PhaseId;
            public string ShotName;
            public View View;

            public PhaseShot(string phaseId, string shotName, View view)
            {
                PhaseId = phaseId; ShotName = shotName; View = view;
            }
        }

        static readonly PhaseShot[] Sequence =
        {
            // July 1 morning — the Reynolds moment (~10:15, CA-J1M-4b/5)
            new PhaseShot("july1-morning", "reynolds-1015",
                new View(9900f, new Vector3(3050f, 0f, 7350f), 90f, 50f, 1900f)),
            // July 1 afternoon — Barlow's Knoll both-sided (~15:30, CA-J1P-3)
            new PhaseShot("july1-afternoon", "knoll-1530",
                new View(9000f, new Vector3(5300f, 0f, 8350f), 0f, 50f, 2000f)),
            // July 2 afternoon — the Wheatfield (~18:00, dayexp2 view)
            new PhaseShot("july2-afternoon", "wheatfield-1800",
                new View(9000f, new Vector3(3650f, 0f, 3150f), 0f, 55f, 2200f)),
            // July 2 evening — Culp's Hill (~20:00, dayexp2 view)
            new PhaseShot("july2-evening", "culp-2000",
                new View(1860f, new Vector3(5950f, 0f, 5500f), 90f, 50f, 2000f)),
            // July 3 afternoon (return leg) — Pickett's Charge inside the
            // Soldier View window (t=8400 = 15:20 LMT; the entry marker
            // must be BACK after the round trip)
            new PhaseShot("july3-afternoon", "picketts-charge-1520",
                new View(8400f, new Vector3(4300f, 0f, 4900f), 90f, 50f, 2400f)),
        };

        // the FPS-comparison view: the same July 3 charge view, sampled
        // fresh-launch and again after the five-phase round trip
        static readonly View FpsView =
            new View(8400f, new Vector3(4300f, 0f, 4900f), 90f, 50f, 2400f);

        string outDir;
        readonly List<string> switchLines = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-phaseswitchshots") < 0)
                return;
            var go = new GameObject("PhaseSwitchCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<PhaseSwitchCaptureHarness>();
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
            outDir = ArgValue("-phaseswitchOut",
                Path.Combine(Application.persistentDataPath, "phase-switch-shots"));
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

        static void ApplyView(BattleClock clock, OrbitCameraController orbit, View v)
        {
            clock.CurrentTime = v.T;
            clock.Playing = false;
            orbit.followPivot = null;
            orbit.pivot = v.Pivot;
            orbit.yawDeg = v.Yaw;
            orbit.pitchDeg = v.Pitch;
            orbit.distance = v.Dist;
        }

        IEnumerator MeasureFps(Action<float> report)
        {
            // steady-state average over a fixed realtime window, first
            // frame skipped (it carries the view-change hitch)
            yield return null;
            int frames = 0;
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < FpsSampleSeconds)
            {
                frames++;
                yield return null;
            }
            report(frames / (Time.realtimeSinceStartup - t0));
        }

        IEnumerator SwitchAndWait(AtlasHud hud, PhaseDto phase)
        {
            float before = Time.realtimeSinceStartup;
            hud.SwitchToPhase(phase);
            if (!hud.Switching)
            {
                Debug.LogError($"phase-switch shots: switch to '{phase.id}' "
                    + "did not start (already loaded? file missing?)");
                Quit(1);
                yield break;
            }
            while (hud.Switching
                && Time.realtimeSinceStartup - before < SwitchTimeout)
                yield return null;
            if (hud.Switching)
            {
                Debug.LogError($"phase-switch shots: switch to '{phase.id}' timed out");
                Quit(1);
                yield break;
            }
            switchLines.Add($"    {{ \"phase\": \"{phase.id}\", "
                + $"\"switchMs\": {hud.LastSwitchMs.ToString("F0", CultureInfo.InvariantCulture)} }}");
            Debug.Log($"phase-switch shots: '{phase.id}' switched in {hud.LastSwitchMs:F0} ms");
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var orbit = FindFirstObjectByType<OrbitCameraController>();
            var hud = FindFirstObjectByType<AtlasHud>();
            if (clock == null || orbit == null || hud == null)
            {
                Debug.LogError("phase-switch shots: scene has no clock/orbit/HUD");
                Quit(1);
                yield break;
            }
            PhaseManifest manifest;
            try
            {
                manifest = PhaseManifest.FromJson(File.ReadAllText(Path.Combine(
                    Application.streamingAssetsPath, "Atlas/battle-manifest.json")));
            }
            catch (Exception e)
            {
                Debug.LogError($"phase-switch shots: manifest rejected — {e.Message}");
                Quit(1);
                yield break;
            }
            var phasesById = new Dictionary<string, PhaseDto>();
            var dayIndexByPhase = new Dictionary<string, int>();
            for (int d = 0; d < manifest.days.Length; d++)
                foreach (PhaseDto p in manifest.days[d].phases)
                {
                    phasesById[p.id] = p;
                    dayIndexByPhase[p.id] = d;
                }

            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            // fresh-launch reference: the July 3 charge view + FPS sample
            ApplyView(clock, orbit, FpsView);
            yield return new WaitForSecondsRealtime(1.0f);
            float fpsFresh = 0f;
            yield return MeasureFps(f => fpsFresh = f);
            yield return Capture("phaseswitch-00-july3-fresh-launch.png");
            hud.OpenDayPanel(2); // July 3: loaded phase lit + honest morning note
            yield return Capture("phaseswitch-01-day-july3-panel-fresh.png");
            hud.CloseDayPanel();

            // the five-phase round trip
            int shotIndex = 2;
            foreach (PhaseShot step in Sequence)
            {
                if (!phasesById.TryGetValue(step.PhaseId, out PhaseDto phase))
                {
                    Debug.LogError($"phase-switch shots: manifest has no phase '{step.PhaseId}'");
                    Quit(1);
                    yield break;
                }
                yield return SwitchAndWait(hud, phase);
                ApplyView(clock, orbit, step.View);
                yield return new WaitForSecondsRealtime(1.0f);
                yield return Capture(
                    $"phaseswitch-{shotIndex:D2}-{step.PhaseId}-{step.ShotName}.png");
                shotIndex++;
                hud.OpenDayPanel(dayIndexByPhase[step.PhaseId]);
                yield return Capture(
                    $"phaseswitch-{shotIndex:D2}-day-panel-{step.PhaseId}.png");
                shotIndex++;
                hud.CloseDayPanel();
            }

            // post-round-trip FPS on the identical view (clock memory
            // returned July 3 to the remembered t; re-stage explicitly so
            // the sample is view-identical regardless)
            ApplyView(clock, orbit, FpsView);
            yield return new WaitForSecondsRealtime(1.0f);
            float fpsAfter = 0f;
            yield return MeasureFps(f => fpsAfter = f);

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"note\": \"one session visiting all five reconstructed phases via AtlasHud.SwitchToPhase (phase-switching slice)\",");
            sb.AppendLine("  \"switches\": [");
            sb.AppendLine(string.Join(",\n", switchLines));
            sb.AppendLine("  ],");
            sb.AppendLine($"  \"fpsJuly3FreshLaunch\": {fpsFresh.ToString("F1", CultureInfo.InvariantCulture)},");
            sb.AppendLine($"  \"fpsJuly3AfterFiveSwitches\": {fpsAfter.ToString("F1", CultureInfo.InvariantCulture)}");
            sb.AppendLine("}");
            File.WriteAllText(
                Path.Combine(outDir, "phase-switch-summary.json"), sb.ToString());

            Debug.Log($"phase-switch shots written to {outDir} "
                + $"(fps fresh {fpsFresh:F1} vs after switches {fpsAfter:F1})");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"PhaseSwitchCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
