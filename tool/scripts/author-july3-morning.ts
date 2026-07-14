// THE JULY 3 MORNING PHASE (Culp's Hill, Johnson's renewed assaults;
// ~04:30-13:00 LMT). Run from tool/:
//   npx vite-node scripts/author-july3-morning.ts
// Committed as the derivation record (the author-w*/A1/A2/dayexp*/decomp1
// pattern). Slice report: docs/reconstruction/audit/july3-morning-slice.md.
//
// THE PHASE (ADR 0005: one battle file = one phase = one clock):
//   startTime 16200 (04:30 LMT — CA-J3M-1/5, "daylight"/civil-twilight) ·
//   endTime 30600 (ends 13:00 LMT — abuts the existing july3-afternoon
//   phase's startTime 46800 UNCHANGED). The manifest's july3-morning phase
//   ECHOES this clock (test-enforced).
//
// THE RESEARCH IS BANKED: dossier pass 9 hardened this chain end to end
// (CA-J3M-1 on Muhlenberg's author primary; CA-J3M-2's three waves clocked
// by their executors; CA-J3M-4's quadruple-attested 10:30-11:00 close),
// pass 8 hardened the July 2 evening chain this phase continues from.
//
// THE NEW OWNER RULING (ED-78, docs/reconstruction/angle-editorial-decisions.md):
// "make a best guess inference for right now" — provisional inference is now
// permitted where the primary record conflicts, PROVIDED the inference is
// marked provisional, both poles stay on the record, and the ruling is
// cited. This unblocks CA-J3M-3 (the ED-30/ED-64 Pfanz-gate precondition):
// the Spangler's Meadow charge is authored at the ~10:00 direction AS
// PROVISIONAL, carrying the Morse 5.30 / 27th IN 6 a.m. marker EARLY POLE
// verbatim alongside on every keyframe/event that uses the ruling.
//
// CROSS-PHASE CONTINUITY: every carried unit's t=0 keyframe here is the
// july2-evening file's own cited t=10860 (22:30 LMT) end state, read
// directly from that file (never re-derived) — the overnight is a
// non-combat gap (22:30 -> 04:30 next day), so position/strength/facing at
// t=0 are UNCHANGED from july2-evening's last keyframe (continuity
// verified below, build-failing assertion). Reinforcements (Daniel,
// O'Neal, Smith) were "continuity static" placeholders in july2-evening
// (no evening combat) — their t=0 here is likewise that file's cited value,
// which for Daniel/O'Neal already nets July 1 losses only (per
// strength-reconciliation-1's supersede) — this phase applies the July 3
// share on top, arithmetic shown per-unit in the keyframe citations.
//
// FILM-SAFETY: gettysburg-july3.json (the afternoon file) and
// app/Assets/Battle/Angle/angle.bundle.json are NEVER READ OR WRITTEN by
// this script. Where this phase's evidence-based end-of-fight strength
// diverges from the afternoon file's frozen t=0 value (both derive from
// the same tablets, but the afternoon file's numbers predate this phase's
// per-unit dossier arithmetic), the divergence is a NAMED residual in the
// slice report — never a silent edit of that file.
//
// HARD RULES observed: no invented motion (every track cites its dossier
// chain); conflicts carried, never averaged (O'Neal's thin EC6 headroom,
// Colgrove's order-corruption pair, the Fesler/Morse-vs-Ruger two-pole
// Spangler's Meadow dispute); no report-nominal clock moves an anchor
// (ED-25 rule 4) except where ED-78 explicitly authorizes a provisional
// reading, marked as such.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe } from "../src/model";
import { fireEvent, moverUnit, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const eveningPath = join(here, "../../app/Assets/Battle/gettysburg-july2-evening.json");
const outPath = join(here, "../../app/Assets/Battle/gettysburg-july3-morning.json");

const evening: Battle = JSON.parse(readFileSync(eveningPath, "utf-8"));
function eveningEnd(id: string): Keyframe {
  const u = evening.units.find((x) => x.id === id);
  if (!u) throw new Error(`continuity: '${id}' not found in gettysburg-july2-evening.json`);
  const kf = u.keyframes[u.keyframes.length - 1]!;
  if (kf.t !== evening.endTime)
    throw new Error(`continuity: '${id}' last keyframe t=${kf.t} != evening endTime ${evening.endTime}`);
  return kf;
}

// Clock: t = seconds from 04:30 LMT (startTime 16200). 13:00 = 30600.
const StartTime = 16200;
const EndT = 30600; // 13:00 LMT — abuts july3-afternoon's startTime 46800
const T = (h: number, m: number) => h * 3600 + m * 60 - StartTime;

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  confidence: Keyframe["confidence"], citation?: string): Keyframe => ({
    t, x, z, facing, formation, strength,
    ...(confidence !== undefined && { confidence }),
    ...(citation !== undefined && { citation }),
  });

// Continuity opener: t=0 keyframe = the july2-evening file's own end state,
// UNCHANGED (the overnight 22:30->04:30 gap is a non-combat hold).
function opener(id: string, note: string): Keyframe {
  const e = eveningEnd(id);
  return kf(0, e.x, e.z, e.facing, e.formation, e.strength, "documented",
    `Continuity: holds gettysburg-july2-evening.json's cited t=${e.t} (22:30 LMT) end state through the overnight non-combat gap (verified by reading that file at build time, never re-derived). ${note}`);
}

interface M {
  id: string; name: string; side: "union" | "confederate";
  frontage?: number; depth?: number; kfs: Keyframe[];
}

