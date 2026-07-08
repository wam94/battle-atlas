// Wave 3 authoring script (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 5): the 15 Confederate artillery battalions — the bombardment
// re-anchors from one anonymous 1,800 m fixed segment to fifteen named,
// cited battalions, and `csa-seminary-bombardment` retires IN THIS SAME
// RUN (the atomic-migration rule, battle-format.md "Segment-emitter
// migration and attested detachments": at no commit may the bombardment
// have zero emitters). Survey basis:
// docs/research/2026-07-02-full-cast-sources.md §3.2 (the 15-battalion
// table, all Stone Sentinels pages VERIFIED), §4 items 1/2/13, §5.2 step 3.
// Position classes from the survey §1 landmark table (Peach Orchard
// 3208,3457; Warfield Ridge 2963,2231; Seminary 3722,6888; Oak Hill
// 4175,8209; Benner's Hill 6619,6305; town square 4864,6839), the sheet-8
// sector crops (s8_peachorchard.jpg — Alexander's/Cabell's/Dearing's named
// batteries; s8_semridge_n.jpg — McIntosh's/Pegram's/Lane's named
// batteries), and the retired segment's own corridor geometry (the gun
// line ~120 m in front of the authored infantry start line, through the
// Virginia Memorial anchor 3176.8,5016.5). Tablet/monument text fetched
// live 2026-07-08 (exact-URL citation convention, Wave 2 forward). Run
// from tool/:
//   npx vite-node scripts/author-w3-csa-artillery.ts
// Committed as the derivation record for the authored data.
//
// STRENGTHS: no Stone Sentinels battalion page carries a personnel figure
// (verified across all 15 this session) and the OOB strengths doc covers
// the assault sector only — every battalion strength below is
// RECONSTRUCTED at ~24 men/gun from the monument's own armament list
// (crews, drivers, battalion staff; the Wave-2 Dilger precedent), flagged
// in each citation. Gun counts are the monuments'; the survey's table
// agrees everywhere.
//
// THE DOCUMENTED SILENCES ARE THE POINT (battle-format.md "Documented
// silence"; survey §3.2 ⚠️ and §4 item 13): the popular picture — "two
// dozen enfilading guns from the northeast" — is contradicted by the
// tablets, which have only ~10–14 of Ewell's ~79 Second Corps guns
// actually firing (Griffin's of Dance's, Carter's detached rifles, Raine's
// two 20-pdr Parrotts, Nelson's 20–25 rounds). So: Jones's battalion and
// Garnett's battalion get ZERO events — their negatives ride the t=0
// keyframe citations; Nelson gets one short LIMITED window; Carter's Oak
// Hill Napoleons stand silent while his detached rifles fire from the
// railroad cut. Benner's Hill and the northern arc render guns present
// and mostly dark — presence without events IS the encoding.
//
// ATTESTED DETACHMENTS AS FIXED SEGMENTS (the locked ruling, format doc
// "Segment-emitter migration"): Carter's battalion stays ONE unit at its
// Oak Hill tablet position; his "Parrotts and Rifled guns … placed on
// Seminary Ridge near the railroad cut" fire as a fixed-segment event at
// the detachment's attested position. Same pattern for Raine's 20-pdr
// section ~1/2 mile north of Benner's Hill. The segment emitter form
// survives for exactly these two.
//
// GARNETT ADJUDICATION (survey §3.2 flag: "weight the tablet"): the full
// tablet reads "The Parrotts and Rifles took part in the battle in
// different position on each of the three days their most active service
// being on the second day in this position. The Napoleons and Howitzers
// were in reserve and not actively engaged at any time." — the July 3
// credit is nonspecific (most active service July 2) and half the
// battalion is explicitly idle; secondaries credit his rifles in the
// cannonade. Ruling per the survey: NO events; the fired-or-not
// disagreement stays in his citation, never reconciled.
//
// WINDOW SPINE: the retired segment's window is kept exactly — t0=420
// (Jacobs 1864: "At seven minutes past 1 p.m." — 13:07) to t1=7500 (the
// authored track spine's step-off keyframes) — so obscuration coverage of
// the cannonade never regresses; the onset/end witness spread the segment
// carried moves into the shared window note below. Henry's flank-cover
// window ends at t=7200 (~15:00) where Wave 6's Bachman/Reilly southward
// swing begins — the two-family-levels guard, decided here so the waves
// meet without double fire.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Unit } from "../src/model";
import {
  addUnitsAfter, exportValidated, fireEvent, staticUnit,
} from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const survey = "docs/research/2026-07-02-full-cast-sources.md §3.2";
const fetched = "fetched 2026-07-08";
const ss = "https://gettysburg.stonesentinels.com/confederate-headquarters/";

