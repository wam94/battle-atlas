// Wave 1 authoring script (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 3): McGilvery's massed line on lower Cemetery Ridge + the four
// reinforcing batteries — 13 units, 17 artillery_fire events. Survey basis:
// docs/research/2026-07-02-full-cast-sources.md §3.3 (McGilvery bullet +
// replacement-battery bullet), §4 items 2/6/9, §5.2 step 1. Position classes
// read off Bachelder timed sheet No. 8 (the saved crop
// docs/research/assets/sheet8-crops/s8_mcgilvery.jpg, re-read at 100% this
// session for the survey's open item 2); strengths/armament/July-3 activity
// text fetched from the survey's Stone Sentinels URLs 2026-07-08. Everything
// funnels through the Task 1 generator lib, then the existing gates
// (validateBattle/exportBattle). Run from tool/:
//   npx vite-node scripts/author-w1-mcgilvery.ts
// Committed as the derivation record for the authored data.
//
// Sheet-8 adjudication (survey open item 2, ruled at hand-edit): the sheet's
// written McGilvery list reads, top (north) to bottom (south): Daniels Mich. /
// Thomas / Thompson Pa / Phillips 5 Ms. / Hart N.Y. / Sterling / "ROCK"
// (Rank?) / Cooper / Dow / Ames. "Daniels" (9th Michigan) and "Rank" are
// OOB-attested at East Cavalry Field (off-map) — ruling FOLLOWS THE OOB:
// neither is authored; the disagreement rides Thompson's t=0 citation.
// Battery K 4th US (Seeley's, Lt. James) is not on the sheet's list but is
// placed on Thompson's right by Thompson's tablet — slotted between Thomas
// and Thompson.
//
// The cannonade-long silence of the line is NOT an event — Hunt's hold-fire /
// conserve-ammunition policy rides every line battery's t=0 keyframe citation
// (battle-format.md "Documented silence"). The two attested cannonade-phase
// exceptions get real windows: Phillips ~13:30 on Hunt's own order, Hart on
// Hancock's direction — the Hunt-vs-Hancock command dispute is carried as
// data, never reconciled.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import { exportBattle } from "../src/io";
import { validateBattle } from "../src/validate";
import type { Battle, EngagementEvent, Unit } from "../src/model";
import { addUnitsAfter, fireEvent, moverUnit, staticUnit, WindowEndT } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const survey = "docs/research/2026-07-02-full-cast-sources.md §3.3";
const sheet8 =
  "Bachelder timed sheet No. 8 written McGilvery battery list (s8_mcgilvery.jpg crop — position class, time inferred)";
const holdFire =
  "held fire through the cannonade under Hunt's conserve-ammunition policy — the line's silence is itself documented " +
  `(${survey}; padresteve.com Hunt essay; Wikipedia Pickett's Charge: only McGilvery, Osborn and Rittenhouse fired on the advance itself; McGilvery's own OR report not found online — flagged)`;

// ---- McGilvery's line, north → south (sheet-8 list order minus the two
// ---- ruled-off labels; ~39 guns per the secondary-grade count) -------------
// Coordinates: lower Cemetery Ridge position classes along the sheet's bar
// line — north end near the left of the II Corps line (Thomas's tablet),
// south end just north of the George Weikert house (local 4112,3646).

