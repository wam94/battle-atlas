# Phase 1 — the media contract, proven

**Date:** 2026-07-08
**Plan:** `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` §12 Phase 1
**Branch:** `v2-phase01`

Phase 1 proves the architectural consequence of pre-rendering — exact
battle-clock/video synchronization through a fixed Soldier View video —
before any art, data migration, or HDRP work.

## What exists now

| Piece | Where |
| --- | --- |
| Viewpoint schema | `reconstruction/schemas/viewpoint.schema.json` |
| Validator + tests (10) | `reconstruction/scripts/validate_viewpoints.py`, `reconstruction/tests/` — `cd reconstruction && uv run pytest -q` |
| Dev viewpoint (`dev-timecode`, t=8160..8170) | `app/Assets/StreamingAssets/SoldierView/viewpoints.json` |
| Pinned proxy generator | `reconstruction/scripts/generate_dev_proxy.sh` (media gitignored) |
| Player runtime | `app/Assets/Scripts/SoldierView/` (`SoldierViewMath`, `ViewpointDefinition`, `SoldierViewPlayer`, `SoldierViewHud`, `SoldierViewBootstrap`) |
| EditMode tests (11 new; suite 130) | `app/Assets/Tests/Editor/SoldierViewMathTests.cs`, `ViewpointDefinitionTests.cs` |
| PlayMode suite (first in project; 10 tests) | `app/Assets/Tests/PlayMode/SoldierViewPlayerSyncTests.cs` |

## The pinned proxy command

`generate_dev_proxy.sh` renders 10 s of `testsrc2` at 1280×720/30 fps with a
`drawtext` burn-in of the battle second, frame-within-second, and absolute
frame number; libx264 CRF 18, keyframe every 15 frames (0.5 s GOP, twice the
plan's 1 s floor), `+faststart`. ffmpeg resolution: system binary if present,
else the static **ffmpeg 7.1** bundled with the `imageio-ffmpeg` dev
dependency of `reconstruction/` (no system ffmpeg existed on the dev machine
at Phase 1; none was installed). Output goes to StreamingAssets and is
gitignored; a sha256 is printed at generation time.

## Synchronization model (implemented and tested)

```
videoTime       = battleClockTime - viewpoint.t0
battleClockTime = viewpoint.t0 + videoTime
```

- The `BattleClock` is the only source of historical time. Entering forces
  1× speed (pre-rendered media is real time); exit restores the prior speed
  and leaves the clock exactly where it was.
- Enterable window is `[t0, t1)`; entering outside it is refused.
- Seeks move the clock first, then settle the decoder. A seek is settled
  only when the presented frame is within half a frame of the target.
- Missing full media falls back to proxy with a warning; missing proxy
  refuses entry with instructions.

## Decoder findings (macOS / AVFoundation, Unity 6000.4.11f1)

Discovered by the PlayMode suite; all handled in `SoldierViewPlayer` and
relevant to the Phase 10 encode design:

1. **`seekCompleted` fires before frame delivery.** Callbacks are hints;
   settle requires the presented frame to match the target.
2. **Frame-boundary requests are ambiguous.** A seek to an exact frame PTS
   lands on the *previous* frame. All seeks target the center of the
   requested frame.
3. **The final ~3 frames of a stream are unreachable seek targets** (the
   decoder settles on an earlier frame forever). `EndGuardFrames = 4`: the
   last four frames are not seek/hold targets, so the effective playable
   window ends `4/fps` before `t1`. **Phase 10 must render each viewpoint
   with at least ~0.5 s of padding past `t1`** so the guard is invisible.
4. **Batchmode game time does not track wall time** (observed ~9000 fps with
   `deltaTime` sums far below real time), so CI asserts sync invariants,
   not real-time pacing; pacing feel is owner-checklist material.

## Seek-latency measurement (Gate P1 input)

Apple M4 / 24 GB / macOS 26.5.1, 720p30 proxy, 0.5 s GOP, 12 deterministic
seeks mixing far jumps, near jumps, and sub-second nudges in both
directions (PlayMode `SeekLatency_MeasuredAcrossWindow`, report written to
`app/p1-seek-latency.json`):

- **median 10.2 ms, worst 19.0 ms** — all settles under one video frame
  (33 ms) and every settle landed within one frame of the battle clock.

Caveat: this machine is far above the base-M1 floor and the proxy is 720p.
Re-measure on the lowest target hardware and with 1440p full media in
Phase 10 before locking bitrate/GOP.

## Proxy/full transition decision

**Chosen: seek-and-hold.** During a seek the player holds the last
presented frame until the decoder settles (no proxy flash, no transition
overlay), because measured settle latency (median 10 ms, worst 19 ms) is
below one frame time — a proxy-frame crossfade would itself be more visible
than the latency it hides. The dev HUD shows a `[settling]` badge for
diagnostics only.

Revisit trigger: if 1440p full-media settle latency on the lowest target
Mac exceeds ~3 frames (100 ms), switch to the plan's proxy-frame
seek-settle transition (show the 720p proxy frame immediately, settle the
full stream behind it). The player's proxy-fallback path is already the
foundation for that.

## Known Phase 1 limitations (by design)

- The dev HUD is IMGUI; production UI is Phase 11.
- No content warning yet (synthetic timecode media only); required before
  any real Soldier View media ships (plan §9.2).
- `viewpoints.json` lives in `StreamingAssets` (Unity's runtime-readable
  channel) rather than `Assets/Battle/Angle/`; plan §5 allows
  Unity-convention placement. The reconstruction suite validates the
  committed file directly.
- Entering at a battle time inside the window but past the end-guard
  (final 4 frames) snaps the clock back by up to `4/fps` seconds.
