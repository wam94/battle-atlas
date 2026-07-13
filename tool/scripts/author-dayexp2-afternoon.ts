// Day expansion slice 2 — THE JULY 2 AFTERNOON PHASE (the en-echelon
// assault on the Union left, 15:30–19:29 LMT sunset). Run from tool/:
//   npx vite-node scripts/author-dayexp2-afternoon.ts
// Committed as the derivation record (the author-w*/A1/A2/dayexp1 pattern).
// Slice report: docs/reconstruction/audit/day-expansion-slice-2.md.
//
// THE PHASE (ADR 0005: one battle file = one phase = one clock):
//   startTime 55800 (15:30 LMT) · endTime 14340 (ends 19:29 LMT — the
//   ED-31/CA-J2A-11 sunset pin; the evening phase abuts at 70140).
// The manifest's july2-afternoon phase ECHOES this clock (test-enforced).
//
// HARD RULES this script observes:
// - The July 3 file (gettysburg-july3.json) is READ ONLY — static poses,
//   names, and rosters are inherited from it for night-continuity, and it
//   is never written (the shipped film's ground truth stays byte-stable).
// - NEVER invent motion: movers carry the dossier corpus's July 2 arcs
//   (passes 5-8; ED-53/56/57/58/59/60/61 rulings); trigger-seamed waves
//   (ED-57) author at the adopted envelope times with `inferred`
//   confidence; conflicts are CARRIED in citations, never averaged.
// - Units without July 2 dossier anchors get honest window-endpoint
//   statics (T1-grade): pose inherited from their attested/July-3 ground,
//   confidence `inferred`, the honesty note in the citation text.
// - Units NOT on the field July 2 15:30-19:29 are EXCLUDED, never faked:
//   Pickett's division + Dearing's battalion (arrived toward evening,
//   bivouacked west, off the reconstruction crop), the July-3 cavalry
//   fields (Farnsworth/Merritt/Elder/Graham), and Gregg's Union cavalry
//   at Brinkerhoff's Ridge (no dossier read banked — honest cut, in the
//   slice report; Walker's detention is carried on HIS record, ED-62).
import { writeFileSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe, Unit } from "../src/model";
import { fireEvent, moverUnit, staticUnit, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const july3Path = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
const outPath = join(here, "../../app/Assets/Battle/gettysburg-july2-afternoon.json");
const july3: Battle = JSON.parse(readFileSync(july3Path, "utf8"));

// Clock: t = seconds from 15:30 LMT (startTime 55800). Sunset 19:29 = 14340.
const StartTime = 55800;
const EndT = 14340; // 19:29 LMT (ED-31; CA-J2A-11 — Wofford's sunset order)
const T = (h: number, m: number) => h * 3600 + m * 60 - StartTime;

// ---------------------------------------------------------------------------
// July-3 pose inheritance (night-continuity + T1 statics)
// ---------------------------------------------------------------------------
const j3ById = new Map(july3.units.map((u) => [u.id, u]));
function j3(id: string): Unit {
  const u = j3ById.get(id);
  if (!u) throw new Error(`dayexp2: July 3 unit '${id}' missing`);
  return u;
}
function j3pose(id: string) {
  const k = j3(id).keyframes[0]!;
  return { x: k.x, z: k.z, facing: k.facing, formation: k.formation, strength: k.strength };
}

// Excluded (not on the field inside this phase's window — see header).
const EXCLUDE = new Set([
  // Pickett's division (arrived toward evening July 2, bivouacked off-crop
  // west; or-27-2-anv frame) + its children + Dearing's battalion.
  "csa-garnett", "csa-8va", "csa-18va", "csa-19va", "csa-28va", "csa-56va",
  "csa-kemper", "csa-1va", "csa-3va", "csa-7va", "csa-11va", "csa-24va",
  "csa-armistead", "csa-9va", "csa-14va", "csa-38va", "csa-53va", "csa-57va",
  "csa-bn-dearing",
  // July-3 cavalry fields (Kilpatrick/Merritt arrived July 3; wave-6 scope).
  "us-cav-farnsworth", "us-cav-merritt", "us-btty-elder", "us-btty-graham",
  // Pettigrew/Pickett-column children exist only for July 3 display grain;
  // their parents render July 2 (in reserve) as single blocks.
  "csa-5al-bn", "csa-13al", "csa-1tn", "csa-7tn", "csa-14tn",
  "csa-11nc", "csa-26nc", "csa-47nc", "csa-52nc",
  "csa-2miss", "csa-11miss", "csa-42miss", "csa-55nc",
  "csa-brock-right", "csa-brock-left",
  // Angle-cast children (July 3 display grain; parents carry July 2).
  "us-69pa", "us-71pa", "us-72pa",
  "us-19ma", "us-20ma", "us-7mi", "us-42ny", "us-59ny",
  "us-13vt", "us-14vt", "us-16vt",
]);

// July-2 command names where the July 3 file's name carries the successor.
const J2NAME: Record<string, string> = {
  "csa-sheffield": "Law's Brigade, Hood's Division",
  "csa-luffman": "G. T. Anderson's Georgia Brigade, Hood's Division",
  "csa-bryan": "Semmes's Brigade, McLaws's Division",
  "csa-humphreys": "Barksdale's Brigade, McLaws's Division",
  "csa-williams": "Nicholls's Brigade, Johnson's Division (Col. J. M. Williams)",
  "csa-dungan": "Jones's Brigade, Johnson's Division",
  "csa-godwin": "Hoke's Brigade, Early's Division (Col. I. E. Avery)",
  "csa-bn-raine": "Andrews's Artillery Battalion (Maj. J. W. Latimer)",
  "us-rice": "Vincent's Brigade (3rd Bde, 1st Div, V Corps)",
  "us-garrard": "Weed's Brigade (3rd Bde, 2nd Div, V Corps)",
  "us-btty-rittenhouse": "Hazlett's Battery D, 5th US Artillery",
  "us-berdan": "Ward's Brigade (2nd Bde, 1st Div, III Corps)",
  "us-madill": "Graham's Brigade (1st Bde, 1st Div, III Corps)",
  "us-sherrill": "Willard's Brigade (3rd Bde, 3rd Div, II Corps)",
  "us-mckeen": "Cross's Brigade (1st Bde, 1st Div, II Corps)",
  "us-fraser": "Zook's Brigade (3rd Bde, 1st Div, II Corps)",
  "us-dana": "Stone's Brigade (2nd Bde, 3rd Div, I Corps — Col. Dana)",
  "us-harris": "Ames's Brigade (2nd Bde, 1st Div, XI Corps)",
  "us-btty-james": "Seeley's Battery K, 4th US Artillery",
  "us-nevin": "Wheaton's/Nevin's Brigade (3rd Bde, 3rd Div, VI Corps)",
};

// ---------------------------------------------------------------------------
// MOVERS — the dossiered July 2 arcs (passes 5-8). Every documented keyframe
// cites its dossier evidence; trigger-seamed times (ED-57) and physics-timed
// legs (tier B) author as `inferred` with the derivation in the citation.
// ---------------------------------------------------------------------------
interface M {
  id: string; frontage?: number; depth?: number; kfs: Keyframe[];
  regiments?: string[]; parent?: string; name?: string; side?: "union" | "confederate";
}

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  confidence: Keyframe["confidence"], citation?: string): Keyframe => ({
    t, x, z, facing, formation, strength,
    ...(confidence !== undefined && { confidence }),
    ...(citation !== undefined && { citation }),
  });

