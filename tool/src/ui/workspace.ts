import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";
import type { Battle, Confidence, Formation, Side, Unit } from "../model";
import { exportBattle, importBattle } from "../io";
import { decomposeBrigade } from "../decompose";
import { validateBattle } from "../validate";
import { loadAutosave, saveAutosave, debouncedSaveAutosave } from "../persist";
import { nextKeyframeTime, clampDraftTime } from "../timeutil";
import { battleToGeoJSON, installBattleLayers, previewToGeoJSON } from "./pathlayer";
import { initOverlayUI, isPickingTiePoint } from "./overlayui";
import { initLandcoverUI, isTracing, landcoverHandleMapClick } from "./landcoverui";

const FORMATIONS: Formation[] = ["column", "line", "skirmish", "scattered", "routed"];
const CONFIDENCES: Confidence[] = ["unknown", "inferred", "documented"];

export function initWorkspace(el: HTMLElement, map: maplibregl.Map, bf: Battlefield): void {
  let battle: Battle = { name: "untitled battle", startTime: 0, endTime: 3600, units: [] };
  let selectedUnitId: string | null = null;
  let selectedKfIndex: number | null = null;
  let draftTime = 0;

  // Restore a crash-recovered autosave on startup. The banner is dismissible
  // (not auto-hidden) since the author needs a deliberate cue that what's on
  // screen came from a recovered draft, not the file they think they opened.
  const restored = loadAutosave(bf.originUtmE);
  let autosaveNotice: string | null = null;
  if (restored) {
    battle = restored;
    autosaveNotice = "restored autosave — Export to keep it";
  }

  // `el` splits into stable children: `dynamicEl` is fully rebuilt by every
  // render() call (as before), while `overlaySection` and `landcoverSection`
  // are built exactly once here so their state (loaded image/tie points/
  // placed raster source; traced features/tracing mode) survives sidebar
  // re-renders instead of being torn down each edit.
  el.replaceChildren();
  const dynamicEl = document.createElement("div");
  dynamicEl.id = "workspace-dynamic";
  const overlaySection = document.createElement("div");
  overlaySection.id = "overlay-section";
  const landcoverSection = document.createElement("div");
  landcoverSection.id = "landcover-section";
  el.append(dynamicEl, overlaySection, landcoverSection);
  initOverlayUI(overlaySection, map, bf);
  initLandcoverUI(landcoverSection, map, bf);

  const installLayers = () => installBattleLayers(map);
  if (map.isStyleLoaded()) installLayers();
  else map.on("load", installLayers);

  // Drag-in-progress guard: a mousedown on a dot starts a drag (see the
  // "unit-dots" mousedown handler below). When a drag has happened, the map's
  // own click handler still fires on mouseup — this flag lets it bail so a
  // drag never also appends a new keyframe at the drop point.
  let dragJustHappened = false;

  map.on("click", (e) => {
    if (isPickingTiePoint()) return; // tie-point picks are not keyframes
    if (isTracing()) { dragJustHappened = false; landcoverHandleMapClick(e.lngLat); return; } // land-cover vertex, not a keyframe; belt-and-suspenders reset
    if (dragJustHappened) { dragJustHappened = false; return; } // drag, not an append click
    const unit = battle.units.find((u) => u.id === selectedUnitId);
    if (!unit) return;
    const [x, z] = bf.lonLatToLocal(e.lngLat.lng, e.lngLat.lat);
    const lastT = unit.keyframes.length ? unit.keyframes[unit.keyframes.length - 1]!.t : null;
    const t = nextKeyframeTime(draftTime, lastT, battle.endTime);
    if (t === null) {
      // the track already reaches endTime — refuse loudly instead of creating
      // an equal-t keyframe the validator would reject
      autosaveNotice = "track reaches endTime — extend endTime to add more keyframes";
      render();
      return;
    }
    unit.keyframes.push({
      t,
      x: Math.round(x), z: Math.round(z),
      facing: 90, formation: "line",
      strength: unit.keyframes.length ? unit.keyframes[unit.keyframes.length - 1]!.strength : 1000,
    });
    render();
  });

  // Keyframe drag-to-move. Registered once (not per render): mousedown on a
  // dot belonging to the selected unit starts a drag — map panning is
  // disabled so the drag reads as moving the point, not the camera — and
  // mousemove live-updates that keyframe's position via syncMap() (cheap,
  // skips the full sidebar rebuild) until mouseup commits with a full render().
  map.on("mousedown", "unit-dots", (e) => {
    if (isTracing()) return; // land-cover tracing owns map clicks — don't let a stale drag start under it
    const feature = e.features?.[0];
    if (!feature) return;
    const unitId = feature.properties?.unitId as string | undefined;
    const index = feature.properties?.index as number | undefined;
    if (unitId == null || index == null || unitId !== selectedUnitId) return;
    const unit = battle.units.find((u) => u.id === unitId);
    if (!unit) return;

    e.preventDefault();
    selectedKfIndex = index;
    map.dragPan.disable();
    render();

    const onMouseMove = (ev: maplibregl.MapMouseEvent) => {
      const kf = unit.keyframes[index];
      if (!kf) return;
      const [x, z] = bf.lonLatToLocal(ev.lngLat.lng, ev.lngLat.lat);
      kf.x = Math.round(x);
      kf.z = Math.round(z);
      syncMap(); // live feedback without rebuilding the sidebar mid-drag
    };
    map.on("mousemove", onMouseMove);
    map.once("mouseup", () => {
      map.off("mousemove", onMouseMove);
      map.dragPan.enable();
      dragJustHappened = true;
      render();
    });
  });

  // Cursor feedback: only invite dragging over dots belonging to the
  // currently selected unit (dots on other units aren't draggable).
  map.on("mouseenter", "unit-dots", (e) => {
    const feature = e.features?.[0];
    const unitId = feature?.properties?.unitId as string | undefined;
    map.getCanvas().style.cursor = unitId != null && unitId === selectedUnitId ? "move" : "pointer";
  });
  map.on("mouseleave", "unit-dots", () => {
    map.getCanvas().style.cursor = "";
  });

  function syncMap(): void {
    debouncedSaveAutosave(battle, bf.originUtmE);
    if (!map.getSource("unit-paths")) return;
    const gj = battleToGeoJSON(battle, bf, selectedUnitId, selectedKfIndex);
    (map.getSource("unit-paths") as maplibregl.GeoJSONSource).setData(gj.paths);
    (map.getSource("unit-dots") as maplibregl.GeoJSONSource).setData(gj.dots);
    (map.getSource("unit-preview") as maplibregl.GeoJSONSource).setData(
      previewToGeoJSON(battle, bf, draftTime),
    );
  }

  function render(): void {
    syncMap();
    dynamicEl.replaceChildren();
    const frag = document.createDocumentFragment();

    // dismissible autosave-restore banner — only shown until the author
    // acknowledges it, then gone for the rest of the session.
    if (autosaveNotice) {
      const banner = document.createElement("div");
      banner.className = "autosave-notice";
      const msg = document.createElement("span");
      msg.textContent = autosaveNotice;
      const dismiss = document.createElement("button");
      dismiss.textContent = "dismiss";
      dismiss.addEventListener("click", () => { autosaveNotice = null; render(); });
      banner.append(msg, dismiss);
      frag.append(banner);
    }

    // battle header
    frag.append(h2("Battle"));
    frag.append(labeled("name", textInput(battle.name, (v) => { battle.name = v; })));
    frag.append(row(
      labeled("startTime (s since midnight)", numInput(battle.startTime, (v) => { battle.startTime = v; })),
      labeled("endTime (s)", numInput(battle.endTime, (v) => { battle.endTime = v; draftTime = clampDraftTime(draftTime, battle.endTime); render(); })),
    ));

    // time scrubber for preview — stable heading reference, updated in place on
    // slider input so scrubbing doesn't trigger a full sidebar re-render.
    const timeH2 = h2(`Preview time: ${fmt(draftTime)}`);
    frag.append(timeH2);
    const slider = document.createElement("input");
    slider.type = "range"; slider.min = "0"; slider.max = String(battle.endTime); slider.value = String(draftTime);
    slider.addEventListener("input", () => {
      draftTime = Number(slider.value);
      syncMap();
      timeH2.textContent = `Preview time: ${fmt(draftTime)}`;
    });
    frag.append(slider);

    // roster
    frag.append(h2("Units"));
    for (const unit of battle.units) {
      const btn = document.createElement("button");
      btn.textContent = `${unit.id === selectedUnitId ? "▶ " : ""}${unit.name} (${unit.side}, ${unit.keyframes.length} kf)`;
      btn.style.display = "block"; btn.style.width = "100%"; btn.style.margin = "2px 0";
      btn.addEventListener("click", () => {
        selectedUnitId = unit.id === selectedUnitId ? null : unit.id;
        selectedKfIndex = null;
        render();
      });
      frag.append(btn);
    }
    const addBtn = document.createElement("button");
    addBtn.textContent = "+ add unit";
    addBtn.addEventListener("click", () => {
      const id = `unit-${battle.units.length + 1}`;
      battle.units.push({ id, name: id, side: "union", frontage_m: 200, depth_m: 30, keyframes: [] });
      selectedUnitId = id; selectedKfIndex = null; render();
    });
    frag.append(addBtn);

    // selected unit editor
    const unit = battle.units.find((u) => u.id === selectedUnitId);
    if (unit) frag.append(unitEditor(unit));

    // io
    frag.append(h2("File"));
    const exportBtn = document.createElement("button");
    exportBtn.textContent = "Export battle JSON";
    const errBox = document.createElement("div"); errBox.className = "error";
    exportBtn.addEventListener("click", () => {
      try {
        const out = exportBattle(battle);
        errBox.textContent = "";
        const a = document.createElement("a");
        a.href = URL.createObjectURL(new Blob([out], { type: "application/json" }));
        a.download = "battle.json";
        a.click();
        URL.revokeObjectURL(a.href);
        // Export is the durable artifact, but the save should always mirror
        // the screen — not cleared here, just kept in sync. Immediate (not
        // debounced): a deliberate user action shouldn't leave a save pending.
        saveAutosave(battle, bf.originUtmE);
      } catch (err) { errBox.textContent = String(err); }
    });
    const importInput = document.createElement("input");
    importInput.type = "file"; importInput.accept = ".json";
    importInput.addEventListener("change", async () => {
      const f = importInput.files?.[0];
      if (!f) return;
      try {
        battle = importBattle(await f.text());
        selectedUnitId = null; selectedKfIndex = null; draftTime = 0;
        autosaveNotice = null;
        // Imported battle replaces whatever the autosave slot held. Immediate:
        // a deliberate user action shouldn't leave a save pending.
        saveAutosave(battle, bf.originUtmE);
        errBox.textContent = ""; render();
      } catch (err) { errBox.textContent = String(err); }
    });
    const liveErrors = validateBattle(battle);
    const status = document.createElement("div");
    status.className = liveErrors.ok ? "muted" : "error";
    // warnings are advisory (strength re-total): shown, never blocking
    status.textContent = liveErrors.ok
      ? ["battle is valid", ...liveErrors.warnings].join("\n")
      : liveErrors.errors.join("\n");
    frag.append(row(exportBtn), labeled("import battle JSON", importInput), status, errBox);

    dynamicEl.append(frag);
  }

  function unitEditor(unit: Unit): DocumentFragment {
    const frag = document.createDocumentFragment();
    frag.append(h2(`Unit: ${unit.id}`));
    frag.append(labeled("name", textInput(unit.name, (v) => { unit.name = v; })));
    const sideSel = select(["union", "confederate"], unit.side, (v) => { unit.side = v as Side; render(); });
    frag.append(labeled("side", sideSel));
    frag.append(row(
      labeled("frontage_m", numInput(unit.frontage_m, (v) => { unit.frontage_m = v; })),
      labeled("depth_m", numInput(unit.depth_m, (v) => { unit.depth_m = v; })),
    ));
    const del = document.createElement("button");
    del.textContent = "delete unit";
    del.addEventListener("click", () => {
      battle.units = battle.units.filter((u) => u.id !== unit.id);
      selectedUnitId = null; selectedKfIndex = null; render();
    });
    frag.append(del);

    // Decompose the roster into first-class child regiments (Task A4). The
    // generated tracks are inferred slot-follow scaffolding the author edits
    // afterwards; the result flows through the normal validation gate (the
    // live status below and the export gate), never a separate one.
    const decompose = document.createElement("button");
    decompose.textContent = "Decompose into regiments…";
    decompose.disabled = !unit.regiments || unit.regiments.length === 0;
    decompose.style.marginLeft = "8px";
    decompose.addEventListener("click", () => {
      const roster = unit.regiments ?? [];
      const equalShare = Math.round((unit.keyframes[0]?.strength ?? 0) / Math.max(1, roster.length));
      const strengths = new Map<string, number>();
      for (const name of roster) {
        const v = window.prompt(`t=0 strength for ${name} (per-regiment, from sources)`, String(equalShare));
        if (v === null) return; // author cancelled — apply nothing
        const parsed = Number(v);
        if (!Number.isFinite(parsed)) {
          // non-numeric entry: refuse loudly rather than let NaN flow into
          // decomposeBrigade — same treatment as Cancel, nothing applied
          autosaveNotice = `decompose cancelled: '${v}' is not a number (strength for ${name})`;
          render();
          return;
        }
        strengths.set(name, parsed);
      }
      try {
        battle = decomposeBrigade(battle, unit.id, strengths);
        selectedKfIndex = null;
      } catch (err) {
        autosaveNotice = String(err); // same loud-refusal surface as endTime clashes
      }
      render();
    });
    frag.append(decompose);

    frag.append(h2("Keyframes (click map to add at preview time)"));
    unit.keyframes.forEach((kf, i) => {
      const box = document.createElement("div");
      box.className = i === selectedKfIndex ? "kf selected" : "kf";
      box.addEventListener("click", (ev) => {
        // Don't hijack clicks on the box's own inputs/buttons — only
        // selecting via the box's empty space (and the text/label chrome).
        if (ev.target instanceof HTMLInputElement || ev.target instanceof HTMLSelectElement || ev.target instanceof HTMLButtonElement) return;
        selectedKfIndex = i;
        render();
      });
      box.append(row(
        labeled("t", numInput(kf.t, (v) => { kf.t = v; render(); })),
        labeled("facing", numInput(kf.facing, (v) => { kf.facing = v; syncMap(); })),
        labeled("strength", numInput(kf.strength, (v) => { kf.strength = v; })),
      ));
      box.append(row(
        labeled("formation", select(FORMATIONS, kf.formation, (v) => { kf.formation = v as Formation; })),
        labeled("confidence", select(CONFIDENCES, kf.confidence ?? "unknown", (v) => { kf.confidence = v as Confidence; render(); })),
      ));
      box.append(labeled("citation", textInput(kf.citation ?? "", (v) => { kf.citation = v || undefined; })));
      box.append(new Text(`local (${kf.x}, ${kf.z})`));
      const del = document.createElement("button");
      del.textContent = "delete keyframe"; del.style.marginLeft = "8px";
      del.addEventListener("click", () => { unit.keyframes.splice(i, 1); render(); });
      box.append(del);
      frag.append(box);
    });
    return frag;
  }

  // small DOM helpers
  function h2(text: string): HTMLHeadingElement { const e = document.createElement("h2"); e.textContent = text; return e; }
  function labeled(text: string, input: HTMLElement): HTMLLabelElement {
    const l = document.createElement("label"); l.textContent = text; l.append(input); return l;
  }
  function textInput(value: string, onChange: (v: string) => void): HTMLInputElement {
    const i = document.createElement("input"); i.type = "text"; i.value = value;
    i.addEventListener("change", () => onChange(i.value)); return i;
  }
  function numInput(value: number, onChange: (v: number) => void): HTMLInputElement {
    const i = document.createElement("input"); i.type = "number"; i.value = String(value);
    i.addEventListener("change", () => onChange(Number(i.value))); return i;
  }
  function select(options: readonly string[], value: string, onChange: (v: string) => void): HTMLSelectElement {
    const s = document.createElement("select");
    for (const o of options) { const opt = document.createElement("option"); opt.value = o; opt.textContent = o; s.append(opt); }
    s.value = value; s.addEventListener("change", () => onChange(s.value)); return s;
  }
  function row(...children: HTMLElement[]): HTMLDivElement {
    const d = document.createElement("div"); d.className = "row"; d.append(...children); return d;
  }
  function fmt(t: number): string {
    const s = Math.floor(battle.startTime + t);
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${pad(Math.floor(s / 3600) % 24)}:${pad(Math.floor(s / 60) % 60)}:${pad(s % 60)}`;
  }

  render();
}
