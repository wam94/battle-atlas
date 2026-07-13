// Day expansion slice 3 — THE JULY 1 AFTERNOON PHASE (Rodes from Oak Hill,
// Iverson's field, Early at Barlow's Knoll, the double-corps collapse, the
// retreat through town, the Cemetery Hill consolidation; 13:00–18:00 LMT).
// Run from tool/ AFTER the morning script (this file starts every carried
// unit at that file's cited end state — the cross-phase continuity rule):
//   npx vite-node scripts/author-dayexp3-afternoon.ts
// Slice report: docs/reconstruction/audit/day-expansion-slice-3.md.
//
// THE PHASE (ADR 0005): startTime 46800 (13:00 LMT — abutting the morning
// phase at the midday-lull seam; reconstructed phases within a day may not
// overlap, battle-manifest.md) · endTime 18000 (18:00 LMT). The 18:00 end
// is the chain-evidence envelope: the last hard clock is CA-J1P-8 (the
// 17:25 Hancock dispatch, a timestamped document pin), Robinson's rear
// guard "until nearly 5 p.m." and Schurz's "after 5 o'clock" bound the
// consolidation's build-out, and CA-J1P-9 (Ewell declines the hills) is
// SEQUENCE-ONLY (after CA-J1P-7, before dark) — it rides the manifest's
// honest july1-evening note, never a keyframe. The XII/III Corps arrival
// columns reach the field's edge at/after this window's close and are an
// honest cut (slice report §4).
//
// CONTENT: ED-27 as amended by ED-67/68/69/70 — CA-J1P-1 (Rodes 14:00,
// with the recorded interior ladder), CA-J1P-2 (Iverson's destruction,
// the single-spike EC6 exemplar, four-source + drawn geometry), CA-J1P-3
// (Gordon=Schurz cross-line pair), CA-J1P-4/5 (the ~16:00 double collapse,
// Perrin cross-line basis), CA-J1P-6 (the town retreat's dossiered legs),
// CA-J1P-7 (Hancock ~16:15 — THE conflict record, carried whole), CA-J1P-8
// (17:25). NO invention; conflicts carried; trigger/sequence anchors
// author `inferred`; no report-nominal clock moves a keyframe.
import { writeFileSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe, Unit } from "../src/model";
import { fireEvent, moverUnit, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const morningPath = join(here, "../../app/Assets/Battle/gettysburg-july1-morning.json");
const outPath = join(here, "../../app/Assets/Battle/gettysburg-july1-afternoon.json");
const morning: Battle = JSON.parse(readFileSync(morningPath, "utf8"));

const StartTime = 46800; // 13:00 LMT
const EndT = 18000; // 18:00 LMT (see header)
const T = (h: number, m: number) => h * 3600 + m * 60 - StartTime;

// Cross-phase continuity: every unit carried over starts at the morning
// file's cited end state (position, facing, formation, strength). Verified
// structurally here — a mismatch is a build error, not a warning.
const mById = new Map(morning.units.map((u) => [u.id, u]));
function morningEnd(id: string) {
  const u = mById.get(id);
  if (!u) throw new Error(`dayexp3-afternoon: morning unit '${id}' missing`);
  const k = u.keyframes[u.keyframes.length - 1]!;
  return k;
}

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  confidence: Keyframe["confidence"], citation?: string): Keyframe => ({
    t, x, z, facing, formation, strength,
    ...(confidence !== undefined && { confidence }),
    ...(citation !== undefined && { citation }),
  });

// Continuity keyframe: t=0 at the morning end state, with a citation.
function cont(id: string, citation: string): Keyframe {
  const k = morningEnd(id);
  return kf(0, k.x, k.z, k.facing, k.formation, k.strength, "documented", citation);
}

interface M {
  id: string; name: string; side: "union" | "confederate";
  frontage?: number; depth?: number; kfs: Keyframe[];
}

