# Full-Cast Sources — Every Organized Body on the Gettysburg Square, July 3, 13:00–16:00

**Date:** 2026-07-02. **Question:** what does it take to author THE FULL CAST of the July 3 afternoon slice — every organized body on or near the 8,507 m battlefield square — at brigade grain (infantry/cavalry) and battery/battalion grain (artillery), beyond the 65 assault-sector units already in `app/Assets/Battle/gettysburg-july3.json`?

**Builds on (does not re-survey):** `docs/research/2026-07-02-regiment-track-sources.md`, `docs/research/2026-07-02-bachelder-timed-set-acquisition.md`, `docs/research/2026-06-13-oob-strengths.md`.

**Method:** (a) first-hand full-resolution IIIF reads of Bachelder timed sheet No. 8 (1–5 PM July 3) across every non-assault sector — nine verification crops saved alongside this report; (b) computed on-map/off-map geometry for ~40 landmarks from the terrain square's UTM origin; (c) three parallel web-research passes (Union OOB, CSA OOB, cavalry/trains/hospitals) with per-claim URLs, live-fetched this session. **VERIFIED** = fetched/inspected this pass; flags mark everything weaker.

---

## 0. Headline findings

1. **The whole-field authoring problem is source-SOLVED at position level.** Bachelder sheet No. 8 ("No. 8. 1-5 P.M. July 3", Rumsey list 12440.022) draws BOTH armies across the ENTIRE square at unit level *inside our exact window* — verified by eye this session in seven sectors (§2.1). Where the sheet is label-sparse (Cemetery Hill batteries), the War Department tablets (via Stone Sentinels) carry per-unit July 3 position AND activity text, including explicit negatives ("Not engaged," "Remained in same position") — which is exactly what position-only authoring needs.
2. **Almost the entire full cast is STATIC in 13:00–16:00.** The Culp's Hill fight ended 10:30–11:00 (both sides' tablets agree); Farnsworth's charge and the 15th GA fight are ~17:00+; East Cavalry Field is off-map. Inside the window the only movers on the square are: the assault column (modeled); ~8 Union batteries rotating into the crest line; Robinson's two I Corps brigades (3 PM) and Shaler's brigade (~3:30 PM); Wright's brief advance-and-recall; the Artillery Reserve park + ammunition train displacing behind Little Round Top; Meade's HQ (Leister→Powers Hill and back); and the South Cavalry Field deployments. **Position-only authoring with attested roles is historically honest for ~90% of the new cast.**
3. **The artillery is the best-attested, highest-value tranche** — and it's where the smoke system should re-anchor. Every gun group that fired in-window has tablet- or OR-grade attestation; so do the famous *silences* (McGilvery holding fire under Hunt's policy, Osborn's deliberate cease-fire ruse, Ewell's near-idle wing, Garnett's battalion in reserve, XII Corps' 26 guns silent after 10:30).
4. **Scale: ~120–140 new units at the directed grain** (§5): 45 Union infantry brigades, 26 CSA infantry brigades, ~30 core + ~8 optional Union batteries, 15 CSA artillery battalions, ~6–8 cavalry-sector units at the south edge, ~8–10 HQ/hospital/train markers. Off-map bodies (East Cavalry Field's 7 brigades + horse artillery; Fairfield; Westminster; Cashtown) should carry explicit OFF-MAP flags, not silent omission.

---

## 1. The square — computed on-map/off-map geometry

Terrain square: battlefield-local meters 0–8507 both axes; origin UTM 18N 304208 E / 4404534 N; town square computes to local (4864, 6839) (matches the brief's ~(4880,6800)). Landmark conversions computed this session (WGS84→UTM):

| Landmark | local (x,y) m | Status |
|---|---|---|
| Gettysburg town square | 4864, 6839 | ON (1.7 km from N edge) |
| Cemetery Hill / East Cemetery Hill | 4993,5656 / 5107,5753 | ON |
| Culp's Hill summit / Spangler's Spring | 5796,5570 / 6084,4752 | ON |
| Benner's Hill / Wolf Hill | 6619,6305 / 7287,5622 | ON |
| **Brinkerhoff's Ridge (Hanover Rd)** | 8095,7135 | **ON — 412 m from E edge** |
| **Rummel farm (East Cavalry Field)** | 10431,6356 | **OFF — 1.92 km beyond E edge** |
| Cress Ridge / Hanover–Low Dutch intersection | 10191,7050 / ~10.5 km E | OFF |
| The Angle / Ziegler's Grove | 4416,4827 / 4509,5146 | ON |
| Lutheran Seminary / Oak Hill | 3722,6888 / 4175,8209 | ON (Oak Hill 298 m from N edge) |
| Peach Orchard / Trostle / Weikert | 3208,3457 / 3824,3797 / 4112,3646 | ON |
| Devil's Den / Little RT / Big RT | 3808,2465 / 4296,2441 / 4076,1869 | ON |
| **Bushman Hill / Slyder farm (Farnsworth ground)** | 3284,1367 / 3448,1108 | **ON — 1.1–1.4 km inside S edge** |
| **Currens farm, Emmitsburg Rd (Merritt)** | 2733,−96 | **EDGE — straddles the S boundary** |
| Ridge Rd/Emmitsburg Rd (Merritt's deeper line) | 2373,−754 | OFF (just south) |
| Powers Hill / Granite Schoolhouse | 5982,4077 / 6433,3966 | ON |
| McAllister's Mill (Balt. Pike @ Rock Cr.) | 6683,3671 | ON |
| White Run trains/hospital area | ~7599,2615 | ON (SE corner, 0.9 km margin) |
| George Spangler farm (XI Corps hosp.) | 5472,3234 | ON |
| Meade HQ (Leister) | 4763,5007 | ON |
| Pitzer's Woods / Warfield Ridge | 2673,4348 / 2963,2231 | ON |
| Black Horse Tavern (CSA hosp.) | 196,4633 | ON — 196 m from W edge |
| Two Taverns / Hunterstown / Cashtown / Fairfield / Westminster | — | all OFF by 2.3–40 km |

**Rulings enforced:** East Cavalry Field is OFF-MAP — the Gregg–Stuart fight cannot be placed (closest attested element ~4 km east; fight core 5+ km; sources disagree 2.5–4 mi but every figure clears the 3.6 km edge — cavalry survey, VERIFIED). The only ECF-related on-map touches: Stuart's *morning* York Pike transit (pre-window) and possibly J. Irvin Gregg's westernmost Hanover Road pickets near Brinkerhoff's Ridge; Neill's VI Corps brigade "making connection with the Cavalry pickets" east of Rock Creek is the on-map end of that screen (tablet, VERIFIED). Farnsworth's brigade + Elder's battery are ON-MAP; Merritt's brigade fights ON the southern edge (Currens farm anchor 96 m off; his line laps both sides). Union trains (Westminster) and CSA trains (Cashtown) are OFF-MAP; placeable logistics: Powers Hill node, Artillery Reserve park, Meade/Lee HQs, Baltimore Pike hospitals, Black Horse Tavern.

---

## 2. Source backbone for the full cast

### 2.1 Bachelder timed sheet No. 8 — first-hand whole-field verification (this session)

Nine crops fetched from `davidrumsey.com/luna/servlet/iiif/RUMSEY~8~1~299035~90070134` (HTTP 200; note: the service **400s on upscale requests** — request width ≤ region width). Read by eye:

- **Culp's Hill (`s8_culps.jpg`)** — Union brigades named in place (Greene's Brig, Geary's Div, Candy Brig, Lockwood Brig, McDougall Brig, Shaler, Ruger's Division, Colgrove Brig, Neill's at lower left, Wadsworth on the hill); **Johnson's Division drawn withdrawn east of Rock Creek** with the label block "Jones, Williams, Steuart, Walker's Brigades, Smith's, O'Neal's, Daniels Brigades / Johnson's Div." — the single strongest map verification of the afternoon disposition (matches the division tablet exactly, §3.1).
- **Cemetery Hill (`s8_cemhill.jpg`, `s8_cemhill_zoom.jpg`)** — Steinwehr's 11 Corps, Schurz Div., von Gilsa, Carroll Brig., Wadsworth, McKnight (Stevens Knoll); batteries drawn as massed gun symbols, labels sparse — tablet texts carry the battery names here (§3.3).
- **Round Tops (`s8_roundtops.jpg`)** — Weed's Brigade (Garrard), Tilton's, Fisher's, Day, Ayres Div., Grant's Brig. at J. Weikert, Walcott's battery; CSA: Benning Ga., Robertson, Law Ala.
- **McGilvery/fishhook south (`s8_mcgilvery.jpg`)** — "McGilvery's Reserve Artillery Brigade" with a written battery list (read: Daniels Mich. / Thomas / Thompson Pa / Phillips 5 Ms. / Hart N.Y. / Sterling / Rank(?"Rock") / Cooper / Dow / Ames); "Artillery Reserve" park label east of Weikert; Caldwell's Cross/Brooke/Zook bars; III Corps Carr/Brewster/Burling/Madill/De Trobriand/Berdan "3 Corps"; VI Corps Torbert's Brig., Nevin, Vincent (Rice). ⚠️ Two of the sheet's McGilvery labels conflict with the OOB record: **Daniels's 9th Michigan and Rank's PA section are placed at East Cavalry Field by the OOB/cavalry sources** (Wikipedia Union OOB; Rank's section on the Hanover Rd per the Gregg HQ marker). Keep as a sheet-vs-OOB disagreement; the other eight labels are corroborated by tablets (§3.3).
- **Town/north (`s8_town.jpg`)** — Doles' Ga., Iverson's N.C. in Long Lane SW of town.
- **Peach Orchard/Emmitsburg (`s8_peachorchard.jpg`)** — Pickett step-off, Wilcox Ala. (two positions + arrows), Perry's Fla., **Alexander's Bat., Cabell's Bat., Dearing's named batteries (Blount, Caskie, Macon)**, Kershaw S.C., McCandless Brigade.
- **Seminary Ridge N (`s8_semridge_n.jpg`)** — McIntosh's Batt. with battery names (Johnson, Hurt, Wallace…), Pegram's batteries (Marye, Zimmerman, Brander…), Lane's Ga. batteries (Ross, Wingfield), McGowan S.C., Thomas Ga., Pender's Division, Ramseur N.C., Iverson N.C., Lane N.C., Heth's Div. Pettigrew Commanding.
- **South margin (`s8_southcav.jpg`)** — **Kilpatrick's Division at regiment level:** Farnsworth's Brigade (1st W.Va, 1st Vt, 5 N.Y., 18 Pa), "Elder's Bat. E 4 U.S.", Graham; Merritt: 6th Pa, 1 U.S., 2 U.S. at W. M. Currans — drawn at the sheet's own neat line, agreeing with our computed edge ruling.

Rights/consumption: see the acquisition report (CC BY-NC-SA scan; underlying work arguably §303(a); **facts extraction is clean regardless** — trace geometry onto the PD 1876 sheet / Warren base).

### 2.2 The rest of the stack

- **Bachelder 1876/1883 Third-Day sheet** — PD-clean whole-field regiment/battery geometry cross-check (regiment-track survey §1.1).
- **War Department tablets via Stone Sentinels** — per-division/brigade/battalion July 3 position + activity text, including documented negatives. URL patterns `/union-headquarters/…`, `/confederate-headquarters/…`. This is the attestation backbone for every static unit below.
- **OR 27/1–27/2** for firing reports (Wheeler's report transcribed at 13nybattery.com — VERIFIED; **McGilvery's own OR report not found online** — flagged; OR Part 1 access remains the standing open item).
- **Sheet No. 9 (5 PM)** as end-state anchor for late movers.

---

## 3. THE FULL CAST — sector by sector

Grades: **A** = documented position + attested in-window activity (firing/movement); **B** = documented position + attested static/negative; **C** = position inferred. All URLs were live this session.

### 3.1 Confederate infantry, non-assault — 26 brigades

Acting commanders July 3 PM: Hood's div under **Law** (Law's bde under Col. Sheffield); Semmes's under Col. Bryan; Barksdale's under Col. B.G. Humphreys; Hoke's under Col. Godwin; Jones's (VA) under Lt. Col. Dungan; McGowan's under Col. Perrin; Anderson's GA under Lt. Col. Luffman.

**Longstreet (8):** Law/Sheffield (W slope Big RT), Robertson (Devil's Den–Rose Woods; ⚠️ command July 3 disputed, Robertson vs Lt. Col. Work), Anderson/Luffman (Rose farm; elements shifted S vs Merritt ~15:00+ — **B+**), Benning (Houck's Ridge), Kershaw (Peach Orchard, extended right to Hood's left **at ~13:00** — one attested in-window move, **B+**), Semmes/Bryan, Barksdale/Humphreys, Wofford — all otherwise static, skirmish fire only; McLaws's tablet: "with the exception of severe skirmishing the Division was not engaged"; his withdrawal to the Warfield line is **post-window (evening)**. Grade B. Sources: https://gettysburg.stonesentinels.com/confederate-headquarters/hoods-division/ · .../mclaws-division/ · .../kershaws-brigade/ · HMDB m=10117, m=12252. Sheet 8 draws Law/Robertson/Benning/Kershaw in place.

**Hill (5):** Wright (Seminary Ridge close support; brief forward move ~15:30–16:00, recalled — **B+**), Posey and Mahone (trench line behind the crest, McMillan Woods sector — B), **Thomas** and **McGowan/Perrin** in **Long Lane** — tablets: "engaged most of the day in severe skirmishing and exposed to heavy artillery fire" / "constantly engaged in skirmishing" — **A−, the two attested in-window skirmish emitters of the CSA line**. Sources: .../thomas-brigade/ · .../perrins-brigade/ · .../andersons-division/ · npshistory.com/series/symposia/gettysburg_seminars/7/essay6.pdf.

**Ewell (13):**
- **Johnson + attached Daniel, O'Neal, Smith (7 brigades: Steuart, Nicholls/Williams, Jones/Dungan, Stonewall/Walker, Daniel, O'Neal, Smith):** tablet "Retired at 10:30 A.M. to former position of July 2" — east of Rock Creek at the base of Benner's Hill/Daniel Lady farm — "held until 10 P.M." (⚠️ Wikipedia's Culp's Hill: "until midnight"; both post-window). Sheet 8 draws exactly this. **B+** (documented position AND documented negative).
- **Early (3 here):** Hays (southern town streets facing Cemetery Hill), Hoke/Godwin (beside Hays), Gordon (Winebrenner's Run) — "Division not further engaged"; house-sharpshooting all window (the fire Carroll's tablet complains of). B.
- **Rodes (3 here):** Doles, Iverson, Ramseur in **Long Lane** ("remained through the third day"), under Union artillery fire during the cannonade. **B+.**
Sources: .../johnsons-division/ · .../earlys-division/ · .../rodes-division/ · .../daniels-brigade/ · .../smiths-brigade/ · https://en.wikipedia.org/wiki/Culp%27s_Hill.

### 3.2 Confederate artillery — 15 battalions (~240 guns present; ~150–170 fired)

Signal guns 13:07 by Miller's battery, **Eshleman's** Washington Artillery, Peach Orchard; general fire to ~14:45–15:00. Tablet-grade verdicts (Stone Sentinels battalion pages, all VERIFIED):

| Battalion (corps) | Cdr | Guns | Position | Fired in window? | Grade |
|---|---|---|---|---|---|
| Alexander's (I) | Alexander/Huger | ~24–26 | Peach Orchard → NE corner Spangler's Woods | **YES — core of cannonade**; "come quick" notes to Pickett | **A** |
| Cabell's (I) | Cabell | 16 | Emmitsburg Rd @ Peach Orchard/Wheatfield Rd | **YES** | **A** |
| Dearing's (I) | Dearing | 18 | Emmitsburg Rd ridge fronting Pickett | **YES** — "fired effectively by battery during the cannonade" | **A** |
| Eshleman's Washington Arty (I) | Eshleman | 10 | Peach Orchard | **YES + the 13:07 signal guns** | **A** |
| Henry's (I) | Henry | 19 | Warfield Ridge/Bushman knolls facing S/SE | **PARTIAL/CONTESTED** — flank cover per Alexander; Bachman & Reilly swung S vs Merritt ~15:00+ | A− (flag) |
| Pegram's (III) | Pegram | 20 | Seminary Ridge S of Fairfield Rd | **YES** (sheet 8 names Marye/Zimmerman/Brander…) | **A** |
| McIntosh's (III) | McIntosh | 16 | Schultz Woods | **YES** (sheet 8 names batteries) | **A** |
| Garnett's (III) | J.J. Garnett | 15 | near McMillan Woods | **MOSTLY NO — ⚠️** tablet "in reserve, not actively engaged" vs secondaries crediting his rifles. Weight the tablet. | B (flag) |
| Poague's (III) | Poague | 16 | behind Pettigrew, Spangler/McMillan Woods | **YES — 10 guns engaged; 6 howitzers sheltered, "no active part"** (documented internal split) | **A** |
| Lane's Sumter GA (III) | John Lane | 17 | Anderson's front, Seminary Ridge | **YES** — 1,082 rounds July 2–3; sheet 8 names Ross/Wingfield | **A** |
| Dance's 1st VA (II) | Dance | 20 | Seminary Ridge near the Seminary/railroad cut | **YES — Griffin's bty "took part in the cannonade"** | **A** |
| Nelson's (II) | Nelson | 11 | Benner's Hill area | **LIMITED — "opened about 12 M., firing 20 to 25 rounds"** then silent | A− |
| Carter's (II) | Carter | 16 | Napoleons Oak Hill; **rifles moved to Seminary Ridge near the railroad cut, "took part in the great cannonade"** | **YES (rifles); Oak Hill Napoleons largely silent** | **A** (split) |
| Jones's (II) | H.P. Jones | 16 | N of town | **NO — "Not actively engaged."** | B |
| Andrews's/Latimer's (II) | Raine | 16 | 20-pdr section ~½ mi N of Benner's Hill; rest in reserve | **PARTIAL — the 20-pdr Parrotts fired in the cannonade** | A− |

URLs: `gettysburg.stonesentinels.com/confederate-headquarters/{alexanders-battalion, cabells-artillery-battalion, dearings-battalion, eshlemans-artillery-battalion, henrys-artillery-battalion, pegrams-artillery-battalion, mcintoshs-artillery-battalion, garnetts-artillery-battalion, poagues-artillery-battalion, lanes-artillery-battalion, dances-artillery-battalion, nelsons-artillery-battalion, carters-artillery-battalion, jones-artillery-battalion, latimers-artillery-battalion}/`.

⚠️ **Ewell's-wing disagreement to encode:** popular "two dozen enfilading guns from the northeast" vs tablet evidence of only **~10–14 Second Corps guns actually firing** (Griffin's, Carter's rifles, two 20-pdrs, Nelson's 20–25 rounds). The near-silence of Ewell's wing and Garnett's idle battalion are documented features — show guns present-but-silent rather than paving over.

