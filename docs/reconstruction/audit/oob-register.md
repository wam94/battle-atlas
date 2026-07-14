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

**Note (pass 12, honesty flag):** this counts table was last reconciled at
pass 8 and has not tracked the I Corps/XI Corps command rows added at pass
10 (`reg-us-i-hq`, `reg-us-xi-hq`) or `reg-us-vi-hq` added this pass (the
VI Corps cheap batch, §"Pass 12" below) — the machine-readable
`oob-register.json`'s own `stats` block is authoritative (**288 total
entries** as of this pass; union command entries 13) and this table is
flagged stale rather than silently corrected without a full pass-by-pass
reconciliation (a pass-13 register-maintenance item).

## July 3 morning slice addition

- **`reg-us-xii-arty`** castStatus updated `not-yet-cast` -> `us-xii-arty`
  (the Muhlenberg XII Corps Artillery Brigade, newly cast this slice —
  `docs/reconstruction/audit/july3-morning-slice.md`; dossier
  `reconstruction/dossiers/us-xii-arty-muhlenberg.md`). No other register
  rows change: the 2nd Massachusetts and 27th Indiana are promoted as
  parentless siblings inside `us-colgrove`'s existing entry (ED-76 —
  same convention as `us-8oh`/`us-6wi`/`us-147ny`/`us-16me`, no separate
  register row for a minority-regiment promotion).
- `stats.inBuild` 162 -> **163**, `stats.notYetCast` 126 -> **125**.

## Pass 12 addition

- **`reg-us-vi-hq`** (VI Corps headquarters, corps command) — added for
  the VI Corps cheap batch (`docs/reconstruction/audit/dossier-pass-12.md`
  §3); dossier: `reconstruction/dossiers/us-vi-hq-sedgwick.md`.
- **Battery register triage** (the ~136-battery tail): every battery
  entry in `oob-register.json` gained a `triage` object (disposition +
  reason + `coveredBy`) this pass — see
  `docs/reconstruction/audit/dossier-pass-12.md` §4 for the method and
  summary counts.

Regiments enumerated: 478. Cast status: **157 → 161 entries matched to
in-build unit ids** across the battle phase files (`gettysburg-july3.json` +
the day-expansion-2 July 2 phase files, which cast six previously
uncast batteries: Clark, Bucklyn, Winslow, Smith's 4th NY, Watson,
Bigelow, + the day-expansion-3 July 1 phase files, which cast Gamble's
and Devin's cavalry brigades and Calef's battery A/2nd US, + decomposition
wave 1, which cast the Ziegler's Grove convergence: Williston's D/2US,
Butler's G/2US, Leonard Martin's F/5US, Harn's 3rd NY —
`docs/reconstruction/audit/decomposition-wave-1.md`);
**not-yet-cast reduced by the same four**
(the eight command rows flagged by dossier passes 3-4 — Pickett, Heth,
Pender/Trimble, Gibbon, Hancock, Hays, Doubleday, Second Corps artillery —
were added as an explicit register-maintenance batch at pass-5 start,
unblocking command-record overlay entries for the HQ dossiers). A direct
scan of `oob-register.json`'s 288 entries after decomposition wave 1 gives
162 castStatus-matched / 126 not-yet-cast — the `stats` block (previously
stale at 148/134, now corrected to match) is the authoritative machine
count; the small residual gap against this paragraph's hand-tracked 161/124
predates this wave and is a bookkeeping drift, not an evidence gap.
The build's 45 child-regiment units (Pickett/Pettigrew regiments, Webb/Hall/
Stannard regiments, Brockenbrough's wings, us-8oh, + decomposition wave 1's
us-6wi/us-147ny/us-16me, all PARENTLESS per the us-8oh precedent — see the
wave report) are represented inside their brigade entries' regiment lists
or citations, not as separate register entries — so all in-build units are
covered by the register at or inside its grain.

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
