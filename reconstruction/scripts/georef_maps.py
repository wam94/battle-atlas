"""Georeferencing for scanned historical map sheets (Bachelder timed set,
Elliott burial map): store and apply sheet-pixel -> battlefield-local-meter
transforms.

Conventions
-----------
Battlefield-local frame: ``local_x`` = UTM 18N (EPSG:26918) easting minus
the macro heightmap origin easting, ``local_z`` = northing minus the origin
northing (``data/heightmap/heightmap.json``; the same frame the landmark
anchors and the canonical reconstruction use).

The image->local similarity convention is a Python port of
``tool/src/overlay.ts``::

    local = [a -b; b a] * (img_x, -img_y) + (tx, ty)

i.e. image y is down, local z is north/up; ``a = s*cos(theta)``,
``b = s*sin(theta)``. Two tie points give an exact solve (the authoring
tool's workflow); n >= 2 gives a least-squares fit with residuals (this
module's addition, matching overlay.ts's "affine is a future upgrade"
note by staying within the similarity family).

Sheet-to-sheet registrations (all sheets of a set share a printed base)
are plain y-down similarities::

    ref = [c -d; d c] * (img_x, img_y) + (ex, ey)

``compose()`` turns (sheet -> reference) + (reference -> local) into
(sheet -> local) without leaving the overlay.ts convention.

The committed transform store is ``reconstruction/spatial/
bachelder-manifest.json``; ``load_manifest()`` wraps it.
"""
from __future__ import annotations

import json
import math
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Similarity:
    """Image (y-down) -> local (y-up) similarity, overlay.ts convention."""
    a: float
    b: float
    tx: float
    ty: float

    @property
    def scale(self) -> float:
        """Meters per source pixel."""
        return math.hypot(self.a, self.b)

    @property
    def rotation_deg(self) -> float:
        return math.degrees(math.atan2(self.b, self.a))

    def apply(self, img: tuple[float, float]) -> tuple[float, float]:
        x, y = img[0], -img[1]
        return (self.a * x - self.b * y + self.tx,
                self.b * x + self.a * y + self.ty)

    def invert(self, local: tuple[float, float]) -> tuple[float, float]:
        u, v = local[0] - self.tx, local[1] - self.ty
        det = self.a * self.a + self.b * self.b
        x = (self.a * u + self.b * v) / det
        y = (-self.b * u + self.a * v) / det
        return (x, -y)


@dataclass(frozen=True)
class ImgSimilarity:
    """Image (y-down) -> image (y-down) similarity (sheet registration)."""
    c: float
    d: float
    ex: float
    ey: float

    @property
    def scale(self) -> float:
        return math.hypot(self.c, self.d)

    def apply(self, img: tuple[float, float]) -> tuple[float, float]:
        x, y = img
        return (self.c * x - self.d * y + self.ex,
                self.d * x + self.c * y + self.ey)


def similarity_from_two_points(p_img, p_local, q_img, q_local) -> Similarity:
    """Exact two-point solve; port of overlay.ts similarityFromTwoPoints."""
    x1, y1 = p_img[0], -p_img[1]
    x2, y2 = q_img[0], -q_img[1]
    u1, v1 = p_local
    u2, v2 = q_local
    dx, dy = x2 - x1, y2 - y1
    det = dx * dx + dy * dy
    if det < 1e-12:
        raise ValueError("tie points coincide in image space")
    du, dv = u2 - u1, v2 - v1
    a = (dx * du + dy * dv) / det
    b = (dx * dv - dy * du) / det
    return Similarity(a, b, u1 - (a * x1 - b * y1), v1 - (b * x1 + a * y1))


def _solve4(ata, atb):
    """Solve the 4x4 normal equations (Gaussian elimination, partial pivot)."""
    n = 4
    m = [row[:] + [atb[i]] for i, row in enumerate(ata)]
    for col in range(n):
        piv = max(range(col, n), key=lambda r: abs(m[r][col]))
        if abs(m[piv][col]) < 1e-12:
            raise ValueError("degenerate tie-point configuration")
        m[col], m[piv] = m[piv], m[col]
        for r in range(n):
            if r == col:
                continue
            f = m[r][col] / m[col][col]
            for k in range(col, n + 1):
                m[r][k] -= f * m[col][k]
    return [m[i][n] / m[i][i] for i in range(n)]


