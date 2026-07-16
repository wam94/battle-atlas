#!/bin/sh
# Fight-prone vocabulary gate evidence encode: the 30-second, 30 fps
# eye-level sequence rendered by FightProneGateRender.RenderSequence into
# H.264 (short GOP: one keyframe per second) plus a 720p proxy.
#
# ffmpeg resolution follows the Phase 1 convention: system ffmpeg if
# present, else the static build bundled with the imageio-ffmpeg dev
# dependency of reconstruction/.
#
# Usage: scripts/fight-prone-encode.sh   (from the repo root)
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SEQ="$ROOT/docs/benchmarks/captures/fight-prone/seq"
OUT="$ROOT/docs/benchmarks/captures/fight-prone"
[ -f "$SEQ/frame_0000.png" ] || {
  echo "no frames at $SEQ; run FightProneGateRender.RenderSequence first" >&2
  exit 1
}

if command -v ffmpeg >/dev/null 2>&1; then
  FFMPEG=ffmpeg
else
  FFMPEG="$(cd "$ROOT/reconstruction" && uv run python -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())')"
fi
echo "== ffmpeg: $FFMPEG"

# frame continuity check before encode (plan §12 Phase 10 discipline)
n_expected=900
n_actual=$(ls "$SEQ" | grep -c '^frame_[0-9]\{4\}\.png$')
[ "$n_actual" -eq "$n_expected" ] || {
  echo "frame count $n_actual != $n_expected; refusing to encode" >&2
  exit 1
}

"$FFMPEG" -y -framerate 30 -i "$SEQ/frame_%04d.png" \
  -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
  -g 30 -keyint_min 30 -movflags +faststart \
  "$OUT/fight-prone-30s-1440p.mp4"

"$FFMPEG" -y -framerate 30 -i "$SEQ/frame_%04d.png" \
  -vf scale=1280:720 \
  -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p \
  -g 30 -keyint_min 30 -movflags +faststart \
  "$OUT/fight-prone-30s-720p.mp4"

shasum -a 256 "$OUT"/fight-prone-30s-*.mp4 | tee "$OUT/fight-prone-media.sha256"
echo "== done: $OUT/fight-prone-30s-1440p.mp4 + 720p proxy"
