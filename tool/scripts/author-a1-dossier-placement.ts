// Authoring Wave A1 — the audit's "place" step: enrich the July 3 afternoon
// tabletop from the completed unit dossiers (reconstruction/dossiers/, passes
// 1-4), per unit-truth-spec.md ("authoring consumes dossiers") and the adopted
// July 3 afternoon anchor chain (docs/reconstruction/audit/
// anchor-chain-proposal.md §1.1, ED-24). Run from tool/:
//   npx vite-node scripts/author-a1-dossier-placement.ts
// Committed as the derivation record for the authored data. Wave report:
// docs/reconstruction/audit/authoring-wave-a1.md (adds, cuts, rationale).
//
// HARD RULES this script observes:
// - The 13 Angle-cast units (angle.bundle.json cast: csa-garnett/kemper/
//   armistead/fry, us-webb/69pa/71pa/72pa, us-btty-cushing/cowan/arnold,
//   us-hall, us-stannard) are NOT touched — their keyframes are load-bearing
//   for the compiled bundle (Gate P5 reconciliation) and the film.
// - NEVER invent motion: every added/edited keyframe and event cites the
//   dossier's cited facts; attested stillness stays still; tier-C/D timing
//   authors at the preferred time with `inferred` confidence.
// - Timing frame: t=0 = startTime 46800 = nominal 13:00 (the chain reads it
//   13:07, shippedSkewMinutes -7 is a LABEL note — nothing is re-timed);
//   CA-J3A-1 signal t=420 · CA-J3A-4 slacken ~t=5400 · CA-J3A-5 quiet t=7200 ·
//   CA-J3A-6 step-off t=7500 · CA-J3A-9 wall t=8700 · CA-J3A-10 repulse t=9000,
//   Wilcox/Lang 15:30-16:00.
//
// WHAT THE WAVE DOES (headlines; full table in the wave report):
// 1. Wilcox & Lang re-placed and re-strengthed from their own OR reports
//    (or-27-2-wilcox "about 1,200"; or-27-2-lang "near 700 strong" minus his
//    own July 2 ~300) — Lang moved to Wilcox's LEFT per "I moved up with his
//    left" + Bachelder j3-01; the supporting advance extended to the
//    documented ravine line below the McGilvery crest (the build's ~590 m
//    displacement understated the documented advance — dossier flag).
// 2. Dearing's battalion placed on its daybreak-advanced road-crest line
//    (or-27-2-dearing + tablet + Bachelder j3-03 per-battery labels) and his
//    cannonade window shortened to the documented ammunition famine
//    ("completely exhausted ... a very heavy fire for upward of an hour
//    without being able to fire a single shot").
// 3. McGilvery's deliberate reply authored (his OR p. 884: hold ~1.5 h, then
//    "a slow, well-directed fire from all the guns under my command") — six
//    per-battery windows t 5820-7200 on the line-grain attestation.
// 4. Miller's advanced section (Washington Artillery) fires from its
//    documented 450-yd-forward position as a fixed-segment detachment event
//    (battle-format.md segment-emitter rule) — the CSA close-range phase of
//    the repulse window.
// 5. ED-50 executed: the 8th Ohio's inverted casualty shape (~45 of 102 fell
//    on the picket line BY NOON, pre-window) — t=0 strength 209→164, flat
//    attrition then a moderate flank-action spike.
// 6. Sherrill's brigade gets its one in-window leg (tablet: "At 3 P. M. ...
//    moved up to the line of the Second Brigade", A vs CA-J3A-5).
// 7. ED-38/ED-44 executed: Eshleman re-strengthed 240→338; Garnett's
//    battalion silence upgraded from tablet-weight to his own OR verbatim.
// 8. Dossier-apportioned casualty decay added where a July-3 split is
//    attested (Eshleman 45, Dearing 14+, Pegram 47, Dow 13, Fitzhugh 7,
//    Parsons 9, Cooper 1); units without a primary day split stay flat —
//    recorded as cuts, never invented.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, EngagementEvent, Keyframe } from "../src/model";
import { fireEvent, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
const battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

// The Angle-cast guard: this wave must never touch these units' keyframes.
const ANGLE_CAST = new Set([
  "csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
  "us-webb", "us-69pa", "us-71pa", "us-72pa",
  "us-btty-cushing", "us-btty-cowan", "us-btty-arnold",
  "us-hall", "us-stannard",
]);

const byId = new Map(battle.units.map((u) => [u.id, u]));
function unit(id: string) {
  if (ANGLE_CAST.has(id))
    throw new Error(`wave A1 guard: '${id}' is Angle-cast and untouchable`);
  const u = byId.get(id);
  if (!u) throw new Error(`wave A1: unit '${id}' missing`);
  return u;
}

// ---------------------------------------------------------------------------
// 1. Wilcox's brigade — re-strength + the documented ravine advance
//    (reconstruction/dossiers/csa-3c-and-1-wilcox.md; or-27-2-wilcox No. 537)
// ---------------------------------------------------------------------------
const wilcoxLaunchNote =
  "LAUNCH-CLOCK ENVELOPE CARRIED, NOT RECONCILED (dossier EC4.2): Wilcox's own 'twenty or thirty minutes' after " +
  "the advance (A vs CA-J3A-6 ⇒ ~15:25-15:35) vs Lang's 'soon after General Pickett's troops retired' (after the " +
  "repulse) vs Longstreet's forward-then-halt — one envelope, 'all consistent with launch ≈ crest-of-repulse'; the " +
  "built 15:44 leg is kept inside CA-J3A-10's 15:30-16:00 adoption because the 16th VT reversal keyframes and the " +
  "Stannard (Angle-cast, untouchable) second-flank-action keyframes pin the contact phase at 10200-10500";

unit("csa-wilcox").keyframes = [
  { t: 0, x: 3130, z: 4450, facing: 80, formation: "line", strength: 1200, confidence: "documented",
    citation: "or-27-2-wilcox (No. 537): before sunrise 'ordered out to support artillery under the command of Colonel Alexander ... formed in line parallel to the Emmitsburg turnpike and about 200 yards from it, artillery being in front, much of it on the road'; Bachelder j3-01 'WILCOX ALA.' ~(3100,4400) ±120 (04:00-08:00 window brackets the before-sunrise move) — reconstruction/dossiers/csa-3c-and-1-wilcox.md EC3.1; STRENGTH IS THE JULY 3 PRIMARY: 'My brigade, about 1,200 in number' (or-27-2-wilcox) — the pre-battle 1,777 (Stone Sentinels monument; conflicting 1,725 extraction flagged) minus the July 2 PRIMARY loss of 577 closes to 1,200 exactly, the two primaries mutually consistent to the man (dossier EC2/EC6); July 3 order right-to-left 9th-10th-11th-8th-14th Alabama (or-27-2-wilcox)" },
  { t: 7200, x: 3130, z: 4450, facing: 80, formation: "line", strength: 1195, confidence: "documented",
    citation: "bombardment received at the support line: 'suffered comparatively little, probably less than a dozen men killed and wounded' (or-27-2-wilcox), against Anderson's 'suffering the most seriously' of his division (or-27-2-anderson-rh) — both carried, compatible (dossier EC5.1); decay shape inferred inside the attested <12" },
  { t: 9840, x: 3130, z: 4450, facing: 80, formation: "line", strength: 1188, confidence: "inferred",
    citation: "the delayed launch: 'The advance had not been made more than twenty or thirty minutes, before three staff officers in quick succession ... gave me orders to advance to the support of Pickett's division' (or-27-2-wilcox); Alexander concurs ('ordered forward 20 minutes or more later'); " + wilcoxLaunchNote },
  { t: 10380, x: 4060, z: 4290, facing: 85, formation: "line", strength: 1090, confidence: "inferred",
    citation: "farthest advance = THE DOCUMENTED RAVINE LINE below the McGilvery crest: 'near the hill upon which were the enemy's batteries and intrenchments' (or-27-2-wilcox); alexander-1907 'several hundred yards beyond our guns ... a small ravine' — the previous build displacement (~590 m to x=3720) understated the documented advance (dossier EC3.3 authoring flag, executed this wave); ~940 m under 'all of the enemy's terrible artillery that could bear ... from both flanks and directly in front' — the McGilvery-line object record; leg pace ~1.75 m/s (quick/double-quick mix under fire), position class ±80 m" },
  { t: 10500, x: 3800, z: 4330, facing: 260, formation: "scattered", strength: 1040, confidence: "inferred",
    citation: "withdrawal on his own judgment ('my small force could do nothing save to make a useless sacrifice of themselves'), retired under artillery fire (or-27-2-wilcox); 'engaged but a few minutes' at the ravine — no musketry phase, the July 3 loss is artillery-dominated (dossier EC5.4/EC6)" },
  { t: 10800, x: 3400, z: 4430, facing: 260, formation: "scattered", strength: 996, confidence: "inferred",
    citation: "still retiring at the window end — 'My line was reformed on the ground it occupied before it advanced' (or-27-2-wilcox) completes POST-window (Alexander: 'moved by his right flank and making a circuit regained our lines at the Peach Orchard'); END STRENGTH ON THE PER-DAY PRIMARY: July 3 loss 204 (or-27-2-wilcox; Alexander concurs 'His loss in this charge was 204 killed and wounded') ⇒ 1,200 − 204 = 996; the return 51/469/257 = 777 whole-battle matches (or-27-2-anv-return p. 343)" },
];

// ---------------------------------------------------------------------------
// 2. Lang's Florida brigade — moved to Wilcox's LEFT + per-day re-strength
//    (reconstruction/dossiers/csa-3c-and-4-lang.md; or-27-2-lang No. 545)
// ---------------------------------------------------------------------------
unit("csa-lang").keyframes = [
  { t: 0, x: 3190, z: 4640, facing: 80, formation: "line", strength: 400, confidence: "documented",
    citation: "PLACED ON WILCOX'S LEFT (the previous build had the brigade on his right): 'About 7 a.m. General Wilcox moved forward ... and, in accordance with orders, I moved up with his left, and put my command in front and at the foot of the hill upon which the batteries were in position, at the same time advancing my skirmishers to the crest of the next hill' (or-27-2-lang) — the below-the-guns anchor; Bachelder j3-01 'PERRY FLA.' ~(3230,4820) ±120 draws the pre-07:00 state NORTH of Wilcox (reconstruction/dossiers/csa-3c-and-4-lang.md EC3.1); position class ±100 m; STRENGTH: 'The brigade went into action near 700 strong' (or-27-2-lang, whole battle) minus his own July 2 figure 'about 300' ⇒ ~400 on July 3 (tier-B subtraction on two primaries, dossier EC6); the smallest brigade in the assault's orbit" },
  { t: 7200, x: 3190, z: 4640, facing: 80, formation: "line", strength: 397, confidence: "inferred",
    citation: "under the bombardment: 'remained quietly until nearly 2 p.m., when the batteries opened a furious bombardment ... which lasted till nearly 4 p.m.' (or-27-2-lang, the −53 late-stated clock pole, profiled rule 4); no brigade bombardment casualty figure exists (dossier EC5.1 open) — decay held near-flat" },
  { t: 9840, x: 3190, z: 4640, facing: 80, formation: "line", strength: 395, confidence: "inferred",
    citation: "the echelon's launch by standing conformity order: 'Soon after General Pickett's troops retired behind our position, General Wilcox began to advance, and, in accordance with previous orders to conform to his movements, I moved forward also' (or-27-2-lang) — trigger class, no new order by design; sequencing envelope shared with Wilcox (dossier conflict 1), leg time slaved to the Wilcox spine" },
  { t: 10380, x: 4020, z: 4430, facing: 85, formation: "line", strength: 340, confidence: "inferred",
    citation: "advanced on Wilcox's left 'under a heavy fire from artillery, but without encountering any infantry until coming to the skirt of woods at the foot of the heights' (or-27-2-lang; alexander-1907 'a small ravine ... some undergrowth') — the woods-skirt line below the McGilvery crest, left of Wilcox's ravine position; ~855 m at ~1.6 m/s; the 16th Vermont's reversed front closes on this flank (us-16vt documented keyframe t=10380 at (4180,4360); veazey-or)" },
  { t: 10500, x: 3830, z: 4470, facing: 260, formation: "scattered", strength: 280, confidence: "inferred",
    citation: "THE LEFT-FLANK COLLAPSE: 'a heavy body of infantry advanced upon my left flank ... To remain ... was certain annihilation'; the retreat order 'failed to reach men firing from the rocks, and [they] remained there until captured'; 'not in time to save a large number of the Second Florida Infantry, together with their colors, from being cut off and captured' (or-27-2-lang) — the capture pocket concentrates the day's 205 missing here (dossier EC6); Capt. McCaslan (AAAG) k during the retreat [D pin]" },
  { t: 10800, x: 3450, z: 4590, facing: 260, formation: "scattered", strength: 245, confidence: "inferred",
    citation: "'Falling back to our artillery, we reformed in our old line' (or-27-2-lang) — the reform completes post-window (night: withdrawn to the original woods line by Anderson's order); END STRENGTH ON THE PER-DAY SPLIT: July 3 ≈ 155 of the whole-battle 455 = 33k/217w/205m (or-27-2-lang = or-27-2-anv-return p. 343 EXACT; July 2 'about 300' his own figure; Alexander: 'Perry's loss was about proportional [to Wilcox's 204], with some prisoners in addition') ⇒ ~400 − 155 = 245; 65% whole-battle loss rate, the highest documented in the dossier batch" },
];

// ---------------------------------------------------------------------------
// 3. Dearing's battalion — the daybreak-advanced road-crest line + decay
//    (reconstruction/dossiers/csa-1c-bn-dearing.md; or-27-2-dearing No. 442)
// ---------------------------------------------------------------------------
const dearingPosition =
  "PLACED ON THE DAYBREAK-ADVANCED ROAD-CREST LINE (the previous build held the start-line centroid (3140,4600) all window): " +
  "early July 3 'marched to the field of battle' into position 'on the crest of the hill immediately in front of the enemy's " +
  "position' (or-27-2-dearing); tablet 'advanced to the front about daybreak' — the advance COMPLETES pre-window; Bachelder " +
  "j3-03 per-battery labels CASKIE (~3610,4480) · BLOUNT (~3670,4550) · MACON (~3620,4310) along the road-crest line between " +
  "the Spangler and Codori farms (reconstruction/dossiers/csa-1c-bn-dearing.md EC3.2-4; 'the two anchors are different " +
  "moments, not a conflict'), centroid adopted (3630,4450) ±60 m; BATTALION RELATIONS PRIMARY: 'On my left and rear was " +
  "Colonel Cabell's Artillery Battalion, and on my right and rear was the Washington Artillery Battalion' (or-27-2-dearing) — " +
  "Dearing forwardmost of the First Corps arc; 18 guns (tablet verbatim); Maj. Read w (head, shell fragment) July 3 morning " +
  "skirmish phase [D pin]";

unit("csa-bn-dearing").keyframes = [
  { t: 0, x: 3630, z: 4450, facing: 78, formation: "line", strength: 432, confidence: "documented",
    citation: dearingPosition + "; strength 432 = B&M-type hop per ED-38 (no primary return)" },
  { t: 7200, x: 3630, z: 4450, facing: 78, formation: "line", strength: 424, confidence: "inferred",
    citation: "cannonade + the famine: fire from the signal until ammunition 'became completely exhausted, excepting a few round in my rifled guns', then 'a very heavy fire for upward of an hour without being able to fire a single shot' (or-27-2-dearing) — the silent-under-fire hour is the CSA counterpart of Hazard's exhaustion record and the physical substance of CA-J3A-2/3; decay shape inferred inside the attested per-battery losses" },
  { t: 10800, x: 3630, z: 4450, facing: 78, formation: "line", strength: 418, confidence: "inferred",
    citation: "holds the crest line to the window end (his own report still driving back infantry 'about 6 p.m.' vs tablet withdrawal 'about 4 P. M.' — the two-hour conflict CARRIED, not authored: the tablet's 16:00 sits at the window edge and neither reading is strong enough to move the block, rule 4); END STRENGTH ON THE PER-BATTERY PRIMARY: Stribling 3 w + Macon 3 k / 3 w + Blount 5 k&w = 14 men (+ 30 horses) all July 3 (or-27-2-dearing p. 389); CASKIE'S FIGURE IS AN EXTRACTION GAP, not attested-absent — the battalion aggregate may exceed 14 (dossier EC6 flag)" },
];

// ---------------------------------------------------------------------------
// 4. Eshleman's Washington Artillery — ED-38 strength + all-July-3 decay
//    (reconstruction/dossiers/csa-1c-bn-wash-eshleman.md; or-27-2-eshleman)
// ---------------------------------------------------------------------------
{
  const kfs = unit("csa-bn-eshleman").keyframes;
  const base = kfs[0]!;
  const ed38 =
    "ED-38 ADOPTED STRENGTH: 338 men, 10 guns (addressing-gettysburg-oob, B&M-type figures, secondary hop cited per the " +
    "cite-the-hop rule; '340 men' SNIPPET variant recorded; the previous build's 240 retired) — per company: 1st 77 (1 " +
    "Napoleon, co-operated all day with Miller), 2nd 80, 3rd (Miller) 92, 4th 80; armament tablet + OR concur 8 Napoleons + " +
    "2 12-pdr howitzers (reconstruction/dossiers/csa-1c-bn-wash-eshleman.md EC2)";
  unit("csa-bn-eshleman").keyframes = [
    { ...base, strength: 338, citation: base.citation + "; " + ed38 },
    { ...base, t: 7200, strength: 315, confidence: "inferred",
      citation: "bombardment fought from the signal guns (CA-J3A-1, Miller's two guns) through the duel — 'my men stood bravely to th[eir] work' (or-27-2-eshleman); Richardson's 2nd Co. served a captured Union 3-inch rifle until axle-struck (marker + OR — an eleventh tube, documented); decay shape inferred inside the attested single-day total; Capt. Norcom w 'early in the day' [pin]" },
    { ...base, t: 10800, strength: 293, confidence: "inferred",
      citation: "END STRENGTH ON THE BATTALION RETURN, ALL JULY 3 (clean single-day): 'Wounded, 3 officers. Killed, 3; wounded, 23, and missing, 16, non-commissioned officers and privates; 37 horses killed and disabled; 3 guns disabled; 1 limber blown up' (or-27-2-eshleman verbatim; tablet folds to 3/26/16 = 45) ⇒ 338 − 45 = 293; horse losses concentrated on Miller's advanced guns — 'From loss of horses but one gun could then be used' (marker; see the csa-wa-miller-advanced-section event); withdrawal to park at dark is post-window" },
  ];
}

// ---------------------------------------------------------------------------
// 5. Pegram's battalion — the only per-day CSA battalion primary
//    (reconstruction/dossiers/csa-3c-bn-pegram.md; or-27-2-brunson)
// ---------------------------------------------------------------------------
{
  const kfs = unit("csa-bn-pegram").keyframes;
  const base = kfs[0]!;
  unit("csa-bn-pegram").keyframes = [
    base,
    { ...base, t: 7200, strength: 445, confidence: "inferred",
      citation: "the July 3 cannonade fought from the Jul 2-3 line right of the Fairfield pike (~1,400 yd from the Union guns); decay shape inferred inside the attested July 3 total; rounds 3,800 over three days (or-27-2-brunson verbatim; tablet concurs)" },
    { ...base, t: 10800, strength: 433, confidence: "inferred",
      citation: "END STRENGTH ON THE PER-DAY PRIMARY (the only one in the CSA battalion set): July 3 = 10 k / 37 w (+ 38 horses) (or-27-2-brunson; reconstruction/dossiers/csa-3c-bn-pegram.md EC6) ⇒ 480 − 47 = 433; the tablet's '47' reproduces ONLY the July 3 row as if it were the battle total — conflict carried (ED-43: tablet casualty lines are day-scoped until corroborated)" },
  ];
}

// ---------------------------------------------------------------------------
// 6. Garnett's battalion — ED-44: the silence closes on his own OR
//    (reconstruction/dossiers/csa-3c-bn-garnett.md)
// ---------------------------------------------------------------------------
{
  const upgrade =
    " — RESOLVED PRIMARY (dossier pass 2; ED-44 attested-silence rendering rule): Garnett's own OR verbatim — the rifle " +
    "group July 3 'did not fire a single shot, having received orders to that effect'; the smoothbores 'bore no part in " +
    "these actions' (or-27-2-garnett-jj via reconstruction/dossiers/csa-3c-bn-garnett.md EC5) — the fired-or-not " +
    "disagreement carried above CLOSES toward silence at primary grade; render present-but-not-firing, muzzles cold; the " +
    "15 idle tubes subtract from every cannonade gun-count claim (pass-2 ledger)";
  for (const k of unit("csa-bn-garnett").keyframes)
    k.citation = (k.citation ?? "") + upgrade;
}

// ---------------------------------------------------------------------------
// 7. 8th Ohio — ED-50: the inverted casualty shape
//    (reconstruction/dossiers/us-ii-3-1-carroll.md; or-27-1-sawyer)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-8oh").keyframes;
  const [k0, k1, k2] = kfs as [Keyframe, Keyframe, Keyframe];
  k0.strength = 164;
  k0.citation = k0.citation +
    " — ED-50 EXECUTED (this wave): '4 of my men having been killed, and 1 captain, 1 lieutenant, the sergeant-major, and " +
    "38 men wounded' BY NOON JULY 3 on the picket line (or-27-1-sawyer) — ≈45 of the whole-battle 102 (report total = " +
    "return row EXACT, or-27-1-union-return p. 176) fell BEFORE this window opens ⇒ t=0 strength 209 − 45 = 164; the " +
    "famous flank action cost the regiment LESS than its 40-hour picket tour — flat-attrition-then-moderate-spike shape, " +
    "superseding the climax-spike default (reconstruction/dossiers/us-ii-3-1-carroll.md EC6)";
  k1.strength = 152;
  k1.citation = k1.citation +
    "; ED-50 shape: cannonade attrition on the picket line — 'Soon after 2 p.m. the enemy opened a terrific fire from " +
    "sixty-four pieces of artillery, in a semicircle which inclosed my position' (or-27-1-sawyer, −55 late-stated profile)";
  k2.strength = 118;
  k2.citation = k2.citation +
    "; ED-50 shape: the flank action's moderate spike (~34 across the fire window), the balance of the 102 falling in the " +
    "post-window evening skirmishing — decline reconstructed inside the primary split (supersedes the previous 'no " +
    "July-3-only casualty split located' note: the split is now primary, or-27-1-sawyer)";
}

