import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import { validateBattle } from "../src/validate";
import { exportBattle, importBattle } from "../src/io";

// Engagement events + environment (battle-format.md "Engagement events" /
// "Environment"): optional top-level passthrough, provenance-gated exactly
// like keyframes. Exactly one emitter form per event — `unitId` (moving
// emitter, position from that unit's track) XOR the x/z/x2/x2 fixed segment.

// A valid battle carrying one event of each emitter form plus wind.
function withEvents(): any {
  const b = structuredClone(placeholder) as any;
  b.events = [
    {
      id: "ev-guns",
      kind: "artillery_fire",
      t0: 400,
      t1: 2000,
      x: 2000, z: 3000, x2: 3500, z2: 3200,
      confidence: "documented",
      citation: "Test source, p. 1",
    },
    {
      id: "ev-musket",
      kind: "musketry",
      t0: 2400,
      t1: 3000,
      unitId: "def-a",
      confidence: "inferred",
      note: "synthetic test window",
    },
  ];
  b.environment = {
    windTowardDeg: 45,
    windMps: 2,
    confidence: "inferred",
    note: "synthetic test wind",
  };
  return b;
}

describe("engagement events + environment", () => {
  it("accepts valid events and environment, and round-trips them sorted by t0 then id", () => {
    const data = withEvents();
    const result = validateBattle(data);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);

    // export/import round-trip: canonical order is t0 ascending, id tiebreak
    data.events.reverse(); // hand the exporter an unsorted array
    const battle = importBattle(JSON.stringify(data));
    const out = exportBattle(battle);
    const parsed = JSON.parse(out);
    expect(parsed.events.map((e: any) => e.id)).toEqual(["ev-guns", "ev-musket"]);
    expect(parsed.events[0]).toEqual(withEvents().events[0]);
    expect(parsed.environment).toEqual(withEvents().environment);
  });

  it("rejects duplicate event ids", () => {
    const bad = withEvents();
    bad.events[1].id = bad.events[0].id;
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("duplicate event id");
  });

  it("rejects windows that don't satisfy t0 < t1 <= endTime", () => {
    const empty = withEvents();
    empty.events[0].t1 = empty.events[0].t0; // empty window
    expect(validateBattle(empty).ok).toBe(false);

    const overlong = withEvents();
    overlong.events[0].t1 = overlong.endTime + 1; // past the battle's end
    const result = validateBattle(overlong);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("endTime");
  });

  it("rejects an event whose unitId doesn't reference an existing unit", () => {
    const bad = withEvents();
    bad.events[1].unitId = "no-such-unit";
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("no-such-unit");
  });

  it("enforces the no-faking gate on events: documented requires citation", () => {
    const bad = withEvents();
    delete bad.events[0].citation;
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toMatch(/citation/i);
  });

  it("rejects an event carrying both emitter forms (unitId AND segment)", () => {
    const bad = withEvents();
    bad.events[0].unitId = "atk-a"; // segment event also claims a unit
    expect(validateBattle(bad).ok).toBe(false);

    const partial = withEvents();
    delete partial.events[0].x2; // half a segment is no emitter either
    expect(validateBattle(partial).ok).toBe(false);
  });
});
