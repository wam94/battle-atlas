# TextMesh Pro Essential Resources — Unity Companion License carve-out

**Status:** resolved — redistribution in this repository is permitted
under the Unity Companion License; the content is carved out of the
repository's MIT license with in-tree notices. Reviewed at Phase 12
(the P11 gate evidence flagged it for this review).

## What is committed and why

`app/Assets/TextMesh Pro/` (Essential Resources: TMP Settings, default
font asset, shaders, style sheet; plus the LiberationSans font) was
committed at Phase 11 because the world-space unit labels
(`UnitLabelField`, TextMeshPro) throw NullReferenceExceptions in any
standalone player without TMP Settings present — surfaced by the P11
standalone screenshot run. It is a required build input, so per the
plan's open-source rule (§2, §11) its redistribution terms had to be
verified rather than assumed.

## License analysis (verified against the license text, not guessed)

The resources are distributed by Unity inside `com.unity.ugui` 2.0.0
(Unity 6's home for TextMesh Pro), whose `LICENSE.md` licenses them
"under the Unity Companion License for Unity-dependent projects".
Archived evidence in `docs/assets/licenses/unity-tmp-essentials/`:

- `com.unity.ugui-2.0.0-LICENSE.md` — the notice shipped in the exact
  package this project resolves (copied from the package cache).
- `unity-companion-license-v1.2.md` — the full Unity Companion License
  v1.2 text (Unity's own license pages block automated archival; this
  copy is the verbatim text as republished with Unity's XR plugin
  headers, sha256
  `899de0f883ff19e3b10b4a9d53acc99462e339d61795ff8851e8e4519601ba2d`).
  Canonical URL: <https://unity.com/legal/licenses/unity-companion-license>.

The findings that matter for an MIT repository:

1. **Redistribution is expressly granted.** The UCL grant is a
   "worldwide, non-exclusive, no-charge, and royalty-free copyright
   license to reproduce, prepare derivative works of, publicly display,
   publicly perform, and distribute the software".
2. **The grant is conditioned on Unity-dependent use** (§1 "Unity
   Companion Use Only"): exercise is limited to "the creation, use,
   and/or distribution of applications, software, or other content
   pursuant to a valid Unity content authoring and rendering engine
   software license". This repository is a Unity project; distributing
   the resources as part of it is inside the grant. But the terms are
   NOT MIT-compatible in the sublicensing sense — a recipient of this
   repository receives the TMP content under the UCL, not MIT, and may
   not lift it out for non-Unity use.
3. **Notice requirement** (§5): the license and copyright notice must
   accompany all substantial portions. Satisfied by
   `app/Assets/TextMesh Pro/LICENSE.md` (committed with the content).

Therefore: **keep the dependency, carve it out**. Replacing TMP was
evaluated and rejected — the world-space label system is TMP-based, the
resources are required for standalone players, and the UCL permits
exactly this use; a font-rendering rewrite to dodge a permitted license
would be pure churn.

## The carve-out

- Repository `LICENSE` (MIT) covers project-owned code and content.
- `app/Assets/TextMesh Pro/**` — Unity Companion License v1.2
  (© Unity Technologies ApS). Notice: `app/Assets/TextMesh Pro/LICENSE.md`.
- `app/Assets/TextMesh Pro/Fonts/LiberationSans.ttf` — SIL Open Font
  License 1.1 (© Google, Red Hat; Reserved Font Name Liberation). Full
  license text committed beside the font
  (`Fonts/LiberationSans - OFL.txt`); the OFL permits bundling and
  redistribution with software.
- `app/Assets/ThirdParty/**` — per-asset CC0/CC-BY, inventoried in
  `app/Assets/ThirdParty/manifest.json` (validated in CI/tests) with
  generated attribution in `docs/assets/THIRD_PARTY_ASSETS.md`.

The README licensing note carries the same statement. The ThirdParty
manifest itself remains CC0/CC-BY-only by design (its validator rejects
anything else); the TMP carve-out lives here, manifest-adjacent, because
the content is Unity-supplied engine-companion material, not an imported
battlefield asset.

`reconstruction/tests/test_tmp_license.py` pins all of this: the notice
file, the OFL text, the archived license evidence, and that no OTHER
directory has crept in beside the manifest-governed trees.
