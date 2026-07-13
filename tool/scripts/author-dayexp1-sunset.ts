// Day expansion slice 1 — THE SUNSET WIDENING: July 3 grows from the
// 13:00–16:00 window (endTime 10800) to 13:00–sunset (endTime 23340 =
// 19:29 LMT, the ED-31 astronomical pin), and A2's deferred post-16:00
// worklist (authoring-wave-a2.md §2 items 5/10, §6.4) becomes in-window,
// attested content. Run from tool/:
//   npx vite-node scripts/author-dayexp1-sunset.ts
// Committed as the derivation record (the author-w*/A1/A2 pattern).
// Slice report: docs/reconstruction/audit/day-expansion-slice-1.md.
//
// HARD RULES this script observes:
// - The 13 Angle-cast units' EXISTING keyframes (all t <= 10800) are
//   byte-unchanged — the widening only ADDS window after 16:00, and the
//   cast's additions are t=23340 evening-state keyframes carrying their
//   pass-3 dossiers' rally/dissolution citations (guarded below: the
//   script throws if any existing cast keyframe changes).
// - NEVER invent motion: every added/edited keyframe and event cites the
//   dossier's facts; attested stillness stays still (the ring's statics
//   keep their two keyframes — the clamp rule holds their pose to sunset,
//   which is exactly the documented-silence convention); tier-C/D timing
//   authors at the preferred time with `inferred` confidence.
// - The re-basing of the assault column's strengths is NOT this script
//   (it is the slice's separate atomic operation with its own film-safety
//   gate — author-dayexp1-rebase.ts).
//
// WHAT THE SCRIPT DOES (full table in the slice report):
// 1. endTime 10800 -> 23340 (sunset 19:29 LMT, ED-31).
// 2. McCartney's 1st MA Battery A — NEW unit: the corpus's clearest
//    "arrived too late" record becomes authorable (reserve ground near
//    the Weickert farm -> the cemetery's left ~16:00, report-primary
//    "the enemy had been repulsed before the battery took position");
//    his four long-range rounds land as a brief fire event.
// 3. The ~17:00 Wheatfield sweep (Bartlett's composite command):
//    McCandless's Reserves advance across the Wheatfield and sweep the
//    woods south; Nevin's brigade follows at 200 yards and comes up to
//    the front (Bartlett's "between 20 and 30" k+w decay); Benning's
//    15th GA is caught withdrawing (~200 prisoners + the colors) and the
//    brigade retires to its tablet ground.
// 4. Taft's flank fire extends to his report's ~6 P.M. cease (the
//    marker's "until 4 p.m." carried as the counter-reading — the A2
//    window-edge compromise retires with the window edge).
// 5. Wheeler's canister fight gets its post-16:00 remainder (inferred,
//    bounded); his "not modeled" annotation retires.
// 6. The three tablet-attested all-day skirmish events (Hays's town
//    sharpshooting, Perrin's and Thomas's Long Lane skirmishing) extend
//    to sunset with their tablets' own day scope.
// 7. Evening-state keyframes (t=23340) for the assault column: the
//    pass-3 rally/reform records (Fry/Marshall/Davis/Lane/Lowrance/
//    Wilcox/Lang/Brockenbrough reform as thin lines; Pickett's three
//    stay dissolution-class scattered) land as cited keyframes instead
//    of a silent clamp.
// 8. moments.json gains the sunset window's cited markers (16:00 the
//    field falls quiet / ~17:00 the sweep / ~18:00 Taft's cease /
//    19:29 sunset).
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe, Unit } from "../src/model";
import { fireEvent, exportValidated, addUnitsAfter } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
const momentsPath = join(
  here, "../../app/Assets/StreamingAssets/Atlas/moments.json");
