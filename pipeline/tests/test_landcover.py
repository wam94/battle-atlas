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
