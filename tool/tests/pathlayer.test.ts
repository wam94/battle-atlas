import { describe, expect, it } from "vitest";
import { battleToGeoJSON, previewToGeoJSON } from "../src/ui/pathlayer";
import { Battlefield } from "../src/geo";
import golden from "./fixtures/geo-golden.json";
import placeholder from "./fixtures/placeholder_battle.json";

const bf = new Battlefield(golden.origin_utm_e, golden.origin_utm_n);

describe("pathlayer geojson", () => {
  it("marks the selected unit", () => {
    const gj = battleToGeoJSON(placeholder as any, bf, "atk-a");
    const selected = gj.paths.features.filter((f) => f.properties!.selected);
    expect(selected.length).toBe(1);
  });

  it("preview polygon centroid sits at the interpolated position", () => {
    const gj = previewToGeoJSON(placeholder as any, bf, 0);
    const ring = (gj.features[0]!.geometry as GeoJSON.Polygon).coordinates[0]!;
    const cx = ring.slice(0, 4).reduce((s, c) => s + c[0]!, 0) / 4;
    const cy = ring.slice(0, 4).reduce((s, c) => s + c[1]!, 0) / 4;
    const unit0 = (placeholder as any).units[0];
    const [lon, lat] = bf.localToLonLat(unit0.keyframes[0].x, unit0.keyframes[0].z);
    expect(cx).toBeCloseTo(lon, 6);
    expect(cy).toBeCloseTo(lat, 6);
  });
});

describe("previewToGeoJSON with keyframeless units", () => {
  it("skips units that have no keyframes yet instead of throwing", () => {
    const battle = structuredClone(placeholder) as any;
    battle.units.push({
      id: "fresh", name: "Fresh Unit", side: "union",
      frontage_m: 100, depth_m: 20, keyframes: [],
    });
    const gj = previewToGeoJSON(battle, bf, 0);
    expect(gj.features.length).toBe(placeholder.units.length);
  });
});

describe("selected keyframe styling", () => {
  it("marks the selected keyframe dot", () => {
    const gj = battleToGeoJSON(placeholder as any, bf, "atk-a", 2);
    const marked = gj.dots.features.filter((f) => f.properties!.kfSelected);
    expect(marked.length).toBe(1);
    expect(marked[0]!.properties!.index).toBe(2);
    expect(marked[0]!.properties!.unitId).toBe("atk-a");
  });
});
