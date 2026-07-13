# Day expansion slice 1 — structural foundations (the sunset widening, the phase model, the re-basing)

**Branch:** `day-expansion-1` (unmerged; owner gate) ·
**Scripts:** `tool/scripts/author-dayexp1-sunset.ts`,
`tool/scripts/author-dayexp1-rebase.ts` (committed deterministic
derivation records, the `author-w*`/A1/A2 pattern) · **ADR:**
`docs/adr/0005-multi-day-phase-manifest.md` · 2026-07-12

Three coupled structural changes plus one atomic data operation, per
the slice charter (motivated by `authoring-wave-a2.md` §6.4 + §2's
deferred list):

1. **July 3 widened** from 13:00–16:00 (endTime 10800) to
   **13:00–sunset 19:29 LMT (endTime 23340**, the ED-31 astronomical
   pin); `startTime` 46800 unchanged — every existing `t` means what it
   meant.
2. **The multi-day/multi-phase model** (ADR 0005): a phase manifest
   over per-phase battle files; July 1/July 2 as honest empty days;
   July 3 with an empty morning phase + the reconstructed afternoon.
3. **Day navigation in the Atlas**: manifest-driven day tabs, honest
   "not yet reconstructed" states, the phase panel.
4. **The Pettigrew/Trimble strength re-basing** (A2 cut 2) — executed,
   with the film-safety tripwire verified GREEN (§3).

## 1. The phase model (ADR 0005, the design call)

**Chosen: a phase manifest over per-phase battle files** — the battle
file format is unchanged (one file = one phase = one clock); a new
manifest document (`StreamingAssets/Atlas/battle-manifest.json`,
schema `docs/format/battle-manifest.schema.json`, format doc
`docs/format/battle-manifest.md`) lists days > phases, where a phase
is either a battle file whose clock the manifest must ECHO exactly
(test-enforced both sides — the manifest can never lie about time) or
an honest `not-reconstructed` note (schema-required — an empty day is
inexpressible without saying so). Rationale in the ADR: the shipped
film's ground truth stays byte-stable by construction; authoring waves
stay one-file reviewable diffs; per-day facts land in their own
phase's file (the §6.4 morning-pins wish) instead of t=0 surgery.

The determinism contract holds: everything rendered is a pure function
of (phase, one battle clock). The July-3-afternoon phase's internal
clock is IDENTICAL to the shipped one — the film viewpoint t=8160..8820
still means 15:16–15:27.

## 2. The sunset window's new contents (all cited in-file)

| Content | What was authored | Basis |
|---|---|---|
| `us-btty-mccartney` (NEW unit, 191 total) | Weickert-farm reserve ground → the cemetery's left, arriving ~16:00 AFTER the repulse; four long-range rounds as a brief fire event (t=11100–11400, inferred window); strength flat 145 (no casualties, tablet) | or-27-1-mccartney No. 238 ("the enemy had been repulsed before the battery took position" — the corpus's clearest arrived-too-late record); battery tablet; dossier us-btty-mccartney.md |
| The ~17:00 Wheatfield sweep | `us-mccandless` advances across the Wheatfield (t=14400–16200) and sweeps the woods south, holding the recovered ground to sunset (strength flat — July 3 loss "light", NO day split); `us-nevin` follows at 200 yards UNDER BARTLETT (the composite command, both-sided) and decays by Bartlett's "between 20 and 30" (midpoint 25 → 1345); the recovery ledger (~200 prisoners, the 15th GA colors, the 9th MA materiel) rides the citations | or-27-1-crawford pp. 653-655; or-27-1-bartlett No. 226; or-27-1-mccandless p. 657; tablets; timing spread (stated 5 P.M. +75 profile / Bartlett ~17:00–18:00 / Nevin 6 P.M.) carried, 17:00 adopted inferred |
| Benning's retirement | the 15th GA caught during the withdrawal (−220 inferred against the attested ~200-prisoner capture mass + "considerable loss"); the brigade retires t=16200–19800 to its tablet's own ground (tab-csa-benning 2785,2269, "retired to position near here"); clock inferred vs the attested sweep, conflict carried | csa-1c-hood-4-benning.md; brigade tablet verbatim; or-27-1-crawford (the Union count) |
| Taft's cease | flank-fire event t1 10800 → **18000**: his report's ~6 P.M. cease is primary and now in-window; the marker's "until 4 p.m." (A2's window-edge compromise) carried as the counter-reading; the t=18000 keyframe carries the itemized expenditure ledger | or-27-1-taft No. 325 pp. 891-892; marker verbatim; dossier us-btty-taft.md EC5.2 |
| Wheeler's remainder | canister event t1 → 11400 (inferred ~10 min); the "post-window remainder not modeled" annotation retires; the clock-error alternative (mid-fight arrival) stays open | or-27-1-wheeler; dossier us-btty-wheeler.md Conflicts §1 |
| All-day skirmish scope | Hays's town sharpshooting + Perrin's/Thomas's Long Lane events extend to sunset — the tablets' own day scope ("constantly engaged" / "most of the day"), previously clipped by the window | tablets verbatim (the wave-5 citations) |
| Evening states (11 keyframes, t=23340) | the assault column's rally records land as cited keyframes instead of a silent clamp: Pickett's three stay dissolution-class `scattered` (Peyton's ~300, the ED-8 record); Fry/Marshall/Davis/Lane/Lowrance/Wilcox/Lang/Brockenbrough reform as thin `line`s; Lang's NIGHT withdrawal falls past sunset and is cited, not modeled | peyton-or-1863; or-27-2-shepard/-jones-26nc/-davis-jr/-lane-jh/-engelhard/-lowrance/-wilcox/-lang; pass-3 dossiers |
| moments.json | 4 cited markers: 16:00 "The field falls quiet" (Davis's 'about 4 p.m.'), ~17:00 "The Wheatfield sweep", 18:00 "Taft's guns cease" (conflict carried), 19:29 "Sunset" (ED-31) | as listed in-file |

NOT authored (honesty cuts, recorded): Farnsworth's ~17:00+ charge and
the South Cavalry Field evening remainder (wave-6 scope; needs its own
dossier read — slice-2 worklist); Lang's night withdrawal (past
sunset); the Ziegler's Grove convergence (A2 cut 1, still awaiting the
decomposition wave — Williston/Butler/Martin/Harn remain non-units);
July-3-morning content (the empty `july3-morning` phase's future
worklist, A2 §2 item 9).

