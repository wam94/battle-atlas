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

**Revisited 2026-07-10 (audit R1 adoption) — ED-1 STANDS.** The R1 pass had
seen, one hop removed at the Wikipedia-citation layer, a possible
14:00–14:30 modern center of gravity (Coddington, Hess, Sears, Wert cited
together for "about 2:00 p.m."). The owner's adoption of the R1 proposals
was evidence-contingent ("adjust the step-off to match our research"), so
the four scholars were verified at the page level and the primary spine
re-verified from full texts
(`docs/reconstruction/audit/verification-2026-07-10.md`). Findings:

- The four-scholar consensus **does not exist**: Coddington p. 502 reads
  "At 3:00 p.m. officers and men of Hancock's Second Corps looking west
  saw a long gray line suddenly emerge…" — the Wikipedia footnote
  misreports him. Hess (step-off ~14:00, repulse "between 2:30 and 2:45
  P.M.") and Wert (one-hour cannonade from ~13:05, on ammunition
  arithmetic) verify as the genuine short-cannonade school. Sears could
  not be page-verified through any anonymous channel and is recorded
  attributed-only.
- New primary evidence fetched this pass **supports the shipped frame**:
  Hancock's OR report (cannonade "About 1 o'clock … After an hour and
  forty-five minutes, the fire of the enemy became less furious, and
  immediately their infantry was seen"), McGilvery's OR report (fire
  "about one hour and a half"; attacking lines crossing "at about
  3 p. m."), Haskell's full-text "two mortal hours" ending "at three
  o'clock, almost precisely", and the NPS/ABT ~15:00 institutional frame.
- The honest counter-evidence is likewise recorded: Jacobs — the chain's
  own precision standard — puts the step-off at **14:30** ("When 2½ p. m.
  came… two long, dark, massive lines"), and Alexander's 13:25/13:40 note
  chain plus "doubtless 1.50 or later" rides an early clock the
  short-cannonade school follows.

**Ruling:** the literature is a genuine two-school split, not an earlier
consensus; the shipped 15:05 sits at the late-but-occupied pole (with
Coddington, Haskell, Hancock, McGilvery, NPS, ABT), and re-timing the
entire shipped reconstruction on a split this even would violate the
never-silently-re-time doctrine ED-1 was built on. Step-off stays
**15:05 (t=7500)**; the envelope stays wide (13:50–15:10) and now carries
the page-verified counter-readings (Hess ~14:00; Wert ~14:05–14:15; Jacobs
14:30) as first-class conflict records on CA-J3A-6 (ED-24). Preconditions
for ever revisiting again: a page-verified Sears, and Stewart's 1959
microhistory (both listed in the verification dossier's honesty ledger).

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

**Addendum 2026-07-10 (audit R1 adoption): the staging seed is pinned.**
The battle seed for every hash-drawn staging decision was previously the
bundle's content checksum — which meant a recompile that only touched
provenance metadata (source edition notes, ED-25 clock profiles) silently
re-rolled every victim draw, step phase, and yaw, desynchronizing the
committed captions from the audio baked into the shipped media (caught by
`test_committed_captions_match_the_event_export` during the ED-25
implementation). That violates this decision's "never re-rolled" rule.
The compiler now emits `stagingSeed`, **pinned at the checksum of the
bundle the owner reviewed and shipped**
(`d470c469…`, the Phase 8–10 media / P12 release input), and all staging
entry points seed from it (`AngleActionStage`, the P9 audio-event export,
the P10 teleport probe). The pin is enforced by an EditMode test and a
corpus test; moving it is a deliberate editorial decision to be made only
when choreography content actually changes — never a recompile side
effect.

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

## Audit R1 adoption rulings (ED-24 … ED-31) — 2026-07-10

Owner ruling: the R1 proposals
(`docs/reconstruction/audit/anchor-chain-proposal.md`, drafted on branch
audit-research-1) are ADOPTED under an adopt-and-adjust doctrine, with the
step-off question gated on page-level verification. The verification
record is `docs/reconstruction/audit/verification-2026-07-10.md`; the
step-off gate resolved as **ED-1 STANDS** (see the ED-1 revisit above).
Anchor ids `CA-<phase>-<n>` used below are defined in the proposal.

### ED-24 — The canonical anchor chain; July 3 afternoon adopted

The `CA-<phase>-<n>` anchor-chain structure and the July 3 afternoon chain
(proposal §1.1) are adopted as ruled, with these verification-pass updates:

- **CA-J3A-1 (signal guns) adopted 13:07** — Jacobs's clock reading
  (re-verified from the 1864 full text) beats the three round-number
  "one o'clock" watches; Longstreet's report-nominal "about 2 p.m." stays
  the recorded outlier (ED-25 rule 4).
- **Shipped-skew handling (the 13:00-vs-13:07 label question):** the
  proposal's recommendation is adopted — the shipped slice clock keeps
  `startTime 46800` as a *nominal* 13:00 and CA-J3A-1 records
  `shippedT: 0`, `shippedSkewMinutes: -7`. Nothing in the shipped slice
  (t=8040..9000) depends on the first seven minutes; **no metadata
  relabel** (relabeling would re-time every derived wall clock in the
  captions for zero evidentiary gain).
- **CA-J3A-4 (Union fire slackens) ~14:30, envelope 14:00–15:00** — now
  corroborated by Hancock's OR "After an hour and forty-five minutes, the
  fire of the enemy became less furious" (~14:45 nominal) and McGilvery's
  "about one hour and a half"; the Hess/Wert short-cannonade pages ride
  the envelope's early edge as verified counter-readings.
- **CA-J3A-5 (cannonade ends) 15:00** — Haskell's watch-checked end,
  re-verified from the full text ("At three o'clock, almost precisely");
  counter-readings recorded (Hess ~14:00 page-verified; Sears 14:30
  attributed-only; Smyth report-nominal).
- **CA-J3A-6 (step-off) = ED-1, 15:05 (t=7500)** — reaffirmed after the
  gate verification; the page-verified counter-evidence (Hess ~14:00, Wert
  ~14:05–14:15, Jacobs 14:30) is carried on the anchor as first-class
  conflict records. Downstream anchors CA-J3A-7..10 remain expressed
  relative to CA-J3A-6.
- **CA-J3A-9/10** — unchanged (= ED-2/ED-8 frames); McGilvery's verified
  "at about 3 p. m." advancing-lines quote joins CA-J3A-7/10's evidence.

### ED-25 — Per-source clock profiles (`clockProfile`)

The schema, assessment method, and worked profiles of proposal §3 are
adopted and implemented: optional `clockProfile` object on
`reconstruction/sources/` records (schema-enforced, validator-checked:
envelope must bracket the offset, anchors must be `CA-` ids, non-`none`
kinds require offset + assessment). Semantics: `canonical time = stated
time + offsetMinutes`; tier C for claims riding a corrected clock;
**rule 4: report-nominal clocks never define or move an anchor** — their
profiles exist so provenance UI can display the skew, not to promote the
source. Profiles landed with this ruling: Jacobs
(contemporaneous-civilian, 0, [−2,+2], strong), Haskell (watch-checked,
+7, [+2,+12], medium), Alexander (retrospective-watch, +7, [0,+15],
medium capped by 44-year distance), Longstreet's report (report-nominal,
−53 raw, never corrects), Hays's report (report-nominal, ~0 — the
agreeing example), Stone Sentinels tablets (tablet-adjudicated, 0, ±30,
with the **McLaws-wing ±60 early-skew exception** per ED-28), and the
four OR 27/1 Union reports verified this pass (Hancock, Howard, Greene,
McGilvery — report-nominal; Hancock's July 3 clock demonstrably sits
within ~7 min of the chain). Timestamped dispatches (Buford 10:10,
Hancock 17:25) are **document pins, not witness clocks** — they carry no
profile and anchor at ±0 by document class.

### ED-26 — July 1 morning chain (PROVISIONAL)

Proposal §2.1 adopted as provisional. Upgrade from this pass: **CA-J1M-3
(Buford's 10:10 dispatch) is now VERIFIED as printed** (OR 27/1 p. 924,
with the gdg.org transcription and Coddington's endnote concurring) —
tier: timestamped document, ±0; the morning's hardest clock. Carried
preconditions: Heth's deploy-vs-attack split (his report verified
time-silent on the attack), the Howard-assumes-command sub-anchor —
note Howard's own report (verified) says "about 11.30 a. m.", vs Pfanz's
10:30; rule at adoption.

### ED-27 — July 1 afternoon chain (PROVISIONAL)

Proposal §2.2 adopted as provisional, with three verification upgrades:

- **CA-J1P-3:** the Early-tablet "about noon" suspicion is RESOLVED — the
  tablet reads "arrived about noon within two miles of Gettysburg by
  Harrisburg Road" (vicinity, not the field); the ~15:00–15:30 attack
  window stands untouched.
