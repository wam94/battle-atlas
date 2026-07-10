#!/bin/sh
# Gate P8 evidence encode: the 90-second, 30 fps eye-level wall sequence
# rendered by GateP8Render.RenderSequence into H.264 (short GOP per plan
# §10: one keyframe per second) plus a 720p proxy.
#
# ffmpeg resolution follows the Phase 1 convention
# (reconstruction/scripts/generate_dev_proxy.sh): system ffmpeg if
# present, else the static build bundled with the imageio-ffmpeg dev
# dependency of reconstruction/.
#
# Usage: scripts/p8-encode.sh   (from the repo root)
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SEQ="$ROOT/docs/benchmarks/captures/p8-gate/seq"
OUT="$ROOT/docs/benchmarks/captures/p8-gate"
[ -f "$SEQ/frame_0000.png" ] || {
  echo "no frames at $SEQ; run GateP8Render.RenderSequence first" >&2
  exit 1
}

if command -v ffmpeg >/dev/null 2>&1; then
  FFMPEG=ffmpeg
else
  FFMPEG="$(cd "$ROOT/reconstruction" && uv run python -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())')"
fi
echo "== ffmpeg: $FFMPEG"

# frame continuity check before encode (plan §12 Phase 10 discipline)
n_expected=2700
n_actual=$(ls "$SEQ" | grep -c '^frame_[0-9]\{4\}\.png$')
[ "$n_actual" -eq "$n_expected" ] || {
  echo "frame count $n_actual != $n_expected; refusing to encode" >&2
  exit 1
}

"$FFMPEG" -y -framerate 30 -i "$SEQ/frame_%04d.png" \
  -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
  -g 30 -keyint_min 30 -movflags +faststart \
  "$OUT/p8-gate-90s-1440p.mp4"

"$FFMPEG" -y -framerate 30 -i "$SEQ/frame_%04d.png" \
  -vf scale=1280:720 \
  -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p \
  -g 30 -keyint_min 30 -movflags +faststart \
  "$OUT/p8-gate-90s-720p.mp4"

shasum -a 256 "$OUT"/p8-gate-90s-*.mp4 | tee "$OUT/p8-gate-media.sha256"
echo "== done: $OUT/p8-gate-90s-1440p.mp4 + 720p proxy"
