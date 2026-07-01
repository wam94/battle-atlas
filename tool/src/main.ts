import "./style.css";
import { Battlefield } from "./geo";
import { createMap } from "./ui/map";
import { initWorkspace } from "./ui/workspace";

const fileInput = document.querySelector<HTMLInputElement>("#heightmap-file")!;
const workspaceEl = document.querySelector<HTMLDivElement>("#workspace")!;

fileInput.addEventListener("change", async () => {
  const file = fileInput.files?.[0];
  if (!file) return;
  const meta = JSON.parse(await file.text());
  if (typeof meta.origin_utm_e !== "number" || typeof meta.width_m !== "number") {
    workspaceEl.textContent = "That doesn't look like heightmap.json (missing origin_utm_e/width_m).";
    return;
  }
  const bf = new Battlefield(meta.origin_utm_e, meta.origin_utm_n);
  const map = createMap(document.querySelector("#map")!, bf, meta.width_m);
  initWorkspace(workspaceEl, map, bf);
});
