#!/usr/bin/env python3
"""Retrograde-facing convention (owner ruling, 2026-07-13): a unit falling
back does NOT spin to face its direction of travel. On a movement leg whose
travel bearing opposes the unit's carried (prior combat) facing by more than
REVERSAL_THRESHOLD_DEG, the unit keeps its prior facing and moves retrograde
(backing away, still presenting toward the enemy) — unless an about-face or
countermarch is explicitly attested in that leg's own keyframe citation
(ATTESTED_ABOUT_FACE_LEGS below; each entry cites its source). Wheeling/
oblique changes at or under the threshold are left untouched: normal turns,
not reversals.

CROSS-FILE CONTINUITY: the five phase files are processed in their
battle-manifest.json day/phase order, and a unit's "current facing" carries
across a file boundary when the next file's kf[0] sits at the SAME position
(within POS_EPSILON_M) as the previous file's last keyframe. This only
fires for the two boundaries that are the SAME narrative moment split
across files for pacing (self-declared in the corpus's own citations —
"Continuity from the morning phase's <t> state" / "Continuity static ...
holds the ... cited 19:29 end state"): july1-morning->july1-afternoon and
july2-afternoon->july2-evening (SAME_DAY_BOUNDARIES below). The other two
boundaries (july1-afternoon->july2-afternoon, july2-evening->july3) are
overnight day-gaps; the corpus routinely re-postures units for the next
day's fight at those seams (fresh tablet/return citations, not "continuity"
ones) and a facing mismatch there is not this bug — left untouched.

Where propagation fires and the next file's own kf[0].facing disagrees with
the (possibly now-corrected) inherited value, kf[0].facing is rewritten to
match — otherwise the file's own explicit "holds the cited end state"
citation would be lies-by-omission after the leg-fix changes what that end
state actually is. This can, in turn, unmask further retrograde legs later
in the same file (evaluated in the same pass, using the corrected seed).

FILM-SAFETY TRIPWIRE: the 13 Angle-cast units' keyframes in
gettysburg-july3.json are pinned byte-identical (ED-21/stagingSeed pin,
decomposition-wave-1.md / strength-reconciliation-1.md precedent). This
script never writes a keyframe belonging to one of PINNED_ANGLE_CAST_UNITS
in PINNED_FILE — neither the within-file leg fix nor cross-file
propagation — even where it would otherwise qualify; such legs are only
reported for owner ruling, never mutated.

Deterministic, idempotent (a second run makes zero further changes), and
touches ONLY the "facing" field of existing keyframes — positions,
strengths, formations, confidence, and citations are never modified. This
is the authoring-pipeline entry point for the retrograde-facing fix; battle
JSON is not hand-edited.

Usage:
    python3 fix_retrograde_facing.py <repo_root>            # report only
    python3 fix_retrograde_facing.py <repo_root> --write     # apply + write
"""
import argparse
import json
import math
import os
import sys

MANIFEST = "app/Assets/StreamingAssets/Atlas/battle-manifest.json"


def manifest_files(repo_root):
    # Manifest-driven so new phases can't silently escape the scan (the
    # july3-morning phase landed in a parallel slice and a hardcoded list
    # missed it). Order follows the manifest's day/phase order.
    with open(os.path.join(repo_root, MANIFEST)) as f:
        manifest = json.load(f)
    return [
        phase["battle"]
        for day in manifest["days"]
        for phase in day["phases"]
        if phase.get("status") == "reconstructed"
    ]

# (earlier_file, later_file) pairs where a unit's facing carries across the
# file boundary — see module docstring for why only these two.
SAME_DAY_BOUNDARIES = {
    ("gettysburg-july1-morning.json", "gettysburg-july1-afternoon.json"),
    ("gettysburg-july2-afternoon.json", "gettysburg-july2-evening.json"),
}

REVERSAL_THRESHOLD_DEG = 120.0
# Safety-net trigger, independent of the bearing check: a handful of legs
# (individual regiments inside a brigade-wide rout, e.g. csa-8va/-1va/-9va/
# -38va/-57va in Pickett's charge) land just under REVERSAL_THRESHOLD_DEG on
# the bearing-vs-carried test (their own drawn path's bearing happens to
# fall a few degrees short of 120 from carried) while their AUTHORED facing
# still swings 150+ degrees in that single leg — the same "spin to face the
# retreat" signature as their siblings that DO cross the bearing threshold.
# A genuine wheel/oblique turn (the threshold's carve-out) has no business
# swinging the symbol's own facing this far in one leg, so this is treated
# as the same defect regardless of the bearing figure. Matches the
# regression guard's own threshold (test_retrograde_facing.py) by design.
FACING_SWING_SAFETY_NET_DEG = 150.0
POS_EPSILON_M = 0.5  # below this, a leg isn't "moving" — no travel bearing to compare

