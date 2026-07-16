using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BattleAtlas;

// Iverson production slice: seek behavior on the REAL iverson-forney-field
// production media (the P10 measurement, repeated for the second film —
// the media contract requires the 1440p full stream's own numbers before
// its bitrate/GOP are considered locked). Needs the gitignored production
// encode — run the Iverson render + scripts/iverson-encode.sh first, then
// copy the media into StreamingAssets; otherwise the test ends
// Inconclusive with instructions.
//
// NOTE the clock: iverson-forney-field rides the JULY 1 AFTERNOON phase
// clock (startTime 46800, endTime 18000) — same 13:00 LMT start value as
// July 3, different day; the player only needs vp.t0-relative times.
public class IversonMediaSeekTests
{
    const double Fps = 30.0;
    const float PrepareTimeout = 20f;
    const float SeekTimeout = 8f;

    BattleClock clock;
    SoldierViewPlayer player;
    GameObject cameraGo;

    static ViewpointDefinition IversonViewpoint()
    {
        var set = ViewpointSet.FromJson(File.ReadAllText(
            SoldierViewPlayer.MediaPath("SoldierView/viewpoints.json")));
        foreach (var vp in set.viewpoints)
            if (vp.id == "iverson-forney-field") return vp;
        throw new System.InvalidOperationException(
            "viewpoints.json has no iverson-forney-field");
    }

    [SetUp]
    public void SetUp()
    {
        cameraGo = new GameObject("TestCamera");
        cameraGo.tag = "MainCamera";
        cameraGo.AddComponent<Camera>();

        var clockGo = new GameObject("TestClock");
        clock = clockGo.AddComponent<BattleClock>();
        clock.StartTime = 46800f; // July 1 afternoon phase clock
        clock.EndTime = 18000f;
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
    public IEnumerator SeekLatency_IversonFull1440pProductionMedia()
    {
        var vp = IversonViewpoint();
        if (string.IsNullOrEmpty(vp.media.full))
            Assert.Ignore("viewpoints.json carries no full media for "
                + "iverson-forney-field yet");
        string fullPath = SoldierViewPlayer.MediaPath(vp.media.full);
        if (!File.Exists(fullPath))
            Assert.Ignore("iverson production media missing — render + "
                + "encode per docs/reconstruction/render-runbook.md "
                + $"(IversonProductionRender; expected at {fullPath})");

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

        // Deterministic battery across the FULL 20-minute stream (the
        // longest Soldier View media yet — long jumps span ~1,200 GOPs):
        // far jumps, near jumps, and sub-second nudges in both
        // directions. All targets stay clear of the end-guard window
        // before t1.
        double[] targets =
        {
            7030.0, 5835.3, 6435.5, 6434.9, 6900.0, 5900.02,
            6640.4, 6640.5, 6100.0, 7020.0, 5830.0, 6300.5,
        };
        var latencies = new List<double>();
        foreach (double t in targets)
        {
            player.Seek(t);
            float sdl = Time.realtimeSinceStartup + SeekTimeout;
            while (player.SeekInProgress && Time.realtimeSinceStartup < sdl)
                yield return null;
            Assert.IsFalse(player.SeekInProgress,
                $"seek to {t} failed to settle in time");
            // sample the corrected steady state (the P10 lesson: playing
            // through a seek can hold half a frame of drift until the
            // paused-drift corrector re-seeks within one Update)
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
            "iverson-forney-field 2560x1440p30 H.264 GOP30 full media\",\n");
        sb.AppendFormat(CultureInfo.InvariantCulture,
            "  \"seeks\": {0}, \"medianMs\": {1:0.0}, \"worstMs\": {2:0.0},\n",
            latencies.Count, median, worst);
        sb.Append("  \"latenciesMsSorted\": [");
        sb.Append(string.Join(", ", latencies.ConvertAll(
            l => l.ToString("0.0", CultureInfo.InvariantCulture))));
        sb.Append("]\n}\n");
        string outPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", "iverson-seek-latency.json"));
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"Iverson 1440p seek latency: median {median:0.0}ms, " +
                  $"worst {worst:0.0}ms over {latencies.Count} seeks -> {outPath}");
    }
}
