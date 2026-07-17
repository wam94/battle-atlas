"""Route-constrained, action-aware compiler for the Angle V2 slice.

Reads the validated corpus (sources, claims, canonical reconstruction) plus the
traced obstacle geometry (data/landcover/landcover.json) and the macro battle
file, and emits:

- app/Assets/Battle/Angle/angle.bundle.json — deterministic per-second unit
  state (position, facing, formation, strength, action, provenance) for every
  second of t=8040..9000, plus the semantic segments and casualty profiles the
  states trace to. The Unity runtime does NOT read this file yet (Phase 5
  changes no macro battle behavior); it is the compiled tactical artifact the
  later phases build on.
- docs/reconstruction/angle-bundle-audit.md — the human-readable Gate P5 audit
  (corpus counts, provenance ratios, reconciliation, authored crossings).

Both outputs are byte-deterministic: no timestamps, sorted keys, fixed float
formatting, and a sha256 content checksum embedded in the bundle.

Usage: uv run python scripts/compile_angle.py
Exits 1 (writing nothing) when the corpus fails validation.
"""

from __future__ import annotations

import hashlib
import json
import math
import sys
from collections import Counter
from pathlib import Path

from validate_reconstruction import (
    BATTLE_PATH,
    CLAIMS_PATH,
    LANDCOVER_PATH,
    RECON_PATH,
    SOURCES_PATH,
    Corpus,
    compiled_strength,
    cumulative_losses,
    load_corpus,
    macro_strength_at,
    validate_corpus,
)

import strike_correlation

REPO = Path(__file__).resolve().parent.parent.parent
BUNDLE_PATH = REPO / "app/Assets/Battle/Angle/angle.bundle.json"
AUDIT_PATH = REPO / "docs/reconstruction/angle-bundle-audit.md"

BUNDLE_FORMAT = "angle-bundle/1"

# ED-21 (never re-rolled): the battle seed for every hash-drawn staging
# decision (victim draws, step phase, yaw, waver) is PINNED at the checksum
# of the bundle the owner reviewed and shipped (Phases 8-10 media, P12
# release), so recompiles that only touch provenance metadata (sources /
# claims edition notes, clock profiles) cannot re-roll the film or desync
# the committed captions from the baked audio mix. Bump DELIBERATELY, with
# an editorial decision, only when choreography content changes.
STAGING_SEED = "d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1"


# ---------------------------------------------------------------------------
# Deterministic kinematics

def _ease(pace: str, u: float) -> float:
    """Arc-length fraction covered at time fraction u. Polynomials only, for
    cross-run determinism."""
    u = min(max(u, 0.0), 1.0)
    if pace in ("steady", "static"):
        return u
    if pace == "surge":
        return u * u
    if pace == "slowing":
        return 1.0 - (1.0 - u) * (1.0 - u)
    if pace == "halt-compress-cross-redress":
        return u * u * (3.0 - 2.0 * u)  # slow-compress, fast crossing, redress
    raise ValueError(f"unknown paceProfile {pace!r}")


def _route_point(route: list, frac: float) -> tuple[float, float]:
    if len(route) == 1:
        return route[0][0], route[0][1]
    legs = [math.dist(a, b) for a, b in zip(route, route[1:])]
    total = sum(legs)
    if total == 0:
        return route[0][0], route[0][1]
    target = frac * total
    run = 0.0
    for (a, b), leg in zip(zip(route, route[1:]), legs):
        if run + leg >= target or leg == legs[-1] and (a, b) == (route[-2], route[-1]):
            f = 0.0 if leg == 0 else (target - run) / leg
            f = min(max(f, 0.0), 1.0)
            return a[0] + f * (b[0] - a[0]), a[1] + f * (b[1] - a[1])
        run += leg
    return route[-1][0], route[-1][1]


