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
        // monolithic symbol, not a roster partition
        Assert.IsTrue(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersRosterSymbols(
            0, "line", BattleDirector.LodTier.Regiments));
        // a child WITH a roster in ordered formation may partition
        Assert.IsTrue(BattleDirector.RendersRosterSymbols(
            2, "line", BattleDirector.LodTier.Regiments));
        // never in disordered formations, never at other tiers
        Assert.IsFalse(BattleDirector.RendersRosterSymbols(
            2, "routed", BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersRosterSymbols(
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

    // ---- symbol integration (slabs out, symbols in) ----

    [Test]
    public void SymbolStyle_MpbSelectionTruthTable()
    {
        // the shader contract: the FillStyle enum ints ARE the _FillStyle
        // MPB floats — 0 solid, 1 hatched, 2 reserved cross-hatch
        Assert.AreEqual(0f, (float)UnitSymbol.FillStyle.Solid);
        Assert.AreEqual(1f, (float)UnitSymbol.FillStyle.Hatched);
        Assert.AreEqual(2f, (float)UnitSymbol.FillStyle.Contested);
        Assert.AreEqual(0f, (float)UnitSymbol.StyleOf("documented"));
        Assert.AreEqual(1f, (float)UnitSymbol.StyleOf("inferred"));
        Assert.AreEqual(1f, (float)UnitSymbol.StyleOf("unknown"));
        // border weights: the brigade sits past the shader's double-line
        // threshold (> 1 draws the double border), the regiment is the
        // thin single line, and the battery baseline / park outline draw
        // the full strip — their echelon mark is the symbol class itself
        Assert.AreEqual(BattleDirector.BrigadeBorderWeight,
            BattleDirector.SymbolBorderWeight(UnitSymbol.Echelon.Brigade));
        Assert.Greater(BattleDirector.BrigadeBorderWeight, 1f);
        Assert.AreEqual(BattleDirector.RegimentBorderWeight,
            BattleDirector.SymbolBorderWeight(UnitSymbol.Echelon.Regiment));
        Assert.Less(BattleDirector.RegimentBorderWeight, 1f);
        Assert.Greater(BattleDirector.RegimentBorderWeight, 0f);
        Assert.AreEqual(BattleDirector.FullBorderWeight,
            BattleDirector.SymbolBorderWeight(UnitSymbol.Echelon.Battery));
        Assert.AreEqual(BattleDirector.FullBorderWeight,
            BattleDirector.SymbolBorderWeight(UnitSymbol.Echelon.Park));
    }

    [Test]
    public void RosterSymbolSpecs_RegimentSpecsSplitStrengthAndSumToParent()
    {
        var s = new UnitState
        {
            posXZ = new Vector2(1000f, 2000f), facingDeg = 0f,
            strength = 1700f, formation = "line",
        };
        var slots = new (Vector2 center, Vector2 size)[4];
        var specs = new BattleDirector.RosterSymbolSpec[4];
        int count = BattleDirector.RosterSymbolSpecs(s, 300f, 40f, 4, slots, specs);
        Assert.AreEqual(4, count);
        float shareSum = 0f;
        float expectedWidth = (300f - 6f * 3) / 4f; // RegimentSlots' line partition
        for (int i = 0; i < count; i++)
        {
            shareSum += specs[i].StrengthShare;
            // slot frontage is the partition's width, thickness the share's
            Assert.AreEqual(expectedWidth, specs[i].Frontage, 1e-3f);
            Assert.AreEqual(
                UnitSymbol.DisplayDepth(1700f / 4f, expectedWidth),
                specs[i].DisplayDepth, 1e-4f);
        }
        // the even split is display-inferred, but it must never invent or
        // lose men: shares sum exactly to the parent's keyframed strength
        Assert.AreEqual(1700f, shareSum, 1e-3f);
        // facing 0: slot centers ride the world X axis, right (+x) first
        // per the roster's right-to-left convention, all on the unit line
        Assert.Greater(specs[0].CenterXZ.x, specs[3].CenterXZ.x);
        for (int i = 0; i < count; i++)
            Assert.AreEqual(2000f, specs[i].CenterXZ.y, 1e-3f);
    }

    [Test]
    public void RosterSymbolSpecs_CentersRotateWithFacing()
    {
        var s = new UnitState
        {
            posXZ = Vector2.zero, facingDeg = 90f,
            strength = 800f, formation = "line",
        };
        var slots = new (Vector2 center, Vector2 size)[2];
        var specs = new BattleDirector.RosterSymbolSpec[2];
        BattleDirector.RosterSymbolSpecs(s, 200f, 40f, 2, slots, specs);
        // facing east (90 deg): the frontage axis swings from east-west to
        // north-south — local +x (right of the line) maps to world -z,
        // exactly the FootprintSamplePoints rotation convention
        float width = (200f - 6f) / 2f;
        float localX = 200f / 2f - width / 2f; // slot 0 center, +x end
        Assert.AreEqual(0f, specs[0].CenterXZ.x, 1e-3f);
        Assert.AreEqual(-localX, specs[0].CenterXZ.y, 1e-3f);
        Assert.AreEqual(localX, specs[1].CenterXZ.y, 1e-3f);
    }

    [Test]
    public void SymbolNeedsRebuild_DelegatesToTheSharedDirtyPredicate()
    {
        UnitState a = new UnitState
        {
            posXZ = new Vector2(100f, 200f), facingDeg = 90f,
            strength = 1000f, formation = "line",
        };
        UnitState moved = a;
        moved.posXZ = new Vector2(100.5f, 200f);
        // static twice at the same tier: keep last frame's mesh
        Assert.IsFalse(BattleDirector.SymbolNeedsRebuild(a, a, 300f,
            UnitSymbol.SymbolKind.Infantry,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block));
        // moved past epsilon, or tier flip: rebuild
        Assert.IsTrue(BattleDirector.SymbolNeedsRebuild(a, moved, 300f,
            UnitSymbol.SymbolKind.Infantry,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block));
        Assert.IsTrue(BattleDirector.SymbolNeedsRebuild(a, a, 300f,
            UnitSymbol.SymbolKind.Infantry,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Regiments));
    }

    [Test]
    public void SymbolNeedsRebuild_ArtilleryGunDotStepDirtiesUnderTheDepthClamp()
    {
        // a battery encodes strength as gun-dot COUNT: its display depth
        // sits pinned at the MinDepth clamp (120 men x 6 m² / 120 m = 6 m
        // -> clamped to 8), so the shared thickness gate never fires — the
        // dot step must dirty instead, or a bleeding battery never redraws
        UnitState full = new UnitState
        {
            posXZ = new Vector2(100f, 200f), facingDeg = 0f,
            strength = 120f, formation = "line",
        };
        UnitState bled = full;
        bled.strength = 80f; // 6 dots -> 4 dots, same clamped depth
        Assert.AreEqual(
            UnitSymbol.DisplayDepth(full.strength, 120f),
            UnitSymbol.DisplayDepth(bled.strength, 120f));
        Assert.IsTrue(BattleDirector.SymbolNeedsRebuild(full, bled, 120f,
            UnitSymbol.SymbolKind.Artillery,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block));
        // the same strengths on an infantry bar stay clean — thickness is
        // its strength channel and the clamp holds it still
        Assert.IsFalse(BattleDirector.SymbolNeedsRebuild(full, bled, 120f,
            UnitSymbol.SymbolKind.Infantry,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block));
    }

    [Test]
    public void FamilyAndRoster_SymbolRepresentationTruthTable()
    {
        // family suppression holds with symbols: the parent's ribbon IS the
        // family at Block tier, the children's ribbons are at Regiments —
        // and the roster partition only ever augments the Regiments tier
        Assert.IsTrue(BattleDirector.RendersAtTier(false, true, BattleDirector.LodTier.Block));
        Assert.IsFalse(BattleDirector.RendersAtTier(false, true, BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Block));
        Assert.IsTrue(BattleDirector.RendersAtTier(true, false, BattleDirector.LodTier.Regiments));
        // a roster brigade at Block renders ONE brigade-grain ribbon (no
        // partition); at Regiments it partitions in ordered formations only
        Assert.IsFalse(BattleDirector.RendersRosterSymbols(
            5, "line", BattleDirector.LodTier.Block));
        Assert.IsTrue(BattleDirector.RendersRosterSymbols(
            5, "line", BattleDirector.LodTier.Regiments));
        Assert.IsTrue(BattleDirector.RendersRosterSymbols(
            5, "column", BattleDirector.LodTier.Regiments));
        Assert.IsFalse(BattleDirector.RendersRosterSymbols(
            5, "scattered", BattleDirector.LodTier.Regiments));
        // and never at the Soldiers tier — figures are the close-zoom truth
        Assert.IsFalse(BattleDirector.RendersRosterSymbols(
            5, "line", BattleDirector.LodTier.Soldiers));
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
