# Phase switching slice — one launch browses the whole battle

**Branch:** `phase-switching` (unmerged; owner gate) ·
**ADR:** 0005 (its deferred hot-swap item, now shipped) · 2026-07-13

The battle spans five reconstructed phases across three days (ADR
0005 manifest), but the app loaded one phase per launch — other
phases needed a relaunch with `-battleFile`. This slice makes the day
tabs → phase panel actually SWITCH phases in-session: a reconstructed
phase that is not the loaded one carries a "Load this phase" button;
not-reconstructed phases remain honest notes, never controls. Pure
runtime/UI — every battle file, the manifest, the moments files, and
the shipped bundle are byte-untouched (`git diff main -- app/Assets/
Battle app/Assets/StreamingAssets` is empty).

## 1. The switching architecture: the successor pattern

A switch (`AtlasHud.SwitchToPhase` → `SwitchRoutine`) is a
**teardown-and-respawn**, not a mutate-in-place:

1. **Guard + resolve.** Reject while switching/transitioning/in
   Soldier View; no-op if the target is already loaded. The phase's
   battle file resolves through `HudModel.BattleFileCandidates`
   (EditMode-pinned order): `-battleDir <dir>` →
   `StreamingAssets/Battle/` (standalone builds carry post-build
   copies — see §1.1) → `Assets/Battle/` (editor + PlayMode) → the
   `-battleFile` directory. Missing/unreadable file: loud warning +
   HUD status, current phase untouched.
2. **Freeze + remember.** Close the day panel and drawer (clearing
   the click selection), pause the clock, and record the outgoing
   phase's clock position in a session dictionary.
3. **Loading veil.** A code-created loading label + the fade overlay
   render for one frame before the blocking load, so the seconds-long
   hitch reads as "Loading <phase> …", not a freeze. World input is
   locked for the whole switch (`InputLocked`).
4. **Validate before teardown.** The target battle text is parsed
   once up front; a rejected file aborts the switch with the current
   phase still running.
5. **Teardown.** Destroy the old `BattleDirector` and the three
   per-battle components its Start spawned — `ObscurationField`,
   `AcousticField`, `UnitLabelField`. Each now releases its own
   runtime objects in `OnDestroy` (§2).
6. **Successor.** `BattleDirector.SpawnSuccessor(old, text, asset)`
   adds a FRESH director on the same GameObject with the same
   serialized wiring (terrain, clock, materials, command overlay) and
   the new battle injected via non-serialized `pendingBattleText` /
   `pendingAssetName` (precedence: switch > `-battleFile` > serialized
   asset — mirrored in `BattleAssetName`). One frame later its
   ordinary `Start` has run: parse, unit entries, families, events,
   fresh Obscuration/Acoustic/Label components. **A scene reload
   still returns to the serialized/`-battleFile` behavior** — the
   injection never persists.
7. **Clock re-base.** `Start` set `StartTime`/`EndTime` from the new
   file; the position becomes the phase's remembered spot (clamped)
   or t=0 for a first visit. **Design call, documented:** per-phase
   clock memory is session-only — returning to a phase resumes where
   you left it (browsing back and forth keeps context); a fresh
   launch starts every phase at its beginning.
8. **HUD refresh** (`RefreshAfterSwitch`): per-phase moments reload
   (the slice-2 `moments-<asset>.json` probe + the `battle`-field
   gate), moment markers rebuilt, timeline range re-laid-out,
   masthead words re-derived, day-tab highlight re-bound, **the
   manifest clock-echo verification re-run against the newly loaded
   battle** — a stale echo warns loudly (log + HUD status + the day
   panel line) on EVERY switch, not just at launch. Soldier View
   markers clear and re-gate (§4).
9. **Report.** The measured switch time (stopwatch from the accepted
   click through the refreshed HUD) logs and shows briefly in the
   status line; `AtlasHud.LastSwitchMs` exposes it to the harness.

### 1.1 Where standalone builds get the other phases

The scene serializes one battle asset; the editor resolves sibling
phases from `Assets/Battle/` directly. `BenchmarkBuild.Build` now
copies `Assets/Battle/*.json` into the built bundle's
`StreamingAssets/Battle/` **post-build** — the project sources and
project StreamingAssets stay byte-untouched, and a vanilla standalone
browses all five phases with no arguments. `-battleFile` keeps
working unchanged (the capture harnesses depend on it), and its
directory doubles as a resolution root, so a `-battleFile` run can
also switch among that directory's siblings.

## 2. The no-leak proof

Two layers:

- **By construction.** The successor pattern means no instance state
  can cross a switch — unit entries, symbol/roster meshes, MPB
  latches, salience/selection state, label slots, pick buffers, flag
  matrices all live on the destroyed components. The gap this slice
  closed was runtime OBJECTS those components created: new
  `OnDestroy` contracts release the per-unit meshes plus (new) the
  shared pose/flag meshes (`BattleDirector`), the puff mesh
  (`ObscurationField`), the 8 audio-source children + 3 synthesized
  clips (`AcousticField`), and the 48 pooled TMP label children
  (`UnitLabelField` — they parent to the director's GameObject, which
  survives the switch, so the component must take them down).
