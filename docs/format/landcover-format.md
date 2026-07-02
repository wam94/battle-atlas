# Land Cover Format

The contract between the authoring tool (writer) and the pipeline (future
reader/writer of the baked splat/tree/fence assets). JSON, UTF-8.
Machine-checkable schema: [landcover.schema.json](landcover.schema.json).
Separate file (`landcover.json`) from the battle track data — different
lifecycle: land cover is traced once per period-map source and revised
occasionally, while the battle track is scrubbed and edited continuously.

## Conventions

- **Positions** (`points`) are battlefield-local meters, identical frame to
  `battle.schema.json`: `x` east, `z` north from the terrain's SW corner,
  valid range `[0, 8507.2]` on both axes (the battlefield square's side
  length; see `docs/research/2026-06-13-landmark-anchors.md`).
- **Feature kinds**: `polygon` (an area — woodlot, orchard, field, pasture,
  marsh) traced as a closed ring, or `line` (a linear feature — stone wall,
  rail fence) traced as an open polyline. `points` is the ring/polyline in
  order; polygons are implicitly closed (do not repeat the first point as the
  last).
- **Provenance**: `source` is free text naming the map a feature was traced
  from (e.g. `"Warren 1868-69, LOC 99448794"`, `"Bachelder sheet 3, LOC
  99447492"`). `confidence` is `documented` (traced from a period-derived
  map — requires `source`) or `inferred` (interpolated/disambiguated between
  sources, or filled in without a direct period trace). `note` is optional
  free text; it carries caveats such as the Warren map's 1868-vs-1863 dating
  gap when a feature is Warren-only.

## Structure

| Field | Type | Rules |
|---|---|---|
| `name` | string | required |
| `features[]` | array | required; may be empty (tracing starts empty) |
| `features[].id` | string | required, unique |
| `features[].kind` | string | `polygon` \| `line` |
| `features[].cls` | string | vocabulary depends on `kind` (see below) |
| `features[].points[]` | array of `[x, z]` pairs | polygon: ≥ 3 points; line: ≥ 2 points; each coordinate in `[0, 8507.2]` |
| `features[].source` | string | optional; REQUIRED non-empty when `confidence == "documented"` |
| `features[].confidence` | string | required; `documented` \| `inferred` |
| `features[].note` | string | optional |

### Class vocabularies

- `kind == "polygon"` → `cls` is one of `woodlot | orchard | field | pasture
  | marsh`.
- `kind == "line"` → `cls` is one of `stone_wall | rail_fence`.

## The no-faking gate

Same rule as the battle track (`battle-format.md`): the authoring tool
refuses to export a feature claiming `documented` confidence without a
`source`. Every polygon and line must be able to answer "says who."

## Baked splat channels (pipeline output contract)

`terrain_pipeline.cli landcover` bakes the polygon features into
`data/landcover/splatmap.png`, a square RGBA PNG (row 0 = north, the same
orientation contract as `heightmap.raw`). Channel layout:

| Channel | Class |
|---|---|
| R | `field` |
| G | `woodlot` |
| B | `marsh` |
| A | unused, always 0 |

`pasture` is the implied base layer: terrain reads as pasture wherever no
channel is painted. `orchard` polygons also paint **no** channel — the
ground under an 1863 orchard is grass, and the orchard read comes from the
baked tree rows (`trees.json`), not ground tint. Merging orchard into the
base keeps the Unity terrain at exactly 4 layers (pasture + 3 channels):
URP's Terrain Lit shader packs 4 layers per pass — a 5th layer
re-rasterizes the entire terrain in an add pass and silently disables
height-based blending (see
`docs/research/2026-07-02-descriptive-graphics-techniques.md` §1a).

## Planned extensions (not yet valid)

- Buildings, roads, and crop typing — fact-checked attributes, never traced
  from copyrighted McElfresh mapping.
