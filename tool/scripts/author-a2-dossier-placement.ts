// Authoring Wave A2 — the July 3 afternoon COMPLETION wave: pour the remaining
// in-window dossier content (passes 4-15) into the macro battle file. A1
// consumed the passes-1-4 July 3 core (tool/scripts/author-a1-dossier-
// placement.ts); this wave consumes everything the later passes added that is
// (a) in-window (t=0..10800, startTime 46800 = nominal 13:00), (b) attested,
// and (c) authorable without new units. Run from tool/:
//   npx vite-node scripts/author-a2-dossier-placement.ts
// Committed as the derivation record. Wave report:
// docs/reconstruction/audit/authoring-wave-a2.md (adds, cuts, supersessions).
//
// HARD RULES this script observes (identical to A1):
// - The 13 Angle-cast units are NOT touched (guarded below — throws on touch).
// - NEVER invent motion: every added/edited keyframe and event cites the
//   dossier's cited facts; attested stillness stays still; tier-C/D/E timing
//   authors at the preferred time with `inferred` confidence.
// - NO new units (the Ziegler's Grove convergence — pass 15's headline VI
//   Corps find — is therefore CUT this wave: a post-repulse movement fact of
//   three not-in-build batteries with NO attested fire; a fixed-segment fire
//   event cannot carry it and inventing fire to visualize a move is exactly
//   what the counter-rule forbids. Recorded in the wave report as the
//   decomposition/day-expansion wave's first pickup.)
// - Out-of-window dossier content (Shaler's morning Culp's Hill fight,
//   Bartlett's ~17:00 sweep, McCartney's ~16:00 arrival + 4 rounds, the ECF
//   fight, July 2 episodes) is DEFERRED, not authored.
//
// WHAT THE WAVE DOES (headlines; full table in the wave report):
// 1. Harrow's brigade re-placed onto its regimental monument-row axis
//    (x≈4384 — the old build slot sat ~55 m east of the attested line, beyond
//    the stones' ±5-15 m class); division-frame + EC6 double-accounting
//    citations landed.
// 2. Davis's brigade gets the dossier batch's cleanest bombardment primary:
//    "In Davis' brigade 2 men were killed and 21 wounded" during the
//    cannonade (or-27-2-davis-jr) — the interpolated −30 becomes the
//    attested −23.
// 3. Poague's battalion decay: return=tablet 32 (ED-43 corroboration), ALL
//    July 3 by the attested July 1-2 non-participation.
// 4. Taft's battery: the pass-15 triage correction executed — two pre-window
//    wounding pins move t=0 strength 146→144, Wittenberg's ~2 P.M. pin lands
//    in-window, and the marker's "engaged at intervals ... until 4 p.m."
//    extends his flank-fire window to the window end.
// 5. Hill's (WV) battery: the per-day-dated fatality pair (Braddock July 2 /
//    Lacy July 3) becomes a 124→123→122 per-day decay.
// 6. Torbert's brigade: the report=tablet-exact 11 picket-line wounded
//    (July 3) authored as window decay.
// 7. Rittenhouse's cannonade-phase fire authored (NEW event): Martin's
//    brigade report ("opened upon the enemy at intervals during the day")
//    SUPERSEDES the wave-2 note that his cannonade fire was unattested —
//    both readings carried.
// 8. Citation upgrades executing the pass-15 static-park corrections on
//    in-build units (Evan Thomas's caisson series + withdrawal conflict,
//    Eakin's/Mason's corrected 10, Norton's relief-in-place, Edgell's
//    own-report + relocation conflict, Sterling's McGilvery-line membership)
//    and the pass-2/4 II Corps records A1 left unconsumed (Smyth's wounding
//    pin + 12th NJ 20-yard wall fire; Carroll's ED-51 verified-in-place).
// 9. Decay NOT authored where no primary day split exists — recorded as
//    cuts, never invented (Smyth 352/366, Harrow 722/768, Sherrill 714,
//    Eakin 10, Norton 7, Edgell 3, Evan Thomas 18, Alexander/Cabell/McIntosh
//    battalions, the CSA assault column's July-3 re-strength).
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
    throw new Error(`wave A2 guard: '${id}' is Angle-cast and untouchable`);
  const u = byId.get(id);
  if (!u) throw new Error(`wave A2: unit '${id}' missing`);
  return u;
}
function event(id: string): EngagementEvent {
  const e = (battle.events ?? []).find((x) => x.id === id);
  if (!e) throw new Error(`wave A2: event '${id}' missing`);
  return e;
}

