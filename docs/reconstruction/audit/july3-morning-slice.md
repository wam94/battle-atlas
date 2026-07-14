# July 3 morning slice — the Culp's Hill fight, and the Pfanz-gate ED-78 unblock

**Branch:** `july3-morning` (unmerged; owner gate; content head
`8e25115` — this SHA-note commit sits one above it, the rts-camera
report convention) ·
**Script:** `tool/scripts/author-july3-morning.ts` (committed deterministic
derivation record, the `author-w*`/A1/A2/dayexp*/decomp1 pattern) ·
**ADR:** 0005 (exercised, not amended) · 2026-07-13/14

The July 3 morning honest note ("Not yet reconstructed... it becomes this
phase's content in a later slice") is replaced with a full reconstructed
phase: Johnson's renewed assaults on Culp's Hill (~04:30–13:00 LMT), the
Muhlenberg artillery ladder, the Steuart/Daniel charge across Pardee
Field, the Spangler's Meadow charge, and Shaler's VI Corps support —
against the returning Geary/Greene/Kane/Candy/Ruger XII Corps line. The
slice is unblocked by a new owner ruling, recorded as **ED-78**
(`docs/reconstruction/angle-editorial-decisions.md`), which supersedes
the standing ED-30/ED-64 Pfanz-gate BLOCK that had held the Spangler's
Meadow charge (CA-J3M-3) as unauthorable since dossier pass 9
(2026-07-11). Every other anchor in the chain (CA-J3M-1, -2, -4, -5) was
already executor-hardened from passes 8–9 and needed no new ruling.

## 1. The owner ruling (ED-78) — what it does and does not do

**Owner's words (this session, verbal):** *"make a best guess inference
for right now [before we proceed]."*

Recorded as ED-78 (`angle-editorial-decisions.md` §"July 3 morning slice
ruling (ED-78)"), ADOPTED: a provisional best-guess inference is now
permitted where the primary record conflicts and no further cheap fetch
is available, PROVIDED (1) the inference is marked `inferred`/PROVISIONAL,
never `documented`; (2) **both poles of the conflict stay on the
record**, quoted verbatim; (3) **ED-78 is cited from every place the
inference is used**. This specifically supersedes the ED-30/ED-64
Pfanz-gate BLOCK: CA-J3M-3 (the 2nd Massachusetts/27th Indiana Spangler's
Meadow charge) is now authorable at its **~10:00 direction** (the
order-chain + Bachelder-drawn-sheet pole), carrying the **~05:30–06:00
EARLY pole** (Morse's "5.30 o'clock"; the 27th Indiana's advance marker
inscribed "6 a.m.") verbatim alongside. The Pfanz *Culp's Hill*
pp. ~340–355 fetch remains open as a recommended hardening target — ED-78
unblocks authoring, it does not retire the research item, and the owner
may re-rule it (adopt-and-adjust doctrine).

**Where ED-78 is used** (the only provisional calls in this phase — every
other keyframe/event in the file is `documented`):
- `us-2ma` keyframe `t=19800` (the charge itself) and its
  `j3m-spangler-2ma` fire event.
- `us-27in` keyframe `t=19800` and its `j3m-spangler-27in` fire event.

Every other CA-J3M-2/-4 clock (Steuart's/Daniel's co-charge at ~10:00,
the general repulse at 10:30–11:00) rides existing executor primaries
(Kane's "about 10.30 o'clock... column of regiments"; Johnson's, Steuart's,
and the XII Artillery tablets' quadruple attestation) — no ED-78 needed
there.

## 2. The phase (the design call)

**Chosen: `july3-morning` (04:30–13:00 LMT) as a new reconstructed phase,
abutting the existing `july3-afternoon` phase at its UNCHANGED 13:00
startTime.**

| Phase | Clock | File |
|---|---|---|
| `july3-morning` | **04:30–13:00 LMT** (startTime 16200, endTime 30600) | `gettysburg-july3-morning.json` |
| `july3-afternoon` | 13:00–19:29 LMT (startTime 46800, endTime 23340) | `gettysburg-july3.json` — **byte-untouched** |

Front edge 04:30 (civil twilight/"daylight", CA-J3M-5, ED-31) is the
tablet ladder's own opening hour and Muhlenberg's stated 4.30 program
start. Back edge 13:00 is the existing afternoon phase's own startTime —
**never renegotiated**, since the afternoon file must stay byte-identical
(film-safety, §6). The actual fighting is over by ~11:00 (CA-J3M-4); the
window's last two hours (11:00–13:00) render the "spent and quiet" state
that the afternoon file's own t=0 citations already independently
describe as documented silence — this phase now SUPPLIES that silence's
prior content rather than assuming it.

**Cross-phase continuity is structural, not just narrative**: every
carried unit's t=0 keyframe is read directly from
`gettysburg-july2-evening.json`'s own last keyframe at build time
(`opener()` in the authoring script calls `eveningEnd(id)`, which THROWS
if the id is missing or the keyframe's `t` doesn't match that file's
`endTime` — a build-failing assertion, the same pattern the
day-expansion afternoon scripts use reading their own morning files).
The overnight (22:30 July 2 → 04:30 July 3) is a non-combat gap: position/
facing/strength carry unchanged.

## 3. The cast and authoring summary

| | Count |
|---|---|
| Units | 17 (7 CSA / 10 US) |
| Movers (position changes) | 17 (every unit moves at least once — this is a fight, not a static-park phase) |
| Keyframes | 89 (85 documented / 4 inferred — the 4 inferred are exactly the two ED-78 Spangler's-Meadow charge keyframes, `us-2ma`/`us-27in`, plus their two Muhlenberg-adjacent continuity notes are documented; see §1) |
| Events (fire windows) | 17 (2 Muhlenberg artillery + 13 brigade musketry + 2 Spangler's Meadow charge, both ED-78 PROVISIONAL) |
| Newly cast units (register `not-yet-cast` → in-build) | 1 (`us-xii-arty`, the Muhlenberg XII Corps Artillery Brigade — reg-us-xii-arty) |
| New parentless-sibling units (ED-76 convention, no register row) | 2 (`us-2ma`, `us-27in` — promoted out of `us-colgrove`, matching the `us-8oh`/`us-6wi`/`us-147ny`/`us-16me` precedent) |

**CSA — Johnson's division, renewed** (`csa-steuart`, `csa-williams`
[Nicholls, Col. J. M. Williams], `csa-walker` [Stonewall Brigade],
`csa-dungan` [Jones's, Lt. Col. Dungan]) plus the reinforcement wave
(`csa-daniel`, `csa-oneal`, `csa-smith`) — all continuing from
`gettysburg-july2-evening.json`'s cited positions/strengths (or, for the
three reinforcements, resuming fresh authoring on top of the
`strength-reconciliation-1`-cleared July-1-end basis).

**US — the XII Corps line + Shaler** (`us-greene`, `us-kane`, `us-candy`,
`us-mcdougall`, `us-lockwood`, `us-colgrove` [now minus the 2nd MA/27th
IN], `us-shaler` [VI Corps, newly authored ground — no prior phase
carried him in combat]).

**Newly cast: `us-xii-arty`** (Muhlenberg's Artillery Brigade, group
grain — the CA-J3M-1 anchor's AUTHOR unit) and the two Spangler's Meadow
parentless siblings `us-2ma`/`us-27in`.

## 4. Cited highlights (the chains as authored)

| Content | As authored | Basis |
|---|---|---|
| The Muhlenberg ladder | `us-xii-arty` opens 04:30 (`j3m-muhlenberg-open`, 04:30–04:45, 15-min burst), pauses, resumes 05:30 (`j3m-muhlenberg-resume`, to 10:00) — 20 guns (report) vs 26 (tablet) carried as an unadopted conflict; end "10 a.m." (report) vs "10.30 A.M." (tablet) carried | CA-J3M-1 (author primary, ED-65); or-27-1-muhlenberg pp. 870-871; or-27-1-williams-aw |
| Johnson's renewed assaults | Steuart/Nicholls/Walker/Jones hold the works from t=0 under escalating fire; Daniel (1.30-4.00 a.m. march) and O'Neal (2.00 a.m.-daylight march) arrive and reinforce; Smith (Early's detachment) arrives on the far right | CA-J3M-2 (three-wave structure, Johnson tablet: "two other assaults were made and repulsed"); or-27-2-daniel/-oneal/-hoffman-smith |
| Steuart's charge, Pardee Field | ~10:00 co-charge with Daniel across the ground the modern battlefield calls Pardee Field (toponym note: no OR primary uses the name — recorded as a position-class identification, not an attestation upgrade); repulsed to the wall | CA-J3M-2 wave 3; or-27-2-steuart/-daniel; or-27-1-kane's 10.30 receiving clock |
| Spangler's Meadow (PROVISIONAL, ED-78) | The 2nd Massachusetts / 27th Indiana charge under Lt. Col. Mudge (killed leading it), authored at ~10:00; repulsed by Smith's Virginians; BOTH POLES carried verbatim in every citation/event | CA-J3M-3 (ED-64 record structure; ED-78 the authoring permission); or-27-1-morse-2ma/-fesler-27in/-ruger; the 27th IN advance marker's inscribed "6 a.m." |
| The last effort / general repulse | Kane: "about 10.30 o'clock... the enemy made their last determined effort by charging in column of regiments"; Johnson's division retires east of Rock Creek | CA-J3M-4 (quadruple attestation: Johnson tablet, Steuart tablet, XII arty tablet, Jacobs) |
| Shaler's support | VI Corps brigade engaged under Geary 9-11 a.m. — the corps's one genuinely committed firing line this battle (74 losses, its heaviest) | us-vi-3-1-shaler.md (report=tablet exact agreement, pass 14) |
| Documented stillness / thin arithmetic | O'Neal's EC6 headroom is nearly exhausted by the July-1-basis subtraction alone (~2 of 696 return) — an approximate, not exact, additional decay applied and flagged (§7 residual, not silently invented) | csa-2c-rod-5-oneal.md EC6 |

## 5. Strength discipline (the exact-subtraction method, and its residuals)

Every continuing unit's t=0 strength is `gettysburg-july2-evening.json`'s
own cited value (continuity-locked, §2). The END-of-fight strength is
computed as: **the dossier's whole-battle EC6 return total, minus the
evening loss ALREADY authored in the existing July 2 files** (read
directly off those files' own keyframes — not re-derived, not invented).
For the three fresh reinforcements (Daniel/O'Neal/Smith, who did not
fight July 2 evening), the July-3 share is computed against the
`strength-reconciliation-1`-cleared July-1-end basis instead. Two classes
of honest residual follow directly from this method:

1. **`csa-oneal`'s thin arithmetic** — the return total (696) minus the
   already-established July-1 loss (1,794→1,100 = 694) leaves only ~2
   men of headroom for a brigade whose own report describes "held for
   three hours, exposed to a murderous fire." This phase applies an
   APPROXIMATE additional decay (-60, not exact-subtracted) and flags
   the inconsistency explicitly in the keyframe citation — not silently
   resolved.
2. **`csa-smith`'s pre-baked t=0** — `gettysburg-july2-evening.json`'s
   inherited value for Smith (660) was computed, before this phase
   existed, as "tablet Present ~800 minus the FULL 142-loss return" —
   i.e. it already nets Smith's entire battle loss even though Smith's
   only combat is inside THIS phase's window. Authoring a fresh full
   142-loss decay on top would double-count. This phase applies only a
   modest further decay (-40, documented-notional) and names the
   structural double-count risk in the citation and here.

**Named residuals for the future reconciliation pass** (per
`strength-reconciliation-1`'s conventions — table below; the afternoon
file is NOT touched):

| Unit | This phase's end strength | `gettysburg-july3.json` t=0 (independent) | Note |
|---|---:|---:|---|
| `csa-steuart` | 998 | 1,020 | Near-agreement (Δ22); both derive from the same brigade tablet, computed independently |
| `csa-williams` | 682 | 710 | Δ28; same tablet, independent computation |
| `csa-walker` | 1,110 | 1,120 | Δ10; near-exact |
| `csa-dungan` | 1,139 | 1,180 | Δ41 |
| `csa-daniel` | 1,378 | 1,185 | Δ193; also a POSITION-CLASS mismatch (this phase's charge geometry vs the afternoon file's Benner's-Hill-base placement) — the larger residual in this set |
| `csa-oneal` | 1,040 | 1,100 | Δ60 (= this phase's flagged approximate decay, §5.1) |
| `csa-smith` | 620 | 660 | Δ40 (= this phase's flagged pre-baked-t0 decay, §5.2) |
| `us-greene` | 1,007 | 1,125 | Δ118 |
| `us-kane` | 502 | 600 | Δ98 |
| `us-candy` | 1,261 | 1,400 | Δ139 (= the full return applied here; the afternoon file never subtracted it) |
| `us-mcdougall` | 1,675 | 1,755 | Δ80 (= the full return) |
| `us-lockwood` | 1,476 | 1,650 | Δ174 (= the full return) |
| `us-colgrove` (family) | 482+180+229=891 | 1,170 | Δ279 (= the full return; the afternoon file has no decomposition — expected, recorded) |
| `us-shaler` | 1,696 | 1,770 | Δ74 (= the full tablet loss; the afternoon file never subtracted it) |

Pattern: the afternoon file's July-3-morning-phase-adjacent units were
authored (A1/A2 era, before this phase existed) as **static blocks that
never subtracted the morning's own attested losses** — this phase is the
first to actually spend those losses inside their own window. Every row
above is the SAME evidence-layer-vs-frozen-build-value class the
`strength-reconciliation-1` report catalogued for July 1→2; it is not a
new class of problem, just this chain's first instance. None of it
touches `gettysburg-july3.json` (§6 verifies byte-identity).

## 6. Film-safety (the untouched proof)

- `gettysburg-july3.json` sha256: **worktree
  `54c3073c3ad83e931e6a879a45d909ccb02a2d667c59d861c6a64108f5daa4ab`** =
  **main (same hash)** — byte-identical, verified directly against the
  owner's main checkout.
- `app/Assets/Battle/Angle/angle.bundle.json` sha256: **worktree
  `82263b07b1b556e5b9627e8641eab66678db5f65c4118f27c0f261ba5125c82f`** =
  **main (same hash)** — byte-identical.
- **stagingSeed pin HELD**: `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  (read from the committed bundle; unaffected by construction — nothing
  in the pin's input closure was touched).
- `git diff main --name-only` on the branch: `tool/scripts/author-july3-morning.ts`,
  `app/Assets/Battle/gettysburg-july3-morning.json`,
  `app/Assets/StreamingAssets/Atlas/battle-manifest.json`,
  `app/Assets/StreamingAssets/Atlas/moments-gettysburg-july3-morning.json`,
  `app/Assets/Resources/Battle/command-overlay.json`,
  `app/Assets/Tests/Editor/{AtlasHudModelTests,CommandOverlayTests,PhaseManifestTests}.cs`,
  `app/Assets/Scripts/July3MorningCaptureHarness.cs`,
  `docs/reconstruction/angle-editorial-decisions.md`,
  `docs/reconstruction/audit/{oob-register.json,oob-register.md,unit-master-table.csv,unit-master-table.xlsx}`,
  `scripts/gen-command-overlay.py`, `tool/tests/manifest.test.ts`, this
  report. **No file under `reconstruction/claims`/`canonical`/`sources`
  or `app/Assets/Battle/Angle/` appears in the diff.**

**VERDICT: FILM-SAFE.**

## 7. Suites and evidence

Suites (all green on the branch):
tool vitest **119** · pipeline pytest **59** · reconstruction pytest
**128 + 1 skipped** · Unity EditMode **405 passed, 0 failed, 4
skipped** (the current-main floor; this slice extends three existing
test bodies — the manifest echo, overlay coverage, and moments-gate
tests now cover all six phase files — rather than adding new cases) ·
Unity PlayMode **21 passed, 0 failed, 0 skipped** (Unity runs: CLI
`-batchmode -runTests -buildTarget OSXUniversal`, worktree Library,
gitignored inputs restored — heightmap + landcover + the three
SoldierView mp4s copied from the main checkout; staging via
`CartographyStage.PrepareScene`, no `-nographics`; the batchmode scene
re-save PrepareScene produces was REVERTED before commit — a staging
artifact, not authored content).

**Shared-machine note (ops):** the first PlayMode runs failed 2-4
SoldierView seek-latency tests with 50-90-frame drifts — root cause was
THREE parallel agent sessions building/capturing on this machine
simultaneously (load average 48; their `UnityShaderCompiler` fleets and
a GPU capture harness). On a quiet machine the same tests pass 21/21
(playmode-results-final.xml). The known 1/30s flake class
(phase-switching slice report) reproduced once at exactly 1.01 frames
mid-contention — same rerun-green behavior.

Perf (`j3m-benchmark.json`, t = 0/7200/19800/21600/30600 on the new
phase, 17 units): **59.3–59.8 avg FPS** (the 59.3 lands at t=19800, the
double-charge climax), p95 frame 17.0–17.5 ms, worst 23.4 ms,
allocations flat at ~321 MB — the morning phase costs nothing measurable
against the baselines.

Evidence: `docs/benchmarks/captures/july3-morning/` (force-added) —
`j3m-timeline-0500.png` (theater view, the phase timeline),
`j3m-muhlenberg-opening.png` (the 4:30 program's first minutes),
`j3m-steuart-pardee-field.png` (the ~10:00 co-charge against the works,
the PROVISIONAL moment label rendering), `j3m-spangler-meadow.png` (the
meadow sector at the same clock), `j3m-quiet-end-1300.png` (the spent
field at the abutment seam), `j3m-day-july3-phases.png` (the July 3 day
panel: BOTH phases now reconstructed, the morning lit as loaded,
04:30–13:00 / 13:00–19:29 clocks rendering), the five benchmark
screenshots + `j3m-benchmark.json`, and the prepare/build/run logs.
Produced via `July3MorningCaptureHarness` (`-j3mShots`, the
DayExpansion3 harness mold) on one Development standalone build.

## 8. Residuals (the worklist)

1. **The Pfanz *Culp's Hill* pp. ~340-355 fetch** remains open — ED-78
   unblocks authoring, it does not retire the research item. A future
   pass that lands the fetch can revise the two ED-78 keyframes/events
   directly (both are named in §1).
2. **The cross-phase strength reconciliation table (§5)** — 13 named
   rows, same evidence-layer-vs-frozen-build-value class as
   `strength-reconciliation-1`; recommended as that pipeline's next
   wave (July 3 morning → July 3 afternoon).
3. **`csa-oneal`'s thin EC6 headroom** (§5.1) — the return total and the
   already-authored July-1 loss are in near-total tension; a B&M-class
   or per-day-split fetch would resolve it cleanly.
4. **`csa-smith`'s pre-baked t0** (§5.2) — a structural artifact of
   authoring the afternoon file before this phase existed; the cleanest
   fix is a future pass re-deriving Smith's July-2-evening placeholder
   from his tablet's ~800 PRESENT figure rather than its post-loss
   ~660, but that edits a file outside this slice's scope.
5. **Register EC2 gaps** carried forward from the dossiers themselves:
   Muhlenberg's personnel figure (no primary; ~24 men/gun reconstructed),
   several XII Corps brigades' regiment-row strength splits.
6. **In-HUD phase switching** now has SIX reconstructed phases to browse
   (ADR 0005 residual, standing).

## 9. Owner questions

1. **ED-78 scope**: is the "standing permission" framing (future passes
   may cite ED-78 directly for a NEW provisional call, not just
   re-litigate this one) the intended breadth, or should each future
   provisional call get its own numbered ED?
2. **The strength-reconciliation table (§5)**: dispatch a
   `strength-reconciliation-2` pass now, or bank it behind the next
   authoring wave?
3. **`csa-daniel`'s position-class residual** (§5, the largest single
   Δ193 + geometry mismatch) — worth a standalone look before the next
   reconciliation pass, since it is qualitatively different from the
   other rows (a placement disagreement, not just a strength one).
