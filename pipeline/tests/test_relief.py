import numpy as np
from PIL import Image

from terrain_pipeline import relief


def _grid(res, pixel_m):
    """Texel-center coordinates in meters. Row 0 = north (max z), matching
    the heightmap orientation contract: z DECREASES with row index."""
    centers = (np.arange(res) + 0.5) * pixel_m
    x = np.tile(centers, (res, 1))            # x east, increasing with column
    z = np.tile(centers[::-1, None], (1, res))  # z north, row 0 = max z
    return x, z


def _bowl(res=64, pixel_m=10.0, depth=20.0, sigma=80.0, cx=None, cz=None):
    """Flat plain with a gaussian pit (a swale): center concave and
    sky-occluded, surrounding rim annulus convex."""
    x, z = _grid(res, pixel_m)
    side = res * pixel_m
    cx = side / 2 if cx is None else cx
    cz = side / 2 if cz is None else cz
    r2 = (x - cx) ** 2 + (z - cz) ** 2
    return 100.0 - depth * np.exp(-r2 / (2 * sigma**2))


def _ridge(res=64, pixel_m=10.0, height=15.0, sigma=40.0):
    """Flat plain with a gaussian ridge running east-west across the middle:
    crest convex, plain flat."""
    x, z = _grid(res, pixel_m)
    side = res * pixel_m
    return 100.0 + height * np.exp(-((z - side / 2) ** 2) / (2 * sigma**2))


def test_bowl_center_darker_than_rim():
    # A swale must darken: the bowl center is concave (positive Laplacian)
    # AND sees less sky hemisphere (low sky-view factor); the rim annulus
    # is convex. Center modulation < rim modulation, and center < neutral.
    heights = _bowl()
    mult = relief.bake_relief(heights, pixel_size_m=10.0)

    c = 32  # bowl center texel
    rim = 32 + 24  # ~3 sigma out: the convex shoulder, clear of the pit's
    # sky-view shadow (inside ~2 sigma the horizon deficit still darkens)
    assert mult[c, c] < mult[c, rim]
    assert mult[c, c] < 1.0
    # the convex rim reads lighter than the concave center by construction;
    # it must also not read DARKER than the flat far corner
    assert mult[c, rim] >= mult[2, 2] - 1e-9


def test_ridge_crest_lighter_than_plain():
    # A crest must lighten: convex (negative Laplacian) with full sky view.
    heights = _ridge()
    mult = relief.bake_relief(heights, pixel_size_m=10.0)

    crest = mult[32, 32]
    plain = mult[2, 32]  # far from the ridge
    assert crest > plain
    assert crest > 1.0


def test_modulation_respects_clamp():
    # Absurdly extreme terrain must still clamp to +-CLAMP so the bake reads
    # as ground variation, not paint. The contour variant may go at most
    # CONTOUR_DARKEN further down, never below ENCODE_MIN.
    res, pixel_m = 64, 10.0
    x, z = _grid(res, pixel_m)
    heights = 1000.0 * np.sin(x / 50.0) * np.cos(z / 70.0)

    mult = relief.bake_relief(heights, pixel_size_m=pixel_m)
    assert (mult >= 1.0 - relief.CLAMP - 1e-12).all()
    assert (mult <= 1.0 + relief.CLAMP + 1e-12).all()
    # extreme terrain should actually REACH the clamp somewhere (the clamp
    # is doing work, not vacuously true)
    assert np.isclose(mult.min(), 1.0 - relief.CLAMP)
    assert np.isclose(mult.max(), 1.0 + relief.CLAMP)

    contoured = relief.bake_relief_contours(heights, pixel_size_m=pixel_m)
    floor = max(1.0 - relief.CLAMP - relief.CONTOUR_DARKEN, relief.ENCODE_MIN)
    assert (contoured >= floor - 1e-12).all()
    assert (contoured <= 1.0 + relief.CLAMP + 1e-12).all()


def test_bake_deterministic_byte_identical(tmp_path):
    # Same DEM in -> byte-identical PNGs out, every run, both variants.
    # Determinism over randomness is the pipeline house rule (landcover.py).
    a = _bowl()
    b = _bowl()

    p1 = relief.write_relief(relief.bake_relief(a, pixel_size_m=10.0), tmp_path / "r1.png")
    p2 = relief.write_relief(relief.bake_relief(b, pixel_size_m=10.0), tmp_path / "r2.png")
    assert p1.read_bytes() == p2.read_bytes()

    c1 = relief.write_relief(
        relief.bake_relief_contours(a, pixel_size_m=10.0), tmp_path / "c1.png"
    )
    c2 = relief.write_relief(
        relief.bake_relief_contours(b, pixel_size_m=10.0), tmp_path / "c2.png"
    )
    assert c1.read_bytes() == c2.read_bytes()
    # and the contour variant is genuinely different from the base bake
    assert c1.read_bytes() != p1.read_bytes()


def test_row0_north_orientation_pin(tmp_path):
    # Orientation contract: row 0 of the input DEM is the NORTH edge (same
    # as heightmap.raw / splatmap.png), and row 0 of the DEM maps to row 0
    # (top) of the written PNG. A swale in the NORTH half must darken the
    # TOP rows of the PNG.
    res, pixel_m = 64, 10.0
    side = res * pixel_m
    # pit centered in the north half: z = 3/4 of the way up -> rows ~ res/4
    heights = _bowl(res=res, pixel_m=pixel_m, depth=30.0, sigma=60.0,
                    cx=side / 2, cz=side * 0.75)

    mult = relief.bake_relief(heights, pixel_size_m=pixel_m)
    path = relief.write_relief(mult, tmp_path / "relief.png")

    img = np.array(Image.open(path))
    assert img.shape == (res, res)
    darkest_row = np.unravel_index(np.argmin(img), img.shape)[0]
    assert darkest_row < res // 2, "north-half swale must darken the top of the PNG"
    assert img[: res // 2].mean() < img[res // 2 :].mean()
