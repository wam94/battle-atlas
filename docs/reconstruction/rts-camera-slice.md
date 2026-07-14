# RTS camera for the Atlas game view — slice report

**Branch:** `rts-camera` (unmerged; coordinator gate) · **Spec:**
`docs/superpowers/specs/2026-07-13-rts-camera-design.md` · 2026-07-13

Extends `OrbitCameraController`/`OrbitMath` in place into a trackpad-first
Total War-style flight rig: desktop input ships in standalone builds (the
`#if UNITY_EDITOR` mouse fence is gone), terrain awareness (pivot rides
sampled ground height, camera never dips within ~3 m of the terrain under
it, distance floor 50 → 5 m, dynamic near-clip), and critically-damped
smoothing that still lets capture harnesses and tests snap-pose the
camera deterministically. Touch is untouched.

## 1. Bindings

| Input | Action | Notes |
|---|---|---|
| `W`/`A`/`S`/`D` or arrow keys | Pan | Yaw-relative, speed scales with zoom distance |
| `Shift` + pan keys | Pan, fast | 3× multiplier |
| `Q` / `E` | Rotate yaw | |
| `F` | Zoom in | Closer to the ground |
| `R` | Zoom out | Rises away — mnemonic matches common fly-cam elevation binds (R/rise, F/fall) |
| Left-drag | Pan (drag-the-map) | Click-vs-drag pixel threshold — a plain click still reaches `UnitPicker`/selection |
| `Option`/`Alt` + left-drag | Rotate (yaw + pitch) | |
| Two-finger scroll, vertical | Zoom | Cursor-anchored — the ground under the cursor stays roughly put while zooming in |
| Two-finger scroll, horizontal | Rotate yaw | Trackpad-native twist |
| One-finger touch drag | Pan | Unchanged |
| Two-finger touch pinch/twist/vertical-drag | Zoom / rotate / tilt | Unchanged |

