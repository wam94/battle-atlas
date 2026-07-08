// Wave 2 authoring script (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 4): the Union gun ring's north and south anchors — Osborn's Cemetery
// Hill concentration, Wainwright's East Cemetery Hill guns, Rittenhouse on
// Little Round Top, the XII Corps guns, and the Artillery Reserve park —
// 20 units, 22 artillery_fire events. Survey basis:
// docs/research/2026-07-02-full-cast-sources.md §3.3 (Osborn / Wainwright /
// Rittenhouse / XII Corps / Artillery Reserve bullets), §4 items 3/4/9/13,
// §5.2 step 2. Position classes from the survey §1 landmark table (Cemetery
// Hill 4993,5656; East Cemetery Hill 5107,5753; Little Round Top 4296,2441;
// Powers Hill 5982,4077; McAllister's Mill 6683,3671) and the sheet-8 crops
// (s8_cemhill.jpg — batteries drawn as massed gun symbols, labels sparse, so
// the tablets carry the names here). Tablet/monument text fetched live
// 2026-07-08. Everything funnels through the Task 1 generator lib and out
// through exportValidated (the structural validation gate). Run from tool/:
//   npx vite-node scripts/author-w2-gun-ring.ts
// Committed as the derivation record for the authored data.
//
// CITATION-URL CONVENTION (this wave forward, per the Wave 1 provenance
// audit): where a tablet/monument page was fetched live, the citation embeds
// the EXACT resolvable URL (the Wheeler 13nybattery.com style), not just a
// descriptive page name — every citation independently re-verifiable.
//
// ADJUDICATION — the plan's 21st unit is NOT authored. The plan's Osborn
// group counted "Taft's 1st CT Heavy" (Batteries B & M, Brooker and Pratt,
// 'Not engaged' per the 2nd Volunteer Brigade tablet) as an authorable
// present-but-silent unit. Verified this session: the two batteries WERE NOT
// AT GETTYSBURG — "Detatched Companies B & M were not at Gettysburg, being
// held in reserve due to the mobile nature of the campaign" (left with the
// reserve at Westminster; Hunt later regretted their absence)
// (https://civilwarintheeast.com/us-regiments-batteries/connecticut/1st-connecticut-heavy-artillery/,
// fetched 2026-07-08). The tablet's "Not engaged" is a not-on-the-field
// negative, not a present-but-silent one. Ruling follows the survey's
// geometry discipline ("nothing placed for a fight the geometry puts off the
// map"): NOT authored; the ruling rides Taft's t=0 citation (his brigade's
// tablet carries the two "Not engaged" lines). Wave lands at 20 units,
// pin 78 → 98 (the plan's 99 assumed the CT unit; Task-8 precedent: pin the
// number the hand-edit lands on).
//
// THE RUSE GAP IS THE DATUM (plan Task 4; survey §4 item 3): Osborn's group
// fires two windows — counter-battery 600–3000, then the hill goes
// DELIBERATELY quiet at ~13:50 (Osborn proposed the cease-fire to Hunt and
// Howard to feign suppressed guns and invite the assault), then the
// Pettigrew-flank fire 7800–9000 (>1,600 rounds during the advance). No
// Osborn event spans t 3000–7800; the gap is documented on every
// counter-battery event's note. Wainwright's I Corps guns are NOT under
// Osborn's ruse — their replies run to the army-wide conserve-ammunition
// cease at t=5400 (14:30), matching the modeled II Corps reply windows.
//
// DOCUMENTED SILENCES (battle-format.md "Documented silence"): the XII Corps'
// 26 guns fired until 10:30 A.M. only — five units, ZERO events, the
// Muhlenberg tablet's negative on every t=0 keyframe; Hall's 2nd ME sent to
// the rear, "no further part in the battle" — authored at its rear position,
// no events.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Unit } from "../src/model";
import {
  addUnitsAfter, exportValidated, fireEvent, moverUnit, staticUnit, WindowEndT,
} from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const survey = "docs/research/2026-07-02-full-cast-sources.md §3.3";
const fetched = "fetched 2026-07-08";

