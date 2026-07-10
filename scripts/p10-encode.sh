#!/bin/sh
# Phase 10 production encode (plan §12 Phase 10, §10.1; media contract
# docs/reconstruction/p1-media-contract.md).
#
#   1. verifies the rendered image sequence is complete (no missing or
#      duplicate frames — HARD FAIL with the frame list otherwise);
#   2. encodes the 2560x1440p30 H.264 full deliverable (short GOP:
#      1 keyframe per second) and muxes the deterministic full-length
#      audio mix (build_viewpoint_audio.py stems);
#   3. encodes the 1280x720p30 proxy from the same source frames;
#   4. writes sha256 checksums and measured sizes/bitrates.
#
# ffmpeg resolution follows the project convention (Phase 1): system
# ffmpeg if present, else the pinned imageio-ffmpeg 7.1 static build from
# the reconstruction dev dependencies. Fails clearly if neither exists.
#
# Usage: scripts/p10-encode.sh   (from the repo root, after
#   Phase10Render.RenderProduction and the stem build; see
#   docs/reconstruction/render-runbook.md)
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SEQ="$ROOT/app/RenderOutput/p10/seq-full"
OUT="$ROOT/app/RenderOutput/p10"
STEMS="$OUT/stems-full"
VP="garnett-road-to-angle"

# frame count must match Phase10Render: (t1 - t0 + PadPastT1) * fps
T0=8160
T1=8820
PAD=0.5
FPS=30
EXPECTED=$(python3 -c "print(round(($T1 - $T0 + $PAD) * $FPS))")

if command -v ffmpeg >/dev/null 2>&1; then
  FFMPEG=ffmpeg
else
  FFMPEG="$(cd "$ROOT/reconstruction" && uv run python -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())')" || {
    echo "FATAL: no system ffmpeg and imageio-ffmpeg unavailable" >&2
    exit 1
  }
fi
echo "== ffmpeg: $FFMPEG"
"$FFMPEG" -version | head -1

# --- 1. frame continuity: missing/duplicate detection (hard fail) ------
echo "== verifying $EXPECTED frames in $SEQ"
python3 - "$SEQ" "$EXPECTED" <<'EOF'
import os, re, sys
seq, expected = sys.argv[1], int(sys.argv[2])
pat = re.compile(r"^frame_(\d{6})\.png$")
seen = {}
extra = []
for name in os.listdir(seq):
    m = pat.match(name)
    if not m:
        extra.append(name)
        continue
    n = int(m.group(1))
    seen[n] = seen.get(n, 0) + 1
missing = [n for n in range(expected) if n not in seen]
dupes = sorted(n for n, c in seen.items() if c > 1)
out_of_range = sorted(n for n in seen if n >= expected)
ok = not missing and not dupes and not out_of_range and not extra
if not ok:
    if missing:
        print(f"MISSING {len(missing)} frames: {missing[:50]}"
              + (" ..." if len(missing) > 50 else ""))
    if dupes:
        print(f"DUPLICATE frame numbers: {dupes[:50]}")
    if out_of_range:
        print(f"OUT-OF-RANGE frames (>= {expected}): {out_of_range[:50]}")
    if extra:
        print(f"UNEXPECTED files: {extra[:50]}")
    sys.exit(1)
print(f"frame sequence complete: {expected} frames, no gaps, no duplicates")
EOF

# --- 2. audio mix must exist and cover the padded window ---------------
[ -f "$STEMS/mix.wav" ] || {
  echo "FATAL: no $STEMS/mix.wav — build stems first:" >&2
  echo "  cd reconstruction && uv run python scripts/build_viewpoint_audio.py \\" >&2
  echo "    --events ../docs/benchmarks/captures/p9-gate/p9-audio-events.json \\" >&2
  echo "    --out ../app/RenderOutput/p10/stems-full --t0 $T0 --t1 $(python3 -c "print($T1+$PAD)")" >&2
  exit 1
}

# --- 3. encodes ----------------------------------------------------------
# Full: H.264 CRF 18 preset slow, 1 keyframe/s (short GOP for seeking),
# +faststart for instant open. AAC 192k from the deterministic mix;
# -shortest trims the audio tail to the video.
echo "== encoding 1440p full"
"$FFMPEG" -y -framerate $FPS -i "$SEQ/frame_%06d.png" -i "$STEMS/mix.wav" \
  -map 0:v -map 1:a \
  -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
  -g $FPS -keyint_min $FPS -movflags +faststart \
  -c:a aac -b:a 192k -shortest \
  "$OUT/$VP.full.mp4"

echo "== encoding 720p proxy"
"$FFMPEG" -y -i "$OUT/$VP.full.mp4" \
  -vf scale=1280:720 \
  -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p \
  -g $FPS -keyint_min $FPS -movflags +faststart \
  -c:a copy \
  "$OUT/$VP.proxy.mp4"

# --- 4. checksums + measured stats --------------------------------------
echo "== measured media"
for f in "$OUT/$VP.full.mp4" "$OUT/$VP.proxy.mp4"; do
  SZ=$(stat -f%z "$f")
  DUR=$(python3 -c "print(($T1 - $T0 + $PAD))")
  MBIT=$(python3 -c "print(f'{$SZ*8/$DUR/1e6:.2f}')")
  echo "$(basename "$f"): $SZ bytes, ~${MBIT} Mbit/s over ${DUR}s"
done
shasum -a 256 "$OUT/$VP.full.mp4" "$OUT/$VP.proxy.mp4" \
  | tee "$OUT/p10-media.sha256"
echo "== done"
