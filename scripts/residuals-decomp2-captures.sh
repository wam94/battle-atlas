#!/bin/bash
# residuals-decomp-2 evidence captures: before/after screenshots (BEFORE =
# the pristine origin/main battle files, AFTER = this pass's edited files)
# for the two decomposed episodes — the 21st Mississippi at the Peach
# Orchard/Trostle window (gettysburg-july2-afternoon.json) and the Colgrove
# split parity panel (gettysburg-july3.json) — plus perf-floor benchmarks
# on both touched files. Reuses the existing generic BenchmarkHarness
# (-benchmark/-benchmarkTimes/-benchmarkPrefix/-battleFile/-benchmarkCamera)
# — no new capture-harness C# needed.
#
# CAMERA NOTE: the before/after pairs use a close -benchmarkCamera override
# (BattleDirector.cs's LOD thresholds: SoldiersInDist=1500, RegimentsInDist
# =4000 — the scene's own committed default distance is 4000, the Block
# tier) so the decomposed units are visually distinguishable at all; the
# PERF runs use the DEFAULT camera (no override) to match the established
# ~59.5 avg-FPS floor exactly — an early attempt at close-in perf numbers
# read ~48-55 avg FPS, an artifact of the closer LOD tier rendering more
# detail, not a real regression (confirmed by re-running at the default
# distance: 59.5-59.9, matching every prior wave's floor).
#
# Requires: the Development standalone already built at
# app/Builds/P0Benchmark/BattleAtlasBenchmark.app (BenchmarkBuild.Build, run
# against THIS worktree) and BEFORE_DIR holding pristine copies of the
# touched battle files (same basenames).
#
# Usage: scripts/residuals-decomp2-captures.sh <before-dir> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
BEFORE_DIR="${1:?usage: residuals-decomp2-captures.sh <before-dir> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/residuals-decomp-2}"
mkdir -p "$OUT"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

run_cam() {
  local prefix="$1" set="$2" battle="$3" times="$4" cam="$5"
  "$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "${prefix}-${set}" \
    -benchmarkTimes "$times" -battleFile "$battle" -benchmarkCamera "$cam" \
    -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
}

run_default() {
  local prefix="$1" battle="$2" times="$3"
  "$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "$prefix" \
    -benchmarkTimes "$times" -battleFile "$battle" \
    -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
}

echo "== 21st Mississippi at the Peach Orchard/Trostle window (July 2 afternoon): before/after =="
# pivot near the Trostle yard (21st MS 3617,3643 / Bigelow stand 3806,3630);
# t=12000 (the divergence), t=12900 (the capture, cross-referenced to
# us-btty-bigelow's own keyframe), t=14340 (day's end). dist=1200 (Soldiers
# tier) resolves the ~350m divergence clearly.
run_cam "rdc2-before" "trostle" "$BEFORE_DIR/gettysburg-july2-afternoon.json" "0,12000,12900,14340" "3700,3630,200,55,1200"
run_cam "rdc2-after" "trostle" "$REPO/app/Assets/Battle/gettysburg-july2-afternoon.json" "0,12000,12900,14340" "3700,3630,200,55,1200"

echo "== Colgrove split parity panel (July 3 afternoon, the Angle-pinned film file): before/after =="
# pivot centered on Colgrove's works (us-colgrove 6080,4800 / us-2ma
# 6096,4865 / us-27in 6120,4885); t=0/t=10800 are the file's only Colgrove
# keyframes (static, "documented silence"). dist=900 for the tighter ~70m spread.
run_cam "rdc2-before" "colgrove" "$BEFORE_DIR/gettysburg-july3.json" "0,10800" "6090,4830,20,55,900"
run_cam "rdc2-after" "colgrove" "$REPO/app/Assets/Battle/gettysburg-july3.json" "0,10800" "6090,4830,20,55,900"

echo "== Perf floor: July 2 afternoon (default camera, touched file) =="
run_default "rdc2-perf-j2p" "$REPO/app/Assets/Battle/gettysburg-july2-afternoon.json" "0,9900,10800,12000,12600,12900,14340"

echo "== Perf floor: July 3 afternoon (default camera, Angle film window included) =="
run_default "rdc2-perf-j3a" "$REPO/app/Assets/Battle/gettysburg-july3.json" "0,8160,8700,9000,10800"

echo "== Results =="
ls -la "$OUT"/rdc2-*.png "$OUT"/rdc2-*-benchmark.json