const manifestPath = join(
  here, "../../app/Assets/StreamingAssets/Atlas/battle-manifest.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const SunsetT = 23340; // 19:29 LMT - 13:00 = 6h29m (ED-31)

// The Angle-cast guard, day-expansion form: existing keyframes (t <= the
// old window end) must survive byte-identical; only t=23340 additions are
// legal on cast units in THIS script.
const ANGLE_CAST = new Set([
  "csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
  "us-webb", "us-69pa", "us-71pa", "us-72pa",
  "us-btty-cushing", "us-btty-cowan", "us-btty-arnold",
  "us-hall", "us-stannard",
]);
const castBefore = new Map<string, string>();
for (const u of battle.units)
  if (ANGLE_CAST.has(u.id)) castBefore.set(u.id, JSON.stringify(u.keyframes));

const byId = new Map(battle.units.map((u) => [u.id, u]));
function unit(id: string): Unit {
  const u = byId.get(id);
  if (!u) throw new Error(`day-expansion 1: unit '${id}' missing`);
  return u;
}
function event(id: string): EngagementEvent {
  const e = (battle.events ?? []).find((x) => x.id === id);
  if (!e) throw new Error(`day-expansion 1: event '${id}' missing`);
  return e;
}
function lastKf(id: string): Keyframe {
  const kfs = unit(id).keyframes;
  return kfs[kfs.length - 1]!;
}

// ---------------------------------------------------------------------------
// 1. The widening itself
// ---------------------------------------------------------------------------
if (battle.endTime !== 10800)
  throw new Error("expected the pre-widening window (endTime 10800)");
battle.endTime = SunsetT;

// ---------------------------------------------------------------------------
// 2. McCartney's 1st Massachusetts Battery A — the new unit
//    (reconstruction/dossiers/us-btty-mccartney.md; or-27-1-mccartney)
// ---------------------------------------------------------------------------
const mccartney: Unit = {
  id: "us-btty-mccartney",
  name: "McCartney's 1st Massachusetts Battery A",
  side: "union",
  frontage_m: 100, // six 12-pdr Napoleons (tablet), the 6-gun convention
  depth_m: 40,
  keyframes: [
    {
      t: 0, x: 4210, z: 3070, facing: 270, formation: "column", strength: 145,
      confidence: "inferred",
      citation:
        "waiting with the VI Corps Artillery Brigade reserve: 'South of Gettysburg, east side of Sedgwick Avenue, " +
        "near the George Weickert farm' (brigade tablet verbatim [E], reconstruction/dossiers/us-vi-arty-tompkins.md " +
        "EC3.2 — placed at the tablet ground beside Nevin's tab-us-vi-3-3 4202,3060; radius honest at ~±150 m, no " +
        "battery-grain morning position exists); strength 145 men, six 12-pdr Napoleons (battery tablet verbatim, " +
        "reconstruction/dossiers/us-btty-mccartney.md EC2); Capt. William H. McCartney, 'a Boston attorney'",
    },
    {
      t: 9600, x: 4210, z: 3070, facing: 270, formation: "column", strength: 145,
      confidence: "inferred",
      citation:
        "departure inferred ~15:40 (the order came with the repulse cresting; 2.4 km at a ~2 m/s road trot lands the " +
        "attested ~4 P.M. arrival): 'this command was ordered into position on the left of the cemetery' " +
        "(or-27-1-mccartney No. 238 pp. 688-689, verbatim)",
    },
    {
      t: 10800, x: 4890, z: 5470, facing: 260, formation: "line", strength: 145,
      confidence: "inferred",
      citation:
        "ARRIVED TOO LATE — the corpus's clearest record of it [C ~16:00, report-primary]: 'the enemy had been " +
        "repulsed before the battery took position' (or-27-1-mccartney, verbatim); position 'near the cemetery, on " +
        "the left and rear of Gettysburg' with the monument [E] at the National Cemetery's south end ~80 yards east " +
        "of the Taneytown Road entrance — placed south of Norton's on the cemetery shoulder (dossier " +
        "us-btty-mccartney.md EC3.2/EC3.3); no casualties (tablet) — consistent with the post-repulse arrival",
    },
    {
      t: SunsetT, x: 4890, z: 5470, facing: 260, formation: "line", strength: 145,
      confidence: "inferred",
      citation:
        "holds the cemetery-left position to sunset; activity after arrival was 'firing only four rounds of long " +
        "range solid shot' (or-27-1-mccartney, verbatim — the fire event) plus the 48-round salvage of the withdrawn " +
        "1st NH battery's ammunition (the standing Edgell corroboration, wave A2)",
    },
  ],
};
battle = addUnitsAfter(battle, "us-btty-taft", [mccartney]);
byId.set("us-btty-mccartney", mccartney);

const mccartneyFire = fireEvent({
  id: "us-btty-mccartney-four-rounds",
  kind: "artillery_fire",
  t0: 11100,
  t1: 11400,
  unitId: "us-btty-mccartney",
  confidence: "inferred",
  citation:
    "'firing only four rounds of long range solid shot' after taking position (or-27-1-mccartney No. 238, verbatim, " +
    "PRIMARY own-report; reconstruction/dossiers/us-btty-mccartney.md EC5.1)",
  note:
    "the fire is report-attested; the ~5-minute window ~16:05-16:10 is inferred (four deliberate long-range rounds " +
    "shortly after the ~16:00 arrival) — minutes-grade, LOW intensity",
});

// ---------------------------------------------------------------------------
// 3. The ~17:00 Wheatfield sweep (Bartlett's composite command)
//    (us-v-3-1-mccandless.md EC4.2/EC5.3; us-vi-1-2-bartlett.md EC3.2/EC5.2;
//     csa-1c-hood-4-benning.md EC3.4/EC4.3/EC6)
// ---------------------------------------------------------------------------
const sweepClockNote =
  "sweep clock INFERRED ~17:00-17:45 from the carried spread: Crawford/McCandless stated 'At 5 o'clock on the 3d' " +
  "(+75 early-stated profile), Bartlett's report class ~17:00-18:00, Nevin's tablet '6 p.m.' at the artillery " +
  "recapture — adopted start 17:00, never reconciled";
{
  const mc = unit("us-mccandless");
  const hold = mc.keyframes[0]!;
  const heldGroundCitation =
    "holds the recovered ground to sunset: 'all of the ground lost the previous day' with the recovery ledger — " +
    "~200 prisoners incl. a lieutenant-colonel, the 15th GA colors, one 12-pdr Napoleon, three caissons (9th MA " +
    "Battery materiel per the Nevin tablet's attribution), 'upward of 7,000 stand of arms', the wounded of both " +
    "sides (or-27-1-crawford p. 655; ED-61 cross-credit pool); July 3 sweep loss LIGHT with NO primary day split — " +
    "strength stays flat per the A1 rule (dossier us-v-3-1-mccandless.md EC6)";
  mc.keyframes = [
    hold,
    {
      ...hold, t: 14400,
      citation:
        "advance begins [C]: 'remained in position until about 5 P. M. and then advanced across the Wheatfield' " +
        "(Crawford division tablet verbatim); " + sweepClockNote,
      confidence: "inferred",
    },
    {
      t: 15300, x: 3900, z: 3010, facing: 260, formation: "line",
      strength: hold.strength, confidence: "inferred",
      citation:
        "across the Wheatfield at its east edge (tab-us-v-3-1 3937,3042 — July 3 sweep semantics, ±65 m " +
        "SS-vs-OSM flag carried) before the change of front southward (or-27-1-crawford pp. 653-655; " +
        "or-27-1-mccandless p. 657)",
    },
    {
      t: 16200, x: 3760, z: 2790, facing: 180, formation: "line",
      strength: hold.strength, confidence: "inferred",
      citation:
        "'changed front, swept southward through the woods west and south of the Wheatfield' (tablet) — the 15th GA " +
        "collision: ~200 prisoners + the colors (or-27-1-crawford; Crawford's 'G. T. Anderson' naming is his honest " +
        "wrong-name of a Georgia brigade — the colors fix it as Benning's 15th GA, dossier Conflicts §1); " +
        "Benning's own side: csa-1c-hood-4-benning.md EC4.3",
    },
    {
      t: SunsetT, x: 3760, z: 2790, facing: 180, formation: "line",
      strength: hold.strength, confidence: "inferred",
      citation: heldGroundCitation,
    },
  ];
}
{
  const nv = unit("us-nevin");
  const hold = nv.keyframes[0]!;
  nv.keyframes = [
    hold,
    {
      ...hold, t: 14400,
      citation:
        "UNDER BARTLETT (the composite command, both-sided): 'ordered by Major-General Sedgwick to command Third " +
        "Brigade ... and advance against Hood's division' (or-27-1-bartlett No. 226 pp. 671-673, PRIMARY — resolves " +
        "the Nevin-dossier command-succession flag); " + sweepClockNote,
      confidence: "inferred",
    },
    {
      t: 15300, x: 3980, z: 2960, facing: 260, formation: "line",
      strength: 1360, confidence: "inferred",
      citation:
        "'in a second line at an interval of 200 yards' behind McCandless through the Wheatfield (Bartlett brigade " +
        "tablet verbatim; us-vi-1-2-bartlett.md EC3.2) — decay begins: Bartlett's July 3 loss for THIS command " +
        "'between 20 and 30' killed and wounded (report, verbatim; midpoint 25 authored, inferred)",
    },
    {
      t: 16200, x: 3860, z: 2860, facing: 200, formation: "line",
      strength: 1345, confidence: "inferred",
      citation:
        "'soon after being engaged the Third Brigade Third Division advanced to the front and the combined forces " +
        "captured about 200 prisoners ... and the colors of the 15th Georgia' (tablet verbatim); Nevin's own tablet " +
        "carries the 6 p.m. artillery recapture (9th MA Battery materiel, ED-61 pool)",
    },
    {
      t: SunsetT, x: 3860, z: 2860, facing: 200, formation: "line",
      strength: 1345, confidence: "inferred",
      citation:
        "holds the front line beside McCandless to sunset (the recovered ground record rides us-mccandless; " +
        "Bartlett's 'between 20 and 30' fully applied — total July 3 decay 25 inferred midpoint)",
    },
  ];
}
{
  const bn = unit("csa-benning");
  const hold = bn.keyframes[0]!;
  bn.keyframes = [
    hold,
    {
      ...hold, t: 16200,
      citation:
        "'Held Devil's Den and the adjacent crest of rocky ridge until late in the evening' (tablet verbatim) with " +
        "the left flank 'entirely exposed' after the McLaws-wing extraction, picket screen improvised " +
        "(or-27-2-benning; csa-1c-hood-4-benning.md EC5.3); retirement clock INFERRED ~17:30 — 'late in the " +
        "evening' is unclocked [D] and is sequenced against the attested ~17:00 Union sweep whose recovery record " +
        "is the other side (both readings carried, not reconciled)",
      confidence: "inferred",
    },
    {
      t: 17400, x: 3450, z: 2400, facing: 250, formation: "column",
      strength: 820, confidence: "inferred",
      citation:
        "THE 15th GEORGIA'S DISASTER during the retirement: 'Through mistake of orders the 15th Georgia did not " +
        "retire directly but moved northward encountered a superior Union force and suffered considerable loss' " +
        "(tablet verbatim) — the Union side counts ~200 prisoners + the colors (or-27-1-crawford); decay -170 here " +
        "+ -50 to the rally is INFERRED against that attested capture mass ('considerable loss' k+w unquantified — " +
        "ED-49-class floor honesty; the brigade's 3-day EC6 carries 497-509)",
    },
    {
      t: 19800, x: 2790, z: 2270, facing: 250, formation: "line",
      strength: 770, confidence: "inferred",
      citation:
        "'under orders the Brigade retired to position near here' (tablet verbatim — the tablet's own ground, " +
        "tab-csa-benning 2785,2269 on South Confederate Ave); pace ~0.6 m/s over ~1.4 km with the fight on the " +
        "column [B]",
    },
    {
      t: SunsetT, x: 2790, z: 2270, facing: 250, formation: "line",
      strength: 770, confidence: "inferred",
      citation:
        "holds the retired line at sunset; July 4 'breastworks facing south until midnight' follows " +
        "(csa-1c-hood-4-benning.md EC3.5)",
    },
  ];
}

// ---------------------------------------------------------------------------
// 4. Taft's flank fire — extended to his report's cease
//    (us-btty-taft.md EC5.2; or-27-1-taft No. 325)
// ---------------------------------------------------------------------------
{
  const ev = event("us-btty-taft-pettigrew-flank");
  if (ev.t1 !== 10800) throw new Error("expected Taft's A2 window end (10800)");
  ev.t1 = 18000;
  ev.citation =
    "own-report PRIMARY for the cease: 'firing ceased ~6 P.M.' (or-27-1-taft No. 325 pp. 891-892; " +
    "reconstruction/dossiers/us-btty-taft.md EC5.2) — the report wins on grade now that the window reaches it; " +
    "the marker's 'Engaged at intervals in same position until 4 p.m.' " +
    "(https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/" +
    "5th-new-york-independent-battery/) carried as the counter-reading";
  ev.note = (ev.note ?? "") +
    "; DAY-EXPANSION SLICE 1: t1 10800 -> 18000 — wave A2 had extended this window to the marker's 4 P.M. " +
    "because the report's ~6 P.M. cease lay beyond the old window end (the conflict was carried, " +
    "authoring-wave-a2.md §1); the widened window retires that compromise: report-primary 18:00 authored, " +
    "marker 4 P.M. carried";
  const kfs = unit("us-btty-taft").keyframes;
  const end = kfs[kfs.length - 1]!;
  kfs.push({
    ...end, t: 18000, confidence: "documented",
    citation:
      "the cease, own report: firing ceased ~6 P.M. (or-27-1-taft No. 325 pp. 891-892 — PRIMARY, own-battery-scoped); " +
      "expenditure itemized by type: '80 Schenkl percussion shell, 63 Schenkl combination shrapnel, 32 Parrott " +
      "time-fuse shell, 382 Parrott time-fuse shrapnel, and 557 cartridges' (ED-42 expenditure-ledger class)",
  });
}

// ---------------------------------------------------------------------------
// 5. Wheeler's post-16:00 remainder (us-btty-wheeler.md EC5.3, Conflicts §1)
// ---------------------------------------------------------------------------
{
  const ev = event("us-btty-wheeler-canister");
  if (ev.t1 !== 10800) throw new Error("expected Wheeler's window-end t1");
  ev.t1 = 11400;
  ev.note = (ev.note ?? "") +
    "; DAY-EXPANSION SLICE 1: the post-16:00 remainder now modeled — t1 10800 -> 11400 (inferred ~10 min: his " +
    "stated 'about 4 p.m.' arrival is profiled -35 [-55,0] and his three-halt enfilade reads mid-fight; the " +
    "remainder is bounded short because the assault had collapsed — the alternative clock-error reading (arrival " +
    "mid-repulse) stays open for a re-timing pass, us-btty-wheeler.md Conflicts §1)";
  const kfs = unit("us-btty-wheeler").keyframes;
  const end = kfs[kfs.length - 1]!;
  if (!end.citation?.includes("post-window remainder not modeled"))
    throw new Error("expected Wheeler's not-modeled annotation");
  end.citation = end.citation.replace(
    "(his canister fight runs past 16:00 — post-window remainder not modeled)",
    "(his canister fight runs past 16:00 — remainder modeled by day-expansion slice 1, see the t=11400 keyframe)");
  kfs.push({
    ...end, t: 11400, confidence: "inferred",
    citation:
      "the canister fight's inferred end (~16:10): Hancock-assigned position held; no withdrawal or further fire " +
      "attested after the fight — the battery stands on the II Corps front (or-27-1-wheeler pp. 752-753; 850 rounds " +
      "expended whole-battle, no per-day split — EC5.4 honesty)",
  });
}

// ---------------------------------------------------------------------------
// 6. The tablet-attested all-day skirmish events reach the tablets' own scope
// ---------------------------------------------------------------------------
for (const id of ["csa-hays-town-sharpshooting", "csa-perrin-long-lane-skirmish",
  "csa-thomas-long-lane-skirmish"]) {
  const ev = event(id);
  if (ev.t1 !== 10800) throw new Error(`expected all-window t1 on ${id}`);
  ev.t1 = SunsetT;
  ev.note = (ev.note ?? "") +
    "; DAY-EXPANSION SLICE 1: t1 10800 -> 23340 — the window was the old " +
    "clip, the tablet's scope is the day ('constantly engaged' / 'most of " +
    "the day' / the town sharpshooting) — the event now spans to sunset " +
    "with the same day-scoped attestation";
}

// ---------------------------------------------------------------------------
// 7. Evening-state keyframes for the assault column (pass-3 rally records)
// ---------------------------------------------------------------------------
const eveningStates: Array<{ id: string; formation: Keyframe["formation"]; citation: string }> = [
  {
    id: "csa-garnett", formation: "scattered",
    citation:
      "evening state, dissolution-class: 'about 300 ... came off slowly, but greatly scattered, the identity of " +
      "every regiment being entirely lost' (peyton-or-1863); rally behind Anderson's division per the corps record " +
      "(or-27-2-longstreet) — no formed brigade line to draw (reconstruction/dossiers/csa-1c-pic-1-garnett.md EC4.5)",
  },
  {
    id: "csa-kemper", formation: "scattered",
    citation:
      "evening state, dissolution-class with the division (Kemper w&c-rescued; retreat under the reopened arc fire, " +
      "division record) — no formed brigade line to draw (reconstruction/dossiers/csa-1c-pic-2-kemper.md EC4)",
  },
  {
    id: "csa-armistead", formation: "scattered",
    citation:
      "evening state, dissolution-class with the division: '643 missing ... never marched back' (the ED-8 record; " +
      "reconstruction/dossiers/csa-1c-pic-3-armistead.md EC4.4) — no formed brigade line to draw",
  },
  {
    id: "csa-fry", formation: "line",
    citation:
      "evening reform: 'Those who remained at the works saw that it was a hopeless case, and fell back ... we " +
      "reformed upon the ground from which we advanced' (or-27-2-shepard; reconstruction/dossiers/" +
      "csa-3c-het-3-fry.md EC4.4) — a thin reformed line on the morning ground",
  },
  {
    id: "csa-marshall", formation: "line",
    citation:
      "evening state: retreat endpoint = the morning position by ~16:00 canonical [C via the davis-jr clock] " +
      "(or-27-2-jones-26nc; reconstruction/dossiers/csa-3c-het-1-marshall.md EC4) — 'but one field officer' left " +
      "to reform it",
  },
  {
    id: "csa-davis", formation: "line",
    citation:
      "evening state: returned 'about 4 p.m.' to the morning line (or-27-2-davis-jr — THE batch's retreat clock, " +
      "+5-class chain-agreeing profile; reconstruction/dossiers/csa-3c-het-4-davis.md EC4) and holds it to sunset",
  },
  {
    id: "csa-brockenbrough", formation: "line",
    citation:
      "evening state: rallying at the start line (ABT maps; the ED-48 honestly-bounded record — no report, no " +
      "officer pins; reconstruction/dossiers/csa-3c-het-2-brockenbrough.md)",
  },
  {
    id: "csa-lane", formation: "line",
    citation:
      "evening reform, ORDERED-class: 'reformed immediately in rear of the artillery' (or-27-2-lane-jh); Cols. " +
      "Avery and Barry brought the two left regiments back 'under direct orders' (or-27-2-engelhard) — the third " +
      "line's ordered retreat vs the first line's dissolution, the pass-3 rendering distinction " +
      "(reconstruction/dossiers/csa-3c-pen-2-lane.md EC4.3)",
  },
  {
    id: "csa-lowrance", formation: "line",
    citation:
      "evening reform: 'without orders, the brigade retreated, leaving many on the field'; rallied 'on the same " +
      "line where it was first formed' (or-27-2-lowrance; reconstruction/dossiers/csa-3c-pen-4-lowrance.md EC4.3)",
  },
  {
    id: "csa-wilcox", formation: "line",
    citation:
      "evening reform: 'My line was reformed on the ground it occupied before it advanced' (or-27-2-wilcox; " +
      "reconstruction/dossiers/csa-3c-and-1-wilcox.md EC3.4) and holds to sunset",
  },
  {
    id: "csa-lang", formation: "line",
    citation:
      "evening reform: 'Falling back to our artillery, we reformed in our old line' (or-27-2-lang); the NIGHT " +
      "withdrawal to the original woods line by Anderson's order falls past sunset and is NOT modeled " +
      "(reconstruction/dossiers/csa-3c-and-4-lang.md EC3.3)",
  },
];
for (const row of eveningStates) {
  const kfs = unit(row.id).keyframes;
  const end = kfs[kfs.length - 1]!;
  if (end.t !== 10800) throw new Error(`expected 10800 end on ${row.id}`);
  kfs.push({
    ...end, t: SunsetT, formation: row.formation,
    confidence: "inferred", citation: row.citation,
  });
}

battle.events = [...(battle.events ?? []), mccartneyFire];

// ---------------------------------------------------------------------------
// the cast guard, then export
// ---------------------------------------------------------------------------
for (const [id, before] of castBefore) {
  const after = JSON.stringify(
    byId.get(id)!.keyframes.filter((k) => k.t <= 10800));
  if (after !== before)
    throw new Error(`day-expansion guard: '${id}' existing keyframes changed`);
}
writeFileSync(battlePath, exportValidated(battle) + "\n");

// moments.json — the sunset window's cited markers (strictly increasing t)
{
  const moments = JSON.parse(readFileSync(momentsPath, "utf8"));
  moments.moments.push(
    {
      t: 10800,
      label: "The field falls quiet",
      detail:
        "By about 4 p.m. the assault's survivors are back on Seminary Ridge — Davis's brigade returns to its " +
        "morning line; McCartney's battery reaches the cemetery too late to fire more than four rounds.",
      citation:
        "or-27-2-davis-jr ('about 4 p.m.', the pass-3 retreat clock; dossier csa-3c-het-4-davis.md EC4); " +
        "or-27-1-mccartney (the arrived-too-late record)",
    },
    {
      t: 14400,
      label: "The Wheatfield sweep",
      detail:
        "McCandless's Reserves, with Nevin's brigade under Bartlett behind, advance across the Wheatfield and " +
        "sweep the woods south — the 15th Georgia is caught: ~200 prisoners and the colors (~17:00).",
      citation:
        "or-27-1-crawford pp. 653-655; or-27-1-bartlett No. 226; Crawford division tablet ('about 5 P. M.' stated, " +
        "+75 profile) — inferred 17:00 canonical, spread carried (dossiers us-v-3-1-mccandless.md, " +
        "us-vi-1-2-bartlett.md)",
    },
    {
      t: 18000,
      label: "Taft's guns cease",
      detail:
        "The 5th New York's fire from the Baltimore Pike ends about 6 p.m. by Taft's own report — his marker says " +
        "4 p.m.; the conflict is carried, not reconciled.",
      citation:
        "or-27-1-taft No. 325 pp. 891-892 (report-primary cease) vs the Baltimore Pike marker's 'until 4 p.m.' " +
        "(dossier us-btty-taft.md EC5.2)",
    },
    {
      t: 23340,
      label: "Sunset",
      detail:
        "Sunset at 19:29 local mean time closes the phase; the armies hold their evening lines.",
      citation:
        "ED-31 (docs/reconstruction/angle-editorial-decisions.md — the astronomical pin for 39.82N 77.23W, " +
        "July 1863; battle clock = Gettysburg local mean time)",
    },
  );
  writeFileSync(momentsPath, JSON.stringify(moments, null, 2) + "\n");
}

// battle-manifest.json — the afternoon phase's clock echo follows the file
{
  const manifest = JSON.parse(readFileSync(manifestPath, "utf8"));
  const july3 = manifest.days.find((d: any) => d.id === "july3");
  const phase = july3.phases.find((p: any) => p.id === "july3-afternoon");
  phase.endTime = SunsetT;
  phase.label = "Afternoon — bombardment to sunset";
  manifest._comment = manifest._comment.replace(
    "startTime 46800 (13:00 LMT, ED-24/ED-31 frame).",
    "startTime 46800 (13:00 LMT, ED-24/ED-31 frame), endTime 23340 (sunset 19:29 LMT, ED-31 astronomical pin).");
  writeFileSync(manifestPath, JSON.stringify(manifest, null, 2) + "\n");
}

console.log(
  `day-expansion slice 1 (sunset widening) written: endTime ${battle.endTime}, ` +
  `${battle.units.length} units (+1 McCartney), ${battle.events?.length ?? 0} events ` +
  `(+1 four-rounds; Taft -> 18000, Wheeler -> 11400, 3 skirmish windows -> sunset); ` +
  `3 sweep movers authored, 11 evening-state keyframes added, 4 moments added`);
