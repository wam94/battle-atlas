// Day expansion slice 3 — THE JULY 1 MORNING PHASE (Buford's stand, the
// meeting engagement, the railroad cut, the midday lull; 07:30–13:00 LMT).
// Run from tool/:
//   npx vite-node scripts/author-dayexp3-morning.ts
// Committed as the derivation record (the author-w*/A1/A2/dayexp1/dayexp2
// pattern). Slice report: docs/reconstruction/audit/day-expansion-slice-3.md.
//
// THE PHASE (ADR 0005: one battle file = one phase = one clock):
//   startTime 27000 (07:30 LMT — CA-J1M-1, the First Shot marker's adopted
//   hour) · endTime 19800 (ends 13:00 LMT — the midday-lull seam; the
//   afternoon phase abuts at 46800). The manifest's july1-morning phase
//   ECHOES this clock (test-enforced).
//
// THE SEAM (the design call recorded in the slice report): the morning
// chain (ED-26) ends at the lull (CA-J1M-7 onset ~11:30); the afternoon
// chain's interior ladder (ED-69: Coulter 12:30 general firing → Doles
// ~13:00 formation → Stone/Dobke 13:30 → tablet 14:00 adopted) begins
// around 13:00. 13:00 sits between the chains: Rodes's and Pender's
// formations are this file's closing states, and the afternoon file
// re-expresses them at t=0 (cross-phase continuity, verified numerically
// by the afternoon script).
//
// HARD RULES this script observes:
// - The July 2/July 3 files are NEVER READ OR WRITTEN (July 1 begins the
//   battle; there is no earlier state to inherit — every t=0 pose is its
//   own dossier-cited arrival/camp state).
// - NEVER invent motion: movers carry the dossier corpus's July 1 arcs
//   (passes 10-11; ED-26 as amended by ED-66, ED-68(a); ED-69 annotations);
//   tier-B physics legs (the pike/Emmitsburg-road approach marches — the
//   best-documented roads of the battle) author `inferred` with the
//   derivation in the citation; conflicts are CARRIED, never averaged;
//   no report-nominal clock moves a keyframe (ED-25 rule 4).
// - Units NOT on the field 07:30-13:00 are ABSENT, never faked: Early's
//   division and Jones's artillery battalion enter the afternoon file
//   (CA-J1P-3 chain; Early's tablet "arrived about noon within two miles"
//   is vicinity, not the field — ED-27 verification); Barlow's and
//   von Steinwehr's XI Corps divisions and the corps batteries beyond
//   Dilger deploy after 13:00 (Schurz's deployment ladder; von Steinwehr's
//   2 o'clock arrival); every other corps of both armies is off-field.
// - Positions sit in the battlefield-local frame (monuments.json: UTM 18N
//   minus the heightmap origin); drawn/monument reads are cited at their
//   honest radii (±62 m sole-read class; ED-39 semantics per marker).
import { writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe } from "../src/model";
import { fireEvent, moverUnit, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const outPath = join(here, "../../app/Assets/Battle/gettysburg-july1-morning.json");

// Clock: t = seconds from 07:30 LMT (startTime 27000). 13:00 = 19800.
const StartTime = 27000;
const EndT = 19800; // 13:00 LMT — the midday-lull seam (see header)
const T = (h: number, m: number) => h * 3600 + m * 60 - StartTime;

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  confidence: Keyframe["confidence"], citation?: string): Keyframe => ({
    t, x, z, facing, formation, strength,
    ...(confidence !== undefined && { confidence }),
    ...(citation !== undefined && { citation }),
  });

// ---------------------------------------------------------------------------
// MOVERS — the dossiered July 1 morning arcs. July 1 has almost no statics:
// the day IS arrivals and motion (approach marches at tier-B physics; the
// fights at the dossiers' drawn/monument grounds).
// ---------------------------------------------------------------------------
interface M {
  id: string; name: string; side: "union" | "confederate";
  frontage?: number; depth?: number; kfs: Keyframe[];
}

