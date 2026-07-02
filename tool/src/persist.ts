import type { Battle } from "./model";
import type { Landcover } from "./landcover";

const KEY = "battle-atlas-autosave";
const LANDCOVER_KEY = "battle-atlas-landcover-autosave";

// Autosave is a crash net, not a document model: last-write-wins, one slot.
// Export files remain the real persistence (and the only validated artifact).
export function saveAutosave(battle: Battle): void {
  try {
    localStorage.setItem(KEY, JSON.stringify(battle));
  } catch {
    // storage full/unavailable — autosave is best-effort by design
  }
}

export function loadAutosave(): Battle | null {
  try {
    const raw = localStorage.getItem(KEY);
    return raw ? (JSON.parse(raw) as Battle) : null;
  } catch {
    return null;
  }
}

export function clearAutosave(): void {
  localStorage.removeItem(KEY);
}

// Land cover has its own slot — separate lifecycle from the battle file (see
// landcover.ts), so its autosave must not collide with or clear the battle
// slot above. Same crash-net semantics: last-write-wins, best-effort.
export function saveLandcoverAutosave(landcover: Landcover): void {
  try {
    localStorage.setItem(LANDCOVER_KEY, JSON.stringify(landcover));
  } catch {
    // storage full/unavailable — autosave is best-effort by design
  }
}

export function loadLandcoverAutosave(): Landcover | null {
  try {
    const raw = localStorage.getItem(LANDCOVER_KEY);
    return raw ? (JSON.parse(raw) as Landcover) : null;
  } catch {
    return null;
  }
}

export function clearLandcoverAutosave(): void {
  localStorage.removeItem(LANDCOVER_KEY);
}
