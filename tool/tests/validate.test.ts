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
});
