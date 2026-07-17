using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Angle-v2 P5 (owner ruling 2026-07-15, "ship p5 mounted officers
/// falling"; proposed ED-81): the anonymous mounted-officer figure —
/// riding station behind the line, the rear under the hit, the rider's
/// single articulated fall, the riderless bolt. Synthetic bundle; no
/// committed bundle carries a mounted officer (the data wave wires
/// Garnett's documented case with its own citations).
/// </summary>
public class MountedOfficerTests
{
    const string UnitId = "test-mounted-line";

    static AngleActionContext ctx;
    static AngleActionContext Ctx => ctx ??= Build();

    static MountedOfficerSpec Spec => new MountedOfficerSpec
    {
        officerId = "test-officer-1",
        unitId = UnitId,
        fallT = 60f,
        backOffsetM = 8f,
        alongOffsetM = 2f,
    };

    static AngleActionContext Build()
    {
        // advance 0-40 (moving), halt 40-120 (static)
        var unit = V2VocabTestRig.Unit(
            UnitId, "confederate", 0f, 120f, 4000f, 8500f, 90f, 60,
            new List<AngleBundleSegment>
            {
                V2VocabTestRig.Seg("s-adv", "advance", 0f, 40f),
                V2VocabTestRig.Seg("s-hold", "hold", 40f, 120f),
            },
            xTrack: new[] { (0f, 4000f), (40f, 4048f), (120f, 4048f) });
        return V2VocabTestRig.Compile(
            "mounted-test-seed", 0f, 120f, null, unit);
    }

    [Test]
    public void HorseClips_TableIsConsistent()
    {
        Assert.AreEqual(4, HorseClips.Count);
        Assert.AreEqual("Horse_Stand", HorseClips.Name(HorseClipId.HorseStand));
        Assert.AreEqual("Horse_Rear", HorseClips.Name(HorseClipId.HorseRear));
        Assert.IsTrue(HorseClips.IsLoop(HorseClipId.HorseWalk));
        Assert.IsFalse(HorseClips.IsLoop(HorseClipId.HorseRear));
        Assert.AreEqual("Ride_Seat", KitClips.Name(ClipId.RideSeat));
        Assert.AreEqual("Rider_Fall", KitClips.Name(ClipId.RiderFall));
        Assert.IsTrue(KitClips.IsLoop(ClipId.RideSeat));
        Assert.IsFalse(KitClips.IsLoop(ClipId.RiderFall));
        // the rider leaves the saddle while the horse is still rearing
        Assert.Less(MountedOfficer.RiderFallStart,
            HorseClips.Duration(HorseClipId.HorseRear));
    }

    [Test]
    public void Riding_FollowsTheUnit_BehindTheLine()
    {
        // advancing: the horse walks; the rider sits
        var s = MountedOfficer.Resolve(Ctx, Spec, 20f);
        Assert.IsTrue(s.horseVisible);
        Assert.AreEqual(HorseClipId.HorseWalk, s.horseClip);
        Assert.AreEqual(ClipId.RideSeat, s.riderClip);
        Assert.IsFalse(s.riderDown);
        var ur = Ctx.Unit(UnitId);
        Vector2 centroid = ur.unit.PositionAt(20f);
        float d = (new Vector2(s.posX, s.posZ) - centroid).magnitude;
        Assert.AreEqual(
            Mathf.Sqrt(Spec.backOffsetM * Spec.backOffsetM +
                       Spec.alongOffsetM * Spec.alongOffsetM), d, 0.3f,
            "the officer rides at his station in the unit frame");
        // the rider rides ON the horse
        Assert.AreEqual(s.posX, s.riderPosX, 1e-4f);
        Assert.AreEqual(s.posZ, s.riderPosZ, 1e-4f);

        // halted: the horse stands
        var h = MountedOfficer.Resolve(Ctx, Spec, 50f);
        Assert.AreEqual(HorseClipId.HorseStand, h.horseClip);
    }

    [Test]
    public void TheHit_RearsTheHorse_AndTakesTheRiderDown()
    {
        // during the rear the horse holds the fall point
        var rear = MountedOfficer.Resolve(Ctx, Spec, 61f);
        Assert.AreEqual(HorseClipId.HorseRear, rear.horseClip);
        Assert.AreEqual(1f, rear.horseClipTime, 1e-3f);

        // the rider is still seated for the first beat...
        var seated = MountedOfficer.Resolve(Ctx, Spec, 60.3f);
        Assert.AreEqual(ClipId.RideSeat, seated.riderClip);
        // ...then leaves the saddle
        var falling = MountedOfficer.Resolve(Ctx, Spec, 61.5f);
        Assert.AreEqual(ClipId.RiderFall, falling.riderClip);
        Assert.IsFalse(falling.riderDown);

        // and lies still afterward, where he fell (persistent body)
        var down = MountedOfficer.Resolve(Ctx, Spec, 80f);
        Assert.AreEqual(ClipId.RiderFall, down.riderClip);
        Assert.IsTrue(down.riderDown);
        Assert.AreEqual(
            KitClips.Duration(ClipId.RiderFall) - 1f / 48f,
            down.riderClipTime, 1e-3f);
        Assert.AreEqual(falling.riderPosX, down.riderPosX, 1e-4f);
        Assert.AreEqual(falling.riderPosZ, down.riderPosZ, 1e-4f);
    }

    [Test]
    public void TheRiderlessHorse_BoltsRearward_OutOfTheFight()
    {
        float boltT0 = Spec.fallT +
            HorseClips.Duration(HorseClipId.HorseRear);
        var fall = MountedOfficer.Resolve(Ctx, Spec, Spec.fallT + 0.1f);
        var b1 = MountedOfficer.Resolve(Ctx, Spec, boltT0 + 4f);
        Assert.AreEqual(HorseClipId.HorseBolt, b1.horseClip);
        Assert.IsTrue(b1.horseVisible);
        float d1 = new Vector2(b1.posX - fall.posX, b1.posZ - fall.posZ)
            .magnitude;
        Assert.AreEqual(MountedOfficer.BoltSpeedMps * 4f, d1, 0.1f);
        // rearward: away from the unit facing (within the yaw jitter)
        float facing = Ctx.Unit(UnitId).unit.FacingAt(Spec.fallT);
        float away = Mathf.Abs(Mathf.DeltaAngle(facing + 180f, b1.facingDeg));
        Assert.LessOrEqual(away, MountedOfficer.BoltYawJitterDeg + 1e-3f);
        // and once past the bolt range the horse has left the scene
        var gone = MountedOfficer.Resolve(
            Ctx, Spec, boltT0 + MountedOfficer.BoltRangeM /
            MountedOfficer.BoltSpeedMps + 5f);
        Assert.IsFalse(gone.horseVisible);
        // the rider's body is still there
        Assert.IsTrue(gone.riderDown);
    }

    [Test]
    public void Resolve_IsRepeatable_Bitwise()
    {
        foreach (float t in new[] { 5f, 45.5f, 60.2f, 61.7f, 63f, 90f })
        {
            var a = MountedOfficer.Resolve(Ctx, Spec, t);
            var b = MountedOfficer.Resolve(Ctx, Spec, t);
            Assert.AreEqual(a.posX, b.posX);
            Assert.AreEqual(a.posZ, b.posZ);
            Assert.AreEqual(a.facingDeg, b.facingDeg);
            Assert.AreEqual(a.horseClip, b.horseClip);
            Assert.AreEqual(a.horseClipTime, b.horseClipTime);
            Assert.AreEqual(a.riderClip, b.riderClip);
            Assert.AreEqual(a.riderClipTime, b.riderClipTime);
            Assert.AreEqual(a.riderPosX, b.riderPosX);
            Assert.AreEqual(a.riderPosZ, b.riderPosZ);
        }
    }

    [Test]
    public void ClipTimes_WithinDurations()
    {
        for (float t = 0f; t <= 120f; t += 0.9f)
        {
            var s = MountedOfficer.Resolve(Ctx, Spec, t);
            Assert.GreaterOrEqual(s.horseClipTime, 0f);
            Assert.LessOrEqual(s.horseClipTime,
                HorseClips.Duration(s.horseClip) + 1e-3f, $"t={t}");
            Assert.GreaterOrEqual(s.riderClipTime, 0f);
            Assert.LessOrEqual(s.riderClipTime,
                KitClips.Duration(s.riderClip) + 1e-3f, $"t={t}");
        }
    }
}
