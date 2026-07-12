using NUnit.Framework;
using BattleAtlas;

public class UnitSymbolTests
{
    [Test]
    public void KindOf_LockedPrefixTableWithParkSplit()
    {
        Assert.AreEqual(UnitSymbol.SymbolKind.Artillery, UnitSymbol.KindOf("us-btty-cushing"));
        Assert.AreEqual(UnitSymbol.SymbolKind.Artillery, UnitSymbol.KindOf("csa-btty-reilly"));
        Assert.AreEqual(UnitSymbol.SymbolKind.Artillery, UnitSymbol.KindOf("csa-bn-alexander"));
        // the park split the slab renderer never made: us-arty- is the
        // non-combat Artillery Reserve park, not a firing battery
        Assert.AreEqual(UnitSymbol.SymbolKind.ArtilleryPark,
            UnitSymbol.KindOf("us-arty-reserve-park"));
        Assert.AreEqual(UnitSymbol.SymbolKind.Cavalry, UnitSymbol.KindOf("us-cav-farnsworth"));
        Assert.AreEqual(UnitSymbol.SymbolKind.Infantry, UnitSymbol.KindOf("us-webb"));
        // Garnett's INFANTRY brigade: csa-bn- must not match by substring
        Assert.AreEqual(UnitSymbol.SymbolKind.Infantry, UnitSymbol.KindOf("csa-garnett"));
        // 5th Alabama Battalion: -bn suffix, infantry — prefixes, never substrings
        Assert.AreEqual(UnitSymbol.SymbolKind.Infantry, UnitSymbol.KindOf("csa-5al-bn"));
    }

    [Test]
    public void KindOf_BattleDirectorDelegatesAndCollapsesPark()
    {
        // single decoder rule: the director's three glyph buckets are
        // exactly the symbol taxonomy with the park folded into Artillery
        foreach (string id in new[]
        {
            "us-btty-cushing", "csa-btty-reilly", "csa-bn-alexander",
            "us-arty-reserve-park", "us-cav-farnsworth", "us-webb",
            "csa-garnett", "csa-5al-bn",
        })
        {
            UnitSymbol.SymbolKind kind = UnitSymbol.KindOf(id);
            BattleDirector.UnitKind expected =
                kind == UnitSymbol.SymbolKind.Cavalry ? BattleDirector.UnitKind.Cavalry
                : kind == UnitSymbol.SymbolKind.Infantry ? BattleDirector.UnitKind.Infantry
                : BattleDirector.UnitKind.Artillery;
            Assert.AreEqual(expected, BattleDirector.KindOf(id), id);
        }
    }

    [Test]
    public void EchelonOf_TruthTable()
    {
        // the kinds that carry their own echelon grammar win outright
        Assert.AreEqual(UnitSymbol.Echelon.Park, UnitSymbol.EchelonOf(
            false, false, false, UnitSymbol.SymbolKind.ArtilleryPark));
        Assert.AreEqual(UnitSymbol.Echelon.Park, UnitSymbol.EchelonOf(
            true, true, true, UnitSymbol.SymbolKind.ArtilleryPark));
        Assert.AreEqual(UnitSymbol.Echelon.Battery, UnitSymbol.EchelonOf(
            false, false, false, UnitSymbol.SymbolKind.Artillery));
        Assert.AreEqual(UnitSymbol.Echelon.Battery, UnitSymbol.EchelonOf(
            false, false, true, UnitSymbol.SymbolKind.Artillery));
        // a decomposed child is a regiment, whatever the arm
        Assert.AreEqual(UnitSymbol.Echelon.Regiment, UnitSymbol.EchelonOf(
            false, false, true, UnitSymbol.SymbolKind.Infantry));
        Assert.AreEqual(UnitSymbol.Echelon.Regiment, UnitSymbol.EchelonOf(
            false, false, true, UnitSymbol.SymbolKind.Cavalry));
        // roster, children, or the parentless macro default: brigade
        Assert.AreEqual(UnitSymbol.Echelon.Brigade, UnitSymbol.EchelonOf(
            true, false, false, UnitSymbol.SymbolKind.Infantry));
        Assert.AreEqual(UnitSymbol.Echelon.Brigade, UnitSymbol.EchelonOf(
            false, true, false, UnitSymbol.SymbolKind.Infantry));
        Assert.AreEqual(UnitSymbol.Echelon.Brigade, UnitSymbol.EchelonOf(
            false, false, false, UnitSymbol.SymbolKind.Infantry));
        Assert.AreEqual(UnitSymbol.Echelon.Brigade, UnitSymbol.EchelonOf(
            false, false, false, UnitSymbol.SymbolKind.Cavalry));
    }

