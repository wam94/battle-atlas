"""Phase 7 environment bake: sourced geometry validated against the traced
land cover, the claims corpus, and the editorial decisions (plan §12 Phase 7:
"Validate all geometry against macro coordinates and claims")."""
import json
import math
import re
from pathlib import Path

import numpy as np
import pytest

from terrain_pipeline import environment
from terrain_pipeline.crop import DEFAULT_CROP

REPO = Path(__file__).resolve().parents[2]
LANDCOVER = REPO / "data" / "landcover" / "landcover.json"
BATTLE = REPO / "app" / "Assets" / "Battle" / "gettysburg-july3.json"
CLAIMS = REPO / "reconstruction" / "claims" / "angle.claims.json"
DECISIONS = REPO / "docs" / "reconstruction" / "angle-editorial-decisions.md"

X0, Z0, X1, Z1 = DEFAULT_CROP


@pytest.fixture(scope="module")
def baked(tmp_path_factory):
    out = tmp_path_factory.mktemp("env")
    meta = out / "heightmap.json"
    meta.write_text(json.dumps({
        "crop_x0_m": X0, "crop_z0_m": Z0, "crop_x1_m": X1, "crop_z1_m": Z1,
    }))
    env_path, splat_path = environment.bake(LANDCOVER, meta, BATTLE, out)
    return json.loads(env_path.read_text()), splat_path.read_bytes()


@pytest.fixture(scope="module")
def features():
    doc = json.loads(LANDCOVER.read_text())
    return {f["id"]: f for f in doc["features"]}


def dist_to_polyline(px, pz, points):
    best = float("inf")
    for i in range(len(points) - 1):
        ax, az = points[i]
        bx, bz = points[i + 1]
        vx, vz = bx - ax, bz - az
        L2 = vx * vx + vz * vz
        t = 0.0 if L2 == 0 else max(0.0, min(1.0, ((px - ax) * vx + (pz - az) * vz) / L2))
        best = min(best, math.hypot(px - (ax + t * vx), pz - (az + t * vz)))
    return best


def test_road_centerline_stays_between_the_traced_fences(baked, features):
    env, _ = baked
    west = features["fence-emmitsburg-road-west"]["points"]
    east = features["fence-emmitsburg-road-east"]["points"]
    for s in env["road"]["centerline"]:
        dw = dist_to_polyline(s["x"], s["z"], west)
        de = dist_to_polyline(s["x"], s["z"], east)
        # the centerline is the midpoint of the local fence span: both
        # distances match the sample's half-width within trace roughness
        assert abs(dw - s["halfWidth"]) < 1.5, (s, dw)
        assert abs(de - s["halfWidth"]) < 1.5, (s, de)
        assert 3.0 < s["halfWidth"] < 10.0, s


def test_wall_polyline_is_the_trace_verbatim(baked, features):
    env, _ = baked
    traced = [[float(x), float(z)] for x, z in
              features["wall-angle-webb-front"]["points"]]
    assert env["wall"]["polyline"] == traced


def test_wall_rails_stay_south_of_the_inner_angle(baked):
    # claim-wall-rails-on-top documents rails on the 69th PA front only;
    # the inner-angle jog is at z=4928 in the trace (ED-12).
    env, _ = baked
    lo, hi = env["wall"]["railsZRange"]
    assert lo < hi <= 4928.0


def test_fence_runs_lie_on_their_traced_polylines(baked, features):
    env, _ = baked
    assert len(env["fences"]) >= 4  # both road fences + interior fences
    ids = {f["featureId"] for f in env["fences"]}
    assert "fence-emmitsburg-road-west" in ids
    assert "fence-emmitsburg-road-east" in ids
    for run in env["fences"]:
        traced = features[run["featureId"]]["points"]
        for x, z in run["polyline"]:
            assert dist_to_polyline(x, z, traced) < 0.01, (run["featureId"], x, z)
            assert X0 - 0.01 <= x <= X1 + 0.01 and Z0 - 0.01 <= z <= Z1 + 0.01


def test_copse_trees_fill_the_traced_polygon(baked, features):
    env, _ = baked
    copse = next(g for g in env["groves"]
                 if g["featureId"] == "woodlot-copse-of-trees")
    assert len(copse["trees"]) == environment.COPSE_TREES
    poly = features["woodlot-copse-of-trees"]["points"]
    for t in copse["trees"]:
        assert environment._point_in_polygon(t["x"], t["z"], poly), t
        assert environment.COPSE_HEIGHTS[0] <= t["heightM"] <= environment.COPSE_HEIGHTS[1]


