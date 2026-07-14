"""Tests for the map-furniture generator (roads/hydrology/town/rail):
schema validity, the source-required-when-documented rule, determinism
(regenerate == byte-identical, docs/format/map-furniture-format.md), and a
geometric sanity check that the traced road/rail spokes actually meet near
the Diamond (self-consistency between features traced in separate crops).
"""
import importlib
import json
import math
import sys
from pathlib import Path

import jsonschema
import pytest

ROOT = Path(__file__).resolve().parent.parent.parent
SCRIPTS = ROOT / "reconstruction" / "scripts"
sys.path.insert(0, str(SCRIPTS))

trace_map_furniture = importlib.import_module("trace_map_furniture")

SCHEMA_PATH = ROOT / "docs" / "format" / "map-furniture.schema.json"
DATA_PATH = ROOT / "data" / "map-furniture" / "map-furniture.json"


@pytest.fixture(scope="module")
def schema():
    return json.loads(SCHEMA_PATH.read_text())


@pytest.fixture(scope="module")
def generated():
    """Build the document in-process (doesn't touch the committed file)."""
    return {
        "name": "test",
        "features": trace_map_furniture.build_features(),
    }


@pytest.fixture(scope="module")
def committed():
    if not DATA_PATH.exists():
        pytest.skip("data/map-furniture/map-furniture.json not generated; "
                     "run trace_map_furniture.py first")
    return json.loads(DATA_PATH.read_text())


class TestSchema:
    def test_generated_doc_matches_schema(self, schema, generated):
        jsonschema.validate(generated, schema)

    def test_committed_doc_matches_schema(self, schema, committed):
        jsonschema.validate(committed, schema)

    def test_committed_doc_matches_freshly_generated(self, generated, committed):
        # the committed file IS the generator's output -- catches "forgot
        # to regenerate after editing the ledger"
        assert committed["features"] == generated["features"]

    def test_every_documented_feature_has_a_nonempty_source(self, generated):
        for f in generated["features"]:
            if f["confidence"] == "documented":
                assert f.get("source", "").strip(), f["id"]

    def test_feature_ids_are_unique(self, generated):
        ids = [f["id"] for f in generated["features"]]
        assert len(ids) == len(set(ids))

    def test_all_points_within_the_valid_square(self, generated):
        for f in generated["features"]:
            for x, z in f["points"]:
                assert 0.0 <= x <= 8507.2, f["id"]
                assert 0.0 <= z <= 8507.2, f["id"]

    def test_expected_named_features_present(self, generated):
        ids = {f["id"] for f in generated["features"]}
        # the task's explicit road/stream lists
        for road in ["chambersburg-pike", "baltimore-pike", "emmitsburg",
                     "taneytown", "hanover", "harrisburg", "carlisle",
                     "mummasburg", "fairfield"]:
            assert f"road-{road}" in ids, road
        for stream in ["rock-creek", "willoughby-run", "plum-run",
                        "pitzers-run", "marsh-creek", "stevens-run",
                        "winebrenners-run"]:
            assert f"stream-{stream}" in ids, stream
        assert "rail-finished-hanover-branch" in ids
        assert "rail-unfinished-cut-west" in ids
        assert any(fid.startswith("town-block-") for fid in ids)


class TestDeterminism:
    def test_regenerate_is_byte_identical(self, tmp_path, monkeypatch):
        out1 = tmp_path / "run1.json"
        out2 = tmp_path / "run2.json"
        monkeypatch.setattr(trace_map_furniture, "OUT_PATH", out1)
        trace_map_furniture.main()
        monkeypatch.setattr(trace_map_furniture, "OUT_PATH", out2)
        trace_map_furniture.main()
        assert out1.read_bytes() == out2.read_bytes()

    def test_clip_points_only_trims_the_ends_here(self):
        # documents the assumption clip_points() relies on: none of the
        # ledger's out-of-bounds picks are interior to a traced course
        for fid, pts in trace_map_furniture.SHEET_PX.items():
            from georef_maps import load_manifest
            sheet = load_manifest()[trace_map_furniture.SHEET_ID]
            local = [sheet.to_local(px, py) for px, py in pts]
            in_bounds = [0.0 <= x <= 8507.2 and 0.0 <= z <= 8507.2 for x, z in local]
            # find the maximal in-bounds run; it should be almost the whole list
            # (interior gaps would silently fragment the traced line)
            first_in = in_bounds.index(True) if True in in_bounds else None
            last_in = len(in_bounds) - 1 - in_bounds[::-1].index(True) if True in in_bounds else None
            assert first_in is not None, fid
            interior = in_bounds[first_in:last_in + 1]
            assert all(interior), (
                f"{fid}: an out-of-bounds point sits between two in-bounds "
                "points -- clip_points() would fragment this course")


class TestJunctionContinuity:
    """Roads/rail traced independently (different crops) should still meet
    near the Diamond -- a cross-check that the per-crop pixel picks and the
    town's own tracing are mutually consistent, not just individually
    schema-valid. Tolerance varies by feature: some spokes were traced with
    an endpoint placed exactly at the town-diamond tie point (tight bound);
    others were traced only as far as the built-up block grid's edge, which
    is legitimately tens to hundreds of meters short of the Diamond itself."""

    DIAMOND = (4875.8, 6835.9)  # manifest town-diamond tie point

    @pytest.mark.parametrize("road_id,endpoint,tolerance_m", [
        ("road-carlisle", -1, 60.0),                 # traced straight to the tie point
        ("road-harrisburg", -1, 60.0),                # traced straight to the tie point
        ("road-hanover", 0, 60.0),                    # traced straight to the tie point
        ("rail-finished-hanover-branch", 0, 60.0),    # traced straight to the tie point
        ("road-chambersburg-pike", -1, 250.0),        # ends at the NW block-grid edge
        ("road-mummasburg", -1, 250.0),               # ends at the NW block-grid edge
        ("road-fairfield", -1, 500.0),                # ends at the SW block-grid edge
        ("rail-unfinished-cut-west", -1, 750.0),      # ends at the Carlisle St grid edge
    ])
    def test_spoke_reaches_near_the_diamond(self, generated, road_id, endpoint, tolerance_m):
        feature = next(f for f in generated["features"] if f["id"] == road_id)
        x, z = feature["points"][endpoint]
        dist = math.hypot(x - self.DIAMOND[0], z - self.DIAMOND[1])
        assert dist < tolerance_m, (
            f"{road_id} endpoint {endpoint} is {dist:.0f} m from the Diamond "
            f"(tolerance {tolerance_m} m)")
