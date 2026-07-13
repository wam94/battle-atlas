// Day expansion slice 2 — THE JULY 2 EVENING PHASE (Culp's Hill and East
// Cemetery Hill by twilight, 19:29–22:30 LMT). Run from tool/ AFTER the
// afternoon script (this file starts every unit at that file's cited end
// state — the cross-phase continuity rule):
//   npx vite-node scripts/author-dayexp2-evening.ts
// Slice report: docs/reconstruction/audit/day-expansion-slice-2.md.
//
// THE PHASE (ADR 0005): startTime 70140 (19:29 LMT — abutting the
// afternoon phase at the ED-31/CA-J2A-11 sunset pin; reconstructed phases
// within a day may not overlap, battle-manifest.md) · endTime 10860
// (22:30 — CA-J2E-5's adopted envelope: Hays 'about 10 o'clock', Greene's
// Kane-return 10 o'clock, Geary 'about 10 p. m.'; Nicholls's four-hours
// reading runs to ~23:00 and is recorded, not adopted — ED-62).
//
// CONTENT: the ED-29/ED-62 chain at executor grain (CA-J2E-2..5), the
// ED-63 battery-possession ruling, the ED-61 gun-recovery ledger, the XII
// Corps returns, and the southern field's cited night states. NO invention;
// conflicts carried; trigger/sequence anchors author `inferred`.
import { writeFileSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe, Unit } from "../src/model";
import { fireEvent, moverUnit, staticUnit, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const afternoonPath = join(here, "../../app/Assets/Battle/gettysburg-july2-afternoon.json");
const outPath = join(here, "../../app/Assets/Battle/gettysburg-july2-evening.json");
const afternoon: Battle = JSON.parse(readFileSync(afternoonPath, "utf8"));

const StartTime = 70140; // 19:29 LMT
const EndT = 10860; // 22:30 LMT
const T = (h: number, m: number) => h * 3600 + m * 60 - StartTime;

const aById = new Map(afternoon.units.map((u) => [u.id, u]));
function endPose(id: string) {
  const u = aById.get(id);
  if (!u) throw new Error(`dayexp2-evening: afternoon unit '${id}' missing`);
  const k = u.keyframes[u.keyframes.length - 1]!;
  return { x: k.x, z: k.z, facing: k.facing, formation: k.formation, strength: k.strength };
}

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  confidence: Keyframe["confidence"], citation?: string): Keyframe => ({
    t, x, z, facing, formation, strength,
    ...(confidence !== undefined && { confidence }),
    ...(citation !== undefined && { citation }),
  });