- **CA-J1P-4:** the XI Corps tablet's "About 4 P. M. the Corps was forced
  back" is verified verbatim, and its "10.30 A. M." attaches to Schurz's
  division specifically (vs Howard's report "Schurz joined me before
  12 m." — conflict recorded). Howard's report adds the verified ladder
  16:00 order → 16:10 positive order → 16:30 columns on Cemetery Hill.
- **CA-J1P-7/8:** the Hancock-arrival controversy is now primary-verified
  at both poles (his report "At 3 p. m. I arrived" vs his own 17:25
  dispatch "When I arrived here an hour since" ⇒ ~16:25, and Howard's
  "At 4.30 p. m. … General Hancock came to me about this time"); the
  proposal's ~16:15 adoption with full envelope and partisan-dispute note
  stands, strengthened. **CA-J1P-8 (Hancock's dispatch) VERIFIED as
  printed "5.25 [P. M. …]"** (OR 27/1 p. 366; bracket caveat recorded:
  the P.M./date are the OR editors') — tier: timestamped document.

### ED-28 — July 2 afternoon chain (PROVISIONAL)

Proposal §2.3 adopted as provisional, including the Longstreet step-off
ruling (16:00–16:30; the dawn-attack-order claim documented as polemic,
not clock) and the **McLaws-wing tablet-skew rule** (tablet clocks on that
wing run ~1 h early; treated as their own clock-offset class, ED-25).
Carried precondition: the Little Round Top verification pass
(Pfanz/Norton/Oates; Chamberlain's OR).

### ED-29 — July 2 evening chain (PROVISIONAL)

Proposal §2.4 adopted as provisional. Upgrade: **Greene's OR report is now
verified** — "we were attacked on the whole of our front by a large force
a few minutes before 7 p. m." (OR 27/1 p. 856) — a primary Union clock
agreeing with the Johnson-tablet "dusk" and Pfanz ~19:00; CA-J2E-2's 19:00
start is the best-corroborated evening anchor alongside Hays's "A little
before 8 p.m." (CA-J2E-3). **2026-07-11 update: hardened by ED-62** (see
§"Dossier pass 8 adoption rulings") — E-1/E-2/E-3 now on executor
primaries, both armies; no times or envelopes moved.

### ED-30 — July 3 morning chain (PROVISIONAL)

Proposal §2.5 adopted as provisional, including the Spangler's Meadow
~10:00 ruling direction. Carried precondition: Pfanz *Culp's Hill* pp.
~340–355 before CA-J3M-3 hardens. **2026-07-11 update: hardened by the
pass-9 executor layer** — CA-J3M-1 at author-primary basis with the
opening-cluster annotation (ED-65), CA-J3M-2's waves clocked by their
executors, CA-J3M-4 receiving-side corroborated; **CA-J3M-3 upgraded
to a structured two-pole primary conflict record by ED-64 (see
§"Dossier pass 9 adoption rulings") — its ~10:00 ruling direction
stays PROVISIONAL and the Pfanz precondition STANDS.**
**2026-07-13 update: the ED-30/ED-64 Pfanz-gate BLOCK on authoring is
SUPERSEDED by the owner's ED-78 ruling** (see §"July 3 morning slice
ruling (ED-78)") — CA-J3M-3 is now authorable at its ~10:00 direction
as PROVISIONAL, both poles carried verbatim; the July 3 morning phase
is authored in full (`docs/reconstruction/audit/july3-morning-slice.md`).
The Pfanz fetch itself remains open (a hardening target, not a
precondition any longer).

### ED-31 — Time-frame convention

Adopted as proposed: the battle clock is **Gettysburg local mean time**
(~9 min ahead of modern EST); astronomical pins sunset July 2 19:29 LMT
and sunrise July 3 ~04:38 LMT; all imported modern-frame times (EST/EDT
artifacts like "19:41" / "05:45") are converted on entry and flagged.
Follow-up carried: USNO-grade ephemeris recomputation for 39.82°N 77.23°W,
July 1–3 1863 (cheap script; removes a whole class of frame bugs).

---

## Dossier pass 1 + spatial program adoption rulings (ED-32 … ED-39) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 1 (`docs/reconstruction/audit/dossier-pass-1.md`
§4, drafted on branch audit-dossiers-1) and the spatial program's monument
reliability profile (branch audit-spatial-1) are ADOPTED as recorded
below. One renumber: the spatial program had provisionally used "ED-32"
for the monument profile, colliding with pass 1's Stannard ruling — the
monument profile is adopted as **ED-39** and renumbered in
`monument-register.md` / `spatial-evidence.md`.

### ED-32 — Stannard's brigade engaged-strength basis: 1,788

Readings: 1,950 brigade aggregate (stone-sentinels brigade page — failed
re-verification in pass 1: the figure could not be found again on the
cited page) vs the regiment-grain sum of the three engaged regiments,
13th VT 480 + 14th VT 647 + 16th VT 661 = **1,788** (stone-sentinels
regiment pages, re-fetched; 12th/15th VT detached as train guard per
or-27-oob).

**Canonical choice:** 1,788 as the engaged basis, regiment-grain.
**Rationale:** the 1,950 reading failed re-verification while the
regiment figures verified; the official p. 174 casualty return (351,
full K/W/M by regiment) reads plausibly against 1,788 (19.6%); and
regiment grain matches the authored 13th/16th VT flank actions. The
1,950 figure stays in the record as a failed re-verification, per the
OOB doctrine (never silently dropped). Consequence: master-table EC2 for
`us-stannard` adopts 1,788; reconciling the macro file's strength decay
is an authoring-pass item, not a silent edit here.

### ED-33 — The Union slackening is MIXED, and "the 18 guns are gone" gets an adopted identification

The question: what physically constituted CA-J3A-4 (~14:30, the Union
fire slackening), and what did Alexander's second note observe?

**Canonical choice — the mixed reading, with the contemporary-vs-postwar
motive split explicit:**

1. **Ordered economy** — Hunt's contemporary report: "About 2.30 p. m.,
   finding our ammunition running low … I directed that the fire should
   be gradually stopped" (or-27-1-hunt).
2. **Ordered cease on Cemetery Hill** — Osborn's contemporary motive:
   "as no satisfactory results were obtained, I ordered all our guns to
   cease firing" (or-27-1-osborn p. 750). The postwar "lure them in"
   ruse framing (osborn-weeklytimes-1879 as-quoted; Hunt B&L) is carried
   as retrospective reinterpretation, NOT adopted as the contemporary
   motive — the 1863 report says results, not ruse.
3. **Genuine damage and withdrawals** — Brown's B/1st RI ordered out
   ~14:40; Cushing's battery wrecked in place; Arnold/Rorty ammunition
   exhaustion (the Hazard-sector reality that made the "slackening"
   partially true damage, not only policy).

**Adopted identification for Alexander's "the 18 guns are gone"
(CA-J3A-3):** the cemetery-crest group falling silent under Osborn's
cease order — Alexander's first note counts "at least 18 guns … firing
from the Cemetery itself" (alexander-shsp4-1877), and he later recorded
being "incorrectly told it was the cemetery" (alexander-1907) about the
battery movement he saw; Osborn's is the only ordered, observable,
sector-wide silence on that frontage in the window. Brown's ~14:40
withdrawal (the one ordered II Corps battery withdrawal) rides the
ruling as the documented contributing event most plausibly conflated in
the smoke — us-btty-brown.md's options resolve as "(b) primary, (a)
contributing." **Rationale:** the only reading consistent with all three
sectors' executor dossiers at once; every component is verbatim-cited in
pass-1 dossiers.

### ED-34 — Artillery activity classes (authored-track vocabulary)

Three documented July 3 afternoon behaviors become named activity
classes for authored artillery tracks, each with a matching
casualty-curve class:

- **ordered-silence-then-fire** (exemplar: McGilvery's line) — held
  silent by pre-arranged order under the cannonade, then deliberate
  battery-by-battery reply and repulse fire. Casualty curve: light
  silent phase, spike at the reply/repulse.
- **ordered-cease** (exemplar: Osborn's Cemetery Hill group) — fired
  deliberately through the cannonade, ceased by order before its end,
  resumed at the assault. Curve: distributed under the duel, second
  spike at the repulse.
- **fought-to-exhaustion** (exemplar: Hazard's brigade) — fired until
  long-range ammunition was gone, wrecked in place, canister at the
  charge. Curve: flat-then-spike into the crisis.

**Rationale:** the macro file currently renders all three as
undifferentiated "firing"; the distinction is exactly what CA-J3A-4's
wording depends on, and each class now has a fully-dossiered exemplar.
Authoring consumes these classes; this ruling does not itself move any
track.

### ED-35 — McGilvery's July 3 composite line is cast as a command unit

**Canonical choice:** the 39-gun low-ground line enters the cast as a
command unit (`reg-us-ar-1v`, July 3 line), with McGilvery's own OR
roster (p. 883, verbatim, ordered left→right: Ames · Dow · "a New Jersey
battery" · Rank's section · 2nd Connecticut · Hart · Phillips ·
Thompson) as its structure. **Rationale:** the roster is primary and
ordered; Hunt posted and directed the line as a unit; battery-grain rows
hang off it as dossiers reach them (pass 2+). Hunt's B&L "forty-one
guns" stays the carried counter-reading on the gun total.

### ED-36 — Battalion-grain CSA July 3 artillery geometry (the Confederate reference frame)

**Canonical choice:** the Confederate artillery reference frame for the
July 3 reconstruction is the BATTALION line (Alexander / Eshleman /
Dearing / Cabell on the First Corps arc; Pegram / McIntosh / Lane /
Poague / Garnett on the Third Corps arc), anchored on the War Department
tablet lines + Bachelder sheet geometry, with per-battery points
authored only where a battery-grain anchor exists (e.g. Miller ~100 yd
left of the Peach Orchard; Taylor at the Smith house). **Rationale:**
CSA per-battery positions/casualties are mostly attested-unreported
(pass-1 finding); monuments are absent on that side (ED-39 §4);
battalion grain is the finest level the evidence carries army-wide, and
matches the OOB register's battalion rows. Pass-2 dossiers are
commissioned at this grain.

### ED-37 — Rank's section and "a New Jersey battery" on McGilvery's line: the conflict is ADOPTED AS RECORDED; resolution stays OPEN

McGilvery's OR roster (p. 883) places "a New Jersey battery" and "one
section New York [Pennsylvania] Artillery, Lieutenant Rock [Captain
Rank]" (OR editors' brackets) on his July 3 line; Bachelder sheet 8
labels "Rank" there too; the OOB/cavalry record places Rank's Battery H,
3rd PA Heavy at East Cavalry Field the same afternoon.

**Canonical choice:** the conflict itself is adopted as a first-class
record (OR-vs-OOB grade, upgraded from the old sheet-vs-OOB grade); NO
placement ruling is made — the register keeps Rank at East Cavalry
Field, the McGilvery-line dossier carries the roster verbatim, and any
cast of the line under ED-35 carries the two disputed slots as
conflict-flagged. **Resolution stays open** pending cavalry-side sources
(split-section hypothesis unresolved). The NJ battery's identity
(candidate: Parsons's A/1st NJ, 4th Volunteer Brigade — six guns, on
the left-center that afternoon) is a pass-2 research item, not a ruling.

### ED-38 — CSA artillery battalion strengths: adopt the reproduction readings, with hops cited

**Canonical choice:** engaged-strength readings for the First Corps
reserve battalions are adopted from the B&M-type reproduction
(addressing-gettysburg-oob), with the secondary hop cited on every use:
Eshleman's Washington Artillery **338** (the build's 240 is retired as
unsourced-below-the-reproduction; the "340" snippet variant stays a
recorded variant), Alexander's **576** (the build already matches). Any
figure from this reproduction is flagged B&M-type/hop in the master
table and claims until a primary return surfaces. **Rationale:** pass 1
found no primary strength return for either battalion; the reproduction
is internally consistent per-battery and matches the tablet armament
counts; citing the hop preserves honesty without leaving the slot empty.

### ED-39 — Monument reliability profile (what a monument position *is*)

Adopted as proposed by the spatial program (RENUMBERED from its
provisional "ED-32"; full text and calibration table:
`docs/reconstruction/audit/monument-register.md`). Summary of the
adopted profile, recorded once and inherited by every claim citing a
marker:

1. Monument positions are **veteran-adjudicated, 20+ year latency**
   evidence (GBMA committee process, Bachelder arbiter of position
   disputes from 1883) — sworn memory filtered through committee
   politics, never survey of battle-time positions.
2. Every marker binds to a **positionSemantics moment** (climax/defense
   position, farthest advance, line extent at the marked moment), never
   to "the battle" at large.
3. Litigated memory (exemplar: the 72nd PA suit = ED-4, its 82 m
   monument-vs-build delta) is encoded as conflict, never averaged.
4. The verification layer is **asymmetric by side** (CSA unit monuments
   nearly absent; War Department tablets/Bachelder carry that side).
5. Radius guidance: ±5–15 m as a check on document-fixed anchors;
   **±50 m as sole anchor**; the 5-unit calibration table (18–31 m
   deltas for defensively-static units, both outliers explained by the
   register's own quality flags) is the adopted empirical basis.

---

## Dossier pass 2 adoption rulings (ED-40 … ED-45) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 2 (`docs/reconstruction/audit/dossier-pass-2.md`
§5, drafted on branch audit-dossiers-2) are ADOPTED as recorded below,
executed at the start of dossier pass 3 (branch audit-dossiers-3). No
renumbers were needed this batch.

### ED-40 — The crisis battery-slot naming: Cowan occupies the Brown-vacated slot

The question: which battery stood in the slot immediately south of the
Copse — the position Brown's B/1st RI vacated mid-cannonade (~14:40,
ED-33) — when the assault crested? Hall's report calls it "Cushing's
battery"; the OR editors' footnote corrects him to "Should be
Arnold's"; both are wrong on the pass-2 evidence.

**Canonical choice:** Cowan's 1st NY Independent Battery is the
occupant. Cowan's own report has him brought at a gallop into the
position Battery B, 1st Rhode Island had occupied; his monument
(verified in-slot, ~20 m agreement between sheet label and build hold)
and Bachelder j3-03 concur; Cushing's traced position is ~200 m north
at the Angle and Arnold's a further ~80 m north (both pass-1 closed).
Hall's "Cushing's" and the OR editors' "Should be Arnold's" are adopted
as recorded MISIDENTIFICATIONS riding the claim — first-class conflict
records, not placement evidence. **Rationale:** the only reading in
which every executor dossier (Cowan, Brown, Hall, Arnold, Cushing via
the V2 corpus) is consistent at once; Hall wrote under the smoke of a
melee on his own front, and the OR editors corrected a name without
geometry. Consequence: any cast of the crisis window names Cowan in the
slot; provenance UI must be able to surface both misidentifications.

### ED-41 — The crisis-reinforcement activity class (fourth ED-34 class)

Pass 2 documented five batteries with in-window arrival records at the
threatened center: Cowan (gallop into the Brown slot), Weir ("conducted
to General Webb's position … under a heavy musketry fire"), Fitzhugh
and Parsons (the pair run to the fence at ~75 yards), Wheeler (detached
from the Osborn group, enfilading a column 400 yards off, halting it
"three times"), plus Cooper as the during-cannonade variant (his ~15:00
transfer to the crest under fire).

**Canonical choice:** a fourth named artillery activity class joins
ED-34, **crisis-reinforcement**: silent/parked or engaged-elsewhere →
displacement leg at speed (limbered gallop — the one activity class
whose movement leg is fast) → short violent fire window at close range.
Matching casualty-curve class: near-zero before arrival, single spike
across the fire window (Cooper's variant: the spike begins on the leg
itself, under the cannonade). **Rationale:** the macro file renders
reinforcing batteries as either absent or statically present; the
documented class is exactly the Union line getting STRONGER during the
assault, from the Reserve node — a rendering-relevant, fully-exemplared
behavior. Authoring consumes the class; this ruling moves no track by
itself.

### ED-42 — The two-ledger rule (issues vs expenditures)

Pass 2 landed both train-side and battery-side ammunition figures:
Gillett's Artillery Reserve ordnance annex (19,189 rounds ISSUED, 4,694
on hand, 70 wagons displaced on the morning of the 3d) and nine
battery/battalion EXPENDITURE returns.

**Canonical choice:** issues and expenditures are distinct evidence
classes and are never conflated. EC5 rounds→duration derivations
(rounds expended ÷ rate of fire) cite battery-side expenditure returns
ONLY; train-side issue figures serve as upper-bound corroboration and
supply-side context (e.g. the physical substance behind Hunt's economy
order, CA-J3A-4). A unit's dossier may carry both, labeled by ledger.
**Rationale:** an issue figure includes rounds still in chests, lost,
or transferred; deriving a firing duration from it inflates the window
— the error class the spec's EC5 method exists to avoid.

### ED-43 — Tablet casualty lines are day-scoped until corroborated

Pass 2 caught the War Department tablets mis-scoping casualty lines in
BOTH directions: Pegram's tablet prints the July 3 row (47) where it
reads as the battle total (the report's battle total is larger), while
Cooper's Hancock-Avenue tablet prints the battle total under a
July-3-only heading.

**Canonical choice:** a corollary to ED-39 §1 — a tablet casualty
figure is adopted ONLY when its scope (which day(s), which engagement)
is corroborated by an OR return or the unit's own report; otherwise it
is carried as a scope-unresolved reading. **Rationale:** two
independent counterexamples in one pass establish that tablet casualty
scope is unreliable as a class; the tablets remain excellent for
armament, structure, and movement statements (ED-39 stands).

### ED-44 — Attested-silence rendering rule

Units with first-class silence or partial-participation records —
Garnett's battalion ("did not fire a single shot, having received
orders to that effect"; the smoothbores "bore no part in these
actions"), and the Ewell-wing partials per ED-45 — have their silence
as EVIDENCE, not absence of evidence.

**Canonical choice:** such units must render **present-but-not-firing**
through the attested window — deployed, under fire where attested,
muzzles cold. The authoring-side counterpart of ED-34: silence with a
primary citation is an activity state, never a rendering omission, and
never smoothed into ambient "firing" for visual effect. **Rationale:**
every cannonade gun-count claim now has a documented subtraction ledger
(pass-2 finding); rendering silent battalions as firing would falsify
the very records that closed that ledger.

### ED-45 — Retire the "Ewell-wing silence" phrasing; adopt the four-class breakdown

Pass 1 carried the Second Corps artillery as "near-silent" during the
July 3 cannonade. Brown's corps report (or-27-2-jtbrown) documents a
structured reality: Dance's battalion ENGAGED; portions of Carter's and
Nelson's battalions PORTION-ENGAGED (diversionary fire); Jones's
battalion OFF-FIELD (with the cavalry); Latimer's battalion
WRECKED-PRIOR (July 2, Benner's Hill).

**Canonical choice:** the wing is described and rendered by the
four-class breakdown — engaged / portion-engaged / off-field /
wrecked-prior — and the phrase "Ewell-wing silence" is retired from
audit prose (historical uses in committed pass reports stand as
records). Each battalion row carries its own class; ED-44 governs the
rendering of the non-firing classes. **Rationale:** the corrected
record is primary and battalion-grained; keeping the blanket phrase
would misdescribe Dance's documented participation.

---

## Dossier pass 3 adoption rulings (ED-46 … ED-49) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 3 (`docs/reconstruction/audit/dossier-pass-3.md`
§5, drafted on branch audit-dossiers-3) are ADOPTED as recorded below,
executed at the start of dossier pass 4 (branch audit-dossiers-4). No
renumbers were needed this batch.

### ED-46 — Garnett's engaged basis: Peyton's previous-evening report (1,287 men + ~140 officers)

Readings: 1,480 "present" (stone-sentinels monument compilation, the
ED-9/macro value, tier inferred) vs Peyton's own report — "The brigade
went into action with 1,287 men and about 140 officers, as shown by the
report of the previous evening" (peyton-or-1863) ≈ 1,427 all ranks.

**Canonical choice:** the Peyton figure is the EC2 engaged basis —
1,287 men + ~140 officers, a July-2-evening morning-report basis, the
best strength evidence any CSA brigade in the assault column has. The
1,480 compilation stays as the recorded ED-9 macro value; reconciling
the shipped strength decay is an authoring-pass item, not a silent edit
(the ED-32 pattern exactly). **Rationale:** a primary morning-report
statement in the brigade's own after-action report beats a
monument-committee compilation on every axis the OOB doctrine scores;
adopting it at the dossier/master-table layer while deferring the build
reconciliation is the established discipline between the evidence layer
and the shipped product.

### ED-47 — Lane's brigade loss basis: 660 of 1,355; the return's 389 is a component measure

The ANV casualty return prints 41 k / 348 w / — = 389 for Lane's
brigade WITH its own footnote "+General Lane reports his entire loss at
660" (or-27-2-anv-return p. 344); Lane's report gives "660 out of an
effective total of 1,355", with July 1–2 loss "but slight"
(or-27-2-lane-jh).

**Canonical choice:** 660 is the battle-total EC6 basis; the return's
389 is carried as the k+w COMPONENT measure per the return's own
footnote — different measures, not competing counts (an ED-49
exemplar). Consequences: the build's 660 decay is CONFIRMED as
primary-sourced, and master-table EC6 notes stop treating 389 as a
competing total. **Rationale:** the return's compiler already flagged
the difference; treating scope differences as conflicts manufactures
disagreement the sources do not contain.

### ED-48 — Brockenbrough's evidence basis (the honestly-bounded unit)

The brigade has no OR report, no officer pins, and a corrupted
compilation record (the Stone Sentinels page duplicates Davis's figures
byte-for-byte — 2026-06-13 finding, standing DO-NOT-USE flag).

**Canonical choice:** (a) EC2 adopts the Mayo-hop **~880** (R. M.
Mayo's Aug. 13, 1863 "800 muskets" via the secondary NPS-essay hop, the
build's figure) as the working basis, with the **800–1,100 range
carried** and no promotion to a clean number; the duplication artifact
is permanently unusable. (b) EC6 adopts the ANV return's regiment-grain
**148 k+w as the loss FLOOR** (missing unreported; or-27-2-anv-return
p. 344) — the build's 100-casualty decay is BELOW the documented floor
and is superseded for authoring purposes (reconciliation flagged, not
silently edited). (c) The build's two-wing rendering (Mayo's left wing)
is declared **reconstruction-grade** until a primary surfaces — carried
as build-inherited reconstruction on both wing rows. **Rationale:**
every slot is bounded by the best evidence that exists while refusing
both invention and the corrupted record; the unit stays T3-honest.

### ED-49 — The casualty-return scope rule (ANV return consumption)

The ANV casualty compilation (OR 27/2 pp. 338-346) was consumed at
regiment grain in pass 3, and its own apparatus demands a standing
consumption rule: per-regiment aggregate rows are **killed + wounded
only**; captured/missing is pooled at brigade level or absent entirely;
the totals are "approximative" and "many of the 'missing' were killed
or wounded. Especially … Pickett's division" (p. 338 headnote,
verbatim).

**Canonical choice:** every EC6 use of the return carries the scope
note: (1) regiment rows are k+w only, never battle totals; (2) a
missing column absent from a brigade's rows is a GAP, not a zero; (3)
return totals are floors wherever the missing column is absent, and
approximative everywhere per the compilation's own headnote; (4) where
a unit's own report gives a larger total including missing, the report
figure is the battle-total basis and the return the component measure
(ED-47 is the exemplar ruling). Exemplars recorded: Fry's 517 pooled
captures, Marshall's missing-less 1,105, Scales's July-1 table larger
than the three-day compiled total. **Rationale:** two passes produced
three independent cases where reading the return naively inverts the
evidence (floor read as total, gap read as zero); the scope rule is the
return's own headnote, promoted to discipline.

---

## Dossier pass 4 adoption rulings (ED-50 … ED-52) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 4 (`docs/reconstruction/audit/dossier-pass-4.md`
§5, drafted on branch audit-dossiers-4) are ADOPTED as recorded below,
executed at the start of dossier pass 5 (branch audit-dossiers-5). No
renumbers were needed this batch.

### ED-50 — The 8th Ohio casualty shape (picket-line-dominant split)

Sawyer's report contains a noon statement — 4 killed + 41 wounded on the
picket line BEFORE the charge window — against his battle total of 102,
an EXACT match to the regiment's Return of Casualties row (or-27-1-return
pp. 155-187; the pass-4 agreement exemplar). The famous July 3 flank
action therefore cost the regiment LESS than its 40-hour picket tour.

**Canonical choice:** us-8oh's casualty decay adopts the
picket-line-dominant split — ≈45 of 102 before the charge window, a
flat-attrition-then-moderate-spike curve — superseding any climax-spike
default for this unit. **Rationale:** the split is stated by the
commander mid-battle and reconciles exactly with the official return;
rendering the flank action as the regiment's bleeding hour would invert
a primary-attested shape. Consequence: the build's us-8oh decay is
flagged for reconciliation at the next authoring pass (never a silent
edit), and the shape joins R-casualty-curve-shape's cited-shape
exemplars.

### ED-51 — Carroll's July 3 placement rule (East Cemetery Hill, not the center)

Carroll's report has the three regiments (4th OH, 14th IN, 7th WV) sent
to East Cemetery Hill the night of July 2 and holding there "until the
5th"; the j2-05 drawn state concurs ("Carroll's Brig 2d Corps" drawn ON
East Cemetery Hill at (5100, 5624)), and no primary returns them to the
II Corps front on July 3. Only the 8th Ohio (detached) fought on the
center's flank.

