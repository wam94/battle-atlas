using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using BattleAtlas;

// Phase 11 integration flows (plan §13 PlayMode items): the first-entry
// content warning and its persistence, the source drawer from a Soldier
// View, entry-marker windowing, and the exact-state Atlas return through
// the HUD's authored-cut transition. The rig is the sync tests' pattern
// (camera + clock + player + dev proxy) plus a real BattleDirector over a
// synthetic terrain and a minimal fixture battle, and the real Resources/UI
// panel assets.
//
// Tests that play media need the gitignored dev proxy
// (reconstruction/scripts/generate_dev_proxy.sh); without it they end
// Inconclusive with instructions, never failing — media is not a committed
// input.
public class AtlasHudFlowTests
{
    const float StepTimeout = 20f;

    BattleClock clock;
    SoldierViewPlayer player;
    AtlasHud hud;
    BattleDirector director;
    GameObject cameraGo, terrainGo, directorGo, uiGo;
    Material material;
    int savedAck;
    bool hadAck;
    int savedIvAck;
    bool hadIvAck;

    static string IversonAckKey =>
        ContentWarningGate.KeyForViewpoint("iverson-forney-field");

    const string FixtureBattle = @"{
      ""name"": ""Test Battle (fixture)"",
      ""startTime"": 46800,
      ""endTime"": 10800,
      ""units"": [
        {
          ""id"": ""csa-garnett"",
          ""name"": ""Garnett's Test Brigade"",
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
      ],
      ""events"": [
        { ""id"": ""ev-fixture-fire"", ""kind"": ""musketry"",
          ""t0"": 8100, ""t1"": 8300, ""unitId"": ""csa-garnett"",
          ""confidence"": ""documented"", ""citation"": ""fixture event citation"" }
      ]
    }";

    static string ProxyPath =>
        SoldierViewPlayer.MediaPath("SoldierView/dev-timecode.proxy.mp4");

    // A PRODUCT (non-development) viewpoint window over the dev proxy so
    // the full marker->warning->enter loop runs without production media.
    static ViewpointSet TestViewpoints()
    {
        return new ViewpointSet
        {
            viewpoints = new[]
            {
                new ViewpointDefinition
                {
                    id = "test-hero",
                    title = "Test Hero Viewpoint",
                    unitId = "csa-garnett",
                    t0 = 8160,
                    t1 = 8170,
                    development = false,
                    editorialNote = "test fixture viewpoint",
                    claimIds = new[] { "claim-test-1" },
                    media = new ViewpointMedia
                    {
                        proxy = "SoldierView/dev-timecode.proxy.mp4",
                        full = null, fps = 30f, width = 1280, height = 720,
                    },
                },
            },
        };
    }

    [SetUp]
    public void SetUp()
    {
        hadAck = PlayerPrefs.HasKey(ContentWarningGate.PrefsKey);
        savedAck = PlayerPrefs.GetInt(ContentWarningGate.PrefsKey, 0);
        hadIvAck = PlayerPrefs.HasKey(IversonAckKey);
        savedIvAck = PlayerPrefs.GetInt(IversonAckKey, 0);

        cameraGo = new GameObject("TestCamera");
        cameraGo.tag = "MainCamera";
        cameraGo.AddComponent<Camera>();

        var terrainData = new TerrainData
        {
            heightmapResolution = 33,
            size = new Vector3(8507f, 30f, 8507f),
        };
        terrainGo = Terrain.CreateTerrainGameObject(terrainData);

        directorGo = new GameObject("TestDirector");
        clock = directorGo.AddComponent<BattleClock>();
        clock.StartTime = 46800f;
        clock.EndTime = 10800f;
        clock.Speed = 60f;
        clock.Playing = false;
        material = new Material(Shader.Find("Standard"));
        director = directorGo.AddComponent<BattleDirector>();
        director.battleJson = new TextAsset(FixtureBattle);
        director.terrain = terrainGo.GetComponent<Terrain>();
        director.clock = clock;
        director.unitMaterial = material;

        uiGo = new GameObject("TestAtlasUI");
        uiGo.SetActive(false); // wire everything before OnEnable/Start
        player = uiGo.AddComponent<SoldierViewPlayer>();
        player.clock = clock;
        var document = uiGo.AddComponent<UIDocument>();
        document.panelSettings = Resources.Load<PanelSettings>("UI/AtlasPanelSettings");
        document.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/AtlasHud");
        Assert.IsNotNull(document.panelSettings, "Resources/UI/AtlasPanelSettings missing");
        Assert.IsNotNull(document.visualTreeAsset, "Resources/UI/AtlasHud.uxml missing");
        hud = uiGo.AddComponent<AtlasHud>();
        hud.clock = clock;
        hud.player = player;
        hud.director = director;
        hud.viewpoints = TestViewpoints();
        uiGo.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        if (player != null && player.InSoldierView) player.Exit();
        Object.Destroy(uiGo);
        Object.Destroy(directorGo);
        Object.Destroy(terrainGo);
        Object.Destroy(cameraGo);
        Object.Destroy(material);
        if (hadAck) PlayerPrefs.SetInt(ContentWarningGate.PrefsKey, savedAck);
        else PlayerPrefs.DeleteKey(ContentWarningGate.PrefsKey);
        if (hadIvAck) PlayerPrefs.SetInt(IversonAckKey, savedIvAck);
        else PlayerPrefs.DeleteKey(IversonAckKey);
        PlayerPrefs.Save();
    }

