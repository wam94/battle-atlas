# Gate P9 evidence — hero viewpoint and audio (plan §12 Phase 9)

**Gate criterion:** "an enthusiast reviewer understands where they are,
what their unit is doing, what threatens it, and that the observer is
representative rather than an identified person." The owner is the
reviewer; this document is the viewing guide.

**One decision is yours at this gate (plan §3.4):** first-person versus
very tight close-third-person. Both 30-second candidates below are the
SAME thirty seconds of the same reconstruction from the same formation
slot — only the camera rig differs. Everything downstream is
camera-style-agnostic (`viewKind` in viewpoints.json flips it).

Evidence (2560×1440, offline HDRP profile, per-frame ephemeris sun):
`docs/benchmarks/captures/p9-gate/` — staged on the `v2-phase9` worktree
and copied to the main checkout's same path (gitignored generated media).

- `p9-candidate-fp-30s-1440p.mp4` — first-person, t=8378..8408
- `p9-candidate-c3p-30s-1440p.mp4` — close third person, same window
- `p9-proof-60s-av-fp-1440p.mp4` (+`-720p`) — the 60 s mixed
  audio-visual proof at the wall, t=8610..8670, labeled FIRST-PERSON
  (the executor's recommended style; your candidate choice is not
  prejudiced — the proof exists to judge the soundscape and the climax
  camera behavior, both identical under either style)
- `p9-still-8165-advance-fp.png` … `p9-still-8760-repulse-fp.png` — the
  route in stills (the advance, both fence climbs at the road, mid-field,
  canister at the wall, the repulse turn), plus `-c3p` framings of two of
  them and a ReducedMotion comparison frame
- `p9-gate-report.json` — machine evidence: bitwise scrub probes over
  logical soldier state AND camera pose, pixel tolerances, timing
- `stems/` — the nine audio stems + `mix.wav` + `stems.json` (per-stem
  file gains) for the proof window; the full-viewpoint stem set
  (`stems-full/`, 1.1 GB) is regenerated in ~9 s by the same script and
  stays on the render machine — the Phase 10 mix is script-generated
  from stems either way
- `p9-audio-events.json` — the exported event streams the stems are
  built from (regenerable: `GateP9Render.ExportAudioEvents`)

## What you are watching (the 30 s candidates, t=8378..8408)

You stand in formation slot 881 of Garnett's Virginia brigade — REAR
rank of file 184, roughly 82 m left of the brigade's colors, so your
file leader and the whole front rank read 1.3 m ahead of the camera
(the §6.5 sketch's front-rank slot 184 was tried first: a forward-facing
first-person camera in the FRONT rank shows an empty road — the
documented slot deviation). The flank trails the
colors, so YOUR road crossing comes ~50 s after the brigade's nominal
crossing window: the line reaches the west rail fence (climb at t≈8385),
crosses the packed road surface, climbs the east fence (t≈8404), and
falls back into the disordered advance toward the wall, 250 m upslope.
Neighbors climb where THEIR paths cross the traced rails. Shell and
canister fire is already taking men; the Union line ahead is firing.
(Finding this take exposed two real resolver defects — the about-face
point-reflection and the trailing-flank crossing gap — both fixed and
regression-tested; see the commits.)

