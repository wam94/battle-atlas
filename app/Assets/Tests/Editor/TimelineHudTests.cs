using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class TimelineHudTests
{
    [Test]
    public void IsTouchOverHud_TrueInsideBottomStrip()
    {
        // touch coords: y measured UP from screen bottom; HUD is the bottom strip
        Assert.IsTrue(TimelineHud.IsTouchOverHud(new Vector2(100f, 50f), hudHeightPx: 120f));
        Assert.IsFalse(TimelineHud.IsTouchOverHud(new Vector2(100f, 121f), hudHeightPx: 120f));
    }

    [Test]
    public void HudScale_NeverBelowOne()
    {
        Assert.AreEqual(1f, TimelineHud.HudScale(0f), 1e-3f);     // dpi unknown
        Assert.AreEqual(1f, TimelineHud.HudScale(120f), 1e-3f);   // low-dpi
        Assert.AreEqual(2.875f, TimelineHud.HudScale(460f), 1e-3f); // iPhone-class
    }

    // ---- selection citation line (the source drawer's display seed) ----

    [Test]
    public void SelectedUnitLine_CarriesTheBracketingStartKeyframesCitation()
    {
        // the citation rides the bracketing START keyframe, exactly the
        // formation/confidence carry rule — a segment cites its start
        var kfs = new List<KeyframeDto>
        {
            new KeyframeDto { t = 0f, citation = "OR XXVII/1 p.428" },
            new KeyframeDto { t = 600f, citation = "Haskell letter" },
        };
        Assert.AreEqual("OR XXVII/1 p.428", TimelineHud.CitationAt(kfs, -50f));
        Assert.AreEqual("OR XXVII/1 p.428", TimelineHud.CitationAt(kfs, 599f));
        Assert.AreEqual("Haskell letter", TimelineHud.CitationAt(kfs, 600f));
        Assert.AreEqual("Haskell letter", TimelineHud.CitationAt(kfs, 9999f));
        // name · echelon · strength-at-t rounded · confidence · citation
        Assert.AreEqual(
            "Webb's Brigade · brigade · 940 men · documented · OR XXVII/1 p.428",
            TimelineHud.SelectedUnitLine("Webb's Brigade",
                UnitSymbol.Echelon.Brigade, 939.6f, "documented",
                "OR XXVII/1 p.428"));
        Assert.AreEqual(
            "Cushing's Battery A · battery · 126 men · inferred · OR XXVII/1 p.428",
            TimelineHud.SelectedUnitLine("Cushing's Battery A",
                UnitSymbol.Echelon.Battery, 126f, "inferred",
                "OR XXVII/1 p.428"));
    }

    [Test]
    public void SelectedUnitLine_AbsentCitationReadsNoReliableRecord()
    {
        // the no-faking gate's display seed: where the record is silent,
        // the line SAYS so — it never invents a source
        Assert.AreEqual(
            "5th Alabama Battalion · regiment · 120 men · unknown · no reliable record",
            TimelineHud.SelectedUnitLine("5th Alabama Battalion",
                UnitSymbol.Echelon.Regiment, 120.4f, "unknown", null));
        Assert.AreEqual(
            "Artillery Reserve · park · 300 men · inferred · no reliable record",
            TimelineHud.SelectedUnitLine("Artillery Reserve",
                UnitSymbol.Echelon.Park, 300f, "inferred", ""));
    }
}
