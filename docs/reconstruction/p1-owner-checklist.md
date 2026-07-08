# Gate P1 — owner verification checklist

**Gate (plan §12 Phase 1):** a tester can scrub Atlas time, enter the proxy,
play/pause/seek, exit, and return to the same battle second without drift
greater than one video frame after settle.

The executor's self-verification (PlayMode suite, 10/10 green headlessly)
does **not** close this gate — this session does. Budget: ~10 minutes.

## Setup (one command)

```bash
./scripts/p1-demo.sh
```

(Generates the gitignored timecode proxy if missing, validates
`viewpoints.json`, opens the Unity editor.) Then open
`Assets/Scenes/Atlas.unity` — **the scene was renamed from
`SampleScene.unity` in Phase 0** — and press Play.

## Checks

Every drift observation uses the video's burned-in text
(`BATTLE t=<second>s +<frame>f`) against the HUD battle clock. One frame =
33 ms. **Any post-settle disagreement beyond one frame fails the gate.**

1. **Entry marker windowing.** Scrub the Atlas timeline toward 15:16
   (t=8160). Outside t=8160..8170 there is no Soldier View button; inside
   it, "Enter Soldier View: Dev: timecode proxy" appears top-right.
2. **Enter.** Click it mid-window (~t=8165). The timecode video appears;
   its burned-in battle second matches the HUD clock (±1 frame). Speed
   drops to 1× (pre-rendered media is real time).
3. **Play.** Burned-in time, frame counter, and HUD clock advance
   together, one battle-second per real second. The moving test pattern
   never freezes while the clock runs.
4. **Pause.** Both stop together; nothing creeps while paused.
5. **Seek (the heart of the gate).** Drag the bottom slider to at least
   five targets: far forward, far back, and small nudges. After each
   settle (the `[settling]` badge clears — measured median 10 ms, so it
   should be imperceptible): burned-in second == HUD clock ±1 frame; the
   HUD drift readout stays within ±1.0 frames; note the seek-latency ms
   readout — flag anything beyond ~50 ms.
6. **Window end.** Seek to the far right of the slider. The clock holds
   ~0.13 s before 8170 (documented decoder end-guard,
   `docs/reconstruction/p1-media-contract.md`) and playback pauses there
   rather than wedging or looping.
7. **Exit.** Note the battle second, click Exit: Atlas returns at exactly
   that second, speed restored to 60×, scene renders normally.
8. **Re-entry boundary.** At t=8170 or later the button is gone.
9. **Missing-media fallback (optional).** Stop Play mode, delete
   `app/Assets/StreamingAssets/SoldierView/dev-timecode.proxy.mp4`, press
   Play, scrub into the window, click Enter: refusal plus a console
   warning naming `generate_dev_proxy.sh`. Regenerate afterwards (step 0).

## Phase 0 items packaged into this session (headless blockers)

- **Real-time pacing feel** (batchmode game time does not track wall time,
  so CI could not verify it): step 3 above covers it.
- **Editor-quality screenshots/FPS.** Headless captures + FPS/memory came
  from a Development standalone build (`scripts/p0-benchmark.sh`, results
  in `docs/benchmarks/`); if you want editor-grade numbers or different
  camera angles, capture them during this session. Dev-build overhead
  means standalone FPS understates release performance slightly.
- Reminder of pre-existing conditions NOT part of this gate: Phase 6
  smoke/audio remains owner-unverified (bombardment-peak perf case, muzzle
  bloom, pause silence) — see `docs/benchmarks/2026-07-08-v2-phase0-baseline.md`.

## Verdict

- [ ] Gate P1 PASSES (all of 1–8 clean) — Phase 2 may start.
- [ ] Gate P1 FAILS — file what broke (step number, battle time, drift,
      console output) against branch `v2-phase01`.
