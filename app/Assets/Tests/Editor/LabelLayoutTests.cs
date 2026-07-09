using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class LabelLayoutTests
{
    static float Priority(UnitSymbol.Echelon e, bool active, bool selected, string id) =>
        LabelLayout.Priority(e, active, selected, id);

    [Test]
    public void Priority_SalienceLadderRankBands()
    {
        // plan D3: selected > active brigades > inactive brigades > active
        // regiments/batteries > rest — ranks are integer bands, the FNV
        // fraction stays inside [0, 0.5) so bands can never interleave
        Assert.AreEqual(LabelLayout.SelectedPriority,
            Priority(UnitSymbol.Echelon.Regiment, false, true, "us-webb-69pa"));
        Assert.Less(LabelLayout.SelectedPriority, 0f);
        float activeBrigade = Priority(UnitSymbol.Echelon.Brigade, true, false, "us-webb");
        float activePark = Priority(UnitSymbol.Echelon.Park, true, false, "us-arty-reserve-park");
        float inactiveBrigade = Priority(UnitSymbol.Echelon.Brigade, false, false, "us-webb");
        float activeRegiment = Priority(UnitSymbol.Echelon.Regiment, true, false, "us-webb-69pa");
        float activeBattery = Priority(UnitSymbol.Echelon.Battery, true, false, "us-btty-cushing");
        float inactiveRegiment = Priority(UnitSymbol.Echelon.Regiment, false, false, "us-webb-69pa");
        float inactiveBattery = Priority(UnitSymbol.Echelon.Battery, false, false, "us-btty-cushing");
        // the park labels with the brigades — it is Block-tier furniture
        Assert.AreEqual(0, Mathf.FloorToInt(activeBrigade));
        Assert.AreEqual(0, Mathf.FloorToInt(activePark));
        Assert.AreEqual(1, Mathf.FloorToInt(inactiveBrigade));
        Assert.AreEqual(2, Mathf.FloorToInt(activeRegiment));
        Assert.AreEqual(2, Mathf.FloorToInt(activeBattery));
        Assert.AreEqual(3, Mathf.FloorToInt(inactiveRegiment));
        Assert.AreEqual(3, Mathf.FloorToInt(inactiveBattery));
        // fraction bounded: a rank's worst hash never reaches the next band
        Assert.Less(activeBrigade, 0.5f);
        Assert.Less(inactiveRegiment, 3.5f);
    }

    [Test]
    public void Priority_FnvTieBreakIsDeterministicWithinARank()
    {
        // same inputs -> the identical float (scrub replay, orbit stability)
        float a1 = Priority(UnitSymbol.Echelon.Brigade, true, false, "us-webb");
        float a2 = Priority(UnitSymbol.Echelon.Brigade, true, false, "us-webb");
        Assert.AreEqual(a1, a2);
        // same rank, different unit -> a strict total order, not a tie
        float b = Priority(UnitSymbol.Echelon.Brigade, true, false, "csa-garnett");
        Assert.AreNotEqual(a1, b);
        Assert.AreEqual(Mathf.FloorToInt(a1), Mathf.FloorToInt(b));
    }

    [Test]
    public void Declutter_GreedyAabbRejectionByPriority()
    {
        var priorities = new[] { 0.1f, 0.2f, 0.3f };
        var rects = new[]
        {
            new Rect(0f, 0f, 100f, 20f),
            new Rect(50f, 0f, 100f, 20f),  // overlaps the winner
            new Rect(500f, 0f, 100f, 20f), // clear of everything
        };
        var shown = new bool[3];
        var eff = new float[3];
        var order = new int[3];
        var results = new int[3];
        int n = LabelLayout.Declutter(3, priorities, rects, shown,
            LabelLayout.StickyBonus, 8, eff, order, shown, results);
        Assert.AreEqual(2, n);
        Assert.IsTrue(shown[0]);
        Assert.IsFalse(shown[1]); // rejected by the higher-priority overlap
        Assert.IsTrue(shown[2]);
        // results come out in priority order — the pool assignment contract
        Assert.AreEqual(0, results[0]);
        Assert.AreEqual(2, results[1]);
    }

    [Test]
    public void Declutter_RespectsBudgetAndNeverShowsAbsentSlots()
    {
        // slot layout is FIXED per unit; a slot whose unit didn't render
        // this frame carries +inf priority and must never show
        var priorities = new[] { 0.4f, 0.3f, 0.2f, 0.1f, float.PositiveInfinity };
        var rects = new Rect[5];
        for (int i = 0; i < 5; i++) rects[i] = new Rect(i * 200f, 0f, 100f, 20f);
        var shown = new bool[5];
        var eff = new float[5];
        var order = new int[5];
        var results = new int[5];
        int n = LabelLayout.Declutter(5, priorities, rects, shown,
            LabelLayout.StickyBonus, 2, eff, order, shown, results);
        Assert.AreEqual(2, n); // budget bites: only the two best survive
        Assert.IsTrue(shown[3]);
        Assert.IsTrue(shown[2]);
        Assert.IsFalse(shown[0]);
        // with room to spare the absent slot still never appears
        n = LabelLayout.Declutter(5, priorities, rects, shown,
            LabelLayout.StickyBonus, 8, eff, order, shown, results);
        Assert.AreEqual(4, n);
        Assert.IsFalse(shown[4]);
    }

    [Test]
    public void Declutter_StickyBonusPreventsFlipFlopAtTheMargin()
    {
        var priorities = new[] { 1.0f, 0.9f };
        var rects = new[] { new Rect(0f, 0f, 100f, 20f), new Rect(50f, 0f, 100f, 20f) };
        // the shown array doubles as last frame's state (the documented
        // aliasing contract): frame 1 starts cold, B wins on raw priority
        var shown = new bool[2];
        var eff = new float[2];
        var order = new int[2];
        var results = new int[2];
        LabelLayout.Declutter(2, priorities, rects, shown,
            LabelLayout.StickyBonus, 1, eff, order, shown, results);
        Assert.IsFalse(shown[0]);
        Assert.IsTrue(shown[1]);
        // frame 2: A edges ahead on raw priority — but by less than the
        // bonus, so the shown label holds and nothing flickers
        priorities[0] = 0.85f;
        LabelLayout.Declutter(2, priorities, rects, shown,
            LabelLayout.StickyBonus, 1, eff, order, shown, results);
        Assert.IsFalse(shown[0]);
        Assert.IsTrue(shown[1]);
        // same frame without the bonus flips — the bonus IS the hysteresis
        LabelLayout.Declutter(2, priorities, rects, shown,
            0f, 1, eff, order, shown, results);
        Assert.IsTrue(shown[0]);
        Assert.IsFalse(shown[1]);
    }

    [Test]
    public void Declutter_SelectedAlwaysSurvives()
    {
        // selected sorts ahead of everything — even a sticky-boosted active
        // brigade sitting on the same pixels with only one budget slot
        var priorities = new[]
        {
            LabelLayout.Priority(UnitSymbol.Echelon.Regiment, false, true, "us-webb-69pa"),
            LabelLayout.Priority(UnitSymbol.Echelon.Brigade, true, false, "us-webb"),
        };
        var rects = new[] { new Rect(0f, 0f, 100f, 20f), new Rect(0f, 0f, 100f, 20f) };
        var shownLast = new[] { false, true }; // the challenger holds the spot
        var shown = new bool[2];
        var eff = new float[2];
        var order = new int[2];
        var results = new int[2];
        int n = LabelLayout.Declutter(2, priorities, rects, shownLast,
            LabelLayout.StickyBonus, 1, eff, order, shown, results);
        Assert.AreEqual(1, n);
        Assert.IsTrue(shown[0]);
        Assert.IsFalse(shown[1]);
    }

    [Test]
    public void LabelsAtTier_VisibilityTable()
    {
        // Block tier: brigades and the park always; batteries/regiments
        // earn a label by acting or being selected (plan D3)
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Block, UnitSymbol.Echelon.Brigade, false, false));
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Block, UnitSymbol.Echelon.Park, false, false));
        Assert.IsFalse(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Block, UnitSymbol.Echelon.Battery, false, false));
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Block, UnitSymbol.Echelon.Battery, true, false));
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Block, UnitSymbol.Echelon.Battery, false, true));
        Assert.IsFalse(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Block, UnitSymbol.Echelon.Regiment, false, false));
        // Regiments tier: every rendering family member is a candidate —
        // the budget and the declutter do the trimming, not the tier rule
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Regiments, UnitSymbol.Echelon.Regiment, false, false));
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Regiments, UnitSymbol.Echelon.Battery, false, false));
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Regiments, UnitSymbol.Echelon.Brigade, false, false));
        // Soldiers tier: selected only — close zoom is deliberate attention
        Assert.IsFalse(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Soldiers, UnitSymbol.Echelon.Brigade, true, false));
        Assert.IsTrue(LabelLayout.LabelsAtTier(
            BattleDirector.LodTier.Soldiers, UnitSymbol.Echelon.Regiment, false, true));
    }

    [Test]
    public void LabelScale_ConstantScreenSizeIsLinearAboveTheFloor()
    {
        // world scale grows linearly with distance = constant screen size
        Assert.Greater(LabelLayout.LabelScale(500f), 0f);
        Assert.Less(LabelLayout.LabelScale(500f), LabelLayout.LabelScale(2000f));
        Assert.AreEqual(4f * LabelLayout.LabelScale(1000f),
            LabelLayout.LabelScale(4000f), 1e-4f);
        // floored below MinScaleDistance so a fly-through never shrinks the
        // text to nothing at the anchor
        Assert.AreEqual(LabelLayout.LabelScale(LabelLayout.MinScaleDistance),
            LabelLayout.LabelScale(1f), 1e-6f);
    }
}
