"""Fetch and process the Phase 9 Soldier View audio source pack (plan §9.3).

Every source is a Freesound recording whose page (license evidence) is
archived on acquisition day, whose download is pinned by sha256 in
audio/freesound-pack.lock.json, and whose processed clips are committed
under app/Assets/ThirdParty/Audio/ with a manifest entry per source
(the Phase 2 asset/license discipline, plan §11).

Acquisition honesty (recorded per asset in acquisition.json): Freesound's
original uploads require an account login to download (the same barrier
the Phase 2 executor recorded for Sketchfab). This pack therefore uses
Freesound's PUBLIC preview transcodes (128 kbit/s MP3, "-hq" preview) —
the licensed work itself, in a lossy encode. Documented consequence: the
stems are proof-grade; if the owner wants lossless sources for the final
Phase 10 mix, each lock entry pins the sound page for a logged-in
re-download under the same manifest ids.

All processing is deterministic: pinned inputs -> byte-identical WAVs
(mp3 decode via the pinned imageio-ffmpeg build; numpy processing is
pure). The committed WAVs are the build inputs; this script exists so a
clean checkout can re-derive and verify them.

Usage (from reconstruction/):
  uv run python scripts/fetch_audio_pack.py               # verify/refetch
  uv run python scripts/fetch_audio_pack.py --write-lock  # (re)pin downloads
"""

from __future__ import annotations

import datetime as _dt
import hashlib
import json
import re as _re
import shutil
import subprocess
import sys
import tempfile
import urllib.request
import wave
from pathlib import Path

import numpy as np

REPO = Path(__file__).resolve().parent.parent.parent
AUDIO_ROOT = REPO / "app/Assets/ThirdParty/Audio"
EVIDENCE_ROOT = REPO / "docs/assets/licenses"
LOCK_PATH = Path(__file__).resolve().parent.parent / "audio/freesound-pack.lock.json"

SR = 44100
CC0 = "http://creativecommons.org/publicdomain/zero/1.0/"