const movers: M[] = [
  // ======================= BUFORD'S DIVISION (CA-J1M-2) ====================
  {
    id: "us-cav-gamble", name: "Gamble's Brigade (1st Bde, 1st Div, Cavalry Corps)",
    side: "union",
    kfs: [
      kf(0, 3718, 7150, 292, "column", 1600, "documented",
        "June 30 night 'in camp at the seminary building' (or-27-1-gamble), division pickets out the western roads (or-27-1-buford); the 07:30 first shot (CA-J1M-1, the 1886 First Shot marker's hour) falls on Jones's picket post ~3 miles out — the pickets ride this citation, not a second track. Strength PRIMARY 'about 1,600 strong' with the battery (or-27-1-gamble; B&M-repro 1,600 EXACT). Dossier us-cav-1-1-gamble.md (T4, July 1 grain)."),
      kf(T(8, 15), 3320, 7280, 292, "skirmish", 1600, "documented",
        "THE DELAY LINE (CA-J1M-2/-4a executor): 'placed in line of battle about 1 mile in front of the seminary, the right resting on the railroad track and the left near the Middletown or Fairfax [Fairfield] road, the Cashtown road being a little to the right of the center, at right angles with the line' (or-27-1-gamble — the line-geometry primary); Gamble 'About 8 o'clock... approaching his pickets... about 3 miles distant' + Buford 'between 8 and 9 a.m.' (ED-66's deploy-phase Union clocks). Drawn: j1-03 '8 Ill Cav' bar (3362,7266); mon-gamble-hq (3264.6,7327.0) reads 115 m from the bar — HQ-on-the-line; mon-buford-hq beside it."),
      kf(T(9, 30), 3390, 7260, 292, "skirmish", 1590, "documented",
        "The one ground yielded before relief: 'compelled to fall back about 200 yards to the next ridge, and there make a stand' (or-27-1-gamble), agreeing with Buford's 'literally dragged back a few hundred yards'; 'After checking and retarding the advance of the enemy several hours' = Buford's 'held its own for more than two hours' — the delay-duration pair (ED-66 basis)."),
      kf(T(10, 20), 3300, 7000, 285, "skirmish", 1570, "documented",
        "Relief hand-off (CA-J1M-4/5 adjacency): 'our infantry advance of the First Corps arrived, and relieved the cavalry brigade' (or-27-1-gamble) — both-sided with Wadsworth/Morrow (the cavalry 'hotly engaged' in the infantry's front as it deployed); the brigade shifts left (south of the Fairfield road front)."),
      kf(EndT, 3550, 6650, 290, "line", 1560, "inferred",
        "Between the morning relief and the afternoon left-flank stand: covering the corps left toward the Fairfield road (position inferred between the relief ground and the j1-13 drawn afternoon bar (3714,6488) — the 4 p.m. stand is the afternoon phase's record). Loss to 13:00 [D]: small share of the campaign return's 99 aggregate (p. 185; no per-day split printed — flagged)."),
    ],
  },
  {
    id: "us-cav-devin", name: "Devin's Brigade (2nd Bde, 1st Div, Cavalry Corps)",
    side: "union",
    kfs: [
      kf(0, 4400, 7700, 0, "column", 1100, "inferred",
        "June 30 'marched by Emmitsburg to Gettysburg, Pa., and encamped' (or-27-1-devin) — camp north of town toward the right-front roads. STRENGTH HONESTY: no primary exists (dossier EC2 open); ~1,100 sabers is compilation-class, carried FLAGGED. Dossier us-cav-1-2-devin.md (T3, July 1 grain)."),
      kf(T(9, 0), 4200, 8300, 10, "skirmish", 1100, "documented",
        "THE FOUR-ROAD SCREEN (CA-J1M-2 right wing, geometry primary): formed 'on the crest of the hill on the right of the First Brigade... my right resting on the road to Mummasburg', vedettes to the pickets 'on the three roads on the right leading toward Carlisle... a continuous line from the York road, on the extreme right, to the left of the First Brigade, on the Cashtown road' (or-27-1-devin) — Buford's 'four roads' made explicit. mon-devin-hq (3574.4,8446.8) pins the ridge front; the screen's centroid sits east of it along the arc (±150 m class)."),
      kf(T(11, 30), 4700, 8300, 30, "skirmish", 1090, "documented",
        "The Heidlersburg front hold: the 9th NY + dismounted line against 'the advance of the enemy's line of battle, coming from the direction of Heidlersburg' (or-27-1-devin — Rodes's approach axis); 'succeeded in holding the rebel line in check for two hours, until relieved by the arrival of the Eleventh Corps' (duration-class primary). NO clock anywhere in his Gettysburg narrative (kind-none) — timing rides Buford and the XI ladder."),
      kf(EndT, 5200, 8100, 30, "skirmish", 1085, "documented",
        "Withdrawing right as the XI Corps relief begins (CA-J1M-7 adjacency: his front's relief IS the lull's right-flank boundary condition): 'This I effected in successive formations in line to the rear by regiment, in the face of the enemy' — the formation-class withdrawal drill primary; massing toward the York road post (the afternoon file's t=0)."),
    ],
  },
  {
    id: "us-btty-calef", name: "Calef's Battery A, 2nd US Artillery",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 3136, 7626, 292, "line", 120, "documented",
        "'Arrived in the evening from Emmitsburg and took position on the Chambersburg Pike' (tablet verbatim); July 1 'advanced with the Cavalry... right section on right of the road, left section on the left, and center section with Colonel William Gamble's Brigade on the right of Fairfield Road' (tablet) — the split three-section deployment; centroid at the pike-straddle marker mon-calef-a2us (3136.3,7626.2). 'THE FIRST UNION GUN OF THE BATTLE WAS FIRED FROM THE RIGHT SECTION' (tablet verbatim). Guns: six 3-inch rifles (tablet + or-27-1-gamble). Personnel: NOT stated in any fetched primary (dossier EC2 explicitly open) — ~120 battery-class figure carried FLAGGED. Dossier us-btty-calef.md (T3)."),
      kf(T(10, 45), 3718, 7100, 292, "column", 118, "documented",
        "Retired on the I Corps relief (Hall's 2nd ME took the pike position; Tidball's report commends 'Calef and Roder' — the parent record); Buford's twelve-gun concentric-fire passage ('Calef held his own gloriously') rides the division record."),
      kf(EndT, 3550, 6680, 290, "column", 118, "documented",
        "With Gamble's brigade toward the left (the brigade's attached battery; the afternoon stand is the next phase's record). Loss (tablet): 12 men wounded, 13 horses killed — the day's cost, spread across both stands."),
    ],
  },
  // ======================= HETH'S DIVISION (CA-J1M-4a/4b) ==================
  {
    id: "csa-fry", name: "Archer's Brigade, Heth's Division",
    side: "confederate",
    kfs: [
      kf(0, 1700, 8100, 112, "column", 1048, "documented",
        "'We left our camp near Cashtown, Pa., early on the morning of July 1, and marched down the turnpike road leading to Gettysburg... came upon the enemy's pickets, who gradually fell back before us for about 3 miles' (or-27-2-shepard — the deploy-phase CSA ground truth; the 'pickets' were Gamble's squadrons). Strength PRIMARY: 'We went into the fight on the 1st with 1,048 men' (or-27-2-shepard). Dossier csa-3c-het-3-fry.md (T4 full-arc)."),
      kf(T(9, 0), 2300, 7700, 112, "line", 1048, "documented",
        "CA-J1M-4a — HETH DEPLOYS (~08:00-09:00, envelope 07:30-09:30, ED-66): the leading brigade files into line south of the pike on Herr Ridge as Pegram's guns open; Heth's own 'at this time — 9 o'clock... I was ignorant what force was at or near Gettysburg' is the deploy-phase CSA clock AND time-silent on the attack (the conflation his report invited is the reason for the ED-66 split)."),
      kf(T(10, 15), 2700, 7200, 112, "line", 1030, "documented",
        "CA-J1M-4b — THE ATTACK (~10:15-10:30, envelope 10:00-10:45, ED-66): the advance to Willoughby Run; 'At the extreme side of the field there was a small creek with a fence and undergrowth... but the brigade rushed across with a cheer' (or-27-2-shepard). Drawn: j1-03 red ARCHER BRIG bars with 13 Ala / 5 Ala Bn / 1st Tenn labels W of the run (label read 2700,7086). The clock is Davis's 10.30 + the Union receiving cluster (Shepard time-silent — kind-none profile)."),
      kf(T(10, 45), 2760, 7220, 112, "line", 950, "documented",
        "THE CREEK POCKET: the Iron Brigade's unloaded counter-charge strikes the right — 'some 75 of the brigade were unable to make their escape, General Archer among the rest' (or-27-2-shepard) = Morrow's 'captured a large number of prisoners, being a part of General Archer's brigade' + Archer brought in by Pvt. Patrick Maloney, 2nd Wis (or-27-1-doubleday) — both-sided at man grain; ARCHER CAPTURED = the first Lee-army general lost. mon-archer-captured (2731.1,7504.7) sits past the run on the charge axis. The capture-count conflict (Doubleday 'nearly 1,000' vs Shepard 'some 75') is CARRIED, not resolved (dossier conflict 1)."),
      kf(T(11, 15), 2500, 7250, 292, "scattered", 800, "documented",
        "Repulsed back over the run ('hurled the enemy back into the run', or-27-1-doubleday); the brigade re-forms west of the creek under Fry."),
      kf(EndT, 2450, 7250, 112, "line", 780, "documented",
        "Re-formed west of Willoughby Run at the lull; the brigade sat out the afternoon renewal at brigade weight (Pettigrew/Brockenbrough carried it — us-i-3 receiving records). Loss to the lull [D]: ~75 captured + the morning k&w share of the campaign 677 (return p. 344; no per-day split — bounded, flagged)."),
    ],
  },
  {
    id: "csa-davis", name: "Davis's Brigade, Heth's Division",
    side: "confederate",
    kfs: [
      kf(0, 1750, 8250, 112, "column", 2000, "documented",
        "From 'camp on the heights near Cashtown' down the pike behind Archer; artillery in position 'within about 2 miles from town' (or-27-2-davis-jr). Strength: 'present on the first day about 2,000' (stone-sentinels, July-1 scope) — THREE regiments engaged (11th Miss 'left as a guard for the division wagon train', the detachment primary); no morning report (open, flagged). Dossier csa-3c-het-4-davis.md (T4 full-arc)."),
      kf(T(9, 0), 2350, 7900, 112, "line", 2000, "documented",
        "CA-J1M-4a: deployed north of the pike (42d Miss right — 2d Miss center — 55th NC left, or-27-2-davis-jr) as the artillery phase opens."),
      kf(T(10, 20), 2900, 7800, 112, "line", 1980, "documented",
        "CA-J1M-4b — THE ATTACK-POLE EXECUTOR CLOCK: 'About 10.30 o'clock a line of battle was formed... skirmishers thrown forward, and the brigade moved forward to the attack' (or-27-2-davis-jr No. 553 p. 649 verbatim — the only stated CSA attack clock, ED-66's keystone), agreeing across the lines with Wadsworth 10:00 / the Reynolds 10:15 pin / Cutler 'in action from 10 a.m.'"),
      kf(T(10, 50), 3350, 7720, 112, "line", 1850, "documented",
        "Drives Cutler's three-regiment wing ('engaged with a vastly superior force... in front and on my right flank', or-27-1-cutler receiving) and wheels toward the railroad grading."),
      kf(T(11, 10), 3480, 7715, 112, "scattered", 1500, "documented",
        "THE RAILROAD CUT (CA-J1M-6): 'rallied near the railroad, where he again made a stand, and, after desperate fighting, with heavy loss on both sides' (or-27-2-davis-jr) — then the trap: the 6th Wis + 95th NY + 14th Brooklyn fence charge, Maj. Blair's 2nd Miss surrender to Dawes, 7 officers + ~225 men taken, the 2nd Miss flag (Waller, MoH), the 2nd ME gun recaptured (or-27-1-dawes-6wi; both-sided with the 147th NY's release, or-27-1-cutler). mon-davis-hq-rrcut (3454.4,7700.5). 'Of 9 field officers present, but 2 escaped unhurt.'"),
      kf(T(11, 40), 2900, 7800, 292, "scattered", 1300, "documented",
        "Extracted westward in disorder ('he fled in great disorder' is his own description of the earlier Union rout — the brigade's own withdrawal is 'This was about 1 p.m.'-bounded); re-rallying near the ridge."),
      kf(EndT, 2600, 7900, 112, "line", 1250, "documented",
        "Withdrawn: 'This was about 1 p.m.' (or-27-2-davis-jr) — the brigade's retirement clock, the morning file's closing CSA state (the skeleton's ~11:00 cut-conclusion vs Davis's 1 p.m. tension is RECORDED in the pass-10 report §5: his clock is the brigade's retirement, not the cut's fall). The ~15:00 re-advance is the afternoon phase's record."),
    ],
  },
  {
    id: "csa-marshall", name: "Pettigrew's Brigade, Heth's Division",
    side: "confederate",
    kfs: [
      kf(0, 1600, 8300, 112, "column", 2000, "documented",
        "Heth's second line on the pike behind Archer/Davis (division frame, or-27-2-heth via davis-jr order of battle). Strength: 'present on the first day about 2,000' (stone-sentinels, the July-1-scope caveat carried; ED-75's July 3 arithmetic runs FROM this figure). Dossier csa-3c-het-1-marshall.md (T4 full-arc)."),
      kf(T(9, 30), 2150, 7500, 112, "line", 2000, "inferred",
        "Second line deployed south of the pike behind Archer (division frame; trigger-seamed on the 4a deploy — no brigade clock exists)."),
      kf(EndT, 2400, 7300, 112, "line", 2000, "documented",
        "Advanced to the attack position east of Herr Ridge at the lull's end: the brigade's July 1 attack 'fell in Heth's AFTERNOON renewal (the division's second effort with Pender behind — after Davis's 1 p.m. retirement clock, before the 3.45-4 p.m. Union retirement)' (dossier EC3.1, [D] bracketed, no CSA clock) — the fight is the afternoon file's record."),
    ],
  },
  {
    id: "csa-brockenbrough", name: "Brockenbrough's Brigade, Heth's Division",
    side: "confederate",
    kfs: [
      kf(0, 1650, 8400, 112, "column", 880, "documented",
        "With Heth's division on the pike behind the Archer/Davis first line (division frame, or-27-2-heth; the dossier's honestly-thin July 1 — NO brigade-authored statement exists, the no-report negative governs). Strength: Mayo-hop ~880 within the 800-1,100 surviving range (ED-48: the Stone Sentinels page is a confirmed duplication artifact, DO-NOT-USE). Dossier csa-3c-het-2-brockenbrough.md (T3)."),
      kf(T(9, 30), 2200, 7650, 112, "line", 880, "inferred",
        "Second line astride/north of Pettigrew (division frame; trigger-seamed)."),
      kf(EndT, 2400, 7550, 112, "line", 880, "documented",
        "At the attack position for the afternoon renewal on the McPherson's Ridge / Stone-front sector — the receiving side documents the front (us-i-3-2-stone.md); committed after 13:00 (afternoon file)."),
    ],
  },
  {
    id: "csa-bn-pegram", name: "Pegram's Artillery Battalion",
    side: "confederate", frontage: 450, depth: 30,
    kfs: [
      kf(0, 1800, 8150, 112, "column", 480, "inferred",
        "With Heth's column down the pike (Hill: 'at 5 a.m., Heth took up the line of march', or-27-2-hill; the battalion's own July 1 report is not banked — position rides the division frame + Davis's artillery note, flagged)."),
      kf(T(8, 15), 2370, 7940, 115, "line", 480, "documented",
        "THE HERR RIDGE GUN LINE: 'the artillery was placed in position within about 2 miles from town' (or-27-2-davis-jr); Gamble's receiving side — the six rifles 'opened on the enemy's advancing column', 'two CSA batteries in reply' (or-27-1-gamble) — the CA-J1M-4a artillery phase's CSA half."),
      kf(EndT, 2370, 7940, 115, "line", 470, "documented",
        "The Herr Ridge line through the morning fight and the lull (the afternoon forward displacement to Seminary Ridge rides the next phase). Decay: the battalion's July 1 share of its campaign 47 (master-table basis) [D, flagged]."),
    ],
  },
  {
    id: "csa-bn-mcintosh", name: "McIntosh's Artillery Battalion",
    side: "confederate", frontage: 400, depth: 30,
    kfs: [
      kf(0, 1500, 8300, 112, "column", 384, "inferred",
        "With Pender's division in the column's second element ('About 8 o'clock... at the head of the division, and just in the rear of the division of Major-General Heth', or-27-2-perrin — the corps column's order; the battalion's own July 1 statement is not banked, flagged)."),
      kf(T(11, 30), 2250, 7850, 115, "line", 384, "inferred",
        "Into the Herr Ridge line's southern extension as Pender's division forms (division frame; trigger-seamed — no battalion clock)."),
      kf(EndT, 2250, 7850, 115, "line", 384, "documented",
        "The reinforced gun line at the lull (Wainwright's receiving side names the converging artillery preparation before the final push; the afternoon displacement rides the next phase)."),
    ],
  },
  // ======================= PENDER'S DIVISION (approach + bounds) ===========
  {
    id: "csa-perrin", name: "Perrin's Brigade (McGowan's), Pender's Division",
    side: "confederate",
    kfs: [
      kf(0, 1300, 8500, 112, "column", 1600, "documented",
        "'About 8 o'clock... commenced the march, on the turnpike leading to Gettysburg, at the head of the division, and just in the rear of the division of Major-General Heth' (or-27-2-perrin) — the corps column's second element (Hill's 5 a.m. start frame, or-27-2-hill). Strength: B&M-repro 1,882 hop MINUS the 1st SC Rifles 'left with the Rifles to guard the wagon train' (the named detachment primary) — engaged four-regiment figure materially below the hop, authored ~1,600 FLAGGED (recorded, not computed — dossier EC2). Dossier csa-3c-pen-1-perrin.md (T4, July 1 grain)."),
      kf(T(10, 30), 1900, 7900, 112, "line", 1600, "documented",
        "Filed off ~3 miles out; line formed leaving room 'between my left and the Gettysburg road for General Scales' brigade', skirmishers right (Haskell) — the division's first line right of the pike (or-27-2-perrin)."),
      kf(T(12, 0), 2350, 7500, 112, "line", 1600, "documented",
        "The first supporting bound: ~1 mile forward behind Heth (or-27-2-perrin — the two-bound approach; the second bound 'until about 3 o'clock' and the assault 'until after 4 o'clock' are the afternoon phase's clocks)."),
      kf(EndT, 2350, 7500, 112, "line", 1600, "documented",
        "In line behind Heth at the lull (the CA-J1P-5 executor waits on Hill's 2:30 junction decision — or-27-2-hill, afternoon file)."),
    ],
  },
  {
    id: "csa-lowrance", name: "Scales's Brigade, Pender's Division",
    side: "confederate",
    kfs: [
      kf(0, 1350, 8600, 112, "column", 1250, "documented",
        "With Pender's column behind Heth (or-27-2-scales; the ~30-min halt behind Hill's corps artillery in his July 1 narrative). Strength: ~1,250 pre-July-1 basis (dossier EC2 authoring note: the decay must cross Lowrance's documented 500-man evening waypoint). Dossier csa-3c-pen-4-lowrance.md (T4 full-arc)."),
      kf(T(10, 30), 2000, 8000, 112, "line", 1250, "documented",
        "Deployed: 'my left resting upon the turnpike leading from Cashtown to Gettysburg' (or-27-2-scales — Scales between the pike and McGowan's; Lane's swap to the far right recorded)."),
      kf(T(12, 0), 2450, 7700, 112, "line", 1250, "inferred",
        "Forward bound with the division line (Perrin's two-bound frame; trigger-seamed)."),
      kf(EndT, 2450, 7700, 112, "line", 1250, "documented",
        "In line at the lull; the Davis-front rescue and the seminary descent are the afternoon phase's records (or-27-2-scales)."),
    ],
  },
  {
    id: "csa-lane", name: "Lane's Brigade, Pender's Division",
    side: "confederate",
    kfs: [
      kf(0, 1250, 8400, 112, "column", 1700, "documented",
        "With Pender's column (or-27-2-lane-jh, July 1 section). Strength: ~1,700 compilation class (hop flagged — no report primary). Dossier basis: the pass-11 July 1 section reads on or-27-2-lane-jh (the Perrin/Lowrance dossiers carry the division anatomy)."),
      kf(T(10, 30), 1950, 7600, 112, "line", 1700, "documented",
        "The swap to the division's far right against the cavalry front (or-27-2-scales records the swap; Lane's own report has the cavalry-watch assignment)."),
      kf(T(12, 0), 2400, 7200, 112, "line", 1700, "inferred",
        "Forward with the division's bounds, the right flank refused toward the Fairfield road (trigger-seamed)."),
      kf(EndT, 2400, 7200, 112, "line", 1700, "documented",
        "The right-flank line at the lull: 'Colonel Barbour threw out 40 men, under Captain Hudson, to keep back some of the enemy's cavalry, which had dismounted and were annoying us with an enfilade fire' (or-27-2-lane-jh) — Gamble's front, both-sided; the afternoon delay this stand imposed is the Gamble claim's three-report convergence."),
    ],
  },
  {
    id: "csa-thomas", name: "Thomas's Brigade, Pender's Division",
    side: "confederate",
    kfs: [
      kf(0, 1200, 8550, 112, "column", 1300, "inferred",
        "With Pender's column (division frame; Thomas's July 1 is the division's reserve record — strength ~1,300 compilation class, hop flagged)."),
      kf(T(11, 0), 1900, 7800, 112, "line", 1300, "inferred",
        "Division reserve line (Pender's fourth brigade held out of the first line — division frame, trigger-seamed)."),
      kf(EndT, 2300, 7600, 112, "line", 1300, "documented",
        "Reserve at the lull (his brigade's July 1 non-commitment is the division records' negative space — Perrin's assault went in without him; carried as documented stillness)."),
    ],
  },
  // ======================= RODES'S DIVISION (arrival at the seam) ==========
  {
    id: "csa-oneal", name: "O'Neal's Brigade, Rodes's Division",
    side: "confederate",
    kfs: [
      kf(0, 3550, 9300, 165, "column", 1794, "documented",
        "Rodes's division on the approach from Heidlersburg via the Middletown road (division frame, or-27-2-rodes — his July 1 narrative has NO clock, page-verified negative). Strength: the Rodes division June 30 return (Carlisle) — O'Neal 138 officers + 1,656 men PFD = 1,794 (or-27-2-rodes-return-jun30, p. 564 — the only division-grain CSA strength return in the 27/2 run). Dossier csa-2c-rod-5-oneal.md."),
      kf(T(12, 0), 3950, 8700, 165, "column", 1794, "documented",
        "Oak Hill reached 'about noon' (Rodes division tablet 'occupied Oak Ridge about noon', ED-26 verification — CA-J1M-7's far-side bound: the lull is the interval between this arrival and the 14:00 attack)."),
      kf(EndT, 3990, 8620, 160, "line", 1794, "documented",
        "Forming on the ridge east slope (Ewell's deployment anatomy: O'Neal between Iverson and the plain, or-27-2-ewell; the 14:00 attack — CA-J1P-1 — is the afternoon phase's opening act)."),
    ],
  },
  {
    id: "csa-iverson", name: "Iverson's Brigade, Rodes's Division",
    side: "confederate",
    kfs: [
      kf(0, 3450, 9400, 165, "column", 1464, "documented",
        "Leading brigade of the division's approach (or-27-2-rodes). Strength: June 30 return — Iverson 114 officers + 1,350 men PFD = 1,464 (or-27-2-rodes-return-jun30 p. 564; the tablet's 'Present about 1,470' sits 6 apart — near-exact, different measures; B&M 1,384 hop carried beside, un-averaged). Dossier csa-2c-rod-2-iverson.md (T4 full-arc; the EC6 single-spike exemplar)."),
      kf(T(12, 15), 3884, 8742, 160, "line", 1464, "documented",
        "THE OAK HILL LAUNCH GROUND: formed on the division right (or-27-2-rodes); tablet mon-iverson-hq (3883.7,8742.4) with the Rodes-division tablet beside it (formation semantics, ED-39)."),
      kf(EndT, 3884, 8742, 160, "line", 1464, "documented",
        "In line at the seam (the advance to the Forney field — CA-J1P-2, the destruction — is the afternoon phase's record; the j1-09 drawn mid-advance bar is its 2 p.m. state)."),
    ],
  },
  {
    id: "csa-doles", name: "Doles's Brigade, Rodes's Division",
    side: "confederate",
    kfs: [
      kf(0, 3600, 9350, 168, "column", 1369, "documented",
        "With the division's approach. Strength PRIMARY: 'The brigade went into action with 131 officers and 1,238 enlisted men; total, 1,369' (or-27-2-doles); the June 30 return's 1,403 PFD reconciles (~34 details). Dossier csa-2c-rod-3-doles.md (T4 full-arc)."),
      kf(T(12, 30), 4500, 8700, 172, "column", 1369, "documented",
        "Into the plain on the division left ('Doles on the left, in the plain', or-27-2-ewell; the 5th Ala guarding the wide gap to O'Neal on the ridge)."),
      kf(EndT, 4600, 8650, 175, "line", 1369, "documented",
        "'Formed into line of battle about 1 p.m. July 1, in front of Gettysburg... We occupied the left of Major-General Rodes' division' (or-27-2-doles) — the seam-adjacent formation clock (ED-69's interior-ladder component: Coulter 12:30 → DOLES ~13:00 → Stone/Dobke 13:30 → tablet 14:00 adopted)."),
    ],
  },
  {
    id: "csa-daniel", name: "Daniel's Brigade, Rodes's Division",
    side: "confederate",
    kfs: [
      kf(0, 3400, 9450, 165, "column", 2294, "documented",
        "With the division's approach (the division's largest brigade). Strength: June 30 return — Daniel 171 officers + 2,123 men PFD = 2,294 (or-27-2-rodes-return-jun30 p. 564). Dossier csa-2c-rod-1-daniel.md."),
      kf(T(12, 30), 3800, 8800, 160, "line", 2294, "documented",
        "Second line behind Iverson's right (or-27-2-rodes deployment anatomy; or-27-2-daniel — his July 1 railroad-cut fight cost 'nearly one-third of their number', the fraction his own report states)."),
      kf(EndT, 3820, 8750, 160, "line", 2294, "documented",
        "In the second line at the seam (his support of Iverson's right and the railroad-cut fight are the afternoon phase's records)."),
    ],
  },
  {
    id: "csa-ramseur", name: "Ramseur's Brigade, Rodes's Division",
    side: "confederate",
    kfs: [
      kf(0, 3350, 9500, 165, "column", 1090, "documented",
        "'In rear of the division train... arrived on the field after the division had formed line of battle' (or-27-2-ramseur — kind-none clock profile). Strength: June 30 return — Ramseur 119 officers + 971 men PFD = 1,090 (or-27-2-rodes-return-jun30 p. 564; the ~1,027 compilation NOT located in a fetchable primary — flagged, not carried). Dossier csa-2c-rod-4-ramseur.md (T4 full-arc)."),
      kf(T(12, 45), 3800, 8850, 160, "column", 1090, "documented",
        "Reserve behind Oak Hill, supporting Doles/O'Neal/Iverson 'according to circumstances' (or-27-2-ramseur); tablet mon-ramseur-hq (3764.0,8804.9, formation semantics)."),
      kf(EndT, 3800, 8850, 160, "column", 1090, "documented",
        "In reserve at the seam ('After resting about fifteen minutes' his commitment follows the afternoon attack's jam — the counter-stroke is the next phase's record)."),
    ],
  },
  {
    id: "csa-bn-carter", name: "Carter's Artillery Battalion",
    side: "confederate", frontage: 350, depth: 30,
    kfs: [
      kf(0, 3500, 9350, 165, "column", 384, "inferred",
        "With Rodes's division column (division frame; the battalion's own July 1 report is not banked — flagged)."),
      kf(T(12, 15), 4050, 8600, 165, "line", 384, "documented",
        "THE OAK HILL GUNS: Carter's batteries onto the hill as the division arrives — received on the Union side as 'between 12 and 1 o'clock... a new battery upon a hill on the extreme right opened a most destructive enfilade' (or-27-1-stone-roy — the receiving clock that pairs the Rodes tablet's 'about noon')."),
      kf(EndT, 4050, 8600, 165, "line", 384, "documented",
        "The enfilade program on the I Corps line from Oak Hill (Stone's re-fold — the 149th into the pike — is its receiving-side consequence; the afternoon duel with Dilger rides the next phase)."),
    ],
  },
  // ======================= I CORPS (Wadsworth first, the stagger) ==========
  {
    id: "us-meredith", name: "Meredith's Iron Brigade (1st Bde, 1st Div, I Corps)",
    side: "union",
    kfs: [
      kf(0, 4400, 5100, 340, "column", 1829, "documented",
        "March from Marsh Creek: 'moved from camp early... 6 or 7 miles' (or-27-1-morrow-24mi); Reynolds took Wadsworth's division forward in person, the corps staggered 'an hour and a half to two hours' behind (or-27-1-doubleday — the stagger primary that explains Wadsworth's lone fight). Strength: brigade ~1,829 compilation (open at brigade grain, flagged); 24th Mich PRIMARY 496 (or-27-1-morrow-24mi). Deployment order R-to-L: 2nd Wis, 7th Wis, 19th Ind, 24th Mich; the 6th Wis + 100-man brigade guard detached as corps reserve (or-27-1-doubleday; or-27-1-dawes-6wi). Dossier us-i-1-1-meredith.md (T4 full-arc)."),
      kf(T(9, 45), 4000, 6500, 320, "column", 1829, "inferred",
        "Obliquing left across the fields toward the seminary (the corps' approach leg at tier-B physics: ~6 miles from Marsh Creek at column pace ≈ 2.5-3 h from an early start; Morrow's 'about 9 a.m. we came near the town' runs early vs Wadsworth's 10:00 — BOTH carried, the keyframe sits on the receiving cluster's 10:00-10:15)."),
      kf(T(10, 15), 3050, 7300, 292, "line", 1829, "documented",
        "THE CHARGE (CA-J1M-4b receiving / CA-J1M-5): deployed 'on the double-quick... no order being given or time allowed for loading our guns' (or-27-1-morrow-24mi); 'deployed en echelon without a moment's hesitation, charged with the utmost steadiness and fury' (or-27-1-doubleday). REYNOLDS FELL LAUNCHING THIS DEPLOYMENT — 'in the beginning of the attack referred to, about 10.15 a.m.' (or-27-1-doubleday) = Wadsworth 'at this time (about 10.15 a.m.) our gallant leader fell' = the WD tablet 'about 10.15 A.M.' — the THREE-PRIMARY pin (ED-66 rider; Howard's 11.30 is when word reached him, collapse recorded). mon-reynolds-kia (3177.2,7265.8). Drawn: j1-03 'MEREDITH'S Brig.' (2971,7100) with the 7'Wis/19'Ind/24'Mich bars arriving in echelon."),
      kf(T(10, 45), 2850, 7280, 292, "line", 1750, "documented",
        "At the run — the Archer pocket: 'the brigade dashed up and over the hill and down into the ravine, through which flows Willoughby's Run' (or-27-1-morrow-24mi); the capture climax both-sided (or-27-2-shepard's 'some 75... General Archer among the rest'). mon-archer-captured on the charge axis; Iron Bde HQ marker mon-ironbde-hq (2905.0,7384.1)."),
      kf(T(11, 15), 3000, 7300, 292, "line", 1700, "documented",
        "Back to the east bank and into McPherson's woods: 'changed front forward on first battalion, and marched into the woods known as McPherson's woods' — 19th Ind left of 24th Mich, 7th Wis right, the 24th's right wing 'curved a little backward' (or-27-1-morrow-24mi); Doubleday's hold order: 'must be held at all hazards.'"),
      kf(EndT, 3020, 7300, 292, "line", 1700, "documented",
        "The woods line held through the lull (skirmishers 'at once engaged'); the afternoon defense against Pettigrew's brigade — the 24th Mich vs 26th NC fight — is the next phase's record."),
    ],
  },
  {
    id: "us-cutler", name: "Cutler's Brigade (2nd Bde, 1st Div, I Corps)",
    side: "union",
    kfs: [
      kf(0, 4450, 5150, 340, "column", 1600, "documented",
        "'Moved from camp early... being the leading brigade of the corps' (or-27-1-cutler). STRENGTH HONESTY: regiment primaries 147th NY 380, 76th NY 375, 56th Pa 252 (or-27-1-cutler); 95th NY / 14th Brooklyn / 7th Ind open — brigade authored ~1,600 compilation class FLAGGED; the 7th Ind detached 'on duty in the rear' July 1 (rejoined at evening — EC2 detachment primary). Dossier us-i-1-2-cutler.md (T4 full-arc)."),
      kf(T(10, 0), 3250, 7550, 292, "line", 1600, "documented",
        "THE OPENING LINE (CA-J1M-4 Union right): across the railroad north of the pike with 76th NY / 147th NY / 56th Pa — 'in action from 10 a.m. until 4 p.m.' (his own bracket); 95th NY + 14th Brooklyn detached left BY GENERAL REYNOLDS's order to support Hall — Reynolds's last tactical act in a brigade primary (or-27-1-cutler). Drawn: j1-03 'CUTLER'S Brig.' (3173,7397), '147 N.Y' bar (3170,7528); mon-cutler-hq (3519.6,7788.0)."),
      kf(T(10, 40), 3500, 7700, 292, "line", 1150, "documented",
        "The right wing's wreck and Wadsworth's fall-back order 'to the woods on the next ridge'; the 147th NY stranded (Lt. Col. Miller w 'at the moment of receiving' the order — the wounding that CAUSED the stranding) 'until the enemy were in possession of the railroad cut on his left'. RATE-CLASS PRIMARIES: 147th NY '207 out of 380 men and officers within half an hour'; 76th NY 169 'within thirty minutes'; 56th Pa 78+ (or-27-1-cutler — the audit's cleanest loss-rate statements)."),
      kf(T(11, 0), 3450, 7690, 292, "line", 1100, "documented",
        "The railroad-cut resolution (CA-J1M-6): the 6th Wis / 95th NY / 14th Brooklyn charge releases the 147th; re-advance to 'the crest of the ridge' for 'half to three-quarters of an hour' (or-27-1-cutler; or-27-1-dawes-6wi; both-sided with or-27-2-davis-jr)."),
      kf(EndT, 3600, 7800, 300, "line", 1100, "documented",
        "On the ridge north of the pike at the lull, both flanks open — 'having no support on either my right or left until 2 o'clock' (or-27-1-cutler; the 2 o'clock Baxter/XI sync is the afternoon phase's clock)."),
    ],
  },
  {
    id: "us-dana", name: "Stone's Bucktail Brigade (2nd Bde, 3rd Div, I Corps — Col. Roy Stone)",
    side: "union",
    kfs: [
      kf(0, 4420, 5250, 340, "column", 1317, "documented",
        "With Rowley's/Doubleday's division behind Wadsworth (the corps stagger, or-27-1-doubleday). Strength: B&M-repro 1,317 (hop flagged; 'two-thirds of it ≈ the return's 853 ✓'). Dossier us-i-3-2-stone.md (T4; the pass-11 July 1 back-extension — Roy Stone's report in full)."),
      kf(T(11, 0), 3150, 7480, 292, "line", 1317, "documented",
        "Posted 'at 11 o'clock a.m.' between Wadsworth's two brigades, 'right resting upon the Chambersburg or Cashtown road', left nearly to Meredith's wood (or-27-1-stone-roy — the stated posting clock); the skirmisher dash to the fence (held all day)."),
      kf(T(12, 45), 3180, 7500, 350, "line", 1310, "documented",
        "THE RE-FOLD: 'between 12 and 1 o'clock... a new battery upon a hill on the extreme right opened a most destructive enfilade' (or-27-1-stone-roy — the Oak Hill/Carter arrival received) — the right-angle re-fold: 149th into the pike, 143rd to its right, 150th on the west face — the brigade's double-front geometry, authored as the north-facing fold."),
      kf(EndT, 3180, 7500, 350, "line", 1310, "documented",
        "The double front held at the seam; 'At about 1.30 p.m. the grand advance of the enemy's infantry began' (or-27-1-stone-roy) is the afternoon file's opening record (ED-69: recorded early pole, report-nominal −20 profile — the anchor's 14:00 unmoved)."),
    ],
  },
  {
    id: "us-biddle", name: "Biddle's Brigade (1st Bde, 3rd Div, I Corps — Col. Chapman Biddle)",
    side: "union",
    kfs: [
      kf(0, 4380, 5300, 340, "column", 1287, "documented",
        "With the division behind Wadsworth. Strength PRIMARY: 'The total number of officers and men who went into the action was 1,287' (or-27-1-cbiddle, the July 2 report — strength, loss, and remnant in one statement; B&M 1,361 carried beside, the gap = detached elements, un-averaged). Dossier us-i-3-1-biddle.md (T4, July 1 grain)."),
      kf(T(11, 0), 3150, 6950, 285, "line", 1287, "documented",
        "Arrival ~11:00 one mile west of town; the line 'on the extreme left, in a field one-third of a mile in front of the seminary and facing west', Cooper's four-piece battery in the interval, the brick house + large stone barn sharpshooter cover named; the 151st Pa detached right-rear (or-27-1-cbiddle)."),
      kf(EndT, 3140, 6950, 285, "line", 1280, "documented",
        "The left line at the seam, 'shifted two or three times' under the enfilading artillery from the north (or-27-1-cbiddle); the Pettigrew-front assault 'between 2 and 3 p.m.' is the afternoon phase's record."),
    ],
  },
  {
    id: "us-coulter", name: "Paul's Brigade (1st Bde, 2nd Div, I Corps)",
    side: "union",
    kfs: [
      kf(0, 4500, 5200, 340, "column", 1537, "documented",
        "Robinson's division in the corps column's rear (or-27-1-robinson). Strength: B&M-repro 1,537 (hop flagged; Robinson's division frame 'We went into action with less than 2,500 men' CONFLICTS with the two-brigade B&M sum 2,989 — both carried un-averaged, dossier EC2). Dossier us-i-2-1-paul.md (T3, July 1 grain)."),
      kf(T(11, 30), 3730, 7220, 292, "line", 1537, "documented",
        "Reserve near the seminary: 'the First Brigade, under Brigadier-General Paul, was set at work to intrench the ridge' (or-27-1-robinson) — THE SEMINARY RAIL-BARRICADE the CSA final push later stormed (Perrin: 'a breastwork of rails behind [which] the enemy was posted' — both-sided across four hours, or-27-2-perrin)."),
      kf(EndT, 3730, 7220, 292, "line", 1537, "documented",
        "At the barricade reserve at the seam (the ~15:00 Oak Ridge relief insertion — Coulter's clock — is the afternoon phase's record)."),
    ],
  },
  {
    id: "us-baxter", name: "Baxter's Brigade (2nd Bde, 2nd Div, I Corps)",
    side: "union",
    kfs: [
      kf(0, 4520, 5250, 340, "column", 1452, "documented",
        "Robinson's division rear ('marched as rapidly as possible' up the Emmitsburg road). Strength: B&M-repro 1,452 (hop flagged; the Robinson <2,500 division conflict carried — dossier EC2). Dossier us-i-2-2-baxter.md (T4, July 1 grain)."),
      kf(T(11, 0), 3700, 7500, 300, "column", 1452, "documented",
        "'Arriving near the front where the battle was raging at 11 a.m.' (or-27-1-baxter) — massing west of the railroad embankment, agreeing with Coulter's 11 a.m. clock (or-27-1-coulter)."),
      kf(T(12, 15), 3900, 8000, 340, "line", 1452, "documented",
        "Two regiments (11th Pa, 97th NY) forward on Wadsworth's right, then the brigade 'changed front by filing to the right', covering the right flank — 'a division of the Eleventh Corps being on our right at least 400 yards' (the gap primary; Robinson's 'at no time less than half a mile' variance carried — both readings, dossier EC3.2)."),
      kf(EndT, 3980, 8100, 340, "line", 1452, "documented",
        "Toward the Oak Ridge crest at the seam ('moved forward to the crest of the hill' — the wall line the j1-09 drawn 'BAXTER'S BRIGADE' bar (4007,8424) fixes is the afternoon fight's ground)."),
    ],
  },
  // ---------------------- I CORPS BATTERIES (Wainwright's) -----------------
  {
    id: "us-btty-hall-2me", name: "Hall's 2nd Maine Battery (B)",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4460, 5220, 340, "column", 127, "documented",
        "Forward with Reynolds and Wadsworth's division (Reynolds took 'Wadsworth+Hall' ahead in person — or-27-1-doubleday, the corps stagger primary)."),
      kf(T(10, 10), 3230, 7560, 292, "line", 127, "documented",
        "Astride the pike replacing Calef's right sections (Doubleday: Hall's battery called up by Wadsworth after the cavalry relief); drawn: j1-03 'HALL 2'ME' (3260,7630) — the pike gun position opposite Pegram's line."),
      kf(T(10, 50), 3600, 7500, 100, "column", 120, "documented",
        "The withdrawal by piece under Davis's flank fire (the 2nd ME gun lost and RECAPTURED at the cut charge — or-27-1-dawes-6wi's trophy record, both-sided); retiring along the pike."),
      kf(EndT, 4400, 7100, 100, "column", 120, "documented",
        "Refitted in rear toward the town at the lull (the register's 'rear, withdrawn' state; Wainwright's brigade day-loss 83 officers and men + ~80 horses spreads across the group — or-27-1-wainwright p. 357)."),
    ],
  },
  {
    id: "us-btty-breck", name: "Reynolds's Battery L (with E), 1st New York Light",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4470, 5280, 340, "column", 141, "inferred",
        "With the corps artillery column (Wainwright's brigade — or-27-1-wainwright; battery-grain July 1 clock not banked, flagged)."),
      kf(T(11, 30), 3700, 7350, 292, "line", 141, "documented",
        "Onto Seminary Ridge north of the pike with the brigade's deployment ('Gettysburg Seminary is situated on a ridge about a quarter of a mile from the town...', or-27-1-wainwright p. 355 — the group's frame statement)."),
      kf(EndT, 3700, 7350, 292, "line", 140, "documented",
        "The Seminary Ridge gun line at the seam (the afternoon canister fight and the fighting withdrawal are the next phase's records — or-27-1-wainwright)."),
    ],
  },
  {
    id: "us-btty-cooper", name: "Cooper's Battery B, 1st Pennsylvania Light",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4430, 5320, 340, "column", 114, "inferred",
        "With the corps artillery column (or-27-1-cooper; us-btty-cooper.md dossier)."),
      kf(T(11, 30), 3200, 7000, 285, "line", 114, "documented",
        "Into Biddle's interval: 'Cooper's four-piece battery in the interval' between the brigade's regiments on the corps' extreme left (or-27-1-cbiddle — the receiving-frame position primary)."),
      kf(EndT, 3200, 7000, 285, "line", 114, "documented",
        "The left-interval position at the seam (the seminary displacement rides the afternoon file)."),
    ],
  },
  {
    id: "us-btty-stevens", name: "Stevens's 5th Maine Battery (E)",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4450, 5350, 340, "column", 136, "inferred",
        "With the corps artillery column (Wainwright's brigade; battery-grain July 1 morning clock not banked, flagged)."),
      kf(T(11, 30), 3750, 7150, 292, "line", 136, "documented",
        "Near the seminary with the group (Wainwright's Seminary-line frame; the battery's named afternoon service — the barricade canister fight — rides the next phase; or-27-1-wainwright)."),
      kf(EndT, 3750, 7150, 292, "line", 136, "documented", "The seminary ground at the seam."),
    ],
  },
  {
    id: "us-btty-stewart", name: "Stewart's Battery B, 4th US Artillery",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4480, 5300, 340, "column", 132, "inferred",
        "With the corps artillery column (Wainwright's brigade)."),
      kf(T(11, 45), 3640, 7660, 300, "line", 132, "documented",
        "THE RAILROAD-CUT STRADDLE: the battery split across the middle cut by the pike (Wainwright's layout; Lt. Davison w — 'First Sergeant John Mitchell... assumed battery command', commended at officer level, or-27-1-wainwright) — the position whose down-the-pike canister anchors the afternoon collapse's cover."),
      kf(EndT, 3640, 7660, 300, "line", 132, "documented", "The cut position held at the seam."),
    ],
  },
  // ======================= XI CORPS (the leading edge only) ================
  {
    id: "us-vonamsberg", name: "Schimmelfennig's Brigade (1st Bde, 3rd Div, XI Corps — Col. von Amsberg)",
    side: "union",
    kfs: [
      kf(0, 4300, 4900, 340, "column", 1670, "documented",
        "Schurz's division on the Emmitsburg road (7 a.m. start, the 10:30 hurry-forward order — or-27-1-schurz; or-27-1-howard's approach ladder). Strength: B&M-repro 1,670 (hop flagged; Schurz's 'hardly over 6,000' two-division frame). THE CASCADE UP: Howard to the field → Schurz to the corps → Schimmelfennig to the division → von Amsberg to the brigade 'at this moment' (or-27-1-dobke-45ny). Dossier us-xi-3-1-schimmelfennig.md (T3, July 1 grain)."),
      kf(T(11, 15), 4700, 6400, 350, "column", 1670, "documented",
        "Arrived ~11 a.m., 'hurried to the battle-field in double-quick, to the right of the First Army Corps' (or-27-1-dobke-45ny)."),
      kf(T(11, 45), 4450, 8050, 350, "skirmish", 1670, "documented",
        "The 45th NY 'on the immediate right of the First Corps, threw out four companies as skirmishers at 11.30 a.m.', then wholly deployed as skirmishers; 'Dilger's battery, in the rear, and the Sixty-first Ohio, on the right' — the brigade's left-anchor geometry primary (or-27-1-dobke-45ny)."),
      kf(EndT, 4500, 8150, 350, "skirmish", 1670, "documented",
        "The open-field skirmish line north of town at the seam — under the Oak Hill batteries' fire ('grape, canister, solid shot, and shell') from Carter's arrival onward; the 1.30 flank-fire event opens the afternoon file."),
    ],
  },
  {
    id: "us-btty-dilger", name: "Dilger's Battery I, 1st Ohio Light",
    side: "union", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4280, 4850, 340, "column", 127, "inferred",
        "With Schurz's division column on the Emmitsburg road (or-27-1-schurz; battery-grain morning clock rides the division ladder, flagged)."),
      kf(T(12, 30), 4650, 7900, 355, "line", 127, "documented",
        "Forward with the division's deployment — 'Dilger's battery, in the rear' of the 45th NY's line (or-27-1-dobke-45ny — the brigade-frame position primary); the Carlisle-road front."),
      kf(EndT, 4700, 8000, 355, "line", 127, "documented",
        "The Carlisle-road gun position at the seam (the counter-battery duel with Carter's Oak Hill line is the afternoon file's opening window)."),
    ],
  },
];

