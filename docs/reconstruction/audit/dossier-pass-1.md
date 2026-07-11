# Dossier pass 1 — anchor-executors of the July 3 afternoon chain

**Date:** 2026-07-10/11 · **Branch:** audit-dossiers-1 (unmerged) ·
**Status: COMPLETE.**

Layer 1 of the unit-truth-spec sequencing doctrine: full six-class dossiers
for the units that ENACTED the adopted ED-24 chain anchors, excluding the
already-T5 Angle cast. Dossiers: `reconstruction/dossiers/` (fixed heading
schema, tier-prefixed facts, full battle arcs per the owner's scope
ruling). Master-table consultation layer:
`docs/reconstruction/audit/dossier-overlay.json`, applied deterministically
by `reconstruction/scripts/build_unit_audit.py` (new overlay input; values
win over workbook-preserved cells). Owner-readable copies:
`docs/benchmarks/captures/audit-d1/`.

## 1. Unit set and achieved T-levels

| Unit | Register id | Chain anchor(s) executed | Achieved T (audited) |
|---|---|---|---|
| Miller's 3rd Co., Washington Artillery | reg-csa-1c-wsh-3 | CA-J3A-1 (fired the signal guns) | **T3** (EC2/EC6 attested-unreported at company grain) |
| Eshleman's Washington Artillery Bn | reg-csa-1c-bn-wash | CA-J3A-1 (parent/order chain) | **T4** (battalion grain) |
| First Corps artillery command (Walton/Alexander) | reg-csa-1c-arty-cmd (NEW row) | CA-J3A-1 order path | context row; carried in the two dossiers above |
| Alexander's Battalion | reg-csa-1c-bn-alexander | CA-J3A-2/3 (clock-bearer's battalion); CA-J3A-4 witness | **T4** (battalion grain) |
| Hunt — AoP artillery command | reg-us-arty-hq (NEW row) | CA-J3A-4/5 (the conservation/cease orders) | **T2** (command record; order events clock-fixed) |
| McGilvery's line (1st Vol. Bde + composite) | reg-us-ar-1v | CA-J3A-4 (ordered silence); CA-J3A-10 (enfilade) | **T4** (brigade/line grain) |
| Osborn's XI Corps / Cemetery Hill group | reg-us-xi-arty | CA-J3A-4/5 (the hill's cease); CA-J3A-10 | **T4** (group grain) |
| Hazard's II Corps Artillery Bde | reg-us-ii-arty | CA-J3A-4 counterpoint (fought to exhaustion) | **T4** (brigade grain) |
| Woodruff's I/1st US | reg-us-ii-b4 | CA-J3A-4 counterpoint; CA-J3A-10 (Pettigrew left) | **T4** |
| Arnold's A/1st RI | reg-us-ii-b2 | CA-J3A-4 counterpoint; CA-J3A-9/10 | **T4** |
| Brown's B/1st RI | reg-us-ii-b3 | CA-J3A-4 (the one ordered II Corps withdrawal) | **T4** |
| Rorty's B/1st NY | reg-us-ii-b1 | CA-J3A-4 counterpoint; CA-J3A-9/10 | **T4** |
| Stannard's 2nd Vermont Bde | reg-us-i-3-3 | CA-J3A-8 (the change of front); CA-J3A-10 | **T4** (T5-grade in the Angle window via V2 claims) |

Register changes: two command rows added (`reg-us-arty-hq`,
`reg-csa-1c-arty-cmd`) — the chain's two ordering minds had no rows to
carry their dossiers; stats updated (273 entries, 125 not-yet-cast).

T-level honesty: T4 = the spec roll-up met at the unit's dossier grain
(anchored endpoints, cited activity summary, physics-timed legs where the
unit moved, episode-grain casualty apportionment), with per-class gaps in
§3 explicit. Nothing here is claim-compiled (T5 is authoring, not
research). Miller is T3, not T4: company strength is a secondary hop and
company casualties are attested-unreported.

## 2. The five facts that most strengthen the chain anchors

1. **CA-J3A-4 now carries its executor's own clock.** [C via
   or-27-1-hunt, report-nominal profile +5 (0,+10), medium] Hunt's OR:
   "About 2.30 p. m., finding our ammunition running low … I directed
   that the fire should be gradually stopped, which was done, **and the
   enemy soon slackened his fire also**" (or-27-1-hunt, encyc.org full
   text, verified). The adopted ~14:30 was previously C/D-grade
   inference; it is now the order's stated time, with the causal link to
   the Confederate slackening in the same sentence.
