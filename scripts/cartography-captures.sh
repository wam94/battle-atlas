#!/bin/bash
# Cartography-slice evidence captures: builds the Development macOS
# standalone and runs it twice — once with -cartoshots (the altitude-
# preset screenshot battery at t=5400 and t=8400) and once with
# -benchmark (the standard perf sample at the Phase 0 reference times).
# Run BEFORE and AFTER the slice with different prefixes so the owner
# compares like against like.
#
# Requires gitignored inputs restored (data/heightmap, data/landcover,
# app/Assets/Generated, SoldierView media) and the Unity editor CLOSED
# for THIS project path (worktree runs leave the owner's editor alone).
#
# Usage: scripts/cartography-captures.sh <prefix> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
PREFIX="${1:?usage: cartography-captures.sh <prefix> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/cartography}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Preparing the Atlas scene (heightmap + landcover import; saves the scene) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.CartographyStage.PrepareScene \
  -logFile "$OUT/$PREFIX-prepare.log"

echo "== Building Development standalone =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/$PREFIX-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Screenshot battery (a window opens; it quits itself) =="
"$BIN" -cartoshots -cartoOut "$OUT" -cartoPrefix "$PREFIX" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Perf benchmark (playback profile; ~1 minute) =="
"$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "$PREFIX" \
  -benchmarkTimes "0,5400,8400,8700" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Results =="
ls -la "$OUT"/$PREFIX-*
