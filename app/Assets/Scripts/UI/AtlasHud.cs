using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace BattleAtlas
{
    // Phase 11 production HUD: the retained-mode (UI Toolkit) replacement
    // for the IMGUI TimelineHud/SoldierViewHud placeholders. One controller
    // drives the whole plan-§10 minimum surface:
    //
    //   - transport (play/pause, 1x/10x/60x), wall clock, day/slice
    //     masthead, phase + minimal conditions readout;
    //   - the timeline with cited moment markers and the Soldier View
    //     window band;
    //   - the unit source/provenance drawer (identity, strength, activity,
    //     citations — ProvenanceDrawer builds every line) with follow;
    //   - Soldier View entry markers (window-gated), the first-entry
    //     graphic-content warning (version-persisted), the authored-cut
    //     fade transition, the in-view transport, and exact-state return;
    //   - the credits view generated from the asset manifest.
    //
    // All decidable logic lives in pure classes (HudModel, ProvenanceDrawer,
    // ContentWarningGate, MomentSet, CreditsManifest); this component is
    // wiring and per-frame sync. It is created at scene load by
    // SoldierViewBootstrap — never serialized into the scene — so scene
    // files stay untouched by UI iteration.
    public class AtlasHud : MonoBehaviour
    {
        public BattleClock clock;
        public BattleDirector director;     // null in Soldier-View-only rigs
        public SunDirector sun;
        public ReliefContourToggle contours;
        public OrbitCameraController orbit;
        public SoldierViewPlayer player;
        public ViewpointSet viewpoints;     // null when viewpoints.json absent

        public const float FadeSeconds = 0.22f;
        // a surfaced warning (refused entry, missing media) outlives the
        // frame that produced it
        const float StatusHoldSeconds = 6f;

        static AtlasHud instance;

        VisualElement root;
        Label contextTitle, contextPhase, contextConditions, clockLabel, markerTip;
        Label drawerTitle, drawerIdentity, drawerStrength, drawerActivity, drawerConfidence;
        Button playButton, chipContours, chipReadingLight, chipCredits, chipOptions;
        Button drawerClose, drawerFollow;
        VisualElement atlasBar, svBar, unitDrawer, entryMarkers, speedGroup;
        VisualElement timelineMarkers, windowBand, modalLayer, warningModal, creditsModal;
        VisualElement optionsModal;
        // day navigation (ADR 0005): tabs from the battle manifest + the
        // honest empty-state panel for unreconstructed days/phases
        VisualElement dayTabs, dayModal;
        Label dayTitle;
        ScrollView dayBody;
        readonly List<Button> dayButtons = new List<Button>();
        VisualElement fadeOverlay, masthead, chips;
        ScrollView drawerSources, creditsList;
        Slider timelineSlider, svSlider, optMaster, optSvVolume;
        Button svPlay, svExit, svSources, svCaptionsToggle;
        Button optCaptions, optReducedMotion;
        Label svClock, svTitle, svProxyBadge, svSettling, svObserverLine;
        Label svSpeed, svCaptions, svMotionNote, optMotionNote;
        readonly List<Button> speedButtons = new List<Button>();

        MomentSet moments;
        PhaseManifest phaseManifest;
        int activeDayIndex = -1;
        bool activeDayBound;
        bool clockEchoChecked;
        string clockMismatch; // non-null = the manifest lied about the clock
        // in-HUD phase switching (ADR 0005's deferred hot-swap, this
        // slice): the in-flight guard, the measured switch time, the
        // remembered per-phase clock position (session-only — returning
        // to a phase resumes where you left it; a never-visited phase
        // starts at its t=0), and the loading veil
        bool switching;
        readonly System.Diagnostics.Stopwatch switchWatch =
            new System.Diagnostics.Stopwatch();
        public float LastSwitchMs { get; private set; }
        readonly Dictionary<string, float> phaseClockMemory =
            new Dictionary<string, float>();
        Label loadingLabel;
        // the battle asset the wired viewpoints address (the SERIALIZED
        // scene battle — the shipped media contract); Soldier View entry
        // exists only while that phase is loaded (HudModel.ViewpointsApplyTo)
        string viewpointsHomeAsset;
        bool loggedViewpointsGated;
        ContentWarningDoc warningDoc;
        ContentWarningGate gate;
        // per-viewpoint acknowledgement gates for warning overrides
        // (created lazily; see GateFor)
        Dictionary<string, ContentWarningGate> vpGates;
        CreditsManifest credits;
        bool creditsBuilt;
        // Phase 12 accessibility (plan §12 P12): persisted options and the
        // deterministic caption track for the Soldier View voice layers
        AccessibilityOptions options;
        CaptionTrack captionTrack;

        bool warningOpen;
        bool creditsOpen;
        bool optionsOpen;
        bool dayOpen;
        ViewpointDefinition pendingEntry;
        UnitTrack drawerTrack;          // unit currently in the drawer
        ViewpointDefinition drawerViewpoint; // non-null when opened from Soldier View
        bool following;
        bool drawerOpen;
        int lastDrawerSecond = int.MinValue;
        int lastEndTime = int.MinValue;
        float statusUntil;
        float seekStartedAt;
        bool wasSeeking;
        bool wasInSoldierView;
        bool transitioning;
        // guards the slider callbacks against PROGRAMMATIC range/value
        // writes: setting lowValue/highValue clamps a stale value and fires
        // the change event — without this, entering Soldier View at 15:20
        // seeked to the window start (observed in the P11 screenshot run)
        bool suppressSliderEvents;
        readonly List<string> visibleMarkerIds = new List<string>();

        // ------------------------------------------------------ static gate

        // True when world input (orbit camera, unit picking) must ignore
        // this pointer: it is over a HUD element, a HUD element holds
        // pointer capture (slider drag), or the HUD owns the screen
        // (Soldier View, a modal, the entry fade). screenPos uses
        // Input.mousePosition conventions (origin bottom-left).
        public static bool PointerBusy(Vector2 screenPos)
        {
            if (instance == null) return false;
            return instance.PointerBusyInstance(screenPos);
        }

        // World input lock without a pointer position (keyboard/scroll).
        public static bool InputLocked =>
            instance != null && instance.InputLockedInstance;

        bool InputLockedInstance =>
            (player != null && player.InSoldierView) || transitioning
            || warningOpen || creditsOpen || optionsOpen || dayOpen
            || switching;

        bool PointerBusyInstance(Vector2 screenPos)
        {
            if (InputLockedInstance) return true;
            IPanel panel = root?.panel;
            if (panel == null) return false;
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null)
                return true;
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
                panel, new Vector2(screenPos.x, Screen.height - screenPos.y));
            return panel.Pick(panelPos) != null;
        }

        // -------------------------------------------------------- lifecycle

        void OnEnable() { instance = this; }

        void OnDisable() { if (instance == this) instance = null; }

        void Start()
        {
            // tolerate lost references the way the scene components do
            if (director == null) director = FindFirstObjectByType<BattleDirector>();
            if (sun == null) sun = FindFirstObjectByType<SunDirector>();
            if (contours == null) contours = FindFirstObjectByType<ReliefContourToggle>();
            if (orbit == null) orbit = FindFirstObjectByType<OrbitCameraController>();
            WireOrbitTerrainAwareness();

            var document = GetComponent<UIDocument>();
            if (document == null || document.rootVisualElement == null)
            {
                Debug.LogWarning("AtlasHud: no UIDocument — HUD disabled");
                enabled = false;
                return;
            }
            root = document.rootVisualElement;
            Query();
            LoadDocuments();
            BuildStaticUi();
            // Soldier View across phases: the shipped viewpoints address
            // the SERIALIZED scene battle's clock and cast; a -battleFile
            // launch or an in-HUD switch to another phase hides entry
            // (HudModel.ViewpointsApplyTo). Empty for fixture rigs.
            if (director != null)
                viewpointsHomeAsset = director.SerializedAssetName;
            LogViewpointGateIfChanged();
        }

        // RTS camera slice: the orbit rig's pivot-rides-terrain/bounds-
        // clamp/zoom-anchor features need the SAME height sampler and
        // battlefield extent the rest of the Atlas uses — wired here
        // (HUD Start, and again on every phase switch, since a switch
        // spawns a fresh BattleDirector/terrain) from the loaded battle,
        // exactly where the camera is bootstrapped. Both orbit and
        // director tolerate being null (fixture rigs, EditMode-adjacent
        // PlayMode tests) — the camera then simply has no terrain
        // awareness, matching pre-slice behavior.
        void WireOrbitTerrainAwareness()
        {
            if (orbit == null || director == null || director.terrain == null
                || director.terrain.terrainData == null)
                return;
            orbit.heightSampler = director.GroundHeightAt;
            Vector3 origin = director.terrain.transform.position;
            Vector3 size = director.terrain.terrainData.size;
            orbit.boundsMinXZ = new Vector2(origin.x, origin.z);
            orbit.boundsMaxXZ = new Vector2(origin.x + size.x, origin.z + size.z);
        }

        // True while the wired viewpoints address the loaded phase — the
        // gate on entry markers, the timeline window band, and entry.
        bool ViewpointsApply => director == null
            || HudModel.ViewpointsApplyTo(
                viewpointsHomeAsset, director.BattleAssetName);

        // Per-viewpoint phase gate (Iverson production slice): a viewpoint
        // may declare its own home asset (vp.battleAsset — the July 1 film
        // rides the July 1 afternoon clock); one without keeps the set's.
        bool ViewpointApplies(ViewpointDefinition vp) => director == null
            || HudModel.ViewpointsApplyTo(
                HudModel.ViewpointHomeAsset(vp, viewpointsHomeAsset),
                director.BattleAssetName);

        void LogViewpointGateIfChanged()
        {
            bool gated = viewpoints != null && !ViewpointsApply;
            if (gated == loggedViewpointsGated) return;
            loggedViewpointsGated = gated;
            if (gated)
                Debug.Log($"AtlasHud: Soldier View entry hidden — the shipped "
                    + $"viewpoints address '{viewpointsHomeAsset}', but "
                    + $"'{director.BattleAssetName}' is loaded (per-phase media honesty).");
        }

        void Query()
        {
            masthead = root.Q("masthead");
            dayTabs = root.Q("day-tabs");
            dayModal = root.Q("day-modal");
            dayTitle = root.Q<Label>("day-title");
            dayBody = root.Q<ScrollView>("day-body");
            contextTitle = root.Q<Label>("context-title");
            contextPhase = root.Q<Label>("context-phase");
            contextConditions = root.Q<Label>("context-conditions");
            chips = root.Q("chips");
            chipContours = root.Q<Button>("chip-contours");
            chipReadingLight = root.Q<Button>("chip-reading-light");
            chipOptions = root.Q<Button>("chip-options");
            chipCredits = root.Q<Button>("chip-credits");
            entryMarkers = root.Q("entry-markers");
            unitDrawer = root.Q("unit-drawer");
            drawerTitle = root.Q<Label>("drawer-title");
            drawerClose = root.Q<Button>("drawer-close");
            drawerIdentity = root.Q<Label>("drawer-identity");
            drawerStrength = root.Q<Label>("drawer-strength");
            drawerActivity = root.Q<Label>("drawer-activity");
            drawerConfidence = root.Q<Label>("drawer-confidence");
            drawerFollow = root.Q<Button>("drawer-follow");
            drawerSources = root.Q<ScrollView>("drawer-sources");
            atlasBar = root.Q("atlas-bar");
            playButton = root.Q<Button>("play-button");
            speedGroup = root.Q("speed-group");
            clockLabel = root.Q<Label>("clock-label");
            markerTip = root.Q<Label>("marker-tip");
            timelineSlider = root.Q<Slider>("timeline-slider");
            timelineMarkers = root.Q("timeline-markers");
            windowBand = root.Q("timeline-window-band");
            svBar = root.Q("sv-bar");
            svPlay = root.Q<Button>("sv-play");
            svSpeed = root.Q<Label>("sv-speed");
            svClock = root.Q<Label>("sv-clock");
            svTitle = root.Q<Label>("sv-title");
            svProxyBadge = root.Q<Label>("sv-proxy-badge");
            svSettling = root.Q<Label>("sv-settling");
            svCaptionsToggle = root.Q<Button>("sv-captions-toggle");
            svSources = root.Q<Button>("sv-sources");
            svExit = root.Q<Button>("sv-exit");
            svSlider = root.Q<Slider>("sv-slider");
            svObserverLine = root.Q<Label>("sv-observer-line");
            svMotionNote = root.Q<Label>("sv-motion-note");
            svCaptions = root.Q<Label>("sv-captions");
            modalLayer = root.Q("modal-layer");
            warningModal = root.Q("warning-modal");
            creditsModal = root.Q("credits-modal");
            creditsList = root.Q<ScrollView>("credits-list");
            optionsModal = root.Q("options-modal");
            optMaster = root.Q<Slider>("opt-master");
            optSvVolume = root.Q<Slider>("opt-sv-volume");
            optCaptions = root.Q<Button>("opt-captions");
            optReducedMotion = root.Q<Button>("opt-reduced-motion");
            optMotionNote = root.Q<Label>("opt-motion-note");
            fadeOverlay = root.Q("fade-overlay");
        }

        void LoadDocuments()
        {
            // per-phase moments: see LoadMoments (re-run on every phase switch)
            moments = LoadMoments(
                director != null ? director.BattleAssetName : null);
            // day/phase navigation (ADR 0005): missing/rejected manifest =
            // no day tabs, warned, everything else keeps working (the
            // moments.json degradation pattern)
            phaseManifest = LoadStreamingJson(
                "Atlas/battle-manifest.json", PhaseManifest.FromJson);
            warningDoc = LoadStreamingJson(
                "SoldierView/content-warning.json", ContentWarningDoc.FromJson);
            if (warningDoc != null)
                gate = new ContentWarningGate(new PlayerPrefsStore(), warningDoc.version);
            credits = LoadStreamingJson("credits.json", CreditsManifest.FromJson);
            options = new AccessibilityOptions(new PlayerPrefsStore());
            captionTrack = LoadStreamingJson(
                "SoldierView/captions.json", CaptionTrack.FromJson);
        }

        // The per-phase moments lookup (ADR 0005, day-expansion slice 2),
        // shared by the initial load and every phase switch: a phase-named
        // file (Atlas/moments-<battleAsset>.json) wins; the default
        // moments.json carries its own `battle` gate — a moments file may
        // never render against another phase's clock.
        MomentSet LoadMoments(string battleAsset)
        {
            MomentSet set = null;
            if (!string.IsNullOrEmpty(battleAsset))
                set = LoadStreamingJsonQuiet(
                    $"Atlas/moments-{battleAsset}.json", MomentSet.FromJson);
            if (set == null)
                set = LoadStreamingJson("Atlas/moments.json", MomentSet.FromJson);
            if (set != null && !set.AppliesTo(battleAsset))
            {
                Debug.Log($"AtlasHud: moments file addresses battle "
                    + $"'{set.battle}' but '{battleAsset}' is loaded — "
                    + "timeline moments omitted (per-phase moments).");
                set = null;
            }
            return set;
        }

        // As LoadStreamingJson, but silent when the file is absent — the
        // per-phase moments probe (absence is the normal case for phases
        // without their own moments file, not a degradation).
        static T LoadStreamingJsonQuiet<T>(string relative, System.Func<string, T> parse)
            where T : class
        {
            string path = Path.Combine(Application.streamingAssetsPath, relative);
            if (!File.Exists(path)) return null;
            try { return parse(File.ReadAllText(path)); }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AtlasHud: {relative} rejected — {e.Message}");
                return null;
            }
        }

        static T LoadStreamingJson<T>(string relative, System.Func<string, T> parse)
            where T : class
        {
            string path = Path.Combine(Application.streamingAssetsPath, relative);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"AtlasHud: {relative} missing");
                return null;
            }
            try
            {
                return parse(File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AtlasHud: {relative} rejected — {e.Message}");
                return null;
            }
        }

        void BuildStaticUi()
        {
            playButton.clicked += TogglePlay;
            foreach (float speed in HudModel.Speeds)
            {
                float s = speed;
                var b = new Button(() => SetSpeed(s)) { text = $"{s:0}×" };
                b.AddToClassList("speed-btn");
                speedGroup.Add(b);
                speedButtons.Add(b);
            }
            timelineSlider.RegisterValueChangedCallback(e =>
            {
                if (suppressSliderEvents) return;
                // user scrub (programmatic writes use SetValueWithoutNotify):
                // grabbing the timeline pauses playback, as it always has
                clock.CurrentTime = e.newValue;
                clock.Playing = false;
            });
            chipContours.clicked += () =>
                { if (contours != null) contours.SetContours(!contours.Contours); };
            chipReadingLight.clicked += () =>
                { if (sun != null) sun.ReadingLight = !sun.ReadingLight; };
            chipCredits.clicked += OpenCredits;
            root.Q<Button>("credits-close").clicked += CloseCredits;
            BuildOptionsUi();
            drawerClose.clicked += CloseDrawer;
            drawerFollow.clicked += ToggleFollow;
            root.Q<Button>("warning-acknowledge").clicked += AcknowledgeWarning;
            root.Q<Button>("warning-decline").clicked += DeclineWarning;
            svPlay.clicked += () => player.SetPlaying(!clock.Playing);
            svExit.clicked += RequestExit;
            svSources.clicked += OpenDrawerForActiveViewpoint;
            svSlider.RegisterValueChangedCallback(e =>
            {
                if (suppressSliderEvents) return;
                if (player.InSoldierView && !player.SeekInProgress
                    && Mathf.Abs(e.newValue - clock.CurrentTime)
                        * player.Active.media.fps > 1f)
                    player.Seek(e.newValue);
            });

            if (director != null)
            {
                contextTitle.text = HudModel.DayContext(director.BattleName);
                contextConditions.text =
                    "conditions: " + HudModel.ConditionsLine(director.Environment);
            }
            else
            {
                contextTitle.text = "";
                contextConditions.text = "";
            }
            // default warning text; RequestEnter re-binds per viewpoint
            // before showing the modal (per-viewpoint warning overrides)
            BindWarningTexts(null);
            BuildMomentMarkers();
            BuildDayTabs();
            root.Q<Button>("day-close").clicked += CloseDayPanel;

            // the phase-switch loading veil: rides ABOVE the fade overlay
            // (added last), created in code so the committed UXML stays
            // untouched by this slice
            loadingLabel = new Label();
            loadingLabel.style.position = Position.Absolute;
            loadingLabel.style.left = 0f;
            loadingLabel.style.right = 0f;
            loadingLabel.style.top = Length.Percent(46f);
            loadingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            loadingLabel.style.fontSize = 22f;
            loadingLabel.style.color = new Color(0.92f, 0.9f, 0.85f);
            loadingLabel.style.display = DisplayStyle.None;
            loadingLabel.pickingMode = PickingMode.Ignore;
            root.Add(loadingLabel);
        }

        // ------------------------------------------- day navigation (ADR 0005)

        void BuildDayTabs()
        {
            dayTabs.Clear();
            dayButtons.Clear();
            if (phaseManifest == null) return; // warned at load; no tabs
            for (int i = 0; i < phaseManifest.days.Length; i++)
            {
                DayDto day = phaseManifest.days[i];
                int captured = i;
                var b = new Button(() => OpenDayPanel(captured)) { text = day.label };
                b.AddToClassList("day-btn");
                // a day with no reconstructed phase reads muted — clickable
                // (it opens the honest empty state), never disabled-and-mute
                bool anyReconstructed = false;
                foreach (PhaseDto p in day.phases)
                    if (p.Reconstructed) anyReconstructed = true;
                if (!anyReconstructed) b.AddToClassList("day-empty");
                dayTabs.Add(b);
                dayButtons.Add(b);
            }
        }

        // The active-day highlight binds lazily (the director's battle asset
        // is known immediately, but a missing director must not break tabs),
        // and the manifest's clock echo is checked ONCE against the loaded
        // battle's clock — the manifest may never lie about time
        // (battle-manifest.md "The honesty rules").
        void BindActiveDay()
        {
            if (phaseManifest == null) return;
            if (!activeDayBound && director != null)
            {
                activeDayBound = true;
                activeDayIndex = phaseManifest.ActiveDayIndex(director.BattleAssetName);
                for (int i = 0; i < dayButtons.Count; i++)
                    dayButtons[i].EnableInClassList("day-on", i == activeDayIndex);
            }
            if (!clockEchoChecked && director != null
                && !string.IsNullOrEmpty(director.BattleName))
            {
                clockEchoChecked = true;
                clockMismatch = phaseManifest.ClockMismatch(
                    director.BattleAssetName, clock.StartTime, clock.EndTime);
                if (clockMismatch != null)
                    Debug.LogWarning($"AtlasHud: {clockMismatch}");
            }
        }

        public bool DayPanelVisible => dayOpen;

        public void OpenDayPanel(int dayIndex)
        {
            if (phaseManifest == null || dayIndex < 0
                || dayIndex >= phaseManifest.days.Length) return;
            DayDto day = phaseManifest.days[dayIndex];
            dayTitle.text = $"{day.label} — {day.date}";
            dayBody.Clear();
            foreach (PhaseDto phase in day.phases)
            {
                bool active = director != null
                    && phase.MatchesBattleAsset(director.BattleAssetName);
                string status = phase.Reconstructed
                    ? (active ? "reconstructed — the loaded phase" : "reconstructed")
                    : "not yet reconstructed";
                var heading = new Label($"{phase.label}  ·  {status}");
                heading.AddToClassList("phase-heading");
                if (active) heading.AddToClassList("phase-status-active");
                dayBody.Add(heading);
                string body = phase.Reconstructed
                    ? HudModel.PhaseClockRange(phase.startTime, phase.endTime)
                      + (active && clockMismatch != null ? "\n" + clockMismatch : "")
                    : phase.note; // the manifest's honest words, verbatim
                var note = new Label(body);
                note.AddToClassList("phase-note");
                dayBody.Add(note);
                // in-HUD phase switching: a reconstructed phase that is
                // not the loaded one is one click away; a not-reconstructed
                // phase stays an honest note, never a control
                if (phase.Reconstructed && !active)
                {
                    PhaseDto captured = phase;
                    var load = new Button(() => SwitchToPhase(captured))
                    {
                        text = "Load this phase",
                    };
                    load.AddToClassList("phase-load-btn");
                    dayBody.Add(load);
                }
            }
            dayOpen = true;
            modalLayer.style.display = DisplayStyle.Flex;
            dayModal.style.display = DisplayStyle.Flex;
        }

        public void CloseDayPanel()
        {
            dayOpen = false;
            dayModal.style.display = DisplayStyle.None;
            if (!warningOpen && !creditsOpen && !optionsOpen)
                modalLayer.style.display = DisplayStyle.None;
        }

        // ------------------------------ in-HUD phase switching (this slice)

        // A switch is in flight (the PlayMode suite and the capture
        // harness await settled state).
        public bool Switching => switching;

        // Loads a reconstructed phase in-session: tears the current battle
        // down (the successor pattern — see BattleDirector.SpawnSuccessor)
        // and brings the target phase up with the HUD refreshed. Public
        // for the day panel's buttons, the capture harness, and the
        // PlayMode suite. Failures are loud and leave the current phase
        // running untouched.
        public void SwitchToPhase(PhaseDto phase)
        {
            if (switching || transitioning || phase == null
                || !phase.Reconstructed || director == null || clock == null)
                return;
            if (player != null && player.InSoldierView)
                return; // exit the film first — the Atlas owns switching
            string assetName = Path.GetFileNameWithoutExtension(phase.battle);
            if (assetName == director.BattleAssetName)
            {
                CloseDayPanel();
                return; // already the loaded phase
            }
            string path = ResolveBattleFile(phase.battle);
            if (path == null)
            {
                string msg = $"phase '{phase.id}' unavailable: battle file "
                    + $"'{phase.battle}' not found (searched -battleDir, "
                    + "StreamingAssets/Battle, Assets/Battle, the -battleFile "
                    + "directory)";
                Debug.LogWarning($"AtlasHud: {msg}");
                ShowStatus(msg, StatusHoldSeconds);
                return;
            }
            string battleText;
            try
            {
                battleText = File.ReadAllText(path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AtlasHud: phase '{phase.id}' battle file "
                    + $"unreadable at {path} — {e.Message}");
                ShowStatus($"phase '{phase.id}' battle file unreadable: {e.Message}",
                    StatusHoldSeconds);
                return;
            }
            StartCoroutine(SwitchRoutine(phase, assetName, battleText));
        }

        string ResolveBattleFile(string fileName)
        {
            foreach (string candidate in HudModel.BattleFileCandidates(
                fileName, ArgValue("-battleDir"), Application.streamingAssetsPath,
                Application.dataPath, BattleDirector.BattleFileOverridePath()))
                if (File.Exists(candidate))
                    return candidate;
            return null;
        }

        static string ArgValue(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, name);
            return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : null;
        }

        IEnumerator SwitchRoutine(PhaseDto phase, string assetName, string battleText)
        {
            switching = true;
            switchWatch.Restart(); // measured from the accepted click
            CloseDayPanel();
            CloseDrawer(); // the drawer's track belongs to the old battle
            clock.Playing = false;
            // remember where the user left the outgoing phase — returning
            // resumes there; a never-visited phase starts at its t=0
            string fromAsset = director.BattleAssetName;
            if (!string.IsNullOrEmpty(fromAsset))
                phaseClockMemory[fromAsset] = clock.CurrentTime;

            // the loading veil, rendered before the blocking load frame
            loadingLabel.text = $"Loading {phase.label} …";
            loadingLabel.style.display = DisplayStyle.Flex;
            fadeOverlay.style.display = DisplayStyle.Flex;
            fadeOverlay.style.opacity = 1f;
            yield return null;
            yield return null;

            // validate the file BEFORE tearing anything down: a rejected
            // battle file must leave the current phase running
            try
            {
                BattleLoader.Parse(battleText);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AtlasHud: phase '{phase.id}' battle file "
                    + $"rejected — {e.Message}; staying on '{fromAsset}'");
                ShowStatus($"phase '{phase.id}' rejected: {e.Message}",
                    StatusHoldSeconds);
                loadingLabel.style.display = DisplayStyle.None;
                fadeOverlay.style.opacity = 0f;
                fadeOverlay.style.display = DisplayStyle.None;
                switching = false;
                yield break;
            }

            // teardown + successor: destroy the old battle's components
            // (each releases its own runtime objects in OnDestroy) and
            // spawn a FRESH director with the same wiring — no instance
            // state can survive, which is the no-leak guarantee the
            // PlayMode fresh-launch-equivalence test pins
            BattleDirector old = director;
            GameObject host = old.gameObject;
            Destroy(host.GetComponent<ObscurationField>());
            Destroy(host.GetComponent<AcousticField>());
            Destroy(host.GetComponent<UnitLabelField>());
            director = BattleDirector.SpawnSuccessor(old, battleText, assetName);
            Destroy(old);
            yield return null; // destroys complete; successor Start has run

            // clock: the successor's Start set StartTime/EndTime from the
            // new battle; position is the remembered spot or the phase start
            clock.CurrentTime = phaseClockMemory.TryGetValue(assetName, out float saved)
                ? Mathf.Clamp(saved, 0f, clock.EndTime)
                : 0f;
            clock.Playing = false;

            RefreshAfterSwitch(assetName);

            switchWatch.Stop();
            LastSwitchMs = (float)switchWatch.Elapsed.TotalMilliseconds;
            Debug.Log($"AtlasHud: phase switch to '{phase.id}' "
                + $"('{assetName}') completed in {LastSwitchMs:F0} ms");

            loadingLabel.style.display = DisplayStyle.None;
            yield return Fade(1f, 0f);
            ShowStatus($"{phase.label} — loaded in {LastSwitchMs / 1000f:0.0} s"
                + (clockMismatch != null ? $"  ⚠ {clockMismatch}" : ""),
                StatusHoldSeconds);
            switching = false;
        }

        // Everything in the HUD that is a function of WHICH phase is
        // loaded, re-derived after a switch: per-phase moments and their
        // markers, the timeline range, the masthead words, the day-tab
        // highlight, the manifest's clock echo (re-verified on EVERY
        // switch — the manifest may never lie about time), and the
        // Soldier View gate.
        void RefreshAfterSwitch(string battleAsset)
        {
            WireOrbitTerrainAwareness(); // the switch spawned a fresh director/terrain
            moments = LoadMoments(battleAsset);
            BuildMomentMarkers();
            lastEndTime = int.MinValue; // LayoutTimeline re-lays-out next frame
            contextPhase.text = moments != null
                ? moments.PhaseAt(clock.CurrentTime) : "";
            contextTitle.text = HudModel.DayContext(director.BattleName);
            contextConditions.text =
                "conditions: " + HudModel.ConditionsLine(director.Environment);
            // day-tab highlight + clock echo re-run against the new battle
            activeDayBound = false;
            clockEchoChecked = false;
            clockMismatch = null;
            BindActiveDay();
            if (clockMismatch != null)
                ShowStatus(clockMismatch, StatusHoldSeconds);
            // Soldier View entry: stale markers clear now; the per-frame
            // gate (ViewpointsApply) owns them from here
            visibleMarkerIds.Clear();
            entryMarkers.Clear();
            LogViewpointGateIfChanged();
        }

        void BuildMomentMarkers()
        {
            timelineMarkers.Clear();
            if (moments == null) return;
            foreach (MomentDto m in moments.moments)
            {
                var marker = new VisualElement();
                marker.AddToClassList("marker");
                var line = new VisualElement();
                line.AddToClassList("marker-line");
                line.pickingMode = PickingMode.Ignore;
                marker.Add(line);
                MomentDto captured = m;
                marker.RegisterCallback<ClickEvent>(_ =>
                {
                    clock.CurrentTime = captured.t;
                    clock.Playing = false;
                });
                marker.RegisterCallback<PointerEnterEvent>(_ => ShowStatus(
                    $"{ClockMath.FormatClockTime(clock.StartTime, captured.t)} · "
                    + $"{captured.label} — {captured.detail}  [{captured.citation}]",
                    2f));
                marker.RegisterCallback<PointerLeaveEvent>(_ =>
                    { if (Time.unscaledTime >= statusUntil) markerTip.text = ""; });
                timelineMarkers.Add(marker);
            }
        }

        // Marker/band horizontal layout rides EndTime, which BattleDirector
        // sets from the battle JSON in ITS Start — so it is re-checked per
        // frame and re-laid-out only when it actually changes.
        void LayoutTimeline()
        {
            int end = Mathf.RoundToInt(clock.EndTime);
            if (end == lastEndTime) return;
            lastEndTime = end;
            suppressSliderEvents = true;
            timelineSlider.highValue = clock.EndTime;
            timelineSlider.SetValueWithoutNotify(
                Mathf.Clamp(clock.CurrentTime, 0f, clock.EndTime));
            suppressSliderEvents = false;
            if (moments != null)
            {
                for (int i = 0; i < timelineMarkers.childCount; i++)
                {
                    float frac = HudModel.TimelineFraction(
                        moments.moments[i].t, clock.EndTime);
                    timelineMarkers[i].style.left = Length.Percent(frac * 100f);
                }
            }
            ViewpointDefinition hero = FirstProductViewpoint();
            if (hero != null)
            {
                (float left, float width) = HudModel.WindowBand(
                    hero.t0, hero.t1, clock.EndTime);
                windowBand.style.left = Length.Percent(left * 100f);
                windowBand.style.width = Length.Percent(width * 100f);
                windowBand.style.display = DisplayStyle.Flex;
            }
            else
            {
                windowBand.style.display = DisplayStyle.None;
            }
        }

        ViewpointDefinition FirstProductViewpoint()
        {
            if (viewpoints?.viewpoints == null) return null;
            foreach (ViewpointDefinition vp in viewpoints.viewpoints)
                if (!vp.development && ViewpointApplies(vp))
                    return vp;
            return null;
        }

        // --------------------------------------------------------- update

        void Update()
        {
            if (root == null) return;
            LayoutTimeline();
            bool inSv = player != null && player.InSoldierView;
            if (inSv != wasInSoldierView)
            {
                wasInSoldierView = inSv;
                SetAtlasChromeVisible(!inSv);
                svBar.style.display = inSv ? DisplayStyle.Flex : DisplayStyle.None;
                if (orbit != null) orbit.enabled = !inSv;
                // world-space unit labels would bleed into the video's
                // letterbox bands; the label layer hides with the chrome
                var labels = FindFirstObjectByType<UnitLabelField>();
                if (labels != null) labels.Hidden = inSv;
                if (!inSv && drawerViewpoint != null) CloseDrawer();
                if (!inSv) svCaptions.style.display = DisplayStyle.None;
            }

            if (inSv) UpdateSoldierViewBar();
            else UpdateAtlasBar();
            UpdateEntryMarkers(inSv);
            UpdateDrawer();
            if (Time.unscaledTime >= statusUntil && markerTip.text != ""
                && statusUntil > 0f)
            {
                markerTip.text = "";
                statusUntil = 0f;
            }
        }

        void SetAtlasChromeVisible(bool visible)
        {
            DisplayStyle d = visible ? DisplayStyle.Flex : DisplayStyle.None;
            atlasBar.style.display = d;
            masthead.style.display = d;
            chips.style.display = d;
            entryMarkers.style.display = d;
        }

        void UpdateAtlasBar()
        {
            BindActiveDay();
            // masthead binds lazily: BattleDirector.Start (which parses the
            // battle JSON) may run after this component's Start
            if (director != null && string.IsNullOrEmpty(contextTitle.text)
                && !string.IsNullOrEmpty(director.BattleName))
            {
                contextTitle.text = HudModel.DayContext(director.BattleName);
                contextConditions.text =
                    "conditions: " + HudModel.ConditionsLine(director.Environment);
            }
            playButton.text = clock.Playing ? "❚❚" : "►";
            for (int i = 0; i < speedButtons.Count; i++)
                speedButtons[i].EnableInClassList(
                    "speed-on", Mathf.Approximately(clock.Speed, HudModel.Speeds[i]));
            clockLabel.text = clock.StartTime > 0f
                ? ClockMath.FormatClockTime(clock.StartTime, clock.CurrentTime)
                : ClockMath.FormatTime(clock.CurrentTime);
            if (moments != null)
                contextPhase.text = moments.PhaseAt(clock.CurrentTime);
            if (!SliderCaptured(timelineSlider))
                timelineSlider.SetValueWithoutNotify(clock.CurrentTime);
            if (contours != null)
                chipContours.EnableInClassList("chip-on", contours.Contours);
            if (sun != null)
                chipReadingLight.EnableInClassList("chip-on", sun.ReadingLight);
        }

        void UpdateSoldierViewBar()
        {
            ViewpointDefinition vp = player.Active;
            svPlay.text = clock.Playing ? "❚❚" : "►";
            // the forced-1× designator (P11 punchlist): the player locked
            // the clock to 1× on entry; say so, and carry the paused state
            svSpeed.text = HudModel.SoldierViewSpeedLabel(clock.Playing);
            UpdateCaptions();
            svClock.text = ClockMath.FormatClockTime(clock.StartTime, clock.CurrentTime)
                + $"  (t={clock.CurrentTime:0})";
            svTitle.text = vp.title;
            svProxyBadge.style.display = player.UsingProxyFallback
                ? DisplayStyle.Flex : DisplayStyle.None;
            if (player.SeekInProgress && !wasSeeking)
                seekStartedAt = Time.unscaledTime;
            wasSeeking = player.SeekInProgress;
            svSettling.style.display = HudModel.ShowSettleIndicator(
                player.SeekInProgress, Time.unscaledTime - seekStartedAt)
                ? DisplayStyle.Flex : DisplayStyle.None;
            suppressSliderEvents = true;
            svSlider.lowValue = (float)vp.t0;
            svSlider.highValue = (float)player.LastSeekableBattleTime;
            suppressSliderEvents = false;
            if (!SliderCaptured(svSlider))
                svSlider.SetValueWithoutNotify(clock.CurrentTime);
            if (warningDoc != null)
                svObserverLine.text =
                    warningDoc.ObserverFor(vp?.id)?.shortLine ?? "";
        }

        bool SliderCaptured(Slider slider)
        {
            IPanel panel = root.panel;
            if (panel == null) return false;
            var capturing = panel.GetCapturingElement(PointerId.mousePointerId)
                as VisualElement;
            while (capturing != null)
            {
                if (capturing == slider) return true;
                capturing = capturing.parent;
            }
            return false;
        }

        void UpdateEntryMarkers(bool inSv)
        {
            if (inSv || viewpoints?.viewpoints == null) return;
            // per-viewpoint phase honesty: a viewpoint's marker exists only
            // while ITS phase is the loaded one (the set's home asset, or
            // the viewpoint's own battleAsset for cross-phase films)
            // rebuild only when the visible set changes
            bool changed = false;
            int visible = 0;
            foreach (ViewpointDefinition vp in viewpoints.viewpoints)
            {
                if (!ViewpointApplies(vp)) continue;
                if (!HudModel.EntryMarkerVisible(vp, clock.CurrentTime)) continue;
                if (visible >= visibleMarkerIds.Count
                    || visibleMarkerIds[visible] != vp.id) changed = true;
                visible++;
            }
            if (visible != visibleMarkerIds.Count) changed = true;
            if (!changed) return;
            visibleMarkerIds.Clear();
            entryMarkers.Clear();
            foreach (ViewpointDefinition vp in viewpoints.viewpoints)
            {
                if (!ViewpointApplies(vp)) continue;
                if (!HudModel.EntryMarkerVisible(vp, clock.CurrentTime)) continue;
                visibleMarkerIds.Add(vp.id);
                ViewpointDefinition captured = vp;
                var b = new Button(() => RequestEnter(captured))
                {
                    text = $"Enter Soldier View — {vp.title}",
                };
                b.AddToClassList("entry-btn");
                entryMarkers.Add(b);
            }
        }

        // ------------------------------------------------------ transport

        void TogglePlay()
        {
            if (!clock.Playing && clock.CurrentTime >= clock.EndTime)
                clock.CurrentTime = 0f; // replay from the start
            clock.Playing = !clock.Playing;
        }

        void SetSpeed(float speed) => clock.Speed = speed;

        void ShowStatus(string text, float holdSeconds)
        {
            markerTip.text = text;
            statusUntil = Time.unscaledTime + holdSeconds;
        }

        // ----------------------------------------------- soldier view flow

        // Entry marker action: first entry (per authored warning version)
        // must pass the graphic-content warning; afterwards entry is an
        // authored cut behind a short fade. Public for the PlayMode suite.
        public void RequestEnter(ViewpointDefinition vp)
        {
            if (player == null || player.InSoldierView || transitioning
                || switching) return;
            if (!ViewpointApplies(vp))
            {
                // unreachable through the UI (markers are gated) but public
                // callers get the honest refusal, not a wrong-clock film
                ShowStatus("Soldier View unavailable: the shipped media "
                    + "addresses '"
                    + HudModel.ViewpointHomeAsset(vp, viewpointsHomeAsset)
                    + "', not the loaded phase",
                    StatusHoldSeconds);
                return;
            }
            if (warningDoc == null || gate == null)
            {
                // the warning is a locked requirement (§9.2) — no text, no entry
                ShowStatus("Soldier View unavailable: content warning text missing "
                    + "(StreamingAssets/SoldierView/content-warning.json)",
                    StatusHoldSeconds);
                return;
            }
            if (GateFor(vp).NeedsAcknowledgement)
            {
                pendingEntry = vp;
                BindWarningTexts(vp);
                warningOpen = true;
                modalLayer.style.display = DisplayStyle.Flex;
                warningModal.style.display = DisplayStyle.Flex;
                return;
            }
            StartCoroutine(EnterRoutine(vp));
        }

        // The acknowledgement gate for a viewpoint: a per-viewpoint warning
        // override acknowledges under its own key + version (each film's
        // warning surfaces before ITS first entry); viewpoints without an
        // override share the default gate, exactly as before.
        ContentWarningGate GateFor(ViewpointDefinition vp)
        {
            var ov = warningDoc?.OverrideFor(vp?.id);
            if (ov == null) return gate;
            vpGates ??= new Dictionary<string, ContentWarningGate>();
            if (!vpGates.TryGetValue(ov.viewpointId, out var g))
            {
                g = new ContentWarningGate(new PlayerPrefsStore(), ov.version,
                    ContentWarningGate.KeyForViewpoint(ov.viewpointId));
                vpGates[ov.viewpointId] = g;
            }
            return g;
        }

        // Warning-modal text for a viewpoint (null = the default authored
        // warning). Per-viewpoint films carry their own warning + observer
        // note (iverson-viewpoint-design.md §7); wording is never composed
        // in code.
        void BindWarningTexts(ViewpointDefinition vp)
        {
            if (warningDoc == null) return;
            var w = warningDoc.WarningFor(vp?.id);
            var o = warningDoc.ObserverFor(vp?.id);
            root.Q<Label>("warning-title").text = w.title;
            root.Q<Label>("warning-body").text = w.body;
            root.Q<Label>("observer-title").text = o?.title ?? "";
            root.Q<Label>("observer-body").text = o?.body ?? "";
            root.Q<Button>("warning-acknowledge").text = w.acknowledgeLabel;
            root.Q<Button>("warning-decline").text = w.declineLabel;
        }

        public void AcknowledgeWarning()
        {
            if (pendingEntry == null) { HideWarning(); return; }
            GateFor(pendingEntry).Acknowledge();
            ViewpointDefinition vp = pendingEntry;
            pendingEntry = null;
            HideWarning();
            StartCoroutine(EnterRoutine(vp));
        }

        public void DeclineWarning()
        {
            pendingEntry = null;
            HideWarning();
        }

        void HideWarning()
        {
            warningOpen = false;
            warningModal.style.display = DisplayStyle.None;
            if (!creditsOpen && !optionsOpen && !dayOpen)
                modalLayer.style.display = DisplayStyle.None;
        }

        public bool WarningVisible => warningOpen;

        // Entry/exit fade in flight (the PlayMode suite awaits settled state).
        public bool Transitioning => transitioning;

        public void RequestExit()
        {
            if (player == null || !player.InSoldierView || transitioning) return;
            StartCoroutine(ExitRoutine());
        }

        IEnumerator EnterRoutine(ViewpointDefinition vp)
        {
            transitioning = true;
            // the authored cut lands on the second the user chose — a
            // clock running at 60x must not drift (possibly out of the
            // window) during the fade
            clock.Playing = false;
            yield return Fade(0f, 1f);
            if (!player.TryEnter(vp))
                ShowStatus($"Soldier View refused: {player.LastWarning}",
                    StatusHoldSeconds);
            // Update() flips the chrome on player.InSoldierView next frame
            yield return null;
            yield return Fade(1f, 0f);
            transitioning = false;
        }

        IEnumerator ExitRoutine()
        {
            transitioning = true;
            yield return Fade(0f, 1f);
            player.Exit();
            // clock.CurrentTime untouched by Exit; the orbit controller was
            // disabled the whole time — Atlas resumes at the exact battle
            // second and camera state
            yield return null;
            yield return Fade(1f, 0f);
            transitioning = false;
        }

        IEnumerator Fade(float from, float to)
        {
            fadeOverlay.style.display = DisplayStyle.Flex;
            float t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < FadeSeconds)
            {
                float u = (Time.unscaledTime - t0) / FadeSeconds;
                fadeOverlay.style.opacity = Mathf.Lerp(from, to, u);
                yield return null;
            }
            fadeOverlay.style.opacity = to;
            if (to <= 0f) fadeOverlay.style.display = DisplayStyle.None;
        }

        // ----------------------------------------------------- unit drawer

        void UpdateDrawer()
        {
            // Atlas mode: the drawer follows the director's click selection.
            // Soldier View mode: the drawer holds the viewpoint's unit when
            // opened via the Sources button (drawerViewpoint != null).
            if (drawerViewpoint == null)
            {
                UnitTrack selected = null;
                BattleDirector.SelectedUnitInfo info = default;
                if (director != null && director.TryGetSelectedInfo(out info))
                    selected = info.Track;
                if (selected != drawerTrack)
                {
                    drawerTrack = selected;
                    following = false;
                    RefreshFollowUi();
                    if (drawerTrack == null) { HideDrawer(); return; }
                    ShowDrawerStatic(info, null);
                    lastDrawerSecond = int.MinValue;
                }
                if (drawerTrack == null) return;
                if (following && orbit != null && director != null)
                    orbit.followPivot = FollowPivot;
            }
            if (drawerTrack == null) return;
            int second = Mathf.FloorToInt(clock.CurrentTime);
            if (second == lastDrawerSecond) return;
            lastDrawerSecond = second;
            RefreshDrawerDynamic();
        }

        Vector3? FollowPivot()
        {
            if (drawerTrack == null || director == null) return null;
            UnitState s = drawerTrack.StateAt(clock.CurrentTime);
            return new Vector3(s.posXZ.x,
                director.GroundHeightAt(s.posXZ.x, s.posXZ.y), s.posXZ.y);
        }

        void ShowDrawerStatic(BattleDirector.SelectedUnitInfo info,
            ViewpointDefinition fromViewpoint)
        {
            drawerTrack = info.Track;
            drawerViewpoint = fromViewpoint;
            drawerOpen = true;
            unitDrawer.style.display = DisplayStyle.Flex;
            drawerTitle.text = info.DisplayName;
            drawerIdentity.text = ProvenanceDrawer.SideWord(info.IsUnion)
                + " " + ProvenanceDrawer.ArmWord(info.Kind)
                + " " + ProvenanceDrawer.EchelonWord(info.Echelon);
            drawerFollow.style.display = fromViewpoint == null && orbit != null
                ? DisplayStyle.Flex : DisplayStyle.None;
            lastDrawerSecond = int.MinValue;
        }

        void RefreshDrawerDynamic()
        {
            float t = clock.CurrentTime;
            UnitState s = drawerTrack.StateAt(t);
            var info = default(BattleDirector.SelectedUnitInfo);
            IReadOnlyList<EventDto> events = null;
            if (director != null
                && director.TryGetUnitInfo(drawerTrack.Unit.id, out info))
                events = info.Events;
            drawerStrength.text = ProvenanceDrawer.StrengthLine(s.strength);
            drawerActivity.text = ProvenanceDrawer.ActivityLine(
                BattleDirector.IsMovingAt(drawerTrack, t), s.formation, events, t);
            drawerConfidence.text =
                "confidence: " + ProvenanceDrawer.ConfidenceWord(s.confidence);
            drawerSources.Clear();
            foreach (ProvenanceDrawer.SourceEntry entry in
                ProvenanceDrawer.SourceEntries(drawerTrack.Unit, t, clock.StartTime, events))
            {
                var heading = new Label(entry.Heading);
                heading.AddToClassList("source-heading");
                if (entry.Live) heading.AddToClassList("source-heading-live");
                drawerSources.Add(heading);
                var cite = new Label(entry.Citation);
                cite.AddToClassList("source-citation");
                if (entry.Citation == ProvenanceDrawer.NoReliableRecord)
                    cite.AddToClassList("source-silent");
                drawerSources.Add(cite);
            }
            if (drawerViewpoint != null)
            {
                var heading = new Label("viewpoint: " + drawerViewpoint.title);
                heading.AddToClassList("source-heading");
                drawerSources.Add(heading);
                var note = new Label(drawerViewpoint.editorialNote);
                note.AddToClassList("source-citation");
                drawerSources.Add(note);
                if (drawerViewpoint.claimIds != null
                    && drawerViewpoint.claimIds.Length > 0)
                {
                    var claims = new Label(
                        "claims: " + string.Join(", ", drawerViewpoint.claimIds));
                    claims.AddToClassList("source-citation");
                    drawerSources.Add(claims);
                }
            }
        }

        // The §13 "source drawer from a Soldier View" surface: the active
        // viewpoint's unit, plus the viewpoint's own claims and editorial
        // note. Public for the PlayMode suite.
        public void OpenDrawerForActiveViewpoint()
        {
            if (player == null || !player.InSoldierView || director == null) return;
            if (drawerOpen && drawerViewpoint != null)
            {
                CloseDrawer();
                return;
            }
            if (director.TryGetUnitInfo(player.Active.unitId,
                    out BattleDirector.SelectedUnitInfo info))
            {
                ShowDrawerStatic(info, player.Active);
                RefreshDrawerDynamic();
            }
        }

        public bool DrawerVisible => drawerOpen;

        public void CloseDrawer()
        {
            if (drawerViewpoint == null)
                director?.ClearSelection();
            drawerViewpoint = null;
            drawerTrack = null;
            following = false;
            if (orbit != null) orbit.followPivot = null;
            HideDrawer();
        }

        void HideDrawer()
        {
            drawerOpen = false;
            unitDrawer.style.display = DisplayStyle.None;
        }

        void ToggleFollow()
        {
            following = !following;
            if (orbit != null)
                orbit.followPivot = following ? (System.Func<Vector3?>)FollowPivot : null;
            RefreshFollowUi();
        }

        void RefreshFollowUi()
        {
            drawerFollow.EnableInClassList("chip-on", following);
            drawerFollow.text = following ? "Following (click to stop)" : "Follow unit";
        }

        // ---------------------------------------- options (Phase 12, §12 P12)

        // The reduced-motion honesty text: the setting is real and persisted,
        // but the SHIPPED media was rendered with the standard profile — a
        // reduced-motion cut is a separate offline render (render runbook).
        const string MotionNoteText =
            "Reduced motion minimizes camera bob, sway, and shake. The current "
            + "Soldier View release media was rendered with the standard motion "
            + "profile; this setting takes full effect with a reduced-motion "
            + "media render (see docs/reconstruction/render-runbook.md).";

        void BuildOptionsUi()
        {
            chipOptions.clicked += OpenOptions;
            root.Q<Button>("options-close").clicked += CloseOptions;
            optMotionNote.text = MotionNoteText;
            optMaster.SetValueWithoutNotify(options.MasterVolume01 * 100f);
            optSvVolume.SetValueWithoutNotify(options.SoldierViewVolume01 * 100f);
            optMaster.RegisterValueChangedCallback(e =>
            {
                options.MasterVolume01 = e.newValue / 100f;
                ApplyAudioLevels();
            });
            optSvVolume.RegisterValueChangedCallback(e =>
            {
                options.SoldierViewVolume01 = e.newValue / 100f;
                ApplyAudioLevels();
            });
            optCaptions.clicked += ToggleCaptions;
            svCaptionsToggle.clicked += ToggleCaptions;
            optReducedMotion.clicked += () =>
            {
                options.ReducedMotion = !options.ReducedMotion;
                RefreshOptionToggles();
            };
            RefreshOptionToggles();
            ApplyAudioLevels();
        }

        void ToggleCaptions()
        {
            options.CaptionsEnabled = !options.CaptionsEnabled;
            RefreshOptionToggles();
        }

        void RefreshOptionToggles()
        {
            optCaptions.text = options.CaptionsEnabled ? "On" : "Off";
            optCaptions.EnableInClassList("chip-on", options.CaptionsEnabled);
            svCaptionsToggle.EnableInClassList("chip-on", options.CaptionsEnabled);
            optReducedMotion.text = options.ReducedMotion ? "On" : "Off";
            optReducedMotion.EnableInClassList("chip-on", options.ReducedMotion);
            // honesty note in the Soldier View bar while reduced motion is on
            svMotionNote.text = options.ReducedMotion
                ? "Reduced motion is on — this media was rendered with the "
                  + "standard motion profile (reduced-motion render pending)."
                : "";
            svMotionNote.style.display = options.ReducedMotion
                ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Master rides the AudioListener (the Atlas synth soundscape);
        // the Soldier View media plays on the VideoPlayer's DIRECT path,
        // which bypasses the listener, so it gets master × mix explicitly.
        void ApplyAudioLevels()
        {
            AudioListener.volume = options.MasterVolume01;
            if (player != null)
                player.SetMixVolume(options.EffectiveSoldierViewVolume01);
        }

        void UpdateCaptions()
        {
            string text = options.CaptionsEnabled && captionTrack != null
                ? captionTrack.TextAt(clock.CurrentTime) : "";
            svCaptions.text = text;
            svCaptions.style.display = text.Length > 0
                ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void OpenOptions()
        {
            optionsOpen = true;
            modalLayer.style.display = DisplayStyle.Flex;
            optionsModal.style.display = DisplayStyle.Flex;
        }

        public void CloseOptions()
        {
            optionsOpen = false;
            optionsModal.style.display = DisplayStyle.None;
            if (!warningOpen && !creditsOpen && !dayOpen)
                modalLayer.style.display = DisplayStyle.None;
        }

        public bool OptionsVisible => optionsOpen;

        // -------------------------------------------------------- credits

        public void OpenCredits()
        {
            creditsOpen = true;
            if (!creditsBuilt)
            {
                creditsBuilt = true;
                creditsList.Clear();
                root.Q<Label>("credits-note").text =
                    "Generated from app/Assets/ThirdParty/manifest.json — the full "
                    + "inventory with checksums and license evidence is "
                    + "docs/assets/THIRD_PARTY_ASSETS.md.";
                if (credits == null)
                {
                    var missing = new Label(
                        "credits.json missing — regenerate with "
                        + "reconstruction/scripts/generate_attribution.py");
                    missing.AddToClassList("credit-entry");
                    creditsList.Add(missing);
                }
                else
                {
                    foreach (CreditEntry entry in credits.assets)
                    {
                        var label = new Label(entry.AttributionLine());
                        label.AddToClassList("credit-entry");
                        creditsList.Add(label);
                    }
                }
            }
            modalLayer.style.display = DisplayStyle.Flex;
            creditsModal.style.display = DisplayStyle.Flex;
        }

        public void CloseCredits()
        {
            creditsOpen = false;
            creditsModal.style.display = DisplayStyle.None;
            if (!warningOpen && !optionsOpen && !dayOpen)
                modalLayer.style.display = DisplayStyle.None;
        }

        public bool CreditsVisible => creditsOpen;
    }
}
