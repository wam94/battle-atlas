"""CLI: `fetch` downloads DEM tiles, `build` produces the Unity heightmap,
`landcover` bakes traced land-cover features into splats/trees/fences,
`relief` bakes the DEM's sky-view/curvature modulation textures,
`crop` cuts a true-scale (1.0x) local tile from the cached 1 m DEM."""
import argparse
import json
import math
import sys
from pathlib import Path

from terrain_pipeline import config, crop, export, fetch, landcover, process, relief

REPO_ROOT = Path(__file__).resolve().parents[2]
DEM_CACHE = REPO_ROOT / "data" / "dem_cache"
HEIGHTMAP_DIR = REPO_ROOT / "data" / "heightmap"
CROP_DIR = REPO_ROOT / "data" / "heightmap_angle"
LANDCOVER_DIR = REPO_ROOT / "data" / "landcover"
LANDCOVER_JSON = LANDCOVER_DIR / "landcover.json"
SPLATMAP_RESOLUTION = 1024
# Relief bakes at the splatmap's resolution ON PURPOSE: the importer
# multiplies layer tints by the relief texture at splat-map resolution, so
# matching 1024 keeps the modulation texel-aligned with the splats (no
# resampling seams), and one texel is ~8.3 m — the scale of the swales the
# bake exists to darken. 2048 would quadruple bake time and texture memory
# for detail below the splat grid the tints can't carry anyway.
RELIEF_RESOLUTION = SPLATMAP_RESOLUTION


def make_parser():
    parser = argparse.ArgumentParser(prog="terrain-pipeline")
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("fetch", help="query TNM and download DEM GeoTIFFs")
    sub.add_parser("build", help="mosaic cached DEMs into the Unity heightmap")
    sub.add_parser(
        "landcover",
        help="bake data/landcover/landcover.json into splatmap.png, trees.json, fences.json",
    )
    sub.add_parser(
        "relief",
        help="bake the heightmap's sky-view/curvature modulation into "
        "relief.png and relief_contours.png",
    )
    crop_p = sub.add_parser(
        "crop",
        help="cut a true-scale local tile (default: the Angle, plan §8.1) "
        "from the cached 1 m DEM into data/heightmap_angle/",
    )
    x0, z0, x1, z1 = crop.DEFAULT_CROP
    crop_p.add_argument("--x0", type=float, default=x0,
                        help="west edge, battlefield-local meters")
    crop_p.add_argument("--z0", type=float, default=z0,
                        help="south edge, battlefield-local meters")
    crop_p.add_argument("--x1", type=float, default=x1,
                        help="east edge, battlefield-local meters")
    crop_p.add_argument("--z1", type=float, default=z1,
                        help="north edge, battlefield-local meters")
    crop_p.add_argument("--dem-cache", type=Path, default=DEM_CACHE,
                        help="directory of cached DEM GeoTIFFs")
    crop_p.add_argument("--out", type=Path, default=CROP_DIR,
                        help="output directory for heightmap.raw/.json")
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


def run_relief():
    if not (HEIGHTMAP_DIR / "heightmap.json").exists():
        sys.exit(f"no heightmap in {HEIGHTMAP_DIR}; run `build` first")

    heights, meta = relief.load_heightmap(HEIGHTMAP_DIR)
    # the bake assumes a square battlefield: one pixel_size_m serves both
    # axes and downsample block-means square blocks — fail loudly on
    # contract drift instead of silently skewing the shading
    assert math.isclose(meta["width_m"], meta["depth_m"], rel_tol=1e-6), (
        f"relief bake assumes a square DEM; heightmap.json says "
        f"{meta['width_m']} x {meta['depth_m']} m"
    )
    down = relief.downsample(heights, RELIEF_RESOLUTION)
    pixel_size_m = meta["width_m"] / RELIEF_RESOLUTION

    mult = relief.bake_relief(down, pixel_size_m)
    contoured = relief.bake_relief_contours(down, pixel_size_m)
    relief_path = relief.write_relief(mult, LANDCOVER_DIR / "relief.png")
    contours_path = relief.write_relief(contoured, LANDCOVER_DIR / "relief_contours.png")

    # Decode constants for the importer (same self-describing-sidecar
    # pattern as heightmap.json).
    meta_out = {
        "resolution": RELIEF_RESOLUTION,
        "row0": "north",
        "clamp": relief.CLAMP,
        "encode_min": relief.ENCODE_MIN,
        "encode_max": relief.ENCODE_MAX,
        "decode": "multiplier = encode_min + byte / 255 * (encode_max - encode_min)",
        "contour_interval_display_m": relief.CONTOUR_INTERVAL_DISPLAY_M,
        "contour_darken": relief.CONTOUR_DARKEN,
        "vertical_exaggeration": relief.DISPLAY_VERTICAL_EXAGGERATION,
    }
    (LANDCOVER_DIR / "relief.json").write_text(json.dumps(meta_out, indent=2))

    print(
        f"relief: {RELIEF_RESOLUTION}px ({pixel_size_m:.2f} m/texel), "
        f"modulation {mult.min() - 1:+.1%}..{mult.max() - 1:+.1%} vs neutral -> "
        f"{relief_path.name}, {contours_path.name}"
    )


def run_crop(args):
    tifs = sorted(Path(args.dem_cache).glob("*.tif"))
    if not tifs:
        sys.exit(f"no GeoTIFFs in {args.dem_cache}; run `fetch` first")
    try:
        macro_meta = crop.load_macro_meta(HEIGHTMAP_DIR)
    except FileNotFoundError as e:
        sys.exit(str(e))

    try:
        heights, spacing = crop.build_crop(
            tifs, macro_meta, args.x0, args.z0, args.x1, args.z1)
    except ValueError as e:
        sys.exit(str(e))
    raw_path, meta_path = crop.export_crop(
        heights, macro_meta, args.x0, args.z0, args.x1, args.z1,
        args.out, spacing)
    print(
        f"crop: local x={args.x0:.0f}..{args.x1:.0f} z={args.z0:.0f}..{args.z1:.0f} "
        f"({args.x1 - args.x0:.0f} m square, {crop.CROP_RESOLUTION}px, "
        f"{spacing:.3f} m/sample, exaggeration 1.0), "
        f"elevation {heights.min():.1f}-{heights.max():.1f} m"
    )
    print(f"wrote {raw_path} and {meta_path}")


def main(argv=None):
    args = make_parser().parse_args(argv)
    if args.command == "fetch":
        run_fetch()
    elif args.command == "build":
        run_build()
    elif args.command == "landcover":
        run_landcover()
    elif args.command == "crop":
        run_crop(args)
    else:
        run_relief()


if __name__ == "__main__":
    main()
