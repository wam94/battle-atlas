// ANGLE-V2 DATA WAVE — the Kemper/Armistead/Fry bombardment re-timing
// (the P1 residual bombardment-prelude EXPLICITLY deferred to film-v2 scope;
// see docs/reconstruction/audit/bombardment-prelude.md §3 and this wave's
// report docs/reconstruction/angle-v2-data.md §2).
//
// AUTHORIZATION: the owner's "angle film v2 authorized" ruling opens the 13
// Angle-cast units' PRE-WINDOW keyframes for exactly this class of fix. The
// film-safety pin on gettysburg-july3.json's cast units is deliberately and
// authorizedly BUMPED by this wave (reconstruction/tests/
// test_retrograde_facing.py::test_pinned_angle_cast_units_byte_identical_to_main
// carries the new sha with the authorization note). The INVARIANT that
// replaces the byte-pin for this pass: nothing at t>=7500 (step-off) changes
// for any touched unit — the in-window compiled state at t=8160.. is
// bit-identical by construction ("moving WHEN, never HOW MUCH", the
// bombardment-prelude method applied verbatim).
//
// THE FIX, per brigade (same two-keyframe pattern as author-bombardment-
// prelude.ts, which fixed the non-cast residuals csa-marshall/
// csa-brockenbrough/csa-lane):
//   t=420  (13:07, CA-J3A-1 signal guns): strength UNCHANGED from t=0.
//   t=3600 (~14:00 mid-bombardment): half the ALREADY-ESTABLISHED t=0->7200
//          loss (Math.round — the corpus's even-split convention).
//   t=7200 keyframe: VALUE untouched; citation upgraded from the generic
//          "interpolated: ... (Alexander 1907)" placeholder to the actual
//          evidence chain.
//   t>=7500: byte-identical (guarded).
//
// EVIDENCE (dossiers, quotes carried in the citations below):
//   csa-kemper    loss 25 (1575->1550). No brigade figure; Wilcox's severity
//                 comparison ("suffered severely", or-27-2-wilcox, dossier
//                 csa-1c-pic-2-kemper.md EC5.1/EC6) sizes Kemper's share
//                 ABOVE Garnett's attested ~20 — the existing 25 already
//                 satisfies the relation and is NOT changed (V&R §2: a
//                 qualitative comparison never becomes an invented total).
//   csa-armistead loss 20 (1650->1630). Second line; "no brigade-specific
//                 bombardment casualty figure survives" (dossier
//                 csa-1c-pic-3-armistead.md EC5.2/"Conflicts" item 2).
//   csa-fry       loss 18 (1048->1030). Heth's division record carries the
//                 timing (or-27-2-davis-jr, the same division quote the
//                 bombardment-prelude used for Marshall/Brockenbrough);
//                 "no brigade figure (open)" (dossier csa-3c-het-3-fry.md
//                 EC5.2).
// Children (five macro child regiments per brigade — Kemper's and
// Armistead's Virginia regiments, Fry's Tennessee/Alabama regiments)
// re-derive via round(childAt0/parentAt0 * parentAt(t)) — the exact
// share formula the existing child keyframes verifiably follow at every
// keyframe time (verified against the pre-pass file: 0 mismatches across
// all 15 children).
//
// Run from tool/:  npx vite-node scripts/author-bombardment-v2.ts
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { createHash } from "node:crypto";
import { dirname, join } from "node:path";
import type { Battle, Keyframe, Unit } from "../src/model";
import { exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
const battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));
const byId = new Map(battle.units.map((u) => [u.id, u]));
function unit(id: string): Unit {
  const u = byId.get(id);
  if (!u) throw new Error(`bombardment-v2: unit '${id}' missing`);
  return u;
}
function kfAt(u: Unit, t: number): Keyframe {
  const k = u.keyframes.find((k) => k.t === t);
  if (!k) throw new Error(`bombardment-v2: '${u.id}' has no keyframe at t=${t}`);
  return k;
}

