# Regiment LOD Implementation Plan (Battle Atlas, Phase 5B)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
> **HARD RULE for every dispatch:** NEVER kill, quit, or terminate the Unity editor or any GUI process. If the Unity project is locked by an open editor, STOP and report BLOCKED. Unity CLI runs require exclusive access — coordinate through the controller.

**Goal:** The middle rung of the zoom ladder: between ~4 km and ~1.5 km, a brigade block splits into its named regiment sub-blocks arranged along the frontage — real historical subdivisions from the research corpus, not decoration. (User-specced; spec's original four-altitude ladder.)

**Architecture:** Optional `regiments: string[]` on units in the battle format (schema + C# DTO + TS type, all passthrough). `FormationLayout.RegimentSlots` computes sub-block centers/widths along the frontage (pure, tested). `BattleDirector` gains a three-tier LOD: brigade block (>4 km), regiment sub-blocks (1.5–4 km, instanced box mesh, one RenderMeshInstanced per unit), soldier figures (<1.5 km, unchanged). Units without a `regiments` roster stay monolithic at the middle tier — honesty over uniformity.

**Data:** Rosters from the research corpus with citations: Pickett's three brigades regiment-by-regiment per Rawley Martin (SHSP 32); Pettigrew/Trimble structure per Longstreet pp. 388-389 + OOB doc; Union brigades per the Stone Sentinels-derived rosters in docs/research/2026-06-13-oob-strengths.md (Webb's 69th/71st/72nd PA + 106th PA, Willard's four NY regiments, Smyth's five, Stannard's 13th/14th/16th VT engaged, etc.). Where the strengths doc lacks a full roster, omit the field and note it.

**Branch:** `regiment-lod`. Baselines: Unity 55 (after landcover-polish merges: same 55), tool 61 (or ~67 after tool-followups merges — verify at execution), pipeline 29.

---

### Task 1: Format + data — regiment rosters

**Files:** `docs/format/battle-format.md`, `docs/format/battle.schema.json`, `tool/src/model.ts`, `app/Assets/Scripts/BattleData.cs`, `app/Assets/Battle/gettysburg-july3.json`, `tool/tests/gettysburg.test.ts`

- [ ] Schema: optional `regiments` (array of non-empty strings, minItems 2) on unit. Format doc: one table row + a note ("display subdivision roster; order = right-to-left along the frontage as the unit faces; carries no per-regiment tracking — that's a planned extension").
- [ ] `tool/src/model.ts`: `regiments?: string[]` on Unit. `app/Assets/Scripts/BattleData.cs`: `public List<string> regiments;` on UnitDto (JsonUtility leaves it empty when absent — loader treats null/empty as no-roster; no validation beyond schema).
- [ ] Battle data: add rosters to `gettysburg-july3.json` for every unit the research attests (each unit's roster addition includes the citation inside the existing t=0 keyframe citation or a unit `name`-adjacent note — put it in the t=0 citation string: append "; roster: [source]"). Kemper (1st/3rd/7th/11th/24th VA — Martin), Garnett (8th/18th/19th/28th/56th VA — Martin), Armistead (9th/14th/38th/53rd/57th VA — Martin), Fry/Archer's, Marshall's, Davis's, Lane's, Lowrance's per OOB doc + NPS-derived rosters where the strengths doc records them; Union: Webb, Hall, Harrow, Smyth, Sherrill/Willard's, Stannard (13th/14th/16th VT only — the engaged three) per the strengths doc. Batteries: no rosters (sections would be over-modeling). Brockenbrough/Wilcox/Lang: add only if the strengths doc names regiments; else omit + note.
- [ ] Tests: `tool/tests/gettysburg.test.ts` gains an assertion: every `regiments` array (when present) has ≥2 unique entries; `cd tool && npm test` green; the battle file still validates.
- [ ] Commit.

### Task 2: FormationLayout.RegimentSlots (TDD, Unity CLI)

**Files:** `app/Assets/Scripts/FormationLayout.cs`, `app/Assets/Tests/Editor/FormationLayoutTests.cs`

- [ ] Pure static `RegimentSlots(int count, float frontage, float depth)` → array of (Vector2 center, Vector2 size) in unit-local frame: `count` sub-blocks side by side along the frontage with a 6 m interval gap between them, each width = (frontage − gaps)/count, depth = unit depth; centers ordered +x (right) to −x (left) to match the roster's right-to-left convention. For `column` formation the sub-blocks stack front-to-back instead (same slot math rotated: width = frontage/4, sub-depths along +y). Scattered/routed formations: middle tier renders the monolithic block (slots not used — document).
- [ ] Tests (~4): 5 slots span the frontage with gaps; widths equal; right-to-left ordering (slot 0 center.x > slot 4 center.x); column stacking; count 1 degenerates to the full block.
- [ ] CLI verify (BLOCKED if locked): expect 55 + 4 = 59. Commit.

### Task 3: Middle tier in BattleDirector (Unity CLI)

**Files:** `app/Assets/Scripts/BattleDirector.cs`, `app/Assets/Scripts/InstancedMeshes.cs` (+1 test)

- [ ] `InstancedMeshes.BuildUnitBox()` — unit cube (1×1×1 centered at y +0.5) for instanced sub-blocks scaled per-slot via TRS; 1 mesh test.
- [ ] BattleDirector: thresholds — figures <1500 (unchanged, hysteresis 1650), regiment tier 1500–4000 (hysteresis 4400), brigade block >4000. Per unit per frame at the middle tier: if roster present and formation is line/column: hide the marker GameObject, build TRS matrices from RegimentSlots (position = unit world pos + rotated slot center; scale = (slot.width, blockHeight, slot.depth) with the existing min-to-max footprint height logic reused per sub-block center — simplify: sample height at each slot center, sub-block height = MarkerHeight + local relief like the monolithic path but per slot), one RenderMeshInstanced with the unit's side color MPB; else (no roster / scattered / routed) show the monolithic block exactly as today.
- [ ] Keep per-frame allocations zero: persistent per-unit Matrix4x4[16] buffer (max regiments realistically ≤10).
- [ ] CLI verify: 60 tests. Commit.

### Task 4: Verification + punchlist entry

- [ ] Editor session (user or MCP): fly the ladder — brigade block at 5 km, five named regiment blocks at 2.5 km, soldiers at 1 km; confirm no flicker at both boundaries; screenshot.
- [ ] Rides into the consolidated punchlist: device build verifies Phases 4 + 5A + 5B together; tag `phase5b-regiment-lod` after device check.

## Done =

The zoom ladder gets its missing rung: pinch in on Garnett's brigade and it resolves into the 8th, 18th, 19th, 28th, and 56th Virginia — named, ordered, and cited — before dissolving into men.

## Risks
- Roster order (right-to-left) is attested for Pickett's division (Martin); other brigades' internal regiment ordering is mostly NOT documented — the format note says order is display convention, and citations mark ordering as inferred where unattested.
- Slot heights per sub-block add ~5 SampleHeight calls/unit/frame at middle tier — negligible.
