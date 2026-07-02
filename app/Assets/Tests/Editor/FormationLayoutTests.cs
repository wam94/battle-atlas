using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class FormationLayoutTests
{
    [Test]
    public void FigureCount_TenMenPerFigure_Capped()
    {
        Assert.AreEqual(140, FormationLayout.FigureCount(1400f));
        Assert.AreEqual(1, FormationLayout.FigureCount(5f));
        Assert.AreEqual(0, FormationLayout.FigureCount(0f));
        Assert.AreEqual(400, FormationLayout.FigureCount(99999f));
    }

    [Test]
    public void Line_TwoRanks_SpansFrontage()
    {
        var offsets = FormationLayout.Offsets("u1", "line", 40, 300f, 40f);
        Assert.AreEqual(40, offsets.Length);
        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var o in offsets)
        {
            if (o.x < minX) minX = o.x;
            if (o.x > maxX) maxX = o.x;
            Assert.LessOrEqual(Mathf.Abs(o.y), 20f + 1f); // within depth envelope
        }
        Assert.Greater(maxX - minX, 250f); // uses most of the 300m frontage
        Assert.LessOrEqual(maxX, 151f);
        Assert.GreaterOrEqual(minX, -151f);
    }

    [Test]
    public void Column_NarrowFront()
    {
        var offsets = FormationLayout.Offsets("u1", "column", 40, 300f, 40f);
        foreach (var o in offsets)
            Assert.LessOrEqual(Mathf.Abs(o.x), 300f / 8f + 1f); // half of frontage/4 + tolerance
    }

    [Test]
    public void Scattered_StaysWithinExpandedFootprint_AndIsDeterministic()
    {
        var a = FormationLayout.Offsets("u1", "scattered", 60, 200f, 30f);
        var b = FormationLayout.Offsets("u1", "scattered", 60, 200f, 30f);
        for (int i = 0; i < a.Length; i++)
        {
            Assert.AreEqual(a[i].x, b[i].x, 1e-5f); // deterministic
            Assert.LessOrEqual(Mathf.Abs(a[i].x), 150f + 1f); // 1.5x half-frontage
            Assert.LessOrEqual(Mathf.Abs(a[i].y), 22.5f + 1f);
        }
        var c = FormationLayout.Offsets("u2", "scattered", 60, 200f, 30f);
        Assert.AreNotEqual(a[0].x, c[0].x); // different unit, different scatter
    }

    [Test]
    public void Routed_TrailsRearward()
    {
        var offsets = FormationLayout.Offsets("u1", "routed", 50, 200f, 30f);
        int behind = 0;
        foreach (var o in offsets) if (o.y < 0f) behind++;
        Assert.Greater(behind, 35); // most figures trail behind the unit center
    }

    [Test]
    public void UnknownFormation_FallsBackToLine()
    {
        var line = FormationLayout.Offsets("u1", "line", 20, 100f, 20f);
        var unknown = FormationLayout.Offsets("u1", "banana", 20, 100f, 20f);
        for (int i = 0; i < line.Length; i++)
            Assert.AreEqual(line[i].x, unknown[i].x, 1e-5f);
    }
}
