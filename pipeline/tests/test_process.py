import numpy as np
import rasterio
from rasterio.transform import from_bounds

from terrain_pipeline import process


def write_tif(path, arr, bounds, crs="EPSG:26918", nodata=-9999.0):
    transform = from_bounds(*bounds, arr.shape[1], arr.shape[0])
    with rasterio.open(
        path, "w", driver="GTiff",
        height=arr.shape[0], width=arr.shape[1], count=1,
        dtype=arr.dtype, crs=crs, transform=transform, nodata=nodata,
    ) as ds:
        ds.write(arr, 1)


def test_mosaics_two_tiles_onto_square_grid(tmp_path):
    # west tile constant 100 m, east tile constant 200 m
    west = np.full((10, 10), 100.0, dtype=np.float32)
    east = np.full((10, 10), 200.0, dtype=np.float32)
    write_tif(tmp_path / "w.tif", west, (0, 0, 100, 100))
    write_tif(tmp_path / "e.tif", east, (100, 0, 200, 100))

    heights = process.build_square_dem(
        [tmp_path / "w.tif", tmp_path / "e.tif"],
        square_bounds=(0, 0, 200, 200),  # taller than data: forces nodata fill
        dst_crs="EPSG:26918",
        resolution=8,
    )
    assert heights.shape == (8, 8)
    assert np.isfinite(heights).all()  # nodata holes were filled
    # bottom half of the grid covers the tiles: west cols ~100, east cols ~200
    assert abs(heights[-1, 0] - 100.0) < 1.0
    assert abs(heights[-1, -1] - 200.0) < 1.0
    # fill value is the minimum valid elevation
    assert abs(heights[0, 0] - 100.0) < 1.0


def test_rejects_all_nodata():
    import pytest

    with pytest.raises(ValueError):
        process._fill_nodata(np.full((4, 4), np.nan, dtype=np.float32))
