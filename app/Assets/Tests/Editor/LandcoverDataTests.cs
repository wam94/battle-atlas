using NUnit.Framework;
using BattleAtlas;

public class LandcoverDataTests
{
    [Test]
    public void ParseTrees_ValidInlineJson()
    {
        string json = @"{""trees"": [
            {""x"": 100, ""z"": 200, ""cls"": ""woodlot""},
            {""x"": 110, ""z"": 205, ""cls"": ""orchard""}
        ]}";
        TreesDto dto = LandcoverData.ParseTrees(json);
        Assert.AreEqual(2, dto.trees.Count);
        Assert.AreEqual(100f, dto.trees[0].x);
        Assert.AreEqual("orchard", dto.trees[1].cls);
    }

    [Test]
    public void ParseTrees_RejectsEmptyList()
    {
        Assert.Throws<System.ArgumentException>(() =>
            LandcoverData.ParseTrees(@"{""trees"": []}"));
    }

    [Test]
    public void ParseFences_ValidInlineJson()
    {
        string json = @"{""posts"": [
            {""x"": 50, ""z"": 60, ""bearing_deg"": 90, ""cls"": ""stone_wall""},
            {""x"": 53, ""z"": 60, ""bearing_deg"": 90, ""cls"": ""rail_fence""}
        ]}";
        FencesDto dto = LandcoverData.ParseFences(json);
        Assert.AreEqual(2, dto.posts.Count);
        Assert.AreEqual(90f, dto.posts[0].bearing_deg);
        Assert.AreEqual("rail_fence", dto.posts[1].cls);
    }

    [Test]
    public void ParseFences_RejectsEmptyList()
    {
        Assert.Throws<System.ArgumentException>(() =>
            LandcoverData.ParseFences(@"{""posts"": []}"));
    }
}
