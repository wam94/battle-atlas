#!/bin/sh
# Gate P9 evidence encode (plan §12 Phase 9):
#   * the two 30 s camera-style candidates (first-person vs very tight
#     close third person), same window t=8270..8300, same slot;
#   * the 60 s first-person audio-visual proof at the wall (t=8610..8670),
#     muxed with the deterministic stem mix (build_viewpoint_audio.py).
#
# ffmpeg resolution follows the Phase 1 convention: system ffmpeg if
# present, else the pinned imageio-ffmpeg static build from the
# reconstruction dev dependencies. Short GOP per plan §10 (1 keyframe/s).
#
# Usage: scripts/p9-encode.sh   (from the repo root, after
#   GateP9Render.RenderCandidates / RenderProof and the stem build)
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/docs/benchmarks/captures/p9-gate"

if command -v ffmpeg >/dev/null 2>&1; then
  FFMPEG=ffmpeg
else
  FFMPEG="$(cd "$ROOT/reconstruction" && uv run python -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())')"
fi
echo "== ffmpeg: $FFMPEG"

encode () { # $1=seq dir  $2=frames expected  $3=out base
  n=$(ls "$1" | grep -c '^frame_[0-9]\{4\}\.png$')
  [ "$n" -eq "$2" ] || { echo "$1: $n frames != $2; refusing" >&2; exit 1; }
  "$FFMPEG" -y -framerate 30 -i "$1/frame_%04d.png" \
    -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
    -g 30 -keyint_min 30 -movflags +faststart \
    "$OUT/$3-1440p.mp4"
}

encode "$OUT/seq-fp"  900 "p9-candidate-fp-30s"
encode "$OUT/seq-c3p" 900 "p9-candidate-c3p-30s"
encode "$OUT/seq-proof" 1800 "p9-proof-60s-video-only"

# mux the proof with the deterministic mix (stems built for the same
# window: --t0 8610 --t1 8670, so mix.wav sample 0 == frame 0)
[ -f "$OUT/stems/mix.wav" ] || {
  echo "no $OUT/stems/mix.wav; run build_viewpoint_audio.py first" >&2
  exit 1
}
"$FFMPEG" -y -i "$OUT/p9-proof-60s-video-only-1440p.mp4" \
  -i "$OUT/stems/mix.wav" \
  -map 0:v -map 1:a -c:v copy -c:a aac -b:a 192k -shortest \
  "$OUT/p9-proof-60s-av-fp-1440p.mp4"

"$FFMPEG" -y -i "$OUT/p9-proof-60s-av-fp-1440p.mp4" \
  -vf scale=1280:720 \
  -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p \
  -g 30 -keyint_min 30 -c:a copy -movflags +faststart \
  "$OUT/p9-proof-60s-av-fp-720p.mp4"

shasum -a 256 "$OUT"/p9-candidate-*.mp4 "$OUT"/p9-proof-*.mp4 \
  | tee "$OUT/p9-gate-media.sha256"
echo "== done"
