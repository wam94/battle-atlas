# The Angle — Editorial Decisions (Reconstruction V2, Phase 5)

This document records every editorial choice the canonical Angle reconstruction
(`reconstruction/canonical/angle.reconstruction.json`, slice `t=8040..9000`,
15:14–15:30 on the canonical clock) makes where witnesses disagree, and every
connective-reconstruction rule the compiler relies on. Per plan §16: where
sources conflict we implement ONE canonical choice and record the disagreement
here; we never invent history and never build scenario switching.

Claims are in `reconstruction/claims/angle.claims.json`; sources in
`reconstruction/sources/sources.json`. The battle clock is seconds from 13:00
(t=0); 15:25 = t=8700.

---

## Canonical choices where witnesses disagree

### ED-1 — Step-off time and the slice clock frame

Witnesses put the step-off anywhere from ~1:50 PM (Alexander, self-hedged) to
2:30 PM (Jacobs) to a ~3:00 PM frame (Haskell's cannonade end; modern ABT
sheets) — a full hour of honest disagreement
(`claim-stepoff-alexander/-jacobs/-haskell`).

**Canonical choice:** step-off ~15:05 (t=7500), the frame the macro battle file
already adopts (`claim-stepoff-canonical`). Every slice arrival time is
arithmetic from it: road fences ~15:16 (t=8160), wall approach ~15:23
(t=8580), wall crossing ~15:25 (t=8700). **Rationale:** it is anchored to the
one watch-checked Union time in the corpus (Haskell's cannonade end "at three
o'clock almost precisely") and matches the modern reconstruction frame; and it
keeps V2 consistent with the shipped macro file rather than silently re-timing
the whole battle.

### ED-2 — Armistead's wall-crossing time

No witness gives a clock for the crossing (verified negative: the Hartwig/NPS
post has no times; the ABT frame puts the climax in its 3:45–4:15 sheet).
Readings therefore span ~15:25 (step-off arithmetic) to ~15:30+ (other
step-off readings) to ~15:45+ (ABT frame) (`claim-repulse-clock-frames`).

**Canonical choice:** t=8700 (~15:25), envelope 8640–8760, matching the macro
file's documented keyframe. **Rationale:** it is the only choice consistent
with ED-1's frame; the event (not the clock) is what two independent
eyewitnesses document (`claim-armistead-crossed-wall`). The claim's time
envelope carries the uncertainty; the clock is always an *inferred time on a
documented event*.

### ED-3 — Cushing's death wound

Fuger: "shot in the mouth and instantly killed" at ~100 yards; Haskell: "shot
through the head." Modern synthesis (NPS): three wounds in sequence, fatal one
through the mouth; both period accounts may describe the same sequence
loosely, but they are not reconcilable at the word level
(`claim-cushing-death`, `claim-cushing-earlier-wounds`).

**Canonical choice:** multi-wound sequence with the fatal shot as the column
closed (t≈8660), per the modern NPS synthesis; both period details ride the
claim as competing references. **Rationale:** the only reading that does not
throw away either eyewitness. Fuger's self-interested embellishments
(Hartwig's judgment, McDermott's direct dispute) are flagged on the source
record, and his timing conflict with Webb's report (wounded much earlier,
functioning longer) is its own claim.

### ED-4 — The 72nd Pennsylvania: crest or wall

Haskell has the 72nd firing from the crest and refusing/not hearing Webb's
charge order; the regiment's veterans sued (and won) to place their monument
at the wall (`claim-72pa-crest`, `claim-72pa-wall-counter`).