const movers: M[] = [
  // ------------------------- HOOD'S DIVISION (the ladder's first rung) ----
  {
    id: "csa-sheffield",
    kfs: [
      kf(0, 2850, 2120, 70, "line", 1500, "documented",
        "Formed on the division right after the New Guilford march: tablet 'Arrived and formed line ... about 4 P.M.' (formed 50 yards west of the tablet, South Confederate Ave); Scruggs 'arriving at 3:30 p.m.' (or-27-2-scruggs-4al); regiment order 4th-47th|15th|44th-48th per Oates (oates-1905). Strength: tablet 'Present approximately 1,500' (compilation tier; the 15th AL's ~500 effectives is the only regiment primary, oates-1905). Shelled in line during the ~30-min cannonade (Hood w in it; law-bl-1884). Dossier csa-1c-hood-1-law.md."),
      kf(T(16, 30), 2850, 2120, 70, "line", 1500, "inferred",
        "Step-off ~16:30 [envelope 16:15-17:00] per ED-53 (CA-J2A-3 revised): the 16:00 Union signal dispatch Law quotes excludes the old front edge; Alexander's 30-min arithmetic; Law's own 'near 5 o'clock' at the late pole — spread carried, not averaged."),
      kf(T(17, 0), 3890, 2360, 80, "line", 1450, "inferred",
        "The assault leg at tier-B physics: ~1,300-1,600 m of broken valley at <=1.5 m/s ~ 20-30 min under battery fire ('Advancing rapidly across the valley ... under a heavy fire from the batteries', law-bl-1884); Law obliqued right toward the Round Tops, opening the gap that pulled the 4th/5th TX to him."),
      kf(T(17, 15), 4180, 2360, 60, "line", 1380, "documented",
        "First contact on Vincent's spur 16:30-17:00 (Norton's compiled ruling; or-27-1-rice-vincent receiving side). Brigade split the j2-03 sheet draws: '15 Ala' bar at (4345,2343) facing VINCENT'S SPUR, '44 Ala' at Plum Run (3795,2538), 4th/48th on the western slope (bachelder j2-03, +-62 m class). Centroid authored between the wings; the 15th/47th AL Great Round Top excursion [D] rides the citation, not a second track."),
      kf(T(19, 0), 4180, 2360, 60, "line", 1010, "documented",
        "The LRT fight ~16:45-19:00 (CA-J2A-6): 'he charged me five times, and was as often repulsed' (or-27-2-oates-15al) mirrored by Chamberlain's waves record — the two-sided episode count; Rice's 'It was now 8 o clock in the evening' repulse-complete clock bounds the end. Decay against the ANV return 74k/276w/146m = 496 (ED-49 scope; tablet ~550 carried)."),
      kf(EndT, 4214, 2238, 90, "line", 950, "documented",
        "Withdrawal at dusk: the 15th AL back over Great Round Top 'like a herd of wild cattle' (oates-1905; trigger: Fisher's arrivals on his rear); the brigade settled at the Round Tops' west foot — mon-csa-law-adv (4214,2238) + j2-05 'LAW'S ALA' drawn line; Scruggs 'Constructed breastworks from rock' (or-27-2-scruggs-4al)."),
    ],
  },
  {
    id: "csa-robertson",
    kfs: [
      kf(0, 2820, 2380, 70, "line", 1100, "documented",
        "Hood's front line, left of Law ('Robertson + Law front ... Anderson + Benning 200 yd rear', alexander-1907). Strength: tablet 'Present approximately 1,100' (compilation; no primary — negative recorded). Dossier csa-1c-hood-2-robertson.md."),
      kf(T(16, 30), 2820, 2380, 70, "line", 1100, "inferred",
        "Step-off with the division ~16:30 (ED-53); Robertson 'but a few minutes before we were ordered to advance' — no long in-position wait."),
      kf(T(17, 0), 3600, 2500, 75, "line", 1050, "inferred",
        "Advance to the Plum Run/Houck's Ridge front at tier-B pace; the brigade's pike-abandonment record (ordered to keep the Emmitsburg Pike, forced off it by Law's oblique) rides the leg."),
      kf(T(17, 30), 3759, 2560, 70, "line", 900, "documented",
        "Devil's Den/Houck's Ridge taken ~17:30 (CA-J2A-5; Law's 'in less than an hour' duration): the 1st TX at the three abandoned 10-pdr Parrotts — ONE convergent hands-change with Benning's Georgians and the 44th/48th AL up the gorge (ED-56; no exclusive credit encodable). mon-csa-robertson-adv (3759,2560)."),
      kf(T(18, 30), 3820, 2540, 70, "line", 640, "documented",
        "The Houck's Ridge fight's two-sided 90-minute duration agreement: Ward's 'space of one and a half hours' = Sheffield's 'the contest had continued for an hour and a half'. Decay against the return's 597 (k+w pooled-missing pattern, ED-49; tablet ~540 carried)."),
      kf(EndT, 3950, 2140, 80, "line", 570, "inferred",
        "Night line between Devil's Den and the Round Tops' foot (continuity with the July 3 file's cited t=0 ground; the return's residue read as July-3 skirmish share [D])."),
    ],
  },
  {
    id: "csa-luffman",
    kfs: [
      kf(0, 2715, 2650, 70, "line", 1800, "documented",
        "Hood's second line: tablet 'Present about 1800' (hop flagged) formed at tab-csa-anderson-gt (2715,2622); the 7th GA detached south 'to watch the Union Cavalry' before the woods fight (tablet + or-27-2-white-gta) — ~4/5 engaged. Dossier csa-1c-hood-3-anderson.md."),
      kf(T(16, 40), 2715, 2650, 70, "line", 1800, "inferred",
        "Second line committed behind the first (Alexander's en-echelon ladder; ED-53 clock frame — inferred interval)."),
      kf(T(17, 10), 3269, 2902, 75, "line", 1720, "documented",
        "Advancing on Rose's Woods: 'ANDERSON'S GA' drawn advancing at (3269,2902) (bachelder j2-03, +-62 m). Wave 1 of the ED-57 record: first advance took the stone fence, was outflanked, retired to Rose Hill (White's three-advance structure, or-27-2-white-gta)."),
      kf(T(17, 45), 3480, 2960, 80, "line", 1580, "inferred",
        "W1 stone-fence high point (trigger-seamed, not clocked — ED-57)."),
      kf(T(18, 5), 3250, 2870, 75, "line", 1520, "inferred",
        "Retired to Rose Hill between advances (White's structure); Anderson w in the second advance (wounded marker (3529,2729))."),
      kf(T(18, 45), 3500, 3000, 80, "line", 1330, "documented",
        "Third advance with the W4 renewal — 'half an hour in the ravine' (or-27-2-white-gta) beside Kershaw/Semmes/Wofford; received as 'fresh troops ... flanking us on the right and left' (Fraser, receiving side)."),
      kf(EndT, 3320, 2880, 75, "line", 1130, "documented",
        "Night on the Rose farm ground (tablet; July 3 file t=0 continuity). Decay: tablet=return exact 671 (pass-6 agreement set)."),
    ],
  },
  {
    id: "csa-benning",
    kfs: [
      kf(0, 2785, 2300, 70, "line", 1500, "documented",
        "Hood's second line, 200 yd rear (alexander-1907 drawn-interval; tab-csa-benning (2785,2269)). Strength: tablet 'Present about 1500' (hop flagged; no primary). Dossier csa-1c-hood-4-benning.md."),
      kf(T(16, 40), 2785, 2300, 70, "line", 1500, "inferred",
        "Second line stepped behind the first (ED-53 frame)."),
      kf(T(17, 10), 3350, 2480, 75, "line", 1460, "documented",
        "The wrong-brigade follow: 'The part of it in our front I took to be Law's brigade, and so I followed it. In truth, it was Robertson's' (or-27-2-benning) — the ladder's best-documented formation error, converging the second line on Devil's Den."),
      kf(T(17, 30), 3697, 2470, 70, "line", 1350, "documented",
        "The Den's hands-change ~17:30 (CA-J2A-5; ED-56): Benning found the 1st TX already at the hill, his regiments intermixed; 'three front guns ... 10-pounder Parrotts' with the enemy withdrawing 'three rear guns' — count and type agreeing with Smith's own report across the lines. mon-csa-benning-adv (3697,2461)."),
      kf(T(19, 0), 3790, 2480, 70, "line", 1010, "documented",
        "Holds Devil's Den/Houck's Ridge through the evening (drawn held on j2-04 AND j2-05 — 'BENNING GA' at the Den); decay against the three EC6 readings 400+/497/509 CARRIED (no primary strength; ED-49 pooled-missing)."),
      kf(EndT, 3790, 2480, 70, "line", 990, "documented",
        "ED-56: Benning's brigade is the HOLDER of the Den ground through July 3 (his holding record + j2-04/j2-05 drawn-state continuity)."),
    ],
  },
  // ------------------------- McLAWS'S DIVISION (rungs two and three) ------
  {
    id: "csa-kershaw",
    kfs: [
      kf(0, 2950, 3340, 80, "line", 1800, "documented",
        "Formed along the stone wall west of the Emmitsburg road after the flank march ('About 3 p.m. the head of my column came into the open field in front of a stone wall', or-27-2-kershaw — report clock profiled +90 early, ED-55; tablet 'Arrived 3:30 P.M.' a partial correction); Cabell's guns in his front. Strength: tablet 'Present about 1800' (compilation). Dossier csa-1c-mcl-1-kershaw.md."),
      kf(T(17, 30), 2950, 3340, 80, "line", 1800, "documented",
        "Step-off ~17:30 (CA-J2A-7, downstream-CONFIRMED by ED-53): Cabell's guns paused and fired THREE GUNS IN RAPID SUCCESSION as the signal (the July 2 counterpart of the July 3 signal pair); Kershaw's own '4 o'clock' clock carried as the +90 report-nominal pole (ED-55), never moving the anchor."),
      kf(T(17, 45), 3350, 3290, 90, "line", 1700, "documented",
        "Mid-advance toward the stony hill: 'KERSHAW' written along the red advance line at (3941,3263)... the SC regimental bars between the Emmitsburg road and the Rose ground (bachelder j2-03, +-62 m); his first-person interval pin: 'When we were about the Emmitsburg road I heard Barksdale's drums beat the assembly' (CA-J2A-7 -> CA-J2A-9 relative pin)."),
      kf(T(18, 10), 3450, 3250, 80, "line", 1550, "documented",
        "Wave 2 (ED-57): Kershaw+Semmes strike the stony hill; Tilton/Sweitzer withdraw on the personally-reconnoitered Rose-house-axis outflanking (ED-59: an ordered two-stage withdrawal, NOT a rout)."),
      kf(T(18, 40), 3330, 3320, 80, "line", 1380, "documented",
        "Wave 3 received: Caldwell's counterattack (CA-J2A-8, 18:00-18:15 start) presses the brigade back to the stony hill's west edge — 'KERSHAW, S.C.' at (3308,3336) with Caldwell's division drawn coming in (bachelder j2-04)."),
      kf(T(19, 10), 3450, 3280, 80, "line", 1220, "documented",
        "Wave 4 renewal with Wofford's sweep and Semmes/Anderson — four receiving-side and three attacking-side accounts of one mechanism (pass-6 record)."),
      kf(EndT, 3380, 3350, 80, "line", 1170, "documented",
        "At sunset the brigade stands on the stony hill/Rose ground (ordered back to the Peach Orchard after dark — the evening phase's leg). Decay: the wing's clean triple agreement report=return=tablet 630."),
    ],
  },
  {
    id: "csa-bryan",
    kfs: [
      kf(0, 2680, 3300, 80, "line", 1200, "documented",
        "The step-off support slot: McLaws's line 'Kershaw on the right supported by Semmes' (alexander-1907); tab-csa-semmes (2650,3318). Strength: tablet 'Present about 1200' (hop flagged; NO OR report at any echelon — verified negative). Dossier csa-1c-mcl-2-semmes.md."),
      kf(T(17, 35), 2680, 3300, 80, "line", 1200, "inferred",
        "Stepped behind Kershaw (Alexander's ladder commits 'Kershaw+Semmes' as the third partial attack; trigger-seamed)."),
      kf(T(18, 0), 3200, 3100, 90, "line", 1100, "documented",
        "Into 'the severe and protracted conflict on Rose Hill and in the ravine and forest east of there and in the vicinity of the Loop' (tablet verbatim); Semmes mw 'in the ravine near the Loop' — mon-csa-semmes-mw (3357,2864)."),
      kf(T(18, 45), 3357, 2960, 85, "line", 950, "documented",
        "The W4 general advance: 'Participated also in the general advance late in the evening by which the Union forces were forced out of the Wheatfield and across Plum Run Valley' (tablet)."),
      kf(EndT, 3280, 3080, 85, "line", 770, "documented",
        "Sunset on the Rose/Wheatfield-south ground (July 3 tablet: held the woodland south of the Wheatfield, resuming the original position at 1 P.M. July 3). Decay: tablet=return exact 430."),
    ],
  },
  {
    id: "csa-humphreys",
    kfs: [
      kf(0, 2923, 3865, 75, "line", 1598, "documented",
        "The Warfield/Seminary Ridge line: tablet 'Arrived about 3 P.M. and formed line here'; Alexander's 18 guns deployed IN FRONT of the brigade (the batteries-mixed-with-the-lines extraction delay, mclaws-shsp7-1878); j2-03 (17:00) draws the regimental numbers with advance arrows POISED at (2923,3865) — the drawn state siding with the ladder against the tablet's 'Advanced at 5 P.M.' (ED-55 skew class). Strength: tablet 'Present 1,598'. Dossier csa-1c-mcl-3-barksdale.md."),
      kf(T(18, 15), 2923, 3865, 75, "line", 1598, "documented",
        "Step-off ~18:15 (CA-J2A-9, downstream-CONFIRMED): McLaws's Lamar-carried order; Kershaw's drums pin (+20-40 min after CA-J2A-7); Alexander's 'at least 20 minutes' interval arithmetic."),
      kf(T(18, 30), 3220, 3760, 70, "line", 1450, "documented",
        "The Peach Orchard salient smashed: the assault axis through the salient angle at the Wentz corner (~(3164,3634) reference, tablet); Graham w&c on the receiving side (CA-J2A-9's Union pin)."),
      kf(T(18, 50), 3650, 3900, 60, "line", 1250, "documented",
        "Driving northeast, the regiments inclining left to Plum Run except the 21st MS (tablet narrative split; j2-04 draws the brigade AS TWO ELEMENTS — the '21' MISS' element at the Trostle ground (3617,3643) over BIGELOW; single-centroid track carries the split in this citation, not a second track)."),
      kf(T(19, 0), 4020, 4070, 60, "line", 1000, "documented",
        "The Plum Run climax ~19:00: red bar against 'WILLARD's Brig' at (4053,4080) (bachelder j2-04); Barksdale mw here (tablet narrative + Hall's 20-yards receiving statement, compatible at ~100 m grain; died July 3 at the Hummelbaugh farm)."),
      kf(EndT, 3820, 3960, 250, "line", 851, "documented",
        "Repulsed by Willard's counterattack, falling back westward at dark (both-sided: us-ii-3-3-willard.md). Decay: tablet 105/550/92 = 747 — the tablet that itemizes the return's pooled missing EXACTLY (pass-5 find; ED-43 satisfied)."),
    ],
  },
  {
    id: "csa-wofford",
    kfs: [
      kf(0, 2680, 3790, 80, "line", 1350, "documented",
        "Formed 100 yards west of tab-csa-wofford (2652,3802) — tablet 'Arrived at 4 P.M. and formed line' (W Confederate Ave); McLaws's line 'Barksdale on the left supported by Wofford'. Strength: tablet 'Present about 1350' (hop flagged; NO OR report — the Semmes/Barksdale class). Dossier csa-1c-mcl-4-wofford.md."),
      kf(T(18, 25), 2680, 3790, 80, "line", 1350, "documented",
        "'Ordered to the front about 6 P.M. and advanced soon afterward along Wheatfield Road' (tablet — the near-zero-skew exemplar of the wing's tablet class, ED-58 discussion): the wave-4 trigger axis."),
      kf(T(18, 45), 3350, 3480, 95, "line", 1280, "documented",
        "The Wheatfield-Road sweep 'flanked the Union forces assailing the Loop' (tablet) — W4's mechanism, received as 'fresh troops ... flanking us on the right and left' (Fraser) and 'flanks threatened ... all would be killed or captured' (Brooke)."),
      kf(T(19, 10), 4050, 3020, 100, "line", 1120, "documented",
        "The farthest line: 'forcing them back through the Wheatfield to the foot of Little Round Top' (tablet); meets the Plum Run counter-line (W5: McCandless/Nevin) at the recall."),
      kf(EndT, 3900, 3060, 260, "line", 995, "documented",
        "'Assailed there by a strong body of fresh troops and receiving at the same moment an order to withdraw the Brigade fell back at sunset' (tablet verbatim) — CA-J2A-11, the ladder's only astronomically pinned end clock (19:29 LMT); j2-05 draws the withdrawal arrows on the Wheatfield's west ravine slope. Decay: the 334-vs-355 tablet-vs-return composition conflict CARRIED (missing agrees at 112; T3 honesty, pass 6)."),
    ],
  },
  // ------------------- ANDERSON'S DIVISION (the echelon runs north) -------
  {
    id: "csa-wilcox",
    kfs: [
      kf(0, 3130, 4450, 80, "line", 1777, "documented",
        "Anderson's line on Seminary Ridge (the brigade's ridge ground; July 3 file continuity). Strength: 1,777 engaged (or-27-2-wilcox primary — the 1,777-577 arithmetic verified in the re-basing record). Dossier csa-3c-and-1-wilcox.md (July 2 arc extended pass 6)."),
      kf(T(18, 20), 3130, 4450, 80, "line", 1777, "inferred",
        "Step-off in echelon after Barksdale (CA-J2A-9 + the division's en-echelon order; trigger-seamed, inferred clock)."),
      kf(T(18, 45), 3750, 4400, 85, "line", 1650, "documented",
        "Across the Emmitsburg road, driving Humphreys's division ('CARR.'/'HUMPHREYS' DIV.' redrawn with NE withdrawal arrows, bachelder j2-04)."),
      kf(T(19, 0), 4112, 4368, 80, "line", 1520, "documented",
        "The Plum Run ravine at ~19:00: 'WILCOX ALA.' drawn at (4112,4368) vs '1' Minn' at (4230,4282) (bachelder j2-04) — the 1st Minnesota collision, both-sided at regiment grain: Coates's double-quick charge meets Wilcox's 'a third line descended at a double-quick ... withdrew after some thirty minutes'."),
      kf(EndT, 3950, 4420, 260, "line", 1404, "documented",
        "Withdrawing at dark after the ~30-minute stand (or-27-2-wilcox; the ravine-advance geometry flag EC3.3 carried). Decay: July 2 share 373 of the 577 battle total (or-27-2-wilcox primary; July 3 204, Alexander concurs)."),
    ],
  },
  {
    id: "csa-lang",
    kfs: [
      kf(0, 3190, 4640, 80, "line", 700, "documented",
        "Perry's Florida Brigade under Lang, Anderson's line left of Wilcox (July 3 file ridge continuity). Strength: 700 battle basis (or-27-2-lang primary, the re-basing record). Dossier csa-3c-and-4-lang.md (July 2 arc extended pass 7)."),
      kf(T(18, 25), 3190, 4640, 80, "line", 700, "inferred",
        "Stepped with Wilcox's left (echelon order; Lang's own July 2 clocks profiled +45..+60 early, ED-58 — never moving the seam)."),
      kf(T(19, 0), 4160, 4502, 80, "line", 540, "documented",
        "Advanced ground west of the Emmitsburg road, 1,100 ft south of Codori — the Perry advanced-ground tablet ('PERRY FLA.' drawn at (4160,4502), bachelder j2-04; distance check exact): 'The color bearer of the 8th Florida fell and its flag was lost' — the captor named on the Union side (Sgt. Thomas Hogan, 3rd Excelsior; Brewster's report + tablet)."),
      kf(EndT, 3700, 4570, 260, "line", 400, "documented",
        "Withdrawn at dark to the ridge line. Decay: July 2 share ~300 of the 700 basis (or-27-2-lang; the 400 July 3 basis confirmed by the return exactly — the re-basing record)."),
    ],
  },
  {
    id: "csa-wright",
    kfs: [
      kf(0, 3060, 4830, 85, "line", 1250, "documented",
        "Anderson's line, left of Lang (ridge ground). Strength: no primary — the two-hop ~1,250 compilation CARRIED AS FLAGGED (dossier csa-3c-and-3-wright.md EC2; ED-60's EC6 basis is the return's 668)."),
      kf(T(18, 40), 3060, 4830, 85, "line", 1250, "inferred",
        "Step-off in echelon after Lang (trigger-seamed; Wright's own clock profiled +100 early, ED-58)."),
      kf(T(19, 0), 3865, 4773, 85, "line", 1100, "documented",
        "Across the Emmitsburg road ('WRIGHT GA.' drawn west at (3865,4773), bachelder j2-04); Codori ground on his left front."),
      kf(T(19, 15), 4289, 4852, 85, "line", 980, "documented",
        "THE HIGH-WATER (ED-60): farthest advance = the Emmitsburg-road-to-west-slope gun line fixed by the advance marker (4330.6,4746.9), the j2-04 drawn state (red bars ON Brown's battery ground, 28 m agreement), and the receiving side (Brown's and Weir's temporary gun losses; the Hall/Harrow/Webb ridge fronts unbroken). The report's 'complete masters of the field'/'reached the crest' claims are the commander's account THAT THE TABLET INHERITED — carried, not adopted. Never on the crest."),
      kf(EndT, 3700, 4850, 265, "line", 800, "documented",
        "Falling back at dusk — both flanks in the air (his report's cause, carried); the withdrawal completes to Seminary Ridge in the evening phase. Decay toward the return's 668 (report 688 and tablet 873 carried; missing = 333 agreed exactly, ED-60)."),
    ],
  },
  {
    id: "csa-posey",
    kfs: [
      kf(0, 2950, 5250, 85, "line", 1150, "inferred",
        "Anderson's line, left of Wright (July 3 ground continuity; strength compilation-tier, hop flagged — dossier csa-3c-and-5-posey.md)."),
      kf(T(18, 0), 3300, 5230, 85, "skirmish", 1130, "documented",
        "The partial advance: regiments forward to the Bliss farm ground as skirmish lines, never the ridge ('Bliss farm skirmishing July 2-3', register/tablet; the brigade's July 2 is the salient-north echelon's documented fade)."),
      kf(EndT, 3050, 5240, 85, "line", 1100, "documented",
        "Back on the ridge line at dark (tablet trench-line ground). July 2 loss small [D], carried against the return's brigade block."),
    ],
  },
  // ------------------- THE BENNER'S HILL DUEL + JOHNSON'S APPROACH --------
  {
    id: "csa-bn-raine", frontage: 300, depth: 30,
    kfs: [
      kf(0, 6520, 6300, 210, "column", 384, "inferred",
        "Andrews's battalion under Latimer, parked NE of town awaiting Johnson's 16:00 order (or-27-2-johnson; Jones's 'about 300 yards in rear and to the left of the battalion' support offset fixes the ground class)."),
      kf(T(16, 0), 6329, 6397, 245, "line", 384, "documented",
        "Benner's Hill, ~16:00: the duel opens — Geary's 4 p.m. Knap's/K-5th counter-duel = Johnson's 4 p.m. Latimer order (the cross-army clock agreement, pass 8). tab-csa-2c-bn-latimer (6329,6397). Dossier csa-2c-arty-ewell-wing.md."),
      kf(T(17, 30), 6329, 6397, 245, "line", 350, "documented",
        "Wrecked in the ~90-minute duel: Latimer mw ('the boy major'); the battalion's Benner's Hill ledger 10k/40w = 50 by battery (OR 27/2 pp. 341-343, the pass-5 closure)."),
      kf(T(18, 0), 6520, 6300, 245, "column", 334, "documented",
        "Withdrawn by Latimer's own dying advice (four guns excepted to cover Johnson's advance); the battalion's July 2 is finished (wrecked-prior class, ED-45)."),
      kf(EndT, 6520, 6300, 245, "column", 334, "documented",
        "Parked; the surviving 20-pdr section's July 3 cannonade window is the July 3 file's record."),
    ],
  },
  {
    id: "csa-dungan",
    kfs: [
      kf(0, 6450, 6500, 210, "line", 1600, "documented",
        "Jones's brigade 'halted under cover of a range of low hills, about 300 yards in rear and to the left of the battalion of artillery' (or-27-2-jones-jm — the rare stated battery-support offset; Benner's Hill tablet (6329,6397)). Strength: tablet 'Present 1600' (tablet-adjudicated hop, pass 10). Dossier csa-2c-joh-4-jones.md."),
      kf(T(18, 40), 6450, 6500, 210, "line", 1600, "inferred",
        "The division advances at dark's approach (Johnson's dark-at-Rock-Creek statement brackets the climb; trigger-seamed)."),
      kf(T(19, 0), 6100, 5900, 230, "line", 1590, "documented",
        "The assault strikes ~19:00 (CA-J2E-2, ED-62): Greene 'a few minutes before 7 p. m.' vs Nicholls's '7 p. m. of July 2 ... ordered forward' — the cross-line minute-class pair; Jones's lane crosses the bottom and climbs 'the hill ... steep, heavily timbered, rocky' against Greene's right (tab-csa-2c-joh-4 (5800,5866); j2-05 approach label (6312,5783) — both semantics recorded)."),
      kf(EndT, 5960, 5850, 235, "line", 1560, "documented",
        "On the east face at sunset, the first assault climbing under fire ('darkness in the woods', or-27-2-jones-jm); Jones w this evening (the pass-8 register correction). The fight continues in the evening phase."),
    ],
  },
  {
    id: "csa-williams",
    kfs: [
      kf(0, 6350, 6350, 215, "line", 1100, "documented",
        "Nicholls's brigade re-formed 'to the left of Jones' brigade' before the advance (or-27-2-williams-nicholls). Strength: tablet 'Present about 1100' (tablet-adjudicated hop, pass 10). Dossier csa-2c-joh-3-nicholls.md."),
      kf(T(18, 40), 6350, 6350, 215, "line", 1100, "inferred",
        "Advance with the division at dark's approach (trigger-seamed)."),
      kf(T(19, 0), 6120, 5820, 230, "line", 1095, "documented",
        "'7 p. m. of July 2 ... ordered forward' (or-27-2-williams-nicholls — the first CSA report clock agreeing on the evening chain; CA-J2E-2 both-sided)."),
      kf(EndT, 6030, 5780, 235, "line", 1070, "documented",
        "Climbing toward the fire line 'about 100 yards from the enemy's works' (his night line — the evening phase's ground; tab-csa-2c-joh-3 (5968,5748))."),
    ],
  },
  {
    id: "csa-steuart",
    kfs: [
      kf(0, 6500, 6200, 215, "line", 1700, "documented",
        "Johnson's left brigade NE of town ('our front facing the south and our left wing in a skirt of woods'). Strength: tablet 'Present about 1700' (tablet-adjudicated hop, pass 10). Dossier csa-2c-joh-1-steuart.md."),
      kf(T(18, 15), 6350, 5800, 225, "line", 1700, "documented",
        "The approach: half-wheel to front 'west by south', Rock Creek crossed right-wing-first ('Crossing Rock Creek at 6 P.M.', the R1-verified tablet clock — riding ED-29's envelope note: crossing != contact); tab-csa-2c-joh-1 (6193,5287) fixes the lane's south end."),
      kf(T(19, 5), 6180, 5450, 240, "line", 1695, "documented",
        "Across the creek and up the slope ('about 50 feet from the bank', report) toward the LOWER hill — the works the XII Corps vacated at 18:30 (CA-J2E-1: Greene's 'until 6.30 p. m.' primary)."),
      kf(EndT, 6080, 5350, 245, "line", 1680, "documented",
        "At sunset the brigade is on the lower slope; the vacated-works lodgment (~20:00, the ED-57-class occupation that HOLDS overnight) is the evening phase's record."),
    ],
  },
  {
    id: "csa-walker",
    kfs: [
      kf(0, 7050, 6100, 180, "line", 1450, "documented",
        "The Stonewall Brigade detained on the Hanover-road flank by Walker's own discretion-order record (or-27-2-walker-stonewall: ordered to follow the division 'unless circumstances rendered it necessary to remain' — the circumstances were Gregg's cavalry on Brinkerhoff's Ridge). Composition consequence: Johnson attacked with THREE brigades (ED-62). Strength: tablet-class ~1,450 (hop flagged). Dossier csa-2c-joh-2-walker.md."),
      kf(T(19, 0), 7050, 6100, 180, "line", 1440, "documented",
        "Held the flank through the evening skirmish (the 2nd VA's handful — the brigade's lightest row IS the July 2 apportionment; negative evidence as evidence, ED-44 class)."),
      kf(EndT, 6900, 6050, 200, "line", 1440, "inferred",
        "Turning toward the division at dark (rejoins on Culp's Hill overnight — the July 3 morning record)."),
    ],
  },
  // ------------------- THE UNION LEFT: V CORPS ----------------------------
  {
    id: "us-rice",
    kfs: [
      kf(0, 4750, 3350, 260, "column", 1336, "documented",
        "Vincent's brigade with Barnes's division in V Corps reserve east of the Wheatfield Road. Strength: 'about 1,000 muskets' (or-27-1-rice-vincent primary) beside the ~1,336 all-ranks compilation — two scopes CARRIED (dossier us-v-1-3-vincent.md)."),
      kf(T(16, 45), 4750, 3350, 260, "column", 1336, "documented",
        "Vincent intercepts Warren's call and takes the brigade to Little Round Top on his own responsibility (the rush — or-27-1-rice-vincent; Norton 1913)."),
      kf(T(17, 0), 4265, 2400, 230, "line", 1336, "documented",
        "The spur line, minutes before first contact (16:30-17:00, Norton's compiled ruling): 44th NY (4253,2453) - 140th NY ground (4257,2476) - 20th ME refused left (4322,2246); Vincent mw at the 16th MI crisis (mon-vincent-mw (4222,2393); died July 7)."),
      kf(T(19, 0), 4265, 2400, 230, "line", 1010, "documented",
        "The fight held: five charges received and repulsed (Oates's own count = Chamberlain's waves record, the two-sided episode agreement); the 20th ME bayonet charge; Rice's 'It was now 8 o clock in the evening' (report clock, +45 profiled) bounds the repulse-complete."),
      kf(EndT, 4265, 2400, 230, "line", 984, "documented",
        "Holding the spur at sunset (the 20th ME's Great Round Top seizure at ~21:00, Chamberlain's order + the j2-05 drawn line, is the evening phase's record). Decay: 352 (return; the brigade's July 3 is at the center, ED-54 — different day, different ground)."),
    ],
  },
  {
    id: "us-garrard",
    kfs: [
      kf(0, 4750, 3250, 260, "column", 1490, "documented",
        "Weed's brigade with Ayres's division in reserve (dossier us-v-2-3-weed.md; strength compilation-tier — no brigade primary, flagged)."),
      kf(T(17, 30), 4750, 3250, 260, "column", 1490, "documented",
        "Ordered to the hill as Vincent's fight opens: O'Rorke's 140th NY diverted straight up the slope by Warren (Garrard + Farley; O'Rorke mw at the head of the regiment — mon-140ny (4257,2476))."),
      kf(T(17, 45), 4275, 2560, 240, "line", 1450, "documented",
        "The summit line ('WEED' bar with the signal-station circle at (4169,2640), bachelder j2-03; mon-weed-hazlett (4295,2546)): Weed mw near the battery with Hazlett mw OVER him — both pins in one primary (Garrard); Weed's death ~21:00 rides the evening record."),
      kf(EndT, 4275, 2560, 240, "line", 1290, "documented",
        "Holds the summit to sunset and through July 3 (the hill's garrison after Vincent's relief, ED-54). Decay: brigade 200 canonical (the pass-6 re-read closure: 140th NY 133 + 146th 28 + 91st 19 + 155th 19 + staff)."),
    ],
  },
  {
    id: "us-btty-rittenhouse", frontage: 100, depth: 30,
    kfs: [
      kf(0, 4900, 3300, 250, "column", 73, "inferred",
        "Hazlett's D/5th US with the V Corps column (dossier us-btty-hazlett.md; personnel compilation-tier)."),
      kf(T(17, 30), 4270, 2560, 250, "line", 73, "documented",
        "Hauled by hand to the Little Round Top summit (~17:30, with Weed's arrival; the pass-5 anchor revision (4200,2600)+-80 m; mon-weed-hazlett ground): 'the six guns of Hazlett appeared on the crest'. Hazlett mw over the dying Weed (Garrard — one primary, both pins)."),
      kf(EndT, 4270, 2560, 250, "line", 68, "documented",
        "Fires over the fight to sunset; Rittenhouse succeeds (the July 3 file's command name). Rounds-expended ledger open (MOLLUS terminal — dossier honesty note)."),
    ],
  },
  {
    id: "us-tilton",
    kfs: [
      kf(0, 4650, 3250, 260, "column", 654, "documented",
        "Barnes's 654 officers-and-men figure (or-27-1-barnes-frame; Tilton's own 474 carried as the after-loss scope — two-scope conflict, dossier us-v-1-1-tilton.md)."),
      kf(T(17, 20), 3400, 3330, 210, "line", 654, "documented",
        "The stony hill: 'BARNES' DIV' with Tilton/Sweitzer bars ON the hill (bachelder j2-03 (3394,3301)); two repulses inflicted on Kershaw's front (his report; 125 casualties by the return)."),
      kf(T(17, 55), 3320, 3450, 240, "line", 590, "documented",
        "ED-59: the withdrawal's CAUSE is the Barksdale-axis outflanking of the salient (Tilton's personally-reconnoitered flanking force 'from the direction of Roses house'); an ORDERED two-stage withdrawal — de Trobriand's 'had fallen back without engaging' accusation CARRIED as his honest view from a front the V Corps line did not cover. Never rendered as a rout."),
      kf(T(18, 20), 3520, 3560, 250, "line", 545, "documented",
        "Second stage into the Trostle woods (his own leg record)."),
      kf(EndT, 4150, 3250, 240, "line", 529, "inferred",
        "Regrouped toward the corps' night ground north of Little Round Top (V Corps frame). Decay: 125 (return; tablet=return class)."),
    ],
  },
  {
    id: "us-sweitzer",
    kfs: [
      kf(0, 4620, 3300, 260, "column", 1010, "documented",
        "Sweitzer's brigade (strength two-scope: ~1,010 minus the 9th MA detached — carried flagged, dossier us-v-1-2-sweitzer.md)."),
      kf(T(17, 20), 3350, 3230, 210, "line", 1010, "documented",
        "Stony hill left of Tilton (bachelder j2-03 bars)."),
      kf(T(17, 55), 3560, 3420, 240, "line", 940, "documented",
        "Withdrew with Tilton under the same outflanking (ED-59; the Prescott conditional-order exchange in his report)."),
      kf(T(18, 25), 3706, 2988, 190, "line", 900, "documented",
        "THE SECOND ADVANCE at Caldwell's personal request (recorded on both sides of the command seam, pass 6): 'Sweitzer's Brigade' drawn ON the stone fence (3706,2988) = 40 m-class agreement with the 4th MI monument (3678,3012) (bachelder j2-04)."),
      kf(T(19, 5), 3706, 2988, 190, "line", 660, "documented",
        "The W4 fence crisis: Jeffords k with the colors (mon-4mi); the 4th MI/62nd PA capture pocket (165/175 rows)."),
      kf(EndT, 4100, 3130, 240, "line", 583, "documented",
        "Extracted east across Plum Run at the collapse; sunset near the corps' ground. Decay: 427 (return)."),
    ],
  },
  {
    id: "us-day",
    kfs: [
      kf(0, 4700, 3150, 250, "column", 1574, "documented",
        "Day's US regulars (strength 1,574 B&M-type hop, flagged; 'not called on myself for a report' — the forwarding-cover negative, dossier us-v-2-1-day.md)."),
      kf(T(18, 20), 3912, 2814, 260, "line", 1574, "documented",
        "Ayres's division across Plum Run to the Wheatfield's east edge (the regulars' bar column (3912,2814) under 'AYRES' DIV.' (3968,2895), bachelder j2-04)."),
      kf(T(19, 0), 4130, 2760, 250, "line", 1300, "documented",
        "THE W4 EXTRACTION: 'the enemy was seen ... moving through a wheat-field to our rear' — both-flank fire on the recrossing (Burbank's mechanism primary; Day's cover concurs)."),
      kf(EndT, 4280, 2700, 245, "line", 1192, "documented",
        "Re-formed on Little Round Top's north slope. Decay: tablet=return exact 382 (pass-7 agreement set)."),
    ],
  },
  {
    id: "us-burbank",
    kfs: [
      kf(0, 4720, 3100, 250, "column", 958, "documented",
        "Burbank's regulars: 'went into action with less than 900 muskets' (or-27-1-burbank primary) beside the 958 all-ranks reproduction — both carried (dossier us-v-2-2-burbank.md)."),
      kf(T(18, 20), 3890, 2780, 260, "line", 958, "documented",
        "Into the valley with Day (bachelder j2-04 bar column)."),
      kf(T(19, 0), 4110, 2730, 250, "line", 640, "documented",
        "The recrossing under both-flank fire: 40 of 80 officers; 447 of <900 muskets ~ 50% — the pass-7 set's highest rate (or-27-1-burbank)."),
      kf(EndT, 4260, 2670, 245, "line", 511, "documented",
        "Re-formed on the north slope. Decay: tablet=return exact 447."),
    ],
  },
  {
    id: "us-mccandless",
    kfs: [
      kf(0, 4550, 3050, 250, "column", 1243, "documented",
        "McCandless's PA Reserves behind the hill (strength 1,243 B&M-type hop, flagged; dossier us-v-3-1-mccandless.md)."),
      kf(T(19, 10), 4300, 3060, 250, "line", 1243, "documented",
        "Formed on the Plum Run slope as Wofford's line reaches the foot of the hill — wave 5's trigger (ED-57)."),
      kf(EndT, 4016, 3000, 260, "line", 1088, "documented",
        "THE W5 CHARGE to the stone wall at the Wheatfield's east edge: the 700-yards-to-the-wall charge geometry primary (or-27-1-crawford/-mccandless); Taylor (Bucktails) k in the charge; 'McCANDLESS BRIGADE' drawn at (4011,3016) against the retiring red (bachelder j2-05). Sunset ends the ladder (CA-J2A-11). Decay: tablet=return exact 155; Crawford's '20 officers and 190 men' = McCandless 155 + Fisher 55 EXACT."),
    ],
  },
  {
    id: "us-fisher",
    kfs: [
      kf(0, 4600, 2950, 250, "column", 1610, "documented",
        "Fisher's PA Reserves with Crawford (strength compilation-tier; dossier frame via us-v-3-1-mccandless.md and or-27-1-crawford)."),
      kf(T(19, 15), 4330, 2500, 220, "line", 1580, "documented",
        "Moved to the Round Tops' right as the counter-line formed (Crawford; the two regiments Oates saw moving on his rear were Fisher's arrivals — the withdrawal trigger in or-27-2-oates-15al, cross-sided)."),
      kf(EndT, 4280, 2320, 210, "line", 1555, "documented",
        "At sunset on the spur's south shoulder; the Great Round Top night ascent with the 20th ME (~21:00) is the evening phase's record. Decay: Fisher 55 (Crawford's arithmetic, exact)."),
    ],
  },
  // ------------------- VI CORPS (the 34-mile arrivals) --------------------
  {
    id: "us-nevin",
    kfs: [
      kf(0, 5850, 3100, 300, "column", 1368, "documented",
        "Wheaton's/Nevin's brigade at the Baltimore Pike after the corps' forced march: 'marched nearly 34 miles within seventeen hours', arriving ~14:00 (or-27-1-nevin primary — the strength's fatigue qualifier; 1,368 B&M-type hop flagged). Dossier us-vi-3-3-nevin.md."),
      kf(T(19, 0), 4500, 3150, 260, "column", 1368, "documented",
        "Brought to the left as the salient collapsed (Wheaton to division command; the triple-succession metadata hazard flagged on the dossier)."),
      kf(T(19, 15), 4360, 3110, 260, "line", 1360, "documented",
        "Formed beside McCandless on the Plum Run line — wave 5 (ED-57)."),
      kf(EndT, 4300, 3090, 260, "line", 1315, "documented",
        "The W5 countercharge: the tablet NAMES Wofford as the object; 'two light 12-pounder brass pieces' recovered in the charge (ED-61 claimant ledger — one convergent recovery, no exclusive credit); 'NEVIN' drawn at (4306,3075) (bachelder j2-05). Decay: 53 = return row EXACT (pass-8 closure)."),
    ],
  },
  // ------------------- III CORPS: THE SALIENT -----------------------------
  {
    id: "us-madill",
    kfs: [
      kf(0, 3405, 3719, 250, "line", 1516, "documented",
        "Graham's brigade at the Peach Orchard apex, drawn TWO-FRONTED ('Graham's Brig.' (3405,3719) with Clark (3408,3605)/Phillips (3518,3543) on the south face — bachelder j2-03). Strength 1,516 (B&M-type hop flagged; NO brigade report exists — page-probed negative). Dossier us-iii-1-1-graham.md."),
      kf(T(18, 15), 3405, 3719, 250, "line", 1400, "documented",
        "Under the converging bombardment since ~15:45; Barksdale's step-off (CA-J2A-9) opens the apex's crisis."),
      kf(T(18, 40), 3560, 3860, 240, "line", 1050, "documented",
        "The salient smashed; Graham w&c (tablet verbatim + Birney + Tippin's inside-the-seam moment — CA-J2A-9's Union pin opposite Barksdale's mw); Sickles mw nearby at 18:00-18:30 ('At 6 o'clock I found Major-General Sickles seriously wounded', Birney; marker (3789.9,3715.2))."),
      kf(T(19, 10), 4150, 4150, 250, "line", 830, "inferred",
        "Falling back toward Cemetery Ridge with the corps (fighting withdrawal frame — never authored as a rout; the division rallied, j2-04)."),
      kf(EndT, 4280, 4230, 255, "line", 776, "documented",
        "Rallied at dark. Decay: tablet=return exact 740 (pass-7 agreement set)."),
    ],
  },
  {
    id: "us-carr",
    kfs: [
      kf(0, 3826, 4331, 260, "line", 1718, "documented",
        "Carr's brigade on the Emmitsburg road line ('Carr's Brig' (3826,4331) with 'P. ROGERS' drawn at (3724,4358) — 28 m from the modern house marker; the 1st MA a separate forward skirmish element (3463,4407); bachelder j2-03). Carr's minute-precision '4.08 p.m.' advance clock (profiled ~0 — the division-grain counter-class to ED-58). Strength 1,718 (B&M-type hop). Dossier us-iii-2-1-carr.md."),
      kf(T(18, 30), 3826, 4331, 260, "line", 1600, "documented",
        "Wilcox/Lang strike the road front (CA-J2A-10 frame)."),
      kf(T(19, 0), 4150, 4400, 260, "line", 1200, "documented",
        "The fighting withdrawal: 'CARR.'/'HUMPHREYS' DIV.' REDRAWN with NE withdrawal arrows AND the division rallied on the ridge — both ends of the withdrawal in one drawn window (bachelder j2-04)."),
      kf(T(19, 15), 4488, 4469, 260, "line", 990, "documented",
        "Rallied on Cemetery Ridge (4488,4469) (bachelder j2-04; Humphreys's stated 2,088 division loss vs the return's 2,092 — ED-52's cleanest exercise)."),
      kf(EndT, 4488, 4469, 260, "line", 928, "documented",
        "Holds the rallied line. Decay: tablet=return exact 790."),
    ],
  },
  {
    id: "us-brewster",
    kfs: [
      kf(0, 3591, 3855, 250, "line", 1837, "documented",
        "Brewster's Excelsior brigade ('Brewster's Brig' (3591,3855), bachelder j2-03). Strength 1,837 (B&M-type hop). Dossier us-iii-2-2-brewster.md."),
      kf(T(18, 25), 3591, 3855, 250, "line", 1700, "documented",
        "Barksdale's axis strikes; the brigade fights the salient's north shoulder."),
      kf(T(19, 5), 4100, 4150, 255, "line", 1250, "documented",
        "Forced back toward the ridge; the 8th FL colors taken in the fighting withdrawal (Sgt. Thomas Hogan, 3rd Excelsior — captor named in the report + tablet, conceded on the CSA advanced-ground tablet)."),
      kf(EndT, 4300, 4300, 255, "line", 1059, "documented",
        "Rallied at dark (the after-sunset gun-recapture leg is the evening phase's record, ED-61 ledger). Decay: report=tablet=return TRIPLE 778."),
    ],
  },
  {
    id: "us-burling",
    kfs: [
      kf(0, 3720, 4020, 250, "column", 1365, "documented",
        "Burling's brigade in reserve — THE DISPERSED CLASS: five of six regiments detached to three fronts before the crisis ('The Eighth New Jersey Volunteers was taken from me without my knowledge' — command friction as a primary); the drawn record confirms the ledger on the hosts' grounds (7' N.J./4' Ex on Graham's apex; 5' N.J. at Seeley's — bachelder j2-04). The centroid is a LEDGER SUM, not a maneuver body (dossier us-iii-2-3-burling.md EC4). Strength 1,365 (hop)."),
      kf(T(18, 45), 3800, 4050, 250, "column", 1100, "documented",
        "The hosts' fronts collapse; the brigade's casualty curve is the sum of five host curves (the dossier's authoring-note class)."),
      kf(EndT, 4250, 4250, 255, "line", 852, "documented",
        "Withdrawn with the corps at dark. Decay: report=tablet=return TRIPLE 513."),
    ],
  },
  // ------------------- II CORPS: CALDWELL + WILLARD + HARROW --------------
  {
    id: "us-mckeen",
    kfs: [
      kf(0, 4420, 4480, 260, "line", 780, "documented",
        "Cross's brigade on the II Corps left: '330 total from 780 muskets' — the strength+loss pairing primary (or-27-1-mckeen). Dossier us-ii-1-1-cross.md."),
      kf(T(17, 50), 4420, 4480, 260, "column", 780, "documented",
        "Caldwell's division ordered to the Wheatfield (the W3 march; trigger: de Trobriand's ammunition running out AS Caldwell arrives — seamed, not clocked)."),
      kf(T(18, 5), 3862, 3088, 210, "line", 760, "documented",
        "Wave 3 opens (CA-J2A-8, CONFIRMED 18:00-18:15): Cross into the wheat — mw at the opening (5th NH ground; fall-site tradition-fixed, flagged); 'Cross' Brig' drawn east Wheatfield (3862,3088) (bachelder j2-04)."),
      kf(T(19, 0), 3980, 3150, 220, "line", 500, "documented",
        "W4 received: the flanking collapse extracts the division (Fraser/Brooke receiving-side mechanism)."),
      kf(EndT, 4380, 4300, 255, "line", 450, "documented",
        "Rallied toward the ridge at dark. Decay: report=return exact 330."),
    ],
  },
  {
    id: "us-kelly",
    kfs: [
      kf(0, 4400, 4420, 260, "line", 530, "documented",
        "Kelly's Irish Brigade: '530 brought into action' (or-27-1-kelly primary; the Wheatfield chaplain's absolution rides the record). Dossier us-ii-1-2-kelly.md."),
      kf(T(17, 50), 4400, 4420, 260, "column", 530, "documented", "The W3 march with Caldwell."),
      kf(T(18, 10), 3560, 3200, 220, "line", 520, "documented",
        "Onto the stony hill's rocks — the 45-minute duration primary (or-27-1-kelly, +70 profiled clock carried per ED-58; the anchor is the wave seam)."),
      kf(T(18, 55), 3700, 3280, 230, "line", 380, "documented",
        "W4: extracted under the double-flank threat."),
      kf(EndT, 4360, 4280, 255, "line", 332, "documented",
        "Rallied at dark. Decay: return 198 (Kelly's 202 carried — ED-52 exemplar class)."),
    ],
  },
  {
    id: "us-fraser",
    kfs: [
      kf(0, 4440, 4520, 260, "line", 975, "documented",
        "Zook's brigade (strength ~975 B&M-type, hop flagged — bracketed by the division arithmetic; dossier us-ii-1-3-zook.md)."),
      kf(T(17, 50), 4440, 4520, 260, "column", 975, "documented", "The W3 march with Caldwell."),
      kf(T(18, 10), 3680, 3290, 220, "line", 950, "documented",
        "Committed on the division right via the Wheatfield Road; Zook mw at the start (mon-zook-statue (3791,3264); the stated brigade line-order in Fraser's report)."),
      kf(T(18, 55), 3750, 3320, 230, "line", 700, "documented",
        "W4 received: 'fresh troops ... flanking us on the right and left' (or-27-1-fraser — the mechanism verbatim)."),
      kf(EndT, 4390, 4340, 255, "line", 617, "documented",
        "Rallied at dark. Decay: return 358."),
    ],
  },
  {
    id: "us-brooke",
    kfs: [
      kf(0, 4380, 4380, 260, "line", 850, "documented",
        "Brooke's brigade (strength ~850 B&M-type, hop flagged; dossier us-ii-1-4-brooke.md)."),
      kf(T(17, 55), 4380, 4380, 260, "column", 850, "documented", "The W3 march with Caldwell."),
      kf(T(18, 15), 3750, 3120, 220, "line", 840, "documented",
        "Through Cross's line into the wheat (his own sequence record)."),
      kf(T(18, 35), 3460, 3180, 230, "line", 720, "documented",
        "THE DEEPEST WHEATFIELD PENETRATION — the Rose-ravine crest (his tablet stands on the farthest-advance ground; 'Brooke's Brig' at the apex (3503,3274), bachelder j2-04)."),
      kf(T(19, 0), 3800, 3200, 235, "line", 530, "documented",
        "W4: 'flanks threatened ... all would be killed or captured' (or-27-1-brooke verbatim) — extracted; Brooke w ('severely bruised', his own words)."),
      kf(EndT, 4340, 4260, 255, "line", 461, "documented",
        "Rallied at dark. Decay: return 389."),
    ],
  },
  {
    id: "us-sherrill",
    kfs: [
      kf(0, 4540, 5115, 260, "line", 1510, "documented",
        "Willard's brigade on Hays's division front (build-adoption strength 1,510, hop flagged; the brigade report's attested-unreported loss is a first-class negative — dossier us-ii-3-3-willard.md)."),
      kf(T(18, 50), 4540, 5115, 260, "column", 1500, "documented",
        "Sent south as the III Corps line collapsed (Hancock's shore-up of the gap)."),
      kf(T(19, 5), 4150, 4150, 240, "line", 1480, "documented",
        "The counterattack line at Plum Run — 'WILLARD's Brig' drawn against Barksdale's red bar (4053,4080) (bachelder j2-04); the charge stops the axis; Barksdale mw in front."),
      kf(EndT, 4180, 4160, 240, "line", 1290, "documented",
        "At sunset the line holds; Willard k by shell returning from the charge (mon-willard-kia (4116,4135)). July 2 decay share [D]: the 39th NY skirmish 28 exact + the charge's share of the tablet's 714 battle total (bracketed, carried)."),
    ],
  },
  // Harrow's brigade — FULL DECOMPOSITION for July 2 (the record is
  // regiment-grain: the Codori detachment pair + the 1st MN charge).
  {
    id: "us-harrow", name: "Harrow's Brigade (2nd Bde, 2nd Div, II Corps)",
    kfs: [
      kf(0, 4350, 4560, 260, "line", 1410, "documented",
        "Harrow's brigade on the II Corps line south of the copse (build adoption 1,410, hop flagged; division primary: 'The division took into action 3,773 men', or-27-1-harrow). July 2 record is regiment-grain — decomposed below (dossier us-ii-2-1-harrow.md)."),
      kf(T(18, 40), 4350, 4560, 260, "line", 1330, "documented",
        "The road-front detachments and the ridge line under the assault (Ward mw, Huston mw at the Codori ground)."),
      kf(EndT, 4340, 4550, 260, "line", 1050, "documented",
        "At dark the brigade re-forms on the ridge. July 2 share of the two-day 722 k+w (report primary; tablet 768 carried) [D, strong episode split]."),
    ],
  },
  {
    id: "us-15ma", parent: "us-harrow", name: "15th Massachusetts (Harrow's)", side: "union",
    kfs: [
      kf(0, 4055, 4620, 260, "line", 240, "documented",
        "Advanced to the Codori ground with the 82nd NY — the detached pair drawn at the Codori house (Codori read (4039,4607); bachelder j2-03). Col. Ward mw here."),
      kf(T(18, 50), 4055, 4620, 260, "line", 200, "documented",
        "Wright's/Lang's front overruns the advanced pair (ED-60 receiving frame)."),
      kf(T(19, 10), 4360, 4600, 260, "line", 165, "documented", "Falls back to the ridge line."),
      kf(EndT, 4370, 4600, 260, "line", 160, "inferred", "Re-formed at dark."),
    ],
  },
  {
    id: "us-82ny", parent: "us-harrow", name: "82nd New York (Harrow's)", side: "union",
    kfs: [
      kf(0, 4050, 4560, 260, "line", 335, "documented",
        "The Codori pair's left (drawn state, bachelder j2-03); Lt. Col. Huston mw."),
      kf(T(18, 50), 4050, 4560, 260, "line", 290, "documented", "Overrun with the 15th MA."),
      kf(T(19, 10), 4350, 4540, 260, "line", 275, "documented", "Falls back to the ridge line."),
      kf(EndT, 4360, 4540, 260, "line", 270, "inferred", "Re-formed at dark."),
    ],
  },
  {
    id: "us-1mn", parent: "us-harrow", name: "1st Minnesota (Harrow's)", side: "union",
    kfs: [
      kf(0, 4260, 4300, 260, "line", 330, "documented",
        "Near Thomas's battery on the ridge (Coates; the j2-04 pair '1' Minn' (4230,4282) vs 'WILCOX ALA.' (4112,4368)). Strength 330 (return frame; monument '262 charged' carried — ED-39/52 both exercised, dossier upgrade pass 6)."),
      kf(T(19, 0), 4230, 4282, 250, "line", 330, "documented",
        "Hancock's order as Wilcox's line reaches the ravine: THE CHARGE — 'at double-quick down the slope of the hill right upon the rebel line' (or-27-1-coates) meeting Wilcox's 'a third line descended at a double-quick' (cross-sided at regiment grain)."),
      kf(T(19, 10), 4150, 4330, 250, "line", 140, "documented",
        "The stand at the ravine: the strength/loss layers carried UN-AVERAGED — monument 262-charged/215-fell/'232 of 330' vs the return's 224; Colvill w."),
      kf(EndT, 4230, 4290, 255, "line", 106, "documented",
        "The survivors withdraw to the battery line at dark (Coates; the second bleeding at the copse is July 3's record)."),
    ],
  },
  {
    id: "us-19me", parent: "us-harrow", name: "19th Maine (Harrow's)", side: "union",
    kfs: [
      kf(0, 4340, 4480, 260, "line", 440, "documented",
        "The brigade's ridge line south of the copse (build/roster frame; fired on the Wilcox/Lang front at the repulse)."),
      kf(T(19, 20), 4340, 4480, 260, "line", 420, "inferred", "Engaged at the repulse of the road front."),
      kf(EndT, 4340, 4480, 260, "line", 415, "inferred", "Holds at dark."),
    ],
  },
  // ------------------- de TROBRIAND + WARD + STONE ------------------------
  {
    id: "us-detrobriand",
    kfs: [
      kf(0, 3640, 3040, 200, "line", 1387, "documented",
        "The Wheatfield garrison: 17th ME at the stone wall / the ravine line / Winslow in the field — all drawn on j2-03. Strength ~1,387 (B&M-type hop). Dossier us-iii-1-3-detrobriand.md."),
      kf(T(17, 45), 3640, 3040, 200, "line", 1150, "documented",
        "Wave 1 held: G. T. Anderson's first advance repulsed at the stone fence (the both-sided W1 record, ED-57)."),
      kf(T(18, 10), 3900, 3300, 230, "line", 1000, "documented",
        "Relieved AS the ammunition ran out and Caldwell arrived — the seam is triggered, not clocked, in every primary (ED-57's W2/W3 seam)."),
      kf(EndT, 4150, 3550, 240, "line", 897, "documented",
        "Re-formed to the rear at dark. Decay: report=return exact 490 (pass-6 agreement set)."),
    ],
  },
  {
    id: "us-berdan",
    kfs: [
      kf(0, 3850, 2730, 190, "line", 1500, "documented",
        "Ward's brigade on Houck's Ridge above Devil's Den ('Ward's Brig' + regiment bars (3841-3856, 2665-2795), bachelder j2-03; mon-124ny (3776,2624) — Van Horne Ellis k). Strength: 'approximately 1,500 effective men engaged' (or-27-1-ward primary). Dossier us-iii-1-2-ward.md."),
      kf(T(17, 0), 3850, 2730, 190, "line", 1450, "documented",
        "The Houck's Ridge fight opens (CA-J2A-5 approach): Robertson/Benning/Law's left against the ridge — the two-sided 90-minute duration agreement (Ward = Sheffield)."),
      kf(T(18, 20), 3900, 2800, 200, "line", 850, "documented",
        "Forced back after ~90 minutes (Ward's tablet 'between 4 and 5 P.M.' carried as the early-skew reading, ED-58; the adopted seam follows the duration agreement from the ~16:45-17:00 contact)."),
      kf(EndT, 4250, 3000, 220, "line", 719, "documented",
        "Re-formed toward the north at dark. Decay: return 781 ('~800' report class carried; the 4th ME row closed clean at 144, pass 8)."),
    ],
  },
  {
    id: "us-dana",
    kfs: [
      kf(0, 4430, 4520, 260, "line", 465, "documented",
        "Stone's brigade (Dana) in the I Corps mass behind Cemetery Ridge; 109-aggregate July 2 strength class per the 150th PA history (the pass-5 T4 upgrade — historyofonehund00cham; brigade figure compilation-tier)."),
      kf(T(18, 0), 4430, 4520, 260, "column", 465, "documented",
        "The ~18:00 double-quick to Humphreys's rear (150th PA history primary — the pass-4 static-July-2 reading CORRECTED)."),
      kf(T(18, 45), 4400, 4400, 260, "line", 455, "documented",
        "Behind the rallying road front; the two-gun recovery (Doubleday cross-quoted)."),
      kf(EndT, 4300, 4560, 262, "line", 450, "documented",
        "Night picket toward the Codori-left ground (150th PA history)."),
    ],
  },
  // ------------------- THE GUN LINE: III CORPS + McGILVERY ----------------
  {
    id: "us-btty-clark", name: "Clark's Battery B, 1st New Jersey", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3408, 3605, 190, "line", 110, "documented",
        "The Peach Orchard south face ('Clark' drawn at (3408,3605), bachelder j2-03; the apex two-fronted). Dossier us-btty-clark.md."),
      kf(T(18, 25), 3408, 3605, 190, "line", 95, "documented",
        "Fought the counter-battery duel and the assault's approach; withdrew as the salient fell."),
      kf(EndT, 4150, 3900, 250, "column", 90, "inferred", "To the rear line at dark."),
    ],
  },
  {
    id: "us-btty-bucklyn", name: "Bucklyn's Battery E, 1st Rhode Island", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3450, 3690, 260, "line", 103, "documented",
        "The Peach Orchard west face on the Emmitsburg road (dossier us-btty-bucklyn.md; the apex's two-front artillery record)."),
      kf(T(18, 25), 3450, 3690, 260, "line", 80, "documented",
        "Wrecked in the apex's collapse (Bucklyn w; the battery's loss the heaviest of the salient's guns)."),
      kf(EndT, 4120, 3950, 255, "column", 73, "inferred", "Withdrawn at dark."),
    ],
  },
  {
    id: "us-btty-james",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3700, 4300, 260, "line", 134, "documented",
        "Seeley's K/4th US on Humphreys's road front ('5' N.J. at Seeley's' — the Burling ledger's drawn host ground; near the Rogers house (3724,4358), bachelder j2-03)."),
      kf(T(19, 0), 3700, 4300, 260, "line", 118, "documented",
        "Fought the road front's collapse; Seeley w (James succeeds — the July 3 file's command name)."),
      kf(EndT, 4320, 4450, 260, "column", 112, "inferred", "Withdrawn to the ridge at dark."),
    ],
  },
  {
    id: "us-btty-winslow", name: "Winslow's Battery D, 1st New York", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3774, 3145, 200, "line", 116, "documented",
        "The Wheatfield interior ('a small wheat-field', or-27-1-winslow; mon-winslow (3774,3145); 'Winslow' drawn NE Wheatfield (3734,3141), bachelder j2-03). Dossier us-btty-winslow.md (pass 8)."),
      kf(T(18, 5), 3774, 3145, 200, "line", 105, "documented",
        "Fired solid shot and case over the infantry's heads, then canister as the wheat filled; withdrew by piece as the flanks came in (or-27-1-winslow)."),
      kf(EndT, 4200, 3700, 250, "column", 98, "documented",
        "To the rear at dark. Decay: the 20-vs-18 both-component conflict CARRIED (dossier)."),
    ],
  },
  {
    id: "us-btty-smith4ny", name: "Smith's 4th New York Independent Battery", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3787, 2551, 190, "line", 126, "documented",
        "Four guns on the Devil's Den crest (mon-4ny-dd (3787,2551)); the rear section in the Plum Run gully (mon-4ny-rear (4016,2802)) — 'Smith' drawn as a SEPARATE rear element (4016,2946) (bachelder j2-03; the two-element drawn state confirming the report's structure). Dossier us-iii-b3-smith4ny class (pass 6). Personnel compilation-tier."),
      kf(T(17, 30), 3787, 2551, 190, "line", 113, "documented",
        "The forward guns lost ~17:30 (CA-J2A-5): exactly THREE 10-pdr Parrotts abandoned (the fourth saved/disabled), NO captor credited, believed later retrieved — Smith's own report, the object-fixing primary of ED-56."),
      kf(T(17, 40), 4016, 2802, 210, "line", 113, "documented",
        "The battery continues from the gully section ('fired obliquely', or-27-1-smith-4ny) — the rear-section fought-on state rendered per his own report."),
      kf(EndT, 4180, 3050, 230, "column", 108, "inferred", "Withdrawn at dark."),
    ],
  },
  {
    id: "us-btty-bigelow", name: "Bigelow's 9th Massachusetts Battery", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3540, 3446, 200, "line", 104, "documented",
        "The Wheatfield Road line (mon-9ma-first (3540,3446); the McGilvery row drawn Clark-Phillips-Bigelow ~(3449,3573), bachelder j2-03). The battery's only OR report is Milton's No. 320 (verified negative on a Bigelow report). Dossier us-btty-bigelow.md (pass 8, the ED-61 gate)."),
      kf(T(18, 25), 3540, 3446, 200, "line", 100, "documented",
        "Retired by prolonge, firing, as the salient fell ('retired 300 yards' — the marker frame reproduces it: first-position to Trostle 323 m)."),
      kf(T(18, 35), 3806, 3630, 220, "line", 96, "documented",
        "THE TROSTLE-ANGLE STAND (mon-9ma-trostle (3806,3630); 'two stone walls met at an obtuse angle', Milton): ordered by McGilvery to hold at all hazards while the Plum Run line formed behind him."),
      kf(T(19, 5), 3806, 3630, 220, "line", 80, "documented",
        "The 21st MS overruns the angle: FOUR of six light 12-pounders lost ('silenced the four pieces on my right, and prevented their withdrawal', Milton; the j2-04 two-element Barksdale draw puts '21' MISS' here over 'BIGELOW'); Bigelow w; Milton's section of two retires. ED-61: ONE loss, a recovery ledger, never per-claimant duplicate guns."),
      kf(EndT, 4100, 3800, 240, "column", 76, "documented",
        "The remnant behind the Plum Run line at sunset (the same-night haul-off ~20:00-21:00 is the evening phase's record). Decay: Milton's itemized per-day loss sums to the return TO THE MAN (27 + 1 = 28)."),
    ],
  },
  {
    id: "us-btty-phillips",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3518, 3543, 200, "line", 104, "documented",
        "Phillips's 5th MA on the Wheatfield Road line ('Phillips' (3518,3543), bachelder j2-03)."),
      kf(T(18, 30), 3518, 3543, 200, "line", 96, "documented",
        "Withdrew by prolonge as the salient fell (McGilvery's line record, or-27-1-mcgilvery)."),
      kf(EndT, 4212, 4080, 260, "line", 92, "documented",
        "On McGilvery's improvised Plum Run line at dark (the July 3 line's ground — or-27-1-mcgilvery pp. 881-884)."),
    ],
  },
  {
    id: "us-btty-hart",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3480, 3620, 200, "line", 99, "documented",
        "Hart's 15th NY on the Wheatfield Road/orchard line (McGilvery's roster frame)."),
      kf(T(18, 30), 3480, 3620, 200, "line", 92, "documented", "Withdrawn as the salient fell."),
      kf(EndT, 4205, 4010, 260, "line", 90, "documented",
        "On the Plum Run line at dark (or-27-1-mcgilvery)."),
    ],
  },
  {
    id: "us-btty-ames",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3470, 3650, 200, "line", 132, "documented",
        "Ames's G/1st NY at the Peach Orchard (McGilvery's brigade line, or-27-1-mcgilvery)."),
      kf(T(18, 30), 3470, 3650, 200, "line", 122, "documented", "Withdrawn as the salient fell."),
      kf(EndT, 4172, 3730, 260, "line", 118, "documented",
        "On the Plum Run line at dark (or-27-1-mcgilvery; the July 3 ground)."),
    ],
  },
  {
    id: "us-btty-thompson",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 3440, 3720, 250, "line", 105, "documented",
        "Thompson's C&F PA at the orchard's west face (McGilvery's roster frame)."),
      kf(T(18, 30), 3440, 3720, 250, "line", 92, "documented",
        "Overrun in the collapse, guns extracted with loss (his July 3 roster place on the line rides or-27-1-mcgilvery p. 883)."),
      kf(EndT, 4220, 4150, 260, "line", 88, "documented",
        "On the Plum Run line at dark (or-27-1-mcgilvery)."),
    ],
  },
  {
    id: "us-btty-dow",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 5150, 4380, 240, "column", 103, "inferred",
        "Dow's 6th ME in the Artillery Reserve park (Reserve node frame)."),
      kf(T(19, 0), 4180, 3800, 250, "line", 100, "documented",
        "Brought onto McGilvery's improvised Plum Run line as it formed (~19:00; or-27-1-dow/-mcgilvery) — the line's left."),
      kf(EndT, 4180, 3800, 250, "line", 99, "documented",
        "Holds at sunset (Dow's procured infantry detail hauls the 9th MA's guns the same night — the evening phase's ED-61 record)."),
    ],
  },
  {
    id: "us-btty-watson", name: "Watson's Battery I, 5th US", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 5150, 4320, 240, "column", 71, "inferred",
        "Battery I/5th US in the Artillery Reserve (T3 by construction — no battery report exists; MacConnell quoted inside Martin's, dossier us-btty-watson.md)."),
      kf(T(18, 30), 4208, 3744, 250, "line", 71, "documented",
        "Onto the Plum Run knoll as the salient fell (mon-watson (4208,3744); 'WATSON' drawn (4093,3607), bachelder j2-04)."),
      kf(T(19, 0), 4208, 3744, 250, "line", 60, "documented",
        "'The guns of Battery I, Fifth Regulars, were abandoned' (or-27-1-mcgilvery) — the four guns taken by the 21st MS's front ('captured but were unable to bring off'); Watson w. ED-61: one loss; the Peeples/39th NY recapture and the same-night haul-off are the evening phase's record."),
      kf(EndT, 4208, 3744, 250, "scattered", 55, "documented",
        "The crews clear of the abandoned pieces at sunset (the taking leg untraced — T3 ceiling accepted, dossier honesty note)."),
    ],
  },
  {
    id: "us-btty-weir",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 5100, 4450, 240, "column", 123, "inferred",
        "Weir's C/5th US with the Reserve (crisis-reinforcement class, ED-41)."),
      kf(T(18, 40), 4620, 4680, 260, "line", 123, "documented",
        "Run forward to the Codori-front gap as the road line collapsed; three guns temporarily lost to Wright's front and retaken (the ED-60 receiving-side record)."),
      kf(EndT, 4620, 4680, 260, "line", 115, "documented",
        "Holds at dark (the temporary-loss state carries no materiel discontinuity — the ED-63 rendering rule's class)."),
    ],
  },
  // ------------------- XII CORPS: THE DEPARTURE (CA-J2E-1) ----------------
  {
    id: "us-greene",
    kfs: [
      kf(0, 5812, 5510, 60, "line", 1350, "documented",
        "Greene's brigade on the upper hill's crest: works of 'logs, cord-wood, stones, and earth' complete 'By 12 o'clock' (or-27-1-greene); the monument chain 60th NY (5793,5577) - statue (5773,5584) - the drawn works bars (5812,5514). Strength: 'My brigade 1,350' — the report's own engaged-force table. Dossier us-xii-2-3-greene.md."),
      kf(T(18, 30), 5812, 5510, 60, "line", 1350, "documented",
        "CA-J2E-1 (PRIMARY basis, ED-62): 'until 6.30 p. m., when the First (Williams') Division and the First and Second Brigades of the Second Division were ordered from my right' — the brigade extends into a single line and holds the hill ALONE."),
      kf(T(19, 0), 5812, 5510, 60, "line", 1345, "documented",
        "'We were attacked on the whole of our front by a large force a few minutes before 7 p. m.' (or-27-1-greene p. 856 — CA-J2E-2's Union primary)."),
      kf(EndT, 5812, 5510, 60, "line", 1310, "documented",
        "Holding at sunset; the 137th NY's refused flank and the reinforcement ledger (I Corps 355 + XI Corps ~400) are the evening phase's record."),
    ],
  },
  {
    id: "us-kane",
    kfs: [
      kf(0, 5865, 5480, 80, "line", 600, "documented",
        "Kane's brigade in the works right of Greene (the XII Corps line built July 2 morning; register/tablet ground)."),
      kf(T(18, 35), 5865, 5480, 80, "column", 600, "documented",
        "Ordered away with Geary's column (CA-J2E-1; Geary's stated 7 p.m. rides as the propagation reading, ED-62)."),
      kf(EndT, 5500, 4800, 170, "column", 600, "documented",
        "On the Baltimore Pike at sunset — Geary's two brigades took the wrong road (his own record; the two-volley return discovery is the evening phase's)."),
    ],
  },
  {
    id: "us-candy",
    kfs: [
      kf(0, 5880, 5380, 90, "line", 1400, "documented",
        "Candy's brigade in the works (the departure state drawn: 'GEARY'S DIV. 12 Corps'/'Candy's Brig' moving labels (5532,5384)/(5655,5316), bachelder j2-04)."),
      kf(T(18, 35), 5880, 5380, 90, "column", 1400, "documented", "Ordered away with Geary (CA-J2E-1)."),
      kf(EndT, 5450, 4650, 170, "column", 1400, "documented",
        "Down the pike at sunset (the wrong-road narrative, or-27-1-geary pp. 828+ flagged unfetched — carried at command grain)."),
    ],
  },
  {
    id: "us-mcdougall",
    kfs: [
      kf(0, 5950, 5230, 100, "line", 1755, "documented",
        "McDougall's brigade in the lower works ('McDougall Brig' vacated position drawn (5876,4988), bachelder j2-04)."),
      kf(T(18, 32), 5950, 5230, 100, "column", 1755, "documented",
        "Ordered away 18:30 (CA-J2E-1 primary) — the vacated lower works become Steuart's overnight lodgment (the ED-57-class occupation record)."),
      kf(EndT, 5500, 4750, 190, "column", 1755, "documented",
        "Marching toward the left at sunset (Ruger's column on the pike, drawn j2-04)."),
    ],
  },
  {
    id: "us-lockwood",
    kfs: [
      kf(0, 5990, 5060, 90, "line", 1650, "documented",
        "Lockwood's brigade (Ruger's division frame; the works line)."),
      kf(T(18, 32), 5990, 5060, 90, "column", 1650, "documented", "Ordered away 18:30 (CA-J2E-1)."),
      kf(EndT, 5300, 4700, 210, "column", 1650, "documented",
        "Marching toward the left at sunset — A. S. Williams conducts it toward McGilvery's front (the 'quite dark' three-piece recapture is the evening phase's ED-61 record)."),
    ],
  },
  {
    id: "us-colgrove",
    kfs: [
      kf(0, 6080, 4800, 30, "line", 1170, "documented",
        "Colgrove's brigade at Spangler's Spring/McAllister's Woods (register/tablet ground)."),
      kf(T(18, 32), 6080, 4800, 30, "column", 1170, "documented", "Ordered away with Ruger (CA-J2E-1)."),
      kf(EndT, 5700, 4400, 210, "column", 1170, "documented",
        "On the pike at sunset (the '9. P.M.' drawn return annotation beside 'RUGER'S' is the evening phase's clock)."),
    ],
  },
  {
    id: "us-btty-atwell", frontage: 100, depth: 30,
    kfs: [
      kf(0, 5700, 5300, 60, "line", 139, "documented",
        "Knap's E PA on the Culp's Hill/pike ground: Geary's 4 p.m. counter-duel against Latimer (the cross-army clock agreement, pass 8)."),
      kf(T(17, 40), 5700, 5300, 60, "line", 133, "documented",
        "The Benner's Hill duel won (~90 min; Latimer wrecked and withdrawn)."),
      kf(EndT, 5960, 4100, 40, "line", 133, "documented",
        "Withdrawn to Powers Hill at the corps' departure (the July 3 ground)."),
    ],
  },
  {
    id: "us-btty-kinzie", frontage: 100, depth: 30,
    kfs: [
      kf(0, 5650, 5250, 60, "line", 77, "documented",
        "Kinzie's K/5th US beside Knap's in the 16:00 duel (Geary's counter-duel pair, pass 8)."),
      kf(T(17, 40), 5650, 5250, 60, "line", 74, "documented", "The duel won."),
      kf(EndT, 5560, 4880, 60, "line", 74, "documented", "To the pike knoll ground at the departure."),
    ],
  },
  // ------------------- VI CORPS ARRIVALS (the reserve mass) ---------------
  {
    id: "us-bartlett",
    kfs: [
      kf(0, 5950, 3050, 300, "column", 1325, "documented",
        "Bartlett's brigade at the Baltimore Pike after the corps' 34-mile march (Sedgwick's arrival frame, or-27-1-sedgwick class; the Nevin march primary is the corps' condition record)."),
      kf(T(18, 30), 4600, 3200, 270, "column", 1325, "inferred", "Moved behind the left as the fighting rose."),
      kf(EndT, 4150, 3000, 260, "line", 1325, "inferred", "In reserve behind the Plum Run line at dark."),
    ],
  },
  {
    id: "us-torbert",
    kfs: [
      kf(0, 6000, 3000, 300, "column", 1320, "documented", "VI Corps arrival column (the 34-mile march frame)."),
      kf(T(18, 45), 4700, 3400, 270, "column", 1320, "inferred", "Behind the left."),
      kf(EndT, 4260, 3540, 260, "line", 1320, "inferred", "Reserve at dark (the July 3 ground)."),
    ],
  },
  {
    id: "us-russell",
    kfs: [
      kf(0, 6050, 2950, 300, "column", 1480, "documented", "VI Corps arrival column."),
      kf(T(19, 0), 4800, 2600, 260, "column", 1480, "inferred", "Toward the Round Tops' rear."),
      kf(EndT, 4400, 2200, 240, "line", 1480, "inferred", "Behind the Round Top line at dark."),
    ],
  },
  {
    id: "us-grant",
    kfs: [
      kf(0, 6100, 2900, 300, "column", 1830, "documented", "VI Corps arrival column (Grant's Vermonters)."),
      kf(T(19, 0), 4900, 2500, 260, "column", 1830, "inferred", "Toward the army's left rear."),
      kf(EndT, 4450, 2280, 240, "line", 1830, "inferred", "Left-rear reserve at dark."),
    ],
  },
  {
    id: "us-neill",
    kfs: [
      kf(0, 6150, 2850, 320, "column", 1775, "documented", "VI Corps arrival column."),
      kf(T(19, 0), 6200, 3800, 20, "column", 1775, "inferred", "Toward the army's right (the corps' one right-flank brigade)."),
      kf(EndT, 6350, 4400, 30, "column", 1775, "inferred", "Near Rock Creek at dark (the extreme-right ground is the July 3 record)."),
    ],
  },
  {
    id: "us-eustis",
    kfs: [
      kf(0, 6000, 2900, 300, "column", 1595, "documented", "VI Corps arrival column."),
      kf(T(18, 45), 5000, 3600, 280, "column", 1595, "inferred", "Behind the center-left."),
      kf(EndT, 4850, 4200, 270, "column", 1595, "inferred", "Reserve at dark."),
    ],
  },
  {
    id: "us-shaler",
    kfs: [
      kf(0, 6050, 2850, 320, "column", 1770, "documented", "VI Corps arrival column."),
      kf(T(19, 0), 5750, 3800, 10, "column", 1770, "inferred", "Toward the right-center reserve."),
      kf(EndT, 5650, 4500, 20, "column", 1770, "inferred", "Near the pike at dark (Culp's Hill reserve is the July 3 morning record)."),
    ],
  },
  {
    id: "us-btty-mccartney", frontage: 100, depth: 30,
    kfs: [
      kf(0, 6000, 2950, 300, "column", 145, "documented", "1st MA Battery A with the VI Corps column (the corps' march frame)."),
      kf(EndT, 4210, 3070, 270, "column", 145, "inferred",
        "Parked at the Weickert-farm reserve ground at dark (the July 3 file's cited t=0 ground)."),
    ],
  },
  {
    id: "us-btty-cowan", frontage: 100, depth: 30,
    kfs: [
      kf(0, 5950, 3000, 300, "column", 113, "documented", "Cowan's 1st NY Independent with the VI Corps column."),
      kf(EndT, 4700, 4300, 270, "column", 113, "inferred",
        "Parked behind the II Corps front at dark (his July 3 detachment to that front is the July 3 record)."),
    ],
  },
  // ------------------- ARRIVING LATE: STANNARD ----------------------------
  {
    id: "us-stannard",
    kfs: [
      kf(0, 4950, 3300, 300, "column", 1788, "documented",
        "Stannard's Vermont brigade closing on the field (arrived the evening of July 2 after the march from the Defenses of Washington; 12th/15th VT detached as train guard — engaged basis 1,788 regiment-grain, ED-32)."),
      kf(EndT, 4650, 4000, 300, "column", 1788, "documented",
        "Nearing the I Corps mass at sunset (joins the line overnight; the July 3 flank attack is the shipped record)."),
    ],
  },
];

