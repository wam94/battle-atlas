# July 3 Source Library — Pickett-Pettigrew-Trimble Assault (1:00–4:00 PM)

**Status:** Research pass 1 of 2 (deep-research workflow, 22 sources fetched, 108 claims extracted, 25 verified by 3-vote adversarial panels: 24 confirmed, 1 refuted). Deliverables 1 and 3 are strong; 4 is anchored but thin; **2 (order of battle) is unfulfilled and needs a dedicated follow-up pass.** Download URLs live-verified 2026-07-01; re-check at build time.

Provenance discipline for everything below: each claim carries its witness/source. Where sources disagree, the disagreement is recorded — it is data for the app's provenance system, not noise to reconcile away.

---

## 1. Map sources (verified, ready to use)

### Warren survey — the georeferencing terrain base
- **LOC item:** https://www.loc.gov/item/99448794/ — "Battle field of Gettysburg," surveyed 1868–69 under Bvt. Maj. Gen. G.K. Warren (ordered by A.A. Humphreys), field-revised by P.M. Blake 1873, published at 1:12,000 (reduction of the original 200-ft-to-the-inch survey; original in NARA RG 77:E 81). LOC dates the printed sheet "187?".
- **Downloads (unrestricted, live-verified HTTP 200):**
  - Master TIFF (~262 MB): https://tile.loc.gov/storage-services/master/gmd/gmd382/g3824/g3824g/cw0353500Z.tif
  - JPEG2000 (8878×9845 px, ~18.9 MB): https://tile.loc.gov/storage-services/service/gmd/gmd382/g3824/g3824g/cw0353500.jp2
  - IIIF service: https://tile.loc.gov/image-services/iiif/service:gmd:gmd382:g3824:g3824g:cw0353500/info.json
- **Critical note (verbatim from LOC): "Positions of troops are not given."** The Warren map is the terrain/roads/fences/vegetation base layer — it cannot source brigade positions. It also names residents on farm buildings, which will matter for landmark georeferencing.

### Bachelder day-by-day troop-position set — the primary July 3 position source
- **LOC item:** https://www.loc.gov/item/99447492/ — "Map of the battlefield of Gettysburg. July 1st, 2nd, 3rd, 1863." Three colored sheets, 74×71 cm, 1:12,000, **drawn on the Warren base** (so it georeferences the same way). **Cite as: "positions compiled by John B. Bachelder, 1876; new edition 1883"** (the 1883 date is a bibliographic attribution — LC Civil War Maps 2nd ed. no. 326 — not printed on the sheet).
- **Sheet 3 = July 3** (visually confirmed: "THIRD DAY'S BATTLE / Positions of Troops compiled by John B. Bachelder"): viewer https://www.loc.gov/resource/g3824g.cw0326000c/ · master TIFF (266,714,848 bytes): https://tile.loc.gov/storage-services/master/gmd/gmd382/g3824/g3824g/cw0326000cZ.tif · JP2 ~8.5k×10.5k px (~21–23 MB).
- ⚠️ **Refuted claim (1-2 vote), do not repeat:** "one troop-position map per day" overstates the sheets' internal time granularity. Describe them as three day-labeled sheets. Bachelder's true hour-by-hour materials (1880s GBMA/War Department work) were **not located in digitized form** in this pass — open question below.

### Bachelder isometric bird's-eye view — brigade positions with names, NOT georeferenceable
- **LOC item:** https://www.loc.gov/item/2007630423/ — copyright **1863** (not 1876 as we assumed going in). Per LOC: "The locations of the corps, divisions, brigades, etc. of both armies, with the names of commanding officers, are given in detail," with facsimile endorsements from Meade, Hancock, Howard, Hunt.
- Downloads: JP2 10861×6469 (~16.8 MB): https://tile.loc.gov/storage-services/service/gmd/gmd382/g3824/g3824g/cw0324000.jp2 · TIFF (~210.8 MB): https://tile.loc.gov/storage-services/master/gmd/gmd382/g3824/g3824g/cw0324000.tif
- **Use as a cross-check for unit identity/adjacency only:** it is a perspective view compositing all three days — geometrically distorted, unsuitable for the two-point georeferencing workflow.

