import { beforeEach, describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import placeholderLandcover from "./fixtures/placeholder_landcover.json";
import {
  loadAutosave, saveAutosave, clearAutosave,
  loadLandcoverAutosave, saveLandcoverAutosave, clearLandcoverAutosave,
} from "../src/persist";

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
