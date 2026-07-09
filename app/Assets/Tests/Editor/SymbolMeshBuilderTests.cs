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
            // depth and MaxGunDots pin everything else at its widest
            var counts = Build(c.id, c.kind, c.formation, 10000f,
                UnitSymbol.MaxDepthM, (x, z) => 0f, verts, uvs, tris,
                gunDots: UnitSymbol.MaxGunDots);
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
