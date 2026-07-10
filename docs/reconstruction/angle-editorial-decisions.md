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

## Phase 7 environment decisions (ED-11 … ED-18)

Phase 7 stages the local crop's physical environment. Every staged feature
traces to a claim (see the Phase 7 additions in `angle.claims.json`) or to a
decision below. Geometry lives in `pipeline/terrain_pipeline/environment.py`
(baked to `data/heightmap_angle/environment.json`) and is staged by
`app/Assets/Editor/Phase7/AngleEnvironmentStage.cs`.

### ED-11 — Emmitsburg Road profile

Documented: unpaved packed earth (`claim-road-surface`, inferred period norm)
in a sunken bed men sheltered in (`claim-road-sunken`); no measured depth and
no documented ditches at this stretch.

**Decision:** the roadbed is carved into the crop terrain as a 0.45 m
depression at the corridor centerline, easing to zero at the traced fence
lines; no ditches are modeled. The corridor and its width come from the span
between the two traced fences (ED-6) — there is still no road-surface layer
to trace. Wheel-path wear is painted as two disturbed-soil stripes at ±0.75 m
from the centerline (period road use is documented generally; exact ruts are
not — reconstruction).

### ED-12 — Fence and wall construction profiles

Documented: "high post and rail fences" climbed in order
(`claim-fences-high-climb`); a low fieldstone wall ~2–3 ft patched with rails
and earth, "breast-high" to the attacker (`claim-wall-profile`,
`claim-wall-rails-on-top`). No primary measurement of either.

**Decision:** road fences are modeled as five-rail post-and-rail at 1.5 m
(NPS modern reconstructions along the road as physical reference); the
interior post-and-rail fence west of the road uses the same form; the two
traced worm fences are zigzag rail at 1.2 m. One trace conflict is
resolved here: `fence-post-and-rail-west-of-road` converges INTO the road
corridor at its south end — georeferencing drift on a feature whose own
trace note places it west of the road — so the staged run terminates at
the corridor edge rather than inventing a fence across the road that the
crossing claims exclude. The wall is 0.75 m of
double-faced fieldstone, ~0.6 m wide, with scattered rail lengths laid on top
along the 69th PA front south of the inner angle (documented) and none north
of it. No gaps or breaks are modeled (ED-6 stands).

### ED-13 — Codori buildings massing

Documented: two-story frame house, brick addition postwar
(`claim-codori-house-structure`); wartime barn replaced 1882, form evidenced
only by the period photograph (`claim-codori-barn-1863`); barn anchor at
(4020.4, 4635.4) with replacement-barn caveat (`claim-codori-barn-position`);
HABS has no Codori coverage — Trostle barn PA-1962 is the period analogue.

**Decision:** the house is staged as a plain two-story gabled frame block,
9.0 × 7.5 m, eaves 5.5 m, unpainted-light siding (color unknown), long side
facing the road at (4005, 4646) — 19 m WNW of the barn anchor, between
barn and road (the landmark-anchor note places the untagged farmhouse
10-20 m from the barn in the same farmyard). The barn is a Pennsylvania bank barn form after the
Trostle HABS analogue scaled to the period photo's plain massing: 18 × 11 m,
eaves 6.5 m, gable roof, no cupolas, long axis parallel to the road bearing.
Both are explicit reconstruction-grade massing — documentary silhouettes, not
replicas. No other farm outbuildings are staged (none sourced).

### ED-14 — Copse representation

Documented: sapling-scale scrub — 50+ oaks mostly ≤ 2 in diameter, sassafras
and oak per Haskell, chestnut-oak photo reading carried, extent larger than
the modern fenced copse, unfenced in 1863 (`claim-copse-form-1863`,
`claim-copse-unfenced-1863`).

**Decision:** the copse is planted across the full traced polygon with ~30
small broadleaf trees at 3–6 m height (the CC0 `island_tree_02` derived
asset standing in for scrub oak/sassafras — species-exact models do not
exist in redistributable form), denser toward the polygon center, no fence.
Ziegler's Grove (traced) uses the same asset scaled 7–9 m (mature grove).