### 3.3 Union — 45 infantry brigades + artillery (tablets VERIFIED per URL)

**I Corps (6 new brigades).** Meredith's Iron Bde (Col. Wm. Robinson) & Cutler — Culp's Hill trenches beside XII Corps, static, morning fight only (`gettysburg.stonesentinels.com/union-headquarters/1st-division-1st-corps/`). **Robinson's division (Coulter's & Baxter's bdes) — the Union's attested in-window infantry movers:** "At daylight moved to the support of batteries on Cemetery Hill. At 9 A.M. sent to the support of Twelfth Corps and **at 3 P.M. took position on the right of Second Corps** and remained until the close of the battle" (`.../2nd-division-first-corps/`) — grade **A−** (documented move at a documented clock inside the window; musketry not attested — flag). Doubleday's two non-Stannard bdes (Biddle/Gates's, Dana's) on Cemetery Ridge left of Gibbon — "assisted in repelling Longstreet's assault… taking many prisoners" / "capturing many prisoners and three stand of colors" — **A−** for repulse participation (`.../1st-brigade-3rd-division-1st-corps/`, `.../3rd-division-1st-corps/`).

**II Corps remainder (5).** Caldwell's four brigades (McKeen for Cross, Kelly, Fraser for Zook, Brooke) — south end of the II Corps front near the Pennsylvania-Memorial ground: "Constructed breastworks early in the morning which gave protection from the cannonade in the afternoon. Remained in position until the close of the battle" — B+ static under the cannonade (`.../1st-division-2nd-corps/`, `.../1st-brigade-1st-division-2nd-corps/`). Carroll's brigade (minus the modeled 8th OH) on East Cemetery Hill supporting the batteries, under "annoying sharpshooters fire from the houses in the town and a cross fire from artillery" — B+ (`.../1st-brigade-3rd-division-2nd-corps/`).

