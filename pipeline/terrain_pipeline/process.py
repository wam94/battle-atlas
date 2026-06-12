"""Mosaic source DEM tiles and reproject onto a square UTM grid."""
import numpy as np
import rasterio
from rasterio.merge import merge as rio_merge
from rasterio.transform import from_bounds
from rasterio.warp import Resampling, reproject


def build_square_dem(tif_paths, square_bounds, dst_crs, resolution):
    """Return heights (float32, shape [resolution, resolution], row 0 = north)."""
    sources = [rasterio.open(p) for p in tif_paths]
    try:
        mosaic, src_transform = rio_merge(sources)
        src_crs = sources[0].crs
        src_nodata = sources[0].nodata
    finally:
        for s in sources:
            s.close()

    minx, miny, maxx, maxy = square_bounds
    dst_transform = from_bounds(minx, miny, maxx, maxy, resolution, resolution)
    dst = np.full((resolution, resolution), np.nan, dtype=np.float32)
    reproject(
        source=mosaic[0],
        destination=dst,
        src_transform=src_transform,
        src_crs=src_crs,
        src_nodata=src_nodata,
        dst_transform=dst_transform,
        dst_crs=dst_crs,
        dst_nodata=np.nan,
        resampling=Resampling.bilinear,
    )
    return _fill_nodata(dst)


def _fill_nodata(heights):
    """Replace NaN cells with the minimum valid elevation (flat fill at the edges)."""
    valid = np.isfinite(heights)
    if not valid.any():
        raise ValueError("DEM grid contains no valid elevation data")
    heights[~valid] = heights[valid].min()
    return heights
