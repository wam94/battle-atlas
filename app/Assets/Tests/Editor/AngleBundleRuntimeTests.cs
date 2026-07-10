using System;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: the runtime loader for the compiled tactical bundle. Phase 5's
/// AngleBundleTests proved the artifact ships; these prove the runtime
/// actually consumes it (units, segments, per-second tracks, casualty
/// profiles with cause mixes).
/// </summary>
public class AngleBundleRuntimeTests
{
    static AngleBundle bundle;
    static AngleBundle Bundle => bundle ??= AngleBundleLoader.Load();

    [Test]
    public void Load_ParsesUnitsSegmentsAndTracks()
    {
        Assert.AreEqual("angle-bundle/1", Bundle.format);
        Assert.AreEqual(13, Bundle.units.Count);
        int seconds = (int)(Bundle.slice.t1 - Bundle.slice.t0) + 1;
        foreach (var u in Bundle.units)
        {
            Assert.AreEqual(seconds, u.perSecond.x.Count, u.unitId);
            Assert.Greater(u.segments.Count, 0, u.unitId);
            Assert.Greater(u.startStrength, 0, u.unitId);
        }
    }

    [Test]
    public void CauseMixes_ParseAndRoughlySumToOne()
    {
        int profiles = 0;
        foreach (var u in Bundle.units)
        {
            foreach (var p in u.casualtyProfiles)
            {
                profiles++;
                float sum = p.causeMix.musketry + p.causeMix.canister +
                            p.causeMix.shell + p.causeMix.unknown;
                Assert.AreEqual(1f, sum, 0.02f, $"{u.unitId}/{p.id}");
            }
        }
        Assert.Greater(profiles, 30);
    }

    [Test]
    public void PositionAt_InterpolatesBetweenSeconds()
    {
        var u = Bundle.Unit("csa-garnett");
        var a = u.PositionAt(8100f);
        var b = u.PositionAt(8101f);
        var mid = u.PositionAt(8100.5f);
        Assert.AreEqual((a.x + b.x) / 2f, mid.x, 1e-3f);
        Assert.AreEqual((a.y + b.y) / 2f, mid.y, 1e-3f);
    }

    [Test]
    public void SegmentAt_ReturnsTheActiveSegment()
    {
        var u = Bundle.Unit("csa-garnett");
        foreach (var seg in u.segments)
        {
            Assert.AreEqual(seg.id, u.SegmentAt(seg.t0).id);
            Assert.AreEqual(seg.id,
                u.SegmentAt((seg.t0 + seg.t1) / 2f).id);
        }
    }

    [Test]
    public void Validate_RejectsMalformedBundles()
    {
        var bad = new AngleBundle
        {
            format = "something-else",
            slice = new AngleBundleSlice { t0 = 0, t1 = 1 },
        };
        Assert.Throws<InvalidOperationException>(
            () => AngleBundleLoader.Validate(bad));
    }

    [Test]
    public void Load_MissingFileThrows()
    {
        Assert.Throws<InvalidOperationException>(
            () => AngleBundleLoader.Load("/no/such/bundle.json"));
    }
}
