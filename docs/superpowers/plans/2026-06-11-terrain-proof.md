# Terrain Proof Implementation Plan (Battle Atlas, Phase 1)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Real Gettysburg LiDAR terrain rendering in Unity on an actual iPhone, with orbit/pan/pinch camera controls — proving the visual foundation of Battle Atlas.

**Architecture:** Two halves per the spec ([docs/superpowers/specs/2026-06-11-battle-atlas-design.md](../specs/2026-06-11-battle-atlas-design.md)): a Python pipeline (`pipeline/`) that fetches USGS 1 m DEM tiles, mosaics/reprojects them onto a square UTM grid, and exports a Unity-ready 16-bit heightmap + metadata JSON; and a Unity project (`app/`) with an editor importer that builds a Terrain asset from those files, plus a touch orbit camera. Pipeline output lands in `data/` (gitignored, regenerable).

**Tech Stack:** Python 3.11 + uv, rasterio/numpy/pyproj/requests, pytest · Unity 6 LTS (URP), Unity Test Framework (EditMode), C# · Xcode 26 for device deploy.

**Scope note:** This is Plan 1 of N. It implements only spec build-order phase 1 ("Terrain proof"). Later phases (simulation core, authoring tool, zoom ladder…) get their own plans.

---

### Task 1: Install Unity (manual prerequisite)

**Files:** none (system setup)

- [ ] **Step 1: Install Unity Hub + Unity 6 LTS with iOS support**

Manual steps (no CLI for this):
1. Download Unity Hub from https://unity.com/download and install it.
2. In Unity Hub → Installs → Install Editor → pick the latest **Unity 6 LTS** (a `6000.x` version marked LTS).
3. In the module picker, check **iOS Build Support**. Install.
4. Sign in with (or create) a free Unity account when prompted; Personal license is fine.

- [ ] **Step 2: Verify the editor binary exists**

Run: `ls /Applications/Unity/Hub/Editor/`
Expected: a directory like `6000.3.5f1`. Note the exact version — several later steps use `$UNITY`:

```bash
# Adjust the version segment to what you saw above; used by later tasks.
export UNITY="/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity"
"$UNITY" -version
```

Expected: prints the version string.

---

### Task 2: Pipeline scaffolding

**Files:**
- Create: `pipeline/pyproject.toml`, `pipeline/conftest.py`, `pipeline/terrain_pipeline/__init__.py`, `pipeline/tests/test_smoke.py`
- Modify: `.gitignore`

- [ ] **Step 1: Create the uv project**

Create `pipeline/pyproject.toml`:

```toml
[project]
name = "terrain-pipeline"
version = "0.1.0"
description = "Battle Atlas terrain pipeline: USGS LiDAR DEM -> Unity heightmap"
requires-python = ">=3.11"
dependencies = [
    "rasterio>=1.3",
    "numpy>=1.26",
    "pyproj>=3.6",
    "requests>=2.31",
]

[dependency-groups]
dev = ["pytest>=8"]
```

Create empty `pipeline/conftest.py` (puts `pipeline/` on sys.path for pytest) and empty `pipeline/terrain_pipeline/__init__.py`.

Create `pipeline/tests/test_smoke.py`:

```python
def test_imports():
    import terrain_pipeline  # noqa: F401
    import rasterio  # noqa: F401
    import pyproj  # noqa: F401
```

- [ ] **Step 2: Install deps and run the smoke test**

Run: `cd pipeline && uv sync && uv run pytest -q`
Expected: `1 passed`. (First `uv sync` downloads rasterio wheels with bundled GDAL — may take a minute.)

- [ ] **Step 3: Extend .gitignore and commit**

Append to `.gitignore`:

```
data/
pipeline/.venv/
__pycache__/
.pytest_cache/
```

```bash
git add .gitignore pipeline/
git commit -m "feat: scaffold terrain pipeline project"
```

---

### Task 3: Battlefield config and UTM square bounds

**Files:**
- Create: `pipeline/terrain_pipeline/config.py`
- Test: `pipeline/tests/test_config.py`

- [ ] **Step 1: Write the failing test**

`pipeline/tests/test_config.py`:

```python
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd pipeline && uv run pytest tests/test_config.py -q`
Expected: FAIL with `cannot import name 'config'` (or AttributeError).

