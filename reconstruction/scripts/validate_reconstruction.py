"""Validate the Angle V2 reconstruction corpus: sources, atomic claims, and the
canonical reconstruction (segments + casualty profiles), plus every cross-file
rule JSON Schema cannot express (plan section 13):

- schema validation for every document;
- source/claim reference integrity (including landcover feature refs);
- per-unit segment contiguity: no overlap, no gap, exact slice coverage;
- route/formation continuity between adjacent segments;
- routes stay inside the battlefield and honor traced obstacles: a route may
  cross a stone_wall/rail_fence line ONLY inside a segment that names that
  obstacle in obstacleIds and whose action is an authored crossing action
  (Gate P5's no-untraced-crossing rule) — and declared obstacles must really
  be crossed (no phantom declarations);
- arm-appropriate speed limits per action;
- casualty discipline: causeMix sums to 1, counts never exceed available
  strength, compiled strength is non-negative and monotone non-increasing;
- reconciliation: compiled strength equals the macro battle file's keyframe
  strengths at every keyframe time inside the slice, and its interpolated
  strength at the slice edges, within +/-1 man (integer rounding).

Usage: uv run python scripts/validate_reconstruction.py
Exit 0 when valid; exit 1 with one error per line otherwise.
"""

from __future__ import annotations

import json
import math
import sys
from dataclasses import dataclass
from pathlib import Path

import jsonschema

REPO = Path(__file__).resolve().parent.parent.parent
SCHEMAS = Path(__file__).resolve().parent.parent / "schemas"

SOURCES_PATH = REPO / "reconstruction/sources/sources.json"
CLAIMS_PATH = REPO / "reconstruction/claims/angle.claims.json"
RECON_PATH = REPO / "reconstruction/canonical/angle.reconstruction.json"
LANDCOVER_PATH = REPO / "data/landcover/landcover.json"
BATTLE_PATH = REPO / "app/Assets/Battle/gettysburg-july3.json"

# Abstract claim subjects that are neither battle units nor landcover features.
# Phase 7 adds the Codori farmyard (buildings have no traced landcover feature;
# position claims carry point geometry) and the Emmitsburg Road itself (no
# traced road-surface layer exists -- ED-6/ED-11; the corridor is the span
# between the two traced fences).
ABSTRACT_SUBJECTS = {"assault-column", "angle-slice", "codori-farm", "emmitsburg-road"}

# Battlefield-local frame extent in meters (data/heightmap/heightmap.json
# width_m/depth_m = 8507.17; that file is gitignored, so the constant is
# pinned here with its provenance).
BATTLEFIELD_EXTENT_M = 8507.2

# Landcover classes that are movement obstacles for Gate P5.
OBSTACLE_CLASSES = {"stone_wall", "rail_fence"}

# Actions allowed to carry an authored obstacle crossing. fall_back/rout
# recrossings additionally require the R-retreat-recross inference rule
# (editorial decisions ED-8 / R-retreat-recross).
CROSSING_ACTIONS = {"cross_obstacle", "breach", "fall_back", "rout"}
RETREAT_ACTIONS = {"fall_back", "rout"}

# Maximum average segment speed (m/s) per action, by arm (plan section 13,
# "arm-appropriate speed limits"; values recorded in R-linear-connective-motion).
STATIC_ACTIONS = {"hold", "halt", "dress_line", "fire_by_rank", "fire_independent", "take_canister"}
INFANTRY_SPEED_CAPS = {
    "hold": 0.35, "halt": 0.35, "dress_line": 0.35,
    "fire_by_rank": 0.35, "fire_independent": 0.35, "take_canister": 0.35,
    "advance": 1.5, "oblique": 1.5, "close_gap": 1.5,
    "double_quick": 2.7, "cross_obstacle": 1.4, "breach": 1.0,
    "waver": 0.5, "fall_back": 2.0, "rout": 3.5,
}
# Artillery in this slice only holds or is manhandled short distances.
ARTILLERY_SPEED_CAP = 0.5

STRENGTH_TOLERANCE = 1.0  # men; integer rounding of interpolated endpoints


