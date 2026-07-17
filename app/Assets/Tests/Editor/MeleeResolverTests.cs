using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Angle-v2 P3: the `melee` action class (Peyton: "hand to hand, and of
/// the most desperate character"). Synthetic two-line bundle at wall
/// range; no committed bundle carries the action (film safety). All
/// pure functions — determinism asserted alongside behavior.
/// </summary>
public class MeleeResolverTests
{
    // "a-csa-line" < "b-us-line" ordinally: the CSA line leads pairs.
    const string Csa = "a-csa-line";
    const string Us = "b-us-line";

    static AngleActionContext ctx;
    static AngleActionContext Ctx => ctx ??= Build();

    static AngleActionContext Build()
    {
        var csa = V2VocabTestRig.Unit(
            Csa, "confederate", 0f, 120f, 4000f, 8508f, 180f, 80,
            new List<AngleBundleSegment>
            {
                V2VocabTestRig.Seg("s-c-hold", "hold", 0f, 6f),
                V2VocabTestRig.Seg("s-c-melee", "melee", 6f, 90f, Us),
                V2VocabTestRig.Seg("s-c-after", "hold", 90f, 120f),
            },
            new List<AngleCasualtyProfile>
            {
                V2VocabTestRig.Profile("cas-c-melee", 20f, 80f, 10),
            });
        var us = V2VocabTestRig.Unit(
            Us, "union", 0f, 120f, 4000f, 8500f, 0f, 60,
            new List<AngleBundleSegment>
            {
                V2VocabTestRig.Seg("s-u-hold", "hold", 0f, 6f),
                V2VocabTestRig.Seg("s-u-melee", "melee", 6f, 90f, Csa),
                V2VocabTestRig.Seg("s-u-after", "hold", 90f, 120f),
            },
            new List<AngleCasualtyProfile>
            {
                V2VocabTestRig.Profile("cas-u-melee", 20f, 80f, 6),
            });
        return V2VocabTestRig.Compile(
            "melee-test-seed", 0f, 120f, null, csa, us);
    }

    static bool IsMeleeClip(ClipId c) =>
        c == ClipId.MeleeClubSwing || c == ClipId.MeleeBayonetThrust ||
        c == ClipId.MeleeGrappleA || c == ClipId.MeleeGrappleB ||
        c == ClipId.MeleeParry;

    [Test]
    public void KitClips_CarryTheMeleeVocabulary()
    {
        Assert.AreEqual("Melee_Club_Swing",
            KitClips.Name(ClipId.MeleeClubSwing));
        Assert.AreEqual("Melee_Grapple_B",
            KitClips.Name(ClipId.MeleeGrappleB));
        Assert.IsTrue(KitClips.IsLoop(ClipId.MeleeGrappleA));
        Assert.IsTrue(KitClips.IsLoop(ClipId.MeleeGrappleB));
        Assert.IsFalse(KitClips.IsLoop(ClipId.MeleeClubSwing));
        Assert.IsFalse(KitClips.IsLoop(ClipId.MeleeParry));
        // grapple halves are authored to the same loop length
        Assert.AreEqual(KitClips.Duration(ClipId.MeleeGrappleA),
            KitClips.Duration(ClipId.MeleeGrappleB));
        // melee is never a fire action: no discharges, no smoke
        Assert.IsFalse(FireCycles.IsFireAction("melee"));
    }

    [Test]
    public void MeleeSegment_ShowsTheBoutVocabulary_AndNoMusketry()
    {
        var ur = Ctx.Unit(Csa);
        var seen = new HashSet<ClipId>();
        for (int slot = 0; slot < ur.slotCount; slot += 3)
        {
            if (!float.IsInfinity(ur.casualties[slot].fallT)) continue;
            for (float t = 8f; t < 88f; t += 1.7f)
                seen.Add(SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t).clip);
        }
        Assert.Contains(ClipId.MeleeClubSwing, new List<ClipId>(seen));
        Assert.Contains(ClipId.MeleeBayonetThrust, new List<ClipId>(seen));
        Assert.Contains(ClipId.MeleeParry, new List<ClipId>(seen));
        Assert.Contains(ClipId.StandReady, new List<ClipId>(seen));
        Assert.IsFalse(seen.Contains(ClipId.Aim),
            "a melee is not a firefight");
        Assert.IsFalse(seen.Contains(ClipId.Fire));
        Assert.IsFalse(seen.Contains(ClipId.Reload));
    }

    [Test]
    public void GrapplePairs_FormAcrossTheLines_AndFaceEachOther()
    {
        var csa = Ctx.Unit(Csa);
        var us = Ctx.Unit(Us);
        float t = 30f;   // pairs locked (stagger 0..4 s + step-in 1.5 s)
        int pairs = 0;
        for (int slot = 0; slot < csa.slotCount; slot++)
        {
            var a = SoldierActionResolver.Resolve(Ctx, csa.unitIndex, slot, t);
            if (a.clip != ClipId.MeleeGrappleA) continue;
            // his opposite number plays the anti-phase half at clinch
            // separation, facing back at him
            bool matched = false;
            for (int v = 0; v < us.slotCount; v++)
            {
                var b = SoldierActionResolver.Resolve(Ctx, us.unitIndex, v, t);
                if (b.clip != ClipId.MeleeGrappleB) continue;
                float d = new Vector2(a.posX - b.posX, a.posZ - b.posZ)
                    .magnitude;
                if (d > MeleeChoreo.PairSeparationM + 0.15f) continue;
                Assert.AreEqual(MeleeChoreo.PairSeparationM, d, 0.15f);
                float facing = Mathf.Abs(Mathf.DeltaAngle(
                    a.facingDeg, b.facingDeg));
                Assert.AreEqual(180f, facing, 1f,
                    "grapplers must face each other");
                matched = true;
                break;
            }
            Assert.IsTrue(matched,
                $"csa slot {slot} grapples with no opposite number");
            pairs++;
        }
        Assert.Greater(pairs, 2, "the hash share must realize some pairs");
        // and the follower side never plays the lead half
        for (int v = 0; v < us.slotCount; v += 1)
            Assert.AreNotEqual(ClipId.MeleeGrappleA,
                SoldierActionResolver.Resolve(Ctx, us.unitIndex, v, t).clip,
                "the ordinally-later unit is the follower (Grapple_B)");
    }

    [Test]
    public void Bouts_AreBounded_ByReadyPauses()
    {
        var ur = Ctx.Unit(Csa);
        // find an unpaired surviving slot working bouts
        for (int slot = 0; slot < ur.slotCount; slot++)
        {
            if (!float.IsInfinity(ur.casualties[slot].fallT)) continue;
            bool bout = false, ready = false;
            ClipId boutClip = ClipId.StandReady;
            for (float t = 12f; t < 60f; t += 0.35f)
            {
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t);
                if (s.clip == ClipId.MeleeGrappleA ||
                    s.clip == ClipId.MeleeGrappleB)
                { bout = false; break; }   // paired man: skip
                if (IsMeleeClip(s.clip)) { bout = true; boutClip = s.clip; }
                if (bout && s.clip == ClipId.StandReady) ready = true;
            }
            if (!bout) continue;
            Assert.IsTrue(ready,
                $"slot {slot}: bouts ({boutClip}) must be separated by " +
                "ready pauses, not swing continuously");
            return;
        }
        Assert.Fail("no unpaired bout slot found");
    }

    [Test]
    public void MeleeCasualties_KeepTheSoberWoundSystem()
    {
        // falls inside a melee segment come from the compiled schedule
        // and play the standard articulated falls (no melee-specific
        // death, no new wound class); grapplers fall AT the clinch
        int checkedCount = 0;
        foreach (var uid in new[] { Csa, Us })
        {
            var ur = Ctx.Unit(uid);
            for (int slot = 0; slot < ur.slotCount; slot++)
            {
                float fallT = ur.casualties[slot].fallT;
                if (float.IsInfinity(fallT)) continue;
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, fallT + 0.05f);
                Assert.AreEqual(SoldierState.StatusFalling, s.status);
                Assert.IsTrue(
                    s.clip == ClipId.FallBack ||
                    s.clip == ClipId.FallCrumple ||
                    s.clip == ClipId.FallSide,
                    $"{uid}/{slot}: melee fall plays a standard fall " +
                    $"clip (got {s.clip})");
                Assert.AreEqual(
                    (byte)CasualtySchedule.Wound(ur.casualties[slot].cause),
                    s.wound);
                // and persists
                var end = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, Ctx.bundle.slice.t1);
                Assert.IsTrue(end.Fallen);
                checkedCount++;
            }
        }
        Assert.Greater(checkedCount, 10);
    }

    [Test]
    public void MidTier_MeleeClips_ReadAsTheScrum()
    {
        foreach (var c in new[] { ClipId.MeleeClubSwing,
            ClipId.MeleeBayonetThrust, ClipId.MeleeGrappleA,
            ClipId.MeleeGrappleB, ClipId.MeleeParry })
            Assert.AreEqual("pose_melee", CrowdTiers.MidPose(c, 0.8f, 5, 30f));
    }

    [Test]
    public void Resolve_IsRepeatable_Bitwise_InTheMeleeWindow()
    {
        foreach (var uid in new[] { Csa, Us })
        {
            var ur = Ctx.Unit(uid);
            foreach (float t in new[] { 6.5f, 21f, 47.25f, 89.5f })
            {
                for (int slot = 0; slot < ur.slotCount; slot += 7)
                {
                    var a = SoldierActionResolver.Resolve(
                        Ctx, ur.unitIndex, slot, t);
                    var b = SoldierActionResolver.Resolve(
                        Ctx, ur.unitIndex, slot, t);
                    Assert.AreEqual(a.clip, b.clip);
                    Assert.AreEqual(a.clipTime, b.clipTime);
                    Assert.AreEqual(a.posX, b.posX);
                    Assert.AreEqual(a.posZ, b.posZ);
                    Assert.AreEqual(a.facingDeg, b.facingDeg);
                    Assert.AreEqual(a.status, b.status);
                }
            }
        }
    }

    [Test]
    public void ClipTimes_WithinDuration_InTheMeleeWindow()
    {
        foreach (var uid in new[] { Csa, Us })
        {
            var ur = Ctx.Unit(uid);
            for (float t = 0f; t <= 120f; t += 3.7f)
            {
                for (int slot = 0; slot < ur.slotCount; slot += 5)
                {
                    var s = SoldierActionResolver.Resolve(
                        Ctx, ur.unitIndex, slot, t);
                    Assert.GreaterOrEqual(s.clipTime, 0f,
                        $"{uid}/{slot}@{t} clip {s.clip}");
                    Assert.LessOrEqual(s.clipTime,
                        KitClips.Duration(s.clip) + 1e-3f,
                        $"{uid}/{slot}@{t} clip {s.clip}");
                }
            }
        }
    }

    [Test]
    public void UnwiredMelee_StillResolves_WithoutPairs()
    {
        // a melee segment with no opponent wired (or one-sided wiring)
        // must fall back to bout work — never throw, never grapple
        var lone = V2VocabTestRig.Unit(
            "lone-line", "confederate", 0f, 40f, 4000f, 8508f, 180f, 40,
            new List<AngleBundleSegment>
            {
                V2VocabTestRig.Seg("s-lone", "melee", 0f, 40f),
            });
        var c = V2VocabTestRig.Compile(
            "melee-lone-seed", 0f, 40f, null, lone);
        var ur = c.Unit("lone-line");
        var seen = new HashSet<ClipId>();
        for (int slot = 0; slot < ur.slotCount; slot += 2)
            for (float t = 5f; t < 38f; t += 1.3f)
                seen.Add(SoldierActionResolver.Resolve(
                    c, ur.unitIndex, slot, t).clip);
        Assert.IsFalse(seen.Contains(ClipId.MeleeGrappleA));
        Assert.IsFalse(seen.Contains(ClipId.MeleeGrappleB));
        Assert.Contains(ClipId.MeleeClubSwing, new List<ClipId>(seen));
    }
}
