#!/bin/bash
# Decomposition wave 1 evidence captures: before/after screenshots (BEFORE =
# the pristine origin/main battle files, AFTER = this wave's edited files)
# for each of the four decomposed episodes (6th Wisconsin's railroad-cut
# charge, the 147th New York's stranded stand, the 16th Maine's sacrifice
# hill, the Ziegler's Grove convergence), plus a perf-floor benchmark on the
# July 3 file (the film-safety-adjacent one) and the July 1 afternoon file.
# Reuses the existing generic BenchmarkHarness (-benchmark/-benchmarkTimes/
# -benchmarkPrefix/-battleFile) rather than a new day-specific harness class
# — no new C# was needed for this wave's captures.
#
# Requires: the Development standalone already built at
# app/Builds/P0Benchmark/BattleAtlasBenchmark.app (BenchmarkBuild.Build, run
# against THIS worktree's edited files so the "after" runs read the current
# scene/assets) and BEFORE_DIR holding pristine copies of the three touched
# battle files (same basenames, so -battleFile's BattleAssetName lookup for
# moments/manifest still resolves correctly).
#
# Usage: scripts/decomp1-captures.sh <before-dir> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
BEFORE_DIR="${1:?usage: decomp1-captures.sh <before-dir> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/decomposition-1}"
mkdir -p "$OUT"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

run() {
  local prefix="$1" battle="$2" times="$3"
  "$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "$prefix" \
    -benchmarkTimes "$times" -battleFile "$battle" \
    -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
}

echo "== 6th Wisconsin / 147th NY (July 1 morning): before/after =="
run "decomp1-before-morning" "$BEFORE_DIR/gettysburg-july1-morning.json" "9000,11700,12900"
run "decomp1-after-morning"  "$REPO/app/Assets/Battle/gettysburg-july1-morning.json" "9000,11700,12900"

echo "== 16th Maine (July 1 afternoon): before/after + full-day perf =="
run "decomp1-before-afternoon" "$BEFORE_DIR/gettysburg-july1-afternoon.json" "7200,10800,12600"
run "decomp1-after-afternoon"  "$REPO/app/Assets/Battle/gettysburg-july1-afternoon.json" "0,3600,7200,9000,10800,12600,18000"

echo "== Ziegler's Grove (July 3): before/after + film-window perf floor =="
run "decomp1-before-july3" "$BEFORE_DIR/gettysburg-july3.json" "9000,10500"
run "decomp1-after-july3"  "$REPO/app/Assets/Battle/gettysburg-july3.json" "0,3600,8160,8700,9000,10500,18000"

echo "== Results =="
ls -la "$OUT"/decomp1-*.png "$OUT"/decomp1-*-benchmark.json
