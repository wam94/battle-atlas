# Angle-v2 vocabulary wave (P3 melee · P4 colors · P5 mounted officer · P6 halt-and-fire)

**Status:** executor evidence for the `angle-v2-vocab` slice — the
ANIMATION/BEHAVIOR VOCABULARY ONLY. The owner authorized the Angle film
v2 program; this wave builds clips, resolver classes, tests, and staged
evidence. **It wires nothing into any committed bundle or battle file**
— the v2 DATA wave (next, separate) rewires content in one consolidated
recompile. One recompile, one re-render, later.
**Branch:** `angle-v2-vocab`. Film-safety: the Angle bundle (stagingSeed
pin `d470c469…`), the Iverson bundle, and all six phase battle files are
byte-identical to main (§7).

Sources: `docs/reconstruction/audit/charge-intensity-proposals.md`
(P3/P4/P5/P6 + the behavior evidence bank §2),
`docs/reconstruction/fight-prone-vocab.md` (the pattern followed),
`docs/reconstruction/violence-and-representation.md` (V&R, binding).

---

## 1. Clip inventory (10 new kit clips + 4 horse clips; kit total 26 → 36)

Authored in `characters/kit/clips.py` (human kit, all six variants
rebuilt via `build_kit.py`, headless bpy 4.5.11 + MPFB per ADR 0004) and
`characters/kit/horse.py` (the new project-owned procedural horse rig —
no external asset, per the CC0/CC-BY-only rule). P6 conventions carry:
articulated transitions, `_drop_musket` for slipped weapons,
deterministic authoring (no random anywhere), Workbench review frames
iterated before the FBX bake (two review rounds: the round-1
clubbed-swing grip and parry-reach defects were caught in preview and
re-authored).

### P3 — melee (5 clips)

Citation: "hand to hand, and of the most desperate character; ... men
climbing over the wall, and fighting the enemy in his own trenches" —
Peyton, or-27-1863 (`csa-1c-pic-1-garnett.md` EC5.4); the clubbed-guns
melee vocabulary is also Fry's (`csa-3c-het-3-fry.md` EC6).

| ClipId | FBX action | dur | loop | what it depicts |
|---|---|---|---|---|
| MeleeClubSwing (26) | `Melee_Club_Swing` | 2.6 s | no | the piece reversed, both hands on the barrel; one overhead stroke that **stops at the horizontal** (contact abstracted), recover to the clubbed ready |
| MeleeBayonetThrust (27) | `Melee_Bayonet_Thrust` | 2.0 s | no | on guard at the hip; one short thrust to full extension (stops there), withdraw |
| MeleeGrappleA (28) | `Melee_Grapple_A` | 2.8 s | yes | the lead half of a locked pair: braced low, own piece crosswise, driving |
| MeleeGrappleB (29) | `Melee_Grapple_B` | 2.8 s | yes | the follower half — anti-phase sway, piece a hand lower, so the clinch reads as a bind, not a mirror |
| MeleeParry (30) | `Melee_Parry` | 2.0 s | no | the defensive half: piece presented two-handed overhead, the jolt of a caught blow (ON the piece, never a man), shove off |

**Sober treatment (V&R §5/§6):** contact is abstracted — every stroke
stops short of a body; the resolver's pairing never closes two figures
under 1.05 m; there is no impact pose, no gore, no new wound class.
The melee reads as struggle, not spectacle.

### P4 — colors falling and passing (3 clips + a scene prop)

Citation: "Every flag in the brigade excepting one was captured at or
within the works" — Shepard, or-27-2-shepard (`csa-3c-het-3-fry.md`
EC5.4): 1st Tenn 3 bearers down, 13th Ala 3, 14th Tenn 4, 7th Tenn 3.

| ClipId | FBX action | dur | loop | what it depicts |
|---|---|---|---|---|
| ColorsCarry (31) | `Colors_Carry` | 2.0 s | yes | staff at the chest (right-hand grip; left steadying at the waist), musket slung across the back, a slow proud sway |
| ColorsBearerFall (32) | `Colors_Bearer_Fall` | 2.2 s | no | the bearer goes down: the staff hand holds through the buckle and releases at 0.9 s; he ends face-down, an arm extended toward the fallen colors. Persistent final frame |
| ColorsPickup (33) | `Colors_Pickup` | 2.0 s | no | the next man stoops, grasps at 0.9 s, rises into the carry (ends exactly on the shared carry pose — seam-free into `Colors_Carry`) |

