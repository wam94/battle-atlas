# ADR 0004 — Character base-mesh source: MakeHuman/MPFB

Status: proposed (executor decision per plan §12 Phase 6; owner may veto at
Gate P6)
Date: 2026-07-09
Plan: `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` §7.1

## Decision

Use **MakeHuman/MPFB core CC0 assets** (MPFB 2.0.16 extension, headless in
the `bpy` 4.5 wheel) as the character base-mesh source for the Phase 6
kit. The Blender Human Base Meshes bundle (CC0) remains the spike-evidence
base and a fallback.

## The comparison (close-view test, plan checklist item)

Both CC0 candidates were driven headlessly through the same Workbench
close-view shots (head ~1 m, torso ~2 m, full body; regenerate with
`characters/spike/bakeoff_mpfb.py` / the spike scripts):

| Criterion | Blender HBM v1.4.1 | MakeHuman/MPFB 2.0.16 |
| --- | --- | --- |
| Close-view face topology | flat-featured at 1 m (sculpt base) | modeled lips/eyelids/nostrils/ears; clearly better at hero distance |
| Rig | **none** (bones, weights, actions all absent) | `game_engine` rig ships in-box: 53 bones **including full fingers**, weighted (204 vgroups) on first call |
| Deterministic body variation (§6.4) | fixed bodies only | parametric macros (gender/age/muscle/weight/height/proportions) — a seeded dict per soldier slot |
| Headless viability | plain .blend, trivially | extension enables and runs in `bpy` wheel batch mode (verified: create, macro targets, rig+weights, pose, render) |
| Mesh cost | 10.6k verts body | 19.2k verts incl. clothes-helper geometry (pruned at export; body-only is comparable) |
| License | CC0 (blender.org demo page) | core assets CC0; MPFB **code** GPL — a build-time tool like Blender itself, never shipped; outputs are asset derivatives of CC0 (evidence archived) |

The plan's own preference rule ("Prefer MakeHuman if its topology, rig,
and deterministic body variation shorten production without compromising
close-view quality") is satisfied on every clause: the in-box weighted
finger rig directly de-risks the reload-cycle hero requirement, and the
macro system is the §6.4 deterministic variation mechanism for free.

## Consequences

- The kit build pipeline (`characters/kit/`) generates soldiers via
  MPFB `HumanService.create_human(macro_detail_dict=...)` +
  `add_builtin_rig(basemesh, "game_engine", import_weights=True)`.
- Helper geometry (clothes/joint helpers) is deleted before export.
- MPFB itself is a pinned, fetch-reproducible build dependency
  (extensions.blender.org, sha256 in the license evidence), not committed
  and not a runtime/build input of the Unity app.
- Community MakeHuman clothes/assets remain NOT approved; garments are
  project-owned scripted models (plan §7.1).
- Evidence: `docs/assets/licenses/makehuman-mpfb-core/`.
