# Day expansion slice 3 — the July 1 authoring day

**Branch:** `day-expansion-3` (unmerged; owner gate) ·
**Scripts:** `tool/scripts/author-dayexp3-morning.ts`,
`tool/scripts/author-dayexp3-afternoon.ts` (committed deterministic
derivation records, the `author-w*`/A1/A2/dayexp1/dayexp2 pattern) ·
**ADR:** 0005 (exercised, not amended) · 2026-07-13

The battle's opening day, authored from the passes 9–11 dossier bank
(Buford's stand, the meeting engagement, the railroad cut, Iverson's
field, Barlow's Knoll, the double-corps collapse and the retreat
through town), as two phase battle files under the ADR-0005 manifest.
**This completes the three-day battle**: all three days now carry
reconstructed phases (§9). The July 2/July 3 files and the shipped
bundle are UNTOUCHED (§6).

## 1. The phase split (the design call)

**Chosen: two reconstructed phases abutting at the midday-lull seam
(13:00 LMT), plus an honest evening note.**

| Phase | Clock | File |
|---|---|---|
| `july1-morning` | **07:30–13:00 LMT** (startTime 27000, endTime 19800) | `gettysburg-july1-morning.json` |
| `july1-afternoon` | **13:00–18:00 LMT** (startTime 46800, endTime 18000) | `gettysburg-july1-afternoon.json` |
| `july1-evening` | — | `not-reconstructed` note (CA-J1P-9 — Ewell declines the hills — is SEQUENCE-ONLY and correctly unclocked; the 7th Indiana's dark-hours Culp's extension and the XII/III arrival columns have no clocked chain — nothing shown rather than something invented) |

