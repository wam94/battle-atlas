# OOB register — summary (companion to oob-register.json)

**Status: ADOPTED (owner ruling 2026-07-10, adopt-and-adjust; rulings
ED-24…ED-31 in `docs/reconstruction/angle-editorial-decisions.md`).**
The machine-readable
register is `docs/reconstruction/audit/oob-register.json`: every unit of both
armies at the audit's working grain (brigade + battery/battalion; regiments
enumerated name-only inside brigade entries), per the unit-truth-spec scope
ruling (full-battle research upfront, phased authoring).

## Counts

| | Union | Confederate |
|---|---|---|
| Infantry brigades | 51 | 37 |
| Cavalry brigades | 8 | 7 (incl. Imboden's command) |
| Batteries (first-class entries) | 67 | 69 (incl. McClanahan's with Imboden) |
| Artillery brigade/battalion parents | 14 | 16 (15 army battalions + Stuart horse artillery) |
| Command entries | 10 (Artillery Reserve park; Provost; Chief of Artillery [pass 1]; II Corps HQ, 2nd Div II Corps, 3rd Div II Corps, 3rd Div I Corps [pass 5]; 2nd Div III Corps [pass 7]; XII Corps HQ, 2nd Div XII Corps [pass 8]) | 6 (First Corps artillery command [pass 1]; Pickett's Div, Heth's Div, Pender's Div, Second Corps artillery command [pass 5]; Johnson's Div [pass 8]) |
| **Total register entries** | **150** | **135** (285 overall) |

Regiments enumerated: 478. Cast status: **148 entries matched to in-build
unit ids** in `app/Assets/Battle/gettysburg-july3.json`; **134 not-yet-cast**
(the eight command rows flagged by dossier passes 3-4 — Pickett, Heth,
Pender/Trimble, Gibbon, Hancock, Hays, Doubleday, Second Corps artillery —
were added as an explicit register-maintenance batch at pass-5 start,
unblocking command-record overlay entries for the HQ dossiers).
The build's 42 child-regiment units (Pickett/Pettigrew regiments, Webb/Hall/
Stannard regiments, Brockenbrough's wings, us-8oh) are represented inside
their brigade entries' regiment lists, not as separate register entries — so
all 190 build units are covered by the register at or inside its grain.

## Sources and method

- **Structure, commanders, successions:** the OR Series I Vol. 27 order-of-
  battle returns (`or-27-oob`) as reproduced by the OR-derived Wikipedia OOB
  articles (`wikipedia-oob-union`, `wikipedia-oob-csa`), whose wikitext was
  fetched 2026-07-10 and used as a verification corpus — every enumerated
  regiment was grep-checked against it (24 non-matches reviewed by hand; all
  were style differences, e.g. "US" vs "United States", "Phillips'" vs
  "Phillips"). One genuine compilation disagreement found and preserved:
  the Crenshaw battery's Gettysburg commander (Capt. W. G. Crenshaw per
  Wikipedia vs Lt. A. B. Johnston in some modern compilations).
- **Strengths:** ONLY where `docs/research/2026-06-13-oob-strengths.md` (or
  the 2026-07-02 full-cast survey) carries figures — 22 units. Disagreements
  preserved verbatim, never averaged, including: the Lane/Wilcox same-page
  extraction conflicts; Stannard's 1,950-vs-1,788 gap; Brockenbrough's
  corrupted Stone Sentinels figure (flagged UNUSABLE, the 800–1,100 range
  kept per-source); the Heth/Pender "present on the first day" scope caveat.
  Busey & Martin remains uncited at page level anywhere in the corpus
  (`busey-martin-2005` records the standing gap).
- **Activity notes / days present / arrivals:** the War Department tablets
  via the 2026-07-02 full-cast survey (documented negatives kept: Huey's
  brigade never on the field; 1st CT Heavy B & M present-not-engaged; Battery
  C 3rd US with Huey; the Engineer Brigade NOT at Gettysburg — do not model).
  Notable arrivals encoded: Pickett (evening July 2), VI Corps (~14:00
  July 2 after ~32 miles), Law (~noon July 2 after ~24–28 miles), Lockwood
  (midday July 2), Stannard (evening July 1), Merritt (July 3, arrival
  disagreement 11:00/13:00/15:00 kept), Stuart's brigades (July 2), Jones/
  Robertson/Imboden (July 3), Gamble/Devin withdrawn before July 3.
- **Off-map ruling** (full-cast survey) carried through: East Cavalry Field,
  Fairfield, Westminster, Cashtown units are register entries with explicit
  off-square notes, not silent omissions.

## What this register is for

The master table (`unit-master-table.xlsx`) now includes not-yet-cast register
entries as rows with a Cast status column (see
`reconstruction/scripts/build_unit_audit.py`); computed columns are blank for
rows with no build data. Dossier research (EC1–EC6) proceeds against register
entries; authoring stays phased per the owner's scope ruling.

## Known limits of this pass

1. Regiment lists are name+number only (per the task grain); company-level
   detachments (e.g. 1st Co. Massachusetts Sharpshooters, Purnell Legion
   Co. A is included as it is brigade-roster-level, 84th PA train guard noted
   in-line) are carried in notes, not enumerated.
2. Strength coverage is the 22 units the corpus has researched; the other
   ~249 entries await the OR-returns/B&M extraction pass (the standing
   authoring-prep item).
3. Union OOB structure rests on the Wikipedia reproduction of the OR
   pending script-friendly OR Part 1 access (standing open item).
4. Commander-succession annotations were carried from the same corpus; the
   k/w/mw/c annotations are OR-derived but not re-verified against each
   report at this pass.