Judge, per the gate: do you know where you are (the road corridor, the
fences, Codori's buildings behind, the crest ahead), what your unit is
doing (halt → climb → redress → advance), and what threatens it
(artillery strikes, the distant line's musketry)?

## First-person versus close-third-person: what to compare

| | first-person | close third person |
| --- | --- | --- |
| eye | 1.66 m, §6.5 camera | +0.35 m, 1.35 m behind, 0.42 m right of the same head |
| the observer | invisible (his slot is real; his figure is hidden from HIS OWN camera only) | visible: back, kit, musket |
| drill readability | ranks/muskets at true eye height; the fence arrives at your chest | slightly elevated, more context, observer's back occludes frame center-left |
| gait bob | full authored amplitudes × stabilization 0.35 | 0.45× (rig damping) |

**Executor's recommendation: first_person**, for the gate's own
criterion. (1) The representative-observer stance reads strongest when
the camera is "no one's and everyone's eyes"; C3P materializes one
specific man — his height, his kit variant — as a protagonist, which is
an identity claim ED-22 exists to avoid. (2) Over-shoulder framing is a
gameplay idiom; against the project's documentary stance it reads as an
avatar you cannot control (the §3.3 no-rotation contract feels like a
bug in C3P, like a choice in FP). (3) C3P's own-body occlusion sits
exactly on the threat axis at the wall. C3P's genuine advantages —
~half the perceived bob, and you can SEE that the observer carries a
musket he never fires (ED-23) — are noted honestly; if motion comfort
weighs heaviest for you, C3P or FP+ReducedMotion both answer it.

## The 60 s proof (t=8610..8670, canister at the wall)

Garnett's line stalled at the wall under canister and musketry
(`take_canister`: the compiled reconstruction has this brigade NOT
returning fire in-window — you take it, you don't give it; ED-23).
Kemper wavers off to the left; Armistead's column presses toward the
wall right of you; men of your file fall around the camera.

Camera behavior to judge: gait bob is gone (the line is halted); nearby
canister strikes duck the camera and jolt pitch; men falling within
arm's reach pull a brief head-turn; the handheld unsteadiness rises with
the local chaos level (a pure function of nearby scheduled strikes and
falls). Amplitudes are deliberately conservative (§12 P9 motion-comfort
rule) — this should read as "unsteady man", never "action camera".

Audio layers to judge (all §9.3, every event stream shared with the
visuals, ED-23):

1. **Musketry** — every report is one resolver-confirmed Union discharge,
   delayed by its true distance at 343 m/s, attenuated and low-passed
   with range, panned against the camera's own heading. Mass fire IS the
   rolling volley — nothing is looped.
2. **Artillery** — Cushing's remaining pieces close on the right;
   Cowan/Arnold farther out; distant-variant samples past 420 m.
3. **Strikes** — canister/shell earth impacts near the unit (the same
   compiled impact points that dust the ground and duck the camera).
4. **Projectile pass-bys** — a deterministic minority of the fire aimed
   at your line whizzes past.
5. **The wounded** — sparse, sober groans tied to scheduled nearby
   casualties; the crawling minority voice again, later and quieter.
6. **Unit noise** — shout bursts at segment transitions and a low mob
   rumble scaled by the chaos level (generic, unattributed voices —
   no invented dialogue).
7. **Near-camera life** — exertion breathing (scaled by gait + chaos),
   the observer's own footfalls and his file's around him.
8. **The ground truth layer** — July meadow insects, light wind.

## The observer policy (ED-22) — what the gate asks you to confirm

The viewpoint's entry text (committed at
`app/Assets/StreamingAssets/SoldierView/content-warning.json`, authored
in `docs/reconstruction/soldier-view-content-warning.md`) must leave a
reviewer knowing the observer is REPRESENTATIVE: an unnamed man in a
real unit's real position, not an identified witness, whose survival
through the window is a disclosed editorial exemption (his slot is
excluded from the casualty draw; the unit's totals are unchanged).

## Honest limitations (known; judge the gate, not these)

- **Sources are Freesound preview transcodes** (128 kbit/s MP3): the
  originals require an account login (recorded, like Phase 2's Sketchfab
  note). Proof-grade; Phase 10 can substitute logged-in downloads under
  the same manifest ids without touching the pipeline.
- **No friendly musketry/reload foley** — Garnett's brigade has no
  compiled fire segment in this window (ED-23). If enthusiast review
  reads this as a defect, it is a reconstruction question (does the
  brigade fire at the wall?), not an audio one.
- **No worded period drill commands** ("Forward — march!") — no CC0/CC-BY
  recording was found; segment transitions carry generic shout bursts
  instead. Deferred; an owner-recorded command set would be
  project-owned and free of this limit.
- **No equipment jingle layer** (canteens/buckles) — no clean CC0 source
  found; movement reads through footfalls and cloth crunch. Deferred.
- **No pre-slice bombardment residue**, visual or audible (P8
  limitation, applies equally here).
- **The approach is artillery-silent**: the shell fire that causes
  Garnett's early casualties comes from batteries OUTSIDE the staged
  Angle bundle (macro units), so no gun in the compiled slice fires
  before the wall batteries open (~t=8400 audible) — the first minutes
  carry marching, ambience, and the enemy line's first musketry only.
  Flagged for Phase 10: an authored distant-artillery layer would need
  its own sourced event basis (same rule as the pre-slice smoke).
- **Flank translation**: the observer file rides Garnett's left flank (~82 m from
  the colors), so compiled line wheels translate the camera laterally —
  measured ~6.8 m/s for ~2 s at the east-fence redress and ~8.6 m/s for
  ~2 s as the repulse about-face at t≈8700 swings the formation frame
  (the P8 evidence's known fall-back facing note). The whole file moves
  together, so relative on-screen motion stays small. Retiming compiled
  tracks/facing is out of scope; flagged for Phase 10 polish.
- **First-person shows no own body/musket** — the kit has no
  first-person arms rig; at eye height with stabilization the absence
  reads as camera, not as an unarmed man. C3P shows the full figure.
- Seek-window audio behavior (scrub-while-inside-viewpoint) is a Phase
  10/11 media concern: stems are clock-locked, but the encoded proof
  pads nothing past t1 — the P1 media contract's ~0.5 s padding past
  `t1` remains owed to Phase 10.

## Motion comfort and the accessibility variant

All camera life is authored constants in `HeroMotionProfile`
(`HeroViewpointCamera.cs`), scaled down by the §6.5 stabilization
(0.35). `ReducedMotion` is the Phase 12 accessibility cut: bob 8 mm,
no roll channel at all, no handheld noise, halved event responses —
same path, same events, same determinism (tested:
`ReducedMotion_IsCalmerThanStandard`, `FullStabilization_RemovesGaitAndHandheld`).
`p9-still-8620-canister-fp-reduced.png` shows it framing-identical.

## Determinism evidence (the machine half)

`p9-gate-report.json`: at probe frames the renderer hashes the full
9,405-slot logical state vector AND the 10-float camera pose, scrubs
away, re-poses out of order, and re-hashes — digests must be bitwise
identical; re-rendered pixels stay inside the documented Phase 8 GPU
tolerance (12/8%). The stem builder is byte-deterministic
(`test_audio.py::test_build_is_byte_deterministic`), and the committed
clip pack is pinned by `reconstruction/audio/freesound-pack.lock.json`.

## Regeneration (all deterministic)

```sh
cd pipeline && uv run python -m terrain_pipeline.cli crop
cd pipeline && uv run python -m terrain_pipeline.cli environment
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.GateP9Render.ExportAudioEvents -logFile p9x.log
"$UNITY" ... GateP9Render.RenderCandidates ... 
"$UNITY" ... GateP9Render.RenderProof ...
"$UNITY" ... GateP9Render.RenderStills ...
cd reconstruction && uv run python scripts/build_viewpoint_audio.py \
  --events ../docs/benchmarks/captures/p9-gate/p9-audio-events.json \
  --out ../docs/benchmarks/captures/p9-gate/stems --t0 8610 --t1 8670
scripts/p9-encode.sh
```

## Suite state at evidence time

- pipeline **59**
- reconstruction **89** (79 + 10 Phase 9 audio)
- tool **108**
- Unity EditMode **313** = 311 passed + 2 known conditional skips
  (15 Phase 9 tests: hero camera comfort/determinism, observer policy,
  formation-frame regressions)
- Unity PlayMode **10/10** (sync-flake single-re-run policy)

## Measured results (p9-gate-report.json + encode log)

- Proof: 1800/1800 frames at 0.33 s/frame offline HDRP 2560×1440,
  1.19 GB peak managed memory. Candidates: 2×900 frames at ~0.25 s/frame.
- Scrub probes (frames 300/1500, re-posed OUT OF ORDER after scrubbing
  away): 9,405-slot logical digests AND the 10-float camera-pose digest
  **bitwise identical, 2/2**; re-rendered pixels within the documented
  GPU tolerance 2/2 (worst 7.72% differing at max channel delta 12 —
  the smoke-heaviest window of the slice, at the Phase 8 envelope).
- Full-viewpoint audio events: 55,439 resolver-confirmed musket
  discharges, 377 cannon discharges, 377 strikes, 1,350 audible-range
  casualties, 641 observer footfalls, 78 observer/neighbor fence
  crossings; 60 s stems build in ~2 s, the full 11-minute stem set in
  ~9 s (byte-deterministic, tested).
- Media: `p9-candidate-fp-30s-1440p.mp4` 89 MB,
  `p9-candidate-c3p-30s-1440p.mp4` 78 MB,
  `p9-proof-60s-av-fp-1440p.mp4` 146 MB (19.3 Mbit/s + 193 kbit/s AAC,
  exactly 60.00 s) + 720p proxy; H.264, 1 keyframe/s GOP; sha256 in
  `p9-gate-media.sha256`; frame-continuity enforced before encode
  (`scripts/p9-encode.sh`).

## Gate P9 verdict

Machine criteria: camera pose and logical state bitwise-deterministic
under scrub; casualty totals unchanged by the observer exemption
(tested); every audio source license-verified CC0 with archived
evidence. Owner judgment on the candidates, the proof, and the
representative-observer text closes the gate.


## Gate P9 verdict

**PASSED — closed by the project owner 2026-07-10.** Camera style chosen:
**first_person**. Comprehension confirmed in the owner's own words: "this
is a soldier approaching the firing line, which is clear and sort of not
something people would find obvious" — the gate's core test. Audio and
smoke explicitly praised. Deferred (owner: "not terribly concerned right
now, but we'll want to fix at some point"): crowd-figure messiness at
close range (interpenetration/pose overlap in the packed line).
**MUST-FIX carried to Phase 10, before the production render:** a visible
teleport at ~25 s into `p9-proof-60s-av-fp-1440p.mp4` (battle time
~t=8635) — root-cause (camera slot path, neighbor placement, or tier
hand-off), fix, and re-verify that window before committing render hours.
