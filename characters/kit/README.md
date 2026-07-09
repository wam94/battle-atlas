# Phase 6 character kit

Builds the project-owned soldier kit — 6 deterministic body/uniform/
equipment variants (3 Union, 3 Confederate) on MPFB `game_engine`-rigged
bodies (ADR 0004), fully clothed and equipped by script, with the §7.3
animation vocabulary (20 clips) authored per rig, exported as Unity FBX
in three LOD tiers. Everything headless `bpy`; no Blender application,
no random anywhere.

## Setup (reproducible)

```sh
characters/kit/setup_toolchain.sh /path/to/.venv-bpy
```

Installs the `bpy==4.5.11` wheel (Python 3.11) and the pinned MPFB
2.0.16 extension (CC0 core assets; GPL build-time code, never shipped —
see `docs/assets/licenses/makehuman-mpfb-core/`).

## Build

```sh
cd characters/kit
KIT_OUT=/tmp/kit-out /path/to/.venv-bpy/bin/python build_kit.py \
    [--only union_a] [--previews-only] [--clip-previews]
```

Outputs per variant: `<name>.blend` (source), `<name>.fbx` (hero tier,
all 20 clips), `<name>_near.fbx` (decimated, same skeleton — the hero
clips sample onto it in Unity), and per faction `<name>_mid.fbx`
(baked static pose meshes for RenderMeshInstanced crowds, Phase 8).
Workbench preview sheets land beside them. Committed copies live at
`app/Assets/ProjectOwned/Characters/Kit/`.

## Files

- `setup_toolchain.sh` — venv + MPFB, pinned and checksummed.
- `soldier_factory.py` — MPFB body creation (macros are SHAPE KEYS —
  baked before any measurement), `game_engine` rig, helper pruning.
- `garments.py` — scripted garments/equipment with measured
  character-right (`lm.rs`) so accoutrements side correctly.
- `musket.py` — project-owned Springfield 1861 from PD dimensions;
  separate ramrod on its own prop bone for the reload.
- `clips.py` — the §7.3 animation vocabulary; see its docstring for the
  pose-compiler method and the 9-stage reload timing table
  (`RELOAD_STAGES`, mirrored by `GateP6Choreography` in Unity).
- `build_kit.py` — orchestrator, variants table, previews, LOD exports.

## Known bpy pitfalls (cost this project real time)

- `pose_bone.matrix` READS are depsgraph-cached: compose transforms in
  Python and assign once, or interleave `view_layer.update()`.
- MPFB rigs ship locked transform channels on some bones; clear them.
- Actions need `use_fake_user` to survive .blend save/reload; purge all
  actions between variants or `bake_anim_use_all_actions` leaks clips
  across factions at export.
- The bpy wheel can segfault at interpreter EXIT (after successful
  completion); check for the `[kit] DONE` marker, not the exit code.
