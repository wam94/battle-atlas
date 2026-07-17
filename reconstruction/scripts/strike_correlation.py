"""Angle-v2 P2 (proposed ED-82): strike-correlated casualty fall times.

Replicates, bit-faithfully where it matters, the C# resolver's compiled
strike pipeline (FireCycles.CompileCannon shot schedule -> per-shot
nearest-enemy-infantry targeting -> impact at shot.t + 0.4 s;
AngleActionContext.CompileEngagements) and the casualty compiler's
cause apportionment (CasualtySchedule.ApportionCauses), so that the
Angle compiler can re-time a profile's canister/shell-cause victims to
cluster at the SAME StrikeEvents the renderer stages strike dust and
flinch/brace reactions from.

The redistribution rule (proposed ED-82, angle-editorial-decisions.md):

  * only profiles carrying an explicit, cited `strikeCorrelation` block
    participate (the cited fraction is the profile's own causeMix share
    of the named strike classes — aggregate evidence, V&R paragraph 2);
  * only victims whose apportioned cause class is in
    strikeCorrelation.classes move; musketry/unknown victims keep the
    smooth inverse-CDF curve;
  * a moved victim snaps to the nearest compiled strike within
    strikeCorrelation.windowS seconds of his smooth draw (preserving
    the cited intensity shape in the aggregate) and within the
    profile's own [t0, t1] evidence bounds;
  * at most strikeCorrelation.maxPerStrike victims cluster on one
    strike (Peyton: "sometimes as many as 10 men ... by the bursting
    of a single shell" — claim-cas-shell-clusters); a small
    deterministic stagger (0.25 + 0.12 j seconds) spreads the cluster
    over ~a second so the burst reads as one arrival, not one frame;
  * victim IDENTITY, cause, wounded-crawl draw, per-profile counts and
    windows are UNCHANGED — only fall TIMES move (the totals-by-
    boundary invariant is preserved exactly).

Emitted fall times are quantized to 1/64 s so their float32 parse in
Unity is exact; the bundle's per-second strength for a correlated
profile counts these times directly, so the schedule reconciles with
the compiled strength EXACTLY (an upgrade over the smooth profiles'
documented within-one-man rounding property).

All arithmetic that the C# side also performs (shot-time accumulation,
position lerps, distances) uses numpy float32 to match Mathf.
"""

from __future__ import annotations

import math

import numpy as np

F = np.float32

CANNON_INTERVAL = F(16.0)
CANNON_JITTER = F(5.0)
STRIKE_DELAY = F(0.4)
STRIKE_MAX_RANGE = 650.0
GUNS_PER_BATTERY = 6

CAUSE_CLASSES = ("musketry", "canister", "shell", "unknown")

# ---------------------------------------------------------------------------
# AngleEnvironmentLayout.Hash01 (FNV-1a over UTF-16 chars, murmur finalizer)


def hash01(key: str, index: int) -> np.float32:
    h = 2166136261
    for c in key:
        h = ((h ^ ord(c)) * 16777619) & 0xFFFFFFFF
    h = ((h ^ (index & 0xFFFFFFFF)) * 16777619) & 0xFFFFFFFF
    h ^= h >> 13
    h = (h * 0x5BD1E995) & 0xFFFFFFFF
    h ^= h >> 15
    return F(h & 0xFFFFFF) / F(0xFFFFFF)


# ---------------------------------------------------------------------------
# CasualtySchedule.InvCdf / ApportionCauses (float32-faithful)


def inv_cdf(curve: str, p: float) -> np.float32:
    p = F(min(max(p, 0.0), 1.0))
    if curve == "uniform":
        return p
    if curve == "rising":
        return F(np.sqrt(p, dtype=F))
    if curve == "falling":
        return F(1.0) - F(np.sqrt(F(1.0) - p, dtype=F))
    if curve == "spike":
        return F(0.5) - F(np.sin(F(np.arcsin(F(1.0) - F(2.0) * p, dtype=F) / F(3.0)), dtype=F))
    raise ValueError(f"unknown intensityCurve {curve!r}")


def apportion_causes(profile_id: str, count: int, cause_mix: dict) -> list[str]:
    """Cause class per victim k=0..count-1 — the exact largest-remainder +
    hash-shuffle the C# CasualtySchedule.ApportionCauses computes (the
    permutation key is seed-INDEPENDENT: p.id + "|causeperm")."""
    mix = [F(cause_mix.get(c, 0.0)) for c in CAUSE_CLASSES]
    total = mix[0] + mix[1] + mix[2] + mix[3]
    if total <= 0:
        mix[3] = F(1.0)
        total = F(1.0)
    counts = [0, 0, 0, 0]
    rem = [F(0)] * 4
    assigned = 0
    for c in range(4):
        exact = F(count) * mix[c] / total
        counts[c] = int(exact)
        rem[c] = exact - F(counts[c])
        assigned += counts[c]
    while assigned < count:
        best = 0
        for c in range(1, 4):
            if rem[c] > rem[best]:
                best = c
        counts[best] += 1
        rem[best] = F(-1.0)
        assigned += 1
    causes: list[str] = []
    for c in range(4):
        causes.extend([CAUSE_CLASSES[c]] * counts[c])
    key = profile_id + "|causeperm"
    perm = sorted(range(count), key=lambda i: (float(hash01(key, i)), i))
    return [causes[perm[i]] for i in range(count)]


