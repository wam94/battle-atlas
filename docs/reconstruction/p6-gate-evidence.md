# Gate P6 evidence — viewing guide

Plan: `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md`
§12 Phase 6 gate. The gate closes on OWNER review; this note tells the
reviewer what was rendered, where it lives, what each checklist property
looks like on screen, and how to regenerate every byte.

## What to watch

**`p6-gate-60s-1440p.mp4`** — 60 seconds, 30 fps, 2560×1440, eye level
(1.66 m, 68° fov), 100 kit soldiers on the true-scale Angle terrain
under the offline HDRP profile and the 15:20 LMT ephemeris sun.
A 720p proxy sits beside it. Media and stills are **generated artifacts
(gitignored)**; they are staged for review in TWO places:

1. worktree output: `docs/benchmarks/captures/p6-gate/` (this checkout)
2. owner copy: `docs/benchmarks/captures/p6-gate/` under
   `/Users/wmitchell/Documents/jetsons/warface` (the main checkout, same
   gitignored path — reachable without touching the worktree)

## Checklist properties, and where to look

| Gate property | Where it appears |
| --- | --- |
| **Credible formation** | From 0:00 a two-rank 64-man Confederate line (plus a trailing echelon entering ~0:20 from the left) advances east at quick time, in step, muskets at the shoulder-arms carry. It should read as a drilled unit, not independent NPCs. |
| **Legible reload cycle** | The skirmisher ~11 m ahead-right of camera fires at 0:07 and reloads 0:08–0:28, again 0:31–0:51: butt to the ground, cartridge from the box on his right hip, tear with teeth, charge at the muzzle, RAMROD drawn high / rammed twice / returned, prime at the lock from the cap pouch, shoulder. The Union line at the wall (background) runs the same cycle staggered, so several stages show simultaneously. |
| **Deterministic action variation** | Three body types + hat/coat/equipment differences per faction (6 variants assigned by slot); per-slot step phase and fence-crossing stagger from the shared FNV hash — identical on every run. |
| **Fence crossing** | 0:15–0:22: the line halts at the rail fence and climbs over file by file, staggered (left hand on the top rail, right leg over, 1.3 m of clip root motion). Nearest crossings ~25 m ahead of camera. |
| **Explicit falls** | 15 men fall between 0:22 and 0:57, clip chosen by incoming direction: frontal musketry throws men backward or crumples them forward; oblique canister knocks two men sideways. Nonfatal hits stagger 5 more (they clutch the wound, recover, march on hunched). |
| **Persistent bodies** | Every fallen man stays where he fell to the end (verify by scrubbing: bodies accumulate east of the fence and never move or vanish). Dropped muskets lie beside them. |
| **Deterministic under scrub** | `p6-gate-report.json` records probe frames 300/900/1500 re-rendered OUT OF ORDER after the sequence and pixel-compared against the sequence frames. Logical state is bit-deterministic (pure choreography + clip sampling; proven by the EditMode scrub-invariance tests). The IMAGE comparison uses a documented GPU tolerance — HDRP raster on Metal shows ~1% of pixels at 1–4/255 shading noise (sky ambient-probe convergence, measured by `GateP6Render.ProbeDiag`); the probes must stay under max channel delta 8 and 5% differing pixels, and the render aborts if they don't. No Animator playback, no `_Time` shaders, no unseeded Random. |

Also staged: the advance stalls short of the wall (~0:45+) and wavers
under fire — the Gate P8 casualty/segment machinery will replace this
staging with compiled reconstruction data.

## Honest limitations (already known; judge the gate, not these)

- Garments are the near-tier-quality scripted drape (skin-tight in
  places), primitive-boxy equipment, flat colors — Phase 7 materials and
  garment looseness work is not in scope for this gate.
- No smoke/muzzle VFX, no audio: Phase 8/9 scope.
- Hands grip approximately; fingers curl but do not articulate
  per-stage beyond grip amounts.
- The staged fence/wall/trees are the Phase 3 placeholder statics, not
  Phase 7 sourced geometry.

## Regeneration (all deterministic)

```sh
# 1. toolchain + kit (headless bpy; ~10 min)
characters/kit/setup_toolchain.sh /tmp/.venv-bpy
cd characters/kit && KIT_OUT=/tmp/kit-out /tmp/.venv-bpy/bin/python build_kit.py
cp /tmp/kit-out/*.fbx app/Assets/ProjectOwned/Characters/Kit/

# 2. terrain crop input (gitignored)
cd pipeline && uv run python -m terrain_pipeline.cli crop

# 3. render (Unity closed on this checkout; ~40 min on M4)
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.GateP6Render.RenderSequence \
  -logFile p6render.log

# 4. encode (pinned; system ffmpeg or imageio-ffmpeg fallback)
scripts/p6-encode.sh
```

Stills for a quick look without the render:
`-executeMethod BattleAtlas.EditorTools.GateP6Render.RenderStills`.

## Suite state at evidence time

pipeline 48 · tool 108 · reconstruction 79 · Unity EditMode 180
(165 pre-phase + 15 new Gate P6) · PlayMode 10. Commands unchanged from
the Phase 0 baseline doc.