2. **CA-J3A-1's order chain is closed primary at every link.** [A vs
   CA-J3A-1] Longstreet: "Colonel Walton was ordered to open the
   batteries" (or-27-2-longstreet); Walton: "fired the signal guns, as
   agreed with General Longstreet" + the order text "Let the batteries
   open…" (walton-shsp5-1877 pp. 47-51); Eshleman: "you ordered me to
   give the signal for opening along the entire line. Two guns in quick
   succession were fired from Captain Miller's battery, and were
   immediately followed by all the battalions along the line"
   (or-27-2-eshleman); position: "about 100 yards to the left of the
   peach orchard, on the immediate left of Captain Taylor's battery."
   The chain's defining event now has a bodied executor with an order
   path, a company-grain position, and the two-gun mechanics.
3. **The slackening is executor-substantiated on ALL THREE Union
   sectors, with three different documented mechanisms.** [A window /
   C] McGilvery: pre-arranged hold-fire + a put-down breach + the
   1.5-hour duration + "total, thirty-nine guns" — his OR pp. 882-883
   gap CLOSED this pass (or-27-1-mcgilvery); Osborn: his own cease
   order, contemporary motive "as no satisfactory results were
   obtained … a few moments later the infantry of the enemy broke over
   the crest" (or-27-1-osborn p. 750) — the tightest Union
   cease-to-step-off coupling in the corpus; Hazard: "did not at first
   reply … they returned it till all their ammunition, excepting
   canister, had been expended" in "an hour and a quarter"
   (or-27-1-hazard pp. 477-481) — even the non-complying sector's fire
   fell away inside the 14:00–15:00 envelope, by exhaustion. Every
   witness disagreement about "when the Union fire slackened" now has a
   sectoral explanation.
4. **The ~14:00 "camp clock" cluster grew from two reports to four,
   across both armies.** [conflict-record class, ED-25 rule 4] Osborn's
   "About 2 p. m. they opened" (or-27-1-osborn) and Stannard's "At about
   2 p. m. the enemy again commenced" (or-27-1-stannard) join Longstreet
   and Smyth at a nominal ~14:00 cannonade opening — strong evidence
   this is a report-clock CLASS, not four independent errors, which is
   exactly the basis rule 4 rests on. (McGilvery's newly-verified "At
   about 12.30 o'clock" adds the spread's early pole.)
5. **CA-J3A-8 gained its first physics-timeable duration input.** [B: 10
   rounds ÷ ~2.5 rds/min ≈ 4 min] Randall's 13th VT: "We had fired about
   10 rounds per man when they seemed to be in utter confusion"
   (or-27-1-randall p. 353) — brackets the wheel-to-surrender interval
   at ~4–6 min, consistent with the authored 15:20–15:28 window; joined
   by the leg geometry now in rods (Benedict pp. 495-497) and the
   re-verified negative that NO witness clock exists for the wheel
   across all three full OR reports.

Honorable mentions: Miller's 400–500-yard supporting ADVANCE (retires his
attested-static reading; or-27-2-eshleman); Alexander's OR-vs-SHSP
displacement contradiction (maintained-positions vs follow-Pickett); the
official p. 174 casualty return for Stannard's brigade (351, full K/W/M
by regiment); Eshleman's ambiguous "About thirty minutes after the signal
guns … our infantry moved forward" (conflict-flagged; the short-cannonade
side's newest primary datum IF read as the charge).

## 3. EC-class coverage honesty table

