"""Bake the Angle crop's sourced environment geometry (plan §12 Phase 7).

Reads the traced land cover (``data/landcover/landcover.json``, Bachelder
trace) plus the crop metadata and emits ``data/heightmap_angle/
environment.json`` (crop-local geometry for the Unity stage) and
``environment_splat.raw`` (a uint8 ground-class raster).

Provenance contract (plan §8.2): every emitted feature carries the claim ids
and/or editorial-decision ids that justify it — the Unity stage builds ONLY
from this file, so nothing un-sourced can reach the scene. Editorial numbers
(road depth, fence height, building massing, battery spacing) live in
docs/reconstruction/angle-editorial-decisions.md ED-11..ED-18 and are pinned
here as named constants.

Determinism: pure functions of the committed inputs; jitter is the same FNV
hash family the Unity side uses (no ``random``).
"""
import hashlib
import json
import math
from pathlib import Path

import numpy as np

# --- Editorial constants (ED-11..ED-17; see angle-editorial-decisions.md) ---

ROAD_DEPTH_M = 0.45          # ED-11: sunken bed at centerline
ROAD_SHOULDER_M = 0.8        # ED-11: bed stops short of the fence lines
WHEEL_PATH_OFFSET_M = 0.75   # ED-11: wheel-path stripes at +/- this offset
WHEEL_PATH_WIDTH_M = 0.4

FENCE_POST_SPACING_M = 3.0   # ED-12: five-rail post-and-rail
FENCE_HEIGHT_M = 1.5
WORM_FENCE_HEIGHT_M = 1.2
WALL_HEIGHT_M = 0.75         # ED-12: double-faced fieldstone
WALL_WIDTH_M = 0.6

HOUSE_POS = (4005.0, 4646.0)   # ED-13 (macro meters)
HOUSE_SIZE = (9.0, 7.5, 5.5)   # long, deep, eaves
BARN_POS = (4020.4, 4635.4)    # claim-codori-barn-position (OSM anchor)
BARN_SIZE = (18.0, 11.0, 6.5)  # ED-13, Trostle-analogue massing

WALL_GUNS = [(4400.0, 4868.0), (4401.0, 4890.0)]  # ED-16
GUN_FACING_DEG = 262.0                            # macro keyframe facing
CREST_OFFSET_M = 28.0        # ED-16: Peyton's "about 30 paces"
LIMBER_OFFSET_M = 5.5        # ED-16: 6 yd (drill-manual norm)
CAISSON_OFFSET_M = 15.5      # ED-16: a further 11 yd

COPSE_TREES = 30             # ED-14
COPSE_HEIGHTS = (3.0, 6.0)
GROVE_HEIGHTS = (7.0, 9.0)
GROVE_PITCH_M = 7.0
ORCHARD_PITCH_M = 7.0        # rows, per period orchard practice
ORCHARD_HEIGHTS = (3.5, 4.5)
TREE_BASE_HEIGHT_M = 3.41    # island_tree_02 derived asset, measured

# Ground classes for the splat raster (Unity maps these to terrain layers).
CLASS_PASTURE = 0
CLASS_DRY_GRASS = 1
CLASS_STUBBLE = 2
CLASS_ROAD = 3
CLASS_DISTURBED = 4
CLASS_TRAMPLED = 5

SPLAT_RESOLUTION = 1024

# ED-15 field assignments (traced feature id -> class).
FIELD_CLASSES = {
    "field-west-of-emmitsburg-codori-front": CLASS_STUBBLE,
    "field-southeast-of-codori": CLASS_DRY_GRASS,
    "field-angle-final-assault": CLASS_PASTURE,   # claim-field-east-grass
    "field-hays-front-north-of-bryan-lane": CLASS_DRY_GRASS,
}

# ED-17 trampled corridor: assault frontage between the road and the wall.
TRAMPLE_Z = (4620.0, 4990.0)


# --- deterministic jitter (FNV family, mirrors FormationLayout.Jitter) ---

def _hash01(key, index):
    h = 2166136261
    for c in key:
        h = ((h ^ ord(c)) * 16777619) & 0xFFFFFFFF
    h = ((h ^ index) * 16777619) & 0xFFFFFFFF
    h ^= h >> 13
    h = (h * 0x5BD1E995) & 0xFFFFFFFF
    h ^= h >> 15
    return (h & 0xFFFFFF) / float(0xFFFFFF)


