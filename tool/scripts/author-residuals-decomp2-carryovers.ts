// residuals-decomp-2, task 3 — the decomposition-wave-1 §7 honest deferral,
// picked up: the 21st Mississippi's promotion out of Barksdale's brigade
// (csa-humphreys), ED-76 parentless-sibling pattern, both July 2 files.
//
// The 9th Massachusetts Battery's "two-section Trostle structure" (the
// OTHER wave-1 carryover) is NOT authored here — deferred, honestly, with
// its own reason recorded in docs/reconstruction/audit/residuals-decomp-2.md
// task 3: no distinct EC3 position anchor exists for Milton's retiring
// 2-gun section separate from the whole-battery Trostle-stand ground (the
// existing single us-btty-bigelow track already terminates at the "remnant
// behind the Plum Run line" ground, i.e. already follows the SURVIVING
// section's own path — splitting the track would relabel, not add,
// geometry), and ED-61 (docs/reconstruction/angle-editorial-decisions.md)
// explicitly warns against "per-claimant duplicate guns" bookkeeping for
// this exact battery's lost/recovered materiel — a second live "captured
// guns" unit risks exactly that double-count. Authoring it would be a
// stretch past what the dossier supports; deferring is the honest call.
//
// Source: csa-1c-mcl-3-barksdale.md (EC3.3: "'21' MISS' drawn separately at
// the Trostle ground (3617,3643) ... over 'BIGELOW'" — the divergence
// primary; EC6: the ANV return's own regiment row, 21st MS 103 k+w, exact);
// us-btty-bigelow.md (the Trostle-angle capture keyframe cross-reference,
// t=12900 in gettysburg-july2-afternoon.json's own us-btty-bigelow track —
// the SAME real-world moment, borrowed as the 21st's own spike timestamp).
//
// Design: ED-76 (adopted, docs/reconstruction/angle-editorial-decisions.md)
// — a parentless sibling, not `parent`/family (Barksdale's other three
// regiments — 13th/17th/18th MS — stay at aggregate grain; giving the 21st
// a `parent` link would suppress them at nearer-than-Block LOD tiers for a
// 1-of-4 promotion, the same rendering-regression rationale decomposition-
// wave-1 established). The residual brigade is renamed "... minus the 21st
// Mississippi" (the us-carroll/us-8oh convention) and reduced by EXACT
// subtraction at every shared keyframe.
//
// Strength grain: no regiment-level STARTING-strength primary exists for
// any of Barksdale's four regiments (dossier EC2: "No primary strength
// statement (negative; no report exists)" — brigade-wide tablet "Present
// 1,598" only). Per the SAME compilation-class-flagged-even-split technique
// decomposition-wave-1 used for the 6th Wisconsin (dossier us-i-1-1-
// meredith.md Conflicts item 4: "only the 24th's 496 fetched... split
// evenly across the four un-primaried regiments"), the 21st's starting
// share is 1,598 / 4 = 399.5, rounded to 400, FLAGGED compilation-class in
// the citation — held FLAT (no invented interim decay) through the shared
// pre-divergence fighting (the Peach Orchard breach, t=0..12000), exactly
// like the 6th Wisconsin held flat pre-charge; the residual absorbs 100% of
// that shared, per-regiment-unattributable loss by construction (exact
// subtraction at every one of Barksdale's own original keyframe times).
// At the Trostle capture (t=12900, cross-referenced to us-btty-bigelow's
// own keyframe), the 21st's OWN real primary applies: the ANV return's
// 21st-MS regimental row, 103 k+w (missing not broken out — the true total
// loss floor, not invented above it) — 400 - 103 = 297, EXACT subtraction,
// the regiment's own attested episode, not a further proportional guess.
//
// Conservation check (printed, not silently assumed): 21st + residual sums
// to Barksdale's ORIGINAL pre-decomposition build value at EVERY one of the
// brigade's own shared keyframes, by construction.
//
// Run from tool/:
//   npx vite-node scripts/author-residuals-decomp2-carryovers.ts
// Report: docs/reconstruction/audit/residuals-decomp-2.md
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe, Unit } from "../src/model";
import { addUnitsAfter, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const afternoonPath = join(here, "../../app/Assets/Battle/gettysburg-july2-afternoon.json");
const eveningPath = join(here, "../../app/Assets/Battle/gettysburg-july2-evening.json");

let afternoon: Battle = JSON.parse(readFileSync(afternoonPath, "utf8"));
let evening: Battle = JSON.parse(readFileSync(eveningPath, "utf8"));

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  citation: string, confidence: Keyframe["confidence"] = "documented"): Keyframe =>
  ({ t, x, z, facing, formation, strength, confidence, citation });

