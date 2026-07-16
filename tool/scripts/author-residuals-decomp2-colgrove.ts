// residuals-decomp-2, task 2 — Colgrove split parity: gettysburg-july3.json
// (the FILM FILE, Angle-pinned) gets the SAME us-2ma/us-27in decomposition
// gettysburg-july3-morning.json already carries (ED-76 parentless-sibling
// pattern), closing strength-reconciliation-2 §13 owner question 3 ("worth a
// small decomposition-wave-style pass on the afternoon file specifically for
// this unit").
//
// FILM-SAFETY: `us-colgrove`/`us-2ma`/`us-27in` are NOT among the 13
// Angle-cast units (csa-garnett, csa-kemper, csa-armistead, csa-fry,
// us-webb, us-69pa, us-71pa, us-72pa, us-btty-cushing, us-btty-cowan,
// us-btty-arnold, us-hall, us-stannard) — verified by direct membership
// check before this script runs (residuals-decomp-2.md's film-safety
// section). `addUnitsAfter` only splices after the anchor id and edits the
// one existing unit named; it never reads or writes any other unit.
//
// STRENGTH: already reconciled (strength-reconciliation-2 §1.2) — this
// file's own us-colgrove t=0/t=10800 already read 891, the family total
// (csa-colgrove 482 + us-2ma 180 + us-27in 229 from gettysburg-july3-
// morning.json's own decomposed EndT keyframes). This pass does NOT change
// any strength value — it only splits the single 891-strength unit into the
// three units that already sum to it, matching the morning file's own unit
// count and naming (ED-76's "minus the Nth ___" convention).
//
// POSITION: this file's us-colgrove keyframes are STATIC (t=0/t=10800,
// "documented silence" — the corps' fight ended at 10:30 a.m., pre-window;
// 13:00-16:00 the works stand fully manned and silent). The morning file's
// own EndT positions for the three post-decomposition units are used
// directly (continuity, not invention): csa-colgrove-residual (6080,4800,
// f30) — IDENTICAL to this file's existing us-colgrove position already;
// us-2ma (6096,4865,f30); us-27in (6120,4885,f30) — each regiment's own
// distinct ground within the works, already attested in the morning file.
//
// Run from tool/:
//   npx vite-node scripts/author-residuals-decomp2-colgrove.ts
// Report: docs/reconstruction/audit/residuals-decomp-2.md
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, Keyframe, Unit } from "../src/model";
import { addUnitsAfter, exportValidated } from "./fullcast-lib";

const ANGLE_CAST = [
  "csa-garnett", "csa-kemper", "csa-armistead", "csa-fry", "us-webb",
  "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing", "us-btty-cowan",
  "us-btty-arnold", "us-hall", "us-stannard",
];

const here = dirname(fileURLToPath(import.meta.url));
const july3Path = join(here, "../../app/Assets/Battle/gettysburg-july3.json");

let july3: Battle = JSON.parse(readFileSync(july3Path, "utf8"));

// Film-safety guard, enforced at authoring time, not just after the fact:
// abort loudly if this script is ever pointed at one of the 13 Angle-cast
// units.
for (const id of ["us-colgrove", "us-2ma", "us-27in"])
  if (ANGLE_CAST.includes(id))
    throw new Error(`FILM-SAFETY ABORT: '${id}' is Angle-cast — this script must not touch it`);

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  citation: string, confidence: Keyframe["confidence"] = "documented"): Keyframe =>
  ({ t, x, z, facing, formation, strength, confidence, citation });

function patchUnit(battle: Battle, id: string, edit: (u: Unit) => void) {
  const u = battle.units.find((x) => x.id === id);
  if (!u) throw new Error(`patchUnit: '${id}' missing`);
  edit(u);
}

const ORIG_STRENGTH = 891; // this file's own pre-decomposition value (already the family total)
const RESIDUAL_STRENGTH = 482; // gettysburg-july3-morning.json us-colgrove EndT
const MS2_STRENGTH = 180; // gettysburg-july3-morning.json us-2ma EndT
const IN27_STRENGTH = 229; // gettysburg-july3-morning.json us-27in EndT

const colgroveResidual: Keyframe[] = [
  kf(0, 6080, 4800, 30, "line", RESIDUAL_STRENGTH,
    "Ruger's division tablet verbatim, July 3: 'At daylight attacked the Confederate Infantry and was hotly engaged with charges and countercharges at different points until 10:30 A. M. when the Confederate forces retired.' (stone-sentinels, fetched 2026-07-08); Col. Silas Colgrove; sheet 8 s8_culps.jpg names 'Colgrove Brig' at the line's right, Spangler's Spring/McAllister's woods, facing the meadow. DOCUMENTED SILENCE (battle-format.md): the corps' seven-hour fight ended at 10:30 A.M., pre-window; in 13:00-16:00 the works stand fully manned and SILENT. DECOMPOSED (residuals-decomp-2, 2026-07-16, ED-76 convention, closing strength-reconciliation-2.md §13 owner question 3): this unit now covers the remaining three regiments (13th NJ, 107th NY, 3rd WI) only — the 2nd Massachusetts and 27th Indiana are promoted to their own parentless units (us-2ma, us-27in), matching gettysburg-july3-morning.json's own decomposition exactly. Strength 482 = gettysburg-july3-morning.json's own us-colgrove EndT value (exact continuity, not recomputed here — strength-reconciliation-2 already grounded this file's total at the family sum, 891 = 482+180+229; this pass only splits the count, not the arithmetic)."),
  kf(10800, 6080, 4800, 30, "line", RESIDUAL_STRENGTH,
    "Held — same 'documented silence' reading as the t=0 keyframe; no combat or movement attested for these three regiments in this window."),
];

