# Descriptive Graphics Techniques for Battle Atlas
## URP 17 (Unity 6000.4) / iOS-Metal research — moving from "indicative" to "descriptive"

Research date: 2026-07-02. Baseline read from `app/Assets/Scripts/VegetationField.cs`,
`FenceField.cs`, `InstancedMeshes.cs`, `BattleDirector.cs`,
`app/Assets/Editor/LandcoverImporter.cs`, `HeightmapImporter.cs`.

Baseline: 8.5 km² LiDAR terrain (4097 heightmap, 2.5× vertical exaggeration), 5 solid-tint
terrain layers from a 1024 splatmap, 52,480 procedural box-blob trees via
`Graphics.RenderMeshInstanced` (≤1023/batch), 2,761 fence posts, instanced soldiers,
unit boxes. Deterministic FNV-jitter, zero per-frame allocations, Material assets only.
Target: recent iPhone (A17/A18-class), 60 fps.

---

## Executive summary (build order, cheapest-biggest-win first)

| # | Element | Technique | Est. cost @60fps A17/A18 | Legibility win |
|---|---------|-----------|--------------------------|----------------|
| 1 | Lighting | Real-time directional sun on computed 1863 ephemeris + optional "reading light" mode; NO baked lightmaps | ~0 ms extra (already lit) | Huge — relief pops everywhere |
| 2 | Terrain AO/curvature | Pipeline-baked hillshade + cavity/AO multiplied into splat tints (offline, from the real DEM) | ~0 ms runtime | Huge — swales/dead ground |
| 3 | Terrain ground detail | Drop to 4 layers (single pass), noise/detail albedo + normal per layer instead of flat 16×16 tints | ~0.2–0.5 ms | Big — wheat vs pasture reads at all distances |
| 4 | Trees | Keep RenderMeshInstanced; richer near mesh + cheap far mesh, spatially-chunked batches with explicit `worldBounds`, shadows OFF | Net ~neutral or cheaper than today | Big — canopy vs orchard row structure |
| 5 | Fences/walls | Dedicated stone-wall segment mesh + per-instance hash irregularity; keep post+rail for rail fences | negligible | Medium |
| 6 | Crops (wheat/corn) | Instanced crossed-quad clumps on "Field" splat cells, camera-radius ring only, distance-faded | 0.5–1.5 ms inside ring | Big at soldier LOD |
| 7 | Rocky ground | Instanced boulder meshes seeded from a Devil's Den class mask (same FenceField pattern) | negligible | Medium, localized |
| 8 | Units | 2–3 pose-variant soldier meshes (hash-picked), instanced flag quads with vertex-wave shader | negligible | Medium |
| 9 | Shadows | 1 directional, 2 cascades, 1024–2048, max distance ≈ 150–300 m, soft OFF or low; trees excluded | 1–2 ms when camera is low | Medium (close-in only) |
| — | SSAO | **Avoid.** Bake instead (item 2) | saved: 1.5–3+ ms | — |

Everything below stays inside the project's existing constraints: deterministic (FNV
hash / fixed noise seed), zero per-frame allocations, Material **assets** (iOS shader
stripping), `RenderMeshInstanced` ≤1023/batch.

---

## 0. Sun position — July 3, 1863, Gettysburg (computed, use these numbers)

Computed with the NOAA/Meeus solar position algorithm for lat 39.81° N, lon 77.23° W.
Note on time basis: **1863 predates US standard time zones (1883)** — "local time" in the
battle accounts is Local Mean Time at Gettysburg's meridian (UT −5h08.9m). Declination on
the day: +22.98°. Azimuth is compass (0=N, 90=E, 180=S, 270=W) — maps directly to Unity
light yaw the same way `facingDeg` does.

| LMT  | Sun elevation | Sun azimuth | Notes |
|------|--------------|-------------|-------|
| 12:00 | 73.2° | 177.0° | near solar noon |
| 13:00 | 69.4° | 219.4° | Pickett's Charge bombardment begins ~13:00 |
| 13:30 | 65.2° | 233.7° | |
| 14:00 | 60.3° | 244.3° | infantry advance ~14:00–15:00 |
| 14:30 | 54.9° | 252.4° | sun WSW — light rakes *down* the charge axis (Confederates advanced ~east) |
| 15:00 | 49.3° | 259.0° | |
| 15:30 | 43.6° | 264.7° | |
| 16:00 | 37.9° | 269.7° | sun due west |
| 17:00 | 26.4° | 278.8° | |

