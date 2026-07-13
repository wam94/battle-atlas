// Decomposition wave 1 — the Ziegler's Grove convergence (authoring-wave-a2.md
// §2 cut 1, the wave's headline cut: "the wave's no-new-units constraint...
// leaves nothing honestly authorable this wave. First pickup for the
// battery/regiment decomposition wave: author the four as units, in-window
// legs triggered at the repulse (t≈9000+), muzzles cold, with the Williston
// conflict carried."). Four VI Corps Artillery Brigade batteries (pass 15,
// reconstruction/dossiers/us-btty-williston.md / -butler.md / -harn.md /
// -martin-5us.md; parent command reconstruction/dossiers/us-vi-arty-
// tompkins.md EC3.3) independently tablet-attest moving to reinforce Hays's
// Division's rear on the repulse of Longstreet's Assault — a post-crisis
// consolidation, not a pre-charge deployment. NONE of the four have an
// attested fire narrative (three explicitly "no fire narrative located";
// Williston explicitly "Not engaged") — no fire events are authored for any
// of them; muzzles stay cold, per the wave brief.
//
// FILM SAFETY: this script ONLY appends new parentless units after
// us-btty-mccartney (itself already cast, NOT one of the 13 Angle-cast
// units). It does not read or modify any existing unit's keyframes. The
// four new units' first keyframe (t=0, the Weickert-farm reserve ground) and
// their departure (t=9000, the repulse) both fall OUTSIDE the film viewpoint
// window (t=8160..8820) and involve no fire — see the wave report for the
// per-second verification against the 13 Angle-cast units.
//
// Run from tool/:
//   npx vite-node scripts/author-decomp1-zieglers-grove.ts
// Wave report: docs/reconstruction/audit/decomposition-wave-1.md
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import type { Battle, Keyframe, Unit } from "../src/model";
import { addUnitsAfter, exportValidated } from "./fullcast-lib";

const here = dirname(fileURLToPath(import.meta.url));
const battlePath = join(here, "../../app/Assets/Battle/gettysburg-july3.json");
let battle: Battle = JSON.parse(readFileSync(battlePath, "utf8"));

const kf = (t: number, x: number, z: number, facing: number,
  formation: Keyframe["formation"], strength: number,
  citation: string): Keyframe =>
  ({ t, x, z, facing, formation, strength, confidence: "inferred", citation });

// The reserve ground: "South of Gettysburg, east side of Sedgwick Avenue,
// near the George Weickert farm" (brigade tablet verbatim [E],
// reconstruction/dossiers/us-vi-arty-tompkins.md EC3.2) — the SAME ground
// already cited on us-btty-mccartney's own t=0 keyframe (4210,3070, radius
// "honest at ~±150 m"); each battery gets a small distinct offset within
// that same radius rather than a literal duplicate coordinate.
const williston: Unit = {
  id: "us-btty-williston", name: "Williston's Battery D, 2nd US Artillery",
  side: "union", frontage_m: 80, depth_m: 40,
  keyframes: [
    kf(0, 4195, 3055, 270, "column",
      109,
      "Waiting with the VI Corps Artillery Brigade reserve: 'South of Gettysburg, east side of Sedgwick Avenue, near the George Weickert farm' (brigade tablet verbatim [E], reconstruction/dossiers/us-vi-arty-tompkins.md EC3.2; radius honest at ~±150 m, no battery-grain morning position exists). Strength 109 (own tablet, verbatim, reconstruction/dossiers/us-btty-williston.md EC2). GUN-COUNT CONFLICT CARRIED, not adjudicated: this battery's own tablet states 'four light 12 pounders'; the brigade-level tablet and a separate cross-check both state six guns for this slot (dossier Conflicts item 1) — neither reading adopted. 1st Lt. Edward B. Williston commanding, brevetted major July 3 'for gallantry and meritorious services in the Gettysburg campaign' (tablet verbatim)."),
    kf(9000, 4195, 3055, 270, "column",
      109,
      "Still parked at the reserve ground at the moment of the repulse (no departure clock stated in any source for this battery)."),
    kf(10500, 4600, 5170, 252, "line",
      109,
      "POSITION CONFLICT CARRIED, not silently resolved: per the companion Battery G (Butler's) page, this battery's PAIR (D and G, both 2nd U.S.) were 'brought up to Ziegler's Grove in rear of Third Division Second Corps on repulse of Longstreet's Assault' (reconstruction/dossiers/us-btty-williston.md EC3.2, cross-referenced via us-btty-butler.md) — adopted here as the authored leg (trigger = the repulse, matching Butler's and Martin's own independently-attested moves to the SAME ground). Williston's OWN individual tablet page, however, states only 'placed in reserve' and 'Not engaged' on Taneytown Road, WITHOUT independently repeating the Ziegler's Grove move — an unresolved internal cross-source tension (dossier Conflicts item 2), carried whole per this corpus's standing doctrine (both readings recorded, neither averaged; candidate reading: Taneytown Road the approach, Ziegler's Grove the final position — NOT ruled). Position placed rear of Woodruff's own gun line (mon-woodruff 4502.6,5190.6), radius wide-open; frontage/depth per Woodruff's own battery-scale convention (80x40). NO FIRE: 'Not engaged' (tablet, verbatim) — no event authored, muzzles cold. NO CASUALTIES (tablet, verbatim)."),
    kf(23340, 4600, 5170, 252, "line",
      109,
      "Holds the Ziegler's Grove reinforcement position to sunset (no further movement or activity documented)."),
  ],
};