**Canonical choice:** the crest reading — the 72nd fights from the crest
through the crisis and advances to the wall only as the penetration collapses
(after the slice's end, t≈9300). **Rationale:** consistent with Webb's Medal
of Honor record (wounded trying to lead the regiment forward) and with the
macro file; Haskell is partisan here, which is flagged, but the monument
litigation is memory politics forty years on. The counter-reading is encoded
as a claim and must surface in any provenance UI.

### ED-5 — Who reached the works (Virginia vs North Carolina)

Fry insists all five of his regimental colors reached the Union works;
Virginia-centric accounts (and Bachelder's High-Water-Mark framing) stop
Pettigrew's front short (`claim-fry-colors-at-works`).

**Canonical choice:** Fry's front reaches the wall north of the inner angle
(documented endpoint kept), wrecked there alongside Garnett's left; no
crossing east of the wall is authored for Fry. **Rationale:** Fry's claim is
first-person, specific, and partially supported by modern scholarship; giving
his brigade the wall but not a breach honors both his account and the absence
of any documented Pettigrew-front penetration at the Angle.

### ED-6 — The Emmitsburg Road: fences, gaps, and the missing road layer

There is no traced road-surface layer; the corridor is the span between the
two traced post-and-rail fences (`claim-fence-west/east-structure`). No
source in the corpus locates specific gaps or breakage along those fences
(`claim-fence-gaps-unknown` — an explicit `unknown` claim, per the plan's
rule that missing evidence is recorded, not painted over).

**Canonical choice:** crossings are modeled as climb-over along the unit
frontage, with no invented gap geometry. Each brigade's road passage is an
authored `cross_obstacle` segment naming BOTH fence features (`obstacleIds`,
the recorded schema deviation), with a halt-and-dress pause in the roadbed
for the first line (Garnett; macro keyframes hold the brigade at the road
8160–8280) and a briefer passage for the follow-on lines.

### ED-7 — Arnold's battery withdrawal timing

Whether Arnold's four guns withdrew before or during the charge is genuinely
debated; one left-section gun demonstrably fired double-shotted canister from
the wall (`claim-arnold-canister`).

**Canonical choice:** the battery unit holds its traced position with a fire
window through the repulse (matching the macro track, which does not model
the partial withdrawal). The dispute rides the claim note; no withdrawal
segment is authored because neither timing reading is strong enough to move
traced geometry.

### ED-8 — What "Armistead's brigade" does after the crossing

Only ~150 men crossed with Armistead (`claim-armistead-party-strength`); the
brigade as a body was stopped at and west of the wall, and its 643 "missing"
(mostly captured) never marched back. The macro centroid nevertheless returns
west (t=9000 at x=4250), because a single-centroid track must put the
surviving body somewhere.

**Canonical choice:** the brigade breaches at t=8640–8700 (centroid crossing
the wall, the documented event) and falls back west across the wall
8700–9000. The fall_back segment names the wall in `obstacleIds` — an
authored recrossing, justified by the inference rule that the centroid
follows the surviving unwounded body, most of which never crossed; the
crossed party's fate (dead, wounded, captured at the guns) is carried by the
casualty profile, not by inventing a second track.

### ED-9 — Strength figures and the 72nd's two numbers

Stone Sentinels strength figures are monument-committee compilations
(inferred tier, per the OOB research pass), and several are scope-dependent
(72nd PA: 458 "present at Gettysburg" vs 473 "present for duty",
`claim-strength-72pa`). **Canonical choice:** the macro file's adopted values
(Garnett 1,480; Kemper 1,575; Armistead 1,650; Webb 1,250; 69th 258; 71st
331; 72nd 458; Cushing 126 documented via the MoH record) are kept so V2
reconciles with the shipped strength decay; each figure's tier and conflicts
ride its claim.

### ED-10 — Wind and weather

No source in the corpus attests wind (`claim-wind-unknown`, assessment
`unknown`); the only documented atmosphere fact is Fuger's dense smoke at the
wall (`claim-smoke-visibility`). The macro file's light-SW-breeze environment
block remains explicitly unattested by its own note. Nothing in this slice's
compiled output depends on wind.

---

## Connective-reconstruction rules (named inference rules)

Segments cite these rules by name in `inferenceRules`. A segment with no
claim citations MUST name at least one rule and is compiled with `editorial`
provenance.

- **R-route-fence-normal** — where a route must cross a traced fence/wall and
  no gap is documented, the authored crossing point is the intersection of
  the unit's macro track with the traced obstacle line (climb-over along the
  frontage, ED-6).
- **R-crossing-duration-drill** — obstacle-crossing durations are
  reconstructed from frontage and drill behavior: a brigade line takes
  roughly 20–60 s to pass a post-and-rail fence (halt-compress-cross-redress),
  longer (100–120 s with an authored halt) when a documented dress pause
  exists at the obstacle.
- **R-macro-track-geometry** — segment routes start from (and never contradict
  by more than the claimed uncertainty) the existing macro/child-regiment
  keyframe geometry in `app/Assets/Battle/gettysburg-july3.json`; any larger
  departure requires a new editorial decision here.
- **R-linear-connective-motion** — between documented positions, motion is
  constant-pace along the authored route (the semantic replacement for the
  macro file's straight-line interpolation); pace must stay inside
  arm-appropriate limits (infantry advance ≤ 1.5 m/s, double-quick ≤ 2.7 m/s,
  crossing ≤ 1.25 m/s, fall back ≤ 2.0 m/s, rout ≤ 3.5 m/s; artillery
  displacement by hand ≤ 0.5 m/s).
- **R-strength-interp-endpoints** — slice-start strengths are the macro
  track's linear interpolation at t=8040, rounded to whole men; casualty
  profiles integrate exactly to the macro keyframe strengths inside the
  slice (reconciliation is validator-enforced).
- **R-casualty-curve-shape** — where only aggregate losses are attested, the
  within-window distribution follows the cited casualty-timing claims
  (rising into canister range per `claim-cas-canister-gaps`; uniform where
  nothing narrows it). Victim identity is never assigned (plan §6.4).
- **R-hold-position-drift** — units attested in place hold their claimed
  position; sub-20 m drifts in the macro track (line dressing, crowding)
  are followed without new claims.
- **R-facing-from-route** — facing is derived from route bearing while
  moving; static segments inherit authored or previous facing; authored
  wheels interpolate shortest-arc (Stannard's change of front).
- **R-retreat-recross** — repulsed units recross road fences/walls where
  their documented start and end positions require it (fall_back/rout
  segments name the obstacles; no gap geometry invented).
- **R-fire-window-from-events** — fire actions (fire_by_rank,
  fire_independent, take_canister) take their windows from the macro file's
  cited engagement events and the firing claims; rate-of-fire and rhythm are
  Phase 8 concerns, not encoded here.

## Schema deviation, recorded

Plan §6.3's illustrative segment shape has a singular `obstacleId`; this
implementation uses `obstacleIds` (array) because one historical road
crossing traverses two traced fences (west and east). Recorded in the schema
description and here.

## What this phase deliberately does not decide

- Per-regiment strength splits for the CSA brigades ("equal fifths" stays an
  inference in the macro child tracks; V2 segments stay at brigade grain for
  the Confederate side, per the plan's unit list).
- Fence-gap locations, the road surface, and ditch geometry (no source yet;
  ED-6).
- Wind (ED-10), exact fire discipline, and casualty cause splits beyond the
  broad `causeMix` classes — the causeMix fractions are reconstruction
  (`assessment: "reconstructed"`) informed by the canister/musketry claims,
  not documented ratios.