def _jitter(key, index, salt):
    """Symmetric [-1, 1) jitter, matching the C# convention."""
    return 2.0 * _hash01(key, index * 97 + salt) - 1.0


# --- geometry helpers ---

def _clip_polyline_to_rect(points, x0, z0, x1, z1):
    """Clip a polyline to a rect; returns list of runs (each a point list).

    Segment/rect intersections are computed exactly (Liang-Barsky) so traced
    lines enter and exit the crop at the boundary rather than snapping.
    """
    def inside(p):
        return x0 <= p[0] <= x1 and z0 <= p[1] <= z1

    def clip_segment(a, b):
        # Liang-Barsky; returns clipped (a', b') or None.
        dx, dz = b[0] - a[0], b[1] - a[1]
        t0, t1 = 0.0, 1.0
        for p, q in (
            (-dx, a[0] - x0), (dx, x1 - a[0]),
            (-dz, a[1] - z0), (dz, z1 - a[1]),
        ):
            if p == 0:
                if q < 0:
                    return None
                continue
            r = q / p
            if p < 0:
                if r > t1:
                    return None
                t0 = max(t0, r)
            else:
                if r < t0:
                    return None
                t1 = min(t1, r)
        if t0 > t1:
            return None
        return (
            (a[0] + t0 * dx, a[1] + t0 * dz),
            (a[0] + t1 * dx, a[1] + t1 * dz),
        )

    runs = []
    run = []
    for i in range(len(points) - 1):
        seg = clip_segment(tuple(points[i]), tuple(points[i + 1]))
        if seg is None:
            if run:
                runs.append(run)
                run = []
            continue
        a, b = seg
        if not run:
            run = [a]
        elif run[-1] != a:
            runs.append(run)
            run = [a]
        run.append(b)
    if run:
        runs.append(run)
    return runs


def _point_in_polygon(x, z, poly):
    inside = False
    j = len(poly) - 1
    for i in range(len(poly)):
        xi, zi = poly[i]
        xj, zj = poly[j]
        if (zi > z) != (zj > z) and x < (xj - xi) * (z - zi) / (zj - zi) + xi:
            inside = not inside
        j = i
    return inside


def _resample_polyline(points, step):
    """Points every `step` meters along a polyline (plus the exact end)."""
    out = [tuple(points[0])]
    carry = 0.0
    for i in range(len(points) - 1):
        ax, az = points[i]
        bx, bz = points[i + 1]
        seg = math.hypot(bx - ax, bz - az)
        if seg == 0:
            continue
        d = step - carry
        while d <= seg:
            t = d / seg
            out.append((ax + t * (bx - ax), az + t * (bz - az)))
            d += step
        carry = (carry + seg) % step
    if out[-1] != tuple(points[-1]):
        out.append(tuple(points[-1]))
    return out


def _closest_point_on_polyline(px, pz, points):
    best = None
    best_d2 = float("inf")
    for i in range(len(points) - 1):
        ax, az = points[i]
        bx, bz = points[i + 1]
        vx, vz = bx - ax, bz - az
        L2 = vx * vx + vz * vz
        t = 0.0 if L2 == 0 else max(0.0, min(1.0, ((px - ax) * vx + (pz - az) * vz) / L2))
        cx, cz = ax + t * vx, az + t * vz
        d2 = (px - cx) ** 2 + (pz - cz) ** 2
        if d2 < best_d2:
            best_d2 = d2
            best = (cx, cz)
    return best


def _bearing_deg(ax, az, bx, bz):
    """Compass bearing (deg) from a to b in the local frame (0 = north/+z)."""
    return math.degrees(math.atan2(bx - ax, bz - az)) % 360.0


# --- feature builders ---

def load_features(landcover_path):
    doc = json.loads(Path(landcover_path).read_text())
    return {f["id"]: f for f in doc["features"]}