// ---------------------------------------------------------------------------
// STATIC OVERRIDES — units held static this phase whose July 2 ground or
// record differs from (or documents) the July-3 inherited pose.
// pos omitted = July 3 pose inherited.
// ---------------------------------------------------------------------------
interface SO {
  x?: number; z?: number; facing?: number; formation?: Keyframe["formation"];
  strength?: number; grade: "A" | "B" | "C"; citation?: string;
}
const staticOverrides: Record<string, SO> = {
  // --- CSA statics with July 2 citations ---
  "csa-mahone": {
    grade: "B", citation:
      "Anderson's line: the brigade DID NOT ADVANCE with the division's echelon July 2 (the division record's carried fact; tablet trench-line ground) — documented stillness, rendered still (ED-44 class).",
  },
  "csa-hays": {
    x: 5318, z: 6523, facing: 200, strength: 1137, grade: "A", citation:
      "Moved at ~02:00 July 2 after a personally-conducted midnight reconnaissance to 'an open field between the city and the base of a hill intervening between us and Cemetery Hill' — all day under skirmisher and artillery fire (or-27-2-hays; tab-csa-2c-ear-1 (5318,6523)). Strength: tablet ~1,200 less the July 1 loss (his own July 1 figure). The 19:45 assault (CA-J2E-3) is the evening phase's record. Dossier csa-2c-ear-1-hays.md.",
  },
  "csa-godwin": {
    x: 5334, z: 6497, facing: 200, strength: 850, grade: "A", citation:
      "Hoke's brigade (Avery) on Hays's left all day July 2 — the jump-off field, tab-csa-2c-ear-3 (5334,6497), 30 m from the Hays tablet: 'the two assault brigades' tablets stand as the pair they attacked as'. NO brigade report exists (Avery mw that evening; Godwin silent — verified negative). Dossier csa-2c-ear-3-avery.md.",
  },
  "csa-gordon": {
    x: 5230, z: 6470, facing: 200, strength: 1200, grade: "B", citation:
      "Gordon's brigade in reserve at Winebrenner's Run (register/tablet ground); Early's support echelon for the evening assault.",
  },
  "csa-smith": {
    grade: "B", citation:
      "Smith's brigade on the York Pike flank guard July 1-2 (register/tablet; detached watching the left rear — documented absence from the assaults).",
  },
  "csa-doles": { grade: "B", citation: "Long Lane July 2-3 (register/tablet): Rodes's line SW of town, skirmishing." },
  "csa-iverson": { grade: "B", citation: "Long Lane July 2-3 (register/tablet); the July 1 wreck's survivors in the line." },
  "csa-ramseur": { grade: "B", citation: "Long Lane July 2-3 (register/tablet)." },
  "csa-daniel": {
    x: 4200, z: 5980, facing: 140, grade: "C", citation:
      "With Rodes SW of town July 2 (the Long Lane line's right; his July 3 attachment to Johnson and the east-of-Rock-Creek afternoon are the next day's record) — T1-grade placement, no July 2 dossier anchor consumed.",
  },
  "csa-oneal": {
    x: 4300, z: 6100, facing: 140, grade: "C", citation:
      "With Rodes SW of town July 2 (Long Lane flank; the July 3 morning attachment to Johnson is the next day's record) — T1-grade placement.",
  },
  "csa-perrin": {
    grade: "B", citation:
      "Long Lane July 2-3: 'constantly engaged in skirmishing' (tablet, the wave-5 citation class) — the in-window emitter.",
  },
  "csa-thomas": {
    grade: "B", citation:
      "Long Lane July 2-3: 'engaged most of the day in severe skirmishing and exposed to heavy artillery fire' (tablet).",
  },
  "csa-lane": {
    x: 3450, z: 5550, facing: 90, grade: "C", citation:
      "Pender's second line on Seminary Ridge July 2 (division frame; his July 3 assault staging is the next day's ground) — T1-grade placement.",
  },
  "csa-lowrance": {
    x: 3480, z: 5400, facing: 90, grade: "C", citation:
      "Pender's second line on Seminary Ridge July 2 (Scales's wrecked brigade at its ~500 re-based strength, or-27-2-lowrance primary — the day-expansion-1 re-basing) — T1-grade placement.",
  },
  "csa-fry": {
    x: 2750, z: 5350, facing: 90, formation: "column", grade: "C", citation:
      "Heth's division in reserve west of Seminary Ridge July 2, recovering from July 1 (division frame) — T1-grade placement.",
  },
  "csa-marshall": {
    x: 2720, z: 5500, facing: 90, formation: "column", grade: "C", citation:
      "Heth's division in reserve July 2 (division frame) — T1-grade placement.",
  },
  "csa-davis": {
    x: 2720, z: 5680, facing: 90, formation: "column", grade: "C", citation:
      "Heth's division in reserve July 2 (division frame) — T1-grade placement.",
  },
  "csa-brockenbrough": {
    x: 2750, z: 5850, facing: 90, formation: "column", grade: "C", citation:
      "Heth's division in reserve July 2 (division frame; the ED-48 honestly-bounded unit) — T1-grade placement.",
  },
  "csa-bn-alexander": {
    x: 2694, z: 3567, facing: 80, grade: "A", citation:
      "Alexander's battalion line on the Warfield ridge (drawn battery line (2694,3567) with gun-count annotations, bachelder j2-03): 36 guns in action ~15:45 opening the cannonade (alexander-1907 arithmetic — CA-J2A-2's early pole; Jacobs's 16:20 reading the late pole, BOTH carried, ED-53); deployed IN FRONT of Barksdale's infantry (the extraction-delay record). Advanced to the captured Peach Orchard ground toward dark ('ALEXANDER'S BATT.' (3497,3751), bachelder j2-04) — rendered on the evening phase.",
  },
  "csa-bn-cabell": {
    x: 2850, z: 3300, facing: 80, grade: "A", citation:
      "Cabell's battalion on Kershaw's front (alexander-1907: 'Cabell's 18 guns in his front'): fired the cannonade from ~15:45 and the THREE-GUN SIGNAL in rapid succession as Kershaw's step-off order (~17:30, CA-J2A-7 — the July 2 counterpart of the July 3 signal pair; pass-5 record).",
  },
  "csa-bn-henry": {
    x: 2920, z: 2260, facing: 120, grade: "A", citation:
      "Henry's battalion on the division right (tablet ground class; the July 3 file's Warfield-ridge/Bushman-knoll continuity): engaged Smith's 4th NY and the Round Top front through the assault (Benning's under-fire enumeration of the Union guns BY SHELF matches Smith's/Hazlett's dossiers gun-for-gun — pass 6).",
  },
  "csa-bn-eshleman": {
    x: 2500, z: 3520, facing: 80, formation: "column", grade: "C", citation:
      "The Washington Artillery in First Corps reserve July 2 (its Peach Orchard line is taken overnight — the July 3 tablet ground) — T1-grade placement.",
  },
  "csa-bn-lane": {
    grade: "B", citation:
      "Lane's (Sumter) battalion on Anderson's front, Seminary Ridge: 1,082 rounds fired July 2-3 (tablet grade A, the register's citation) — the division assault's supporting fire.",
  },
  "csa-bn-pegram": { grade: "C", citation: "Seminary Ridge line (July 3 ground continuity) — T1-grade placement." },
  "csa-bn-mcintosh": { grade: "C", citation: "Schultz Woods line (July 3 ground continuity) — T1-grade placement." },
  "csa-bn-garnett": { grade: "C", citation: "Near McMillan Woods in reserve (tablet 'in reserve, not actively engaged' — July 3 scope; T1-grade July 2 placement)." },
  "csa-bn-poague": { grade: "C", citation: "Behind Anderson/Pettigrew, Spangler/McMillan Woods (July 3 ground continuity) — T1-grade placement." },
  "csa-bn-dance": { grade: "C", citation: "Seminary Ridge near the Seminary (July 3 ground continuity; Dance ENGAGED class per ED-45) — T1-grade placement." },
  "csa-bn-nelson": { grade: "C", citation: "Benner's Hill area reserve (July 3 ground continuity) — T1-grade placement." },
  "csa-bn-carter": { grade: "C", citation: "Oak Hill (July 3 ground continuity) — T1-grade placement." },
  "csa-bn-jones": { grade: "B", citation: "North of town — 'Not actively engaged' (tablet, documented negative; ED-44 rendering class)." },
  // --- Union statics with July 2 citations ---
  "us-meredith": { grade: "B", citation: "Culp's Hill trenches beside the XII Corps July 2-3 (tablet; the July 1 wreck's survivors)." },
  "us-cutler": { grade: "B", citation: "Culp's Hill July 2-3 (tablet)." },
  "us-coulter": { grade: "C", citation: "Robinson's division massed on Cemetery Hill's rear July 2 (division frame) — T1-grade placement." },
  "us-baxter": { grade: "C", citation: "Robinson's division massed on Cemetery Hill's rear July 2 — T1-grade placement." },
  "us-biddle": { grade: "C", citation: "Doubleday's division massed behind Cemetery Ridge July 2 — T1-grade placement." },
  "us-vongilsa": {
    grade: "A", citation:
      "Von Gilsa's brigade behind the stone walls at East Cemetery Hill's north foot (the 41st NY/von-Gilsa monument cluster, pass-8 register batch): all day under the town's sharpshooter fire; the 19:45 assault strikes here (evening phase). NO brigade report (verified negative; the 41st NY regimental carried, or-27-1-einsiedel-41ny).",
  },
  "us-harris": {
    grade: "A", citation:
      "Ames's brigade (Harris) at East Cemetery Hill: the 75th OH's 91-officers-and-men July 2 basis (Harris — the July-1-wreckage arithmetic in one line); all day under fire; the evening assault is the next phase's record. Dossier us-xi-1-2-harris.md.",
  },
  "us-vonamsberg": { grade: "C", citation: "XI Corps line on Cemetery Hill July 2 (corps frame) — T1-grade placement." },
  "us-krzyzanowski": { grade: "C", citation: "XI Corps line on Cemetery Hill July 2 — T1-grade placement." },
  "us-coster": { grade: "C", citation: "Cemetery Hill north edge July 2 (corps frame) — T1-grade placement." },
  "us-smith": { grade: "B", citation: "Cemetery Hill front along the Taneytown/Emmitsburg roads July 2-3 (register/tablet)." },
  "us-carroll": {
    x: 4560, z: 5080, facing: 260, grade: "B", citation:
      "Carroll's brigade on the II Corps right near Ziegler's Grove July 2 (division frame); the ~20:30 night charge to East Cemetery Hill (CA-J2E-4) is the evening phase's record (ED-51).",
  },
  "us-8oh": {
    grade: "B", citation:
      "The 8th Ohio detached on the picket line west of the Emmitsburg road (the 40-hour picket tour begins July 2 evening — Sawyer; the picket-line-dominant casualty split is ED-50's record).",
  },
  "us-webb": { grade: "B", citation: "The II Corps line at the copse from July 2 (brigade ground; the Angle fight is July 3's record). The 106th PA's night detachment to East Cemetery Hill rides the evening record at command grain." },
  "us-hall": { grade: "B", citation: "The II Corps line left of Webb from July 2 (brigade ground; Hall's receiving statement on Barksdale's front is the pass-5 attribution-conflict record, carried)." },
  "us-smyth": {
    grade: "A", citation:
      "Smyth's brigade on Hays's front: the Bliss-farm skirmish axis July 2-3 (the 1st DE skirmish line; register/tablet; dossier us-ii-3-2-smyth.md) — the July 2 share of the two-accounting loss (352 report vs 366 tablet, both carried).",
  },
  "us-btty-woodruff": { grade: "B", citation: "Ziegler's Grove (register/tablet ground, July 2-3)." },
  "us-btty-arnold": { grade: "B", citation: "The II Corps line north of the copse July 2-3 (brigade ground)." },
  "us-btty-cushing": { grade: "B", citation: "The Angle front July 2-3 (brigade ground; the July 3 wreck is the shipped record)." },
  "us-btty-brown": {
    grade: "A", citation:
      "Brown's B/1st RI west of the ridge crest: overrun by Wright's high-water ~19:15 July 2 — the TEMPORARY gun loss that fixes ED-60's receiving side; the battery resumed service (no materiel discontinuity, the ED-63 rendering class).",
  },
  "us-btty-rorty": {
    x: 5100, z: 4430, facing: 240, formation: "column", grade: "C", citation:
      "B/1st NY with the reserve July 2 (Rorty takes command July 3) — T1-grade placement.",
  },
  "us-btty-thomas": {
    x: 5120, z: 4310, facing: 240, formation: "column", grade: "C", citation:
      "C/4th US in the Artillery Reserve July 2 (its McGilvery-line ground is July 3's) — T1-grade placement.",
  },
  "us-btty-sterling": {
    x: 5180, z: 4390, facing: 240, formation: "column", grade: "C", citation:
      "2nd CT in the Artillery Reserve July 2 — T1-grade placement.",
  },
  "us-btty-fitzhugh": {
    x: 5210, z: 4340, facing: 240, formation: "column", grade: "C", citation:
      "K/1st NY in the Artillery Reserve July 2 (crisis-reinforcement July 3, ED-41) — T1-grade placement.",
  },
  "us-btty-parsons": {
    x: 5240, z: 4380, facing: 240, formation: "column", grade: "C", citation:
      "A/1st NJ in the Artillery Reserve July 2 (the ED-37 roster question is July 3's record) — T1-grade placement.",
  },
  "us-btty-wiedrich": {
    grade: "A", citation:
      "Wiedrich's I/1st NY on East Cemetery Hill's north front (mon-wiedrich (5017,5850) — 21 m from the drawn lodgment bar): engaged the Benner's Hill duel ~16:00-17:30; the ~20:00 intrenchments fight is the evening phase's record (ED-63). Dossier us-btty-wiedrich.md.",
  },
  "us-btty-ricketts": {
    x: 5140, z: 5745, facing: 55, formation: "column", grade: "B", citation:
      "Ricketts's F&G PA brought onto East Cemetery Hill toward evening July 2 (relieving Cooper's front; the assault finds it in position — evening phase). Dossier us-btty-ricketts.md.",
  },
  "us-btty-cooper": {
    grade: "A", citation:
      "Cooper's B/1st PA on East Cemetery Hill: engaged the Benner's Hill duel ~16:00-17:30 (the hill's counter-battery group; his July 3 Hancock-Ave move is the next day's record).",
  },
  "us-btty-stevens": {
    grade: "A", citation:
      "Stevens's 5th ME on the knoll between Cemetery and Culp's Hills (mon-5me-stevens (5435,5542) class ground): engaged the Benner's Hill duel; the enfilade of the 19:45 assault is the evening phase's record.",
  },
  "us-btty-stewart": { grade: "B", citation: "Astride the Baltimore Pike, East Cemetery Hill July 2-3 (register/tablet); engaged the Benner's Hill duel." },
  "us-btty-breck": { grade: "C", citation: "Wainwright's group, East Cemetery Hill July 2 — T1-grade placement." },
  "us-btty-taft": { grade: "C", citation: "Cemetery Hill July 2 evening class (the 20-pdr group's July 3 record is the shipped file's) — T1-grade placement." },
  "us-btty-mason": { grade: "C", citation: "Reserve/Cemetery Hill ground July 2 — T1-grade placement." },
  "us-btty-edgell": { grade: "C", citation: "Reserve/Cemetery Hill ground July 2 — T1-grade placement." },
  "us-btty-norton": { grade: "C", citation: "Reserve/Cemetery Hill ground July 2 — T1-grade placement." },
  "us-btty-hill-wv": { grade: "C", citation: "Reserve/Cemetery Hill ground July 2 — T1-grade placement." },
  "us-btty-wheeler": { grade: "C", citation: "XI Corps line July 2 (his July 3 reserve-to-line move is the shipped record) — T1-grade placement." },
  "us-btty-dilger": { grade: "C", citation: "Cemetery Hill July 2 — T1-grade placement." },
  "us-btty-bancroft": { grade: "C", citation: "Cemetery Hill July 2 — T1-grade placement." },
  "us-btty-hall-2me": { grade: "B", citation: "Rear of Cemetery Hill July 2 (the July 1 fight's battery in reserve; register note)." },
  "us-btty-rigby": { grade: "B", citation: "Powers Hill July 2-3 (register/tablet)." },
  "us-btty-winegar": { grade: "B", citation: "McAllister's Hill (Baltimore Pike/Rock Creek) July 2-3 (register/tablet)." },
  "us-btty-rugg": { grade: "C", citation: "Near the pike behind the XII Corps July 2 — T1-grade placement." },
  "us-arty-reserve-park": {
    grade: "B", citation:
      "The Artillery Reserve park (the Reserve node): the batteries detached to the fighting line July 2 (McGilvery's 1st Volunteer brigade, Dow, Watson, Weir) render as their own units — the park holds the remainder and the 70-wagon train context (ED-42's supply-side ledger).",
  },
};

