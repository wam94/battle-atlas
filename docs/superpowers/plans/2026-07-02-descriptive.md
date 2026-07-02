# Descriptive Battle Atlas Implementation Plan (Phase 5C)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
> **HARD RULE for every dispatch:** NEVER kill, quit, or terminate the Unity editor or any GUI process. If the Unity project is locked by an open editor, STOP and report BLOCKED. Unity CLI runs require exclusive access — coordinate through the controller.

**Goal:** The graphics go from *indicative* to *descriptive*. Our differentiation against
games like Total War is truth-density: cover and concealment legible to the viewer the way
a soldier read the ground — a swale that hides a regiment, wheat that stops at the waist,
a wall you'd want on your side — AND every element attested. Descriptive never means
invented detail: every new visual is a rendering of the record (the DEM, the traced
landcover, the ephemeris) or is labeled presentation. And the regiment sub-blocks stop
being decoration: regiments become units on their own researched tracks, cited keyframe
by keyframe, per the feasibility survey.

**Research basis:** `docs/research/2026-07-02-regiment-track-sources.md` (what can honestly
be tracked, and at what grade) and `docs/research/2026-07-02-descriptive-graphics-techniques.md`
(what each visual element costs on URP/iOS and in what order to build). This plan defers to
both; where it deviates it says so.