- [ ] **Step 3: Implement config**

`pipeline/terrain_pipeline/config.py`:

```python
"""Battlefield extent and grid constants for the Gettysburg terrain proof."""
from pyproj import Transformer

# west, south, east, north (WGS84). Covers the main field: the town,
# Seminary/Cemetery Ridges, the Round Tops, Culp's Hill.
BBOX_WGS84 = (-77.28, 39.77, -77.195, 39.845)

WGS84_CRS = "EPSG:4326"
UTM_CRS = "EPSG:26918"  # NAD83 / UTM 18N (meters)

HEIGHTMAP_RESOLUTION = 4097  # Unity terrain heightmaps must be 2^n + 1


def utm_square_bounds(bbox: tuple[float, float, float, float] = BBOX_WGS84):
    """UTM bounds of bbox, expanded to a centered square (meters).

    Square because Unity heightmaps are square; we pad the short axis.
    """
    west, south, east, north = bbox
    t = Transformer.from_crs(WGS84_CRS, UTM_CRS, always_xy=True)
    corners = [t.transform(x, y) for x in (west, east) for y in (south, north)]
    xs = [c[0] for c in corners]
    ys = [c[1] for c in corners]
    minx, maxx, miny, maxy = min(xs), max(xs), min(ys), max(ys)
    side = max(maxx - minx, maxy - miny)
    cx, cy = (minx + maxx) / 2, (miny + maxy) / 2
    return (cx - side / 2, cy - side / 2, cx + side / 2, cy + side / 2)
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd pipeline && uv run pytest tests/test_config.py -q`
Expected: `4 passed`

- [ ] **Step 5: Commit**

```bash
git add pipeline/terrain_pipeline/config.py pipeline/tests/test_config.py
git commit -m "feat: battlefield bbox and UTM square-bounds math"
```

---

### Task 4: DEM discovery and download (USGS TNM API)

**Files:**
- Create: `pipeline/terrain_pipeline/fetch.py`
- Test: `pipeline/tests/test_fetch.py`

