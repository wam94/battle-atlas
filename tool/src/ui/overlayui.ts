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

  const basemapOpacity = document.createElement("input");
  basemapOpacity.type = "range"; basemapOpacity.min = "0"; basemapOpacity.max = "100"; basemapOpacity.value = "100";

  let imgUrl: string | null = null;
  let imgSize: [number, number] | null = null;
  let imgPts: [number, number][] = [];
  let mapPts: [number, number][] = [];

  // The pending map-click listener for the current tie-point pick, so it can
  // be torn down (map.off) on cancel or when a new overlay load supersedes
  // it — otherwise a stale listener from an abandoned session fires later
  // and corrupts the next session's tie points.
  let pendingMapClickHandler: ((e: maplibregl.MapMouseEvent) => void) | null = null;
  // The Esc handler is registered for the whole picking session (image-pick
  // AND map-pick phases), not just while the modal DOM is present, so Esc
  // cancels a map-pick-phase wait too.
  let escHandler: ((e: KeyboardEvent) => void) | null = null;
  // Set only while the image-pick modal is showing, so cancellation during
  // that phase can remove it from the DOM (no modal exists during the
  // map-pick phase — the modal is already gone by then).
  let activeModal: HTMLDivElement | null = null;

  function endPickingSession(): void {
    pickingActive = false;
    if (pendingMapClickHandler) {
      map.off("click", pendingMapClickHandler);
      pendingMapClickHandler = null;
    }
    if (escHandler) {
      document.removeEventListener("keydown", escHandler);
      escHandler = null;
    }
    if (activeModal) {
      activeModal.remove();
      activeModal = null;
    }
  }

  function cancelPicking(): void {
    endPickingSession();
    status.textContent = "tie-point picking cancelled";
  }

  function startPickingSession(): void {
    // Starting a new session must not leave a previous session's listeners
    // dangling (e.g. author loads a second image before finishing the
    // first's tie points).
    endPickingSession();
    pickingActive = true;
    escHandler = (e: KeyboardEvent) => {
      if (e.key === "Escape") cancelPicking();
    };
    document.addEventListener("keydown", escHandler);
  }

  fileInput.addEventListener("change", async () => {
    const f = fileInput.files?.[0];
    if (!f) return;
    const prevUrl = imgUrl;
    imgUrl = URL.createObjectURL(f);
    if (prevUrl) URL.revokeObjectURL(prevUrl);
    const probe = new Image();
    probe.onload = () => {
      imgSize = [probe.naturalWidth, probe.naturalHeight];
      imgPts = []; mapPts = [];
      startPickingSession();
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
      activeModal = null;
      modal.remove();
      pickMapPoint(n);
    });
    // Backdrop click (on the modal itself, not the image) cancels.
    modal.addEventListener("click", (e) => {
      if (e.target === modal) cancelPicking();
    });
    modal.append(img);
    document.body.append(modal);
    activeModal = modal;
  }

  function pickMapPoint(n: 1 | 2): void {
    status.textContent = `Now click where tie point ${n} really is on the MAP…`;
    const handler = (e: maplibregl.MapMouseEvent) => {
      pendingMapClickHandler = null;
      mapPts.push([e.lngLat.lng, e.lngLat.lat]);
      if (n === 1) {
        pickImagePoint(2);
      } else {
        endPickingSession();
        placeOverlay();
      }
    };
    pendingMapClickHandler = handler;
    map.once("click", handler);
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

  basemapOpacity.addEventListener("input", () => {
    if (map.getLayer("osm"))
      map.setPaintProperty("osm", "raster-opacity", Number(basemapOpacity.value) / 100);
  });

  const h = document.createElement("h2");
  h.textContent = "Historical map overlay";
  const basemapLabel = document.createElement("label");
  basemapLabel.textContent = "modern basemap opacity (dim under period overlays)";
  basemapLabel.append(basemapOpacity);
  container.append(basemapLabel, h, fileInput, status, opacity);
}
