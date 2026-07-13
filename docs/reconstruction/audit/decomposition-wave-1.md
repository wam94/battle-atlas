# Decomposition wave 1 — the July 1 regiment promotions + the Ziegler's Grove convergence

**Branch:** `decomposition-wave-1` (unmerged; coordinator gate) ·
**Scripts:** `tool/scripts/author-decomp1-regiments.ts` (the three July 1
regiment promotions, both phase files), `tool/scripts/author-decomp1-
zieglers-grove.ts` (the four Ziegler's Grove batteries, July 3) ·
2026-07-13

The first decomposition wave (day-expansion-slice-3.md §9 pickup 1 / §7
item 5): promotes three July 1 regiments whose episodes were already
authored verbatim inside their brigades' own citations to first-class
tracked units, and lifts authoring-wave-a2.md's no-new-units constraint for
exactly the four batteries its own report named as "cheap to author the
moment new units are allowed" (the Ziegler's Grove convergence).

## 1. The design call: parentless siblings, not `parent`/family

The wave brief and the slice-2/slice-3 reports describe this class of work
via the Harrow precedent (slice 2: regiments as first-class child units
with `parent`, family-atomic LOD, strength conservation). Investigating the
actual rendering contract before authoring changed the plan:

`BattleDirector.cs`'s family-suppression contract (`RendersAtTier`,
`battle-format.md` "Parent / children") hides a decomposed brigade's
PARENT symbol at every LOD tier nearer than Block once it has ANY
children — the whole family swaps atomically, evaluated once from the
parent's center. This is correct and intended for the precedents that use
it: Harrow (4/4 regiments modeled), Webb/Stannard/Hall (Task A5: 3/4, 5/5,
5/5 — Webb's 106th PA is the only unmodeled slice, ~16% of the brigade,
the source of the validator's known ±15% advisory warning). A
single-regiment promotion out of a five- or six-regiment brigade is a
different shape of fact: giving the 6th Wisconsin (1 of 5) a `parent` link
to Meredith's brigade would suppress the OTHER FOUR regiments — roughly
80% of the Iron Brigade — from view at the Regiments and Soldiers LOD
tiers, a real visual regression the wave brief did not ask for and the
dossiers do not support (none of the other four regiments has its own
track; they would simply vanish).

The codebase already has the right precedent for this exact shape:
`gettysburg-july3.json`'s `us-carroll` ("Carroll's Brigade... minus the 8th
Ohio") and `us-8oh` (the 8th Ohio, promoted from Carroll's brigade,
**parentless**, its own citation reading "NO parent link (family rules:
none, divisions not modeled)"). This wave applies the SAME pattern to all
three July 1 promotions and reads it, not the Harrow/A5 precedent, as the
applicable doctrine for single-regiment-out-of-an-unmodeled-brigade
promotions. **Proposed ED-76** (below) records this as a doctrine
candidate for the owner; nothing here self-adopts it.

Each promoted regiment is a new parentless `Unit`. Each source brigade's
aggregate unit is renamed "... minus the Nth ___" (the `us-carroll`
convention) and its OWN strength track is reduced by the promoted
regiment's figure at every one of the brigade's own keyframe times —
**exact subtraction**, not the `parent`/children ±15% advisory tolerance
(`tool/src/validate.ts`'s `StrengthSumTolerance`, which only fires for
`parent`-linked families and therefore never triggers on this wave's
units — correctly: there is no family to check).

## 2. The three July 1 regiment promotions