// The strength-reconstruction flag every battalion carries (header note).
const strengthNote = (guns: number) =>
  `NO personnel figure on the battalion page — strength reconstructed at ~24 men/gun from the monument's ${guns}-gun armament list (crews, drivers, battalion staff), flagged (the Wave-2 Dilger precedent)`;

// The Ewell's-wing disagreement, encoded as data on the northern arc
// (survey §3.2 ⚠️): rides every Second Corps battalion's t=0 keyframe.
const ewellDisagreement =
  "EWELL'S-WING DISAGREEMENT CARRIED (survey §3.2 ⚠️): the popular picture has 'two dozen enfilading guns from the northeast'; the tablets attest only ~10–14 of the Second Corps' ~79 guns actually firing (Griffin's of Dance's, Carter's detached rifles, Raine's two 20-pdrs, Nelson's 20–25 rounds) — the near-silence of Ewell's arc is a documented feature, rendered as guns present and mostly dark, never paved over";

// The window spine the firing battalions share — the retired segment's own
// provenance, kept verbatim so the migration loses nothing.
const windowNote =
  "window = the retired csa-seminary-bombardment segment's spine, kept exactly: t0=420 per Jacobs 1864 (Notes on the Rebel Invasion), 'At seven minutes past 1 p.m., the awful and portentous silence was broken' — 13:07, contemporaneous and habitually precise; onset disagreement carried, never reconciled (Alexander 1907 'just 1 P.M. by my watch'; Haskell watch-checked ~13:00; Sawyer (8th OH) 'between twelve and one' — a 0–7 minute spread); t1=7500 follows the authored track spine (step-off keyframes at t=7500); witnesses put the general fire's end anywhere 13:50–15:00 (Alexander ~13:50, Jacobs 14:30, Haskell 'two mortal hours… at three o'clock almost precisely') — geometry follows the spine, the citation carries the spread";

// ---- First Corps battalions — the Peach Orchard–Warfield arc, the
// ---- cannonade's core (sheet 8 s8_peachorchard.jpg names the batteries) ----

