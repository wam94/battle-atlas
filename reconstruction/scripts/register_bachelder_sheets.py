#!/usr/bin/env python3
# RESEARCH TOOLING: run from data/maps/bachelder/ after fetch_bachelder_maps.sh.
# Needs numpy+Pillow+opj_decompress (not part of the test suite deps).
"""Register every main-field Bachelder sheet to the reference sheet
(12440022, July 3 1-5 PM) by patch phase-correlation on the shared
Warren 1873 printed base, then compose with the reference sheet's
eyeballed tie-point similarity to get per-sheet full-res-pixel ->
battlefield-local-meter transforms.

Run from data/maps/bachelder/. Writes georef_working.json.
"""
import json, math, os, subprocess, tempfile
import numpy as np
from PIL import Image

Image.MAX_IMAGE_PIXELS = None
REDUCE = 2          # decode at 1/4 resolution
SCALE = 2 ** REDUCE
REF = "12440022"
# patch centers in REF full-res pixels: terrain-detail areas spread over
# the sheet (road junction sectors used as tie points + one NE filler)
PATCH_CENTERS = [(4022, 7642), (5678, 4138), (888, 5750), (3533, 1370),
                 (7600, 3000), (5762, 8424)]
PATCH = 384         # patch size at 1/4 res (=1536 full-res px ~ 1.4 km)

def decode(jp2, reduce=REDUCE):
    with tempfile.NamedTemporaryFile(suffix=".tif", delete=False) as f:
        tif = f.name
    subprocess.run(["opj_decompress", "-i", jp2, "-o", tif, "-r", str(reduce),
                    "-quiet"], check=True, capture_output=True)
    im = Image.open(tif).convert("L")
    a = np.asarray(im, dtype=np.float32)
    os.unlink(tif)
    return a

def hann2d(n):
    w = np.hanning(n)
    return np.outer(w, w)

def phase_corr(a, b):
    """displacement (dx, dy) that moves b to align with a; subpixel via
    parabolic peak interpolation. Returns (dx, dy, peak_response)."""
    n = a.shape[0]
    win = hann2d(n)
    fa = np.fft.fft2((a - a.mean()) * win)
    fb = np.fft.fft2((b - b.mean()) * win)
    r = fa * np.conj(fb)
    r /= np.abs(r) + 1e-9
    c = np.real(np.fft.ifft2(r))
    peak = np.unravel_index(np.argmax(c), c.shape)
    py, px = peak
    def para(cm, c0, cp):
        d = (cm - cp) / (2 * (cm - 2 * c0 + cp) + 1e-12)
        return d
    dy = py + para(c[(py - 1) % n, px], c[py, px], c[(py + 1) % n, px])
    dx = px + para(c[py, (px - 1) % n], c[py, px], c[py, (px + 1) % n])
    if dy > n / 2: dy -= n
    if dx > n / 2: dx -= n
    resp = float(c[py, px])
    return float(dx), float(dy), resp

def crop(img, cx, cy, size):
    h, w = img.shape
    x0, y0 = int(cx - size // 2), int(cy - size // 2)
    if x0 < 0 or y0 < 0 or x0 + size > w or y0 + size > h:
        return None
    return img[y0:y0 + size, x0:x0 + size]

def fit_similarity(pairs):
    """pairs: list of ((x,y) in target, (x,y) in ref). Solve
    ref = [a -b; b a] * target + t (image coords, y-down; plain 2D)."""
    rows, rhs = [], []
    for (tx_, ty_), (rx, ry) in pairs:
        rows.append([tx_, -ty_, 1, 0]); rhs.append(rx)
        rows.append([ty_,  tx_, 0, 1]); rhs.append(ry)
    sol, *_ = np.linalg.lstsq(np.array(rows, float), np.array(rhs, float),
                              rcond=None)
    a, b, tx, ty = sol
    res = []
    for (tx_, ty_), (rx, ry) in pairs:
        px = a * tx_ - b * ty_ + tx
        py = b * tx_ + a * ty_ + ty
        res.append(math.hypot(px - rx, py - ry))
    return (a, b, tx, ty), res

def main():
    cat = json.load(open("catalog.json"))
    main_field = [s for s in cat["sheets"] if s["file"].startswith("12440")]
    ref_img = decode(REF + ".jp2")
    ref_patches = []
    for cx, cy in PATCH_CENTERS:
        p = crop(ref_img, cx / SCALE, cy / SCALE, PATCH)
        ref_patches.append(p)
    out = {}
    for s in main_field:
        fid = s["file"][:-4]
        if fid == REF:
            out[fid] = {"toRef": [1, 0, 0, 0], "patchResidualPx": 0.0,
                        "patches": len(PATCH_CENTERS), "note": "reference sheet"}
            print(f"{fid}: reference")
            continue
        img = decode(fid + ".jp2")
        pairs, resps = [], []
        for (cx, cy), rp in zip(PATCH_CENTERS, ref_patches):
            if rp is None:
                continue
            tp = crop(img, cx / SCALE, cy / SCALE, PATCH)
            if tp is None:
                continue
            dx, dy, resp = phase_corr(rp, tp)
            # target patch center that corresponds to ref (cx,cy):
            # correlation gives shift of tp relative to rp
            txy = (cx / SCALE - dx, cy / SCALE - dy)
            pairs.append((txy, (cx / SCALE, cy / SCALE)))
            resps.append(resp)
        (a, b, tx, ty), res = fit_similarity(pairs)
        # convert to full-res: coordinates scale by SCALE; a,b unchanged
        T = [a, b, tx * SCALE, ty * SCALE]
        rms = float(np.sqrt(np.mean(np.square(res)))) * SCALE
        s_ = math.hypot(a, b); rot = math.degrees(math.atan2(b, a))
        out[fid] = {"toRef": T, "patchResidualPx": round(rms, 2),
                    "patches": len(pairs),
                    "scaleVsRef": round(s_, 6), "rotVsRefDeg": round(rot, 4),
                    "respMin": round(min(resps), 3)}
        print(f"{fid}: scale {s_:.5f} rot {rot:+.3f} deg rms {rms:.1f} px "
              f"resp>={min(resps):.3f} n={len(pairs)}")
    json.dump(out, open("georef_working.json", "w"), indent=1)

if __name__ == "__main__":
    main()