| Unit | EC1 | EC2 | EC3 | EC4 | EC5 | EC6 | Thin because |
|---|---|---|---|---|---|---|---|
| Miller's 3rd Co. | ● | ◐ | ● | ● | ● | ○ | strength = secondary hop; casualties "heavy but not reported in detail" (attested-unreported) |
| Eshleman's Bn | ● | ◐ | ● | ● | ● | ● | 240-vs-338 strength conflict unresolved (ED-38); rounds attested-absent |
| Alexander's Bn | ● | ◐ | ◐ | ◐ | ● | ● | per-battery positions/casualties mostly unreported; no July 2/3 casualty split; OR-vs-SHSP movement conflict |
| Hunt (command) | ● | n/a | ● | n/a | ● | n/a | command record by design |
| McGilvery | ● | ● | ◐ | ● | ● | ◐ | line-axis battery coordinates deferred to pass 2; composite-line battery casualties sit on other returns |
| Osborn | ● | ◐ | ◐ | ◐ | ● | ◐ | men-engaged unlocated; hill coordinates at the square margin; men-casualty detail in revised statement p. 183 (unfetched); rounds report-declined |
| Hazard | ● | ● | ● | ● | ● | ◐ | brigade K/W/M two accountings; revised statement p. 177 unfetched; no numeric rounds |
| Woodruff | ● | ◐ | ● | ● | ● | ● | strength single-source; no battery OR report exists (index-verified) |
| Arnold | ● | ◐ | ● | ◐ | ● | ◐ | ED-7 withdrawal dispute open (Hunt's "toward the close" added at the during-pole); no July 2/3 split |
| Brown | ● | ◐ | ● | ● | ● | ● | strength single-source; withdrawal order's primary wording unfound |
| Rorty | ● | ◐ | ● | ● | ● | ● | strength/battle-total single-source; gun-attrition sequence variant (SS vs Waitt) |
| Stannard | ● | ◐ | ● | ● | ● | ● | 1,788-vs-1,950 strength conflict (1,950 failed re-verification); per-episode split bracketed, not stated |

● = filled with citations (incl. negative evidence) · ◐ = filled with
flagged conflicts/gaps · ○ = attested-unreported/open. Universal gap:
**numeric rounds-expended figures do not exist in any fetched report**
(Eshleman none, Alexander none, Hazard qualitative only, Osborn
explicitly declines, McGilvery gives the ENEMY's 10,000) — the spec's
rounds÷rate duration method needs ordnance returns (OR ordnance series /
Busey & Martin page access), carried as the pass-2/3 standing item.

## 4. New ED candidates (proposed by this pass; ADOPTED 2026-07-11 as ED-32…ED-38 in the pass-2 batch ruling, angle-editorial-decisions.md)

- **ED-32 — Stannard strength basis.** Adopt regiment-grain 480/647/661
  (= 1,788) as the engaged basis; the 1,950 figure failed
  re-verification this pass and the official return's 351 casualties
  reads more plausibly against 1,788.
- **ED-33 — the mixed slackening reading.** Adopt: ordered economy
  (Hunt OR, contemporary) + Osborn's cease (contemporary motive "no
  satisfactory results"; the LURE framing is postwar) + genuine
  damage/withdrawals (Brown out ~14:40, Cushing wrecked) — and an
  adopted identification for Alexander's "the 18 guns are gone". The
  contemporary-vs-postwar motive split is now verbatim.
- **ED-34 — artillery activity classes.** Three documented behaviors
  for authored tracks: *ordered-silence-then-fire* (McGilvery),
  *ordered-cease* (Osborn), *fought-to-exhaustion* (Hazard) — with
  matching casualty-curve classes (silent-then-fire vs flat-then-spike).
- **ED-35 — cast McGilvery's July 3 composite line as a command unit**;
  the 39-gun roster is now primary and ordered left→right.
- **ED-36 — battalion-grain CSA July 3 geometry** (Alexander/Eshleman/
  Dearing tablet lines) as the Confederate reference frame for pass 2.
- **ED-37 — Rank's section (and the "New Jersey battery") on
  McGilvery's line.** McGilvery's own OR roster now conflicts with the
  OOB's East-Cavalry-Field placement (upgrading the old sheet-8-vs-OOB
  conflict to OR-vs-OOB); possibly a split section — needs cavalry-side
  sources.
- **ED-38 — CSA artillery strength readings.** Eshleman 240 (build) vs
  338 (B&M-type reproduction); adopt with hops cited (Alexander's 576
  already matches the build).

## 5. Sources added and clockProfiles assessed

16 records added (sources.json 55 → 71), all with access routes and
failure records per house style; 2 upgraded:

| id | What | clockProfile |
|---|---|---|
| or-27-1-stannard | OR pp. 348-351, verified (civilwar.com transcription route) | report-nominal −53 (the ~14:00 cluster) |
| or-27-1-randall | OR pp. 351-353, verified | report-nominal (no anchor statements) |
| benedict-1888 | Vermont in the Civil War vol. 2, page-pinned via search-inside; author was Stannard's ADC | none |
| veazey-or (UPGRADED) | full text found in the OR APPENDIX pp. 1041-1042, verified | report-nominal −55 |
| or-27-2-eshleman | OR (NPS transcription), verified | report-nominal 0 [−53,+7] (interval statement) |
| or-27-2-alexander | OR (NPS transcription), verified | report-nominal (no J3A anchor clocks) |
| alexander-shsp4-1877 | SHSP 4 letter, verified (single-issue scan) | retrospective-watch +7 [0,+15] |
| walton-shsp5-1877 | SHSP 5 letter (Perseus), verified; polemic caveat | retrospective-watch +7 (paraphrase-widened) |
| or-27-2-pendleton | OR (NPS transcription), verified | report-nominal +7 |
| owen-1885 | ACCESS FAILED for the Gettysburg chapter — recorded; Owen-as-quoted via Walton only | none |
| longstreet-hightide-1904 | the "exactly 1.30 P.M." order claim (Gutenberg), verified | retrospective-watch −23 (conflict record) |
| addressing-gettysburg-oob | B&M-type strengths reproduction; hop flagged | — |
| or-27-1-hunt | OR pp. 228-243 via encyc.org (ehistory serial gap found and recorded) | report-nominal +5 [0,+10] medium |
| hunt-bl-3 | B&L Third Day; TWO reproductions, historycentral ABRIDGED (caveat recorded) | retrospective-watch +7 |
| or-27-1-osborn | OR pp. 747-751, read live at ehistory | report-nominal −53 (joins the cluster) |
| osborn-weeklytimes-1879 | 1879 account, as-quoted only; ruse verbatim = verified absence | none |
| or-27-1-hazard | OR pp. 477-481 read live + Iron Brigader cross-check (verbatim-identical) | report-nominal +7 [−8,+22] medium |
| or-27-1-mcgilvery (UPGRADED) | pp. 882-883 gap CLOSED | report-nominal +5 [−30,+37] (12:30 opening outlier) |

Access-environment findings recorded on the records: web.archive.org is
hard-blocked for the fetcher (the verification pass's Wayback route is
session-dependent); ehistory 403s WebFetch but serves the browser pane;
ehistory serial 043 has a page gap over OR pp. ~228-244; civilwarhome.com
is dead (parked); civilwar.com's page-by-page OR transcription works via
a stable URL pattern (id = 188622 + page) with OCR noise.

## 6. Suites and mechanics

- reconstruction 103 (102 passed + 1 skipped) · pipeline 59 · tool 108 —
  all green on the branch after the sources landed (the committed Angle
  bundle/audit embed the source count; a metadata recompile was run —
  the ED-21 stagingSeed pin held, enforced by test).
- Master table regenerated via
  `uv run --with openpyxl python scripts/build_unit_audit.py`; 315 rows
  (190 in-build + 125 register); 12 rows carry audited T-levels from the
  overlay.

## 7. Recommended pass-2 unit set (the reference-frame layer)

The units other units' position/timing statements cite:

**Union gun line + II Corps front:**
1. Cowan's 1st NY Independent (`us-btty-cowan`) — the Brown-replacement
   geometry at the crisis.
2. The documented in-window reinforcing batteries: Fitzhugh's K/1st NY,
   Parsons's A/1st NJ, Weir's C/5th US (also Stannard's July 2 object),
   Wheeler's 13th NY, Cooper's B/1st PA (its ~15:00 tablet track).
3. Hazard's frame infantry: Hays's division (Smyth, Sherrill) and
   Gibbon's (Hall, Harrow) — the brigades every battery statement is
   positioned against; several in-build.
4. reg-us-ar-hq (Artillery Reserve HQ/park + trains) — the ammunition
   physics behind Hunt's policy and the resupply legs Osborn describes.
5. Wainwright's East Cemetery Hill group + Rittenhouse (Little Round
   Top) — closes the Union gun ring; both cited across the lines
   (Hunt: "The steady fire from McGilvery and Rittenhouse … caused
   Pickett's men to 'drift'").
6. McGilvery's line at battery grain (the nine roster batteries incl.
   the ED-37 Rank/NJ question).

**CSA artillery battalions:**
7. Dearing's battalion (fronting Pickett; "arrived since dusk" with
   Eshleman per Pendleton — same march chain).
8. Cabell's battalion (the Peach Orchard arc's fourth battalion).
9. Pegram's + McIntosh's (Third Corps arc fronting Pettigrew).
10. The documented negatives: Garnett's idle battalion, the Ewell-wing
    near-silence (Dance/Nelson/Carter splits) — the bombardment
    picture's attested silences.

Rationale: every pass-1 dossier's open EC3 items (battery-point
coordinates on the McGilvery/Osborn lines; CSA per-battery anchors) and
the ED-37/ED-38 rulings are exactly what this layer resolves; pass-1
dossiers already carry the primary spacing data to place them against
(Hazard's 150-yard grove relation, McGilvery's left→right roster,
Eshleman's company layout, Pendleton's battalion order).
