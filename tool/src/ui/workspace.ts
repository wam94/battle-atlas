import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";
import type { Battle, Confidence, Formation, Side, Unit } from "../model";
import { exportBattle, importBattle } from "../io";
import { validateBattle } from "../validate";
import { battleToGeoJSON, installBattleLayers, previewToGeoJSON } from "./pathlayer";

const FORMATIONS: Formation[] = ["column", "line", "skirmish", "scattered", "routed"];
const CONFIDENCES: Confidence[] = ["unknown", "inferred", "documented"];

export function initWorkspace(el: HTMLElement, map: maplibregl.Map, bf: Battlefield): void {
  let battle: Battle = { name: "untitled battle", startTime: 0, endTime: 3600, units: [] };
  let selectedUnitId: string | null = null;
  let draftTime = 0;

  const installLayers = () => installBattleLayers(map);
  if (map.isStyleLoaded()) installLayers();
  else map.on("load", installLayers);

  map.on("click", (e) => {
    const unit = battle.units.find((u) => u.id === selectedUnitId);
    if (!unit) return;
    const [x, z] = bf.lonLatToLocal(e.lngLat.lng, e.lngLat.lat);
    const lastT = unit.keyframes.length ? unit.keyframes[unit.keyframes.length - 1]!.t : -1;
    unit.keyframes.push({
      t: Math.max(draftTime, lastT + 1),
      x: Math.round(x), z: Math.round(z),
      facing: 90, formation: "line",
      strength: unit.keyframes.length ? unit.keyframes[unit.keyframes.length - 1]!.strength : 1000,
    });
    render();
  });

  function syncMap(): void {
    if (!map.getSource("unit-paths")) return;
    const gj = battleToGeoJSON(battle, bf, selectedUnitId);
    (map.getSource("unit-paths") as maplibregl.GeoJSONSource).setData(gj.paths);
    (map.getSource("unit-dots") as maplibregl.GeoJSONSource).setData(gj.dots);
    (map.getSource("unit-preview") as maplibregl.GeoJSONSource).setData(
      previewToGeoJSON(battle, bf, draftTime),
    );
  }

  function render(): void {
    syncMap();
    el.replaceChildren();
    const frag = document.createDocumentFragment();

    // battle header
    frag.append(h2("Battle"));
    frag.append(labeled("name", textInput(battle.name, (v) => { battle.name = v; })));
    frag.append(row(
      labeled("startTime (s since midnight)", numInput(battle.startTime, (v) => { battle.startTime = v; })),
      labeled("endTime (s)", numInput(battle.endTime, (v) => { battle.endTime = v; render(); })),
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
      btn.addEventListener("click", () => { selectedUnitId = unit.id === selectedUnitId ? null : unit.id; render(); });
      frag.append(btn);
    }
    const addBtn = document.createElement("button");
    addBtn.textContent = "+ add unit";
    addBtn.addEventListener("click", () => {
      const id = `unit-${battle.units.length + 1}`;
      battle.units.push({ id, name: id, side: "union", frontage_m: 200, depth_m: 30, keyframes: [] });
      selectedUnitId = id; render();
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
      } catch (err) { errBox.textContent = String(err); }
    });
    const importInput = document.createElement("input");
    importInput.type = "file"; importInput.accept = ".json";
    importInput.addEventListener("change", async () => {
      const f = importInput.files?.[0];
      if (!f) return;
      try {
        battle = importBattle(await f.text());
        selectedUnitId = null; draftTime = 0;
        errBox.textContent = ""; render();
      } catch (err) { errBox.textContent = String(err); }
    });
    const liveErrors = validateBattle(battle);
    const status = document.createElement("div");
    status.className = liveErrors.ok ? "muted" : "error";
    status.textContent = liveErrors.ok ? "battle is valid" : liveErrors.errors.join("\n");
    frag.append(row(exportBtn), labeled("import battle JSON", importInput), status, errBox);

    el.append(frag);
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
      selectedUnitId = null; render();
    });
    frag.append(del);

    frag.append(h2("Keyframes (click map to append at preview time)"));
    unit.keyframes.forEach((kf, i) => {
      const box = document.createElement("div");
      box.className = "kf";
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