const ms2: Keyframe[] = [
  kf(0, 6096, 4865, 30, "line", MS2_STRENGTH,
    "At the works, quiet — the 2nd Massachusetts's charge across the meadow (~10 a.m., ED-78 provisional direction) is PRE-window; gettysburg-july3-morning.json's own EndT keyframe for this unit ('Back at the works, quiet — a new parentless unit this phase') is inherited directly: position (6096,4865,facing 30) and strength 180 (316 at the day's opening minus Morse's own itemized loss, 136 — or-27-1-morse-2ma p. 817-818 — carried in full on the morning file's own us-2ma t=10:12/EndT citations). PROMOTED (residuals-decomp-2, 2026-07-16, ED-76 convention, closing strength-reconciliation-2.md §13 owner question 3): this file previously rendered Colgrove's brigade as one undecomposed unit (891, the family total per strength-reconciliation-2 §1.2); now split to match gettysburg-july3-morning.json's own unit count. mon-2ma inscription: 'From the hill behind this monument on the morning of July Third 1863 the Second Massachusetts Infantry made an assault upon the Confederate troops in the works at the base of Culp's Hill opposite.'"),
  kf(10800, 6096, 4865, 30, "line", MS2_STRENGTH,
    "Held — quiet at the works through the window's end; no further combat or movement attested (matching Colgrove's own 'documented silence' reading for this window)."),
];

const in27: Keyframe[] = [
  kf(0, 6120, 4885, 30, "line", IN27_STRENGTH,
    "At the works, quiet — the 27th Indiana's charge across the meadow (~10 a.m., ED-78 provisional direction) is PRE-window; gettysburg-july3-morning.json's own EndT keyframe for this unit is inherited directly: position (6120,4885,facing 30) and strength 229 (339 carried into action, or-27-1-fesler-27in / mon-27in exact agreement, minus the return-basis loss 110 — carried in full on the morning file's own us-27in t=10:15/EndT citations). PROMOTED (residuals-decomp-2, 2026-07-16, ED-76 convention, closing strength-reconciliation-2.md §13 owner question 3): this file previously rendered Colgrove's brigade as one undecomposed unit (891, the family total per strength-reconciliation-2 §1.2); now split to match gettysburg-july3-morning.json's own unit count. Lieut. Col. John R. Fesler commanding; monument 'Number engaged 339' (mon-27in)."),
  kf(10800, 6120, 4885, 30, "line", IN27_STRENGTH,
    "Held — quiet at the works through the window's end; no further combat or movement attested (matching Colgrove's own 'documented silence' reading for this window)."),
];

patchUnit(july3, "us-colgrove", (u) => {
  u.name = "Colgrove's Brigade (3rd Bde, 1st Div, XII Corps — minus the 2nd Massachusetts and 27th Indiana)";
  u.frontage_m = 130; // 482 * 0.27, rounded — this file's own t=0 residual strength
  u.keyframes = colgroveResidual;
});

const ms2Unit: Unit = {
  id: "us-2ma", name: "2nd Massachusetts (Colgrove's)", side: "union",
  frontage_m: 49, // 180 * 0.27, rounded
  depth_m: 20, keyframes: ms2,
};
const in27Unit: Unit = {
  id: "us-27in", name: "27th Indiana (Colgrove's)", side: "union",
  frontage_m: 62, // 229 * 0.27, rounded
  depth_m: 20, keyframes: in27,
};

july3 = addUnitsAfter(july3, "us-colgrove", [ms2Unit, in27Unit]);

// Conservation check (printed, not silently assumed): the three new/edited
// units sum to the ORIGINAL single-unit value at every shared keyframe.
for (const t of [0, 10800]) {
  const sum = RESIDUAL_STRENGTH + MS2_STRENGTH + IN27_STRENGTH;
  if (sum !== ORIG_STRENGTH)
    throw new Error(`conservation break at t=${t}: ${RESIDUAL_STRENGTH}+${MS2_STRENGTH}+${IN27_STRENGTH}=${sum} != ${ORIG_STRENGTH}`);
}
console.log(`conservation: csa-colgrove-residual(${RESIDUAL_STRENGTH}) + us-2ma(${MS2_STRENGTH}) + us-27in(${IN27_STRENGTH}) = ${RESIDUAL_STRENGTH + MS2_STRENGTH + IN27_STRENGTH} == the pre-decomposition family total ${ORIG_STRENGTH}, exact`);

// Angle-cast byte-identity guard: confirm none of the 13 units changed
// object identity / values (belt-and-suspenders on top of the film-safety
// hash check the report performs externally).
for (const id of ANGLE_CAST) {
  const before = JSON.parse(readFileSync(july3Path, "utf8")).units.find((u: Unit) => u.id === id);
  const after = july3.units.find((u) => u.id === id);
  if (JSON.stringify(before) !== JSON.stringify(after))
    throw new Error(`FILM-SAFETY BREAK: Angle-cast unit '${id}' changed`);
}
console.log("film-safety: all 13 Angle-cast units byte-identical, pre- vs post-script (in-process check)");

writeFileSync(july3Path, exportValidated(july3));
console.log(`wrote ${july3Path}: ${july3.units.length} units`);