def _route_bearing(route: list, frac: float) -> float | None:
    """Compass bearing (0=north/+z, 90=east/+x) of the route leg at frac."""
    if len(route) < 2:
        return None
    legs = [math.dist(a, b) for a, b in zip(route, route[1:])]
    total = sum(legs)
    if total == 0:
        return None
    target = min(max(frac, 0.0), 1.0) * total
    run = 0.0
    chosen = (route[0], route[1])
    for (a, b), leg in zip(zip(route, route[1:]), legs):
        chosen = (a, b)
        if run + leg >= target and leg > 0:
            break
        run += leg
    (ax, az), (bx, bz) = chosen
    if ax == bx and az == bz:
        return None
    return math.degrees(math.atan2(bx - ax, bz - az)) % 360.0


def _lerp_bearing(a: float, b: float, u: float) -> float:
    """Shortest-arc interpolation between compass bearings."""
    diff = ((b - a + 180.0) % 360.0) - 180.0
    return (a + diff * u) % 360.0


def segment_provenance(seg: dict, claim_by_id: dict) -> str:
    """documented > inferred > editorial, from the cited claims' assessments."""
    if not seg["claimIds"]:
        return "editorial"
    assessments = {claim_by_id[cid]["assessment"] for cid in seg["claimIds"]}
    if "documented" in assessments:
        return "documented"
    return "inferred"


# ---------------------------------------------------------------------------
# Compilation

