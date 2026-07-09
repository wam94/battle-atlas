# V2 Phase 3 — HDRP/URP visual-target bake-off (Gate P3 viewing guide)

Branch `v2-phase3`. Plan: `docs/superpowers/plans/2026-07-08-angle-reconstruction-v2.md`
§12 Phase 3, §8.1. **Gate P3 closes only on owner approval of an eye-level
frame — this document packages the candidates; it does not close the gate.**

## What was built

1. **True-scale Angle terrain crop** (`pipeline` `crop` command):
   battlefield-local `x=3900..4700, z=4450..5250` (800 m square — Emmitsburg
   Road crossing, the approach, stone wall/Angle, Copse, Cushing's position),
   1025×1025 samples (0.781 m spacing), `verticalExaggeration == 1.0`, cut
   from the cached USGS 3DEP 1 m bare-earth DEM (no modern above-ground
   structures survive that product). Local↔macro↔UTM round-trips are tested
   in both Python (`pipeline/tests/test_crop.py`) and Unity EditMode
   (`AngleTerrainFrameTests`). Regenerate:

   ```sh
   cd pipeline && uv run python -m terrain_pipeline.cli crop
   ```

2. **Identically staged URP and HDRP scenes** (`AngleBakeoffStage`):
   the crop terrain at 1.0×, placeholder Emmitsburg Road strip, rail fences
   (road flanks + an approach remnant so fence and wall share the eye-level
   frame), the Angle stone wall with its 90° jog, wheat band, Copse trees,
   100 soldiers (80 CSA advancing / 20 USA at the wall, tight double-rank
   lines, drawn through `Graphics.RenderMeshInstanced` — deliberately the
   Atlas's instanced path), deterministic black-powder smoke bank, and the
   ephemeris sun at the canonical comparison time
   **t=8400 → 15:20 LMT** (elevation ≈ 45.5°, azimuth ≈ 262.8°).
   All content comes from `AngleBakeoffLayout` (pure constants + the shared
   FNV jitter; no `Random`), so both pipelines stage byte-identical
   geometry. Only shaders/sky/exposure plumbing differ (that is what is
   being compared): URP/Lit + procedural skybox + linear fog + ACES versus
   HDRP/Lit + physically based sky + fixed exposure 13.2 EV + HDRP fog +
   ACES.

3. **Committed thin scenes** `Assets/Scenes/AngleVisualTarget-URP.unity` /
   `-HDRP.unity`: each holds only a marker; stage content via
   **BattleAtlas ▸ Bakeoff ▸ Stage Content For Open Scene Marker** (or the
   explicit URP/HDRP menu items). For interactive HDRP viewing use
   **BattleAtlas ▸ Bakeoff ▸ Use HDRP For This Session** (session-only;
   nothing saved) and **Restore URP (Session)** afterward.

## The frames (gitignored; regenerate below)

`docs/benchmarks/captures/`:

| Frame | URP | HDRP |
| --- | --- | --- |
| Theater (whole 800 m tile, high oblique) | `p3-theater-urp.png` | `p3-theater-hdrp.png` |
| Tactical (assault corridor, 45 m up) | `p3-tactical-urp.png` | `p3-tactical-hdrp.png` |
| **Eye level (Gate P3 candidate, 1.66 m, 68°)** | `p3-eye-urp.png` | `p3-eye-hdrp.png` |
| Side-by-side contact sheet | `p3-contact-sheet.png` | (same file) |

Camera definitions are code (`AngleBakeoffLayout.Cameras`), deterministic.
The eye-level camera stands at the CSA line's rear-left flank
(crop-local (417, 360), eye height 1.66 m, fov 68° — the §6.5 viewpoint
parameters), looking NE across the advancing line toward the approach
fence, wall corner, smoke bank, and Copse.

Regenerate everything (Unity editor must be CLOSED for this project copy):

```sh
UNITY=/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity
"$UNITY" -batchmode -projectPath app -executeMethod \
    BattleAtlas.EditorTools.AngleBakeoffRender.RenderUrp  -logFile /tmp/p3-urp.log
"$UNITY" -batchmode -projectPath app -executeMethod \
    BattleAtlas.EditorTools.AngleBakeoffRender.RenderHdrp -logFile /tmp/p3-hdrp.log
cd pipeline && uv run python ../scripts/p3-contact-sheet.py
```

## Measurements (Apple M4, 24 GB, macOS 26.5.1, 2560×1440)

Per-frame time = instanced-draw issue + full pipeline render + synchronous
full-frame readback (an honest offline seconds-per-saved-frame number),
averaged over 8 frames after 3 warmups. Memory = Unity
allocated/reserved after all renders. JSON: `p3-measurements-{urp,hdrp}.json`.

| Metric | URP | HDRP |
| --- | --- | --- |
| Theater ms/frame (avg / min / max) | 25.6 / 18.4 / 64.5 | 36.2 / 26.0 / 105.1 |
| Tactical ms/frame | 25.4 / 21.2 / 46.4 | 39.0 / 33.3 / 75.3 |
| Eye ms/frame | 34.7 / 28.0 / 62.0 | 50.6 / 41.9 / 99.6 |
| Allocated / reserved MB | 348 / 772 | 361 / 763 |

Both are far inside the §3.5 render budget (≤ 5 s/frame ⇒ ~27.5 render-
hours for the full viewpoint); at these placeholder complexities either
pipeline renders the 19,800-frame hero viewpoint in **well under one hour**.
Budget headroom, not final numbers — production crowds/materials/volumetrics
will grow both.

### Setup complexity notes

- **URP:** zero new configuration (project's pipeline). Bake-off runs off a
  transient copy of `PC_RPAsset` with a 1200 m shadow distance.
- **HDRP:** package install (`com.unity.render-pipelines.high-definition`
  17.4.0) compiled cleanly on Apple Silicon/Metal; first activation
  generated `Assets/HDRPDefaultResources/HDRenderPipelineGlobalSettings.asset`
  (committed) and registered it in `GraphicsSettings.asset`'s
  global-settings map (committed; the project's ACTIVE pipeline remains
  URP). Scene needs a Volume (sky/exposure/fog) or it renders black —
  staged in code. Physical light units (sun ≈ 95,000 lux) and fixed
  exposure required.
- **Headless quirk (both pipelines):** in `-batchmode -executeMethod`, the
  C++ render loop that binds `currentRenderPipelineGlobalSettings` never
  runs, so pipeline construction NREs. `AngleBakeoffRender` binds it via
  reflection (version-pinned to 6000.4, fails loudly). Interactive editor
  use is unaffected.
- **`Graphics.RenderMeshInstanced` works under BOTH pipelines** (the
  soldiers/smoke in every frame are instanced draws), including inside
  headless render requests when issued from `beginCameraRendering`.

## What to look at for Gate P3

Open `p3-eye-urp.png` and `p3-eye-hdrp.png` (or the contact sheet) and judge:

- true-scale ground (compare the Atlas's 2.5× sand-table relief — this is
  the honest, nearly-flat approach Pickett's men actually walked);
- rail fence (right), stone wall + jog (center-right distance), soldier
  line with kepi cue, smoke bank, Copse silhouette;
- shadowing: 15:20 WSW sun means shadows fall ENE — *away* from an
  east-facing camera and foreshortened; tree/figure shadows are visible but
  not dramatic. That is the ephemeris, not a lighting bug;
- atmosphere: URP = analytic linear fog; HDRP = physically based sky with
  altitude-dependent haze (the deeper zenith and warmer field tone).
- **No macro slabs exist in any frame** (nothing from the Atlas scene is
  staged).

Known placeholder shortcuts (Phase 5–8 work, listed so they are not judged
as pipeline differences): monochrome figures (pipeline Lit shaders ignore
the vertex-color uniform bands; a dark head/kepi instanced overlay restores
the silhouette), box smoke puffs, unsourced road/fence/wall/copse positions
(roughly correct relations only), no Codori buildings, no artillery pieces.

If neither eye-level frame is approvable, Gate P3 stays open: the owner
should say what fails (scale read? figure legibility? atmosphere?) and the
staging — not the pipeline choice — iterates.