The **staff and flag are a scene prop**, not a kit prop bone: the clips
shape the hands for a vertical staff through the right-hand grip, and
the render harness stages the staff there (carried / tipping with the
fall / flat on the ground / rising with the pickup). There was **no
per-figure color-bearer slot in the engine before this wave** — the
task brief's "the color-bearer slot already exists in rosters/flags"
premise was checked and found to describe only the atlas-level
per-UNIT flag (`BattleDirector.flagMaterial`), not a Soldier-View
figure role; `ColorGuard.cs` (§2) adds the figure-level role.

### P5 — mounted officer falls (2 rider clips + 4 horse clips) — OWNER-APPROVED

Owner ruling 2026-07-15, relayed verbatim in the wave brief: **"ship p5
mounted officers falling"** — the V&R §1 named-officer rule's
separate-approval trigger (charge-intensity-proposals.md P5). The
proposed **ED-81** (`angle-editorial-decisions.md`, marked PROPOSED, not
self-adopted) records the ruling and the anonymity/sobriety bounds.

| Clip | rig | dur | loop | what it depicts |
|---|---|---|---|---|
| RideSeat (34) `Ride_Seat` | kit | 2.4 s | yes | upright seat at saddle height (root raised to `RIDER_SADDLE_Z`), reins in the left hand |
| RiderFall (35) `Rider_Fall` | kit | 2.4 s | no | the hit at 0.3 s; he tips left off the saddle from 0.55 s, one articulated slide to the ground, lies still. **Reads as loss, not ragdoll spectacle** — no thrash, a single slump, persistent body |
| `Horse_Stand` | horse | 3.0 s | yes | easy halt: head bob, tail sway, breathing |
| `Horse_Walk` | horse | 1.6 s | yes | 4-beat walk in place (LH LF RH RF), head nod |
| `Horse_Rear` | horse | 2.5 s | no | the hit: gathers, rears (pivot verified at the rear-hoof line, ±4 cm), paws once, drops back |
| `Horse_Bolt` | horse | 0.8 s | yes | the riderless bolt: gallop reach, neck flat, tail streaming |

The horse (`horse.py` → `horse.fbx`) is a **minimal project-owned
procedural mesh + 13-bone FK rig** modeled from public-domain
proportions — no Asset Store, no third-party mesh (the CC0/CC-BY rule
is satisfied by owning the asset outright). The horse is never a
casualty in this vocabulary (proposed ED-81 §4).

### P6 — halt-and-fire (0 new clips — verified covered)

Citation: "our men stopped and began firing, instead of mounting the
fence; while making efforts to get them over the fence I was wounded" —
Trimble, trimble-shsp26-1898 (`csa-3c-pen-hq-trimble.md`).

The clip needs are covered by the existing vocabulary, verified case by
case: the halt is `Halt_DressLine`, the fire is the standing
`Aim_Musket`/`Fire_Recoil`/`Reload_Musket` cycle, the ready gaps are
`Stand_Ready`, the eventual crossing is `Cross_RailFence`, and a recoil
instead of a crossing is the existing `fall_back` grammar. The missing
piece was the **segment class** (§2), not art.

### Crowd tiers (mid/far)

- `build_kit.py` `MID_POSES` gains **`pose_melee`** (`Melee_Club_Swing`
  @ 1.05 s — mid-stroke): a wall fight at 100 m+ reads as a scrum, not
  a firing line. Baked into both `*_a_mid.fbx`; `AngleActionScene.LoadKit`
  now requires it.
- `CrowdTiers.MidPose`: melee clips → `pose_melee`; colors carry/pickup →
  `pose_march_a`; `Colors_Bearer_Fall` → `pose_fallen_back`;
  `Ride_Seat` → `pose_march_a`, `Rider_Fall` → `pose_fallen_side`
  (defensive only — the mounted officer is a hero/near figure by
  construction, one per brigade at most; the horse itself is
  harness/scene-staged and never enters tier bookkeeping in this wave;
  the data wave must decide far-tier treatment if it stages officers at
  distance).

## 2. Resolver classes (`SoldierActionResolver` + three new pure classes)

All additive; every existing action path resolves bit-identically (§7).
Everything is a pure function of (bundle, seed, unitId, slotId, t) —
scrub-invariant, no state, no Random.

### `melee` segment action (`MeleeChoreo.cs`)

