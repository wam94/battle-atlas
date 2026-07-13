#!/bin/bash
# Day-expansion slice 3 evidence captures: builds the Development macOS
# standalone once, then runs it twice with -battleFile pointing at each
# July 1 phase file — Buford's stand, the Reynolds moment, the railroad
# cut, Iverson's field, Barlow's Knoll, the double collapse, the town
# retreat, the Cemetery Hill consolidation, and the July 1 day panel
# (all three days now carry reconstructed phases).
#
# Requires gitignored inputs restored (data/heightmap, data/landcover,
# app/Assets/Generated, SoldierView media) and the Unity editor CLOSED
# for THIS project path (worktree runs leave the owner's editor alone).
#
# Usage: scripts/dayexp3-captures.sh [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures/day-expansion-3}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Preparing the Atlas scene (heightmap + landcover import; saves the scene) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.CartographyStage.PrepareScene \
  -logFile "$OUT/dayexp3-prepare.log"

echo "== Building Development standalone (a few minutes; copies media) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/dayexp3-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Morning phase shots (a window opens; it quits itself) =="
"$BIN" -dayexp3shots -dayexp3set morning -dayexp3Out "$OUT" \
  -battleFile "$REPO/app/Assets/Battle/gettysburg-july1-morning.json" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Afternoon phase shots =="
"$BIN" -dayexp3shots -dayexp3set afternoon -dayexp3Out "$OUT" \
  -battleFile "$REPO/app/Assets/Battle/gettysburg-july1-afternoon.json" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Perf benchmark on the afternoon phase (~1 minute) =="
"$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "dayexp3-afternoon" \
  -benchmarkTimes "0,3600,9000,12600,18000" \
  -battleFile "$REPO/app/Assets/Battle/gettysburg-july1-afternoon.json" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Results =="
ls -la "$OUT"/dayexp3-*.png