No edge-pan (fights trackpads, per the spec's owner ruling). All flight
input — keyboard included — is muted by `AtlasHud.InputLocked` (a modal,
the day panel, warning/credits/options, or Soldier View); pointer-based
input (drag start, scroll) additionally respects `AtlasHud.PointerBusy`
at the cursor, matching the pre-existing touch/click-select gating.

## 2. Feel-constant inventory (`OrbitCameraController`, all serialized)

| Field | Default | Governs |
|---|---|---|
| `orbitSpeed` | `0.2` | Inherited from pre-slice code; **unused** (was already dead before this slice — left untouched rather than silently repurposed) |
| `panSpeed` | `1.0` | Touch drag-pan and mouse left-drag pan (screen-delta based) |
| `tiltSpeed` | `0.1` | Touch two-finger vertical-drag tilt |
| `keyPanSpeed` | `0.1` | WASD/arrows pan (world-rate based, `× distance × dt`) |
| `shiftFastMultiplier` | `3` | Shift-fast pan |
| `keyRotateSpeedDegPerSec` | `90` | Q/E rotate |
| `keyZoomSpeedPerSec` | `1.5` | R/F zoom (multiplicative rate) |
| `dragThresholdPx` | `4` | Click-vs-drag pixel threshold |
| `mouseDragRotateSpeedDegPerPx` | `0.2` | Option/alt-drag rotate |
| `scrollZoomSpeed` | `0.05` | Two-finger vertical scroll zoom |
| `scrollRotateSpeedDegPerNotch` | `3` | Two-finger horizontal scroll rotate |
| `pivotSmoothTime` | `0.15` s | Critically-damped pivot ease |
| `yawSmoothTime` | `0.15` s | Critically-damped yaw ease (angle-aware, `SmoothDampAngle`) |
| `pitchSmoothTime` | `0.15` s | Critically-damped pitch ease |
| `distanceSmoothTime` | `0.15` s | Critically-damped distance ease |
| `minTerrainClearanceM` | `3` | Camera-above-terrain floor |
| `descentKneeDistanceM` | `150` | Distance below which the pitch ceiling / near-clip start easing |
| `descentMaxPitchAtFloorDeg` | `35` | Oblique pitch cap at the 5 m distance floor |
| `nearClipAtAltitudeM` | `10` | Near-clip at/above the knee (matches pre-slice value) |
| `nearClipAtFloorM` | `0.3` | Near-clip eased toward at the 5 m floor |
| `zoomAnchorMaxRayDistM` | `20000` | Cursor-anchor raymarch max distance (matches the far clip) |

`OrbitMath.MinDistance` drops `50 → 5` (global — both touch pinch-zoom and
desktop zoom now reach the 5 m floor); `MaxDistance` (12000) and pitch
bounds (`MinPitchDeg` 10 / `MaxPitchDeg` 85, now a distance-dependent
*ceiling* via `ClampPitchForDistance`) are unchanged constants.

All twenty-one tunables above are cheap verdict-round knobs — no code
changes needed to retune feel.

## 3. Design notes

**Pose contract (why the six capture harnesses needed no edits).**
`pivot`/`yawDeg`/`pitchDeg`/`distance` are now C# properties, not plain
fields. The private `target*` fields are what live input (keyboard/mouse-
drag/scroll) nudges each frame; the private `live*` fields are what
`LateUpdate` renders, critically-damped toward the targets
(`Vector3.SmoothDamp` / `Mathf.SmoothDampAngle` / `Mathf.SmoothDamp`).
Assigning a property is a **snap**: it sets `target = live = value` with
zero velocity in the same statement, so damping is trivially identity on
the next frame — exactly "targets = current" from the spec. Every existing
call site (`DayExpansionCaptureHarness`, `DayExpansion2CaptureHarness`,
`DayExpansion3CaptureHarness`, `PhaseSwitchCaptureHarness`,
`CartographyCaptureHarness`, `AtlasHdrpRender`) already poses the camera
by direct field assignment — that syntax is unchanged, so **zero capture
harness files needed edits**; all six were exercised by running the
EditMode/PlayMode suites (`AtlasHdrpRender`'s own reflection-driven
`Awake`/`LateUpdate` calls are unaffected — `heightSampler` stays null in
that edit-mode-only tool, exactly as before) and the flight-strip capture
run (§5) below, which reuses the identical posing idiom.

**Descent curve.** `OrbitMath.DescentProgress` (private) is a smoothstep
ease from 0 (at or above `descentKneeDistanceM`) to 1 (at `MinDistance`).
`DescentMaxPitchDeg` lerps the pitch *ceiling* from `MaxPitchDeg` (85°,
unrestricted) down to `descentMaxPitchAtFloorDeg` (35°) along that curve;
`ClampPitchForDistance` clamps user pitch input into
`[MinPitchDeg, ceiling]`. `DynamicNearClip` shares the exact same
progress curve, easing `nearClipAtAltitudeM` (10 m) toward
`nearClipAtFloorM` (0.3 m). Both curves are keyed off the *live* (already-
easing) distance every frame in `LateUpdate`, not the raw input target, so
the ceiling and the near-clip plane themselves ease in smoothly as the
camera descends rather than snapping the instant a key is pressed. The
150 m knee sits well below every existing capture harness's authored
distance (all ≥ 700 m — see §6), so none of the six pre-existing poses are
affected by the new ceiling.

**Terrain-clearance solve.** Two independent mechanisms, both driven by
the injected `heightSampler` (`Func<float,float,float>`, wired from
`BattleDirector.GroundHeightAt` — the same sampler `UnitPicker`'s
click-picker and the symbol drape use):
1. **Pivot rides terrain**: every frame (and on every snap-set), the
   pivot's XZ is clamped to the battlefield bounds
   (`OrbitMath.ClampPivotToBounds`, wired from the terrain's
   `transform.position`/`terrainData.size`) and its Y is set directly to
   `heightSampler(x, z)` — no lag, since Y is re-derived from the
   (possibly still-easing) XZ every frame rather than itself damped.
2. **Camera clearance floor**: after computing the camera's position from
   the eased pose, `OrbitMath.ResolveTerrainClearance` independently lifts
   it straight up if it dips within `minTerrainClearanceM` of the terrain
   sampled directly under the *camera* (not the pivot) — catches a slope
   grazing the camera's own footprint even when the pivot itself sits
   correctly on the ground (see the Culp's Hill capture, §5, where this
   fired: computed clearance 3.449 m vs. the raw 2.868 m the unclamped
   geometry would have given).

Both are inert (`heightSampler == null`) in EditMode rigs and the
edit-mode-only `AtlasHdrpRender` tool — pre-slice behavior there is
unchanged.

**Cursor-anchored zoom-anchor solve.** `OrbitMath.ZoomAnchorPoint` checks
the cursor's viewport position (`CursorInViewport`, a plain 0..1 box test)
and, if inside, ray-marches the cursor ray against `heightSampler` by
reusing `UnitPicker.RaycastTerrain` (rather than re-implementing the
march) — falling back to the supplied pivot on an off-viewport cursor or a
ray that finds no ground within `zoomAnchorMaxRayDistM`. On a zoom-IN step
only, `OrbitCameraController.ApplyCursorAnchoredZoom` blends the target
pivot toward that anchor point (`OrbitMath.ZoomAnchorPivot`, a plain
`Vector3.Lerp`) by the exact fraction the distance just shrank — the
standard "keep the point under the cursor fixed" zoom-to-cursor identity
for small per-step deltas. Zooming OUT leaves the pivot alone
(pivot-anchored), matching the touch pinch-zoom convention, which this
slice deliberately left untouched. R/F (keyboard) zoom is **not**
cursor-anchored — the mouse cursor may be resting anywhere (including over
the HUD) during keyboard-only flight, and pulling the pivot toward an
unrelated point the player wasn't looking at would be a surprise; R/F is
plain pivot-anchored distance change.

**Click-vs-drag threshold.** `HandleMouse` arms a drag only on a
mouse-down that isn't `PointerBusy`, then requires `dragThresholdPx` of
cumulative screen-space movement before treating it as a pan/rotate — a
plain click never nudges the camera. `BattleDirector.HandleSelectionClick`
fires on the *mouse-down* frame regardless (unchanged), so click-select
was never actually at risk of the drag machinery stealing its click; the
threshold is what keeps a pure click from producing a one-pixel camera
flick and, if a followed unit is selected, from spuriously dropping
`followPivot`. Once a drag is confirmed it rides out the button hold even
if the cursor later crosses a HUD strip (only the *start* of a drag needs
a clean, non-HUD mouse-down) — `AtlasHud.InputLocked` still hard-stops and
clears the drag state immediately if a modal/Soldier View grabs the screen
mid-drag.

## 4. Invariant evidence

| Invariant | Result |
|---|---|
| `git diff main -- app/Assets/Battle/` | **Empty** (verified) |
| Compiled bundle byte-identical | Implied by the empty diff above — `app/Assets/Battle/Angle/angle.bundle.json` untouched |
| `stagingSeed` pin | `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1` — **held**, grep-verified in the committed bundle |
| `tool vitest` / `pipeline pytest` / `reconstruction pytest` | **Not re-run.** `git diff main --stat -- tool pipeline reconstruction` is empty — this slice touches only `app/Assets/Scripts/`, `app/Assets/Tests/`, and this doc/script pair, so those suites are unaffected by construction. Floors carried forward from main (119 / 59 / 128+1) without spinning up three fresh toolchains for a zero-diff area; happy to run them on request. |
| Unity EditMode | Baseline 383 passed + 4 skipped (387 total) → **405 passed, 4 skipped, 0 failed (409 total)** — the 22 new `OrbitMathTests` all pass, nothing else regressed |
| Unity PlayMode | Baseline 20 passed → **21 total (20 baseline + 1 new — `OrbitCameraTerrainClearanceTests`)**. Run 1: 18 passed, 3 failed; run 2 (identical command, no code changes between runs): 20 passed, 1 failed — a **different** subset of the same `SoldierViewFullMediaSeekTests`/`SoldierViewPlayerSyncTests` real-time video-seek-latency assertions failed each time (e.g. "drift 90.00 frames", "video crept while paused 2.9667 vs 3.0"). Pre-existing, wall-clock-sensitive video decoder tests this slice touches zero lines of (`SoldierViewPlayer.cs` is untouched) — flaky on this machine, not a regression. Every camera/orbit-related test (including the new one) passed on **both** runs. |
| Perf floor (~59.5 FPS) | **59.4 / 59.6 / 59.5 / 59.5 FPS** at t=0/8160/8700/9000 (`docs/benchmarks/captures/rts-camera/p0-benchmark.json`) — at or above floor at every sampled timestamp |
| Unity CLI invocation | `-batchmode -runTests -buildTarget OSXUniversal`, this worktree's own `app/Library`, gitignored inputs (`data/heightmap`, `data/landcover`, `app/Assets/Generated`, `app/Assets/StreamingAssets/SoldierView/*.mp4`) restored by copying from the owner's main checkout (byte-identical, `.meta` GUIDs intact) rather than `CartographyStage.PrepareScene` — `app/Assets/Generated` already held the correctly-imported terrain, so re-running the importer was unnecessary; no `-nographics` used |
| New `.meta` files | Unity-generated via the batchmode test runs themselves (which import new assets before running) and committed — `app/Assets/Tests/PlayMode/OrbitCameraTerrainClearanceTests.cs.meta` |

## 5. Captures

`docs/benchmarks/captures/rts-camera/` (force-added; gitignored by
default like every other slice's capture directory):

- `rtscam-01-theater.png` … `rtscam-12-near-clip-floor.png` — the scripted
  flight strip (`RtsCameraCaptureHarness`, launched with `-rtscamshots`):
  theater altitude → zoom over the Angle wall crossing down through
  tactical to the **5 m distance floor** (`04`) → three low-altitude
  rotate steps (`05`-`07`, yaw 0/90/180 at 40 m) → a three-step pan along
  the Union line (`08`-`10`) → a terrain-clearance shot on the **Culp's
  Hill slope** (`11`) → a dedicated **dynamic-near-clip-floor** shot
  (`12`, same pose as `04` — near-clip eased to 0.3 m, called out
  separately per the gate's ask).
- `rts-camera-poses.json` — the exact pivot/yaw/pitch/distance flown at
  each step, plus the *measured* camera Y, ground Y, clearance margin, and
  near-clip plane the running rig actually produced (not just the
  authored pose) — e.g. pose `04`/`12`: pitch clamped to the 35° ceiling,
  near-clip `0.3`, clearance `3.449` m (the terrain-clearance floor caught
  it — raw geometry alone would have given 2.868 m); pose `11`: clearance
  `30` m (comfortably clear, clamp inactive).
- `atlas-t{0,8160,8700,9000}.png`, `p0-benchmark.json` — the standard perf
  sample (§4).
- `scripts/rts-camera-captures.sh` — reproduces the whole set (build +
  flight strip + perf sample) in one command.

No separate keybindings image — the table in §1 is the glance-while-flying
reference.

## 6. Residuals / notes for the owner's verdict round

- **R/F zoom direction** is a judgment call (the spec names R/F but not
  which is in/out): `F` = in, `R` = out, matching the common fly-cam
  elevation mnemonic (R/rise, F/fall). Easy to swap
  (`OrbitCameraController.HandleKeyboard`) if it reads backwards in hand.
- **`orbitSpeed`** was already an unused field before this slice (declared,
  never referenced) — left alone rather than silently repurposed or
  deleted; flagging in case the owner wants it cleaned up separately.
- **Descent-curve numbers** (`descentKneeDistanceM` 150, `descentMaxPitchAtFloorDeg`
  35) and all smoothing time constants (0.15 s) are first-pass values
  reasoned from the geometry and the existing capture poses (all ≥ 700 m,
  safely above the 150 m knee) rather than tuned by eye in a live
  session — exactly the kind of thing the feel-constant table (§2) is for.
- **PlayMode video-latency flakiness** (§4) predates this slice and is
  unrelated to it, but is worth a separate look if it's new since the
  last audit — not investigated further here as out of scope.
- Accepted consequences carried from the spec: crude soldier-figure
  read at the 5 m floor (Atlas is the map; Soldier View is the film), no
  per-phase home view, legacy `Input` API retained.

## 7. Return-to-coordinator summary

- **Status:** implementation complete, suites green (see §4), captures
  and report in place, pushed to `origin/rts-camera`. Not merged — awaiting
  the owner's flight.
- **Branch head:** `a41534aea8162f336efeef0106b7a1eadd9c8deb`
- **Report path:** `docs/reconstruction/rts-camera-slice.md` (this file).
- **Feel-constant list:** §2 (twenty-one fields, all serialized).
- **Needs an owner ruling:** the R/F direction mnemonic (§6) if it reads
  backwards; whether to spend a follow-up cleaning up the dead
  `orbitSpeed` field.