// ---------------------------------------------------------------------------
// MOVERS — the evening arcs.
// ---------------------------------------------------------------------------
interface M { id: string; kfs: Keyframe[] }
const movers: M[] = [
  // ------------------- CULP'S HILL (CA-J2E-2 continued) -------------------
  {
    id: "csa-dungan",
    kfs: [
      kf(0, 5960, 5850, 235, "line", 1560, "documented",
        "Continuity from the afternoon phase's 19:29 state: Jones's brigade on the steep east face against Greene's right (or-27-2-jones-jm; CA-J2E-2)."),
      kf(T(20, 15), 5880, 5760, 240, "line", 1440, "documented",
        "Two assaults pressed and repulsed in 'darkness in the woods' (or-27-2-jones-jm); Jones w this evening ('When near the first line of intrenchments ... I received a flesh wound' — the pass-8 register correction); Higginbotham, Cobb, Pendleton Jones pins on the slope."),
      kf(T(21, 0), 6120, 5890, 235, "line", 1400, "documented",
        "Withdrawn to the low ground at the creek after the repulses (his report's sequence)."),
      kf(EndT, 6120, 5890, 235, "line", 1400, "documented",
        "Holds the bottom overnight (the July 3 morning renewal under Dungan is the next phase's record)."),
    ],
  },
  {
    id: "csa-williams",
    kfs: [
      kf(0, 6030, 5780, 235, "line", 1070, "documented",
        "Continuity: Nicholls's brigade climbing at sunset (or-27-2-williams-nicholls)."),
      kf(T(20, 0), 5975, 5752, 235, "line", 1000, "documented",
        "The fire line 'about 100 yards from the enemy's works' — tab-csa-2c-joh-3 (5968,5748); the assaults failed to carry the crest."),
      kf(EndT, 5975, 5752, 235, "line", 940, "documented",
        "'Held during the night' at the fire line (his report verbatim); the loss split across the two four-hour fires carried half-half AS FLAGGED RECONSTRUCTION (dossier EC6)."),
    ],
  },
  {
    id: "csa-steuart",
    kfs: [
      kf(0, 6080, 5350, 245, "line", 1680, "documented",
        "Continuity: Steuart's brigade on the lower slope at sunset."),
      kf(T(20, 0), 5940, 5245, 250, "line", 1620, "documented",
        "THE VACATED-WORKS LODGMENT (~20:00, vs Greene's clock frame): the perpendicular breastwork + traverse ground between the 137th NY monument (5814,5305) and the Williams-division works — the works the XII Corps left at 18:30 (CA-J2E-1). The lodgment HOLDS overnight — the ED-57-class occupation record (the ED-63 contrast case: vacated works, not served guns)."),
      kf(EndT, 5940, 5245, 250, "line", 1580, "documented",
        "The night line 'between the captured breastwork and a stone wall on the left of and parallel to it' (report; Geary fixes the same wall from the other side). The 10th/23rd VA works-takers lost light — the episode structure in the return's regiment rows."),
    ],
  },
  {
    id: "csa-walker",
    kfs: [
      kf(0, 6900, 6050, 200, "line", 1440, "documented",
        "Continuity: the Stonewall Brigade turning in from the Hanover-road flank (or-27-2-walker-stonewall; ED-62's three-brigade composition fact)."),
      kf(EndT, 6500, 5950, 230, "line", 1440, "documented",
        "Rejoins the division's left overnight (his July 3 morning fight is the next phase's record)."),
    ],
  },
  {
    id: "us-greene",
    kfs: [
      kf(0, 5812, 5510, 60, "line", 1310, "documented",
        "Continuity: the brigade single-line in the works, attacked since 'a few minutes before 7 p. m.' (or-27-1-greene p. 856)."),
      kf(T(20, 30), 5812, 5510, 60, "line", 1220, "documented",
        "The four charges received; the 137th NY refused perpendicular to the works against Steuart's lodgment (Ireland's record; the regiment's 137 = nearly half the brigade's cost); the reinforcement ledger in action — I Corps 355 + XI Corps ~400 + part of Candy's 1,000: 'Not more than 1,300 men were in the lines at any one time' (or-27-1-greene p. 858)."),
      kf(EndT, 5812, 5510, 60, "line", 1170, "documented",
        "Firing dies down ~22:00 (Greene's Kane-return 10 o'clock; Geary 'about 10 p. m.'; CA-J2E-5). Decay: report 307 vs return 303 — the 4-man component conflict CARRIED (dossier)."),
    ],
  },
  // ------------------- EAST CEMETERY HILL (CA-J2E-3/4) --------------------
  {
    id: "csa-hays",
    kfs: [
      kf(0, 5318, 6523, 200, "line", 1137, "documented",
        "Continuity: the jump-off field (tab-csa-2c-ear-1). Early's trigger primary welds the assault to Johnson's: 'as soon as Johnson became warmly engaged, which was a little before dusk, I ordered Hays and Avery to advance' (or-27-2-early; ED-62)."),
      kf(T(19, 46), 5318, 6523, 200, "line", 1137, "documented",
        "Step-off 19:45-20:00 (CA-J2E-3, CONFIRMED): the three-source both-armies agreement — Hays 'a little before 8' / Wiedrich 'about 8' / Ricketts 'at about 8'; the twilight-physics bracket (sunset 19:29, civil-twilight end ~19:59) against 'the darkness of the evening, now verging into night'."),
      kf(T(20, 0), 5150, 6100, 205, "line", 1060, "documented",
        "The ascent ladder: over the first hill, the bottom, the stone-wall second line, the abatis/rifle-pit third line (or-27-2-hays — the ladder verbatim)."),
      kf(T(20, 10), 5060, 5880, 210, "line", 1010, "documented",
        "THE LODGMENT among the batteries: 'several pieces of artillery ... every piece ... silenced' — the drawn red bars at (5021,5869), 21 m from the Wiedrich monument (bachelder j2-05; ED-63: possession momentary, NO gun left the hill, the brought-off trophies are four COLORS)."),
      kf(T(20, 35), 5140, 6080, 30, "line", 970, "documented",
        "Driven out by the infantry counterattack (Carroll + the rallied XI Corps regiments, CA-J2E-4 ~20:30 — the sequence anchor's inferred-clock flag carried); the withdrawal chain: the stone wall, 'a fence some 75 yards distant', around the hill by the right flank (or-27-2-hays)."),
      kf(T(22, 0), 5318, 6523, 20, "line", 956, "documented",
        "'About 10 o'clock' back at the original position (or-27-2-hays — CA-J2E-5's clock). Decay: his per-day table July 2 = 21k/119w/41m = 181 (p. 481 — the evening phase's only per-day brigade split, PRIMARY; the return's 313 scope conflict flagged, not averaged)."),
      kf(EndT, 5318, 6523, 20, "line", 956, "documented", "The night line (to the town street before daybreak — the July 3 record)."),
    ],
  },
  {
    id: "csa-godwin",
    kfs: [
      kf(0, 5334, 6497, 200, "line", 850, "documented",
        "Continuity: Hoke's brigade (Avery) on Hays's left at the jump-off pair (tab-csa-2c-ear-3, 30 m from the Hays tablet)."),
      kf(T(19, 46), 5334, 6497, 200, "line", 850, "documented",
        "Steps with Hays (Early's trigger; the Hoke tablet's '8 P.M.' concurring — CA-J2E-3)."),
      kf(T(20, 0), 5200, 6060, 210, "line", 780, "documented",
        "The outside (left/east) lane of the two-brigade front, over the same ridge-bottom-wall-abatis ladder; AVERY MW on the ascent — 'I died with my face to the enemy' written left-handed to Maj. Tate (or-27-2-early + the NC State Archives note; the 'written in the dark' framing corrected against the archival page — paralysis/left hand, not lighting)."),
      kf(T(20, 12), 5100, 5850, 215, "line", 730, "documented",
        "Colors planted on the lunettes (the R1-verified Hoke tablet; the ED-63 convergent lodgment among Ricketts's guns — the captured-and-spiked left piece is damage, not removal)."),
      kf(T(20, 40), 5230, 6150, 30, "line", 650, "documented",
        "Repulsed by the counterattack (CA-J2E-4); the darkness claimed as cover by BOTH sides on this ground (pass-8 twilight class)."),
      kf(T(21, 30), 5334, 6497, 20, "line", 605, "documented",
        "Re-formed at the jump-off field under Godwin (no brigade report exists — Avery dead, Godwin silent; verified negative). Decay: return 345 with the 57th NC print-conflict row adopted at quadruple-arithmetic grade 62 (ED-71 exhibit)."),
      kf(EndT, 5334, 6497, 20, "line", 605, "documented", "The night line."),
    ],
  },
  {
    id: "csa-gordon",
    kfs: [
      kf(0, 5230, 6470, 200, "line", 1200, "documented",
        "Continuity: Gordon's brigade in Early's support echelon at Winebrenner's Run."),
      kf(T(20, 10), 5260, 6420, 200, "line", 1200, "documented",
        "Advanced to the position from which Hays and Avery had moved, and HALTED there — Early stopped the support when Rodes's advance on the right did not develop (or-27-2-early/-gordon frame; documented halt, rendered halted)."),
      kf(EndT, 5260, 6420, 200, "line", 1200, "documented", "The night line at the run."),
    ],
  },
  {
    id: "us-carroll",
    kfs: [
      kf(0, 4560, 5080, 260, "line", 640, "documented",
        "Continuity: Carroll's brigade near Ziegler's Grove (ED-51's day ground)."),
      kf(T(20, 28), 4560, 5080, 260, "column", 640, "documented",
        "Sent at the sound of the hill's crisis (Hancock's dispatch of the brigade — the II Corps night reinforcement)."),
      kf(T(20, 40), 5115, 5680, 30, "line", 630, "documented",
        "THE COUNTERATTACK CLEARS THE HILL (CA-J2E-4, ~20:30 — a SEQUENCE anchor, no hard clock on either side; the inferred-anchor flag stays, ED-62): Ricketts's 'a part of the Second Army Corps charged in' is the receiving-side pin, 34 m from the Carroll HQ marker (5024,5793)."),
      kf(EndT, 5115, 5680, 30, "line", 625, "documented",
        "Holds East Cemetery Hill 'until the 5th' (or-27-1-carroll; ED-51 — the brigade's July 3 is HERE, not the center)."),
    ],
  },
  {
    id: "us-harris",
    kfs: [
      kf(0, 5170, 5790, 40, "line", 565, "documented",
        "Continuity: Ames's brigade (Harris) behind the ECH stone walls at a fraction of its rolls (the 75th OH's 91-man July 2 basis)."),
      kf(T(20, 10), 5170, 5790, 40, "line", 520, "documented",
        "The wall fight in 'darkness and smoke' (or-27-1-harris); the line pierced at the von Gilsa seam; the batteries' ground fought hand-to-hand."),
      kf(EndT, 5170, 5790, 40, "line", 505, "documented",
        "Re-established with the counterattack; the July 2 evening share REAL but BOUNDED by the shrunken line (no per-day primary — split flagged as reconstruction, dossier us-xi-1-2-harris.md)."),
    ],
  },
  {
    id: "us-vongilsa",
    kfs: [
      kf(0, 5130, 5860, 10, "line", 605, "documented",
        "Continuity: von Gilsa's brigade at the hill's north foot (the 41st NY's 218 all-ranks the neighbor datapoint; no brigade report — verified negative)."),
      kf(T(20, 10), 5130, 5860, 10, "line", 560, "documented",
        "The first line struck by the assault ('attack ... at 6.30 p. m.' — von Einsiedel's +75 profiled early clock CARRIED, never moving the anchor; ED-58's evening extension)."),
      kf(EndT, 5130, 5860, 10, "line", 545, "documented",
        "The line re-established after the repulse; 'the few escaped in it' — the darkness claimed as cover on the Union side too (pass-8 twilight class)."),
    ],
  },
  {
    id: "us-btty-wiedrich",
    kfs: [
      kf(0, 5115, 5795, 20, "line", 141, "documented",
        "Continuity: Wiedrich's I/1st NY on the north front."),
      kf(T(20, 10), 5115, 5795, 20, "line", 132, "documented",
        "The fight 'got into the intrenchments of my battery' (or-27-1-wiedrich — hand-to-hand among the guns; ED-63: one convergent event, honest views of the same minutes)."),
      kf(T(20, 40), 5115, 5795, 20, "line", 128, "documented",
        "Possession ends with the infantry counterattack; the battery RESUMES SERVICE the same night — never a captured-battery state (ED-63's rendering rule)."),
      kf(EndT, 5115, 5795, 20, "line", 128, "documented",
        "In battery at the walls. Decay: the 14-vs-13 one-man conflict carried (dossier)."),
    ],
  },
  {
    id: "us-btty-ricketts",
    kfs: [
      kf(0, 5140, 5745, 55, "line", 144, "documented",
        "Continuity: Ricketts's F&G PA in position on the east front ('at about 8' his own clock for the assault — CA-J2E-3's third source)."),
      kf(T(20, 10), 5140, 5745, 55, "line", 130, "documented",
        "The left piece captured and SPIKED IN PLACE — damage, not removal (or-27-1-ricketts; ED-63: no captured-ordnance row exists on either side's returns)."),
      kf(T(20, 40), 5140, 5745, 55, "line", 124, "documented",
        "'A part of the Second Army Corps charged in' (or-27-1-ricketts — CA-J2E-4's receiving pin); service resumed."),
      kf(EndT, 5140, 5745, 55, "line", 121, "documented", "In battery at the lunettes."),
    ],
  },
  // ------------------- THE XII CORPS RETURNS (E-1's other end) ------------
  {
    id: "us-colgrove",
    kfs: [
      kf(0, 5700, 4400, 210, "column", 1170, "documented",
        "Continuity: Ruger's column on the pike."),
      kf(T(21, 0), 6080, 4800, 30, "line", 1170, "documented",
        "The return: the drawn '9. P.M.' annotation beside 'RUGER'S' (5931,4723) — a sheet-internal clock (bachelder j2-05); back at McAllister's Woods/Spangler's Spring south."),
      kf(EndT, 6080, 4800, 30, "line", 1170, "documented",
        "The night line (the Spangler's Meadow morning is July 3's record)."),
    ],
  },
  {
    id: "us-mcdougall",
    kfs: [
      kf(0, 5500, 4750, 190, "column", 1755, "documented", "Continuity: on the pike with Ruger."),
      kf(T(21, 15), 5860, 5090, 60, "column", 1755, "documented",
        "Returned toward the lower works and HALTED — the works found occupied (A. S. Williams's near-midnight report of the wrong state of things; the Steuart lodgment)."),
      kf(EndT, 5860, 5090, 60, "line", 1755, "documented",
        "In line short of the vacated works overnight (the July 3 dawn recovery is the next phase's record)."),
    ],
  },
  {
    id: "us-lockwood",
    kfs: [
      kf(0, 5300, 4700, 210, "column", 1650, "documented", "Continuity: conducted toward the left by A. S. Williams."),
      kf(T(20, 5), 3900, 3690, 250, "line", 1645, "documented",
        "'Recapturing three pieces of artillery abandoned by the enemy' at 'quite dark' (A. S. Williams — the ED-61 claimant ledger's command-grain entry; the Trostle ground, McGilvery in-person context)."),
      kf(T(21, 30), 5137, 5396, 40, "column", 1645, "documented",
        "Withdrawn east — 'LOCKWOOD's Brigade During the night' drawn at (5137,5396) (bachelder j2-05)."),
      kf(EndT, 5137, 5396, 40, "column", 1645, "documented", "The night ground."),
    ],
  },
  {
    id: "us-kane",
    kfs: [
      kf(0, 5500, 4800, 170, "column", 600, "documented", "Continuity: Geary's wrong-road column on the pike."),
      kf(T(21, 30), 5750, 5150, 60, "column", 600, "documented",
        "Kane's brigade returns first up the pike (Geary's record of the counter-march)."),
      kf(T(22, 0), 5845, 5400, 60, "line", 595, "documented",
        "THE TWO-VOLLEY DISCOVERY (~22:00 class): the returning brigade fired into from the works it had left — the occupied-works discovery at narrative grain (Kane via Geary; pass 8); takes position on Greene's right."),
      kf(EndT, 5845, 5400, 60, "line", 595, "documented", "The night line right of Greene."),
    ],
  },
  {
    id: "us-candy",
    kfs: [
      kf(0, 5450, 4650, 170, "column", 1400, "documented", "Continuity: Geary's wrong-road column."),
      kf(T(22, 15), 5620, 4950, 50, "column", 1400, "documented",
        "Counter-marching up the pike behind Kane (Geary's wrong-road narrative — his own pp. 828+ unfetched, carried at command grain; the full return completes before dawn, the July 3 phase's t=0)."),
      kf(EndT, 5620, 4950, 50, "column", 1400, "documented", "En route at the window's end."),
    ],
  },
  // ------------------- THE SOUTHERN FIELD'S NIGHT -------------------------
  {
    id: "csa-wofford",
    kfs: [
      kf(0, 3900, 3060, 260, "line", 995, "documented",
        "Continuity: falling back on the sunset order (CA-J2A-11)."),
      kf(T(20, 0), 3520, 3160, 260, "line", 995, "documented",
        "'Fell back at sunset to the grove west of the Wheatfield' (tablet verbatim — the withdrawal's own destination)."),
      kf(EndT, 3520, 3160, 260, "line", 995, "documented", "The night grove."),
    ],
  },
  {
    id: "csa-kershaw",
    kfs: [
      kf(0, 3380, 3350, 80, "line", 1170, "documented", "Continuity: the stony hill/Rose ground at sunset."),
      kf(T(20, 30), 3350, 3620, 80, "line", 1170, "documented",
        "Ordered back to the Peach Orchard after dark (or-27-2-kershaw; tablet concurs)."),
      kf(EndT, 3350, 3620, 80, "line", 1170, "documented", "The Peach Orchard night line (held to noon July 3)."),
    ],
  },
  {
    id: "csa-humphreys",
    kfs: [
      kf(0, 3820, 3960, 250, "line", 851, "documented", "Continuity: Barksdale's survivors falling back from Plum Run."),
      kf(T(20, 15), 3450, 3830, 250, "line", 851, "documented",
        "Re-formed toward the Peach Orchard/Emmitsburg road ground under Humphreys (the brigade's night frame; Barksdale mw left on the field, dying at the Hummelbaugh farm July 3)."),
      kf(EndT, 3450, 3830, 250, "line", 851, "documented", "The night line."),
    ],
  },
  {
    id: "csa-wilcox",
    kfs: [
      kf(0, 3950, 4420, 260, "line", 1404, "documented", "Continuity: withdrawing from the ravine stand."),
      kf(T(20, 15), 3200, 4460, 80, "line", 1404, "documented",
        "Back on the Seminary Ridge line (or-27-2-wilcox — the ~30-minute stand's withdrawal completed at dark)."),
      kf(EndT, 3200, 4460, 80, "line", 1404, "documented", "The ridge night line (his July 3 echelon post is the next day's)."),
    ],
  },
  {
    id: "csa-lang",
    kfs: [
      kf(0, 3700, 4570, 260, "line", 400, "documented", "Continuity: withdrawing."),
      kf(T(20, 10), 3250, 4640, 80, "line", 400, "documented",
        "Back on the ridge (or-27-2-lang; the 400 July 3 basis = the return exactly)."),
      kf(EndT, 3250, 4640, 80, "line", 400, "documented", "The ridge night line."),
    ],
  },
  {
    id: "csa-wright",
    kfs: [
      kf(0, 3700, 4850, 265, "line", 800, "documented", "Continuity: falling back from the gun line (ED-60)."),
      kf(T(20, 15), 3100, 4880, 85, "line", 582, "documented",
        "Rallied on Seminary Ridge; the brigade's 668 (return; report 688 and tablet 873 carried) consumed by the night count — the July 2 fight was the brigade's battle (ED-60's EC6 basis)."),
      kf(EndT, 3100, 4880, 85, "line", 582, "documented", "The ridge night line."),
    ],
  },
  {
    id: "csa-bn-alexander",
    kfs: [
      kf(0, 2694, 3567, 80, "line", 576, "documented", "Continuity: the Warfield ridge line."),
      kf(T(20, 0), 3497, 3751, 70, "line", 576, "documented",
        "Advanced to the captured Peach Orchard ground at dark ('ALEXANDER'S BATT.' drawn advanced at (3497,3751), bachelder j2-04) — the July 3 cannonade's core position taken."),
      kf(EndT, 3497, 3751, 70, "line", 576, "documented", "In battery on the orchard ground overnight."),
    ],
  },
  {
    id: "us-fisher",
    kfs: [
      kf(0, 4280, 2320, 210, "line", 1555, "documented", "Continuity: the spur's south shoulder."),
      kf(T(21, 0), 4150, 1990, 200, "line", 1555, "documented",
        "The Great Round Top night ascent (~21:00): Chamberlain's ordered advance with Fisher's regiments behind — the 20th ME drawn ONTO Great Round Top (bachelder j2-05; or-27-1-chamberlain; 'Big Round Top breastworks night July 2', register/tablet)."),
      kf(EndT, 4150, 1990, 200, "line", 1555, "documented", "The summit breastworks overnight."),
    ],
  },
  {
    id: "us-brewster",
    kfs: [
      kf(0, 4300, 4300, 255, "line", 1059, "documented", "Continuity: rallied on the ridge."),
      kf(T(20, 5), 3980, 4250, 255, "line", 1059, "documented",
        "The after-sunset advance: 'recaptured guns that had been left on the field' (or-27-1-brewster — the ED-61 claimant ledger's infantry entry; ground-retaken, then haul-off by the artillery details, per the sequence the primaries carry)."),
      kf(T(20, 45), 4300, 4300, 255, "line", 1059, "documented", "Back on the ridge line."),
      kf(EndT, 4300, 4300, 255, "line", 1059, "documented", "The night line."),
    ],
  },
  {
    id: "us-btty-watson",
    kfs: [
      kf(0, 4208, 3744, 250, "scattered", 55, "documented",
        "Continuity: the four guns abandoned on the knoll (ED-61's object count)."),
      kf(T(20, 0), 4208, 3744, 250, "line", 55, "documented",
        "The ground retaken — Peeples + the Garibaldi Guards' (39th NY) recapture charge, the one named-actor recapture primary (or-27-1-martin-v p. 660)."),
      kf(T(21, 0), 5050, 4350, 60, "column", 55, "documented",
        "The haul-off: 'a detail from the 6th Maine + Seeley's battery hauled off Battery I's guns' — 'The guns of the two batteries, numbering eight, were brought safely to the rear' by ~21:00 (or-27-1-mcgilvery ~20:00 disposition record; ED-61's recovery spine)."),
      kf(EndT, 5050, 4350, 60, "column", 55, "documented", "In the Reserve park; ALL EIGHT guns off the field."),
    ],
  },
  {
    id: "us-btty-bigelow",
    kfs: [
      kf(0, 4100, 3800, 240, "column", 76, "documented", "Continuity: the remnant behind the Plum Run line."),
      kf(T(21, 0), 5020, 4320, 60, "column", 76, "documented",
        "Dow procured the infantry detail that brought off the four 9th MA guns the same night (or-27-1-mcgilvery; Milton: 'Our four guns were recovered the same day' — ED-61's recovery spine; the Crawford July 3 Napoleon attribution carried as the ledger's named residue)."),
      kf(EndT, 5020, 4320, 60, "column", 76, "documented", "In the Reserve park."),
    ],
  },
  {
    id: "us-stannard",
    kfs: [
      kf(0, 4650, 4000, 300, "column", 1788, "documented", "Continuity: closing on the field."),
      kf(T(21, 0), 4400, 4300, 280, "column", 1788, "documented",
        "Joins the I Corps mass behind the ridge in the night (the brigade's evening arrival; its July 3 flank attack is the shipped record)."),
      kf(EndT, 4400, 4300, 280, "column", 1788, "documented", "The night mass."),
    ],
  },
];

