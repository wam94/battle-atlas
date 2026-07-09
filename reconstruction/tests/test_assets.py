"""Tests for the third-party asset manifest validator and attribution
generator (Angle Reconstruction V2 plan sections 11 and 12 Phase 2).

Fixture tests build a miniature repo tree in tmp_path; the committed-repo
tests validate the real manifest, tree, and generated attribution document
so a clean checkout fails on any manifest violation ("CI enforcement").
"""

import json
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
from validate_assets import (  # noqa: E402
    compute_content_sha256,
    validate_manifest,
    validate_repo,
)
import generate_attribution  # noqa: E402

REPO = Path(__file__).resolve().parent.parent.parent


# ---------------------------------------------------------------- fixtures

def build_repo(tmp_path: Path) -> tuple[Path, dict]:
    """A minimal valid repo: one CC0 asset, one CC-BY asset."""
    root = tmp_path / "repo"
    cc0_dir = root / "app/Assets/ThirdParty/Materials/TestGround"
    ccby_dir = root / "app/Assets/ThirdParty/Models/TestCart"
    cc0_dir.mkdir(parents=True)
    ccby_dir.mkdir(parents=True)
    (cc0_dir / "albedo.png").write_bytes(b"not-really-a-png")
    (cc0_dir / "albedo.png.meta").write_text("guid: 123")  # ignored by digests
    (ccby_dir / "cart.obj").write_bytes(b"o cart\nv 0 0 0\n")
    for evid in ("test-ground", "test-cart"):
        d = root / "docs/assets/licenses" / evid
        d.mkdir(parents=True)
        (d / "acquisition.json").write_text("{}")

    manifest = {
        "assets": [
            {
                "id": "test-ground",
                "path": "Assets/ThirdParty/Materials/TestGround",
                "title": "Test Ground",
                "author": "Example Author",
                "sourceUrl": "https://example.com/test-ground",
                "license": "CC0-1.0",
                "licenseUrl": "https://creativecommons.org/publicdomain/zero/1.0/",
                "acquired": "2026-07-08",
                "redistributable": True,
                "modified": False,
                "modifications": "",
                "sha256": compute_content_sha256(cc0_dir),
                "downloadUrl": "https://example.com/dl/test-ground.zip",
                "downloadSha256": "0" * 64,
                "licenseEvidence": "docs/assets/licenses/test-ground/acquisition.json",
            },
            {
                "id": "test-cart",
                "path": "Assets/ThirdParty/Models/TestCart",
                "title": "Test Cart",
                "author": "Example Modeler",
                "sourceUrl": "https://example.com/test-cart",
                "license": "CC-BY-4.0",
                "licenseUrl": "https://creativecommons.org/licenses/by/4.0/",
                "acquired": "2026-07-08",
                "redistributable": True,
                "modified": True,
                "modifications": "scale correction",
                "sha256": compute_content_sha256(ccby_dir),
                "downloadUrl": "https://example.com/dl/test-cart.obj",
                "downloadSha256": "1" * 64,
                "licenseEvidence": "docs/assets/licenses/test-cart/acquisition.json",
            },
        ]
    }
    (root / "app/Assets/ThirdParty/manifest.json").write_text(json.dumps(manifest))
    return root, manifest


@pytest.fixture()
def repo(tmp_path):
    return build_repo(tmp_path)


# ------------------------------------------------------------ happy paths

def test_valid_fixture_repo_passes(repo):
    root, manifest = repo
    assert validate_manifest(manifest, root) == []
    assert validate_repo(root) == []


def test_repo_without_thirdparty_tree_passes(tmp_path):
    (tmp_path / "app/Assets").mkdir(parents=True)
    assert validate_repo(tmp_path) == []


def test_print_sha_helper_matches_manifest(repo):
    root, manifest = repo
    for asset in manifest["assets"]:
        assert compute_content_sha256(root / "app" / asset["path"]) == asset["sha256"]


# ------------------------------------- section 11 failure conditions

def test_deliberately_unmanifested_fixture_is_rejected(repo):
    """Gate P2: a file under ThirdParty with no manifest owner must fail."""
    root, manifest = repo
    stowaway = root / "app/Assets/ThirdParty/Models/Stowaway/unlicensed.fbx"
    stowaway.parent.mkdir(parents=True)
    stowaway.write_bytes(b"binary junk")
    errors = validate_manifest(manifest, root)
    assert any("unmanifested file" in e and "Stowaway/unlicensed.fbx" in e for e in errors)


def test_thirdparty_content_without_manifest_is_rejected(repo):
    root, _ = repo
    (root / "app/Assets/ThirdParty/manifest.json").unlink()
    errors = validate_repo(root)
    assert any("manifest.json is missing" in e for e in errors)


