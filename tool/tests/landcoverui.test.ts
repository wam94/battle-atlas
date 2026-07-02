import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_landcover.json";
import { landcoverToGeoJSON } from "../src/ui/landcoverui";
import type { LandcoverFeature } from "../src/landcover";

const features = placeholder.features as LandcoverFeature[];

describe("landcoverToGeoJSON", () => {
  it("separates polygon features into fills and line features into lines", () => {
    const gj = landcoverToGeoJSON(features);
    expect(gj.fills.features.length).toBe(1);
    expect(gj.lines.features.length).toBe(1);
    expect(gj.fills.features[0]!.geometry.type).toBe("Polygon");
    expect(gj.lines.features[0]!.geometry.type).toBe("LineString");
  });

  it("closes the polygon ring and tags each feature with a class color", () => {
    const gj = landcoverToGeoJSON(features);
    const polyFeature = gj.fills.features[0]!;
    const ring = (polyFeature.geometry as GeoJSON.Polygon).coordinates[0]!;
    expect(ring[0]).toEqual(ring[ring.length - 1]);
    expect(ring.length).toBe(features[0]!.points.length + 1);
    expect(polyFeature.properties!.color).toMatch(/^#[0-9a-f]{6}$/i);
    expect(polyFeature.properties!.cls).toBe("woodlot");
  });

  it("maps line class to a distinct color property from polygon classes", () => {
    const gj = landcoverToGeoJSON(features);
    const lineFeature = gj.lines.features[0]!;
    expect(lineFeature.properties!.cls).toBe("rail_fence");
    expect(lineFeature.properties!.color).toMatch(/^#[0-9a-f]{6}$/i);
    expect(lineFeature.properties!.color).not.toBe(gj.fills.features[0]!.properties!.color);
  });
});