const CAST = ["csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
  "us-webb", "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing",
  "us-btty-cowan", "us-btty-arnold", "us-hall", "us-stannard"];

// The three cast brigades this wave is AUTHORIZED to touch, plus their
// macro children. Every other unit in the file must stay byte-identical
// (including the other ten cast units).
const TOUCHED_BRIGADES = ["csa-kemper", "csa-armistead", "csa-fry"];
const TOUCHED = new Set<string>(TOUCHED_BRIGADES);
for (const b of TOUCHED_BRIGADES)
  for (const c of battle.units.filter((u) => u.parent === b)) TOUCHED.add(c.id);

const untouchedBefore = new Map<string, string>();
for (const u of battle.units)
  if (!TOUCHED.has(u.id)) untouchedBefore.set(u.id, JSON.stringify(u));

// The step-off invariant: everything at t>=7500 must be byte-identical
// after this script for every touched unit (moving WHEN, never HOW MUCH).
const postStepOffBefore = new Map<string, string>();
for (const id of TOUCHED)
  postStepOffBefore.set(id, JSON.stringify(
    unit(id).keyframes.filter((k) => k.t >= 7500)));

const GENERIC_PLACEHOLDER =
  /^interpolated: (line|second line) static under bombardment \(Alexander 1907\)/;
