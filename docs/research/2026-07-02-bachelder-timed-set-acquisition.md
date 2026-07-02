# Bachelder Timed Map Set — July 3 Sheets: Source Acquisition Report

**Date:** 2026-07-02. **Scope:** locate georeferenceable rasters of the July 3 sheets of Bachelder's timed troop-position set (the "28-map set," Rumsey scan of the 1995 Morningside reproduction), verify rights, verify LOC/other-archive alternatives, document download mechanics. **No assets downloaded** beyond metadata, JP2 header bytes, small derivative JPEGs, and IIIF region crops used for verification. All HTTP statuses below observed live this session.

---

## 1. Headline findings (three upgrades over the research doc)

1. **The afternoon sheet is "1–5 P.M.," not "1–3 P.M." — and it depicts the charge itself.** Read by eye from the sheet's printed bottom margin: **"No. 8. 1-5 P.M. July 3."** A full-resolution IIIF crop of the Angle sector shows the assault **in motion**: red regiment-granular bars mid-field between Seminary Ridge and the wall, labeled `PETTIGREW'S AND TRIMBLE'S COMMANDS`, `PICKETT'S DIVISION`, `GARNETT`, with Union batteries individually named (`ARNOLD`, `CUSHING`, `COWAN`, `BROWN`, `RORTY`) and regiments named on the Union flank (`13' Vt`, `14' Vt`, `16' Vt` under `STANNARD'S Brigade`; `14' Ct` on Hays's front). **The research doc's "the charge falls in the gap between sheets" conclusion (§1.2) is refuted — sheet No. 8 covers our entire 13:00–16:00 window including the assault.**
2. **The sheet-numbering conflict (research doc §1.2 ⚠️) is resolved.** The civilwardigital "maps 10–12, 4–11 / 11–2 / 2–5" numbering belongs to the **East Cavalry Field (Gregg vs. Stuart) set** — verified against the Rumsey/IA titles of pub-group 12441 ("No. 10. 4-11 a.m. July 3", "No. 11. 11 a.m.-2 p.m. July 3", "No. 12. 2-5 p.m. July 3"). Horse Soldier's "1:00–3:00 PM" for the main-field afternoon sheet is **wrong**; the printed sheet says 1–5 PM. Main-field July 3 slices: **4–8 AM, 8–11 AM, 1–5 PM, 5 PM**, carrying dual numbers: per-day No. 1–4 and a continuous-series No. 6–9 (July 2's five sheets are No. 1–5 in that second scheme).
3. **Rumsey's IIIF image service is directly consumable — no bot wall, no auth.** The davidrumsey.com *HTML* pages sit behind a verification interstitial (WebFetch and curl both get a 4 KB "Verify Access" page), but the **Luna IIIF endpoints respond 200 to plain curl** at full resolution (~9,500 × ~10,750 px, IIIF Image API 2, level 2 — same API family as the LOC endpoints the authoring tool already drapes).

Physical set composition confirmed from the Rumsey/IA record: **23 main-field maps** (14 July 1 + 5 July 2 + 4 July 3), pub-group 12440, plus **5 cavalry-field maps**, pub-group 12441 = the "28-map set." The IA description states verbatim: *"Part of a set of 23 maps prepared by order of Major General A.A. Humphreys. This group of maps was reproduced from the originals by Morningside Press in 1995. The set is now out of print."*

**What these sheets physically are:** Bachelder's manuscript troop positions (red/blue hand ink, hand lettering) drawn onto the printed **Warren 1873 engineer base map** (1:12,000, Julius Bien lith.). The IA/Rumsey record accordingly credits *creator: Warren, G. K. / Julius Bien Lith.*, *date: 1873*, *publisher: United States Army, Engineers*. Caution for anyone reading the sheet header: the printed top-right legend text "Positions of troops are not given" is the **Warren base-map legend** — the troop positions are the manuscript overlay, not part of the printed base.

---

## 2. Catalog entries — the four July 3 sheets (all URLs verified live)

Rumsey publication group: **pub-list-no 12440.000**. Each sheet exists in two places: davidrumsey.com (Luna) and Rumsey's own mirror on Internet Archive (collection `david-rumsey-map-collection`).

