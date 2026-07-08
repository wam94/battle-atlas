# V2 Phase 0 baseline — 2026-07-08

Preservation point for the Angle Reconstruction V2 plan
(`docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md`).

- **Git SHA:** `64d572b` ("plan: Angle Reconstruction V2 — adopt with executor amendments")
- **Preservation tag:** `v2-baseline` (annotated, on `main`)
- **Phase branch:** `v2-phase01` (cut from the tag; owner reviews before merge)
- **Measurement machine:** Apple M4, 24 GB, macOS 26.5.1, Metal 4
  (NOT the base-M1 playback floor — M1 8 GB numbers still needed before
  Phase 12; see plan §3.5, §12 Phase 12)
- **Unity:** 6000.4.11f1, URP, EditMode suite only (pre-V2 state)

## Suite baselines (measured, not inherited)

Run from a fresh worktree of `v2-baseline` with a clean Unity Library import
(gitignored `app/Assets/Generated/` copied from the working checkout; a truly
fresh clone regenerates it via `BattleAtlas ▸ Import Heightmap`).

| Suite | Command | Result |
| --- | --- | --- |
| Pipeline (Python) | `cd pipeline && uv run pytest -q` | **38 passed** (13.2 s) |
| Authoring tool (TS) | `cd tool && npm ci && npm test` | **108 passed** (16 files, 2.1 s) |
| Unity EditMode | `"$UNITY" -batchmode -projectPath app -runTests -testPlatform EditMode -testResults <xml> -logFile <log>` | **119/119 passed**, exit 0 |

These match the last recorded baselines (tool 108 / pipeline 38 / Unity 119 at
`af2022c`) exactly.

## Known pre-existing conditions (NOT attributable to V2 work)

Recorded per plan §12 Phase 0 so later regressions are triaged correctly:

- The Phase 6 smoke/audio systems on `main` are code-complete but **not yet
  owner-verified** in editor or on device.
- Open performance case: bombardment peak, ~30 simultaneous obscuration
  emitters.
- Owner-unconfirmed fixes: muzzle bloom, pause silence.
- Terrain renders with 2.5× vertical exaggeration (sand-table convention) in
  the preserved Atlas scene; V2 locks 1.0× for all NEW work only.
- The Unity suite is EditMode-only at baseline; there is no PlayMode suite
  yet (Phase 1 adds the first one).
- `app/unity-test-review.log` was present untracked in the owner's working
  tree at pause time (not part of the baseline).

## Screenshots at t=0 / 8160 / 8700 / 9000

See `docs/benchmarks/captures/` policy below. Capture was performed headlessly
via the Phase 0 benchmark harness (development Mac standalone build run with
`-benchmark`); PNGs are gitignored generated media — the capture run's summary
(hashes, sizes, clock times) is recorded in this document's companion
`2026-07-08-v2-phase0-measurements.json` once captured.

Status: see "Measurements" section below (filled by the harness run; any item
that could not be captured headlessly is packaged into the Gate P1 owner
checklist instead).

## Real-time frame rate and memory

Measured by the same harness (development standalone, windowed 1440×900,
2-second warmup then 10-second sample per timestamp). The Unity editor was
open on the owner's checkout during this session, so editor-based measurement
was not attempted (plan §16.1: never disturb or kill the running editor). The
standalone-build path avoids the editor entirely.

Status: see "Measurements" section below.

## Measurements

(Filled in by `scripts/p0-benchmark.sh` output; blockers documented inline.)
