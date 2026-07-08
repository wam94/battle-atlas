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

## Measurements (captured 2026-07-08 via `scripts/p0-benchmark.sh`)

Method: Development macOS standalone built headlessly from this branch
(`BattleAtlas.EditorTools.BenchmarkBuild.Build`), run windowed 1440×900 with
`-benchmark`. The Unity editor was open on the owner's checkout throughout
this session, so all Unity CLI work (including this capture) ran from a git
worktree copy of the project — the editor and its checkout were never
touched (plan §16.1). Screenshots verified non-blank (t=8700 shows the Angle
crisis at wall clock 15:25:00 with units, smoke, and HUD).

Screenshots + JSON live in `docs/benchmarks/captures/` (gitignored generated
media; regenerate with one command). 10-second sample per timestamp, battle
playing at 60×, vsync on (60 Hz cap):

| t | screenshot | avg FPS | p95 frame | worst frame | allocated | reserved |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | atlas-t0.png | 59.6 | 17.3 ms | 27.1 ms | 263.5 MB | 402.8 MB |
| 8160 | atlas-t8160.png | 59.2 | 17.4 ms | 58.8 ms | 263.9 MB | 402.8 MB |
| 8700 | atlas-t8700.png | 59.6 | 17.3 ms | 22.3 ms | 264.0 MB | 402.8 MB |
| 9000 | atlas-t9000.png | 59.7 | 17.2 ms | 21.9 ms | 263.9 MB | 402.8 MB |

Notes:

- Development-build overhead means these slightly understate release
  performance; the vsync cap hides headroom. Good enough as the Phase 0
  "before" record.
- The worst frame (58.8 ms) lands at t=8160 — consistent with the known
  pre-existing bombardment-peak/obscuration performance case noted above.
- Numbers are from an M4/24 GB machine, not the base-M1 8 GB playback
  floor (still owed before Phase 12).
- Editor-grade capture (scene view, custom angles) remains interactive-only
  and is packaged into the Gate P1 owner checklist as an optional item.