// ---------------------------------------------------------------------------
// 8. Sherrill's brigade — the one in-window leg (slope → the wall line)
//    (reconstruction/dossiers/us-ii-3-3-willard.md; or-27-1-bull + tablet)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-sherrill").keyframes;
  const [k0, k1, ...rest] = kfs as Keyframe[];
  k0!.x = 4540; k0!.z = 5115;
  k0!.citation = k0!.citation +
    " — RE-PLACED ON THE SLOPE EAST OF THE WALL (dossier pass 4): July 3 the regiments were 'so placed as supports to the " +
    "artillery' on 'the slope of the hill occupied by us' (or-27-1-bull) — the monument row x≈4460-4540 is the CLIMAX line " +
    "(ED-39 semantics), reached by the 15:00 move-up below; pre-move position class ~70 m east of the wall line, inferred " +
    "radius (reconstruction/dossiers/us-ii-3-3-willard.md EC3.3)";
  k1!.x = 4540; k1!.z = 5115;
  unit("us-sherrill").keyframes = [
    k0!, k1!,
    { t: 7440, x: 4470, z: 5110, facing: 258, formation: "line", strength: 1430, confidence: "documented",
      citation: "THE BRIGADE'S ONE IN-WINDOW LEG (A vs CA-J3A-5, tablet-adjudicated 15:00): 'At 3 P. M. after a terrific cannonade of two hours the Brigade was moved up to the line of the Second Brigade and assisted in repulsing Longstreet's assault' (stone-sentinels 3rd Bde 3rd Div II Corps tablet; or-27-1-bull's cannonade clock 'About 1 p.m. ... continued about two hours' is the +7-class agreement) — a short support-to-wall leg [B: 2-4 min at advance pace]; the brigade's climax line interleaves Smyth's regimental stones (39th NY 4467,4988 · 125th NY 4468,5053 · 111th NY 4458,5127 · 126th NY 4543,5267; reconstruction/dossiers/us-ii-3-3-willard.md EC3.4)" },
    ...rest,
  ];
}

