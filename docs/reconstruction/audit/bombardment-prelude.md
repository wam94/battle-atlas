# Bombardment prelude — the residual pre-charge cannonade re-timing (P1)

**Branch:** `bombardment-prelude` (unmerged; owner gate) · **Script:**
`tool/scripts/author-bombardment-prelude.ts` (committed deterministic
derivation record, the `author-*`/A1/A2 pattern) · direct edit of
`app/Assets/Battle/gettysburg-july3.json` · 2026-07-16

Executes proposal P1 from `docs/reconstruction/audit/charge-intensity-
proposals.md` §4 ("Bombardment casualty prelude") against its own §1
pacing evidence: "the ~1-hour bombardment (Garnett ~20 k&w / ~2% of
strength, including a named officer's death; Kemper worse but uncounted)
is **entirely unrendered** — pre-subtracted into the starting headcount
with zero visible cause."

## 0. Summary

Most of the assault column's bombardment share was **already**
time-authored by prior passes (`day-expansion-1`'s ED-46/47/48 re-basing,
`authoring-wave-a2`'s Davis PRIMARY). The residual — three non-cast
brigades whose bombardment keyframe still carried the file's original,
pre-authoring generic placeholder citation, verbatim, unchanged since
before any wave touched it — is fixed here:

- **`csa-marshall`** (+ children `csa-11nc`/`26nc`/`47nc`/`52nc`)
- **`csa-brockenbrough`** (+ wings `csa-brock-left`/`csa-brock-right`)
- **`csa-lane`**

Three Angle-cast brigades (`csa-kemper`, `csa-armistead`, `csa-fry`) carry
the exact same unfixed placeholder but are film-pinned — **deferred to
film-v2 scope**, not touched, per the hard film-safety rule. `csa-garnett`
(cast) was already fixed by `day-expansion-1`'s ED-46. Four more non-cast
brigades (`csa-davis`, `csa-lowrance`, `csa-wilcox`, `csa-lang`) were
already properly time-authored by earlier passes and are untouched here.

## 1. Inventory — pre-subtracted vs. time-authored, before this pass

| Unit | Cast? | Bombardment-window (t=0→7200) status before this pass | Basis |
| --- | --- | --- | --- |
| `csa-garnett` | **YES** | **Already fixed** (`day-expansion-1`, ED-46) | Peyton: "During the shelling, we lost about 20 killed and wounded" (incl. Lt. Col. Ellis k) — PRIMARY, −20 exact |
| `csa-kemper` | **YES** | **Residual** — generic placeholder, unfixed | No brigade-specific figure; Wilcox's comparison ("suffered severely") only sizes it as worse than Garnett's ~20, not a total (V&R §2: qualitative comparison ≠ invented number) |
| `csa-armistead` | **YES** | **Residual** — generic placeholder, unfixed | No brigade-specific figure; second line, "Alexander 1907" only |
| `csa-fry` | **YES** | **Residual** — generic placeholder, unfixed | No brigade-specific figure |
| `csa-marshall` | no | **Residual → FIXED this pass** | Division record only (or-27-2-davis-jr): timing, no brigade figure |
| `csa-brockenbrough` | no | **Residual → FIXED this pass** | Division record only (or-27-2-davis-jr): timing, no brigade figure ("no brigade figure (open)", dossier EC5) |
| `csa-lane` | no | **Residual → FIXED this pass** | Division record only (or-27-2-engelhard): "the two hours' exposure", no brigade figure |
| `csa-davis` | no | Already fixed (`authoring-wave-a2`) | or-27-2-davis-jr: "In Davis' brigade 2 men were killed and 21 wounded" — PRIMARY, −23 exact |
| `csa-lowrance` | no | Already fixed (`day-expansion-1`) | or-27-2-lowrance: "at least one hour, under a most galling fire of artillery" — no figure, −8 inferred (≈2% class), but properly cited and timed |
| `csa-wilcox` | no | Already fixed (`day-expansion-1`) | or-27-2-wilcox: "suffered comparatively little, probably less than a dozen men killed and wounded" — bounded, cited |
| `csa-lang` | no | Already fixed (`day-expansion-1`) | or-27-2-lang: bombardment duration attested, "no brigade bombardment casualty figure exists (dossier EC5.1 open); decay held near-flat" — honest zero, cited |

"Residual" here means specifically: the t=7200 keyframe's citation still
read `"interpolated: [line|second line] static under bombardment
(Alexander 1907)"` — the file's own default boilerplate for this class of
row before any authoring wave touched it (confirmed against
`author-dayexp1-rebase.ts`'s own header, which explicitly deferred
Marshall as "Residual for slice 2" and never touched Brockenbrough's or
Lane's t=7200 citation even while re-deriving their *values*). Garnett,
Davis, Lowrance, Wilcox, and Lang all had this same text superseded by a
real citation in a prior pass; Kemper, Armistead, Fry, Marshall,
Brockenbrough, and Lane did not, as of this pass's start.

## 2. What changed

Per unit: two new keyframes inside the attested cannonade window, the
existing t=7200 keyframe's **citation** upgraded (its **value** is
UNCHANGED — this is a re-timing, not a re-basing), and t=7500 (step-off)
onward untouched.

| Unit | t=0 | t=420 (NEW, 13:07 signal) | t=3600 (NEW, ~14:00 midpoint) | t=7200 (value unchanged, citation re-timed) | t=7500 (untouched) |
| --- | --- | --- | --- | --- | --- |
| `csa-marshall` | 900 | 900 | 893 | 887 | 884 |
| `csa-brockenbrough` | 880 | 880 | 876 | 873 | 870 |
| `csa-lane` | 1355 | 1355 | 1347 | 1340 | 1338 |

Children/wings re-derive via `round(parentAt(t)/N)` — the SAME formula
`author-dayexp1-rebase.ts`/`author-ed75-placeholder.ts` already used for
these exact families (N=4 for Marshall's four regiments, N=2 for
Brockenbrough's two wings) — not a new convention.

**t=420 (13:07, CA-J3A-1 signal guns):** strength unchanged from t=0 — no
loss in the instant the guns open; the keyframe exists only to pin the
attested start-of-cannonade moment onto the clock (Alexander 1907's ~1:00
p.m. / Jacobs 1864's more precise 1:07 p.m., both already present on
every one of these brigades' own t=0 citations; for Marshall, Jones's own
+65-profiled "about 12 o'clock ... our batteries opened" corroborates the
same real moment from the brigade's own regiment-grain report).

**t=3600 (mid-bombardment, ~14:00):** the ALREADY-ESTABLISHED total loss
for t=0→7200 (13/10/17 men respectively — unchanged from the pre-pass
build) is split evenly, landing here at the midpoint. No brigade-specific
casualty figure exists for any of the three (Marshall/Brockenbrough:
Heth's division record, or-27-2-davis-jr, "About 1 p.m. the artillery
along our entire line opened ... For two hours the fire was heavy and
incessant ... The artillery ceased firing at 3 o'clock" — division-level
timing only; Lane: Pender's/Trimble's division record, or-27-2-engelhard,
"suffering no little from the two hours' exposure to the heavy artillery
fire which preceded the attack" — same class of gap). The even split is
the SAME compilation-class technique the corpus already uses when a
shared/unattributed loss must be distributed without inventing a number —
`decomposition-wave-1`'s 6th Wisconsin, the 21st Mississippi's 1,598/4
rounding (`residuals-decomp-2` task 3a) — disclosed as `inferred`
confidence, not promoted to `documented`.

**t=7200 (order to advance, ~15:00):** value untouched (887/873/1340,
byte-identical to the pre-pass build); only the citation is replaced,
from the generic `"interpolated: ... (Alexander 1907)"` placeholder to
one that names the actual division-record evidence, the CA-J3A bracket,
and states explicitly that the total is unchanged from the prior build.

**t=7500 and everything after: byte-identical, not touched at all** — see
§4.

No total was invented for any of the three brigades. The only numbers
moved are the ones the file already carried (13/10/17 men); their timing
now traces to the CA-J3A chain and the two cited division reports instead
of a copy-pasted default label.

## 3. Why Kemper/Armistead/Fry are deferred, not fixed

All three carry the identical unfixed generic placeholder at t=7200
("interpolated: [line|second line] static under bombardment (Alexander
1907)") and are, evidentially, the SAME class of gap as Marshall/
Brockenbrough/Lane. They are not touched here because they are three of
the 13 Angle-cast units, and the hard film-safety rule requires their
keyframes in `gettysburg-july3.json` to stay byte-identical, full stop —
re-timing their bombardment share (even only before t=8160, even only a
citation edit) would violate that pin. Per the task brief's own
instruction, this is recorded as film-v2 scope, not built:

- **`csa-kemper`**: the one candidate with a *qualitative* comparison
  available (Wilcox, watching from Kemper's right rear: "The brigade
  lying on my right (Kemper's) suffered severely" during the cannonade —
  `or-27-2-wilcox`, cited `csa-1c-pic-2-kemper.md` EC5.1) — but V&R §2
  forbids converting "worse than Garnett's ~20" into an invented total;
  the existing −25 interpolated placeholder already happens to exceed
  Garnett's −20, so no value change would even be defensible without a
  real figure.
- **`csa-armistead`**: no comparative or absolute figure located in this
  pass; second-line brigade, "Alexander 1907" only.
- **`csa-fry`**: no comparative or absolute figure located in this pass;
  `day-expansion-1`'s own header explicitly left Fry's value unchanged
  "(Angle-cast: the shipped film compiles from this track)" while citing
  every OTHER brigade's confirmed basis — the same posture is preserved
  here.

If a future film revision (a new Soldier View render, a new bundle slice,
or an owner ruling to re-cut the pinned window) ever reopens
`gettysburg-july3.json`'s cast keyframes, these three become the next
pickup — same CA-J3A/t=420/t=3600 pattern, contingent on locating (or the
owner accepting an honest "held near-flat, no figure" posture matching
Lang's own precedent) a brigade-specific figure.

## 4. Film safety — verdict: FILM-SAFE

**Verdict: FILM-SAFE.** Three independent checks, matching
`residuals-decomp-2`'s own methodology:

### 4a. Angle-cast unit byte identity

The 13 cast units (`csa-garnett`, `csa-kemper`, `csa-armistead`,
`csa-fry`, `us-webb`, `us-69pa`, `us-71pa`, `us-72pa`, `us-btty-cushing`,
`us-btty-cowan`, `us-btty-arnold`, `us-hall`, `us-stannard`), extracted
(sorted-key JSON) from this worktree's `gettysburg-july3.json` before and
after the authoring script ran: **sha256
`27273effa17fc2c35130a853c1c15f384b28ff4810cb5a8015ba7e9f3491ab56`** —
MATCH, byte-identical (also asserted inside the authoring script itself,
which throws if any cast unit's serialized JSON changes at all).

### 4b. Per-second compiled-state comparison, t=8160..8820, all touched units + all 13 cast units

Linear-interpolated `(x, z, facing, strength, formation)` computed
independently in Python for every second from 8160 to 8820 inclusive
(661 seconds), for the 9 touched units (`csa-marshall` + 4 children,
`csa-brockenbrough` + 2 wings, `csa-lane`) and the 13 cast units, compared
between the pre-pass (`git show HEAD:...`) and post-pass files:
**22 units × 661 seconds = 14,542 state comparisons, 0 mismatches.**
This is the direct proof of "moving WHEN, never HOW MUCH": nothing this
pass touched produces a different value anywhere at or after the film
window's start, because nothing at t≥7500 was touched at all (§4c).

### 4c. Structural containment — nothing at t≥7500 was touched

The authoring script diffs each touched unit's `keyframes.filter(t =>
t >= 7500)` before and after, and throws if that slice changed at all.
It didn't: the new keyframes (t=420, t=3600) sit strictly inside
[0, 7200); the existing t=7200 keyframe's `strength`/`x`/`z`/`facing`/
`formation` fields are untouched (only `citation` changed — not a
compiled-state field); t=7500 and every later keyframe for all 9 touched
units is byte-identical, verified by direct JSON-string comparison in the
script (not just spot values). This is a stronger guarantee than the
per-second sampling in §4b — the sampling is a proof-by-construction
sanity check, not the load-bearing one.

### 4d. Bundle recompile

`reconstruction/scripts/compile_angle.py` re-run after the edit: every
field (`units`, `claimsIndex`, `slice`, `format`, `note`, `clock`,
`stagingSeed`) byte-identical to the pre-edit bundle, EXCEPT
`inputs.battle` (the whole-file sha256 of `gettysburg-july3.json`,
expected — non-cast units changed: `1f57b8c7...` → `d7b45a3c...`) and the
top-level self-referential `checksum` (`47c6f183...` → `e529c74a...`).
`stagingSeed` HELD: `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`.

### 4e. Diff scope

Exactly 9 unit records changed in `gettysburg-july3.json`: `csa-marshall`,
`csa-11nc`, `csa-26nc`, `csa-47nc`, `csa-52nc`, `csa-brockenbrough`,
`csa-brock-left`, `csa-brock-right`, `csa-lane`. No other unit, no
top-level battle field (`startTime`/`endTime`/`environment`/`events`),
changed at all.

## 5. Regeneration

- **`reconstruction/scripts/build_unit_audit.py`** re-run: **340 rows**
  (unchanged — no new unit ids, only keyframe counts increased by 2 for
  the 9 touched rows: Marshall 22→24, its 4 children 9→11 each,
  Brockenbrough 19→21, its 2 wings 7→9 each, Lane 21→23).
  `unit-master-table.csv`/`.xlsx` regenerated; diffed — only those 9 rows'
  `Keyframes`/`KF inferred` columns changed, nothing else.
- **`scripts/gen-command-overlay.py`** re-run: **215 mapped, 0 unmapped**
  (unchanged — no new unit ids introduced by this pass).
- **`reconstruction/scripts/compile_angle.py`** re-run: metadata-only
  recompile, verified §4d.
- **Retrograde-facing fixer** (`reconstruction/scripts/
  fix_retrograde_facing.py`), report mode, run against the full corpus
  after the edit: **zero new violations** — `converted=0` across every
  phase file; the pre-existing `preserved=2`/`deferred=6` entries in
  `gettysburg-july3.json` (the two attested about-faces, the six
  Angle-cast-pinned legs) are unchanged, none touching any unit this pass
  edited.
- **`docs/reconstruction/audit/oob-register.json`/`.md`** — not
  regenerated/edited: no new unit ids, matching the established
  convention for a pure re-timing pass.

## 6. Suites

| Suite | Floor (current main) | This pass |
|---|---|---|
| tool vitest | 119 | **119 passed, 0 failed** (no test file touched; no test asserts Marshall's/Brockenbrough's/Lane's exact keyframe array beyond the existing membership/regiments/decomposition checks, all of which still pass) |
| pipeline pytest | 66 | **66 passed** |
| reconstruction pytest | 158 + 1 skipped | **158 passed, 1 skipped** (unchanged — no `reconstruction/` schema/validator source touched) |
| Unity EditMode | 437 total | **433 passed, 0 failed, 4 skipped** (no C# touched; this is current main's own suite reacting to the data file, unaffected) |
| Unity PlayMode | 21 total | **21 passed, 0 failed, 0 skipped**, on a clean re-run — see note below |

**PlayMode flake, disclosed:** the first PlayMode run (system under load
from concurrent Unity batch processes belonging to other, unrelated
worktree sessions on this machine — `ps aux` showed `webb-cushing` and
`wt-fight-prone` batch processes running throughout this pass) reported
20 passed / 1 failed: `SoldierViewPlayerSyncTests.Seek_OutsideWindow_
ClampsToDecodableRange`, "end clamp: drift 3.00 frames" — a video-decode
frame-timing tolerance test entirely unrelated to this pass (no
SoldierView/video/C# file was touched). A clean immediate re-run (same
worktree, same build) passed 21/21, confirming the failure was a
system-load timing flake, not a regression. Both result files are kept in
the worktree.

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`, this
worktree's own `Library`; gitignored inputs restored from the main
checkout before any run — `data/heightmap`, `data/dem_cache`,
`data/landcover/{fences,relief,relief_contours,splatmap}.{json,png}`/
`trees.json` (the tracked `landcover.json`/`oakridge.landcover.json`
needed no restore), `app/Assets/Generated`, the SoldierView `.mp4` media;
staging via `CartographyStage.PrepareScene` — NOT
`Phase12Review.PrepareStandaloneScene` — run once before any test/build;
no `-nographics` flag. `Atlas.unity`'s fileID churn from `PrepareScene`'s
re-import was reverted before commit — a staging byproduct, not a data
change, matching the established convention of not committing it.)

## 7. Perf

`gettysburg-july3.json` re-benchmarked (`BenchmarkHarness`,
`-benchmark`/`-benchmarkTimes`/`-benchmarkPrefix`/`-battleFile`, default
camera, matching every prior wave's own methodology), sampled at
t=0/420/3600/7200/7500/7800/8100/8160/8700/9000/10800 (the whole
bombardment-through-repulse span, the Angle film window explicitly
included, 197 units):

| t | avg FPS | p95 frame (ms) | worst frame (ms) |
|---|---|---|---|
| 0 | 59.6 | 17.39 | 23.41 |
| 420 | 59.7 | 17.16 | 22.35 |
| 3600 | 59.6 | 17.35 | 21.84 |
| 7200 | 59.6 | 17.32 | 22.70 |
| 7500 | 59.7 | 17.18 | 22.88 |
| 7800 | 59.7 | 17.10 | 23.51 |
| 8100 | 56.9 | 17.26 | 242.52 (one-off spike; avg unaffected) |
| 8160 | 59.6 | 17.14 | 32.63 |
| 8700 | 59.5 | 17.08 | 33.04 |
| 9000 | 59.7 | 17.15 | 22.83 |
| 10800 | 59.7 | 17.14 | 22.69 |

Matches the established ~59.5 avg-FPS floor throughout, including through
the newly-authored t=420/3600 window and the untouched film window —
this pass's two new keyframes per unit (18 keyframes total, no new
render-cost geometry) cost nothing measurable.

## 8. Evidence

`docs/benchmarks/captures/bombardment-prelude/` (force-added; build/
prepare/test logs and raw XML test-result files kept in the worktree,
gitignored by design, not committed — matching `residuals-decomp-2`'s own
convention):

- **`bp-before/after-woods-t{7500,7800,8100}.png`** and
  **`bp-before/after-atlas-t{7500,7800,8100}.png`**: the literally
  requested capture times, close-cam (pivot 3200,5500, dist 2200,
  Spangler's Woods) and wide default-camera (with the HUD timeline)
  respectively. Pixel-diff (numpy, threshold 10/channel-sum, matching
  `residuals-decomp-2`'s own methodology): **0 of 1,600,000 px differ at
  any of the 6 before/after pairs** — this is the EXPECTED and CORRECT
  signature of the "moving WHEN, never HOW MUCH" design: by t=7500
  (step-off) the touched brigades' strength has already converged back to
  exactly what the file carried before this pass, so a snapshot at or
  after step-off is byte-identical by construction. This is not a null
  result — it is the film-safety-adjacent property from §4c, independently
  re-confirmed visually.
- **`bp-before/after-thinning-t{0,420,3600,7200}.png`** (same close cam):
  the times where the actual re-timed data lives. t=0: 0 px differ
  (expected — the arrival strength is untouched). t=420: 120 px differ (a
  small HUD label-text region — the new signal-guns keyframe registers as
  new "nearby" content to the label system even though strength is
  unchanged). **t=3600 and t=7200: >1,000,000 of 1,600,000 px differ** —
  the Atlas's camera framing and division/corps label set both visibly
  change once Marshall's/Brockenbrough's/Lane's own new keyframes put
  live content at these timestamps, where the pre-pass file had none (a
  bare 2-point straight-line interpolation, no keyframe boundary at all).
  Determinism re-verified independently: re-running the SAME (before)
  battle file twice at t=7200 produces a byte-identical screenshot
  (`bp-before-thinning-rerun-t7200.png`, 0 px diff against the first
  before-run) — ruling out engine/GPU non-determinism as the explanation;
  the difference is a genuine, deterministic reaction to this pass's data.
  The exact mechanism (camera auto-frame and/or label-declutter priority
  keying off "a keyframe boundary exists near the current clock time" for
  a unit) wasn't traced to source in this pass — flagged as an owner
  question below, since it means the Atlas UI itself surfaces the
  newly-authored bombardment activity beyond just the roster's own dot
  density, which is a bonus beyond what this pass set out to prove but is
  worth understanding if the owner wants to rely on it deliberately.
- **`bp-perf-j3a-*.png`/`bp-perf-j3a-benchmark.json`**: §7's perf sample,
  default camera, 11 timestamps.

## 9. Owner questions

1. **Kemper/Armistead/Fry's bombardment share** (§3): the identical
   generic-placeholder gap exists on three Angle-cast brigades, correctly
   deferred here per the film-safety rule. Worth flagging for a future
   film-v2 pass (a new Soldier View render / bundle re-slice) as the
   direct next pickup for this exact class of fix — Kemper in particular
   has a real (if non-numeric) primary ("suffered severely") that a
   future pass could size against Garnett's ~20 the same way this pass
   sized Marshall/Brockenbrough/Lane's shares, once the film-pin
   constraint is lifted or an owner ruling accepts an in-window re-render.
2. **The camera/label response at t=3600/7200** (§8): a real, deterministic,
   large-magnitude Atlas UI reaction to the new keyframes that this pass
   didn't need to trace to its source (`AtlasHud.cs`/`BattleDirector.cs`'s
   label-declutter and/or camera auto-frame logic) to satisfy the film-safety
   and suite obligations. Worth a dedicated look if the owner wants to
   understand or deliberately tune how the Atlas highlights "active" brigades.
3. **The two remaining true-unattributed gaps** (Marshall/Brockenbrough/
   Lane's OWN bombardment casualty counts, as opposed to their
   division's): none of the three has ever had a brigade-specific figure
   located in any pass to date (this one included). If a Busey & Martin
   fetch or a regimental-grain source ever surfaces one, replacing the
   even-split `inferred` midpoint with a `documented` one would be a
   clean, contained follow-on — same shape as this pass, one number
   swapped in.

No new EDs proposed — this pass re-times, it doesn't re-base; the
existing ED-46/47/48 (`day-expansion-1`) and the wave-A2 Davis primary
already cover the adjacent "what's the total" question for their own
units, untouched here.
