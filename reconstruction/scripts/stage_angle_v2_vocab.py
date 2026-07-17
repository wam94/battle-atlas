"""Stage the Angle-v2 vocabulary demo bundle (gate evidence, NOT history).

Emits a small angle-bundle/1 artifact that exercises the four Angle-v2
vocabulary sets in one 40 s window ON THE REAL ANGLE GEOMETRY (the
default Angle crop; obstacle ids reference the sourced environment
bake's traced features):

  P3 melee        two lines locked at the stone wall
                  (wall-angle-webb-front): grapple pairs, clubbed-musket
                  swings, bayonet thrusts, parries;
  P4 colors       a color party (colorParty=6) under heavy fire 40 m
                  west of the wall: bearer falls, colors grounded,
                  taken up by the next man;
  P5 mounted      the render harness attaches a mounted officer to the
                  colors line (spec constants in
                  AngleV2VocabGateRender.cs — the bundle carries no
                  officer record);
  P6 halt-fire    a line advancing on the post-and-rail fence west of
                  the Emmitsburg road halts AT it and fires
                  (halt_fire_obstacle), then crosses.

THIS IS A STAGED CHOREOGRAPHY DEMO for the P6-style vocabulary gate: it
is not a reconstruction, traces to no claims, and lives with the
evidence captures (docs/benchmarks/captures/angle-v2-vocab/) —
regenerate with `uv run python scripts/stage_angle_v2_vocab.py`.
Byte-deterministic.
"""

from __future__ import annotations

from pathlib import Path

import validate_reconstruction as vr
from compile_angle import bundle_checksum, dump_bundle

REPO = Path(__file__).resolve().parent.parent.parent
OUT = (REPO / "docs/benchmarks/captures/angle-v2-vocab/"
       "angle-v2-vocab-demo.bundle.json")

STAGING_SEED = "angle-v2-vocab-demo-seed/1"
T0, T1 = 0, 40

# Real Angle geometry (data/heightmap_angle/environment.json):
#   the stone wall wall-angle-webb-front passes (4399, 4867);
#   fence-post-and-rail-west-of-road passes (4143.2, 4900.5) bearing
#   ~21 deg (normal ~111 deg east).
WALL_CSA = (4398.0, 4867.0)
WALL_US = (4406.0, 4869.0)
COLORS_POS = (4358.0, 4867.0)
CSA_FACING = 79.0
US_FACING = 262.0

FENCE_STAND = (4139.0, 4902.1)     # 4.5 m west of the traced fence
FENCE_START = (4127.8, 4906.4)     # 12 m back along the advance
FENCE_BEYOND = (4148.3, 4898.5)    # 10 m past the fence line
FENCE_FACING = 111.0
ROAD_US_POS = (4194.0, 4881.0)
ROAD_US_FACING = 291.0


def track(waypoints, seconds):
    """Per-second positions linearly interpolated between (t, x, z)."""
    xs, zs = [], []
    for t in seconds:
        x, z = waypoints[-1][1], waypoints[-1][2]
        for k in range(len(waypoints) - 1):
            ta, xa, za = waypoints[k]
            tb, xb, zb = waypoints[k + 1]
            if t <= tb:
                f = 0.0 if tb == ta else max(0.0, min(1.0, (t - ta) / (tb - ta)))
                x = xa + (xb - xa) * f
                z = za + (zb - za) * f
                break
        xs.append(round(x, 3))
        zs.append(round(z, 3))
    return xs, zs


def unit(unit_id, name, side, facing, start, segments, profiles,
         waypoints=None, pos=None, color_party=0):
    seconds = list(range(T0, T1 + 1))
    if waypoints is None:
        waypoints = [(T0, pos[0], pos[1]), (T1, pos[0], pos[1])]
    xs, zs = track(waypoints, seconds)
    seg_index = []
    for t in seconds:
        si = len(segments) - 1
        for i, s in enumerate(segments):
            if s["t0"] <= t < s["t1"]:
                si = i
                break
        seg_index.append(si)
    u = {
        "unitId": unit_id,
        "name": name,
        "side": side,
        "arm": "infantry",
        "startStrength": start,
        "segments": segments,
        "casualtyProfiles": profiles,
        "perSecond": {
            "t0": T0,
            "x": xs,
            "z": zs,
            "facingDeg": [facing] * len(seconds),
            "strength": [vr.compiled_strength(start, profiles, t)
                         for t in seconds],
            "segmentIndex": seg_index,
        },
    }
    if color_party:
        u["colorParty"] = color_party
    return u