// The group-level attestations the per-battery windows ride on.
const osbornCollective =
  "collective attestation, the Webb-siblings pattern: Osborn's ~39-gun Cemetery Hill concentration poured its fire into the flank of Pettigrew's division during the assault — 'More than 1,600 rounds were fired at Pettigrew's men during the assault.' (https://en.wikipedia.org/wiki/Pickett%27s_Charge, " + fetched + "; " + survey + "; padresteve.com Hunt essay) — per-battery window inferred on the group's attested fire (per-battery arcs/rounds attested collectively, not individually — survey flag)";
const ruseGapNote =
  "THE GAP IS THE DATUM (survey §4 item 3): this window ends at t=3000 (~13:50) because the hill's guns went DELIBERATELY silent mid-cannonade — Osborn proposed the cease-fire ruse, Hunt and Howard concurred, and Osborn rode battery to battery ordering it, to feign suppressed guns and invite the assault (padresteve.com Hunt essay; 'to fool Alexander, Hunt ordered his cannons to cease fire slowly to create the illusion they were being destroyed one by one' — https://en.wikipedia.org/wiki/Pickett%27s_Charge, " + fetched + "). No Osborn-group event spans t 3000–7800; the silence between the windows is the documented datum, not missing data";
const wainwrightCollective =
  "hill-wide firing in-window attested collectively, per-battery flagged (" + survey + ", Wainwright bullet) — window on the modeled cannonade-reply pattern (t 600–5400, the army-wide conserve-ammunition cease at ~14:30), inferred; Wainwright's I Corps guns are not under Osborn's documented ruse gap";
const muhlenbergSilence =
  "DOCUMENTED SILENCE — War Dept tablet, Artillery Brigade Twelfth Corps (Lieut. Edward D. Muhlenberg): 'At daylight the artillery (26) guns opened on the position occupied by Major Gen. Johnson's Division and fired for about 15 minutes then ceased to allow the infantry to advance. Began firing again at 5.30 and continued at intervals until 10.30 A. M. when the Confederates were forced from their position along the entire line.' — the corps' 26 guns fired in the MORNING only; in 13:00–16:00 they stand loaded and silent, commanding the valley of Rock Creek — presence without events IS the encoding (https://gettysburg.stonesentinels.com/union-headquarters/artillery-brigade-12th-corps/, " + fetched + ")";
const huntingtonTablet =
  "https://gettysburg.stonesentinels.com/union-headquarters/3rd-volunteer-brigade-artillery-reserve/";

// ---- Osborn's Cemetery Hill concentration (9 authored; the plan's 10th is
// ---- the off-map CT Heavy pair — see the header adjudication) --------------
// Coordinates: position classes inside the Evergreen/National Cemetery ground
// west of the Baltimore Pike (survey §1: Cemetery Hill 4993,5656) and the
// East Cemetery Hill lunettes (5107,5753); sheet 8 draws the hill's batteries
// as massed gun symbols with sparse labels — the tablets carry the names.

