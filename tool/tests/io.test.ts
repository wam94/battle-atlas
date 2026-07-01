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

  it("export emits keys in canonical order for clean diffs", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    const out = exportBattle(battle);
    const firstKf = out.indexOf('"keyframes"');
    expect(out.indexOf('"id"')).toBeLessThan(firstKf);
  });
});
