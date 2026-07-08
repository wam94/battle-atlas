// Wave 6 authoring script (plan: docs/superpowers/plans/2026-07-02-full-cast.md
// Task 8): the South Cavalry Field cluster — the only maneuver combat on the
// square outside the charge, and it IS in-window. Survey basis:
// docs/research/2026-07-02-full-cast-sources.md §3.4 (South Cavalry Field),
// §2.1 (s8_southcav.jpg — sheet 8's south margin at regiment level), §4 item
// 12, §5.2 step 7. Six units: Farnsworth's and Merritt's brigades with
// Elder's and Graham's batteries on the Union side; Bachman's and Reilly's
// batteries as csa-bn-henry's children on the Confederate side — plus the
// attested detachment of Reilly's Parrott section as a fixed-segment event.
// Position classes from the survey §1 landmark table (Bushman Hill 3284,1367;
// Slyder farm 3448,1108; Currens farm 2733,−96 — the EDGE ruling; Big RT
// 4076,1869), the sheet-8 south-margin crop (s8_southcav.jpg: "Kilpatrick's
// Division"; Farnsworth's Brigade drawn as 1st W.Va / 1st Vt / 5 N.Y. / 18 Pa
// with "Elder's Bat. E 4 U.S."; Merritt's 6th Pa / 1 U.S. / 2 U.S. and
// Graham's battery symbols at W. M. Currans on the sheet's own neat line),
// the Snell essay's two maps (npshistory.com/series/symposia/
// gettysburg_seminars/9/essay8.pdf pp. 186–187, read this session: Merritt's
// regiments echeloned at/over the south edge; Reilly & Bachman drawn at the
// Emmitsburg Road at charge time; the 1st Texas behind a stone wall south of
// the Bushman farm), and monument/tablet lat/lons off the stonesentinels
// pages converted to battlefield-local meters this session (WGS84→UTM 18N −
// origin 304208/4404534; town-square sanity check landed (4858,6836) vs the
// survey's 4864,6839). All tablet text fetched live 2026-07-08 (exact-URL
// citation convention, Wave 2 forward). Run from tool/:
//   npx vite-node scripts/author-w6-south-cavalry.ts
// Committed as the derivation record for the authored data.
//
// GRAIN ADJUDICATION — brigades, not regiments (plan Task 8 ruling): sheet
// 8's south margin draws Kilpatrick at regiment level, but the plan's
// checklist authors the cluster at brigade grain ("us-cav-farnsworth
// (brigade grain, 1,925…)", "us-cav-merritt") — the regiment-level sheet
// geometry rides the citations as the position attestation, exactly like the
// battalion-level battery lists of Wave 3. On the CSA side the attested
// grain IS battery (the survey names Bachman and Reilly as the movers;
// their War Dept tablets attest their July 3 fire individually):
// csa-btty-bachman and csa-btty-reilly land as `parent: csa-bn-henry`
// children — attested grain rules, in both directions. Henry's parent track
// stays the battalion record (19 guns, Garden's and Latham's batteries
// unmodeled and counted only there — the known-short decomposition rides the
// validator's ±15% strength advisory, the Webb/106th-PA precedent).
//
// THE t=7200 FAMILY HANDOFF (the two-family-levels guard, verified against
// the committed data before authoring): Wave 3's csa-bn-henry-flank-cover
// runs t 420–7200 and its note says BY DESIGN "Wave 6 attaches the post-7200
// fire to the batteries." The children's events below start at exactly
// t0=7200 — parent and child windows TOUCH and never overlap (never the same
// fire at two family levels, battle-format.md "Engagement events"); the
// batteries' unwindowed all-day tablet fire before 15:00 is rendered by the
// battalion's own window, in which they are counted.
//
// SWING ADJUDICATION (tablets weighted over the secondary, the Garnett
// precedent): the survey reads Snell as "Bachman's & Reilly's batteries
// swung south vs Merritt ~15:00+". The War Dept tablets, fetched this
// session, refine that: Bachman "In position here and actively engaged in
// firing upon the Union lines within range" (no move attested); Reilly "Two
// Parrotts moved to right. The other guns engaged in firing upon the Union
// lines within range" — the BATTERY stood while its two-Parrott SECTION
// detached, and Reilly's second marker attests the section's own position
// and fire verbatim ("These guns were detached and first occupied position
// 300 yards west of this hotly engaged with the artillery of the Union
// Cavalry Division down the Emmitsburg Road"). So: both batteries are
// authored STATIC at their tablet positions with post-7200 fire events, the
// section fires as a fixed-SEGMENT event at its tablet-attested coordinates
// (the locked detachment ruling — the Carter's-rifles/Raine's-20-pdr
// pattern; different guns than the battery's remaining four, so battery
// event + section segment is two attested fires, not one fire at two
// grains), and the Snell both-batteries-swung reading rides the citations
// as a carried disagreement, never reconciled.
//
// MERRITT'S EDGE RULING + ARRIVAL SPREAD (plan Task 8; survey §1/§3.4): the
// Currens farm anchor converts to (2733,−96) — 96 m OFF the square's south
// edge; the survey rules the brigade "fights ON the southern edge… his line
// laps both sides." Per the plan ("the edge-lapping is honest: keyframes sit
// at the attested coordinate even though his line laps off-map") the unit
// centre sits AT the Currens anchor, z=−96 — the one authored keyframe off
// the terrain square, carried as an explicit exception in the on-battlefield
// test rather than silently pulled inside. Sheet 8 draws his three regiment
// bars immediately N of the farm (on-map side); Snell's map echelons the
// brigade SW of it (off-map side); the deeper Ridge Rd line is OFF just
// south — the anchor splits the attested spread. ARRIVAL CARRIED AS DATA,
// all clocks, never reconciled: ~11:00 (Wittenberg-derived,
// studycivilwar.wordpress.com) vs ~13:00 (Snell) vs ~15:00 (Wikipedia
// third-day page, fetched live: "arrived at about 3:00 p.m. and took up a
// position striding the Emmitsburg Road, to Farnsworth's left") vs his own
// tablet's arithmetic ("At noon marched four miles on the road to
// Gettysburg… engaged four hours" ≈ on the ground ~13:00–13:30) vs
// Farnsworth's tablet ("being supported at 3 P. M. by the Reserve Cavalry
// Brigade on the left"). The skirmish event's t0=1800 (~13:30) follows the
// tablet's own noon-march arithmetic — the spread rides the note.
//
// EAST CAVALRY FIELD — ZERO UNITS (survey ruling "annotate, never fake";
// plan design decision: research-doc-only, no format element): the
// Gregg–Custer–Stuart fight is 4–5.5 km beyond the east edge. The off-map
// annotation rides the citations of the on-map bodies that attest it:
// us-cav-farnsworth carries the Kilpatrick division tablet's "The Second
// Brigade reported to Gen. Gregg and was engaged on the extreme right"
// (Custer, OFF-MAP east), us-cav-merritt carries the 6th US detached to
// Fairfield (~13 km SW, OFF-MAP, wrecked that afternoon — 242 cas.). No
// unit is placed for Gregg, McIntosh, J.I. Gregg, Custer, Stuart, Hampton,
// Fitz Lee, Chambliss, Jenkins/Witcher, or the five horse batteries.
//
// FIRE FOR THE CLUSTER — the plan's four Union events + the two attested
// battery events + the section segment, nothing else. ATTACH-LEVEL CHECK
// against the existing events, done before authoring: no existing event
// models any fire south of Devil's Den (the corridor musketry is the repulse
// fire at the wall, 7800+; Wave 5 authored exactly three CSA emitters, all
// north; Wave 5's Robertson citation defers "the South Cavalry Field
// exchange" here by name). The CSA musketry side of the Merritt fight (9th
// GA + Black's ~100 SC troopers + armed teamsters + Hart's gun; Wikipedia
// adds the 47th AL and "the 1st South Carolina and artillery" reinforcing)
// stays UNMODELED fire riding the citations — csa-luffman's Wave-5 keyframe
// already carries his brigade's "sent down Emmitsburg Road… repulsing and
// holding in check Union cavalry" attestation, and no survey-named CSA
// musketry emitter exists at battery/brigade grain in this sector.
//
// FARNSWORTH'S CHARGE IS POST-WINDOW (~17:00; sources 5:00 vs 5:30 P.M.,
// spread carried): both tablets clock it 5.30 P. M.; Kilpatrick got word of
// the repulse ~17:00 (survey). The brigade's end keyframe stays DEPLOYED and
// the citation says why; the charge, Farnsworth's death, and the batteries'
// "About 5 P. M. aided in repelling cavalry" windows are all citation
// annotations, never events.
//
// DECLINED — the 1st Texas shift (the plan's decide-at-hand-edit item,
// adjudicated against the Snell essay read this session): Snell DOES give
// geometry (his p. 187 map draws "1 TEXAS" behind a stone wall south of the
// Bushman farm, skirmish line running east toward Plum Run; his text has the
// West Virginians "greeted by soldiers of the 1st Texas Infantry, who had
// taken cover behind a stone wall" — at the ~17:30 charge, post-window), and
// Robertson's tablet attests the detachment ("Early in the day the 1st Texas
// was sent to confront the Union Cavalry threatening the right flank"). NOT
// authored as a unit: no source gives the regiment's July 3 strength (the
// survey's do-not-invent line — Robertson's 560 reconstructed effectives are
// brigade-grain and Wave 5 kept them whole), authoring it as csa-robertson's
// only child would hide the brigade's other three regiments at the
// regiments render tier (a single-child decomposition misrepresents), and
// authoring it parentless would double-count its men against Robertson's
// whole-brigade strength. The shift rides Robertson's Wave-5 citation (which
// defers here by name) and this header records the adjudication; its
// in-window fire (dismounted skirmishing vs Farnsworth's probing) is the
// exchange rendered from the Union side by us-cav-farnsworth-probing, per
// the survey's emitter grain. Also declined: Hart's one CSA gun vs Merritt
// (unmodeled, rides the Merritt citations; no position attested beyond
// Snell's map symbol), and the 5th US mounted charge + Georgia counter as a
// separate event (inside the Merritt-sector skirmish window at the survey's
// chosen grain — "early-to-mid afternoon", unwindowed; rides the note).
//
// STRENGTHS: Farnsworth 1,925 (survey §3.4; Wikipedia third-day page
// verbatim "1,925 troops"; his tablet carries casualties only — 98). Merritt
// UNATTESTED — his tablet carries casualties only (418, of which 275
// captured/missing are predominantly the 6th US wrecked at FAIRFIELD, not
// SCF losses — said in the citation so the number can't be misread):
// reconstructed ~1,300 = the Reserve Brigade less the detached 6th US,
// approximation flagged (the Wave-4 norms pattern). Batteries: Bachman 71
// (the stonesentinels page's strength figure); Elder ~96 and Graham ~144
// reconstructed at ~24 men/gun from their monuments' 4- and 6-gun armament
// lists, flagged (the Wave-2 Dilger precedent); Reilly ~144 from his 6-gun
// armament (two Napoleons, two 10-pdr Parrotts, rifles — one burst July 2, a
// captured Parrott substituted), flagged, strength kept whole while the
// Parrott section is detached (the Wofford outpost-regiment precedent).
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Unit } from "../src/model";
import {
  addUnitsAfter, exportValidated, fireEvent, staticUnit, WindowEndT,
} from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const survey = "docs/research/2026-07-02-full-cast-sources.md §3.4";
const fetched = "fetched 2026-07-08";
const uhq = "https://gettysburg.stonesentinels.com/union-headquarters/";
const cbat = "https://gettysburg.stonesentinels.com/confederate-batteries/";
const snell =
  "Snell, 'A hell of a damned fool' (npshistory.com/series/symposia/gettysburg_seminars/9/essay8.pdf, read 2026-07-08)";