const osbornGroup: Unit[] = [
  staticUnit({
    id: "us-btty-wiedrich",
    name: "Wiedrich's Battery I, 1st New York Light Artillery",
    side: "union", strength: 141, x: 5115, z: 5795, facing: 20,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "monument front: 'Battery I, 1st Regiment N. Y. Light Artillery / Capt. M. Wiedrich commanding / 2nd Division 11th Corps / July 1st, 2nd. and 3rd, 1863' — in the East Cemetery Hill lunettes stormed the evening of July 2; the page narrative has the battery in the July 3 bombardment (prose, not tablet) (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/1st-new-york-battery-i/, " + fetched + "); " +
      "141 men, six 3-inch Ordnance Rifles; casualties 3 killed, 10 wounded (monument); " + survey,
  }),
  staticUnit({
    id: "us-btty-dilger",
    name: "Dilger's Battery I, 1st Ohio Light Artillery",
    side: "union", strength: 127, x: 5035, z: 5705, facing: 270,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "National Cemetery marker: 'took position in the Cemetery next the Baltimore Pike facing westerly. Remained there until the close of the battle.' (https://gettysburg.stonesentinels.com/union-monuments/ohio/battery-i-1st-ohio-artillery/, " + fetched + "); " +
      "six 12-pounders (marker); NO strength figure on the page — 127 reconstructed from six-gun volunteer-battery norms, flagged; casualties wounded 13 men, 28 horses killed (marker); Capt. Hubert Dilger; " + survey,
  }),
  staticUnit({
    id: "us-btty-bancroft",
    name: "Bancroft's Battery G, 4th US Artillery",
    side: "union", strength: 124, x: 4975, z: 5625, facing: 270,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'July 3 About 2 p.m. two sections were engaged in the Cemetery until the repulse of the Confederates.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/4th-us-artillery-battery-g/, " + fetched + ") — ⚠️ the tablet's 'about 2 p.m.' start sits INSIDE the group's documented ruse gap (~13:50 →): witness clock kept here, windows follow the group's documented gap structure — disagreement carried, not reconciled; " +
      "124 men, six 12-pounders; casualties killed 1 officer and 1 man, wounded 11 men, missing 4 (tablet); Lt. Eugene A. Bancroft; " + survey,
  }),
  staticUnit({
    id: "us-btty-taft",
    name: "Taft's 5th New York Independent Battery",
    side: "union", strength: 146, x: 5010, z: 5745, facing: 65,
    frontage_m: 110, depth_m: 40, grade: "A",
    citation:
      "Baltimore Pike marker verbatim: 'July 3. Engaged at intervals in same position until 4 p.m. One gun on Baltimore Pike having burst, the other three relieved the section firing westwardly. Remained in this position until close of battle.' — July 2 disposition 'Four guns south of and facing Baltimore Pike firing on a Confederate battery on Benner's Hill. Two guns firing westwardly'; 1,114 rounds expended (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/5th-new-york-independent-battery/, " + fetched + "); " +
      "146 men, six 20-pounder Parrott Rifles; Capt. Elijah D. Taft; " +
      "ADJUDICATION — his brigade tablet also lists '1st Conn. Heavy Battery B Capt. Albert F. Brooker Not engaged' and '1st Conn. Heavy Battery M Capt. Franklin A. Pratt Not engaged' (https://gettysburg.stonesentinels.com/union-headquarters/2nd-volunteer-brigade-artillery-reserve/, " + fetched + "): verified this session the two batteries WERE NOT AT GETTYSBURG — held in reserve off-map with the trains (https://civilwarintheeast.com/us-regiments-batteries/connecticut/1st-connecticut-heavy-artillery/) — NOT authored, per the off-map geometry ruling; " + survey,
  }),
  staticUnit({
    id: "us-btty-mason",
    name: "Mason's Battery H, 1st US Artillery (Eakin's)",
    side: "union", strength: 153, x: 4930, z: 5585, facing: 245,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet verbatim: 'In position on Cemetery Hill facing the Emmitsburg Road. Engaged July 2nd and 3rd. Lieut. Eakin severely wounded after his guns went into battery and the command devolved on Lieut. Philip D. Mason.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/1st-us-artillery-battery-h/, " + fetched + "); " +
      "153 men, six 12-pounder Napoleons; casualties 1 killed, 1 officer and 7 men wounded, 1 missing (tablet); " + survey,
  }),
  staticUnit({
    id: "us-btty-edgell",
    name: "Edgell's 1st New Hampshire Battery",
    side: "union", strength: 111, x: 4960, z: 5675, facing: 285,
    frontage_m: 70, depth_m: 40, grade: "A",
    citation:
      "monument verbatim: 'On this ground / Edgell's 1st New Hampshire Battery / Light Artillery / fired three hundred and fifty-three / rounds of ammunition / July 2nd and 3rd, 1863' (https://gettysburg.stonesentinels.com/union-monuments/new-hampshire/1st-new-hampshire-battery/, " + fetched + "); brigade tablet: '1st New Hampshire Battery Capt. Frederick M. Edgell July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + "); " +
      "111 men, four Ordnance Rifles; 3 wounded; relieved Hall's 2nd ME — Hall's rear withdrawal rides us-btty-hall-2me; " + survey,
  }),
  staticUnit({
    id: "us-btty-norton",
    name: "Norton's Battery H, 1st Ohio Light Artillery (Huntington's)",
    side: "union", strength: 123, x: 4940, z: 5540, facing: 260,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "brigade tablet verbatim: '1st Ohio Battery H Lieut. George W. Norton July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + "); monument front: 'Huntington's Battery H 1st Ohio Light Artillery [3rd Volunteer Brigade Artillery Reserve] July 2d and 3d 1863.' (https://gettysburg.stonesentinels.com/union-monuments/ohio/battery-h-1st-ohio-artillery/); " +
      "123 men, six 3-inch Ordnance Rifles; " + survey,
  }),
  staticUnit({
    id: "us-btty-hill-wv",
    name: "Hill's Battery C, 1st West Virginia Artillery",
    side: "union", strength: 124, x: 5015, z: 5595, facing: 290,
    frontage_m: 70, depth_m: 40, grade: "A",
    citation:
      "brigade tablet verbatim: 'West Virginia Battery C Capt. Wallace Hill July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + "); monument stands along the eastern fence between the National and Evergreen Cemeteries (https://gettysburg.stonesentinels.com/union-monuments/west-virginia/battery-c-first-west-virginia-artillery/); " +
      "124 men, four 10-pounder Parrott Rifles; casualties incl. Charles Lacy killed July 3 (page); " + survey,
  }),
  staticUnit({
    id: "us-btty-ricketts",
    name: "Ricketts's Batteries F & G, 1st Pennsylvania Light Artillery",
    side: "union", strength: 144, x: 5140, z: 5745, facing: 55,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "monument verbatim: 'July 3rd. Engaged with the Rebel batteries on the left and centre of the line.' — in the East Cemetery Hill lunettes held hand-to-hand the evening of July 2 (https://gettysburg.stonesentinels.com/union-monuments/pennsylvania/pennsylvania-artillery/batteries-f-g-1st-pennsylvania-artillery/, " + fetched + "); brigade tablet: 'July 2 and 3. Engaged on East Cemetery Hill.' (" + huntingtonTablet + "); " +
      "144 men, six Ordnance Rifles; casualties 6 killed, 14 wounded, 3 missing (page); Capt. R. Bruce Ricketts; " + survey,
  }),
];