| Unit | Parent brigade (renamed, residual) | Episode | Basis |
|---|---|---|---|
| `us-6wi` — 6th Wisconsin | `us-meredith` ("... minus the 6th Wisconsin") | The railroad-cut charge (detached corps reserve) | `reconstruction/dossiers/us-i-1-1-meredith.md`; or-27-1-dawes-6wi; or-27-1-cutler (both-sided); or-27-1-union-return p.173 |
| `us-147ny` — 147th New York | `us-cutler` ("... minus the 147th New York") | The stranded stand at the railroad cut | `reconstruction/dossiers/us-i-1-2-cutler.md`; or-27-1-cutler (own regiment primary, rate-class loss) |
| `us-16me` — 16th Maine | `us-coulter` ("... minus the 16th Maine") | The sacrifice-hill rear-guard order | `reconstruction/dossiers/us-i-2-1-paul.md`; or-27-1-farnham-16me (sacrifice order verbatim, the day-total loss primary) |

All three authored in BOTH `gettysburg-july1-morning.json` and
`gettysburg-july1-afternoon.json` (each regiment's own episode plus its
continuation through the retreat/Culp's Hill convergence); cross-phase
continuity verified by hand in the derivation script (the dayexp3-
afternoon.ts convention, applied to a patch script rather than a full-file
constructor) — printed and asserted: **"continuity: us-6wi, us-147ny,
us-16me all carry exactly from morning end to afternoon t=0."**

### 6th Wisconsin (`us-6wi`)

Detached as corps reserve with the 100-man brigade guard at deployment
(Doubleday's order, or-27-1-dawes-6wi), held back from Meredith's own
Willoughby Run charge, ordered up to the fence line as Cutler's right
wrecks, the railroad-cut charge (event `j1m-rrcut-charge`'s own drawn
coordinates, 3400,7690 → 3560,7720) with the 2nd Mississippi's surrender to
Dawes, re-advance to the crest with Cutler's released regiments, then
through the afternoon with Cutler's detachment before separating to rejoin
its own brigade at Culp's Hill — "reporting to Col. W. W. Robinson, now
commanding the brigade" (or-27-1-dawes-6wi verbatim, the Meredith
succession pin). Strength: no regiment-grain primary exists (dossier
Conflicts item 4: "only the 24th's 496 fetched"); t=0 authored at 333
compilation-class FLAGGED (1,829 brigade total minus the 24th Michigan's
own 496, split evenly across the four other un-primaried regiments). The
cut-charge loss is a single documented spike (Dawes: "not less than 160...
in the charge"; his own itemized total 158; the union return's regiment row
168 ADOPTED, un-averaged) — 333 → 165, held flat for the rest of the day
(no further loss documented, not invented).

### 147th New York (`us-147ny`)

The audit's cleanest single-regiment episode: a real regiment-grain
strength primary (380, or-27-1-cutler) AND its own drawn position (the
j1-03 "147 N.Y" bar, 3170,7528 — already cited on `us-cutler`'s own t=9000
keyframe before this wave). Lt. Col. Miller wounded "at the moment of
receiving" Wadsworth's fall-back order; Maj. Harney "held the regiment to
its position until the enemy were in possession of the railroad cut on his
left" — the position keyframe is UNCHANGED at the stand (the whole point:
it does not fall back with the rest of the brigade). Rate-class loss
primary, exact: "207 out of 380 men and officers within half an hour" — 380
→ 173. No further per-regiment loss documented for the rest of the day
(held flat).

### 16th Maine (`us-16me`)

The regiment's only hard EC6 number is its own casualty total (223, k+w+m,
or-27-1-farnham-16me) — its engaged strength has NO primary anywhere in the
corpus; the dossier's own 275 literature-class figure is explicitly flagged
"NOT hopped... no primary located this pass" (`reconstruction/dossiers/
us-i-2-1-paul.md` EC2), carried here as the most heavily flagged strength
row this wave authors. "THE SACRIFICE ORDER: ordered, alone, by General
Robinson, to take possession of a hill which commanded the road, and hold
the same as long as there was a man left" (or-27-1-farnham-16me verbatim) —
no drawn geometry or named landmark exists for the hill; position placed at
the division's own retreat-corridor ground (Coulter's own t=10800
coordinate), radius wide-open, an honest EC3 gap recorded on the unit.
"THE THREE-STAGE COLLAPSE: hollow → woods → 'ordered a retreat, but not in
time to reach the main body'" — the missing column (162 of the primary's
223) IS the sacrifice hill's capture pocket. 275 → 52 in one spike (a
compounded-uncertainty flag carried forward: 275 is not a primary, 223 is).

