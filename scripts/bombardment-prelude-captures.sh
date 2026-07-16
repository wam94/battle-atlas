#!/bin/bash
# bombardment-prelude evidence captures: before/after screenshots (BEFORE =
# the pristine origin/main gettysburg-july3.json, AFTER = this pass's
# re-timed file) at t=7500/7800/8100 — the bombardment-to-step-off window,
# strictly before the t=8160 film pin — showing Marshall's/Brockenbrough's/
# Lane's brigades visibly thinning in Spangler's Woods before the charge
# steps off, plus the HUD timeline read. Reuses the existing generic
# BenchmarkHarness (-benchmark/-benchmarkTimes/-benchmarkPrefix/-battleFile/
# -benchmarkCamera) — no new capture-harness C# needed, matching
# residuals-decomp-2's own precedent.
#
# Requires: the Development standalone already built at
# app/Builds/P0Benchmark/BattleAtlasBenchmark.app (BenchmarkBuild.Build, run
# against THIS worktree) and BEFORE_DIR holding a pristine copy of
# gettysburg-july3.json (same basename).
#
# Usage: scripts/bombardment-prelude-captures.sh <before-dir> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
BEFORE_DIR="${1:?usage: bombardment-prelude-captures.sh <before-dir> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/bombardment-prelude}"
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

echo "== Spangler's Woods close-in (Marshall/Brockenbrough/Lane): before/after, t=7500/7800/8100 =="
# pivot centered on the three brigades' static ground (Marshall ~3270,5380;
# Brockenbrough ~3280,5730; Lane ~3080,5350) — dist=2200 (Regiments tier)
# so all three read together with the thinning visible.
run_cam "bp-before-woods" "$BEFORE_DIR/gettysburg-july3.json" "7500,7800,8100" "3200,5500,0,55,2200"
run_cam "bp-after-woods" "$REPO/app/Assets/Battle/gettysburg-july3.json" "7500,7800,8100" "3200,5500,0,55,2200"

echo "== Wide Atlas tabletop + HUD timeline: before/after, t=7500/7800/8100 (default camera) =="
run_default "bp-before-atlas" "$BEFORE_DIR/gettysburg-july3.json" "7500,7800,8100"
run_default "bp-after-atlas" "$REPO/app/Assets/Battle/gettysburg-july3.json" "7500,7800,8100"

echo "== Perf floor: July 3 afternoon (default camera, touched file, Angle film window included) =="
run_default "bp-perf-j3a" "$REPO/app/Assets/Battle/gettysburg-july3.json" "0,420,3600,7200,7500,7800,8100,8160,8700,9000,10800"

echo "== Results =="
ls -la "$OUT"/bp-*.png "$OUT"/bp-*-benchmark.json
