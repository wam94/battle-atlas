# Fight-prone vocabulary (Iverson production-render blocker)

**Status:** executor evidence for the `fight-prone-vocab` slice.
**Why:** the Iverson design gate (`iverson-viewpoint-design.md` §4.3)
named the lying-down fight the film's **T5 vocabulary gap #1** — "my
line of battle still lying down in position" (`or-27-2-iverson`,
claim-iv-lying-down) — and §8.4 recommended it BLOCK the production
render. This slice closes it: clips, resolver action class, Iverson
wiring, and rendered evidence.
**Branch:** `fight-prone-vocab`. Film-safety: the Angle bundle, its
stagingSeed pin `d470c469…`, and all six phase battle files are
byte-untouched (§6).

---

## 1. Clip inventory (6 new clips; kit total 20 → 26)

Authored in `characters/kit/clips.py` on the shared prone-ready base
pose (so the resolver's cycle switches read as one man), rebuilt into
all six kit variants (`build_kit.py`, headless bpy 4.5.11 + MPFB per
ADR 0004). The P6 fix-round conventions carry: articulated transitions
(no rigid flips), `_drop_musket` for slipped weapons, deterministic
authoring (no random anywhere).

| ClipId | FBX action | dur | loop | what it depicts |
|---|---|---|---|---|
| GoProne (20) | `Go_Prone` | 1.8 s | no | drop from standing: crouch, left hand to the ground, knees, onto the belly, piece brought forward |
| ProneIdle (21) | `Prone_Idle` | 3.0 s | yes | at the ready between cycles: chest propped on the elbows, head up watching, breathing |
| FightProneFire (22) | `Fight_Prone_Fire` | 2.4 s | no | level the piece from the ground, cheek to stock, **discharge at t=1.5** (`FireCycles.ProneMuzzleDelay`), recover |
| FightProneReload (23) | `Fight_Prone_Reload` | 26 s | no | **the roll-to-load compromise**: half-roll onto the left side, butt to the ground at the hip, muzzle raised; cartridge → tear → charge → rammer (two short strokes) → prime → roll back. Nine stages (`PRONE_RELOAD_STAGES`) |
| RiseFromProne (24) | `Rise_From_Prone` | 2.2 s | no | push up over the left arm, right knee under, rise to the standing carry |
| ProneHitSettle (25) | `Prone_Hit_Settle` | 1.8 s | no | a man hit while lying: a jolt, the grip slackens, the piece slips beside him, a half-roll, still. A settle, not a thrash |

### Historical grounding

- **The prone fight itself is documented at this site**: Iverson,
  `or-27-2-iverson` No. 513 — "a most desperate fight took place" with
  "my line of battle still lying down in position"
  (claim-iv-lying-down, assessment **documented**; corroborated from
  the receiving side by Baxter's "repeated lines repulsed",
  claim-iv-fight). The men fought from where they lay in the swale.
- **Loading prone is the attested awkward-and-slow compromise.** The
  period skirmisher drill (the school of the skirmisher in the
  Scott/Hardee-lineage US infantry tactics, in Confederate use) taught
  loading while lying by rolling onto the side/back to bring the piece
  within reach of the cartridge box and keep the muzzle clear — the
  depiction (half-roll left, butt grounded at the hip, muzzle raised)
  follows that drill shape rather than inventing motion. Its cost is
  kept visible: the prone reload runs 26 s against the standing
  drill's 20 s, so the full prone cycle is **~1.9 rounds/min vs ~2.5
  standing** (`FireCycles.ProneCycle` = 2.4 + 26 + 3 ≈ 31.4 s). This
  is a drill-precedent inference (clip-level), not a site-documented
  choreography; the *fact* of the lying fight is what
  claim-iv-lying-down documents.
- **Sober representation** (`violence-and-representation.md`): prone
  fighting is depicted as defensive drill behavior; the one new
  casualty-adjacent clip (`Prone_Hit_Settle`) exists precisely so a
  man hit while lying does NOT stand up into a canned fall (the P6
  mannequin-fall defect class). No new wound vocabulary, no gore; the
  wound-category table is unchanged.

## 2. Resolver mapping (`SoldierActionResolver`, pure/deterministic)

New segment action class **`fight_prone`**, per slot (all draws from
the shared FNV hash; scrub-invariant):

```
seg.t0 + stagger(0..4 s)                    StandReady (waiting his turn)
  -> Go_Prone (1.8 s)                       the line drops file by file
  -> prone cycle (offset hash 0..31.4 s):
       Fight_Prone_Fire (2.4 s, discharge at 1.5)
       Fight_Prone_Reload (26 s)
       Prone_Idle (3 s ready pause)
  [casualty while prone -> Prone_Hit_Settle; body persists where it lay;
   the wounded-crawl hash subset drags on as before]
  [next segment != fight_prone -> staggered (0..2 s) Rise_From_Prone
   into the new segment's behavior]
```

- **Formation layout unchanged**: prone men hold their file positions
  (`BasePosition` untouched; tested). The prone body extends ~1.9 m
  toward the enemy from the slot point — a man drops forward where he
  stood; the line's geometry (claim-iv-death-line's straight line) is
  exactly preserved.
