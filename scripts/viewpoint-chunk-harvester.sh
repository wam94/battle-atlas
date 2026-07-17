#!/bin/sh
# Rolling chunk harvester (webb-cushing slice) — p10-chunk-harvester.sh
# parameterized by viewpoint id. Encodes each completed 60 s chunk with
# the FINAL delivery codec settings (libx264 preset slow CRF 18 yuv420p,
# 1 keyframe/s, closed GOP — chunk mp4s concat losslessly), verifies the
# decoded frame count against the chunk manifest, then reclaims the PNGs
# so steady-state disk use stays around two chunks of frames.
#
# Usage: scripts/viewpoint-chunk-harvester.sh <viewpointId> [--once]
#   loops until app/RenderOutput/<vp>/render-done exists and all chunks
#   are harvested; --once does a single pass.
set -eu
[ $# -ge 1 ] || { echo "usage: $0 <viewpointId> [--once]" >&2; exit 2; }
VP="$1"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/app/RenderOutput/$VP"
SEQ="$OUT/seq-full"
CHUNKS="$OUT/chunks"
MANIFESTS="$OUT/manifests"
FPS=30
mkdir -p "$CHUNKS"

if command -v ffmpeg >/dev/null 2>&1; then
  FFMPEG=ffmpeg
else
  FFMPEG="$(cd "$ROOT/reconstruction" && uv run python -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())')"
fi

harvest_pass() {
  for mf in "$MANIFESTS"/chunk_*.json; do
    [ -e "$mf" ] || continue
    c=$(basename "$mf" .json | sed 's/chunk_//')
    mp4="$CHUNKS/chunk_$c.mp4"
    [ -f "$mp4" ] && continue
    frame0=$(python3 -c "import json;print(json.load(open('$mf'))['frame0'])")
    count=$(python3 -c "import json;print(json.load(open('$mf'))['frameCount'])")
    ok=1
    i=0
    while [ $i -lt "$count" ]; do
      n=$(printf %06d $((frame0 + i)))
      [ -f "$SEQ/frame_$n.png" ] || { ok=0; break; }
      i=$((i + 1))
    done
    [ $ok -eq 1 ] || { echo "chunk $c: frames missing, skip"; continue; }
    echo "== encoding chunk $c (frames $frame0 +$count)"
    "$FFMPEG" -y -loglevel error -start_number "$frame0" -framerate $FPS \
      -i "$SEQ/frame_%06d.png" -frames:v "$count" \
      -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
      -g $FPS -keyint_min $FPS \
      "$mp4.part.mp4"
    decoded=$("$FFMPEG" -loglevel info -i "$mp4.part.mp4" -map 0:v -f null - 2>&1 \
      | sed -n 's/.*frame=[[:space:]]*\([0-9][0-9]*\).*/\1/p' | tail -1)
    if [ "$decoded" != "$count" ]; then
      echo "chunk $c: decoded $decoded != $count frames; keeping PNGs" >&2
      rm -f "$mp4.part.mp4"
      exit 1
    fi
    mv "$mp4.part.mp4" "$mp4"
    i=0
    while [ $i -lt "$count" ]; do
      n=$(printf %06d $((frame0 + i)))
      rm -f "$SEQ/frame_$n.png"
      i=$((i + 1))
    done
    echo "== chunk $c harvested ($(du -h "$mp4" | cut -f1))"
  done
  return 0
}

if [ "${2:-}" = "--once" ]; then
  harvest_pass
  exit 0
fi

while :; do
  harvest_pass
  if [ -f "$OUT/render-done" ]; then
    harvest_pass
    echo "== harvester done ($VP)"
    exit 0
  fi
  sleep 30
done
