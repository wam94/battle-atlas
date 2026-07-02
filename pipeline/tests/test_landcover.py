import numpy as np
from PIL import Image

from terrain_pipeline import landcover


def _feature(id_, kind, cls_, points):
    return {"id": id_, "kind": kind, "cls": cls_, "points": points}


def test_field_polygon_west_half_fills_red_channel():
    # 100m square at resolution 4 -> each cell is 25m. West half = cols 0-1.
    features = [
        _feature("f-1", "polygon", "field", [[0, 0], [50, 0], [50, 100], [0, 100]]),
    ]
    arr = landcover.rasterize_splats(features, size_m=100, resolution=4)

    assert arr.shape == (4, 4, 4)
    assert arr.dtype == np.uint8
    assert (arr[:, 0:2, 0] == 255).all()  # west columns: R (field) filled
    assert (arr[:, 2:4, 0] == 0).all()    # east columns: R empty
    # no other channels touched
    assert (arr[:, :, 1] == 0).all()
    assert (arr[:, :, 2] == 0).all()
    assert (arr[:, :, 3] == 0).all()


def test_north_polygon_lands_in_first_rows():
    # Orientation pin: row 0 = north (max z), matching heightmap.raw contract.
    # Polygon covers the NORTH half (z in [50, 100]) of a 100m square.
    features = [
        _feature("f-2", "polygon", "field", [[0, 50], [100, 50], [100, 100], [0, 100]]),
    ]
    arr = landcover.rasterize_splats(features, size_m=100, resolution=4)

    assert (arr[0:2, :, 0] == 255).all()  # first (north) rows filled
    assert (arr[2:4, :, 0] == 0).all()    # last (south) rows empty


def test_marsh_polygon_fills_alpha_channel():
    features = [
        _feature("f-3", "polygon", "marsh", [[0, 0], [100, 0], [100, 100], [0, 100]]),
    ]
    arr = landcover.rasterize_splats(features, size_m=100, resolution=4)

    assert (arr[:, :, 3] == 255).all()
    assert (arr[:, :, 0] == 0).all()
    assert (arr[:, :, 1] == 0).all()
    assert (arr[:, :, 2] == 0).all()


def test_later_feature_wins_on_cross_class_overlap():
    # Load-bearing "later wins" test: an earlier field polygon (whole square)
    # is partially overlapped by a LATER orchard polygon (west half). At
    # shared pixels the later feature must win: west half reads as orchard
    # only (B=255, R=0); east half (untouched by the later polygon) keeps
    # the earlier field paint (R=255).
    features = [
        _feature("f-4", "polygon", "field", [[0, 0], [100, 0], [100, 100], [0, 100]]),
        _feature("f-5", "polygon", "orchard", [[0, 0], [50, 0], [50, 100], [0, 100]]),
    ]
    arr = landcover.rasterize_splats(features, size_m=100, resolution=4)

    assert (arr[:, 0:2, 2] == 255).all()  # west: orchard (later) wins -> B
    assert (arr[:, 0:2, 0] == 0).all()    # west: field overwritten -> R cleared
    assert (arr[:, 2:4, 0] == 255).all()  # east: untouched, field remains -> R


def test_later_pasture_clears_earlier_field():
    # Pasture is the implied base (no dedicated channel). A later pasture
    # polygon overlapping an earlier field polygon must still win per the
    # "later-listed features overwrite earlier" rule, clearing R in the
    # overlap even though pasture itself paints no channel.
    features = [
        _feature("f-6", "polygon", "field", [[0, 0], [100, 0], [100, 100], [0, 100]]),
        _feature("f-7", "polygon", "pasture", [[0, 0], [50, 0], [50, 100], [0, 100]]),
    ]
    arr = landcover.rasterize_splats(features, size_m=100, resolution=4)

    assert (arr[:, 0:2, 0] == 0).all()    # west: pasture (later) wins, field cleared
    assert (arr[:, 2:4, 0] == 255).all()  # east: untouched, field remains


def test_line_feature_contributes_nothing():
    features = [
        _feature("fl-1", "line", "rail_fence", [[0, 0], [100, 100]]),
    ]
    arr = landcover.rasterize_splats(features, size_m=100, resolution=4)

    assert (arr == 0).all()


def test_write_splatmap_roundtrips_via_pillow(tmp_path):
    arr = np.zeros((4, 4, 4), dtype=np.uint8)
    arr[:, 0:2, 0] = 255
    arr[:, :, 3] = 128

    out_path = tmp_path / "splatmap.png"
    landcover.write_splatmap(arr, out_path)

    assert out_path.exists()
    img = Image.open(out_path)
    assert img.mode == "RGBA"
    assert img.size == (4, 4)

    restored = np.array(img)
    assert restored.shape == (4, 4, 4)
    assert np.array_equal(restored, arr)