**Canonical choice:** us-carroll renders on East Cemetery Hill for the
whole of July 3 — under the cannonade cross-fire without reply (ED-44
class); any build track placing the three-regiment body on the II Corps
front July 3 is contradicted by the primary and flagged for
reconciliation. us-8oh renders independently per its own dossier.
**Rationale:** report + drawn state agree; the famous brigade is two
units with two different July 3s, and rendering must follow the split,
not the fame.

### ED-52 — The Union report-vs-return EC6 rule (ED-49's mirror)

Pass 4 consumed the Union Return of Casualties (OR 27/1 pp. 155-187) at
regiment grain — full k/w/captured-missing columns — and surfaced one
clean conflict: Webb's report states 525, the return compiles 491 (with
two exact report=return agreements beside it: 8th Ohio 102; Gates's 170
= the 80th NY row).

**Canonical choice:** where a Union commander's stated loss disagrees
with the official Return of Casualties, the RETURN is the adopted
battle-total EC6 basis and the report figure is carried as the
commander's count (first-class conflict record, never averaged).
Exemplar: Webb 491 adopted / 525 carried. The rule is ED-49's mirror
with the asymmetry explicit: the Union return's full missing columns
make it a compilation of record, where the ANV return's absent missing
columns make it a floor (ED-49 rule 3 still governs that side).
**Rationale:** the return is the army's own reconciled compilation
against the commander's in-the-smoke count; adopting it while carrying
the report figure preserves both without manufacturing precision.

---

## Dossier pass 5 adoption rulings (ED-53 … ED-55) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 5 (`docs/reconstruction/audit/dossier-pass-5.md`
§5, drafted on branch audit-dossiers-5) are ADOPTED as recorded below,
executed at the start of dossier pass 6 (branch audit-dossiers-6). No
renumbers were needed this batch. ED-53 is the audit's first CHAIN
REVISION ruling — it moves an adopted anchor's preferred time on
executor evidence, per the sequencing doctrine's design (anchors
substantiated by executors' dossiers beat anchors resting on event
quotes alone).

### ED-53 — CA-J2A-2/3 revision (the July 2 artillery opening and Hood's step-off)

The July 2 afternoon chain (ED-28, provisional) carried CA-J2A-2
(Longstreet's artillery opens) at 15:45–16:00 and CA-J2A-3 (Hood's
division steps off) at 16:00–16:30. Pass 5's executor dossiers (Law,
Robertson, Vincent, Weed + the closed ED-28 precondition fetches:
Chamberlain's OR, Oates, Norton) assembled a step-off ladder no prior
pass had.

**Canonical choice:**

- **CA-J2A-2 adopts ~15:45–16:20 as a recorded two-pole structure** —
  Alexander's 15:45 (36 guns in action, his own arithmetic) plus the
  Scruggs/Oates 15:30–15:45 arrival window at the early pole, vs
  Jacobs's "twenty minutes past 4 P.M." clock READING at the late pole.
  The chain's precision standard (Jacobs, contemporaneous-civilian,
  ±2 min) sits at the late pole again, exactly the CA-J3A-1 situation;
  neither pole is discarded.
