using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace BattleAtlas
{
    // Phase 0 (Angle V2) measurement harness. Inert unless the player is
    // launched with -benchmark; then it drives the BattleClock to the plan's
    // reference timestamps (t=0, 8160, 8700, 9000), captures a screenshot and
    // a 10-second frame-time/memory sample at each, writes a JSON report to
    // -benchmarkOut <dir>, and quits. It exists so Phase 0 baselines can be
    // captured headlessly (dev standalone build) while the owner's editor
    // stays untouched — see docs/benchmarks/2026-07-08-v2-phase0-baseline.md.
    public class BenchmarkHarness : MonoBehaviour
    {
        // Default = the Phase 0 reference timestamps; -benchmarkTimes overrides
        // with a comma-separated list (Wave A1 evidence captures pass
        // "0,5400,8160,8700,9900" — the audit's slacken/road/wall/echelon
        // moments) without disturbing the reproducible Phase 0/4 baselines.
        static readonly float[] DefaultTimestamps = { 0f, 8160f, 8700f, 9000f };
        float[] timestamps = DefaultTimestamps;
        const float WarmupSeconds = 2f;
        const float SettleSeconds = 1.0f;   // renderers pose from the clock each frame
        const float SampleSeconds = 10f;

        string outDir;
        // screenshot/report basename prefix (-benchmarkPrefix). Default keeps
        // the Phase 0 names (atlas-t*.png / p0-benchmark.json); the Phase 4
        // HDRP capture passes "hdrp-atlas" so before/after files coexist.
        string prefix;
        BattleClock clock;
        readonly StringBuilder report = new StringBuilder();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-benchmark") < 0) return;
            var go = new GameObject("BenchmarkHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<BenchmarkHarness>();
        }

        static string ArgValue(string name, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : fallback;
        }

        void Start()
        {
            // The harness is usually launched from a script without window
            // focus; without this the player pauses and the run never ends.
            Application.runInBackground = true;
            outDir = ArgValue("-benchmarkOut",
                Path.Combine(Application.persistentDataPath, "p0-benchmark"));
            prefix = ArgValue("-benchmarkPrefix", "atlas");
            string times = ArgValue("-benchmarkTimes", "");
            if (!string.IsNullOrWhiteSpace(times))
                timestamps = times.Split(',')
                    .Select(s => float.Parse(s.Trim(), CultureInfo.InvariantCulture))
                    .ToArray();
            Directory.CreateDirectory(outDir);
            clock = FindFirstObjectByType<BattleClock>();
            if (clock == null)
            {
                File.WriteAllText(Path.Combine(outDir, ReportFileName()),
                    "{\"error\":\"no BattleClock in scene\"}");
                Quit(1);
                return;
            }
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            clock.Playing = false;
            clock.CurrentTime = 0f;
            yield return new WaitForSecondsRealtime(WarmupSeconds);

            report.Append("{\n");
            report.AppendFormat("  \"unity\": \"{0}\",\n", Application.unityVersion);
            report.AppendFormat("  \"development\": {0},\n", Debug.isDebugBuild ? "true" : "false");
            report.AppendFormat("  \"screen\": \"{0}x{1}\",\n", Screen.width, Screen.height);
            report.AppendFormat("  \"clockStartTime\": {0},\n", clock.StartTime);
            report.AppendFormat("  \"clockEndTime\": {0},\n", clock.EndTime);
            report.Append("  \"samples\": [\n");

            for (int i = 0; i < timestamps.Length; i++)
            {
                yield return CaptureAt(timestamps[i], i == timestamps.Length - 1);
            }

            report.Append("  ]\n}\n");
            File.WriteAllText(Path.Combine(outDir, ReportFileName()), report.ToString());
            Quit(0);
        }

        // "atlas" (the Phase 0 default) keeps its original report name so
        // scripts/p0-benchmark.sh output stays reproducible byte-for-name.
        string ReportFileName() =>
            prefix == "atlas" ? "p0-benchmark.json" : prefix + "-benchmark.json";

        IEnumerator CaptureAt(float t, bool last)
        {
            clock.Playing = false;
            clock.CurrentTime = Mathf.Min(t, clock.EndTime);
            yield return new WaitForSecondsRealtime(SettleSeconds);

            string shot = Path.Combine(outDir,
                string.Format(CultureInfo.InvariantCulture, "{0}-t{1:0}.png", prefix, t));
            ScreenCapture.CaptureScreenshot(shot);
            // CaptureScreenshot completes asynchronously a frame or two later.
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.25f);

            // Frame-time sample during normal playback (default 60x speed).
            clock.Speed = 60f;
            clock.Playing = true;
            var frames = new List<float>(2048);
            float elapsed = 0f;
            while (elapsed < SampleSeconds)
            {
                yield return null;
                float dt = Time.unscaledDeltaTime;
                frames.Add(dt);
                elapsed += dt;
            }
            clock.Playing = false;

            long allocated = Profiler.GetTotalAllocatedMemoryLong();
            long reserved = Profiler.GetTotalReservedMemoryLong();

            var stats = FrameStats.From(frames);
            report.Append("    {");
            report.AppendFormat(CultureInfo.InvariantCulture,
                "\"t\": {0:0}, \"screenshot\": \"{1}\", \"frames\": {2}, " +
                "\"avgFps\": {3:0.0}, \"p95FrameMs\": {4:0.00}, \"worstFrameMs\": {5:0.00}, " +
                "\"allocatedMB\": {6:0.0}, \"reservedMB\": {7:0.0}",
                t, Path.GetFileName(shot), frames.Count,
                stats.AvgFps, stats.P95Ms, stats.WorstMs,
                allocated / (1024.0 * 1024.0), reserved / (1024.0 * 1024.0));
            report.Append(last ? "}\n" : "},\n");
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"BenchmarkHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }

        // Pure so it stays testable without a player.
        public struct FrameStats
        {
            public float AvgFps;
            public float P95Ms;
            public float WorstMs;

            public static FrameStats From(IReadOnlyList<float> frameSeconds)
            {
                if (frameSeconds == null || frameSeconds.Count == 0)
                    return new FrameStats();
                var sorted = frameSeconds.OrderBy(x => x).ToArray();
                float total = sorted.Sum();
                int p95Index = Mathf.Clamp(
                    Mathf.CeilToInt(0.95f * sorted.Length) - 1, 0, sorted.Length - 1);
                return new FrameStats
                {
                    AvgFps = sorted.Length / total,
                    P95Ms = sorted[p95Index] * 1000f,
                    WorstMs = sorted[sorted.Length - 1] * 1000f,
                };
            }
        }
    }
}