// ---------------------------------------------------------------------------
// 9. Battery decay where the dossier gives a July-3 apportionment
// ---------------------------------------------------------------------------
// Dow: 13 wounded in the duel that PRECEDED the charge (the one McGilvery-line
// battery with an in-window bombardment apportionment).
{
  const kfs = unit("us-btty-dow").keyframes;
  const [k0, kEnd] = kfs as [Keyframe, Keyframe];
  unit("us-btty-dow").keyframes = [
    k0,
    { ...k0, t: 420, confidence: "inferred",
      citation: "decay begins with the cannonade (CA-J3A-1)" },
    { ...k0, t: 7200, strength: 90, confidence: "documented",
      citation: "the battery's 13 wounded fell 'in the artillery duel that preceded Pickett's Charge on the afternoon of July 3' (Stone Sentinels 6th Maine Battery page prose) — under fire while holding McGilvery's silent line; the one per-battery bombardment apportionment on the line (us-ar-1v-mcgilvery.md EC6: 'July 3 light by his own overshoot statement' at line grain)" },
    { ...kEnd, strength: 90 },
  ];
}
// Fitzhugh: 7 w (K/1st NY + 11th NY), single-episode crisis profile (ED-41).
{
  const kfs = unit("us-btty-fitzhugh").keyframes;
  const kEnd = kfs.pop()!;
  unit("us-btty-fitzhugh").keyframes = [
    ...kfs,
    { ...kEnd, t: 9000, strength: 144, confidence: "inferred",
      citation: "crisis-window spike (ED-41 casualty-curve class: near-zero before arrival, single spike across the fire window); 7 wounded + 5 horses, K/1st NY + 11th NY (reconstruction/dossiers/us-btty-fitzhugh.md EC6, single-episode)" },
    { ...kEnd, strength: 142,
      citation: "holds the crest position to the window end; end strength 149 − 7 (7 w + 5 horses, single-episode crisis profile, dossier EC6)" },
  ];
}
// Parsons: 2 k / 7 w, single-episode.
{
  const kfs = unit("us-btty-parsons").keyframes;
  const kEnd = kfs.pop()!;
  unit("us-btty-parsons").keyframes = [
    ...kfs,
    { ...kEnd, t: 9000, strength: 108, confidence: "inferred",
      citation: "crisis-window spike (ED-41 class); 2 k / 7 w + 5 horses (reconstruction/dossiers/us-btty-parsons.md EC6, single-episode)" },
    { ...kEnd, strength: 107,
      citation: "holds to the window end; end strength 116 − 9 (2 k / 7 w, single-episode crisis profile, dossier EC6)" },
  ];
}
// Cooper: July 3 = 1 w BY HIS OWN REPORT (the monument's July-3 heading is
// wrong by the battery's own per-day pins).
{
  const kfs = unit("us-btty-cooper").keyframes;
  const kEnd = kfs.pop()!;
  unit("us-btty-cooper").keyframes = [
    ...kfs,
    { ...kEnd, strength: 113,
      citation: kEnd.citation + " — PER-DAY PINS PRIMARY (dossier pass 2): July 3 loss = 1 w by the battery's own report (whole-battle 12 = 3k/9w; July 1 dominant, July 2 = 2k/6w) — 'the monument's July-3 heading [is] wrong by the battery's own report' (reconstruction/dossiers/us-btty-cooper.md EC6); rounds per day ~400/500/150, the set's best tier-B basis [July 3 ≈ 20-25 min fire]" },
  ];
}

