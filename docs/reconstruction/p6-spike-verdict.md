# Phase 6 feasibility spike — go/no-go note

Plan: `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` §12
Phase 6, first checklist item. Executed 2026-07-09 on branch `v2-phase6`.

## Verdict: GO

The mandated spike — one CC0 base human mesh, one garment, one march clip,
rendered at hero distance in Unity HDRP — was produced end-to-end in one
session, entirely headlessly, and every component worked on the first or
second crude pass. Nothing in the toolchain failed or required workarounds
that would compound at kit scale. The named residual risks (below) are
quality-labor risks, not feasibility risks, and the full-kit work is
ordered so the highest one (the reload cycle) resolves first.

## What was produced

| Piece | How | Evidence |
| --- | --- | --- |
| Base mesh | Blender Studio **Human Base Meshes v1.4.1** (CC0), `GEO-body_male_realistic`: 10,582 verts quad topology, unrigged | `characters/spike/fetch_base_meshes.sh`; license: `docs/assets/licenses/blender-human-base-meshes-v1.4.1/` |
| Toolchain | **No Blender install exists on this machine.** The `bpy` 4.5.11 PyPI wheel on Python 3.11 did everything: open bundle, scripted 21-bone armature from measured mesh landmarks, automatic-weight skinning, garment extraction, keyframing, Workbench preview renders, FBX export | `characters/spike/spike_build.py` (deterministic, no random) |
| Garment | Crude sack coat: torso+sleeve face bands duplicated, displaced 12 mm, solidified 6 mm; inherits skin weights; navy wool flat PBR | same script; 6,708 verts / 11,080 tris |
| March clip | Hand-keyed 26-frame 24 fps shoulder-arms march (~110 steps/min): contact/passing/contact leg cycle, pelvis bob, hip/chest counter-rotation, left-arm swing, right arm static at the musket-carry | `March_ShoulderArms` in the FBX |
| Arm posing method | Temporary IK constraint against wrist targets, visual-transform apply, read back local eulers (`characters/spike/ik_solve.py`) — precise hand placement without guessing rotations | solved carry pose renders correctly |
| Hero render | Unity 6000.4.11f1, **offline HDRP profile** (PBS sky, 95 klux sun, EV100 13.2, ACES), 2560×1440, headless batchmode via the Phase 4 render harness pattern | `docs/benchmarks/captures/p6-spike-{5,8,15}m-p{00,25,50,75}.png` + `p6-spike-az*.png` (gitignored; regenerate with `SpikeHeroRender.RenderSpike`) |

Imported figure cost: body 16,105 verts / 23,336 tris (Unity-split), coat
11,080 tris, 21 bones — comfortably inside the §7.4 hero-tier budget.

## The honest §7.3 / reload-cycle judgment

**Can this toolchain author the historically ordered multi-stage
muzzle-loading reload at hero quality?** Judgment: **yes, with the labor
concentrated exactly where the plan predicted.** Grounds:

1. The reload is a sequence of *reach poses* (hand to muzzle, hand to
   cartridge box, tear cartridge, charge barrel, draw/return ramrod, ram,
   half-cock, cap from cap pouch, shoulder) connected by timing. The spike
   proved a repeatable primitive for exactly this: IK-solve a hand target,
   apply, read back rotations, key. That is how the carry pose in every
   rendered frame was authored, and it took minutes, not hours.
2. Sampling in Unity is deterministic (`clip.SampleAnimation` at fixed
   phases produced the rendered matrix), so reload phase can be driven
   from the battle clock per §7.5.
3. The march clip — a harder *rhythm* problem than the reload's
   pose-to-pose problem — already reads as walking at 5/8/15 m on a first
   crude pass.

**Residual risks, named (quality-labor, not feasibility):**

- **Hands.** The spike rig has no finger bones; hands read as mittens.
  A legible reload at 5 m needs at least thumb + index/finger-mass bones
  (3–4 extra bones per hand) and per-stage hand poses, or the reload will
  read as pawing. Scripted-rig extension is routine with the same
  landmark method; it is scheduled inside the reload task, first.
- **Garment quality.** The shrinkwrap coat proves the garment *pipeline*
  (extract → skin → deform) but reads skin-tight at 5 m — a sweater, not
  a sack coat. The full kit must model real looseness: flared skirt,
  collar, button placket, cuffs. This is the largest pure-labor item; the
  fallback if hero-tier garments stall is accepting near-tier garment
  quality at hero distance, NOT the Quaternius fallback (the base mesh
  and animation path are proven).
- **Musket interaction.** The reload needs the musket as a bone-parented
  prop with grip swaps (shoulder → butt-on-ground → ram). Prop parenting
  and constraint swapping are standard bpy scripting; untested in the
  spike but uses the same primitives.

None of these rise to MARGINAL: no component of the mandated spike failed,
and each risk has a scripted-method mitigation already demonstrated in
kind. The documented fallback (lower-fidelity rigged set, e.g. Quaternius)
is NOT invoked.

## Fallback trigger (kept explicit)

If, during full-kit work, the reload cycle after finger-rig and musket-prop
work still reads as a generic modern-rifle reload to a reviewer at 5–8 m,
stop and surface to the owner with the render evidence — that is the §7.3
release-blocker line, and the owner decides between more animation labor
and the reduced-fidelity fallback.

## Toolchain facts for the record

- `bpy==4.5.11` wheel requires Python 3.11 (system 3.11.4 matched; venv in
  session scratchpad, recreate per `characters/spike/README.md`).
- Blender bundle .blend (62 MB) exceeds the ADR 0002 10 MB per-file limit:
  fetch-reproducible with pinned sha256, not committed.
- Workbench headless rendering works for fast previews (~0.2 s/frame);
  material viewport colors must be set separately from node base colors.
- `bpy.ops.object.parent_set(type='ARMATURE_AUTO')` (automatic weights)
  works headlessly; produced clean deformation on the 10.5k base at hero
  distance with zero manual weight painting.
- Blender FBX export defaults import into Unity at correct scale and
  orientation; the animation arrives as a Generic-rig clip sampled fine.
