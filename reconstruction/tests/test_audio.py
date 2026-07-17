"""Phase 9 audio: pack integrity (plan §11 discipline applied to §9.3
sources) and deterministic stem synthesis (ED-23: the mix is a pure
function of the compiled event streams and the battle seed)."""

import hashlib
import json
import sys
import wave
from pathlib import Path

import numpy as np
import pytest

RECON = Path(__file__).resolve().parent.parent
REPO = RECON.parent
sys.path.insert(0, str(RECON / "scripts"))

import build_viewpoint_audio as bva  # noqa: E402
import fetch_audio_pack as fap  # noqa: E402

LOCK = json.loads((RECON / "audio/freesound-pack.lock.json").read_text())
MANIFEST = json.loads(
    (REPO / "app/Assets/ThirdParty/manifest.json").read_text())
SR = bva.SR


# --------------------------------------------------------------------------
# Pack integrity (offline: committed bytes against the committed lock)
# --------------------------------------------------------------------------

def test_every_pack_source_is_locked_and_manifested():
    ids = {a["id"] for a in MANIFEST["assets"]}
    for entry in fap.PACK:
        assert entry["id"] in LOCK, f"{entry['id']} missing from lock"
        assert entry["id"] in ids, f"{entry['id']} missing from manifest"


def test_committed_clips_match_locked_digests():
    for aid, rec in LOCK.items():
        assert rec["outputs"], f"{aid}: no outputs locked"
        for rel, digest in rec["outputs"].items():
            p = REPO / rel
            assert p.exists(), f"{aid}: missing committed clip {rel}"
            actual = hashlib.sha256(p.read_bytes()).hexdigest()
            assert actual == digest, f"{aid}: {rel} content drift"


def test_license_evidence_archived_per_source():
    for entry in fap.PACK:
        ev = REPO / "docs/assets/licenses" / entry["id"]
        acq = json.loads((ev / "acquisition.json").read_text())
        assert acq["license"] == "CC0-1.0"
        assert "zero/1.0" in acq["licenseUrl"]
        assert (ev / "freesound-page.html").stat().st_size > 10_000, \
            f"{entry['id']}: archived page looks empty"
        assert (ev / "downloads.sha256").read_text().split()[0] == \
            LOCK[entry["id"]]["previewSha256"]


def test_audio_manifest_entries_are_cc0_with_pinned_downloads():
    audio = [a for a in MANIFEST["assets"]
             if a["path"].startswith("Assets/ThirdParty/Audio/")]
    assert len(audio) == len(fap.PACK)
    for a in audio:
        assert a["license"] == "CC0-1.0", a["id"]
        assert a["redistributable"] is True
        assert a["downloadUrl"].startswith("https://cdn.freesound.org/")
        assert len(a["downloadSha256"]) == 64
        assert a["modified"] is True and "fetch_audio_pack" in a["modifications"]


# --------------------------------------------------------------------------
# Deterministic stem synthesis (synthetic pack + synthetic events)
# --------------------------------------------------------------------------

def write_wav(path: Path, x: np.ndarray):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(
            np.clip(np.round(x * 32767), -32768, 32767)
            .astype(np.int16).tobytes())


