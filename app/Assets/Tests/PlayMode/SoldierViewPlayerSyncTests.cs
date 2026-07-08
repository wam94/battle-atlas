using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BattleAtlas;

// The project's first PlayMode suite (everything before Phase 1 was
// EditMode-only). Run (editor closed for this project path):
//
//   "$UNITY" -batchmode -projectPath app -runTests -testPlatform PlayMode \
//     -testResults playmode-results.xml -logFile -
//
// Tests marked [RequiresMedia] need the gitignored dev proxy; generate it
// first with reconstruction/scripts/generate_dev_proxy.sh. Without it they
// end Inconclusive (Assert.Ignore) with instructions rather than failing —
// media is never a committed input (plan sections 10.1, 16).
public class SoldierViewPlayerSyncTests
{
    const double T0 = 8160.0;
    const double T1 = 8170.0;
    const double Fps = 30.0;
    const float PrepareTimeout = 15f;
    const float SeekTimeout = 5f;

    BattleClock clock;
    SoldierViewPlayer player;
    GameObject cameraGo;

    static string ProxyPath =>
        SoldierViewPlayer.MediaPath("SoldierView/dev-timecode.proxy.mp4");

    static ViewpointDefinition DevViewpoint()
    {
        var set = ViewpointSet.FromJson(File.ReadAllText(
            SoldierViewPlayer.MediaPath("SoldierView/viewpoints.json")));
        return set.viewpoints[0];
    }

    [SetUp]
    public void SetUp()
    {
        cameraGo = new GameObject("TestCamera");
        cameraGo.tag = "MainCamera";
        cameraGo.AddComponent<Camera>();

        var clockGo = new GameObject("TestClock");
        clock = clockGo.AddComponent<BattleClock>();
        clock.StartTime = 46800f;
        clock.EndTime = 10800f;
        clock.Speed = 60f;
        clock.Playing = false;

        var playerGo = new GameObject("TestSoldierView");
        player = playerGo.AddComponent<SoldierViewPlayer>();
        player.clock = clock;
    }

    [TearDown]
    public void TearDown()
    {
        if (player != null && player.InSoldierView) player.Exit();
        Object.Destroy(player.gameObject);
        Object.Destroy(clock.gameObject);
        Object.Destroy(cameraGo);
    }

    static void RequireProxyOrIgnore()
    {
        if (!File.Exists(ProxyPath))
            Assert.Ignore(
                "dev proxy missing — run reconstruction/scripts/generate_dev_proxy.sh " +
                $"(expected at {ProxyPath})");
    }

