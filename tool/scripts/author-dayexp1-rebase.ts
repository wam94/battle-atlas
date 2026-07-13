// Day expansion slice 1 — THE ASSAULT-COLUMN STRENGTH RE-BASING (the
// slice's atomic data operation; A2 cut 2 executed). Pass-3 dossier
// primaries (dossier-pass-3.md §2 item 3; ED-46/47/48) replace the
// July-1-scoped / compilation strength bases where a primary exists,
// with the whole decay shape re-derived in the same commit. Run from
// tool/:  npx vite-node scripts/author-dayexp1-rebase.ts
// Committed as the derivation record. Slice report:
// docs/reconstruction/audit/day-expansion-slice-1.md.
//
// THE FILM-SAFETY DESIGN (the tripwire this operation is gated on):
// the shipped Soldier View film is compiled from
// reconstruction/canonical/angle.reconstruction.json (startStrength
// 1393 = "macro track linear interpolation at t=8040", casualty
// profiles inside the slice 8040..9000) — per-second compiled states
// change ONLY if that file changes. This script therefore:
//   - touches NO recon/claims file;
//   - keeps every Angle-cast keyframe with 8040 < t <= 9000
//     byte-identical (guarded);
//   - pins the slice edge with a NEW t=8040 keyframe on csa-garnett at
//     exactly the compiled slice-start strength (1393) and the exact
//     old interpolated pose, so the ±1 reconciliation at slice_t0
//     holds by construction and the ED-46 base delta is absorbed
//     OUTSIDE the film window (pre-slice: the attested -20 bombardment
//     replaces the interpolated -25; post-slice: the dissolution
//     re-derives to the return-grade end 1427-941=486).
// Consequence to verify after recompile (scripts/compile_angle.py):
// bundle payload byte-identical except inputs.battle + checksum;
// stagingSeed d470c469... held. If that verification ever fails, the
// operation must be REVERTED and the owner consulted — a film
// invalidation is an owner ruling, never an executor default.
//
// WHAT CHANGES (values; every keyframe cites its basis):
// - csa-garnett (CAST): 1480 -> 1427 (ED-46, Peyton's previous-evening
//   report 1,287 men + ~140 officers); bombardment -20 attested
//   (peyton-or-1863) replaces interpolated -25; slice-edge pin 1393 at
//   t=8040; in-slice values untouched; end 539 -> 486 (= 1427 - 941,
//   or-27-2-anv-return p. 339). Children (5 VA regiments, display-grain
//   even split) re-split from the new parent track.
// - csa-lowrance: 1250 (pre-July-1 compilation) -> 500 (PRIMARY,
//   or-27-2-lowrance July 1 evening "about 500 men"); July-3 loss has
//   NO primary total — ~185 inferred, bounded ("very heavy in
//   proportion", pass 3), end 315; the two-brigade "not numbering in
//   all 800 guns" works statement rides the climax citation as a
//   PRESENT-MEASURE scope note (ED-49 discipline: a different measure
//   than the attrition track, carried, not authored as a value).
// - csa-brockenbrough: base 880 kept (ED-48a, Mayo-hop, 800-1100 range
//   carried); decay 100 -> 148 (ED-48b: the return's 148 k+w is the
//   loss FLOOR and the build's 100 sat below it), end 780 -> 732;
//   wings re-split.
// - csa-fry (CAST), csa-lane, csa-wilcox, csa-lang: values CONFIRMED
//   by pass-3 primaries — citation upgrades only (Fry's 1,048 carries
//   its own July-1 scope caveat verbatim; Lane ED-47; Wilcox's
//   1,777-577=1,200 closure; Lang's 700-basis arithmetic).
// - csa-marshall, csa-davis: NO primary July-3 basis exists (2,000 is
//   a July-1 "present" figure; subtraction gives ~900 [B] for Marshall)
//   — NOT re-based; the heterogeneity is annotated in-file, loudly,
//   as an owner-ruling residual.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, Keyframe, Unit } from "../src/model";
import { exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
const battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));
const byId = new Map(battle.units.map((u) => [u.id, u]));
function unit(id: string): Unit {
  const u = byId.get(id);
  if (!u) throw new Error(`rebase: unit '${id}' missing`);
  return u;
}
function kfAt(u: Unit, t: number): Keyframe {
  const k = u.keyframes.find((k) => k.t === t);
  if (!k) throw new Error(`rebase: '${u.id}' has no keyframe at t=${t}`);
  return k;
}

