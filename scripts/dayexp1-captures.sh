#!/bin/bash
# Day-expansion slice 1 evidence captures: builds the Development macOS
# standalone and runs it with -dayexp1shots — the widened timeline with
# day tabs, the sunset window's new content (16:00 quiet, ~17:30 sweep +
# Benning's retirement, Taft's evening fire, the 19:29 sunset), and the
# honest empty-day states.
#
# Requires gitignored inputs restored (data/heightmap, data/landcover,
# app/Assets/Generated, SoldierView media) and the Unity editor CLOSED
# for THIS project path (worktree runs leave the owner's editor alone).
#
# Usage: scripts/dayexp1-captures.sh [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures/day-expansion-1}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Preparing the Atlas scene (heightmap + landcover import; saves the scene) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.CartographyStage.PrepareScene \
  -logFile "$OUT/dayexp1-prepare.log"

echo "== Building Development standalone (a few minutes; copies media) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/dayexp1-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Screenshot battery (a window opens; it quits itself) =="
"$BIN" -dayexp1shots -dayexp1Out "$OUT" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Perf benchmark (playback profile; ~1 minute) =="
"$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "dayexp1" \
  -benchmarkTimes "0,8400,10800,16200,23340" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Results =="
ls -la "$OUT"/dayexp1-*.png
