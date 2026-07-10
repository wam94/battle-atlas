using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: black-powder VFX math (plan §9.1). Every puff attribute is a
/// pure function of (event, time, seed): no state, no Random, no _Time.
/// The ADR 0003 deferred item dies here — determinism first, then the
/// render evidence judges whether it reads as smoke.
/// </summary>
public class BlackPowderVfxTests
{
    const string Seed = "p8-test-seed";

    static SmokeEvent Musket(float t = 8500f) => new SmokeEvent
    {
        t = t,
        pos = new Vector2(4400f, 4860f),
        heightM = 1.45f,
        dirDeg = 262f,
        kind = SmokeKind.Musket,
        seedIndex = 17,
    };

    [Test]
    public void Emit_IsPureAndDeterministic()
    {
        var e = Musket();
        var a = new List<PuffInstance>();
        var b = new List<PuffInstance>();
        BlackPowderVfx.Emit(e, 8512.3f, Seed, a);
        BlackPowderVfx.Emit(e, 8512.3f, Seed, b);
        Assert.AreEqual(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            Assert.AreEqual(a[i].pos, b[i].pos);
            Assert.AreEqual(a[i].radius, b[i].radius);
            Assert.AreEqual(a[i].alpha, b[i].alpha);
            Assert.AreEqual(a[i].shade, b[i].shade);
            Assert.AreEqual(a[i].rollDeg, b[i].rollDeg);
        }
    }

    [Test]
    public void NoPuff_BeforeBirthOrAfterLife()
    {
        var e = Musket();
        var outList = new List<PuffInstance>();
        Assert.AreEqual(0, BlackPowderVfx.Emit(e, e.t - 0.01f, Seed, outList));
        Assert.AreEqual(0, BlackPowderVfx.Emit(
            e, e.t + BlackPowderVfx.MusketLife + 0.01f, Seed, outList));
        Assert.Greater(BlackPowderVfx.Emit(e, e.t + 1f, Seed, outList), 0);
    }

    [Test]
    public void Smoke_GrowsAndDriftsDownwind()
    {
        var e = Musket();
        var young = new List<PuffInstance>();
        var old = new List<PuffInstance>();
        BlackPowderVfx.Emit(e, e.t + 2f, Seed, young);
        BlackPowderVfx.Emit(e, e.t + 25f, Seed, old);
        Assert.Greater(old[0].radius, young[0].radius, "smoke expands");
        Vector2 driftDir = BlackPowderVfx.WindMps.normalized;
        float youngAlong = Vector2.Dot(young[0].pos - e.pos, driftDir);
        float oldAlong = Vector2.Dot(old[0].pos - e.pos, driftDir);
        Assert.Greater(oldAlong, youngAlong + 10f,
            "23 s of ED-19 wind carries the bank well downwind");
    }

    [Test]
    public void Cannon_ReadsBiggerThanMusket()
    {
        var m = Musket();
        var c = Musket();
        c.kind = SmokeKind.Cannon;
        var mp = new List<PuffInstance>();
        var cp = new List<PuffInstance>();
        BlackPowderVfx.Emit(m, m.t + 3f, Seed, mp);
        BlackPowderVfx.Emit(c, c.t + 3f, Seed, cp);
        Assert.Greater(cp.Count, mp.Count, "more sub-puffs");
        float mMax = 0f, cMax = 0f;
        foreach (var p in mp) mMax = Mathf.Max(mMax, p.radius);
        foreach (var p in cp) cMax = Mathf.Max(cMax, p.radius);
        Assert.Greater(cMax, mMax * 1.5f);
    }

    [Test]
    public void Dust_IsBrownAndShortLived()
    {
        var d = Musket();
        d.kind = SmokeKind.StrikeDust;
        var outList = new List<PuffInstance>();
        BlackPowderVfx.Emit(d, d.t + 1f, Seed, outList);
        Assert.Greater(outList.Count, 0);
        Assert.Less(outList[0].shade, 0.5f, "earth, not powder smoke");
        outList.Clear();
        Assert.AreEqual(0, BlackPowderVfx.Emit(
            d, d.t + BlackPowderVfx.StrikeDustLife + 0.1f, Seed, outList));
    }

    [Test]
    public void Visibility_DegradesMonotonicallyWithSmokeMass()
    {
        float prev = float.MaxValue;
        for (float mass = 0f; mass <= 3000f; mass += 250f)
        {
            float mfp = BlackPowderVfx.FogMeanFreePath(mass);
            Assert.LessOrEqual(mfp, prev, "more smoke, less visibility");
            Assert.GreaterOrEqual(mfp, BlackPowderVfx.MinMeanFreePathM);
            Assert.LessOrEqual(mfp, BlackPowderVfx.ClearMeanFreePathM);
            prev = mfp;
        }
        Assert.AreEqual(BlackPowderVfx.ClearMeanFreePathM,
            BlackPowderVfx.FogMeanFreePath(0f));
    }

    [Test]
    public void MarchDust_OnlyWhileTheUnitMoves()
    {
        // synthetic unit: moves east for 100 s, then halts for 100 s
        int seconds = 201;
        var u = new AngleBundleUnit
        {
            unitId = "dust-test",
            startStrength = 400,
            perSecond = new AnglePerSecond
            {
                t0 = 0f,
                x = new List<float>(),
                z = new List<float>(),
                facingDeg = new List<float>(),
                strength = new List<float>(),
                segmentIndex = new List<int>(),
            },
        };
        for (int s = 0; s < seconds; s++)
        {
            u.perSecond.x.Add(s < 100 ? s * 1.2f : 120f);
            u.perSecond.z.Add(0f);
            u.perSecond.facingDeg.Add(90f);
            u.perSecond.strength.Add(400f);
            u.perSecond.segmentIndex.Add(0);
        }
        var events = new List<SmokeEvent>();
        int seedIndex = 0;
        BlackPowderVfx.CompileMarchDust(u, 400, Seed, events, ref seedIndex);
        Assert.Greater(events.Count, 10, "marching raises dust");
        foreach (var e in events)
            Assert.Less(e.t, 104f, "no dust after the halt");
    }

    [Test]
    public void FlashWindow_IsShort()
    {
        Assert.IsTrue(BlackPowderVfx.FlashActive(100f, 100.02f));
        Assert.IsFalse(BlackPowderVfx.FlashActive(100f, 99.99f));
        Assert.IsFalse(BlackPowderVfx.FlashActive(100f, 100.2f));
    }
}