// ---------------------------------------------------------------------------
// 10. Events
// ---------------------------------------------------------------------------
// 10a. Dearing's cannonade window shortened to the documented famine.
{
  const ev = (battle.events ?? []).find((e) => e.id === "csa-bn-dearing-cannonade");
  if (!ev) throw new Error("csa-bn-dearing-cannonade missing");
  ev.t1 = 5700;
  ev.citation = ev.citation +
    "; WINDOW SHORTENED THIS WAVE on his own report (or-27-2-dearing, fetched dossier pass 2): 'commencing firing slowly " +
    "and deliberately' by battery at the signal, until ammunition 'became completely exhausted, excepting a few round in " +
    "my rifled guns'; then 'a very heavy fire for upward of an hour without being able to fire a single shot' — the " +
    "silent-under-fire hour spans the cannonade's last phase (t1 ≈ 5700 ≈ 14:35, inferred inside the ≥1 h attested " +
    "silence before the assault's support phase; Wilcox's 'I then rode back rapidly to our artillery, but could find " +
    "none near that had ammunition' is the infantry-side witness of the same famine)";
  ev.note = (ev.note ? ev.note + "; " : "") +
    "the tablet's 16:00 withdrawal vs his own 18:00 repulse-fire statement is a carried two-hour conflict (dossier EC4.3) — neither authored";
}

