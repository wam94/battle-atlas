// BOMBARDMENT-PRELUDE (P1, docs/reconstruction/audit/charge-intensity-proposals.md
// §4 P1) — the residual pass over the ~1-hour July 3 cannonade's CSA-infantry
// casualties. Inventory (this script's own header, and the branch report
// docs/reconstruction/audit/bombardment-prelude.md, carry the full table):
// most of the assault column's bombardment share is ALREADY time-authored —
// csa-garnett (ED-46, day-expansion slice 1, PRIMARY: Peyton's "about 20
// killed and wounded"), csa-davis (wave A2, PRIMARY: "2 men were killed and
// 21 wounded"), csa-lowrance/csa-wilcox/csa-lang (day-expansion slice 1,
// each with its own attested quote, even where no exact figure exists). The
// RESIDUAL — three brigades whose t=7200 bombardment keyframe still carries
// day-expansion-1's ORIGINAL generic placeholder citation ("interpolated:
// [line/second line] static under bombardment (Alexander 1907)", the exact
// text every brigade in the file started with before wave A2 / day-expansion
// slice 1 touched it) — is:
//   - csa-marshall (+ children csa-11nc/26nc/47nc/52nc): dossier
//     csa-3c-het-1-marshall.md EC5 — "under the two-hour fire, division
//     record [A window]" — no brigade-specific figure.
//   - csa-brockenbrough (+ wings csa-brock-left/csa-brock-right): dossier
//     csa-3c-het-2-brockenbrough.md EC5 — "under the two-hour fire with the
//     division [A window, division record]; no brigade figure (open)".
//   - csa-lane: dossier csa-3c-pen-2-lane.md EC5 — "the two hours' exposure"
//     (Engelhard, Pender's/Trimble's division record) — "no brigade figure
//     (open)".
// (csa-kemper/csa-armistead/csa-fry carry the SAME unfixed generic
// placeholder but are Angle-cast — deferred to film-v2 scope, NOT touched
// here; see the branch report's inventory table.)
//
// THE FIX (moving WHEN, never HOW MUCH): none of these three brigades has a
// primary bombardment-specific casualty count (division-level reports give
// timing — Davis's division report, or-27-2-davis-jr: "About 1 p.m. the
// artillery along our entire line opened ... For two hours the fire was
// heavy and incessant ... The artillery ceased firing at 3 o'clock" for
// Heth's division (Marshall/Brockenbrough); Engelhard's division record,
// "the two hours' exposure", for Pender's/Trimble's (Lane) — but NEITHER
// gives a brigade-grain figure for these three). Per V&R doctrine (no
// invented totals) the ALREADY-ESTABLISHED t=0->t=7200 loss (13/10/17 men)
// is NOT changed — it is re-timed to accrue across the attested CA-J3A
// bracket (signal guns ~13:07, Alexander 1907 / Jacobs 1864 — already on
// every one of these brigades' own t=0 citations — through the division
// record's "ceased at 3 o'clock" / "two hours' exposure" close, ~15:00)
// instead of sitting behind one generic label with no anchor to the actual
// clock. Two new keyframes per unit:
//   t=420  (13:07, CA-J3A-1 signal guns): strength UNCHANGED from t=0 (no
//           loss in the instant the guns open) — pins the moment.
//   t=3600 (14:00, mid-bombardment): half the ALREADY-ESTABLISHED t=0->7200
//           loss (Math.round, matching the corpus's own even-split
//           convention — decomposition-wave-1's 6th Wisconsin / the 21st
//           Mississippi's 1,598/4 rounding) — visualizes the thinning
//           happening DURING the fire, not just retroactively labeled at
//           its close.
// The existing t=7200 keyframe's STRENGTH is left untouched (only its
// citation is upgraded); t=7500 (step-off) and everything after is not
// touched at all. Children/wings re-derive via the SAME
// round(parentAt(t)/N) formula the day-expansion-slice-1 / ED-75 scripts
// already used for these exact families (N=4 for Marshall's regiments, N=2
// for Brockenbrough's wings) — consistent, not a new convention.
//
// FILM SAFETY: none of these three units (or their children) is Angle-cast.
// Nothing at t>=7500 is touched, so compiled per-second states at t>=8160
// are trivially unchanged (guarded below: byte-diff of every touched unit's
// keyframes with t>=7500 before vs after). The 13-cast-unit byte-identity
// check (guarded separately, and re-verified externally per the branch
// report) is not even exercised by this script, since it touches none of
// them — verified anyway, belt-and-suspenders.
//
// Run from tool/:  npx vite-node scripts/author-bombardment-prelude.ts
// Report: docs/reconstruction/audit/bombardment-prelude.md
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
  if (!u) throw new Error(`bombardment-prelude: unit '${id}' missing`);
  return u;
}
function kfAt(u: Unit, t: number): Keyframe {
  const k = u.keyframes.find((k) => k.t === t);
  if (!k) throw new Error(`bombardment-prelude: '${u.id}' has no keyframe at t=${t}`);
  return k;
}

