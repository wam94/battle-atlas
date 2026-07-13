# Strength reconciliation 1 — the cross-file strength wave

**Branch:** `strength-reconciliation-1` (unmerged; coordinator gate) ·
**Script:** `reconstruction/scripts/build_unit_audit.py` (manifest-driven
rewrite), `reconstruction/scripts/validate_reconstruction.py` (claim-subject
vocabulary broadened) · direct battle-file edits (no new author-*.ts script
this wave — a data-reconciliation pass over already-shipped keyframes, not
new authoring) · 2026-07-13

Closes the residual `day-expansion-slice-3.md` §7 item 1 named: July-1-end
strengths diverge from the July-2 files' t=0 (which inherited older
July-3/compilation bases) on most of the 42 rows shared between the July 1
afternoon file and the July 2 files. Batch-adopts ED-76/ED-77 (proposed by
decomposition-wave-1) at wave start per the standing adopt-and-adjust
convention. Fixes the pipeline gap decomposition-wave-1.md §10 item 2 named:
the master-table build script and (more conservatively) the reconstruction
validator both only read `gettysburg-july3.json`.

## 0. Pipeline gap fix (blocking prerequisite)

**`reconstruction/scripts/build_unit_audit.py`** was hardcoded to
`gettysburg-july3.json`. Rewritten to be manifest-driven
(`app/Assets/StreamingAssets/Atlas/battle-manifest.json`, ADR 0005): it now
reads every `status: reconstructed` phase's battle file, in the manifest's
own day/phase order, and merges a unit's keyframes/events across every
phase it appears in into ONE row (`merge_units_across_phases`) — a
continuing brigade's "Start strength" is its earliest appearance's t=0 and
"End strength" is its latest appearance's final keyframe, with the same
movement-metric math (path length, confidence tallies) running over the
unit's full known arc instead of one phase's slice. A new **"Phases"**
column names every phase a unit appears in (e.g.
`july1-morning/july1-afternoon/july2-afternoon/july2-evening/
july3-afternoon` for a unit present the whole battle). Consultation columns
are preserved exactly as before (re-read-before-write, keyed by Unit ID —
unaffected by the source-file change). The casualty-% formula and column
widths, previously column-index magic numbers, are now computed from
column *names* so inserting "Phases" didn't silently mis-wire them.

