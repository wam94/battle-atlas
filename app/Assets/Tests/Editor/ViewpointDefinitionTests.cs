using System.IO;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class ViewpointDefinitionTests
{
    static string CommittedJsonPath =>
        Path.Combine(Application.streamingAssetsPath, "SoldierView/viewpoints.json");

    [Test]
    public void CommittedViewpointsFile_ParsesAndValidates()
    {
        var set = ViewpointSet.FromJson(File.ReadAllText(CommittedJsonPath));
        Assert.AreEqual(1, set.viewpoints.Length);
        var vp = set.viewpoints[0];
        Assert.AreEqual("dev-timecode", vp.id);
        Assert.AreEqual(8160.0, vp.t0, 1e-9);
        Assert.AreEqual(8170.0, vp.t1, 1e-9);
        Assert.AreEqual("SoldierView/dev-timecode.proxy.mp4", vp.media.proxy);
        Assert.IsFalse(vp.media.HasFull, "dev viewpoint has no full media yet");
        Assert.AreEqual(30f, vp.media.fps);
        Assert.IsNull(vp.Validate());
    }

    [Test]
    public void FromJson_RejectsInvertedWindow()
    {
        string json = File.ReadAllText(CommittedJsonPath)
            .Replace("\"t1\": 8170", "\"t1\": 8150");
        var ex = Assert.Throws<System.ArgumentException>(
            () => ViewpointSet.FromJson(json));
        StringAssert.Contains("t1", ex.Message);
    }

    [Test]
    public void FromJson_RejectsMissingProxy()
    {
        string json = File.ReadAllText(CommittedJsonPath)
            .Replace("\"SoldierView/dev-timecode.proxy.mp4\"", "\"\"");
        Assert.Throws<System.ArgumentException>(() => ViewpointSet.FromJson(json));
    }

    [Test]
    public void FromJson_RejectsMissingViewpointsArray()
    {
        Assert.Throws<System.ArgumentException>(() => ViewpointSet.FromJson("{}"));
    }
}
