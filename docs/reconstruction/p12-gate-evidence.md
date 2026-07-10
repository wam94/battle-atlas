# Phase 12 — executor evidence (review-and-release, buildable subset)

**Phase:** 12 — review and release (plan §12).
**Branch:** `v2-phase12` (from `main` = 6c86c6b).
**The final gate is the owner's** — historical review, three-enthusiast
review, base-M1 test, and publishing the release are packaged in
`docs/reconstruction/p12-owner-checklist.md` (copy in the evidence dir).
This document is the executor's evidence for the technical items.
Evidence dir: `docs/benchmarks/captures/p12-gate/` (gitignored generated
artifacts; staged on the `v2-phase12` worktree and copied to the main
checkout's same path). Machine: Apple M4, 24 GB, macOS 26.5.2, Unity
6000.4.11f1.

## 1. The assigned P11 punchlist fix: the Soldier View speed designator

Entering Soldier View forces 1× (pre-rendered media is real time); the
Atlas speed group is hidden inside, so the forced state was illegible.
The sv-bar now carries a locked designator styled as the active member
of the Atlas speed-button family: **"1× real time"**, becoming
**"1× real time — paused"** while paused. `HudModel.SoldierViewSpeedLabel`
(EditMode-pinned); element pinned in `AtlasUiAssetTests`. Visible in
`p12-sv-playing.png`. Punchlist entry marked shipped.

## 2. Accessibility basics (§12 P12)

- **Captions, default OFF** — `generate_captions.py` derives time-coded
  captions for the mix's two voice layers (shouted orders, wounded
  voices) from the SAME deterministic event export and hash gates the
  audio build uses (imported, not copied): 58 captions (3 orders / 55
  wounded), bracketed non-speech wording only (§9.2 — the audio carries
  no worded dialogue, so captions may not invent any). Committed as
  `StreamingAssets/SoldierView/captions.json` with a staleness test;
  rendered by `CaptionTrack` (newest-first stacking, 2-line cap) in a
  high-contrast band; toggled by the CC button in the sv-bar or the
  Options modal.
- **Volume controls** — Options modal: master + Soldier View mix
  (persisted; `AccessibilityOptions`). Master rides
  `AudioListener.volume`; the media's authored AAC mix plays via the
  VideoPlayer DIRECT path (which bypasses the listener) at master × mix.
  Two defects fixed on the way: the mix had never actually played
  in-app (`audioOutputMode = None` since P1 — the owner's praised P10
  sound was the encode listened to externally), and the Atlas synth
  soundscape used to keep playing UNDER Soldier View (same events at
  macro grain — doubled battle). Now: direct audio at the composed
  gain, muted during seek settle (no audible scrubbing), and the Atlas
  field ducks to zero through its anti-click slew while inside.
- **Reduced motion** — persisted setting selecting
  `HeroMotionProfile.ReducedMotion` (no roll, no handheld, minimal
  bob/sway). Implemented as the **Atlas-side setting plus a documented
  requires-re-render media cut**: the shipped media was rendered with
  the standard profile, the HUD says so while the setting is on (sv-bar
  honesty note + Options note), and the render runbook documents the
  reduced-motion render (`GateP9Render.Settings` is the seam; distinct
  output names; audio mix reusable).
- **Readable text** — 12px floor across the HUD stylesheet (was 11px in
  seven styles), dimmest grays lifted (conditions line, silent-source
  italics, observer line, modal notes); pinned by
  `StyleSheet_HonorsTheReadableTextFloor`.

## 3. License verification (§12 P12 + the P11 TMP flag)

- Manifest: 36 assets validate (`validate_assets.py`); attribution
  (`THIRD_PARTY_ASSETS.md`) and `credits.json` regenerate clean; audio
  pack lock + per-source archived license evidence green.
- **TMP/UCL carve-out resolved** (was flagged at P11): verified against
  the Unity Companion License v1.2 text (archived, sha256-pinned, in
  `docs/assets/licenses/unity-tmp-essentials/` — Unity's license pages
  block automated archival, so the verbatim republished text plus the
  shipping `com.unity.ugui` 2.0.0 notice are the evidence). The UCL
  grant expressly includes distribution, conditioned on Unity-dependent
  use (§1) and notices (§5). Resolution: keep the dependency, carve it
  out of MIT — in-tree notice `app/Assets/TextMesh Pro/LICENSE.md`
  (satisfying UCL §5, previously absent), analysis record
  `docs/assets/tmp-unity-companion-license.md`, README statement,
  LiberationSans OFL text confirmed committed beside the font. Pinned by
  `test_tmp_license.py` (4 tests).

## 4. Golden frames (§13) — visual-review reference set