// ---------------------------------------------------------------------------
// Statics: every other afternoon unit holds its cited 19:29 end state.
// A few carry better-than-continuity citations (below); the rest carry the
// continuity note (the afternoon file holds the position evidence).
// ---------------------------------------------------------------------------
const nightCitations: Record<string, string> = {
  "csa-benning": "Holds Devil's Den overnight — the HOLDER through July 3 (ED-56; j2-04/j2-05 drawn continuity).",
  "csa-sheffield": "The Round Tops' west-foot night line (mon-csa-law-adv; j2-05 'LAW'S ALA'; Scruggs's rock breastworks).",
  "csa-robertson": "Night between the Den and the Round Tops' foot (the division's night line frame).",
  "csa-luffman": "Rose farm night ground (tablet; July 3 continuity).",
  "csa-bryan": "The Rose/Wheatfield-south woodland overnight (July 3 tablet frame).",
  "us-rice": "Holds the spur overnight (the 20th ME's Great Round Top line rides Fisher's night track — Chamberlain's ~200-man advance; or-27-1-rice-vincent frame).",
  "us-garrard": "The summit line overnight; Weed dies ~21:00 ('dead as Julius Caesar' — Farley via Norton).",
  "us-mccandless": "Holds the recovered stone-wall line at the Wheatfield's east edge overnight (or-27-1-crawford; j2-05 drawn).",
  "us-nevin": "The Plum Run line overnight (or-27-1-nevin; the two recovered Napoleons in the ED-61 ledger).",
  "us-btty-dow": "The Plum Run line overnight; Dow's procured detail executes the 9th MA haul-off (ED-61).",
};

