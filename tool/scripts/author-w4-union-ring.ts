// Wave 4 authoring script (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 6): the Union infantry ring — 45 brigades, the fishhook stops being
// empty. Survey basis: docs/research/2026-07-02-full-cast-sources.md §3.3
// (corps-by-corps), §4 items 7/8/13, §5.2 steps 4/5. High-count, low-cost:
// mostly 2-keyframe statics with tablet citations; the three attested
// in-window infantry movers (Robinson's Coulter & Baxter at 3 P.M., Shaler
// ~3:30) get real tracks. Position classes from the survey §1 landmark table
// (Culp's Hill summit 5796,5570; Spangler's Spring 6084,4752; Cemetery Hill
// 4993,5656; East Cemetery Hill 5107,5753; the Angle 4416,4827; Ziegler's
// Grove 4509,5146; Weikert 4112,3646; Little RT 4296,2441; Big RT 4076,1869;
// Wolf Hill 7287,5622) and the sheet-8 sector crops (s8_culps.jpg — Greene /
// Candy / Lockwood / McDougall / Colgrove / Shaler / Neill / Wadsworth named
// in place; s8_cemhill.jpg — Steinwehr / Schurz / von Gilsa / Carroll;
// s8_roundtops.jpg — Weed's (Garrard) / Tilton / Fisher / Day / Ayres /
// Grant at J. Weikert; s8_mcgilvery.jpg — Caldwell's bars, III Corps block,
// Torbert / Nevin / Vincent (Rice)). Tablet text fetched live 2026-07-08
// (exact-URL citation convention, Wave 2 forward). Run from tool/:
//   npx vite-node scripts/author-w4-union-ring.ts
// Committed as the derivation record for the authored data.
//
// GRAIN: brigades stay brigades — the survey's attested grain, NO regiment
// decomposition outside the corridor (plan design decision 5). All 45 are
// PARENTLESS: divisions and corps are not modeled (family rules: none).
// The already-modeled 8th Ohio stays a free-standing unit; Carroll's brigade
// is authored MINUS it (strength excludes the detachment, said in the
// citation) — no parent link is created.
//
// NO MUSKETRY FOR THE RING except where the survey attests it (plan Task 6):
// the existing corridor events stand; the ring's infantry fire in-window is
// unattested except THREE places the survey names — Doubleday's two brigades
// (division/brigade tablets: "assisted in repulsing Longstreet's assault…
// many prisoners and three stand of colors") and Neill's Rock Creek skirmish
// ("encountered and checked the advancing Confederate sharpshooters and
// skirmishers"). Everything else — Culp's Hill after 10:30, the XI Corps
// hill, Robinson's arriving brigades ("musketry not attested — no event,
// flag in citation"), Carroll under the town sharpshooters — is presence
// without events, the documented-silence convention (battle-format.md).
//
// STRENGTHS: no Stone Sentinels headquarters tablet page carries a strength
// figure (verified across every division/brigade page fetched this session —
// they carry casualties only). Every brigade strength below is RECONSTRUCTED
// July-3 effectives: standard engaged-strength norms less the battle's
// earlier-day losses (the fetched tablets' own casualty totals bound the
// wrecked I/XI Corps brigades), rounded — the Wave-3 ~24-men/gun precedent
// extended to infantry, flagged in every citation. Approximation-grade by
// design; the position class, not the headcount, is the claim.
//
// SHALER CLOCK ADJUDICATION (the one Wave-4 disagreement to reconcile as
// GEOMETRY while carrying both readings): his brigade tablet says "At 3 P. M.
// returned and under terrific fire of artillery was ordered by Major Gen.
// G. G. Meade to remain in rear of Third Corps"; Wikipedia has "About 3:30
// that afternoon, Shaler's brigade was sent to the center of the army as a
// reserve". Encoded as DEPARTURE at the tablet's 3 P.M. (t=7200) and ARRIVAL
// at the survey's ~3:30 (t=9000) — the two clocks are a march apart, not a
// contradiction; both ride the keyframe citations, per the plan's t=9000.
//
// DECLINED — Wright's advance-and-recall: the dispatch brief lists it with
// this wave's movers, but Wright's is a CONFEDERATE brigade (Anderson's
// division, survey §3.1) owned by plan Task 7 (Wave 5, csa infantry ring,
// t=9000→10500). This wave is the 45 us-* brigades exactly (pin 113 → 158);
// authoring Wright here would break the wave arithmetic and the survey's
// wave order. Not authored — Task 7's checklist already carries him.
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
const uhq = "https://gettysburg.stonesentinels.com/union-headquarters/";

// The strength-reconstruction flag every brigade carries (header note).
const strengthNote = (n: number) =>
  `NO strength figure on the tablet page (casualties only) — ${n} reconstructed July-3 effectives from standard engaged-strength norms less the battle's earlier-day losses, rounded, flagged (the Wave-3 reconstruction precedent)`;

// ---- SECTOR: Culp's Hill works — XII Corps (6) ------------------------------
// Survey §3.3 "XII Corps (6)": McDougall, Colgrove, Lockwood (Ruger's div),
// Candy, Kane/Cobham, Greene (Geary's div) — the works from the summit to
// Spangler's Spring, silent after 10:30; the silence citations are the point.
// Sheet 8 s8_culps.jpg names Candy Brig / Lockwood Brig / McDougall Brig /
// Colgrove Brig / Ruger's Division / Greene's Brig, Geary's Div in place.

const rugerTablet =
  "Ruger's division tablet verbatim, July 3: 'At daylight attacked the Confederate Infantry and was hotly engaged with charges and countercharges at different points until 10:30 A. M. when the Confederate forces retired.' (" +
  uhq + "1st-division-12th-corps/, " + fetched + ")";
const gearyTablet =
  "Geary's division tablet verbatim, July 3: 'At 3 A. M. an attack by infantry and artillery was made on Johnson's Division and after a contest of seven hours the Confederate forces were driven from their position losing heavily in killed wounded and prisoners also three battle flags and over 5,000 small arms.' (" +
  uhq + "2nd-division-12th-corps/, " + fetched + ")";
