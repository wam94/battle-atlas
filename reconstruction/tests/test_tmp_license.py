"""Phase 12 license review: the TextMesh Pro Essential Resources carve-out
(Unity Companion License content in an MIT repo — flagged at Gate P11).
Pins the in-tree notices, the archived license evidence, and the README
statement so the carve-out cannot silently rot."""

import hashlib
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent.parent
TMP = REPO / "app/Assets/TextMesh Pro"
EVIDENCE = REPO / "docs/assets/licenses/unity-tmp-essentials"

UCL_SHA256 = "899de0f883ff19e3b10b4a9d53acc99462e339d61795ff8851e8e4519601ba2d"


def test_tmp_notice_is_committed_with_the_content():
    # UCL §5: the license/copyright notice must accompany all substantial
    # portions of the Software
    notice = (TMP / "LICENSE.md").read_text()
    assert "Unity Companion License" in notice
    assert "Unity Technologies" in notice
    assert "docs/assets/tmp-unity-companion-license.md" in notice


def test_liberation_sans_carries_its_ofl_text():
    ofl = (TMP / "Fonts" / "LiberationSans - OFL.txt").read_text()
    assert "SIL OPEN FONT LICENSE Version 1.1" in ofl
    assert (TMP / "Fonts" / "LiberationSans.ttf").exists()


def test_carveout_record_and_archived_terms_exist():
    record = (REPO / "docs/assets/tmp-unity-companion-license.md").read_text()
    assert "carve-out" in record.lower()
    ucl = EVIDENCE / "unity-companion-license-v1.2.md"
    assert hashlib.sha256(ucl.read_bytes()).hexdigest() == UCL_SHA256, \
        "archived UCL text drifted from the reviewed copy"
    text = ucl.read_text()
    # the two clauses the carve-out decision rests on
    assert "royalty-free copyright license" in text and "distribute" in text
    assert "Unity Companion Use Only" in text
    assert (EVIDENCE / "com.unity.ugui-2.0.0-LICENSE.md").exists()


def test_readme_states_the_carveout():
    readme = (REPO / "README.md").read_text()
    assert "Unity Companion License" in readme
    assert "TextMesh Pro" in readme