// The film guard: cast keyframes with 8040 < t <= 9000 must be
// byte-identical after this script (the compiled slice is 8040..9000 and
// reconciliation checks every keyframe time strictly inside it plus the
// edges; the 8040 edge is pinned separately below).
const CAST = ["csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
  "us-webb", "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing",
  "us-btty-cowan", "us-btty-arnold", "us-hall", "us-stannard"];
const inSliceBefore = new Map<string, string>();
for (const id of CAST)
  inSliceBefore.set(id, JSON.stringify(
    unit(id).keyframes.filter((k) => k.t > 8040 && k.t <= 9000)));

// ---------------------------------------------------------------------------
// 1. csa-garnett — ED-46 executed with the slice-edge film pin
// ---------------------------------------------------------------------------
{
  const u = unit("csa-garnett");
  const k0 = kfAt(u, 0);
  if (k0.strength !== 1480) throw new Error("expected Garnett's ED-9 base");
  k0.strength = 1427;
  k0.citation = (k0.citation ?? "") +
    " — RE-BASED (ED-46, day-expansion slice 1): 'The brigade went into action with 1,287 men and about 140 " +
    "officers, as shown by the report of the previous evening' (peyton-or-1863, PRIMARY — the best strength " +
    "evidence any CSA brigade in the assault column has) ≈ 1,427 all ranks, adopted over the 1,480 monument " +
    "compilation (stone-sentinels, ED-9 macro value — carried, not erased; claim-strength-garnett records the " +
    "compilation reading)";
  const k7200 = kfAt(u, 7200);
  k7200.strength = 1407;
  k7200.citation =
    "BOMBARDMENT SHARE, PRIMARY (re-based, ED-46): 'During the shelling, we lost about 20 killed and wounded' " +
    "(peyton-or-1863, with the Lt. Col. Ellis k pin) — the attested −20 replaces wave-authoring's interpolated " +
    "−25; ~2% of strength, one of the two primaries sizing the waiting ordeal (dossier csa-1c-pic-1-garnett.md EC6)";
  k7200.confidence = "documented";
  const k7500 = kfAt(u, 7500);
  k7500.strength = 1405;
  // the slice-edge pin: the exact old interpolated pose at t=8040 (between
  // the 7500 and 8160 keyframes, u = 540/660) and the compiled slice-start
  // strength — the film window's ground truth, unchanged by construction
  const u8040 = 540 / 660;
  const k8160 = kfAt(u, 8160);
  const pin: Keyframe = {
    t: 8040,
    x: Math.round((k7500.x + u8040 * (k8160.x - k7500.x)) * 10) / 10,
    z: Math.round((k7500.z + u8040 * (k8160.z - k7500.z)) * 10) / 10,
    facing: Math.round((k7500.facing + u8040 * (k8160.facing - k7500.facing)) * 10) / 10,
    formation: k7500.formation,
    strength: 1393,
    confidence: "inferred",
    citation:
      "SLICE-EDGE FILM PIN (ED-21 discipline, day-expansion slice 1): the shipped Angle bundle compiles this " +
      "brigade from startStrength 1393 at t=8040 (angle.reconstruction.json, basis 'macro track linear " +
      "interpolation at t=8040') — the ED-46 re-basing absorbs the base delta OUTSIDE the film window (bombardment " +
      "−20 attested, approach 7500→8040 −12 inferred) so every compiled per-second state of the shipped film is " +
      "unchanged; pose is the exact pre-pin interpolation (no geometry change)",
  };
  const idx = u.keyframes.findIndex((k) => k.t === 8160);
  u.keyframes.splice(idx, 0, pin);
  const k9600 = kfAt(u, 9600);
  k9600.strength = 541; // 700 − round((700−580)/(700−539) × (700−486))
  const endCitation =
    " — RE-BASED END (ED-46 + or-27-2-anv-return p. 339): 1,427 − 941 (78 k / 324 w / 539 m, the official return, " +
    "= stone-sentinels' 941 = Peyton's own '941 killed, wounded, and missing'; ED-49 scope note — 'the majority " +
    "(those reported missing) are either killed or wounded', peyton-or-1863) = 486; the old 539 was 1,480 − 941 " +
    "on the compilation base";
  const k10800 = kfAt(u, 10800);
  k10800.strength = 486;
  k10800.citation = (k10800.citation ?? "") + endCitation;
  kfAt(u, 23340).strength = 486;
  // children: the display-grain even split re-derives from the new parent
  // track (the standing convention: child = round(parent/5) at the child's
  // own keyframe times; in-slice values land unchanged because the parent's
  // in-slice values are unchanged)
  const parentAt = (t: number): number => {
    const kfs = u.keyframes;
    if (t <= kfs[0]!.t) return kfs[0]!.strength;
    for (let i = 1; i < kfs.length; i++)
      if (t <= kfs[i]!.t) {
        const a = kfs[i - 1]!, b = kfs[i]!;
        return a.strength + (t - a.t) / (b.t - a.t) * (b.strength - a.strength);
      }
    return kfs[kfs.length - 1]!.strength;
  };
  for (const child of battle.units.filter((c) => c.parent === "csa-garnett"))
    for (const k of child.keyframes)
      k.strength = Math.round(parentAt(k.t) / 5);
}

