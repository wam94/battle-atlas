import type { Battle } from "./model";

const KEY = "battle-atlas-autosave";

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
