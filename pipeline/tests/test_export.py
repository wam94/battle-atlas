import json

import numpy as np

from terrain_pipeline import export


def test_roundtrip_within_quantization(tmp_path):
    heights = np.array([[100.0, 150.0], [200.0, 125.0]], dtype=np.float32)
    raw_path, meta_path = export.export_unity_heightmap(
        heights, square_bounds=(0, 0, 2000, 2000), out_dir=tmp_path, crs="EPSG:26918"
    )

    meta = json.loads(meta_path.read_text())
    assert meta["resolution"] == 2
    assert meta["width_m"] == 2000
    assert meta["min_elev_m"] == 100.0
    assert meta["max_elev_m"] == 200.0
    assert meta["row0"] == "north"

    raw = np.frombuffer(raw_path.read_bytes(), dtype="<u2").reshape(2, 2)
    restored = meta["min_elev_m"] + raw / 65535.0 * (meta["max_elev_m"] - meta["min_elev_m"])
    assert np.allclose(restored, heights, atol=0.01)  # 100 m range / 65535 steps


def test_flat_terrain_does_not_divide_by_zero(tmp_path):
    heights = np.full((2, 2), 42.0, dtype=np.float32)
    raw_path, _ = export.export_unity_heightmap(
        heights, square_bounds=(0, 0, 100, 100), out_dir=tmp_path, crs="EPSG:26918"
    )
    raw = np.frombuffer(raw_path.read_bytes(), dtype="<u2")
    assert (raw == 0).all()