const xiiSilence =
  "DOCUMENTED SILENCE (battle-format.md): the corps' seven-hour fight ended at 10:30 A. M. when Johnson retired east of Rock Creek — in 13:00–16:00 the works stand fully manned and SILENT; presence without events IS the encoding (" +
  survey + "; the Muhlenberg gun-line negative on the Wave-2 XII Corps batteries corroborates)";

const xiiCorps: Unit[] = [
  staticUnit({
    id: "us-greene",
    name: "Greene's Brigade (3rd Bde, 2nd Div, XII Corps)",
    side: "union", strength: 1125, x: 5810, z: 5590, facing: 60,
    grade: "B+",
    citation:
      gearyTablet + "; Brig. Gen. George S. Greene — his July 2 works on the Culp's Hill summit, held throughout July 3; sheet 8 s8_culps.jpg names 'Greene's Brig, Geary's Div' in place (position class: summit works, survey §1 Culp's Hill 5796,5570); " +
      xiiSilence + "; " + strengthNote(1125),
  }),
  staticUnit({
    id: "us-kane",
    name: "Kane's Brigade (2nd Bde, 2nd Div, XII Corps — Col. Cobham)",
    side: "union", strength: 600, x: 5865, z: 5480, facing: 80,
    grade: "B+",
    citation:
      gearyTablet + "; the division tablet lists 'Col. Geo. A. Cobham Jr. / Brig. Gen. Thos. L. Kane' — command passed between them across the battle, both carried, not reconciled; on Greene's right in the works; " +
      xiiSilence + "; " + strengthNote(600),
  }),
  staticUnit({
    id: "us-candy",
    name: "Candy's Brigade (1st Bde, 2nd Div, XII Corps)",
    side: "union", strength: 1400, x: 5880, z: 5380, facing: 90,
    grade: "B+",
    citation:
      gearyTablet + "; Col. Charles Candy; sheet 8 s8_culps.jpg names 'Candy Brig' on the lower hill (position class: works below Greene/Kane toward the saddle); " +
      xiiSilence + "; " + strengthNote(1400),
  }),
  staticUnit({
    id: "us-mcdougall",
    name: "McDougall's Brigade (1st Bde, 1st Div, XII Corps)",
    side: "union", strength: 1755, x: 5950, z: 5230, facing: 100,
    grade: "B+",
    citation:
      rugerTablet + "; Col. Archibald L. McDougall; sheet 8 s8_culps.jpg names 'McDougall Brig' (position class: the lower-hill works toward the swale); " +
      xiiSilence + "; " + strengthNote(1755),
  }),
  staticUnit({
    id: "us-lockwood",
    name: "Lockwood's Brigade (2nd Bde, 1st Div, XII Corps)",
    side: "union", strength: 1650, x: 5990, z: 5060, facing: 90,
    grade: "B+",
    citation:
      rugerTablet + "; Brig. Gen. Henry H. Lockwood (the division tablet lists him 2nd Brigade); sheet 8 s8_culps.jpg names 'Lockwood Brig' (position class: between McDougall and the Spangler's Spring ground); " +
      xiiSilence + "; " + strengthNote(1650),
  }),
  staticUnit({
    id: "us-colgrove",
    name: "Colgrove's Brigade (3rd Bde, 1st Div, XII Corps)",
    side: "union", strength: 1170, x: 6080, z: 4800, facing: 30,
    grade: "B+",
    citation:
      rugerTablet + "; Col. Silas Colgrove; sheet 8 s8_culps.jpg names 'Colgrove Brig' at the line's right, Spangler's Spring/McAllister's woods, facing the meadow (survey §1 Spangler's Spring 6084,4752) — his 2nd MA/27th IN charge across the meadow was ~10 A.M., PRE-window; " +
      xiiSilence + "; " + strengthNote(1170),
  }),
];

// ---- SECTOR: Culp's Hill north face — I Corps, Wadsworth's division (2) -----
// Survey §3.3 "I Corps": Meredith's Iron Bde (Col. Wm. Robinson) & Cutler —
// Culp's Hill trenches beside XII Corps, static, morning fight only.
// Sheet 8 s8_culps.jpg draws Wadsworth on the hill.

const wadsworthTablet =
  "Wadsworth's division tablet verbatim: 'July 2 & 3. Entrenched on Culp's Hill and repulsed attacks made in the evening of second and morning of third.' (" +
  uhq + "1st-division-1st-corps/, " + fetched + ") — the morning repulse is the division's last fight; in-window the trenches are held in silence (documented-silence convention, " + survey + ")";

const wadsworthDiv: Unit[] = [
  staticUnit({
    id: "us-meredith",
    name: "Meredith's Iron Brigade (Col. Wm. W. Robinson)",
    side: "union", strength: 730, x: 5620, z: 5680, facing: 10,
    grade: "B+",
    citation:
      wadsworthTablet + "; Col. William W. Robinson commanding (Brig. Gen. Meredith wounded July 1 — acting-commander naming per the survey); position class: the steep north face of Culp's Hill, the division's left; " +
      strengthNote(730) + " — the brigade was wrecked July 1 (the division's 2,155 casualties are overwhelmingly July 1)",
  }),
  staticUnit({
    id: "us-cutler",
    name: "Cutler's Brigade (2nd Bde, 1st Div, I Corps)",
    side: "union", strength: 1015, x: 5720, z: 5615, facing: 45,
    grade: "B+",
    citation:
      wadsworthTablet + "; Brig. Gen. Lysander Cutler; position class: the trenches on Greene's left, upper north slope; " +
      strengthNote(1015) + " — heavy July 1 losses",
  }),
];

// ---- SECTOR: Cemetery Hill — XI Corps (6) + Carroll (II Corps) --------------
// Survey §3.3 "XI Corps (6)": von Gilsa, Harris, Coster, Smith, von Amsberg,
// Krzyżanowski — all static, documented negatives under fire; and Carroll's
// brigade (minus the modeled 8th OH) supporting the East Cemetery Hill
// batteries. Sheet 8 s8_cemhill.jpg: Steinwehr's 11 Corps, Schurz Div.,
// von Gilsa, Carroll Brig. drawn in place.

