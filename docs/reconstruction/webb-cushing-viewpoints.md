# webb-wall + cushing-canister — the plan's two named Angle extension viewpoints

**Slice:** webb-cushing-viewpoints (charge-intensity proposals OP-2/OP-3,
punchlist P8/P9). **Owner-authorized straight to production** — no owner
proof-gate; production renders fired once the determinism probes passed.

Two new pre-rendered Soldier View films of the EXISTING July 3 Angle
window, both named in the V2 plan and never built: the defender's view
at the wall (`webb-wall`) and the gun-crew view at Cushing's battery
(`cushing-canister`). Both RENDER the existing compiled bundle states —
**zero data authoring**.

## Film safety (the absolute constraint)

- The Angle bundle (`app/Assets/Battle/Angle/angle.bundle.json`), its
  stagingSeed pin
  `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`,
  all six phase battle files, and the existing `garnett-road-to-angle`
  viewpoint entry + its media are **byte-untouched** (verified below).
- The modified audio stem builder and NOISY_ACTIONS set regenerate the
  committed Gate P9 garnett proof stems AND the committed garnett
  `captions.json` **byte-identically** (both verified against the main
  checkout's gitignored p9 event export during this slice; the recon
  suite additionally pins the builder's byte-determinism).
- One disclosed consequence, inherent to ED-22's mechanism: protecting
  the two new observer slots reroutes WHICH slot of `us-71pa` /
  `us-btty-cushing` draws each reconstructed fate (totals, windows, and
  strength reconciliation unchanged — tested). The shipped garnett film's
  pixels are fixed media and unaffected; a hypothetical re-render of it
  at this commit would show different victim identities in those two
  Union units, exactly as the Iverson slice's exemption would have done
  had it shared a bundle. The compiled casualty **totals** per unit are
  pinned by test.

## 1. webb-wall (OP-2, punchlist P8) — the defender's view

**Viewpoint:** `us-71pa` slot **230** = rear rank of file 73 (313 slots
→ 157 files), just left of the regimental center of the 71st
Pennsylvania at the outer-angle wall. Window **t=8160..8820** — the
existing viewpoint window, i.e. the same compiled eleven minutes as the
shipped garnett film, faced the other way. First-person, eye 1.66 m,
FOV 68°, stabilization 0.35 (the ratified Gate P9 grammar).

**Slot adjudication** (probe:
`docs/benchmarks/captures/webb-wall/webb-wall-observer-probe.json`):

- **Host regiment.** The task named "a slot in the 69th/71st PA line at
  the wall". The compiled geometry decides it: the 71st's frontage
  (centroid 4400,4868, facing 262°) faces Pickett's approach axis
  dead-on — Garnett's line crosses the road and closes to its stall
  point (4389,4870) ~14 m in front of this slot's wall position, and
  Armistead's column drives through the same frame. The 69th's frontage
  (centroid 4402,4830) faces the inter-brigade gap: from its files the
  wall-crisis concentration sits 55–75° off-axis, outside the ±34°
  first-person frame. A 71pa slot keeps the charge in frame for the
  whole approach AND carries the regiment's attested crisis record.
- **Rear rank** (P9 lesson, the garnett slot-881 precedent): the front
  rank and the wall read 1.3 m ahead of the camera; a front-rank slot
  at the wall shows an empty field with no friendly line in frame.
- **File 73** (local x = −2.5 m): the compiled battery interleaves
  Cushing's pieces WITH the infantry line at the wall (the
  claim-cushing-guns-to-wall staging); file 73 stands in the gap midway
  between gun 1 (6.6 m south) and gun 2 (7.4 m north) — both pieces out
  of frame but at point-blank earshot ("canister outgoing"), forward
  view clear to the field.