// ---- Wainwright's I Corps guns (East Cemetery Hill / Stevens Knoll /
// ---- Baltimore Pike) + the withdrawn 2nd Maine ------------------------------

const wainwrightGroup: Unit[] = [
  staticUnit({
    id: "us-btty-stevens",
    name: "Stevens's 5th Maine Battery (Whittier's)",
    side: "union", strength: 136, x: 5380, z: 5680, facing: 15,
    frontage_m: 100, depth_m: 40, grade: "B+",
    citation:
      "monument on Stevens' Knoll: 'Fought here July 1,2,3, 1863', 979 rounds expended across the battle; the July 2 inscription is the famous one (double canister into the East Cemetery Hill assault's flank) — July 3 in-window fire rides the hill-wide collective only, per-battery flagged (https://gettysburg.stonesentinels.com/union-monuments/maine/5th-maine-battery/, " + fetched + "); " +
      "136 men, six 12-pounder Napoleons; Lt. Edward N. Whittier commanding (Capt. Stevens wounded July 2); Stevens Knoll position class between East Cemetery Hill and Culp's Hill (survey §1); " + survey,
  }),
  staticUnit({
    id: "us-btty-breck",
    name: "Breck's Battery L (with E), 1st New York Light Artillery (Reynolds's)",
    side: "union", strength: 141, x: 5090, z: 5720, facing: 50,
    frontage_m: 100, depth_m: 40, grade: "A-",
    citation:
      "Reynolds Ave monument rear: 'July 2nd and 3rd engaged with enemy from position on Cemetery Hill.'; East Cemetery Hill works monument: 'These works were built and held by Battery L, Lieutenant George Breck commanding against assaults of infantry and artillery during the second and third days of July 1863.' (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/1st-new-york-battery-l/ and .../1st-new-york-battery-e/, " + fetched + "); " +
      "141 men, six 3-inch Ordnance Rifles; Lt. George Breck commanding (Capt. Reynolds wounded July 1); " + survey,
  }),
  staticUnit({
    id: "us-btty-stewart",
    name: "Stewart's Battery B, 4th US Artillery",
    side: "union", strength: 132, x: 5055, z: 5845, facing: 350,
    frontage_m: 100, depth_m: 40, grade: "B+",
    citation:
      "War Dept tablet, July 2 & 3: 'Remained in this position.' — astride the Baltimore Pike commanding the approach from the town (" + survey + "); presence attested, July 3 fire NOT battery-specific — rides the hill-wide collective, flagged (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/4th-us-artillery-battery-b/, " + fetched + "); " +
      "132 men, six 12-pounders; casualties 2 killed, 2 officers and 23 men wounded, 3 missing (tablet); Lt. James Stewart",
  }),
  staticUnit({
    id: "us-btty-hall-2me",
    name: "Hall's 2nd Maine Battery (rear, withdrawn)",
    side: "union", strength: 127, x: 5200, z: 5300, facing: 0,
    formation: "column", frontage_m: 70, depth_m: 40, grade: "B+",
    citation:
      "DOCUMENTED NEGATIVE — page narrative: 'The battery was relieved by the 1st New Hampshire Battery from the Reserve Artillery. Hall's men went to the rear and took no further part in the battle.' — present and honest at its rear park, NO events; 'no action July 3' (" + survey + ") (https://gettysburg.stonesentinels.com/union-monuments/maine/2nd-maine-battery/, " + fetched + "); " +
      "rear position class behind Cemetery Hill — coordinate inferred within it; 127 men brought to the field, six 3-inch Ordnance Rifles, 18 wounded and 4 captured (all July 1); Capt. James A. Hall",
  }),
];