# ---------------------------------------------------------------------------
# FireCycles.CompileCannon + CompileEngagements strike targeting


def _lerp32(a: float, b: float, f: np.float32) -> np.float32:
    return F(a) + (F(b) - F(a)) * f


def _position_at(per_second: dict, t: np.float32) -> tuple[np.float32, np.float32]:
    """AngleBundleUnit.PositionAt over the compiled per-second tables
    (float32 lerp of the rounded values, matching Mathf.Lerp)."""
    xs = per_second["x"]
    zs = per_second["z"]
    n = len(xs)
    ft = F(min(max(float(t) - per_second["t0"], 0.0), n - 1))
    i = min(int(ft), n - 2)
    frac = ft - F(i)
    return _lerp32(xs[i], xs[i + 1], frac), _lerp32(zs[i], zs[i + 1], frac)


def compile_cannon(seed: str, unit_id: str, segments: list) -> list[np.float32]:
    """Shot times for one battery (gun ids not needed downstream)."""
    shots: list[np.float32] = []
    key = seed + "|" + unit_id + "|cannon"
    fire_actions = {"fire_by_rank", "fire_independent", "fight_prone", "halt_fire_obstacle"}
    for seg in segments:
        if seg["action"] not in fire_actions:
            continue
        for g in range(GUNS_PER_BATTERY):
            t = F(seg["t0"]) + F(6.0) * hash01(key, g * 131 + 7)
            k = 0
            while float(t) < seg["t1"]:
                shots.append(t)
                t = t + CANNON_INTERVAL + CANNON_JITTER * hash01(key, g * 131 + k * 17 + 29)
                k += 1
    shots.sort(key=float)
    return shots


def compile_strikes(units_out: list) -> dict[str, list[float]]:
    """Per-target-unit strike times, replicating CompileEngagements'
    nearest-enemy-infantry targeting (<= 650 m) at shot time. `units_out`
    is the compiler's already-built unit list (with perSecond tables).
    Note the C# context EXCLUDES parent roll-up units (us-webb) from
    staging; irrelevant here (parents share the batteries' side)."""
    strikes: dict[str, list[float]] = {}
    seed_free = None  # strikes use the bundle stagingSeed via caller key
    infantry = [u for u in units_out if u["arm"] != "artillery" and u["unitId"] != "us-webb"]
    batteries = [u for u in units_out if u["arm"] == "artillery"]
    for battery in batteries:
        shots = battery["_shotTimes"]
        for shot_t in shots:
            gx, gz = _position_at(battery["perSecond"], shot_t)
            best = None
            best_d = float("inf")
            for cand in infantry:
                if cand["side"] == battery["side"]:
                    continue
                cx, cz = _position_at(cand["perSecond"], shot_t)
                dx = cx - gx
                dz = cz - gz
                d = float(np.sqrt(dx * dx + dz * dz, dtype=F))
                if d < best_d:
                    best_d = d
                    best = cand
            if best is None or best_d > STRIKE_MAX_RANGE:
                continue
            strikes.setdefault(best["unitId"], []).append(float(shot_t + STRIKE_DELAY))
    for v in strikes.values():
        v.sort()
    return strikes


# ---------------------------------------------------------------------------
# The redistribution itself


def _quantize(t: float) -> float:
    """1/64-second grid — exactly representable in float32 at these
    magnitudes, so the C# parse and comparisons are exact."""
    return math.floor(t * 64.0 + 0.5) / 64.0


def profile_fall_times(profile: dict, unit_strikes: list[float]) -> list[float]:
    """The per-victim (k-indexed) fall-time array for a strike-correlated
    profile. Victims keep their hash-drawn identity and cause; only the
    times of the named strike classes move."""
    sc = profile["strikeCorrelation"]
    count = profile["count"]
    t0, t1 = float(profile["t0"]), float(profile["t1"])
    dur = F(t1) - F(t0)
    causes = apportion_causes(profile["id"], count, profile["causeMix"])
    classes = set(sc["classes"])
    window = float(sc["windowS"])
    cap = int(sc["maxPerStrike"])

    in_window = [s for s in unit_strikes if t0 <= s <= t1]
    load: dict[int, int] = {}
    times: list[float] = []
    lo = t0 + 0.03
    hi = t1 - 1.0 / 64.0
    for k in range(count):
        smooth = float(F(t0) + dur * inv_cdf(profile["intensityCurve"], (k + 0.5) / count))
        t = smooth
        if causes[k] in classes and in_window:
            best_i = None
            best_dt = window
            for i, s in enumerate(in_window):
                if load.get(i, 0) >= cap:
                    continue
                dt = abs(s - smooth)
                if dt <= best_dt:
                    # ties resolve to the LATER strike (<=) — deterministic
                    best_dt = dt
                    best_i = i
            if best_i is not None:
                j = load.get(best_i, 0)
                load[best_i] = j + 1
                t = in_window[best_i] + 0.25 + 0.12 * j
        times.append(_quantize(min(max(t, lo), hi)))
    return times


def cumulative_fallen(fall_times: list[float], t: float) -> int:
    """Fallen count through battle-second t (fallT <= t counts as fallen —
    matches CasualtySchedule.AliveCount's `fallT > t` alive test)."""
    return sum(1 for ft in fall_times if ft <= t)
