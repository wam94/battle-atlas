# Residuals + decomposition 2 — Smith placeholder, Colgrove split parity, the wave-1 carryovers, O'Neal's third flag

**Branch:** `residuals-decomp-2` (unmerged; coordinator gate) · direct
battle-file edits for the strength-only row (task 1, matching
`strength-reconciliation-1`/`-2`'s precedent for this exact class of pass)
plus two new deterministic authoring scripts (`tool/scripts/author-
residuals-decomp2-carryovers.ts` for task 3, `tool/scripts/author-
residuals-decomp2-colgrove.ts` for task 2) · 2026-07-16

Overnight unattended slice, smallest-risk-first: (1) the `csa-smith`
placeholder re-derivation `strength-reconciliation-2.md` §4 named and left
open, (2) Colgrove split parity on the Angle-pinned film file, (3) the
`decomposition-wave-1.md` §7 honest deferrals (21st Mississippi, the 9th MA
battery), (4) `csa-oneal`'s documentation-only flag. Work order followed
risk, not the brief's own 1-2-3-4 numbering: task 1 (small, contained,
non-film) → task 4 (doc-only) → task 3 (non-film, new units) → task 2
(the pinned file, tripwire treatment) last.

## Task 1 — `csa-smith`'s July-2 strength placeholder, re-derived

**Files:** `gettysburg-july2-afternoon.json`, `gettysburg-july2-evening.json`
(direct edits, no new script — the established convention for a pure
strength-value reconciliation pass, per `strength-reconciliation-1`/`-2`'s
own header).

**Basis:** `strength-reconciliation-2.md` §4's preserved refinement note:
*"re-deriving `gettysburg-july2-evening.json`'s `csa-smith` placeholder from
the tablet's ~800 PRESENT figure directly, rather than its post-loss ~660,
so the morning phase needs no inherited pre-netting at all — remains open."*
The dossier (`reconstruction/dossiers/csa-2c-ear-2-smith.md`) gives the
exact basis: tablet **"Present about 800"** (EC2), and the brigade's
**only** documented combat is the July 3 Culp's Hill dislodgment action
(report No. 476's own itemized total, 142 k/w/m EXACT against the tablet's
"Total 142", EC6) — Smith's brigade spent July 1-2 on the York Pike flank
guard, **no combat, verified negative** (EC3/EC5). The old 660 placeholder
was computed *before* the morning phase existed as "tablet ~800 minus the
FULL 142-loss return" — netting a July-3 loss into a July-1/2 static
placeholder, backwards double-counting at the root (`july3-morning-
slice.md` §5.2 first named this; `strength-reconciliation-2` fixed the
morning phase's own *second* netting on top of it but left this, the
*first* netting, in place).

**Fix:** both keyframes in both files, 660 → **800**, adopted directly,
approximation flagged in the citation text (matching the corpus's own
"tablet Present [X]... approximation flagged" convention for this class of
row). No script — direct edit, per the `strength-reconciliation-1`/`-2`
precedent for strength-only rows.

**Bonus closure, found while re-deriving:** `gettysburg-july1-afternoon.json`'s
own `csa-smith` keyframe *already reads 800* (shipped, untouched by this
pass) — `strength-reconciliation-1.md` §2.2 had flagged exactly this
Δ140 (800 vs 660) as a *"stale compilation-drift artifact... flagged for a
future harmonization pass"* at the July-1/July-2 boundary. This pass's fix
closes that flag as a side effect: July-1-afternoon (800) now agrees
EXACTLY with July-2-afternoon/evening (800, this pass) — Δ0, no separate
edit needed for that boundary.

