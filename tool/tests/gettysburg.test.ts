import { describe, expect, it } from "vitest";
import battle from "../../app/Assets/Battle/gettysburg-july3.json";
import { validateBattle } from "../src/validate";

describe("authored July 3 battle", () => {
  it("passes schema + rule validation", () => {
    const result = validateBattle(battle);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });
  it("keeps every keyframe on the battlefield", () => {
    for (const u of (battle as any).units)
      for (const k of u.keyframes) {
        expect(k.x).toBeGreaterThan(0); expect(k.x).toBeLessThan(8507);
        expect(k.z).toBeGreaterThan(0); expect(k.z).toBeLessThan(8507);
      }
  });
  it("every documented keyframe carries a citation", () => {
    for (const u of (battle as any).units)
      for (const k of u.keyframes)
        if (k.confidence === "documented") expect(k.citation?.trim()).toBeTruthy();
  });
});
