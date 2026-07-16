"""Bake the Oak Ridge (Iverson's field) crop environment — the second
Soldier View film's site (July 1 afternoon).

Same contract and output shape as :mod:`environment` (the Angle bake), so
the Unity stage (`AngleEnvironmentStage`) consumes it unchanged via its
crop-directory override. Reads the July 1 landcover file
``data/landcover/oakridge.landcover.json`` (SEPARATE from landcover.json by
design — film-safety for the pinned Angle bundle) and the Oak Ridge crop
metadata, and emits ``environment.json`` + ``environment_splat.raw``.

Provenance contract (plan §8.2) unchanged: every emitted feature carries
claim ids (reconstruction/claims/iverson.claims.json) and/or editorial ids
(EDI-1..EDI-6, docs/reconstruction/iverson-viewpoint-design.md).

Editorial constants for THIS site:

- EDI-1  wall trace: straight through the j1-09 drawn Baxter bar, bearing 70
- EDI-2  woods band + open-field surface classes
- EDI-4  trampled advance corridor (the brigade's final-approach frontage)
- EDI-6  Forney farm massing (Codori-analogue props, ED-13 convention)

PROOF-STAGE MINIMALISM (disclosed): no road corridor (the Mummasburg Road
sits behind the Union line and its only battlefield-frame trace is
map-furniture-grade, ~700 m adrift here — sheet-trace hardening is named
follow-up scope), no field-boundary fences (no trace; not invented), no
staged batteries (Carter's Oak Hill guns are outside the cast — noted in
the audio assessment).
"""
import hashlib
import json
import math
from pathlib import Path

import numpy as np

from terrain_pipeline.environment import (
    CLASS_DRY_GRASS, CLASS_PASTURE, CLASS_TRAMPLED, SPLAT_RESOLUTION,
    TREE_BASE_HEIGHT_M, WALL_HEIGHT_M, WALL_WIDTH_M, _point_in_polygon,
    _polygon_grid_trees, load_features)

WOODS_PITCH_M = 9.0
WOODS_HEIGHTS = (7.0, 10.0)

# EDI-6: Forney farm massing at the drawn point (claim-forney-farm); the
# barn is offset NW of the house along the farm lane's assumed axis.
FORNEY_HOUSE_POS = (3614.0, 8637.0)
FORNEY_HOUSE_SIZE = (9.0, 7.5, 5.5)
FORNEY_BARN_POS = (3592.0, 8654.0)
FORNEY_BARN_SIZE = (18.0, 11.0, 6.5)
FORNEY_YAW_DEG = 70.0  # editorial: long axes parallel to the ridge/wall

# EDI-4: the trampled advance corridor — the four-regiment frontage swept
# from the slice-start line to the death line (canonical route geometry).
TRAMPLE_QUAD = [
    [4035.8, 8740.8],   # 5th NC left flank at t0
    [4116.1, 8568.6],   # 5th NC left flank at the death line
    [3826.1, 8433.4],   # 12th NC right flank at the death line
    [3745.8, 8605.6],   # 12th NC right flank at t0
]


def build_wall(features):
    wall = features["wall-forney-field"]
    return {
        "featureId": "wall-forney-field",
        "polylineFlat": [float(v) for x, z in wall["points"] for v in (x, z)],
        "heightM": WALL_HEIGHT_M,
        "widthM": WALL_WIDTH_M,
        # degenerate range (z0 > z1): no rails-on-top anywhere — the Angle's
        # rails were a 69th-PA-front claim with no July 1 counterpart
        "railsZRange": [1.0, 0.0],
        "claimIds": ["claim-wall-structure", "claim-iv-100yards"],
        "editorialIds": ["EDI-1"],
    }


def build_woods(features, crop):
    poly = features["woodlot-oakridge"]["points"]
    return [{
        "featureId": "woodlot-oakridge",
        "trees": _polygon_grid_trees(
            "oakridge-woods", poly, crop, WOODS_PITCH_M, WOODS_HEIGHTS),
        "claimIds": ["claim-woods-oakridge", "claim-wall-structure"],
        "editorialIds": ["EDI-2"],
    }]


