"""Phase 5 (Reconstruction V2) tests: corpus validation, negative fixtures for
every cross-file rule, deterministic compilation, and the Gate P5 rules
(every compiled second has deterministic state and action; every output traces
to claims and/or named inference rules; no untraced obstacle crossings)."""

import copy
import json
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
from validate_reconstruction import (  # noqa: E402
    Corpus,
    compiled_strength,
    load_corpus,
    macro_strength_at,
    validate_corpus,
)
import compile_angle  # noqa: E402

REPO = Path(__file__).resolve().parent.parent.parent
BUNDLE = REPO / "app/Assets/Battle/Angle/angle.bundle.json"
AUDIT = REPO / "docs/reconstruction/angle-bundle-audit.md"


@pytest.fixture(scope="module")
def corpus() -> Corpus:
    return load_corpus()


@pytest.fixture()
def mutable(corpus) -> Corpus:
    return copy.deepcopy(corpus)


@pytest.fixture(scope="module")
def bundle() -> dict:
    return json.loads(BUNDLE.read_text())


# ---------------------------------------------------------------------------
# Committed corpus is valid

def test_committed_corpus_validates(corpus):
    assert validate_corpus(corpus) == []


def test_required_claim_properties_covered(corpus):
    """Plan section 6.2: the slice's claims must cover every required property."""
    required = {
        "position", "time", "strength", "formation", "movement", "firing",
        "obstacle_crossing", "command_action", "casualty_total",
        "casualty_timing", "land_cover", "structure", "weather",
    }
    present = {c["property"] for c in corpus.claims["claims"]}
    assert required <= present


def test_named_units_are_reconstructed(corpus):
    """Plan Phase 5: Garnett, Armistead, Kemper, Webb, 69th/71st/72nd PA,
    Cushing's battery, and directly visible supporting units."""
    uids = {u["unitId"] for u in corpus.recon["units"]}
    assert {
        "csa-garnett", "csa-armistead", "csa-kemper",
        "us-webb", "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing",
    } <= uids


def test_every_documented_claim_has_reference_with_interpretation(corpus):
    for c in corpus.claims["claims"]:
        if c["assessment"] == "documented":
            assert c["references"], c["id"]
            for r in c["references"]:
                assert r["interpretation"], c["id"]


def test_excerpts_stay_rights_compliant(corpus):
    for c in corpus.claims["claims"]:
        for r in c["references"]:
            assert len(r.get("excerpt", "")) <= 300, c["id"]


# ---------------------------------------------------------------------------
# Negative fixtures: every validator rule can actually fail

def test_dangling_source_reference_rejected(mutable):
    mutable.claims["claims"][0]["references"] = [
        {"sourceId": "no-such-source", "interpretation": "x"}]
    assert any("unknown source" in e for e in validate_corpus(mutable))


def test_documented_claim_without_reference_rejected(mutable):
    for c in mutable.claims["claims"]:
        if c["assessment"] == "documented":
            c["references"] = []
            break
    assert any("documented claims require" in e for e in validate_corpus(mutable))


def test_clock_profile_offset_outside_envelope_rejected(mutable):
    for s in mutable.sources["sources"]:
        if s["id"] == "haskell-1908":
            s["clockProfile"]["offsetMinutes"] = 99
            break
    assert any("outside offsetEnvelope" in e for e in validate_corpus(mutable))


def test_clock_profile_requires_offset_and_assessment(mutable):
    for s in mutable.sources["sources"]:
        if s["id"] == "jacobs-1864":
            s["clockProfile"] = {"kind": "watch-checked"}
            break
    errs = validate_corpus(mutable)
    assert any("requires offsetMinutes" in e for e in errs)
    assert any("requires an assessment" in e for e in errs)


def test_committed_clock_profiles_present(corpus):
    """ED-25: the five worked profile classes are on the source records."""
    by = {s["id"]: s.get("clockProfile") for s in corpus.sources["sources"]}
    assert by["jacobs-1864"]["kind"] == "contemporaneous-civilian"
    assert by["haskell-1908"]["kind"] == "watch-checked"
    assert by["haskell-1908"]["offsetMinutes"] == 7
    assert by["alexander-1907"]["kind"] == "retrospective-watch"
    assert by["or-27-2-longstreet"]["kind"] == "report-nominal"
    assert by["stone-sentinels"]["kind"] == "tablet-adjudicated"