## 3. The re-basing and THE FILM-SAFETY VERDICT

**Verdict: GREEN — the re-basing was applied and the shipped film is
untouched.** Verified after recompile (`compile_angle.py`):

- bundle payload **byte-identical** (`units` array compared
  key-for-key against the pre-rebase bundle; only `inputs.battle`
  sha256 and `checksum` changed — the A2 metadata-only precedent);
- **stagingSeed pin HELD**: `d470c469…f7ac1` verbatim (ED-21);
- per-second states across the shipped viewpoint window
  (t=8160..8820) extracted and diffed for all 13 cast units:
  **identical** (x, z, facingDeg, strength, segmentIndex);
- reconciliation green: macro(8040) = 1393.0 = compiled start, exact;
  every in-slice keyframe value unchanged (audit table,
  `angle-bundle-audit.md`);
- no recon/claims file was touched.

The design that made it safe: csa-garnett's ED-46 delta is absorbed
OUTSIDE the compiled slice — a new t=8040 keyframe pins the slice
edge at exactly the compiled startStrength (1393, which is the recon
file's own stated basis: "macro track linear interpolation at
t=8040"), at the exact pre-pin interpolated pose. Pre-slice, the
attested bombardment −20 (Peyton) replaces the interpolated −25;
post-slice, the dissolution re-derives to the return-grade end.

| Unit | Old base → new | Old end → new | Basis |
|---|---|---|---|
| csa-garnett (CAST) | 1,480 → **1,427** | 539 → **486** (= 1427−941) | ED-46 (peyton-or-1863 primary); or-27-2-anv-return p. 339; slice pinned 1393@8040 |
| csa-lowrance | 1,250 → **500** | 651 → **315** | PRIMARY or-27-2-lowrance ("about 500 men", July 1 evening); July-3 loss ~185 INFERRED-BOUNDED (no primary total; the 535 return is 3-day with Scales's July-1 table LARGER — the ED-49 exemplar) |
| csa-brockenbrough | 880 (kept, ED-48a) | 780 → **732** | ED-48b: the return's 148 k+w consumed as the loss FLOOR (the old 100 decay sat below it) |
| csa-fry (CAST) | 1,048 unchanged | 371 unchanged | or-27-2-shepard primary — but JULY-1-scoped, recorded verbatim (same heterogeneity class as Marshall/Davis) |
| csa-lane | 1,355 confirmed | 695 confirmed | ED-47 (660 battle total; the return's 389 = k+w component) |
| csa-wilcox | 1,200 confirmed | 996 confirmed | or-27-2-wilcox primary; 1,777−577=1,200 exact; July-3 204 (Alexander concurs) |
| csa-lang | 400 confirmed (700 battle basis − July-2 ~300) | 245 confirmed | or-27-2-lang primary (= return exactly); July-3 share ≈ 155 |
| csa-marshall | 2,000 NOT re-based | — | no primary (July-1 'present' figure; ~900 by [B] subtraction) — annotated in-file as an OWNER-RULING residual |
| csa-davis | 2,000 NOT re-based | — | no primary; July 3 included the fresh 11th Miss — annotated in-file, residual |

The joint "not numbering in all 800 guns" works statement (Lane +
Lowrance) is carried on both climax keyframes as a **present-measure
scope note** (ED-49 discipline) — the strength tracks are the
attrition measure, a different scale; authoring the 800 as a value
would have silently redefined `strength` for two units.
Garnett's five regiment children and Brockenbrough's two wings
re-split from their new parent tracks (display-grain convention;
in-slice child values land unchanged).

## 4. Day-navigation UX (what the owner will see)

- **Day tabs** in the masthead (July 1 / July 2 / July 3), built from
  the manifest; the day owning the loaded phase reads as the lit chip;
  days with nothing reconstructed render muted but stay clickable.
- Clicking any day opens the **day panel**: the day's phases with
  status words — "reconstructed — the loaded phase" (with its clock
  range, e.g. "13:00–19:29 local mean time") or "not yet
  reconstructed" with the manifest's own note verbatim. July 3 shows
  both its empty morning phase and the active afternoon; July 1/2 show
  their honest empty states. Closing returns the Atlas untouched (the
  clock never moves).
- The manifest's clock echo is cross-checked once against the loaded
  battle at runtime; a mismatch warns loudly and renders inside the
  panel (the manifest may never lie about time).
- Degradation: a missing/rejected manifest = no tabs, a warning, and
  everything else keeps working (the moments.json pattern).
- Phase switching between MULTIPLE reconstructed phases is
  deliberately deferred until a second reconstructed phase exists
  (ADR 0005) — no dead UI shipped.

## 5. Suites

| Suite | Before (baseline) | After (this slice) |
|---|---|---|
| tool vitest | 110 | **118 passed, 0 failed** (6 manifest tests + the widening and re-basing content blocks) |
| reconstruction pytest | 122 + 1 skip | **122 passed, 1 skipped** (bundle recompiled twice — the widening and the re-basing — METADATA-ONLY both times: `inputs.battle` + `checksum`; `test_committed_bundle_matches_recompilation` is the forcing test) |
| pipeline pytest | 59 | **59 passed** |
| Unity EditMode | 369 + 4 skips | **375 passed, 0 failed, 4 skipped** (+6 PhaseManifestTests incl. the committed manifest's clock echo; the 4 skips are the HDRP-bake AngleEnvironmentTests, expected on this rig; the McCartney unit surfaced a command-overlay coverage failure on the first run — fixed by regenerating the overlay + the register castStatus, exactly what the coverage test exists to force) |
| Unity PlayMode | 16 | **17 passed, 0 failed** (+ the day-tab honest-empty-state flow; the media sync/seek tests ran against the REAL staged full media, `garnett-road-to-angle.full.mp4`) |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal`,
worktree Library, gitignored inputs restored — `data/heightmap`,
`data/landcover`, `app/Assets/Generated`, SoldierView proxies + full
media; logs `editmode2.log`/`playmode.log` + `*-results*.xml` in the
worktree, gitignored by design.)

## 6. Evidence

`docs/benchmarks/captures/day-expansion-1/` (force-added; owner copies
in the main checkout's same gitignored path):
- `dayexp1-timeline-charge-1520.png` — the widened timeline: day tabs,
  the hero window band now at ~35% of the 13:00–19:29 bar;
- `dayexp1-quiet-1600.png` — 16:00, the field falls quiet;
- `dayexp1-sweep-1730.png` / `dayexp1-ridge-rally-1730.png` — ~17:30:
  the Wheatfield sweep and Benning's retirement; the reformed thin
  lines vs Pickett's scattered survivors on the ridge;
- `dayexp1-taft-fire-1745.png` — Taft's evening fire on the pike;
- `dayexp1-sunset-1929.png` — the 19:29 sunset light at theater zoom;
- `dayexp1-day-july1-empty.png` / `dayexp1-day-july3-phases.png` — the
  honest empty-day states;
- `dayexp1-benchmark.json` — perf at t = 0/8400/10800/16200/23340.

## 7. Residuals (what slices 2–3 need)

1. **Marshall/Davis re-basing needs an owner ruling** (annotated
   in-file): Marshall's ~900-by-subtraction is [B] tier; adopting it
   is a policy call (subtraction-tier bases) the executor must not
   default.
2. **South Cavalry Field post-16:00** (Farnsworth's charge, Merritt's
   evening) — the wave-6 events still end at 10800; needs a dossier
   read before authoring (this slice's honesty cut).
3. **July-3-morning phase content** (the manifest's empty
   `july3-morning`): the A2 §2 item-9 worklist (Culp's Hill fire,
   Taft's 8 A.M. window, Shaler's morning fight) — plus the
   morning-position anchors already sitting in pass-3 dossiers (j3-01/
   j3-02 sheet reads).
4. **Phase hot-swapping** lands with the first second reconstructed
   phase (ADR 0005): BattleDirector re-init from a manifest phase
   selection, per-phase moments files, per-phase command overlay.
5. **Per-day loss ledgers on the unit** (A2 §6.4's second observation)
   — a battle-format change, deliberately not taken this slice.
6. **Wheeler's re-timing alternative** (clock-error reading: arrival
   mid-repulse) stays open on the dossier.
7. **Wilcox's understated advance geometry** (dossier EC3.3 authoring
   flag: the build's ~170 m net displacement vs the documented ravine
   advance) — untouched this slice (geometry, not strength).
8. Lowrance's inferred July-3 loss (~185) should be revisited if a
   primary surfaces (Winchester/parole records are the pass-3
   standing fetch item for exactly this capture-mass uncertainty).
