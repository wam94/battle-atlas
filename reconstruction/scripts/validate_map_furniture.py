"""Validate data/map-furniture/map-furniture.json against
docs/format/map-furniture.schema.json plus the cross-feature rules the
schema can't express (unique ids, non-empty source on every "documented"
feature). Pattern: validate_assets.py's CLI shape.

Usage:
  uv run python reconstruction/scripts/validate_map_furniture.py [<repo-root>]

Exits 1 (one error per line) on any violation.
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

import jsonschema

DEFAULT_REPO = Path(__file__).resolve().parent.parent.parent
SCHEMA_RELPATH = "docs/format/map-furniture.schema.json"
DATA_RELPATH = "data/map-furniture/map-furniture.json"


def validate(repo_root: Path) -> list[str]:
    errors: list[str] = []
    schema_path = repo_root / SCHEMA_RELPATH
    data_path = repo_root / DATA_RELPATH

    if not data_path.exists():
        return [f"{data_path} does not exist -- run "
                "reconstruction/scripts/trace_map_furniture.py first"]

    schema = json.loads(schema_path.read_text())
    doc = json.loads(data_path.read_text())

    validator = jsonschema.Draft7Validator(schema)
    for err in sorted(validator.iter_errors(doc), key=lambda e: list(e.absolute_path)):
        path = "/".join(str(p) for p in err.absolute_path)
        errors.append(f"schema: {path or '<root>'}: {err.message}")

    if errors:
        # further checks assume schema-valid shape; bail out early
        return errors

    seen_ids: dict[str, int] = {}
    for i, feature in enumerate(doc.get("features", [])):
        fid = feature.get("id", f"<index {i}>")
        seen_ids[fid] = seen_ids.get(fid, 0) + 1
        if feature.get("confidence") == "documented" and not feature.get("source", "").strip():
            errors.append(f"{fid}: confidence=documented requires a non-empty source")
        pts = feature.get("points", [])
        for (x, z) in pts:
            if not (0.0 <= x <= 8507.2 and 0.0 <= z <= 8507.2):
                errors.append(f"{fid}: point ({x}, {z}) outside the valid [0, 8507.2] square")

    for fid, count in seen_ids.items():
        if count > 1:
            errors.append(f"duplicate feature id {fid!r} ({count} occurrences)")

    return errors


def main() -> None:
    repo_root = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_REPO
    errors = validate(repo_root)
    if errors:
        for e in errors:
            print(e, file=sys.stderr)
        sys.exit(1)
    doc = json.loads((repo_root / DATA_RELPATH).read_text())
    print(f"OK: {len(doc['features'])} map-furniture features valid")


if __name__ == "__main__":
    main()