**III Corps (6).** Both divisions in army reserve behind the left-centre: Birney's (Madill for Graham, Berdan for Ward, de Trobriand) "held in reserve and detachments moved to threatened points"; Humphreys's (Carr, Brewster, Burling) moved at sunrise to the rear-left, resupplied, then "in rear of the First Second Fifth and some Sixth Corps in support of threatened positions" — B (which detachments moved is unattested — flag) (`.../1st-division-3rd-corps/`, `.../2nd-division-3rd-corps/`).

**V Corps (8).** Tilton (relieved Rice on the Round Top line July 3 AM), Sweitzer & Rice north of LRT, Day & Burbank ("Remained in same position", woods behind LRT), Garrard for Weed (LRT summit line), McCandless (advanced Plum Run line — his Wheatfield advance is ~5 PM, **post-window**), Fisher (Big RT breastworks) — all **B** static (`.../1st-division-5th-corps/`, `.../2nd-division-5th-corps/`, `.../3rd-division-5th-corps/`).

**VI Corps (8).** Torbert (SE of the Weikert House, "Not engaged except on the skirmish line"), Bartlett + Nevin's bde under him (advanced Plum Run line; the Benning/15th GA fight is post-window), Russell (E slope Big RT; "late in the afternoon" to the left-centre — straddles 16:00, flag), Grant's Vermonters (right on E slope Round Top, **left on the Taneytown Road**, "under no fire except that from artillery"), **Neill (across Rock Creek on the army's extreme right, Wolf Hill sector, "making connection with the Cavalry pickets," skirmishing attested — B+/A−)**, **Shaler (morning Culp's Hill reserve; ~3:30 PM sent to the army centre — in-window mover, A−)**, Eustis (right-centre reserve under Newton, "Not engaged but subject to artillery fire," 69 casualties) — tablets at `.../{1st,2nd,3rd}-brigade-1st-division-6th-corps/`, `.../2nd-brigade-2nd-division-6th-corps/`, `.../3rd-brigade-2nd-division-6th-corps/`, `.../1st-brigade-3rd-division/`, `.../2nd-brigade-3rd-division/`, `.../3rd-brigade-3rd-division/`, https://en.wikipedia.org/wiki/Alexander_Shaler.