def build_road(features, crop):
    """Road corridor from the span between the two traced fences (ED-6)."""
    x0, z0, x1, z1 = crop
    west = features["fence-emmitsburg-road-west"]["points"]
    east = features["fence-emmitsburg-road-east"]["points"]
    pad = 60.0  # sample slightly beyond the crop so the carve exits cleanly
    runs = _clip_polyline_to_rect(west, x0 - pad, z0 - pad, x1 + pad, z1 + pad)
    if not runs:
        raise ValueError("west road fence does not cross the padded crop")
    samples = []
    for wx, wz in _resample_polyline(runs[0], 5.0):
        ex, ez = _closest_point_on_polyline(wx, wz, east)
        cx, cz = (wx + ex) / 2.0, (wz + ez) / 2.0
        half = math.hypot(ex - wx, ez - wz) / 2.0
        samples.append({
            "x": round(cx, 2), "z": round(cz, 2),
            "halfWidth": round(half, 2),
        })
    return {
        "centerline": samples,
        "depthM": ROAD_DEPTH_M,
        "shoulderM": ROAD_SHOULDER_M,
        "wheelPathOffsetM": WHEEL_PATH_OFFSET_M,
        "sourceFeatures": ["fence-emmitsburg-road-west", "fence-emmitsburg-road-east"],
        "claimIds": ["claim-road-crossing", "claim-road-sunken", "claim-road-surface",
                     "claim-fence-west-structure", "claim-fence-east-structure"],
        "editorialIds": ["ED-6", "ED-11"],
    }


def build_fences(features, crop):
    """Traced fence polylines clipped to the crop, styled per ED-12."""
    x0, z0, x1, z1 = crop
    styles = {
        "fence-emmitsburg-road-west": ("post_and_rail", ["claim-fence-west-structure", "claim-fences-high-climb"]),
        "fence-emmitsburg-road-east": ("post_and_rail", ["claim-fence-east-structure", "claim-fences-high-climb"]),
        "fence-post-and-rail-west-of-road": ("post_and_rail", []),
        "fence-worm-north-of-codori-east-west": ("worm", []),
        "fence-codori-lane-south-worm": ("worm", []),
        "fence-post-and-rail-southeast-of-codori": ("post_and_rail", []),
    }
    fences = []
    for fid, (style, claims) in styles.items():
        if fid not in features:
            continue
        for run in _clip_polyline_to_rect(features[fid]["points"], x0, z0, x1, z1):
            fences.append({
                "featureId": fid,
                "style": style,
                "heightM": FENCE_HEIGHT_M if style == "post_and_rail" else WORM_FENCE_HEIGHT_M,
                "postSpacingM": FENCE_POST_SPACING_M,
                # flat [x0, z0, x1, z1, ...] so Unity's JsonUtility (no
                # nested-array support) can parse it directly
                "polylineFlat": [round(v, 2) for x, z in run for v in (x, z)],
                "claimIds": claims,
                "editorialIds": ["ED-12"],
            })
    return fences


def build_wall(features, battle_path):
    """The traced stone wall plus the rails-on-top range (69th PA front)."""
    wall = features["wall-angle-webb-front"]
    # 69th PA frontage from the macro battle file (rails documented there).
    battle = json.loads(Path(battle_path).read_text())
    unit = next(u for u in battle["units"] if u["id"] == "us-69pa")
    kf = min(unit["keyframes"], key=lambda k: abs(k["t"] - 8400))
    half_front = unit.get("frontage_m", 80) / 2.0
    return {
        "featureId": "wall-angle-webb-front",
        "polylineFlat": [float(v) for x, z in wall["points"] for v in (x, z)],
        "heightM": WALL_HEIGHT_M,
        "widthM": WALL_WIDTH_M,
        "railsZRange": [round(kf["z"] - half_front, 1), round(kf["z"] + half_front, 1)],
        "claimIds": ["claim-wall-structure", "claim-wall-profile", "claim-wall-rails-on-top"],
        "editorialIds": ["ED-12"],
    }


def _polygon_grid_trees(key, poly, crop, pitch, heights, jitter_frac=0.35):
    """Deterministic grid+jitter tree placements inside polygon ∩ crop."""
    x0, z0, x1, z1 = crop
    xs = [p[0] for p in poly]
    zs = [p[1] for p in poly]
    trees = []
    i = 0
    gx = math.floor(min(xs) / pitch) * pitch
    while gx <= max(xs):
        gz = math.floor(min(zs) / pitch) * pitch
        while gz <= max(zs):
            x = gx + pitch * jitter_frac * _jitter(key, i, 17)
            z = gz + pitch * jitter_frac * _jitter(key, i, 29)
            i += 1
            if not (x0 <= x <= x1 and z0 <= z <= z1):
                gz += pitch
                continue
            if _point_in_polygon(x, z, poly):
                h = heights[0] + (heights[1] - heights[0]) * _hash01(key, i * 7 + 3)
                trees.append({
                    "x": round(x, 2), "z": round(z, 2),
                    "heightM": round(h, 2),
                    "yawDeg": round(360.0 * _hash01(key, i * 11 + 5), 1),
                })
            gz += pitch
        gx += pitch
    return trees


