# Iverson's Field — the second Soldier View viewpoint (design gate)

**Status:** OWNER GATE — design + proof renders only. The production
render is NOT authorized by this slice; the owner chooses at this gate.
**Site:** Iverson's brigade destroyed, Forney field / Oak Ridge, July 1,
~14:30–15:00 (CA-J1P-2; the corpus's single-spike EC6 exemplar:
1,464 → ~650 in one keyframe pair, with the 500-on-the-dress-parade-line
quote four-echelon corroborated).
**Phase file:** `app/Assets/Battle/gettysburg-july1-afternoon.json`
(startTime 46800 = 13:00 LMT; byte-untouched by this slice).
**Branch:** `iverson-viewpoint`. The Angle film, its bundle (stagingSeed
pin `d470c469…`), and `gettysburg-july3.json` are byte-untouched
(verified §9).

This is the corpus's most concentrated killing. The
sober-representation doctrine (`violence-and-representation.md`) binds
every decision below; nothing here is dramatized, and the two places
where the record outruns the current representation vocabulary are
disclosed as gaps, not papered over (§4.3).

---

## 1. The observer question

Three candidate classes were assessed from the dossier bank
(`reconstruction/dossiers/csa-2c-rod-2-iverson.md`,
`us-i-2-2-baxter.md`; pass 10–11 audits). T5's bar: every behavior the
camera sees must trace to the record.

### Candidate A — riding Iverson's line into the killing ground (RECOMMENDED, with the 12th NC refinement)

