// Georeference a scanned historical map with two tie points: solve the
// similarity transform (uniform scale + rotation + translation) from image
// pixels (y down) to battlefield-local meters (z north, y up). Two points is
// the fastest workflow that still handles rotated scans; affine (3 pts) is a
// future upgrade for skewed scans.

export interface Similarity {
  a: number; // = s*cos(theta)
  b: number; // = s*sin(theta)
  tx: number;
  ty: number;
}

export interface TiePoint {
  img: readonly [number, number];
  local: readonly [number, number];
}

export function similarityFromTwoPoints(p: TiePoint, q: TiePoint): Similarity {
  // Work in a y-up image frame so rotation comes out conventional:
  // local = [a -b; b a] * (imgX, -imgY) + (tx, ty)
  const x1 = p.img[0], y1 = -p.img[1];
  const x2 = q.img[0], y2 = -q.img[1];
  const u1 = p.local[0], v1 = p.local[1];
  const u2 = q.local[0], v2 = q.local[1];
  const dx = x2 - x1, dy = y2 - y1;
  const det = dx * dx + dy * dy;
  if (det < 1e-12) throw new Error("tie points coincide in image space");
  const du = u2 - u1, dv = v2 - v1;
  const a = (dx * du + dy * dv) / det;
  const b = (dx * dv - dy * du) / det;
  return { a, b, tx: u1 - (a * x1 - b * y1), ty: v1 - (b * x1 + a * y1) };
}

export function applySimilarity(T: Similarity, img: readonly [number, number]): [number, number] {
  const x = img[0], y = -img[1];
  return [T.a * x - T.b * y + T.tx, T.b * x + T.a * y + T.ty];
}
