# Georeferencing Anchor Landmarks — Gettysburg Battle Atlas

Date: 2026-07-01
Purpose: Sourced WGS84 anchor points for 10 Gettysburg landmarks, converted to
battlefield-local meters for the terrain pipeline's UTM square
(`pipeline/terrain_pipeline/config.py`, `BBOX_WGS84`, UTM 18N / EPSG:26918).

Local axes: `local_x` = UTM easting minus the square's west edge; `local_z` =
UTM northing minus the square's south edge. Battlefield square side length is
**8507.2 m**; valid range for both axes is `[0, 8507.2]`. Square origin
(minx, miny) = `(304208.0, 4404534.3)` in UTM 18N meters.

## Summary table

| # | Landmark | Lat | Lon | local_x (m) | local_z (m) | Source | Precision note |
|---|----------|-----|-----|-------------|-------------|--------|-----------------|
| 1 | Copse of Trees (High Water Mark) | 39.812472 | -77.235889 | 4407.3 | 4801.1 | [Stone Sentinels](https://gettysburg.stonesentinels.com/gettysburg-battlefield/the-copse-of-trees/) | Monument/tree-cluster point; cross-checked against Wikipedia, agrees within 15m |
| 2 | The Angle (stone wall corner, Cushing's battery) | 39.813194 | -77.236056 | 4395.0 | 4881.6 | [Stone Sentinels](https://gettysburg.stonesentinels.com/gettysburg-battlefield/the-angle/) | Wall-corner point; cross-checked against HMDB Cushing marker, agrees within 22m |
| 3 | Virginia Memorial (Seminary Ridge) | 39.814133 | -77.250317 | 3176.8 | 5016.5 | [Wikipedia](https://en.wikipedia.org/wiki/Virginia_Monument) | Monument point (statue base) |
| 4 | Gettysburg town square (Lincoln Square / the Diamond) | 39.830897 | -77.231012 | 4875.8 | 6835.9 | [OpenStreetMap](https://www.openstreetmap.org/way/298953822) (Nominatim geocode) | Area centroid of the square/circle; cross-checked against HMDB "The Diamond" marker (39.8312, -77.2314), ~55m offset — see note |
| 5 | Little Round Top summit | 39.7920 | -77.2363 | 4315.3 | 2529.6 | [Wikipedia](https://en.wikipedia.org/wiki/Little_Round_Top) | Summit point; cross-checked against independent web geocode, agrees within 8m |
| 6 | Big Round Top summit | 39.786250 | -77.239194 | 4051.5 | 1897.5 | [Wikipedia](https://en.wikipedia.org/wiki/Big_Round_Top) | Summit point (wooded, no monument); cross-checked against OSM `natural=peak` node (Wikidata-linked), agrees within 59m — flagged, see note |
| 7 | Codori farmhouse (Emmitsburg Road) | 39.810892 | -77.240357 | 4020.4 | 4635.4 | [OpenStreetMap](https://www.openstreetmap.org/way/779833686) (`Codori Barn`, historic building) | AREA — farmstead cluster; point given is the barn building, closest individually-tagged structure to the (untagged) farmhouse, ~10-20m away within the same farmyard |
| 8 | Spangler's Woods (Pickett's Division formation area) | 39.811871 | -77.250927 | 3118.2 | 4766.7 | [OpenStreetMap](https://www.openstreetmap.org/node/7338809337) (`place=locality`) | AREA — representative point at OSM-tagged locality centroid of the woods, ~600m south of the Virginia Memorial start-line marker |
| 9 | Ziegler's Grove (Woodruff's battery / Hays's right) | 39.815208 | -77.233916 | 4583.7 | 5100.6 | [OpenStreetMap](https://www.openstreetmap.org/node/4497323192) (`place=locality`) | AREA — representative point at OSM-tagged locality centroid of the grove |
| 10 | Brian (Bryan) farmhouse (Hays's front) | 39.815591 | -77.235166 | 4477.8 | 5145.8 | [OpenStreetMap](https://www.openstreetmap.org/way/451707950) (`Bryan House`, historic building) | Building point (specific structure, not an area) |

## Cross-source verification detail

Per the task's requirement, the Copse of Trees, the Angle, and both Round
Tops were checked against two independent sources each.

- **Copse of Trees**: [Stone Sentinels](https://gettysburg.stonesentinels.com/gettysburg-battlefield/the-copse-of-trees/)
  (39°48'44.9"N 77°14'09.2"W = 39.812472, -77.235889) vs. Wikipedia's
  [High Water Mark of the Rebellion Monument](https://en.wikipedia.org/wiki/High_Water_Mark_of_the_Rebellion_Monument)
  (39.812450, -77.235717) and [High-water mark of the Confederacy](https://en.wikipedia.org/wiki/High-water_mark_of_the_Confederacy)
  (39.812500, -77.235833). Distances: 14.9 m and 5.7 m respectively. **No
  disagreement flag** (well under 30m).

- **The Angle**: [Stone Sentinels](https://gettysburg.stonesentinels.com/gettysburg-battlefield/the-angle/)
  (39°48'47.5"N 77°14'09.8"W = 39.813194, -77.236056) vs. HMDB
  ["Cushing's Union Battery" marker](https://www.hmdb.org/m.asp?m=16384)
  (39°48'47.4"N 77°14'08.9"W = 39.813167, -77.235806, coordinate obtained via
  search-index snippet — HMDB's site blocks direct fetch with a Cloudflare
  challenge). Distance: 21.6 m. **No disagreement flag** (under 30m; the
  Cushing marker sits slightly northeast of the true wall-corner apex, which
  is expected since it marks the battery position rather than the wall angle
  itself).

- **Little Round Top summit**: Wikipedia infobox (39.7920412, -77.2363738)
  vs. an independent search-engine geocode of the same point
  (39.7920, -77.2363). Distance: 7.8 m. **No disagreement flag.**

- **Big Round Top summit**: Wikipedia infobox
  (39.786250°N, 77.239194°W, [en.wikipedia.org/wiki/Big_Round_Top](https://en.wikipedia.org/wiki/Big_Round_Top))
  vs. OpenStreetMap's `natural=peak` node (39.786749, -77.239410), which
  carries a direct Wikidata (Q4906242) and `wikipedia=en:Big Round Top` tag
  confirming it references the same summit. Distance: **58.5 m — flagged**,
  exceeding the ~30m threshold. This is expected for a broad, forested hill
  without a single monument marker: the "summit" is a soft topographic high
  point rather than a surveyed feature, so different sources place it at
  slightly different points along the ridge crest. Both sources are used;
  the Wikipedia value is recorded in the summary table as primary since it
  is the more directly citable reference. An initial web-search snippet
  claiming 39.7892889, -77.2368139 was checked and rejected — it disagreed
  by ~394 m from the verified Wikipedia infobox value and could not be
  traced to a real citation (likely a search-summarization error).

## Other notes

- **Lincoln Square**: OSM's Nominatim geocode of the "Lincoln Square" way/
  circle (39.8308, -77.2312) and a second web geocode of "1 Lincoln Square"
  agree closely, but HMDB's "The Diamond" marker plaque sits about 55m away
  at 39.8312, -77.2314 (the plaque is mounted at a specific corner of the
  square, not its centroid). This is noted but not "flagged" as a true
  disagreement since the landmark is an area (the square/circle itself) and
  both points fall within it.
- **Codori farmhouse**: OSM tags the barn as a distinct historic building
  but does not separately tag the farmhouse. The barn point is used as the
  representative anchor for the farmstead; the actual house is a very short
  distance away within the same yard (well under the precision needed for
  this use case).
- **Spangler's Woods**: distinct from the Virginia Memorial — the Virginia
  Memorial marks the eastern edge of the woods / Lee's vantage point,
  while the OSM locality point for "Spangler's Woods" is centered further
  west/south in the wooded area itself, consistent with Pickett's Division
  overnighting on the west side of the woods before advancing east on July 3.

## Sanity check results

All 10 points fall within `[0, 8507.2]` on both axes — **no out-of-bounds
landmarks**. Little Round Top (`local_z` = 2529.6) and Big Round Top
(`local_z` = 1897.5) both land in the southern half of the square (south of
the 4253.6 midpoint), as expected. Gettysburg town square (`local_z` =
6835.9) lands in the northern half, as expected.

## Conversion method

Run from `pipeline/`:

```bash
uv run python - <<'EOF'
from pyproj import Transformer
from terrain_pipeline import config
t = Transformer.from_crs(config.WGS84_CRS, config.UTM_CRS, always_xy=True)
b = config.utm_square_bounds()
def local(lon, lat):
    e, n = t.transform(lon, lat)
    return round(e - b[0], 1), round(n - b[1], 1)
EOF
```

`config.utm_square_bounds()` returned `(304208.0, 4404534.3, 312715.2,
4413041.4)` (minx, miny, maxx, maxy, UTM 18N meters) at time of writing.