| Sheet | Rumsey list no. | Rumsey Luna ID | IIIF full-res (px, W×H) | IA identifier | IA JP2 size |
|---|---|---|---|---|---|
| No. 1 (No. 6), 4–8 AM | 12440.020 | `RUMSEY~8~1~299033~90070136` | 9513 × 10739 | `dr_battle-field-of-gettysburg-no-1-no-6-4-8-am-july-3-1863-12440020` | 15,567,464 B |
| No. 2 (No. 7), 8–11 AM | 12440.021 | `RUMSEY~8~1~299034~90070135` | 9480 × 10654 | `dr_battle-field-of-gettysburg-no-2-no-7-8-11-am-july-3-1863-12440021` | 15,393,243 B |
| **No. 3 (No. 8), 1–5 PM** | **12440.022** | **`RUMSEY~8~1~299035~90070134`** | **9471 × 10752** | **`dr_battle-field-of-gettysburg-no-3-no-8-1-5-pm-july-3-1863-12440022`** | **15,514,271 B** |
| **No. 4 (No. 9), 5 PM** | **12440.023** | **`RUMSEY~8~1~299036~90070133`** | **9502 × 10803** | **`dr_battle-field-of-gettysburg-no-4-no-9-5-pm-july-3-1863-12440023`** | **15,646,522 B** |

