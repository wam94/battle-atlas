# Dossier pass 12 — the cavalry theater opener + register triage

**Date:** 2026-07-12 · **Branch:** `audit-dossiers-12` (unmerged) ·
**Scope:** pass-11's deferred recommendation: the cavalry theater
(East Cavalry Field brigade grain + South Cavalry Field), the VI
Corps cheap batch, and a register-grain triage over the ~136-battery
tail. Research/documentation only — no Unity, no battle-file changes.
Executed on a smaller model than prior passes as a deliberate cost
decision: doctrine and validators carry the rigor, quotes are
verbatim, and uncertainty is recorded rather than resolved.

## 1. ED adoption + schema fix (first tasks)

- **Batch-adopted ED-69/70/71** (pass 11's candidates), recorded in
  full in `angle-editorial-decisions.md` §"Dossier pass 11 adoption
  rulings": ED-69 annotates CA-J1P-1's interior ladder (Coulter 12:30
  → Doles ~13:00 → Stone=Dobke 13:30 → tablet 14:00 adopted → Hill
  ~14:30), no move (both new primaries are report-nominal, ED-25 rule
  4); ED-70 upgrades CA-J1P-5's basis to the Perrin-vs-receiving-
  cluster pair + Robinson's rear-guard tail, no move; ED-71 is the
  failed-row-arithmetic rule (extends ED-49/ED-52) for printed return
  rows that fail their own arithmetic in two independent channels.
  `anchor-chain-proposal.md` and the dossiers index's ED-counter were
  updated in place; Stone's and Dobke's clockProfile assessment notes
  resolved from "pending ED-69" to final (values unchanged).
- **Schema fix**: `reconstruction/schemas/source.schema.json`'s
  `clockProfile.anchorsUsed` pattern (`^CA-[A-Z0-9]+-[0-9]+$`) did not
  admit ED-66's split-anchor ids (`CA-J1M-4a`/`4b`). Widened to
  `^CA-[A-Z0-9]+-[0-9]+[a-z]?$` (optional trailing lowercase letter);
  two tests added (`test_clock_profile_anchors_used_admits_split_
  anchor_ids`, `test_clock_profile_anchors_used_still_rejects_
  malformed_ids`).
- Editing `sources.json`'s assessment text changed the corpus content
  checksum, so `angle.bundle.json` + `angle-bundle-audit.md` were
  recompiled per the standing rule (never leave the suite red on a
  provenance-only edit) — checksum moved to `3245941b9279…`, **ED-21
  stagingSeed pin HELD (`d470c469…`)**.

## 2. Unit set and achieved T-levels

Eleven new dossiers + one new register command row (overlay
130 → 141 units; master table 329 → 330 rows):