- Strike reactions do not interrupt a prone man (he is already low);
  locomotion still wins if the compiled track moves (defensive).
- Deaths: `ProneAt()` decides — hit after his Go_Prone finished →
  `Prone_Hit_Settle`; hit standing (drop stagger or after his rise) →
  the standing by-incoming-direction falls.
- VFX/audio: `FireCycles.SegmentDischargeTimes` unifies discharge
  enumeration (standing vs prone cycles) for the smoke compiler and
  both audio-event exporters; a prone discharge's smoke/flash births
  at **0.35 m** (`ProneMuzzleHeightM`), 1.6 m ahead of the slot.
- Crowd tiers: hero/near sample the real clips; mid tier gains a baked
  **`pose_prone_fire`** (a prone firing line at 100 m+ reads LOW);
  far impostors render prone fighters as ground quads, never standing
  cross-quads.

EditMode coverage: `FightProneResolverTests` (12 tests — vocabulary
constants, Iverson wiring, full-cycle clip sweep, file-position hold,
prone-death settle + persistence, discharge/resolver agreement, the
12th NC standing, bitwise repeatability, clip-time bounds, staggered
drop, rise transition on a synthetic hold→fight_prone→hold bundle, mid
poses) plus the `CrowdTiersTests` bake-coverage update.

## 3. Iverson wiring (`compile_iverson.py`)

