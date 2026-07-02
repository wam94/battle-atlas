"""Rasterize land-cover polygons (docs/format/landcover-format.md) into a
Unity-ready splat map: a 4-channel RGBA PNG.

Orientation contract: SAME row-0-is-north convention as the heightmap
(see export.py). `rasterize_splats` builds its transform with
`rasterio.transform.from_bounds(0, 0, size_m, size_m, resolution, resolution)`,
which — like `process.build_square_dem`'s `from_bounds(minx, miny, maxx, maxy,
...)` — maps row 0 of the output array to the maximum-z (north) edge and the
last row to z=0 (south). Land-cover feature points are battlefield-local
meters (x east, z north from the SW corner; see landcover-format.md), so this
falls out of `from_bounds` for free: increasing z moves toward row 0.

Channel layout: R=field, G=woodlot (woods-floor), B=orchard, A=marsh.
`pasture` is the implied base layer (no channel of its own) — terrain reads
as pasture wherever none of the four channels are painted.

Overlap semantics ("later wins"): where two features' polygons cover the same
pixel, the LATER-listed feature in `features` wins outright, regardless of
class — the pixel takes on the later feature's class (or no class, if the
later feature is `pasture`/a line), clearing any earlier paint at that pixel.
This is implemented with a single class-index raster: each pixel is burned
with the index (in `features`) of the last polygon covering it, then the
4 output channels are derived by comparing each pixel's winning index against
each feature's class. `line` features never win any pixel (they contribute
no geometry to the class-index raster).

Tree placement (`tree_placements`): woodlot and orchard polygons get baked
instance points. Determinism (same input -> same output, every call, every
machine) matters more than true randomness, so both classes derive jitter
from `_jitter`, an FNV-1a-style string/int hash mirroring the pattern used
client-side by `FormationLayout.Jitter` (app/Assets/Scripts/FormationLayout.cs)
and `VegetationField.Placements` (app/Assets/Scripts/VegetationField.cs) —
hash the feature id + candidate index + a salt, fold it into a float in
[-1, 1]. This is a *convention match*, not a wire-format requirement: nothing
here needs the Python and C# hashes to produce identical numbers for the same
inputs, only that each language's own placements are stable run over run.
- woodlot: a grid at 9m pitch is laid over the polygon's bbox; each grid
  point is jittered up to +-3.5m in x and z (two independent `_jitter` draws
  per point, salts 31/37 to match VegetationField's salts for x/y jitter);
  points that land outside the polygon (pre- or post-jitter) are dropped.
  Nominal density is 1 candidate per 9m x 9m cell (~1/(9m)^2), and the +-3.5m
  jitter is small relative to the 9m pitch, so actual yield after edge
  clipping lands close to nominal.
- orchard: same bbox-grid approach at 8m pitch, but jitter is only +-0.8m —
  small enough that rows stay visibly regular, unlike the loose woodlot
  scatter.

Fence posts (`fence_posts`): line features are resampled at a fixed spacing
(both endpoints always included) into posts carrying a compass bearing per
segment, matching the battle format's facing convention (0 = north/+z,
90 = east/+x; see docs/format/battle-format.md's `keyframes[].facing`).
"""
import math
from pathlib import Path

import numpy as np
from PIL import Image
from rasterio.features import rasterize
from rasterio.transform import from_bounds
from shapely.geometry import Point, Polygon

# Polygon class -> output channel index (R, G, B, A). Pasture has none.
_CHANNELS = {
    "field": 0,
    "woodlot": 1,
    "orchard": 2,
    "marsh": 3,
}


def _polygon_geometry(points):
    ring = list(points) + [points[0]]
    return {"type": "Polygon", "coordinates": [ring]}


def rasterize_splats(features, size_m, resolution):
    """Rasterize polygon land-cover features into a (resolution, resolution, 4)
    uint8 RGBA array. See module docstring for orientation and overlap rules.

    `line` features contribute nothing (fences/walls are placed separately,
    not baked into the splat map).
    """
    transform = from_bounds(0, 0, size_m, size_m, resolution, resolution)

    # Burn each polygon feature's array index (1-based; 0 = untouched/pasture)
    # in feature order, so a single rasterize() call resolves "later wins"
    # for us (rasterio burns shapes in list order, later overwriting earlier
    # at shared pixels).
    shapes = []
    for i, feature in enumerate(features):
        if feature["kind"] != "polygon":
            continue
        shapes.append((_polygon_geometry(feature["points"]), i + 1))

    out = np.zeros((resolution, resolution, 4), dtype=np.uint8)
    if not shapes:
        return out

    winner = rasterize(
        shapes,
        out_shape=(resolution, resolution),
        fill=0,
        transform=transform,
        dtype=np.int32,
    )

    for i, feature in enumerate(features):
        if feature["kind"] != "polygon":
            continue
        channel = _CHANNELS.get(feature["cls"])
        if channel is None:  # pasture: implied base, no channel to paint
            continue
        out[:, :, channel] = np.where(winner == (i + 1), 255, out[:, :, channel])

    return out


