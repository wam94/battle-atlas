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
        // Phase 1 dev fixture. The Iverson's-field design-stage viewpoint
        // (second film) is third. The plan's two named Angle extensions
        // (webb-wall OP-2, cushing-canister OP-3) are fourth and fifth.
        Assert.AreEqual(5, set.viewpoints.Length);

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

        // The second film's viewpoint (Iverson's field, production slice).
        // NOTE its t0/t1 ride the July 1 AFTERNOON phase clock (startTime
        // 46800), not the July 3 clock — battleAsset declares that home
        // phase and gates entry to it (per-phase media honesty).
        var iv = set.viewpoints[2];
        Assert.AreEqual("iverson-forney-field", iv.id);
        Assert.AreEqual("csa-12nc", iv.unitId);
        Assert.AreEqual(184, iv.slotId);
        Assert.AreEqual(5830.0, iv.t0, 1e-9);
        Assert.AreEqual(7040.0, iv.t1, 1e-9);
        Assert.AreEqual(1.66f, iv.camera.eyeHeightM);
        Assert.AreEqual("gettysburg-july1-afternoon", iv.battleAsset);
        Assert.AreEqual("first_person", iv.viewKind);
        Assert.IsTrue(iv.media.HasFull, "the production slice sets the full media path");
        Assert.AreEqual("SoldierView/iverson-forney-field.full.mp4", iv.media.full);
        StringAssert.Contains("Representative unnamed soldier", iv.editorialNote);
        Assert.IsNull(iv.Validate());

        // The defender's view at the outer-angle wall (OP-2). Same July 3
        // clock and window as the hero viewpoint: the SAME compiled bundle
        // states, seen from the receiving line.
        var webb = set.viewpoints[3];
        Assert.AreEqual("webb-wall", webb.id);
        Assert.AreEqual("us-71pa", webb.unitId);
        Assert.AreEqual(230, webb.slotId);
        Assert.AreEqual(8160.0, webb.t0, 1e-9);
        Assert.AreEqual(8820.0, webb.t1, 1e-9);
        Assert.AreEqual(1.66f, webb.camera.eyeHeightM);
        Assert.AreEqual(68f, webb.camera.fovDeg);
        Assert.AreEqual(0.35f, webb.camera.stabilization);
        Assert.AreEqual("first_person", webb.viewKind);
        Assert.IsTrue(webb.media.HasFull);
        Assert.AreEqual("SoldierView/webb-wall.full.mp4", webb.media.full);
        StringAssert.Contains("Representative unnamed soldier", webb.editorialNote);
        StringAssert.Contains("ED-22", webb.editorialNote);
        Assert.IsNull(webb.Validate());

        // The gun-crew view at Cushing's battery (OP-3). The observer is
        // an unnamed crew position, never Lt. Cushing himself (ED-3).
        var cc = set.viewpoints[4];
        Assert.AreEqual("cushing-canister", cc.id);
        Assert.AreEqual("us-btty-cushing", cc.unitId);
        Assert.AreEqual(44, cc.slotId);
        Assert.AreEqual(8400.0, cc.t0, 1e-9);
        Assert.AreEqual(8760.0, cc.t1, 1e-9);
        Assert.AreEqual(1.66f, cc.camera.eyeHeightM);
        Assert.AreEqual("first_person", cc.viewKind);
        Assert.IsTrue(cc.media.HasFull);
        Assert.AreEqual("SoldierView/cushing-canister.full.mp4", cc.media.full);
        StringAssert.Contains("NOT Lieutenant Cushing", cc.editorialNote);
        StringAssert.Contains("ED-3", cc.editorialNote);
        Assert.IsNull(cc.Validate());

        // the July 3 viewpoints keep the set-home default (no battleAsset)
        Assert.IsTrue(string.IsNullOrEmpty(hero.battleAsset));
        Assert.IsTrue(string.IsNullOrEmpty(vp.battleAsset));
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