# ---------------------------------------------------------------------------
# Loading

@dataclass
class Corpus:
    sources: dict
    claims: dict
    recon: dict
    landcover: dict
    battle: dict


def load_corpus(
    sources_path: Path = SOURCES_PATH,
    claims_path: Path = CLAIMS_PATH,
    recon_path: Path = RECON_PATH,
    landcover_path: Path = LANDCOVER_PATH,
    battle_path: Path = BATTLE_PATH,
) -> Corpus:
    return Corpus(
        sources=json.loads(sources_path.read_text()),
        claims=json.loads(claims_path.read_text()),
        recon=json.loads(recon_path.read_text()),
        landcover=json.loads(landcover_path.read_text()),
        battle=json.loads(battle_path.read_text()),
    )


def _schema(name: str) -> dict:
    return json.loads((SCHEMAS / name).read_text())


# ---------------------------------------------------------------------------
# Geometry helpers

def seg_intersect(p1, p2, q1, q2) -> bool:
    """Proper segment intersection (shared endpoints/collinear touch count too,
    within a small epsilon), 2D."""
    def cross(o, a, b):
        return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])

    eps = 1e-9
    d1 = cross(q1, q2, p1)
    d2 = cross(q1, q2, p2)
    d3 = cross(p1, p2, q1)
    d4 = cross(p1, p2, q2)
    if ((d1 > eps and d2 < -eps) or (d1 < -eps and d2 > eps)) and \
       ((d3 > eps and d4 < -eps) or (d3 < -eps and d4 > eps)):
        return True

    def on_seg(a, b, c):
        return (min(a[0], b[0]) - eps <= c[0] <= max(a[0], b[0]) + eps and
                min(a[1], b[1]) - eps <= c[1] <= max(a[1], b[1]) + eps)

    if abs(d1) <= eps and on_seg(q1, q2, p1):
        return True
    if abs(d2) <= eps and on_seg(q1, q2, p2):
        return True
    if abs(d3) <= eps and on_seg(p1, p2, q1):
        return True
    if abs(d4) <= eps and on_seg(p1, p2, q2):
        return True
    return False


def route_crosses_feature(route: list, feature: dict) -> bool:
    pts = feature["points"]
    for a, b in zip(route, route[1:]):
        for fa, fb in zip(pts, pts[1:]):
            if seg_intersect(a, b, fa, fb):
                return True
    return False


def route_length(route: list) -> float:
    return sum(math.dist(a, b) for a, b in zip(route, route[1:]))


# ---------------------------------------------------------------------------
# Casualty compilation (shared with the compiler for reconciliation)

def _curve_weight(curve: str, u: float) -> float:
    u = min(max(u, 0.0), 1.0)
    if curve == "uniform":
        return u
    if curve == "rising":
        return u * u
    if curve == "falling":
        return 1.0 - (1.0 - u) * (1.0 - u)
    if curve == "spike":
        return u * u * (3.0 - 2.0 * u)  # smoothstep: center-weighted density
    raise ValueError(f"unknown intensityCurve {curve!r}")


def cumulative_losses(profiles: list, t: float) -> int:
    """Deterministic integer losses for one unit from slice start through
    battle-second t (inclusive)."""
    total = 0
    for p in profiles:
        if t < p["t0"]:
            continue
        u = 1.0 if t >= p["t1"] else (t - p["t0"]) / (p["t1"] - p["t0"])
        total += round(p["count"] * _curve_weight(p["intensityCurve"], u))
    return total


def compiled_strength(start: int, profiles: list, t: float) -> int:
    return start - cumulative_losses(profiles, t)


def macro_strength_at(keyframes: list, t: float) -> float:
    if t <= keyframes[0]["t"]:
        return keyframes[0]["strength"]
    if t >= keyframes[-1]["t"]:
        return keyframes[-1]["strength"]
    for a, b in zip(keyframes, keyframes[1:]):
        if a["t"] <= t <= b["t"]:
            f = (t - a["t"]) / (b["t"] - a["t"])
            return a["strength"] + f * (b["strength"] - a["strength"])
    raise AssertionError("unreachable")


