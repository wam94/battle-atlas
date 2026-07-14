# Retrograde-facing — units retire facing the enemy, not the direction of travel

**Branch:** `retrograde-facing` (unmerged; owner gate) ·
**Script:** `reconstruction/scripts/fix_retrograde_facing.py` (deterministic
rewrite over the five phase battle files; no hand-edited JSON), a new
regression test `reconstruction/tests/test_retrograde_facing.py`, and a new
evidence harness `app/Assets/Scripts/RetrogradeFacingCaptureHarness.cs` ·
2026-07-13

**OWNER RULING driving this slice:** units must NOT "spin around" when they
reverse direction of travel. A line that falls back retires FACING THE
ENEMY (retrograde movement, backing up), not about-facing and marching
away.

## 1. Mechanism found

Facing is authored per keyframe (`UnitTrack.facingDeg`,
`Mathf.LerpAngle` between keyframes — `app/Assets/Scripts/UnitTrack.cs`)
and consumed as-authored by `UnitFormationRenderer.BuildMatrices`/
`BuildPoseMatrices` and `SymbolMeshBuilder.BuildRibbon`'s facing chevron
(`app/Assets/Scripts/UnitFormationRenderer.cs`,
`app/Assets/Scripts/SymbolMeshBuilder.cs`). Neither renderer computes
facing from motion — they only interpolate/rotate by whatever `facing`
value the battle JSON's keyframes carry. **The bug is not in the
renderer; it is in the authored data**: across the five phase files, a
retreating unit's keyframe `facing` was set to (approximately) the
travel bearing of the retreat leg, not held at the unit's prior combat
orientation. `Mathf.LerpAngle` then sweeps the shortest arc between the
pre-retreat and post-retreat facing values across the whole leg, so the
symbol visibly rotates through the turn while backing away — the
reported "spin."

This is NOT a single buggy function call in one authoring script (the
`tool/scripts/author-*.ts` wave scripts mostly hardcode literal facing
degrees per keyframe, by hand/editorial judgment) — it is a systemic
authoring convention applied uniformly across every wave that touched a
retreating unit: **whoever/whatever set a keyframe's facing during a
falling-back leg used the leg's own direction of travel**, not the
unit's established combat facing. `reconstruction/scripts/compile_angle.py`
(`_route_bearing`/`_lerp_bearing`, lines 97–122) shows the same
pattern formalized as an explicit *fallback* for its separate
Angle-bundle compilation (segments with no authored `facingDeg` fall back
to route bearing) — evidence that "facing = direction of travel" was the
house default this whole corpus inherited, not a one-off mistake.

Verified empirically: for the vast majority of flagged legs, the
authored end-of-leg facing lands within a few degrees of the leg's raw
travel bearing (e.g. Pickett's-charge survivors' post-repulse keyframe
facing matches their retreat bearing within 1–5°, dozens of times across
`gettysburg-july3.json` alone).

## 2. The convention (implemented at the data layer)

`reconstruction/scripts/fix_retrograde_facing.py`, run deterministically
over the five phase battle files (never hand-edited):

- Walk each unit's keyframes in order, carrying a running "current
  facing" forward.
- For each moving leg (position changes by more than
  `POS_EPSILON_M` = 0.5 m): compute the leg's travel bearing
  (`atan2(dx,dz)`, matching `compile_angle.py`'s own convention) and its
  angular difference from the carried facing.
  - If the difference exceeds `REVERSAL_THRESHOLD_DEG` = 120° **or** the
    leg's authored facing itself swings more than
    `FACING_SWING_SAFETY_NET_DEG` = 150° from the carried facing (a
    safety net — a genuine wheel/oblique turn has no business swinging
    the symbol's own facing that far in one leg; a few individual
    regiments inside a brigade-wide rout land just under 120° on the
    bearing test alone while still showing the same near-total-reversal
    signature as their siblings) — the leg is a **retrograde leg**: the
    unit's facing is held at the carried value (unchanged) instead of
    adopting the authored value, unless…
  - …an about-face/countermarch is **attested** in that leg's own
    citation (preserved verbatim, not overridden — §3), or the unit is
    one of the 13 Angle-cast units in `gettysburg-july3.json` (film-safety
    pinned — never touched, §4).
  - Otherwise (≤120° and no hard swing): a normal turn/wheel/oblique —
    the authored value is adopted as-is, unchanged.
- **Cross-file continuity**: the five phase files are processed in
  battle-manifest.json day/phase order. A unit's carried facing crosses a
  file boundary only where the corpus's own citations declare the two
  files are the SAME narrative moment split for pacing — "Continuity
  from the morning phase's `<t>` state" / "Continuity static ... holds
  the ... cited 19:29 end state" — i.e. `july1-morning→july1-afternoon`
  and `july2-afternoon→july2-evening`. Where the fix changes a unit's
  end-of-file facing and the next file's `kf[0]` disagrees (same
  position, stale facing), `kf[0]` is corrected too — otherwise the
  file's own "holds the cited end state" citation would be false after
  the leg-fix. The other two boundaries (`july1-afternoon→july2-afternoon`,
  `july2-evening→july3`) are overnight day-gaps with fresh
  tablet/return-based citations, not "continuity" ones — the corpus
  routinely re-postures units for the next day there, and a mismatch at
  those seams is not this bug. Left untouched.

