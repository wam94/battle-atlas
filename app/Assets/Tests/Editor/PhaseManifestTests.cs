using System;
using NUnit.Framework;
using BattleAtlas;

// The day/phase manifest (ADR 0005): parse-time honesty rules (an empty
// day is inexpressible without its note; a phase can never smuggle in a
// battle or lie about a clock), the active-day/phase lookups the HUD's
// day tabs ride, and the committed StreamingAssets instance itself —
// including its clock echo against the committed battle file.
public class PhaseManifestTests
{
    const string Valid = @"{
      ""name"": ""Test Battle"",
      ""days"": [
        { ""id"": ""d1"", ""label"": ""July 1"", ""date"": ""1863-07-01"",
          ""phases"": [
            { ""id"": ""p1"", ""label"": ""The first day"",
              ""status"": ""not-reconstructed"", ""note"": ""Not yet reconstructed."" } ] },
        { ""id"": ""d3"", ""label"": ""July 3"", ""date"": ""1863-07-03"",
          ""phases"": [
            { ""id"": ""p3m"", ""label"": ""Morning"",
              ""status"": ""not-reconstructed"", ""note"": ""Nothing authored."" },
            { ""id"": ""p3a"", ""label"": ""Afternoon"",
              ""status"": ""reconstructed"", ""battle"": ""test-battle.json"",
              ""startTime"": 46800, ""endTime"": 23340 } ] }
      ]
    }";

    [Test]
    public void Valid_Parses_WithLookups()
    {
        PhaseManifest m = PhaseManifest.FromJson(Valid);
        Assert.AreEqual(2, m.days.Length);
        Assert.AreEqual(1, m.ActiveDayIndex("test-battle"));
        Assert.AreEqual(-1, m.ActiveDayIndex("some-fixture"));
        Assert.AreEqual(-1, m.ActiveDayIndex(null));
        Assert.AreEqual("p3a", m.PhaseForBattle("test-battle").id);
        Assert.IsNull(m.PhaseForBattle("some-fixture"));
    }

    [Test]
    public void ClockEcho_MismatchIsLoud_AgreementIsSilent()
    {
        PhaseManifest m = PhaseManifest.FromJson(Valid);
        Assert.IsNull(m.ClockMismatch("test-battle", 46800f, 23340f));
        StringAssert.Contains("stale", m.ClockMismatch("test-battle", 46800f, 10800f));
        // no matching phase (fixtures) = nothing to check, no false alarm
        Assert.IsNull(m.ClockMismatch("some-fixture", 0f, 1f));
    }

    static string Mutated(string find, string replace)
        => Valid.Replace(find, replace);

    [Test]
    public void Rejects_EmptyPhaseWithoutNote_AndSmuggledBattle()
    {
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson(
            Mutated(@"""note"": ""Not yet reconstructed.""", @"""note"": """"")));
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson(
            Mutated(@"""status"": ""not-reconstructed"", ""note"": ""Nothing authored.""",
                @"""status"": ""not-reconstructed"", ""note"": ""n"", ""battle"": ""x.json""")));
    }

    [Test]
    public void Rejects_DuplicateIds_DateDisorder_UnknownStatus_MissingClock()
    {
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson(
            Mutated(@"""id"": ""d3""", @"""id"": ""d1""")));
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson(
            Mutated(@"""date"": ""1863-07-03""", @"""date"": ""1863-06-30""")));
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson(
            Mutated(@"""status"": ""reconstructed""", @"""status"": ""imagined""")));
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson(
            Mutated(@"""endTime"": 23340", @"""endTime"": 0")));
        Assert.Throws<ArgumentException>(() => PhaseManifest.FromJson("{}"));
    }

    [Test]
    public void PhaseClockRange_ReadsAsWallClock()
    {
        Assert.AreEqual("13:00–19:29 local mean time",
            HudModel.PhaseClockRange(46800f, 23340f));
    }

    [Test]
    public void CommittedManifest_ParsesAndEchoesTheCommittedBattleClock()
    {
        string manifestPath = UnityEngine.Application.dataPath
            + "/StreamingAssets/Atlas/battle-manifest.json";
        PhaseManifest m = PhaseManifest.FromJson(
            System.IO.File.ReadAllText(manifestPath));
        Assert.AreEqual(3, m.days.Length); // July 1 / 2 / 3
        // July 3 afternoon is the loaded reconstruction
        Assert.AreEqual(2, m.ActiveDayIndex("gettysburg-july3"));
        PhaseDto phase = m.PhaseForBattle("gettysburg-july3");
        Assert.AreEqual("july3-afternoon", phase.id);
        // the clock echo against the committed battle file itself: the
        // manifest may never lie about a phase's clock
        var battle = BattleLoader.Parse(System.IO.File.ReadAllText(
            UnityEngine.Application.dataPath + "/Battle/gettysburg-july3.json"));
        Assert.IsNull(m.ClockMismatch("gettysburg-july3",
            battle.startTime, battle.endTime));
        Assert.AreEqual(46800f, phase.startTime);
        Assert.AreEqual(23340f, phase.endTime); // sunset 19:29 LMT (ED-31)
        // July 2 (day-expansion slice 2): two reconstructed phases abutting
        // at the sunset pin (70140 = 19:29 LMT), each echoing its own file.
        Assert.AreEqual(1, m.ActiveDayIndex("gettysburg-july2-afternoon"));
        Assert.AreEqual(1, m.ActiveDayIndex("gettysburg-july2-evening"));
        PhaseDto j2a = m.PhaseForBattle("gettysburg-july2-afternoon");
        Assert.AreEqual("july2-afternoon", j2a.id);
        PhaseDto j2e = m.PhaseForBattle("gettysburg-july2-evening");
        Assert.AreEqual("july2-evening", j2e.id);
        Assert.AreEqual(j2a.startTime + j2a.endTime, j2e.startTime,
            "the July 2 phases abut at sunset, never overlap");
        var j2aBattle = BattleLoader.Parse(System.IO.File.ReadAllText(
            UnityEngine.Application.dataPath + "/Battle/gettysburg-july2-afternoon.json"));
        Assert.IsNull(m.ClockMismatch("gettysburg-july2-afternoon",
            j2aBattle.startTime, j2aBattle.endTime));
        var j2eBattle = BattleLoader.Parse(System.IO.File.ReadAllText(
            UnityEngine.Application.dataPath + "/Battle/gettysburg-july2-evening.json"));
        Assert.IsNull(m.ClockMismatch("gettysburg-july2-evening",
            j2eBattle.startTime, j2eBattle.endTime));
        // the empty days are honest: every not-reconstructed phase carries
        // its note and no battle
        foreach (DayDto day in m.days)
            foreach (PhaseDto p in day.phases)
                if (!p.Reconstructed)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(p.note), p.id);
                    Assert.IsTrue(string.IsNullOrEmpty(p.battle), p.id);
                }
    }
}
