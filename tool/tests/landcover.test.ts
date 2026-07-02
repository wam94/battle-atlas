import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_landcover.json";
import { exportLandcover, importLandcover, validateLandcover } from "../src/landcover";

describe("validateLandcover", () => {
  it("accepts the placeholder land cover", () => {
    const result = validateLandcover(placeholder);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });

  it("accepts empty features (tracing starts empty)", () => {
    const empty = { name: "Empty trace", features: [] };
    const result = validateLandcover(empty);
    expect(result.ok).toBe(true);
  });

  it("enforces the no-faking gate: documented requires source", () => {
    const bad = structuredClone(placeholder) as any;
    delete bad.features[0].source;
    const result = validateLandcover(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toMatch(/source/i);
  });

  it("rejects a polygon with fewer than 3 points", () => {
    const bad = structuredClone(placeholder) as any;
    bad.features[0].points = [[1000, 1000], [1200, 1000]];
    const result = validateLandcover(bad);
    expect(result.ok).toBe(false);
  });

  it("rejects a line feature with a polygon class", () => {
    const bad = structuredClone(placeholder) as any;
    bad.features[1].cls = "woodlot";
    const result = validateLandcover(bad);
    expect(result.ok).toBe(false);
  });

  it("rejects an out-of-bounds point", () => {
    const bad = structuredClone(placeholder) as any;
    bad.features[0].points[0] = [9000, 1000];
    const result = validateLandcover(bad);
    expect(result.ok).toBe(false);
  });

  it("rejects duplicate feature ids", () => {
    const bad = structuredClone(placeholder) as any;
    bad.features[1].id = bad.features[0].id;
    const result = validateLandcover(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("duplicate");
  });
});

describe("landcover IO", () => {
  it("round-trips the placeholder land cover structurally (parsed-equal)", () => {
    const landcover = importLandcover(JSON.stringify(placeholder));
    const out = exportLandcover(landcover);
    expect(JSON.parse(out)).toEqual(placeholder);
  });

  it("export refuses an invalid land cover", () => {
    const landcover = importLandcover(JSON.stringify(placeholder));
    delete landcover.features[0]!.source;
    landcover.features[0]!.confidence = "documented";
    expect(() => exportLandcover(landcover)).toThrow(/source/i);
  });

  it("export emits keys in canonical order for clean diffs", () => {
    const landcover = importLandcover(JSON.stringify(placeholder));
    const out = exportLandcover(landcover);
    const firstFeature = out.indexOf('"features"');
    expect(out.indexOf('"name"')).toBeLessThan(firstFeature);
    const idIdx = out.indexOf('"id"');
    const kindIdx = out.indexOf('"kind"');
    expect(idIdx).toBeLessThan(kindIdx);
  });
});
