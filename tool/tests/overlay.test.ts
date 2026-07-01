import { describe, expect, it } from "vitest";
import { similarityFromTwoPoints, applySimilarity } from "../src/overlay";

describe("two-point similarity transform (scanned map georeferencing)", () => {
  // image pixel coords (y down) -> battlefield-local meters (z north/up)
  it("recovers scale, rotation, and translation from two tie points", () => {
    // synthetic: image is 2x scale, rotated 90deg CCW, shifted by (100, 50), applied to the
    // y-up flip of the image point: p_local = R(90) * 2 * (imgX, -imgY) + (100, 50);
    // R(90)*(x,y) = (-y,x), so p_local = (100 + 2*imgY, 50 + 2*imgX).
    const tie1 = { img: [10, 20] as const, local: [100 + 2 * 20, 50 + 2 * 10] as const };
    const tie2 = { img: [50, 80] as const, local: [100 + 2 * 80, 50 + 2 * 50] as const };
    const T = similarityFromTwoPoints(tie1, tie2);
    const p = applySimilarity(T, [30, 40]);
    expect(p[0]).toBeCloseTo(100 + 2 * 40, 6);
    expect(p[1]).toBeCloseTo(50 + 2 * 30, 6);
  });

  it("identity when ties are identity", () => {
    const T = similarityFromTwoPoints(
      { img: [0, 0], local: [0, 0] },
      { img: [100, 0], local: [100, 0] },
    );
    const p = applySimilarity(T, [25, 75]);
    expect(p[0]).toBeCloseTo(25, 6);
    expect(p[1]).toBeCloseTo(-75, 6); // image y-down flips to local z-up
  });

  it("throws when tie points coincide", () => {
    expect(() =>
      similarityFromTwoPoints({ img: [1, 1], local: [0, 0] }, { img: [1, 1], local: [5, 5] }),
    ).toThrow();
  });
});