- **CA-J2A-3 is REVISED to ~16:30, envelope 16:15–17:00** (was
  16:00–16:30), on the executor ladder: (a) **the 16:00 Union signal
  dispatch quoted verbatim by Law** — "To General Meade — four o'clock
  P.M. The only infantry of the enemy visible is on the extreme
  (Federal) left; it has been moving toward Emmitsburg" — a timestamped
  DOCUMENT (±0 by class, the Buford-10:10 pattern) showing the column
  still deploying at 16:00, which EXCLUDES the old window's front edge;
  (b) Alexander's arithmetic (36 guns ~15:45, "perhaps 30 minutes" of
  cannonade, then Hood's order) ⇒ ~16:15–16:25; (c) Law's own "near
  5 o'clock" with only ~15 min of prior artillery (the late pole);
  (d) Oates's dark-minus-"four hours previously" ⇒ ~16:00–16:30;
  (e) Robertson's "but a few minutes before we were ordered to advance"
  (no long in-position wait); (f) Norton's compiled ruling (cannonade
  ~15:30, infantry after ~30 min, first contact on Vincent 16:30–17:00).

**Downstream anchors are CONFIRMED, not moved:** CA-J2A-5 (Devil's Den
~17:30), CA-J2A-6 (LRT climax; the Rice 20:00 / Chamberlain 21:00
evening end now triple-anchored), CA-J2A-7 (Kershaw ~17:30, hardened by
the executor's own trigger record and ladder arithmetic against his own
stated clock), CA-J2A-9 (Barksdale ~18:15) — the revision tightens one
node and hardens the rest. **Consequences:** CA-J2A-2/3 upgrade from
provisional to ruled (ED-28's named precondition — the Little Round Top
verification pass — was CLOSED in pass 5, so the chain's precondition
list is now empty); the anchor-chain table (`anchor-chain-proposal.md`
§2.3) is revised in place with this ruling cited; law-bl-1884's
CONTINGENT clock profile resolves against the revised anchor (offset
~−30 against the adopted preferred, inside envelope). **Rationale:**
a document pin plus four independent witness arithmetics against one
tablet-class "soon after 4" reading; the never-silently-re-time
doctrine is satisfied because the revision is recorded here, moves no
shipped geometry, and the July 2 afternoon has no shipped slice clock
to re-time.

### ED-54 — Vincent-brigade July 3 placement rule

Rice's report has the brigade "relieved during the forenoon … ordered
to the center of the line" on July 3 — the II Corps-rear center as
reserve, under the cannonade cross-fire without reply (ED-44 class) —
and no primary returns it to Little Round Top that day; the hill's
July 3 garrison is the Weed-brigade/Fisher ground.

**Canonical choice:** us-rice renders at the II Corps-rear center for
July 3, present-but-not-firing through the cannonade; any build track
holding the brigade on Little Round Top through July 3 is contradicted
by the primary and flagged for reconciliation (never a silent edit).
**Rationale:** the ED-51 pattern applied to the other famous brigade —
report + relief record agree, and rendering must follow the evidence,
not the fame. The 20th Maine's Great Round Top night position (j2-05
drawn state + Chamberlain) governs only the night of July 2–3; the
forenoon relief supersedes it for the July 3 afternoon window.

### ED-55 — The McLaws-wing REPORT-clock skew class (ED-28's rule extended to its origin)

Pass 5's commissioned executor test established that the McLaws-wing
early skew ORIGINATES in the reports, not the tablets: Kershaw's own
report states arrival "about 3 p.m." and the advance order "about
4 o'clock" — ~90 min early against the adopted ~17:30 — and the
tablet's 3:30/4:30 is a PARTIAL CORRECTION of the report clock, not an
independent error.

**Canonical choice:** the wing's OR-report clocks are a named
clock-offset class, **report-nominal +45..+105 min early** against the
chain, profiled per source on fetch (exemplar: or-27-2-kershaw,
profiled +90 [+45,+105] weak). The tablet class keeps its ED-25 ±60
McLaws-wing exception, now understood as partial correction of the
same house clock. ED-25 rule 4 unchanged: these clocks never define or
move an anchor. The V Corps counterpart (Rice +45, Sykes +15 — the
early skew is BILATERAL on this field, unlike July 3's late-stated
cluster) is recorded as a finding on the profiles, not promoted to a
rule. **Rationale:** the skew rule adopted with ED-28 was scoped to
tablets because tablets were the evidence then in hand; the executor
pass traced the phenomenon to its source, and the profile mechanism
(ED-25) is the correct carrier for a per-source, per-wing offset class.

---

## Dossier pass 6 adoption rulings (ED-56 … ED-59) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 6 (`docs/reconstruction/audit/dossier-pass-6.md`
§5, drafted on branch audit-dossiers-6) are ADOPTED as recorded below,
executed at the start of dossier pass 7 (branch audit-dossiers-7). No
renumbers were needed this batch. No anchor moves: ED-57 CONFIRMS
CA-J2A-8 as wave 3's start and exercises CA-J2A-11 as wave 5's end pin;
the anchor-chain table is annotated, not revised.

### ED-56 — The Smith's-guns convergent-capture ruling

Pass 5 left the three abandoned guns of the 4th New York Independent
Battery carried as an unresolved both-dossiers cross-credit (Law tablet
crediting the 44th/48th AL vs Robertson's 1st TX claim); pass 6 closed
the record both-sided — Smith's own report has exactly THREE 10-pdr
Parrotts abandoned on the Devil's Den hill (the fourth forward gun
saved/disabled, the rear section fighting on from the gully), credits
NO captor, and believed them later retrieved; Benning states "three
front guns … 10-pounder Parrotts" with the enemy withdrawing "three
rear guns" — count and type agree across the lines.

**Canonical choice:** the three 10-pdr Parrotts changed hands **ONCE
(~17:30, CA-J2A-5)** to an intermixed force — Robertson's 1st TX +
Benning's Georgians, with the 44th/48th AL engaged up the gorge on
their right — and all three unit-level credits (Law tablet, Robertson
report, Benning report+tablet) are honest views of ONE convergent
event; **no exclusive credit is encodable** (the Union primary fixes
the object's count and type and names no captor). **Benning's brigade
is the HOLDER through July 3** (his holding record + the j2-04/j2-05
drawn-state continuity). Consequences: casts render one hands-change
with a three-way provenance note; the pass-5 both-dossiers-carry-it
conflict resolves without a winner; Smith's rear-section
fought-on-from-the-gully state renders per his own report.
**Rationale:** the only reading consistent with all four executor
dossiers at once; exclusive credit would manufacture a resolution the
sources do not contain.

### ED-57 — The Wheatfield occupation-wave record (multi-occupation ground)

The Wheatfield's notorious multi-occupation sequence, assembled by
pass 6 from the executors' primaries without inventing a master clock
(pass-6 report §3.1: W1 de Trobriand/Anderson ~17:00–17:45 · W2
Kershaw+Semmes vs Tilton/Sweitzer ~17:45–18:15 · W3 Caldwell's
counterattack 18:00–18:15 start · W4 Wofford's sweep + the renewal
~18:30–19:15 · W5 the Plum Run counter-line to sunset 19:15–~19:45).

**Canonical choice:** the five-wave structure is adopted as the
canonical conflict-record treatment for multi-occupation ground: each
wave carries BOTH sides' executor citations; wave boundaries are
**TRIGGERED** (relief, ammunition exhaustion, flank collapse, sunset
order) rather than clocked, except where an anchor (CA-J2A-8 —
**CONFIRMED as wave 3's start**) or an astronomical pin (CA-J2A-11,
Wofford's "fell back at sunset" = 19:29 LMT — wave 5's end) exists;
the ground's occupant function follows the wave record. No anchor
moves. **Rationale:** the waves reconciled without a single averaged
reading — the remaining conflicts are NAMED (ED-59; the Wofford EC6
composition) and ride their dossiers; a wave record whose seams are
triggers is exactly what tier-D encoding wants. Consequence: any cast
of contested ground renders occupancy by wave, and future
multi-occupation research (Culp's Hill's traded works) applies the
same structure.

### ED-58 — The field-wide July 2 report-clock early-skew class (ED-55 generalized)

Pass 6 landed six more early profiles across BOTH armies' southern
field: Smith +90 (III Corps), Tilton +75 (V Corps), Kelly +70
(II Corps), Caldwell +60 class, Benning +75 (Hood's wing), Coates +45
weak — joining pass 5's Kershaw +90 (ED-55), Rice +45, Sykes +15.

**Canonical choice:** the southern field's OR-report clocks run
**+45..+110 early on BOTH armies** — a field-wide class, not a McLaws
peculiarity. Consequences: per-source `clockProfile`s on fetch (all
pass-5/6 exemplars landed); the ED-25 tablet exception stays
wing-scoped but its Union counterpart is RECORDED (the Union brigade
tablets' "between 5 and 6 P.M." class is a partial correction; Ward's
tablet "forced back between 4 and 5 P.M." shows the tablet class skews
early on the Union side too — heterogeneous, per-source, never
promoted to a blanket tablet rule); **ED-25 rule 4 unchanged** —
report-nominal clocks never define or move an anchor. Alternative
considered and rejected: leaving ED-55 CSA-scoped (six Union profiles
now carry the same structure). **Rationale:** the phenomenon is the
field's, not a wing's; the profile mechanism already carries it
per-source, so the ruling names the class and changes no anchor.

### ED-59 — The Tilton/Sweitzer withdrawal-conflict ruling

De Trobriand's report: the V Corps brigades "had fallen back without
engaging the enemy" — against the V Corps record (Tilton: two
repulses, 125 casualties by the return, a personally-reconnoitered
flanking force "from the direction of Roses house"; Sweitzer: the
Prescott conditional-order exchange; Barnes's 654-strength frame).

**Canonical choice:** the withdrawal's CAUSE is the Barksdale-axis
outflanking of the salient (Tilton's own reconnaissance record), and
it PRECEDED the stony hill's storming rather than following a rout;
the accusation is CARRIED as de Trobriand's honest view from a front
the V Corps line did not cover. Neither report is averaged; **renders
must not show the V Corps brigades routed** — an ordered two-stage
withdrawal under an outflanking threat, per Tilton's own leg record.
**Rationale:** the accusation and the casualty/repulse record cannot
both be literal; the return's 125 and the two-repulse record are
primary against an adjacent-front impression, and the ED-39 §3
litigated-memory discipline (encode the conflict, adopt the reading
the evidence carries) applies to report-vs-report conflicts equally.

---

## Dossier pass 7 adoption rulings (ED-60 … ED-61) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 7 (`docs/reconstruction/audit/dossier-pass-7.md`
§5, drafted on branch audit-dossiers-7) are ADOPTED as recorded below,
executed at the start of dossier pass 8 (branch audit-dossiers-8). No
renumbers. No anchor moves. **Sequencing note:** ED-61 was adopted
CONDITIONALLY on its named precondition — the Bigelow/9th MA and
Watson/I-5th US battery records — and the precondition was SATISFIED
FIRST (pass-8 batch-1 fetches, committed before this ruling): the
object counts and the recovery disposition are now fixed by primaries,
and the ruling below records the ledger as gated-then-closed.

### ED-60 — The Wright high-water ruling

**Canonical choice:** Wright's brigade's farthest advance on July 2 is
the Emmitsburg-road-to-west-slope gun line fixed by the advance marker
(4330.6, 4746.9), the j2-04 drawn state (red bars ON Brown's battery
ground, 28 m agreement), and the receiving-side dossiers (Brown's and
Weir's temporary gun losses; the Hall/Harrow/Webb ridge fronts
unbroken). The report's "complete masters of the field" / "key of the
enemy's whole line" / "over twenty pieces" claims and the WD tablet's
"reached the crest" are carried verbatim as the commander's account
THAT THE TABLET INHERITED (a tablet-inherits-report case for the
ED-39/43 jurisprudence — the tablet is not an independent witness for
the crest). EC6 basis: the return's 668 (report 688 and tablet 873
carried as named conflict components; the missing column agrees
exactly, 333 = 333 — the conflict is confined to k/w composition).
**Consequences:** casts render the brigade at the gun line at the
climax, never on the crest; the build's `csa-wright` track is checked
against the marker geometry at the next authoring pass (never a silent
edit here). **Alternative rejected:** adopting the tablet's crest —
every independent evidence class contradicts it. **Rationale:** the
one reading consistent with the marker, the drawn state, and the
receiving side at once; fame is not an evidence class.

### ED-61 — The Plum Run / Trostle gun-recovery cross-credit rule

The ED-56 convergent-event pattern, mirrored for RECOVERIES, adopted
for the Trostle/Plum Run ground — with the precondition fetches landed
first and the ledger accordingly CLOSED rather than merely carried:

- **Objects fixed (the gate):** the 9th Massachusetts left FOUR of its
  six light 12-pounders at the Trostle angle (Milton: "silenced the
  four pieces on my right, and prevented their withdrawal"; Milton's
  own section of two retired); Watson's I/5th US had its FOUR guns
  abandoned on the same ground ("the guns of Battery I, Fifth
  Regulars, were abandoned", McGilvery).
- **The recovery spine (same night):** McGilvery's 20:00 disposition
  record — a detail from the 6th Maine + Seeley's battery hauled off
  Battery I's guns, and Dow's procured infantry detail the four 9th MA
  guns: "The guns of the two batteries, numbering eight, were brought
  safely to the rear." Milton concurs ("Our four guns were recovered
  the same day"). ALL EIGHT guns were off the field by ~21:00 July 2.
- **The claimant ledger (each an honest view of one convergent
  sequence, no exclusive credit encodable):** Peeples + the Garibaldi
  Guards' recapture charge of Watson's guns (Martin, the one
  named-actor recapture primary); Lockwood's brigade "recapturing
  three pieces of artillery abandoned by the enemy" at "quite dark"
  (A. S. Williams — the XII Corps left-wing detachment, McGilvery
  in-person context); Nevin's "two light 12-pounder brass pieces" in
  the W5 charge; Brewster's after-sunset "recaptured guns that had
  been left on the field"; the loss-side state (Barksdale's 21st MS
  "captured but were unable to bring off").
- **Named residue (carried, not resolved):** Crawford/McCandless's
  July 3 Napoleon + three caissons (tablet-attributed to the 9th MA
  Battery) conflicts with the all-eight-off-the-field-July-2 spine —
  recorded as an unreconciled recovery-ledger residue (most plausibly
  materiel other than the four guns, e.g. caissons/limbers; not
  adjudicated).

**Consequences:** the Trostle-ground battery casts render ONE loss per
battery and a recovery ledger — never per-claimant duplicate guns; the
recovery renders as ground-retaken (infantry claimants) followed by
haul-off (artillery details), per the sequence the primaries carry.
**Rationale:** exactly ED-56's logic — the Union disposition record
(McGilvery, as Smith's report was for ED-56) fixes count and type, and
the multiple claimants converge on one event chain the sources decline
to apportion.

---

## Dossier pass 8 adoption rulings (ED-62 … ED-63) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 8 (`docs/reconstruction/audit/dossier-pass-8.md`
§5, drafted on branch audit-dossiers-8) are ADOPTED as recorded below,
executed at the start of dossier pass 9 (branch audit-dossiers-9). No
renumbers. **No anchor moves** — ED-62 is a basis upgrade on the adopted
CA-J2E chain (contrast ED-53, the audit's one chain revision), and ED-63
is the convergent-event pattern's third application (after ED-56 and
ED-61). Neither ruling carried a precondition; batch adoption executed
directly, validators green after the edit.

### ED-62 — The CA-J2E chain-hardening ruling (anchor upgrades, no moves)

**Canonical choice:** the July 2 evening chain (ED-29, provisional at
adoption) is upgraded anchor by anchor onto the pass-8 executor
primaries, with every adopted time and envelope UNCHANGED:

- **CA-J2E-1 (the XII Corps ordered away) — basis upgraded to
  PRIMARY.** Greene's "until 6.30 p. m., when the First (Williams')
  Division and the First and Second Brigades of the Second Division
  were ordered from my right" (or-27-1-greene p. 856) is the anchor's
  new spine — a primary clock inside the adopted 18:30–19:00 window,
  replacing the tablet basis. Geary's stated 7 p.m. rides as a
  propagation-vs-skew reading (order issued vs order reaching his
  column), and the j2-04 drawn departure state (the moving
  Geary/Candy labels, McDougall's vacated works, Ruger's column on
  the pike) is the anchor's drawn corroboration.
