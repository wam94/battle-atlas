import type maplibregl from "maplibre-gl";
import type { Battle } from "../model";
import { stateAt } from "../model";
import type { Battlefield } from "../geo";

export function battleToGeoJSON(battle: Battle, bf: Battlefield, selectedUnitId: string | null) {
  const paths: GeoJSON.Feature[] = [];
  const dots: GeoJSON.Feature[] = [];
  for (const unit of battle.units) {
    const coords = unit.keyframes.map((k) => bf.localToLonLat(k.x, k.z));
    paths.push({
      type: "Feature",
      properties: { side: unit.side, selected: unit.id === selectedUnitId },
      geometry: { type: "LineString", coordinates: coords },
    });
    unit.keyframes.forEach((k, i) => {
      dots.push({
        type: "Feature",
        properties: { side: unit.side, unitId: unit.id, index: i, selected: unit.id === selectedUnitId },
        geometry: { type: "Point", coordinates: bf.localToLonLat(k.x, k.z) },
      });
    });
  }
  return {
    paths: { type: "FeatureCollection" as const, features: paths },
    dots: { type: "FeatureCollection" as const, features: dots },
  };
}

export function previewToGeoJSON(battle: Battle, bf: Battlefield, t: number) {
  const features: GeoJSON.Feature[] = battle.units.map((unit) => {
    const s = stateAt(unit, t);
    // oriented footprint rectangle (local meters -> lon/lat ring)
    const rad = ((90 - s.facing) * Math.PI) / 180; // compass -> math angle of unit FORWARD
    const fwd = [Math.cos(rad), Math.sin(rad)];
    const right = [Math.sin(rad), -Math.cos(rad)];
    const hw = unit.frontage_m / 2;
    const hd = unit.depth_m / 2;
    const cornersLocal: [number, number][] = [
      [s.x - right[0]! * hw - fwd[0]! * hd, s.z - right[1]! * hw - fwd[1]! * hd],
      [s.x + right[0]! * hw - fwd[0]! * hd, s.z + right[1]! * hw - fwd[1]! * hd],
      [s.x + right[0]! * hw + fwd[0]! * hd, s.z + right[1]! * hw + fwd[1]! * hd],
      [s.x - right[0]! * hw + fwd[0]! * hd, s.z - right[1]! * hw + fwd[1]! * hd],
    ];
    const ring = [...cornersLocal, cornersLocal[0]!].map(([x, z]) => bf.localToLonLat(x, z));
    return {
      type: "Feature",
      properties: { side: unit.side },
      geometry: { type: "Polygon", coordinates: [ring] },
    };
  });
  return { type: "FeatureCollection" as const, features };
}

export function installBattleLayers(map: maplibregl.Map): void {
  map.addSource("unit-paths", { type: "geojson", data: emptyFC() });
  map.addSource("unit-dots", { type: "geojson", data: emptyFC() });
  map.addSource("unit-preview", { type: "geojson", data: emptyFC() });
  map.addLayer({
    id: "unit-preview", type: "fill", source: "unit-preview",
    paint: {
      "fill-color": ["match", ["get", "side"], "union", "#3b5a9c", "confederate", "#a05050", "#888888"],
      "fill-opacity": 0.65,
    },
  });
  map.addLayer({
    id: "unit-paths", type: "line", source: "unit-paths",
    paint: {
      "line-color": ["match", ["get", "side"], "union", "#3b5a9c", "confederate", "#a05050", "#888888"],
      "line-width": ["case", ["get", "selected"], 3, 1.5],
      "line-opacity": ["case", ["get", "selected"], 0.95, 0.5],
    },
  });
  map.addLayer({
    id: "unit-dots", type: "circle", source: "unit-dots",
    paint: {
      "circle-radius": ["case", ["get", "selected"], 5, 3],
      "circle-color": ["match", ["get", "side"], "union", "#3b5a9c", "confederate", "#a05050", "#888888"],
      "circle-stroke-width": 1,
      "circle-stroke-color": "#ffffff",
    },
  });
}

const emptyFC = () => ({ type: "FeatureCollection" as const, features: [] });