const movers: M[] = [
  // ========================= JOHNSON'S DIVISION (CA-J3M-2) ==================
  {
    id: "csa-steuart", name: "Steuart's Brigade, Johnson's Division",
    side: "confederate",
    kfs: [
      opener("csa-steuart",
        "In the captured works (the July 2 evening lodgment); 1st MD Bn/37th/10th VA in the line, the two NC regiments pinned outside — the brigade fought the night 'half in, half out' of the line (csa-2c-joh-1-steuart.md)."),
      kf(T(4, 45), 5940, 5245, 250, "line", 1580, "documented",
        "CA-J3M-2 wave 1 (~04:30-05:30): under Muhlenberg's opening program from the first minutes and the dawn infantry escalation (Cobham '3 o'clock... by 4 o'clock the firing had become general', receiving this brigade's front); Johnson's division primary opens the morning HERE — 'Early next morning, the Stonewall Brigade was ordered to the support of the others, and the assault was renewed with great determination' (or-27-2-johnson) names Steuart's front as the renewal's anchor."),
      kf(T(10, 0), 5895, 5340, 60, "line", 1200, "inferred",
        "CA-J3M-2 wave 3, THE CO-CHARGE (~10:00, division-frame clock; Kane's receiving-side 10.30 is the anchor's hard edge): 'line at right angles to the works' — 3rd NC/1st MD/37th/23rd/1st NC in order — across the open ground toward the second breastworks (or-27-2-steuart). Daniel, alongside: 'orders from General Johnson to charge the enemy's works, in conjunction with General Steuart... Owing to the heavy fire brought upon General Steuart, he was unable to advance farther' (or-27-2-daniel p. 568). Kane received it: 'about 10.30 o'clock, when the enemy made their last determined effort by charging in column of regiments' (or-27-1-kane). The charge lane is the ground the modern battlefield calls PARDEE FIELD (named for Col. Ario Pardee Jr., 147th PA — us-xii-2-1-candy.md dossier note; no OR primary uses the toponym, carried as a position-class identification over the report's own 'open ground toward the second breastworks')."),
      kf(T(10, 20), 5945, 5255, 250, "scattered", 1050, "documented",
        "Repulsed 'in good order' back to the wall (or-27-2-steuart: 'fell back in good order'); the 3rd NC ammunition crisis (cartridges taken 'from the wounded and dead') dates this stand."),
      kf(T(11, 0), 6120, 5220, 250, "line", 998, "documented",
        "'Raged fiercely until 11 A.M. when this Brigade and the entire line fell back to the base of the hill' (brigade tablet, ED-24-class verbatim, already the afternoon file's own t=0 citation basis) — east of Rock Creek, the division's CA-J3M-4 withdrawal. STRENGTH ARITHMETIC (flagged, [D]): return total 682 (Johnson's table EXACT, or-27-2-anv-return p. 341) minus the July 2 evening loss already authored in gettysburg-july2-evening.json (1680->1580 = 100, from that file's own decay) = 582 July-3 loss; 1580-582 = 998."),
      kf(EndT, 6250, 5200, 250, "line", 998, "documented",
        "Spent and quiet east of the creek at the phase's close, matching the base-of-hill ground both brigade tablets and the July 3 afternoon file's independently-cited DOCUMENTED SILENCE describe (gettysburg-july3.json csa-steuart t=0: x=6420,z=5150,strength=1020 — a residual noted in the slice report, both readings derive from the same tablets but were computed independently; not reconciled here per the film-safety rule)."),
    ],
  },
  {
    id: "csa-williams", name: "Nicholls's Brigade, Johnson's Division (Col. J. M. Williams)",
    side: "confederate",
    kfs: [
      opener("csa-williams", "In the fire line ~100 yards from the works, holding since the evening assault (csa-2c-joh-3-nicholls.md)."),
      kf(T(4, 45), 5975, 5752, 235, "line", 940, "documented",
        "'At early light opened on the enemy again... four hours almost without cessation, and at intervals until 12 m.' (or-27-2-williams-nicholls) — the CA-J3M-1 program's infantry half, center lane against Greene's works, matching the pass-8 evening fire's ~100-yard range."),
      kf(T(10, 30), 5990, 5760, 235, "line", 780, "documented",
        "Continued interval fire through the CA-J3M-2/4 climax on its left (Steuart/Daniel's charge); no clock of its own for the charge (kind-none profile) — held in place, fire only."),
      kf(T(12, 0), 6020, 5790, 235, "line", 682, "documented",
        "Noon withdrawal: 'the ravine or creek, about 300 yards from the line held during the night' (or-27-2-williams-nicholls). STRENGTH ARITHMETIC (flagged, [D], EC6 half-half convention — no per-day primary): return total 388 (Johnson's table EXACT) minus the evening loss already authored in the July 2 evening file (1070->940 = 130) = 258 July-3 loss; 940-258 = 682."),
      kf(EndT, 6020, 5790, 235, "line", 682, "documented",
        "Spent and quiet at the creek line through the phase's close (gettysburg-july3.json csa-williams t=0 independently cites x=6610,z=5780,strength=710 — a residual noted in the slice report, both readings derive from the same tablet, not reconciled here)."),
    ],
  },
  {
    id: "csa-walker", name: "Stonewall Brigade, Johnson's Division (Brig. Gen. James A. Walker)",
    side: "confederate",
    kfs: [
      opener("csa-walker",
        "Rejoined the division overnight after missing the evening assault (Brinkerhoff's Ridge detention, self- and division-attested: 'General Walker did not arrive in time to participate in the assault that night', or-27-2-johnson) — the brigade's July 2 loss was near-zero (2nd VA's 14, its lightest row); this phase's cost is 'almost entirely July 3' (csa-2c-joh-2-walker.md)."),
      kf(T(4, 45), 6500, 5950, 230, "line", 1440, "documented",
        "'Early next morning, the Stonewall Brigade was ordered to the support of the others, and the assault was renewed with great determination' (or-27-2-johnson — the division's own wave-1 composition pin: Walker's brigade opens the renewal). Drawn: j3-01/j3-02 red 'WALKER' labels hold the east-slope ground across both morning sheets, matching the five-hours-firing record."),
      kf(T(8, 0), 6450, 5900, 230, "line", 1300, "documented",
        "'My right, extending beyond the breastworks, suffered very heavily' (or-27-2-walker-stonewall) — five hours' incessant firing under Muhlenberg's program and the works' close musketry."),
      kf(T(10, 30), 6400, 5920, 230, "line", 1180, "documented",
        "The refit hour after the general repulse, then 'two more advances with equally bad success' — the division's CA-J3M-4 order to fall back with the entire line."),
      kf(T(12, 0), 6600, 6000, 230, "line", 1110, "documented",
        "'Desultory fire to dark' as the division holds east of the creek. STRENGTH ARITHMETIC ([D]): return total 330 (Johnson's table EXACT) minus the evening loss already authored (1440->1440 = 0, matching the brigade's self-attested evening absence) = 330 July-3 loss; 1440-330 = 1110."),
      kf(EndT, 6600, 6000, 230, "line", 1110, "documented",
        "Spent and quiet at the creek line (gettysburg-july3.json csa-walker t=0 independently cites x=6670,z=5990,strength=1120 — near-exact agreement, both position and strength within a few dozen meters/men; a residual note only, not reconciled here)."),
    ],
  },
  {
    id: "csa-dungan", name: "Jones's Brigade, Johnson's Division (Lt. Col. R. H. Dungan)",
    side: "confederate",
    kfs: [
      opener("csa-dungan",
        "In the lead-assault lodgment ground, Gen. Jones w the evening before, Lt. Col. Dungan (48th VA) now commanding (csa-2c-joh-4-jones.md; the register's July-2-evening wounding correction)."),
      kf(T(4, 45), 6120, 5890, 235, "line", 1400, "documented",
        "Daniel's brigade ordered ~04:00 'to the support of Jones' brigade, Colonel [R. H.] Dungan commanding' (or-27-2-daniel — the succession named by the supporting brigade's own primary); Daniel 'found its skirmishers engaging the enemy at long range' — the lane's early-morning activity. Drawn: j3-01 'JONES VA.' label (5721,5780) with 'DANIEL' arriving immediately behind it (5754,5842) — the arrival state exactly as Daniel's report describes it."),
      kf(T(8, 0), 6080, 5850, 235, "line", 1260, "documented",
        "Holding the lead sector through the division's renewed assaults (Johnson's division tablet: 'The Division being reinforced by four brigades two other assaults were made and repulsed' — this brigade the lead element of both)."),
      kf(T(10, 30), 6100, 5870, 235, "scattered", 1180, "documented",
        "Under the CA-J3M-2 climax alongside Steuart's/Daniel's charge on its right; the division's CA-J3M-4 general order to fall back with the entire line."),
      kf(T(12, 0), 6300, 5980, 235, "line", 1139, "documented",
        "Holds near the creek. STRENGTH ARITHMETIC ([D]): return total 421 (Johnson's table EXACT, with the return's own footnote regimental-counter-reading conflict carried, not resolved) minus the evening loss already authored (1560->1400 = 160) = 261 July-3 loss; 1400-261 = 1139."),
      kf(EndT, 6300, 5980, 235, "line", 1139, "documented",
        "Spent and quiet (gettysburg-july3.json csa-dungan t=0 independently cites x=6760,z=6090,strength=1180 — a residual noted in the slice report, not reconciled here)."),
    ],
  },
  // ================= THE CSA MORNING REINFORCEMENTS (CA-J3M-2) ==============
  {
    id: "csa-daniel", name: "Daniel's Brigade, Rodes's Division (attached to Johnson)",
    side: "confederate",
    kfs: [
      opener("csa-daniel",
        "'Continuity static' placeholder in gettysburg-july2-evening.json (no evening combat — Daniel's division was resting/marching, not on Culp's Hill July 2 evening); this phase's t=0 is the strength-reconciliation-1-superseded July-1-end figure (1650), CLEARED for fresh July-3 authoring (csa-2c-rod-1-daniel.md)."),
      kf(T(4, 45), 4200, 5980, 140, "column", 1650, "documented",
        "THE NIGHT MARCH [B: ~4 miles, ~0.72 m/s night-column pace]: off 'at about 1.30 a. m.', reported to Johnson 'at about 4 a. m.' (or-27-2-daniel) — 'to the left of the town, a distance of about 4 miles.' Immediately into Jones's support: 'my troops were much exposed, and many were killed and wounded' under Muhlenberg's program from the first minutes."),
      kf(T(6, 30), 5900, 5850, 235, "line", 1560, "documented",
        "THE STAFF-GUIDED LEFT-FLANK MOVE [D, 'after some two or three hours']: 'move by the left flank to the left, under the guidance of a staff officer... my troops were much exposed, and many were killed and wounded' — a costly lateral leg inside Muhlenberg's fire envelope (or-27-2-daniel), closing on Steuart's sector. Drawn: j3-02 'DANIEL' label moves ~470 m left/south (6035,5465) matching this leg."),
      kf(T(10, 0), 5900, 5340, 60, "line", 1420, "inferred",
        "THE STEUART CO-CHARGE (~10:00, ED-78/ED-64 provisional direction — Pfanz gate now unblocked by the owner ruling, see docs/reconstruction/angle-editorial-decisions.md ED-78): 'I received orders from General Johnson to charge the enemy's works, in conjunction with General Steuart. This charge was made in a most gallant manner, and the enemy driven from a portion of their works in front of my center and right... Owing to the heavy fire brought upon General Steuart, he was unable to advance farther, and I was, therefore, unable to occupy the works' (or-27-2-daniel p. 568) — CA-J3M-2 wave 3, both-sided with csa-steuart."),
      kf(T(11, 0), 5950, 5870, 140, "line", 1400, "documented",
        "Repulsed to a sheltered position 'within less than [~100 yards]' — the post-charge close-range hold (or-27-2-daniel, page cut in channel, distance class carried)."),
      kf(EndT, 4900, 5990, 140, "line", 1378, "documented",
        "At 13:00 the brigade holds east of the creek, spent. Daniel's OWN report states the brigade was 'not withdrawn until between 3 and 4 o'clock in the afternoon' with skirmishers 'engaged until nearly 12 o'clock at night' — clocks that fall AFTER this phase's 13:00 close; recorded honestly here as text (not truncated to fit) rather than authored as an out-of-window keyframe. STRENGTH ARITHMETIC ([D]): ANV return 916 (p. 342) minus the July 1 loss already established (2294 baseline -> 1650 t=0 here, a 644-man loss) = 272 July-3 share; 1650-272 = 1378. gettysburg-july3.json csa-daniel t=0 independently cites x=4200,z=5980,strength=1185 (position class differs — that file places Daniel at the Benner's Hill base per its own DOCUMENTED SILENCE convention, position-class not track-continuous with this phase's charge geometry) — a NAMED residual for the strength-reconciliation pipeline (see slice report), not reconciled here."),
    ],
  },
  {
    id: "csa-oneal", name: "O'Neal's Brigade, Rodes's Division (attached to Johnson)",
    side: "confederate",
    kfs: [
      opener("csa-oneal",
        "'Continuity static' placeholder in gettysburg-july2-evening.json (no evening combat); this phase's t=0 (1100) already reflects the July 1 Oak Hill loss (csa-2c-rod-5-oneal.md)."),
      kf(T(4, 45), 4300, 6100, 140, "column", 1100, "documented",
        "THE NIGHT MARCH [B-class, ~4 miles, ~0.7 m/s, ≈02:00-04:30/40]: 'About 2 a. m. on July 3, I was ordered to move to the left of our lines, to re-enforce General Edward Johnson, and arrived there at daylight' (or-27-2-oneal) — the second reinforcement column, ~30 min behind Daniel's. Drawn: j3-01 'O'NEAL ALA' arrival label (6223,5804)."),
      kf(T(8, 0), 6000, 5750, 200, "line", 1100, "documented",
        "CA-J3M-2 WAVE 2, THE EXECUTOR CLOCK: 'did not actively engage the foe until 8 a. m., when I was ordered to attack the works of the enemy, strongly posted in a long fort on the spur of the mountain. The attack was made with great spirit by the Sixth, Twelfth, Twenty-sixth, and Third Alabama Regiments... moved forward in fine style, under a terrific fire of grape and small-arms' (or-27-2-oneal) — the skeleton's softest clock resolved by the unit that made it. Drawn: j3-02 'O'NEAL' label moves ~510 m to the north-slope attack (5721,5694)."),
      kf(T(9, 0), 5800, 5700, 200, "line", 1040, "documented",
        "'Gained a hill near the enemy's works, which it held for three hours, exposed to a murderous fire' (or-27-2-oneal). STRENGTH HONESTY (flagged, thin headroom): ANV return 696 (p. 342, asterisked regimental counter-readings carried) minus the July 1 loss already established (1794 baseline -> 1100 t=0 here, a 694-man loss) leaves only ~2 men of arithmetic headroom for July 3 — inconsistent with the brigade's own 'murderous fire' three-hour narrative. This keyframe applies an APPROXIMATE additional decay (60 men, inferred, not exact-subtracted) for the attack-and-hold episode, recorded as a NAMED reconciliation gap (not resolved) — the return's 696 total likely undercounts the July 1 share relative to the build's inherited 1,794->1,100 figure, or vice versa; flagged for the strength-reconciliation pipeline."),
      kf(T(11, 0), 6000, 5900, 140, "scattered", 1040, "documented",
        "'Held their ground until ordered to fall back with the entire line' (CA-J3M-4's general order), then the behind-the-hill hold."),
      kf(EndT, 6050, 5950, 140, "line", 1040, "documented",
        "Behind the hill at the phase's close (its stated hold 'till 12 o'clock at night' extends past this window, recorded honestly). gettysburg-july3.json csa-oneal t=0 independently cites x=4300,z=6100,strength=1100 — a position-class (not track-continuous) residual, not reconciled here."),
    ],
  },
  {
    id: "csa-smith", name: "Smith's Brigade, Early's Division (attached to Johnson)",
    side: "confederate",
    kfs: [
      opener("csa-smith",
        "'Continuity static' placeholder in gettysburg-july2-evening.json; this file's t=0 (660) is inherited from the A1/A2-era July-3-afternoon reconstruction, which pre-baked the tablet's FULL 142-loss arithmetic ('Present about 800' less 142) into that placeholder BEFORE this per-phase morning authoring existed. This phase's own combat arc (below) applies only a modest further decay to avoid double-counting the tablet's already-spent loss — a NAMED structural residual (not an invented number), flagged for the strength-reconciliation pipeline (csa-2c-ear-2-smith.md, report No. 476)."),
      kf(T(4, 45), 6800, 6420, 250, "column", 660, "documented",
        "THE DETACHMENT MARCH [D, trigger primary]: 'After night, I was ordered by General Ewell to send Smith's brigade to report to General Johnson, on the left, by daylight, and General Smith was ordered to do so, and did report to General Johnson' (or-27-2-early p. 471) — the third reinforcement column, no clock of its own. Report-grain ground (Hoffman, No. 476, located pass 10): the 49th+52nd VA 'formed between the creek and the enemy's works near the left of General Johnson's division, and thence moved to the left, and formed nearly at right angles to the extreme left of that division.'"),
      kf(T(9, 30), 6700, 6300, 250, "line", 640, "documented",
        "THE FAR-LEFT DISLODGMENT: 'the Forty-ninth, supported by the Fifty-second Virginia Regiment, advanced upon a large body of the enemy near the left flank of that division, and dislodged it from its position' (or-27-2-hoffman-smith) — Smith on the far right of the CSA line / Johnson's extreme left, opposite the Spangler's Meadow ground."),
      kf(T(10, 5), 6650, 6150, 250, "line", 630, "documented",
        "THE SPANGLER'S MEADOW REPULSE (ED-78/ED-64 provisional ~10:00 direction, early pole 05:30-06:00 carried alongside — see the us-2ma/us-27in events below): 'It repulsed the charge of the 2nd Massachusetts and 27th Indiana Regiments against this line and held its ground until the Union forces regained their works on the hill' (brigade tablet, already quoted verbatim in gettysburg-july3.json's own csa-smith t=0 citation)."),
      kf(T(11, 30), 6820, 6440, 250, "line", 620, "documented",
        "'It then moved to a position further up the creek' (brigade tablet). STRENGTH ARITHMETIC NOTE: report primary (No. 476) totals 3 off + 12 men k / 5 off + 105 men w / 17 m = 142 EXACT against the tablet ('Present about 800... Total 142') — that full loss is already reflected in this phase's inherited t=0 (660); this keyframe's further -40 is a modest documented-notional decay for the dislodgment/repulse combat, NOT independently derived (residual, see report)."),
      kf(EndT, 6820, 6440, 250, "line", 620, "documented",
        "Further up the creek, quiet at the phase's close. gettysburg-july3.json csa-smith t=0 independently cites x=6800,z=6420,strength=660 — position near-exact, strength differs by ~40 (the structural double-count risk noted above); flagged for the strength-reconciliation pipeline, not reconciled here."),
    ],
  },
  // ============================= XII CORPS (US) ==============================
  {
    id: "us-greene", name: "Greene's Brigade (3rd Bde, 2nd Div, XII Corps)",
    side: "union",
    kfs: [
      opener("us-greene",
        "The one brigade that never left the works (us-xii-2-3-greene.md) — the chain's Union spine through both the evening and this morning. The PRE-WINDOW escalation (ED-65's opening-cluster class, recorded here as text, not authored as a move before this phase's 04:30 start): Kane 'the attack in force upon us commenced at 3.30 a. m.'; Cobham 'by 4 o'clock the firing had become general along the whole line' (or-27-1-kane/-cobham, Greene's neighbors) — Greene's own works receive the same cluster."),
      kf(T(4, 35), 5812, 5510, 60, "line", 1170, "documented",
        "CA-J3M-1: Muhlenberg's 4.30 program opens overhead; the relief rotation begins — 'relieved from thirty to ninety minutes by others with fresh ammunition... the fire was kept up constantly' (or-27-1-greene) — the fire-continuity mechanism, not a static seven-hour blaze."),
      kf(T(9, 0), 5812, 5510, 60, "line", 1080, "documented",
        "Deep into the seven-hour fight; the 66th OH's enfilade sortie (Candy's brigade) fires from beyond Greene's right; the 147th PA's 5 a.m. stone-wall charge (Candy) and the 20th CT's woods fight (McDougall) both anchor off Greene's flanks."),
      kf(T(10, 30), 5812, 5510, 60, "line", 1007, "documented",
        "CA-J3M-4's close: the general repulse and Johnson's withdrawal east of Rock Creek. STRENGTH ARITHMETIC ([D], exact subtraction against the build's own authored evening decay): return 303 (p. 185; report 307 carried as a 4-man component conflict, not resolved) minus the evening loss already authored in gettysburg-july2-evening.json (1310->1170 = 140, read from that file) = 163 July-3 share; 1170-163 = 1007."),
      kf(EndT, 5812, 5510, 60, "line", 1007, "documented",
        "The works held, quiet, at the phase's close — matching gettysburg-july3.json's own DOCUMENTED SILENCE for this ground (that file's us-greene t=0 independently cites strength 1125 — a residual noted in the slice report, both readings derive from the same evidence class, not reconciled here)."),
    ],
  },
  {
    id: "us-kane", name: "Kane's Brigade (2nd Bde, 2nd Div, XII Corps — Col. Cobham)",
    side: "union",
    kfs: [
      opener("us-kane",
        "Back in the refused works since ~22:00, on Greene's right (us-xii-2-2-kane.md). THE JULY 3 OPENING LADDER (PRE-WINDOW, recorded as text — before this phase's 04:30 start): Cobham 'At 3 o'clock next morning, July 3, the enemy's skirmishers commenced firing on us, and by 4 o'clock the firing had become general along the whole line'; Kane 'The attack in force upon us commenced at 3.30 a. m.' — the skirmish-to-general escalation PRECEDING Muhlenberg's 4.30 program."),
      kf(T(8, 35), 5845, 5400, 60, "line", 560, "documented",
        "THE RELIEF LADDER: '~8.35 a.m. the 14th Brooklyn + 147th NY (both together about 150 strong) sent in to reinforce Kane's right'; 'At 9 a.m. the One hundred and twenty-second New York... relieved the One hundred and eleventh Pennsylvania, of Kane's, which had been engaged in the front line all the morning, and whose ammunition was failing' (or-27-1-geary pp. 828-830) — the brigade's morning arc closes on a division-primary relief clock."),
      kf(T(10, 30), 5845, 5400, 60, "line", 502, "documented",
        "THE SEVEN-HOUR SPINE + THE LAST EFFORT: 'kept up a fire of unintermitting strength for seven hours, until about 10.30 o'clock, when the enemy made their last determined effort by charging in column of regiments' (or-27-1-kane) — CA-J3M-2 wave 3 and CA-J3M-4's edge in one primary sentence, receiving Steuart's/Daniel's charge. 'The Confederate Major-General Johnson's division led, followed by Rodes'' (Kane) — naming the Daniel/O'Neal reinforcement wave. STRENGTH ([D]): the brigade's two reports conflict (Kane 96 vs Cobham 98; return = Cobham's 98, p. 184: 29th Pa 66, 109th Pa 10, 111th Pa 22) minus the evening loss already authored (600->595 = 5) = 93 July-3 share; 595-93 = 502."),
      kf(EndT, 5845, 5400, 60, "line", 502, "documented",
        "The works held, quiet, at the close (gettysburg-july3.json us-kane t=0 independently cites strength 600 — a residual noted in the slice report, not reconciled here)."),
    ],
  },
  {
    id: "us-candy", name: "Candy's Brigade (1st Bde, 2nd Div, XII Corps)",
    side: "union",
    kfs: [
      opener("us-candy",
        "Still forming up as the division's rotation-pool reserve behind the works (us-xii-2-1-candy.md). PRE-WINDOW (recorded as text, before this phase's 04:30 start): 'Remained in the latter position until 3.45 a. m., when the enemy were opened upon by a battery placed for the purpose of shelling them' (or-27-1-candy) — 45 min ahead of Muhlenberg's stated 4.30 program, inside the opening cluster's envelope."),
      kf(T(5, 0), 5680, 5050, 60, "line", 1370, "documented",
        "'At 5 a.m. the One hundred and forty-seventh Pennsylvania... was ordered to charge and carry the stone wall... They did in handsome style' (or-27-1-geary pp. 828-830) — the ground the battlefield later names Pardee Field, after the 147th's colonel."),
      kf(T(5, 45), 5750, 5150, 80, "line", 1350, "documented",
        "'At 5.45 a.m. the Sixty-sixth Ohio was ordered to advance outside of Greene's intrenchments and perpendicular to them... recalled by an order at 11 a.m.' (or-27-1-geary) — the morning's only Union outside-the-works episode; Geary's own friendly-fire note: 'a few of their men advanced too far, and fell by our own artillery fire.' 'At 6 a.m. the Twenty-eighth Pennsylvania, and Fifth, Seventh, and Twenty-ninth Ohio... were ordered into the intrenchments, to relieve some of Greene's regiments... between the files' — the rotation-pool mechanics."),
      kf(T(11, 0), 5680, 5050, 60, "line", 1261, "documented",
        "The 66th OH recalled at 11 a.m., its enfilade sortie done. STRENGTH ([D]): return brigade total 139 (p. 184: 5th OH 18, 7th OH 18, 29th OH 38, 66th OH 17 — the exposed sortie's flagged key cell, 28th Pa 28, 147th Pa 20) minus the evening loss already authored (1400->1400 = 0) = 139 July-3 share; 1400-139 = 1261."),
      kf(EndT, 5680, 5050, 60, "line", 1261, "documented",
        "Back in the rotation pool, quiet (gettysburg-july3.json us-candy t=0 independently cites strength 1400 — a residual noted in the slice report, not reconciled here)."),
    ],
  },
  {
    id: "us-mcdougall", name: "McDougall's Brigade (1st Bde, 1st Div, XII Corps)",
    side: "union",
    kfs: [
      opener("us-mcdougall",
        "Behind the rise south of the vacated works, arms-down since the night discovery (us-xii-1-1-mcdougall.md) — 'withdrew a short distance to the rear... concealed by a rise of ground... remained upon their arms until morning.'"),
      kf(T(4, 35), 5950, 5230, 100, "line", 1755, "documented",
        "The rally ground at dawn, j3-01 re-read this pass (5991,4530): CA-J3M-1's artillery preparation begins the division's morning plan — no night assault, the works reoccupation waits for the guns."),
      kf(T(5, 0), 6000, 5000, 60, "column", 1740, "documented",
        "THE 20TH CT WOODS FIGHT: 'advanced into the woods in front of our troops, where the enemy had posted himself' — fighting to 'keep the enemy in check' WHILE dodging friendly overhead artillery; men 'severely wounded by our artillery fire' (McDougall) — the friendly-fire record matching Muhlenberg's 600-800-yard overhead arc."),
      kf(T(10, 30), 5950, 5150, 60, "line", 1700, "documented",
        "THE WORKS REOCCUPATION (CA-J3M-4): the 123rd NY 'not finding any enemy... entered and then held the breastworks' — Ruger's 'line of the division from center to left was at once advanced' (both echelons agreeing) — the brigade line follows into the recovered ground."),
      kf(T(12, 0), 5950, 5230, 100, "line", 1675, "documented",
        "In the recovered works. STRENGTH ([D]): return 80 (p. 184: 5th CT 7, 20th CT 28 — the woods fight's largest row, 3rd MD 8, 123rd NY 14, 145th NY 10, 46th PA 13) minus the evening loss already authored (1755->1755 = 0) = 80 July-3 share; 1755-80 = 1675."),
      kf(EndT, 5950, 5230, 100, "line", 1675, "documented",
        "Quiet in the reoccupied works (gettysburg-july3.json us-mcdougall t=0 independently cites strength 1755 — a residual noted in the slice report, not reconciled here)."),
    ],
  },
  {
    id: "us-lockwood", name: "Lockwood's Brigade (2nd Bde, 1st Div, XII Corps)",
    side: "union",
    kfs: [
      opener("us-lockwood",
        "The night ground, unattached but under corps/division direction (us-xii-1-2-lockwood.md). PRE-WINDOW (recorded as text, before this phase's 04:30 start): THE ~4 A.M. STONE-WALL ATTACK (Maulsby, pp. 805-807 in full, pass 10) — bayonets fixed, ceased fire for the flanking 'brigade of National troops' — the ED-65 escalation preamble's Lockwood member."),
      kf(T(6, 0), 5990, 5060, 90, "line", 1620, "documented",
        "THE 6 A.M. WOODS DEPLOYMENT: 'At about 6 a. m. I received orders to deploy a regiment and engage the enemy within these woods' (or-27-1-lockwood) — Maulsby's 1st MD PHB into the meadow-flank woods; 'these regiments supported a battery placed to shell the woods in front of the rifle-pits on our right' — the program's infantry-support corroboration. DISTINCT from the Colgrove/Spangler's-Meadow charge — the conflation risk ED-64/ED-78 name explicitly."),
      kf(T(9, 0), 6000, 5000, 90, "line", 1560, "documented",
        "'About 9 o'clock... hold the front of the rifle-pits... until the enemy's fire wholly ceased, about 12 m.' (Maulsby) — a late-edge close reading recorded against CA-J3M-4's 10:30-11:00 (both carried, not reconciled)."),
      kf(T(11, 30), 5900, 5100, 90, "line", 1500, "documented",
        "The white-flag scene (Maulsby, no single-claimant caution carried); officer pins Lts. Smith/Willman k at the stone wall, Lt. Eader k at the rifle-pits."),
      kf(EndT, 5850, 5150, 90, "line", 1476, "documented",
        "Quiet at the phase's close. STRENGTH ([D]): return 174 (p. 184: 1st MD PHB 104 — the woods fight's cost, 1st MD ES 25, 150th NY 45) minus the evening loss already authored (1650->1645 = 5) = 169 July-3 share; 1645-169 = 1476. gettysburg-july3.json us-lockwood t=0 independently cites strength 1650 — a residual noted in the slice report, not reconciled here."),
    ],
  },
  // ---- Colgrove's brigade, minus the 2nd MA and 27th IN (ED-76 convention) --
  {
    id: "us-colgrove",
    name: "Colgrove's Brigade (3rd Bde, 1st Div, XII Corps — minus the 2nd Massachusetts and 27th Indiana)",
    side: "union",
    kfs: [
      kf(0, 6080, 4800, 30, "line", 515, "documented",
        "CONTINUITY WITH DECOMPOSITION: the whole brigade held gettysburg-july2-evening.json's cited t=10860 (22:30 LMT) position/facing (x=6080,z=4800,facing=30) through the overnight non-combat gap, strength 1170 — but THIS phase promotes the 2nd MA and 27th IN as parentless siblings (ED-76 convention: minority-regiment promotions dodge the parent/children family-suppression contract), so this unit's own t=0 strength is the EXACT SUBTRACTION: 1170 - 316 (2nd MA: 22 off + 294 enl, mon-2ma/Cogswell p. 818) - 339 (27th IN: 'officers and men', or-27-1-fesler-27in, monument-exact) = 515, covering the remaining three regiments (13th NJ, 107th NY, 3rd WI). In the meadow's south works (us-xii-1-3-colgrove.md). PRE-WINDOW (recorded as text, before this phase's 04:30 start): 'Before it was fairly light, the battle commenced on our left' (Colgrove); Grimes (13th NJ) 'At 4 a. m. July 3, firing commenced by the enemy' — CA-J3M-1's opening cluster on the brigade's own front."),
      kf(T(9, 45), 6080, 4800, 30, "line", 500, "documented",
        "Fire support for the charge across the meadow (below): '3rd WI + supports covering the withdrawal... at every volley of the enemy, gaps were being cut through its ranks' on the 27th's oblique (Colgrove)."),
      kf(T(10, 30), 6150, 4820, 30, "line", 482, "documented",
        "The counter-repulse: the enemy's counter-rush over the works 'quickly driven back by the two regiments, who turned and opened fire' (or-27-1-ruger p. 781) — ~100 prisoners taken. STRENGTH ([D]): return rows 13th NJ 21 + 107th NY 2 + 3rd WI 10 = 33 of the brigade return's 279; 515-33 = 482."),
      kf(EndT, 6080, 4800, 30, "line", 482, "documented",
        "Under the afternoon cannonade's opening reach: 'shell, shot, and missiles bursting over them, around them, and among them for hours' (Colgrove) — the works held, quiet otherwise. gettysburg-july3.json us-colgrove t=0 independently cites strength 1170 (whole-brigade, pre-decomposition figure) — expected given the decomposition; the slice report notes the family total (482+2ma+27in) for the reconciliation ledger."),
    ],
  },
  {
    id: "us-2ma", name: "2nd Massachusetts (Colgrove's)", side: "union",
    kfs: [
      kf(0, 6096, 4865, 30, "line", 316, "documented",
        "At the works: 'From the hill behind this monument on the morning of July Third 1863 the Second Massachusetts Infantry made an assault upon the Confederate troops in the works at the base of Culp's Hill opposite' (mon-2ma inscription). Strength: '294 enlisted men' (Cogswell's supplementary, p. 818) + '22 officers' (mon-2ma) = 316 all ranks. Lieut. Col. Charles R. Mudge commanding."),
      kf(T(5, 30), 6096, 4865, 30, "line", 316, "documented",
        "THE EARLY POLE, CARRIED VERBATIM (ED-64/ED-78 two-pole record): Morse (2nd MA, No. 288): 'At daylight, July 3, our skirmishers, Company E, Captain Robeson, became engaged. Firing was kept up until 5.30 o'clock, when the regiment was ordered to charge the woods in front of us.' This keyframe records the pole's TIME without moving the charge's authored position — the regiment's own primary states 5.30, not adopted as the render time (see the t=10:00 keyframe below, ED-78's provisional direction)."),
      kf(T(10, 0), 6079, 4921, 10, "scattered", 200, "inferred",
        "THE CHARGE, AUTHORED AT THE ED-78/ED-64 PROVISIONAL ~10:00 DIRECTION (owner ruling 2026-07-13, docs/reconstruction/angle-editorial-decisions.md ED-78 — 'make a best guess inference for right now'; supersedes the Pfanz-gate BLOCK on this anchor; the EARLY POLE ~05:30 is carried verbatim above and in the event note, both poles on the record): 'The men jumped over the breastworks with a cheer, and went forward on the double-quick' (Morse); THE MUDGE PIN — 'Colonel Mudge gave the order, Forward!... I now took command of the regiment, Colonel Mudge having been killed' (Morse, p. 817); Colgrove: Mudge 'fell while gallantly leading his men in the charge.' Drawn: j3-02 the 8-11 a.m. sheet draws '2 Mass' advanced (6079,4921), 59 m from the monument — Bachelder's own time adjudication for the LATE pole. Repulsed by Smith's Virginians (csa-smith): 'It repulsed the charge of the 2nd Massachusetts and 27th Indiana Regiments against this line' (Smith's brigade tablet)."),
      kf(T(10, 12), 6096, 4865, 30, "line", 180, "documented",
        "Withdrawn by order, covered by the supports' fire (Colgrove/3rd WI). STRENGTH: Morse's own enumeration (2 o. k, 2 o. mw, 6 o. w; 21 m k, 102 w, 3 m ~= 136) = return 136 (agreement at aggregate); 316-136 = 180."),
      kf(EndT, 6096, 4865, 30, "line", 180, "documented",
        "Back at the works, quiet — a new parentless unit this phase (not present in gettysburg-july3.json; the afternoon file continues to render Colgrove's brigade whole, a residual noted in the slice report)."),
    ],
  },
  {
    id: "us-27in", name: "27th Indiana (Colgrove's)", side: "union",
    kfs: [
      kf(0, 6120, 4885, 30, "line", 339, "documented",
        "At the works: '339 officers and men' carried into action (or-27-1-fesler-27in), matched EXACTLY by the monument's 'Number engaged 339' (mon-27in). Lieut. Col. John R. Fesler commanding."),
      kf(T(6, 0), 6120, 4885, 30, "line", 339, "documented",
        "THE EARLY POLE, CARRIED VERBATIM (ED-64/ED-78 two-pole record): Fesler occupied the 3rd WI works 'between 5 and 6 o'clock… I was then ordered by you to charge' (or-27-1-fesler-27in); the 27th IN's own farthest-advance marker INSCRIBES the clock: 'its charge at 6 a.m. July 3d. 1863.' This keyframe records the pole's time without moving the charge's authored position (see the t=10:00 keyframe below, ED-78's provisional direction)."),
      kf(T(10, 0), 6156, 4908, 10, "scattered", 240, "inferred",
        "THE CHARGE, AUTHORED AT THE ED-78/ED-64 PROVISIONAL ~10:00 DIRECTION (both poles on the record; see docs/reconstruction/angle-editorial-decisions.md ED-78): Colgrove's tactical statement — a line of skirmishers 'would be cut down before they could fairly gain the open ground', so 'carry his position by storming it'; the 27th obliquing right with 'nearly double the distance to traverse' the 2nd MA had. Ruger's order chain (the order-corruption pair, carried unresolved): Colgrove — 'The general directs that you advance your line immediately' (verbal, via Lieut. Snow) vs Ruger — 'advance skirmishers... and, if not found in too great force, to advance two regiments' with his own 'mistake of the staff officer, or misunderstanding on the part of Colonel Colgrove.' Drawn: j3-02 '27 Ind' bar advanced (6156,4908), 69 m from the main monument, 79.2 m from the farthest-advance marker (mon-27in-advance) — Bachelder's own time adjudication for the LATE pole. THE 27TH IN FARTHEST-ADVANCE MARKER (6115.9,4964.0) fixes the charge's far edge. Four color bearers killed, four wounded (advance marker)."),
      kf(T(10, 15), 6120, 4885, 30, "line", 229, "documented",
        "Withdrawn by order under fire, the oblique's cost: 'at every volley of the enemy, gaps were being cut through its ranks' (Colgrove). STRENGTH: Fesler's itemized episode split — 'In the charge… killed, 15; wounded, including 7 commissioned officers, 83… in works, 3 enlisted men killed; wounded, 10… Total… 111' (98 of 111 in the charge itself) — vs the return's 110 vs Colgrove's '112' (all three carried, un-averaged); 339-110 = 229 (the return-basis figure)."),
      kf(EndT, 6120, 4885, 30, "line", 229, "documented",
        "Back at the works, quiet — a new parentless unit this phase (not present in gettysburg-july3.json; a residual noted in the slice report)."),
    ],
  },
  {
    id: "us-shaler", name: "Shaler's Brigade (1st Bde, 3rd Division, VI Corps)",
    side: "union",
    kfs: [
      opener("us-shaler",
        "In reserve behind the Culp's Hill woods, the morning fight already underway (us-vi-3-1-shaler.md — Shaler's own two reports, battle-day + a November 1863 correction letter naming HIS brigade, not 'Wheaton's', as the one that reported to Geary)."),
      kf(T(8, 0), 5650, 4500, 20, "column", 1770, "documented",
        "'Ordered to the left and at 8 A. M. to the right to the support of Second Division Twelfth Corps. Took position in rear of woods on Culp's Hill beyond which action was progressing' (brigade tablet, verbatim); Shaler's own report independently: 'reported to Brigadier-General Geary at 8 a.m. on July 3rd, positioning themselves in rear of a piece of woods, beyond which the action was then progressing' — report=tablet exact agreement."),
      kf(T(9, 0), 5620, 5240, 80, "line", 1770, "documented",
        "THE REGIMENT-GRAIN LADDER: '9:00 a.m. — 122nd NY directed to relieve 111th PA [a XII Corps regiment]; 9:20 a.m. — 23rd PA positioned 150 yards in rear; ~3 hours later — five companies detached to support Second Division, Twelfth Corps' (Shaler's report, verbatim) — 'engaged under command of Brig. Gen. J. W. Geary from 9 until 11 A. M.' (tablet), 'under a severe fire' / 'galling fire of musketry' (report) — direct combat contact, the corps's one genuinely committed firing-line unit this battle."),
      kf(T(11, 30), 5650, 4600, 20, "column", 1696, "documented",
        "'11:30 a.m. — 122nd NY relieved by 82nd PA' (Shaler) — the engagement closes inside CA-J3M-4's window. STRENGTH: brigade tablet 'Killed 1 Off + 14 Men, Wounded 3 Off + 53 Men, Captured/Missing 3 Men, Total 74' (verbatim, entirely July 3 per the dossier's own reading — the corps's heaviest VI Corps brigade loss this battle); 1770-74 = 1696."),
      kf(EndT, 5650, 4500, 20, "column", 1696, "documented",
        "Retiring toward reserve (the stated 15:30 withdrawal to the II/III Corps center-rear falls after this phase's 13:00 close — recorded honestly, not truncated to fit; gettysburg-july3.json us-shaler t=0 independently cites strength 1770 — a residual noted in the slice report, not reconciled here)."),
    ],
  },
];

