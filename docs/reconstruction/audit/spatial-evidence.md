# Spatial evidence program — the cartographic substrate for EC3

**Status:** PROPOSAL (audit-spatial-1 branch, 2026-07-11). Adopted
practice is described as such where it already is (the LiDAR/UTM frame);
everything new here is proposed, and the monument reliability profile is
an ED candidate (proposed ED-32, `monument-register.md`).

This is the source layer every dossier's **EC3 (position anchors)** is
judged against: what each cartographic source *is*, what uncertainty it
carries, and how a dossier author uses it.

## The map hierarchy

| Layer | Source | Role | Absolute accuracy |
|---|---|---|---|
| **Metric substrate** | USGS 1 m LiDAR on EPSG:26918, heightmap origin (304208.02 E, 4404534.27 N) | The frame itself. Battlefield-local meters = UTM minus origin; all geometry lives here. (Adopted practice since the terrain pipeline.) | ~1 m |
| **Cartographic authority** | Bachelder timed set: manuscript troop positions on the Warren 1873 1:12,000 engineer base — 23 main-field sheets (14 July 1 + 5 July 2 + 4 July 3) + 5 East Cavalry Field sheets | The only time-sliced, whole-army, regiment-granular position source. Terrain-relative readings (unit X behind wall Y) carry the Warren survey's quality; the troop overlay carries Bachelder's 20-year correspondence project — authoritative but Union-sourced and High-Water-Mark-framed (see ED-5). | ±31 m rms (georeference); ±10–20 m sheet-relative |
| **Verification** | Monument / flank-marker register (`reconstruction/spatial/monuments.json`) | Independent, veteran-adjudicated point checks on the Union side; nearly silent on the CSA side. Never primary; used to corroborate or flag. | ±5–15 m (coordinates); ±50 m (as line evidence) |
| **Casualty-spatial** | Elliott's burial map (1864, LOC, PD) | The only systematic spatial record of field burials + dead horses; feeds EC6 apportionment, not EC3 anchors. | ±100 m class (see below) |

## Acquisition and georeferencing (deliverable state)

- All **28 Bachelder sheets** (~381 MB JP2 masters, Rumsey scans via
  Internet Archive) plus **Elliott** (13.4 MB, LOC) are fetch-reproducible:
  `reconstruction/scripts/fetch_bachelder_maps.sh` (pinned URLs + sha256).
  `data/maps/` stays gitignored; the committed record is
  `reconstruction/spatial/bachelder-manifest.json` (per-sheet provenance,
  license basis, sha256, dimensions, transform, residuals).
- **Transforms** use the authoring tool's similarity convention
  (`tool/src/overlay.ts`; Python port with least-squares + composition:
  `reconstruction/scripts/georef_maps.py`, 15 tests). Per-sheet
  `sheet pixel -> battlefield-local meters` via `load_manifest()`.
- **Reference sheet** (No. 8, 1–5 PM July 3): fit on five
  road-intersection ties (Peach Orchard, Taneytown×Wheatfield, the
  Diamond, Fairfield×Black Horse Tavern, Mummasburg×Herrs Ridge), 31 m
  rms; Codori/Bryan building checks 22/25 m. Residuals mix pin error,
  1863-vs-modern junction drift, and paper distortion; an affine fit
  halves road-junction residuals but was **not** adopted (overlay.ts
  parity; revisit if EC3 ever needs <20 m absolute from these sheets).
- **Other 22 main-field sheets**: registered to the reference by patch
  phase-correlation on the shared Warren base (1.6–19 px rms ≈ 2–18 m),
  then composed. July 1 sheets show up to 3% print/paper scale variance
  vs the reference — the per-sheet transform absorbs it.
- **ECF sheets**: two-tie solve on the Maxson 1880 base
  (Hanover×Low Dutch + Rummel farm; Hoffman×Hanover check ~20 m),
  PROVISIONAL ±75 m class; coarser scan (~1.28 m/px). East Cavalry Field
  lies off the heightmap square (local_x > 8507) — the frame extends by
  the same UTM offset.
- **Elliott**: two-tie solve (Diamond + Peach Orchard); Codori check
  77 m → treat all feature positions as **±100 m class**.

## Per-source uncertainty guidance for EC3 radii

- **Bachelder main-field sheet, absolute**: radius ≥ max(60 m
  [2× ref rms], legibility of the feature being read). Use for units
  placed against bare terrain.
