# Map furniture — roads, hydrology, town of Gettysburg, railroad

**Branch:** `map-furniture`, head `9218f5381bc27fcd1c981a9b6acd9815503b4c59`
(branched from `origin/main` at `c6f516645507ca91f289fcfa69d19b222ee05c2a`
— see "Invariants held" for why that fixed SHA, not the live
`origin/main` ref, is the correct diff base). **Gate:** visual — the owner judges the
before/after captures. **Owner's ask this slice answers:** roads,
rivers/streams, and the town of Gettysburg "appropriately built into the
map" — cartographic layers in the Atlas game view, in the existing map
grammar (muted, documentary), not photoreal environment art.

**Evidence:** `docs/benchmarks/captures/map-furniture/` —
`{before,after}-{theater,mid,tactical,rts-low}.png` (same camera poses
both builds, `MapFurnitureCaptureHarness` / `-mapfurnshots`). Perf:
`{before,after}-benchmark.json`.

## Sources and provenance

Every road, stream, rail, and town-block feature is traced from **sheet
j3-03** — the Bachelder Third Day set's No. 8 sheet (1-5 PM July 3,
archive.org `dr_battle-field-of-gettysburg-no-3-no-8-1-5-pm-july-3-1863-
12440022`, Rumsey/Stanford scan) — the repo's reference sheet
(`reconstruction/spatial/bachelder-manifest.json`, 31.0 m rms absolute
georeference). This is the *Warren 1873 engineer base* under Bachelder's
troop overlay: the same printed road/stream/building network underlies
every sheet in the set, so one full-field sheet carries the complete 1863
road/hydrology/town network for the whole 8.5 km crop — no need to
cross-reference multiple sheets for geometry (only for troop positions,
which this slice does not touch).

Tracing method: `reconstruction/scripts/crop_sheet.py` crops of the full-
resolution master (fetched via `fetch_bachelder_maps.sh`, sha256-verified
against the pinned manifest hash), read at scale factors from 0.16 (whole-
field overview, ~6 m/pixel effective) to 0.75 (town detail, ~1.3 m/pixel)
— dossier-pass reading discipline: pixel picks at bends and junctions, not
every meander, consistent with the sheet's own schematic fidelity and the
±31 m rms absolute floor. Two roads (Emmitsburg, Taneytown) and Fairfield
Road's SW end use the manifest's existing tie points directly (`peach-
orchard-crossroads`, `taneytown-x-wheatfield`, `fairfield-x-
blackhorsetavern`) as anchors — these are OSM-node-based, independently
placed intersections that both pin the road course and cross-check the
sheet reading. Two farm lanes (Codori, Bryan) anchor one end at the
manifest's `codori-barn`/`bryan-farm` check tie points.

Full crop-to-local pixel ledger (source of `reconstruction/scripts/
trace_map_furniture.py`'s `SHEET_PX` table): 9 crops generated at the
sheet's full resolution (overview, town, nw/ne/sw/se quadrants, rrcut,
south, rockcreek), each read visually for the features it covers. See the
script's module docstring and per-feature notes in `FEATURE_META` for the
full per-feature reading record (mirrors the audit dossiers' "cite sheet +
pixel read" convention, condensed here since this is one sheet, not a
multi-pass OOB read).

### What got traced, from where

