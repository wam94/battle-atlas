"""The Iverson's-field (Oak Ridge, July 1) Soldier View corpus: validation,
decomposition reconciliation, and byte-deterministic compilation — the same
contract test_reconstruction_v2.py holds over the Angle corpus, over the
NEW July 1 files (which never touch the pinned Angle inputs)."""
import copy
import json
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "scripts"))

import compile_iverson  # noqa: E402
import validate_iverson as vi  # noqa: E402
import validate_reconstruction as vr  # noqa: E402

REPO = Path(__file__).resolve().parents[2]
BUNDLE = REPO / "app" / "Assets" / "Battle" / "Iverson" / "iverson.bundle.json"
VIEWPOINTS = REPO / "app" / "Assets" / "StreamingAssets" / "SoldierView" / "viewpoints.json"


@pytest.fixture(scope="module")
def corpus():
    return vi.load_corpus()


def test_committed_corpus_validates(corpus):
    assert vi.validate_iverson(corpus) == []


def test_angle_extent_constant_is_restored_after_validation(corpus):
    before = vr.BATTLEFIELD_EXTENT_M
    vi.validate_iverson(corpus)
    assert vr.BATTLEFIELD_EXTENT_M == before


def test_regiment_sum_deviation_rejected(corpus):
    broken = copy.deepcopy(corpus)
    for p in broken.recon["casualtyProfiles"]:
        if p["id"] == "cas-5nc-fight":
            p["count"] += 10
    errors = vi.validate_iverson(broken)
    assert any("decomposition[csa-iverson]" in e for e in errors)


def test_slice_ends_before_the_surrender(corpus):
    # The capture of the lying line (claim-iv-surrender-mass, preferred
    # t=7200) is a named vocabulary gap: the slice must end before it.
    assert corpus.recon["slice"]["t1"] <= 7100
    claim = next(c for c in corpus.claims["claims"]
                 if c["id"] == "claim-iv-surrender-mass")
    assert corpus.recon["slice"]["t1"] <= claim["time"]["preferred"]


def test_left_three_regiments_carry_the_dress_parade_500(corpus):
    losses = {}
    for p in corpus.recon["casualtyProfiles"]:
        losses[p["unitId"]] = losses.get(p["unitId"], 0) + p["count"]
    on_the_line = sum(
        p["count"] for p in corpus.recon["casualtyProfiles"]
        if p["unitId"] in ("csa-5nc", "csa-20nc", "csa-23nc", "csa-12nc")
        and p["t0"] >= 6000)
    # the brigade's loss ON the halted line (t>=6000) is 525 — the
    # macro-track-forced quantity nearest the attested '500 of my men ...
    # on a line as straight as a dress parade'; the left three carry 485
    # of it, the 12th NC 40
    assert on_the_line == 525
    left_three_fight = sum(
        p["count"] for p in corpus.recon["casualtyProfiles"]
        if p["unitId"] in ("csa-5nc", "csa-20nc", "csa-23nc")
        and p["t0"] >= 6000)
    assert left_three_fight == 485
    assert losses["csa-12nc"] < 60  # the documented survivor regiment


def test_compile_is_byte_deterministic(corpus):
    a = compile_iverson.dump_bundle(compile_iverson.compile_bundle(corpus))
    b = compile_iverson.dump_bundle(compile_iverson.compile_bundle(corpus))
    assert a == b


def test_committed_bundle_matches_recompilation(corpus):
    committed = BUNDLE.read_text()
    fresh = compile_iverson.dump_bundle(compile_iverson.compile_bundle(corpus))
    assert committed == fresh, (
        "committed iverson.bundle.json is stale; regenerate with "
        "`uv run python scripts/compile_iverson.py`")


def test_bundle_never_touches_angle_paths():
    bundle = json.loads(BUNDLE.read_text())
    assert bundle["inputs"]["claims"] != "", "inputs recorded"
    for key, path in (
        ("sources", vi.SOURCES_PATH), ("claims", vi.CLAIMS_PATH),
        ("reconstruction", vi.RECON_PATH), ("landcover", vi.LANDCOVER_PATH),
    ):
        assert "angle" not in path.name, f"{key} must come from July 1 files"
    # and the staging seed is the proof pin, not the Angle's ED-21 seed
    assert bundle["stagingSeed"] == compile_iverson.STAGING_SEED
    assert bundle["stagingSeed"] != "d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1"


def test_viewpoint_window_inside_bundle_slice():
    bundle = json.loads(BUNDLE.read_text())
    doc = json.loads(VIEWPOINTS.read_text())
    vp = next(v for v in doc["viewpoints"] if v["id"] == "iverson-forney-field")
    assert vp["unitId"] == "csa-12nc"
    assert vp["t0"] >= bundle["slice"]["t0"]
    # media contract: at least 0.5 s of renderable data past t1 for the
    # end-guard padding (p1-media-contract.md)
    assert vp["t1"] + 0.5 <= bundle["slice"]["t1"]


def test_observer_slot_is_rear_rank():
    bundle = json.loads(BUNDLE.read_text())
    twelfth = next(u for u in bundle["units"] if u["unitId"] == "csa-12nc")
    files = (twelfth["startStrength"] + 1) // 2
    doc = json.loads(VIEWPOINTS.read_text())
    vp = next(v for v in doc["viewpoints"] if v["id"] == "iverson-forney-field")
    assert vp["slotId"] // files == 1, "rear rank (the P9 lesson)"


def test_lying_down_claim_compiles_to_fight_prone(corpus):
    """The fight-prone wiring (T5 vocabulary gap #1): every fire segment
    carrying claim-iv-lying-down compiles to the fight_prone action class;
    every segment WITHOUT the claim keeps its corpus action. The 12th NC —
    the survivor regiment the record keeps standing — must stay a standing
    fire_independent."""
    bundle = compile_iverson.compile_bundle(corpus)
    src_by_id = {s["id"]: s for s in corpus.recon["segments"]}
    prone_ids = set()
    for u in bundle["units"]:
        for s in u["segments"]:
            src = src_by_id[s["id"]]
            if compile_iverson.PRONE_CLAIM in src["claimIds"]:
                assert s["action"] == "fight_prone", s["id"]
                prone_ids.add(s["id"])
            else:
                assert s["action"] == src["action"], s["id"]
    assert prone_ids == {"seg-5nc-fight", "seg-20nc-fight", "seg-23nc-fight"}
    twelfth = next(u for u in bundle["units"] if u["unitId"] == "csa-12nc")
    fight = next(s for s in twelfth["segments"] if s["id"] == "seg-12nc-fight")
    assert fight["action"] == "fire_independent"