// The film guard: the 13 Angle-cast units must stay byte-identical (this
// script touches none of them, but the guard is unconditional — belt and
// suspenders, matching every prior wave's own methodology).
const CAST = ["csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
  "us-webb", "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing",
  "us-btty-cowan", "us-btty-arnold", "us-hall", "us-stannard"];
const castBefore = new Map<string, string>();
for (const id of CAST) castBefore.set(id, JSON.stringify(unit(id)));

// Non-cast residual units: everything at t>=7500 must be byte-identical
// after this script (the "moving WHEN, never HOW MUCH" guard, generalized
// to the non-cast units this pass touches).
const RESIDUAL_FAMILY = ["csa-marshall", "csa-11nc", "csa-26nc", "csa-47nc", "csa-52nc",
  "csa-brockenbrough", "csa-brock-left", "csa-brock-right", "csa-lane"];
const postStepOffBefore = new Map<string, string>();
for (const id of RESIDUAL_FAMILY)
  postStepOffBefore.set(id, JSON.stringify(unit(id).keyframes.filter((k) => k.t >= 7500)));

const GENERIC_PLACEHOLDER = /^interpolated: (line|second line) static under bombardment \(Alexander 1907\)/;

function insertKeyframe(u: Unit, kf: Keyframe) {
  const idx = u.keyframes.findIndex((k) => k.t > kf.t);
  if (idx < 0) u.keyframes.push(kf);
  else u.keyframes.splice(idx, 0, kf);
}

function signalKeyframe(base: Keyframe, note: string): Keyframe {
  return {
    t: 420, x: base.x, z: base.z, facing: base.facing, formation: base.formation,
    strength: base.strength, confidence: "inferred",
    citation: `CA-J3A-1: the signal guns open the cannonade ~13:07 (Alexander 1907's ~1:00 p.m. start / ` +
      `Jacobs 1864's more precise 1:07 p.m. — both already carried on this brigade's t=0 keyframe). ${note} ` +
      `No loss yet at the signal itself — this pin only anchors WHEN the brigade's already-attested bombardment ` +
      `share (below) begins to accrue; strength unchanged from the arrival figure (bombardment-prelude pass).`,
  };
}

function midpointKeyframe(base: Keyframe, strength: number, divisionQuote: string,
  dossierNote: string, shareMen: number, shareFrom: number, shareTo: number): Keyframe {
  return {
    t: 3600, x: base.x, z: base.z, facing: base.facing, formation: base.formation,
    strength, confidence: "inferred",
    citation: `mid-bombardment (~14:00): ${divisionQuote} — no brigade-specific casualty figure exists for ` +
      `this brigade (${dossierNote}), so the already-established ${shareMen}-man bombardment share ` +
      `(${shareFrom}→${shareTo} by ~15:00, UNCHANGED total from the prior build) is spread across the ` +
      `attested window rather than landing all at once at its close; this keyframe marks the even-split ` +
      `midpoint (compilation-class, inferred — matches the corpus's own even-split convention, e.g. ` +
      `decomposition-wave-1's 6th Wisconsin technique and the 21st Mississippi's 1,598/4 rounding, ` +
      `residuals-decomp-2 task 3a) — NOT an attested casualty-rate claim (bombardment-prelude pass).`,
  };
}

