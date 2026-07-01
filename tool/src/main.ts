import "./style.css";
import type maplibregl from "maplibre-gl";
import { Battlefield } from "./geo";
import { createMap } from "./ui/map";
import { initWorkspace } from "./ui/workspace";

const fileInput = document.querySelector<HTMLInputElement>("#heightmap-file")!;
const workspaceEl = document.querySelector<HTMLDivElement>("#workspace")!;

// Loading a second heightmap.json in the same session must not stack a new
// MapLibre instance on top of the previous one (duplicate WebGL contexts,
// duplicate event listeners) — tear down the old map before creating a
// replacement.
let currentMap: maplibregl.Map | null = null;

fileInput.addEventListener("change", async () => {
  const file = fileInput.files?.[0];
  if (!file) return;
  const meta = JSON.parse(await file.text());
  if (
    typeof meta.origin_utm_e !== "number" || typeof meta.width_m !== "number" ||
    typeof meta.origin_utm_n !== "number" || typeof meta.depth_m !== "number"
  ) {
    workspaceEl.textContent =
      "That doesn't look like heightmap.json (missing origin_utm_e/origin_utm_n/width_m/depth_m).";
    return;
  }
  if (currentMap) {
    currentMap.remove();
    currentMap = null;
  }
  const bf = new Battlefield(meta.origin_utm_e, meta.origin_utm_n);
  const map = createMap(document.querySelector("#map")!, bf, meta.width_m);
  currentMap = map;
  initWorkspace(workspaceEl, map, bf);
});
