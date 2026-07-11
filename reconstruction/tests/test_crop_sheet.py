"""Tests for reconstruction/scripts/crop_sheet.py (pass-5 committed crop tool).

The transform pair must invert exactly (the similarity convention from
tool/src/overlay.ts: local = [a -b; b a] * (img_x, -img_y) + (tx, ty)),
and every manifest sheet must carry the georef fields the tool needs.
"""

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "reconstruction/scripts"))

import crop_sheet  # noqa: E402

MANIFEST = json.loads((ROOT / "reconstruction/spatial/bachelder-manifest.json").read_text())


def test_transform_round_trip_on_every_sheet():
    for sheet in MANIFEST["sheets"]:
        g = sheet["georef"]
        for ix, iy in [(0.0, 0.0), (4022.0, 7642.0), (5762.0, 8424.0)]:
            lx, lz = crop_sheet.img_to_local(g, ix, iy)
            rx, ry = crop_sheet.local_to_img(g, lx, lz)
            assert abs(rx - ix) < 1e-6 and abs(ry - iy) < 1e-6, sheet["id"]


def test_reference_tie_points_reproduce_within_fit_residual():
    # The manifest's reference tie points must map img->local under the
    # referenceFit similarity within the documented per-point residual
    # class (worst documented point is ~44 m).
    g = MANIFEST["referenceFit"]
    documented_worst = max(
        v for k, v in g["perPointResidualM"].items() if "(check)" not in k
    )
    worst = 0.0
    for tp in MANIFEST["referenceTiePoints"]:
        ix, iy = tp["img"]
        lx, lz = crop_sheet.img_to_local(g, ix, iy)
        ex, ez = tp["local"]
        d = ((lx - ex) ** 2 + (lz - ez) ** 2) ** 0.5
        worst = max(worst, d)
    assert worst <= documented_worst + 1.0, (worst, documented_worst)


def test_manifest_sheets_have_georef_fields():
    for sheet in MANIFEST["sheets"]:
        g = sheet["georef"]
        for k in ("a", "b", "tx", "ty", "metersPerPixel", "estAbsUncertaintyM"):
            assert k in g, (sheet["id"], k)