**Coverage result:** 337 rows (up from 191 when only `gettysburg-july3.json`
was read — the pre-existing hardcoded baseline; 195 was decomposition-wave-1's
July-3-only count after its own additions). 211 in-build, 126 not-yet-cast
(register). Every day-expansion-authored unit is now visible: Gamble,
Devin, Calef (day-expansion-slice-3), the six decomposition-wave-1 units
(`us-6wi`, `us-147ny`, `us-16me`, the four Ziegler's Grove batteries), and
every July 1/July 2 unit that never appears in `gettysburg-july3.json` at
all (most of the July 1 and July 2 cast — two-thirds of the three-day
battle's authored units were invisible to the computed columns before this
fix).

**`reconstruction/scripts/validate_reconstruction.py`** — a narrower,
conservative extension. The Angle reconstruction/bundle is and stays
July-3-afternoon-only by design (one compiled slice, one clock); its
strength-reconciliation and per-unit segment checks (§7 of the script)
stay scoped to `gettysburg-july3.json` (`corpus.battle`, `BATTLE_PATH`
unchanged) — rescoping those to a merged multi-clock unit dict would be
wrong, not a fix. The actual gap: the **claims corpus's subject
vocabulary** (`c["subjectId"] not in units...`) only recognized July-3
unit ids, so a claim about any day-expansion-authored unit had no valid
subject to reference. Added `Corpus.all_battle_unit_ids` (manifest-driven,
union across all five phases) and rewired the claim-subject check onto it;
every other check (reconstruction-unit membership, the Angle-slice
reconciliation) is untouched. This is additive-only (a superset of the old
vocabulary) — it cannot turn a passing check into a failing one, only
recognize previously-unknown-but-valid subjects.

**Tests added** (`reconstruction/tests/test_unit_audit.py`, new; one test
added to `test_reconstruction_v2.py`):

- Synthetic 2-phase fixture proving the merge logic precisely: a
  continuing unit's keyframes/events concatenate across phases in manifest
  order, last-phase-wins identity fields, a `not-reconstructed` phase is
  never read (would raise `FileNotFoundError` if it tried — the fixture
  never wrote that phase's battle file).
- Committed-repo coverage floor: the manifest names ≥5 reconstructed
  phases including all five current battle files by name (regresses loudly
  if a phase is ever dropped).
- Committed-repo regression guard: `us-cav-gamble`, `us-cav-devin`,
  `us-btty-calef`, `us-6wi`, `us-147ny`, `us-16me` (day-expansion-only ids,
  absent from `gettysburg-july3.json` by construction) must appear in the
  merged unit set, and the merged set must be a strict superset of the
  July-3-only set — this is the test that would fail if the hardcoding
  ever regressed.
- `test_claim_subject_from_day_expansion_phase_accepted`: a claim whose
  subject is a July-1-only unit id validates cleanly (previously rejected
  as "not a battle unit").

## 1. ED-76/ED-77 batch adoption

Adopted at wave start under the project's standing adopt-and-adjust
convention, recorded in `docs/reconstruction/angle-editorial-decisions.md`
exactly as prior batch-adoptions were recorded (dossier passes 1–9, ED-75):

- **ED-76** — single-regiment promotions use the parentless "minus the
  Nth ___" convention (exact subtraction), not `parent`/family. Already
  shipped by decomposition-wave-1 (`us-6wi`/`us-147ny`/`us-16me`); this
  entry formally adopts that reading as doctrine.
- **ED-77** — the Ziegler's Grove convergence: all four batteries'
  post-repulse move authored uniformly, Williston's tablet conflict
  carried on the authored reading, not adjudicated. Already shipped
  (`us-btty-williston`/`us-btty-butler`/`us-btty-martin-5us`/
  `us-btty-harn`).

Both entries note explicitly: **the owner may re-rule either decision
later** — adopt-and-adjust unblocks authoring, it does not close the
ruling.

## 2. The 42-row cross-phase strength picture

The 42 rows shared between `gettysburg-july1-afternoon.json`'s end
keyframe and the July 2 files' (`gettysburg-july2-afternoon.json` /
`gettysburg-july2-evening.json`) t=0 keyframe, recomputed fresh off current
`main` (not the `day-expansion-slice-3.md` §7 figures, which predate
decomposition-wave-1's edits to three of these same rows — `us-cutler`,
`us-coulter`, `us-meredith` changed value when the 147th NY/16th
Maine/6th Wisconsin were split out by exact subtraction, so the old
"26 of 42" count is stale; the fresh count is 35 of 42 diverge, 7 land
exact):

| Class | Count |
|---|---|
| Exact match (no action needed) | 7 |
| **Reconciled this wave** (propagated, cited) | **14** |
| Documented-irreducible (basis note, no invented number) | 21 |
| **Total shared rows** | **42** |

The 7 exact-match rows (unchanged, the method's proof): Hays 1,137,
Lowrance 500, Baxter 800, Dana 465, Wiedrich 141, Jones's and McIntosh's
battalions.

### 2.1 Reconciled (14 rows) — J1A's evidence-grounded end value propagated into July 2

Method: where the July-1-afternoon end keyframe's citation carries a real
per-day primary (a quoted OR report/return figure, a report=return exact
match, or a same-day-dated officer statement) **and** the July-2 t=0
citation is mere T1-grade ground-continuity placement with no competing
number, the July-1 figure is propagated into the July-2 t=0 keyframe.
Every one of these 14 units is a **static reserve/garrison unit through
all of July 2** (verified: single distinct strength value across every
keyframe in both July 2 files before the edit, zero casualty-bearing
events in the window except csa-perrin's one zero-loss skirmish event) —
so the fix is a uniform strength+confidence+citation update across every
keyframe of the unit in both `gettysburg-july2-afternoon.json` and
`gettysburg-july2-evening.json` (the two files already agreed with each
other at their internal boundary; both keep agreeing after the edit).
Confidence set to `documented` where the July-1 basis is a clean primary,
kept `inferred` where the July-1 citation itself flags bounded
reconstruction (Daniel's fraction, Fry's [D]-tier bound, Cutler's "pending
a full per-regiment reconciliation").

| Unit | Old J2 t0 | New (= J1A end) | Basis |
|---|---:|---:|---|
| `csa-ramseur` | 1,715 | **930** | Report=return EXACT 177 at all four regiments (the audit's fourth full-regiment-grain agreement). NAMED CONFLICT: 1,715 traces to a tablet's own "Present 1909 minus 196" calculation (found on `gettysburg-july3.json`'s own t=0 for this unit, §2.3) — a genuine primary, but a monument-tablet "Present minus assumed loss" reading, less authoritative than the brigade's own report=return exact match. Per doctrine (primary beats compilation/tablet — ED-32/ED-46/ED-47 pattern), 930 is defensible. |
| `csa-daniel` | 1,185 | **1,650** | The Rodes June-30 division return's 2,294 basis minus Daniel's own "about one-third of my men" loss statement — flagged [D] fraction-based, but the only evidenced reading either side carries (J2's 1,185 was bare placement, "no July 2 dossier anchor consumed"). |
| `csa-doles` | 1,130 | **1,240** | Report=return EXACT (179: 24k/124w/31m) against Doles's own 1,369 primary. |
| `csa-fry` | 1,048 | **750** | J2's 1,048 was Shepard's 1,048 **MORNING** primary carried un-netted into July 2 (the same never-netted error class as Davis/Marshall, §2.2) — Fry commanded Archer's brigade the morning of July 1, well before any July 1 losses. New value: ~75 captured + the return's k&w share, bounded [D], netted against the day's actual fighting. FILM-SAFETY: `csa-fry` is one of the 13 Angle-cast units, but only via `gettysburg-july3.json` at a different clock; this edit touches neither that file nor the compiled bundle — verified independently, §5. |
| `csa-gordon` | 1,200 | **990** | J2's 1,200 was Gordon's un-netted MORNING primary ("about 1,200 men... one regiment detached"). New value: his own report's 350 k+w (40 killed) netted against the return's 380 and Guild's 323 — three count sets carried un-averaged, mid-reading adopted. |
| `csa-iverson` | 650 | **645** | The audit's single-spike EC6 exemplar (1,464→650 in-window, ED-71 arithmetic adopted for computation) refined to the evening remnant's own 645 — a near-exact convergence (Δ5), the method's proof extends to an eighth row. |
| `csa-perrin` | 1,025 | **1,050** | Return 100k/477w/0m = 577 July-1-dominant (or-27-2-anv-return p. 344, division-row arithmetic-exact); J2's 1,025 was a bare skirmish-tablet quote with no strength arithmetic. |
| `us-biddle` | 460 | **390** | Chapman Biddle's OWN JULY-2-DATED report: "leaving as the present effective force only 390 officers and men" (or-27-1-cbiddle) — a direct dated primary for exactly this boundary, the strongest single basis in this pass. |
| `us-coster` | 655 | **620** | Return p. 183 aggregate sums exactly (597 loss basis, printed-noise missing-cell flagged separately elsewhere). |
| `us-coulter` | 510 | **709** | THE DAY-SPLIT PRIMARY (or-27-1-coulter's table, the only per-day brigade table in the division's records: July 1 776 / July 2 28 / July 3 14 / July 4 3), independently cross-validated by the 16th Maine's own day-loss split (both sum exactly to 776 — decomposition-wave-1's cleanest reconciliation). The strongest-evidenced row in this pass. |
| `us-cutler` | 1,015 | **777** | The brigade's own return total (1,002) minus the 147th New York's now-separately-tracked 207-loss share; flagged by its own citation as "pending a full per-regiment reconciliation" but still the best-evidenced reading either side carries. |
| `us-krzyzanowski` | 750 | **734** | Return p. 183 digit-grain, report=return EXACT on the 82nd Ohio's 181. |
| `us-meredith` | 730 | **417** | decomposition-wave-1's corrected 1,247 combined total (6th Wisconsin split out, the 118-man t=13500 correction applied); the return p. 173 brigade total (1,153) recorded alongside as the whole-compilation anchor. J2's 730 was a bare tablet note ("the July 1 wreck's survivors") with no arithmetic. |
| `us-vonamsberg` | 875 | **863** | Return p. 183 digit-grain, rows sum exactly (45th NY 224, 157th NY 307). |

### 2.2 Documented-irreducible (21 rows) — no invented number

**No primary either side (15 rows)** — both the July-1-end and July-2-t0
citations are bare position/ground continuity with no casualty arithmetic,
or explicitly flagged reconstruction on both sides with no way to prefer
one over the other. Left unchanged; the honest basis note is this row of
the table (per unit, the specific gap):

| Unit | J1A end | J2 t0 | Note |
|---|---:|---:|---|
| `csa-bn-carter` | 380 | 384 | Neither keyframe's citation states a strength basis (position-only both sides); Δ4 is inside plausible per-day muster noise. |
| `csa-bn-pegram` | 460 | 480 | Same class as bn-carter; Δ20, no arithmetic either side. |
| `csa-godwin` | 880 | 850 | Both sides weak: "NO brigade report exists (Avery mw... Godwin silent — verified negative)" governs both readings; Δ30. |
| `csa-oneal` | 1,300 | 1,100 | J1A's own citation already documents this as a recorded cross-file residual: "the return's 696 covers July 1 + July 3 with NO per-day primary... the July-3-file t=0 (1,100) implies ~694." O'Neal's July-2 (1,100) and July-3 t=0 already agree internally; forcing J1A's flagged/bounded 1,300 into July 2 would break that existing agreement without new evidence to justify it. |
| `csa-smith` (Early's div.) | 800 | 660 | Both sides attest the SAME fact (ED-44 documented flank-guard stillness, no combat either day) yet the numbers differ by 140 with no combat to explain a loss — a stale compilation-drift artifact, not new evidence either way; flagged for a future harmonization pass rather than an invented split. |
| `csa-thomas` | 1,300 | 930 | J1A: pure placement ("his Long Lane skirmish day-scope opens with the July 2 file" — no arithmetic). J2: a tablet quote attesting combat exposure but no derivation. Neither is a primary. |
| `us-smith` (Orland Smith) | 1,595 | 1,290 | J1A cites Doubleday's "our lines... were reformed" — an attestation of reforming, not a strength primary. J2 is bare tablet placement. Δ305 flagged for future research, not invented. |
| `us-btty-bancroft` | 120 | 124 | Battery-level, Δ4, no strength basis either side. |
| `us-btty-breck` | 135 | 141 | Δ6, no basis either side. |
| `us-btty-cooper` | 113 | 114 | Δ1, no basis either side. |
| `us-btty-dilger` | 126 | 127 | Δ1, no basis either side. |
| `us-btty-hall-2me` | 120 | 127 | Δ7, no basis either side. |
| `us-btty-stevens` | 134 | 136 | Δ2, no basis either side. |
| `us-btty-stewart` | 125 | 132 | Δ7, no basis either side. |
| `us-btty-wheeler` | 117 | 118 | Δ1, no basis either side. |

**ED-gated (2 rows)** — already-owner-ruled placeholders; touching them
here would either contradict the standing ruling or invent a fourth
number:

| Unit | J1A end | J2 t0 | Note |
|---|---:|---:|---|
| `csa-davis` | 1,200 | 2,000 | ED-75 (PROVISIONAL-ADOPTED 2026-07-13) already establishes Davis's post-July-1 basis via a different method (900 for the three July-1-engaged regiments, term 1 of a two-term July-3 sum) that does not cleanly match this script's own 1,200 figure. Two evidence-layer derivations disagree; J2's 2,000 is the definitively-worse un-netted tablet reading (ED-75's own text: "their July-1 losses were never netted"), but forcing EITHER 1,200 or 900 into July 2 would preempt ED-75's own explicit precondition (the Busey & Martin purchase) rather than honor it. Left as the SAME residual ED-75 already names, not silently promoted. |
| `csa-marshall` | 1,000 | 2,000 | Same ED-75 gate, parallel structure (Marshall's own placeholder is 900, envelope 750-1,050 — again not a clean match to this script's 1,000). |

**ED-48-bounded (1 row)** — an existing ruling explicitly refuses a clean
number for this unit:

| Unit | J1A end | J2 t0 | Note |
|---|---:|---:|---|
| `csa-brockenbrough` | 820 | 880 | ED-48 already rules this "the honestly-bounded unit": EC2 stays an 800–1,100 range with "no promotion to a clean number," and the duplication-corrupted Stone Sentinels record is permanently unusable. J1A's own 820 is itself "[D, flagged reconstruction]." Forcing a propagation here would violate ED-48's own explicit refusal to clean this unit up. |

**Named conflict, J2 already carries the defensible value (2 rows)** — no
edit made; the July-2 figure is independently grounded and, per doctrine,
preferred over July-1's own end value. Flagged for a future July-1-track
re-basing pass rather than edited here (editing J1A's own multi-keyframe
afternoon decay curve is a bigger, riskier surgery than this wave's
single-keyframe propagations, and is not required to clear THIS
boundary — the boundary already reads correctly from the July-2 side):

| Unit | J1A end | J2 t0 (unchanged, defensible) | Conflict |
|---|---:|---:|---|
| `csa-lane` | 1,660 | 1,355 | J2's 1,355 is the **ED-47-adopted** figure verbatim — Lane's own report, "660 out of an effective total of 1,355." J1A's 1,660 was authored (day-expansion-slice-3) without cross-checking the already-adopted ED-47 figure. **Defensible value: 1,355** (already resident in July 2 — no edit needed there); J1A's own end-of-day keyframe is flagged here as needing a future re-basing pass against ED-47, out of this wave's single-keyframe-propagation scope. |
| `us-harris` | 700 | 565 | Both grounded: J1A's 700 is a 3-day-weighted return apportionment ("mostly July 1, flagged"); J2's 565 is Harris's OWN JULY-2-DATED report ("the 75th OH's 91-officers-and-men July 2 basis"). The day-specific dated primary is more probative for this exact boundary. **Defensible value: 565** (already resident in July 2); J1A's 700 flagged for a future correction, not forced here. |

**Composition change, not a data conflict (1 row)** — the divergence is
correctly explained by the unit's actual composition changing between the
two windows, not by either side being wrong:

| Unit | J1A end | J2 t0 (unchanged, correct as-is) | Explanation |
|---|---:|---:|---|
| `us-vongilsa` | 400 | 605 | The 41st NY (part of von Gilsa's brigade) is attested absent during the July-1-afternoon window (return p. 182: "the 41st NY's 75 all July 2-3... the structural day-split anchor") and rejoins before July 2 — so the brigade's July-1-end strength CORRECTLY excludes it (400) while July-2's t0 CORRECTLY includes it (605). Propagating 400 into July 2 would be wrong: it would incorrectly drop a regiment that was demonstrably present for the July 2 fight. No edit; the divergence is the composition change, already correctly rendered on both sides. |

## 3. Register / overlay / master table regeneration

- **`reconstruction/scripts/build_unit_audit.py`** — re-run after both the
  pipeline fix and the strength edits (deterministic; consultation columns
  re-read-before-write, unaffected). 337 rows, 211 in-build / 126
  not-yet-cast. `docs/reconstruction/audit/unit-master-table.csv`/`.xlsx`
  regenerated.
- **`scripts/gen-command-overlay.py`** — re-run: **211 mapped, 0
  unmapped**, byte-identical to the committed
  `app/Assets/Resources/Battle/command-overlay.json` (no diff produced —
  none of this wave's edits touch unit id/parent/register composition,
  only strength/confidence/citation fields on existing keyframes).
- **`docs/reconstruction/audit/oob-register.json`/`.md`** — not
  regenerated; no castStatus/parentChain/composition change this wave
  (strength-only edits don't touch the register's own fields).
- **`reconstruction/scripts/compile_angle.py`** — re-run (§5): bundle
  byte-identical, confirming zero drift (stronger than a metadata-only
  recompile — this wave's edits don't touch `gettysburg-july3.json` at
  all, so the compiler's only input didn't change).

## 4. Suites (all green, no regressions)

| Suite | Baseline (main, decomposition-wave-1) | This wave |
|---|---|---|
| tool vitest | 119 | **119 passed, 0 failed** (unchanged — no `tool/` TS source touched) |
| pipeline pytest | 59 | **59 passed** |
| reconstruction pytest | 122 passed, 1 skipped | **128 passed, 1 skipped** (+6: 5 new tests in `test_unit_audit.py`, 1 new test in `test_reconstruction_v2.py`) |
| Unity EditMode | 377 passed, 0 failed, 4 skipped | **383 passed, 0 failed, 4 skipped** (387 total; above the 377+4=381 floor, zero failures — the task's own instruction treats current main's numbers as a floor, not an exact pin) |
| Unity PlayMode | 17 passed (may be 20 on current main) | **20 passed, 0 failed** — matches the "may be 20" floor exactly |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`, this
worktree's own `Library` — built fresh on first run, not shared with the
owner's main checkout — gitignored inputs restored from the main checkout
(`data/heightmap`, `data/dem_cache`, `data/landcover/*`, `app/Assets/
Generated`, SoldierView `.mp4` media); `CartographyStage.PrepareScene` run
first, NOT `Phase12Review.PrepareStandaloneScene`; no `-nographics` flag
used, per the task's warning about a spurious HDRP RenderTexture failure.
`PrepareScene`'s terrain-reimport touched `app/Assets/Scenes/Atlas.unity`
locally — reverted before committing, since no prior wave commits that
file for this reason and it carries no content from this wave. Logs
`testlogs/editmode2.log`/`playmode.log` + results XML in the worktree,
gitignored by design, not committed.)

## 5. Film-safety tripwire — verdict FILM-SAFE

Only one unit touched by this wave (`csa-fry`, §2.1) is among the 13
Angle-cast units (`csa-garnett`, `csa-kemper`, `csa-armistead`, `csa-fry`,
`us-webb`, `us-69pa`, `us-71pa`, `us-72pa`, `us-btty-cushing`,
`us-btty-cowan`, `us-btty-arnold`, `us-hall`, `us-stannard`); every other
edited/checked unit (Ramseur, Daniel, Davis, Doles, Godwin, Gordon,
Iverson, Lane, Marshall, O'Neal, Perrin, Smith, Thomas, Biddle, Coster,
Coulter, Cutler, Harris, Krzyzanowski, Meredith, von Amsberg, von Gilsa,
the eight batteries, the two artillery battalions) is not in that set at
all. `csa-fry` is touched only via `gettysburg-july2-afternoon.json`/
`gettysburg-july2-evening.json` — its OWN, separate track at a different
clock from the Angle slice (t=8040..9000 on `gettysburg-july3.json`'s
clock).

Verified three independent ways, exactly as decomposition-wave-1.md §5
established the pattern:

1. **Source-file byte identity.** `git diff --stat -- app/Assets/Battle/
   gettysburg-july3.json` is empty — this wave never opened, read for
   writing, or touched that file. The 13 Angle-cast units' extracted JSON
   (sorted-key, sha256'd) is **byte-identical** between this worktree and
   `origin/main`: `df52ce3a8bdcc854bb4bc2ac57e8f548e2c114577898b2b02b5ecb855a064cc0`.
2. **Compiled bundle payload identity.** `reconstruction/scripts/
   compile_angle.py` re-run after all battle-file edits landed:
   `app/Assets/Battle/Angle/angle.bundle.json` is **byte-identical**
   before/after (`diff` empty — stronger than a metadata-only recompile,
   since the compiler's only input, `gettysburg-july3.json`, never
   changed). `stagingSeed` unchanged:
   `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
   (the ED-21 pin HOLDS).
3. **Structural containment.** Every unit this wave edits or names as
   irreducible lives in `gettysburg-july1-afternoon.json` /
   `gettysburg-july2-afternoon.json` / `gettysburg-july2-evening.json` —
   files the Angle bundle compiler never reads. No row in this wave
   touches `gettysburg-july3.json`.

**No row was reverted; none needed to be.**

### Perf (the two edited files)

Built `BattleAtlas.EditorTools.BenchmarkBuild.Build` fresh in this
worktree, ran the generic `BenchmarkHarness` (`-benchmark
-benchmarkTimes -benchmarkPrefix -battleFile`) against both touched files
across their full window:

- **`gettysburg-july2-afternoon.json`** (t = 0/3600/7200/10800/14340, 152
  units): **59.1–59.8 avg FPS**, p95 frame 16.9–17.4 ms, allocations flat
  at ~323 MB — matches the established 59.5–59.9 baseline range (the
  single t=0 sample at 59.1 is the usual startup-frame artifact seen in
  every prior wave's t=0 sample).
- **`gettysburg-july2-evening.json`** (t = 0/3600/7200/10860, 152 units):
  **59.4–59.7 avg FPS**, p95 frame 17.0–17.2 ms, allocations flat at
  ~322 MB.

`gettysburg-july3.json` (the film-window file) was not re-benchmarked —
it is untouched, byte-identical to `origin/main` (§5), so its previously
recorded perf (decomposition-wave-1.md: 59.5–59.7 avg FPS at the film
window) still stands unchanged.

Evidence: `docs/benchmarks/captures/strength-reconciliation-1/`
(force-added) — both benchmark JSONs and their per-timestamp screenshots.

## 6. Rows to keep an eye on (recommendations for the next wave)

1. **`csa-ramseur`'s July-3 t0 (1,715)** carries its own independent
   tablet-based derivation ("Present 1909 minus 196"), now the ONLY place
   in the corpus still carrying that reading (July 1 and the freshly-fixed
   July 2 both read 930). A future wave should decide whether to extend
   this reconciliation into `gettysburg-july3.json` — doing so would
   trigger a bundle recompile, but Ramseur is not one of the 13 Angle-cast
   units, so it would not touch the film window.
2. **`csa-lane` and `us-harris`** (§2.2, named conflicts) need a
   dedicated re-basing pass on their July-1-afternoon tracks — the
   defensible value already lives in July 2, but J1A's own multi-keyframe
   decay curve was never corrected to match.
3. **`csa-davis`/`csa-marshall`** remain gated on ED-75's stated
   precondition (the Busey & Martin purchase); once that lands, this
   wave's July-2 t0 values for both (currently untouched at 2,000 each)
   should be revisited in the same pass.
4. **The 15 no-primary-either-side rows** (§2.2) are not errors — they're
   an honest floor. Several (the 8 battery rows, Δ1–7 men) are plausibly
   ordinary muster noise; `csa-smith`'s Δ140 with attested stillness on
   BOTH sides is the one genuinely worth a dossier re-check (a documented
   flank-guard unit with no combat shouldn't show any strength delta at
   all).
5. **`build_unit_audit.py`'s new "Phases" column** makes the master
   table's own coverage visible at a glance; a follow-up could add a
   computed "cross-phase strength continuity" flag column (green/red per
   row) so this class of residual surfaces automatically instead of
   requiring a bespoke reconciliation pass each time.