function patchUnit(battle: Battle, id: string, edit: (u: Unit) => void) {
  const u = battle.units.find((x) => x.id === id);
  if (!u) throw new Error(`patchUnit: '${id}' missing`);
  edit(u);
}

// ===========================================================================
// THE 21ST MISSISSIPPI (parentless sibling of csa-humphreys / Barksdale's
// brigade), gettysburg-july2-afternoon.json
// ===========================================================================

// Original Barksdale/Humphreys shared keyframe times + pre-decomposition
// combined values, transcribed from the committed file before this edit
// (kept here as the conservation check's ground truth): t=0:1598, t=9900:
// 1598, t=10800:1450, t=12000:1250, t=12600:1000, t=14340:851.
const ORIG_HUMPHREYS_AFTERNOON: Record<number, number> = {
  0: 1598, 9900: 1598, 10800: 1450, 12000: 1250, 12600: 1000, 14340: 851,
};

const START_SHARE_21MS = 400; // 1,598 / 4 = 399.5, rounded; compilation-class, flagged
const LOSS_21MS = 103; // ANV return's own regiment row, 21st MS, k+w exact (dossier EC6)
const END_21MS_AFTERNOON = START_SHARE_21MS - LOSS_21MS; // 297

const ms21Afternoon: Keyframe[] = [
  kf(0, 2923, 3865, 75, "line", START_SHARE_21MS,
    "With the brigade on the Warfield/Seminary Ridge line (tablet 'Arrived about 3 P.M. and formed line here'; j2-03 poised state). STRENGTH HONESTY: no regiment-level strength primary exists for any of Barksdale's four regiments (dossier csa-1c-mcl-3-barksdale.md EC2: 'No primary strength statement (negative; no report exists)' — only the brigade-wide tablet 'Present 1,598'). Authored 400 compilation-class FLAGGED (1,598 / 4, rounded — the SAME even-split technique decomposition-wave-1 used for the 6th Wisconsin, dossier us-i-1-1-meredith.md Conflicts item 4)."),
  kf(9900, 2923, 3865, 75, "line", START_SHARE_21MS,
    "Step-off ~18:15 with the brigade (CA-J2A-9, downstream-CONFIRMED) — still one line; the divergence has not yet happened."),
  kf(10800, 3220, 3760, 70, "line", START_SHARE_21MS,
    "The Peach Orchard salient smashed — the 21st MS fights through the breach WITH the other three regiments (dossier EC4.2: the divergence happens at 'the breach and the wheel', i.e. AFTER this point, not during it). No regiment-grain attribution exists for this shared loss (brigade 1,598→1,450 here); held FLAT — the residual brigade absorbs the full shared delta by construction (exact subtraction, not an invented per-regiment split of an unattributable shared loss)."),
  kf(12000, 3617, 3643, 90, "line", START_SHARE_21MS,
    "THE DIVERGENCE: 'the regiments inclining left to Plum Run except the 21st MS' (tablet, csa-1c-mcl-3-barksdale.md EC3.2); drawn j2-04 (19:00) puts '21' MISS' separately at the Trostle ground OVER 'BIGELOW' with 'WATSON' (I/5th US) nearby — (3617,3643), the divergence's own primary coordinate (dossier EC3.3, read 2026-07-11). Facing east ('straight east past the Trostle house onto Bigelow's and Watson's batteries', EC4.2) while the brigade's other three regiments wheel northeast toward Plum Run (csa-humphreys' own t=12000 keyframe, this file). Strength still 400 — no loss yet attributed to the 21st specifically at this instant."),
  kf(12900, 3617, 3643, 100, "line", END_21MS_AFTERNOON,
    "THE TROSTLE CAPTURE, cross-referenced to us-btty-bigelow's own t=12900 keyframe (SAME real-world moment, this file): 'silenced the four pieces on my right, and prevented their withdrawal' (Milton, or-27-1-milton — the 9th MA Battery's own record); 'captured but were unable to bring off' (Barksdale's brigade tablet, csa-1c-mcl-3-barksdale.md EC5.3) — Bigelow's four guns and Watson's I/5th US four guns both taken at this ground. LOSS PRIMARY, exact: the ANV return's own 21st-Mississippi regimental row, 103 k+w (or-27-2-anv-return pp. 338-339, dossier EC6 — the SAME return whose brigade-wide total the tablet itemizes exactly; missing not broken out by regiment in the return, so 103 is the k+w floor, not an invented full total). 400 − 103 = 297."),
  kf(14340, 3617, 3643, 100, "line", END_21MS_AFTERNOON,
    "Held at the Trostle ground through the window's end. HONEST GAP (EC3/EC4, matching the corpus's own convention for this class of gap — e.g. decomposition-wave-1's 6th Wisconsin between its crest re-advance and the Culp's Hill rendezvous): no citation attests the 21st Mississippi specifically rejoining the other three regiments' fall-back to the Emmitsburg Road ground before dark; the brigade-wide reconsolidation (csa-humphreys' own evening-file citation, 'Re-formed... under Humphreys') is a BRIGADE fact, not separately regiment-attested. No further loss documented (held flat, not invented)."),
];