const firstCorps: Unit[] = [
  staticUnit({
    id: "csa-bn-alexander",
    name: "Alexander's Artillery Battalion (Maj. Huger)",
    side: "confederate", strength: 576, x: 3170, z: 4150, facing: 78,
    frontage_m: 500, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'In the line on ridge from Peach Orchard to N. E. corner of Spangler's Woods. Aided in the cannonade and supported Longstreet's assault.' (" + ss + "alexanders-battalion/, " + fetched + ") — the cannonade's core; Alexander's 'come quick' notes to Pickett (" + survey + "); Col. E.P. Alexander directing the First Corps guns, Maj. Frank Huger the battalion (survey Cdr column: Alexander/Huger); " +
      "24 pieces (two 20-pdr Parrotts, one 10-pdr Parrott, seven 3-inch Rifles, six Napoleons, four 24-pdr and four 12-pdr howitzers); casualties killed 19, wounded 114, missing 6, 116 horses (monument); " +
      "the tablet's line runs ~1.5 km Peach Orchard → Spangler's Woods — unit centered mid-sector at the sheet-8 read, frontage capped at the 500 m battalion-gun-line convention, the sector is the claim; " + strengthNote(24),
  }),
  staticUnit({
    id: "csa-bn-cabell",
    name: "Cabell's Artillery Battalion",
    side: "confederate", strength: 384, x: 3190, z: 3400, facing: 70,
    frontage_m: 350, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'July 2-3. Took an active part in the battle.' (" + ss + "cabells-artillery-battalion/, " + fetched + "); Col. Henry C. Cabell; Fraser's, McCarthy's, Carlton's, Manly's batteries; " +
      "position: Emmitsburg Rd at the Peach Orchard/Wheatfield Rd corner (" + survey + "; survey §1 Peach Orchard 3208,3457; sheet 8 s8_peachorchard.jpg draws 'Cabell's Bat.'); " +
      "16 guns (four Napoleons, four 10-pdr Parrotts, six 3-inch Rifles, two 12-pdr howitzers); casualties killed 12, wounded 30, missing 4, 80 horses (monument); " + strengthNote(16),
  }),
  staticUnit({
    id: "csa-bn-dearing",
    name: "Dearing's Artillery Battalion",
    side: "confederate", strength: 432, x: 3140, z: 4600, facing: 85,
    frontage_m: 400, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'Advanced to the front about daybreak and took a conspicuous part in the battle. In the cannonade preceding Longstreet's assault it fired by battery and very effectively.' (" + ss + "dearings-battalion/, " + fetched + "); Maj. James Dearing; Stribling's, Caskie's, Macon's, Blount's batteries — sheet 8 s8_peachorchard.jpg names Blount, Caskie, Macon on the Emmitsburg Rd ridge fronting Pickett (" + survey + "); " +
      "18 pieces (two 20-pdr Parrotts, three 10-pdr Parrotts, one 3-inch Rifle, twelve Napoleons); casualties killed 8, wounded 17, 37 horses (monument); " + strengthNote(18),
  }),
  staticUnit({
    id: "csa-bn-eshleman",
    name: "Eshleman's Battalion, Washington Artillery",
    side: "confederate", strength: 240, x: 3220, z: 3550, facing: 55,
    frontage_m: 250, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'Arrived on the field before daylight and was engaged all day. Captured one 3 inch rifle.' (" + ss + "eshlemans-artillery-battalion/, " + fetched + "); Maj. Benjamin F. Eshleman; Miller's, Squires's, Richardson's, Norcom's batteries (the Washington Louisiana Artillery); " +
      "THE 13:07 SIGNAL GUNS ARE HIS — Miller's battery tablet: 'advanced before daylight into position about 100 yards north of the Peach Orchard… fired the signal guns for the cannonade preceding Longstreet's assault, took part therein, and supported the charge of the infantry by advancing 450 yards and keeping up a vigorous fire' (https://gettysburg.stonesentinels.com/confederate-batteries/millers-louisiana-battery/, " + fetched + "; " + survey + " and §4 item 1); " +
      "10 guns (eight Napoleons, two 12-pdr howitzers); casualties killed 3, wounded 26, missing 16, 37 horses, 3 guns disabled (monument); " + strengthNote(10),
  }),
  staticUnit({
    id: "csa-bn-henry",
    name: "Henry's Artillery Battalion",
    side: "confederate", strength: 456, x: 2980, z: 2280, facing: 150,
    frontage_m: 450, depth_m: 40, grade: "A-",
    citation:
      "War Dept tablet verbatim: 'July 2-3. Occupied this line and took active part in the battle as described on the tablets of the several batteries.' (" + ss + "henrys-artillery-battalion/, " + fetched + "); Maj. Mathis W. Henry; Reilly's, Bachman's, Garden's, Latham's batteries; " +
      "position: Warfield Ridge/Bushman knolls facing S/SE (survey §1 Warfield Ridge 2963,2231; " + survey + ") — ⚠️ PARTIAL/CONTESTED cannonade weight: flank cover per Alexander rather than full participation in the bombardment; Bachman's & Reilly's southward swing vs Merritt ~15:00+ belongs to Wave 6 as his children (plan Task 8) — flag carried; " +
      "19 pieces (eleven Napoleons, four 10-pdr Parrotts, two 3-inch Rifles, one 12-pdr howitzer, one 6-pdr bronze gun); casualties killed 4, wounded 23 (monument); " + strengthNote(19),
  }),
];

// ---- Third Corps battalions — the Seminary Ridge line, Fairfield Rd to
// ---- Anderson's front (sheet 8 s8_semridge_n.jpg names the batteries) ------

