"""Bake a terrain relief-modulation texture from the measured DEM.

HONESTY NOTE: every band in this texture is a derivative of the measured
elevation data (data/heightmap/heightmap.raw) — sky-view factor is the DEM's
hemisphere visibility, curvature is its Laplacian, contour lines are its
level sets. Nothing is painted by hand; the bake adds no fictional relief.

Two grayscale multiplier textures (research doc
2026-07-02-descriptive-graphics-techniques.md sections 3b and 4 — the mobile
replacement for SSAO, which stays banned):

- relief.png — sky-view factor/AO (how much sky hemisphere each texel sees:
  swales, the Plum Run valley, and reverse slopes darken regardless of sun
  position; time-invariant, so it composes honestly with the scrubbable
  ephemeris sun) combined with curvature (Laplacian-of-DEM: concave slightly
  darker/cooler, convex crests slightly lighter/warmer) into ONE modulation
  layer, clamped to +-CLAMP (10% — the middle of the plan's 8-12% band) so
  it reads as ground variation, not paint.
- relief_contours.png — the same bake with elevation contour lines at
  CONTOUR_INTERVAL_DISPLAY_M (3 m at display scale) darkened a further
  CONTOUR_DARKEN (4%): perfectly honest (it IS the DEM) and it quietly
  labels every swale. The runtime toggle wiring lands with the importer.

Vertical exaggeration: heights are scaled by DISPLAY_VERTICAL_EXAGGERATION
(2.5, matching HeightmapImporter.cs VerticalExaggeration) before both the
sky-view and curvature passes, so the baked shading agrees with the rendered
geometry rather than the un-exaggerated ground.

Orientation contract: row 0 of the input DEM is the NORTH edge — SAME
convention as heightmap.raw (export.py) and splatmap.png (landcover.py) —
and row 0 of the array is row 0 (top) of the written PNG. Pinned by test.

Encoding: 8-bit grayscale PNG, byte value b decoding to
    multiplier = ENCODE_MIN + b / 255 * (ENCODE_MAX - ENCODE_MIN)
i.e. the full byte range spans [0.85, 1.15] so the +-10% (+4% contour) band
gets ~0.12% steps instead of the ~0.8% steps a naive 128-neutral encoding
would give (no banding across smooth slopes). The importer multiplies layer
tints by the decoded value; the CLI records the constants in relief.json.

Determinism: pure numpy on the same input produces the same bytes, run over
run — the same bar landcover.py sets (determinism over randomness).
"""
import json
import math
from pathlib import Path

import numpy as np
from PIL import Image

# Matches app/Assets/Editor/HeightmapImporter.cs VerticalExaggeration: the
# bake must shade the terrain the viewer actually sees.
DISPLAY_VERTICAL_EXAGGERATION = 2.5

# Max luminance modulation, +-. The plan allows 8-12%; 10% is the middle of
# the band — strong enough that dead ground reads, weak enough that the
# splat tints still read as ground type.
CLAMP = 0.10

# Contour lines: interval in DISPLAY meters (i.e. after the 2.5x
# exaggeration, so contours agree with the rendered geometry's apparent
# relief), darkened this much further than the base bake.
CONTOUR_INTERVAL_DISPLAY_M = 3.0
CONTOUR_DARKEN = 0.04

# Gains from derivative to luminance delta, then clamped. Tuned on the real
# Gettysburg DEM at the 1024 bake so the modulation reads as ground
# variation, not paint: ~92% of texels land inside the clamp band (median
# -2.6%, quartiles -5%..-0.5%, p95 +3.4%); only the deepest swale walls and
# creek cuts reach the clamp. Note the AO term only DARKENS (that is what
# ambient occlusion means — flat open ground is the 1.0 ceiling); lightening
# comes from convex curvature.
SVF_GAIN = 0.4
CURVATURE_GAIN = 0.2

# Curvature is sampled at a 3-texel step (~25 m at the 1024 bake) — the
# scale of swale floors and ridge crests. The 1-texel Laplacian of a LiDAR
# DEM is dominated by micro-roughness (measured: quartiles quadruple moving
# from landform scale to texel scale), which would bake in noise, not form.
_CURVATURE_STEP_PX = 3

# PNG byte range decodes to this multiplier span (see module docstring).
# Wide enough for CLAMP + CONTOUR_DARKEN on the dark side, symmetric so
# neutral 1.0 sits at mid-gray.
ENCODE_MIN = 0.85
ENCODE_MAX = 1.15

# Sky-view sampling: 8 azimuth directions, horizon scanned at these pixel
# distances (geometric-ish spacing — near samples catch sharp banks, far
# samples catch broad valley walls). At the 1024 bake a texel is ~8.3 m, so
# the 64 px max radius scans ~530 m of horizon: swale-scale occlusion, not
# mountain-scale.
_SVF_DIRECTIONS = 8
_SVF_DISTANCES_PX = (1, 2, 3, 4, 6, 8, 11, 16, 23, 32, 45, 64)


def load_heightmap(heightmap_dir):
    """Read heightmap.raw + heightmap.json (export.py's format) back into
    elevation meters. Returns (heights float64 [res, res] row-0-north, meta).
    """
    heightmap_dir = Path(heightmap_dir)
    meta = json.loads((heightmap_dir / "heightmap.json").read_text())
    raw = np.frombuffer((heightmap_dir / "heightmap.raw").read_bytes(), dtype="<u2")
    res = meta["resolution"]
    norm = raw.reshape(res, res).astype(np.float64) / 65535.0
    heights = meta["min_elev_m"] + norm * (meta["max_elev_m"] - meta["min_elev_m"])
    return heights, meta