// ---------------------------------------------------------------------------
// Assemble units: movers by table; everything else from the July 3 file as
// window-endpoint statics (T1 tier unless an override cites better).
// ---------------------------------------------------------------------------
const moverIds = new Set(movers.map((m) => m.id));
const units: Unit[] = [];

for (const m of movers) {
  const base = j3ById.get(m.id);
  const name = m.name ?? J2NAME[m.id] ?? base?.name;
  const side = m.side ?? base?.side;
  if (!name || !side) throw new Error(`dayexp2: mover '${m.id}' needs name/side`);
  units.push(moverUnit({
    id: m.id, name, side,
    frontage_m: m.frontage ?? base?.frontage_m,
    depth_m: m.depth ?? base?.depth_m,
    ...(m.parent !== undefined && { parent: m.parent }),
    keyframes: m.kfs,
  }));
  // Decomposed parents may not carry rosters (battle-format.md); Harrow's
  // roster is dropped for this file only. Undecomposed movers keep theirs.
  const u = units[units.length - 1]!;
  if (m.id !== "us-harrow" && !m.parent && base?.regiments) u.regiments = base.regiments;
}

const T1_NOTE =
  "T1-grade July 2 authoring (day-expansion slice 2): held at the unit's attested/July-3 ground for the phase window — window endpoints only, no July 2 dossier anchor consumed.";

