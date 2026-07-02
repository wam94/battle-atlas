import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";
import type {
  Landcover,
  LandcoverClass,
  LandcoverConfidence,
  LandcoverFeature,
  FeatureKind,
} from "../landcover";
import { exportLandcover, importLandcover, validateLandcover } from "../landcover";
import { loadLandcoverAutosave, saveLandcoverAutosave } from "../persist";

const POLYGON_CLASSES: readonly LandcoverClass[] = ["woodlot", "orchard", "field", "pasture", "marsh"];
const LINE_CLASSES: readonly LandcoverClass[] = ["stone_wall", "rail_fence"];
const CONFIDENCES: readonly LandcoverConfidence[] = ["documented", "inferred"];
const DEFAULT_SOURCE = "Warren 1868-69, LOC 99448794";

// class -> fill/line color, per the plan's sand-table palette. Polygon
// classes get fill colors; line classes get line colors (stone grey / rail
// brown). Kept alongside the class vocabularies so landcoverToGeoJSON and the
// sidebar swatches can never drift apart.
const CLASS_COLOR: Record<LandcoverClass, string> = {
  woodlot: "#2f4a2b", // dark green
  orchard: "#7dab5c", // light green
  field: "#c9b36a", // wheat gold
  pasture: "#a9c48a", // pale green
  marsh: "#7c8f96", // blue-grey
  stone_wall: "#8a8a82", // stone grey
  rail_fence: "#8b5a2b", // rail brown
};

// True while land-cover tracing mode is on, so the workspace's map-click
// handler can delegate vertex placement here instead of appending a
// keyframe. Same module-level guard-flag-as-exported-function shape as
// overlayui.ts's isPickingTiePoint — the workspace click handler checks this
// AFTER the tie-point guard and BEFORE the drag guard.
let tracing = false;
export function isTracing(): boolean {
  return tracing;
}

// Set once initLandcoverUI runs; the workspace's map click handler calls this
// directly (mirrors how isPickingTiePoint gates the same handler).
let handleMapClick: (lngLat: { lng: number; lat: number }) => void = () => {};
export function landcoverHandleMapClick(lngLat: { lng: number; lat: number }): void {
  handleMapClick(lngLat);
}

// Pure builder: features -> three GeoJSON FeatureCollections (fills, lines,
// vertex dots for the selected/in-progress feature aren't included here —
// this only covers committed features). Mirrors pathlayer's
// battleToGeoJSON: pure, testable, no map/DOM dependency.
export function landcoverToGeoJSON(features: LandcoverFeature[]) {
  const fillFeatures: GeoJSON.Feature[] = [];
  const lineFeatures: GeoJSON.Feature[] = [];
  for (const f of features) {
    const color = CLASS_COLOR[f.cls];
    if (f.kind === "polygon") {
      const ring = [...f.points, f.points[0]!];
      fillFeatures.push({
        type: "Feature",
        properties: { id: f.id, cls: f.cls, color },
        geometry: { type: "Polygon", coordinates: [ring] },
      });
    } else {
      lineFeatures.push({
        type: "Feature",
        properties: { id: f.id, cls: f.cls, color },
        geometry: { type: "LineString", coordinates: f.points },
      });
    }
  }
  return {
    fills: { type: "FeatureCollection" as const, features: fillFeatures },
    lines: { type: "FeatureCollection" as const, features: lineFeatures },
  };
}

function previewToGeoJSON(points: [number, number][], bf: Battlefield, kind: FeatureKind) {
  if (points.length === 0) return { type: "FeatureCollection" as const, features: [] };
  const coords = points.map(([x, z]) => bf.localToLonLat(x, z));
  const geometry: GeoJSON.Geometry =
    kind === "polygon" && coords.length >= 3
      ? { type: "Polygon", coordinates: [[...coords, coords[0]!]] }
      : { type: "LineString", coordinates: coords.length > 1 ? coords : [coords[0]!, coords[0]!] };
  return {
    type: "FeatureCollection" as const,
    features: [{ type: "Feature" as const, properties: {}, geometry }],
  };
}

// Installs the land-cover map sources/layers once. Layer order: UNDER unit
// layers (unit-preview/paths/dots), ABOVE the hist-overlay raster — so traced
// polygons and fences read as ground truth beneath the authored battle, but
// still legible over the draped period map used to trace them. overlayui.ts
// inserts hist-overlay with beforeId "landcover-fill" (falling back to
// "unit-preview") when this layer exists, keeping the overlay below us.
export function installLandcoverLayers(map: maplibregl.Map): void {
  map.addSource("landcover-fills", { type: "geojson", data: emptyFC() });
  map.addSource("landcover-lines", { type: "geojson", data: emptyFC() });
  map.addSource("landcover-preview", { type: "geojson", data: emptyFC() });

  map.addLayer(
    {
      id: "landcover-fill",
      type: "fill",
      source: "landcover-fills",
      paint: { "fill-color": ["get", "color"], "fill-opacity": 0.45 },
    },
    "unit-preview",
  );
  map.addLayer(
    {
      id: "landcover-line",
      type: "line",
      source: "landcover-lines",
      paint: { "line-color": ["get", "color"], "line-width": 2.5 },
    },
    "unit-preview",
  );
  map.addLayer(
    {
      id: "landcover-preview",
      type: "line",
      source: "landcover-preview",
      paint: { "line-color": "#c9a227", "line-width": 2, "line-dasharray": [2, 1] },
    },
    "unit-preview",
  );
}