@pytest.fixture()
def mini_pack(tmp_path):
    """A pack of dirac-ish clicks under every prefix build() consumes."""
    root = tmp_path / "pack"
    click = np.zeros(SR // 10, dtype=np.float32)
    click[0] = 0.9
    bed = np.full(SR * 2, 0.05, dtype=np.float32)
    for cat, name in [
        ("Musket", "fs-musket-x/fs-musket-x.wav"),
        ("Cannon", "fs-cannon-tman95-x/fs-cannon-tman95-x.wav"),
        ("Cannon", "fs-cannon-andykub-x/fs-cannon-andykub-x.wav"),
        ("Cannon", "fs-cannon-vishwajay-x/fs-cannon-vishwajay-x.wav"),
        ("Cannon", "fs-cannon-qubodup-x/fs-cannon-qubodup-x.wav"),
        ("Cannon", "fs-cannon-distant-x/fs-cannon-distant-x.wav"),
        ("Projectile", "fs-ricochet-x/fs-ricochet-x.wav"),
        ("Strike", "fs-dirt-x/fs-dirt-x.wav"),
        ("Movement", "fs-step-x/fs-step-x.wav"),
        ("Movement", "fs-walk-x/fs-walk-x.wav"),
        ("Movement", "fs-run-x/fs-run-x.wav"),
        ("Movement", "fs-rail-x/fs-rail-x.wav"),
        ("Voice", "fs-shout-x/fs-shout-x.wav"),
        ("Voice", "fs-groan-x/fs-groan-x.wav"),
    ]:
        write_wav(root / cat / name, click)
    write_wav(root / "Voice/fs-mob-x/fs-mob-x.wav", bed)
    write_wav(root / "Voice/fs-breath-x/fs-breath-x.wav", bed)
    write_wav(root / "Ambience/fs-meadow-trp-x/fs-meadow-trp-x.wav", bed)
    write_wav(root / "Ambience/fs-meadow-wind-x/fs-meadow-wind-x.wav", bed)
    return root


def events_doc(**over):
    doc = {
        "viewpoint": "test-vp",
        "seed": "test-seed",
        "window": {"t0": 100.0, "t1": 110.0},
        "observer": [
            {"t": 100.0 + i, "x": 0.0, "z": 0.0, "headingDeg": 0.0,
             "chaos": 0.0, "clip": "March", "crossing": False}
            for i in range(12)
        ],
        "footfalls": [],
        "crossings": [],
        "musketDischarges": [],
        "cannonDischarges": [],
        "strikes": [],
        "casualties": [],
        "observerSegments": [],
    }
    doc.update(over)
    return doc


def load_stem(out: Path, name: str) -> np.ndarray:
    with wave.open(str(out / f"{name}.wav"), "rb") as w:
        x = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16)
    return x.reshape(-1, 2).astype(np.float32) / 32768.0


def test_distance_delay_is_speed_of_sound(mini_pack, tmp_path):
    # a cannon 343 m due north fires at t=101 -> report arrives t=102
    doc = events_doc(cannonDischarges=[
        {"t": 101.0, "x": 0.0, "z": 343.0, "side": "union", "unitId": "b"}])
    out = tmp_path / "out"
    bva.build(doc, mini_pack, out, None, None)
    art = load_stem(out, "artillery")
    mono = np.abs(art).sum(axis=1)
    onset = int(np.argmax(mono > 1e-4))
    assert abs(onset - 2.0 * SR) < SR * 0.01, \
        f"onset at {onset / SR:.3f} s, expected 2.000 s"


def test_pan_follows_bearing(mini_pack, tmp_path):
    # heading north; a musket due EAST must land mostly in the right ear
    doc = events_doc(musketDischarges=[
        {"t": 100.5, "x": 40.0, "z": 0.0, "side": "union"}])
    out = tmp_path / "out"
    bva.build(doc, mini_pack, out, None, None)
    mus = load_stem(out, "musketry")
    left = float(np.abs(mus[:, 0]).sum())
    right = float(np.abs(mus[:, 1]).sum())
    assert right > left * 2.0


def test_attenuation_monotonic_with_distance(mini_pack, tmp_path):
    outs = []
    for dist in (20.0, 200.0):
        doc = events_doc(musketDischarges=[
            {"t": 100.5, "x": 0.0, "z": dist, "side": "confederate"}])
        out = tmp_path / f"out{int(dist)}"
        bva.build(doc, mini_pack, out, None, None)
        outs.append(float(np.abs(load_stem(out, "musketry")).max()))
    assert outs[0] > outs[1] * 2.0


def test_build_is_byte_deterministic(mini_pack, tmp_path):
    doc = events_doc(
        cannonDischarges=[
            {"t": 101.0, "x": 100.0, "z": 50.0, "side": "union", "unitId": "b"}],
        musketDischarges=[
            {"t": 100.5 + 0.1 * i, "x": 30.0 * (i % 5), "z": 60.0,
             "side": "union"} for i in range(40)],
        footfalls=[{"t": 100.2 + 0.4 * i, "clip": "March"} for i in range(20)],
        casualties=[{"t": 103.0, "x": 3.0, "z": 2.0, "cause": "Musketry",
                     "crawls": True, "side": "confederate"}],
        strikes=[{"t": 104.0, "x": 5.0, "z": 5.0, "unitId": "csa-garnett"}],
        observerSegments=[{"id": "s", "t0": 102.0, "t1": 109.0,
                           "action": "take_canister"}],
    )
    d1 = bva.build(doc, mini_pack, tmp_path / "a", None, None)
    d2 = bva.build(doc, mini_pack, tmp_path / "b", None, None)
    assert d1 == d2
    for name in d1:
        b1 = (tmp_path / "a" / f"{name}.wav").read_bytes()
        b2 = (tmp_path / "b" / f"{name}.wav").read_bytes()
        assert b1 == b2, f"stem {name} not byte-identical"


