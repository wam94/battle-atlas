"""Validate the Iverson's-field (Oak Ridge, July 1) Soldier View corpus.

Reuses validate_reconstruction.py's machinery over the July 1 corpus files
(sources/july1.sources.json, claims/iverson.claims.json,
canonical/iverson.reconstruction.json, data/landcover/oakridge.landcover.json,
app/Assets/Battle/gettysburg-july1-afternoon.json) — all SEPARATE from the
Angle corpus by design: the committed angle.bundle.json embeds sha256s of the
Angle input files, so this slice must not touch them (film-safety, ED-21).

Regimental decomposition: the slice decomposes csa-iverson into its four
regiments (csa-5nc/csa-20nc/csa-23nc/csa-12nc), which do NOT exist in the
July 1 phase file (the Atlas stays brigade-grain; promoting the regiments to
macro units is proposed follow-up scope). Validation therefore:

- extends the battle-unit vocabulary with the regiment declarations below,
  synthesizing battle entries whose keyframes carry the reconstruction's own
  compiled strengths (self-consistent by construction — the per-regiment
  check adds no information and exists only to keep the shared validator's
  per-unit machinery engaged);
- adds the check that DOES carry the honesty load: at every csa-iverson
  macro keyframe inside the slice and at both slice edges, the SUM of the
  compiled regimental strengths must equal the brigade's macro track within
  +/-1 man (the same tolerance the Angle slice uses per unit).

Usage: uv run python scripts/validate_iverson.py
Exit 0 when valid; exit 1 with one error per line otherwise.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

import validate_reconstruction as vr

REPO = Path(__file__).resolve().parent.parent.parent

SOURCES_PATH = REPO / "reconstruction/sources/july1.sources.json"
CLAIMS_PATH = REPO / "reconstruction/claims/iverson.claims.json"
RECON_PATH = REPO / "reconstruction/canonical/iverson.reconstruction.json"
LANDCOVER_PATH = REPO / "data/landcover/oakridge.landcover.json"
BATTLE_PATH = REPO / "app/Assets/Battle/gettysburg-july1-afternoon.json"

# The tactical decomposition of csa-iverson (EDI-5: regimental order
# left-to-right 5th-20th-23rd-12th from the j1-09 bar's drawn numbers +
# the 12th-intact record; offsets are meters along the brigade line's
# RIGHT vector, facing 155).
REGIMENTS = {
    "csa-5nc": {
        "name": "5th North Carolina, Iverson's Brigade",
        "side": "confederate", "parent": "csa-iverson", "offsetRightM": -120.0,
    },
    "csa-20nc": {
        "name": "20th North Carolina, Iverson's Brigade",
        "side": "confederate", "parent": "csa-iverson", "offsetRightM": -40.0,
    },
    "csa-23nc": {
        "name": "23rd North Carolina, Iverson's Brigade",
        "side": "confederate", "parent": "csa-iverson", "offsetRightM": 40.0,
    },
    "csa-12nc": {
        "name": "12th North Carolina, Iverson's Brigade",
        "side": "confederate", "parent": "csa-iverson", "offsetRightM": 120.0,
    },
}

PARENT_ID = "csa-iverson"
STRENGTH_TOLERANCE = 1.0

# The shared validator's battlefield extent (8507.2 m) is the MACRO HEIGHTMAP
# tile, pinned for the July 3 corpus. The battlefield-local frame itself
# extends past it: the July 1 phase file's own canon places Oak Hill at
# z=8742 (mon-iverson-hq, csa-iverson t=0). The Oak Ridge slice routes live
# in z=8300..8730, cut from the same 1 m DEM cache (which covers the area);
# scope the extent up for THIS corpus only, restored after validation.
OAKRIDGE_EXTENT_M = 8800.0


def _compiled_regiment_strengths(recon: dict) -> dict[str, tuple[int, list]]:
    """unitId -> (startStrength, sorted profiles) for the regiment units."""
    profiles: dict[str, list] = {uid: [] for uid in REGIMENTS}
    for p in recon["casualtyProfiles"]:
        if p["unitId"] in profiles:
            profiles[p["unitId"]].append(p)
    out = {}
    for u in recon["units"]:
        if u["unitId"] in REGIMENTS:
            out[u["unitId"]] = (
                u["startStrength"]["value"],
                sorted(profiles[u["unitId"]], key=lambda p: p["t0"]),
            )
    return out


def _synthetic_battle(battle: dict, recon: dict) -> dict:
    """The July 1 battle dict + synthetic regiment entries.

    Keyframe strengths are the reconstruction's own compiled values (see
    module docstring); positions ride the recon segment routes' endpoints.
    """
    t0, t1 = recon["slice"]["t0"], recon["slice"]["t1"]
    parent = next(u for u in battle["units"] if u["id"] == PARENT_ID)
    times = [t0] + [kf["t"] for kf in parent["keyframes"] if t0 < kf["t"] < t1] + [t1]

    segs_by_unit: dict[str, list] = {uid: [] for uid in REGIMENTS}
    for s in recon["segments"]:
        if s["unitId"] in segs_by_unit:
            segs_by_unit[s["unitId"]].append(s)

    strengths = _compiled_regiment_strengths(recon)
    battle = json.loads(json.dumps(battle))  # deep copy; never mutate input
    for uid, decl in REGIMENTS.items():
        start, profs = strengths[uid]
        kfs = []
        for t in times:
            segs = sorted(segs_by_unit[uid], key=lambda s: s["t0"])
            pos = segs[0]["route"][0]
            for s in segs:
                if s["t0"] <= t:
                    frac = 1.0 if t >= s["t1"] else (t - s["t0"]) / (s["t1"] - s["t0"])
                    # endpoint-grade position is enough for a strength-only
                    # synthetic keyframe; use the nearer route endpoint
                    pos = s["route"][-1] if frac >= 0.5 else s["route"][0]
            kfs.append({
                "t": t, "x": pos[0], "z": pos[1], "facing": 155,
                "formation": "line",
                "strength": vr.compiled_strength(start, profs, t),
                "confidence": "documented",
                "citation": "synthetic decomposition keyframe (validate_iverson.py; "
                            "R-iv-regiment-split / R-iv-regiment-apportionment)",
            })
        battle["units"].append({
            "id": uid,
            "name": decl["name"],
            "side": decl["side"],
            "parent": decl["parent"],
            "frontage_m": 80,
            "depth_m": 5,
            "keyframes": kfs,
        })
    return battle


def load_corpus() -> vr.Corpus:
    sources = json.loads(SOURCES_PATH.read_text())
    claims = json.loads(CLAIMS_PATH.read_text())
    recon = json.loads(RECON_PATH.read_text())
    landcover = json.loads(LANDCOVER_PATH.read_text())
    battle = json.loads(BATTLE_PATH.read_text())
    battle = _synthetic_battle(battle, recon)
    all_ids = vr.manifest_battle_unit_ids() | frozenset(REGIMENTS)
    return vr.Corpus(
        sources=sources, claims=claims, recon=recon,
        landcover=landcover, battle=battle,
        all_battle_unit_ids=all_ids,
    )


def validate_iverson(corpus: vr.Corpus) -> list[str]:
    prev_extent = vr.BATTLEFIELD_EXTENT_M
    vr.BATTLEFIELD_EXTENT_M = OAKRIDGE_EXTENT_M
    try:
        errors = vr.validate_corpus(corpus)
    finally:
        vr.BATTLEFIELD_EXTENT_M = prev_extent

    # The load-bearing decomposition check: regiment sums vs the brigade
    # macro track at every in-slice brigade keyframe and both slice edges.
    recon = corpus.recon
    t0, t1 = recon["slice"]["t0"], recon["slice"]["t1"]
    parent = next(u for u in corpus.battle["units"] if u["id"] == PARENT_ID)
    times = [t0] + [kf["t"] for kf in parent["keyframes"] if t0 < kf["t"] < t1] + [t1]
    strengths = _compiled_regiment_strengths(recon)
    if len(strengths) != len(REGIMENTS):
        errors.append("iverson: reconstruction must declare all four regiments")
        return errors
    for t in times:
        got = sum(vr.compiled_strength(s, p, t) for s, p in strengths.values())
        want = vr.macro_strength_at(parent["keyframes"], t)
        if abs(got - want) > STRENGTH_TOLERANCE:
            errors.append(
                f"decomposition[{PARENT_ID}]: regiment sum {got} at t={t} deviates "
                f"from the brigade macro track {want:.1f} by more than "
                f"{STRENGTH_TOLERANCE}")
    return errors


def main(argv: list[str]) -> int:
    corpus = load_corpus()
    errors = validate_iverson(corpus)
    for e in errors:
        print(e, file=sys.stderr)
    if not errors:
        print(f"OK: iverson corpus valid ({len(corpus.claims['claims'])} claims, "
              f"{len(corpus.recon['segments'])} segments, "
              f"{len(corpus.recon['casualtyProfiles'])} profiles)")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
