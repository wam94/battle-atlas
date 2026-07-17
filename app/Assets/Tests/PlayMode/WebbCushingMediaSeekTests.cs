using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BattleAtlas;

// webb-wall + cushing-canister production media: the same seek-latency
// measurement Gate P10 ran on the garnett 1440p stream, per viewpoint
// (each encode is its own GOP/bitrate reality; the media contract says
// every shipped stream earns its own numbers). Needs the gitignored
// production encodes staged into StreamingAssets — otherwise Ignore with
// instructions, exactly like SoldierViewFullMediaSeekTests.
public class WebbCushingMediaSeekTests
{
    const double Fps = 30.0;
    const float PrepareTimeout = 20f;
    const float SeekTimeout = 8f;

    BattleClock clock;
    SoldierViewPlayer player;
    GameObject cameraGo;

    static ViewpointDefinition Viewpoint(string id)
    {
        var set = ViewpointSet.FromJson(File.ReadAllText(
            SoldierViewPlayer.MediaPath("SoldierView/viewpoints.json")));
        foreach (var vp in set.viewpoints)
            if (vp.id == id) return vp;
        throw new System.InvalidOperationException(
            $"viewpoints.json has no {id}");
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

    [UnityTest]
    public IEnumerator SeekLatency_WebbWallProductionMedia()
    {
        // long jumps across hundreds of GOPs, near jumps, sub-second
        // nudges, both directions; clear of the end-guard before t1
        double[] targets =
        {
            8790.0, 8165.3, 8520.5, 8519.9, 8700.0, 8200.02,
            8444.4, 8444.5, 8280.0, 8810.0, 8160.0, 8635.5,
        };
        return Measure("webb-wall", targets, "webb-wall-seek-latency.json");
    }

    [UnityTest]
    public IEnumerator SeekLatency_CushingCanisterProductionMedia()
    {
        double[] targets =
        {
            8750.0, 8402.3, 8600.5, 8599.9, 8700.0, 8420.02,
            8555.4, 8555.5, 8460.0, 8745.0, 8400.0, 8660.5,
        };
        return Measure("cushing-canister", targets,
            "cushing-canister-seek-latency.json");
    }

    IEnumerator Measure(string vpId, double[] targets, string reportName)
    {
        var vp = Viewpoint(vpId);
        if (string.IsNullOrEmpty(vp.media.full))
            Assert.Ignore($"{vpId}: viewpoints.json carries no full media");
        string fullPath = SoldierViewPlayer.MediaPath(vp.media.full);
        if (!File.Exists(fullPath))
            Assert.Ignore($"{vpId}: production media missing — render + " +
                "encode per docs/reconstruction/webb-cushing-viewpoints.md " +
                $"(expected at {fullPath})");

        double t0 = vp.t0;
        clock.CurrentTime = (float)t0;
        Assert.IsTrue(player.TryEnter(vp), "TryEnter should succeed");
        float deadline = Time.realtimeSinceStartup + PrepareTimeout;
        while ((!player.Video.isPrepared || player.SeekInProgress) &&
               Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.IsTrue(player.Video.isPrepared, "video failed to prepare in time");
        Assert.IsFalse(player.SeekInProgress, "initial seek failed to settle");
        yield return null;

        var latencies = new List<double>();
        foreach (double t in targets)
        {
            player.Seek(t);
            float sdl = Time.realtimeSinceStartup + SeekTimeout;
            while (player.SeekInProgress && Time.realtimeSinceStartup < sdl)
                yield return null;
            Assert.IsFalse(player.SeekInProgress,
                $"seek to {t} failed to settle in time");
            // steady-state check incl. the paused-drift corrector (see
            // SoldierViewFullMediaSeekTests for the discovered decoder
            // behavior this guards)
            float cdl = Time.realtimeSinceStartup + SeekTimeout;
            while ((player.SeekInProgress || !SoldierViewMath.WithinOneFrame(
                        player.Video.time, clock.CurrentTime, t0, Fps)) &&
                   Time.realtimeSinceStartup < cdl)
                yield return null;
            Assert.IsTrue(SoldierViewMath.WithinOneFrame(
                    player.Video.time, clock.CurrentTime, t0, Fps),
                $"seek to {t}: drift {player.CurrentDriftFrames:0.00} frames " +
                "after settle + paused-drift correction");
            latencies.Add(player.LastSeekLatencyMs);
        }

        latencies.Sort();
        double median = latencies[latencies.Count / 2];
        double worst = latencies[latencies.Count - 1];
        var sb = new StringBuilder();
        sb.Append("{\n  \"description\": \"SoldierView seek-settle latency, " +
            $"production 2560x1440p30 H.264 GOP30 full media ({vpId})\",\n");
        sb.AppendFormat(CultureInfo.InvariantCulture,
            "  \"seeks\": {0}, \"medianMs\": {1:0.0}, \"worstMs\": {2:0.0},\n",
            latencies.Count, median, worst);
        sb.Append("  \"latenciesMsSorted\": [");
        sb.Append(string.Join(", ", latencies.ConvertAll(
            l => l.ToString("0.0", CultureInfo.InvariantCulture))));
        sb.Append("]\n}\n");
        string outPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", reportName));
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"SoldierView {vpId} seek latency: median {median:0.0}ms, " +
                  $"worst {worst:0.0}ms over {latencies.Count} seeks -> {outPath}");
    }
}