# ---------------------------------------------------------------------------
# Validation

def validate_corpus(corpus: Corpus) -> list[str]:
    errors: list[str] = []

    # 1. Schemas -----------------------------------------------------------
    for name, doc in (
        ("source.schema.json", corpus.sources),
        ("claim.schema.json", corpus.claims),
        ("reconstruction.schema.json", corpus.recon),
    ):
        validator = jsonschema.Draft202012Validator(_schema(name))
        for err in sorted(validator.iter_errors(doc), key=lambda e: list(e.absolute_path)):
            path = "/".join(str(p) for p in err.absolute_path) or "<root>"
            errors.append(f"schema[{name}]: {path}: {err.message}")
    if errors:
        return errors  # cross-file checks assume schema shape

    sources = corpus.sources["sources"]
    claims = corpus.claims["claims"]
    recon = corpus.recon
    features = {f["id"]: f for f in corpus.landcover["features"]}
    units = {u["id"]: u for u in corpus.battle["units"]}

    # 2. Sources -----------------------------------------------------------
    source_ids = [s["id"] for s in sources]
    for sid in sorted({s for s in source_ids if source_ids.count(s) > 1}):
        errors.append(f"sources: duplicate source id {sid!r}")
    source_id_set = set(source_ids)

    # 3. Claims ------------------------------------------------------------
    claim_ids = [c["id"] for c in claims]
    for cid in sorted({c for c in claim_ids if claim_ids.count(c) > 1}):
        errors.append(f"claims: duplicate claim id {cid!r}")
    claim_by_id = {c["id"]: c for c in claims}

    for c in claims:
        where = f"claims[{c['id']}]"
        if c["subjectId"] not in units and c["subjectId"] not in features \
                and c["subjectId"] not in ABSTRACT_SUBJECTS:
            errors.append(f"{where}: subjectId {c['subjectId']!r} is not a battle unit, "
                          f"landcover feature, or declared abstract subject")
        for r in c["references"]:
            if r["sourceId"] not in source_id_set:
                errors.append(f"{where}: reference to unknown source {r['sourceId']!r}")
        if c["assessment"] == "documented" and not c["references"]:
            errors.append(f"{where}: documented claims require at least one reference")
        t = c.get("time")
        if t and not (t["earliest"] <= t["preferred"] <= t["latest"]):
            errors.append(f"{where}: time envelope must satisfy earliest <= preferred <= latest")
        g = c.get("geometry")
        if g and g["kind"] == "featureRef" and g["featureId"] not in features:
            errors.append(f"{where}: geometry.featureId {g['featureId']!r} not in landcover")

    # 4. Reconstruction: units and references ------------------------------
    slice_t0 = recon["slice"]["t0"]
    slice_t1 = recon["slice"]["t1"]
    if not slice_t1 > slice_t0:
        errors.append("reconstruction: slice.t1 must exceed slice.t0")

    recon_unit_ids = [u["unitId"] for u in recon["units"]]
    for uid in sorted({u for u in recon_unit_ids if recon_unit_ids.count(u) > 1}):
        errors.append(f"reconstruction: duplicate unit {uid!r}")
    arms = {u["unitId"]: u["arm"] for u in recon["units"]}
    starts = {u["unitId"]: u["startStrength"]["value"] for u in recon["units"]}
    for u in recon["units"]:
        if u["unitId"] not in units:
            errors.append(f"reconstruction: unit {u['unitId']!r} not in battle file")
        for cid in u["startStrength"].get("claimIds", []):
            if cid not in claim_by_id:
                errors.append(f"reconstruction[{u['unitId']}]: startStrength cites unknown claim {cid!r}")

    seg_ids = [s["id"] for s in recon["segments"]]
    for sid in sorted({s for s in seg_ids if seg_ids.count(s) > 1}):
        errors.append(f"segments: duplicate segment id {sid!r}")

    obstacle_features = {fid: f for fid, f in features.items()
                         if f["kind"] == "line" and f["cls"] in OBSTACLE_CLASSES}

    segs_by_unit: dict[str, list] = {uid: [] for uid in arms}
    for s in recon["segments"]:
        where = f"segments[{s['id']}]"
        if s["unitId"] not in arms:
            errors.append(f"{where}: unitId {s['unitId']!r} not declared in reconstruction units")
            continue
        segs_by_unit[s["unitId"]].append(s)
        if not s["t1"] > s["t0"]:
            errors.append(f"{where}: t1 must exceed t0")
        if not s["claimIds"] and not s["inferenceRules"]:
            errors.append(f"{where}: must cite at least one claim or name an inference rule "
                          f"(editorial connective reconstruction)")
        for cid in s["claimIds"]:
            if cid not in claim_by_id:
                errors.append(f"{where}: cites unknown claim {cid!r}")
            else:
                subj = claim_by_id[cid]["subjectId"]
                if subj in units and subj != s["unitId"] and subj != units[s["unitId"]].get("parent"):
                    errors.append(f"{where}: cites claim {cid!r} about a different unit ({subj!r})")
        for oid in s.get("obstacleIds", []):
            if oid not in obstacle_features:
                errors.append(f"{where}: obstacleIds entry {oid!r} is not a traced "
                              f"stone_wall/rail_fence landcover line")

    # 5. Per-unit contiguity, continuity, geometry, speed -------------------
    for uid, segs in sorted(segs_by_unit.items()):
        if not segs:
            errors.append(f"reconstruction[{uid}]: no segments")
            continue
        segs = sorted(segs, key=lambda s: (s["t0"], s["t1"]))
        if segs[0]["t0"] != slice_t0:
            errors.append(f"reconstruction[{uid}]: first segment starts at {segs[0]['t0']}, "
                          f"not slice t0 {slice_t0}")
        if segs[-1]["t1"] != slice_t1:
            errors.append(f"reconstruction[{uid}]: last segment ends at {segs[-1]['t1']}, "
                          f"not slice t1 {slice_t1}")
        for a, b in zip(segs, segs[1:]):
            if a["t1"] != b["t0"]:
                kind = "overlap" if a["t1"] > b["t0"] else "gap"
                errors.append(f"reconstruction[{uid}]: {kind} between {a['id']} (t1={a['t1']}) "
                              f"and {b['id']} (t0={b['t0']})")
            if math.dist(a["route"][-1], b["route"][0]) > 0.5:
                errors.append(f"reconstruction[{uid}]: route discontinuity between {a['id']} "
                              f"and {b['id']} ({math.dist(a['route'][-1], b['route'][0]):.1f} m)")
            if a["formationTo"] != b["formationFrom"]:
                errors.append(f"reconstruction[{uid}]: formation discontinuity between "
                              f"{a['id']} ({a['formationTo']}) and {b['id']} ({b['formationFrom']})")

        for s in segs:
            where = f"segments[{s['id']}]"
            for x, z in s["route"]:
                if not (0 <= x <= BATTLEFIELD_EXTENT_M and 0 <= z <= BATTLEFIELD_EXTENT_M):
                    errors.append(f"{where}: route point ({x}, {z}) outside the battlefield "
                                  f"(0..{BATTLEFIELD_EXTENT_M} m)")

            # Speed limits (average over the segment).
            dist = route_length(s["route"])
            speed = dist / (s["t1"] - s["t0"]) if s["t1"] > s["t0"] else 0.0
            if arms[uid] == "artillery":
                cap = ARTILLERY_SPEED_CAP
            else:
                cap = INFANTRY_SPEED_CAPS[s["action"]]
            if speed > cap + 1e-9:
                errors.append(f"{where}: average speed {speed:.2f} m/s exceeds the "
                              f"{arms[uid]} cap {cap} m/s for action {s['action']!r}")

            # Obstacle discipline (Gate P5).
            declared = set(s.get("obstacleIds", []))
            crossed = {fid for fid, f in sorted(obstacle_features.items())
                       if route_crosses_feature(s["route"], f)}
            for fid in sorted(crossed - declared):
                errors.append(f"{where}: route crosses traced obstacle {fid!r} without an "
                              f"authored crossing (obstacleIds)")
            for fid in sorted(declared - crossed):
                errors.append(f"{where}: declares obstacle {fid!r} but the route never "
                              f"crosses it (phantom crossing)")
            if declared and s["action"] not in CROSSING_ACTIONS:
                errors.append(f"{where}: action {s['action']!r} may not carry an obstacle "
                              f"crossing (allowed: {sorted(CROSSING_ACTIONS)})")
            if declared and s["action"] in RETREAT_ACTIONS \
                    and "R-retreat-recross" not in s["inferenceRules"]:
                errors.append(f"{where}: retreat crossing requires the R-retreat-recross "
                              f"inference rule")

    # 6. Casualty profiles ---------------------------------------------------
    prof_ids = [p["id"] for p in recon["casualtyProfiles"]]
    for pid in sorted({p for p in prof_ids if prof_ids.count(p) > 1}):
        errors.append(f"casualtyProfiles: duplicate id {pid!r}")

    profiles_by_unit: dict[str, list] = {uid: [] for uid in arms}
    for p in recon["casualtyProfiles"]:
        where = f"casualtyProfiles[{p['id']}]"
        if p["unitId"] not in arms:
            errors.append(f"{where}: unitId {p['unitId']!r} not declared in reconstruction units")
            continue
        profiles_by_unit[p["unitId"]].append(p)
        if not p["t1"] > p["t0"]:
            errors.append(f"{where}: t1 must exceed t0")
        if p["t0"] < slice_t0 or p["t1"] > slice_t1:
            errors.append(f"{where}: window outside the slice")
        mix = sum(p["causeMix"].values())
        if abs(mix - 1.0) > 1e-6:
            errors.append(f"{where}: causeMix sums to {mix}, not 1.0")
        for cid in p["claimIds"]:
            if cid not in claim_by_id:
                errors.append(f"{where}: cites unknown claim {cid!r}")

    for uid, profs in sorted(profiles_by_unit.items()):
        profs = sorted(profs, key=lambda p: (p["t0"], p["t1"]))
        for a, b in zip(profs, profs[1:]):
            if a["t1"] > b["t0"]:
                errors.append(f"casualtyProfiles[{uid}]: overlap between {a['id']} and {b['id']}")
        total = sum(p["count"] for p in profs)
        if uid in starts and total > starts[uid]:
            errors.append(f"casualtyProfiles[{uid}]: total losses {total} exceed start "
                          f"strength {starts[uid]}")

    # 7. Strength reconciliation against the macro battle file ---------------
    for uid in sorted(arms):
        if uid not in units or uid not in starts:
            continue
        profs = sorted(profiles_by_unit[uid], key=lambda p: p["t0"])
        kfs = units[uid]["keyframes"]
        check_times = [slice_t0] + [kf["t"] for kf in kfs if slice_t0 < kf["t"] < slice_t1] + [slice_t1]
        for t in check_times:
            got = compiled_strength(starts[uid], profs, t)
            want = macro_strength_at(kfs, t)
            if abs(got - want) > STRENGTH_TOLERANCE:
                errors.append(f"reconciliation[{uid}]: compiled strength {got} at t={t} "
                              f"deviates from macro track {want:.1f} by more than "
                              f"{STRENGTH_TOLERANCE}")
            if got < 0:
                errors.append(f"reconciliation[{uid}]: negative compiled strength at t={t}")
        # Monotone non-increasing across the whole slice.
        prev = starts[uid]
        for t in range(int(slice_t0), int(slice_t1) + 1):
            cur = compiled_strength(starts[uid], profs, t)
            if cur > prev:
                errors.append(f"reconciliation[{uid}]: strength increases at t={t}")
                break
            prev = cur

    return errors


def main(argv: list[str]) -> int:
    corpus = load_corpus()
    errors = validate_corpus(corpus)
    for e in errors:
        print(e, file=sys.stderr)
    if not errors:
        recon = corpus.recon
        print(f"OK: {len(corpus.sources['sources'])} sources, "
              f"{len(corpus.claims['claims'])} claims, "
              f"{len(recon['units'])} units, {len(recon['segments'])} segments, "
              f"{len(recon['casualtyProfiles'])} casualty profiles")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