def compile_bundle(corpus: Corpus) -> dict:
    recon = corpus.recon
    claim_by_id = {c["id"]: c for c in corpus.claims["claims"]}
    battle_units = {u["id"]: u for u in corpus.battle["units"]}
    t0, t1 = int(recon["slice"]["t0"]), int(recon["slice"]["t1"])
    seconds = list(range(t0, t1 + 1))

    segs_by_unit: dict[str, list] = {}
    for s in recon["segments"]:
        segs_by_unit.setdefault(s["unitId"], []).append(s)
    for segs in segs_by_unit.values():
        segs.sort(key=lambda s: s["t0"])
    profiles_by_unit: dict[str, list] = {}
    for p in recon["casualtyProfiles"]:
        profiles_by_unit.setdefault(p["unitId"], []).append(p)
    for profs in profiles_by_unit.values():
        profs.sort(key=lambda p: p["t0"])

    units_out = []
    for u in recon["units"]:
        uid = u["unitId"]
        segs = segs_by_unit[uid]
        profs = profiles_by_unit.get(uid, [])
        start = u["startStrength"]["value"]
        bu = battle_units[uid]

        xs: list[float] = []
        zs: list[float] = []
        facings: list[float] = []
        strengths: list[int] = []
        seg_index: list[int] = []

        prev_facing: float | None = None
        for t in seconds:
            # Active segment: the one with t0 <= t < t1 (last one owns t == slice t1).
            si = len(segs) - 1
            for i, s in enumerate(segs):
                if s["t0"] <= t < s["t1"]:
                    si = i
                    break
            s = segs[si]
            u_time = (t - s["t0"]) / (s["t1"] - s["t0"])
            frac = _ease(s["paceProfile"], u_time)
            x, z = _route_point(s["route"], frac)

            if "facingDeg" in s and "facingToDeg" in s:
                facing = _lerp_bearing(s["facingDeg"], s["facingToDeg"], u_time)
            elif "facingDeg" in s:
                facing = s["facingDeg"]
            else:
                bearing = _route_bearing(s["route"], frac)
                if bearing is None:
                    bearing = prev_facing if prev_facing is not None else bu["keyframes"][0]["facing"]
                facing = bearing
            prev_facing = facing

            xs.append(round(x, 1))
            zs.append(round(z, 1))
            facings.append(round(facing, 1))
            seg_index.append(si)

        out_segments = []
        for s in segs:
            seg_out = {
                "id": s["id"],
                "t0": s["t0"],
                "t1": s["t1"],
                "action": s["action"],
                "provenance": segment_provenance(s, claim_by_id),
                "formationFrom": s["formationFrom"],
                "formationTo": s["formationTo"],
                "paceProfile": s["paceProfile"],
                "route": s["route"],
                "obstacleIds": s.get("obstacleIds", []),
                "claimIds": s["claimIds"],
                "inferenceRules": s["inferenceRules"],
            }
            # angle-v2 P3: symmetric melee wiring rides into the bundle.
            if "meleeOpponentId" in s:
                seg_out["meleeOpponentId"] = s["meleeOpponentId"]
            out_segments.append(seg_out)

        unit_out = {
            "unitId": uid,
            "name": bu["name"],
            "side": bu["side"],
            "arm": u["arm"],
            "startStrength": start,
            "segments": out_segments,
            "casualtyProfiles": [
                {
                    "id": p["id"],
                    "t0": p["t0"],
                    "t1": p["t1"],
                    "count": p["count"],
                    "intensityCurve": p["intensityCurve"],
                    "causeMix": p["causeMix"],
                    "assessment": p["assessment"],
                    "claimIds": p["claimIds"],
                }
                for p in profs
            ],
            "perSecond": {
                "t0": t0,
                "x": xs,
                "z": zs,
                "facingDeg": facings,
                # strengths land in the post-pass below (strike-correlated
                # profiles count their emitted fallTimes)
                "strength": [],
                "segmentIndex": seg_index,
            },
        }
        # angle-v2 P4/P5: cited unit-level vocabulary fields.
        if "colorParty" in u:
            unit_out["colorParty"] = u["colorParty"]["value"]
            unit_out["colorPartyClaimIds"] = u["colorParty"]["claimIds"]
        if "mountedOfficers" in u:
            unit_out["mountedOfficers"] = [
                {
                    "officerId": mo["officerId"],
                    "unitId": uid,
                    "fallT": mo["fallT"],
                    "backOffsetM": mo["backOffsetM"],
                    "alongOffsetM": mo["alongOffsetM"],
                    "claimIds": mo["claimIds"],
                }
                for mo in u["mountedOfficers"]
            ]
        if u["arm"] == "artillery":
            unit_out["_shotTimes"] = strike_correlation.compile_cannon(
                STAGING_SEED, uid, unit_out["segments"])
        units_out.append(unit_out)

    # ---- angle-v2 P2 post-pass (proposed ED-82): strike-correlated fall
    # times, then per-second strengths (correlated profiles count their
    # own emitted times; smooth profiles keep the closed-form curve).
    strikes_by_unit = strike_correlation.compile_strikes(units_out)
    profs_by_unit = profiles_by_unit  # canonical dicts (carry strikeCorrelation)
    for unit_out in units_out:
        uid = unit_out["unitId"]
        unit_strikes = strikes_by_unit.get(uid, [])
        fall_times_by_pid: dict[str, list[float]] = {}
        for p in profs_by_unit.get(uid, []):
            if "strikeCorrelation" in p and p["count"] > 0:
                fall_times_by_pid[p["id"]] = strike_correlation.profile_fall_times(
                    p, unit_strikes)
        for p_out in unit_out["casualtyProfiles"]:
            if p_out["id"] in fall_times_by_pid:
                p_out["fallTimes"] = fall_times_by_pid[p_out["id"]]
                src = next(p for p in profs_by_unit[uid] if p["id"] == p_out["id"])
                p_out["strikeCorrelation"] = src["strikeCorrelation"]
        profs = profs_by_unit.get(uid, [])
        start = unit_out["startStrength"]
        strengths = unit_out["perSecond"]["strength"]
        for t in seconds:
            losses = 0
            for p in profs:
                ft = fall_times_by_pid.get(p["id"])
                if ft is not None:
                    losses += strike_correlation.cumulative_fallen(ft, t)
                else:
                    losses += cumulative_losses([p], t)
            strengths.append(start - losses)
        unit_out.pop("_shotTimes", None)

    claims_index = {
        c["id"]: {
            "property": c["property"],
            "assessment": c["assessment"],
            "sourceIds": sorted({r["sourceId"] for r in c["references"]}),
        }
        for c in corpus.claims["claims"]
    }

    payload = {
        "format": BUNDLE_FORMAT,
        "stagingSeed": STAGING_SEED,
        "note": "Compiled tactical artifact for the Angle slice (Reconstruction V2 "
                "Phase 5). Per-second state traces to semantic segments; segments "
                "trace to atomic claims and/or named inference rules "
                "(docs/reconstruction/angle-editorial-decisions.md). Not yet read "
                "by the Unity runtime.",
        "slice": {"t0": t0, "t1": t1},
        "clock": {"startTimeSecondsSinceMidnight": corpus.battle["startTime"]},
        "inputs": {
            "sources": _sha256_file(SOURCES_PATH),
            "claims": _sha256_file(CLAIMS_PATH),
            "reconstruction": _sha256_file(RECON_PATH),
            "landcover": _sha256_file(LANDCOVER_PATH),
            "battle": _sha256_file(BATTLE_PATH),
        },
        "claimsIndex": claims_index,
        "units": units_out,
    }
    payload["checksum"] = bundle_checksum(payload)
    return payload


