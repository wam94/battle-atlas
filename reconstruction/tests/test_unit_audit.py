"""Tests for the tabletop unit-activity master table builder
(reconstruction/scripts/build_unit_audit.py).

Guards the strength-reconciliation-1 pipeline fix: the script must read
EVERY reconstructed phase named in the battle manifest (ADR 0005), not just
gettysburg-july3.json — before this fix, every day-expansion-authored unit
(Gamble/Devin/Calef in july1-morning, the six decomposition-wave-1 units,
etc.) was invisible to the master table's computed columns
(decomposition-wave-1.md §10 pickup 2). These tests fail if that hardcoding
regresses, and separately unit-test the cross-phase merge (keyframe/event
concatenation, "Phases" provenance) against a small synthetic manifest so
the merge logic is checked independent of the committed corpus's current
content.

openpyxl (needed only for the xlsx-writing tail of main()) is imported
lazily inside main() by the script itself, so these tests never need it.
"""

import json
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
import build_unit_audit as bua  # noqa: E402

REPO = Path(__file__).resolve().parent.parent.parent


# ---------------------------------------------------------------- fixtures

def _write_battle(path: Path, start_time: int, units: list[dict], events=None) -> None:
    path.write_text(json.dumps({
        "name": path.stem,
        "startTime": start_time,
        "endTime": 100,
        "units": units,
        "events": events or [],
        "environment": {},
    }))


def _kf(t, strength, x=0, z=0):
    return {
        "t": t, "x": x, "z": z, "facing": 0, "formation": "line",
        "strength": strength, "confidence": "documented", "citation": "test",
    }


@pytest.fixture()
def two_phase_repo(tmp_path: Path):
    """A minimal 2-phase manifest: 'alpha' has a unit A (only there) and a
    continuing unit C; 'beta' has C (renamed/re-based) and a fresh unit B."""
    battle_dir = tmp_path / "Battle"
    battle_dir.mkdir()
    _write_battle(
        battle_dir / "alpha.json", 0,
        [
            {"id": "unit-a", "name": "Unit A", "side": "union",
             "keyframes": [_kf(0, 100, x=0), _kf(100, 90, x=10)]},
            {"id": "unit-c", "name": "Unit C", "side": "union",
             "keyframes": [_kf(0, 200, x=0), _kf(100, 180, x=20)]},
        ],
        events=[{"id": "ev1", "unitId": "unit-a", "kind": "musketry"}],
    )
    _write_battle(
        battle_dir / "beta.json", 200,
        [
            {"id": "unit-c", "name": "Unit C (renamed)", "side": "union",
             "keyframes": [_kf(0, 180, x=20), _kf(100, 150, x=25)]},
            {"id": "unit-b", "name": "Unit B", "side": "confederate",
             "keyframes": [_kf(0, 50, x=5), _kf(100, 50, x=5)]},
        ],
        events=[{"id": "ev2", "unitId": "unit-c", "kind": "musketry"}],
    )
    manifest = {
        "name": "Test manifest",
        "days": [
            {"id": "day1", "label": "Day 1", "date": "1900-01-01", "phases": [
                {"id": "alpha", "label": "Alpha", "status": "reconstructed",
                 "battle": "alpha.json", "startTime": 0, "endTime": 100},
                {"id": "skipped", "label": "Skipped", "status": "not-reconstructed",
                 "note": "honest gap — must not be read"},
            ]},
            {"id": "day2", "label": "Day 2", "date": "1900-01-02", "phases": [
                {"id": "beta", "label": "Beta", "status": "reconstructed",
                 "battle": "beta.json", "startTime": 200, "endTime": 100},
            ]},
        ],
    }
    manifest_path = tmp_path / "manifest.json"
    manifest_path.write_text(json.dumps(manifest))
    return manifest_path, battle_dir


# ---------------------------------------------------------------- unit tests

def test_reconstructed_phases_skips_not_reconstructed(two_phase_repo):
    manifest_path, _ = two_phase_repo
    phases = bua.reconstructed_phases(manifest_path)
    assert phases == [("alpha", "alpha.json"), ("beta", "beta.json")]


