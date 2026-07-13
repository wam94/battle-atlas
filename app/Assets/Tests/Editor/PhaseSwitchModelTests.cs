using System.IO;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

// In-HUD phase switching (the ADR 0005 deferred hot-swap): the pure
// resolution/gating helpers and the SpawnSuccessor wiring contract. The
// runtime teardown/rebuild flow itself is pinned by the PlayMode suite
// (PhaseSwitchFlowTests — fresh-launch equivalence and the leak audit).
public class PhaseSwitchModelTests
{
    // ------------------------------------------ battle-file resolution

    [Test]
    public void BattleFileCandidates_FullPriorityOrder()
    {
        string[] c = HudModel.BattleFileCandidates(
            "gettysburg-july1-morning.json",
            "/captures/battledir",
            "/build/StreamingAssets",
            "/project/Assets",
            "/repo/app/Assets/Battle/gettysburg-july3.json");
        Assert.AreEqual(4, c.Length);
        // 1. explicit -battleDir wins
        Assert.AreEqual(
            Path.Combine("/captures/battledir", "gettysburg-july1-morning.json"), c[0]);
        // 2. the bundle's StreamingAssets/Battle (standalone builds)
        Assert.AreEqual(
            Path.Combine("/build/StreamingAssets", "Battle", "gettysburg-july1-morning.json"),
            c[1]);
        // 3. Assets/Battle (the editor and PlayMode tests)
        Assert.AreEqual(
            Path.Combine("/project/Assets", "Battle", "gettysburg-july1-morning.json"),
            c[2]);
        // 4. the -battleFile override's own directory (sibling phases)
        Assert.AreEqual(
            Path.Combine("/repo/app/Assets/Battle", "gettysburg-july1-morning.json"),
            c[3]);
    }

    [Test]
    public void BattleFileCandidates_SkipsAbsentRoots()
    {
        string[] c = HudModel.BattleFileCandidates(
            "x.json", null, "/sa", "/data", null);
        Assert.AreEqual(2, c.Length);
        Assert.AreEqual(Path.Combine("/sa", "Battle", "x.json"), c[0]);
        Assert.AreEqual(Path.Combine("/data", "Battle", "x.json"), c[1]);
    }

    [Test]
    public void BattleFileCandidates_EditorResolvesTheRealPhaseFiles()
    {
        // the editor-path candidate must point at the ACTUAL authored
        // battle files — the in-HUD switcher depends on it in PlayMode
        string[] c = HudModel.BattleFileCandidates(
            "gettysburg-july1-morning.json", null, null,
            Application.dataPath, null);
        Assert.AreEqual(1, c.Length);
        Assert.IsTrue(File.Exists(c[0]),
            $"expected the authored battle file at {c[0]}");
    }

    // ------------------------------------- Soldier View per-phase gate

    [Test]
    public void ViewpointsApplyTo_OnlyTheHomePhase()
    {
        // the shipped film addresses one phase's clock and cast
        Assert.IsTrue(HudModel.ViewpointsApplyTo(
            "gettysburg-july3", "gettysburg-july3"));
        Assert.IsFalse(HudModel.ViewpointsApplyTo(
            "gettysburg-july3", "gettysburg-july1-morning"));
        Assert.IsFalse(HudModel.ViewpointsApplyTo(
            "gettysburg-july3", "gettysburg-july2-evening"));
        Assert.IsFalse(HudModel.ViewpointsApplyTo("gettysburg-july3", null));
    }

    [Test]
    public void ViewpointsApplyTo_EmptyHomeAppliesEverywhere()
    {
        // fixture rigs wire viewpoints over unnamed TextAssets — they own
        // their own truth and must keep working unchanged
        Assert.IsTrue(HudModel.ViewpointsApplyTo(null, "anything"));
        Assert.IsTrue(HudModel.ViewpointsApplyTo("", "anything"));
    }

    // ------------------------------------------- successor wiring

    [Test]
    public void SpawnSuccessor_CopiesAllWiring_AndInjectsThePendingBattle()
    {
        var go = new GameObject("successor test");
        var terrainGo = new GameObject("terrain");
        Material m1 = null, m2 = null, m3 = null, m4 = null, m5 = null, m6 = null;
        try
        {
            var clock = go.AddComponent<BattleClock>();
            var old = go.AddComponent<BattleDirector>();
            old.battleJson = new TextAsset("{}") { name = "serialized-battle" };
            old.terrain = terrainGo.AddComponent<Terrain>();
            old.clock = clock;
            Shader s = Shader.Find("Standard");
            old.unitMaterial = m1 = new Material(s);
            old.soldierMaterial = m2 = new Material(s);
            old.flagMaterial = m3 = new Material(s);
            old.symbolMaterial = m4 = new Material(s);
            old.smokeMaterial = m5 = new Material(s);
            old.dustMaterial = m6 = new Material(s);
            old.commandOverlayJson = new TextAsset("{}");

            BattleDirector next = BattleDirector.SpawnSuccessor(
                old, "{\"name\":\"next\"}", "gettysburg-july1-morning");

            Assert.AreNotSame(old, next);
            Assert.AreSame(old.gameObject, next.gameObject);
            Assert.AreSame(old.battleJson, next.battleJson);
            Assert.AreSame(old.terrain, next.terrain);
            Assert.AreSame(old.clock, next.clock);
            Assert.AreSame(old.unitMaterial, next.unitMaterial);
            Assert.AreSame(old.soldierMaterial, next.soldierMaterial);
            Assert.AreSame(old.flagMaterial, next.flagMaterial);
            Assert.AreSame(old.symbolMaterial, next.symbolMaterial);
            Assert.AreSame(old.smokeMaterial, next.smokeMaterial);
            Assert.AreSame(old.dustMaterial, next.dustMaterial);
            Assert.AreSame(old.commandOverlayJson, next.commandOverlayJson);
            Assert.AreEqual("{\"name\":\"next\"}", next.pendingBattleText);
            Assert.AreEqual("gettysburg-july1-morning", next.pendingAssetName);
            // the asset-name precedence: an in-session switch outranks the
            // serialized asset; the serialized name stays visible for the
            // Soldier View home-phase gate
            Assert.AreEqual("gettysburg-july1-morning", next.BattleAssetName);
            Assert.AreEqual("serialized-battle", next.SerializedAssetName);
        }
        finally
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(terrainGo);
            foreach (var m in new[] { m1, m2, m3, m4, m5, m6 })
                if (m != null) Object.DestroyImmediate(m);
        }
    }
}