def test_clock_profile_anchors_used_admits_split_anchor_ids(mutable):
    """Pass-11 flagged follow-up, closed pass 12: ED-66 split CA-J1M-4 into
    CA-J1M-4a/4b, but the anchorsUsed pattern only admitted bare-digit anchor
    ids. A profile citing the split ids must validate cleanly."""
    for s in mutable.sources["sources"]:
        if s["id"] == "jacobs-1864":
            s["clockProfile"]["anchorsUsed"] = ["CA-J1M-4a", "CA-J1M-4b", "CA-J3A-1"]
            break
    assert validate_corpus(mutable) == []


def test_clock_profile_anchors_used_still_rejects_malformed_ids(mutable):
    for s in mutable.sources["sources"]:
        if s["id"] == "jacobs-1864":
            s["clockProfile"]["anchorsUsed"] = ["CA-J1M-"]
            break
    errs = validate_corpus(mutable)
    assert any("anchorsUsed" in e for e in errs)


def test_time_envelope_ordering_enforced(mutable):
    for c in mutable.claims["claims"]:
        if "time" in c:
            c["time"]["earliest"] = c["time"]["latest"] + 1
            break
    assert any("earliest <= preferred <= latest" in e for e in validate_corpus(mutable))


def test_unknown_claim_subject_rejected(mutable):
    mutable.claims["claims"][0]["subjectId"] = "csa-nonexistent"
    assert any("not a battle unit" in e for e in validate_corpus(mutable))


def test_segment_citing_unknown_claim_rejected(mutable):
    mutable.recon["segments"][0]["claimIds"] = ["claim-does-not-exist"]
    assert any("unknown claim" in e for e in validate_corpus(mutable))


def test_segment_gap_rejected(mutable):
    segs = [s for s in mutable.recon["segments"] if s["unitId"] == "csa-garnett"]
    segs[1]["t0"] += 10  # open a gap after the first segment
    errors = validate_corpus(mutable)
    assert any("gap" in e for e in errors)


def test_segment_overlap_rejected(mutable):
    segs = [s for s in mutable.recon["segments"] if s["unitId"] == "csa-garnett"]
    segs[1]["t0"] -= 10
    assert any("overlap" in e for e in validate_corpus(mutable))


def test_slice_coverage_enforced(mutable):
    segs = sorted((s for s in mutable.recon["segments"] if s["unitId"] == "us-webb"),
                  key=lambda s: s["t0"])
    segs[-1]["t1"] -= 30
    assert any("not slice t1" in e for e in validate_corpus(mutable))


def test_route_discontinuity_rejected(mutable):
    segs = sorted((s for s in mutable.recon["segments"] if s["unitId"] == "csa-kemper"),
                  key=lambda s: s["t0"])
    segs[1]["route"][0] = [segs[1]["route"][0][0] + 25, segs[1]["route"][0][1]]
    assert any("route discontinuity" in e for e in validate_corpus(mutable))


def test_formation_discontinuity_rejected(mutable):
    segs = sorted((s for s in mutable.recon["segments"] if s["unitId"] == "csa-kemper"),
                  key=lambda s: s["t0"])
    segs[1]["formationFrom"] = "routed"
    assert any("formation discontinuity" in e for e in validate_corpus(mutable))


def test_out_of_bounds_route_rejected(mutable):
    seg = mutable.recon["segments"][0]
    seg["route"] = [[-5.0, seg["route"][0][1]]] + seg["route"][1:]
    assert any("outside the battlefield" in e for e in validate_corpus(mutable))


def test_infantry_speed_cap_enforced(mutable):
    for s in mutable.recon["segments"]:
        if s["id"] == "seg-garnett-advance-to-wall":
            s["t1"] = s["t0"] + 60  # 283 m in 60 s = 4.7 m/s
        if s["id"] == "seg-garnett-take-canister-at-wall":
            s["t0"] = 8400  # keep contiguity
    assert any("exceeds the infantry cap" in e for e in validate_corpus(mutable))


def test_artillery_speed_cap_enforced(mutable):
    for s in mutable.recon["segments"]:
        if s["id"] == "seg-cowan-canister":
            s["route"] = [s["route"][0], [s["route"][0][0] + 400, s["route"][0][1]]]
            s["action"] = "fall_back"  # movement action; still capped for artillery
    errors = validate_corpus(mutable)
    assert any("artillery cap" in e for e in errors)


def test_untraced_wall_crossing_rejected(mutable):
    """Gate P5: a straight segment through the stone wall without an authored
    crossing action must fail."""
    for s in mutable.recon["segments"]:
        if s["id"] == "seg-armistead-breach":
            del s["obstacleIds"]
    errors = validate_corpus(mutable)
    assert any("crosses traced obstacle 'wall-angle-webb-front' without an authored crossing"
               in e for e in errors)


