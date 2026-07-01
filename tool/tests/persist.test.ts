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