The script touches **only** the `"facing"` field of existing keyframes.
Positions, strengths, formations, confidence, and citations are
byte-identical before/after (verified — every changed line in every
diff is a `"facing": N` line; see §5).

## 3. Attested about-faces preserved (2, both cited)

Every leg the threshold flagged was manually reviewed for citation
language describing a deliberate reversal (regex pass +
hand-verification of every hit; the full converted-leg citation set was
also re-scanned for missed vocabulary — one false positive found, a
cross-reference to us-16vt's own event embedded in a neighboring unit's
citation, not a claim about that unit's own facing). Two legs carry an
unambiguous, cited about-face and are preserved as-authored:

| Unit | File | Leg | Citation |
|---|---|---|---|
| `us-16vt` | `gettysburg-july3.json` | t=8700→9900 | "recalled and re-forms facing south as Wilcox and Lang step off ~15:45 — **change of front to the rear**" (Veazey's OR report; Benedict vol. 2) |
| `us-13vt` | `gettysburg-july3.json` | t=8700→9600 | "returns toward the original front after Pickett's repulse" (Sturtevant 1910) — the companion reformation to `us-16vt`'s attested change of front; same Stannard-brigade flank-attack action against Wilcox/Lang's supporting advance |

Two further legs carry explicit "counter-march" citations (`us-kane`,
`us-candy` in `gettysburg-july2-evening.json` — Geary's documented "wrong
road" countermarch back onto Culp's Hill) but need **no** exception: once
cross-file boundary propagation corrects their `kf[0]` facing (inherited
from `gettysburg-july2-afternoon.json`'s corrected end state), their
first leg's travel bearing no longer opposes the corrected carried facing
by more than the threshold — it resolves as a normal turn on its own. See
`fix_retrograde_facing.py`'s `ATTESTED_ABOUT_FACE_LEGS` comment for the
record.

## 4. Angle-cast defects found but NOT fixed — owner ruling needed

**FILM-SAFETY TRIPWIRE (absolute):** the 13 Angle-cast units' keyframes
in `gettysburg-july3.json` are pinned byte-identical. Four of them
exhibit the exact spin defect and are left untouched, deferred here:

| Unit | Leg | Authored (unfixed) facing | Would-be held facing | Bearing |
|---|---|---:|---:|---:|
| `csa-garnett` | t=8700→9000 | 265.0 | 78.0 | 264.0 |
| `csa-kemper` | t=8700→9000 | 260.0 | 60.0 | 251.6 |
| `csa-armistead` | t=8700→9000 | 265.0 | 80.0 | 268.3 |
| `csa-fry` | t=8700→9000 | 270.0 | 100.0 | 279.6 |

All four are the same moment — the repulse at the stone wall — and all
four show the textbook signature (authored facing within a few degrees
of the raw retreat bearing). The regiments composing Garnett's,
Kemper's, and Armistead's brigades (`csa-18va`, `csa-19va`, `csa-28va`,
`csa-56va`, `csa-8va`, `csa-3va`, `csa-7va`, `csa-11va`, `csa-24va`,
`csa-1va`, `csa-9va`, `csa-14va`, `csa-53va`, `csa-38va`, `csa-57va`,
etc. — **not** in the pinned 13) exhibit and were fixed identically, so
the Pickett-recross deliverable capture below uses `csa-marshall`
(Pettigrew's division, also not pinned) rather than any of these four.

`us-71pa` and `us-stannard` were also flagged by the threshold but are
**not** defects: `us-71pa`'s authored facing never actually changes on
its flagged leg (262→262 — already compliant, a threshold false-positive
against the raw bearing only); `us-stannard`'s two flagged legs both
carry citations describing the brigade's own documented reformation
after its flank attack ("regiments return toward the original front
after Pickett's repulse" / the wheel-south-then-return sequence) — the
same attested event `us-13vt`/`us-16vt` carry, just examined here rather
than fixed (pinned either way). No owner ruling needed for either; noted
for completeness.

**Recommendation:** the four listed legs are candidates for the SAME fix
(hold `csa-garnett`/`csa-kemper`/`csa-armistead`/`csa-fry` at their
pre-repulse facing through the retreat) whenever the owner authorizes an
Angle-cast re-cast/recompile wave — the fix is mechanical and identical
to what this wave already proved correct on the same brigades' own
regiments.

## 5. Film-safety evidence

1. **Source-file byte identity.** The 13 Angle-cast units' keyframes,
   extracted from `gettysburg-july3.json` (sorted-key JSON, sha256'd):
   **`69163017ebc0c670b742d91e89658310d526232ecbbe966f12e291d67c03ab68`**
   — identical before and after this wave's script ran (verified twice,
   once per script revision).
2. **Compiled bundle payload identity.** `reconstruction/scripts/
   compile_angle.py` re-run after the battle-file edits: the bundle's
   `units` field (the per-second simulation for the 13 Angle-cast units)
   is **byte-identical** before/after (sha256 of the sorted-key JSON:
   `7568ee9c79dc6e3103428732729688a920b8c2c41441c79cea4e77d6378cfcd1`).
   `stagingSeed` unchanged:
   `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1` (the
   ED-21 pin HOLDS). Only `inputs.battle` (the sha256 of the whole,
   now-different battle file) and the derived top-level `checksum`
   changed — the metadata-only-recompile signature established by
   `decomposition-wave-1.md`/`authoring-wave-a1.md`/`a2.md`/
   `strength-reconciliation-1.md`.
3. **Diff shape.** `git diff -- app/Assets/Battle/` touches 5 files, 274
   insertions / 274 deletions — every changed line in every file is a
   `"facing": N,` line (grep-verified: zero non-facing lines in the
   diff).

**Verdict: FILM-SAFE**, with the four Angle-cast defects in §4 explicitly
deferred, not silently fixed or silently ignored.

## 6. Per-file leg counts

| File | Converted (facing now holds) | Boundary-propagated | Preserved (attested) | Deferred (pinned) | No-op (already held) |
|---|---:|---:|---:|---:|---:|
| `gettysburg-july1-morning.json` | 9 | 0 | 0 | 0 | 9 |
| `gettysburg-july1-afternoon.json` | 57 | 5 | 0 | 0 | 9 |
| `gettysburg-july2-afternoon.json` | 46 | 0 | 0 | 0 | 16 |
| `gettysburg-july2-evening.json` | 11 | 31 | 0 | 0 | 4 |
| `gettysburg-july3.json` | 115 | 0 | 2 | 6 | 6 |
| **Total** | **238** | **36** | **2** | **6** | **44** |

("Boundary-propagated" legs are counted at the later file, where the
correction lands; the 6 deferred legs are the 4 `csa-garnett`/
`csa-kemper`/`csa-armistead`/`csa-fry` legs plus `us-71pa`'s and
`us-stannard`'s threshold false-positives from §4, all pinned regardless
of classification.) Re-running the script against its own output is a
no-op (confirmed: 0 converted/boundary on a second pass, matching
idempotence).

## 7. Regression guard

`reconstruction/tests/test_retrograde_facing.py`:

- `test_no_unattested_facing_reversal_over_150deg` — scans every moving
  leg across all five files for a raw facing swing over 150°; fails
  unless the leg is in the small, cited `ALLOWED_LARGE_DELTA_LEGS` table
  (the 4 pinned Angle-cast defects from §4 + `us-16vt`'s attested
  about-face — `us-13vt`'s own swing is 115°, under the guard's own
  threshold, so it needs no entry).
- `test_allowlist_entries_still_present` — the allowlist itself must
  stay live (a stale entry for a leg that no longer exists is a bug in
  the guard, not a pass).
- `test_pinned_angle_cast_units_byte_identical_to_main` — standing
  film-safety check, pins the §5 sha256 directly in the test suite (not
  just this doc).

## 8. Deliverables (before/after captures)

`app/Assets/Scripts/RetrogradeFacingCaptureHarness.cs` (new;
`-retrofacingshots`/`-retrofacingset collapse|pickett`/`-retrofacingOut`,
same mold as the `DayExpansion2/3CaptureHarness` evidence harnesses) +
`scripts/retrograde-facing-captures.sh` (BEFORE = pristine origin/main
battle files via `-battleFile`, AFTER = this wave's fixed files; 16 PNGs
total). Both episodes deliberately sit MID-LEG on a `line`-formation
retreat leg: pre-fix, `Mathf.LerpAngle` is mid-sweep between the old
keyframe pair there, so the spin is visible in a still; and the leg's
start formation is `line`, so the map symbol wears its facing chevron
(the chevron only renders for ordered formations). Each episode is
captured at BOTH LOD tiers — plain shots in the Soldiers tier (450 m,
individual figures show the line's orientation directly) and `-symbol-`
shots in the Block tier (4,600 m, the monolithic draped symbol with the
chevron the owner ruling is about):

1. **July 1 collapse (von Gilsa's brigade, `us-vongilsa`, Barlow's Knoll
   → the break → through town; leg t=6300→9600):**
   `*-collapse-vongilsa-engaged[-symbol]-t6300` (the knoll line's north
   face, facing 350 — the leg's start keyframe, identical before/after
   by construction, the control shot) vs
   `*-collapse-vongilsa-retreat[-symbol]-t8500` (mid-leg, u≈0.67, the
   brigade ~250 m back off the knoll — BEFORE: facing mid-sweep at ~240,
   the bar/line visibly rotated diagonal, chevron swung away from the
   enemy; AFTER: facing holds 350, the bar stays east–west with the
   chevron still pointing north AT the knoll while the line backs away).
2. **July 3, the Pickett episode's supporting advance (Wilcox's brigade,
   `csa-wilcox` — NOT one of the 13 pinned units; leg t=10380→10500,
   the retreat to Seminary Ridge after the 16th Vermont's flank
   attack):** `*-pickett-wilcox-engaged[-symbol]-t10380` (facing 85,
   toward the Union line — control shot) vs
   `*-pickett-wilcox-recross[-symbol]-t10450` (mid-leg, u≈0.58 —
   BEFORE: facing mid-sweep at ~187, chevron spun south/away; AFTER:
   facing holds 85, chevron still pointing east AT the McGilvery line
   while the brigade retires west).

(The Pickett recross proper — Garnett/Kemper/Armistead/Fry's own
t=8700→9000 back-leg — was checked FIRST per the brief and found
Angle-cast-pinned; it is deferred in §4, so the episode's capture uses
Wilcox's un-pinned supporting-advance retreat instead.)

Screenshots + both perf-benchmark JSONs (below) in
`docs/benchmarks/captures/retrograde-facing/` (force-added, per the
project's `docs/benchmarks/captures/` gitignore rule).

### Perf (the two capture files, post-fix)

Generic `BenchmarkHarness` against the built Development standalone:

- **`gettysburg-july3.json`** (t = 0/8160/8700/9000/10500):
  **59.6–59.7 avg FPS**, p95 frame 17.15–17.33 ms, allocations flat at
  ~323–324 MB (`retrofacing-july3-benchmark.json`).
- **`gettysburg-july1-afternoon.json`** (t = 0/6300/9600/12600/18000):
  **59.3–59.8 avg FPS**, p95 frame 17.0–17.25 ms, ~321–324 MB
  (`retrofacing-july1pm-benchmark.json`; the single 59.3 sample is the
  same end-of-run noise class prior waves' reports carry).

Both within the established ~59.5 FPS floor band.

## 9. Suites

| Suite | Floor (main) | This wave |
|---|---|---|
| tool vitest | 119 | **119 passed** (unchanged — no `tool/` TS source touched) |
| pipeline pytest | 59 | **59 passed** (unchanged — no `pipeline/` source touched) |
| reconstruction pytest | 128 passed, 1 skipped | **131 passed, 1 skipped** (+3: `test_retrograde_facing.py`) |
| Unity EditMode | 405 passed, 0 failed, 4 skipped (floor) | **405 passed, 0 failed, 4 skipped** (409 total — exactly the floor) |
| Unity PlayMode | 21 passed (floor) | **21 passed, 0 failed** (full SoldierView media restored; two earlier runs each dropped ONE video-seek-latency test — `SeekLatency_MeasuredAcrossWindow` in one run, `SeekLatency_Full1440pProductionMedia` in the other, i.e. the known perf-sensitive video-decode flake class, unrelated to battle data — the recorded clean run is `testlogs/playmode-results5.xml`) |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`, this
worktree's own `Library` — built fresh, not shared with the owner's main
checkout; gitignored inputs restored from the main checkout
(`data/heightmap`, `data/landcover/*`, `app/Assets/Generated`) via
`CartographyStage.PrepareScene` (NOT `Phase12Review.PrepareStandaloneScene`);
no `-nographics` flag. `PrepareScene`'s terrain-reimport touched
`app/Assets/Scenes/Atlas.unity` locally — reverted before committing,
carries no content from this wave.)

## 10. Cross-phase continuity

This slice changes **facing values only** — no position, strength,
formation, or citation field is touched anywhere in the five phase
files (§2, §5.3). The existing test suites (which include the
cross-phase merge/audit machinery from `strength-reconciliation-1`) stay
green (§9), and the boundary-propagation step (§2) actively *improves*
cross-file facing continuity at the two same-day seams rather than
perturbing it.