// ---------------------------------------------------------------------------
// Muhlenberg's Artillery Brigade, XII Corps — the CA-J3M-1 anchor's AUTHOR
// unit (not-yet-cast; reg-us-xii-arty). Group grain per the evidence (the
// report is the brigade's one voice; battery-grain records are section-level
// fragments inside it — us-xii-arty-muhlenberg.md).
// ---------------------------------------------------------------------------
const muhlenberg: M = {
  id: "us-xii-arty", name: "Artillery Brigade, XII Corps (Muhlenberg)",
  side: "union", frontage: 650, depth: 30,
  kfs: [
    kf(0, 5550, 4550, 60, "line", 480, "documented",
      "The Baltimore Pike / Powers Hill / McAllister's Hill gun line, 600-800 yards from the works: mon-rugg-f4us (5445.6,4845.6, F/4th US), mon-kinzie-pike (5457.7,4830.3, K/5th US), M/1st NY toward Powers Hill (mon-m1ny-powers 5787.3,4123.4) — Knap's Pennsylvania battery (E) completes the four. First Lieut. Edward D. Muhlenberg (4th US) commanding, four batteries: F/4th US (Rugg), K/5th US (Kinzie), E Pennsylvania (Atwell/Knap's), M/1st NY (Winegar). STRENGTH: NO personnel figure in the report (open, EC2) — ~24 men/gun (the Jones's-battalion precedent) x 20 guns (the report's own count) = 480, flagged reconstruction."),
    kf(T(4, 32), 5550, 4550, 60, "line", 480, "documented",
      "CA-J3M-1, THE ANCHOR'S AUTHOR TEXT: 'At 4.30 a. m. the two rifle batteries (ten guns) and the two light 12-pounder batteries (ten guns) opened… fired for fifteen minutes without intermission at a range of from 600 to 800 yards… Commenced at 5.30 a. m., and continued firing at intervals until 10 a. m.' (or-27-1-muhlenberg, pp. 870-871) — the corps commander's report carries the same program verbatim-class: 'The artillery opened with a tremendous fire at daylight, at from 600 to 800 yards range, which was continued by arrangement for fifteen minutes' (or-27-1-williams-aw). GUN-COUNT CONFLICT CARRIED (ED-65, not adopted): report 20 guns vs the WD tablet's 26 — the likeliest reconciliation (Rigby's A/1st MD, Artillery Reserve, Powers Hill, 6 guns) is flagged, NOT adopted."),
    kf(T(5, 45), 5550, 4550, 60, "line", 480, "documented",
      "The 15-minute by-arrangement pause (04:45-05:30, encoded structurally by the two fire events below) ends; resumed firing 'at intervals' through the morning."),
    kf(EndT, 5550, 4550, 60, "line", 471, "documented",
      "THE PROGRAM'S END, CONFLICT CARRIED (ED-65): report 'until 10 a. m.' vs the WD tablet's '10.30 A.M.' — Jacobs ('From 4½ to 10½') and the Johnson tablet side with 10.30; the report's 10:00 is carried as the author's own reading, not adjudicated. STRENGTH: return p. 185, 9 w aggregate (Knap's 3, F/4th US 1, K/5th US 5) — a light loss for a static gun line; 480-9 = 471."),
  ],
};