// ---------------------------------------------------------------------------
// 2. csa-lowrance — the 500-man primary basis; July-3 loss inferred-bounded
// ---------------------------------------------------------------------------
{
  const u = unit("csa-lowrance");
  if (kfAt(u, 0).strength !== 1250) throw new Error("expected Lowrance's compilation base");
  const strengths: Record<number, number> = {
    0: 500, 7200: 492, 7500: 490, 8400: 430, 8700: 340,
    9060: 325, 9600: 318, 10800: 315, 23340: 315,
  };
  for (const k of u.keyframes) {
    if (!(k.t in strengths)) throw new Error(`unexpected Lowrance keyframe t=${k.t}`);
    k.strength = strengths[k.t]!;
  }
  const k0 = kfAt(u, 0);
  k0.citation = (k0.citation ?? "") +
    " — RE-BASED (day-expansion slice 1, pass-3 primary): 'about 500 men, without any field officers, excepting " +
    "Lieutenant-Colonel Gordon and myself ... In this depressed, dilapidated, and almost unorganized condition, I " +
    "took command' (or-27-2-lowrance, July 1 evening — the July 3 step-off basis, July 2 skirmish wear 'slight' " +
    "and non-separable); the old 1,250 was the PRE-July-1 compilation figure (July 1 cost the brigade ~500 of it, " +
    "or-27-2-scales; dossier csa-3c-pen-4-lowrance.md EC2)";
  kfAt(u, 7200).citation =
    "bombardment: 'here we remained at least one hour, under a most galling fire of artillery, which ... the men " +
    "endured with the coolness and determined spirit of veterans' (or-27-2-lowrance) — no figure; −8 inferred " +
    "(≈ the column's attested ~2% class)";
  kfAt(u, 8400).citation =
    "the advance 'over a wide, hot, and already crimson plain'; at two-thirds of the way 'troops from the front " +
    "came tearing through our ranks, which caused many of our men to break, but with the remaining few we went " +
    "forward' (or-27-2-lowrance) — the washback's ATTRITION share inferred (the breakage itself is a " +
    "present-measure fact and rides this citation, not the strength value)";
  kfAt(u, 8700).citation =
    "the works: 'the right of the brigade touched the enemy's line of breastworks' (or-27-2-lowrance) — the " +
    "Bryan/Angle seam; THE JOINT PRESENT-MEASURE PIN rides here as scope, not value (ED-49 discipline): the two " +
    "brigades (with Lane) were 'not numbering in all 800 guns' at the works — a men-present measure; this track " +
    "is the attrition measure (casualties cumulated), a different scale, carried without reconciling";
  kfAt(u, 9060).citation =
    "'without orders, the brigade retreated, leaving many on the field' (or-27-2-lowrance)";
  kfAt(u, 10800).citation =
    "rallied 'on the same line where it was first formed' (or-27-2-lowrance); END INFERRED-BOUNDED: NO July-3 loss " +
    "primary exists — the 535 return is three-day with July 1 dominant and Scales's own July-1 table (545) LARGER " +
    "than it (or-27-2-anv-return p. 345 vs or-27-2-scales — the ED-49 exemplar conflict, carried); ~185 (37%) " +
    "inferred from the pass-3 'very heavy in proportion' bound (dossier csa-3c-pen-4-lowrance.md EC6)";
  kfAt(u, 10800).confidence = "inferred";
}

