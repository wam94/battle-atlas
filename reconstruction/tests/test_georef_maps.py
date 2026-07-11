"""Tests for the map-sheet georeferencing helper + the committed manifest.

The transform convention under test is the overlay.ts similarity
(local = [a -b; b a] * (img_x, -img_y) + t); parity with the authoring
tool's two-point workflow is the first contract.
"""
import json
import math
import sys
from pathlib import Path

import pytest

SCRIPTS = Path(__file__).resolve().parent.parent / "scripts"
sys.path.insert(0, str(SCRIPTS))

from georef_maps import (  # noqa: E402
    ImgSimilarity, Similarity, compose, fit_img_similarity, fit_similarity,
    load_manifest, similarity_from_two_points, DEFAULT_MANIFEST)


def make_sim(scale, rot_deg, tx, ty):
    th = math.radians(rot_deg)
    return Similarity(scale * math.cos(th), scale * math.sin(th), tx, ty)


class TestSimilarityCore:
    def test_two_point_solve_matches_overlay_ts_semantics(self):
        # identical to the overlay.ts contract: image y down, local z north
        T = similarity_from_two_points((0, 0), (100.0, 200.0),
                                       (10, 0), (110.0, 200.0))
        # unit scale, no rotation: pixel +x -> local +x, pixel +y -> local -z
        assert T.apply((0, 10)) == pytest.approx((100.0, 190.0))
        assert T.scale == pytest.approx(1.0)
        assert T.rotation_deg == pytest.approx(0.0)

    def test_two_point_solve_recovers_rotation_and_scale(self):
        ref = make_sim(0.94, -2.9, -280.0, 11026.0)
        p, q = (4022, 7642), (5678, 4138)
        T = similarity_from_two_points(p, ref.apply(p), q, ref.apply(q))
        assert T.a == pytest.approx(ref.a)
        assert T.b == pytest.approx(ref.b)
        assert T.tx == pytest.approx(ref.tx)
        assert T.ty == pytest.approx(ref.ty)

    def test_coincident_points_raise(self):
        with pytest.raises(ValueError):
            similarity_from_two_points((5, 5), (0, 0), (5, 5), (1, 1))

    def test_invert_round_trips(self):
        T = make_sim(0.9466, -2.908, -279.5, 11026.1)
        img = (4889.0, 6496.0)
        assert T.invert(T.apply(img)) == pytest.approx(img, abs=1e-6)


class TestLeastSquaresFit:
    def test_exact_with_two_points_matches_closed_form(self):
        ref = make_sim(1.3, 12.0, 55.0, -20.0)
        pts = [((0, 0), ref.apply((0, 0))), ((100, 50), ref.apply((100, 50)))]
        T, res = fit_similarity(pts)
        assert max(res) < 1e-9
        assert T.a == pytest.approx(ref.a)
        assert T.b == pytest.approx(ref.b)

    def test_overdetermined_recovers_truth_and_reports_residuals(self):
        ref = make_sim(0.94, -2.9, -280.0, 11026.0)
        pts = [(4022, 7642), (5762, 8424), (5678, 4138), (888, 5750),
               (3533, 1370)]
        noise = [(3, -2), (-4, 1), (2, 3), (0, -3), (-1, 2)]
        ties = [((x, y), tuple(c + n for c, n in zip(ref.apply((x, y)), dn)))
                for (x, y), dn in zip(pts, noise)]
        T, res = fit_similarity(ties)
        assert T.scale == pytest.approx(0.94, abs=0.01)
        assert T.rotation_deg == pytest.approx(-2.9, abs=0.2)
        assert all(r < 6.0 for r in res)  # noise magnitude, not more

    def test_requires_two_points(self):
        with pytest.raises(ValueError):
            fit_similarity([((0, 0), (1, 1))])