def _lstsq_similarity(rows, rhs):
    ata = [[0.0] * 4 for _ in range(4)]
    atb = [0.0] * 4
    for row, b in zip(rows, rhs):
        for i in range(4):
            atb[i] += row[i] * b
            for j in range(4):
                ata[i][j] += row[i] * row[j]
    return _solve4(ata, atb)


def fit_similarity(ties) -> tuple[Similarity, list[float]]:
    """Least-squares overlay-convention similarity over n >= 2 tie points.

    ties: iterable of (img_xy, local_xz). Returns (Similarity,
    per-tie residual distances in meters, same order).
    """
    ties = list(ties)
    if len(ties) < 2:
        raise ValueError("need at least two tie points")
    rows, rhs = [], []
    for (ix, iy), (lx, lz) in ties:
        x, y = ix, -iy
        rows.append([x, -y, 1.0, 0.0]); rhs.append(lx)
        rows.append([y, x, 0.0, 1.0]);  rhs.append(lz)
    a, b, tx, ty = _lstsq_similarity(rows, rhs)
    sim = Similarity(a, b, tx, ty)
    res = [math.dist(sim.apply(img), loc) for img, loc in ties]
    return sim, res


def fit_img_similarity(pairs) -> tuple[ImgSimilarity, list[float]]:
    """Least-squares y-down image->image similarity over n >= 2 pairs.

    pairs: iterable of (src_xy, dst_xy). Residuals in destination pixels.
    """
    pairs = list(pairs)
    if len(pairs) < 2:
        raise ValueError("need at least two point pairs")
    rows, rhs = [], []
    for (sx, sy), (dx, dy) in pairs:
        rows.append([sx, -sy, 1.0, 0.0]); rhs.append(dx)
        rows.append([sy, sx, 0.0, 1.0]);  rhs.append(dy)
    c, d, ex, ey = _lstsq_similarity(rows, rhs)
    sim = ImgSimilarity(c, d, ex, ey)
    res = [math.dist(sim.apply(s), t) for s, t in pairs]
    return sim, res


def compose(to_ref: ImgSimilarity, ref_to_local: Similarity) -> Similarity:
    """(sheet -> reference pixels) then (reference -> local meters).

    Both families are similarities, so the composition stays a
    Similarity in the overlay.ts convention:
    a' = A*c + B*d, b' = B*c - A*d (with A,B the reference coefficients),
    translation = reference transform applied to to_ref's translation.
    """
    A, B = ref_to_local.a, ref_to_local.b
    c, d = to_ref.c, to_ref.d
    a2 = A * c + B * d
    b2 = B * c - A * d
    tx2, ty2 = ref_to_local.apply((to_ref.ex, to_ref.ey))
    return Similarity(a2, b2, tx2, ty2)


class SheetGeoref:
    """One sheet's georeference as loaded from the manifest."""

    def __init__(self, entry: dict):
        self.id = entry["id"]
        self.entry = entry
        g = entry["georef"]
        self.transform = Similarity(g["a"], g["b"], g["tx"], g["ty"])
        self.uncertainty_m = g["estAbsUncertaintyM"]

    def to_local(self, px: float, py: float) -> tuple[float, float]:
        """Full-resolution sheet pixel -> battlefield-local meters."""
        return self.transform.apply((px, py))

    def from_local(self, lx: float, lz: float) -> tuple[float, float]:
        """Battlefield-local meters -> full-resolution sheet pixel."""
        return self.transform.invert((lx, lz))


class MapManifest:
    def __init__(self, doc: dict):
        self.doc = doc
        self.sheets = {}
        for entry in doc["sheets"]:
            if "georef" in entry:
                self.sheets[entry["id"]] = SheetGeoref(entry)

    def __getitem__(self, sheet_id: str) -> SheetGeoref:
        return self.sheets[sheet_id]

    def __contains__(self, sheet_id: str) -> bool:
        return sheet_id in self.sheets

    def ids(self):
        return sorted(self.sheets)


DEFAULT_MANIFEST = (Path(__file__).resolve().parent.parent / "spatial" /
                    "bachelder-manifest.json")


def load_manifest(path: str | Path = DEFAULT_MANIFEST) -> MapManifest:
    with open(path) as f:
        return MapManifest(json.load(f))