**Architecture — two parallel tracks.** They touch disjoint files and can run
concurrently (this project's proven pattern), with a consolidated punchlist at the end:

- **Track A — data fidelity** (`tool/`, `docs/format/`, `app/Assets/Scripts/BattleData.cs`,
  `BattleDirector.cs`, `app/Assets/Battle/gettysburg-july3.json`): regiments as
  first-class units with a `parent` field; authoring in feasibility-grade order.
- **Track B — visual fidelity** (`pipeline/`, `app/Assets/Editor/LandcoverImporter.cs`,
  `VegetationField.cs`, `FenceField.cs`, `InstancedMeshes.cs`, `UnitFormationRenderer.cs`,
  new `SunDirector.cs`/`CropField.cs`): the graphics report's cheapest-biggest-win ladder,
  kept faithfully.

The one shared seam is Unity test-count arithmetic — each track's counts below are
track-local from the shared baseline; the second track to merge rebases its expectations.

**Branches:** `regiment-tracks` (Track A), `descriptive-graphics` (Track B); merge both,
then run the punchlist on the merged result. **Baselines: tool 74, pipeline 29, Unity 65**
(Task B1's culling fix already executed and merged to main before this plan was committed:
`SpatialBatcher` + 5 tests, commit 072aabd — task-local Unity counts below start from 65).

---

## Format decisions locked here (Track A)

Regiments become first-class units via an optional **`parent`** field (the brigade's unit
id) — chosen over a children-aggregate-upward design, and over runtime slot-derivation,
for these reasons:

1. **Scrubbing determinism stays trivial.** Every unit remains a pure interpolation of its
   own keyframes (`UnitTrack.StateAt` / `stateAt` untouched). No cross-unit derivation at
   runtime, no aggregation feedback: state at time T is a function of (unit, T), exactly
   as today. The alternative — brigade tier computed as the hull/centroid of children —
   makes the far tier a derived quantity, deletes 22 authored, cited brigade tracks, and
   invents aggregate geometry (frontage/facing of a hull) that no source attests.
2. **The LOD ladder switches what RENDERS, never what the data IS.** BattleDirector's
   three tiers already work this way; `parent` slots in as a render-suppression
   relationship, not a data relationship.
3. **Provenance needs keyframes to live on.** The spec anticipated runtime
   brigade-slot derivation for untracked regiments; we deliberately deviate: slot-derived
   positions are **baked into child keyframes at authoring time** (marked `inferred`),
   because a runtime-derived position has no keyframe to hang a citation or a confidence
   label on. Truth-density is the point of the phase; the format's per-keyframe
   confidence/citation is the mechanism.

**Rules (schema + code-rules + loader, mirroring the existing belt-and-suspenders):**

- `units[].parent` — optional string; must reference an existing unit id; depth 1 only
  (a parent may not itself have a parent). Draft-07 can't express cross-unit references —
  these are code rules in `validate.ts` and thrown errors in `BattleLoader.Parse`.
- **Full decomposition or none:** a unit with children must NOT carry a `regiments`
  roster (validator error; loader throws). A brigade either keeps its display-only roster
  exactly as today (Lane, Lowrance, Wilcox, Lang) or decomposes completely into child
  units. This kills the residual-slot ambiguity (who renders the un-promoted regiments?
  where does their strength live?) before it exists. Children MAY carry rosters
  (Brockenbrough's two wings each roster their regiment pair).
- **Parent keyframes are unchanged** — they keep whole-brigade strength and remain the
  far-tier record. Children carry per-regiment strengths from
  `docs/research/2026-06-13-oob-strengths.md`. The tool warns (advisory, not error) when
  children's t=0 strengths sum outside ±15% of the parent's (Webb's is expected to be
  short: the 106th PA's two companies stay unmodeled, see Task A7).
- **Rendering contract by tier** (family-atomic, keyed on the PARENT's center distance so
  a family never half-swaps): Block tier (>4 km) — parent renders its block, children
  hidden; Regiments tier (1.5–4 km) — parent hidden, children render as their own block
  markers (a child with a roster may sub-block via the existing `RegimentSlots` path);
  Soldiers tier (<1.5 km) — children render figures. Existing hysteresis bands unchanged.
- **Backward compatibility:** units with no `parent` and no children behave exactly as
  today, all three tiers, roster sub-blocks included. Batteries unchanged. Additive
  optional field; no format version bump.
- **Id convention for new units:** `us-13vt`, `csa-11miss`, `csa-brock-right`, etc.
- **Disagreements per the pass-1 convention:** never a reconciled true time. Competing
  clocks live side by side in citation text; a track picks the primary reading for
  geometry and names the dispute in the citation (e.g. the 72nd PA's two readings; Sawyer's
  "between twelve and one" vs the 1:07 Jacobs anchor). Bachelder-derived positions are
  `documented` positions with `inferred` times — the map gives classes, not clocks.

---

## TRACK A — DATA FIDELITY (regiment tracks)

### Task A1: Format — `parent` field (schema + tool + loader)

**Files:** `docs/format/battle-format.md`, `docs/format/battle.schema.json`,
`tool/src/model.ts`, `tool/src/validate.ts`, `tool/tests/validate.test.ts`,
`app/Assets/Scripts/BattleData.cs`, `app/Assets/Tests/Editor/BattleLoaderTests.cs`

- [ ] Schema: optional `parent` (non-empty string) on unit. Format doc: table row + a
  "Parent / children" section stating the rules above verbatim (reference-validity,
  depth 1, full-decomposition-or-none, rendering contract, backward compat), and update
  the "Regiment rosters" section's "planned extension" sentence — the extension has
  landed, rosters remain the display-LOD path for undecomposed brigades.
- [ ] `tool/src/model.ts`: `parent?: string` on Unit. `tool/src/validate.ts` code rules:
  parent references an existing id; no grandparents; parent-with-children has no
  `regiments`; advisory strength-sum warning (returned separately from errors, or logged —
  match the existing ValidationResult shape, record the choice).
- [ ] `BattleData.cs`: `public string parent;` on UnitDto (JsonUtility leaves it
  null/empty when absent). `BattleLoader.Parse` throws on: unknown parent id, parent that
  itself has a parent, parent-with-children carrying a roster — same loud style as the
  existing keyframe checks.
- [ ] Tests: tool +6 (each rule accepts/rejects; round-trip through io.ts) → **80**;
  Unity +3 loader tests → **63**. `cd tool && npm test` + typecheck; Unity CLI verify
  (BLOCKED if locked). Commit.

### Task A2: Bachelder timed-set acquisition (early; feeds A5/A6)

**Files:** `docs/research/` (new dated note), tool overlay session (no code)

- [ ] Hunt the July 3 main-field sheets of Bachelder's 1880s timed set in David Rumsey's
  digitization of the 1995 Morningside reprint (browse route in the survey §1.2); resolve
  the sheet-slicing conflict as data: Horse Soldier's 4–8 AM / 8–11 AM / **1:00–3:00 PM**
  / 5:00 PM vs the maps-10–12 numbering (likely East Cavalry Field conflation). Fallbacks
  in order: *Bachelder Papers* vol. 3 map supplement; NARA RG 77 originals.
- [ ] Provenance discipline: the 1880s maps are PD; Rumsey's site license (CC BY-NC-SA)
  covers the scans. **Trace positions as facts; never ship or trace scan imagery as
  geometry we redistribute.** Prefer georeferencing the scan in the tool as a working
  overlay (like Warren/Bachelder sheet 3 in Phase 5A Task 7) and authoring keyframes from
  what it attests, cited "Bachelder 1880s timed set, July 3 1:00–3:00 PM sheet (1995
  Morningside reprint)".
- [ ] Deliverable: a short dated research note — which sheets exist digitized, at what
  resolution, what the 1–3 PM sheet actually attests at regiment level for both armies
  (the dealer's "every regiment and battery" claim is unverified until read by eye), and
  the confirmed slicing. **The 3:00–4:00 PM charge window falls in the gap between sheets
  on either numbering — say so in the note; it is this plan's central data risk.**
- [ ] No tests. Commit (research note + any overlay config).

### Task A3: Family LOD in BattleDirector (Unity CLI)

**Files:** `app/Assets/Scripts/BattleDirector.cs`,
`app/Assets/Tests/Editor/BattleDirectorTests.cs`

- [ ] Start: build an id→entry map; attach children lists to parent entries; a UnitEntry
  knows `IsChild` / `HasChildren`. Update: families evaluate the tier ONCE from the
  parent's center (family-atomic swap — per-child distances would tear a family at a
  boundary); the existing hysteresis latch moves to the family. Render per the contract:
  Block → parent block only; Regiments → children as block markers (children with rosters
  reuse the existing `RenderRegiments` path); Soldiers → children's figures. Parentless
  units: untouched code path.
- [ ] Extract the suppression decision as a pure static helper
  (`RendersAtTier(isChild, hasChildren, tier)`) so it's testable without a scene.
- [ ] Zero per-frame allocations: children lists built in Start; no LINQ in Update.
- [ ] Tests: +3 (suppression truth table; family tier uses parent center; roster-less
  child renders monolithic) → **66**. CLI verify (BLOCKED if locked). Commit.

### Task A4: Tool — "decompose brigade" helper

**Files:** `tool/src/decompose.ts` (new), `tool/src/ui/` (one button on the unit panel),
`tool/tests/decompose.test.ts`

- [ ] Pure `decomposeBrigade(battle, unitId, strengths: Map<string, number>)`: for each
  roster entry, emit a child unit (`parent` set, id per the convention, frontage =
  its `RegimentSlots` share of the brigade frontage, per-regiment strength) with
  slot-follow keyframes — one per parent keyframe, position = parent keyframe +
  rotated slot center, formation/facing inherited, **confidence `inferred`**, citation
  "derived from brigade track [parent citation]; slot order [roster citation]". Remove the
  parent's roster. The author then EDITS the generated tracks — the helper is scaffolding
  for honesty, not a generator of truth; nothing it emits is ever marked `documented`.
- [ ] UI: "Decompose into regiments…" button, disabled when no roster; result flows
  through the normal validation gate.
- [ ] Tests: +4 (child count/ids; slot geometry matches `FormationLayout.RegimentSlots`
  convention — mirror the math, it's the same right-to-left contract; everything inferred;
  roster removed and battle re-validates) → **84**. Commit.

### Task A5: Author the A-grades + the two missing units

**Files:** `app/Assets/Battle/gettysburg-july3.json`, `tool/tests/gettysburg.test.ts`

Feasibility grades from the survey §6. Every keyframe cited; Bachelder sheet 3 start
positions `documented` (map citation) with `inferred` times; disagreements carried, never
reconciled. Battle t axis: startTime 46800 = 13:00 LMT, so 15:20 = t 8400.

- [ ] **Stannard (A):** decompose into 13th/14th/16th VT. The 13th and 16th get the
  wheel — "change front forward on first company," advance ~200 yds onto the assault's
  right flank ~15:20–15:30 (Sturtevant 1910 pp. 304–305; 13th VT monument; Benedict vol. 2 —
  extract the passages, survey open item 3); the 16th's reversal against Wilcox/Lang
  ~15:45 (Veazey OR); 14th VT essentially static. The best-documented moving regiments on
  the field go in first.
- [ ] **Webb (A):** 69th PA static at the wall (McDermott 1889); 71st PA two-step (wall →
  partial withdrawal at the breakthrough, Smith's OR report); 72nd PA **contested** —
  track the crest-then-final-advance reading, citation names the monument-litigation
  counter-reading. 106th PA stays unmodeled as a track (vignette anchor, Task A7); Webb's
  parent strength keeps them counted (expected advisory warning).
- [ ] **Hall (A–):** 19th MA, 20th MA, 7th MI, 42nd NY, 59th NY (short frontage — a
  4-company battalion, 182 men). Crisis sequence ~15:20–15:45 well ordered, clocks
  inferred: Hall shifts right toward the copse; the 59th NY's bolt is encodable honestly
  as formation `routed`; the 19th MA/42nd NY plunge (Devereux/Waitt). Melee-zone positions
  carry the widest uncertainty — survey disagreement 9.
- [ ] **8th Ohio — new tracked regiment, no parent** (Carroll's brigade isn't modeled):
  209 men, ~200 yds forward. Three keyframes: Emmitsburg road cut (skirmish) → change of
  front to the fence, firing into the column's left flank → advance that cut off three
  regiments (colors of the 34th NC and 38th VA taken — Sawyer's regimental history,
  VERIFIED full text; OR 27/1:461–62). Keep Sawyer's "between twelve and one" cannonade
  clock as a disagreement in the citation.
- [ ] **Brown's Battery B, 1st RI — the missing 23rd unit:** in line per Bachelder sheet 3
  (`BROWN`), wrecked during the cannonade, withdrawn mid-bombardment — the event Alexander
  keyed Pickett's advance on (Brown 1985 Filson PDF; Stone Sentinels). And fix
  `us-btty-cowan`: add the documented gallop-in replacing Brown (it currently starts in
  place at kf 0).
- [ ] Tests: gettysburg.test.ts +3 (children present with valid parents for the A-grades;
  every `documented` keyframe cited — extend if already pinned; unit count updated) →
  **87**. Tool validation green; commit per brigade or as one reviewed data commit.

### Task A6: Author the B-grade Confederates (documented starts, phased endpoints)

**Files:** `app/Assets/Battle/gettysburg-july3.json`, `tool/tests/gettysburg.test.ts`

- [ ] Decompose, in order: **Armistead** (B+ — 9/14/38/53/57 VA, slot order per Rawley
  Martin; 53rd VA colors over the wall `documented`, ~15:30; 38th VA left-flank endpoint =
  its colors taken by the 8th Ohio), **Fry** (B — all five colors to the works per Fry
  SHSP 7, a rare all-regiment documented endpoint; conflicts with Virginia-centric
  accounts — carry both, disagreement 2), **Davis** (B — 11th Mississippi earns a real
  track: fresh on July 3, farthest in, documented endpoint at the Brian barn wall; 2nd/42nd
  MS + 55th NC ride slot-derived tracks), **Garnett** and **Kemper** (B — start-position
  regiment bars verified legible on sheet 3; endpoints from *Nothing But Glory* /
  Hessler-Motts-Stanley facts, Feist framing: encode "regiment X at landmark Y in phase Z"
  sentences with page citations, NEVER trace their drawn geometry), **Marshall** (B– —
  26th NC pair at the wall on the 12th NJ front).
- [ ] **Brockenbrough decomposes into two WINGS, not four regiments** (the survey's W
  grade — the wing split is attested, the regiment split is not): `csa-brock-right`
  (40th VA + 22nd Bn) and `csa-brock-left` (47th + 55th VA), each with a 2-entry roster,
  breaking ~15:00–15:15 under the 8th Ohio's flank fire (Mayo's OR report; Fry SHSP 7;
  Gottfried GM 23) — formation `routed` before the road.
- [ ] **Lane, Lowrance, Wilcox, Lang stay display-LOD rosters** — unattested per-regiment
  movement; decomposing them would manufacture nine fictional tracks per the survey.
  (Lowrance's one endpoint fact — the 34th NC colors — already lives in the 8th Ohio's
  citation.) The middle-tier sub-blocks these brigades show are labeled by the format doc
  as display convention; that sentence already exists.
- [ ] Movement between documented start and phased endpoint is the A2 gap: interpolation
  `inferred`, times bracketed by step-off ~14:50–15:00 and repulse ~15:30–15:50 literature
  clocks; the ABT 3:00–4:15 frame noted where used (disagreement 7 — never mix clock
  frames silently).
- [ ] Tests: gettysburg.test.ts +1 (B-grade children valid; four display-LOD brigades
  still have rosters and no children) → **88**. Commit per division.

### Task A7: Company fragments — vignette anchors, catalogued, NOT units

**Files:** `docs/research/2026-07-XX-company-vignette-anchors.md` (new)

- [ ] No map or text source places companies systematically in the window (survey §4,
  verified negative) — so companies do NOT become tracked units. Catalog the nine attested
  fragments as **vignette anchors** for the spec's vignette phase, each with citation and
  confidence grade:
  1. 69th PA Cos. I/A/F/D at the Angle — the refused flank, F's dead captain, D's clubbed
     muskets (McDermott 1889, verbatim).
  2. Cushing's section split — 2 serviceable guns run to the wall, 4 wrecks at the crest
     (NPS/Fuger).
  3. Cowan's 5+1 gun split — **five guns SOUTH of the copse, one (Mullaly's) NORTH**
     (Brown 1985; orientation corrected by the survey).
  4. Brown's B as a 4-gun unit; Lt. Milne detached to Cushing's, killed there.
  5. Arnold's left-section gun at the wall, last round double-shotted (timing debated —
     disagreement 4).
  6. 106th PA Cos. A & B on the skirmish line in front of Cowan (Ward 1906 via Filson
     fn. 75) — kept an anchor, not a track, per the company rule.
  7. Vermont wheel mechanics — first company pivots, successive companies wheel in
     (Sturtevant; the maneuver is company-resolution by definition).
  8. 8th Ohio skirmish split — companies unnamed on July 3 PM; model reserve + line,
     no company ids (Sawyer names none).
  9. University Greys (Co. A, 11th Miss) at the Brian wall, ~31 men, 100% casualties.