    IEnumerator EnterPrepared(double battleTime)
    {
        clock.CurrentTime = (float)battleTime;
        Assert.IsTrue(player.TryEnter(DevViewpoint()), "TryEnter should succeed");
        float deadline = Time.realtimeSinceStartup + PrepareTimeout;
        while ((!player.Video.isPrepared || player.SeekInProgress) &&
               Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.IsTrue(player.Video.isPrepared, "video failed to prepare in time");
        Assert.IsFalse(player.SeekInProgress, "initial seek failed to settle in time");
        yield return null; // one frame so Update() computes drift
    }

    IEnumerator WaitForSeekSettle()
    {
        float deadline = Time.realtimeSinceStartup + SeekTimeout;
        while (player.SeekInProgress && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.IsFalse(player.SeekInProgress, "seek failed to settle in time");
        yield return null;
    }

    // --- No media required -------------------------------------------------

    [UnityTest]
    public IEnumerator Enter_OutsideWindow_IsRefused()
    {
        clock.CurrentTime = 5000f;
        LogAssert.Expect(LogType.Warning, new Regex("not enterable"));
        Assert.IsFalse(player.TryEnter(DevViewpoint()));
        Assert.IsFalse(player.InSoldierView);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Enter_AtT1Boundary_IsRefused()
    {
        clock.CurrentTime = (float)T1;
        LogAssert.Expect(LogType.Warning, new Regex("not enterable"));
        Assert.IsFalse(player.TryEnter(DevViewpoint()));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Enter_MissingProxy_RefusedWithClearWarning()
    {
        var vp = DevViewpoint();
        vp.media.proxy = "SoldierView/does-not-exist.mp4";
        clock.CurrentTime = 8165f;
        LogAssert.Expect(LogType.Warning, new Regex("proxy media missing"));
        Assert.IsFalse(player.TryEnter(vp));
        Assert.IsFalse(player.InSoldierView);
        StringAssert.Contains("generate_dev_proxy", player.LastWarning);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Enter_MissingFull_FallsBackToProxyWithWarning()
    {
        RequireProxyOrIgnore();
        var vp = DevViewpoint();
        vp.media.full = "SoldierView/full-not-rendered-yet.mp4";
        clock.CurrentTime = 8165f;
        LogAssert.Expect(LogType.Warning, new Regex("full media missing"));
        Assert.IsTrue(player.TryEnter(vp));
        Assert.IsTrue(player.UsingProxyFallback);
        yield return null;
    }

    // --- Media required ----------------------------------------------------

    [UnityTest]
    public IEnumerator Enter_SyncsVideoToClock_WithinOneFrame()
    {
        RequireProxyOrIgnore();
        yield return EnterPrepared(8163.0);
        Assert.AreEqual(1f, clock.Speed, "Soldier View forces 1x battle speed");
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                player.Video.time, clock.CurrentTime, T0, Fps),
            $"video {player.Video.time:0.000}s vs battle t={clock.CurrentTime:0.000} " +
            $"drift {player.CurrentDriftFrames:0.00} frames");
    }

    [UnityTest]
    public IEnumerator PlayThenPause_ClockAndVideoStayTogether()
    {
        RequireProxyOrIgnore();
        yield return EnterPrepared(8161.0);
        player.SetPlaying(true);
        yield return new WaitForSecondsRealtime(2f);
        player.SetPlaying(false);
        // A drift-resync seek may be in flight at the pause instant, and its
        // target was computed pre-pause; wait until the player converges on
        // the now-frozen clock. (Batchmode game time lags wall time badly, so
        // real-time pacing itself is owner-checklist material, not CI's.)
        float deadline = Time.realtimeSinceStartup + SeekTimeout;
        while ((player.SeekInProgress || !SoldierViewMath.WithinOneFrame(
                   player.Video.time, clock.CurrentTime, T0, Fps)) &&
               Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.IsFalse(player.Video.isPlaying, "video must pause with the clock");
        Assert.Greater(clock.CurrentTime, 8161.0f, "clock advanced during playback");
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                player.Video.time, clock.CurrentTime, T0, Fps),
            $"paused convergence: drift {player.CurrentDriftFrames:0.00} frames");

        // paused: nothing moves
        float tBefore = clock.CurrentTime;
        double vBefore = player.Video.time;
        yield return new WaitForSecondsRealtime(0.5f);
        Assert.AreEqual(tBefore, clock.CurrentTime, 1e-4f, "clock crept while paused");
        Assert.AreEqual(vBefore, player.Video.time, 1e-4, "video crept while paused");
    }

    [UnityTest]
    public IEnumerator Seek_SettlesWithinOneFrame()
    {
        RequireProxyOrIgnore();
        yield return EnterPrepared(8160.5);
        player.Seek(8168.25);
        yield return WaitForSeekSettle();
        Assert.AreEqual(8168.25f, clock.CurrentTime, 1e-3f);
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                player.Video.time, clock.CurrentTime, T0, Fps),
            $"after settle: video {player.Video.time:0.000}s battle {clock.CurrentTime:0.000} " +
            $"drift {player.CurrentDriftFrames:0.00} frames (latency {player.LastSeekLatencyMs:0}ms)");
    }