    static void RequireProxyOrIgnore()
    {
        if (!File.Exists(ProxyPath))
            Assert.Ignore(
                "dev proxy missing — run reconstruction/scripts/generate_dev_proxy.sh "
                + $"(expected at {ProxyPath})");
    }

    IEnumerator WaitUntil(System.Func<bool> done, string what)
    {
        float deadline = Time.realtimeSinceStartup + StepTimeout;
        while (!done() && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.IsTrue(done(), $"timed out waiting for: {what}");
    }

    ViewpointDefinition Hero => hud.viewpoints.viewpoints[0];

    [UnityTest]
    public IEnumerator ContentWarning_GatesFirstEntry_AndPersists()
    {
        RequireProxyOrIgnore();
        PlayerPrefs.DeleteKey(ContentWarningGate.PrefsKey);
        yield return null; // let Start run
        clock.CurrentTime = 8165f;

        hud.RequestEnter(Hero);
        Assert.IsTrue(hud.WarningVisible, "first entry must show the content warning");
        Assert.IsFalse(player.InSoldierView, "entry must wait for acknowledgement");

        hud.AcknowledgeWarning();
        yield return WaitUntil(
            () => player.InSoldierView && !hud.Transitioning, "enter after acknowledge");

        // acknowledgement persisted (a fresh gate over real PlayerPrefs —
        // the next session's state)
        Assert.IsFalse(
            new ContentWarningGate(new PlayerPrefsStore(), 1).NeedsAcknowledgement,
            "acknowledgement must persist");

        hud.RequestExit();
        yield return WaitUntil(
            () => !player.InSoldierView && !hud.Transitioning, "exit");

        // second entry: no warning this time
        hud.RequestEnter(Hero);
        Assert.IsFalse(hud.WarningVisible, "acknowledged warning must not re-show");
        yield return WaitUntil(
            () => player.InSoldierView && !hud.Transitioning, "second enter");
    }

    // A fixture stand-in for the SECOND film's viewpoint: the real id
    // (so the committed per-viewpoint warning override binds) and its
    // cross-phase battleAsset, over the dev proxy so the flow runs
    // without production media. Window rides the proxy's 8160..8170.
    static ViewpointDefinition IversonFixtureViewpoint() =>
        new ViewpointDefinition
        {
            id = "iverson-forney-field",
            title = "With the Twelfth North Carolina (fixture window)",
            unitId = "csa-garnett",
            t0 = 8160,
            t1 = 8170,
            development = false,
            battleAsset = "gettysburg-july1-afternoon",
            editorialNote = "test fixture stand-in for the July 1 film",
            claimIds = new string[0],
            media = new ViewpointMedia
            {
                proxy = "SoldierView/dev-timecode.proxy.mp4",
                full = null, fps = 30f, width = 1280, height = 720,
            },
        };

    [UnityTest]
    public IEnumerator IversonWarning_RendersInEntryFlow_WithItsOwnAcknowledgement()
    {
        RequireProxyOrIgnore();
        // production shape: the July 1 afternoon phase is the loaded one
        // and the film's viewpoint declares it home (battleAsset)
        director.battleJson.name = "gettysburg-july1-afternoon";
        var iv = IversonFixtureViewpoint();
        hud.viewpoints = new ViewpointSet { viewpoints = new[] { iv } };
        yield return null; // let Start run

        // the Angle's warning was acknowledged long ago; the Iverson
        // film's own warning must still surface before ITS first entry
        PlayerPrefs.SetInt(ContentWarningGate.PrefsKey, 99);
        PlayerPrefs.DeleteKey(IversonAckKey);
        clock.CurrentTime = 8165f;

        hud.RequestEnter(iv);
        Assert.IsTrue(hud.WarningVisible,
            "the per-viewpoint warning must gate the film's first entry "
            + "even after the default warning was acknowledged");
        Assert.IsFalse(player.InSoldierView);

        // the modal carries the film's OWN authored text (the committed
        // override, iverson-viewpoint-design.md §7), not the Angle's
        var root = uiGo.GetComponent<UIDocument>().rootVisualElement;
        var committed = ContentWarningDoc.FromJson(File.ReadAllText(
            SoldierViewPlayer.MediaPath("SoldierView/content-warning.json")));
        var ov = committed.OverrideFor("iverson-forney-field");
        Assert.IsNotNull(ov, "committed content-warning.json must carry the override");
        Assert.AreEqual(ov.warning.body, root.Q<Label>("warning-body").text);
        StringAssert.Contains("Iverson's North Carolinians",
            root.Q<Label>("warning-body").text);
        StringAssert.Contains("12th North Carolina",
            root.Q<Label>("observer-body").text);

        hud.AcknowledgeWarning();
        yield return WaitUntil(
            () => player.InSoldierView && !hud.Transitioning,
            "enter after acknowledging the film's warning");

        // acknowledged under the film's own key; the default untouched
        Assert.GreaterOrEqual(PlayerPrefs.GetInt(IversonAckKey, 0), 1);
        Assert.AreEqual(99, PlayerPrefs.GetInt(ContentWarningGate.PrefsKey, 0));
    }

    [UnityTest]
    public IEnumerator CrossPhaseViewpoint_IsRefusedOffItsOwnPhase()
    {
        // the July 1 film must not be enterable while July 3 is loaded
        // (per-phase media honesty, per viewpoint)
        director.battleJson.name = "gettysburg-july3";
        var iv = IversonFixtureViewpoint();
        hud.viewpoints = new ViewpointSet { viewpoints = new[] { iv } };
        yield return null;
        PlayerPrefs.DeleteKey(IversonAckKey);
        clock.CurrentTime = 8165f;

        hud.RequestEnter(iv);
        yield return null;
        Assert.IsFalse(hud.WarningVisible,
            "no warning for a viewpoint whose phase is not loaded");
        Assert.IsFalse(player.InSoldierView,
            "entry must be refused off the viewpoint's home phase");
    }

    [UnityTest]
    public IEnumerator ContentWarning_DeclineStaysInAtlas()
    {
        yield return null;
        PlayerPrefs.DeleteKey(ContentWarningGate.PrefsKey);
        clock.CurrentTime = 8165f;
        hud.RequestEnter(Hero);
        Assert.IsTrue(hud.WarningVisible);
        hud.DeclineWarning();
        yield return null;
        Assert.IsFalse(hud.WarningVisible);
        Assert.IsFalse(player.InSoldierView, "decline must stay in the Atlas");
        Assert.AreEqual(8165f, clock.CurrentTime, 1e-3f);
        Assert.IsTrue(
            new ContentWarningGate(new PlayerPrefsStore(), 1).NeedsAcknowledgement,
            "declining must not count as acknowledgement");
    }

    [UnityTest]
    public IEnumerator SourceDrawer_FromSoldierView_ShowsViewpointUnitProvenance()
    {
        RequireProxyOrIgnore();
        yield return null;
        PlayerPrefs.SetInt(ContentWarningGate.PrefsKey, 99); // pre-acknowledged
        clock.CurrentTime = 8165f;
        hud.RequestEnter(Hero);
        yield return WaitUntil(
            () => player.InSoldierView && !hud.Transitioning, "enter");

        hud.OpenDrawerForActiveViewpoint();
        Assert.IsTrue(hud.DrawerVisible, "sources drawer must open from Soldier View");

        var root = uiGo.GetComponent<UIDocument>().rootVisualElement;
        Assert.AreEqual("Garnett's Test Brigade",
            root.Q<Label>("drawer-title").text);
        StringAssert.Contains("Confederate infantry brigade",
            root.Q<Label>("drawer-identity").text);
        StringAssert.Contains("men", root.Q<Label>("drawer-strength").text);
        // the drawer carries the fixture's citations and the viewpoint's own
        // editorial context
        bool sawKeyframeCite = false, sawEventCite = false, sawClaim = false;
        root.Q<ScrollView>("drawer-sources").Query<Label>().ForEach(l =>
        {
            if (l.text.Contains("fixture citation")) sawKeyframeCite = true;
            if (l.text.Contains("fixture event citation")) sawEventCite = true;
            if (l.text.Contains("claim-test-1")) sawClaim = true;
        });
        Assert.IsTrue(sawKeyframeCite, "keyframe citation must be listed");
        Assert.IsTrue(sawEventCite, "engagement event citation must be listed");
        Assert.IsTrue(sawClaim, "viewpoint claim ids must be listed");

        hud.RequestExit();
        yield return WaitUntil(
            () => !player.InSoldierView && !hud.Transitioning, "exit");
        Assert.IsFalse(hud.DrawerVisible,
            "the viewpoint drawer closes with the viewpoint");
    }

    [UnityTest]
    public IEnumerator ExitThroughHud_RestoresExactBattleTimeAndAtlasChrome()
    {
        RequireProxyOrIgnore();
        yield return null;
        PlayerPrefs.SetInt(ContentWarningGate.PrefsKey, 99);
        clock.CurrentTime = 8163f;
        float savedSpeed = clock.Speed;
        hud.RequestEnter(Hero);
        yield return WaitUntil(
            () => player.InSoldierView && !hud.Transitioning, "enter");
        Assert.AreEqual(1f, clock.Speed, 1e-3f, "media is real time");
        // let the in-view bar sync once: its slider range writes must NOT
        // seek the clock (the lowValue-clamp regression the screenshot run
        // caught — entry at 8163 snapped to the window start)
        yield return null;
        yield return WaitUntil(() => !player.SeekInProgress, "entry seek settle");
        Assert.AreEqual(8163f, clock.CurrentTime, 0.5f,
            "entering must land on the second the user chose");

        // hold somewhere else in the window, then leave
        player.Seek(8166.0);
        yield return WaitUntil(() => !player.SeekInProgress, "seek settle");
        float atExit = clock.CurrentTime;
        hud.RequestExit();
        yield return WaitUntil(
            () => !player.InSoldierView && !hud.Transitioning, "exit");

        Assert.AreEqual(atExit, clock.CurrentTime, 1e-3f,
            "Atlas must resume at the exact battle second");
        Assert.AreEqual(savedSpeed, clock.Speed, 1e-3f, "speed restored");
    }

    [UnityTest]
    public IEnumerator EntryMarkers_ExistOnlyInsideTheWindow()
    {
        yield return null; // Start
        var root = uiGo.GetComponent<UIDocument>().rootVisualElement;
        var markers = root.Q("entry-markers");

        clock.CurrentTime = 8000f; // outside
        yield return null;
        Assert.AreEqual(0, markers.childCount, "no marker outside the window");

        clock.CurrentTime = 8165f; // inside
        yield return null;
        Assert.AreEqual(1, markers.childCount, "one marker inside the window");
        StringAssert.Contains("Test Hero Viewpoint",
            ((Button)markers[0]).text);

        clock.CurrentTime = 8170f; // t1 boundary is exclusive
        yield return null;
        Assert.AreEqual(0, markers.childCount, "marker gone at t1");
    }

    // Day navigation (ADR 0005, day-expansion slice 1): the tabs come from
    // the REAL committed StreamingAssets manifest; an unreconstructed day
    // opens the honest empty state showing the manifest's own note, and
    // closing it returns the Atlas untouched. The fixture battle matches no
    // manifest phase, so no tab is highlighted active — also asserted.
    [UnityTest]
    public IEnumerator DayTabs_EmptyDayShowsHonestNote_AndCloses()
    {
        yield return null; // Start (loads Atlas/battle-manifest.json)
        var root = uiGo.GetComponent<UIDocument>().rootVisualElement;
        var tabs = root.Q("day-tabs");
        Assert.AreEqual(3, tabs.childCount, "July 1 / 2 / 3 tabs");
        Assert.AreEqual("July 1", ((Button)tabs[0]).text);
        yield return null; // BindActiveDay ran in Update
        for (int i = 0; i < tabs.childCount; i++)
            Assert.IsFalse(tabs[i].ClassListContains("day-on"),
                "fixture battle matches no manifest phase — no active tab");

        hud.OpenDayPanel(0); // July 1 — not reconstructed
        Assert.IsTrue(hud.DayPanelVisible);
        StringAssert.Contains("July 1", root.Q<Label>("day-title").text);
        // the manifest's own words render verbatim — the honest empty state
        bool foundNote = false;
        root.Q<ScrollView>("day-body").Query<Label>().ForEach(l =>
        {
            if (l.text != null && l.text.Contains("Not yet reconstructed"))
                foundNote = true;
        });
        Assert.IsTrue(foundNote, "the not-reconstructed note must render");

        hud.CloseDayPanel();
        Assert.IsFalse(hud.DayPanelVisible);
        float t = clock.CurrentTime;
        yield return null;
        Assert.AreEqual(t, clock.CurrentTime, 1e-3f,
            "browsing days must not touch the clock");
    }
}