| Unit | T | Basis |
|---|---|---|
| McIntosh (us-cav-2-1-mcintosh) | T3 (ECF grain) | CA-ECF-2/3 co-executor; own OR report verified post-battle-itinerary grade |
| Custer (us-cav-3-2-custer) | T3 (ECF grain) | CA-ECF-2/3 co-executor; the day-scope EC6 conflict (ED-43 class); own report verified diary-form |
| J. Irvin Gregg (us-cav-2-3-jigregg) | T2 (honestly thin) | Brinkerhoff's Ridge July 2; the pass's weakest sourcing (tablet 404'd; own report identified, not fetched) |
| Hampton (csa-cav-ham-hampton) | T3 (ECF grain) | CA-ECF-1/3; the wounding, the theater's clearest officer pin, unclocked |
| Fitzhugh Lee (csa-cav-fitz-lee) | T3 (ECF grain) | CA-ECF-1/3; the 1st Md. Battalion detachment negative |
| Chambliss (csa-cav-chambliss) | T2 (honestly thin) | CA-ECF-1; own OR report verified thin (Brandy Station content at both fetched pages) |
| Jenkins (csa-cav-jenkins) | T2 (honestly thin, negative-evidence) | CA-ECF-1; the ~10-rounds-per-man ammunition negative (ED-44 class) |
| Stuart Horse Artillery (csa-cav-bn-ha-beckham) | T2 (command/frame grain) | the distribution table — at most 2 of 6 batteries actually at ECF |
| Farnsworth (us-cav-3-1-farnsworth) | T3 (SCF grain) | CA-SCF-1/2; the death pin + the Oates-suicide-vs-Parsons conflict (unresolved) |
| Merritt (us-cav-1-3-merritt) | **T4 (July 3 grain)** | CA-SCF-1/2; THE FAIRFIELD-POOLING EC6 finding, own OR report primary |
| VI Corps HQ (us-vi-hq-sedgwick) | T2 (command record) | attested-static-with-exceptions; new register row `reg-us-vi-hq` |

## 3. Best finds

1. **THE ECF/SCF CHAIN PROPOSAL (→ ED-72 candidate, first find).** No
   R1 skeleton chain existed for the cavalry theater (the
   anchor-chain-proposal drafted six phases in the main-field frame;
   East/South Cavalry Field were never chained). This pass builds one
   from scratch off the executor dossiers' own tablets/reports:
   - **CA-ECF-1 (~noon/12:00-13:00, envelope 11:00-13:00, tablet-
     adjudicated).** FOUR independent CSA brigade markers agree
     within an hour on arrival: Hampton "about noon," Chambliss
     "about noon," Fitzhugh Lee "soon after midday," Jenkins "about
     noon" — the cavalry theater's best-corroborated single clock,
     and a genuine four-primary cluster the main-field chain rarely
     matches.
   - **CA-ECF-2 (~14:00, tablet-adjudicated).** McIntosh's and
     Custer's tablets independently agree: "About 2 P. M. a large
     Confederate force having been observed... Gregg ordered [Custer]
     to return" / "about 2 P. M. [Custer] was immediately engaged."
   - **CA-ECF-3 (~15:00, tablet-adjudicated).** McIntosh's tablet
     (fullest account, names the Stallsmith Farm concealment) and the
     2nd Division tablet independently agree: "About 3 P. M...
     Hampton's and... Fitzhugh Lee's Brigades... emerged from the
     woods... and charged but were repulsed."
   - **CA-SCF-1 (~noon/13:00) and CA-SCF-2 (~17:30, TRIPLE agreement).**
     Merritt's own OR report ("marched... about 12 m.") is a PRIMARY,
     not tablet-class, start clock; the ~17:30 charge is a genuine
     triple agreement — the 3rd Division tablet, the 1st Brigade
     tablet, AND Merritt's own report's "about four hours... brought
     to a close by a heavy rain," independently corroborating each
     other across the report/tablet divide. This is the strongest
     single clock landed anywhere in the cavalry theater.
2. **THE FAIRFIELD-POOLING FINDING (Merritt's dossier).** The Reserve
   Brigade's tablet casualty total (418, with 268 captured/missing) is
   wildly disproportionate to a "drove the enemy... routing him" South
   Cavalry Field action — Merritt's own report explains it: the 6th US
   Cavalry was separately detached toward Fairfield, "engaged a
   superior force of the enemy, not without success," and "lost
   heavily," with Maj. Starr losing an arm. The tablet pools TWO
   fields into one total. This is exactly the ED-49/ED-52 scope
   discipline applied fresh — a report explaining what a pooled figure
   actually contains.
3. **THE STUART HORSE ARTILLERY DISTRIBUTION TABLE.** Beckham's own
   marker states plainly that his six batteries were "not permanently
   attached... but were sent [to brigades] when needed," and names
   the July 3 attachment of each: only McGregor's (with Hampton's
   brigade) is confidently placed at East Cavalry Field; Chew's rode
   with Jones's brigade (itself off-field); Griffin's was detached to
   Ewell's infantry corps; Hart's guarded the trains; Moorman's has no
   report. The honest picture is one or two batteries at Rummel farm,
   not "the horse artillery" as a body — directly useful for the
   battery-triage pass (§4) and a corrective to any undifferentiated
   rendering.
4. **The ammunition negative (Jenkins's brigade).** "Armed with
   Enfield Rifles but an oversight brought to this field only about
   ten rounds of ammunition... actively engaged mainly on foot as
   sharpshooters... while this lasted" — the theater's cleanest
   ED-44-class fact (present, engaged, then necessarily silent, with
   a stated cause).
5. **Four independent CSA "about noon" markers, one Union "about 1
   P.M." marker (Farnsworth's brigade), one Union primary "about 12
   m." (Merritt's report) — the movement-onset clocks are, on the
   whole, the theater's STRONGEST evidence class, better corroborated
   than the mid-afternoon charge clocks on the main field.** Recorded
   as a genuine, if narrow, finding: the cavalry theater's clock
   problem is uneven, not uniformly thin.

## 4. EC-class coverage honesty

- **EC1**: Hampton's wounding is the theater's clearest officer pin
  (unclocked); Farnsworth's death is the best-narrated (Parsons's
  B&L account, retrospective, attributed grade only — not
  independently page-verified this pass); Maj. Starr (6th US,
  Fairfield) is a primary-sourced pin from Merritt's own report;
  Maj. Ferry (5th MI) surfaced incidentally in Custer's report's
  itinerary line.
- **EC2**: every new unit's strength is B&M-repro HOP grade
  (addressing-gettysburg-oob); NO report-stated brigade total was
  found for any of the eight new brigade dossiers this pass — the
  cavalry corps's OR reports are, almost without exception, thin or
  itinerary-form where the infantry corps's are narrative-rich (see
  EC5 below). This is a genuine EC2 gap, not a choice.
