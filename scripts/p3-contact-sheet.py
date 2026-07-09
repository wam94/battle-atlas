#!/usr/bin/env python3
"""Compose the Phase 3 bake-off frames into one side-by-side contact sheet.

Usage (Pillow comes from the pipeline venv):
    cd pipeline && uv run python ../scripts/p3-contact-sheet.py

Reads docs/benchmarks/captures/p3-{camera}-{pipeline}.png (generate them
first with the AngleBakeoffRender.RenderUrp / RenderHdrp batchmode calls;
see docs/benchmarks/2026-07-08-v2-phase3-bakeoff.md) and writes
docs/benchmarks/captures/p3-contact-sheet.png. Output is gitignored
generated media, regenerable with one command, like every capture.
"""
from pathlib import Path

from PIL import Image, ImageDraw

REPO = Path(__file__).resolve().parents[1]
CAPTURES = REPO / "docs" / "benchmarks" / "captures"
CAMERAS = ["theater", "tactical", "eye"]
PIPELINES = ["urp", "hdrp"]
TILE_W = 1280  # 2560 halved
PAD = 8
LABEL_H = 28


def main():
    tile_h = None
    tiles = {}
    for cam in CAMERAS:
        for pipe in PIPELINES:
            path = CAPTURES / f"p3-{cam}-{pipe}.png"
            if not path.exists():
                raise SystemExit(f"missing {path}; render both pipelines first")
            img = Image.open(path)
            scale = TILE_W / img.width
            tile_h = int(img.height * scale)
            tiles[(cam, pipe)] = img.resize((TILE_W, tile_h), Image.LANCZOS)

    row_h = LABEL_H + tile_h + PAD
    sheet = Image.new(
        "RGB", (PAD + 2 * (TILE_W + PAD), PAD + 3 * row_h), (18, 18, 18))
    draw = ImageDraw.Draw(sheet)
    for r, cam in enumerate(CAMERAS):
        for c, pipe in enumerate(PIPELINES):
            x = PAD + c * (TILE_W + PAD)
            y = PAD + r * row_h
            draw.text((x + 4, y + 6), f"{cam.upper()} — {pipe.upper()}",
                      fill=(235, 235, 235))
            sheet.paste(tiles[(cam, pipe)], (x, y + LABEL_H))

    out = CAPTURES / "p3-contact-sheet.png"
    sheet.save(out)
    print(f"wrote {out} ({sheet.width}x{sheet.height})")


if __name__ == "__main__":
    main()