// ---------------------------------------------------------------------------
// 1. Harrow's brigade — re-placed onto the regimental monument-row axis
//    (reconstruction/dossiers/us-ii-2-1-harrow.md EC3.2; or-27-1-harrow)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-harrow").keyframes as Keyframe[];
  const [k0, k7200, k8700, k9000, kEnd] = kfs as [Keyframe, Keyframe, Keyframe, Keyframe, Keyframe];
  k0.x = 4384; k0.z = 4602;
  k0.citation = (k0.citation ?? "") +
    " — RE-PLACED ON THE REGIMENTAL MONUMENT ROW (dossier pass 2 EC3.2, executed wave A2): 19th ME (4384,4644) · " +
    "15th MA (4384,4610) · 1st MN position marker (4385,4567) · 82nd NY (4385,4564) — a clean single-line axis x≈4384, " +
    "z 4560→4645, frontage ~85 m, left (south) of Hall's wall line and fronting Rorty's battery ground (mon-rorty " +
    "4412,4597 sits INSIDE the regimental span); Bachelder j3-03 draws the front bar continuous from Hall's southward " +
    "through this span — the old build slot (4440,4640) sat ~55 m east of the attested axis, beyond the stones' " +
    "±5-15 m class (reconstruction/dossiers/us-ii-2-1-harrow.md); DIVISION FRAME PRIMARY: 'The division took into " +
    "action 3,773 men, and lost 1,657' (or-27-1-harrow p. 421) — Webb 1,250 + Hall 920 + Harrow 1,410 = 3,580, the " +
    "~190-man residual a recorded compilation gap";
  k7200.x = 4384; k7200.z = 4602;
  k7200.citation = (k7200.citation ?? "") +
    "; the cannonade bracket [C +7]: 'At 1 p. m. the enemy opened a fierce cannonade upon the line from a hundred or " +
    "more guns, which was continued until nearly 3 p. m.' (or-27-1-harrow p. 420, chain-agreeing +7 class)";
  k8700.citation = (k8700.citation ?? "") +
    "; the crisis lateral [A vs CA-J3A-9]: 'the Third and First Brigades, of this division, inclined to the right, " +
    "engaging the enemy as they moved' (or-27-1-harrow p. 420) — ~100-250 m by regiment [B: 2-5 min]; 'the crest of " +
    "the hill occupied by the right of Colonel Hall and the left of General Webb seemed to be the point to which " +
    "their main attack was directed' — the convergence-target statement in the division commander's own words";
  k9000.citation = (k9000.citation ?? "") +
    "; 'the contest raged with almost unparalleled ferocity for nearly an hour' (or-27-1-harrow p. 420) — the " +
    "division-grain melee duration; ends intermingled (no formed line anchor after — dossier EC3.4 negative)";
  kEnd.x = 4384; kEnd.z = 4602;
  kEnd.citation = (kEnd.citation ?? "") +
    " — EC6 TWO ACCOUNTINGS CARRIED, never averaged (wave A2): Harrow's own report 722 over the two days (148 k / " +
    "574 w, k+w only) vs the tablet 768 (incl. 48 captured/missing) — July 2 share heavy (Ward mw, Huston mw July 2; " +
    "the 1st MN's 215 of its return-224 fell July 2); NO primary brigade day split — the in-window decline stays the " +
    "build's inferred shape, not re-derived (reconstruction/dossiers/us-ii-2-1-harrow.md EC6); end pose returned to " +
    "the monument-row axis";
}