const amesTablet =
  "Ames's division tablet verbatim, July 3: 'At 1 P. M. heavy cannonade opened and continued with considerable effect for an hour and a half followed by a charge on the Second Corps on the left which was repulsed with great loss.' (" +
  uhq + "1st-division-11th-corps/, " + fetched + ") — the division watches the bombardment and the charge from East Cemetery Hill; no engagement of its own is claimed: a documented negative under fire (" + survey + ")";
const steinwehrTablet =
  "von Steinwehr's division tablet verbatim, July 3: 'Not engaged but subject to the fire of sharpshooters and artillery.' (" +
  uhq + "2nd-division-11th-corps/, " + fetched + ") — the documented negative IS the attestation (" + survey + ")";
const schurzTablet =
  "Schurz's division tablet verbatim, July 3: 'Not engaged except skirmishing.' (" +
  uhq + "3rd-division-11th-corps/, " + fetched + ") — skirmishing at picket grain, below the event model's attested threshold: no event authored, the negative rides here (" + survey + ")";

const cemeteryHill: Unit[] = [
  staticUnit({
    id: "us-vongilsa",
    name: "von Gilsa's Brigade (1st Bde, 1st Div, XI Corps)",
    side: "union", strength: 605, x: 5130, z: 5860, facing: 10,
    grade: "B+",
    citation:
      amesTablet + "; Col. Leopold von Gilsa; sheet 8 s8_cemhill.jpg names 'von Gilsa' at the East Cemetery Hill base facing the town approaches (survey §1 East Cemetery Hill 5107,5753); " +
      strengthNote(605) + " — wrecked July 1, stormed July 2 evening",
  }),
  staticUnit({
    id: "us-harris",
    name: "Harris's Brigade (2nd Bde, 1st Div, XI Corps — Ames's)",
    side: "union", strength: 565, x: 5170, z: 5790, facing: 40,
    grade: "B+",
    citation:
      amesTablet + "; Col. Andrew L. Harris commanding (Brig. Gen. Ames at division for Barlow, wounded July 1 — acting-commander naming per the survey); behind the stone wall on von Gilsa's right, the ground of the July 2 evening fight; " +
      strengthNote(565),
  }),
  staticUnit({
    id: "us-coster",
    name: "Coster's Brigade (1st Bde, 2nd Div, XI Corps)",
    side: "union", strength: 655, x: 4935, z: 5765, facing: 340,
    grade: "B+",
    citation:
      steinwehrTablet + "; Col. Charles R. Coster; position class: the northwest slope of Cemetery Hill toward the town edge (survey §1 Cemetery Hill 4993,5656); " +
      strengthNote(655) + " — heavy July 1 losses in the brickyard fight",
  }),
  staticUnit({
    id: "us-smith",
    name: "Smith's Brigade (2nd Bde, 2nd Div, XI Corps)",
    side: "union", strength: 1290, x: 4880, z: 5610, facing: 280,
    grade: "B+",
    citation:
      steinwehrTablet + "; Col. Orland Smith; position class: the west face of Cemetery Hill behind the stone wall over the Taneytown Road — under the cannonade's direct fire; sheet 8 s8_cemhill.jpg draws 'Steinwehr's 11 Corps' here; " +
      strengthNote(1290),
  }),
  staticUnit({
    id: "us-vonamsberg",
    name: "von Amsberg's Brigade (1st Bde, 3rd Div, XI Corps — Schimmelfennig's)",
    side: "union", strength: 875, x: 4900, z: 5655, facing: 300,
    grade: "B+",
    citation:
      schurzTablet + "; Col. George von Amsberg commanding (Brig. Gen. Schimmelfennig cut off in the town since July 1 — acting-commander naming per the survey); position class: interior of the hill behind Smith's wall line; " +
      strengthNote(875),
  }),
  staticUnit({
    id: "us-krzyzanowski",
    name: "Krzyżanowski's Brigade (2nd Bde, 3rd Div, XI Corps)",
    side: "union", strength: 750, x: 5065, z: 5625, facing: 320,
    grade: "B+",
    citation:
      schurzTablet + "; Col. Włodzimierz Krzyżanowski; position class: among the cemetery's gun line (the hill's infantry lay in support between and behind Osborn's batteries); sheet 8 s8_cemhill.jpg draws 'Schurz Div.'; " +
      strengthNote(750),
  }),
  staticUnit({
    id: "us-carroll",
    name: "Carroll's Brigade (1st Bde, 3rd Div, II Corps — minus the 8th Ohio)",
    side: "union", strength: 640, x: 5115, z: 5675, facing: 30,
    grade: "B+",
    citation:
      "War Dept tablet verbatim, July 3: 'Sharp skirmishing continued through the day the Brigade was subjected to an annoying sharpshooters fire from the houses in the town and a cross fire from artillery from the north east and west. The 8th Ohio assisted in the repulse of Longstreet's assault.' (" +
      uhq + "1st-brigade-3rd-division-2nd-corps/, " + fetched + "); Col. Samuel S. Carroll; on East Cemetery Hill supporting the batteries since the July 2 evening counterattack; sheet 8 s8_cemhill.jpg names 'Carroll Brig.'; " +
      "MINUS THE MODELED 8th OHIO: the detached regiment is the free-standing unit us-8oh (its own track and flank-fire events, out toward the Emmitsburg Road) — this unit is the brigade's remaining three regiments, strength excludes the detachment, NO parent link (family rules: none, divisions not modeled); the house-sharpshooter fire the tablet complains of is Early's, owned by Wave 5; " +
      strengthNote(640),
  }),
];

// ---- SECTOR: the fishhook centre — I Corps remnants (4: 2 movers + 2) -------
// Survey §3.3 "I Corps": Robinson's division (Coulter's & Baxter's bdes) —
// THE Union attested in-window infantry movers, "at 3 P.M. took position on
// the right of Second Corps" (t=7200, A−; musketry NOT attested — no event,
// flag in citation); Doubleday's two non-Stannard brigades on Cemetery Ridge
// left of Gibbon — A− repulse participation, one musketry event each.

const robinsonTablet =
  "Robinson's division tablet verbatim, July 3: 'At daylight moved to the support of batteries on Cemetery Hill. At 9 A. M. sent to the support of Twelfth Corps and at 3 P M. took position on the right of Second Corps and remained until the close of the battle.' (" +
  uhq + "2nd-division-first-corps/, " + fetched + ") — the documented 3 P.M. clock = t 7200 (" + survey + "; §4 item 7)";
