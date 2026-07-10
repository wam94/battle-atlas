using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace BattleAtlas
{
    // Phase 12 playback verification (plan §12 P12: "playback test on base
    // M1 8 GB if available; otherwise document the lowest tested Apple
    // Silicon configuration"). Inert unless the player is launched with
    // -p12probe; then it drives the REAL product loop in a standalone
    // Development build — Atlas playback, enter Soldier View on the
    // production media (through the HUD, content warning included), play,
    // a deterministic seek battery, play again, exit — capturing
    // screenshots, frame-time/memory samples (the Phase 0 BenchmarkHarness
    // pattern), seek latencies, drift, and the exact-return check to
    // -p12out <dir> as p12-playback.json. Quits with 0 only if the loop
    // completed and the exit restored the entry battle second.
    public class P12PlaybackProbe : MonoBehaviour
    {
        const float WarmupSeconds = 3f;
        const float AtlasSampleSeconds = 10f;
        const float SvSampleSeconds = 10f;
        const float EnterT = 8400f;        // mid-window: closing under canister
        // deterministic seek battery on the 11-minute stream: long jumps
        // across hundreds of GOPs in both directions, near jumps, and a
        // sub-second nudge (the P10/P11 measurement shape)
        static readonly float[] SeekTargets =
            { 8700f, 8200f, 8615f, 8610.4f, 8790f, 8300f };

        string outDir;
        BattleClock clock;
        SoldierViewPlayer player;
        AtlasHud hud;
        readonly StringBuilder report = new StringBuilder();
        bool failed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-p12probe") < 0)
                return;
            var go = new GameObject("P12PlaybackProbe");
            DontDestroyOnLoad(go);
            go.AddComponent<P12PlaybackProbe>();
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
            outDir = ArgValue("-p12out",
                Path.Combine(Application.persistentDataPath, "p12-playback"));
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            clock = FindFirstObjectByType<BattleClock>();
            hud = FindFirstObjectByType<AtlasHud>();
            player = FindFirstObjectByType<SoldierViewPlayer>();
            if (clock == null || hud == null || player == null)
            {
                File.WriteAllText(Path.Combine(outDir, "p12-playback.json"),
                    "{\"error\":\"no clock/hud/player in scene\"}");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(WarmupSeconds);

            report.Append("{\n");
            report.AppendFormat("  \"machine\": \"{0}; {1}; {2} MB RAM; {3}\",\n",
                SystemInfo.operatingSystem, SystemInfo.deviceModel,
                SystemInfo.systemMemorySize, SystemInfo.graphicsDeviceName);
            report.AppendFormat("  \"unity\": \"{0}\",\n", Application.unityVersion);
            report.AppendFormat("  \"development\": {0},\n",
                Debug.isDebugBuild ? "true" : "false");
            report.AppendFormat("  \"screen\": \"{0}x{1}\",\n",
                Screen.width, Screen.height);

            // ---- 1. Atlas playback (60x, the product's fast-scrub speed)
            clock.Playing = false;
            clock.CurrentTime = 8000f;
            yield return new WaitForSecondsRealtime(1f);
            yield return Shot("p12-atlas.png");
            clock.Speed = 60f;
            clock.Playing = true;
            yield return Sample("atlasPlayback60x", AtlasSampleSeconds);
            clock.Playing = false;

            // ---- 2. Enter Soldier View on the production media, via the
            // HUD (content warning path included — fresh state)
            clock.CurrentTime = EnterT;
            PlayerPrefs.DeleteKey(ContentWarningGate.PrefsKey);
            ViewpointDefinition hero = null;
            if (hud.viewpoints?.viewpoints != null)
                foreach (var vp in hud.viewpoints.viewpoints)
                    if (!vp.development) { hero = vp; break; }
            if (hero == null)
            {
                Fail("no product viewpoint in viewpoints.json");
                yield break;
            }
            hud.RequestEnter(hero);
            yield return null;
            if (hud.WarningVisible) hud.AcknowledgeWarning();
            yield return WaitUntil(
                () => player.InSoldierView && !hud.Transitioning
                    && !player.SeekInProgress, 30f, "enter");
            if (!player.InSoldierView) { Fail("enter failed"); yield break; }
            report.AppendFormat(CultureInfo.InvariantCulture,
                "  \"enteredAtT\": {0:0.00}, \"usingProxyFallback\": {1},\n",
                clock.CurrentTime, player.UsingProxyFallback ? "true" : "false");

            // ---- 3. Play the 1440p media
            player.SetPlaying(true);
            yield return new WaitForSecondsRealtime(1f);
            yield return Shot("p12-sv-playing.png");
            yield return Sample("soldierViewPlayback", SvSampleSeconds);

            // ---- 4. Deterministic seek battery. The sync contract is the
            // CORRECTED steady state (P11 evidence: play-through-seek can
            // freeze Video.time up to half a frame past the target; the
            // paused-drift corrector re-seeks one Update later — the frame
            // a viewer actually holds on is what must be within one frame).
            // Waiting for steadiness between targets also keeps the next
            // seek from overlapping a corrective seek.
            player.SetPlaying(false);
            player.SeekLatenciesMs.Clear();
            var seekLines = new List<string>();
            foreach (float target in SeekTargets)
            {
                player.Seek(target);
                yield return WaitUntil(() => !player.SeekInProgress, 10f,
                    $"seek {target}");
                double latency = player.LastSeekLatencyMs;
                yield return WaitUntil(() => !player.SeekInProgress
                        && Math.Abs(player.CurrentDriftFrames) <= 1.0,
                    5f, $"steady after seek {target}");
                seekLines.Add(string.Format(CultureInfo.InvariantCulture,
                    "    {{\"target\": {0}, \"latencyMs\": {1:0.0}, " +
                    "\"steadyDriftFrames\": {2:0.000}}}",
                    target, latency, player.CurrentDriftFrames));
                if (Mathf.Abs((float)player.CurrentDriftFrames) > 1f)
                    Fail($"seek to {target} held {player.CurrentDriftFrames} frames off");
            }
            yield return Shot("p12-sv-after-seek.png");
            report.Append("  \"seeks\": [\n");
            report.Append(string.Join(",\n", seekLines));
            report.Append("\n  ],\n");

            // ---- 5. Play again after seeking, then exit
            player.SetPlaying(true);
            yield return Sample("soldierViewAfterSeeks", 5f);
            player.SetPlaying(false);
            float beforeExit = clock.CurrentTime;
            hud.RequestExit();
            yield return WaitUntil(
                () => !player.InSoldierView && !hud.Transitioning, 15f, "exit");
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Shot("p12-atlas-return.png");
            bool exactReturn = Mathf.Abs(clock.CurrentTime - beforeExit) < 1e-3f;
            if (!exactReturn) Fail("exit did not restore the battle second");
            report.AppendFormat(CultureInfo.InvariantCulture,
                "  \"exitAtT\": {0:0.00}, \"exactReturn\": {1},\n",
                clock.CurrentTime, exactReturn ? "true" : "false");

            long allocated = Profiler.GetTotalAllocatedMemoryLong();
            long reserved = Profiler.GetTotalReservedMemoryLong();
            report.AppendFormat(CultureInfo.InvariantCulture,
                "  \"finalAllocatedMB\": {0:0.0}, \"finalReservedMB\": {1:0.0},\n",
                allocated / (1024.0 * 1024.0), reserved / (1024.0 * 1024.0));
            report.AppendFormat("  \"pass\": {0}\n}}\n", failed ? "false" : "true");
            File.WriteAllText(Path.Combine(outDir, "p12-playback.json"),
                report.ToString());
            Quit(failed ? 1 : 0);
        }

        // Frame-time/memory sample in the BenchmarkHarness mold.
        IEnumerator Sample(string name, float seconds)
        {
            var frames = new List<float>(2048);
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                yield return null;
                float dt = Time.unscaledDeltaTime;
                frames.Add(dt);
                elapsed += dt;
            }
            var stats = BenchmarkHarness.FrameStats.From(frames);
            long allocated = Profiler.GetTotalAllocatedMemoryLong();
            long reserved = Profiler.GetTotalReservedMemoryLong();
            report.AppendFormat(CultureInfo.InvariantCulture,
                "  \"{0}\": {{\"seconds\": {1:0.0}, \"frames\": {2}, " +
                "\"avgFps\": {3:0.0}, \"p95FrameMs\": {4:0.00}, " +
                "\"worstFrameMs\": {5:0.00}, \"allocatedMB\": {6:0.0}, " +
                "\"reservedMB\": {7:0.0}}},\n",
                name, seconds, frames.Count, stats.AvgFps, stats.P95Ms,
                stats.WorstMs, allocated / (1024.0 * 1024.0),
                reserved / (1024.0 * 1024.0));
        }

        IEnumerator Shot(string name)
        {
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
            if (!done()) Fail($"timed out on {what}");
        }

        void Fail(string why)
        {
            failed = true;
            Debug.LogError($"P12 probe: {why}");
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"P12PlaybackProbe done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