// 10b. McGilvery's deliberate reply — six per-battery windows on the
// line-grain OR attestation (his p. 884; the hold-fire citations on the t=0
// keyframes remain the governing posture before t=5820).
const mcgilveryReply =
  "McGilvery's OR (No. 318, p. 884) verbatim: 'After the enemy had fired about one hour and a half, and expended at least " +
  "10,000 rounds of ammunition, with but comparatively little damage to our immediate line, a slow, well-directed fire from " +
  "all the guns under my command was concentrated upon single batteries of the enemy of those best in view, and several " +
  "badly broken up and successively driven from their position to the rear' — the deliberate reply, Hunt's 10:00 " +
  "husband-ammunition instruction executed to the letter (reconstruction/dossiers/us-ar-1v-mcgilvery.md EC5.3, the ED-34 " +
  "silent-then-fire class exemplar); per-battery window inferred on the line-grain attestation";
const mcgilveryReplyNote =
  "window: CA-J3A-1 + ~1.5 h ⇒ t0=5820 (≈14:37; his stated 12:30 opening is the corpus's one EARLY clock, profiled +5 " +
  "[-30,+37]); t1 at the cannonade's fall-quiet (CA-J3A-5, t=7200); distinct from the two evented cannonade-phase breaches " +
  "(Phillips ~13:30 by Hunt's order, Hart by Hancock's direction) and from the 7800-9000 repulse enfilade";