const robinsonNoMusketry =
  "MUSKETRY NOT ATTESTED (plan Task 6 flag): the tablet gives position and clock, no fire — no event authored; the brigades arrive, stand on the II Corps right, and the record stops there";

const coulter = moverUnit({
  id: "us-coulter",
  name: "Coulter's Brigade (1st Bde, 2nd Div, I Corps — Paul's)",
  side: "union",
  keyframes: [
    { t: 0, x: 5500, z: 5280, facing: 80, formation: "line", strength: 510, confidence: "documented",
      citation: robinsonTablet + "; at t=0 the division stands in support of the Twelfth Corps (the tablet's 9 A.M. posting) — position class: the Baltimore Pike reserve ground behind Culp's Hill, coordinate inferred within it; Col. Richard Coulter commanding (Brig. Gen. Paul blinded July 1 — the tablet still lists Paul; acting-commander naming per the survey); " + strengthNote(510) + " — wrecked July 1 (division total 1,690 casualties)" },
    { t: 6600, x: 5500, z: 5280, facing: 80, formation: "column", strength: 510, confidence: "inferred",
      citation: "limbers onto the pike ~14:50 to make the tablet's 3 P.M. arrival at a march — departure clock inferred, move documented" },
    { t: 7200, x: 4500, z: 5210, facing: 260, formation: "line", strength: 510, confidence: "documented",
      citation: robinsonTablet + "; arrival: 'at 3 P M. took position on the right of Second Corps' — right (north) of Sherrill's brigade toward Ziegler's Grove (survey §1: 4509,5146), coordinate inferred within the attested position class; " + robinsonNoMusketry },
    { t: WindowEndT, x: 4500, z: 5210, facing: 260, formation: "line", strength: 510, confidence: "documented",
      citation: robinsonTablet + "; 'and remained until the close of the battle' — holds the II Corps right to the window end" },
  ],
});

const baxter = moverUnit({
  id: "us-baxter",
  name: "Baxter's Brigade (2nd Bde, 2nd Div, I Corps)",
  side: "union",
  keyframes: [
    { t: 0, x: 5545, z: 5220, facing: 80, formation: "line", strength: 800, confidence: "documented",
      citation: robinsonTablet + "; at t=0 in support of the Twelfth Corps beside Coulter — position class: the Baltimore Pike reserve ground, coordinate inferred within it; Brig. Gen. Henry Baxter; " + strengthNote(800) },
    { t: 6600, x: 5545, z: 5220, facing: 80, formation: "column", strength: 800, confidence: "inferred",
      citation: "limbers onto the pike ~14:50 with Coulter — departure clock inferred, move documented" },
    { t: 7200, x: 4525, z: 5275, facing: 260, formation: "line", strength: 800, confidence: "documented",
      citation: robinsonTablet + "; arrival: 'at 3 P M. took position on the right of Second Corps' — beyond Coulter on the division's right near Ziegler's Grove, coordinate inferred within the attested position class; " + robinsonNoMusketry },
    { t: WindowEndT, x: 4525, z: 5275, facing: 260, formation: "line", strength: 800, confidence: "documented",
      citation: robinsonTablet + "; 'and remained until the close of the battle'" },
  ],
});

const rowleyTablet =
  "Doubleday's (Rowley's) division tablet verbatim, July 3: 'In position on left of Second Division Second Corps. Assisted in repulsing Longstreet's assault capturing many prisoners and three stand of colors.' (" +
  uhq + "3rd-division-1st-corps/, " + fetched + ")";

const doubledayBrigades: Unit[] = [
  staticUnit({
    id: "us-biddle",
    name: "Biddle's Brigade (1st Bde, 3rd Div, I Corps — Col. Gates at the line)",
    side: "union", strength: 460, x: 4480, z: 4555, facing: 262,
    grade: "A-",
    citation:
      "brigade tablet verbatim, July 3: 'Remained in the same position and assisted in repelling Longstreet's assault in the afternoon taking many prisoners.' (" +
      uhq + "1st-brigade-3rd-division-1st-corps/, " + fetched + "); " + rowleyTablet + "; Col. Chapman Biddle wounded July 1 (the tablet lists Biddle and Rowley) — Col. Theodore B. Gates's demi-brigade held the line July 3, both names carried; position class: second line on Cemetery Ridge left of Gibbon, in rear of Rorty's gun line and Caldwell's front, beside its own division's Stannard; " +
      strengthNote(460) + " — wrecked July 1 (brigade total 898 casualties)",
  }),
  staticUnit({
    id: "us-dana",
    name: "Dana's Brigade (2nd Bde, 3rd Div, I Corps — Stone's)",
    side: "union", strength: 465, x: 4420, z: 4500, facing: 262,
    grade: "A-",
    citation:
      rowleyTablet + " — the division tablet is the July 3 attestation (the 2nd Brigade's own tablet adds no finer window); Col. Edmund L. Dana commanding (Col. Stone and Col. Wister both wounded July 1 — acting-commander naming per the survey); position class: second line left of Gibbon beside Biddle; " +
      strengthNote(465) + " — wrecked July 1",
  }),
];

// ---- SECTOR: the fishhook centre-south — II Corps, Caldwell's division (4) --
// Survey §3.3 "II Corps remainder": McKeen (for Cross), Kelly, Fraser (for
// Zook), Brooke — the south end of the II Corps front, B+ statics under the
// cannonade. Sheet 8 s8_mcgilvery.jpg draws Caldwell's Cross/Brooke/Zook bars.

const caldwellTablet =
  "Caldwell's division tablet verbatim, July 3: 'The Division formed in single line threw up breastworks and remained in position until close of the battle.' (" +
  uhq + "1st-division-2nd-corps/, " + fetched + ")";