const butler: Unit = {
  id: "us-btty-butler", name: "Butler's Battery G, 2nd US Artillery",
  side: "union", frontage_m: 80, depth_m: 40,
  keyframes: [
    kf(0, 4225, 3055, 270, "column",
      113,
      "Waiting with the VI Corps Artillery Brigade reserve, arrived 'in the afternoon with the Corps and held in reserve' July 2 (brigade tablet verbatim, reconstruction/dossiers/us-btty-butler.md EC3.1); reserve ground per us-vi-arty-tompkins.md EC3.2 (Sedgwick Avenue, near the Weickert farm), placed beside Williston's. Strength 113 (own tablet, verbatim); six 12-pounders (tablet, verbatim — a brigade-tablet gun-listing variance for this slot noted, not adjudicated). 1st Lt. John H. Butler commanding."),
    kf(9000, 4225, 3055, 270, "column",
      113,
      "Still at the reserve ground at the moment of the repulse."),
    kf(10500, 4630, 5170, 252, "line",
      113,
      "THE ZIEGLER'S GROVE CONVERGENCE, this battery's own tablet — the clearest single statement of this wave's authored fact: 'brought up to Ziegler's Grove in rear of Third Division Second Corps on repulse of Longstreet's Assault' (tablet verbatim, reconstruction/dossiers/us-btty-butler.md EC3.2) — trigger = the repulse itself (a post-crisis reinforcement move, not a pre-charge deployment); no distance or exact clock stated, arrival inferred at a comparable pace to McCartney's own documented leg (this file, us-btty-mccartney). Position rear of Woodruff's gun line (mon-woodruff 4502.6,5190.6), radius wide-open. NO FIRE: 'no fire narrative located this pass' — the battery's own page frames its role as reserve/reinforcement, matching the tablet's zero-casualty line; no event authored, muzzles cold. NO CASUALTIES (tablet, verbatim)."),
    kf(23340, 4630, 5170, 252, "line",
      113,
      "Holds the Ziegler's Grove reinforcement position to sunset."),
  ],
};