const replyBatteries = ["ames", "dow", "sterling", "hart", "phillips", "thompson"];
const newEvents: EngagementEvent[] = replyBatteries.map((b) =>
  fireEvent({
    id: `us-btty-${b}-deliberate-reply`, kind: "artillery_fire",
    t0: 5820, t1: 7200, unitId: `us-btty-${b}`,
    confidence: "inferred", citation: mcgilveryReply, note: mcgilveryReplyNote,
  }));

// 10c. Miller's advanced section — the attested detachment fires as a fixed
// segment (battle-format.md "Segment-emitter migration and attested
// detachments"): the battalion stays one unit on its tablet line; Miller's
// and Battles's advanced guns fire from their own attested position.
newEvents.push(fireEvent({
  id: "csa-wa-miller-advanced-section", kind: "artillery_fire",
  t0: 8100, t1: 9600,
  segment: { x: 3536, z: 3814, x2: 3576, z2: 3756 },
  confidence: "documented",
  citation: "or-27-2-eshleman verbatim: 'I immediately directed Captain Miller to advance his and Lieutenant Battles' batteries ... he took position 400 or 500 yards to the front, and opened with deadly effect upon the enemy'; marker: 'supported the charge of the infantry by advancing 450 yards' (stone-sentinels Miller's Louisiana Battery marker; reconstruction/dossiers/csa-1c-wsh-3-miller.md EC4.2) — THE documented CSA close-range phase of the repulse window, retiring the pass-1 attested-static reading",
  note: "segment = ~450 yd forward of the battalion's tablet line on its facing (coordinate inference ±80 m); leg [B: ~410 m at horsed-battery walk + limber/unlimber ≈ 6-8 min from the post-step-off order] ⇒ opens ~t=8100; fire until horse losses left 'but one gun' usable (marker), end inferred ~t=9600; the battalion-level cannonade event ends t=7500 — different window, never the same fire at two grains",
}));

