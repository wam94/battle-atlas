// ED-75 — Marshall's and Davis's brigades: PLACEHOLDER July-3 subtraction
// bases (day-expansion slice 2 residual, resolved as a documented inference
// per owner ruling 2026-07-13: "should we do an inference in the meantime as
// a placeholder?" — yes). Neither brigade has a primary July-3 morning-report
// figure (unlike ED-46/47/48's Garnett/Lane/Brockenbrough); both get an
// INFERRED placeholder built from cited secondary/tablet readings, explicitly
// superseded on arrival by Busey & Martin, "Regimental Strengths and Losses
// at Gettysburg" page-level figures (owner has stated purchase intent — the
// Pfanz-gate provisional pattern, ED-64). Run from tool/:
//   npx vite-node scripts/author-ed75-placeholder.ts
// Full derivation + inputs: docs/reconstruction/audit/ed75-placeholder.md.
// Ruling: docs/reconstruction/angle-editorial-decisions.md ED-75.
//
// FILM SAFETY: neither csa-marshall nor csa-davis is in the compiled Angle
// cast (angle.reconstruction.json, slice t=8040..9000) — the CAST list below
// is copied from author-dayexp1-rebase.ts verbatim as a guard, but this
// script does not touch any of those units. The shipped bundle is therefore
// untouched by construction; the guard below fails loud if that ever stops
// being true.
//
// METHOD (both brigades; same class of inference, cited per input):
// arrival strength = (July-1-engaged regiments' post-July-1 survivors) +
// (any regiment fresh to July 3). Where a regiment's own July-1 losses have
// no primary total, the brigade's printed whole-battle return (k+w, ED-49
// scope: missing usually absent) is read as PREDOMINANTLY July-1-incurred —
// the same reading the day-expansion-1 script's own in-file annotation
// already used for Marshall's "~900" figure (now promoted from a citation
// note to the actual keyframe value) — because each brigade's defining
// casualty event (the 26th NC's stand opposite the Iron Brigade; the
// railroad-cut trap) was a July-1 event that both brigades' own dossiers
// document as dominating their loss profile.
//
// csa-marshall: 2,000 (tablet, "present on the first day") minus ~1,100
//   (whole-battle regimental return 190k/915w=1,105, or-27-2-anv-return
//   p.344, read as ~all July-1) = 900 [B, subtraction — already the
//   dossier's own EC2 figure]. Envelope 750-1,050 (Wikipedia's uncited
//   "roughly 2,500" base pole; the return's brigade-wide missing entirely
//   absent, ED-49).
// csa-davis: two terms. (a) the three July-1-engaged regiments (2nd/42nd
//   Miss, 55th NC): 2,000 (tablet's "present on the first day" reads as
//   these three only — the same tablet separately says the 11th Mississippi
//   "joined later") minus ~1,100 (regiment-grain return for these three,
//   232+265+198=695 k+w, or-27-2-anv-return p.344, plus the three
//   regiments' share of the brigade's ~500 missing after crediting the 11th
//   Mississippi's own 37 captured, read as ~all July-1 — the railroad cut,
//   "about three hundred Confederates surrendered") = 900, mirroring
//   Marshall's identical subtraction class. (b) the 11th Mississippi's own
//   July-3 arrival: 393 (TWO independent GBMA/War Department markers agree
//   digit-exact on "Combatants – 393" despite disagreeing on the internal
//   killed/non-casualty split — ED-39 marker-corroboration). Sum: 900 + 393
//   = 1,293 [B, subtraction + monument]. Envelope 1,100-1,550 (the
//   tablet-vs-uncited-ECW "1,508" three-regiment base disagreement; the
//   11th Mississippi's own figure carried elsewhere as "592" via a
//   web-secondary UNVERIFIED citation of Busey & Martin — the exact class
//   of number this ruling is a placeholder FOR, not a substitute for).
//   Davis's one attested PRIMARY delta (the bombardment share, "2 men were
//   killed and 21 wounded," or-27-2-davis-jr) is preserved as an absolute
//   -23 applied to the new base, not rescaled — it is a real headcount, not
//   a percentage of an assumed total.
//
// Every keyframe after t=0 (t=7200 for csa-davis) is PROPORTIONALLY
// rescaled from the existing (ABT-map-reconstruction) decay shape — no
// independent per-keyframe re-derivation exists or is claimed; this is
// exactly the "rescaled decay endpoints" the placeholder ruling calls for,
// not a new reconstruction of the charge.
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
  if (!u) throw new Error(`ed75: unit '${id}' missing`);
  return u;
}
function kfAt(u: Unit, t: number): Keyframe {
  const k = u.keyframes.find((k) => k.t === t);
  if (!k) throw new Error(`ed75: '${u.id}' has no keyframe at t=${t}`);
  return k;
}