- **Bachelder, terrain-relative** (the normal case): when the sheet
  shows a unit against a drawn feature (wall, road, orchard edge) that
  we can also fix in LiDAR/OSM, read the *relationship*, not the
  absolute plot: radius 15–30 m depending on bar/feature crispness.
  This is the recommended default for EC3 anchors from these sheets.
- **Bachelder, unit-to-unit on one sheet** ("on Woodruff's right"):
  10–20 m class (registration-level), inherits the reference-frame
  unit's own anchor radius.
- **Time envelope**: a sheet position is EC3 evidence for the *whole
  printed window* (e.g. 1–5 PM) unless the drawn state pins it tighter
  (sheet 8 draws the assault in motion — mid-charge, not
  window-start). Cite sheet id + list number; tier E by default,
  upgraded when the depicted action ties to a canonical anchor.
- **Monuments**: ±50 m as sole anchor; ±5–15 m as a *check* on an
  anchor fixed by documents. Bind to `positionSemantics` and its
  moment. CSA side: tablets/Bachelder instead (register is
  Union-asymmetric).
- **Elliott**: EC6 only — never an EC3 anchor. Burial rows attach
  casualties to *sectors* (±100 m), not points.

## Elliott assessment (EC6 use)

**For:** 8,352 individual burial locations + 345 dead horses, surveyed
"by transit and chain" and published 1864 — the only systematic spatial
record of where the dead lay (Frassanito: "the first professional
post-battle survey of the field"). Dead-horse clusters are strong
independent evidence for battery positions under fire (EC5/EC6
crossover); Confederate trench rows (largely undisturbed until the
1871–73 Weaver removals) spatially apportion CSA dead by sector —
exactly what EC6's episode-distribution needs on the side where our
casualty returns are weakest.

**Against (honest cautions, from the literature):** Elliott could not
have reached Gettysburg before late January 1864, by which time most
Union dead had been moved to the National Cemetery — the Union graves
plot is secondhand (Wills-associated surveys), not his fieldwork; the
Rose farm sector demonstrably undercounts documented burial
concentrations ("not even in the ballpark" per modern battlefield-guide
analysis); parts of the base appear copied from an 1857–58 county
survey; only 17 of 8,352 graves are named; and Elliott's own biography
(the ACHS "railroad swindler" profile) counsels against treating the
publication as disinterested. Sources: Adams County Historical
Society / gettysburghistory.org S.G. Elliott profile; American
Battlefield Trust Elliott map page; civilwartalk Rose-farm threads.

**Ruling proposed:** use Elliott for *relative spatial apportionment*
of casualties within a unit's known sector (which end of the line bled)
and for dead-horse corroboration of battery positions; never for
absolute counts (trust OR returns) and never as a position anchor.
Where Elliott contradicts documented burial records (Rose farm), the
documents win and the conflict is recorded.

## How a dossier author consumes this

1. **EC3 anchor from a Bachelder sheet:** open the sheet via the
   manifest (`load_manifest()['j3-03'].to_local(px, py)`), read the
   unit bar's position *relative to a drawn terrain feature*, convert,
   record: sheet id + Rumsey list no., pixel coords read, radius per
   the guidance above, time envelope = printed window (tier E unless
   anchored tighter).
2. **Verify (Union units):** look the unit up in
   `reconstruction/spatial/monuments.json`; if a marker exists, compare
   against the anchor with the marker's `positionSemantics` moment in
   mind. Agreement within class: cite as corroboration. Disagreement:
   encode both readings + ED if adopted (the 72nd PA pattern).
3. **EC6 shaping:** consult Elliott's sector around the unit's ground
   for burial rows / dead horses; use to weight the casualty
   distribution across episodes, cite with the ±100 m caveat.
4. **Cross-sheet movement:** a unit drawn on consecutive sheets gives
   window-bracketed positions in one metric frame — movement legs (EC4)
   inherit endpoints from the two sheets' windows.

## Rights posture (summary)

Manifest carries the full per-sheet statement. Short form: Warren 1873
base PD; Bachelder overlay first published 1995 (17 USC 303(a)
discussion in the 2026-07-02 acquisition report §3.2 — unresolved,
owner-accepted for research use); Rumsey scan formally CC BY-NC-SA 3.0
with a broader publication grant on Rumsey's permissions page, credit
"David Rumsey Map Collection, David Rumsey Map Center, Stanford
Libraries"; positions read off the sheets are uncopyrightable facts
(the repo's Feist framing). Elliott: published 1864, PD, LOC scan.
Full-res rasters: gitignored, fetch-reproducible. Committed images:
low-res documentation crops only
(`docs/research/assets/spatial-evidence/`).
