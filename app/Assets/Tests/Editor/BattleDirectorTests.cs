using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class BattleDirectorTests
{
    [Test]
    public void SideColor_DistinguishesSidesAndDefaultsNeutral()
    {
        Assert.AreNotEqual(BattleDirector.SideColor("union"),
                           BattleDirector.SideColor("confederate"));
        Assert.AreEqual(Color.gray, BattleDirector.SideColor("martian"));
    }

    [Test]
    public void MarkerPose_PlacesBottomOnGroundAndConvertsFacing()
    {
        var state = new UnitState
        {
            posXZ = new Vector2(100f, 200f),
            facingDeg = 90f,
            strength = 1f,
            formation = "line",
        };
        var (pos, rot) = BattleDirector.MarkerPose(state, groundY: 50f, markerHeight: 3f);
        Assert.AreEqual(new Vector3(100f, 51.5f, 200f), pos);
        // facing 90 = east: marker forward should be world +X
        Vector3 fwd = rot * Vector3.forward;
        Assert.AreEqual(1f, fwd.x, 1e-3f);
        Assert.AreEqual(0f, fwd.z, 1e-3f);
    }

    [Test]
    public void FootprintSamplePoints_UnrotatedCornersSpanFrontageAndDepth()
    {
        var buffer = new Vector2[9];
        BattleDirector.FootprintSamplePoints(new Vector2(100f, 200f), 0f, 300f, 40f, buffer);
        Assert.AreEqual(100f, buffer[0].x, 1e-3f); // center first
        Assert.AreEqual(200f, buffer[0].y, 1e-3f);
        Assert.AreEqual(-50f, buffer[1].x, 1e-3f); // 100 - 300/2
        Assert.AreEqual(180f, buffer[1].y, 1e-3f); // 200 - 40/2
        Assert.AreEqual(250f, buffer[4].x, 1e-3f); // 100 + 300/2
        Assert.AreEqual(220f, buffer[4].y, 1e-3f); // 200 + 40/2
        Assert.AreEqual(100f, buffer[5].x, 1e-3f); // south edge midpoint
        Assert.AreEqual(180f, buffer[5].y, 1e-3f);
        Assert.AreEqual(-50f, buffer[7].x, 1e-3f); // west edge midpoint
        Assert.AreEqual(200f, buffer[7].y, 1e-3f);
    }

    [Test]
    public void FootprintSamplePoints_RotateWithFacing()
    {
        var buffer = new Vector2[9];
        // facing east (90 deg): the frontage axis swings from east-west to north-south
        BattleDirector.FootprintSamplePoints(Vector2.zero, 90f, 300f, 40f, buffer);
        // local corner (-150 east, -20 north) rotates to (-20 east, +150 north)
        Assert.AreEqual(-20f, buffer[1].x, 1e-3f);
        Assert.AreEqual(150f, buffer[1].y, 1e-3f);
    }

    [Test]
    public void RendersAtTier_SuppressionTruthTable()
    {
        // parentless & childless: every tier (the pre-family behavior, untouched)
        Assert.IsTrue(BattleDirector.RendersAtTier(false, false, BattleDirector.LodTier.Block));
        Assert.IsTrue(BattleDirector.RendersAtTier(false, false, BattleDirector.LodTier.Regiments));
        Assert.IsTrue(BattleDirector.RendersAtTier(false, false, BattleDirector.LodTier.Soldiers));
        // parent: far tier only — the near tiers belong to its children
        Assert.IsTrue(BattleDirector.RendersAtTier(false, true, BattleDirector.LodTier.Block));
        Assert.IsFalse(BattleDirector.RendersAtTier(false, true, BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersAtTier(false, true, BattleDirector.LodTier.Soldiers));
        // child: hidden while the parent's block represents the family
        Assert.IsFalse(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Block));
        Assert.IsTrue(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Regiments));
        Assert.IsTrue(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Soldiers));
    }

    [Test]
    public void EvaluateTier_FamilyTierKeyedOnParentCenterWithHysteresis()
    {
        // EvaluateTier sees ONE distance — the family parent's center — so a
        // family can never half-swap: children at any range share this result
        Assert.AreEqual(BattleDirector.LodTier.Block,
            BattleDirector.EvaluateTier(5000f, BattleDirector.LodTier.Block));
        // resolve in below the In distances...
        Assert.AreEqual(BattleDirector.LodTier.Regiments,
            BattleDirector.EvaluateTier(3900f, BattleDirector.LodTier.Block));
        Assert.AreEqual(BattleDirector.LodTier.Soldiers,
            BattleDirector.EvaluateTier(1400f, BattleDirector.LodTier.Regiments));
        // ...and hold through the hysteresis band on the way out
        Assert.AreEqual(BattleDirector.LodTier.Soldiers,
            BattleDirector.EvaluateTier(1600f, BattleDirector.LodTier.Soldiers));
        Assert.AreEqual(BattleDirector.LodTier.Regiments,
            BattleDirector.EvaluateTier(1700f, BattleDirector.LodTier.Soldiers));
        Assert.AreEqual(BattleDirector.LodTier.Regiments,
            BattleDirector.EvaluateTier(4200f, BattleDirector.LodTier.Regiments));
        Assert.AreEqual(BattleDirector.LodTier.Block,
            BattleDirector.EvaluateTier(4500f, BattleDirector.LodTier.Regiments));
    }

    [Test]
    public void RosterlessChildRendersMonolithicAtRegimentsTier()
    {
        // a roster-less child renders at the middle tier — but as a single
        // block marker, not sub-blocks
        Assert.IsTrue(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersRegimentSubBlocks(
            0, "line", BattleDirector.LodTier.Regiments));
        // a child WITH a roster in ordered formation may sub-block
        Assert.IsTrue(BattleDirector.RendersRegimentSubBlocks(
            2, "line", BattleDirector.LodTier.Regiments));
        // never in disordered formations, never at other tiers
        Assert.IsFalse(BattleDirector.RendersRegimentSubBlocks(
            2, "routed", BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersRegimentSubBlocks(
            2, "line", BattleDirector.LodTier.Block));
    }

    // ---- field-legibility pass ----

    // a track that moves east at 1 m/s for its first 100 s, then holds
    static UnitTrack MovingTrack() => new UnitTrack(new UnitDto
    {
        id = "test-mover",
        keyframes = new List<KeyframeDto>
        {
            new KeyframeDto { t = 0f, x = 0f, z = 0f, formation = "line", strength = 1f },
            new KeyframeDto { t = 100f, x = 100f, z = 0f, formation = "line", strength = 1f },
        },
    });

    // a track that never moves (single keyframe: StateAt clamps everywhere)
    static UnitTrack StaticTrack() => new UnitTrack(new UnitDto
    {
        id = "test-static",
        keyframes = new List<KeyframeDto>
        {
            new KeyframeDto { t = 0f, x = 500f, z = 500f, formation = "line", strength = 1f },
        },
    });

    [Test]
    public void BlockCenterAndHeight_FlatAndModerateReliefMatchTheOldStretch()
    {
        // flat ground: exactly the pre-clamp marker — MarkerHeight tall,
        // sitting on the ground (center = ground + height/2)
        var (centerY, height) = BattleDirector.BlockCenterAndHeight(12f, 12f, 6f);
        Assert.AreEqual(6f, height, 1e-3f);
        Assert.AreEqual(15f, centerY, 1e-3f);
        // moderate relief, under the clamp: the old min-to-max stretch —
        // bottom at the lowest ground, top clearance above the highest
        (centerY, height) = BattleDirector.BlockCenterAndHeight(0f, 5f, 6f);
        Assert.AreEqual(11f, height, 1e-3f);
        Assert.AreEqual(0f, centerY - height / 2f, 1e-3f);  // bottom = minY
        Assert.AreEqual(11f, centerY + height / 2f, 1e-3f); // top = maxY + clearance
    }

    [Test]
    public void BlockCenterAndHeight_SteepReliefClampsWithTopAnchored()
    {
        // 30 m of relief: height clamps to MaxBlockHeight, and the TOP face
        // keeps its clearance above the HIGHEST ground — the crest never
        // pokes through; the low end lifts off minY into the hillside
        var (centerY, height) = BattleDirector.BlockCenterAndHeight(0f, 30f, 6f);
        Assert.AreEqual(14f, height, 1e-3f);
        Assert.AreEqual(36f, centerY + height / 2f, 1e-3f); // top = maxY + clearance, always
        Assert.AreEqual(22f, centerY - height / 2f, 1e-3f); // bottom sunk into the slope
    }

    [Test]
    public void KindOf_LockedIdPrefixConvention()
    {
        // the full-cast plan's locked id convention, one case per rule
        Assert.AreEqual(BattleDirector.UnitKind.Artillery, BattleDirector.KindOf("us-btty-cushing"));
        Assert.AreEqual(BattleDirector.UnitKind.Artillery, BattleDirector.KindOf("csa-btty-reilly"));
        Assert.AreEqual(BattleDirector.UnitKind.Artillery, BattleDirector.KindOf("csa-bn-alexander"));
        Assert.AreEqual(BattleDirector.UnitKind.Artillery, BattleDirector.KindOf("us-arty-reserve-park"));
        Assert.AreEqual(BattleDirector.UnitKind.Cavalry, BattleDirector.KindOf("us-cav-farnsworth"));
        Assert.AreEqual(BattleDirector.UnitKind.Infantry, BattleDirector.KindOf("us-webb"));
        // csa-bn- must not swallow the infantry brigade it was named to dodge
        Assert.AreEqual(BattleDirector.UnitKind.Infantry, BattleDirector.KindOf("csa-garnett"));
        // prefixes, never substrings: the 5th Alabama BATTALION is infantry
        Assert.AreEqual(BattleDirector.UnitKind.Infantry, BattleDirector.KindOf("csa-5al-bn"));
    }

    [Test]
    public void KindGlyph_HeightAndShadeMultipliersPinned()
    {
        // the glyph constants are a visual contract — pin them
        Assert.AreEqual(0.55f, BattleDirector.ArtilleryHeightMul);
        Assert.AreEqual(0.8f, BattleDirector.CavalryHeightMul);
        Assert.AreEqual(0.75f, BattleDirector.ArtilleryShadeMul);
        Assert.AreEqual(1f, BattleDirector.KindHeightMul(BattleDirector.UnitKind.Infantry));
        Assert.AreEqual(BattleDirector.ArtilleryHeightMul,
            BattleDirector.KindHeightMul(BattleDirector.UnitKind.Artillery));
        Assert.AreEqual(BattleDirector.CavalryHeightMul,
            BattleDirector.KindHeightMul(BattleDirector.UnitKind.Cavalry));
        // on flat ground the multiplier IS the height ratio: artillery
        // markers stand at 55% of the infantry block
        var (_, infantry) = BattleDirector.BlockCenterAndHeight(0f, 0f, 6f);
        var (_, artillery) = BattleDirector.BlockCenterAndHeight(
            0f, 0f, 6f * BattleDirector.ArtilleryHeightMul);
        Assert.AreEqual(0.55f, artillery / infantry, 1e-3f);

        Color side = BattleDirector.SideColor("union");
        Color arty = BattleDirector.KindShade(side, BattleDirector.UnitKind.Artillery);
        Assert.AreEqual(side.r * 0.75f, arty.r, 1e-5f); // uniformly darker
        Assert.AreEqual(side.g * 0.75f, arty.g, 1e-5f);
        Assert.AreEqual(side.b * 0.75f, arty.b, 1e-5f);
        Color cav = BattleDirector.KindShade(side, BattleDirector.UnitKind.Cavalry);
        Assert.AreEqual(side.r * BattleDirector.CavalryWarmR, cav.r, 1e-5f); // warmer: r up...
        Assert.AreEqual(side.g, cav.g, 1e-5f);
        Assert.AreEqual(side.b * BattleDirector.CavalryWarmB, cav.b, 1e-5f); // ...b down
        Assert.AreEqual(side, BattleDirector.KindShade(side, BattleDirector.UnitKind.Infantry));
    }

    [Test]
    public void IsActiveAt_TruthTable()
    {
        var windows = new List<BattleDirector.EventWindow>
        {
            new BattleDirector.EventWindow(200f, 300f),
        };
        // moving-only: no events needed while the track position changes
        Assert.IsTrue(BattleDirector.IsActiveAt(MovingTrack(), null, 50f));
        // event-only: a static unit is active exactly while its window is live
        Assert.IsTrue(BattleDirector.IsActiveAt(StaticTrack(), windows, 250f));
        // neither: static, no live window (before, between-less, and after)
        Assert.IsFalse(BattleDirector.IsActiveAt(StaticTrack(), windows, 100f));
        Assert.IsFalse(BattleDirector.IsActiveAt(StaticTrack(), null, 250f));
        // a mover that has ARRIVED (track clamped past its last keyframe)
        // goes quiet like any static unit
        Assert.IsFalse(BattleDirector.IsActiveAt(MovingTrack(), null, 500f));
        // boundaries are INCLUSIVE — an event owns its own t0 and t1 instants
        Assert.IsTrue(BattleDirector.IsActiveAt(StaticTrack(), windows, 200f));
        Assert.IsTrue(BattleDirector.IsActiveAt(StaticTrack(), windows, 300f));
        Assert.IsFalse(BattleDirector.IsActiveAt(StaticTrack(), windows, 199.5f));
        Assert.IsFalse(BattleDirector.IsActiveAt(StaticTrack(), windows, 300.5f));
    }

    [Test]
    public void InactiveColor_DesaturatesTowardFieldGrayByPinnedFactor()
    {
        Assert.AreEqual(0.55f, BattleDirector.InactiveDesat);
        Color side = BattleDirector.SideColor("confederate");
        Color inactive = BattleDirector.InactiveColor(side);
        // exactly the documented lerp — deterministic, no time term
        Assert.AreEqual(Color.Lerp(side, BattleDirector.FieldGray,
            BattleDirector.InactiveDesat), inactive);
        // recedes without vanishing: not the side color, not fully gray
        Assert.AreNotEqual(side, inactive);
        Assert.AreNotEqual(BattleDirector.FieldGray, inactive);
    }
}