### ED-15 — Field cover assignment

Documented: grass between road and wall (`claim-field-east-grass` — this
retires the Phase 3 placeholder wheat band east of the road); a mixed
patchwork west of the road — wheat nearly ripe, grass/pasture, corn,
orchards (`claim-fields-west-mixed`); parcel-by-parcel identity undocumented
(McElfresh's copyrighted map is the purchasable verification path, facts
only, never traced).

**Decision:** east of the road: pasture/dry-summer-grass blend, no crop
geometry. West of the road: the traced `field-west-of-emmitsburg-codori-front`
polygon is assigned wheat-stubble-with-standing-wheat patches (Haskell's
"nearly ripe" wheat placed here as the most road-adjacent cultivated parcel);
other traced fields in the crop render as dry summer grass; orchard floors
as pasture. Corn is NOT staged (no parcel evidence at all inside the crop).

### ED-16 — Cushing's battery layout at 15:20

Documented: six 3-inch Ordnance rifles (`claim-cushing-armament`); two
serviceable guns run to the wall pre-slice (`claim-cushing-guns-to-wall`);
the rest disabled; battery traced at (4398, 4880) facing 262° with 80 m
frontage (macro keyframes); "about 30 paces" wall-to-crest relation
(`claim-wall-profile`, Peyton); per-piece limber + caisson organization
(`claim-battery-organization`); wreckage vocabulary (`claim-corridor-trampled`).

**Decision:** two intact guns at the wall at (4400, 4868) and (4401, 4890)
facing 262°; four pieces staged as disabled at the crest line 28 m east of
the wall (the 30-paces relation), two of them dismounted/wrecked; limbers
6 yd and caissons a further 11 yd behind the crest line per drill-manual
spacing (reconstruction — the manual gives the norm, not this hour's
disorder); ammunition chests and scattered implements between wall guns and
crest. Dead battery horses are Phase 8 casualty work, not staged here.

### ED-17 — Trampling and ground disturbance in-slice

Documented: the corridor's post-battle trampled state (`claim-corridor-trampled`).
The in-slice progression is not witnessed hour-by-hour.