# The 13 Angle-cast units whose gettysburg-july3.json keyframes are pinned
# byte-identical (film-safety tripwire). Same set as
# author-a2-dossier-placement.ts's guard / decomposition-wave-1.md §5 /
# strength-reconciliation-1.md §5.
PINNED_ANGLE_CAST_UNITS = frozenset({
    "csa-garnett", "csa-kemper", "csa-armistead", "csa-fry",
    "us-webb", "us-69pa", "us-71pa", "us-72pa",
    "us-btty-cushing", "us-btty-cowan", "us-btty-arnold",
    "us-hall", "us-stannard",
})
PINNED_FILE = "gettysburg-july3.json"

# Attested about-faces / countermarches: explicit deliberate-reversal
# language in the leg's own END-keyframe citation, verified by manual
# review of every leg this script's threshold flags (see
# docs/reconstruction/audit/retrograde-facing.md §3 for the full review).
# Each entry is (unit_id, file, t0, t1) -> citation excerpt kept for the record.
ATTESTED_ABOUT_FACE_LEGS = {
    ("us-16vt", "gettysburg-july3.json", 8700, 9900):
        "recalled and re-forms facing south as Wilcox and Lang step off "
        "~15:45 — change of front to the rear (Veazey's OR report; "
        "Benedict vol. 2)",
    ("us-13vt", "gettysburg-july3.json", 8700, 9600):
        "returns toward the original front after Pickett's repulse "
        "(Sturtevant 1910) — companion reformation to us-16vt's attested "
        "change of front, same Stannard-brigade flank-attack action",
    # us-kane / us-candy (Geary's "wrong road" countermarch, both in
    # gettysburg-july2-evening.json) are NOT listed here even though their
    # t=0 keyframes carry explicit "counter-march"/"wrong-road" citations:
    # once cross-file boundary propagation corrects their kf[0] facing
    # (inherited from july2-afternoon.json's corrected end state — see
    # SAME_DAY_BOUNDARIES), their first leg's travel bearing no longer
    # opposes the corrected carried facing by more than
    # REVERSAL_THRESHOLD_DEG, so it classifies as a normal turn on its own
    # and never reaches this table. No special-case needed; recorded here
    # so the citation match isn't mistaken for an oversight.
}


def bearing_deg(dx: float, dz: float) -> float:
    """Compass bearing (0=north/+z, 90=east/+x), matching
    reconstruction/scripts/compile_angle.py's _route_bearing and
    pipeline/terrain_pipeline/environment.py's _bearing_deg."""
    return math.degrees(math.atan2(dx, dz)) % 360.0


