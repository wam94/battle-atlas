using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Fight-prone vocabulary (claim-iv-lying-down): the resolver's
/// fight_prone action class over the committed Iverson bundle (the left
/// three regiments fight prone; the 12th NC stands), plus a synthetic
/// hold -> fight_prone -> hold bundle for the rise transition. All pure
/// functions — determinism is asserted alongside behavior.
/// </summary>
public class FightProneResolverTests
{
    static string BundlePath => Path.Combine(
        Application.dataPath, "Battle", "Iverson", "iverson.bundle.json");

    static AngleActionContext ctx;
    static AngleActionContext Ctx => ctx ??= AngleActionContext.Compile(
        AngleBundleLoader.Load(BundlePath),
        AngleBundleLoader.Load(BundlePath).StagingSeed, null);

    static readonly string[] ProneRegiments =
        { "csa-5nc", "csa-20nc", "csa-23nc" };

    static bool IsProneClip(ClipId c) =>
        c == ClipId.GoProne || c == ClipId.ProneIdle ||
        c == ClipId.FightProneFire || c == ClipId.FightProneReload ||
        c == ClipId.ProneHitSettle;

    [Test]
    public void KitClips_CarryTheProneVocabulary()
    {
        Assert.AreEqual(36, KitClips.Count);   // 26 + the Angle-v2 set
        Assert.AreEqual("Go_Prone", KitClips.Name(ClipId.GoProne));
        Assert.AreEqual("Fight_Prone_Reload",
            KitClips.Name(ClipId.FightProneReload));
        Assert.IsTrue(KitClips.IsLoop(ClipId.ProneIdle));
        Assert.IsFalse(KitClips.IsLoop(ClipId.FightProneReload));
        // the attested cost of loading prone: slower than the standing drill
        Assert.Greater(KitClips.Duration(ClipId.FightProneReload),
            KitClips.Duration(ClipId.Reload));
        Assert.Greater(FireCycles.ProneCycle, FireCycles.Cycle,
            "the prone cycle must be slower than the standing cycle");
    }

    [Test]
    public void IversonBundle_LeftThreeFightProne_TwelfthStands()
    {
        foreach (var uid in ProneRegiments)
        {
            var ur = Ctx.Unit(uid);
            var seg = ur.unit.SegmentAt(6600f);
            Assert.AreEqual("fight_prone", seg.action, uid);
        }
        Assert.AreEqual("fire_independent",
            Ctx.Unit("csa-12nc").unit.SegmentAt(6600f).action,
            "the survivor regiment the record keeps standing");
    }

    [Test]
    public void ProneFight_ShowsTheFullVocabulary_AndOnlyIt()
    {
        var ur = Ctx.Unit("csa-20nc");
        var seg = ur.unit.SegmentAt(6600f);
        var seen = new HashSet<ClipId>();
        int slot = 0;
        while (ur.casualties[slot].fallT < seg.t1) slot++;
        // the drop itself, sampled clear of the segment boundary (the
        // +-0.6 s speed probe still sees the advance right at t0, so
        // locomotion legitimately wins there — same as any fire segment)
        float dropT = FireCycles.ProneDropTime(
            Ctx.seed, ur.unit.unitId, seg, slot);
        var mid = SoldierActionResolver.Resolve(
            Ctx, ur.unitIndex, slot, dropT + 1.7f);
        Assert.AreEqual(ClipId.GoProne, mid.clip);
        // then through two full prone cycles
        for (float t = seg.t0 + 1.3f;
             t < dropT + KitClips.Duration(ClipId.GoProne)
             + 2f * FireCycles.ProneCycle; t += 0.2f)
        {
            if (t >= seg.t1) break;
            var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
            seen.Add(s.clip);
        }
        Assert.Contains(ClipId.GoProne, new List<ClipId>(seen));
        Assert.Contains(ClipId.FightProneFire, new List<ClipId>(seen));
        Assert.Contains(ClipId.FightProneReload, new List<ClipId>(seen));
        Assert.Contains(ClipId.ProneIdle, new List<ClipId>(seen));
        foreach (var c in seen)
            Assert.IsTrue(IsProneClip(c) || c == ClipId.StandReady,
                $"unexpected clip {c} inside a fight_prone segment");
    }

