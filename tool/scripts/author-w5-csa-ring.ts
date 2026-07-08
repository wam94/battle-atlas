// Wave 5 authoring script (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 7): the Confederate infantry ring — 26 brigades, the CSA line stops
// being an artillery-only sketch. Survey basis:
// docs/research/2026-07-02-full-cast-sources.md §3.1 (Longstreet 8 / Hill 5 /
// Ewell 13), §4 items 10/11/13, §5.2 step 6. Mostly 2-keyframe statics with
// tablet citations and the documented McLaws/Hood/Ewell quiet riding every
// t=0 keyframe; the two attested in-window CSA movers get real tracks
// (Kershaw's ~13:00 extension right, Wright's ~15:30 advance-and-recall) and
// the three survey-named CSA fire emitters get events (Thomas & Perrin in
// Long Lane, Hays's town sharpshooters). Position classes from the survey §1
// landmark table (Big RT 4076,1869; Devil's Den 3808,2465; Peach Orchard
// 3208,3457; the Angle 4416,4827; town square 4864,6839; Benner's Hill
// 6619,6305; Culp's Hill 5796,5570; Spangler's Spring 6084,4752), the sheet-8
// sector crops (s8_roundtops.jpg — Benning Ga. / Robertson / Law Ala. in
// place; s8_town.jpg — Doles' Ga. / Iverson's N.C. in Long Lane SW of town;
// s8_peachorchard.jpg — Kershaw S.C.; s8_culps.jpg — the Johnson's Division
// label block EAST of Rock Creek: "Jones, Williams, Steuart, Walker's
// Brigades, Smith's, O'Neal's, Daniels Brigades / Johnson's Div."), and
// monument lat/lons off the tablet pages converted to battlefield-local
// meters this session (WGS84→UTM 18N − origin 304208/4404534; town-square
// sanity check landed (4868,6837) vs the survey's 4864,6839). All tablet
// text fetched live 2026-07-08 (exact-URL citation convention, Wave 2
// forward). Run from tool/:
//   npx vite-node scripts/author-w5-csa-ring.ts
// Committed as the derivation record for the authored data.
//
// GRAIN: brigades stay brigades (plan design decision 5). All 26 are
// PARENTLESS: divisions and corps are not modeled (family rules: none).
//
// ID CONVENTIONS & COLLISION DODGES (plan "Id conventions"): acting
// commanders get the survey's names — the survey §3.1 header list Sheffield,
// Bryan, Humphreys, Godwin, Dungan, Perrin, Luffman, plus Williams for
// Nicholls's (the Johnson division tablet rosters "Nicholl's Brigade Col.
// J. M. Williams"). This ALSO resolves every surname collision with the
// already-authored cast: Jones's VA brigade is `csa-dungan`, never
// `csa-jones` (which would shadow-collide with `csa-bn-jones`, H.P. Jones's
// artillery battalion — the Stone Sentinels page itself warns of the three
// Joneses); Anderson's GA brigade is `csa-luffman` (dodging R.H. Anderson's
// DIVISION and the `andersons-division` tablet namespace). No Wave-5 brigade
// shares a bare surname with the assault brigades (csa-garnett, csa-lane
// keep their tracks untouched).
//
// FIRE FOR THE RING — exactly the survey's three emitters, nothing else
// (survey §3.1: Thomas & Perrin are "the two attested in-window skirmish
// emitters of the CSA line", A−; §4 item 11 adds Early's house-sharpshooters
// as ONE collective low-intensity event on Hays). ATTACH-LEVEL CHECK against
// the existing corridor events, done before authoring: the corridor's
// musketry (us-smyth/us-sherrill/us-hall-children wall fire 8340–9300,
// us-8oh-flank-fire 7800–8700, the Doubleday pair 8100–9900) is the REPULSE
// fire of Union units — no existing event models CSA skirmish or
// sharpshooter fire anywhere on the field, and us-carroll's Wave-4 citation
// explicitly deferred "the house-sharpshooter fire the tablet complains of"
// to this wave. The three new events are therefore first-authored fire, not
// a second grain of existing fire. Everything else — McLaws's "with the
// exception of severe skirmishing the Division was not engaged" (division
// grain, no brigade window attested: rides the citations per the plan),
// Hood's all-day sharpshooter skirmishing at the Round Top breastworks
// (Wave 6 owns the South Cavalry Field exchange), Gordon's Winebrenner's Run
// skirmishing and Dungan's "sometimes skirmishing heavily" (the sector fire
// the survey renders on Hays / leaves unwindowed) — is presence without
// events, the documented-silence convention (battle-format.md).
//
// STRENGTHS: unlike the Union headquarters tablets (casualties only), the
// CSA tablets carry "Present" figures. Every strength below is the tablet's
// Present less its own battle losses — reconstructed July-3-afternoon
// effectives (losses ran overwhelmingly July 1 for Ewell/Rodes, July 2 for
// Longstreet/Anderson, the July 3 MORNING for Johnson's block — i.e. before
// this window), rounded, flagged per citation. Better grounded than Wave 4's
// norms-based reconstruction, still approximation-grade by design; the
// position class, not the headcount, is the claim.
//
// KERSHAW ADJUDICATION (the one Wave-5 geometry disagreement, carried not
// reconciled): the survey reads his one attested in-window move as "extended
// right to Hood's left at ~13:00" FROM the Peach Orchard; his tablet
// verbatim is "At 1 P. M. under orders resumed position here extending line
// to right and keeping in touch with Hood's Division on left" — "here" being
// the monument on the Warfield Ridge line (300 ft S of the observation
// tower; the neighboring Semmes monument converts to local 2650,3318).
// Geometry follows the tablet's monument anchor (t=0 forward at the Peach
// Orchard, t=600 on the ridge extended right); both readings ride the
// keyframe citations. SEMMES'S SAME CLOCK: his tablet also attests "At 1
// P. M. under orders it resumed its original position near here" — the move
// completes at/just after the window opens, sub-keyframe grain, so he is
// authored STATIC at the resumed position per the survey's B grade with the
// clock verbatim in his citation (said there, not silently dropped).
//
// DECLINED — O'Neal's 5th Alabama town detachment: his tablet attests "The
// 5th Regiment lay in the southern borders of the town firing upon the
// Union artillery with their long range rifles" — a documented detachment
// firing away from the brigade's Culp's Hill-sector position. NOT authored
// as a segment event: the survey's chosen grain for the town-sector fire is
// the single collective Hays event (§4 item 11), the detachment's exact
// position is unattested beyond "southern borders", and a second overlapping
// town emitter would double the survey's attested grain. The detachment
// rides O'Neal's citation as a flag. Also declined: Wright's post-recall
// "Afterward was moved to the right to meet a threatened attack" (unwindowed,
// straddles/post-16:00 — carried in his final keyframe citation, not
// modeled); Hood's/McLaws's evening withdrawals (post-window per the survey).
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