## 3. Strength-conservation evidence (the invariant, checked three ways)

Every keyframe pair (promoted regiment + residual brigade) sums back to
the pre-decomposition build value AT EVERY SHARED TIMESTAMP, by
construction (exact subtraction, not a tolerance band):

- **147th NY / Cutler**: morning t=0 173+... — full check: 380 (147th) +
  1,220 (residual) = 1,600 = the pre-decomposition build's own t=0.
  Afternoon end: 173 + 777 = 950 = the pre-decomposition build's own
  t=18000 EXACTLY. A bonus cross-check landed in the citations: the
  original combined −450 delta at the stranding (t=9000→11400) splits into
  the 147th's own evidenced −207 and the residual's own −243 — matching
  the SAME report's other two regiment-grain loss primaries in that
  window almost exactly ("76th NY... 169 within thirty minutes" + "56th
  Pa... 78+" = 247 vs the arithmetic residual's 243, a 4-man agreement).
- **16th Maine / Paul's (Coulter)**: morning t=0: 275 + 1,262 = 1,537 = the
  pre-decomposition build's own t=0. Afternoon end: 52 + 709 = 761 = the
  pre-decomposition build's own t=18000 EXACTLY — and the brigade's OWN
  day-split primary (or-27-1-coulter's table: July 1 = 776) is preserved
  by construction: the 16th Maine's own day loss (275−52=223, its own
  primary) plus the residual's own day loss (1,262−709=553) sum to
  **776**, exact, the wave's cleanest reconciliation.
- **6th Wisconsin / Meredith**: conserves exactly at t=0 (333+1,496=1,829)
  and through t=11700 (333+1,417=1,750), but NOT at the pre-decomposition
  build's own afternoon end (165+417=582 vs the old build's 700). This is
  a flagged, deliberate CORRECTION, not a silent edit: the old
  undifferentiated compilation's own t=13500 checkpoint (1,700) implicitly
  assumed the reserve detachment held near-full strength through that
  point — it never separately accounted for a detached reserve regiment's
  own rate-class cut-charge loss, because the regiment wasn't separately
  tracked. Once 6th Wisconsin's own evidenced spike (333→165) is
  known, the combined total at that instant is properly 1,417+165=1,582,
  not 1,700 — a 118-man correction, applied at t=13500 and carried flat
  through the rest of the day (both files' citations state the correction
  explicitly; class: ED-32/ED-46, the standing evidence-layer-vs-build
  pattern, not a new ruling).

## 4. The Ziegler's Grove convergence (July 3)

`authoring-wave-a2.md` §2 cut 1's headline cut, executed per its own
recorded pickup: "author the four as units, in-window legs triggered at
the repulse (t≈9000+), muzzles cold, with the Williston conflict carried."

| Unit | Strength | Guns | Fire authored? |
|---|---|---|---|
| `us-btty-williston` (D, 2nd US) | 109 | conflict carried: "four light 12 pounders" (own tablet) vs six 10-pdr (brigade tablet/cross-check) — NOT adjudicated | NO — "Not engaged" (own tablet) |
| `us-btty-butler` (G, 2nd US) | 113 | six 12-pounders | NO — "no fire narrative located" |
| `us-btty-martin-5us` (F, 5th US, Leonard Martin's — distinct from Kinzie's XII Corps battery of the same name) | 125 (3 off + 122 enl.) | six 10-pdr Parrotts | NO — "no fire narrative located" |
| `us-btty-harn` (3rd NY) | 119 | six 10-pdr Parrott rifles | NO — "no engagement narrative located" beyond "ordered to support" |

