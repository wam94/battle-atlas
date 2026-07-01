import maplibregl from "maplibre-gl";
import "maplibre-gl/dist/maplibre-gl.css";
import type { Battlefield } from "../geo";

// OSM raster style — fine for an internal tool; respect the tile usage policy
// (low volume, single user).
const OSM_STYLE: maplibregl.StyleSpecification = {
  version: 8,
  sources: {
    osm: {
      type: "raster",
      tiles: ["https://tile.openstreetmap.org/{z}/{x}/{y}.png"],
      tileSize: 256,
      attribution: "© OpenStreetMap contributors",
    },
  },
  layers: [{ id: "osm", type: "raster", source: "osm" }],
};

export function createMap(container: HTMLElement, bf: Battlefield, sizeM: number): maplibregl.Map {
  const [centerLon, centerLat] = bf.localToLonLat(sizeM / 2, sizeM / 2);
  const map = new maplibregl.Map({
    container,
    style: OSM_STYLE,
    center: [centerLon, centerLat],
    zoom: 12.2,
  });

  map.on("load", () => {
    const corners: [number, number][] = [
      bf.localToLonLat(0, 0),
      bf.localToLonLat(sizeM, 0),
      bf.localToLonLat(sizeM, sizeM),
      bf.localToLonLat(0, sizeM),
      bf.localToLonLat(0, 0),
    ];
    map.addSource("battlefield-bounds", {
      type: "geojson",
      data: { type: "Feature", properties: {}, geometry: { type: "LineString", coordinates: corners } },
    });
    map.addLayer({
      id: "battlefield-bounds",
      type: "line",
      source: "battlefield-bounds",
      paint: { "line-color": "#c9a227", "line-width": 2, "line-dasharray": [3, 2] },
    });
  });

  return map;
}