// Residual: Barksdale's brigade (13th/17th/18th MS), EXACT subtraction of
// the 21st's above figures at every one of the brigade's OWN original
// keyframe times (the times already committed in gettysburg-july2-
// afternoon.json before this edit).
const humphreysResidualAfternoon: Keyframe[] = [
  kf(0, 2923, 3865, 75, "line", ORIG_HUMPHREYS_AFTERNOON[0]! - START_SHARE_21MS,
    "The Warfield/Seminary Ridge line: tablet 'Arrived about 3 P.M. and formed line here'; Alexander's 18 guns deployed IN FRONT of the brigade (the batteries-mixed-with-the-lines extraction delay, mclaws-shsp7-1878); j2-03 (17:00) draws the regimental numbers with advance arrows POISED at (2923,3865) — the drawn state siding with the ladder against the tablet's 'Advanced at 5 P.M.' (ED-55 skew class). Strength: brigade tablet 'Present 1,598' MINUS the 21st Mississippi's now-separately-tracked compilation-class share (400, own citation, us-21ms) = 1,198 (ED-76, residuals-decomp-2). Dossier csa-1c-mcl-3-barksdale.md."),
  kf(9900, 2923, 3865, 75, "line", ORIG_HUMPHREYS_AFTERNOON[9900]! - START_SHARE_21MS,
    "Step-off ~18:15 (CA-J2A-9, downstream-CONFIRMED): McLaws's Lamar-carried order; Kershaw's drums pin (+20-40 min after CA-J2A-7); Alexander's 'at least 20 minutes' interval arithmetic."),
  kf(10800, 3220, 3760, 70, "line", ORIG_HUMPHREYS_AFTERNOON[10800]! - START_SHARE_21MS,
    "The Peach Orchard salient smashed: the assault axis through the salient angle at the Wentz corner (~(3164,3634) reference, tablet); Graham w&c on the receiving side (CA-J2A-9's Union pin). Decay: the original combined −148 delta at this keyframe, carried in full by these three regiments (the 21st MS was in the SAME breach fight, per its own citation, but no regiment-grain attribution exists for this shared loss — exact subtraction from the flat 21st share, not an invented split, per this pass's design note)."),
  kf(12000, 3900, 4050, 55, "line", ORIG_HUMPHREYS_AFTERNOON[12000]! - START_SHARE_21MS,
    "THE DIVERGENCE: 'the regiments inclining left to Plum Run except the 21st MS' (tablet) — the three remaining regiments (13th/17th/18th MS) wheel northeast toward Plum Run while the 21st goes east to the Trostle ground (own citation, us-21ms). Position adjusted off the pre-decomposition single-centroid reading (3650,3900) toward the Plum Run axis specifically, since the 21st's own divergent position is now separately tracked."),
  kf(12600, 4020, 4070, 60, "line", ORIG_HUMPHREYS_AFTERNOON[12600]! - START_SHARE_21MS,
    "THE PLUM RUN CLIMAX ~19:00: red bar against 'WILLARD's Brig' at (4053,4080) (bachelder j2-04); Barksdale mw here (tablet narrative + Hall's 20-yards receiving statement, compatible at ~100 m grain; died July 3 at the Hummelbaugh farm) — the three-regiment element's own fight; the file's own clock has the 21st MS's Trostle capture at t=12900 (cross-referenced to us-btty-bigelow's own keyframe), i.e. AFTER this instant, so the 21st's flat pre-capture share (400) is still what this residual subtracts here, not yet its post-capture value."),
  kf(14340, 3820, 3960, 60, "line", ORIG_HUMPHREYS_AFTERNOON[14340]! - END_21MS_AFTERNOON,
    "Repulsed by Willard's counterattack, falling back westward at dark (both-sided: us-ii-3-3-willard.md). Decay: tablet 105/550/92 = 747 brigade-wide (the tablet that itemizes the return's pooled missing EXACTLY, pass-5 find, ED-43 satisfied) MINUS the 21st Mississippi's own separately-tracked 103-loss share (own citation, us-21ms) = 644, this residual's own day loss (1,198 → 554). CONSERVATION CHECK: 554 (residual) + 297 (21st) = 851 = the pre-decomposition build's own t=14340 value, EXACT."),
];

