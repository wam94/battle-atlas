#!/usr/bin/env python3
"""Crop a georeferenced Bachelder sheet around a battlefield-local point.

The standing friction item from dossier passes 2-4 (each pass improvised
the same scratchpad crop script) — committed at pass 5.

Usage:
    uv run --with pillow python reconstruction/scripts/crop_sheet.py \
        <sheet-id> <local_x> <local_z> [--size 800] [--scale 1.0] \
        [--maps data/maps] [--out out.png]

Converts the local point to sheet pixels via the manifest's per-sheet
similarity (local = [a -b; b a] * (img_x, -img_y) + (tx, ty), the
tool/src/overlay.ts convention), crops a size×size-meter window, and
writes a PNG. Requires the jp2 master fetched by
reconstruction/scripts/fetch_bachelder_maps.sh (system Pillow may lack
the jpeg2k decoder — use `uv run --with pillow`, the pass-4 note).

Reading discipline: docs/reconstruction/audit/spatial-evidence.md —
label/bar positions read off crops carry >= the sheet's
estAbsUncertaintyM (~31 m rms, 62 m floor guidance for sole anchors);
print the inverse transform so a pixel picked in the crop can be
converted back to local meters.
"""

import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "reconstruction/spatial/bachelder-manifest.json"


def load_sheet(sheet_id: str) -> dict:
    m = json.loads(MANIFEST.read_text())
    for s in m["sheets"]:
        if s["id"] == sheet_id:
            return s
    raise SystemExit(f"sheet {sheet_id!r} not in manifest")


def local_to_img(g: dict, lx: float, lz: float) -> tuple[float, float]:
    a, b, tx, ty = g["a"], g["b"], g["tx"], g["ty"]
    det = a * a + b * b
    dx, dz = lx - tx, lz - ty
    ix = (a * dx + b * dz) / det
    neg_iy = (-b * dx + a * dz) / det
    return ix, -neg_iy


def img_to_local(g: dict, ix: float, iy: float) -> tuple[float, float]:
    a, b, tx, ty = g["a"], g["b"], g["tx"], g["ty"]
    return a * ix - b * (-iy) + tx, b * ix + a * (-iy) + ty


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("sheet")
    ap.add_argument("local_x", type=float)
    ap.add_argument("local_z", type=float)
    ap.add_argument("--size", type=float, default=800.0, help="window size in meters")
    ap.add_argument("--scale", type=float, default=1.0, help="output pixels per source pixel")
    ap.add_argument("--maps", default=str(ROOT / "data/maps"))
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    from PIL import Image

    Image.MAX_IMAGE_PIXELS = None
    s = load_sheet(args.sheet)
    g = s["georef"]
    path = Path(args.maps) / "bachelder" / s["file"]
    if not path.exists():
        raise SystemExit(f"master not fetched: {path} (run fetch_bachelder_maps.sh)")

    cx, cy = local_to_img(g, args.local_x, args.local_z)
    half_px = args.size / 2 / g["metersPerPixel"]
    box = (int(cx - half_px), int(cy - half_px), int(cx + half_px), int(cy + half_px))
    im = Image.open(path)
    box = (max(box[0], 0), max(box[1], 0), min(box[2], im.width), min(box[3], im.height))
    crop = im.crop(box)
    if args.scale != 1.0:
        crop = crop.resize((int(crop.width * args.scale), int(crop.height * args.scale)))
    out = args.out or f"{args.sheet}_{int(args.local_x)}_{int(args.local_z)}.png"
    crop.convert("RGB").save(out)
    print(f"wrote {out}  crop box px={box}  center px=({cx:.0f},{cy:.0f})")
    print(
        f"to local: local = img_to_local; crop pixel (u,v) -> source px "
        f"({box[0]}+u/{args.scale:.2f}, {box[1]}+v/{args.scale:.2f}); "
        f"estAbsUncertaintyM={g['estAbsUncertaintyM']}"
    )


if __name__ == "__main__":
    main()
