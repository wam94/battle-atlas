# Canonical anchor-chain proposal — all six battle phases

**Status: ADOPTED 2026-07-10 (owner ruling, adopt-and-adjust).** The chains
are canonical as ruled in `docs/reconstruction/angle-editorial-decisions.md`:
ED-24 (July 3 afternoon, with verification updates), ED-25 (clock profiles,
implemented on `reconstruction/sources/` + validator), ED-26…ED-30 (the five
skeleton chains, PROVISIONAL with preconditions carried), ED-31 (LMT frame).
**The step-off gate resolved: ED-1 STANDS** — the one-hop 14:00–14:30 center
did not survive page-level verification (Coddington p. 502 reads 3:00 P.M.;
Hess/Wert verified as the genuine short-cannonade school; Sears unreachable;
new OR evidence supports ~15:00). Full record:
`docs/reconstruction/audit/verification-2026-07-10.md`. Historical draft text
below is preserved as proposed; where it conflicts with the EDs, the EDs win. Implements the unit-truth-spec's master-clock
resolution: a per-phase canonical anchor chain plus a per-source clock-offset
assessment. July 3 afternoon is drafted in full (it must reconcile with the
shipped reconstruction); the five other phases are skeletons seeded from
targeted verification passes (2026-07-10) and the existing corpus.

Conventions used throughout:

- **Anchor ids** are `CA-<phase>-<n>` (e.g. `CA-J3A-6` = July 3 afternoon,
  anchor 6). Claims citing an anchor are timing tier A per the spec.
- **Adopted time** is on the canonical battle clock (local mean time at
  Gettysburg, see the frame ruling under proposed ED-31). Envelope =
  earliest/latest honest reading, not a confidence interval.
- **VERIFIED** = the quoted words were fetched from the cited page during this
  research pass (directly or by the verification subagents, 2026-07-10);
  **SNIPPET** = search-summary only, flagged unreliable; corpus citations
  (haskell-1908, alexander-1907, jacobs-1864 …) resolve in
  `reconstruction/sources/sources.json` and were verbatim-verified in the
  2026-06-13 source-library passes.
- Tablet quotes are the War Department / Gettysburg National Park Commission
  tablets (~1900–1912) via gettysburg.stonesentinels.com — official but
  ~35-year-retrospective adjudications; where tablet clocks skew against
  modern scholarship this is flagged per event (see the McLaws-wing skew rule,
  ED-28 note).
- **No invented times.** Every adopted time below traces to named sources;
  where only arithmetic from another anchor exists, the anchor is marked
  tier B (physics/arithmetic) and cites its inputs.

---

## 1. July 3 afternoon — bombardment and Pickett's Charge (FULL DRAFT)

This chain must reconcile with the SHIPPED reconstruction: slice clock
startTime 46800 s = 13:00 (t=0); ED-1 step-off ~15:05 (t=7500); ED-2 wall
crossing t=8700 ≈ 15:25; slice t=8040..9000 = 15:14–15:30. The chain below
keeps the shipped frame (per ED-1's rationale: never silently re-time the
battle) and *records* every tension with the literature instead of papering
over it — see §1.2.

### 1.1 The chain (proposed ED-24)

| id | Event | Adopted | Envelope | Tier basis |
|---|---|---|---|---|
| CA-J3A-0 | Longstreet–Pickett exchange; order to advance affirmed by bow | unclocked | sequence: after CA-J3A-3, before CA-J3A-6 | D |
| CA-J3A-1 | Cannonade signal guns (Miller's 3rd Co., Washington Artillery, Peach Orchard) | **13:07** | 13:00–13:07 | A (defines the chain) |
| CA-J3A-2 | Alexander's first note to Pickett ("still 18 guns firing from the cemetery") | 13:32 corrected (13:25 stated) | 13:20–13:40 | C via Alexander clock profile |
| CA-J3A-3 | Alexander's second note ("For God's sake come quick") | 13:47 corrected (13:40 stated) | 13:35–13:55 | C via Alexander clock profile |
| CA-J3A-4 | Union fire slackens (Hunt's conservation order; Osborn/Hunt cease-fire ruse) | ~14:30 | 14:00–15:00 | C/D |
| CA-J3A-5 | Cannonade ends / falls quiet on the Union crest | **15:00** | 14:00–15:07 | A-candidate (Haskell watch-checked) |
| CA-J3A-6 | Step-off of the assault column (= ED-1) | **15:05** (t=7500) | 13:50–15:10 | ruled (ED-1) |
| CA-J3A-7 | First line crosses the Emmitsburg Road; Garnett's halt-and-dress at the fences | 15:16–15:18 (t=8160–8280) | ±4 min on ED-1's frame | B (leg arithmetic from CA-J3A-6) |
| CA-J3A-8 | Stannard's change of front ("change front forward on first company") | ~15:20 (t=8400) | 15:18–15:24 | D→B (sequence pinned between CA-J3A-7 and CA-J3A-9) |
| CA-J3A-9 | Armistead crosses the wall; falls at the guns (= ED-2) | **15:25** (t=8700) | 15:24–15:26 (8640–8760); literature frame to ~16:00 | ruled (ED-2) |
| CA-J3A-10 | Repulse complete at the Angle; survivors recross and fall back; Wilcox/Lang's supporting advance smashed | 15:30 (t=9000) for the Angle; Wilcox/Lang 15:30–16:00 | 15:28–16:15 | B/D |

#### CA-J3A-1 — Signal guns, adopted 13:07

- **Sources and stated times:**
  - Jacobs (jacobs-1864, contemporaneous, clock-habituated civilian):
    "At seven minutes past 1 P.M., the awful and portentous silence was
    broken" — **13:07**, the only true clock *reading* (vs round-number
    recollection) in the set. Corpus-verified; re-verified against the
    archive.org full text 2026-07-10.
  - Alexander (alexander-1907): "It was just 1 P.M. by my watch when the
    signal guns were fired" — **13:00** (his watch; 44-year retrospective).
  - Haskell (haskell-1908): watch-checked "five minutes before one o'clock,"
    then "the distinct sharp sound of one of the enemy's guns" — **~13:00**.
  - Waitt (waitt-1906, 19th MA regimental): "Just at one o'clock the sharp
    report of a shotted gun within the enemy's lines, broke the oppressive
    stillness" — **13:00** (compiled 1906).
  - Rawley Martin (martin-smith-shsp32-1904, inside the column): "About
    1 o'clock two signal guns were fired by the Washington Artillery" —
    **~13:00**.
  - Longstreet's OR report (VERIFIED via the NPS reproduction,
    nps.gov/gett/learn/historyculture/official-report-of-lieut-general-james-longstreet.htm):
    "About 2 p.m. General Pickett … reported that the troops were in order …
    Colonel Walton was ordered to open the batteries. The signal guns were
    fired" — **~14:00**, a genuine primary-source outlier.
  - Modern adoption (VERIFIED at the page level 2026-07-10): Wikipedia
    "Pickett's Charge" "around 1 p.m."; American Battlefield Trust "near
    1 p.m."; the 13:07 figure is the standard hard pin in the modern
    literature because it is a clock reading.
