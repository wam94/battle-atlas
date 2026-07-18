# Iverson production render — `iverson-forney-field` (second Soldier View film)

## RESUME STATE — CLOSED (slice complete 2026-07-18)

Everything below was executed; the quiet-machine PlayMode re-run passed
(23+1 of 24, zero failures) and the contract seek numbers are in §8.
Retained for the record of the interrupted-and-resumed run.

## RESUME STATE (checkpoint 2026-07-17, owner-ordered pause)

**DONE (all evidence committed/pushed on `iverson-production`):**
ED-21 seed re-pin + tests; cross-phase entry wiring + per-viewpoint
content warning + PlayMode coverage; terrain inputs regenerated and
byte-verified; preflight PASS; determinism pair PASS; **the full
production render COMPLETE** (36,315/36,315 frames, 21/21 chunks
rendered + harvested, zero lost frames); audio events + stems + mix;
full + proxy encodes (sha256 in §7) with decoded-count verification;
release manifest/notes; five production stills; EditMode suite 454+1
of 455 PASS; tool 119 / pipeline 66 / reconstruction 162+1 PASS.
Media staged in `app/Assets/StreamingAssets/SoldierView/` and kept in
`app/RenderOutput/iverson/` (both gitignored). NO render/harvest/encode
process of this slice is running or needed — the media pipeline is
finished.

**IN FLIGHT / REMAINING (exact next commands):**
1. **Quiet-machine PlayMode re-run** (the only open verification). The
   first full run (`playmode-results.xml`, kept in the worktree) ran
   while another executor's production render saturated the machine:
   21/24 passed incl. ALL new Iverson tests; the two failures are the
   PRE-EXISTING dev-proxy sync tests `SoldierViewPlayerSyncTests.
   {Seek_OutsideWindow_ClampsToDecodableRange, SeekLatency_MeasuredAcrossWindow}`
   (drift/timeout under load — load-flaky, not code), and the Iverson
   seek medians are contended (median 218.8 ms / worst 365 ms — NOT
   contract numbers). When `pgrep -f "MacOS/Unity.*batchmode"` is
   clear:
   ```sh
   cd <worktree> && "$UNITY" -batchmode -projectPath app \
     -buildTarget OSXUniversal -runTests -testPlatform PlayMode \
     -testResults playmode-results.xml -logFile iverson-playmode2.log
   # then: app/iverson-seek-latency.json carries the contract numbers;
   # update §8/§10 and the suite table with the quiet-run results
   ```
