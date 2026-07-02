import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import { exportBattle, importBattle } from "../src/io";

describe("battle IO", () => {
  it("round-trips the placeholder battle structurally (parsed-equal)", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    const out = exportBattle(battle);
    expect(JSON.parse(out)).toEqual(placeholder);
  });

  it("import rejects invalid battles with messages", () => {
    expect(() => importBattle('{"name":"x","startTime":0,"endTime":1,"units":[]}'))
      .toThrow(/minItems|fewer/i);
  });

  it("export refuses an invalid battle", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    battle.units[0]!.keyframes[0]!.confidence = "documented";
    expect(() => exportBattle(battle)).toThrow(/citation/i);
  });

  it("round-trips parent and regiments through export", () => {
    const withFamily = structuredClone(placeholder) as any;
    withFamily.units[3].parent = "atk-a"; // def-a rides atk-a for the test
    withFamily.units[3].regiments = ["1st Test", "2nd Test"]; // children MAY roster
    const battle = importBattle(JSON.stringify(withFamily));
    const out = exportBattle(battle);
    expect(JSON.parse(out)).toEqual(withFamily);
  });

  it("export emits keys in canonical order for clean diffs", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    const out = exportBattle(battle);
    const firstKf = out.indexOf('"keyframes"');
    expect(out.indexOf('"id"')).toBeLessThan(firstKf);
  });
});
