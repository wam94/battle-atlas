# Angle-cast facing adjudication — the six deferred retrograde legs

**Branch:** `angle-facing-adjudication` · **Scope:** the six Angle-cast legs
flagged and deferred by `retrograde-facing.md` §4 (owner gate; this wave
proposes, the owner ratifies) · 2026-07-16

**Question adjudicated:** the retrograde-facing owner ruling (2026-07-13)
says a unit falling back retires FACING THE ENEMY, not spinning to face its
direction of travel. Six legs on film-pinned Angle-cast units in
`gettysburg-july3.json` were flagged by the threshold but left untouched
(film-safety tripwire). Are they true spin-defects awaiting a fix, or
correct depictions the convention was never meant to cover?

**Verdict up front: none of the six is a defect. No edge-pin was needed; no
battle data, bundle byte, or film frame changes in this wave.** The four
CSA repulse legs are ATTESTED-FLIGHT (a rout — the convention, written for
FORMED units retiring under fire, does not apply; the spin is the
historically correct depiction and is what the shipped film itself shows).
The two `us-stannard` legs are ATTESTED-MANEUVER (the documented
change-of-front/return sequence, the same event class as the preserved
`us-13vt`/`us-16vt` legs). Proposed ED-79 (rout exemption) and ED-80
(per-leg dispositions) below, for owner ratification.

## 1. The two data layers (what "fix" would even mean)

- **Macro battle file** (`app/Assets/Battle/gettysburg-july3.json`,
  `UnitTrack.facingDeg`, `Mathf.LerpAngle`): the Atlas map view. The six
  flagged legs live here. The 13 Angle-cast units' keyframes are pinned
  byte-identical (sha256 `69163017…` — `test_retrograde_facing.py`'s
  standing tripwire, the ED-21/decomposition-wave-1 pin chain).
- **Compiled Angle bundle** (`app/Assets/Battle/Angle/angle.bundle.json`,
  from `reconstruction/scripts/compile_angle.py`): per-second states
  t=8040..9000 compiled from the reconstruction corpus's semantic segments
  — NOT from the macro keyframes (the battle file enters the bundle only as
  an input hash, a strength-reconciliation reference, and the `kf[0]`
  facing fallback). The shipped Soldier View film (t=8160..8820, camera in
  Garnett rear-rank slot 881, stagingSeed pin `d470c469…`) was staged from
  these per-second states.

The flagged "spin" is therefore an **Atlas-map phenomenon** on the macro
track; the film's own facing in the overlap (t=8700..8820) comes from the
bundle's `fall_back` segments, which compile facing from **route bearing**
(no authored `facingDeg` — `compile_angle.py` `_route_bearing` fallback),
i.e. the film staged the men facing their direction of flight. Macro track
and film agree: both depict a turn-and-flee.

## 2. Per-leg adjudication

### 2.1 The four CSA repulse legs (t=8700→9000) — ATTESTED-FLIGHT

Shared facts, all four legs:

- **Formation state (Task 1a):** every leg starts `scattered` (t=8700) and
  ends `routed` (t=9000). The preceding leg (8580/8640→8700) already ends
  `scattered` — at **no second of the flagged legs is the unit a formed
  body**. The bundle's matching `fall_back` segments (8700–9000) carry
  `formationFrom=scattered`, `formationTo=routed`, action `fall_back`.
