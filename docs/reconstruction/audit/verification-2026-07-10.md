# Verification pass — 2026-07-10 (audit R1 adoption, Step 1)

Every quote below was fetched on **2026-07-10** from the URL given with it.
Access routes are named per item because several channels required
workarounds (Google Books search-within-volume API, Open Library full-text
search, Wayback captures of ehistory.osu.edu). Items that could NOT be
reached are listed honestly in §6 — nothing below is papered over.

This pass exists to discharge the owner's evidence-contingent instruction on
the R1 anchor-chain proposal ("adjust the step-off to match our research"):
the R1 pass had seen — one hop removed, at the Wikipedia-citation layer — a
possible 14:00–14:30 modern center of gravity for the July 3 step-off vs the
shipped 15:05 (ED-1). §1 verifies the four cited scholars at the page level;
§2 re-verifies the primary spine from full texts; §3–§5 cover the two
timestamped dispatches, the two suspect tablet extractions, and the OR 27/1
Union reports. The ruling that consumes this file is the **ED-1 revisit**
and **ED-24** in `docs/reconstruction/angle-editorial-decisions.md`.

---

## 1. The four modern scholars, at the page level

### 1.1 Coddington, *The Gettysburg Campaign: A Study in Command* (1968) — VERIFIED, supports ~15:00

Access: Google Books search-within-volume API on the Touchstone reprint
(volume `P-7RMD77eQcC`, print page ids), https://books.google.com/books?id=P-7RMD77eQcC.

- **p. 493** (cannonade opens): "…signal guns, and 100,000 heads turned
  toward the Peach Orchard. Over 150 guns responded to the signal with a
  roar as their projectiles hurled toward the Union lines. Colonel
  Alexander, who was in charge of the cannonade, pulled out his watch,
  looked at it, and snapped it shut. It was just one o'clock. Then he got
  behind a tree to steady his spy glass and intently watched the effect of
  the fire."
- **p. 499** (duration): Union infantry endured "…two hours under the storm
  of hissing and exploding shells."
- **p. 502** (STEP-OFF): "…At 3:00 p.m. officers and men of Hancock's
  Second Corps looking west saw a long gray line suddenly emerge into the
  bright sunlight from the dark fringe of…" [the woods on Seminary Ridge].
- **p. 682** (endnote, bonus corroboration for §3): "At 10:10 A.M. Buford
  sent Meade a message about the advance of the enemy and said Reynolds was
  advancing and was within three miles of 'this'…"