- **The fall-back is in the film.** The 71st's compiled crisis segment
  (`seg-71pa-fall-back`, t=8580..8700; claim-71pa-fallback) turns the
  observer east within ~6 s of 8580 and walks him ~48 m back toward the
  crest — during which the camera honestly faces the 72nd's crest line
  firing toward it (ED-4's crest reading) — then `close_gap` returns
  him west with the seal; the window ends mid-return, exactly as the
  garnett film ends mid-repulse.

**The exemption, honestly (ED-22).** The receiving line takes
casualties: the compiled window costs the 71st 48 of 313 (3 approach +
30 fall-back spike + 15 return). Slot 230 is excluded from victim
selection so the viewpoint can witness its full window; another slot
draws the fate it would have drawn, the unit totals are unchanged
(pinned: `WebbCushingViewpointTests.Exemption_PreservesTotals_ForBothNewObserverUnits`),
and the exemption is disclosed in the viewpoint's editorialNote. The
observer's survival coincides with the documented majority of the
regiment (265 of 313 stand at window end).

### Machine evidence

- **Preflight** (`webb-wall-preflight.json`): the full padded window
  t=8160..8820.5, every unit, every slot — 62,110,620 coarse pairs,
  10,757 suspect windows refined at 30 fps, all of them the designed
  crossing-exit hand-off, **zero violations**; camera max
  0.0152 m/frame (bound 0.15) — the defender camera is nearly static.
- **Determinism pair** (`webb-wall-determinism.json`): t=8600..8610
  (the smoke-heaviest wall-crisis stretch) rendered twice from two
  fully independent stagings. Freeze metadata identical; logical-state
  + camera-pose digests **bitwise identical 10/10 in both passes**
  (probe 2/2 criterion exceeded); pixels worst 0.02% differing, max
  channel delta 14 confined to **1 isolated outlier pixel** (tolerance:
  Phase 8 envelope ≤8% at delta ≤12, plus ≤8 isolated outliers/frame).
  **PASS.**

### Production render (measured)

- Window t=8160..8820 + 0.5 s media-contract pad = **19,815 frames** at
  2560×1440p30 (identical geometry to the shipped garnett film).
- **0.280 s/frame weighted** (0.235 in the hold chunks to 0.471 in the
  smoke-heavy crisis/repulse chunks) = **1.54 h pure render**; peak
  managed **1,166 MB**; 12 × 60 s resumable chunks, rolling-harvested
  into losslessly-concatenating chunk encodes (1.0 GB); the loop
  restarted Unity across jetsam kills and one wrapper kill mid-run —
  **zero frames lost or duplicated** (frame continuity and final decoded
  count re-verified = 19,815 by `scripts/viewpoint-encode.sh`).
- Chunk manifests: per-chunk git SHA, bundle checksum, viewpoint id,
  battle-time range, `t(frame) = 8160 + frame/30`, settings hash —
  **settingsHash identical across all 12 chunks** (one documentation
  commit landed between resume attempts, the P10 pattern); freeze
  record `webb-wall-freeze.json` (settingsHash `11e751d0e39ee4aa…`,
  input checksums identical to the P10 production freeze).
- Deliverables (`webb-wall-media.sha256`, release manifest committed):

| file | size | avg bitrate |
| --- | --- | --- |
| `webb-wall.full.mp4` | 1,014,939,176 B (0.95 GiB) | 12.29 Mbit/s — 2560×1440p30 H.264 CRF18, GOP 30, +faststart, AAC 192k deterministic mix, sha256 `908634fc64ee4583…` |
| `webb-wall.proxy.mp4` | 201,227,997 B | 2.44 Mbit/s — 1280×720p30 CRF20, same audio, sha256 `1f454e2ec45081ab…` |

  (Lower bitrate than the garnett film's 20.2 Mbit/s at the same CRF —
  the defending camera is nearly static, so inter frames are cheap; the
  quality target, not the bitrate, is pinned.)
- Stills committed (`webb-wall-still-*.png`): the line at the wall
  (8165), first fire (8460), the charge closing point-blank over the
  front rank at the wall (8575), the wall crisis (8610), the breach
  passing left (8650), the fall-back/rally frame (8700), the return
  (8790) — plus one determinism frame pair.

### Audio — the receiving position's mix

Same 9-stem deterministic pipeline (`build_viewpoint_audio.py`), same
compiled event streams, re-addressed for a **Union observer** (the
export now carries `observerUnit`/`observerSide`; legacy garnett
exports reproduce the shipped stems byte-identically — proven, and
regression-tested in `reconstruction/tests/test_audio.py`):

- **Canister outgoing:** Cushing's 99 in-window discharges fire from
  point-blank flanking positions (guns 6–8 m from the observer);
  Cowan/Arnold and the rest of the staged line farther out.
- **The charge arriving:** 3,149 Confederate discharges (Fry's wall
  fight north of the Angle + Armistead's breach fire) now drive the
  projectile pass-by layer — the whiz stem keys on ENEMY fire relative
  to the observer's side. The 51,842 Union discharges are the
  observer's own line: near, un-whizzed, and dominant.
- **Strikes:** with a tagged observer the stem keeps every compiled
  impact within 120 m regardless of unit — the canister tearing ground
  in Garnett's stalled mass ~15–40 m in front of the wall is audible
  from the receiving side (the garnett-era own-unit shortcut applies
  only to untagged legacy exports).
- **Shout bursts:** `fire_by_rank` / `fire_independent` / `close_gap`
  joined NOISY_ACTIONS for the defending observers (a firing line's
  loud human moments); the garnett film's segment vocabulary contains
  none of these, so its mix/captions are unchanged.
- **Character vs the garnett mix:** near-silent approach phases carry
  meadow ambience and the distant enemy mass; almost no gait/footfall
  bed (the observer stands at a wall); friendly musketry IS present
  (unlike garnett's ED-23 no-return-fire consequence); wounded voices
  are Union men falling around the camera. Chaos-driven handheld stays
  near zero until the crisis (no compiled strikes land on Union units —
  the CSA guns are outside the staged bundle; recorded gap, the mirror
  of garnett's artillery-silent approach).

Captions: `captions-webb-wall.json` (22 captions) generated from this
film's own event export by the shared generator; the HUD now selects
the caption track matching the ACTIVE viewpoint (a garnett caption can
never show inside another film — `CaptionTrack.ForViewpoint`, tested).

## 2. cushing-canister (OP-3, punchlist P9) — the gun-crew view

**Viewpoint:** `us-btty-cushing` slot **44** = gun 2's rearmost left
serving position (crew slots are gun + 6·member; member 7 of 8, 6.1 m
behind the piece at the trail). Window **t=8400..8760** (OP-3's named
window): opens exactly with the battery's compiled `fire_independent`
canister segment (pinned by test) and runs into the overrun. First
person, same camera grammar.

**Slot adjudication** (probe:
`docs/benchmarks/captures/cushing-canister/cushing-canister-observer-probe.json`):

- **Not Cushing (ED-3).** The observer is an unnamed crew position. No
  rendered figure in the battery asserts Lt. Cushing's identity; his
  documented multi-wound death as the column closed (canonical fatal
  shot ~t=8660) rides `claim-cushing-death` — a claim on the record,
  cited by the battery's aggregate casualty profile, never assigned to
  a slot. The editorialNote says this explicitly, and
  `CushingCanister_ObserverIsGunCrew_NeverCushingHimself` pins it.
- **Gun 2, rear of the serving arc:** from 6.1 m behind and left of the
  piece the whole eight-man crew serves in frame-right, the muzzle
  fires over the wall ahead, Garnett's stall point sits 16 m dead ahead
  at the crisis, and Armistead's column drives left-of-frame to its
  breach. The rear echelon (limbers, 22 m back) would see only backs;
  a forward crew slot would put the camera inside the recoil lane.
- **Register (OP-3's own analysis):** a battery crew's losses are light
  (9 of 97 across the slice, 97→88 in-window) — this film leans on
  tempo and mechanism: `ResolveArtillery` braces each gun's crew at its
  own discharges (99 scheduled shots for this battery in-window), the
  crew arcs hold drill spacing, double canister goes out into a closing
  mass. The observer slot is stationary the whole window (probe: same
  position, facing 265°, t=8400..8760).
- **Staging honesty (ED-16, recorded conflict):** the compiled schedule
  works all six pieces at drill interval; Webb's "three of Cushing's
  guns were run down to the fence" versus the corpus's
  two-serviceable-guns count rides `claim-cushing-guns-to-wall`
  unadjudicated, and the editorialNote says so.

**Exemption:** slot 44 is excluded from the draw (ED-22); battery
totals unchanged (same pinned test as the 71st).

### Machine evidence

- **Preflight** (`cushing-canister-preflight.json`): padded window
  t=8400..8760.5, every unit, every slot — 33,895,620 coarse pairs,
  3,982 suspects refined, all designed crossing-exit hand-offs, **zero
  violations**; camera max delta **0.0000 m/frame** (the crew slot is
  fully static). **PASS.**
- **Determinism pair** (`cushing-canister-determinism.json`):
  t=8520..8530 (rolling canister, guns hot) rendered twice from
  independent stagings. Freeze metadata identical; logical + pose
  digests **bitwise identical 10/10 both passes**; pixels worst 0.01%
  differing with **2 isolated outlier pixels** (≤8 allowed; max channel
  delta 31 confined to those isolated depth/coverage-tie pixels — the
  documented P10 two-tier tolerance). **PASS.**

### Production render (measured)

- Window t=8400..8760 + 0.5 s pad = **10,815 frames** at 2560×1440p30.
- **0.332 s/frame weighted** (the whole window is smoke-heavy close
  action) = **1.00 h pure render**; peak managed **1,162 MB**; 7 × 60 s
  resumable chunks (one resume across the session cut — 5 chunks
  survived on disk, the loop resumed at chunk 5, zero frames lost);
  **settingsHash identical across all 7 chunks**, single render gitSha;
  freeze `cushing-canister-freeze.json` (settingsHash
  `2fbed41f3b38b7dc…`, input checksums identical to the P10 freeze).
- Deliverables (`cushing-canister-media.sha256`, release manifest
  committed):

| file | size | avg bitrate |
| --- | --- | --- |
| `cushing-canister.full.mp4` | 431,728,399 B | 9.58 Mbit/s — 2560×1440p30 H.264 CRF18, GOP 30, +faststart, AAC 192k deterministic mix, sha256 `69c6be7f56b0aa64…` |
| `cushing-canister.proxy.mp4` | 103,710,405 B | 2.30 Mbit/s — 1280×720p30 CRF20, same audio, sha256 `75067d24dc21c1b6…` |

- Stills committed (`cushing-canister-still-*.png`): canister opens
  (8405), the serving rhythm (8520), the stall in front of the guns
  (8600), double canister into the breach window (8650, the crew inside
  the powder smoke), the overrun phase (8730) — plus one determinism
  frame pair.

### Audio — the crew position's mix

Built from this viewpoint's own export (t=8394..8761): the dominant
layer is the battery itself — the observer's own gun 2 discharges at
arm's length with the five sister pieces flanking — over the same
enemy-side whiz keying and distance math as webb-wall. 47,092 musket
discharges in the padded window (43,943 Union / 3,149 CSA); 338 staged
cannon discharges; 986 casualties within earshot; zero footfalls (the
crew position never marches). Captions:
`captions-cushing-canister.json` (49 captions, dominated by wounded
voices in the overrun window).

## SV entry UI

The Iverson slice's multi-viewpoint pattern carries the wiring: entry
markers are built per-frame from EVERY committed non-development
viewpoint whose window covers the clock (`HudModel.EntryMarkerVisible` →
`AtlasHud.UpdateEntryMarkers`), so the two new films appear as
additional "Enter Soldier View — <title>" buttons alongside the
garnett entry through their overlapping windows (all three at
t=8460..8760). New EditMode pin:
`AtlasHudModelTests.EntryMarkers_AllCommittedViewpointsInTheirOverlappingWindows`.
The content-warning gate applies unchanged (same warning document, same
first-entry flow, viewpoint-independent). Caption selection is now
per-active-viewpoint (see above).

## Seek measurements

PlayMode seek batteries on the real 1440p production streams (12
deterministic seeks each — long jumps across hundreds of GOPs, near
jumps, sub-second nudges, both directions, clear of the end-guard;
every settle asserted within one video frame of the battle clock;
measured on the idle machine, reports committed beside the evidence):

| stream | median | worst |
| --- | --- | --- |
| `webb-wall.full.mp4` | **33.3 ms** | **50.1 ms** |
| `cushing-canister.full.mp4` | **33.4 ms** | **66.7 ms** |

Both beat the garnett film's Gate P10 numbers (33.9 / 107.3 ms) — no
seek grazes the P1 ~100 ms revisit trigger, so seek-and-hold stands
without qualification for both new streams. End-guard: both media are
padded 0.5 s past t1, so the unreachable-final-frames window sits
entirely in padding (P1 media contract).

## Suite state at slice end

All at or above the current-main floors (tool 119 / pipeline 66 /
recon 158+1 / EditMode 436+1 / PlayMode 20+1), run in this worktree
via the standard CLI (`-batchmode -runTests -buildTarget OSXUniversal`,
worktree Library, no `-nographics`):

- tool **119** passed
- pipeline **66** passed
- reconstruction **161 + 1 skip** (+3: defending-observer mix tests)
- Unity EditMode **446 passed + 1 known conditional skip** (447 total;
  +9 over the 436 floor: viewpoint-definition pins ×2 extended,
  WebbCushingViewpointTests ×6, caption selection ×3, entry-marker
  overlap pin, observer-protection asserts — net new tests)
- Unity PlayMode **23/23, zero skips** (20 baseline + 2 new production-
  media seek measurements + 1; full media staged — dev proxy
  regenerated byte-identical to main's via the pinned script; one
  transient mid-suite failure of the P1 proxy latency test occurred
  only while the concurrent iverson production render loaded the GPU,
  and the suite passed 23/23 in full afterwards under the sync-flake
  single-re-run policy)

Content-warning text and gate: applied unchanged (same document, same
first-entry flow; `ContentWarningGateTests` untouched and green).

## Reproduction

```sh
# per viewpoint (webb-wall shown; cushing-canister identical):
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.ViewpointRender.Preflight \
  -viewpointId webb-wall -logFile pf.log
"$UNITY" ... ViewpointRender.RenderDeterminismPair -viewpointId webb-wall ...
UNITY=... scripts/viewpoint-render-loop.sh webb-wall 12   # + in a 2nd shell:
scripts/viewpoint-chunk-harvester.sh webb-wall
"$UNITY" ... ViewpointRender.ExportAudioEvents -viewpointId webb-wall ...
cd reconstruction && uv run python scripts/build_viewpoint_audio.py \
  --events ../docs/benchmarks/captures/webb-wall/webb-wall-audio-events.json \
  --out ../app/RenderOutput/webb-wall/stems-full --t0 8160 --t1 8820.5
scripts/viewpoint-encode.sh webb-wall
python3 scripts/viewpoint-release-manifest.py webb-wall
```

(cushing-canister: 7 chunks, stems t0 8400 t1 8760.5.) Everything else
— inputs regeneration, the no-accumulation/motion-blur decision, the
render-loop-vs-jetsam story — is the garnett render runbook
(`docs/reconstruction/render-runbook.md`) unchanged; media ships via
GitHub Releases per the per-viewpoint release manifests.

## Residuals

1. **Main moved mid-slice.** The angle-v2 vocabulary wave (melee,
   colors succession, mounted falls, halt-and-fire; ED-81) merged to
   main while these renders ran. Both films render the **v1 compiled
   states at this branch's fork point (cff17dd)** — exactly as
   chartered ("your renders are of the CURRENT v1 states"). This branch
   touches zero battle files relative to its fork point, so merging it
   cannot revert the v2 wave; but a v2-vocabulary RE-RENDER of all
   three Angle films is the anticipated follow-up, and these two
   viewpoints are now one `-viewpointId` argument each away from it.
2. **ED-22 draw rerouting** (disclosed above): re-rendering the GARNETT
   film at this commit would show different victim identities within
   us-71pa and us-btty-cushing (totals unchanged). Since a garnett
   re-render is expected under the v2 wave anyway, the two waves land
   together.
3. **Iverson marker leakage (pre-existing, out of scope):** the
   design-stage `iverson-forney-field` entry rides the July 1 clock,
   but the HUD's per-phase gate is per-SET (home battle asset), so its
   entry marker appears during July 3 at t=5830..7040 and entry is
   refused only by the missing-media path. Wiring cross-phase viewpoint
   homes is the Iverson production slice's declared follow-up scope.
4. **No incoming-strike layer for Union observers:** the CSA guns that
   fired on Cemetery Ridge sit outside the staged bundle, so the
   defenders' films carry no incoming artillery strikes (visual or
   audible) — the mirror of the garnett film's artillery-silent
   approach, recorded under the same rule (an authored layer would need
   its own sourced event basis).
5. **Battery crew choreography is the §9.1 placeholder** (brace at
   discharge, kneel/stand otherwise): no rammer/swab/handspike drill
   cycle exists in the kit. The cushing-canister film's mechanism
   register would gain the most from a future crew-drill clip set;
   recorded as kit work, not a reconstruction question.
6. **Seek reports for the two new streams** live at
   `docs/benchmarks/captures/<vp>/<vp>-seek-latency.json` (the app/
   copies stay gitignored like p10's).
7. **Media publication** is the owner's: per-viewpoint release
   manifests + notes are committed with the exact
   `gh release create soldier-view-media-<vp>-v1` commands.
