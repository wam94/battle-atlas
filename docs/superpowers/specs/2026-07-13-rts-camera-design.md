# RTS Camera for Atlas Game View — Design

**Date:** 2026-07-13 · **Owner ruling:** trackpad-first, ~5 m zoom floor,
single mode (no dev/audience split). Approved in session.

## Purpose

Total War-style camera flight in the Atlas game view — pan, rotate,
zoom-to-ground — for two audiences at once: the owner's QA touring and
the eventual distributable. Today `OrbitCameraController` is touch-first
and its mouse path is compiled `#if UNITY_EDITOR`, so a standalone macOS
build has **no camera input at all**. This slice makes desktop input
first-class.

## Approach (chosen: extend in place)

Keep `OrbitCameraController`'s pivot/yaw/pitch/distance model — it is
already the right substrate. Add desktop input, terrain awareness, and
feel. Rejected: a replacement rig (re-wires follow-pivot, HUD PointerBusy,
SV transitions, six capture harnesses for no model gain) and Cinemachine
(package dependency + determinism risk in render harnesses).

## Input model (trackpad-first)

- Remove the `#if UNITY_EDITOR` fence — mouse/keyboard ship in builds.
- **WASD / arrows**: pan, yaw-relative; speed scales with zoom distance;
  **Shift** = fast multiplier.
- **Q/E**: rotate yaw. **R/F**: zoom.
- **Two-finger scroll**: vertical = zoom, horizontal = rotate yaw
  (trackpad-native twist).
- **Left-drag**: pan (drag-the-map, matches the one-finger touch idiom).
  Releases unit-follow, same semantics as today's pan. Click-vs-drag
  threshold so `UnitPicker` click-select still works.
- **Option/Alt-drag**: rotate (yaw + pitch).
- **No edge-pan** (fights trackpads).
- Touch paths unchanged.
- All flight input (including keyboard) respects the `AtlasHud.PointerBusy`
  gate; keyboard flight is muted while a modal or Soldier View owns the
  screen.

## Terrain awareness

- Controller gets an injected terrain-height sampler
  (`System.Func<float, float, float>` over world x/z, wired from the
  existing heightmap apparatus by whoever spawns the camera —
  BattleDirector/bootstrap).
- Pivot rides sampled terrain height and is clamped to battlefield bounds.
- Camera position is kept ≥ ~3 m above the terrain under it.
- Distance floor drops 50 m → **5 m**; ceiling unchanged (12 km).
- Near-clip plane becomes dynamic: eases from 10 m at altitude toward
  ~0.3 m near the ground (depth precision holds — reversed-Z, 20 km far
  plane).

## Feel

- Critically-damped smoothing on pivot/yaw/pitch/distance: input moves
  *targets*, the rig eases. Harnesses/tests can snap-set pose (targets =
  current ⇒ damping is identity), so captures stay deterministic.
- **Descent curve**: below a distance knee, the allowed pitch range eases
  toward oblique — zooming down tilts you toward the horizon, Total War
  style. User pitch input still works within the eased clamp.
- **Cursor-anchored zoom**: zoom moves toward the cursor's terrain point
  (ray-march the height sampler; pure math). Falls back to pivot-anchored
  when the cursor is off-viewport or over HUD.
- All feel constants (speeds, damping time-constants, curve knees) are
  serialized fields for cheap verdict-round tuning.

## Testing & invariants

- New math is pure functions in `OrbitMath` (descent curve, terrain
  clearance solve, pan-speed scaling, zoom-anchor ray-march) with EditMode
  tests; one PlayMode smoke test pins terrain clearance (pivot set below
  ground → camera resolves above it).
- Runtime-only slice: battle files + compiled bundle byte-untouched,
  stagingSeed pin d470c469 held, suite floors are current main's
  (tool 119 / pipeline 59 / recon 128+1 / EditMode 383+4 / PlayMode 20),
  perf floor ~59.5 FPS.
- Gate: owner flies it. Captures: a scripted flight strip (theater → zoom
  to 5 m over a formation → rotate → pan) at documented rig poses.

## Accepted consequences / residuals

- At 5 m the realtime pose-bucketed figures read as crude — accepted by
  owner ruling (Atlas is the map; Soldier View is the film).
- Per-phase home view (phase-switching residual) remains out of scope.
- Legacy `Input` API retained (consistent with the rest of the runtime;
  Input System migration is not this slice).