def test_orchard_trees_stay_inside_polygon_and_crop(baked, features):
    env, _ = baked
    assert any(o["featureId"] == "orchard-codori" for o in env["orchards"])
    for o in env["orchards"]:
        poly = features[o["featureId"]]["points"]
        assert o["trees"], o["featureId"]
        for t in o["trees"]:
            assert environment._point_in_polygon(t["x"], t["z"], poly), (o["featureId"], t)
            assert X0 <= t["x"] <= X1 and Z0 <= t["z"] <= Z1


def test_barn_position_matches_its_claim(baked):
    env, _ = baked
    claims = {c["id"]: c for c in json.loads(CLAIMS.read_text())["claims"]}
    geom = claims["claim-codori-barn-position"]["geometry"]
    barn = next(b for b in env["buildings"] if b["id"] == "codori-barn")
    assert barn["x"] == geom["x"] and barn["z"] == geom["z"]
    house = next(b for b in env["buildings"] if b["id"] == "codori-house")
    assert math.hypot(house["x"] - barn["x"], house["z"] - barn["z"]) \
        <= geom["uncertaintyM"]


def test_battery_layout_honors_ed16(baked, features):
    env, _ = baked
    wall_pts = features["wall-angle-webb-front"]["points"]
    battle = json.loads(BATTLE.read_text())
    unit = next(u for u in battle["units"] if u["id"] == "us-btty-cushing")
    kf = min(unit["keyframes"], key=lambda k: abs(k["t"] - 8400))
    half_front = unit.get("frontage_m", 80) / 2.0

    guns = env["battery"]["guns"]
    assert len(guns) == 6  # claim-cushing-armament
    wall_guns = [g for g in guns if g["at"] == "wall"]
    assert len(wall_guns) == 2  # claim-cushing-guns-to-wall
    for g in wall_guns:
        assert g["state"] == "intact"
        assert dist_to_polyline(g["x"], g["z"], wall_pts) < 6.0, g
    def wall_x_at(z):
        # first polyline segment spanning z, in trace order — the outer
        # N-S run wins near the jog (mirrors the builder's convention)
        for i in range(len(wall_pts) - 1):
            (ax, az), (bx, bz) = wall_pts[i], wall_pts[i + 1]
            if min(az, bz) <= z <= max(az, bz) and az != bz:
                return ax + (z - az) / (bz - az) * (bx - ax)
        raise AssertionError(f"z {z} off the wall")

    for g in guns:
        assert abs(g["z"] - kf["z"]) <= half_front + 5.0, g
        if g["at"] == "crest":
            d = g["x"] - wall_x_at(g["z"])
            assert abs(d - environment.CREST_OFFSET_M) < 5.0, (g, d)
    assert len(env["battery"]["limbers"]) == 6
    assert len(env["battery"]["caissons"]) == 6


def test_splat_raster_is_deterministic_and_in_vocabulary(baked, tmp_path):
    env, splat_bytes = baked
    assert len(splat_bytes) == environment.SPLAT_RESOLUTION ** 2
    grid = np.frombuffer(splat_bytes, dtype=np.uint8)
    assert grid.max() <= environment.CLASS_TRAMPLED
    # the documented covers all appear
    for cls in (environment.CLASS_PASTURE, environment.CLASS_STUBBLE,
                environment.CLASS_ROAD, environment.CLASS_TRAMPLED):
        assert (grid == cls).any(), cls
    # second bake is byte-identical (determinism gate)
    meta = tmp_path / "heightmap.json"
    meta.write_text(json.dumps({
        "crop_x0_m": X0, "crop_z0_m": Z0, "crop_x1_m": X1, "crop_z1_m": Z1,
    }))
    env2_path, splat2 = environment.bake(LANDCOVER, meta, BATTLE, tmp_path)
    assert splat2.read_bytes() == splat_bytes
    env2 = json.loads(env2_path.read_text())
    assert env2 == env


def test_every_provenance_id_resolves(baked):
    """Plan §8.2: each staged feature cites claims and/or editorial decisions."""
    env, _ = baked
    claims = {c["id"] for c in json.loads(CLAIMS.read_text())["claims"]}
    decisions = set(re.findall(r"### (ED-\d+)", DECISIONS.read_text()))

    def check(obj, where):
        cids = obj.get("claimIds", [])
        eids = obj.get("editorialIds", [])
        assert cids or eids, f"{where}: no provenance at all"
        for cid in cids:
            assert cid in claims, f"{where}: unknown claim {cid}"
        for eid in eids:
            assert eid in decisions, f"{where}: unknown decision {eid}"

    check(env["road"], "road")
    check(env["wall"], "wall")
    check(env["battery"], "battery")
    check(env["wheat"], "wheat")
    for f in env["fences"]:
        check(f, f"fence {f['featureId']}")
    for g in env["groves"]:
        check(g, f"grove {g['featureId']}")
    for o in env["orchards"]:
        check(o, f"orchard {o['featureId']}")
    for b in env["buildings"]:
        check(b, f"building {b['id']}")