const martin5us: Unit = {
  id: "us-btty-martin-5us", name: "(Leonard) Martin's Battery F, 5th US Artillery",
  side: "union", frontage_m: 80, depth_m: 40,
  keyframes: [
    kf(0, 4195, 3095, 270, "column",
      125,
      "Waiting with the VI Corps Artillery Brigade reserve, arrived 'in the afternoon with the Corps and held in reserve' July 2 (brigade tablet verbatim, reconstruction/dossiers/us-btty-martin-5us.md EC3.1); reserve ground per us-vi-arty-tompkins.md EC3.2. Strength 3 officers + 122 enlisted = 125 (tablet, verbatim); six 10-pounder Parrotts (tablet, verbatim). 1st Lt. Leonard Martin commanding — DISTINCT from Lt. David H. Kinzie's separate 'Battery F, 5th U.S.' (XII Corps Artillery Brigade); the name-collision independently resolved, both units confirmed distinct (dossier EC1)."),
    kf(9000, 4195, 3095, 270, "column",
      125,
      "Still at the reserve ground at the moment of the repulse."),
    kf(10500, 4600, 5210, 252, "line",
      125,
      "THE ZIEGLER'S GROVE CONVERGENCE, third of the three regular-battery statements: 'brought up to Ziegler's Grove in rear of Third Division Second Corps on the repulse of Longstreet's assault' (tablet verbatim, reconstruction/dossiers/us-btty-martin-5us.md EC3.2) — trigger = the repulse; no distance or clock stated. Position rear of Woodruff's gun line (mon-woodruff 4502.6,5190.6), radius wide-open; the monument itself sits ON Ziegler's Grove ground (east side of Hancock Avenue, ~225 ft east of the avenue, at the former Cyclorama building site — dossier EC3.3 [E], a rare case where the memorial siting matches the attested battle-day ground). NO FIRE: 'no fire narrative located this pass' — reserve/reinforcement framing, matching Williston's and Butler's own pattern; no event authored, muzzles cold. NO CASUALTIES (tablet, verbatim)."),
    kf(23340, 4600, 5210, 252, "line",
      125,
      "Holds the Ziegler's Grove reinforcement position to sunset."),
  ],
};

const harn: Unit = {
  id: "us-btty-harn", name: "Harn's 3rd New York Independent Battery",
  side: "union", frontage_m: 80, depth_m: 40,
  keyframes: [
    kf(0, 4225, 3095, 270, "column",
      119,
      "Waiting with the VI Corps Artillery Brigade reserve after a '36 mile forced march from Manchester Maryland' arriving July 2 afternoon (tablet verbatim, reconstruction/dossiers/us-btty-harn.md EC3.2); reserve ground per us-vi-arty-tompkins.md EC3.2. Strength 119 (tablet, verbatim); six 10-pounder Parrott rifles (tablet, verbatim). Capt. William A. Harn commanding (own report No. 240, OR 27/1 pp. 691-693 — a march-itinerary document, no July 2-3 action narrative)."),
    kf(9000, 4225, 3095, 270, "column",
      119,
      "Still at the reserve ground at the moment of the repulse."),
    kf(10500, 4630, 5210, 252, "line",
      119,
      "THE FOURTH ZIEGLER'S-GROVE-CROSS-REFERENCE WITNESS, a different (New York, not regular) battery: 'ordered to support Hays's Division of the Second Corps during Pickett's Charge' (tablet verbatim, reconstruction/dossiers/us-btty-harn.md EC3.3) — trigger inferred as the same repulse-window reinforcement Williston's, Butler's, and Martin's own tablets independently attest, per the parent dossier's four-battery grouping (us-vi-arty-tompkins.md EC3.3/EC5.4); no distance or clock stated. Position rear of Woodruff's gun line, radius wide-open. NO FIRE: 'no engagement narrative located this pass' beyond the 'ordered to support' fact — a reserve/support record, own-report and tablet consistent; no event authored, muzzles cold. NO CASUALTIES (tablet, verbatim)."),
    kf(23340, 4630, 5210, 252, "line",
      119,
      "Holds the Hays-front support position to sunset."),
  ],
};

battle = addUnitsAfter(battle, "us-btty-mccartney",
  [williston, butler, martin5us, harn]);

writeFileSync(battlePath, exportValidated(battle));
console.log(`wrote ${battlePath}: ${battle.units.length} units, ${battle.events!.length} events`);