def seg(seg_id, t0, t1, action, opponent=None, obstacles=None):
    s = {
        "id": seg_id,
        "t0": t0,
        "t1": t1,
        "action": action,
        "provenance": "editorial",
        "formationFrom": "line",
        "formationTo": "line",
        "paceProfile": "static",
        "route": [],
        "obstacleIds": obstacles or [],
        "claimIds": [],
        "inferenceRules": ["angle-v2-vocab-demo-staging"],
    }
    if opponent:
        s["meleeOpponentId"] = opponent
    return s


def profile(pid, t0, t1, count):
    return {
        "id": pid,
        "t0": float(t0), "t1": float(t1), "count": count,
        "intensityCurve": "uniform",
        "causeMix": {"musketry": 1.0, "canister": 0.0,
                     "shell": 0.0, "unknown": 0.0},
        "assessment": "editorial",
        "claimIds": [],
    }


def build() -> dict:
    units = [
        # P3: the wall fight
        unit("demo-csa-breach", "Demo breach line (melee)", "confederate",
             CSA_FACING, 90,
             [seg("seg-melee-csa", T0, T1, "melee",
                  opponent="demo-us-wall")],
             [profile("cas-melee-csa", 8, 30, 9)],
             pos=WALL_CSA),
        unit("demo-us-wall", "Demo wall line (melee)", "union",
             US_FACING, 70,
             [seg("seg-melee-us", T0, T1, "melee",
                  opponent="demo-csa-breach")],
             [profile("cas-melee-us", 8, 30, 5)],
             pos=WALL_US),
        # P4: the colors under fire (heavy window so the chain walks
        # on camera)
        unit("demo-csa-colors", "Demo color party line", "confederate",
             CSA_FACING, 80,
             [seg("seg-colors-hold", T0, T1, "hold")],
             [profile("cas-colors", 5, 24, 62)],
             pos=COLORS_POS, color_party=6),
        # P6: halt-and-fire at the traced post-and-rail fence
        unit("demo-csa-haltfire", "Demo halt-and-fire line", "confederate",
             FENCE_FACING, 80,
             [seg("seg-hf-adv", T0, 10, "advance"),
              seg("seg-hf-halt", 10, 34, "halt_fire_obstacle",
                  obstacles=["fence-post-and-rail-west-of-road"]),
              seg("seg-hf-cross", 34, T1, "cross_obstacle",
                  obstacles=["fence-post-and-rail-west-of-road"])],
             [profile("cas-hf", 14, 32, 5)],
             waypoints=[(T0, *FENCE_START), (10, *FENCE_STAND),
                        (34, *FENCE_STAND), (T1, *FENCE_BEYOND)]),
        # the opposing fire beyond the road (incoming-fire context for
        # the halt-and-fire line)
        unit("demo-us-road", "Demo opposing line (road)", "union",
             ROAD_US_FACING, 60,
             [seg("seg-road-fire", T0, T1, "fire_independent")],
             [],
             pos=ROAD_US_POS),
    ]
    payload = {
        "format": "angle-bundle/1",
        "stagingSeed": STAGING_SEED,
        "note": "ANGLE-V2 VOCABULARY DEMO (gate evidence). Staged "
                "choreography exercising melee / colors succession / "
                "halt_fire_obstacle on the real Angle crop geometry "
                "(the mounted officer is harness-staged). NOT A "
                "RECONSTRUCTION: editorial staging, no claims, never "
                "shipped in a film. Regenerate: uv run python "
                "scripts/stage_angle_v2_vocab.py",
        "slice": {"t0": T0, "t1": T1},
        "clock": {"startTimeSecondsSinceMidnight": 46800 + 8600},
        "inputs": {},
        "claimsIndex": {},
        "units": units,
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