- **EC3**: the East Cavalry Field ±75 m georeference class
  (`reconstruction/spatial/bachelder-manifest.json`, `ecf-base` tie
  point: Hanover×Low Dutch + Rummel farm centroid) is CITED by every
  ECF dossier but was NOT re-exercised with a new sheet crop this
  pass — no drawn-bar read, no new tie point. South Cavalry Field sits
  on the main heightmap square (no ±75 m caveat needed) but likewise
  got no dedicated spatial exercise. Honest gap, recorded rather than
  papered over with an unearned precision claim.
- **EC4**: leg-level detail is essentially absent for the mounted
  charge sequences on both fields — the tablets compress an
  afternoon's back-and-forth into one or two sentences each. The
  EC4 physics-timing program (flagged unexecuted since pass 10)
  remains unexecuted; the cavalry theater adds no new endpoints to it
  this pass beyond the four arrival/charge anchor pairs above.
- **EC5**: the pass's headline finding here is negative — FOUR of
  eight new brigade-grain OR reports (McIntosh, Custer, Chambliss,
  Beckham) were fetch-verified as thin, itinerary-form, or
  chronologically front-loaded (opening on Brandy Station or a
  post-battle march rather than the Gettysburg narrative), a pattern
  distinct from anything seen in ten passes of infantry-corps
  research. The tablet/marker layer carries the narrative burden
  instead — itself a first-class finding about this corpus's cavalry
  coverage, not a research failure to paper over.
- **EC6**: the Fairfield-pooling finding (§3.2) is the pass's best
  EC6 work; the Custer day-scope conflict (ED-43 class, tablet 257
  total vs the July-3-only return row's "1 and 28 killed") is the
  second; Jenkins's and Beckham's "losses/casualties not reported"
  are explicit first-class negatives, not gaps.
- **The register-grain BATTERY TRIAGE** (the ~118-battery-tail item):
  every one of the register's 136 battery entries gained a `triage`
  object (`disposition` / `coveredBy` / `reason`) in
  `oob-register.json`. Method: an individual dossier wins first (17
  batteries); else the parent battalion/group/command dossier's
  coverage is checked and inherited (CSA battalion grain per ED-36,
  the Ewell-wing group dossier, the four dossiered Union corps-arty
  groups, McGilvery's July-3 composite line per ED-35, and this
  pass's Beckham dossier — with a PER-BATTERY attachment note, since
  the battalion dossier itself shows only ~2 of 6 batteries actually
  at East Cavalry Field); else an Artillery-Reserve battery not
  individually researched gets `static-park` (an honesty placeholder,
  explicitly NOT a claim of non-participation); everything else is
  `needs-own-dossier` with a specific reason.

  **Result: 90 attached-to-dossiered-battalion (17 own + 73
  inherited) · 13 static-park · 33 needs-own-dossier.** The 33-battery
  gap, named:
  - **Two CSA battalions have NO battalion dossier at all** — Henry's
    (Hood's Division, 4 batteries, 2 already in-build with zero
    dossier coverage) and Lane's Sumter Georgia (Anderson's Division,
    3 batteries, all not-yet-cast). These are the two holes in
    otherwise-complete First/Third Corps battalion arcs.
  - **Three Union corps artillery brigades are essentially
    unresearched at battery grain** — III Corps (5 batteries, none
    individually dossiered), V Corps (4 of 5 uncovered; only
    Rittenhouse dossiered), VI Corps (7 of 8 uncovered; only Cowan
    dossiered — this pass's corps-grain command dossier frames but
    does not individually research the artillery brigade).
  - **Both Union horse-artillery brigades (9 batteries) are
    uncovered** — including Randol's (Batteries E&G, 1st US) and
    Pennington's (Battery M, 2nd US), BOTH namechecked in this pass's
    own fetched sources (McIntosh's tablet: "Custer and McIntosh and
    the Batteries of Randol and Pennington were soon hotly engaged")
    yet neither has a dossier — a direct, high-priority pass-13 item
    that this pass's own research surfaced but did not close.
  - **McClanahan's battery** (Imboden's command, structurally grouped
    under the Stuart Horse Artillery register ids but NOT part of
    Beckham's marker's distribution list) is its own one-battery gap.