    [UnityTest]
    public IEnumerator Seek_OutsideWindow_ClampsToDecodableRange()
    {
        RequireProxyOrIgnore();
        yield return EnterPrepared(8165.0);
        // Above the window: clamps to the last PRESENTABLE frame — the
        // window end minus the decoder end-guard (the decoder refuses the
        // final frames of a stream as seek targets; see EndGuardFrames).
        player.Seek(9500.0);
        yield return WaitForSeekSettle();
        Assert.AreEqual((float)player.LastSeekableBattleTime, clock.CurrentTime, 1e-3f);
        Assert.Less(clock.CurrentTime, (float)T1);
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                player.Video.time, clock.CurrentTime, T0, Fps),
            $"end clamp: drift {player.CurrentDriftFrames:0.00} frames");
        player.Seek(100.0);
        yield return WaitForSeekSettle();
        Assert.AreEqual((float)T0, clock.CurrentTime, 1e-3f);
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                player.Video.time, clock.CurrentTime, T0, Fps),
            $"start clamp: drift {player.CurrentDriftFrames:0.00} frames");
    }

    [UnityTest]
    public IEnumerator Exit_ReturnsAtlasAtExactBattleTime()
    {
        RequireProxyOrIgnore();
        yield return EnterPrepared(8162.0);
        player.SetPlaying(true);
        yield return new WaitForSecondsRealtime(1.0f);
        player.SetPlaying(false);
        yield return null;
        float atExit = clock.CurrentTime;
        player.Exit();
        yield return null;
        Assert.IsFalse(player.InSoldierView);
        Assert.AreEqual(atExit, clock.CurrentTime, 0f, "exit must not move the clock");
        Assert.AreEqual(60f, clock.Speed, 1e-4f, "Atlas speed restored on exit");
        Assert.IsNull(player.GetComponent<UnityEngine.Video.VideoPlayer>(),
            "VideoPlayer must be torn down on exit");
    }

    // Phase 1 measurement: seek-settle latency across the window, written to
    // app/p1-seek-latency.json (gitignored) for the transition decision and
    // the Gate P1 report.
    [UnityTest]
    public IEnumerator SeekLatency_MeasuredAcrossWindow()
    {
        RequireProxyOrIgnore();
        yield return EnterPrepared(8160.0);

        // Deterministic targets: mix of far jumps, near jumps, and
        // sub-second nudges, forward and backward.
        double[] targets =
        {
            8169.5, 8160.2, 8164.9, 8165.1, 8168.0, 8161.3,
            8166.66, 8166.7, 8163.0, 8169.9, 8160.0, 8167.5,
        };
        var latencies = new List<double>();
        foreach (double t in targets)
        {
            player.Seek(t);
            yield return WaitForSeekSettle();
            Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                    player.Video.time, clock.CurrentTime, T0, Fps),
                $"seek to {t}: drift {player.CurrentDriftFrames:0.00} frames");
            latencies.Add(player.LastSeekLatencyMs);
        }

        latencies.Sort();
        double median = latencies[latencies.Count / 2];
        double worst = latencies[latencies.Count - 1];
        var sb = new StringBuilder();
        sb.Append("{\n  \"description\": \"SoldierView seek-settle latency, dev proxy 720p30 GOP15\",\n");
        sb.AppendFormat(CultureInfo.InvariantCulture,
            "  \"seeks\": {0}, \"medianMs\": {1:0.0}, \"worstMs\": {2:0.0},\n",
            latencies.Count, median, worst);
        sb.Append("  \"latenciesMsSorted\": [");
        sb.Append(string.Join(", ", latencies.ConvertAll(
            l => l.ToString("0.0", CultureInfo.InvariantCulture))));
        sb.Append("]\n}\n");
        string outPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", "p1-seek-latency.json"));
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"SoldierView seek latency: median {median:0.0}ms, " +
                  $"worst {worst:0.0}ms over {latencies.Count} seeks -> {outPath}");
    }
}
