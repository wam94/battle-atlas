# Gate P11 — evidence and decisions (executor self-verification)

**Phase:** 11 — Atlas presentation and integration (plan §12).
**Branch:** `v2-phase11` (from `main` = 9a3810a).
**Gate:** a new user can locate Pickett's Charge, understand the approach,
enter the hero viewpoint during its valid window, seek and play it, inspect
why the reconstruction made its choices, and return — without developer
assistance. **The owner's session closes this gate**
(`docs/reconstruction/p11-owner-checklist.md`, entry point
`scripts/p11-demo.sh`); this document is the executor's evidence.

## What Phase 11 shipped

### Retained-mode UI (UI Toolkit) — the IMGUI placeholders are gone

`TimelineHud` (IMGUI strip) and `SoldierViewHud` (IMGUI dev panel) are
deleted; a one-shot editor surgery removed the TimelineHud component from
`Atlas.unity` (and wired the previously-unwired `symbolMaterial` —
`UnitSymbol.mat` — whose scene test had been skipping). The replacement:

- **Assets (committed source):** `app/Assets/Resources/UI/AtlasHud.uxml`
  (structure), `AtlasHud.uss` (style), `BattleAtlasTheme.tss` (default
  runtime theme), `AtlasPanelSettings.asset` (ConstantPhysicalSize @
  160 dpi — the retired `HudScale` intent, natively).
- **Controller:** `app/Assets/Scripts/UI/AtlasHud.cs`, created at scene
  load by `SoldierViewBootstrap` together with the `SoldierViewPlayer` and
  the `UIDocument` — scene files stay untouched by UI iteration, and
  `-benchmark` runs still show the pre-V2 Atlas.
- **Pure model layer (EditMode-pinned):** `HudModel` (speeds 1×/10×/60×
  per plan §10 — the 300× IMGUI convenience speed retired; timeline
  fractions; masthead context; conditions line; entry-marker windowing;
  window band; settle-indicator policy), `MomentSet`, `ProvenanceDrawer`,
  `ContentWarningGate`, `CreditsManifest`.
- **Input arbitration:** `AtlasHud.PointerBusy` replaces the IMGUI
  `hotControl`/HUD-strip math in `BattleDirector` (unit picking) and
  `OrbitCameraController` (orbit/zoom/pan) — panel picking + pointer
  capture + screen ownership (Soldier View, modals, the fade).

### Plan-§10 minimum Atlas UI, item by item

- play/pause + speeds 1×/10×/60× — transport row.
- wall-clock time — the participants' clock (13:00 = t0).
- day/slice context — masthead: battle title, current **phase** (the
  last moment marker at or before t), minimal **conditions** readout
  (wind bearing/strength + provenance word from the battle JSON).
- moment markers — `StreamingAssets/Atlas/moments.json`: seven curated
  anchors (bombardment 13:00, step-off 15:05, road 15:16, canister
  ~15:20, wall 15:23, Angle crisis 15:25, repulse ~15:27), every one
  citing the claim/editorial decision that places it; `MomentSet`
  REJECTS a marker without a citation. Markers are clickable (seek) and
  hoverable (time + detail + citation).
- unit labels with decluttering — unchanged world-space TMP layer from
  the cartography slice (it survives the UI migration by design).
- selection and follow — click selection unchanged; the drawer's
  "Follow unit" rides the orbit pivot on the unit's ground anchor; pan
  or re-toggle releases; deselection clears.
- documented/reconstructed/contested — symbol fills unchanged; the
  drawer now says the word (`unknown` displays as "inferred
  (unrecorded)" per the 2026-07-09 ruling; `contested` passes through
  for the V2 bundle vocabulary).
- source/provenance drawer — see below.
- contours + relief shading — the two chips ported off IMGUI verbatim
  (labels kept, including "Reading light (presentation)").
- Soldier View entry markers, content warning, exact return — see below.

### The source/provenance drawer

Tap a unit (or "Sources" inside Soldier View): identity (name + side +
arm + echelon words), live strength (`n,nnn men`), current activity
(moving/holding + formation, `firing (musketry/artillery)` only while an
authored engagement window is live), the confidence word, and the source
list — the bracketing track segment's citation first (the same START-
keyframe carry rule the symbols use; moved verbatim from the IMGUI seed),
then the unit's engagement events (live first, each with clock window and
confidence). Where the record is silent the entry says **"no reliable
record"** — the no-faking gate's display text. From inside Soldier View
the drawer adds the viewpoint's editorial note (the ED-22 observer
exemption disclosure) and its claim ids. Data feed: the battle JSON's own
keyframe/event provenance via `BattleDirector.SelectedUnitInfo` /
`TryGetUnitInfo`; every line is built by pure `ProvenanceDrawer`
functions pinned in `ProvenanceDrawerTests`.

### Soldier View integration (real media)

