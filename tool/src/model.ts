// Mirrors docs/format/battle-format.md and the Unity app's BattleData/UnitTrack.
// The interpolation here MUST match UnitTrack.StateAt — the authoring preview
// must show authors exactly what the app will show viewers.

export type Side = "union" | "confederate";
export type Formation = "column" | "line" | "skirmish" | "scattered" | "routed";
export type Confidence = "documented" | "inferred" | "unknown";

export interface Keyframe {
  t: number;
  x: number;
  z: number;
  facing: number;
  formation: Formation;
  strength: number;
  confidence?: Confidence;
  citation?: string;
}

export interface Unit {
  id: string;
  name: string;
  side: Side;
  frontage_m: number;
  depth_m: number;
  regiments?: string[];
  parent?: string; // id of an existing depth-0 unit; see battle-format.md "Parent / children"
  keyframes: Keyframe[];
}

export type EventKind = "artillery_fire" | "musketry";

// Engagement event: a provenance-gated fire window (battle-format.md
// "Engagement events"). Exactly one emitter form: `unitId` (moving emitter,
// position read from that unit's track at emission time) XOR the x/z/x2/z2
// fixed segment (gun lines not authored as units). Dust is never authored —
// it derives from unit velocity (no `advance_dust` kind, by design).
export interface EngagementEvent {
  id: string;
  kind: EventKind;
  t0: number;
  t1: number;
  unitId?: string;
  x?: number;
  z?: number;
  x2?: number;
  z2?: number;
  confidence?: Confidence;
  citation?: string;
  note?: string;
}

// Wind (battle-format.md "Environment"): `windTowardDeg` is the compass
// bearing smoke drifts TOWARD (not the meteorological from-direction);
// windMps 0 = calm = no drift.
export interface Environment {
  windTowardDeg: number;
  windMps: number;
  confidence?: Confidence;
  citation?: string;
  note?: string;
}

export interface Battle {
  name: string;
  startTime: number;
  endTime: number;
  units: Unit[];
  events?: EngagementEvent[];
  environment?: Environment;
}

export interface UnitState {
  x: number;
  z: number;
  facing: number;
  strength: number;
  formation: Formation;
}

const lerp = (a: number, b: number, u: number) => a + (b - a) * u;

function lerpAngle(a: number, b: number, u: number): number {
  const delta = ((b - a + 540) % 360) - 180; // shortest arc, like Mathf.LerpAngle
  return a + delta * u;
}

export function stateAt(unit: Unit, t: number): UnitState {
  const kfs = unit.keyframes;
  const first = kfs[0]!;
  const last = kfs[kfs.length - 1]!;
  if (t <= first.t) return fromKeyframe(first);
  if (t >= last.t) return fromKeyframe(last);

  // first index with kf.t > t (binary upper bound, same as UnitTrack)
  let lo = 0;
  let hi = kfs.length;
  while (lo < hi) {
    const mid = (lo + hi) >> 1;
    if (kfs[mid]!.t <= t) lo = mid + 1;
    else hi = mid;
  }
  const a = kfs[lo - 1]!;
  const b = kfs[lo]!;
  const u = (t - a.t) / (b.t - a.t);
  return {
    x: lerp(a.x, b.x, u),
    z: lerp(a.z, b.z, u),
    facing: lerpAngle(a.facing, b.facing, u),
    strength: lerp(a.strength, b.strength, u),
    formation: a.formation,
  };
}

function fromKeyframe(k: Keyframe): UnitState {
  return { x: k.x, z: k.z, facing: k.facing, strength: k.strength, formation: k.formation };
}