**Verdict:** Coddington's chronology is cannonade ~13:00 → ~15:00 (two
hours) with the step-off at **15:00**. Wikipedia's footnote for "stepped off
… at about 2:00 p.m." cites *Coddington p. 502* — the page itself says
3:00 P.M. **The one-hop citation is refuted at the page level.** (Wikipedia's
*other* footnote, on durations, correctly reports "Coddington indicates the
bombardment stopped at 3 p.m." — the article contradicts itself.)

### 1.2 Hess, *Pickett's Charge — The Last Attack at Gettysburg* (2001) — VERIFIED, supports ~14:00

Access: Google Books search-within-volume API (ebook volume `U2E7TZOcb_QC`,
position ids `PT…`, not print pages), https://books.google.com/books?id=U2E7TZOcb_QC;
plus Open Library full-text search (https://openlibrary.org/search/inside)
snippets attributed to editions OL24083718M/OL3944607M of the same title.
Wikipedia's citation places the step-off discussion at print p. 162.

- Cannonade start (OL fulltext): "…noting that the bombardment erupted at
  precisely 1:07 P.M. Alexander, who was in charge of the show, recorded…"
  (the "meticulous recorder" is Jacobs: "Professor Michael Jacobs,
  instructor of mathematics and chemistry…" [GB PT120]). Hess adopts
  **Jacobs's 13:07**.
- [PT138]: "…1:30 P.M. and demanded to know why his guns were not in
  action…" (Hancock/Hart exchange, mid-bombardment).
- [PT151]: "…1:45 P.M., Longstreet found Alexander and Maj. John C.
  Haskell… 'His first question was what we thought we had done'…
  Alexander thought, and Haskell agreed, that they had 'silenced…'"
- [PT152] (STEP-OFF): "…2:00 P.M., Pickett's division approached the
  artillery line with a steady tread. Any opportunity for Longstreet to
  stop the attack was finally ended."
- [PT156]: "The temperature was eighty-seven degrees at 2:00 P.M., and it
  would rise at least three degrees during the course of the attack."
- Repulse (OL fulltext): "…flee or surrender. The time was somewhere
  between 2:30 and 2:45 P.M. The survivors of Fry's, Marshall's…"

**Verdict:** Hess runs the entire assault on an early clock: cannonade
13:07–~14:00 (under an hour), **step-off ~13:50–14:00**, repulse complete
14:30–14:45. Materially earlier than the shipped frame, page-verified.

### 1.3 Wert, *Gettysburg, Day Three* (2001) — VERIFIED, supports ~14:05–14:15

Access: Google Books search-within-volume API on the Touchstone paperback
(volume `wsu3YM0wTxEC`, print page ids), https://books.google.com/books?id=wsu3YM0wTxEC.

- **p. 166** (cannonade opens): "It was 1:00 P.M., or a few minutes after.
  As one of Longstreet's staff officers had predicted only three hours
  before, 'This will be a great day in history.'"
- **p. 182** (duration — the page Wikipedia cites for the step-off):
  "…cannonade from forty-five minutes to two hours or more, but the most
  reliable evidence places it at about an hour. The Confederates could not
  have sustained a two-hour bombardment, given their limited supply of
  solid shots and shells."

**Verdict:** Wert = ~one-hour cannonade from ~13:05, step-off **~14:05–
14:15**. Earlier school, page-verified. Note his stated basis is
**ammunition arithmetic** ("could not have sustained"), an argument, not a
clock observation.

### 1.4 Sears, *Gettysburg* (2003) — **UNVERIFIED at the page level**

Channels tried and outcomes, all 2026-07-10:

- archive.org lending copy `gettysburg00sear`: search-inside endpoint
  returns 403 / "Item not available" without a borrower session.
- Google Books: hardcover `pd52AAAAMAAJ` and Mariner paperback are
  `noview`; the ebook `_7y7BwAAQBAJ` has "partial" preview but its
  search-within index returns 0 results for every term including "Pickett"
  (broken/disabled index).
- Open Library full-text search: confirms Sears's *Gettysburg* (editions
  OL22535304M / OL3578734M) contains a section opening "PROFESSOR MICHAEL
  JACOBS of Pennsylvania College, as meticulous as a mathematician was
  expected to be, continued carefully recording the weather even amidst
  the…" — i.e. Sears leans on Jacobs — but his cannonade/step-off clock
  sentences could not be pulled (common-phrase queries cap out).
- A citing work pins Wikipedia's cited page to the charge's distance, not
  its clock: "mile and a half: Sears, Gettysburg, 415" (notes of French,
  *The Man Who Would Not Be Washington*, via Open Library full-text).

One-hop layer, recorded as ATTRIBUTED, NOT VERIFIED: Wikipedia's duration
footnote says "Sears states the bombardment ended at 2:30 p.m."; its
step-off footnote cites Sears p. 415 among the four "about 2:00 p.m."
citations — a footnote demonstrably wrong about Coddington (§1.1), so it
cannot be trusted about Sears either.

### 1.5 The tertiary layer itself (fetched for the record)

- Wikipedia "Pickett's Charge" (https://en.wikipedia.org/wiki/Pickett%27s_Charge):
  cannonade "starting around 1 p.m." (citing Coddington 485, Sears 377–80,
  Wert 127); durations footnote: Coddington stop 3 p.m. / Hess "essentially
  over by 2 p.m." / Wert "most reliable" one hour / Sears ended 2:30;
  step-off sentence "stepped off … at about 2:00 p.m." citing Coddington
  502 + Hess 162 + Sears 415 + Wert 182. The Coddington cite is refuted
  (§1.1); Hess and Wert check out (§1.2, §1.3); Sears unverified.
- Encyclopedia Virginia "Pickett's Charge"
  (https://encyclopediavirginia.org/entries/picketts-charge/): "at one
  o'clock in the afternoon unleashed about an hour-long bombardment … The
  Confederate infantry marched at around two o'clock." (Tertiary; follows
  the Hess school.)

---

## 2. Primary spine re-verification (full texts, public domain)

### 2.1 Jacobs (1864) — re-verified from the archive.org full text

Source: `notesonrebelinva00jaco` full text
(https://archive.org/download/notesonrebelinva00jaco/notesonrebelinva00jaco_djvu.txt),
pp. 40–42 of *Notes on the Rebel Invasion of Maryland and Pennsylvania*.

- Lull: "From 11 a. m. to 1 p. m. there was a perfect lull, each party
  apparently waiting…"
- **Cannonade opens 13:07:** "At seven minutes past 1 p. m., the awful and
  portentous silence was broken. Probably not less than 150 guns on each
  side belched forth the missiles of death…"
- **Step-off 14:30:** "When 2½ p. m. came, it witnessed a determined
  effort on the part of the enemy to accomplish this result… At this
  time, Pickett's division of Longstreet's corps, consisting of the
  brigades of Garnett, Kemper, and Armistead, was seen to emerge from the
  wooded crest of the Seminary ridge, just to the south of McMillan's
  orchard, and to move in two long, dark, massive lines, over the plain
  towards our left centre."

Jacobs — the chain's precision standard for 13:07 — puts the step-off at
**14:30** on the same clock: an ~83-minute cannonade. He agrees with
NEITHER school and is the strongest single piece of earlier-step-off
evidence in the corpus.

### 2.2 Haskell (1908) — re-verified from the archive.org full text

Source: `battleofgettysbu00hask` full text
(https://archive.org/download/battleofgettysbu00hask/battleofgettysbu00hask_djvu.txt).

- Watch check at the first gun: "…I yawned and looked at my watch; it was
  five minutes before one o'clock. I returned my watch to its pocket…
  What sound was that? There was no mistaking it! The distinct, sharp
  sound of one of the enemy's guns…"
- Mid-bombardment markers: "An hour has droned its flight since first the
  roar began." … "Half past two o'clock, an hour and a half since the
  com[mencement]…"
- Duration: "Cipher out the number of tons of gunpowder and iron that made
  these two hours hideous."
- **Cannonade ends 15:00:** "At three o'clock, almost precisely, the last
  shot hummed and bounded and fell, and the cannonade was over."

Haskell attests a **two-hour cannonade ending 15:00** on a repeatedly
watch-checked clock that read ~13:00 at the 13:07 signal (offset +7, per
his ED-25 profile). He is the direct, internally consistent counter to the
Hess/Wert one-hour reading.

### 2.3 Alexander (1907) — re-verified from the archive.org full text

Source: `militarymemoirso00alex` full text
(https://archive.org/download/militarymemoirso00alex/militarymemoirso00alex_djvu.txt),
pp. 422–424.

- "It was just 1 P.M. by my watch when the signal guns were fired and the
  cannonade opened."
- Plan: "I dared not presume on using more ammunition than one hour's
  firing would consume… So I determined to send Pickett the order at the
  very first favorable sign and not later than after 30 minutes' firing."
- 13:25 note (his watch): "General: If you are to advance at all, you must
  come at once or we will not be able to support you as we ought. But the
  enemy's fire has not slackened materially and there are still 18 guns
  firing from the cemetery."
- 13:40 note: "For God's sake come quick. The 18 guns have gone. Come
  quick or my ammunition will not let me support you properly."
- Step-off hedge: "It was doubtless 1.50 or later, but I did not look at
  my watch again." (Longstreet joining him at the guns; Pickett's line
  "scarcely 300 yards behind my guns" had still not come forward.)

Alexander's note chain is what the Hess school's clock rides on. Taken at
face value it is very hard to reconcile with a 15:05 step-off (85 minutes
between "come quick" and the advance); taken against Haskell/Hancock/
McGilvery/Jacobs it is equally hard to reconcile the other way (his "one
hour's firing" plan vs their 1.5–2-hour observed duration). His +7 watch
offset (vs Jacobs) moves nothing materially. This conflict is the genuine
crux; it is recorded, not resolved, in ED-24 §tension (2).

### 2.4 Union OR clocks bearing on the step-off (fetched this pass — see §5)

- **Hancock** (OR 27/1 pp. 372–373): cannonade opened "About 1 o'clock,
  apparently by a given signal"; "After an hour and forty-five minutes,
  the fire of the enemy became less furious, and immediately their
  infantry was seen in the woods beyond the Emmitsburg road, preparing for
  the assault." → slackening ~14:45 nominal, infantry visible immediately
  after; his "about 1" clock sits within minutes of the 13:07 chain.
- **McGilvery** (OR 27/1 p. 884): "After the enemy had fired about one
  hour and a half, and expended at least 10,000 rounds of ammunition…"
  then "At about 3 p. m. a line of battle of about 3,000 or 4,000 men
  appeared, advancing directly upon our front, which was completely broken
  up and scattered by our fire before coming within musket range of our
  lines. Immediately after, appeared three extended lines of battle…" →
  the assault is crossing the field at ~15:00 on his clock.
- **Longstreet** (OR 27/2, NPS reproduction, verified in the R1 pass):
  signal guns "About 2 p.m." — the report-nominal outlier, excluded from
  anchors by ED-25 rule 4, kept as recorded.

### 2.5 Institutional adoptions (verified in the R1 pass, unchanged)

NPS Civil War Series heading "July 3, 1863, 3 P.M. — The Charge"; American
Battlefield Trust "lurched forward near 3 p.m."; ABT climax sheet
3:45–4:15 PM; Stone Sentinels July 3 timeline "3:30–4:00 p.m. … Armistead
was mortally wounded."

---

## 3. The two timestamped dispatches (OR 27/1) — BOTH VERIFIED

Access: ehistory.osu.edu reproduces OR Ser. 1 Vol. 27 Pt. 1 page-by-page
(Serial 043); it blocks automated fetches, so pages were read through
Wayback Machine captures of those pages; Buford's dispatch cross-checked
against the independent gdg.org transcription, which matches verbatim.

### 3.1 Buford to Meade, July 1, 1863, 10:10 a.m. — OR 27/1, p. 924

> "HEADQUARTERS FIRST CAVALRY DIVISION, / Gettysburg, July 1,
> 1863—10.10 a.m. / The enemy's force (A. P. Hill's) are advancing on me
> at this point, and driving my pickets and skirmishers very rapidly.
> There is also a large force at Heidlersburg that is driving my pickets
> at that point from that direction. General Reynolds is advancing, and is
> within 3 miles of this point with his leading division. I am positive
> that the whole of A. P. Hill's force is advancing. / JNO. BUFORD. /
> General MEADE, Commanding Army of the Potomac."

Timestamp "10.10 a.m." printed in the document header; forwarded by
Pleasonton (cover note on the same page). URLs:
http://web.archive.org/web/2023id_/https://ehistory.osu.edu/books/official-records/043/0924
(canonical https://ehistory.osu.edu/books/official-records/043/0924);
cross-check http://www.gdg.org/research/Other%20Documents/Overview/reynoldsarrives.htm.
Corroborated independently by Coddington's endnote (§1.1, p. 682).

### 3.2 Hancock to Meade, July 1, 1863, 5:25 p.m. — OR 27/1, p. 366

> "5.25 [P. M., JULY 1, 1863.] / GENERAL: When I arrived here an hour
> since, I found that our troops had given up the front of Gettysburg and
> the town. We have now taken up a position in the cemetery, and cannot
> [well] be taken. It is a position, however, easily turned. Slocum is now
> coming on the ground, and is taking position on the right… When night
> comes, it can retire; if not, we can fight here, as the ground appears
> not unfavorable with good troops. … WINF'D S. HANCOCK, Major-General,
> Commanding corps."

The "5.25" is printed; "[P. M., JULY 1, 1863.]" is supplied in the OR
editors' brackets (recorded caveat). "[well]" corrects an OCR slip
("will") in the ehistory text. "We can fight here" sentiment verbatim.
Note the internal cross-corroboration: "When I arrived here an hour since"
⇒ arrival ~16:25, vs his report's "At 3 p. m. I arrived" (§5.1) — the
dispatch supports the LATER arrival reading of CA-J1P-7. URL:
http://web.archive.org/web/2023id_/https://ehistory.osu.edu/books/official-records/043/0366.

---

## 4. The two suspect tablet extractions — BOTH RESOLVED

Access: gettysburg.stonesentinels.com tablet pages, fetched directly.

### 4.1 Early's Division tablet — "about noon" is real but is NOT an on-field arrival

https://gettysburg.stonesentinels.com/confederate-headquarters/earlys-division/ —
exact July 1 wording:

> "July 1. The Division arrived about noon within two miles of Gettysburg
> by Harrisburg Road. Formed line across road north of Rock Creek.
> Gordon's Brigade was ordered to support a brigade of Rodes' Division
> engaged with a division of the Eleventh Corps… and the remainder of the
> Division was ordered forward as Gordon's Brigade was engaged. After a
> short and severe contest the Union troops were forced through the town
> losing many prisoners."

"About noon" attaches to reaching a point **within two miles of Gettysburg
on the Harrisburg Road** — vicinity, not the field, not the attack. The
prior extraction that read it as an arrival/attack clock is corrected; the
tablet is fully consistent with the modern ~15:00 attack (CA-J1P-3
unchanged). The separate ANV itinerary tablet
(https://gettysburg.stonesentinels.com/other-monuments/army-of-northern-virginia-itinerary-tablets/)
has no time and no halt.

### 4.2 XI Corps tablet — "10.30 A. M." belongs to Schurz's division specifically

https://gettysburg.stonesentinels.com/union-headquarters/11th-corps/ —
exact July 1 wording:

> "Schurz's Division in advance arrived at 10.30 A. M. was formed in line
> northwest of the town. Barlow's Division formed on Schurz's right.
> Steinwehr's Division was placed on Cemetery Hill. The line in front was
> attacked by brigades of Rodes's and Early's Divisions. About 4 P. M. the
> Corps was forced back and retired through the town to Cemetery Hill and
> formed on each side of the Baltimore Pike."

"10.30 A. M." = Schurz's division (the advance) arriving — not the corps.
"About 4 P. M." = the corps forced back to Cemetery Hill (CA-J1P-4's
anchor quote, now verbatim). Note vs Howard's OR report (§5.2): Howard has
Schurz *joining him* "before 12 m." — the 10:30 tablet clock is early
against Howard's own account; both readings now recorded.

Bonus from the same host's July 1 timeline page
(https://gettysburg.stonesentinels.com/timeline-battle-july-1/), narrative
prose, not tablet inscription: Hancock "arrived at around 4:00", dispatch
to Meade "we can fight here" timestamped 5:25 p.m.

---

## 5. OR 27/1 Union report quotes — VERIFIED (access as §3)

Report page ranges per the Civil War Cycling OR index
(https://civilwarcycling.com/index/gettysburg-officer-reports-1/):
Hancock 366–378; Howard 696–712; Greene 855–860; McGilvery 881–884.

### 5.1 Hancock (II Corps)

- July 1 (p. 367–368): "A few minutes before 1 p. m., I received orders to
  proceed in person to the front…"; "**At 3 p. m. I arrived at Gettysburg
  and assumed the command.** At this time the First and Eleventh Corps
  were retiring through the town, closely pursued by the enemy…" (vs his
  own 17:25 dispatch's "arrived here an hour since" — the canonical
  Hancock-arrival controversy, both poles now primary-verified; CA-J1P-7.)
- July 3 (pp. 372–373): "From 11 a. m. until 1 p. m. there was an ominous
  stillness. **About 1 o'clock**, apparently by a given signal, the enemy
  opened upon our front with the heaviest artillery fire I have ever
  known…" / "**After an hour and forty-five minutes**, the fire of the
  enemy became less furious, and immediately their infantry was seen in
  the woods beyond the Emmitsburg road, preparing for the assault."
- Repulse (p. 374): "…after a few moments of desperate fighting the
  enemy's troops were repulsed… The battle-flags were ours and the
  victory was won."

### 5.2 Howard (XI Corps)

- p. 701: "At 3.30 a. m. July 1, orders were received… At 8 a. m. orders
  were received from him [Reynolds] directing the corps to march to
  Gettysburg."
- p. 702: Reynolds's death "**This was about 11.30 a. m.** … On hearing of
  the death of General Reynolds, I assumed command of the left wing…";
  "Here General Schurz joined me before 12 m."; "About 12.30 [p. m.]
  General Buford sent me word that the enemy was massing between the York
  and Harrisburg roads…"
- p. 704: "**About 4 p. m.** I sent word to General Doubleday that, if he
  could not hold out longer, he must fall back…" / "**At 4.10 p. m.**,
  finding that I could hold out no longer… I sent a positive order to the
  commanders of the First and Eleventh Corps to fall back gradually…" /
  "**At 4.30 p. m.** the columns reached Cemetery Hill… General Hancock
  came to me about this time…"

### 5.3 Greene (3rd Brig., 2nd Div., XII Corps)

- p. 856: "By 12 o'clock we had a good cover for the men. … Before any
  further movements could be made, we were attacked on the whole of our
  front by a large force **a few minutes before 7 p. m.**" (CA-J2E-2's
  19:00 start, now primary-verified.)
- p. 857: "About 10 o'clock I was informed that General Kane, with his
  brigade, was returning to his position…"

### 5.4 McGilvery (1st Volunteer Brigade, Artillery Reserve)

- July 2 (p. 881): "…at about 3.30 p. m. on July 2, I received an order…
  to report to General Sickles with one light 12-pounder and one rifled
  battery. The Fifth Massachusetts Battery, Captain Phillips, and Ninth
  Massachusetts Battery, Captain Bigelow, were marched immediately… I
  placed [Hart's battery] in position in a peach orchard…"
- July 3 (p. 884): "About one-half hour after the commencement, some
  general commanding the infantry line ordered three of the batteries to
  return the fire… After the enemy had fired about one hour and a half,
  and expended at least 10,000 rounds of ammunition… At about 3 p. m. a
  line of battle of about 3,000 or 4,000 men appeared, advancing directly
  upon our front, which was completely broken up and scattered by our fire
  before coming within musket range of our lines. Immediately after,
  appeared three extended lines of battle, of at least 35,000 men,
  advancing upon our center… by training the whole line of guns obliquely
  to the right, we had a raking fire through all three of these lines."
  (Caveat: his July 3 *opening-minute* clock sits on pp. 882–883, which
  have no Wayback capture — his stated start time is UNVERIFIED; duration
  and the 3 p.m. advance are verified from p. 884.)

---

## 6. Honesty ledger — what could NOT be verified

1. **Sears (2003) at the page level** (§1.4) — every anonymous channel
   exhausted; his position is recorded as attributed-only ("ended at
   2:30 p.m." per Wikipedia's duration footnote). Follow-up: a borrowed
   archive.org session or a physical copy closes it in minutes.
2. **McGilvery's July 3 cannonade start clock** (OR 27/1 pp. 882–883) — no
   Wayback capture; ehistory blocks direct fetch.
3. **George R. Stewart, *Pickett's Charge: A Microhistory* (1959)** — the
   deepest published timing analysis; archive.org copy is lending-only.
   Not required for the ruling; flagged as the highest-value future fetch
   on this question.
4. All OR quotes carry ehistory's OCR (read through Wayback), not page
   images; one OCR slip corrected and flagged (§3.2). Buford's dispatch is
   double-sourced (gdg.org matches verbatim).
5. Hess quotes from the Google Books EBOOK carry position ids (PT…), not
   print pages; print p. 162 for the step-off passage is taken from the
   Wikipedia citation and not independently confirmed (the passage text
   itself IS verified).

## 7. What §1–§2 add up to (consumed by the ED-1 revisit)

| Witness/scholar | Cannonade start | Duration | Step-off | Basis |
|---|---|---|---|---|
| Jacobs (1864, contemporaneous clock-reader) | **13:07** | ~83 min implied | **14:30** | clock readings, verified full text |
| Haskell (1863 letter, watch-checked) | ~13:00 (his watch; +7 vs chain) | "two hours" | after **15:00** end | watch checks, verified full text |
| Alexander (1907, retrospective watch) | 13:00 (his watch) | planned ≤ 1 h | "doubtless 1.50 or later" (self-hedged) | verified full text |
| Hancock (OR) | "about 1 o'clock" | 1 h 45 min to slackening | infantry seen immediately after (~14:45+) | verified OR text |
| McGilvery (OR) | (start unverified) | "about one hour and a half" | lines crossing "at about 3 p. m." | verified OR text |
| Longstreet (OR) | "about 2 p.m." (nominal) | — | — | verified (R1 pass) |
| Coddington 1968 | ~13:00 (p. 493) | two hours (p. 499) | **15:00** (p. 502) | verified pages |
| Hess 2001 | 13:07 | < 1 h | **~14:00** | verified pages/positions |
| Wert 2001 | 13:00 "or a few minutes after" (p. 166) | "about an hour" (p. 182) | **~14:05–14:15** | verified pages |
| Sears 2003 | (leans on Jacobs) | ended 14:30 (attributed only) | unverified | NOT page-verified |
| NPS / ABT | ~13:00–13:07 | — | **~15:00** | verified (R1 pass) |

The alleged modern **consensus** at 14:00–14:30 does not survive page-level
verification: it was an artifact of one Wikipedia footnote that misreports
Coddington. What exists is a genuine, honest **two-school split** —
short-cannonade (Hess, Wert, probably Sears; ultimately riding Alexander's
note chain and ammunition arithmetic) vs long-cannonade (Coddington, NPS,
ABT; riding Haskell's watch, Hancock's and McGilvery's durations) — with
Jacobs alone in the middle at 14:30.