const caldwellDiv: Unit[] = [
  staticUnit({
    id: "us-mckeen",
    name: "McKeen's Brigade (1st Bde, 1st Div, II Corps — Cross's)",
    side: "union", strength: 520, x: 4370, z: 4560, facing: 262,
    grade: "B+",
    citation:
      "brigade tablet verbatim, July 3: 'Constructed breastworks early in the morning which gave protection from the cannonade in the afternoon. Remained in position until the close of the battle.' (" +
      uhq + "1st-brigade-1st-division-2nd-corps/, " + fetched + "); Col. H. Boyd McKeen commanding (Col. Cross mortally wounded July 2 — acting-commander naming per the survey); the division's right, front line south of Harrow; sheet 8 s8_mcgilvery.jpg draws the Cross bar; " +
      strengthNote(520),
  }),
  staticUnit({
    id: "us-kelly",
    name: "Kelly's Irish Brigade (2nd Bde, 1st Div, II Corps)",
    side: "union", strength: 330, x: 4355, z: 4500, facing: 262,
    grade: "B+",
    citation:
      caldwellTablet + "; Col. Patrick Kelly; in the division's single line behind breastworks under the cannonade; " +
      strengthNote(330) + " — the smallest brigade on the ridge after the July 2 Wheatfield fight",
  }),
  staticUnit({
    id: "us-fraser",
    name: "Fraser's Brigade (3rd Bde, 1st Div, II Corps — Zook's)",
    side: "union", strength: 615, x: 4345, z: 4445, facing: 262,
    grade: "B+",
    citation:
      caldwellTablet + "; Lieut. Col. John Fraser commanding (Brig. Gen. Zook mortally wounded July 2 — acting-commander naming per the survey); sheet 8 s8_mcgilvery.jpg draws the Zook bar; Stannard's Vermonters stand forward-left of this front; " +
      strengthNote(615),
  }),
  staticUnit({
    id: "us-brooke",
    name: "Brooke's Brigade (4th Bde, 1st Div, II Corps)",
    side: "union", strength: 460, x: 4335, z: 4390, facing: 262,
    grade: "B+",
    citation:
      caldwellTablet + "; Col. John R. Brooke; the division's left, the II Corps front's south end above McGilvery's gun line; sheet 8 s8_mcgilvery.jpg draws the Brooke bar; " +
      strengthNote(460),
  }),
];

// ---- SECTOR: the rear-left — III Corps in army reserve (6) ------------------
// Survey §3.3 "III Corps (6)": both divisions in reserve behind the
// left-centre; "which detachments moved is unattested — flag". Grade B,
// reserve mass (column). Sheet 8 s8_mcgilvery.jpg draws the block:
// Carr/Brewster/Burling/Madill/De Trobriand/Berdan "3 Corps".

const birneyTablet =
  "Birney's division tablet verbatim, July 3: 'The Division was held in reserve and detachments moved to threatened points.' (" +
  uhq + "1st-division-3rd-corps/, " + fetched + ") — WHICH detachments moved where is unattested (survey flag): the division is authored at its reserve position class, no detachment tracks invented";
const humphreysTablet =
  "Humphreys's division tablet verbatim, July 3: 'About sunrise moved to the rear and left and was supplied with rations and ammunition. Burling's Brigade joined the Division moved to different points in rear of the First Second Fifth and some Sixth Corps in support of threatened positions.' (" +
  uhq + "2nd-division-3rd-corps/, " + fetched + ") — the in-window 'different points' are unattested (survey flag): authored at the reserve position class";

const iiiCorps: Unit[] = [
  staticUnit({
    id: "us-madill",
    name: "Madill's Brigade (1st Bde, 1st Div, III Corps — Graham's)",
    side: "union", strength: 775, x: 4640, z: 4180, facing: 265,
    formation: "column", grade: "B",
    citation:
      birneyTablet + "; Maj. Henry J. Madill commanding (Brig. Gen. Graham wounded and captured July 2 — acting-commander naming per the survey); sheet 8 s8_mcgilvery.jpg names 'Madill' in the 3 Corps block; reserve mass between the Taneytown Road and the ridge; " +
      strengthNote(775),
  }),
  staticUnit({
    id: "us-berdan",
    name: "Berdan's Brigade (2nd Bde, 1st Div, III Corps — Ward's)",
    side: "union", strength: 1405, x: 4660, z: 4100, facing: 265,
    formation: "column", grade: "B",
    citation:
      birneyTablet + "; Col. Hiram Berdan commanding July 3 (Brig. Gen. Ward at division duty — acting-commander naming per the survey); sheet 8 s8_mcgilvery.jpg names 'Berdan'; " +
      strengthNote(1405),
  }),
  staticUnit({
    id: "us-detrobriand",
    name: "de Trobriand's Brigade (3rd Bde, 1st Div, III Corps)",
    side: "union", strength: 895, x: 4680, z: 4020, facing: 265,
    formation: "column", grade: "B",
    citation:
      birneyTablet + "; Col. P. Régis de Trobriand; sheet 8 s8_mcgilvery.jpg names 'De Trobriand'; " +
      strengthNote(895),
  }),
  staticUnit({
    id: "us-carr",
    name: "Carr's Brigade (1st Bde, 2nd Div, III Corps)",
    side: "union", strength: 925, x: 4745, z: 4265, facing: 265,
    formation: "column", grade: "B",
    citation:
      humphreysTablet + "; Brig. Gen. Joseph B. Carr; sheet 8 s8_mcgilvery.jpg names 'Carr'; " +
      strengthNote(925),
  }),
  staticUnit({
    id: "us-brewster",
    name: "Brewster's Excelsior Brigade (2nd Bde, 2nd Div, III Corps)",
    side: "union", strength: 1060, x: 4765, z: 4185, facing: 265,
    formation: "column", grade: "B",
    citation:
      humphreysTablet + "; Col. William R. Brewster; sheet 8 s8_mcgilvery.jpg names 'Brewster'; " +
      strengthNote(1060),
  }),
  staticUnit({
    id: "us-burling",
    name: "Burling's Brigade (3rd Bde, 2nd Div, III Corps)",
    side: "union", strength: 850, x: 4785, z: 4105, facing: 265,
    formation: "column", grade: "B",
    citation:
      humphreysTablet + " — 'Burling's Brigade joined the Division' after its July 2 dispersal; Col. George C. Burling; sheet 8 s8_mcgilvery.jpg names 'Burling'; " +
      strengthNote(850),
  }),
];