// ---------------------------------------------------------------------------
// 2. Smyth's brigade — the pass-2 record consumed (citations; no re-place:
//    the build slot sits ~18 m from the monument-row centroid, inside class)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-smyth").keyframes as Keyframe[];
  const [, k7200, k8700, k9000, kEnd] = kfs as [Keyframe, Keyframe, Keyframe, Keyframe, Keyframe];
  k7200.citation = (k7200.citation ?? "") +
    "; COMMAND PIN [D during CA-J3A-1..5]: 'During the cannonading, having received a wound, I was obliged to quit " +
    "the field, and surrendered the command to Lieutenant Colonel Francis E. Pierce' (or-27-1-smyth, pp. 464-466 — " +
    "the primary replacing the pass-0 Wikipedia hop); his 'At 2 p. m. a most terrific cannonading' clock is " +
    "report-nominal (−53 profile, ED-25 rule 4)";
  k8700.citation = (k8700.citation ?? "") +
    "; the receiving side's own record [A window vs CA-J3A-6..10]: 'As the fire of the enemy's batteries slackened, " +
    "their infantry moved upon our position in three lines'; the 12th NJ 'held fire until they were within 20 yards' " +
    "— a canister-grade range statement for the wall fight (or-27-1-smyth); Lt. William Smith (1st DE) k 'with one " +
    "of the enemy's captured flags in his hand' [D repulse]";
  k9000.citation = (k9000.citation ?? "") +
    "; 'The number of prisoners captured by this brigade is estimated at from 1,200 to 1,500'; 9 battle-flags " +
    "(or-27-1-smyth) — the Pettigrew-front collapse from the receiving side";
  kEnd.citation = (kEnd.citation ?? "") +
    " — EC6 TWO ACCOUNTINGS CARRIED (wave A2): OR recapitulation 352 (officers 7 k / 28 w / 1 m; men 53 k / 238 w / " +
    "25 m, OCR-flagged) vs War Dept tablet 366 — July 3 dominated by cannonade + repulse, 'the One hundred and eighth " +
    "New York lost heavily, the casualties being about half of the regiment in action' (Woodruff's support regiment " +
    "bled with the battery); NO primary day split — the in-window decline stays the build's inferred shape " +
    "(reconstruction/dossiers/us-ii-3-2-smyth.md EC6)";
}

// ---------------------------------------------------------------------------
// 3. Carroll's three regiments — ED-51 verified in place; the ED-44-class
//    under-fire record and capture ledger landed
//    (reconstruction/dossiers/us-ii-3-1-carroll.md)
// ---------------------------------------------------------------------------
{
  const upgrade =
    " — ED-51 VERIFIED IN PLACE (wave A2 audit item): the three regiments held East Cemetery Hill 'until the 5th' " +
    "(or-27-1-carroll + the j2-05 drawn state) — this East-Cemetery-Hill track IS the adopted rendering, no contrary " +
    "build state found; July 3 in-window state [A window]: 'exposed to a great deal of cross-firing during the heavy " +
    "cannonading of the 3d' (or-27-1-carroll) — under-fire-without-reply, ED-44 class: zero events is the correct " +
    "encoding; capture ledger: 252 prisoners, 113 enemy wounded cared for, 37 buried, 349 stand of arms " +
    "(or-27-1-carroll); brigade return 211 three-day incl. the detached 8th Ohio's 102 (or-27-1-union-return p. 176) " +
    "— the three-regiment day split is OPEN, no decay authored (reconstruction/dossiers/us-ii-3-1-carroll.md)";
  for (const k of unit("us-carroll").keyframes) k.citation = (k.citation ?? "") + upgrade;
}

