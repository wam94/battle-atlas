#!/bin/sh
# ANGLE-V2 rolling chunk harvester (adapted from p10-chunk-harvester.sh).
#
# The render machine cannot hold the full 2560x1440 PNG sequence
# (~19,815 frames x ~3.2 MB ≈ 63 GB > free disk). Run this LOOP in a
# second terminal while AngleV2Render.RenderProduction runs: whenever a
# chunk's manifest appears (written only after every frame of the chunk
# is on disk), it
#   1. verifies the chunk's frame files are all present,
#   2. encodes the chunk with the FINAL delivery codec settings
#      (libx264 preset slow CRF 18 yuv420p, 1 keyframe/s, closed GOP —
#      x264 default — so chunk mp4s concat losslessly into the exact
#      stream a single-pass encode of the same frames would give),
#   3. verifies the encoded frame count by decoding the chunk,
#   4. deletes the chunk's PNGs (regenerable: the render is resumable
#      and deterministic; AngleV2Render.ChunkComplete accepts an
#      encoded chunk as complete).
#
# Steady-state disk use stays at ~2 chunks of PNGs (~12 GB).
#
# Usage: scripts/angle-v2-chunk-harvester.sh [--once]
#   loops until app/RenderOutput/angle-v2/render-done exists (touch it when
#   RenderProduction exits) and all chunks are harvested; --once does a
#   single pass.
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/app/RenderOutput/angle-v2"
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
  did=0
  for mf in "$MANIFESTS"/chunk_*.json; do
    [ -e "$mf" ] || continue
    c=$(basename "$mf" .json | sed 's/chunk_//')
    mp4="$CHUNKS/chunk_$c.mp4"
    [ -f "$mp4" ] && continue
    frame0=$(python3 -c "import json;print(json.load(open('$mf'))['frame0'])")
    count=$(python3 -c "import json;print(json.load(open('$mf'))['frameCount'])")
    # 1. all frames present?
    ok=1
    i=0
    while [ $i -lt "$count" ]; do
      n=$(printf %06d $((frame0 + i)))
      [ -f "$SEQ/frame_$n.png" ] || { ok=0; break; }
      i=$((i + 1))
    done
    [ $ok -eq 1 ] || { echo "chunk $c: frames missing, skip"; continue; }
    # 2. encode with the final delivery settings
    echo "== encoding chunk $c (frames $frame0 +$count)"
    "$FFMPEG" -y -loglevel error -start_number "$frame0" -framerate $FPS \
      -i "$SEQ/frame_%06d.png" -frames:v "$count" \
      -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
      -g $FPS -keyint_min $FPS \
      "$mp4.part.mp4"
    # 3. decoded frame count must equal the manifest's
    decoded=$("$FFMPEG" -loglevel info -i "$mp4.part.mp4" -map 0:v -f null - 2>&1 \
      | sed -n 's/.*frame=[[:space:]]*\([0-9][0-9]*\).*/\1/p' | tail -1)
    if [ "$decoded" != "$count" ]; then
      echo "chunk $c: decoded $decoded != $count frames; keeping PNGs" >&2
      rm -f "$mp4.part.mp4"
      exit 1
    fi
    mv "$mp4.part.mp4" "$mp4"
    # 4. reclaim the PNGs
    i=0
    while [ $i -lt "$count" ]; do
      n=$(printf %06d $((frame0 + i)))
      rm -f "$SEQ/frame_$n.png"
      i=$((i + 1))
    done
    did=1
    echo "== chunk $c harvested ($(du -h "$mp4" | cut -f1))"
  done
  return 0
}

if [ "${1:-}" = "--once" ]; then
  harvest_pass
  exit 0
fi

while :; do
  harvest_pass
  if [ -f "$OUT/render-done" ]; then
    # final pass then exit
    harvest_pass
    echo "== harvester done"
    exit 0
  fi
  sleep 30
done