def build_buildings():
    return [
        {
            "id": "forney-house",
            "kind": "house",
            "x": FORNEY_HOUSE_POS[0], "z": FORNEY_HOUSE_POS[1],
            "yawDeg": FORNEY_YAW_DEG,
            "sizeM": list(FORNEY_HOUSE_SIZE),
            "claimIds": ["claim-forney-farm"],
            "editorialIds": ["EDI-6"],
        },
        {
            "id": "forney-barn",
            "kind": "barn",
            "x": FORNEY_BARN_POS[0], "z": FORNEY_BARN_POS[1],
            "yawDeg": FORNEY_YAW_DEG,
            "sizeM": list(FORNEY_BARN_SIZE),
            "claimIds": ["claim-forney-farm"],
            "editorialIds": ["EDI-6"],
        },
    ]


def rasterize_splat(features, crop, resolution=SPLAT_RESOLUTION):
    """uint8 ground-class raster, row 0 = north (heightmap convention)."""
    x0, z0, x1, z1 = crop
    size = x1 - x0
    px = size / resolution
    grid = np.full((resolution, resolution), CLASS_PASTURE, dtype=np.uint8)

    def paint_polygon(poly, cls):
        xs = [p[0] for p in poly]
        zs = [p[1] for p in poly]
        ci0 = max(0, int((min(xs) - x0) / px))
        ci1 = min(resolution - 1, int((max(xs) - x0) / px))
        for ci in range(ci0, ci1 + 1):
            x = x0 + (ci + 0.5) * px
            ri0 = max(0, int((z1 - max(zs)) / px))
            ri1 = min(resolution - 1, int((z1 - min(zs)) / px))
            for ri in range(ri0, ri1 + 1):
                z = z1 - (ri + 0.5) * px
                if _point_in_polygon(x, z, poly):
                    grid[ri, ci] = cls

    # the open Forney fields (EDI-2), then the trampled corridor over them
    paint_polygon(features["field-forney"]["points"], CLASS_DRY_GRASS)
    paint_polygon(TRAMPLE_QUAD, CLASS_TRAMPLED)
    return grid


def bake(landcover_path, crop_meta_path, out_dir):
    meta = json.loads(Path(crop_meta_path).read_text())
    crop = (meta["crop_x0_m"], meta["crop_z0_m"], meta["crop_x1_m"], meta["crop_z1_m"])
    features = load_features(landcover_path)

    wall = build_wall(features)
    groves = build_woods(features, crop)
    buildings = build_buildings()
    splat = rasterize_splat(features, crop)

    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    splat_path = out_dir / "environment_splat.raw"
    splat_path.write_bytes(splat.tobytes())

    doc = {
        "name": "Oak Ridge / Iverson's field crop environment (second Soldier View film, proof stage)",
        "frame": "battlefield-local meters (macro); Unity stage converts via AngleTerrainFrame",
        "crop": {"x0": crop[0], "z0": crop[1], "x1": crop[2], "z1": crop[3]},
        "provenance": ("every feature carries claimIds (reconstruction/claims/iverson.claims.json) "
                       "and/or editorialIds (EDI-*, docs/reconstruction/iverson-viewpoint-design.md)"),
        "road": {
            "centerline": [],
            "depthM": 0.0,
            "shoulderM": 0.0,
            "wheelPathOffsetM": 0.0,
            "sourceFeatures": [],
            "claimIds": [],
            "editorialIds": ["EDI-3"],
        },
        "fences": [],
        "wall": wall,
        "groves": groves,
        "orchards": [],
        "buildings": buildings,
        "battery": {
            "unitId": "none",
            "guns": [], "limbers": [], "caissons": [], "detritus": [],
            "claimIds": [], "editorialIds": [],
        },
        "wheat": {
            "featureId": "",
            "polygonFlat": [],
            "clumpSpacingM": 2.4,
            "patchNoiseKey": "oakridge-wheat",
            "claimIds": [],
            "editorialIds": [],
        },
        "splat": {
            "path": "environment_splat.raw",
            "resolution": SPLAT_RESOLUTION,
            "row0": "north",
            "classes": {
                "pasture": CLASS_PASTURE, "dry_grass": CLASS_DRY_GRASS,
                "trampled": CLASS_TRAMPLED,
            },
            "sha256": hashlib.sha256(splat.tobytes()).hexdigest(),
        },
        "treeAsset": {
            "path": "Assets/ThirdParty/Models/PolyHavenIslandTree02/island_tree_02_decimated.fbx",
            "baseHeightM": TREE_BASE_HEIGHT_M,
        },
    }
    out_path = out_dir / "environment.json"
    out_path.write_text(json.dumps(doc, indent=1) + "\n")
    return out_path, splat_path