- Entry markers exist ONLY while the clock is inside a viewpoint's
  window (t=8160..8820 for `garnett-road-to-angle`); `t1` exclusive;
  development fixtures (`development: true`, new optional schema field —
  the Phase 1 timecode proxy is marked) never surface a marker, so
  entering outside a window is impossible through the UI, and
  `SoldierViewPlayer.TryEnter` still refuses independently.
- Entry transition: authored cut behind a 0.22 s fade (§3.3); the orbit
  controller is disabled while inside, so exit restores the EXACT camera
  state as well as the exact battle second (player leaves the clock
  untouched; speed restored).
- Media: the player drives the P10 production encodes
  (`garnett-road-to-angle.full.mp4` 1440p30 / `.proxy.mp4` 720p30) from
  `StreamingAssets/SoldierView/`; missing full falls back to proxy with
  an on-screen badge; missing both refuses with a clear message
  (unchanged P1 contract). `scripts/p11-demo.sh` stages the media from
  `docs/benchmarks/captures/p10-gate/` (or `$P11_MEDIA_DIR`) and
  verifies the release sha256s.

### Seek-transition decision (from the P10 measurement, confirmed here)

**Seek-and-hold, no proxy-frame flash.** P10 measured median 33.9 ms /
worst 107 ms; re-measured this phase on the same 1440p stream through the
production player: median 63 ms, worst 101 ms over the 12-target battery.
A hold of ~2–3 frames reads as a held frame, not a defect, so the UI
shows nothing for it; a "settling…" badge appears only past 150 ms
(`HudModel.SettleIndicatorAfterSeconds`), which no measured seek reached.
The proxy-frame transition plumbing (P1) remains available; **the owner's
step 6 (seek feel) decides whether Phase 12 schedules it.** One test
correction landed with this: play-through-seek can freeze `Video.time` up
to half a frame past the requested frame center, which the player's
paused-drift corrector fixes one Update later — the P10 full-media test
now asserts the corrected steady state (the frame a viewer actually holds
on), which is also what the sync contract means.

### Content warning

Authored text (`content-warning.json`, v1) shown before the FIRST entry;
acknowledgement persists per authored VERSION (PlayerPrefs
`battleatlas.soldierview.contentwarning.ackVersion`) so a rewritten
warning re-surfaces; declining stays in the Atlas and does NOT count as
acknowledgement. Missing warning text BLOCKS entry (§9.2 is a locked
requirement). PlayMode-tested including cross-gate persistence.

### Credits

`StreamingAssets/credits.json` is now generated beside
`THIRD_PARTY_ASSETS.md` by `generate_attribution.py` (staleness fails the
reconstruction suite; 36 assets). The Credits chip shows the attribution
lines with license/source; the modal points at the full inventory doc.

## Suites (this machine, worktree)

| Suite | Before (main) | After |
| --- | --- | --- |
| pipeline (pytest) | 59 | 59 |
| tool (vitest) | 108 | 108 |
| reconstruction (pytest) | 89 | **90** (+ credits generation/staleness) |
| Unity EditMode | 319 (313 passed + 6 skips*) | **347** (343 passed + 4 skips*) |
| Unity PlayMode | 11 (10 + 1 needing full media) | **16** (all 16 passing with media staged) |

\* skips are gitignored-generated-input dependent (the four Angle
bake-fixture tests); the `symbolMaterial` scene skip and the generated-
terrain-material skip both converted to PASSES this phase. Net EditMode
delta: +32 new tests, −4 retired with the IMGUI placeholders.

## Known conditions / owed to Phase 12

- Seek-feel verdict (owner step 6) → proxy-frame transition go/no-go.
- Atlas terrain still displays 2.5× vertical exaggeration (pre-existing;
  the ribbon system is scale-agnostic; V2 DoD decision outstanding).
- Marker hover details render in the transport row, not floating
  tooltips; fine at this fidelity, revisit with accessibility pass.
- The credits view lists attribution only; per-asset license evidence
  stays in the repository docs (linked from the modal note).
- Conditions readout is the minimal wind line (plan allows).
- Non-16:9 windows letterbox the video; the slivers show the Atlas
  behind it (world labels hidden while inside). Black backing = Phase 12
  polish candidate.
- TMP Essential Resources are now committed under Assets/TextMesh Pro
  (runtime labels NRE'd in any player without TMP Settings — surfaced by
  the P11 standalone screenshot run). Unity Companion License content;
  bundled LiberationSans is SIL OFL 1.1 — flagged for the Phase 12
  license review alongside the CC0/CC-BY manifest discipline.
- P7 deferred polish (haze wall, banding, species-accurate trees)
  unchanged.


## Gate P11 verdict

**PASSED — closed by the project owner 2026-07-10** ("unless otherwise
mentioned the 12 points pass"; Soldier View entry/exit "works well").
Owner feedback routed to `docs/reconstruction/post-v2-punchlist.md`:
theater label legibility (own slice), unit-type differentiation in the
ribbon language, macro movement/orientation crudeness, the missing
speed designator inside Soldier View (assigned to Phase 12), and the
charge-phase intensity/carnage depth question (research-first slice:
casualty pacing per phase, behavior under fire, observer placement).
