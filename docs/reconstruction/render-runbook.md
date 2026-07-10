# Soldier View production render runbook (plan §5, §12 Phase 10)

How to reproduce the `garnett-road-to-angle` production render
end-to-end on a clean checkout. Every step is deterministic; the same
commit, inputs, and settings produce identical logical output and
visually identical frames within the documented GPU tolerance
(max channel delta 12 on ≤ 8% of pixels — the Phase 8 envelope).

**Machine used for the Phase 10 production numbers:** Apple M4, 24 GB,
macOS 26.5.1, Unity 6000.4.11f1. Playback targets a base M1 8 GB;
rendering wants 16 GB+.

## 0. Preconditions

- Unity 6000.4.11f1 (`"$UNITY"` below), **editor closed** for this
  project path — batch CLI is the only Unity owner (plan §16.1).
- `uv` for the Python pipelines.
- Disk: the full-resolution PNG scratch is ~63 GB (measured
  ~3.2 MB/frame). If that fits, nothing else is needed. If it does not
  (the Phase 10 production machine had ~58 GB free), run the **rolling
  chunk harvester** (step 3b) alongside the render: it encodes each
  completed 60 s chunk with the final delivery codec settings, verifies
  the decoded frame count, and reclaims the PNGs — steady-state disk
  use stays around ~12 GB plus ~3 GB of chunk encodes.
- No system ffmpeg is required: encoding falls back to the pinned
  **imageio-ffmpeg 7.1** static build from `reconstruction/`'s dev
  dependencies and fails clearly if neither is available.

## 1. Regenerate gitignored inputs

```sh
cd pipeline
uv run python -m terrain_pipeline.cli fetch        # USGS 1 m DEM cache (once)
uv run python -m terrain_pipeline.cli build        # macro heightmap
uv run python -m terrain_pipeline.cli crop         # true-scale Angle crop
uv run python -m terrain_pipeline.cli environment  # sourced environment bake
```

Input checksums (heightmap.json/raw, environment.json/splat) are
recorded in `p10-freeze.json` by every render entry point; a mismatch
means your inputs differ from the production render's.

## 2. Preflight — the global no-teleport gate

Asserts, for EVERY slot of EVERY unit across the whole padded window
(t=8160..8820.5): per-frame movement ≤ sprint (0.267 m/frame at 30 fps),
except the single designed crossing-exit hand-off frame
(≤ CrossTravelM + sprint); and the hero camera ≤ 0.15 m/frame. 10 Hz
coarse sweep + 30 fps refinement of every suspect window. Hard-fails
before any render hours are committed (see
`docs/reconstruction/p10-teleport-postmortem.md` for why this exists).

```sh
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.Phase10Render.Preflight \
  -logFile p10-preflight.log
# report: docs/benchmarks/captures/p10-gate/p10-preflight.json
```

## 3. The production render (chunked, resumable)

```sh
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.Phase10Render.RenderProduction \
  -logFile p10-production.log
```

- Output: `app/RenderOutput/p10/seq-full/frame_%06d.png` (gitignored),
  19,815 frames = (8820 − 8160 + 0.5 s pad) × 30 fps at 2560×1440.
  The 0.5 s pad past t1 satisfies the P1 media contract: the last ~3
  frames of a stream are unreachable seek targets (EndGuardFrames = 4),
  so the guard must sit in padding, never in content.
- **Resumable:** frames land in 60 s chunks; each chunk writes
  `app/RenderOutput/p10/manifests/chunk_NNN.json` when complete. A
  killed run (this machine has crashed mid-render) is simply re-run:
  chunks with a manifest and all frames on disk are skipped.
- Frame metadata: every chunk manifest carries the Git SHA, input
  bundle checksum, viewpoint id, battle-time range, the frame-time
  formula `t(frame) = 8160 + frame/30`, and the settings hash; the
  full freeze record is `app/RenderOutput/p10/p10-freeze.json`
  (also copied to the gate evidence).
- The scene is staged procedurally (the committed `AngleRender.unity`
  is a thin marker per project convention — content comes from code +
  data at render time). First-person: the observer's own figure is
  hidden; the lens guard (0.6 m) is active.

### 3b. The rolling chunk harvester (disk-constrained machines)

In a second terminal, for the whole duration of the render:

```sh
scripts/p10-chunk-harvester.sh
# when RenderProduction exits cleanly:
touch app/RenderOutput/p10/render-done   # lets the harvester finish + exit
```

