import { describe, expect, it } from "vitest";
import manifest from "../../app/Assets/StreamingAssets/Atlas/battle-manifest.json";
import battle from "../../app/Assets/Battle/gettysburg-july3.json";
import { validateManifest, type BattleManifest } from "../src/validate";

// The day/phase manifest (ADR 0005; docs/format/battle-manifest.md): the
// committed instance validates, its honesty rules hold, and — the rule the
// pure validator cannot check — every reconstructed phase's clock is a true
// ECHO of its battle file's own startTime/endTime.
describe("battle manifest (ADR 0005)", () => {
  const m = manifest as BattleManifest;

  it("passes schema + rule validation", () => {
    const result = validateManifest(manifest);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });

  it("carries the three days in date order with July 3 afternoon reconstructed", () => {
    expect(m.days.map((d) => d.id)).toEqual(["july1", "july2", "july3"]);
    const july3 = m.days[2]!;
    expect(july3.phases.map((p) => p.status)).toEqual([
      "not-reconstructed", "reconstructed",
    ]);
    expect(july3.phases[1]!.battle).toBe("gettysburg-july3.json");
  });

  it("cross-file: reconstructed phase clocks echo the battle file exactly", () => {
    // Only one battle file exists today; when a second phase is
    // reconstructed, extend this lookup (the manifest may never lie about a
    // phase's clock — battle-manifest.md "The honesty rules").
    const battles: Record<string, { startTime: number; endTime: number }> = {
      "gettysburg-july3.json": battle as any,
    };
    for (const day of m.days)
      for (const phase of day.phases) {
        if (phase.status !== "reconstructed") continue;
        const b = battles[phase.battle!];
        expect(b, `manifest names unknown battle file ${phase.battle}`).toBeDefined();
        expect(phase.startTime).toBe(b!.startTime);
        expect(phase.endTime).toBe(b!.endTime);
      }
  });

  it("empty days are honest: not-reconstructed phases carry a note, never a battle", () => {
    for (const day of m.days)
      for (const phase of day.phases) {
        if (phase.status !== "not-reconstructed") continue;
        expect(phase.note ?? "").not.toBe("");
        expect(phase.battle).toBeUndefined();
      }
  });

  it("rejects a manifest that lies (battle on a not-reconstructed phase; duplicate ids; date disorder)", () => {
    const lying = JSON.parse(JSON.stringify(manifest));
    lying.days[0].phases[0].battle = "gettysburg-july3.json";
    expect(validateManifest(lying).ok).toBe(false);

    const dup = JSON.parse(JSON.stringify(manifest));
    dup.days[1].id = dup.days[0].id;
    expect(validateManifest(dup).ok).toBe(false);
    expect(validateManifest(dup).errors.join(" ")).toContain("duplicate day id");

    const disorder = JSON.parse(JSON.stringify(manifest));
    disorder.days[1].date = "1863-06-30";
    expect(validateManifest(disorder).ok).toBe(false);
    expect(validateManifest(disorder).errors.join(" ")).toContain("strict date order");
  });

  it("rejects overlapping reconstructed phases within a day", () => {
    const overlap = JSON.parse(JSON.stringify(manifest));
    const afternoon = overlap.days[2].phases[1];
    overlap.days[2].phases[0] = {
      id: "july3-morning", label: "Morning", status: "reconstructed",
      battle: afternoon.battle, startTime: afternoon.startTime,
      endTime: afternoon.endTime,
    };
    const result = validateManifest(overlap);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("overlap");
  });
});
