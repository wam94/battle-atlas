from pyproj import Transformer

from terrain_pipeline import config


def test_bounds_are_square():
    minx, miny, maxx, maxy = config.utm_square_bounds()
    assert abs((maxx - minx) - (maxy - miny)) < 1e-6


def test_bounds_have_battlefield_scale():
    minx, miny, maxx, maxy = config.utm_square_bounds()
    side = maxx - minx
    assert 7_000 < side < 12_000  # battlefield is ~8 km across


def test_bounds_contain_little_round_top():
    # Little Round Top summit, WGS84
    lon, lat = -77.236, 39.791
    t = Transformer.from_crs(config.WGS84_CRS, config.UTM_CRS, always_xy=True)
    e, n = t.transform(lon, lat)
    minx, miny, maxx, maxy = config.utm_square_bounds()
    assert minx < e < maxx and miny < n < maxy


def test_resolution_is_power_of_two_plus_one():
    assert (config.HEIGHTMAP_RESOLUTION - 1) & (config.HEIGHTMAP_RESOLUTION - 2) == 0
