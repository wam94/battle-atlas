# Authoring Polish Implementation Plan (Battle Atlas, Phase 3.5)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the friction that would tax hours of real authoring sessions: autosave, keyframe drag/selection, sane time entry, a dim-basemap control, and the small leaks/hygiene items from the Phase 3 final review.

**Architecture:** All changes inside `tool/` (no format, pipeline, or Unity changes). Pure logic additions get Vitest coverage; DOM/map wiring stays thin. Baseline: 29 tests green, typecheck clean.

**Tech Stack:** unchanged (Vite + TS + Vitest, MapLibre).

**Branch:** `authoring-polish`.

**Source findings (Phase 3 final review):** autosave absence, orphaned `.kf.selected` CSS, `URL.createObjectURL` never revoked, overlay modal lacks cancel, stale `map.once` listener across overlay sessions, endTime shrink doesn't clamp draftTime, second heightmap load stacks maps, `origin_utm_n` unguarded, keyframe-append heading inaccurate.

---

### Task 1: Autosave + restore (localStorage)

**Files:**
- Create: `tool/src/persist.ts`
- Modify: `tool/src/ui/workspace.ts`
- Test: `tool/tests/persist.test.ts`

- [ ] **Step 1: Failing tests** — `tool/tests/persist.test.ts`:

```typescript
import { beforeEach, describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import { loadAutosave, saveAutosave, clearAutosave } from "../src/persist";

// vitest node environment: provide a minimal localStorage
beforeEach(() => {
  const store = new Map<string, string>();
  globalThis.localStorage = {
    getItem: (k: string) => store.get(k) ?? null,
    setItem: (k: string, v: string) => void store.set(k, v),
    removeItem: (k: string) => void store.delete(k),
    clear: () => store.clear(),
    key: () => null,
    get length() { return store.size; },
  } as Storage;
});

describe("autosave", () => {
  it("round-trips a battle", () => {
    saveAutosave(placeholder as never);
    expect(loadAutosave()).toEqual(placeholder);
  });

  it("returns null when empty or corrupt", () => {
    expect(loadAutosave()).toBeNull();
    localStorage.setItem("battle-atlas-autosave", "{not json");
    expect(loadAutosave()).toBeNull();
  });

  it("clear removes the save", () => {
    saveAutosave(placeholder as never);
    clearAutosave();
    expect(loadAutosave()).toBeNull();
  });
});
```

- [ ] **Step 2:** Run focused test — fails (module missing).

- [ ] **Step 3: Implement** — `tool/src/persist.ts`:

```typescript
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
```

