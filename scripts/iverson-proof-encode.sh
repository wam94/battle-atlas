#!/bin/sh
# Iverson's-field owner-gate proof encode (second Soldier View film):
#   * the two 30 s camera-style candidates (first-person vs very tight
#     close third person), same window t=6090..6120 (the destruction's
#     peak), same slot (12th NC rear rank, slot 184) — the P9 FP-vs-3P
#     owner-choice gate pattern;
#   * both muxed with the deterministic stem mix when it exists
#     (build_viewpoint_audio.py over iverson-audio-events.json,
#     --t0 6090 --t1 6120), else video-only.
#
# ffmpeg resolution follows the Phase 1 convention: system ffmpeg if
# present, else the pinned imageio-ffmpeg static build. Short GOP per
# plan §10 (1 keyframe/s) — the production media-contract setting, so
# the proofs measure the same decode behavior the film will ship with.
#
# Usage: scripts/iverson-proof-encode.sh   (from the repo root, after
#   IversonGateRender.RenderCandidates [+ the stem build])
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/docs/benchmarks/captures/iverson-viewpoint"

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
    "$OUT/$3-video-only.mp4"
  if [ -f "$OUT/stems/mix.wav" ]; then
    "$FFMPEG" -y -i "$OUT/$3-video-only.mp4" -i "$OUT/stems/mix.wav" \
      -map 0:v -map 1:a -c:v copy -c:a aac -b:a 192k -shortest \
      "$OUT/$3-1440p.mp4"
    rm "$OUT/$3-video-only.mp4"
  else
    mv "$OUT/$3-video-only.mp4" "$OUT/$3-1440p.mp4"
    echo "== no stems/mix.wav: $3 encoded video-only"
  fi
}

encode "$OUT/seq-fp"  900 "iverson-proof-fp-30s"
encode "$OUT/seq-c3p" 900 "iverson-proof-c3p-30s"

shasum -a 256 "$OUT"/iverson-proof-*.mp4 | tee "$OUT/iverson-proof-media.sha256"
echo "== done"
