#!/bin/bash
# Gate P11 UI-state screenshots: builds the Development macOS standalone
# (the Phase 0 benchmark build) and runs it with -p11shots, producing
# p11-*.png previews of the new HUD states. Requires the Soldier View
# media staged (scripts/p11-demo.sh does that) and the Unity editor
# CLOSED for THIS project path.
#
# Usage: scripts/p11-screenshots.sh [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures/p11-gate}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Building Development standalone (a few minutes; copies media) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/p11-shots-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Running screenshot pass (a window opens briefly; it quits itself) =="
"$BIN" -p11shots -p11out "$OUT" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Results =="
ls -la "$OUT"/p11-*.png
