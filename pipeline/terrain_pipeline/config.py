"""Battlefield extent and grid constants for the Gettysburg terrain proof."""
from pyproj import Transformer

# west, south, east, north (WGS84). Covers the main field: the town,
# Seminary/Cemetery Ridges, the Round Tops, Culp's Hill.
BBOX_WGS84 = (-77.28, 39.77, -77.195, 39.845)

WGS84_CRS = "EPSG:4326"
UTM_CRS = "EPSG:26918"  # NAD83 / UTM 18N (meters)

HEIGHTMAP_RESOLUTION = 4097  # Unity terrain heightmaps must be 2^n + 1


def utm_square_bounds(bbox: tuple[float, float, float, float] = BBOX_WGS84):
    """UTM bounds of bbox, expanded to a centered square (meters).

    Square because Unity heightmaps are square; we pad the short axis.
    """
    west, south, east, north = bbox
    t = Transformer.from_crs(WGS84_CRS, UTM_CRS, always_xy=True)
    corners = [t.transform(x, y) for x in (west, east) for y in (south, north)]
    xs = [c[0] for c in corners]
    ys = [c[1] for c in corners]
    minx, maxx, miny, maxy = min(xs), max(xs), min(ys), max(ys)
    side = max(maxx - minx, maxy - miny)
    cx, cy = (minx + maxx) / 2, (miny + maxy) / 2
    return (cx - side / 2, cy - side / 2, cx + side / 2, cy + side / 2)
