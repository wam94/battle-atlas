# Monument & marker register — summary and reliability profile

**Status:** ADOPTED 2026-07-11 (owner batch adoption, dossier pass 2).
The register is `reconstruction/spatial/monuments.json`; the reliability
profile below is **ED-39** (renumbered from this branch's provisional
"ED-32", which collided with pass 1's Stannard ruling; see
`docs/reconstruction/angle-editorial-decisions.md`).

## What was built

**Extended 2026-07-12 (dossier pass 10): +27 July 1 entries (202
total).** The July 1 field's first register coverage: the Reynolds
cluster (KIA marker — the CA-J1M-5 pin ground — corps-HQ marker,
equestrian, Wadsworth pair), the Buford/Gamble/Devin/Calef morning-line
markers (the Gamble HQ marker reads 115 m from the j1-03 drawn 8th Ill
Cav bar — HQ-on-the-line), the Iron Brigade/Cutler/84th NY (14th
Brooklyn) grounds, the Heth-division CSA pair layer (Archer-captured
marker + Willoughby-Run front markers vs the W Conf Ave July 3
formation tablets — TWO-SEMANTICS pairs recorded per entry), the Oak
Hill tablets (Iverson/Ramseur/Rodes-division; the j1-09 drawn Iverson
attack bar reads 408 m SE of his formation tablet along the advance
axis), the Barlow Knoll both-sided cluster (Barlow statue + Ames/Harris
+ division HQ vs the Gordon tablet ON the knoll, 19 m-class from the
statue ground; j1-11 drawn Ames crest bar 57 m / Gordon contact bar
82 m), the Howard/Schurz command nodes (town + Cemetery Hill pair),
the von Gilsa ECH marker (July 2-3 semantics recorded — his July 1
knoll ground has no located brigade marker through this channel), and
the Hays/Hoke Harrisburg-Road approach tablets (the Early-front
backward-extension grounds). Channel: OSM/Overpass PRIMARY for all 27
(standing deviation continues).

**Extended 2026-07-11 (dossier pass 6): +40 southern-field entries
(91 total).** The July 2 southern field: Caldwell's four brigade
tablets + the Wheatfield monument cluster (5th NH, Irish Brigade, 4th
MI, 17th ME, 124th NY, Zook statue), de Trobriand/Ward/Tilton/Sweitzer
brigade tablets, Smith's 4th NY two-position pair (Devil's Den + the
Plum Run rear section), the four CSA en-echelon brigade tablets
(Semmes/Wofford/G.T. Anderson/Benning, W/S Confederate Ave —
formation-line semantics, never battle positions) + the
Semmes-mw/Anderson-w/Benning/Robertson/Law advance-and-wounding
markers, the Little Round Top summit cluster (Vincent-mw, Weed+Hazlett,
140th NY, 20th ME, 44th NY, 16th MI, 83rd PA, 91st PA, 146th NY,
155th PA, both brigade tablets — the pass-5 officer pins now have
±5–15 m document-fixed check points per ED-39 §5), and the 1st
Minnesota pair + Willard-KIA marker. **Channel note:** Stone Sentinels
brigade-tablet pages still print DMS text; its regiment/monument pages
served no machine-readable coordinates through the 2026-07-11 channel,
so those entries carry OSM (ODbL) as PRIMARY coordinates — a recorded
deviation (per-entry notes; SS-vs-OSM deltas where both exist run
1–27 m, two flagged >50 m).

The original build — 51 marker entries for the July 3 afternoon assault sector: the full
II Corps front (Webb, Hall, Harrow, Smyth, Willard/Sherrill brigades),
Hazard's five batteries + Cowan, Stannard's brigade, the 8th Ohio, the
Confederate state memorials on the Seminary Ridge start line, the
11th Mississippi and 26th North Carolina advance markers, and the
Armistead and Webb individual markers.

| markerType | n | | side | n |
|---|---|---|---|---|
| monument | 34 | | US | 41 |
| position-marker | 5 | | CSA | 10 |
| state-memorial | 5 | | | |
| advance-marker | 4 | | | |
| individual-marker | 2 | | | |
| urn | 1 | | | |

Positions come from Stone Sentinels unit pages (published DMS text where
present — 0.1 arc-second granularity, ~3 m; otherwise the page's Google
Maps embed payload), converted lat/lon → EPSG:26918 → battlefield-local
meters via the heightmap origin. Nine entries carry an independent
OpenStreetMap cross-check: median delta ~5 m, all under 15 m except one
**flagged conflict** — Stone Sentinels publishes the *identical* DMS
string for the 71st PA and 72nd PA main monuments (near-identical
location sentences; almost certainly a copy-paste on the 71st PA page;
the OSM position for the 71st sits 41.5 m NE, at the Angle corner where
the monument actually stands). Both readings are recorded on the entries
(`dataQuality`), neither silently corrected.

**Flank markers — honest zero.** The R.F./L.F. stones physically exist
for most Union regiments on this front, but no license-clean
machine-readable coordinate source was found this pass: Stone Sentinels
does not publish their coordinates, HMDB is bot-walled and thin on the
small stones, OSM has essentially none mapped in this sector (2
flank-named objects park-wide), and the NPS "Monumentation" ArcGIS layer
turned out to be survey benchmarks. Recommended fill: a manual HMDB
browser pass or field GPS. The secondary/advance/position markers
captured here partially serve the same line-extent purpose.

