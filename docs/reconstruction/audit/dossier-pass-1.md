# Dossier pass 1 — anchor-executors of the July 3 afternoon chain

**Date:** 2026-07-10 · **Branch:** audit-dossiers-1 · **Status:** complete
(this pass); web-verification layer integrated where the fetches landed,
gaps recorded honestly.

Layer 1 of the unit-truth-spec sequencing doctrine: full six-class dossiers
for the units that ENACTED the adopted ED-24 chain anchors, excluding the
already-T5 Angle cast. Dossiers: `reconstruction/dossiers/` (fixed heading
schema, tier-prefixed facts). Master-table consultation layer:
`docs/reconstruction/audit/dossier-overlay.json`, applied deterministically
by `reconstruction/scripts/build_unit_audit.py`.

## 1. Unit set and achieved T-levels

| Unit | Register id | Chain anchor(s) executed | Achieved T |
|---|---|---|---|
| Miller's 3rd Co., Washington Artillery | reg-csa-1c-wsh-3 | CA-J3A-1 (fired the signal guns) | T3 |
| Eshleman's Washington Artillery Bn | reg-csa-1c-bn-wash | CA-J3A-1 (parent/order chain) | T4 |
| First Corps artillery command (Walton/Alexander) | reg-csa-1c-arty-cmd (NEW row) | CA-J3A-1 order path context | (context row; carried in the two dossiers above) |
| Alexander's Battalion | reg-csa-1c-bn-alexander | CA-J3A-2/3 (the clock-bearer's battalion); CA-J3A-4 witness | T4 |
| Hunt (AoP artillery command) | reg-us-arty-hq (NEW row) | CA-J3A-4/5 (the conservation/cease orders) | T2 (command record) |
| McGilvery's line (1st Vol. Bde + composite) | reg-us-ar-1v | CA-J3A-4 (ordered silence); CA-J3A-10 (enfilade) | T4 |
| Osborn's XI Corps / Cemetery Hill group | reg-us-xi-arty | CA-J3A-4/5 (the cease-fire ruse); CA-J3A-10 | T4 |
| Hazard's II Corps Artillery Bde | reg-us-ii-arty | CA-J3A-4 counterpoint (did NOT slacken) | T4 |
| Woodruff's I/1st US | reg-us-ii-b4 | CA-J3A-4 counterpoint; CA-J3A-10 (Pettigrew left) | T4 |
| Arnold's A/1st RI | reg-us-ii-b2 | CA-J3A-4 counterpoint; CA-J3A-9/10 | T4 |
| Brown's B/1st RI | reg-us-ii-b3 | CA-J3A-4 (the one ordered II Corps withdrawal) | T4 |
| Rorty's B/1st NY | reg-us-ii-b1 | CA-J3A-4 counterpoint; CA-J3A-9/10 | T4 |
| Stannard's 2nd Vermont Bde | reg-us-i-3-3 | CA-J3A-8 (the change of front); CA-J3A-10 | T4 (T5-grade in the Angle window) |

Register changes: two command rows added (`reg-us-arty-hq` Hunt,
`reg-csa-1c-arty-cmd` Walton/Alexander) — the chain's two ordering minds
had no rows to carry their dossiers; stats updated (273 entries).

T-level honesty: "T4" here means the spec's roll-up (anchored endpoints,
cited activity summary, physics-timed legs where the unit moved, casualty
apportionment authored at episode grain) is met at the unit's dossier
grain, with the per-class gaps in §3 explicitly carried. No unit is
claim-compiled by this pass (that is T5 authoring work, not research).

## 2. The facts that most strengthen the chain anchors

(Filled in the final section of this pass — see §2 of the committed
version.)

## 3. EC-class coverage honesty table

(Filled in the final section of this pass.)

## 4. New ED candidates (proposed, NOT adopted)

- **ED-32** — regiment-grain strength readings for Stannard's brigade
  (480/647/661 alongside the 1,950 total; the ~160-man gap recorded).
- **ED-33** — adopted mixed reading of the Union slackening (ordered
  economy + Osborn's ruse + genuine damage/withdrawals), including an
  adopted identification of Alexander's "18 guns … have gone".
- **ED-34** — artillery activity classes for authoring: *ordered-silence*
  (McGilvery/Osborn) vs *fought-to-wreckage* (Hazard's line), with
  matching casualty-curve classes.
- **ED-35** — cast McGilvery's July 3 composite line as a command unit
  with the tablet-verified battery roster as children.
- **ED-36** — battalion-grain CSA July 3 geometry (Alexander/Eshleman/
  Dearing tablet lines) as the Confederate reference frame for pass 2.

## 5. Sources added and clockProfiles assessed

(Filled in the final section of this pass.)

## 6. Suites and mechanics

- reconstruction: 103 (102 passed + 1 skipped) · pipeline: 59 · tool: 108
  — all green on the branch.
- Master table regenerated via
  `uv run --with openpyxl python scripts/build_unit_audit.py`; the new
  dossier-overlay input keeps the consultation layer deterministic;
  315 rows (190 in-build + 125 register).
- Owner-readable copies: `docs/benchmarks/captures/audit-d1/`.

## 7. Recommended pass-2 unit set (the reference-frame layer)

The units other units' position/timing statements cite, per the
sequencing doctrine:

**Union gun line + II Corps front (the "on Woodruff's right / supporting
Cushing" frame):**
1. Cowan's 1st NY Independent (reg-us-vi? — II Corps front attachment;
   in-build `us-btty-cowan`) — the Brown-replacement geometry.
2. Fitzhugh's K/1st NY, Parsons's A/1st NJ, Weir's C/5th US, Wheeler's
   13th NY — the documented in-window reinforcing tracks.
3. Hays's 3rd Division II Corps brigades (Smyth, Sherrill) and Hall/
   Harrow of Gibbon's — the infantry frame the artillery statements
   reference (several already in-build; dossiers raise their audited
   tier).
4. Webb's brigade full-arc extension (T5 in-window already).
5. The Artillery Reserve HQ/park row (reg-us-ar-hq) — the ammunition
   physics behind Hunt's policy (EC5 upgrade path for every Union
   battery).
6. Wainwright's I Corps group + Rittenhouse (Little Round Top) — closes
   the Union gun ring; Rittenhouse's enfilade is cited by CSA accounts.

**CSA artillery battalions (the Confederate frame):**
7. Dearing's battalion (fronting Pickett — the assault corridor's own
   guns; tablet-grade "fired effectively by battery").
8. Cabell's battalion (Peach Orchard/Wheatfield Rd arc).
9. Pegram's + McIntosh's (Third Corps arc fronting Pettigrew).
10. The documented negatives: Garnett's idle battalion; the Ewell-wing
    near-silence (Dance/Nelson/Carter splits) — reference-frame negative
    evidence the bombardment picture depends on.

Rationale: every pass-1 dossier's open EC3 items (battery-point
coordinates on McGilvery's/Osborn's lines; CSA per-battery anchors) are
exactly the statements the reference-frame layer resolves.