- **Pinned.** PlayMode `PhaseSwitchFlowTests`:
  - `Switch_ThenScrub_IsByteIdenticalToFreshLaunch` — a session that
    launches into a fixture battle, runs it (selection made, symbol
    meshes built), then switches to the REAL
    `gettysburg-july1-morning.json` and scrubs to t=9900, compared
    against a session launched straight into that file: **every unit
    of the phase, exact float equality** (position, strength, facing,
    formation, confidence) at the probe time, plus clock
    StartTime/EndTime equality; the fixture's unit id resolves to
    nothing and no selection survives.
  - `Switch_LeaksNoComponentsOrRuntimeChildren` — after a switch the
    host GameObject carries exactly one director, one obscuration,
    one acoustic, one label component, exactly 1+7 audio-source
    children and exactly 48 label children.

## 3. Switch time and steady-state perf

Measured by the evidence run (§5; Development standalone, one
session, `phase-switch-summary.json`):

| Switch | Time |
|---|---|
| july3-afternoon → july1-morning | 66 ms |
| july1-morning → july1-afternoon | 72 ms |
| july1-afternoon → july2-afternoon | 75 ms |
| july2-afternoon → july2-evening | 89 ms |
| july2-evening → july3-afternoon | 83 ms |

Every switch lands in **66–89 ms** (measured click → refreshed HUD,
Development standalone, M-series hardware) — the "few seconds"
budget was never approached; the loading veil reads as a blink. The
veil stays: a slower disk or a future larger phase file degrades to a
labeled wait, never a freeze.

Steady-state FPS on the identical July 3 Pickett's-Charge view
(t=8400, same orbit): **59.5 avg FPS at fresh launch vs 59.8 after
the five-phase round trip** — switching costs nothing at steady
state, which is the §2 no-leak result seen from the frame counter.

## 4. Soldier View across phases

The shipped viewpoints/media address ONE phase's clock and cast — the
scene's serialized battle asset (July 3 afternoon; the film contract,
ADR 0005 §4). The slice replaces day-expansion slice 2's blanket
"-battleFile disables Soldier View" with a per-phase gate:

- `BattleDirector.SerializedAssetName` records the home phase;
  `HudModel.ViewpointsApplyTo(home, loaded)` (EditMode-pinned) gates
  entry markers, the timeline window band, and `RequestEnter` itself.
- Switching to July 1/2 phases: markers and band vanish, a direct
  `RequestEnter` call gets an honest refusal status. Switching BACK
  to July 3 afternoon: entry is restored (the round-trip capture
  shows the marker back).
- A `-battleFile` launch now benefits too: viewpoints load, stay
  hidden on the foreign phase, and appear if the user switches to the
  home phase in-session — strictly better than the old session-wide
  disable.
- Test fixture rigs wire viewpoints over unnamed TextAssets; an empty
  home asset applies everywhere, so the existing PlayMode flows run
  unchanged (and the new flow test NAMES its fixture to engage the
  gate deliberately).

## 5. Suites, evidence

Suites (all green on the branch; worktree CLI runs, editor closed):
tool vitest **119** · pipeline pytest **59** · reconstruction pytest
**122 + 1 skipped** · Unity EditMode **383 passed, 0 failed, 4
skipped** (377+4 baseline + 6: battle-file resolution order ×3,
viewpoint gate ×2, successor wiring ×1) · Unity PlayMode **20
passed, 0 failed** (17 baseline + 3: fresh-launch equivalence, leak
audit, the HUD switch flow). Honesty note: one full-suite PlayMode
run had a single failure in the PRE-EXISTING
`SoldierViewPlayerSyncTests.PlayThenPause_ClockAndVideoStayTogether`
("video crept while paused", one 1/30 s frame) — decoder timing under
batch load, untouched by this slice; the class in isolation (10/10)
and the immediate full-suite re-run (20/20) both pass.

Evidence: `docs/benchmarks/captures/phase-switching/` (force-added;
owner copies in the main checkout's same gitignored path) — ONE
session (launched normally, no `-battleFile`) visiting all five
reconstructed phases through `AtlasHud.SwitchToPhase` (the day
panel's own code path): a distinct battle moment per phase (the
Reynolds moment, Barlow's Knoll, the Wheatfield, Culp's Hill at dusk,
Pickett's Charge on the return leg), the owning day panel per phase
(loaded phase lit, "Load this phase" on the others, the honest
not-reconstructed notes verbatim), `phase-switch-summary.json` with
per-switch times and the before/after FPS samples. Produced by
`scripts/phase-switching-captures.sh` (one Development build, one
run).

## 6. Residuals

1. **Session-only clock memory.** Per-phase positions are not
   persisted; a relaunch starts phases at t=0. Persisting them (the
   AccessibilityOptions PlayerPrefs pattern) is a small follow-up if
   browsing sessions want continuity across launches.
2. **The loading veil is minimal.** A centered label over the fade
   overlay; a progress read (parse → build → refresh) would need the
   load split across frames — not worth it at current switch times.
3. **Switch blocks the frame.** The battle parse + unit build runs on
   the main thread behind the veil (by design — determinism over
   async complexity at these sizes). If a future battle file grows
   past ~2 MB, consider a threaded parse.
4. **Orbit camera carries across switches** (deliberate: the user
   keeps their vantage on the same terrain). A per-phase "home view"
   (fly to the phase's action) is an editorial follow-up.
5. **`-benchmark` runs skip the HUD** (SoldierViewBootstrap early
   exit, pre-existing), so the benchmark harness cannot exercise
   switching; the capture harness's before/after FPS sample covers
   the perf question instead.
