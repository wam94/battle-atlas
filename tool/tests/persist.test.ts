import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import placeholderLandcover from "./fixtures/placeholder_landcover.json";
import {
  loadAutosave, saveAutosave, clearAutosave, debouncedSaveAutosave, flushAutosaves,
  loadLandcoverAutosave, saveLandcoverAutosave, clearLandcoverAutosave,
} from "../src/persist";

// Drain the module-level debounce map between tests so a timer armed in one
// test can't fire (or linger) into the next.
afterEach(() => {
  flushAutosaves();
});

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

describe("landcover autosave", () => {
  it("round-trips a landcover", () => {
    saveLandcoverAutosave(placeholderLandcover as never);
    expect(loadLandcoverAutosave()).toEqual(placeholderLandcover);
  });

  it("returns null when empty or corrupt", () => {
    expect(loadLandcoverAutosave()).toBeNull();
    localStorage.setItem("battle-atlas-landcover-autosave", "{not json");
    expect(loadLandcoverAutosave()).toBeNull();
  });

  it("clear removes the save", () => {
    saveLandcoverAutosave(placeholderLandcover as never);
    clearLandcoverAutosave();
    expect(loadLandcoverAutosave()).toBeNull();
  });

  it("saving landcover does not clobber the battle slot, and vice versa", () => {
    saveAutosave(placeholder as never);
    saveLandcoverAutosave(placeholderLandcover as never);
    expect(loadAutosave()).toEqual(placeholder);
    expect(loadLandcoverAutosave()).toEqual(placeholderLandcover);

    clearLandcoverAutosave();
    expect(loadAutosave()).toEqual(placeholder);
    expect(loadLandcoverAutosave()).toBeNull();
  });
});

describe("debouncedSaveAutosave", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });
  afterEach(() => {
    vi.useRealTimers();
  });

  it("collapses rapid calls into a single write after the debounce window", () => {
    for (let i = 0; i < 60; i++) {
      debouncedSaveAutosave({ ...placeholder, name: `tick-${i}` } as never);
    }
    // nothing written yet — still within the debounce window
    expect(loadAutosave()).toBeNull();
    vi.advanceTimersByTime(500);
    const saved = loadAutosave();
    expect(saved).not.toBeNull();
    expect((saved as unknown as { name: string }).name).toBe("tick-59");
  });

  it("does not write before the debounce window elapses", () => {
    debouncedSaveAutosave(placeholder as never);
    vi.advanceTimersByTime(499);
    expect(loadAutosave()).toBeNull();
  });

  it("saveAutosave (immediate) still writes synchronously, unaffected by pending debounce", () => {
    debouncedSaveAutosave({ ...placeholder, name: "debounced" } as never);
    saveAutosave(placeholder as never);
    expect(loadAutosave()).toEqual(placeholder);
  });

  it("a synchronous save cancels the pending debounced save for the same slot", () => {
    debouncedSaveAutosave({ ...placeholder, name: "state-a" } as never);
    saveAutosave({ ...placeholder, name: "state-b" } as never);
    // if the stale timer survived, it would now clobber state-b with state-a
    vi.advanceTimersByTime(500);
    const saved = loadAutosave();
    expect((saved as unknown as { name: string }).name).toBe("state-b");
  });

  it("flushAutosaves writes pending saves immediately and disarms their timers", () => {
    debouncedSaveAutosave({ ...placeholder, name: "pending" } as never);
    expect(loadAutosave()).toBeNull();
    flushAutosaves();
    const saved = loadAutosave();
    expect((saved as unknown as { name: string }).name).toBe("pending");
    // the flushed timer must not fire a second, stale write later
    saveAutosave({ ...placeholder, name: "after-flush" } as never);
    vi.advanceTimersByTime(500);
    expect((loadAutosave() as unknown as { name: string }).name).toBe("after-flush");
  });
});

describe("autosave scoping by battlefield origin", () => {
  it("two origins produce independent slots", () => {
    const battleA = { ...placeholder, name: "battle-a" } as never;
    const battleB = { ...placeholder, name: "battle-b" } as never;
    saveAutosave(battleA as never, 500000);
    saveAutosave(battleB as never, 600000);
    expect(loadAutosave(500000)).toEqual(battleA);
    expect(loadAutosave(600000)).toEqual(battleB);
  });

  it("rounds origin to the nearest integer meter for key stability", () => {
    saveAutosave(placeholder as never, 500000.4);
    expect(loadAutosave(500000.49)).toEqual(placeholder);
  });

  it("falls back to the legacy unscoped key when the scoped slot is empty", () => {
    saveAutosave(placeholder as never); // legacy, unscoped write
    expect(loadAutosave(500000)).toEqual(placeholder);
  });

  it("migrates legacy data to the first scoped reader and deletes the legacy key", () => {
    saveAutosave(placeholder as never); // legacy, unscoped write
    expect(loadAutosave(500000)).toEqual(placeholder); // first reader claims it
    expect(localStorage.getItem("battle-atlas-autosave")).toBeNull();
    expect(loadAutosave(500000)).toEqual(placeholder); // now served from its own slot
    expect(loadAutosave(600000)).toBeNull(); // no cross-battlefield inheritance
  });

  it("migrates legacy landcover data the same way", () => {
    saveLandcoverAutosave(placeholderLandcover as never); // legacy, unscoped write
    expect(loadLandcoverAutosave(500000)).toEqual(placeholderLandcover);
    expect(localStorage.getItem("battle-atlas-landcover-autosave")).toBeNull();
    expect(loadLandcoverAutosave(600000)).toBeNull();
  });

  it("prefers the scoped slot over the legacy key once it has its own data", () => {
    const legacyBattle = { ...placeholder, name: "legacy" } as never;
    const scopedBattle = { ...placeholder, name: "scoped" } as never;
    saveAutosave(legacyBattle as never); // legacy key
    saveAutosave(scopedBattle as never, 500000); // scoped key
    expect(loadAutosave(500000)).toEqual(scopedBattle);
  });

  it("scopes the landcover slot the same way, with legacy fallback", () => {
    const lcA = { ...placeholderLandcover, name: "lc-a" } as never;
    const lcB = { ...placeholderLandcover, name: "lc-b" } as never;
    saveLandcoverAutosave(lcA as never, 500000);
    saveLandcoverAutosave(lcB as never, 600000);
    expect(loadLandcoverAutosave(500000)).toEqual(lcA);
    expect(loadLandcoverAutosave(600000)).toEqual(lcB);

    saveLandcoverAutosave(placeholderLandcover as never); // legacy key
    expect(loadLandcoverAutosave(700000)).toEqual(placeholderLandcover);
  });
});