const moverIds = new Set(movers.map((m) => m.id));
const units: Unit[] = [];

for (const m of movers) {
  const base = aById.get(m.id);
  if (!base) throw new Error(`dayexp2-evening: mover '${m.id}' not in afternoon file`);
  units.push(moverUnit({
    id: m.id, name: base.name, side: base.side,
    frontage_m: base.frontage_m, depth_m: base.depth_m,
    ...(base.parent !== undefined && { parent: base.parent }),
    keyframes: m.kfs,
  }));
  const u = units[units.length - 1]!;
  if (base.regiments) u.regiments = base.regiments;
}

const CONT_NOTE =
  "Continuity static (day-expansion slice 2): holds the July 2 afternoon phase file's cited 19:29 end state through the evening window — the position evidence rides that file's keyframes; no independent evening anchor consumed.";

for (const u of afternoon.units) {
  if (moverIds.has(u.id)) continue;
  const p = endPose(u.id);
  const named = nightCitations[u.id];
  const row = staticUnit({
    id: u.id, name: u.name, side: u.side,
    strength: p.strength, x: p.x, z: p.z, facing: p.facing,
    formation: p.formation,
    frontage_m: u.frontage_m, depth_m: u.depth_m,
    ...(u.parent !== undefined && { parent: u.parent }),
    grade: named ? "B" : "C",
    citation: named ? named : CONT_NOTE,
    endT: EndT,
  });
  if (u.regiments) row.regiments = u.regiments;
  units.push(row);
}