### Download mechanics (programmatic)
loc.gov HTML pages return 403 to non-browser clients (Cloudflare); use the JSON API (`?fo=json`), IIIF endpoints, or tile.loc.gov directly — all serve unauthenticated with CORS `*`.

### Supporting corpus (found, not yet mined)
- **The Bachelder Papers** (Ladd & Ladd transcription of his veteran correspondence — the raw evidence behind his maps): https://archive.org/details/bachelderpapersg0001unse
- American Battlefield Trust modern map sets for the assault window: https://www.battlefields.org/learn/maps/gettysburg-picketts-charge-july-3-1863-330-345-pm and .../345-400-pm

---

## 2. Order of battle — **GAP: follow-up pass required**

No OOB claims survived adversarial verification in this pass. **Do not author strengths from what's below** — these are only the verified fragments that fell out of other deliverables:

- Pickett's three brigades named: Garnett, Kemper, Armistead (Jacobs, 1864, verbatim).
- Webb's regiments at the Angle: 69th, 71st, 72nd Pennsylvania (Haskell).
- II Corps battery commanders killed/mortally wounded: Cushing (Btry A, 4th US — the Angle), Woodruff (Btry I, 1st US — Ziegler's Grove, *not* the Angle proper), Rorty (Btry B, 1st NY) (Haskell).
- Assault column strength: Haskell's period estimate **18,000** ("a sloping forest of flashing steel") vs. modern scholarship **~10,500 (Stewart 1957) to ~12,500–13,000 (Hess 2001, ABT, Encyclopedia Virginia)**. Store Haskell's figure as a flagged period overestimate.
- Bachelder's sheets/isometric provide map-derived brigade identity + position cross-checks.

**Follow-up targets:** Official Records Series I Vol. 27 (returns and reports) and Busey & Martin, *Regimental Strengths and Losses at Gettysburg* — locate free full texts, extract per-brigade engaged strengths for both sides, and record where they disagree (they will, especially for Pettigrew's and Trimble's commands).

---

## 3. Timeline 1:00–4:00 PM — verified, with the disagreements that ARE the data

All quotes verified verbatim against public-domain full texts.

| Event | Witness times (each tagged to its source) |
|---|---|
| Bombardment opens | **1:00 PM** — Alexander: "It was just 1 P.M. by my watch when the signal guns were fired" · **~1:00 PM** — Haskell: watch-checked "five minutes before one o'clock," then "the distinct sharp sound of one of the enemy's guns" · **1:07 PM** — Jacobs: "At seven minutes past 1 p.m., the awful and portentous silence was broken" |
| Signal guns | Plural (Alexander/Confederate) vs. one perceived gun (Haskell, hedged) |
| Alexander's 1st note to Pickett | **1:25 PM** ("still 18 guns firing from the cemetery") |
| Union slackening / "ruse" | Hunt's deliberate ammunition-conserving slackening (his Battles & Leaders account) — but some II Corps guns near the copse were genuinely withdrawn for damage; Alexander: "incorrectly told it was the cemetery." Not purely a ruse: encode as mixed. |
| Alexander's 2nd note | **1:40 PM** ("For God's sake come quick. The 18 guns have gone…") |
| Longstreet–Pickett exchange | Pickett: "General, shall I advance?" — Longstreet "turned in his saddle and looked away" — Pickett: "I am going to move forward, sir." (Alexander, *hearsay one step removed* — he wrote "I afterward learned what had followed." Flag it.) |
| Step-off | **shortly after ~1:50 PM** (Alexander: "doubtless 1.50 or later, but I did not look at my watch again") · **2:30 PM** (Jacobs: "When 2½ p.m. came… two long, dark, massive lines") · **~3:00 PM implied** (Haskell: cannonade ended "At three o'clock almost precisely"; ABT: "about 3:00 p.m.") · Official reports widen further (Smyth: cannonade *start* 2:00 PM) |
| Cannonade duration | Haskell: "two mortal hours"; his mid-bombardment marker "Half-past two o'clock, an hour and a half since the commencement" |

**Encoding guidance:** bombardment start 1:00–1:07 (three watches, 0–7 min spread — remarkably tight); step-off attested anywhere **~1:50 to ~3:00 PM** depending on witness (a full hour of honest disagreement). Store each time with its witness; never a reconciled "true" time. Not yet verified: times for Stannard's flank maneuver and the final repulse (open question).

**Witness bias notes (verified):** Alexander = 44-year retrospective (1907), times self-reported; Haskell = weeks-after letter but famously partisan (72nd PA veterans disputed his Angle characterization); Jacobs = contemporaneous (1864) and habitually precise, but a distant civilian observer (his "150 guns on each side" overstates the initial Union reply of ~80 under Hunt's conservation order).

---

## 4. First-person accounts — 3 verified anchors; need 4–8 more

### Verified with free public-domain full texts

**Frank A. Haskell** (Union 1st Lt., aide to Gen. Gibbon, on Cemetery Ridge) — *The Battle of Gettysburg*, Wisconsin History Commission Reprints No. 1 (1908). Full text: Project Gutenberg #33121 — HTML https://www.gutenberg.org/files/33121/33121-h/33121-h.htm · plain text https://www.gutenberg.org/cache/epub/33121/pg33121.txt (note: the `/files/33121/33121-0.txt` path 404s). Verified vignette passages: the watch-check and signal gun; "the arms of eighteen thousand men, barrel and bayonet, gleam in the sun, a sloping forest of flashing steel"; "Cushing, firing almost his last canister, had dropped dead among his guns shot through the head by a bullet" (Fuger's account says the bullet entered his mouth — competing detail, flag it); the Angle crisis — "one [rebel flag] was already waving over one of the guns of the dead Cushing."

**E.P. Alexander** (Confederate colonel directing the assault artillery on Longstreet's front — NOT "Lee's artillery chief"; Pendleton held that post) — *Military Memoirs of a Confederate* (Scribner's, 1907), ch. 18 "Gettysburg: Third Day." Full texts: Internet Archive https://archive.org/details/militarymemoirso00alex (best for scripted retrieval) · Perseus https://www.perseus.tufts.edu/hopper/text?doc=Perseus:text:2001.05.0130:chapter%3D18 · Google Books id kuopAAAAYAAJ (human-browser only; CAPTCHA blocks scripts). Verified passages: the 1:00 PM signal, both notes to Pickett, the Longstreet–Pickett exchange, Longstreet to Alexander: "I do not want to make this charge… I would not make it now but that Gen. Lee has ordered it and is expecting it."

**Michael Jacobs** (civilian, professor at Pennsylvania College, Gettysburg — the most contemporaneous source, 1864) — *Notes on the Rebel Invasion of Maryland and Pennsylvania* (Lippincott, 1864). Full text: https://archive.org/details/notesonrebelinva00jaco. Verified passages: the 1:07 PM start; the 2:30 PM advance naming Garnett, Kemper, Armistead "in two long, dark, massive lines." (OCR renders "2½ p.m." as "2J p.m.")

### Follow-up targets for the remaining 4–8 voices
Rawley Martin and J.H. Lewis (Armistead's breakthrough), Frederick Fuger (Cushing's battery — also resolves the Cushing death-wound discrepancy), a Vermont Brigade account for Stannard's flank attack (e.g., Wheelock Veazey), Birkett Fry or J.B. Smith (Pettigrew's front), a Union enlisted voice, plus the Southern Historical Society Papers index at http://www.gdg.org/research/SHSP/shsp.html.

---

## Open questions (feed research pass 2)

1. Free full-text access to OR Vol. 27 returns and Busey & Martin per-brigade engaged strengths — and how much they disagree for Pettigrew/Trimble.
2. Do Bachelder's more granular 1880s position maps (GBMA/War Department era) exist digitized anywhere (LOC/NARA/GNMP)?
3. Verifiable public-domain accounts for Armistead's breakthrough, Cushing's battery, and Stannard's flank movement (names above).
4. Best-attested times for Stannard's maneuver and the final repulse — the back half of our window is currently time-poor.

## Workflow stats
5 search angles · 22 sources fetched · 108 claims extracted · 25 adversarially verified (24 confirmed 3-0, 1 refuted 1-2) · 104 agents.
