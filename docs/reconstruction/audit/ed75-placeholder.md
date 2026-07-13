# ED-75 — Marshall's and Davis's brigades: placeholder July-3 strengths

**Date:** 2026-07-13 · **Branch:** ed75-placeholder (unmerged) · **Status:
COMPLETE, PROVISIONAL.** Owner ruling recorded in
`docs/reconstruction/angle-editorial-decisions.md` ED-75.

Day-expansion slice 1 (ED-46/47/48) re-based every CSA assault-column
brigade with a primary July-3 basis and left Marshall's and Davis's
brigades — the two with none — annotated in-file as an open residual.
The owner ruled 2026-07-13: yes to a documented-inference placeholder in
the meantime, pending purchase of Busey & Martin, *Regimental Strengths
and Losses at Gettysburg* (the page-level per-brigade source that
supersedes on arrival). This report is the subtraction inputs, the chosen
values, and the envelope.

## 1. Subtraction inputs (quoted, hop tiers stated, disagreements verbatim)

**Arrival/base strength:**

- West Confederate Ave tablet (via stone-sentinels, inferred/compilation
  tier): Marshall's — "Present on the first day about 2000. Killed 190
  Wounded 915 Missing about 300 Total 1405." Davis's — "Present on the
  first day about 2000 Killed 180 Wounded 717 Missing about 500 Total
  1397," plus "Joined later by the 11th Regiment previously on duty
  guarding trains the Brigade fought until the day's contest ended" —
  read as the tablet's "2,000" covering the three July-1-engaged
  regiments only, the 11th Mississippi excluded from that count.
- Disagreement, Marshall's: Wikipedia / military-history.fandom, uncited:
  "roughly 2,500" and "over 40 percent" July-1 losses for all four
  regiments (a secondary synthesis, attributed-only tier, NOT
  independently verified against a named historian's page).
- Disagreement, Davis's three engaged regiments: Emerging Civil War
  (Kristopher D. White, no footnote, no bibliography — attributed-only
  tier): "Carrying some 1,508 men into battle on July 1st... The 2nd and
  11th Mississippi were the only veteran units in the formation (the 11th
  Mississippi was on detached duty on the morning of July 1st and would
  have brought the brigade's numbers up to 2,305)."
- The 11th Mississippi's own July-3 strength: TWO independent GBMA/War
  Department markers (fetched via stonesentinels.com) agree digit-exact:
  main brigade tablet "Combatants – 393 / Killed in action/died of wounds
  – 110 / Wounded/wounded captured – 193 / Captured unwounded – 37 /
  Non-casualty – 53" (scope: "Afternoon July 2 – July 4, 1863"); Bryan
  Barn position marker "Combatants – 393 / Killed in action/died of
  wounds – 100 / Wounded/wounded captured – 193 / Captured unwounded – 37
  / Non-casualties – 39" (scope: "July 3, 1863"). The two markers disagree
  on the internal killed/non-casualty split (110 vs 100; 53 vs 39) but
  agree exactly on the combatant total — carried as a corroborated
  reading (ED-39), not averaged.
- Disagreement, the 11th Mississippi's OWN figure elsewhere: a web search
  result attributed to Busey & Martin reports "the 11th Mississippi
  entering the battle with 592 men and losing 102 killed, 168 wounded,
  and 42 missing or captured" — **this is an UNVERIFIED third-party
  citation of the book, not a page read**, i.e. exactly the class of
  number this whole placeholder exists to be superseded by. It is carried
  as the high pole of the envelope, explicitly flagged as a hop, never
  adopted as the working value.

**July-1 loss inputs (both brigades' printed returns are whole-battle,
regiment-grain k+w only, per ED-49):**

- Marshall's/Pettigrew's: "190 k / 915 w / — = 1,105; 11th NC 209, 26th NC
  588 (86 k / 502 w), 47th NC 161, 52d NC 147" (or-27-2-anv-return p. 344
  — **no missing column printed at all**, the ED-49 exemplar). Day-split
  anchor: the 26th NC's own primary, "We went in with over 800 men in the
  regiment. There came out but 216, all told, unhurt" (July 1); "now have
  only about 80 men for duty" (July 4) (or-27-2-young-26nc) — the
  regiment's dominant loss share is July 1 (588 whole-battle, ~584 read as
  July 1 per the dossier's own [B] subtraction).
- Davis's: "180 k / 717 w / — = 897; 2d Miss 232, 11th Miss 202, 42d Miss
  265, 55th NC 198" (or-27-2-anv-return p. 344), with the dossier's own
  day-split anchor that "the 11th Miss's 202 k+w are July 3 ONLY (train
  guard July 1)" — leaving 695 k+w for the three July-1-engaged regiments,
  whole-battle. The railroad-cut disaster is the brigade's dominant loss
  event: "about three hundred Confederates surrendered" (Wikipedia /
  Emerging Civil War, corroborating Davis's own report, "rallied near the
  railroad, where he again made a stand, and, after desperate fighting,
  with heavy loss on both sides" then "This was about 1 p.m." withdrawal,
  or-27-2-davis-jr) — the basis for reading the printed total as
  predominantly July-1.
- The one PRIMARY delta either brigade carries: "In Davis' brigade 2 men
  were killed and 21 wounded" during the two-hour cannonade
  (or-27-2-davis-jr) — a real headcount, applied absolute, not
  proportional, to whichever base is adopted.