const movers: M[] = [
  // ======================= RODES FROM OAK HILL (CA-J1P-1/2) ================
  {
    id: "csa-oneal", name: "O'Neal's Brigade, Rodes's Division", side: "confederate",
    kfs: [
      cont("csa-oneal",
        "Continuity from the morning phase's 13:00 state: formed on Oak Hill's east slope (or-27-2-rodes; the June 30 return's 1,794 PFD)."),
      kf(T(14, 0), 4150, 8350, 165, "line", 1794, "documented",
        "CA-J1P-1 — RODES ATTACKS (14:00 adopted, envelope 13:30-14:45): 'made a vigorous attack at 2 P.M. with superior numbers along the entire line' (1st Corps tablet, ED-26/27 verification). ED-69 interior ladder RECORDED, no move: Coulter 12:30 general firing → Doles ~13:00 formation → Stone 13:30 = Dobke 13:30 (independent cross-corps, report-nominal −20 profile) → tablet 14:00 → Hill 'About 2.30' corps retrospective."),
      kf(T(14, 30), 4050, 8500, 345, "scattered", 1600, "documented",
        "Repulsed 'almost instantaneously' (or-27-2-iverson's support-miscarriage ledger); the left regiment flanked (dossier EC3.1); the 3rd Ala detached — the j1-09 sheet draws '3 ALA' DETACHED at the Hoffman farm (the Bachelder sheet drawing Rodes's report's O'Neal anatomy, pass-10 spatial note). The 45th NY's flank-fire harvest is the same event from the XI side (or-27-1-dobke-45ny)."),
      kf(T(15, 30), 4150, 8300, 165, "line", 1500, "documented",
        "Renewed pressure beside Ramseur's committed wing (2nd + 4th NC sent to O'Neal's front — or-27-2-ramseur's split order)."),
      kf(T(16, 30), 4400, 7900, 170, "line", 1350, "documented",
        "Forward through the seam as the I Corps right gives way (CA-J1P-5's receiving cluster; Robinson's rear-guard tail 'until nearly 5 p.m.' carried on the Union rows)."),
      kf(EndT, 4500, 7700, 170, "line", 1300, "documented",
        "On the captured Oak Ridge / town NW ground at 18:00. Decay honesty: the return's 696 covers July 1 + July 3 with NO per-day primary (dossier EC6, flagged) — the authored ~494 July 1 share is bounded reconstruction; the July-3-file t=0 (1,100) implies ~694 and the difference is a recorded cross-file residual (slice report §7)."),
    ],
  },
  {
    id: "csa-iverson", name: "Iverson's Brigade, Rodes's Division", side: "confederate",
    kfs: [
      cont("csa-iverson",
        "Continuity: the Oak Hill launch ground (mon-iverson-hq 3883.7,8742.4, formation semantics; June 30 return 1,464 PFD)."),
      kf(T(14, 15), 3728, 8365, 155, "line", 1464, "documented",
        "THE ADVANCE, DRAWN MID-STRIDE: j1-09 (2 p.m. sheet) red 'IVERSON, N.C.' bar with regimental numbers read at (3728,8365) — 408 m along the advance axis from the formation tablet toward the Forney-field wall; 'J. FORNEY' farm drawn at (3614,8637); the receiving line drawn: 'BAXTER'S BRIGADE' blue at the Mummasburg-road wall (4007,8424). Launch on Rodes's order; the gap on the left never filled (O'Neal's instant repulse) (or-27-2-iverson; ED-67 basis)."),
      kf(T(14, 40), 3960, 8420, 155, "line", 1250, "documented",
        "THE DEATH LINE (CA-J1P-2 adopted 14:30-15:00, envelope 14:15-15:15): 'advanced to within 100 yards' of 'the enemy, strongly posted in woods and behind a concealed stone wall'; 'a most desperate fight took place' (or-27-2-iverson) — Baxter's line 'opened on the advancing foes a most deadly fire' (or-27-1-baxter receiving)."),
      kf(T(15, 0), 3960, 8420, 155, "scattered", 650, "documented",
        "THE SINGLE-SPIKE EXEMPLAR: '500 of my men were left lying dead and wounded on a line as straight as a dress parade' (or-27-2-iverson) = 'His dead lay in a distinctly marked line of battle' (or-27-2-rodes) + Ramseur's 'almost annihilated' + Cutler's flank-volley surrender mass + Baxter's charge (88th Pa two flags, 97th NY one) + Ewell's 'the greater part of three regiments had fallen where they stood' — four echelons, both-sided. EC6: return p. 342 130k/328w/308m printed 820 FAILS its own arithmetic (766) — ED-71: arithmetic reconstruction adopted for computation, printed cells carried verbatim; the 308 missing IS the line's capture mass; the surrender-and-exoneration record carried whole."),
      kf(T(15, 20), 4050, 8380, 155, "line", 650, "documented",
        "The remnant rallied and led forward by Capt. Halsey (AAG) — Iverson and Rodes both credit him; the 12th NC held intact under Lt. Col. Davis (or-27-2-iverson)."),
      kf(T(16, 30), 4300, 7600, 170, "line", 648, "documented",
        "The railroad pursuit: 'the Twelfth and Fifty-third North Carolina were the first to reach the railroad along which the enemy were retreating'; prisoners 'numberless... cut off' (or-27-2-iverson; the treble-counting risk vs Ramseur's 800-900 recorded, none adopted as a count)."),
      kf(EndT, 4750, 7000, 170, "line", 645, "documented",
        "Evening: the remnant attached to Ramseur, 'on the street facing the heights' (or-27-2-iverson) — the town's east-west street line."),
    ],
  },
  {
    id: "csa-daniel", name: "Daniel's Brigade, Rodes's Division", side: "confederate",
    kfs: [
      cont("csa-daniel",
        "Continuity: the second line behind Iverson's right (June 30 return 2,294 PFD — the division's largest)."),
      kf(T(14, 0), 3650, 8300, 175, "line", 2294, "documented",
        "Committed with the division's attack toward the pike and railroad cuts on Iverson's right (or-27-2-daniel; or-27-2-rodes's deployment anatomy)."),
      kf(T(14, 45), 3550, 7900, 180, "line", 2100, "documented",
        "THE RAILROAD-CUT FIGHT (his July 1 spine): the brigade against Stone's double front at the cuts — both-sided with the 149th Pa's cut episode (or-27-1-stone-roy: the fence volley at pistol-shot, the 30-yard volley + charge); 'losing nearly one-third of their number' is his own July 1 fraction (or-27-2-daniel)."),
      kf(T(15, 30), 3560, 7780, 180, "line", 1900, "documented",
        "The second effort against the barn/pike front (Rodes sequences Ramseur's stroke 'just as his [Daniel's] last effort was made' — or-27-2-rodes)."),
      kf(T(16, 15), 3800, 7600, 160, "line", 1750, "documented",
        "The final advance as the I Corps line folds back to the seminary (CA-J1P-5 window)."),
      kf(EndT, 3900, 7500, 160, "line", 1650, "documented",
        "On the cut/seminary-north ground at 18:00. Decay: ~644 ≈ his stated third (the 916 return total spans July 1-3; no numeric per-day primary — the fraction governs, flagged)."),
    ],
  },
  {
    id: "csa-ramseur", name: "Ramseur's Brigade, Rodes's Division", side: "confederate",
    kfs: [
      cont("csa-ramseur",
        "Continuity: division reserve behind Oak Hill (June 30 return 1,090 PFD; kind-none clock profile — the commitment rides the division sequence)."),
      kf(T(15, 0), 3980, 8480, 150, "line", 1090, "documented",
        "THE COMMITMENT ('After resting about fifteen minutes'): the split order — 2nd + 4th NC to O'Neal's front, 14th + 30th with Ramseur against 'the enemy's strong position in a body of woods, surrounded by a stone fence' — the same wall-and-woods Iverson died in front of; Battle's 3rd Ala self-attached ('came in at the right place, at the right time, and in the right way') (or-27-2-ramseur)."),
      kf(T(15, 45), 4150, 8250, 155, "line", 1040, "documented",
        "THE EN-MASSE FLANK ATTACK: turned 'by attacking en masse on his right flank... getting in his rear', the 12th NC pushing the front simultaneously; 'the enemy... ran off the field in confusion, leaving his killed and wounded and between 800 and 900 prisoners in our hands' (or-27-2-ramseur — the capture ledger faces Robinson's own 'about 1,000 prisoners' claim across the same wall; both carried)."),
      kf(T(16, 45), 4700, 7200, 170, "line", 950, "documented",
        "The pursuit 'through Gettysburg to the heights beyond' (or-27-2-ramseur); Lt. Harney (14th NC sharpshooters) mw taking the 150th Pa colors in the streets — the pass-4 Stone colors record closed both-sided at man grain."),
      kf(EndT, 4850, 6900, 170, "line", 930, "documented",
        "'I received an order to halt, and form line of battle in a street in Gettysburg running east and west' (or-27-2-ramseur) — the pursuit's stop line (CA-J1P-9 companion at brigade grain). Decay: report=return EXACT 177 at all four regiments (the audit's fourth full-regiment-grain agreement) — July 1 dominant."),
    ],
  },
  {
    id: "csa-doles", name: "Doles's Brigade, Rodes's Division", side: "confederate",
    kfs: [
      cont("csa-doles",
        "Continuity: 'formed into line of battle about 1 p.m. July 1... the left of Major-General Rodes' division' in the plain (or-27-2-doles; strength primary 1,369)."),
      kf(T(14, 30), 4800, 8500, 175, "skirmish", 1369, "documented",
        "The left-flank moves: cavalry pickets driven, the skirmisher hill taken and contested (his '3.30 p.m.' re-occupation clock carried with its sequence ambiguity RECORDED — dossier conflict 1; CA-J1P-3's 15:00-15:30 is not moved, the Gordon=Schurz pair controls)."),
      kf(T(15, 15), 5000, 8300, 175, "line", 1330, "documented",
        "The joint knoll attack, second axis: moved by the left flank to meet the threat; Gordon's conjunction on his left (or-27-2-doles = or-27-2-gordon both-sided); the rock-fence position carried — 'driven from behind a rock fence, with heavy loss... a large number of prisoners'."),
      kf(T(15, 45), 4800, 8150, 190, "line", 1300, "documented",
        "THE 157th NY DESTRUCTION, CSA side: 'a strong force... appeared on my right flank and rear. We changed our front... attacked and routed him... But few of this force escaped us' (or-27-2-doles) — pairs the Union return's 157th NY 307-of-~409 row (p. 183) and Ewell's corps-grain credit for the two-regiment front change (or-27-2-ewell)."),
      kf(T(16, 30), 4750, 7600, 180, "line", 1250, "documented",
        "Pursuit 'across the plain in front of Gettysburg'; the rapid left-flank move to cut off the college-hill retreat — 'he retired faster than we advanced' (a pace-class negative: pursuit failed on pace, primary)."),
      kf(EndT, 4900, 6950, 175, "line", 1240, "documented",
        "'As far as the outer edge of town... form line of battle in the street running east and west through the town' (or-27-2-doles) — the night line. Decay: report=return EXACT 179 (24k/124w/31m), July-1-dominant; the friendly-fire appendix ('a severe fire from one of our own batteries... a two-gun battery (brass pieces)') rides this evening bucket."),
    ],
  },
  {
    id: "csa-bn-carter", name: "Carter's Artillery Battalion", side: "confederate",
    frontage: 350, depth: 30,
    kfs: [
      cont("csa-bn-carter", "Continuity: the Oak Hill gun line (Stone's received-enfilade clock)."),
      kf(EndT, 4050, 8600, 165, "line", 380, "documented",
        "The Oak Hill program through the afternoon (Dilger's counter-battery duel is the receiving Union half; the 45th NY's 'grape, canister, solid shot, and shell' window rides the whole line fight)."),
    ],
  },
  // ======================= EARLY'S DIVISION (CA-J1P-3) =====================
  {
    id: "csa-gordon", name: "Gordon's Brigade, Early's Division", side: "confederate",
    kfs: [
      kf(0, 5500, 9200, 190, "column", 1200, "documented",
        "NEW ENTRANT (absent from the morning file): Early's division down the Heidlersburg/Harrisburg road — the Early tablet's 'arrived about noon within two miles of Gettysburg by Harrisburg Road' is VICINITY, not the field (ED-27 verification resolved the suspicion). Strength PRIMARY: 'I carried into action about 1,200 men, one regiment having been detached' — the 26th Ga with Jones's artillery (or-27-2-gordon). Dossier csa-2c-ear-4-gordon.md (T4 full-arc)."),
      kf(T(14, 30), 5254, 8676, 170, "line", 1200, "documented",
        "Formed on the division right at Blocher's farm — j1-11 (3 p.m. sheet) 'GORDON GA' label at the farm buildings (5254,8676); the Harrisburg-road approach tablets flank the road (mon-hays-tablet-j1 / mon-hoke-tablet-j1 register entries)."),
      kf(T(15, 0), 5341, 8516, 170, "line", 1200, "documented",
        "CA-J1P-3 (adopted 15:00-15:30, ED-67 basis): 'About 3 p.m. I was ordered to move my brigade forward to the support of Major-General Rodes' left' (or-27-2-gordon) = Schurz 'about 3 o'clock... the enemy appeared in our front with heavy masses' — THE CROSS-LINE PAIR. The obstacle leg: 'over rail and plank fences, and crossing a creek [Blocher's Run] whose banks were so abrupt as to prevent a passage excepting at certain points' — deliberately slow until ~300 yards, then the rush. Drawn contact bar (5341,8516), 82 m from the Barlow statue."),
      kf(T(15, 30), 5310, 8440, 170, "line", 1100, "documented",
        "THE KNOLL TAKEN: 'the colors on portions of the two lines were separated by a space of less than 50 paces' (range-class attestation); the two-line break; BARLOW W&C — 'Among the latter was a division commander (General Barlow), who was severely wounded' (or-27-2-gordon — the captured-general pin; the reunion legend is NOT in the primary and NOT carried). mon-gordon-tablet ON the knoll, 19 m-class from the statue ground."),
      kf(T(15, 50), 5150, 8000, 175, "line", 1000, "documented",
        "Driving the second line 'in the greatest confusion' toward the almshouse ground; 'I was here ordered by Major-General Early to halt' — the advance's stop (Early's fresh-lines decision; Hays/Avery are the division's next echelon)."),
      kf(EndT, 5200, 7900, 175, "line", 990, "documented",
        "The halt line north of town at 18:00 (the York-road flank move rides the evening — or-27-2-early). Decay honesty: his 'killed and wounded was 350, of whom 40 were killed' vs return 380 (71k/270w/39m) vs Guild 323 — three count sets carried un-averaged (ED-42 discipline); the 40-vs-71 killed conflict recorded; authored ~210 in-window with the balance on the day's tail [D, flagged]. Cross-file residual: the July-2 files carry Gordon at his 1,200 engaged primary (slice report §7)."),
    ],
  },
  {
    id: "csa-hays", name: "Hays's Brigade (Louisiana), Early's Division", side: "confederate",
    kfs: [
      kf(0, 5700, 9100, 190, "column", 1200, "documented",
        "NEW ENTRANT: Early's second echelon on the Harrisburg road. Strength: WD tablet 'Present about 1200' (tablet-adjudicated hop, pass 10 — no report primary). Dossier csa-2c-ear-1-hays.md (July 1 backward-extension)."),
      kf(T(14, 45), 5450, 8700, 180, "line", 1200, "documented",
        "Formed east of the Harrisburg road on Gordon's left (Early's deployment; the approach tablet pair registered pass 10)."),
      kf(T(15, 45), 5150, 7650, 180, "line", 1180, "documented",
        "THE SECOND EFFORT: 'Hays and Avery advanced on Gordon's left, driving back into the town in great confusion the second line' (or-27-2-early) — the Coster brickyard front, both-sided (us-xi-2-1-coster.md: the brigade's fight is 'attested mainly from the division above and the enemy in front'); shared credit in the two-Napoleon capture rides the convergent ruling (dossier note)."),
      kf(T(16, 15), 4950, 7200, 185, "line", 1150, "documented",
        "Into the town with the pursuit, taking prisoners through the streets (or-27-2-hays; the XI Corps' 1,448 c/m return row is the mass's Union ledger)."),
      kf(EndT, 4980, 6900, 185, "line", 1137, "documented",
        "'One of the upper southern streets' of the town — the night line (or-27-2-hays EC3.1). Decay: his own July 1 figure 7 k / 41 w / 15 m = 63 (or-27-2-hays) → 1,137, which IS the July 2 files' t=0 strength (cross-file continuity holds exactly on this row)."),
    ],
  },
  {
    id: "csa-godwin", name: "Hoke's Brigade, Early's Division (Col. Isaac E. Avery)", side: "confederate",
    kfs: [
      kf(0, 5750, 9050, 190, "column", 900, "documented",
        "NEW ENTRANT: the three-regiment brigade (6th, 21st, 57th NC) under Col. Avery on the Harrisburg road. Strength: WD tablet 'Present about 900' (tablet-adjudicated hop; no brigade report exists — Avery mw July 2, Godwin's succession left none; the no-report negative governs). Dossier csa-2c-ear-3-avery.md."),
      kf(T(14, 45), 5550, 8650, 180, "line", 900, "documented",
        "On Hays's left east of the road (the division's second-echelon pair; mon-hoke-tablet-j1 approach ground)."),
      kf(T(15, 45), 5250, 7700, 180, "line", 890, "documented",
        "With Hays against the brickyard line — 'driving back into the town in great confusion the second line' (or-27-2-early; the 134th/154th NY pocket is the receiving ledger)."),
      kf(T(16, 30), 5450, 7100, 190, "line", 885, "documented",
        "Advanced 'to the left of it [the town] across the railroad... in the fields on the left, and facing Cemetery Hill' (or-27-2-early p. 469)."),
      kf(EndT, 5400, 6900, 190, "line", 880, "documented",
        "Sheltered 'under the cover of a low ridge' facing Cemetery Hill at 18:00 (or-27-2-early) — the ground class the July 2 jump-off tablets pin (tab-csa-2c-ear-3). July 1 decay small [D]; the brigade's spike is July 2 evening."),
    ],
  },
  {
    id: "csa-smith", name: "Smith's Brigade, Early's Division", side: "confederate",
    kfs: [
      kf(0, 5800, 9150, 190, "column", 800, "documented",
        "NEW ENTRANT: three regiments (31st, 49th, 52nd Va — 'Having left the Thirteenth and Fifty-eighth Virginia Regiments near Winchester', the structure primary, or-27-2-hoffman-smith). Strength: tablet 'Present about 800'. Dossier csa-2c-ear-2-smith.md (T3, the pass-10 Hoffman find)."),
      kf(T(15, 0), 5600, 8300, 150, "line", 800, "documented",
        "'Formed on the northward of the town' — the division's flank guard from day one (or-27-2-hoffman-smith); Early's persistent flank-anxiety detachment (the York-road cavalry reports)."),
      kf(T(16, 30), 5750, 7500, 140, "line", 800, "documented",
        "'Twice moved forward toward the town. After the enemy had been driven beyond the town, the brigade moved to the left, crossing the [Gettysburg] Railroad and York turnpike' (or-27-2-hoffman-smith)."),
      kf(EndT, 5850, 7400, 130, "line", 800, "documented",
        "The York-road flank post at 18:00 (the brigade MISSED the day's fighting — documented flank-guard stillness, ED-44 class; its July 3 action is its spike)."),
    ],
  },
  {
    id: "csa-bn-jones", name: "Jones's Artillery Battalion (Early's Division)", side: "confederate",
    frontage: 350, depth: 30,
    kfs: [
      kf(0, 5750, 9000, 190, "column", 384, "inferred",
        "NEW ENTRANT: Early's divisional artillery under Lt. Col. H. P. Jones on the Harrisburg road (division frame; the battalion's own July 1 report is not banked — flagged; the 26th Ga detached to support it, or-27-2-gordon's detachment primary)."),
      kf(T(14, 45), 5560, 8560, 190, "line", 384, "documented",
        "THE ENFILADE GUNS east of the Harrisburg road: 'completely enfilading General Barlow's line' (or-27-1-schurz — the receiving-side primary that names the break's second cause; the two-battery geometry rides his record)."),
      kf(EndT, 5500, 8400, 190, "line", 384, "documented",
        "Displaced forward with the division's advance at 18:00 (division frame; the battalion's July 2 Benner's-Hill-adjacent record rides later files)."),
    ],
  },
  // ======================= HETH'S RENEWAL + PENDER (CA-J1P-5) ==============
  {
    id: "csa-marshall", name: "Pettigrew's Brigade, Heth's Division", side: "confederate",
    kfs: [
      cont("csa-marshall",
        "Continuity: at the attack position east of Herr Ridge ('present on the first day about 2,000', July-1 scope)."),
      kf(T(14, 0), 2700, 7250, 100, "line", 2000, "documented",
        "THE AFTERNOON RENEWAL: the division's second effort with Pender behind — bracketed [D] after Davis's 1 p.m. retirement, before the ~16:00 Union retirement (dossier EC3.1, no CSA clock); the advance to the run on the 26th's front — 'the branch, the wheat-field, and the wooded hill' (or-27-2-jones-26nc); regimental order R-to-L 52d—47th—11th—26th NC."),
      kf(T(15, 0), 2950, 7280, 100, "line", 1500, "documented",
        "HERBST WOODS: the 26th NC vs the 24th Mich directly opposite — Morrow's step-by-step contested retreat vs the 26th's 'lost more than half its men killed and wounded'; Burgwyn k, Lane w 'both with the colors'; the two color ledgers face each other (24th Mich nine carriers/four k; the 26th's eleven shot down July 1, or-27-2-young-26nc). mon-26nc (2866.8,7311.2, July 1 semantics)."),
      kf(T(16, 0), 3150, 7300, 100, "line", 1150, "documented",
        "Presses to McPherson's Ridge east edge as the Iron Brigade falls back 'step by step, contesting every foot of ground, to the barricade' (or-27-1-morrow-24mi); the exhausted line then passed by Pender's fresh assault (Perrin's pass-through primary: past 'Pettigrew's exhausted line', or-27-2-perrin)."),
      kf(EndT, 3300, 7280, 100, "line", 1000, "documented",
        "Halted west of the seminary at 18:00 (the division's spent first line). Decay: the 26th NC 'went in with over 800... There came out but 216, all told, unhurt' (or-27-2-young-26nc — the regiment-grain July 1 subtraction primary); brigade July 1 ≈ 1,100 k&w class of the 1,105 k+w return (ED-49 — the return's missing column absent entirely); end ~1,000 with the ED-75 arithmetic's ~900 survivors as the night floor."),
    ],
  },
  {
    id: "csa-brockenbrough", name: "Brockenbrough's Brigade, Heth's Division", side: "confederate",
    kfs: [
      cont("csa-brockenbrough",
        "Continuity: at the renewal's attack position (Mayo-hop ~880, ED-48 basis; the no-report negative governs the whole arc)."),
      kf(T(14, 0), 2750, 7500, 100, "line", 880, "documented",
        "Committed in the afternoon renewal on the McPherson's Ridge / Stone-front sector — the receiving side documents the front (the 150th Pa's west face and the barn fight, or-27-1-stone-roy via us-i-3-2-stone.md; no brigade-authored statement exists)."),
      kf(T(15, 30), 3000, 7480, 100, "line", 850, "documented",
        "Pressing the McPherson barn line with Daniel's left (the receiving records' geometry; radius honest at ±150 m — dossier EC3 note)."),
      kf(EndT, 3200, 7420, 100, "line", 820, "documented",
        "Halted as Pender's line passed through at 18:00. Decay: the return's 148 k+w is the campaign FLOOR with July 1/July 3 unapportionable (ED-48(b)); authored ~60 as the July 1 share [D, flagged reconstruction]."),
    ],
  },
  {
    id: "csa-fry", name: "Archer's Brigade, Heth's Division", side: "confederate",
    kfs: [
      cont("csa-fry",
        "Continuity: re-formed west of Willoughby Run after the morning pocket (Shepard's 1,048 primary minus the morning's loss)."),
      kf(EndT, 2600, 7000, 112, "line", 750, "documented",
        "The brigade's afternoon is recovery, not assault (its July 1 weight was spent in the morning fight; Pettigrew/Brockenbrough carried the renewal): at dark 'we lay in position upon a road upon the right of our line' (or-27-2-shepard). July 1 loss bounded [D]: ~75 captured + the k&w share of the campaign 677 (no per-day split — flagged)."),
    ],
  },
  {
    id: "csa-davis", name: "Davis's Brigade, Heth's Division", side: "confederate",
    kfs: [
      cont("csa-davis",
        "Continuity: withdrawn at 'about 1 p.m.' west of the ridge (or-27-2-davis-jr)."),
      kf(T(15, 0), 3300, 7750, 100, "line", 1250, "documented",
        "THE RE-ADVANCE: 'About 3 p.m. [when] a division of Lieutenant-General Ewell's corps came up on our left, moving in line perpendicular to ours' (or-27-2-davis-jr) — the Rodes-progress sync read from Hill's front (CA-J1P-1 adjacency)."),
      kf(T(16, 30), 4000, 7500, 100, "line", 1220, "documented",
        "Forward along the railroad axis with the general advance (the perpendicular junction closing on the Oak Ridge seam)."),
      kf(EndT, 4400, 7300, 100, "line", 1200, "documented",
        "'Reached the suburbs of the town' (or-27-2-davis-jr) — the 18:00 state. Decay: the morning cut dominates the brigade's July 1 (7 of 9 field officers; the three engaged regiments' share of the return's k+w, ED-49 missing-absent pattern)."),
    ],
  },
  {
    id: "csa-bn-pegram", name: "Pegram's Artillery Battalion", side: "confederate",
    frontage: 450, depth: 30,
    kfs: [
      cont("csa-bn-pegram", "Continuity: the Herr Ridge gun line."),
      kf(T(17, 0), 3650, 7500, 105, "line", 460, "documented",
        "Forward to Seminary Ridge with the captured ground (the corps' guns displaced onto the ridge at the collapse — Wainwright's receiving frame names the converging preparation; division frame, trigger-seamed)."),
      kf(EndT, 3650, 7500, 105, "line", 460, "documented", "The Seminary Ridge line at 18:00."),
    ],
  },
  {
    id: "csa-bn-mcintosh", name: "McIntosh's Artillery Battalion", side: "confederate",
    frontage: 400, depth: 30,
    kfs: [
      cont("csa-bn-mcintosh", "Continuity: the Herr Ridge southern extension."),
      kf(T(16, 45), 3700, 7400, 105, "line", 384, "documented",
        "Onto Seminary Ridge behind the final push (the 'terrific fire of grape and shell on our flank' Scales received descending the ridge was UNION metal — this battalion's forward displacement follows the breakthrough; division frame, trigger-seamed)."),
      kf(EndT, 3700, 7400, 105, "line", 384, "documented", "The ridge gun line at 18:00."),
    ],
  },
  {
    id: "csa-perrin", name: "Perrin's Brigade (McGowan's), Pender's Division", side: "confederate",
    kfs: [
      cont("csa-perrin",
        "Continuity: the first supporting bound behind Heth (or-27-2-perrin's two-bound approach; ~1,600 engaged FLAGGED — the Rifles detached with the trains)."),
      kf(T(15, 0), 2650, 7350, 100, "line", 1600, "documented",
        "The second bound 'until about 3 o'clock'... ~½ mile (or-27-2-perrin); corps context: 'About 2.30 o'clock, the right wing of Ewell's corps made its appearance on my left... Pender's division was then ordered forward' (or-27-2-hill — the junction that staged the final push)."),
      kf(T(16, 5), 3100, 7200, 95, "line", 1600, "documented",
        "CA-J1P-5 — THE ASSAULT ORDER (adopted ~16:00-16:30, envelope 15:45-16:45; ED-70 basis): 'I remained in this position probably until after 4 o'clock, when I was ordered by General Pender to advance' (or-27-2-perrin) vs the receiving cluster — I Corps tablet 'At 4 P.M. the Corps retired', Dobke 'about 4 p.m.', Jacobs 'At about 4 o'clock', C. Biddle 'compelled about 4 p.m. to retire' — the cross-line agreement pair; the ravine re-form and the hold-fire order ('not to allow a gun to be fired... until they received orders')."),
      kf(T(16, 20), 3550, 7180, 95, "line", 1400, "documented",
        "THE LAST FENCE ~200 yards from the grove at the theological college: 'the brigade received the most destructive fire of musketry I have ever been exposed to'; the 14th SC staggered ('It looked to us as though this regiment was entirely destroyed'); 'General Scales' brigade had halted... General Lane's brigade did not move upon my right at all' — the unsupported-center record, both both-sided (or-27-2-perrin; or-27-2-scales; or-27-2-lane-jh)."),
      kf(T(16, 35), 3718, 7150, 95, "line", 1300, "documented",
        "THE BREAKTHROUGH: the 1st SC's right-oblique around the rail breastwork (Paul's seminary entrenchment, both-sided) + left flank attack routing the barricade; the 12th/13th right-oblique vs the stone fence to the right of the college (Gamble's dismounted line — the j1-13 drawn 'GAMBLE' bar, both-sided at last); 'at least thirty pieces' of Union artillery limbering to the rear (Wainwright's line, receiving)."),
      kf(T(17, 0), 4400, 7000, 110, "line", 1150, "documented",
        "Pursuit into the town, 'capturing hundreds of prisoners, two field-pieces, and a number of caissons' (pairs Hill's corps ledger '2 pieces of artillery and 2,300 prisoners'); the reunification order in town after the oblique split."),
      kf(EndT, 4600, 6700, 120, "line", 1050, "documented",
        "The town's southwest front at 18:00 (toward the Long Lane ground the July 2 file carries at t=0). Decay: return 100k/477w/0m = 577 July-1-dominant (or-27-2-anv-return p. 344; division row arithmetic-exact); the two left regiments 'reduced... to less than one-half' — the same-day halving statement locates the spike at the fence."),
    ],
  },
  {
    id: "csa-lowrance", name: "Scales's Brigade, Pender's Division", side: "confederate",
    kfs: [
      cont("csa-lowrance",
        "Continuity: in line at the lull behind Heth (~1,250 pre-assault basis; the decay must cross Lowrance's documented 500-man evening waypoint — dossier authoring note)."),
      kf(T(16, 5), 2900, 7500, 100, "line", 1250, "documented",
        "The advance left resting on the pike (or-27-2-scales); the pass-through of Heth's halted line ('without ammunition, and would not advance farther' — the first line's ammunition state, primary); the Davis-front rescue rides the approach ('The enemy, with their flank thus exposed to our charge, immediately gave way')."),
      kf(T(16, 20), 3600, 7350, 100, "line", 700, "documented",
        "THE SEMINARY DESCENT: 'a most terrific fire of grape and shell on our flank, and grape and musketry in our front. Every discharge made sad havoc in our line' — halted at the bottom, ~75 yards from the ridge and ~75 from the college, where SCALES FELL WOUNDED and 'only a squad here and there marked the place where regiments had rested' (or-27-2-scales); Perrin's side: 'halted... near the fence, about 200 yards distance' — the halt both-sided, the 75-vs-200-yard readings carried as the two reports' own."),
      kf(T(16, 35), 3600, 7350, 100, "line", 550, "documented",
        "'In less than ten minutes after I was disabled... the enemy... gave way' (or-27-2-scales) — the minutes-grade pin welding the brigade's wreck to Perrin's breakthrough; 'Every field officer of the brigade save one had been disabled'; Pender personally rallied the brigade (or-27-2-engelhard)."),
      kf(EndT, 3650, 7300, 100, "line", 500, "documented",
        "The seminary ground at 18:00 — 'about 500 men, without any field officers, excepting Lieutenant-Colonel Gordon and myself... In this depressed, dilapidated, and almost unorganized condition, I took command' (or-27-2-lowrance, July 1 evening — THE DOCUMENTED WAYPOINT). EC6 conflict carried: Scales's own July-1 table prints 545 vs the compiled three-day return's 535 (ED-49's starkest exemplar; never averaged)."),
    ],
  },
  {
    id: "csa-lane", name: "Lane's Brigade, Pender's Division", side: "confederate",
    kfs: [
      cont("csa-lane",
        "Continuity: the division's far right against the cavalry front (~1,700 hop, flagged)."),
      kf(T(16, 5), 3000, 6900, 100, "line", 1700, "documented",
        "Advanced on the right of the final push — and held to the cavalry front: 'Colonel Barbour threw out 40 men, under Captain Hudson, to keep back some of the enemy's cavalry, which had dismounted and were annoying us with an enfilade fire' (or-27-2-lane-jh)."),
      kf(T(16, 30), 3450, 6700, 100, "line", 1680, "documented",
        "THE DISMOUNTED STAND'S EFFECT: 'General Lane's brigade did not move upon my right at all, and was not at this time in sight of me' (or-27-2-perrin) — Gamble's carbine line demonstrably pulled the brigade off the assault; his 'saved a division of our infantry' claim graduates to three-report convergence (dossier us-cav-1-1-gamble.md, CLOSED pass 11)."),
      kf(EndT, 3650, 6550, 100, "line", 1660, "documented",
        "Up to the Fairfield-road front at 18:00 (the woods rush and the McMillan peach-orchard halt, or-27-2-lane-jh July 1 section). July 1 decay small beside the brigade's July 3 (return 660 battle total)."),
    ],
  },
  {
    id: "csa-thomas", name: "Thomas's Brigade, Pender's Division", side: "confederate",
    kfs: [
      cont("csa-thomas", "Continuity: division reserve (~1,300 hop, flagged)."),
      kf(T(16, 30), 3000, 7400, 100, "line", 1300, "documented",
        "The reserve follows the assault echelon (division frame; his brigade's July 1 non-commitment is the division records' negative space — documented stillness)."),
      kf(EndT, 3500, 7300, 100, "line", 1300, "documented",
        "Seminary Ridge reserve at 18:00 (his Long Lane skirmish day-scope opens with the July 2 file)."),
    ],
  },
  // ======================= THE I CORPS DEFENSE AND RETREAT =================
  {
    id: "us-meredith", name: "Meredith's Iron Brigade (1st Bde, 1st Div, I Corps)", side: "union",
    kfs: [
      cont("us-meredith",
        "Continuity: McPherson's woods line (Doubleday's 'must be held at all hazards')."),
      kf(T(14, 30), 3000, 7290, 285, "line", 1500, "documented",
        "The woods defense against Pettigrew's renewal: 'We had inflicted severe loss on the enemy, but their numbers were so overpowering...' (or-27-1-morrow-24mi) — the 24th Mich vs 26th NC front; the 24th's flag carried by NINE men, four color-bearers k (the Union counterpart of the Fry color ledger)."),
      kf(T(15, 45), 3300, 7290, 285, "line", 1100, "documented",
        "'Forced back, step by step, contesting every foot of ground, to the barricade' west of the seminary (or-27-1-morrow-24mi); Meredith w — succession to Col. W. W. Robinson pinned in Dawes's report."),
      kf(T(16, 15), 3700, 7200, 285, "line", 900, "documented",
        "THE SEMINARY BARRICADE STAND with Biddle/Stone remnants and Stewart's/Cooper's/Stevens's guns (or-27-1-doubleday; drawn j1-13 'MEREDITH' bar on the seminary line (3621,7292) beside Biddle with SCALES/McGOWAN red opposite)."),
      kf(T(16, 45), 4700, 6700, 160, "column", 800, "documented",
        "Through the town ('orders... to fall back, given, I believe, by Major-General Doubleday'; Dawes's faced-by-the-rear-rank retreat with the through-town crush; Morrow w scalp & c in town)."),
      kf(T(17, 30), 5620, 5680, 10, "line", 720, "documented",
        "CULP'S HILL: the brigade to the hill's west shoulder (Morrow: 'occupied Culp's Hill'; Dawes 'in open field on Culp's Hill' reporting to Col. Robinson 'now commanding the brigade') — the consolidation's I Corps right."),
      kf(EndT, 5620, 5680, 10, "line", 700, "documented",
        "The Culp's Hill line at 18:00. Decay: return p. 173 brigade 1,153 — the heaviest Union brigade aggregate of the battle, single-day concentrated ('little or no loss' July 2-4); the Morrow 316-vs-363 return reconciliation carried (print note 'But see revised statement')."),
    ],
  },
  {
    id: "us-cutler", name: "Cutler's Brigade (2nd Bde, 1st Div, I Corps)", side: "union",
    kfs: [
      cont("us-cutler",
        "Continuity: the ridge north of the pike, both flanks open (or-27-1-cutler)."),
      kf(T(14, 0), 3750, 8050, 340, "line", 1100, "documented",
        "THE 2 O'CLOCK SYNC: 'having no support on either my right or left until 2 o'clock, when a brigade from the Second Division formed on my right, and the Eleventh Corps came in on the right of them' (or-27-1-cutler) — the Baxter/XI junction clocked from the receiving line (CA-J1P-1's receiving preamble)."),
      kf(T(14, 45), 3850, 8200, 340, "line", 1080, "documented",
        "THE FLANK VOLLEY on Iverson's line: 'came in on their flank, and opened so hot a fire on them that one regiment threw down their arms and surrendered' (or-27-1-cutler) — the Iverson capture mass from the Union left (the surrendered regiment unnamed — carried, not identified)."),
      kf(T(15, 45), 3700, 7700, 320, "line", 1050, "documented",
        "Ammunition exhaustion and relief; the twenty-minute wait at the railroad; the three-regiment detachment to the seminary (or-27-1-cutler)."),
      kf(T(16, 30), 4300, 7400, 130, "column", 1000, "documented",
        "THE EMBANKMENT RETREAT: 'I moved off on the railroad embankment, and, although exposed to the enemy's fire on both flanks, the men marched with perfect steadiness and no excitement. Their steadiness had the effect to bring the enemy to a halt' (or-27-1-cutler — the formation-class primary)."),
      kf(T(17, 0), 5150, 5750, 45, "line", 960, "documented",
        "Through town to Cemetery Hill, then 'to hold the crest of a hill to the right' — the 7th Ind sent by Wadsworth: THE CULP'S HILL FIRST-OCCUPATION PIN (or-27-1-cutler; both-sided with the Greene frame)."),
      kf(EndT, 5450, 5450, 45, "line", 950, "documented",
        "Toward Culp's Hill's right at 18:00 (the 7th Ind — fresh, detached all day — rejoins at evening, its ~20-loss return row the structural control). Decay: return 1,002, the opening half-hour dominant (the rate-class primaries)."),
    ],
  },
  {
    id: "us-dana", name: "Stone's Bucktail Brigade (2nd Bde, 3rd Div, I Corps — Col. Roy Stone)", side: "union",
    kfs: [
      cont("us-dana",
        "Continuity: the double front at the McPherson barn (or-27-1-stone-roy)."),
      kf(T(13, 30), 3170, 7490, 350, "line", 1310, "documented",
        "'At about 1.30 p.m. the grand advance of the enemy's infantry began' — traced 'for at least 2 miles' (or-27-1-stone-roy) — ED-69's recorded early pole (report-nominal −20 [−45,10] profile; the CA-J1P-1 tablet 14:00 UNMOVED — rule 4)."),
      kf(T(15, 0), 3160, 7480, 350, "line", 800, "documented",
        "The three-front fight: the 149th's railroad-cut episode (the fence volley at pistol-shot, the 30-yard volley + charge, the enfilading battery forcing it out); THE CASCADE IN ACTION — Stone w → Wister 'badly wounded in the mouth and unable to speak, remained in the front' → Dana; Huidekoper and Dwight fighting wounded (or-27-1-stone-roy); Harney's capture of the 150th's colors closes it both-sided (or-27-2-ramseur)."),
      kf(T(16, 0), 3600, 7350, 300, "line", 600, "documented",
        "The fighting withdrawal 'making an occasional stand' to Seminary Ridge ('a firm stand was made and a battery brought off') (or-27-1-stone-roy)."),
      kf(T(16, 45), 4700, 6800, 150, "column", 500, "documented",
        "The town gauntlet: 'the enemy... already occupied the streets on both their flanks' (or-27-1-stone-roy); 'Nearly two-thirds of my command fell on the field. Every field officer save one was wounded.'"),
      kf(EndT, 5100, 5600, 20, "line", 465, "documented",
        "Cemetery Hill at 18:00 under Col. Dana (the July 2 battery-support day follows). Decay: return 853 of the B&M 1,317 — 'two-thirds' ✓ (the arithmetic coherence check in the dossier)."),
    ],
  },
  {
    id: "us-biddle", name: "Biddle's Brigade (1st Bde, 3rd Div, I Corps — Col. Chapman Biddle)", side: "union",
    kfs: [
      cont("us-biddle",
        "Continuity: the corps' extreme left line, Cooper's battery in the interval (or-27-1-cbiddle's 1,287 primary)."),
      kf(T(14, 30), 3140, 6960, 285, "line", 1150, "documented",
        "'Between 2 and 3 p.m. a large body of them, amounting to a division or more, advanced in two lines toward us' (or-27-1-cbiddle) — the Pettigrew-front receiving clock, both-sided with the Marshall records; Col. Cummins (142nd Pa) k, McFarland (151st) badly w."),
      kf(T(16, 0), 3660, 7100, 285, "line", 700, "documented",
        "Outflanked left; 'compelled about 4 p.m. to retire' to cover at the seminary edge (or-27-1-cbiddle — a receiving member of the CA-J1P-5/ED-70 four-source cluster)."),
      kf(T(16, 45), 4850, 6300, 160, "column", 500, "documented",
        "Held 'till the batteries and many of the troops in the town had withdrawn', then retired to the cemetery (or-27-1-cbiddle — the corps-left cover role, primary)."),
      kf(EndT, 4950, 5450, 270, "line", 390, "documented",
        "The Taneytown-road line at 18:00 ('posted in line along the Taneytown road', or-27-1-gates). STRENGTH IS THE PRIMARY: 'leaving as the present effective force only 390 officers and men' (or-27-1-cbiddle, dated July 2) — the 897-loss statement ≈ the return's 898 (coherence exact)."),
    ],
  },
  {
    id: "us-coulter", name: "Paul's Brigade (1st Bde, 2nd Div, I Corps)", side: "union",
    kfs: [
      cont("us-coulter",
        "Continuity: the seminary barricade reserve (or-27-1-robinson; B&M 1,537 hop)."),
      kf(T(15, 0), 4000, 8380, 340, "line", 1537, "documented",
        "THE RELIEF INSERTION: 'until after 3 p.m., at which time, the ammunition being exhausted, we were relieved by a portion of the First Brigade' (or-27-1-coulter — the Paul-for-Baxter relief clocked at the incoming echelon); brought up by Robinson in person; 'part of it in the position first occupied by Baxter's brigade' (or-27-1-robinson)."),
      kf(T(16, 0), 3950, 8300, 340, "line", 1200, "documented",
        "The relieved line's fight: 'The enemy now made repeated attacks on the division, in all of which he was handsomely repulsed' (or-27-1-robinson); PAUL FELL severely wounded (shot through both eyes — literature-grade nature, flagged) — the FIVE-COMMANDER CASCADE (Paul → Leonard w → Root w → Coulter, with his 11th Pa); the 16th Maine ordered 'to take possession of a hill which commanded the road, and hold the same as long as there was a man left' (or-27-1-farnham-16me — the sacrifice order verbatim)."),
      kf(T(16, 45), 4750, 6900, 160, "column", 900, "documented",
        "The town retreat under the 16th Maine's cover (its three-stage collapse: hollow → woods → 'not in time to reach the main body'; the regiment's 162-missing column IS the sacrifice hill's capture pocket); Robinson's division the ordered rear guard, 'until nearly 5 p.m.' (the ED-70 recorded back-edge tail)."),
      kf(T(17, 30), 5000, 5700, 20, "line", 780, "documented",
        "Cemetery Hill; Coulter assigned to the brigade command 'taking his regiment with him' (or-27-1-baxter = or-27-1-coulter, both echelons)."),
      kf(EndT, 5000, 5700, 20, "line", 761, "documented",
        "The hill line at 18:00. Decay: THE DAY-SPLIT PRIMARY — July 1 776 · July 2 28 · July 3 14 · July 4 3 = 821 (or-27-1-coulter's table, the only per-day brigade table in the division; 'Many reported missing... were killed or wounded. Some were recovered on re-entering the town.')."),
    ],
  },
  {
    id: "us-baxter", name: "Baxter's Brigade (2nd Bde, 2nd Div, I Corps)", side: "union",
    kfs: [
      cont("us-baxter",
        "Continuity: filing onto the Oak Ridge crest (the 400-yards-vs-half-a-mile XI-gap variance carried)."),
      kf(T(13, 30), 4007, 8424, 340, "line", 1452, "documented",
        "THE WALL LINE: 'moved forward to the crest of the hill' — the j1-09 drawn 'BAXTER'S BRIGADE' bar at the Mummasburg-road wall (4007,8424), drawn=report agreement (or-27-1-baxter; the pass-10 read)."),
      kf(T(14, 40), 4007, 8424, 340, "line", 1400, "documented",
        "THE IVERSON REPULSE (CA-J1P-2 receiving executor): 'the brigade opened on the advancing foes a most deadly fire, soon causing them to recoil and give way'; the charge (97th NY + 83rd NY + 88th Pa) 'capturing many prisoners, the Eighty-eighth Pennsylvania taking two battle-flags and the Ninety-seventh New York one'; 'The Twelfth Massachusetts had a galling fire on the flank of this brigade at this time, which I think had a great influence upon its surrender' (or-27-1-baxter)."),
      kf(T(15, 15), 3750, 7900, 320, "line", 1250, "documented",
        "Relieved ~15:00 with ammunition exhausted ('engaged over two hours', his duration primary; Coulter's relief clock) — to Stewart's battery support (or-27-1-baxter)."),
      kf(T(16, 30), 4700, 7100, 150, "column", 1000, "documented",
        "The town retreat along the railroad, 'suffering most severely... galling fire of musketry and artillery' (or-27-1-coulter's grain); Col. Wheelock (97th NY) captured (later escaped), Lt. Thomas (staff) k by shell in the streets, Lt. Knaggs captured (or-27-1-baxter — the streets' named cost)."),
      kf(T(17, 15), 5100, 5550, 30, "line", 850, "documented",
        "Cemetery Hill re-form; 'About 5 o'clock... from Cemetery Hill to the left and forward, near and parallel with the Emmitsburg road' with temporary breastworks (or-27-1-baxter — the stated evening move)."),
      kf(EndT, 4800, 5450, 280, "line", 800, "documented",
        "The forward-left line at 18:00 (relieved by Webb next morning — both-sided with us-ii-2-2-webb). Decay: division 'lost considerably more than half' of <2,500 (or-27-1-robinson primary); brigade rows p. 174 not digit-read (flagged)."),
    ],
  },
  // ---------------------- I CORPS BATTERIES (the withdrawal ladder) --------
  {
    id: "us-btty-hall-2me", name: "Hall's 2nd Maine Battery (B)", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-hall-2me", "Continuity: refitted in rear after the pike fight."),
      kf(T(17, 0), 5200, 5300, 0, "column", 120, "documented",
        "To the hill with the corps' artillery reflux (Wainwright's group reorganization; the battery's morning fight was its July 1 — 'rear, withdrawn' state)."),
      kf(EndT, 5200, 5300, 0, "column", 120, "documented", "In reserve behind the hill at 18:00."),
    ],
  },
  {
    id: "us-btty-breck", name: "Reynolds's Battery L (with E), 1st New York Light", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-breck", "Continuity: the Seminary Ridge line (or-27-1-wainwright)."),
      kf(T(16, 30), 3900, 7200, 285, "line", 138, "documented",
        "The fighting withdrawal under the final push: the right section covering from 'some 75 yards to the rear', the last piece's horses all shot, Wadsworth's 'peremptory order' to the heights (or-27-1-wainwright pp. 355-357 — the group's July 1 withdrawal record; Reynolds's battery in its ladder)."),
      kf(T(17, 15), 5090, 5720, 50, "line", 135, "documented",
        "EAST CEMETERY HILL, the layout verbatim: '...next Cooper's battery and then Reynolds'' — thirteen 3-inch guns on the north front (or-27-1-wainwright p. 357)."),
      kf(EndT, 5090, 5720, 50, "line", 135, "documented", "The north-front gun line at 18:00."),
    ],
  },
  {
    id: "us-btty-cooper", name: "Cooper's Battery B, 1st Pennsylvania Light", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-cooper", "Continuity: Biddle's interval on the corps left (or-27-1-cbiddle)."),
      kf(T(16, 0), 3730, 7180, 285, "line", 114, "documented",
        "Displaced to the seminary as the left folded (us-btty-cooper.md dossier; the barricade group with Stewart/Stevens — or-27-1-doubleday's covering-stand record)."),
      kf(T(16, 40), 4400, 6600, 150, "column", 113, "documented",
        "Withdrawn under the peremptory order through the town's south edge (Wainwright's ladder)."),
      kf(T(17, 15), 5100, 5740, 10, "line", 113, "documented",
        "East Cemetery Hill: 'four guns of Battery I... on the left; next Cooper's battery' (or-27-1-wainwright p. 357 — the layout roster)."),
      kf(EndT, 5100, 5740, 10, "line", 113, "documented", "The hill line at 18:00."),
    ],
  },
  {
    id: "us-btty-stevens", name: "Stevens's 5th Maine Battery (E)", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-stevens", "Continuity: the seminary ground with the group."),
      kf(T(16, 20), 3730, 7150, 285, "line", 136, "documented",
        "The barricade canister fight against the final push (the group's 'canister at the charge' record; Wainwright's 83-casualty day is the brigade's July 1 pin)."),
      kf(T(17, 15), 5380, 5680, 15, "line", 134, "documented",
        "'The Fifth Maine battery was posted to the right and some 50 yards in front of this line, on a small knoll' (or-27-1-wainwright p. 357) — the McKnight's-Knoll relation (mon-5me-stevens ground)."),
      kf(EndT, 5380, 5680, 15, "line", 134, "documented", "The knoll at 18:00."),
    ],
  },
  {
    id: "us-btty-stewart", name: "Stewart's Battery B, 4th US Artillery", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-stewart", "Continuity: the railroad-cut straddle position."),
      kf(T(16, 20), 3640, 7660, 300, "line", 128, "documented",
        "The cut defense at the collapse: double canister down the pike front as the corps folded (the position's storied afternoon; Lt. Davison w — Sgt. Mitchell commanding, or-27-1-wainwright's officer-level commendation)."),
      kf(T(16, 50), 4750, 6600, 150, "column", 125, "documented",
        "Withdrawn through the town under fire (Wainwright's ladder; Baxter's brigade its infantry support in the fall-back)."),
      kf(T(17, 15), 5055, 5845, 350, "line", 125, "documented",
        "East Cemetery Hill: 'Four guns of Battery B, Fourth U.S. Artillery, across the road so as to command the approaches from the town' (or-27-1-wainwright p. 357, verbatim layout)."),
      kf(EndT, 5055, 5845, 350, "line", 125, "documented", "Commanding the town approaches at 18:00."),
    ],
  },
  // ======================= THE XI CORPS (CA-J1P-3/4) =======================
  {
    id: "us-vonamsberg", name: "Schimmelfennig's Brigade (1st Bde, 3rd Div, XI Corps — Col. von Amsberg)", side: "union",
    kfs: [
      cont("us-vonamsberg",
        "Continuity: the skirmish line north of town under the Oak Hill fire (or-27-1-dobke-45ny)."),
      kf(T(13, 30), 4300, 8250, 340, "skirmish", 1650, "documented",
        "THE 1.30 FLANK FIRE: 'At about 1.30 p.m. a long line of the enemy moved on the extreme right of the First Corps, passing the left of the Forty-fifth, and offering the flank' — fire at 50-100 yards; 'the whole of the enemy's line halted, gradually disappeared on the same spot where they stood, and the remainder... surrendered, partly to the First Corps and a great number to the Forty-fifth' (or-27-1-dobke-45ny) — the O'Neal/Iverson-front surrender event from the THIRD receiving echelon (ED-69 early-lean profile carried; no anchor moved)."),
      kf(T(15, 0), 4500, 8200, 350, "skirmish", 1500, "documented",
        "The open-field line under 'grape, canister, solid shot, and shell' from deployment to retreat (or-27-1-dobke-45ny); the 157th NY detached leftward against Doles's flank by Schurz's order — routed, return row 307 of ~409 (p. 183; both-sided with or-27-2-doles)."),
      kf(T(16, 15), 4550, 7500, 180, "line", 1200, "documented",
        "~16:00 ordered back (CA-J1P-4: the corps tablet's 'About 4 P.M. the Corps was forced back'); THE SEMINARY RALLY: 'ordered to halt and cover the retreat of the First and Eleventh Corps through Gettysburg' — a cross-CORPS cover task, primary (or-27-1-dobke-45ny)."),
      kf(T(16, 45), 4850, 6900, 175, "column", 1000, "documented",
        "THE TOWN MAZE: the 45th NY's street-by-street track (blocks, the panic column, the alley trap) — the audit's most granular town-retreat leg; 'About 100 of the Forty-fifth Regiment extricated themselves... and arrived safely at the graveyard' (or-27-1-dobke-45ny); Schimmelfennig cut off and hidden in town until July 4."),
      kf(EndT, 4900, 5655, 300, "line", 863, "documented",
        "The Cemetery Hill stone fence at 18:00. Decay: return p. 183 digit-grain — brigade 807 (rows sum exactly; 45th NY 224, 157th NY 307), the missing column (453) dominated by the town trap."),
    ],
  },
  {
    id: "us-krzyzanowski", name: "Krzyzanowski's Brigade (2nd Bde, 3rd Div, XI Corps)", side: "union",
    kfs: [
      kf(0, 4700, 6300, 355, "column", 1403, "documented",
        "NEW ENTRANT: 'About 2 p.m. on July 1, the regiment arrived... and was ordered at once to the front' (or-27-1-jacobs-26wi, secondhand-flagged — used because it agrees with von Steinwehr's independent 2 o'clock; the t=0 column is the approach's edge). Strength: B&M-repro 1,403 (hop flagged; 82nd Ohio 312 PFD the regiment primary). Dossier us-xi-3-2-krzyzanowski.md."),
      kf(T(14, 30), 4800, 8200, 355, "line", 1403, "documented",
        "The line north of town: brigade front with the 82nd Ohio on its left (or-27-1-thomson-82oh), 26th Wis the extreme right (or-27-1-jacobs-26wi) — the open-field line the knoll collapse uncovered."),
      kf(T(15, 15), 4950, 8250, 355, "line", 1200, "documented",
        "THE OPEN-FIELD STAND (~14:30-16:00, bracketed by CA-J1P-3/4): 'furiously attacked by vastly superior numbers, but held its own until ordered... to retreat' (or-27-1-jacobs-26wi; the good-order claim carried as stated — the loss figures say the cost); Gordon's 50-paces colors passage and Doles's conjunction are the attacking half."),
      kf(T(16, 15), 4850, 7300, 180, "column", 1000, "documented",
        "The fall-back through town by division order; Col. James S. Robinson (82nd Ohio) severely w entering the town (or-27-1-thomson-82oh); the 26th Wis command cascade Boebel → Fuchs (or-27-1-jacobs-26wi)."),
      kf(T(16, 30), 5065, 5625, 320, "line", 800, "documented",
        "THE RALLY CLOCK: 'At about 4 o'clock... on Cemetery Hill... behind the stone fence' (or-27-1-jacobs-26wi — pairing the corps tablet's 'About 4 P.M.')."),
      kf(EndT, 5065, 5625, 320, "line", 734, "documented",
        "The stone-fence line at 18:00. Decay: return p. 183 digit-grain — brigade 669 (report=return EXACT on the 82nd Ohio's 181); k/w in the ~90-minute stand, the missing (206) in the town."),
    ],
  },
  {
    id: "us-vongilsa", name: "von Gilsa's Brigade (1st Bde, 1st Div, XI Corps)", side: "union",
    kfs: [
      kf(0, 4750, 6350, 355, "column", 800, "documented",
        "NEW ENTRANT: Barlow's division through the town (the corps' second column). STRENGTH HONESTY: no primary exists (dossier EC2 open); the knoll line was THREE regiments — the 41st NY arrived ~10 p.m. with the trains (or-27-1-einsiedel-41ny) — authored ~800 compilation-class FLAGGED. Dossier us-xi-1-1-vongilsa.md (T2, honestly thin — no brigade report exists; the record is the attackers' + Schurz's)."),
      kf(T(14, 15), 5100, 8250, 350, "line", 800, "documented",
        "Deployed right of the 3rd Division, then ADVANCED to the knoll — Barlow 'had moved forward his whole line, thus losing on his left the connection with the Third Division' (or-27-1-schurz — the forward-tilt record of an order Schurz did not give; Barlow's own letters remain the unfetched agency record, dossier flag)."),
      kf(T(14, 45), 5350, 8480, 350, "line", 800, "documented",
        "THE KNOLL LINE's right/north face (drawn j1-11: the small blue bars on the knoll's north face under the red Gordon line); von Gilsa's July 1 ground has NO located brigade marker (negative recorded; his ECH marker is July 2-3 semantics)."),
      kf(T(15, 40), 5150, 8100, 185, "routed", 600, "documented",
        "THE BREAK (~15:00-15:30 bracketed by the Gordon/Schurz pair): under the Harrisburg-road battery enfilade + Gordon's fence-and-creek assault — 'forced back... not, however, without hotly contesting every inch of ground' (Schurz) vs Ames's 'the men of the First Brigade... running through lines of the regiments of my brigade' — THE WHICH-BROKE-FIRST CONFLICT carried whole, neither adopted (dossier conflict 1)."),
      kf(T(16, 30), 5000, 6900, 180, "column", 450, "documented",
        "Through town to Cemetery Hill (the corps' 1,448 c/m capture mass is this leg's cost signature)."),
      kf(EndT, 5130, 5860, 10, "line", 400, "documented",
        "The East Cemetery Hill stone fences at 18:00 (the pass-8 ECH ground's first evening). Decay: return p. 182 — brigade 527 with the 41st NY's 75 all July 2-3 (the structural day-split anchor): the knoll's three regiments carry ~451 minus their later residue."),
    ],
  },
  {
    id: "us-harris", name: "Ames's Brigade (2nd Bde, 1st Div, XI Corps)", side: "union",
    kfs: [
      kf(0, 4780, 6400, 355, "column", 1300, "documented",
        "NEW ENTRANT: Barlow's second brigade under Brig. Gen. Adelbert Ames. Strength: brigade aggregate OPEN (dossier EC2) — ~1,300 compilation-class FLAGGED (the 75th OH's 'about 160... increased to 91' arithmetic is the wreckage's regiment-grain signal). Dossier us-xi-1-2-harris.md (July 1 extension, pass 10)."),
      kf(T(14, 15), 5250, 8380, 350, "line", 1300, "documented",
        "The knoll crest with the division's forward tilt: the brigade's crest line DRAWN on j1-11 — the '17 Ct / 25 Ohio' bar (5366,8448), 57 m from the Barlow statue (the pass-10 headline cross-check); '75 Ohio' bar (5117,8457); '107 Ohio' second line; Wilkeson's guns beside (dossier EC3.0)."),
      kf(T(15, 30), 5310, 8420, 350, "line", 1050, "documented",
        "THE KNOLL FIGHT, receiving side: Gordon's 'most obstinate resistance until the colors on portions of the two lines were separated by a space of less than 50 paces' IS this line's fight record (the attacker attests the defense); BARLOW W&C — Ames to the division, Harris (75th OH) to the brigade late in the day (or-27-1-ames; or-27-1-harris); Lt. Col. Fowler (17th CT) k."),
      kf(T(15, 50), 5150, 8000, 185, "line", 900, "documented",
        "Back through the almshouse line in the general break (Ames's running-through-my-regiments reading carried beside Schurz's — the conflict rides the von Gilsa row)."),
      kf(T(16, 30), 5050, 6900, 180, "column", 750, "documented",
        "Through town with the corps' retreat (CA-J1P-4's north-of-town component)."),
      kf(EndT, 5170, 5790, 40, "line", 700, "documented",
        "The ECH base wall at 18:00 ('a stone wall on the right, and nearly at right angles with the Baltimore road', or-27-1-harris — the mon-25-75oh / mon-17ct wall line). Decay: return 778 THREE-DAY-weighted, mostly July 1 (flagged); cross-file residual: the July 2 file's t=0 565 nets the July 2 share differently (slice report §7)."),
    ],
  },
  {
    id: "us-coster", name: "Coster's Brigade (1st Bde, 2nd Div, XI Corps)", side: "union",
    kfs: [
      kf(0, 4800, 6200, 340, "column", 1156, "documented",
        "NEW ENTRANT: von Steinwehr's division arrived ~2 o'clock (or-27-1-vonsteinwehr). Strength: B&M-repro 1,156 (hop flagged; NO report filed by Coster or any successor — the third XI no-report brigade; the record is the division's + the enemy's). Dossier us-xi-2-1-coster.md (T2; the brickyard fight T3 via the both-sided record)."),
      kf(T(14, 15), 5000, 5800, 20, "line", 1156, "documented",
        "Cemetery Hill first: 'I placed the First Brigade, Col. Charles R. Coster, on the northeast end of the hill, in support of Wiedrich's battery' — one regiment skirmished forward, one 'into a large stone church and the surrounding houses in town' (or-27-1-vonsteinwehr)."),
      kf(T(15, 45), 5049, 7543, 10, "line", 1150, "documented",
        "THE COMMITMENT: Schurz's second reinforcement call — 'I ordered Colonel Coster to advance with his brigade through the town... Colonel Coster met General Schurz in town, who ordered him to take a position north and east of Gettysburg, and to check the advance of the enemy' (or-27-1-vonsteinwehr) — the brickyard line NE of town (Kuhn's brickyard ground, ±100 m; the 73rd Pa stayed on the hill)."),
      kf(T(16, 15), 5049, 7543, 10, "scattered", 750, "documented",
        "THE BRICKYARD WRECK: 'Colonel Coster had a severe engagement with the advancing enemy' (division grain) — the CSA side is the record's muscle (Hays's and Avery's converging assault, pass-10 executors); the 134th NY (252) and 154th NY (200) rows are its minutes-scale ledger — a single-spike EC6 case second only to Iverson's on this field."),
      kf(T(16, 40), 4970, 6300, 0, "column", 650, "documented",
        "'Not strong enough to restore the battle... again took up his position on Cemetery Hill, leaving one regiment to occupy the nearest brick houses of the town, which successfully prevented the farther advance of the enemy' (or-27-1-vonsteinwehr) — the brick-house stopper, the brigade's second delaying artifact."),
      kf(EndT, 4935, 5765, 340, "line", 620, "documented",
        "The hill's NE front at 18:00. Decay: return p. 183 — 597 (aggregate sums exactly; the missing cell row-forced, printed-noise class flagged)."),
    ],
  },
  {
    id: "us-smith", name: "Orland Smith's Brigade (2nd Bde, 2nd Div, XI Corps)", side: "union",
    kfs: [
      kf(0, 4780, 6150, 340, "column", 1600, "documented",
        "NEW ENTRANT: von Steinwehr's second brigade to Cemetery Hill with the division's 2 o'clock arrival (or-27-1-vonsteinwehr). Strength ~1,600 compilation class (hop flagged). The brigade's July 1 role is the hill's garrison anchor — the reserve that made the rally position real BEFORE the retreat reached it (Howard's 11:30 selection, or-27-1-howard/or-27-1-schurz)."),
      kf(T(14, 15), 4880, 5610, 280, "line", 1600, "documented",
        "The hill's west/southwest front along the Taneytown-road shoulder (division deployment, or-27-1-vonsteinwehr) — the line the re-forming corps dressed on."),
      kf(EndT, 4880, 5610, 280, "line", 1595, "documented",
        "The garrison line at 18:00, the consolidation's anchor (Doubleday: 'our lines, with those of the Eleventh Corps, were reformed under the direction of Major-General Howard' — the third-party rally attestation)."),
    ],
  },
  // ---------------------- XI CORPS BATTERIES -------------------------------
  {
    id: "us-btty-dilger", name: "Dilger's Battery I, 1st Ohio Light", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-dilger", "Continuity: the Carlisle-road gun position behind the 45th NY."),
      kf(T(15, 30), 4750, 8050, 355, "line", 127, "documented",
        "The counter-battery duel with Carter's Oak Hill line and canister against the infantry (the brigade-frame position primary, or-27-1-dobke-45ny; the corps' one long-service battery of the day)."),
      kf(T(16, 20), 4850, 7000, 180, "column", 126, "documented",
        "Covering the corps' withdrawal by sections down the Baltimore-street axis (the XI artillery's retreat ladder — corps frame, trigger-seamed on CA-J1P-4)."),
      kf(EndT, 5035, 5705, 350, "line", 126, "documented", "The Cemetery Hill plateau at 18:00."),
    ],
  },
  {
    id: "us-btty-wheeler", name: "Wheeler's 13th New York Independent Battery", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 4650, 6350, 355, "column", 118, "inferred",
        "NEW ENTRANT: with the corps' second artillery echelon through town (corps frame; the battery's own July 1 clock not banked — flagged; dossier us-btty-wheeler.md)."),
      kf(T(14, 0), 4780, 7950, 355, "line", 118, "documented",
        "Beside Dilger on the Carlisle-road front (the division's north-front gun pair; Schurz's division frame)."),
      kf(T(16, 20), 4900, 7000, 180, "column", 117, "documented",
        "The withdrawal under fire with the corps (the battery's dragged-section episode rides its dossier's July 1 note)."),
      kf(EndT, 5075, 5560, 350, "line", 117, "documented", "The plateau at 18:00."),
    ],
  },
  {
    id: "us-btty-bancroft", name: "Wilkeson's Battery G, 4th US Artillery", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 4800, 6450, 355, "column", 124, "documented",
        "NEW ENTRANT: with Barlow's column to the knoll (corps frame). Named for July 1 under Lt. Bayard Wilkeson (the register's July 3 name carries Bancroft, his successor)."),
      kf(T(14, 15), 5290, 8390, 350, "line", 124, "documented",
        "THE KNOLL GUNS: the battery on Barlow's crest beside Ames's line (the knoll cluster's drawn/monument frame); WILKESON MW here directing the guns under the converging Jones-battalion enfilade (register-grade pin, compilation-corroborated; carried at that honesty grade — no OR battery report banked, flagged)."),
      kf(T(15, 45), 5150, 7980, 185, "column", 120, "documented",
        "Withdrawn under Lt. Bancroft through the almshouse line with the division's break (corps frame)."),
      kf(EndT, 4975, 5625, 350, "line", 120, "documented", "The Cemetery Hill plateau at 18:00."),
    ],
  },
  {
    id: "us-btty-wiedrich", name: "Wiedrich's Battery I, 1st New York Light", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      kf(0, 4850, 6100, 20, "column", 141, "documented",
        "NEW ENTRANT: with von Steinwehr's reserve to Cemetery Hill ~2 o'clock (or-27-1-vonsteinwehr — Coster 'in support of Wiedrich's battery' names the position)."),
      kf(T(14, 15), 5115, 5795, 20, "line", 141, "documented",
        "THE EAST CEMETERY HILL LUNETTES: the battery on the hill's northeast face (or-27-1-vonsteinwehr; Wainwright's later layout 'four guns of Battery I, First New York Artillery... on the left' — the two-hill command seam recorded on both dossiers)."),
      kf(EndT, 5115, 5795, 20, "line", 141, "documented",
        "The lunettes at 18:00, firing over the retreat (Devin's friendly-fire record — 'a heavy fire of shells... from one of our own batteries on Cemetery Hill' — names A hill battery, candidate THIS one, NOT adopted; carried unresolved on both rows)."),
    ],
  },
  // ======================= BUFORD'S DIVISION (the left) ====================
  {
    id: "us-cav-gamble", name: "Gamble's Brigade (1st Bde, 1st Div, Cavalry Corps)", side: "union",
    kfs: [
      cont("us-cav-gamble", "Continuity: covering the corps left toward the Fairfield road."),
      kf(T(16, 0), 3714, 6488, 290, "skirmish", 1560, "documented",
        "THE AFTERNOON STAND: 'ordered my brigade forward at a trot' — 'deployed in line on the ridge of woods, with the seminary on our right', half the command dismounted 'behind a portion of a stone wall and under cover of trees' (or-27-1-gamble); DRAWN on j1-13 (4 p.m. sheet): the 'GAMBLE' bar at (3714,6488), seminary on the line's right exactly as reported; 'we opened a sharp and rapid carbine fire, which killed and wounded so many of the first line... that it fell back upon the second line' (Lane's front — the three-report convergence)."),
      kf(T(16, 45), 4200, 6100, 180, "column", 1550, "documented",
        "The mounted withdrawal when nearly enveloped (or-27-1-gamble); 'This brigade had the honor to commence the fight in the morning and close it in the evening.'"),
      kf(EndT, 4300, 5700, 270, "line", 1540, "documented",
        "Withdrawn 'to the next ridge, on the left of the town' at 18:00; the division 'bivouacked that night on the left of our position, with pickets extending almost to Fairfield' (or-27-1-buford). Decay: the return's campaign 99 spread across the day's two episodes (no per-day split — flagged)."),
    ],
  },
  {
    id: "us-cav-devin", name: "Devin's Brigade (2nd Bde, 1st Div, Cavalry Corps)", side: "union",
    kfs: [
      cont("us-cav-devin", "Continuity: massing toward the York road as the XI relief completed."),
      kf(T(14, 30), 5500, 7900, 30, "skirmish", 1085, "documented",
        "The York-road post: massed 'on the right of the York road', pickets 'advanced three-quarters of a mile' (or-27-1-devin)."),
      kf(T(16, 15), 5000, 6600, 180, "column", 1080, "documented",
        "Shelled off the post BY A UNION BATTERY and through the town — 'a heavy fire of shells was opened on us from one of our own batteries on Cemetery Hill, immediately in my rear... many of the shells bursting among us'; 'the column being shelled the whole distance' (or-27-1-devin — THE DAY'S UNION-ON-UNION RECORD, battery unidentified, carried unresolved)."),
      kf(T(17, 0), 4800, 6200, 0, "skirmish", 1080, "documented",
        "Re-formed 'in rear of the batteries of the division, with its right flank resting on the town'; the 9th NY squadron dismounted 'who, with their carbines, drove them some distance into the town, punishing them severely' — the last Union offensive act north of town on July 1 (or-27-1-devin)."),
      kf(EndT, 4500, 5700, 270, "column", 1080, "documented",
        "'Ordered to the extreme left, where it bivouacked' (or-27-1-devin) — moving to the division's night ground at 18:00. Decay: return 28 campaign aggregate — the screen's low-contact profile."),
    ],
  },
  {
    id: "us-btty-calef", name: "Calef's Battery A, 2nd US Artillery", side: "union",
    frontage: 100, depth: 30,
    kfs: [
      cont("us-btty-calef", "Continuity: with Gamble's brigade toward the left."),
      kf(T(16, 0), 3730, 6500, 290, "line", 118, "documented",
        "With the afternoon stand on the Fairfield-road ridge (the brigade's attached battery — or-27-1-gamble's composition primary; Buford's twelve-gun concentric passage)."),
      kf(T(17, 0), 4300, 5750, 270, "column", 118, "documented",
        "Withdrawn with the brigade toward the left-of-town bivouac ground."),
      kf(EndT, 4300, 5750, 270, "column", 118, "documented",
        "The night ground at 18:00 (tablet loss 12 w / 13 horses across the day's two stands)."),
    ],
  },
];