// ---------------------------------------------------------------------------
// Events.
// ---------------------------------------------------------------------------
const events: EngagementEvent[] = [
  fireEvent({
    id: "j2e-culp-jones", kind: "musketry", unitId: "csa-dungan",
    t0: 0, t1: T(21, 0), confidence: "documented",
    citation: "Jones's assaults on Greene's right, pressed in the dark and repulsed (or-27-2-jones-jm).",
  }),
  fireEvent({
    id: "j2e-culp-nicholls", kind: "musketry", unitId: "csa-williams",
    t0: 0, t1: T(22, 0), confidence: "documented",
    citation: "The fire line 100 yards from the works, 'kept up ... until late in the night' (or-27-2-williams-nicholls).",
  }),
  fireEvent({
    id: "j2e-culp-steuart", kind: "musketry", unitId: "csa-steuart",
    t0: T(19, 40), t1: T(21, 30), confidence: "documented",
    citation: "The lower-works fight and the lodgment (report; the 137th NY's refused front is the receiving record).",
  }),
  fireEvent({
    id: "j2e-culp-greene", kind: "musketry", unitId: "us-greene",
    t0: 0, t1: T(22, 0), confidence: "documented",
    citation: "The four charges received; firing dies down ~22:00 (Greene's Kane-return 10 o'clock; Geary 'about 10 p. m.' — CA-J2E-5).",
    note: "Nicholls's four-hours-from-19:00 reading runs to the envelope's 23:00 edge and is recorded, not adopted (ED-62).",
  }),
  fireEvent({
    id: "j2e-ech-hays", kind: "musketry", unitId: "csa-hays",
    t0: T(19, 46), t1: T(20, 40), confidence: "documented",
    citation: "The assault ladder and the lodgment fight (or-27-2-hays; CA-J2E-3).",
  }),
  fireEvent({
    id: "j2e-ech-avery", kind: "musketry", unitId: "csa-godwin",
    t0: T(19, 46), t1: T(20, 40), confidence: "documented",
    citation: "The left lane's assault (the Hoke tablet's 8 P.M.; Avery mw).",
  }),
  fireEvent({
    id: "j2e-ech-harris", kind: "musketry", unitId: "us-harris",
    t0: T(19, 48), t1: T(20, 45), confidence: "documented",
    citation: "The wall fight in 'darkness and smoke' (or-27-1-harris).",
  }),
  fireEvent({
    id: "j2e-ech-vongilsa", kind: "musketry", unitId: "us-vongilsa",
    t0: T(19, 48), t1: T(20, 45), confidence: "documented",
    citation: "The first line's fight at the foot (or-27-1-einsiedel-41ny).",
  }),
  fireEvent({
    id: "j2e-ech-wiedrich", kind: "artillery_fire", unitId: "us-btty-wiedrich",
    t0: T(19, 35), t1: T(20, 8), confidence: "documented",
    citation: "Canister into the assault until the intrenchments fight (or-27-1-wiedrich 'about 8').",
  }),
  fireEvent({
    id: "j2e-ech-ricketts", kind: "artillery_fire", unitId: "us-btty-ricketts",
    t0: T(19, 35), t1: T(20, 8), confidence: "documented",
    citation: "Canister 'at about 8' until the lodgment (or-27-1-ricketts).",
  }),
  fireEvent({
    id: "j2e-ech-stevens", kind: "artillery_fire", unitId: "us-btty-stevens",
    t0: T(19, 40), t1: T(20, 30), confidence: "documented",
    citation: "The 5th ME's enfilade from the knoll across the assault's flank (the battery's ground record; mon-5me-stevens class).",
  }),
  fireEvent({
    id: "j2e-ech-carroll", kind: "musketry", unitId: "us-carroll",
    t0: T(20, 30), t1: T(20, 50), confidence: "documented",
    citation: "The counterattack clears the batteries (or-27-1-carroll; Ricketts's receiving-side pin — CA-J2E-4).",
  }),
  fireEvent({
    id: "j2e-ech-wiedrich-resume", kind: "artillery_fire", unitId: "us-btty-wiedrich",
    t0: T(20, 45), t1: T(21, 15), confidence: "documented",
    citation: "Service resumed the same night (ED-63 — never a captured-battery state).",
  }),
  fireEvent({
    id: "j2e-kane-two-volley", kind: "musketry", unitId: "us-kane",
    t0: T(22, 0), t1: T(22, 10), confidence: "documented",
    citation: "The two-volley discovery: the returning brigade fired into from its own vacated works (Kane via Geary — the occupied-works discovery).",
  }),
];

const battle: Battle = {
  name: "Gettysburg — July 2, 1863: Evening (Culp's Hill and East Cemetery Hill)",
  startTime: StartTime,
  endTime: EndT,
  units,
  events,
  environment: {
    windTowardDeg: 45, windMps: 0.0, confidence: "unknown",
    note: "No sourced wind observation for the July 2 evening exists in the corpus (the ED-10/ED-19 class); calm authored.",
  },
};

writeFileSync(outPath, exportValidated(battle));
console.log(`wrote ${outPath}: ${units.length} units, ${events.length} events, ` +
  `window ${StartTime}..${StartTime + EndT} (19:29-22:30 LMT)`);
