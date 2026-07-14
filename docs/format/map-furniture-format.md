# Map Furniture Format

The contract between the tracing generator (`reconstruction/scripts/
trace_map_furniture.py`) and the Unity importer/runtime (`MapFurnitureImporter`
/ `MapFurnitureField`). JSON, UTF-8. Machine-checkable schema:
[map-furniture.schema.json](map-furniture.schema.json). Sibling to
`landcover-format.md` (same `points`/`kind`/`cls`/`source`/`confidence`
shape) but a **separate file** — `landcover-format.md`'s "Planned
extensions" section explicitly reserved roads/buildings as fact-checked
additions, never traced from copyrighted modern cartography (McElfresh).
This format is that extension, realized: the 1863 road network, hydrology,
and the town of Gettysburg's street-block massing, traced from the
Bachelder timed-set / Warren 1873 engineer base (the same source class as
`landcover.json`'s woodlot/fence tracings — see
`docs/reconstruction/audit/spatial-evidence.md`).

Unlike `landcover.json` (baked into a terrain splatmap — an area/texture
concern), map furniture is a **vector rendering** concern: roads and
streams are drawn as terrain-draped ribbon meshes at runtime
(`MapFurnitureMeshBuilder`, the same drape primitive family as
`SymbolMeshBuilder`), and town blocks as flat-fill footprint meshes. No
rasterization step.

## Conventions

- **Positions** (`points`) are battlefield-local meters, identical frame to
  `landcover.json`/`battle.schema.json`: `x` east, `z` north from the
  terrain's SW corner, valid range `[0, 8507.2]` on both axes. Points
  outside this range (the sheet extends past the heightmap square on three
  sides) are clipped by the generator before commit — there is no terrain
  to drape them on.
- **Feature kinds**: `line` (a road, stream, or railroad — traced as an
  open polyline, `points` in course order) or `polygon` (a town block
  footprint — a closed ring, `points` not repeating the first point).
- **Provenance**: `source` names the sheet and reading method (Bachelder
  Third Day sheet j3-03 / archive.org `12440022.jp2`, Rumsey/Stanford scan
  — see `reconstruction/spatial/bachelder-manifest.json`); `confidence` is
  `documented` (a direct pixel trace, run through
  `georef_maps.SheetGeoref.to_local`) or `inferred` (a short connector
  point not independently read — e.g. a farm lane's road-end, snapped to
  the nearest traced road rather than separately picked). `note` carries
  reading caveats (which named road two drawn corridors were merged into,
  fence-legend cross-reads, etc.).

## Structure

| Field | Type | Rules |
|---|---|---|
| `name` | string | required |
| `features[]` | array | required |
| `features[].id` | string | required, unique |
| `features[].kind` | string | `line` \| `polygon` |
| `features[].cls` | string | vocabulary depends on `kind` (see below) |
| `features[].points[]` | array of `[x, z]` pairs | line: ≥ 2 points; polygon: ≥ 3 points; each coordinate in `[0, 8507.2]` |
| `features[].source` | string | optional; REQUIRED non-empty when `confidence == "documented"` |
| `features[].confidence` | string | required; `documented` \| `inferred` |
| `features[].note` | string | optional |

### Class vocabularies

- `kind == "line"` → `cls` is one of:
  - Road classes (line-weight grammar: pike widest, lane narrowest):
    `pike` (macadamized turnpike — Chambersburg, Baltimore), `road`
    (public/county road), `lane` (farm lane).
  - Stream classes: `creek` (named "Creek" — Rock Creek, Marsh Creek,
    wider draped ribbon), `run` (named "Run" — Willoughby, Plum, Pitzer's,
    Stevens, Winebrenner's, narrower ribbon).
  - Rail classes: `rail_finished` (in service at the time of the battle —
    the Gettysburg & Hanover branch), `rail_unfinished` (graded but
    unrailed — the cut west of town the First Day fighting turned on;
    dashed ink, distinct from a completed line).
- `kind == "polygon"` → `cls` is `town_block` (a street-block's footprint —
  massing, not individual buildings; `docs/reconstruction/map-furniture-
  slice.md` documents the block-cluster simplification used to trace
  Gettysburg's built-up grid at this reading scale).

## The no-faking gate

Same rule as `landcover.json`/the battle track: a feature claiming
`documented` confidence must carry a non-empty `source`. Every road,
stream, rail, and block answers "says who" — `validate_map_furniture.py`
enforces it (schema + the source-required-when-documented rule).

## Generation (not hand-authored)

`landcover.json` was traced through the browser authoring tool by hand;
`map-furniture.json` is generated deterministically by
`reconstruction/scripts/trace_map_furniture.py`: a committed table of
full-resolution sheet-pixel picks (read from `crop_sheet.py` crops of
`data/maps/bachelder/12440022.jp2`, dossier-pass reading discipline) is run
through `georef_maps.load_manifest()['j3-03'].to_local()`, rounded to
0.1 m, clipped to the valid square, and serialized with stable key/point
order — the same inputs always produce the same output bytes (checked by
`reconstruction/tests/test_map_furniture.py::test_determinism`).

## Reading-grain disclosure

Sheet j3-03's absolute georeference is 31.0 m rms (`bachelder-manifest.
json`); pixel picks were read from crops at a scale where the resulting
positional grain sits around 25-60 m depending on crop scale factor
(`reconstruction/scripts/trace_map_furniture.py`'s `SHEET_PX` table
comment header records each source crop's scale). This is a cartographic
furniture layer, not a survey: road/stream courses are traced at the
sheet's own schematic fidelity (junctions and named-road bearings are
reliable; sub-50 m meanders are not), consistent with the map grammar's
"muted, documentary" register — see `docs/reconstruction/map-furniture-
slice.md`.