const lineBatteries: Unit[] = [
  staticUnit({
    id: "us-btty-thomas",
    name: "Thomas's Battery C, 4th US Artillery",
    side: "union", strength: 115, x: 4230, z: 4290, facing: 265,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet: 'July 3 In position near the left of the Second Corps line.' (Stone Sentinels 4th US Artillery Battery C page, fetched 2026-07-08); " +
      sheet8 + " — 'THOMAS' second from the top of the list, north end; six 12-pounders (tablet); " +
      "NO strength figure on the tablet page — 115 reconstructed from six-gun regular-battery norms, flagged; " +
      "casualties killed 1 man, wounded 1 officer and 16 men (tablet); " + holdFire,
  }),
  staticUnit({
    id: "us-btty-james",
    name: "James's Battery K, 4th US Artillery (Seeley's)",
    side: "union", strength: 134, x: 4225, z: 4220, facing: 265,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "War Dept tablet: 'July 3 Remained in the position of the previous night.' (Stone Sentinels 4th US Artillery Battery K page, fetched 2026-07-08) — presence attested, fire NOT; " +
      "Lt. Robert James commanding after Seeley twice wounded July 2 (III Corps battery on McGilvery's line, " + survey + "); " +
      "slot on Thompson's right per Thompson's tablet ('on line with Battery K 4th U.S. on right' — " + survey + "); not on the sheet-8 written list; " +
      "134 men and six 12-pounders brought to the field (Stone Sentinels); " + holdFire,
  }),
  staticUnit({
    id: "us-btty-thompson",
    name: "Thompson's Batteries C & F, 1st Pennsylvania Light Artillery",
    side: "union", strength: 105, x: 4220, z: 4150, facing: 265,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "Battery C monument: 'July 3rd. In position on right of First Volunteer Brigade Reserve Artillery and engaged the enemy.'; Battery F monument: 'July 3. With the left centre on Cemetery Ridge on left of First Volunteer Brigade Reserve Artillery marked by tablet.' (Stone Sentinels Pennsylvania Independent Batteries C & F page, fetched 2026-07-08); " +
      sheet8 + " — 'THUMPSON Pa'; 105 officers and men consolidated, six 3-inch Ordnance Rifles; " +
      "SHEET-VS-OOB ADJUDICATION (survey open item 2, crop re-read at 100% this session): the sheet's list also writes 'Daniels Mich.' above Thomas and 'ROCK' (Rank?) between Sterling and Cooper — the OOB places Daniels's 9th Michigan and Rank's PA section at East Cavalry Field (Wikipedia Union OOB; Rank's section on the Hanover Rd per the Gregg HQ marker); ruling follows the OOB, neither authored — disagreement kept, not reconciled; " +
      holdFire,
  }),
  staticUnit({
    id: "us-btty-phillips",
    name: "Phillips's 5th Massachusetts Battery (E)",
    side: "union", strength: 104, x: 4212, z: 4080, facing: 265,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "Hancock Avenue tablet (Stone Sentinels 5th Massachusetts Battery page, fetched 2026-07-08): the line's best single firing attestation — 'About 1:30 by order of Brig. Gen. H.J. Hunt fired on the Confederate batteries but did little damage.', then " + holdFire + "; " +
      sheet8 + " — 'PHILLIPS 5 Ms.' between Thompson and Hart; 104 men and six 3-inch Ordnance Rifles brought to the field; " +
      "casualties 7 enlisted men killed, 1 officer and 12 men wounded (tablet); Capt. Charles A. Phillips",
  }),
  staticUnit({
    id: "us-btty-hart",
    name: "Hart's 15th New York Independent Battery",
    side: "union", strength: 99, x: 4205, z: 4010, facing: 265,
    frontage_m: 70, depth_m: 40, grade: "A",
    citation:
      "Hancock Avenue marker (Stone Sentinels 15th New York Independent Battery page, fetched 2026-07-08): 'July 3. Ordered early to the front and took position in the battalion on the left of Battery E, 5th Massachusetts. Directed by Maj. General Hancock to open on the Confederate batteries with solid shot and shell.' — Hancock's order AGAINST Hunt's conserve-ammunition policy: the command dispute is carried as data on this unit and its counter-battery event, not reconciled (" + survey + "); " +
      sheet8 + " — 'HART. N.Y.' on Phillips's left, corroborating the marker; 99 men and four 12-pounders; " +
      "casualties killed 3 men, wounded 2 officers and 11 men (marker); Capt. Patrick Hart, wounded July 3",
  }),
  staticUnit({
    id: "us-btty-sterling",
    name: "Sterling's 2nd Connecticut Light Battery",
    side: "union", strength: 106, x: 4195, z: 3940, facing: 265,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "Monument front: 'Artillery Reserve / Position July 3, 1863.' — position attested; the monument and page carry NO July 3 activity text, so in-window fire is not battery-specific (survey §3.3 flag kept) (Stone Sentinels 2nd Connecticut Light Battery page, fetched 2026-07-08); " +
      sheet8 + " — 'STERLING.'; 106 men, four 14-pounder James Rifles and two 12-pounder howitzers (the only Federal battery at Gettysburg with either type); " + holdFire,
  }),
  staticUnit({
    id: "us-btty-dow",
    name: "Dow's 6th Maine Battery (F, 1st Maine Light)",
    side: "union", strength: 103, x: 4180, z: 3800, facing: 265,
    frontage_m: 70, depth_m: 40, grade: "A",
    citation:
      "Monument: 'Dow's 6th Maine Battery / McGilvery's Brigade. Reserve Artillery.' — no July 3 activity inscription; the page's descriptive text (prose, not tablet) has the battery suffer 13 men wounded 'in the artillery duel that preceded Pickett's Charge on the afternoon of July 3' — under fire while holding (Stone Sentinels 6th Maine Battery page, fetched 2026-07-08); " +
      sheet8 + " — 'DOW.' second from the south end; 103 men, four 12-pounder Napoleons; Lt. Edwin B. Dow; " + holdFire,
  }),
  staticUnit({
    id: "us-btty-ames",
    name: "Ames's Battery G, 1st New York Light Artillery",
    side: "union", strength: 132, x: 4172, z: 3730, facing: 265,
    frontage_m: 100, depth_m: 40, grade: "A",
    citation:
      "Monument front: 'July 3, on Cemetery Ridge with 1st Div., 2d Corps.' (Stone Sentinels 1st New York Battery G page, fetched 2026-07-08) — the monument pulls toward Caldwell's ground at the II Corps south end while " +
      sheet8 + " writes 'AMES.' at the very SOUTH end of McGilvery's list near the George Weikert house — sheet position class kept, monument reading carried, disagreement not reconciled; " +
      "132 men, six 12-pounder Napoleons; casualties 7 wounded (monument); Capt. Nelson Ames; " + holdFire,
  }),
];

