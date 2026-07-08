"""Validate a Soldier View viewpoints document against viewpoint.schema.json
plus the cross-field rules the schema cannot express.

Usage: uv run python scripts/validate_viewpoints.py <viewpoints.json>
Exit 0 when valid; exit 1 with one error per line otherwise.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

import jsonschema

SCHEMA_PATH = Path(__file__).resolve().parent.parent / "schemas" / "viewpoint.schema.json"

# July 3 slice bounds on the canonical battle clock (BattleClock.EndTime).
BATTLE_END = 10800.0


def load_schema() -> dict:
    return json.loads(SCHEMA_PATH.read_text())


def validate_document(doc: dict) -> list[str]:
    """Return a list of human-readable errors; empty means valid."""
    errors: list[str] = []
    validator = jsonschema.Draft202012Validator(load_schema())
    for err in sorted(validator.iter_errors(doc), key=lambda e: list(e.absolute_path)):
        path = "/".join(str(p) for p in err.absolute_path) or "<root>"
        errors.append(f"schema: {path}: {err.message}")
    if errors:
        return errors  # semantic checks assume schema shape

    seen_ids: set[str] = set()
    for i, vp in enumerate(doc["viewpoints"]):
        where = f"viewpoints[{i}] ({vp['id']})"
        if vp["id"] in seen_ids:
            errors.append(f"{where}: duplicate viewpoint id")
        seen_ids.add(vp["id"])
        if not vp["t1"] > vp["t0"]:
            errors.append(f"{where}: t1 ({vp['t1']}) must be > t0 ({vp['t0']})")
        if vp["t1"] > BATTLE_END:
            errors.append(f"{where}: t1 ({vp['t1']}) exceeds battle end ({BATTLE_END})")
        for key in ("proxy", "full"):
            path = vp["media"][key]
            if path is None:
                continue
            if Path(path).is_absolute() or ".." in Path(path).parts:
                errors.append(
                    f"{where}: media.{key} must be a plain relative path "
                    f"under StreamingAssets, got {path!r}")
    return errors


def main(argv: list[str]) -> int:
    if len(argv) != 2:
        print(__doc__, file=sys.stderr)
        return 2
    doc = json.loads(Path(argv[1]).read_text())
    errors = validate_document(doc)
    for e in errors:
        print(e, file=sys.stderr)
    if not errors:
        print(f"OK: {argv[1]} ({len(doc['viewpoints'])} viewpoint(s))")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