def angle_diff(a: float, b: float) -> float:
    """Shortest signed diff b-a, in (-180, 180]."""
    return (b - a + 180.0) % 360.0 - 180.0


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("repo_root")
    ap.add_argument("--write", action="store_true", help="apply the fix and write files")
    args = ap.parse_args()

    battle_dir = f"{args.repo_root}/app/Assets/Battle"
    files = manifest_files(args.repo_root)
    data_by_file = {}
    for fname in files:
        with open(f"{battle_dir}/{fname}", encoding="utf-8") as f:
            data_by_file[fname] = json.load(f)

    results = {f: {"converted": [], "boundary": [], "preserved": [], "deferred": [], "noop": []} for f in files}
    last_state = {}  # unit_id -> (facing, x, z), corrected, after the previous file
    prev_fname = None

    for fname in files:
        data = data_by_file[fname]
        this_file_final = {}  # unit_id -> (facing, x, z) after this file, for the next boundary

        for u in data["units"]:
            uid = u["id"]
            kfs = u["keyframes"]
            if not kfs:
                continue
            pinned = fname == PINNED_FILE and uid in PINNED_ANGLE_CAST_UNITS

            carried = kfs[0]["facing"]
            boundary_ok = prev_fname is not None and (prev_fname, fname) in SAME_DAY_BOUNDARIES
            if boundary_ok and not pinned and uid in last_state:
                pf, px, pz = last_state[uid]
                dist0 = math.hypot(kfs[0]["x"] - px, kfs[0]["z"] - pz)
                if dist0 <= POS_EPSILON_M:
                    if abs(angle_diff(pf, carried)) > 1.0:
                        results[fname]["boundary"].append((uid, kfs[0]["t"], carried, pf))
                        if args.write:
                            kfs[0]["facing"] = pf
                    carried = pf

            for i in range(len(kfs) - 1):
                a, b = kfs[i], kfs[i + 1]
                dx, dz = b["x"] - a["x"], b["z"] - a["z"]
                dist = math.hypot(dx, dz)
                if dist <= POS_EPSILON_M:
                    carried = b["facing"]
                    continue

                bearing = bearing_deg(dx, dz)
                delta = angle_diff(carried, bearing)
                old_facing = b["facing"]
                swings_hard = abs(angle_diff(carried, old_facing)) > FACING_SWING_SAFETY_NET_DEG
                if abs(delta) <= REVERSAL_THRESHOLD_DEG and not swings_hard:
                    carried = b["facing"]  # normal turn/wheel/oblique — adopt as-is
                    continue

                leg_key = (uid, fname, a["t"], b["t"])
                if pinned:
                    if abs(angle_diff(carried, old_facing)) > 1.0:
                        results[fname]["deferred"].append((uid, a["t"], b["t"], carried, old_facing, bearing))
                    carried = b["facing"]
                    continue
                if leg_key in ATTESTED_ABOUT_FACE_LEGS:
                    results[fname]["preserved"].append((uid, a["t"], b["t"], carried, old_facing, ATTESTED_ABOUT_FACE_LEGS[leg_key]))
                    carried = b["facing"]
                    continue
                if abs(angle_diff(carried, old_facing)) < 1.0:
                    results[fname]["noop"].append((uid, a["t"], b["t"], carried))
                    carried = b["facing"]
                    continue

                results[fname]["converted"].append((uid, a["t"], b["t"], carried, old_facing, bearing))
                if args.write:
                    b["facing"] = carried
                # carried stays the same — retrograde motion holds facing

            # `carried` already reflects the corrected value of the unit's
            # last keyframe regardless of --write (it's updated in lockstep
            # with every b["facing"] mutation above) — use it, not the raw
            # JSON field, so report-mode previews boundary propagation too.
            this_file_final[uid] = (carried, kfs[-1]["x"], kfs[-1]["z"])

        last_state = this_file_final
        prev_fname = fname

    if args.write:
        for fname in files:
            with open(f"{battle_dir}/{fname}", "w", encoding="utf-8") as f:
                f.write(json.dumps(data_by_file[fname], indent=2, ensure_ascii=False))

    totals = {k: 0 for k in ("converted", "boundary", "preserved", "deferred", "noop")}
    for fname in files:
        r = results[fname]
        print(f"=== {fname} ===")
        print(f"  boundary-propagated (kf0 inherits prior file's corrected end): {len(r['boundary'])}")
        for uid, t0, carried, pf in r["boundary"]:
            print(f"    {uid} t0={t0}: facing {carried:.1f} -> {pf:.1f} (inherited)")
        print(f"  converted (facing now holds):  {len(r['converted'])}")
        for uid, t0, t1, carried, old, bearing in r["converted"]:
            print(f"    {uid} t={t0}->{t1}: facing {old:.1f} -> {carried:.1f} (bearing was {bearing:.1f})")
        print(f"  preserved (attested about-face): {len(r['preserved'])}")
        for uid, t0, t1, carried, new, cite in r["preserved"]:
            print(f"    {uid} t={t0}->{t1}: kept authored {new:.1f} (was {carried:.1f}) — {cite[:90]}")
        print(f"  deferred (Angle-cast pinned):    {len(r['deferred'])}")
        for uid, t0, t1, carried, old, bearing in r["deferred"]:
            print(f"    {uid} t={t0}->{t1}: authored facing {old:.1f} (would-be hold {carried:.1f}, bearing {bearing:.1f}) — PINNED, NOT TOUCHED")
        print(f"  no-op (already held):             {len(r['noop'])}")
        print()
        for k in totals:
            totals[k] += len(r[k])

    print("=== totals ===")
    print(" ".join(f"{k}={v}" for k, v in totals.items()))
    print("(write applied)" if args.write else "(report only — pass --write to apply)")


if __name__ == "__main__":
    sys.exit(main())