// ---- the two documented in-window battery tracks on/joining the line -------

const cooper = moverUnit({
  id: "us-btty-cooper",
  name: "Cooper's Battery B, 1st Pennsylvania Light Artillery",
  side: "union", frontage_m: 70, depth_m: 40,
  keyframes: [
    { t: 0, x: 5100, z: 5740, facing: 262, formation: "line", strength: 114, confidence: "documented",
      citation: "on East Cemetery Hill from July 1 until 3 p.m. July 3 — 'July 3 Moved to this position from East Cemetery Hill at 3 p.m.' reads back to this start (Stone Sentinels Battery B 1st Pennsylvania Artillery page, Hancock Avenue marker, fetched 2026-07-08); 114 men and four 3-inch Ordnance Rifles brought to the field; East Cemetery Hill local (5107,5753), survey §1 landmark table" },
    { t: 6600, x: 5100, z: 5740, facing: 262, formation: "column", strength: 114, confidence: "inferred",
      citation: "limbers and departs ~14:50 to make the marker's 3 p.m. arrival at a trot — clock inferred, move documented" },
    { t: 7200, x: 4188, z: 3870, facing: 265, formation: "line", strength: 114, confidence: "documented",
      citation: "Hancock Avenue marker verbatim: 'July 3 Moved to this position from East Cemetery Hill at 3 p.m. during a heavy cannonade and opened fire upon a Confederate Battery in front.' (Stone Sentinels Battery B 1st Pennsylvania Artillery page) — a documented in-window battery track (survey §3.3); slot in McGilvery's line per the sheet-8 list ('COOPER.' between the ROCK label and Dow); coordinate = that slot's position class" },
    { t: WindowEndT, x: 4188, z: 3870, facing: 265, formation: "line", strength: 114, confidence: "inferred",
      citation: "holds the line slot to the window end; casualties killed 3 men, wounded 1 officer and 8 men, total 12 (marker) — decline not clocked, strength held" },
  ],
});