- [ ] Structural attributes recorded alongside (not anchors): 59th NY and 39th NY are
  four-company battalions (short frontage — already encoded via frontage_m); 14th CT
  Cos. A/B carried Sharps rifles.
- [ ] No tests. Commit.

---

## TRACK B — VISUAL FIDELITY (the graphics report's ladder)

Order per the report's cheapest-biggest-win build order, with the culling bugfix promoted
to first (it is a today-bug, and its before/after measurement anchors the punchlist perf
story). Tasks B3 and the pipeline half of B4 are **pure `pipeline/` TDD — no Unity, can
run any time, even with the editor open**. B1, B2, B5, B6, B7 and the importer half of B4
are **Unity CLI tasks — editor closed, exclusive access, coordinate through the
controller**. B8 is an editor/device session.

### Task B1: Tree/fence culling bugfix — spatial cells + worldBounds ✅ DONE (pre-plan)

**Executed and merged to main before this plan was committed** (commit 072aabd, merge
7a394b9): new pure `SpatialBatcher.Build` (512 m cells, ≤1023 split, member min/max +
mesh-extent margin bounds, deterministic cell ordering) routed through both
`VegetationField` paths and `FenceField`; `RenderParams.worldBounds` set per batch;
`shadowCastingMode = Off` explicit on both fields (the B8 trap, pre-armed). 5 tests
(SpatialBatcherTests.cs), 65/65 EditMode. Batch topology recorded in the commit message
(trees: 52 map-spanning batches → 94 cell batches averaging 282×257 m). Remaining from
this task's original scope: the before/after Frame Debugger / device measurement — rides
the punchlist perf re-check as planned.

