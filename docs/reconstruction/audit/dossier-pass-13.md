# Dossier pass 13 — the coverage closers + the full-coverage gap census

**Date:** 2026-07-13 · **Branch:** `audit-dossiers-13` (unmerged) ·
**Scope:** pass-12's recommendation in full — batch-adopt ED-72/73,
run the EC3 sheet-crop exercise pass 12 skipped, close the two
self-surfaced horse-artillery batteries, the two orphaned CSA
battalions, one Union corps-artillery group, and the Anderson/Pender
quiet-flank + command batch — then complete the full-coverage gap
census pass 12 started. Research/documentation only — no Unity, no
battle-file changes. Executed on Sonnet per standing cost policy:
doctrine and validators carry the rigor, quotes are verbatim,
uncertainty is recorded rather than resolved.

## 1. ED adoption + the ECF EC3 sheet-crop patch

- **Batch-adopted ED-72/73** (pass-12's candidates) per the standing
  adopt-and-adjust doctrine, recorded in full in
  `angle-editorial-decisions.md` §"Dossier pass 12 adoption rulings":
  ED-72 installs the East/South Cavalry Field chain (CA-ECF-1..3,
  CA-SCF-1..2) as a fifth-tier evidence chain (tablet-adjudicated,
  alongside ED-39's monument class); ED-73 names the CSA
  cavalry-brigade position-marker class (and, extended this pass, the
  Bachelder ECF sheet set's own hand-lettered annotations) as a
  monument-adjacent tier, ±30 min per event, never anchor-defining
  alone. `anchor-chain-proposal.md` gained a new §2.6 (the ADOPTED
  chain table) and §2.6.1 (the sheet-crop exercise); the §4 status
  table and header summary updated in place.
- **The EC3 sheet-crop exercise** (pass-12-skipped, executed this
  pass): all five Bachelder East Cavalry Field sheets
  (`12441001`-`12441005`) fetched via `fetch_bachelder_maps.sh`'s
  pinned URLs, **sha256-verified against every hash in
  `bachelder-manifest.json`**, and read at brigade/battery grain with
  `reconstruction/scripts/crop_sheet.py` + `georef_maps.py`'s
  `img_to_local`. Internal-consistency check: the Rummel farm building
  read off three independent sheets (`ecf-j3-am`/`-mid`/`-pm`) landed
  within ~25 m of the manifest's own tie point on every read.
  **Ten cavalry-theater dossiers patched** (McIntosh, Custer, J. I.
  Gregg, Hampton, Fitzhugh Lee, Chambliss, Jenkins, Beckham,
  Farnsworth, Merritt) — the task specified "11 cavalry dossiers";
  pass 12 in fact produced exactly ten (eight ECF + two SCF), and this
  pass patched all ten; the discrepancy is recorded here rather than
  inventing an eleventh. South Cavalry Field's own sheets
  (`12440022`/`12440023`) were explicitly OUT of this pass's scope
  (the task named the 12441 — East Cavalry Field — set) and remain a
  pass-14 item, recorded honestly in both SCF dossiers rather than
  silently left to look "done."

## 2. Unit set and achieved T-levels

Nine new dossiers (overlay 141 → 150 units; **149 register-grain
entries now audited**, up from 140 — the overlay's 150th key,
`us-8oh`, is a legacy regiment-grain outlier from pass 4, not a
register row, per the register's own regiments-are-name-only-strings
convention). No new register rows this pass (all nine were existing
register entries). Master table: 330 rows, 150 audited overlay units.

| Unit | T | Basis |
|---|---|---|
| Randol's Battery (us-btty-randol) | T3 (ECF grain, split-section) | tablet + sheet-crop; the ~370 m section split confirmed drawn |
| Pennington's Battery (us-btty-pennington) | T3 (ECF grain) | report=tablet corroborating pair (second instance this pass) |
| Henry's Artillery Battalion (csa-1c-bn-henry) | T4 (battalion grain) | own-report primary; report=tablet digit-exact EC1/EC6 |
| Lane's Sumter Artillery Battalion (csa-3c-bn-lane) | T4 (battalion grain, own-report primary) | double digit-exact 3-way cross-check (rounds AND casualties) |
| V Corps Artillery Brigade (us-v-arty-martin) | T4 (brigade grain) | own-report primary across 3 fetched pages |
| Mahone's Brigade (csa-3c-and-2-mahone) | T4 (negative-evidence class) | own report, report=tablet digit-exact; the report's silence IS the finding |
| Posey's Brigade (csa-3c-and-5-posey) | T4 (own-report primary, partial-advance) | four-stage piecemeal commitment, commander's own account |
| Thomas's Brigade (csa-3c-pen-3-thomas) | T3 | register-corrected (Pender's, not Anderson's); tablet + attributed report (dead mirror) |
| Provost Guard (us-hq-provost-patrick) | T2 (command record) | 93rd NY tablet fetch-verified; rest attributed grade |

## 3. Best finds

1. **THE EC3 SHEET-CROP EXERCISE ITSELF, first find.** Ten
   cavalry-theater dossiers went from "tie-point-only, ±75 m
   asserted" to actual drawn-bar reads. Two genuinely new facts: (a)
   McIntosh's brigade sits ~1,400 m EAST of Custer's on the same
   sheet — the tablet text alone ("took position with the right on
   Hanover Road") never made this separation legible; (b) Beckham's
   battalion's "possibly Breathed's" hedge (pass 12) upgrades to a
   drawn CONFIRMATION — McGregor's and Breathed's batteries both
   positively placed at Rummel farm, ~110 m apart. A third feature —
   "Section of Green's Baty" — has NO match in Beckham's six-battery
   roster or any fetched OOB source; recorded as an open
   identification question, not resolved.
2. **THE DOUBLE DIGIT-EXACT CROSS-CHECK (Lane's Sumter battalion).**
   Three independently fetched battery markers (Ross's, Patterson's,
   Wingfield's) sum EXACTLY to the battalion tablet's figure on BOTH
   rounds expended (506+170+406=1,082) AND casualties (10+9+11=30) —
   a doubled version of the single cross-check pattern earlier passes
   have found once at a time. Riding the same row: Lane's OWN report
   gives a DIFFERENT total (29, internally self-consistent) — a
   genuinely new conflict shape, proposed as ED-74.
3. **THE FIVE-SOURCE CA-SCF-2 CORROBORATION.** Pass 12 called the
   ~17:30 South Cavalry Field charge clock "the strongest single clock
   landed anywhere in the cavalry theater" on a THREE-source basis
   (Union tablet ×2 + Merritt's report). This pass adds TWO independent
   CSA battery markers (Latham's, Garden's — Henry's battalion) that
   both credit "about 5 P.M." fire "repelling cavalry under Brig. Gen.
   Farnsworth" — the anchor now rests on five independent sources
   across both sides of the field.
4. **MAHONE'S REPORT'S SILENCE.** The report a research pass would
   most want to explain the "why didn't Mahone advance" controversy
   instead narrates only the skirmish line and the shelling — offering
   NO account of the refusal at all. Fetched and confirmed this pass,
   not inferred: the negative is itself the primary finding, exactly
   the unit-truth-spec's "attested stillness... first-class, cited"
   discipline applied to a controversy rather than a quiet unit.
5. **A NEW BLISS-FARM CROSS-REFERENCE, surfaced twice independently.**
   Posey's brigade's report names an advance "200-300 yards beyond the
   barn and house, which were burned"; Thomas's brigade's own tablet
   independently names its July 3 right flank "near the Bliss House
   and Barn." Neither dossier previously cross-referenced the other;
   this pass ties them for the first time — the same contested farm
   complex, from two different Third Corps brigades, on two different
   days.
6. **A digit-exact division-casualty cross-check, incidentally
   surfaced while re-fetching J. I. Gregg's brigade**: McIntosh's
   tablet total (35) + Gregg's tablet total (21) = the 2nd Cavalry
   Division tablet's own aggregate (56), fetch-verified this pass —
   not one of the pass's headline assignments, but a clean,
   independent confirmation of three separately-sourced numbers.

## 4. EC-class coverage honesty

- **EC1**: Hazlett's death (V Corps Artillery) upgrades from
  tablet-only to a report-primary with an explicit time of death;
  Wingfield's wounding (Lane's battalion) and the officer-attrition
  entries in Mahone's/Posey's own reports are the pass's other EC1
  pins. No new pin reaches primary-report grade for Randol's or
  Pennington's batteries (both no-casualty or thin).
- **EC2**: genuinely open for BOTH new CSA artillery battalions
  (Henry's, Lane's) — neither a report nor a tablet states a personnel
  total for either, a real gap this pass records rather than fills
  with an inferred B&M-repro figure. Two strength conflicts newly on
  record (Posey's, Thomas's brigades), neither resolved, per standing
  OOB doctrine (never averaged).
- **EC3**: the pass's central program (§1) for the ten cavalry
  dossiers; command-frame only (no new coordinate geometry) for the
  Mahone/Posey/Thomas/Provost batch — none of those four dossiers
  authored a Bachelder sheet read this pass (the main-field j3-03/04
  sheets were not fetched), an honest gap distinct from the ECF-sheet
  program's scope.
- **EC4**: Posey's four-stage piecemeal commitment (§3.2 candidate
  material) is this pass's best EC4 find — a named, self-attested
  MULTI-leg failure-to-cohere, a genuinely different shape than the
  corpus's usual single-leg advance/retreat pattern. Mahone's EC4 is
  the deliberate absence of a leg (the defining fact, not a gap).
- **EC5**: the Battery I (V Corps Artillery) overrun-and-recapture
  episode is the pass's most vivid single EC5 record — guns actually
  left Union hands and were retaken, a first for this corpus's
  artillery-activity vocabulary (ED-34's classes do not yet name this
  shape; not proposed as a new ED this pass, flagged for a future
  pass if the pattern recurs).
- **EC6**: TWO new report-vs-tablet/marker-sum conflicts this pass
  (Lane's battalion's 29-vs-30 split; Posey's brigade's 12/71-vs-15/68
  split) plus the DOUBLE digit-exact cross-check (§3.2) and the
  incidental division-total cross-check (§3.6) — this pass's EC6 work
  is unusually rich in independently-verifiable arithmetic, both
  confirming and conflicting.

## 5. ED candidates NOT self-adopted

- **ED-74 candidate — the report-vs-corroborated-tablet conflict
  class**, extending ED-71 (failed-row-arithmetic) to a THIRD conflict
  shape: a report's own internally-consistent total disagreeing with
  an independently cross-checked tablet/battery-marker consensus,
  distinct from ED-71's single-table self-contradiction and from
  ED-49/ED-52's report-vs-return scope conflicts. Full text and the
  exhibit (Lane's Sumter battalion, 29 vs 30): §5 of this report's
  source dossier, `csa-3c-bn-lane.md`. Proposal: adopt the
  battery-marker-corroborated tablet figure as the EC6 basis while
  carrying the report's total verbatim as a first-class component,
  tagging the cell class to match ED-71's pattern exactly. NOT
  self-adopted — owner ruling needed.

## 6. Sources, clockProfiles, suites, commits

- New sources fetched/identified this pass (full list per dossier
  source registers, not individually added to `sources.json` this
  pass — following pass-12's own precedent of not syncing every
  dossier-cited id into the source library every pass; a periodic
  "Sources d-N" pass remains the mechanism for that, last run at
  pass 11): five Bachelder ECF sheet masters (sha256-verified);
  gettysburg.stonesentinels.com tablet/marker pages for Randol's,
  Pennington's, Henry's battalion + 2 constituent batteries, Lane's
  battalion + 3 constituent batteries, Mahone's, Posey's, Thomas's
  brigades, and the 93rd NY; OR reports for Tidball (No. 372),
  Robertson (No. 368), Henry (No. 463), Lane (No. 548), Martin
  (No. 221), Mahone, Posey (both via nps.gov); two re-attempted OR
  fetches that came back verified-negative (Stuart's report, pp. 693/
  712; Munford's report, pp. 737/739 — both narrowing, not closing,
  standing gaps carried since pass 11/12); one confirmed-dead mirror
  (civilwarhome.com, redirects to an unrelated commercial domain —
  affects Thomas's brigade's report access, flagged for a pass-14
  re-route via civilwar.com's page-ID scheme).
- No new `clockProfile` records were added this pass — the new
  sources' clocks are tablet-class or report-thin, same standing
  pattern as pass 12.
- Suites at close: **reconstruction 122 passed + 1 skipped
  (unchanged from pass 12 — no schema or test changes this pass) ·
  pipeline/tool not re-run this pass** (no code paths touched;
  dossiers, the overlay, the register, and docs are the only files
  this pass modified — none of which feed the pipeline/tool suites or
  the corpus compile). **ED-21 stagingSeed pin HELD** (unchanged;
  `angle.bundle.json` was not recompiled this pass — nothing in this
  pass's diff touches `reconstruction/sources/`, `reconstruction/
  claims/`, or any other corpus-compile input).
- **A style note, recorded for the owner**: the `oob-register.json`
  triage patch (§ below) was applied via a Python `json.dump` that
  normalized the file's indentation, producing a full-file diff
  (8,527/8,527 lines) instead of a minimal one. The CONTENT change is
  exactly the nine triage-object updates described below (verified via
  `Counter` diffing before commit); the formatting churn is cosmetic
  but makes that one commit's diff unreadable at a glance — flagged
  honestly rather than silently left for a future `git blame` to
  puzzle over.
- Commits pushed early and often on `audit-dossiers-13` (unmerged):
  ED-72/73 adoption; the EC3 sheet-crop patch (10 dossiers); the four
  new coverage-closer dossiers (Randol/Pennington/Henry/Lane); the V
  Corps Artillery dossier; the Mahone/Posey/Thomas/Provost batch; the
  overlay + master-table regeneration; the register triage update; the
  index update. Owner copies in
  `docs/benchmarks/captures/audit-d13/` (gitignored, main repo).
- Master table: `cd reconstruction && uv run --with openpyxl python
  scripts/build_unit_audit.py` — 330 rows, 150 audited units (141 + 9).
- **Register triage**: Randol's and Pennington's batteries move to
  `attached-to-dossiered-battalion` (own dossier); Henry's battalion's
  four batteries and Lane's battalion's three batteries move to
  `attached-to-dossiered-battalion` (inherited, parent battalion
  dossier). `needs-own-dossier`: 33 → 24 from those nine closes alone.
  **A second fix, found while building the census (§8)**: three
  battery entries (Winslow's `reg-us-iii-b2`, Smith's `reg-us-iii-b3`,
  Watson's `reg-us-v-b5`) had STALE triage — each already has its own
  dossier from an earlier pass (`us-btty-winslow.md`,
  `us-btty-smith.md`, `us-btty-watson.md`) but the pass-12 triage
  sweep had left their `disposition` at `needs-own-dossier`. Corrected
  to `attached-to-dossiered-battalion` (own dossier) this pass — a
  register data-quality fix, not new research. **Final count:
  `needs-own-dossier` 33 → 21; `attached-to-dossiered-battalion`
  90 → 102; `static-park` unchanged at 13** (90+9+3=102 ✓,
  33-9-3=21 ✓).

## 7. Command-record grain (the judgment call, recorded)

Matching pass-11 §8's and pass-12 §7's precedent: **no
`reg-csa-1c-arty-cmd` or `reg-csa-2c-joh-hq` standalone dossier was
authored this pass**, despite both surfacing in the census (§8) as
"non-battery uncovered." Both already carry a `notes` field on the
register explaining their content is DISTRIBUTED across existing
dossiers (the First Corps artillery command's CA-J3A-1 signal-order
chain is carried on Eshleman's and Miller's dossiers; Johnson's
division HQ's Culp's Hill command record is carried on its four
brigade dossiers, added at pass 8 as a register-maintenance note, no
standalone file). Classified `command-record-sufficient` in the census
rather than `needs-dossier` — writing a standalone row file for either
would be grain inflation the sequencing doctrine warns against, per
the identical rationale pass 11/12 already applied to Gregg's and
Stuart's division-command rows.

## 8. THE FULL-COVERAGE GAP CENSUS

Every one of the register's 288 entries, partitioned into exactly one
class (computed directly from `oob-register.json` + `dossier-
overlay.json`, verified to sum to 288):

| Category | Count | Detail |
|---|---|---|
| **Audited (≥T2, has a dossier)** | **149** | up from 140 at pass-12 close (+9); zero audited units fall below T2 (checked: 97 at T4, 34 at T3, 18 at T2) |
| **Battalion/group-covered (battery, inherited, no individual dossier)** | 81 | attached to a dossiered parent battalion/group per ED-36; NOT individually audited but doctrine-sufficient per the CSA-battalion-grain pattern |
| **Battery needs-own-dossier** | 20 | down from pass-12's 33 (-9 closed this pass, +2 net reclassified as true off-map on closer census reading — see below) |
| **Static-park (battery, honesty placeholder)** | 13 | unchanged; Artillery Reserve's un-individuated brigades — their FOUR parent command rows are themselves in the needs-dossier count below, so this placeholder class cannot yet be upgraded to battalion-covered |
| **Non-battery needs-dossier** | 17 | 9 in-build Union brigades (7 of VI Corps's 8 brigades — only Nevin's is dossiered; 1 V Corps PA Reserves brigade — Fisher's; 1 XI Corps brigade — Smith's) + 8 artillery-brigade command rows (III Corps, VI Corps, both horse-artillery brigade HQs [Randol's/Pennington's own parents — cheap, source material already fetched this pass], 4 Artillery Reserve brigade HQs) |
| **True off-map / off-square (never on the mapped terrain, incl. the ECF/SCF extension)** | 6 | Huey's cavalry brigade + its attached battery (Fuller's, Battery C/3rd US) at Westminster; Robertson's, Jones's, and Imboden's CSA cavalry commands + Jones's attached horse battery (Chew's) in the Fairfield/Cashtown corridor — a one-line command note is sufficient per the standing off-map ruling, not a research gap |
| **Command-record-sufficient (no standalone row; content distributed elsewhere)** | 2 | First Corps artillery command (Eshleman's/Miller's dossiers carry it); Johnson's division HQ (its four brigade dossiers carry it) — §7 |
| **TOTAL** | **288** | matches `oob-register.json`'s own `stats.totalEntries` |

**The finish line, stated plainly**: closing the register to "every
unit ≥ T2" requires **37 more dossiers at most** (20 battery +
17 non-battery) — NOT 139 (288 − 149), because 81 batteries are
already doctrine-sufficient at battalion grain, 13 are an explicit
honesty placeholder rather than a research gap, 6 are off-map by
ruling, and 2 are already covered by distributed content. Of the 37,
the two horse-artillery brigade HQ rows (`reg-us-ha-1`, `reg-us-ha-2`)
are the cheapest possible next items — this pass already fetched and
read Tidball's and Robertson's own reports as their parent-command
sources.

**Recommended pass-14 order**: (1) the two horse-artillery brigade
HQ rows (cheap, sources already in hand); (2) South Cavalry Field's
own EC3 sheet-crop exercise (`j3-03`/`j3-04`, the gap this pass
explicitly left open for Farnsworth's/Merritt's dossiers); (3) the
VI Corps brigade batch (7 brigades, all in-build, the corps HQ's own
"attested-static majority" framing means most are likely cheap T2
closes — Shaler's is flagged as needing primary verification per the
existing VI Corps HQ dossier); (4) III Corps Artillery Brigade (the
one remaining uncovered Union corps-arty group, closing that class
entirely alongside this pass's V Corps and pass-1's II/I/XI/XII Corps
dossiers); (5) the four Artillery Reserve brigade HQ rows, which would
let the 13 static-park batteries graduate to an honest
battalion-covered classification instead of an open placeholder. After
that set, the register's remaining gaps are the 20 battery
needs-own-dossier rows (mostly V/VI Corps and the horse-artillery
tail) plus Fisher's and Smith's two remaining brigade rows — at that
point essentially the entire register clears the T2 floor.
