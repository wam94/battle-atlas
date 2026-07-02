// Mirrors docs/format/landcover-format.md. Separate file/lifecycle from
// battle.json (model.ts/io.ts/validate.ts) — same code shape, deliberately.

import Ajv from "ajv";
import schema from "../../docs/format/landcover.schema.json";

export type FeatureKind = "polygon" | "line";
export type PolygonClass = "woodlot" | "orchard" | "field" | "pasture" | "marsh";
export type LineClass = "stone_wall" | "rail_fence";
export type LandcoverClass = PolygonClass | LineClass;
export type LandcoverConfidence = "documented" | "inferred";

export interface LandcoverFeature {
  id: string;
  kind: FeatureKind;
  cls: LandcoverClass;
  points: [number, number][];
  source?: string;
  confidence: LandcoverConfidence;
  note?: string;
}

export interface Landcover {
  name: string;
  features: LandcoverFeature[];
}

export interface ValidationResult {
  ok: boolean;
  errors: string[];
}

const POLYGON_CLASSES: readonly PolygonClass[] = [
  "woodlot",
  "orchard",
  "field",
  "pasture",
  "marsh",
];
const LINE_CLASSES: readonly LineClass[] = ["stone_wall", "rail_fence"];

// Battlefield square side length (docs/research/2026-06-13-landmark-anchors.md);
// valid range for both axes. Also encoded in the schema (belt-and-suspenders).
const BOUND_MIN = 0;
const BOUND_MAX = 8507.2;

const ajv = new Ajv({ allErrors: true, allowUnionTypes: true });
// Ajv's compile is generic: the returned validator is a `data is Landcover`
// type guard, checked at runtime against the schema (Landcover stays the
// source of truth for the tool's TS code; the schema is the cross-language
// contract), same pattern as validate.ts.
const schemaValidate = ajv.compile<Landcover>(schema);

// Schema first, then the rules draft-07 can't express (see
// landcover-format.md): unique feature ids, all points within the
// battlefield square, and polygon/line cls cross-checked against kind
// (belt-and-suspenders with the schema's if/then).
export function validateLandcover(data: unknown): ValidationResult {
  const errors: string[] = [];
  if (!schemaValidate(data)) {
    for (const e of schemaValidate.errors ?? [])
      errors.push(`${e.instancePath || "/"} ${e.message ?? "invalid"}`);
    return { ok: false, errors };
  }
  const landcover = data;
  const seenIds = new Set<string>();
  for (const feature of landcover.features) {
    if (seenIds.has(feature.id)) errors.push(`duplicate feature id '${feature.id}'`);
    seenIds.add(feature.id);

    for (const [x, z] of feature.points) {
      if (x < BOUND_MIN || x > BOUND_MAX || z < BOUND_MIN || z > BOUND_MAX)
        errors.push(
          `feature '${feature.id}' point [${x}, ${z}] out of bounds [${BOUND_MIN}, ${BOUND_MAX}]`,
        );
    }

    if (feature.kind === "polygon" && !POLYGON_CLASSES.includes(feature.cls as PolygonClass))
      errors.push(`feature '${feature.id}' kind 'polygon' has non-polygon cls '${feature.cls}'`);
    if (feature.kind === "line" && !LINE_CLASSES.includes(feature.cls as LineClass))
      errors.push(`feature '${feature.id}' kind 'line' has non-line cls '${feature.cls}'`);

    if (feature.confidence === "documented" && !feature.source?.trim())
      errors.push(`feature '${feature.id}': documented confidence requires a source`);
  }
  return { ok: errors.length === 0, errors };
}

export function importLandcover(json: string): Landcover {
  const data = JSON.parse(json);
  const result = validateLandcover(data);
  if (!result.ok) throw new Error(`invalid land cover: ${result.errors.join("; ")}`);
  return data as Landcover;
}

// Canonical key order keeps exports diffable in git, mirroring io.ts.
export function exportLandcover(landcover: Landcover): string {
  const result = validateLandcover(landcover);
  if (!result.ok) throw new Error(`refusing to export: ${result.errors.join("; ")}`);
  const ordered = {
    name: landcover.name,
    features: landcover.features.map((f) => ({
      id: f.id,
      kind: f.kind,
      cls: f.cls,
      points: f.points,
      ...(f.source !== undefined && { source: f.source }),
      confidence: f.confidence,
      ...(f.note !== undefined && { note: f.note }),
    })),
  };
  return JSON.stringify(ordered, null, 2);
}
