# ADR 0003 — Render pipeline for the Angle visual target: HDRP

**Status:** Proposed (owner decides at Gate P3)
**Date:** 2026-07-08
**Context:** Angle Reconstruction V2 plan §12 Phase 3 (branch `v2-phase3`);
evidence in `docs/benchmarks/2026-07-08-v2-phase3-bakeoff.md` and the
`docs/benchmarks/captures/p3-*` frame pairs.

## Context

The V2 plan targets museum-grade offline-rendered realism for Soldier View
on Apple Silicon (raster only — no Metal path tracing, §3.2), while Atlas
Mode keeps real-time documentary cartography. The plan's default (§12
Phase 3) is HDRP **unless** it (a) fails on Apple Silicon, (b) breaks
required instancing without a tractable replacement, or (c) shows no
meaningful visual advantage in the target frames.

Phase 3 staged identical deterministic content — the true-scale 800 m Angle
terrain crop, placeholder road/wall/fences/wheat/copse, 100
`RenderMeshInstanced` soldiers, deterministic smoke, ephemeris sunlight at
t=8400 (15:20 LMT) — and rendered the same theater/tactical/eye-level
frames headlessly from URP 17.4.0 and HDRP 17.4.0 under Unity 6000.4.11f1.

## Findings against the three escape hatches

1. **Apple Silicon:** HDRP 17.4.0 installs, compiles, and renders on
   macOS/Metal (M4) in batchmode and in-editor. No failure.
2. **Instancing:** `Graphics.RenderMeshInstanced` draws (soldiers, smoke)
   render correctly under HDRP, including in headless render requests.
   No replacement needed.
3. **Visual advantage:** the HDRP frames show a physically based sky with
   altitude/atmosphere response, physical light units + exposure control
   (the calibration path the offline renderer needs), and volumetric-ready
   fog. At placeholder asset quality the gap is visible but modest —
   URP's frames are legible and creditable. The advantage argument rests
   substantially on what Phases 6–10 need next (volumetrics for smoke,
   area/probe lighting, physically based materials at hero distance,
   accumulation motion blur via Recorder), which are HDRP-native and
   URP-partial. "No meaningful advantage" is arguably not disproven by
   these placeholder frames alone — this is flagged for the owner rather
   than smoothed over.

## Cost observed

HDRP renders the same 1440p frame ~1.4–1.5× slower than URP
(eye: 50.6 vs 34.7 ms incl. readback) and uses marginally more memory
(361/763 MB vs 348/772 MB allocated/reserved). Both are orders of magnitude
inside the §3.5 offline budget. Setup cost: one generated global-settings
asset, per-scene Volume (sky/exposure/fog), physical light units, and a
documented reflection workaround for headless global-settings binding
(version-pinned to Unity 6000.4).

## Decision (proposed)

Adopt **HDRP** as the visual-target and offline-render pipeline for the
Angle slice (Phase 4 migration), per the plan default: none of the three
escape conditions occurred, and the capabilities the later phases require
(volumetric smoke, physical lights/exposure, Recorder accumulation) are the
strongest on HDRP. Atlas Mode remains URP-active on `main` until Gate P3
approval explicitly starts Phase 4; the bake-off machinery keeps the
project's committed settings URP throughout.

## Consequences

- Phase 4 (full HDRP migration) begins only after the owner approves a
  Gate P3 eye-level frame and this ADR.
- Custom URP shaders (SoldierVertexTint, Flag, terrain tint modulation)
  need HDRP-compatible equivalents in Phase 4; the bake-off already shows
  pipeline-default Lit shaders drop the vertex-color language.
- The M1-8GB playback budget must be re-verified after migration (HDRP's
  floor is higher than URP's; Phase 12 owes the measurement either way).
- If the owner instead judges the HDRP advantage insufficient for the added
  complexity, staying on URP is viable: everything staged here runs on
  both, and the decision reverses cheaply until Phase 4 converts
  shaders/materials wholesale.
