"""CLI: `fetch` downloads DEM tiles, `build` produces the Unity heightmap,
`landcover` bakes traced land-cover features into splats/trees/fences."""
import argparse
import json
import sys
from pathlib import Path

from terrain_pipeline import config, export, fetch, landcover, process

REPO_ROOT = Path(__file__).resolve().parents[2]
DEM_CACHE = REPO_ROOT / "data" / "dem_cache"
HEIGHTMAP_DIR = REPO_ROOT / "data" / "heightmap"
LANDCOVER_DIR = REPO_ROOT / "data" / "landcover"
LANDCOVER_JSON = LANDCOVER_DIR / "landcover.json"
SPLATMAP_RESOLUTION = 1024


def make_parser():
    parser = argparse.ArgumentParser(prog="terrain-pipeline")
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("fetch", help="query TNM and download DEM GeoTIFFs")
    sub.add_parser("build", help="mosaic cached DEMs into the Unity heightmap")
    sub.add_parser(
        "landcover",
        help="bake data/landcover/landcover.json into splatmap.png, trees.json, fences.json",
    )
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


def run_landcover():
    if not LANDCOVER_JSON.exists():
        sys.exit(f"no {LANDCOVER_JSON}; trace land cover in the tool and export it first")

    data = json.loads(LANDCOVER_JSON.read_text())
    if "features" not in data:
        sys.exit(f"{LANDCOVER_JSON} is missing a 'features' key; not a valid landcover.json")
    features = data["features"]

    bounds = config.utm_square_bounds()
    side = bounds[2] - bounds[0]

    splat = landcover.rasterize_splats(features, size_m=side, resolution=SPLATMAP_RESOLUTION)
    splat_path = landcover.write_splatmap(splat, LANDCOVER_DIR / "splatmap.png")

    trees = landcover.tree_placements(features)
    trees_path = LANDCOVER_DIR / "trees.json"
    trees_path.write_text(json.dumps({"trees": trees}))

    posts = landcover.fence_posts(features)
    fences_path = LANDCOVER_DIR / "fences.json"
    fences_path.write_text(json.dumps({"posts": posts}))

    print(
        f"landcover: {len(features)} features -> "
        f"{splat_path.name} ({SPLATMAP_RESOLUTION}px, {side:.0f}m), "
        f"{len(trees)} trees -> {trees_path.name}, "
        f"{len(posts)} fence posts -> {fences_path.name}"
    )


def main(argv=None):
    args = make_parser().parse_args(argv)
    if args.command == "fetch":
        run_fetch()
    elif args.command == "build":
        run_build()
    else:
        run_landcover()


if __name__ == "__main__":
    main()
