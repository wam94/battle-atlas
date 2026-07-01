import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";
import { applySimilarity, similarityFromTwoPoints, type TiePoint } from "../overlay";

// True while a tie-point pick is in progress, so the workspace's map-click
// handler doesn't ALSO drop a keyframe on the selected unit when the author
// clicks a tie point's true location.
let pickingActive = false;
export function isPickingTiePoint(): boolean {
  return pickingActive;
}

// Workflow: load a scanned map image; click two points ON THE IMAGE (shown in
// a modal canvas), then their true locations ON THE MAP; the similarity
// transform places the image as a raster source with adjustable opacity.
export function initOverlayUI(
  container: HTMLElement, map: maplibregl.Map, bf: Battlefield,
): void {
  const fileInput = document.createElement("input");
  fileInput.type = "file"; fileInput.accept = "image/*";
  const status = document.createElement("div"); status.className = "muted";
  const opacity = document.createElement("input");
  opacity.type = "range"; opacity.min = "0"; opacity.max = "100"; opacity.value = "60";

  let imgUrl: string | null = null;
  let imgSize: [number, number] | null = null;
  let imgPts: [number, number][] = [];
  let mapPts: [number, number][] = [];

  fileInput.addEventListener("change", async () => {
    const f = fileInput.files?.[0];
    if (!f) return;
    imgUrl = URL.createObjectURL(f);
    const probe = new Image();
    probe.onload = () => {
      imgSize = [probe.naturalWidth, probe.naturalHeight];
      imgPts = []; mapPts = [];
      pickingActive = true;
      pickImagePoint(1);
    };
    probe.src = imgUrl;
  });

  function pickImagePoint(n: 1 | 2): void {
    status.textContent = `Click tie point ${n} on the IMAGE…`;
    const modal = document.createElement("div");
    modal.style.cssText =
      "position:fixed;inset:0;background:rgba(0,0,0,.7);display:flex;align-items:center;justify-content:center;z-index:10";
    const img = new Image();
    img.src = imgUrl!;
    img.style.cssText = "max-width:90vw;max-height:90vh;cursor:crosshair";
    img.addEventListener("click", (e) => {
      const r = img.getBoundingClientRect();
      const sx = imgSize![0] / r.width;
      const sy = imgSize![1] / r.height;
      imgPts.push([(e.clientX - r.left) * sx, (e.clientY - r.top) * sy]);
      modal.remove();
      pickMapPoint(n);
    });
    modal.append(img);
    document.body.append(modal);
  }

  function pickMapPoint(n: 1 | 2): void {
    status.textContent = `Now click where tie point ${n} really is on the MAP…`;
    map.once("click", (e) => {
      mapPts.push([e.lngLat.lng, e.lngLat.lat]);
      if (n === 1) {
        pickImagePoint(2);
      } else {
        pickingActive = false;
        placeOverlay();
      }
    });
  }

  function placeOverlay(): void {
    const ties: TiePoint[] = imgPts.map((img, i) => ({
      img: img as [number, number],
      local: bf.lonLatToLocal(mapPts[i]![0], mapPts[i]![1]),
    }));
    const T = similarityFromTwoPoints(ties[0]!, ties[1]!);
    const [w, h] = imgSize!;
    const cornersImg: [number, number][] = [[0, 0], [w, 0], [w, h], [0, h]];
    const coords = cornersImg.map((c) =>
      bf.localToLonLat(...applySimilarity(T, c)),
    ) as [[number, number], [number, number], [number, number], [number, number]];
    if (map.getLayer("hist-overlay")) { map.removeLayer("hist-overlay"); map.removeSource("hist-overlay"); }
    map.addSource("hist-overlay", { type: "image", url: imgUrl!, coordinates: coords });
    map.addLayer(
      { id: "hist-overlay", type: "raster", source: "hist-overlay", paint: { "raster-opacity": 0.6 } },
      "unit-preview", // keep overlays under the authored content
    );
    status.textContent = "Overlay placed. Adjust opacity below; reload the image to redo tie points.";
  }

  opacity.addEventListener("input", () => {
    if (map.getLayer("hist-overlay"))
      map.setPaintProperty("hist-overlay", "raster-opacity", Number(opacity.value) / 100);
  });

  const h = document.createElement("h2");
  h.textContent = "Historical map overlay";
  container.append(h, fileInput, status, opacity);
}