- **Grapple pairs, deterministic from slot hash:** a `GrappleShare`
  (22%) hash subset of FRONT-RANK files grapples when the segment is
  wired to an opponent (`segment.meleeOpponentId`, set symmetrically by
  the data wave). The ordinally-lesser unitId leads (`Melee_Grapple_A`);
  the follower file is the proportional roster map; both sides run the
  same first-candidate-wins selection, so they always agree. The pair
  meets at the midpoint of their two roster positions (locked at the
  pair's staggered start), stands `PairSeparationM` = 1.05 m apart
  facing each other, and dissolves when either man falls (the survivor
  walks back and rejoins the bout work).
- **Bounded bouts for everyone else:** clubbed swing / bayonet thrust /
  parry by hash (one third each), staggered 0–3 s, separated by ready
  pauses of 1.5–4 s. Strike reactions interrupt the pauses, never a
  stroke (the fire-cycle rule). Locomotion still wins while the compiled
  track moves.
- **Casualties resolve through the existing sober wound system** —
  the compiled schedule decides who falls when; a melee fall plays the
  standard articulated falls (a grappler falls at his clinch anchor);
  cause/wound classes unchanged; `IsFireAction("melee")` is false, so a
  melee produces no discharges, no smoke, no audio fire events.
- Duration discipline: bouts are clip+rest bounded; segment length is a
  data-wave decision (the wall-breach minutes — Peyton's fight is
  segment-scale brief, not a battle-long brawl).

### Colors succession (`ColorGuard.cs` + `colorParty` bundle field)

- `AngleBundleUnit.colorParty` (int, default 0 = inert — every committed
  bundle parses to 0 and resolves bit-identically). The data wave sets
  it per regiment against Shepard's counts.
- The chain: front-rank files walking outward from the line's center
  (0, +1, −1, +2, −2, …), `ChainLength = min(colorParty, files)`.
- `ColorGuard.StateAt(ctx, ur, t)` walks the compiled casualty schedule
  along the chain — phases `Carried → BearerFalling → Grounded →
  Raising → Carried …` and `Down` when the chain is exhausted (the
  colors stay on the ground; whether a grounded flag is CAPTURED is a
  semantic/data-wave event, deliberately not depicted here).
  `PickupDelay` = 4 s; the raise is the `Colors_Pickup` clip.
- Resolver integration: the acting bearer's stationary fight/ready clips
  are replaced by `Colors_Carry` (locomotion, crossings, reactions and
  the prone/melee sets keep their clips — the staff prop simply travels
  with him); his fall plays `Colors_Bearer_Fall` through the same
  compiled casualty entry (same cause, same wound table, same timing).

### Mounted officer (`MountedOfficer.cs`, `HorseClipId`/`HorseClips`)

- `MountedOfficerSpec { officerId, unitId, fallT, backOffsetM,
  alongOffsetM }` — a C# spec, **not** a bundle field: nothing is wired;
  the data wave owns both the schema landing (a per-unit
  `mountedOfficers` list is the natural shape) and the citations.
- Timeline: rides at his station in the unit's formation frame
  (`Horse_Walk`/`Horse_Stand` by unit ground speed, rider `Ride_Seat`);
  at `fallT` the horse rears once (`Horse_Rear`, anchored at the fall
  point) and the rider leaves the saddle 0.55 s in (`Rider_Fall`),
  lying where he fell to the end of the slice; the riderless horse
  bolts rearward (unit facing + 180° ± 25° hash yaw, 5.5 m/s) and
  leaves the scene at 160 m. Pure; bitwise-repeatable.

### `halt_fire_obstacle` segment action

- Joins `FireCycles.IsFireAction` (fire windows compile for
  engagements/smoke/audio — additive: no committed segment carries the
  action).
- Per slot: staggered `Halt_DressLine` at the segment start (the men
  come to a stop AT the fence), then the standing fire cycle.
  `FireCycles.Offset` adds the same `HaltLeadIn` per slot, so
  **discharge enumeration and the resolver stay in lockstep**
  (`Discharges_MatchTheResolverExactly` proves it).
- `CompileCrossings` deliberately does not scan these segments: **no man
  mounts the fence while the action holds** (Trimble's men stopped at
  the rails); the following `cross_obstacle`/`fall_back` segment owns
  the resume or the recoil.

## 3. What the DATA wave must wire (segment-by-segment recommendations)

Nothing below is wired in this wave. One consolidated recompile, with
ED citations per edit:

1. **P3 melee at the wall** — split the tail of the `breach` segments:
   `csa-garnett` seg `breach` (8580–8700) and `csa-armistead`
   `seg-armistead-breach` (8640–8700) currently resolve ranged fire
   cycles through Peyton's "hand to hand" (charge-intensity §2.1 row 2).
   Recommend `melee` for the final ~60–90 s of each, wired symmetrically
   against the wall regiments (`us-69pa`/`us-71pa` gain matching `melee`
   windows; `us-72pa`'s counterattack per its own dossier record).
   Citations: peyton-or-1863 EC5.4; the 69th/71st PA dossier ECs.
2. **P4 colors** — set `colorParty` on `csa-fry` (the attested chain:
   3–4 bearers down per regiment, or-27-2-shepard) and, if the owner
   wants brigade-level colors on the CSA brigades, on
   `csa-garnett`/`csa-armistead`/`csa-kemper` at regiment-count grain.
   Note Fry is compiled at brigade grain: either wire `colorParty` ≈
   sum-of-regiment chains as one brigade color party (a disclosed
   simplification) or wait for a regiment-grain recast. NO name is ever
   attached to a bearer (V&R §1).
3. **P5 mounted officer** — Garnett: attach a spec to `csa-garnett`,
   fallT inside the final approach (Peyton: "within about 25 paces of
   the stone wall" → the last seconds of `advance`/first of `breach`,
   ~t=8560–8590), backOffset small (he rode the line's front — the
   spec's offsets support negative values for a forward station), citations
   peyton-or-1863 EC1/EC4 + proposed ED-81 (owner must adopt ED-81
   first). Schema: promote `MountedOfficerSpec` to an optional
   `mountedOfficers` array on the bundle unit at that point.
4. **P6 halt-and-fire** — Trimble's command (Lane/Lowrance) is NOT in
   the current 13-unit cast (charge-intensity §4 P6 scope note): either
   (a) the class stays an exercised-in-tests capability until a cast
   expansion, or (b) the owner approves adding the two brigades. If the
   Angle cast's own fence checks (Emmitsburg road fences, 8120–8340)
   are judged to carry halt-and-fire evidence for any unit, the same
   class applies — but Trimble's quote is about HIS command; do not
   transplant it without unit-specific sourcing (the punchlist's
   "do not invent motion" rule).
5. **Iverson reuse** — none of this wave touches the Iverson bundle;
   the melee/halt-fire classes are available to any future corpus.

## 4. Staged render evidence (`docs/benchmarks/captures/angle-v2-vocab/`, force-added)

Staged from a committed generator
(`reconstruction/scripts/stage_angle_v2_vocab.py` → a loudly-labeled
NOT-A-RECONSTRUCTION demo bundle; regenerable byte-identically) **on
the real Angle crop geometry**: the melee lines straddle the traced
stone wall `wall-angle-webb-front` at (4399, 4867); the halt-and-fire
line stands 4.5 m west of the traced
`fence-post-and-rail-west-of-road`; the colors line and the mounted
officer are 40 m west of the wall. Rendered by
`app/Assets/Editor/AngleV2Vocab/AngleV2VocabGateRender.cs`
(FightProneGateRender pattern; the staff/flag prop and the horse+rider
are harness-staged extras posed from the pure resolver classes).

- `seq-wall` — 30 s, 30 fps, 2560×1440 (gitignored frames; report
  committed): the colors line under fire (bearer falls, colors
  grounded, taken up), the mounted officer falling at t=14, the wall
  melee beyond.
- `seq-fence` — 30 s: the advance to the rails, the staggered
  halt-dress, the fire AT the fence, the crossing resuming at t=34.
- `av2-*.png` — committed stills (wall/colors arc, melee close-ups,
  the mounted fall arc, the fence arc).
- `clip-sweep/` — one committed mid-phase frame per NEW clip
  (`sweep_csa_a_*`, `sweep_union_a_*` from
  `ReloadStageDiag.RenderClipSweep`; `sweep_horse_*` from
  `RenderHorseSweep`).
- `angle-v2-vocab-gate-report-{wall,fence}.json` — machine evidence:
  bitwise scrub probes, pixel tolerances (Phase 8 envelope), timing,
  and the colors succession timeline as it played on camera.
- `angle-v2-vocab-demo.bundle.json` + `angle-v2-vocab-media.sha256`.

Measured results: §6.

## 5. Suites (floors in parentheses)

| suite | result |
|---|---|
| tool | **119** (119) |
| pipeline | **66** (66) |
| reconstruction | **159 + 1 skip** (159+1) |
| Unity EditMode | **PENDING** (floor 448+1 of 449; +~40 new tests across MeleeResolverTests, ColorGuardTests, MountedOfficerTests, HaltFireObstacleTests) |
| Unity PlayMode | **PENDING** (floor 20+1 of 21) |

Unity CLI: `-batchmode -runTests -buildTarget OSXUniversal`, worktree
Library, no `-nographics`; Angle + Oak Ridge crops and environment
bakes regenerated in the worktree; dev proxy regenerated for PlayMode.

## 6. Measured results

PENDING — filled from the gate reports after the staged renders.

## 7. Film-safety verdict

- `app/Assets/Battle/Angle/angle.bundle.json` — **byte-identical to
  main**; stagingSeed pin
  `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  holds.
- `app/Assets/Battle/Iverson/iverson.bundle.json` — **byte-identical to
  main** (this wave never compiles it).
- All six phase battle files (`gettysburg-*.json`) — **byte-identical
  to main**.
- Resolver changes are additive: new action classes gated on actions/
  fields no committed bundle carries (`melee`, `halt_fire_obstacle`,
  `colorParty` > 0, `meleeOpponentId`); the Angle EditMode scrub/digest
  suites run against the committed Angle bundle and pass unchanged.
- The kit FBX rebuild (26 → 36 clips) regenerates the same 26 existing
  actions from the same committed authoring code plus the 10 new ones —
  the fight-prone precedent: kit assets are not under the film pin; the
  authored keys of the existing 26 clips are unchanged in clips.py
  (two melee-clip fixes touched only NEW clip functions).

## 8. Residuals (disclosed, not hidden)

1. **Grapple step-in/step-out glides.** A grappler walks up to ~3.5 m
   from his roster slot to the clinch anchor during `StepInDur` (1.5 s)
   playing the grapple loop, and back over `ReturnDur` after the pair
   dissolves playing `Stand_Ready` — a fast glide either side of the
   clinch. Sober bound: pairs form only inside melee segments; the data
   wave should keep opposing melee lines inside ~10 m (the wall gap) so
   the step-in stays short.
2. **A bout may cut to a grapple at the pair's staggered start** (the
   candidate works swing bouts until his pairT0 arrives) — a clip cut,
   not a glide; visible only in the first ~4 s of a melee segment.
3. **The bearer's carry does not have a marching variant**: a bearer in
   a MOVING unit plays the normal gait (the staff prop still travels
   with him). A `Colors_Carry_March` clip is future work if the data
   wave stages colors on the advance (it will: the charge). Flagged as
   the top follow-up for a second vocabulary pass.
4. **Colors pickup position snap**: the successor plays the pickup at
   his own roster position, not at the grounded staff (typically 0.5–2 m
   away — chain slots are adjacent files; the staff prop is staged at
   the STATE's position, so the prop and the man can be up to ~2 m
   apart during a raise). Fix if it reads badly: a position override
   walking the successor to the staff (the crossing-hold pattern).
5. **A successor who falls DURING his 2 s raise** is reported `Raising`
   until the raise window ends before the state re-grounds — a ≤2 s
   bookkeeping overlap, deterministic, invisible at documentary
   distance (his body plays the standard fall via the casualty system
   throughout).
6. **The rider's seat straddle is approximate** on the widest kit
   variants (the legs are IK'd to fixed stirrup points; heavy variants'
   thighs intersect the saddle blanket edge slightly). Acceptable at
   hero distance; a per-variant stirrup calibration is future polish.
7. **Horse walk/bolt are in-place cycles keyed by time,** not
   distance-driven like the human gait (no `MetersPerCycle` contract on
   the horse yet). At the walk speeds the demo stages, foot-skate is
   minor; the data wave should add distance keying if a mounted officer
   rides a long moving track.

## 9. Proposed ED-81 (verbatim; not self-adopted)

See `angle-editorial-decisions.md` § "ED-81 — PROPOSED". Summary: the
owner's 2026-07-15 ruling "ship p5 mounted officers falling" authorizes
an ANONYMOUS mounted-officer-fall vocabulary (no names, no insignia, no
new wound classes, the horse never a casualty); attaching the figure to
Garnett's documented fall is a data-wave act with segment-level
citations under V&R §1. The entry awaits owner adoption.
