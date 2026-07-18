#!/usr/bin/env python3
"""Iverson production release manifest (plan §10.1; the P10 pattern).

Generated media ships via GitHub Releases, never Git history. This
script records names, sizes, sha256, durations, and provenance (git
SHA, settings hash from the render freeze) for the production encodes,
plus the release notes the owner publishes with.

Usage (repo root, after scripts/iverson-encode.sh):
    python3 scripts/iverson-release-manifest.py
Outputs:
    docs/benchmarks/captures/iverson-production/iverson-release-manifest.json
    docs/benchmarks/captures/iverson-production/iverson-release-notes.md
"""
import hashlib
import json
import pathlib
import subprocess

ROOT = pathlib.Path(__file__).resolve().parent.parent
OUT = ROOT / "app/RenderOutput/iverson"
EVIDENCE = ROOT / "docs/benchmarks/captures/iverson-production"
VP = "iverson-forney-field"
T0, T1, PAD, FPS = 5830, 7040, 0.5, 30


def sha256(path: pathlib.Path) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> None:
    git_sha = subprocess.check_output(
        ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
    freeze = json.loads((OUT / "iverson-freeze.json").read_text())
    duration = T1 - T0 + PAD

    media = []
    for name, kind in [(f"{VP}.full.mp4", "full 2560x1440p30"),
                       (f"{VP}.proxy.mp4", "proxy 1280x720p30")]:
        p = OUT / name
        size = p.stat().st_size
        media.append({
            "name": name,
            "kind": kind,
            "bytes": size,
            "sha256": sha256(p),
            "durationS": duration,
            "avgVideoPlusAudioMbit": round(size * 8 / duration / 1e6, 2),
            "codec": "H.264 yuv420p + AAC 192k, 1 keyframe/s, faststart",
        })

    manifest = {
        "release": "soldier-view-media-v2-iverson",
        "viewpointId": VP,
        "window": {"t0": T0, "t1": T1, "padPastT1": PAD, "fps": FPS,
                   "clock": "gettysburg-july1-afternoon (startTime 46800)"},
        "gitSha": git_sha,
        "renderGitSha": freeze["gitSha"],
        "settingsHash": freeze["settingsHash"],
        "bundleChecksum": freeze["bundleChecksum"],
        "battleSeed": freeze["battleSeed"],
        "unityVersion": freeze["unityVersion"],
        "media": media,
        "publishCommand": (
            "gh release create soldier-view-media-v2-iverson "
            f"app/RenderOutput/iverson/{VP}.full.mp4 "
            f"app/RenderOutput/iverson/{VP}.proxy.mp4 "
            "docs/benchmarks/captures/iverson-production/iverson-release-manifest.json "
            "--title 'Soldier View media v2 — iverson-forney-field' "
            "--notes-file docs/benchmarks/captures/iverson-production/iverson-release-notes.md"
        ),
        "note": "OWNER publishes. Do not publish from the executor slice.",
    }
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "iverson-release-manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n")

    notes = f"""# Soldier View media v2 — With the Twelfth North Carolina

Pre-rendered first-person Soldier View media for the Battle Atlas
`iverson-forney-field` viewpoint (the destruction of Iverson's brigade,
Forney field / Oak Ridge, July 1, 1863; July 1 afternoon battle clock
t={T0}..{T1}, 20.2 minutes at 30 fps, plus a {PAD} s seek-guard pad).

| file | content | sha256 |
| --- | --- | --- |
"""
    for m in media:
        notes += (f"| `{m['name']}` | {m['kind']}, {m['bytes']/1e9:.2f} GB, "
                  f"~{m['avgVideoPlusAudioMbit']} Mbit/s | `{m['sha256'][:16]}…` |\n")
    notes += f"""
Rendered deterministically at commit `{freeze['gitSha'][:12]}`
(settings hash `{freeze['settingsHash'][:16]}…`, ED-21 production seed
pin `{freeze['battleSeed'][:12]}…`); reproduction:
`docs/reconstruction/render-runbook.md` with the Iverson entry points
(`IversonProductionRender`, `scripts/iverson-*.sh`). Install: place
both files in `app/Assets/StreamingAssets/SoldierView/`.

Content warning: this reconstruction depicts the most concentrated
killing in the corpus, soberly and without gameplay framing; the film
ships behind its own first-entry warning (see
`docs/reconstruction/violence-and-representation.md` and the
`iverson-forney-field` entry in `content-warning.json`).
"""
    (EVIDENCE / "iverson-release-notes.md").write_text(notes)
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