# --- tree_placements -------------------------------------------------------


def _square(id_, cls_, x0=0, z0=0, side=100):
    return _feature(id_, "polygon", cls_, [
        [x0, z0], [x0 + side, z0], [x0 + side, z0 + side], [x0, z0 + side],
    ])


def test_woodlot_trees_all_inside_polygon():
    features = [_square("w-1", "woodlot")]
    trees = landcover.tree_placements(features)

    assert len(trees) > 0
    for t in trees:
        assert t["cls"] == "woodlot"
        assert 0 <= t["x"] <= 100
        assert 0 <= t["z"] <= 100


def test_woodlot_count_scales_with_area_100m_square():
    # 1 tree / (9m)^2 over a 100m square -> ~100*100/81 ~= 123 trees,
    # loosened to the task's 110-135 band to allow for jitter clipping at
    # the polygon boundary.
    features = [_square("w-2", "woodlot")]
    trees = landcover.tree_placements(features)

    assert 110 <= len(trees) <= 135


def test_woodlot_placements_deterministic_across_calls():
    features = [_square("w-3", "woodlot")]
    a = landcover.tree_placements(features)
    b = landcover.tree_placements(features)

    assert a == b


def test_orchard_rows_at_8m_pitch_inside_polygon():
    features = [_square("o-1", "orchard")]
    trees = landcover.tree_placements(features)

    assert len(trees) > 0
    for t in trees:
        assert t["cls"] == "orchard"
        assert 0 <= t["x"] <= 100
        assert 0 <= t["z"] <= 100


def test_orchard_jitter_much_tighter_than_woodlot():
    # Orchard rows are "visibly regular" (+-0.8m jitter) vs woodlot's
    # loose scatter (+-3.5m jitter): mean absolute offset from the
    # underlying grid point should be much smaller for orchard.
    woodlot = landcover.tree_placements([_square("w-4", "woodlot")])
    orchard = landcover.tree_placements([_square("o-2", "orchard")])

    def mean_abs_offset_from_grid(trees, pitch):
        offsets = []
        for t in trees:
            gx = round(t["x"] / pitch) * pitch
            gz = round(t["z"] / pitch) * pitch
            offsets.append(abs(t["x"] - gx) + abs(t["z"] - gz))
        return sum(offsets) / len(offsets)

    woodlot_var = mean_abs_offset_from_grid(woodlot, 9.0)
    orchard_var = mean_abs_offset_from_grid(orchard, 8.0)

    assert orchard_var < woodlot_var * 0.5


def test_tree_placements_ignores_line_and_non_tree_polygon_classes():
    features = [
        _feature("l-1", "line", "rail_fence", [[0, 0], [100, 0]]),
        _square("f-1", "field"),
    ]
    trees = landcover.tree_placements(features)

    assert trees == []


# --- fence_posts -------------------------------------------------------


def test_straight_30m_east_west_line_gives_11_posts_bearing_90():
    # West -> east line, resampled at 3m spacing including both endpoints:
    # 30m / 3m = 10 segments -> 11 posts. A west->east segment points east,
    # bearing 90 (0=north/+z, 90=east/+x per battle-format.md facing).
    features = [
        _feature("fl-1", "line", "rail_fence", [[0, 0], [30, 0]]),
    ]
    posts = landcover.fence_posts(features, spacing=3.0)

    assert len(posts) == 11
    assert posts[0]["x"] == 0 and posts[0]["z"] == 0
    assert posts[-1]["x"] == 30 and posts[-1]["z"] == 0
    for p in posts:
        assert p["bearing_deg"] == 90
        assert p["cls"] == "rail_fence"


def test_fence_posts_ignores_polygon_features():
    features = [_square("f-2", "field")]
    posts = landcover.fence_posts(features)

    assert posts == []


def test_fence_posts_no_duplicate_at_bend():
    feature = {
        "id": "f1", "kind": "line", "cls": "rail_fence",
        "points": [[0, 0], [30, 0], [30, 30]],
    }
    posts = landcover.fence_posts([feature])
    positions = [(round(p["x"], 6), round(p["z"], 6)) for p in posts]
    assert len(positions) == len(set(positions)), "duplicate post positions"
    assert (30.0, 0.0) in positions  # the corner post exists exactly once
