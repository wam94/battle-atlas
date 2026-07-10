#!/usr/bin/env python3
"""Phase 10 release manifest (plan §10.1, §12 Phase 10).

Generated media ships via GitHub Releases, never Git history. This
script records names, sizes, sha256, durations, and provenance (git
SHA, settings hash from the render freeze) for the production encodes,
plus the release notes the owner publishes with at Phase 12.

Usage (repo root, after scripts/p10-encode.sh):
    python3 scripts/p10-release-manifest.py
Outputs:
    docs/benchmarks/captures/p10-gate/p10-release-manifest.json
    docs/benchmarks/captures/p10-gate/p10-release-notes.md
"""
import hashlib
import json
import pathlib
import subprocess

ROOT = pathlib.Path(__file__).resolve().parent.parent
OUT = ROOT / "app/RenderOutput/p10"
EVIDENCE = ROOT / "docs/benchmarks/captures/p10-gate"
VP = "garnett-road-to-angle"
T0, T1, PAD, FPS = 8160, 8820, 0.5, 30


def sha256(path: pathlib.Path) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> None:
    git_sha = subprocess.check_output(
        ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
    freeze = json.loads((OUT / "p10-freeze.json").read_text())
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
        "release": "soldier-view-media-v1",
        "viewpointId": VP,
        "window": {"t0": T0, "t1": T1, "padPastT1": PAD, "fps": FPS},
        "gitSha": git_sha,
        "renderGitSha": freeze["gitSha"],
        "settingsHash": freeze["settingsHash"],
        "bundleChecksum": freeze["bundleChecksum"],
        "unityVersion": freeze["unityVersion"],
        "media": media,
        "publishCommand": (
            "gh release create soldier-view-media-v1 "
            f"app/RenderOutput/p10/{VP}.full.mp4 "
            f"app/RenderOutput/p10/{VP}.proxy.mp4 "
            "docs/benchmarks/captures/p10-gate/p10-release-manifest.json "
            "--title 'Soldier View media v1 — garnett-road-to-angle' "
            "--notes-file docs/benchmarks/captures/p10-gate/p10-release-notes.md"
        ),
        "note": "Owner publishes at Phase 12 (plan §12). Do not publish "
                "from Phase 10.",
    }
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "p10-release-manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n")

    notes = f"""# Soldier View media v1 — With Garnett's Line

Pre-rendered first-person Soldier View media for the Battle Atlas
`garnett-road-to-angle` viewpoint (Pickett's Charge, the Emmitsburg
Road to the Angle, battle clock t={T0}..{T1}, 11 minutes at 30 fps,
plus a {PAD} s seek-guard pad).

| file | content | sha256 |
| --- | --- | --- |
"""
    for m in media:
        notes += f"| `{m['name']}` | {m['kind']}, {m['bytes']/1e9:.2f} GB, ~{m['avgVideoPlusAudioMbit']} Mbit/s | `{m['sha256'][:16]}…` |\n"
    notes += f"""
Rendered deterministically at commit `{freeze['gitSha'][:12]}`
(settings hash `{freeze['settingsHash'][:16]}…`); reproduction:
`docs/reconstruction/render-runbook.md`. Install: place both files in
`app/Assets/StreamingAssets/SoldierView/`.

Content warning: this reconstruction depicts battlefield violence,
wounds, and death soberly and without gameplay framing (see
`docs/reconstruction/violence-and-representation.md`).
"""
    (EVIDENCE / "p10-release-notes.md").write_text(notes)
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