const thirdCorps: Unit[] = [
  staticUnit({
    id: "csa-bn-pegram",
    name: "Pegram's Artillery Battalion",
    side: "confederate", strength: 480, x: 3580, z: 6400, facing: 105,
    frontage_m: 450, depth_m: 40, grade: "A",
    citation:
      "monument verbatim: 'July 1 – 3. The Battalion was actively engaged on each of the three days'; ammunition expended 3,800 rounds (" + ss + "pegrams-artillery-battalion/, " + fetched + "); Maj. William J. Pegram; Marye's, Crenshaw's, Zimmerman's, McGraw's, Brander's batteries — sheet 8 s8_semridge_n.jpg names Marye, Zimmerman, Brander on Seminary Ridge S of Fairfield Rd (" + survey + "); " +
      "20 guns (ten Napoleons, four 10-pdr Parrotts, four 3-inch Rifles, two 12-pdr howitzers); casualties killed 10, wounded 37, 38 horses (monument); " + strengthNote(20),
  }),
  staticUnit({
    id: "csa-bn-mcintosh",
    name: "McIntosh's Artillery Battalion",
    side: "confederate", strength: 384, x: 3530, z: 6180, facing: 105,
    frontage_m: 400, depth_m: 40, grade: "A",
    citation:
      "monument verbatim: 'July 1 – 4. The Battalion was actively engaged on each of the three days of the battle…' (" + ss + "mcintoshs-artillery-battalion/, " + fetched + "); Maj. David Gregg McIntosh; Johnson's, Rice's, Hurt's, Wallace's batteries — sheet 8 s8_semridge_n.jpg names Johnson, Hurt, Wallace in the Schultz Woods sector (" + survey + "); " +
      "16 guns (six Napoleons, two Whitworths, eight 3-inch Rifles); casualties 7 killed, 25 wounded of whom 16 captured, 38 horses (monument); " + strengthNote(16),
  }),
  staticUnit({
    id: "csa-bn-garnett",
    name: "Garnett's Artillery Battalion",
    side: "confederate", strength: 360, x: 3350, z: 5880, facing: 95,
    frontage_m: 350, depth_m: 40, grade: "B",
    citation:
      "DOCUMENTED SILENCE, DISAGREEMENT CARRIED — War Dept tablet verbatim: 'The Parrotts and Rifles took part in the battle in different position on each of the three days their most active service being on the second day in this position. The Napoleons and Howitzers were in reserve and not actively engaged at any time.' (" + ss + "garnetts-artillery-battalion/, " + fetched + ") — the July 3 credit is nonspecific (most active service July 2) and half the battalion is explicitly idle; secondaries credit his rifles in the July 3 cannonade — ⚠️ fired-or-not disagreement kept here, WEIGHT THE TABLET per the survey (" + survey + ", §4 item 13): NO events, presence without fire IS the encoding; " +
      "Lt. Col. John J. Garnett; Grandy's, Moore's, Lewis's, Maurin's batteries; position near McMillan Woods (monument ~160 ft N of the McMillan House); " +
      "15 guns (four Napoleons, two 10-pdr Parrotts, seven 3-inch Rifles, two 12-pdr howitzers); casualties wounded 5, missing 17, 13 horses (monument); " + strengthNote(15),
  }),
  staticUnit({
    id: "csa-bn-poague",
    name: "Poague's Artillery Battalion",
    side: "confederate", strength: 384, x: 3290, z: 5300, facing: 92,
    frontage_m: 350, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim, July 3: 'The ten guns were actively engaged.' — and the howitzer position marker: the six howitzers, sheltered, 'took no active part in the battle' — a DOCUMENTED INTERNAL SPLIT, both halves on one tablet pair (" + ss + "poagues-artillery-battalion/, " + fetched + "; " + survey + "); Lt. Col. William T. Poague; Ward's, Brooke's, Wyatt's, Graham's batteries; 657 rounds expended; " +
      "position: behind Pettigrew, Spangler/McMillan Woods sector (" + survey + "); " +
      "16 pieces (seven Napoleons, six 12-pdr howitzers, one 10-pdr Parrott, two 3-inch Rifles) — ten engaged; casualties killed 2, wounded 24, missing 6, 17 horses (monument); " + strengthNote(16),
  }),
  staticUnit({
    id: "csa-bn-lane",
    name: "Lane's Sumter (Georgia) Artillery Battalion",
    side: "confederate", strength: 408, x: 3160, z: 4870, facing: 88,
    frontage_m: 400, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'July 2 – 3. Took part in the battle.' (" + ss + "lanes-artillery-battalion/, " + fetched + "); 1,082 rounds expended July 2–3 (" + survey + "); Maj. John Lane; Patterson's, Wingfield's, Ross's batteries — sheet 8 s8_semridge_n.jpg names Ross, Wingfield on Anderson's front, Seminary Ridge (" + survey + "); " +
      "17 pieces (three Napoleons, two 20-pdr Parrotts, three 10-pdr Parrotts, four 3-inch Navy Parrotts, five 12-pdr howitzers); casualties killed 3, wounded 21, missing 6 (monument); " + strengthNote(17),
  }),
];