// ---------------------------------------------------------------------------
// 4. Davis's brigade — the bombardment-share primary (the batch's cleanest)
//    (reconstruction/dossiers/csa-3c-het-4-davis.md EC6; or-27-2-davis-jr)
// ---------------------------------------------------------------------------
{
  const kfs = unit("csa-davis").keyframes as Keyframe[];
  const k7200 = kfs.find((k) => k.t === 7200);
  const k7500 = kfs.find((k) => k.t === 7500);
  if (!k7200 || !k7500) throw new Error("csa-davis: expected 7200/7500 keyframes");
  k7200.strength = 1977;
  k7200.confidence = "documented";
  k7200.citation =
    "BOMBARDMENT SHARE PRIMARY (wave A2, the dossier batch's cleanest): 'In Davis' brigade 2 men were killed and 21 " +
    "wounded' during the two-hour cannonade (or-27-2-davis-jr) ⇒ 2,000 − 23 = 1,977 at the cannonade's fall-quiet — " +
    "replaces the build's interpolated −30; ~2.5% of the standing base, closely matching Garnett's ~20 on the other " +
    "wing (the two independent primaries that size the whole column's bombardment ordeal) — " +
    "reconstruction/dossiers/csa-3c-het-4-davis.md EC6; NOTE the t=0 base stays the July-1-scoped 'about 2,000' " +
    "(stone-sentinels) — the July 3 morning re-strength (return 897 k+w at regiment grain; the 11th Miss fresh, " +
    "train-guard July 1) is a deferred re-cast item, so the attested decay is applied to the standing base";
  k7500.strength = 1977;
  k7500.citation = (k7500.citation ?? "") +
    "; step-off at the bombardment-end strength (wave A2 monotonic correction on the attested −23, or-27-2-davis-jr)";
}

// ---------------------------------------------------------------------------
// 5. Poague's battalion — return=tablet 32, ALL July 3 by the attested
//    July 1-2 non-participation (reconstruction/dossiers/csa-3c-bn-poague.md)
// ---------------------------------------------------------------------------
{
  const kfs = unit("csa-bn-poague").keyframes;
  const base = kfs[0]!;
  base.citation = (base.citation ?? "") +
    " — pass-4 dossier consumed (wave A2): July 1-2 attested NON-participation ('I got no service out of those " +
    "useless guns' — the six-howitzer negative; the battalion's first firing day is July 3), so the whole battle " +
    "loss falls on July 3; two-position geometry drawn on j3-03 (WYATT/GRAHAM vs POAGUE'S BATT. labels, 5 guns near " +
    "Pegram + 5 Napoleons 400 yd right) — the build's single block spans the position class " +
    "(reconstruction/dossiers/csa-3c-bn-poague.md)";
  unit("csa-bn-poague").keyframes = [
    base,
    { ...base, t: 7200, strength: 362, confidence: "inferred",
      citation: "cannonade + counter-fire decay, shape inferred inside the attested single-day total (all 32 are July 3 — July 1-2 non-participation attested, or-27-2-poague via dossier EC6); tablet 'Ammunition expended 657 rounds' (three-day, mostly this window)" },
    { ...base, t: 10800, strength: 352, confidence: "inferred",
      citation: "END STRENGTH ON THE RETURN=TABLET AGREEMENT (ED-43 corroboration satisfied): batteries '*Not reported in detail', battalion total 2 k / 24 w / 6 m = 32 (or-27-2-anv-return p. 345; tablet prints the same 32, same scope) ⇒ 384 − 32 = 352; the July 3 MORNING-affair share (8 horses, 'some of the w') sits pre-window and is not separable — the decay is authored across the in-window fire arc with that overstatement flagged (reconstruction/dossiers/csa-3c-bn-poague.md EC6)" },
  ];
}