// ---- Rittenhouse on Little Round Top ----------------------------------------

const rittenhouse = staticUnit({
  id: "us-btty-rittenhouse",
  name: "Rittenhouse's Battery D, 5th US Artillery (Hazlett's)",
  side: "union", strength: 73, x: 4300, z: 2445, facing: 280,
  frontage_m: 100, depth_m: 40, grade: "A",
  citation:
    "War Dept tablet verbatim: 'July 3 Remained in position and in the afternoon did effective service on the lines of infantry engaged in Longstreet's Assault.' — Little Round Top summit (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/5th-us-artillery-battery-d/, " + fetched + "); the survey's grade-A enfilade of Kemper's right (" + survey + "; civilwarintheeast.com Battery D page); " +
    "2 officers and 71 men, six 10-pounder Parrott Rifles; Lt. Benjamin F. Rittenhouse commanding (Lt. Hazlett mortally wounded July 2); casualties killed 1 officer and 8 men, wounded 5 (tablet)",
});

// ---- XII Corps artillery: 26 guns, ZERO events — the silence is the point --

const xiiCorpsGuns: Unit[] = [
  staticUnit({
    id: "us-btty-rugg",
    name: "Rugg's Battery F, 4th US Artillery",
    side: "union", strength: 96, x: 5510, z: 4930, facing: 55,
    frontage_m: 100, depth_m: 40, grade: "B+",
    citation:
      "War Dept tablet verbatim: 'July 3. At 1 a.m. posted opposite the centre of the line of the Twelfth Corps and at 4:30 opened fire on the Confederates… Continued firing until after 10 a.m. when the Confederates were driven from the line. In the afternoon the Battery was exposed to a severe shelling which passed over Cemetery Hill.' — under the cannonade's overshoots, holding, silent (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/4th-us-artillery-battery-f/, " + fetched + "); " +
      "96 men, six 12-pounder Napoleons; 1 wounded; Lt. Sylvanus T. Rugg; " + muhlenbergSilence,
  }),
  staticUnit({
    id: "us-btty-kinzie",
    name: "Kinzie's Battery K, 5th US Artillery",
    side: "union", strength: 77, x: 5560, z: 4880, facing: 60,
    frontage_m: 70, depth_m: 40, grade: "B+",
    citation:
      "War Dept tablet: 'posted with Battery F 4th U.S. Artillery on the south side of Baltimore Pike opposite' the centre of the Twelfth Corps line; 'At 4:30 a.m. opened fire on the Confederates in possession of the line vacated by the Twelfth' — morning fight only (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/5th-us-artillery-battery-k/, " + fetched + "); " +
      "2 officers and 75 men, four 12-pounder Napoleons; 5 wounded; Lt. David H. Kinzie; " + muhlenbergSilence,
  }),
  staticUnit({
    id: "us-btty-atwell",
    name: "Atwell's Independent Battery E, Pennsylvania Light (Knap's)",
    side: "union", strength: 139, x: 5960, z: 4100, facing: 40,
    frontage_m: 100, depth_m: 40, grade: "B+",
    citation:
      "Powers Hill monument rear: '…the Battery went into this position where it remained until the close of the battle.' (https://gettysburg.stonesentinels.com/union-monuments/pennsylvania/pennsylvania-artillery/pennsylvania-independent-battery-e/, " + fetched + "); brigade tablet: 'Battery E Penna. and Battery A Maryland… on Powers's Hill all commanding the valley of Rock Creek'; " +
      "4 officers and 135 men, six 10-pounder Parrott Rifles; 3 wounded; Capt. Charles A. Atwell; Powers Hill local (5982,4077), survey §1; " + muhlenbergSilence,
  }),
  staticUnit({
    id: "us-btty-rigby",
    name: "Rigby's Battery A, 1st Maryland Light Artillery",
    side: "union", strength: 106, x: 6010, z: 4060, facing: 40,
    frontage_m: 100, depth_m: 40, grade: "B+",
    citation:
      "monument rear verbatim: 'Occupied this position on the morning of July 2nd, 1863 and remained in battery until the termination of the battle engaging a battery of the enemy on the 2nd, and on the morning of the 3rd shelling the woods in front for nearly three hours assisting in driving out the enemy.' — morning fire only; attached to the XII Corps guns from the Reserve's 4th Volunteer Brigade (https://gettysburg.stonesentinels.com/union-monuments/maryland/maryland-battery-a/, " + fetched + "); " +
      "4 officers and 102 men, six Ordnance Rifles; Capt. James H. Rigby; Powers Hill, " + muhlenbergSilence,
  }),
  staticUnit({
    id: "us-btty-winegar",
    name: "Winegar's Battery M, 1st New York Light Artillery",
    side: "union", strength: 96, x: 6600, z: 3720, facing: 30,
    frontage_m: 70, depth_m: 40, grade: "B+",
    citation:
      "monument front verbatim: 'Battery M / 1st N.Y. Light Artillery / 1st Division, 12th Corps / Held this position / July 2d – 3d, 1863.' — McAllister's Hill above the Baltimore Pike crossing of Rock Creek (survey §1: McAllister's Mill 6683,3671) (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/1st-new-york-battery-m/, " + fetched + "); " +
      "96 men, four 10-pounder Parrott Rifles, no casualties at Gettysburg; Lt. Charles E. Winegar; " + muhlenbergSilence,
  }),
];

