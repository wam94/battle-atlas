import { describe, expect, it } from "vitest";
import golden from "./fixtures/geo-golden.json";
import { Battlefield } from "../src/geo";

const bf = new Battlefield(golden.origin_utm_e, golden.origin_utm_n);

describe("Battlefield geo conversions", () => {
  for (const c of golden.cases) {
    const [localX, localZ] = c.local as [number, number];
    const [lonGolden, latGolden] = c.lonLat as [number, number];

    it(`localToLonLat matches pyproj for ${c.name}`, () => {
      const [lon, lat] = bf.localToLonLat(localX, localZ);
      expect(lon).toBeCloseTo(lonGolden, 6); // ~0.1 m at this latitude
      expect(lat).toBeCloseTo(latGolden, 6);
    });

    it(`lonLatToLocal round-trips for ${c.name}`, () => {
      const [x, z] = bf.lonLatToLocal(lonGolden, latGolden);
      expect(x).toBeCloseTo(localX, 2); // centimeters
      expect(z).toBeCloseTo(localZ, 2);
    });
  }
});