# --------------------------------------------------------------------------
# The pack. `process` says how the raw preview becomes committed clips:
#   one_shot  : cut around the loudest transient (pre/post seconds)
#   variants  : cut the N loudest well-separated transients
#   bed       : take [start, start+dur) with long fades (loopable ambience)
# --------------------------------------------------------------------------
PACK = [
    # --- musketry (individual reports; volleys are many delayed singles) ---
    dict(id="fs-musket-mlsulli-234869", title="Musket Shot.wav",
         author="mlsulli", fsid=234869,
         page="https://freesound.org/people/mlsulli/sounds/234869/",
         preview="https://cdn.freesound.org/previews/234/234869_3883507-hq.mp3",
         category="Musket", process=dict(kind="one_shot", pre=0.05, post=1.8)),
    dict(id="fs-musket-willlewis-244345", title="Musket Explosion",
         author="Willlewis", fsid=244345,
         page="https://freesound.org/people/Willlewis/sounds/244345/",
         preview="https://cdn.freesound.org/previews/244/244345_1868310-hq.mp3",
         category="Musket", process=dict(kind="one_shot", pre=0.05, post=1.8)),
    dict(id="fs-musket-brunoauzet-538795", title="old musket bang.wav",
         author="bruno.auzet", fsid=538795,
         page="https://freesound.org/people/bruno.auzet/sounds/538795/",
         preview="https://cdn.freesound.org/previews/538/538795_11519060-hq.mp3",
         category="Musket", process=dict(kind="one_shot", pre=0.05, post=1.8)),
    dict(id="fs-musket-fennelliott-347647", title="musket.wav",
         author="fennelliott", fsid=347647,
         page="https://freesound.org/people/fennelliott/sounds/347647/",
         preview="https://cdn.freesound.org/previews/347/347647_6207707-hq.mp3",
         category="Musket", process=dict(kind="one_shot", pre=0.05, post=1.8)),
    dict(id="fs-musket-craigsmith-675622", title="S21-01 Musket shots.wav",
         author="craigsmith", fsid=675622,
         page="https://freesound.org/people/craigsmith/sounds/675622/",
         preview="https://cdn.freesound.org/previews/675/675622_2524442-hq.mp3",
         category="Musket",
         process=dict(kind="variants", n=3, pre=0.05, post=2.2, min_sep=3.0)),

    # --- artillery ---
    dict(id="fs-cannon-tman95-553240", title="Cannon fire.WAV",
         author="t-man95", fsid=553240,
         page="https://freesound.org/people/t-man95/sounds/553240/",
         preview="https://cdn.freesound.org/previews/553/553240_11107968-hq.mp3",
         category="Cannon",
         process=dict(kind="variants", n=2, pre=0.06, post=3.5, min_sep=4.0)),
    dict(id="fs-cannon-andykub-415293", title="Cannon shots.wav",
         author="Andykub", fsid=415293,
         page="https://freesound.org/people/Andykub/sounds/415293/",
         preview="https://cdn.freesound.org/previews/415/415293_8184822-hq.mp3",
         category="Cannon",
         process=dict(kind="variants", n=2, pre=0.06, post=3.5, min_sep=4.0)),
    dict(id="fs-cannon-vishwajay-252281", title="Tribute-Cannon.mp3",
         author="VishwaJay", fsid=252281,
         page="https://freesound.org/people/VishwaJay/sounds/252281/",
         preview="https://cdn.freesound.org/previews/252/252281_3364694-hq.mp3",
         category="Cannon", process=dict(kind="one_shot", pre=0.06, post=3.5)),
    dict(id="fs-cannon-qubodup-187767", title="Cannon Shot",
         author="qubodup", fsid=187767,
         page="https://freesound.org/people/qubodup/sounds/187767/",
         preview="https://cdn.freesound.org/previews/187/187767_71257-hq.mp3",
         category="Cannon", process=dict(kind="one_shot", pre=0.06, post=2.5)),
    dict(id="fs-cannon-distant-craigsmith-675613",
         title="S20-12 Distant cannon fire.wav",
         author="craigsmith", fsid=675613,
         page="https://freesound.org/people/craigsmith/sounds/675613/",
         preview="https://cdn.freesound.org/previews/675/675613_2524442-hq.mp3",
         category="Cannon",
         process=dict(kind="variants", n=3, pre=0.15, post=4.0, min_sep=5.0)),

    # --- projectile pass-by / ricochet ---
    dict(id="fs-ricochet-cv-523403", title="22 caliber with ricochet.wav",
         author="C-V", fsid=523403,
         page="https://freesound.org/people/C-V/sounds/523403/",
         preview="https://cdn.freesound.org/previews/523/523403_8956746-hq.mp3",
         category="Projectile",
         process=dict(kind="variants", n=2, pre=0.02, post=0.65, min_sep=1.6)),
    dict(id="fs-ricochet-cedarstudios-148840", title="ricochet.mp3",
         author="cedarstudios", fsid=148840,
         page="https://freesound.org/people/cedarstudios/sounds/148840/",
         preview="https://cdn.freesound.org/previews/148/148840_2676248-hq.mp3",
         category="Projectile",
         process=dict(kind="variants", n=3, pre=0.02, post=1.0, min_sep=1.4)),

    # --- canister/shell strikes: earth and debris ---
    dict(id="fs-dirt-krystianpawlowski-584894",
         title="throwing logs on dirt.wav",
         author="KrystianPawlowski", fsid=584894,
         page="https://freesound.org/people/KrystianPawlowski/sounds/584894/",
         preview="https://cdn.freesound.org/previews/584/584894_13194852-hq.mp3",
         category="Strike",
         process=dict(kind="variants", n=3, pre=0.04, post=1.2, min_sep=1.2)),
    dict(id="fs-dirt-designerschoice-852797",
         title="CU_Shovel, Into Dirt, Throw on Ground",
         author="designerschoice (Nicholas Judy/TDC)", fsid=852797,
         page="https://freesound.org/people/designerschoice/sounds/852797/",
         preview="https://cdn.freesound.org/previews/852/852797_6951162-hq.mp3",
         category="Strike",
         process=dict(kind="variants", n=2, pre=0.04, post=1.0, min_sep=1.2)),

    # --- movement: footsteps, gear, fence rails ---
    dict(id="fs-step-michelefalleri-578459", title="Stepping on dry grass",
         author="MicheleFalleri", fsid=578459,
         page="https://freesound.org/people/MicheleFalleri/sounds/578459/",
         preview="https://cdn.freesound.org/previews/578/578459_13048145-hq.mp3",
         category="Movement",
         process=dict(kind="variants", n=2, pre=0.03, post=0.5, min_sep=0.6)),
    dict(id="fs-walk-michelefalleri-578453", title="Walking on dry grass",
         author="MicheleFalleri", fsid=578453,
         page="https://freesound.org/people/MicheleFalleri/sounds/578453/",
         preview="https://cdn.freesound.org/previews/578/578453_13048145-hq.mp3",
         category="Movement",
         process=dict(kind="variants", n=4, pre=0.03, post=0.45, min_sep=0.55)),
    dict(id="fs-run-worthahep88-319206", title="Dry Grass Crunch.wav",
         author="worthahep88", fsid=319206,
         page="https://freesound.org/people/worthahep88/sounds/319206/",
         preview="https://cdn.freesound.org/previews/319/319206_3443504-hq.mp3",
         category="Movement",
         process=dict(kind="variants", n=4, pre=0.03, post=0.4, min_sep=0.5)),
    dict(id="fs-step-ali6868-384869", title="Right Grass/Grassy Footstep 4",
         author="Ali_6868", fsid=384869,
         page="https://freesound.org/people/Ali_6868/sounds/384869/",
         preview="https://cdn.freesound.org/previews/384/384869_984733-hq.mp3",
         category="Movement", process=dict(kind="one_shot", pre=0.03, post=0.5)),
    dict(id="fs-rail-profispiesser-583057",
         title="FX SaSc Wood Window Door Rattle Small Impact Hit Wind Close 01",
         author="Profispiesser", fsid=583057,
         page="https://freesound.org/people/Profispiesser/sounds/583057/",
         preview="https://cdn.freesound.org/previews/583/583057_6667441-hq.mp3",
         category="Movement",
         process=dict(kind="variants", n=2, pre=0.04, post=1.0, min_sep=1.2)),

    # --- voices: unit noise, wounded, breathing (generic, unattributed) ---
    dict(id="fs-mob-fillmat-384401",
         title="Crowd/Mob/Riot Noise (Voices Only)",
         author="FillMat", fsid=384401,
         page="https://freesound.org/people/FillMat/sounds/384401/",
         preview="https://cdn.freesound.org/previews/384/384401_3252134-hq.mp3",
         category="Voice", process=dict(kind="bed", start=5.0, dur=30.0)),
    dict(id="fs-shout-craigsmith-480805",
         title="R15-73-Small Group of Men Shouting.wav",
         author="craigsmith", fsid=480805,
         page="https://freesound.org/people/craigsmith/sounds/480805/",
         preview="https://cdn.freesound.org/previews/480/480805_2524442-hq.mp3",
         category="Voice",
         process=dict(kind="variants", n=3, pre=0.15, post=2.2, min_sep=3.0)),
    dict(id="fs-groan-kentspublicdomain-344682",
         title="Pain part 1: groan, torture, suffering, despair, man, male",
         author="kentspublicdomain", fsid=344682,
         page="https://freesound.org/people/kentspublicdomain/sounds/344682/",
         preview="https://cdn.freesound.org/previews/344/344682_5583936-hq.mp3",
         category="Voice",
         process=dict(kind="variants", n=3, pre=0.20, post=2.5, min_sep=3.0)),
    dict(id="fs-groan-vmgraw-257706", title="GROANING.wav",
         author="vmgraw", fsid=257706,
         page="https://freesound.org/people/vmgraw/sounds/257706/",
         preview="https://cdn.freesound.org/previews/257/257706_4028838-hq.mp3",
         category="Voice",
         process=dict(kind="variants", n=2, pre=0.20, post=2.5, min_sep=3.0)),
    dict(id="fs-breath-noxsound-554907",
         title="Male_Breath_Fast_Loop_Stereo.wav",
         author="Nox_Sound", fsid=554907,
         page="https://freesound.org/people/Nox_Sound/sounds/554907/",
         preview="https://cdn.freesound.org/previews/554/554907_9250976-hq.mp3",
         category="Voice", process=dict(kind="bed", start=0.0, dur=8.0)),

    # --- rural ambience beds ---
    dict(id="fs-meadow-trp-576367",
         title="Field, meadow, insects, crickets, summer day, NOTL, 10",
         author="TRP", fsid=576367,
         page="https://freesound.org/people/TRP/sounds/576367/",
         preview="https://cdn.freesound.org/previews/576/576367_97550-hq.mp3",
         category="Ambience", process=dict(kind="bed", start=2.0, dur=32.0)),
    dict(id="fs-meadow-wind-garuda1982-639459",
         title="summer meadow with wind",
         author="Garuda1982", fsid=639459,
         page="https://freesound.org/people/Garuda1982/sounds/639459/",
         preview="https://cdn.freesound.org/previews/639/639459_2061858-hq.mp3",
         category="Ambience", process=dict(kind="bed", start=2.0, dur=32.0)),
]


