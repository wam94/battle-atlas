# Gate P10 evidence — offline render pipeline (plan §12 Phase 10)

**Gate criterion:** two separate renders of the same 10-second range
produce identical logical metadata and visually identical output within
documented nondeterministic GPU tolerances; encoded video seeks
acceptably on target hardware.

Evidence dir: `docs/benchmarks/captures/p10-gate/` (staged on the
`v2-phase10` worktree and copied to the main checkout's same path;
gitignored generated media). Machine: Apple M4, 24 GB, macOS 26.5.1,
Unity 6000.4.11f1, offline HDRP profile, 2560×1440p30.

## 1. The Gate P9 must-fix first: the teleport at t≈8635

Root-caused from per-frame machine probes before touching anything else
— full narrative in `docs/reconstruction/p10-teleport-postmortem.md`.
The camera path was innocent (max 6.6 mm/frame). THREE defects of one
class (placement discontinuities) were found and fixed deterministically:

1. **Lens pass-through** (the owner's report): Armistead's column walks
   through Garnett's halted line; slot 1014's track passed **0.06 m**
   from the camera at t=8635.97 — a body crossing the near plane at
   walking pace reads as a materialization. Fix: `LensGuard`, a
   render-time chord deflection around the observer (0.6 m,
   presentation-only, scrub-invariant).
2. **Formation wheel steps**: the compiled facing table steps 17–20°/s
   at segment boundaries (Armistead's breach at t=8639..8640 wheels his
   ~370 m frontage's flanks at up to ~65 m/s — 30,944 frame-pairs above
   sprint speed in one 40 s window; also the root cause of P9's
   documented "flank translation" camera limitation). Fix:
   `SmoothFormationFrame` — per-unit placement-frame smoothing bounded
   at 3 m/s flank speed; body facing unchanged; Union units untouched.
3. **Crossing-chain hold pop** (caught by the new full-window
   preflight): a crossing starting while the previous crossing's
   catch-up still blends snapped the hold to the raw formation position
   (garnett slot 241: 2.44 m in 0.1 s at t=8374). Fix: `PositionUpTo`
   computes every hold through the pipeline — continuous by
   construction.

**Before/after for your eyes:**

- Before: `../p9-gate/p9-proof-60s-av-fp-1440p.mp4` at ~24.5–26.5 s
  (the man materializing through the camera) and ~29–30 s (the
  mid-ground crowd sliding sideways in one second).
- After: **`p10-teleport-fix-60s-1440p.mp4`** — the same t=8610..8670,
  same slot, re-rendered with the fixes. At ~24.5–26.5 s Armistead's
  men now pass AROUND the observer at arm's length; at ~29–30 s the
  breach wheel is a gradual pivot.

**Machine verification:** `teleport-probe.json` (after-fix run): 0
spike pairs above even the near-camera fast-walk bound (was 30,944);
`p10-preflight.json`: the full padded window t=8160..8820.5, every unit,
every slot — 62.1M coarse pairs, 10,757 suspect windows refined at
30 fps, ALL of them the designed crossing-exit root-motion hand-off,
**zero violations**; camera max 0.1255 m/frame (bound 0.15). EditMode
regressions: `NoTeleportTests` (6 tests).

## 2. Determinism pair (the gate's machine half)

`Phase10Render.RenderDeterminismPair`: t=8400..8410 (300 frames)
rendered TWICE from two fully independent stagings (fresh scene, fresh
context compile each pass). Report: `p10-determinism.json`. **PASS.**

- Freeze metadata (git SHA, input checksums, settings hash): identical.
- Logical state digests at 10 probe times: identical.
- Pixels: worst frame 0.35% differing pixels, all within the Phase 8
  envelope (measured max delta 3 there) except at most **4 isolated
  outlier pixels** per frame (~0.0001% of 3.7 M) where two independent
  stagings resolve exact depth/coverage ties differently on hard edges.
  Documented two-tier tolerance: the Phase 8 envelope (≤8% at delta
  ≤12) plus ≤8 isolated outlier pixels/frame. Sample pairs:
  `p10-det-a-*.png` / `p10-det-b-*.png`.
- Getting to PASS found and fixed a real harness defect: two static
  material caches (`GateP6Render.RemappedMats`, environment `propMats`)
  handed DESTROYED materials to any second staging in one process —
  pass B rendered figures (then distant props) magenta. First honest
  pair run: 96–99% differing; after the cache fixes: 0.35%.

## 3. The production render

`Phase10Render.RenderProduction` + `scripts/p10-chunk-harvester.sh`
(see `docs/reconstruction/render-runbook.md` for the exact end-to-end
reproduction).

- Window: t=8160..8820 + 0.5 s pad past t1 (the P1 media contract's
  end-guard requirement) = **19,815 frames** at 2560×1440p30.
- Chunking: 12 × 60 s resumable chunks with per-chunk manifests (git
  SHA, bundle checksum, viewpoint, battle-time range, frame formula,
  settings hash); freeze record `p10-freeze.json`.
- Measured: **0.280 s/frame** weighted (0.217 in the open fields to
  0.349 in the smoke-heavy wall chunks) = **1.54 h pure render**;
  **1.83 h wall** including every crash/restart; peak managed memory
  **1,209 MB**; PNG scratch ~3.2 MB/frame (~63 GB full sequence —
  rolling-harvested into chunk encodes because the machine had ~58 GB
  free; the 12 chunk mp4s total **1.6 GB** and concat losslessly into
  the deliverable stream).
- **The crash story this pipeline was built for happened**: macOS
  SIGKILLed the Unity batch process EIGHT times across the render
  (exit 137, jetsam; managed memory flat ~1 GB — a native leak under
  long batch rendering, worst after my mid-run asset-unload mitigation
  which is why `scripts/p10-render-loop.sh` exists). Every kill
  resumed losslessly from the chunk manifests; zero frames were lost
  or re-encoded incorrectly (final decoded count re-verified 19,815).
- Chunk manifests carry three gitShas (documentation/pipeline-infra
  commits landed mid-render between resume attempts); the render-
  relevant code and the **settingsHash are identical across all 12
  chunks** (verified), and the deterministic scene code is unchanged
  since the fixes commit.

## 4. Encodes and measured media

`scripts/p10-encode.sh` (pinned imageio-ffmpeg 7.1 route; frame
continuity verified before encode — missing/duplicate frames hard-fail
with the list; final decoded frame count re-verified = 19,815).

| file | size | avg bitrate | notes |
| --- | --- | --- | --- |
| `garnett-road-to-angle.full.mp4` | 1.67 GB | 20.18 Mbit/s | 2560×1440p30 H.264 CRF18, GOP 30 (1 keyframe/s), +faststart, AAC 192k full mix; 19,815 frames decode-verified; duration 11:00.50 |
| `garnett-road-to-angle.proxy.mp4` | 331 MB | 4.01 Mbit/s | 1280×720p30 CRF20, same audio |

Audio: the full-length deterministic mix (t=8160..8820.5) built from
the Phase 9 stem pipeline (`build_viewpoint_audio.py`); stems retained
at `app/RenderOutput/p10/stems-full/` (§9.3). Checksums:
`p10-media.sha256` and the release manifest.

## 5. Seek behavior on the real 1440p media (gate's second half)

PlayMode `SeekLatency_Full1440pProductionMedia` (12 deterministic
seeks: long jumps across hundreds of GOPs, near jumps, sub-second
nudges, both directions), report `app/p10-seek-latency.json`:

- median **33.9 ms**, worst **107.3 ms** (P1 proxy-only numbers were
  10.2/19.0 ms; the media contract predicted the real 1440p stream
  needed its own measurement).
- Every settle landed within one video frame of the battle clock
  (asserted per seek).
- Judgment: the median is one video frame — seek-and-hold remains the
  right transition. The single worst seek (107.3 ms ≈ 3.2 frames)
  grazes the P1 revisit trigger (~100 ms): if the owner feels long
  jumps at 1440p, Phase 11 can enable the proxy-frame seek-settle
  transition the player already has the plumbing for.
- End-guard: media padded 0.5 s past t1, so the unreachable last-frames
  window sits entirely in padding.

## 6. Release manifest (owner publishes at Phase 12 — NOT published)

`p10-release-manifest.json` + `p10-release-notes.md` (names, sizes,
sha256, provenance, and the exact `gh release create` command).

## 7. Suite state at evidence time

- pipeline **59**
- reconstruction **89**
- tool **108**
- Unity EditMode **319** = 317 passed + 2 known conditional skips
  (313 baseline + 6 new NoTeleportTests)
- Unity PlayMode **11/11** (10 baseline + the 1440p production-media
  seek measurement; no skips — media staged)

## Viewing guide (the full 11-minute film)

`garnett-road-to-angle.full.mp4` — battle time = 8160 + video seconds:

| video time | battle t | chapter |
| --- | --- | --- |
| 0:00 | 8160 | In line on the west side: the advance begins; Codori buildings behind, the crest ahead |
| 3:20–4:10 | 8360–8410 | The observer's own road crossing: west rail fence climb, the packed road, the east fences (the trailing left flank crosses late) |
| 4:10–7:00 | 8410–8580 | The long advance under artillery: strikes begin taking men, smoke thickens ahead |
| 7:00–9:00 | 8580–8700 | `take_canister` at the wall: the line stalls, Cushing's canister, men falling around the camera; Armistead's column presses through (~7:54, the fixed teleport window) and breaches right of you (~8:00) |
| 9:00–11:00 | 8700–8820 | The repulse: the line turns back; bodies and wreckage of the approach corridor on the way out |

Judge, per the gate: identical logical metadata + tolerance-bounded
pixels for the determinism pair (§2), and acceptable seeking on the
real media (§5). The owner's teleport is gone (§1 before/after).
