"""Oak Ridge (Iverson's field) environment bake: same provenance/shape
contract as the Angle bake (test_environment.py), over the July 1 site."""
import json
from pathlib import Path

import numpy as np
import pytest

from terrain_pipeline import environment_oakridge as eo
from terrain_pipeline.environment import (
    CLASS_DRY_GRASS, CLASS_PASTURE, CLASS_TRAMPLED)

REPO = Path(__file__).resolve().parents[2]
LANDCOVER = REPO / "data" / "landcover" / "oakridge.landcover.json"

# The Oak Ridge Soldier View crop (design doc EDI; cli environment-oakridge).
X0, Z0, X1, Z1 = 3350.0, 7800.0, 4350.0, 8800.0


@pytest.fixture(scope="module")
def baked(tmp_path_factory):
    out = tmp_path_factory.mktemp("env-oakridge")
    meta = out / "heightmap.json"
    meta.write_text(json.dumps({
        "crop_x0_m": X0, "crop_z0_m": Z0, "crop_x1_m": X1, "crop_z1_m": Z1,
    }))
    env_path, splat_path = eo.bake(LANDCOVER, meta, out)
    return json.loads(env_path.read_text()), splat_path.read_bytes()


@pytest.fixture(scope="module")
def features():
    doc = json.loads(LANDCOVER.read_text())
    return {f["id"]: f for f in doc["features"]}


def test_bake_is_deterministic(baked, tmp_path):
    env, splat = baked
    meta = tmp_path / "heightmap.json"
    meta.write_text(json.dumps({
        "crop_x0_m": X0, "crop_z0_m": Z0, "crop_x1_m": X1, "crop_z1_m": Z1,
    }))
    env_path2, splat_path2 = eo.bake(LANDCOVER, meta, tmp_path)
    assert json.loads(env_path2.read_text()) == env
    assert splat_path2.read_bytes() == splat


def test_wall_is_the_traced_feature(baked, features):
    env, _ = baked
    wall = env["wall"]
    assert wall["featureId"] == "wall-forney-field"
    pts = features["wall-forney-field"]["points"]
    flat = [v for x, z in pts for v in (x, z)]
    assert wall["polylineFlat"] == flat
    # degenerate rails range: no rails-on-top anywhere on this site
    assert wall["railsZRange"][0] > wall["railsZRange"][1]
    assert wall["claimIds"], "provenance contract: claims required"


def test_wall_passes_through_the_drawn_baxter_bar(features):
    (ax, az), (bx, bz) = features["wall-forney-field"]["points"]
    # distance from the j1-09 drawn bar (4007, 8424) to the trace
    vx, vz = bx - ax, bz - az
    t = ((4007 - ax) * vx + (8424 - az) * vz) / (vx * vx + vz * vz)
    cx, cz = ax + t * vx, az + t * vz
    assert ((4007 - cx) ** 2 + (8424 - cz) ** 2) ** 0.5 < 1.0


def test_woods_trees_inside_polygon_and_crop(baked, features):
    env, _ = baked
    grove = env["groves"][0]
    assert grove["featureId"] == "woodlot-oakridge"
    assert 300 < len(grove["trees"]) < 1500
    for tree in grove["trees"]:
        assert X0 <= tree["x"] <= X1 and Z0 <= tree["z"] <= Z1


def test_every_feature_carries_provenance(baked):
    env, _ = baked
    for b in env["buildings"]:
        assert b["claimIds"] or b["editorialIds"]
    assert env["wall"]["claimIds"]
    for g in env["groves"]:
        assert g["claimIds"] or g["editorialIds"]


def test_splat_has_field_and_trampled_classes(baked):
    env, splat = baked
    res = env["splat"]["resolution"]
    grid = np.frombuffer(splat, dtype=np.uint8).reshape(res, res)
    counts = {c: int((grid == c).sum()) for c in
              (CLASS_PASTURE, CLASS_DRY_GRASS, CLASS_TRAMPLED)}
    assert counts[CLASS_DRY_GRASS] > 0, "the open Forney fields"
    assert counts[CLASS_TRAMPLED] > 0, "the advance corridor"
    assert counts[CLASS_PASTURE] > 0

    def class_at(x, z):
        px = (X1 - X0) / res
        ci = int((x - X0) / px)
        ri = int((Z1 - z) / px)
        return grid[ri, ci]

    # mid-corridor sample: the ground the brigade walked is trampled
    assert class_at(3930.0, 8590.0) == CLASS_TRAMPLED
    # SE of the wall (the woods floor / reverse slope): untouched pasture
    assert class_at(4100.0, 8300.0) == CLASS_PASTURE


def test_site_minimalism_is_explicit(baked):
    env, _ = baked
    assert env["road"]["centerline"] == []
    assert env["fences"] == []
    assert env["battery"]["guns"] == []
    assert env["wheat"]["polygonFlat"] == []
