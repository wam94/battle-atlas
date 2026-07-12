import { describe, expect, it } from "vitest";
import battle from "../../app/Assets/Battle/gettysburg-july3.json";
import { validateBattle } from "../src/validate";

describe("authored July 3 battle", () => {
  it("passes schema + rule validation", () => {
    const result = validateBattle(battle);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });
  it("keeps every keyframe on the battlefield (one attested edge-lap excepted)", () => {
    // The ONE exception (full-cast plan Task 8; survey §1): us-cav-merritt's
    // keyframes sit at the attested Currens farm anchor (2733,−96) — 96 m off
    // the square's south edge, the survey's EDGE ruling ("straddles the S
    // boundary… his line laps both sides"). The edge-lapping is honest and
    // carried HERE, explicitly, rather than silently pulled inside.
    for (const u of (battle as any).units)
      for (const k of u.keyframes) {
        expect(k.x).toBeGreaterThan(0); expect(k.x).toBeLessThan(8507);
        if (u.id === "us-cav-merritt") {
          expect(k.z).toBe(-96);
          continue;
        }
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
    // Henry (Wave 6): only Bachman's & Reilly's batteries are attested SCF
    // actors and modeled as children (71 + 144 = 215); Garden's and Latham's
    // stay unmodeled, counted only in the battalion's 456 — the known-short
    // decomposition, the Webb/106th-PA precedent.
    expect(result.warnings.join(" ")).toContain("us-webb");
    expect(result.warnings.join(" ")).toContain("us-hall");
    expect(result.warnings.join(" ")).toContain("csa-bn-henry");
    expect(result.warnings).toHaveLength(3);
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
    //    + 15 Wave-3 CSA artillery battalions (full-cast Task 5) — the
    //      bombardment's named emitters AND its documented silences; the
    //      csa-seminary-bombardment segment retired in the same commit.
    //    + 45 Wave-4 Union infantry brigades (full-cast Task 6): the ring —
    //      XII Corps (6) + Wadsworth (2) on Culp's Hill, XI Corps (6) +
    //      Carroll on Cemetery Hill, I Corps remnants (4, incl. Robinson's
    //      two movers), Caldwell (4), III Corps reserve (6), V Corps (8),
    //      VI Corps (8, incl. Shaler the ~3:30 mover). All parentless —
    //      divisions/corps are not modeled.
    //    + 26 Wave-5 CSA infantry brigades (full-cast Task 7): Longstreet 8
    //      (Hood 4 + McLaws 4, incl. Kershaw the ~13:00 extension-right
    //      mover), Hill 5 (incl. Wright's advance-and-recall and the two
    //      Long Lane skirmish emitters Thomas & Perrin), Ewell 13 (Johnson's
    //      seven-brigade block east of Rock Creek, Early's three at the
    //      town, Rodes's three in Long Lane). All parentless.
    //    + 6 Wave-6 South Cavalry Field units (full-cast Task 8): Farnsworth's
    //      and Merritt's brigades + Elder's and Graham's batteries (Union,
    //      parentless), Bachman's and Reilly's batteries as csa-bn-henry
    //      children (attested grain is battery there). The plan's "~7th" —
    //      the 1st Texas shift — was adjudicated NOT authored (no attested
    //      regiment strength; see author-w6-south-cavalry.ts header); East
    //      Cavalry Field contributes ZERO units by the survey's off-map
    //      ruling, annotated in the Wave-6 citations.
    expect(units).toHaveLength(190);
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
  it("Wave 3: the 15 CSA artillery battalions — the segment retired, the silences encoded, the detachments as segments", () => {
    const units = (battle as any).units;
    const events = (battle as any).events;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    // all 15 battalions present (full-cast plan Task 5; survey §3.2 table)
    const wave3 = [
      "csa-bn-alexander", "csa-bn-cabell", "csa-bn-dearing", "csa-bn-eshleman",
      "csa-bn-henry", "csa-bn-pegram", "csa-bn-mcintosh", "csa-bn-garnett",
      "csa-bn-poague", "csa-bn-lane", "csa-bn-dance", "csa-bn-nelson",
      "csa-bn-carter", "csa-bn-jones", "csa-bn-raine",
    ];
    for (const id of wave3) expect(byId(id), id).toBeDefined();
    // the bn- prefix dodges the infantry ids (csa-garnett, csa-lane are
    // brigades and keep their own tracks untouched)
    expect(byId("csa-garnett").name).toMatch(/Brig/i);
    expect(byId("csa-lane").regiments).toBeDefined();
    // THE SEGMENT RETIRED (battle-format.md "Segment-emitter migration"):
    // csa-seminary-bombardment is gone, and the only surviving segment-form
    // events are the four attested detachments (Carter's rifles at the
    // railroad cut, Raine's 20-pdr section N of Benner's Hill, Wave 6's
    // Reilly two-Parrott section at its own marker-attested position, and —
    // Wave A1 — Miller's advanced Washington Artillery section, 450 yds
    // forward of the battalion's tablet line per or-27-2-eshleman).
    expect(events.some((e: any) => e.id === "csa-seminary-bombardment")).toBe(false);
    const segmentEvents = events.filter((e: any) => e.unitId === undefined);
    expect(segmentEvents.map((e: any) => e.id).sort()).toEqual(
      ["csa-bn-carter-rifles-cannonade", "csa-bn-raine-20pdr-cannonade",
        "csa-btty-reilly-parrott-section", "csa-wa-miller-advanced-section"]);
    for (const e of segmentEvents) expect(e.confidence).toBe("documented");
    // DOCUMENTED SILENCES (survey §3.2 ⚠️; §4 item 13): Garnett's and
    // Jones's battalions have ZERO events — the negative rides the t=0
    // keyframe citation; Ewell's arc renders present and mostly dark.
    for (const id of ["csa-bn-garnett", "csa-bn-jones"]) {
      expect(events.some((e: any) => e.unitId === id), id).toBe(false);
      const k0 = byId(id).keyframes[0];
      expect(k0.confidence, id).toBe("documented");
      expect(k0.citation, id).toMatch(/not actively engaged/i);
    }
    // the split battalions' PARENT tracks never fire as units — their
    // detached pieces fire as the segments above (never the same fire at
    // two grains), and the Ewell disagreement rides every II Corps t=0
    for (const id of ["csa-bn-carter", "csa-bn-raine"])
      expect(events.some((e: any) => e.unitId === id), id).toBe(false);
    for (const id of ["csa-bn-dance", "csa-bn-nelson", "csa-bn-carter",
      "csa-bn-jones", "csa-bn-raine"])
      expect(byId(id).keyframes[0].citation, id).toMatch(/EWELL'S-WING DISAGREEMENT/);
    // Nelson: the LIMITED window — short, then silent (tablet: 20-25 rounds)
    const nelson = events.filter((e: any) => e.unitId === "csa-bn-nelson");
    expect(nelson).toHaveLength(1);
    expect(nelson[0].t1).toBeLessThanOrEqual(1500);
    // Henry's flagged flank cover ends at t=7200 where Wave 6's
    // Bachman/Reilly children begin — the two-family-levels guard
    const henry = events.filter((e: any) => e.unitId === "csa-bn-henry");
    expect(henry).toHaveLength(1);
    expect(henry[0].t1).toBe(7200);
    // the 13:07 signal guns are Eshleman's (Miller's battery in the citation)
    const signal = events.find((e: any) => e.id === "csa-bn-eshleman-signal");
    expect(signal.t0).toBe(420);
    expect(signal.confidence).toBe("documented");
    expect(signal.citation).toMatch(/Miller/);
  });
  it("smoke never regresses: CSA artillery_fire covers the cannonade window the segment covered", () => {
    // The atomic-migration rule (full-cast plan Task 5; battle-format.md):
    // the retired csa-seminary-bombardment segment covered t 420-7500 on
    // Seminary Ridge — the replacement per-battalion events must cover the
    // same window on the CSA side, with >= 10 emitters through the general
    // fire (the survey's 10 YES battalions + the two detachment segments).
    const events = (battle as any).events;
    const csaFire = events.filter(
      (e: any) => e.kind === "artillery_fire" &&
        ((e.unitId ?? e.id).startsWith("csa-bn-")));
    // outer envelope: fire from the 13:07 signal to the step-off spine
    expect(Math.min(...csaFire.map((e: any) => e.t0))).toBe(420);
    expect(Math.max(...csaFire.map((e: any) => e.t1))).toBe(7500);
    // every instant of the old segment's window has a live CSA emitter, and
    // the general fire runs >= 10 emitters deep (the never-regress check)
    for (let t = 420; t < 7500; t += 60) { // half-open [t0, t1) sampling
      const live = csaFire.filter((e: any) => e.t0 <= t && t < e.t1);
      expect(live.length, `t=${t}`).toBeGreaterThanOrEqual(1);
      if (t >= 480 && t < 7200)
        expect(live.length, `t=${t}`).toBeGreaterThanOrEqual(10);
    }
  });
  it("Wave 4: the Union infantry ring — 45 cited brigades, the movers move, the ring stays silent except where the survey attests fire", () => {
    const units = (battle as any).units;
    const events = (battle as any).events;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    // all 45 Wave-4 brigades present, per sector (full-cast plan Task 6;
    // survey §3.3 corps-by-corps)
    const culpsHill = ["us-greene", "us-kane", "us-candy", "us-mcdougall",
      "us-lockwood", "us-colgrove", "us-meredith", "us-cutler"];
    const cemeteryHill = ["us-vongilsa", "us-harris", "us-coster", "us-smith",
      "us-vonamsberg", "us-krzyzanowski", "us-carroll"];
    const centre = ["us-coulter", "us-baxter", "us-biddle", "us-dana",
      "us-mckeen", "us-kelly", "us-fraser", "us-brooke"];
    const rearLeft = ["us-madill", "us-berdan", "us-detrobriand", "us-carr",
      "us-brewster", "us-burling"];
    const roundTops = ["us-tilton", "us-sweitzer", "us-rice", "us-day",
      "us-burbank", "us-garrard", "us-mccandless", "us-fisher"];
    const viCorps = ["us-torbert", "us-bartlett", "us-nevin", "us-russell",
      "us-grant", "us-neill", "us-shaler", "us-eustis"];
    const wave4 = [...culpsHill, ...cemeteryHill, ...centre, ...rearLeft,
      ...roundTops, ...viCorps];
    expect(wave4).toHaveLength(45);
    for (const id of wave4) {
      const u = byId(id);
      expect(u, id).toBeDefined();
      expect(u.side, id).toBe("union");
      // parent/family rules: none — divisions and corps are not modeled
      expect(u.parent, id).toBeUndefined();
      // every Union brigade t=0 keyframe documented-with-citation (the wave
      // assertion the plan pins)
      expect(u.keyframes[0].confidence, id).toBe("documented");
      expect(u.keyframes[0].citation?.trim(), id).toBeTruthy();
    }
    // the attested in-window movers actually move (>1 keyframe, displaced);
    // everyone else is a 2-keyframe static at an identical pose
    const movers = ["us-coulter", "us-baxter", "us-shaler"];
    for (const id of movers) {
      const kfs = byId(id).keyframes;
      expect(kfs.length, id).toBeGreaterThan(2);
      expect(kfs[0].x, id).not.toBe(kfs[kfs.length - 1].x);
    }
    for (const id of wave4.filter((i) => !movers.includes(i))) {
      const kfs = byId(id).keyframes;
      expect(kfs, id).toHaveLength(2);
      expect(kfs[0].x, id).toBe(kfs[1].x);
      expect(kfs[0].z, id).toBe(kfs[1].z);
    }
    // Robinson's two: the tablet's 3 P.M. arrival on the right of Second
    // Corps (t=7200, documented) — and musketry NOT attested: flagged, no event
    for (const id of ["us-coulter", "us-baxter"]) {
      const arrive = byId(id).keyframes.find((k: any) => k.t === 7200);
      expect(arrive, id).toBeDefined();
      expect(arrive.confidence, id).toBe("documented");
      expect(arrive.citation, id).toMatch(/right of Second Corps/);
      expect(arrive.citation, id).toMatch(/MUSKETRY NOT ATTESTED/);
    }
    // Shaler: departs at the tablet's 3 P.M. (t=7200), arrives at the
    // survey's ~3:30 (t=9000) — the clock spread carried, not reconciled
    const shalerTs = byId("us-shaler").keyframes.map((k: any) => k.t);
    expect(shalerTs).toContain(7200);
    expect(shalerTs).toContain(9000);
    // documented silences: the XII Corps works stand manned and quiet after
    // 10:30 — the negative rides every t=0 citation (Wadsworth's two carry
    // the division tablet's morning-only fight the same way)
    for (const id of culpsHill.slice(0, 6))
      expect(byId(id).keyframes[0].citation, id).toMatch(/10:30 A. M./);
    // Carroll is authored minus the modeled 8th Ohio, and says so; the
    // detached regiment stays a free-standing unit (no family link)
    expect(byId("us-carroll").keyframes[0].citation).toMatch(/8th OHIO/i);
    expect(byId("us-8oh").parent).toBeUndefined();
    // NO musketry for the ring except the three survey-attested windows:
    // Doubleday's two repulse events + Neill's Rock Creek skirmish
    const wave4Ids = new Set(wave4);
    const wave4Events = events.filter((e: any) => wave4Ids.has(e.unitId));
    expect(wave4Events.map((e: any) => e.id).sort()).toEqual([
      "us-biddle-repulse-fire", "us-dana-repulse-fire",
      "us-neill-rock-creek-skirmish"]);
    for (const e of wave4Events) {
      expect(e.kind, e.id).toBe("musketry");
      expect(e.confidence, e.id).toBe("documented");
      expect(e.t1, e.id).toBeLessThanOrEqual(10800);
    }
  });
  it("Wave 5: the CSA infantry ring — 26 cited brigades, Johnson's block east of Rock Creek, the two movers, the three attested emitters", () => {
    const units = (battle as any).units;
    const events = (battle as any).events;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    // all 26 Wave-5 brigades present, per sector (full-cast plan Task 7;
    // survey §3.1 — acting-commander ids per the survey header: Sheffield,
    // Bryan, Humphreys, Godwin, Dungan, Perrin, Luffman, + Williams for
    // Nicholls per the Johnson division tablet roster)
    const hood = ["csa-sheffield", "csa-robertson", "csa-benning", "csa-luffman"];
    const mclaws = ["csa-kershaw", "csa-bryan", "csa-humphreys", "csa-wofford"];
    const hill = ["csa-wright", "csa-posey", "csa-mahone", "csa-thomas", "csa-perrin"];
    const johnson = ["csa-steuart", "csa-daniel", "csa-oneal", "csa-williams",
      "csa-walker", "csa-dungan", "csa-smith"];
    const early = ["csa-hays", "csa-godwin", "csa-gordon"];
    const rodes = ["csa-ramseur", "csa-iverson", "csa-doles"];
    const wave5 = [...hood, ...mclaws, ...hill, ...johnson, ...early, ...rodes];
    expect(wave5).toHaveLength(26);
    for (const id of wave5) {
      const u = byId(id);
      expect(u, id).toBeDefined();
      expect(u.side, id).toBe("confederate");
      // parent/family rules: none — divisions and corps are not modeled
      expect(u.parent, id).toBeUndefined();
      // every CSA brigade t=0 keyframe documented-with-citation
      expect(u.keyframes[0].confidence, id).toBe("documented");
      expect(u.keyframes[0].citation?.trim(), id).toBeTruthy();
    }
    // id-collision dodges hold: no bare csa-jones (csa-bn-jones is H.P.
    // Jones's artillery battalion; the VA infantry brigade is csa-dungan)
    // and the assault-sector csa-garnett/csa-lane tracks are untouched
    expect(units.some((u: any) => u.id === "csa-jones")).toBe(false);
    expect(byId("csa-dungan").name).toMatch(/Jones's Brigade/);
    expect(byId("csa-garnett").name).toMatch(/Brig/i);
    expect(byId("csa-lane").regiments).toBeDefined();
    // JOHNSON'S BLOCK EAST OF ROCK CREEK (the plan's wave assertion): all
    // seven stand beyond the creek line at the Benner's Hill base — sheet 8
    // and the division tablet agree exactly
    for (const id of johnson) {
      expect(byId(id).keyframes[0].x, id).toBeGreaterThan(6300);
      // the documented silence: retired 10:30 A.M., held to 10 P.M. — the
      // negative rides every t=0 citation
      expect(byId(id).keyframes[0].citation, id).toMatch(/Retired at 10.30 A. M./);
    }
    // McLaws's documented negative rides all four of his brigades
    for (const id of mclaws)
      expect(byId(id).keyframes[0].citation, id)
        .toMatch(/severe skirmishing the Division was not engaged/);
    // the two attested in-window movers actually move; everyone else is a
    // 2-keyframe static at an identical pose
    const movers = ["csa-kershaw", "csa-wright"];
    for (const id of wave5.filter((i) => !movers.includes(i))) {
      const kfs = byId(id).keyframes;
      expect(kfs, id).toHaveLength(2);
      expect(kfs[0].x, id).toBe(kfs[1].x);
      expect(kfs[0].z, id).toBe(kfs[1].z);
    }
    // Kershaw: the ~13:00 extension right — t=0 forward of the Peach
    // Orchard, t=600 on the Warfield Ridge line (tablet '1 P. M.' clock)
    const kershaw = byId("csa-kershaw").keyframes;
    expect(kershaw.map((k: any) => k.t)).toEqual([0, 600, 10800]);
    expect(kershaw[0].x).not.toBe(kershaw[1].x);
    expect(kershaw[1].citation).toMatch(/At 1 P. M./);
    // Wright: the advance-and-recall (t=9000→10500 per plan) — departs at
    // the division tablet's 3.30 P.M., stands ~550 m forward covering
    // Pickett's retreat, and is back on the ridge when the order is
    // countermanded; per-keyframe citations throughout
    const wright = byId("csa-wright").keyframes;
    expect(wright.map((k: any) => k.t)).toEqual([0, 9000, 9660, 10500, 10800]);
    expect(wright[2].x - wright[0].x).toBeGreaterThan(400); // the 600 yards
    expect(wright[3].x).toBe(wright[0].x); // recalled to the start line
    expect(wright[3].z).toBe(wright[0].z);
    for (const k of wright) {
      expect(k.confidence).toBe("documented");
      expect(k.citation?.trim()).toBeTruthy();
    }
    // fire for the ring: EXACTLY the survey's three emitters — Thomas &
    // Perrin in Long Lane (A−) and the collective Hays town-sharpshooting —
    // documented musketry spanning the window; no other Wave-5 unit fires
    // (McLaws/Hood/Ewell quiet is presence without events)
    const wave5Ids = new Set(wave5);
    const wave5Events = events.filter((e: any) => wave5Ids.has(e.unitId));
    expect(wave5Events.map((e: any) => e.id).sort()).toEqual([
      "csa-hays-town-sharpshooting", "csa-perrin-long-lane-skirmish",
      "csa-thomas-long-lane-skirmish"]);
    for (const e of wave5Events) {
      expect(e.kind, e.id).toBe("musketry");
      expect(e.confidence, e.id).toBe("documented");
      expect(e.t0, e.id).toBe(0); // the tablets' all-window skirmishing
      expect(e.t1, e.id).toBe(10800);
    }
  });
  it("Wave 6: South Cavalry Field — the cluster present, the t=7200 family handoff holds, the edge and the spreads carried, East Cavalry Field empty", () => {
    const units = (battle as any).units;
    const events = (battle as any).events;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    // the six Wave-6 units (full-cast plan Task 8; survey §3.4): Union
    // parentless, the two CSA batteries as csa-bn-henry children (attested
    // grain is battery there — the tablets attest their July 3 fire
    // individually; Henry's parent track stays the battalion record)
    const union6 = ["us-cav-farnsworth", "us-cav-merritt", "us-btty-elder",
      "us-btty-graham"];
    const csa6 = ["csa-btty-bachman", "csa-btty-reilly"];
    for (const id of union6) {
      const u = byId(id);
      expect(u, id).toBeDefined();
      expect(u.side, id).toBe("union");
      expect(u.parent, id).toBeUndefined();
    }
    for (const id of csa6) {
      const u = byId(id);
      expect(u, id).toBeDefined();
      expect(u.side, id).toBe("confederate");
      expect(u.parent, id).toBe("csa-bn-henry");
    }
    // the whole cluster is 2-keyframe static (tablets weighted over Snell's
    // swing reading — the adjudication in author-w6-south-cavalry.ts), every
    // t=0 documented-with-citation; Farnsworth's mounted charge is
    // POST-window: the end keyframe stays deployed and the citation says why
    for (const id of [...union6, ...csa6]) {
      const kfs = byId(id).keyframes;
      expect(kfs, id).toHaveLength(2);
      expect(kfs[0].x, id).toBe(kfs[1].x);
      expect(kfs[0].z, id).toBe(kfs[1].z);
      expect(kfs[0].confidence, id).toBe("documented");
      expect(kfs[0].citation?.trim(), id).toBeTruthy();
    }
    expect(byId("us-cav-farnsworth").keyframes[0].citation)
      .toMatch(/THE MOUNTED CHARGE IS POST-WINDOW/);
    // Merritt: the attested Currens-anchor edge-lap (asserted per-keyframe in
    // the on-battlefield test above) + the arrival spread carried whole —
    // 11:00 / 13:00 / 15:00 and both tablets' clocks, never reconciled
    const merritt = byId("us-cav-merritt").keyframes[0];
    expect(merritt.z).toBe(-96);
    expect(merritt.citation).toMatch(/ARRIVAL SPREAD CARRIED/);
    expect(merritt.citation).toMatch(/~11:00/);
    expect(merritt.citation).toMatch(/~13:00/);
    expect(merritt.citation).toMatch(/~15:00/);
    // THE t=7200 FAMILY HANDOFF (never the same fire at two family levels):
    // csa-bn-henry's flank-cover window ends at 7200 exactly (pinned in the
    // Wave 3 test) and every child event begins at 7200 exactly — windows
    // touch, never overlap, so the arrivals cannot double-fire
    const childEvents = events.filter((e: any) => csa6.includes(e.unitId));
    expect(childEvents.map((e: any) => e.id).sort()).toEqual(
      ["csa-btty-bachman-fire", "csa-btty-reilly-fire"]);
    for (const e of childEvents) {
      expect(e.t0, e.id).toBe(7200);
      expect(e.confidence, e.id).toBe("documented");
    }
    // Reilly's detached Parrott section fires as a fixed segment at its own
    // marker-attested position (different guns than the battery's remaining
    // four — two attested fires, not one fire at two grains), on the same
    // handoff clock
    const section = events.find((e: any) => e.id === "csa-btty-reilly-parrott-section");
    expect(section.unitId).toBeUndefined();
    expect(section.z).toBe(1713);
    expect(section.t0).toBe(7200);
    expect(section.confidence).toBe("documented");
    // the four Union windows (Elder shelling Law's line, Graham firing,
    // Merritt-sector skirmish, Farnsworth's dismounted probing) — all
    // documented, all inside the slice
    const unionEvents = events.filter((e: any) => union6.includes(e.unitId));
    expect(unionEvents.map((e: any) => e.id).sort()).toEqual([
      "us-btty-elder-shelling", "us-btty-graham-fire",
      "us-cav-farnsworth-probing", "us-cav-merritt-skirmish"]);
    for (const e of unionEvents) {
      expect(e.confidence, e.id).toBe("documented");
      expect(e.t1, e.id).toBeLessThanOrEqual(10800);
    }
    // EAST CAVALRY FIELD: ZERO UNITS (the survey's off-map ruling — the
    // fight is 4–5.5 km beyond the east edge); the annotation rides the
    // Wave-6 citations instead (Custer-to-Gregg on Farnsworth, the 6th US at
    // Fairfield on Merritt), so the omission is accounted for, not silent
    expect(units.some((u: any) =>
      /custer|stuart|hampton|chambliss|witcher|fitz-lee/i.test(u.id))).toBe(false);
    expect(byId("us-cav-farnsworth").keyframes[0].citation)
      .toMatch(/EAST CAVALRY FIELD/);
    expect(byId("us-cav-merritt").keyframes[0].citation).toMatch(/FAIRFIELD/);
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
  it("Wave A1: dossier placement — the echelon re-placed, the famine windows, ED-38/44/50 executed", () => {
    const units = (battle as any).units;
    const events = (battle as any).events;
    const byId = (id: string) => units.find((u: any) => u.id === id);
    const evt = (id: string) => events.find((e: any) => e.id === id);
    // Wilcox/Lang: July 3 strengths on their own OR primaries (1,200 / ~400),
    // Lang on Wilcox's LEFT, the advance reaching the documented ravine line
    const wilcox = byId("csa-wilcox").keyframes;
    expect(wilcox[0].strength).toBe(1200);
    expect(wilcox[wilcox.length - 1].strength).toBe(996); // − the 204 per-day primary
    const lang = byId("csa-lang").keyframes;
    expect(lang[0].strength).toBe(400);
    expect(lang[0].z).toBeGreaterThan(wilcox[0].z); // left = north of Wilcox
    const wilcoxFarthest = wilcox.find((k: any) => k.t === 10380);
    expect(wilcoxFarthest.x).toBeGreaterThan(4000); // the ravine below McGilvery's crest
    // Dearing: the daybreak-advanced road-crest line + the famine-shortened window
    expect(byId("csa-bn-dearing").keyframes[0].x).toBe(3630);
    expect(evt("csa-bn-dearing-cannonade").t1).toBe(5700);
    // McGilvery's deliberate reply: six per-battery windows, hold → reply → quiet
    for (const b of ["ames", "dow", "sterling", "hart", "phillips", "thompson"]) {
      const reply = evt(`us-btty-${b}-deliberate-reply`);
      expect(reply, b).toBeDefined();
      expect(reply.t0, b).toBe(5820);
      expect(reply.t1, b).toBe(7200);
      expect(reply.confidence, b).toBe("inferred"); // line-grain OR, per-battery inferred
    }
    // Miller's advanced section: segment form, documented, post-cannonade window
    const miller = evt("csa-wa-miller-advanced-section");
    expect(miller.unitId).toBeUndefined();
    expect(miller.t0).toBe(8100);
    expect(miller.confidence).toBe("documented");
    expect(miller.citation).toMatch(/400 or 500 yards/);
    // Fitzhugh's second window rides the Wilcox/Lang spine
    expect(evt("us-btty-fitzhugh-wilcox-enfilade").t0).toBe(10200);
    // ED-50: the 8th Ohio's inverted shape (~45 of 102 pre-window)
    const ohio = byId("us-8oh").keyframes;
    expect(ohio[0].strength).toBe(164);
    expect(ohio[0].citation).toMatch(/ED-50/);
    // ED-38: Eshleman re-strengthed; ED-44: Garnett's silence at primary grade
    expect(byId("csa-bn-eshleman").keyframes[0].strength).toBe(338);
    expect(byId("csa-bn-garnett").keyframes[0].citation)
      .toMatch(/did not fire a single shot/);
    expect(events.some((e: any) => e.unitId === "csa-bn-garnett")).toBe(false);
    // Sherrill's one in-window leg: slope → the wall line at the tablet's 15:00
    const sherrill = byId("us-sherrill").keyframes;
    expect(sherrill[0].x).toBe(4540);
    expect(sherrill.find((k: any) => k.t === 7440).x).toBe(4470);
    // dossier-apportioned decay is monotonic non-increasing everywhere it landed
    for (const id of ["csa-wilcox", "csa-lang", "csa-bn-dearing", "csa-bn-eshleman",
      "csa-bn-pegram", "us-8oh", "us-btty-dow", "us-btty-fitzhugh",
      "us-btty-parsons", "us-btty-cooper"]) {
      const kfs = byId(id).keyframes;
      for (let i = 1; i < kfs.length; i++)
        expect(kfs[i].strength, `${id} t=${kfs[i].t}`).toBeLessThanOrEqual(kfs[i - 1].strength);
    }
  });
});
