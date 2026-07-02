# 1863 Land-Cover Sources — Research Report

**Verdict: trace it ourselves in the authoring tool.** No public 1863 land-cover vector dataset is confirmed to exist. (Deep-research pass: 20 sources, 72 claims, 25 verified — 21 confirmed, 4 refuted. URLs verified 2026-07-01.)

## What does NOT exist (verified dead ends)
- **Knowles/Smithsonian 2013 GIS**: reconstructed subtractively from the Warren map + modern data (orchards, farms, forests, wooden fences, stone walls, buildings) — but a bespoke project product with **no confirmed public download**. Acquisition path (contact Knowles at Middlebury/UMaine, Alex Tait/International Mapping, or Esri) unverified — optional side quest, non-blocking. Two plausible claims about its internals were refuted 0-3 (bare-TIN viewshed; light-table contour tracing) — do not repeat them.
- **NPS DataStore/data.gov products**: all three retrievable Gettysburg GIS products are wrong-era — VMI vegetation is modern (2007), GRI is 1929-derived bedrock geology, Tract & Boundary is ownership parcels. 3-0 verified dead ends.
- **NPS internal 1863 base maps**: documented to exist (post-1999 rehabilitation program digitized 1863 woodlots/orchards/thickets/fencelines from period maps + battle-era photos) but not downloadable — request/FOIA path only. Optional side quest.

## What we trace from (public domain, already in the library)
1. **Warren survey** (LOC 99448794, PD, free-to-use; credit "Library of Congress, Geography and Map Division"): the terrain/land-cover base — **with the verified caveat that it surveys 1868-69, not 1863** ("a better map of 1868 than 1863 Gettysburg"). Woodlots and fences 5-6 years post-battle; cross-check battle-critical areas against Bachelder.
2. **Bachelder day sheets** (LOC 99447492, PD, 1:12,000): period-derived; the 1876 companion states vegetation, fences, and houses are depicted "as near as possible as it was at the time of the battle." Primary source for battle-date deviations from Warren.
3. **NPS's own method validates ours**: the park derives 1863 fences/stone walls from exactly these maps plus battle-era photography (Gardner, Brady, Gutekunst). We trace the same sources.
4. **McElfresh watercolor maps (©1994)**: carry the crop-level detail (wheat vs corn at the Codori/Emmitsburg Road fields) — **copyrighted; facts-only cross-check under Feist, never traced.** Open question whether Warren/Bachelder distinguish crops at all; if not, crop typing is fact-checked prose → provenance-tagged attributes, not traced geometry.
5. **Rumsey scans**: skip — LOC scans of the same PD maps sidestep any license ambiguity (the claim that Rumsey asserts CC-NC on these was itself refuted 0-3, but LOC is cleaner regardless).

## Provenance rule for traced polygons
Every polygon/line carries: source map (Warren | Bachelder sheet N | photo ref), confidence (`documented` = drawn on a period-derived map; `inferred` = interpolated/disambiguated between sources), and the 1868-vs-1863 caveat where Warren is the sole source.

## Open questions (side quests, non-blocking)
- Knowles dataset by direct contact; NPS 1863 vectors by request/FOIA.
- Crop-type detail in Warren/Bachelder legends (inspect the scans at full zoom during tracing).
- OpenHistoricalMap 1863 coverage (named in the brief, unverified either way).