def write_splatmap(array, path):
    """Write a (resolution, resolution, 4) uint8 RGBA array as a PNG."""
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(array, mode="RGBA").save(path)
    return path


# --- tree placement ----------------------------------------------------

# Tree class config: grid pitch (m) and per-axis jitter half-width (m).
# See module docstring "Tree placement" for the density/regularity rationale.
_TREE_CLASSES = {
    "woodlot": {"pitch": 9.0, "jitter": 3.5},
    "orchard": {"pitch": 8.0, "jitter": 0.8},
}


def _jitter(feature_id, i, salt):
    """Deterministic pseudo-random float in [-1, 1] from (feature_id, i, salt).

    FNV-1a-style hash mirroring FormationLayout.Jitter (C#) / the same
    pattern used by VegetationField.Placements — see module docstring:
    determinism is the requirement, not cross-language numeric equality.
    """
    h = 2166136261
    for c in feature_id:
        h = ((h ^ ord(c)) * 16777619) & 0xFFFFFFFF
    h = ((h ^ i) * 16777619) & 0xFFFFFFFF
    h = ((h ^ salt) * 16777619) & 0xFFFFFFFF
    # extra avalanche mix (same shape as the C# xorshift-multiply-xorshift)
    h ^= h >> 13
    h = (h * 0x5BD1E995) & 0xFFFFFFFF
    h ^= h >> 15
    return ((h & 0xFFFFFF) / float(0xFFFFFF)) * 2.0 - 1.0


def _grid_candidates(polygon, pitch):
    """Grid points at `pitch` spacing covering the polygon's bbox."""
    minx, minz, maxx, maxz = polygon.bounds
    points = []
    z = minz
    while z <= maxz:
        x = minx
        while x <= maxx:
            points.append((x, z))
            x += pitch
        z += pitch
    return points


def tree_placements(features):
    """Deterministic tree instance points for woodlot/orchard polygons.

    Returns a list of {"x", "z", "cls"} dicts. See module docstring for the
    grid+jitter algorithm and why determinism (not cross-language hash
    equality) is the bar.
    """
    trees = []
    for feature in features:
        if feature["kind"] != "polygon":
            continue
        cfg = _TREE_CLASSES.get(feature["cls"])
        if cfg is None:
            continue

        polygon = Polygon(feature["points"])
        pitch, jitter = cfg["pitch"], cfg["jitter"]

        for i, (gx, gz) in enumerate(_grid_candidates(polygon, pitch)):
            x = gx + _jitter(feature["id"], i, 31) * jitter
            z = gz + _jitter(feature["id"], i, 37) * jitter
            if polygon.contains(Point(x, z)):
                trees.append({"x": x, "z": z, "cls": feature["cls"]})

    return trees


# --- fence posts ---------------------------------------------------------


def _bearing_deg(x0, z0, x1, z1):
    """Compass bearing of the segment (x0,z0)->(x1,z1): 0=north/+z,
    90=east/+x — the battle format's facing convention
    (docs/format/battle-format.md `keyframes[].facing`)."""
    dx, dz = x1 - x0, z1 - z0
    return math.degrees(math.atan2(dx, dz)) % 360


def fence_posts(features, spacing=3.0):
    """Resample line features into posts at `spacing` meters, both endpoints
    included, each carrying the compass bearing of its segment.

    Returns a list of {"x", "z", "bearing_deg", "cls"} dicts.
    """
    posts = []
    for feature in features:
        if feature["kind"] != "line":
            continue

        pts = feature["points"]
        for seg_start, seg_end in zip(pts[:-1], pts[1:]):
            x0, z0 = seg_start
            x1, z1 = seg_end
            length = math.hypot(x1 - x0, z1 - z0)
            bearing = _bearing_deg(x0, z0, x1, z1)
            if length == 0:
                continue

            steps = max(1, math.ceil(length / spacing))
            for i in range(steps + 1):
                t = min(i * spacing / length, 1.0)
                # skip the segment-start post when it coincides with the
                # previous segment's end (shared vertex of a bent line)
                if i == 0 and posts and feature["kind"] == "line":
                    prev = posts[-1]
                    if abs(prev["x"] - x0) < 1e-9 and abs(prev["z"] - z0) < 1e-9 and prev["cls"] == feature["cls"]:
                        continue
                posts.append({
                    "x": x0 + (x1 - x0) * t,
                    "z": z0 + (z1 - z0) * t,
                    "bearing_deg": bearing,
                    "cls": feature["cls"],
                })

    return posts
