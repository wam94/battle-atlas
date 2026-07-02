// Task A5 derivation script (plan: docs/superpowers/plans/2026-07-02-descriptive.md).
// Decomposes the A-grade brigades (Stannard, Webb, Hall) via the A4 helper —
// ids, parent links, frontage shares, roster removal — then EDITS the
// scaffolding into researched tracks, adds the 8th Ohio and Brown's Battery B,
// and fixes Cowan's missing gallop-in. Run from tool/:
//   npx vite-node scripts/author-a5-regiments.ts
// The output is canonical-ordered via exportBattle and validated on the way
// out. Committed as the derivation record for the authored data.
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import { decomposeBrigade } from "../src/decompose";
import { exportBattle } from "../src/io";
import { validateBattle } from "../src/validate";
import type { Battle, Keyframe, Unit } from "../src/model";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const oob = "docs/research/2026-06-13-oob-strengths.md";

// ---- decompose the three A-grade brigades (scaffolding) --------------------

battle = decomposeBrigade(
  battle, "us-stannard",
  new Map([["13th Vermont", 480], ["14th Vermont", 647], ["16th Vermont", 661]]),
  `13th VT monument / Stone Sentinels regiment pages via ${oob}`,
);

// Webb: the 106th PA stays unmodeled as a track (only Cos. A & B were on this
// front — survey §4 item 6); the parent's whole-brigade strength keeps it
// counted, so the ±15% advisory warning on Webb is EXPECTED and correct.
const webb = battle.units.find((u) => u.id === "us-webb")!;
webb.regiments = ["69th Pennsylvania", "71st Pennsylvania", "72nd Pennsylvania"];
battle = decomposeBrigade(
  battle, "us-webb",
  new Map([["69th Pennsylvania", 258], ["71st Pennsylvania", 331], ["72nd Pennsylvania", 458]]),
  `Stone Sentinels regiment pages via ${oob}`,
);

battle = decomposeBrigade(
  battle, "us-hall",
  new Map([
    ["19th Massachusetts", 231], ["20th Massachusetts", 301], ["7th Michigan", 165],
    ["42nd New York", 197], ["59th New York (4 companies)", 182],
  ]),
  `Stone Sentinels regiment pages via ${oob}; 20th MA (301) and 59th NY (182) fetched from Stone Sentinels regiment pages 2026-07-02`,
);

// ---- authored tracks (the edit pass over the scaffolding) ------------------
// Times: battle t is seconds since startTime 46800 = 13:00 LMT (15:20 = 8400).
// Bachelder-derived positions are `documented` positions with `inferred`
// times — the map gives position classes, not clocks. Disagreements are
// carried in citation text, never reconciled.

