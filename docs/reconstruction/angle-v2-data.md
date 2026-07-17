# Angle-v2 DATA wave — the film-invalidating content recompile

**Status:** executor report for the `angle-v2-data` slice — the wave the
owner explicitly authorized ("angle film v2 authorized"): in-window
(t=8160..8820) intensity content lands in the Angle bundle, the bundle
recompiles ONCE, and the v2 film renders. The shipped v1 media stays
untouched on disk (all v2 output under `app/RenderOutput/angle-v2/` and
`docs/benchmarks/captures/angle-v2-data/`).
**Branch:** `angle-v2-data` (unmerged; the v2 film is an owner-review
gate — the coordinator holds the branch until the owner watches it).

Sources: `docs/reconstruction/angle-v2-vocab.md` §3 (the data-wave
wiring recommendations, followed), `docs/reconstruction/audit/
charge-intensity-proposals.md` (P1/P2 evidence),
`docs/reconstruction/audit/bombardment-prelude.md` §3 (the deferral
picked up), `angle-editorial-decisions.md` ED-81 (ADOPTED) / ED-8 /
ED-4 / ED-22, `violence-and-representation.md` (binding), and the
dossiers cited per edit below.

## 0. Summary of changes

| # | Scope | What changed | Where |
|---|---|---|---|
| 1 | P1 deferral | Kemper/Armistead/Fry bombardment re-timed (pre-window keyframes; values unchanged) | `gettysburg-july3.json` (+15 children), `author-bombardment-v2.ts` |
| 2 | P2 bursts | 6 cited profiles strike-correlated; compiler emits per-victim `fallTimes` clustered at the C# StrikeEvents | `angle.reconstruction.json`, `compile_angle.py`, `strike_correlation.py`, `CasualtySchedule.cs` (proposed **ED-82**) |
| 3a | P3 melee | Garnett↔69pa symmetric melee 8640–8700; Armistead unpaired melee tail 8670–8700 | `angle.reconstruction.json` (3 segment splits), resolver facing/pair bounds |
| 3b | P4 colors | `colorParty: 13` on csa-fry (Shepard's counts; brigade-grain caveat) | `angle.reconstruction.json` |
| 3c | P5 mounted | `mountedOfficers` spec on csa-garnett, fallT 8580 (ED-81 ADOPTED; claim-garnett-death) | `angle.reconstruction.json`, `AngleBundleData.cs`, `AngleActionScene` staging |
| 3d | P6 halt-fire | **Wired NOWHERE** — adjudicated per each unit's own record (§5) | — |
| 4 | Recompile | ONE consolidated bundle recompile; stagingSeed pin HELD verbatim | `angle.bundle.json` |
| 5 | Render | v2 production render (rear-rank slot 881, same framing) + P7 front-rank proof | `AngleV2Render.cs`, §8 |

- **New bundle checksum:** `1f613b3ccff5…` (v1: `e529c74af77d…`).
- **stagingSeed:** `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  — **HELD verbatim** (the 07-10 decoupling): victim draws are unchanged
  per profile; the committed v1 captions/media are not re-rolled.
- **Pinned-cast sha bump (authorized):**
  `69163017ebc0c670…` → `d6bc14b84d38e214…`
  (`reconstruction/tests/test_retrograde_facing.py`, comment carries the
  authorization and both hashes).

## 1. Kemper/Armistead/Fry bombardment re-timing (P1 deferral)

The exact `bombardment-prelude` method applied to the three cast
brigades it deferred (its §3): two new pre-window keyframes each
(t=420 signal guns, CA-J3A-1; t=3600 even-split midpoint), the t=7200
keyframe's citation upgraded from the generic placeholder — **values
unchanged**; t≥7500 byte-identical (the total-by-step-off invariant).

| Unit | t=0 | t=3600 (NEW) | t=7200 (value untouched) | Evidence |
|---|---|---|---|---|
| csa-kemper | 1575 | 1562 | 1550 | Wilcox "suffered severely" (or-27-2-wilcox; dossier csa-1c-pic-2-kemper.md EC5.1/EC6) — a severity comparison, never a total (V&R §2); the existing 25-man share already exceeds Garnett's attested ~20 |
| csa-armistead | 1650 | 1640 | 1630 | second line; "no brigade-specific bombardment casualty figure survives" (dossier csa-1c-pic-3-armistead.md) |
| csa-fry | 1048 | 1039 | 1030 | Heth's division record or-27-2-davis-jr ("About 1 p.m. … ceased firing at 3 o'clock"; dossier csa-3c-het-3-fry.md EC5.2) |

15 children re-derived by the family share formula
(round(share×parentAt(t)) — verified to reproduce every existing child
keyframe before authoring). **Invariant verified:** 28 units × 661 s of
in-window compiled state (t=8160..8820), **0 mismatches**; exactly 18
unit records changed; every other unit byte-identical (guards inside
`author-bombardment-v2.ts` throw otherwise).

## 2. P2 — strike-correlated casualty bursts (proposed ED-82)

Six profiles whose canister share is claim-backed carry a cited
`strikeCorrelation` block: `cas-garnett-final-approach`,
`cas-garnett-wall-fight`, `cas-kemper-flank-and-front`,
`cas-armistead-closing`, `cas-armistead-breach`, `cas-fry-closing`.
Citations: claim-cas-shell-clusters (NEW — Peyton: "sometimes as many
as 10 men … by the bursting of a single shell", the maxPerStrike bound)
+ claim-cas-canister-gaps (Fuger's "immense gaps").

Mechanism (full text: ED-82 PROPOSED, angle-editorial-decisions.md):
the compiler re-times the profile's canister-cause victims (the cited
causeMix fraction) to the nearest compiled StrikeEvent within ±30 s of
their smooth draw and inside the profile window; ≤10 per strike,
staggered over ~1.2 s. Victim identity, causes, counts, windows
unchanged. Emitted as 1/64 s-quantized `fallTimes` in the bundle;
per-second strength counts them directly (reconciliation EXACT for
correlated profiles; ±1 rounding property unchanged elsewhere).

**Verification** (`angle-v2-vocab-evidence.json`): the Python
replication (`strike_correlation.py`, float32-faithful FNV/CompileCannon/
targeting) matches the C# StrikeEvents within **0.5 ms across all 491
in-window strikes** (garnett 118, kemper 41, armistead 126, fry 206) —
bursts land on the same instants as the rendered strike dust and
flinches. Wall-fight peak: **7 falls in one second** (v1 smooth curve:
~2.5/s). All boundary totals exact; AngleActionScene's ±1 reconciliation
and the full CasualtyScheduleTests pass unchanged.

## 3. P3 — melee at the wall

- `seg-garnett-take-canister-at-wall` split at 8640:
  take_canister 8580–8640 (canister reception; scatter completes) +
  **`seg-garnett-wall-melee` 8640–8700, `meleeOpponentId: us-69pa`**.
  Citation: NEW claim-garnett-works-melee — Peyton EC5.4 "hand to hand,
  and of the most desperate character; … climbing over the wall, and
  fighting the enemy in his own trenches until entirely surrounded".
- `seg-69pa-wall-crisis` split at 8640: fire_independent 8580–8640 +
  **`seg-69pa-wall-melee` 8640–8700, `meleeOpponentId: csa-garnett`**
  (symmetric). Citation: claim-69pa-refused-flank (McDermott: Co. D
  "held the enemy at bay, using their muskets as clubs").
- `seg-armistead-breach` split at 8670: breach 8640–8670 (the crossing
  surge, wall crossed at x≈4400) + **`seg-armistead-works-melee`
  8670–8700, unpaired** (no opponent field — the symmetric wiring is
  Garnett↔69pa; the crossed party works clubbed-swing/thrust/parry
  bouts at the guns). Citation: NEW claim-armistead-works-melee
  (McDermott receiving-side + Martin "under the guns he had captured").
- **Adjudicated NO melee:** us-72pa (ED-4 — fights from the crest; its
  advance to the wall is post-slice t≈9300); us-71pa (its own record is
  the fall-back, claim-71pa-fallback).

Measured on the pinned seed: **3 grapple pairs** straddle the wall
(max step-in 6.41 m, inside the new 8 m production reach guard) plus
line-wide bout work; pairs are sparse because the proportional file map
(697 Garnett files onto 121 of the 69th) only aligns spatially near the
wall — an honest geometry consequence, disclosed. Resolver hardening
found by the comfort-bound/no-teleport suites (all melee-gated, so v1
bundles resolve bit-identically): 3 s fight-facing ramp-in; the fight
facing applies while the formation frame drifts a slot across the
locomotion threshold (kills a 2 Hz facing flap); clinches dissolve
ReturnDur before segment end; the following retreat segment blends OUT
of the held fight bearing after the compiled about-face sweep (a lerp
across the sweep hits the LerpAngle antipode — a 285 deg/s camera spike
the test caught).

## 4. P4 — colors on csa-fry, and an owner question

`colorParty: 13` (Shepard: 1st Tenn 3 + 13th Ala 3 + 14th Tenn 4 + 7th
Tenn 3 bearers down; "Every flag in the brigade excepting one was
captured at or within the works" — NEW claim-fry-color-bearers +
claim-fry-colors-at-works). **Brigade-grain caveat (disclosed in the
canonical note):** csa-fry compiles as one brigade line, so the four
regimental chains are wired as one 13-deep brigade color party; a
regiment-grain recast would carry one chain per regiment. The staff/
flag prop is now first-class production staging (`AngleActionScene`,
posed from `ColorGuard.StateAt`).

**FINDING (owner question):** on the pinned stagingSeed the acting
bearer (front-rank slot 239, the line's center) draws NO fate — the
colors are CARRIED throughout the v2 window and never pass
(`angle-v2-vocab-evidence.json` colorsTimeline: Carried, bearer 239,
8160→8820). The succession vocabulary (fall/ground/pickup) therefore
does not play on camera in this film; it remains exercised in the
committed demo + 9 ColorGuardTests. Making the succession play would
require biasing victim selection toward chain slots (cited by Shepard's
counts) — that changes in-window victim DRAWS, which this wave kept
stable by design; flagged for an owner ruling rather than slipped in.

## 5. P6 — halt_fire_obstacle adjudication: wired NOWHERE

Trimble's quote ("our men stopped and began firing, instead of mounting
the fence", trimble-shsp26-1898) covers HIS command — Lane's and
Lowrance's brigades, neither in the 13-unit Angle cast. Per-unit
adjudication from their own records:

- **csa-lane** — his own report CONTRADICTS a halt: "Lowrance's brigade
  and my own, without ever having halted, took position…"
  (or-27-2-lane-jh, dossier csa-3c-pen-2-lane.md EC4.2). No halt-fire.
- **csa-lowrance** — the only fence-halt-to-fire in his record is
  JULY 1 (Perrin: "Scales' brigade had halted to return the enemy's
  fire, near the fence" — or-27-2-perrin, dossier EC5.0); his July 3
  record carries no fence halt. Not transplanted across days.
- **Angle-cast fence checks (Emmitsburg road, 8120–8340):** Garnett
  climbed ("it had to climb three high post and rail fences",
  peyton-or-1863); Fry's men "rushed over rapidly as they could"
  (or-27-2-shepard) — both records attest crossing, not halting to
  fire. Kemper/Armistead carry no fence-behavior statement.

Result: the class stays an exercised-in-tests capability
(HaltFireObstacleTests, 6 tests), exactly the vocab report's option (a)
— "do not invent motion" (punchlist rule); no unit's own record attests
the behavior inside this cast.

## 6. P5 — the mounted officer (ED-81, ADOPTED)

`mountedOfficers` promoted to a bundle field (`AngleBundleData.cs`) and
wired on csa-garnett: `officerId: csa-garnett-mounted-1` (anonymous —
no name, insignia, or identification, ED-81 §1), `fallT: 8580`
(claim-garnett-death preferred time; envelope 8520–8700, validated),
`backOffsetM: -3` (a forward station — "he rode the line's front"),
`alongOffsetM: 0` ("near the center of the brigade"). Ride position at
fallT ≈ (4383, 4868) — inside claim-garnett-death's (4380, 4868) ±30 m
and ~16 m from the traced wall ("within about 25 paces"). Verified arc
(`angle-v2-vocab-evidence.json`): rides Horse_Walk to 8580, rears at
the hit, the rider is down by 8583 and persists; the riderless horse
bolts west and leaves the scene by ~8620. Horse+rider are first-class
production staging (`AngleActionScene.StageVocabExtras`). No caption
names the figure (ED-81 §3: a product caption naming Garnett must quote
the record, not the renderer).

## 7. Suites (floors in parentheses)

| suite | result |
|---|---|
| tool vitest | **119** (119) |
| pipeline pytest | **66** (66) |
| reconstruction pytest | **159 + 1 skip** (159+1) |
| Unity EditMode | **479 of 479 passed** (floor 478+1 — the former terrain-material self-skip RAN: crops/bakes present in the worktree) |
| Unity PlayMode | **21 of 21 passed** (floor 20+1 — the production-media-conditional seek test RAN against the copied v1 media) |

Unity CLI: `-batchmode -runTests -buildTarget OSXUniversal`, worktree
Library, no `-nographics`; gitignored inputs copied from the main
checkout / sibling worktrees; `CartographyStage.PrepareScene` run once
before any test; `Atlas.unity` fileID churn reverted per convention.
Two pre-existing tests were updated for v2 content, both disclosed in
§3: the lens-guard test now DERIVES its offender set from the compiled
content (the v1 hand-pinned slots were schedule fixtures) and carries
the designed crossing-exit exemption the preflight already had.

## 8. The v2 render (P10 pattern) — RESULTS PENDING RENDER QUEUE

Preflight (the global no-teleport gate over the NEW states): **PASS** —
62,110,620 coarse pairs, 10,777 suspect windows refined (ALL of them
designed crossing-exit hand-offs), 0 violations; camera max
0.1255 m/frame (bound 0.15). `angle-v2-preflight.json`.

Render entry points (`AngleV2Render.cs`): same viewpoint
`garnett-road-to-angle`, rear-rank slot 881, first-person, t=8160..8820
+0.5 s pad, 2560×1440p30, offline HDRP profile, resumable 60 s chunks —
every output under `app/RenderOutput/angle-v2/` (v1 media untouched).
The determinism pair, production render, encode
(`garnett-road-to-angle.v2.full/proxy.mp4`), stems (built, byte-
deterministic, `stems-full/stems.sha256`), v2 captions
(`captions-v2.json`, 69 cues — the committed v1 `captions.json` is NOT
regenerated: it captions the shipped v1 media), and the release-style
manifest are recorded below when the render mutex clears (iverson,
then cushing, hold priority).

<!-- RENDER-RESULTS -->

## 9. P7 comparison proof — front vs rear framing

`AngleV2Render.RenderFrontRankProof`: the same 30 climax seconds
(t=8640–8670 — melee locked, Armistead crossing, burst spike) from
front-rank slot 184 AND the shipped rear-rank slot 881. Slot 184 is not
ED-22-protected: verified surviving through the proof window on the v2
schedule (fallT = ∞; neighbors 180–189 mostly survive too — the harness
fails loudly rather than silently re-slotting). Side-by-side stills in
`docs/benchmarks/captures/angle-v2-data/av2-p7-*.png`.

<!-- P7-RESULTS -->

## 10. Proposed EDs and owner checklist

- **ED-82 (PROPOSED)** — the strike-correlation redistribution rule
  (§2; full text in angle-editorial-decisions.md). Adopt/reject with
  the film review.
- **Owner questions:**
  1. The v2 film itself (rear-rank slot 881, unchanged framing) — the
     review gate this branch waits on.
  2. P7 framing: rear (v1) vs front (slot 184) — §9 stills.
  3. The colors finding (§4): authorize a chain-aware victim preference
     (changes in-window draws) or accept the carried-colors depiction?
  4. ED-82 adoption (§2).
  5. Kemper's bombardment share stays 25 (a placeholder value with a
     real qualitative bound); if a Busey & Martin-grade figure ever
     surfaces, the same two-keyframe pattern swaps one number.

## 11. Residuals (disclosed)

1. Grapple pairs are sparse (3) at the wall: the proportional file map
   aligns the two rosters spatially only near the wall; distant files
   keep bout work. A spatial (nearest-file) pair map is future
   vocabulary work.
2. Armistead's melee is unpaired (one shared opponent field per
   segment; the 69th reciprocates Garnett). Disclosed in the canonical
   note.
3. The colors never pass on the pinned seed (§4 — owner question).
4. The vocab wave's demo harness (`AngleV2VocabGateRender`) stages its
   own staff/horse extras; re-running it against a bundle that ALSO
   carries colorParty/mountedOfficers would double-stage props. Its
   committed demo bundle has them only on demo units, and the harness
   is evidence-frozen — noted, not fixed.
5. `strike_correlation.py` replicates C# float32 math; any future
   change to FireCycles.CompileCannon/CompileEngagements must update
   the replication (the parity check in `DumpVocabEvidence` catches
   drift).