2. Fill §10 (suites final) + §11 (film-safety verdict — the byte checks
   are already recorded in the git history of this report's commits)
   and §12 (stills index); final push. No merge (owner instruction).

**Branch head at checkpoint:** see `git log` — every artifact above is
already pushed; nothing uncommitted remains in the worktree beyond
run logs (`iverson-*.log`, `*-results.xml`, kept untracked by design).

---

**Status:** executor evidence for the `iverson-production` slice.
**Authorization:** the design gate PASSED (owner: 12th NC observer,
first person, window t=5830..7040, smoke AS-IS) and the named blocker
(`fight_prone` vocabulary) merged — `iverson-viewpoint-design.md` §8.5's
production path runs: the ED-21 seed re-pin plus the P10 pattern.
**Branch:** `iverson-production`. Owner publishes the media release;
this slice never publishes.

---

## 1. The ED-21 production seed pin

`compile_iverson.py`'s `STAGING_SEED` moved from the gate-stage proof
pin (`iverson-proof-seed/1`) to the **checksum of the bundle the owner
reviewed at the gate**: `2f15dd2f4e5e399e9899de45e5606a5610309dfad503ff472edf5e2edb09bf43`
(the fight-prone merge's reviewed bundle) — the Angle ED-21 convention.
The bundle was regenerated deterministically; **only the `stagingSeed`
and `checksum` fields changed** (every other top-level key verified
byte-identical; new bundle checksum `ebb9e3b6095f…`). Enforcement, like
the Angle's `d470c469…` pin:

- reconstruction: `test_staging_seed_is_the_ed21_production_pin`
- EditMode: `IversonBundleTests.Bundle_StagingSeed_IsPinnedAtTheReviewedChecksum`
- render time: every `IversonProductionRender` entry point hard-fails
  on any other seed (`AssertPin`)

Consequence (inherent in the re-pin the design doc ordered): the
hash-drawn choreography (victim draws, stagger phases, yaw) re-rolled
once, from the proof seed to the production seed; the preflight and
determinism gates below validate the production draws. Future
provenance-only recompiles can never re-roll the shipped film again.

## 2. Harness

`IversonProductionRender` (app/Assets/Editor/IversonProduction/) — the
P10 pattern site-parameterized per the design doc: staging is
`IversonGateRender.Boot` (Oak Ridge crop + Iverson bundle, offline HDRP
profile), the frame loop / chunk manifests / tolerances are
`Phase10Render`'s own members. Window t=5830..7040 + 0.5 s media-
contract pad = **36,315 frames** at 2560×1440/30 fps in **21 resumable
60 s chunks**. Scripts: `iverson-render-loop.sh` (SIGKILL-resilient
runner), `iverson-chunk-harvester.sh` (the full PNG scratch is ~116 GB
against ~44 GB free — the rolling harvester is mandatory here),
`iverson-encode.sh` (pinned libx264 slow CRF 18, GOP 30, +faststart,
AAC 192k mix), `iverson-release-manifest.py`.

## 3. Preflight (no-teleport gate) — PASS

`iverson-preflight.json`: the whole padded window t=5830..7040.5,
every slot of every unit — **43,441,256** coarse pairs at 10 Hz,
**0 suspect windows** (nothing even reached the refinement pass),
0 crossing-exit hand-offs (no fences on this site), camera swept at
full render rate over all 36,315 frames with **max 0.0392 m/frame**
against the 0.150 bound. 59 s sweep. The p10-teleport-postmortem
defect classes (lens pass-through, formation-wheel steps, crossing-
chain pops) are provably absent from this geometry under the
production seed.

## 4. Determinism pair (t=6300..6310, two independent stagings) — PASS

`iverson-determinism.json`: freeze metadata byte-identical; logical
3,589-slot state + 10-float camera pose digests **bitwise identical
10/10** probes; pixels worst **0.02 %** differing, max channel delta 21
carried by **1 isolated outlier pixel** per frame worst-case (tolerance:
Phase 8 envelope 12/8 % + ≤8 independent-staging outliers, the P10
allowance). Three comparison frame pairs committed
(`iverson-det-{a,b}-*.png`).

## 5. Render stats (Apple M4 24 GB, Unity 6000.4.11f1, offline HDRP profile)

- **36,315 frames** (t=5830..7040.5 at 30 fps, 2560×1440), 21 chunks.
- **0.375 s/frame weighted** (per-chunk 0.355–0.413 — faster than the
  gate forecast's 0.47 because the P10-pattern loop renders without the
  proof harness's scrub probes), **3.78 h pure render**, **3.88 h wall**
  (first launch 23:55 → render-done 03:48).
- **~20 interruptions** (21 loop attempts): the machine SIGKILLed
  nearly every Unity invocation after ~1 chunk (jetsam meets the known
  native leak, aggravated by a concurrent executor's Unity work early
  on) — the resumable-chunk design absorbed all of it with **zero lost
  frames**. Peak managed memory 1,246 MB (flat; the leak is native).
- **Rolling harvester mandatory and clean:** full PNG scratch would be
  ~116 GB against ~40 GB free; the harvester encoded each completed
  chunk with the final delivery settings (decoded-count verified per
  chunk) and reclaimed PNGs; free disk never dropped below 13 GB.
- A machine-wide spend-limit outage interrupted the EXECUTOR mid-render;
  the loop kept running unattended and the render completed during the
  outage — the resumability design also covers operator loss.

## 6. Audio (9-stem deterministic mix)

`IversonProductionRender.ExportAudioEvents` → the production-seed event
streams (`iverson-audio-events.json`, committed): **109,527**
resolver-confirmed musket discharges in the padded window, 266 observer
footfalls, 0 cannon / 0 strikes / 0 crossings (the disclosed
artillery-silent gap and the no-fence site). Stems + mix built
byte-deterministically for t=5830..7040.5
(`build_viewpoint_audio.py`; hashes committed in
`iverson-stems.sha256`; mix.wav `f629974c…`). The discharge count
differs from the gate export's 115,942 because the ED-21 re-pin
re-rolled the per-slot fire-cycle phases — same segments, same rates,
different deterministic offsets.

## 7. Encode + media (pinned settings; mode B lossless chunk concat)

Chunk coverage + final decoded frame count verified = 36,315 before any
deliverable was written.

| file | bytes | avg rate | sha256 |
|---|---|---|---|
| `iverson-forney-field.full.mp4` (2560×1440p30 CRF18 GOP30 +faststart, AAC 192k mix) | 1,982,330,100 | 13.10 Mbit/s | `aca0536d3e17cc671e3cc7c2c39b8dd9da13e473602fc17e85ad3323250325a7` |
| `iverson-forney-field.proxy.mp4` (1280×720p30 CRF20, same audio) | 397,709,639 | 2.63 Mbit/s | `1345d5b889e20e4b7c1b9dd78c0b1053691fb7c34fdc8e5f8e3b360fed392d51` |

`iverson-media.sha256` + `iverson-release-manifest.json` +
`iverson-release-notes.md` committed; media itself is gitignored
(GitHub Releases artifact — **owner publishes**, release
`soldier-view-media-v2-iverson`; the manifest carries the exact
command). Local copies: `app/RenderOutput/iverson/` and staged in
`app/Assets/StreamingAssets/SoldierView/` for the seek suite.

## 8. Seek measurements (media contract)

`IversonMediaSeekTests.SeekLatency_IversonFull1440pProductionMedia` —
12 deterministic seeks across the full 20-minute 1440p stream (far
jumps spanning ~1,200 GOPs, near jumps, sub-second nudges), every
settle landing within one frame of the battle clock (test-asserted).

**Quiet-machine contract measurement (Apple M4 24 GB):
median 65.6 ms, worst 107.3 ms**
(`iverson-seek-latency.json`, committed). A contended first pass
(another executor's production render saturating the machine) measured
218.8/365.0 ms and is recorded only as a load data point, not contract
evidence.

**Owner attention — the P1 revisit trigger:** the media contract set
"~3 frames (100 ms) on the lowest target Mac" as the threshold for
switching to the proxy-frame seek-settle transition. This film's WORST
seek is 107.3 ms on an M4 — already at the threshold on hardware far
above the base-M1 floor (the 20-minute stream spans ~2× the Angle
film's GOP count, and long jumps cost more). Median is comfortable
(~2 frames), and the hold-last-frame behavior masks the settle, but a
base-M1 measurement should be taken before this film's bitrate/GOP is
declared locked; the player's proxy-fallback path (the designed
mitigation) already exists if it exceeds the trigger there.

## 9. Content warning + cross-phase entry

The design slice's §7 text ships as a **per-viewpoint override** in
`content-warning.json` (`viewpointOverrides[0]`, viewpointId
`iverson-forney-field`, version 1) with its own acknowledgement key
(`…contentwarning.ackVersion.iverson-forney-field`): acknowledging the
Angle's warning never suppresses this film's — each film's warning
surfaces before ITS first entry (same mechanics, per override).
PlayMode-verified in the real entry flow
(`IversonWarning_RendersInEntryFlow_WithItsOwnAcknowledgement`).

**Owner check (wording change):** the design-gate draft of the observer
note disclosed TWO unshown record facts (the lying-down fight and the
surrender). The `fight_prone` slice shipped the lying-down fight, so
that clause became false; the shipped text now discloses only the
surrender gap ("One thing the record describes is not yet shown…").
Test-pinned (`test_iverson_content_warning_override_ships` asserts the
prone claim is gone). If the owner prefers different wording, it is one
JSON edit + version bump.

Cross-phase entry: `ViewpointDefinition.battleAsset` (additive; empty =
set home) — `iverson-forney-field` declares
`gettysburg-july1-afternoon`, so its entry marker, timeline band, and
entry exist only while that phase is loaded (per-phase media honesty,
now per viewpoint). The July 3 viewpoints are bit-untouched by the
gate (no battleAsset = the set's home asset, exactly the old behavior).
PlayMode: `CrossPhaseViewpoint_IsRefusedOffItsOwnPhase`.

## 10. Suites (floors in parentheses)

| suite | result |
|---|---|
| tool | **119** (119) |
| pipeline | **66** (66) |
| reconstruction | **162 + 1 skip** (159+1; +3: the ED-21 pin, the cross-phase viewpoint declaration, the warning-override ship test) |
| Unity EditMode | **454 + 1 skip of 455** (448+1 of 449; +6 new tests — bundle pin, warning overrides ×4, per-viewpoint phase gate; the 1 skip is the terrain-material self-skip; the Angle-bake-conditional environment tests RAN — bake regenerated in the worktree) |
| Unity PlayMode | **23 + 1 skip of 24** (20+1 of 21; +3 new tests — Iverson warning entry flow, cross-phase refusal, Iverson full-media seek battery, ALL RUN and passing against the real production media; the 1 skip is the garnett full-media seek test — that media is a release artifact absent from this worktree) |

Quiet-machine PlayMode note: a first full pass taken while another
executor's production render saturated the machine showed 2 failures in
the PRE-EXISTING dev-proxy sync tests (drift/timeout under load); the
quiet re-run passes everything — load-flakiness of timing-asserting
tests, not code. Kept as `playmode-results.xml` (contended) vs
`playmode-results-quiet.xml` in the worktree run logs.

## 11. Film-safety verdict

- `app/Assets/Battle/Angle/angle.bundle.json` — **byte-identical to
  origin/main** (`git diff` names only the Iverson bundle under
  `app/Assets/Battle/`); stagingSeed pin
  `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  holds; `test_committed_bundle_matches_corpus` passes in the suite run.
- **All six phase battle files** (`gettysburg-*.json`) + the July 3
  bundle inputs — byte-identical to origin/main.
- **Angle media** — untouched (not present in this worktree; nothing
  staged over it; the garnett release artifacts are unaffected by this
  branch).
- **Iverson bundle** — changed EXACTLY as ordered: the ED-21 seed
  re-pin (`stagingSeed` + `checksum` fields; every other top-level key
  verified byte-identical to main's, §1). The render exposed **no data
  defect**; nothing else was patched.
- Shared-code changes are additive-defaulted: `battleAsset` (empty =
  old behavior, all July 3 viewpoints bit-untouched by the gate),
  warning overrides (absent = the existing single-warning flow, the
  legacy ack key preserved), `ContentWarningGate` key parameter
  (default = the legacy key), and two members made `internal` for
  harness reuse (no behavior change).

## 12. Stills index (`docs/benchmarks/captures/iverson-production/`)

| still | t | LMT | what it shows |
|---|---|---|---|
| `ivp-still-5840-advance.png` | 5840 | 14:37 | the final advance across Forney's open field, the wall line ahead |
| `ivp-still-5990-volley.png` | 5990 | 14:39 | Baxter's massed volley opening (`fire_by_rank` at the 100-yard closure) |
| `ivp-still-6100-destruction.png` | 6100 | 14:41 | the falling-curve casualty spike at peak, down-line frame-left |
| `ivp-still-6600-prone-fight.png` | 6600 | 14:50 | the observer's 12th NC standing and firing while the left three regiments fight prone (the fight-prone vocabulary in production; Cutler's flank fire joined) |
| `ivp-still-7040-end.png` | 7040 | 14:57 | the end state — the dead in the line the commanders described; the film ends before the surrender |

Plus the determinism pairs (`iverson-det-{a,b}-{000000,000150,000299}.png`).
