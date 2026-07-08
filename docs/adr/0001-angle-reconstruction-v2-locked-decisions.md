# ADR 0001 — Angle Reconstruction V2: locked decisions

**Date:** 2026-07-08
**Status:** Accepted
**Plan of record:** `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` (committed at `64d572b`)
**Preservation tag:** `v2-baseline` (the pre-V2 state; created before any V2 work)

## Context

The project pivots from an iPhone sand-table demo toward a desktop-first
(Apple Silicon macOS) interactive reconstruction of Pickett's Charge at the
Emmitsburg Road and the Angle (`t=8040..9000` on the canonical clock), with a
pre-rendered fixed-viewpoint "Soldier View" synchronized to the battle clock.
These decisions are requirements, not open design questions. They are recorded
here so later phases do not relitigate them.

## Decisions (locked)

1. Keep Unity as the playback, staging, and final-render engine.
2. Target Apple Silicon macOS first. Playback must work on a base M1-class
   Mac; offline rendering may take longer on lower-end hardware.
3. Defer iPhone support. Do not preserve mobile rendering constraints at the
   expense of the desktop visual target.
4. Preserve the current 190-unit July 3 reconstruction as Atlas Mode context.
5. Stop full-cast expansion until the Angle slice passes all final gates.
6. Use true-scale terrain at `1.0×` vertical scale in all new work.
7. Use a single canonical reconstruction. Do not build scenario switching.
8. Show the basis and uncertainty of the canonical choice where sources
   disagree.
9. Soldier View is a fixed camera-level experience, not locomotion or
   gameplay. The user may enter, exit, play, pause, and seek — never steer.
10. Soldier View contains explicit falls, blood, wounds, and persistent
    bodies, presented soberly with a content warning.
11. Do not add dismemberment or invented named-person wound details unless
    separately approved and specifically sourced.
12. The repository is open source. Every required asset must be
    redistributable under CC0, CC-BY, or a compatible project-owned license.
13. Do not make Unity Asset Store EULA assets, Mixamo source files, or other
    non-redistributable content required for a clean build.
14. Generated final video is a release artifact, not a normal Git object.
15. Historical realism and intelligibility outrank spectacle.

## Consequences

- Unity HDRP path tracing is unavailable on Metal; the visual target uses
  HDRP raster (pending the Phase 3 bake-off), baked/probe indirect lighting,
  and offline accumulation via Recorder. DX12 ray tracing is never a
  dependency.
- Soldier View exists only where an authored `ViewpointDefinition` and
  rendered media file exist; every additional viewpoint costs render time and
  release size approximately linearly.
- The existing macro `battle.json` is not replaced immediately; Angle V2 data
  compiles into a tactical sidecar (`angle.bundle.json`) and integrates
  progressively.
- Full-battle expansion (Days 1–2, more units) is frozen until the Angle
  slice passes its final gates.

## Phase 0 state at adoption

- Baselines re-measured at `v2-baseline`: tool 108/108, pipeline 38/38,
  Unity EditMode 119/119 (see `docs/benchmarks/2026-07-08-v2-phase0-baseline.md`).
- Current scene renamed `Assets/Scenes/Atlas.unity` without functional change.