// The film guard, copied verbatim from author-dayexp1-rebase.ts: the
// compiled Angle cast must remain untouched. csa-marshall/csa-davis are
// asserted NOT in this list (the actual film-safety property this script
// depends on); the assertion fails loud if that ever stops being true.
const CAST = ["csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
  "us-webb", "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing",
  "us-btty-cowan", "us-btty-arnold", "us-hall", "us-stannard"];
for (const id of ["csa-marshall", "csa-davis"])
  if (CAST.includes(id))
    throw new Error(`ed75 FILM GUARD: '${id}' is unexpectedly in the compiled cast — abort`);
const castSnapshot = new Map<string, string>();
for (const id of CAST)
  castSnapshot.set(id, JSON.stringify(unit(id).keyframes));

const OLD_MARSHALL_DEFERRED_NOTE =
  " — RE-BASING DEFERRED, HETEROGENEITY RECORDED (day-expansion slice 1): the 2,000 is a JULY-1 'present' " +
  "figure (stone-sentinels compilation); July 3 basis ≈ 900 by subtraction of ~1,100 July-1 k&w [B tier, no " +
  "primary morning report — dossier csa-3c-het-1-marshall.md EC2]. The pass-3 re-basing (ED-46/47/48) executed " +
  "on this brigade's neighbors used PRIMARIES; none exists here — a [B] subtraction re-base is an owner ruling, " +
  "not an executor default. Residual for slice 2.";
const OLD_DAVIS_DEFERRED_NOTE =
  " — RE-BASING DEFERRED, HETEROGENEITY RECORDED (day-expansion slice 1): the 2,000 is a JULY-1 'present' " +
  "figure (stone-sentinels compilation); July 3 went in with all FOUR regiments (11th Miss fresh atop July-1 " +
  "survivors — the batch's one fresh-regiment case) and no morning report survives [dossier " +
  "csa-3c-het-4-davis.md EC2]. No primary to re-base to — recorded, not hidden. Residual for slice 2.";

const MARSHALL_ED75_NOTE =
  " — ED-75 PROVISIONAL PLACEHOLDER (owner-ruled 2026-07-13, day-expansion slice 2; supersedes the slice-1 " +
  "deferred annotation): July-3 arrival strength set to 900 (envelope 750-1,050), confidence INFERRED, by " +
  "subtraction — 'present on the first day about 2,000' (West Conf Ave tablet / stone-sentinels compilation, " +
  "inferred tier) minus ~1,100 read as July-1-incurred from the brigade's whole-battle regimental return (190 k " +
  "/ 915 w = 1,105, NO missing column printed at all, or-27-2-anv-return p. 344, ED-49 exemplar) — the 26th NC's " +
  "own primary day-split ('We went in with over 800 men... There came out but 216' July 1; '216... only about " +
  "80' July 4, or-27-2-young-26nc) shows this brigade's dominant loss share fell July 1, the basis for reading " +
  "the printed total as ~all-July-1. Envelope reflects (a) the base-strength disagreement — Wikipedia / " +
  "military-history.fandom's uncited 'roughly 2,500' vs the 2,000 tablet compilation, both attributed-only tier, " +
  "recorded verbatim, NOT averaged — and (b) the return's brigade-wide missing/captured mass being wholly " +
  "absent (ED-49). PRECONDITION: superseded on arrival by Busey & Martin, Regimental Strengths and Losses at " +
  "Gettysburg page-level figures (owner has stated purchase intent) — until then this value is a documented " +
  "inference, not a re-basing, and the decay curve below is proportionally RESCALED from the existing " +
  "ABT-map reconstruction shape, not independently re-derived per keyframe. dossier " +
  "csa-3c-het-1-marshall.md EC2/EC6; docs/reconstruction/audit/ed75-placeholder.md.";
