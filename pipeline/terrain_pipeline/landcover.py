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
"""
from pathlib import Path

import numpy as np
from PIL import Image
from rasterio.features import rasterize
from rasterio.transform import from_bounds

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
