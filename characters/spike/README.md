# Phase 6 feasibility spike — Blender sources

Plan: `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` §12
Phase 6, first checklist item. Verdict note:
`docs/reconstruction/p6-spike-verdict.md`.

These scripts build `app/Assets/ProjectOwned/Characters/Spike/SpikeSoldier.fbx`
from the CC0 Blender Human Base Meshes bundle, **entirely headlessly** (no
Blender application install; the `bpy` PyPI wheel).

## Toolchain

- Python 3.11 (the `bpy` 4.5 wheel requires 3.11)
- `python3.11 -m venv .venv && .venv/bin/pip install bpy==4.5.11`
- No GUI, no Blender.app: everything runs through `bpy` module scripts.

## Regenerate

```sh
./fetch_base_meshes.sh downloads          # pinned URL + sha256 (62 MB blend, not committed — ADR 0002)
.venv/bin/python spike_build.py           # -> spike_soldier.{blend,fbx} + Workbench previews
```

Note: the committed scripts use absolute scratchpad paths from the spike
session (they are evidence of method, frozen as run). Adjust `SP`/`BUNDLE`
at the top of each script to rerun elsewhere. The full-kit pipeline (post
spike verdict) will parameterize paths properly.

## Files

- `fetch_base_meshes.sh` — pinned acquisition of the CC0 bundle.
- `analyze_body.py` — extracts skeleton landmark statistics from the base
  mesh (bbox, arm/leg/torso slice centroids) used to place bones.
- `spike_build.py` — the full build: extract `GEO-body_male_realistic`,
  join eyes, drop multires, build a 21-bone Unity-Humanoid-compatible
  armature from measured landmarks, automatic-weight skinning, extract a
  crude shrinkwrap sack coat (face-band duplicate + displace + solidify),
  flat PBR materials (skin / navy wool / sky-blue wool / brogan leather),
  hand-key a 26-frame 24 fps shoulder-arms march cycle, render Workbench
  previews, export FBX.
- `ik_solve.py` — pose utility: solves arm poses with a temporary IK
  constraint against wrist targets (right hand at the shoulder for the
  musket carry, left wrist at the thigh for the natural hang), applies the
  visual transform, and prints the local eulers that `spike_build.py`
  hard-codes.

## License

Base mesh: CC0-1.0 (Blender Studio Human Base Meshes v1.4.1); evidence at
`docs/assets/licenses/blender-human-base-meshes-v1.4.1/acquisition.json`.
Everything added by these scripts (rig, garment, animation, materials) is
project-owned.