// The Merritt arrival disagreement, carried whole on every keyframe/window
// it governs — all clocks, never reconciled (survey §4 disagreement list).
const merrittSpread =
  "ARRIVAL SPREAD CARRIED, not reconciled (" + survey + "; §4): ~11:00 (Wittenberg-derived, studycivilwar.wordpress.com) vs ~13:00 (" + snell + ") vs ~15:00 (https://en.wikipedia.org/wiki/Battle_of_Gettysburg,_third_day_cavalry_battles, " + fetched + ": 'arrived at about 3:00 p.m. and took up a position striding the Emmitsburg Road, to Farnsworth's left') vs his own tablet's arithmetic ('At noon marched four miles on the road to Gettysburg' ≈ on the ground ~13:00–13:30) vs Farnsworth's tablet ('being supported at 3 P. M. by the Reserve Cavalry Brigade on the left') — either way skirmishing underway before 16:00";

// ---- SECTOR: the Union cluster — Kilpatrick's/Farnsworth's ground ----------
// Survey §3.4; sheet 8 s8_southcav.jpg gives the regiment-level geometry
// (the brigade-grain ruling in the header). Farnsworth's line south of the
// Bushman farm/Bushman Hill ground (survey §1: Bushman Hill 3284,1367,
// Slyder 3448,1108 — 1.1–1.4 km inside the S edge), facing Law's refused
// flank; Elder on the knoll anchored by his own monument (converts to local
// 3353,1743, text 'on a hill southwest of Round Top').