// ---------------------------------------------------------------------------
// Events — the afternoon's fire windows.
// ---------------------------------------------------------------------------
const events: EngagementEvent[] = [
  // Oak Hill / Carlisle road artillery.
  fireEvent({
    id: "j1p-carter-oakhill", kind: "artillery_fire", unitId: "csa-bn-carter",
    t0: 0, t1: T(16, 30), confidence: "documented",
    citation: "The Oak Hill enfilade program continued from its 12-1 opening (or-27-1-stone-roy receiving) through the division's attack (the 45th NY's 'grape, canister, solid shot, and shell' window).",
  }),
  fireEvent({
    id: "j1p-dilger-duel", kind: "artillery_fire", unitId: "us-btty-dilger",
    t0: T(13, 15), t1: T(16, 15), confidence: "documented",
    citation: "The Carlisle-road counter-battery duel with Carter's line and canister on the infantry (or-27-1-dobke-45ny's brigade-frame position; the corps front's gun anchor).",
  }),
  fireEvent({
    id: "j1p-wheeler-north", kind: "artillery_fire", unitId: "us-btty-wheeler",
    t0: T(14, 15), t1: T(16, 15), confidence: "documented",
    citation: "The north-front pair's fire beside Dilger (division frame).",
  }),
  fireEvent({
    id: "j1p-pegram-afternoon", kind: "artillery_fire", unitId: "csa-bn-pegram",
    t0: T(14, 0), t1: T(16, 45), confidence: "documented",
    citation: "The Herr Ridge line supporting the renewal and the final push (Scales's received 'grape and shell on our flank' descending the ridge names the UNION half; this window is the CSA preparation Wainwright's line answered).",
  }),
  fireEvent({
    id: "j1p-mcintosh-afternoon", kind: "artillery_fire", unitId: "csa-bn-mcintosh",
    t0: T(15, 30), t1: T(16, 45), confidence: "documented",
    citation: "The battalion into the preparation for Pender's assault (division frame; trigger-seamed on Hill's 2:30 junction order).",
  }),
  // Rodes's attack (CA-J1P-1/2).
  fireEvent({
    id: "j1p-oneal-attack", kind: "musketry", unitId: "csa-oneal",
    t0: T(14, 0), t1: T(14, 35), confidence: "documented",
    citation: "The 14:00 wave against Baxter's line — repulsed 'almost instantaneously' (or-27-2-iverson's ledger; or-27-1-baxter receiving).",
  }),
  fireEvent({
    id: "j1p-iverson-field", kind: "musketry", unitId: "csa-iverson",
    t0: T(14, 20), t1: T(15, 5), confidence: "documented",
    citation: "The Forney-field advance and the 100-yard fight (CA-J1P-2; or-27-2-iverson; the j1-09 drawn mid-advance state).",
  }),
  fireEvent({
    id: "j1p-baxter-wall", kind: "musketry", unitId: "us-baxter",
    t0: T(14, 0), t1: T(15, 15), confidence: "documented",
    citation: "The wall line's 'most deadly fire' and the flag-taking charge (or-27-1-baxter; 'engaged over two hours' to ammunition exhaustion).",
  }),
  fireEvent({
    id: "j1p-cutler-flank", kind: "musketry", unitId: "us-cutler",
    t0: T(14, 30), t1: T(15, 30), confidence: "documented",
    citation: "The flank volley on the Iverson front and the changed-front fight (or-27-1-cutler).",
  }),
  fireEvent({
    id: "j1p-daniel-cuts", kind: "musketry", unitId: "csa-daniel",
    t0: T(14, 0), t1: T(16, 30), confidence: "documented",
    citation: "The railroad-cut fight and the second effort ('losing nearly one-third of their number', or-27-2-daniel; both-sided with the 149th Pa's episode).",
  }),
  fireEvent({
    id: "j1p-ramseur-stroke", kind: "musketry", unitId: "csa-ramseur",
    t0: T(15, 0), t1: T(16, 45), confidence: "documented",
    citation: "The en-masse flank attack and the pursuit (or-27-2-ramseur; bracketed 14:30-16:00 by the division sequence + the receiving side).",
  }),
  fireEvent({
    id: "j1p-coulter-relief", kind: "musketry", unitId: "us-coulter",
    t0: T(15, 0), t1: T(16, 45), confidence: "documented",
    citation: "The relief line's repeated repulses and the rear-guard fight to 'nearly 5 p.m.' (or-27-1-robinson; or-27-1-coulter; the 16th Maine's hill stand inside it).",
  }),
  // The Heth renewal front.
  fireEvent({
    id: "j1p-pettigrew-woods", kind: "musketry", unitId: "csa-marshall",
    t0: T(14, 0), t1: T(16, 15), confidence: "documented",
    citation: "The McPherson's Ridge renewal — the branch, the wheat-field, and the wooded hill (or-27-2-jones-26nc; the 26th NC's half-loss fight).",
  }),
  fireEvent({
    id: "j1p-meredith-woods", kind: "musketry", unitId: "us-meredith",
    t0: T(14, 0), t1: T(16, 40), confidence: "documented",
    citation: "The woods defense, the step-by-step retreat, and the barricade stand (or-27-1-morrow-24mi; or-27-1-doubleday).",
  }),
  fireEvent({
    id: "j1p-brockenbrough-barn", kind: "musketry", unitId: "csa-brockenbrough",
    t0: T(14, 15), t1: T(16, 15), confidence: "documented",
    citation: "The Stone-front pressure (the receiving side's record; no brigade-authored statement exists — ED-48 honesty).",
  }),
  fireEvent({
    id: "j1p-stone-threefront", kind: "musketry", unitId: "us-dana",
    t0: T(13, 30), t1: T(16, 30), confidence: "documented",
    citation: "The double-front fight from the 1.30 grand advance to the fighting withdrawal (or-27-1-stone-roy).",
  }),
  fireEvent({
    id: "j1p-biddle-left", kind: "musketry", unitId: "us-biddle",
    t0: T(14, 30), t1: T(16, 20), confidence: "documented",
    citation: "The left line vs Pettigrew's right 'between 2 and 3 p.m.' to the ~4 p.m. retirement (or-27-1-cbiddle).",
  }),
  fireEvent({
    id: "j1p-davis-readvance", kind: "musketry", unitId: "csa-davis",
    t0: T(15, 0), t1: T(16, 45), confidence: "documented",
    citation: "The 3 p.m. re-advance to the suburbs (or-27-2-davis-jr).",
  }),
  // The knoll (CA-J1P-3).
  fireEvent({
    id: "j1p-jones-enfilade", kind: "artillery_fire", unitId: "csa-bn-jones",
    t0: T(14, 50), t1: T(15, 50), confidence: "documented",
    citation: "'Completely enfilading General Barlow's line' (or-27-1-schurz) — the knoll break's second cause.",
  }),
  fireEvent({
    id: "j1p-gordon-knoll", kind: "musketry", unitId: "csa-gordon",
    t0: T(15, 0), t1: T(15, 55), confidence: "documented",
    citation: "The knoll assault — fences, the creek, the 50-paces closure (or-27-2-gordon; CA-J1P-3's executor window).",
  }),
  fireEvent({
    id: "j1p-vongilsa-knoll", kind: "musketry", unitId: "us-vongilsa",
    t0: T(15, 0), t1: T(15, 45), confidence: "documented",
    citation: "The knoll's right-face defense (the attacker attests it: 'most obstinate resistance'; or-27-2-gordon via the dossier's both-sided record).",
  }),
  fireEvent({
    id: "j1p-ames-knoll", kind: "musketry", unitId: "us-harris",
    t0: T(15, 0), t1: T(15, 50), confidence: "documented",
    citation: "The crest line's fight (drawn 17 Ct / 25 Ohio bar; or-27-1-ames; Fowler k).",
  }),
  fireEvent({
    id: "j1p-wilkeson-knoll", kind: "artillery_fire", unitId: "us-btty-bancroft",
    t0: T(14, 20), t1: T(15, 40), confidence: "documented",
    citation: "The knoll battery under the converging enfilade; Wilkeson mw (register-grade pin, carried at its honesty grade).",
  }),
  fireEvent({
    id: "j1p-doles-plain", kind: "musketry", unitId: "csa-doles",
    t0: T(14, 30), t1: T(16, 30), confidence: "documented",
    citation: "The plain fight: the skirmisher hill, the knoll conjunction, the 157th NY rout, the pursuit (or-27-2-doles).",
  }),
  fireEvent({
    id: "j1p-krzyzanowski-stand", kind: "musketry", unitId: "us-krzyzanowski",
    t0: T(14, 40), t1: T(16, 10), confidence: "documented",
    citation: "The open-field stand against the converging attack (or-27-1-jacobs-26wi; or-27-1-thomson-82oh).",
  }),
  fireEvent({
    id: "j1p-vonamsberg-line", kind: "musketry", unitId: "us-vonamsberg",
    t0: T(13, 30), t1: T(16, 20), confidence: "documented",
    citation: "The 1.30 flank fire, the skirmish line's day, the seminary cover (or-27-1-dobke-45ny).",
  }),
  // Hays/Avery vs Coster.
  fireEvent({
    id: "j1p-hays-brickyard", kind: "musketry", unitId: "csa-hays",
    t0: T(15, 45), t1: T(16, 30), confidence: "documented",
    citation: "The second effort into the brickyard line and the town (or-27-2-early; or-27-2-hays).",
  }),
  fireEvent({
    id: "j1p-avery-brickyard", kind: "musketry", unitId: "csa-godwin",
    t0: T(15, 45), t1: T(16, 30), confidence: "documented",
    citation: "With Hays against the brickyard and into the fields left of town (or-27-2-early).",
  }),
  fireEvent({
    id: "j1p-coster-brickyard", kind: "musketry", unitId: "us-coster",
    t0: T(15, 50), t1: T(16, 35), confidence: "documented",
    citation: "The brickyard delaying action and the brick-house stopper (or-27-1-vonsteinwehr; the 134th/154th NY ledger).",
  }),
  // The final push (CA-J1P-5) and the cavalry front.
  fireEvent({
    id: "j1p-perrin-assault", kind: "musketry", unitId: "csa-perrin",
    t0: T(16, 5), t1: T(17, 0), confidence: "documented",
    citation: "The seminary assault: the hold-fire approach, the fence, the two obliques, the town (or-27-2-perrin; CA-J1P-5/ED-70).",
  }),
  fireEvent({
    id: "j1p-scales-descent", kind: "musketry", unitId: "csa-lowrance",
    t0: T(16, 5), t1: T(16, 35), confidence: "documented",
    citation: "The descent into the converging fire and the halt at the bottom (or-27-2-scales).",
  }),
  fireEvent({
    id: "j1p-lane-cavalry", kind: "musketry", unitId: "csa-lane",
    t0: T(16, 5), t1: T(16, 50), confidence: "documented",
    citation: "The right's fight against the dismounted cavalry (or-27-2-lane-jh; or-27-2-perrin's did-not-move record).",
  }),
  fireEvent({
    id: "j1p-gamble-stand", kind: "musketry", unitId: "us-cav-gamble",
    t0: T(16, 0), t1: T(16, 45), confidence: "documented",
    citation: "The carbine stand at the stone wall — 'killed and wounded so many of the first line... that it fell back upon the second line' (or-27-1-gamble; drawn j1-13).",
  }),
  fireEvent({
    id: "j1p-calef-stand", kind: "artillery_fire", unitId: "us-btty-calef",
    t0: T(16, 0), t1: T(16, 45), confidence: "documented",
    citation: "The attached battery with the afternoon stand (or-27-1-gamble composition; tablet record).",
  }),
  // The Seminary Ridge guns and the covering batteries.
  fireEvent({
    id: "j1p-breck-seminary", kind: "artillery_fire", unitId: "us-btty-breck",
    t0: T(15, 45), t1: T(16, 45), confidence: "documented",
    citation: "The Seminary line's canister fight and fighting withdrawal (or-27-1-wainwright pp. 355-357).",
  }),
  fireEvent({
    id: "j1p-cooper-seminary", kind: "artillery_fire", unitId: "us-btty-cooper",
    t0: T(14, 30), t1: T(16, 40), confidence: "documented",
    citation: "The left interval then the seminary group (or-27-1-cbiddle; us-btty-cooper.md).",
  }),
  fireEvent({
    id: "j1p-stevens-barricade", kind: "artillery_fire", unitId: "us-btty-stevens",
    t0: T(16, 0), t1: T(16, 50), confidence: "documented",
    citation: "Canister at the charge from the barricade group (Wainwright's 83-casualty day frame).",
  }),
  fireEvent({
    id: "j1p-stewart-cut", kind: "artillery_fire", unitId: "us-btty-stewart",
    t0: T(15, 30), t1: T(16, 45), confidence: "documented",
    citation: "The cut position's down-the-pike canister through the collapse (Wainwright's layout; Davison w).",
  }),
  fireEvent({
    id: "j1p-wiedrich-cover", kind: "artillery_fire", unitId: "us-btty-wiedrich",
    t0: T(15, 45), t1: T(17, 0), confidence: "documented",
    citation: "The hill's guns over the retreat (or-27-1-vonsteinwehr; Devin's friendly-fire record carried unresolved — the York-road shelling names a Cemetery Hill battery, candidate only).",
  }),
];