| Feature | Class | Sheet region read |
|---|---|---|
| Chambersburg Pike | pike | rrcut crop (WNW corridor past the Seminary) + town crop (entry) |
| Baltimore Pike | pike | south crop (SE curve past Cemetery/Culp's Hill) |
| Mummasburg Road | road | nw crop |
| Carlisle Road | road | town crop (straight N spoke to the manifest's `town-diamond` tie point) |
| Harrisburg Road | road | ne crop |
| Hanover Road | road | ne crop (York Road's own split was not separately legible at this reading scale — merged; see Residuals) |
| Fairfield Road | road | rrcut crop + `fairfield-x-blackhorsetavern` tie point |
| Emmitsburg Road | road | town/south crops + `peach-orchard-crossroads` tie point |
| Taneytown Road | road | south crop + `taneytown-x-wheatfield` tie point |
| Codori farm lane | lane | `codori-barn` check tie point + one connector pick (inferred) |
| Bryan farm lane | lane | `bryan-farm` check tie point + one connector pick (inferred) |
| Gettysburg & Hanover RR (finished) | rail_finished | ne/town crops, same corridor as Hanover Road |
| Unfinished RR cut (west of town) | rail_unfinished | rrcut crop — the First Day Railroad Cut corridor |
| Rock Creek | creek | rockcreek + se crops, full N-S course through the square |
| Marsh Creek | creek | sw crop (near Black Horse Tavern) |
| Stevens Run | run | rockcreek crop (confluence with Rock Creek NE of town) |
| Willoughby Run | run | nw + sw crops, full length |
| Plum Run | run | sw + se crops (coarser grain — see Residuals) |
| Pitzer's Run | run | sw crop |
| Winebrenner's Run | run | south crop (low point density — see Residuals) |
| 8 town-block clusters | town_block | town crop, block-cluster simplification (see below) |

### Town massing simplification

The task calls for "period building-block massing... NOT individual
building detail." At this reading scale (the town spans ~700 x 900 m on
the sheet, individual blocks ~60-150 m), tracing every one of the ~25
distinct blocks precisely would cost reading time disproportionate to
what a cartographic furniture layer needs. Instead, 8 block-CLUSTER quads
approximate the built-up grid's shape: NW/NE quadrants north of the
Diamond, the Diamond block itself, SW/SE quadrants (upper and lower rows)
south of it, and a southward extension along Baltimore St toward Cemetery
Hill. Each cluster's corners were read off the sheet's street-grid lines,
not invented — the overall cross/diamond footprint matches the source —
but a future pass could split these into per-block polygons if the owner
wants closer massing fidelity.

## Data pipeline (first-class committed artifact)

- **Schema**: `docs/format/map-furniture-format.md` +
  `docs/format/map-furniture.schema.json` — sibling to
  `landcover-format.md` (same `points`/`kind`/`cls`/`source`/`confidence`
  shape; `landcover-format.md`'s "Planned extensions" section explicitly
  reserved roads/buildings for exactly this kind of fact-checked, PD-
  sourced addition). New `cls` vocabulary: `pike | road | lane` (line-
  weight road grammar), `creek | run` (stream-weight grammar),
  `rail_finished | rail_unfinished`, `town_block`.
- **Generator**: `reconstruction/scripts/trace_map_furniture.py` — a
  committed `SHEET_PX` table (full-resolution j3-03 pixel picks) run
  through `georef_maps.load_manifest()['j3-03'].to_local()`, rounded to
  0.1 m, clipped to the valid `[0, 8507.2]` square (the sheet extends past
  the heightmap square on three sides — no terrain to drape past the
  edge), and serialized with stable id-sorted order. Regenerating produces
  byte-identical output (`reconstruction/tests/test_map_furniture.py::
  TestDeterminism::test_regenerate_is_byte_identical`).
- **Validator**: `reconstruction/scripts/validate_map_furniture.py` —
  schema (jsonschema Draft7) + unique-id + non-empty-source-when-
  documented + in-bounds-points checks, CLI-runnable
  (`validate_map_furniture.py [<repo-root>]`, exit 1 with one error per
  line on failure). Pattern matches `validate_assets.py`.
- **Committed data**: `data/map-furniture/map-furniture.json` (28
  features; `.gitignore` carries a `!data/map-furniture/map-furniture.json`
  allow-rule, same convention as `data/landcover/landcover.json`).
- **Runtime consumer**: `app/Assets/Scripts/MapFurnitureData.cs` (JSON
  parser — JsonUtility can't deserialize the nested `[[x,z],...]` points
  arrays, a known Unity limitation; a small bracket-depth scanner extracts
  them separately and zips them back onto the JsonUtility-parsed metadata),
  `MapFurnitureMeshBuilder.cs` (pure static terrain-draped ribbon/fill
  builder), `MapFurnitureField.cs` (MonoBehaviour: builds 4 static meshes
  once at Start, redraws via `Graphics.RenderMesh` every frame —
  BattleDirector's idiom, no MeshFilter/MeshRenderer GameObjects),
  `MapFurnitureImporter.cs` (Editor: copies the committed JSON into
  `Assets/Generated`, creates 5 muted-ink HDRP Unlit material assets,
  wires `MapFurnitureField` onto `Gettysburg Terrain` — `LandcoverImporter`'s
  idiom). Wired into `CartographyStage.PrepareScene` alongside the
  heightmap/landcover steps.

## Styling vs. the map grammar

- **Road-class line weights**: pike 5 m, road 3.5 m, lane 2 m ribbon width
  — a single shared `RoadInk` material (muted warm umber, `#5C4630`); the
  CLASS distinction rides geometry, not color, matching the period-map
  convention the cartography slice already established for artillery
  (value/weight over hue).
- **Stream-class widths**: creek 5 m, run 2.5 m; `StreamInk` material, a
  subdued slate blue (`#4A6B7A`) chosen to read against the terrain's
  olive/tan palette without fighting it. Unlit shader (`Universal Render
  Pipeline/Unlit`): the ink reads identically under both `SunDirector`
  reading-light presets by construction — no per-mode tuning needed.
- **Railroad**: one `RailInk` material (near-black, `#302A26`); the
  unfinished cut renders DASHED (a fixed 8 m-on/5 m-off arc-length
  pattern) — the same "honesty in the geometry, not a second idiom" rule
  the cartography slice used for scattered/routed units' dashed borders.
- **Town blocks**: a two-submesh mesh (fill + outline) — `TownBlockFill`
  (`#B7A788`, a warm neutral between the terrain's Field and Pasture
  tints) and `TownBlockOutline` (`#6B5A42`, a darker edge stroke) — massing
  without individual-building detail, matching the task's explicit
  instruction.

## Two things the first capture pass caught

- **Render pipeline**: the project's active SRP is HDRP
  (`ProjectSettings/GraphicsSettings.asset`'s `m_CustomRenderPipeline` ->
  `Assets/Settings/HDRP/BattleAtlasHDRP_Playback.asset`), not URP —
  `LandcoverImporter.cs`'s own comments say "URP's Terrain Lit shader" but
  its CODE calls `Shader.Find("HDRP/TerrainLit")`; the comments are stale
  from an earlier iteration (there's a literal `HdrpMigration.cs` in the
  repo). The first capture pass, built against `Shader.Find("Universal
  Render Pipeline/Unlit")`, rendered every map-furniture mesh as Unity's
  magenta missing-shader color. Fixed by switching to `"HDRP/Unlit"` +
  `_UnlitColor` + `HDMaterial.ValidateMaterial()` — the exact pattern
  `AngleActionStage.cs`'s `flashMat` already uses. Worth a note for future
  Atlas work: treat "Atlas is URP" claims in older comments/memory as
  unverified until checked against `GraphicsSettings.asset` or an actual
  render.
- **Town-block massing read as one solid mass**: the 8 traced block-
  cluster quads share edges by construction (each cluster's boundary is
  the sheet's own street-grid line, which is also the next cluster's
  boundary), so the first render painted one contiguous gray shape with no
  visible street gaps — not legible as "blocks." Fixed with
  `MapFurnitureMeshBuilder.InsetTowardCentroid` (15 m radial inset, capped
  at 40% of each vertex's own distance to its polygon's centroid so a
  small block can't invert): the traced DATA stays the honest full-extent
  quad (to the street centerline — the standard block-boundary
  convention), the RENDERED footprint pulls in enough to show a street gap
  between neighbors. See the after-state captures for the corrected read.

## Declutter interplay

Map furniture must render UNDER unit ribbons and labels
(`docs/reconstruction/cartography-slice.md`'s grammar). Rather than a
render-queue trick, this rides the SAME mechanism `SymbolMeshBuilder.
DefaultLiftM` already established: every furniture class drapes at a
fixed height strictly below the unit-symbol drape height (0.4 m), in a
documented painter's order —

```
TownBlockLiftM (0.05) < StreamLiftM (0.08) < RailLiftM (0.11)
    < RoadLiftM (0.12) < SymbolMeshBuilder.DefaultLiftM (0.4)
```

— fills lowest, then hydrology, then rail, then roads, then units/labels.
Ordinary depth testing keeps the layers in the correct visual order at
every altitude without touching render queues or the label declutter
system at all (roads/streams are not labels — `LabelLayout`'s screen-rect
collision estimate never sees them). `ReliefContourToggle`'s two terrain
variants are both untouched by this layer (it lives above the terrain
mesh, not in a terrain layer).

## Perf evidence

`docs/benchmarks/captures/map-furniture/{before,after}-benchmark.json`
(standard `-benchmark` sample, t=0/8160/8700/9000, same Development
standalone build shape as every prior slice's evidence):

| t | before FPS | after FPS |
|---|---|---|
| 0 | 59.1 | 59.5 (rerun; an isolated 44.3/1.3s-hitch sample on the machine was reproduced-clean on rerun — `after-rerun-benchmark.json`) |
| 8160 | 59.6 | 59.7 |
| 8700 | 59.4 | 59.7 |
| 9000 | 58.8 | 59.7 |

At/above the ~59.5 FPS floor at every timestamp, before and after — the
~5-7k total vertices across all four map-furniture meshes (roughly 70 km
of traced line length at a 30 m adaptive-resample cap, plus 8 small town-
block quads) is a rounding error next to the unit-ribbon budget
(`SymbolMeshBuilder` alone budgets up to 1024 verts x 190 units). No
batching/simplification was needed.

Screenshots: `{before,after}-{theater,mid,tactical,rts-low}.png` +
the standard `{before,after}-t{0,8160,8700,9000}.png` battery
(`MapFurnitureCaptureHarness` / `-mapfurnshots`, poses centered on the
Diamond — see the harness's module comment for the exact pose table).

## Suite numbers

| Suite | Floor | This slice |
|---|---|---|
| tool vitest | 119 | untouched (no `tool/` changes) |
| pipeline pytest | 59 | 59 passed (unchanged — no `pipeline/` changes) |
| reconstruction pytest | 128+1 | **145 passed, 1 skipped** (+17 new: `reconstruction/tests/test_map_furniture.py`) |
| Unity EditMode | 405+4 | **429 passed, 4 skipped, 0 failed** (433 total; +24 new: `MapFurnitureDataTests.cs` (5), `MapFurnitureMeshBuilderTests.cs` (19)) |
| Unity PlayMode | 21 | 21 total, 10 passed, 0 failed, 11 skipped (unchanged — no new PlayMode tests; this slice's runtime surface is exercised by EditMode + the capture harness) |

## Invariants held

- `git diff <branch-point c6f5166> -- app/Assets/Battle/` — empty
  (verified; NOTE: `origin/main` advanced past this branch's base during
  the session — a concurrent `july3-morning` slice merged — so diffing
  against the live `origin/main` ref shows unrelated churn. The fixed
  branch-point SHA is the correct comparison and is clean).
- `stagingSeed` pin `d470c469...` — held (Battle/ untouched, so the pin is
  unaffected; grep-verified present in `app/Assets/Battle/Angle/
  angle.bundle.json`).
- No unmanifested third-party assets: no files added under
  `app/Assets/ThirdParty/`; the 5 new material assets reference Unity's
  built-in HDRP Unlit shader (not third-party content), created
  programmatically by `MapFurnitureImporter` (no runtime material
  creation at PLAY time — the assets are baked at import time, same rule
  the cartography slice held for `UnitLabelInk.mat`/`UnitSymbol.mat`).

## Residuals for a future map-furniture round

- **York Road's own split** was not separately legible from Hanover
  Road's corridor at this reading scale near town; the two are traced as
  one corridor labeled `road-hanover` (the sheet's own "HANOVER" label).
  A tighter crop at the actual fork (a few hundred meters out) would
  resolve it if the owner wants both roads distinct.
- **Plum Run** is traced at coarser point density — dense contour/hachure
  ink over the Devil's Den/Wheatfield sector made the creek's centerline
  harder to isolate confidently at the crop scales used here.
- **Marsh Creek's mill-pond** (a distinct oxbow/pond shape drawn at the
  Black Horse Tavern crossing) was not traced as its own polygon — the
  task mentions "ponds where drawn" as a nice-to-have; this one specific
  pond is a candidate for a follow-up trace.
- **Farm lanes**: only Codori's and Bryan's are traced (the two the task
  calls out by name via their tactical significance), both `inferred`
  confidence (one end anchored at a manifest tie point, the other end
  snapped toward the nearest traced road rather than independently
  paced). Other lanes the fighting turned on (Bliss farm, Trostle farm,
  Sherfy) are not yet traced — a natural extension.
- **Town massing** is 8 block-cluster quads, not per-block footprints (see
  "Town massing simplification" above) — a deliberate reading-time
  tradeoff, flagged for the owner's visual judgment on the gate.
- **Reading grain**: positions carry roughly 25-60 m of grain depending on
  which crop scale read them (documented per-crop in the generator's
  header comment), on top of the sheet's own 31 m rms absolute
  uncertainty — appropriate for a cartographic furniture layer, not
  survey-grade GIS.

## Owner questions

1. Is the 8-cluster town-block massing enough, or should a follow-up trace
   closer-to-source per-block footprints (more polygons, tighter to the
   sheet's actual street lines)?
2. Worth a dedicated pass to split York Road from the merged Hanover
   corridor?
3. Any interest in the Marsh Creek mill-pond as its own polygon, or
   additional farm lanes (Bliss, Trostle, Sherfy)?
