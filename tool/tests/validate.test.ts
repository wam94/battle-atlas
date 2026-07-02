import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import { validateBattle } from "../src/validate";

describe("validateBattle", () => {
  it("accepts the placeholder battle", () => {
    const result = validateBattle(placeholder);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });

  it("rejects non-increasing keyframe times", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].keyframes[1].t = bad.units[0].keyframes[0].t;
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("strictly increase");
  });

  it("rejects duplicate unit ids", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[1].id = bad.units[0].id;
    expect(validateBattle(bad).ok).toBe(false);
  });

  it("enforces the no-faking gate: documented requires citation", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].keyframes[0].confidence = "documented";
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toMatch(/citation/i);
  });

  it("rejects keyframes beyond endTime", () => {
    const bad = structuredClone(placeholder) as any;
    bad.endTime = 10;
    expect(validateBattle(bad).ok).toBe(false);
  });

  it("rejects unknown side via schema", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].side = "martian";
    expect(validateBattle(bad).ok).toBe(false);
  });

  // parent/children rules (see battle-format.md "Parent / children")

  it("accepts a decomposed family: parent + children, children strengths in tolerance", () => {
    const fam = structuredClone(placeholder) as any;
    // atk-a (t=0 strength 1800) decomposes into two children summing to 1800
    fam.units[3].parent = "atk-a";
    fam.units[3].keyframes[0].strength = 900;
    fam.units[4].parent = "atk-a";
    fam.units[4].keyframes[0].strength = 900;
    const result = validateBattle(fam);
    expect(result.errors).toEqual([]);
    expect(result.warnings).toEqual([]);
    expect(result.ok).toBe(true);
  });

  it("rejects a parent that doesn't reference an existing unit id", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[3].parent = "no-such-brigade";
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("no-such-brigade");
  });

  it("rejects grandparents: a parent may not itself have a parent", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[1].parent = "atk-a"; // atk-b is a child...
    bad.units[2].parent = "atk-b"; // ...and a parent: depth 2, forbidden
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toMatch(/parent/i);
  });

  it("rejects a parent-with-children carrying a regiments roster (full decomposition or none)", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].regiments = ["1st Test", "2nd Test"];
    bad.units[3].parent = "atk-a";
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toMatch(/regiments/i);
  });

  it("warns (advisory, still ok) when children t=0 strengths sum outside ±15% of the parent's", () => {
    const fam = structuredClone(placeholder) as any;
    // atk-a's t=0 strength is 1800; shrink one child so the sum (900 + 100)
    // falls far below the 85% floor
    fam.units[3].parent = "atk-a";
    fam.units[4].parent = "atk-a";
    fam.units[4].keyframes[0].strength = 100; // sum 1000 < 0.85 * 1800
    const result = validateBattle(fam);
    expect(result.ok).toBe(true);
    expect(result.errors).toEqual([]);
    expect(result.warnings.join(" ")).toContain("atk-a");
  });
});
