"""Write a Unity-ready heightmap: 16-bit little-endian RAW + metadata JSON.

Convention shared with the Unity importer (HeightmapDecoder.cs):
- values normalize min..max elevation to 0..65535
- row 0 of the RAW is the NORTH edge (the importer flips rows because
  Unity's heightmap row 0 is the south edge)
"""
import json
from pathlib import Path

import numpy as np


def export_unity_heightmap(heights, square_bounds, out_dir, crs):
    out = Path(out_dir)
    out.mkdir(parents=True, exist_ok=True)

    min_e = float(heights.min())
    max_e = float(heights.max())
    rng = max(max_e - min_e, 1e-6)
    norm = ((heights - min_e) / rng * 65535.0).round().astype("<u2")

    raw_path = out / "heightmap.raw"
    raw_path.write_bytes(norm.tobytes())

    minx, miny, maxx, maxy = square_bounds
    meta = {
        "resolution": int(heights.shape[0]),
        "width_m": maxx - minx,
        "depth_m": maxy - miny,
        "min_elev_m": min_e,
        "max_elev_m": max_e,
        "origin_utm_e": minx,
        "origin_utm_n": miny,
        "crs": crs,
        "row0": "north",
    }
    meta_path = out / "heightmap.json"
    meta_path.write_text(json.dumps(meta, indent=2))
    return raw_path, meta_path
