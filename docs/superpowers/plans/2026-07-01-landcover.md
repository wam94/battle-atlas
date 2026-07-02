# Land Cover Implementation Plan (Battle Atlas, Phase 5A)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
> **HARD RULE for every dispatch:** NEVER kill, quit, or terminate the Unity editor or any GUI process. If the Unity project is locked by an open editor, STOP and report BLOCKED. Environment premises ("editor closed") are observations that can go stale, not conditions to enforce.

**Goal:** The white battlefield becomes 1863 Pennsylvania: traced woodlots, orchards, fields, and fences from the public-domain Warren/Bachelder maps, baked into terrain splats, instanced trees, and fence lines — with per-polygon provenance.

**Architecture:** Three legs matching the existing system. (1) **Tool**: a land-cover mode — polygon tracing (woodlot/orchard/field/pasture/marsh) and line tracing (stone_wall/rail_fence) over the draped period maps, per-feature provenance, separate `landcover.json` export with its own schema. (2) **Pipeline**: `landcover` CLI subcommand rasterizes polygons into a 4-channel splat PNG (rasterio.features), emits deterministic tree placements per woodlot/orchard polygon and fence-post lines. (3) **Unity**: importer applies splats as TerrainLayers with procedurally generated tint textures; `VegetationField` v2 consumes baked placements; fences render as instanced rails. Research basis: docs/research/2026-07-01-landcover-sources.md (verdict: trace from PD sources; Warren's 1868-vs-1863 caveat carried as provenance).

**Format decisions locked here:**
- New file `landcover.json`, separate from battle data (different lifecycle): `{ name, features: [{ id, kind: "polygon"|"line", cls, points: [[x,z],...], source, confidence, note? }] }` — coordinates battlefield-local meters, same frame as everything.
- Polygon classes: `woodlot | orchard | field | pasture | marsh`. Line classes: `stone_wall | rail_fence`.
- Provenance per feature: `source` (free text naming the map: "Warren 1868-69, LOC 99448794" / "Bachelder sheet 3, LOC 99447492"), `confidence` (`documented` = traced from a period-derived map, requires source; `inferred` = interpolated/disambiguated). Warren-only features carry the 1868 caveat in `note`.
- Splat channels: R=field, G=woods-floor, B=orchard, A=marsh; base weight (pasture/grass) = 1−sum. Tree densities: woodlot 1/(9m)², orchard 1/(8m)² grid-jittered rows.

**Branch:** `landcover`. Tool baseline 41 tests; pipeline 12; Unity 47.

---

### Task 1: Land-cover format doc + schema + tool model/validation (TDD)

**Files:** `docs/format/landcover-format.md`, `docs/format/landcover.schema.json`, `tool/src/landcover.ts`, `tool/tests/landcover.test.ts`

- [ ] Format doc: the structure/vocabulary/provenance rules above, plus "planned extensions: buildings, roads, crop typing (fact-checked attributes, never traced from copyrighted McElfresh)".
- [ ] Schema (draft-07): required name/features; feature requires id/kind/cls/points/confidence; polygon min 3 points, line min 2 (express as if/then on kind); documented⇒source (if/then, mirroring the battle schema's citation gate); cls vocabularies per kind (if/then).
- [ ] `tool/src/landcover.ts`: TS types + `validateLandcover(data)` (ajv against the schema + code rules: unique ids, points in [0,8507], polygon-kind cls must be a polygon class and line-kind cls a line class — belt-and-suspenders with the schema) + `exportLandcover`/`importLandcover` (canonical order, validation-gated, mirroring io.ts).
- [ ] Tests (TDD, ~7): valid fixture accepts; empty features accepts (tracing starts empty); documented-without-source rejects; 2-point polygon rejects; line with polygon class rejects; out-of-bounds point rejects; export refuses invalid + round-trips.
- [ ] `cd tool && npm test` (expect 48) + typecheck. Commit.

### Task 2: Tool land-cover tracing UI

**Files:** `tool/src/ui/landcoverui.ts`, modify `tool/src/ui/workspace.ts` (stable section, like the overlay), `tool/src/style.css`

- [ ] Stable sidebar section "Land cover (trace from draped period maps)": mode toggle button (OFF = normal battle editing; ON = map clicks add vertices instead of keyframes — reuse the tie-point-guard pattern: expose `isTracing()` consulted by the workspace click handler BEFORE the keyframe append, after the tie-point guard).
- [ ] While tracing: class selector (polygon + line classes), source text field (prefilled "Warren 1868-69, LOC 99448794"), confidence selector; clicks append vertices (live polyline preview layer); "close polygon" / "finish line" button commits the feature; Esc cancels the in-progress feature.
- [ ] Feature list with per-feature delete + class/source/confidence editing; features render on the map as translucent fills (class-colored: woods dark green, orchard light green, field wheat-gold, pasture pale green, marsh blue-grey) and fence lines (stone grey / rail brown).
- [ ] Separate autosave slot (`battle-atlas-landcover-autosave`, same persist.ts pattern) + Import/Export buttons using Task 1's io.
- [ ] Pure helpers unit-tested where they exist (feature→GeoJSON builder like pathlayer's: `landcoverToGeoJSON` with class-color properties — 2 tests → 50). Dev-server smoke. Commit.

### Task 3: Pipeline — rasterize splats (TDD)

**Files:** `pipeline/terrain_pipeline/landcover.py`, `pipeline/tests/test_landcover.py`

- [ ] TDD with synthetic fixtures: `rasterize_splats(features, size_m, resolution)` → uint8 RGBA array via `rasterio.features.rasterize` per class (polygon classes → channel weights 255; overlapping polygons: later features win — document). A 4×4 test grid with one field polygon covering the west half: R=255 west, 0 east; base implied. Lines are ignored by the rasterizer (fence test asserts no channel).
- [ ] `write_splatmap(array, path)` → PNG via `rasterio` (or Pillow — add pillow dep if simpler; record choice). Round-trip test.
- [ ] Note orientation: row 0 = north, consistent with the heightmap convention — SAME row-flip contract as heightmap.raw; test pins it (polygon in the north half → first rows).
- [ ] `uv run pytest` (expect 12 + ~4 = 16). Commit.

### Task 4: Pipeline — tree + fence placements (TDD)

**Files:** extend `pipeline/terrain_pipeline/landcover.py`, tests

- [ ] `tree_placements(features)` → list of (x, z, cls): woodlots Poisson-ish via deterministic hash-jittered grid at 1/(9m)² (reuse the FNV-hash-in-python pattern — write `_jitter(feature_id, i, salt)` mirroring the C#/TS approach; cross-language consistency is NOT required here, only determinism); orchards on jittered 8m rows (visibly regular). Point-in-polygon via `shapely` (add dep) or matplotlib.path (prefer shapely, already a rasterio ecosystem citizen — record choice). Tests: counts scale with area, all inside polygon, deterministic, orchard rows more regular than woodlot scatter (row-variance assertion).
- [ ] `fence_posts(features, spacing=3.0)` → resampled points along lines with per-post orientation (bearing of segment). Test: straight 30m line at 3m spacing → 11 posts, bearings constant.
- [ ] CLI: `landcover` subcommand — reads `data/landcover/landcover.json`, writes `data/landcover/splatmap.png`, `trees.json`, `fences.json`. Parser test. `uv run pytest` (expect ~21). Commit.

### Task 5: Unity — terrain layers + splat import

**Files:** modify `app/Assets/Editor/HeightmapImporter.cs` (or a new `LandcoverImporter.cs` menu item — prefer separate), `app/Assets/Scripts/...` as needed. Editor closed; CLI verify.

- [ ] `[MenuItem("BattleAtlas/Import Land Cover")]`: generates 5 TerrainLayer assets under `Assets/Generated/` (pasture base + field/woods/orchard/marsh) with small procedural solid-tint diffuse textures (16×16 Texture2D assets, colors: pasture #8FA05A, field #C9B36A, woods #4C6340, orchard #7DA05A, marsh #6E7F72 — muted, sand-table palette); reads `data/landcover/splatmap.png`, builds the alphamap array (note Unity alphamaps are [y,x,layer] with y=south-first — apply the SAME row flip as the heightmap decoder, comment the contract), `terrainData.terrainLayers = ...; SetAlphamaps(0,0,...)`. Idempotent re-runs (replace layers/assets like the terrain importer does).
- [ ] Contract validation: splat PNG resolution must match or cleanly resample to the terrain's alphamap resolution (set alphamapResolution to the PNG size; validate 2^n).
- [ ] EditMode test for the pure row-flip/weight-normalization helper (extract it static): ~2 tests → 49 Unity. CLI verify. Commit.

### Task 6: Unity — trees + fences from baked data

**Files:** modify `app/Assets/Scripts/VegetationField.cs`, create `app/Assets/Scripts/FenceField.cs`, extend the land-cover import menu

- [ ] VegetationField v2: optional `public TextAsset treesJson;` — when set, groves come from the baked trees.json (grouped into ≤1023-instance batches; orchard trees get a smaller scale variant) instead of the hardcoded three circles (keep those as fallback when unset, marked legacy-indicative).
- [ ] `FenceField.cs`: consumes fences.json; instanced rail segments (reuse InstancedMeshes with a new `BuildFencePost()` ~20-vert mesh: post + two rails oriented per bearing); batches of ≤1023; stone walls use a low grey box variant.
- [ ] Import menu wires the TextAssets onto the components. JSON parsing via existing JsonUtility DTO pattern (small DTO file + 1 loader test → 50-51 Unity tests). CLI verify. Commit.

### Task 7: Trace the real thing (user + agent, in the tool)

- [ ] Agent: drape Bachelder sheet 3 + Warren in the tool (georeference session — can be scripted via the browser extension as in Phase 3 verification), trace the FIRST CUT: the battle-window critical zone (the charge field between Seminary and Cemetery Ridges — Spangler's/McMillan/Codori orchards and woodlots, the Emmitsburg Road fence lines (rail), the stone wall at the Angle, the big field polygons) with provenance per feature. Export `data/landcover/landcover.json`, run the pipeline bake, commit the traced JSON (it's authored source, like the battle file).
- [ ] User: review the tracing against the draped maps in the tool (your Bachelder-vs-Warren judgment calls), refine, re-export.

### Task 8: End-to-end + device

- [ ] Editor session: Import Land Cover; verify the valley reads as farmland — gold fields, fence lines along the Emmitsburg Road, the woods real; screenshots.
- [ ] iOS build; device check INCLUDING the deferred Phase 4 perf checklist (the Angle at 15:25 worst case, now with trees + fences + splats). Tag `phase5a-landcover`.

## Done =

Scrub to 15:10 and Pickett's brigades cross actual fields — between actual fences, past the Codori orchard, toward a stone wall that exists — on ground that finally looks like Pennsylvania in July. Every polygon can answer "says who."

## Risks
- Tracing effort is the long pole — Task 7 scopes to the charge-critical zone first; the full battlefield fills in over later sessions (the tool makes this incremental).
- Terrain alphamap resolution/perf on device: start at 1024², measure.
- Warren-vs-Bachelder disagreements on woodlot edges: provenance notes carry both; render follows the traced choice.
