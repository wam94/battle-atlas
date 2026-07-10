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
        // Phase 9: the required hero viewpoint (plan §3.4) joins the
        // Phase 1 dev fixture.
        Assert.AreEqual(2, set.viewpoints.Length);

        var hero = set.viewpoints[0];
        Assert.AreEqual("garnett-road-to-angle", hero.id);
        Assert.AreEqual("csa-garnett", hero.unitId);
        Assert.AreEqual(881, hero.slotId);
        Assert.AreEqual(8160.0, hero.t0, 1e-9);
        Assert.AreEqual(8820.0, hero.t1, 1e-9);
        Assert.AreEqual(1.66f, hero.camera.eyeHeightM);
        Assert.AreEqual(68f, hero.camera.fovDeg);
        Assert.AreEqual(0.35f, hero.camera.stabilization);
        // Phase 10 delivered the production encode; the media FILE stays
        // gitignored (GitHub Releases artifact), only the path is metadata.
        Assert.IsTrue(hero.media.HasFull, "Phase 10 sets the full media path");
        Assert.AreEqual("SoldierView/garnett-road-to-angle.full.mp4",
            hero.media.full);
        StringAssert.Contains("Representative unnamed soldier", hero.editorialNote);
        Assert.IsNull(hero.Validate());

        var vp = set.viewpoints[1];
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
