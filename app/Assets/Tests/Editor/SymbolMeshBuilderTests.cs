using System;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class SymbolMeshBuilderTests
{
    const float Lift = SymbolMeshBuilder.DefaultLiftM;

    static Vector3[] NewVerts() => new Vector3[SymbolMeshBuilder.MaxSymbolVerts];
    static Vector2[] NewUvs() => new Vector2[SymbolMeshBuilder.MaxSymbolVerts];
    static int[] NewTris() => new int[SymbolMeshBuilder.MaxSymbolIndices];

    static SymbolMeshBuilder.SymbolCounts Build(
        string unitId, UnitSymbol.SymbolKind kind, string formation,
        float frontage, float depth, Func<float, float, float> groundY,
        Vector3[] verts, Vector2[] uvs, int[] tris,
        Vector2 center = default, float facing = 0f, int gunDots = 4)
    {
        return SymbolMeshBuilder.BuildRibbon(
            unitId, center, facing, frontage, depth, kind, formation, gunDots,
            groundY, Lift, verts, uvs, tris);
    }

    static UnitState State(
        float x, float z, float facing, float strength, string formation) =>
        new UnitState
        {
            posXZ = new Vector2(x, z), facingDeg = facing,
            strength = strength, formation = formation,
        };

    [Test]
    public void FlatGround_RibbonSpansFrontageByDisplayDepth()
    {
        var verts = NewVerts();
        var counts = Build("us-webb", UnitSymbol.SymbolKind.Infantry, "line",
            300f, 20f, (x, z) => 0f, verts, NewUvs(), NewTris());
        Assert.Greater(counts.VertexCount, 0);
        Assert.Greater(counts.BodyIndexCount, 0);
        Assert.Greater(counts.BorderIndexCount, 0);
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        for (int i = 0; i < counts.VertexCount; i++)
        {
            minX = Mathf.Min(minX, verts[i].x); maxX = Mathf.Max(maxX, verts[i].x);
            minZ = Mathf.Min(minZ, verts[i].z); maxZ = Mathf.Max(maxZ, verts[i].z);
            Assert.AreEqual(Lift, verts[i].y, 1e-4f); // flat ground: lift only
        }
        // the attested frontage IS the length, the strength depth the
        // thickness — the border frame is inset, so nothing pokes past
        Assert.AreEqual(-150f, minX, 1e-3f);
        Assert.AreEqual(150f, maxX, 1e-3f);
        Assert.AreEqual(-10f, minZ, 1e-3f);
        Assert.AreEqual(10f, maxZ, 1e-3f);
    }

    [Test]
    public void Draping_EveryVertexSitsLiftAboveATwentyMeterRamp()
    {
        // the Culp's Hill assertion: on ~20 m of relief, a draped symbol
        // CANNOT stab into the slope or float off it — every vertex sits
        // exactly `lift` above the sampled ground, rotation included
        Func<float, float, float> ramp = (x, z) => (x - 250f) / 15f; // ~20 m over 300 m
        var verts = NewVerts();
        var counts = Build("us-webb", UnitSymbol.SymbolKind.Infantry, "line",
            300f, 20f, ramp, verts, NewUvs(), NewTris(),
            center: new Vector2(400f, 900f), facing: 37f);
        for (int i = 0; i < counts.VertexCount; i++)
        {
            float ground = ramp(verts[i].x, verts[i].z);
            Assert.AreEqual(Lift, verts[i].y - ground, 1e-3f,
                $"vertex {i} floats or stabs");
        }
    }

    [Test]
    public void Draping_IsVerticalScaleAgnostic()
    {
        // the exaggeration pin: the builder samples the DISPLAYED terrain,
        // so scaling the height field x2.5 (today's Atlas exaggeration)
        // moves every vertex onto the scaled relief and changes nothing else
        Func<float, float, float> g = (x, z) => Mathf.Sin(x * 0.01f) * 8f + z * 0.02f;
        Func<float, float, float> g25 = (x, z) => 2.5f * g(x, z);
        var vertsA = NewVerts();
        var vertsB = NewVerts();
        var countsA = Build("csa-garnett", UnitSymbol.SymbolKind.Infantry, "line",
            250f, 24f, g, vertsA, NewUvs(), NewTris(), facing: 200f);
        var countsB = Build("csa-garnett", UnitSymbol.SymbolKind.Infantry, "line",
            250f, 24f, g25, vertsB, NewUvs(), NewTris(), facing: 200f);
        Assert.AreEqual(countsA.VertexCount, countsB.VertexCount);
        Assert.AreEqual(countsA.BodyIndexCount, countsB.BodyIndexCount);
        Assert.AreEqual(countsA.BorderIndexCount, countsB.BorderIndexCount);
        for (int i = 0; i < countsA.VertexCount; i++)
        {
            Assert.AreEqual(vertsA[i].x, vertsB[i].x, 1e-4f);
            Assert.AreEqual(vertsA[i].z, vertsB[i].z, 1e-4f);
            Assert.AreEqual(2.5f * g(vertsA[i].x, vertsA[i].z) + Lift,
                vertsB[i].y, 1e-3f);
        }
    }

    [Test]
    public void ColumnCount_FiveMetersPerColumnClamped()
    {
        Assert.AreEqual(SymbolMeshBuilder.MinColumns, SymbolMeshBuilder.ColumnCount(10f));
        Assert.AreEqual(SymbolMeshBuilder.MaxColumns, SymbolMeshBuilder.ColumnCount(10000f));
        Assert.AreEqual(30, SymbolMeshBuilder.ColumnCount(150f));
        Assert.AreEqual(60, SymbolMeshBuilder.ColumnCount(300f));
    }

    [Test]
    public void Cavalry_FillShearsAtTheConstantAngle()
    {
        var verts = NewVerts();
        Build("us-cav-farnsworth", UnitSymbol.SymbolKind.Cavalry, "line",
            200f, 20f, (x, z) => 0f, verts, NewUvs(), NewTris());
        // fill grid is emitted first, row-major: vertex 0 is (col 0, rear
        // row), vertex (RowCount-1)*cols is (col 0, front row)
        int cols = SymbolMeshBuilder.ColumnCount(200f);
        Vector3 rear = verts[0];
        Vector3 front = verts[(SymbolMeshBuilder.RowCount - 1) * cols];
        float observedShear = (front.x - rear.x) / (front.z - rear.z);
        Assert.AreEqual(
            Mathf.Tan(SymbolMeshBuilder.CavalryShearDeg * Mathf.Deg2Rad),
            observedShear, 1e-3f);
    }

    [Test]
    public void Artillery_GunDotsCountAndSpaceEvenly()
    {
        var verts = NewVerts();
        var counts = Build("us-btty-cushing", UnitSymbol.SymbolKind.Artillery, "line",
            120f, 8f, (x, z) => 0f, verts, NewUvs(), NewTris(), gunDots: 6);
        // 6 dots, each a 3x3 draped grid of 2x2 cells: discrete squares,
        // not a continuous bar
        Assert.AreEqual(6 * 24, counts.BodyIndexCount);
        float spacing = (120f - SymbolMeshBuilder.GunDotSizeM) / 5f;
        float prevCenter = float.NaN;
        for (int d = 0; d < 6; d++)
        {
            float cx = 0f;
            for (int i = 0; i < 9; i++) cx += verts[d * 9 + i].x;
            cx /= 9f;
            if (d > 0) Assert.AreEqual(spacing, cx - prevCenter, 1e-3f);
            prevCenter = cx;
        }
    }

    [Test]
    public void Artillery_BattalionDrawsDoubleBaseline()
    {
        // approved mockup: battery gun-dots ride ONE baseline, an
        // artillery battalion's ride TWO (same LOCKED csa-bn- prefix)
        var battery = Build("us-btty-cushing", UnitSymbol.SymbolKind.Artillery,
            "line", 120f, 8f, (x, z) => 0f, NewVerts(), NewUvs(), NewTris(), gunDots: 6);
        var battalion = Build("csa-bn-alexander", UnitSymbol.SymbolKind.Artillery,
            "line", 120f, 8f, (x, z) => 0f, NewVerts(), NewUvs(), NewTris(), gunDots: 6);
        Assert.Greater(battery.BorderIndexCount, 0);
        Assert.AreEqual(2 * battery.BorderIndexCount, battalion.BorderIndexCount);
        Assert.AreEqual(battery.BodyIndexCount, battalion.BodyIndexCount);
    }

    [Test]
    public void Park_OutlineHasNoInteriorFillTris()
    {
        var verts = NewVerts();
        var counts = Build("us-arty-reserve-park", UnitSymbol.SymbolKind.ArtilleryPark,
            "line", 200f, 40f, (x, z) => 0f, verts, NewUvs(), NewTris());
        // hollow: the outline + inset cross are ALL border — zero fill
        // tris, so the park can never read as a solid firing line
        Assert.AreEqual(0, counts.BodyIndexCount);
        Assert.Greater(counts.BorderIndexCount, 0);
        for (int i = 0; i < counts.VertexCount; i++)
        {
            // outline reaches the footprint, the cross stays inset inside it
            Assert.LessOrEqual(Mathf.Abs(verts[i].x), 100f + 1e-3f);
            Assert.LessOrEqual(Mathf.Abs(verts[i].z), 20f + 1e-3f);
        }
    }

    [Test]
    public void Scattered_FragmentGapsAreDeterministicPerUnit()
    {
        var vertsA = NewVerts();
        var vertsB = NewVerts();
        var trisA = NewTris();
        var trisB = NewTris();
        var countsA = Build("csa-garnett", UnitSymbol.SymbolKind.Infantry, "scattered",
            300f, 20f, (x, z) => 0f, vertsA, NewUvs(), trisA);
        var countsB = Build("csa-garnett", UnitSymbol.SymbolKind.Infantry, "scattered",
            300f, 20f, (x, z) => 0f, vertsB, NewUvs(), trisB);
        // same unitId -> the identical gap pattern twice (scrub replay)
        Assert.AreEqual(countsA.BodyIndexCount, countsB.BodyIndexCount);
        Assert.AreEqual(countsA.BorderIndexCount, countsB.BorderIndexCount);
        Assert.AreEqual(countsA.VertexCount, countsB.VertexCount);
        for (int i = 0; i < countsA.VertexCount; i++)
            Assert.AreEqual(vertsA[i], vertsB[i]);
        int total = countsA.BodyIndexCount + countsA.BorderIndexCount;
        for (int i = 0; i < total; i++)
            Assert.AreEqual(trisA[i], trisB[i]);
        // and gaps actually exist: a dissolving unit must not read ordered
        var ordered = Build("csa-garnett", UnitSymbol.SymbolKind.Infantry, "line",
            300f, 20f, (x, z) => 0f, NewVerts(), NewUvs(), NewTris());
        Assert.Less(countsA.BodyIndexCount, ordered.BodyIndexCount);
        Assert.Less(countsA.BorderIndexCount, ordered.BorderIndexCount);
    }

    // ------------------------------------------------------------------
    // Cartography slice 3: facing chevron, motion trail, column extents

    [Test]
    public void FacingChevron_AddsInkInsideTheFootprintForOrderedBars()
    {
        var plainVerts = NewVerts();
        var plain = Build("us-webb", UnitSymbol.SymbolKind.Infantry, "line",
            300f, 20f, (x, z) => 0f, plainVerts, NewUvs(), NewTris());
        var verts = NewVerts();
        var uvs = NewUvs();
        var counts = SymbolMeshBuilder.BuildRibbon(
            "us-webb", default, 0f, 300f, 20f, UnitSymbol.SymbolKind.Infantry,
            "line", 4, (x, z) => 0f, Lift, verts, uvs, NewTris(),
            facingSpine: true);
        // chevron ink lives in the border submesh, on the solid-ink band
        Assert.AreEqual(plain.BodyIndexCount, counts.BodyIndexCount);
        Assert.Greater(counts.BorderIndexCount, plain.BorderIndexCount);
        bool sawInkBand = false;
        for (int i = 0; i < counts.VertexCount; i++)
        {
            // extents doctrine: the chevron never pokes past the footprint
            Assert.LessOrEqual(Mathf.Abs(verts[i].x), 150f + 1e-3f);
            Assert.LessOrEqual(Mathf.Abs(verts[i].z), 10f + 1e-3f);
            if (uvs[i].y >= SymbolMeshBuilder.InkBandUvY) sawInkBand = true;
        }
        Assert.IsTrue(sawInkBand, "no solid-ink band vertices emitted");
        // chevron vertices sit forward of center: the arrow points the way
        float maxInkZ = float.MinValue;
        for (int i = 0; i < counts.VertexCount; i++)
            if (uvs[i].y >= SymbolMeshBuilder.InkBandUvY)
                maxInkZ = Mathf.Max(maxInkZ, verts[i].z);
        Assert.Greater(maxInkZ, 0f);
    }

    [Test]
    public void FacingChevron_SkipsDisorderAndNonBars()
    {
        // scattered/routed men assert no facing; a dotted skirmish line and
        // the artillery grammar carry no chevron either
        foreach ((UnitSymbol.SymbolKind kind, string formation) c in new[]
        {
            (UnitSymbol.SymbolKind.Infantry, "scattered"),
            (UnitSymbol.SymbolKind.Infantry, "routed"),
            (UnitSymbol.SymbolKind.Infantry, "skirmish"),
            (UnitSymbol.SymbolKind.Artillery, "line"),
            (UnitSymbol.SymbolKind.ArtilleryPark, "line"),
        })
        {
            var uvs = NewUvs();
            var counts = SymbolMeshBuilder.BuildRibbon(
                "us-webb", default, 0f, 300f, 20f, c.kind, c.formation, 4,
                (x, z) => 0f, Lift, NewVerts(), uvs, NewTris(),
                facingSpine: true);
            for (int i = 0; i < counts.VertexCount; i++)
                Assert.Less(uvs[i].y, SymbolMeshBuilder.InkBandUvY,
                    $"{c.kind}/{c.formation} emitted a chevron");
        }
    }

    [Test]
    public void MotionTrail_DashedWakeOnlyWhileMoving()
    {
        var still = Build("csa-garnett", UnitSymbol.SymbolKind.Infantry, "line",
            250f, 20f, (x, z) => 0f, NewVerts(), NewUvs(), NewTris());
        var verts = NewVerts();
        var uvs = NewUvs();
        var tris = NewTris();
        var moving = SymbolMeshBuilder.BuildRibbon(
            "csa-garnett", new Vector2(500f, 500f), 0f, 250f, 20f,
            UnitSymbol.SymbolKind.Infantry, "line", 4, (x, z) => 0f, Lift,
            verts, uvs, tris,
            hasTrail: true, trailFromXZ: new Vector2(300f, 480f));
        Assert.Greater(moving.BorderIndexCount, still.BorderIndexCount);
        // the wake is dashed: strictly fewer tris than an undashed strip of
        // its segment count would carry (gaps let the ground show through)
        int segs = Mathf.Clamp(
            Mathf.CeilToInt(new Vector2(200f, 20f).magnitude
                / SymbolMeshBuilder.TrailSegLenM),
            2, SymbolMeshBuilder.TrailMaxSegs);
        int addedIndices = moving.BorderIndexCount - still.BorderIndexCount;
        Assert.Less(addedIndices, segs * 6);
        Assert.Greater(addedIndices, 0);
        // trail vertices reach back toward where the unit came from
        float minX = float.MaxValue;
        for (int i = 0; i < moving.VertexCount; i++)
            minX = Mathf.Min(minX, verts[i].x);
        Assert.Less(minX, 320f); // the fill alone reaches only x=375
        // determinism: the same build twice is identical (scrub replay)
        var vertsB = NewVerts();
        var trisB = NewTris();
        var again = SymbolMeshBuilder.BuildRibbon(
            "csa-garnett", new Vector2(500f, 500f), 0f, 250f, 20f,
            UnitSymbol.SymbolKind.Infantry, "line", 4, (x, z) => 0f, Lift,
            vertsB, NewUvs(), trisB,
            hasTrail: true, trailFromXZ: new Vector2(300f, 480f));
        Assert.AreEqual(moving.VertexCount, again.VertexCount);
        Assert.AreEqual(moving.BorderIndexCount, again.BorderIndexCount);
        for (int i = 0; i < moving.VertexCount; i++)
            Assert.AreEqual(verts[i], vertsB[i]);
        // a sub-segment displacement draws no wake (no flicker at the
        // moving epsilon)
        var tiny = SymbolMeshBuilder.BuildRibbon(
            "csa-garnett", new Vector2(500f, 500f), 0f, 250f, 20f,
            UnitSymbol.SymbolKind.Infantry, "line", 4, (x, z) => 0f, Lift,
            NewVerts(), NewUvs(), NewTris(),
            hasTrail: true, trailFromXZ: new Vector2(498f, 500f));
        Assert.AreEqual(still.BorderIndexCount, tiny.BorderIndexCount);
    }

    [Test]
    public void Column_NarrowsToTheFormationLayoutFootprint()
    {
        // the monolithic ribbon adopts the same column footprint the
        // figures and roster slots already use: frontage/4 wide, depth x4
        Assert.AreEqual((75f, 80f), SymbolMeshBuilder.EffectiveExtents(
            UnitSymbol.SymbolKind.Infantry, "column", 300f, 20f));
        Assert.AreEqual((300f, 20f), SymbolMeshBuilder.EffectiveExtents(
            UnitSymbol.SymbolKind.Infantry, "line", 300f, 20f));
        Assert.AreEqual((300f, 20f), SymbolMeshBuilder.EffectiveExtents(
            UnitSymbol.SymbolKind.Artillery, "column", 300f, 20f));
        var verts = NewVerts();
        var counts = Build("us-webb", UnitSymbol.SymbolKind.Infantry, "column",
            300f, 20f, (x, z) => 0f, verts, NewUvs(), NewTris());
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        for (int i = 0; i < counts.VertexCount; i++)
        {
            minX = Mathf.Min(minX, verts[i].x); maxX = Mathf.Max(maxX, verts[i].x);
            minZ = Mathf.Min(minZ, verts[i].z); maxZ = Mathf.Max(maxZ, verts[i].z);
        }
        Assert.AreEqual(-37.5f, minX, 1e-3f);
        Assert.AreEqual(37.5f, maxX, 1e-3f);
        Assert.AreEqual(-40f, minZ, 1e-3f);
        Assert.AreEqual(40f, maxZ, 1e-3f);
    }

    [Test]
    public void SymbolDirty_StaticUnitStaysClean()
    {
        UnitState a = State(100f, 200f, 90f, 1000f, "line");
        UnitState b = State(100f, 200f, 90f, 1000f, "line");
        Assert.IsFalse(SymbolMeshBuilder.SymbolDirty(a, b, 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block,
            false, false));
    }

    [Test]
    public void SymbolDirty_PositionAndFacingEpsilonsGate()
    {
        UnitState a = State(100f, 200f, 90f, 1000f, "line");
        Assert.IsTrue(SymbolMeshBuilder.SymbolDirty(
            a, State(100.06f, 200f, 90f, 1000f, "line"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
        Assert.IsFalse(SymbolMeshBuilder.SymbolDirty(
            a, State(100.04f, 200f, 90f, 1000f, "line"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
        Assert.IsTrue(SymbolMeshBuilder.SymbolDirty(
            a, State(100f, 200f, 90.6f, 1000f, "line"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
        Assert.IsFalse(SymbolMeshBuilder.SymbolDirty(
            a, State(100f, 200f, 90.3f, 1000f, "line"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
    }

    [Test]
    public void SymbolDirty_StrengthDirtiesOnlyAcrossAQuantumStep()
    {
        UnitState a = State(100f, 200f, 90f, 1000f, "line");
        // 1000 -> 1004 stays inside the half-meter depth quantum at 300 m
        Assert.IsFalse(SymbolMeshBuilder.SymbolDirty(
            a, State(100f, 200f, 90f, 1004f, "line"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
        // 1000 -> 1030 crosses to the next step (20.0 -> 20.5 m)
        Assert.IsTrue(SymbolMeshBuilder.SymbolDirty(
            a, State(100f, 200f, 90f, 1030f, "line"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
    }

    [Test]
    public void SymbolDirty_FormationTierAndSelectionChangesDirty()
    {
        UnitState a = State(100f, 200f, 90f, 1000f, "line");
        Assert.IsTrue(SymbolMeshBuilder.SymbolDirty(
            a, State(100f, 200f, 90f, 1000f, "column"), 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block, false, false));
        Assert.IsTrue(SymbolMeshBuilder.SymbolDirty(a, a, 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Regiments,
            false, false));
        Assert.IsTrue(SymbolMeshBuilder.SymbolDirty(a, a, 300f,
            BattleDirector.LodTier.Block, BattleDirector.LodTier.Block,
            false, true));
    }

    [Test]
    public void Capacity_WorstCasesFitThePreallocatedBuffers()
    {
        var verts = NewVerts();
        var uvs = NewUvs();
        var tris = NewTris();
        foreach ((string id, UnitSymbol.SymbolKind kind, string formation) c in new[]
        {
            ("us-webb", UnitSymbol.SymbolKind.Infantry, "line"),
            ("csa-garnett", UnitSymbol.SymbolKind.Infantry, "scattered"),
            ("us-cav-farnsworth", UnitSymbol.SymbolKind.Cavalry, "line"),
            ("csa-bn-alexander", UnitSymbol.SymbolKind.Artillery, "line"),
            ("us-arty-reserve-park", UnitSymbol.SymbolKind.ArtilleryPark, "line"),
            ("us-webb", UnitSymbol.SymbolKind.Infantry, "skirmish"),
        })
        {
            // a corps-length frontage pins ColumnCount at MaxColumns, max
            // depth and MaxGunDots pin everything else at its widest; the
            // chevron and a max-length trail ride along (slice 3 audit)
            var counts = SymbolMeshBuilder.BuildRibbon(
                c.id, default, 0f, 10000f, UnitSymbol.MaxDepthM, c.kind,
                c.formation, UnitSymbol.MaxGunDots, (x, z) => 0f, Lift,
                verts, uvs, tris, facingSpine: true, hasTrail: true,
                trailFromXZ: new Vector2(9000f, 9000f));
            Assert.LessOrEqual(counts.VertexCount, SymbolMeshBuilder.MaxSymbolVerts, c.id);
            Assert.LessOrEqual(counts.BodyIndexCount + counts.BorderIndexCount,
                SymbolMeshBuilder.MaxSymbolIndices, c.id);
        }
        // and the skirmish grammar spans FillSkirmish's frontage x 1.2
        var skirmishVerts = NewVerts();
        var skirmish = Build("us-webb", UnitSymbol.SymbolKind.Infantry, "skirmish",
            100f, 8f, (x, z) => 0f, skirmishVerts, NewUvs(), NewTris());
        float maxX = float.MinValue;
        for (int i = 0; i < skirmish.VertexCount; i++)
            maxX = Mathf.Max(maxX, skirmishVerts[i].x);
        Assert.AreEqual(100f * SymbolMeshBuilder.SkirmishSpreadMul / 2f, maxX, 1e-3f);
    }
}
