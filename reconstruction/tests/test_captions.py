"""Phase 12 accessibility captions: the committed captions.json is valid,
sober (bracketed non-speech, no invented dialogue), consistent with the
shipped viewpoint window, and — when the gitignored audio-event export is
present — exactly regenerable from it (the credits.json staleness
discipline applied to captions)."""

import json
import sys
from pathlib import Path

import pytest

RECON = Path(__file__).resolve().parent.parent
REPO = RECON.parent
sys.path.insert(0, str(RECON / "scripts"))

import generate_captions as gc  # noqa: E402
import build_viewpoint_audio as bva  # noqa: E402

CAPTIONS_PATH = REPO / "app/Assets/StreamingAssets/SoldierView/captions.json"
VIEWPOINTS_PATH = REPO / "app/Assets/StreamingAssets/SoldierView/viewpoints.json"
EVENTS_PATH = REPO / "docs/benchmarks/captures/p9-gate/p9-audio-events.json"

DOC = json.loads(CAPTIONS_PATH.read_text())


def test_captions_cover_the_hero_viewpoint_window():
    vps = json.loads(VIEWPOINTS_PATH.read_text())["viewpoints"]
    hero = next(v for v in vps if v["id"] == "garnett-road-to-angle")
    assert DOC["viewpointId"] == hero["id"]
    # generated for the production mix window: t0..t1 + the 0.5 s media pad
    assert DOC["window"]["t0"] == hero["t0"]
    assert DOC["window"]["t1"] == hero["t1"] + 0.5


def test_captions_are_sorted_time_coded_and_windowed():
    w0, w1 = DOC["window"]["t0"], DOC["window"]["t1"]
    prev = float("-inf")
    assert DOC["captions"], "no captions"
    for c in DOC["captions"]:
        assert c["t"] >= prev, f"unsorted at t={c['t']}"
        prev = c["t"]
        assert c["dur"] > 0
        # arrival delay may land in the encode tail (build_viewpoint_audio)
        assert w0 <= c["t"] <= w1 + bva.TAIL_S


def test_captions_are_sober_nonspeech_descriptions():
    # §9.2: the audio carries no worded dialogue, so captions must be
    # bracketed non-speech descriptions from the authored vocabulary
    allowed = {
        "orders": {gc.ORDERS_TEXT},
        "wounded": {gc.WOUNDED_TEXT, gc.WOUNDED_AGAIN_TEXT},
    }
    for c in DOC["captions"]:
        assert c["kind"] in allowed, c
        assert c["text"] in allowed[c["kind"]], c
        assert c["text"].startswith("[") and c["text"].endswith("]")


def test_both_voice_layers_are_captioned():
    kinds = {c["kind"] for c in DOC["captions"]}
    assert kinds == {"orders", "wounded"}


@pytest.mark.skipif(not EVENTS_PATH.exists(),
                    reason="gitignored audio-event export not present "
                           "(render-runbook §4 regenerates it)")
def test_committed_captions_match_the_event_export():
    events = json.loads(EVENTS_PATH.read_text())
    regenerated = gc.render(gc.build_captions(
        events, DOC["window"]["t0"], DOC["window"]["t1"]))
    assert CAPTIONS_PATH.read_text() == regenerated, (
        "captions.json is stale — regenerate with "
        "reconstruction/scripts/generate_captions.py")
    # and the generator is deterministic
    assert regenerated == gc.render(gc.build_captions(
        events, DOC["window"]["t0"], DOC["window"]["t1"]))
