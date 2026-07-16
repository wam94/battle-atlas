"""Compile the Iverson's-field (Oak Ridge, July 1) tactical bundle.

Same contract as compile_angle.py, over the July 1 corpus: validates via
validate_iverson.py, then emits

- app/Assets/Battle/Iverson/iverson.bundle.json — deterministic per-second
  unit state for t=5820..7050 on the July 1 afternoon clock (startTime
  46800), for the second Soldier View film's staging;
- docs/reconstruction/iverson-bundle-audit.md — the human-readable audit.

Byte-deterministic (sorted keys, fixed formatting, embedded checksum).
The Angle corpus and bundle are UNTOUCHED (film-safety, ED-21).

Usage: uv run python scripts/compile_iverson.py
"""

from __future__ import annotations

import json
import sys
from collections import Counter
from pathlib import Path

import validate_iverson as vi
import validate_reconstruction as vr
from compile_angle import (
    _ease, _lerp_bearing, _route_bearing, _route_point, _sha256_file,
    bundle_checksum, dump_bundle, segment_provenance)

REPO = Path(__file__).resolve().parent.parent.parent
BUNDLE_PATH = REPO / "app/Assets/Battle/Iverson/iverson.bundle.json"
AUDIT_PATH = REPO / "docs/reconstruction/iverson-bundle-audit.md"

BUNDLE_FORMAT = "angle-bundle/1"  # same schema version; site-specific path

# ED-21 discipline for THIS bundle: the staging seed for every hash-drawn
# decision (victim draws, step phase, yaw, waver) is a PINNED literal token,
# so recompiles (including provenance-only ones) never re-roll the proof
# choreography the owner reviews at the gate. The OWNER GATE re-pins this
# (to the reviewed bundle's checksum, the Angle ED-21 convention) before
# the production render is authorized.
STAGING_SEED = "iverson-proof-seed/1"

# Fight-prone wiring (T5 vocabulary gap #1, CLOSED by the fight-prone-vocab
# slice): a fire segment that carries claim-iv-lying-down ("my line of
# battle still lying down in position", or-27-2-iverson) compiles to the
# `fight_prone` action class — the resolver's go-prone / prone fire /
# roll-to-load / prone-idle cycle — instead of the standing fire cycle.
# The canonical corpus keeps the semantic fire action + the claim tag;
# this rule is the single, declarative place the attestation becomes
# choreography. The 12th NC's fight segment does NOT carry the claim
# (the record pins the lying line to the destroyed left) and stays a
# standing/kneeling fire_independent.
PRONE_CLAIM = "claim-iv-lying-down"
_PRONE_MAPPABLE = {"fire_independent", "fire_by_rank"}


def bundle_action(seg: dict) -> str:
    if PRONE_CLAIM in seg.get("claimIds", []) \
            and seg["action"] in _PRONE_MAPPABLE:
        return "fight_prone"
    return seg["action"]


def compile_bundle(corpus: vr.Corpus) -> dict:
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

        xs, zs, facings, strengths, seg_index = [], [], [], [], []
        prev_facing = None
        for t in seconds:
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
                    bearing = prev_facing if prev_facing is not None \
                        else bu["keyframes"][0]["facing"]
                facing = bearing
            prev_facing = facing

            xs.append(round(x, 1))
            zs.append(round(z, 1))
            facings.append(round(facing, 1))
            strengths.append(vr.compiled_strength(start, profs, t))
            seg_index.append(si)

        units_out.append({
            "unitId": uid,
            "name": bu["name"],
            "side": bu["side"],
            "arm": u["arm"],
            "startStrength": start,
            "segments": [
                {
                    "id": s["id"],
                    "t0": s["t0"],
                    "t1": s["t1"],
                    "action": bundle_action(s),
                    "provenance": segment_provenance(s, claim_by_id),
                    "formationFrom": s["formationFrom"],
                    "formationTo": s["formationTo"],
                    "paceProfile": s["paceProfile"],
                    "route": s["route"],
                    "obstacleIds": s.get("obstacleIds", []),
                    "claimIds": s["claimIds"],
                    "inferenceRules": s["inferenceRules"],
                }
                for s in segs
            ],
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
                "strength": strengths,
                "segmentIndex": seg_index,
            },
        })

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
        "note": "Compiled tactical artifact for the Iverson's-field slice "
                "(Oak Ridge, July 1 afternoon; second Soldier View film, "
                "PROOF stage). Per-second state traces to semantic segments; "
                "segments trace to atomic claims and/or named inference rules "
                "(docs/reconstruction/iverson-viewpoint-design.md). Fire "
                "segments attesting the lying-down fight (claim-iv-lying-down) "
                "compile to the fight_prone action class "
                "(docs/reconstruction/fight-prone-vocab.md). The Angle "
                "bundle is untouched.",
        "slice": {"t0": t0, "t1": t1},
        "clock": {"startTimeSecondsSinceMidnight": corpus.battle["startTime"]},
        "inputs": {
            "sources": _sha256_file(vi.SOURCES_PATH),
            "claims": _sha256_file(vi.CLAIMS_PATH),
            "reconstruction": _sha256_file(vi.RECON_PATH),
            "landcover": _sha256_file(vi.LANDCOVER_PATH),
            "battle": _sha256_file(vi.BATTLE_PATH),
        },
        "claimsIndex": claims_index,
        "units": units_out,
    }
    payload["checksum"] = bundle_checksum(payload)
    return payload