const edits: Record<string, { frontage_m?: number; depth_m?: number; keyframes: Keyframe[] }> = {
  // ---- Stannard (A): the wheel -------------------------------------------
  "us-13vt": {
    frontage_m: 113,
    keyframes: [
      { t: 0, x: 4279, z: 4587, facing: 260, formation: "line", strength: 480, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('13'Vt' regiment bar on Stannard's advanced line — the map gives position classes, not clocks; time inferred); coordinate = the regiment's RegimentSlots share of the brigade line, right of the 14th; strength 480 'officers and men engaged': 13th VT monument via Stone Sentinels; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4279, z: 4587, facing: 260, formation: "line", strength: 470, confidence: "inferred", citation: "holding the advanced line under the bombardment (Sturtevant 1910); slot-follow of the brigade track" },
      { t: 8100, x: 4279, z: 4587, facing: 260, formation: "line", strength: 466, confidence: "inferred", citation: "Pickett's right passes the brigade front obliquing north toward the Copse (Sturtevant 1910; ABT 3:30-3:45 PM map — ABT clock frame noted, never mixed silently)" },
      { t: 8400, x: 4292, z: 4650, facing: 15, formation: "line", strength: 460, confidence: "inferred", citation: "wheel under way ~15:20: 'change front forward on first company' — the first company pivots, successive companies wheel in (Sturtevant 1910 pp.304-305; Benedict, Vermont in the Civil War vol. 2); clock inferred from the step-off reconstruction" },
      { t: 8700, x: 4326, z: 4764, facing: 15, formation: "line", strength: 450, confidence: "documented", citation: "13th VT monument: 'changed front forward on first company, advanced 200 yards, attacking the Confederate right flank ... capturing 243 prisoners' (Stone Sentinels 13th Vermont page); Sturtevant 1910 pp.304-305 — Pickett's command 'not sixty yards away'; coordinate = ~200 yds forward of the Bachelder line onto Kemper's right; time ~15:25 inferred" },
      { t: 9600, x: 4285, z: 4550, facing: 260, formation: "line", strength: 446, confidence: "inferred", citation: "returns toward the original front after Pickett's repulse (Sturtevant 1910); slot-follow of the brigade track" },
      { t: 10800, x: 4279, z: 4587, facing: 260, formation: "line", strength: 440, confidence: "inferred", citation: "end at the advanced line (reconstruction); brigade losses 350 (Stone Sentinels Strength & Casualties USA table) are not split by regiment — per-regiment decline reconstructed" },
    ],
  },
  "us-14vt": {
    frontage_m: 113,
    keyframes: [
      { t: 0, x: 4300, z: 4470, facing: 260, formation: "line", strength: 647, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('14'Vt' bar, center of Stannard's advanced line; position class, time inferred); strength 647 brought to the field: Stone Sentinels 14th Vermont page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4300, z: 4470, facing: 260, formation: "line", strength: 635, confidence: "inferred", citation: "holding under the bombardment (Sturtevant 1910); slot-follow of the brigade track" },
      { t: 8700, x: 4300, z: 4470, facing: 260, formation: "line", strength: 620, confidence: "inferred", citation: "essentially static through the crisis, firing into Kemper's front as the assault obliques past (Sturtevant 1910; Stone Sentinels 3rd Brig 3rd Div I Corps page)" },
      { t: 10200, x: 4300, z: 4470, facing: 190, formation: "line", strength: 612, confidence: "inferred", citation: "front refused south from its position, joining the 16th's fire against Wilcox and Lang's advance (Stone Sentinels 3rd Brig 3rd Div I Corps page; Veazey's OR report names the 16th's movement — the 14th joins by fire, not movement); clock ~15:50 inferred" },
      { t: 10800, x: 4300, z: 4470, facing: 260, formation: "line", strength: 605, confidence: "inferred", citation: "end (reconstruction); brigade losses 350 (Stone Sentinels Strength & Casualties USA table) not split by regiment" },
    ],
  },
  "us-16vt": {
    frontage_m: 113,
    keyframes: [
      { t: 0, x: 4321, z: 4353, facing: 260, formation: "line", strength: 661, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('16'Vt' bar, left of Stannard's advanced line; position class, time inferred); strength 661 brought to the field: Stone Sentinels 16th Vermont page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4321, z: 4353, facing: 260, formation: "line", strength: 650, confidence: "inferred", citation: "holding under the bombardment (Sturtevant 1910); slot-follow of the brigade track" },
      { t: 8400, x: 4295, z: 4640, facing: 15, formation: "line", strength: 645, confidence: "inferred", citation: "wheels out with the 13th ~15:20, forming on its left for the flank attack (Sturtevant 1910 pp.304-305; Veazey's OR report); clock inferred" },
      { t: 8700, x: 4240, z: 4770, facing: 15, formation: "line", strength: 635, confidence: "inferred", citation: "on the 13th's left firing into the right flank of Pickett's column (Sturtevant 1910; Veazey's OR report); flank-position coordinate inferred from the 13th's documented 200-yd advance" },
      { t: 9900, x: 4260, z: 4520, facing: 190, formation: "line", strength: 628, confidence: "inferred", citation: "recalled and re-forms facing south as Wilcox and Lang step off ~15:45 — change of front to the rear (Veazey's OR report; Benedict vol. 2); clock inferred" },
      { t: 10380, x: 4180, z: 4360, facing: 200, formation: "line", strength: 620, confidence: "documented", citation: "the 16th's reversal: strikes the left flank of Wilcox and Lang's supporting advance (Veazey's OR report; Stone Sentinels 3rd Brig 3rd Div I Corps page — Stannard wounded in the thigh directing the flank attack); time ~15:53 and coordinate inferred from Lang's track" },
      { t: 10800, x: 4321, z: 4353, facing: 260, formation: "line", strength: 615, confidence: "inferred", citation: "returns to the line (reconstruction); brigade losses 350 not split by regiment" },
    ],
  },

  // ---- Webb (A): the Angle ------------------------------------------------
  "us-69pa": {
    frontage_m: 76,
    keyframes: [
      { t: 0, x: 4402, z: 4830, facing: 262, formation: "line", strength: 258, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('69'Pa' bar at the stone wall between the Angle and the Copse; position class, time inferred); strength 258 at the Angle: Stone Sentinels 69th Pennsylvania page; roster and company detail: McDermott 1889; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4402, z: 4830, facing: 262, formation: "line", strength: 245, confidence: "inferred", citation: "prone at the wall under the bombardment's concentric fire on Gibbon's front (McDermott 1889; Waitt 1906 pp.234-237)" },
      { t: 8580, x: 4402, z: 4830, facing: 262, formation: "line", strength: 240, confidence: "inferred", citation: "holds fire at the wall as Pickett's front closes (McDermott 1889; Haskell 1908)" },
      { t: 8700, x: 4402, z: 4830, facing: 262, formation: "line", strength: 200, confidence: "documented", citation: "the regiment never leaves the wall: Cos. I and A change front against the flankers as Armistead crosses, Co. F's captain killed before he could give the command, Co. D 'held the enemy at bay, using their muskets as clubs' (McDermott 1889 verbatim); time ~15:25 inferred" },
      { t: 9000, x: 4402, z: 4830, facing: 262, formation: "line", strength: 185, confidence: "inferred", citation: "wall held, breakthrough sealed (McDermott 1889; Haskell 1908)" },
      { t: 10800, x: 4402, z: 4830, facing: 262, formation: "line", strength: 180, confidence: "inferred", citation: "end: brigade losses 490 (Stone Sentinels Strength & Casualties USA table) not split by regiment — decline reconstructed" },
    ],
  },
  "us-71pa": {
    frontage_m: 76,
    keyframes: [
      { t: 0, x: 4400, z: 4868, facing: 262, formation: "line", strength: 331, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('71'Pa' bar at the Angle wall; position class, time inferred); strength 331 brought to the field: Stone Sentinels 71st Pennsylvania page; 8 companies at the wall, 2 fifty yards right-rear (Smith's OR report via Stone Sentinels — the report names no companies); bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4400, z: 4868, facing: 262, formation: "line", strength: 318, confidence: "inferred", citation: "under the bombardment at the outer-angle wall (Smith's OR report; Waitt 1906 pp.234-237 for the fire on this front)" },
      { t: 8580, x: 4400, z: 4868, facing: 262, formation: "line", strength: 310, confidence: "inferred", citation: "at the wall as the column closes; ~50 men help serve Cushing's guns (Smith's OR report; NPS Hartwig 2012)" },
      { t: 8700, x: 4448, z: 4866, facing: 262, formation: "line", strength: 280, confidence: "documented", citation: "step one of the two-step: falls back from the outer angle at the breakthrough rather than be overrun (Smith's OR report via Stone Sentinels); crest-ward coordinate and time ~15:25 inferred" },
      { t: 9000, x: 4408, z: 4866, facing: 262, formation: "line", strength: 265, confidence: "inferred", citation: "step two: returns to the wall as the breakthrough is sealed (Smith's OR report; Haskell 1908)" },
      { t: 10800, x: 4400, z: 4868, facing: 262, formation: "line", strength: 260, confidence: "inferred", citation: "end: brigade losses 490 not split by regiment — decline reconstructed" },
    ],
  },
  "us-72pa": {
    frontage_m: 76,
    keyframes: [
      { t: 0, x: 4465, z: 4855, facing: 262, formation: "line", strength: 458, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('72'Pa' bar in reserve behind the Angle crest; position class, time inferred); strength 458 'present at Gettysburg' vs 473 'present for duty' — both on Stone Sentinels 72nd Pennsylvania page, conflict kept; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4465, z: 4855, facing: 262, formation: "line", strength: 440, confidence: "inferred", citation: "in reserve behind the crest under the bombardment (Haskell 1908)" },
      { t: 8700, x: 4452, z: 4857, facing: 262, formation: "line", strength: 400, confidence: "documented", citation: "CONTESTED — tracked reading: brought up to the crest and fires from it, not advancing when Webb orders the charge (Webb wounded trying to lead it; Haskell 1908; Medal of Honor citation for Webb); the regiment's monument-litigation counter-reading holds it fought at the wall from the start (72nd PA monument case via HistoryNet) — counter-reading recorded, not adopted; time ~15:25 inferred" },
      { t: 9300, x: 4412, z: 4860, facing: 262, formation: "line", strength: 370, confidence: "inferred", citation: "the final advance to the wall as the penetration collapses ~15:35 (Haskell 1908; survey: advanced only at the end); clock inferred" },
      { t: 10800, x: 4412, z: 4860, facing: 262, formation: "line", strength: 360, confidence: "inferred", citation: "end at the retaken wall (reconstruction); brigade losses 490 not split by regiment" },
    ],
  },

  // ---- Hall (A–): the crisis at the Copse --------------------------------
  // Front-line order at the wall north-to-south per the Bachelder sheet's
  // legible bar order (59'N.Y. / 7'Mich / 20'Ms); 19th MA and 42nd NY in
  // reserve behind (Waitt 1906; Devereux's OR report). Melee-zone positions
  // carry the survey's widest uncertainty (disagreement 9) and stay inferred.
  "us-59ny": {
    frontage_m: 34,
    keyframes: [
      { t: 0, x: 4413, z: 4786, facing: 262, formation: "line", strength: 182, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('59'N.Y.' bar at the wall immediately south of the Copse; position class, time inferred); a four-company battalion of 182 men, consolidated June 1863 — short frontage encoded (Stone Sentinels 59th New York page); bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4413, z: 4786, facing: 262, formation: "line", strength: 175, confidence: "inferred", citation: "prostrate under the bombardment (Waitt 1906 pp.234-237, this brigade's front)" },
      { t: 8580, x: 4413, z: 4786, facing: 262, formation: "line", strength: 172, confidence: "inferred", citation: "at the wall as Kemper's and Armistead's fronts close (Hall's OR report, OR 27/1)" },
      { t: 8700, x: 4455, z: 4788, facing: 262, formation: "routed", strength: 165, confidence: "documented", citation: "the bolt: the battalion 'inexplicably bolted for the rear' at the climax, uncovering Cowan's guns — the moment of Cowan's double canister (Stone Sentinels 59th New York page; Brown 1985 Filson); formation routed; time ~15:25 and rally coordinate inferred; melee-zone positions carry the survey's widest uncertainty (disagreement 9)" },
      { t: 9000, x: 4425, z: 4786, facing: 262, formation: "line", strength: 160, confidence: "inferred", citation: "rallied and returned to the line after the repulse (Hall's OR report; reconstruction)" },
      { t: 10800, x: 4413, z: 4786, facing: 262, formation: "line", strength: 158, confidence: "inferred", citation: "end: brigade losses 365 (Stone Sentinels Strength & Casualties USA table) not split by regiment" },
    ],
  },
  "us-7mi": {
    frontage_m: 31,
    keyframes: [
      { t: 0, x: 4416, z: 4758, facing: 262, formation: "line", strength: 165, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('7'Mich' bar, center of Hall's wall line; position class, time inferred); strength 165 (14 officers, 151 men present for duty): Stone Sentinels 7th Michigan page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4416, z: 4758, facing: 262, formation: "line", strength: 158, confidence: "inferred", citation: "prostrate under the bombardment (Waitt 1906 pp.234-237)" },
      { t: 8700, x: 4410, z: 4788, facing: 262, formation: "line", strength: 145, confidence: "inferred", citation: "Hall shifts his regiments right/north toward the Copse at the break (Hall's OR report, OR 27/1; Haskell 1908); melee-zone position — widest uncertainty (survey disagreement 9); clock ~15:25 inferred" },
      { t: 9000, x: 4408, z: 4792, facing: 262, formation: "line", strength: 135, confidence: "inferred", citation: "close-range fight west of the Copse until the penetration collapses (Hall's OR report; Haskell 1908)" },
      { t: 10800, x: 4416, z: 4758, facing: 262, formation: "line", strength: 130, confidence: "inferred", citation: "end: brigade losses 365 not split by regiment" },
    ],
  },
  "us-20ma": {
    frontage_m: 56,
    keyframes: [
      { t: 0, x: 4419, z: 4726, facing: 262, formation: "line", strength: 301, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('20'Ms' bar, left of Hall's wall line; position class, time inferred); strength 301 brought to the field, losing 30 killed / 94 wounded / 3 missing across the battle: Stone Sentinels 20th Massachusetts page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4419, z: 4726, facing: 262, formation: "line", strength: 288, confidence: "inferred", citation: "prostrate under the bombardment (Waitt 1906 pp.234-237)" },
      { t: 8700, x: 4412, z: 4775, facing: 262, formation: "line", strength: 260, confidence: "inferred", citation: "crowds right toward the Copse with the 7th MI (Hall's OR report, OR 27/1); melee-zone position — widest uncertainty (survey disagreement 9: even Hess's 20th MA / 59th NY placement is critiqued); clock ~15:25 inferred" },
      { t: 9000, x: 4410, z: 4780, facing: 262, formation: "line", strength: 240, confidence: "inferred", citation: "in the melee at the clump until the penetration collapses (Hall's OR report; Haskell 1908)" },
      { t: 10800, x: 4419, z: 4726, facing: 262, formation: "line", strength: 235, confidence: "inferred", citation: "end: brigade losses 365 not split by regiment; the 20th's own whole-battle loss was 127 (Stone Sentinels)" },
    ],
  },
  "us-19ma": {
    frontage_m: 43,
    keyframes: [
      { t: 0, x: 4448, z: 4768, facing: 262, formation: "line", strength: 231, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 (Union regiments drawn individually; the 19th MA in reserve behind Hall's line per Waitt 1906; position class, time inferred); strength 231 brought to the field: Stone Sentinels 19th Massachusetts page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4448, z: 4768, facing: 262, formation: "line", strength: 220, confidence: "inferred", citation: "prostrate under the bombardment — Rorty's battery mauled in its front (Waitt 1906 pp.234-237)" },
      { t: 8700, x: 4422, z: 4795, facing: 262, formation: "line", strength: 195, confidence: "inferred", citation: "the plunge: Devereux asks Hancock's leave and throws the 19th MA and 42nd NY into the gap at the Copse (Devereux's OR report, OR 27/1; Waitt 1906 ch. XXIX); melee-zone position — widest uncertainty (survey disagreement 9); clock ~15:25-15:30 inferred" },
      { t: 9000, x: 4415, z: 4798, facing: 262, formation: "line", strength: 180, confidence: "inferred", citation: "hand-to-hand at the clump until the penetration collapses (Waitt 1906)" },
      { t: 10800, x: 4448, z: 4768, facing: 262, formation: "line", strength: 175, confidence: "inferred", citation: "end: brigade losses 365 not split by regiment" },
    ],
  },
  "us-42ny": {
    frontage_m: 37,
    keyframes: [
      { t: 0, x: 4446, z: 4745, facing: 262, formation: "line", strength: 197, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 (Union regiments drawn individually; the 42nd NY in reserve with the 19th MA behind Hall's line per Waitt 1906; position class, time inferred); strength 197 brought to the field: Stone Sentinels 42nd New York page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
      { t: 7200, x: 4446, z: 4745, facing: 262, formation: "line", strength: 188, confidence: "inferred", citation: "prostrate under the bombardment (Waitt 1906 pp.234-237)" },
      { t: 8700, x: 4428, z: 4790, facing: 262, formation: "line", strength: 170, confidence: "inferred", citation: "plunges into the gap at the Copse beside the 19th MA on Devereux's initiative with Hancock's verbal order (Devereux's OR report, OR 27/1; Waitt 1906 ch. XXIX); melee-zone position — widest uncertainty (survey disagreement 9); clock inferred" },
      { t: 9000, x: 4420, z: 4792, facing: 262, formation: "line", strength: 160, confidence: "inferred", citation: "in the melee at the clump until the penetration collapses (Waitt 1906; Haskell 1908)" },
      { t: 10800, x: 4446, z: 4745, facing: 262, formation: "line", strength: 155, confidence: "inferred", citation: "end: brigade losses 365 not split by regiment" },
    ],
  },
};

for (const [id, edit] of Object.entries(edits)) {
  const unit = battle.units.find((u) => u.id === id);
  if (!unit) throw new Error(`edit target '${id}' missing — decompose output changed?`);
  if (edit.frontage_m !== undefined) unit.frontage_m = edit.frontage_m;
  if (edit.depth_m !== undefined) unit.depth_m = edit.depth_m;
  unit.keyframes = edit.keyframes;
}

// ---- the two missing units --------------------------------------------------

const eighthOhio: Unit = {
  id: "us-8oh",
  name: "8th Ohio Infantry",
  side: "union",
  frontage_m: 120,
  depth_m: 20,
  // no parent: Carroll's brigade isn't modeled — a parentless tracked regiment
  keyframes: [
    { t: 0, x: 4270, z: 5310, facing: 265, formation: "skirmish", strength: 209, confidence: "documented", citation: "on the advanced picket line in the sunken Emmitsburg road cut, ~200 yds in front of Hays's main line, held from July 2 (Sawyer 1881, A Military History of the 8th Reg't Ohio Vol. Inf'y, full text verified; ABT 'The Picket Line Against Pickett's Charge': 209 men); Bachelder Third Day sheet, LOC 99447492 ('8'Ohio' bar); Sawyer clocks the cannonade opening 'between twelve and one o'clock' and lasting 'nearly two hours' — against the 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864) anchors, disagreement kept, not reconciled; road-cut coordinate inferred from the road trace northwest of the Bryan farm" },
    { t: 7800, x: 4235, z: 5425, facing: 185, formation: "line", strength: 195, confidence: "documented", citation: "'We changed our front, and taking position by a fence, facing the left flank of the advancing column of rebels, the men were ordered to fire into their flank at will' (Sawyer 1881; Sawyer's report, OR 27/1:461-62) — the flank fire under which Brockenbrough's command broke; fence coordinate north of the column's path and time ~15:10 inferred" },
    { t: 8700, x: 4310, z: 5360, facing: 180, formation: "line", strength: 170, confidence: "documented", citation: "advanced and cut off three regiments of the column's left, taking the colors of the 34th North Carolina (Lowrance's) and the 38th Virginia (Armistead's) (Sawyer 1881; OR 27/1:461-62); time ~15:25 inferred; strength decline reconstructed — no July-3-only casualty split located" },
  ],
};

const brownsBattery: Unit = {
  id: "us-btty-brown",
  name: "Brown's Battery B, 1st Rhode Island Light Artillery",
  side: "union",
  frontage_m: 80,
  depth_m: 40,
  keyframes: [
    { t: 0, x: 4445, z: 4785, facing: 262, formation: "line", strength: 103, confidence: "documented", citation: "Bachelder Third Day sheet, LOC 99447492 ('BROWN' named just south of the Copse of Trees; position class, time inferred); 'On July 3rd the four serviceable guns of Brown's Battery were placed just south of the Copse of Trees under the command of Lieutenant Perrin' — Capt. Brown wounded July 2 (Stone Sentinels Rhode Island Battery B page); strength 103 brought to the field, six 12-pdr Napoleons; Lt. Milne detached to Cushing's battery and killed there (Brown 1985 Filson); bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
    { t: 5400, x: 4445, z: 4785, facing: 262, formation: "line", strength: 92, confidence: "inferred", citation: "wrecked by the converging bombardment, ammunition nearly gone (Brown 1985 Filson; Stone Sentinels: Hunt found the battery severely damaged); losses 7 killed / 19 wounded / 2 missing are whole-battle figures (Stone Sentinels) — decline reconstructed" },
    { t: 6000, x: 4530, z: 4795, facing: 262, formation: "column", strength: 90, confidence: "documented", citation: "withdrawn mid-bombardment on Hunt's order, 'an order it obeyed promptly' (Stone Sentinels Rhode Island Battery B page; Brown 1985 Filson) — the withdrawal E. P. Alexander keyed Pickett's advance on (Alexander 1907); time ~14:40 and route inferred" },
    { t: 6600, x: 4610, z: 4800, facing: 262, formation: "line", strength: 90, confidence: "inferred", citation: "parked in reserve behind the crest (reconstruction)" },
    { t: 10800, x: 4610, z: 4800, facing: 262, formation: "line", strength: 88, confidence: "inferred", citation: "end: 103 minus 28 whole-battle casualties (Stone Sentinels Rhode Island Battery B page); July-3-only split not documented" },
  ],
};

// 8th Ohio joins Hays's sector (after Sherrill); Brown's B goes beside Cowan.
const insertAfter = (afterId: string, unit: Unit) => {
  const i = battle.units.findIndex((u) => u.id === afterId);
  if (i < 0) throw new Error(`insert anchor '${afterId}' missing`);
  battle.units.splice(i + 1, 0, unit);
};
insertAfter("us-sherrill", eighthOhio);
insertAfter("us-btty-cowan", brownsBattery);

// ---- fix us-btty-cowan: the documented gallop-in replacing Brown ------------
// It currently starts in place at kf 0; per Brown 1985 Cowan was brought up
// from the south at Webb's wave, into the slot Brown's Battery B vacated.
const cowan = battle.units.find((u) => u.id === "us-btty-cowan")!;
cowan.keyframes = [
  { t: 0, x: 4452, z: 4620, facing: 262, formation: "line", strength: 113, confidence: "inferred", citation: "July 3 start position south of its later post (Brown 1985 Filson: Cowan's battery was brought up from the south at Webb's wave; exact start coordinate inferred); strength 113: Stone Sentinels 1st NY Independent Battery page; bombardment opens 1:00 PM (Alexander 1907) / 1:07 PM (Jacobs 1864)" },
  { t: 6240, x: 4452, z: 4620, facing: 262, formation: "column", strength: 111, confidence: "inferred", citation: "limbers to move as Brown's wrecked Battery B pulls out of the line mid-bombardment (Brown 1985 Filson); clock ~14:44 inferred" },
  { t: 6300, x: 4450, z: 4770, facing: 262, formation: "line", strength: 110, confidence: "documented", citation: "the gallop-in: Cowan brings the battery up at a gallop into the position Brown's Battery B vacated just south of the Copse, waved in by Webb (Kent Masterson Brown, 'Double Canister at Ten Yards', Filson Club History Quarterly 59:3, 1985); Bachelder Third Day sheet, LOC 99447492 names 'COWAN' here — the map records this final position (position class; time ~14:45 inferred)" },
  ...cowan.keyframes.slice(1), // existing t=7200 / t=8700 / t=10800 keyframes unchanged
];

// ---- validate + canonical export --------------------------------------------

const result = validateBattle(battle);
if (!result.ok) {
  console.error(result.errors.join("\n"));
  throw new Error("authored battle failed validation");
}
console.log("warnings (advisory, expected: Webb short of the 106th PA; Hall regimental sums exceed the contested brigade table figure):");
for (const w of result.warnings) console.log("  " + w);
writeFileSync(battlePath, exportBattle(battle) + "\n");
console.log(`wrote ${battlePath}: ${battle.units.length} units`);
