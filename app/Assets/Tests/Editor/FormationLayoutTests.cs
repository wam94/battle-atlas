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

    [Test]
    public void RegimentSlots_Line_FiveSlots_SpanFrontageWithGaps()
    {
        var slots = FormationLayout.RegimentSlots("line", 5, 300f, 40f);
        Assert.AreEqual(5, slots.Length);
        // outer edges reach the unit's frontage: right edge of slot 0 at +150,
        // left edge of slot 4 at -150
        Assert.AreEqual(150f, slots[0].center.x + slots[0].size.x / 2f, 1e-3f);
        Assert.AreEqual(-150f, slots[4].center.x - slots[4].size.x / 2f, 1e-3f);
        for (int i = 0; i < 4; i++)
        {
            // 6m interval gap between adjacent sub-blocks
            float gap = (slots[i].center.x - slots[i].size.x / 2f)
                      - (slots[i + 1].center.x + slots[i + 1].size.x / 2f);
            Assert.AreEqual(6f, gap, 1e-3f);
        }
        foreach (var s in slots)
        {
            Assert.AreEqual(0f, s.center.y, 1e-3f);  // all on the frontage axis
            Assert.AreEqual(40f, s.size.y, 1e-3f);   // full unit depth each
        }
    }

    [Test]
    public void RegimentSlots_Line_EqualWidths_OrderedRightToLeft()
    {
        var slots = FormationLayout.RegimentSlots("line", 5, 300f, 40f);
        float expectedWidth = (300f - 4 * 6f) / 5f; // frontage minus 4 gaps, split 5 ways
        foreach (var s in slots)
            Assert.AreEqual(expectedWidth, s.size.x, 1e-3f);
        // roster convention: slot 0 = rightmost (+x), last = leftmost (-x)
        for (int i = 0; i < 4; i++)
            Assert.Greater(slots[i].center.x, slots[i + 1].center.x);
    }

    [Test]
    public void RegimentSlots_Column_StacksFrontToBack()
    {
        var slots = FormationLayout.RegimentSlots("column", 4, 300f, 40f);
        Assert.AreEqual(4, slots.Length);
        float expectedDepth = (40f * 4f - 3 * 6f) / 4f; // column footprint depth minus 3 gaps
        foreach (var s in slots)
        {
            Assert.AreEqual(0f, s.center.x, 1e-3f);        // stacked on the depth axis
            Assert.AreEqual(300f / 4f, s.size.x, 1e-3f);   // column width = frontage/4
            Assert.AreEqual(expectedDepth, s.size.y, 1e-3f);
        }
        // slot 0 leads (+y = forward), later slots trail behind it
        Assert.AreEqual(80f, slots[0].center.y + slots[0].size.y / 2f, 1e-3f);  // front edge
        Assert.AreEqual(-80f, slots[3].center.y - slots[3].size.y / 2f, 1e-3f); // rear edge
        for (int i = 0; i < 3; i++)
            Assert.Greater(slots[i].center.y, slots[i + 1].center.y);
    }

    [Test]
    public void RegimentSlots_CountOne_DegeneratesToFullBlock()
    {
        var slots = FormationLayout.RegimentSlots("line", 1, 300f, 40f);
        Assert.AreEqual(1, slots.Length);
        Assert.AreEqual(0f, slots[0].center.x, 1e-3f);
        Assert.AreEqual(0f, slots[0].center.y, 1e-3f);
        Assert.AreEqual(300f, slots[0].size.x, 1e-3f);
        Assert.AreEqual(40f, slots[0].size.y, 1e-3f);
    }
}
