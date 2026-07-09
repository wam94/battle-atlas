import json

import numpy as np
import pytest
import rasterio
from rasterio.transform import from_bounds

from terrain_pipeline import crop

# a tiny fake macro frame: origin at UTM (300000, 4400000), 2 km square
MACRO_META = {
    "origin_utm_e": 300000.0,
    "origin_utm_n": 4400000.0,
    "width_m": 2000.0,
    "depth_m": 2000.0,
    "crs": "EPSG:26918",
}


def write_tif(path, arr, bounds, crs="EPSG:26918", nodata=-9999.0):
    transform = from_bounds(*bounds, arr.shape[1], arr.shape[0])
    with rasterio.open(
        path, "w", driver="GTiff",
        height=arr.shape[0], width=arr.shape[1], count=1,
        dtype=arr.dtype, crs=crs, transform=transform, nodata=nodata,
    ) as ds:
        ds.write(arr, 1)


def gradient_tif(tmp_path):
    """1 m/px DEM over local 0..2000 m: elevation = easting - origin (m)."""
    cols = np.arange(2000, dtype=np.float32)
    arr = np.tile(cols, (2000, 1))  # elevation == local x at cell center + 0.5
    bounds = (300000.0, 4400000.0, 302000.0, 4402000.0)
    path = tmp_path / "grad.tif"
    write_tif(path, arr, bounds)
    return path


def test_local_macro_round_trip_is_exact():
    x0, z0 = 3900.0, 4450.0
    for p in [(0.0, 0.0), (123.456, 789.012), (800.0, 800.0)]:
        macro = crop.crop_local_to_macro(*p, x0, z0)
        back = crop.macro_to_crop_local(*macro, x0, z0)
        # float add/subtract round trip: exact to well under a millimeter
        assert back[0] == pytest.approx(p[0], abs=1e-9)
        assert back[1] == pytest.approx(p[1], abs=1e-9)


def test_macro_utm_round_trip_is_exact():
    e, n = MACRO_META["origin_utm_e"], MACRO_META["origin_utm_n"]
    for p in [(0.0, 0.0), (4415.0, 4855.0)]:
        utm = crop.macro_to_utm(*p, e, n)
        assert crop.utm_to_macro(*utm, e, n) == p


def test_crop_utm_bounds_offsets_from_macro_origin():
    bounds = crop.crop_utm_bounds(MACRO_META, 100.0, 200.0, 900.0, 1000.0)
    assert bounds == (300100.0, 4400200.0, 300900.0, 4401000.0)


def test_rejects_non_square_window():
    with pytest.raises(ValueError, match="square"):
        crop.validate_crop_window(0, 0, 800, 700)


def test_rejects_empty_window():
    with pytest.raises(ValueError, match="empty"):
        crop.validate_crop_window(100, 100, 100, 900)


def test_rejects_spacing_coarser_than_one_meter():
    # 2000 m window at 1025 samples = 1.95 m spacing > 1 m acceptance
    with pytest.raises(ValueError, match="spacing"):
        crop.validate_crop_window(0, 0, 2000, 2000)


def test_default_crop_satisfies_acceptance_spacing():
    x0, z0, x1, z1 = crop.DEFAULT_CROP
    spacing = crop.validate_crop_window(x0, z0, x1, z1)
    assert spacing <= 1.0
    assert spacing == pytest.approx(800.0 / 1024.0)


def test_crop_heights_round_trip_to_macro_frame(tmp_path):
    """Elevations sampled from the crop match the macro-frame DEM truth.

    The synthetic DEM encodes elevation = battlefield-local x, so a correct
    crop must read `x0 + crop-local x` everywhere — position information
    survives the trip local -> UTM -> raster -> crop grid.
    """
    tif = gradient_tif(tmp_path)
    x0, z0, x1, z1 = 400.0, 600.0, 1424.0, 1624.0  # 1024 m square
    heights, spacing = crop.build_crop(
        [tif], MACRO_META, x0, z0, x1, z1, resolution=1025)

    assert heights.shape == (1025, 1025)
    assert spacing == pytest.approx(1.0)
    assert np.isfinite(heights).all()

    # Rasterization is cell-centered (rasterio from_bounds; the same
    # convention the macro build uses): sample c sits at local
    # x = (c + 0.5) * extent / resolution. The synthetic DEM is the
    # continuous function elevation(x) = macro x - 0.5 under bilinear
    # interpolation (cell k spans [k, k+1] with value k), so a correct
    # crop reads that function at each sample's macro position.
    extent = x1 - x0
    for c in [10, 512, 1000]:
        local_x = (c + 0.5) * extent / 1025
        macro_x, _ = crop.crop_local_to_macro(local_x, 0.0, x0, z0)
        assert heights[512, c] == pytest.approx(macro_x - 0.5, abs=0.02)
    # and the gradient runs along x only: rows are constant
    assert np.allclose(heights[10], heights[900], atol=0.01)


def test_export_crop_metadata_contract(tmp_path):
    tif = gradient_tif(tmp_path)
    x0, z0, x1, z1 = 400.0, 600.0, 912.0, 1112.0  # 512 m square
    heights, spacing = crop.build_crop(
        [tif], MACRO_META, x0, z0, x1, z1, resolution=513)
    raw_path, meta_path = crop.export_crop(
        heights, MACRO_META, x0, z0, x1, z1, tmp_path / "out", spacing)

    meta = json.loads(meta_path.read_text())
    # §8.1 acceptance: true scale, <= 1 m spacing, macro round-trip data
    assert meta["vertical_exaggeration"] == 1.0
    assert meta["sample_spacing_m"] <= 1.0
    assert meta["crop_x0_m"] == x0
    assert meta["crop_z0_m"] == z0
    assert meta["macro_origin_utm_e"] == MACRO_META["origin_utm_e"]
    assert meta["macro_origin_utm_n"] == MACRO_META["origin_utm_n"]
    assert meta["row0"] == "north"
    assert meta["width_m"] == pytest.approx(512.0)
    # UTM origin of the crop tile itself = macro origin + crop origin
    assert meta["origin_utm_e"] == pytest.approx(300400.0)
    assert meta["origin_utm_n"] == pytest.approx(4400600.0)

    # RAW decodes back to the source heights within quantization
    raw = np.frombuffer(raw_path.read_bytes(), dtype="<u2").reshape(513, 513)
    restored = meta["min_elev_m"] + raw / 65535.0 * (
        meta["max_elev_m"] - meta["min_elev_m"])
    assert np.allclose(restored, heights, atol=0.02)


def test_load_macro_meta_missing_is_actionable(tmp_path):
    with pytest.raises(FileNotFoundError, match="run `build` first"):
        crop.load_macro_meta(tmp_path)
