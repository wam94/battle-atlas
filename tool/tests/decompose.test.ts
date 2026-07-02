import { describe, expect, it } from "vitest";
import { decomposeBrigade } from "../src/decompose";
import { validateBattle } from "../src/validate";
import type { Battle } from "../src/model";

// A worked example small enough to check the slot math by hand: a 3-regiment
// brigade, one line keyframe facing east and one column keyframe facing north.
const fixture = (): Battle => ({
  name: "decompose fixture",
  startTime: 0,
  endTime: 3600,
  units: [
    {
      id: "us-stannard",
      name: "Stannard's Vermont Brigade",
      side: "union",
      frontage_m: 350,
      depth_m: 60,
      regiments: ["13th Vermont", "14th Vermont", "16th Vermont"],
      keyframes: [
        { t: 0, x: 4300, z: 4470, facing: 90, formation: "line", strength: 1950, confidence: "documented", citation: "Bachelder sheet 3" },
        { t: 1200, x: 4200, z: 4500, facing: 0, formation: "column", strength: 1900, confidence: "inferred", citation: "reconstruction" },
      ],
    },
  ],
});

const strengths = new Map([
  ["13th Vermont", 480],
  ["14th Vermont", 647],
  ["16th Vermont", 661],
]);

describe("decomposeBrigade", () => {
  it("emits one child per roster entry: convention ids, parent set, frontage share, per-regiment strength", () => {
    const out = decomposeBrigade(fixture(), "us-stannard", strengths);
    const children = out.units.filter((u) => u.parent === "us-stannard");
    expect(children.map((c) => c.id)).toEqual(["us-13vt", "us-14vt", "us-16vt"]);
    expect(children.map((c) => c.name)).toEqual(["13th Vermont", "14th Vermont", "16th Vermont"]);
    for (const c of children) {
      // RegimentSlots share of the brigade frontage: (350 - 6*2) / 3
      expect(c.frontage_m).toBeCloseTo(338 / 3, 10);
      expect(c.depth_m).toBe(60);
      expect(c.keyframes).toHaveLength(2);
    }
    expect(children.map((c) => c.keyframes[0]!.strength)).toEqual([480, 647, 661]);
    // later keyframes scale with the parent's own decline: 480 * 1900/1950
    expect(children[0]!.keyframes[1]!.strength).toBe(468);
  });

  it("slot-follow geometry mirrors FormationLayout.RegimentSlots (right-to-left, 0°=north=+z, 90°=east=+x)", () => {
    const out = decomposeBrigade(fixture(), "us-stannard", strengths);
    const [vt13, vt14, vt16] = out.units.filter((u) => u.parent === "us-stannard");
    // Line kf facing 90 (east): slot 0 is rightmost (+x local = 118.667 m),
    // and right of an east-facing line is SOUTH (-z). x unchanged.
    expect(vt13!.keyframes[0]).toMatchObject({ x: 4300, z: 4351 }); // 4470 - 118.667
    expect(vt14!.keyframes[0]).toMatchObject({ x: 4300, z: 4470 });
    expect(vt16!.keyframes[0]).toMatchObject({ x: 4300, z: 4589 });
    // Column kf facing 0 (north): slots stack front-to-back along +z, slot 0
    // leading: totalDepth 240, subDepth (240-12)/3 = 76, centers +82 / 0 / -82.
    expect(vt13!.keyframes[1]).toMatchObject({ x: 4200, z: 4582 });
    expect(vt14!.keyframes[1]).toMatchObject({ x: 4200, z: 4500 });
    expect(vt16!.keyframes[1]).toMatchObject({ x: 4200, z: 4418 });
    // formation and facing inherited from the parent keyframes
    expect(vt13!.keyframes[0]!.formation).toBe("line");
    expect(vt13!.keyframes[1]!.formation).toBe("column");
    expect(vt13!.keyframes[1]!.facing).toBe(0);
  });

  it("marks everything inferred with the derived-from citation — scaffolding, never documented", () => {
    const out = decomposeBrigade(fixture(), "us-stannard", strengths, "roster: oob-strengths doc");
    for (const c of out.units.filter((u) => u.parent === "us-stannard"))
      for (const kf of c.keyframes) {
        expect(kf.confidence).toBe("inferred");
        expect(kf.citation).toContain("derived from brigade track [");
        expect(kf.citation).toContain("slot order [roster: oob-strengths doc]");
      }
    // the parent's own keyframes (incl. their documented confidence) are untouched
    const parent = out.units.find((u) => u.id === "us-stannard")!;
    expect(parent.keyframes[0]!.confidence).toBe("documented");
  });

  it("removes the parent roster, leaves the input battle unmutated, and the result re-validates", () => {
    const input = fixture();
    const out = decomposeBrigade(input, "us-stannard", strengths);
    expect(out.units.find((u) => u.id === "us-stannard")!.regiments).toBeUndefined();
    // pure: the input still carries its roster and 1 unit
    expect(input.units).toHaveLength(1);
    expect(input.units[0]!.regiments).toHaveLength(3);
    // the result passes the normal validation gate (no errors; strengths sum
    // 1788 vs parent 1950 is inside the ±15% advisory band, so no warning)
    const result = validateBattle(out);
    expect(result.errors).toEqual([]);
    expect(result.warnings).toEqual([]);
    expect(result.ok).toBe(true);
    // refuses dishonest inputs loudly instead of partially applying
    expect(() => decomposeBrigade(out, "us-stannard", strengths)).toThrow(/no regiments roster/);
    expect(() => decomposeBrigade(input, "us-stannard", new Map())).toThrow(/no strength provided/);
  });
});
