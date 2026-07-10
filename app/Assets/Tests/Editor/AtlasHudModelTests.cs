using System;
using NUnit.Framework;
using BattleAtlas;

// Pins the Phase 11 HUD's pure presentation math (HudModel) and the moment
// marker set (MomentSet): the plan-mandated speed set, marker layout,
// masthead context, conditions readout, entry-marker windowing, and the
// no-invented-times rule on moments.
public class AtlasHudModelTests
{
    [Test]
    public void Speeds_ArePlanSection10Set()
    {
        Assert.AreEqual(new[] { 1f, 10f, 60f }, HudModel.Speeds);
    }

    [Test]
    public void SoldierViewSpeedLabel_SaysForcedSpeedAndPausedState()
    {
        // P11 punchlist (assigned to Phase 12): the forced 1× must be
        // legible inside Soldier View, including while paused
        Assert.AreEqual("1× real time", HudModel.SoldierViewSpeedLabel(true));
        Assert.AreEqual("1× real time — paused",
            HudModel.SoldierViewSpeedLabel(false));
    }

    [Test]
    public void TimelineFraction_ClampsAndScales()
    {
        Assert.AreEqual(0f, HudModel.TimelineFraction(-5f, 100f));
        Assert.AreEqual(0.25f, HudModel.TimelineFraction(25f, 100f), 1e-6f);
        Assert.AreEqual(1f, HudModel.TimelineFraction(150f, 100f));
        Assert.AreEqual(0f, HudModel.TimelineFraction(50f, 0f)); // degenerate
    }

    [Test]
    public void DayContext_TrimsParentheticalQualifier()
    {
        Assert.AreEqual("Pickett's Charge — July 3, 1863",
            HudModel.DayContext(
                "Pickett's Charge — July 3, 1863 (brigade level, reconstruction)"));
        Assert.AreEqual("No qualifier", HudModel.DayContext("No qualifier"));
        Assert.AreEqual("", HudModel.DayContext(null));
    }

    [Test]
    public void ConditionsLine_WindAndCalm()
    {
        var env = new EnvironmentDto
        {
            windTowardDeg = 45f, windMps = 2f, confidence = "inferred",
        };
        Assert.AreEqual("wind 2 m/s toward NE (inferred)", HudModel.ConditionsLine(env));
        Assert.AreEqual("calm", HudModel.ConditionsLine(new EnvironmentDto()));
        Assert.AreEqual("calm", HudModel.ConditionsLine(null));
    }

    [Test]
    public void CompassWord_EightPointTable()
    {
        Assert.AreEqual("N", HudModel.CompassWord(0f));
        Assert.AreEqual("NE", HudModel.CompassWord(45f));
        Assert.AreEqual("S", HudModel.CompassWord(180f));
        Assert.AreEqual("NW", HudModel.CompassWord(315f));
        Assert.AreEqual("N", HudModel.CompassWord(359f));
        Assert.AreEqual("E", HudModel.CompassWord(450f)); // wraps
    }

    static ViewpointDefinition Viewpoint(bool development)
    {
        return new ViewpointDefinition
        {
            id = "vp", t0 = 8160, t1 = 8820, development = development,
            media = new ViewpointMedia { proxy = "p.mp4", fps = 30f },
        };
    }

    [Test]
    public void EntryMarker_OnlyInsideWindow_AndNeverForDevelopment()
    {
        ViewpointDefinition vp = Viewpoint(false);
        Assert.IsFalse(HudModel.EntryMarkerVisible(vp, 8159.9));
        Assert.IsTrue(HudModel.EntryMarkerVisible(vp, 8160.0));
        Assert.IsTrue(HudModel.EntryMarkerVisible(vp, 8500.0));
        Assert.IsFalse(HudModel.EntryMarkerVisible(vp, 8820.0)); // t1 exclusive
        Assert.IsFalse(HudModel.EntryMarkerVisible(Viewpoint(true), 8500.0));
        Assert.IsFalse(HudModel.EntryMarkerVisible(null, 8500.0));
    }

    [Test]
    public void DevelopmentFlag_DefaultsFalseFromJson()
    {
        // absent field = product viewpoint (JsonUtility zero-default)
        var set = ViewpointSet.FromJson(
            "{\"viewpoints\":[{\"id\":\"x\",\"t0\":1,\"t1\":2," +
            "\"media\":{\"proxy\":\"p.mp4\",\"fps\":30}}]}");
        Assert.IsFalse(set.viewpoints[0].development);
    }

    [Test]
    public void WindowBand_FractionsAlongTimeline()
    {
        (float left, float width) = HudModel.WindowBand(8160, 8820, 10800f);
        Assert.AreEqual(8160f / 10800f, left, 1e-5f);
        Assert.AreEqual(660f / 10800f, width, 1e-5f);
    }

    [Test]
    public void SettleIndicator_OnlyAfterThreshold()
    {
        Assert.IsFalse(HudModel.ShowSettleIndicator(true, 0.05));
        Assert.IsFalse(HudModel.ShowSettleIndicator(false, 1.0));
        Assert.IsTrue(HudModel.ShowSettleIndicator(true, 0.2));
    }

    // ------------------------------------------------------------ moments

    static string MomentsJson(params (float t, string label, string cite)[] ms)
    {
        var parts = new System.Text.StringBuilder("{\"moments\":[");
        for (int i = 0; i < ms.Length; i++)
        {
            if (i > 0) parts.Append(',');
            parts.Append("{\"t\":").Append(ms[i].t)
                .Append(",\"label\":\"").Append(ms[i].label)
                .Append("\",\"detail\":\"d\",\"citation\":\"")
                .Append(ms[i].cite).Append("\"}");
        }
        return parts.Append("]}").ToString();
    }

    [Test]
    public void Moments_ParseAndPhaseAt()
    {
        MomentSet set = MomentSet.FromJson(MomentsJson(
            (0f, "Bombardment", "c1"), (7500f, "Step-off", "c2"),
            (8700f, "Angle crisis", "c3")));
        Assert.AreEqual("Bombardment", set.PhaseAt(0f));
        Assert.AreEqual("Bombardment", set.PhaseAt(7499f));
        Assert.AreEqual("Step-off", set.PhaseAt(7500f));
        Assert.AreEqual("Angle crisis", set.PhaseAt(10800f));
    }

    [Test]
    public void Moments_RejectMissingCitationAndDisorder()
    {
        Assert.Throws<ArgumentException>(() =>
            MomentSet.FromJson(MomentsJson((0f, "A", ""))));
        Assert.Throws<ArgumentException>(() =>
            MomentSet.FromJson(MomentsJson((100f, "A", "c"), (50f, "B", "c"))));
        Assert.Throws<ArgumentException>(() => MomentSet.FromJson("{}"));
    }

    [Test]
    public void CommittedMomentsFile_ParsesAndBracketsTheSlice()
    {
        string path = UnityEngine.Application.dataPath
            + "/StreamingAssets/Atlas/moments.json";
        MomentSet set = MomentSet.FromJson(System.IO.File.ReadAllText(path));
        Assert.GreaterOrEqual(set.moments.Length, 5);
        Assert.AreEqual(0f, set.moments[0].t); // bombardment anchors t=0
        // the hero-viewpoint window's ends are marked (Gate P11: a new user
        // must be able to FIND the charge)
        Assert.IsTrue(Array.Exists(set.moments, m => m.t == 8160f));
        Assert.IsTrue(Array.Exists(set.moments, m => m.t == 8820f));
    }
}