// ---- the Artillery Reserve park + ammunition train — the plan's one
// ---- non-combat exception: an organized artillery command with an A−
// ---- documented mid-window displacement (its motion derives dust for free) --

const tylerOrgPage =
  "https://gettysburg.stonesentinels.com/armies/army-of-the-potomac/artillery-reserve/";
const huntAccount = "https://www.historycentral.com/CivilWar/getty/Hunt3.html";

const reservePark = moverUnit({
  id: "us-arty-reserve-park",
  name: "Artillery Reserve park + ammunition train (Tyler's)",
  side: "union", frontage_m: 350, depth_m: 250,
  keyframes: [
    { t: 0, x: 5150, z: 4350, facing: 200, formation: "column", strength: 2375, confidence: "documented",
      citation: "parked between the Taneytown Rd and the Baltimore Pike behind the centre (" + survey + ", Artillery Reserve bullet); Brig. Gen. Robert O. Tyler commanding, 2,375 men and 114 guns nominal (" + tylerOrgPage + ", " + fetched + ") — the roster INCLUDES the reserve brigades deployed forward and authored as their own units (McGilvery's line, Huntington's and Taft's hill batteries, Fitzhugh's reinforcements): this unit renders the caisson park + ammunition train, strength kept at the command roster per the plan; coordinate inferred within the park position class" },
    { t: 5700, x: 5150, z: 4350, facing: 200, formation: "column", strength: 2375, confidence: "inferred",
      citation: "limbers under the cannonade's overshoots — departure clock inferred mid-cannonade (Hunt, riding back for fresh batteries, found the park already gone)" },
    { t: 6600, x: 4650, z: 2950, facing: 200, formation: "column", strength: 2375, confidence: "documented",
      citation: "Hunt's own account verbatim: 'Thence I rode to the Artillery Reserve to order fresh batteries and ammunition to be sent up to the ridge as soon as the cannonade ceased; but both the reserve and the train had gone to a safer place.' and 'Turning into the Taneytown pike, I saw evidence of the necessity under which the reserve had ‘decamped,’ in the remains of a dozen exploded caissons, which had been placed under cover of a hill, but which the shells had managed to search out.' (" + huntAccount + ", " + fetched + "; americanheritage.com Slaughter on Cemetery Ridge) — displaced south behind the Round Top ground, the survey's A− in-window mover; sheet 8 (1–5 PM) draws its 'Artillery Reserve' label east of the Weikert house — the in-window southward park corroborated, coordinate inferred within the position class, precision disagreement carried" },
    { t: WindowEndT, x: 4650, z: 2950, facing: 200, formation: "column", strength: 2375, confidence: "inferred",
      citation: "holds the safer park to the window end; the dozen wrecked caissons on the Taneytown pike ride the arrival citation (Hunt)" },
  ],
});

