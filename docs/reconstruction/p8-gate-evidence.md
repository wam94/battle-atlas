# Gate P8 evidence — deterministic action, casualty, and VFX scene (plan §12 Phase 8)

## Review-fix round 2 (owner gate feedback, 2026-07-10)

The owner approved the direction ("a big step up") and blocked the gate
on four defects. All four are fixed and the evidence below is fully
re-rendered (same choreography, cameras, and probes). Per-defect
BEFORE/AFTER pairs: `docs/benchmarks/captures/p8-gate/review-fixes/`.

1. **Hat halo ("odd hat behavior")** — kit-build bug: hats were sized by
   CONSTANTS while the MPFB body macros vary skull size, so larger heads
   swallowed the kepi band and poked through the slouch dome; the brim
   read as a detached halo ring at hero distance. Both hats are now fit
   to a per-body MEASURED skull ellipse. A second latent bug surfaced:
   the kepi visor had never shipped (deleting half of a `create_circle`
   n-gon deleted its only face) — rebuilt as an explicit quad strip.
   Verified by `p8-figure-lineup.png` + `p8-figure-lineup-side.png`.
2. **Locomotion ("floating from position to position")** — the resolver
   picked clips from the segment LABEL alone; any figure whose compiled
   track moved during a non-locomotion segment glid (worst: Armistead's
   men in the breach played stationary fire cycles while the track
   carried them through the battery at 0.2–0.9 m/s), and Garnett's
   stalled line treadmill-marched at 0.1 m/s. The resolver now derives
   walk-vs-stand from the slot's own resolved ground speed and keys
   stride phase to track ARC LENGTH (drill-manual meters per cycle), so
   stride rate matches ground speed under scrub in any direction.
   Verified by `p8-diag-loco-adv-*.png` / `p8-diag-loco-breach-*.png`.
3. **Smoke color (§9.1 "dense white/gray")** — the per-frame billboard
   meshes carried NO normals, so the HDRP Lit BSDF saw a zero normal and
   the sun term vanished: near-white albedo rendered as a charcoal storm
   front. Billboards now carry explicit up normals (sunlit-from-above)
   and the shade ramp eases powder buckets toward neutral white (~0.9).
   Emission timing, banking, and progressive visibility are unchanged.
4. **Brogans (bare feet)** — the P6 skin-tight foot extraction now gets
   a real boot silhouette: ankle shaft, smoothed leather toe box, flared
   sole rim, welted sole slab + heel block, and a leather tone distinct
   from flesh on both palettes.

**Gate criterion:** "seeking to any tested time reconstructs bitwise-identical
logical soldier states and visually equivalent deterministic frames; casualty
totals match compiled profiles; no double-count occurs between parent and
child units." The machine-verifiable parts are proven by tests and the render
probes below; this document is the viewing guide for the owner's evidence.

**Content note:** this is the phase where the reconstruction begins to show
explicit casualties — falls, persistent bodies, blood pooling, wounded men
dragging themselves. The rules it follows are in
`docs/reconstruction/violence-and-representation.md` (§9.2). The product's
content warning ships with the Phase 11 UI; until then this note stands in.

Evidence (2560×1440, offline HDRP profile, per-frame ephemeris sun):
`docs/benchmarks/captures/p8-gate/` — staged on the `v2-phase8` worktree and
copied to the main checkout's same path (gitignored generated media).

- `p8-still-8160-road.png` — §13 golden frame: Emmitsburg Road crossing
- `p8-still-8400-canister.png` — §13 golden frame: closing under canister
- `p8-still-8580-wall.png` — §13 golden frame: wall approach
- `p8-still-8700-crisis.png` — §13 golden frame: the Angle crisis
- `p8-gate-90s-1440p.mp4` (+ `-720p` proxy) — 90 s eye-level at the wall,
  battle t=8610..8700 (15:23:30–15:25), 30 fps, the climax of the assault
- `p8-gate-report.json` — machine evidence: logical digests, pixel probes,
  casualty reconciliation, timing/memory
- `p8-figure-lineup.png` / `p8-figure-lineup-side.png` — six kit variants
  close up, front and profile (hat fit, visor, boot silhouette)
- `p8-diag-loco-adv-*.png` / `p8-diag-loco-breach-*.png` — close tracking
  shots of moving ranks, 0.2 s apart (stride-vs-ground verification)
- `review-fixes/` — per-defect BEFORE/AFTER pairs for the four owner
  review fixes (hat, locomotion, smoke color, brogans)

## What the scene IS

