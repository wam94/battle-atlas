# Charge-intensity research pass — evidence bank and proposal set

Punchlist item 4 (`docs/reconstruction/post-v2-punchlist.md`): "a lack of
action during the actual charge — the scale of the carnage isnt clear,
the soldier is not on the frontline himself, and there's a lot of
standing around." Research-first per the punchlist's own framing. **No
battle-file, bundle, scene, or runtime changes are made by this
document** — it is evidence plus a sliced proposal set for the owner to
rule on, on branch `charge-intensity-research`.

Every proposal below is bound by `docs/reconstruction/violence-and-representation.md`
(the sober-representation doctrine) — cited inline as "V&R §n" — and is
checked against it explicitly in §4.

## 0. Sources consulted

- `reconstruction/dossiers/*.md` (197 files, 15 audit passes,
  `docs/reconstruction/audit/dossier-pass-1..15.md`).
- The compiled Angle bundle: `app/Assets/Battle/Angle/angle.bundle.json`
  (13 units, 40 casualty profiles, slice t=8040–9000 = 15:14:00–15:30:00).
- `docs/reconstruction/angle-bundle-audit.md` (Gate P5 compiled-bundle
  report — strength reconciliation and slice-loss totals by unit).
- Render code: `app/Assets/Scripts/CasualtySchedule.cs`,
  `CasualtyDressing.cs`, `SoldierActionResolver.cs`, `KitClips.cs`,
  `SoldierView/ViewpointDefinition.cs`, `SoldierView/ViewpointObservers.cs`.
- `app/Assets/StreamingAssets/SoldierView/viewpoints.json` (the one
  committed observer).
- `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` §3.3–3.5
  (viewpoint scope and render-budget doctrine).

All quantitative pacing numbers in §1 are derived by re-implementing
`CasualtySchedule.InvCdf` in Python against the compiled bundle's own
profile data (t0/t1/count/intensityCurve) — i.e. they describe what the
shipped resolver *actually computes*, not an independent estimate.

---

## 1. Casualty pacing — evidence bank

### 1.1 The Angle, July 3 (the rendered window, t=8040–9000)

The bundle's 13-unit, 40-profile compiled schedule **is** the pacing
evidence already assembled by the V2 compiler; the gap is between what
it computes and what a viewer actually perceives. Aggregate CSA infantry
losses (Garnett + Kemper + Armistead + Fry, the four brigades that reach
the wall), bucketed at 30 s from the compiled `InvCdf` fall times:

| Window | Duration | % of runtime | Losses | % of losses |
| --- | --- | --- | --- | --- |
| 15:14:00–15:18:00 (advance, 2 fence crossings, dress-line) | 4 min | 25.0% | 105 | 4.0% |
| 15:18:00–15:23:00 (final approach — canister/musketry rise) | 5 min | 31.3% | 972 | 37.5% |
| 15:23:00–15:25:00 (wall fight — the spike) | 2 min | 12.5% | 706 | 27.2% |
| 15:25:00–15:30:00 (repulse — falling) | 5 min | 31.3% | 810 | 31.2% |
| **Total** | **16 min** | 100% | **2,593** | 100% |

Source: `angle.bundle.json` `casualtyProfiles[*].{t0,t1,count,intensityCurve}`
for `csa-garnett`, `csa-kemper`, `csa-armistead`, `csa-fry`, re-integrated
with `CasualtySchedule.InvCdf`.