def test_missing_required_field_fails_schema(repo):
    root, manifest = repo
    del manifest["assets"][0]["licenseUrl"]
    errors = validate_manifest(manifest, root)
    assert any(e.startswith("schema:") and "licenseUrl" in e for e in errors)


@pytest.mark.parametrize("license_id,needle", [
    ("CC-BY-NC-4.0", "NonCommercial"),
    ("CC-BY-ND-4.0", "NoDerivatives"),
    ("Sketchfab-Editorial", "allowlist"),          # source-only / incompatible
    ("unknown", "unknown licenses"),
    ("Unity-Asset-Store-EULA", "Asset Store"),
    ("Mixamo-Terms", "Mixamo"),
])
def test_incompatible_licenses_are_rejected(repo, license_id, needle):
    root, manifest = repo
    manifest["assets"][0]["license"] = license_id
    errors = validate_manifest(manifest, root)
    assert any(needle in e for e in errors), errors


@pytest.mark.parametrize("field", ["sourceUrl", "downloadUrl"])
@pytest.mark.parametrize("host", ["assetstore.unity.com", "www.mixamo.com"])
def test_asset_store_and_mixamo_sources_are_rejected(repo, field, host):
    root, manifest = repo
    manifest["assets"][1][field] = f"https://{host}/whatever"
    errors = validate_manifest(manifest, root)
    assert any("required build input" in e for e in errors), errors


def test_non_redistributable_is_rejected(repo):
    root, manifest = repo
    manifest["assets"][0]["redistributable"] = False
    errors = validate_manifest(manifest, root)
    assert any("redistributable" in e for e in errors)


def test_ccby_without_attribution_fields_is_rejected(repo):
    root, manifest = repo
    manifest["assets"][1]["author"] = "   "  # blank; schema minLength passes
    errors = validate_manifest(manifest, root)
    assert any("attribution text cannot be generated" in e for e in errors)


def test_checksum_drift_without_manifest_update_is_rejected(repo):
    root, manifest = repo
    (root / "app/Assets/ThirdParty/Models/TestCart/cart.obj").write_bytes(b"tampered")
    errors = validate_manifest(manifest, root)
    assert any("checksum mismatch" in e for e in errors)


def test_added_file_inside_owned_path_changes_checksum(repo):
    root, manifest = repo
    (root / "app/Assets/ThirdParty/Models/TestCart/extra.bin").write_bytes(b"x")
    errors = validate_manifest(manifest, root)
    assert any("checksum mismatch" in e for e in errors)


def test_meta_files_do_not_affect_checksum(repo):
    root, manifest = repo
    (root / "app/Assets/ThirdParty/Models/TestCart/cart.obj.meta").write_text("guid: 9")
    assert validate_manifest(manifest, root) == []


def test_missing_license_evidence_is_rejected(repo):
    root, manifest = repo
    (root / "docs/assets/licenses/test-cart/acquisition.json").unlink()
    errors = validate_manifest(manifest, root)
    assert any("licenseEvidence" in e for e in errors)


def test_modified_requires_modifications_text(repo):
    root, manifest = repo
    manifest["assets"][1]["modifications"] = "  "
    errors = validate_manifest(manifest, root)
    assert any("modifications is empty" in e for e in errors)


def test_overlapping_paths_are_rejected(repo):
    root, manifest = repo
    dup = dict(manifest["assets"][1])
    dup["id"] = "test-cart-dup"
    manifest["assets"].append(dup)
    errors = validate_manifest(manifest, root)
    assert any("overlaps" in e for e in errors)


def test_duplicate_ids_are_rejected(repo):
    root, manifest = repo
    manifest["assets"][1]["id"] = manifest["assets"][0]["id"]
    errors = validate_manifest(manifest, root)
    assert any("duplicate asset id" in e for e in errors)


# --------------------------------------------------- attribution document

def test_attribution_doc_generation_and_check(repo):
    root, _ = repo
    assert generate_attribution.main(["prog", str(root)]) == 0
    assert generate_attribution.main(["prog", "--check", str(root)]) == 0
    doc = root / "docs/assets/THIRD_PARTY_ASSETS.md"
    text = doc.read_text()
    assert "Test Cart" in text and "Example Modeler" in text
    assert "CC-BY-4.0" in text
    # hand-editing the generated doc must fail the check
    doc.write_text(text + "\nhand edit\n")
    assert generate_attribution.main(["prog", "--check", str(root)]) == 1


# ------------------------------------------------- committed repo (CI gate)

def test_committed_manifest_and_tree_are_valid():
    assert validate_repo(REPO) == []


def test_committed_attribution_doc_is_current():
    assert generate_attribution.main(["prog", "--check", str(REPO)]) == 0