Each completed chunk is encoded to
`app/RenderOutput/p10/chunks/chunk_NNN.mp4` with the exact final
delivery codec settings (libx264 preset slow CRF 18 yuv420p, 1
keyframe/s, closed GOP), its decoded frame count is verified against
the chunk manifest, and only then are its PNGs deleted. Chunk encodes
concat losslessly (`-c copy`) into the same stream a single-pass encode
of the same frames would produce; `p10-encode.sh` picks this "mode B"
automatically and re-verifies the total decoded frame count. Resume
still works: `Phase10Render.ChunkComplete` accepts an encoded chunk in
place of its PNGs.

**Harness decision (recorded):** the render uses the project's proven
`RenderPipeline.SubmitRenderRequest` loop (`GateP6Render.RenderOnce`)
under the offline HDRP profile
(`app/Assets/Settings/BattleAtlasHDRP_Offline.asset`), the same path
every gate P3–P9 rendered with. The Unity Recorder package (installed)
was not used: it requires play-mode timeline capture in batchmode,
which fights the pure `Pose(t)` staging and adds a nondeterminism
surface for no image benefit. **No accumulation motion blur:** the
owner accepted Gate P9 media rendered at one deterministic sample per
frame; accumulation would change the accepted look (decision recorded
here per plan §3.2; revisit only with owner sign-off).

## 4. Audio — the full-length deterministic mix

```sh
# export the event streams (same compiled data the frames render from)
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.GateP9Render.ExportAudioEvents \
  -logFile p10-audio-events.log

# build stems + mix for the padded window (byte-deterministic)
cd reconstruction
uv run python scripts/build_viewpoint_audio.py \
  --events ../docs/benchmarks/captures/p9-gate/p9-audio-events.json \
  --out ../app/RenderOutput/p10/stems-full --t0 8160 --t1 8820.5
```

Stems are retained (plan §9.3) so the mix can be revised without
re-rendering images.

## 5. Encode (pinned)

```sh
scripts/p10-encode.sh
```

Verifies the sequence is complete (hard fail with the missing/duplicate
frame list), then encodes:

- `garnett-road-to-angle.full.mp4` — 2560×1440p30 H.264 (libx264,
  preset slow, CRF 18, yuv420p), **GOP 30 = 1 keyframe/s** (seeking),
  `+faststart`, AAC 192k mix muxed, `-shortest`.
- `garnett-road-to-angle.proxy.mp4` — 1280×720p30 (CRF 20), audio
  copied.
- `p10-media.sha256` — media checksums.

Measured sizes/bitrates print at the end and are recorded in the gate
evidence and release manifest.

## 6. Verify

```sh
# determinism pair (Gate P10): two independent stagings of t=8400..8410
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.Phase10Render.RenderDeterminismPair \
  -logFile p10-determinism.log
# report: docs/benchmarks/captures/p10-gate/p10-determinism.json

# seek latency on the real 1440p media: stage it for playback, then
cp app/RenderOutput/p10/garnett-road-to-angle.full.mp4 \
   app/RenderOutput/p10/garnett-road-to-angle.proxy.mp4 \
   app/Assets/StreamingAssets/SoldierView/
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -runTests -testPlatform PlayMode \
  -testResults playmode-results.xml -logFile p10-playmode.log
# report: app/p10-seek-latency.json
```

## 7. Release packaging

Generated media ships via GitHub Releases (plan §10.1), never Git
history. `docs/benchmarks/captures/p10-gate/p10-release-manifest.json`
carries names, sizes, sha256, and provenance. The owner publishes at
Phase 12 with:

```sh
gh release create soldier-view-media-v1 \
  app/RenderOutput/p10/garnett-road-to-angle.full.mp4 \
  app/RenderOutput/p10/garnett-road-to-angle.proxy.mp4 \
  docs/benchmarks/captures/p10-gate/p10-release-manifest.json \
  --title "Soldier View media v1 — garnett-road-to-angle" \
  --notes-file docs/benchmarks/captures/p10-gate/p10-release-notes.md
```

## Cost controls (plan §3.5)

Measured Phase 10 actuals live in the gate evidence. If a future
re-render exceeds seven days or 500 GB scratch, reduce cost strictly in
the plan's §3.5 order (shorten viewpoint → fewer samples → 1080p →
simpler hero materials → thinner distant crowd) — never by restoring
terrain exaggeration, cutting historical smoke, or changing timing.