// ---------------------------------------------------------------------------
// 6. Taft's battery — the pass-15 triage correction executed: pre-window
//    pins, the in-window Wittenberg pin, the itemized ledger, and the
//    marker's own 4 P.M. window end (reconstruction/dossiers/us-btty-taft.md)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-btty-taft").keyframes;
  const [k0, kEnd] = kfs as [Keyframe, Keyframe];
  k0.strength = 144;
  k0.citation = (k0.citation ?? "") +
    " — STATIC-PARK TRIAGE CORRECTED AT BATTERY GRAIN (pass 15, own-report primary; wave A2 executes): the battery " +
    "reported directly to Maj. Gen. Howard July 2, in position 5 P.M. [C]; PRE-WINDOW PINS: Lt. Clinton Thalheimer " +
    "w (abdomen, bullet) at 9 A.M. July 3 (died at midnight) and Sgt. Henry Dillon w (neck, slight) ~10 A.M. July 3 " +
    "both fell BEFORE this window opens ⇒ t=0 strength 146 − 2 = 144 (or-27-1-taft No. 325, pp. 891-892, via " +
    "reconstruction/dossiers/us-btty-taft.md EC1)";
  kEnd.strength = 143;
  kEnd.citation = (kEnd.citation ?? "") +
    " — end strength 144 − 1 (Wittenberg, the one in-window pin); the brigade-vs-battery arithmetic tension CARRIED, " +
    "not reconciled: the 2nd Volunteer Brigade tablet's 8-total (with Brooker's/Pratt's attested zero shares) sits " +
    "against the four individually-named casualties, Begg's and Thalheimer's deaths falling outside the tablet's " +
    "day-scoped columns (dossier EC6); the report's cease-fire ~6 P.M. vs the marker's 'until 4 p.m.' is a " +
    "two-hour end-clock conflict carried on the fire event";
  unit("us-btty-taft").keyframes = [
    k0,
    { ...k0, t: 3600, strength: 143, confidence: "inferred",
      citation: "Pvt. Adolph Wittenberg severely wounded through the leg ~2 P.M. July 3 [C, report-primary] — an individually-timed IN-WINDOW pin (or-27-1-taft); one 20-pdr Parrott burst at the muzzle during the action (equipment failure, distinct from combat loss); rounds itemized by projectile type: 80 Schenkl percussion shell, 63 Schenkl combination shrapnel, 32 Parrott time-fuse shell, 382 Parrott time-fuse shrapnel, 557 cartridges (ED-42 expenditure-ledger class; reconstruction/dossiers/us-btty-taft.md EC5)" },
    kEnd,
  ];
}
{
  const ev = event("us-btty-taft-pettigrew-flank");
  ev.t1 = 10800;
  ev.citation = (ev.citation ?? "") +
    "; WINDOW EXTENDED THIS WAVE (A2) to the marker's own end: 'Engaged at intervals in same position until 4 p.m.' " +
    "— 4 P.M. is this window's exact end (t=10800); the report's 'returned enemy fire ... again after 1:30 p.m.' " +
    "with cease ~6 P.M. (or-27-1-taft) overshoots the marker — the two-hour end-clock conflict is carried, the " +
    "marker's reading kept because it closes inside the window (reconstruction/dossiers/us-btty-taft.md EC5)";
}

// ---------------------------------------------------------------------------
// 7. Hill's (WV) battery — the per-day-dated fatality pair becomes decay
//    (reconstruction/dossiers/us-btty-hill.md EC6)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-btty-hill-wv").keyframes;
  const [k0, kEnd] = kfs as [Keyframe, Keyframe];
  k0.strength = 123;
  k0.citation = (k0.citation ?? "") +
    " — PER-DAY PINS (pass 15, wave A2 executes): 'S. J. Braddock, killed July 2; Charles Lacy, killed July 3' " +
    "(tablet, named AND dated) ⇒ t=0 strength 124 − 1 (Braddock fell pre-window) — the corpus's clearest per-day " +
    "casualty date split in the pass-15 Artillery Reserve batch (reconstruction/dossiers/us-btty-hill.md EC6)";
  kEnd.strength = 122;
  kEnd.confidence = "inferred";
  kEnd.citation = (kEnd.citation ?? "") +
    " — end strength 123 − 1: Charles Lacy's July 3 death authored across the window (intra-day time unknown, [E] " +
    "floor); the tablet's 2 wounded carry no day split and are NOT apportioned (reconstruction/dossiers/us-btty-hill.md)";
}