- **Disagreement and proposed reading:** 13:00 (three round-number watches) vs
  13:07 (Jacobs's reading) vs ~14:00 (Longstreet's report). **Adopt 13:07** —
  a stated minutes-past reading from the corpus's most contemporaneous and
  habitually precise source beats round-number recollections; the three
  "one o'clock" statements sit inside a 7-minute envelope and become
  clock-offset data (§3). Longstreet's 14:00 is report-nominal (§3, example 4)
  and is excluded from the anchor by rule, kept as the recorded outlier.
- **Extends:** seeds the chain that ED-1/ED-2 already imply; the shipped-clock
  tension is §1.2(1). **Needs new ED-24.**

#### CA-J3A-2 / CA-J3A-3 — Alexander's two notes to Pickett

- Alexander (alexander-1907, corpus-verified quotes): first note timed
  **1:25 PM** ("still 18 guns firing from the cemetery"); second **1:40 PM**
  ("For God's sake come quick. The 18 guns have gone…"). Longstreet's memoir
  reproduces the 1:40 note with slightly different wording
  (longstreet-1896) — flagged in the corpus, don't reconcile.
- These are the only interior clocks of the bombardment. Both are Alexander's
  watch: corrected by his +7 min offset (§3 example 3) they become 13:32 and
  13:47 with the profile's envelope.
- **Disagreement:** none directly (no rival timings); reliability rides
  Alexander's retrospective-watch profile. **Proposed reading:** adopt as
  tier-C anchors (they anchor other bombardment claims but are themselves
  offset-corrected, not chain-defining). **Extends ED-24.**

#### CA-J3A-4 — Union fire slackens / the ruse

- Corpus (pass-1 timeline): Hunt's deliberate ammunition-conserving slackening
  (his Battles & Leaders account); Alexander "incorrectly told it was the
  cemetery"; the full-cast survey adds Osborn's deliberate cease-fire ruse on
  Cemetery Hill (padresteve.com Hunt essay; tablet-collective attestation).
  Haskell's mid-bombardment marker: "Half-past two o'clock, an hour and a
  half since the commencement" — a watch statement placing the bombardment
  still in full roar at 14:30 (corrected ~14:37 by his profile).
- Modern spread (VERIFIED at the Wikipedia-citation layer, pages not yet
  fetched): Sears has the cannonade ending ~14:30; Hess "essentially over by
  2 p.m."; Coddington ~15:00.
- **Proposed reading:** adopt ~14:30 as a soft anchor (envelope 14:00–15:00)
  for the *slackening*, distinct from CA-J3A-5's *end*. The slackening is what
  Alexander acted on; the two must stay separate events. **Extends ED-24.**
  Open item: fetch Hess/Sears pages to harden the envelope (they are
  in-copyright; times as attributed facts).

#### CA-J3A-5 — Cannonade ends, adopted 15:00

