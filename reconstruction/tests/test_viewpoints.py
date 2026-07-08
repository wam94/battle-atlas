import copy
import json
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
from validate_viewpoints import validate_document  # noqa: E402

REPO = Path(__file__).resolve().parent.parent.parent
COMMITTED = REPO / "app/Assets/StreamingAssets/SoldierView/viewpoints.json"


def valid_doc() -> dict:
    return {
        "viewpoints": [
            {
                "id": "dev-timecode",
                "title": "Dev: timecode proxy",
                "unitId": "csa-garnett",
                "slotId": 0,
                "viewKind": "first_person",
                "t0": 8160,
                "t1": 8170,
                "camera": {
                    "eyeHeightM": 1.66,
                    "fovDeg": 68,
                    "lookOffsetDeg": [0, 0],
                    "stabilization": 0.0,
                },
                "media": {
                    "proxy": "SoldierView/dev-timecode.proxy.mp4",
                    "full": None,
                    "fps": 30,
                    "width": 1280,
                    "height": 720,
                },
                "claimIds": [],
                "editorialNote": "Phase 1 media-contract proof; synthetic timecode video.",
            }
        ]
    }


def test_committed_viewpoints_file_is_valid():
    doc = json.loads(COMMITTED.read_text())
    assert validate_document(doc) == []


def test_valid_document_passes():
    assert validate_document(valid_doc()) == []


def test_t1_must_exceed_t0():
    doc = valid_doc()
    doc["viewpoints"][0]["t1"] = doc["viewpoints"][0]["t0"]
    assert any("t1" in e for e in validate_document(doc))


def test_t1_within_battle_end():
    doc = valid_doc()
    doc["viewpoints"][0]["t1"] = 10801
    assert any("battle end" in e for e in validate_document(doc))


def test_duplicate_ids_rejected():
    doc = valid_doc()
    doc["viewpoints"].append(copy.deepcopy(doc["viewpoints"][0]))
    assert any("duplicate" in e for e in validate_document(doc))


def test_missing_required_field_rejected():
    doc = valid_doc()
    del doc["viewpoints"][0]["media"]["proxy"]
    assert any("proxy" in e for e in validate_document(doc))


def test_bad_view_kind_rejected():
    doc = valid_doc()
    doc["viewpoints"][0]["viewKind"] = "free_camera"
    assert validate_document(doc) != []


def test_absolute_media_path_rejected():
    doc = valid_doc()
    doc["viewpoints"][0]["media"]["proxy"] = "/tmp/evil.mp4"
    assert any("relative path" in e for e in validate_document(doc))


def test_negative_slot_rejected():
    doc = valid_doc()
    doc["viewpoints"][0]["slotId"] = -1
    assert validate_document(doc) != []


def test_camera_bounds_enforced():
    doc = valid_doc()
    doc["viewpoints"][0]["camera"]["fovDeg"] = 179
    assert validate_document(doc) != []