// ---------------------------------------------------------------------------
// 8. Torbert's brigade — the report=tablet-exact picket-line 11 (July 3)
//    (reconstruction/dossiers/us-vi-1-1-torbert.md)
// ---------------------------------------------------------------------------
{
  const kfs = unit("us-torbert").keyframes;
  const [k0, kEnd] = kfs as [Keyframe, Keyframe];
  k0.citation = (k0.citation ?? "") +
    " — pass-14 dossier consumed (wave A2): Torbert's own report independently places the brigade 'at the center of " +
    "the line under Major-General Newton's command' on the morning of July 3 (report=tablet agreement on the " +
    "Weikert-farm-area siting; the move itself is pre-window); 'Not engaged except on the skirmish line' (tablet = " +
    "report, the attested-static negative; picket credit to Lt. Col. Wiebecke, 2nd NJ) " +
    "(reconstruction/dossiers/us-vi-1-1-torbert.md)";
  kEnd.strength = 1309;
  kEnd.confidence = "inferred";
  kEnd.citation = (kEnd.citation ?? "") +
    " — THE PICKET-LINE CASUALTY EVENT (wave A2): '11 enlisted men wounded' on the picket line July 3 (Torbert's " +
    "report, verbatim) = the brigade tablet's 'Wounded 11 Men' EXACTLY, a clean report=tablet agreement scoped to " +
    "Gettysburg alone ⇒ 1,320 − 11 = 1,309; intra-day timing at the [E] floor, the decline authored across the " +
    "window; the report's broader '1 killed, 17 wounded' line is the CAMPAIGN superset (Funkstown/Hagerstown/" +
    "Fairfield), never conflated (ED-49/ED-52 scope discipline) — reconstruction/dossiers/us-vi-1-1-torbert.md EC6";
}

// ---------------------------------------------------------------------------
// 9. Rittenhouse — the cannonade-phase fire is now REPORT-ATTESTED (pass 13):
//    new event + supersession note on the wave-2 repulse event
//    (us-v-arty-martin.md EC5.3; us-btty-rittenhouse.md EC5)
// ---------------------------------------------------------------------------
const rittenhouseCannonade = fireEvent({
  id: "us-btty-rittenhouse-cannonade-intervals", kind: "artillery_fire",
  t0: 600, t1: 5400, unitId: "us-btty-rittenhouse",
  confidence: "documented",
  citation:
    "Martin's V Corps Artillery Brigade report (fetch-verified pass 13): Battery D 'opened upon the enemy at " +
    "intervals during the day' July 3 — the battery's cannonade-phase fire is now report-attested, SUPERSEDING the " +
    "wave-2 reading ('his cannonade-phase fire is not tablet-attested and is NOT authored') — both readings carried, " +
    "the report wins on grade (reconstruction/dossiers/us-v-arty-martin.md EC5.3); same-hill witness: 'a terrible " +
    "raking fire on the rebel line, which was made with great effect. This caused a concentration of the fire of " +
    "many rebel batteries upon Hazlett's position, their shell and shot crashing among the rocks' (norton-1913, the " +
    "146th NY Garrard-staff account, dossier pass-4 upgrade); receiving side: Peyton — 'about 1 mile to our right " +
    "... sometimes as many as 10 men being killed and wounded by the bursting of a single shell' (peyton-or-1863) — " +
    "reconstruction/dossiers/us-btty-rittenhouse.md EC5",
  note:
    "window: 'at intervals during the day' is day-grain [C] — the in-window share is authored 600 (the Union " +
    "reply's opening) to 5400 (CA-J3A-4, Hunt's conservation slackening; the LRT summit under the same ammunition " +
    "economy), leaving the 5400-7800 conservation quiet; distinct from the 7800-9000 repulse enfilade (never the " +
    "same fire twice); Martin's 'engaged only about an hour' duration is the JULY 2 duel before Hazlett fell, not " +
    "this window; EC6 day split NOT authorable (return 13 = 7 k / 6 w, July 2 dominant — Hazlett k July 2), " +
    "strength stays flat",
});
{
  const ev = event("us-btty-rittenhouse-repulse-enfilade");
  ev.note = (ev.note ?? "") +
    "; SUPERSESSION (wave A2): this note's cannonade-phase clause is retired — Martin's brigade report ('opened " +
    "upon the enemy at intervals during the day', pass 13) now attests it; see us-btty-rittenhouse-cannonade-intervals";
}

