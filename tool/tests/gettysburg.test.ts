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
  it("every regiment roster has at least 2 unique entries", () => {
    for (const u of (battle as any).units) {
      if (u.regiments === undefined) continue;
      expect(u.regiments.length).toBeGreaterThanOrEqual(2);
      expect(new Set(u.regiments).size).toBe(u.regiments.length);
    }
  });
  it("A-grade brigades are fully decomposed: children with valid parents, rosters gone", () => {
    const units = (battle as any).units;
    const childrenOf = (p: string) =>
      units.filter((u: any) => u.parent === p).map((u: any) => u.id).sort();
    expect(childrenOf("us-stannard")).toEqual(["us-13vt", "us-14vt", "us-16vt"]);
    expect(childrenOf("us-webb")).toEqual(["us-69pa", "us-71pa", "us-72pa"]);
    expect(childrenOf("us-hall")).toEqual(
      ["us-19ma", "us-20ma", "us-42ny", "us-59ny", "us-7mi"]);
    for (const id of ["us-stannard", "us-webb", "us-hall"]) {
      const parent = units.find((u: any) => u.id === id);
      expect(parent.regiments).toBeUndefined(); // full decomposition or none
      expect(parent.parent).toBeUndefined(); // depth 1
    }
  });
  it("known-honest strength gaps surface as advisory warnings, never errors", () => {
    const result = validateBattle(battle);
    expect(result.ok).toBe(true);
    // Webb: the 106th PA stays unmodeled, counted only in the parent (short).
    // Hall: attested regimental sums (1,076) exceed the contested Stone
    // Sentinels brigade-table 920 — a kept disagreement, not a reconciliation.
    expect(result.warnings.join(" ")).toContain("us-webb");
    expect(result.warnings.join(" ")).toContain("us-hall");
    expect(result.warnings).toHaveLength(2);
  });
  it("the two new units exist and the unit count is pinned", () => {
    const units = (battle as any).units;
    expect(units).toHaveLength(35); // 22 + 11 children + 8th Ohio + Brown's B
    const ohio = units.find((u: any) => u.id === "us-8oh");
    expect(ohio).toBeDefined();
    expect(ohio.parent).toBeUndefined(); // Carroll's brigade isn't modeled
    expect(ohio.keyframes).toHaveLength(3);
    const brown = units.find((u: any) => u.id === "us-btty-brown");
    expect(brown).toBeDefined();
    // Cowan's gallop-in fix: no longer starts in place at its Copse position
    const cowan = units.find((u: any) => u.id === "us-btty-cowan");
    expect(cowan.keyframes[0].z).not.toBe(cowan.keyframes[cowan.keyframes.length - 1].z);
  });
});
