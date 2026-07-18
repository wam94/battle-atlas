using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using BattleAtlas;

// Pins the first-launch graphic-content warning gate (plan §9.2/§10): the
// warning must show before the first Soldier View entry, be remembered per
// authored version, and re-surface when the authored text's version rises.
// The committed authored text itself is validated for the fields the modal
// binds.
public class ContentWarningGateTests
{
    class FakeStore : IAcknowledgementStore
    {
        public readonly Dictionary<string, int> Values = new Dictionary<string, int>();
        public int SaveCalls;

        public int GetInt(string key, int fallback) =>
            Values.TryGetValue(key, out int v) ? v : fallback;

        public void SetInt(string key, int value) => Values[key] = value;

        public void Save() => SaveCalls++;
    }

    [Test]
    public void FirstEntry_NeedsAcknowledgement()
    {
        var gate = new ContentWarningGate(new FakeStore(), 1);
        Assert.IsTrue(gate.NeedsAcknowledgement);
    }

    [Test]
    public void Acknowledge_PersistsAndSuppresses()
    {
        var store = new FakeStore();
        var gate = new ContentWarningGate(store, 1);
        gate.Acknowledge();
        Assert.IsFalse(gate.NeedsAcknowledgement);
        Assert.AreEqual(1, store.SaveCalls); // persisted, not just cached
        // a NEW gate over the SAME store (a later session) stays satisfied
        Assert.IsFalse(new ContentWarningGate(store, 1).NeedsAcknowledgement);
    }

    [Test]
    public void NewWarningVersion_ResurfacesTheWarning()
    {
        var store = new FakeStore();
        new ContentWarningGate(store, 1).Acknowledge();
        Assert.IsTrue(new ContentWarningGate(store, 2).NeedsAcknowledgement);
    }