def build_woodlands(features, crop):
    """Copse (ED-14), Ziegler's Grove, and the traced orchards."""
    copse_poly = features["woodlot-copse-of-trees"]["points"]
    xs = [p[0] for p in copse_poly]
    zs = [p[1] for p in copse_poly]
    cx, cz = sum(xs) / len(xs), sum(zs) / len(zs)
    copse_trees = []
    i = 0
    attempts = 0
    while len(copse_trees) < COPSE_TREES and attempts < COPSE_TREES * 30:
        # rejection-sample the polygon, biased toward the center
        r = _hash01("copse-p7", attempts * 3 + 1) ** 0.7
        ang = 2 * math.pi * _hash01("copse-p7", attempts * 3 + 2)
        x = cx + r * (max(xs) - min(xs)) * 0.62 * math.cos(ang)
        z = cz + r * (max(zs) - min(zs)) * 0.62 * math.sin(ang)
        attempts += 1
        if _point_in_polygon(x, z, copse_poly):
            h = COPSE_HEIGHTS[0] + (COPSE_HEIGHTS[1] - COPSE_HEIGHTS[0]) * _hash01("copse-p7", attempts * 5 + 3)
            copse_trees.append({
                "x": round(x, 2), "z": round(z, 2),
                "heightM": round(h, 2),
                "yawDeg": round(360.0 * _hash01("copse-p7", attempts * 7 + 4), 1),
            })
            i += 1
    groves = [{
        "featureId": "woodlot-copse-of-trees",
        "trees": copse_trees,
        "claimIds": ["claim-copse-landcover", "claim-copse-form-1863", "claim-copse-unfenced-1863"],
        "editorialIds": ["ED-14"],
    }]
    if "woodlot-zieglers-grove" in features:
        groves.append({
            "featureId": "woodlot-zieglers-grove",
            "trees": _polygon_grid_trees(
                "zieglers-p7", features["woodlot-zieglers-grove"]["points"],
                crop, GROVE_PITCH_M, GROVE_HEIGHTS),
            "claimIds": [],
            "editorialIds": ["ED-14"],
        })
    orchards = []
    for fid in ("orchard-codori", "orchard-bliss-farm"):
        if fid not in features:
            continue
        trees = _polygon_grid_trees(
            f"{fid}-p7", features[fid]["points"], crop,
            ORCHARD_PITCH_M, ORCHARD_HEIGHTS, jitter_frac=0.12)
        if trees:
            orchards.append({
                "featureId": fid,
                "trees": trees,
                "claimIds": ["claim-codori-orchard"] if fid == "orchard-codori" else [],
                "editorialIds": ["ED-14", "ED-15"],
            })
    return groves, orchards


def build_buildings(features, road):
    """Codori house and barn (ED-13; claim-codori-barn-position)."""
    # orient both to the local road bearing
    cl = road["centerline"]
    near = min(cl, key=lambda s: (s["x"] - BARN_POS[0]) ** 2 + (s["z"] - BARN_POS[1]) ** 2)
    idx = cl.index(near)
    nxt = cl[min(idx + 1, len(cl) - 1)]
    prv = cl[max(idx - 1, 0)]
    bearing = _bearing_deg(prv["x"], prv["z"], nxt["x"], nxt["z"])
    return [
        {
            "id": "codori-house",
            "kind": "house",
            "x": HOUSE_POS[0], "z": HOUSE_POS[1],
            "yawDeg": round(bearing, 1),
            "sizeM": list(HOUSE_SIZE),
            "claimIds": ["claim-codori-house-structure", "claim-codori-barn-position"],
            "editorialIds": ["ED-13"],
        },
        {
            "id": "codori-barn",
            "kind": "barn",
            "x": BARN_POS[0], "z": BARN_POS[1],
            "yawDeg": round(bearing, 1),
            "sizeM": list(BARN_SIZE),
            "claimIds": ["claim-codori-barn-position", "claim-codori-barn-1863"],
            "editorialIds": ["ED-13"],
        },
    ]


