# ADR 0002 — Git LFS versus reproducible fetch, per asset class

**Date:** 2026-07-08
**Status:** Proposed (owner decides; nothing in Phase 2 enables LFS or restructures the repo)
**Plan of record:** `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md` §12 Phase 2

## Context

The V2 plan brings binary assets into the project: third-party PBR
materials and models (license-manifested under `app/Assets/ThirdParty`,
see ADR 0001 decision 12 and plan §11), project-owned authored assets,
gigabyte-scale offline render scratch, and encoded Soldier View media.
Each class needs an explicit storage policy before the asset volume grows.

**Checked fact:** this repository currently has **no git remote**
(`git remote -v` is empty as of commit `702ad39`). Git LFS requires an
LFS server (normally the hosting remote); with no remote, LFS-tracked
files would live only in the local `.git/lfs` store and add tooling
friction (`git lfs install` before clone/checkout) without providing any
of its benefits today. A future GitHub remote would provide one, with
quota limits (1 GB storage/bandwidth on the free tier).

The manifest validator (`reconstruction/scripts/validate_assets.py`)
records a sha256 for every third-party download and for the imported
content, which makes *reproducible fetch* safe: a script can re-download
from the recorded URL and verify the recorded checksum, and the test
suite fails on any drift.

## Policy per asset class

| Asset class | Examples | Policy |
| --- | --- | --- |
| Third-party imported assets (small) | the Phase 2 process-test assets: 4×1k JPG maps (~1.7 MB), a 76-vertex OBJ | **Plain git objects.** Committed while each file is < 5 MB and the `ThirdParty` aggregate is < ~100 MB. They are required build inputs; a clean checkout must not need network or LFS tooling. |
| Third-party source assets (large) | 8k texture sets, high-poly scan sources for retopology (Phases 6–7) | **Reproducible fetch, not committed.** Keep only the derived/optimized asset in git; record source URL + sha256 in the manifest/evidence so the original is re-fetchable and verifiable. If a large *derived* asset is itself required and cannot be regenerated, that is the LFS trigger below. |
| Project-owned authored assets | clothing meshes, animations, Blender sources | Plain git while small; working sources that exceed ~10 MB/file (e.g. .blend with packed textures, audio stems) wait for the LFS decision or live in release-source storage per plan §9.3. |
| Cached pipeline inputs | 1 m DEM tiles, land cover | **Reproducible fetch** (already the pattern: `data/*` is gitignored and rebuilt by the pipeline). |
| Render scratch | PNG/EXR sequences, 160 GB–1 TB (plan §3.5) | **Never in git, never in LFS.** Gitignored local scratch (`app/RenderOutput/`). |
| Final encoded media + proxies | 1440p/720p H.264 Soldier View | **Release artifacts** with checksums (GitHub Releases or equivalent), per locked decision 14 and plan §10. Not git, not LFS. |

## Recommendation

Do **not** adopt Git LFS now: there is no remote for it to serve from,
every heavy asset class is already either reproducibly fetchable
(checksum-verified by the validator) or a release artifact by locked
decision, and the committed binaries are small process fixtures. Revisit
— as a one-line amendment to this ADR — when a hosting remote exists
**and** a required, non-regenerable build input exceeds ~10 MB per file
or `ThirdParty` exceeds ~100 MB aggregate; at that point adopt LFS for
that class only, never for render scratch or release media.

## Consequences

- The two Phase 2 test assets are committed as normal git objects
  (explicit choice per the plan's media/binaries policy).
- A clean `git clone` (once a remote exists) builds with no LFS tooling
  and no network fetch of required inputs.
- Asset growth is bounded by the validator + this ADR's thresholds rather
  than by ad-hoc judgment; breaching a threshold forces a recorded
  decision instead of a silent repo-size regression.
