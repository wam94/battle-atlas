import { describe, expect, it } from "vitest";
import manifest from "../../app/Assets/StreamingAssets/Atlas/battle-manifest.json";
import battle from "../../app/Assets/Battle/gettysburg-july3.json";
import july3Morning from "../../app/Assets/Battle/gettysburg-july3-morning.json";
import july2Afternoon from "../../app/Assets/Battle/gettysburg-july2-afternoon.json";
import july2Evening from "../../app/Assets/Battle/gettysburg-july2-evening.json";
import july1Morning from "../../app/Assets/Battle/gettysburg-july1-morning.json";
import july1Afternoon from "../../app/Assets/Battle/gettysburg-july1-afternoon.json";
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

  it("carries the three days in date order with all three days reconstructed", () => {
    expect(m.days.map((d) => d.id)).toEqual(["july1", "july2", "july3"]);
    // July 1 (day-expansion slice 3): two abutting reconstructed phases
    // seamed at the midday lull (13:00 LMT), plus the honest evening note
    // (CA-J1P-9 is sequence-only — no clocked chain covers the evening).
    const july1 = m.days[0]!;
    expect(july1.phases.map((p) => p.status)).toEqual([
      "reconstructed", "reconstructed", "not-reconstructed",
    ]);
    expect(july1.phases[0]!.battle).toBe("gettysburg-july1-morning.json");
    expect(july1.phases[1]!.battle).toBe("gettysburg-july1-afternoon.json");
    // abutting, never overlapping: morning [27000, 46800) meets afternoon
    // [46800, 64800) exactly at the lull seam
    expect(july1.phases[0]!.startTime! + july1.phases[0]!.endTime!)
      .toBe(july1.phases[1]!.startTime);
    // July 2 (day-expansion slice 2): honest morning + two abutting
    // reconstructed phases seamed at sunset 19:29 LMT (ED-31/CA-J2A-11).
    const july2 = m.days[1]!;
    expect(july2.phases.map((p) => p.status)).toEqual([
      "not-reconstructed", "reconstructed", "reconstructed",
    ]);
    expect(july2.phases[1]!.battle).toBe("gettysburg-july2-afternoon.json");
    expect(july2.phases[2]!.battle).toBe("gettysburg-july2-evening.json");
    // abutting, never overlapping: afternoon [55800, 70140) meets evening
    // [70140, 81000) exactly at the sunset pin
    expect(july2.phases[1]!.startTime! + july2.phases[1]!.endTime!)
      .toBe(july2.phases[2]!.startTime);
    // July 3 (this slice): two abutting reconstructed phases seamed at
    // 13:00 LMT (the Culp's Hill morning fight's window end / the shipped
    // afternoon file's unchanged startTime).
    const july3 = m.days[2]!;
    expect(july3.phases.map((p) => p.status)).toEqual([
      "reconstructed", "reconstructed",
    ]);
    expect(july3.phases[0]!.battle).toBe("gettysburg-july3-morning.json");
    expect(july3.phases[1]!.battle).toBe("gettysburg-july3.json");
    // abutting, never overlapping: morning [16200, 46800) meets afternoon
    // [46800, 70140) exactly at 13:00 LMT — the afternoon file's startTime
    // is UNCHANGED (film-safety: 46800 was already the shipped value).
    expect(july3.phases[0]!.startTime! + july3.phases[0]!.endTime!)
      .toBe(july3.phases[1]!.startTime);
  });

  it("cross-file: reconstructed phase clocks echo the battle file exactly", () => {
    // Every reconstructed phase's battle file is in this lookup (the
    // manifest may never lie about a phase's clock — battle-manifest.md
    // "The honesty rules").
    const battles: Record<string, { startTime: number; endTime: number }> = {
      "gettysburg-july3.json": battle as any,
      "gettysburg-july3-morning.json": july3Morning as any,
      "gettysburg-july2-afternoon.json": july2Afternoon as any,
      "gettysburg-july2-evening.json": july2Evening as any,
      "gettysburg-july1-morning.json": july1Morning as any,
      "gettysburg-july1-afternoon.json": july1Afternoon as any,
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
    // july2-morning is the not-reconstructed phase (july1's first phase is
    // reconstructed since day-expansion slice 3)
    lying.days[1].phases[0].battle = "gettysburg-july3.json";
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
