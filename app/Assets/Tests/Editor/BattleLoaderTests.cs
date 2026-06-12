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
}