// ---- events: Osborn's two windows around the documented ruse gap;
// ---- Wainwright's cannonade replies; Rittenhouse's enfilade -----------------
// Spine: cannonade reply 600–5400 (modeled II Corps pattern); Osborn's hill
// goes deliberately quiet at t=3000 (~13:50, the ruse); step-off ~7500;
// advance/repulse 7800–9000.

const osbornFiring = [
  ["wiedrich", "us-btty-wiedrich"], ["dilger", "us-btty-dilger"],
  ["bancroft", "us-btty-bancroft"], ["taft", "us-btty-taft"],
  ["mason", "us-btty-mason"], ["edgell", "us-btty-edgell"],
  ["norton", "us-btty-norton"], ["hill-wv", "us-btty-hill-wv"],
  ["ricketts", "us-btty-ricketts"],
] as const;

// Battery-specific July 3 fire text (documented) vs collective-only (inferred).
const osbornDocumented: Record<string, { counterbattery?: string; flank?: string }> = {
  "us-btty-bancroft": {
    flank: "tablet verbatim: 'July 3 About 2 p.m. two sections were engaged in the Cemetery until the repulse of the Confederates.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/4th-us-artillery-battery-g/, " + fetched + ")",
  },
  "us-btty-taft": {
    counterbattery: "marker verbatim: 'July 3. Engaged at intervals in same position until 4 p.m.' — 'at intervals' carries the gap (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/5th-new-york-independent-battery/, " + fetched + ")",
    flank: "marker verbatim: 'July 3. Engaged at intervals in same position until 4 p.m. One gun on Baltimore Pike having burst, the other three relieved the section firing westwardly.' (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/5th-new-york-independent-battery/, " + fetched + ")",
  },
  "us-btty-mason": {
    counterbattery: "tablet verbatim: 'Engaged July 2nd and 3rd.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/1st-us-artillery-battery-h/, " + fetched + ")",
    flank: "tablet verbatim: 'Engaged July 2nd and 3rd.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/1st-us-artillery-battery-h/, " + fetched + ")",
  },
  "us-btty-edgell": {
    counterbattery: "monument verbatim: 'fired three hundred and fifty-three rounds of ammunition July 2nd and 3rd, 1863' (https://gettysburg.stonesentinels.com/union-monuments/new-hampshire/1st-new-hampshire-battery/, " + fetched + "); brigade tablet: 'July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ")",
    flank: "brigade tablet verbatim: 'July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + ")",
  },
  "us-btty-norton": {
    counterbattery: "brigade tablet verbatim: '1st Ohio Battery H Lieut. George W. Norton July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + ")",
    flank: "brigade tablet verbatim: '1st Ohio Battery H Lieut. George W. Norton July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + ")",
  },
  "us-btty-hill-wv": {
    counterbattery: "brigade tablet verbatim: 'West Virginia Battery C Capt. Wallace Hill July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + ")",
    flank: "brigade tablet verbatim: 'West Virginia Battery C Capt. Wallace Hill July 2 and 3. Engaged on Cemetery Hill.' (" + huntingtonTablet + ", " + fetched + ")",
  },
  "us-btty-ricketts": {
    counterbattery: "monument verbatim: 'July 3rd. Engaged with the Rebel batteries on the left and centre of the line.' (https://gettysburg.stonesentinels.com/union-monuments/pennsylvania/pennsylvania-artillery/batteries-f-g-1st-pennsylvania-artillery/, " + fetched + ")",
  },
};

const unwindowedNote =
  "the attestation is phase-unwindowed — window rides the group's documented gap structure (counter-battery to ~13:50, then the ruse silence, then the flank fire on the advance)";

