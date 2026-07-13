# Day expansion slice 2 — the July 2 authoring day

**Branch:** `day-expansion-2` (unmerged; owner gate) ·
**Scripts:** `tool/scripts/author-dayexp2-afternoon.ts`,
`tool/scripts/author-dayexp2-evening.ts` (committed deterministic
derivation records, the `author-w*`/A1/A2/dayexp1 pattern) ·
**ADR:** 0005 (exercised, not amended) · 2026-07-13

The first entirely new reconstructed day: July 2 authored from the
dossier corpus's deepest bank (passes 5–8 — the en-echelon assault,
Little Round Top, the Wheatfield's five waves, the Peach Orchard
collapse, Plum Run, and the evening attacks on Culp's Hill and East
Cemetery Hill), as two phase battle files under the ADR-0005 manifest.
The July 3 file and the shipped bundle are UNTOUCHED (§6).

## 1. The phase split (the design call)

**Chosen: two reconstructed phases abutting at the sunset pin, plus an
honest morning note.**

| Phase | Clock | File |
|---|---|---|
| `july2-morning` | — | `not-reconstructed` note (no CA-J2M chain exists; arrival marches sit in the dossiers as notes, not anchors — nothing shown rather than something invented) |
| `july2-afternoon` | **15:30–19:29 LMT** (startTime 55800, endTime 14340) | `gettysburg-july2-afternoon.json` |
| `july2-evening` | **19:29–22:30 LMT** (startTime 70140, endTime 10860) | `gettysburg-july2-evening.json` |

Rationale for the seam: battle-manifest.md's honesty rules forbid
overlapping reconstructed phases within a day, so the charter's
"overlapping-or-abutting" call resolves to ABUTTING, and the best seam
in the evidence is **sunset 19:29 LMT — the day's only astronomically
pinned clock (ED-31), which is simultaneously CA-J2A-11 (Wofford
"fell back at sunset"), the afternoon ladder's terminal event**. The
cost is that CA-J2E-1/2 (the XII Corps departure 18:30; Johnson's
first strike ~19:00) fall in the afternoon file — accepted because
both are executor-clocked primaries that the afternoon file carries as
cited content, and the evening file re-expresses their 19:29 states at
t=0. The alternative seam (19:00) was rejected: it would clip wave 5,
the Plum Run climax, and CA-J2A-11 itself out of the afternoon ladder.
The evening window's 22:30 end carries CA-J2E-5's adopted envelope
(three ~22:00 primaries; Nicholls's ~23:00 reading recorded, not
adopted — ED-62).

**Cross-phase continuity is structural:** the evening script reads the
afternoon file and starts every unit at its cited 19:29 end state
(verified numerically: parentless strength total 103,057 at the
afternoon's last keyframe = 103,057 at the evening's first).

## 2. The cast and authoring summary

Both files carry the same 152-unit cast (102 US / 50 CSA; 62
artillery-class units; 4 regiment children):

| | Afternoon | Evening |
|---|---|---|
| Units | 152 | 152 |
| Movers (position changes) | 75 | 30 |
| Attested statics / T1 endpoints | 77 | 122 |
| Keyframes | 447 (321 documented / 126 inferred) | 349 (125 documented / 224 inferred-continuity) |
| Events (fire windows) | 66 | 14 |
| Strength decay (parentless sum) | 14,173 | 1,355 |

- **Movers** carry the dossier arcs of passes 5–8 at brigade/battery
  grain: Hood's four (Law under `csa-sheffield`, Robertson, G. T.
  Anderson, Benning), McLaws's four (Kershaw, Semmes, Barksdale,
  Wofford), Anderson's echelon (Wilcox, Lang, Wright, Posey; Mahone's
  documented non-advance stays still per ED-44), Latimer's Benner's
  Hill duel (`csa-bn-raine`), Johnson's three + Walker's detention,
  Hays/Avery + Gordon's halted support, and on the Union side
  Vincent/Weed/Hazlett, Barnes's and Ayres's four, Caldwell's four,
  Crawford's two, Nevin + the VI Corps arrival columns, the III Corps
  salient (Graham, Ward, de Trobriand, Carr, Brewster, Burling's
  dispersed-ledger centroid), Willard, Stone's double-quick, the
  XII Corps departure/returns, Carroll's night charge, and twelve
  batteries with attested legs (Bigelow's prolonge-and-stand the
  exemplar).
