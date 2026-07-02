using NUnit.Framework;
using BattleAtlas;

public class BattleLoaderTests
{
    const string ValidJson = @"{
        ""name"": ""Test battle"",
        ""startTime"": 0,
        ""endTime"": 100,
        ""units"": [
            {
                ""id"": ""u1"",
                ""name"": ""Test Brigade A"",
                ""side"": ""union"",
                ""frontage_m"": 200,
                ""depth_m"": 30,
                ""keyframes"": [
                    {""t"": 0, ""x"": 100, ""z"": 200, ""facing"": 90, ""formation"": ""column"", ""strength"": 1500},
                    {""t"": 100, ""x"": 300, ""z"": 200, ""facing"": 90, ""formation"": ""column"", ""strength"": 1400}
                ]
            }
        ]
    }";

    [Test]
    public void Parse_ValidBattle()
    {
        BattleDto b = BattleLoader.Parse(ValidJson);
        Assert.AreEqual("Test battle", b.name);
        Assert.AreEqual(100f, b.endTime);
        Assert.AreEqual(1, b.units.Count);
        Assert.AreEqual("u1", b.units[0].id);
        Assert.AreEqual(2, b.units[0].keyframes.Count);
        Assert.AreEqual(300f, b.units[0].keyframes[1].x);
    }

    [Test]
    public void Parse_RejectsBattleWithNoUnits()
    {
        Assert.Throws<System.ArgumentException>(() =>
            BattleLoader.Parse(@"{""name"": ""empty"", ""startTime"": 0, ""endTime"": 10, ""units"": []}"));
    }

    [Test]
    public void Parse_RejectsNonIncreasingKeyframeTimes()
    {
        string bad = ValidJson.Replace(@"""t"": 100,", @"""t"": 0,");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("u1", ex.Message);
    }

    [Test]
    public void Parse_RejectsUnitWithNoKeyframes()
    {
        string bad = @"{
            ""name"": ""b"", ""startTime"": 0, ""endTime"": 10,
            ""units"": [{""id"": ""ux"", ""name"": ""n"", ""side"": ""union"",
                         ""frontage_m"": 1, ""depth_m"": 1, ""keyframes"": []}]
        }";
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("ux", ex.Message);
    }

    [Test]
    public void Parse_RejectsKeyframeBeyondEndTime()
    {
        string bad = ValidJson.Replace(@"""endTime"": 100", @"""endTime"": 50");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("endTime", ex.Message);
    }

    [Test]
    public void Parse_RejectsNonPositiveFrontage()
    {
        string bad = ValidJson.Replace(@"""frontage_m"": 200", @"""frontage_m"": 0");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("u1", ex.Message);
    }

    [Test]
    public void Parse_ReadsProvenanceFields()
    {
        string json = ValidJson.Replace(
            @"{""t"": 0, ""x"": 100,",
            @"{""t"": 0, ""confidence"": ""documented"", ""citation"": ""test source"", ""x"": 100,");
        BattleDto b = BattleLoader.Parse(json);
        Assert.AreEqual("documented", b.units[0].keyframes[0].confidence);
        Assert.AreEqual("test source", b.units[0].keyframes[0].citation);
    }

    // A two-unit family for the parent/children rules: u1 decomposed into u2.
    const string FamilyJson = @"{
        ""name"": ""Family battle"",
        ""startTime"": 0,
        ""endTime"": 100,
        ""units"": [
            {
                ""id"": ""u1"",
                ""name"": ""Test Brigade A"",
                ""side"": ""union"",
                ""frontage_m"": 200,
                ""depth_m"": 30,
                ""keyframes"": [
                    {""t"": 0, ""x"": 100, ""z"": 200, ""facing"": 90, ""formation"": ""column"", ""strength"": 1500}
                ]
            },
            {
                ""id"": ""u2"",
                ""name"": ""Test Regiment A1"",
                ""side"": ""union"",
                ""parent"": ""u1"",
                ""frontage_m"": 100,
                ""depth_m"": 30,
                ""keyframes"": [
                    {""t"": 0, ""x"": 100, ""z"": 180, ""facing"": 90, ""formation"": ""column"", ""strength"": 750}
                ]
            }
        ]
    }";

    [Test]
    public void Parse_ReadsParentAndRejectsUnknownParent()
    {
        BattleDto b = BattleLoader.Parse(FamilyJson);
        Assert.AreEqual("u1", b.units[1].parent);
        Assert.IsTrue(string.IsNullOrEmpty(b.units[0].parent)); // absent = null/empty

        string bad = FamilyJson.Replace(@"""parent"": ""u1""", @"""parent"": ""no-such-unit""");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("no-such-unit", ex.Message);
    }

    [Test]
    public void Parse_RejectsGrandparent()
    {
        // point u1 at u2: u1 becomes both parent and child — depth 2, forbidden
        string bad = FamilyJson.Replace(
            @"""id"": ""u1"",", @"""id"": ""u1"", ""parent"": ""u2"",");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("parent", ex.Message);
    }

    [Test]
    public void Parse_RejectsParentWithChildrenCarryingRoster()
    {
        // full decomposition or none: u1 has a child, so a roster is an error
        string bad = FamilyJson.Replace(
            @"""id"": ""u1"",",
            @"""id"": ""u1"", ""regiments"": [""1st Test"", ""2nd Test""],");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("regiments", ex.Message);
    }

    // Engagement events + environment (battle-format.md "Engagement events" /
    // "Environment"): one event per emitter form, plus wind.
    const string EventsJson = @"{
        ""name"": ""Events battle"",
        ""startTime"": 0,
        ""endTime"": 100,
        ""events"": [
            {""id"": ""ev-guns"", ""kind"": ""artillery_fire"", ""t0"": 10, ""t1"": 90,
             ""x"": 100, ""z"": 200, ""x2"": 400, ""z2"": 250,
             ""confidence"": ""documented"", ""citation"": ""test source""},
            {""id"": ""ev-musket"", ""kind"": ""musketry"", ""t0"": 50, ""t1"": 80,
             ""unitId"": ""u1"", ""confidence"": ""inferred"", ""note"": ""test window""}
        ],
        ""environment"": {""windTowardDeg"": 45, ""windMps"": 2, ""confidence"": ""inferred""},
        ""units"": [
            {
                ""id"": ""u1"",
                ""name"": ""Test Brigade A"",
                ""side"": ""union"",
                ""frontage_m"": 200,
                ""depth_m"": 30,
                ""keyframes"": [
                    {""t"": 0, ""x"": 100, ""z"": 200, ""facing"": 90, ""formation"": ""column"", ""strength"": 1500}
                ]
            }
        ]
    }";

    [Test]
    public void Parse_ReadsEventsAndEnvironment()
    {
        BattleDto b = BattleLoader.Parse(EventsJson);
        Assert.AreEqual(2, b.events.Count);
        Assert.AreEqual("artillery_fire", b.events[0].kind);
        Assert.AreEqual(400f, b.events[0].x2);
        Assert.IsTrue(string.IsNullOrEmpty(b.events[0].unitId)); // segment form
        Assert.AreEqual("u1", b.events[1].unitId);               // unit form
        Assert.AreEqual(80f, b.events[1].t1);
        Assert.AreEqual(45f, b.environment.windTowardDeg);
        Assert.AreEqual(2f, b.environment.windMps);
        // JsonUtility quirk, relied on as the calm fallback: an absent
        // environment block deserializes as a zeroed instance = windMps 0 =
        // no drift
        Assert.AreEqual(0f, BattleLoader.Parse(ValidJson).environment.windMps);
    }

    [Test]
    public void Parse_RejectsEventWithUnknownUnit()
    {
        string bad = EventsJson.Replace(@"""unitId"": ""u1""", @"""unitId"": ""no-such-unit""");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("no-such-unit", ex.Message);
    }

    [Test]
    public void Parse_RejectsEventWithEmptyWindow()
    {
        string bad = EventsJson.Replace(@"""t0"": 50, ""t1"": 80", @"""t0"": 80, ""t1"": 80");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("ev-musket", ex.Message);
    }

    [Test]
    public void Parse_RejectsEventWithUnknownKind()
    {
        string bad = EventsJson.Replace(@"""kind"": ""musketry""", @"""kind"": ""cavalry_charge""");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("cavalry_charge", ex.Message);
    }

    [Test]
    public void PlaceholderAsset_ParsesAndStaysOnBattlefield()
    {
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(
            "Assets/Battle/placeholder_battle.json");
        Assert.IsNotNull(asset, "placeholder_battle.json missing");
        BattleDto b = BattleLoader.Parse(asset.text);
        Assert.GreaterOrEqual(b.units.Count, 4);
        foreach (var u in b.units)
        {
            StringAssert.Contains("Test", u.name, "placeholder units must be obviously fake");
            foreach (var k in u.keyframes)
            {
                // terrain is an 8507 m square; keep test data well inside it
                Assert.That(k.x, Is.InRange(500f, 8000f), $"{u.id} x out of range");
                Assert.That(k.z, Is.InRange(500f, 8000f), $"{u.id} z out of range");
            }
        }
    }
}
