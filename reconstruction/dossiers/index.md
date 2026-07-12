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
as ED candidates (ED-75+ as of pass 14 — the pass-1 candidates ED-32…ED-38,
the monument profile ED-39, the pass-2 candidates ED-40…ED-45, the
pass-3 candidates ED-46…ED-49, the pass-4 candidates ED-50…ED-52, the
pass-5 candidates ED-53…ED-55 (incl. the CA-J2A-2/3 chain revision), and
the pass-6 candidates ED-56…ED-59 (the guns convergent-capture, the
occupation-wave record, the field-wide early-skew class, the
withdrawal-conflict ruling) were all adopted 2026-07-11, and the
pass-7 candidates ED-60/ED-61 (the Wright high-water ruling; the
Trostle gun-recovery cross-credit rule, its Bigelow/Watson
precondition satisfied first) were adopted 2026-07-11 at pass-8
start, and the pass-8 candidates ED-62/ED-63 (the CA-J2E
chain-hardening basis upgrade; the ECH battery-possession convergent
ruling) were adopted 2026-07-11 at pass-9 start, and the pass-9
candidates ED-64/ED-65 (the CA-J3M-3 two-pole conflict-record upgrade
— record adopted, ~10:00 direction provisional behind the standing
Pfanz gate; the CA-J3M-1 opening-cluster annotation) were adopted
2026-07-11 at pass-10 start, and the pass-10 candidates ED-66…ED-68 +
the ED-65 addendum (the CA-J1M-4 deploy-vs-attack split with the
CA-J1M-5 three-primary rider; the CA-J1P-2/3 basis upgrades; the
Howard ~11:30 sub-anchor + the CA-J1P-7 front-edge tightening; the
Geary/Muhlenberg record-only conflict) were adopted 2026-07-12 at
pass-11 start, and the pass-11 candidates ED-69…ED-71 (the CA-J1P-1
early-component annotation; the CA-J1P-5 cross-line basis upgrade +
Robinson rear-guard tail; the failed-row-arithmetic rule) were
adopted 2026-07-12 at pass-12 start, and the pass-12 candidates ED-72/
ED-73 (the East/South Cavalry Field chain; the CSA cavalry-brigade
position-marker evidence class) were adopted 2026-07-13 at pass-13
start, and the pass-13 candidate ED-74 (the report-vs-corroborated-
tablet-conflict class, extending ED-71) was adopted 2026-07-14 at
pass-14 start), never self-adopted.

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
| [csa-1c-pic-hq-pickett.md](csa-1c-pic-hq-pickett.md) | reg-csa-1c-pic-hq (added pass 5) | division command record; the suppressed report | T2 (command record) |
| [csa-1c-pic-1-garnett.md](csa-1c-pic-1-garnett.md) | reg-csa-1c-pic-1 | first line right-center; the Soldier View brigade | T4 full-arc (T5 window) |
| [csa-1c-pic-2-kemper.md](csa-1c-pic-2-kemper.md) | reg-csa-1c-pic-2 | first line right flank; Stannard/LRT object | T4 full-arc (T5 window) |
| [csa-1c-pic-3-armistead.md](csa-1c-pic-3-armistead.md) | reg-csa-1c-pic-3 | second line; the crossing (CA-J3A-9) | T4 full-arc (T5 window) |
| [csa-3c-het-hq-pettigrew.md](csa-3c-het-hq-pettigrew.md) | reg-csa-3c-het-hq (added pass 5) | left-wing division command record | T3 (command record) |
| [csa-3c-het-3-fry.md](csa-3c-het-3-fry.md) | reg-csa-3c-het-3 | the column's guide; the color record | T4 full-arc |
| [csa-3c-het-1-marshall.md](csa-3c-het-1-marshall.md) | reg-csa-3c-het-1 | left-wing right-center; the 26th NC arc | T4 full-arc |
| [csa-3c-het-4-davis.md](csa-3c-het-4-davis.md) | reg-csa-3c-het-4 | left-wing left-center; Bryan-barn front | T4 full-arc |
| [csa-3c-het-2-brockenbrough.md](csa-3c-het-2-brockenbrough.md) | reg-csa-3c-het-2 | extreme left; the corrupted-strength unit, now bounded | T3 full-arc |
| [csa-3c-pen-hq-trimble.md](csa-3c-pen-hq-trimble.md) | reg-csa-3c-pen-hq (added pass 5) | third-line command record (Pender→Lane→Trimble→Lane) | T3 (command record) |
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
| [us-ii-2-hq-gibbon.md](us-ii-2-hq-gibbon.md) | reg-us-ii-2-hq (added pass 5) | division command record; the first wounding pin | T3 (command record) |
| [us-ii-hq-hancock.md](us-ii-hq-hancock.md) | reg-us-ii-hq (added pass 5) | corps command record; the wounding CONFLICT record | T3 (command record) |
| [us-ii-3-hq-hays.md](us-ii-3-hq-hays.md) | reg-us-ii-3-hq (added pass 5) | division command record; the four-lines repulse | T3 (command record) |
| [us-ii-3-1-carroll.md](us-ii-3-1-carroll.md) | reg-us-ii-3-1 | split-brigade arc (East Cemetery Hill night fight; the 8th Ohio flank action FIRST-CLASS) | T4 full-arc |
| [us-i-3-1-biddle.md](us-i-3-1-biddle.md) | reg-us-i-3-1 | Gates's demi-brigade at the Copse's south shoulder | T3 full-arc |
| [us-i-3-2-stone.md](us-i-3-2-stone.md) | reg-us-i-3-2 | second/third-line frame (honestly thin) | T3 full-arc |
| [us-i-3-hq-doubleday.md](us-i-3-hq-doubleday.md) | reg-us-i-3-hq (added pass 5) | division command record; late-clock counter-profile | T2 (command record) |
| [csa-3c-bn-poague.md](csa-3c-bn-poague.md) | reg-csa-3c-bn-poague | ED-36 frame COMPLETED; the howitzer negative; 657-round return | T4 full-arc |
| [csa-2c-arty-hq-brown.md](csa-2c-arty-hq-brown.md) | reg-csa-2c-arty-cmd (added pass 5) | Second Corps artillery command record | T2 (command record) |

Pass-4 upgrades to existing dossiers: csa-3c-pen-hq-trimble.md (SHSP 26
closed — the road-fence wounding conflict), us-btty-rittenhouse.md
(Norton same-hill witness; MOLLUS still access-failed ×3),
csa-1c-pic-2-kemper.md + csa-1c-pic-3-armistead.md (morning-report sweep
closed verified-negative; B&M-type compilation readings carried),
csa-3c-pen-4-lowrance.md (Poague-frame note closed).

Companion: `docs/reconstruction/audit/dossier-pass-4.md`.

## Pass 5 — the July 2 southern-field batch (CA-J2P/ED-28 chain hardening)