All four: parentless (batteries never carry `parent` in this build); t=0
at the VI Corps Artillery Brigade's reserve ground ("South of Gettysburg,
east side of Sedgwick Avenue, near the George Weickert farm," brigade
tablet verbatim, `reconstruction/dossiers/us-vi-arty-tompkins.md` EC3.2 —
the SAME ground already cited on `us-btty-mccartney`'s own t=0 keyframe,
4210,3070; each battery placed at a small distinct offset within the
documented ~±150 m radius); departure t=9000 (the repulse, no independent
clock exists for any of the four); arrival ~t=10500 rear of Woodruff's own
gun line (mon-woodruff 4502.6,5190.6), radius wide-open (no drawn geometry
exists); holds to the window's sunset end (t=23340). **No fire events
authored for any of the four** — all four dossiers independently record
either "Not engaged," "no fire narrative located," or an "ordered to
support" fact with zero casualties; muzzles stay cold, per the wave brief
and the counter-rule (inventing fire to visualize a move is forbidden).

**Williston's conflict, carried, not resolved**: his OWN individual
tablet states "placed in reserve" and "Not engaged" on Taneytown Road,
without repeating the Ziegler's Grove move; the companion Battery G
(Butler's) page states BOTH batteries (D and G) moved to Ziegler's Grove.
This wave adopts the move for Williston too (matching the A2 report's own
framing, "the Williston conflict carried" — carried ON the authored
reading, not used to withhold authoring) — the full conflict text, both
readings, sits on `us-btty-williston`'s own t=10500 keyframe citation.
Neither reading is adjudicated here.

## 5. Film-safety verification (the tripwire)

The July 3 additions recompile the Angle bundle. Verified three
independent ways, before writing any change:

1. **Source-file byte identity.** Before running the Ziegler's Grove
   script, the 13 Angle-cast unit ids (`csa-garnett`, `csa-kemper`,
   `csa-armistead`, `csa-fry`, `us-webb`, `us-69pa`, `us-71pa`, `us-72pa`,
   `us-btty-cushing`, `us-btty-cowan`, `us-btty-arnold`, `us-hall`,
   `us-stannard` — the same guard set `author-a2-dossier-placement.ts`
   enforces) were extracted from `gettysburg-july3.json` into a snapshot
   and sha256'd: `eda67dd9c88dda2a15cab286b24dafeb3adf6175d660af98114164228441db0c`.
   The script only calls `addUnitsAfter` (splice-insert after
   `us-btty-mccartney`, itself NOT one of the 13) — it never reads or
   writes any existing unit. After running, the SAME 13-unit extraction
   was re-hashed: **identical**, byte for byte (`diff` empty).
2. **Compiled bundle payload identity.** `reconstruction/scripts/
   compile_angle.py` was re-run after the battle-file change. The
   bundle's `units` field (the per-second t=8040..9000 simulation for the
   13 Angle-cast units) is **byte-identical** before/after (Python dict
   equality check on the parsed JSON). `stagingSeed` is unchanged
   (`d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1` —
   the ED-21 pin HELD). Only `inputs.battle` (the sha256 of the whole,
   now-larger battle file) and the derived top-level `checksum` changed —
   the exact metadata-only-recompile signature `authoring-wave-a2.md` and
   `authoring-wave-a1.md` both establish for this class of change. The
   regenerated `docs/reconstruction/angle-bundle-audit.md` confirms the
   same story at the audit-doc level: Units 13, Segments 53, Casualty
   profiles 40 — all unchanged; only the "Bundle checksum" line differs.
3. **Structural containment.** The Ziegler's Grove units' first
   documented activity (departure) is t=9000 — outside the film viewpoint
   window (t=8160..8820) by construction — and carries no fire event at
   any time. The July 1 regiment promotions touch a different battle file
   entirely (`gettysburg-july1-{morning,afternoon}.json`), which the
   Angle bundle compiler does not read.