// ---------------------------------------------------------------------------
// 3. csa-brockenbrough — ED-48b: the 148 k+w loss floor replaces the 100
// ---------------------------------------------------------------------------
{
  const u = unit("csa-brockenbrough");
  if (kfAt(u, 10800).strength !== 780) throw new Error("expected the 100-decay end");
  const strengths: Record<number, number> = {
    0: 880, 7200: 873, 7500: 870, 7920: 806, 8400: 762,
    9000: 740, 10800: 732, 23340: 732,
  };
  for (const k of u.keyframes) {
    if (!(k.t in strengths)) throw new Error(`unexpected Brockenbrough keyframe t=${k.t}`);
    k.strength = strengths[k.t]!;
  }
  const k0 = kfAt(u, 0);
  k0.citation = (k0.citation ?? "") +
    " — ED-48 EXECUTED (day-expansion slice 1): (a) base 880 KEPT — the Mayo-hop '800 muskets' ≈ 880 working " +
    "basis with the 800–1,100 range carried, no promotion to a clean number (the Stone Sentinels page duplicates " +
    "Davis's figures byte-for-byte — permanently unusable, standing DO-NOT-USE); (b) the decay below re-derives to " +
    "the return's 148 k+w LOSS FLOOR (or-27-2-anv-return p. 344 — missing unreported, so 148 is 'at least'); " +
    "(c) the two-wing rendering stays reconstruction-grade (dossier csa-3c-het-2-brockenbrough.md; ED-48)";
  kfAt(u, 10800).citation =
    "ED-48b END: 880 − 148 = 732, the return's regiment-grain k+w consumed as the loss FLOOR (missing unreported; " +
    "or-27-2-anv-return p. 344; ED-49 rule 2 — an absent missing column is a GAP, not a zero) — supersedes the " +
    "old 100-casualty decay, which sat BELOW the documented floor (flagged by ED-48, now executed)";
  kfAt(u, 10800).confidence = "inferred";
  const parentAt = (t: number): number => {
    const kfs = u.keyframes;
    if (t <= kfs[0]!.t) return kfs[0]!.strength;
    for (let i = 1; i < kfs.length; i++)
      if (t <= kfs[i]!.t) {
        const a = kfs[i - 1]!, b = kfs[i]!;
        return a.strength + (t - a.t) / (b.t - a.t) * (b.strength - a.strength);
      }
    return kfs[kfs.length - 1]!.strength;
  };
  for (const wing of battle.units.filter((c) => c.parent === "csa-brockenbrough"))
    for (const k of wing.keyframes)
      k.strength = Math.round(parentAt(k.t) / 2);
}

