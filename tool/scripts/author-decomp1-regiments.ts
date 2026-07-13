// Decomposition wave 1 — the three July 1 regiment-grain promotions (slice-3
// §7 item 5 / §9 pickup 1): the 6th Wisconsin (railroad cut), the 147th New
// York (the stranded stand), the 16th Maine (the sacrifice hill). Each
// regiment's episode is already authored VERBATIM inside its brigade's own
// keyframe citations (gettysburg-july1-morning.json us-cutler; gettysburg-
// july1-afternoon.json us-coulter; the j1m-rrcut-charge fixed-segment event)
// — this script promotes each to its own first-class tracked unit.
//
// DESIGN CALL (proposed ED, see docs/reconstruction/audit/decomposition-
// wave-1.md): these are PARENTLESS siblings, not `parent`-linked children.
// The Harrow (slice 2) and Webb/Stannard/Hall (A5) precedents use `parent`
// because those decompositions cover ALL or NEARLY ALL of the brigade's
// regiments (Harrow 4/4, Webb 3/4 with only two companies of the 4th
// unmodeled) — BattleDirector's family-suppression contract (battle-
// format.md "Parent / children"; BattleDirector.cs RendersAtTier) hides the
// PARENT symbol at every tier nearer than Block once it has ANY children, so
// a single-regiment promotion out of a 5-6 regiment brigade would erase the
// other 80-85% of the brigade from view at the Regiments/Soldiers tiers.
// This wave's three promotions are each ONE regiment out of five (Meredith,
// Paul) or six (Cutler) — the Carroll's-Brigade/8th-Ohio precedent already
// in the shipped July 3 file (gettysburg-july3.json us-carroll: "minus the
// 8th Ohio" — a parentless sibling, "NO parent link (family rules: none,
// divisions not modeled)") is the applicable pattern, not Harrow's. Each
// brigade's own aggregate track is renamed "... minus the Nth ___" and its
// strength reduced by the promoted regiment's own figure at every shared
// keyframe time, so the two together always sum to the brigade's total —
// exact subtraction, not an advisory tolerance band.
//
// Run from tool/:
//   npx vite-node scripts/author-decomp1-regiments.ts
// Wave report: docs/reconstruction/audit/decomposition-wave-1.md
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe, Unit } from "../src/model";
import { addUnitsAfter, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const morningPath = join(here, "../../app/Assets/Battle/gettysburg-july1-morning.json");
const afternoonPath = join(here, "../../app/Assets/Battle/gettysburg-july1-afternoon.json");

let morning: Battle = JSON.parse(readFileSync(morningPath, "utf8"));
let afternoon: Battle = JSON.parse(readFileSync(afternoonPath, "utf8"));

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
// 1. THE 6TH WISCONSIN (parentless sibling of us-meredith), morning + afternoon
// ===========================================================================
//
// Sources: reconstruction/dossiers/us-i-1-1-meredith.md (EC1/EC2/EC4.2/EC5.2/
// EC6; Conflicts item 4 — "brigade strength primary open, only the 24th's
// 496 fetched"); or-27-1-dawes-6wi (the reserve detachment + the cut-charge
// primary, full text in reconstruction/sources/sources.json); or-27-1-cutler
// (both-sided: the 147th's release, the re-advance to the crest);
// or-27-1-union-return p.173 (the day-total loss, 168 — adopted over Dawes's
// own itemized 158/"not less than 160", un-averaged, both carried).

const sixWisMorning: Keyframe[] = [
  kf(0, 4400, 5100, 340, "column", 333,
    "March from Marsh Creek with the brigade (or-27-1-morrow-24mi); detached at the deployment as 'a general reserve to the line of the division' with the 100-man brigade guard attached, by Doubleday's order (or-27-1-dawes-6wi verbatim; dossier us-i-1-1-meredith.md EC1/EC4.2). STRENGTH HONESTY: no regiment primary fetched (dossier Conflicts item 4: 'only the 24th's 496 fetched'); authored 333 compilation-class FLAGGED (brigade total 1,829 minus the 24th Mich's primaried 496, split evenly across the four un-primaried regiments — same class as Cutler's own ~1,600 brigade-level flag). The return's regiment-grain LOSS row (168, or-27-1-union-return p.173) is the only hard EC6 primary this unit carries. The attached 100-man brigade guard is NOT folded into this figure (a separate, uncounted body per Dawes's own phrasing)."),
  kf(8100, 4000, 6500, 320, "column", 333,
    "Continuity: marching with the brigade column (slot-follow of Meredith's own approach leg; no independent clock or position exists for the detachment before deployment).",
    "inferred"),
  kf(9900, 3200, 7150, 292, "line", 333,
    "HALTED AS RESERVE: 'halted... and detached from the brigade as a general reserve to the line of the division' (or-27-1-dawes-6wi) — while the rest of the brigade charges into Willoughby Run (CA-J1M-4b/5), the 6th Wis + guard holds back. No coordinate is stated in Dawes's report; position inferred near the division's reserve ground west of the ravine, held back from Meredith's own charge line.",
    "inferred"),
  kf(11700, 3400, 7690, 290, "line", 333,
    "Ordered up toward the railroad cut as Cutler's right wrecks and the 147th NY is stranded: the fence fire-by-file checks the CSA advance ('checked the advance of the rebels, who took refuge in a railroad cut', or-27-1-dawes-6wi) — the fence-line position at the ground the charge itself opens from (event j1m-rrcut-charge's own start coordinate); no loss yet attributed to this specific phase in Dawes's report."),
  kf(12900, 3560, 7720, 290, "line", 165,
    "THE RAILROAD-CUT CHARGE (CA-J1M-6) concludes: 'upon a double-quick, well closed, in face of a terribly destructive fire'; 'the rebels began throwing down their arms in token of surrender'; Maj. Blair's 2nd Miss surrenders to Dawes, '7 officers and about 225 men' to the cavalry guard, the 2nd Miss flag (Cpl. Waller, MoH), the 2nd Maine gun recaptured (or-27-1-dawes-6wi) — both-sided with Davis's report and the 147th NY's release (or-27-1-cutler). LOSS, un-averaged: Dawes 'not less than 160 men killed or wounded' in the charge alone / his own itemized July-1 total 2+5 officers, 27 k / 100 w / 24 m = 158; the union return's regiment row (or-27-1-union-return p.173) gives 168 — the return figure is ADOPTED for the authored decay (333 → 165), the higher figure and consistent with how this corpus treats return-vs-report EC6 pairs elsewhere (ED-52 class)."),
  kf(13500, 3520, 7760, 292, "line", 165,
    "Re-advances 'to the crest of the ridge' with Cutler's released regiments 'for half to three-quarters of an hour' (or-27-1-cutler; or-27-1-dawes-6wi both-sided) — the detachment fights on with Cutler's brigade rather than rejoining Meredith's woods line; no further loss documented."),
  kf(19800, 3600, 7800, 300, "line", 165,
    "Holds with Cutler's brigade at the lull ('no support on either my right or left until 2 o'clock', or-27-1-cutler, carried on this unit by association); Dawes's own report states no clock or position between the crest re-advance and the evening Culp's Hill rendezvous — the detachment's afternoon whereabouts are this file's honest EC3 gap, closed only by the afternoon phase's Culp's Hill convergence with Col. W. W. Robinson (Meredith's own succession pin)."),
];

const sixWisAfternoon: Keyframe[] = [
  kf(0, 3600, 7800, 300, "line", 165, "Continuity from the morning phase's 19800 state."),
  kf(6300, 3850, 8180, 340, "line", 165,
    "Presumed still fighting with Cutler's detachment through the flank-volley phase (Iverson's line, or-27-1-cutler) — no independent report clock exists for the 6th Wis specifically in this window; position slot-follows Cutler's own citation. STRENGTH HONESTY: no further loss documented — held flat, not invented.",
    "inferred"),
  kf(12600, 4300, 7420, 130, "column", 165,
    "THE EMBANKMENT RETREAT: 'the men marched with perfect steadiness and no excitement' (or-27-1-cutler, the formation-class primary) — the detachment retires with Cutler's column along the railroad embankment; Dawes's own town-retreat narrative: 'faced by the rear rank', 'the streets were crowded with retiring troops, batteries, and ambulance trains', 'hearty cheers for the old Sixth' (or-27-1-dawes-6wi)."),
  kf(15000, 5300, 6100, 20, "column", 165,
    "Through town, separating from Cutler's column to rejoin its own brigade — Dawes's report narrates the through-town retreat directly, without naming a shared column after the embankment; the separation point is inferred, not stated.",
    "inferred"),
  kf(18000, 5620, 5680, 10, "line", 165,
    "TO CULP'S HILL: 'reporting to Col. W. W. Robinson, now commanding the brigade' (or-27-1-dawes-6wi verbatim) — the Meredith succession pin (dossier us-i-1-1-meredith.md EC1); rejoins the Iron Brigade's west-shoulder line at Meredith's own end coordinate (this file's us-meredith t=18000 keyframe). Decay: return p.173 168 total (adopted, un-averaged against Dawes's own 158/'not less than 160') — no further loss documented after the cut charge."),
];

// Meredith's residual (the other four regiments: 2nd Wis, 7th Wis, 19th Ind,
// 24th Mich), EXACT subtraction of the 6th Wis figures above at every one of
// Meredith's own keyframe times. At t=13500 the pre-existing compilation-
// class total (1,700) does not leave room for the 6th Wis's own now-
// evidenced cut-charge spike without going non-monotonic — the old
// undifferentiated total never separately accounted for a detached reserve
// regiment's rate-class loss. Corrected here, flagged, not silently edited
// (the A2/Davis-decay-correction class): the "other four" hold their OWN
// t=11700 Archer-pocket loss (1,750 − 333 = 1,417) FLAT through 13500 (no
// new documented loss for THEM in that narrow window — their own combat
// resumes only in the woods defense, carried at full absolute-delta weight
// from there on).
const meredithResidualMorning: Keyframe[] = [
  kf(0, 4400, 5100, 340, "column", 1496,
    "March from Marsh Creek: 'moved from camp early... 6 or 7 miles' (or-27-1-morrow-24mi); Reynolds took Wadsworth's division forward in person, the corps staggered 'an hour and a half to two hours' behind (or-27-1-doubleday). Deployment order R-to-L: 2nd Wis, 7th Wis, 19th Ind, 24th Mich (the charging four). Strength: brigade 1,829 compilation MINUS the 6th Wisconsin's now-separately-tracked reserve detachment (333, own citation on us-6wi) = 1,496; 24th Mich PRIMARY 496 (or-27-1-morrow-24mi) is the only regiment-grain figure fetched among these four. Dossier us-i-1-1-meredith.md (T4 full-arc)."),
  kf(8100, 4000, 6500, 320, "column", 1496,
    "Obliquing left across the fields toward the seminary (the corps' approach leg, tier-B physics: ~6 miles from Marsh Creek at column pace ≈ 2.5-3 h from an early start; Morrow's 'about 9 a.m.' runs early vs Wadsworth's 10:00 — both carried, keyframe sits on the receiving cluster's 10:00-10:15)."),
  kf(9900, 3050, 7300, 292, "line", 1496,
    "THE CHARGE (CA-J1M-4b receiving / CA-J1M-5): deployed 'on the double-quick... no order being given or time allowed for loading our guns' (or-27-1-morrow-24mi); 'deployed en echelon without a moment's hesitation, charged with the utmost steadiness and fury' (or-27-1-doubleday). REYNOLDS FELL LAUNCHING THIS DEPLOYMENT — the three-primary pin (ED-66 rider). mon-reynolds-kia (3177.2,7265.8)."),
  kf(11700, 2850, 7280, 292, "line", 1417,
    "At the run — the Archer pocket: 'the brigade dashed up and over the hill and down into the ravine, through which flows Willoughby's Run' (or-27-1-morrow-24mi); the capture climax both-sided (or-27-2-shepard's 'some 75... General Archer among the rest'). Decay: −79 from the deploy state (the original whole-brigade compilation's own delta, now carried by these four regiments exactly — the 6th Wis was in reserve for this phase, per its own citation, and contributed none of this loss)."),
  kf(13500, 3000, 7300, 292, "line", 1417,
    "Back to the east bank and into McPherson's woods: 'changed front forward on first battalion, and marched into the woods known as McPherson's woods' — 19th Ind left of 24th Mich, 7th Wis right (or-27-1-morrow-24mi); Doubleday's hold order: 'must be held at all hazards.' No further loss documented for these four regiments in this narrow window (their own woods-defense casualties accumulate in the afternoon phase); STRENGTH CORRECTION FLAGGED: the old undifferentiated compilation showed the whole brigade at 1,700 here, implicitly assuming the reserve detachment's share held near-full strength through this point — now that the 6th Wisconsin's own cut-charge spike (333→165, or-27-1-dawes-6wi) is separately evidenced, the combined total this instant is properly 1,417 + 165 = 1,582, not the old 1,700; the 118-man difference is the previously-uncounted slice of the 6th Wisconsin's own documented rate-class loss (a decomposition-wave evidence-layer correction, ED-32/ED-46 class, recorded in the wave report)."),
  kf(19800, 3020, 7300, 292, "line", 1417,
    "The woods line held through the lull (skirmishers 'at once engaged'); the afternoon defense against Pettigrew's brigade is the next phase's record."),
];

const meredithResidualAfternoon: Keyframe[] = [
  kf(0, 3020, 7300, 292, "line", 1417, "Continuity: McPherson's woods line (Doubleday's 'must be held at all hazards')."),
  kf(5400, 3000, 7290, 285, "line", 1217,
    "The woods defense against Pettigrew's renewal: 'We had inflicted severe loss on the enemy, but their numbers were so overpowering...' (or-27-1-morrow-24mi) — the 24th Mich vs 26th NC front; the 24th's flag carried by NINE men, four color-bearers k. Decay: −200, the original combined delta carried exactly (this loss belongs to the woods-fighting four; the 6th Wis is elsewhere with Cutler's detachment, own citation)."),
  kf(9900, 3300, 7290, 285, "line", 817,
    "'Forced back, step by step, contesting every foot of ground, to the barricade' west of the seminary (or-27-1-morrow-24mi); Meredith w — succession to Col. W. W. Robinson pinned in Dawes's report. Decay: −400, the original combined delta."),
  kf(11700, 3700, 7200, 285, "line", 617,
    "THE SEMINARY BARRICADE STAND with Biddle/Stone remnants and Stewart's/Cooper's/Stevens's guns (or-27-1-doubleday; drawn j1-13 'MEREDITH' bar on the seminary line beside Biddle with SCALES/McGOWAN red opposite). Decay: −200, the original combined delta."),
  kf(13500, 4700, 6700, 160, "column", 517,
    "Through the town ('orders... to fall back, given, I believe, by Major-General Doubleday'; Morrow w scalp & c in town). Decay: −100, the original combined delta."),
  kf(16200, 5620, 5680, 10, "line", 437,
    "CULP'S HILL: the brigade to the hill's west shoulder (Morrow: 'occupied Culp's Hill'; Dawes 'in open field on Culp's Hill' reporting to Col. Robinson 'now commanding the brigade') — the consolidation's I Corps right; the 6th Wisconsin's own detachment rejoins here (see us-6wi t=18000). Decay: −80, the original combined delta."),
  kf(18000, 5620, 5680, 10, "line", 417,
    "The Culp's Hill line at 18:00. Decay: −20, the original combined delta. TOTAL DAY LOSS (these four regiments): 1,829 nominal share minus the 6th Wisconsin's 333 = 1,496 start, 417 end = 1,079 lost; PLUS the 6th Wisconsin's own separately-tracked 168 = 1,247 combined — corrected UP from the pre-decomposition build's undifferentiated 1,129 (the 118-man correction flagged at t=13500, carried through unchanged from there). The return p.173 brigade total (1,153) remains the single-track EC6 anchor for the WHOLE original compilation; this decomposition's corrected 1,247 is a wave-report residual, not a silent override of that return (both recorded)."),
];

// ===========================================================================
// 2. THE 147TH NEW YORK (parentless sibling of us-cutler), morning only
//    (carried flat through the afternoon file with no further documented
//    action distinct from the brigade).
// ===========================================================================
//
// Sources: or-27-1-cutler (the regiment primary 380; the rate-class loss
// 207/380 in half an hour; Maj. Harney's stand order; the drawn j1-03
// "147 N.Y" bar (3170,7528), already cited on us-cutler's own t=9000
// keyframe); reconstruction/dossiers/us-i-1-2-cutler.md.

const nyMorning: Keyframe[] = [
  kf(0, 4450, 5150, 340, "column", 380,
    "'Moved from camp early... being the leading brigade of the corps' (or-27-1-cutler); with the brigade column. STRENGTH: regiment primary 380 (or-27-1-cutler, page-verified) — one of only three regiment-grain figures in this brigade (147th NY 380, 76th NY 375, 56th Pa 252; the 95th NY / 14th Brooklyn / 7th Ind stay open at brigade grain). Dossier us-i-1-2-cutler.md (T4 full-arc)."),
  kf(9000, 3170, 7528, 292, "line", 380,
    "THE OPENING LINE (CA-J1M-4 Union right): across the railroad north of the pike — 'in action from 10 a.m. until 4 p.m.' (or-27-1-cutler's own bracket). Drawn: j1-03 '147 N.Y' bar, THIS regiment's own labeled position (3170,7528) — the sharpest single-regiment geometry in the morning file."),
  kf(11400, 3170, 7528, 292, "line", 173,
    "THE STRANDED STAND: Lt. Col. Miller w 'at the moment of receiving' Wadsworth's fall-back order — the wounding that CAUSED the stranding — Maj. Harney 'held the regiment to its position until the enemy were in possession of the railroad cut on his left' (or-27-1-cutler verbatim); position UNCHANGED — the whole point of the stand is that the regiment does NOT fall back with the rest of the brigade. RATE-CLASS PRIMARY, exact: '2 officers killed and 10 wounded, 42 men killed and 153 wounded — 207 out of 380 men and officers within half an hour' (or-27-1-cutler) — 380 − 207 = 173."),
  kf(12600, 3450, 7690, 292, "line", 173,
    "Released by the railroad-cut charge (CA-J1M-6, us-6wi/j1m-rrcut-charge); falls back to reunite with the brigade — position matches Cutler's own t=12600 coordinate exactly. No further per-regiment loss documented in the re-advance 'to the crest of the ridge... half to three-quarters of an hour' (or-27-1-cutler)."),
  kf(19800, 3600, 7800, 300, "line", 173,
    "Holds with the brigade at the lull ('no support on either my right or left until 2 o'clock', or-27-1-cutler) — matches Cutler's own t=19800 position exactly."),
];

const nyAfternoon: Keyframe[] = [
  kf(0, 3600, 7800, 300, "line", 173, "Continuity from the morning phase's 19800 state."),
  kf(12600, 4300, 7400, 130, "column", 173,
    "THE EMBANKMENT RETREAT with the brigade: 'the men marched with perfect steadiness and no excitement' (or-27-1-cutler) — matches Cutler's own t=12600 coordinate exactly. No further loss documented for this regiment specifically in the afternoon (held flat, not invented)."),
  kf(18000, 5450, 5450, 45, "line", 173,
    "Toward Culp's Hill's right at 18:00 with the brigade — matches Cutler's own t=18000 coordinate exactly."),
];

// Cutler's residual (the other five: 76th NY, 84th NY/14th Brooklyn, 95th
// NY, 7th Ind, 56th Pa), EXACT subtraction of the 147th NY figures above at
// every one of Cutler's own keyframe times. Reconciles cleanly: the
// original's own −450 delta (1,600→1,150, t=9000→11400) splits into the
// 147th's own −207 (evidenced) and the residual's −243 — closely matching
// the OTHER regiment primaries in the SAME report ('76th NY... lost 169
// within thirty minutes', '56th Pa... lost 78+') = 247, a 4-man agreement
// with the arithmetic residual, recorded in the wave report as a strength-
// conservation proof point.
const cutlerResidualMorning: Keyframe[] = [
  kf(0, 4450, 5150, 340, "column", 1220,
    "'Moved from camp early on the 1st instant (being the leading brigade of the corps)' (or-27-1-cutler). STRENGTH HONESTY: brigade ~1,600 compilation MINUS the 147th New York's own regiment primary (380, or-27-1-cutler) = 1,220; the 95th NY / 14th Brooklyn / 7th Ind stay open at brigade grain; the 7th Ind detached 'on duty in the rear' July 1 (rejoined at evening — EC2 detachment primary). Dossier us-i-1-2-cutler.md (T4 full-arc)."),
  kf(9000, 3250, 7550, 292, "line", 1220,
    "THE OPENING LINE (CA-J1M-4 Union right): across the railroad north of the pike with 76th NY / 147th NY / 56th Pa — 'in action from 10 a.m. until 4 p.m.' (his own bracket); 95th NY + 14th Brooklyn detached left BY GENERAL REYNOLDS's order to support Hall — Reynolds's last tactical act in a brigade primary (or-27-1-cutler)."),
  kf(11400, 3500, 7700, 292, "line", 977,
    "The right wing's wreck and Wadsworth's fall-back order 'to the woods on the next ridge'; the 147th NY's own stand (its own citation, us-147ny) is what let the rest of the line disengage. RATE-CLASS PRIMARIES for the other regiments in the SAME window: '76th NY... lost 169 within thirty minutes' (Maj. Grover k); '56th Pa... lost 78+ at that point' (or-27-1-cutler) — 169+78 = 247, closely matching this residual's own −243 delta (1,220→977), a strength-conservation cross-check."),
  kf(12600, 3450, 7690, 292, "line", 927,
    "The railroad-cut resolution (CA-J1M-6): the 6th Wis / 95th NY / 14th Brooklyn charge releases the 147th; re-advance to 'the crest of the ridge' for 'half to three-quarters of an hour' (or-27-1-cutler; or-27-1-dawes-6wi both-sided)."),
  kf(19800, 3600, 7800, 300, "line", 927,
    "On the ridge north of the pike at the lull, both flanks open — 'having no support on either my right or left until 2 o'clock' (or-27-1-cutler; the 2 o'clock Baxter/XI sync is the afternoon phase's clock)."),
];

const cutlerResidualAfternoon: Keyframe[] = [
  kf(0, 3600, 7800, 300, "line", 927, "Continuity: the ridge north of the pike, both flanks open (or-27-1-cutler)."),
  kf(3600, 3750, 8050, 340, "line", 927,
    "THE 2 O'CLOCK SYNC: 'having no support on either my right or left until 2 o'clock, when a brigade from the Second Division formed on my right, and the Eleventh Corps came in on the right of them' (or-27-1-cutler) — the Baxter/XI junction clocked from the receiving line (CA-J1P-1's receiving preamble)."),
  kf(6300, 3850, 8200, 340, "line", 907,
    "THE FLANK VOLLEY on Iverson's line: 'came in on their flank, and opened so hot a fire on them that one regiment threw down their arms and surrendered' (or-27-1-cutler) — the Iverson capture mass from the Union left (the surrendered regiment unnamed — carried, not identified)."),
  kf(9900, 3700, 7700, 320, "line", 877,
    "Ammunition exhaustion and relief; the twenty-minute wait at the railroad; the three-regiment detachment to the seminary (or-27-1-cutler)."),
  kf(12600, 4300, 7400, 130, "column", 827,
    "THE EMBANKMENT RETREAT: 'I moved off on the railroad embankment, and, although exposed to the enemy's fire on both flanks, the men marched with perfect steadiness and no excitement. Their steadiness had the effect to bring the enemy to a halt' (or-27-1-cutler — the formation-class primary)."),
  kf(14400, 5150, 5750, 45, "line", 787,
    "Through town to Cemetery Hill, then 'to hold the crest of a hill to the right' — the 7th Ind sent by Wadsworth: THE CULP'S HILL FIRST-OCCUPATION PIN (or-27-1-cutler; both-sided with the Greene frame)."),
  kf(18000, 5450, 5450, 45, "line", 777,
    "Toward Culp's Hill's right at 18:00 (the 7th Ind — fresh, detached all day — rejoins at evening). Decay: the whole brigade's return total (1,002 — the source's own figure, carried as-is on this residual pending a full per-regiment reconciliation), the opening half-hour dominant (the rate-class primaries); the 147th New York's own 207-loss is now carried separately (us-147ny), the residual figure here is this brigade's OTHER five regiments only."),
];

// ===========================================================================
// 3. THE 16TH MAINE (parentless sibling of us-coulter/Paul's Brigade)
// ===========================================================================
//
// Sources: reconstruction/dossiers/us-i-2-1-paul.md (EC2/EC3.3/EC5.2/EC6);
// or-27-1-farnham-16me (the sacrifice order verbatim, the three-stage
// collapse, the July 1 loss table 223 = the missing column's capture
// pocket).

const meMorning: Keyframe[] = [
  kf(0, 4500, 5200, 340, "column", 275,
    "Robinson's division in the corps column's rear, with the brigade (or-27-1-robinson). STRENGTH HONESTY: no regiment or brigade primary exists (dossier us-i-2-1-paul.md EC2: 'no primary located this pass'); 275 is the dossier's own literature-class figure, explicitly NOT hopped to a primary — the most heavily flagged strength row this wave authors. The regiment's only hard EC6 number is its casualty total (223, or-27-1-farnham-16me), which bounds the true engaged strength from below."),
  kf(14400, 3730, 7220, 292, "line", 275,
    "Reserve near the seminary: 'the First Brigade, under Brigadier-General Paul, was set at work to intrench the ridge' (or-27-1-robinson) — the seminary rail-barricade the CSA final push later stormed. With the brigade, not yet detached."),
  kf(19800, 3730, 7220, 292, "line", 275,
    "At the barricade reserve at the seam; not yet engaged (the ~15:00 Oak Ridge relief insertion and the hill detachment are the afternoon phase's record)."),
];

const meAfternoon: Keyframe[] = [
  kf(0, 3730, 7220, 292, "line", 275, "Continuity from the morning phase's 19800 state."),
  kf(7200, 4000, 8380, 340, "line", 275,
    "THE RELIEF INSERTION with the brigade: 'until after 3 p.m., at which time, the ammunition being exhausted, we were relieved by a portion of the First Brigade' (or-27-1-coulter — the Paul-for-Baxter relief clocked at the incoming echelon); brought up by Robinson in person — matches Coulter's own t=7200 coordinate exactly."),
  kf(10800, 3980, 8320, 30, "line", 275,
    "THE SACRIFICE ORDER: 'ordered, alone, by General Robinson, to take possession of a hill which commanded the road, and hold the same as long as there was a man left' (or-27-1-farnham-16me verbatim) — the detachment from the brigade begins here. POSITION HONESTY: no drawn geometry or named landmark exists for 'a hill which commanded the road' in Farnham's report; placed near the division's own retreat-corridor ground (Coulter's own t=10800 coordinate), radius wide-open — a genuine EC3 gap (dossier us-i-2-1-paul.md EC3.3, [D])."),
  kf(12600, 3980, 8320, 30, "scattered", 52,
    "THE THREE-STAGE COLLAPSE: 'ordered a retreat, but not in time to reach the main body' — the fall-backs hollow → woods → the failed retreat (or-27-1-farnham-16me verbatim). LOSS PRIMARY, exact: July 1 1+8 k / 5+47 w / 11+151 m = 223 (or-27-1-farnham-16me) — the missing column (162) IS the sacrifice hill's capture pocket (dossier EC6). Strength 275 − 223 = 52 (compounded-uncertainty flag: 275 is itself not a primary; 223 is; the residual carries both flags forward, not smoothed)."),
  kf(18000, 5000, 5700, 20, "line", 52,
    "The remnant reaches Cemetery Hill with the division at day's end (or-27-1-coulter; or-27-1-farnham-16me's own July 2 record picks up from Cemetery Hill battery support). No further loss documented (held flat, not invented)."),
];

// Coulter's/Paul's residual (13th Mass, 94th NY, 104th NY, 107th Pa), EXACT
// subtraction of the 16th Maine figures above at every one of Coulter's own
// keyframe times. TOTAL DAY LOSS CROSS-CHECK: the pre-existing build's own
// t=0/t=18000 pair (1,537 → 761) already totals exactly 776 — the brigade's
// OWN day-split primary (or-27-1-coulter's table: July 1 776). This
// decomposition preserves that total exactly BY CONSTRUCTION: the 16th
// Maine's own loss (275−52=223, its own primary) plus the residual's own
// loss (1,262−709=553) sum to 776 — the wave's cleanest strength-
// conservation proof (recorded in the wave report).
const coulterResidualMorning: Keyframe[] = [
  kf(0, 4500, 5200, 340, "column", 1262,
    "Robinson's division in the corps column's rear (or-27-1-robinson). Strength: B&M-repro 1,537 (hop flagged) MINUS the 16th Maine's own separately-tracked figure (275, own citation, us-16me) = 1,262; Robinson's division frame 'We went into action with less than 2,500 men' CONFLICTS with the two-brigade B&M sum — both carried un-averaged (dossier EC2). Dossier us-i-2-1-paul.md (T3, July 1 grain)."),
  kf(14400, 3730, 7220, 292, "line", 1262,
    "Reserve near the seminary: 'the First Brigade, under Brigadier-General Paul, was set at work to intrench the ridge' (or-27-1-robinson) — THE SEMINARY RAIL-BARRICADE the CSA final push later stormed (Perrin: 'a breastwork of rails behind [which] the enemy was posted' — both-sided across four hours, or-27-2-perrin)."),
  kf(19800, 3730, 7220, 292, "line", 1262,
    "At the barricade reserve at the seam (the ~15:00 Oak Ridge relief insertion — Coulter's clock — is the afternoon phase's record)."),
];

const coulterResidualAfternoon: Keyframe[] = [
  kf(0, 3730, 7220, 292, "line", 1262, "Continuity: the seminary barricade reserve (or-27-1-robinson; B&M 1,537 hop, minus the 16th Maine's own 275)."),
  kf(7200, 4000, 8380, 340, "line", 1262,
    "THE RELIEF INSERTION: 'until after 3 p.m., at which time, the ammunition being exhausted, we were relieved by a portion of the First Brigade' (or-27-1-coulter — the Paul-for-Baxter relief clocked at the incoming echelon); brought up by Robinson in person; 'part of it in the position first occupied by Baxter's brigade' (or-27-1-robinson)."),
  kf(10800, 3950, 8300, 340, "line", 925,
    "The relieved line's fight: 'The enemy now made repeated attacks on the division, in all of which he was handsomely repulsed' (or-27-1-robinson); PAUL FELL severely wounded — the FIVE-COMMANDER CASCADE (Paul → Leonard w → Root w → Coulter). The 16th Maine is detached to the hill THIS instant (own citation, us-16me) — its loss is not yet booked; this residual's own −337 delta (1,262→925) is the OTHER four regiments' share of the relieved line's fight."),
  kf(13500, 4750, 6900, 160, "column", 848,
    "The town retreat under the 16th Maine's cover (its own three-stage collapse is us-16me's citation; the regiment's sacrifice bought this residual's own retreat); Robinson's division the ordered rear guard, 'until nearly 5 p.m.' (the ED-70 recorded back-edge tail). Decay: −77 (925→848), these four regiments' own retreat losses, distinct from the 16th Maine's capture-pocket spike."),
  kf(16200, 5000, 5700, 20, "line", 728,
    "Cemetery Hill; Coulter assigned to the brigade command 'taking his regiment with him' (or-27-1-baxter = or-27-1-coulter, both echelons)."),
  kf(18000, 5000, 5700, 20, "line", 709,
    "The hill line at 18:00. Decay: THE DAY-SPLIT PRIMARY — July 1 776 · July 2 28 · July 3 14 · July 4 3 = 821 (or-27-1-coulter's table, the only per-day brigade table in the division's records). STRENGTH-CONSERVATION CHECK: this residual's own day loss (1,262 − 709 = 553) plus the 16th Maine's own day loss (275 − 52 = 223, its own primary) = 776 — EXACTLY the brigade's day-split table total, preserved by construction (the wave report's cleanest reconciliation)."),
];

// ---------------------------------------------------------------------------
// Apply: patch the residual brigades, insert the three new parentless units,
// add the unit-scoped activity events, validate, export.
// ---------------------------------------------------------------------------

patchUnit(morning, "us-meredith", (u) => {
  u.frontage_m = 404;
  u.keyframes = meredithResidualMorning;
});
patchUnit(afternoon, "us-meredith", (u) => {
  u.frontage_m = 383;
  u.keyframes = meredithResidualAfternoon;
});
patchUnit(morning, "us-cutler", (u) => {
  u.frontage_m = 329;
  u.keyframes = cutlerResidualMorning;
});
patchUnit(afternoon, "us-cutler", (u) => {
  u.frontage_m = 250;
  u.keyframes = cutlerResidualAfternoon;
});
patchUnit(morning, "us-coulter", (u) => {
  u.name = "Paul's Brigade (1st Bde, 2nd Div, I Corps — minus the 16th Maine)";
  u.frontage_m = 341;
  u.keyframes = coulterResidualMorning;
});
patchUnit(afternoon, "us-coulter", (u) => {
  u.name = "Paul's Brigade (1st Bde, 2nd Div, I Corps — minus the 16th Maine)";
  u.frontage_m = 341;
  u.keyframes = coulterResidualAfternoon;
});
// Meredith's and Cutler's names carry the same "minus" convention (Carroll's
// Brigade precedent, gettysburg-july3.json us-carroll).
patchUnit(morning, "us-meredith", (u) => {
  u.name = "Meredith's Iron Brigade (1st Bde, 1st Div, I Corps — minus the 6th Wisconsin)";
});
patchUnit(afternoon, "us-meredith", (u) => {
  u.name = "Meredith's Iron Brigade (1st Bde, 1st Div, I Corps — minus the 6th Wisconsin)";
});
patchUnit(morning, "us-cutler", (u) => {
  u.name = "Cutler's Brigade (2nd Bde, 1st Div, I Corps — minus the 147th New York)";
});
patchUnit(afternoon, "us-cutler", (u) => {
  u.name = "Cutler's Brigade (2nd Bde, 1st Div, I Corps — minus the 147th New York)";
});

const sixWisMorningUnit: Unit = {
  id: "us-6wi", name: "6th Wisconsin (Meredith's)", side: "union",
  frontage_m: 90, depth_m: 20, keyframes: sixWisMorning,
};
const nyMorningUnit: Unit = {
  id: "us-147ny", name: "147th New York (Cutler's)", side: "union",
  frontage_m: 103, depth_m: 20, keyframes: nyMorning,
};
const meMorningUnit: Unit = {
  id: "us-16me", name: "16th Maine (Paul's)", side: "union",
  frontage_m: 74, depth_m: 20, keyframes: meMorning,
};
const sixWisAfternoonUnit: Unit = {
  id: "us-6wi", name: "6th Wisconsin (Meredith's)", side: "union",
  frontage_m: 90, depth_m: 20, keyframes: sixWisAfternoon,
};
const nyAfternoonUnit: Unit = {
  id: "us-147ny", name: "147th New York (Cutler's)", side: "union",
  frontage_m: 103, depth_m: 20, keyframes: nyAfternoon,
};
const meAfternoonUnit: Unit = {
  id: "us-16me", name: "16th Maine (Paul's)", side: "union",
  frontage_m: 74, depth_m: 20, keyframes: meAfternoon,
};

morning = addUnitsAfter(morning, "us-meredith", [sixWisMorningUnit]);
morning = addUnitsAfter(morning, "us-cutler", [nyMorningUnit]);
morning = addUnitsAfter(morning, "us-coulter", [meMorningUnit]);
afternoon = addUnitsAfter(afternoon, "us-meredith", [sixWisAfternoonUnit]);
afternoon = addUnitsAfter(afternoon, "us-cutler", [nyAfternoonUnit]);
afternoon = addUnitsAfter(afternoon, "us-coulter", [meAfternoonUnit]);

// Unit-scoped activity events (the segment event j1m-rrcut-charge stays
// unchanged — it renders the ground-anchored THREE-regiment convergence,
// 6th Wis + 95th NY + 14th Brooklyn, none of the latter two separately
// tracked this wave); these events give the newly-tracked regiments their
// own activity-salience windows without re-authoring the convergence.
const newMorningEvents: EngagementEvent[] = [
  {
    id: "j1m-6wi-cut-charge", kind: "musketry", t0: 11700, t1: 12900,
    unitId: "us-6wi", confidence: "documented",
    citation: "The charge and volley fire (or-27-1-dawes-6wi verbatim: 'upon a double-quick, well closed, in face of a terribly destructive fire'); the ground-anchored segment j1m-rrcut-charge renders the three-regiment convergence — this event carries the 6th Wisconsin's own unit-scoped activity flag now that it is separately tracked.",
  },
  {
    id: "j1m-147ny-stand", kind: "musketry", t0: 9000, t1: 11400,
    unitId: "us-147ny", confidence: "documented",
    citation: "The stranded stand: Maj. Harney 'held the regiment to its position until the enemy were in possession of the railroad cut on his left' (or-27-1-cutler verbatim); RATE-CLASS: '207 out of 380 men and officers within half an hour.'",
  },
];
const newAfternoonEvents: EngagementEvent[] = [
  {
    id: "j1p-16me-hillstand", kind: "musketry", t0: 10800, t1: 12600,
    unitId: "us-16me", confidence: "documented",
    citation: "THE SACRIFICE: 'ordered, alone, by General Robinson, to take possession of a hill which commanded the road, and hold the same as long as there was a man left' (or-27-1-farnham-16me verbatim); the three-stage collapse (hollow → woods → 'not in time to reach the main body').",
  },
];

morning = { ...morning, events: [...(morning.events ?? []), ...newMorningEvents] };
afternoon = { ...afternoon, events: [...(afternoon.events ?? []), ...newAfternoonEvents] };

// Cross-phase continuity audit (printed, not silently assumed): the three
// new units' afternoon t=0 must equal their own morning file's final
// keyframe exactly (the dayexp3-afternoon.ts convention, applied by hand
// here since this is a patch script, not a full-file constructor).
for (const id of ["us-6wi", "us-147ny", "us-16me"]) {
  const m = morning.units.find((u) => u.id === id)!;
  const a = afternoon.units.find((u) => u.id === id)!;
  const mEnd = m.keyframes[m.keyframes.length - 1]!;
  const aStart = a.keyframes[0]!;
  for (const f of ["x", "z", "facing", "strength"] as const)
    if (mEnd[f] !== aStart[f])
      throw new Error(`continuity break on ${id}.${f}: afternoon t=0 ${aStart[f]} vs morning end ${mEnd[f]}`);
}
console.log("continuity: us-6wi, us-147ny, us-16me all carry exactly from morning end to afternoon t=0");

writeFileSync(morningPath, exportValidated(morning));
writeFileSync(afternoonPath, exportValidated(afternoon));
console.log(`wrote ${morningPath}: ${morning.units.length} units, ${morning.events!.length} events`);
console.log(`wrote ${afternoonPath}: ${afternoon.units.length} units, ${afternoon.events!.length} events`);