- **Doctrine (Task 1b):** the retrograde convention was written for FORMED
  units retiring under fire ("a line that falls back retires facing the
  enemy"). A rout is attested flight — men turn and run. Holding these
  centroids' facing east through a 35-minute, ~700 m dissolution would
  depict four shattered brigades backing away in parade-ground retrograde,
  contradicting both the record and the shipped film.
- **What the film shows:** the P10 gate's own shot list
  (`p10-gate-evidence.md`, 9:00–11:00 ≙ t=8700–8820): *"The repulse: the
  line turns back; bodies and wreckage of the approach corridor on the way
  out."* The turn IS rendered film content. A ratified "fix" that held
  facing east on the macro track would put the Atlas map in direct
  contradiction to the film for the 120 s of overlap.
- **Visual harm on the map (Task 1c):** the facing chevron renders only for
  `formation == "line" || "column"`
  (`SymbolMeshBuilder.cs` ~line 303: "ordered formations only — a
  dissolving unit asserts no arrow"). Both endpoints of every flagged leg
  are `scattered`/`routed`, so **no chevron exists anywhere on these legs**
  — the Block-tier symbol is a dashed-border, 35 %-fragment-gapped bar
  (`FragmentGapFraction`) whose slow whole-rect rotation asserts no
  direction. At the Soldiers tier, `UnitFormationRenderer.PoseFor` puts
  routed figures in running poses facing the interpolated facing — men
  turning and running west, which is the point.

Per-leg evidence chain:

| Leg | Facing (authored) | Δ | Travel bearing | Withdrawal as attested |
|---|---|---:|---:|---|
| `csa-garnett` 8700→9000 | 78 → 265 | 173° | 264.0 | Peyton's OR (page-verified, dossier `csa-1c-pic-1-garnett.md` EC-4.5 [D]): "about 300 … came off slowly, but greatly scattered, the identity of every regiment being entirely lost" — the dossier's own classification: "a dissolution-class leg (no formed body to track)". Macro t=9000 citation: "survivors stream back toward Seminary Ridge" (ABT 3:45–4:00 PM map). |
| `csa-kemper` 8700→9000 | 60 → 260 | 160° | 251.6 | Dossier `csa-1c-pic-2-kemper.md` EC-4.4: "the survivors' retreat is dissolution-class (division record)"; retreat "under the reopened arc fire". |
| `csa-armistead` 8700→9000 | 80 → 265 | 175° | 268.3 | Dossier `csa-1c-pic-3-armistead.md` EC-4.4: "Retreat/rally: dissolution-class with the division; '643 missing … never marched back'" — and ED-8 (adopted) already rules the fall-back leg an authored centroid recrossing of a dissolved body, not a formed movement. |
| `csa-fry` 8700→9000 | 100 → 270 | 170° | 279.6 | Shepard's OR (dossier `csa-3c-het-3-fry.md` EC-4.4): "Those who remained at the works saw that it was a hopeless case, and fell back … we reformed upon the ground from which we advanced" — flight first, re-formation only back at the start line (matching the macro t=10800 `scattered` rally keyframe). |

Classification: **ATTESTED-FLIGHT**, all four. The authored travel-bearing
facing is correct; the convention exemption (proposed ED-79) applies. These
are not "deferred defects" — `retrograde-facing.md` §4's recommendation to
apply the formed-unit hold here whenever a recompile wave is authorized is
**withdrawn** by this adjudication (proposed ED-80): applying it would be a
historical and film-continuity regression.

### 2.2 The two `us-stannard` legs — ATTESTED-MANEUVER

- **Formation state:** `line` at every keyframe involved — a formed body
  throughout (the chevron does render; the facing sweep is visible on the
  map and means something).
- **Leg t=8400→9600 (facing 15 → 260, Δ115°):** the return from the
  documented flank position. The change of front itself is the
  executor-hardened anchor CA-J3A-8 (dossier `us-i-3-3-stannard.md`:
  "the Thirteenth changed front forward on first company; the Sixteenth …
  performed the same … at right angles to the main line", Stannard's OR
  p. 350); the return is equally documented ("In pursuance of orders from
  General Hancock, we slowly fell back to the main line", Randall's OR
  p. 352; macro citation: "regiments return toward the original front
  after Pickett's repulse", Sturtevant 1910). This is the brigade-level
  aggregate of the SAME event whose regimental legs (`us-13vt` t=8700→9600,
  `us-16vt` t=8700→9900) the retrograde wave explicitly preserved as
  attested (§3 of `retrograde-facing.md`).
- **Leg t=10200→10800 (facing 190 → 260, Δ70°):** the return from the
  second, southward change of front against Wilcox/Lang ("after moving by
  the flank to the right for some fifty rods, made an oblique change of
  front", Benedict pp. 496–497/502; the 16th VT monument: "changing front
  charged left flank of Wilcox's and Perry's brigades"; Stannard wounded
  directing it). A 70° wheel by a formed brigade — under every threshold,
  flagged originally only because the travel bearing of the short
  re-positioning leg opposes the carried facing.
- Neither leg exceeds the 150° raw-swing guard, so neither is (or needs to
  be) in `ALLOWED_LARGE_DELTA_LEGS`. `retrograde-facing.md` §4 already
  reached the same conclusion ("not defects"); this wave confirms it with
  the dossier chain and closes the question.

Classification: **ATTESTED-MANEUVER**, both legs — the cited facing values
are deliberate authored content (the 13th/16th VT precedent), not the
travel-bearing authoring convention.

### 2.3 TRUE-DEFECT count: zero

No leg required the edge-pin procedure (day-expansion-slice-1 precedent,
t=8040 pin). Task 3 is a no-op by adjudication, not by avoidance: the
edge-pin at t=8820 was designed, understood, and found unnecessary —
"fixing" any of these legs would itself be the defect.

## 3. Proposed editorial decisions (for owner ratification — NOT self-adopted)

### Proposed ED-79 — The rout/dissolution exemption to the retrograde-facing convention

The retrograde-facing owner ruling (2026-07-13, `retrograde-facing.md`)
governs **formed units** retiring under fire: they hold their combat facing
and move retrograde. It does not govern **attested flight**: where a leg's
formation state is `scattered` or `routed` (or its segment action is
`fall_back`/rout-class with a dissolution-class dossier record), the men
attestedly turned and ran, and facing = direction of travel is the correct
depiction. Concretely:

1. A moving leg whose start AND end formations are both in
   {`scattered`, `routed`} is exempt from the hold-facing convention when
   the unit's dossier/citations attest dissolution-class withdrawal.
2. The exemption is per-leg and evidence-backed — it never applies to a
   `line`/`column`/`line_disordered` leg, and a rout exemption must cite
   the attestation (OR report, dossier EC-4 entry, or ED).
3. The map grammar already encodes the distinction (chevron and solid
   borders for ordered formations only); no renderer change is implied.
4. `fix_retrograde_facing.py` needs no code change for the current corpus
   (its five-file rewrite already ran; the only legs it would have wrongly
   converted are the four film-pinned ones it was forbidden to touch). Any
   FUTURE deterministic re-run over new authoring must incorporate rule 1
   before converting rout-class legs.

### Proposed ED-80 — Dispositions for the six deferred Angle-cast legs

1. `csa-garnett`/`csa-kemper`/`csa-armistead`/`csa-fry` t=8700→9000:
   **ATTESTED-FLIGHT under ED-79 — authored facing stands, permanently.**
   `retrograde-facing.md` §4's standing recommendation (apply the
   formed-unit hold "whenever the owner authorizes an Angle-cast
   re-cast/recompile wave") is WITHDRAWN — the mooted fix would contradict
   Peyton's/Shepard's reports, the dossiers' dissolution-class records,
   ED-8, and the shipped film's own repulse footage (t=8700–8820).
2. `us-stannard` t=8400→9600 and t=10200→10800: **ATTESTED-MANEUVER** —
   cited change-of-front/return facings stand (the `us-13vt`/`us-16vt`
   precedent, CA-J3A-8/9).
3. The film-safety pin on the 13 Angle-cast units' keyframes is unchanged
   and stays in force (sha256 `69163017…`); nothing in this ED authorizes
   touching them.
4. `test_retrograde_facing.py`'s `ALLOWED_LARGE_DELTA_LEGS` annotations now
   cite ED-79/ED-80 (this wave, comment/annotation-only): the four entries
   are doctrine (correct depictions), not TODOs awaiting a fix.

**Ratification effect:** if the owner adopts ED-79/ED-80, the texts above
should be copied into `docs/reconstruction/angle-editorial-decisions.md` as
ED-79 and ED-80 (next free numbers after ED-78) with the PROPOSED labels
dropped; the `(proposed …)` wording in `test_retrograde_facing.py`'s
annotations and module docstring should then be updated in place. If the
owner rejects the adjudication for any leg, that leg reverts to
deferred-defect status and the edge-pin procedure in §5 is the authorized
fix path.

## 4. Film-safety evidence (this wave)

- **Zero content changes.** `git diff origin/main` touches only:
  `reconstruction/tests/test_retrograde_facing.py` (docstring + allowlist
  annotation strings — no logic), and this report. No battle JSON, no
  bundle, no recon/claims/canonical file, no media, no renderer.
- **Pinned-cast byte identity:** trivially preserved (file untouched);
  `test_pinned_angle_cast_units_byte_identical_to_main` (sha256
  `69163017ebc0c670b742d91e89658310d526232ecbbe966f12e291d67c03ab68`)
  passes.
- **Bundle recompile determinism:** `compile_angle.py` re-run in this
  worktree; output byte-identical to the committed
  `angle.bundle.json` (sha256 match, checksum `ffb6b59a…`, stagingSeed
  `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  — the ED-21 pin HOLDS). Every per-second state of every one of the 13
  cast units across the whole window is identical because the whole file
  is identical.
- **Captures:** none required — no unit's authored data changed, so there
  is no before/after to photograph (the task's capture requirement is
  conditioned on "any changed unit"; there are none).

## 5. The edge-pin procedure (recorded, unused)

For the record, had any leg been TRUE-DEFECT: insert a keyframe at t=8820
carrying the exact `Mathf.LerpAngle` interpolation of the flagged leg at
that second (for `csa-garnett` 8700→9000: u=0.4 along 78→265 shortest arc
(+187 normalizes to −173), 78 − 173×0.4 = 8.8 ≡ facing 8.8 — and
correspondingly for position), then re-author facing only on the ≥8820
side; atomic recompile; verify per-second identity t=8160..8820 for all 13
cast units (the day-expansion-slice-1 t=8040 pin precedent). This was NOT
executed. Note the macro-layer subtlety it would have carried: the pinned
sha256 covers ALL keyframes of the 13 units, so even a film-frame-safe
edge-pin requires an owner-ratified pin bump — one more reason the
adjudication, which needs no bump, is the right resolution.

## 6. Suites

| Suite | Floor (main) | This wave |
|---|---|---|
| tool vitest | 119 | **119 passed** (no `tool/` source touched) |
| pipeline pytest | 59 | **59 passed** (no `pipeline/` source touched) |
| reconstruction pytest | 148 passed + 1 skipped (149 total) | **148 passed, 1 skipped** — includes `test_retrograde_facing.py`'s 3 (guard, allowlist-liveness, pinned-cast sha256), all green with the new annotations |
| Unity EditMode | 429 passed, 0 failed, 4 skipped | **429 passed, 0 failed, 4 skipped** (433 total — exactly the floor) |
| Unity PlayMode | 21 passed, 0 failed | **21 passed, 0 failed, 0 skipped** — the recorded clean run is `playmode-results2.xml`; a first run under heavy machine load (load avg ~19: the owner's editor open on the main checkout plus this worktree's first-import churn) dropped THREE video-seek-latency tests (`SeekLatency_Full1440pProductionMedia`, `Seek_OutsideWindow_ClampsToDecodableRange`, `Seek_SettlesWithinOneFrame` — drift/latency assertions, e.g. "latency 3017ms"), the same known perf-sensitive video-decode flake class `retrograde-facing.md` §9 documents; clean after a 2-minute settle (load avg ~4.8), unrelated to this wave's comment-only change |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`, this
worktree's own `Library` (built fresh); gitignored inputs restored from the
main checkout — `data/heightmap`, `data/dem_cache`, `data/landcover/*`
(incl. `splatmap.png`), `data/map-furniture/*`, `app/Assets/Generated`,
the SoldierView `.mp4` media; staging via `CartographyStage.PrepareScene`,
no `-nographics`. The owner's Unity editor was open on the MAIN checkout
throughout — never touched; this worktree is a separate Unity project
directory with its own lock. `PrepareScene`'s terrain-reimport touched
`app/Assets/Scenes/Atlas.unity` locally — reverted before committing, same
as every prior wave.)

## 7. What needs the owner

1. **Ratify or reject proposed ED-79** (rout/dissolution exemption — the
   doctrine amendment).
2. **Ratify or reject proposed ED-80** (the six-leg dispositions,
   including withdrawing `retrograde-facing.md` §4's fix-when-authorized
   recommendation for the four CSA legs).
3. Nothing else: no film re-render ruling is needed (no true defect was
   found, in-window or out), and no pin bump is requested.