function retimedCitation(divisionQuote: string, dossierNote: string,
  shareMen: number, shareFrom: number, shareTo: number): string {
  return `BOMBARDMENT SHARE, RE-TIMED (bombardment-prelude pass, docs/reconstruction/audit/` +
    `charge-intensity-proposals.md §4 P1): ${divisionQuote} anchors the CA-J3A bracket (signal guns ~13:07; ` +
    `order to advance ~15:00-15:05, matching this brigade's own step-off keyframe below); no brigade-specific ` +
    `casualty figure survives (${dossierNote}) — the already-established ${shareMen}-man share ` +
    `(${shareFrom}→${shareTo}, UNCHANGED total from the prior build) is re-timed to accrue across the ` +
    `attested window instead of sitting behind a flat generic interpolation label; confidence stays inferred ` +
    `(no brigade-grain primary exists to promote it).`;
}

const HETH_DIVISION_QUOTE = "'About 1 p.m. the artillery along our entire line opened ... For two hours the " +
  "fire was heavy and incessant ... The artillery ceased firing at 3 o'clock' (or-27-2-davis-jr, Heth's " +
  "division report)";
const PENDER_DIVISION_QUOTE = "'the two hours' exposure' (or-27-2-engelhard, Pender's/Trimble's division record)";

// ---------------------------------------------------------------------------
// 1. csa-marshall (+ 4 regiment children, round(parentAt(t)/4))
// ---------------------------------------------------------------------------
{
  const u = unit("csa-marshall");
  const k0 = kfAt(u, 0);
  const k7200 = kfAt(u, 7200);
  if (!GENERIC_PLACEHOLDER.test(k7200.citation ?? ""))
    throw new Error("csa-marshall t=7200 citation no longer matches the expected pre-pass placeholder");
  const loss = k0.strength - k7200.strength; // 900 - 887 = 13
  const midStrength = k0.strength - Math.round(loss / 2); // 900 - 7 = 893
  insertKeyframe(u, signalKeyframe(k0,
    "Jones's own +65-profiled clock corroborates the same real moment ('about 12 o'clock ... our batteries " +
    "opened', or-27-2-jones-26nc, this brigade's own regiment-grain report)."));
  insertKeyframe(u, midpointKeyframe(k0, midStrength, HETH_DIVISION_QUOTE,
    "dossier csa-3c-het-1-marshall.md EC5: 'under the two-hour fire, division record [A window]'",
    loss, k0.strength, k7200.strength));
  k7200.citation = retimedCitation(HETH_DIVISION_QUOTE,
    "dossier csa-3c-het-1-marshall.md EC5", loss, k0.strength, k7200.strength);

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
  for (const child of battle.units.filter((c) => c.parent === "csa-marshall")) {
    const c0 = kfAt(child, 0);
    const c7200 = kfAt(child, 7200);
    insertKeyframe(child, {
      t: 420, x: c0.x, z: c0.z, facing: c0.facing, formation: c0.formation,
      strength: Math.round(parentAt(420) / 4), confidence: "inferred",
      citation: `derived from brigade track [${signalKeyframe(c0, "").citation}]; slot order unchanged from t=0 ` +
        `(bombardment-prelude pass, round(parentAt(t)/4) — the same formula day-expansion slice 1 / ED-75 used ` +
        `for this family)`,
    });
    insertKeyframe(child, {
      t: 3600, x: c0.x, z: c0.z, facing: c0.facing, formation: c0.formation,
      strength: Math.round(parentAt(3600) / 4), confidence: "inferred",
      citation: `derived from brigade track [mid-bombardment (~14:00), re-timed share, see csa-marshall's own ` +
        `t=3600 keyframe]; round(parentAt(3600)/4) (bombardment-prelude pass)`,
    });
    c7200.citation = `derived from brigade track [${k7200.citation}]; round(parentAt(7200)/4) unchanged ` +
      `(bombardment-prelude pass — value not touched)`;
  }
}

