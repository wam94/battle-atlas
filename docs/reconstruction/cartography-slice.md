# Cartography deep-dive — punchlist slices 1–3 (visual guide)

**Branch:** `cartography-slice`. **Gate:** explicitly visual — the owner
judges the before/after captures. **Owner's words this slice answers:**
"the labels are there but not easy to see. this will need some thinking"
and "i dont see any differentiation among unit types (e.g. cannons)"
(Gate P11, reaffirmed reviewing wave A1).

**Evidence:** `docs/benchmarks/captures/cartography/` —
`{before,after}-{default,theater,mid,tactical}-t{5400,8400}.png`, same
camera poses both builds (`scripts/cartography-captures.sh`; the
`default` pose is the committed scene camera — the angle the owner's
P11/wave reviews actually saw). Perf: `{before,after}-benchmark.json`.

## How to read the map now

| You are at | You see | You read |
|---|---|---|
| Theater (whole field) | a dozen CORPS words + ribbons | who holds what ground |
| Mid (corridor) | DIVISION words + brigade names | the command grain in motion |
| Tactical (close) | brigades, batteries, regiments | formations, guns, facing |

## Slice 1 — label legibility

- **Altitude → grain policy** (`LabelLayout.BandFor/LabelsAtBand`):
  which NAMES the map offers is a function of camera height above the
  displayed terrain, independent of the mesh LOD. Theater (> ~2.4 km):
  corps words only. Mid: division words + brigade/park names. Tactical
  (< ~1 km): the close-range tier rule as before. Hysteresis at each
  boundary; a selected unit keeps its name in every band.
- **Corps/division words** come from the audited OOB register's
  `parentChain` (never invented), joined to unit ids by
  `scripts/gen-command-overlay.py` into
  `Resources/Battle/command-overlay.json`. Rendered UPPERCASE at 1.8×/
  1.4× label scale — the cartographic register for command grains —
  anchored at the strength-weighted centroid of their member units, so
  the corps word rides its formations as they move. Union divisions
  with ordinal names are qualified ("1ST DIVISION, I CORPS"); CSA
  divisions keep their proper names ("PICKETT'S DIVISION").
- **Short names on the map, pedigree in the drawer**
  (`LabelLayout.ShortName`): "Riddle's Brigade (1st Bde, 3rd Div, I
  Corps — Col. Gates at the line)" labels as **"Riddle's Brigade"**;
  the full record still appears when the unit is selected and always in
  the provenance drawer. Trailing parentheticals and ", X's Division"
  comma-tails fall away; identity-bearing mid-name parentheticals stay.
- **Halo contrast** (`Resources/UI/UnitLabelInk.mat`): one shared TMP
  material asset (no runtime materials) with a dark warm outline
  (0.22 width) under every label; label ink is the side color lifted
  42% toward paper white (`LabelLayout.InkTint`) — hue stays the
  side's, contrast comes from the halo, readable on every terrain
  color including the olive fields that swallowed the P11 labels.
- **Declutter really drops losers**: the screen-rect estimate widened
  (10 px/glyph × 24 px line) so accepted labels stop brushing; corps/
  division words claim rects scaled by their multiplier. Priority
  ladder: selected > corps/division aggregates > active brigades >
  inactive brigades > batteries/regiments (deterministic FNV
  tie-breaks, sticky hysteresis unchanged).
- **Leader lines**: considered, rejected this round — with band gating
  + short names the collision rate at every preset dropped to where
  leader lines would add ink without adding reads (and they fight the
  billboarded world-space TMP idiom). Revisit if the owner wants
  denser tactical labeling.

## Slice 2 — unit-type differentiation

- **Artillery prints dark** (`UnitSymbol.FillInk`, shader
  `_FillInkMul` 0.45): the period-map convention — batteries drawn in
  black ink — made scale-free. At theater zoom a 9 m gun-dot is one
  pixel; VALUE contrast is what survives that, not shape. Up close the
  dots stay recognizably side-hued (darkened, not replaced), and the
  side-colored baseline stroke under the dot row carries side identity
  at every distance. Gun-dot size also raised 6 → 9 m (was subpixel
  before the whole field fit the frame).
- **Gun-count dot row stays the battery glyph** (chosen over tick
  marks at the P11 slice and kept): dots ARE the strength encoding
  (~20 effectives/gun, 2–8 dots), one baseline for a battery, two for
  a battalion — a dot-bar reads as "guns in battery" at tactical zoom
  and as a dark bead-row at theater zoom.