def test_sub_window_build_matches_gate_usage(mini_pack, tmp_path):
    doc = events_doc(musketDischarges=[
        {"t": 104.0, "x": 0.0, "z": 30.0, "side": "union"}])
    out = tmp_path / "out"
    digests = bva.build(doc, mini_pack, out, 103.0, 106.0)
    assert set(digests) == set(bva.STEM_GAIN) | {"mix"}
    mus = load_stem(out, "musketry")
    assert len(mus) == int((106.0 - 103.0 + bva.TAIL_S) * SR)
    assert float(np.abs(mus).max()) > 0.0


def test_wounded_voices_are_sparse_and_nearby_only(mini_pack, tmp_path):
    # far casualties never voice; near ones only through the hash gate
    doc = events_doc(casualties=[
        {"t": 102.0, "x": 500.0, "z": 0.0, "cause": "Canister",
         "crawls": False, "side": "confederate"}])
    out = tmp_path / "out"
    bva.build(doc, mini_pack, out, None, None)
    vw = load_stem(out, "voices_wounded")
    assert float(np.abs(vw).max()) == 0.0


# --------------------------------------------------------------------------
# Defending-observer mixes (webb-wall / cushing-canister slice): newer
# event exports carry observerUnit/observerSide; legacy garnett exports
# have neither and must keep their shipped byte behavior.
# --------------------------------------------------------------------------

def test_whiz_layer_keys_on_enemy_fire_relative_to_observer_side(
        mini_pack, tmp_path):
    # Union observer: CONFEDERATE fire whizzes, Union fire does not.
    # (Many discharges so at least one passes the 5% hash gate.)
    shots = [{"t": 100.3 + 0.02 * i, "x": 0.0, "z": 60.0,
              "side": "confederate"} for i in range(200)]
    doc = events_doc(observerUnit="us-71pa", observerSide="union",
                     musketDischarges=shots)
    out_enemy = tmp_path / "enemy"
    bva.build(doc, mini_pack, out_enemy, None, None)
    assert float(np.abs(load_stem(out_enemy, "projectiles")).max()) > 0.0

    for s in shots:
        s["side"] = "union"  # friendly fire: never a pass-by
    out_friendly = tmp_path / "friendly"
    bva.build(doc, mini_pack, out_friendly, None, None)
    assert float(np.abs(load_stem(out_friendly, "projectiles")).max()) == 0.0


def test_strikes_near_observer_are_kept_regardless_of_unit_when_tagged(
        mini_pack, tmp_path):
    # With observerUnit present, a compiled impact 7 m away is audible even
    # when it belongs to ANOTHER unit's strike stream (the receiving line
    # hears the canister tearing ground in front of the wall)...
    strike = [{"t": 104.0, "x": 5.0, "z": 5.0, "unitId": "csa-garnett"}]
    doc = events_doc(observerUnit="us-71pa", observerSide="union",
                     strikes=strike)
    out = tmp_path / "tagged"
    bva.build(doc, mini_pack, out, None, None)
    assert float(np.abs(load_stem(out, "strikes")).max()) > 0.0

    # ...while a legacy (untagged) export keeps the garnett-era own-unit
    # filter byte-exactly: the same impact under another unitId is dropped.
    legacy = events_doc(strikes=[
        {"t": 104.0, "x": 5.0, "z": 5.0, "unitId": "us-webb"}])
    out2 = tmp_path / "legacy"
    bva.build(legacy, mini_pack, out2, None, None)
    assert float(np.abs(load_stem(out2, "strikes")).max()) == 0.0


def test_defender_segment_actions_carry_shout_bursts(mini_pack, tmp_path):
    # fire_by_rank / fire_independent / close_gap are NOISY_ACTIONS now
    # (defending observers); the garnett segment vocabulary is unaffected.
    assert {"fire_by_rank", "fire_independent", "close_gap"} <= bva.NOISY_ACTIONS
    doc = events_doc(observerUnit="us-71pa", observerSide="union",
                     observerSegments=[{"id": "s", "t0": 102.0, "t1": 109.0,
                                        "action": "fire_by_rank"}])
    out = tmp_path / "out"
    bva.build(doc, mini_pack, out, None, None)
    assert float(np.abs(load_stem(out, "voices_unit")).max()) > 0.0