// ---- SECTOR: the Round Tops — V Corps (8) -----------------------------------
// Survey §3.3 "V Corps (8)": Tilton (relieved Rice on the line July 3 AM),
// Sweitzer & Rice north of LRT, Day & Burbank behind, Garrard for Weed on the
// summit line, McCandless (advanced Plum Run line; his Wheatfield advance is
// ~5 PM, POST-window — static here), Fisher (Big RT breastworks) — all B
// static. Sheet 8 s8_roundtops.jpg: Weed's Brigade (Garrard), Tilton's,
// Fisher's, Day, Ayres Div., Grant's Brig. at J. Weikert.

const barnesTablet =
  "Barnes's division tablet verbatim, July 3: 'The Third Brigade was relieved by the First Brigade and joined Second Brigade north of Little Round Top. Remained in these positions until the close of the battle except reconnaissance to the front.' (" +
  uhq + "1st-division-5th-corps/, " + fetched + ")";
const ayresTablet =
  "Ayres's division tablet, July 3: 'Remained in same position' — the woods behind the Third Brigade at Little Round Top (" +
  uhq + "2nd-division-5th-corps/, " + fetched + "; " + survey + ")";
const crawfordTablet =
  "Crawford's division tablet verbatim, July 3 (First Brigade): 'remained in position until about 5 P. M. and then advanced across the Wheatfield and through the woods beyond and on the left capturing many prisoners.' (" +
  uhq + "3rd-division-5th-corps/, " + fetched + ") — the Wheatfield advance is ~5 P.M., POST-window: static in 13:00–16:00, the advance is out of slice (plan Task 6 note)";

const vCorps: Unit[] = [
  staticUnit({
    id: "us-tilton",
    name: "Tilton's Brigade (1st Bde, 1st Div, V Corps)",
    side: "union", strength: 530, x: 4260, z: 2620, facing: 255,
    grade: "B",
    citation:
      barnesTablet + " — the First Brigade relieved Rice's on the Round Top line the morning of July 3; Col. William S. Tilton; sheet 8 s8_roundtops.jpg names 'Tilton's'; position class: the line north shoulder of Little Round Top (survey §1: 4296,2441); " +
      strengthNote(530),
  }),
  staticUnit({
    id: "us-sweitzer",
    name: "Sweitzer's Brigade (2nd Bde, 1st Div, V Corps)",
    side: "union", strength: 995, x: 4290, z: 2760, facing: 260,
    grade: "B",
    citation:
      barnesTablet + "; Col. Jacob B. Sweitzer; position class: north of Little Round Top with Rice; " +
      strengthNote(995) + " — heavy July 2 Wheatfield losses",
  }),
  staticUnit({
    id: "us-rice",
    name: "Rice's Brigade (3rd Bde, 1st Div, V Corps — Vincent's)",
    side: "union", strength: 985, x: 4315, z: 2860, facing: 260,
    grade: "B",
    citation:
      barnesTablet + " — relieved on the hill by Tilton and 'joined Second Brigade north of Little Round Top'; Col. James C. Rice commanding (Col. Strong Vincent mortally wounded July 2 — acting-commander naming per the survey); sheet 8 s8_mcgilvery.jpg names 'Vincent (Rice)'; " +
      strengthNote(985),
  }),
  staticUnit({
    id: "us-day",
    name: "Day's US Regular Brigade (1st Bde, 2nd Div, V Corps)",
    side: "union", strength: 1170, x: 4430, z: 2520, facing: 250,
    grade: "B",
    citation:
      ayresTablet + "; Col. Hannibal Day; sheet 8 s8_roundtops.jpg names 'Day'; position class: the woods behind the summit line; " +
      strengthNote(1170),
  }),
  staticUnit({
    id: "us-burbank",
    name: "Burbank's US Regular Brigade (2nd Bde, 2nd Div, V Corps)",
    side: "union", strength: 505, x: 4460, z: 2440, facing: 250,
    grade: "B",
    citation:
      ayresTablet + "; Col. Sidney Burbank; beside Day in the woods behind Little Round Top; " +
      strengthNote(505) + " — heavy July 2 losses in the Wheatfield plain",
  }),
  staticUnit({
    id: "us-garrard",
    name: "Garrard's Brigade (3rd Bde, 2nd Div, V Corps — Weed's)",
    side: "union", strength: 1290, x: 4330, z: 2400, facing: 250,
    grade: "B",
    citation:
      ayresTablet + "; Col. Kenner Garrard commanding (Brig. Gen. Weed killed July 2 — acting-commander naming per the survey); sheet 8 s8_roundtops.jpg names 'Weed's Brigade (Garrard)' on the Little Round Top summit line beside Rittenhouse's guns; " +
      strengthNote(1290),
  }),
  staticUnit({
    id: "us-mccandless",
    name: "McCandless's Pennsylvania Reserves Brigade (1st Bde, 3rd Div, V Corps)",
    side: "union", strength: 1095, x: 3980, z: 2680, facing: 260,
    grade: "B",
    citation:
      crawfordTablet + "; Col. William McCandless; position class: the advanced Plum Run line west of Little Round Top, held since the July 2 evening counterattack; sheet 8 s8_peachorchard.jpg names 'McCandless Brigade'; " +
      strengthNote(1095),
  }),
  staticUnit({
    id: "us-fisher",
    name: "Fisher's Pennsylvania Reserves Brigade (3rd Bde, 3rd Div, V Corps)",
    side: "union", strength: 1555, x: 4085, z: 1925, facing: 240,
    grade: "B",
    citation:
      "Crawford's division page (" + uhq + "3rd-division-5th-corps/, " + fetched + ") — the tablet details the First Brigade; Fisher's July 3 movements are not separately inscribed (flag): position per the survey — Big Round Top breastworks, held from the night of July 2 (" + survey + "; survey §1 Big RT 4076,1869); Col. Joseph W. Fisher; sheet 8 s8_roundtops.jpg names 'Fisher's'; " +
      strengthNote(1555),
  }),
];

