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
  };
  return JSON.stringify(ordered, null, 2);
}