def _sha256_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def bundle_checksum(payload: dict) -> str:
    body = {k: v for k, v in payload.items() if k != "checksum"}
    canonical = json.dumps(body, sort_keys=True, separators=(",", ":"), ensure_ascii=False)
    return hashlib.sha256(canonical.encode("utf-8")).hexdigest()


def dump_bundle(payload: dict) -> str:
    return json.dumps(payload, sort_keys=True, separators=(",", ":"), ensure_ascii=False) + "\n"


# ---------------------------------------------------------------------------
# Audit

def build_audit(corpus: Corpus, bundle: dict) -> str:
    recon = corpus.recon
    claims = corpus.claims["claims"]
    t0, t1 = bundle["slice"]["t0"], bundle["slice"]["t1"]
    n_seconds = t1 - t0 + 1

    prop_counts = Counter(c["property"] for c in claims)
    ass_counts = Counter(c["assessment"] for c in claims)
    action_counts = Counter(s["action"] for s in recon["segments"])

    lines: list[str] = []
    lines.append("# Angle Bundle Audit (Gate P5)")
    lines.append("")
    lines.append("Generated by `reconstruction/scripts/compile_angle.py`; regenerate with "
                 "`uv run python scripts/compile_angle.py` from `reconstruction/`. "
                 "Deterministic — no timestamps; byte-identical for identical inputs.")
    lines.append("")
    lines.append(f"- Slice: t={t0}..{t1} ({n_seconds} compiled seconds per unit; "
                 f"15:14-15:30 on the canonical clock)")
    lines.append(f"- Sources: {len(corpus.sources['sources'])}")
    lines.append(f"- Claims: {len(claims)}")
    lines.append(f"- Units: {len(recon['units'])}  |  Segments: {len(recon['segments'])}  |  "
                 f"Casualty profiles: {len(recon['casualtyProfiles'])}")
    lines.append(f"- Bundle checksum: `{bundle['checksum']}`")
    lines.append("")

    lines.append("## Claims by property")
    lines.append("")
    lines.append("| property | claims |")
    lines.append("|---|---|")
    for prop, n in sorted(prop_counts.items()):
        lines.append(f"| {prop} | {n} |")
    lines.append("")
    lines.append("## Claims by assessment")
    lines.append("")
    lines.append("| assessment | claims |")
    lines.append("|---|---|")
    for a, n in sorted(ass_counts.items()):
        lines.append(f"| {a} | {n} |")
    lines.append("")

    lines.append("## Segments by action")
    lines.append("")
    lines.append("| action | segments |")
    lines.append("|---|---|")
    for a, n in sorted(action_counts.items()):
        lines.append(f"| {a} | {n} |")
    lines.append("")

    # Provenance of compiled seconds (each unit-second belongs to exactly one
    # segment; the segment's provenance classifies it).
    lines.append("## Compiled-second provenance (documented / inferred / editorial)")
    lines.append("")
    lines.append("A unit-second's provenance is its owning segment's: `documented` when the "
                 "segment cites at least one documented claim, `inferred` when it cites only "
                 "inferred/unknown claims, `editorial` when it relies on named inference "
                 "rules alone.")
    lines.append("")
    lines.append("| unit | documented | inferred | editorial |")
    lines.append("|---|---|---|---|")
    grand = Counter()
    for u in bundle["units"]:
        counts = Counter()
        for s in u["segments"]:
            end = s["t1"] if s["t1"] == t1 else s["t1"] - 1
            counts[s["provenance"]] += int(end) - int(s["t0"]) + 1
        grand.update(counts)
        total = sum(counts.values())
        lines.append(
            f"| {u['unitId']} | "
            f"{counts['documented']} ({100 * counts['documented'] / total:.1f}%) | "
            f"{counts['inferred']} ({100 * counts['inferred'] / total:.1f}%) | "
            f"{counts['editorial']} ({100 * counts['editorial'] / total:.1f}%) |")
    gt = sum(grand.values())
    lines.append(
        f"| **all units** | "
        f"**{grand['documented']} ({100 * grand['documented'] / gt:.1f}%)** | "
        f"**{grand['inferred']} ({100 * grand['inferred'] / gt:.1f}%)** | "
        f"**{grand['editorial']} ({100 * grand['editorial'] / gt:.1f}%)** |")
    lines.append("")

    lines.append("## Authored obstacle crossings")
    lines.append("")
    lines.append("Every intersection between a compiled route and a traced stone_wall/"
                 "rail_fence line happens inside one of these segments "
                 "(validator-enforced; Gate P5's no-untraced-crossing rule).")
    lines.append("")
    lines.append("| segment | unit | action | t | obstacles |")
    lines.append("|---|---|---|---|---|")
    for u in bundle["units"]:
        for s in u["segments"]:
            if s["obstacleIds"]:
                lines.append(f"| {s['id']} | {u['unitId']} | {s['action']} | "
                             f"{s['t0']}-{s['t1']} | {', '.join(s['obstacleIds'])} |")
    lines.append("")

    lines.append("## Strength reconciliation vs the macro battle file")
    lines.append("")
    lines.append("Compiled strength at every macro keyframe time inside the slice (and at "
                 "the slice edges, where the macro value is linear interpolation) — "
                 "tolerance +/-1 man (validator-enforced).")
    lines.append("")
    lines.append("| unit | t | macro | compiled |")
    lines.append("|---|---|---|---|")
    battle_units = {u["id"]: u for u in corpus.battle["units"]}
    profiles_by_unit: dict[str, list] = {}
    for p in recon["casualtyProfiles"]:
        profiles_by_unit.setdefault(p["unitId"], []).append(p)
    for u in recon["units"]:
        uid = u["unitId"]
        kfs = battle_units[uid]["keyframes"]
        profs = sorted(profiles_by_unit.get(uid, []), key=lambda p: p["t0"])
        times = [t0] + [kf["t"] for kf in kfs if t0 < kf["t"] < t1] + [t1]
        for t in times:
            macro = macro_strength_at(kfs, t)
            got = compiled_strength(u["startStrength"]["value"], profs, t)
            lines.append(f"| {uid} | {t} | {macro:.1f} | {got} |")
    lines.append("")

    lines.append("## Casualty totals within the slice")
    lines.append("")
    lines.append("| unit | start | slice losses | end |")
    lines.append("|---|---|---|---|")
    for u in bundle["units"]:
        losses = sum(p["count"] for p in u["casualtyProfiles"])
        lines.append(f"| {u['unitId']} | {u['startStrength']} | {losses} | "
                     f"{u['startStrength'] - losses} |")
    lines.append("")
    return "\n".join(lines) + "\n"


# ---------------------------------------------------------------------------

def main(argv: list[str]) -> int:
    corpus = load_corpus()
    errors = validate_corpus(corpus)
    if errors:
        for e in errors:
            print(e, file=sys.stderr)
        print(f"validation failed with {len(errors)} error(s); nothing written",
              file=sys.stderr)
        return 1

    bundle = compile_bundle(corpus)
    BUNDLE_PATH.parent.mkdir(parents=True, exist_ok=True)
    BUNDLE_PATH.write_text(dump_bundle(bundle))
    AUDIT_PATH.write_text(build_audit(corpus, bundle))
    size_kb = BUNDLE_PATH.stat().st_size / 1024
    print(f"OK: wrote {BUNDLE_PATH.relative_to(REPO)} ({size_kb:.0f} KiB, "
          f"checksum {bundle['checksum'][:12]}…) and {AUDIT_PATH.relative_to(REPO)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