def build_battery(features):
    """Cushing's battery materiel at 15:20 (ED-16)."""
    wall_pts = features["wall-angle-webb-front"]["points"]

    def wall_x_at(z):
        for i in range(len(wall_pts) - 1):
            (ax, az), (bx, bz) = wall_pts[i], wall_pts[i + 1]
            lo, hi = min(az, bz), max(az, bz)
            if lo <= z <= hi and az != bz:
                t = (z - az) / (bz - az)
                return ax + t * (bx - ax)
        raise ValueError(f"z {z} not on the wall's N-S run")

    guns = [
        {"x": x, "z": z, "yawDeg": GUN_FACING_DEG, "state": "intact", "at": "wall"}
        for x, z in WALL_GUNS
    ]
    crest_zs = [4850.0, 4870.0, 4893.0, 4912.0]
    for i, z in enumerate(crest_zs):
        x = wall_x_at(z) + CREST_OFFSET_M + 2.0 * _jitter("battery-p7", i, 17)
        guns.append({
            "x": round(x, 2), "z": round(z + 1.5 * _jitter("battery-p7", i, 29), 2),
            "yawDeg": round(GUN_FACING_DEG + 8.0 * _jitter("battery-p7", i, 43), 1),
            "state": "wrecked" if i in (0, 2) else "disabled",
            "at": "crest",
        })
    limbers = []
    caissons = []
    for i in range(6):
        z = 4845.0 + i * 13.0
        base_x = wall_x_at(min(max(z, 4614.0), 4927.0)) + CREST_OFFSET_M
        limbers.append({
            "x": round(base_x + LIMBER_OFFSET_M + 1.5 * _jitter("limber-p7", i, 17), 2),
            "z": round(z + 2.0 * _jitter("limber-p7", i, 29), 2),
            "yawDeg": round(GUN_FACING_DEG + 12.0 * _jitter("limber-p7", i, 43), 1),
        })
        caissons.append({
            "x": round(base_x + CAISSON_OFFSET_M + 2.5 * _jitter("caisson-p7", i, 17), 2),
            "z": round(z + 2.5 * _jitter("caisson-p7", i, 29), 2),
            "yawDeg": round(GUN_FACING_DEG + 15.0 * _jitter("caisson-p7", i, 43), 1),
        })
    # ammunition chests / detritus between the wall guns and the crest
    detritus = []
    for i in range(10):
        z = 4840.0 + 8.5 * i + 3.0 * _jitter("detritus-p7", i, 29)
        x = wall_x_at(min(max(z, 4614.0), 4927.0)) + 6.0 + 18.0 * _hash01("detritus-p7", i * 13 + 7)
        detritus.append({
            "x": round(x, 2), "z": round(z, 2),
            "yawDeg": round(360.0 * _hash01("detritus-p7", i * 17 + 11), 1),
            "kind": "ammo_chest" if i % 2 == 0 else "wheel_wreck",
        })
    return {
        "unitId": "us-btty-cushing",
        "guns": guns,
        "limbers": limbers,
        "caissons": caissons,
        "detritus": detritus,
        "claimIds": ["claim-cushing-armament", "claim-cushing-guns-to-wall",
                     "claim-battery-organization", "claim-ordnance-rifle-dimensions",
                     "claim-corridor-trampled"],
        "editorialIds": ["ED-16"],
    }


def build_wheat_region(features, crop):
    """Standing-wheat/stubble region west of the road (ED-15)."""
    x0, z0, x1, z1 = crop
    poly = features["field-west-of-emmitsburg-codori-front"]["points"]
    clipped = [round(v, 1) for x, z in poly
               for v in (max(x0, min(x1, x)), max(z0, min(z1, z)))]
    return {
        "featureId": "field-west-of-emmitsburg-codori-front",
        "polygonFlat": clipped,
        "clumpSpacingM": 2.4,
        "patchNoiseKey": "wheat-p7",
        "claimIds": ["claim-fields-west-mixed"],
        "editorialIds": ["ED-15"],
    }


# --- splat raster ---

