# Unit dossiers — index

Six-class evidence dossiers per `docs/reconstruction/audit/unit-truth-spec.md`.
One file per unit; headings are schema-fixed so a later pass can compile them
to claims. Times are Gettysburg local mean time (ED-31); every timed fact
carries its tier in square brackets:

- `[A vs CA-…]` — stated relative to a canonical anchor (±1–3 min)
- `[B: inputs]` — physics-derived, inputs shown (±2–5 min)
- `[C via <source> clockProfile ±n]` — witness clock corrected per ED-25
- `[D]` — sequence-only (before/after brackets)
- `[E]` — window-level presence only

Citations are `(source-id, locator)`; source ids resolve in
`reconstruction/sources/sources.json`. Facts with **no tier bracket are
untimed** (identity, strength, materiel). Negative evidence and conflicts are
first-class and live in their own section. New editorial calls are PROPOSED
as ED candidates (ED-53+ as of pass 5 — the pass-1 candidates ED-32…ED-38,
the monument profile ED-39, the pass-2 candidates ED-40…ED-45, the
pass-3 candidates ED-46…ED-49, and the pass-4 candidates ED-50…ED-52
were all adopted 2026-07-11), never self-adopted.

## Fixed heading schema

```
# Dossier: <unit name> (<register id>)
<status block: pass, cast status, achieved T-level, date>
## EC1 — Identity & command
## EC2 — Engaged strength
## EC3 — Position anchors
## EC4 — Movement legs
## EC5 — Activity record
## EC6 — Casualty apportionment
## Conflicts & negative evidence
## Chain anchors substantiated
## ED candidates proposed
## Source register (ids + access notes)
```

## Pass 1 — anchor-executors of the July 3 afternoon chain (ED-24)

Sequencing doctrine layer 1 (unit-truth-spec): the units that enacted the
adopted chain anchors. The Angle cast (Cushing, Webb's regiments, the CSA
brigades) is already T5 via the V2 claim corpus and is not re-dossier'd here.

