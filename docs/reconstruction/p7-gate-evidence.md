# Gate P7 evidence — environment completion (plan §12 Phase 7)

**Gate criterion:** "the local scene is recognizably the Angle without units
or UI, at both tactical and eye height." Owner review closes the gate; this
document is the viewing guide for the prepared evidence.

Evidence frames (2560×1440 PNG, offline HDRP profile, 15:20 LMT ephemeris
sun at t=8400, no figures, no UI):
`docs/benchmarks/captures/p7-gate/` — staged on the `v2-phase7` worktree and
copied to the main checkout's same path (gitignored generated media).

## What each frame is, and what to judge

### `p7-tactical.png` — drone height over Kemper's approach, looking NE

Camera crop-local (140, 215) at +55 m, looking at (500, 430). Everything in
frame is traced or claimed geometry — nothing is placeholder blocking:

- **Emmitsburg Road** (left): the corridor IS the span between the two
  traced fences (ED-6); packed-dirt bed sunk 0.45 m (ED-11,
  `claim-road-sunken`), wheel-path wear painted along it.
- **Post-and-rail fences** both sides of the road (`claim-fence-*-structure`,
  Peyton's "three high post and rail fences" — `claim-fences-high-climb`).
- **Standing wheat + stubble** west of the road only (Haskell's "wheat, now
  nearly ripe" — `claim-fields-west-mixed`, ED-15). The Phase 3 placeholder
  wheat band EAST of the road is gone: that ground is grass
  (`claim-field-east-grass`, Peyton + Cushing's gunner).
- **The stone wall with the right-angle jog** on the crest (traced
  `wall-angle-webb-front`, `claim-wall-structure`).
- **The Copse of Trees** behind the wall (traced polygon; sapling-scale
  scrub per `claim-copse-form-1863`, ED-14; no fence — 1863 had none,
  `claim-copse-unfenced-1863`).
- **Cushing's battery** behind the wall: two intact guns AT the wall,
  four disabled/wrecked pieces at the 30-pace crest line, limbers and
  caissons behind (ED-16; `claim-cushing-guns-to-wall`,
  `claim-cushing-armament`, `claim-battery-organization`).
- **Codori orchard** (bottom right, `claim-codori-orchard`) and the
  interior fences per their traces (one trace conflict resolved at the
  corridor — ED-12).
- **Trampled corridor** between road and wall reads as abused grass
  (ED-17; static approximation — Phase 8 owes track-driven trampling).

### `p7-eye-p3cam.png` — THE Phase 3 gate camera, unchanged

Crop-local (417, 360) at eye height 1.66 m, look (513, 408), fov 68 — the
exact ADR 0003 frame. Deferred items from that ADR to re-judge here:

- *"stone wall and road not in the eye frame"*: the **wall is now in frame**
  (crest line with the battery pieces and copse). The **road remains out of
  this frame** — it is 200 m behind the camera; putting it in frame is
  camera blocking, owned by Phase 9 (the `p7-eye-road` frame below shows the
  road at eye height instead).
- *"shadows not legible at the honest 15:20 sun with an east-facing
  camera"*: forward shadows still fall away from an east-facing camera at
  15:20 (physics, not a defect); judge shadow legibility on the fence/wall
  shadows in `p7-eye-road.png`.

### `p7-eye-road.png` — standing IN the Emmitsburg Road at the crossing

Crop-local (174, 330) at 1.66 m, corridor center between the traced fences,
looking ENE toward the inner angle. Judge: the sunken packed-dirt bed
underfoot, both fences as physical obstacles at true scale, wheat left,
the grass slope rising ~330 m to the wall — the ground Garnett's brigade
crossed under fire.

### `p7-eye-codori.png` — the Codori farmyard, orchard, and road

Crop-local (258, 262) at 1.66 m looking WSW (into the afternoon sun —
buildings are backlit and read dark; that is the honest light). Judge: the
orchard as gnarled fruit trees (`claim-codori-orchard`), the house and barn
massing beyond (ED-13 reconstruction-grade silhouettes: two-story frame
house per `claim-codori-house-structure`; bank-barn form per the Trostle
HABS analogue — HABS has NO Codori coverage, verified negative;
`claim-codori-barn-1863`), the road fences crossing the frame.

## Honest limitations (already known; judge the gate, not these)

- The 800 m tile ends against empty sky/haze — the macro battlefield is not
  staged around it (Phase 10/11 composition work).
- Buildings are documentary massing, not architectural models; backlit at
  15:20 from the east they read near-silhouette.
- Standing wheat is crossed-card clumps — credible at 30 m+, simple up
  close; a dedicated wheat card texture is a possible Phase 8/9 polish.
- Distant terrain tiling shows faint banding at tactical height.
- Trampling is a static ED-17 corridor; Phase 8 owes trampling driven by
  the compiled troop paths.
- Smoke, dust, and figures are deliberately absent (the gate says without
  units; combat atmosphere is Phase 8).

## Regeneration (all deterministic)

```sh
cd pipeline && uv run python -m terrain_pipeline.cli crop         # heightmap
cd pipeline && uv run python -m terrain_pipeline.cli environment  # sourced geometry + splat
"$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.GateP7Render.RenderStills \
  -logFile p7render.log
```

`p7-gate-report.json` beside the frames records the Unity version, offline
pipeline asset, sun battle-time, and the sha256 of the environment.json the
frames were staged from.

## Provenance chain

Every staged feature carries `claimIds` and/or `editorialIds` in
`data/heightmap_angle/environment.json`; the Unity stage builds ONLY from
that file. Pipeline tests validate the geometry against the traced land
cover, the claims corpus (93 claims incl. the 17 Phase 7 environment
claims), and the ED-11..ED-18 decisions in
`docs/reconstruction/angle-editorial-decisions.md`.

## Suite state at evidence time

- pipeline **59** (48 + 11 environment-bake tests)
- reconstruction **79** (claims corpus grew 76→93; validators green)
- tool **108**
- Unity EditMode **239** = 237 passed + 2 known conditional skips
  (12 new environment tests)
- Unity PlayMode **10/10** (dev proxy regenerated; no sync flake this run)

## Gate P7 verdict

**PASSED — closed by the project owner 2026-07-09**, with the guide's
listed limitations logged as deferred polish (tile-edge haze wall and
tactical-height banding → Phase 10/11 composition work). Owner also named
a future detail pass beyond this plan's gates: species-accurate
vegetation for Gettysburg, PA (the current copse/orchard/grove tree is a
single CC0 model at documented scales) — "we can get into detail later
... this looks good to move forward." 