const wheeler = moverUnit({
  id: "us-btty-wheeler",
  name: "Wheeler's 13th New York Independent Battery",
  side: "union", frontage_m: 70, depth_m: 40,
  keyframes: [
    { t: 0, x: 5075, z: 5560, facing: 262, formation: "line", strength: 118, confidence: "documented",
      citation: "Wheeler's OR report, transcribed at 13nybattery.com/battles/wheeler_gettys.htm (VERIFIED, survey §2.2): 'During the morning of July 3, I lay in reserve behind Cemetery Hill. During the heavy cannonade from 1 to 3 p.m., I lost some horses, but fortunately no men.' — reserve position class behind Cemetery Hill (local 4993,5656), coordinate inferred; 118 men and four 3-inch Ordnance Rifles (Stone Sentinels 13th New York Independent Battery page, fetched 2026-07-08)" },
    { t: 10200, x: 5075, z: 5560, facing: 262, formation: "column", strength: 118, confidence: "inferred",
      citation: "limbers on the ~4 p.m. order — departure clock inferred to land the arrival tight against the window end" },
    { t: 10680, x: 4560, z: 5135, facing: 250, formation: "line", strength: 118, confidence: "documented",
      citation: "OR verbatim: 'At about 4 p.m. I received an order form [sic, transcription] you to go to assist the Second Corps, upon which a very heavy attack was being made.' — the report's ~4 p.m. sits at our window's very end; encoded tight against it per the plan (t=10680 ≈ 15:58); coordinate = north flank of the II Corps line near Ziegler's Grove, the approach from Cemetery Hill that gives the report's enfilade geometry — position class inferred; ⚠️ the Stone Sentinels monument narrative keeps the battery on Cemetery Hill July 3 — the OR is primary for the move, disagreement kept" },
    { t: WindowEndT, x: 4560, z: 5135, facing: 250, formation: "line", strength: 118, confidence: "inferred",
      citation: "in action at the window end (his canister fight runs past 16:00 — post-window remainder not modeled); casualties 8 wounded, 3 missing (Stone Sentinels) — decline not clocked" },
  ],
});

// ---- the reinforcing batteries fed to the crest -----------------------------

const fitzhugh = moverUnit({
  id: "us-btty-fitzhugh",
  name: "Fitzhugh's Battery K, 1st New York Light Artillery (11th NY attached)",
  side: "union", frontage_m: 100, depth_m: 40,
  keyframes: [
    { t: 0, x: 4600, z: 3850, facing: 265, formation: "line", strength: 149, confidence: "inferred",
      citation: "in reserve with the Artillery Reserve; park position class from sheet 8's 'Artillery Reserve' label east of the Weikert house (s8_mcgilvery.jpg) — exact park slot unattested" },
    { t: 7980, x: 4600, z: 3850, facing: 265, formation: "column", strength: 149, confidence: "inferred",
      citation: "'ordered into line at the gallop' at the height of Pickett's Charge (Stone Sentinels 1st New York Battery K page narrative, fetched 2026-07-08); departure clock inferred" },
    { t: 8280, x: 4437, z: 4640, facing: 262, formation: "line", strength: 149, confidence: "documented",
      citation: "monument rear verbatim: 'Battery K, (Fitzhugh's), Held this position July 3rd 1863 and assisted in repulsing Pickett's Charge.' (Stone Sentinels 1st New York Battery K page — slug 1s-new-york-battery-k, fetched 2026-07-08); on the Second Corps line south of the Copse ('on Second Corps line', survey §3.3); 149 men, six 3-inch Ordnance Rifles; arrival clock ~15:18 inferred from the narrative's height-of-the-charge timing" },
    { t: WindowEndT, x: 4437, z: 4640, facing: 262, formation: "line", strength: 149, confidence: "inferred",
      citation: "holds the crest position to the window end; casualties 7 wounded (monument)" },
  ],
});