- [ ] **Step 4: Wire into workspace.ts.** In `initWorkspace`: on startup, if `loadAutosave()` returns a battle, restore it (battle = saved) and show a dismissible status line "restored autosave — Export to keep it". Debounce-save on every `render()` and on the scrubber input (simple: `saveAutosave(battle)` at the top of `syncMap()` — it's called on every mutation path and is cheap at this data size). Import replaces the autosave; successful export calls `saveAutosave` too (not `clearAutosave` — the save should always mirror the screen).

- [ ] **Step 5:** Full suite green (expect 32), typecheck clean, commit `feat: autosave battles to localStorage`.

---

### Task 2: Keyframe selection + drag-to-move on the map

**Files:**
- Modify: `tool/src/ui/workspace.ts`, `tool/src/ui/pathlayer.ts`, `tool/src/style.css`
- Test: append to `tool/tests/pathlayer.test.ts`

- [ ] **Step 1: Failing test** — append:

```typescript
describe("selected keyframe styling", () => {
  it("marks the selected keyframe dot", () => {
    const gj = battleToGeoJSON(placeholder as any, bf, "atk-a", 2);
    const marked = gj.dots.features.filter((f) => f.properties!.kfSelected);
    expect(marked.length).toBe(1);
    expect(marked[0]!.properties!.index).toBe(2);
    expect(marked[0]!.properties!.unitId).toBe("atk-a");
  });
});
```

- [ ] **Step 2:** `battleToGeoJSON` gains an optional 4th param `selectedKfIndex: number | null = null`; dots get `kfSelected: unit.id === selectedUnitId && i === selectedKfIndex`. The dots layer paint gets a larger radius + gold stroke when `kfSelected` (`["case", ["get", "kfSelected"], ...]`). Existing calls pass the new arg.

- [ ] **Step 3: Workspace wiring.** Closure state `selectedKfIndex: number | null`. Clicking a keyframe's box in the sidebar selects it (`.kf.selected` class finally earns its keep). Map interactions:
  - `map.on("mousedown", "unit-dots", ...)`: if the dot belongs to the selected unit, select that keyframe index, disable map dragPan, and start a drag; on `mousemove` update that keyframe's x/z (`bf.lonLatToLocal`, rounded); on `mouseup` re-enable dragPan and `render()`.
  - Plain map click (not on a dot) with a unit selected keeps appending, unchanged. The existing tie-point guard stays first.
  - Cursor feedback: `map.on("mouseenter", "unit-dots")` → `move` cursor when over the selected unit's dots; `mouseleave` restores.

- [ ] **Step 4:** Suite green (expect 33), typecheck, dev-server smoke, commit `feat: keyframe selection and drag-to-move`.

---

### Task 3: Time entry sanity + endTime clamp

**Files:**
- Modify: `tool/src/ui/workspace.ts`
- Test: create `tool/tests/timeutil.test.ts` + create `tool/src/timeutil.ts`

- [ ] **Step 1:** Extract the append-time rule into a pure function and improve it — `tool/src/timeutil.ts`:

```typescript
// Where a new keyframe lands on the time axis: at the preview time if that's
// after the unit's last keyframe, else nudged past the end. Pure so the rule
// is testable and documented in one place.
export function nextKeyframeTime(draftTime: number, lastT: number | null, endTime: number): number {
  const t = lastT === null ? draftTime : Math.max(draftTime, lastT + 1);
  return Math.min(t, endTime);
}

export function clampDraftTime(draftTime: number, endTime: number): number {
  return Math.min(Math.max(0, draftTime), endTime);
}
```

Tests (`tool/tests/timeutil.test.ts`): first keyframe lands exactly at draftTime (even 0); subsequent ones use max(draft, last+1); both clamp to endTime; clampDraftTime bounds both ends.

```typescript
import { describe, expect, it } from "vitest";
import { clampDraftTime, nextKeyframeTime } from "../src/timeutil";

describe("nextKeyframeTime", () => {
  it("first keyframe lands at the preview time", () => {
    expect(nextKeyframeTime(120, null, 3600)).toBe(120);
  });
  it("later keyframes never go backwards", () => {
    expect(nextKeyframeTime(100, 200, 3600)).toBe(201);
    expect(nextKeyframeTime(500, 200, 3600)).toBe(500);
  });
  it("clamps to endTime", () => {
    expect(nextKeyframeTime(4000, 3999, 3600)).toBe(3600);
  });
});

describe("clampDraftTime", () => {
  it("bounds both ends", () => {
    expect(clampDraftTime(-5, 3600)).toBe(0);
    expect(clampDraftTime(9999, 3600)).toBe(3600);
    expect(clampDraftTime(50, 3600)).toBe(50);
  });
});
```

- [ ] **Step 2:** Workspace uses `nextKeyframeTime` in the map-click append; the endTime input's change handler runs `draftTime = clampDraftTime(draftTime, battle.endTime)` before `render()`. Fix the keyframes heading to match reality: "Keyframes (click map to add at preview time)".

- [ ] **Step 3:** Suite green (expect 37), typecheck, commit `fix: keyframe time rules extracted, endTime clamps preview`.

---

### Task 4: Dim-basemap control + overlay modal cancel + leak hygiene

**Files:**
- Modify: `tool/src/ui/overlayui.ts`, `tool/src/ui/map.ts`, `tool/src/ui/workspace.ts`, `tool/src/main.ts`

- [ ] **Step 1: Dim basemap.** In the overlay section (stable, survives re-renders — right place for map-wide display controls), add a "modern basemap" opacity slider (default 100) driving `map.setPaintProperty("osm", "raster-opacity", v/100)`. Label it so the author understands the intent ("dim the modern map under period overlays").

- [ ] **Step 2: Overlay modal cancel.** Esc key and backdrop click close the modal and abort the whole picking session (`pickingActive = false`, status "tie-point picking cancelled"). Remove any pending `map.once("click", ...)` listener when cancelling or when starting a NEW overlay load (`map.off("click", handler)` — name the handler so it can be removed; this also fixes the stale-listener carryover flagged in review).

- [ ] **Step 3: Object URL hygiene.** `overlayui.ts`: revoke the previous `imgUrl` when a new file loads AND keep the current one alive while the overlay uses it (revoke only the replaced one). `workspace.ts` export: `URL.revokeObjectURL(a.href)` after `a.click()`.

- [ ] **Step 4: Heightmap reload guard.** `main.ts`: validate `origin_utm_n` and `depth_m` alongside the existing checks; on a second file load, `map.remove()` the previous MapLibre instance before creating the new one (keep it simple: store the instance in a module-level `let`).

- [ ] **Step 5:** Suite green (37), typecheck, dev-server smoke, commit `fix: basemap dimming, overlay cancel, url + map instance hygiene`.

---

### Task 5: Live verification + close out

- [ ] Browser smoke via the Chrome extension (controller): load heightmap, add unit, click keyframes, drag one, scrub, dim basemap, cancel an overlay pick with Esc, reload the page → autosave restores. Console clean.
- [ ] README: one line under the tool section noting autosave exists but exports are the durable artifact.
- [ ] Commit, merge to main per standing pattern after review, tag `phase3.5-authoring-polish`.

## Done =

An author can spend a multi-hour session in the tool without losing work, fighting the time axis, or squinting past modern airports — the friction list from the Phase 3 final review is cleared.