const DAVIS_ED75_NOTE =
  " — ED-75 PROVISIONAL PLACEHOLDER (owner-ruled 2026-07-13, day-expansion slice 2; supersedes the slice-1 " +
  "deferred annotation): July-3 arrival strength set to 1,293 (envelope 1,100-1,550), confidence INFERRED, by " +
  "two-term subtraction. Term 1 (the three July-1-engaged regiments, 2nd/42nd Miss + 55th NC): 2,000 (the West " +
  "Confederate Ave tablet's 'present on the first day' figure, read as these three only — the SAME tablet " +
  "separately states 'Joined later by the 11th Regiment previously on duty guarding trains') minus ~1,100 read " +
  "as July-1-incurred (regiment-grain whole-battle return for these three, 232+265+198=695 k+w, plus this " +
  "trio's share of the brigade's ~500 missing after crediting the 11th Mississippi's own 37 captured, " +
  "or-27-2-anv-return p. 344 — 'about three hundred Confederates surrendered' at the railroad cut, the " +
  "brigade's single dominant loss event, is the basis for the ~all-July-1 reading, mirroring Marshall's " +
  "identical subtraction class) = 900. Term 2, the 11th Mississippi's OWN July-3 arrival: 393 — TWO " +
  "independent GBMA/War Department markers (the main brigade tablet and the Bryan-barn position marker) agree " +
  "digit-exact on 'Combatants – 393' despite disagreeing on the internal killed/non-casualty split (110 vs 100 " +
  "killed, 53 vs 39 non-casualty) — ED-39 marker-corroboration. Sum: 900 + 393 = 1,293. Envelope reflects the " +
  "tablet-vs-uncited-secondary three-regiment base disagreement (Emerging Civil War, uncited, 'carrying some " +
  "1,508 men into battle on July 1st') and the 11th Mississippi's OWN figure being reported elsewhere as 592 " +
  "via an UNVERIFIED web-secondary citation of Busey & Martin (a hop, not a page read — exactly the class of " +
  "number this ruling is a placeholder FOR). The one attested PRIMARY delta this brigade carries — 'In Davis' " +
  "brigade 2 men were killed and 21 wounded' during the two-hour cannonade (or-27-2-davis-jr) — is preserved " +
  "as an absolute -23 applied to the new base (a real headcount, not rescaled), not a percentage. PRECONDITION: " +
  "superseded on arrival by Busey & Martin page-level figures (owner has stated purchase intent). dossier " +
  "csa-3c-het-4-davis.md EC2/EC6; docs/reconstruction/audit/ed75-placeholder.md.";

