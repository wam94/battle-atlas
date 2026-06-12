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