// ---------------------------------------------------------------------------
// 4. Confirmed bases — citation upgrades only (values already primary)
// ---------------------------------------------------------------------------
{
  const fry0 = kfAt(unit("csa-fry"), 0);
  fry0.citation = (fry0.citation ?? "") +
    " — BASIS CONFIRMED PRIMARY WITH ITS SCOPE (pass 3, day-expansion slice 1): 'We went into the fight on the 1st " +
    "with 1,048 men' (or-27-2-shepard) — the build's figure IS the primary, but it is JULY-1-scoped; July 3 " +
    "step-off = 1,048 minus July 1 losses (~75 captured at the railroad cut + the day's k&w), no July-3 morning " +
    "report survives — subtraction-only, carried as the same heterogeneity class as Marshall/Davis (dossier " +
    "csa-3c-het-3-fry.md EC2); value UNCHANGED (Angle-cast: the shipped film compiles from this track)";
}
{
  const lane = unit("csa-lane");
  const k0 = kfAt(lane, 0);
  k0.citation = (k0.citation ?? "") +
    " — BASIS CONFIRMED PRIMARY (ED-47, day-expansion slice 1): 'an effective total of 1,355, including ambulance " +
    "corps and rear guard' with July 1–2 loss 'but slight' (or-27-2-lane-jh) — a serviceable July-3 basis, unlike " +
    "the wing's July-1-scoped figures; the 660 decay is CONFIRMED as the battle-total basis (the return's 389 is " +
    "the k+w component measure per its own footnote — different measures, not competing counts)";
  const k8700 = kfAt(lane, 8700);
  k8700.citation = (k8700.citation ?? "") +
    "; THE JOINT PRESENT-MEASURE PIN (ED-49 scope discipline): with Lowrance the two brigades were 'not numbering " +
    "in all 800 guns' at the works (or-27-2-lowrance) — a men-present measure against this attrition track " +
    "(casualties cumulated), carried as scope, not authored as a value";
}
{
  const k0 = kfAt(unit("csa-wilcox"), 0);
  k0.citation = (k0.citation ?? "") +
    " — BASIS CONFIRMED PRIMARY (pass 3, day-expansion slice 1): 'My brigade, about 1,200 in number' " +
    "(or-27-2-wilcox) — and the arithmetic closes to the man: 1,777 pre-battle − 577 July 2 = 1,200; July 3 loss " +
    "204 (his per-day split, the batch's cleanest; Alexander concurs '204 killed and wounded') gives the standing " +
    "996 end (dossier csa-3c-and-1-wilcox.md EC2/EC6)";
}
{
  const k0 = kfAt(unit("csa-lang"), 0);
  k0.citation = (k0.citation ?? "") +
    " — BASIS CONFIRMED PRIMARY (pass 3, day-expansion slice 1): 'The brigade went into action near 700 strong, " +
    "and lost ... 455' (or-27-2-lang = the return p. 343 exactly); July 2 'about 300' (his own figure) leaves the " +
    "~400 July-3 start authored here, and the July-3 share ≈ 155 gives the standing 245 end — 65% of strength, " +
    "the highest documented loss RATE in the assault's orbit (dossier csa-3c-and-4-lang.md EC2/EC6)";
}

// ---------------------------------------------------------------------------
// 5. Marshall / Davis — the honest non-re-base, annotated loudly
// ---------------------------------------------------------------------------
for (const [id, note] of [
  ["csa-marshall",
    " — RE-BASING DEFERRED, HETEROGENEITY RECORDED (day-expansion slice 1): the 2,000 is a JULY-1 'present' " +
    "figure (stone-sentinels compilation); July 3 basis ≈ 900 by subtraction of ~1,100 July-1 k&w [B tier, no " +
    "primary morning report — dossier csa-3c-het-1-marshall.md EC2]. The pass-3 re-basing (ED-46/47/48) executed " +
    "on this brigade's neighbors used PRIMARIES; none exists here — a [B] subtraction re-base is an owner ruling, " +
    "not an executor default. Residual for slice 2."],
  ["csa-davis",
    " — RE-BASING DEFERRED, HETEROGENEITY RECORDED (day-expansion slice 1): the 2,000 is a JULY-1 'present' " +
    "figure (stone-sentinels compilation); July 3 went in with all FOUR regiments (11th Miss fresh atop July-1 " +
    "survivors — the batch's one fresh-regiment case) and no morning report survives [dossier " +
    "csa-3c-het-4-davis.md EC2]. No primary to re-base to — recorded, not hidden. Residual for slice 2."],
] as const) {
  const k0 = kfAt(unit(id), 0);
  k0.citation = (k0.citation ?? "") + note;
}

// ---------------------------------------------------------------------------
// the film guard, then export
// ---------------------------------------------------------------------------
for (const id of CAST) {
  const after = JSON.stringify(
    unit(id).keyframes.filter((k) => k.t > 8040 && k.t <= 9000));
  if (after !== inSliceBefore.get(id))
    throw new Error(`FILM GUARD: '${id}' in-slice keyframes changed — the tripwire fires; revert`);
}
// reconciliation preview (the compiler enforces ±1; fail fast here): the
// pinned slice edge must equal the compiled slice-start strength exactly
{
  const g = unit("csa-garnett");
  if (kfAt(g, 8040).strength !== 1393)
    throw new Error("FILM GUARD: Garnett slice-edge pin must be 1393");
}
writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(
  "re-basing written: Garnett 1427 (pin 1393@8040, end 486), Lowrance 500 (end 315), " +
  "Brockenbrough floor 148 (end 732); Fry/Lane/Wilcox/Lang confirmed; Marshall/Davis deferred-annotated; " +
  "Garnett children + Brockenbrough wings re-split");
