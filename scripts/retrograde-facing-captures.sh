#!/bin/bash
# Retrograde-facing slice evidence captures: before/after screenshots
# (BEFORE = the pristine origin/main battle files, AFTER = this wave's
# fixed files) for two known retreats — von Gilsa's brigade breaking at
# Barlow's Knoll (July 1 collapse/retreat-through-town) and Marshall's
# brigade recrossing to Seminary Ridge after Pickett's repulse (July 3) —
# using the new RetrogradeFacingCaptureHarness (-retrofacingshots/
# -retrofacingset/-retrofacingOut), same mold as the day-expansion capture
# harnesses.
#
# Requires: the Development standalone already built at
# app/Builds/P0Benchmark/BattleAtlasBenchmark.app (BenchmarkBuild.Build, run
# against THIS worktree so the harness class is compiled in) and BEFORE_DIR
# holding pristine copies of gettysburg-july1-afternoon.json and
# gettysburg-july3.json (same basenames, so -battleFile's sibling-directory
# lookup still resolves).
#
# Usage: scripts/retrograde-facing-captures.sh <before-dir> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
BEFORE_DIR="${1:?usage: retrograde-facing-captures.sh <before-dir> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/retrograde-facing}"
mkdir -p "$OUT"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

run() {
  local prefix="$1" set="$2" battle="$3"
  "$BIN" -retrofacingshots -retrofacingOut "$OUT" -retrofacingset "$set" \
    -battleFile "$battle" \
    -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
  # each run's shots share the harness's own -retrofacing-<set>-<name>.png
  # naming, so before/after runs would collide — rename with the prefix.
  for f in "$OUT"/retrofacing-"$set"-*.png; do
    [ -e "$f" ] || continue
    mv "$f" "$OUT/${prefix}-$(basename "$f")"
  done
}

echo "== von Gilsa's brigade (July 1 collapse): before/after =="
run "before" "collapse" "$BEFORE_DIR/gettysburg-july1-afternoon.json"
run "after"  "collapse" "$REPO/app/Assets/Battle/gettysburg-july1-afternoon.json"

echo "== Marshall's brigade (July 3 Pickett recross): before/after =="
run "before" "pickett" "$BEFORE_DIR/gettysburg-july3.json"
run "after"  "pickett" "$REPO/app/Assets/Battle/gettysburg-july3.json"

echo "== Results =="
ls -la "$OUT"/before-*.png "$OUT"/after-*.png