Directional-light Euler for Unity: `rotation = Quaternion.Euler(elevation, azimuth + 180°, 0)`
(light points *from* the sun *toward* the ground).

**The honest problem:** at 13:00–15:00 the sun is 49–69° high. High sun is exactly why
noon aerial photos look flat — 60°+ elevation produces almost no relief shading on
Gettysburg's gentle slopes (~167 m relief over 8.5 km, even at 2.5×). The historically
honest sun will NOT make swales legible by itself until ~15:30+.

**Recommendation:** drive the real sun from battle time (it's cheap and honest — it's just
`light.transform.rotation` per frame from a 2-key table lerp), but add a **"reading
light" toggle**: a fixed NW-ish raking light (az ~315°, el ~30–35°), which is the
cartographic hillshade standard precisely because it maximizes relief legibility. Label
it in UI as a presentation mode, like the 2.5× exaggeration already is (same "display-only,
data stays honest" doctrine as `HeightmapImporter.VerticalExaggeration`).

---

## 1. Terrain ground detail

### 1a. First, fix the layer count: 5 layers = the whole terrain renders twice

URP's Terrain Lit shader packs **4 layers per pass** (one RGBA splat control texture);
layers 5+ trigger an *add pass* that re-rasterizes the entire terrain
([URP Terrain Lit reference](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/shader-terrain-lit.html),
[community confirmation](https://discussions.unity.com/t/add-more-than-4-layers-for-a-custom-terrain-shader-urp/900638)).
Worse, URP's **height-based blending is silently ignored when the terrain has more than
four layers** (same doc). The current importer creates 5 (`Pasture, Field, Woods, Orchard,
Marsh`).

**Recommendation:** merge Orchard into Pasture (or Woods) at the splat level — orchards are
readable from tree *row structure* (element 2), not ground tint; the ground under an 1863
orchard is grass anyway. 4 layers → single terrain pass, height-blend available. This is a
~10-line change in `LandcoverImporter.Layers` + the pipeline splat encoder.

### 1b. Detail textures + normal maps per layer — yes; triplanar — no (and not needed)

- Replace the 16×16 flat-tint diffuse textures with small (256–512) tileable albedo +
  normal per layer: pasture = mottled green with fine grass-blade noise normal; field =
  golden wheat-stubble rows (directional stripe pattern — July wheat was ripe/being cut);
  woods = dark leaf-litter; marsh = darker mottle. Keep the current hue palette so the
  sand-table read survives; the alpha=0 smoothness trick in `CreateTerrainLayer` still
  applies (author alpha=0 into the new albedo textures).
- Per-layer **normal maps are supported** by URP Terrain Lit and are what makes raking
  light work at ground scale. Cost on A17/A18: one extra texture sample per layer per
  pixel — with 4 layers/1 pass this is well within budget (~0.2–0.5 ms est. at native res).
- Enable **Per-pixel Normal** on the material (requires terrain **Draw Instanced** ON) so
  distant terrain keeps geometric normal detail from the 4097 heightmap instead of
  vertex-interpolated mush — directly serves swale legibility
  ([URP Terrain Lit reference](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/shader-terrain-lit.html)).
- **Triplanar: URP Terrain Lit does not support it** (that's an HDRP TerrainLit feature),
  and Gettysburg has no cliffs steep enough to smear a top-down projection — even at 2.5×
  exaggeration, max slopes are modest. Skip it; don't buy a custom shader graph terrain
  for this.
- Set **Base Map Distance** (terrain settings) to ~800–1500 m so per-layer sampling only
  runs near the camera; beyond it Unity uses the pre-composited basemap — essentially free
  fill rate at the strategic zoom
  ([URP perf configuration](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html)).
- Add one **macro variation texture** (a 1–2 km-tile low-frequency noise multiplied over
  albedo). URP Terrain Lit has no built-in macro slot; the cheapest honest version is to
  bake the variation *into the splat-tint colors offline in the pipeline* (see §3 AO bake —
  same texture channel trick), which costs zero runtime.

Tiling: current `TileSizeM = 50` is fine for tint, too coarse for detail; use ~4–8 m for
the detail albedo/normal and let the macro/AO bake kill the repetition at distance.

### 1c. Wheat/corn as geometry — what shipped mobile titles actually do

Consensus from mobile-scale references: nobody renders per-stalk crops on phones. The
pattern is a **three-band read**:
1. **Far (strategic zoom, most of your play time):** ground texture only — the golden
   stripe/row pattern in the Field layer albedo + normal IS the wheat field.
2. **Mid:** nothing extra — texture + AO bake carries it.
3. **Near (soldier LOD, camera inside ~150–300 m):** sparse instanced **crossed-quad
   clumps** ("X" cards, ~0.9 m tall for July wheat, 8–16 verts each), density from the
   splatmap Field class, faded out by distance in the vertex shader (scale-to-zero, not
   alpha — cheaper on TBDR).

Two implementation routes:

- **Unity terrain detail meshes, GPU-instanced mode.** Supported in URP; instanced detail
  rendering batches ≤1023, and Unity explicitly recommends the instanced mode
  ([Grass and other details](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Grass.html)).
  Determinism: detail placement is generated from a density map + **Noise Seed** field, so
  it is reproducible; you'd fill `terrainData.SetDetailLayer` from the splatmap in
  `LandcoverImporter`. Caveats: instanced details don't support the healthy/dry color
  noise, and detail density/distance are global knobs you tune live (Detail Density ~0.3–0.6,
  Detail Distance 150–250 m for iOS).
- **Your own `RenderMeshInstanced` crop field** (a `CropField.cs` cloned from
  `VegetationField`): clump positions hash-jittered per splat cell exactly like tree
  placement — *fully* deterministic under your existing FNV scheme, zero new subsystems,
  and you control the camera-radius ring rebuild (rebuild only when the camera crosses a
  cell boundary, reusing preallocated matrix buffers → still zero steady-state alloc).

**Recommendation: the second** (own instanced crop ring). It matches every house
constraint you already enforce, and terrain-detail placement determinism, while real, is
Unity-version-dependent internal behavior rather than your own hash. Budget: a 250 m ring
at 1 clump / 4 m² over, say, 30% field coverage ≈ 5–15k clumps ≈ 5–15 batches ≈
0.5–1.5 ms GPU (est.) — and only when zoomed in.

**Overdraw warning (applies to both routes):** avoid alpha-tested billboard grass. On
Apple TBDR GPUs, `discard`/alpha-test defeats hidden-surface removal and stacked
transparent quads are the classic vegetation fill-rate killer. Use *opaque* crossed quads
with the wheat texture baked to solid texels (no cutout silhouette), or cutout only at the
top edge. See §6.

---

## 2. Trees

### Keep `RenderMeshInstanced`; do not migrate to Unity terrain trees

Unity terrain trees would give you automatic billboarding and batching, but: (a) the
billboard path is designed around SpeedTree-style prefabs with LODGroup/BillboardRenderer
assets — more asset pipeline, exactly what `InstancedMeshes` avoids; (b) SpeedTree-style
trees carry 3–4 materials → multiple draw calls each, which Unity's own guidance says to
avoid on mobile ([tree performance tips](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Tree-Performance.html),
[SpeedTree manual](https://docs.unity3d.com/6000.4/Documentation/Manual/SpeedTree.html));
(c) placement via `TreeInstance[]` is deterministic data you could write, but wind/color
variance/billboard transitions are engine-internal and version-drifty. Your current
approach already is the "right" mobile architecture (GPU instancing, one material, one
mesh); it just needs a better mesh and real culling.

(For reference: Unity 6's **GPU Resident Drawer** automates BatchRendererGroup instancing
for *GameObjects* — not applicable to your procedural draw path, and its LOD crossfade
support is still limited
([GRD manual](https://docs.unity3d.com/Manual/urp/gpu-resident-drawer.html),
[LOD crossfade thread](https://discussions.unity.com/t/lod-crossfade-in-graphics-resident-drawer/1561993)).
`BatchRendererGroup` directly would remove the 1023 cap and the managed matrix arrays, and
is proven at 60 fps on far weaker devices than A17
([Unity BRG sample blog](https://unity.com/blog/engine-platform/batchrenderergroup-sample-high-frame-rate-on-budget-devices)) —
but it's a bigger rewrite; treat as a later optimization, not a prerequisite.)

### 2a. Fix culling first — it's currently the biggest hidden cost

`Graphics.RenderMeshInstanced` culls **per call, not per instance**: "Unity uses the
bounds to cull and sort all the instances of this Mesh as a single entity"
([RenderMeshInstanced API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html)).
`VegetationField.BuildBatchesFromTrees` batches trees in **file order**, so every batch
likely spans the map and nothing ever frustum-culls — all 52,480 trees are vertex-shaded
every frame from every camera angle. Fix:

1. **Bucket trees into spatial cells** (e.g., 512 m × 512 m grid → ~280 cells; split any
   cell >1023 trees). Deterministic — it's a pure function of position.
2. **Set `RenderParams.worldBounds` per cell** (min/max of instance positions + tree
   height). Don't rely on default bounds — set them explicitly and verify culling in the
   Frame Debugger.
3. Same fix applies verbatim to `FenceField`.

This alone can cut tree vertex cost 40–70% for typical oblique camera angles, for free.

### 2b. Richer mesh + 2-tier LOD (no impostor tier needed at this instance budget)

- **Near mesh (~within 800–1200 m):** trunk cylinder (6-sided) + 2–3 squashed icosphere
  canopy blobs ≈ 120–200 verts. Deciduous woodlot read: wide low canopy. Add a second
  *understory* color band (darker, lower blob) — canopy+understory density is exactly the
  concealment cue you want. Per-instance hue/scale jitter via FNV hash packed into a
  float and read in shader (or simpler: 3–4 pre-tinted `MaterialPropertyBlock` buckets,
  trees hash-assigned — zero shader work).
- **Far mesh (beyond):** your current 24-vert box-blob tree, or an even cheaper 2-blob
  16-vert version. At >1 km on a phone screen a tree is 2–8 pixels; geometry beats
  billboards here (no alpha, no sorting, no atlas — see §6 on billboard overdraw).
  **Skip octahedral impostors entirely** — they pay off for 10k-vert SpeedTrees, not
  200-vert blob trees; their alpha-blended cards are a TBDR overdraw tax
  ([impostor breakdown refs](https://github.com/roundyyy/Tree-Cross-Quad-Impostor-Generator),
  [Amplify Impostors](https://80.lv/articles/new-optimization-solution-amplify-impostors)).
- **Tier switch per spatial cell, not per tree** (cell center vs camera distance, with a
  hysteresis band exactly like `BattleDirector`'s LOD latch). Cells own two prebuilt
  matrix arrays? No — same array, two meshes: the matrices don't change, only which mesh
  the cell draws. Zero per-frame allocation, deterministic, no crossfade needed (pop at
  1 km with hysteresis is invisible; URP's own guidance is to disable LOD crossfade on
  mobile anyway — [URP perf configuration](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html)).
- **Orchards:** the row structure is already in your `trees.json` positions (if the
  pipeline emits planted grids, it will read); render orchard-class trees with a distinct
  rounded-lollipop near mesh at `OrchardScale`, brighter green. Orchard legibility comes
  from regular spacing + smaller uniform size — geometry you already have.

**Budget reality check (A17 Pro/A18, Metal):** these GPUs sustain >1 GTri/s class
throughput; the practical mobile ceiling is bandwidth/fill, not vertex count at this
scale. 52,480 trees: worst case today = 52k × 36 tris ≈ 1.9 M tris/frame, every frame,
uncullled. After the cell fix, a typical view might hold ~20k trees: e.g. 3k near
(200 verts → 600k verts) + 17k far (16 verts → 272k) ≈ **<1 M verts/frame ≈ comfortably
60 fps** with a simple lit shader. Verify on device per project doctrine, but the
headroom is real — the BRG sample hits 60 fps with similar scenes on a Galaxy A51
([Unity blog](https://unity.com/blog/engine-platform/batchrenderergroup-sample-high-frame-rate-on-budget-devices)).

**Shadow trap:** `RenderParams` defaults `shadowCastingMode = On`. The moment main-light
shadows are enabled (element 3/9), all 52k trees render *again* into every cascade.
Explicitly set `shadowCastingMode = ShadowCastingMode.Off` on tree/fence/crop
RenderParams; only near-cell trees (and soldiers) should ever cast, if at all.

---

## 3. Lighting for terrain readability

### 3a. Baked vs realtime: realtime directional is the only option that scrubs

A time-scrubbable sun rules out lightmap baking for the sun term (a bake is one frozen
sun position; N bakes don't interpolate honestly, and nothing in this scene is
lightmap-eligible anyway — terrain + procedural instances). So:

- **Main light = realtime directional**, rotation driven from `BattleClock.CurrentTime`
  through the §0 ephemeris table (lerp between hourly keys; 2 float3 lerps per frame —
  free). No lightmaps, no light probes → also avoids the "instanced details don't support
  probes" limitation.
- **Ambient:** flat ambient or a simple gradient (sky/equator/ground tricolor) tuned so
  shadowed slopes stay readable, slightly cool vs the warm sun. Scrubbing 13:00→16:00,
  warm the sun color and drop intensity slightly toward 16:00 — cheap and evocative.

### 3b. Terrain-scale shadowing: bake it from the DEM, don't shadow-map it

A shadow map cannot cover 8.5 km at mobile budgets (see 3d), and at 49–69° sun elevation
cast shadows are short anyway. The honest, free solution is **pipeline-side raking-light
bakes from the real (unexaggerated ×2.5 display-matched) DEM**:

- **Hillshade / analytic sun term:** N·L computed from the DEM at, say, 4 sun keys
  (13/14/15/16 LMT) → 4 grayscale textures (1024–2048). At runtime the terrain shader—or
  far simpler, a pre-multiplied splat tint—darkens by the current hillshade. Simplest
  honest version that needs *no custom shader*: bake ONE neutral relief-legibility layer
  (see next bullet) into the albedo and let the realtime light do the time-varying part.
- **Baked AO / sky-view factor from the heightmap:** classic cartographic relief
  reinforcement — for each texel, how much sky hemisphere is visible. Swales, the
  Plum Run valley, and reverse slopes darken subtly regardless of sun position;
  time-invariant, so it composes honestly with the moving sun. Multiply into the layer
  tints **offline in `LandcoverImporter`** (modulate the splat colors per-texel when
  generating layer textures — or emit a single 1024 "AO tint" texture and multiply it
  into the terrain basemap). Zero runtime cost. This is the standard replacement for
  SSAO on mobile terrain
  ([polycount AO map](http://wiki.polycount.com/wiki/Ambient_occlusion_map),
  [heightmap AO reference](https://github.com/awsdocs/amazon-lumberyard-user-guide/blob/master/doc_source/mat-shaders-heightmap_ambient_occlusion.md)).
- (Later, fancier, still honest: **horizon mapping** — bake per-texel horizon angles in
  8 directions from the DEM; the shader compares sun elevation vs horizon angle for true
  terrain self-shadowing at ANY scrubbed sun position, one texture sample. This is the
  "right" scrubbable terrain shadow technique, but it needs a custom terrain shader pass —
  defer.)

### 3c. SSAO: don't

URP SSAO needs the DepthNormals prepass (or depth-only reconstruct) + a multi-sample AO
pass + blur — full-screen bandwidth passes, the worst thing you can do on a TBDR mobile
GPU. Unity's perf guidance singles out SSAO settings as having "a large performance
impact" ([URP perf configuration](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html),
[URP SSAO reference](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-ssao.html)).
Realistic cost on an iPhone at native-ish res: **1.5–3+ ms** (estimate; worse with
Depth-Normals source). And SSAO would darken *box soldiers*, not terrain relief — the
wrong AO for this product. The §3b bake gives terrain AO for free. Skip; revisit never.

### 3d. Realtime shadow budget (for the close-in soldier tier only)

Per Unity's URP mobile guidance (reduce cascades, reduce max distance, soft shadows off
or low — [shadow optimization](https://docs.unity3d.com/6000.0/Documentation/Manual/shadows-optimization.html)):

- Main light shadows ON, **2 cascades**, resolution **1024–2048**, **Max Distance
  150–300 m** — shadows exist only at soldier/fence zoom, where they anchor figures to
  the ground. At strategic zoom the shadow pass renders almost nothing.
- Soft shadows OFF (or Low quality) — soft shadows "significantly impact tile-based
  platforms" (same doc). Hard 1024 shadows at 150 m distance look fine at figure scale.
- Shadow casters: soldiers, fences, near-cell trees ONLY (explicit
  `shadowCastingMode` per RenderParams — see §2 trap). Terrain *receives*, never casts
  (terrain self-shadowing comes from §3b bakes).
- Expected cost: ~1–2 ms when zoomed in, ~0 when out. If soldiers alone need grounding,
  an even cheaper trick is an instanced **blob quad** under each figure (no shadow pass at
  all) — consider before enabling shadow maps.

---

## 4. Swale / dead-ground legibility

Is 2.5× + lighting enough? **Almost, but not at 13:00–15:00 sun.** The honest fixes,
in order of honesty-per-millisecond:

1. **Baked AO/sky-view factor** (§3b) — pure function of the real DEM; darkens concavities
   exactly where dead ground is. This is the single biggest swale win and costs nothing.
2. **Curvature-based tint** — bake a curvature (Laplacian-of-DEM) map in the pipeline:
   concave → slightly darker/cooler, convex crests → slightly lighter/warmer. This is a
   standard relief-cartography and game-texturing technique (cavity/curvature maps —
   [polycount curvature map](http://wiki.polycount.com/wiki/Curvature_map),
   [terrain shading techniques survey](https://www.maplibrary.org/11231/7-terrain-shading-techniques-a-comparison/)),
   and it is **honest**: it visualizes a derivative of the real elevation data, adds no
   fictional relief. Bake it into the same albedo modulation as the AO — still zero
   runtime cost. Keep it subtle (±8–12% luminance) so it reads as ground variation.
3. **Contour hinting** — faint elevation contours (e.g., 3 m interval at 2.5× display
   scale) as a shader effect (`frac(worldY / interval)` + `fwidth` AA) or, zero-shader
   version, baked into the AO/curvature tint texture as ~4%-darker lines. Perfectly
   honest (it IS the DEM), instantly reads as "map", and quietly labels every swale.
   Recommend baked-texture contours first (free, no custom terrain shader); shader
   contours later if you want a toggle.
4. **"Reading light" toggle** (§0) for when the historical sun is too high.

Recommended stack: 1 + 2 always on (one combined baked modulation texture), 3 as a
UI toggle, 4 as a UI toggle. All derived from the DEM in `pipeline/`, all deterministic,
all zero runtime cost.

---

## 5. Unit rendering

- **Pose variants:** build 2–3 soldier meshes in `InstancedMeshes` (standing/shoulder-arms,
  advancing lean, kneeling-firing). `RenderMeshInstanced` draws one mesh per call, so
  `UnitFormationRenderer` splits each unit's figures into per-pose matrix buckets —
  pose chosen by `FormationLayout.Jitter(unitId, i, salt)` (deterministic), optionally
  biased by unit state (advancing units → lean pose; engaged → firing pose, which turns
  pose into *information*). Cost: figures ≤400/unit already fit one batch; 3 buckets →
  ≤3 calls/unit instead of 1. With ~10–20 close units that's +20–40 draw calls — nothing
  on Metal. Preallocate the 3 buckets at `MaxFigures` size once (house rule: no growth at
  render time).
- **Two-tone figures without new draws:** cheapest visual upgrade is vertex colors on the
  soldier mesh (dark trousers / coat body / flesh head band) — `InstancedMeshes` already
  owns the mesh build; add a `Color32` channel and a shader that multiplies vertex color ×
  `_BaseColor`. One mesh, same batching, soldiers stop being monochrome boxes.
- **Flags:** one quad mesh (~8×4 segments, 45 verts) per regiment/brigade, instanced
  across all units in ONE `RenderMeshInstanced` call. Vertex-wave in the shader:
  `y += sin(worldPos.x * k + _Time.y * w) * amplitude * uv.x` (pinned at the staff edge,
  phase from world position → per-flag desync with zero CPU work and full determinism —
  time-scrubbing even replays the same wave). Shader Graph does this trivially
  ([Unity flag-wave tutorial](https://learn.unity.com/project/make-a-flag-move-with-shadergraph),
  [Shader Graph feature examples include a waving flag](https://docs.unity3d.com/Packages/com.unity.shadergraph@14.0/manual/Shader-Graph-Sample-Feature-Examples.html)) —
  but remember the iOS stripping lesson: the graph must live on a **Material asset**
  referenced from the scene. Color: `_BaseColor` per side via the existing
  MaterialPropertyBlock pattern (union national colors vs battle flag red as flat tints).
  Flags are the single best "unit identity at middle zoom" feature per vertex spent.
- **Skip skeletal animation entirely** at this figure scale — pose-swap + flag motion
  gives 90% of the life at ~0% of the cost.

---

## 6. What to AVOID on mobile URP (traps, with the mechanism)

1. **5+ terrain layers** — add pass re-renders the whole terrain and disables URP
   height-based blending (§1a). Stay at 4.
2. **SSAO** — full-screen depth(+normals) prepass + AO + blur = bandwidth on a TBDR GPU;
   1.5–3+ ms for AO that mostly shades unit boxes. Bake from the DEM instead (§3b/§4).
3. **Realtime shadows from mass instancing** — `RenderParams.shadowCastingMode` defaults
   **On**; enabling main-light shadows without setting it Off on trees/fences/crops
   re-renders ~50k instances per cascade. Set Off explicitly; keep Max Distance ≤300 m,
   2 cascades, soft shadows off
   ([shadow optimization](https://docs.unity3d.com/6000.0/Documentation/Manual/shadows-optimization.html)).
4. **Alpha-tested/blended vegetation billboards** — clip/discard defeats Apple GPUs'
   hidden-surface removal, and stacked transparent cards are the classic vegetation
   fill-rate cliff. Prefer opaque low-poly geometry (blob trees, crossed quads with
   opaque interiors); if cutout is unavoidable, keep it to the top edge of the card and
   the instance count low.
5. **Un-bounded `RenderMeshInstanced` batches** — per-call culling means map-spanning
   batches never cull (§2a). Spatial cells + explicit `worldBounds`.
6. **LOD crossfade / dithered fades on mobile** — Unity's own URP perf page says disable
   LOD Cross Fade (alpha-test cost, same HSR issue as #4). Use hysteresis pops at
   generous distances instead — you already do this in `BattleDirector`.
7. **MSAA 4× + full-res rendering by default** — consider 2× MSAA (cheap-ish on TBDR) but
   test; and consider render scale 0.8–0.9 at the strategic zoom if fill-bound.
8. **Runtime-created materials** — already learned (magenta/stripping); the flag/detail
   shaders must ship as Material assets in Resources/scene references.
9. **Per-frame managed arrays for instancing** — already avoided; keep it that way for the
   new crop ring and pose buckets (preallocate at max size).
10. **Realtime point/spot lights** (e.g., muzzle-flash ambience later) — each shadowed
    point light = 6 shadow passes; use emissive/vertex tricks instead
    ([shadow optimization](https://docs.unity3d.com/6000.0/Documentation/Manual/shadows-optimization.html)).

---

## Suggested build order (restated with rationale)

1. **Sun ephemeris + reading-light toggle** — few hours; transforms relief legibility;
   no new assets. (§0, §3a)
2. **Pipeline bake: AO/sky-view + curvature (+optional contour) modulation into terrain
   tints** — pure `pipeline/` + `LandcoverImporter` work, zero runtime cost, biggest
   swale/dead-ground win. (§3b, §4)
3. **4-layer merge + detail albedo/normal per layer + per-pixel normal + basemap
   distance** — makes ground cover *type* readable; small, bounded GPU cost. (§1a/1b)
4. **Tree spatial cells + worldBounds + shadowCastingMode Off** — pure perf, likely
   *recovers* budget for everything else. (§2a)
5. **Tree near/far meshes + orchard/woodlot variants** — the canopy/row-structure read. (§2b)
6. **Stone wall vs rail fence meshes** — `InstancedMeshes.BuildWallSegment()` (low
   irregular block strip, hash-perturbed top edge) vs existing post+rails; medium win,
   trivial cost. (§5-adjacent, uses existing FenceField classes)
7. **Crop ring (instanced crossed-quads on Field cells)** — the waist-high-wheat read at
   soldier zoom. (§1c)
8. **Devil's Den boulders** — instanced rock meshes from a new landcover class mask;
   same pattern as trees. 
9. **Soldier pose buckets + vertex-color uniforms + instanced flags.** (§5)
10. **Main-light shadows, 150–300 m, 2 cascades** — last, because everything above must
    first set its shadow flags correctly. (§3d)

Each step is independently shippable and device-verifiable, per project doctrine.

---

## Sources

- URP Terrain Lit shader (layers, height-blend >4-layer limitation, per-pixel normal):
  https://docs.unity3d.com/6000.1/Documentation/Manual/urp/shader-terrain-lit.html
- URP configure-for-performance (SSAO impact, LOD crossfade, store actions, shadows):
  https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html
- URP shadow optimization (cascades, max distance, soft shadows on tile-based GPUs):
  https://docs.unity3d.com/6000.0/Documentation/Manual/shadows-optimization.html
- URP SSAO reference: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-ssao.html
- Terrain details/grass (instanced details, noise seed, density):
  https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Grass.html
- Terrain tree performance tips: https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Tree-Performance.html
- SpeedTree import/materials: https://docs.unity3d.com/6000.4/Documentation/Manual/SpeedTree.html
- Graphics.RenderMeshInstanced (per-call culling, 1023/511 limits):
  https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html
- RenderParams struct: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RenderParams.html
- GPU Resident Drawer (URP): https://docs.unity3d.com/Manual/urp/gpu-resident-drawer.html ;
  LOD crossfade limitation: https://discussions.unity.com/t/lod-crossfade-in-graphics-resident-drawer/1561993
- BatchRendererGroup 60 fps on budget mobile:
  https://unity.com/blog/engine-platform/batchrenderergroup-sample-high-frame-rate-on-budget-devices
- 1M-instance mobile grass via indirect instancing (NiloCat example):
  https://github.com/ColinLeung-NiloCat/UnityURP-MobileDrawMeshInstancedIndirectExample
- >4-layer URP terrain discussion: https://discussions.unity.com/t/add-more-than-4-layers-for-a-custom-terrain-shader-urp/900638 ;
  https://medium.com/@sinitsyndev/removing-the-4-layer-limit-for-height-based-blending-in-unity-terrain-urp-c0ba85444f58
- Impostors (when they pay off / overdraw caveats):
  https://github.com/roundyyy/Tree-Cross-Quad-Impostor-Generator ;
  https://80.lv/articles/new-optimization-solution-amplify-impostors ;
  https://www.simplygon.com/posts/b9c254b6-9ee1-47d3-b6aa-9418743e1f2a
- Cavity/curvature/AO maps for relief: http://wiki.polycount.com/wiki/Curvature_map ;
  http://wiki.polycount.com/wiki/Ambient_occlusion_map ;
  https://www.maplibrary.org/11231/7-terrain-shading-techniques-a-comparison/ ;
  https://github.com/awsdocs/amazon-lumberyard-user-guide/blob/master/doc_source/mat-shaders-heightmap_ambient_occlusion.md
- Flag vertex-wave: https://learn.unity.com/project/make-a-flag-move-with-shadergraph ;
  https://docs.unity3d.com/Packages/com.unity.shadergraph@14.0/manual/Shader-Graph-Sample-Feature-Examples.html
- Vegetation card/geometry practice: https://www.theastronauts.com/2014/02/approached-3d-foliage-vanishing-ethan-carter/ ;
  https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-1-toward-photorealism-virtual-botany
- Sun position: NOAA/Meeus solar position algorithm, computed for lat 39.81°N lon 77.23°W,
  1863-07-03, Local Mean Time basis (pre-standard-time). Script:
  scratchpad/sun.py (this session).