**Refinement note (preserved, not closed):** propagating the fix into
`gettysburg-july3-morning.json`'s inherited t=0 (still 660) and
`gettysburg-july3.json`'s own t=0 (still 660, previously Δ0 with the
morning-end per `strength-reconciliation-2`) is **out of this task's small,
contained scope** — deliberately not touched, per the task brief's own
"small, contained" framing and to keep task 1 clear of the film file
(task 2's job, with its own tripwire). This **creates a new, honestly
named boundary residual**: July-2-evening EndT (800) vs July-3-morning t0
(660), Δ140 — the SAME evidence-layer-vs-inherited-placeholder class
`strength-reconciliation-1`/`-2` catalogue elsewhere, now flagged a third
time for this exact unit. Propagating the fix through the morning phase's
own decay arithmetic (800 − 142 = 658, replacing the currently-inherited
pre-netted 660, and updating `gettysburg-july3.json`'s own t=0 to match) is
the recommended next pickup — see Owner Questions below.

No test asserts `csa-smith`'s numeric strength (`tool/tests/gettysburg.test.ts`
line 466 only lists the unit id in a membership set); no test change needed.

## Task 2 — Colgrove split parity (`gettysburg-july3.json`, the FILM FILE)

**Script:** `tool/scripts/author-residuals-decomp2-colgrove.ts`.

**Basis:** `strength-reconciliation-2.md` §13 owner question 3: *"worth a
small decomposition-wave-style pass on the afternoon file specifically for
this unit?"* This file's own `us-colgrove` already reads the correct
FAMILY-TOTAL strength (891 = `csa-colgrove` 482 + `us-2ma` 180 + `us-27in`
229, per that same pass's §1.2 composition note) but renders it as ONE
undecomposed unit, while `gettysburg-july3-morning.json` already carries
the ED-76 three-way split. This pass closes that composition gap: **no
strength value changes** — it only splits the unit count to match.

**Decomposition (ED-76, parentless siblings):** `us-colgrove` renamed
"Colgrove's Brigade (3rd Bde, 1st Div, XII Corps — minus the 2nd
Massachusetts and 27th Indiana)", strength 482 (both keyframes); `us-2ma`
and `us-27in` promoted as new parentless units at 180 and 229. Position for
all three: the morning file's own EndT keyframes, inherited directly (no
new geometry invented) — `us-colgrove` residual (6080,4800,f30), `us-2ma`
(6096,4865,f30), `us-27in` (6120,4885,f30). This file's own `us-colgrove`
keyframes are static (t=0/t=10800, "documented silence" — the corps' fight
ended pre-window at 10:30 a.m.); the split units are likewise held flat.