- **Cavalry keeps its structural cut**: the 30° sheared parallelogram
  plus center slash (the map-symbol slash made geometry). Only two
  cavalry units stand on the July 3 square; the cut reads at mid zoom
  and the slice adds nothing that could be confused with the
  provenance hatch.
- **HQ/command standard**: deferred — the current 190-unit cast
  carries no HQ records (`docs/reconstruction/audit/oob-register.json`
  has them as not-yet-cast). The glyph slot is reserved; adding it
  without cast members would be dead code.
- **Provenance ghosting survives**: hatched = inferred, solid =
  documented, unchanged; the ink multiplier composes with both (a
  hatched dark battery reads hatched-dark).

## Slice 3 — movement, orientation, formation

- **Facing chevron** (`SymbolMeshBuilder` ink band, uv.y ≥ 4): a
  draped "Λ" at the symbol's leading edge, inside the footprint
  (extents doctrine holds: a symbol spans exactly frontage ×
  displayed depth). Ordered formations only — scattered/routed men
  assert no facing; the artillery grammar already encodes facing
  (dots forward, baseline rear). Border-shade ink, no echelon weight
  clip, so brigades' double-border rule can't hollow it.
- **Motion trail**: a dashed wake strip from where the unit stood 180
  battle-seconds ago to its current center — moving units grow a tail
  at every zoom, holding units don't. Deterministic in t (track
  states + per-unit hash dashes), no `_Time`; scrubbing replays the
  identical wake. Rebuild honesty: a moving unit already rebuilds its
  ribbon on the position epsilon; the moving-flag flip catches the
  stop so no stale tail lingers.
- **Column formation reads as a column**: the monolithic ribbon now
  adopts the same column footprint the figures and roster slots always
  used (frontage/4 × depth·4) — a marching brigade stops reading as a
  deployed line at every tier. Line/skirmish/scattered/routed grammar
  unchanged (skirmish dotted, dissolving units fragment + dash).
- Pick footprints follow the effective extents, so clicking a column
  hits the column.

## Riders

- **Game-view QHD scaling**: `AtlasPanelSettings` switched
  ConstantPhysicalSize → **ScaleWithScreenSize**, reference 1200×800,
  match = height. The HUD now scales proportionally with window
  height — a simulated-QHD Game view no longer shrinks it (the editor
  reports desktop DPI, which the physical-size mode trusted).
- **Letterbox black backing** (P11 report): while Soldier View owns
  the camera, the camera culls nothing and clears to black — non-16:9
  windows letterbox onto black, not onto live Atlas world. Restored
  exactly on exit. World-space labels were already hidden inside
  Soldier View; they can no longer bleed into the bands by
  construction.

## Constraints held

- Determinism: every new mark is pure in (unitId, t, camera); no
  `_Time`, no `Random`. Scrub-replay pinned by tests.
- No runtime material creation: the label halo and symbol ink are
  material ASSETS; per-unit variation rides MaterialPropertyBlocks.
- Perf: see `{before,after}-benchmark.json` (same playback profile,
  same machine); the label pool, declutter buffers, and aggregate
  accumulators are fixed-capacity, zero steady-state allocation (the
  scale test's ≤1 KB/frame guard still passes).

## Considered and rejected

- **Constant-screen-size symbol glyphs** (billboard icons for
  batteries): breaks the draped-ink language and double-encodes with
  labels; value contrast achieves the read without a second idiom.
- **Synthesizing corps labels from unit-name parsing**: names carry
  corps inconsistently (47/190); the audited register is the honest
  source and already exists.
- **Leader lines** (see slice 1).
- **Per-band font size changes**: scale multipliers on the shared
  material keep one TMP setup and zero re-layout churn.

## Residuals for a future cartography round

- Aggregate label centroids can sit over empty ground between a
  corps' separated wings (III Corps July 3); a split-cluster centroid
  (k-means at k=wings) would fix it if the owner notices.
- The provenance hatch and the artillery dark ink both reduce fill
  luminance; at extreme distance a hatched-inferred battery and a
  documented one converge. Acceptable now (baseline + label
  disambiguate), worth a dedicated pass if inferred batteries become
  common.
- Roster ribbons carry no facing chevron (one arrow per brigade, by
  design) — if the owner wants per-regiment arrows at tactical zoom,
  it is one flag flip.
- Trail length is fixed (180 s); a speed-proportional tail was
  considered and deferred — it reads as velocity but costs a second
  track sample per rebuild.
- Contested styling still dataless (pre-existing punchlist item).