const parsons = moverUnit({
  id: "us-btty-parsons",
  name: "Parsons's Battery A, 1st New Jersey Light Artillery",
  side: "union", frontage_m: 100, depth_m: 40,
  keyframes: [
    { t: 0, x: 5850, z: 3960, facing: 262, formation: "line", strength: 116, confidence: "documented",
      citation: "monument front verbatim: 'Battery A, 1st N. J. Art. from its position in reserve S.W. of Powers Hill…' (Stone Sentinels New Jersey Battery A page, fetched 2026-07-08) — reserve position class SW of Powers Hill (local 5982,4077, survey §1), coordinate inferred within it; 116 men and six 10-pounder Parrott Rifles brought to the field; 1st Lt. Augustine N. Parsons commanding (Capt. Hexamer on sick leave)" },
    { t: 6840, x: 5850, z: 3960, facing: 262, formation: "column", strength: 116, confidence: "inferred",
      citation: "limbers for the monument's 3 p.m. gallop — departure clock inferred" },
    { t: 7200, x: 4450, z: 4690, facing: 262, formation: "line", strength: 116, confidence: "documented",
      citation: "monument verbatim: '…galloped into action at 3 p.m., July 3, 1863.' with 'Position in action 45 yards E. of this stone.' (Stone Sentinels New Jersey Battery A page) — 'on line of Second Division Second Corps' (survey §3.3); a documented in-window arrival clock" },
    { t: WindowEndT, x: 4450, z: 4690, facing: 262, formation: "line", strength: 116, confidence: "inferred",
      citation: "holds the crest position to the window end; losses killed 2, wounded 7 (monument)" },
  ],
});

const weir = moverUnit({
  id: "us-btty-weir",
  name: "Weir's Battery C, 5th US Artillery",
  side: "union", frontage_m: 100, depth_m: 40,
  keyframes: [
    { t: 0, x: 4620, z: 4680, facing: 262, formation: "line", strength: 123, confidence: "documented",
      citation: "War Dept tablet: 'In the rear of the line until Longstreet's Assault was made…' (Stone Sentinels 5th US Artillery Battery C page, fetched 2026-07-08) — rear-of-the-line position class, coordinate inferred; 2 officers and 121 men, six 12-pounder Napoleons; Lt. Gulian V. Weir" },
    { t: 7500, x: 4620, z: 4680, facing: 262, formation: "column", strength: 123, confidence: "inferred",
      citation: "moves as the assault steps off — 'until Longstreet's Assault was made' (tablet); clock = the track spine's step-off, inferred" },
    { t: 7800, x: 4472, z: 4838, facing: 262, formation: "line", strength: 123, confidence: "documented",
      citation: "tablet verbatim: '…when the Battery was moved up to Brig. General A.S. Webb's line and opened with canister at short range on the advancing Confederates.' (Stone Sentinels 5th US Artillery Battery C page) — behind Webb's crest at the Angle; 'on Cemetery Ridge and in front on left of Second Corps' (survey §3.3, reserve-brigade tablet) — the two tablet position readings both carried; arrival clock inferred" },
    { t: WindowEndT, x: 4472, z: 4838, facing: 262, formation: "line", strength: 123, confidence: "inferred",
      citation: "holds at Webb's line to the window end — the tablet's 'At 6:30 p.m. returned to the Artillery Reserve' is post-window; casualties killed 2 men, wounded 2 officers and 12 men (tablet)" },
  ],
});

// ---- events: the attested fire windows, coherent with the bombardment/charge
// ---- spine (cannonade reply 600–5400; step-off ~7500; canister climax
// ---- 8400–9000; Wilcox/Lang follow-up 10200–10500) --------------------------

const phillipsTablet =
  "Stone Sentinels 5th Massachusetts Battery page, Hancock Avenue tablet, fetched 2026-07-08";
const hartMarker =
  "Stone Sentinels 15th New York Independent Battery page, Hancock Avenue marker, fetched 2026-07-08";
const collectiveEnfilade =
  "collective attestation, the Webb-siblings pattern: McGilvery's ~39 guns enfiladed Pickett's/Kemper's flank at the repulse (survey §4 item 9; padresteve.com Hunt essay; Wikipedia Pickett's Charge: only McGilvery, Osborn and Rittenhouse fired on the advance itself) — per-battery window inferred on the line's attested fire";

