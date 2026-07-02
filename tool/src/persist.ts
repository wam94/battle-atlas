import type { Battle } from "./model";
import type { Landcover } from "./landcover";

const KEY = "battle-atlas-autosave";
const LANDCOVER_KEY = "battle-atlas-landcover-autosave";

// Autosave slots are scoped to the battlefield's UTM origin so that loading a
// different heightmap doesn't resurrect an autosave whose coordinates belong
// to another battlefield entirely (they'd be silently wrong — same local x/z,
// different UTM anchor). Rounding to the nearest meter keeps the key stable
// against float jitter while still being effectively unique per battlefield.
// `scope` is optional so callers/tests that don't care about multi-battlefield
// isolation (or predate this feature) keep working against the legacy key.
function scopedKey(base: string, originE?: number): string {
  return originE == null ? base : `${base}:${Math.round(originE)}`;
}

// Autosave is a crash net, not a document model: last-write-wins, one slot.
// Export files remain the real persistence (and the only validated artifact).
export function saveAutosave(battle: Battle, originE?: number): void {
  try {
    localStorage.setItem(scopedKey(KEY, originE), JSON.stringify(battle));
  } catch {
    // storage full/unavailable — autosave is best-effort by design
  }
}

// Trailing debounce around saveAutosave for hot call sites (e.g. keyframe
// drag mousemove, which fires 60+/sec). Collapses a burst of rapid calls into
// one write ~500ms after the last call. Deliberate, one-shot actions (import,
// export, finish-feature) should call saveAutosave directly instead so the
// save is immediate and not left pending if the tab closes.
const DEBOUNCE_MS = 500;
const pendingTimers = new Map<string, ReturnType<typeof setTimeout>>();

export function debouncedSaveAutosave(battle: Battle, originE?: number): void {
  const key = scopedKey(KEY, originE);
  const existing = pendingTimers.get(key);
  if (existing != null) clearTimeout(existing);
  pendingTimers.set(
    key,
    setTimeout(() => {
      pendingTimers.delete(key);
      saveAutosave(battle, originE);
    }, DEBOUNCE_MS),
  );
}

// One-time migration: if the scoped slot is empty but the legacy (unscoped)
// key has data, fall back to it. Cheap and kind — an author who was mid-battle
// when this shipped shouldn't lose their autosave just because the key format
// changed underneath them.
export function loadAutosave(originE?: number): Battle | null {
  try {
    const raw = localStorage.getItem(scopedKey(KEY, originE));
    if (raw) return JSON.parse(raw) as Battle;
    if (originE == null) return null;
    const legacy = localStorage.getItem(KEY);
    return legacy ? (JSON.parse(legacy) as Battle) : null;
  } catch {
    return null;
  }
}

export function clearAutosave(originE?: number): void {
  localStorage.removeItem(scopedKey(KEY, originE));
}

// Land cover has its own slot — separate lifecycle from the battle file (see
// landcover.ts), so its autosave must not collide with or clear the battle
// slot above. Same crash-net semantics: last-write-wins, best-effort, scoped
// to battlefield origin with legacy-key migration.
export function saveLandcoverAutosave(landcover: Landcover, originE?: number): void {
  try {
    localStorage.setItem(scopedKey(LANDCOVER_KEY, originE), JSON.stringify(landcover));
  } catch {
    // storage full/unavailable — autosave is best-effort by design
  }
}

// No debounced variant: landcoverui.ts has no hot per-frame call site (its
// syncMap() runs on discrete clicks — vertex placement, finish/cancel,
// import/export — not a mousemove drag loop), so every landcover autosave is
// already a deliberate, low-frequency action.
export function loadLandcoverAutosave(originE?: number): Landcover | null {
  try {
    const raw = localStorage.getItem(scopedKey(LANDCOVER_KEY, originE));
    if (raw) return JSON.parse(raw) as Landcover;
    if (originE == null) return null;
    const legacy = localStorage.getItem(LANDCOVER_KEY);
    return legacy ? (JSON.parse(legacy) as Landcover) : null;
  } catch {
    return null;
  }
}

export function clearLandcoverAutosave(originE?: number): void {
  localStorage.removeItem(scopedKey(LANDCOVER_KEY, originE));
}