// ---------------------------------------------------------------------------
// Events — fire windows per the activity records (attach level = the level
// the source attests; documented silence stays event-free).
// ---------------------------------------------------------------------------
const events: EngagementEvent[] = [
  // The first shot + the cavalry delay (CA-J1M-1/2).
  fireEvent({
    id: "j1m-calef-opening", kind: "artillery_fire", unitId: "us-btty-calef",
    t0: T(8, 0), t1: T(10, 30), confidence: "documented",
    citation: "'The first Union gun of the battle was fired from the right section' (tablet verbatim); the six rifles 'opened on the enemy's advancing column, doing good execution' (or-27-1-gamble).",
    note: "CA-J1M-1's 07:30 first shot (the 1886 marker's hour) is Jones's carbine at the picket post ~3 miles west — off the authored line; the battery's window opens with the delay-line fight.",
  }),
  fireEvent({
    id: "j1m-gamble-carbines", kind: "musketry", unitId: "us-cav-gamble",
    t0: T(8, 15), t1: T(10, 30), confidence: "documented",
    citation: "The dismounted skirmisher line's fight, ~08:00 to the infantry relief: 'After checking and retarding the advance of the enemy several hours' (or-27-1-gamble) = Buford's 'more than two hours' (CA-J1M-2).",
  }),
  fireEvent({
    id: "j1m-devin-screen", kind: "musketry", unitId: "us-cav-devin",
    t0: T(9, 0), t1: T(12, 30), confidence: "documented",
    citation: "The four-road screen's checked-and-held fight: 'advanced upon Devin by four roads, and on each was checked and held' (or-27-1-buford); the two-hour Heidlersburg hold (or-27-1-devin).",
  }),
  // The CSA artillery phase (CA-J1M-4a).
  fireEvent({
    id: "j1m-pegram-herr", kind: "artillery_fire", unitId: "csa-bn-pegram",
    t0: T(8, 15), t1: T(11, 45), confidence: "documented",
    citation: "The Herr Ridge line against the cavalry then the I Corps: 'the artillery was placed in position within about 2 miles from town' (or-27-2-davis-jr); 'two CSA batteries in reply' (or-27-1-gamble receiving).",
  }),
  fireEvent({
    id: "j1m-hall-pike", kind: "artillery_fire", unitId: "us-btty-hall-2me",
    t0: T(10, 10), t1: T(10, 50), confidence: "documented",
    citation: "The pike position's counter-battery and canister fight, relief to the flanked withdrawal by piece (or-27-1-doubleday; or-27-1-dawes-6wi's recaptured-gun record bounds the loss).",
  }),
  // The infantry attack (CA-J1M-4b) and the cut (CA-J1M-6).
  fireEvent({
    id: "j1m-archer-run", kind: "musketry", unitId: "csa-fry",
    t0: T(10, 15), t1: T(11, 15), confidence: "documented",
    citation: "The run crossing and the pocket ('the brigade rushed across with a cheer' → the counter-charge, or-27-2-shepard).",
  }),
  fireEvent({
    id: "j1m-meredith-charge", kind: "musketry", unitId: "us-meredith",
    t0: T(10, 15), t1: T(11, 30), confidence: "documented",
    citation: "The unloaded double-quick charge and the run fight (or-27-1-morrow-24mi; or-27-1-doubleday).",
  }),
  fireEvent({
    id: "j1m-davis-attack", kind: "musketry", unitId: "csa-davis",
    t0: T(10, 20), t1: T(11, 25), confidence: "documented",
    citation: "'About 10.30 o'clock... the brigade moved forward to the attack' through the cut fight (or-27-2-davis-jr p. 649).",
  }),
  fireEvent({
    id: "j1m-cutler-opening", kind: "musketry", unitId: "us-cutler",
    t0: T(10, 0), t1: T(11, 20), confidence: "documented",
    citation: "'In action from 10 a.m.' — the two-line frontal+flank assault received at short range; the 147th NY's stranded half-hour (or-27-1-cutler; the rate-class loss primaries).",
  }),
  fireEvent({
    id: "j1m-rrcut-charge", kind: "musketry",
    segment: { x: 3400, z: 7690, x2: 3560, z2: 7720 },
    t0: T(10, 45), t1: T(11, 5), confidence: "documented",
    citation: "The railroad-cut charge (CA-J1M-6): the 6th Wis (detached corps reserve) + 95th NY + 14th Brooklyn fence fire-by-file and charge — 'the rebels began throwing down their arms'; Blair's 2nd Miss surrender to Dawes (or-27-1-dawes-6wi; the detachment fought off its brigade track, hence the fixed segment).",
  }),
  fireEvent({
    id: "j1m-stone-skirmish", kind: "musketry", unitId: "us-dana",
    t0: T(11, 0), t1: EndT, confidence: "documented",
    citation: "The skirmisher dash to the fence 'held all day' + the enfilade received from Oak Hill 'between 12 and 1 o'clock' (or-27-1-stone-roy).",
  }),
  // The Oak Hill guns open at the seam.
  fireEvent({
    id: "j1m-carter-oakhill", kind: "artillery_fire", unitId: "csa-bn-carter",
    t0: T(12, 30), t1: EndT, confidence: "documented",
    citation: "'Between 12 and 1 o'clock... a new battery upon a hill on the extreme right opened a most destructive enfilade' (or-27-1-stone-roy receiving) — Carter's Oak Hill line, pairing the Rodes tablet's 'about noon' arrival.",
  }),
];

// ---------------------------------------------------------------------------
// The battle document.
// ---------------------------------------------------------------------------
const battle: Battle = {
  name: "Gettysburg — July 1, 1863: Morning (Buford's stand and the meeting engagement)",
  startTime: StartTime,
  endTime: EndT,
  units: movers.map((m) => moverUnit({
    id: m.id, name: m.name, side: m.side,
    frontage_m: m.frontage, depth_m: m.depth, keyframes: m.kfs,
  })),
  events,
  environment: {
    windTowardDeg: 45, windMps: 0.0, confidence: "unknown",
    note: "No sourced wind observation for the July 1 morning exists in the corpus (the ED-10/ED-19 class); calm authored — windMps 0 = no drift.",
  },
};

writeFileSync(outPath, exportValidated(battle));
console.log(`wrote ${outPath}: ${battle.units.length} units, ${events.length} events, ` +
  `window ${StartTime}..${StartTime + EndT} (07:30-13:00 LMT)`);