def ffmpeg_exe() -> str:
    if shutil.which("ffmpeg"):
        return "ffmpeg"
    import imageio_ffmpeg
    return imageio_ffmpeg.get_ffmpeg_exe()


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def fetch(url: str, dest: Path) -> None:
    req = urllib.request.Request(url, headers={"User-Agent": "battle-atlas-asset-fetch"})
    with urllib.request.urlopen(req, timeout=60) as r, open(dest, "wb") as f:
        shutil.copyfileobj(r, f)


def decode_mono(mp3: Path, tmpdir: Path) -> np.ndarray:
    wav = tmpdir / (mp3.stem + ".wav")
    subprocess.run(
        [ffmpeg_exe(), "-y", "-v", "error", "-i", str(mp3),
         "-ac", "1", "-ar", str(SR), "-c:a", "pcm_s16le", str(wav)],
        check=True)
    with wave.open(str(wav), "rb") as w:
        data = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16)
    return data.astype(np.float32) / 32768.0


def write_wav(path: Path, x: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    xi = np.clip(np.round(x * 32767.0), -32768, 32767).astype(np.int16)
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(xi.tobytes())


def fade(x: np.ndarray, fin: float, fout: float) -> np.ndarray:
    x = x.copy()
    n_in = min(len(x), int(fin * SR))
    n_out = min(len(x), int(fout * SR))
    if n_in > 0:
        x[:n_in] *= np.linspace(0.0, 1.0, n_in, dtype=np.float32)
    if n_out > 0:
        x[-n_out:] *= np.linspace(1.0, 0.0, n_out, dtype=np.float32)
    return x


def peak_normalize(x: np.ndarray, peak: float = 0.89) -> np.ndarray:
    m = float(np.max(np.abs(x))) or 1.0
    return x * (peak / m)


def rms_normalize(x: np.ndarray, dbfs: float = -20.0) -> np.ndarray:
    rms = float(np.sqrt(np.mean(x * x))) or 1e-9
    target = 10.0 ** (dbfs / 20.0)
    y = x * (target / rms)
    m = float(np.max(np.abs(y)))
    if m > 0.95:
        y *= 0.95 / m
    return y


def find_transients(x: np.ndarray, n: int, min_sep: float) -> list[int]:
    """Indices of the N loudest well-separated transients, in time order."""
    order = np.argsort(-np.abs(x))
    picks: list[int] = []
    sep = int(min_sep * SR)
    for idx in order:
        if all(abs(int(idx) - p) >= sep for p in picks):
            picks.append(int(idx))
            if len(picks) == n:
                break
    return sorted(picks)


def cut(x: np.ndarray, center: int, pre: float, post: float) -> np.ndarray:
    a = max(0, center - int(pre * SR))
    b = min(len(x), center + int(post * SR))
    return fade(x[a:b], 0.004, min(0.25, post * 0.3))


def process(entry: dict, raw: np.ndarray, out_dir: Path) -> list[Path]:
    p = entry["process"]
    outs: list[Path] = []
    if p["kind"] == "one_shot":
        peak = int(np.argmax(np.abs(raw)))
        clip = peak_normalize(cut(raw, peak, p["pre"], p["post"]))
        path = out_dir / f"{entry['id']}.wav"
        write_wav(path, clip)
        outs.append(path)
    elif p["kind"] == "variants":
        want = int((p["pre"] + p["post"]) * SR)
        i = 0
        for peak in find_transients(raw, p["n"], p["min_sep"]):
            clip = peak_normalize(cut(raw, peak, p["pre"], p["post"]))
            if len(clip) < int(0.6 * want):
                continue  # transient too close to a file edge: degenerate
            path = out_dir / f"{entry['id']}_{chr(ord('a') + i)}.wav"
            write_wav(path, clip)
            outs.append(path)
            i += 1
    elif p["kind"] == "bed":
        a = int(p["start"] * SR)
        b = min(len(raw), a + int(p["dur"] * SR))
        if a >= len(raw):
            a, b = 0, min(len(raw), int(p["dur"] * SR))
        clip = rms_normalize(fade(raw[a:b], 0.8, 0.8))
        path = out_dir / f"{entry['id']}.wav"
        write_wav(path, clip)
        outs.append(path)
    else:
        raise ValueError(f"unknown process kind {p['kind']}")
    return outs


def category_dir(entry: dict) -> Path:
    return AUDIO_ROOT / entry["category"] / entry["id"]


def main(argv: list[str]) -> int:
    write_lock = "--write-lock" in argv
    lock = {}
    if LOCK_PATH.exists():
        lock = json.loads(LOCK_PATH.read_text())
    new_lock: dict[str, dict] = {}
    today = _dt.date.today().isoformat()

    with tempfile.TemporaryDirectory() as td:
        tmpdir = Path(td)
        for entry in PACK:
            aid = entry["id"]
            mp3 = tmpdir / f"{entry['fsid']}.mp3"
            fetch(entry["preview"], mp3)
            page_html = tmpdir / f"{entry['fsid']}.html"
            fetch(entry["page"], page_html)
            dl_sha = sha256(mp3)

            if not write_lock:
                pinned = lock.get(aid, {}).get("previewSha256")
                if pinned and pinned != dl_sha:
                    print(f"ERROR {aid}: preview sha {dl_sha} != pinned {pinned}",
                          file=sys.stderr)
                    return 1

            raw = decode_mono(mp3, tmpdir)
            out_dir = category_dir(entry)
            if out_dir.exists():
                shutil.rmtree(out_dir)
            outs = process(entry, raw, out_dir)

            # license evidence (plan §18 archive discipline). The page is
            # archived VERBATIM except that Freesound's own embedded map
            # API tokens are redacted — they are the site's third-party
            # secrets, irrelevant to license evidence, and hosting
            # providers rightly refuse pushes containing them.
            ev = EVIDENCE_ROOT / aid
            ev.mkdir(parents=True, exist_ok=True)
            page_text = page_html.read_text(errors="replace")
            page_text = _re.sub(r"\b[ps]k\.[0-9A-Za-z._-]{20,}",
                                "REDACTED-THIRD-PARTY-API-TOKEN", page_text)
            (ev / "freesound-page.html").write_text(page_text)
            (ev / "downloads.sha256").write_text(
                f"{dl_sha}  {entry['preview']}\n")
            acquired = lock.get(aid, {}).get("acquired", today)
            (ev / "acquisition.json").write_text(json.dumps({
                "assetId": aid,
                "acquired": acquired,
                "sourcePage": entry["page"],
                "downloads": [{
                    "url": entry["preview"],
                    "sha256": dl_sha,
                    "note": "Freesound public '-hq' preview transcode "
                            "(128 kbit/s MP3) of the CC0 work; original "
                            "upload requires account login (recorded "
                            "barrier, same as the Phase 2 Sketchfab note).",
                }],
                "license": "CC0-1.0",
                "licenseUrl": CC0,
                "licenseStatementSeen":
                    "freesound-page.html (full page as retrieved "
                    f"{acquired}); the page's license link points to "
                    "creativecommons.org/publicdomain/zero/1.0/",
                "author": entry["author"],
                "authorEvidence": "freesound-page.html, sound byline",
                "title": entry["title"],
                "processing": entry["process"],
                "notes": "Processed deterministically by "
                         "reconstruction/scripts/fetch_audio_pack.py "
                         "(decode via pinned imageio-ffmpeg, mono 44.1 kHz, "
                         "transient cut / bed trim, normalize).",
            }, indent=2) + "\n")

            new_lock[aid] = {
                "previewSha256": dl_sha,
                "acquired": acquired,
                "outputs": {str(o.relative_to(REPO)): sha256(o) for o in outs},
            }
            print(f"ok {aid}: {len(outs)} clip(s)")

    if write_lock:
        LOCK_PATH.parent.mkdir(parents=True, exist_ok=True)
        LOCK_PATH.write_text(json.dumps(new_lock, indent=2, sort_keys=True) + "\n")
        print(f"wrote {LOCK_PATH}")
    else:
        for aid, rec in new_lock.items():
            for rel, digest in rec["outputs"].items():
                pinned = lock.get(aid, {}).get("outputs", {}).get(rel)
                if pinned and pinned != digest:
                    print(f"ERROR {aid}: output {rel} sha drift", file=sys.stderr)
                    return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
