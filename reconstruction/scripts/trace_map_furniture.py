#!/usr/bin/env python3
"""Generate data/map-furniture/map-furniture.json (docs/format/map-
furniture-format.md) from full-resolution sheet-pixel picks read off the
Bachelder Third Day reference sheet (j3-03 / archive.org 12440022.jp2,
Rumsey/Stanford scan; bachelder-manifest.json, 31.0 m rms absolute).

This is the roads/hydrology/town-of-Gettysburg/railroad extension
landcover-format.md's "Planned extensions" section reserved — traced from
the same period-derived, PD/CC-BY source class as landcover.json (never
from copyrighted modern cartography).

Pipeline: SHEET_PX (below) is a committed ledger of full-sheet pixel
coordinates, read via `reconstruction/scripts/crop_sheet.py` crops of
`data/maps/bachelder/12440022.jp2` at scale factors of 0.16 (whole-field
overview) to 0.75 (town detail) — dossier-pass reading discipline (pixel
picks at bend/junction points, not every meander; see spatial-evidence.md
"Bachelder main-field sheet" guidance). Each point is converted through
`georef_maps.load_manifest()['j3-03'].to_local()`, rounded to 0.1 m,
and clipped to the valid [0, 8507.2] square (the sheet extends past the
heightmap square on three sides; there is no terrain to drape past the
edge).

Determinism: SHEET_PX and FEATURE_META below are the only inputs; running
this script twice against the same manifest produces byte-identical output
(reconstruction/tests/test_map_furniture.py::test_determinism).

Usage:
    uv run --with pillow python reconstruction/scripts/trace_map_furniture.py
    (does not need pillow itself; --with pillow keeps the invocation
    consistent with crop_sheet.py's environment. A plain
    `python3 reconstruction/scripts/trace_map_furniture.py` works too.)
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(Path(__file__).resolve().parent))
from georef_maps import load_manifest  # noqa: E402

OUT_PATH = ROOT / "data" / "map-furniture" / "map-furniture.json"
SHEET_ID = "j3-03"
BOUND = 8507.2

SOURCE_J3_03 = (
    "Bachelder Third Day sheet No. 8, 1-5 PM July 3 (j3-03, archive.org "
    "dr_battle-field-of-gettysburg-no-3-no-8-1-5-pm-july-3-1863-12440022, "
    "Rumsey/Stanford scan) -- the Warren 1873 engineer base under "
    "Bachelder's troop overlay; traced via reconstruction/scripts/"
    "crop_sheet.py crops + georef_maps.SheetGeoref.to_local "
    "(bachelder-manifest.json j3-03, 31.0 m rms absolute)."
)

# Full-resolution j3-03 pixel picks, in course order (line) or ring order
# (polygon). Read from crop_sheet.py crops at these scales: overview 0.16,
# town 0.7, nw/ne/sw/se 0.32, rrcut 0.75, south 0.65, rockcreek 0.30 --
# ledger kept for audit in docs/reconstruction/map-furniture-slice.md.
SHEET_PX: dict[str, list[tuple[float, float]]] = {
    "road-chambersburg-pike": [(3452.0, 3223.7), (3852.0, 3297.0), (4278.7, 3390.3), (4718.7, 3497.0), (5118.7, 3603.7), (5532.0, 3710.3), (4390.0, 3857.1), (4761.4, 3950.0), (5132.9, 4014.3), (5504.3, 4064.3)],
    "road-mummasburg": [(4899.5, 950.8), (4274.5, 1450.8), (3712.0, 1982.0), (3180.8, 2388.2), (2712.0, 2794.5), (2399.5, 3107.0), (4961.4, 3307.1), (5275.7, 3564.3), (5461.4, 3792.9), (5575.7, 3964.3)],
    "road-carlisle": [(5654.3, 2850.0), (5658.6, 3207.1), (5664.3, 3564.3), (5671.4, 3921.4), (5677.1, 4137.1)],
    "road-harrisburg": [(4739.4, 148.0), (4911.2, 960.5), (5317.5, 1710.5), (5567.5, 2335.5), (5755.0, 2960.5), (5817.5, 3429.2), (5932.9, 2907.1), (5804.3, 3392.9), (5732.9, 3778.6), (5677.1, 4137.1)],
    "road-hanover": [(5677.1, 4137.1), (6032.9, 3921.4), (6390.0, 3764.3), (6818.6, 3650.0), (6286.2, 3335.5), (7067.5, 3116.8), (8161.2, 2804.2), (9098.8, 2429.2), (10036.2, 1960.5)],
    "road-fairfield": [(3452.0, 4777.0), (3878.7, 4577.0), (4318.7, 4390.3), (4718.7, 4217.0), (5158.7, 4137.0), (4390.0, 4492.9), (4818.6, 4392.9), (5247.1, 4264.3), (5418.6, 4535.7)],
    "road-emmitsburg": [(5418.6, 4535.7), (5332.9, 4850.0), (5247.1, 5207.1), (5182.9, 5397.1), (5464.0, 5251.0), (4839.0, 6032.2), (4151.5, 6657.2), (3839.0, 7188.5)],
    "road-taneytown": [(4590.0, 4138.3), (4728.5, 4507.5), (4836.2, 4907.5), (4943.8, 5353.7), (5051.5, 5815.2), (5143.8, 6230.6), (5236.2, 6661.4), (5328.5, 7199.8)],
    "road-baltimore-pike": [(4590.0, 4138.3), (4990.0, 4569.1), (5390.0, 4907.5), (5790.0, 5246.0), (6159.2, 5584.5), (6559.2, 5969.1), (6990.0, 6384.5), (7436.2, 6784.5), (7897.7, 7199.8), (8282.3, 7661.4), (8666.9, 8122.9)],
    "rail-finished-hanover-branch": [(5677.1, 4137.1), (5932.9, 3964.3), (6218.6, 3850.0), (6532.9, 3735.7), (6911.2, 3523.0), (7848.8, 3273.0), (8942.5, 2898.0), (10036.2, 2398.0)],
    "rail-unfinished-cut-west": [(3532.0, 3417.0), (4118.7, 3577.0), (4652.0, 3777.0), (4985.3, 3950.3), (5185.3, 4083.7), (4390.0, 4064.3), (4675.7, 4164.3), (4961.4, 4235.7)],
    "stream-rock-creek": [(5985.7, 1493.0), (6035.7, 2159.7), (6202.3, 2826.3), (6435.7, 3359.7), (6635.7, 3826.3), (6769.0, 4326.3), (6869.0, 4659.7), (6935.7, 4993.0), (7002.3, 5293.0), (7069.0, 5559.7), (7002.3, 5826.3), (6935.7, 6093.0), (7069.0, 6359.7), (7202.3, 6559.7), (7882.0, 5230.5), (7632.0, 5824.2), (7475.8, 6449.2), (7632.0, 7074.2), (7882.0, 7699.2), (7944.5, 8324.2), (8100.8, 8949.2), (8413.2, 9574.2), (8569.5, 10199.2), (8632.0, 10824.2)],
    "stream-stevens-run": [(6002.3, 1543.0), (6069.0, 1993.0), (6202.3, 2493.0), (6335.7, 2893.0), (6402.3, 3293.0), (6502.3, 3493.0)],
    "stream-willoughby-run": [(3180.8, 1857.0), (3024.5, 2388.2), (2930.8, 3013.2), (3024.5, 3638.2), (3180.8, 4263.2), (3337.0, 4888.2), (3430.8, 5513.2), (3493.2, 6110.1), (2464.0, 5251.0), (2339.0, 6032.2), (2245.2, 6813.5), (2307.8, 7594.8), (2401.5, 8376.0), (2339.0, 9157.2), (2214.0, 9938.5), (2026.5, 10563.5), (1901.5, 11004.1)],
    "stream-plum-run": [(3745.2, 10094.8), (3839.0, 9469.8), (3964.0, 8844.8), (4151.5, 8219.8), (4370.2, 7594.8), (5444.5, 9574.2), (5694.5, 8949.2), (5819.5, 8324.2)],
    "stream-pitzers-run": [(2901.5, 6032.2), (2964.0, 6657.2), (3057.8, 7282.2), (3151.5, 7907.2), (3276.5, 8376.0)],
    "stream-marsh-creek": [(212.0, 5513.2), (464.0, 5438.5), (680.8, 5825.8), (1089.0, 5532.2), (1714.0, 5657.2), (2339.0, 5563.5)],
    "stream-winebrenners-run": [(6743.8, 4046.0), (6897.7, 4353.7), (7051.5, 4661.4), (7205.4, 4969.1)],
    "lane-codori": [(4889.0, 6496.0), (4790.0, 6430.0)],
    "lane-bryan": [(5321.0, 5969.0), (5220.0, 5760.0)],
    "town-block-nw": [(5147.1, 3650.0), (5668.6, 3650.0), (5668.6, 3964.3), (5147.1, 3964.3)],
    "town-block-ne": [(5668.6, 3650.0), (6132.9, 3692.9), (6032.9, 3992.9), (5668.6, 3964.3)],
    "town-block-diamond": [(5575.7, 3964.3), (5761.4, 3964.3), (5761.4, 4207.1), (5575.7, 4207.1)],
    "town-block-sw-upper": [(5147.1, 3964.3), (5668.6, 3964.3), (5668.6, 4207.1), (5147.1, 4207.1)],
    "town-block-se-upper": [(5668.6, 3964.3), (6032.9, 3992.9), (6032.9, 4207.1), (5668.6, 4207.1)],
    "town-block-sw-lower": [(5247.1, 4207.1), (5668.6, 4207.1), (5668.6, 4392.9), (5247.1, 4392.9)],
    "town-block-se-lower": [(5668.6, 4207.1), (5990.0, 4207.1), (5990.0, 4392.9), (5668.6, 4392.9)],
    "town-block-south": [(5390.0, 4392.9), (5747.1, 4392.9), (5747.1, 4707.1), (5390.0, 4707.1)],
}

# id -> (kind, cls, confidence, note). source is SOURCE_J3_03 for every
# "documented" feature (the whole ledger is one sheet); lanes are
# "inferred" (one end anchored at a manifest tie point / building read,
# the other snapped toward the nearest traced road rather than
# independently paced out -- see the format doc's confidence rule).
FEATURE_META: dict[str, tuple[str, str, str, str]] = {
    "road-chambersburg-pike": ("line", "pike", "documented",
        "WNW turnpike into town, past the Lutheran Seminary and E. McPherson's; the First Day Iron Brigade corridor."),
    "road-mummasburg": ("line", "road", "documented",
        "NW road toward Mummasburg, split off north of the Chambersburg Pike near Oak Ridge."),
    "road-carlisle": ("line", "road", "documented",
        "Due-north street/road out of the Diamond, labeled 'CARLISLE' on the sheet."),
    "road-harrisburg": ("line", "road", "documented",
        "N/NNE road out of town past Cobean/Ross/Yeatts, bearing toward Harrisburg."),
    "road-hanover": ("line", "road", "documented",
        "ENE road+rail corridor out of town labeled 'HANOVER' on the sheet, past David Schaffer's; York Road's own split "
        "was not separately legible at this reading scale and is merged into this traced corridor near town (residual, "
        "see map-furniture-slice.md)."),
    "road-fairfield": ("line", "road", "documented",
        "SW road out of town, labeled 'HAGERSTOWN ROAD' on the sheet (the Fairfield Road's period alternate name -- it "
        "continues to Hagerstown, MD); passes Black Horse Tavern (manifest tie point) at the traced extent's SW end."),
    "road-emmitsburg": ("line", "road", "documented",
        "SSW road out of town toward the Peach Orchard crossroads (manifest tie point); Sickles' III Corps axis."),
    "road-taneytown": ("line", "road", "documented",
        "Due-south road out of town toward the Taneytown x Wheatfield Road crossroads (manifest tie point); Meade's HQ axis."),
    "road-baltimore-pike": ("line", "pike", "documented",
        "SE turnpike out of town, curving along Cemetery/Culp's Hill's east base toward Rock Creek and Two Taverns."),
    "rail-finished-hanover-branch": ("line", "rail_finished",
        "documented", "Gettysburg & Hanover branch, in service at the time of the battle; runs the same ENE corridor as the Hanover Road."),
    "rail-unfinished-cut-west": ("line", "rail_unfinished", "documented",
        "The graded, unrailed cut west of town the First Day fighting turned on (Iron Brigade vs. Davis's Brigade, the Railroad Cut); "
        "runs just north of and roughly parallel to the Chambersburg Pike through the McPherson's Ridge corridor into town near Penn College."),
    "stream-rock-creek": ("line", "creek", "documented",
        "Main eastern watercourse; traced from its north entry past David Schaffer's, through the Stevens Run confluence, "
        "around Culp's Hill's east base, south past Benner's Hill and George Bushman's toward the SE corner of the traced square."),
    "stream-stevens-run": ("line", "run", "documented",
        "Tributary flowing SE past Josiah Benner's and Thomas Goldy's into Rock Creek NE of town."),
    "stream-willoughby-run": ("line", "run", "documented",
        "West of Seminary Ridge, First Day fighting axis; traced N-S the full length visible on the sheet through the traced square."),
    "stream-plum-run": ("line", "run", "documented",
        "Drains the Round Tops/Wheatfield sector north toward the Peach Orchard corridor; traced at coarser grain (dense contour ink over "
        "Devil's Den/the Wheatfield made the centerline harder to isolate at this reading scale -- residual, see map-furniture-slice.md)."),
    "stream-pitzers-run": ("line", "run", "documented",
        "Small stream east of and roughly parallel to Willoughby Run, near Pitzer's Woods."),
    "stream-marsh-creek": ("line", "creek", "documented",
        "SW corner watercourse near Black Horse Tavern (manifest tie point); a mill-pond/oxbow shape is drawn at the tavern crossing but "
        "was not separately traced as a polygon at this reading scale (residual)."),
    "stream-winebrenners-run": ("line", "run", "documented",
        "Minor stream SE of town near the Baltimore Pike corridor; traced at low point density (short, faint watercourse on the sheet)."),
    "lane-codori": ("line", "lane", "inferred",
        "Short farm lane from the Emmitsburg Road to the N. Codori farmstead (Pickett's Charge axis); west end anchored at the manifest's "
        "codori-barn check tie point (image px 4889,6496; landmark-anchors doc #7), east end snapped toward the traced Emmitsburg Road "
        "rather than independently paced -- confidence inferred for the whole short connector."),
    "lane-bryan": ("line", "lane", "inferred",
        "Short farm lane from the Taneytown Road to the Bryan farmstead (the Angle/Copse of Trees); anchored at the manifest's bryan-farm "
        "check tie point (image px 5321,5969; landmark-anchors doc #10), east/west connector snapped rather than independently paced."),
    "town-block-nw": ("polygon", "town_block", "documented", "NW quadrant block cluster (Chambersburg St side, north of the crossing)."),
    "town-block-ne": ("polygon", "town_block", "documented", "NE quadrant block cluster (York/Carlisle corner, north of the crossing)."),
    "town-block-diamond": ("polygon", "town_block", "documented", "The Diamond (Lincoln Square) block, centered on the four-road crossing."),
    "town-block-sw-upper": ("polygon", "town_block", "documented", "SW quadrant block cluster, immediately south-west of the Diamond."),
    "town-block-se-upper": ("polygon", "town_block", "documented", "SE quadrant block cluster, immediately south-east of the Diamond."),
    "town-block-sw-lower": ("polygon", "town_block", "documented", "SW quadrant block cluster, second row south of the Diamond."),
    "town-block-se-lower": ("polygon", "town_block", "documented", "SE quadrant block cluster, second row south of the Diamond."),
    "town-block-south": ("polygon", "town_block", "documented",
        "Southward extension of the built-up grid along Baltimore St toward Cemetery Hill."),
}

# Block-cluster simplification is a deliberate massing choice, not
# per-building detail -- see docs/reconstruction/map-furniture-slice.md.
NOTE_METHOD = (
    "Street-block MASSING, not individual buildings: each polygon "
    "approximates a cluster of adjoining blocks bounded by the sheet's "
    "drawn main streets, simplified to a quad at this reading scale. "
    "See docs/reconstruction/map-furniture-slice.md 'town massing "
    "simplification'."
)


def clip_points(points: list[tuple[float, float]]) -> list[tuple[float, float]]:
    """Drop points outside the valid [0, BOUND] square (the sheet extends
    past the heightmap square on three sides; there's no terrain to drape
    past the edge). All observed out-of-range points sit at line ends, so a
    simple keep-in-bounds filter does not fragment any traced course."""
    return [(x, z) for (x, z) in points if 0.0 <= x <= BOUND and 0.0 <= z <= BOUND]


def build_features() -> list[dict]:
    manifest = load_manifest()
    sheet = manifest[SHEET_ID]
    features = []
    for fid in sorted(SHEET_PX):  # stable, deterministic order
        kind, cls, confidence, note = FEATURE_META[fid]
        local_pts = [sheet.to_local(px, py) for (px, py) in SHEET_PX[fid]]
        rounded = [(round(x, 1), round(z, 1)) for (x, z) in local_pts]
        clipped = clip_points(rounded)
        min_pts = 3 if kind == "polygon" else 2
        if len(clipped) < min_pts:
            raise ValueError(
                f"{fid}: only {len(clipped)} in-bounds points after "
                f"clipping, need >= {min_pts}")
        feature = {
            "id": fid,
            "kind": kind,
            "cls": cls,
            "points": [[x, z] for (x, z) in clipped],
            "confidence": confidence,
        }
        if confidence == "documented":
            feature["source"] = SOURCE_J3_03
        full_note = note
        if cls == "town_block":
            full_note = f"{note} {NOTE_METHOD}"
        feature["note"] = full_note
        features.append(feature)
    return features


def main() -> None:
    doc = {
        "name": "Gettysburg 1863 map furniture -- roads, hydrology, town, railroad "
                "(Warren 1873 base, Bachelder sheet j3-03)",
        "features": build_features(),
    }
    OUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    with open(OUT_PATH, "w") as f:
        json.dump(doc, f, indent=1)
        f.write("\n")
    print(f"wrote {OUT_PATH} ({len(doc['features'])} features)")


if __name__ == "__main__":
    main()
