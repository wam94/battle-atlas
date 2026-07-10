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
}
