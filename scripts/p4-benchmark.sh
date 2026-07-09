#!/bin/bash
# Phase 4 (Angle V2) HDRP capture: builds a Development macOS standalone from
# the migrated (HDRP) project and runs it with the BenchmarkHarness, producing
# hdrp-atlas-t{0,8160,8700,9000}.png plus FPS/memory samples in
# hdrp-atlas-benchmark.json — the same cameras, timestamps, window size, and
# method as the Phase 0 URP baselines (scripts/p0-benchmark.sh), so the
# before/after comparison isolates the pipeline.
#
# Requirements:
#   - Unity editor CLOSED for THIS project path (any other project is fine).
#   - macOS Build Support module installed.
#
# Usage: scripts/p4-benchmark.sh [output-dir]   (default: docs/benchmarks/captures)
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Building Development standalone (this takes a few minutes) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/p4-benchmark-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Running benchmark player (a window will open briefly; it quits itself) =="
"$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix hdrp-atlas \
  -screen-fullscreen 0 -screen-width 1440 -screen-height 900

echo "== Results =="
ls -la "$OUT"
cat "$OUT/hdrp-atlas-benchmark.json"