def build_audit(corpus: vr.Corpus, bundle: dict) -> str:
    recon = corpus.recon
    claims = corpus.claims["claims"]
    t0, t1 = bundle["slice"]["t0"], bundle["slice"]["t1"]

    lines = []
    lines.append("# Iverson Bundle Audit (second Soldier View film — proof stage)")
    lines.append("")
    lines.append("Generated by `reconstruction/scripts/compile_iverson.py`; regenerate with "
                 "`uv run python scripts/compile_iverson.py` from `reconstruction/`. "
                 "Deterministic — no timestamps; byte-identical for identical inputs.")
    lines.append("")
    lines.append(f"- Slice: t={t0}..{t1} on the July 1 AFTERNOON clock "
                 f"(startTime 46800 = 13:00 LMT; the slice is 14:37-14:57) — "
                 f"inside CA-J1P-2's adopted 14:30-15:00 destruction bracket")
    lines.append(f"- Sources: {len(corpus.sources['sources'])}  |  Claims: {len(claims)}")
    lines.append(f"- Units: {len(recon['units'])}  |  Segments: {len(recon['segments'])}  |  "
                 f"Casualty profiles: {len(recon['casualtyProfiles'])}")
    lines.append(f"- Bundle checksum: `{bundle['checksum']}`")
    lines.append(f"- Staging seed (proof pin): `{bundle['stagingSeed']}` — the owner gate "
                 f"re-pins per ED-21 before the production render")
    lines.append("")

    lines.append("## Compiled-second provenance")
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

    lines.append("## Decomposition reconciliation (regiment sum vs csa-iverson macro track)")
    lines.append("")
    lines.append("| t | macro (brigade) | compiled regiment sum |")
    lines.append("|---|---|---|")
    parent = next(u for u in corpus.battle["units"] if u["id"] == vi.PARENT_ID)
    strengths = vi._compiled_regiment_strengths(recon)
    times = [t0] + [kf["t"] for kf in parent["keyframes"] if t0 < kf["t"] < t1] + [t1]
    for t in times:
        got = sum(vr.compiled_strength(s, p, t) for s, p in strengths.values())
        want = vr.macro_strength_at(parent["keyframes"], t)
        lines.append(f"| {t} | {want:.1f} | {got} |")
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
    lines.append("The brigade loses 525 on the halted line (t>=6000) inside the slice — "
                 "the '500 ... on a line as straight as a dress parade' exemplar "
                 "(claim-iv-death-line); the 12th NC stays light (claim-iv-return-rows). "
                 "The ~308-man capture mass (claim-iv-surrender-mass) is PAST the slice "
                 "end by design: surrender is not representable with the current "
                 "action/clip vocabulary (named T5 gap #2).")
    lines.append("")

    lines.append("## Fight-prone wiring (claim-iv-lying-down — T5 gap #1 closed)")
    lines.append("")
    lines.append("Fire segments carrying `claim-iv-lying-down` compile to the "
                 "`fight_prone` action class (go-prone under fire, prone fire "
                 "cycle with the roll-to-load compromise, prone deaths that "
                 "settle where the man lay). The 12th NC keeps the standing "
                 "`fire_independent` — the record pins the lying line to the "
                 "destroyed left, not the survivor regiment.")
    lines.append("")
    lines.append("| segment | corpus action | compiled action |")
    lines.append("|---|---|---|")
    for u in bundle["units"]:
        for s in u["segments"]:
            src = next(x for x in recon["segments"] if x["id"] == s["id"])
            if s["action"] != src["action"] or s["action"] == "fight_prone":
                lines.append(f"| {s['id']} | {src['action']} | {s['action']} |")
    lines.append("")
    return "\n".join(lines) + "\n"


def main(argv: list[str]) -> int:
    corpus = vi.load_corpus()
    errors = vi.validate_iverson(corpus)
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