const survey = "docs/research/2026-07-02-full-cast-sources.md §3.1";
const fetched = "fetched 2026-07-08";
const chq = "https://gettysburg.stonesentinels.com/confederate-headquarters/";

// The strength-derivation flag every brigade carries (header note): the CSA
// tablets carry Present figures; effectives = Present − battle losses.
const strengthNote = (present: string, losses: string, n: number) =>
  `strength: tablet Present ${present} less its ${losses} battle losses → ~${n} reconstructed July-3-afternoon effectives (losses ran before this window except where said), approximation flagged`;

// ---- SECTOR: the southern flank — Longstreet: Hood's division (4) -----------
// Survey §3.1 "Longstreet (8)". Hood's four hold the July 2 gains from Big
// Round Top's west slope to Devil's Den — the division tablet's July 3 is a
// documented negative ("was not engaged") with two carried exceptions: the
// post-window Farnsworth charge (~17:00, out of slice) and the all-day
// breastworks skirmishing (Wave 6 owns the South Cavalry Field exchange).
// Sheet 8 s8_roundtops.jpg draws Benning Ga. / Robertson / Law Ala. in place.

const hoodTablet =
  "Hood's division tablet verbatim, July 3: 'Occupied the ground gained and with the exception of resisting a Cavalry charge and heavy skirmishing was not engaged.' (" +
  chq + "hoods-division/, " + fetched + ") — the cavalry charge (Farnsworth) is ~17:00, POST-window; the skirmishing is unwindowed division-grain and rides here, not an event (documented-silence convention, battle-format.md; " + survey + ")";