- Haskell (haskell-1908): cannonade ended "At three o'clock almost
  precisely" — the one watch-checked Union end-time in the corpus, and the
  time ED-1's rationale is anchored to. Corrected by Haskell's +7 profile it
  reads ~15:07; **adopt 15:00 stated** (the correction is inside the
  envelope; ED-1's arithmetic was built on the stated time).
- Counter-readings: Hess "essentially over by 2 p.m."; Sears ~14:30 (both via
  the VERIFIED Wikipedia citation layer); Smyth's OR report has the cannonade
  *starting* at 14:00 (corpus pass-1) — report-nominal.
- **Proposed reading:** keep Haskell (15:00), envelope 14:00–15:07, and record
  the modern-scholarship pull toward 14:00–14:30 as the chain's biggest open
  tension (§1.2(2)). **Extends ED-1; formalized in ED-24.**

#### CA-J3A-6 — Step-off (= ED-1, unchanged)

- Witness spread (corpus, all verbatim-verified): Alexander "doubtless 1.50 or
  later, but I did not look at my watch again" (self-hedged); Jacobs "When
  2½ p.m. came… two long, dark, massive lines" (**14:30**); Haskell ~15:00
  implied (cannonade end + immediate advance); ABT sheets ~15:00.
- 2026-07-10 verification pass: NPS Civil War Series heading "July 3, 1863,
  3 P.M. – The Charge" (VERIFIED); ABT "lurched forward near 3 p.m."
  (VERIFIED); but Wikipedia's sourced text says ~14:00 citing Coddington,
  Hess, Sears, and Wert together (VERIFIED at the Wikipedia layer only).
- **Proposed reading: ED-1 stands** — step-off 15:05 (t=7500), the frame the
  shipped macro file and every slice arrival time is arithmetic from. The
  14:00–14:30 scholarly center of gravity is recorded as tension §1.2(2), not
  adopted. Re-ruling would re-time the entire shipped reconstruction and is
  an owner decision with a direct-page verification of Hess/Sears as its
  precondition.

#### CA-J3A-7 — Emmitsburg Road crossing / Garnett's halt

- No witness clock; the macro keyframes hold Garnett at the road 8160–8280
  (15:16–15:18) and V2 authored the halt-and-dress pause there (ED-6, ED-12;
  claim-fences-high-climb; Peyton's OR report [peyton-or-1863] for the fences
  and the grass slope on Garnett's front).
- Tier B: leg arithmetic from CA-J3A-6 at drill-manual advance pace across
  ~1,100 m, self-timing to ±2–3 min (unit-truth-spec §movement legs).
- **Proposed reading:** adopt the macro window as the anchor, envelope ±4 min,
  explicitly conditional on ED-1's frame. **Extends ED-1/ED-6; formalized in
  ED-24.**

#### CA-J3A-8 — Stannard's change of front

- Sturtevant (sturtevant-1910): Stannard's "famous and now historic order …
  'Change front, forward on first company'" — no clock. Veazey's OR report
  (veazey-or): the 16th VT firing into Pickett's right flank, then reversing
  against Wilcox/Lang — sequence only. 13th VT monument (vt13-monument):
  "changed front forward on first company, advanced 200 yards, attacking the
  Confederate right flank" — no clock.
- Corpus pass-1 flagged Stannard's maneuver as time-poor (open question 4);
  the 2026-07-10 pass found no new clock. It is bracketed: after the column
  passes the road (CA-J3A-7), before the wall crisis peaks (CA-J3A-9).
- **Proposed reading:** ~15:20 (t=8400), tier D promoted to B by the bracket
  arithmetic; envelope 15:18–15:24. **Needs ED-24** (no prior ED covers it).

#### CA-J3A-9 — Armistead crosses the wall (= ED-2, unchanged)

- The event is documented by two independent eyewitnesses (Martin: "rushed
  forward, scaled the wall, and cried: 'Boys, give them the cold steel!'";
  Fuger: "I saw General Armistead leap over the stone wall … landing right in
  the middle of our Battery"); **no witness gives a clock** (verified
  negative — the Hartwig/NPS post has no times at all, fetch-verified).
- Literature frames: ABT climax sheet 3:45–4:15 PM; Stone Sentinels July 3
  timeline "3:30–4:00 p.m. … Armistead was mortally wounded" (VERIFIED
  2026-07-10) — both are step-off + interval arithmetic on *their* later
  step-off clocks, not observations.
- **Proposed reading: ED-2 stands** — t=8700 (15:25), envelope 8640–8760;
  the 2026-07-10 pass's recommendation matches V2 practice: express the
  climax as an offset from step-off (+20 min) rather than an absolute time,
  which is exactly what the shipped arithmetic does.

#### CA-J3A-10 — Repulse complete; the supporting advance

- The Angle: macro slice ends t=9000 (15:30) with Armistead's survivors
  falling back west (ED-8); Fuger's final-canister passage ("Pickett's charge
  collapsed") and Haskell's account carry the event; no clock.
- Wilcox/Lang: advanced in support after the main repulse and were smashed by
  McGilvery's line (Phillips's 5th MA: "Opened an enfilading fire soon after
  on Longstreet's advancing line of infantry … " then the Florida brigade,
  monument text VERIFIED in the full-cast survey) and by the 16th VT's
  reversed front (veazey-or). Wright's OR report (July 2 usage, VERIFIED)
  shows his "5 p.m." clock runs nominal — supporting-brigade report times are
  not anchor-grade.
- **Proposed reading:** Angle repulse t=9000 (15:30) tier B (macro end +
  ED-8); supporting advance 15:30–16:00 tier D (sequence after CA-J3A-9);
  envelope out to 16:15 acknowledging the ABT frame. **Extends ED-8;
  formalized in ED-24.**

### 1.2 Tensions with the shipped reconstruction — recorded, not resolved

1. **13:00 vs 13:07 (7 min).** The shipped slice clock defines t=0 = 13:00 =
   startTime 46800 and the macro file opens the bombardment at its nominal
   start; the proposed chain adopts Jacobs's 13:07 for CA-J3A-1. Proposal:
   the chain records `shippedT: 0` on CA-J3A-1 with an explicit
   `shippedSkewMinutes: -7` — i.e. the shipped file's t=0 is a *nominal*
   13:00 that the chain reads as 13:07. Nothing in the shipped slice
   (t=8040..9000) depends on the first seven minutes; no re-authoring is
   proposed. If the owner prefers, the alternative is to re-label startTime
   as 13:07 — a metadata-only change, but it re-times every derived wall
   clock in the captions; flagged as an owner choice.
2. **Step-off 15:05 vs the literature's 14:00–15:00 spread.** ED-1 chose the
   Haskell/ABT/NPS ~15:00 frame; the 2026-07-10 pass surfaced (at the
   Wikipedia-citation layer) that Coddington, Hess, Sears and Wert may
   center on ~14:00–14:30, with Jacobs himself at 14:30. If true at the page
   level, the shipped clock sits at the LATE edge of the scholarly envelope,
   not its center. This does not invalidate ED-1 (the envelope has always
   spanned 13:50–15:10, and internal arithmetic is self-consistent), but the
   proposal recommends: (a) verify Hess ch. on the bombardment and Sears
   pp. ~375–400 directly; (b) the owner then either re-affirms ED-1 or rules
   a re-time — the chain is built so that every downstream anchor
   (CA-J3A-7..10) is expressed relative to CA-J3A-6 and would shift with it.
3. **The 1:07 signal vs a 15:05 step-off implies a ~118-minute cannonade.**
   Haskell's "two mortal hours" supports this; Hess/Sears's shorter cannonade
   (~90–110 min ending 14:00–14:30) is the same tension as (2) seen from the
   other end. No shipped conflict; recorded.
4. **ABT climax frame (15:45–16:15) vs shipped 15:25.** Already recorded in
   ED-2 (claim-repulse-clock-frames); the chain keeps ED-2 and carries the
   ABT frame in CA-J3A-9/10 envelopes.

---

## 2. Skeleton chains — the other five phases

Grades below: each anchor lists adopted time, envelope, and its best sources
with reliability flags. These are candidate chains for owner review; each
phase's adoption would be its own ED (ED-26..ED-30). Quotes marked VERIFIED
were fetched 2026-07-10 by the verification passes.

### 2.1 July 1 morning — McPherson's Ridge (proposed ED-26)

**Pass-10 annotation (2026-07-12):** the chain is EXECUTOR-HARDENED —
CA-J1M-2 on Gamble's/Buford's own reports (the 8:00 warning + the
several-hours/two-hours duration pair); CA-J1M-4's carried
deploy-vs-attack precondition now has BOTH poles clocked (Davis's
10.30 formed-and-attacked verbatim vs the Wadsworth/Cutler/pin
receiving cluster 10:00-10:15) → **ADOPTED as ED-66 (2026-07-12,
pass-11 open): CA-J1M-4 split into 4a/4b below**; CA-J1M-5
upgraded to THREE-primary agreement (tablet = Wadsworth = Doubleday
at 10.15; Howard's 11.30 collapsed as word-reaching-him, Schurz
concurring — the upgrade rides ED-66); CA-J1M-6 gains a recorded
tension (Davis's 'about 1 p.m.' brigade retirement vs the
skeleton's ~11:00 cut conclusion). The Howard-assumes-command
sub-anchor is two-primary at ~11:30 → **ADOPTED as ED-68(a)
(~11:30; the tablet 10:30 collapsed onto the hurry-forward order;
Pfanz derivation an open gate)**. See dossier-pass-10.md §3/§5 and
angle-editorial-decisions.md ED-66…ED-68.

| id | Event | Adopted | Envelope | Principal sources |
|---|---|---|---|---|
| CA-J1M-1 | First shot, Chambersburg Pike (Jones, 8th IL Cav) | 07:30 | 07:00–08:00 | 1886 First Shot marker inscription, VERIFIED: "First shot / Gettysburg / July 1st 1863 / 7:30 a.m" (veteran self-commemoration; the *identity* was contested, the hour broadly accepted) |
| CA-J1M-2 | Buford's delaying fight opens (Gamble vs Heth) | 08:00–09:00 | 07:30–09:00 | Buford's OR report, VERIFIED reproduction: "between 8 and 9 a.m., reports came in … that the enemy was coming down from toward Cashtown in force"; Gamble "held its own for more than two hours" |
| CA-J1M-3 | **Buford's 10:10 dispatch to Meade** | 10:10 | ±0 (timestamped document) | SNIPPET via NPS Seminar 10 essay 3 (Phipps) — "driving my pickets and skirmishers very rapidly"; **the hardest clock of the morning; verify in OR 27/1 correspondence** |
| CA-J1M-4a | Heth deploys; artillery phase opens (ED-66 split) | ~08:00–09:00 | 07:30–09:30 | Gamble "About 8 o'clock… about 3 miles distant" + Buford "between 8 and 9 a.m." + the several-hours/two-hours delay-duration pair (executor primaries, pass 10); Heth's "at this time — 9 o'clock… I was ignorant what force was at or near Gettysburg" (deploy-phase CSA clock; time-silent on the attack) |
| CA-J1M-4b | Heth's infantry attack strikes the I Corps line (Archer/Davis; ED-66 split) | **~10:15–10:30** | 10:00–10:45 | **Davis "About 10.30 o'clock a line of battle was formed… and the brigade moved forward to the attack" (No. 553 p. 649, the only stated CSA attack clock)** vs the receiving cluster: Wadsworth 10:00 road-strike, Cutler "in action from 10 a.m.", the Reynolds 10:15 pin; the old single-row envelope closed from both sides |
| CA-J1M-5 | Reynolds arrives; Reynolds killed | arrive ~10:00; killed **10:15** | 10:00–11:00 | 1st Corps tablet, VERIFIED: "Arrived at Gettysburg between 10 A. M. and noon... General Reynolds fell mortally wounded about 10.15 A. M." — the park-commission ruling; eyewitness letter "about ten o'clock" (SNIPPET) |
| CA-J1M-6 | Railroad-cut fight concludes | ~11:00 | 10:30–11:30 | Dawes's OR report (VERIFIED, time-silent — the clock is modern back-solving from CA-J1M-5); Stone Sentinels timeline 10:45–11:00 (VERIFIED) |
| CA-J1M-7 | Midday lull onset | ~11:30 | 11:00–12:00 | Stone Sentinels timeline (VERIFIED); Rodes division tablet "occupied Oak Ridge about noon" (VERIFIED) bounds the far side |

Known disagreements, disposition as of pass-11 open: Howard-assumes-command
→ ADOPTED ~11:30 as the ED-68(a) sub-anchor (two-primary; tablet 10:30
collapsed; the Pfanz-derivation gate stays open on the record); Heth
deploy-vs-attack conflation → RESOLVED by the ED-66 split (rows 4a/4b
above).

### 2.2 July 1 afternoon — collapse and retreat through town (proposed ED-27)

**Pass-10 annotation (2026-07-12):** CA-J1P-2 (Iverson) upgraded
from slaved-sequence to a bracketed event with drawn geometry (the
j1-09 mid-advance bar; the dress-parade line four-source) and
CA-J1P-3 (Gordon/knoll) closed its MEMORY-grade precondition with
the page-verified Gordon primary — 'About 3 p.m.' pairing Schurz's
'about 3 o'clock' ACROSS THE LINES → **ADOPTED as ED-67 (2026-07-12,
pass-11 open; both anchors' adopted windows unchanged)**. CA-J1P-4/5
confirmed by the Howard 16:00/16:10/16:30 issuing ladder +
Wadsworth's 3.45 (front edge) + Doubleday's 'about 4 p.m.'.
CA-J1P-7 FORMALIZED as a structured conflict record (document pin +
Doubleday's dual-command scene + Schurz's after-5 corps bound, all
on the late pole) → **the front edge TIGHTENED 15:00 → 15:30 by
ED-68(b) (2026-07-12, pass-11 open; ~16:15 adoption unchanged;
Hancock's 15:00 carried as the report-nominal pole)**. CA-J1P-9 gains
Rodes's division-grain companion record (his verbatim non-attack
rationale). See dossier-pass-10.md §3/§5.

**Pass-11 annotation (2026-07-12):** CA-J1P-1 gains the full interior
ladder (Coulter 12:30 → Doles ~13:00 → **Stone = Dobke 13:30,
independent cross-corps** → tablet 14:00 adopted → Hill ~14:30) as
recorded structure, adopted time and envelope UNCHANGED → **ED-69
(2026-07-12, pass-12 open; annotation only, per ED-25 rule 4)**.
CA-J1P-5's basis upgrades to the Perrin-vs-receiving-cluster
cross-line pair (Perrin's "after 4 o'clock" order-to-advance vs the
tablet/Dobke/Jacobs/C. Biddle four-source ~4 p.m. cluster), plus
Robinson's "nearly 5 p.m." rear-guard tail on the back edge → **ED-70
(2026-07-12, pass-12 open; adopted time and envelope UNCHANGED)**.
See dossier-pass-11.md §3/§5.

| id | Event | Adopted | Envelope | Principal sources |
|---|---|---|---|---|
| CA-J1P-1 | Rodes attacks from Oak Hill (ends the lull) | **14:00** | 13:30–14:45 | 1st Corps tablet, VERIFIED: "made a vigorous attack at 2 P. M. with superior numbers along the entire line"; Stone Sentinels timeline 14:30; Rodes's own report has NO July 1 clocks (VERIFIED negative). **ED-69 interior ladder (annotation, no move):** Coulter "about half-past 12" general firing → Doles "about 1 p.m." formation → **Stone "At about 1.30 p.m. the grand advance of the enemy's infantry began" = Dobke "At about 1.30 p.m. a long line of the enemy moved... offering the flank"** (independent, cross-corps, report-nominal, offset −20 [−45,10]) → tablet 14:00 (adopted) → Hill "About 2.30" corps retrospective |
| CA-J1P-2 | Iverson's brigade destroyed (Forney field) | 14:30–15:00 | 14:15–15:15 | **ED-67 basis: bracketed event with drawn geometry** — the four-source dress-parade convergence (Iverson + Rodes + Ramseur + Cutler receiving) + the j1-09 drawn mid-advance bar (408 m along the advance axis from the formation tablet); tablet "almost annihilated," 820 of 1,470 |
| CA-J1P-3 | Early's assault on the XI Corps right (Barlow Knoll) | 15:00–15:30 | 14:30–15:45 | **ED-67 basis: the Gordon = Schurz cross-line pair** — Gordon "About 3 p.m. I was ordered to move my brigade forward" (page-verified, pass 10) vs Schurz "about 3 o'clock… heavy masses"; the old MEMORY-grade Early precondition CLOSED; Stone Sentinels timeline 15:00–16:00 (VERIFIED) |
| CA-J1P-4 | XI Corps collapse north of town | **~16:00** | 15:30–16:30 | 11th Corps tablet, VERIFIED: "About 4 P. M. the Corps was forced back and retired through the town to Cemetery Hill" |
| CA-J1P-5 | I Corps collapse on Seminary Ridge (Pender's assault) | **~16:00–16:30** | 15:45–16:45 | 1st Corps tablet, VERIFIED: "At 4 P. M. the Corps retired and took positions on Culps Hill and Cemetery ridge"; Wikipedia/Pfanz 16:00–16:30 (VERIFIED page). **ED-70 basis upgrade:** Perrin "I remained in this position probably until after 4 o'clock, when I was ordered by General Pender to advance" vs the receiving cluster (tablet + Dobke "about 4 p.m., when the First Corps, on our left, gave way" + Jacobs "At about 4 o'clock the regiment rallied" + C. Biddle "compelled about 4 p.m. to retire"); Robinson's "until nearly 5 p.m." carried as the rear-guard back-edge tail |
| CA-J1P-6 | Retreat through the town streets | 16:00→16:30 | 16:00–17:00 | interval hung between CA-J1P-4/5 and CA-J1P-7; ABT map sheet "3:45–5:00 pm" brackets it (VERIFIED) |
| CA-J1P-7 | Hancock arrives on Cemetery Hill | **~16:15** | **15:30–16:30 (front edge tightened by ED-68(b))** | THE canonical watch controversy, FORMALIZED (pass 10): Hancock's own report ~15:00 (report-nominal pole, carried) vs his own 17:25 dispatch's "an hour since" ⇒ ~16:25 (document pin AGAINST his own report), Howard's ~16:30, Doubleday's post-retreat dual-command scene, Schurz's "after 5 o'clock" corps bound; modern consensus (Coddington/Pfanz/Martin) 16:00–16:30. Both principals had stakes in the command-authority feud — partisan-dispute note stands |
| CA-J1P-8 | Hancock's "we can fight here" dispatch to Meade | 17:25 | ±0 if verified | Stone Sentinels timeline "5:25 p.m." (VERIFIED at the page level) — a timestamped document if confirmed in OR 27/1; second hard clock of the day |
| CA-J1P-9 | Ewell declines Cemetery/Culp's Hill ("if practicable") | unclocked | sequence: after CA-J1P-7, before dark | Lee's report wording via VERIFIED secondary ("Carry the hill occupied by the enemy, if he found it practicable…"); Ewell's report "jaded by twelve hours' marching and fighting" — correctly scoped sequence-only, no clock assigned |

### 2.3 July 2 afternoon — the en-echelon assault (adopted ED-28; CA-J2A-2/3 REVISED by ED-53, 2026-07-11)

**Frame note (VERIFIED):** sunset July 2 was 19:29 local mean time (Stone
Sentinels timeline states "7:29 p.m."); the familiar "19:41" is the same event
in the modern-EST frame. See ED-31.

**Tablet-skew rule to adopt with this chain:** the McLaws-wing tablets run
systematically ~1 hour EARLY against modern scholarship (Kershaw tablet
"advanced about 4.30" vs modern ~17:30; Barksdale tablet "Advanced at 5 P.M."
vs modern ~18:15), while the Ewell-front tablets match modern times well.
Treat McLaws-wing tablet clocks as a clock-offset class of their own.
*(Extended by ED-55, 2026-07-11: the skew ORIGINATES in the wing's OR
reports — report-nominal +45..+105 early, exemplar or-27-2-kershaw +90;
the tablets are partial corrections of the same house clock.)*

**ED-53 revision (2026-07-11):** CA-J2A-2/3 were revised on dossier
pass 5's executor evidence and upgraded from provisional to ruled —
CA-J2A-2 to a recorded two-pole structure ~15:45–16:20 (Alexander
15:45 + Scruggs/Oates arrivals vs Jacobs's 16:20 clock reading);
CA-J2A-3 to **~16:30, envelope 16:15–17:00** (the 16:00 signal-dispatch
document pin excludes the old window's front edge; Alexander's 30-min
arithmetic; Law's "near 5 o'clock"; Oates's dark arithmetic;
Robertson's few-minutes-before; Norton's compiled 16:30–17:00 first
contact). Downstream anchors CA-J2A-5/6/7/9 are CONFIRMED by the same
executor ladder, not moved. The rows below carry the revised values;
the superseded pre-revision values are preserved in this note and in
ED-53's full text (angle-editorial-decisions.md).

**ED-57/ED-58 annotation (2026-07-11):** the Wheatfield occupation-wave
record (ED-57) CONFIRMS CA-J2A-8 as wave 3's start and exercises
CA-J2A-11 as wave 5's astronomical end pin — no anchor moves; the
report-clock early-skew class is generalized FIELD-WIDE (both armies,
+45..+110) by ED-58, ED-25 rule 4 unchanged.

| id | Event | Adopted | Envelope | Principal sources |
|---|---|---|---|---|
| CA-J2A-1 | Sickles's salient complete (Peach Orchard line occupied) | ~15:30 | 14:00–15:30 | Stone Sentinels timeline "Noon" conflates the Pitzer Woods recon with the corps advance (VERIFIED); Pfanz 14:00–15:30 (attributed). Anchor the *completed* salient |
| CA-J2A-2 | Longstreet's artillery opens | **~15:45–16:20 two-pole (ED-53)** | 15:30–16:20 | REVISED ED-53: Alexander 15:45 arithmetic + Scruggs/Oates 15:30–15:45 arrivals (early pole) vs **Jacobs "at twenty minutes past 4 P.M. the enemy began the battle of the 2d" — 16:20, the chain's precision standard at the late pole (VERIFIED)**; McLaws/Hood tablets "About 4 P.M." (VERIFIED) sit between |
| CA-J2A-3 | Hood's division steps off | **~16:30 (ED-53)** | 16:15–17:00 | REVISED ED-53 (was 16:00–16:30): the 16:00 Union signal dispatch quoted by Law — a timestamped DOCUMENT excluding the old front edge; Alexander's 30-min arithmetic ⇒ ~16:15–16:25; Law "near 5 o'clock" (late pole); Oates dark-minus-four-hours; Robertson "but a few minutes before"; Norton compiled 16:30–17:00 first contact. Hood tablet "soon after" 16:00 (VERIFIED) carried as tablet-class reading. The dawn-attack-order claim is Lost Cause polemic — documented separately from the clock |
| CA-J2A-4 | Warren on Little Round Top; Vincent diverted | 15:30–16:00 | 15:30–16:00 | Stone Sentinels timeline 16:00 (VERIFIED); Warren's 1872 Farley letter and Pfanz ~15:40 unfetched — follow-up |
| CA-J2A-5 | Devil's Den falls | ~17:30 (downstream-CONFIRMED, ED-53) | 17:15–18:00 | Stone Sentinels timeline: attack 16:30, "fierce fighting lasting over an hour" (VERIFIED); Law tablet credits 44th/48th AL + 3 guns of the 4th NY (VERIFIED, no clock) |
| CA-J2A-6 | Little Round Top climax (Vincent mw; O'Rorke; 20th Maine charge) | fight 16:45–19:00; charge **~18:30** (downstream-CONFIRMED, ED-53; evening end triple-anchored: Rice 20:00 + Chamberlain 21:00 + j2-05 drawn state) | 17:30–18:45 | Stone Sentinels timeline puts the whole climax at 17:30 — an hour earlier than the modern ~18:15–18:45 (VERIFIED, flagged); Chamberlain's OR "reaching the field at about 4 p.m." (SNIPPET, 403 on the CSI reprint — re-fetch); dedicated verification pass needed (Pfanz, Norton, Oates) |
| CA-J2A-7 | Kershaw steps off (McLaws wing engages) | **~17:30** (downstream-CONFIRMED, ED-53; executor trigger record + ladder arithmetic, pass 5) | 16:30 (tablet)–17:45 | Kershaw tablet "Advanced about 4.30 to battle" (VERIFIED — the skew exemplar); Stone Sentinels timeline 17:30 (VERIFIED); Wikipedia "By 5:30 p.m. … Kershaw's regiments neared the Rose farmhouse" (VERIFIED); Kershaw's OR unfetched |
| CA-J2A-8 | Caldwell's counterattack into the Wheatfield | 18:00–18:15 **(CONFIRMED as wave 3's start, ED-57 2026-07-11)** | 17:45–18:30 | Stone Sentinels timeline 18:00 (VERIFIED); ABT map-sheet slicing 17:45–18:30 (titles as the Trust's adopted clock) |
| CA-J2A-9 | Barksdale smashes the Peach Orchard; Sickles wounded | **18:00–18:30, ~18:15 preferred** (downstream-CONFIRMED, ED-53; drums pin + 20-min ladder interval) | 17:00 (tablet)–18:30 | Barksdale tablet "Advanced at 5 P.M." (VERIFIED, skewed early); Stone Sentinels timeline 18:00 (VERIFIED); Pfanz ~18:15 (attributed) |
| CA-J2A-10 | Wright reaches Cemetery Ridge; 1st Minnesota's charge | **19:00–19:15** ("shortly before sunset") | 18:00–19:15 | Wright's OR: general advance signal "at about 5 p.m." (VERIFIED — notoriously self-serving report); Wright tablet "Advanced at 6 P.M." (VERIFIED); Stone Sentinels timeline 18:30–19:00 for both events (VERIFIED); 1st MN monument has no clock (VERIFIED). A 2-hour spread on one brigade — pin to sunset-adjacency |
| CA-J2A-11 | Astronomical pin: sunset | **19:29 LMT** (exercised as wave 5's end pin — Wofford's "fell back at sunset", ED-57) | ±1 min | Stone Sentinels "7:29 p.m. Sunset" (VERIFIED); = 19:41 EST. Frame per ED-31 |

### 2.4 July 2 evening — Culp's Hill / East Cemetery Hill (proposed ED-29)

| id | Event | Adopted | Envelope | Principal sources |
|---|---|---|---|---|
| CA-J2E-1 | XII Corps (less Greene) ordered away to the left | 18:30–19:00 | 18:30–19:00 | Greene brigade tablet: "At 6.30 P.M. the First and Second Brigades were ordered to follow the First Division…" (VERIFIED); ABT "just before 7:00 p.m." (SNIPPET) |
| CA-J2E-2 | Johnson assaults Culp's Hill (Greene's defense) | **~19:00 start; lower works occupied 20:00** | 18:00 (Steuart tablet crossing)–20:00 | Johnson tablet "the infantry advanced to assault at dusk" (VERIFIED); Steuart tablet "Crossing Rock Creek at 6 P.M." (VERIFIED — crossing ≠ contact, model as two events); Greene tablet: "four distinct charges and at 8 P.M. occupied the works that the First Division had vacated" (VERIFIED); Pfanz ~19:00 via Wikipedia p. 207 (VERIFIED page) |
| CA-J2E-3 | Early assaults East Cemetery Hill | **19:45–20:00** | 19:30–20:00 | **Hays's OR report, VERIFIED (NPS): "A little before 8 p.m. I was ordered to advance with my own and Hoke's brigade" — the best single clock of the evening**; Hoke tablet "at 8 P.M. … charged East Cemetery Hill … planted its colors on the lunettes" (VERIFIED); NPS Civil War Series heading "East Cemetery Hill 8-9 P.M." (VERIFIED). Rare tablet/OR/modern agreement |
| CA-J2E-4 | Carroll's counterattack clears the batteries | ~20:30 | 20:00–21:00 | Carroll tablet: "At dark the Brigade went to relief of Eleventh Corps and was hotly engaged … until after 10 P.M." (VERIFIED — no hard clock; ~20:30 is inferred from CA-J2E-3 + fight duration; flag as inferred anchor) |
| CA-J2E-5 | Fighting dies out | 22:00–22:30 | 21:30–23:00 | Hays's OR "about 10 o'clock" withdrawal complete (VERIFIED); Carroll tablet "until after 10 P.M."; Wikipedia "battlefield fell silent around 10:30 p.m." (VERIFIED, uncited). Note: Meade's 20:00 "repulsed at all points" telegram PREDATES the end of fighting — good pipeline consistency test |

### 2.5 July 3 morning — Culp's Hill (proposed ED-30)

**ED-64/ED-65 annotation (2026-07-11):** CA-J3M-1 keeps 04:30 at
author-primary basis (Muhlenberg = Ruger = Williams = tablet) and
carries the 03:00–04:00 infantry-escalation preamble plus the two
author-vs-tablet conflicts (20-vs-26 guns; 10:00-vs-10:30 end) as
recorded structure (ED-65 — no move). CA-J3M-3's evidence is
RESTRUCTURED as a two-pole primary conflict (ED-64): EARLY
~05:30–06:00 (Morse 5.30 / Fesler between-5-and-6 / the 27th IN
advance marker's inscribed 6 a.m.) vs LATE ~10:00 (Ruger's order
clock + Bachelder's drawn adjudication — the charge appears on
j3-02, 8–11 a.m., only); the ~10:00 ruling direction stays
PROVISIONAL behind the standing Pfanz pp. ~340–355 gate; no cast
renders the charge until the gate resolves.

| id | Event | Adopted | Envelope | Principal sources |
|---|---|---|---|---|
| CA-J3M-1 | XII Corps artillery opens (26 guns) | **04:30 ("daylight")** | 04:15–04:45 | XII Corps Artillery tablet, VERIFIED: "At daylight the artillery (26) guns opened … and fired for about 15 minutes then ceased to allow the infantry to advance. Began firing again at 5.30 and continued at intervals until 10.30 A.M." — the richest single source of the phase (opening, pause, resumption, end); Jacobs: "From 4½ to 10½ A.M. … our men pushed the enemy backward" (VERIFIED) |
| CA-J3M-2 | Johnson's renewed assaults (three waves) | first ~04:30–05:30; second ~08:00–08:30; final ~10:00 | 04:30–11:00 | Johnson tablet: "The assault was renewed in early morning… two other assaults were made and repulsed" (VERIFIED, explicit 3-wave structure); Greene tablet "a fierce engagement ensued for seven hours" (VERIFIED); Pfanz final attack ~10:00 via Wikipedia pp. 310–325 (VERIFIED page). Wave 2 (07:00 vs 08:30) is the softest clock |
| CA-J3M-3 | 2nd MA / 27th IN charge, Spangler's Meadow | **~10:00** | 06:00–"near noon" — widest spread of the phase | 2nd MA monument (VERIFIED, no clock); Stone Sentinels timeline 06:00 (VERIFIED — outlier, likely conflation with Ruger's probing); Pfanz-derived accounts ~10:00 (SNIPPET); Sears "near noon" via Wikipedia (VERIFIED page). Rule via Pfanz Culp's Hill pp. ~340–355 before adoption |
| CA-J3M-4 | Final repulse; Johnson retires east of Rock Creek | **10:30–11:00** | tight | Johnson tablet: "Retired at 10.30 A.M. to former position of July 2" … "held until 10 P.M." (VERIFIED — exactly per the full-cast survey); Steuart tablet: fighting "raged fiercely until 11 A.M." (VERIFIED); XII arty tablet "until 10.30 A.M."; Jacobs "10½ A.M." — quadruple attestation, the best-anchored event outside July 3 afternoon. Adopt 10:30 division order / 11:00 last units clear |
| CA-J3M-5 | Astronomical pins | first light ~04:00–04:15; sunrise **~04:38 LMT** | — | tablets' "daylight" ≈ civil twilight ≈ 04:30; the 05:45 figure circulating is a modern-EDT frame artifact. Recompute per ED-31 |

---

## 3. The per-source clock-offset scheme (proposed ED-25)

### 3.1 Schema

Add one optional object to each record in `reconstruction/sources/sources.json`:

```json
"clockProfile": {
  "kind": "watch-checked | retrospective-watch | contemporaneous-civilian | report-nominal | tablet-adjudicated | none",
  "offsetMinutes": 7,
  "offsetEnvelope": [2, 12],
  "anchorsUsed": ["CA-J3A-1", "CA-J3A-5"],
  "assessment": "one-paragraph method note: which anchor statements were compared and how",
  "confidence": "strong | medium | weak"
}
```

Semantics: `canonical time = source's stated time + offsetMinutes`. A claim
whose only timing evidence is source S's stated clock is tier C with the
corrected time and an envelope widened by `offsetEnvelope`. The profile is
recorded ONCE on the source and inherited by all its claims (unit-truth-spec:
"recorded once on the source and inherited by its claims").

### 3.2 Assessment method

1. Collect every statement the source makes about a chain anchor event
   (its "anchor statements").
2. `offsetMinutes` = median of (chain adopted time − source stated time)
   over those statements; `offsetEnvelope` = min/max of the differences,
   widened to the kind-default when fewer than two anchor statements exist.
3. Kind defaults where data is thin: watch-checked ±5; contemporaneous-civilian
   ±5; retrospective-watch ±15; tablet-adjudicated ±30 (±60 on the McLaws
   wing, per the ED-28 skew rule); report-nominal ±45.
4. **Rule: report-nominal clocks never define or move an anchor.** They are
   sequence evidence (tier D) unless corroborated; the offset exists so the
   audit can *display* a corrected reading, not to promote the source.
5. Profiles are re-assessed whenever a phase chain is adopted or revised;
   `anchorsUsed` makes the derivation reproducible.

### 3.3 Worked examples

**(1) Jacobs (`jacobs-1864`) — kind `contemporaneous-civilian`, offset 0,
envelope [−2, +2], confidence strong.** The chain's precision standard:
CA-J3A-1 is *defined* on his "at seven minutes past 1 P.M." reading, so his
offset is 0 by construction. Cross-checks: his July 2 artillery opening "at
twenty minutes past 4 P.M." sits at the late edge of that event's envelope
(the tablets say "about 4") — consistent with a man reading a clock rather
than rounding; his July 3 morning "From 4½ to 10½ A.M." agrees with the
tablet 04:30/10:30 pair to the half hour. Caveats ride the source record:
distant observer; his "150 guns on each side" overstates the initial Union
reply.

**(2) Haskell (`haskell-1908`) — kind `watch-checked`, offset +7, envelope
[+2, +12], confidence medium.** Two anchor statements: watch-check "five
minutes before one o'clock" immediately before the first gun (stated ~13:00
vs chain 13:07 → +7) and cannonade end "at three o'clock almost precisely"
(stated 15:00; corrected 15:07 — inside CA-J3A-5's envelope, and consistent
with ED-1's arithmetic which was built on his stated time). His
"half-past two o'clock, an hour and a half since the commencement" is
internally consistent (13:00 + 1:30 = 14:30 on his own watch). Offset +7 from
the first statement; envelope reflects one-source derivation. Partisanship
(72nd PA) affects his facts, not his watch.

**(3) Alexander (`alexander-1907`) — kind `retrospective-watch`, offset +7,
envelope [0, +15], confidence weak-to-medium.** One anchor statement: "It was
just 1 P.M. by my watch when the signal guns were fired" (stated 13:00 vs
chain 13:07 → +7). His 13:25 and 13:40 note times correct to 13:32/13:47
(CA-J3A-2/3). His step-off "doubtless 1.50 or later, but I did not look at my
watch again" is self-hedged and excluded from anchor statements. The 44-year
retrospective distance caps confidence regardless of arithmetic: envelope
held wide.

**(4) OR report times — kind `report-nominal`, per-report offsets, envelope
±45, confidence weak. Two sub-examples:**
- *Longstreet's report* (`or-27-2-longstreet`, VERIFIED via the NPS
  reproduction): "About 2 p.m. … Colonel Walton was ordered to open the
  batteries. The signal guns were fired." Stated ~14:00 vs chain 13:07 →
  raw offset −53 min. Per rule 4 this does NOT correct into the chain: the
  profile records the raw offset so the provenance UI can show "Longstreet's
  report clock runs ~50 min late against the chain," and his July 2 "about
  4 p.m. (in position)" is read with the same caution.
- *Smyth's report* (corpus pass-1): cannonade *start* stated 14:00 → same
  raw −53. Two independent Union/Confederate reports sharing the ~14:00
  start suggests a genuine "camp clock" cluster — worth a note in the
  conflict record rather than silent correction; if more reports cluster
  there, the cluster itself becomes evidence about how field time was kept
  (and feeds tension §1.2(2)).
- *Hays's report* (`or-27-2-hays`, VERIFIED): "A little before 8 p.m. I was
  ordered to advance" — an example of a report clock that AGREES with its
  phase chain (CA-J2E-3); offset ~0, but kind still caps it at tier C.

**(5) Tablets (`stone-sentinels` War Department tablets) — kind
`tablet-adjudicated`, offset 0 default with the McLaws-wing exception,
envelope ±30 (±60 McLaws wing), confidence medium.** The tablets are
adjudications, not witnesses; on Ewell's front they match modern scholarship
(Hoke 20:00; Johnson "dusk"; retired 10:30), on the McLaws wing they run ~1 h
early (Kershaw 16:30, Barksdale 17:00). The profile mechanism handles this as
a per-wing note on the source record.

---

## 4. Proposed new EDs (owner rulings needed)

| ED | Scope | Status |
|---|---|---|
| **ED-24** | Adopt the canonical anchor chain structure (`CA-<phase>-<n>`) and the July 3 afternoon chain (§1.1), formalizing ED-1/ED-2 as chain nodes CA-J3A-6/9; record the 13:00-vs-13:07 shipped skew and the step-off literature tension (§1.2) | drafted here, ready for ruling |
| **ED-25** | Adopt the `clockProfile` schema, assessment method, and the five worked profiles (§3) | drafted here, ready for ruling |
| **ED-26** | July 1 morning chain (§2.1) | ADOPTED provisional 2026-07-10; CA-J1M-3 document-pin verified at adoption; EXECUTOR-HARDENED by dossier pass 10 (2026-07-12); **CA-J1M-4 SPLIT into 4a/4b + CA-J1M-5 three-primary upgrade by ED-66, and the Howard ~11:30 sub-anchor adopted by ED-68(a) (2026-07-12, pass-11 open)** |
| **ED-27** | July 1 afternoon chain (§2.2), incl. the Hancock-arrival ruling (~16:15, partisan-dispute note) | ADOPTED provisional 2026-07-10 (Hancock/Howard texts verified at adoption); EXECUTOR-HARDENED by dossier pass 10 (2026-07-12); **CA-J1P-2/3 basis upgrades adopted by ED-67, CA-J1P-7 front edge tightened to 15:30 by ED-68(b) (2026-07-12, pass-11 open); CA-J1P-1 interior ladder annotated by ED-69 and CA-J1P-5 basis upgraded + Robinson rear-guard tail by ED-70 (2026-07-12, pass-12 open; no moves)** |
| **ED-28** | July 2 afternoon chain (§2.3), incl. the Longstreet step-off ruling (16:00–16:30; the dawn-order claim documented as polemic, not clock) and the McLaws-wing tablet-skew rule | ADOPTED provisional 2026-07-10; precondition (LRT verification pass) CLOSED by dossier pass 5; CA-J2A-2/3 REVISED & upgraded to ruled by ED-53 (2026-07-11); skew rule extended to report clocks by ED-55; CA-J2A-8 CONFIRMED + CA-J2A-11 exercised by the ED-57 wave record; skew class generalized field-wide by ED-58 (2026-07-11) |
| **ED-29** | July 2 evening chain (§2.4) | skeleton; strongest of the five (Hays's OR clock) |
| **ED-30** | July 3 morning chain (§2.5), incl. the Spangler's Meadow ruling (~10:00 vs the 06:00 outlier) | ADOPTED provisional 2026-07-10; hardened by dossier pass 9 (CA-J3M-1 author primary, wave clocks, receiving-side 10.30); CA-J3M-1 annotated + CA-J3M-3 restructured two-pole by ED-64/ED-65 (2026-07-11) — the ~10:00 direction stays provisional; precondition Pfanz pp. ~340–355 STANDS |
| **ED-31** | Time-frame convention: the battle clock is Gettysburg local mean time (~9 min ahead of modern EST); astronomical pins adopted as sunset July 2 19:29 LMT, sunrise July 3 ~04:38 LMT, with a USNO-grade recomputation as a follow-up; all imported modern-frame times (EST/EDT artifacts like "19:41" / "05:45") converted on entry | drafted here, ready for ruling |

## 5. Open questions and risks

1. **The July 3 step-off center of gravity** (§1.2(2)) — the one place the
   shipped clock may sit at the edge of the scholarly envelope. Blocking
   verification: direct pages of Hess (2001) and Sears (2003); both
   in-copyright, times as attributed facts. Until then ED-1 stands.
2. **OR Part 1 access** remains the standing blocker for Union report quotes
   (Hancock, Howard, Greene, Chamberlain, McGilvery, Osborn) — same item
   flagged in the pass-2 library and the full-cast survey.
3. **Timestamped dispatches** (Buford 10:10, Hancock 17:25) would be the two
   hardest clocks in the whole battle if verified in the OR correspondence
   volumes — highest-value single fetches for July 1.
4. **Tablet extraction hygiene:** two suspect tablet readings surfaced
   (Early's division "about noon" arrival; XI Corps "10.30 A.M." Schurz
   arrival) — re-fetch the tablet pages directly before encoding those two.
5. **Stone Sentinels day-timeline pages** are convenient but derivative;
   this proposal never rests an adoption on them alone.
6. Astronomical recomputation (ED-31) — script against a solar ephemeris for
   39.82°N 77.23°W, July 1–3 1863, LMT; cheap and removes a whole class of
   frame bugs.