// ---- Second Corps battalions — the northern arc, mostly dark (the survey's
// ---- Ewell's-wing disagreement rendered as presence-without-events) --------

const secondCorps: Unit[] = [
  staticUnit({
    id: "csa-bn-dance",
    name: "Dance's 1st Virginia Artillery Battalion",
    side: "confederate", strength: 480, x: 3700, z: 6850, facing: 115,
    frontage_m: 450, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'July 2 & 3. The four first named batteries occupied positions at various points on this ridge. Graham's Battery of 20 Pounder Parrotts served east of Rock Creek. All were actively engaged.' (" + ss + "dances-artillery-battalion/, " + fetched + "); Griffin's battery 'took part in the cannonade' (" + survey + "); Capt. Willis A. Dance; Cunningham's, Smith's, Watson's, Griffin's, Graham's batteries; " +
      "position: Seminary Ridge near the Seminary/railroad cut (survey §1 Seminary 3722,6888) — the tablet disperses the four ridge batteries 'at various points'; unit centered at the position class, the sector is the claim; Graham's east-of-Rock-Creek service is a known coarseness kept at battalion grain (the survey attests no separate in-window position for it — noted, not authored); " +
      "20 guns (four 20-pdr Parrotts, four 10-pdr Parrotts, ten 3-inch Rifles, two Napoleons); casualties killed 3, wounded 1 (monument); " + strengthNote(20) + "; " + ewellDisagreement,
  }),
  staticUnit({
    id: "csa-bn-nelson",
    name: "Nelson's Artillery Battalion",
    side: "confederate", strength: 264, x: 6560, z: 6420, facing: 240,
    frontage_m: 280, depth_m: 40, grade: "A-",
    citation:
      "War Dept tablet verbatim: 'Ordered to the extreme left of the Confederate line to find a position to withdraw the fire from the Confederate infantry. Opened about 12 M. firing 20 to 25 rounds.' — then silent (" + ss + "nelsons-artillery-battalion/, " + fetched + "; " + survey + ": LIMITED, A−); Lt. Col. William Nelson; Kirkpatrick's, Massie's, Milledge's batteries; " +
      "position class: Benner's Hill area (survey §1 Benner's Hill 6619,6305); " +
      "11 guns (one 10-pdr Parrott, four 3-inch Rifles, six Napoleons); 'Casualties not reported.' (monument); " + strengthNote(11) + "; " + ewellDisagreement,
  }),
  staticUnit({
    id: "csa-bn-carter",
    name: "Carter's Artillery Battalion",
    side: "confederate", strength: 384, x: 4185, z: 8180, facing: 175,
    frontage_m: 350, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim, July 3: 'The Parrotts and Rifled guns were placed on Seminary Ridge near the railroad cut and took part in the great cannonade preceding Longstreet's assault.' (" + ss + "carters-artillery-battalion/, " + fetched + ") — a DOCUMENTED SPLIT (" + survey + ": YES rifles / Oak Hill Napoleons largely silent): the battalion stands at its Oak Hill tablet position with the Napoleons SILENT, and the detached rifles' fire is a fixed-segment event at the railroad-cut position (the format doc's surviving segment use); Lt. Col. Thomas H. Carter; Carter's, Fry's, Page's, Reese's batteries; " +
      "position: Oak Hill (survey §1: 4175,8209); 16 guns (four 10-pdr Parrotts, six 3-inch Rifles, six Napoleons); casualties 6 killed, 35 wounded, 24 missing; 1,898 rounds expended across the battle (monument); " + strengthNote(16) + "; " + ewellDisagreement,
  }),
  staticUnit({
    id: "csa-bn-jones",
    name: "Jones's Artillery Battalion",
    side: "confederate", strength: 384, x: 4950, z: 7480, facing: 195,
    frontage_m: 350, depth_m: 40, grade: "B",
    citation:
      "DOCUMENTED SILENCE — War Dept tablet verbatim, July 3: 'Occupied same position. Not actively engaged.' — sixteen guns north of the town, present and dark all window; NO events, the absence IS the encoding (battle-format.md 'Documented silence') (" + ss + "jones-artillery-battalion/, " + fetched + "; " + survey + "); Lt. Col. Hilary P. Jones; Carrington's, Tanner's, Green's, Garber's batteries; " +
      "position class: N of town, where the battalion had stood since its July 1 enfilade (survey §1 town square 4864,6839); " +
      "16 guns (two 10-pdr Parrotts, six 3-inch guns, eight Napoleons); " + strengthNote(16) + "; " + ewellDisagreement,
  }),
  staticUnit({
    id: "csa-bn-raine",
    name: "Andrews's (Latimer's) Artillery Battalion (Capt. Raine)",
    side: "confederate", strength: 384, x: 6650, z: 6150, facing: 240,
    formation: "column", frontage_m: 250, depth_m: 40, grade: "A-",
    citation:
      "War Dept tablet verbatim, July 3: 'The 20 pounder Parrotts took part in the great cannonade while the other batteries were in reserve.' (" + ss + "latimers-artillery-battalion/, " + fetched + ") — a DOCUMENTED SPLIT (" + survey + ": PARTIAL, A−): the battalion parks in reserve behind Benner's Hill (formation column) and the detached 20-pdr section's fire is a fixed-segment event ~1/2 mile north of Benner's Hill, the section's attested position (the format doc's surviving segment use); Maj. Joseph W. Latimer mortally wounded July 2 (died Aug 1) — Capt. Charles I. Raine commanding (acting-commander naming per the survey); Brown's, Carpenter's, Dement's, Raine's batteries; " +
      "16 guns (two 20-pdr Parrotts, five 10-pdr Parrotts, three 3-inch Rifles, six Napoleons); casualties 10 killed, 40 wounded, 30 horses (monument); " + strengthNote(16) + "; " + ewellDisagreement,
  }),
];

// ---- events: the signal guns, the ten YES cannonade windows, the two
// ---- attested-detachment segments, Nelson's limited window, Henry's
// ---- flagged flank cover — and NOTHING for Garnett or Jones ----------------

// The ten YES battalions' unitId cannonade windows (tablet citations above
// carry the fire attestation; the event citation quotes the firing line).
const cannonadeYes: ReadonlyArray<readonly [string, string]> = [
  ["csa-bn-alexander",
    "tablet: 'Aided in the cannonade and supported Longstreet's assault.' (" + ss + "alexanders-battalion/, " + fetched + ")"],
  ["csa-bn-cabell",
    "tablet: 'July 2-3. Took an active part in the battle.' (" + ss + "cabells-artillery-battalion/, " + fetched + ")"],
  ["csa-bn-dearing",
    "tablet: 'In the cannonade preceding Longstreet's assault it fired by battery and very effectively.' (" + ss + "dearings-battalion/, " + fetched + ")"],
  ["csa-bn-eshleman",
    "tablet: 'engaged all day'; Miller's battery tablet: 'fired the signal guns for the cannonade preceding Longstreet's assault, took part therein, and supported the charge of the infantry' (" + ss + "eshlemans-artillery-battalion/ and https://gettysburg.stonesentinels.com/confederate-batteries/millers-louisiana-battery/, " + fetched + ")"],
  ["csa-bn-pegram",
    "monument: 'The Battalion was actively engaged on each of the three days'; 3,800 rounds expended (" + ss + "pegrams-artillery-battalion/, " + fetched + ")"],
  ["csa-bn-mcintosh",
    "monument: 'The Battalion was actively engaged on each of the three days of the battle' (" + ss + "mcintoshs-artillery-battalion/, " + fetched + ")"],
  ["csa-bn-poague",
    "tablet, July 3: 'The ten guns were actively engaged.' — the six howitzers, sheltered, 'took no active part' (documented internal split; the window is the ten guns') (" + ss + "poagues-artillery-battalion/, " + fetched + ")"],
  ["csa-bn-lane",
    "tablet: 'July 2 – 3. Took part in the battle.'; 1,082 rounds July 2–3 (" + ss + "lanes-artillery-battalion/, " + fetched + "; " + survey + ")"],
  ["csa-bn-dance",
    "tablet: 'All were actively engaged.'; Griffin's battery 'took part in the cannonade' (" + ss + "dances-artillery-battalion/, " + fetched + "; " + survey + ")"],
] as const;

const events: EngagementEvent[] = [
  // -- 13:07: the two signal guns, Miller's battery, Eshleman's battalion -----
  fireEvent({
    id: "csa-bn-eshleman-signal", kind: "artillery_fire",
    t0: 420, t1: 480, unitId: "csa-bn-eshleman", confidence: "documented",
    citation:
      "Miller's battery tablet verbatim: 'fired the signal guns for the cannonade preceding Longstreet's assault' — Miller's 3rd Company, Washington Artillery, ~100 yards north of the Peach Orchard (https://gettysburg.stonesentinels.com/confederate-batteries/millers-louisiana-battery/, " + fetched + "); clock: Jacobs 1864, 'At seven minutes past 1 p.m., the awful and portentous silence was broken' — 13:07 = t 420 (" + survey + ", §4 item 1)",
    note: "the two signal shots that opened the cannonade; the battalion's own cannonade window follows at t=480",
  }),
  // -- the ten YES battalions' cannonade windows (the segment's replacement) --
  ...cannonadeYes.map(([id, cite]) =>
    fireEvent({
      id: `${id}-cannonade`, kind: "artillery_fire",
      t0: id === "csa-bn-eshleman" ? 480 : 420, t1: 7500, unitId: id,
      confidence: "documented", citation: cite,
      note: windowNote,
    })),
  // -- Carter's detached rifles: the attested-detachment segment at the
  // -- railroad cut (his tenth YES verdict — the unit's Napoleons stay dark) --
  fireEvent({
    id: "csa-bn-carter-rifles-cannonade", kind: "artillery_fire",
    t0: 420, t1: 7500,
    segment: { x: 3660, z: 7030, x2: 3705, z2: 7170 },
    confidence: "documented",
    citation:
      "War Dept tablet verbatim, July 3: 'The Parrotts and Rifled guns were placed on Seminary Ridge near the railroad cut and took part in the great cannonade preceding Longstreet's assault.' (" + ss + "carters-artillery-battalion/, " + fetched + "; " + survey + ")",
    note:
      "ATTESTED DETACHMENT AS FIXED SEGMENT (battle-format.md 'Segment-emitter migration and attested detachments'): the parent battalion csa-bn-carter stays one unit at its Oak Hill tablet position (Napoleons largely silent — the negative rides its keyframes); the ten rifled pieces fire from the railroad-cut position — segment traced on Seminary Ridge just N of the Seminary (survey §1: 3722,6888), coordinate inferred within the tablet's position class; " + windowNote,
  }),
  // -- Raine's 20-pdr section: the second surviving segment use ---------------
  fireEvent({
    id: "csa-bn-raine-20pdr-cannonade", kind: "artillery_fire",
    t0: 420, t1: 7500,
    segment: { x: 6600, z: 7070, x2: 6640, z2: 7130 },
    confidence: "documented",
    citation:
      "War Dept tablet verbatim, July 3: 'The 20 pounder Parrotts took part in the great cannonade while the other batteries were in reserve.' — the section positioned ~1/2 mile north of Benner's Hill (" + ss + "latimers-artillery-battalion/, " + fetched + "; " + survey + ": PARTIAL, A−)",
    note:
      "ATTESTED DETACHMENT AS FIXED SEGMENT: the parent battalion csa-bn-raine parks in reserve behind Benner's Hill (the silence of its other batteries rides its keyframes); the two 20-pdr Parrotts fire from ~800 m N of Benner's Hill (survey §1: 6619,6305), coordinate inferred within the tablet's position class; " + windowNote,
  }),
  // -- Nelson: the LIMITED window — 20 to 25 rounds, then silent --------------
  fireEvent({
    id: "csa-bn-nelson-limited", kind: "artillery_fire",
    t0: 420, t1: 1500, unitId: "csa-bn-nelson", confidence: "documented",
    citation:
      "War Dept tablet verbatim: 'Opened about 12 M. firing 20 to 25 rounds.' — then silent for the rest of the battle (" + ss + "nelsons-artillery-battalion/, " + fetched + "; " + survey + ": LIMITED, A−)",
    note:
      "⚠️ the tablet's clock ('about 12 M.') sits BEFORE the slice; the survey and plan read the rounds as part of the Second Corps' thin share of the cannonade (§3.2 ⚠️: 'Nelson's 20–25 rounds' counted among Ewell's ~10–14 firing guns) — window placed at the cannonade onset, short (20–25 rounds ≈ minutes of fire, not hours); witness clock kept in the citation, disagreement carried, not reconciled",
  }),
  // -- Henry: the flagged flank-cover window (ends where Wave 6 begins) -------
  fireEvent({
    id: "csa-bn-henry-flank-cover", kind: "artillery_fire",
    t0: 420, t1: 7200, unitId: "csa-bn-henry", confidence: "documented",
    citation:
      "tablet: 'July 2-3. Occupied this line and took active part in the battle as described on the tablets of the several batteries.' (" + ss + "henrys-artillery-battalion/, " + fetched + ") — ⚠️ A− PARTIAL/CONTESTED (" + survey + "): flank cover facing S/SE per Alexander, not full weight in the bombardment corridor",
    note:
      "window ends at t=7200 (~15:00) BY DESIGN: Bachman's & Reilly's attested southward swing vs Merritt (~15:00+) belongs to Wave 6 as csa-bn-henry's children — the battalion-level window must not overlap the children's (battle-format.md: never the same fire at two family levels); Wave 6 attaches the post-7200 fire to the batteries",
  }),
];

// ---- assemble: add the 15, RETIRE THE SEGMENT, gate, export -----------------
// The battalions land grouped with the CSA sector (after csa-lang, the last
// Confederate unit). The segment deletion and the replacement events are one
// atomic write — the bombardment never has zero emitters.

const wave3: Unit[] = [...firstCorps, ...thirdCorps, ...secondCorps];
battle = addUnitsAfter(battle, "csa-lang", wave3);

const before = battle.events?.length ?? 0;
battle.events = (battle.events ?? []).filter((e) => e.id !== "csa-seminary-bombardment");
if ((battle.events.length ?? 0) !== before - 1)
  throw new Error("csa-seminary-bombardment not found — the migration must retire it in this run");
battle.events = [...battle.events, ...events]; // exportValidated sorts canonically

// The never-regress gate, checked structurally before writing: the cannonade
// window the segment covered (t 420–7500 on the CSA side) must be covered by
// the replacement emitters.
const csaFire = battle.events.filter(
  (e) => e.kind === "artillery_fire" &&
    ((e.unitId?.startsWith("csa-bn-") ?? false) || e.id.startsWith("csa-bn-")));
const fullWindow = csaFire.filter((e) => e.t0 <= 480 && e.t1 >= 7200);
if (fullWindow.length < 10)
  throw new Error(`smoke regression: only ${fullWindow.length} CSA emitters cover the cannonade window (need >= 10)`);

writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events?.length ?? 0} events (${csaFire.length} CSA artillery_fire)`);
