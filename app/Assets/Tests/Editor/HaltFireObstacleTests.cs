using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Angle-v2 P6: the `halt_fire_obstacle` segment class (Trimble: "our
/// men stopped and began firing, instead of mounting the fence").
/// Synthetic bundle: an advancing line halts short of a traced fence,
/// works the standing fire cycle through the segment WITHOUT crossing,
/// then crosses in the following cross_obstacle segment. The clip needs
/// are covered by the existing standing vocabulary — this class is
/// resolver/fire-compiler wiring, not new art.
/// </summary>
public class HaltFireObstacleTests
{
    const string UnitId = "test-trimble-line";
    const float FenceX = 4052f;

    static AngleActionContext ctx;
    static AngleActionContext Ctx => ctx ??= Build();

    static AngleActionContext Build()
    {
        var unit = V2VocabTestRig.Unit(
            UnitId, "confederate", 0f, 200f, 4000f, 8500f, 90f, 60,
            new List<AngleBundleSegment>
            {
                V2VocabTestRig.Seg("s-adv", "advance", 0f, 40f),
                V2VocabTestRig.Seg("s-halt", "halt_fire_obstacle", 40f, 140f,
                    obstacles: new List<string> { "test-fence" }),
                V2VocabTestRig.Seg("s-cross", "cross_obstacle", 140f, 170f,
                    obstacles: new List<string> { "test-fence" }),
                V2VocabTestRig.Seg("s-on", "hold", 170f, 200f),
            },
            xTrack: new[] {
                (0f, 4000f), (40f, 4048f),      // advance to 4 m short
                (140f, 4048f),                   // the halt: stopped AT it
                (170f, 4062f), (200f, 4062f),    // over and beyond
            });
        var obstacles = new Dictionary<string, List<Vector2>>
        {
            ["test-fence"] = new List<Vector2>
            {
                new Vector2(FenceX, 8420f),
                new Vector2(FenceX, 8580f),
            },
        };
        return V2VocabTestRig.Compile(
            "halt-fire-test-seed", 0f, 200f, obstacles, unit);
    }

    [Test]
    public void HaltFire_IsAFireAction_WithAHaltLeadIn()
    {
        Assert.IsTrue(FireCycles.IsFireAction("halt_fire_obstacle"));
        var ur = Ctx.Unit(UnitId);
        var seg = ur.unit.SegmentAt(60f);
        Assert.AreEqual("halt_fire_obstacle", seg.action);
        for (int slot = 0; slot < ur.slotCount; slot += 9)
        {
            float lead = FireCycles.HaltLeadIn(Ctx.seed, UnitId, slot);
            Assert.GreaterOrEqual(lead,
                KitClips.Duration(ClipId.HaltDress));
            Assert.GreaterOrEqual(
                FireCycles.Offset(Ctx.seed, UnitId, seg, slot, ur.slotCount),
                lead, "the fire offset must clear the halt-dress");
        }
    }

    [Test]
    public void TheLine_Halts_ThenFires_AndNeverMountsTheFence()
    {
        var ur = Ctx.Unit(UnitId);
        var seen = new HashSet<ClipId>();
        for (int slot = 0; slot < ur.slotCount; slot += 2)
        {
            for (float t = 41f; t < 139f; t += 2.3f)
            {
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t);
                seen.Add(s.clip);
                Assert.AreNotEqual(ClipId.Cross, s.clip,
                    $"slot {slot}@{t}: no man mounts the fence while " +
                    "the halt-and-fire holds");
                // and nobody is beyond the traced line
                Assert.Less(s.posX, FenceX,
                    $"slot {slot}@{t} crossed the obstacle");
            }
        }
        Assert.Contains(ClipId.HaltDress, new List<ClipId>(seen),
            "the men come to a dressed halt at the fence");
        Assert.Contains(ClipId.Aim, new List<ClipId>(seen));
        Assert.Contains(ClipId.Fire, new List<ClipId>(seen));
        Assert.Contains(ClipId.Reload, new List<ClipId>(seen));
    }

    [Test]
    public void TheCrossing_ResumesAfterward()
    {
        var ur = Ctx.Unit(UnitId);
        int crossed = 0, sawCrossClip = 0;
        for (int slot = 0; slot < ur.slotCount; slot++)
        {
            bool clip = false;
            for (float t = 140f; t < 172f; t += 0.5f)
                if (SoldierActionResolver.Resolve(
                        Ctx, ur.unitIndex, slot, t).clip == ClipId.Cross)
                { clip = true; break; }
            if (clip) sawCrossClip++;
            var end = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, 195f);
            if (end.posX > FenceX) crossed++;
        }
        Assert.AreEqual(ur.slotCount, crossed,
            "every man ends beyond the fence after the crossing segment");
        Assert.Greater(sawCrossClip, ur.slotCount / 2,
            "the crossing plays the climb clip");
    }

    [Test]
    public void Discharges_MatchTheResolverExactly()
    {
        var ur = Ctx.Unit(UnitId);
        var seg = ur.unit.SegmentAt(60f);
        var times = new List<float>();
        int confirmed = 0;
        for (int slot = 0; slot < ur.slotCount; slot += 4)
        {
            times.Clear();
            FireCycles.SegmentDischargeTimes(
                Ctx.seed, UnitId, seg, slot, ur.slotCount,
                float.PositiveInfinity, seg.t0, seg.t1, times);
            Assert.Greater(times.Count, 0);
            foreach (float ft in times)
            {
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, ft);
                Assert.AreEqual(ClipId.Fire, s.clip,
                    $"slot {slot} discharge at t={ft}");
                confirmed++;
            }
        }
        Assert.Greater(confirmed, 30);
    }

    [Test]
    public void Resolve_IsRepeatable_Bitwise_AcrossTheArc()
    {
        var ur = Ctx.Unit(UnitId);
        foreach (float t in new[] { 20f, 41.5f, 88.8f, 141f, 155.5f, 190f })
        {
            for (int slot = 0; slot < ur.slotCount; slot += 7)
            {
                var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                var b = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                Assert.AreEqual(a.clip, b.clip);
                Assert.AreEqual(a.clipTime, b.clipTime);
                Assert.AreEqual(a.posX, b.posX);
                Assert.AreEqual(a.posZ, b.posZ);
            }
        }
    }

    [Test]
    public void ClipTimes_WithinDuration_AcrossTheArc()
    {
        var ur = Ctx.Unit(UnitId);
        for (float t = 0f; t <= 200f; t += 4.3f)
        {
            for (int slot = 0; slot < ur.slotCount; slot += 5)
            {
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t);
                Assert.GreaterOrEqual(s.clipTime, 0f,
                    $"slot {slot}@{t} clip {s.clip}");
                Assert.LessOrEqual(s.clipTime,
                    KitClips.Duration(s.clip) + 1e-3f,
                    $"slot {slot}@{t} clip {s.clip}");
            }
        }
    }
}
