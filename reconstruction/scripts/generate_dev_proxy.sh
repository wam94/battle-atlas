#!/bin/bash
# Generates the Phase 1 ten-second timecode proxy for the dev-timecode
# viewpoint (t=8160..8170, 30 fps, 1280x720). The burned-in text shows the
# battle-clock second, the frame within that second, and the absolute frame
# number, so clock/video synchronization can be verified by eye.
#
# PINNED COMMAND — do not tweak casually; Phase 10 inherits this discipline.
# Generated media is gitignored (plan section 10/16: media is a release
# artifact, never a Git object). Commit this script, not its output.
#
# ffmpeg resolution: system ffmpeg if present, else the static build bundled
# with the imageio-ffmpeg dev dependency of reconstruction/ (no system
# install required). Version is printed at generation time.
set -euo pipefail

RECON="$(cd "$(dirname "$0")/.." && pwd)"
REPO="$(dirname "$RECON")"
OUT="${1:-$REPO/app/Assets/StreamingAssets/SoldierView/dev-timecode.proxy.mp4}"

T0=8160        # viewpoint t0 (battle-clock seconds)
FPS=30
DURATION=10
SIZE=1280x720
FONT=/System/Library/Fonts/Menlo.ttc

if command -v ffmpeg >/dev/null 2>&1; then
  FFMPEG=ffmpeg
else
  FFMPEG="$(cd "$RECON" && uv run python -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())')"
fi
echo "== ffmpeg: $FFMPEG"
"$FFMPEG" -version | head -1

mkdir -p "$(dirname "$OUT")"

# testsrc2: moving synthetic pattern (any dropped/frozen frame is visible).
# drawtext: "BATTLE t=8163s +14f  frame 104" for frame n at t0 + n/FPS.
# GOP: keyframe every 15 frames (0.5 s) — plan section 10.1 wants one
# keyframe per second or better for seekable video; sc_threshold 0 keeps the
# cadence exact.
"$FFMPEG" -y \
  -f lavfi -i "testsrc2=size=${SIZE}:rate=${FPS}:duration=${DURATION}" \
  -vf "drawtext=fontfile=${FONT}:text='BATTLE t\\=%{eif\\:${T0}+trunc(n/${FPS})\\:d}s +%{eif\\:mod(n,${FPS})\\:d\\:2}f  frame %{frame_num}':fontsize=56:fontcolor=white:x=(w-tw)/2:y=h-th-60:box=1:boxcolor=black@0.65:boxborderw=12" \
  -c:v libx264 -profile:v high -crf 18 -pix_fmt yuv420p \
  -g 15 -keyint_min 15 -sc_threshold 0 \
  -movflags +faststart \
  "$OUT"

echo "== Wrote $OUT"
shasum -a 256 "$OUT"