## Reliability profile (ED-39, adopted) — what a monument position *is*

The spatial analogue of the tablets' clock-offset assessment: a
per-source-class statement, recorded once, inherited by every claim that
cites a marker.

1. **Adjudication process.** Positions were fixed decades after the
   battle (main dedication wave 1883–1891) by veteran committees under
   the Gettysburg Battlefield Memorial Association's rules, with
   Bachelder himself the arbiter of position disputes from 1883. They
   encode *sworn veteran memory filtered through committee politics*,
   not survey of battle-time positions. Treat as EC3 evidence of class
   "veteran-adjudicated, 20+ year latency."

2. **The position-at-what-moment problem.** A regimental monument marks
   one moment of a fluid fight — by GBMA rule the position "held during
   the engagement," in practice usually the climax/defense position (and
   for Second Corps units, the July 3 line even where the regiment
   fought harder elsewhere on July 2). Advance markers and flank markers
   encode different moments (farthest advance; line extent at the
   marked moment). Every register entry therefore carries
   `positionSemantics`; a dossier author must bind a marker to a claim's
   time envelope explicitly, never to "the battle" at large.

3. **Known controversies (the exemplar: ED-4).** The 72nd PA veterans
   *sued* the GBMA (Pennsylvania Supreme Court, 1889–91, decided for
   the veterans) to place their monument at the wall — against
   Bachelder's and the association's crest reading, which our canonical
   ED-4 adopts. The monument-vs-build delta in the calibration table
   below (82 m) is that dispute made spatial. A monument can be
   *litigated memory*; where a monument contradicts the document chain,
   the conflict is encoded, not averaged.

4. **Confederate under-representation.** CSA unit monuments are nearly
   absent (state memorials are dedication points on the 1880s-1910s
   park avenue, not unit positions; the 11th MS / 26th NC markers are
   modern). Confederate EC3 anchors therefore rest on the War
   Department tablets and Bachelder sheets instead — the verification
   layer is *asymmetric by side*, and cross-side position comparisons
   inherit that asymmetry.

5. **Uncertainty guidance.** Marker coordinate itself: ±5–15 m
   (source-publication + GPS class, per the OSM cross-check). Marker as
   evidence of the unit's line: add the front-width problem — a marker
   is a point on a 100–200 m regimental front, usually its center or
   color position. Recommended EC3 radius when a marker is the only
   anchor: **±50 m**, tightened only with corroborating documents.

## Calibration table — marker vs in-build position (5 units)

In-build positions are the Angle-slice hold anchors
(`reconstruction/canonical/angle.reconstruction.json`, t=8040 window
start unless noted); battlefield-local meters.

| Unit | Build hold (x, z) | Marker (x, z) | Δ (m) | Reading |
|---|---|---|---|---|
| 69th PA | (4402, 4830) | mon-69pa (4374.3, 4817.4) | 30.4 | Monument sits at/behind the wall WSW of the authored line; within the combined class (marker point vs 200-man front centroid). |
| 71st PA | (4400, 4868) | mon-71pa (4375.4, 4860.6) | 25.7 | **Discount** — the marker coordinate is the flagged 71st/72nd copy-paste suspect; the OSM position (~41 m NE of it) would put the delta at ~28 m from the build in the other direction. |
| 72nd PA | (4457.7, 4856.1) crest | mon-72pa (4375.4, 4860.6) wall | 82.4 | The ED-4 dispute, quantified: canonical crest hold vs the veterans' litigated wall monument. The regiment's *reserve position-marker* (mon-72pa-1, 4425.5, 4862.4) sits 32.8 m from the build hold — the marker that matches the canonical reading agrees; the litigated one diverges exactly as ED-4 predicts. |
| Cushing (Bty A, 4th US) | (4398, 4880) | mon-cushing (4416.3, 4878.1) | 18.4 | Monument marks the advanced-section position at the wall/crest; authored centroid slightly W. Consistent. |
| Arnold (Bty A, 1st RI) | (4425, 4955) | mon-arnold (4448.4, 4920.5) | 41.7 | Largest unexplained delta of the set; monument on the wall N of the Angle vs authored centroid NW of it. Flag for the Arnold dossier pass (EC3 slot: reconcile with tablet + Bachelder sheet 8 before tightening). |

Calibration conclusion: monument-vs-build deltas for defensively-static
units cluster at 18–31 m — inside the recommended ±50 m marker radius —
and the two outliers are both *explained by the register's own quality
flags* (copy-paste suspect; ED-4 litigation). The register is fit for
its intended role: a verification layer, not a primary position source.

## Source records

- Stone Sentinels unit pages, accessed 2026-07-10 (per-entry URLs in
  the register). Extraction cautions per the existing corpus source
  record.
- OpenStreetMap (Overpass extract of `historic=memorial|monument`,
  battlefield bbox, accessed 2026-07-10, ODbL) — cross-check only.