**Conservation:** 482 + 180 + 229 = 891 = the pre-decomposition value,
exact, at both shared keyframes (script-asserted, printed at run time —
see the script's own conservation check, which throws on any mismatch).

### Film-safety tripwire — VERDICT: FILM-SAFE

`us-colgrove`/`us-2ma`/`us-27in` are **NOT** among the 13 Angle-cast units
(`csa-garnett`, `csa-kemper`, `csa-armistead`, `csa-fry`, `us-webb`,
`us-69pa`, `us-71pa`, `us-72pa`, `us-btty-cushing`, `us-btty-cowan`,
`us-btty-arnold`, `us-hall`, `us-stannard`) — verified by direct membership
check inside the authoring script itself (aborts loudly if ever pointed at
one) and independently outside it (below). Verified three ways, matching
`decomposition-wave-1`/`strength-reconciliation-1`/`-2`'s own methodology:

1. **Angle-cast unit byte identity.** The 13 units, extracted (sorted-key
   JSON) from this worktree's `gettysburg-july3.json` and from
   `origin/main`'s tip **after this pass merged in `angle-facing-
   adjudication`** (`c8aa934`, §Post-merge below): **sha256
   `032152356b7c35707e44711ffa28f7cdf744e18446d1c2e7fb9d7128029c379c`** —
   MATCH, byte-identical, both before this pass's edit and after (checked
   pre- and post-edit, and again pre- and post-merge).
2. **Compiled bundle payload identity.** `reconstruction/scripts/
   compile_angle.py` re-run after the edit: every field (`units`,
   `claimsIndex`, `slice`, `format`, `note`, `clock`) byte-identical to the
   pre-edit bundle EXCEPT `inputs.battle` (the whole-file sha256 of
   `gettysburg-july3.json`, expected — non-Angle-cast units in that file
   changed: `e212b670...` → `1f57b8c7...`) and the top-level self-referential
   `checksum` (`ffb6b59a...` → `47c6f183...`). **`stagingSeed` HELD:
   `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`.**
3. **Structural containment.** `us-colgrove`/`us-2ma`/`us-27in`'s only
   keyframes in this file sit at t=0/t=10800 (13:00/16:00 LMT) — well
   outside the compiler's t=8040..9000 read window — AND none of the three
   is Angle-cast, so the compiler never reads them regardless of time.

## Task 3 — the `decomposition-wave-1.md` §7 carryovers

**Script:** `tool/scripts/author-residuals-decomp2-carryovers.ts`.
**Files:** `gettysburg-july2-afternoon.json`, `gettysburg-july2-evening.json`
(NOT the film file — no tripwire needed for this task).

### 3a. The 21st Mississippi — AUTHORED (ED-76)

**Basis:** `reconstruction/dossiers/csa-1c-mcl-3-barksdale.md` EC3.3: the
brigade's own drawn bachelder sheet (j2-04, 19:00) puts *"'21' MISS' drawn
separately at the Trostle ground (3617,3643)... over 'BIGELOW'"* — a real
divergence-coordinate primary, matching the tablet's own narrative split
("the regiments inclining left to Plum Run except the 21st MS"). EC6: the
ANV return's own regiment row, 21st MS **103** k+w (or-27-2-anv-return
pp. 338-339) — a real per-regiment casualty primary, missing not broken
out (the k+w floor, not invented above it).

**Strength grain:** no regiment-level STARTING-strength primary exists for
any of Barksdale's four regiments (EC2: *"No primary strength statement
(negative; no report exists)"* — only the brigade-wide tablet "Present
1,598"). Per the SAME compilation-class even-split technique
`decomposition-wave-1` used for the 6th Wisconsin (dossier `us-i-1-1-
meredith.md` Conflicts item 4), the 21st's starting share is 1,598 / 4 =
399.5, rounded to **400**, FLAGGED compilation-class, held FLAT through
the shared pre-divergence fighting (the Peach Orchard breach — no
regiment-grain attribution exists for that shared loss, so the residual
absorbs it by exact subtraction, not an invented per-regiment split). At
the Trostle capture — **t=12900, cross-referenced to `us-btty-bigelow`'s
own pre-existing keyframe at the SAME timestamp, the same real-world
moment** ("THE 21ST MS OVERRUNS THE ANGLE: FOUR of six light 12-pounders
lost... Milton's section of two retires") — the 21st's own primary loss
applies: 400 − 103 = **297**.

**Conservation:** the residual brigade ("Barksdale's Brigade, McLaws's
Division — minus the 21st Mississippi") absorbs the 21st's current value
by EXACT subtraction at every one of Barksdale's own six original
keyframe times; the script asserts (and prints) the sum equals the
pre-decomposition value at every one — 1598, 1598, 1450, 1250, 1000, 851 —
exact, no tolerance band. Cross-phase continuity (afternoon end → evening
t=0) asserted for both the 21st and the residual: exact match, both
position and strength.

**Honest gap, carried, not invented:** no citation attests the 21st
Mississippi specifically rejoining the brigade's night reconsolidation —
the brigade-wide "re-formed... under Humphreys" (evening file) is a
BRIGADE fact, not regiment-specific. The 21st holds at its last attested
ground (Trostle) through both files' end, matching the corpus's own
honesty convention for this class of gap (`decomposition-wave-1`'s 6th
Wisconsin between its crest re-advance and the Culp's Hill rendezvous is
the same class).

### 3b. The 9th Massachusetts Battery's two-section Trostle structure — DEFERRED

**Not authored.** The dossier (`reconstruction/dossiers/us-btty-
bigelow.md`) attests the SPLIT in outcome — Milton's own account, *"Having
succeeded in retiring my section, I found myself in command of the
battery"*; the existing single-track `us-btty-bigelow` citation already
narrates *"Milton's section of two retires"* against *"FOUR of six light
12-pounders lost"* — but does **not** attest a distinct **position**
anchor for the two sections:

1. **No EC3 gap the corpus's own precedent would cover.** The 16th Maine
   precedent (`decomposition-wave-1`) authored through an EC3 gap by
   placing the unit at a wide-open radius on a SIBLING unit's own already-
   drawn coordinate (Coulter's division retreat-corridor ground). No
   equivalent sibling anchor exists here: Milton's own retirement
   direction ("McGilvery's new [Plum Run] line") is a named DESTINATION
   but not a coordinate, and the existing `us-btty-bigelow` single track's
   own t=14340 keyframe ("the remnant behind the Plum Run line at sunset",
   4100,3800) is **already** effectively Milton's/the survivors' own path
   — splitting the track would relabel that geometry, not add any.
2. **ED-61 is a live guard against exactly this move.** The existing
   `us-btty-bigelow` t=12900 citation reads: *"ED-61: ONE loss, a recovery
   ledger, never per-claimant duplicate guns."* ED-61 was adopted
   specifically to prevent double-booking this battery's four lost/four-
   recovered guns across multiple claimant records. A second, independently
   -tracked "captured section" unit — whose guns are ALREADY the subject of
   an existing recovery keyframe elsewhere in the corpus (`gettysburg-
   july2-evening.json`: *"Dow procured the infantry detail that brought
   off the four 9th MA guns the same night"*) — risks exactly that
   double-count for no new evidentiary gain (the strength split available,
   2-of-6 vs 4-of-6 guns, is proportional/compilation-class only; no
   primary attests it either).

Authoring this split would be a stretch past what the dossier supports;
the honest call is to defer it, name the reason, and leave it for a future
pass if a Milton-retirement-endpoint primary or a section-grain strength
primary is ever fetched.

## Task 4 — `csa-oneal`: documentation only, no value changes

**File:** `docs/reconstruction/audit/dossier-overlay.json` (the master
table's consultation layer — `reconstruction/scripts/build_unit_audit.py`
applies this over the workbook on every regeneration, so it is the correct
place to make a persistent, regenerable note, not a one-off xlsx edit).

Per `strength-reconciliation-2.md` §3/§13 owner question 1: `csa-oneal`'s
thin EC6 margin — the return's 696 total, minus the already-established
July-1 loss share, leaves only ~2 men of arithmetic headroom for a brigade
attested *"held for three hours, exposed to a murderous fire"* — is now
flagged at its **second** file boundary (July 1/2 in `strength-
reconciliation-1`; July-3-morning/afternoon in `strength-reconciliation-2`)
with the SAME underlying gap both times. **Verified this pass: no new
primary landed either side** (no B&M purchase, no Pfanz fetch) since
`strength-reconciliation-2`. The `Notes` field on `csa-oneal`'s
consultation-layer entry now records this as a **third** flag, naming both
prior boundary Δs explicitly (July-1-afternoon end 1,300 vs July-2 t0
1,100; July-3-morning end 1,040 vs July-3-afternoon t0 1,100) — recommending
the B&M/Pfanz fetch specifically for this brigade before a fourth boundary
recurs. **No strength value changed in any battle file.**

## Regeneration

- **`reconstruction/scripts/build_unit_audit.py`** re-run: **340 rows**
  (up from 339 — the one genuinely new unit id, `csa-21ms`; `us-2ma`/
  `us-27in` already existed as rows from `july3-morning-slice`, so gaining
  a second phase-file appearance extends their `Phases` column, not a new
  row — matches the script's own union-by-unit-id design). `unit-master-
  table.csv`/`.xlsx` regenerated.
- **`scripts/gen-command-overlay.py`** re-run: first pass **214 mapped, 1
  unmapped** (`csa-21ms` — no manual chain entry yet, the SAME gap
  `us-6wi`/`us-147ny`/`us-16me` hit in `decomposition-wave-1`); fixed by
  adding `csa-21ms` to `MANUAL_CHAINS` (`Army of Northern Virginia / First
  Corps / McLaws's Division`, matching `reg-csa-1c-mcl-3`'s own
  `parentChain`, the same pattern the three prior regiment promotions
  used). Re-run: **215 mapped, 0 unmapped**.