    [Test]
    public void InvalidVersion_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => new ContentWarningGate(new FakeStore(), 0));
    }

    [Test]
    public void CommittedWarningDoc_CarriesTheModalFields()
    {
        string path = UnityEngine.Application.dataPath
            + "/StreamingAssets/SoldierView/content-warning.json";
        ContentWarningDoc doc = ContentWarningDoc.FromJson(File.ReadAllText(path));
        Assert.GreaterOrEqual(doc.version, 1);
        Assert.IsNotEmpty(doc.warning.title);
        Assert.IsNotEmpty(doc.warning.body);
        Assert.IsNotEmpty(doc.warning.acknowledgeLabel);
        Assert.IsNotEmpty(doc.warning.declineLabel);
        Assert.IsNotEmpty(doc.representativeObserver.shortLine);
        Assert.IsNotEmpty(doc.representativeObserver.body);
    }

    [Test]
    public void MalformedWarningDoc_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => ContentWarningDoc.FromJson("{}"));
        Assert.Throws<ArgumentException>(() => ContentWarningDoc.FromJson(
            "{\"version\":0,\"warning\":{\"body\":\"b\",\"acknowledgeLabel\":\"a\","
            + "\"declineLabel\":\"d\"}}"));
    }

    // ---------------- per-viewpoint overrides (Iverson production slice)

    const string DocWithOverride =
        "{\"version\":1,\"warning\":{\"title\":\"T\",\"body\":\"B\","
        + "\"acknowledgeLabel\":\"A\",\"declineLabel\":\"D\"},"
        + "\"representativeObserver\":{\"title\":\"OT\",\"body\":\"OB\","
        + "\"shortLine\":\"OS\"},"
        + "\"viewpointOverrides\":[{\"viewpointId\":\"vp-x\",\"version\":3,"
        + "\"warning\":{\"title\":\"XT\",\"body\":\"XB\"},"
        + "\"representativeObserver\":{\"title\":\"XOT\",\"body\":\"XOB\","
        + "\"shortLine\":\"XOS\"}}]}";

    [Test]
    public void Override_ResolvesPerViewpoint_AndFallsBackToDefault()
    {
        var doc = ContentWarningDoc.FromJson(DocWithOverride);
        // the overridden viewpoint gets its own text; missing button
        // labels fall back to the default's (shared mechanics)
        var w = doc.WarningFor("vp-x");
        Assert.AreEqual("XT", w.title);
        Assert.AreEqual("XB", w.body);
        Assert.AreEqual("A", w.acknowledgeLabel);
        Assert.AreEqual("D", w.declineLabel);
        Assert.AreEqual("XOS", doc.ObserverFor("vp-x").shortLine);
        // any other viewpoint (and null) keeps the default
        Assert.AreEqual("B", doc.WarningFor("vp-other").body);
        Assert.AreEqual("B", doc.WarningFor(null).body);
        Assert.AreEqual("OS", doc.ObserverFor("vp-other").shortLine);
        Assert.IsNull(doc.OverrideFor("vp-other"));
        Assert.AreEqual(3, doc.OverrideFor("vp-x").version);
    }

    [Test]
    public void OverrideAcknowledgement_IsPerViewpoint()
    {
        // acknowledging the default warning says nothing about a film
        // with its own override — and vice versa
        var store = new FakeStore();
        var def = new ContentWarningGate(store, 1);
        var vpx = new ContentWarningGate(store, 3,
            ContentWarningGate.KeyForViewpoint("vp-x"));
        def.Acknowledge();
        Assert.IsFalse(def.NeedsAcknowledgement);
        Assert.IsTrue(vpx.NeedsAcknowledgement,
            "the default acknowledgement must not satisfy an override gate");
        vpx.Acknowledge();
        Assert.IsFalse(vpx.NeedsAcknowledgement);
        Assert.AreEqual(1, store.GetInt(ContentWarningGate.PrefsKey, 0));
        Assert.AreEqual(3, store.GetInt(
            ContentWarningGate.KeyForViewpoint("vp-x"), 0));
    }

    [Test]
    public void MalformedOverride_IsRejected()
    {
        // override without a body
        Assert.Throws<ArgumentException>(() => ContentWarningDoc.FromJson(
            "{\"version\":1,\"warning\":{\"body\":\"b\",\"acknowledgeLabel\":\"a\","
            + "\"declineLabel\":\"d\"},\"viewpointOverrides\":[{"
            + "\"viewpointId\":\"vp-x\",\"version\":1,\"warning\":{\"title\":\"t\"}}]}"));
        // override with version 0
        Assert.Throws<ArgumentException>(() => ContentWarningDoc.FromJson(
            "{\"version\":1,\"warning\":{\"body\":\"b\",\"acknowledgeLabel\":\"a\","
            + "\"declineLabel\":\"d\"},\"viewpointOverrides\":[{"
            + "\"viewpointId\":\"vp-x\",\"version\":0,\"warning\":{\"body\":\"xb\"}}]}"));
    }

    [Test]
    public void CommittedWarningDoc_CarriesTheIversonOverride()
    {
        // the design slice's text (iverson-viewpoint-design.md §7) ships
        // BESIDE the Angle warning with its own acknowledgement
        string path = UnityEngine.Application.dataPath
            + "/StreamingAssets/SoldierView/content-warning.json";
        ContentWarningDoc doc = ContentWarningDoc.FromJson(File.ReadAllText(path));
        var ov = doc.OverrideFor("iverson-forney-field");
        Assert.IsNotNull(ov, "iverson-forney-field warning override must ship");
        Assert.GreaterOrEqual(ov.version, 1);
        StringAssert.Contains("Iverson's North Carolinians", ov.warning.body);
        StringAssert.Contains("12th North Carolina",
            ov.representativeObserver.body);
        // the default (Angle) warning is untouched by the addition
        StringAssert.Contains("at the Angle", doc.warning.body);
        Assert.AreNotEqual(doc.warning.body, doc.WarningFor("iverson-forney-field").body);
    }
}
