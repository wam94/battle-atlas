using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Angle-v2 P4: colors falling and passing (Shepard: "Every flag in the
/// brigade excepting one was captured at or within the works"; 3-4
/// bearers down per regiment). Synthetic hold-under-fire bundle with a
/// heavy casualty window so the succession chain is genuinely walked;
/// the expected succession is recomputed independently from the
/// compiled schedule and compared against ColorGuard.StateAt.
/// </summary>
public class ColorGuardTests
{
    const string UnitId = "test-colors-line";
    const int Strength = 80;      // 40 files, center file 19
    const int Party = 6;

    static AngleActionContext ctx;
    static AngleActionContext Ctx => ctx ??= Build(Party);

    static AngleActionContext Build(int party)
    {
        var unit = V2VocabTestRig.Unit(
            UnitId, "confederate", 0f, 120f, 4000f, 8508f, 165f, Strength,
            new List<AngleBundleSegment>
            {
                V2VocabTestRig.Seg("s-hold", "hold", 0f, 120f),
            },
            new List<AngleCasualtyProfile>
            {
                // heavy loss so the chain is walked end to end
                V2VocabTestRig.Profile("cas-heavy", 10f, 60f, 70),
            },
            colorParty: party);
        return V2VocabTestRig.Compile(
            "colors-test-seed", 0f, 120f, null, unit);
    }

    [Test]
    public void KitClips_CarryTheColorsVocabulary()
    {
        Assert.AreEqual("Colors_Carry", KitClips.Name(ClipId.ColorsCarry));
        Assert.AreEqual("Colors_Bearer_Fall",
            KitClips.Name(ClipId.ColorsBearerFall));
        Assert.AreEqual("Colors_Pickup", KitClips.Name(ClipId.ColorsPickup));
        Assert.IsTrue(KitClips.IsLoop(ClipId.ColorsCarry));
        Assert.IsFalse(KitClips.IsLoop(ClipId.ColorsBearerFall));
        Assert.IsFalse(KitClips.IsLoop(ClipId.ColorsPickup));
    }

    [Test]
    public void Chain_WalksTheFrontRank_FromTheCenterOut()
    {
        int files = FormationRoster.Files(Strength);
        int center = (files - 1) / 2;
        Assert.AreEqual(center, ColorGuard.ChainSlot(Strength, 0));
        Assert.AreEqual(center + 1, ColorGuard.ChainSlot(Strength, 1));
        Assert.AreEqual(center - 1, ColorGuard.ChainSlot(Strength, 2));
        Assert.AreEqual(center + 2, ColorGuard.ChainSlot(Strength, 3));
        Assert.AreEqual(center - 2, ColorGuard.ChainSlot(Strength, 4));
        // every chain slot is front rank
        for (int i = 0; i < Party; i++)
            Assert.AreEqual(0, FormationRoster.RankOf(
                ColorGuard.ChainSlot(Strength, i), Strength));
    }

    // The reference walk, straight off the compiled schedule.
    static List<(int bearer, float from, float to)> ExpectedCarries()
    {
        var ur = Ctx.Unit(UnitId);
        var result = new List<(int, float, float)>();
        float from = 0f;
        int i = 0;
        while (i < Party)
        {
            int bearer = ColorGuard.ChainSlot(Strength, i);
            float fallT = ur.casualties[bearer].fallT;
            if (fallT <= from) { i++; continue; }
            result.Add((bearer, from, fallT));
            if (float.IsInfinity(fallT)) break;
            float pickupT = fallT + ColorGuard.PickupDelay;
            int j = i + 1;
            while (j < Party && Ctx.Unit(UnitId).casualties[
                ColorGuard.ChainSlot(Strength, j)].fallT <= pickupT)
                j++;
            if (j >= Party) break;
            i = j;
            from = pickupT;
        }
        return result;
    }

    [Test]
    public void Succession_MatchesTheCompiledSchedule()
    {
        var ur = Ctx.Unit(UnitId);
        var carries = ExpectedCarries();
        Assert.Greater(carries.Count, 1,
            "the heavy window must fell at least one bearer");

        foreach (var (bearer, from, to) in carries)
        {
            // mid-carry: this man holds the colors
            float mid = float.IsInfinity(to)
                ? from + 10f : (from + to) / 2f;
            if (mid >= Ctx.bundle.slice.t1) mid = Ctx.bundle.slice.t1 - 1f;
            var cs = ColorGuard.StateAt(Ctx, ur, mid);
            Assert.AreEqual(bearer, cs.bearerSlot,
                $"carry [{from},{to}) at t={mid}");
            Assert.IsTrue(
                cs.phase == ColorGuard.Phase.Carried ||
                cs.phase == ColorGuard.Phase.Raising);
        }

        // between a fall and the next pickup the colors are grounded at
        // the fallen bearer's spot
        var (b0, f0, t0) = carries[0];
        Assert.IsFalse(float.IsInfinity(t0));
        var grounded = ColorGuard.StateAt(
            Ctx, ur, t0 + ColorGuard.PickupDelay - 0.5f);
        Assert.IsTrue(
            grounded.phase == ColorGuard.Phase.Grounded ||
            grounded.phase == ColorGuard.Phase.BearerFalling ||
            grounded.phase == ColorGuard.Phase.Down);
        var fallState = SoldierActionResolver.Resolve(
            Ctx, ur.unitIndex, b0, t0 + 0.05f);
        Assert.AreEqual(fallState.posX, grounded.posX, 1e-3f,
            "grounded colors lie where the bearer fell");
        Assert.AreEqual(fallState.posZ, grounded.posZ, 1e-3f);
    }

    [Test]
    public void TheBearer_CarriesTheColors_NotHisPiece()
    {
        var ur = Ctx.Unit(UnitId);
        var carries = ExpectedCarries();
        var (bearer, from, to) = carries[0];
        float t = Mathf.Min((from + (float.IsInfinity(to) ? from + 20f : to))
            / 2f, 119f);
        var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, bearer, t);
        Assert.AreEqual(ClipId.ColorsCarry, s.clip,
            "a holding bearer in a hold segment carries the colors");
        // his file mates still stand at the ready
        int other = ColorGuard.ChainSlot(Strength, 0) + 10;
        if (float.IsInfinity(ur.casualties[other].fallT))
            Assert.AreEqual(ClipId.StandReady, SoldierActionResolver
                .Resolve(Ctx, ur.unitIndex, other, t).clip);
    }

    [Test]
    public void AFallingBearer_GoesDownWithTheStaff()
    {
        var ur = Ctx.Unit(UnitId);
        var carries = ExpectedCarries();
        var (bearer, _, fallT) = carries[0];
        Assert.IsFalse(float.IsInfinity(fallT));
        var s = SoldierActionResolver.Resolve(
            Ctx, ur.unitIndex, bearer, fallT + 0.05f);
        Assert.AreEqual(ClipId.ColorsBearerFall, s.clip);
        Assert.AreEqual(SoldierState.StatusFalling, s.status);
        // the body persists in the release pose
        var end = SoldierActionResolver.Resolve(
            Ctx, ur.unitIndex, bearer, 119f);
        if (!ur.casualties[bearer].woundedCrawl)
        {
            Assert.AreEqual(ClipId.ColorsBearerFall, end.clip);
            Assert.AreEqual(SoldierState.StatusDead, end.status);
        }
        // a fallen NON-bearer plays the standard falls
        for (int slot = 0; slot < ur.slotCount; slot++)
        {
            if (FormationRoster.RankOf(slot, Strength) == 0) continue;
            float ft = ur.casualties[slot].fallT;
            if (float.IsInfinity(ft)) continue;
            var ns = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, ft + 0.05f);
            Assert.AreNotEqual(ClipId.ColorsBearerFall, ns.clip);
            return;
        }
    }

    [Test]
    public void TheNextMan_TakesUpTheColors()
    {
        var ur = Ctx.Unit(UnitId);
        var carries = ExpectedCarries();
        Assert.Greater(carries.Count, 1);
        var (successor, from, _) = carries[1];
        // his first RaiseDur seconds are the pickup
        var s = SoldierActionResolver.Resolve(
            Ctx, ur.unitIndex, successor, from + 0.5f);
        Assert.AreEqual(ClipId.ColorsPickup, s.clip);
        var cs = ColorGuard.StateAt(Ctx, ur, from + 0.5f);
        Assert.AreEqual(ColorGuard.Phase.Raising, cs.phase);
        Assert.AreEqual(successor, cs.bearerSlot);
    }

    [Test]
    public void WithoutAColorParty_NothingChanges()
    {
        var plain = Build(0);
        var ur = plain.Unit(UnitId);
        for (int slot = 0; slot < ur.slotCount; slot += 2)
        {
            for (float t = 1f; t < 119f; t += 6.3f)
            {
                var s = SoldierActionResolver.Resolve(
                    plain, ur.unitIndex, slot, t);
                Assert.AreNotEqual(ClipId.ColorsCarry, s.clip);
                Assert.AreNotEqual(ClipId.ColorsPickup, s.clip);
                Assert.AreNotEqual(ClipId.ColorsBearerFall, s.clip);
            }
        }
    }

    [Test]
    public void StateAt_IsRepeatable_Bitwise()
    {
        var ur = Ctx.Unit(UnitId);
        for (float t = 0.5f; t < 120f; t += 4.1f)
        {
            var a = ColorGuard.StateAt(Ctx, ur, t);
            var b = ColorGuard.StateAt(Ctx, ur, t);
            Assert.AreEqual(a.phase, b.phase);
            Assert.AreEqual(a.bearerSlot, b.bearerSlot);
            Assert.AreEqual(a.posX, b.posX);
            Assert.AreEqual(a.posZ, b.posZ);
            Assert.AreEqual(a.sinceT, b.sinceT);
        }
    }

    [Test]
    public void ClipTimes_WithinDuration_ForTheChain()
    {
        var ur = Ctx.Unit(UnitId);
        for (float t = 0f; t <= 120f; t += 1.9f)
        {
            for (int i = 0; i < Party; i++)
            {
                int slot = ColorGuard.ChainSlot(Strength, i);
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