const emptyFC = () => ({ type: "FeatureCollection" as const, features: [] });

export function initLandcoverUI(container: HTMLElement, map: maplibregl.Map, bf: Battlefield): void {
  let landcover: Landcover = loadLandcoverAutosave() ?? { name: "untitled land cover", features: [] };
  let selectedId: string | null = null;

  // In-progress feature state, live while tracing.
  let draftKind: FeatureKind = "polygon";
  let draftCls: LandcoverClass = "woodlot";
  let draftSource = DEFAULT_SOURCE;
  let draftConfidence: LandcoverConfidence = "documented";
  let draftPoints: [number, number][] = [];

  const installLayers = () => installLandcoverLayers(map);
  if (map.isStyleLoaded()) installLayers();
  else map.on("load", installLayers);

  function syncMap(): void {
    saveLandcoverAutosave(landcover);
    if (!map.getSource("landcover-fills")) return;
    const gj = landcoverToGeoJSON(landcover.features);
    (map.getSource("landcover-fills") as maplibregl.GeoJSONSource).setData(gj.fills);
    (map.getSource("landcover-lines") as maplibregl.GeoJSONSource).setData(gj.lines);
    (map.getSource("landcover-preview") as maplibregl.GeoJSONSource).setData(
      previewToGeoJSON(draftPoints, bf, draftKind),
    );
  }

  function resetDraft(): void {
    draftPoints = [];
  }

  function cancelDraft(): void {
    resetDraft();
    syncMap();
    render();
  }

  // Registered once via the exported landcoverHandleMapClick indirection —
  // workspace.ts's single map click handler calls it while isTracing().
  handleMapClick = (lngLat) => {
    if (!tracing) return;
    const [x, z] = bf.lonLatToLocal(lngLat.lng, lngLat.lat);
    draftPoints.push([Math.round(x), Math.round(z)]);
    syncMap();
    render();
  };

  const escHandler = (e: KeyboardEvent) => {
    if (e.key === "Escape" && tracing && draftPoints.length > 0) cancelDraft();
  };
  document.addEventListener("keydown", escHandler);

  function finishFeature(): void {
    const minPoints = draftKind === "polygon" ? 3 : 2;
    if (draftPoints.length < minPoints) return;
    const id = `lc-${Date.now().toString(36)}-${Math.floor(Math.random() * 1000)}`;
    const feature: LandcoverFeature = {
      id,
      kind: draftKind,
      cls: draftCls,
      points: draftPoints,
      confidence: draftConfidence,
      ...(draftSource.trim() && { source: draftSource.trim() }),
    };
    landcover.features.push(feature);
    resetDraft();
    syncMap();
    render();
  }

  function render(): void {
    container.replaceChildren();
    const frag = document.createDocumentFragment();

    frag.append(h2("Land cover (trace from draped period maps)"));

    const modeBtn = document.createElement("button");
    modeBtn.textContent = tracing ? "Stop tracing" : "Start tracing";
    modeBtn.addEventListener("click", () => {
      tracing = !tracing;
      if (!tracing) resetDraft();
      syncMap();
      render();
    });
    frag.append(modeBtn);

    if (tracing) {
      const kindSel = select(["polygon", "line"], draftKind, (v) => {
        draftKind = v as FeatureKind;
        draftCls = draftKind === "polygon" ? POLYGON_CLASSES[0]! : LINE_CLASSES[0]!;
        cancelDraft();
      });
      frag.append(labeled("feature kind", kindSel));

      const clsOptions = draftKind === "polygon" ? POLYGON_CLASSES : LINE_CLASSES;
      const clsSel = select(clsOptions, draftCls, (v) => { draftCls = v as LandcoverClass; });
      frag.append(labeled("class", clsSel));

      const sourceInput = textInput(draftSource, (v) => { draftSource = v; });
      frag.append(labeled("source", sourceInput));

      const confSel = select(CONFIDENCES, draftConfidence, (v) => {
        draftConfidence = v as LandcoverConfidence;
      });
      frag.append(labeled("confidence", confSel));

      const status = document.createElement("div");
      status.className = "muted";
      const minPoints = draftKind === "polygon" ? 3 : 2;
      status.textContent = `${draftPoints.length} point(s) placed — click the map to add vertices (need ${minPoints}+).`;
      frag.append(status);

      const finishBtn = document.createElement("button");
      finishBtn.textContent = draftKind === "polygon" ? "finish feature (close polygon)" : "finish feature (finish line)";
      finishBtn.disabled = draftPoints.length < minPoints;
      finishBtn.addEventListener("click", finishFeature);
      const cancelBtn = document.createElement("button");
      cancelBtn.textContent = "cancel (Esc)";
      cancelBtn.addEventListener("click", cancelDraft);
      frag.append(row(finishBtn, cancelBtn));
    }

    frag.append(h2("Features"));
    for (const f of landcover.features) {
      frag.append(featureEditor(f));
    }

    frag.append(h2("File"));
    const exportBtn = document.createElement("button");
    exportBtn.textContent = "Export land cover JSON";
    const errBox = document.createElement("div");
    errBox.className = "error";
    exportBtn.addEventListener("click", () => {
      try {
        const out = exportLandcover(landcover);
        errBox.textContent = "";
        const a = document.createElement("a");
        a.href = URL.createObjectURL(new Blob([out], { type: "application/json" }));
        a.download = "landcover.json";
        a.click();
        URL.revokeObjectURL(a.href);
        saveLandcoverAutosave(landcover);
      } catch (err) {
        errBox.textContent = String(err);
      }
    });
    const importInput = document.createElement("input");
    importInput.type = "file";
    importInput.accept = ".json";
    importInput.addEventListener("change", async () => {
      const file = importInput.files?.[0];
      if (!file) return;
      try {
        landcover = importLandcover(await file.text());
        selectedId = null;
        resetDraft();
        saveLandcoverAutosave(landcover);
        errBox.textContent = "";
        syncMap();
        render();
      } catch (err) {
        errBox.textContent = String(err);
      }
    });
    const liveErrors = validateLandcover(landcover);
    const statusBox = document.createElement("div");
    statusBox.className = liveErrors.ok ? "muted" : "error";
    statusBox.textContent = liveErrors.ok ? "land cover is valid" : liveErrors.errors.join("\n");
    frag.append(row(exportBtn), labeled("import land cover JSON", importInput), statusBox, errBox);

    container.append(frag);
  }

  function featureEditor(f: LandcoverFeature): HTMLElement {
    const box = document.createElement("div");
    box.className = f.id === selectedId ? "kf selected" : "kf";
    box.addEventListener("click", (ev) => {
      if (
        ev.target instanceof HTMLInputElement ||
        ev.target instanceof HTMLSelectElement ||
        ev.target instanceof HTMLButtonElement
      )
        return;
      selectedId = f.id === selectedId ? null : f.id;
      render();
    });
    box.append(new Text(`${f.id} (${f.kind}, ${f.points.length} pts)`));
    const clsOptions = f.kind === "polygon" ? POLYGON_CLASSES : LINE_CLASSES;
    box.append(
      labeled(
        "class",
        select(clsOptions, f.cls, (v) => {
          f.cls = v as LandcoverClass;
          syncMap();
        }),
      ),
    );
    box.append(labeled("source", textInput(f.source ?? "", (v) => { f.source = v || undefined; })));
    box.append(
      labeled(
        "confidence",
        select(CONFIDENCES, f.confidence, (v) => {
          f.confidence = v as LandcoverConfidence;
          render();
        }),
      ),
    );
    box.append(labeled("note", textInput(f.note ?? "", (v) => { f.note = v || undefined; })));
    const del = document.createElement("button");
    del.textContent = "delete feature";
    del.style.marginLeft = "8px";
    del.addEventListener("click", () => {
      landcover.features = landcover.features.filter((x) => x.id !== f.id);
      selectedId = null;
      syncMap();
      render();
    });
    box.append(del);
    return box;
  }

  // small DOM helpers (mirrors workspace.ts)
  function h2(text: string): HTMLHeadingElement {
    const e = document.createElement("h2");
    e.textContent = text;
    return e;
  }
  function labeled(text: string, input: HTMLElement): HTMLLabelElement {
    const l = document.createElement("label");
    l.textContent = text;
    l.append(input);
    return l;
  }
  function textInput(value: string, onChange: (v: string) => void): HTMLInputElement {
    const i = document.createElement("input");
    i.type = "text";
    i.value = value;
    i.addEventListener("change", () => onChange(i.value));
    return i;
  }
  function select(
    options: readonly string[],
    value: string,
    onChange: (v: string) => void,
  ): HTMLSelectElement {
    const s = document.createElement("select");
    for (const o of options) {
      const opt = document.createElement("option");
      opt.value = o;
      opt.textContent = o;
      s.append(opt);
    }
    s.value = value;
    s.addEventListener("change", () => onChange(s.value));
    return s;
  }
  function row(...children: HTMLElement[]): HTMLDivElement {
    const d = document.createElement("div");
    d.className = "row";
    d.append(...children);
    return d;
  }

  syncMap();
  render();
}