// 10d. Fitzhugh's second window — the Wilcox/Lang enfilade (his dossier's
// two-attack record; rides the same spine as the Vermont/Phillips/Hart
// follow-up events).
newEvents.push(fireEvent({
  id: "us-btty-fitzhugh-wilcox-enfilade", kind: "artillery_fire",
  t0: 10200, t1: 10500, unitId: "us-btty-fitzhugh",
  confidence: "documented",
  citation: "the battery's two-attack record (reconstruction/dossiers/us-btty-fitzhugh.md EC5): the first attack 'collapsed in 30-45 min of his fire'; the second (Wilcox/Lang) 'broken by enfilade' — window = the Wilcox/Lang follow-up spine (10200-10500), matching us-btty-phillips-florida-repulse and us-btty-hart-second-line on the same ground",
  note: "ROUNDS ITEMIZED across both windows: 57 percussion shell + 15 shrapnel + 17 time shell = 89 (or-27-1-fitzhugh via dossier) [B: ~8-10 min of full-battery fire total] — the physical tiebreaker that ruled his move-clock conflict toward ~15:00 (13:00-vs-15:00 carried on the unit)",
}));

battle.events = [...(battle.events ?? []), ...newEvents];

// ---------------------------------------------------------------------------
// gate + export
// ---------------------------------------------------------------------------
writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(
  `wave A1 written: ${battle.units.length} units, ${battle.events?.length ?? 0} events ` +
  `(+${newEvents.length} events; 11 unit tracks enriched, 2 citation upgrades)`);