The audit's first move into a new battle phase: the provisional July 2
afternoon chain's executors, both sides of each anchor, per pass 4 §8.
Batch adoption of ED-50…ED-52 executed at pass start; the EIGHT flagged
command register rows added (register-maintenance batch, 273 → 281
entries). Headline evidence classes: the executor step-off clocks
(Law's 16:00 signal-dispatch document pin; Alexander's four-attack
ladder; Kershaw's report clock sourced as the McLaws-wing skew's
ORIGIN), the LRT defense's compiled primaries (Norton's Warren
letters + Rice/Chamberlain/Garrard OR reports), and the return blocks
for both wings (pooled-missing pattern; the Barksdale tablet=return
exact match).

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [csa-1c-hood-1-law.md](csa-1c-hood-1-law.md) | reg-csa-1c-hood-1 | en-echelon lead; LRT attack right wing; the water-detail ledger | T4 full-arc |
| [csa-1c-hood-2-robertson.md](csa-1c-hood-2-robertson.md) | reg-csa-1c-hood-2 | Devil's Den executor; the pike-abandonment decision | T4 full-arc |
| [us-v-1-3-vincent.md](us-v-1-3-vincent.md) | reg-us-v-1-3 | the LRT defense; 1,000-muskets primary; 20:00 repulse clock | T4 full-arc |
| [us-v-2-3-weed.md](us-v-2-3-weed.md) | reg-us-v-2-3 | the second wave; Weed/O'Rorke/Hazlett paired pins | T4 full-arc |
| [csa-1c-mcl-1-kershaw.md](csa-1c-mcl-1-kershaw.md) | reg-csa-1c-mcl-1 | CA-J2A-7 executor; the skew exemplar; the three-gun signal | T4 full-arc |
| [csa-1c-mcl-3-barksdale.md](csa-1c-mcl-3-barksdale.md) | reg-csa-1c-mcl-3 | CA-J2A-9 executor; the no-report brigade; tablet=return 747 | T4 full-arc |

Pass-5 upgrades to existing dossiers: us-ii-2-1-harrow.md +
us-ii-2-3-hall.md (July 2 arcs closed both-sided — **T4 full-arc**),
us-btty-rittenhouse.md (**T3**; MOLLUS round 4 TERMINAL — library/CDL;
return row 13 landed; summit anchor refined), us-i-3-2-stone.md
(**T4 full-arc**; the 150th Pa history standing fetch LANDED).
Register maintenance: the eight pass-3/4 command rows added
(reg-csa-1c-pic-hq, reg-csa-3c-het-hq, reg-csa-3c-pen-hq,
reg-csa-2c-arty-cmd, reg-us-ii-hq, reg-us-ii-2-hq, reg-us-ii-3-hq,
reg-us-i-3-hq) with overlay entries.

Companion: `docs/reconstruction/audit/dossier-pass-5.md`.

## Pass 6 — the Wheatfield middle + the CSA en-echelon ladder's completion

The July 2 southern field's executor mass finished per pass 5 §9:
the Wheatfield's multi-occupation sequence treated as THE test of the
conflict-record discipline (five occupation waves, each both-sided —
pass-6 report §3), the Smith's-guns cross-credit closed both-sided
(ED-56 candidate), and the CA-J2A-10 legs. Batch adoption of
ED-53…ED-55 executed at pass start — **ED-53 is the audit's first
CHAIN REVISION** (CA-J2A-3 → ~16:30 [16:15–17:00]; CA-J2A-2 two-pole;
downstream anchors confirmed; anchor-chain table revised in place).
Headline evidence classes: three report=return exact agreements
(Cross 330, de Trobriand 490, and the tablet=return pair Semmes 430 /
Anderson 671), the two-sided 90-minute duration agreement on Houck's
Ridge, and the field-wide report-clock early skew (ED-58 candidate).
Monument register extended +40 southern-field entries (91 total).

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [us-ii-1-1-cross.md](us-ii-1-1-cross.md) | reg-us-ii-1-1 | wave-3 lead; Cross mw; 330=330 exact | T4 full-arc |
| [us-ii-1-2-kelly.md](us-ii-1-2-kelly.md) | reg-us-ii-1-2 | stony-hill prong; 530-primary; named-opponent tablet | T4 full-arc |
| [us-ii-1-3-zook.md](us-ii-1-3-zook.md) | reg-us-ii-1-3 | right prong; Zook mw; the 140th-PA shape | T4 full-arc |
| [us-ii-1-4-brooke.md](us-ii-1-4-brooke.md) | reg-us-ii-1-4 | the farthest advance (Rose ravine); wave-4 victim | T4 full-arc |
| [us-iii-1-3-detrobriand.md](us-iii-1-3-detrobriand.md) | reg-us-iii-1-3 | wave-1 garrison; 490=490 exact; the ED-59 accusation | T4 full-arc |
| [us-iii-1-2-ward.md](us-iii-1-2-ward.md) | reg-us-iii-1-2 | Houck's Ridge defense; two-sided 90-min agreement | T4 full-arc |
| [us-btty-smith.md](us-btty-smith.md) | reg-us-iii-b3 | the guns cross-credit closed; 140-round return | T4 |
| [us-v-1-1-tilton.md](us-v-1-1-tilton.md) | reg-us-v-1-1 | stony-hill wave 2; the ED-59 subject | T4 full-arc |
| [us-v-1-2-sweitzer.md](us-v-1-2-sweitzer.md) | reg-us-v-1-2 | twice-committed; the fence crisis (Jeffords pin) | T4 full-arc |
| [csa-1c-mcl-2-semmes.md](csa-1c-mcl-2-semmes.md) | reg-csa-1c-mcl-2 | ladder step 3 second echelon; Semmes mw; 430=430 | T4 full-arc |
| [csa-1c-mcl-4-wofford.md](csa-1c-mcl-4-wofford.md) | reg-csa-1c-mcl-4 | the wave-4 road sweep; sunset-pinned recall | T3 full-arc |
| [csa-1c-hood-3-anderson.md](csa-1c-hood-3-anderson.md) | reg-csa-1c-hood-3 | wave-1 opener; three-advance structure; 671=671 | T4 full-arc |
| [csa-1c-hood-4-benning.md](csa-1c-hood-4-benning.md) | reg-csa-1c-hood-4 | Devil's Den taker-and-holder; the third gun claimant | T4 full-arc |

Pass-6 upgrades to existing dossiers: csa-3c-and-1-wilcox.md
(**July 2 arc extended backward** — the CA-J2A-10 executor leg, the
thirty-minute stand, the 1st-MN receiving side), us-ii-2-1-harrow.md
(**the 1st Minnesota moment both-sided at regiment grain** — Coates
primary + Wilcox p. 618 + the j2-04 drawn pair).

Companion: `docs/reconstruction/audit/dossier-pass-6.md`.

## Pass 7 — the Peach Orchard / Plum Run periphery (the July 2 afternoon phase's completion)