- [ ] **Step 1: Verify the live API shape before coding against it (don't fake anything)**

Run:

```bash
curl -s "https://tnmaccess.nationalmap.gov/api/v1/products?datasets=Digital%20Elevation%20Model%20(DEM)%201%20meter&bbox=-77.28,39.77,-77.195,39.845&prodFormats=GeoTIFF&max=5" | head -c 2000
```

Expected: JSON with an `items` array whose entries include `title` and `downloadURL` fields pointing at `.tif` files.
**If the response shape differs** (field names, dataset string), adjust the constants/keys in Step 2's code to match reality before proceeding — the test fixtures below must mirror the real response shape you observed.

- [ ] **Step 2: Write the failing tests**

`pipeline/tests/test_fetch.py`:

```python
import json
from pathlib import Path

from terrain_pipeline import fetch


class FakeResponse:
    def __init__(self, payload=None, content=b""):
        self._payload = payload
        self.content = content

    def raise_for_status(self):
        pass

    def json(self):
        return self._payload


def test_query_parses_products():
    captured = {}

    def fake_get(url, params=None, timeout=None):
        captured["url"] = url
        captured["params"] = params
        return FakeResponse(payload={"items": [
            {"title": "USGS 1m x44y441 PA", "downloadURL": "https://example.com/a.tif"},
            {"title": "USGS 1m x45y441 PA", "downloadURL": "https://example.com/b.tif"},
        ]})

    products = fetch.query_dem_products((-77.28, 39.77, -77.195, 39.845), get=fake_get)
    assert captured["url"] == fetch.TNM_API
    assert captured["params"]["bbox"] == "-77.28,39.77,-77.195,39.845"
    assert [p["url"] for p in products] == ["https://example.com/a.tif", "https://example.com/b.tif"]


def test_download_writes_and_skips_existing(tmp_path: Path):
    calls = []

    def fake_get(url, timeout=None):
        calls.append(url)
        return FakeResponse(content=b"tif-bytes")

    products = [{"title": "t", "url": "https://example.com/tile.tif"}]
    paths = fetch.download_products(products, tmp_path, get=fake_get)
    assert paths[0].read_bytes() == b"tif-bytes"

    fetch.download_products(products, tmp_path, get=fake_get)  # second call: cached
    assert len(calls) == 1
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `cd pipeline && uv run pytest tests/test_fetch.py -q`
Expected: FAIL with `No module named 'terrain_pipeline.fetch'`

- [ ] **Step 4: Implement fetch**

`pipeline/terrain_pipeline/fetch.py`:

```python
"""Query and download USGS 3DEP 1 m DEM GeoTIFFs via the TNM Access API."""
from pathlib import Path

import requests

TNM_API = "https://tnmaccess.nationalmap.gov/api/v1/products"
DATASET = "Digital Elevation Model (DEM) 1 meter"


def query_dem_products(bbox_wgs84, get=requests.get):
    west, south, east, north = bbox_wgs84
    params = {
        "datasets": DATASET,
        "bbox": f"{west},{south},{east},{north}",
        "prodFormats": "GeoTIFF",
        "outputFormat": "JSON",
        "max": 100,
    }
    resp = get(TNM_API, params=params, timeout=60)
    resp.raise_for_status()
    items = resp.json()["items"]
    return [{"title": i["title"], "url": i["downloadURL"]} for i in items]


def download_products(products, dest_dir, get=requests.get):
    dest = Path(dest_dir)
    dest.mkdir(parents=True, exist_ok=True)
    paths = []
    for p in products:
        out = dest / p["url"].rsplit("/", 1)[-1]
        if not out.exists():
            resp = get(p["url"], timeout=600)
            resp.raise_for_status()
            out.write_bytes(resp.content)
        paths.append(out)
    return paths
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `cd pipeline && uv run pytest tests/test_fetch.py -q`
Expected: `2 passed`

- [ ] **Step 6: Commit**

```bash
git add pipeline/terrain_pipeline/fetch.py pipeline/tests/test_fetch.py
git commit -m "feat: TNM API DEM discovery and cached download"
```

---

### Task 5: Mosaic, reproject, and grid the DEM

**Files:**
- Create: `pipeline/terrain_pipeline/process.py`
- Test: `pipeline/tests/test_process.py`

- [ ] **Step 1: Write the failing test**

`pipeline/tests/test_process.py`:

```python
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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd pipeline && uv run pytest tests/test_process.py -q`
Expected: FAIL with `No module named 'terrain_pipeline.process'`

- [ ] **Step 3: Implement process**

`pipeline/terrain_pipeline/process.py`:

```python
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd pipeline && uv run pytest tests/test_process.py -q`
Expected: `2 passed`

- [ ] **Step 5: Commit**

```bash
git add pipeline/terrain_pipeline/process.py pipeline/tests/test_process.py
git commit -m "feat: DEM mosaic + reproject onto square UTM grid"
```

---

### Task 6: Export Unity heightmap (RAW + metadata)

**Files:**
- Create: `pipeline/terrain_pipeline/export.py`
- Test: `pipeline/tests/test_export.py`

- [ ] **Step 1: Write the failing test**

`pipeline/tests/test_export.py`:

```python
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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd pipeline && uv run pytest tests/test_export.py -q`
Expected: FAIL with `No module named 'terrain_pipeline.export'`

- [ ] **Step 3: Implement export**

`pipeline/terrain_pipeline/export.py`:

```python
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd pipeline && uv run pytest tests/test_export.py -q`
Expected: `2 passed`

- [ ] **Step 5: Commit**

```bash
git add pipeline/terrain_pipeline/export.py pipeline/tests/test_export.py
git commit -m "feat: Unity heightmap RAW + metadata export"
```

---

### Task 7: CLI and the real pipeline run

**Files:**
- Create: `pipeline/terrain_pipeline/cli.py`
- Test: `pipeline/tests/test_cli.py`

- [ ] **Step 1: Write the failing test**

`pipeline/tests/test_cli.py`:

```python
from terrain_pipeline import cli


def test_parser_has_fetch_and_build():
    p = cli.make_parser()
    assert p.parse_args(["fetch"]).command == "fetch"
    args = p.parse_args(["build"])
    assert args.command == "build"
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd pipeline && uv run pytest tests/test_cli.py -q`
Expected: FAIL with `No module named 'terrain_pipeline.cli'`

- [ ] **Step 3: Implement the CLI**

`pipeline/terrain_pipeline/cli.py`:

```python
"""CLI: `fetch` downloads DEM tiles, `build` produces the Unity heightmap."""
import argparse
import sys
from pathlib import Path

from terrain_pipeline import config, export, fetch, process

REPO_ROOT = Path(__file__).resolve().parents[2]
DEM_CACHE = REPO_ROOT / "data" / "dem_cache"
HEIGHTMAP_DIR = REPO_ROOT / "data" / "heightmap"


def make_parser():
    parser = argparse.ArgumentParser(prog="terrain-pipeline")
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("fetch", help="query TNM and download DEM GeoTIFFs")
    sub.add_parser("build", help="mosaic cached DEMs into the Unity heightmap")
    return parser


def run_fetch():
    products = fetch.query_dem_products(config.BBOX_WGS84)
    print(f"TNM returned {len(products)} products:")
    for p in products:
        print(f"  {p['title']}")
    paths = fetch.download_products(products, DEM_CACHE)
    print(f"cached {len(paths)} GeoTIFFs in {DEM_CACHE}")


def run_build():
    tifs = sorted(DEM_CACHE.glob("*.tif"))
    if not tifs:
        sys.exit(f"no GeoTIFFs in {DEM_CACHE}; run `fetch` first")
    bounds = config.utm_square_bounds()
    heights = process.build_square_dem(
        tifs, bounds, config.UTM_CRS, config.HEIGHTMAP_RESOLUTION
    )
    raw_path, meta_path = export.export_unity_heightmap(
        heights, bounds, HEIGHTMAP_DIR, config.UTM_CRS
    )
    side = bounds[2] - bounds[0]
    print(f"terrain: {side:.0f} m square, elevation {heights.min():.1f}-{heights.max():.1f} m")
    print(f"wrote {raw_path} and {meta_path}")


def main(argv=None):
    args = make_parser().parse_args(argv)
    if args.command == "fetch":
        run_fetch()
    else:
        run_build()


if __name__ == "__main__":
    main()
```

- [ ] **Step 4: Run all pipeline tests**

Run: `cd pipeline && uv run pytest -q`
Expected: all tests pass (smoke + config + fetch + process + export + cli).

- [ ] **Step 5: Run the pipeline for real**

```bash
cd pipeline
uv run python -m terrain_pipeline.cli fetch    # downloads ~4-9 tiles, can take minutes
uv run python -m terrain_pipeline.cli build
```

Expected output: tile titles covering the Gettysburg area, then a build line like `terrain: ~8300 m square, elevation ~120-~270 m` (Gettysburg ground truth: creek bottoms ~120-150 m, Big Round Top ~240 m, so sanity-check the printed range — if elevations are wildly outside 100-300 m, something is wrong; stop and investigate).

- [ ] **Step 6: Verify artifacts**

```bash
ls -la ../data/heightmap/
python3 -c "import json; m=json.load(open('../data/heightmap/heightmap.json')); print(m); assert m['resolution']==4097; assert 50 < m['min_elev_m'] < m['max_elev_m'] < 400"
```

Expected: `heightmap.raw` ≈ 33.6 MB (4097²×2 bytes), JSON prints and assertion passes.

- [ ] **Step 7: Commit**

```bash
git add pipeline/terrain_pipeline/cli.py pipeline/tests/test_cli.py
git commit -m "feat: pipeline CLI; first real Gettysburg heightmap built"
```

---

### Task 8: Unity project setup

**Files:**
- Create: `app/` (Unity project, via Unity Hub), `app/.gitignore`

- [ ] **Step 1: Create the project in Unity Hub (manual)**

1. Unity Hub → New project → template **Universal 3D** (URP) → name `app`, location `/Users/wmitchell/Documents/jetsons/warface` (so the project root is `warface/app`). Create.
2. When the editor opens: **Edit ▸ Project Settings ▸ Player ▸ Other Settings ▸ Active Input Handling → Both** (allows the simple legacy touch API; editor restarts).
3. **Window ▸ Package Manager ▸ Unity Registry**: confirm **Test Framework** is installed (it is by default in Unity 6; install if missing).

- [ ] **Step 2: Add Unity .gitignore**

Create `app/.gitignore`:

```
Library/
Logs/
Temp/
Obj/
Build/
Builds/
UserSettings/
MemoryCaptures/
*.csproj
*.sln
test-results.xml
Assets/Generated/
Assets/Generated.meta
```

(`Assets/Generated/` holds the imported TerrainData asset — ~70 MB and regenerable from `data/`, so it stays out of git.)

- [ ] **Step 3: Commit the project skeleton**

```bash
git add app/
git commit -m "feat: Unity 6 URP project skeleton with iOS-ready settings"
```

Expected: commit includes `app/Assets`, `app/Packages`, `app/ProjectSettings` but no `Library/`.

---

### Task 9: Assembly definitions + heightmap decoder (TDD in Unity)

**Files:**
- Create: `app/Assets/Scripts/BattleAtlas.Runtime.asmdef`
- Create: `app/Assets/Scripts/HeightmapMetadata.cs`
- Create: `app/Assets/Scripts/HeightmapDecoder.cs`
- Create: `app/Assets/Tests/Editor/BattleAtlas.Tests.Editor.asmdef`
- Test: `app/Assets/Tests/Editor/HeightmapDecoderTests.cs`

- [ ] **Step 1: Create assembly definitions**

`app/Assets/Scripts/BattleAtlas.Runtime.asmdef`:

```json
{
    "name": "BattleAtlas.Runtime",
    "rootNamespace": "BattleAtlas",
    "references": []
}
```

`app/Assets/Tests/Editor/BattleAtlas.Tests.Editor.asmdef`:

```json
{
    "name": "BattleAtlas.Tests.Editor",
    "references": [
        "BattleAtlas.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

- [ ] **Step 2: Write the failing test**

`app/Assets/Tests/Editor/HeightmapDecoderTests.cs`:

```csharp
using NUnit.Framework;
using BattleAtlas;

public class HeightmapDecoderTests
{
    [Test]
    public void Decode_FlipsRowsAndScalesTo01()
    {
        // 2x2, little-endian. RAW row 0 (north): [0, 65535]; row 1 (south): [32768, 16384]
        byte[] raw = {
            0x00, 0x00, 0xFF, 0xFF,
            0x00, 0x80, 0x00, 0x40,
        };
        float[,] h = HeightmapDecoder.Decode(raw, 2);
        // Unity heights row 0 = south edge => RAW row 1 lands there
        Assert.AreEqual(32768f / 65535f, h[0, 0], 1e-4f);
        Assert.AreEqual(16384f / 65535f, h[0, 1], 1e-4f);
        Assert.AreEqual(0f, h[1, 0], 1e-4f);
        Assert.AreEqual(1f, h[1, 1], 1e-4f);
    }

    [Test]
    public void Decode_RejectsWrongLength()
    {
        Assert.Throws<System.ArgumentException>(() => HeightmapDecoder.Decode(new byte[3], 2));
    }
}
```

- [ ] **Step 3: Run tests to verify they fail to compile (decoder missing)**

Close the Unity editor first (CLI and editor can't open the project simultaneously), then:

```bash
"$UNITY" -batchmode -projectPath "$(pwd)/app" -runTests -testPlatform EditMode \
  -testResults "$(pwd)/app/test-results.xml" -logFile - 2>&1 | tail -5; echo "exit: $?"
```

Expected: nonzero exit with compile errors mentioning `HeightmapDecoder` not found.

- [ ] **Step 4: Implement metadata + decoder**

`app/Assets/Scripts/HeightmapMetadata.cs`:

```csharp
using System;

namespace BattleAtlas
{
    [Serializable]
    public class HeightmapMetadata
    {
        // field names match the pipeline's heightmap.json keys (JsonUtility maps by name;
        // extra JSON keys like origin_utm_e are ignored)
        public int resolution;
        public float width_m;
        public float depth_m;
        public float min_elev_m;
        public float max_elev_m;
        public string crs;
        public string row0;
    }
}
```

`app/Assets/Scripts/HeightmapDecoder.cs`:

```csharp
namespace BattleAtlas
{
    public static class HeightmapDecoder
    {
        // RAW convention (see pipeline export.py): 16-bit little-endian, row 0 = north.
        // Unity heights[0, *] is the SOUTH edge, so rows flip here.
        public static float[,] Decode(byte[] raw, int resolution)
        {
            if (raw.Length != resolution * resolution * 2)
                throw new System.ArgumentException(
                    $"raw length {raw.Length} != 2 * {resolution}^2");

            var heights = new float[resolution, resolution];
            for (int row = 0; row < resolution; row++)
            {
                int dstY = resolution - 1 - row;
                for (int x = 0; x < resolution; x++)
                {
                    int i = (row * resolution + x) * 2;
                    ushort v = (ushort)(raw[i] | (raw[i + 1] << 8));
                    heights[dstY, x] = v / 65535f;
                }
            }
            return heights;
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
"$UNITY" -batchmode -projectPath "$(pwd)/app" -runTests -testPlatform EditMode \
  -testResults "$(pwd)/app/test-results.xml" -logFile - 2>&1 | tail -5; echo "exit: $?"
grep -o 'failed="[0-9]*"' app/test-results.xml | head -1
```

Expected: `exit: 0` and `failed="0"`.

- [ ] **Step 6: Commit**

```bash
git add app/Assets/Scripts app/Assets/Tests
git commit -m "feat: heightmap decoder with row-flip convention + EditMode tests"
```

---

### Task 10: Editor heightmap importer

**Files:**
- Create: `app/Assets/Editor/BattleAtlas.Editor.asmdef`
- Create: `app/Assets/Editor/HeightmapImporter.cs`

- [ ] **Step 1: Create the editor assembly**

`app/Assets/Editor/BattleAtlas.Editor.asmdef`:

```json
{
    "name": "BattleAtlas.Editor",
    "rootNamespace": "BattleAtlas.EditorTools",
    "references": ["BattleAtlas.Runtime"],
    "includePlatforms": ["Editor"]
}
```

- [ ] **Step 2: Implement the importer**

`app/Assets/Editor/HeightmapImporter.cs`:

```csharp
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    public static class HeightmapImporter
    {
        [MenuItem("BattleAtlas/Import Heightmap")]
        public static void Import()
        {
            // repo layout: <root>/app/Assets and <root>/data/heightmap
            string dir = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../../data/heightmap"));
            byte[] raw = File.ReadAllBytes(Path.Combine(dir, "heightmap.raw"));
            var meta = JsonUtility.FromJson<HeightmapMetadata>(
                File.ReadAllText(Path.Combine(dir, "heightmap.json")));

            var terrainData = new TerrainData();
            // resolution must be set BEFORE size (setting it resets terrain size)
            terrainData.heightmapResolution = meta.resolution;
            terrainData.size = new Vector3(
                meta.width_m, meta.max_elev_m - meta.min_elev_m, meta.depth_m);
            terrainData.SetHeights(0, 0, HeightmapDecoder.Decode(raw, meta.resolution));

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));
            AssetDatabase.CreateAsset(terrainData, "Assets/Generated/GettysburgTerrain.asset");

            var go = Terrain.CreateTerrainGameObject(terrainData);
            go.name = "Gettysburg Terrain";
            AssetDatabase.SaveAssets();
            Debug.Log($"Imported terrain {meta.width_m:F0}m x {meta.depth_m:F0}m, " +
                      $"elevation range {meta.max_elev_m - meta.min_elev_m:F0}m. " +
                      $"Camera pivot hint: ({meta.width_m / 2f:F0}, 0, {meta.depth_m / 2f:F0})");
        }
    }
}
```

- [ ] **Step 3: Run the import (manual, in editor)**

Open the project in Unity. Open `Assets/Scenes/SampleScene`. Menu **BattleAtlas ▸ Import Heightmap**.

Expected: a "Gettysburg Terrain" object appears; console logs dimensions and the pivot hint. In the Scene view you should *recognize the battlefield* — the Round Tops in the south, Cemetery and Seminary Ridges running north, Culp's Hill in the east. (Default gray material is fine; directional light reveals the relief.)

- [ ] **Step 4: Verify EditMode tests still pass (close editor first)**

```bash
"$UNITY" -batchmode -projectPath "$(pwd)/app" -runTests -testPlatform EditMode \
  -testResults "$(pwd)/app/test-results.xml" -logFile - 2>&1 | tail -3; echo "exit: $?"
```

Expected: `exit: 0`.

- [ ] **Step 5: Commit**

```bash
git add app/Assets/Editor
git commit -m "feat: editor importer builds Gettysburg TerrainData from pipeline output"
```

---

### Task 11: Orbit camera (TDD on the math, then the controller)

**Files:**
- Create: `app/Assets/Scripts/OrbitMath.cs`
- Create: `app/Assets/Scripts/OrbitCameraController.cs`
- Test: `app/Assets/Tests/Editor/OrbitMathTests.cs`

- [ ] **Step 1: Write the failing tests**

`app/Assets/Tests/Editor/OrbitMathTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class OrbitMathTests
{
    [Test]
    public void ClampPitch_StaysAboveHorizonAndBelowVertical()
    {
        Assert.AreEqual(OrbitMath.MinPitchDeg, OrbitMath.ClampPitch(-30f));
        Assert.AreEqual(OrbitMath.MaxPitchDeg, OrbitMath.ClampPitch(120f));
        Assert.AreEqual(45f, OrbitMath.ClampPitch(45f));
    }

    [Test]
    public void ClampDistance_StaysInRange()
    {
        Assert.AreEqual(OrbitMath.MinDistance, OrbitMath.ClampDistance(0f));
        Assert.AreEqual(OrbitMath.MaxDistance, OrbitMath.ClampDistance(1e9f));
    }

    [Test]
    public void CameraPosition_PitchNinetyIsStraightUp()
    {
        Vector3 p = OrbitMath.CameraPosition(Vector3.zero, 0f, 90f, 100f);
        Assert.AreEqual(0f, p.x, 1e-3f);
        Assert.AreEqual(100f, p.y, 1e-3f);
        Assert.AreEqual(0f, p.z, 1e-3f);
    }

    [Test]
    public void CameraPosition_PitchZeroSitsBehindPivotOnHorizon()
    {
        Vector3 p = OrbitMath.CameraPosition(new Vector3(10f, 0f, 10f), 0f, 0f, 100f);
        Assert.AreEqual(10f, p.x, 1e-3f);
        Assert.AreEqual(0f, p.y, 1e-3f);
        Assert.AreEqual(-90f, p.z, 1e-3f);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail to compile**

```bash
"$UNITY" -batchmode -projectPath "$(pwd)/app" -runTests -testPlatform EditMode \
  -testResults "$(pwd)/app/test-results.xml" -logFile - 2>&1 | tail -5; echo "exit: $?"
```

Expected: nonzero exit, compile error on `OrbitMath`.

- [ ] **Step 3: Implement the math and the controller**

`app/Assets/Scripts/OrbitMath.cs`:

```csharp
using UnityEngine;

namespace BattleAtlas
{
    public static class OrbitMath
    {
        public const float MinPitchDeg = 10f;
        public const float MaxPitchDeg = 85f;
        public const float MinDistance = 50f;
        public const float MaxDistance = 12000f;

        public static float ClampPitch(float deg) =>
            Mathf.Clamp(deg, MinPitchDeg, MaxPitchDeg);

        public static float ClampDistance(float d) =>
            Mathf.Clamp(d, MinDistance, MaxDistance);

        public static Vector3 CameraPosition(
            Vector3 pivot, float yawDeg, float pitchDeg, float distance)
        {
            var rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);
            return pivot + rot * (Vector3.back * distance);
        }
    }
}
```

`app/Assets/Scripts/OrbitCameraController.cs`:

```csharp
using UnityEngine;

namespace BattleAtlas
{
    public class OrbitCameraController : MonoBehaviour
    {
        public Vector3 pivot;
        public float yawDeg;
        public float pitchDeg = 45f;
        public float distance = 4000f;
        public float orbitSpeed = 0.2f;
        public float panSpeed = 1.0f;

        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        void LateUpdate()
        {
            if (Input.touchCount == 1)
            {
                Vector2 d = Input.GetTouch(0).deltaPosition;
                yawDeg += d.x * orbitSpeed;
                pitchDeg = OrbitMath.ClampPitch(pitchDeg - d.y * orbitSpeed);
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                float prev = ((t0.position - t0.deltaPosition) -
                              (t1.position - t1.deltaPosition)).magnitude;
                float curr = (t0.position - t1.position).magnitude;
                distance = OrbitMath.ClampDistance(
                    distance * (prev / Mathf.Max(curr, 1f)));

                Vector2 avg = (t0.deltaPosition + t1.deltaPosition) * 0.5f;
                Quaternion yawRot = Quaternion.Euler(0f, yawDeg, 0f);
                pivot -= (yawRot * Vector3.right * avg.x + yawRot * Vector3.forward * avg.y)
                         * panSpeed * distance * 0.001f;
            }

#if UNITY_EDITOR
            // mouse fallback so the editor Play button is usable
            if (Input.GetMouseButton(0))
            {
                yawDeg += Input.GetAxis("Mouse X") * 3f;
                pitchDeg = OrbitMath.ClampPitch(pitchDeg - Input.GetAxis("Mouse Y") * 3f);
            }
            distance = OrbitMath.ClampDistance(
                distance * (1f - Input.mouseScrollDelta.y * 0.05f));
#endif

            transform.position = OrbitMath.CameraPosition(pivot, yawDeg, pitchDeg, distance);
            transform.LookAt(pivot);
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
"$UNITY" -batchmode -projectPath "$(pwd)/app" -runTests -testPlatform EditMode \
  -testResults "$(pwd)/app/test-results.xml" -logFile - 2>&1 | tail -3; echo "exit: $?"
grep -o 'failed="[0-9]*"' app/test-results.xml | head -1
```

Expected: `exit: 0`, `failed="0"` (decoder + orbit tests all green).

- [ ] **Step 5: Wire the scene and fly (manual, in editor)**

1. Open SampleScene. Select **Main Camera**, Add Component → **Orbit Camera Controller**.
2. Set `pivot` to the value the importer logged (≈ half of width/depth, e.g. `(4150, 0, 4150)`); leave pitch 45, distance 4000.
3. Press Play. Drag to orbit, scroll to zoom.

Expected: smooth flight over recognizable Gettysburg terrain in the Game view. Save the scene.

- [ ] **Step 6: Commit**

```bash
git add app/Assets/Scripts app/Assets/Tests app/Assets/Scenes
git commit -m "feat: orbit/pan/pinch camera over Gettysburg terrain"
```

---

### Task 12: iOS build and on-device verification

**Files:** none new (build settings + device run)

- [ ] **Step 1: Configure player settings (manual)**

In Unity: **Edit ▸ Project Settings ▸ Player ▸ iOS tab**: set Bundle Identifier (e.g. `com.willmitchell.battleatlas`), Target minimum iOS 15+, default everything else.

- [ ] **Step 2: Build for iOS**

**File ▸ Build Profiles** (or Build Settings) → iOS → Switch Platform → **Build** to `app/Builds/ios/`.
Expected: Xcode project generated at `app/Builds/ios/Unity-iPhone.xcodeproj`.

- [ ] **Step 3: Deploy to iPhone**

1. `open app/Builds/ios/Unity-iPhone.xcodeproj`
2. In Xcode: select the Unity-iPhone target → Signing & Capabilities → check *Automatically manage signing*, pick your Apple ID team.
3. Connect the iPhone, select it as run destination, ⌘R.
4. On first run, trust the developer profile on the phone (Settings ▸ General ▸ VPN & Device Management).

Expected: the app launches showing the battlefield.

- [ ] **Step 4: On-device verification checklist**

- [ ] One-finger drag orbits; two-finger pinch zooms; two-finger drag pans
- [ ] The Round Tops, Devil's Den, Cemetery Ridge, and Culp's Hill are recognizable in relief
- [ ] Frame rate feels fluid while orbiting at multiple zoom levels (Xcode's FPS gauge ≥ 30, target 60)
- [ ] No thermal warning / runaway battery drain over 5 minutes of use

Record the observed FPS in the commit message.

- [ ] **Step 5: Commit + tag the milestone**

```bash
git add app/ProjectSettings
git commit -m "feat: iOS build settings; terrain proof verified on device (<observed fps> fps)"
git tag terrain-proof
```

---

## Done = 

The phase 1 exit criterion from the spec: **flying around Little Round Top on a real iPhone over real LiDAR terrain.** Everything compiles from a fresh clone with: `uv sync` + `fetch` + `build` (pipeline), Unity import menu, Xcode run. Next plan: Phase 2 — Simulation Core + timeline scrubber with placeholder units.
