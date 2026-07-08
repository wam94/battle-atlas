// Full-cast generator library (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 1). The position-table-driven builder the authoring waves share: each
// wave script (author-w{N}-*.ts, the author-a5-regiments.ts pattern) holds a
// hand-edited position table whose rows funnel through these helpers into
// units and events, then through the EXISTING gates — `validateBattle` /
// `exportBattle` (tool/src/validate.ts, io.ts). The lib builds structures; it
// does not re-validate. No format surface is added or changed here.
//
// Grade → confidence mapping (survey docs/research/2026-07-02-full-cast-sources.md §3,
// locked in the plan's design decisions):
//   A / A-  → `documented` (position + attested in-window activity), citation required
//   B+ / B  → `documented` (position per tablet; the attested static/negative
//             rides the citation TEXT — the documented-silence convention,
//             battle-format.md "Engagement events"), citation required
//   C       → `inferred` — and the survey's do-not-invent C items are not
//             authored at all; the mapping exists so nothing silently
//             upgrades a C row to documented
import type {
  Battle, Confidence, EngagementEvent, EventKind, Formation, Keyframe, Side, Unit,
} from "../src/model";

export type Grade = "A" | "A-" | "B+" | "B" | "C";

// The July 3 slice window: 13:00–16:00 LMT = t 0..10800 (startTime 46800).
export const WindowEndT = 10800;

export function confidenceForGrade(grade: Grade): Confidence {
  return grade === "C" ? "inferred" : "documented";
}

// Frontage heuristic for units the record doesn't measure — a PRESENTATION
// convention, noted once here, never an attestation claim (plan "Id
// conventions" note): infantry ≈ strength × 0.27 m, consistent with the
// existing authored brigades. Batteries (80–120 m) and battalion gun lines
// (250–500 m, read off sheet 8) don't scale with headcount — pass those
// explicitly via frontage_m.
export function frontageHeuristic(strength: number): number {
  return Math.round(strength * 0.27);
}

// Default block depth when the row doesn't give one — same presentation
// convention as the heuristic frontage (existing brigades run ~20 m).
const DefaultDepthM = 20;

const gateCitation = (what: string, confidence: Confidence, citation?: string) => {
  if (confidence === "documented" && !citation?.trim())
    throw new Error(`${what}: documented confidence requires a citation (no-faking gate)`);
};

export interface StaticRow {
  id: string;
  name: string;
  side: Side;
  strength: number;
  x: number;
  z: number;
  facing: number;
  frontage_m?: number;
  depth_m?: number;
  formation?: Formation;
  grade: Grade;
  citation?: string;
  parent?: string;
  endT?: number; // the window end; override only for non-July-3 fixtures
}

// A position-table row → a 2-keyframe static unit: identical pose at t=0 and
// t=endT (plan: "Static units: two keyframes"). The t=0 citation carries the
// position attestation AND, for present-but-quiet units, the negative
// ("Not actively engaged") — the documented-silence convention.
export function staticUnit(row: StaticRow): Unit {
  const confidence = confidenceForGrade(row.grade);
  gateCitation(`staticUnit '${row.id}'`, confidence, row.citation);
  const pose = {
    x: row.x,
    z: row.z,
    facing: row.facing,
    formation: row.formation ?? ("line" as Formation),
    strength: row.strength,
    confidence,
    ...(row.citation?.trim() && { citation: row.citation }),
  };
  return {
    id: row.id,
    name: row.name,
    side: row.side,
    frontage_m: row.frontage_m ?? frontageHeuristic(row.strength),
    depth_m: row.depth_m ?? DefaultDepthM,
    ...(row.parent !== undefined && { parent: row.parent }),
    keyframes: [
      { t: 0, ...pose },
      { t: row.endT ?? WindowEndT, ...pose },
    ],
  };
}

export interface MoverRow {
  id: string;
  name: string;
  side: Side;
  frontage_m?: number;
  depth_m?: number;
  parent?: string;
  keyframes: Keyframe[];
}

// Movers get their attested keyframes verbatim — each keyframe carries its
// own confidence/citation (per-keyframe grading, exactly like the A5 edit
// tables), so there is no unit-level grade to map. Keyframes pass through
// UNTOUCHED; ordering/coverage errors surface at the validateBattle gate.
export function moverUnit(row: MoverRow): Unit {
  if (row.keyframes.length === 0)
    throw new Error(`moverUnit '${row.id}': at least one keyframe required`);
  return {
    id: row.id,
    name: row.name,
    side: row.side,
    frontage_m: row.frontage_m ?? frontageHeuristic(row.keyframes[0]!.strength),
    depth_m: row.depth_m ?? DefaultDepthM,
    ...(row.parent !== undefined && { parent: row.parent }),
    keyframes: row.keyframes,
  };
}

export interface FireRow {
  id: string;
  kind: EventKind;
  t0: number;
  t1: number;
  unitId?: string; // emitter form A: moving emitter on that unit's track
  segment?: { x: number; z: number; x2: number; z2: number }; // form B: fixed line
  confidence?: Confidence;
  citation?: string;
  note?: string;
}

// A fire window with exactly ONE emitter form (battle-format.md "Engagement
// events"): `unitId` XOR the fixed segment. The segment form survives for
// attested detachments of units authored at coarser grain (Carter's rifles
// at the railroad cut, Raine's 20-pdr section).
export function fireEvent(row: FireRow): EngagementEvent {
  if ((row.unitId !== undefined) === (row.segment !== undefined))
    throw new Error(
      `fireEvent '${row.id}': exactly one emitter form (unitId XOR segment)`);
  if (row.confidence !== undefined)
    gateCitation(`fireEvent '${row.id}'`, row.confidence, row.citation);
  return {
    id: row.id,
    kind: row.kind,
    t0: row.t0,
    t1: row.t1,
    ...(row.unitId !== undefined && { unitId: row.unitId }),
    ...(row.segment !== undefined && {
      x: row.segment.x, z: row.segment.z, x2: row.segment.x2, z2: row.segment.z2,
    }),
    ...(row.confidence !== undefined && { confidence: row.confidence }),
    ...(row.citation !== undefined && { citation: row.citation }),
    ...(row.note !== undefined && { note: row.note }),
  };
}

// Pure: returns a NEW battle with `units` spliced in immediately after
// `anchorId` (the A5 insertAfter pattern, generalized to a batch — wave
// units land grouped with their sector). Throws on a missing anchor or a
// duplicate id rather than deferring to the validator: a wave script must
// fail before it writes anything.
export function addUnitsAfter(battle: Battle, anchorId: string, units: Unit[]): Battle {
  const i = battle.units.findIndex((u) => u.id === anchorId);
  if (i < 0) throw new Error(`addUnitsAfter: anchor '${anchorId}' missing`);
  const existing = new Set(battle.units.map((u) => u.id));
  for (const u of units) {
    if (existing.has(u.id))
      throw new Error(`addUnitsAfter: unit id '${u.id}' already exists`);
    existing.add(u.id);
  }
  return {
    ...battle,
    units: [...battle.units.slice(0, i + 1), ...units, ...battle.units.slice(i + 1)],
  };
}
