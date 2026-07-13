using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using BattleAtlas;

// In-HUD phase switching (the ADR 0005 deferred hot-swap, this slice):
//
//   1. FRESH-LAUNCH EQUIVALENCE — switching a running battle to another
//      phase and scrubbing produces byte-identical logical state (unit
//      positions / strengths / facings / formations / confidence at a
//      probe time) to a session that launched straight into that phase.
//      Run over the REAL authored July 1 morning battle file.
//   2. THE LEAK AUDIT — after a switch the director GameObject carries
//      exactly one battle's worth of components and runtime children
//      (audio sources, label pool); the old battle's unit ids resolve to
//      nothing.
//   3. THE HUD FLOW — AtlasHud.SwitchToPhase over the real committed
//      manifest and battle files: clock re-based to the target phase and
//      re-verified against the manifest's echo, per-phase moments
//      reloaded, the day tab highlight following the loaded phase,
//      Soldier View entry gated off phases the shipped media does not
//      address, and the per-phase clock memory restored on return.
public class PhaseSwitchFlowTests
{
    const float StepTimeout = 30f;
    const float ProbeT = 9900f; // ~10:15 LMT on the July 1 morning clock

    const string FixtureBattle = @"{
      ""name"": ""Test Battle (fixture)"",
      ""startTime"": 46800,
      ""endTime"": 10800,
      ""units"": [
        {
          ""id"": ""csa-fixture-only"",
          ""name"": ""Fixture Brigade"",
          ""side"": ""confederate"",
          ""frontage_m"": 400,
          ""depth_m"": 40,
          ""keyframes"": [
            { ""t"": 0, ""x"": 3200, ""z"": 4930, ""facing"": 88,
              ""formation"": ""line"", ""strength"": 1480,
              ""confidence"": ""documented"", ""citation"": ""fixture citation A"" },
            { ""t"": 10800, ""x"": 4400, ""z"": 4900, ""facing"": 88,
              ""formation"": ""line"", ""strength"": 700,
              ""confidence"": ""inferred"", ""citation"": ""fixture citation B"" }
          ]
        }
      ]
    }";

    static string July1MorningPath => Path.Combine(
        Application.dataPath, "Battle", "gettysburg-july1-morning.json");

    GameObject cameraGo, terrainGo;
    readonly System.Collections.Generic.List<GameObject> cleanup =
        new System.Collections.Generic.List<GameObject>();
    Material material;

    [SetUp]
    public void SetUp()
    {
        cameraGo = new GameObject("TestCamera");
        cameraGo.tag = "MainCamera";
        cameraGo.AddComponent<Camera>();
        var terrainData = new TerrainData
        {
            heightmapResolution = 33,
            size = new Vector3(8507f, 30f, 8507f),
        };
        terrainGo = Terrain.CreateTerrainGameObject(terrainData);
        material = new Material(Shader.Find("Standard"));
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in cleanup) Object.Destroy(go);
        cleanup.Clear();
        Object.Destroy(terrainGo);
        Object.Destroy(cameraGo);
        Object.Destroy(material);
    }

    (GameObject go, BattleClock clock, BattleDirector director) MakeDirector(
        string battleJson, string assetName)
    {
        var go = new GameObject($"director {assetName}");
        cleanup.Add(go);
        var clock = go.AddComponent<BattleClock>();
        clock.Playing = false;
        var director = go.AddComponent<BattleDirector>();
        director.battleJson = new TextAsset(battleJson) { name = assetName };
        director.terrain = terrainGo.GetComponent<Terrain>();
        director.clock = clock;
        director.unitMaterial = material;
        return (go, clock, director);
    }

    IEnumerator WaitUntil(System.Func<bool> done, string what)
    {
        float deadline = Time.realtimeSinceStartup + StepTimeout;
        while (!done() && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.IsTrue(done(), $"timed out waiting for: {what}");
    }

    // The production teardown/successor sequence, exactly as
    // AtlasHud.SwitchRoutine performs it (minus the HUD chrome).
    static IEnumerator SwitchDirector(
        GameObject host, string battleText, string assetName,
        System.Action<BattleDirector> assign)
    {
        Object.Destroy(host.GetComponent<ObscurationField>());
        Object.Destroy(host.GetComponent<AcousticField>());
        Object.Destroy(host.GetComponent<UnitLabelField>());
        BattleDirector old = host.GetComponent<BattleDirector>();
        BattleDirector next = BattleDirector.SpawnSuccessor(
            old, battleText, assetName);
        Object.Destroy(old);
        yield return null; // destroys complete; successor Start has run
        assign(next);
    }

    [UnityTest]
    public IEnumerator Switch_ThenScrub_IsByteIdenticalToFreshLaunch()
    {
        string july1Text = File.ReadAllText(July1MorningPath);
        BattleDto july1 = BattleLoader.Parse(july1Text);

        // session F: fresh launch straight into July 1 morning
        var (fGo, fClock, fresh) = MakeDirector(july1Text, "gettysburg-july1-morning");
        // session S: launches into the fixture battle, then switches
        var (sGo, sClock, switchedInitial) =
            MakeDirector(FixtureBattle, "fixture-battle");
        yield return null; // both Starts ran

        // S runs its first battle for a bit (state to tear down: symbol
        // meshes, salience latches, selection)
        sClock.CurrentTime = 5000f;
        switchedInitial.TrySelectUnit("csa-fixture-only");
        yield return null;

        BattleDirector switched = null;
        yield return SwitchDirector(
            sGo, july1Text, "gettysburg-july1-morning", d => switched = d);
        Assert.IsNotNull(switched);
        Assert.AreEqual("gettysburg-july1-morning", switched.BattleAssetName);

        // both sessions scrub to the probe time
        fClock.CurrentTime = ProbeT;
        sClock.CurrentTime = ProbeT;
        yield return null;

        // clock re-based identically
        Assert.AreEqual(fClock.StartTime, sClock.StartTime, 0f, "StartTime");
        Assert.AreEqual(fClock.EndTime, sClock.EndTime, 0f, "EndTime");

        // BYTE-IDENTICAL logical state: every unit of the target phase,
        // exact float equality (same parser, same track math, no carried
        // state) — position, strength, facing, formation, confidence
        Assert.Greater(july1.units.Count, 0);
        foreach (UnitDto u in july1.units)
        {
            Assert.IsTrue(fresh.TryGetUnitInfo(u.id, out var fInfo),
                $"fresh launch must know '{u.id}'");
            Assert.IsTrue(switched.TryGetUnitInfo(u.id, out var sInfo),
                $"switched session must know '{u.id}'");
            UnitState f = fInfo.Track.StateAt(ProbeT);
            UnitState s = sInfo.Track.StateAt(ProbeT);
            Assert.AreEqual(f.posXZ.x, s.posXZ.x, 0f, $"{u.id} x at t={ProbeT}");
            Assert.AreEqual(f.posXZ.y, s.posXZ.y, 0f, $"{u.id} z at t={ProbeT}");
            Assert.AreEqual(f.strength, s.strength, 0f, $"{u.id} strength");
            Assert.AreEqual(f.facingDeg, s.facingDeg, 0f, $"{u.id} facing");
            Assert.AreEqual(f.formation, s.formation, $"{u.id} formation");
            Assert.AreEqual(f.confidence, s.confidence, $"{u.id} confidence");
        }

        // and the old battle is GONE: its unit resolves to nothing, and
        // no stale selection survived the switch
        Assert.IsFalse(switched.TryGetUnitInfo("csa-fixture-only", out _),
            "the pre-switch battle's units must not survive the switch");
        Assert.IsFalse(switched.TryGetSelected(out _, out _),
            "selection must not survive the switch");
    }

    [UnityTest]
    public IEnumerator Switch_LeaksNoComponentsOrRuntimeChildren()
    {
        var (sGo, sClock, initial) = MakeDirector(FixtureBattle, "fixture-battle");
        yield return null; // Start ran (spawned the per-battle components)
        yield return null; // sibling Starts ran (audio sources, label pool)

        string july1Text = File.ReadAllText(July1MorningPath);
        BattleDirector switched = null;
        yield return SwitchDirector(
            sGo, july1Text, "gettysburg-july1-morning", d => switched = d);
        yield return null; // successor's siblings ran their Starts
        yield return null; // deferred child destroys fully flushed

        // exactly ONE battle's worth of components on the host
        Assert.AreEqual(1, sGo.GetComponents<BattleDirector>().Length, "directors");
        Assert.AreEqual(1, sGo.GetComponents<ObscurationField>().Length, "obscuration");
        Assert.AreEqual(1, sGo.GetComponents<AcousticField>().Length, "acoustic");
        Assert.AreEqual(1, sGo.GetComponents<UnitLabelField>().Length, "labels");

        // exactly ONE battle's worth of runtime children: 1 ambient + 7
        // spatial audio sources, 48 pooled labels — the old set destroyed
        // with its components (the OnDestroy contracts this slice added)
        int audioChildren = 0, labelChildren = 0;
        foreach (Transform child in sGo.transform)
        {
            if (child.GetComponent<AudioSource>() != null) audioChildren++;
            if (child.name.StartsWith("unit label")) labelChildren++;
        }
        Assert.AreEqual(1 + AcousticField.SpatialSourceCount, audioChildren,
            "audio source children must not accumulate across switches");
        Assert.AreEqual(UnitLabelField.PoolSize, labelChildren,
            "label pool children must not accumulate across switches");
    }

    [UnityTest]
    public IEnumerator HudSwitch_RebasesClock_ReloadsMoments_GatesSoldierView_AndRemembersPositions()
    {
        // the AtlasHudFlowTests rig: fixture battle + real UI resources +
        // the real committed manifest; the battleJson is NAMED so the
        // Soldier View home-phase gate engages (fixture-battle is home)
        var (dirGo, clock, director) = MakeDirector(FixtureBattle, "fixture-battle");
        clock.StartTime = 46800f;
        clock.EndTime = 10800f;

        var uiGo = new GameObject("TestAtlasUI");
        cleanup.Add(uiGo);
        uiGo.SetActive(false);
        var player = uiGo.AddComponent<SoldierViewPlayer>();
        player.clock = clock;
        var document = uiGo.AddComponent<UIDocument>();
        document.panelSettings = Resources.Load<PanelSettings>("UI/AtlasPanelSettings");
        document.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/AtlasHud");
        Assert.IsNotNull(document.panelSettings, "Resources/UI/AtlasPanelSettings missing");
        Assert.IsNotNull(document.visualTreeAsset, "Resources/UI/AtlasHud.uxml missing");
        var hud = uiGo.AddComponent<AtlasHud>();
        hud.clock = clock;
        hud.player = player;
        hud.director = director;
        hud.viewpoints = new ViewpointSet
        {
            viewpoints = new[]
            {
                new ViewpointDefinition
                {
                    id = "test-hero", title = "Test Hero Viewpoint",
                    unitId = "csa-fixture-only", t0 = 8160, t1 = 8170,
                    development = false, editorialNote = "fixture",
                    media = new ViewpointMedia
                    {
                        proxy = "SoldierView/dev-timecode.proxy.mp4",
                        full = null, fps = 30f, width = 1280, height = 720,
                    },
                },
            },
        };
        uiGo.SetActive(true);
        yield return null; // Starts ran (manifest loaded from StreamingAssets)

        var root = uiGo.GetComponent<UIDocument>().rootVisualElement;
        var manifest = PhaseManifest.FromJson(File.ReadAllText(Path.Combine(
            Application.streamingAssetsPath, "Atlas/battle-manifest.json")));
        PhaseDto july1Morning = null, july2Afternoon = null;
        foreach (DayDto day in manifest.days)
            foreach (PhaseDto p in day.phases)
            {
                if (p.id == "july1-morning") july1Morning = p;
                if (p.id == "july2-afternoon") july2Afternoon = p;
            }
        Assert.IsNotNull(july1Morning);
        Assert.IsNotNull(july2Afternoon);

        // the fixture battle is home: markers exist inside the window
        clock.CurrentTime = 8165f;
        yield return null;
        Assert.AreEqual(1, root.Q("entry-markers").childCount,
            "home phase must show its entry marker");

        // ---- switch 1: to July 1 morning
        hud.SwitchToPhase(july1Morning);
        Assert.IsTrue(hud.Switching, "switch must start");
        yield return WaitUntil(() => !hud.Switching, "switch to july1-morning");
        Assert.Greater(hud.LastSwitchMs, 0f, "switch time must be measured");
        Assert.AreNotSame(director, hud.director, "a switch spawns a successor");
        Assert.AreEqual("gettysburg-july1-morning", hud.director.BattleAssetName);
        Assert.AreEqual(27000f, clock.StartTime, 0f, "07:30 LMT phase start");
        Assert.AreEqual(19800f, clock.EndTime, 0f, "phase duration");
        Assert.AreEqual(0f, clock.CurrentTime, 0f,
            "a never-visited phase starts at its t=0");
        Assert.IsFalse(clock.Playing, "a switch lands paused");

        // per-phase moments reloaded (the July 1 morning moments file)
        Assert.Greater(root.Q("timeline-markers").childCount, 0,
            "the target phase's own moments must render");

        // Soldier View gated: the shipped (fixture) viewpoints do not
        // address this phase — no markers even inside the t-window
        clock.CurrentTime = 8165f;
        yield return null;
        Assert.AreEqual(0, root.Q("entry-markers").childCount,
            "no Soldier View entry on a phase the media does not address");

        // day tab highlight follows the loaded phase (July 1 = tab 0)
        var tabs = root.Q("day-tabs");
        Assert.IsTrue(tabs[0].ClassListContains("day-on"),
            "July 1 tab must light after switching to its phase");

        // ---- switch 2: to July 2 afternoon, leaving July 1 at t=5000
        clock.CurrentTime = 5000f;
        hud.SwitchToPhase(july2Afternoon);
        yield return WaitUntil(() => !hud.Switching, "switch to july2-afternoon");
        Assert.AreEqual(55800f, clock.StartTime, 0f);
        Assert.AreEqual(0f, clock.CurrentTime, 0f);

        // ---- switch 3: back to July 1 morning — the remembered position
        hud.SwitchToPhase(july1Morning);
        yield return WaitUntil(() => !hud.Switching, "switch back to july1-morning");
        Assert.AreEqual(5000f, clock.CurrentTime, 0f,
            "returning to a phase resumes where the user left it");
    }
}