// ---------------------------------------------------------------------------
// 2. csa-brockenbrough (+ 2 wings, round(parentAt(t)/2))
// ---------------------------------------------------------------------------
{
  const u = unit("csa-brockenbrough");
  const k0 = kfAt(u, 0);
  const k7200 = kfAt(u, 7200);
  if (!GENERIC_PLACEHOLDER.test(k7200.citation ?? ""))
    throw new Error("csa-brockenbrough t=7200 citation no longer matches the expected pre-pass placeholder");
  const loss = k0.strength - k7200.strength; // 880 - 873 = 7
  const midStrength = k0.strength - Math.round(loss / 2); // 880 - 4 = 876
  insertKeyframe(u, signalKeyframe(k0, ""));
  insertKeyframe(u, midpointKeyframe(k0, midStrength, HETH_DIVISION_QUOTE,
    "dossier csa-3c-het-2-brockenbrough.md EC5: 'under the two-hour fire with the division [A window, " +
    "division record]; no brigade figure (open)'",
    loss, k0.strength, k7200.strength));
  k7200.citation = retimedCitation(HETH_DIVISION_QUOTE,
    "dossier csa-3c-het-2-brockenbrough.md EC5", loss, k0.strength, k7200.strength);

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
  for (const wing of battle.units.filter((c) => c.parent === "csa-brockenbrough")) {
    const w0 = kfAt(wing, 0);
    const w7200 = kfAt(wing, 7200);
    insertKeyframe(wing, {
      t: 420, x: w0.x, z: w0.z, facing: w0.facing, formation: w0.formation,
      strength: Math.round(parentAt(420) / 2), confidence: "inferred",
      citation: `${signalKeyframe(w0, "").citation} wing offset inferred.`,
    });
    insertKeyframe(wing, {
      t: 3600, x: w0.x, z: w0.z, facing: w0.facing, formation: w0.formation,
      strength: Math.round(parentAt(3600) / 2), confidence: "inferred",
      citation: `mid-bombardment (~14:00), re-timed share, see csa-brockenbrough's own t=3600 keyframe; ` +
        `round(parentAt(3600)/2) (bombardment-prelude pass); wing offset inferred.`,
    });
    w7200.citation = `${k7200.citation} wing offset inferred; round(parentAt(7200)/2) unchanged (value not touched).`;
  }
}

// ---------------------------------------------------------------------------
// 3. csa-lane (no children)
// ---------------------------------------------------------------------------
{
  const u = unit("csa-lane");
  const k0 = kfAt(u, 0);
  const k7200 = kfAt(u, 7200);
  if (!GENERIC_PLACEHOLDER.test(k7200.citation ?? ""))
    throw new Error("csa-lane t=7200 citation no longer matches the expected pre-pass placeholder");
  const loss = k0.strength - k7200.strength; // 1355 - 1340 = 15
  const midStrength = k0.strength - Math.round(loss / 2); // 1355 - 8 = 1347
  insertKeyframe(u, signalKeyframe(k0, ""));
  insertKeyframe(u, midpointKeyframe(k0, midStrength, PENDER_DIVISION_QUOTE,
    "dossier csa-3c-pen-2-lane.md EC5: 'the two hours' exposure' (or-27-2-engelhard); no brigade figure (open)",
    loss, k0.strength, k7200.strength));
  k7200.citation = retimedCitation(PENDER_DIVISION_QUOTE,
    "dossier csa-3c-pen-2-lane.md EC5", loss, k0.strength, k7200.strength);
}

// ---------------------------------------------------------------------------
// guards, then export
// ---------------------------------------------------------------------------
for (const id of CAST) {
  const after = JSON.stringify(unit(id));
  if (after !== castBefore.get(id))
    throw new Error(`FILM GUARD: Angle-cast unit '${id}' changed — the tripwire fires; revert`);
}
for (const id of RESIDUAL_FAMILY) {
  const after = JSON.stringify(unit(id).keyframes.filter((k) => k.t >= 7500));
  if (after !== postStepOffBefore.get(id))
    throw new Error(`STEP-OFF GUARD: '${id}' keyframes at t>=7500 changed — moving WHEN must never move HOW MUCH`);
}
// conservation preview (printed, not silently assumed)
for (const [parentId, n] of [["csa-marshall", 4], ["csa-brockenbrough", 2]] as const) {
  const p = unit(parentId);
  const children = battle.units.filter((c) => c.parent === parentId);
  for (const t of [420, 3600, 7200]) {
    const pv = kfAt(p, t).strength;
    const cv = children.reduce((s, c) => s + kfAt(c, t).strength, 0);
    console.log(`conservation ${parentId}@t=${t}: parent=${pv} children_sum=${cv} (Δ${cv - pv}, ${n} children)`);
  }
}
writeFileSync(battlePath, exportValidated(battle) + "\n");
console.log(
  "bombardment-prelude written: csa-marshall 900→893→887 (t=420/3600/7200), " +
  "csa-brockenbrough 880→876→873, csa-lane 1355→1347→1340; " +
  "children/wings re-split (round(parentAt(t)/N)); t>=7500 and the 13 Angle-cast units byte-unchanged.");