// ---------------------------------------------------------------------------
// Events — fire windows per the activity records (attach level = the level
// the source attests; documented silence stays event-free).
// ---------------------------------------------------------------------------
const events: EngagementEvent[] = [
  // CA-J3M-1: Muhlenberg's ladder (opening burst + resumed program).
  fireEvent({
    id: "j3m-muhlenberg-open", kind: "artillery_fire", unitId: "us-xii-arty",
    t0: T(4, 30), t1: T(4, 45), confidence: "documented",
    citation: "'At 4.30 a. m. the two rifle batteries (ten guns) and the two light 12-pounder batteries (ten guns) opened… fired for fifteen minutes without intermission' (or-27-1-muhlenberg).",
  }),
  fireEvent({
    id: "j3m-muhlenberg-resume", kind: "artillery_fire", unitId: "us-xii-arty",
    t0: T(5, 30), t1: T(10, 0), confidence: "documented",
    citation: "'Commenced at 5.30 a. m., and continued firing at intervals until 10 a. m.' (or-27-1-muhlenberg) — the tablet's 10.30 close is the conflict's late component (carried, ED-65).",
  }),
  // The Johnson-division works fight, brigade grain, "unintermitting strength".
  fireEvent({
    id: "j3m-steuart-fire", kind: "musketry", unitId: "csa-steuart",
    t0: 0, t1: T(11, 0), confidence: "documented",
    citation: "'Raged fiercely until 11 A.M.' (brigade tablet); the dawn escalation through the doomed charge — Kane's receiving-side 'unintermitting strength for seven hours.'",
  }),
  fireEvent({
    id: "j3m-williams-fire", kind: "musketry", unitId: "csa-williams",
    t0: T(4, 45), t1: T(12, 0), confidence: "documented",
    citation: "'At early light opened on the enemy again... four hours almost without cessation, and at intervals until 12 m.' (or-27-2-williams-nicholls).",
  }),
  fireEvent({
    id: "j3m-walker-fire", kind: "musketry", unitId: "csa-walker",
    t0: 0, t1: T(12, 0), confidence: "documented",
    citation: "'My right, extending beyond the breastworks, suffered very heavily'; 'desultory fire to dark' (or-27-2-walker-stonewall) — five hours' incessant firing plus two further advances.",
  }),
  fireEvent({
    id: "j3m-dungan-fire", kind: "musketry", unitId: "csa-dungan",
    t0: 0, t1: T(11, 0), confidence: "documented",
    citation: "The lead sector's continuous fire through the division's renewed assaults (or-27-2-johnson: 'two other assaults were made and repulsed').",
  }),
  fireEvent({
    id: "j3m-daniel-fire", kind: "musketry", unitId: "csa-daniel",
    t0: 0, t1: T(11, 0), confidence: "documented",
    citation: "'My troops were much exposed, and many were killed and wounded' on the staff-guided flank move (or-27-2-daniel), through the Steuart co-charge.",
  }),
  fireEvent({
    id: "j3m-oneal-fire", kind: "musketry", unitId: "csa-oneal",
    t0: T(8, 0), t1: T(11, 0), confidence: "documented",
    citation: "'The attack was made with great spirit... under a terrific fire of grape and small-arms'; 'gained a hill... held it for three hours, exposed to a murderous fire' (or-27-2-oneal) — CA-J3M-2 wave 2.",
  }),
  fireEvent({
    id: "j3m-smith-fire", kind: "musketry", unitId: "csa-smith",
    t0: T(9, 30), t1: T(11, 0), confidence: "documented",
    citation: "'Advanced upon a large body of the enemy near the left flank... and dislodged it from its position' (or-27-2-hoffman-smith); the Spangler's Meadow repulse.",
  }),
  fireEvent({
    id: "j3m-greene-fire", kind: "musketry", unitId: "us-greene",
    t0: 0, t1: T(10, 30), confidence: "documented",
    citation: "'Relieved from thirty to ninety minutes by others with fresh ammunition... the fire was kept up constantly' (or-27-1-greene) — the seven-hour fight's spine (its true opening at 3.30 a.m. precedes this phase's 04:30 window, recorded on the unit's t=0 citation).",
  }),
  fireEvent({
    id: "j3m-kane-fire", kind: "musketry", unitId: "us-kane",
    t0: 0, t1: T(10, 30), confidence: "documented",
    citation: "'Kept up a fire of unintermitting strength for seven hours, until about 10.30 o'clock, when the enemy made their last determined effort by charging in column of regiments' (or-27-1-kane) — its true opening at 3.30 a.m. precedes this phase's 04:30 window, recorded on the unit's t=0 citation.",
  }),
  fireEvent({
    id: "j3m-candy-fire", kind: "musketry", unitId: "us-candy",
    t0: 0, t1: T(11, 0), confidence: "documented",
    citation: "The 147th PA's 5 a.m. stone-wall charge and the 66th OH's 5.45 a.m.-11 a.m. enfilade sortie outside the works (or-27-1-geary pp. 828-830).",
  }),
  fireEvent({
    id: "j3m-mcdougall-fire", kind: "musketry", unitId: "us-mcdougall",
    t0: T(5, 0), t1: T(10, 30), confidence: "documented",
    citation: "The 20th CT's woods fight 'to keep the enemy in check', including friendly overhead-artillery wounds (McDougall).",
  }),
  fireEvent({
    id: "j3m-lockwood-fire", kind: "musketry", unitId: "us-lockwood",
    t0: 0, t1: T(12, 0), confidence: "documented",
    citation: "The ~4 a.m. stone-wall attack (pre-window) and the 6 a.m. Maulsby woods deployment, 'until the enemy's fire wholly ceased, about 12 m.' (Maulsby).",
  }),
  fireEvent({
    id: "j3m-colgrove-fire", kind: "musketry", unitId: "us-colgrove",
    t0: 0, t1: T(10, 30), confidence: "documented",
    citation: "The 13th NJ/107th NY/3rd WI works fire and the counter-repulse covering fire (Colgrove).",
  }),
  // THE SPANGLER'S MEADOW CHARGE — ED-78/ED-64 provisional ~10:00, both poles
  // carried (Morse 5.30 / the 27th IN advance marker's inscribed 6 a.m.).
  fireEvent({
    id: "j3m-spangler-2ma", kind: "musketry", unitId: "us-2ma",
    t0: T(10, 0), t1: T(10, 12), confidence: "documented",
    citation: "'The men jumped over the breastworks with a cheer, and went forward on the double-quick' (Morse) — the charge, repulsed by Smith's Virginians.",
    note: "PROVISIONAL under ED-78 (docs/reconstruction/angle-editorial-decisions.md — the owner's 'make a best guess inference for right now' ruling, 2026-07-13), which supersedes the ED-30/ED-64 Pfanz-gate BLOCK. BOTH POLES ON THE RECORD: EARLY POLE ~05:30 — Morse (2nd MA, No. 288): 'Firing was kept up until 5.30 o'clock, when the regiment was ordered to charge the woods in front of us.' LATE POLE ~10:00 (the direction authored here) — Ruger (division): 'This state of things continued until about 10 a. m.… At this time I received orders to try the enemy… with two regiments'; Bachelder's j3-02 (8-11 a.m. sheet) draws the advanced '2 Mass'/'27 Ind' bars, NOT the 4-8 a.m. sheet. Neither pole is adjudicated; the Pfanz *Culp's Hill* pp. ~340-355 precondition (ED-30/ED-64) is no longer a BLOCK on authoring (ED-78) but remains open as a hardening target.",
  }),
  fireEvent({
    id: "j3m-spangler-27in", kind: "musketry", unitId: "us-27in",
    t0: T(10, 0), t1: T(10, 15), confidence: "documented",
    citation: "The 27th IN's oblique, 'nearly double the distance to traverse' the 2nd MA's — Colgrove: 'at every volley of the enemy, gaps were being cut through its ranks.'",
    note: "PROVISIONAL under ED-78 (see the us-2ma event note for the full ruling text). BOTH POLES ON THE RECORD: EARLY POLE ~06:00 — Fesler (27th IN, No. 287): occupied the works 'between 5 and 6 o'clock… I was then ordered by you to charge'; the 27th IN's OWN FARTHEST-ADVANCE MARKER INSCRIBES 'its charge at 6 a.m. July 3d. 1863' (verbatim, mon-27in-advance). LATE POLE ~10:00 (the direction authored here) — Ruger's order-chain primary + Bachelder's j3-02 drawn adjudication (the same evidence as the 2nd MA event). Neither pole is adjudicated.",
  }),
];

// ---------------------------------------------------------------------------
// The battle document.
// ---------------------------------------------------------------------------
const allMovers = [...movers, muhlenberg];
const battle: Battle = {
  name: "Gettysburg — July 3, 1863: Morning (Culp's Hill)",
  startTime: StartTime,
  endTime: EndT,
  units: allMovers.map((m) => moverUnit({
    id: m.id, name: m.name, side: m.side,
    frontage_m: m.frontage, depth_m: m.depth, keyframes: m.kfs,
  })),
  events,
  environment: {
    windTowardDeg: 45, windMps: 0.0, confidence: "unknown",
    note: "No sourced wind observation for the July 3 morning exists in the corpus (the ED-10/ED-19 class); calm authored — windMps 0 = no drift.",
  },
};

writeFileSync(outPath, exportValidated(battle));
console.log(`wrote ${outPath}: ${battle.units.length} units, ${events.length} events, ` +
  `window ${StartTime}..${StartTime + EndT} (04:30-13:00 LMT)`);