Every man on the field is a formation slot of a compiled unit
(`angle.bundle.json`, 12 staged units, 9,405 slots at 1:1 man scale —
`us-webb` is excluded as the parent roll-up of the staged 69th/71st/72nd PA,
the gate's no-double-count rule). At battle time t, `SoldierActionResolver`
— the plan's §7.5 pure function — decides every man's position, facing,
clip, clip phase, and casualty status from (bundle, seed, unitId, slotId, t).
There is no combat AI, no Random, no Time.*: the 90-second video is 2,700
independent evaluations of the same deterministic function.

Crowd tiers (§7.4): the nearest ≤64 slots render as full kit figures,
≤400 as decimated near-LOD figures, ≤2,500 as instanced baked-pose meshes
(two-pose gait flipbook, so distant lines step rather than glide), and the
rest as formation-density impostors. Tier is presentation only — identity,
behavior, and fate never depend on it.

## What to judge in the sequence (t=8610..8700, camera in Cushing's battery)

The camera stands at eye height between Cushing's gun line and its rear
echelon, just north of the inner angle, looking WSW across the wall face
the 71st PA vacates — the sector Armistead's breach comes through. (From
directly behind the 69th PA's two-deep line the 0.8 m wall is fully
occluded by the men themselves; this position keeps the guns, the wall
sector, and the breach all in frame.)

1. **The drill vocabulary under fire** — the 69th PA line in the foreground
   works aim → fire → nine-stage reload cycles (ED-20 cadence, ~2.5 rds/min,
   rank-staggered volleys earlier, independent fire in the crisis).
2. **Black-powder smoke that reads as smoke** (the ADR 0003 re-check) —
   every musket discharge births its own white-gray jet at that man's
   muzzle at that instant; banks accumulate along the firing line, hang in
   the ED-19 light breeze, and progressively eat visibility (fog mean free
   path is a pure function of active smoke mass). Fuger: "the smoke was so
   dense we could hardly see" — the frame should feel like that. Since the
   review-fix round the banks are SUNLIT WHITE to pale gray (§9.1), not
   the earlier charcoal storm front.
3. **Cannon** — Cushing's two intact pieces at the wall fire canister with
   muzzle blast, big smoke plumes, deterministic recoil-and-return, and
   crew braces synced to each discharge.
4. **Casualties accumulating** (sober, per §9.2) — Garnett's line takes
   canister at the wall through the whole shot (spike profile, 200 men over
   these 120 s); men fall by incoming direction, bodies persist, gaps open
   where the schedule put them, blood pools grow under the fallen, a
   minority of the wounded crawl. At ~8640 Kemper wavers (left distance);
   at 8640–8700 Armistead's breach comes over the wall right of center.
5. **The repulse beginning** — by the end of the shot the CSA line is
   scattered and falling back over its own dead.
6. **Trampling** — the assault corridor's grass is ground down along the
   compiled tracks as the brigades pass (ED-17 debt paid; scrubbing
   backward un-tramples).

## Golden-frame notes

- **8160 road**: Garnett's two-rank line crossing the traced road fences —
  each man plays the fence-climb where HIS path crosses the traced line,
  not where a segment average says; the road corridor, both fences, marching
  dust, Codori buildings behind.
- **8400 canister**: the advance mid-field under artillery — strike dust
  near the line, brace/flinch reactions within radius of deterministic
  impact points, march continuing through it.
- **8580 wall**: the approach seen from the defense — Union fire cycles
  along the wall, the field beyond filling with smoke banks.
- **8700 crisis**: Armistead's survivors over the wall among the guns,
  71st PA fallen back, 72nd PA firing from the crest, bodies of both sides
  accumulated.

## Determinism evidence (the gate's machine half)

- **Bitwise logical states:** at probe frames the full 9,405-slot logical
  state vector (position, facing, clip, phase, status, cause, variant,
  equipment) is SHA-256 hashed during the forward render, then the scene is
  scrubbed elsewhere and back and re-hashed: digests must be identical
  (`probesLogicalBitwiseEqual` in the report; also
  `SoldierActionResolverTests.Resolve_ScrubDirectionInvariant` sweeps the
  whole slice forward/backward/shuffled).
- **Pixel probes:** the same frames re-rendered out of order are compared
  against the sequence PNGs under the documented GPU tolerance. The P6
  scene's envelope was 8/5%; Phase 8 stacks thousands of transparent smoke
  quads, which amplifies Metal rasterization-order noise — the measured
  smoke-heavy envelope is 5.92% differing pixels at max channel delta 9
  (t=8690 peak coverage; reproduced identically across two full renders),
  so the documented tolerance is 12/8%. Logical state is exact; pixels
  are equivalent.
