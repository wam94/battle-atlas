import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";

export function initWorkspace(el: HTMLElement, _map: maplibregl.Map, _bf: Battlefield): void {
  el.textContent = "workspace ready";
}