- **CA-J2E-2 (Johnson's assault strikes Greene) — CONFIRMED ~19:00**
  on the cross-line minute-class pair: Greene "a few minutes before
  7 p. m." vs Nicholls's "7 p. m. of July 2… ordered forward" (the
  first CSA report clock agreeing on the evening chain), bracketed by
  Johnson's dark-at-Rock-Creek and Jones's lateness/darkness
  statements. The attacking force composition is fixed at THREE
  brigades (Jones, Nicholls, Steuart) — Walker's Stonewall Brigade
  detained on the Hanover-road flank by his own discretion-order
  record (or-27-2-walker-stonewall), matching Johnson's
  did-not-arrive-in-time division record.
- **CA-J2E-3 (the East Cemetery Hill assault) — CONFIRMED
  19:45–20:00** on the three-source both-armies agreement (Hays "a
  little before 8" / Wiedrich "about 8" / Ricketts "at about 8", with
  Harris's "about dusk" and the Hoke tablet's 8 P.M. concurring), and
  EXPRESSED CAUSALLY relative to E-2 per Early's trigger primary
  ("as soon as Johnson became warmly engaged, which was a little
  before dusk, I ordered Hays and Avery to advance"). The
  twilight-physics class is adopted as the anchor's independent
  bracket: sunset 19:29 LMT / civil-twilight end ~19:59 (ED-31)
  against Hays's "darkness of the evening, now verging into night"
  ascent — five independent physics-adjacent darkness pins, both
  armies.
- **CA-J2E-4 (the counterattack clears the hill) — KEPT ~20:30 as a
  sequence anchor.** Ricketts's "a part of the Second Army Corps
  charged in" + Carroll's record make it both-sided, but no hard
  clock exists on either side — the "inferred anchor" flag STAYS.
- **CA-J2E-5 (the fighting dies down) — KEPT 22:00–22:30** on the
  three ~22:00 primaries (Hays "about 10 o'clock", Greene's
  Kane-return 10 o'clock, Geary's "about 10 p. m."); Nicholls's
  four-hours-from-19:00 reading runs to the envelope's 23:00 edge and
  is recorded, not adopted.

**Consequences:** ED-29's PROVISIONAL status is lifted to
adopted-with-primary-basis at E-1/E-2/E-3; E-4 remains
inferred-sequence class; envelope arithmetic is untouched everywhere,
so no downstream cast or caption re-times. Per-source clockProfiles
(ED-25) carry the evening skew class: Einsiedel +75 and Steuart +75
ride as profiled early-stated outliers; the agreeing cluster
(Greene/Nicholls/Hays/Wiedrich/Ricketts/Early/Johnson ~0) is the
chain's working set. **Alternative rejected:** re-timing E-1 to
Geary's 7 p.m. — propagation delay is not a clock conflict.
**Rationale:** the chain now rests on executor primaries at every
anchor, both armies, which is exactly what the sequencing doctrine
(research → anchor → place) was for.

### ED-63 — The East Cemetery Hill battery-possession ruling (the convergent-event pattern's third application)

**Canonical choice:** the ~20:00 lodgment among Wiedrich's and
Ricketts's guns on East Cemetery Hill is ONE convergent event, not a
set of competing captures. Hays's "several pieces of artillery…
every piece… silenced", Ricketts's captured-and-spiked left piece,
Wiedrich's fight inside "the intrenchments of my battery", and the
Hoke tablet's colors-on-the-lunettes are honest views of the same
minutes. The ruling fixes:

- **No gun left the hill.** No captured-ordnance row exists on either
  side's returns; Hays's brought-off trophies are the four COLORS
  (his own wording). One Ricketts piece was spiked in place — damage,
  not removal.
- **Possession was momentary.** It ends with the infantry
  counterattack (Carroll's brigade + the rallied XI Corps regiments,
  CA-J2E-4), and both batteries resume service the same night.
- **Casts render temporary possession + hand-to-hand + resumption —
  never a captured-battery state.** The batteries' tracks never show
  a lost-guns strength or materiel discontinuity.

**Contrast case, explicitly out of scope:** the Culp's Hill lodgment
(Steuart in the vacated XII Corps works) HELD overnight — that is
ED-57's occupation-wave discipline, not this ruling; the two grounds
are adjacent but the evidence classes differ (vacated works vs served
guns). **Alternative rejected:** adjudicating which CSA regiment
"took" which battery — the sources decline to apportion, and the
ED-56/ED-61 pattern (fix the objects, carry the claimant ledger,
render one event) is the recorded jurisprudence. **Rationale:** the
same discipline that closed ED-56 (Smith's guns), ED-60 (Wright's
high-water), and ED-61 (the Trostle recoveries): the disposition
record fixes count and outcome; fame is not an evidence class.

---

## Dossier pass 9 adoption rulings (ED-64 … ED-65) — 2026-07-11

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 9 (`docs/reconstruction/audit/dossier-pass-9.md`
§5, drafted on branch audit-dossiers-9) are ADOPTED as recorded below,
executed at the start of dossier pass 10 (branch audit-dossiers-10). No
renumbers. **No anchor moves** — ED-64 adopts a conflict-record
STRUCTURE (the two-pole upgrade), not a resolution: CA-J3M-3's ~10:00
ruling direction stays PROVISIONAL behind the standing Pfanz *Culp's
Hill* pp. ~340–355 gate, which this adoption does not touch. ED-65 is
an annotation ruling (preamble + carried conflicts) on an anchor whose
04:30 time is unchanged. Batch adoption executed directly, validators
green after the edit.

### ED-64 — The CA-J3M-3 two-pole conflict-record upgrade (the Spangler's Meadow charge time)

**Canonical choice:** the anchor's evidence basis is restructured as an
explicit two-pole PRIMARY conflict — the record is adopted; the
resolution is not.

- **EARLY pole ~05:30–06:00**: Morse (2nd MA, or-27-1-morse-2ma)
  "Firing was kept up until 5.30 o'clock, when the regiment was
  ordered to charge the woods in front of us"; Fesler (27th IN,
  or-27-1-fesler-27in) works occupied "between 5 and 6 o'clock" then
  charge in one narrative motion; the 27th IN advance marker's
  INSCRIBED "its charge at 6 a.m. July 3d. 1863."; Colgrove's
  early-morning narrative placement (clock-silent, verified).
- **LATE pole ~10:00**: Ruger's "This state of things continued until
  about 10 a. m.… At this time I received orders" (or-27-1-ruger),
  welded to the CA-J3M-4 works-reoccupation sequence; **Bachelder's
  drawn adjudication** — the advanced "2 Mass"/"27 Ind" bars appear on
  j3-02 (8–11 a.m.) and NOT on j3-01 (4–8 a.m.); Pfanz-derived ~10:00
  and Sears's near-noon as the secondary layer.
- **The ~10:00 ruling direction is RETAINED as provisional** (the
  order-chain + drawn + secondary convergence). **The Pfanz
  pp. ~340–355 precondition STANDS as the hardening gate.**
- The early-pole sources carry NO clockProfile offset — profiling
  would prejudge the dispute (recorded on or-27-1-morse-2ma /
  or-27-1-fesler-27in as kind-none with reasons; the dispute-pole
  rule).
- The Lockwood ~6 a.m. woods deployment (1st MD PHB, the meadow's NW
  woods) is recorded as the conflation-candidate adjacent event.
- **Cast rule:** no cast renders the charge before the Pfanz gate
  resolves the pole; the morning sheet's Colgrove position state is
  safe either way (in position south of the meadow on j3-01).