Rationale for the seam: the morning chain (ED-26) ends at the lull
(CA-J1M-7 onset ~11:30; Rodes tablet "about noon" bounds the far
side), and the afternoon chain's recorded interior ladder (ED-69:
Coulter 12:30 general firing → **Doles ~13:00 formation** →
Stone/Dobke 13:30 → tablet 14:00 adopted) begins around 13:00 — the
seam sits between the chains at the day's quietest documented
half-hour, and the two formations that straddle it (Rodes's brigades
forming on Oak Hill; Pender's line at its first bound) are the morning
file's closing keyframes, re-expressed at afternoon t=0. Front edge:
07:30 is CA-J1M-1's adopted hour (the 1886 First Shot marker — the
identity was contested, the hour broadly accepted). Back edge: 18:00
— the last hard clock is CA-J1P-8 (the 17:25 Hancock dispatch,
timestamped-document tier ±0); Robinson's "until nearly 5 p.m."
rear-guard tail and Schurz's "after 5 o'clock" corps bound close the
consolidation's documented build-out inside the window. The cost of
the 18:00 end is that the XII/III Corps arrival columns (reaching the
field's edge toward and after 18:00) are excluded — recorded as the
honest cut §4.1 rather than authored from arrival notes.

**Cross-phase continuity is structural:** the afternoon script reads
the morning file and starts every carried unit at its cited 13:00 end
state — verified as a build-failing assertion, not a warning (32
units carried, position/facing/strength exact).

## 2. The cast and authoring summary

Off-field July 1 units are simply absent (they arrive in later
phases — the manifest model handles it): no II/III/V/VI/XII Corps, no
Longstreet, no Johnson/Stuart, no Artillery Reserve.

| | Morning | Afternoon |
|---|---|---|
| Units | 32 (16 US / 16 CSA) | 45 (24 US / 21 CSA) |
| New entrants at t=0 | — | 13 (Early's four brigades + Jones's battalion; Krzyzanowski, von Gilsa, Ames, Coster, Orland Smith; Wheeler, Wilkeson/G-4US, Wiedrich) |
| Events (fire windows) | 12 | 39 |
| Keyframes | 118 (100 documented / 18 inferred; all cited) | 215 (213 documented / 2 inferred; all cited) |
| Newly cast register rows | 3 (`us-cav-gamble`, `us-cav-devin`, `us-btty-calef` — castStatus set; overlay 204 mapped / 0 unmapped) | — |

- **Morning movers** carry the ED-26 chain: Buford's delaying action
  (Gamble's line-geometry primary + the drawn j1-03 bar with the
  115 m HQ-marker check; Devin's four-road screen; Calef's
  first-Union-gun tablet), Heth's ED-66 deploy (4a ~08:00–09:00)
  then attack (4b ~10:15–10:30, Davis's 10.30 keystone vs the
  receiving cluster), the Iron Brigade charge and the Archer capture
  (both-sided at man grain), Cutler's rate-class opening (147th NY
  207-of-380 in half an hour), the railroad-cut charge (a
  fixed-segment event — the 6th Wis fought detached from its brigade
  track), Hall's withdrawal-by-piece, the I Corps arrival stagger
  (Doubleday's 1.5–2-hour primary), the XI Corps' leading edge
  (45th NY skirmishers 11:30), and the Rodes/Pender formations as
  seam states.
- **Afternoon movers** carry ED-27 as amended: Rodes's 14:00 attack
  with O'Neal's instant repulse and the drawn 3-Ala-detached anatomy;
  **Iverson's destruction** (§3); Daniel's cut fight at his own
  one-third fraction; Ramseur's en-masse counter-stroke and the
  street-line halt; Doles's knoll conjunction and the 157th NY
  episode; Early both-sided at the knoll (Gordon=Schurz pair;
  von Gilsa/Ames receiving with the which-broke-first conflict
  carried); Hays/Avery vs Coster's brickyard (the no-report brigade
  authored from the division above and the enemy in front); the
  ~16:00 double collapse (Perrin's ED-70 executor arc: ravine dress →
  hold-fire → fence → obliques → town; Scales's wreck crossing
  Lowrance's documented 500-man waypoint; Lane held off by Gamble's
  dismounted stand — the three-report convergence); the town-retreat
  legs the dossiers actually carry (the 45th NY's street grain,
  Cutler's embankment, the 16th Maine's sacrifice order verbatim,
  Baxter's named street losses); and the Cemetery Hill consolidation
  (Wainwright's East-Hill layout VERBATIM per battery; Orland Smith's
  garrison anchor; Steinwehr's 2 o'clock arrival).
- **Command records ride the moments layer, not invented units**: the
  Reynolds 10:15 three-primary pin, Howard's ~11:30 assumption
  (ED-68(a)), and the CA-J1P-7 Hancock arrival CONFLICT RECORD
  (report 15:00 pole vs his own 17:25 dispatch's ~16:25 + Howard's
  16:30, partisan-dispute note intact) are per-phase timeline moments
  with full citations — consistent with the build's standing pattern
  (no HQ units cast in any phase file; the register command rows
  remain not-yet-cast).
- **Strength discipline:** t=0 strengths are the evidence layer's
  bases — primaries where they exist (Shepard's 1,048; Doles's 1,369;
  Gordon's 1,200-with-detachment; Chapman Biddle's 1,287; the Rodes
  June-30 division return's five rows at return grade: Daniel 2,294 /
  Doles 1,403 / Iverson 1,464 / Ramseur 1,090 / O'Neal 1,794), tablet
  hops flagged (Hays ~1,200, Avery ~900, Smith ~800), B&M-repro hops
  flagged (Baxter 1,452, Paul 1,537, Stone 1,317, von Amsberg 1,670,
  Krzyzanowski 1,403, Coster 1,156), and HONESTLY-OPEN rows authored
  at compilation class with the flag in the citation text (Devin,
  von Gilsa, Ames, Cutler's brigade total, Calef's personnel).
  Decays follow the EC6 records: the day-split primaries (Coulter's
  776/28/14/3 table; the 41st NY's July-2-3-only row; the 11th Miss
  train-guard split), the rate-class statements, ED-71's
  printed-arithmetic rule on Iverson's row, ED-49 missing-scope
  handling throughout; where no per-day primary exists the
  apportionment is flagged as reconstruction in the citation.
- **Formations per records:** every approach march authors `column`
  (the pikes were the day's roads: Cashtown pike, Emmitsburg road,
  Heidlersburg/Harrisburg road, Middletown road); the screens and
  skirmish lines author `skirmish`; the two attested breaks author
  `routed`/`scattered` (von Gilsa's knoll break; the Archer/Davis
  pocket extractions; Coster's brickyard wreck).
- **Tier-B physics:** the approach legs are paced at column rates
  against the documented endpoints (Doubleday's stagger; Schurz's
  7:00-start/10:30-hurry/13:30-deploy ladder; Morrow's 6-7 miles;
  Rodes's Middletown turn; Early's Heidlersburg descent), authored
  `inferred` with the derivation in the citation. No report-nominal
  clock moved a keyframe (Morrow's 9 a.m., Stone/Dobke's 1.30,
  Doubleday's 1.30 Ewell reading, Hancock's 3 p.m. — all carried in
  citations at their ED-25 profiles).

## 3. Cited highlights (the chains as authored)

| Content | As authored | Basis |
|---|---|---|
| Buford's stand | Gamble's line at the drawn j1-03 bar (mon-gamble-hq 115 m check), the 200-yard fall-back, the relief hand-off; Devin's four-road screen with the two-hour hold; Calef's split sections and the first-gun tablet; the 10:10 dispatch as a timeline moment (timestamped document, ±0) | CA-J1M-1/2/3; ED-66's deploy-phase clocks; us-cav-1-1/-1-2, us-btty-calef dossiers |
| The ED-66 split | 4a: Heth deploys t=T(9:00) with Pegram's guns opening t=T(8:15) (Gamble/Buford executor clocks; Heth's 9-o'clock ignorance quote carried); 4b: the attack strikes t=T(10:15)-T(10:30) — Davis's "About 10.30" keystone against Wadsworth 10:00/Reynolds 10:15/Cutler 10:00; ~90 minutes of cavalry-only fighting rendered as exactly that | ED-66 (the slice's spine); or-27-2-davis-jr p. 649; or-27-2-shepard |
| Reynolds 10:15 | A moment pin + the launching keyframe's citation (the Iron Brigade deployment he died directing); three-primary basis (tablet = Wadsworth = Doubleday); Howard's 11.30 carried as the collapsed word-reaching-him reading | CA-J1M-5 as upgraded by ED-66's rider |
| The railroad cut | Davis's track wheels to the cut and breaks (scattered, −350 in the trap minutes); the charge is a fixed-SEGMENT event at the cut (the 6th Wis + 95th NY + 14th Brooklyn fought off their brigade tracks); Blair's surrender, the 2nd ME gun recaptured, the 147th released; Davis's own "about 1 p.m." retirement closes the morning file's CSA line | CA-J1M-6; or-27-1-dawes-6wi both-sided; the pass-10 §5 recorded tension (11:00-class cut fall vs the 1 p.m. retirement) held as recorded |
| Iverson's field | Formation tablet → the j1-09 drawn mid-advance bar (t=T(14:15), documented) → the death line 100 yards from Baxter's wall → the spike: 1,464→650 in the 14:40–15:00 keyframe pair, with the 500-on-the-line quote, the four-echelon corroboration, the 308-missing capture mass, and ED-71's printed-arithmetic record ALL in the citations; Halsey's remnant rally and the railroad pursuit follow | CA-J1P-2 (ED-67 basis); the audit's single-spike EC6 exemplar rendered as a single spike |
| Barlow's Knoll | Both-sided inside one drawn crop: Gordon's farm label → contact bar (82 m from the statue) → the crest; the Ames drawn crest bar (57 m); von Gilsa's three-regiment line (41st NY absent, its return row the day-split anchor); Jones's battalion enfilade (Schurz's receiving primary); Barlow w&c on Gordon's record; the which-brigade-broke-first Schurz-vs-Ames conflict carried whole | CA-J1P-3 (ED-67: the Gordon=Schurz cross-line pair); j1-11 |
| The double collapse | CA-J1P-4: the XI line folds ~16:00 (tablet + Jacobs's 4-o'clock rally clock); CA-J1P-5: Perrin's after-4 order vs the four-source receiving cluster (ED-70), Scales's 75-yard halt both-sided with Perrin's 200-yard reading (both carried), Robinson's rear guard to nearly 5; the seminary barricade stormed by the men who built it that morning (Paul's entrenchment ↔ Perrin's breastwork-of-rails, both-sided across four hours) | ED-70; or-27-2-perrin/-scales; or-27-1-cbiddle/-dobke-45ny |
| The retreat through town | The dossiered legs only: the 45th NY's alley trap (~100 extricated), Cutler's perfect-steadiness embankment march, the 16th Maine spent by order, Baxter's named street casualties, Schimmelfennig's four-day concealment; the corps' missing columns cited as the leg's ledger (XI 1,448 c/m) | CA-J1P-6 (interval hung between 4/5 and 7); the ABT 3:45–5:00 bracket in the moments file |
| Hancock's arrival | NOT a unit, NOT a resolved time: the CA-J1P-7 conflict record as a timeline moment at the adopted ~16:15 with both poles, the document-pin-against-his-own-report structure, and the partisan-dispute note verbatim; the 17:25 dispatch is its own moment (the day's second hard clock) | CA-J1P-7/8 (ED-68(b) envelope; ED-27 verifications) |
| The consolidation | Wainwright's East Cemetery Hill layout authored battery-by-battery from the verbatim roster (Stewart across the pike / Wiedrich / Cooper / Reynolds; Stevens 50 yards forward on the knoll); Cutler toward Culp's right with the 7th Ind first-occupation pin cited; Meredith on Culp's; Baxter's stated 5-o'clock forward-left move; Orland Smith's garrison line as the anchor the corps re-formed on | or-27-1-wainwright p. 357; or-27-1-cutler; or-27-1-baxter; or-27-1-vonsteinwehr |
| Documented stillness | Thomas's reserve (the division records' negative space); Smith's brigade missing the fight on the York-road flank ("twice moved forward toward the town" and back — ED-44 class); Archer's brigade sitting out the renewal; Marshall/Brockenbrough waiting through the morning | the executor-first doctrine's negative-evidence layer |

## 4. What was cut or deferred (honesty ledger)

1. **The July 1 evening (18:00–dark) = an honest note, not a phase
   file.** CA-J1P-9 is sequence-only ("if practicable" — correctly
   unclocked in the chain); the XII/III arrival columns and Geary's
   evening move toward the Round Tops have arrival notes, not
   executor chains. Authoring them would be invention at track grain.
2. **Heckman's Battery K, 1st Ohio is NOT cast** (the XI Corps'
   fourth battery, engaged covering the retreat, two guns lost): no
   dossier read is banked and no register-grade position exists —
   the Gregg-in-slice-2 cut class. Worklist item.
3. **The corps/army command rows stay uncast** (Reynolds I-HQ,
   Howard XI-HQ, Hancock): their records are carried as moments and
   keyframe citations. Casting command markers is a display-model
   decision the owner should gate (no phase file has HQ units today).
4. **Buford's picket line at the first shot** rides the Gamble t=0
   citation (the 07:30 shot fell ~3 miles west of the authored
   line); the vedette screen is not a separate track.
5. **The Iverson wounded-cell adoption** follows ED-71 exactly: the
   printed 328 carried, the 382 reconstruction used only where the
   aggregate needs it; the authored decay uses the aggregate-consistent
   spike (814 total loss on the row's tablet-agreeing 820 class).
6. **In-HUD phase switching stays deferred** (ADR 0005): five
   reconstructed phases now wait on the BattleDirector re-init UI;
   this slice ships only manifest phases + per-phase moments +
   `-battleFile` capture-path runs (the slice-2 mechanism, unchanged).
7. **Per-day loss ledgers on the unit** (A2 §6.4) — still not taken
   (battle-format change), and July 1 sharpens the need: this slice
   carries three clean per-day primaries (Coulter's table, Hays's
   table, the 41st NY row) in citation text only.

## 5. Runtime/format changes riding the slice

- **Manifest**: July 1 = two abutting reconstructed phases + honest
  evening note; echo tests extended on BOTH sides (tool
  `manifest.test.ts` imports both files and asserts the lull-seam
  abutment; `PhaseManifestTests.CommittedManifest…` parses both files,
  asserts the abutment and per-file clock echo; the manifest-lying
  mutation test retargeted to july2-morning, the remaining
  not-reconstructed-with-battle candidate).
- **Per-phase moments**: `moments-gettysburg-july1-morning.json` /
  `moments-gettysburg-july1-afternoon.json`, battle-gated; the
  committed-moments Unity test extended to all four per-phase files
  (renamed `CommittedPhaseMomentsFiles_ParseAndGateToTheirPhases`).
- **Command overlay**: generator scans all five phase files (204
  mapped, 0 unmapped); Unity coverage test extended.
- **Register**: three castStatus updates (Gamble/Devin/Calef); md
  counts 154 → 157 in-build.
- **Capture harness**: `DayExpansion3CaptureHarness` (+
  `scripts/dayexp3-captures.sh`), the slice-2 mold with morning /
  afternoon shot sets and the July 1 day panel.

## 6. July-2/3 + bundle safety (the untouched proof)

- The branch merged current `main` (the sibling `ed75-placeholder`
  work — Marshall/Davis provisional strengths — landed mid-slice);
  after the merge, `git diff main --name-only` lists ONLY this
  slice's files: the two July 1 battle files + their derivation
  scripts, the manifest, the two moments files, the register
  json/md, the overlay json + generator, the three test files, the
  harness + capture script, and this report.
- `gettysburg-july3.json`, `gettysburg-july2-afternoon.json`,
  `gettysburg-july2-evening.json`, and `app/Assets/Battle/Angle/`
  are byte-identical to main (absent from the diff); July 3 sha256
  `c392bf22…` (the ED-75 owner-merged state, not this slice's edit).
- **The ED-21 stagingSeed pin HELD: `d470c469…`** (read from the
  committed bundle; nothing in its input closure changed here — the
  Marshall/Davis dossiers and the ED doc belong to the sibling and
  were not touched by this slice, per the coordination rule; ED
  numbering from ED-76 upward was reserved but NOT needed — this
  slice adopts existing rulings and proposes none).

## 7. Residuals (the worklist)

1. **Cross-file strength-continuity flags (the July-1-night vs
   July-2-t0 class)**: this slice adopts the evidence layer, so
   July-1-end values differ from the July 2 files' t=0 (which
   inherited July-3/compilation bases) on 26 of 42 shared rows.
   SEVEN land EXACT (Hays 1,137 — his own July 1 table's
   subtraction; Lowrance 500 — his documented waypoint; Baxter 800;
   Dana 465; Wiedrich 141; Jones's and McIntosh's battalions): the
   class's proof it converges where per-day primaries exist. The
   big divergences are structural, both directions: Ramseur 930 vs
   1,715 (the June-30 division return, found pass 11, sits far BELOW
   the compilation the July 2/3 files carry); Daniel 1,650 vs 1,185
   and Davis 1,200 / Marshall 1,000 vs the 2,000 'first day' figures
   the later files hold statically (their July-1 losses were never
   netted); von Gilsa 400 vs 605 (the 41st NY rejoins ~22:00, after
   this window); Coulter 761 vs 510; Harris 700 vs 565. Each is the
   known evidence-layer-vs-build class (ED-32/ED-46 pattern; slice-2
   §7.2's mirror) — enumerated in full by the afternoon script's
   continuity ledger, flagged for a reconciliation pass, never
   silently edited. NOTE the days do not abut (the July 1 evening
   and July 2 morning are honest notes), so no validator rule is
   violated.
2. **Heckman's battery + the XI artillery brigade grain** (cut 2).
3. **In-HUD phase switching** (ADR 0005 residual, FIVE phases now
   waiting).
4. **The EC4 physics-timing program** (passes 10-11 flag) remains
   un-executed as a formal computation; this slice's approach legs
   are hand-paced tier-B against documented endpoints — the program
   would sharpen them to ±2-3 min on the pike marches.
5. **The 16th Maine / 147th NY / 6th Wisconsin decomposition
   candidates**: three regiments whose July 1 records are
   regiment-grain episodes (the sacrifice hill, the stranded stand,
   the cut charge — the last already a segment event); the Harrow
   precedent (slice 2) applies.
6. **Barlow's letters** (the forward-tilt agency question) — still
   the cheapest way to both-side the day's most consequential Union
   decision (dossier flag carried).
7. **Devin/von Gilsa/Ames/Meredith-brigade EC2 primaries** — the
   day's honestly-open strength rows.

## 8. Suites and evidence

Suites (all green on the branch):
tool vitest **118** · pipeline pytest **59** · reconstruction pytest
**122 + 1 skipped** · Unity EditMode **377 passed, 0 failed, 4
skipped** (the slice-2 baseline shape; the manifest echo, overlay
coverage, and moments gate tests extended to all five phase files —
the overlay corps-count floor made per-file, since July 1 is honestly
a five-corps meeting engagement) · Unity PlayMode **17 passed**
(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`,
worktree Library, gitignored inputs restored; logs
`editmode2.log`/`playmode.log` + results XML in the worktree,
gitignored by design).

Perf: PENDING (`dayexp3-afternoon-benchmark.json`).

Evidence: `docs/benchmarks/captures/day-expansion-3/` (force-added;
owner copies in the main checkout's same gitignored path) — PENDING.

## 9. DAY-EXPANSION COMPLETION — the three-day battle

With this slice the Gettysburg reconstruction covers **all three
days** under one manifest:

| Day | Phases | Window | Units |
|---|---|---|---|
| July 1 | morning + afternoon (+ honest evening note) | 07:30–18:00 | 32 / 45 |
| July 2 | honest morning note + afternoon + evening | 15:30–22:30 | 152 / 152 |
| July 3 | honest morning note + afternoon | 13:00–19:29 | 191 |

**What the full battle now contains:** five reconstructed phases,
~28.5 authored hours across 63 clock-anchored chain rows (CA-J1M-1…7,
CA-J1P-1…9, CA-J2A-1…11, CA-J2E-1…5, CA-J3M/J3A, CA-ECF/SCF pending),
every combat formation of both armies standing on an audited dossier,
three timestamped document pins (10:10, 17:25, 5.25), and the
honesty apparatus (per-phase moments, conflict records, flagged
reconstructions) carried uniformly.

**What the decomposition wave should take first:**
1. The July 1 regiment-grain exemplars (§7.5) — the 6th Wisconsin
   already fights as a segment event and is the natural first
   promotion; the 147th NY and 16th Maine episodes are single-source
   clean.
2. The slice-2 carryovers (21st Mississippi; the 9th MA's
   two-section Trostle structure).
3. Iverson's field as the first T4→T5 Soldier-View candidate (the
   geometry is pinned: formation tablet, drawn mid-advance bar,
   receiving wall, death line — the passes 10-11 recommendation
   stands, now with the phase file to host it).

**The remaining honest gaps (in manifest note form):** July 1
evening (sequence-only chain), July 2 morning/midday (no CA-J2M
chain), July 3 morning (the standing Pfanz gate ED-30/ED-64 — still
the one unresolved precondition in the chain apparatus), and the
cavalry fields (ED-72 chains adopted; ECF/SCF authoring is its own
slice, off the main crop). The morning-phase gaps are dossier-thin,
not dossier-empty — each note names what exists and why it is not
yet track-grade.