Declarative rule at the compiler: a fire segment carrying
**claim-iv-lying-down** compiles to `fight_prone`
(`compile_iverson.bundle_action`; tested in Python and C#). The
canonical corpus keeps its semantic fire actions + claim tags.

| segment | corpus action | compiled action |
|---|---|---|
| seg-5nc-fight (6000–7050) | fire_independent | **fight_prone** |
| seg-20nc-fight (6000–7050) | fire_independent | **fight_prone** |
| seg-23nc-fight (6000–7050) | fire_independent | **fight_prone** |
| seg-12nc-fight (6000–7050) | fire_independent | fire_independent |

The 12th NC — the survivor regiment the record keeps standing on the
sheltered right — does not carry the claim and keeps the standing
fight; the observer (slot 184, 12th NC rear rank) therefore watches
the left three regiments fight and die **lying down, in line**, from
a standing line, which is what the record describes.

Bundle regenerated deterministically (`iverson.bundle.json`, checksum
`2f15dd2f4e5e…`; stagingSeed pin `iverson-proof-seed/1` held —
victim draws and all existing choreography hashes unchanged; the
prone-cycle draws are new keys). `iverson-bundle-audit.md` gains the
wiring table; claim-iv-lying-down's interpretation note now records
the gap as closed. The reconstruction suite carries a new wiring test
(`test_lying_down_claim_compiles_to_fight_prone`).

## 4. Render evidence (`docs/benchmarks/captures/fight-prone/`)

P6-style staged gate render on the Oak Ridge crop at the real
Iverson–Baxter fight geometry (the t=6600 positions, 98 m range),
staged from a committed generator
(`reconstruction/scripts/stage_fight_prone.py` → a loudly-labeled
NOT-A-RECONSTRUCTION demo bundle; regenerable byte-identically):

- `fight-prone-30s-1440p.mp4` (+720p proxy; gitignored, regenerable) —
  30 s, 30 fps, 2560×1440, fixed eye-level flank camera: the line
  stands under fire, goes prone file by file, fights prone (fire /
  roll-to-load / ready), takes prone casualties that settle where they
  lay, and rises.
- `fp-still-*.png` — the arc in six committed stills (standing under
  fire, going prone, prone fire, roll-to-load, rising, recovered).
  The late stills (19 s+) are partly veiled by the line's own
  accumulated ground-level powder bank — honest physics of what is
  being depicted; the video carries the roll-to-load and the rise in
  motion, and the t=8/13 stills carry the clean silhouettes.
- `iv-660*-prone-fight-{fp,c3p}-{before,after}.png` — the Iverson
  window at t≈6600 from the film's own observer (slot 184, FP + C3P):
  BEFORE (fire_independent, the fight standing — the disclosed gap)
  vs AFTER (the left three regiments fighting prone). The observer's
  own 12th NC stands in both — correct: it does not carry the claim.
- `iv-6600-23nc-line-{before,after}.png` — the legible pair: a
  documentary flank still down the 23rd NC's line (presentation-only
  camera; staged states untouched). Before: the line stands. After:
  the line is down and firing from the ground — inside the dense
  ground-level powder bank its own prone muzzles build (the §10
  smoke-ceiling owner-tunable from the design gate applies with
  extra force at a 0.35 m muzzle height).
- Clip sweep: `docs/benchmarks/captures/p6-gate/clip-sweep/
  sweep_csa_a_{Go_Prone,Prone_Idle,Fight_Prone_Fire,
  Fight_Prone_Reload,Rise_From_Prone,Prone_Hit_Settle}_*.png` — one
  mid-phase frame per new clip committed (full 26-clip × 4-phase sweep
  regenerable via `ReloadStageDiag.RenderClipSweep`); no skin
  poke-through (the P6 defect-1 mask carries over — garment coverage
  is body-face deletion, pose-independent).
- `fight-prone-gate-report.json` — machine evidence: bitwise scrub
  probes + pixel tolerances (Phase 8 envelope), timing.
- `fight-prone-demo.bundle.json` + `fight-prone-media.sha256`.

Silhouette/clip-sweep checks per the P6 lessons: Blender workbench
review frames for all six clips (iterated four rounds: ground-plane
contact, reachable leg IK with low knee poles, chest prop, shouldered
aim), and the in-engine stills above (skinned per-render —
`forceMatrixRecalculationPerRender` is inherited from the P6 fix).

Measured results: §7.

## 5. Suites (floors in parentheses)

| suite | result |
|---|---|
| tool | **119** (119) |
| pipeline | **66** (66) |
| reconstruction | **159 + 1 skip** (158+1; +1 new wiring test) |
| Unity EditMode | **448 + 1 skip of 449** (floor 436+1 of 437; +12 new tests; the 1 skip is the terrain-material self-skip, and the Angle-bake-conditional environment tests RAN — the bake was regenerated in the worktree) |
| Unity PlayMode | **20 + 1 skip of 21** (floor; the skip is the production-media-conditional seek test) |

Unity CLI: `-batchmode -runTests -buildTarget OSXUniversal`, worktree
Library, no `-nographics`; Angle + Oak Ridge crops and environment
bakes regenerated in the worktree; dev proxy regenerated for PlayMode.

## 6. Film-safety verdict