def downsample(heights, resolution):
    """Block-mean the DEM down to `resolution` (e.g. 4097 -> 1024 via 4x4
    block means, dropping the last DEM row/col — a sub-texel sliver at the
    south/east edge). Mean (not decimation) so the bake sees anti-aliased
    ground, and deterministically so.
    """
    src = heights.shape[0]
    block = src // resolution
    trimmed = heights[: block * resolution, : block * resolution]
    return trimmed.reshape(resolution, block, resolution, block).mean(axis=(1, 3))


def sky_view_factor(display_heights, pixel_size_m):
    """Fraction of the sky hemisphere visible from each texel, in [0, 1].

    For each of _SVF_DIRECTIONS azimuths, find the maximum horizon tangent
    over _SVF_DISTANCES_PX samples; SVF = mean over directions of
    1 - sin(horizon angle). Flat ground -> 1.0; a swale floor sees less.
    Edges are padded with their own values (flat continuation — the world
    does not fall away at the map edge).
    """
    res_y, res_x = display_heights.shape
    pad = max(_SVF_DISTANCES_PX)
    padded = np.pad(display_heights, pad, mode="edge")

    total = np.zeros(display_heights.shape, dtype=np.float64)
    for k in range(_SVF_DIRECTIONS):
        angle = 2.0 * math.pi * k / _SVF_DIRECTIONS
        ux, uy = math.cos(angle), math.sin(angle)
        max_tan = np.zeros(display_heights.shape, dtype=np.float64)
        for d in _SVF_DISTANCES_PX:
            ox, oy = round(ux * d), round(uy * d)
            if ox == 0 and oy == 0:
                continue
            dist_m = math.hypot(ox, oy) * pixel_size_m
            shifted = padded[pad + oy : pad + oy + res_y, pad + ox : pad + ox + res_x]
            np.maximum(max_tan, (shifted - display_heights) / dist_m, out=max_tan)
        # sin(atan(t)) = t / sqrt(1 + t^2); below-horizontal never occludes
        total += 1.0 - max_tan / np.sqrt(1.0 + max_tan**2)
    return total / _SVF_DIRECTIONS


def _laplacian(heights, step=1):
    """4-neighbor Laplacian at `step` texels (height units per step^2),
    edge-padded. Positive = concave (neighbors higher), negative = convex
    crest."""
    k = step
    p = np.pad(heights, k, mode="edge")
    center = p[k:-k, k:-k]
    return p[: -2 * k, k:-k] + p[2 * k :, k:-k] + p[k:-k, : -2 * k] + p[k:-k, 2 * k :] - 4.0 * center


def bake_relief(heights_m, pixel_size_m, exaggeration=DISPLAY_VERTICAL_EXAGGERATION):
    """Combined sky-view + curvature modulation multiplier per texel,
    clamped to [1 - CLAMP, 1 + CLAMP]. Input is elevation METERS,
    row 0 = north; exaggeration is applied here so callers pass real ground.
    """
    disp = heights_m.astype(np.float64) * exaggeration
    svf = sky_view_factor(disp, pixel_size_m)
    lap = _laplacian(disp, _CURVATURE_STEP_PX)
    delta = SVF_GAIN * (svf - 1.0) - CURVATURE_GAIN * lap / (_CURVATURE_STEP_PX * pixel_size_m)
    return 1.0 + np.clip(delta, -CLAMP, CLAMP)


def contour_mask(heights_m, exaggeration=DISPLAY_VERTICAL_EXAGGERATION,
                 interval=CONTOUR_INTERVAL_DISPLAY_M):
    """Boolean mask of contour-line texels: where the display-scale elevation
    crosses an `interval` level set between a texel and its east or south
    neighbor (one-texel-wide lines)."""
    band = np.floor(heights_m * exaggeration / interval)
    mask = np.zeros(band.shape, dtype=bool)
    mask[:, :-1] |= band[:, :-1] != band[:, 1:]
    mask[:-1, :] |= band[:-1, :] != band[1:, :]
    return mask


def bake_relief_contours(heights_m, pixel_size_m,
                         exaggeration=DISPLAY_VERTICAL_EXAGGERATION):
    """The base bake with contour lines darkened a further CONTOUR_DARKEN,
    floored at ENCODE_MIN (contours stay visible even inside a fully
    clamped-dark swale, within the encodable range)."""
    mult = bake_relief(heights_m, pixel_size_m, exaggeration)
    mask = contour_mask(heights_m, exaggeration)
    return np.where(mask, np.maximum(mult - CONTOUR_DARKEN, ENCODE_MIN), mult)


def encode(mult):
    """Multiplier array -> uint8 bytes per the module-docstring encoding."""
    norm = (np.clip(mult, ENCODE_MIN, ENCODE_MAX) - ENCODE_MIN) / (ENCODE_MAX - ENCODE_MIN)
    return np.round(norm * 255.0).astype(np.uint8)


def write_relief(mult, path):
    """Write a multiplier array as an 8-bit grayscale PNG (row 0 = north)."""
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(encode(mult), mode="L").save(path)
    return path
