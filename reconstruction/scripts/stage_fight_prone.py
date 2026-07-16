"""Stage the fight-prone vocabulary demo bundle (gate evidence, NOT history).

Emits a small angle-bundle/1 artifact that exercises the full prone arc in
one 30 s window — a Confederate line standing under fire, going prone
(staggered Go_Prone), fighting prone (Fight_Prone_Fire + the roll-to-load
Fight_Prone_Reload + Prone_Idle), taking prone casualties that settle where
they lay, and rising (Rise_From_Prone) — against a Union line firing
throughout. Staged on the Oak Ridge crop at the real Iverson/Baxter fight
geometry so terrain and ranges read true.

THIS IS A STAGED CHOREOGRAPHY DEMO for the P6-style vocabulary gate: it is
not a reconstruction, traces to no claims, and lives with the evidence
captures (docs/benchmarks/captures/fight-prone/, gitignored) — regenerate
with `uv run python scripts/stage_fight_prone.py`. Byte-deterministic.
"""

from __future__ import annotations

from pathlib import Path

import validate_reconstruction as vr
from compile_angle import bundle_checksum, dump_bundle

REPO = Path(__file__).resolve().parent.parent.parent
OUT = REPO / "docs/benchmarks/captures/fight-prone/fight-prone-demo.bundle.json"

STAGING_SEED = "fight-prone-demo-seed/1"
T0, T1 = 0, 40

# The real t=6600 fight geometry from the Iverson bundle (csa-20nc /
# us-baxter), so the demo stands in the actual swale at the actual range.
CSA_POS = (4007.4, 8517.9)
CSA_FACING = 155.0
US_POS = (4008.4, 8420.2)
US_FACING = 340.0


def unit(unit_id, name, side, pos, facing, start, segments, profiles):
    seconds = list(range(T0, T1 + 1))
    seg_index = []
    for t in seconds:
        si = len(segments) - 1
        for i, s in enumerate(segments):
            if s["t0"] <= t < s["t1"]:
                si = i
                break
        seg_index.append(si)
    return {
        "unitId": unit_id,
        "name": name,
        "side": side,
        "arm": "infantry",
        "startStrength": start,
        "segments": segments,
        "casualtyProfiles": profiles,
        "perSecond": {
            "t0": T0,
            "x": [pos[0]] * len(seconds),
            "z": [pos[1]] * len(seconds),
            "facingDeg": [facing] * len(seconds),
            "strength": [vr.compiled_strength(start, profiles, t)
                         for t in seconds],
            "segmentIndex": seg_index,
        },
    }


def seg(seg_id, t0, t1, action):
    return {
        "id": seg_id,
        "t0": t0,
        "t1": t1,
        "action": action,
        "provenance": "editorial",
        "formationFrom": "line",
        "formationTo": "line",
        "paceProfile": "static",
        "route": [],
        "obstacleIds": [],
        "claimIds": [],
        "inferenceRules": ["fight-prone-demo-staging"],
    }


def build() -> dict:
    csa_profiles = [
        {
            "id": "cas-demo-prone",
            "t0": 9.0, "t1": 23.0, "count": 9,
            "intensityCurve": "uniform",
            "causeMix": {"musketry": 1.0, "canister": 0.0,
                         "shell": 0.0, "unknown": 0.0},
            "assessment": "editorial",
            "claimIds": [],
        },
        {
            "id": "cas-demo-standing",
            "t0": 28.0, "t1": 38.0, "count": 2,
            "intensityCurve": "uniform",
            "causeMix": {"musketry": 1.0, "canister": 0.0,
                         "shell": 0.0, "unknown": 0.0},
            "assessment": "editorial",
            "claimIds": [],
        },
    ]
    payload = {
        "format": "angle-bundle/1",
        "stagingSeed": STAGING_SEED,
        "note": "FIGHT-PRONE VOCABULARY DEMO (gate evidence). Staged "
                "choreography exercising go_prone / fight_prone / "
                "rise_from_prone on the Oak Ridge crop at the real "
                "Iverson-Baxter fight geometry. NOT A RECONSTRUCTION: "
                "editorial staging, no claims, never shipped in a film. "
                "Regenerate: uv run python scripts/stage_fight_prone.py",
        "slice": {"t0": T0, "t1": T1},
        "clock": {"startTimeSecondsSinceMidnight": 46800 + 6600},
        "inputs": {},
        "claimsIndex": {},
        "units": [
            unit("demo-csa-line", "Demo line (fight-prone vocabulary)",
                 "confederate", CSA_POS, CSA_FACING, 120,
                 [seg("seg-demo-hold", 0, 6, "hold"),
                  seg("seg-demo-prone", 6, 24, "fight_prone"),
                  seg("seg-demo-recover", 24, 40, "hold")],
                 csa_profiles),
            unit("demo-us-line", "Demo opposing line", "union",
                 US_POS, US_FACING, 160,
                 [seg("seg-demo-us-fire", 0, 40, "fire_independent")],
                 []),
        ],
    }
    payload["checksum"] = bundle_checksum(payload)
    return payload


def main() -> int:
    OUT.parent.mkdir(parents=True, exist_ok=True)
    bundle = build()
    OUT.write_text(dump_bundle(bundle))
    print(f"OK: wrote {OUT.relative_to(REPO)} "
          f"(checksum {bundle['checksum'][:12]}…)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