`Phase12Review.RenderGoldenFrames`: the five §13 times (t=8160 road
crossing, 8400 canister, 8580 wall approach, 8700 Angle crisis, 8820
collapse/repulse) at BOTH review resolutions (2560×1440 and 1920×1080),
offline HDRP profile, exact production staging (first person, lens
guard, observer slot hidden). Output: `p12-gate/golden/` + a manifest
carrying git SHA, settings hash (**matches the P10 production freeze:
`618d72ce…`**), bundle checksum, and per-file sha256 — the §13
side-by-side review contract. Spot-checked against the accepted P9/P10
look (formation, smoke bank, bodies, kit silhouettes).

## 5. Standalone playback verification on this machine (§12 P12)

`scripts/p12-playback.sh` → Development macOS standalone (2.3 GB with
media) → `P12PlaybackProbe` drives the real product loop: Atlas playback
at 60×, HUD entry at t=8400 on the production 1440p media (content
warning included), 10 s playback, a six-target seek battery, 5 s
playback after seeking, exit. Report: `p12-gate/p12-playback.json`;
screenshots `p12-atlas.png`, `p12-sv-playing.png`,
`p12-sv-after-seek.png`, `p12-atlas-return.png`.

- **PASS** (final run, display active): Atlas playback at 60× —
  **59.7 avg fps** (p95 17.2 ms, worst 21.7 ms); Soldier View playing
  the 1440p production media — **59.7 avg fps** (p95 17.3 ms); after
  the seek battery — 59.8 avg fps. Memory steady at ~315 MB allocated /
  ~432 MB reserved through the whole loop (playback is light; the
  ~1.2 GB P10 figure was the offline renderer). Entry on full media (no
  proxy fallback); exit restored the exact battle second. The speed
  designator ("1× real time") and the CC button are visible in
  `p12-sv-playing.png`.
- **Seeks:** five of six targets settled in **50–67 ms** and held
  within one video frame of the clock (the corrected steady state — the
  P10/P11 contract). One target — the backward sub-second nudge
  8615→8610.4 — missed its event-side settle, rode the player's
  designed 3 s wedge-escape, then converged to −0.012 frames. Sync was
  never wrong on the held frame, but a ~3 s hold on a nudge is a real
  feel item: recorded as joining the punchlist's seek-feel /
  proxy-frame-transition question (the P1 plumbing remains available).
  Decoder-only comparison on this machine, same phase: PlayMode
  `SeekLatency_Full1440pProductionMedia` median **62.4 ms** / worst
  **93.8 ms** over its 12-seek battery (`app/p10-seek-latency.json`).
- **Two earlier probe runs were occlusion-throttled** (unattended
  display: macOS holds an invisible window near ~3 fps and
  frame-quantizes settle detection) — kept in mind for the base-M1 run:
  execute `scripts/p12-playback.sh` with the display awake.
- **Fresh-checkout defect found and fixed by this test:** the committed
  Atlas scene's terrain reference dangles until the README import step
  runs inside the scene and the scene is saved; the first probe build
  rendered Atlas with no macro ground ("Terrain has no valid
  TerrainData!"). `Phase12Review.PrepareStandaloneScene` is the headless
  equivalent, now part of `p12-playback.sh`.
- **Lowest tested configuration (plan fallback wording): Apple M4,
  24 GB.** The base-M1 8 GB run remains an owner item
  (checklist item 3).

## 6. README + docs final pass

README rewritten for the shipped product: the three checkout layers
(source / required generated terrain / optional media downloads, with
the `soldier-view-media-v1` release referenced as forthcoming and
checksum verification against the committed release manifest), the
licensing carve-outs, a docs index (plan, runbook, owner checklist,
punchlist, editorial decisions, gate evidence, attribution). Render
runbook gained the reduced-motion-cut section; the release-publish
command was corrected to durable artifact paths (the P10 manifest's
command pointed into the disposable phase-10 worktree) and re-verified:
gate-evidence media sha256s match the release manifest
(`50c4725e…` full / `57e164bd…` proxy).

## Suites (this machine, worktree)

| Suite | P11 close | P12 |
| --- | --- | --- |
| pipeline (pytest) | 59 | **59** |
| tool (vitest) | 108 | **108** |
| reconstruction (pytest) | 90 | **99** (+5 captions, +4 TMP license) |
| Unity EditMode | 347 = 343 + 4 skips | **357 = 356 passed + 1 skip*** (+10 new) |
| Unity PlayMode | 16 | **16** (all passing, media staged; audio path enabled) |

\* the four Angle bake-fixture skips convert to passes with the
regenerated gitignored inputs; the one remaining skip
(generated-terrain material) passes once the README/`PrepareStandaloneScene`
import has run.

## Remaining for the owner (the final gate)

`docs/reconstruction/p12-owner-checklist.md`: (1) historical review
against the claims corpus, (2) three uninvolved enthusiasts + the §14
comprehension test, (3) base-M1 8 GB playback (or accept M4/24 GB as
documented lowest), (4) `gh release create soldier-view-media-v1 …`
(verified command in the checklist) and the README release-URL swap.