def rasterize_splat(features, road, crop, resolution=SPLAT_RESOLUTION):
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
            zmin, zmax = min(zs), max(zs)
            ri0 = max(0, int((z1 - zmax) / px))
            ri1 = min(resolution - 1, int((z1 - zmin) / px))
            for ri in range(ri0, ri1 + 1):
                z = z1 - (ri + 0.5) * px
                if _point_in_polygon(x, z, poly):
                    grid[ri, ci] = cls

    # fields first (ED-15), so road/trample overwrite them
    for fid, cls in FIELD_CLASSES.items():
        if cls != CLASS_PASTURE and fid in features:
            paint_polygon(features[fid]["points"], cls)

    # trampled corridor (ED-17): east fence -> wall, TRAMPLE_Z band
    east = features["fence-emmitsburg-road-east"]["points"]
    wall = features["wall-angle-webb-front"]["points"]

    def x_on(polyline, z):
        best = _closest_point_on_polyline(0.0, z, polyline)
        # closest point to (0, z) is not what we want; interpolate by z instead
        for i in range(len(polyline) - 1):
            (ax, az), (bx, bz) = polyline[i], polyline[i + 1]
            lo, hi = min(az, bz), max(az, bz)
            if lo <= z <= hi and az != bz:
                return ax + (z - az) / (bz - az) * (bx - ax)
        return best[0]

    tz0, tz1 = TRAMPLE_Z
    quad = [
        [x_on(east, tz0), tz0], [x_on(wall, max(tz0, 4614.0)) , tz0],
        [x_on(wall, min(tz1, 4927.0)) + 60.0, tz1], [x_on(east, tz1), tz1],
    ]
    paint_polygon(quad, CLASS_TRAMPLED)

    # road corridor + wheel paths (ED-11) — painted last, they win
    cl = road["centerline"]
    for i in range(len(cl) - 1):
        a, b = cl[i], cl[i + 1]
        steps = max(2, int(math.hypot(b["x"] - a["x"], b["z"] - a["z"]) / (px * 0.5)))
        dirx, dirz = b["x"] - a["x"], b["z"] - a["z"]
        L = math.hypot(dirx, dirz) or 1.0
        nx, nz = dirz / L, -dirx / L
        for s in range(steps + 1):
            t = s / steps
            cxm = a["x"] + t * (b["x"] - a["x"])
            czm = a["z"] + t * (b["z"] - a["z"])
            half = (a["halfWidth"] + t * (b["halfWidth"] - a["halfWidth"])) - ROAD_SHOULDER_M
            w = 0.0
            while w <= half:
                for side in (-1.0, 1.0):
                    x = cxm + nx * w * side
                    z = czm + nz * w * side
                    ci = int((x - x0) / px)
                    ri = int((z1 - z) / px)
                    if 0 <= ci < resolution and 0 <= ri < resolution:
                        on_wheel = abs(abs(w) - WHEEL_PATH_OFFSET_M) <= WHEEL_PATH_WIDTH_M / 2
                        grid[ri, ci] = CLASS_DISTURBED if on_wheel else CLASS_ROAD
                    if w == 0.0:
                        break
                w += px * 0.5
    return grid


# --- entry point ---

def bake(landcover_path, crop_meta_path, battle_path, out_dir):
    meta = json.loads(Path(crop_meta_path).read_text())
    crop = (meta["crop_x0_m"], meta["crop_z0_m"], meta["crop_x1_m"], meta["crop_z1_m"])
    features = load_features(landcover_path)

    road = build_road(features, crop)
    fences = build_fences(features, crop)
    wall = build_wall(features, battle_path)
    groves, orchards = build_woodlands(features, crop)
    buildings = build_buildings(features, road)
    battery = build_battery(features)
    wheat = build_wheat_region(features, crop)
    splat = rasterize_splat(features, road, crop)

    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    splat_path = out_dir / "environment_splat.raw"
    splat_path.write_bytes(splat.tobytes())

    doc = {
        "name": "Angle crop environment (plan §12 Phase 7)",
        "frame": "battlefield-local meters (macro); Unity stage converts via AngleTerrainFrame",
        "crop": {"x0": crop[0], "z0": crop[1], "x1": crop[2], "z1": crop[3]},
        "provenance": ("every feature carries claimIds (reconstruction/claims/angle.claims.json) "
                       "and/or editorialIds (docs/reconstruction/angle-editorial-decisions.md)"),
        "road": road,
        "fences": fences,
        "wall": wall,
        "groves": groves,
        "orchards": orchards,
        "buildings": buildings,
        "battery": battery,
        "wheat": wheat,
        "splat": {
            "path": "environment_splat.raw",
            "resolution": SPLAT_RESOLUTION,
            "row0": "north",
            "classes": {
                "pasture": CLASS_PASTURE, "dry_grass": CLASS_DRY_GRASS,
                "stubble": CLASS_STUBBLE, "road": CLASS_ROAD,
                "disturbed": CLASS_DISTURBED, "trampled": CLASS_TRAMPLED,
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