**The first quarter of the rendered window carries essentially none of
the visible loss** (4.0% of 2,593 falls in 25% of the runtime) — this is
the "lot of standing around" the owner is describing, and it is not an
authoring accident: the compiled `cas-*-approach` / `cas-*-at-fences`
profiles are genuinely small counts (`n=13`, `n=20` for Garnett;
`n=12`, `n=10` for Kemper) spread `uniform` over these minutes, because
the evidence puts the real losses later (below). The render is faithful
to the evidence here; the *perception* problem is that faithfully-sparse
early casualties read as "nothing is happening" without any other signal
of danger (see §2 for what's missing besides falls).

**Per-unit peak rate (Garnett's Brigade only, 15 s buckets):**

| Window | Losses/15s (peak bucket) | Instantaneous rate |
| --- | --- | --- |
| 15:14:00–15:18:00 (early) | 1–4 | ≈0.1–0.25 men/s |
| 15:23:45–15:24:15 (wall-fight peak) | 37 | ≈2.5 men/s |

A ~15–25x rate increase — but note the *shape*: `InvCdf("spike")` is an
inverse-smoothstep S-curve, not a burst. The wall-fight peak (37/15s) is
only ~1.25x the immediately-preceding rising-phase rate (30/15s at
15:22:45–15:23:00) — **the compiled curve ramps smoothly through the
spike rather than concentrating losses into a few violent seconds tied
to actual discrete events** (a canister discharge, a volley). See gap
G1 below.

**Cross-check against the primary source.** Peyton (Garnett's report,
`peyton-or-1863`, cited in `reconstruction/dossiers/csa-1c-pic-1-garnett.md`
EC5.4): "more than half having already fallen" by the wall fight. The
compiled schedule: Garnett alive at t=8580 (wall-fight segment start) =
1,050 of 1,393 start strength; alive at t=9000 (slice end) = 700 —
losses = 693/1,393 = **49.8%**, matching Peyton almost exactly. The
*aggregate* totals are faithful to the record; §1.1's finding is about
*pacing shape*, not total-count accuracy.

**The invisible hour: bombardment casualties are entirely pre-slice.**
Peyton, `csa-1c-pic-1-garnett.md` EC6: **"During the shelling, we lost
about 20 killed and wounded"** — ~2% of strength, including Lt. Col.
John T. Ellis (19th Va) killed. The bundle's slice starts at t=8040
(15:14:00), well after the ~1-hour cannonade (Peyton: "kept up without
intermission for one hour"); Garnett's compiled starting strength
(1,393) is already net of these losses. **No fall, no reaction, no
blood plays for this loss episode at all** — it is baked into the
starting headcount with zero visible cause. Kemper's brigade suffered
worse: Wilcox, watching from Kemper's right rear, "The brigade lying on
my right (Kemper's) suffered severely" during the cannonade
(`or-27-2-wilcox`, cited `csa-1c-pic-2-kemper.md` EC5.1) — no number
survives, but the comparison places it above Garnett's ~20, and it is
equally invisible in the render.

### 1.2 Contrast — July 1, Iverson's Brigade (the single-spike exemplar)

`reconstruction/dossiers/csa-2c-rod-2-iverson.md` — the audit's own
"EC6 SHAPE EXEMPLAR" label. **500 of ~820–1,470 engaged men fell in ONE
episode**, the Forney-field firing line, inside a single ~14:00–15:00
bracket: "advanced to within 100 yards" of a concealed stone wall, the
enemy held its fire, then "500 of my men were left lying dead and
wounded on a line as straight as a dress parade" (Iverson's own report,
`or-27-2-iverson`). Independently, Rodes: "His dead lay in a distinctly
marked line of battle." This is a **front-loaded, near-instantaneous
collapse** — not a ramp. There is effectively no "advance" phase with
casualties at all; the brigade closes to ~100 yards essentially
unhurt, then the entire loss happens at once when a withheld volley
opens. Contrast with the Angle's compiled shape (§1.1): a ~16-minute
render with 65% of losses in the final 44% of runtime is *already*
back-loaded relative to a naive uniform pace, but Iverson's is a true
single spike — nearly all loss in a window a small fraction as long as
the approach that preceded it.

### 1.3 Contrast — July 2, the Wheatfield (attritional, multi-wave)

Not a single unit's ramp at all: `reconstruction/dossiers/us-iii-1-3-detrobriand.md`
documents "the Wheatfield's ORIGINAL garrison (occupation wave 1)... 
ammunition exhaustion timed the wave-2 hand-off" — de Trobriand's brigade
holds ~17:00–18:15 (Episode split, dossier EC), is relieved by Cross's
brigade (wave 2), which is itself relieved by Caldwell's further brigades
(wave 3) after Cross is mortally wounded "early in this engagement"
(Caldwell, `or-27-1-caldwell`, cited `us-ii-1-1-cross.md`). Cross's own
brigade report: **"330 total from 780 muskets"** (42%) — but that loss
accrues to a brigade that entered the field *after* two other brigades
had already fought there, over ground that changed hands repeatedly for
roughly two and a half hours (~17:00–19:30). The pacing "shape" here is
not a curve at all — it is a **sequence of discrete unit-relief
episodes**, each with its own casualty burst, stacked on the same ~9
acres.

### 1.4 The three-way contrast, stated plainly

| Engagement | Duration | Shape | Loss concentration |
| --- | --- | --- | --- |
| The Angle, July 3 | ~16 min (rendered), preceded by an invisible ~1 hr bombardment | single escalating charge → spike → repulse | 65% of loss in final 44% of render time |
| Iverson, July 1 | minutes (the kill window) inside a ~1 hr bracket | near-instant single spike | ~500 of ~800–1,470 (34–61%) in one volley exchange |
| Wheatfield, July 2 | ~2.5 hrs | sequential multi-wave attrition, ground changes hands | 42% (Cross alone) stacked atop two prior brigades' losses on the same ground |

This is the shape vocabulary the owner's "worth taking as individual
slices" framing implies exists across the corpus — the Angle is not a
generic "charge," it has its own specific pacing signature (long
faithful setup, genuine late concentration, but a *smoothed* concentration
that under-reads against Iverson's true instantaneousness).

---

## 2. Behavior under fire — evidence bank vs. resolver vocabulary

`KitClips.ClipId` (the complete pose vocabulary, `app/Assets/Scripts/KitClips.cs`):
`StandReady, March, RouteStep, DoubleQuick, RoutedRun, Aim, Fire, Reload,
Cross, HitNonfatal, FallBack, FallCrumple, FallSide, TurnRetreat, Waver,
KneelReady, Brace, Flinch, HaltDress, ProneCrawl` — 20 clips. The
punchlist's own framing ("brace/flinch/waver/kneel" already exists) is
correct as far as it goes; the gap is in what triggers them and what
isn't in the vocabulary at all.

### 2.1 Attested behavior bank (citations, mapped to resolver support)

| # | Behavior | Citation | Unit | Resolver support today |
| --- | --- | --- | --- | --- |
| 1 | Halt-and-fire instead of crossing an obstacle | "our men stopped and began firing, instead of mounting the fence; while making efforts to get them over the fence I was wounded" — Trimble, `trimble-shsp26-1898`, `csa-3c-pen-hq-trimble.md` | Lane's/Lowrance's brigades (Trimble's command; not in the current 13-unit Angle cast) | **None.** `cross_obstacle`/`breach` segments always complete on a deterministic hold+catch-up (`SoldierActionResolver.PositionUpTo`); there is no branch where a unit halts and exchanges fire AT an obstacle instead of climbing it. Only Garnett gets an explicit stall class (`take_canister`, 30% hash chance of `Waver`). |
| 2 | Hand-to-hand / clubbed-musket fighting at the wall | "hand to hand, and of the most desperate character; but more than half having already fallen... men climbing over the wall, and fighting the enemy in his own trenches until entirely surrounded" — Peyton, `csa-1c-pic-1-garnett.md` EC5.4; Fry's brigade "clubbed-guns melee" at Falling Waters (`csa-3c-het-3-fry.md` EC6, later action, same vocabulary gap) | Garnett's Brigade | **None.** `breach` segments resolve to `Aim`/`Fire`/`Reload`/`StandReady` via `FireCycles` — the same musketry cycle as an open-field firefight. No melee/bayonet/clubbed-musket `ClipId` exists. |
| 3 | Colors falling repeatedly / color-bearer succession under fire | "Every flag in the brigade excepting one was captured at or within the works": 1st Tenn 3 color-bearers down, 13th Ala 3, 14th Tenn 4, 7th Tenn 3 (flag saved by Capt. Norris tearing it from the staff) — `or-27-2-shepard`, `csa-3c-het-3-fry.md` EC5.4 | Fry's Brigade (`csa-fry`, in-build) | **None.** No standard/color-bearer distinguished role exists in `SoldierState` (the `variant` byte is cosmetic kit variety only, per `SoldierActionResolver.Resolve`); no flag prop, no "colors fall, are retrieved, fall again" event chain. |
| 4 | Mounted officer shot down among the infantry | Garnett "shot from his horse... within about 25 paces of the stone wall," riding "against the dismount norm because he was too unwell to walk" — `peyton-or-1863`, `csa-1c-pic-1-garnett.md` EC1/EC4 | Garnett (brigade commander) | **None — structurally absent.** `FormationRoster.cs`: "no horses are modeled." There is no mounted rig, and per V&R §1, named officers are represented only at unit level in this phase — any mounted-Garnett vignette needs explicit owner sign-off, not just an art asset (see proposal P5). |
| 5 | Men lying/kneeling under fire at the wall (defenders) | "the men, lying down, poured into him so well-directed a fire that he halted, fell back, and finally broke in great disorder" — Hall, `or-27-1-hall` p. 436, `us-ii-2-3-hall.md` EC5.1 (**dated July 2**, same brigade defends the Angle July 3, tactic not independently re-attested for July 3) | Hall's Brigade (`us-hall`, in-build) | **Partial, and misdated if reused as-is.** `KneelReady` exists (20% hash chance during `waver`/artillery-crew idle) but no unit's July-3 `hold`/`fire_by_rank` segment triggers it — every defending unit stands the full "hold" window (`Webb`: 7 min, `Hall`: 5 min, `Stannard`: 1 min) in `StandReady` only. Citation is July 2; reuse for July 3 needs its own sourcing pass, not an inference (punchlist item 0's "do not invent motion" rule applies). |
| 6 | Line "wavers, halts, fires, breaks" at close range (repulse) | "The enemy halted to deliver his fire, wavered, and fled" — Hall, p. 439, `us-ii-2-3-hall.md` EC5.3 | Kemper's Brigade (attacking, receiving end of Hall's account) | **Implemented.** Kemper's own segment table carries an explicit `waver` action (15:24–15:25) resolving to `Waver`/`KneelReady`; this is the one behavior class the resolver already expresses end-to-end. |
| 7 | Drift toward the point of least resistance under flanking fire | "The steady fire from McGilvery and Rittenhouse... caused Pickett's men to 'drift'" — Hunt, `hunt-bl-3`, `us-btty-rittenhouse.md` | Kemper's Brigade (right flank) | **Implemented at the formation level.** Kemper's `oblique` segment (15:18–15:20) models this; individual gait during the oblique is a plain `March`/`RouteStep` clip, so the *drift* (formation-level lateral bend) reads but the *reason for* the drift (flinching from enfilade) has no per-man tell. |
| 8 | Multi-man casualty burst from a single shell | "sometimes as many as 10 men being killed and wounded by the bursting of a single shell" — Peyton, mountain-battery enfilade, `csa-1c-pic-1-garnett.md` EC5.2 | Garnett's Brigade | **Not correlated.** `AngleActionContext.CompileEngagements` already computes discrete `StrikeEvent`s (canister/shell ground impacts) used for `Flinch`/`Brace` *reactions* — but `CasualtySchedule.Compile` draws fall times independently via smooth `InvCdf`, never grouping several victims to one `StrikeEvent`. The data plumbing to do this already exists; it's unused for fall-time correlation. |
| 9 | Casualty pacing invisible during the bombardment | "During the shelling, we lost about 20 killed and wounded" (~2%) — `csa-1c-pic-1-garnett.md` EC6; worse for Kemper (severity comparison, uncounted) | Garnett's, Kemper's, all first-line brigades | **Not rendered at all** (§1.1) — pre-slice, no schedule entries exist for this window. |
| 10 | Officer-density loss cluster at the climax ("every regimental commander killed or wounded") | Peyton, `csa-1c-pic-1-garnett.md` EC1; Armistead's two colonels killed at/near the wall, `csa-1c-pic-3-armistead.md` EC1 | Garnett's, Armistead's brigades | **Not representable** — same structural gap as #4 (no officer distinction in `SoldierState`; V&R §1 bars invented named-person detail without separate approval). |

**Top 5 attested-behavior gaps (ranked by evidence strength × current
absence), for the headline:**

1. **Bombardment casualties are entirely unrendered** (#9) — the best-cited,
   most quantified gap (~2% of Garnett alone, a named officer death,
   zero render presence).
2. **Hand-to-hand/melee vocabulary is absent** (#2) — Peyton's own words
   ("hand to hand, and of the most desperate character") describe the
   wall fight's climax and the resolver plays ranged-fire clips through it.
3. **No multi-casualty burst correlation to discrete strike events** (#8) —
   the plumbing (`StrikeEvent`) exists and is simply not wired to
   `CasualtySchedule`.
4. **Colors/color-bearer succession has no representation at all** (#3) —
   the single densest, most specific behavior record in the corpus
   ("every flag... excepting one was captured") with zero engine support.
5. **Halt-and-fire-at-the-obstacle is a documented but unmodeled unit
   behavior** (#1) — currently every crossing is deterministic
   climb-and-continue; Trimble's own wounding account describes a unit
   that didn't climb.

---

## 3. Observer placement — evidence + proposals

### 3.1 What the current default observer actually is

`app/Assets/StreamingAssets/SoldierView/viewpoints.json` has exactly
**one** committed product viewpoint: `garnett-road-to-angle`
("With Garnett's Line"), `unitId=csa-garnett`, `slotId=881`,
`t0=8160, t1=8820` (15:16:00–15:27:00), eye height 1.66 m, FOV 68°.

Two properties of this single observer, both **documented in code**,
directly explain the punchlist complaint:

1. **The observer slot is structurally exempt from ever becoming a
   casualty** (`SoldierView/ViewpointObservers.cs`, policy ED-22): "the
   observer slot of every committed viewpoint is EXEMPT from victim
   selection... the camera would end face-down in the wheat for the
   remainder of the window [otherwise]." This is correct
   engineering (a scrub-invariant camera needs a stable rig) but its
   experiential cost is that **the viewer riding this view is never
   personally at risk** — every fall the viewer sees happens to someone
   else, at a hash-random distance, never adjacent-and-scripted.
2. **The slot is explicitly rear-rank** (same file, inline comment):
   "slot 881 = rear rank of file 184 (documented deviation from §6.5's
   illustrative slotId 184: the front-rank slot shows an empty road in
   first person; from the rear rank the whole front rank reads 1.3 m
   ahead)." The chosen slot was picked to solve a *sightline* problem
   (an empty road ahead), but the fix is "put a wall of the observer's
   own comrades between the camera and the enemy/the wall/the falling
   men" for the *entire* 11-minute viewpoint. Structurally, the render's
   only committed camera looks at the backs of other men's heads more
   than it looks at the fight.

Two further structural facts (`docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md`
§3.3–3.4) bound what any proposal can do: the camera cannot turn or move
inside a pre-rendered viewpoint, and the plan itself already named two
**unshipped extension viewpoints** — `webb-wall` (defender's side,
t=8520–9000) and `cushing-canister` (gun-crew view, t=8400–8760) — as
post-required-viewpoint work, never begun (no reference to either exists
outside the plan document; `viewpoints.json` has no entry for them).

### 3.2 Proposals (concrete, 2–3 as requested)

**OP-1 — Front-rank slot variant of the existing Garnett viewpoint.**
Re-derive the protected slot for the *front* rank of the same file
(or a nearby file) instead of the rear rank, and re-solve the sightline
problem the original rear-rank choice was solving (the "empty road"
issue) by re-timing `t0` to start *after* the fence crossings, when the
formation has closed up and the road is no longer empty ahead. Trades
comrade-occlusion for direct exposure to the wall, the fallen, and the
smoke. Cheapest of the three (no new render infrastructure, same
11-minute render budget) but does not by itself fix gap #1 above (the
viewer is still exempt from risk).

**OP-2 — Ship `webb-wall` (the plan's own named extension).** A
defender's first-person view behind the stone wall, t=8520–9000 (~8 min,
~9,900 frames — roughly 70% of the required viewpoint's render cost per
§3.5's linear budget). This is the single highest-leverage placement
available: it sits at the point the compiled schedule shows the loss
concentration (§1.1's 15:23–15:25 spike is entirely inside this window)
and faces the *opposite* direction from the only existing viewpoint — an
approaching, thickening mass of the enemy closing to point-blank range,
which reads as building intensity even with zero camera movement,
independent of any casualty-vocabulary work in §2.

**OP-3 — Ship `cushing-canister` (the plan's other named extension).**
Gun-crew-level view at Cushing's battery, t=8400–8760 (~6 min). Puts the
viewer inside the mechanical rhythm of loading/firing double canister
into a closing mass (Webb's report: "Three of Cushing's guns were run
down to the fence, carrying with them their canister" — `csa-1c-pic-1-garnett.md`
cross-ref via `us-ii-2-2-webb.md` EC5.2) — a fundamentally different
intensity register than either infantry view (task-focused urgency
under direct fire rather than a marching approach), and a battery crew
is a small, legible cast (the crew slots are already distinguished from
loaders/idle in `ResolveArtillery`).

**Recommendation ordering:** OP-2 (`webb-wall`) is the single best
value-for-scope proposal — it directly answers "the soldier is not on
the frontline himself" by putting the *defending* frontline soldier's
face toward the charge at its most concentrated 2 minutes, using
viewpoint infrastructure that already exists and evidence (§1.1, §2.1
row 5/6) already assembled. OP-1 is the cheapest incremental fix and
could ship alongside or before OP-2. OP-3 is the most novel but least
evidenced against the specific "carnage isn't clear" complaint (a gun
crew's own losses are light — 9 of 97 for Cushing's battery across the
whole slice, per `angle-bundle-audit.md`) and would need to lean on
what it can see (the approaching mass) rather than what happens to the
crew itself.

---

## 4. Proposal slice set

Each slice: what changes, evidence backing it, estimated scope, and a
sober-representation check against `violence-and-representation.md`.
None of these are executed by this pass — they are for the owner to
rule on, per the punchlist's "worth taking as individual slices" framing.

| # | Slice | What changes | Evidence | Scope | V&R check |
| --- | --- | --- | --- | --- | --- |
| P1 | Bombardment casualty prelude | Extend the Angle bundle's slice window (or add a short pre-slice segment) so the ~1 hr cannonade's attested losses (Garnett ~20 k&w incl. Ellis; Kemper "suffered severely," uncounted) play as visible falls before the charge begins, rather than being pre-subtracted into the starting headcount | §1.1 (Peyton `csa-1c-pic-1-garnett.md` EC6; Wilcox `csa-1c-pic-2-kemper.md` EC5.1) | **M** — needs a new casualty-profile class for a stationary/prone unit under long-range fire (no locomotion, sparse hash-random falls in place), plus a slice-window extension or a new pre-charge segment; touches the compiler, not just the resolver | Compliant by construction if built on the existing `unknown`/`shell` cause classes (V&R §3) — Kemper's uncounted severity comparison must NOT be converted into an invented number (V&R §2, aggregate evidence only); the qualitative "worse than Garnett's ~20" relation can bias a hash *distribution*, not a total |
| P2 | Multi-casualty burst correlation | Wire `AngleActionContext`'s already-compiled `StrikeEvent`s into `CasualtySchedule.Compile` so a subset of a profile's victims can cluster to specific canister/shell impacts instead of every fall being drawn independently from a smooth curve | §1.1 (peak-rate analysis), §2.1 row 8 (Peyton: "10 men... by a single shell") | **M** — the strike data already exists (`CompileEngagements`); the casualty compiler needs a new victim-to-strike assignment pass that stays exact against `causeMix`/`intensityCurve` totals (the existing reconciliation tests must still pass) | Compliant — still aggregate/hash-driven, still no named individuals (V&R §1/§2); tightens *when* within the profile a death lands, doesn't add wound detail beyond the existing vocabulary (V&R §3) |
| P3 | Melee/close-combat clip pair | Author 1–2 new `ClipId`s (e.g. `Melee_Bayonet`, `Melee_ClubbedMusket`) and a resolver branch for `breach`-segment slots that have crossed AND are within close range of an enemy slot, replacing/supplementing the current `Aim`/`Fire`/`Reload` reuse at the wall | §2.1 row 2 (Peyton: "hand to hand, and of the most desperate character... climbing over the wall, and fighting the enemy in his own trenches") | **M–L** — new hand-keyed clips (the P6 spike's IK-solve method applies; `docs/reconstruction/p6-spike-verdict.md` judges pose-to-pose sequences tractable) plus new resolver logic to decide melee-vs-ranged per slot-pair proximity, which is a new spatial query the resolver doesn't currently do at per-slot grain | Compliant — no gore beyond the existing wound-category table (V&R §3/§6); melee is a *pose* addition, not a new wound class; must not read as "game grammar" combat (V&R §5) — no player agency, still deterministic and undirected by the viewer |
| P4 | Color-bearer succession | A distinguished "color party" sub-role (2–4 slots per infantry unit) with its own fall/pickup-and-carry-on event chain, driven by the attested per-regiment color-bearer-down counts | §2.1 row 3 (Shepard: "Every flag in the brigade excepting one was captured at or within the works," 3–4 bearers down per regiment) | **L** — needs a flag prop asset, a new persistent-object system (the flag must visibly pass between figures, distinct from the existing static `DroppedItem`/`BloodPool` decals), and per-regiment count-matching against the cited figures; the highest-novelty item in this set | Compliant if scoped carefully — V&R §1 bars attaching a historical *name* to a procedural figure; a color-bearer slot can be evidence-sourced at the **regiment** level (matching Shepard's counts) without ever claiming "this is the man who carried the 13th Alabama's flag." Needs explicit owner review before build (V&R "Review obligations": widening the wound/persistence vocabulary requires sign-off) |
| P5 | Mounted-officer fall (Garnett) | A distinct mounted rig + fall clip for Garnett's documented horseback death ~25 paces from the wall | §2.1 row 4 (Peyton: shot from his horse near the brigade center) | **L** — no horse asset exists at all (`FormationRoster.cs`: "no horses are modeled"); this is new character/rig work, not a resolver change, and is the most infrastructure-heavy item here | **Requires owner approval before any build step.** V&R §1 explicitly: "Named officers whose wounds are documented (Garnett...) are represented only at unit level in this phase; if identified figures are added later, their treatment requires specific sourcing and separate approval (locked decision: no invented named-person wound details)." This slice is the locked decision's exact trigger condition — flag, do not build, without an explicit ruling |
| P6 | Halt-and-fire-at-obstacle segment class | A new segment `action` (or a resolver-level branch on existing `cross_obstacle`) where a unit halts and fires at a traced obstacle line instead of climbing it on schedule | §2.1 row 1 (Trimble: "our men stopped and began firing, instead of mounting the fence") | **S–M** — Trimble's command (Lane's/Lowrance's brigades) is not in the current 13-unit Angle cast, so this slice either (a) stays a resolver *capability* exercised nowhere yet, or (b) requires adding those brigades to the bundle first (a scope decision for the owner, not this pass) | Compliant — behavioral pacing only, no casualty/wound implications; low representation risk |
| P7 | Front-rank observer variant (OP-1) | Re-slot the existing `garnett-road-to-angle` viewpoint to a front-rank position, re-timed to avoid the empty-road sightline problem the original rear-rank choice solved | §3.1–3.2 | **S** — no new render infrastructure, re-derive one `slotId` + `t0`, re-render the same ~11-minute budget | Compliant — same protected-observer policy (ED-22) applies unchanged |
| P8 | Ship `webb-wall` viewpoint (OP-2) | Author and render the plan's own named extension viewpoint, t=8520–9000, defender-side at the stone wall | §3.1–3.2; plan §3.4 | **L** — ~8 min at 30 fps ≈ 14,400 frames (~73% of the required viewpoint's 19,800 frames); at the plan §3.5 unit costs that is ~20 render-hours at 5 s/frame up to ~10 render-days at 60 s/frame; needs its own observer-slot protection entry, camera authoring, and visual-target gate pass, same as the original required viewpoint | Compliant — same doctrine as the shipped viewpoint; no new wound/persistence rules needed, though it will make the existing wall-fight spike (P2 if built) far more visible and should ship *after* P2 if both are approved, so the defender's view isn't the first place a smoothed casualty curve gets exposed at close range |
| P9 | Ship `cushing-canister` viewpoint (OP-3) | Author and render the plan's other named extension viewpoint, t=8400–8760, gun-crew view | §3.1–3.2; plan §3.4 | **L** — ~6 min ≈ 10,800 frames (~55% of the required viewpoint's 19,800 frames); ~15 render-hours at 5 s/frame up to ~7.5 render-days at 60 s/frame; same gate process as P8 | Compliant — artillery crew casualties are light in this window (9/97 for Cushing's battery across the whole slice) so this viewpoint leans on tempo/mechanism rather than personal risk; no representation concerns beyond the existing doctrine |

**Suggested sequencing if the owner wants a minimal first cut:** P7
(cheap, immediate) + P8 (highest leverage on the actual complaint) first;
P1/P2 (pacing-engine fixes) next since they improve *every* current and
future viewpoint at once; P3/P6 (behavior vocabulary) as a follow-on;
P4 pending explicit scoping; P5 held pending an explicit owner ruling
per V&R §1; P9 whenever render budget allows.

---

## 5. What rides with `iverson-viewpoint` vs. the Angle/charge-intensity track

`iverson-viewpoint` (branch exists, no unique commits yet as of this
pass's HEAD `9118eb8` — confirmed via `git log` on the worktree at
`.../scratchpad/iverson-wt`) is scoped to **July 1, Iverson's Brigade,
the Forney-field destruction** — a different day, a different rendering
subject (not currently in the HDRP Angle bundle at all), and per §1.2
above, a **structurally different pacing shape** (true single-spike,
not a ramp). Recommend:

- **Rides `iverson-viewpoint`:** any camera/observer work specific to
  Iverson's destruction line itself (a "the volley opens" viewpoint
  analogous to OP-2/OP-3 but for the Forney field); any casualty-schedule
  work needed to compile Iverson's brigade into a bundle at all (it is
  not currently a Soldier-View subject — this is new-scope work, not a
  fix to existing pacing); the single-spike shape (§1.2) as its own
  design reference, since Iverson's "line as straight as a dress parade"
  image is a *different* visual problem (uniform density loss, not an
  escalating charge) than anything in this document's proposal set.
- **Rides the Angle/charge-intensity track (this document's scope):**
  everything in §4 (P1–P9) — all of it is Angle-bundle-specific (the 13
  existing units, the existing viewpoint infrastructure, the existing
  resolver). P2's strike-correlation mechanism and P6's
  halt-and-fire-at-obstacle segment class are the two items here with
  the most direct reuse value for Iverson's brigade *if* it is later
  added to a bundle (Iverson's collapse is essentially a single massive
  correlated volley — P2's mechanism, generalized, is close to what a
  faithful Iverson render would need) — worth flagging to whoever scopes
  `iverson-viewpoint` next, but not itself a reason to block or merge
  either branch's work into the other.

---

## 6. Summary for the owner

**Headline pacing findings:**

- First 25% of the Angle render's runtime carries 4.0% of the visible
  CSA-infantry losses (105 of 2,593); the final 44% carries 65%; the
  2-minute wall-fight spike alone (12.5% of runtime) carries 27%. The
  aggregate is faithful to Peyton's "more than half" (compiled: 49.8%
  for Garnett) — the gap is in *shape*, not total count.
- The ~1-hour bombardment (Garnett ~20 k&w / ~2% of strength, including
  a named officer's death; Kemper worse but uncounted) is **entirely
  unrendered** — pre-subtracted into the starting headcount with zero
  visible cause.
- Peak instantaneous rate (Garnett, 15 s buckets): ~2.5 men/s at the
  wall-fight peak vs. ~0.1–0.25 men/s early — a real but *smoothly
  ramped* increase, not a burst tied to discrete shell/canister events
  even though the data plumbing for such bursts (`StrikeEvent`) already
  exists and is unused for this purpose.
- Three-way contrast: the Angle is a single escalating charge/repulse
  (~16 min, back-loaded); Iverson (July 1) is a true single-spike
  collapse (minutes, front-loaded-to-instant); the Wheatfield (July 2)
  is multi-wave attrition over ~2.5 hrs with no single continuous curve.

**Top 5 attested-behavior gaps:** (1) bombardment casualties unrendered,
(2) no melee/hand-to-hand vocabulary at the wall, (3) no multi-casualty
burst correlation to discrete strikes, (4) no color-bearer/colors-falling
representation, (5) no halt-and-fire-at-obstacle behavior class.

**Observer proposals:** OP-1 front-rank re-slot of the existing
viewpoint (cheap); OP-2 ship `webb-wall` (highest leverage, plan-named,
unshipped); OP-3 ship `cushing-canister` (novel register, plan-named,
unshipped).

**Proposal-slice list:** P1 bombardment prelude, P2 strike-correlated
casualty bursts, P3 melee clip pair, P4 color-bearer succession (needs
scoping), P5 mounted-officer fall (**needs explicit owner approval,
V&R §1 trigger — do not build without a ruling**), P6 halt-and-fire
segment class, P7 front-rank observer, P8 ship webb-wall, P9 ship
cushing-canister.

**Branch:** `charge-intensity-research`, pushed to
`origin/charge-intensity-research`, not merged.
