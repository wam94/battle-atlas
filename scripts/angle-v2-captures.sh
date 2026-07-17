#!/bin/bash
# angle-v2-data evidence captures: Atlas before/after screenshots (BEFORE =
# pristine origin/main gettysburg-july3.json, AFTER = this wave's re-timed
# file) at the re-timed bombardment moments t=420/3600/7200 (Kemper/
# Armistead/Fry visibly thinning on the waiting ground) and at t=8160/8700
# (the film window: 0-px identity expected — the total-by-step-off
# invariant, seen visually). The bombardment-prelude-captures.sh pattern,
# reusing the generic BenchmarkHarness.
#
# Requires: the Development standalone at
# app/Builds/P0Benchmark/BattleAtlasBenchmark.app (BenchmarkBuild.Build run
# against THIS worktree) and BEFORE_DIR holding the pristine
# gettysburg-july3.json (same basename).
#
# Usage: scripts/angle-v2-captures.sh <before-dir> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
BEFORE_DIR="${1:?usage: angle-v2-captures.sh <before-dir> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/angle-v2-data}"
mkdir -p "$OUT"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

run_cam() {
  local prefix="$1" battle="$2" times="$3" cam="$4"
  "$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "$prefix" \
    -benchmarkTimes "$times" -battleFile "$battle" -benchmarkCamera "$cam" \
    -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
}

run_default() {
  local prefix="$1" battle="$2" times="$3"
  "$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "$prefix" \
    -benchmarkTimes "$times" -battleFile "$battle" \
    -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
}

echo "== Pickett/Pettigrew waiting ground close-in: before/after, t=420/3600/7200 =="
# pivot on the assault column's sheltered start line (Garnett ~3200,4930;
# Kemper right-south; Armistead behind; Fry north ~3400,5100)
run_cam "av2-before-column" "$BEFORE_DIR/gettysburg-july3.json" "420,3600,7200" "3320,4900,0,55,3000"
run_cam "av2-after-column" "$REPO/app/Assets/Battle/gettysburg-july3.json" "420,3600,7200" "3320,4900,0,55,3000"

echo "== Film-window invariance (0-px identity expected): before/after, t=8160/8700 =="
run_cam "av2-before-window" "$BEFORE_DIR/gettysburg-july3.json" "8160,8700" "4300,4860,0,55,1400"
run_cam "av2-after-window" "$REPO/app/Assets/Battle/gettysburg-july3.json" "8160,8700" "4300,4860,0,55,1400"

echo "== Wide Atlas tabletop + HUD timeline: before/after, t=3600 =="
run_default "av2-before-atlas" "$BEFORE_DIR/gettysburg-july3.json" "420,3600,7200"
run_default "av2-after-atlas" "$REPO/app/Assets/Battle/gettysburg-july3.json" "420,3600,7200"

echo "== Perf floor: July 3 afternoon incl. the film window =="
run_default "av2-perf-j3a" "$REPO/app/Assets/Battle/gettysburg-july3.json" "0,420,3600,7200,7500,8160,8700,9000"

echo "== pixel diffs =="
python3 - "$OUT" <<'EOF'
import sys, os
try:
    import numpy as np
    from PIL import Image
except ImportError:
    print("PIL/numpy unavailable; skip diff"); sys.exit(0)
out = sys.argv[1]
for name in sorted(os.listdir(out)):
    if not name.startswith("av2-after") or not name.endswith(".png"):
        continue
    before = os.path.join(out, name.replace("av2-after", "av2-before"))
    if not os.path.exists(before):
        continue
    a = np.asarray(Image.open(before).convert("RGB"), dtype=np.int16)
    b = np.asarray(Image.open(os.path.join(out, name)).convert("RGB"), dtype=np.int16)
    if a.shape != b.shape:
        print(f"{name}: SHAPE MISMATCH"); continue
    d = (np.abs(a - b).sum(axis=2) > 10).sum()
    print(f"{name}: {d} of {a.shape[0]*a.shape[1]} px differ")
EOF
echo "== done =="