const DERIVED_PLACEHOLDER =
  /derived from brigade track \[interpolated: (line|second line) static under bombardment \(Alexander 1907\)/;

function insertKeyframe(u: Unit, kf: Keyframe) {
  const idx = u.keyframes.findIndex((k) => k.t > kf.t);
  if (idx < 0) u.keyframes.push(kf);
  else u.keyframes.splice(idx, 0, kf);
}

function signalKeyframe(base: Keyframe): Keyframe {
  return {
    t: 420, x: base.x, z: base.z, facing: base.facing, formation: base.formation,
    strength: base.strength, confidence: "inferred",
    citation: `CA-J3A-1: the signal guns open the cannonade ~13:07 (Alexander 1907's ~1:00 p.m. start / ` +
      `Jacobs 1864's more precise 1:07 p.m. — both already carried on this brigade's t=0 keyframe). ` +
      `No loss yet at the signal itself — this pin only anchors WHEN the brigade's already-established bombardment ` +
      `share (below) begins to accrue; strength unchanged from the arrival figure (angle-v2-data wave, ` +
      `owner-authorized film-v2 scope; the bombardment-prelude pattern applied to the deferred cast brigades).`,
  };
}

function midpointKeyframe(base: Keyframe, strength: number, evidence: string,
  dossierNote: string, shareMen: number, shareFrom: number, shareTo: number): Keyframe {
  return {
    t: 3600, x: base.x, z: base.z, facing: base.facing, formation: base.formation,
    strength, confidence: "inferred",
    citation: `mid-bombardment (~14:00): ${evidence} — no brigade-specific casualty figure exists for ` +
      `this brigade (${dossierNote}), so the already-established ${shareMen}-man bombardment share ` +
      `(${shareFrom}→${shareTo} by ~15:00, UNCHANGED total from the prior build) is spread across the ` +
      `attested window rather than landing all at once at its close; this keyframe marks the even-split ` +
      `midpoint (compilation-class, inferred — the corpus's even-split convention, e.g. decomposition-wave-1's ` +
      `6th Wisconsin technique) — NOT an attested casualty-rate claim (angle-v2-data wave).`,
  };
}

function retimedCitation(evidence: string, dossierNote: string,
  shareMen: number, shareFrom: number, shareTo: number): string {
  return `BOMBARDMENT SHARE, RE-TIMED (angle-v2-data wave, owner-authorized film-v2 scope; ` +
    `docs/reconstruction/audit/charge-intensity-proposals.md §4 P1, the bombardment-prelude §3 deferral ` +
    `picked up): ${evidence} anchors the CA-J3A bracket (signal guns ~13:07; order to advance ~15:00-15:05, ` +
    `matching this brigade's own step-off keyframe below); no brigade-specific casualty figure survives ` +
    `(${dossierNote}) — the already-established ${shareMen}-man share (${shareFrom}→${shareTo}, UNCHANGED ` +
    `total from the prior build) is re-timed to accrue across the attested window instead of sitting behind ` +
    `a flat generic interpolation label; confidence stays inferred (no brigade-grain primary exists to ` +
    `promote it).`;
}

const HETH_DIVISION_QUOTE = "'About 1 p.m. the artillery along our entire line opened ... For two hours the " +
  "fire was heavy and incessant ... The artillery ceased firing at 3 o'clock' (or-27-2-davis-jr, Heth's " +
  "division report — Fry's brigade is Heth's right)";
const KEMPER_EVIDENCE = "'The brigade lying on my right (Kemper's) suffered severely' during the cannonade — " +
  "Wilcox, watching from ~200 yards to Kemper's right rear (or-27-2-wilcox; the column's only cross-brigade " +
  "bombardment-severity comparison, sizing Kemper's share ABOVE Garnett's attested ~20 — the existing 25-man " +
  "share already satisfies the relation and V&R §2 forbids converting the comparison into a new total)";
const ARMISTEAD_EVIDENCE = "second line under the same cannonade (Alexander 1907; the CA-J3A bracket) — " +
  "no division- or brigade-grain casualty record survives for Armistead's bombardment share";

function fixBrigade(id: string, evidence: string, dossierNote: string,
  childN: number) {
  const u = unit(id);
  const k0 = kfAt(u, 0);
  const k7200 = kfAt(u, 7200);
  if (!GENERIC_PLACEHOLDER.test(k7200.citation ?? ""))
    throw new Error(`${id} t=7200 citation no longer matches the expected pre-pass placeholder`);
  const loss = k0.strength - k7200.strength;
  const midStrength = k0.strength - Math.round(loss / 2);
  insertKeyframe(u, signalKeyframe(k0));
  insertKeyframe(u, midpointKeyframe(k0, midStrength, evidence, dossierNote,
    loss, k0.strength, k7200.strength));
  k7200.citation = retimedCitation(evidence, dossierNote,
    loss, k0.strength, k7200.strength);

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
  const children = battle.units.filter((c) => c.parent === id);
  if (children.length !== childN)
    throw new Error(`${id}: expected ${childN} children, found ${children.length}`);
  for (const child of children) {
    const c0 = kfAt(child, 0);
    const c7200 = kfAt(child, 7200);
    if (!DERIVED_PLACEHOLDER.test(c7200.citation ?? ""))
      throw new Error(`${child.id} t=7200 citation no longer matches the derived placeholder`);
    const share = c0.strength / k0.strength;
    // the share formula the existing child keyframes verifiably follow
    const childAt = (t: number) => Math.round(share * parentAt(t));
    if (childAt(7200) !== c7200.strength)
      throw new Error(`${child.id}: share formula does not reproduce t=7200 ` +
        `(${childAt(7200)} vs ${c7200.strength}) — investigate before authoring`);
    insertKeyframe(child, {
      t: 420, x: c0.x, z: c0.z, facing: c0.facing, formation: c0.formation,
      strength: childAt(420), confidence: "inferred",
      citation: `derived from brigade track [${signalKeyframe(c0).citation}]; slot order unchanged from t=0 ` +
        `(angle-v2-data wave, round(share×parentAt(t)) — the same share formula this family's existing ` +
        `child keyframes follow)`,
    });
    insertKeyframe(child, {
      t: 3600, x: c0.x, z: c0.z, facing: c0.facing, formation: c0.formation,
      strength: childAt(3600), confidence: "inferred",
      citation: `derived from brigade track [mid-bombardment (~14:00), re-timed share, see ${id}'s own ` +
        `t=3600 keyframe]; round(share×parentAt(3600)) (angle-v2-data wave)`,
    });
    c7200.citation = `derived from brigade track [${k7200.citation}]; round(share×parentAt(7200)) ` +
      `unchanged (angle-v2-data wave — value not touched)`;
  }
  console.log(`${id}: ${k0.strength} -> ${midStrength} (t=3600) -> ${k7200.strength} (t=7200, untouched); ` +
    `${children.length} children re-split`);
}

fixBrigade("csa-kemper", KEMPER_EVIDENCE,
  "dossier csa-1c-pic-2-kemper.md EC5.1/EC6: severity comparison only, no total", 5);
fixBrigade("csa-armistead", ARMISTEAD_EVIDENCE,
  "dossier csa-1c-pic-3-armistead.md EC5.2: 'no brigade-specific bombardment casualty figure survives'", 5);
fixBrigade("csa-fry", HETH_DIVISION_QUOTE,
  "dossier csa-3c-het-3-fry.md EC5.2: 'under the two-hour fire with the division; no brigade figure (open)'", 5);

// ---------------------------------------------------------------------------
// guards, then export
// ---------------------------------------------------------------------------
for (const u of battle.units) {
  if (TOUCHED.has(u.id)) continue;
  if (JSON.stringify(u) !== untouchedBefore.get(u.id))
    throw new Error(`SCOPE GUARD: untouched unit '${u.id}' changed — revert`);
}
for (const id of TOUCHED) {
  const after = JSON.stringify(unit(id).keyframes.filter((k) => k.t >= 7500));
  if (after !== postStepOffBefore.get(id))
    throw new Error(`STEP-OFF GUARD: '${id}' keyframes at t>=7500 changed — moving WHEN must never move HOW MUCH`);
}
// conservation preview
for (const parentId of TOUCHED_BRIGADES) {
  const p = unit(parentId);
  const children = battle.units.filter((c) => c.parent === parentId);
  for (const t of [420, 3600, 7200]) {
    const pv = kfAt(p, t).strength;
    const cv = children.reduce((s, c) => s + kfAt(c, t).strength, 0);
    console.log(`conservation ${parentId}@t=${t}: parent=${pv} children_sum=${cv} (Δ${cv - pv})`);
  }
}
writeFileSync(battlePath, exportValidated(battle) + "\n");

// print the NEW cast sha256 for the authorized pin bump (the exact blob the
// pytest pin hashes: {id: unit} over the 13 cast ids, json sort_keys)
const battleAfter = JSON.parse(readFileSync(battlePath, "utf8"));
const castUnits: Record<string, unknown> = {};
for (const u of battleAfter.units) if (CAST.includes(u.id)) castUnits[u.id] = u;
// python json.dumps(sort_keys=True, ensure_ascii=False) equivalence:
function pyDumps(v: unknown): string {
  if (v === null) return "null";
  if (typeof v === "number") {
    if (Number.isInteger(v)) return String(v);
    return String(v);
  }
  if (typeof v === "boolean") return v ? "true" : "false";
  if (typeof v === "string") return JSON.stringify(v);
  if (Array.isArray(v)) return "[" + v.map(pyDumps).join(", ") + "]";
  const o = v as Record<string, unknown>;
  return "{" + Object.keys(o).sort().map(
    (k) => JSON.stringify(k) + ": " + pyDumps(o[k])).join(", ") + "}";
}
const blob = pyDumps(castUnits);
console.log("NEW pinned-cast sha256 (verify against pytest):",
  createHash("sha256").update(blob, "utf8").digest("hex"));
console.log("bombardment-v2 written: kemper 1575→1562→1550, armistead 1650→1640→1630, " +
  "fry 1048→1039→1030 (t=420/3600/7200); children re-split; t>=7500 byte-unchanged.");