**Verdict: FILM-SAFE.** No sub-task was reverted; nothing needed to be.

## 6. Proposed EDs (owner review — nothing here self-adopted)

### ED-76 (proposed) — Single-regiment promotions use the parentless "minus" convention, not `parent`/family

**Question:** when a wave promotes ONE regiment out of a brigade whose
other regiments stay at aggregate grain, should the promoted regiment get
`parent` (the Harrow/Webb/Stannard/Hall family mechanic) or be authored as
a parentless sibling (the Carroll's-Brigade/8th-Ohio pattern already
shipped in `gettysburg-july3.json`)?

**Finding:** `BattleDirector.cs`'s family-suppression contract
(`RendersAtTier`) hides the parent's symbol at every tier nearer than
Block once it has ANY children — appropriate when the children cover ALL
or NEARLY ALL of the brigade (Harrow 4/4; Webb 3/4, only ~16% unmodeled).
For a 1-of-5 or 1-of-6 promotion, `parent` would erase 80%+ of the brigade
from view at the Regiments/Soldiers tiers — a rendering regression, not a
data-honesty question, but one the format's own precedent (`us-8oh`,
already shipped, its own citation stating "NO parent link (family rules:
none, divisions not modeled)") already resolves correctly.

**Proposed rule:** author single-regiment (or small-minority-regiment)
promotions as parentless siblings; rename the source brigade "... minus
the Nth ___" and reduce its own strength track by EXACT subtraction (not
the `parent`/children ±15% advisory) at every shared keyframe time.
Reserve `parent`/family for decompositions covering all or nearly all of a
brigade's regiments.

**Applied this wave:** `us-6wi`, `us-147ny`, `us-16me` (§2); the
Meredith/Cutler/Paul(Coulter) aggregate units renamed and re-based
accordingly.

### ED-77 (proposed) — The Ziegler's Grove convergence: post-repulse legs authored, Williston's conflict carried on the authored reading

**Question:** authoring-wave-a2.md cut this content for want of new
units; this wave lifts that constraint. Should all four batteries'
post-repulse move be authored uniformly (including Williston's, whose own
individual tablet conflicts with the move), or should Williston be held
back at his own tablet's Taneytown Road reading pending resolution?

**Finding:** the A2 report's own recorded pickup framing ("author the
four as units... with the Williston conflict carried") reads as
authoring the move for all four, with the conflict text carried on
Williston's own citation — not as an instruction to exclude him. Both
other batteries' tablets (Butler's most explicitly, Harn's by the parent
dossier's four-battery grouping) independently corroborate the SAME
ground and trigger.

**Proposed rule:** adopted here (§4) — Williston's move is authored,
his own tablet's "Not engaged"/Taneytown-Road reading is carried in full
on the SAME keyframe's citation, neither reading adjudicated. No fire
event authored for any of the four batteries (matches every one of their
own dossiers: zero casualties, no fire narrative, or "Not engaged").

**Applied this wave:** `us-btty-williston`'s t=10500 keyframe citation
(§4).

## 7. Deferred (honest, out of scope this wave)

- **The slice-2 carryovers** (21st Mississippi from Barksdale's centroid;
  the 9th Massachusetts's two-section Trostle structure) — scope item 5,
  explicitly last-priority ("if capacity remains"). Not started: the
  wave's time went to the three regiment promotions (each requiring full
  cross-phase-continuity authoring across two files, not a single-file
  edit) and the Ziegler's Grove convergence's film-safety verification.
  Both carryovers are drawn/documented at sub-brigade grain already (per
  `day-expansion-slice-2.md` §7 item 4) and are the natural next pickup —
  same technique this wave establishes (parentless single-unit promotion
  where only a fraction of the parent is separately attested; ED-76's
  proposed rule would apply directly if the owner adopts it).
