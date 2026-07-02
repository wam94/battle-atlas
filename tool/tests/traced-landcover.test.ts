import { describe, expect, it } from "vitest";
import landcover from "../../data/landcover/landcover.json";
import { validateLandcover } from "../src/landcover";

describe("traced 1863 land cover (Bachelder, charge corridor)", () => {
  it("passes schema + rule validation", () => {
    const result = validateLandcover(landcover);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });
  it("covers the corridor and the full battlefield square", () => {
    expect((landcover as any).features.length).toBeGreaterThanOrEqual(59);
  });
  it("every feature answers 'says who'", () => {
    for (const f of (landcover as any).features)
      if (f.confidence === "documented") expect(f.source?.trim()).toBeTruthy();
  });
});