| Dossier | Register id | Executes | Achieved T |
|---|---|---|---|
| [csa-1c-wsh-3-miller.md](csa-1c-wsh-3-miller.md) | reg-csa-1c-wsh-3 | CA-J3A-1 (fires the signal guns) | T3 |
| [csa-1c-bn-wash-eshleman.md](csa-1c-bn-wash-eshleman.md) | reg-csa-1c-bn-wash (+ reg-csa-1c-arty-cmd context) | CA-J3A-1 (order chain), bombardment line | T4 |
| [csa-1c-bn-alexander.md](csa-1c-bn-alexander.md) | reg-csa-1c-bn-alexander | CA-J3A-2/3 (the clock-bearer's battalion) | T4 |
| [us-arty-hq-hunt.md](us-arty-hq-hunt.md) | reg-us-arty-hq | CA-J3A-4/5 (conservation order) | T2 (command record) |
| [us-ar-1v-mcgilvery.md](us-ar-1v-mcgilvery.md) | reg-us-ar-1v | CA-J3A-4 (the slackening IS his line's silence), CA-J3A-10 | T4 |
| [us-xi-arty-osborn.md](us-xi-arty-osborn.md) | reg-us-xi-arty | CA-J3A-4/5 (the cease-fire ruse) | T4 |
| [us-ii-arty-hazard.md](us-ii-arty-hazard.md) | reg-us-ii-arty | CA-J3A-4 counterpoint (the line that did NOT slacken by order) | T4 |
| [us-btty-woodruff.md](us-btty-woodruff.md) | reg-us-ii-b4 | CA-J3A-4/5 counterpoint; Pettigrew-front repulse | T4 |
| [us-btty-arnold.md](us-btty-arnold.md) | reg-us-ii-b2 | CA-J3A-4/5 counterpoint; Fry/Marshall-front repulse | T4 |
| [us-btty-brown.md](us-btty-brown.md) | reg-us-ii-b3 | CA-J3A-4 (his mid-cannonade withdrawal is the II Corps line's one ordered slackening) | T4 |
| [us-btty-rorty.md](us-btty-rorty.md) | reg-us-ii-b1 | CA-J3A-4/5 counterpoint; Kemper-front repulse | T4 |
| [us-i-3-3-stannard.md](us-i-3-3-stannard.md) | reg-us-i-3-3 | CA-J3A-8 (the change of front) | T4 (T5 inside the Angle window) |

Companion documents: `docs/reconstruction/audit/dossier-pass-1.md` (pass
report), `docs/reconstruction/audit/dossier-overlay.json` (master-table
consultation layer, applied by `reconstruction/scripts/build_unit_audit.py`).

## Pass 2 — the reference-frame layer (ED-36 grain; spatial apparatus first use)

Sequencing doctrine layer 2: the units other units' position/timing
statements cite. First pass with the spatial program (EC3 anchors read off
the georeferenced Bachelder sheets per `docs/reconstruction/audit/
spatial-evidence.md`; monuments per ED-39). Ordnance-returns hunt opened:
nine rounds-expended figures found (pass 1's uniform gap closed for this
layer).

| Dossier | Register id | Frame role | Achieved T |
|---|---|---|---|
| [us-ii-3-2-smyth.md](us-ii-3-2-smyth.md) | reg-us-ii-3-2 | Hays-front wall line (Angle → Bryan barn) | T4 |
| [us-ii-3-3-willard.md](us-ii-3-3-willard.md) | reg-us-ii-3-3 | second line / interleaved wall right | T4 |
| [us-ii-2-3-hall.md](us-ii-2-3-hall.md) | reg-us-ii-2-3 | wall south of the Copse; the crisis rush | T4 |
| [us-ii-2-1-harrow.md](us-ii-2-1-harrow.md) | reg-us-ii-2-1 | Gibbon-front left; division frame July 3 | T4 |
| [us-btty-cowan.md](us-btty-cowan.md) | reg-us-vi-b2 | the Brown-slot crisis geometry | T4 (T5 window in-build) |
| [us-btty-weir.md](us-btty-weir.md) | reg-us-ar-1r-4 | crisis reinforcement; Stannard's July 2 object; 280-round return | T4 |
| [us-btty-fitzhugh.md](us-btty-fitzhugh.md) | reg-us-ar-4v-5 | crisis pair at Webb's fence; 89-round return | T4 |
| [us-btty-parsons.md](us-btty-parsons.md) | reg-us-ar-4v-3 | crisis pair, the clean 15:00 Hunt clock; ~240 rounds | T4 |
| [us-btty-wheeler.md](us-btty-wheeler.md) | reg-us-xi-b2 | Osborn-group crisis detachment; 850-round return | T4 |
| [us-btty-cooper.md](us-btty-cooper.md) | reg-us-i-b4 | the during-cannonade transfer; 1,050 rounds per-day | T4 |
| [us-ar-hq-tyler-park.md](us-ar-hq-tyler-park.md) | reg-us-ar-hq | the park + Gillett ammunition ledger (19,189/4,694) | T3 |
| [us-i-arty-wainwright.md](us-i-arty-wainwright.md) | reg-us-i-arty | East Cemetery Hill group, layout verbatim | T4 (group grain) |
| [us-btty-rittenhouse.md](us-btty-rittenhouse.md) | reg-us-v-b4 | Little Round Top ring-closer | T2 (MOLLUS access-failed) |
| [csa-1c-bn-dearing.md](csa-1c-bn-dearing.md) | reg-csa-1c-bn-dearing | fronting Pickett; the battalion-relations quote | T4 |
| [csa-1c-bn-cabell.md](csa-1c-bn-cabell.md) | reg-csa-1c-bn-cabell | Peach Orchard arc's 4th battalion; ~3,300 rounds | T4 |
| [csa-3c-bn-pegram.md](csa-3c-bn-pegram.md) | reg-csa-3c-bn-pegram | Third Corps arc; per-day casualty split; 3,800 rounds | T4 |
| [csa-3c-bn-mcintosh.md](csa-3c-bn-mcintosh.md) | reg-csa-3c-bn-mcintosh | Third Corps arc; 1,395 rounds itemized | T4 |
| [csa-3c-bn-garnett.md](csa-3c-bn-garnett.md) | reg-csa-3c-bn-garnett | THE attested silence ("did not fire a single shot") | T2 (negative-evidence) |
| [csa-2c-arty-ewell-wing.md](csa-2c-arty-ewell-wing.md) | reg-csa-2c-bn-dance/-nelson/-carter/-latimer/-jones | partial-participation record (corrects "near-silence") | T2 (group grain) |

Companion: `docs/reconstruction/audit/dossier-pass-2.md`.

## Pass 3 — the first command batch (the assault column, full arcs)

Sequencing doctrine layer 3: everything else in command batches, placed
against the frame. The assault column at brigade grain — the units every
pass-2 fire statement targets — researched across the full battle arc.
The Angle-cast T5 window (15:05–15:30) is EXTENDED, never re-litigated.
Batch adoption of ED-40…ED-45 executed at pass start. Headline evidence
class: the ANV casualty return (OR 27/2 pp. 338-346) consumed at regiment
grain, plus five primary engaged-strength statements.

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [csa-1c-pic-hq-pickett.md](csa-1c-pic-hq-pickett.md) | (no register row — flagged) | division command record; the suppressed report | T2 (command record) |
| [csa-1c-pic-1-garnett.md](csa-1c-pic-1-garnett.md) | reg-csa-1c-pic-1 | first line right-center; the Soldier View brigade | T4 full-arc (T5 window) |
| [csa-1c-pic-2-kemper.md](csa-1c-pic-2-kemper.md) | reg-csa-1c-pic-2 | first line right flank; Stannard/LRT object | T4 full-arc (T5 window) |
| [csa-1c-pic-3-armistead.md](csa-1c-pic-3-armistead.md) | reg-csa-1c-pic-3 | second line; the crossing (CA-J3A-9) | T4 full-arc (T5 window) |
| [csa-3c-het-hq-pettigrew.md](csa-3c-het-hq-pettigrew.md) | (no register row — flagged) | left-wing division command record | T3 (command record) |
| [csa-3c-het-3-fry.md](csa-3c-het-3-fry.md) | reg-csa-3c-het-3 | the column's guide; the color record | T4 full-arc |
| [csa-3c-het-1-marshall.md](csa-3c-het-1-marshall.md) | reg-csa-3c-het-1 | left-wing right-center; the 26th NC arc | T4 full-arc |
| [csa-3c-het-4-davis.md](csa-3c-het-4-davis.md) | reg-csa-3c-het-4 | left-wing left-center; Bryan-barn front | T4 full-arc |
| [csa-3c-het-2-brockenbrough.md](csa-3c-het-2-brockenbrough.md) | reg-csa-3c-het-2 | extreme left; the corrupted-strength unit, now bounded | T3 full-arc |
| [csa-3c-pen-hq-trimble.md](csa-3c-pen-hq-trimble.md) | (no register row — flagged) | third-line command record (Pender→Lane→Trimble→Lane) | T3 (command record) |
| [csa-3c-pen-2-lane.md](csa-3c-pen-2-lane.md) | reg-csa-3c-pen-2 | third line left; 1,355/660 primaries | T4 full-arc |
| [csa-3c-pen-4-lowrance.md](csa-3c-pen-4-lowrance.md) | reg-csa-3c-pen-4 | third line right; the ~500-man brigade | T4 full-arc |
| [csa-3c-and-1-wilcox.md](csa-3c-and-1-wilcox.md) | reg-csa-3c-and-1 | late echelon right (CA-J3A-10); 1,200/204 primaries | T4 full-arc |
| [csa-3c-and-4-lang.md](csa-3c-and-4-lang.md) | reg-csa-3c-and-4 | late echelon left; the 2d Fla capture pocket | T4 full-arc |

Companion: `docs/reconstruction/audit/dossier-pass-3.md`.

## Pass 4 — the Union center command batch + CSA frame completion

Sequencing doctrine layer 3, second command batch: the Union units that
RECEIVED the assault column (closing the loop pass 3 opened), the two
wounded command records whose pins bracket the climax, the deferred
Doubleday pieces, and the CSA arc's two completion items. Batch adoption
of ED-46…ED-49 executed at pass start. Headline evidence classes: the
Union Return of Casualties consumed at regiment grain (the ANV return's
counterpart, full k/w/m columns); Trimble's SHSP 26 account (standing
fetch CLOSED); the prisoner-records cross-check layer (Meade's 13,621 /
burial ledger / Hartwig's 12,227).

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [us-ii-2-2-webb.md](us-ii-2-2-webb.md) | reg-us-ii-2-2 | the Angle's receiving brigade, full arc | T4 full-arc (T5 window held) |
| [us-ii-2-hq-gibbon.md](us-ii-2-hq-gibbon.md) | (no register row — flagged) | division command record; the first wounding pin | T3 (command record) |
| [us-ii-hq-hancock.md](us-ii-hq-hancock.md) | (no register row — flagged) | corps command record; the wounding CONFLICT record | T3 (command record) |
| [us-ii-3-hq-hays.md](us-ii-3-hq-hays.md) | (no register row — flagged) | division command record; the four-lines repulse | T3 (command record) |
| [us-ii-3-1-carroll.md](us-ii-3-1-carroll.md) | reg-us-ii-3-1 | split-brigade arc (East Cemetery Hill night fight; the 8th Ohio flank action FIRST-CLASS) | T4 full-arc |
| [us-i-3-1-biddle.md](us-i-3-1-biddle.md) | reg-us-i-3-1 | Gates's demi-brigade at the Copse's south shoulder | T3 full-arc |
| [us-i-3-2-stone.md](us-i-3-2-stone.md) | reg-us-i-3-2 | second/third-line frame (honestly thin) | T3 full-arc |
| [us-i-3-hq-doubleday.md](us-i-3-hq-doubleday.md) | (no register row — flagged) | division command record; late-clock counter-profile | T2 (command record) |
| [csa-3c-bn-poague.md](csa-3c-bn-poague.md) | reg-csa-3c-bn-poague | ED-36 frame COMPLETED; the howitzer negative; 657-round return | T4 full-arc |
| [csa-2c-arty-hq-brown.md](csa-2c-arty-hq-brown.md) | (no register row — flagged ×2) | Second Corps artillery command record | T2 (command record) |

Pass-4 upgrades to existing dossiers: csa-3c-pen-hq-trimble.md (SHSP 26
closed — the road-fence wounding conflict), us-btty-rittenhouse.md
(Norton same-hill witness; MOLLUS still access-failed ×3),
csa-1c-pic-2-kemper.md + csa-1c-pic-3-armistead.md (morning-report sweep
closed verified-negative; B&M-type compilation readings carried),
csa-3c-pen-4-lowrance.md (Poague-frame note closed).

Companion: `docs/reconstruction/audit/dossier-pass-4.md`.