def test_merge_concatenates_keyframes_and_events_across_phases(two_phase_repo):
    manifest_path, battle_dir = two_phase_repo
    phase_battles = bua.load_phase_battles(manifest_path, battle_dir)
    units, events_by_unit, parents = bua.merge_units_across_phases(phase_battles)

    # unit-a only exists in alpha: 2 keyframes, its own event.
    assert len(units["unit-a"]["keyframes"]) == 2
    assert units["unit-a"]["phases"] == ["alpha"]
    assert len(events_by_unit.get("unit-a", [])) == 1

    # unit-c exists in BOTH phases: 4 keyframes concatenated in phase order,
    # start strength from alpha's t=0 (200), end strength from beta's last
    # keyframe (150) — the whole point of the fix (a continuing unit's full
    # arc, not just one file's slice).
    c = units["unit-c"]
    assert c["phases"] == ["alpha", "beta"]
    assert [kf["strength"] for kf in c["keyframes"]] == [200, 180, 180, 150]
    # last-phase-wins identity: the beta rename is what's reported.
    assert c["name"] == "Unit C (renamed)"
    assert len(events_by_unit.get("unit-c", [])) == 1

    # unit-b only exists in beta.
    assert units["unit-b"]["phases"] == ["beta"]

    # not-reconstructed phases are never read (no file for it, so this
    # would raise if the loader tried).
    assert set(units) == {"unit-a", "unit-b", "unit-c"}


def test_load_phase_battles_never_reads_not_reconstructed_phase(two_phase_repo, tmp_path):
    manifest_path, battle_dir = two_phase_repo
    # if the loader tried to read a "skipped.json" for the not-reconstructed
    # phase it would raise FileNotFoundError; the fixture never wrote that
    # file, so a clean run proves the honest-note phase is skipped.
    bua.load_phase_battles(manifest_path, battle_dir)


# ------------------------------------------------------- committed-repo guard

def test_committed_manifest_has_five_reconstructed_phases():
    """Coverage floor: the current manifest names five reconstructed
    phases (day-expansion-slice-3.md §9). If a future phase is added this
    number should only grow; if it silently drops, the master table lost
    coverage."""
    phases = bua.reconstructed_phases()
    assert len(phases) >= 5
    battle_files = {f for _, f in phases}
    assert "gettysburg-july3.json" in battle_files
    assert "gettysburg-july1-morning.json" in battle_files
    assert "gettysburg-july1-afternoon.json" in battle_files
    assert "gettysburg-july2-afternoon.json" in battle_files
    assert "gettysburg-july2-evening.json" in battle_files


def test_committed_master_table_input_includes_day_expansion_units():
    """The regression this whole fix exists to prevent: units authored
    only in a day-expansion phase file (never in gettysburg-july3.json)
    must appear in the merged unit set. Before the fix, BATTLE was
    hardcoded to gettysburg-july3.json and these ids were invisible to
    every computed column."""
    phase_battles = bua.load_phase_battles()
    units, _, _ = bua.merge_units_across_phases(phase_battles)

    july3_only = json.loads(
        (REPO / "app/Assets/Battle/gettysburg-july3.json").read_text()
    )
    july3_ids = {u["id"] for u in july3_only["units"]}

    # Gamble/Devin/Calef (day-expansion-slice-3) and the decomposition-wave-1
    # regiment promotions (us-6wi/us-147ny/us-16me) are July-1-only.
    day_expansion_only = {"us-cav-gamble", "us-cav-devin", "us-btty-calef",
                           "us-6wi", "us-147ny", "us-16me"}
    assert day_expansion_only.issubset(units.keys())
    assert day_expansion_only.isdisjoint(july3_ids), (
        "fixture assumption broken: these ids were expected to be absent "
        "from gettysburg-july3.json"
    )

    # the merged set must be a strict superset of july3-only coverage.
    assert set(units) >= july3_ids
    assert len(units) > len(july3_ids)