- **`docs/reconstruction/audit/oob-register.json`/`.md`** — **not
  regenerated/edited**: `21st Mississippi` is already listed inside
  `reg-csa-1c-mcl-3`'s own `regiments` array (no new register row needed,
  matching `decomposition-wave-1`'s own documented convention: "in-build
  child regiments... are represented inside their brigade entry's
  regiments list, not as separate register entries"); `us-2ma`/`us-27in`'s
  register entry (`reg-us-xii-1-3`) and `castStatus` were already correct
  from `july3-morning-slice` and needed no change for appearing in a
  second phase file.
- **`reconstruction/scripts/compile_angle.py`** re-run (task 2's edit
  only; tasks 1/3/4 touch no Angle-compiler input): metadata-only recompile,
  verified above.
- **Retrograde-facing fixer** (`reconstruction/scripts/
  fix_retrograde_facing.py`), report mode, run against the full corpus
  after all four tasks: **zero new violations** — `converted=0` across
  every phase file; the pre-existing `preserved=2`/`deferred=6` entries in
  `gettysburg-july3.json` (the two attested about-faces, the six
  Angle-cast-pinned legs) are unchanged, none of them touching any unit
  this pass edited or added.

## Suites

| Suite | Floor (post-merge, current main) | This pass |
|---|---|---|
| tool vitest | 119 | **119 passed, 0 failed** (one test updated — `gettysburg.test.ts`'s pinned July-3 unit count, 195 → 197, for task 2's two new units) |
| pipeline pytest | 59 | **59 passed** |
| reconstruction pytest | 148 + 1 skipped | **148 passed, 1 skipped** (untouched — no `reconstruction/` schema/validator source touched by this pass's data edits) |
| Unity EditMode | 429 passed, 0 failed, 4 skipped | **429 passed, 0 failed, 4 skipped** |
| Unity PlayMode | 21 passed, 0 failed, 0 skipped | **21 passed, 0 failed, 0 skipped** (no sibling Unity process on the machine immediately before launch, checked via `ps aux`) |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`, this
worktree's own `Library`; gitignored inputs restored from the main
checkout before any run — `data/heightmap`, `data/dem_cache`,
`data/landcover/{fences,relief,relief_contours,splatmap}.{json,png}` (the
tracked `landcover.json`/`map-furniture.json` needed no restore),
`app/Assets/Generated`, the SoldierView `.mp4` media; staging via
`CartographyStage.PrepareScene` — NOT `Phase12Review.PrepareStandaloneScene`
— run once before any test/build; no `-nographics` flag.)

## Post-merge re-verification (main moved under this branch)

Per the task's collision note: `angle-facing-adjudication` merged to
`origin/main` (`c8aa934`) while this pass was in flight. Diffed
`a2a17ee..c8aa934` first (`git diff --stat`) before merging: **only**
`docs/reconstruction/audit/angle-facing-adjudication.md` (new doc) and
`reconstruction/tests/test_retrograde_facing.py` (test-internal changes) —
**no battle JSON touched**, so zero collision risk with this pass's edits
to `gettysburg-july2-{afternoon,evening}.json`/`gettysburg-july3.json`.
Merged `origin/main` into this branch (clean merge, no conflicts, `ort`
strategy). Re-verified after the merge:

- **tool vitest**: 119 passed, 0 failed (unchanged).
- **reconstruction pytest**: 148 passed, 1 skipped (unchanged — the
  merged branch's test-internal changes didn't change the pass/skip
  totals).
- **pipeline pytest**: 59 passed (unchanged).
- **Angle-cast byte identity**: re-checked against the NEW `origin/main`
  tip (`c8aa934`) — still MATCH, sha256
  `032152356b7c35707e44711ffa28f7cdf744e18446d1c2e7fb9d7128029c379c`.
- **Angle bundle**: `compile_angle.py` re-run fresh against the merged
  tree — checksum `47c6f1832243307dc4b5eaac34032b16a315320229524dc887720ba382370d9b`,
  IDENTICAL to the pre-merge recompile (expected: the merge touched no
  Angle-compiler input — no battle file, no `reconstruction/` claims/
  canonical/sources, no landcover). `git status` clean after the recompile
  (byte-identical to what was already committed). `stagingSeed` re-verified
  HELD.

## Perf

Both touched files re-benchmarked (`BenchmarkHarness`,
`-benchmark`/`-benchmarkTimes`/`-benchmarkPrefix`/`-battleFile`, **default
camera** — an initial attempt using the same close `-benchmarkCamera`
override as the evidence screenshots read ~48-55 avg FPS, which is a LOD-
tier rendering-detail artifact of the close range, not a regression;
re-run at the scene's own default 4000-distance camera to get a fair floor
comparison, matching every prior wave's own methodology):

- **`gettysburg-july2-afternoon.json`** (t = 0/9900/10800/12000/12600/
  12900/14340, 153 units): **59.5–59.9 avg FPS**, p95 frame 16.73–17.44 ms,
  worst 19.4–33.4 ms, allocations flat at ~316 MB.
- **`gettysburg-july3.json`** (t = 0/8160/8700/9000/10800, the Angle film
  window explicitly included, 197 units): **59.5–59.7 avg FPS**, p95 frame
  17.10–17.57 ms, worst 20.2–49.5 ms, allocations flat at ~316 MB.

Both match the established ~59.5 floor — this pass's edits (a strength
placeholder change, a unit-count split on a static/silent brigade, and a
new parentless regiment holding flat outside the film window) cost
nothing measurable, including through the film window itself.

## Evidence

`docs/benchmarks/captures/residuals-decomp-2/` (force-added):

- **21st Mississippi / Trostle** (`rdc2-before-trostle-t{0,12000,12900,
  14340}.png` vs `rdc2-after-trostle-t{...}.png`, pivot near the Trostle
  yard, close `-benchmarkCamera` override so the ~350 m divergence reads
  clearly): pixel-diff spot check (numpy/Pillow, threshold 10/channel-sum,
  matching `decomposition-wave-1`'s own methodology) confirms each pair
  differs in a small, LOCALIZED region only — t=0: 147 px (a HUD/roster
  element, not the terrain — the unit-count change is visible there even
  though position is identical at t=0, expected, since the 21st MS and the
  residual are still co-located pre-divergence); t=12000 (the divergence):
  612 px; t=12900 (the capture): 208 px; t=14340: 49 px. Every diff bbox is
  a bounded region, not full-frame noise.
- **Colgrove panel** (`rdc2-before-colgrove-t{0,10800}.png` vs `rdc2-after-
  colgrove-t{0,10800}.png`): 123-125 px localized diff, SAME bbox both
  timestamps (625,332)-(986,450) — consistent with a roster/unit-count
  panel element reflecting the 1-unit-vs-3-unit split, not a terrain change
  (Colgrove's own units are static this window, ~40-70 m apart, below this
  camera's resolving threshold for the terrain itself).
- **Perf**: `rdc2-perf-j2p-benchmark.json` + 7 screenshots, `rdc2-perf-j3a-
  benchmark.json` + 5 screenshots (default camera, §Perf above).
- Build/prepare/test logs kept in the worktree (gitignored by design, not
  committed).

## Owner questions

1. **`csa-smith`'s July-3 propagation** (task 1's new residual): July-2-
   evening's corrected 800 vs July-3-morning's still-inherited 660 (Δ140) —
   propagating the fix through the morning phase's own decay arithmetic
   (800 − 142 = 658, and updating `gettysburg-july3.json`'s own t=0 to
   match) would close this cleanly; worth a small dedicated pickup, or fold
   into the next reconciliation wave (recommended: whichever pass next
   touches `gettysburg-july3-morning.json` for other reasons, to avoid a
   film-safety-adjacent touch on its own).
2. **The 9th MA battery's two-section split** (task 3b, deferred): would a
   Milton-retirement-endpoint fetch (a specific coordinate for "McGilvery's
   new [Plum Run] line" as Milton's own section reached it) or a section-
   grain strength primary change the call? Absent either, this stays
   deferred — worth naming as a targeted research item if the owner wants
   this specific decomposition eventually.
3. **`csa-oneal`'s thin EC6** (task 4, third flag): now recurring at every
   file boundary it touches. Worth prioritizing the B&M purchase or a
   targeted Pfanz fetch specifically for this brigade, as
   `strength-reconciliation-2` §13 already asked?

No new EDs proposed — ED-76 (the parentless-sibling "minus the Nth ___"
convention) already covers both this pass's decompositions exactly as
adopted; no new doctrine question arose.
