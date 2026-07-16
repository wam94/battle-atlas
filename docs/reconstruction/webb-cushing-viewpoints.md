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

<!-- WEBB_RENDER_STATS -->

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

<!-- CUSHING_EVIDENCE -->

### Production render (measured)

<!-- CUSHING_RENDER_STATS -->

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

<!-- SEEK_NUMBERS -->

## Suite state at slice end

<!-- SUITES -->

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

<!-- RESIDUALS -->
