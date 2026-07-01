import { describe, expect, it } from "vitest";
import { stateAt, type Unit } from "../src/model";

function makeUnit(kfs: Array<[number, number, number, number, number]>): Unit {
  return {
    id: "u",
    name: "Test",
    side: "union",
    frontage_m: 100,
    depth_m: 20,
    keyframes: kfs.map(([t, x, z, facing, strength]) => ({
      t, x, z, facing, formation: "line", strength,
    })),
  };
}

describe("stateAt (must match Unity UnitTrack semantics)", () => {
  it("interpolates position and strength linearly", () => {
    const u = makeUnit([[0, 0, 0, 0, 1000], [10, 100, 200, 0, 800]]);
    const s = stateAt(u, 5);
    expect(s.x).toBeCloseTo(50);
    expect(s.z).toBeCloseTo(100);
    expect(s.strength).toBeCloseTo(900);
  });

  it("clamps before first and after last", () => {
    const u = makeUnit([[10, 5, 6, 90, 100], [20, 50, 60, 90, 90]]);
    expect(stateAt(u, 0).x).toBeCloseTo(5);
    expect(stateAt(u, 99).x).toBeCloseTo(50);
  });

  it("interpolates facing across the north wrap", () => {
    const u = makeUnit([[0, 0, 0, 350, 1], [10, 0, 0, 10, 1]]);
    const f = stateAt(u, 5).facing;
    const delta = ((f - 0 + 540) % 360) - 180;
    expect(delta).toBeCloseTo(0);
  });

  it("picks the correct segment among many", () => {
    const u = makeUnit([[0, 0, 0, 0, 1], [10, 10, 0, 0, 1], [20, 30, 0, 0, 1], [40, 70, 0, 0, 1]]);
    expect(stateAt(u, 15).x).toBeCloseTo(20);
    expect(stateAt(u, 30).x).toBeCloseTo(50);
  });

  it("takes formation from the segment start, including exact interior keyframes", () => {
    const u = makeUnit([[0, 0, 0, 0, 1], [10, 10, 0, 0, 1], [20, 30, 0, 0, 1]]);
    u.keyframes[0]!.formation = "column";
    u.keyframes[1]!.formation = "line";
    u.keyframes[2]!.formation = "scattered";
    expect(stateAt(u, 5).formation).toBe("column");
    expect(stateAt(u, 10).formation).toBe("line");
  });
});

describe("stateAt single-keyframe units", () => {
  it("returns the lone keyframe at any time", () => {
    const u = makeUnit([[100, 42, 24, 180, 500]]);
    expect(stateAt(u, 0).x).toBeCloseTo(42);
    expect(stateAt(u, 100).x).toBeCloseTo(42);
    expect(stateAt(u, 9999).z).toBeCloseTo(24);
  });
});
