# Strength reconciliation 2 — the July 3 morning/afternoon seam, csa-daniel's best guess

**Branch:** `strength-reconciliation-2` (unmerged; coordinator gate) · direct
battle-file edits (no new `author-*.ts` script for the strength-only rows —
same "data-reconciliation pass over already-shipped keyframes" convention
`strength-reconciliation-1` established), one targeted fix to the existing
`tool/scripts/author-july3-morning.ts` (csa-smith's double-count) re-run
through its own deterministic pipeline · 2026-07-14

Closes the `july3-morning-slice.md` §5/§8/§9 residual: the 13-row (+1,
csa-daniel) morning-end-vs-afternoon-t0 strength table produced when
`july3-morning` was authored on top of an afternoon file (`gettysburg-
july3.json`) that predates it. Exercises the owner's three same-session
rulings verbatim (quoted below). Follows `strength-reconciliation-1.md`'s
report conventions exactly: per-row old/new/basis/citation, honest-basis
notes for irreducible rows, no invented numbers, film-safety verified three
independent ways.

## 0. Owner rulings in force (2026-07-14)

Quoted from this pass's task brief:

1. **ED-78 scope, owner-confirmed standing** — `july3-morning-slice.md`
   §9 owner-question #1 asked whether ED-78's "STANDING permission"
   framing was the intended breadth. The owner answered **"yes"** —
   this is now an owner-confirmed ruling, not the executor's own reading
   of ED-78's text. Recorded as an annotation on the ED-78 entry itself
   (`docs/reconstruction/angle-editorial-decisions.md`, "Owner
   confirmation (2026-07-14, strength-reconciliation-2)").
2. **This pass runs NOW**; every resolution below carries a preserved
   refinement note for future hardening (the Busey & Martin purchase, the
   Pfanz *Culp's Hill* pp. ~340-355 fetch — both still open, neither
   retired by this pass).
3. **`csa-daniel` gets a BEST GUESS NOW** (strength Δ193 AND the
   position-class/geometry mismatch), provisional per ED-78, refinement
   note preserved — §2 below.

## 1. The 13-row reconciliation (morning-end vs afternoon-t0)

Method (per `strength-reconciliation-1`'s established convention): where
one side's citation carries a real primary (an OR return/report with
exact regimental rows, or a tablet's own casualty total) exact-subtracted
through an ACTUAL authored continuity chain, and the other side is a bare
rounded approximation or a generic non-primary placeholder, the
primary-grounded reading is propagated. Where no primary distinguishes
the two sides, or an owner-gated conflict already governs the row, it is
left untouched and documented. Every row below is the SAME
evidence-layer-vs-frozen-build-value class `strength-reconciliation-1`
catalogued for July 1→2 (`july3-morning-slice.md` §5's own framing) — not
a new class of problem, this chain's second instance.

All 13 rows live in `gettysburg-july3.json` (the afternoon file, legacy —
no generating script; edited directly, matching `strength-reconciliation-
1`'s precedent for this exact class of pass) EXCEPT csa-smith, which is
fixed at its source in `gettysburg-july3-morning.json` instead (§4).

### 1.1 CSA tablet-computed rows (4) — RECONCILED, near-agreement, same return primary

All four afternoon citations read `"tablet Present [X] less its [Y]
(Culp's Hill fight) battle losses -> ~[Z]", approximation flagged` — a
ROUNDED tablet "Present" figure minus the SAME return total the morning
phase itself exact-subtracts through its own authored July-2-evening
continuity chain. Both sides anchor to the identical primary (Johnson's
table, EXACT); the morning reading is preferred because it traces an
actual authored decay chain rather than an independently-rounded "Present
~X" tablet guess.

| Unit | Old (afternoon t0) | New | Return primary (both sides) | Basis for the new value |
|---|---:|---:|---:|---|
| `csa-steuart` | 1020 | **998** | 682 (Johnson's table, exact) | 1580 (July-2-evening authored end) − 582 (July-3 share) = 998; Δ=22 vs the rounded tablet reading |
| `csa-williams` | 710 | **682** | 388 (exact) | 940 − 258 = 682; Δ=28 |
| `csa-walker` | 1120 | **1110** | 330 (exact) | 1440 − 330 = 1110; Δ=10, near-exact |
| `csa-dungan` | 1180 | **1139** | 421 (exact, footnote regimental conflict carried) | 1400 − 261 = 1139; Δ=41 |

### 1.2 US XII Corps rows (7) — RECONCILED, primary beats a non-primary placeholder

All seven afternoon citations read, verbatim: *"NO strength figure on the
tablet page (casualties only) — [N] reconstructed July-3 effectives from
standard engaged-strength norms... rounded, flagged"* — an explicitly
NON-primary generic heuristic. The morning phase's own end value in every
case traces an actual OR return (exact regimental rows) or a tablet's own
verbatim casualty total, exact-subtracted through the authored July-2-
evening chain. This is the clearest class in the whole pass: primary vs.
self-flagged-non-primary, no residual doubt.

| Unit | Old (afternoon t0) | New | Primary | Basis |
|---|---:|---:|---:|---|
| `us-greene` | 1125 | **1007** | Return 303 (p.185; report 307, 4-man conflict carried) | 1170 − 163 = 1007 |
| `us-kane` | 600 | **502** | Return = Cobham's 98 (p.184, exact rows; Kane's own report says 96, carried) | 595 − 93 = 502 |
| `us-candy` | 1400 | **1261** | Return 139 (p.184, exact rows) | 1400 − 139 = 1261 (the full return; the afternoon file never subtracted it) |
| `us-mcdougall` | 1755 | **1675** | Return 80 (p.184, exact rows) | 1755 − 80 = 1675 (full return, never subtracted) |
| `us-lockwood` | 1650 | **1476** | Return 174 (p.184, exact rows) | 1645 − 169 = 1476 |
| `us-shaler` | 1770 | **1696** | Brigade tablet's own casualty total, verbatim ("Total 74") | 1770 − 74 = 1696 (the same tablet the afternoon file's own t0 already reads, but never subtracted) |
| `us-colgrove` (family) | 1170 (whole, pre-decomposition) | **891** | Family total from the morning phase's own decomposed EndT keyframes | `csa-colgrove` 482 + `us-2ma` 180 + `us-27in` 229 = 891 — COMPOSITION NOTE below, not a plain propagation |

**`us-colgrove` composition note** (matching `strength-reconciliation-1`'s
`us-vongilsa` "composition change, not a data conflict" class): the
morning phase decomposes Colgrove's brigade into three parentless
siblings (ED-76 convention — `us-2ma`/`us-27in` promoted out) while
`gettysburg-july3.json` still renders the brigade as ONE unit. The
divergence in unit COUNT is expected and not reconciled here (that would
require a decomposition-wave pass on the afternoon file, out of this
wave's scope); the STRENGTH value is reconciled to the family total so
the single afternoon unit correctly reads the combined post-charge
effectives rather than the stale pre-decomposition figure.

### 1.3 `csa-oneal` — DOCUMENTED, not changed (thin EC6, no new primary)

Per this pass's Task 4 instruction ("document status; only change values
if a primary supports it"): **left unchanged, both files.** Morning-end
1040 (afternoon: 1100, Δ60) is not a clean propagation candidate — see §3
for the full analysis. This is the SAME documented-irreducible treatment
`strength-reconciliation-1` gave `csa-oneal`'s July-1/July-2 boundary
(that report's §2.2: "forcing... would break existing agreement without
new evidence to justify it").

### 1.4 `csa-smith` — FIXED at its source, not propagated (§4)

Was Δ40 (morning-end 620 vs afternoon 660); the double-count was inside
the MORNING file's own arithmetic, corrected there (now 660 = 660, Δ0).
No `gettysburg-july3.json` edit needed or made for this unit.

### 1.5 `csa-daniel` — task 2, best guess (§2)

Was Δ193 + a position-class mismatch; handled separately per the owner's
specific ruling (c).

## 2. `csa-daniel` — the best-guess resolution (ED-78, owner-confirmed standing scope)

Per owner ruling (c): dossier-checked both the strength gap and the
position-class/geometry mismatch; both resolved NOW as a single
PROVISIONAL best guess, both poles carried, ED-78 cited, refinement note
preserved.

### 2.1 Strength: 1185 → 1378 (adopted), both poles carried

- **Rejected-for-now pole** (carried verbatim in the new citation): the
  afternoon file's own tablet reading — *"tablet Present 2100 less its
  916 (July 1 + the Culp's Hill morning) battle losses → ~1185... approximation
  flagged"*.
- **Adopted pole**: the morning phase's exact-subtraction — ANV return
  916 total (p. 342, the brigade's largest Second Corps loss) minus the
  `strength-reconciliation-1`-established July-1-end basis (1650, itself
  the Rodes June-30 division return minus Daniel's own "about one-third
  of my men" fraction) = 1378. This traces through the ACTUAL authored
  July-1-end figure rather than an independently-rounded "Present 2100"
  tablet guess — the same preference class `strength-reconciliation-1`
  established for `csa-ramseur` (exact-subtraction primary over a
  tablet's "Present minus assumed loss" reading).
- **Refinement note (preserved, not closed)**: the tablet's "Present
  2100" and the Rodes return's 2,294 basis disagree on Daniel's
  PRE-battle baseline by 194 men — a Busey & Martin purchase or the Pfanz
  fetch could resolve which baseline is correct and retire this
  uncertainty. Not resolved by this pass.

### 2.2 Position: a withdrawal-transition, not a single overwrite

The morning phase's own EndT (13:00) continuity keyframe places Daniel
east of Rock Creek at (4900,5990) — the terminus of a Bachelder-sourced
chain (j3-02 "DANIEL" label 6035,5465, the ~470 m staff-guided left-flank
move onto Steuart's sector, per the bachelder-manifest sheet-pixel-to-
local-meter transform → the post-charge "sheltered position... within
less than [~100 yards]" hold → this terminus). The afternoon file's own
t0 (6490,5360) is Bachelder sheet 8's (s8_culps.jpg) drawn "Benner's Hill
base" — also a real primary, for the SAME 13:00 clock.

Daniel's OWN dated OR report resolves which pole governs which part of
the window: *"not withdrawn until between 3 and 4 o'clock in the
afternoon"* — a first-person primary that outranks the tablets' summary
clock (primary beats tablet/compilation — the ED-32/ED-46/ED-47 doctrine
`strength-reconciliation-1` already applied to `csa-ramseur`). Best
guess: **author the transition instead of picking one pole for the whole
window** — both readings are correct, at different times:

| t | Position | Confidence | Reading |
|---|---|---|---|
| 0 (13:00) | (4900,5990,facing 140) | inferred | ADOPTED for this clock: Daniel's own report, "not yet withdrawn" |
| 10800 (16:00) | (6490,5360,facing 255) | inferred | the LATE edge of Daniel's own "3 to 4 o'clock" bracket (conservative, not mid-bracket) — now matching the tablets/Bachelder sheet 8 for the rest of the window |

Both poles of the withdrawal-clock conflict (this pass's report-derived
15:00–16:00 vs. the tablets' near-noon/10:30 clock) stay on the record at
BOTH keyframes per ED-78 condition 2 — neither is silently dropped.
Strength stays 1378 at both keyframes (no further combat attested in this
repositioning; Daniel's skirmishers "engaged until nearly 12 o'clock at
night" is carried as text, below this survey's keyframe strength-grain).

Geometry basis for the adopted position (per this pass's Task 2
instruction): the morning phase's own Bachelder-sourced chain (above);
the Benner's Hill base pole is Bachelder sheet 8's own drawn point,
unchanged from the afternoon file's prior reading, now correctly
sequenced to the LATE half of the window instead of the whole window.

**Captures** (before/after, position moved):
`docs/benchmarks/captures/strength-reconciliation-2/daniel-before-culp-t0.png`
vs `daniel-after-culp-t0.png` (Culp's Hill-sector camera, t=0/13:00 — the
`RODES'S DIVISION` division label, whose position is the centroid of its
member brigades including Daniel, becomes visible in-frame in the AFTER
shot because Daniel's brigade physically moved west into this camera's
field; it is absent from the BEFORE shot at the same pose) and
`daniel-before-culp-t10800.png` vs `daniel-after-culp-t10800.png` (t=10800/
16:00 — the two converge, since the withdrawal transition lands Daniel
back at the tablets' Benner's Hill base by then). Default-camera captures
(`daniel-before-t0.png` etc., theater-wide pose) also included for
completeness but are not diagnostic at that zoom.

## 3. `csa-oneal`'s thin EC6 — documented, not resolved

The afternoon file's own reading (1100) uses the SAME return primary
(696, p. 342) the morning phase does, computed the SAME way the CSA
tablet-rows in §1.1 were (`tablet Present 1794 less its 696... → ~1100`).
On the surface this looks like a §1.1-class propagation candidate. It is
NOT, for a reason specific to this unit, already flagged by the morning
phase's own citation: **the return's 696 total, minus the already-
established July-1 loss (1794→1100, a 694-man share), leaves only ~2 men
of arithmetic headroom for a brigade whose own report describes "held for
three hours, exposed to a murderous fire."** The morning phase applies an
APPROXIMATE, not exact-subtracted, additional −60 decay to produce its
own 1040 and explicitly flags the inconsistency rather than resolving it.

Forcing either number into the other file would not fix anything — both
1100 and 1040 already sit inside the SAME thin, flagged margin; picking
one over the other would manufacture false precision from an
acknowledged-insufficient primary, not adopt a stronger one. This is
structurally identical to `strength-reconciliation-1`'s own `csa-oneal`
row (§2.2 of that report: *"the return's 696 covers July 1 + July 3 with
NO per-day primary... forcing [a different reading] would break [the]
existing agreement without new evidence to justify it"*) — the same
brigade, the same underlying evidentiary gap, now recurring at the next
file boundary. **Left unchanged in both files per this pass's Task 4
instruction.** A B&M-class per-day split or a Pfanz fetch (both already
open research items, neither retired by this pass) would resolve it
cleanly; nothing short of that would.

## 4. `csa-smith`'s pre-baked t0 — fixed (double-count corrected)

`gettysburg-july2-evening.json`'s inherited placeholder for `csa-smith`
(660) was computed, before the morning phase existed, as "tablet Present
~800 minus the FULL 142-loss return" — i.e. it already nets Smith's
ENTIRE battle loss, even though Smith's only combat is inside the morning
phase's own window (`july3-morning-slice.md` §5.2 named this precisely).
The morning phase's authoring script (`tool/scripts/author-july3-
morning.ts`) inherited that 660 at t=0 correctly, but then its own
`T(11, 30)` keyframe applied a FURTHER "-40 modest documented-notional
decay... NOT independently derived" on top — subtracting Smith's one
attested battle loss a second time.

**Fix**: removed the redundant decay. The keyframe's strength stays 660
(matching t0) through to `EndT`; the citation is rewritten to name the
double-count explicitly and record the correction. `gettysburg-july3-
morning.json` was regenerated deterministically (`npx vite-node
scripts/author-july3-morning.ts` from `tool/`, matching the
`author-july3-morning.ts` header's own committed run command) — the diff
is exactly 4 lines (2 strength values + 2 citation strings, both inside
the one corrected keyframe and its EndT twin).

`gettysburg-july3.json`'s own `csa-smith` t0 (660) needed no edit — it
now agrees EXACTLY with the corrected morning-end value (Δ=0).

**Refinement note (preserved, not closed)**: the STRUCTURAL fix — re-
deriving `gettysburg-july2-evening.json`'s `csa-smith` placeholder from
the tablet's ~800 PRESENT figure directly, rather than its post-loss
~660, so the morning phase needs no inherited pre-netting at all — remains
open. It requires editing a July-2 file this pass did not otherwise touch
and was judged out of scope; a future pass can pick it up directly
(`july3-morning-slice.md` §8 item 4 already named this as the
recommended structural fix).

## 5. Rows resolved / provisional / documented (tally)

| Class | Count | Rows |
|---|---:|---|
| Reconciled (propagated primary-grounded value) | 11 | steuart, williams, walker, dungan, greene, kane, candy, mcdougall, lockwood, colgrove (family), shaler |
| Fixed at source (double-count corrected) | 1 | smith |
| Best-guess, provisional (ED-78, both poles) | 1 | daniel (strength + position transition) |
| Documented, unchanged (no primary to prefer) | 1 | oneal |
| **Total** | **14** | (the 13-row table + csa-daniel, task 2) |

## 6. ED-78 annotation

`docs/reconstruction/angle-editorial-decisions.md`'s ED-78 entry now
carries an "Owner confirmation (2026-07-14, strength-reconciliation-2)"
paragraph recording the owner's explicit "yes" to the standing-permission
reading (owner ruling (a), §0 above) — this pass is the first to cite
ED-78 directly for a NEW provisional call (csa-daniel) under that
confirmed scope, rather than re-litigating or merely extending the
original Spangler's-Meadow ruling.

## 7. Register / master table / overlay regeneration

- **`reconstruction/scripts/build_unit_audit.py`** re-run (`cd
  reconstruction && uv run --with openpyxl python scripts/
  build_unit_audit.py`): 339 rows (unchanged count — this wave edits
  existing keyframe values only, no unit added/removed/reparented), 214
  in-build / 125 not-yet-cast. `unit-master-table.csv`/`.xlsx`
  regenerated — diff is a targeted 12-line CSV change (the computed
  strength/casualty columns for the reconciled units) plus the matching
  `.xlsx` binary.
- **`scripts/gen-command-overlay.py`** re-run: **214 mapped, 0
  unmapped**, byte-identical to the committed `command-overlay.json` (no
  diff — this wave touches no unit id/parent/register composition field,
  only strength/position/confidence/citation on existing keyframes).
- **`docs/reconstruction/audit/oob-register.json`/`.md`** — not
  regenerated; no castStatus/parentChain/composition change this wave
  (matching `strength-reconciliation-1`'s same finding for the same
  reason).
- **`reconstruction/scripts/compile_angle.py`** re-run: bundle payload
  identical except `inputs.battle` (the whole-file checksum of
  `gettysburg-july3.json`, expected — that file changed for non-Angle-
  cast units) and the top-level `checksum` (self-referential over the
  changed `inputs`); every unit/claim/segment/staging field byte-for-byte
  identical (§8).

## 8. Film-safety tripwire — verdict FILM-SAFE

None of the units this wave touches is among the 13 Angle-cast units
(`csa-garnett`, `csa-kemper`, `csa-armistead`, `csa-fry`, `us-webb`,
`us-69pa`, `us-71pa`, `us-72pa`, `us-btty-cushing`, `us-btty-cowan`,
`us-btty-arnold`, `us-hall`, `us-stannard`). All edited `gettysburg-
july3.json` keyframes sit at t=0 or t=10800 (13:00/16:00 LMT) — well
outside the t=8160..8820 Angle-cast film window this pass's tripwire
names; no row required perturbing Angle-cast state inside that window, so
no row needed a STOP-and-defer.

Verified three independent ways:

1. **Angle-cast unit byte identity.** The 13 units, extracted (sorted-key
   JSON) from this worktree's `gettysburg-july3.json` and from
   `origin/main`'s: **sha256
   `6690091a19dcf7e27ddb63fcbd3d65052ce2593b7597d9c265416a730503a3cb`** —
   MATCH, byte-identical.
2. **Compiled bundle payload identity.** `compile_angle.py` re-run after
   all edits: `angle.bundle.json`'s payload is identical to the
   pre-edit bundle in every field EXCEPT `inputs.battle` (the
   `gettysburg-july3.json` whole-file hash, which legitimately changed —
   non-Angle-cast units in that file were edited) and the top-level
   self-referential `checksum`. `stagingSeed` **HELD**:
   `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`.
3. **Structural containment.** Every edited/checked unit lives in
   `gettysburg-july3-morning.json` (a file the Angle compiler never
   reads at all) or in `gettysburg-july3.json` at t=0/t=10800, outside
   the compiler's t=8040..9000 read window.

**VERDICT: FILM-SAFE.**

## 9. Suites (all green, at or above the current-main floor)

| Suite | Floor (current main) | This wave |
|---|---|---|
| tool vitest | 119 | **119 passed, 0 failed** (one test extended — `gettysburg.test.ts`'s Wave-5 assertion now names `csa-daniel` as the one legitimate ED-78-provisional/mover exception among the 26 CSA Wave-5 brigades, rather than adding a new test) |
| pipeline pytest | 59 | **59 passed** (untouched — no `pipeline/` source touched) |
| reconstruction pytest | 128 + 1 skipped | **128 passed, 1 skipped** (untouched — no `reconstruction/` schema/validator source touched, only data the existing tests already exercise) |
| Unity EditMode | 405 passed, 0 failed, 4 skipped | **405 passed, 0 failed, 4 skipped** (run twice — before and after the `BenchmarkHarness.cs` capture-tooling addition, §10 — identical both times) |
| Unity PlayMode | 21 passed, 0 failed, 0 skipped | **21 passed, 0 failed, 0 skipped** (run twice, identical both times; no sibling Unity process on the machine during either run — checked via `ps aux` first, per the task's shared-machine warning) |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`, this
worktree's own `Library`; gitignored inputs restored from the main
checkout — `data/heightmap`, `data/dem_cache`, `app/Assets/Generated`,
the SoldierView `.mp4` media; staging via `CartographyStage.PrepareScene`
— NOT `Phase12Review.PrepareStandaloneScene` — run before the benchmark
build; no `-nographics` flag; `PrepareScene`'s terrain-reimport touched
`app/Assets/Scenes/Atlas.unity` locally, reverted before every commit,
same as every prior wave.)

**The one test change** (`tool/tests/gettysburg.test.ts`, the Wave-5 CSA
brigade assertion): previously asserted EVERY CSA Wave-5 brigade's t0
keyframe is `"documented"`, sits east of x=6300, and quotes "Retired at
10.30 A. M."; `csa-daniel`'s t0 is now legitimately `"inferred"`
(ED-78 PROVISIONAL) and west of x=6300 (still holding, not yet
withdrawn). The test now carves out `csa-daniel` explicitly — asserting
its t0 is `inferred`+cites ED-78, its t10800 is the Benner's Hill base
(x>6300)+cites ED-78, and adds it to the file's "movers" list (it is no
longer a static 2-identical-keyframe unit) — rather than weakening the
blanket assertion for everyone else.

## 10. `BenchmarkHarness.cs` — a small additive capture-tooling change

Added an optional `-benchmarkCamera x,z,yawDeg,pitchDeg,dist` CLI
override (inert unless passed; every existing benchmark invocation in
every prior wave's script is byte-for-byte unaffected) so this pass could
aim the orbit camera at the Culp's Hill/Johnson's-block sector instead of
the scene's committed default pose (which frames the Angle/Pickett's-
Charge sector and does not usefully show Daniel's position at all — see
the non-diagnostic `daniel-before-t0.png`/`daniel-after-t0.png` pair,
kept for completeness). Unity EditMode/PlayMode suites re-run in full
after this change (§9) — identical results both times.

## 11. Perf

Both edited battle files re-benchmarked (`BattleAtlas.EditorTools.
BenchmarkBuild.Build`, then the generic `BenchmarkHarness` with
`-benchmark -benchmarkTimes -benchmarkPrefix -battleFile`):

- **`gettysburg-july3-morning.json`** (t = 0/7200/19800/21600/30600, the
  same timestamps `july3-morning-slice.md` used, 17 units): **59.3–59.5
  avg FPS**, p95 frame 17.46–17.57 ms, worst 21.3–31.6 ms, allocations
  flat at ~320–321 MB — matches that slice's own 59.3–59.8 baseline.
- **`gettysburg-july3.json`** (t = 0/8160/8700/9000, the Angle film
  window): **59.3–59.7 avg FPS**, p95 frame 17.12–17.58 ms, worst
  21.8–22.4 ms, allocations flat at ~322.9 MB — matches the established
  59.5–59.9 range; confirms the afternoon-file strength/position edits
  (all non-Angle-cast, all outside the film window) cost nothing
  measurable at the film window itself.

Evidence: `docs/benchmarks/captures/strength-reconciliation-2/`
(force-added) — `sr2-morning-benchmark.json` + 5 screenshots,
`sr2-afternoon-benchmark.json` + 4 screenshots, `daniel-before-culp-t{0,
10800}.png` / `daniel-after-culp-t{0,10800}.png` (the diagnostic
before/after pair, §2.2), `daniel-before-t{0,10800}.png` /
`daniel-after-t{0,10800}.png` (default-camera, non-diagnostic, kept for
completeness), and the prepare/build/run logs.

## 12. Post-merge re-verification (main moved under this branch)

Per the task's collision note: `retrograde-facing` (facing-value
regeneration, manifest-driven, across the reconstructed phases — 8 legs
in `gettysburg-july3-morning.json`: Steuart/Daniel/O'Neal withdrawals,
Candy's return, the 2nd MA/27th IN charge recalls) and `map-furniture`
(roads/streams/town/railroad) both merged to `origin/main` while this
pass was in flight. Merged `origin/main` into this branch
(`9f84118`). Collision resolution:

- `gettysburg-july3-morning.json` / `gettysburg-july3.json`: **auto-
  merged cleanly** — `retrograde-facing`'s 8 facing-only edits and this
  pass's strength/position/citation edits touch disjoint keyframes (its
  Daniel edit is `t=19800`'s facing; this pass's Daniel edits are the
  afternoon file's `t=0`/`t=10800`, a different file and different
  keyframes entirely; its `csa-smith` was untouched by `retrograde-
  facing`). No manual resolution needed.
- `app/Assets/Battle/Angle/angle.bundle.json` / `docs/reconstruction/
  angle-bundle-audit.md`: **conflicted** (both branches recompiled the
  bundle independently) — resolved BY REGENERATION per the established
  precedent (`decomposition-wave-1`/`strength-reconciliation-1`'s own
  collision doctrine): `uv run python scripts/compile_angle.py` re-run
  fresh against the merged source tree. `stagingSeed` re-verified HELD
  (`d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`).

Re-verified after the merge, all clean:

- **Angle-cast unit byte identity**: re-checked against the NEW
  `origin/main` tip (`c712ad7`) — still MATCH, sha256
  `6690091a19dcf7e27ddb63fcbd3d65052ce2593b7597d9c265416a730503a3cb`.
- **`csa-daniel`/`csa-smith` values survived the merge intact**:
  `csa-daniel` morning-end still `(4900,5990,facing 140,strength 1378)`;
  afternoon `t=0`/`t=10800` still the authored transition; `csa-smith`
  still `660` at every keyframe in both files.
- **Master table / overlay** regenerated again on the merged tree:
  `build_unit_audit.py` (unchanged row/column counts vs this pass's own
  run, §7 — the merge's own files carry no strength/composition change
  this table tracks), `gen-command-overlay.py` (214 mapped, 0 unmapped,
  byte-identical, no diff).
- **Suites, re-run on the merged tree** (new floor after the merge —
  `retrograde-facing`/`map-furniture` each added tests of their own):

  | Suite | Post-merge floor | This wave (post-merge) |
  |---|---|---|
  | tool vitest | 119 | **119 passed, 0 failed** |
  | pipeline pytest | 59 | **59 passed** |
  | reconstruction pytest | 132 (retrograde-facing) + map-furniture's own additions | **148 passed, 1 skipped** |
  | Unity EditMode | 405+4 (pre-merge) + map-furniture's additions | **429 passed, 0 failed, 4 skipped** |
  | Unity PlayMode | 21 | **21 passed, 0 failed, 0 skipped** |

  No sibling Unity process on the machine during any post-merge run
  (checked via `ps aux` immediately before each invocation).

## 13. Owner questions

1. **`csa-oneal`'s thin EC6** (§3) is now flagged at its SECOND file
   boundary (July 1→2 in `strength-reconciliation-1`, July-3-morning→
   afternoon here) with the identical underlying gap both times — worth
   prioritizing the B&M purchase or a targeted Pfanz fetch specifically
   for this brigade before a third boundary recurs?
2. **`csa-smith`'s structural fix** (§4 refinement note) — re-deriving
   `gettysburg-july2-evening.json`'s placeholder from the tablet's ~800
   Present figure directly would retire the double-count risk at its
   root rather than patching it downstream each time a new phase reads
   that file. Worth a small dedicated pass, or fold into the next
   reconciliation wave?
3. **`us-colgrove`'s composition gap** (§1.2) — `gettysburg-july3.json`
   still renders Colgrove's brigade as one undecomposed unit while the
   morning phase (and ED-76 doctrine generally) has moved to parentless-
   sibling decomposition. Worth a small decomposition-wave-style pass on
   the afternoon file specifically for this unit, or is single-file
   composition divergence acceptable indefinitely for non-Angle-cast
   units?