// ---------------------------------------------------------------------------
// The battle document.
// ---------------------------------------------------------------------------
const battle: Battle = {
  name: "Gettysburg — July 1, 1863: Afternoon (the collapse and retreat to Cemetery Hill)",
  startTime: StartTime,
  endTime: EndT,
  units: movers.map((m) => moverUnit({
    id: m.id, name: m.name, side: m.side,
    frontage_m: m.frontage, depth_m: m.depth, keyframes: m.kfs,
  })),
  events,
  environment: {
    windTowardDeg: 45, windMps: 0.0, confidence: "unknown",
    note: "No sourced wind observation for the July 1 afternoon exists in the corpus (the ED-10/ED-19 class); calm authored — windMps 0 = no drift.",
  },
};

// Continuity audit (printed, not silently assumed): every carried unit's
// t=0 state must equal the morning file's final keyframe.
let carried = 0;
for (const u of battle.units) {
  const m = mById.get(u.id);
  if (!m) continue;
  carried++;
  const a = u.keyframes[0]!;
  const b = m.keyframes[m.keyframes.length - 1]!;
  for (const f of ["x", "z", "facing", "strength"] as const)
    if (a[f] !== b[f])
      throw new Error(`continuity break on ${u.id}.${f}: afternoon t=0 ${a[f]} vs morning end ${b[f]}`);
}
console.log(`continuity: ${carried} units carried from the morning file, all end states re-expressed exactly`);

writeFileSync(outPath, exportValidated(battle));
console.log(`wrote ${outPath}: ${battle.units.length} units, ${events.length} events, ` +
  `window ${StartTime}..${StartTime + EndT} (13:00-18:00 LMT)`);
