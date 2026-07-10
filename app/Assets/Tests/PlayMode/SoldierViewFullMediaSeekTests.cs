using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BattleAtlas;

// Phase 10: seek behavior on the REAL production media (plan §12 Phase
// 10 gate: "encoded video seeks acceptably on target hardware"). The
// Gate P1 numbers were measured on the 720p dev proxy only; the media
// contract predicted the 1440p full stream needed its own measurement
// before locking bitrate/GOP. Needs the gitignored production encode —
// run the Phase 10 render + scripts/p10-encode.sh first, then copy or
// symlink the media into StreamingAssets (see the render runbook);
// otherwise the test ends Inconclusive with instructions.
public class SoldierViewFullMediaSeekTests
{
    const double Fps = 30.0;
    const float PrepareTimeout = 20f;
    const float SeekTimeout = 8f;

    BattleClock clock;
    SoldierViewPlayer player;
    GameObject cameraGo;

    static ViewpointDefinition HeroViewpoint()
    {
        var set = ViewpointSet.FromJson(File.ReadAllText(
            SoldierViewPlayer.MediaPath("SoldierView/viewpoints.json")));
        foreach (var vp in set.viewpoints)
            if (vp.id == "garnett-road-to-angle") return vp;
        throw new System.InvalidOperationException(
            "viewpoints.json has no garnett-road-to-angle");
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
    public IEnumerator SeekLatency_Full1440pProductionMedia()
    {
        var vp = HeroViewpoint();
        if (string.IsNullOrEmpty(vp.media.full))
            Assert.Ignore("viewpoints.json carries no full media yet — " +
                "Phase 10 sets media.full after the production encode");
        string fullPath = SoldierViewPlayer.MediaPath(vp.media.full);
        if (!File.Exists(fullPath))
            Assert.Ignore("production media missing — render + encode per " +
                $"docs/reconstruction/render-runbook.md (expected at {fullPath})");

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

        // Deterministic battery across the FULL 11-minute stream: long
        // jumps spanning hundreds of GOPs (the case the proxy-only P1
        // measurement could not exercise), near jumps, and sub-second
        // nudges in both directions. All targets stay clear of the
        // end-guard window before t1.
        double[] targets =
        {
            8790.0, 8165.3, 8520.5, 8519.9, 8700.0, 8200.02,
            8444.4, 8444.5, 8280.0, 8810.0, 8160.0, 8635.5,
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
            // Sample the corrected STEADY state, not the first settle:
            // playing through a seek can freeze Video.time up to half a
            // frame past the requested frame center (frame-delivery
            // timing), which reads as ~1.005 frames of drift against a
            // clock sitting near a frame boundary (observed on this
            // machine at 8165.3 with the production 1440p stream, Phase
            // 11 staging). The player's paused-drift corrector re-seeks
            // within one Update in that case; the frame a viewer actually
            // holds on is the corrected one, and THAT must be within one
            // frame of the clock.
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
            "production 2560x1440p30 H.264 GOP30 full media\",\n");
        sb.AppendFormat(CultureInfo.InvariantCulture,
            "  \"seeks\": {0}, \"medianMs\": {1:0.0}, \"worstMs\": {2:0.0},\n",
            latencies.Count, median, worst);
        sb.Append("  \"latenciesMsSorted\": [");
        sb.Append(string.Join(", ", latencies.ConvertAll(
            l => l.ToString("0.0", CultureInfo.InvariantCulture))));
        sb.Append("]\n}\n");
        string outPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", "p10-seek-latency.json"));
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"SoldierView 1440p seek latency: median {median:0.0}ms, " +
                  $"worst {worst:0.0}ms over {latencies.Count} seeks -> {outPath}");
    }
}