const hoodDiv: Unit[] = [
  staticUnit({
    id: "csa-sheffield",
    name: "Law's Brigade, Hood's Division (Col. James L. Sheffield)",
    side: "confederate", strength: 950, x: 3920, z: 1850, facing: 75,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Occupied the breastworks on west slope of Round Top. The 4th and 15th Regiments assisted at 5 P. M. in repulsing cavalry led by Brig. Gen. E. J. Farnsworth in Plum Run Valley.' (" +
      chq + "laws-brigade/, " + fetched + ") — the Farnsworth repulse is 5 P.M., POST-window: in 13:00–16:00 the brigade stands in the Big Round Top west-slope works (survey §1 Big RT 4076,1869); Col. James L. Sheffield commanding (Brig. Gen. Law up to division for the wounded Hood — acting-commander naming per the survey header); sheet 8 s8_roundtops.jpg names 'Law Ala.'; " +
      hoodTablet + "; " + strengthNote("about 1500", "about 550 (July 2 Little Round Top)", 950),
  }),
  staticUnit({
    id: "csa-robertson",
    name: "Robertson's Texas Brigade, Hood's Division",
    side: "confederate", strength: 560, x: 3950, z: 2130, facing: 60,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'At 2 A. M. the 1st Texas and 3d Arkansas were moved to the right and joined the 4th and 5th Texas on the northwest spur of Big Round Top. Three regiments occupied the breastworks there all day skirmishing hotly with Union sharpshooters. Early in the day the 1st Texas was sent to confront the Union Cavalry threatening the right flank.' (" +
      chq + "robertsons-brigade/, " + fetched + ") — the all-day skirmishing is unwindowed sharpshooter grain, no event (the South Cavalry Field exchange is Wave 6's); the 1st Texas detachment southward is the survey's SCF shift, decided at Wave 6 per plan Task 8, strength kept whole here; COMMAND DISPUTE CARRIED, not reconciled (survey ⚠️): Brig. Gen. J. B. Robertson (wounded July 2 per his page) vs Lt. Col. P. A. Work — both readings kept; sheet 8 s8_roundtops.jpg names 'Robertson'; " +
      hoodTablet + "; " + strengthNote("about 1100", "about 540 (July 2 Devil's Den/LRT)", 560),
  }),
  staticUnit({
    id: "csa-benning",
    name: "Benning's Brigade, Hood's Division",
    side: "confederate", strength: 990, x: 3790, z: 2480, facing: 70,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Held Devil's Den and the adjacent crest of rocky ridge until late in the evening when under orders the Brigade retired to position near here. Through mistake of orders the 15th Georgia did not retire directly but moved northward encountered a superior Union force and suffered considerable loss.' (" +
      chq + "bennings-brigade/, " + fetched + ") — the withdrawal and the 15th Georgia fight are LATE EVENING, post-window: in-slice the brigade holds Devil's Den (survey §1: 3808,2465); sheet 8 s8_roundtops.jpg names 'Benning Ga.'; " +
      hoodTablet + "; " + strengthNote("about 1500", "509 (July 2 + the post-window 15th GA fight — in-window effectives ran somewhat higher)", 990),
  }),
  staticUnit({
    id: "csa-luffman",
    name: "Anderson's Georgia Brigade, Hood's Division (Lt. Col. William Luffman)",
    side: "confederate", strength: 1130, x: 3320, z: 2880, facing: 175,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'The Brigade was sent down Emmitsburg Road and assisted in repulsing and holding in check Union cavalry which sought to flank the division' (" +
      chq + "anderson/, " + fetched + ") — the southward move's clock is UNATTESTED (Merritt's arrival spread 11:00/13:00/15:00, survey §3.4): authored static at the survey's Rose farm position class facing the flank, the elements-shifted-south note carried per plan Task 7 ('elements shifted S vs Merritt ~15:00+ — B+'); Lt. Col. William Luffman commanding (Brig. Gen. G. T. Anderson wounded July 2 — the division tablet rosters both; acting-commander naming per the survey header); " +
      hoodTablet + "; " + strengthNote("about 1800", "671 (July 2 Wheatfield + the flank affair)", 1130),
  }),
];

// ---- SECTOR: Peach Orchard–Warfield Ridge — Longstreet: McLaws's division (4)
// Survey §3.1: Kershaw carries the wave's first attested in-window move
// (~13:00, B+ — the KERSHAW ADJUDICATION in the header); Semmes/Bryan,
// Barksdale/Humphreys, Wofford stand under the McLaws division negative.
// Sheet 8 s8_peachorchard.jpg names 'Kershaw S.C.'. Monument anchors
// converted this session: Semmes (2650,3318), Wofford (2652,3803) — the
// Warfield Ridge line; Barksdale's Emmitsburg Rd marker (3145,3611).

const mclawsTablet =
  "McLaws's division tablet verbatim, July 3: 'With the exception of severe skirmishing the Division was not engaged and after night disposition were made to withdraw.' (" +
  chq + "mclaws-division/, " + fetched + ") — the skirmishing is unwindowed division-grain, no brigade window attested: it rides the citations, never an event (plan Task 7; documented-silence convention); the withdrawal is after night, post-window (" + survey + ")";

const kershawTablet =
  "brigade tablet verbatim, July 3: 'At Peach Orchard until noon then sent farther to front. At 1 P. M. under orders resumed position here extending line to right and keeping in touch with Hood's Division on left.' (" +
  chq + "kershaws-brigade/, " + fetched + ")";

const kershaw = moverUnit({
  id: "csa-kershaw",
  name: "Kershaw's Brigade, McLaws's Division",
  side: "confederate",
  keyframes: [
    { t: 0, x: 3330, z: 3390, facing: 95, formation: "line", strength: 1170, confidence: "documented",
      citation: kershawTablet + " — at t=0 the brigade stands forward of the Peach Orchard ('sent farther to front', survey §1 Peach Orchard 3208,3457), the 1 P.M. return order just issued; coordinate inferred within the attested position class; sheet 8 s8_peachorchard.jpg names 'Kershaw S.C.'; " + mclawsTablet + "; " + strengthNote("about 1800", "630 (July 2 Wheatfield/Loop)", 1170) },
    { t: 600, x: 2680, z: 3190, facing: 110, formation: "line", strength: 1170, confidence: "documented",
      citation: kershawTablet + " — arrival: the tablet's 'resumed position here' is the Warfield Ridge monument line (300 ft S of the observation tower; neighboring Semmes monument converts to local 2650,3318), 'extending line to right and keeping in touch with Hood's Division' — READING SPREAD CARRIED, not reconciled: the survey reads the move as 'extended right to Hood's left at ~13:00' from the Peach Orchard; geometry here follows the tablet's monument anchor, t≈600 per plan Task 7 (a ~10-minute march); " + mclawsTablet },
    { t: WindowEndT, x: 2680, z: 3190, facing: 110, formation: "line", strength: 1170, confidence: "documented",
      citation: kershawTablet + " — holds the extended ridge line to the window end; the division's withdrawal is after night, post-window" },
  ],
});

const mclawsDiv: Unit[] = [
  staticUnit({
    id: "csa-bryan",
    name: "Semmes's Brigade, McLaws's Division (Col. Goode Bryan)",
    side: "confederate", strength: 770, x: 2650, z: 3335, facing: 90,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'During the afternoon Anderson's Brigade being withdrawn for duty elsewhere the Brigade was left in the occupancy of the woodland south of the Wheatfield. At 1 P. M. under orders it resumed its original position near here.' (" +
      chq + "semmes-brigade/, " + fetched + ") — the 1 P.M. clock completes at/just after the window opens: authored static at the resumed Warfield Ridge position (monument converts to local 2650,3318), the move sub-keyframe grain and said here per the survey's B-static grade (header adjudication); Col. Goode Bryan commanding (Brig. Gen. Semmes mortally wounded July 2 — acting-commander naming per the survey header; the division tablet prints 'Col. George Bryan', both spellings carried); " +
      mclawsTablet + "; " + strengthNote("about 1200", "430 (July 2 Rose Hill/Loop)", 770),
  }),
  staticUnit({
    id: "csa-humphreys",
    name: "Barksdale's Brigade, McLaws's Division (Col. B. G. Humphreys)",
    side: "confederate", strength: 850, x: 3080, z: 3560, facing: 85,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Supported artIllery on Peach Orchard Ridge. Withdrew from the front late in the afternoon.' [sic, tablet-page spelling] (" +
      chq + "barksdales-brigade/, " + fetched + ") — position class: Peach Orchard Ridge behind Eshleman's and Cabell's gun line (the battalions this brigade supports are the Wave-3 units csa-bn-eshleman/csa-bn-cabell); the 'late in the afternoon' withdrawal straddles/post-dates 16:00 per the survey (McLaws's withdrawal post-window) — left static, flagged; Col. Benjamin G. Humphreys commanding (Brig. Gen. Barksdale mortally wounded July 2 — acting-commander naming per the survey header); " +
      mclawsTablet + "; " + strengthNote("1598", "747 (July 2 Peach Orchard/Plum Run)", 850),
  }),
  staticUnit({
    id: "csa-wofford",
    name: "Wofford's Brigade, McLaws's Division",
    side: "confederate", strength: 995, x: 3060, z: 3240, facing: 90,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'One regiment was left on outpost duty in that grove. The others supported artillery on Peach Orchard Ridge. All withdrew late in the afternoon.' (" +
      chq + "woffords-brigade/, " + fetched + ") — position class: Peach Orchard Ridge south of the Wheatfield Road gun line; the outpost regiment (grove west of the Wheatfield) is an unattested-position detachment, strength kept whole; the 'late in the afternoon' withdrawal straddles/post-dates 16:00 — left static, flagged (the survey has McLaws's withdrawal post-window); " +
      mclawsTablet + "; " + strengthNote("about 1350", "355 (July 2 Wheatfield)", 995),
  }),
];

// ---- SECTOR: Seminary Ridge — Hill: Anderson's division (3) + Pender's
//      Long Lane pair (2) ----------------------------------------------------
// Survey §3.1 "Hill (5)": Wright is THE CSA attested in-window mover — the
// advance-and-recall (t=9000→10500, B+; the Anderson division tablet's
// '3.30 P. M.' converts to t=9000 exactly); Posey & Mahone hold the trench
// line behind the crest; Thomas & Perrin in Long Lane are the CSA line's two
// attested in-window skirmish emitters (A−). Sheet 8 s8_semridge_n.jpg names
// McGowan S.C. / Thomas Ga. on the ridge sector.

const andersonDivTablet =
  "Anderson's division tablet verbatim, July 3: 'The Division remained in position until 3.30 P. M. Orders were given to support Lieut. Gen. Longstreet's attack on the Union centre Wilcox and Perry moved forward. The assault failed the order to advance was countermanded.' (" +
  chq + "andersons-division/, " + fetched + ")";

const wright = moverUnit({
  id: "csa-wright",
  name: "Wright's Brigade, Anderson's Division",
  side: "confederate",
  keyframes: [
    { t: 0, x: 2960, z: 5060, facing: 95, formation: "line", strength: 580, confidence: "documented",
      citation: andersonDivTablet + " — at t=0 in the division line on Seminary Ridge in close support behind the assault sector (position class: the July 2 formation ground at the brigade monument just south of the Virginia Monument, coordinate inferred within it, west of Armistead's step-off ground); the division tablet rosters 'Wright's Brigade Bde. Gen. A. R. Wright / Col. William Gibson' — command passed during the July 2 fight, both carried; " + strengthNote("1450", "873 (July 2 — the deepest brigade loss on this ridge)", 580) },
    { t: 9000, x: 2960, z: 5060, facing: 95, formation: "line", strength: 580, confidence: "documented",
      citation: andersonDivTablet + " — departure: 'remained in position until 3.30 P. M.' = t 9000 exactly; the advance order comes as the assault collapses (" + survey + "; plan Task 7 t=9000)" },
    { t: 9660, x: 3509, z: 5020, facing: 95, formation: "line", strength: 580, confidence: "documented",
      citation: "brigade tablet verbatim, July 3: 'Advanced 600 yards to cover the retreat of Pickett's Division.' (" + chq + "wrights-brigade/, " + fetched + ") — forward position ~550 m east of the ridge line covering the stragglers streaming back from the repulse (the corridor collapse spine 8700–9300); bearing and clock of the advance inferred within the attested 600 yards (~11 minutes at quick step)" },
    { t: 10500, x: 2960, z: 5060, facing: 95, formation: "line", strength: 580, confidence: "documented",
      citation: andersonDivTablet + " — recall: 'The assault failed the order to advance was countermanded' — back on the ridge by ~16:00 (survey 'brief forward move ~15:30–16:00, recalled', plan t=10500); the brigade tablet's 'Afterward was moved to the right to meet a threatened attack' is UNWINDOWED and straddles/post-dates 16:00 — carried here as a flag, not modeled (header Declined)" },
    { t: WindowEndT, x: 2960, z: 5060, facing: 95, formation: "line", strength: 580, confidence: "documented",
      citation: andersonDivTablet + " — holds the ridge line to the window end" },
  ],
});

const andersonDiv: Unit[] = [
  staticUnit({
    id: "csa-posey",
    name: "Posey's Brigade, Anderson's Division",
    side: "confederate", strength: 1065, x: 2950, z: 5250, facing: 95,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Was held in reserve here supporting artillery in its front.' (" +
      chq + "poseys-brigade/, " + fetched + ") — a documented reserve posture: the trench line behind the Seminary Ridge crest (survey 'trench line behind the crest, McMillan Woods sector'), the Wave-3 gun line (csa-bn-lane, csa-bn-poague) in its front; no fire attested, presence without events; " +
      andersonDivTablet + "; " + strengthNote("1150", "83 (the lightest loss on the ridge)", 1065),
  }),
  staticUnit({
    id: "csa-mahone",
    name: "Mahone's Brigade, Anderson's Division",
    side: "confederate", strength: 1400, x: 2960, z: 5480, facing: 95,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Remained here in support of the artillery. Took no active part in the battle except by skirmishers.' (" +
      chq + "mahones-brigade/, " + fetched + ") — a documented negative: the trench line behind the crest north of Posey; the skirmisher exception is picket grain, below the event model's attested threshold — no event, the negative rides here (documented-silence convention); " +
      andersonDivTablet + "; " + strengthNote("1500", "102", 1400),
  }),
];

// Long Lane — the sunken lane from the town's southwest edge toward the
// Bliss house/barn ground; five brigades hold it through the window, facing
// Cemetery Hill. Perrin's tablet fixes the order ('took position on
// Ramseur's right in the Long Lane leading from the town to the Bliss House
// and Barn'), Thomas's fixes the SW end ('left flank in touch with
// McGowan's Brigade and the right near the Bliss House and Barn'): from SW
// to NE — Thomas, Perrin, Ramseur, Iverson, Doles. Sheet 8 s8_town.jpg
// draws Doles' Ga. / Iverson's N.C. in Long Lane SW of town.

const longLanePair: Unit[] = [
  staticUnit({
    id: "csa-thomas",
    name: "Thomas's Brigade, Pender's Division",
    side: "confederate", strength: 930, x: 4090, z: 5700, facing: 140,
    grade: "A-",
    citation:
      "brigade tablet verbatim, July 3: 'Engaged most of the day in severe skirmishing and exposed to a heavy fire of artillery. After dark retired to this Ridge.' — in Long Lane since 10 P.M. July 2, 'left flank in touch with McGowan's Brigade and the right near the Bliss House and Barn' (" +
      chq + "thomas-brigade/, " + fetched + ") — the SW end of the lane, right toward the Bliss ground; one of the survey's two attested in-window CSA skirmish emitters (A−, " + survey + "; §4 item 11) — his fire is the event csa-thomas-long-lane-skirmish; the after-dark retirement is post-window; " +
      strengthNote("about 1200", "270 (July 1 + the lane's skirmishing)", 930),
  }),
  staticUnit({
    id: "csa-perrin",
    name: "Perrin's Brigade (McGowan's), Pender's Division",
    side: "confederate", strength: 1025, x: 4210, z: 5880, facing: 140,
    grade: "A-",
    citation:
      "brigade tablet verbatim, July 3: 'In the same position and constantly engaged in skirmishing.' — the position being July 2's 10 P.M. posting 'on Ramseur's right in the Long Lane leading from the town to the Bliss House and Barn' (" +
      chq + "perrins-brigade/, " + fetched + ") — Col. Abner Perrin commanding for the wounded Brig. Gen. McGowan (acting-commander naming per the survey header; the page notes the brigade 'is often (and properly) called McGowan's Brigade'); the second of the survey's two attested in-window CSA skirmish emitters (A−) — his fire is the event csa-perrin-long-lane-skirmish; " +
      strengthNote("about 1600", "577 (overwhelmingly July 1)", 1025),
  }),
];

// ---- SECTOR: east of Rock Creek — Ewell: Johnson's seven-brigade block (7) --
// Survey §3.1: the wave's visual payoff — "Retired at 10:30 A.M. to former
// position of July 2" at the base of Benner's Hill east of Rock Creek, "held
// until 10 P.M."; sheet 8 s8_culps.jpg draws exactly this block ("Jones,
// Williams, Steuart, Walker's Brigades, Smith's, O'Neal's, Daniels Brigades
// / Johnson's Div."). Culp's Hill stands manned by Wave 4's XII Corps and
// SILENT on both sides — presence without events on the whole sector.

const johnsonTablet =
  "Johnson's division tablet verbatim, July 3: 'The assault was renewed in early morning. An attempt was made by the Union forces to retake the works occupied the night before and was repulsed. The Division being reinforced by four brigades two other assaults were made and repulsed. Retired at 10.30 A. M. to former position of July 2 which was held until 10 P. M. when the Division was withdrawn to the ridge northwest of town.' (" +
  chq + "johnsons-division/, " + fetched + ")";
const johnsonSilence =
  "DOCUMENTED SILENCE (battle-format.md): the Culp's Hill fight ended 10:30 A.M. when the division retired EAST of Rock Creek — in 13:00–16:00 the block stands at the Benner's Hill base in the open, spent and quiet, exactly where sheet 8 s8_culps.jpg draws it; presence without events IS the encoding (" +
  survey + "); HOLD-UNTIL SPREAD CARRIED, not reconciled: division tablet 'held until 10 P. M.', the brigade tablets 'about midnight moved… to Seminary Ridge', Wikipedia's Culp's Hill 'until midnight' — all post-window";

const johnsonBlock: Unit[] = [
  staticUnit({
    id: "csa-steuart",
    name: "Steuart's Brigade, Johnson's Division",
    side: "confederate", strength: 1020, x: 6420, z: 5150, facing: 255,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'The Union troops reinforced the conflict at dawn and it raged fiercely until 11 A. M. when this Brigade and the entire line fell back to the base of the hill and from thence moved about midnight to Seminary Ridge northwest of the town.' (" +
      chq + "steuarts-brigade/, " + fetched + ") — the division's left, southern end of the block (clock spread 10:30/11 A.M. carried, both pre-window); " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("about 1700", "682 (the July 2–3 Culp's Hill fight)", 1020),
  }),
  staticUnit({
    id: "csa-daniel",
    name: "Daniel's Brigade, Rodes's Division (attached to Johnson)",
    side: "confederate", strength: 1185, x: 6490, z: 5360, facing: 255,
    grade: "B+",
    citation:
      "East Confederate Avenue brigade tablet verbatim, July 3: 'the Brigade marched about 1.30 A. M. from its position in the town to Culp's Hill to reinforce Johnson's Division. Arriving about 4 A. M. it fought at different points wherever ordered through the long and fierce conflict its main position being in the ravine between the two summits of Culp's Hill. At the close of the struggle near noon it was withdrawn by Gen. Johnson with the rest of the line to the base of the hill from whence it moved during the night to Seminary Ridge' (" +
      chq + "daniels-brigade/, " + fetched + ") — one of the division tablet's four reinforcing brigades, in the block through the window; " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("2100", "916 (July 1 + the Culp's Hill morning)", 1185),
  }),
  staticUnit({
    id: "csa-oneal",
    name: "O'Neal's Brigade, Rodes's Division (attached to Johnson)",
    side: "confederate", strength: 1100, x: 6550, z: 5570, facing: 255,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'The 5th Regiment lay in the southern borders of the town firing upon the Union artillery with their long range rifles. The other regiments moved to Culp's Hill to reinforce Johnson's Division.' (" +
      chq + "oneals-brigade/, " + fetched + "); Rodes's division tablet: 'The Brigades of Daniel and O'Neal were ordered to report to Gen. E. Johnson on the left early in the morning and joined in the attack on Culp's Hill.' (" + chq + "rodes-division/, " + fetched + ") — DETACHMENT FLAG, not modeled (header Declined): the 5th Alabama's town sharpshooting is attested but its position class is 'southern borders of the town' only, and the survey's chosen grain for the town-sector fire is the single collective Hays event — the detachment rides here, strength kept whole; the brigade body stands in Johnson's block after the 10:30 retirement; " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("1794", "696 (overwhelmingly July 1)", 1100),
  }),
  staticUnit({
    id: "csa-williams",
    name: "Nicholls's Brigade, Johnson's Division (Col. J. M. Williams)",
    side: "confederate", strength: 710, x: 6610, z: 5780, facing: 255,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'At dawn the Brigade reopened fire and continued it for many hours then retired to line near the creek whence about midnight it moved with Division and Corps to Seminary Ridge.' (" +
      chq + "nicholls-brigade/, " + fetched + ") — 'retired to line near the creek': the block's centre; Col. Jesse M. Williams commanding (Brig. Gen. Nicholls absent wounded since Chancellorsville — the division tablet rosters 'Nicholl's Brigade Col. J. M. Williams'; acting-commander naming per the survey); " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("about 1100", "388 (the Culp's Hill fight)", 710),
  }),
  staticUnit({
    id: "csa-walker",
    name: "Stonewall Brigade, Johnson's Division (Brig. Gen. James A. Walker)",
    side: "confederate", strength: 1120, x: 6670, z: 5990, facing: 255,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'Took part in the unsuccessful struggle lasting from daybreak until near noon and then retired to the foot of the hill and from thence about midnight moved with the Division and Corps to Seminary Ridge.' (" +
      chq + "walkers-brigade/, " + fetched + ") — 'near noon' vs the division's 10:30 A.M., clock spread carried, both pre-window; " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("about 1450", "330 (July 2–3 Culp's Hill)", 1120),
  }),
  staticUnit({
    id: "csa-dungan",
    name: "Jones's Brigade, Johnson's Division (Lt. Col. R. H. Dungan)",
    side: "confederate", strength: 1180, x: 6760, z: 6090, facing: 250,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'In line near here all day sometimes skirmishing heavily. About midnight moved with the Division and Corps to Seminary Ridge northwest of the town.' (" +
      chq + "jones-brigade/, " + fetched + ") — the brigade (wrecked July 2, Brig. Gen. John M. Jones badly wounded) held near the creek line through the day; the 'sometimes skirmishing heavily' is unwindowed and below the survey's attested-emitter grain — no event, it rides here (documented-silence convention); Lt. Col. Robert H. Dungan commanding (the division tablet spells 'Duncan' — both carried; acting-commander naming per the survey header; id csa-dungan dodges the csa-bn-jones surname collision, header note); position class: the block's right toward the Hanover Road, west face of Benner's Hill base; " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("1600", "421 (July 2 Culp's Hill, where Gen. Jones fell)", 1180),
  }),
  staticUnit({
    id: "csa-smith",
    name: "Smith's Brigade, Early's Division (attached to Johnson)",
    side: "confederate", strength: 660, x: 6800, z: 6420, facing: 250,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'The Brigade having been detached two days guarding York Pike and other roads against the reported approach of Union Cavalry was ordered to Culp's Hill to reinforce Johnson's Division. Arriving early formed in line along this stone wall receiving and returning fire of Infantry and sharpshooters in the woods opposite and being subjected to heavy fire of Artillery. It repulsed the charge of the 2nd Massachusetts and 27th Indiana Regiments against this line and held its ground until the Union forces regained their works on the hill. It then moved to a position further up the creek' (" +
      chq + "smiths-brigade/, " + fetched + ") — the Spangler's meadow repulse (2nd MA/27th IN) is ~10 A.M., PRE-window (Wave 4's us-colgrove carries the Union side); in-slice the brigade stands 'further up the creek' at the block's northern end near the Hanover Road; Early's division tablet: 'At daylight Smith's Brigade was ordered to support of Johnson's Division on the left.' (" + chq + "earlys-division/, " + fetched + "); " +
      johnsonTablet + "; " + johnsonSilence + "; " + strengthNote("about 800", "142", 660),
  }),
];

// ---- SECTOR: the town — Ewell: Early's division (3) -------------------------
// Survey §3.1: Hays and Hoke/Godwin in the southern streets facing Cemetery
// Hill, Gordon at Winebrenner's Run — 'The Division not further engaged';
// house-sharpshooting all window is the sector's one collective fire,
// authored as ONE low-intensity event on Hays (§4 item 11; the fire Wave 4's
// us-carroll citation explicitly deferred here).

const earlyTablet =
  "Early's division tablet verbatim, July 3: 'At daylight Smith's Brigade was ordered to support of Johnson's Division on the left. Hays' and Hoke's Brigades formed line in town holding the position of previous day. Gordon's Brigade held the line of the day before. The Division not further engaged.' (" +
  chq + "earlys-division/, " + fetched + ")";

const earlyDiv: Unit[] = [
  staticUnit({
    id: "csa-hays",
    name: "Hays's Brigade, Early's Division",
    side: "confederate", strength: 870, x: 4900, z: 6620, facing: 175,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Occupied a position on High St. in town.' (" +
      chq + "hays-brigade/, " + fetched + ") — the southern streets facing Cemetery Hill; the division's 'not further engaged' negative is carried WITH the sector's documented exception: the all-window house-sharpshooting Wave 4's us-carroll tablet complains of ('an annoying sharpshooters fire from the houses in the town'), authored as the single collective event csa-hays-town-sharpshooting per the survey's grain (§4 item 11); " +
      earlyTablet + "; " + strengthNote("about 1200", "332 (July 1–2, incl. the East Cemetery Hill assault)", 870),
  }),
  staticUnit({
    id: "csa-godwin",
    name: "Hoke's Brigade, Early's Division (Col. A. C. Godwin)",
    side: "confederate", strength: 555, x: 5060, z: 6630, facing: 180,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Ordered to railroad cut in rear and later to High Street in town.' (" +
      chq + "hokes-brigade/, " + fetched + ") — the shift's clock is unattested; authored at the High Street line beside Hays per the division tablet ('Hays' and Hoke's Brigades formed line in town holding the position of previous day'), the railroad-cut episode riding here; Col. Archibald C. Godwin commanding (Col. Isaac E. Avery mortally wounded July 2 — the division tablet rosters both; acting-commander naming per the survey header); the sector's sharpshooting fire is authored ONCE, on Hays (never the same fire at two levels — sibling share said there); " +
      earlyTablet + "; " + strengthNote("about 900", "345 (July 1–2)", 555),
  }),
  staticUnit({
    id: "csa-gordon",
    name: "Gordon's Brigade, Early's Division",
    side: "confederate", strength: 1120, x: 5230, z: 6470, facing: 195,
    grade: "B",
    citation:
      "brigade tablet verbatim, July 3: 'Remained here skirmishing with sharpshooters and exposed to artillery fire.' (" +
      chq + "gordons-brigade/, " + fetched + ") — Winebrenner's Run ground between the town and Rock Creek, held from July 2 evening; his skirmishing is part of the town-sector exchange the survey renders as the ONE collective Hays event — no second emitter authored (attach-level discipline), the attestation rides here; " +
      earlyTablet + "; " + strengthNote("about 1500", "380 (July 1 + supporting the July 2 assault)", 1120),
  }),
];

// ---- SECTOR: Long Lane, the Rodes three (3) ---------------------------------
// Survey §3.1: Doles, Iverson, Ramseur 'remained through the third day' in
// Long Lane under the cannonade's Union counter-fire — B+ statics NE of the
// Pender pair (Perrin's tablet: he stands on Ramseur's right).

const rodesTablet =
  "Rodes's division tablet verbatim, July 3: 'The remainder of the Division held the position of day before and at night retired to Seminary Ridge.' (" +
  chq + "rodes-division/, " + fetched + ")";

const rodesLane: Unit[] = [
  staticUnit({
    id: "csa-ramseur",
    name: "Ramseur's Brigade, Rodes's Division",
    side: "confederate", strength: 1715, x: 4330, z: 6060, facing: 140,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'In sunken lane southwest of town.' (" +
      chq + "ramseurs-brigade/, " + fetched + ") — Perrin's tablet fixes the join ('took position on Ramseur's right in the Long Lane'): Ramseur is the lane's centre, Perrin on his right; Iverson's July 1 remnants serve with this brigade (Iverson's tablet) — counted there, not here; no fire attested for the Rodes three, presence without events under the cannonade; " +
      rodesTablet + "; " + strengthNote("1909", "196 (July 1)", 1715),
  }),
  staticUnit({
    id: "csa-iverson",
    name: "Iverson's Brigade, Rodes's Division",
    side: "confederate", strength: 650, x: 4450, z: 6240, facing: 140,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'With other brigades in the sunken road southwest of town. At night withdrew to Seminary Ridge.' (" +
      chq + "iversons-brigade/, " + fetched + ") — the July 1 wreck stands in the lane between Ramseur and Doles: 'a vigorous assault by Union forces in front and on left flank almost annihilated three regiments… the remnants of the others joined Ramseur's Brigade and served with it throughout the battle' — the unit here is the surviving body (the sheltered 12th NC + remnants), strength approximation-flagged accordingly; sheet 8 s8_town.jpg names 'Iverson's N.C.' in Long Lane; " +
      rodesTablet + "; " + strengthNote("1470", "820 (July 1, three regiments nearly annihilated)", 650),
  }),
  staticUnit({
    id: "csa-doles",
    name: "Doles's Brigade, Rodes's Division",
    side: "confederate", strength: 1130, x: 4570, z: 6420, facing: 140,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'In line with other brigades in the sunken road southwest of town.' (" +
      chq + "doles-brigade/, " + fetched + ") — the lane's NE end toward the town edge; the page's summary: 'The brigade was not engaged for the rest of the battle, occupying a quiet sector of the Confederate line southwest of Gettysburg' — a documented negative; sheet 8 s8_town.jpg names 'Doles' Ga.' in Long Lane; " +
      rodesTablet + "; " + strengthNote("1369", "241 (July 1)", 1130),
  }),
];

// ---- events: the survey's THREE CSA emitters, nothing else ------------------
// Thomas & Perrin: one musketry window each spanning the slice (the tablets'
// 'most of the day' / 'constantly engaged' — the plan's attested grain);
// Hays: the collective town-sector sharpshooting, low intensity, all window.
// Attach-level check against the existing corridor events is in the header.

const events: EngagementEvent[] = [
  fireEvent({
    id: "csa-thomas-long-lane-skirmish", kind: "musketry",
    t0: 0, t1: WindowEndT, unitId: "csa-thomas", confidence: "documented",
    citation:
      "brigade tablet verbatim: 'Engaged most of the day in severe skirmishing and exposed to a heavy fire of artillery.' (" +
      chq + "thomas-brigade/, " + fetched + "; " + survey + " — A−, one of 'the two attested in-window skirmish emitters of the CSA line')",
    note:
      "window spans the slice per the tablet's 'most of the day' (plan Task 7: 'one musketry event each spanning the window'); skirmish grain, low intensity — the Bliss-ground picket exchange opposite the II Corps line, not a volley line; no existing event models this fire (the corridor musketry is the Union repulse fire, 7800+)",
  }),
  fireEvent({
    id: "csa-perrin-long-lane-skirmish", kind: "musketry",
    t0: 0, t1: WindowEndT, unitId: "csa-perrin", confidence: "documented",
    citation:
      "brigade tablet verbatim: 'In the same position and constantly engaged in skirmishing.' (" +
      chq + "perrins-brigade/, " + fetched + "; " + survey + " — A−, the second attested in-window skirmish emitter)",
    note:
      "window spans the slice per the tablet's 'constantly engaged' (plan Task 7); skirmish grain, low intensity, as csa-thomas-long-lane-skirmish",
  }),
  fireEvent({
    id: "csa-hays-town-sharpshooting", kind: "musketry",
    t0: 0, t1: WindowEndT, unitId: "csa-hays", confidence: "documented",
    citation:
      "collective attestation, both sides (survey §4 item 11 'B/documented-collective'): Hays's tablet places the brigade 'on High St. in town' facing Cemetery Hill (" +
      chq + "hays-brigade/, " + fetched + "); the fire itself is documented by its TARGETS — Wave 4's us-carroll tablet ('the Brigade was subjected to an annoying sharpshooters fire from the houses in the town', https://gettysburg.stonesentinels.com/union-headquarters/1st-brigade-3rd-division-2nd-corps/) and the XI Corps division tablets' sharpshooter negatives",
    note:
      "the town-sector house-sharpshooting, all window, LOW intensity (sharpshooter grain, not a musketry line); authored ONCE on Hays per the survey's grain — Godwin's and Gordon's shares ride their citations, never a second event (attach-level rule); Wave 4 deferred this fire here by name in us-carroll's citation",
  }),
];

// ---- assemble, gate, export -------------------------------------------------
// The 26 land grouped per-sector after the CSA artillery battalions
// (csa-bn-raine is the CSA block's last unit) — Longstreet south to north,
// then Hill's ridge, then Ewell's arc east and north, matching the survey's
// walk. Pin 158 → 184.

const wave5: Unit[] = [
  ...hoodDiv, //                       Hood: Big RT west slope to Rose farm
  kershaw, ...mclawsDiv, //            McLaws: Peach Orchard–Warfield Ridge
  wright, ...andersonDiv, //           Anderson: Seminary Ridge line
  ...longLanePair, //                  Pender's two in Long Lane (the emitters)
  ...johnsonBlock, //                  Johnson's seven east of Rock Creek
  ...earlyDiv, //                      Early's three at the town
  ...rodesLane, //                     Rodes's three in Long Lane
];
battle = addUnitsAfter(battle, "csa-bn-raine", wave5);
battle.events = [...(battle.events ?? []), ...events]; // exportValidated sorts canonically

writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events?.length ?? 0} events (wave 5: ${wave5.length} brigades, ${events.length} musketry)`);
