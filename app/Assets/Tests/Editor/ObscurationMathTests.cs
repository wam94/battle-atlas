using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class ObscurationMathTests
{
    // fixed-position moving-emitter stand-in: the math only sees a position
    // function, so a constant one isolates emission/aging from track motion
    static readonly System.Func<float, Vector2> FixedEmitter = _ => new Vector2(3000f, 4000f);

    static EventDto UnitEvent(string id, string kind, float t0, float t1) =>
        new EventDto { id = id, kind = kind, t0 = t0, t1 = t1, unitId = "u1" };

    static EventDto SegmentEvent(string id, float t0, float t1,
        float x, float z, float x2, float z2) =>
        new EventDto { id = id, kind = "artillery_fire", t0 = t0, t1 = t1, x = x, z = z, x2 = x2, z2 = z2 };

    [Test]
    public void LivePuffs_EmptyBeforeT0AndAfterLastPuffDies()
    {
        var e = UnitEvent("ev-a", "musketry", 100f, 130f); // life 30
        var buffer = new Puff[64];
        Assert.AreEqual(0, ObscurationMath.LivePuffs(e, FixedEmitter, Vector2.zero, 99f, buffer));
        Assert.AreEqual(1, ObscurationMath.LivePuffs(e, FixedEmitter, Vector2.zero, 100f, buffer));
        // t1 + life: even the window's last puff has aged out
        Assert.AreEqual(0, ObscurationMath.LivePuffs(e, FixedEmitter, Vector2.zero, 160f, buffer));
    }

    [Test]
    public void LivePuffs_DriftsLinearlyDownwindAndGrows()
    {
        // one-puff event (single tick before t1) so the same emission is
        // observed at three times. Each puff rides its OWN hash-jittered
        // wind (±25°, ±30% speed — plumes read as weather, not a conveyor),
        // so the delta is that puff's fixed drift vector: inside the jitter
        // envelope of the authored wind, and exactly LINEAR in time.
        var e = UnitEvent("ev-drift", "artillery_fire", 0f, 4f);
        var wind = new Vector2(1.5f, -0.5f);
        var early = new Puff[4];
        var mid = new Puff[4];
        var late = new Puff[4];
        Assert.AreEqual(1, ObscurationMath.LivePuffs(e, FixedEmitter, wind, 10f, early));
        Assert.AreEqual(1, ObscurationMath.LivePuffs(e, FixedEmitter, wind, 25f, mid));
        Assert.AreEqual(1, ObscurationMath.LivePuffs(e, FixedEmitter, wind, 40f, late));
        Vector2 delta30 = late[0].posXZ - early[0].posXZ;  // 30s of drift
        Vector2 delta15 = mid[0].posXZ - early[0].posXZ;   // 15s of drift
        // envelope: speed within ±30% of authored, bearing within 25°
        float speed = delta30.magnitude / 30f;
        Assert.GreaterOrEqual(speed, wind.magnitude * (1f - ObscurationMath.DriftSpeedJitter) - 1e-3f);
        Assert.LessOrEqual(speed, wind.magnitude * (1f + ObscurationMath.DriftSpeedJitter) + 1e-3f);
        float angle = Vector2.Angle(delta30, wind) * Mathf.Deg2Rad;
        Assert.LessOrEqual(angle, ObscurationMath.DriftJitterRad + 1e-3f);
        // linear: twice the time, exactly twice the displacement
        Assert.AreEqual(delta30.x, delta15.x * 2f, 1e-3f);
        Assert.AreEqual(delta30.y, delta15.y * 2f, 1e-3f);
        Assert.Greater(late[0].radius, early[0].radius); // grows with age
        Assert.Greater(late[0].age01, early[0].age01);
    }

    [Test]
    public void LivePuffs_DeterministicAcrossScrubDirection()
    {
        // the reverse-scrub guarantee: the state at t computed fresh equals
        // the state at t computed after querying a far future t — bitwise,
        // because nothing is stateful
        var e = SegmentEvent("ev-line", 0f, 7200f, 2000f, 3000f, 3800f, 3200f);
        var wind = ObscurationMath.WindVector(45f, 2f);
        var first = new Puff[512];
        var again = new Puff[512];
        int n1 = ObscurationMath.LivePuffs(e, null, wind, 500f, first);
        ObscurationMath.LivePuffs(e, null, wind, 1100f, again); // scrub forward 600s...
        int n2 = ObscurationMath.LivePuffs(e, null, wind, 500f, again); // ...and back
        Assert.AreEqual(n1, n2);
        Assert.Greater(n1, 0);
        for (int i = 0; i < n1; i++)
        {
            Assert.AreEqual(first[i].posXZ, again[i].posXZ);
            Assert.AreEqual(first[i].age01, again[i].age01);
            Assert.AreEqual(first[i].radius, again[i].radius);
        }
    }

    [Test]
    public void LivePuffs_MovingEmitterSamplesPositionAtEmissionTime()
    {
        // an emitter marching east at 1 m/s: the puff emitted at tEmit must
        // sit at x = tEmit (plus its own wind drift) — smoke TRAILS the
        // column. A bug sampling emitterPosAt(t) instead of
        // emitterPosAt(tEmit) would stack every puff at the emitter's
        // current position and still pass every fixed-emitter test.
        System.Func<float, Vector2> marching = time => new Vector2(time, 0f);
        var e = UnitEvent("ev-march", "musketry", 0f, 30f); // cadence 3, life 30
        var wind = new Vector2(0.2f, 0f);
        var buffer = new Puff[32];
        int n = ObscurationMath.LivePuffs(e, marching, wind, 29f, buffer);
        Assert.AreEqual(10, n); // ticks at 0, 3, ..., 27 all still alive at t=29
        PuffParams p = PuffParams.For("musketry");
        for (int i = 0; i < n; i++)
        {
            float age = buffer[i].age01 * p.life;
            float tEmit = 29f - age;
            // slack: positional hash jitter + the per-puff drift variation
            // envelope (±30% speed and ±25° rotation < 0.75|w|·age combined)
            float driftSlack = wind.magnitude * age * 0.75f;
            Assert.AreEqual(tEmit + wind.x * age, buffer[i].posXZ.x, p.jitterM + driftSlack + 1e-3f);
            Assert.AreEqual(0f, buffer[i].posXZ.y, p.jitterM + driftSlack + 1e-3f);
        }
    }

    [Test]
    public void LivePuffs_JitterVariesByEventId()
    {
        var a = UnitEvent("ev-a", "musketry", 0f, 30f);
        var b = UnitEvent("ev-b", "musketry", 0f, 30f);
        var puffsA = new Puff[32];
        var puffsB = new Puff[32];
        int n = ObscurationMath.LivePuffs(a, FixedEmitter, Vector2.zero, 20f, puffsA);
        Assert.AreEqual(n, ObscurationMath.LivePuffs(b, FixedEmitter, Vector2.zero, 20f, puffsB));
        Assert.Greater(n, 0);
        // same emitter, same times — only the id-seeded jitter differs
        for (int i = 0; i < n; i++)
            Assert.AreNotEqual(puffsA[i].posXZ, puffsB[i].posXZ);
    }

    [Test]
    public void LivePuffs_SegmentSpreadsPuffsAlongTheLine()
    {
        // 600m line -> round(600/150) = 4 puffs per tick, hash-spread
        var e = SegmentEvent("ev-line", 0f, 7200f, 1000f, 2000f, 1600f, 2000f);
        var buffer = new Puff[128];
        int n = ObscurationMath.LivePuffs(e, null, Vector2.zero, 89f, buffer);
        // 23 live ticks (cadence 4, life 90) x 4 puffs
        Assert.AreEqual(92, n);
        float jitter = PuffParams.For("artillery_fire").jitterM;
        float minX = float.MaxValue, maxX = float.MinValue;
        for (int i = 0; i < n; i++)
        {
            Assert.That(buffer[i].posXZ.x, Is.InRange(1000f - jitter, 1600f + jitter));
            Assert.That(buffer[i].posXZ.y, Is.InRange(2000f - jitter, 2000f + jitter));
            minX = Mathf.Min(minX, buffer[i].posXZ.x);
            maxX = Mathf.Max(maxX, buffer[i].posXZ.x);
        }
        Assert.Greater(maxX - minX, 300f, "puffs must spread over half the segment");
    }

    [Test]
    public void LivePuffs_OverflowClampsOldestFirst()
    {
        // 23 live puffs (cadence 4, life 90) into a buffer of 5: the newest
        // five survive — ages 0, 4, 8, 12, 16 seconds, newest first — and
        // the oldest are the ones clamped away (they die soonest anyway)
        var e = UnitEvent("ev-cap", "artillery_fire", 0f, 1000f);
        var buffer = new Puff[5];
        int n = ObscurationMath.LivePuffs(e, FixedEmitter, Vector2.zero, 100f, buffer);
        Assert.AreEqual(5, n);
        for (int i = 0; i < n; i++)
            Assert.AreEqual(i * 4f / 90f, buffer[i].age01, 1e-4f);
    }

    [Test]
    public void AgeBucket_MonotonicQuartersClamped()
    {
        Assert.AreEqual(0, ObscurationMath.AgeBucket(0f));
        Assert.AreEqual(0, ObscurationMath.AgeBucket(0.2f));
        Assert.AreEqual(1, ObscurationMath.AgeBucket(0.3f));
        Assert.AreEqual(2, ObscurationMath.AgeBucket(0.6f));
        Assert.AreEqual(3, ObscurationMath.AgeBucket(0.9f));
        Assert.AreEqual(3, ObscurationMath.AgeBucket(1f)); // clamped, never bucket 4
        int prev = 0;
        for (float a = 0f; a <= 1f; a += 0.05f)
        {
            int bucket = ObscurationMath.AgeBucket(a);
            Assert.GreaterOrEqual(bucket, prev);
            prev = bucket;
        }
    }

    static UnitTrack Track(params (float t, float x, float z)[] kfs)
    {
        var unit = new UnitDto
        {
            id = "u1", name = "Test", side = "union", frontage_m = 100f, depth_m = 20f,
            keyframes = new List<KeyframeDto>(),
        };
        foreach (var (t, x, z) in kfs)
            unit.keyframes.Add(new KeyframeDto
            { t = t, x = x, z = z, facing = 0f, formation = "line", strength = 500f });
        return new UnitTrack(unit);
    }

    [Test]
    public void DustSpeedAt_MovingSegmentMatchesKnownSpeed()
    {
        // 300m in 300s = 1 m/s, well over the shed threshold mid-track;
        // at the track's start StateAt clamps, so the central difference
        // degrades one-sided to half the speed instead of misreading
        var track = Track((0f, 0f, 0f), (300f, 300f, 0f));
        Assert.AreEqual(1f, ObscurationMath.DustSpeedAt(track, 150f), 1e-4f);
        Assert.Greater(ObscurationMath.DustSpeedAt(track, 150f),
            ObscurationMath.DustSpeedThresholdMps);
        Assert.AreEqual(0.5f, ObscurationMath.DustSpeedAt(track, 0f), 1e-4f);
    }

    [Test]
    public void DustSpeedAt_StaticUnitIsZero()
    {
        var track = Track((0f, 4000f, 3000f), (600f, 4000f, 3000f));
        Assert.AreEqual(0f, ObscurationMath.DustSpeedAt(track, 300f), 1e-5f);
    }

    [Test]
    public void MuzzlePoint_OffsetsAlongCompassFacing()
    {
        // 90 = east = +x; 0 = north = +z(.y) — the compass convention.
        // A west-facing (270) Union gun's smoke must bloom WEST of the piece.
        var s = new UnitState { posXZ = new Vector2(100f, 200f), facingDeg = 90f };
        Vector2 east = ObscurationMath.MuzzlePoint(s, 10f);
        Assert.AreEqual(110f, east.x, 1e-3f);
        Assert.AreEqual(200f, east.y, 1e-3f);
        s.facingDeg = 270f;
        Vector2 west = ObscurationMath.MuzzlePoint(s, 10f);
        Assert.AreEqual(90f, west.x, 1e-3f);
        Assert.AreEqual(200f, west.y, 1e-3f);
        Assert.AreEqual(s.posXZ, ObscurationMath.MuzzlePoint(s, 0f)); // dust: no offset
    }
}