def test_crossing_on_non_crossing_action_rejected(mutable):
    for s in mutable.recon["segments"]:
        if s["id"] == "seg-armistead-breach":
            s["action"] = "advance"
    errors = validate_corpus(mutable)
    assert any("may not carry an obstacle crossing" in e for e in errors)


def test_phantom_crossing_declaration_rejected(mutable):
    for s in mutable.recon["segments"]:
        if s["id"] == "seg-garnett-fall-back":
            s["obstacleIds"] = ["fence-emmitsburg-road-west"]
            s["inferenceRules"] = list(s["inferenceRules"]) + ["R-retreat-recross"]
    errors = validate_corpus(mutable)
    assert any("phantom crossing" in e for e in errors)


def test_retreat_recross_requires_named_rule(mutable):
    for s in mutable.recon["segments"]:
        if s["id"] == "seg-armistead-fall-back":
            s["inferenceRules"] = [r for r in s["inferenceRules"] if r != "R-retreat-recross"]
    errors = validate_corpus(mutable)
    assert any("requires the R-retreat-recross" in e for e in errors)


def test_segment_without_claims_or_rules_rejected(mutable):
    mutable.recon["segments"][0]["claimIds"] = []
    mutable.recon["segments"][0]["inferenceRules"] = []
    assert any("editorial connective reconstruction" in e for e in validate_corpus(mutable))


def test_cross_unit_claim_citation_rejected(mutable):
    for s in mutable.recon["segments"]:
        if s["unitId"] == "us-hall":
            s["claimIds"] = ["claim-strength-garnett"]
            break
    assert any("about a different unit" in e for e in validate_corpus(mutable))


def test_cause_mix_must_sum_to_one(mutable):
    mutable.recon["casualtyProfiles"][0]["causeMix"] = {"musketry": 0.5, "unknown": 0.4}
    assert any("causeMix sums to" in e for e in validate_corpus(mutable))


def test_casualties_cannot_exceed_strength(mutable):
    for p in mutable.recon["casualtyProfiles"]:
        if p["unitId"] == "us-btty-cushing":
            p["count"] += 500
            break
    errors = validate_corpus(mutable)
    assert any("exceed start strength" in e for e in errors)


def test_strength_reconciliation_enforced(mutable):
    for p in mutable.recon["casualtyProfiles"]:
        if p["id"] == "cas-garnett-final-approach":
            p["count"] -= 50
    errors = validate_corpus(mutable)
    assert any("deviates from macro track" in e for e in errors)


def test_casualty_profile_overlap_rejected(mutable):
    profs = [p for p in mutable.recon["casualtyProfiles"] if p["unitId"] == "csa-garnett"]
    profs[1]["t0"] = profs[0]["t0"] + 1
    # keep reconciliation from masking the overlap error we're after
    errors = validate_corpus(mutable)
    assert any("overlap" in e for e in errors)


# ---------------------------------------------------------------------------
# Compilation: determinism and Gate P5

def test_compile_is_deterministic(corpus):
    b1 = compile_angle.compile_bundle(corpus)
    b2 = compile_angle.compile_bundle(copy.deepcopy(corpus))
    assert compile_angle.dump_bundle(b1) == compile_angle.dump_bundle(b2)
    assert b1["checksum"] == b2["checksum"]


def test_committed_bundle_matches_recompilation(corpus):
    """The committed artifact is exactly what the committed corpus compiles to."""
    fresh = compile_angle.dump_bundle(compile_angle.compile_bundle(corpus))
    assert BUNDLE.read_text() == fresh


def test_committed_audit_matches_regeneration(corpus, bundle):
    assert AUDIT.read_text() == compile_angle.build_audit(corpus, bundle)


def test_staging_seed_pinned_and_provenance_stable(bundle, corpus):
    """ED-21: the staging seed is the pinned shipped value, and recompiling
    with provenance-only differences cannot move it (it is a compiler
    constant, independent of every input hash)."""
    assert bundle["stagingSeed"] == compile_angle.STAGING_SEED
    assert len(bundle["stagingSeed"]) == 64
    assert bundle["stagingSeed"] != bundle["checksum"] or \
        bundle["inputs"] == {}  # seed stays put while the checksum drifts


def test_bundle_checksum_self_consistent(bundle):
    assert compile_angle.bundle_checksum(bundle) == bundle["checksum"]