for (const u of july3.units) {
  if (moverIds.has(u.id) || EXCLUDE.has(u.id)) continue;
  const o = staticOverrides[u.id];
  const p = j3pose(u.id);
  const row = staticUnit({
    id: u.id,
    name: J2NAME[u.id] ?? u.name,
    side: u.side,
    strength: o?.strength ?? p.strength,
    x: o?.x ?? p.x,
    z: o?.z ?? p.z,
    facing: o?.facing ?? p.facing,
    formation: o?.formation ?? p.formation,
    frontage_m: u.frontage_m,
    depth_m: u.depth_m,
    grade: o?.grade === "A" ? "A" : o?.grade === "B" ? "B" : "C",
    citation: o?.citation ?? T1_NOTE,
    endT: EndT,
  });
  if (u.regiments) row.regiments = u.regiments;
  units.push(row);
}

// ---------------------------------------------------------------------------
// Events — fire windows per the activity records (attach level = the level
// the source attests; documented silence stays event-free, battle-format.md).
// ---------------------------------------------------------------------------
const events: EngagementEvent[] = [
  // The opening cannonade (CA-J2A-2, two-pole — ED-53).
  fireEvent({
    id: "j2a-cannonade-alexander", kind: "artillery_fire", unitId: "csa-bn-alexander",
    t0: T(15, 45), t1: T(16, 35), confidence: "documented",
    citation: "36 guns in action ~15:45 (alexander-1907 arithmetic; Scruggs/Oates 15:30-15:45 arrivals at the early pole).",
    note: "CA-J2A-2 is a recorded two-pole structure: Jacobs's 'twenty minutes past 4 P.M.' clock reading is the late pole — both carried, neither erased (ED-53).",
  }),
  fireEvent({
    id: "j2a-cannonade-cabell", kind: "artillery_fire", unitId: "csa-bn-cabell",
    t0: T(15, 45), t1: T(17, 32), confidence: "documented",
    citation: "Cabell's guns fought the Peach Orchard duel from the opening; the window closes on the THREE-GUN step-off signal for Kershaw (~17:30, CA-J2A-7 — pass-5 record).",
  }),
  fireEvent({
    id: "j2a-henry-hood-front", kind: "artillery_fire", unitId: "csa-bn-henry",
    t0: T(16, 10), t1: T(18, 0), confidence: "documented",
    citation: "Henry's battalion against the Devil's Den/Round Top front (Smith's and the receiving dossiers; Benning's gun-by-shelf enumeration cross-bears).",
  }),
  // The Union salient guns answer.
  fireEvent({
    id: "j2a-po-clark", kind: "artillery_fire", unitId: "us-btty-clark",
    t0: T(15, 50), t1: T(18, 25), confidence: "documented",
    citation: "The south-face duel from the orchard (drawn two-fronted apex, bachelder j2-03).",
  }),
  fireEvent({
    id: "j2a-po-bucklyn", kind: "artillery_fire", unitId: "us-btty-bucklyn",
    t0: T(15, 50), t1: T(18, 25), confidence: "documented",
    citation: "The west-face duel on the Emmitsburg road (dossier us-btty-bucklyn.md).",
  }),
  fireEvent({
    id: "j2a-po-ames", kind: "artillery_fire", unitId: "us-btty-ames",
    t0: T(16, 0), t1: T(18, 30), confidence: "documented",
    citation: "McGilvery's line in the orchard duel (or-27-1-mcgilvery).",
  }),
  fireEvent({
    id: "j2a-po-phillips", kind: "artillery_fire", unitId: "us-btty-phillips",
    t0: T(16, 0), t1: T(18, 35), confidence: "documented",
    citation: "The Wheatfield Road line's duel and repulse fire (or-27-1-mcgilvery; drawn row, bachelder j2-03).",
  }),
  fireEvent({
    id: "j2a-po-hart", kind: "artillery_fire", unitId: "us-btty-hart",
    t0: T(16, 0), t1: T(18, 30), confidence: "documented",
    citation: "The Wheatfield Road line (McGilvery's roster).",
  }),
  fireEvent({
    id: "j2a-po-thompson", kind: "artillery_fire", unitId: "us-btty-thompson",
    t0: T(16, 0), t1: T(18, 30), confidence: "documented",
    citation: "The orchard's west face (McGilvery's roster).",
  }),
  fireEvent({
    id: "j2a-bigelow-line", kind: "artillery_fire", unitId: "us-btty-bigelow",
    t0: T(16, 0), t1: T(18, 25), confidence: "documented",
    citation: "The Wheatfield Road line fire (Milton No. 320; mon-9ma-first ground).",
  }),
  fireEvent({
    id: "j2a-bigelow-trostle", kind: "artillery_fire", unitId: "us-btty-bigelow",
    t0: T(18, 36), t1: T(19, 5), confidence: "documented",
    citation: "The Trostle-angle canister stand to the muzzles (Milton No. 320: the four right pieces silenced and lost; ED-61's object-fixing primary).",
  }),
  fireEvent({
    id: "j2a-seeley-front", kind: "artillery_fire", unitId: "us-btty-james",
    t0: T(17, 30), t1: T(19, 0), confidence: "documented",
    citation: "Seeley's K/4th US on the Emmitsburg road front (the Burling-ledger host ground; Seeley w).",
  }),
  fireEvent({
    id: "j2a-winslow-wheatfield", kind: "artillery_fire", unitId: "us-btty-winslow",
    t0: T(17, 0), t1: T(18, 5), confidence: "documented",
    citation: "Solid shot and case over the infantry, canister as the wheat filled; withdrew by piece (or-27-1-winslow).",
  }),
  fireEvent({
    id: "j2a-smith4ny-den", kind: "artillery_fire", unitId: "us-btty-smith4ny",
    t0: T(16, 30), t1: T(17, 28), confidence: "documented",
    citation: "The four forward Parrotts from the Den crest against Hood's advance (or-27-1-smith-4ny).",
  }),
  fireEvent({
    id: "j2a-smith4ny-gully", kind: "artillery_fire", unitId: "us-btty-smith4ny",
    t0: T(17, 40), t1: T(18, 30), confidence: "documented",
    citation: "The rear section from the Plum Run gully, firing obliquely (or-27-1-smith-4ny; the two-element drawn state).",
  }),
  fireEvent({
    id: "j2a-hazlett-summit", kind: "artillery_fire", unitId: "us-btty-rittenhouse",
    t0: T(17, 32), t1: T(19, 15), confidence: "documented",
    citation: "Hazlett's guns from the summit over the fight (Garrard; the Weed/Hazlett mw pair rides the position record).",
  }),
  fireEvent({
    id: "j2a-watson-knoll", kind: "artillery_fire", unitId: "us-btty-watson",
    t0: T(18, 35), t1: T(19, 0), confidence: "documented",
    citation: "The knoll fire until the four guns were abandoned (or-27-1-mcgilvery; or-27-1-martin-v p. 660).",
  }),
  fireEvent({
    id: "j2a-dow-plumrun", kind: "artillery_fire", unitId: "us-btty-dow",
    t0: T(19, 0), t1: EndT, confidence: "documented",
    citation: "The 6th ME on the improvised Plum Run line (~19:00 to dark; or-27-1-dow class record via McGilvery).",
  }),
  fireEvent({
    id: "j2a-weir-codori", kind: "artillery_fire", unitId: "us-btty-weir",
    t0: T(18, 45), t1: T(19, 15), confidence: "documented",
    citation: "Canister at the Codori-front gap (the ED-60 receiving side; three guns temporarily lost and retaken).",
  }),
  fireEvent({
    id: "j2a-brown-ridge", kind: "artillery_fire", unitId: "us-btty-brown",
    t0: T(18, 45), t1: T(19, 10), confidence: "documented",
    citation: "B/1st RI on Wright's front until overrun (temporary loss; ED-60).",
  }),
  // The Benner's Hill duel (~16:00-17:30).
  fireEvent({
    id: "j2a-benner-latimer", kind: "artillery_fire", unitId: "csa-bn-raine",
    t0: T(16, 0), t1: T(17, 30), confidence: "documented",
    citation: "The Benner's Hill duel: Johnson's 4 p.m. Latimer order = Geary's 4 p.m. counter-duel (cross-army agreement, pass 8); Latimer mw; battalion ledger 50 by battery (OR 27/2 pp. 341-343).",
  }),
  fireEvent({
    id: "j2a-benner-atwell", kind: "artillery_fire", unitId: "us-btty-atwell",
    t0: T(16, 0), t1: T(17, 35), confidence: "documented",
    citation: "Knap's E PA in the counter-duel (Geary).",
  }),
  fireEvent({
    id: "j2a-benner-kinzie", kind: "artillery_fire", unitId: "us-btty-kinzie",
    t0: T(16, 0), t1: T(17, 35), confidence: "documented",
    citation: "K/5th US in the counter-duel (Geary).",
  }),
  fireEvent({
    id: "j2a-benner-cooper", kind: "artillery_fire", unitId: "us-btty-cooper",
    t0: T(16, 5), t1: T(17, 30), confidence: "documented",
    citation: "The East Cemetery Hill group against Benner's Hill (the hill's counter-battery record).",
  }),
  fireEvent({
    id: "j2a-benner-stevens", kind: "artillery_fire", unitId: "us-btty-stevens",
    t0: T(16, 5), t1: T(17, 30), confidence: "documented",
    citation: "The 5th ME from the knoll in the duel (the battery's ground record).",
  }),
  fireEvent({
    id: "j2a-benner-wiedrich", kind: "artillery_fire", unitId: "us-btty-wiedrich",
    t0: T(16, 5), t1: T(17, 30), confidence: "documented",
    citation: "Wiedrich's I/1st NY in the duel (dossier us-btty-wiedrich.md).",
  }),
  // Houck's Ridge / Devil's Den musketry.
  fireEvent({
    id: "j2a-houcks-ward", kind: "musketry", unitId: "us-berdan",
    t0: T(17, 0), t1: T(18, 20), confidence: "documented",
    citation: "Ward's 'space of one and a half hours' = Sheffield's 'an hour and a half' — the two-sided duration agreement.",
  }),
  fireEvent({
    id: "j2a-houcks-robertson", kind: "musketry", unitId: "csa-robertson",
    t0: T(17, 0), t1: T(18, 30), confidence: "documented",
    citation: "The Texans against Houck's Ridge and the Den (or-27-2-robertson; the 90-minute agreement).",
  }),
  fireEvent({
    id: "j2a-houcks-benning", kind: "musketry", unitId: "csa-benning",
    t0: T(17, 10), t1: T(18, 40), confidence: "documented",
    citation: "Benning's Georgians in the Den fight and its holding (or-27-2-benning).",
  }),
  // Little Round Top.
  fireEvent({
    id: "j2a-lrt-law", kind: "musketry", unitId: "csa-sheffield",
    t0: T(17, 0), t1: T(19, 0), confidence: "documented",
    citation: "The five charges on the spur (or-27-2-oates-15al; or-27-2-scruggs-4al).",
  }),
  fireEvent({
    id: "j2a-lrt-vincent", kind: "musketry", unitId: "us-rice",
    t0: T(17, 0), t1: T(19, 0), confidence: "documented",
    citation: "Vincent's line receives and repulses the charges; the 20th ME bayonet counter (or-27-1-rice-vincent; or-27-1-chamberlain).",
  }),
  fireEvent({
    id: "j2a-lrt-weed", kind: "musketry", unitId: "us-garrard",
    t0: T(17, 40), t1: T(19, 0), confidence: "documented",
    citation: "The 140th NY's counter-rush and the summit line's fire (Garrard; O'Rorke mw).",
  }),
  // The Wheatfield waves (ED-57 — windows at the adopted envelope times).
  fireEvent({
    id: "j2a-w1-detrobriand", kind: "musketry", unitId: "us-detrobriand",
    t0: T(17, 0), t1: T(18, 5), confidence: "documented",
    citation: "W1: the garrison's fight to ammunition exhaustion (or-27-1-detrobriand; the relief seam is triggered, not clocked).",
  }),
  fireEvent({
    id: "j2a-w1w4-anderson", kind: "musketry", unitId: "csa-luffman",
    t0: T(17, 0), t1: T(19, 10), confidence: "documented",
    citation: "G. T. Anderson's three advances (White's structure) — W1 through the W4 renewal; the window spans the attested advances, the retirements riding the track.",
  }),
  fireEvent({
    id: "j2a-w2-kershaw", kind: "musketry", unitId: "csa-kershaw",
    t0: T(17, 45), t1: T(19, 20), confidence: "documented",
    citation: "W2 onward: the stony hill fight, the W3 counterattack received, the W4 renewal (or-27-2-kershaw; ED-57).",
  }),
  fireEvent({
    id: "j2a-w2-semmes", kind: "musketry", unitId: "csa-bryan",
    t0: T(17, 50), t1: T(19, 5), confidence: "documented",
    citation: "Semmes's ravine fight and the W4 general advance (tablet verbatim; Semmes mw).",
  }),
  fireEvent({
    id: "j2a-w2-tilton", kind: "musketry", unitId: "us-tilton",
    t0: T(17, 25), t1: T(17, 58), confidence: "documented",
    citation: "Two repulses inflicted before the ordered withdrawal (or-27-1-tilton; ED-59 — never a rout).",
  }),
  fireEvent({
    id: "j2a-w2-sweitzer-first", kind: "musketry", unitId: "us-sweitzer",
    t0: T(17, 25), t1: T(17, 58), confidence: "documented",
    citation: "The stony hill's first defense (or-27-1-sweitzer).",
  }),
  fireEvent({
    id: "j2a-w4-sweitzer-second", kind: "musketry", unitId: "us-sweitzer",
    t0: T(18, 30), t1: T(19, 10), confidence: "documented",
    citation: "The second advance at Caldwell's request; the fence crisis (Jeffords k; the 4th MI/62nd PA pocket).",
  }),
  fireEvent({
    id: "j2a-w3-cross", kind: "musketry", unitId: "us-mckeen",
    t0: T(18, 5), t1: T(18, 50), confidence: "documented",
    citation: "W3 (CA-J2A-8): Cross into the wheat, mw at the opening (or-27-1-mckeen).",
  }),
  fireEvent({
    id: "j2a-w3-kelly", kind: "musketry", unitId: "us-kelly",
    t0: T(18, 10), t1: T(18, 55), confidence: "documented",
    citation: "The 45-minute rocks fight (or-27-1-kelly — the duration primary).",
  }),
  fireEvent({
    id: "j2a-w3-zook", kind: "musketry", unitId: "us-fraser",
    t0: T(18, 10), t1: T(18, 55), confidence: "documented",
    citation: "Zook's front (mw at the start; or-27-1-fraser).",
  }),
  fireEvent({
    id: "j2a-w3-brooke", kind: "musketry", unitId: "us-brooke",
    t0: T(18, 15), t1: T(19, 0), confidence: "documented",
    citation: "The deepest penetration and the flanked extraction (or-27-1-brooke).",
  }),
  fireEvent({
    id: "j2a-w4-wofford", kind: "musketry", unitId: "csa-wofford",
    t0: T(18, 35), t1: EndT, confidence: "documented",
    citation: "The Wheatfield-Road sweep to the foot of Little Round Top, ending on the sunset order (tablet; CA-J2A-11).",
  }),
  fireEvent({
    id: "j2a-w4-day", kind: "musketry", unitId: "us-day",
    t0: T(18, 25), t1: T(19, 5), confidence: "documented",
    citation: "The regulars' stand and extraction under both-flank fire (Ayres/Day frame; Burbank's mechanism).",
  }),
  fireEvent({
    id: "j2a-w4-burbank", kind: "musketry", unitId: "us-burbank",
    t0: T(18, 25), t1: T(19, 5), confidence: "documented",
    citation: "'The enemy was seen ... moving through a wheat-field to our rear' — the recrossing under fire (or-27-1-burbank).",
  }),
  fireEvent({
    id: "j2a-w5-mccandless", kind: "musketry", unitId: "us-mccandless",
    t0: T(19, 12), t1: EndT, confidence: "documented",
    citation: "W5: the charge to the stone wall (or-27-1-crawford/-mccandless; Taylor k).",
  }),
  fireEvent({
    id: "j2a-w5-nevin", kind: "musketry", unitId: "us-nevin",
    t0: T(19, 15), t1: EndT, confidence: "documented",
    citation: "W5: the countercharge the tablet aims at Wofford by name (or-27-1-nevin; tablet).",
  }),
  // The Peach Orchard and the road front.
  fireEvent({
    id: "j2a-po-graham", kind: "musketry", unitId: "us-madill",
    t0: T(18, 15), t1: T(18, 50), confidence: "documented",
    citation: "The apex's infantry fight (Graham w&c; Tippin's seam moment).",
  }),
  fireEvent({
    id: "j2a-po-barksdale", kind: "musketry", unitId: "csa-humphreys",
    t0: T(18, 15), t1: T(19, 10), confidence: "documented",
    citation: "Barksdale's assault from the step-off to the Plum Run climax (CA-J2A-9; the mw record).",
  }),
  fireEvent({
    id: "j2a-road-carr", kind: "musketry", unitId: "us-carr",
    t0: T(18, 30), t1: T(19, 15), confidence: "documented",
    citation: "The Emmitsburg road front's fighting withdrawal (or-27-1-carr; drawn both-ends state j2-04).",
  }),
  fireEvent({
    id: "j2a-road-brewster", kind: "musketry", unitId: "us-brewster",
    t0: T(18, 25), t1: T(19, 10), confidence: "documented",
    citation: "The Excelsiors on the salient's north shoulder (or-27-1-brewster; the 8th FL colors record).",
  }),
  fireEvent({
    id: "j2a-road-wilcox", kind: "musketry", unitId: "csa-wilcox",
    t0: T(18, 25), t1: T(19, 25), confidence: "documented",
    citation: "Wilcox's advance to the ravine and the 30-minute stand (or-27-2-wilcox).",
  }),
  fireEvent({
    id: "j2a-road-lang", kind: "musketry", unitId: "csa-lang",
    t0: T(18, 30), t1: T(19, 20), confidence: "documented",
    citation: "The Floridians' advance and withdrawal (or-27-2-lang July 2 keys).",
  }),
  fireEvent({
    id: "j2a-road-wright", kind: "musketry", unitId: "csa-wright",
    t0: T(18, 45), t1: T(19, 25), confidence: "documented",
    citation: "Wright's advance to the gun line and the flanked withdrawal (or-27-2-wright; ED-60).",
  }),
  fireEvent({
    id: "j2a-1mn-charge", kind: "musketry", unitId: "us-1mn",
    t0: T(19, 0), t1: T(19, 12), confidence: "documented",
    citation: "The 1st Minnesota's charge (or-27-1-coates; both-sided with Wilcox at regiment grain).",
  }),
  fireEvent({
    id: "j2a-willard-charge", kind: "musketry", unitId: "us-sherrill",
    t0: T(19, 5), t1: EndT, confidence: "documented",
    citation: "Willard's counterattack at Plum Run (the drawn collision, j2-04; Willard k).",
  }),
  fireEvent({
    id: "j2a-harrow-repulse", kind: "musketry", unitId: "us-19me",
    t0: T(19, 5), t1: T(19, 25), confidence: "documented",
    citation: "The ridge line's repulse fire on the road front (brigade frame; the Codori pair's fight rides their own tracks).",
  }),
  // The all-day skirmish scopes (the wave-5 citation class).
  fireEvent({
    id: "j2a-bliss-posey", kind: "musketry", unitId: "csa-posey",
    t0: 0, t1: EndT, confidence: "documented",
    citation: "Bliss farm skirmishing July 2-3 (register/tablet; the brigade's documented partial advance).",
  }),
  fireEvent({
    id: "j2a-bliss-smyth", kind: "musketry", unitId: "us-smyth",
    t0: 0, t1: EndT, confidence: "documented",
    citation: "The 1st DE skirmish line on the Bliss axis (or-27-1-smyth; register/tablet).",
  }),
  fireEvent({
    id: "j2a-longlane-perrin", kind: "musketry", unitId: "csa-perrin",
    t0: 0, t1: EndT, confidence: "documented",
    citation: "'Constantly engaged in skirmishing' (tablet verbatim — the day-scope class).",
  }),
  fireEvent({
    id: "j2a-longlane-thomas", kind: "musketry", unitId: "csa-thomas",
    t0: 0, t1: EndT, confidence: "documented",
    citation: "'Engaged most of the day in severe skirmishing and exposed to heavy artillery fire' (tablet verbatim).",
  }),
  fireEvent({
    id: "j2a-hays-sharpshooters", kind: "musketry", unitId: "csa-hays",
    t0: 0, t1: EndT, confidence: "documented",
    citation: "All day under and returning skirmisher/sharpshooter fire between the town and the hill (or-27-2-hays).",
  }),
  // The Culp's Hill strike at the window's edge (CA-J2E-2).
  fireEvent({
    id: "j2a-culp-jones", kind: "musketry", unitId: "csa-dungan",
    t0: T(19, 0), t1: EndT, confidence: "documented",
    citation: "The first assault climbs against Greene's right (or-27-2-jones-jm; CA-J2E-2).",
  }),
  fireEvent({
    id: "j2a-culp-nicholls", kind: "musketry", unitId: "csa-williams",
    t0: T(19, 2), t1: EndT, confidence: "documented",
    citation: "'7 p. m. of July 2 ... ordered forward' (or-27-2-williams-nicholls).",
  }),
  fireEvent({
    id: "j2a-culp-greene", kind: "musketry", unitId: "us-greene",
    t0: T(19, 0), t1: EndT, confidence: "documented",
    citation: "'Attacked on the whole of our front ... a few minutes before 7 p. m.' (or-27-1-greene p. 856).",
  }),
];

// ---------------------------------------------------------------------------
// The battle document.
// ---------------------------------------------------------------------------
const battle: Battle = {
  name: "Gettysburg — July 2, 1863: Afternoon (the assault on the Union left)",
  startTime: StartTime,
  endTime: EndT,
  units,
  events,
  environment: {
    windTowardDeg: 45, windMps: 0.0, confidence: "unknown",
    note: "No sourced wind observation for the July 2 afternoon exists in the corpus (the ED-10/ED-19 class); calm authored — windMps 0 = no drift.",
  },
};

writeFileSync(outPath, exportValidated(battle));
console.log(`wrote ${outPath}: ${units.length} units, ${events.length} events, ` +
  `window ${StartTime}..${StartTime + EndT} (15:30-19:29 LMT)`);
