#!/usr/bin/env python3
"""Release manifest for a filmed Soldier View viewpoint (plan §10.1) —
p10-release-manifest.py parameterized by viewpoint id (webb-cushing
slice). Generated media ships via GitHub Releases, never Git history.

Usage (repo root, after scripts/viewpoint-encode.sh <vp>):
    python3 scripts/viewpoint-release-manifest.py <viewpointId>
Outputs:
    docs/benchmarks/captures/<vp>/<vp>-release-manifest.json
    docs/benchmarks/captures/<vp>/<vp>-release-notes.md
"""
import hashlib
import json
import pathlib
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
PAD, FPS = 0.5, 30


def sha256(path: pathlib.Path) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("usage: viewpoint-release-manifest.py <viewpointId>")
    vp = sys.argv[1]
    out = ROOT / "app/RenderOutput" / vp
    evidence = ROOT / "docs/benchmarks/captures" / vp
    evidence.mkdir(parents=True, exist_ok=True)

    doc = json.loads((ROOT / "app/Assets/StreamingAssets/SoldierView/"
                      "viewpoints.json").read_text())
    entry = next(v for v in doc["viewpoints"] if v["id"] == vp)
    t0, t1 = entry["t0"], entry["t1"]
    duration = t1 - t0 + PAD

    git_sha = subprocess.check_output(
        ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
    freeze = json.loads((out / f"{vp}-freeze.json").read_text())

    media = []
    for name, kind in [(f"{vp}.full.mp4", "full 2560x1440p30"),
                       (f"{vp}.proxy.mp4", "proxy 1280x720p30")]:
        p = out / name
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
        "release": f"soldier-view-media-{vp}-v1",
        "viewpointId": vp,
        "window": {"t0": t0, "t1": t1, "padPastT1": PAD, "fps": FPS},
        "gitSha": git_sha,
        "renderFreeze": freeze,
        "media": media,
    }
    mpath = evidence / f"{vp}-release-manifest.json"
    mpath.write_text(json.dumps(manifest, indent=2) + "\n")

    notes = evidence / f"{vp}-release-notes.md"
    lines = [
        f"# Soldier View media — {vp}",
        "",
        f"Pre-rendered Soldier View film `{vp}` (battle window "
        f"t={t0}..{t1} + {PAD}s media-contract pad, {FPS} fps).",
        "Deterministic render of the compiled July 3 Angle bundle "
        f"(settingsHash `{freeze['settingsHash']}`, git `{git_sha[:12]}`).",
        "",
        "| file | bytes | sha256 |",
        "| --- | --- | --- |",
    ]
    for m in media:
        lines.append(f"| {m['name']} | {m['bytes']} | `{m['sha256']}` |")
    lines += [
        "",
        "Publish (owner):",
        "```sh",
        f"gh release create soldier-view-media-{vp}-v1 \\",
        f"  app/RenderOutput/{vp}/{vp}.full.mp4 \\",
        f"  app/RenderOutput/{vp}/{vp}.proxy.mp4 \\",
        f"  docs/benchmarks/captures/{vp}/{vp}-release-manifest.json \\",
        f"  --title \"Soldier View media — {vp}\" \\",
        f"  --notes-file docs/benchmarks/captures/{vp}/{vp}-release-notes.md",
        "```",
        "",
    ]
    notes.write_text("\n".join(lines))
    print(f"wrote {mpath}\nwrote {notes}")


if __name__ == "__main__":
    main()