def test_every_second_has_state_and_action(bundle):
    """Gate P5: every compiled second from t=8040..9000 has deterministic unit
    state and action for every unit."""
    t0, t1 = bundle["slice"]["t0"], bundle["slice"]["t1"]
    n = t1 - t0 + 1
    assert n == 961
    for u in bundle["units"]:
        ps = u["perSecond"]
        assert ps["t0"] == t0
        for key in ("x", "z", "facingDeg", "strength", "segmentIndex"):
            assert len(ps[key]) == n, (u["unitId"], key)
        # Each second maps to a segment, which carries the action.
        for t_off, si in enumerate(ps["segmentIndex"]):
            seg = u["segments"][si]
            assert seg["t0"] <= t0 + t_off <= seg["t1"], u["unitId"]
            assert seg["action"]


def test_every_output_traces_to_claims_or_rules(bundle):
    """Gate P5: every segment cites claims and/or names inference rules, and
    every cited claim resolves in the bundle's claims index."""
    for u in bundle["units"]:
        for s in u["segments"]:
            assert s["claimIds"] or s["inferenceRules"], s["id"]
            for cid in s["claimIds"]:
                assert cid in bundle["claimsIndex"], cid
            assert s["provenance"] in ("documented", "inferred", "editorial")
        for p in u["casualtyProfiles"]:
            for cid in p["claimIds"]:
                assert cid in bundle["claimsIndex"], cid


def test_bundle_strengths_never_negative_and_monotone(bundle):
    for u in bundle["units"]:
        s = u["perSecond"]["strength"]
        assert all(v >= 0 for v in s), u["unitId"]
        assert all(a >= b for a, b in zip(s, s[1:])), u["unitId"]


def test_bundle_strength_matches_macro_keyframes(corpus, bundle):
    macro_units = {u["id"]: u for u in corpus.battle["units"]}
    t0, t1 = bundle["slice"]["t0"], bundle["slice"]["t1"]
    for u in bundle["units"]:
        kfs = macro_units[u["unitId"]]["keyframes"]
        strengths = u["perSecond"]["strength"]
        for kf in kfs:
            if t0 <= kf["t"] <= t1:
                got = strengths[int(kf["t"]) - t0]
                assert abs(got - kf["strength"]) <= 1, (u["unitId"], kf["t"])


def test_bundle_positions_continuous(bundle):
    """No teleports: successive compiled positions move at most ~4 m/s."""
    for u in bundle["units"]:
        xs, zs = u["perSecond"]["x"], u["perSecond"]["z"]
        for i in range(1, len(xs)):
            step = ((xs[i] - xs[i - 1]) ** 2 + (zs[i] - zs[i - 1]) ** 2) ** 0.5
            assert step <= 4.0, (u["unitId"], i, step)


def test_armistead_crossing_is_at_documented_point(bundle):
    """The breach carries the documented crossing claim and reaches the claimed
    point (4415, 4855) +/- its 30 m uncertainty."""
    unit = next(u for u in bundle["units"] if u["unitId"] == "csa-armistead")
    breach = next(s for s in unit["segments"] if s["action"] == "breach")
    assert "claim-armistead-crossed-wall" in breach["claimIds"]
    end = breach["route"][-1]
    assert ((end[0] - 4415) ** 2 + (end[1] - 4855) ** 2) ** 0.5 <= 30


def test_macro_battle_file_untouched_by_compiler(corpus):
    """Phase 5 must not change macro battle behavior: the compiler reads the
    battle file but the bundle lives in its own sidecar."""
    assert compile_angle.BUNDLE_PATH.name == "angle.bundle.json"
    assert "Angle" in str(compile_angle.BUNDLE_PATH)
    assert compile_angle.BATTLE_PATH.read_text().find("angle.bundle") == -1


# ---------------------------------------------------------------------------
# Helper sanity

def test_macro_strength_interpolation():
    kfs = [{"t": 0, "strength": 100}, {"t": 100, "strength": 0}]
    assert macro_strength_at(kfs, 50) == 50
    assert macro_strength_at(kfs, -10) == 100
    assert macro_strength_at(kfs, 500) == 0


def test_cumulative_losses_integrate_exactly():
    profs = [
        {"t0": 0, "t1": 100, "count": 33, "intensityCurve": "rising"},
        {"t0": 100, "t1": 200, "count": 17, "intensityCurve": "spike"},
    ]
    assert compiled_strength(100, profs, 200) == 50
    assert compiled_strength(100, profs, 0) == 100
    seq = [compiled_strength(100, profs, t) for t in range(0, 201)]
    assert all(a >= b for a, b in zip(seq, seq[1:]))
