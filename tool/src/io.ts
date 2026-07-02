import type { Battle } from "./model";
import { validateBattle } from "./validate";

export function importBattle(json: string): Battle {
  const data = JSON.parse(json);
  const result = validateBattle(data);
  if (!result.ok) throw new Error(`invalid battle: ${result.errors.join("; ")}`);
  return data as Battle;
}

// Canonical key order keeps exports diffable in git and matches the committed
// placeholder asset's shape.
export function exportBattle(battle: Battle): string {
  const result = validateBattle(battle);
  if (!result.ok) throw new Error(`refusing to export: ${result.errors.join("; ")}`);
  const ordered = {
    name: battle.name,
    startTime: battle.startTime,
    endTime: battle.endTime,
    units: battle.units.map((u) => ({
      id: u.id,
      name: u.name,
      side: u.side,
      frontage_m: u.frontage_m,
      depth_m: u.depth_m,
      ...(u.regiments !== undefined && { regiments: u.regiments }),
      ...(u.parent !== undefined && { parent: u.parent }),
      keyframes: u.keyframes.map((k) => ({
        t: k.t,
        x: k.x,
        z: k.z,
        facing: k.facing,
        formation: k.formation,
        strength: k.strength,
        ...(k.confidence !== undefined && { confidence: k.confidence }),
        ...(k.citation !== undefined && { citation: k.citation }),
      })),
    })),
    // events export sorted by t0 then id — canonical order, diffable
    ...(battle.events !== undefined && {
      events: [...battle.events]
        .sort((a, b) => a.t0 - b.t0 || (a.id < b.id ? -1 : a.id > b.id ? 1 : 0))
        .map((e) => ({
          id: e.id,
          kind: e.kind,
          t0: e.t0,
          t1: e.t1,
          ...(e.unitId !== undefined && { unitId: e.unitId }),
          ...(e.x !== undefined && { x: e.x }),
          ...(e.z !== undefined && { z: e.z }),
          ...(e.x2 !== undefined && { x2: e.x2 }),
          ...(e.z2 !== undefined && { z2: e.z2 }),
          ...(e.confidence !== undefined && { confidence: e.confidence }),
          ...(e.citation !== undefined && { citation: e.citation }),
          ...(e.note !== undefined && { note: e.note }),
        })),
    }),
    ...(battle.environment !== undefined && {
      environment: {
        windTowardDeg: battle.environment.windTowardDeg,
        windMps: battle.environment.windMps,
        ...(battle.environment.confidence !== undefined &&
          { confidence: battle.environment.confidence }),
        ...(battle.environment.citation !== undefined &&
          { citation: battle.environment.citation }),
        ...(battle.environment.note !== undefined && { note: battle.environment.note }),
      },
    }),
  };
  return JSON.stringify(ordered, null, 2);
}