const unionCluster: Unit[] = [
  staticUnit({
    id: "us-cav-farnsworth",
    name: "Farnsworth's Cavalry Brigade, Kilpatrick's Division (1st Brigade, 3rd Division, Cavalry Corps)",
    side: "union", strength: 1925, x: 3350, z: 950, facing: 15,
    frontage_m: 650, depth_m: 100,
    grade: "A-",
    citation:
      "brigade tablet verbatim, July 3: 'Moved to the left to attack the Confederate right and rear arriving about 1 P. M. and engaged with Confederate skirmishers being supported at 3 P. M. by the Reserve Cavalry Brigade on the left. At 5.30 P. M. the 18th Pa. 1st Vt. and 1st West Virginia charged the Confederate left through the woods and among stone fences held by superior forces of Infantry and Artillery but were repulsed with heavy loss including Brig. Gen. Farnsworth killed.' (" +
      uhq + "1st-brigade-3rd-division-cavalry-corps/, " + fetched + ") — arrives/deploys ~13:00 AS THE CANNONADE OPENS = the t=0 pose (" + survey + ", A−); THE MOUNTED CHARGE IS POST-WINDOW (tablet 5.30 P. M.; Kilpatrick got word of the repulse ~17:00, sources 5:00 vs 5:30 P.M. — spread carried): the end keyframe stays DEPLOYED, dismounted and probing, the charge never modeled in-slice; position class: the brigade line south of the Bushman farm/Bushman Hill ground (survey §1 Bushman Hill 3284,1367 / Slyder 3448,1108), sheet 8 s8_southcav.jpg drawing the brigade at REGIMENT level (1st W.Va, 1st Vt, 5 N.Y., 18 Pa with Elder's battery) — regiment geometry rides here, the unit is brigade grain per plan Task 8; his in-window fire is the event us-cav-farnsworth-probing; OFF-MAP ANNOTATION (survey ruling 'annotate, never fake'): the division tablet — 'The Second Brigade reported to Gen. Gregg and was engaged on the extreme right' (" +
      uhq + "3rd-division-cavalry-corps/, " + fetched + ") — is Custer at EAST CAVALRY FIELD, 4–5.5 km beyond the square's east edge with Gregg's division vs Stuart: no unit is placed for that fight; strength 1,925 (" + survey + "; Wikipedia third-day page verbatim '1,925 troops', " + fetched + "; the tablet carries casualties only — 98, mostly the post-window charge)",
  }),
  staticUnit({
    id: "us-cav-merritt",
    name: "Merritt's Reserve Cavalry Brigade (1st Division, Cavalry Corps)",
    side: "union", strength: 1300, x: 2733, z: -96, facing: 10,
    frontage_m: 800, depth_m: 100,
    grade: "A-",
    citation:
      "brigade tablet verbatim, July 3: 'At noon marched four miles on the road to Gettysburg met Confederate detachments and for more than a mile drove them from stone fences barricades and other positions being engaged four hours and until the operations were brought to a close by heavy rain.' (" +
      uhq + "reserve-brigade-1st-division-cavalry-corps/, " + fetched + ") — THE EDGE-LAPPING IS HONEST (plan Task 8; survey §1 'Currens farm, Emmitsburg Rd (Merritt) | 2733,−96 | EDGE — straddles the S boundary'): the unit centre sits AT the attested Currens anchor, z=−96, the one keyframe off the terrain square — sheet 8 s8_southcav.jpg draws his 6th Pa / 1 U.S. / 2 U.S. immediately N of the farm (on-map side), Snell's map echelons the brigade SW of it (off-map side), and the deeper Ridge Rd line (2373,−754) is OFF just south: his line laps both sides and the anchor splits the attested spread; " +
      merrittSpread + "; dismounted skirmishing vs the 9th GA + Black's ~100 SC troopers + armed teamsters + Hart's gun (" + survey + "; the 5th US mounted charge and the Georgia counter ride the event note); his fire is the event us-cav-merritt-skirmish; OFF-MAP ANNOTATION: the 6th US Cavalry, detached, was wrecked at FAIRFIELD ~13 km SW that afternoon (242 cas., no precise clock attested — survey §3.4) — not placed; strength UNATTESTED: the tablet carries casualties only (418, of which 275 captured/missing are predominantly the 6th US at Fairfield, NOT South Cavalry Field losses) — reconstructed ~1,300 = the Reserve Brigade (1st/2nd/5th US, 6th PA) less the detached 6th US, approximation flagged",
  }),
  staticUnit({
    id: "us-btty-elder",
    name: "Elder's Battery E, 4th US Artillery",
    side: "union", strength: 96, x: 3353, z: 1743, facing: 60,
    frontage_m: 90, depth_m: 25,
    grade: "A-",
    citation:
      "monument verbatim, July 3: 'Arrived on the field and took position on a hill southwest of Round Top and engaged under Brig. General E. J. Farnsworth in the afternoon against the Confederate right.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/4th-us-artillery-battery-e/, " + fetched + ") — Lieut. Samuel S. Elder; four 3-inch Rifles; position: the monument's own anchor, converted to local (3353,1743) this session — the knoll S/SW of Big Round Top (" + survey + ": 'shelling Law's line during the window — A−'); sheet 8 s8_southcav.jpg draws 'Elder's Bat. E 4 U.S.' inside Farnsworth's regiment cluster (the sheet reads south of the monument knoll — position-class spread carried); his fire is the event us-btty-elder-shelling; the ~17:00+ charge support (5th NY kept in his support per Snell; 'About 5 P. M.' on the CSA battery tablets) is POST-window; casualties killed 1 man (monument); NO personnel figure on the page — strength reconstructed at ~24 men/gun from the 4-gun armament, flagged (the Wave-2 Dilger precedent)",
  }),
  staticUnit({
    id: "us-btty-graham",
    name: "Graham's Battery K, 1st US Artillery",
    side: "union", strength: 144, x: 2650, z: 120, facing: 10,
    frontage_m: 110, depth_m: 25,
    grade: "A-",
    citation:
      "monument verbatim, July 3: 'Arrived on the field and took position on the left with cavalry and engaged during the attack of Brig. General E. J. Farnsworth's and Brig. General W. Merritt's Brigades on the Confederate right.' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/1st-us-artillery-battery-k/, " + fetched + ") — Capt. William M. Graham; six 3-inch Rifles; position class: with Merritt astride the Emmitsburg Rd at the Currens ground — sheet 8 s8_southcav.jpg draws his battery symbols immediately NW of the farm, ON-map (coordinate inferred within the class); firing attested — A− (" + survey + ", Snell); his fire is the event us-btty-graham-fire; " + merrittSpread + "; NO personnel figure on the page — strength reconstructed at ~24 men/gun from the 6-gun armament, flagged (the Wave-2 Dilger precedent)",
  }),
];

