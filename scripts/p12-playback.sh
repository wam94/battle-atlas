#!/bin/bash
# Phase 12 playback verification (plan §12 P12): builds the Development
# macOS standalone and runs the automated product loop — Atlas playback,
# enter Soldier View on the production media (content warning included),
# play, a deterministic seek battery, play again, exit with exact-state
# return — capturing screenshots + FPS/memory/seek-latency evidence.
#
# Requires: production media staged (scripts/p11-demo.sh does that),
# generated terrain imported (README "Fresh-clone setup"), and the Unity
# editor CLOSED for THIS project path.
#
# Usage: scripts/p12-playback.sh [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures/p12-gate}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

for f in garnett-road-to-angle.full.mp4 garnett-road-to-angle.proxy.mp4; do
  [ -f "$REPO/app/Assets/StreamingAssets/SoldierView/$f" ] ||
    { echo "ERROR: $f not staged — run scripts/p11-demo.sh first." >&2; exit 2; }
done

echo "== Preparing the Atlas scene (terrain import; saves the scene) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.Phase12Review.PrepareStandaloneScene \
  -logFile "$OUT/p12-prepare.log"

echo "== Building Development standalone (a few minutes; copies media) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/p12-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Running the playback probe (a window opens; ~2 minutes; quits itself) =="
"$BIN" -p12probe -p12out "$OUT" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000
CODE=$?

echo "== Results (pass requires exit code 0) =="
echo "probe exit: $CODE"
ls -la "$OUT"/p12-*.png "$OUT"/p12-playback.json
exit $CODE