- **Heckman's Battery K, 1st Ohio** (the XI Corps's fourth battery,
  slice-3 §4 item 2) — no dossier read banked; out of this wave's scope
  (not named in the coordinator's brief).
- **Iverson's field T4→T5** and **the 26-row strength reconciliation** —
  explicitly out of scope per the coordinator's brief (film-class /
  next-dispatch work respectively).
- **The register's `stats` block was independently found stale** before
  this wave touched it (148/134 in-build/not-yet-cast vs a direct scan's
  288-entry total of 157/125 recorded in `oob-register.md` at the time)
  — corrected here to the accurate scanned totals (162/126, including
  this wave's 4 new register-castStatus rows), flagged in
  `oob-register.md` as a pre-existing bookkeeping drift, not an evidence
  gap. Not investigated further (out of this wave's scope).

## 8. Regeneration (deterministic, per the invariants)

- **Master table**: `reconstruction/scripts/build_unit_audit.py` re-run;
  195 in-build rows (up from 191 — the 4 new July 3 units; the three July
  1 regiment promotions do NOT appear as computed rows, a PRE-EXISTING
  limitation — the script only reads `gettysburg-july3.json`, not the day-
  expansion phase files, so no day-expansion-authored unit has ever
  appeared in the computed columns; not introduced or worsened by this
  wave). Consultation columns preserved (re-read-before-write, per the
  script's own design).
- **Command overlay**: `scripts/gen-command-overlay.py` re-run; **211
  mapped, 0 unmapped** (up from 208 mapped before this wave's `MANUAL_CHAINS`
  addition — the three new parentless regiments needed the SAME manual
  chain-override entry `us-8oh` already has, since they have neither a
  `parent` field nor their own register row; added: `us-6wi` → I Corps/1st
  Division, `us-147ny` → I Corps/1st Division, `us-16me` → I Corps/2nd
  Division, matching each promoted regiment's actual parent brigade's own
  register `parentChain`).
- **Register**: `docs/reconstruction/audit/oob-register.json` — four
  castStatus flips (`reg-us-vi-b3/b6/b7/b8` → `us-btty-harn` /
  `us-btty-williston` / `us-btty-butler` / `us-btty-martin-5us`); NO new
  rows for the three regiment promotions (matches the register's own
  documented convention: "In-build child regiments... are represented
  inside their brigade entry's regiments list, not as separate register
  entries" — `us-6wi`/`us-147ny`/`us-16me` join `us-8oh` in that class).
  `oob-register.md` updated to match (§7's stale-stats note).
- **Angle bundle**: `reconstruction/scripts/compile_angle.py` re-run;
  metadata-only recompile verified (§5).

## 9. Suites and evidence

Suites:

| Suite | Before (main) | After (this wave) |
|---|---|---|
| tool vitest | 118 | **119 passed, 0 failed** (one test updated: the pinned July-3 unit count 191 → 195, `tests/gettysburg.test.ts`) |
| pipeline pytest | 59 | **59 passed** |
| reconstruction pytest | 122 + 1 skip | **122 passed, 1 skipped** (unchanged — `test_committed_bundle_matches_recompilation` green after the metadata-only recompile) |
| Unity EditMode | 377 passed, 0 failed, 4 skipped | **377 passed, 0 failed, 4 skipped** (identical to the pre-wave baseline; an initial run with `-nographics` added spuriously failed one HDRP RenderTexture-size test unrelated to this wave's content — rerun without it, matching the task's exact specified command, reproduced the baseline exactly) |
| Unity PlayMode | 17 passed | **17 passed, 0 failed** (identical to baseline) |

Perf (`decomp1-after-*-benchmark.json`, screen 1600x1000, Development
build; `-benchmark`/`-benchmarkTimes`/`-benchmarkPrefix`/`-battleFile`, the
existing generic `BenchmarkHarness` — no new capture-harness C# was needed
for this wave):

- **July 3** (`decomp1-after-july3-benchmark.json`, t =
  0/3600/8160/8700/9000/10500/18000, 195 units — the film window
  8160/8700/9000 explicitly included): steady **59.5–59.7 avg FPS** at
  every timestamp, p95 frame 16.87–17.22 ms, allocations flat at
  ~323.6–323.8 MB — matches the pre-wave baseline range (59.6–59.9 avg
  FPS) exactly; the four new parentless batteries cost nothing measurable,
  including through the film window.
- **July 1 afternoon** (`decomp1-after-afternoon-benchmark.json`, t =
  0/3600/7200/9000/10800/12600/18000, 48 units): steady **59.6–59.7 avg
  FPS**, p95 frame 17.08–17.32 ms, allocations flat at ~321.6–321.9 MB —
  matches the day-expansion-3 baseline (59.6–59.9 avg FPS) exactly.

Evidence: `docs/benchmarks/captures/decomposition-1/` (force-added; owner
copies in the main checkout's same gitignored path) — BEFORE (pristine
`origin/main` battle files) and AFTER (this wave's edited files) screenshot
pairs at matched timestamps for all four episodes, plus the two perf
benchmark JSONs above and their per-timestamp screenshots. Produced by
`scripts/decomp1-captures.sh <before-dir>` (one Development build, six
`-battleFile` runs — three files × before/after, reusing the existing
generic benchmark harness rather than a new day-specific one).

Pixel-diff spot checks (both runs from this worktree + restored inputs,
threshold 10/channel-sum, `numpy`/`Pillow`): each before/after pair differs
only in a localized region matching the episode's own ground —
morning t=9000 (the opening line, 774 px, bbox ~(633,136)-(776,229) — the
147th NY's own drawn position now rendering independently), morning
t=12900 (the cut charge concluded, 82 px, bbox ~(592,126)-(742,185) — the
6th Wisconsin's own block at the cut), afternoon t=12600 (the 16th Maine's
collapse, 74 px, bbox ~(762,101)-(837,200)), july3 t=9000 (the four new
batteries already visible at the Weickert-farm reserve ground before their
post-repulse leg, 2,895 px, bbox ~(782,548)-(1006,711)), july3 t=10500
(arrived at Ziegler's Grove, 2,868 px, bbox ~(843,326)-(1007,568) — a
DIFFERENT screen region than t=9000's, confirming the leg actually moved
them). The rest of every frame is stable — no other unit's rendering
changed, consistent with the film-safety verification (§5) and the
parentless-sibling design (§1: the un-promoted regiments' own symbols are
untouched, since nothing here uses the family-suppression mechanic).

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`,
worktree Library, gitignored inputs restored from the main checkout —
`data/heightmap`, `data/dem_cache`, `app/Assets/Generated`, the
SoldierView proxy + full media files; logs `editmode2.log`/`playmode.log`
+ results XML in the worktree, gitignored by design.)

## 10. Recommendation for the next wave

1. **The slice-2 carryovers** (21st Mississippi; the 9th MA's Trostle
   split) are the cleanest next pickup — same technique this wave
   establishes, already drawn/documented at sub-brigade grain.
2. **`build_unit_audit.py` should learn to read the day-expansion phase
   files** (`gettysburg-july1-{morning,afternoon}.json`,
   `gettysburg-july2-{afternoon,evening}.json`), not just
   `gettysburg-july3.json` — every day-expansion-authored unit (Gamble,
   Devin, Calef, Harrow's four children, and now this wave's six units)
   is invisible to the master table's computed columns. A real gap, not
   introduced by this wave, worth its own small dispatch before the next
   content wave compounds it.
3. **If ED-76 is adopted**, its "minus the Nth ___" naming/exact-
   subtraction convention should be written into `battle-format.md`
   alongside the existing `parent`/children section, so future waves
   don't have to re-derive it from the `us-carroll`/`us-8oh` precedent by
   inspection.
