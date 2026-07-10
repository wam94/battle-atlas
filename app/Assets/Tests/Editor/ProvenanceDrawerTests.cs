using System.Collections.Generic;
using NUnit.Framework;
using BattleAtlas;

// Pins the source/provenance drawer's data build (Phase 11): the bracketing
// keyframe carry rule (inherited verbatim from the IMGUI citation-line
// seed), identity words, activity composition, and the source-entry list —
// including the no-faking fallback where the record is silent.
public class ProvenanceDrawerTests
{
    static List<KeyframeDto> Keyframes()
    {
        return new List<KeyframeDto>
        {
            new KeyframeDto { t = 0f, citation = "cite A", confidence = "documented" },
            new KeyframeDto { t = 100f, citation = "cite B", confidence = "inferred" },
            new KeyframeDto { t = 200f, citation = "", confidence = "" },
        };
    }

    [Test]
    public void CitationAt_CarriesBracketingStartKeyframe()
    {
        List<KeyframeDto> kfs = Keyframes();
        Assert.AreEqual("cite A", ProvenanceDrawer.CitationAt(kfs, 0f));
        Assert.AreEqual("cite A", ProvenanceDrawer.CitationAt(kfs, 99.9f));
        Assert.AreEqual("cite B", ProvenanceDrawer.CitationAt(kfs, 100f));
        Assert.AreEqual("", ProvenanceDrawer.CitationAt(kfs, 250f)); // clamp cites the end
    }

    [Test]
    public void IdentityWords_Table()
    {
        Assert.AreEqual("brigade", ProvenanceDrawer.EchelonWord(UnitSymbol.Echelon.Brigade));
        Assert.AreEqual("regiment", ProvenanceDrawer.EchelonWord(UnitSymbol.Echelon.Regiment));
        Assert.AreEqual("battery", ProvenanceDrawer.EchelonWord(UnitSymbol.Echelon.Battery));
        Assert.AreEqual("park", ProvenanceDrawer.EchelonWord(UnitSymbol.Echelon.Park));
        Assert.AreEqual("infantry", ProvenanceDrawer.ArmWord(BattleDirector.UnitKind.Infantry));
        Assert.AreEqual("artillery", ProvenanceDrawer.ArmWord(BattleDirector.UnitKind.Artillery));
        Assert.AreEqual("cavalry", ProvenanceDrawer.ArmWord(BattleDirector.UnitKind.Cavalry));
        Assert.AreEqual("Union", ProvenanceDrawer.SideWord(true));
        Assert.AreEqual("Confederate", ProvenanceDrawer.SideWord(false));
    }

    [Test]
    public void ConfidenceWord_UnknownReadsAsInferredUnrecorded()
    {
        Assert.AreEqual("documented", ProvenanceDrawer.ConfidenceWord("documented"));
        Assert.AreEqual("inferred", ProvenanceDrawer.ConfidenceWord("inferred"));
        Assert.AreEqual("inferred (unrecorded)", ProvenanceDrawer.ConfidenceWord("unknown"));
        Assert.AreEqual("inferred (unrecorded)", ProvenanceDrawer.ConfidenceWord(""));
        Assert.AreEqual("inferred (unrecorded)", ProvenanceDrawer.ConfidenceWord(null));
        Assert.AreEqual("contested", ProvenanceDrawer.ConfidenceWord("contested"));
    }

    [Test]
    public void StrengthLine_RoundsAndFormats()
    {
        Assert.AreEqual("1,455 men", ProvenanceDrawer.StrengthLine(1455.4f));
        Assert.AreEqual("300 men", ProvenanceDrawer.StrengthLine(299.6f));
    }

    static EventDto Fire(string kind, float t0, float t1, string cite = "ev cite")
    {
        return new EventDto { id = "e", kind = kind, t0 = t0, t1 = t1, citation = cite };
    }

    [Test]
    public void ActivityLine_MovementFormationAndLiveFire()
    {
        Assert.AreEqual("holding in line",
            ProvenanceDrawer.ActivityLine(false, "line", null, 50f));
        Assert.AreEqual("moving in column",
            ProvenanceDrawer.ActivityLine(true, "column", null, 50f));
        Assert.AreEqual("routed",
            ProvenanceDrawer.ActivityLine(true, "routed", null, 50f));
        var events = new List<EventDto> { Fire("musketry", 40f, 60f) };
        Assert.AreEqual("holding in line · firing (musketry)",
            ProvenanceDrawer.ActivityLine(false, "line", events, 50f));
        Assert.AreEqual("holding in line",
            ProvenanceDrawer.ActivityLine(false, "line", events, 70f)); // window over
        var both = new List<EventDto>
        {
            Fire("musketry", 40f, 60f), Fire("artillery_fire", 40f, 60f),
        };
        Assert.AreEqual("holding in line · firing (artillery, musketry)",
            ProvenanceDrawer.ActivityLine(false, "line", both, 50f));
    }

    [Test]
    public void SourceEntries_TrackSegmentFirstThenEventsLiveFirst()
    {
        var unit = new UnitDto { id = "u", keyframes = Keyframes() };
        var events = new List<EventDto>
        {
            Fire("artillery_fire", 150f, 300f, "late cite"),
            Fire("musketry", 90f, 120f, "live cite"),
        };
        List<ProvenanceDrawer.SourceEntry> entries =
            ProvenanceDrawer.SourceEntries(unit, 110f, 46800f, events);
        Assert.AreEqual(3, entries.Count);
        // 1: the bracketing segment (t=100, inferred), always first
        StringAssert.Contains("track segment from 13:01:40", entries[0].Heading);
        StringAssert.Contains("(inferred)", entries[0].Heading);
        Assert.AreEqual("cite B", entries[0].Citation);
        // 2: the LIVE musketry window beats the not-yet artillery window
        StringAssert.Contains("musketry", entries[1].Heading);
        StringAssert.Contains("— live", entries[1].Heading);
        Assert.AreEqual("live cite", entries[1].Citation);
        StringAssert.Contains("artillery fire", entries[2].Heading);
        StringAssert.DoesNotContain("— live", entries[2].Heading);
    }

    [Test]
    public void SourceEntries_SilentRecordSaysSo()
    {
        var unit = new UnitDto
        {
            id = "u",
            keyframes = new List<KeyframeDto>
            {
                new KeyframeDto { t = 0f, citation = "", confidence = "" },
            },
        };
        List<ProvenanceDrawer.SourceEntry> entries =
            ProvenanceDrawer.SourceEntries(unit, 10f, 46800f, null);
        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual(ProvenanceDrawer.NoReliableRecord, entries[0].Citation);
        StringAssert.Contains("inferred (unrecorded)", entries[0].Heading);
    }
}
