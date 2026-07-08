import { describe, expect, it } from "vitest";
import battle from "../../app/Assets/Battle/gettysburg-july3.json";
import { validateBattle } from "../src/validate";

describe("authored July 3 battle", () => {
  it("passes schema + rule validation", () => {
    const result = validateBattle(battle);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });
  it("keeps every keyframe on the battlefield", () => {
    for (const u of (battle as any).units)
      for (const k of u.keyframes) {
        expect(k.x).toBeGreaterThan(0); expect(k.x).toBeLessThan(8507);
        expect(k.z).toBeGreaterThan(0); expect(k.z).toBeLessThan(8507);
      }
  });
  it("every documented keyframe carries a citation", () => {
    for (const u of (battle as any).units)
      for (const k of u.keyframes)
        if (k.confidence === "documented") expect(k.citation?.trim()).toBeTruthy();
  });
  it("every regiment roster has at least 2 unique entries", () => {
    for (const u of (battle as any).units) {
      if (u.regiments === undefined) continue;
      expect(u.regiments.length).toBeGreaterThanOrEqual(2);
      expect(new Set(u.regiments).size).toBe(u.regiments.length);
    }
  });
  it("A-grade brigades are fully decomposed: children with valid parents, rosters gone", () => {
    const units = (battle as any).units;
    const childrenOf = (p: string) =>
      units.filter((u: any) => u.parent === p).map((u: any) => u.id).sort();
    expect(childrenOf("us-stannard")).toEqual(["us-13vt", "us-14vt", "us-16vt"]);
    expect(childrenOf("us-webb")).toEqual(["us-69pa", "us-71pa", "us-72pa"]);
    expect(childrenOf("us-hall")).toEqual(
      ["us-19ma", "us-20ma", "us-42ny", "us-59ny", "us-7mi"]);
    for (const id of ["us-stannard", "us-webb", "us-hall"]) {
      const parent = units.find((u: any) => u.id === id);
      expect(parent.regiments).toBeUndefined(); // full decomposition or none
      expect(parent.parent).toBeUndefined(); // depth 1
    }
  });
  it("known-honest strength gaps surface as advisory warnings, never errors", () => {
    const result = validateBattle(battle);
    expect(result.ok).toBe(true);
    // Webb: the 106th PA stays unmodeled, counted only in the parent (short).
    // Hall: attested regimental sums (1,076) exceed the contested Stone
    // Sentinels brigade-table 920 — a kept disagreement, not a reconciliation.
    expect(result.warnings.join(" ")).toContain("us-webb");
    expect(result.warnings.join(" ")).toContain("us-hall");
    expect(result.warnings).toHaveLength(2);
  });
  it("the two new units exist and the unit count is pinned", () => {
    const units = (battle as any).units;
    // 22 + 11 A-grade children + 8th Ohio + Brown's B (A5)
    //    + 28 B-grade children + 2 Brockenbrough wings (A6)
    //    + 13 Wave-1 batteries: McGilvery's line (9) + the four reinforcing
    //      batteries (full-cast Task 3)
    //    + 20 Wave-2 units: Osborn group (9), Wainwright (4), Rittenhouse,
    //      XII Corps guns (5), Artillery Reserve park (full-cast Task 4).
    //      The plan's arithmetic said 21/99: its 21st (Taft's 1st CT Heavy
    //      B & M, "Not engaged") was verified NOT AT GETTYSBURG — held in
    //      reserve off-map — and is not authored (see author-w2-gun-ring.ts
    //      header adjudication; asserted in the Wave 2 test below).
    expect(units).toHaveLength(98);
    const ohio = units.find((u: any) => u.id === "us-8oh");
    expect(ohio).toBeDefined();
    expect(ohio.parent).toBeUndefined(); // Carroll's brigade isn't modeled
    expect(ohio.keyframes).toHaveLength(3);
    const brown = units.find((u: any) => u.id === "us-btty-brown");
    expect(brown).toBeDefined();
    // Cowan's gallop-in fix: no longer starts in place at its Copse position
    const cowan = units.find((u: any) => u.id === "us-btty-cowan");
    expect(cowan.keyframes[0].z).not.toBe(cowan.keyframes[cowan.keyframes.length - 1].z);
  });
  it("engagement events: validate, cite when documented, never the same fire at two family levels", () => {
    const result = validateBattle(battle);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
    const events = (battle as any).events;
    // the authored slice carries both kinds (bombardment + the climax musketry)
    expect(events.some((e: any) => e.kind === "artillery_fire")).toBe(true);
    expect(events.some((e: any) => e.kind === "musketry")).toBe(true);
    // the no-faking gate, asserted on the data (validate.ts enforces it too)
    for (const e of events)
      if (e.confidence === "documented") expect(e.citation?.trim()).toBeTruthy();
    // canonical export order: t0 then id (tool/src/io.ts exportBattle)
    const order = events.map((e: any) => [e.t0, e.id] as const);
    const sorted = [...order].sort((a, b) => a[0] - b[0] || (a[1] < b[1] ? -1 : 1));
    expect(order).toEqual(sorted);
    // Attach-level discipline as a data test (battle-format.md "Engagement
    // events"): an event attaches at the level the SOURCE attests, and the
    // same fire must never be authored at both a parent and its child — a
    // regiment window plus a parent window over the same span would double
    // the smoke and the sound. The validator cannot read attestation grain;
    // THIS file's discipline is checked here.
    const units = (battle as any).units;
    const byId = new Map<string, any>(units.map((u: any) => [u.id, u]));
    const unitEvents = events.filter((e: any) => e.unitId !== undefined);
    for (const a of unitEvents)
      for (const b of unitEvents) {
        if (a === b || a.kind !== b.kind) continue;
        if (a.t0 >= b.t1 || b.t0 >= a.t1) continue; // windows don't overlap
        const parentAndChild =
          byId.get(a.unitId)?.parent === b.unitId ||
          byId.get(b.unitId)?.parent === a.unitId;
        expect(parentAndChild).toBe(false);
      }
  });
  it("Wave 1: McGilvery's line + the four reinforcing batteries — presence, documented silence, mover tracks", () => {
    const units = (battle as any).units;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    // all 13 Wave-1 batteries present (full-cast plan Task 3; survey §3.3)
    const wave1 = [
      "us-btty-thomas", "us-btty-james", "us-btty-thompson", "us-btty-phillips",
      "us-btty-hart", "us-btty-sterling", "us-btty-cooper", "us-btty-dow",
      "us-btty-ames", "us-btty-fitzhugh", "us-btty-parsons", "us-btty-weir",
      "us-btty-wheeler",
    ];
    for (const id of wave1) expect(byId(id), id).toBeDefined();
    // Documented silence (battle-format.md "Engagement events"): every battery
    // STANDING on McGilvery's line at t=0 carries Hunt's hold-fire policy in
    // its t=0 keyframe citation — the policy is the line's default posture,
    // and for the six batteries with no attested cannonade fire the citation
    // IS the silence encoding. Phillips and Hart are the two documented
    // cannonade EXCEPTIONS (their counter-battery fire is separately
    // evented); the policy citation still rides their keyframes because it
    // governs them outside those attested windows. Cooper and Wheeler arrive
    // mid-window and go straight into attested fire; the reinforcements
    // start in reserve — none of those five stands on the line at t=0, so
    // the policy rides the eight.
    const lineAtT0 = [
      "us-btty-thomas", "us-btty-james", "us-btty-thompson", "us-btty-phillips",
      "us-btty-hart", "us-btty-sterling", "us-btty-dow", "us-btty-ames",
    ];
    for (const id of lineAtT0) {
      const k0 = byId(id).keyframes[0];
      expect(k0.confidence).toBe("documented");
      expect(k0.citation, id).toMatch(/Hunt/);
    }
    // the sheet-vs-OOB adjudication (survey open item 2) is carried, not
    // silently dropped: no Daniels/Rank unit, disagreement on Thompson's t=0
    expect(units.some((u: any) => /daniels|rank/i.test(u.id))).toBe(false);
    expect(byId("us-btty-thompson").keyframes[0].citation).toMatch(/Daniels/);
    // the documented in-window movers actually move
    for (const id of ["us-btty-cooper", "us-btty-wheeler", "us-btty-fitzhugh",
      "us-btty-parsons", "us-btty-weir"]) {
      const kfs = byId(id).keyframes;
      expect(kfs.length, id).toBeGreaterThan(2);
      expect(kfs[0].z, id).not.toBe(kfs[kfs.length - 1].z);
    }
    // attested fire windows exist and cohere with the slice
    const events = (battle as any).events;
    const evt = (id: string) => events.find((e: any) => e.id === id);
    expect(evt("us-btty-phillips-counterbattery").confidence).toBe("documented");
    expect(evt("us-btty-hart-counterbattery").confidence).toBe("documented");
    expect(evt("us-btty-phillips-repulse-enfilade").confidence).toBe("documented");
    const wheelerCanister = evt("us-btty-wheeler-canister");
    expect(wheelerCanister.confidence).toBe("documented");
    expect(wheelerCanister.t1).toBeLessThanOrEqual(10800); // tight against the window end
    // the Florida/second-line follow-up rides the Wilcox/Lang spine window
    expect(evt("us-btty-phillips-florida-repulse").t0).toBe(10200);
    expect(evt("us-btty-hart-second-line").t1).toBe(10500);
  });
  it("Wave 2: the gun ring — Osborn's ruse gap, Wainwright, Rittenhouse's enfilade, the silent XII Corps guns, the Reserve park mover", () => {
    const units = (battle as any).units;
    const events = (battle as any).events;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    // all 20 Wave-2 units present (full-cast plan Task 4; survey §3.3)
    const osborn = [
      "us-btty-wiedrich", "us-btty-dilger", "us-btty-bancroft", "us-btty-taft",
      "us-btty-mason", "us-btty-edgell", "us-btty-norton", "us-btty-hill-wv",
      "us-btty-ricketts",
    ];
    const wave2 = [
      ...osborn,
      "us-btty-stevens", "us-btty-breck", "us-btty-stewart", "us-btty-hall-2me",
      "us-btty-rittenhouse",
      "us-btty-rugg", "us-btty-kinzie", "us-btty-atwell", "us-btty-rigby",
      "us-btty-winegar",
      "us-arty-reserve-park",
    ];
    for (const id of wave2) expect(byId(id), id).toBeDefined();
    // The adjudicated omission (author-w2-gun-ring.ts header): Taft's 1st CT
    // Heavy Batteries B & M ("Not engaged" on the brigade tablet) were NOT at
    // Gettysburg — held in reserve off-map. Never authored; the ruling rides
    // Taft's t=0 citation. (The plan's 21-unit count assumed them present.)
    expect(units.some((u: any) => /brooker|pratt|ct-heavy|conn/i.test(u.id))).toBe(false);
    expect(byId("us-btty-taft").keyframes[0].citation).toMatch(/Not engaged/);
    // THE RUSE GAP IS THE DATUM (survey §4 item 3): Osborn's hill goes
    // deliberately quiet t=3000 (~13:50) until the advance (~7800) — no
    // Osborn-group event may span the gap, and each firing battery has fire
    // on BOTH sides of it (counter-battery, then the Pettigrew-flank window).
    const gapStart = 3000, gapEnd = 7800;
    const osbornEvents = events.filter((e: any) => osborn.includes(e.unitId));
    expect(osbornEvents.length).toBe(18); // 9 firing batteries × 2 windows
    for (const e of osbornEvents) {
      const spansGap = e.t0 < gapEnd && e.t1 > gapStart;
      expect(spansGap, e.id).toBe(false);
    }
    for (const id of osborn) {
      const mine = osbornEvents.filter((e: any) => e.unitId === id);
      expect(mine.some((e: any) => e.t1 <= gapStart), id).toBe(true);
      expect(mine.some((e: any) => e.t0 >= gapEnd), id).toBe(true);
    }
    // Rittenhouse: the wave's headline — documented Little Round Top enfilade
    // of the assault, window coherent with the spine (the Wave-1 pattern)
    const enfilade = events.find((e: any) => e.id === "us-btty-rittenhouse-repulse-enfilade");
    expect(enfilade.confidence).toBe("documented");
    expect(enfilade.t0).toBe(7800);
    expect(enfilade.t1).toBe(9000);
    // Documented silences (battle-format.md): the XII Corps' 26 guns fired
    // until 10:30 A.M. only, and Hall's 2nd ME went to the rear — presence
    // WITHOUT events; the negative rides every t=0 keyframe citation.
    const silent = ["us-btty-rugg", "us-btty-kinzie", "us-btty-atwell",
      "us-btty-rigby", "us-btty-winegar", "us-btty-hall-2me"];
    for (const id of silent) {
      expect(events.some((e: any) => e.unitId === id), id).toBe(false);
      const k0 = byId(id).keyframes[0];
      expect(k0.confidence, id).toBe("documented");
      expect(k0.citation, id).toMatch(id === "us-btty-hall-2me" ? /no further part/ : /10.30 A. M./);
    }
    // Wainwright's replies end at the army-wide ~14:30 cease (t=5400), not
    // Osborn's ruse gap — his I Corps guns are not under Osborn's datum
    for (const id of ["us-btty-stevens", "us-btty-breck", "us-btty-stewart"]) {
      const mine = events.filter((e: any) => e.unitId === id);
      expect(mine.length, id).toBe(1);
      expect(mine[0].t1, id).toBe(5400);
    }
    // the Artillery Reserve park actually displaces (Hunt's A− mover; its
    // motion is what derives the overshoot dust — childless, formation column)
    const park = byId("us-arty-reserve-park");
    expect(park.keyframes.length).toBeGreaterThan(2);
    expect(park.keyframes[0].z).not.toBe(park.keyframes[park.keyframes.length - 1].z);
    for (const k of park.keyframes) expect(k.formation).toBe("column");
    expect(units.some((u: any) => u.parent === "us-arty-reserve-park")).toBe(false);
  });
  it("B-grade Confederate children are valid; the four display-LOD brigades keep rosters and no children", () => {
    const units = (battle as any).units;
    const childrenOf = (p: string) =>
      units.filter((u: any) => u.parent === p).map((u: any) => u.id).sort();
    expect(childrenOf("csa-garnett")).toEqual(
      ["csa-18va", "csa-19va", "csa-28va", "csa-56va", "csa-8va"]);
    expect(childrenOf("csa-kemper")).toEqual(
      ["csa-11va", "csa-1va", "csa-24va", "csa-3va", "csa-7va"]);
    expect(childrenOf("csa-armistead")).toEqual(
      ["csa-14va", "csa-38va", "csa-53va", "csa-57va", "csa-9va"]);
    expect(childrenOf("csa-fry")).toEqual(
      ["csa-13al", "csa-14tn", "csa-1tn", "csa-5al-bn", "csa-7tn"]);
    expect(childrenOf("csa-marshall")).toEqual(
      ["csa-11nc", "csa-26nc", "csa-47nc", "csa-52nc"]);
    expect(childrenOf("csa-davis")).toEqual(
      ["csa-11miss", "csa-2miss", "csa-42miss", "csa-55nc"]);
    // Brockenbrough: two WINGS, not four regiments — the wing split is
    // attested (Mayo's OR), the regiment split is not; children MAY roster
    expect(childrenOf("csa-brockenbrough")).toEqual(["csa-brock-left", "csa-brock-right"]);
    for (const id of ["csa-brock-right", "csa-brock-left"]) {
      const wing = units.find((u: any) => u.id === id);
      expect(wing.regiments).toHaveLength(2);
    }
    for (const id of ["csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
      "csa-marshall", "csa-davis", "csa-brockenbrough"]) {
      const parent = units.find((u: any) => u.id === id);
      expect(parent.regiments).toBeUndefined(); // full decomposition or none
      expect(parent.parent).toBeUndefined(); // depth 1
    }
    // Lane/Lowrance/Wilcox/Lang stay display-LOD rosters — decomposing them
    // would manufacture unattested tracks (the plan's honesty line)
    for (const id of ["csa-lane", "csa-lowrance", "csa-wilcox", "csa-lang"]) {
      const u = units.find((x: any) => x.id === id);
      expect(u.regiments.length).toBeGreaterThanOrEqual(2);
      expect(u.parent).toBeUndefined();
      expect(childrenOf(id)).toEqual([]);
    }
  });
});