**XI Corps (6).** von Gilsa, Harris (for Ames, at division), Coster, Smith, von Amsberg (for Schimmelfennig), Krzyżanowski — Cemetery Hill/East Cemetery Hill, all static; division tablets: "At 1 P.M. heavy cannonade opened and continued with considerable effect for an hour and a half followed by a charge on the Second Corps"; "Not engaged but subject to the fire of sharpshooters and artillery"; "Not engaged except skirmishing" (`.../1st-division-11th-corps/`, `.../2nd-division-11th-corps/`, `.../3rd-division-11th-corps/`). B+ (documented negative under fire).

**XII Corps (6).** McDougall, Colgrove, Lockwood (Ruger's div), Candy, Kane/Cobham, Greene (Geary's div) — Culp's Hill works; the fight ran "until 10:30 A.M. when the Confederate forces retired" / "a contest of seven hours" from 3 A.M. — static and silent in-window. B+ (`.../1st-division-12th-corps/`, `.../2nd-division-12th-corps/`).

**Union artillery — the tranche that carries the window:**

- **McGilvery's massed line, lower Cemetery Ridge (~39 guns; largely silent during the cannonade per Hunt's policy, then enfiladed Pickett/Kemper):** Thompson's C&F PA ("on line with Battery K 4th U.S. on right and Hart's Battery on left"), James's K/4th US (Seeley's, III Corps), Hart's 15th NY ("on Second Corps line south of Pleasonton Avenue"), **Phillips's 5th MA — the best single firing attestation: "About 1:30 by order of Brig. Gen. H.J. Hunt fired on the Confederate batteries… Opened an enfilading fire soon after on Longstreet's advancing line of infantry and assisted in repulsing the assault," then smashed the Florida brigade's follow-up** (`gettysburg.stonesentinels.com/union-monuments/massachusetts/5th-massachusetts-battery/`), Sterling's 2nd CT (position attested; in-window fire not battery-specific — flag), Dow's 6th ME, Ames's G/1st NY, Thomas's C/4th US, plus **Cooper's B/1st PA arriving at 3 PM from East Cemetery Hill "during a heavy cannonade" and going straight into case-then-canister** (`.../pennsylvania-artillery/battery-b-1st-pennsylvania-artillery/` — a documented in-window battery track). Grade **A** for the line. Tablet URLs: `.../union-headquarters/1st-volunteer-brigade-artillery-reserve/`, `.../4th-volunteer-brigade-artillery-reserve/`, `.../1st-regular-brigade-artillery-reserve/`. ⚠️ McGilvery's own OR report not found online; "39 guns" is secondary-grade (padresteve.com Hunt essay; Wikipedia Pickett's Charge: only McGilvery, Osborn, Rittenhouse fired on the advance itself). ⚠️ Sheet 8's "Daniels"/"Rank" labels on this line conflict with the OOB (both at ECF) — encode the disagreement.
- **Replacement batteries fed to the crest during/after the cannonade (all tablet-attested "Engaged… July 3," all in-window movers, grade A):** Fitzhugh's K/1st NY ("on Second Corps line"), Parsons's A/1st NJ ("on line of Second Division Second Corps"), Weir's C/5th US ("on Cemetery Ridge **and in front** on left of Second Corps"), **Wheeler's 13th NY — OR report transcribed: in reserve behind Cemetery Hill "during the heavy cannonade from 1 to 3 p.m.," ordered to the II Corps ~4 PM, "enfilade their column with canister… brought them to a halt three times"** (http://www.13nybattery.com/battles/wheeler_gettys.htm).
- **Osborn's Cemetery Hill concentration (~39 guns; ~1,600 rounds into Pettigrew's left; deliberate mid-cannonade cease-fire ruse with Hunt/Howard):** Wiedrich I/1st NY, Dilger I/1st OH, Bancroft G/4th US, Taft's 5th NY 20-pdrs, Mason's H/1st US (for Eakin), Edgell's 1st NH, Norton's H/1st OH, Hill's C/WV, Ricketts's F&G/1st PA (East Cemetery Hill) — "Engaged on Cemetery Hill July 2 and 3" per the reserve-brigade tablets; Heckman's K/1st OH wrecked July 1, July 3 role unattested (flag); Taft's 1st CT Heavy B & M "not engaged." Grade **A** collectively; ⚠️ per-battery arcs/rounds attested collectively, not individually. Sources: `.../2nd-volunteer-brigade-artillery-reserve/`, `.../3rd-volunteer-brigade-artillery-reserve/`, `.../1st-regular-brigade-artillery-reserve/`, https://en.wikipedia.org/wiki/Cemetery_Hill, padresteve.com "The Artillery…Must Concur as a Unit."
- **Wainwright's I Corps guns (East Cemetery Hill/Stevens Knoll):** Stevens's 5th ME (Whittier) on Stevens Knoll, Breck's E&L/1st NY in the lunettes, Stewart's B/4th US astride the Baltimore Pike "commanding the approach from the town" — positions documented; hill-wide firing in-window attested collectively, per-battery flagged; **Hall's 2nd ME relieved by Edgell and sent to the rear — "no action July 3" (documented negative)**. Grade B+/A−.
- **Rittenhouse's D/5th US, Little Round Top summit — grade A:** "did effective service on the lines of infantry engaged in Longstreet's assault"; enfiladed Kemper's right (`gettysburg.stonesentinels.com/us-regulars/us-artillery/5th-us-artillery-battery-d/`, civilwarintheeast.com Battery D page).
- **XII Corps artillery (Muhlenberg):** Rugg's F/4th US + Kinzie's K/5th US rear of the corps centre; Atwell's E/PA + **Rigby's A/MD on Powers Hill**; Winegar's M/1st NY on McAllister's Hill — 26 guns "commanding the valley of Rock Creek," **fired daylight–10:30 AM only; silent in-window** (documented negative) (`.../artillery-brigade-12th-corps/`, `.../4th-volunteer-brigade-artillery-reserve/`). Grade B+.
- **V Corps artillery (Martin):** Gibbs's L/1st OH, Walcott's 3rd MA, Barnes's C/1st NY on/behind the Round Top line — in-window fire unattested (flag); Watson's I/5th US wrecked July 2, out of action (`.../artillery-brigade-5th-corps/`). Grade B.
- **VI Corps artillery (Tompkins, 8 batteries/48 guns):** "placed in reserve on different portions of the field so as to be available but… were not actively engaged" (Cowan excepted — modeled) — exact parks unattested (flag) (`.../artillery-brigade-6th-corps/`). Grade B−/C as one or two park markers.
- **III Corps artillery (Clark acting for Randolph):** four batteries refitting in the rear after July 2; only Seeley's K/4th US (Lt. James) in action, on McGilvery's line (`.../artillery-brigade-3rd-corps/`). Grade B/C for the refit park.
- **Artillery Reserve park + ammunition train (Tyler; 2,375 men, 114 guns nominal):** parked between Taneytown Rd and Baltimore Pike behind the centre; **shelled by overshoots during the cannonade — the park and all caissons displaced south behind Little Round Top mid-window; Hunt found "the remains of a dozen exploded caissons" on the Taneytown pike** (Hunt's own account, https://www.historycentral.com/CivilWar/getty/Hunt3.html; https://www.americanheritage.com/slaughter-cemetery-ridge). **A− in-window mover.** Org roster: `gettysburg.stonesentinels.com/armies/army-of-the-potomac/artillery-reserve/`.

### 3.4 Cavalry, HQs, trains, hospitals

**South Cavalry Field — the one on-map cavalry fight, and it IS in-window:**
- **Farnsworth's brigade** (1st VT, 1st WV, 18th PA, 5th NY; 1,925 men) arrived **~13:00 as the cannonade opened**, deploying south of the Bushman farm/Bushman Hill (ON map); dismounted probing of Law's line; a 1st VT squadron took the Bushman buildings and was ejected when the 1st TX arrived. **The mounted charge is post-window** (Kilpatrick got word of the repulse ~17:00; sources 5:00 vs 5:30 PM — flagged). Grade **A−** for the in-window deployment + probing. Sources: npshistory.com/series/symposia/gettysburg_seminars/9/essay8.pdf (Snell); https://en.wikipedia.org/wiki/Battle_of_Gettysburg,_third_day_cavalry_battles; hmdb.org/m.asp?m=15521. Sheet 8 gives regiment-level geometry (§2.1).
- **Elder's E/4th US** on the knoll S of Big Round Top, **shelling Law's line during the window** — A−. **Graham's K/1st US** with Merritt, firing — A− (Snell).
- **Merritt's Reserve Brigade** (1st/2nd/5th US, 6th PA; 6th US detached to Fairfield) astride the Emmitsburg Rd at the map's southern edge (Currens/Kern ground): dismounted skirmishing vs the 9th GA + Black's ~100 SC troopers + armed teamsters + Hart's gun; the 5th US mounted charge and the GA counter, early-to-mid afternoon. ⚠️ Arrival disagreement: ~11:00 (Wittenberg-derived, studycivilwar.wordpress.com) vs ~13:00 (Snell) vs ~15:00 (Wikipedia) — either way **skirmishing underway before 16:00**. Grade A−/B+ with the time spread encoded. CSA response: Law's 1st TX shift + **Bachman's & Reilly's batteries (Henry's bn) swung south** — the in-window CSA counter-move (Snell; battlefields.org South-Cavalry-Field map page, facts only per the ABT ToS ruling).
- **East Cavalry Field: OFF-MAP — do not place units.** Encode an off-map engagement flag: Gregg's division (McIntosh, J.I. Gregg) + Custer vs Stuart (Hampton, Fitz Lee, Chambliss, Jenkins/Witcher) + 5 horse batteries (Pennington, Randol; Jackson, Breathed, McGregor); dismounted fight from ~13:00, mounted charges climaxing ~15:00, Hampton wounded — entirely 4–5.5 km east of the square. On-map fringe only: J. Irvin Gregg's westernmost Hanover Rd pickets near Brinkerhoff's Ridge (possible, not position-attested — leave off) and Neill's tablet-attested link to "the Cavalry pickets." Off-field confirmed with URLs: Jones & Robertson (Fairfield corridor; **6th US Cav wrecked at Fairfield ~13 km SW that afternoon, 242 cas., no precise clock attested** — Wikipedia Battle of Fairfield, hmdb m=15179), Imboden (Cashtown, reached the field-rear at noon guarding trains — stonesentinels), Huey (Westminster), Buford/Gamble/Devin (Westminster→Frederick).

**HQs & logistics (all URL-attested in the cavalry/logistics pass):**
- **Meade's HQ:** Leister house shelled; Meade displaced to **Powers Hill (Slocum's HQ + signal station) mid-cannonade, returning during the repulse** — an authorable in-window HQ track (gettysburgdaily.com/powers-hill/; stonesentinels Leister-farm page; nps.gov/places/lydia-leister-farm.htm).
- **Powers Hill node:** Slocum HQ, signal station, artillery/ambulance park, ammunition-train shelter; 4th NJ train guard stopping fugitives — the Union rear hub, ON map (gettysburgdaily.com; gettysburgciviliannetwork.com Powers Hill article).
- **Provost (Patrick):** HQ at the Sheely farm; prisoner depot on/near the Baltimore Pike; 93rd NY + 8th US "stationed at Taneytown and not engaged" (stonesentinels 93rd NY monument). Post-repulse prisoner flood is after 16:00.
- **Engineer Brigade (Benham): not at Gettysburg** — left for Washington July 1 (Wikipedia Union OOB). Do not model.
- **Hospitals ON map:** XI Corps at George Spangler farm (~1,900 wounded, actively receiving in the window — gettysburgfoundation.org; pa-roots.com); XII Corps at the George Bushman house on Rock Creek (⚠️ name-collision with the SCF Bushman farm); the **Rock Creek–White Run complex** (I/II/III/V/VI Corps sites, Jacob Schwartz farm) straddling the SE corner (Wikipedia Rock Creek-White Run article; mtjoytwp.us); CSA: **Black Horse Tavern (McLaws's division hospital)** at the W edge (stonesentinels; Wikipedia); Pickett's hospital (Bream's Mill/Currens on Marsh Creek) borderline W/SW — its flood arrives after ~16:00 (civilwartalk thread; hmdb m=64317 — tradition-grade, flag). Ewell's hospitals N/NE of town mostly beyond the N edge (exact farms low-confidence — flag). **Camp Letterman is July 20+ — not a July 3 feature** (VERIFIED).
- **Lee's HQ:** Thompson house, Chambersburg Pike (ON map); Lee forward on Seminary Ridge near the Point of Woods during the charge (stonesentinels Thompson-house page).
- **CSA ordnance trains** behind Seminary Ridge (Willoughby Run/Fairfield Rd corridors): would be on-map but **specific July 3 park sites are not URL-attested — inference-grade only (Kent Masterson Brown's *Retreat from Gettysburg* is the literature to consult); grade C** — do not invent positions.

---

## 4. In-window engagement events shortlist (beyond the modeled assault sector)

Documented events the full cast makes authorable, each cited above:

1. **13:07 signal guns** — Miller's battery, Eshleman's battalion, Peach Orchard (tablet).
2. **13:07–~14:45 bombardment emitters** — 11 CSA battalions firing (§3.2); Union counter-battery from Osborn's hill and Hazard (modeled); Phillips "about 1:30… fired on the Confederate batteries" then held under Hunt's conserve-ammunition policy — **the McGilvery line's silence is itself documented.**
3. **Osborn/Hunt deliberate cease-fire ruse** mid-cannonade to feign silenced guns and invite the assault.
4. **Artillery Reserve park + caissons displace behind Little Round Top under the overshoots; a dozen caissons explode along the Taneytown Rd** (Hunt).
5. **Meade's HQ displaces Leister→Powers Hill and back** (mid-window).
6. **Cooper's B/1st PA moves East Cemetery Hill→Hancock Ave at 3 PM under fire and opens** (monument text).
7. **Robinson's two I Corps brigades take position on the right of II Corps at 3 PM** (division tablet).
8. **Shaler's brigade to the army centre ~3:30 PM** (tablet + Wikipedia).
9. **Repulse fires (~15:00–16:00):** McGilvery's ~39 guns enfilade Pickett's/Kemper's flank; Osborn's ~1,600 rounds into Pettigrew's left; Rittenhouse enfilades from Little Round Top; Fitzhugh/Parsons/Weir feed the crest; Wheeler's canister halts the column "three times" (~16:00, his OR report); Phillips smashes the Florida brigade's follow-up.
10. **Wright's brigade advances in support and is recalled ~15:30–16:00** (NPS seminar essay).
11. **Long Lane skirmish fire all window** — Thomas & Perrin tablets ("severe skirmishing… heavy artillery fire" / "constantly engaged"), opposite Hays's pickets; Early's house-sharpshooters vs Carroll on East Cemetery Hill.
12. **South Cavalry Field:** Farnsworth arrives/deploys ~13:00; Elder & Graham shelling; Merritt vs Black/9th GA skirmish + the 5th US charge; Law's 1st TX + Bachman/Reilly counter-shift (~15:00+).
13. **Documented negatives to display, not hide:** Ewell's wing ~10–14 of ~79 guns firing; Garnett's battalion idle; XII Corps' 26 guns silent after 10:30; VI Corps' 48 reserve guns parked; Johnson's seven brigades spent east of Rock Creek; Hall's 2nd ME withdrawn.

**Timing/fact disagreements to encode as data** (extends the regiment survey §7 list): Merritt's arrival (11:00/13:00/15:00); Farnsworth's charge (17:00 vs 17:30 — post-window either way); Johnson's hold-until (22:00 vs midnight); ECF duration ("40 minutes" vs multi-hour) and Custer relief (noon vs ~14:00); Robertson's TX command (Robertson vs Work); Garnett's battalion fired-or-not; Henry's cannonade weight; Russell's move straddling 16:00; sheet-8 "Daniels"/"Rank" on McGilvery's line vs the OOB placing both at East Cavalry Field.

---

## 5. Scale estimate & recommended authoring order

### 5.1 Unit counts at the directed grain

| Tranche | Count | Grade mix |
|---|---|---|
| Union infantry brigades (I:6, II:5, III:6, V:8, VI:8, XI:6, XII:6) | **45** | 4 A− movers; rest B+/B static |
| CSA infantry brigades (Longstreet 8, Hill 5, Ewell 13) | **26** | 2 A− (Thomas, Perrin), 4 B+, rest B |
| CSA artillery battalions | **15** | 10 A firing, 3 A− partial, 2 B silent |
| Union batteries, core (McGilvery line 9 + reinforcements 4 + Osborn group 10 + Wainwright 4 + Rittenhouse 1 + XII Corps 5) | **~33** | ~18 A firing/moving; rest B+ documented-static |
| Union batteries/parks, optional (V Corps 3, 9th MA, Turnbull F&K/3rd US, Heckman, VI Corps park, III Corps refit park, Arty Reserve park+train) | **~8–10** | B/C; the Reserve park itself is A− (documented mid-window displacement) |
| South Cavalry Field (Farnsworth bde [or its 4 regts per sheet 8], Merritt bde @ edge, Elder, Graham; CSA Black/Hart, 1st TX shift, Bachman, Reilly) | **6–8** | A−/B+ |
| HQ/hospital/logistics markers (Meade HQ track, Lee HQ, Powers Hill node, provost/Sheely, Spangler hosp., Bushman hosp., White Run complex, Black Horse Tavern, ± Bream's) | **8–9** | B+ documented sites; 2 in-window movers |
| **Total new** | **~120–140** | (existing battle file: 65 units) |

Off-map flags (encode as flags, not units): East Cavalry Field (Gregg, McIntosh, J.I. Gregg, Custer; Hampton, Fitz Lee, Chambliss, Jenkins/Witcher + 5 horse batteries), Fairfield (6th US vs Jones; Robertson), Westminster (Huey, Buford, main trains), Cashtown (Imboden, CSA trains).

### 5.2 Recommended authoring order (artillery-that-fired first)

1. **McGilvery's line + the four reinforcing batteries** (13–14 units) — densest A-grade attestation; two documented in-window battery tracks (Cooper 3 PM, Wheeler ~4 PM); directly extends the existing Hazard/Cowan/Brown modeling southward; encodes Hunt's hold-fire policy as a visible behavior.
2. **Osborn's Cemetery Hill group + Wainwright's + Rittenhouse** (~15) — closes the Union gun ring; carries the cease-fire ruse and the Pettigrew-flank fires.
3. **CSA artillery battalions, all 15** — the bombardment's emitters AND its documented silences; **re-anchor the app's procedural smoke corridor to these named battalion positions** (every firing battalion is a cited in-window emitter with a tablet/OR citation).
4. **In-window movers:** Robinson's two brigades (15:00), Shaler (~15:30), Wright's advance-recall, Artillery Reserve park displacement, Meade HQ track.
5. **Union static ring** (remaining ~41 brigades: Culp's Hill I/XII, Cemetery Hill XI + Carroll, Caldwell, Doubleday, Round Tops V/VI, III Corps reserve) — cheap 1–2-keyframe tracks, each with a tablet citation and a role string ("not engaged — subject to artillery fire").
6. **CSA static ring** (remaining ~24 brigades: Hood/McLaws right, Anderson/Pender ridge + Long Lane skirmishers, Johnson/Early/Rodes arc) — same pattern; Johnson's east-of-Rock-Creek block is the visual payoff (sheet 8 and the tablet agree exactly).
7. **South Cavalry Field cluster** (6–8 units) — the only maneuver combat on the square outside the charge; regiment-level geometry already on sheet 8's south margin.
8. **HQ/hospital/logistics markers + off-map engagement flags.**

Rationale: each step ships independently; steps 1–3 change what the player *sees* during the bombardment (the whole horseshoe firing, not just the Angle); steps 5–6 are high-count/low-cost position-only fills exactly per the position-only directive; nothing requires a source not already in hand except McGilvery's/Osborn's OR reports (upgrades, not blockers).

### 5.3 Open items

1. **McGilvery's and Osborn's OR reports (27/1)** — would upgrade per-battery firing attestation from tablet-collective to report-individual. Same standing OR-Part-1 access item as the regiment survey.
2. **Sheet-8 vs OOB conflicts** to adjudicate at authoring time: the "Daniels"/"Rank" labels on McGilvery's line (both units attested at ECF); re-read the sheet lettering at 100% before encoding.
3. **CSA ordnance-train park sites** west of Seminary Ridge — on-map but inference-grade; consult Kent Masterson Brown, *Retreat from Gettysburg*, before placing anything.
4. **Ewell's division hospital farms** N/NE of town — Coco's hospital studies would settle; currently low confidence, mostly off the N edge anyway.
5. **J. Irvin Gregg's westernmost Hanover Rd vedettes** — the only possibly-on-map ECF element; not position-attested; leave off unless a source surfaces.

## Workflow note

Three research passes (Union OOB / CSA OOB / cavalry-trains-hospitals) all returned complete, URL-cited reports this session and are synthesized above; the two decisive primary verifications were done first-hand (nine sheet-8 IIIF sector crops read by eye; landmark geometry computed from the terrain square's UTM origin). Verification crops saved alongside this report: `sheet8_full_1400.jpg`, `s8_culps.jpg`, `s8_cemhill.jpg`, `s8_cemhill_zoom.jpg`, `s8_roundtops.jpg`, `s8_mcgilvery.jpg`, `s8_town.jpg`, `s8_peachorchard.jpg`, `s8_semridge_n.jpg`, `s8_southcav.jpg`.