// ---------------------------------------------------------------------------
// 10. Pass-15 static-park corrections on in-build units — citation upgrades
//     (no decay where no day split exists; no movement on sequence-only legs)
// ---------------------------------------------------------------------------
// Evan Thomas — the caisson series (in-window) + the withdrawal conflict.
{
  const upgrade =
    " — STATIC-PARK TRIAGE CORRECTED AT BATTERY GRAIN (pass 15, wave A2): tablet — July 2 'Arrived and took " +
    "position on crest of hill near General Meade's Headquarters on the left of the Second Corps', 'actively " +
    "engaged in repelling the attack'; July 3 'shifted position slightly' pre-charge (no distance stated — inside " +
    "this slot's position class, not re-placed); THE DIRECT-HIT CAISSON SERIES [D, IN-WINDOW]: during the " +
    "pre-charge bombardment 'a direct hit detonated another caisson, causing three additional explosions that drew " +
    "a cheer from Confederate lines' (tablet verbatim — cross-lines-witnessed); an earlier caisson explosion killed " +
    "and wounded men of the nearby 14th Vermont (collateral cross-reference, flagged not verified); EC6: 1 k + 1 " +
    "officer & 16 men w = 18 across the TWO days, deaths-from-wounds Joseph A. Campbell and William McNeal named — " +
    "NO day split, no decay authored; CONFLICT CARRIED: the tablet's post-bombardment 'withdrew to the Artillery " +
    "Reserve' [D, sequence-only] vs this unit's inferred McGilvery-line repulse-enfilade window (7800-9000, " +
    "collective attestation) — both readings recorded, the withdrawal NOT authored on a sequence-only tablet leg " +
    "with no clock (reconstruction/dossiers/us-btty-evanthomas.md)";
  for (const k of unit("us-btty-thomas").keyframes) k.citation = (k.citation ?? "") + upgrade;
}
// Eakin's/Mason's — page-verified strength + the corrected 10-total.
{
  const upgrade =
    " — battery-grain verification (pass 15, page-verified DIRECTLY, wave A2): strength 153 confirmed at tablet " +
    "grain; EC6 corrected total 10 — 'Killed 1 Man Wounded 1 Officer and 7 Men Missing 1 Man' (tablet verbatim; an " +
    "initial search-stage '9' undercounted, the correction recorded not hidden); 'Continued engagement' July 3 — " +
    "the two-day record carries NO day split, no decay authored (reconstruction/dossiers/us-btty-eakin.md)";
  for (const k of unit("us-btty-mason").keyframes) k.citation = (k.citation ?? "") + upgrade;
}
// Norton's — the relief-in-place record + named casualties.
{
  const upgrade =
    " — RELIEF-IN-PLACE RECORD (pass 15, wave A2): 'sent into action on Cemetery Hill, taking the place of a badly " +
    "shot up battery' and remained through the end of the battle (tablet verbatim) — a distinct activity class; " +
    "WHICH battery it relieved is unidentified (open cross-reference, possibly an XI Corps battery); EC6 named: " +
    "Pvts. Henry Schram and Jacob Kirsh k, Pvt. John Edmunds mw, 4 w — no day split, no decay authored " +
    "(reconstruction/dossiers/us-btty-norton-1oh.md)";
  for (const k of unit("us-btty-norton").keyframes) k.citation = (k.citation ?? "") + upgrade;
}
// Edgell's — own-report primary + the relocation conflict (ED-74 class).
{
  const upgrade =
    " — OWN-REPORT PRIMARY AT BATTERY GRAIN (pass 15 triage correction, wave A2): 248 rounds fired ON JULY 3 of 353 " +
    "total (the 105 figure is a pre-relocation SUB-TOTAL — a scope distinction, not a conflict, ED-42 discipline); " +
    "EC6: 3 men wounded (1 seriously), 3 horses killed, 1 wheel and axle broken (replaced night of July 4) — no day " +
    "split, no decay authored; CONFLICT CARRIED (ED-74 class): the report's July 3 relocation 'to a cornfield near " +
    "the Baltimore turnpike' [D, un-clocked] vs the monument's own 'On this ground ... July 2nd and 3rd' — " +
    "corroborated sideways by McCartney's battery collecting '48 rounds of 3-inch projectiles, perfect' abandoned " +
    "when the 1st New Hampshire Battery withdrew (or-27-1-mccartney, pass 15) — the position is NOT moved on a " +
    "sequence-only leg with no stated geometry (reconstruction/dossiers/us-btty-edgell.md)";
  for (const k of unit("us-btty-edgell").keyframes) k.citation = (k.citation ?? "") + upgrade;
  const ev = event("us-btty-edgell-counterbattery");
  ev.citation = (ev.citation ?? "") +
    "; battery-grain upgrade (pass 15, wave A2): 248 of the 353 rounds are JULY 3 by his own report " +
    "(or-27-1-edgell p. 893) — the day's expenditure now report-attested at battery grain";
}
// Sterling's — McGilvery-line membership now battery-attested.
{
  const upgrade =
    " — battery-grain upgrade (pass 15, wave A2): the survey §3.3 'no July 3 activity text' flag PARTIALLY RETIRED — " +
    "the battery's own tablet joins it to McGilvery's composite line from July 2 evening: 'Reinforced Third Corps " +
    "line and late in the day retired and formed line under Lieut. Col. F. McGilvery on left of Second Corps' " +
    "(tablet verbatim) — the A1 line-grain fire windows now rest on an ATTESTED line membership (their per-battery " +
    "windows stay line-grain inferred); unique ordnance: four 14-pdr James rifles + two 12-pdr howitzers, 'the only " +
    "Federal battery to be armed with either type of piece at Gettysburg'; EC6: 3 w, 2 m — no day split " +
    "(reconstruction/dossiers/us-btty-sterling.md)";
  for (const k of unit("us-btty-sterling").keyframes) k.citation = (k.citation ?? "") + upgrade;
  for (const id of ["us-btty-sterling-deliberate-reply", "us-btty-sterling-repulse-enfilade"]) {
    const ev = event(id);
    ev.note = (ev.note ?? "") +
      "; pass-15 upgrade (wave A2): line membership now battery-attested (tablet: 'formed line under Lieut. Col. F. " +
      "McGilvery') — the §3.3 flag retires to window-grain-only";
  }
}

battle.events = [...(battle.events ?? []), rittenhouseCannonade];

// ---------------------------------------------------------------------------
// gate + export
// ---------------------------------------------------------------------------
writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(
  `wave A2 written: ${battle.units.length} units, ${battle.events?.length ?? 0} events ` +
  `(+1 event, 1 event window extended, 1 supersession note; 13 unit tracks touched — ` +
  `1 re-place, 5 decay sets, 7 citation upgrades)`);
