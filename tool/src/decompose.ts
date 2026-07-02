import type { Battle, Keyframe, Unit } from "./model";

// Decompose a rostered brigade into first-class child regiment units
// (plan Task A4). The output is SCAFFOLDING for honesty, not truth: every
// generated keyframe is `inferred` slot-follow geometry that the author must
// then edit against sources. Nothing emitted here may ever be upgraded to
// `documented` without a citation of its own.

// Interval between regiment sub-blocks, meters — MUST match
// FormationLayout.RegimentGap (app/Assets/Scripts/FormationLayout.cs).
const RegimentGap = 6;

export interface Slot {
  center: { x: number; y: number };
  size: { x: number; y: number };
}

// TS mirror of FormationLayout.RegimentSlots — the same right-to-left
// contract, in the unit-local frame (x along frontage, right positive; y
// along depth, forward positive). Line: blocks side by side along the
// frontage, slot 0 rightmost (+x). Column: blocks stack front-to-back in the
// column footprint (frontage/4 wide, depth*4 deep), slot 0 leading.
// Scattered/routed fall through to the line math, matching the C# else
// branch ("line and anything unrecognized").
export function regimentSlots(
  formation: string, count: number, frontage: number, depth: number,
): Slot[] {
  const slots: Slot[] = [];
  if (count <= 0) return slots;
  if (formation === "column") {
    const width = frontage / 4;
    const totalDepth = depth * 4;
    // floor at 0: a deep roster in a shallow footprint yields degenerate
    // (invisible) slots, never negative sizes — same clamp as the C#
    const subDepth = Math.max(0, (totalDepth - RegimentGap * (count - 1)) / count);
    for (let i = 0; i < count; i++) {
      const y = totalDepth / 2 - subDepth / 2 - i * (subDepth + RegimentGap);
      slots.push({ center: { x: 0, y }, size: { x: width, y: subDepth } });
    }
  } else {
    const width = Math.max(0, (frontage - RegimentGap * (count - 1)) / count);
    for (let i = 0; i < count; i++) {
      const x = frontage / 2 - width / 2 - i * (width + RegimentGap);
      slots.push({ center: { x, y: 0 }, size: { x: width, y: depth } });
    }
  }
  return slots;
}

// Facing convention (same as the C#): 0° = north = +z, 90° = east = +x.
// A local offset (lx along frontage-right, ly along depth-forward) rotates to
// world as Unity's Quaternion.Euler(0, facing, 0) does.
function rotateOffset(lx: number, ly: number, facingDeg: number): { dx: number; dz: number } {
  const rad = (facingDeg * Math.PI) / 180;
  const cos = Math.cos(rad);
  const sin = Math.sin(rad);
  return { dx: lx * cos + ly * sin, dz: -lx * sin + ly * cos };
}

// Full state name → the id convention's abbreviation (us-13vt, csa-11miss).
const StateAbbrev: [string, string][] = [
  ["north carolina", "nc"], ["south carolina", "sc"], ["new york", "ny"],
  ["new jersey", "nj"], ["rhode island", "ri"], ["vermont", "vt"],
  ["pennsylvania", "pa"], ["massachusetts", "ma"], ["michigan", "mi"],
  ["virginia", "va"], ["mississippi", "miss"], ["tennessee", "tn"],
  ["alabama", "al"], ["florida", "fl"], ["ohio", "oh"], ["maine", "me"],
  ["minnesota", "mn"], ["connecticut", "ct"], ["delaware", "de"],
  ["georgia", "ga"], ["maryland", "md"],
];

// "13th Vermont" → us-13vt; "11th Mississippi" → csa-11miss;
// "22nd Virginia Battalion" → csa-22va-bn; parentheticals like
// "(4 companies)" are dropped. Unrecognized names fall back to a sanitized
// slug so the helper never silently drops a roster entry.
export function childId(side: Unit["side"], rosterName: string): string {
  const prefix = side === "union" ? "us" : "csa";
  const cleaned = rosterName.replace(/\(.*?\)/g, "").trim().toLowerCase();
  const m = cleaned.match(/^(\d+)(?:st|nd|rd|th)\s+(.+)$/);
  if (m) {
    const num = m[1]!;
    const rest = m[2]!;
    const bn = /\bbattalion\b/.test(rest) ? "-bn" : "";
    for (const [state, abbrev] of StateAbbrev)
      if (rest.startsWith(state)) return `${prefix}-${num}${abbrev}${bn}`;
  }
  return `${prefix}-${cleaned.replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "")}`;
}

// Pure: returns a NEW battle in which `unitId`'s roster is decomposed into
// child units (one per roster entry, inserted right after the parent) and the
// parent's roster is removed. Parent keyframes are untouched — they remain
// the far-tier record. Throws (never partially applies) on anything it can't
// do honestly: unknown unit, no roster, a parent that is itself a child
// (depth 1 only), a missing per-regiment strength, or a generated id that
// already exists.
export function decomposeBrigade(
  battle: Battle,
  unitId: string,
  strengths: Map<string, number>,
  rosterCitation = "parent regiments roster, order as listed",
): Battle {
  const parent = battle.units.find((u) => u.id === unitId);
  if (!parent) throw new Error(`decomposeBrigade: no unit '${unitId}'`);
  if (!parent.regiments || parent.regiments.length === 0)
    throw new Error(`decomposeBrigade: unit '${unitId}' has no regiments roster`);
  if (parent.parent !== undefined)
    throw new Error(`decomposeBrigade: unit '${unitId}' is itself a child (depth 1 only)`);
  if (parent.keyframes.length === 0)
    throw new Error(`decomposeBrigade: unit '${unitId}' has no keyframes to follow`);

  const roster = parent.regiments;
  const n = roster.length;
  const existing = new Set(battle.units.map((u) => u.id));
  // each child's frontage is its RegimentSlots share of the brigade frontage
  const shareWidth = Math.max(0, (parent.frontage_m - RegimentGap * (n - 1)) / n);
  const parentStrength0 = parent.keyframes[0]!.strength;

  const children: Unit[] = roster.map((name, i) => {
    const strength = strengths.get(name);
    if (strength === undefined)
      throw new Error(`decomposeBrigade: no strength provided for '${name}'`);
    const id = childId(parent.side, name);
    if (existing.has(id))
      throw new Error(`decomposeBrigade: generated id '${id}' already exists`);
    existing.add(id);

    // one slot-follow keyframe per parent keyframe: parent position + rotated
    // slot center, formation/facing inherited, strength scaled by the
    // parent's own decline (scaffolding — the author edits from here)
    const keyframes: Keyframe[] = parent.keyframes.map((kf) => {
      const slot = regimentSlots(kf.formation, n, parent.frontage_m, parent.depth_m)[i]!;
      const { dx, dz } = rotateOffset(slot.center.x, slot.center.y, kf.facing);
      return {
        t: kf.t,
        x: Math.round(kf.x + dx),
        z: Math.round(kf.z + dz),
        facing: kf.facing,
        formation: kf.formation,
        strength: Math.round((strength * kf.strength) / parentStrength0),
        confidence: "inferred",
        citation:
          `derived from brigade track [${kf.citation ?? "uncited"}]; ` +
          `slot order [${rosterCitation}]`,
      };
    });

    return {
      id,
      name,
      side: parent.side,
      frontage_m: shareWidth,
      depth_m: parent.depth_m,
      parent: parent.id,
      keyframes,
    };
  });

  const strippedParent: Unit = { ...parent };
  delete strippedParent.regiments;
  const units = battle.units.flatMap((u) =>
    u.id === unitId ? [strippedParent, ...children] : [u]);
  return { ...battle, units };
}