// ===========================================================================
// THE 21ST MISSISSIPPI, gettysburg-july2-evening.json — held flat (no
// citation attests further movement or loss; the honest EC3/EC4 gap named
// on the afternoon file's own final keyframe carries forward).
// ===========================================================================
const ms21Evening: Keyframe[] = [
  kf(0, 3617, 3643, 100, "line", END_21MS_AFTERNOON,
    "Continuity: held at the Trostle ground, the afternoon file's own final attested position for this regiment (own citation, us-21ms, that file's t=14340) — no citation attests the 21st Mississippi specifically rejoining the brigade's own night reconsolidation (an honest EC3/EC4 gap, not closed by this pass)."),
  kf(10860, 3617, 3643, 100, "line", END_21MS_AFTERNOON,
    "The night line — same honest gap; no further movement or loss attested for this regiment specifically."),
];

const humphreysResidualEvening: Keyframe[] = [
  kf(0, 3820, 3960, 60, "line", 851 - END_21MS_AFTERNOON,
    "Continuity: Barksdale's OTHER three regiments' (13th/17th/18th MS) survivors falling back from Plum Run — 851 brigade-wide MINUS the 21st Mississippi's own separately-tracked 297 (own citation, us-21ms) = 554 (ED-76, residuals-decomp-2)."),
  kf(2760, 3450, 3830, 60, "line", 851 - END_21MS_AFTERNOON,
    "Re-formed toward the Peach Orchard/Emmitsburg road ground under Humphreys (the brigade's night frame; Barksdale mw left on the field, dying at the Hummelbaugh farm July 3) — these three regiments' own reconsolidation; the 21st Mississippi's own whereabouts after the Trostle capture are this pass's honest gap (own citation, us-21ms)."),
  kf(10860, 3450, 3830, 250, "line", 851 - END_21MS_AFTERNOON,
    "The night line."),
];