// ---- SECTOR: the left rear and extreme right — VI Corps (8) -----------------
// Survey §3.3 "VI Corps (8)": Torbert (SE of Weikert, 'Not engaged except on
// the skirmish line'), Bartlett + Nevin (advanced Plum Run line; the
// Benning/15th GA fight is post-window), Russell (E slope Big RT; his late
// move straddles 16:00 — static, flagged), Grant's Vermonters (Round Top to
// the Taneytown Road), Neill (across Rock Creek, the army's extreme right —
// skirmish attested, the wave's one non-corridor musketry emitter east of the
// creek), Shaler (the ~3:30 mover to the army centre), Eustis (right-centre
// reserve under Newton).

const viCorps: Unit[] = [
  staticUnit({
    id: "us-torbert",
    name: "Torbert's New Jersey Brigade (1st Bde, 1st Div, VI Corps)",
    side: "union", strength: 1320, x: 4210, z: 3560, facing: 250,
    grade: "B+",
    citation:
      "War Dept tablet verbatim, July 3: 'Moved to a position southeast of the Weikert House and remained until the close of the battle. Not engaged except on the skirmish line.' (" +
      uhq + "1st-brigade-1st-division-6th-corps/, " + fetched + ") — a documented negative; the skirmish-line exception is picket grain, no event authored (documented-silence convention); Brig. Gen. Alfred T. A. Torbert; sheet 8 s8_mcgilvery.jpg names 'Torbert's Brig.' (survey §1 Weikert 4112,3646); " +
      strengthNote(1320),
  }),
  staticUnit({
    id: "us-bartlett",
    name: "Bartlett's Brigade (2nd Bde, 1st Div, VI Corps)",
    side: "union", strength: 1325, x: 4050, z: 2980, facing: 260,
    grade: "B",
    citation:
      "War Dept tablet, July 3 (verbatim, the late-day passage): 'Late in the day the Third Brigade Third Division in a second line at an interval of 200 yards supported First Brigade Third Division Fifth Corps in an advance through the Wheatfield…' — the advance (with McCandless, vs Benning's 15th GA) is ~5 P.M., POST-window: static on the advanced Plum Run line in 13:00–16:00 (" +
      uhq + "2nd-brigade-1st-division-6th-corps/, " + fetched + "; " + survey + "); Brig. Gen. Joseph J. Bartlett; " +
      strengthNote(1325),
  }),
  staticUnit({
    id: "us-nevin",
    name: "Nevin's Brigade (3rd Bde, 3rd Div, VI Corps — Wheaton's)",
    side: "union", strength: 1370, x: 4095, z: 3085, facing: 260,
    grade: "B",
    citation:
      "War Dept tablet, July 3 (verbatim): 'Late in the day supported First Brigade Third Division Fifth Corps at an interval of 200 yards in advance through the Wheatfield' — POST-window (~5 P.M.); in-window static on the advanced Plum Run line beside Bartlett (" +
      uhq + "3rd-brigade-3rd-division/, " + fetched + "); COMMAND NOTE, carried not reconciled: the survey and sheet 8 ('Nevin') have Col. David J. Nevin commanding (Wheaton up to division); the Stone Sentinels page assigns Brig. Gen. Bartlett to the brigade the morning of July 3 — both readings kept (" + survey + "); " +
      strengthNote(1370),
  }),
  staticUnit({
    id: "us-russell",
    name: "Russell's Brigade (3rd Bde, 1st Div, VI Corps)",
    side: "union", strength: 1480, x: 4185, z: 1845, facing: 120,
    grade: "B",
    citation:
      "War Dept tablet verbatim, July 3: 'Moved to the extreme left and on the east slope of Round Top and remained until late in the afternoon then went into position on the left centre in support of Fifth Corps. Not engaged.' (" +
      uhq + "3rd-brigade-1st-division-6th-corps/, " + fetched + ") — the 'late in the afternoon' move STRADDLES 16:00 (survey flag): left static at the east-slope position, the move is not modeled inside the window (plan Task 6); Brig. Gen. David A. Russell; " +
      strengthNote(1480),
  }),
  staticUnit({
    id: "us-grant",
    name: "Grant's Vermont Brigade (2nd Bde, 2nd Div, VI Corps)",
    side: "union", strength: 1830, x: 4340, z: 2180, facing: 150,
    grade: "B+",
    citation:
      "War Dept tablet verbatim, July 3: 'The Brigade advanced a short distance and took position with its right on east slope of Round Top its left on the Taneytown Road and remained until the close of the battle under no fire except that from artillery.' (" +
      uhq + "2nd-brigade-2nd-division-6th-corps/, " + fetched + ") — a documented negative under fire; the line spans the Round Top east slope to the Taneytown Road guarding the left rear, unit centered mid-span; Col. Lewis A. Grant; sheet 8 s8_roundtops.jpg names 'Grant's Brig.' at J. Weikert; " +
      strengthNote(1830),
  }),
  staticUnit({
    id: "us-neill",
    name: "Neill's Brigade (3rd Bde, 2nd Div, VI Corps)",
    side: "union", strength: 1775, x: 7000, z: 5480, facing: 350,
    grade: "A-",
    citation:
      "War Dept tablet verbatim, July 3: 'The Brigade by order of Major Gen. Slocum crossed Rock Creek and took position on the extreme right of the Army making connection with the Cavalry pickets and encountered and checked the advancing Confederate sharpshooters and skirmishers and remained until the close of the battle.' (" +
      uhq + "3rd-brigade-2nd-division-6th-corps/, " + fetched + ") — the army's extreme right, EAST of Rock Creek in the Wolf Hill sector (survey §1 Wolf Hill 7287,5622), the on-map end of the cavalry screen toward Brinkerhoff's Ridge; skirmishing attested → the wave's one right-flank musketry window (survey B+/A−); Brig. Gen. Thomas H. Neill; sheet 8 s8_culps.jpg names 'Neill's' at lower left; " +
      strengthNote(1775),
  }),
  staticUnit({
    id: "us-eustis",
    name: "Eustis's Brigade (2nd Bde, 3rd Div, VI Corps)",
    side: "union", strength: 1595, x: 4820, z: 4780, facing: 265,
    formation: "column", grade: "B+",
    citation:
      "War Dept tablet verbatim, July 3: 'Moved to the right centre and reported to Major Gen. J. Newton and was held in reserve during the battle. Not engaged but subject to artillery fire.' (" +
      uhq + "2nd-brigade-3rd-division/, " + fetched + ") — a documented negative under fire (69 casualties across the battle, tablet); Col. Henry L. Eustis; reserve mass behind the II Corps centre; " +
      strengthNote(1595),
  }),
];