A representative unnamed man inside the brigade that the episode is
ABOUT. What the record supports seeing, minute by minute: the final
advance across the open field, unsupported on the left
(or-27-2-iverson's support-miscarriage ledger; O'Neal repulsed "almost
instantaneously"); the massed fire from a "concealed stone wall" at
~100 yards (or-27-2-iverson; or-27-1-baxter: "opened on the advancing
foes a most deadly fire"); "a most desperate fight" from the halted
line; the dead accumulating IN LINE ("500 of my men were left lying
dead and wounded on a line as straight as a dress parade" = Rodes's
"His dead lay in a distinctly marked line of battle" — two independent
line-geometry statements); Cutler's flank volley; the 12th Mass flank
fire; the handkerchiefs.

**The refinement that makes this honest: the observer rides the 12th
North Carolina — the documented survivor regiment.** The 12th, on the
brigade's sheltered right, "held intact" under Lt. Col. Davis (both
Iverson and Rodes), lost 56 k+w against the left three's ~400
(or-27-2-anv-return p. 342 regiment rows), did not surrender, and
formed the core of Capt. Halsey's remnant rally. The Garnett-film
observer exemption (ED-22) asks the viewer to accept an unhurt walker
in a unit that lost half its men; HERE the exemption coincides with a
real, documented survival population. The observer stands in the rear
rank near the 12th's LEFT flank — twelve meters from the 23rd NC's
right — so the destruction of the left three regiments plays out
down-line in frame-left at eye level, while his own regiment fights
and holds. The camera witnesses the exemplar without the camera's
survival contradicting it.

### Candidate B — receiving at Baxter's wall (the 88th PA / 83rd NY front)

A representative man behind the wall. Record support is also strong:
the concealment ("concealed stone wall" is the ENEMY's own
description), the rise-and-volley, "repeated lines repulsed", the
three-regiment charge "capturing many prisoners, the Eighty-eighth
Pennsylvania taking two battle-flags and the Ninety-seventh New York
one", ammunition exhaustion and the ~15:00 relief (or-27-1-baxter;
or-27-1-coulter's clock). Assessment: T5-viable, and the exemption is
mild (Baxter's in-window loss is light). But the film would be a
FIRING-line POV — the viewer spends twenty minutes killing a brigade
at 90 m. It is the stronger choice only if the owner wants the
receiving-side framing; it inverts the first film's grammar (Garnett's
line TAKES fire without returning it; Iverson's line would GIVE it).
Proposed as the natural EXTENSION viewpoint (the `webb-wall` pattern,
plan §3.4), not the hero.

### Candidate C — a named individual pinned in the record

Assessed and RECOMMENDED AGAINST. The record pins individuals only at
command grain: Capt. D. P. Halsey (AAG — but his documented acts, the
rally and remnant charge, are AFTER the destruction window); Lt. Col.
Davis (12th NC — "held intact" is a unit statement, not a personal
track); Capt. Benjamin Robinson (5th NC sharpshooters — his corps'
in-window position is not pinned); Baxter himself (uninjured, but no
personal in-window track). No diary/letter places a named man at a
specific spot through the destruction. Under plan §15 ("historically
identified anonymous soldiers" is a non-goal) and the
violence-and-representation rules (no invented named-person details
without specific sourcing and separate approval), a named observer
would require new primary sourcing and owner approval — noted as a
possible future hardening (a Halsey viewpoint for a RALLY film would
be the strongest named case), out of scope here.

**Recommendation: Candidate A** — `iverson-forney-field`, first
person, formation slot 184 of the 12th North Carolina (rear rank,
file 24 of 160; the P9 rear-rank lesson applied — the whole front rank
reads 1.3 m ahead of the camera). Committed as the design-stage entry
in `viewpoints.json` with the disclosure-heavy editorial note; the
owner may swap to Candidate B at this gate without re-authoring (the
bundle stages Baxter's line fully; only the viewpoint entry changes).

## 2. The window

| | phase-t | LMT | why |
|---|---|---|---|
| bundle slice t0 | 5820 | 14:37 | inside CA-J1P-2's adopted 14:30–15:00; the final 190 m of the approach (3 min of marching = the entry pad; the wall's fire still held) |
| the volley | 5985–6240 | 14:39–14:44 | Baxter opens (`fire_by_rank`); the falling-curve casualty spike |
| the fight | 6240–6900 | 14:44–14:55 | "a most desperate fight"; both lines firing; Cutler's flank fire from 6600 |
| the tightening | 6900–7050 | 14:55–14:57 | rising-curve profiles as the flank fire converges |
| bundle slice t1 | 7050 | 14:57 | **ends BEFORE the surrender** (see §4.3) |
| viewpoint window | 5830–7040 | | 20.2 min; ±10 s inside the slice so the media contract's end-guard padding (+0.5 s past t1, `p1-media-contract.md`) is renderable — tested (`test_viewpoint_window_inside_bundle_slice`) |

Proposed FILM window = the viewpoint window above (~20 min ≈ 36,300
frames; at the P10 machine's 0.28 s/frame ≈ 2.8 h render). An owner
TRIM option: t0=5900 (drop 70 s of approach) if 20 min reads slow;
the approach minutes are what make the volley land, so the
recommendation is to keep them.

An EXTENSION option (gated on §4.3's vocabulary work): t1→~7530 to
include the charge, the white handkerchiefs on the left, and the start
of Halsey's rally. Without that vocabulary the film must end before
the surrender, which the current window does.

## 3. Camera design

Everything reuses the P9-accepted machinery unchanged
(`HeroViewpointCamera` pure pose function, stabilization 0.35, eye
1.66 m, FOV 68°, lens guard, `HeroMotionProfile.Standard` with the
`ReducedMotion` accessibility variant available):

- **Slot:** csa-12nc / 184 (rear rank, 27.75 m in from the regiment's
  left flank). Exempt from the victim draw per ED-22
  (`ViewpointObservers`); unit totals unchanged (tested).
- **FP vs close-3P:** owner's choice at this gate, per the P9 pattern —
  both 30 s candidates below are the same 30 seconds from the same
  slot; `viewKind` flips everything downstream. The executor's
  recommendation remains **first_person** for the same three reasons
  recorded at P9 (the representative-observer stance, the
  no-rotation contract reading as a choice rather than a bug, and
  own-body occlusion on the threat axis) — plus one site-specific one:
  in C3P the observer's back occludes exactly the direction the
  destruction develops from (frame-left), which is this film's whole
  subject.
- **Heading:** the unit facing (155°) — the wall, the smoke sheet, and
  the front rank ahead; the left three regiments' line recedes
  down-frame-left; Forney's farm behind the right shoulder early.
- **No new camera behaviors were authored** — halted-line behavior
  (no gait bob, canister-duck, near-fall head-turns, chaos-scaled
  handheld) is the P9 vocabulary; this window is dominated by the
  halted fight, so the camera reads as a standing man under fire for
  ~17 of 20 minutes. The proofs exist to judge exactly this.

## 4. The cast and the T4→T5 gap

### 4.1 What was authored for the proofs (in the bundle, this slice)

| unit | slots | basis |
|---|---|---|
| csa-5nc / 20nc / 23nc / 12nc | 319 each | csa-iverson decomposed (equal-quarter start split, R-iv-regiment-split; order 5-20-23-12 left→right, EDI-5); per-regiment losses ∝ the ANV return's k+w rows with the 12th's documented lightness (R-iv-regiment-apportionment); regiment sums reconcile to the brigade macro track ±1 at every in-slice keyframe + both edges (validator-enforced) |
| us-baxter | 1,402 | brigade line 4 m SE of the wall trace, hold → fire_by_rank at the 100-yard closure |
| us-cutler | 911 | the flank: hold → the changed-front closure (macro t=6300 keyframe) → fire_independent |

3,589 slots total (vs the Angle's 9,405). The brigade's on-the-line
loss (t≥6000) is 525 — the macro-forced quantity nearest the attested
500 — of which the left three carry 485 and the 12th NC 40.
Environment authored: the wall trace through the drawn Baxter bar
(EDI-1), the Oak Ridge woods band (EDI-2), the open Forney fields +
trampled advance corridor (EDI-4), Forney farm massing (EDI-6).
July 3 sun-ephemeris table reused for July 1 (2 days ≈ <0.3° solar
geometry error at the same LMT; disclosed as EDI-7).

### 4.2 Proposed follow-up scope (the production-render slice)

1. **Sheet-trace hardening (EDI-1/2 → traced):** crop_sheet.py +
   georef over j1-09 (the Angle-standard method) for the wall line,
   the woods edges, the field boundaries, and the Mummasburg Road —
   the map-furniture road trace is ~700 m adrift here and was NOT
   used. Proof geometry is editorial-from-pinned-points; production
   should trace.
2. **O'Neal and Ramseur context casts:** O'Neal's repulsed wave
   NNW-of-field (the empty left the record explains the destruction
   by) and Ramseur's arriving column late in an extended window.
3. **Union regimental decomposition:** Baxter's wall regiments (the
   88th PA / 83rd NY / 97th NY charge sub-structure, the 12th Mass
   flank position) — needed for the extension window's charge, not
   for the current one.
4. **Atlas-side regiment promotion (optional):** the four NC regiments
   exist only in the tactical bundle; promoting them to the July 1
   phase file (the decomposition-wave pattern) is an Atlas concern,
   not a film blocker.
5. **Carter's Oak Hill artillery** (see §5 — the audio gap is the real
   motivation; a staged distant battery would also give the approach
   minutes their shell strikes).

### 4.3 The two vocabulary gaps the record forces (named, not hidden)

1. **The lying-down fight.** "My line of battle still lying down in
   position" — the brigade fought prone in the swale. The clip
   vocabulary has `ProneCrawl` (wounded) but no prone-fire cycle; the
   proofs show the fight standing/kneeling (`fire_independent`).
   Follow-up: a `fight_prone` action + prone aim/fire/reload clips —
   the single highest-value fidelity item for this film
   (claim-iv-lying-down carries the disclosure).
2. **The surrender.** The 308-man capture mass, the white
   handkerchiefs from a lying line — no `surrender` action, no
   hands-up/kneel-captive clips, and no captor choreography exist.
   The slice window therefore ENDS at 14:57, before the event
   (validator-tested). Follow-up: `surrender` vocabulary + the
   extension window; until then the film's honest shape is
   "the destruction, not the capture" (claim-iv-surrender-mass).

Also disclosed: Baxter's pre-volley concealment (kneeling below the
wall crest) has no representation — his line stands at the ready
behind the wall until 5985 (gap #3, minor).

## 5. Audio assessment (July 1 window, existing 9-stem pipeline)

The stem builder (`build_viewpoint_audio.py`) is event-generic; the
harness exports the same streams P9/P10 used
(`IversonGateRender.ExportAudioEvents` →
`docs/benchmarks/captures/iverson-viewpoint/iverson-audio-events.json`).
What the existing byte-deterministic pipeline produces for this window:

- **Musketry — the film's dominant layer, and it fits the record's
  shape exactly.** Every report is a resolver-confirmed discharge,
  distance-delayed/attenuated/panned. Baxter's 1,400 muskets opening
  `fire_by_rank` at t=5985 IS the single massed volley the dossiers
  describe, followed by the rolling sustained fire; Cutler's 900 join
  from the flank at 6600. Measured export: **115,942 discharges** in
  the padded 20-min window (vs the Angle's 55,439 over 11 min) — this
  window is musketry-dense on both sides.
- **Friendly fire cycles — new vs the first film.** Garnett's brigade
  never fires in-window (ED-23); the 12th NC DOES (`fire_independent`,
  claim-iv-fight), so the observer's own regiment's discharges and
  reload Foley surround the camera for the first time. The proof is
  the place to judge that layer.
- **The wounded, unit noise, breathing, footfalls, ambience** — all
  carry over unchanged (casualty stream: every audible-range fall).
- **GAPS:** (1) **No artillery layer**: Carter's Oak Hill battalion
  enfiladed the field (macro event `j1p-carter-oakhill`) but no
  battery is in the staged cast, so the approach minutes carry no
  shell reports and the `strikes` stream is empty — the same
  "artillery-silent approach" limitation class P9 recorded; fix rides
  §4.2.5. (2) No fence-rail layer (no fences on this site — correct,
  not a gap). (3) The P9-recorded source-quality and missing-layer
  notes (Freesound preview transcodes, no worded commands, no
  equipment jingle) apply unchanged. **No new sourcing needed for the
  gate**: the stems for the 30 s proof window build from the existing
  pinned pack (`freesound-pack.lock.json`).

**Verdict: the existing pipeline covers this film's soundscape; the
one real gap (distant artillery) is a cast question, not an audio
question.**

## 6. Proof renders (this gate's evidence)

`docs/benchmarks/captures/iverson-viewpoint/` (representative stills
force-added; sequences/media gitignored, regenerable deterministically):

- `iverson-proof-fp-30s-1440p.mp4` — first person, t=6090..6120
- `iverson-proof-c3p-30s-1440p.mp4` — close third person, same window
  (both muxed with the deterministic 30 s stem mix)
- `iv-still-5840-advance-fp.png` … `iv-still-7040-endstate-c3p.png` —
  the arc in stills (the advance, the volley opening, the destruction
  at peak in both styles, the late fight, the end state with the
  line's dead)
- `iverson-gate-report.json` — machine evidence: bitwise scrub probes
  (logical 3,589-slot state + camera pose), pixel tolerances, timing
- `iverson-audio-events.json` + `stems/` — the event export and the
  proof-window stems
- `iverson-proof-media.sha256`

Regeneration (deterministic; Unity editor closed, worktree Library):

```sh
cd pipeline
uv run python -m terrain_pipeline.cli crop --x0 3350 --z0 7800 \
  --x1 4350 --z1 8800 --out ../data/heightmap_oakridge
uv run python -m terrain_pipeline.cli environment-oakridge
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.IversonGateRender.RenderCandidates \
  -logFile iv-cand.log
"$UNITY" ... IversonGateRender.RenderStills ...
"$UNITY" ... IversonGateRender.ExportAudioEvents ...
cd reconstruction && uv run python scripts/build_viewpoint_audio.py \
  --events ../docs/benchmarks/captures/iverson-viewpoint/iverson-audio-events.json \
  --out ../docs/benchmarks/captures/iverson-viewpoint/stems --t0 6090 --t1 6120
scripts/iverson-proof-encode.sh
```

Measured results are appended in §10.

Production-render plan (LATER slice, owner-gated): the P10 pattern
unchanged — resumable 60 s chunks via a site-parameterized
`Phase10Render` entry, preflight no-teleport gate over the padded
window, rolling harvester on disk-constrained machines, pinned encode
(libx264 slow CRF 18, GOP 30, +faststart), stems-first audio, media
via GitHub Releases. ~36,600 frames (window + 0.5 s pad) ≈ 2.9 h pure
render on the P10 machine by its measured 0.28 s/frame.

## 7. Content warning (draft for this film)

To ship beside (not replace) the Angle warning; same acknowledgment
mechanics (`content-warning.json` gains a per-viewpoint entry at the
production slice — presentation wiring is Phase-11-pattern follow-up):

> **Before you enter Soldier View**
>
> You are about to watch a reconstruction of the destruction of a
> Confederate brigade at Gettysburg — Iverson's North Carolinians in
> the fields north of town, July 1, 1863 — from inside the one
> regiment that came through it largely intact. It depicts, explicitly
> and without relief: a massed volley fired into a line of men at
> close range, soldiers killed and wounded in great numbers in a few
> minutes, the dead lying where they fall — in the straight line their
> commander described — and the sounds of battle, including the
> wounded.
>
> This field saw some of the most concentrated killing of the war.
> Nothing here is dramatized for effect; casualties occur where and
> when the reconstruction's evidence places them, and the depiction is
> held deliberately sober. It is still a depiction of mass violence,
> and it is meant to be difficult.
>
> If you prefer not to watch this, the Atlas view presents the same
> events at map scale.

And the representative-observer note (info panel), Iverson variant:

> **Whose eyes are these?**
>
> No one's, and everyone's in that regiment. The viewpoint stands with
> formation slot 184 of the 12th North Carolina — a rear-rank man near
> the regiment's left, a *representative, unnamed* soldier, not an
> identified person. No diary, letter, or service record places a
> specific man at this spot; the reconstruction does not pretend
> otherwise.
>
> The 12th North Carolina is the regiment the record itself keeps
> standing: posted on the brigade's sheltered right, it lost lightly
> while the three regiments beside it were destroyed, and it did not
> surrender. The observer's survival through the window is still a
> disclosed editorial exemption — his slot is excluded from the
> casualty draw, and the unit's totals are unchanged — but here that
> choice coincides with what happened to his regiment, not against it.
>
> The men around the camera march, fire, fall, and die according to
> the same aggregate evidence that drives the whole reconstruction.
> None of them is an identified casualty. Two things the record
> describes are not yet shown, and we say so rather than fake them:
> the brigade fought much of this action lying down, and the survivors
> of the left regiments surrendered where they lay; the film ends
> before the surrender.

## 8. Open owner choices (this gate)

1. **Observer:** Candidate A (12th NC, recommended) vs Candidate B
   (Baxter's wall) vs defer. Candidate C (named individual) is
   recommended against without new sourcing.
2. **FP vs close-3P:** judge the two 30 s proofs (executor recommends
   first_person, §3).
3. **Window trim:** keep the 20.2-min window (recommended) vs trim the
   approach (t0→5900) vs authorize the §4.3 vocabulary work first and
   extend through the surrender/rally.
4. **Vocabulary priority:** whether `fight_prone` (gap #1) blocks the
   production render or ships as a v2 refinement. Executor's view: it
   is the film's biggest fidelity item; recommend blocking.
5. **Production render authorization** — explicitly NOT granted by
   this slice; on approval the ED-21 seed is re-pinned to the reviewed
   bundle checksum and the P10 pattern runs (§6).

## 9. Film-safety verdict

- `app/Assets/Battle/Angle/angle.bundle.json` — byte-untouched
  (stagingSeed pin `d470c469…` holds; suite-verified:
  `test_committed_bundle_matches_corpus` recompiles it from the
  untouched Angle inputs).
- `gettysburg-july3.json`, `gettysburg-july1-afternoon.json`, every
  other phase file — byte-untouched (this slice adds NO battle-file
  edits; the regimental decomposition lives in the tactical bundle
  only).
- The Angle corpus inputs (sources.json, angle.claims.json,
  landcover.json, angle.reconstruction.json) — byte-untouched; the
  July 1 corpus is in new, separate files by design.
- Shared code changes are additive-defaulted:
  `AngleEnvironmentStage.CropDirOverride` (null = Angle) and
  `AngleActionScene.StageAll(bundlePath = null)`.

## 10. Measured proof results (Apple M4 24 GB, Unity 6000.4.11f1, offline HDRP profile)

From `iverson-gate-report.json` + the encode log:

- **Staging:** 3.6 s, 3,589 slots, bundle `1e2e802f4eca…`, seed
  `iverson-proof-seed/1` (the proof pin).
- **Render:** 2×900 frames at 2560×1440; FP 0.47 s/frame, C3P
  0.43 s/frame; peak managed memory 1,130 MB. (Production forecast at
  this rate: ~4.8 h for the 20-min film — slower than the Angle's
  0.28 s/frame because the halted fight keeps ~2,300 muskets' smoke
  on screen continuously.)
- **Scrub probes (frames 150/600, re-posed out of order):** logical
  3,589-slot state + 10-float camera pose **bitwise identical 2/2**;
  re-rendered pixels 3.84%/4.26% differing at max channel delta 8 —
  well inside the documented Phase 8 envelope (12 / 8%).
- **Audio events:** 115,942 resolver-confirmed musket discharges, 301
  audible-range casualties, 266 observer footfalls, 0 cannon / 0
  strikes / 0 crossings (the artillery-silent gap and the no-fence
  site, both disclosed above). 30 s stems build byte-deterministically
  (hashes in `stems/stems.sha256`).
- **Proof media:** `iverson-proof-fp-30s-1440p.mp4` 44.9 MB,
  `iverson-proof-c3p-30s-1440p.mp4` 35.6 MB (libx264 slow CRF 18,
  GOP 30 = 1 keyframe/s — the production media-contract setting —
  AAC 192k mix muxed); sha256 in `iverson-proof-media.sha256`;
  frame-count-verified before encode.
- **Smoke density note for the gate:** by mid-fight the field reads
  through a dense powder haze (see `iv-still-6100-destruction-fp.png`
  vs the clear `iv-still-5840-advance-fp.png`). This is the §9.1
  accumulation model doing what the record describes over a ~15-minute
  2,300-musket exchange, but the owner should judge whether the peak
  reads as atmosphere or as fog-wash; the accumulation ceiling is a
  tunable if it reads wrong (flagged as an owner check, not defended).
- **Suite state at evidence time (floors in parentheses):** tool
  **119** (119) · pipeline **66** (59) · reconstruction **158+1**
  (~149) · Unity EditMode **436+1** of 437 (429+4 = 433 total; +4 new
  Iverson bundle tests, and the 4 Angle-bake-conditional environment
  tests RAN here — the bake was regenerated in the worktree) · Unity
  PlayMode **20+1** of 21 (21; the skip is the production-media-
  conditional full-1440p seek test — that media is a gitignored
  release artifact).
- **Seek measurements:** the proofs are encoded with the production
  GOP; actual decoder seek latency on full-length 1440p media is a
  production-render measurement (the P1 numbers — median 10.2 ms,
  worst 19 ms on this machine class — carry until then). The media
  contract's structural requirement is designed in and tested: the
  viewpoint window ends 10 s inside the bundle slice, so the +0.5 s
  end-guard pad past t1 is renderable
  (`test_viewpoint_window_inside_bundle_slice`).
