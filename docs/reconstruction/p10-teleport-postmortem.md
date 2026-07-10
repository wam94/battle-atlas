# P10 must-fix postmortem — the Gate P9 "teleport" at t≈8635

**Defect report (owner, Gate P9 close):** a visible teleport at ~25 s
into `p9-proof-60s-av-fp-1440p.mp4` (proof window t=8610..8670, so
battle time ≈ t=8635). Root-cause required before any Phase 10
production render.

**Method:** no pixels were trusted. `Phase10TeleportProbe` compiles the
deterministic action context (no rendering) and dumps the camera pose
and EVERY slot of EVERY unit per frame (30 fps) around the defect
window, flagging any per-frame planar delta above sprint speed
(0.34 m/frame) — and above fast-walk (0.12 m/frame) within 40 m of the
camera — plus a full trace of any slot passing within 3 m of the lens.
Evidence: `docs/benchmarks/captures/p10-gate/teleport-probe.json`
(regenerable; see the render runbook). The camera path itself was clean:
max 6.6 mm/frame across the whole window.

Three real defects were found — the owner's report, one adjacent defect
four seconds later, and one more caught by the full-window preflight
sweep the fixes mandated.

## Defect 1 — lens pass-through (the owner's teleport, t≈8635.8)

`csa-armistead` slot 1014's compiled track passes **0.06 m** from the
hero camera at t=8635.97 (slots 222–225 pass at 0.25–0.77 m around
t≈8634.6). Armistead's column advances straight through Garnett's
halted line — historically right — but nothing stopped a man walking
through the OBSERVER. At walking pace a body crossing the near plane
reads as a materialization: invisible until his hat brim sweeps the
near plane (video frame ≈25.8 s), then a full torso at frame center.

**Fix:** `LensGuard` (`app/Assets/Scripts/SoldierView/LensGuard.cs`) —
a render-time deflection: any hero-tier figure entering the 0.6 m
guard circle around the camera's plan position slides along the chord
of its own travel direction, walking AROUND the observer at arm's
length. The deflection is zero at the circle boundary (continuous in
t), a pure function of the resolved path and camera position
(scrub-invariant), and presentation-only — logical state, Gate P8
digests, casualty bookkeeping, and audio-event positions are untouched,
exactly like the hidden observer figure. Standing figures inside the
guard are pushed radially. 0.6 m leaves the observer's own
touch-of-elbows file (nominal 0.5 m spacing; measured ≥ 0.63 m with
formation jitter) essentially untouched.

## Defect 2 — formation wheel steps (t=8639..8640 and three more)

The compiled per-second facing table STEPS at segment boundaries:
Armistead's `advance-through-wreck` → `breach` boundary turns the unit
frame 82.8° → 99.5° in the single second t=8639..8640. Placement
rotates every slot offset by that frame, so the step wheels the whole
line at once: on Armistead's ~370 m frontage the flank men swept at up
to ~65 m/s (the probe counted **30,944** frame-pairs above sprint speed
in one 40-second window). Every CSA brigade carries a 17–20°/s step
somewhere (Garnett and Kemper at t=8279, Fry at t=8579, Armistead at
t=8639); Union units carry none. The Gate P9 evidence's documented
"flank translation" camera limitation (~6.8 m/s lateral rides) was this
same defect seen through the camera's ±2.6 s smoother.

**Fix:** `AngleActionContext.SmoothFormationFrame`
(`SoldierActionResolver.cs`) — the unwrapped placement frame
(`UnitRuntime.frameFacing`, already the about-face fix's home) is
smoothed with a symmetric triangular kernel sized per unit from its own
worst step and widest slot offset, so no wheel moves any man faster
than `MaxWheelFlankSpeedMps` (3.0 m/s — double-quick, drill-plausible
for a wheeling flank). Body facing still reads the raw compiled table
(a man may snap his own shoulders in a second; the LINE may not).
Symmetric + precomputed = scrub-invariant. Units without fast wheels
(all Union units) are bit-untouched. After the fix the same probe
reports **0** spike pairs.

## Defect 3 — crossing-chain hold pop (preflight catch, t=8374)

The full-window preflight sweep (mandated by this postmortem: every
unit, every slot, t=8160..8820.5) caught `csa-garnett` slot 241 moving
2.44 m in 0.1 s at t=8374, at the paired east road fences. Cause:
crossing detection spaces consecutive crossings by `CrossDur + 1 s`
(5 s), but a full hold + catch-up lasts `CrossDur + CatchupDur` (7 s) —
so a man could still be mid-catch-up from fence 1 when his fence-2
crossing began, and the old hold snapped him to the RAW formation
position at that instant.

**Fix:** `PositionUpTo` (`SoldierActionResolver.cs`) — a crossing's
hold position is now computed THROUGH the pipeline with all
strictly-earlier crossings applied. At the instant crossing *i*
activates, that value equals the position an instant before, so the
switch is continuous by construction (recursion depth = the slot's
prior crossing count).

## The one designed exception

At the END of a crossing clip the logical position absorbs the clip's
forward root motion in one frame (`CrossTravelM` = 1.3 m). The rendered
mesh is continuous — the clip roots the body over the rail — so this is
exempted (bounded at `CrossTravelM` + sprint) everywhere the no-teleport
bound is asserted.

## Regression coverage

- `NoTeleportTests` (EditMode, 6 tests): wheel-rate bound for every
  unit; frame end-point preservation; render-rate no-teleport sweep of
  both defect windows (t=8630..8645 and t=8365..8385, every slot);
  lens-guard containment + continuity + identity; camera calm through
  all wheel boundaries.
- `Phase10Render.Preflight`: the full-window global assertion — every
  unit, every slot, t0..t1+pad at 10 Hz with 30 fps refinement of every
  suspect window, plus the camera at full render rate — hard-fails
  before any production render hours are committed. Report:
  `docs/benchmarks/captures/p10-gate/p10-preflight.json`.

## Before/after evidence

- Before: `p9-proof-60s-av-fp-1440p.mp4` (shipped Gate P9 media),
  ~24.5–26.5 s.
- After: `p10-teleport-fix-60s-1440p.mp4` in
  `docs/benchmarks/captures/p10-gate/` — the same t=8610..8670 window
  re-rendered with the fixes (see the gate evidence's viewing guide).