- **Harrow's brigade is fully decomposed for July 2** (the record is
  regiment-grain): the 15th MA/82nd NY Codori pair, the 1st Minnesota's
  charge (both strength readings carried, un-averaged), the 19th ME
  ridge line.
- **Six batteries newly cast** (register castStatus updated; overlay
  regenerated, 201 mapped / 0 unmapped): Clark (B/1st NJ), Bucklyn
  (E/1st RI), Winslow (D/1st NY), Smith's 4th NY Independent, Watson
  (I/5th US), Bigelow (9th MA).
- **T1 tier:** units present July 2 without dossier anchors hold
  window endpoints at their attested/July-3 ground, confidence
  `inferred`, with the honesty note in the citation text (e.g. the
  Artillery Reserve park cluster, Heth's resting division, the
  Cemetery Hill XI Corps line).
- **Strength discipline:** t=0 strengths are the dossiers' EC2 adopted
  bases (primaries where they exist: Cross's 780 muskets, Kelly's 530,
  Wilcox's 1,777, Greene's 1,350, Rice's ~1,000-muskets carried
  against the compilation, Burbank's <900); decays are the EC6
  apportionments (ED-49/52 scope rules; Hays's per-day table the one
  per-day primary). Where the evidence layer's adopted basis differs
  from the July 3 file's build value, THIS file adopts the evidence
  layer and the difference is a recorded residual (§7.2), never a
  silent edit of the July 3 file.

## 3. Cited highlights (the chains as authored)

| Content | As authored | Basis |
|---|---|---|
| The en-echelon ladder | Artillery opens t=900 (~15:45, two-pole with Jacobs's 16:20 carried) → Hood steps off t=3600 (**~16:30**, inferred confidence at the adopted envelope time) → Devil's Den t=7200 (~17:30) → Kershaw t=7200 signal/t=8100 strike → Barksdale t=9900 (**18:15**) → Wilcox/Lang ~18:25 → Wright ~18:45 | ED-53 (CA-J2A-2/3 as revised); CA-J2A-5/7/9 downstream-CONFIRMED; Alexander's interval arithmetic; Kershaw's drums pin |
| The five waves (ED-57) | W1 de Trobriand vs G. T. Anderson (17:00–17:45) · W2 Kershaw+Semmes vs Tilton/Sweitzer (17:45–18:15, ED-59 ordered withdrawal — never routed) · W3 Caldwell's four t=9000+ (CA-J2A-8 CONFIRMED) · W4 Wofford's sweep + the renewal (18:30–19:15; Sweitzer's second advance; the regulars' extraction) · W5 McCandless/Nevin to the wall (19:12–sunset) — **wave boundaries expressed at adopted envelope times with inferred confidence; the seams are triggers, and every wave keyframe carries both sides' citations** | ED-57 verbatim; pass-6 §3.1; CA-J2A-11 as the file's endTime |
| The Smith's-guns hands-change | ONE change of hands at t=7200; Smith's battery falls back to its drawn rear-section gully and fights on; Benning holds the Den through both files | ED-56 (or-27-1-smith-4ny fixes 3 × 10-pdr Parrott; no exclusive credit encodable) |
| Wright's high-water | Farthest keyframe (4289, 4852) at t=13500 — the gun line, NEVER the crest; Brown's/Weir's temporary losses rendered without materiel discontinuity | ED-60 (marker 4330.6, 4746.9; j2-04 28 m agreement; the tablet-inherits-report caveat in the citation) |
| The Trostle ground | Bigelow: Wheatfield Rd (mon-9ma-first) → prolonge retire ("300 yards" = 323 m in the marker frame) → the angle stand → four guns lost (Milton's 27+1=28 exact decay); Watson's four guns abandoned → Peeples/39th NY ground-retaken → ALL EIGHT hauled off by ~21:00 (evening file) | ED-61 (the gate primaries: Milton No. 320, McGilvery pp. 881-884, Martin p. 660); the Crawford July 3 Napoleon residue carried |
| The evening chain | Afternoon file carries E-1 (18:30 departure, Greene's primary) and E-2 (19:00 strike, the Greene=Nicholls minute-class pair); evening file: Steuart's vacated-works lodgment HOLDS (~20:00), Hays/Avery step t=1020 (19:45–20:00, three-source both-armies + Early's trigger), lodgment t≈1860–2460 (drawn bar 21 m from the Wiedrich monument), Carroll clears t=3660 (~20:30 kept as SEQUENCE anchor, inferred), dies down by t=9060 (~22:00) | ED-29 as hardened by ED-62 (no times moved); CA-J2E-1..5 |
| The battery possession | Wiedrich/Ricketts render temporary possession + hand-to-hand + SAME-NIGHT resumption; Ricketts's left piece spiked in place; no captured-battery state anywhere | ED-63 |
| The clock-skew discipline | Kershaw's "4 o'clock", Ward's tablet "between 4 and 5", Einsiedel's "6.30" etc. are CARRIED in citations as profiled early readings; no report-nominal clock moved any keyframe | ED-55/ED-58 (rule 4 held throughout) |
| Documented stillness | Mahone ("did not advance"), Walker's Hanover-road detention (the three-brigade composition fact), Gordon's halted support, the Bliss/Long Lane all-window skirmish emitters | ED-44; ED-62; tablets verbatim |

## 4. What was cut or deferred (honesty ledger)

1. **July 2 morning/midday = an honest note, not a phase file.** No
   CA-J2M chain exists; the countermarch and Sickles's forward move
   would be invention at track grain today.
2. **Gregg's Union cavalry at Brinkerhoff's Ridge is NOT cast** (no
   dossier read banked for Gregg's division); Walker's detention —
   the fact that matters to the evening chain — is carried on HIS
   record (ED-62). Slice-3+ worklist item.
3. **Pickett's division, Dearing's battalion, and the July-3 cavalry
   fields are excluded** (arrivals/bivouacs outside the window or off
   the crop) — exclusion, never a faked position.
4. **Rodes's aborted evening advance is not authored as motion**: the
   five brigades render at their register/tablet grounds (Long Lane
   day-scope for Doles/Iverson/Ramseur); the advance-and-halt narrative
   needs its own dossier read (slice-3 July 1/Rodes bank).
5. **The 21st Mississippi's split from Barksdale's centroid** rides
   the citation (the j2-04 two-element drawn state), not a second
   track — single-centroid discipline kept; a future decomposition
   wave may promote it.
6. **In-HUD phase switching stays deferred** (ADR 0005): this slice
   ships the `-battleFile` override (capture/evidence path), per-phase
   moments files with battle gating, and the manifest phases; the
   BattleDirector re-init UI is the standing residual, now with THREE
   reconstructed phases waiting on it.
7. **Per-day loss ledgers on the unit** (A2 §6.4) — still not taken
   (battle-format change).

## 5. Runtime/format changes riding the slice

- **Manifest**: July 2 = honest morning + two abutting reconstructed
  phases; echo tests extended on BOTH sides (tool `manifest.test.ts`
  imports both files; `PhaseManifestTests.CommittedManifest…` parses
  both files and asserts the abutment).
- **`-battleFile` override** (BattleDirector, static so the HUD's
  moments lookup agrees): loads a battle JSON from disk;
  `BattleAssetName` follows it, so the day panel lights the loaded
  phase. SoldierView entry is disabled under an override (the shipped
  viewpoints address the July 3 afternoon clock and cast).
- **Per-phase moments**: `MomentSet.battle` gate + quiet probe for
  `Atlas/moments-<battleAsset>.json`; `moments.json` gated to
  `gettysburg-july3`; cited July 2 moments files for both phases.
- **Command overlay** generator scans every phase file (201 mapped,
  0 unmapped); coverage test extended to all three files.
- **Register**: six castStatus updates + md counts (148 → 154
  in-build).

## 6. July-3/bundle safety (the untouched proof)

- `app/Assets/Battle/gettysburg-july3.json` **byte-identical** to
  main (sha256 `9a4c693bc9cc885c07d011f4a9a944d0c3ef4be19c5a6a61676e831c476e48d0`
  both sides; `git diff main -- app/Assets/Battle/gettysburg-july3.json`
  empty).
- No file under `reconstruction/` claims/canonical/sources was touched;
  the angle bundle was not recompiled — **the ED-21 stagingSeed pin
  (d470c469…) is unaffected by construction** (nothing in its input
  closure changed; `test_committed_bundle_matches_recompilation` green
  in the suite run).
- `moments.json` gained ONLY the additive `battle` field (its times
  and citations byte-unchanged); the shipped film's media, viewpoints,
  and captions are untouched.

## 7. Residuals (slice 3+ worklist)

1. **July 1 (slice 3) needs**: the pass-10/11 dossier bank (railroad
   cut, Oak Ridge, Barlow Knoll, the Seminary Ridge collapse, the town
   retreat) is already executor-grade; a CA-J1M/J1P phase split
   (morning 07:30–13:30?, afternoon 13:30–~18:30?) with ED-66's
   deploy-vs-attack split as the morning's spine; Rodes/Early July 1
   arcs feed the July 2 t=0 statics authored here (Long Lane etc.);
   Gregg/Brinkerhoff and Rodes's evening advance (this slice's cuts 2
   and 4); Buford's cavalry as the morning's executor mass.
2. **Cross-file strength reconciliation flags**: this slice adopts the
   evidence layer's EC2/EC6 bases, so a handful of July-2-end values
   differ from the July 3 file's t=0 build values (exemplars: Cross
   450 vs us-mckeen 520; Vincent 984 vs us-rice 985 [agrees]; Willard
   1,290 vs us-sherrill 1,510; Wright 582 vs csa-wright 580). Each is
   the known evidence-layer-vs-build class (ED-32/ED-46 pattern) —
   flagged for a July 3 reconciliation pass, never silently edited
   here.
3. **In-HUD phase switching** (ADR 0005 residual, third phase now
   waiting): BattleDirector re-init from a manifest phase selection;
   per-phase command overlay is already unified (one overlay covers
   all files).
4. **The Wheatfield/Trostle decompositions**: 21st MS (Barksdale) and
   the 9th MA's two-section structure are drawn/documented at
   sub-brigade grain and could be promoted the way Harrow was this
   slice.
5. **VI Corps arrival legs are coarse** (inferred staging on the
   Baltimore Pike): a Sedgwick-corps dossier read would pin the
   arrival ladder (only Nevin's 34-mile primary is banked).
6. **Johnson's/Early's brigade strengths** remain tablet-hop class
   (the pass-8 systematic gap): the B&M-class cheap fetch stands.
7. **Benner's Hill Union counter-battery grain**: the duel's Union
   windows ride Geary's/battery grounds; Muhlenberg's brigade-grain
   expenditure rows would sharpen EC5 windows.

## 8. Suites and evidence

Suites (all green on the branch; details in the final report):
tool vitest 118 · pipeline pytest 59 · reconstruction pytest 122+1s ·
Unity EditMode 375+4s (extended: manifest echo ×3 files, overlay
coverage ×3 files, moments battle-gate + committed July 2 files) ·
Unity PlayMode 17.

Evidence: `docs/benchmarks/captures/day-expansion-2/` (force-added;
owner copies in the main checkout's same gitignored path) — the
afternoon phase's timeline/en-echelon/LRT/Wheatfield-W3/Barksdale/
Plum Run shots, the evening phase's ECH/Culp's Hill/returns shots,
the July 2 day panel with the loaded phase lit, and the perf
benchmark JSON. Produced by `scripts/dayexp2-captures.sh` (one
Development build, two `-battleFile` runs).