    [Test]
    public void DisplayDepth_AreaProportionalBetweenClamps()
    {
        // 6 m² per effective: depth = strength * 6 / frontage
        Assert.AreEqual(20f, UnitSymbol.DisplayDepth(1000f, 300f), 1e-4f);
        Assert.AreEqual(10f, UnitSymbol.DisplayDepth(500f, 300f), 1e-4f);
        // halving frontage doubles thickness — area is conserved
        Assert.AreEqual(40f, UnitSymbol.DisplayDepth(1000f, 150f), 1e-4f);
    }

    [Test]
    public void DisplayDepth_ClampsThinSliversToMin()
    {
        // 100 men on 300 m would be a 2 m film — floor at 8 m so it reads
        Assert.AreEqual(UnitSymbol.MinDepthM, UnitSymbol.DisplayDepth(100f, 300f), 1e-4f);
        Assert.AreEqual(UnitSymbol.MinDepthM, UnitSymbol.DisplayDepth(0f, 300f), 1e-4f);
    }

    [Test]
    public void DisplayDepth_ClampsMassedColumnsToMax()
    {
        // 2000 men on 100 m would be 120 m of ink — cap at 48 m
        Assert.AreEqual(UnitSymbol.MaxDepthM, UnitSymbol.DisplayDepth(2000f, 100f), 1e-4f);
    }

    [Test]
    public void DisplayDepth_QuantizesToHalfMeterSteps()
    {
        // strength drift inside a quantum yields the identical depth, so
        // interpolation doesn't dirty the mesh every frame
        Assert.AreEqual(
            UnitSymbol.DisplayDepth(1000f, 300f),
            UnitSymbol.DisplayDepth(1001f, 300f));
        // 1013 * 6 / 300 = 20.26 -> nearest half meter is 20.5
        Assert.AreEqual(20.5f, UnitSymbol.DisplayDepth(1013f, 300f), 1e-4f);
    }

    [Test]
    public void DisplayDepth_BrigadeVisiblyOutweighsSmallRegiment()
    {
        // the user requirement, pinned: a 300-man/150 m regiment is a
        // sliver next to a 1,700-man/300 m brigade
        float regiment = UnitSymbol.DisplayDepth(300f, 150f);  // 12 m
        float brigade = UnitSymbol.DisplayDepth(1700f, 300f);  // 34 m
        Assert.AreEqual(12f, regiment, 1e-4f);
        Assert.AreEqual(34f, brigade, 1e-4f);
        Assert.Greater(brigade, regiment * 2f);
    }

    [Test]
    public void GunDotCount_ApproximatesGunsAndClamps()
    {
        Assert.AreEqual(4, UnitSymbol.GunDotCount(80f));   // ~20 men per gun
        Assert.AreEqual(6, UnitSymbol.GunDotCount(120f));
        Assert.AreEqual(UnitSymbol.MinGunDots, UnitSymbol.GunDotCount(10f));
        Assert.AreEqual(UnitSymbol.MaxGunDots, UnitSymbol.GunDotCount(1000f));
    }

    [Test]
    public void StyleOf_DocumentedRendersSolid()
    {
        Assert.AreEqual(UnitSymbol.FillStyle.Solid, UnitSymbol.StyleOf("documented"));
    }

    [Test]
    public void StyleOf_InferredAndUnknownRenderHatched()
    {
        // USER RULING 2026-07-09: two states only — anything not
        // documented (inferred, unknown, the empty default) hatches
        Assert.AreEqual(UnitSymbol.FillStyle.Hatched, UnitSymbol.StyleOf("inferred"));
        Assert.AreEqual(UnitSymbol.FillStyle.Hatched, UnitSymbol.StyleOf("unknown"));
        Assert.AreEqual(UnitSymbol.FillStyle.Hatched, UnitSymbol.StyleOf(""));
        Assert.AreEqual(UnitSymbol.FillStyle.Hatched, UnitSymbol.StyleOf(null));
    }

    [Test]
    public void FillInk_ArtilleryPrintsDarkEverythingElseFull()
    {
        // cartography slice 2: the period-map "batteries in black"
        // convention as a fill multiplier — dark enough to read as ink at
        // theater zoom, side hue preserved up close
        Assert.AreEqual(UnitSymbol.ArtilleryFillInk,
            UnitSymbol.FillInk(UnitSymbol.SymbolKind.Artillery));
        Assert.Less(UnitSymbol.ArtilleryFillInk, 0.6f);
        Assert.Greater(UnitSymbol.ArtilleryFillInk, 0.2f);
        Assert.AreEqual(1f, UnitSymbol.FillInk(UnitSymbol.SymbolKind.Infantry));
        Assert.AreEqual(1f, UnitSymbol.FillInk(UnitSymbol.SymbolKind.Cavalry));
        Assert.AreEqual(1f, UnitSymbol.FillInk(UnitSymbol.SymbolKind.ArtilleryPark));
    }
}
