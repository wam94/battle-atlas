#!/bin/bash
# Phase 0 (Angle V2) baseline capture: builds a Development macOS standalone
# and runs it with the BenchmarkHarness, producing screenshots at
# t=0/8160/8700/9000 plus FPS/memory samples in p0-benchmark.json.
#
# Requirements:
#   - Unity editor CLOSED for THIS project path (any other project is fine).
#   - macOS Build Support module installed.
#
# Usage: scripts/p0-benchmark.sh [output-dir]   (default: docs/benchmarks/captures)
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
  -logFile "$OUT/p0-benchmark-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Running benchmark player (a window will open briefly; it quits itself) =="
"$BIN" -benchmark -benchmarkOut "$OUT" \
  -screen-fullscreen 0 -screen-width 1440 -screen-height 900

echo "== Results =="
ls -la "$OUT"
cat "$OUT/p0-benchmark.json"