### Task B2: Ephemeris sun + reading-light toggle (Unity CLI)

**Files:** `app/Assets/Scripts/SunEphemeris.cs` (new), `app/Assets/Scripts/SunDirector.cs`
(new), HUD wiring, `app/Assets/Tests/Editor/SunEphemerisTests.cs`

- [ ] `SunEphemeris`: the report §0 table (NOAA/Meeus, lat 39.81°N lon 77.23°W,
  1863-07-03, **Local Mean Time** — 1863 predates standard time; same axis as `startTime`,
  which the HUD already converts to wall clock). Pure static
  `SunAngles(secondsSinceMidnight)` lerping between hourly keys: 13:00 → el 69.4°/az
  219.4°; 16:00 → el 37.9°/az 269.7°; clamp outside the table. Light rotation
  `Quaternion.Euler(elevation, azimuth + 180°, 0)`.
- [ ] `SunDirector` (component on the directional light): drives rotation from
  `BattleClock` every frame (two float lerps — free); warms color/drops intensity slightly
  toward 16:00.
- [ ] **Reading-light toggle**: fixed NW raking light (az ~315°, el ~30–35°, the
  cartographic hillshade standard), because the honest 13:00–15:00 sun sits at 49–69° and
  flattens Gettysburg's relief exactly like a noon aerial photo. **Both are
  display-honesty features:** the ephemeris sun IS the record (computed, source logged in
  the research doc); the reading light is presentation and says so on its label — UI chip
  text "Reading light (presentation)" — the same labeled-not-smuggled doctrine as
  `HeightmapImporter.VerticalExaggeration`'s 2.5×.