const events: EngagementEvent[] = [
  // -- Osborn window A: counter-battery until the hill goes quiet -------------
  ...osbornFiring.map(([short, id]) => {
    const doc = osbornDocumented[id]?.counterbattery;
    return fireEvent({
      id: `us-btty-${short}-counterbattery`, kind: "artillery_fire",
      t0: 600, t1: 3000, unitId: id,
      confidence: doc ? "documented" : "inferred",
      citation: doc ?? osbornCollective,
      note: (doc ? unwindowedNote + "; " : "") + ruseGapNote,
    });
  }),
  // -- Osborn window B: the Pettigrew-flank fire on the advance ---------------
  ...osbornFiring.map(([short, id]) => {
    const doc = osbornDocumented[id]?.flank;
    return fireEvent({
      id: `us-btty-${short}-pettigrew-flank`, kind: "artillery_fire",
      t0: 7800, t1: 9000, unitId: id,
      confidence: doc ? "documented" : "inferred",
      citation: doc ?? osbornCollective,
      note:
        ">1,600 rounds into the flank of Pettigrew's left during the assault (Wikipedia Pickett's Charge; survey §4 item 9); window on the track spine (advance under way ~15:10 to the climax's collapse)" +
        (id === "us-btty-bancroft"
          ? "; ⚠️ the tablet's 'about 2 p.m. … until the repulse' straddles the group's documented ruse gap — witness reading kept, window follows the gap"
          : id === "us-btty-ricketts"
            ? "; his own monument attests July 3 counter-battery only — the flank window rides the collective"
            : ""),
    });
  }),
  // -- Wainwright's cannonade replies (not under Osborn's ruse) ---------------
  fireEvent({
    id: "us-btty-stevens-cannonade-reply", kind: "artillery_fire",
    t0: 600, t1: 5400, unitId: "us-btty-stevens", confidence: "inferred",
    citation: wainwrightCollective,
    note: "his monument's famous inscription is the July 2 double-canister flank fire; July 3 fire is collective-grade only",
  }),
  fireEvent({
    id: "us-btty-breck-cannonade-reply", kind: "artillery_fire",
    t0: 600, t1: 5400, unitId: "us-btty-breck", confidence: "documented",
    citation: "Reynolds Ave monument rear verbatim: 'July 2nd and 3rd engaged with enemy from position on Cemetery Hill.' (https://gettysburg.stonesentinels.com/union-monuments/new-york/new-york-artillery-and-engineers/1st-new-york-battery-l/, " + fetched + ")",
    note: "phase-unwindowed — window on the modeled cannonade-reply pattern (600–5400, the army-wide ~14:30 conserve-ammunition cease)",
  }),
  fireEvent({
    id: "us-btty-stewart-cannonade-reply", kind: "artillery_fire",
    t0: 600, t1: 5400, unitId: "us-btty-stewart", confidence: "inferred",
    citation: wainwrightCollective,
    note: "his own tablet attests only 'Remained in this position.' — presence, not fire; tension with the hill-wide collective kept visible here (the Wave-1 James pattern)",
  }),
  // -- Rittenhouse: the wave's headline — the Little Round Top enfilade -------
  fireEvent({
    id: "us-btty-rittenhouse-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-rittenhouse", confidence: "documented",
    citation: "War Dept tablet verbatim: 'July 3 Remained in position and in the afternoon did effective service on the lines of infantry engaged in Longstreet's Assault.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/5th-us-artillery-battery-d/, " + fetched + ") — the survey's grade-A enfilade of Kemper's right from the Little Round Top summit (" + survey + "; §4 item 9: 'Rittenhouse enfilades from Little Round Top'; Wikipedia Pickett's Charge: only McGilvery, Osborn and Rittenhouse fired on the advance itself)",
    note: "window on the track spine, coherent with the assault (advance under way ~15:10 to the climax's collapse) — the Wave-1 7800–9000 pattern; his cannonade-phase fire is not tablet-attested and is NOT authored",
  }),
];

// ---- assemble, gate, export -------------------------------------------------
// The 20 units land grouped after Wave 1's block (Wheeler is the current last
// unit) — the gun ring closes north (Cemetery Hill), east (Baltimore Pike),
// south (Little Round Top), and rear (Powers/McAllister's, the silent guns).

const wave2: Unit[] = [
  ...osbornGroup, ...wainwrightGroup, rittenhouse, ...xiiCorpsGuns, reservePark,
];
battle = addUnitsAfter(battle, "us-btty-wheeler", wave2);
battle.events = [...(battle.events ?? []), ...events]; // exportValidated sorts canonically

writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events?.length ?? 0} events`);