**Decision:** a trampled-grass corridor is painted from the road crossing to
the wall across the assault frontage (macro tracks' swept band, z≈4600–5000),
blended dry-grass/disturbed-soil; wheel-path wear on the road per ED-11.
This is a static Phase 7 approximation; **Phase 8 owes trampling driven by
the compiled troop paths** (recorded as a carried item).

### ED-18 — Atmosphere and grade

Unchanged from the Phase 3/4 decisions: 15:20 LMT ephemeris sun (t=8400),
physically based sky, fixed EV100 13.2, ACES tonemap, distance haze with
900 m mean free path (historically supportable July haze; Fuger's dense
smoke is combat smoke, Phase 8). No additional color grade is introduced in
Phase 7; if one lands later it must be documented here.

---

## Phase 8 action decisions (ED-19 … ED-21)

### ED-19 — Wind for smoke drift

Documented: nothing (`claim-wind-unknown`; ED-10 stands — no compiled unit
output depends on wind). Fuger attests dense smoke hanging at the wall
(`claim-smoke-visibility`).

**Decision:** the Phase 8 smoke field needs SOME drift vector to avoid
reading as fixed set dressing. The reconstruction uses a deliberately weak
southwest breeze, 1.1 m/s toward the NE (`BlackPowderVfx.WindMps`) —
consistent with the macro file's unattested light-SW-breeze block and weak
enough that firing-line smoke accumulates and hangs, matching Fuger. This
is reconstruction, not evidence; if a sourced wind observation for the
charge hour ever surfaces, it replaces this constant.

### ED-20 — Fire discipline cadence

Documented: which units fired and how (`fire_by_rank` / `fire_independent`
segments trace to the OR/Haskell/Fuger claims). Not documented: any unit's
exact per-man cadence.

**Decision:** the resolved fire/reload cycle is aim (1.4 s) + fire (0.9 s)
+ the kit's nine-stage reload (20 s) + ready pause (2 s) ≈ 2.5 rounds/min —
the drill-manual rate for trained infantry under stress. `fire_by_rank`
staggers ranks half a cycle apart (rolling rank volleys, ±0.45 s raggedness
inside a rank); `fire_independent` staggers each man by slot hash across
the whole cycle. Cushing-sector guns cycle 16–21 s between discharges
(canister at close range, reduced crews). All cadence constants live in
`FireCycles` and are reconstruction.

### ED-21 — Casualty distribution to individual figures

Documented: aggregate profile counts, windows, curves, cause mixes (the
Phase 5 corpus). Not documented: which man fell when.

**Decision:** victim k of a profile falls at the inverse-CDF midpoint
quantile of the profile's pinned intensity curve, so the on-field alive
count tracks the compiled per-second strength exactly at profile boundaries
(within one man mid-window; rounding-tie artifact, tested). Victims,
causes, fall directions, and the wounded-crawl minority (~22% of fallen)
are hash-selected — never named, never re-rolled. Full rules and the
wound-vocabulary limits: `docs/reconstruction/violence-and-representation.md`.

### ED-22 — The Soldier View observer is exempt from the casualty draw

Documented: nothing about any individual man at the viewpoint's position
(the observer is representative by design, plan §6.5). Problem: ED-21's
hash-based victim selection could deterministically assign one of the
unit's reconstructed fates to the observer slot itself, which would (a)
implicitly assert "the man HERE fell at THIS second," a per-person claim
the aggregate evidence never makes, and (b) leave the camera lying in the
wheat for the remainder of a fixed, unskippable viewpoint window.

**Decision:** the observer slot of every committed viewpoint
(`ViewpointObservers.ProtectedSlots`, pinned against `viewpoints.json` by
test) is excluded from victim selection in `CasualtySchedule.Compile`.
The unit's casualty totals, windows, curves, and reconciliation with
compiled strength are unchanged — another slot draws the fate the
observer would have drawn. Nothing else about the slot is special: same
drill, same formation position, same reactions to nearby fire. The
exemption is disclosed to the user in the viewpoint's editorial note and
in the representative-observer text
(`docs/reconstruction/soldier-view-content-warning.md`). Synthetic `dev-`
fixtures are not observers and are not protected.

### ED-23 — What the viewpoint's soundscape may and may not contain

Documented: the engagement's weapon types, the units firing and when (the
compiled fire segments), the bombardment before the slice, wounded men on
the field. Not documented: any specific utterance, order wording, or
per-man sound.

**Decision:** the audio stems are driven ONLY by the compiled event
streams the visuals already render from — every musket report is one
resolved discharge, every cannon report one scheduled shot, delayed by
distance at 343 m/s and attenuated with range. Consequences accepted for
fidelity: Garnett's brigade has no compiled fire segment in this window
(`take_canister` at the wall), so the viewpoint carries NO friendly
musketry or reload foley — the observer's line takes fire without
returning it, which is what the reconstruction says. Shouted orders are
generic period drill commands from unattributed voices keyed to segment
transitions (no invented named-person dialogue, §9.3); wounded voices are
sparse, sober, and tied to scheduled nearby casualties (§9.2). No
pre-slice bombardment residue is synthesized (the P8 "no pre-slice smoke"
limitation applies to audio equally), and the long-range guns that cause
the approach-phase shell casualties sit OUTSIDE the staged bundle — the
soundscape's first artillery is the staged wall batteries opening, so
the approach reads artillery-silent (recorded gap; a Phase 10 distant
layer would need its own sourced event basis). Ambient wind/insect beds
are the only unmodeled-continuous layers. Smoke-based muffling is NOT applied —
at these ranges black-powder smoke has no acoustically justifiable
attenuation (§9.3 "only where acoustically justifiable").

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
- Fence-gap locations (still no source; the road surface/profile is now the
  ED-11 reconstruction, and ditch geometry remains unmodeled) (ED-6).
- Wind (ED-10), exact fire discipline, and casualty cause splits beyond the
  broad `causeMix` classes — the causeMix fractions are reconstruction
  (`assessment: "reconstructed"`) informed by the canister/musketry claims,
  not documented ratios.
