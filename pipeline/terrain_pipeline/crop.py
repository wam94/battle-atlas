"""Crop a true-scale local terrain tile from the cached 1 m DEM.

Local-frame convention (shared with the Unity Atlas): battlefield-local
``x`` is meters east of the macro heightmap's ``origin_utm_e`` and ``z`` is
meters north of ``origin_utm_n``. The macro Unity terrain sits at the world
origin, so battlefield-local (x, z) ARE Unity world coordinates in the
Atlas scene. A crop is a square sub-tile of that frame whose own origin is
at ``(x0, z0)``; positions local to the crop map back to the macro frame by
adding the crop origin — see :func:`crop_local_to_macro`.

V2 plan §8.1 acceptance encoded here:
- ``vertical_exaggeration`` is written as 1.0 (true scale) and the Unity
  importer must consume it rather than applying its own display factor;
- sample spacing must be <= ~1 m (Unity terrain resolutions are 2^n + 1,
  so an 800 m crop at 1025 samples gives 0.78125 m spacing);
- positions round-trip to the macro battlefield frame (tested);
- the source DEM is the USGS 3DEP 1 m bare-earth DTM: lidar returns from
  vegetation and above-ground structures (monuments, buildings, power
  lines) are already removed by USGS processing, so no modern above-ground
  structure is baked into the crop surface.

Grid convention: rasterization is cell-centered (rasterio ``from_bounds``
with ``resolution`` cells across the extent — the same convention the
macro `build` uses), while Unity treats heightmap samples as corner-aligned
(spacing = extent / (resolution - 1)). The disagreement is half a texel
(~0.4 m across an 800 m tile) — under the 1 m DEM's own horizontal
accuracy, and identical to the accepted macro-terrain convention.
"""
import json
from pathlib import Path

from terrain_pipeline import export, process

# Unity terrain heightmaps must be 2^n + 1 samples on a side.
CROP_RESOLUTION = 1025

# Plan §8.1's suggested first crop, battlefield-local meters: covers the
# Emmitsburg Road crossing used by Pickett's division, the Codori approach,
# the stone wall and Angle, the Copse of Trees, and Cushing's position.
DEFAULT_CROP = (3900.0, 4450.0, 4700.0, 5250.0)  # x0, z0, x1, z1


def crop_local_to_macro(x_local, z_local, x0, z0):
    """Crop-local meters -> battlefield-local (macro / Unity Atlas) meters."""
    return (x0 + x_local, z0 + z_local)


def macro_to_crop_local(x_macro, z_macro, x0, z0):
    """Battlefield-local (macro) meters -> crop-local meters."""
    return (x_macro - x0, z_macro - z0)


def macro_to_utm(x_macro, z_macro, origin_utm_e, origin_utm_n):
    """Battlefield-local meters -> UTM easting/northing (macro frame datum)."""
    return (origin_utm_e + x_macro, origin_utm_n + z_macro)


def utm_to_macro(easting, northing, origin_utm_e, origin_utm_n):
    """UTM easting/northing -> battlefield-local meters."""
    return (easting - origin_utm_e, northing - origin_utm_n)


def crop_utm_bounds(macro_meta, x0, z0, x1, z1):
    """UTM (minx, miny, maxx, maxy) of a battlefield-local crop window."""
    e = macro_meta["origin_utm_e"]
    n = macro_meta["origin_utm_n"]
    return (e + x0, n + z0, e + x1, n + z1)


def validate_crop_window(x0, z0, x1, z1, resolution=CROP_RESOLUTION):
    """Reject windows the exporter or Unity would silently distort."""
    if x1 <= x0 or z1 <= z0:
        raise ValueError(f"empty crop window ({x0},{z0})..({x1},{z1})")
    width = x1 - x0
    depth = z1 - z0
    if abs(width - depth) > 1e-9:
        raise ValueError(
            f"crop must be square (Unity heightmaps are square); "
            f"got {width} x {depth} m")
    spacing = width / (resolution - 1)
    if spacing > 1.0 + 1e-9:
        raise ValueError(
            f"sample spacing {spacing:.3f} m exceeds the 1 m acceptance "
            f"(plan §8.1); enlarge resolution or shrink the window")
    return spacing


def build_crop(tif_paths, macro_meta, x0, z0, x1, z1,
               resolution=CROP_RESOLUTION):
    """Mosaic + reproject the cached DEM tiles onto the crop grid.

    Returns (heights, spacing_m). Heights are float32, row 0 = north,
    true elevation meters (no exaggeration anywhere in the data path).
    """
    spacing = validate_crop_window(x0, z0, x1, z1, resolution)
    bounds = crop_utm_bounds(macro_meta, x0, z0, x1, z1)
    heights = process.build_square_dem(
        tif_paths, bounds, macro_meta["crs"], resolution)
    return heights, spacing


def export_crop(heights, macro_meta, x0, z0, x1, z1, out_dir,
                spacing_m):
    """Write heightmap.raw/.json for the crop, with local-frame metadata.

    Reuses the macro exporter (same RAW convention, row 0 = north; the
    Unity decoder is shared), then extends the sidecar with the crop's
    battlefield-local placement and the true-scale contract.
    """
    bounds = crop_utm_bounds(macro_meta, x0, z0, x1, z1)
    raw_path, meta_path = export.export_unity_heightmap(
        heights, bounds, out_dir, macro_meta["crs"])

    meta = json.loads(meta_path.read_text())
    meta.update({
        # battlefield-local placement: macro = crop origin + crop-local
        "crop_x0_m": float(x0),
        "crop_z0_m": float(z0),
        "crop_x1_m": float(x1),
        "crop_z1_m": float(z1),
        "macro_origin_utm_e": float(macro_meta["origin_utm_e"]),
        "macro_origin_utm_n": float(macro_meta["origin_utm_n"]),
        "sample_spacing_m": float(spacing_m),
        # locked V2 decision: all new work is true scale
        "vertical_exaggeration": 1.0,
        "surface": "USGS 3DEP 1 m bare-earth DTM (no above-ground structures)",
    })
    meta_path.write_text(json.dumps(meta, indent=2))
    return raw_path, meta_path


def load_macro_meta(heightmap_dir):
    """Load the macro heightmap sidecar that defines the local frame datum."""
    meta_path = Path(heightmap_dir) / "heightmap.json"
    if not meta_path.exists():
        raise FileNotFoundError(
            f"no {meta_path}; run `build` first — the crop's local frame is "
            f"defined relative to the macro heightmap origin")
    return json.loads(meta_path.read_text())
