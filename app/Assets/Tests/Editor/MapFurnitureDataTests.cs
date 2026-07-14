using System;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class MapFurnitureDataTests
{
    const string TwoFeatureJson = @"{
 ""name"": ""test"",
 ""features"": [
  {
   ""id"": ""road-a"",
   ""kind"": ""line"",
   ""cls"": ""pike"",
   ""points"": [[0.0, 0.0], [10.5, 20.25], [30.0, 5.0]],
   ""source"": ""test sheet"",
   ""confidence"": ""documented"",
   ""note"": ""a note""
  },
  {
   ""id"": ""block-b"",
   ""kind"": ""polygon"",
   ""cls"": ""town_block"",
   ""points"": [[0.0, 0.0], [10.0, 0.0], [10.0, 10.0], [0.0, 10.0]],
   ""confidence"": ""inferred""
  }
 ]
}";

    [Test]
    public void Parse_ReadsMetadataAndPointsForEveryFeature()
    {
        MapFurnitureDto dto = MapFurnitureData.Parse(TwoFeatureJson);

        Assert.AreEqual("test", dto.name);
        Assert.AreEqual(2, dto.features.Count);

        MapFurnitureFeatureDto road = dto.features[0];
        Assert.AreEqual("road-a", road.id);
        Assert.AreEqual("line", road.kind);
        Assert.AreEqual("pike", road.cls);
        Assert.AreEqual("documented", road.confidence);
        Assert.AreEqual("test sheet", road.source);
        Assert.AreEqual("a note", road.note);
        Assert.AreEqual(3, road.points.Length);
        Assert.AreEqual(new Vector2(0f, 0f), road.points[0]);
        Assert.AreEqual(new Vector2(10.5f, 20.25f), road.points[1]);
        Assert.AreEqual(new Vector2(30f, 5f), road.points[2]);

        MapFurnitureFeatureDto block = dto.features[1];
        Assert.AreEqual("block-b", block.id);
        Assert.AreEqual("polygon", block.kind);
        Assert.AreEqual(4, block.points.Length);
        Assert.AreEqual(new Vector2(10f, 10f), block.points[2]);
        Assert.IsNull(block.source); // optional field, absent in the JSON
    }

    [Test]
    public void Parse_HandlesNegativeAndScientificNotationCoordinates()
    {
        const string json = @"{""name"":""t"",""features"":[
            {""id"":""x"",""kind"":""line"",""cls"":""run"",
             ""points"":[[-12.3,1e2],[4.5e-1,-0.0]],
             ""confidence"":""inferred""}
        ]}";
        MapFurnitureDto dto = MapFurnitureData.Parse(json);
        Vector2[] pts = dto.features[0].points;
        Assert.AreEqual(2, pts.Length);
        Assert.AreEqual(-12.3f, pts[0].x, 1e-4f);
        Assert.AreEqual(100f, pts[0].y, 1e-4f);
        Assert.AreEqual(0.45f, pts[1].x, 1e-4f);
    }

    [Test]
    public void Parse_ThrowsOnNoFeatures()
    {
        Assert.Throws<ArgumentException>(() =>
            MapFurnitureData.Parse(@"{""name"":""empty"",""features"":[]}"));
    }

    [Test]
    public void Parse_ThrowsOnZeroPointFeature()
    {
        const string json = @"{""name"":""t"",""features"":[
            {""id"":""x"",""kind"":""line"",""cls"":""run"",""points"":[],""confidence"":""inferred""}
        ]}";
        Assert.Throws<InvalidOperationException>(() => MapFurnitureData.Parse(json));
    }

    [Test]
    public void ParsePointsArrays_FindsOneArrayPerFeatureInDocumentOrder()
    {
        var arrays = MapFurnitureData.ParsePointsArrays(TwoFeatureJson);
        Assert.AreEqual(2, arrays.Count);
        Assert.AreEqual(3, arrays[0].Length);
        Assert.AreEqual(4, arrays[1].Length);
    }
}