// ---------------------------------------------------------------------------
// Apply
// ---------------------------------------------------------------------------
patchUnit(afternoon, "csa-humphreys", (u) => {
  u.name = "Barksdale's Brigade, McLaws's Division (minus the 21st Mississippi)";
  u.frontage_m = 323; // 1,198 * 0.27, the fullcast-lib frontage heuristic, this file's t=0 share
  u.keyframes = humphreysResidualAfternoon;
});
patchUnit(evening, "csa-humphreys", (u) => {
  u.name = "Barksdale's Brigade, McLaws's Division (minus the 21st Mississippi)";
  u.frontage_m = 150; // 554 * 0.27, rounded
  u.keyframes = humphreysResidualEvening;
});

const ms21AfternoonUnit: Unit = {
  id: "csa-21ms", name: "21st Mississippi (Barksdale's)",
  side: "confederate", frontage_m: 108 /* 400 * 0.27 */, depth_m: 20,
  keyframes: ms21Afternoon,
};
const ms21EveningUnit: Unit = {
  id: "csa-21ms", name: "21st Mississippi (Barksdale's)", side: "confederate",
  frontage_m: 80 /* 297 * 0.27, rounded */, depth_m: 20,
  keyframes: ms21Evening,
};

afternoon = addUnitsAfter(afternoon, "csa-humphreys", [ms21AfternoonUnit]);
evening = addUnitsAfter(evening, "csa-humphreys", [ms21EveningUnit]);

// Unit-scoped activity event: the Trostle capture.
const newAfternoonEvents: EngagementEvent[] = [
  {
    id: "j2p-21ms-trostle-capture", kind: "musketry", t0: 12600, t1: 12900,
    unitId: "csa-21ms", confidence: "documented",
    citation: "The 21st Mississippi overruns Bigelow's and Watson's guns at the Trostle angle: 'silenced the four pieces on my right, and prevented their withdrawal' (Milton, or-27-1-milton); 'captured but were unable to bring off' (Barksdale's brigade tablet). Cross-referenced to us-btty-bigelow's own t=12900 keyframe, same real-world moment.",
  },
];
afternoon = { ...afternoon, events: [...(afternoon.events ?? []), ...newAfternoonEvents] };

// ---------------------------------------------------------------------------
// Conservation + continuity audit (printed, not silently assumed).
// ---------------------------------------------------------------------------
for (const [tStr, orig] of Object.entries(ORIG_HUMPHREYS_AFTERNOON)) {
  const t = Number(tStr);
  const ms21 = ms21Afternoon.filter((k) => k.t <= t).at(-1)!;
  const residual = humphreysResidualAfternoon.find((k) => k.t === t)!;
  const sum = ms21.strength + residual.strength;
  if (sum !== orig)
    throw new Error(`conservation break at t=${t}: 21st(${ms21.strength}) + residual(${residual.strength}) = ${sum} != orig ${orig}`);
}
console.log("conservation: csa-21ms + csa-humphreys(residual) == the pre-decomposition Barksdale build value at every shared afternoon keyframe");

const mEnd = ms21Afternoon.at(-1)!;
const aStart = ms21Evening[0]!;
if (mEnd.strength !== aStart.strength || mEnd.x !== aStart.x || mEnd.z !== aStart.z)
  throw new Error(`csa-21ms continuity break: afternoon end (${mEnd.x},${mEnd.z},${mEnd.strength}) vs evening t=0 (${aStart.x},${aStart.z},${aStart.strength})`);
console.log("continuity: csa-21ms afternoon end == evening t=0, exact");

const hEnd = humphreysResidualAfternoon.at(-1)!;
const hStart = humphreysResidualEvening[0]!;
if (hEnd.strength !== hStart.strength)
  throw new Error(`csa-humphreys residual continuity break: afternoon end ${hEnd.strength} vs evening t=0 ${hStart.strength}`);
console.log("continuity: csa-humphreys (residual) afternoon end strength == evening t=0 strength, exact");

writeFileSync(afternoonPath, exportValidated(afternoon));
writeFileSync(eveningPath, exportValidated(evening));
console.log(`wrote ${afternoonPath}: ${afternoon.units.length} units, ${(afternoon.events ?? []).length} events`);
console.log(`wrote ${eveningPath}: ${evening.units.length} units, ${(evening.events ?? []).length} events`);
