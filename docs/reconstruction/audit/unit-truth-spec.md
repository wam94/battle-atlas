# Unit truth specification — the tabletop audit's data standard

**Status:** Adopted by the owner 2026-07-10 ("this looks great").
Governs punchlist item 0 (the tabletop unit-activity audit) and all
subsequent macro-cast authoring. Extends — never replaces — the V2
reconstruction discipline (claims with time envelopes, editorial
decisions, no invention).

## The question

What information makes a single unit *true* — i.e., fully determines its
rendered position, facing, formation, strength, and activity as a
function of the battle clock, with honest provenance?

## The six evidence classes (per unit)

Every unit's dossier has one slot per class. A slot is **filled with
citations**, **filled with negative evidence** (attested stillness or
silence — first-class, cited), or **explicitly open**.

1. **Identity & command state.** Designation, parent chain, arm;
   commander plus mid-window casualties/successions (often the
   best-timed events a unit has). Artillery: gun types and counts
   (range envelopes let geometry infer engagement windows).
2. **Engaged strength.** Morning-of figure with source disagreements
   preserved (per the OOB research doctrine — never averaged), minus
   detachments (skirmish companies, train guards, ammunition details).
3. **Position anchors.** Attested positions, each with geometry
   (coordinates or terrain-feature reference), an uncertainty radius,
   and a time envelope (earliest/preferred/latest). Static unit: two
   anchors (window start/end) + negative evidence it stayed. Mover: an
   anchor chain.
4. **Movement legs.** Between consecutive anchors: route class (road /
   cross-country / obstacles), formation (column/line), pace class
   (drill-manual), and trigger (whose order, delivered how). Physics
   self-times a leg with anchored endpoints to ±2–3 min.
5. **Activity record.** Fire windows (target, munition — canister
   attests range and therefore position), under-fire windows,
   skirmishing, supporting, ammunition draws. Ordnance returns (rounds
   expended ÷ rate of fire) yield firing *durations*.
6. **Casualty apportionment.** Battle-total K/W/M (OR returns), then
   distribution across the window by episode (bombardment vs charge vs
   skirmishing), intensity shape, cause mix, and individually-timed
   officer casualties as hard pins.

**Cross-cutting (the "seventh"):** the conflict record — where sources
disagree, both readings are encoded and the adopted reading gets an
ED-numbered editorial decision, per the Phase 5 pattern.

## Timing tiers (per fact)

The master clock problem: witnesses' watches disagreed by 15–30 min.
Resolution: a **canonical anchor chain** — adopted times for events all
sources reference (cannonade opens / slackens, step-off, Armistead
falls; ED-1/ED-2 seed it) — plus a **per-source clock-offset
assessment** (how the source's stated clock relates to the chain),
recorded once on the source and inherited by its claims.

| Tier | Basis | Precision |
| --- | --- | --- |
| **A** | stated relative to a canonical anchor | ±1–3 min |
| **B** | physics-derived (leg distance ÷ pace; rounds ÷ rate) | ±2–5 min |
| **C** | witness clock, corrected via source clock-offset | ±10–15 min |
| **D** | sequence-only (before/after constraints) | bracketed |
| **E** | window-level presence only | the floor |

Every timed fact in the audit carries its tier. Facts should cite the
anchor they hang from (tier A) or the physics inputs (tier B).

## Truth levels (per unit, roll-up)

| Level | Meaning |
| --- | --- |
| **T0** | present-only: a position, no time depth |
| **T1** | anchored endpoints + attested-static / attested-moved classification |
| **T2** | activity summary written and cited (provenance-drawer ready) |
| **T3** | movement legs authored with physics-derived timing |
| **T4** | casualties apportioned across the window (shape + cause mix) |
| **T5** | full claim-compiled treatment (the Angle-cast standard) |

**Targets:** every unit ≥ T2 · every mover ≥ T3 · every engaged unit
≥ T4 · T5 reserved for Soldier View candidates.

## Where each layer lives

- Evidence classes + T-level: the master table
  (`docs/reconstruction/audit/unit-master-table.xlsx`), one row per
  unit; computed columns regenerate, consultation columns persist.
- Facts + tiers: claims files as the audit deepens
  (`reconstruction/claims/`, macro-grain), validated by the existing
  corpus tooling; before a unit graduates to claims, tiered facts may
  live in the consultation columns as `[A] …` / `[B] …` prefixed notes.
- Source clock-offsets: a new field on `reconstruction/sources/`
  records as sources are re-assessed (schema extension when the first
  batch lands).
- Rulings: `docs/reconstruction/angle-editorial-decisions.md`
  (ED-numbered, continuing the existing series).


## Scope ruling (owner, 2026-07-10)

**Research window: the full battle, upfront.** Dossiers cover each
unit's complete arc (arrival on the field → departure), captured in one
pass through its sources — windowed research would be redundant, and
all three days are the destination. **Authoring stays phased**: July 3
afternoon first (the current tabletop), then expansion, each wave
consuming already-complete dossiers.

Consequences: the master table expands to the full three-day order of
battle of both armies with a cast-status column (in-build /
not-yet-cast); the canonical anchor chain is structured per battle
phase (July 1 morning + afternoon; July 2 afternoon en echelon +
evening; July 3 morning + afternoon), July 3 afternoon drafted first.