## 5. New ED candidates (proposed, NOT adopted)

- **ED-72 — the East/South Cavalry Field chain (CA-ECF-1..3,
  CA-SCF-1..2).** Full text and evidence: §3.1 above and the eight
  cavalry-theater dossiers' "Chain anchors substantiated" sections.
  No R1 skeleton existed to adopt-and-adjust; this is a from-scratch
  proposal, tablet/marker-heavy (only CA-SCF-1/2 rest partly on report
  primaries — Merritt's). Options for the owner: adopt as a fifth-tier
  evidence chain (tablet-adjudicated, alongside the ED-39 monument
  class) with the marker-provenance caveat (ED-73) attached, or hold
  pending a primary-report pass on Stuart's full report (identified,
  not exhaustively fetched — §7) and Kilpatrick's report (identified,
  not fetched).
- **ED-73 — the CSA cavalry-brigade position-marker class.** The six
  gettysburg.stonesentinels.com "confederate-headquarters" pages used
  throughout this pass (Hampton's, Fitz Lee's, Chambliss's, Jenkins's,
  Stuart's Division, Stuart Horse Artillery) read as GNMP-era
  interpretive position markers, not the 1900s War Department troop
  tablets ED-39 profiles — no erection date or adjudicating body was
  found on any fetched page. Proposal: name this as its own evidence
  subclass (monument-adjacent tier, ±30 min class per event, never
  anchor-defining alone until corroborated), distinct from ED-39's War
  Department tablet class, because its process/date of adjudication is
  currently unknown and CSA cavalry has essentially NO other physical
  marker record (ED-39 §4's asymmetry-by-side rule in its strongest
  form). Not self-adopted — an owner ruling on how to weight this
  class going forward.

## 6. Sources, clockProfiles, suites, commits

- New sources fetched/identified this pass: or-27-1-gregg-dmm,
  or-27-1-mcintosh-jb, or-27-1-custer, or-27-1-merritt, or-27-2-
  chambliss, or-27-2-beckham (all OR fetches, several verified thin —
  see §4 EC5); six gettysburg.stonesentinels.com marker pages (CSA
  cavalry) + five Union cavalry tablet pages + the VI Corps tablet;
  addressing-gettysburg-oob (B&M-repro hop, per-regiment/per-brigade
  strength figures for all eight new units); civilwarcycling.com's OR
  index (used to LOCATE reports, not itself cited as content); two
  attributed-grade web-research items (Parsons's B&L account, the
  Hampton-wounding secondary survey) explicitly flagged as NOT
  citation-grade primaries.
- No new `clockProfile` records were added this pass (the new sources'
  clocks are tablet-class or report-thin; none met the bar for a
  worked ED-25 profile — recorded honestly as a gap rather than force
  a thin profile).
- Suites at close: **reconstruction 122 passed + 1 skipped (120+2 new
  from the schema-fix tests) · pipeline 59 · tool 109** (tool/
  untouched this pass, run read-only to confirm). Corpus validator OK.
- **Bundle**: recompiled once, after the sources.json editionNote
  edits (§1) — checksum `3245941b9279…`, **ED-21 stagingSeed pin HELD
  (`d470c469…`)**. No further recompiles needed (the cavalry-theater
  dossiers, overlay, and register triage touch only
  `reconstruction/dossiers/`, `docs/reconstruction/audit/`, and
  `reconstruction/schemas/` — none of which feed the corpus compile).
- Commits pushed early and often on `audit-dossiers-12` (unmerged);
  owner copies in `docs/benchmarks/captures/audit-d12/` (gitignored,
  main repo).
- Master table: `cd reconstruction && uv run --with openpyxl python
  scripts/build_unit_audit.py` — 330 rows, 141 audited units (130 + 11).

## 7. Command-record grain (the judgment call, recorded)

Two deliberate register-grain restraints, matching pass-11 §8's
precedent:

1. **No `reg-us-cav-2-hq` (Gregg's division command) row was added.**
   Gregg's own report (or-27-1-gregg-dmm) was fetched and its content
   (the McIntosh/Custer credit lines, the Custer-retention decision)
   folded into the McIntosh and Custer dossiers as evidence for
   existing/proposed anchors, not as a new command-record subject. No
   anchor names Gregg's decision as its own event (yet) — if ED-72 is
   adopted with Gregg's retention-of-Custer as a distinct sub-anchor,
   this restraint should be revisited.
2. **No `reg-csa-cav-stuart-div-hq` row was added.** Stuart's own
   report (OR 27/2 pp. 679-720, No. 565) was identified via the
   civilwarcycling.com index but NOT substantively fetched this pass
   (a 42-page whole-campaign report; the East Cavalry Field section's
   exact page was not located) — recorded as a standing fetch gap
   (§4/§8), not filled with a command-record row built on a source
   that was never actually read.

Rationale unchanged from pass 11: adding register command rows for
sources used as supporting evidence, rather than as an anchor's own
named event, is grain inflation the sequencing doctrine warns against.

## 8. Pass-13 recommendation (with the full-coverage gap census)

**Immediate cavalry-theater follow-through** (the two units this
pass's own fetches surfaced as urgent and did not close): Randol's
(Batteries E&G, 1st US) and Pennington's (Battery M, 2nd US) horse
artillery batteries — both namechecked in this pass's own sources,
neither dossiered. A short pass could close both at T2-T3 from the
same tablet layer this pass used.

**Fetch gaps to close before ED-72 hardens**: Stuart's full OR report
(the ECF section's exact page, within pp. 679-720), Kilpatrick's
report (No. 359, pp. 985-996 — the Farnsworth-charge-order
controversy), and a targeted re-fetch of Fitzhugh Lee's OR report
(pp. 737-741, currently misattributed in the working index to
Munford) and Beckham's report (pp. 772-773, verified thin at the
fetched boundary but not exhaustively read).

**The full-coverage gap census** (what remains between here and every
register unit ≥ T2, per the unit-truth-spec's target):

| Category | Count | Detail |
|---|---|---|
| Battery rows needing their own dossier | 33 | §4 — two CSA battalions (Henry's, Lane's Sumter) with zero battalion coverage; III/V/VI Corps + both horse-artillery brigades on the Union side |
| Battery rows at `static-park` (honesty placeholder, not yet individually researched) | 13 | Artillery Reserve's un-individuated brigades (1st Regular, 2nd/3rd Volunteer beyond their already-dossiered batteries) |
| Infantry brigades still not-yet-cast or undossier'd at the quiet-flank grain | Mahone/Posey/Thomas (the quiet Anderson/Pender CSA brigades); Huey's cavalry brigade (never engaged, per the standing off-map/negative record — a one-line command note, not a research gap) | carried from pass 11 §9, unchanged this pass |
| Provost/HQ rows | Patrick's Provost Guard (`reg-us-hq-provost`) | carried from pass 11 §9, unchanged |
| Command rows added but not yet researched beyond frame grain | `reg-us-vi-hq` (this pass, T2 command record — the corps frame exists; individual brigade/battery research is the actual work) | new this pass |
| Depth work queued behind coverage | the EC4 physics-timing program (endpoints banked since pass 10, still unexecuted); T4→T5 Soldier-View candidates (Iverson's field remains the natural first, per pass 11); the claims-compilation graduation the spec forecasts | unchanged |

**Recommended pass-13 order**: (1) the two horse-artillery batteries
(cheap, high-value, self-surfaced this pass); (2) the two orphaned CSA
artillery battalions (Henry's, Lane's Sumter — each is a
battalion-grain dossier away from closing a whole gap class, matching
the ED-36 pattern exactly); (3) one Union corps-arty-group dossier
(III, V, or VI Corps — whichever the owner judges highest-value,
closing 4-7 batteries at once via the established group-grain
pattern); (4) Mahone/Posey/Thomas + the Provost Guard as a cheap
command-batch closer, matching this pass's VI Corps HQ treatment.
After that set, the register's remaining gaps are almost entirely
individual battery rows inside brigades that already have SOME
coverage — the point at which, per pass 11's own framing, "coverage
claims can shift from 'phases' to 'the field.'"