// Shaler — the ~3:30 mover to the army centre (survey A−; §4 item 8). Clock
// adjudication per the header: departure at the tablet's 3 P.M. (t=7200),
// arrival at the survey's ~3:30 (t=9000), both readings carried.
const shalerTablet =
  "War Dept tablet verbatim, July 3: 'At 3 P. M. returned and under terrific fire of artillery was ordered by Major Gen. G. G. Meade to remain in rear of Third Corps and to report to Major Gen. J. Newton.' (" +
  uhq + "1st-brigade-3rd-division/, " + fetched + ")";

const shaler = moverUnit({
  id: "us-shaler",
  name: "Shaler's Brigade (1st Bde, 3rd Div, VI Corps)",
  side: "union",
  keyframes: [
    { t: 0, x: 5620, z: 5240, facing: 80, formation: "line", strength: 1770, confidence: "documented",
      citation: "tablet, July 3: 'Took position in rear of woods on Culp's Hill beyond which action was progressing and was engaged…from 9 until 11 A. M. when the original line of the Twelfth Corps was regained.' (" + uhq + "1st-brigade-3rd-division/, " + fetched + ") — at t=0 in reserve in rear of the Culp's Hill woods, the morning fight over; position class inferred within it; sheet 8 s8_culps.jpg names 'Shaler'; Brig. Gen. Alexander Shaler; " + strengthNote(1770) },
    { t: 7200, x: 5620, z: 5240, facing: 80, formation: "column", strength: 1770, confidence: "documented",
      citation: shalerTablet + " — departure at the tablet's 3 P.M., 'under terrific fire of artillery' (the cannonade's overshoots on the pike)" },
    { t: 9000, x: 4720, z: 4430, facing: 265, formation: "line", strength: 1770, confidence: "documented",
      citation: shalerTablet + "; arrival ~3:30 per the survey — 'About 3:30 that afternoon, Shaler's brigade was sent to the center of the army as a reserve around the time of the repulse of Pickett's Charge.' (https://en.wikipedia.org/wiki/Alexander_Shaler, " + fetched + "; " + survey + ", plan t=9000) — CLOCK SPREAD CARRIED: tablet 3 P.M. vs Wikipedia ~3:30, encoded as a 30-minute march, both readings kept; position class: in rear of the III Corps reserve block at the army centre, coordinate inferred within it" },
    { t: WindowEndT, x: 4720, z: 4430, facing: 265, formation: "line", strength: 1770, confidence: "inferred",
      citation: "holds at the centre to the window end — the tablet's next clock ('At 7 P. M. moved half a mile to the right') is post-window" },
  ],
});

// ---- events: the THREE survey-attested windows, nothing else ----------------
// Doubleday's two brigades: one musketry event each, ~15:15–15:45 per the
// plan (t 8100–9900), documented per the division/brigade tablets; Neill's
// Rock Creek skirmish: one small window, clock inferred. NO other Wave-4
// unit fires (the ring's silence is the encoding).

const events: EngagementEvent[] = [
  fireEvent({
    id: "us-biddle-repulse-fire", kind: "musketry",
    t0: 8100, t1: 9900, unitId: "us-biddle", confidence: "documented",
    citation:
      "brigade tablet verbatim: 'assisted in repelling Longstreet's assault in the afternoon taking many prisoners.' (" +
      uhq + "1st-brigade-3rd-division-1st-corps/, " + fetched + "); " + rowleyTablet,
    note:
      "window ~15:15–15:45 per plan Task 6 — the tablet is unwindowed; spans the corridor climax (existing events 8340–9300) through the prisoner-taking at the collapse, coherent with its own division's Stannard flank events (8400–9000) on the same ground",
  }),
  fireEvent({
    id: "us-dana-repulse-fire", kind: "musketry",
    t0: 8100, t1: 9900, unitId: "us-dana", confidence: "documented",
    citation:
      rowleyTablet + " — 'capturing many prisoners and three stand of colors'; attestation at DIVISION grain (the 2nd Brigade's own tablet adds no finer July 3 window) — the event attaches at the brigade because the division is not modeled, said here per the attach-level rule",
    note:
      "window ~15:15–15:45 per plan Task 6, as us-biddle-repulse-fire",
  }),
  fireEvent({
    id: "us-neill-rock-creek-skirmish", kind: "musketry",
    t0: 1800, t1: 3600, unitId: "us-neill", confidence: "documented",
    citation:
      "War Dept tablet verbatim: 'encountered and checked the advancing Confederate sharpshooters and skirmishers' — east of Rock Creek on the army's extreme right (" +
      uhq + "3rd-brigade-2nd-division-6th-corps/, " + fetched + "; " + survey + ")",
    note:
      "clock unattested — the tablet's skirmishing is unwindowed; one small window placed mid-early in the slice per plan Task 6 ('one small musketry window'), the Wave-1 Hart pattern: fire documented, window inferred; skirmish grain, low intensity",
  }),
];

// ---- assemble, gate, export -------------------------------------------------
// The 45 land grouped per-sector after the existing Union infantry block
// (us-16vt is the last infantry unit) — Culp's Hill first, then Cemetery
// Hill, then the fishhook centre-south, matching the survey's walk.

const wave4: Unit[] = [
  ...xiiCorps, ...wadsworthDiv, //     Culp's Hill (XII Corps + Wadsworth)
  ...cemeteryHill, //                  Cemetery Hill (XI Corps + Carroll)
  coulter, baxter, ...doubledayBrigades, // I Corps remnants (centre)
  ...caldwellDiv, //                   II Corps south end
  ...iiiCorps, //                      the rear-left reserve
  ...vCorps, //                        the Round Tops
  ...viCorps, shaler, //               VI Corps ring + the ~3:30 mover
];
battle = addUnitsAfter(battle, "us-16vt", wave4);
battle.events = [...(battle.events ?? []), ...events]; // exportValidated sorts canonically

writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events?.length ?? 0} events (wave 4: ${wave4.length} brigades, ${events.length} musketry)`);
