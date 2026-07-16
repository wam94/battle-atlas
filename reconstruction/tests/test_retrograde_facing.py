"""Regression guard for the retrograde-facing convention (owner ruling,
2026-07-13; docs/reconstruction/audit/retrograde-facing.md): a FORMED unit
falling back must not spin to face its direction of travel — it holds its
prior combat facing and moves retrograde, unless an about-face/countermarch
is attested, or the flight itself is attested (a rout is not a formed
retirement: men turn and run, and the authored travel-bearing facing is the
historically correct depiction — ED-79 (ADOPTED), the rout/dissolution
exemption; docs/reconstruction/audit/angle-facing-adjudication.md), or the
unit is one of the 13 Angle-cast units in gettysburg-july3.json
(film-safety pinned regardless of classification).

test_no_unattested_facing_reversal_over_150deg is the guard proper: it
scans every moving leg (position changes by more than
fix_retrograde_facing.POS_EPSILON_M) in all five battle files for a raw
facing swing over 150 degrees, and fails if one shows up that isn't in the
small, cited ALLOWED_LARGE_DELTA_LEGS table below. A future edit that
reintroduces the "facing = direction of travel" bug on some leg — including
one this wave's fix never touched — trips this test.
"""

import json
import math
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
import fix_retrograde_facing as fx  # noqa: E402

REPO = Path(__file__).resolve().parent.parent.parent
BATTLE_DIR = REPO / "app" / "Assets" / "Battle"

RAW_DELTA_GUARD_DEG = 150.0

# Every leg in the committed corpus whose raw facing swing exceeds
# RAW_DELTA_GUARD_DEG, and why it's not a spin-defect regression:
#   - the 4 Angle-cast repulse legs: ADJUDICATED ATTESTED-FLIGHT under the
#     ED-79 rout/dissolution exemption, ADOPTED (per-leg dispositions:
#     ED-80, ADOPTED; docs/reconstruction/audit/angle-facing-adjudication.md)
#     — each leg runs formation scattered->routed, its withdrawal is
#     dissolution-class in the unit's own dossier (Peyton's OR: "came off
#     slowly, but greatly scattered, the identity of every regiment being
#     entirely lost"), and the shipped film itself shows the turn
#     (p10-gate-evidence.md shot list, 8700-8820: "The repulse: the line
#     turns back"). Not defects deferred for fixing — the authored
#     travel-bearing facing is the historically correct depiction of a
#     rout. The keyframes additionally stay film-safety pinned (see
#     fix_retrograde_facing.PINNED_ANGLE_CAST_UNITS / PINNED_FILE).
#   - us-16vt: attested about-face ("change of front to the rear", Veazey's
#     OR report) — see fix_retrograde_facing.ATTESTED_ABOUT_FACE_LEGS.
ALLOWED_LARGE_DELTA_LEGS = {
    ("csa-garnett", "gettysburg-july3.json", 8700, 9000): "ATTESTED-FLIGHT, ED-79/ED-80 (angle-facing-adjudication.md) — scattered->routed dissolution (peyton-or-1863); Angle-cast pinned",
    ("csa-kemper", "gettysburg-july3.json", 8700, 9000): "ATTESTED-FLIGHT, ED-79/ED-80 (angle-facing-adjudication.md) — scattered->routed dissolution (division record); Angle-cast pinned",
    ("csa-armistead", "gettysburg-july3.json", 8700, 9000): "ATTESTED-FLIGHT, ED-79/ED-80 (angle-facing-adjudication.md) — scattered->routed dissolution (ED-8; '643 missing never marched back'); Angle-cast pinned",
    ("csa-fry", "gettysburg-july3.json", 8700, 9000): "ATTESTED-FLIGHT, ED-79/ED-80 (angle-facing-adjudication.md) — scattered->routed dissolution (or-27-2-shepard: 'a hopeless case, and fell back'); Angle-cast pinned",
    ("us-16vt", "gettysburg-july3.json", 8700, 9900): "attested about-face — change of front to the rear",
}


def moving_legs():
    """Yield (fname, unit_id, kf_a, kf_b) for every leg across all
    manifest-reconstructed battle files where the unit's position
    actually changes."""
    for fname in fx.manifest_files(str(REPO)):
        data = json.loads((BATTLE_DIR / fname).read_text(encoding="utf-8"))
        for u in data["units"]:
            kfs = u["keyframes"]
            for i in range(len(kfs) - 1):
                a, b = kfs[i], kfs[i + 1]
                dx, dz = b["x"] - a["x"], b["z"] - a["z"]
                if math.hypot(dx, dz) > fx.POS_EPSILON_M:
                    yield fname, u["id"], a, b


def test_no_unattested_facing_reversal_over_150deg():
    violations = []
    for fname, uid, a, b in moving_legs():
        delta = abs(fx.angle_diff(a["facing"], b["facing"]))
        if delta > RAW_DELTA_GUARD_DEG:
            key = (uid, fname, a["t"], b["t"])
            if key not in ALLOWED_LARGE_DELTA_LEGS:
                violations.append((fname, uid, a["t"], b["t"], round(delta, 1)))
    assert not violations, (
        "unattested facing reversal(s) over 150 degrees found on a single "
        "moving leg — possible retrograde-facing spin regression (or a new "
        "leg that needs a cited exception added to ALLOWED_LARGE_DELTA_LEGS "
        f"if it's a real about-face/countermarch): {violations}"
    )


def test_allowlist_entries_still_present():
    """Every allowlisted leg should still exist with its position/citation
    intact — if one disappears (unit re-authored, file restructured), the
    allowlist entry is stale and should be re-reviewed, not silently kept."""
    present = {(uid, fname, a["t"], b["t"]) for fname, uid, a, b in moving_legs()}
    missing = [key for key in ALLOWED_LARGE_DELTA_LEGS if key not in present]
    assert not missing, f"allowlisted leg(s) no longer exist in the corpus: {missing}"


def test_pinned_angle_cast_units_byte_identical_to_main():
    """Film-safety tripwire, standing check: the 13 Angle-cast units'
    keyframes in gettysburg-july3.json never change, regardless of what
    facing-convention fixes land elsewhere in the file. Pins the exact
    sha256 established in retrograde-facing.md (matches the pre-existing
    ED-21/decomposition-wave-1/strength-reconciliation-1 pin chain)."""
    data = json.loads((BATTLE_DIR / fx.PINNED_FILE).read_text(encoding="utf-8"))
    units = {u["id"]: u for u in data["units"] if u["id"] in fx.PINNED_ANGLE_CAST_UNITS}
    assert len(units) == len(fx.PINNED_ANGLE_CAST_UNITS)
    blob = json.dumps(units, sort_keys=True, ensure_ascii=False)
    import hashlib
    got = hashlib.sha256(blob.encode("utf-8")).hexdigest()
    assert got == "69163017ebc0c670b742d91e89658310d526232ecbbe966f12e291d67c03ab68"
