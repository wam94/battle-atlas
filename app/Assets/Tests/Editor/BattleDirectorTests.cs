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
}
