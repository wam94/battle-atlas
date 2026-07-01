import Ajv from "ajv";
import schema from "../../docs/format/battle.schema.json";
import type { Battle } from "./model";

export interface ValidationResult {
  ok: boolean;
  errors: string[];
}

const ajv = new Ajv({ allErrors: true, allowUnionTypes: true });
// Ajv's compile is generic: the returned validator is a `data is Battle` type
// guard, checked at runtime against the schema (Battle stays the source of
// truth for the tool's TS code; the schema is the cross-language contract).
const schemaValidate = ajv.compile<Battle>(schema);

// Schema first, then the rules draft-07 can't express (see battle-format.md):
// strictly increasing t, unique unit ids, endTime covers all keyframes,
// and the no-faking gate (documented => citation).
export function validateBattle(data: unknown): ValidationResult {
  const errors: string[] = [];
  if (!schemaValidate(data)) {
    for (const e of schemaValidate.errors ?? [])
      errors.push(`${e.instancePath || "/"} ${e.message ?? "invalid"}`);
    return { ok: false, errors };
  }
  const battle = data;
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
  }
  return { ok: errors.length === 0, errors };
}