- Rumsey detail pages (pattern `https://www.davidrumsey.com/luna/servlet/detail/<LunaID>`): human-browsable, but **HTTP-fetch-blocked** by the site's verification interstitial (observed: 200 with a 4 KB "Verify Access" body). Use for citation, not scripting.
- IA item pages: `https://archive.org/details/<identifier>` — metadata verified via `https://archive.org/metadata/<identifier>` (HTTP 200, JSON).
- JP2 native dimensions verified independently by parsing the JP2 `ihdr` box from a 4 KB HTTP range request (HTTP 206): 9471 × 10752 for sheet No. 8 — **matches the Rumsey IIIF info.json exactly**, i.e., the IA JP2 and the Rumsey IIIF service carry the same full-resolution scan.
- Effective resolution: ~9,500 px across a ~69 cm sheet ≈ **350 ppi — trace quality**, comparable to the LOC scans of the 1876 three-sheet set (8,315–9,608 px) already used in the repo.
- The companion volume (Sauers/Bearss *User's Guide*, Morningside 1995, in copyright) is also in Rumsey: Luna ID `RUMSEY~8~1~299043~90070126` — reference only.
- East Cavalry Field July 3 sheets, if ever wanted: IA `dr_map-of-the-field-of-operations-of-greggs-union--stuarts-confederate-12441003/-004/-005` (pub-group 12441.000; No. 10–12). LOC independently holds an ECF sheet: https://www.loc.gov/item/99447493/.

### Echelon verification (by eye, full-res IIIF crops, this session)

- **Sheet No. 8 (1–5 PM):** both armies drawn as regiment/battery-granular bars. Union batteries individually named; Stannard's Vermont regiments individually named; larger masses labeled at brigade/division (`HAYS' DIVISION`, `HALL`, `HARROW`, `MADILL'S BRIG.`, `McGILVERY'S RVE ARTILLERY BRIGADE` with battery names Sterling/Rock/Cooper/Dow/Ames). The Confederate assault column is drawn as dense red bars (regiment-granular) labeled at command level (`PICKETT'S DIVISION`, `GARNETT`, `PETTIGREW'S AND TRIMBLE'S COMMANDS`).
- **Sheet No. 9 (5 PM):** post-repulse. Confederate fragments withdrawn west of the Emmitsburg Road (scattered red dashes with arrows); Union line labeled to regiment/battery in places (`COWAN'S BAT`, `PARSONS`, `BROWN`, `19' MAINE`, `20' INDIANA`, `4' MAINE`, `WEBB`, `HALL`, `HARROW`, `STANNARD`).
- The dealer claim "every regiment and battery of both armies" is **plausible at bar granularity but overstated at label granularity** — labeling density varies by sector (mirrors the 1876 sheet's asymmetry). A full-sheet legibility pass should happen at georeferencing time.

---

## 3. Rights — carefully

### 3.1 The Rumsey scans (both hosts)

- **Per-item rights statement on the IA copies (verbatim, identical on all four July 3 items):**
  > "Images may be downloaded and used following Creative Commons CC BY-NC-SA 3.0 license. Image credit should be given to 'David Rumsey Map Collection, David Rumsey Map Center, Stanford Libraries.' Please contact the David Rumsey Map Collection for commercial use."
- **davidrumsey.com Copyright & Permissions page** (https://www.davidrumsey.com/about/copyright-and-permissions, fetched HTTP 200 this session) is actually **broader** than plain BY-NC-SA. Verbatim, first operative sentence:
  > "Images from this web site and database may be reproduced or transmitted and used without charge for personal use or in any publication, either in print or digital media, **by any for profit or non profit publisher**. Please contact us for permission if you intend to make reproductions of our maps as stand alone for sale facsimiles."
  followed by: "This work is licensed under a Creative Commons License" (linked: `creativecommons.org/licenses/by-nc-sa/3.0/`), a post-1929 copyright-check obligation on the user, and the credit line "David Rumsey Map Collection, David Rumsey Map Center, Stanford University Libraries."
- Net: Rumsey's own terms permit even for-profit *publication* use with credit; the formal license badge is CC BY-NC-SA 3.0. (The familiar argument that a faithful scan of a PD flat work adds no copyright — Bridgeman — exists, but our repo precedent is to not rely on it and to prefer license-clean sources.)

### 3.2 The underlying work — a wrinkle the research doc's "PD status" line missed

The research doc (§1.2) says "original 1880s sheets = PD (pre-1930 + War Department authority)." **That reasoning assumes 19th-century publication — but these sheets were never published in the 19th century.** The same source base says so: delivered to the War Department 1886, "languished, unseen... for more than a hundred years," first reproduced by Morningside in **1995**. That is exactly the **17 U.S.C. §303(a)** fact pattern the research doc itself flags for the Bachelder *letters*: a pre-1978 unpublished work first published 1978–2002 is potentially protected until **Dec. 31, 2047** — and the protected expression here would be Bachelder's drawn troop positions (the Warren 1873 printed base underneath is unambiguously PD).

Countervailing arguments (real but not self-executing):

- **Government commission/property:** Congress appropriated the funds; the maps were produced for and delivered to the War Department; the originals are federal property (research doc: NARA RG 77 is "the next hunt"). A work owned by the U.S. government sits awkwardly with a private §303(a) claim, and no heir/publisher claimant is in evidence.
- **Rumsey/Stanford openly publish the scans** under a permissive statement, and Morningside's 1995 edition asserted (at most) reproduction-level rights over a work it did not author.
- **Facts are free regardless:** unit X drawn at position Y in time-window Z is an uncopyrightable fact under the repo's existing Feist framing.

**This is a judgment call for the user, not for this report** — see §6.

### 3.3 Alternative, license-cleaner archives — checked

- **Library of Congress: negative, re-verified this pass.** Full search `loc.gov/search/?q=bachelder+gettysburg&fo=json` (HTTP 200, 100 results inspected): LOC holds the 1876/1883 three-sheet sets (99447490/91/92, lva0000067/68), the isometric (2007630423 etc.), the ECF cavalry sheets (99447493, 99448799), the plain Warren base ("Battle field of Gettysburg," 187?, item 99448794), and the reduced single reprint sheet (99447494, per prior pass) — **no main-field timed sheets.**
- **Internet Archive:** holds all 23 main-field sheets — but *only* as Rumsey's own uploads (collection `david-rumsey-map-collection`), carrying Rumsey's rights statement. Not an independent scan. (Full-collection search verified: 46 Gettysburg items, all timed sheets accounted for.)
- **NARA RG 77:** the manuscript originals presumably live here; the RG 77 "Civil Works Map File" has been partially digitized (catalog.archives.gov, e.g. NAID 7491452 for the Civil War map series), but **no digitized Bachelder timed sheets were located** in this pass. A targeted NARA catalog hunt is the one remaining route to a fully-PD-clean, license-free scan — likely slow, uncertain, and the scans (if any) would be of the fragile originals.
- **HathiTrust / NYPL / BPL / New Hampshire Historical Society:** not exhaustively swept this pass; NHHS holds the Bachelder *papers* (letters), not known scans of this map set; the *Bachelder Papers* vol. 3 map supplement (Morningside 1994–95) exists only as a controlled-lending book on IA — not a raster source. Low expected yield; noted for completeness.

---

## 4. Download / consumption mechanics (verified)

### Option A — Rumsey Luna IIIF, direct drape (recommended for the authoring tool)

Same consumption model as the LOC `tile.loc.gov/image-services/iiif/...` precedent; IIIF Image API **v2, level 2**, tiles 1536×1536, scaleFactors 1–16, formats jpg/png. **Verified HTTP 200 on all four info.json endpoints and on sample tile/region requests, plain curl, no cookies/auth** (a browser User-Agent header was sent; not verified whether it is required):

```
https://www.davidrumsey.com/luna/servlet/iiif/RUMSEY~8~1~299033~90070136/info.json   # 4-8 AM   (9513x10739)
https://www.davidrumsey.com/luna/servlet/iiif/RUMSEY~8~1~299034~90070135/info.json   # 8-11 AM  (9480x10654)
https://www.davidrumsey.com/luna/servlet/iiif/RUMSEY~8~1~299035~90070134/info.json   # 1-5 PM   (9471x10752)
https://www.davidrumsey.com/luna/servlet/iiif/RUMSEY~8~1~299036~90070133/info.json   # 5 PM     (9502x10803)
```

Example verified requests:
- Tile/thumb: `.../RUMSEY~8~1~299036~90070133/full/512,/0/default.jpg` → 200, image/jpg, 62 KB.
- Region crop: `.../RUMSEY~8~1~299035~90070134/4700,5300,2200,1700/1100,/0/default.jpg` → 200, 425 KB (the Angle-sector verification crop).

Caveats: the info.json declares `@context` IIIF v2 (not v3) — fine if the tool's IIIF layer speaks v2 (LOC's services are also v2). The *HTML* side of davidrumsey.com is bot-walled; only the `luna/servlet/iiif/` and `luna/servlet/detail` image-API paths were exercised. Sustained bulk tile scraping may hit rate limits — untested, and unnecessary given Option B.

### Option B — Internet Archive full-file JP2 (recommended for a local georeferencing master)

```
https://archive.org/download/dr_battle-field-of-gettysburg-no-3-no-8-1-5-pm-july-3-1863-12440022/12440022.jp2   # 15.5 MB, 9471x10752
https://archive.org/download/dr_battle-field-of-gettysburg-no-4-no-9-5-pm-july-3-1863-12440023/12440023.jp2   # 15.6 MB, 9502x10803
```
(morning sheets: `.../12440020/12440020.jp2` 15.6 MB, `.../12440021/12440021.jp2` 15.4 MB.) Range requests work (verified HTTP 206). Each item also has a ~1.8 MB reduced `NNNNNNNN.jpg` derivative (verified 200) — good for quick-look layers.

**Do not use IA's IIIF for these items:** `iiif.archive.org/iiif/3/<identifier>/info.json` is bound to the small JPEG derivative (verified: 1353 × 1536 px only), and the file-scoped `.../<identifier>%2f<file>.jp2/info.json` form 404s. IA is a *file host* here, not a usable tile service.

Total for the two in-window sheets: **~31 MB** (all four July 3 sheets: ~62 MB).

---

## 5. Recommendation

1. **Primary raster source: the Rumsey scans** — they are the only digitization of the main-field timed set anywhere (LOC negative re-verified; NARA undigitized as far as this pass could see). Consume via **Option A (Rumsey IIIF)** in the authoring tool, and pull the two **IA JP2 masters (Option B)** for offline georeferencing, with the credit line "David Rumsey Map Collection, David Rumsey Map Center, Stanford Libraries" recorded in the battle-file provenance fields.
2. **Prioritize sheet No. 8 (1–5 PM, list 12440.022)** — it single-handedly covers the whole 13:00–16:00 slice including the charge — then **No. 9 (5 PM, 12440.023)** as the end-state anchor.
3. Keep the repo's existing two-tier posture: **positions read off these sheets are facts** (encode with citation to sheet + list no., `documented` position / `inferred` clock within the sheet's window) regardless of any license question; **draping/serving the raster itself** is the part governed by §6.
4. Record the corrected slicing (4–8 / 8–11 / **1–5** / 5) in `docs/research/2026-07-02-regiment-track-sources.md` §1.2 and §8 (open item 1 can be closed except for the license question), including that the ECF set owns the "No. 10–12" numbers.

## 6. Open questions for the user's judgment (not decided here)

1. **Is our use "non-commercial"?** Rumsey's formal license is CC BY-NC-SA 3.0, but Rumsey's own permissions page grants publication use "without charge for personal use or in any publication... by any for profit or non profit publisher" (facsimile sales excepted). A personal art project appears comfortably inside *both* readings — but if the project might ever be sold or ship as a product, the NC clause and the "contact for commercial use" sentence become live. User call: use-with-attribution now, or treat as fact-check-only like the copyrighted map studies?
2. **The §303(a) wrinkle (§3.2):** Bachelder's overlay was arguably unpublished until 1995, which *could* mean protection to 2047 for the drawn positions themselves — cutting against the research doc's "PD" label. Counterarguments (government commission, government-owned originals, no claimant) are strong but unadjudicated. Options: (a) accept the risk as negligible for a personal project with attribution; (b) restrict these sheets to fact-extraction + georeferencing reference (never redistributing the raster); (c) ask NARA whether the originals are digitized/PD-certifiable. Same counsel-check the doc already recommends for the Bachelder letters would cover this.
3. **Attribution SHA/provenance:** if we drape Rumsey IIIF directly, do we want the battle-format provenance fields to carry the Rumsey list numbers + credit line as a standing requirement? (Cheap to do now; matches the format's provenance contract.)

## Appendix — verification log (URL → status, this session)

| URL | Status |
|---|---|
| `archive.org/advancedsearch.php` (collection `david-rumsey-map-collection` × gettysburg, 46 hits) | 200 |
| `archive.org/metadata/dr_...12440020 / 21 / 22 / 23` | 200 (JSON) |
| `archive.org/download/dr_...12440022/12440022.jp2` (range 0–4095) | 206; `ihdr` = 10752×9471 |
| `archive.org/download/dr_...12440022/12440022.jpg` (1.8 MB derivative) | 200 |
| `iiif.archive.org/iiif/3/dr_...12440022/info.json` | 200 but **1353×1536 derivative only** |
| `iiif.archive.org/iiif/3/dr_...12440022%2f12440022.jp2/info.json` | 404 |
| `davidrumsey.com/luna/servlet/iiif/RUMSEY~8~1~299033~90070136/info.json` (and ~299034/35/36) | 200 ×4, full-res |
| `davidrumsey.com/luna/servlet/iiif/.../full/512,/0/default.jpg` (5 PM) | 200 |
| `davidrumsey.com/luna/servlet/iiif/.../4700,5300,2200,1700/1100,/0/default.jpg` (No. 8 and No. 9) | 200 ×2, crops read by eye |
| `davidrumsey.com/about/copyright-and-permissions` | 200 (license text quoted §3.1) |
| `davidrumsey.com/luna/servlet/detail/RUMSEY~8~1~299035~90070134` | 200 but bot-wall "Verify Access" body — cite, don't script |
| `loc.gov/search/?q=bachelder+gettysburg&fo=json&c=100` | 200 (WebFetch 403; plain curl w/ browser UA 200) — no timed sheets in results |

Verification crops saved alongside this report: `angle_crop2_sheet8.jpg` (No. 8, the charge in motion), `angle_crop_sheet9_5pm.jpg` (No. 9, post-repulse), `angle_crop_sheet8.jpg` (No. 8, southern sector), `sheet22_deriv.jpg` (No. 8 full-sheet derivative with the "No. 8. 1-5 P.M. July 3." margin label).
