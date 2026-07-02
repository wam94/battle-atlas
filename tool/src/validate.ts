import Ajv from "ajv";
import schema from "../../docs/format/battle.schema.json";
import type { Battle } from "./model";

// Recorded choice (plan Task A1): advisory findings ride a separate `warnings`
// array on the existing result shape — they never affect `ok`, so import/export
// gates stay error-only while the UI can still surface them.
export interface ValidationResult {
  ok: boolean;
  errors: string[];
  warnings: string[];
}

const ajv = new Ajv({ allErrors: true, allowUnionTypes: true });
// Ajv's compile is generic: the returned validator is a `data is Battle` type
// guard, checked at runtime against the schema (Battle stays the source of
// truth for the tool's TS code; the schema is the cross-language contract).
const schemaValidate = ajv.compile<Battle>(schema);

// Children's t=0 strengths should roughly re-total the parent's whole-brigade
// strength; a sum outside this band suggests a double-count or a lost roster
// entry. Advisory only — known-short decompositions exist (Webb's unmodeled
// 106th PA companies).
const StrengthSumTolerance = 0.15;

// Schema first, then the rules draft-07 can't express (see battle-format.md):
// strictly increasing t, unique unit ids, endTime covers all keyframes,
// the no-faking gate (documented => citation), and the parent/children rules
// (parent exists, depth 1, full decomposition or none).
export function validateBattle(data: unknown): ValidationResult {
  const errors: string[] = [];
  const warnings: string[] = [];
  if (!schemaValidate(data)) {
    for (const e of schemaValidate.errors ?? [])
      errors.push(`${e.instancePath || "/"} ${e.message ?? "invalid"}`);
    return { ok: false, errors, warnings };
  }
  const battle = data;
  const byId = new Map(battle.units.map((u) => [u.id, u]));
  const seenIds = new Set<string>();
  for (const unit of battle.units) {
    if (seenIds.has(unit.id)) errors.push(`duplicate unit id '${unit.id}'`);
    seenIds.add(unit.id);
    for (let i = 1; i < unit.keyframes.length; i++) {
      if (unit.keyframes[i]!.t <= unit.keyframes[i - 1]!.t)
        errors.push(`unit '${unit.id}' keyframe times must strictly increase (index ${i})`);
    }
    const lastT = unit.keyframes[unit.keyframes.length - 1]!.t;
    if (lastT > battle.endTime)
      errors.push(`unit '${unit.id}' keyframe t ${lastT} exceeds endTime ${battle.endTime}`);
    for (const [i, kf] of unit.keyframes.entries()) {
      if (kf.confidence === "documented" && !kf.citation?.trim())
        errors.push(`unit '${unit.id}' keyframe ${i}: documented confidence requires a citation`);
    }
    if (unit.parent !== undefined) {
      const parent = byId.get(unit.parent);
      if (!parent)
        errors.push(`unit '${unit.id}' parent '${unit.parent}' does not exist`);
      else if (parent.parent !== undefined)
        errors.push(
          `unit '${unit.id}' parent '${unit.parent}' has a parent itself (depth 1 only)`);
    }
  }
  // Engagement events (battle-format.md "Engagement events"): unique ids,
  // t0 < t1 <= endTime, unitId references an existing unit (any unit —
  // parent, child, or parentless), the no-faking gate, and exactly one
  // emitter form (belt-and-suspenders: the schema's oneOf already enforces
  // it, but the loader-side rule must have a tool-side twin).
  const seenEventIds = new Set<string>();
  for (const ev of battle.events ?? []) {
    if (seenEventIds.has(ev.id)) errors.push(`duplicate event id '${ev.id}'`);
    seenEventIds.add(ev.id);
    if (ev.t0 >= ev.t1)
      errors.push(`event '${ev.id}' window must satisfy t0 < t1 (${ev.t0} >= ${ev.t1})`);
    if (ev.t1 > battle.endTime)
      errors.push(`event '${ev.id}' t1 ${ev.t1} exceeds endTime ${battle.endTime}`);
    const segmentFields = [ev.x, ev.z, ev.x2, ev.z2].filter((v) => v !== undefined).length;
    if (ev.unitId !== undefined ? segmentFields > 0 : segmentFields !== 4)
      errors.push(
        `event '${ev.id}' must carry exactly one emitter form (unitId XOR x/z/x2/z2)`);
    if (ev.unitId !== undefined && !byId.has(ev.unitId))
      errors.push(`event '${ev.id}' unitId '${ev.unitId}' does not exist`);
    if (ev.confidence === "documented" && !ev.citation?.trim())
      errors.push(`event '${ev.id}': documented confidence requires a citation`);
  }
  // Full decomposition or none, plus the advisory strength re-total — both
  // need the family assembled, so they run after the per-unit pass.
  for (const parent of battle.units) {
    const children = battle.units.filter((u) => u.parent === parent.id);
    if (children.length === 0) continue;
    if (parent.regiments !== undefined)
      errors.push(
        `unit '${parent.id}' has children but still carries a regiments roster ` +
        `(full decomposition or none)`);
    const parentStrength = parent.keyframes[0]!.strength;
    const childSum = children.reduce((sum, c) => sum + c.keyframes[0]!.strength, 0);
    if (Math.abs(childSum - parentStrength) > parentStrength * StrengthSumTolerance)
      warnings.push(
        `unit '${parent.id}' children's t=0 strengths sum to ${childSum}, ` +
        `outside ±15% of the parent's ${parentStrength}`);
  }
  return { ok: errors.length === 0, errors, warnings };
}