The phase's remaining executor mass per pass 6 §9 and the sequencing
doctrine (finish the current phase's chain executors before opening a
new phase's): the III Corps salient at brigade grain (Graham's apex +
Humphreys's division — the CA-J2A-9/10 legs), the Plum Run
stabilization that ends wave 5 (McCandless/Nevin + Ayres's regulars,
the W4 extraction), and the Anderson-division salient-north pair
(Wright's famous high-water CONFLICT record treated as such; Lang's
July 2 backward-extension). Batch adoption of ED-56…ED-59 executed at
pass start (no anchor moves; CA-J2A-8 CONFIRMED, CA-J2A-11 exercised).
Headline evidence classes: FIVE tablet=return exact agreements
(Graham 740, Carr 790, Day 382, Burbank 447, McCandless 155) plus two
TRIPLES (report=tablet=return: Brewster 778, Burling 513); the 8th
Florida colors both-sided at flag grain; the division-grain AGREEING
clock counter-class (Carr 4.08 p.m.); Wright's three-reading EC6 with
missing=333 agreed. Monument register +20 (111).

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [us-iii-1-1-graham.md](us-iii-1-1-graham.md) | reg-us-iii-1-1 (us-madill) | the salient apex; Graham w&c pin; the Union no-report-class member | T4 full-arc |
| [us-iii-2-hq-humphreys.md](us-iii-2-hq-humphreys.md) | reg-us-iii-2-hq (added pass 7) | the fighting-withdrawal physics spine; Sickles pin | T3 (command record) |
| [us-iii-2-1-carr.md](us-iii-2-1-carr.md) | reg-us-iii-2-1 (us-carr) | the road front line; the 4.08 p.m. agreeing clock | T4 full-arc |
| [us-iii-2-2-brewster.md](us-iii-2-2-brewster.md) | reg-us-iii-2-2 (us-brewster) | second line; 778 triple; the 8th Fla colors both-sided | T4 full-arc |
| [us-iii-2-3-burling.md](us-iii-2-3-burling.md) | reg-us-iii-2-3 (us-burling) | the dispersed-class detachment ledger; 513 triple | T4 full-arc |
| [us-v-2-1-day.md](us-v-2-1-day.md) | reg-us-v-2-1 (us-day) | W3-W4 regulars second line; forwarding-cover negative | T4 full-arc (thin) |
| [us-v-2-2-burbank.md](us-v-2-2-burbank.md) | reg-us-v-2-2 (us-burbank) | the W4 extraction's biggest victim; <900-muskets primary | T4 full-arc |
| [us-v-3-1-mccandless.md](us-v-3-1-mccandless.md) | reg-us-v-3-1 (us-mccandless) | W5 left pole; 700-yards geometry; Taylor pin | T4 full-arc |
| [us-vi-3-3-nevin.md](us-vi-3-3-nevin.md) | reg-us-vi-3-3 (us-nevin) | W5 right pole; Wofford named-opponent; ED-61 proposer | T4 full-arc |
| [csa-3c-and-3-wright.md](csa-3c-and-3-wright.md) | reg-csa-3c-and-3 (csa-wright) | CA-J2A-10's named executor; THE high-water conflict record; ED-60 proposer | T4 full-arc |

Pass-7 upgrades to existing dossiers: csa-3c-and-4-lang.md (**July 2
arc extended backward** — the advanced Emmitsburg-Rd tablet, the 8th
Fla colors both-sided, the opposite-direction clock heterogeneity).

Companion: `docs/reconstruction/audit/dossier-pass-7.md`.

## Pass 8 — the July 2 evening chain (ED-29 opened) + the ED-61-gate closers

The evening phase opened per pass 7 §9 and the sequencing doctrine:
Culp's Hill both-sided (Greene's spine vs Johnson's division at
brigade grain, the XII Corps absent-then-returning frame as command
records), East Cemetery Hill both-sided (Hays/Avery vs Harris + the
Wiedrich/Ricketts hand-to-hand), and the pass-7 cheap closers
(Bigelow/Watson — ED-61's gate, satisfied BEFORE the batch adoption of
ED-60/61 executed at pass start — plus Winslow and the two return
rows). Headline evidence classes: the CA-J2E-2 cross-line minute-class
agreement (Greene "a few minutes before 7 p. m." vs Nicholls "7
p. m."); the CA-J2E-3 three-source both-armies 8-p.m. agreement
(Hays/Wiedrich/Ricketts); Johnson's divisional casualty table = the
ANV return EXACT at all four brigades (682/388/330/421); Hays's
per-day loss table (the evening's only per-day brigade split); the
twilight/darkness physics class (sunset 19:29 LMT, ED-31). Monument
register +32 (143); register +3 command rows (285) + the Jones
wounding-date correction.

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [us-xii-2-3-greene.md](us-xii-2-3-greene.md) | reg-us-xii-2-3 (us-greene) | the chain's Union spine; 1,350 strength table; four-clock ladder | T4 full-arc |
| [us-xii-hq-slocum-williams.md](us-xii-hq-slocum-williams.md) | reg-us-xii-hq (added pass 8) | CA-J2E-1's issuing echelon; the ED-61 Lockwood claim | T3 (command record) |
| [us-xii-2-hq-geary.md](us-xii-2-hq-geary.md) | reg-us-xii-2-hq (added pass 8) | division over Greene; the wrong-road negative; Kane's two-volley return | T3 (command record) |
| [csa-2c-joh-4-jones.md](csa-2c-joh-4-jones.md) | reg-csa-2c-joh-4 (csa-dungan) | the lead assault lane; Jones w July 2 (register corrected) | T4 full-arc |
| [csa-2c-joh-3-nicholls.md](csa-2c-joh-3-nicholls.md) | reg-csa-2c-joh-3 (csa-williams) | center lane; the 19:00 agreeing CSA clock | T4 full-arc |
| [csa-2c-joh-1-steuart.md](csa-2c-joh-1-steuart.md) | reg-csa-2c-joh-1 (csa-steuart) | left wing; the lodgment both-sided w/ the 137th NY; Betterton colors | T4 full-arc |
| [csa-2c-joh-2-walker.md](csa-2c-joh-2-walker.md) | reg-csa-2c-joh-2 (csa-walker) | the discretionary evening negative (Brinkerhoff's Ridge) | T3 full-arc |
| [csa-2c-ear-1-hays.md](csa-2c-ear-1-hays.md) | reg-csa-2c-ear-1 (csa-hays) | CA-J2E-3 executor/clock-bearer; per-day EC6 primary | T4 full-arc |
| [csa-2c-ear-3-avery.md](csa-2c-ear-3-avery.md) | reg-csa-2c-ear-3 (csa-godwin) | the left lane; Avery mw pin + the archival note | T4 (no-report class) |
| [us-xi-1-2-harris.md](us-xi-1-2-harris.md) | reg-us-xi-1-2 (us-harris) | the breached base wall; the gap confession | T4 full-arc |
| [us-btty-wiedrich.md](us-btty-wiedrich.md) | reg-us-xi-b1 (us-btty-wiedrich) | the intrenchments hand-to-hand; 21-m drawn-bar check | T4 full-arc |
| [us-btty-ricketts.md](us-btty-ricketts.md) | reg-us-ar-3v-3 (us-btty-ricketts) | the spiked piece; 23 report=return exact; CA-J2E-4 pin | T4 full-arc |
| [us-btty-bigelow.md](us-btty-bigelow.md) | reg-us-ar-1v-2 | the Trostle stand; ED-61 object 1; 28 arithmetic-exact | T4 |
| [us-btty-watson.md](us-btty-watson.md) | reg-us-v-b5 | ED-61 object 2; the Peeples recapture primary | T3 (brigade-carried) |
| [us-btty-winslow.md](us-btty-winslow.md) | reg-us-iii-b2 | the Wheatfield battery; blind-fire method primary | T4 |

Pass-8 upgrades to existing dossiers: us-vi-3-3-nevin.md (**EC6
CLOSED — tablet=return 53 exact**, the pass-7 flag), us-iii-1-2-ward.md
(the 4th ME 144 row clean re-read), us-ii-3-1-carroll.md (ECH 27-m
label repeatability + the HQ-marker-to-Ricketts 34-m pin).

Companion: `docs/reconstruction/audit/dossier-pass-8.md`.

## Pass 9 — the July 3 morning Culp's Hill resumption (ED-30's chain) + the Spangler's Meadow test

The morning phase opened per pass 8 §9: six executors inherited from
the pass-8 full-arc dossiers (Greene, Steuart, Walker, Nicholls,
Jones, the XII Corps command records — all upgraded in place), the
NEW executor set dossier'd (the returning XII Corps brigades at
brigade grain, the artillery ladder's author at group grain, the CSA
morning reinforcements), and the commissioned CHEAP-FETCH TEST of the
Spangler's Meadow dispute run FIRST. Batch adoption of **ED-62/ED-63**
executed at pass start (no preconditions; no anchor moves). Headline
evidence classes: the CA-J3M-1 ladder landed on its AUTHOR primary
(Muhlenberg's 4.30 / 15-min / 5.30 program, Williams verbatim-class
concurring); the CA-J3M-2 wave structure clocked by its executors
(O'Neal's 8 a.m. wave-2 primary; the Daniel/Steuart co-charge
both-sided; Kane's 10.30 column-of-regiments receiving clock); and
**the Spangler's Meadow dispute upgraded from
secondary-vs-secondary to a fully-characterized primary TWO-POLE
conflict** (Morse's 5.30 + Fesler's sequence + the 27th IN advance
marker's inscribed "6 a.m." vs Ruger's "about 10 a. m." order clock
+ Bachelder's drawn adjudication — the charge appears on the
8-11 a.m. sheet, not the 4-8) → **ED-64 candidate, NOT adopted; the
ED-30/Pfanz precondition STANDS**. Monument register +32 (175).

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [us-xii-1-3-colgrove.md](us-xii-1-3-colgrove.md) | reg-us-xii-1-3 (us-colgrove) | THE CA-J3M-3 executor; the order-corruption pair; Mudge pin | T4 full-arc |
| [us-xii-2-2-kane.md](us-xii-2-2-kane.md) | reg-us-xii-2-2 (us-kane) | the 652-strength primary; 3.30/seven-hours/10.30 receiving ladder | T4 full-arc |
| [us-xii-1-1-mcdougall.md](us-xii-1-1-mcdougall.md) | reg-us-xii-1-1 (us-mcdougall) | the stopped night approach; the 123rd NY reoccupation; friendly-fire record | T3 full-arc |
| [us-xii-1-2-lockwood.md](us-xii-1-2-lockwood.md) | reg-us-xii-1-2 (us-lockwood) | the unattached brigade; the ~6 a.m. woods deployment; ED-61 grain carried | T3 full-arc |
| [us-xii-2-1-candy.md](us-xii-2-1-candy.md) | reg-us-xii-2-1 (us-candy) | the wrong-road confession; the rotation pool; the 66th OH enfilade | T3 full-arc |
| [us-xii-arty-muhlenberg.md](us-xii-arty-muhlenberg.md) | reg-us-xii-arty | **CA-J3M-1's author unit** (the tablet ladder's primary text) | T3 (group grain) |
| [csa-2c-rod-1-daniel.md](csa-2c-rod-1-daniel.md) | reg-csa-2c-rod-1 (csa-daniel) | the 1.30-4.00 a.m. reinforcement march; the co-charge; the assessment primary | T4 full-arc |
| [csa-2c-rod-5-oneal.md](csa-2c-rod-5-oneal.md) | reg-csa-2c-rod-5 (csa-oneal) | **the wave-2 executor clock (8 a.m.)**; the ~510 m drawn advance | T4 full-arc |
| [csa-2c-ear-2-smith.md](csa-2c-ear-2-smith.md) | reg-csa-2c-ear-2 (csa-smith) | the third reinforcement at command-note grain; Johnson's-silence record | T2 (command-note grain) |

Pass-9 upgrades to existing dossiers: csa-2c-joh-1-steuart.md (**the
10 a.m. charge both-sided at three grains** — Daniel co-charge, Kane
10.30 receiving clock, Colgrove prisoner identification),
csa-2c-joh-4-jones.md (the Daniel-supports-Dungan weld),
csa-2c-joh-2-walker.md (the wave-1 composition pin),
csa-2c-joh-3-nicholls.md (morning frame note), us-xii-2-3-greene.md
(the hardened seven-hour frame), us-xii-hq-slocum-williams.md (the
program's issuer-executor verbatim pair closed; Slocum Powers Hill
pin), us-xii-2-hq-geary.md (the night-return clock pair at
brigade/regiment grain).

Companion: `docs/reconstruction/audit/dossier-pass-9.md`.

## Pass 10 — July 1 (the last uncovered major phase; ED-26/ED-27 executor-hardened)

Both July 1 chains taken off their R1 event-quote basis per pass 9
§9: the CA-J1M morning executors both-sided (Buford's brigades vs
Heth's four — the deploy-vs-attack split now executor-clocked:
Davis's **"About 10.30 o'clock a line of battle was formed... and
the brigade moved forward to the attack"** vs the Union receiving
cluster at 10:00-10:15 → **ED-66 candidate**), the Reynolds 10:15
death pin raised to THREE-primary agreement (tablet = Wadsworth =
Doubleday), and the CA-J1P afternoon pairs (Iverson's destruction —
the EC6 single-spike exemplar; Gordon's "About 3 p.m." order clock
pairing Schurz's "about 3 o'clock" receiving clock ACROSS THE LINES
→ **ED-67 candidate**; the Howard-assumes-command sub-anchor
two-primary at 11:30 and the Hancock-arrival controversy FORMALIZED
→ **ED-68 candidate**). Batch adoption of ED-64/ED-65 executed at
pass start (record-only; the CA-J3M-3 Pfanz gate STANDS). New
channel: the OR 27/1-27/2 IA full-text layer (page-verified against
civilwar.com where both serve) — it OVERTURNED the pass-9 Hoffman
negative (Smith's-brigade report No. 476 located). Spatial: first
j1-sheet exercise (4 masters, 7 crops, 23 reads; Ames crest bar
57 m from the Barlow statue); monument register +27 (202); register
+2 command rows (287); sources 186 → 199.

| Dossier | Register id | Arc role | Achieved T |
|---|---|---|---|
| [us-cav-1-1-gamble.md](us-cav-1-1-gamble.md) | reg-us-cav-1-1 | THE CA-J1M-2 executor; the delay line geometry primary; 1,600 + Calef | T4 (July 1 grain) |
| [us-cav-1-2-devin.md](us-cav-1-2-devin.md) | reg-us-cav-1-2 | the four-road screen; the friendly-fire record | T3 (July 1 grain) |
| [us-i-1-1-meredith.md](us-i-1-1-meredith.md) | reg-us-i-1-1 (us-meredith) | the Archer capture both-sided; 1,153 (heaviest Union brigade) | T4 full-arc |
| [us-i-1-2-cutler.md](us-i-1-2-cutler.md) | reg-us-i-1-2 (us-cutler) | first infantry engaged; the 147th NY rate-class primaries; "10 a.m. until 4 p.m." | T4 full-arc |
| [us-i-hq-reynolds.md](us-i-hq-reynolds.md) | reg-us-i-hq (added pass 10) | THE 10:15 PIN three-primary; the succession spine | T3 (command record) |
| [csa-2c-rod-2-iverson.md](csa-2c-rod-2-iverson.md) | reg-csa-2c-rod-2 (csa-iverson) | CA-J1P-2 — the destruction; the single-spike EC6 exemplar | T4 full-arc |
| [csa-2c-rod-4-ramseur.md](csa-2c-rod-4-ramseur.md) | reg-csa-2c-rod-4 (csa-ramseur) | the counter-stroke; report=return exact ×4; the 200-yard reconnaissance | T4 full-arc |
| [csa-2c-ear-4-gordon.md](csa-2c-ear-4-gordon.md) | reg-csa-2c-ear-4 (csa-gordon) | CA-J1P-3 executor; the 3 p.m. cross-line pair; 1,200-engaged primary | T4 full-arc |
| [us-xi-hq-howard.md](us-xi-hq-howard.md) | reg-us-xi-hq (added pass 10) | the assumes-command sub-anchor (11:30 two-primary); the 16:00 ladder | T3 (command record) |
| [us-xi-1-1-vongilsa.md](us-xi-1-1-vongilsa.md) | reg-us-xi-1-1 (us-vongilsa) | the knoll's receiving right; no-report class, honest | T2 (honestly thin) |

Pass-10 upgrades to existing dossiers: csa-3c-het-3-fry.md,
csa-3c-het-4-davis.md, csa-3c-het-1-marshall.md,
csa-3c-het-2-brockenbrough.md (**Heth's four backward-extended to
July 1** — the creek pocket / the 10.30 clock / the facing color
ledgers / the honest thin), csa-2c-ear-1-hays.md +
csa-2c-ear-3-avery.md (July 1 Coster-front extension + EC2 tablet
hops; the Gordon-non-advance conflict CLOSED by his own silence),
us-xi-1-2-harris.md (the knoll drawn both-sided),
us-ii-hq-hancock.md (**the CA-J1P-7 conflict record FORMALIZED**),
**csa-2c-ear-2-smith.md (THE HOFFMAN HUNT LANDED — T2 → T3;
report=tablet 142 exact)**, us-xii-1-2-lockwood.md (Maulsby in
full; Geary's 1,700-strong EC2 primary), us-xii-2-1-candy.md +
us-xii-2-2-kane.md (p. 184 rows, sums exact; the Geary ladder),
us-xii-2-hq-geary.md (pp. 826-830; the 3.30-vs-4.30 CA-J3M-1
conflict pair; the 8 a.m. wave-2 receiving clock),
csa-2c-joh-1/-2/-3/-4 (EC2 tablet hops).

Companion: `docs/reconstruction/audit/dossier-pass-10.md`.

## Pass 11 — the residual sweep (toward full executor coverage)

Pass-10's recommendation executed: the July 1 residual brigades at
judgment grain, the cheap cell re-reads banked across passes, the CSA
EC2 systematic sweep (one B&M-class pass + THE RODES JUNE-30 DIVISION
RETURN, OR 27/2 p. 564 — the only division-grain CSA strength return
printed in the volume's reports run), and the command records folded
in where chain-relevant (Hill No. 534, Ewell No. 467). The pass-10
candidates ED-66…ED-68 + the ED-65 addendum were batch-adopted at
pass open. The cavalry theater was assessed and DEFERRED to pass 12
(capacity honesty; the recommendation stands in the pass report).

| Dossier | Register id | Role | Achieved T |
|---|---|---|---|
| [csa-2c-rod-3-doles.md](csa-2c-rod-3-doles.md) | reg-csa-2c-rod-3 (csa-doles) | strength primary 1,369; the friendly-fire appendix; report=return EXACT 179; the 157th NY both-sided | T4 full-arc |
| [us-i-2-2-baxter.md](us-i-2-2-baxter.md) | reg-us-i-2-2 (us-baxter) | CA-J1P-2's Union receiving executor — the surrender mass at the direct echelon; Coulter's 12:30 pin | T4 (July 1 grain) |
| [us-i-2-1-paul.md](us-i-2-1-paul.md) | reg-us-i-2-1 (us-coulter) | the five-commander cascade; the 16th Maine sacrifice order verbatim; THE DAY-SPLIT TABLE 776/28/14/3 | T3 (July 1 grain) |
| [csa-3c-pen-1-perrin.md](csa-3c-pen-1-perrin.md) | reg-csa-3c-pen-1 (csa-perrin) | THE CA-J1P-5 EXECUTOR — after-4:00 assault clock vs the receiving cluster (ED-70 candidate) | T4 (July 1 grain) |
| [us-xi-3-1-schimmelfennig.md](us-xi-3-1-schimmelfennig.md) | reg-us-xi-3-1 (us-vonamsberg) | the command cascade up; the alley-trap town leg; p. 183 rows digit-exact 807 | T3 (July 1 grain) |
| [us-xi-3-2-krzyzanowski.md](us-xi-3-2-krzyzanowski.md) | reg-us-xi-3-2 (us-krzyzanowski) | the knoll seam's Union center; 82nd OH 312-PFD primary, report=return 181 EXACT | T3 (July 1 grain) |
| [us-xi-2-1-coster.md](us-xi-2-1-coster.md) | reg-us-xi-2-1 (us-coster) | the brickyard; no-report class (the third XI Corps case), both-sided via Hays/Avery | T2 (honestly thin) |

Pass-11 upgrades to existing dossiers: us-i-3-1-biddle.md (**T3 →
T4 July 1 grain** — the 1,287/897/390 next-day primary),
us-i-3-2-stone.md (Roy Stone's report in full — the 11:00 / 12–1
Oak Hill / **1.30 grand-advance** clock ladder → ED-69 candidate),
csa-3c-pen-4-lowrance.md (Scales's July 1 narrative — the 75-yard
halt, the ten-minutes pin), us-xi-1-2-harris.md (**the structured
Barlow-Gordon record** — contemporary layer adopted, the 1903
memoir NO status), us-cav-1-1-gamble.md (the afternoon stand's CSA
half CLOSED via Lane + Perrin), csa-2c-rod-2-iverson.md (EC2 closed
at return grade 1,464; the wounded cell re-read → the
printed-arithmetic conflict class, ED-71 candidate),
csa-2c-ear-4-gordon.md (the 13th Ga row closed at arithmetic grade;
the Guild second ledger), csa-2c-ear-3-avery.md (57th NC = 62,
quadruple-arithmetic), us-v-2-3-weed.md (p. 180 re-read — the total
row digit-exact), us-iii-1-3-detrobriand.md (the garbled clock
resolved: "About 2 p. m." = the tablet pair).

Companion: `docs/reconstruction/audit/dossier-pass-11.md`.

## Pass 12 — the cavalry theater opener + register triage

Pass-11's deferred recommendation executed: East Cavalry Field at
brigade grain (Gregg's division vs Stuart's four engaged brigades)
and South Cavalry Field (Farnsworth vs Merritt), the theater's clock
problem assessed honestly (mostly tablet-adjudicated, thin below the
hour), plus the VI Corps cheap batch (one corps-grain command record)
and a REGISTER-GRAIN TRIAGE pass over all 136 battery entries
(disposition + reason, closing the "measurable coverage" item). ED-69
…ED-71 (pass-11's candidates) batch-adopted at pass open; the
source-schema `anchorsUsed` pattern fixed to admit split anchor ids.
ED-72 (the ECF/SCF chain) and ED-73 (the CSA cavalry marker-provenance
class) proposed, NOT adopted.

| Dossier | Register id | Role | Achieved T |
|---|---|---|---|
| [us-cav-2-1-mcintosh.md](us-cav-2-1-mcintosh.md) | reg-us-cav-2-1 | ECF Union right; CA-ECF-2/3 co-executor | T3 (ECF grain) |
| [us-cav-3-2-custer.md](us-cav-3-2-custer.md) | reg-us-cav-3-2 | ECF Union right; CA-ECF-2/3 co-executor; the day-scope EC6 conflict (ED-43 class) | T3 (ECF grain) |
| [us-cav-2-3-jigregg.md](us-cav-2-3-jigregg.md) | reg-us-cav-2-3 | the reserve; Brinkerhoff's Ridge July 2; the pass's weakest sourcing | T2 (honestly thin) |
| [csa-cav-ham-hampton.md](csa-cav-ham-hampton.md) | reg-csa-cav-ham | CA-ECF-1/3; Hampton's wounding, the theater's clearest officer pin | T3 (ECF grain) |
| [csa-cav-fitz-lee.md](csa-cav-fitz-lee.md) | reg-csa-cav-fitz | CA-ECF-1/3; the 1st Md. Battalion detachment negative | T3 (ECF grain) |
| [csa-cav-chambliss.md](csa-cav-chambliss.md) | reg-csa-cav-chambliss | CA-ECF-1; the pass's thinnest CSA OR-report coverage | T2 (honestly thin) |
| [csa-cav-jenkins.md](csa-cav-jenkins.md) | reg-csa-cav-jenkins | CA-ECF-1; THE AMMUNITION NEGATIVE (~10 rounds/man, ED-44 class) | T2 (honestly thin, negative-evidence) |
| [csa-cav-bn-ha-beckham.md](csa-cav-bn-ha-beckham.md) | reg-csa-cav-bn-ha | frame unit; the distribution table (at most 2 of 6 batteries actually at ECF) | T2 (command/frame grain) |
| [us-cav-3-1-farnsworth.md](us-cav-3-1-farnsworth.md) | reg-us-cav-3-1 (us-cav-farnsworth) | CA-SCF-1/2; the death pin + the Oates-suicide-vs-Parsons conflict | T3 (SCF grain) |
| [us-cav-1-3-merritt.md](us-cav-1-3-merritt.md) | reg-us-cav-1-3 (us-cav-merritt) | CA-SCF-1/2; THE FAIRFIELD-POOLING EC6 FINDING | T4 (July 3 grain) |
| [us-vi-hq-sedgwick.md](us-vi-hq-sedgwick.md) | reg-us-vi-hq (new row) | VI Corps cheap batch; attested-static-with-exceptions (Nevin pass 7, Shaler flagged) | T2 (command record) |

Companion: `docs/reconstruction/audit/dossier-pass-12.md`; the battery
triage lives in `docs/reconstruction/audit/oob-register.json` (each
battery entry's `triage` object) with a summary in the pass report §4.

## Pass 13 — the coverage closers (ED-72/73 adoption + the pass-12 recommendation set)

ED-72/ED-73 batch-adopted at pass open, installing
`anchor-chain-proposal.md` §2.6 (the ADOPTED East/South Cavalry Field
chain) and hardening it with a first-ever EC3 sheet-crop exercise over
the Bachelder 12441 (East Cavalry Field) sheet set — all five sheets
fetched, sha256-verified, and read at brigade/battery grain, patching
the ten pass-12 cavalry-theater dossiers' EC3 sections. Then the
pass-12-recommended set in full: the two self-surfaced horse-artillery
batteries (Randol's, Pennington's), the two orphaned CSA artillery
battalions (Henry's, Lane's Sumter — each closing the last gap in an
otherwise-complete corps battalion arc), one Union corps-artillery
group (V Corps, Martin's — picked over III/VI on a reasoned source
check), and the Anderson/Pender quiet-flank + command batch
(Mahone/Posey/Thomas, register-corrected for Thomas's actual division;
the Provost Guard). Nine new dossiers; no new register rows (all nine
were existing register entries). A full coverage-gap census closes the
pass report.

| Dossier | Register id | Role | Achieved T |
|---|---|---|---|
| [us-btty-randol.md](us-btty-randol.md) | reg-us-ha-2-1 | ECF Union right, split-section; Chester's/Kinney's sections sheet-confirmed ~370 m apart | T3 (ECF grain) |
| [us-btty-pennington.md](us-btty-pennington.md) | reg-us-ha-1-4 | ECF Union right, Custer's attached battery; report=tablet corroboration | T3 (ECF grain) |
| [csa-1c-bn-henry.md](csa-1c-bn-henry.md) | reg-csa-1c-bn-henry | closes the First Corps battalion arc; the CA-SCF-2 CSA-side corroboration (2 batteries) | T4 (battalion grain) |
| [csa-3c-bn-lane.md](csa-3c-bn-lane.md) | reg-csa-3c-bn-lane | closes the Third Corps battalion arc; the double digit-exact 3-way EC6/EC5 cross-check; the ED-74 candidate | T4 (battalion grain, own-report primary) |
| [us-v-arty-martin.md](us-v-arty-martin.md) | reg-us-v-arty | the pass-12-recommended Union corps-arty group; Hazlett's death upgraded to report-primary; Battery I overrun-then-retaken | T4 (brigade grain) |
| [csa-3c-and-2-mahone.md](csa-3c-and-2-mahone.md) | reg-csa-3c-and-2 (csa-mahone) | THE non-participation case; report=tablet digit-exact; the report's silence on the controversy is the finding | T4 (negative-evidence class) |
| [csa-3c-and-5-posey.md](csa-3c-and-5-posey.md) | reg-csa-3c-and-5 (csa-posey) | the PARTIAL-advance case; the four-stage piecemeal commitment; the Bliss-farm cross-ref to Thomas's brigade | T4 (own-report primary) |
| [csa-3c-pen-3-thomas.md](csa-3c-pen-3-thomas.md) | reg-csa-3c-pen-3 (csa-thomas) | register-corrected (Pender's, not Anderson's); NOT quiet — 270 casualties, the batch's highest | T3 |
| [us-hq-provost-patrick.md](us-hq-provost-patrick.md) | reg-us-hq-provost | the cheap command-record closer; downstream CA-J3A-9/10 context (~2,000 prisoners) | T2 (command record) |

Companion: `docs/reconstruction/audit/dossier-pass-13.md`; the ten
pass-12 cavalry dossiers' EC3 patches and the battery-triage update
(Henry's/Lane's constituent batteries + Randol's/Pennington's) are
recorded in the pass report §1/§6, not re-listed here.

## Pass 14 — the first finish-line batch (ED-74 adoption + the census's recommended slice)

ED-74 batch-adopted at pass open (`csa-3c-bn-lane.md`'s
report-vs-corroborated-tablet-conflict class, extending ED-71). Then
the pass-13 census's full recommended order: the two horse-artillery
brigade HQs; the South Cavalry Field EC3 sheet-crop exercise (the gap
pass 13 explicitly left open, patching Farnsworth's/Merritt's/Henry's
battalion's dossiers); the VI Corps brigade batch (all seven remaining
brigades, closing the corps HQ dossier's own Shaler verification
flag); the III Corps Artillery Brigade (the last uncovered Union
corps-artillery group); and all four remaining Artillery Reserve
brigade HQs. Fourteen new dossiers; no new register rows (all fourteen
were existing register entries). A structural finding this pass:
three of the four Artillery Reserve volunteer-brigade commanders
(who double as their own battery's captain) filed NO separate
brigade-command report — only Fitzhugh's is a genuine brigade
narrative.

| Dossier | Register id | Role | Achieved T |
|---|---|---|---|
| [us-ha-1-robertson.md](us-ha-1-robertson.md) | reg-us-ha-1 | the July 3 Artillery-Reserve succession finding (Robertson replaced Tyler); batteries split across 3 theaters | T3 (command grain, own-report primary) |
| [us-ha-2-tidball.md](us-ha-2-tidball.md) | reg-us-ha-2 | a forwarding-letter-class report, self-explained | T2 (command record) |
| [us-vi-1-1-torbert.md](us-vi-1-1-torbert.md) | reg-us-vi-1-1 (us-torbert) | attested-static reserve; commander-itemized 1,663-strength table | T3 |
| [us-vi-1-2-bartlett.md](us-vi-1-2-bartlett.md) | reg-us-vi-1-2 (us-bartlett) | GENUINELY ENGAGED; the composite-command resolution of Nevin's "triple succession" hazard (July 3 Crawford-support sweep) | T4 |
| [us-vi-1-3-russell.md](us-vi-1-3-russell.md) | reg-us-vi-1-3 (us-russell) | attested-static; the report-vs-tablet casualty conflict (0 vs 2) | T3 |
| [us-vi-2-1-grant.md](us-vi-2-1-grant.md) | reg-us-vi-2-1 (us-grant) | attested-static; the OR report's Gettysburg content honestly not located | T3 |
| [us-vi-2-2-neill.md](us-vi-2-2-neill.md) | reg-us-vi-2-2 (us-neill) | the corps's least-static brigade — detached to hold Powers Hill, then Rock Creek to the extreme right | T3 |
| [us-vi-3-1-shaler.md](us-vi-3-1-shaler.md) | reg-us-vi-3-1 (us-shaler) | CLOSES the corps HQ dossier's own flagged verification item; engaged at Culp's Hill 9-11am, sub-hour ladder feeds CA-J3M-2/4 | T4 |
| [us-vi-3-2-eustis.md](us-vi-3-2-eustis.md) | reg-us-vi-3-2 (us-eustis) | the corpus's 4th confirmed no-report-class Union brigade; the unexplained 25-missing figure | T3 |
| [us-iii-arty-randolph.md](us-iii-arty-randolph.md) | reg-us-iii-arty | closes the last uncovered Union corps-artillery group; 3 named officer casualties incl. Randolph himself | T4 (group grain) |
| [us-ar-1r-ransom.md](us-ar-1r-ransom.md) | reg-us-ar-1r | CONFIRMED no brigade report exists; explains Tyler's own "no report from Ransom's battery" line | T2 (command record) |
| [us-ar-2v-taft.md](us-ar-2v-taft.md) | reg-us-ar-2v | own-battery-scoped report; 4 individually-timed casualties | T3 |
| [us-ar-3v-huntington.md](us-ar-3v-huntington.md) | reg-us-ar-3v | 3rd confirmed no-brigade-report instance; Edgell's own battery report | T3 |
| [us-ar-4v-fitzhugh.md](us-ar-4v-fitzhugh.md) | reg-us-ar-4v | the ONE genuine brigade report of the four; a new repulse-duration detail on the Webb's-fence crisis-reinforcement record (ED-41) | T4 |

Pass-14 upgrades to existing dossiers: `us-cav-3-1-farnsworth.md` +
`us-cav-1-3-merritt.md` (the SCF sheet-crop patch — Farnsworth's
Brigade, Merritt's regulars, and Elder's battery all drawn adjacent on
Bachelder sheet j3-03; j3-04 checked negative), `csa-1c-bn-henry.md`
(Bachman's battery sector-positioned by the same exercise),
`csa-cav-bn-ha-beckham.md` (an incidental first-class conflict: a
"HART'S S.C." block drawn at South Cavalry Field directly contradicts
the battalion's own marker, which places Hart's battery off-ground
guarding trains), `us-vi-3-3-nevin.md` + `us-vi-hq-sedgwick.md`
(cross-reference patches), `us-ar-hq-tyler-park.md` (the Ransom-naming
explanation).

Companion: `docs/reconstruction/audit/dossier-pass-14.md`.

## Pass 15 — the register clearance (THE FULL CENSUS CLEARED to zero needs-dossier)

The pass-14 census's complete remainder in one pass: the 3 remaining
non-battery rows (VI Corps Artillery Brigade, Fisher's brigade, Smith's
brigade — identity verified as Col. Orland Smith, XI Corps, not W.
"Extra Billy" Smith, CSA), the 20 needs-own-dossier batteries (III
Corps, V Corps, VI Corps, and Horse Artillery brigades' remaining
batteries), and a full verification sweep over the 13 static-park
Artillery Reserve batteries. No new ED candidates proposed (none
required — all findings are dossier-level or register-triage
corrections, not doctrine rulings). Thirty-seven new dossiers; no new
register rows (all were existing register entries, including a bonus
38th closer, McClanahan's CSA battery, outside the task's core 23-unit
scope, to leave the register's `needs-own-dossier` triage disposition
at true zero). Headline finding: direct verification of the 13
"static-park" batteries found only 2 of 13 (Brooker's and Pratt's, both
held at Westminster, Md.) were genuinely never-engaged — the other 11
were detached and fought, several with real casualties and named
deaths, misclassified by the pass-12 triage batch pass; all 13 (plus
the 20 needs-own-dossier and the bonus McClanahan closer) had their
register `triage.disposition` corrected to `attached-to-dossiered-
battalion` with a `coveredBy` link to their own new dossier.

| Dossier | Register id | Role | Achieved T |
|---|---|---|---|
| [us-vi-arty-tompkins.md](us-vi-arty-tompkins.md) | reg-us-vi-arty | closes the last Union corps-artillery command row; no brigade report exists; the Ziegler's Grove 4-battery convergence | T2 (command record) |
| [us-v-3-2-fisher.md](us-v-3-2-fisher.md) | reg-us-v-3-2 | Big Round Top's own-report primary; the 10 p.m. occupation in the commander's own words | T4 full-arc |
| [us-xi-2-2-smith.md](us-xi-2-2-smith.md) | reg-us-xi-2-2 | closes the XI Corps; identity verified (Orland Smith); report-vs-tablet casualty conflict recorded | T4 full-arc |
| [us-btty-clark.md](us-btty-clark.md) | reg-us-iii-b1 | III Corps salient early-fire battery; 1300 rds | T4 |
| [us-btty-bucklyn.md](us-btty-bucklyn.md) | reg-us-iii-b4 | Freeborn's own succession report fetched this pass; exact 3 p.m. trigger | T4 |
| [us-btty-seeley.md](us-btty-seeley.md) | reg-us-iii-b5 | apple-orchard flank; Seeley's ~17:30 wounding | T3 |
| [us-btty-walcott.md](us-btty-walcott.md) | reg-us-v-b1 | the "not to be found" command-confusion negative | T2 |
| [us-btty-barnes.md](us-btty-barnes.md) | reg-us-v-b2 | attested-static reserve, 4-gun battery | T2 |
| [us-btty-gibbs.md](us-btty-gibbs.md) | reg-us-v-b3 | LRT north-slope duel; rejects a bad "43-casualty" search figure | T3 |
| [us-btty-mccartney.md](us-btty-mccartney.md) | reg-us-vi-b1 | own-report; arrived after the repulse | T3 |
| [us-btty-harn.md](us-btty-harn.md) | reg-us-vi-b3 | Ziegler's Grove convergence witness #4 | T3 |
| [us-btty-waterman.md](us-btty-waterman.md) | reg-us-vi-b4 | Webb's-division support tie | T3 |
| [us-btty-adams.md](us-btty-adams.md) | reg-us-vi-b5 | proves no consolidated Tompkins report exists | T3 |
| [us-btty-williston.md](us-btty-williston.md) | reg-us-vi-b6 | Ziegler's Grove #1; gun-count/position conflict unresolved | T2 |
| [us-btty-butler.md](us-btty-butler.md) | reg-us-vi-b7 | Ziegler's Grove #2, the clearest statement | T2 |
| [us-btty-martin-5us.md](us-btty-martin-5us.md) | reg-us-vi-b8 | Ziegler's Grove #3; Kinzie name-collision resolved | T2 |
| [us-btty-daniels.md](us-btty-daniels.md) | reg-us-ha-1-1 | 12:30p-7a precise window; report=tablet exact | T3 |
| [us-btty-martin-6ny.md](us-btty-martin-6ny.md) | reg-us-ha-1-2 | "flying battery"; in-reserve w/ 1 wounded tension | T2 |
| [us-btty-heaton.md](us-btty-heaton.md) | reg-us-ha-1-3 | UPGRADES the pass-14 brigade dossier's "In Reserve only" claim | T3 |
| [us-btty-elder.md](us-btty-elder.md) | reg-us-ha-1-5 | packages the ADOPTED CA-SCF-2 corroboration at battery grain | T3 |
| [us-btty-graham.md](us-btty-graham.md) | reg-us-ha-2-2 | South Cavalry Field, search-summary grade | T2 |
| [us-btty-calef.md](us-btty-calef.md) | reg-us-ha-2-3 | the opening-gun battery, verbatim tablet claim | T3 |
| [us-btty-fuller.md](us-btty-fuller.md) | reg-us-ha-2-4 | confirms the off-map/never-engaged classification | T2 |
| [us-btty-eakin.md](us-btty-eakin.md) | reg-us-ar-1r-1 | static-park CORRECTED: engaged 2 days, 10 casualties | T2 |
| [us-btty-turnbull.md](us-btty-turnbull.md) | reg-us-ar-1r-2 | static-park CORRECTED: lost/recaptured 4 guns | T3 |
| [us-btty-evanthomas.md](us-btty-evanthomas.md) | reg-us-ar-1r-3 | static-park CORRECTED: direct-hit caisson series drew a CSA cheer | T4 |
| [us-btty-brooker.md](us-btty-brooker.md) | reg-us-ar-2v-1 | static-park CONFIRMED: never left Westminster | T2 |
| [us-btty-pratt.md](us-btty-pratt.md) | reg-us-ar-2v-2 | static-park CONFIRMED: never left Westminster | T2 |
| [us-btty-sterling.md](us-btty-sterling.md) | reg-us-ar-2v-3 | static-park CORRECTED: unique ordnance, McGilvery-line member | T3 |
| [us-btty-taft.md](us-btty-taft.md) | reg-us-ar-2v-4 | static-park CORRECTED: the pass's largest triage/research mismatch | T4 |
| [us-btty-edgell.md](us-btty-edgell.md) | reg-us-ar-3v-1 | static-park CORRECTED: matches Taft's mismatch pattern exactly | T4 |
| [us-btty-norton-1oh.md](us-btty-norton-1oh.md) | reg-us-ar-3v-2 | static-park CORRECTED: relieved a "badly shot up" battery in place | T3 |
| [us-btty-hill.md](us-btty-hill.md) | reg-us-ar-3v-4 | static-park CORRECTED: per-day-dated casualties | T3 |
| [us-btty-dow.md](us-btty-dow.md) | reg-us-ar-4v-1 | static-park CORRECTED: 13 casualties in the pre-charge cannonade | T3 |
| [us-btty-rigby.md](us-btty-rigby.md) | reg-us-ar-4v-2 | static-park CORRECTED: 211 rds at Culp's/Benner's Hill | T3 |
| [us-btty-ames.md](us-btty-ames.md) | reg-us-ar-4v-4 | static-park CORRECTED: precisely-timed Peach Orchard action | T3 |
| [csa-cav-ha-mcclanahan.md](csa-cav-ha-mcclanahan.md) | reg-csa-cav-ha-7 | bonus closer; present-but-non-combatant train guard | T2 |

Companion: `docs/reconstruction/audit/dossier-pass-15.md`.
