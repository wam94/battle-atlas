import proj4 from "proj4";

// NAD83 / UTM zone 18N — the pipeline's CRS (see pipeline/terrain_pipeline/config.py)
proj4.defs(
  "EPSG:26918",
  "+proj=utm +zone=18 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs +type=crs",
);

// Battlefield-local meters (x east, z north from the terrain SW corner) anchored
// to UTM by the heightmap's origin. The same convention as the Unity app —
// see docs/format/battle-format.md.
export class Battlefield {
  constructor(
    readonly originUtmE: number,
    readonly originUtmN: number,
  ) {}

  localToLonLat(x: number, z: number): [number, number] {
    const [lon, lat] = proj4("EPSG:26918", "WGS84", [
      this.originUtmE + x,
      this.originUtmN + z,
    ]);
    return [lon, lat];
  }

  lonLatToLocal(lon: number, lat: number): [number, number] {
    const [e, n] = proj4("WGS84", "EPSG:26918", [lon, lat]);
    return [e - this.originUtmE, n - this.originUtmN];
  }
}
