#!/bin/sh
# Viewpoint production encode (webb-cushing slice) — p10-encode.sh
# parameterized by viewpoint id; window read from the committed
# viewpoints.json so the frame-count verification cannot drift from the
# schema. Same two source modes (full PNG sequence, or harvested chunk
# encodes concat'd losslessly), same missing/duplicate hard-fail, same
# pinned codec settings, same deterministic-mix mux.
#
# Deliverables: app/RenderOutput/<vp>/<vp>.full.mp4 (+ .proxy.mp4) and
# <vp>-media.sha256.
#
# Usage: scripts/viewpoint-encode.sh <viewpointId>
set -eu
[ $# -eq 1 ] || { echo "usage: $0 <viewpointId>" >&2; exit 2; }
VP="$1"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/app/RenderOutput/$VP"
SEQ="$OUT/seq-full"
CHUNKS="$OUT/chunks"
MANIFESTS="$OUT/manifests"
STEMS="$OUT/stems-full"
FPS=30
PAD=0.5

read -r T0 T1 <<EOF2
$(python3 - "$ROOT" "$VP" <<'EOF'
import json, sys
root, vp = sys.argv[1], sys.argv[2]
doc = json.load(open(f"{root}/app/Assets/StreamingAssets/SoldierView/viewpoints.json"))
for v in doc["viewpoints"]:
    if v["id"] == vp:
        print(v["t0"], v["t1"])
        break
else:
    raise SystemExit(f"no viewpoint {vp}")
EOF
)
EOF2
EXPECTED=$(python3 -c "print(round(($T1 - $T0 + $PAD) * $FPS))")
echo "== $VP: window t=$T0..$T1 (+${PAD}s pad) = $EXPECTED frames"

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

decoded_frames() { # $1 = video file
  "$FFMPEG" -loglevel info -i "$1" -map 0:v -f null - 2>&1 \
    | sed -n 's/.*frame=[[:space:]]*\([0-9][0-9]*\).*/\1/p' | tail -1
}

# --- audio mix must exist and cover the padded window -------------------
[ -f "$STEMS/mix.wav" ] || {
  echo "FATAL: no $STEMS/mix.wav — build stems first:" >&2
  echo "  cd reconstruction && uv run python scripts/build_viewpoint_audio.py \\" >&2
  echo "    --events ../docs/benchmarks/captures/$VP/$VP-audio-events.json \\" >&2
  echo "    --out ../app/RenderOutput/$VP/stems-full --t0 $T0 --t1 $(python3 -c "print($T1+$PAD)")" >&2
  exit 1
}

# --- source mode ---------------------------------------------------------
NPNG=$(ls "$SEQ" 2>/dev/null | grep -c '^frame_[0-9]\{6\}\.png$' || true)
VIDEO_ONLY="$OUT/$VP.video.mp4"

if [ "$NPNG" -eq "$EXPECTED" ]; then
  echo "== mode A: full PNG sequence ($NPNG frames)"
  python3 - "$SEQ" "$EXPECTED" <<'EOF'
import os, re, sys
seq, expected = sys.argv[1], int(sys.argv[2])
seen = {}
for name in os.listdir(seq):
    m = re.match(r"^frame_(\d{6})\.png$", name)
    if m:
        n = int(m.group(1))
        seen[n] = seen.get(n, 0) + 1
missing = [n for n in range(expected) if n not in seen]
dupes = sorted(n for n, c in seen.items() if c > 1)
if missing or dupes:
    if missing:
        print(f"MISSING {len(missing)} frames: {missing[:50]}"
              + (" ..." if len(missing) > 50 else ""))
    if dupes:
        print(f"DUPLICATE frames: {dupes[:50]}")
    sys.exit(1)
print(f"frame sequence complete: {expected} frames")
EOF
  "$FFMPEG" -y -framerate $FPS -i "$SEQ/frame_%06d.png" \
    -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p \
    -g $FPS -keyint_min $FPS \
    "$VIDEO_ONLY"
else
  echo "== mode B: harvested chunk mp4s (seq has $NPNG loose frames)"
  python3 - "$MANIFESTS" "$CHUNKS" "$EXPECTED" <<'EOF'
import json, os, sys
manifests, chunks, expected = sys.argv[1], sys.argv[2], int(sys.argv[3])
covered = {}
missing_mp4 = []
for name in sorted(os.listdir(manifests)):
    if not name.startswith("chunk_") or not name.endswith(".json"):
        continue
    m = json.load(open(os.path.join(manifests, name)))
    mp4 = os.path.join(chunks, name.replace(".json", ".mp4"))
    if not os.path.exists(mp4):
        missing_mp4.append(os.path.basename(mp4))
    for f in range(m["frame0"], m["frame0"] + m["frameCount"]):
        covered[f] = covered.get(f, 0) + 1
missing = [f for f in range(expected) if f not in covered]
dupes = sorted(f for f, c in covered.items() if c > 1)
if missing_mp4 or missing or dupes:
    if missing_mp4:
        print(f"MISSING chunk encodes: {missing_mp4}")
    if missing:
        print(f"MISSING {len(missing)} frames: {missing[:50]}"
              + (" ..." if len(missing) > 50 else ""))
    if dupes:
        print(f"DUPLICATE frames: {dupes[:50]}")
    sys.exit(1)
print(f"chunk coverage complete: {expected} frames across "
      f"{len(os.listdir(chunks))} chunk encodes")
EOF
  CONCAT="$OUT/concat.txt"
  : > "$CONCAT"
  for f in "$CHUNKS"/chunk_*.mp4; do
    echo "file '$f'" >> "$CONCAT"
  done
  "$FFMPEG" -y -f concat -safe 0 -i "$CONCAT" -c copy "$VIDEO_ONLY"
  rm -f "$CONCAT"
fi

# --- final decoded-frame-count verification (both modes) ----------------
DECODED=$(decoded_frames "$VIDEO_ONLY")
[ "$DECODED" = "$EXPECTED" ] || {
  echo "FATAL: deliverable decodes $DECODED frames, expected $EXPECTED" >&2
  exit 1
}
echo "== video stream verified: $DECODED frames"

# --- mux audio + faststart, then proxy -----------------------------------
echo "== muxing 1440p full"
"$FFMPEG" -y -i "$VIDEO_ONLY" -i "$STEMS/mix.wav" \
  -map 0:v -map 1:a -c:v copy \
  -c:a aac -b:a 192k -shortest -movflags +faststart \
  "$OUT/$VP.full.mp4"

echo "== encoding 720p proxy"
"$FFMPEG" -y -i "$OUT/$VP.full.mp4" \
  -vf scale=1280:720 \
  -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p \
  -g $FPS -keyint_min $FPS -movflags +faststart \
  -c:a copy \
  "$OUT/$VP.proxy.mp4"

# --- checksums + measured stats ------------------------------------------
echo "== measured media"
DUR=$(python3 -c "print($T1 - $T0 + $PAD)")
for f in "$OUT/$VP.full.mp4" "$OUT/$VP.proxy.mp4"; do
  SZ=$(stat -f%z "$f")
  MBIT=$(python3 -c "print(f'{$SZ*8/$DUR/1e6:.2f}')")
  echo "$(basename "$f"): $SZ bytes, ~${MBIT} Mbit/s over ${DUR}s"
done
shasum -a 256 "$OUT/$VP.full.mp4" "$OUT/$VP.proxy.mp4" \
  | tee "$OUT/$VP-media.sha256"
echo "== done"