// ---------------------------------------------------------------------------
// 1. csa-marshall — placeholder base 900, proportional rescale of the decay
// ---------------------------------------------------------------------------
{
  const u = unit("csa-marshall");
  const k0 = kfAt(u, 0);
  if (k0.strength !== 2000) throw new Error("ed75: expected Marshall's slice-1 base 2000");
  if (!k0.citation?.includes(OLD_MARSHALL_DEFERRED_NOTE))
    throw new Error("ed75: Marshall's slice-1 deferred annotation not found verbatim — refusing to guess");
  const scale = 900 / 2000;
  const oldT0 = k0.strength;
  for (const k of u.keyframes) k.strength = Math.round((k.strength / oldT0) * 900);
  const newK0 = kfAt(u, 0);
  if (newK0.strength !== 900) throw new Error("ed75: Marshall t=0 must land on 900 exactly");
  newK0.confidence = "inferred";
  newK0.citation = k0.citation.replace(OLD_MARSHALL_DEFERRED_NOTE, MARSHALL_ED75_NOTE);
  for (const k of u.keyframes) if (k.t !== 0) k.confidence = "inferred";
  void scale;

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
  for (const child of battle.units.filter((c) => c.parent === "csa-marshall"))
    for (const k of child.keyframes) {
      k.strength = Math.round(parentAt(k.t) / 4);
      k.confidence = "inferred";
    }
}

// ---------------------------------------------------------------------------
// 2. csa-davis — placeholder base 1,293; the -23 bombardment PRIMARY is
//    preserved as an absolute delta, the rest of the curve rescales from it
// ---------------------------------------------------------------------------
{
  const u = unit("csa-davis");
  const k0 = kfAt(u, 0);
  if (k0.strength !== 2000) throw new Error("ed75: expected Davis's slice-1 base 2000");
  if (!k0.citation?.includes(OLD_DAVIS_DEFERRED_NOTE))
    throw new Error("ed75: Davis's slice-1 deferred annotation not found verbatim — refusing to guess");
  const oldK7200 = kfAt(u, 7200).strength; // 1977 (2000 - 23 PRIMARY)
  const oldK7500 = kfAt(u, 7500).strength; // 1977 (flat)
  if (oldK7200 !== 1977 || oldK7500 !== 1977)
    throw new Error("ed75: expected Davis's slice-1 bombardment-share keyframes 1977/1977");

  k0.strength = 1293;
  k0.confidence = "inferred";
  k0.citation = k0.citation.replace(OLD_DAVIS_DEFERRED_NOTE, DAVIS_ED75_NOTE);

  const k7200 = kfAt(u, 7200);
  const newK7200 = 1293 - 23; // the attested absolute delta, not a percentage
  k7200.strength = newK7200;
  k7200.confidence = "inferred";
  k7200.citation = (k7200.citation ?? "") +
    " — ED-75: the attested delta (-23, 'In Davis' brigade 2 men were killed and 21 wounded', " +
    "or-27-2-davis-jr) is PRESERVED as an absolute headcount and applied to the ED-75 placeholder base " +
    "(1,293 - 23 = 1,270), not rescaled proportionally — the 23 real casualties do not change with which " +
    "reading of the brigade's total strength is adopted.";
  const k7500 = kfAt(u, 7500);
  k7500.strength = newK7200; // flat, matching the slice-1 shape
  k7500.confidence = "inferred";

  const scale = newK7200 / oldK7200;
  for (const k of u.keyframes) {
    if (k.t <= 7500) continue;
    k.strength = Math.round(k.strength * scale);
    k.confidence = "inferred";
  }

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
  for (const child of battle.units.filter((c) => c.parent === "csa-davis"))
    for (const k of child.keyframes) {
      k.strength = Math.round(parentAt(k.t) / 4);
      k.confidence = "inferred";
    }
}

// ---------------------------------------------------------------------------
// the film guard, then export
// ---------------------------------------------------------------------------
for (const id of CAST) {
  const after = JSON.stringify(unit(id).keyframes);
  if (after !== castSnapshot.get(id))
    throw new Error(`ed75 FILM GUARD: '${id}' keyframes changed — the compiled cast must be untouched; revert`);
}
writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(
  "ED-75 placeholder written: Marshall 900 (end " + kfAt(unit("csa-marshall"), 10800).strength + "), " +
  "Davis 1,293 (end " + kfAt(unit("csa-davis"), 10800).strength + "); children re-split; " +
  "compiled Angle cast untouched (csa-marshall/csa-davis confirmed absent from it)");
