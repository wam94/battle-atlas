"""Validate the third-party asset manifest against third-party-asset.schema.json
plus the licensing/coverage/checksum rules the schema cannot express
(Angle Reconstruction V2 plan section 11).

Usage:
  uv run python scripts/validate_assets.py [<repo-root>]
  uv run python scripts/validate_assets.py --print-sha [<repo-root>]

Default repo root is this repo. Validation fails (exit 1, one error per
line) when:
  - a file exists under app/Assets/ThirdParty without a manifest owner;
  - a required field is absent (schema);
  - the license is unknown, NC, ND, source-only, or otherwise incompatible;
  - the asset comes from the Unity Asset Store or Mixamo (never a required
    build input, plan sections 2 and 12 Phase 2);
  - `redistributable` is false;
  - attribution text cannot be generated for CC-BY assets;
  - the content checksum changes without a manifest update;
  - archived license evidence (plan section 18) is missing.

`--print-sha` prints the computed content digest for every entry path, for
use when authoring or updating manifest entries.
"""

from __future__ import annotations

import hashlib
import json
import sys
from pathlib import Path

import jsonschema

SCHEMA_PATH = Path(__file__).resolve().parent.parent / "schemas" / "third-party-asset.schema.json"
DEFAULT_REPO = Path(__file__).resolve().parent.parent.parent

# Manifest location and the tree it governs, relative to the repo root.
MANIFEST_RELPATH = "app/Assets/ThirdParty/manifest.json"
THIRDPARTY_RELPATH = "app/Assets/ThirdParty"

# Only these licenses may appear under ThirdParty (locked decision 12:
# CC0, CC-BY, or compatible). Project-owned work lives under
# Assets/ProjectOwned and is not manifested here.
ALLOWED_LICENSES = {"CC0-1.0", "CC-BY-3.0", "CC-BY-4.0"}

# Attribution is mandatory for these (generate_attribution.py must be able
# to emit a complete attribution line).
ATTRIBUTION_REQUIRED_LICENSES = {"CC-BY-3.0", "CC-BY-4.0"}

# Sources that may never become required build inputs, regardless of the
# license string claimed in the manifest.
FORBIDDEN_URL_SUBSTRINGS = (
    "assetstore.unity.com",
    "mixamo.com",
)

# License strings that get a specific explanation rather than the generic
# "not on the allowlist" message.
FORBIDDEN_LICENSE_HINTS = {
    "-nc": "NonCommercial licenses are incompatible with an open-source redistributable build",
    "-nd": "NoDerivatives licenses are incompatible (assets are modified and redistributed)",
    "mixamo": "Mixamo terms do not permit redistribution of source files",
    "asset-store": "Unity Asset Store EULA content cannot be a required build input",
    "assetstore": "Unity Asset Store EULA content cannot be a required build input",
    "unknown": "unknown licenses are never acceptable",
}

# Unity bookkeeping files are excluded from ownership and digests.
IGNORED_SUFFIXES = (".meta",)
IGNORED_NAMES = (".DS_Store",)


def load_schema() -> dict:
    return json.loads(SCHEMA_PATH.read_text())


def content_files(asset_dir: Path) -> list[Path]:
    """All digest-relevant files under an asset directory, sorted."""
    return sorted(
        p for p in asset_dir.rglob("*")
        if p.is_file()
        and p.suffix not in IGNORED_SUFFIXES
        and p.name not in IGNORED_NAMES
    )


def compute_content_sha256(asset_dir: Path) -> str:
    """Deterministic digest of all non-*.meta files under asset_dir.

    For each file in sorted posix-relpath order, feed the digest with
    relpath bytes, a NUL, and the file's raw sha256 digest. A rename,
    addition, removal, or content change all change the result.
    """
    outer = hashlib.sha256()
    for p in content_files(asset_dir):
        rel = p.relative_to(asset_dir).as_posix()
        outer.update(rel.encode("utf-8"))
        outer.update(b"\0")
        outer.update(hashlib.sha256(p.read_bytes()).digest())
    return outer.hexdigest()


def _license_error(license_id: str) -> str | None:
    if license_id in ALLOWED_LICENSES:
        return None
    lowered = license_id.lower()
    for hint, reason in FORBIDDEN_LICENSE_HINTS.items():
        if hint in lowered:
            return f"license {license_id!r} rejected: {reason}"
    return (
        f"license {license_id!r} is not on the allowlist "
        f"({', '.join(sorted(ALLOWED_LICENSES))}); unknown/source-only/"
        "incompatible licenses are rejected"
    )