// ---- SECTOR: the CSA counter — Henry's batteries as children ---------------
// Survey §3.4 CSA response; the SWING ADJUDICATION and the t=7200 handoff in
// the header. Both tablet anchors converted this session: Bachman
// (2804,2250), Reilly (2769,1831) — the battalion's S/SE-facing line south
// of csa-bn-henry's own tablet anchor (2980,2280).

const henryChildren: Unit[] = [
  staticUnit({
    id: "csa-btty-bachman",
    name: "Bachman's Battery, Henry's Battalion (German Artillery, SC)",
    side: "confederate", strength: 71, x: 2804, z: 2250, facing: 190,
    frontage_m: 90, depth_m: 25,
    parent: "csa-bn-henry",
    grade: "A-",
    citation:
      "War Dept tablet verbatim, July 3: 'In position here and actively engaged in firing upon the Union lines within range. About 5 P. M. aided in repelling cavalry under Brig. Gen. Farnsworth which had charged into the valley between this point and Round Top.' (" +
      cbat + "german-south-carolina-artillery/, " + fetched + ") — Capt. William K. Bachman; four Napoleons; 71 men (the page's strength figure); the tablet anchor converts to local (2804,2250) — authored STATIC there: SWING DISAGREEMENT CARRIED, not reconciled (tablet weighted, the Garnett precedent): the survey reads Snell as 'Bachman's & Reilly's batteries swung south vs Merritt ~15:00+' and Snell's charge-time map draws the battery at the Emmitsburg Road, but his own tablet attests 'In position HERE' with no move (" + survey + "; " + snell + "); child of csa-bn-henry (attested grain is battery here, plan Task 8) — his fire before ~15:00 is inside the battalion's flank-cover window, his own post-handoff fire is the event csa-btty-bachman-fire (t0=7200, the family handoff); the 'About 5 P. M.' Farnsworth-repulse fire is POST-window, annotation only",
  }),
  staticUnit({
    id: "csa-btty-reilly",
    name: "Reilly's Battery, Henry's Battalion (Rowan Artillery, NC)",
    side: "confederate", strength: 144, x: 2769, z: 1831, facing: 175,
    frontage_m: 100, depth_m: 25,
    parent: "csa-bn-henry",
    grade: "A-",
    citation:
      "War Dept tablet verbatim, July 3: 'Two Parrotts moved to right. The other guns engaged in firing upon the Union lines within range. About 5 P. M. aided in repelling cavalry under Brig. Gen. Farnsworth which had charged into the valley between this point and Round Top.' (" +
      cbat + "rowan-north-carolina-artillery/, " + fetched + ") — Capt. James Reilly; the tablet anchor converts to local (2769,1831) — the BATTERY authored STATIC there while its two-Parrott SECTION detached south (the tablet's 'moved to right'): the section's own second marker attests its position and fire verbatim ('These guns were detached and first occupied position 300 yards west of this hotly engaged with the artillery of the Union Cavalry Division down the Emmitsburg Road' — marker 2 converts to (2914,1713), the section 274 m west at (2640,1713)), authored as the fixed-segment event csa-btty-reilly-parrott-section per the locked detachment ruling; SWING DISAGREEMENT CARRIED as on csa-btty-bachman (survey/Snell 'swung south ~15:00+' vs the tablet's battery-stands-section-detaches — tablet weighted); child of csa-bn-henry (attested grain is battery, plan Task 8) — fire before ~15:00 is inside the battalion's window, his remaining guns' post-handoff fire is the event csa-btty-reilly-fire (t0=7200); the 'About 5 P. M.' charge repulse (the section 'placed here' at marker 2 for it) is POST-window, annotation only; 6-gun armament (two Napoleons, two 10-pdr Parrotts, rifles — one burst July 2, a captured 10-pdr Parrott substituted); NO personnel figure — strength ~24 men/gun, flagged (the Dilger precedent), kept whole while the section is detached (the Wofford outpost precedent)",
  }),
];