**Base note:** origin/main advanced mid-slice (the bombardment-prelude
merge, `7363561` — battle JSONs/audits/tool script only; zero file
overlap with this slice). `origin/main` was merged into this branch
(`d93d959`) and every suite re-run on the merged base; the byte checks
below are against the CURRENT origin/main.

- `app/Assets/Battle/Angle/angle.bundle.json` — **byte-identical to
  main** (git diff empty); stagingSeed pin
  `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  holds; `test_committed_bundle_matches_corpus` passes.
- All six phase battle files (`gettysburg-*.json`) — **byte-identical
  to main** (this slice adds no battle-file edits).
- The Angle corpus inputs — untouched; only the July 1 corpus changed
  (claim note) and only the **Iverson bundle** recompiled, as
  authorized; its audit doc is regenerated in lockstep.
- Resolver changes are additive: new action class + new ClipIds; every
  existing action path resolves bit-identically (the Angle EditMode
  scrub/digest suites run against the committed Angle bundle and
  pass unchanged).

## 7. Measured results (Apple M4 24 GB, Unity 6000.4.11f1, offline HDRP profile)

From `fight-prone-gate-report.json` + the encode log:

- **Demo staging:** 280 slots, bundle `10ddc15c34d1…`, seed
  `fight-prone-demo-seed/1`.
- **Sequence:** 900 frames at 2560×1440, **0.40 s/frame**.
- **Scrub probes (frames 150/450, re-posed out of order):** logical
  280-slot state **bitwise identical 2/2**; re-rendered pixels
  1.64% / 0.89% differing at max channel delta 7 / 8 — well inside the
  documented Phase 8 envelope (12 / 8%).
- **Media:** `fight-prone-30s-1440p.mp4`
  `58c235b09806e7e89e78459f9def24d57e84f723b225c512fd6bd1ef16511c5f`
  (+720p proxy; libx264 slow CRF 18, GOP 30 — the media-contract
  settings; frame-count-verified before encode; hashes in
  `fight-prone-media.sha256`).
- **Iverson before/after stills:** staged from the real bundle
  (3,589 slots) — BEFORE from the pre-slice bundle
  (`1e2e802f4eca…`, fight standing), AFTER from the regenerated
  bundle (`2f15dd2f4e5e…`, left three regiments prone); same
  viewpoint/observer machinery as the design-gate proofs, plus a
  documentary flank still of the 23rd NC's line in both states.
- **Blender kit rebuild:** all 6 variants × 26 clips, headless bpy
  4.5.11 + MPFB 2.0.16 (`setup_toolchain.sh` unchanged); previews
  iterated four rounds against a ground plane before the FBX bake.

## 8. Residuals (disclosed, not hidden)

1. **Rising into a moving segment glides.** The staggered rise plays
   at the START of the following segment; if that segment's track
   moves immediately, a rising man can glide up to ~2 m before his
   locomotion clip takes him. The Iverson slice never exercises this
   (the prone segments end at slice end); the demo rises into a hold.
   Fix if a future corpus needs it: hold the rise men against the
   track like the crossing hold.
2. **Prone reload stage boundaries are not per-stage audited** the way
   the standing reload's were at P6 (drill-manual stage-by-stage
   review); the staging follows the drill shape but a dedicated
   reload-stage diagnostic render (the `ReloadStageDiag` pattern) is
   future hardening.
3. **`Prone_Wounded_Crawl` (P6) sinks ~0.2–0.3 m into the ground** in
   its authored root height (pre-existing; discovered while authoring
   these clips against a ground plane). The new prone clips sit ON the
   ground; re-rooting the crawl to match is a small follow-up to the
   P6 kit, not done here to keep the diff inside this slice's scope.
4. **12th NC posture**: the record pins the lying line to the
   destroyed left three regiments; if the owner reads "my line of
   battle" as the whole brigade, tagging seg-12nc-fight with
   claim-iv-lying-down in the corpus is a one-line change and the
   compiler will do the rest.
5. The **surrender vocabulary** (T5 gap #2) remains open — the slice
   window still ends at 14:57 by design.