class TestComposition:
    def test_compose_equals_sequential_application(self):
        ref_to_local = make_sim(0.9466, -2.908, -279.5, 11026.1)
        th = math.radians(0.21)
        s = 1.0016
        to_ref = ImgSimilarity(s * math.cos(th), s * math.sin(th), 14.2, -9.6)
        comp = compose(to_ref, ref_to_local)
        for img in [(0, 0), (4700, 5300), (9000, 10000), (123.4, 8765.4)]:
            seq = ref_to_local.apply(to_ref.apply(img))
            assert comp.apply(img) == pytest.approx(seq, abs=1e-6)

    def test_img_similarity_fit(self):
        th = math.radians(-0.3)
        truth = ImgSimilarity(1.002 * math.cos(th), 1.002 * math.sin(th),
                              25.0, -14.0)
        pts = [(500, 500), (2000, 300), (900, 2400), (2200, 2300)]
        pairs = [((x, y), truth.apply((x, y))) for x, y in pts]
        S, res = fit_img_similarity(pairs)
        assert max(res) < 1e-9
        assert S.c == pytest.approx(truth.c)
        assert S.d == pytest.approx(truth.d)


MANIFEST = json.load(open(DEFAULT_MANIFEST)) if DEFAULT_MANIFEST.exists() else None
needs_manifest = pytest.mark.skipif(MANIFEST is None,
                                    reason="manifest not built")


@needs_manifest
class TestManifest:
    def test_all_28_sheets_present_with_provenance(self):
        sheets = MANIFEST["sheets"]
        assert len(sheets) == 28
        for s in sheets:
            assert s["sha256"] and len(s["sha256"]) == 64
            assert s["urls"]["jp2"].startswith("https://archive.org/download/")
            assert s["license"]["credit"].startswith("David Rumsey")
            assert s["widthPx"] > 4000 and s["heightPx"] > 8000

    def test_main_field_sheets_georeferenced(self):
        main = [s for s in MANIFEST["sheets"] if s["file"].startswith("12440")]
        assert len(main) == 23
        for s in main:
            g = s["georef"]
            assert 0.9 < g["metersPerPixel"] < 1.0
            assert abs(g["rotationDeg"]) < 5.0
            assert g["estAbsUncertaintyM"] <= 60.0

    def test_reference_fit_residuals_within_stated_rms(self):
        fit = MANIFEST["referenceFit"]
        assert fit["rmsResidualM"] < 45.0
        m = load_manifest()
        ref_id = None
        for s in MANIFEST["sheets"]:
            if s["file"][:-4] == MANIFEST["referenceSheet"]:
                ref_id = s["id"]
        ref = m[ref_id]
        for t in MANIFEST["referenceTiePoints"]:
            if t["role"] != "tie":
                continue
            lx, lz = ref.to_local(*t["img"])
            d = math.dist((lx, lz), t["local"])
            assert d <= 2.5 * fit["rmsResidualM"], t["name"]

    def test_loader_round_trips_pixels(self):
        m = load_manifest()
        for sid in m.ids():
            g = m[sid]
            px, py = 4000.0, 5000.0
            lx, lz = g.to_local(px, py)
            assert g.from_local(lx, lz) == pytest.approx((px, py), abs=1e-5)

    def test_sheet8_angle_sector_sanity(self):
        """The Angle wall corner (landmark anchor #2) must land inside the
        charge sector of the 1-5 PM sheet (the research doc's verified
        Angle crop region 4700..6900 x 5300..7000)."""
        m = load_manifest()
        ref = m["j3-03"]
        px, py = ref.from_local(4395.0, 4881.6)
        assert 4700 < px < 6900
        assert 5300 < py < 7000

    def test_elliott_entry(self):
        e = MANIFEST["elliott"]
        assert e["sha256"] and e["license"]["basis"].startswith("published 1864")
        g = e["georef"]
        # Elliott is ~1:9051 scanned coarser than the Bachelder set
        assert 0.5 < g["metersPerPixel"] < 0.9
        assert g["estAbsUncertaintyM"] >= 75.0
