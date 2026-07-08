import { describe, expect, it } from "vitest";
import {
  addUnitsAfter, fireEvent, frontageHeuristic, moverUnit, staticUnit, WindowEndT,
} from "../scripts/fullcast-lib";
import { validateBattle } from "../src/validate";
import type { Battle, Keyframe } from "../src/model";

describe("fullcast-lib", () => {
  const cite = "War Department tablet via Stone Sentinels (test row)";

  it("staticUnit builds 2 keyframes at identical pose with grade-mapped confidence", () => {
    // A/A− → documented; B+/B → documented (position per tablet, the
    // negative rides the citation text); C → inferred
    for (const [grade, confidence] of [
      ["A", "documented"], ["A-", "documented"],
      ["B+", "documented"], ["B", "documented"], ["C", "inferred"],
    ] as const) {
      const u = staticUnit({
        id: "us-test", name: "Test Brigade", side: "union", strength: 1200,
        x: 4000, z: 5000, facing: 262, grade, citation: cite,
      });
      expect(u.keyframes).toHaveLength(2);
      const [a, b] = u.keyframes as [Keyframe, Keyframe];
      expect(a.t).toBe(0);
      expect(b.t).toBe(WindowEndT);
      // identical pose start to end — static means static
      for (const key of ["x", "z", "facing", "formation", "strength"] as const)
        expect(b[key]).toEqual(a[key]);
      expect(a.confidence).toBe(confidence);
      expect(b.confidence).toBe(confidence);
      expect(a.citation).toBe(cite);
    }
    // frontage heuristic applies when the record doesn't measure the line
    const u = staticUnit({
      id: "us-test-2", name: "Test 2", side: "union", strength: 1000,
      x: 4000, z: 5000, facing: 0, grade: "B", citation: cite,
    });
    expect(u.frontage_m).toBe(frontageHeuristic(1000));
    // an explicit frontage wins over the heuristic
    const v = staticUnit({
      id: "us-test-3", name: "Test 3", side: "union", strength: 1000,
      x: 4000, z: 5000, facing: 0, frontage_m: 90, grade: "B", citation: cite,
    });
    expect(v.frontage_m).toBe(90);
  });

  it("staticUnit enforces the no-faking gate: documented-mapping grades require a citation", () => {
    const row = {
      id: "us-x", name: "X", side: "union" as const, strength: 500,
      x: 4000, z: 5000, facing: 0,
    };
    for (const grade of ["A", "A-", "B+", "B"] as const)
      expect(() => staticUnit({ ...row, grade, citation: "  " })).toThrow(/citation/i);
    // C maps to inferred — no citation demanded (and C rows are normally
    // not authored at all per the survey's do-not-invent list)
    expect(() => staticUnit({ ...row, grade: "C", citation: "" })).not.toThrow();
  });

  it("moverUnit passes explicit keyframes through untouched", () => {
    const keyframes: Keyframe[] = [
      { t: 0, x: 5107, z: 5753, facing: 200, formation: "line", strength: 106,
        confidence: "documented", citation: cite },
      { t: 7200, x: 4400, z: 4600, facing: 262, formation: "line", strength: 104,
        confidence: "documented", citation: cite },
      { t: 10800, x: 4400, z: 4600, facing: 262, formation: "line", strength: 100,
        confidence: "inferred", citation: "end of window (reconstruction)" },
    ];
    const u = moverUnit({
      id: "us-btty-test", name: "Test Battery", side: "union",
      frontage_m: 90, depth_m: 40, keyframes,
    });
    expect(u.keyframes).toEqual(keyframes); // untouched, order and fields intact
    // heuristic fallback reads strength off the first keyframe
    const v = moverUnit({
      id: "us-mover-2", name: "M2", side: "union",
      keyframes: [{ t: 0, x: 1, z: 1, facing: 0, formation: "line", strength: 1000 }],
    });
    expect(v.frontage_m).toBe(frontageHeuristic(1000));
    expect(() => moverUnit({
      id: "us-mover-3", name: "M3", side: "union", keyframes: [],
    })).toThrow(/keyframe/i);
  });

  it("fireEvent builds exactly one emitter form", () => {
    const byUnit = fireEvent({
      id: "ev-a", kind: "artillery_fire", t0: 420, t1: 7200,
      unitId: "csa-bn-alexander", confidence: "documented", citation: cite,
    });
    expect(byUnit.unitId).toBe("csa-bn-alexander");
    for (const k of ["x", "z", "x2", "z2"] as const) expect(byUnit[k]).toBeUndefined();
    const bySegment = fireEvent({
      id: "ev-b", kind: "artillery_fire", t0: 420, t1: 7200,
      segment: { x: 4100, z: 7900, x2: 4300, z2: 7800 },
      confidence: "inferred", note: "detached section at its attested position",
    });
    expect(bySegment.unitId).toBeUndefined();
    expect([bySegment.x, bySegment.z, bySegment.x2, bySegment.z2])
      .toEqual([4100, 7900, 4300, 7800]);
    // one form, never both, never neither
    expect(() => fireEvent({
      id: "ev-c", kind: "musketry", t0: 0, t1: 10,
      unitId: "u", segment: { x: 1, z: 1, x2: 2, z2: 2 },
    })).toThrow(/emitter/i);
    expect(() => fireEvent({ id: "ev-d", kind: "musketry", t0: 0, t1: 10 }))
      .toThrow(/emitter/i);
    // the no-faking gate holds for events too
    expect(() => fireEvent({
      id: "ev-e", kind: "musketry", t0: 0, t1: 10, unitId: "u",
      confidence: "documented",
    })).toThrow(/citation/i);
  });

  it("generator output round-trips validateBattle clean on a minimal fixture", () => {
    const anchor = staticUnit({
      id: "us-anchor", name: "Anchor Brigade", side: "union", strength: 1500,
      x: 4400, z: 4800, facing: 262, grade: "A", citation: cite,
    });
    const added = [
      staticUnit({
        id: "csa-quiet", name: "Quiet Battalion", side: "confederate",
        strength: 300, x: 3700, z: 6900, facing: 82, formation: "line",
        grade: "B", citation: `${cite}; 'Not actively engaged' — the silence IS the citation`,
      }),
      moverUnit({
        id: "us-mover", name: "Moving Battery", side: "union", depth_m: 40,
        keyframes: [
          { t: 0, x: 5107, z: 5753, facing: 200, formation: "column", strength: 110,
            confidence: "documented", citation: cite },
          { t: 7200, x: 4400, z: 4600, facing: 262, formation: "line", strength: 106,
            confidence: "documented", citation: cite },
        ],
      }),
    ];
    const base: Battle = {
      name: "fullcast lib round-trip fixture",
      startTime: 46800,
      endTime: WindowEndT,
      units: [anchor],
      events: [
        fireEvent({
          id: "ev-mover-fire", kind: "artillery_fire", t0: 7200, t1: 10680,
          unitId: "us-mover", confidence: "documented", citation: cite,
        }),
        fireEvent({
          id: "ev-detached", kind: "artillery_fire", t0: 420, t1: 7200,
          segment: { x: 4100, z: 7900, x2: 4300, z2: 7800 }, confidence: "inferred",
        }),
      ],
    };
    const battle = addUnitsAfter(base, "us-anchor", added);
    expect(battle.units.map((u) => u.id)).toEqual(["us-anchor", "csa-quiet", "us-mover"]);
    const result = validateBattle(battle);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
    // the lib never re-validates — bad anchors and duplicate ids throw loud
    expect(() => addUnitsAfter(base, "no-such-anchor", added)).toThrow(/no-such-anchor/);
    expect(() => addUnitsAfter(battle, "us-anchor", [anchor])).toThrow(/us-anchor/);
  });
});