// ---- events: the plan's four Union windows + the attested CSA three --------
// Attach-level check in the header. All windows end at the window end —
// both fights ran past 16:00 (Merritt's tablet: 'engaged four hours…until
// the operations were brought to a close by heavy rain'; the charge and its
// repulse are post-window).

const events: EngagementEvent[] = [
  fireEvent({
    id: "us-cav-farnsworth-probing", kind: "musketry",
    t0: 600, t1: WindowEndT, unitId: "us-cav-farnsworth", confidence: "documented",
    citation:
      "brigade tablet verbatim: 'arriving about 1 P. M. and engaged with Confederate skirmishers' (" +
      uhq + "1st-brigade-3rd-division-cavalry-corps/, " + fetched + "; " + survey + " — A−, 'dismounted probing of Law's line')",
    note:
      "dismounted probing, skirmish grain, LOW intensity — t0=600 gives the ~13:00 arrival a deployment interval (clock inferred within the tablet's 'about 1 P. M.'); the 1st VT squadron/Bushman-buildings episode rides here: the squadron took the Bushman buildings and was ejected when the 1st Texas arrived ('made some headway earlier in the afternoon' — " + snell + "); the CSA side of this exchange (1st TX skirmish line behind the stone wall S of Bushman farm) is unmodeled and rides csa-robertson's citation, per the survey's emitter grain; the 5:30 P.M. mounted charge is POST-window",
  }),
  fireEvent({
    id: "us-cav-merritt-skirmish", kind: "musketry",
    t0: 1800, t1: WindowEndT, unitId: "us-cav-merritt", confidence: "documented",
    citation:
      "brigade tablet verbatim: 'met Confederate detachments and for more than a mile drove them from stone fences barricades and other positions being engaged four hours' (" +
      uhq + "reserve-brigade-1st-division-cavalry-corps/, " + fetched + "; " + survey + " — 'skirmishing underway before 16:00' on every arrival reading)",
    note:
      "dismounted carbine skirmish astride the Emmitsburg Rd vs the 9th GA + Black's ~100 SC troopers + armed teamsters + Hart's gun (all unmodeled, riding the citations; Wikipedia adds the 47th AL and '1st South Carolina and artillery' reinforcing); TIME-SPREAD NOTE (plan Task 8): t0=1800 (~13:30) follows the tablet's own noon-march arithmetic — the ~11:00 reading would put fire underway at t=0, the ~15:00 reading at t≈7200; all clocks in the unit citation, never reconciled; the 5th US mounted charge and the Georgia counter ('early-to-mid afternoon', unwindowed) fall inside this window at the survey's chosen grain",
  }),
  fireEvent({
    id: "us-btty-elder-shelling", kind: "artillery_fire",
    t0: 600, t1: WindowEndT, unitId: "us-btty-elder", confidence: "documented",
    citation:
      "monument verbatim: 'engaged under Brig. General E. J. Farnsworth in the afternoon against the Confederate right' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/4th-us-artillery-battery-e/, " + fetched + "; " + survey + " — 'shelling Law's line during the window — A−')",
    note:
      "four 3-inch Rifles on the knoll vs Law's refused flank; t0=600 with the brigade's deployment (clock inferred within 'about 1 P. M.'/'in the afternoon'); the charge-support fire ~17:00+ is post-window",
  }),
  fireEvent({
    id: "us-btty-graham-fire", kind: "artillery_fire",
    t0: 1800, t1: WindowEndT, unitId: "us-btty-graham", confidence: "documented",
    citation:
      "monument verbatim: 'engaged during the attack of Brig. General E. J. Farnsworth's and Brig. General W. Merritt's Brigades on the Confederate right' (https://gettysburg.stonesentinels.com/us-regulars/us-artillery/1st-us-artillery-battery-k/, " + fetched + "; " + survey + " — A−)",
    note:
      "six 3-inch Rifles with Merritt at the Currens ground; window rides Merritt's (t0=1800, the tablet's noon-march arithmetic — the arrival spread governs this window too and rides the unit citations); his counter-battery opposite number is Reilly's detached Parrott section ('hotly engaged with the artillery of the Union Cavalry Division down the Emmitsburg Road')",
  }),
  fireEvent({
    id: "csa-btty-bachman-fire", kind: "artillery_fire",
    t0: 7200, t1: WindowEndT, unitId: "csa-btty-bachman", confidence: "documented",
    citation:
      "War Dept tablet verbatim: 'In position here and actively engaged in firing upon the Union lines within range.' (" +
      cbat + "german-south-carolina-artillery/, " + fetched + "; " + survey + " — the CSA counter vs Merritt ~15:00+, A−)",
    note:
      "THE t=7200 FAMILY HANDOFF (battle-format.md: never the same fire at two family levels): csa-bn-henry-flank-cover ends at t=7200 BY DESIGN and this window begins exactly there — the battery's unwindowed all-day tablet fire before ~15:00 is rendered by the battalion's window, in which its guns are counted; post-handoff the battery fires as itself (the survey's ~15:00+ southward counter-move clock); the 'About 5 P. M.' Farnsworth-repulse fire is post-window",
  }),
  fireEvent({
    id: "csa-btty-reilly-fire", kind: "artillery_fire",
    t0: 7200, t1: WindowEndT, unitId: "csa-btty-reilly", confidence: "documented",
    citation:
      "War Dept tablet verbatim: 'The other guns engaged in firing upon the Union lines within range.' (" +
      cbat + "rowan-north-carolina-artillery/, " + fetched + "; " + survey + " — the CSA counter vs Merritt ~15:00+, A−)",
    note:
      "the battery's four remaining guns (the two-Parrott section fires separately as csa-btty-reilly-parrott-section from its own attested position — different guns, two attested fires, not one fire at two grains); t0=7200 per the family handoff, as csa-btty-bachman-fire",
  }),
  fireEvent({
    id: "csa-btty-reilly-parrott-section", kind: "artillery_fire",
    t0: 7200, t1: WindowEndT,
    segment: { x: 2610, z: 1713, x2: 2670, z2: 1713 },
    confidence: "documented",
    citation:
      "section marker verbatim, July 3: 'These guns were detached and first occupied position 300 yards west of this hotly engaged with the artillery of the Union Cavalry Division down the Emmitsburg Road.' (" +
      cbat + "rowan-north-carolina-artillery/ marker 2, " + fetched + ") — marker 2 converts to local (2914,1713); the attested position is 300 yards (274 m) west: segment centred at (2640,1713)",
    note:
      "the locked detachment ruling (battle-format.md 'Segment-emitter migration and attested detachments' — the Carter's-rifles/Raine's-20-pdr pattern): Reilly's two 10-pdr Parrotts fire from the section's OWN tablet-attested coordinates while the battery unit stands at its tablet position; opposite number Graham/Elder down the Emmitsburg Rd; t0=7200 per the family handoff — the tablet's 'moved to right' clock is unattested, the window follows the survey's ~15:00+ counter-move spine, earlier section fire (if any) subsumed in the battalion's flank-cover window by design; at the post-window charge the section 'were placed here' (marker 2) for the repulse — annotation only",
  }),
];

// ---- assemble, gate, export -------------------------------------------------
// Henry's children land immediately after their parent (the A5 family
// pattern); the Union cluster lands at the end of the unit list after
// us-arty-reserve-park (the Wave-2 tail). Pin 184 → 190.

battle = addUnitsAfter(battle, "csa-bn-henry", henryChildren);
battle = addUnitsAfter(battle, "us-arty-reserve-park", unionCluster);
battle.events = [...(battle.events ?? []), ...events]; // exportValidated sorts canonically

writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events?.length ?? 0} events (wave 6: ${henryChildren.length + unionCluster.length} units, ${events.length} fire windows)`);