- [ ] No lightmaps, no probes — a scrubbable sun rules baking out, and nothing in the
  scene is lightmap-eligible anyway.
- [ ] Tests: +3 (table endpoints exact; interpolation between keys; clamp) → **68**
  track-local. CLI verify. Commit.

### Task B3: Pipeline relief bake — AO/sky-view + curvature (+ contour variant) (TDD, no Unity)

**Files:** `pipeline/terrain_pipeline/relief.py` (new), `pipeline/terrain_pipeline/cli.py`,
`pipeline/tests/test_relief.py`

- [ ] From the real DEM (`data/heightmap/heightmap.raw` + metadata; display-exaggeration
  matched at ×2.5 so baked shading agrees with rendered geometry): bake **sky-view
  factor/AO** (how much sky hemisphere each texel sees — swales, the Plum Run valley, and
  reverse slopes darken regardless of sun position; time-invariant, so it composes
  honestly with the moving sun) and **curvature** (Laplacian-of-DEM: concave slightly
  darker/cooler, convex crests slightly lighter/warmer). Combine into ONE modulation
  texture, luminance clamped to **±8–12%** so it reads as ground variation, not paint.
  This is the standard mobile replacement for SSAO — which stays banned (report trap #2).
- [ ] Emit a second variant with **contour lines** baked in (~3 m interval at display
  scale, ~4% darker) — perfectly honest (it IS the DEM) and it quietly labels every swale;
  the toggle wiring lands in B4.
- [ ] Honesty note in the module docstring: every band in this texture is a derivative of
  the measured elevation data; nothing is painted by hand.
- [ ] Orientation contract: row 0 = north, SAME as heightmap/splatmap — test pins it.
  CLI: `relief` subcommand → `data/landcover/relief.png` (+ `relief_contours.png`),
  1024–2048.
- [ ] Tests (TDD, +5): synthetic bowl → center darker than rim; synthetic ridge → crest
  lighter; clamp respected; deterministic byte-identical re-runs; row-0-north pin →
  pipeline **34**. `uv run pytest`. Commit.

### Task B4: Terrain — 4-layer constraint + relief into tints + per-layer detail (pipeline TDD, then Unity CLI)

**Files:** `pipeline/terrain_pipeline/landcover.py`, `pipeline/tests/test_landcover.py`,
`app/Assets/Editor/LandcoverImporter.cs`, `app/Assets/Tests/Editor/SplatmapDecoderTests.cs`,
`docs/format/landcover-format.md`

- [ ] **4 layers, hard constraint** (report trap #1): URP Terrain Lit packs 4 layers per
  pass; a 5th re-rasterizes the ENTIRE terrain and silently disables height-based
  blending. Merge Orchard into Pasture at the splat level — orchards read from tree ROW
  structure (B5), not ground tint; the ground under an 1863 orchard is grass. Pipeline:
  orchard polygons stop painting a channel (still emit trees); channel layout becomes
  R=field, G=woods, B=marsh (record in landcover-format.md + the module docstring).
  Importer: `Layers` drops Orchard. Pipeline +2 tests → **36**; splat decoder tests
  adjusted.
- [ ] Importer applies the B3 relief bake: generate each layer's albedo as tint × relief
  at splat-map resolution with `tileSize` = full terrain extent (keeps the alpha=0
  smoothness trick), so the modulation is spatially anchored with **zero shader work and
  zero runtime cost**. Contour toggle = swap in the `relief_contours` variant textures
  (UI chip next to the reading light).
- [ ] **Recorded decision — the diffuse-slot conflict:** whole-terrain-tiled relief albedo
  and 4–8 m-tiled detail albedo want the same per-layer texture slot. Resolution ladder:
  (1) this task ships relief-in-albedo (the big swale/dead-ground win, most of the phase's
  legibility) plus **per-pixel normal ON** (requires Draw Instanced) so close range gets
  geometric detail from the 4097 heightmap, and **Base Map Distance 800–1500 m**;
  (2) IF ground-type texture at soldier zoom still reads flat after B6's wheat, a minimal
  Terrain Lit variant that multiplies one relief sample over 4–8 m detail layers is the
  single custom-shader buy this phase MAY make — Material asset only (the iOS stripping
  lesson), and it must be device-verified before merge. Do not buy it speculatively.
- [ ] Unity tests: +2 (layer count = 4; generated albedo = tint × relief at a probed
  texel) → **70** track-local. CLI verify. Commit.

### Task B5: Trees — near/far meshes, orchard rows, stone walls (Unity CLI)

**Files:** `app/Assets/Scripts/InstancedMeshes.cs`, `app/Assets/Scripts/VegetationField.cs`,
`app/Assets/Scripts/FenceField.cs`, tests

- [ ] Keep `RenderMeshInstanced` (do NOT migrate to Unity terrain trees — multi-material
  SpeedTree draw calls and engine-internal placement are exactly what `InstancedMeshes`
  avoids). `BuildTreeNear()`: 6-sided trunk + 2–3 squashed canopy blobs (~120–200 verts)
  with a darker understory band — canopy + understory density is the concealment cue.
  Far mesh: the existing box-blob (or a 16-vert 2-blob) — at >1 km a tree is 2–8 px;
  opaque geometry beats billboards on TBDR (no alpha, no sorting; report trap #4 — no
  alpha-tested vegetation anywhere this phase).
- [ ] Tier switch **per B1 spatial cell, not per tree**, with a hysteresis band exactly
  like BattleDirector's latch (~800–1200 m, pop not crossfade — LOD crossfade is a mobile
  anti-pattern, trap #6). Same matrices, two meshes — zero per-frame allocation. Skip
  octahedral impostors entirely (they pay off for 10k-vert trees, not blobs).
- [ ] Orchard-class trees: distinct rounded-lollipop near mesh at `OrchardScale`, brighter
  green — row legibility comes from the pipeline's regular 8 m grid, already baked.
- [ ] Stone walls stop being fence posts: `BuildWallSegment()` — low irregular block strip,
  hash-perturbed top edge (FNV, deterministic) — for `stone_wall` lines; post+rails stays
  for `rail_fence`. The Angle's wall is cover the viewer must read as cover.
- [ ] Per-instance hue/scale variation via 3–4 pre-tinted `MaterialPropertyBlock` buckets,
  hash-assigned (zero shader work).
- [ ] Budget reality (report §2b): after B1's culling a typical view holds well under
  1 M verts — comfortable 60 fps headroom on A17/A18; verify on device at the punchlist.
- [ ] Tests: +4 (near/far mesh vert budgets; cell-tier hysteresis helper; wall segment
  determinism; orchard variant selection) → **74** track-local. CLI verify. Commit.

### Task B6: Wheat ring — instanced crop clumps at soldier zoom (Unity CLI)

**Files:** `app/Assets/Scripts/CropField.cs` (new), `app/Assets/Scripts/InstancedMeshes.cs`,
tests

- [ ] Three-band read per the report: far = the Field layer's golden tint (B4); mid =
  relief bake carries it; near = **opaque crossed-quad clumps** (~0.9 m — July wheat,
  ripe/being cut), instanced on Field-class splat cells inside a **150–300 m camera
  ring only**. Own `RenderMeshInstanced` field cloned from the VegetationField pattern
  (NOT Unity terrain details — placement determinism must be OUR hash, not
  version-drifty engine internals): clump positions FNV-jittered per splat cell, ring
  rebuilt only when the camera crosses a cell boundary into preallocated matrix buffers —
  zero steady-state allocation.
- [ ] Opaque texels only (no cutout silhouette — trap #4); distance fade by scale-to-zero
  at rebuild, not alpha. `shadowCastingMode = Off`. Budget: ~5–15k clumps ≈ 0.5–1.5 ms
  GPU inside the ring, zero outside — respect it, tune density down first if the device
  says otherwise.
- [ ] Waist-high wheat at soldier LOD is concealment the viewer reads the way the 8th
  Ohio's skirmishers did — that's the point of the feature, and it renders only where the
  traced, provenance-carrying splat says Field.
- [ ] Tests: +3 (deterministic clump placement per cell; ring membership on cell crossing;
  batch cap) → **77** track-local. CLI verify. Commit.

### Task B7: Units — pose variants, vertex-color uniforms, flags (Unity CLI)

**Files:** `app/Assets/Scripts/InstancedMeshes.cs`,
`app/Assets/Scripts/UnitFormationRenderer.cs`, flag material/shader asset, tests

- [ ] 2–3 soldier pose meshes (standing/shoulder-arms, advancing lean, kneeling-firing);
  `UnitFormationRenderer` splits figures into per-pose matrix buckets, pose from the
  existing `FormationLayout.Jitter` hash **biased by unit state** — advancing units lean,
  engaged units kneel — turning pose into information, not garnish. Buckets preallocated
  at `MaxFigures` once (house rule); ≤3 draws/unit instead of 1 — nothing on Metal.
- [ ] Vertex-color two-tone (trousers/coat/flesh bands) via a `Color32` channel on the
  soldier mesh × `_BaseColor` — needs a small shader that multiplies vertex color; it
  ships as a **Material asset** (the magenta/stripping lesson).
- [ ] Flags: one ~45-vert quad mesh, ONE `RenderMeshInstanced` across all units;
  vertex-wave in the shader phase-keyed on world position — per-flag desync with zero CPU
  work, and **time-scrub replays the same wave** (determinism even in the cloth). Side
  colors via the existing MPB pattern. With Track A merged, a flag over each regiment
  track is unit identity at exactly the zoom where the new tracks live.
- [ ] Skip skeletal animation entirely — pose-swap + flag motion is 90% of the life at
  ~0% of the cost.
- [ ] Tests: +3 (pose bucket split determinism; bucket sizes sum to figure count; flag
  mesh budget) → **80** track-local. CLI verify. Commit.

### Task B8: Shadows last (editor session + device)

**Files:** URP asset settings, `app/Assets/Scripts/` RenderParams audit

- [ ] Last deliberately: everything above must set its shadow flags first. Main light
  shadows ON with the report's mobile budget verbatim: **2 cascades, 1024–2048, Max
  Distance 150–300 m, soft shadows OFF** — shadows exist only at soldier/fence zoom where
  they anchor figures; at strategic zoom the pass renders ~nothing.
- [ ] Casters: soldiers (and at most near-cell trees) ONLY. Audit every `RenderParams`
  construction site for explicit `shadowCastingMode` (trees/fences/crops/flags Off —
  the B1 trap, re-checked now that shadows are real). Terrain receives, never casts —
  terrain self-shadowing is B3's bake.
- [ ] Before enabling at all: try the cheaper instanced blob-quad under figures; if it
  grounds them acceptably, keep shadow maps off entirely and record the saved 1–2 ms.
- [ ] No new tests; editor screenshots at soldier zoom, on/off comparison. Commit.

---

## Punchlist (after both tracks merge)

**Files:** merged main; device

- [ ] Test-count reconciliation on the merged branch: tool **88**, pipeline **36**, Unity
  **86** (65 + A:6 + B:15) — re-verify at merge, the arithmetic above is track-local.
- [ ] Editor ladder flight: brigade block at 5 km → Stannard resolves into the 13th, 14th,
  and 16th Vermont *on their own tracks* at 2.5 km → soldiers in wheat behind a stone wall
  under the 15:25 sun at 800 m. Scrub the wheel at 15:20–15:30; confirm family-atomic tier
  swaps (no flicker, no half-families) at both boundaries; screenshot set.
- [ ] Device build (iOS) verifies BOTH tracks together. Perf re-check at the Phase 4
  worst case (the Angle at 15:25) with the full stack: B1's tree-culling fix measured
  before/after on device (compare against the numbers recorded in B1's commit), then
  cumulative cost of B4/B5/B6/B7/B8 against 60 fps. Any element over budget reverts to
  its previous rung — every Track B task is independently shippable by design.
- [ ] Provenance spot-audit: pick five new regiment keyframes at random; each must answer
  "says who" from the citation string alone, and the `documented` bars must trace to
  Bachelder sheet 3 or a named text, never to a copyrighted reconstruction's geometry.
- [ ] Tag `phase5c-descriptive`.

## Done =

Scrub to 15:25 and pinch in on Stannard: the 13th Vermont wheels out of the line — a named
unit on its own cited track, not a slot in a box — across ground where the swale that hid
it actually darkens, through wheat that stands waist-high exactly where the traced fields
say wheat stood, under the sun where the ephemeris puts it, toward regiments whose start
positions Bachelder drew and whose collapse the format admits it inferred. The four
brigades nobody can attest stay honest sub-blocks. Every keyframe, every shadow, every
stalk answers "says who" — the data with a citation, the light with a computation, the
presentation with a label.

## Risks

- **The 3–4 PM Bachelder gap is structural:** the timed set brackets the charge
  (1:00–3:00 PM and 5:00 PM sheets) but the charge itself falls between sheets — so the
  regiment positions in the window's most-watched minutes lean on interpolation plus prose
  sources. The confidence system carries this honestly (documented starts, documented
  endpoints, inferred middles); the risk is reviewer expectation, not data integrity —
  say it in the research note and let the ghosting say it in-app.
- **iOS thermal budget:** near tree meshes, the crop ring, pose buckets, and shadows all
  land at the same zoom band. Mitigations: B1 recovers budget first and is measured;
  every Track B rung is independently revertible; the punchlist gates on the Angle-at-15:25
  device case, and sustained-play thermals (not just first-minute fps) get a 10-minute
  soak check there.
- **Scope discipline: 5C is the slice's fidelity, not an engine.** No BatchRendererGroup
  rewrite (noted as a later optimization, not a prerequisite), no horizon-mapping custom
  shader, no Devil's Den boulders (outside the slice's traced zone), no company tracking
  system, no crop typing beyond wheat. The B4 custom-shader fallback is the only
  discretionary buy, and it has an explicit trigger condition.
- **Rumsey license vs PD provenance:** the 1880s maps are PD; the scans carry CC BY-NC-SA.
  We trace positions as facts and never redistribute scan imagery; if that line ever feels
  thin, the NARA RG 77 / reprint routes are the clean fallback (survey open item 1).
- **Family LOD seams:** a violated full-decomposition rule (parent with both roster and
  children) or a strength double-count would lie at some zoom. The validator errors on the
  former; the advisory strength-sum warning catches the latter; the loader throws loudly —
  authored data the renderer can't show honestly must never vanish silently.
- **Copyright discipline under authoring pressure:** the densest Confederate
  regiment-position source (Hessler/Motts/Stanley) is drawn ON our PD base map — the
  temptation to trace it will be constant. Feist framing only: facts with page citations,
  our geometry drawn on Bachelder/Warren. The gettysburg.test.ts citation assertions and
  the punchlist spot-audit are the enforcement.