**Alternative rejected:** adopting the early pole on its
two-regimental-primaries + inscription strength — Ruger's order clock
is the ISSUING echelon's primary, Bachelder drew the late reading, and
ED-25 rule 4 (no fiat resolution of primary disputes) controls.
**Rationale:** the executor-first doctrine's first DISPUTED-anchor
outcome: the right deliverable was a properly-structured conflict
record, not a pick. The order-corruption pair (Ruger's
"mistake of the staff officer" vs Colgrove's verbal "advance your
line immediately") is carried verbatim at both echelons.

### ED-65 — The CA-J3M-1 opening-cluster annotation (no move)

**Canonical choice:** CA-J3M-1 keeps **04:30** as the ARTILLERY
opening, now at author-primary basis (Muhlenberg 4.30 = Ruger 4.30 =
Williams "daylight" = the XII Corps tablet), and gains:

- **(a) the 03:00–04:00 infantry-escalation preamble** as recorded
  pre-anchor structure: Cobham "At 3 o'clock… the enemy's skirmishers
  commenced firing on us, and by 4 o'clock the firing had become
  general"; Kane 3.30 "attack in force"; Candy's 3.45 battery clock;
  Grimes (13th NJ) 4 a.m. The anchor's text already says "artillery
  opens" — the infantry fire precedes it and is rendered as
  escalation, not as a competing opening time.
- **(b) the two author-vs-tablet conflicts as carried records**:
  **20 guns (Muhlenberg's report) vs 26 (tablet)** — Rigby's A/1st MD
  (Powers Hill, registered) is the flagged reconciliation candidate,
  NOT adopted; and **"until 10 a. m." (report) vs "10.30 A.M."
  (tablet)** — Jacobs and the Johnson tablet side 10.30, and
  CA-J3M-4's quadruple-attested 10:30 close is unaffected (the
  report's 10:00 rides as the recorded early component).
- The aftermath gains the Daniel/O'Neal midnight-release clocks
  (skirmishers to ~24:00; Daniel's main body 15:00–16:00) as envelope
  notes on the anchor context — no anchor move.

**Alternative rejected:** re-timing the anchor to the 03:00–04:00
infantry cluster — that would change the anchor's referent (the guns)
to a different event class. **Rationale:** same discipline as ED-62
(basis upgrade, arithmetic untouched); the conflicts are carried where
ED-42/ED-43 jurisprudence says they live — on the record, unresolved.

---

## Dossier pass 10 adoption rulings (ED-66 … ED-68, + the ED-65 addendum) — 2026-07-12

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 10 (`docs/reconstruction/audit/dossier-pass-10.md`
§5, drafted on branch audit-dossiers-10) are ADOPTED as recorded below,
executed at the start of dossier pass 11 (branch audit-dossiers-11). No
renumbers. **One structural chain change** — ED-66 splits CA-J1M-4 into
two rows (the deploy-vs-attack split the anchor carried as a
precondition since proposal); the split is evidence-driven (both poles
now executor-clocked) and narrows envelopes, moving no adopted time
outside its prior envelope. ED-67 and ED-68(a) are basis upgrades;
ED-68(b) is an envelope-edge tightening on an unchanged ~16:15
adoption. Batch adoption executed directly, validators green after the
edit.

### ED-66 — The CA-J1M-4 deploy-vs-attack split ruling

**Canonical choice:** CA-J1M-4 is split into two chain rows, both
sides now executor-clocked (dossier pass 10 delivered the evidence the
proposal's precondition asked for):

- **CA-J1M-4a — Heth deploys / artillery phase opens**, adopted
  **~08:00–09:00** (envelope 07:30–09:30): Gamble "About 8 o'clock…
  approaching his pickets… about 3 miles distant" + Buford "between 8
  and 9 a.m." + "held its own for more than two hours" (the delay
  duration pair); **Heth's own "at this time — 9 o'clock… I was
  ignorant what force was at or near Gettysburg"** is recorded as the
  deploy-phase CSA clock AND as time-silent on the attack (the
  conflation his report invited is the reason for the split).
- **CA-J1M-4b — Heth's infantry attack strikes the I Corps line**,
  adopted **~10:15–10:30** (envelope 10:00–10:45): **Davis's "About
  10.30 o'clock a line of battle was formed… and the brigade moved
  forward to the attack" (No. 553 p. 649 verbatim — the only stated
  CSA attack clock)** against the Union receiving cluster — Wadsworth
  10:00 road-strike, Cutler "in action from 10 a.m.", the Reynolds
  10:15 pin (killed receiving this attack). The old 09:00–10:45
  envelope is closed from both sides.
- **CA-J1M-5 basis upgrade (rider, as proposed):** the Reynolds-killed
  pin's basis note upgrades from tablet-adjudicated to
  **tablet + two-report three-primary agreement** (tablet "about 10.15
  A.M." = Wadsworth "at this time (about 10.15 a.m.) our gallant
  leader fell" = Doubleday "about 10.15 a.m."); Howard's 11.30 reading
  is COLLAPSED as word-reaching-him (Schurz independently witnesses
  the 11:30 scene). Adopted time unchanged.

**Alternative rejected:** keeping one row with a wide 09:00–10:45
envelope — the wide row existed only because Heth's report conflates
the phases; with executors clocked at both poles, the single row
mis-renders ~90 minutes of cavalry-only fighting as "assault".
**Rationale:** the split is what the anchor's own carried precondition
("split into two events as above") called for; downstream, movers
placed against "the attack" (Meredith/Cutler/Davis/Fry) gain a
half-hour-class envelope for free.

### ED-67 — The CA-J1P-2/3 basis-upgrade ruling (Iverson; Gordon/knoll)

**Canonical choice:**

- **CA-J1P-2 (Iverson destroyed)** upgrades from "slaved to CA-J1P-1,
  sequence" to a **bracketed event with drawn geometry**: adopted
  window 14:30–15:00 held, envelope 14:15–15:15 held; basis now the
  four-source dress-parade convergence (Iverson + Rodes + Ramseur +
  Cutler receiving) plus the j1-09 drawn mid-advance bar 408 m along
  the advance axis from his formation tablet. The audit's cleanest
  single-spike EC6 exemplar backs the anchor.
- **CA-J1P-3 (Early's assault, Barlow Knoll)** adopted **~15:00–15:30**
  on the **Gordon = Schurz cross-line pair** (attacker's "About 3 p.m.
  I was ordered to move my brigade forward" vs receiver's "about 3
  o'clock… the enemy appeared in our front with heavy masses") — the
  skeleton's "Early's OR (MEMORY-grade, fetch)" precondition is CLOSED
  with a page-verified brigade primary. Envelope 14:30–15:45 held.

**No downstream moves** (both adopted windows unchanged; only their
evidence class rises). **Rationale:** ED-62's discipline — basis
upgrades on adopted anchors, arithmetic untouched.

### ED-68 — The July 1 command sub-anchors (Howard; the Hancock front edge)

**Canonical choice:**

- **(a) Howard-assumes-command adopted ~11:30** as a formal sub-anchor
  of the ED-26 chain (Howard = Schurz two-primary agreement; the
  XI-Corps tablet's 10:30 is COLLAPSED onto the hurry-forward order,
  not the assumption of command). **The Pfanz 10:30 derivation stays
  an honest open gate** — recorded on the sub-anchor, not blocking it.
- **(b) The CA-J1P-7 front edge tightens 15:00 → ~15:30** (envelope now
  **15:30–16:30**, adopted **~16:15 unchanged**): the formalized
  conflict record has every piece beyond Hancock's report-nominal
  15:00 on the late pole — his own 17:25 dispatch's "an hour since"
  (a document pin AGAINST his own report), Howard's 16:30, Doubleday's
  post-retreat dual-command scene, Schurz's "after 5 o'clock" corps
  bound. Hancock's 15:00 stays carried verbatim as the report-nominal
  pole (this tightens an envelope edge; it does not delete a primary).
- **(c) Wadsworth's "2.30 Schurz fell back" early counter-reading:
  RECORDED, no status** (report-nominal, ED-25 rule 4) — as proposed.

**Alternative rejected:** holding the 15:00 front edge for symmetry
with the report — the edge would then be held by a reading the record
shows contradicted by its own author's same-day dispatch.
**Rationale:** the ED-64 lesson applied with the poles reversed: there
the record stayed two-pole because both poles had uncontradicted
primaries; here one pole is document-pinned against itself.

### ED-65 addendum (record-only) — the Geary/Muhlenberg opening conflict

The CA-J3M-1 carried conflict set (ED-65(b)) gains the pass-10
within-command pair: **Geary's "3.30 a.m. + about ten minutes"
opening vs Muhlenberg's author-primary "4.30 + ~15 minutes"** — carried
unresolved alongside the 20-vs-26-guns and 10:00-vs-10:30 records. The
anchor's 04:30 adopted time is unchanged (Muhlenberg remains the
author primary; Geary's version joins the recorded early component
class). NOT a resolution.

---

## Dossier pass 11 adoption rulings (ED-69 … ED-71) — 2026-07-12

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 11 (`docs/reconstruction/audit/dossier-pass-11.md`
§5, drafted on branch audit-dossiers-11) are ADOPTED as recorded below,
executed at the start of dossier pass 12 (branch audit-dossiers-12). No
renumbers. **No anchor moves** — ED-69 is an annotation ruling under ED-25
rule 4 (both new primaries are report-nominal-class, so the ladder is
recorded, not promoted to a move); ED-70 is a basis upgrade in the ED-62/
ED-67 pattern; ED-71 is a rule of computation, generalizing ED-49/ED-52 to
a case those rulings did not anticipate (the printed figure fails its OWN
arithmetic, not just a rival source's). Validators green after the edit.

### ED-69 — The CA-J1P-1 early-component annotation (no move)

Pass 11 landed a two-independent-primary early pole — Roy Stone "At about
1.30 p.m. the grand advance of the enemy's infantry began" and Dobke (45th
NY) "At about 1.30 p.m. a long line of the enemy moved on the extreme
right of the First Corps" — a cross-corps agreement sitting 30 minutes
ahead of the adopted 14:00 tablet reading, inside an interior ladder
(Coulter 12:30 general firing → Doles ~13:00 formation → Stone/Dobke 13:30
→ tablet 14:00 adopted → Hill ~14:30 corps retrospective).

**Canonical choice:** CA-J1P-1 KEEPS **14:00 adopted, envelope 13:30–14:45
unchanged** (the 13:30 pole already sits inside the standing envelope), and
gains the full interior ladder as recorded structure — the same annotation
treatment ED-65 gave CA-J3M-1's opening cluster. Stone's and Dobke's
clockProfiles resolve from "pending revision" to a named early-lean class
(**report-nominal, offset −20, envelope [−45, 10], medium confidence** — the pass-11
provisional figures, now final) — descriptive of the skew, not
anchor-moving, per ED-25 rule 4. **Rationale:** both primaries are
report-nominal kind (a Union brigade commander's after-action narrative
and a regimental commander's, both written after the fact); rule 4 exists
exactly for this shape of evidence — real, cross-corps, and still not
promoted to redefine an anchor a tablet-adjudicated reading already
occupies within its own envelope. **Alternative rejected:** widening the
front envelope to 13:30 flush with the pole — the envelope already covers
13:30; nothing about the pole falls outside the standing bracket, so a
widen-only move would edit metadata for a case already inside tolerance.

### ED-70 — The CA-J1P-5 cross-line basis upgrade (no move)

**Canonical choice:** CA-J1P-5's evidence basis upgrades from tablet +
Pfanz-via-Wikipedia to the **Perrin-vs-receiving-cluster cross-line
pair** as its primary spine — Perrin ("I remained in this position
probably until after 4 o'clock, when I was ordered by General Pender to
advance") against the four-source receiving cluster (I Corps tablet "At
4 P.M. the Corps retired"; Dobke "about 4 p.m., when the First Corps, on
our left, gave way"; Jacobs "At about 4 o'clock the regiment rallied";
Chapman Biddle "compelled about 4 p.m. to retire") — the same
cross-line-agreement pattern the chain now runs on at CA-J1M-4b/CA-J1P-3.
Robinson's rear-guard "until nearly 5 p.m." is added as the recorded
**back-edge tail**, extending the anchor's documented activity without
moving its adopted time. **Adopted time and envelope UNCHANGED**
(~16:00–16:30, 15:45–16:45) — the pair already sits inside the standing
bracket. **Rationale:** ED-62's discipline exactly — an executor-sourced
basis upgrade on an anchor whose adopted arithmetic is untouched;
Robinson's tail is exactly the kind of negative-space fact (how late the
position was actually held) the executor-first doctrine exists to surface.

### ED-71 — The failed-row-arithmetic rule (extends ED-49/ED-52)

Pass 11's cheap-cell re-reads surfaced two return rows that fail their OWN
printed arithmetic in both fetch channels: Iverson's total row
(130+328+308 = 766 by column sum, vs the row's printed total of 820) and
the 57th NC aggregate (the printed "623" fails quadruple column
cross-checks that force 62). Neither ED-49 (return-vs-report scope) nor
ED-52 (report-vs-return conflict) covers a printed table disagreeing with
itself.

**Canonical choice:** where a printed return row fails its own row/column
arithmetic and the failure reproduces in two independent fetch channels
(verified-not-OCR), the audit (a) adopts the **multiply-cross-checked
arithmetic reconstruction** as the basis for any EC6 computation that
depends on the row, (b) carries the **printed reading verbatim** on the
record as a first-class conflict component (never silently corrected
away), and (c) tags the cell class **`printed-arithmetic-conflict`** so
future passes can find the pattern without re-deriving it. Exhibits:
Iverson's wounded/total row (printed 820; arithmetic 766; the 382-vs-328
wounded-cell dispute rides the same row); the 57th NC row (printed 623;
arithmetic 62); the mirror case already on the record under ED-49 (the
Scales report-table 545 vs return 535). **Rationale:** two independent
cases in one pass establish print-layer defects as a distinct evidence
class from source disagreement or OCR noise; the rule gives EC6 a
computation basis without pretending the printed number was never there —
exactly ED-49's own headnote discipline ("approximative… many of the
'missing' were killed or wounded"), extended to arithmetic instead of
scope.

## Dossier pass 12 adoption rulings (ED-72 … ED-73) — 2026-07-13

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by dossier pass 12 (`docs/reconstruction/audit/dossier-pass-12.md`
§5, drafted on branch audit-dossiers-12) are ADOPTED as recorded below,
executed at the start of dossier pass 13 (branch audit-dossiers-13). No
renumbers. ED-72 installs the East/South Cavalry Field chain as a new
skeleton (§2.6 of `anchor-chain-proposal.md`, added this pass, hardened
with the EC3 sheet-crop exercise below); ED-73 names the CSA
cavalry-brigade position-marker class as its own evidence subclass,
distinct from ED-39's War Department tablet class.

### ED-72 — The East/South Cavalry Field chain (CA-ECF-1…3, CA-SCF-1…2)

**Canonical choice:** adopt the five-anchor skeleton exactly as pass 12
built it from the executor dossiers' own tablets/reports (full text:
`anchor-chain-proposal.md` §2.6, ADOPTED table below) — CA-ECF-1
(~noon, envelope 11:00–13:00, tablet-adjudicated four-primary CSA
cluster), CA-ECF-2 (~14:00, McIntosh=Custer tablet pair), CA-ECF-3
(~15:00, McIntosh=2nd Division tablet pair), CA-SCF-1 (~noon/13:00,
Merritt's own report, PRIMARY not tablet-class), CA-SCF-2 (~17:30,
triple agreement — 3rd Division tablet + 1st Brigade tablet + Merritt's
own report). Adopted as a **fifth-tier evidence chain**: tablet-
adjudicated alongside the ED-39 monument class, with the ED-73
marker-provenance caveat attached to every CA-ECF anchor (below) —
**not** promoted to report-primary tier, since only CA-SCF-1/2 rest
partly on report primaries. **Precondition carried, not blocking
adoption:** a primary-report pass on Stuart's full OR report (identified
pp. 679-720, not exhaustively fetched) and Kilpatrick's report
(identified, not fetched) remains open; if either report's East/South
Cavalry Field section surfaces a clock materially outside the adopted
envelopes, the anchor is revised under the standing adopt-and-adjust
rule, not silently held. **Rationale:** the same rationale pass 12
recorded — no R1 skeleton existed for this theater; a from-scratch
chain built off executor primaries/tablets is stronger evidence than
leaving the theater unchained while the field's own dossiers already
cite CA-ECF/CA-SCF ids. **This pass's addition to the basis:** the EC3
sheet-crop exercise (§2.6, below) — the Bachelder 12441 (East Cavalry
Field) sheet set, fetched and read this pass for the first time — draws
EVERY CA-ECF-executing brigade at brigade-block grain with the
sheet's own hand-marked clock annotations (e.g. "JENKINS 2 P.M.",
"CHAMBLISS 2½ P.M.", "1 N.J. BEAUMONT 2½ P.M.") independently
corroborating the tablet-adjudicated adopted times without being
counted as a new primary tier (drawn map annotations are ED-73-class
evidence, same caveat as the position markers — see below).

### ED-73 — The CSA cavalry-brigade position-marker class

**Canonical choice:** adopt the gettysburg.stonesentinels.com
"confederate-headquarters" cavalry pages (Hampton's, Fitz Lee's,
Chambliss's, Jenkins's, Stuart's Division, Stuart Horse Artillery) —
and, by the same reasoning, the Bachelder East Cavalry Field sheet
set's own hand-lettered unit blocks and clock annotations — as a named
**monument-adjacent evidence subclass**, distinct from ED-39's War
Department troop-tablet class: no erection date or adjudicating body
was found on any fetched marker page, and the Bachelder sheets are a
19th-century cartographer's own compiled adjudication (Bachelder
interviewed veterans and compiled composite positions decades after
the battle — a different process than the War Department Commission's
tablet-siting work ED-39 profiles). **Weight:** ±30 min class per
event for clock content; position content from this class earns EC3
anchors at the sheet's own registration-residual radius
(**~75-80 m**, per `bachelder-manifest.json`'s ECF `estAbsUncertaintyM`
~76-77 m) but is **never anchor-defining alone until corroborated** —
exactly the same discipline ED-72's CA-ECF anchors already apply
(tablet + sheet, not sheet alone). **Rationale:** CSA cavalry
essentially has NO other physical marker record at Gettysburg (no
War Department tablets for CSA units generally, and the ECF/SCF
theater additionally lacks the monument density of the main field) —
ED-39 §4's asymmetry-by-side rule in its strongest form. Naming the
class lets future passes weight it consistently instead of re-deriving
the caveat dossier by dossier.

`anchor-chain-proposal.md` §4's status table and the dossiers index's
ED-counter updated in place.

---

## Dossier pass 13 adoption ruling (ED-74) — 2026-07-14

Owner ruling (standing adopt-and-adjust doctrine): the ED candidate
proposed by dossier pass 13 (`docs/reconstruction/audit/dossier-pass-13.md`
§5, drafted on branch audit-dossiers-13) is ADOPTED as recorded below,
executed at the start of dossier pass 14 (branch audit-dossiers-14). No
renumber needed.

### ED-74 — The report-vs-corroborated-tablet-conflict class (extends ED-71)

Pass 13's Lane's (Sumter, Georgia) Artillery Battalion dossier
(`csa-3c-bn-lane.md` §"EC6") surfaced a third conflict shape distinct
from both standing arithmetic-conflict rules: Lane's own report gives an
internally-consistent battalion loss total ("Men killed, 3; wounded
seriously, 2; severely, 7; slightly, 13; missing, 4; total loss of men,
29") that disagrees by exactly one man's worth of arithmetic with the
battalion tablet's total (30), where the tablet figure is INDEPENDENTLY
cross-checked — the three constituent battery markers (Ross's,
Patterson's, Wingfield's) sum digit-exact to 30, not to 29. Neither
ED-71 (a single printed table failing its OWN internal arithmetic) nor
ED-49/ED-52 (a report's scope disagreeing with a return's different
scope) covers this: here TWO different evidence classes (a commander's
own report; an independently-summed tablet/marker consensus) each pass
their own internal-consistency check and still disagree by a small,
genuinely irreducible margin.

**Canonical choice:** name this conflict shape
`report-vs-corroborated-tablet-conflict` and, wherever it recurs, adopt
the **battery-marker-corroborated tablet figure as the EC6 basis**
while carrying the **report's total verbatim as a first-class
component** (never averaged, never silently dropped) — the same
carry-both discipline ED-71(a)/(b) already apply to a different
evidence-class pairing, extended here to cover report-vs-tablet instead
of table-vs-itself. Tag the affected cell `report-vs-corroborated-
tablet-conflict` so future passes can find the pattern without
re-deriving it. Exhibit of record: Lane's Sumter battalion, 29 (report)
vs 30 (tablet, corroborated by the three-battery marker sum) —
`csa-3c-bn-lane.md` EC6 and Conflicts sections carry the full text and
now cite this ruling by name rather than "NOT self-adopted."
**Rationale:** the tablet reading here is not "preferred because
official" (that would violate ED-39's own caution about tablet
casualty scope, ED-43) — it is preferred because it is the ONLY one of
the two readings independently reproduced from components fetched
separately from the total itself (three battery pages, none of which
cites the battalion tablet or Lane's report); the report's total, while
self-consistent, is a single unverified source on this specific
question. Where a future case lacks that independent-sum corroboration,
this rule does NOT apply and the conflict stays carried unresolved
per ED-71's baseline discipline.

`csa-3c-bn-lane.md`'s "ED candidates proposed" section is updated to
record the adoption rather than re-propose it.

---

## Day-expansion slice 2 ruling (ED-75) — 2026-07-13

### ED-75 — Marshall's and Davis's brigades: PROVISIONAL-ADOPTED placeholder July-3 strengths

Day-expansion slice 1 (ED-46/47/48, executed by
`tool/scripts/author-dayexp1-rebase.ts`) re-based every CSA assault-column
brigade with a primary July-3 basis — except Marshall's and Davis's, which
have none: both brigades' only strength evidence is a July-1 "present"
tablet compilation (~2,000 each) and a whole-battle casualty return with no
clean day split. The slice-1 script left both **annotated in-file as an
open residual** ("a [B] subtraction re-base is an owner ruling, not an
executor default"), pending the owner's stated intent to purchase Busey &
Martin, *Regimental Strengths and Losses at Gettysburg* — the page-level
per-brigade engaged-strength source the rest of the OOB register already
treats as the superseding authority where cited as a hop (ED-38).

**Owner ruling, 2026-07-13** (plain instruction: "should we do an inference
in the meantime as a placeholder?"): **yes** — both brigades get a
documented-inference placeholder, PROVISIONAL-ADOPTED, following the
Pfanz-gate pattern (ED-64: the record is adopted, the resolution is not).

**Canonical choice — the method (both brigades, one class of inference):**
arrival strength = (July-1-engaged regiments' post-July-1 survivors) + (any
regiment fresh to July 3). Where a regiment's own July-1 losses have no
primary total, the brigade's printed whole-battle return (k+w; per ED-49,
missing is usually absent or brigade-pooled) is read as
**predominantly July-1-incurred** — both brigades' own dossiers document a
single defining July-1 casualty event that plausibly accounts for most of
the printed total (the 26th NC's stand opposite the Iron Brigade; the
railroad-cut trap, "about three hundred Confederates surrendered").

- **csa-marshall: 900** (envelope 750–1,050). 2,000 (West Conf Ave tablet,
  "present on the first day," inferred/compilation tier) minus ~1,100 read
  as July-1-incurred (the brigade's whole-battle regimental return, 190 k /
  915 w = 1,105, **no missing column printed at all** — or-27-2-anv-return
  p. 344, the ED-49 exemplar) = 900. The 26th NC's own primary day-split
  ("We went in with over 800 men... There came out but 216" July 1;
  "216... only about 80" July 4, or-27-2-young-26nc) is the basis for
  reading the brigade total as dominated by July 1. This is not a new
  figure — it is the dossier's own EC2 subtraction (`csa-3c-het-1-marshall.md`),
  promoted from a citation annotation to the actual keyframe value. Envelope
  poles: Wikipedia/military-history.fandom's uncited "roughly 2,500" brigade
  base (disagreeing with the 2,000 tablet compilation, recorded verbatim,
  NOT averaged) on the high side; the return's wholly-absent brigade-wide
  missing/captured mass on the low side.
- **csa-davis: 1,293** (envelope 1,100–1,550), a two-term sum. **Term 1**
  (the three July-1-engaged regiments, 2nd/42nd Miss + 55th NC): 2,000 (the
  West Conf Ave tablet's "present on the first day," read as these three
  only — the same tablet separately states "Joined later by the 11th
  Regiment previously on duty guarding trains") minus ~1,100 read as
  July-1-incurred (regiment-grain whole-battle return for these three,
  232+265+198=695 k+w, plus this trio's share of the brigade's ~500 missing
  after crediting the 11th Mississippi's own 37 captured, or-27-2-anv-return
  p. 344) = 900, mirroring Marshall's identical subtraction class. **Term 2**
  (the 11th Mississippi's own July-3 arrival): 393 — TWO independent
  GBMA/War Department markers (the main brigade tablet and the Bryan-barn
  position marker) agree digit-exact on "Combatants – 393" despite
  disagreeing on the internal killed/non-casualty split (110 vs 100 killed,
  53 vs 39 non-casualty) — an ED-39 marker-corroboration case. **Sum:
  900 + 393 = 1,293.** Envelope poles: the tablet-vs-uncited-secondary
  three-regiment base disagreement (Emerging Civil War, uncited, no
  footnote, "carrying some 1,508 men into battle on July 1st") on the low
  side; the 11th Mississippi's own strength reported elsewhere as 592 via an
  **unverified web-secondary citation of Busey & Martin** (a hop, not a
  page read) on the high side. Davis's one attested PRIMARY delta — "In
  Davis' brigade 2 men were killed and 21 wounded" during the cannonade
  (or-27-2-davis-jr) — is preserved as an absolute −23 applied to the new
  base, not rescaled: 23 real casualties do not change with which reading
  of the brigade's total strength is adopted.

**Decay curve:** every keyframe after t=0 (t=7200 for csa-davis, where the
attested −23 anchors it) is **proportionally rescaled** from the existing
ABT-map-reconstruction decay shape — no independent per-keyframe
re-derivation exists or is claimed for the charge/repulse/withdrawal
geometry, only the strength magnitude moves.

**Confidence: INFERRED** on every touched keyframe (never `documented` —
neither brigade has a primary July-3 total; ED-25/ED-49's citation-gate
discipline applies). **Precondition (the hardening gate, Pfanz-gate
pattern):** Busey & Martin page-level per-brigade figures supersede this
placeholder on arrival — the owner has stated purchase intent. Until then
this ruling is **PROVISIONAL-ADOPTED**: the record (the method, the cited
inputs, the envelope) is adopted now so authoring is not blocked on a
future book purchase; the specific numbers are explicitly not final.

**Rationale:** an executor default would either invent false precision (a
bare number with no envelope) or leave the residual open indefinitely
(silently understating the compiled cast's honesty about these two units,
which were already the batch's clearest heterogeneity exemplar). A
documented-inference placeholder — envelope carried, confidence marked,
precondition named — is the same discipline the corpus already applies to
Lowrance's and Brockenbrough's July-3 loss shares (ED-48b) and is
explicitly an owner ruling, not a silent executor promotion of a [B]-tier
subtraction to a `documented` figure.

**Consequence:** neither `csa-marshall` nor `csa-davis` is in the compiled
Angle cast (`angle.reconstruction.json`, slice t=8040..9000) — this ruling
touches no shipped film state; `stagingSeed d470c469…` is unaffected by
construction, verified by the same film guard pattern ED-46 established.
Full inputs, quoted verbatim with disagreements preserved, and the
derivation arithmetic: `docs/reconstruction/audit/ed75-placeholder.md`.
Implementation: `tool/scripts/author-ed75-placeholder.ts`.

---

## Decomposition wave 1 adoption rulings (ED-76 … ED-77) — 2026-07-13

Owner ruling (standing adopt-and-adjust doctrine): the ED candidates
proposed by decomposition-wave-1 (`docs/reconstruction/audit/
decomposition-wave-1.md` §6, drafted on branch decomposition-wave-1) are
ADOPTED as recorded below, batch-executed at the start of
strength-reconciliation-1 (branch strength-reconciliation-1). No
renumbers were needed this batch. **The owner may re-rule either
decision later**; adopt-and-adjust means authoring is not blocked on a
ruling window, not that the ruling is closed to revision.

### ED-76 — Single-regiment promotions use the parentless "minus" convention, not `parent`/family

**Question:** when a wave promotes ONE regiment out of a brigade whose
other regiments stay at aggregate grain, should the promoted regiment
get `parent` (the Harrow/Webb/Stannard/Hall family mechanic) or be
authored as a parentless sibling (the Carroll's-Brigade/8th-Ohio
pattern already shipped in `gettysburg-july3.json`)?

**Adopted as proposed** (decomposition-wave-1.md §1/§6): `Battle
Director.cs`'s family-suppression contract (`RendersAtTier`) hides the
parent's symbol at every tier nearer than Block once it has ANY
children — correct when the children cover ALL or NEARLY ALL of the
brigade (Harrow 4/4; Webb 3/4), wrong for a 1-of-5 or 1-of-6 promotion
(it would erase 80%+ of the brigade from view at the Regiments/Soldiers
tiers). Single-regiment (or small-minority-regiment) promotions are
authored as parentless siblings; the source brigade is renamed "...
minus the Nth ___" and its own strength track reduced by EXACT
subtraction (not the `parent`/children ±15% advisory) at every shared
keyframe time. `parent`/family stays reserved for decompositions
covering all or nearly all of a brigade's regiments.

**Applied by decomposition-wave-1:** `us-6wi`, `us-147ny`, `us-16me`
(the Meredith/Cutler/Paul(Coulter) aggregate units renamed and re-based
accordingly) — already shipped on main (4fbef4a) under this wave's own
reading of the convention; this ruling formally adopts that reading as
doctrine so future waves don't have to re-derive it from the
`us-carroll`/`us-8oh` precedent by inspection. **Recommended follow-up**
(decomposition-wave-1.md §10 item 3): write the "minus the Nth ___"
naming/exact-subtraction convention into `battle-format.md` alongside
the existing `parent`/children section — not done by this batch
adoption (doc-only ED entry; the format-doc edit is separate work).

### ED-77 — The Ziegler's Grove convergence: post-repulse legs authored, Williston's conflict carried on the authored reading

**Question:** `authoring-wave-a2.md` cut the four Ziegler's Grove
batteries' post-repulse move for want of new units; decomposition-wave-1
lifted that constraint. Should all four batteries' post-repulse move be
authored uniformly (including Williston's, whose own individual tablet
conflicts with the move), or should Williston be held back at his own
tablet's Taneytown Road reading pending resolution?

**Adopted as proposed** (decomposition-wave-1.md §4/§6): the A2 report's
own recorded pickup framing ("author the four as units... with the
Williston conflict carried") reads as authoring the move for all four,
with the conflict text carried on Williston's own citation — not as an
instruction to exclude him. Both other batteries' tablets (Butler's most
explicitly, Harn's by the parent dossier's four-battery grouping)
independently corroborate the same ground and trigger. Williston's move
is authored; his own tablet's "Not engaged"/Taneytown-Road reading is
carried in full on the same keyframe's citation; neither reading is
adjudicated. No fire event is authored for any of the four batteries
(matches every one of their own dossiers: zero casualties, no fire
narrative, or "Not engaged").

**Applied by decomposition-wave-1:** `us-btty-williston`, `us-btty-
butler`, `us-btty-martin-5us`, `us-btty-harn` (already shipped on main,
4fbef4a) — this ruling formally adopts the reading. Film-safety already
verified by decomposition-wave-1.md §5 (structural containment: the
four batteries' first documented activity is t=9000, outside the film
viewpoint window 8160..8820, no fire event authored at any time;
re-verified independently by strength-reconciliation-1, which touched
neither `gettysburg-july3.json` nor the compiled bundle — see
`docs/reconstruction/audit/strength-reconciliation-1.md` §5).

---

## July 3 morning slice ruling (ED-78) — 2026-07-13

### ED-78 — Owner ruling: provisional inference is authorized where the primary record conflicts, superseding the ED-30/ED-64 Pfanz-gate BLOCK

**Owner ruling (verbal, this session, 2026-07-13), ADOPTED as recorded
below** — the owner's own words: **"make a best guess inference for
right now [before we proceed]."**

**Question:** the July 3 morning phase (Culp's Hill, ~04:30-13:00 LMT)
was blocked from authoring by the standing ED-30/ED-64 precondition — a
library-terminal fetch (Pfanz, *Culp's Hill and the Battle for
Gettysburg*, pp. ~340-355) that no executor pass had been able to
commission, leaving the Spangler's Meadow charge (CA-J3M-3, the 2nd
Massachusetts/27th Indiana charge under Lt. Col. Mudge) an unresolved
two-pole primary conflict (dossier-pass-9.md §3; ED-64) with no
authorable render time. Everything ELSE in the July 3 morning chain
(CA-J3M-1, -2, -4, -5) was already executor-hardened and authorable —
this single anchor was the one standing BLOCK on the whole phase.

**Adopted as ruled:** for remaining movements in question across the
reconstruction where the primary record conflicts and no further cheap
fetch is available, a PROVISIONAL best-guess inference is now permitted,
**PROVIDED**:

1. The inference is explicitly marked `"confidence": "inferred"` (or
   carries an equivalent PROVISIONAL label in its citation/note) — never
   rendered as `"documented"` fact.
2. **Both poles of the conflict stay on the record** — the rejected
   pole's primary sources are quoted verbatim alongside the adopted
   pole, never silently dropped.
3. **This ruling (ED-78) is cited from every place the provisional
   inference is used** — so a future pass with better evidence (the
   Pfanz fetch, still recommended, still open) can find and revise every
   authored instance.

**This specifically supersedes the ED-30/ED-64 Pfanz-gate BLOCK**: the
CA-J3M-3 Spangler's Meadow charge is now authorable at its **~10:00
direction** (the order-chain + Bachelder-drawn-sheet pole; ED-64 §
"LATE pole ~10:00"), carrying the **~05:30-06:00 EARLY pole** (Morse's
"5.30 o'clock"; the 27th Indiana's advance marker inscribed "6 a.m.")
**verbatim alongside** on every keyframe, event, and report reference
that uses the ruling. The Pfanz *Culp's Hill* pp. ~340-355 fetch remains
a recommended hardening target — ED-78 unblocks authoring, it does not
retire the open research item.

**Applied by the July 3 morning slice**
(`docs/reconstruction/audit/july3-morning-slice.md`): the `us-2ma` and
`us-27in` charge keyframes (t≈10:00, `confidence: "inferred"`) and their
`j3m-spangler-2ma`/`j3m-spangler-27in` fire events in
`gettysburg-july3-morning.json`, each carrying both poles verbatim in
its citation/note text and citing ED-78 by name. No other anchor in the
July 3 morning chain needed ED-78 (CA-J3M-1/2/4/5 were already
executor-primary, non-provisional).

**Scope note:** ED-78 is a STANDING permission for future provisional
calls under the same three conditions, not a one-time exception — future
passes may cite it directly rather than re-litigating the "is a
best-guess inference allowed here" question, but each individual
provisional call still needs its own both-poles citation per condition
2. The owner may re-rule this (adopt-and-adjust doctrine; the ruling is
open to revision, not closed by being recorded).

**Owner confirmation (2026-07-14, strength-reconciliation-2):** the
`july3-morning-slice.md` §9 owner-question #1 asked explicitly whether
this "standing permission" framing (above) was the intended breadth, or
whether each future provisional call should get its own numbered ED. The
owner answered **"yes"** to the standing-permission reading — this is now
an OWNER-CONFIRMED ruling, not merely the executor's own interpretation
of ED-78's text. `strength-reconciliation-2` is the first pass to cite
ED-78 directly for a NEW provisional call under this confirmed scope (the
`csa-daniel` strength + position-class best-guess, per owner ruling (c)
of that pass's task brief) — see
`docs/reconstruction/audit/strength-reconciliation-2.md` §2.

---

### ED-79 and ED-80 — ADOPTED (owner ratification, 2026-07-15: "you are good to go" on the adjudication)

Adjudicated in docs/reconstruction/audit/angle-facing-adjudication.md; ratified verbatim.

### ED-79 — The rout/dissolution exemption to the retrograde-facing convention

The retrograde-facing owner ruling (2026-07-13, `retrograde-facing.md`)
governs **formed units** retiring under fire: they hold their combat facing
and move retrograde. It does not govern **attested flight**: where a leg's
formation state is `scattered` or `routed` (or its segment action is
`fall_back`/rout-class with a dissolution-class dossier record), the men
attestedly turned and ran, and facing = direction of travel is the correct
depiction. Concretely:

1. A moving leg whose start AND end formations are both in
   {`scattered`, `routed`} is exempt from the hold-facing convention when
   the unit's dossier/citations attest dissolution-class withdrawal.
2. The exemption is per-leg and evidence-backed — it never applies to a
   `line`/`column`/`line_disordered` leg, and a rout exemption must cite
   the attestation (OR report, dossier EC-4 entry, or ED).
3. The map grammar already encodes the distinction (chevron and solid
   borders for ordered formations only); no renderer change is implied.
4. `fix_retrograde_facing.py` needs no code change for the current corpus
   (its five-file rewrite already ran; the only legs it would have wrongly
   converted are the four film-pinned ones it was forbidden to touch). Any
   FUTURE deterministic re-run over new authoring must incorporate rule 1
   before converting rout-class legs.

### ED-80 — Dispositions for the six deferred Angle-cast legs

1. `csa-garnett`/`csa-kemper`/`csa-armistead`/`csa-fry` t=8700→9000:
   **ATTESTED-FLIGHT under ED-79 — authored facing stands, permanently.**
   `retrograde-facing.md` §4's standing recommendation (apply the
   formed-unit hold "whenever the owner authorizes an Angle-cast
   re-cast/recompile wave") is WITHDRAWN — the mooted fix would contradict
   Peyton's/Shepard's reports, the dossiers' dissolution-class records,
   ED-8, and the shipped film's own repulse footage (t=8700–8820).
2. `us-stannard` t=8400→9600 and t=10200→10800: **ATTESTED-MANEUVER** —
   cited change-of-front/return facings stand (the `us-13vt`/`us-16vt`
   precedent, CA-J3A-8/9).
3. The film-safety pin on the 13 Angle-cast units' keyframes is unchanged
   and stays in force (sha256 `69163017…`); nothing in this ED authorizes
   touching them.
4. `test_retrograde_facing.py`'s `ALLOWED_LARGE_DELTA_LEGS` annotations now
   cite ED-79/ED-80 (this wave, comment/annotation-only): the four entries
   are doctrine (correct depictions), not TODOs awaiting a fix.


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