const events: EngagementEvent[] = [
  // -- cannonade phase: the two attested exceptions to the hold-fire ----------
  fireEvent({
    id: "us-btty-phillips-counterbattery", kind: "artillery_fire",
    t0: 1800, t1: 2700, unitId: "us-btty-phillips", confidence: "documented",
    citation: `tablet verbatim: 'About 1:30 by order of Brig. Gen. H.J. Hunt fired on the Confederate batteries but did little damage.' (${phillipsTablet})`,
    note: "window LENGTH inferred (~15 min of deliberate fire); the battery then held under Hunt's conserve-ammunition policy — the line's silence resumes and is documented on the keyframes",
  }),
  fireEvent({
    id: "us-btty-hart-counterbattery", kind: "artillery_fire",
    t0: 1800, t1: 3600, unitId: "us-btty-hart", confidence: "documented",
    citation: `marker verbatim: 'Directed by Maj. General Hancock to open on the Confederate batteries with solid shot and shell.' (${hartMarker})`,
    note: "clock unattested — window inferred inside the cannonade; Hancock's direction stands AGAINST Hunt's conserve-ammunition hold-fire — the command dispute carried as data, not reconciled",
  }),
  // -- the repulse enfilade, ~15:10–15:30 on the track spine ------------------
  fireEvent({
    id: "us-btty-phillips-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-phillips", confidence: "documented",
    citation: `tablet verbatim: 'Opened an enfilading fire soon after on Longstreet's advancing line of infantry and assisted in repulsing the assault.' (${phillipsTablet})`,
    note: "the headline attestation of the line's enfilade into the assault's right; window on the track spine (advance under way ~15:10 to the climax's collapse)",
  }),
  fireEvent({
    id: "us-btty-hart-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-hart", confidence: "documented",
    citation: `marker verbatim: 'Upon the advance of the Confederate infantry, fired shell and shrapnel and canister when the line was within 500 yards.' (${hartMarker})`,
    note: "the marker continues past the repulse: 'The fire of the battery was then directed against the artillery on the Confederate right and several caissons and limbers were exploded by the shells' — that later counter-battery fire is not separately windowed",
  }),
  fireEvent({
    id: "us-btty-thompson-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-thompson", confidence: "documented",
    citation: "Battery C monument: 'July 3rd. In position on right of First Volunteer Brigade Reserve Artillery and engaged the enemy.' (Stone Sentinels Pennsylvania Independent Batteries C & F page, fetched 2026-07-08)",
    note: "'engaged the enemy' is unwindowed — the window rides the line's collective repulse attestation (survey §4 item 9)",
  }),
  fireEvent({
    id: "us-btty-thomas-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-thomas", confidence: "inferred",
    citation: collectiveEnfilade,
  }),
  fireEvent({
    id: "us-btty-james-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-james", confidence: "inferred",
    citation: collectiveEnfilade,
    note: "the battery's own tablet attests only 'Remained in the position of the previous night' — presence, not fire; tension with the collective attestation kept visible here",
  }),
  fireEvent({
    id: "us-btty-sterling-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-sterling", confidence: "inferred",
    citation: collectiveEnfilade,
    note: "survey §3.3 flag: Sterling's in-window fire is not battery-specific — no July 3 activity text on monument or page",
  }),
  fireEvent({
    id: "us-btty-dow-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-dow", confidence: "inferred",
    citation: collectiveEnfilade,
    note: "the page's prose has Dow's 13 wounded in the duel that PRECEDED the charge — under fire while holding; his own repulse fire rides the collective only",
  }),
  fireEvent({
    id: "us-btty-ames-repulse-enfilade", kind: "artillery_fire",
    t0: 7800, t1: 9000, unitId: "us-btty-ames", confidence: "inferred",
    citation: collectiveEnfilade,
  }),
  // -- the joining/reinforcing batteries' attested fire -----------------------
  fireEvent({
    id: "us-btty-cooper-repulse-fire", kind: "artillery_fire",
    t0: 7200, t1: 9000, unitId: "us-btty-cooper", confidence: "documented",
    citation: "marker verbatim: '…opened fire upon a Confederate Battery in front. In half an hour a line of Confederate Infantry approached over the crest of the hill about 1000 yards distant. The Battery in connection with the Batteries in line fired case shot until the Confederates reached canister range a few charges of which compelled their retreat.' (Stone Sentinels Battery B 1st Pennsylvania Artillery page, Hancock Avenue marker, fetched 2026-07-08)",
    note: "opens on arrival at 3 p.m., counter-battery then case-then-canister; the marker's internal half-hour clock runs later than the track spine's climax (~15:23–15:30) — witness clock kept, window aligned to the spine",
  }),
  fireEvent({
    id: "us-btty-parsons-repulse-fire", kind: "artillery_fire",
    t0: 7380, t1: 9000, unitId: "us-btty-parsons", confidence: "documented",
    citation: "monument verbatim: 'Fired 120 rounds shrapnel at Pickett's column, and 80 shell at a battery in left front.' (Stone Sentinels New Jersey Battery A page, fetched 2026-07-08)",
    note: "opens after the documented 3 p.m. gallop-in; window to the climax's collapse inferred",
  }),
  fireEvent({
    id: "us-btty-fitzhugh-repulse-fire", kind: "artillery_fire",
    t0: 8280, t1: 9000, unitId: "us-btty-fitzhugh", confidence: "documented",
    citation: "monument rear verbatim: '…assisted in repulsing Pickett's Charge.' (Stone Sentinels 1st New York Battery K page, fetched 2026-07-08); page narrative: ordered into line at the gallop at the height of the charge, firing 89 rounds (57 percussion shell, 15 shrapnel, 17 time shell)",
    note: "opens on the inferred gallop-in arrival ~15:18",
  }),
  fireEvent({
    id: "us-btty-weir-canister", kind: "artillery_fire",
    t0: 8400, t1: 9000, unitId: "us-btty-weir", confidence: "documented",
    citation: "tablet verbatim: '…opened with canister at short range on the advancing Confederates.' (Stone Sentinels 5th US Artillery Battery C page, fetched 2026-07-08)",
    note: "canister window aligned with the climax phase (8400–9000), matching the modeled II Corps batteries",
  }),
  // -- the follow-up: Wilcox/Lang, coherent with the Vermont events -----------
  fireEvent({
    id: "us-btty-phillips-florida-repulse", kind: "artillery_fire",
    t0: 10200, t1: 10500, unitId: "us-btty-phillips", confidence: "documented",
    citation: `tablet verbatim: 'A charge was made within the range of the battery immediately afterwards by the Florida brigade and at about the same time a Confederate battery opened on the left front which at once received the concentrated fire of the batteries of the brigade driving the cannoneers from their guns which they abandoned.' (${phillipsTablet})`,
    note: "window = the Wilcox/Lang follow-up spine (us-14vt-wilcox-front / us-16vt-wilcox-flank, 10200–10500)",
  }),
  fireEvent({
    id: "us-btty-hart-second-line", kind: "artillery_fire",
    t0: 10200, t1: 10500, unitId: "us-btty-hart", confidence: "documented",
    citation: `marker verbatim: 'A second line advancing was met with double canister which dispersed it.' (${hartMarker})`,
    note: "the marker does not name the formation — window aligned with the Wilcox/Lang follow-up spine, matching Phillips's Florida-brigade attestation on the same ground",
  }),
  // -- the tail: Wheeler, tight against the window end ------------------------
  fireEvent({
    id: "us-btty-wheeler-canister", kind: "artillery_fire",
    t0: 10680, t1: 10800, unitId: "us-btty-wheeler", confidence: "documented",
    citation: "Wheeler's OR report verbatim: 'This gave me a fine opportunity to enfilade their column with canister, which threw them into great disorder, and brought them to a halt three times.' (transcribed at 13nybattery.com/battles/wheeler_gettys.htm, VERIFIED survey §2.2)",
    note: "his ~4 p.m. clock sits at the window's very end — encoded tight against it per the plan (t1 = endTime); the fight's post-16:00 remainder is out of slice",
  }),
];

// ---- assemble, gate, export -------------------------------------------------
// The 13 units land grouped after the existing battery block (Woodruff is the
// current last unit) — McGilvery's line extends the modeled gun line south.

const wave1: Unit[] = [
  ...lineBatteries.slice(0, 6), // Thomas … Sterling (north → south)
  cooper, //                       Cooper's line slot sits between Sterling and Dow
  ...lineBatteries.slice(6), //    Dow, Ames (the line's south end)
  fitzhugh, parsons, weir, wheeler, // the reinforcing batteries
];
battle = addUnitsAfter(battle, "us-btty-woodruff", wave1);
battle.events = [...(battle.events ?? []), ...events]; // exportBattle sorts canonically

const result = validateBattle(battle);
if (!result.ok) {
  console.error(result.errors.join("\n"));
  throw new Error("authored battle failed validation");
}
writeFileSync(battlePath, exportBattle(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events?.length ?? 0} events`);