def validate_manifest(manifest: dict, repo_root: Path) -> list[str]:
    """Return a list of human-readable errors; empty means valid."""
    errors: list[str] = []
    validator = jsonschema.Draft202012Validator(load_schema())
    for err in sorted(validator.iter_errors(manifest), key=lambda e: list(e.absolute_path)):
        path = "/".join(str(p) for p in err.absolute_path) or "<root>"
        errors.append(f"schema: {path}: {err.message}")
    if errors:
        return errors  # semantic checks assume schema shape

    thirdparty_dir = repo_root / THIRDPARTY_RELPATH
    owned: dict[Path, str] = {}  # asset dir -> entry id
    seen_ids: set[str] = set()

    for i, asset in enumerate(manifest["assets"]):
        where = f"assets[{i}] ({asset['id']})"

        if asset["id"] in seen_ids:
            errors.append(f"{where}: duplicate asset id")
        seen_ids.add(asset["id"])

        lic_err = _license_error(asset["license"])
        if lic_err:
            errors.append(f"{where}: {lic_err}")

        for url_field in ("sourceUrl", "downloadUrl", "licenseUrl"):
            lowered = asset[url_field].lower()
            for bad in FORBIDDEN_URL_SUBSTRINGS:
                if bad in lowered:
                    errors.append(
                        f"{where}: {url_field} points at {bad}; Unity Asset "
                        "Store and Mixamo content may not be a required build input")

        if not asset["redistributable"]:
            errors.append(f"{where}: redistributable is false; every required "
                          "asset must be redistributable (locked decision 12)")

        if asset["modified"] and not asset["modifications"].strip():
            errors.append(f"{where}: modified is true but modifications is empty")

        if asset["license"] in ATTRIBUTION_REQUIRED_LICENSES:
            for field in ("title", "author", "sourceUrl", "licenseUrl"):
                if not asset[field].strip():
                    errors.append(
                        f"{where}: attribution text cannot be generated: "
                        f"{field} is blank but {asset['license']} requires attribution")

        evidence = repo_root / asset["licenseEvidence"]
        if not evidence.exists():
            errors.append(
                f"{where}: licenseEvidence {asset['licenseEvidence']!r} does not "
                "exist; archive the acquisition-day license metadata (plan section 18)")

        # path checks: exists, non-empty, checksum agreement.
        rel = Path(asset["path"])
        asset_dir = repo_root / "app" / rel
        if not asset_dir.is_dir():
            errors.append(f"{where}: path {asset['path']!r} is not a directory under app/")
            continue
        files = content_files(asset_dir)
        if not files:
            errors.append(f"{where}: path {asset['path']!r} contains no content files")
            continue
        actual = compute_content_sha256(asset_dir)
        if actual != asset["sha256"]:
            errors.append(
                f"{where}: content checksum mismatch: manifest has "
                f"{asset['sha256']}, files hash to {actual}; the checksum "
                "changed without a manifest update")

        for other_dir, other_id in owned.items():
            if asset_dir == other_dir or asset_dir in other_dir.parents or other_dir in asset_dir.parents:
                errors.append(
                    f"{where}: path overlaps entry {other_id!r}; every file "
                    "must have exactly one manifest owner")
        owned[asset_dir] = asset["id"]

    # Coverage: every content file under ThirdParty needs a manifest owner.
    if thirdparty_dir.is_dir():
        manifest_file = repo_root / MANIFEST_RELPATH
        for p in content_files(thirdparty_dir):
            if p == manifest_file:
                continue
            if not any(d == p.parent or d in p.parents for d in owned):
                rel = p.relative_to(repo_root).as_posix()
                errors.append(
                    f"unmanifested file: {rel} exists under ThirdParty "
                    "without a manifest owner")

    return errors


def validate_repo(repo_root: Path) -> list[str]:
    manifest_path = repo_root / MANIFEST_RELPATH
    if not manifest_path.exists():
        thirdparty_dir = repo_root / THIRDPARTY_RELPATH
        if thirdparty_dir.is_dir() and content_files(thirdparty_dir):
            return [f"{MANIFEST_RELPATH} is missing but {THIRDPARTY_RELPATH} has content"]
        return []  # no third-party content, nothing to validate
    manifest = json.loads(manifest_path.read_text())
    return validate_manifest(manifest, repo_root)


def main(argv: list[str]) -> int:
    args = [a for a in argv[1:] if a != "--print-sha"]
    print_sha = "--print-sha" in argv[1:]
    if len(args) > 1:
        print(__doc__, file=sys.stderr)
        return 2
    repo_root = Path(args[0]).resolve() if args else DEFAULT_REPO

    if print_sha:
        manifest = json.loads((repo_root / MANIFEST_RELPATH).read_text())
        for asset in manifest["assets"]:
            asset_dir = repo_root / "app" / asset["path"]
            print(f"{asset['id']}: {compute_content_sha256(asset_dir)}")
        return 0

    errors = validate_repo(repo_root)
    for e in errors:
        print(e, file=sys.stderr)
    if not errors:
        manifest_path = repo_root / MANIFEST_RELPATH
        n = len(json.loads(manifest_path.read_text())["assets"]) if manifest_path.exists() else 0
        print(f"OK: {MANIFEST_RELPATH} ({n} asset(s))")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
