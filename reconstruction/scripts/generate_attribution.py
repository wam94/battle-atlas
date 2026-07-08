"""Generate docs/assets/THIRD_PARTY_ASSETS.md from the third-party asset
manifest (Angle Reconstruction V2 plan section 11). The document is always
generated, never hand-maintained; the reconstruction test suite fails when
the committed document is stale.

Usage:
  uv run python scripts/generate_attribution.py [<repo-root>]          # write
  uv run python scripts/generate_attribution.py --check [<repo-root>]  # verify
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

DEFAULT_REPO = Path(__file__).resolve().parent.parent.parent
MANIFEST_RELPATH = "app/Assets/ThirdParty/manifest.json"
DOC_RELPATH = "docs/assets/THIRD_PARTY_ASSETS.md"

HEADER = """\
# Third-party assets

<!-- GENERATED FILE — DO NOT EDIT.
     Regenerate with: cd reconstruction && uv run python scripts/generate_attribution.py
     Source of truth: app/Assets/ThirdParty/manifest.json -->

Every third-party asset in this repository is inventoried in
`app/Assets/ThirdParty/manifest.json` and validated by
`reconstruction/scripts/validate_assets.py` (run as part of the
reconstruction test suite). Only CC0 and CC-BY licensed assets are
accepted; Unity Asset Store and Mixamo content is rejected by the
validator and is never a required build input.
"""


def attribution_line(asset: dict) -> str:
    line = (
        f"**“{asset['title']}”** by **{asset['author']}**, licensed under "
        f"[{asset['license']}]({asset['licenseUrl']}). "
        f"Source: <{asset['sourceUrl']}>."
    )
    if asset["modified"]:
        line += f" Modified by the Battle Atlas project: {asset['modifications']}."
    return line


def render(manifest: dict) -> str:
    parts = [HEADER]
    for asset in sorted(manifest["assets"], key=lambda a: a["id"]):
        parts.append(f"\n## {asset['title']} (`{asset['id']}`)\n")
        parts.append(attribution_line(asset) + "\n")
        parts.append(
            f"\n- Path: `app/{asset['path']}`\n"
            f"- Acquired: {asset['acquired']} from <{asset['downloadUrl']}>\n"
            f"- Download sha256: `{asset['downloadSha256']}`\n"
            f"- Imported content sha256: `{asset['sha256']}`\n"
            f"- License evidence as archived on acquisition day: "
            f"[`{asset['licenseEvidence']}`](../../{asset['licenseEvidence']})\n"
        )
    if not manifest["assets"]:
        parts.append("\nNo third-party assets are currently manifested.\n")
    return "".join(parts)


def main(argv: list[str]) -> int:
    args = [a for a in argv[1:] if a != "--check"]
    check = "--check" in argv[1:]
    if len(args) > 1:
        print(__doc__, file=sys.stderr)
        return 2
    repo_root = Path(args[0]).resolve() if args else DEFAULT_REPO

    manifest = json.loads((repo_root / MANIFEST_RELPATH).read_text())
    doc_path = repo_root / DOC_RELPATH
    expected = render(manifest)

    if check:
        if not doc_path.exists():
            print(f"{DOC_RELPATH} is missing; regenerate it", file=sys.stderr)
            return 1
        if doc_path.read_text() != expected:
            print(f"{DOC_RELPATH} is stale; regenerate it "
                  "(never hand-edit, plan section 11)", file=sys.stderr)
            return 1
        print(f"OK: {DOC_RELPATH} is current")
        return 0

    doc_path.parent.mkdir(parents=True, exist_ok=True)
    doc_path.write_text(expected)
    print(f"wrote {DOC_RELPATH} ({len(manifest['assets'])} asset(s))")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