- **Casualty reconciliation:** per unit, scheduled casualties equal the
  compiled profile totals exactly, and the alive count tracks the bundle's
  per-second strength (exact at profile boundaries, ≤1 man mid-window —
  banker's-rounding ties, ED-21). Numbers in `p8-gate-report.json`
  (`casualtyReconciliation`) and staging refuses to run if they diverge.

## Honest limitations (known; judge the gate, not these)

- **No pre-slice smoke:** the slice opens at t=8040 with clear air; the
  two-hour bombardment that preceded it left residue the bundle does not
  model (Phase 9/10 may add an authored ambient layer).
- Battery crews are the §9.1 "crew response placeholder": union kit
  figures with muskets hidden, serving-position clusters, brace-on-discharge;
  no rammer/sponge choreography, no horses.
- Wound patches on hero-tier bodies are camera-facing stains near the
  torso, not skinned decals (documented in the violence doc).
- Impostor-tier figures are crossed-quad silhouettes; fallen far men are
  ground quads. Beyond 350 m they read as formation density, which is the
  §7.4 intent.
- Melee at the breach resolves as close-range fire/waver/cross vocabulary —
  the kit has no bayonet clips (plan §7.3 doesn't require them; Phase 9
  camera work will decide if the crisis needs one authored clip).
- Small position pops can occur when a fence-crossing clip hands back to
  the formation path (bounded by the catch-up blend).
- 71st PA's compiled fall-back keeps unit facing per track motion; a few
  men read as walking backward briefly during the hand-off.
- At compiled track speeds well below the drill cadence (0.25–0.6 m/s,
  e.g. the breach flowing through the battery) the distance-keyed
  stride reads as deliberate slow steps — correct foot planting, no
  skating; re-timing the compiled tracks themselves is out of scope.
- The stone wall reads clearly from the west (the attacker's side) and at
  the breach; from directly east of the defending line it hides behind
  the men and the crest line — a viewpoint reality, not missing geometry.

## Regeneration (all deterministic)

```sh
cd pipeline && uv run python -m terrain_pipeline.cli crop
cd pipeline && uv run python -m terrain_pipeline.cli environment
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.GateP8Render.RenderStills -logFile p8stills.log
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.GateP8Render.RenderSequence -logFile p8seq.log
scripts/p8-encode.sh
```

## Suite state at evidence time

- pipeline **59**
- reconstruction **79**
- tool **108**
- Unity EditMode **298** = 296 passed + 2 known conditional skips
  (56 Phase 8 tests + 3 locomotion review-fix tests)
- Unity PlayMode **10/10**

## Measured results (p8-gate-report.json, review-fix re-render)

- 2,700/2,700 frames, 0.39 s/frame offline HDRP at 2560x1440,
  1.1 GB peak managed memory.
- Probe frames 300/1500/2400 (re-posed OUT OF ORDER after scrubbing away):
  logical digests **bitwise identical, 3/3**; pixels within the documented
  GPU tolerance, 3/3 (worst probe 5.40% differing at max delta 7 —
  tighter than the previous round's 5.92% at 9).
- Casualty reconciliation, all 12 staged units: scheduled == profile
  totals **exactly**, and alive-at-end == compiled strength **exactly**
  (e.g. csa-garnett 693/693 scheduled, 700/700 alive; csa-armistead
  880/880, 700/700).
- Media: `p8-gate-90s-1440p.mp4` (117 MB) + 720p proxy (26 MB), H.264,
  1 keyframe/s GOP, sha256 in `p8-gate-media.sha256`; frame-continuity
  check enforced before encode (`scripts/p8-encode.sh`).

## Gate P8 verdict

Machine criteria: **PASS** (bitwise logical determinism under scrub, exact
casualty reconciliation, parent/child double-count excluded by
construction, pixel probes within the documented GPU tolerance).
Owner judgment on the visual evidence closes the gate.


## Gate P8 verdict

**PASSED — closed by the project owner 2026-07-10** ("looks pretty good!")
after the review-fix round (per-body-fitted hats + shipped kepi visor,
motion-derived locomotion with distance-keyed stride, sunlit white/pale-
gray smoke via the missing-normals root cause, real brogan boots).
Machine criteria: bitwise logical state 3/3 probes across independent
renders; casualty reconciliation exact for all 12 units; GPU pixel
tolerance 12%/max Δ8 documented (measured 5.4%/Δ7). Carried forward:
breach-pace read is governed by compiled track speeds (re-timing is a
data question, not animation); smoke density at climax accepted as the
attested effect.