No page-level Busey & Martin figures for either brigade were found —
confirming the gap this placeholder fills. (Searches attempted: direct
B&M brigade/regiment strength queries, David Martin's *Gettysburg July 1*
for a Davis brigade table, Pfanz's *Gettysburg — The First Day* for a
Davis strength figure. None surfaced a page-citable number beyond the one
unverified 11th Mississippi web citation above.)

## 2. Method, chosen values, envelopes

**Method (one class of inference, both brigades):** arrival strength =
(July-1-engaged regiments' post-July-1 survivors) + (any regiment fresh to
July 3). Where a regiment's own July-1 losses have no primary total, the
whole-battle return is read as predominantly July-1-incurred, because each
brigade's dossier documents a single defining July-1 casualty event
(26th NC vs the Iron Brigade; the railroad-cut trap) that plausibly
accounts for most of the printed total.

| Brigade | Central value | Envelope | Basis |
|---|---|---|---|
| csa-marshall | **900** | 750–1,050 | 2,000 (tablet) − ~1,100 (July-1-read whole-battle return, no missing column) — the dossier's own EC2 figure, promoted to the keyframe |
| csa-davis | **1,293** | 1,100–1,550 | 900 (three engaged regiments, mirrors Marshall's method) + 393 (11th Mississippi's own tablet-corroborated July-3 strength) |

Confidence: **INFERRED** on every touched keyframe (never `documented` —
neither brigade has a primary July-3 total). Decay curves: every keyframe
after t=0 (t=7200 for Davis, where the attested −23 anchors it) is
**proportionally rescaled** from the existing ABT-map-reconstruction decay
shape — the charge/repulse/withdrawal geometry is not independently
re-derived, only the strength magnitude moves. Children re-split
`round(parent/4)` at each child's own keyframe times (the standing
even-split convention; per-regiment splits remain a general
reconstruction-grade limitation, not something this ED resolves).

Precondition: superseded on arrival by Busey & Martin page-level figures
(owner has stated purchase intent) — the Pfanz-gate provisional pattern
(ED-64). Recorded PROVISIONAL-ADOPTED, not ADOPTED.

## 3. Bundle-untouched proof

Neither `csa-marshall` nor `csa-davis` is in the compiled Angle cast
(`CAST` in `tool/scripts/author-ed75-placeholder.ts`, asserted by a guard
that throws if either ever appears in it). `reconstruction/scripts/
compile_angle.py` was re-run after the battle-file edit and the two
bundles diffed key-for-key:

- `units` array: **byte-identical**.
- `stagingSeed`: `d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1`
  — held verbatim.
- Only `checksum` and `inputs.battle` (the whole battle file's own hash)
  changed — the same metadata-only precedent ED-46's slice-1 recompile
  established.

## 4. Suites

| Suite | Baseline | This change |
|---|---|---|
| tool vitest | 118 | **119 passed** (+1 dedicated ED-75 test; 2 slice-1 assertions updated for the superseded state) |
| reconstruction pytest | 122 + 1 skip | **122 passed, 1 skipped** (unchanged) |
| pipeline pytest | 59 | **59 passed** (unchanged) |
| Unity EditMode | 375 + 4 skips (day-expansion slice 1 baseline) | **375 passed, 0 failed, 4 skipped** (unchanged — the 4 skips are the HDRP-bake AngleEnvironmentTests, expected on this rig) |
| Unity PlayMode | 17 (day-expansion slice 1 baseline) | **17 passed, 0 failed** (unchanged) |

(Unity runs: CLI `-batchmode -runTests -buildTarget OSXUniversal` — no
`-quit`, which cuts the run short before the test runner starts — worktree
Library rsync'd from the main checkout, gitignored inputs restored
(`data/heightmap`, `data/landcover`, `app/Assets/Generated`,
`app/Assets/StreamingAssets/SoldierView`); this branch was cut before
day-expansion-2 merged to main, so its Unity baseline is the
day-expansion-slice-1 numbers, not main's current post-merge count.)

## 5. What Busey & Martin will settle

Beyond these two brigades, B&M page-level figures are the standing
precondition or open item on several other places in the corpus that this
placeholder does not touch:

- **csa-brockenbrough** (ED-48a): the 800–1,100 Mayo-hop range is carried,
  "no promotion to a clean number" — B&M would close it.
- **csa-fry** (ED-46/pass-3): confirmed primary but explicitly
  July-1-scoped, "subtraction-only, carried as the same heterogeneity
  class as Marshall/Davis" — B&M's July-3-specific figure (if the book
  carries one) would close the same class of gap this ED addresses.
- **csa-bn-eshleman / csa-bn-alexander** (ED-38): "adopted from the B&M-type
  reproduction... flagged B&M-type/hop... until a primary return
  surfaces" — the real book supersedes the reproduction directly.
- The 11th Mississippi's own strength (this ED, Term 2): the 393 vs 592
  disagreement is exactly a B&M-page-vs-secondary-citation question.
- More broadly: every OOB-register row still flagged `attributed-only` or
  citing a Stone Sentinels compilation instead of a primary (the
  systemic caveat in `docs/research/2026-06-13-oob-strengths.md` §1) is a
  candidate for a B&M page check — the book is the one source class that
  can turn compiled "present" figures into engaged-strength figures with
  a stated method, army-wide, rather than brigade-by-brigade inference.