    [Test]
    public void ProneMen_HoldTheirFilePositions()
    {
        // formation layout unchanged: a prone man's resolved slot position
        // is the same file position he stood on when the segment began
        var ur = Ctx.Unit("csa-23nc");
        var seg = ur.unit.SegmentAt(6600f);
        for (int slot = 0; slot < ur.slotCount; slot += 37)
        {
            if (ur.casualties[slot].fallT < 6900f) continue;
            var at = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, seg.t0 + 1f);
            var later = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, 6900f);
            Assert.AreEqual(at.posX, later.posX, 0.15f,
                $"slot {slot} drifted while prone");
            Assert.AreEqual(at.posZ, later.posZ, 0.15f);
        }
    }

    [Test]
    public void ProneCasualties_SettleWhereTheyLay_AndPersist()
    {
        var ur = Ctx.Unit("csa-5nc");
        var seg = ur.unit.SegmentAt(6600f);
        int checkedCount = 0;
        for (int slot = 0; slot < ur.slotCount && checkedCount < 25; slot++)
        {
            var cas = ur.casualties[slot];
            if (float.IsInfinity(cas.fallT)) continue;
            float dropT = FireCycles.ProneDropTime(
                Ctx.seed, ur.unit.unitId, seg, slot);
            if (cas.fallT < dropT + KitClips.Duration(ClipId.GoProne) + 1f)
                continue;   // fell standing (drop window) — standing falls
            if (cas.fallT > seg.t1 - 5f) continue;
            checkedCount++;
            var atFall = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, cas.fallT + 0.1f);
            Assert.AreEqual(ClipId.ProneHitSettle, atFall.clip,
                $"slot {slot}: a man hit while lying must settle, not " +
                "stand up to fall down");
            if (cas.woundedCrawl) continue;
            var end = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, Ctx.bundle.slice.t1);
            Assert.AreEqual(SoldierState.StatusDead, end.status);
            Assert.AreEqual(ClipId.ProneHitSettle, end.clip,
                "the body persists in the settle pose");
            Assert.AreEqual(atFall.posX, end.posX, 1e-4f);
            Assert.AreEqual(atFall.posZ, end.posZ, 1e-4f);
        }
        Assert.Greater(checkedCount, 10,
            "the destruction must produce prone casualties to check");
    }

    [Test]
    public void ProneDischarges_MatchTheResolverExactly()
    {
        // the VFX/audio invariant: every enumerated prone discharge time
        // resolves to Fight_Prone_Fire at the discharge moment (unless
        // the man has already fallen)
        var ur = Ctx.Unit("csa-20nc");
        var seg = ur.unit.SegmentAt(6600f);
        var times = new List<float>();
        int confirmed = 0;
        for (int slot = 0; slot < ur.slotCount; slot += 11)
        {
            times.Clear();
            FireCycles.SegmentDischargeTimes(
                Ctx.seed, ur.unit.unitId, seg, slot, ur.slotCount,
                ur.casualties[slot].fallT, seg.t0, seg.t1, times);
            foreach (float ft in times)
            {
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, ft);
                if (s.Fallen) continue;
                Assert.AreEqual(ClipId.FightProneFire, s.clip,
                    $"slot {slot} discharge at t={ft}");
                confirmed++;
            }
        }
        Assert.Greater(confirmed, 30);
    }

    [Test]
    public void TwelfthNC_StillFightsStanding()
    {
        var ur = Ctx.Unit("csa-12nc");
        var seen = new HashSet<ClipId>();
        for (int slot = 0; slot < ur.slotCount; slot += 29)
        {
            if (ur.casualties[slot].fallT < 6700f) continue;
            for (float t = 6500f; t < 6700f; t += 7f)
                seen.Add(SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t).clip);
        }
        foreach (var c in seen)
            Assert.IsFalse(IsProneClip(c),
                $"the 12th NC must not fight prone (got {c})");
        Assert.Contains(ClipId.Fire, new List<ClipId>(seen));
    }

    [Test]
    public void Resolve_IsRepeatable_Bitwise_InTheProneWindow()
    {
        foreach (var uid in ProneRegiments)
        {
            var ur = Ctx.Unit(uid);
            foreach (float t in new[] { 6003f, 6100.5f, 6600f, 7049f })
            {
                for (int slot = 0; slot < ur.slotCount; slot += 53)
                {
                    var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    var b = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    Assert.AreEqual(a.clip, b.clip);
                    Assert.AreEqual(a.clipTime, b.clipTime);
                    Assert.AreEqual(a.posX, b.posX);
                    Assert.AreEqual(a.posZ, b.posZ);
                    Assert.AreEqual(a.status, b.status);
                }
            }
        }
    }

    [Test]
    public void ClipTimes_WithinDuration_InTheProneWindow()
    {
        foreach (var uid in ProneRegiments)
        {
            var ur = Ctx.Unit(uid);
            for (float t = 6000f; t <= 7050f; t += 41f)
            {
                for (int slot = 0; slot < ur.slotCount; slot += 43)
                {
                    var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    Assert.GreaterOrEqual(s.clipTime, 0f,
                        $"{uid}/{slot}@{t} clip {s.clip}");
                    Assert.LessOrEqual(s.clipTime,
                        KitClips.Duration(s.clip) + 1e-3f,
                        $"{uid}/{slot}@{t} clip {s.clip}");
                }
            }
        }
    }

    // ------------------------------------------------------------------
    // Rise transition: synthetic hold -> fight_prone -> hold bundle.
    static AngleActionContext BuildRiseContext()
    {
        const int seconds = 41;
        const int start = 40;
        AngleBundleSegment Seg(string id, string action, float t0, float t1)
            => new AngleBundleSegment
            {
                id = id, action = action, t0 = t0, t1 = t1,
                formationFrom = "line", formationTo = "line",
                paceProfile = "static", provenance = "editorial",
                obstacleIds = new List<string>(),
            };
        var ps = new AnglePerSecond
        {
            t0 = 0,
            x = new List<float>(), z = new List<float>(),
            facingDeg = new List<float>(), strength = new List<float>(),
            segmentIndex = new List<int>(),
        };
        for (int i = 0; i < seconds; i++)
        {
            ps.x.Add(4000f); ps.z.Add(8500f);
            ps.facingDeg.Add(155f); ps.strength.Add(start);
            ps.segmentIndex.Add(i < 6 ? 0 : i < 24 ? 1 : 2);
        }
        var unit = new AngleBundleUnit
        {
            unitId = "test-prone-line", name = "test", side = "confederate",
            arm = "infantry", startStrength = start,
            segments = new List<AngleBundleSegment>
            {
                Seg("s-hold", "hold", 0f, 6f),
                Seg("s-prone", "fight_prone", 6f, 24f),
                Seg("s-recover", "hold", 24f, 40f),
            },
            casualtyProfiles = new List<AngleCasualtyProfile>(),
            perSecond = ps,
        };
        var bundle = new AngleBundle
        {
            format = "angle-bundle/1",
            stagingSeed = "fight-prone-test-seed",
            slice = new AngleBundleSlice { t0 = 0f, t1 = 40f },
            clock = new AngleBundleClock { startTimeSecondsSinceMidnight = 46800 },
            units = new List<AngleBundleUnit> { unit },
        };
        return AngleActionContext.Compile(bundle, bundle.StagingSeed, null);
    }

    [Test]
    public void RiseFromProne_PlaysInto_TheFollowingSegment()
    {
        var c = BuildRiseContext();
        var ur = c.Unit("test-prone-line");
        int rose = 0, idled = 0;
        for (int slot = 0; slot < ur.slotCount; slot++)
        {
            // before his staggered rise he still lies at the ready
            var early = SoldierActionResolver.Resolve(c, ur.unitIndex, slot, 24.05f);
            if (early.clip == ClipId.ProneIdle) idled++;
            bool sawRise = false;
            for (float t = 24f; t < 29f; t += 0.1f)
            {
                var s = SoldierActionResolver.Resolve(c, ur.unitIndex, slot, t);
                if (s.clip == ClipId.RiseFromProne) { sawRise = true; break; }
            }
            if (sawRise) rose++;
            // and by well after the stagger + rise he stands
            var after = SoldierActionResolver.Resolve(c, ur.unitIndex, slot, 30f);
            Assert.AreEqual(ClipId.StandReady, after.clip,
                $"slot {slot} must stand after rising");
        }
        Assert.AreEqual(ur.slotCount, rose, "every man rises exactly once");
        Assert.Greater(idled, 0,
            "the staggered rise must leave some men prone at t=24.05");
    }

    [Test]
    public void GoProne_StaggersAcrossTheLine_AndDropsEveryone()
    {
        var c = BuildRiseContext();
        var ur = c.Unit("test-prone-line");
        int standing = 0, dropping = 0;
        foreach (float probe in new[] { 6.2f, 7.0f, 8.0f, 9.0f })
        {
            for (int slot = 0; slot < ur.slotCount; slot++)
            {
                var s = SoldierActionResolver.Resolve(
                    c, ur.unitIndex, slot, probe);
                if (s.clip == ClipId.StandReady) standing++;
                if (s.clip == ClipId.GoProne) dropping++;
            }
        }
        Assert.Greater(standing, 0, "the drop is staggered, not a snap");
        Assert.Greater(dropping, 0);
        for (int slot = 0; slot < ur.slotCount; slot++)
        {
            var s = SoldierActionResolver.Resolve(c, ur.unitIndex, slot, 13f);
            Assert.IsTrue(IsProneClip(s.clip) && s.clip != ClipId.GoProne,
                $"slot {slot} must be prone by t=13 (got {s.clip})");
        }
    }

    [Test]
    public void MidTier_ProneClips_ReadLow()
    {
        Assert.AreEqual("pose_prone_fire",
            CrowdTiers.MidPose(ClipId.ProneIdle, 0.5f, 3, 6600f));
        Assert.AreEqual("pose_prone_fire",
            CrowdTiers.MidPose(ClipId.FightProneFire, 1.5f, 3, 6600f));
        Assert.AreEqual("pose_prone_fire",
            CrowdTiers.MidPose(ClipId.FightProneReload, 12f, 3, 6600f));
        Assert.AreEqual("pose_prone_fire",
            CrowdTiers.MidPose(ClipId.ProneHitSettle, 1.7f, 3, 6600f));
    }
}
