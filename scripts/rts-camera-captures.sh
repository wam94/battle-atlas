#!/bin/bash
# RTS camera slice evidence captures: builds the Development macOS
# standalone and runs it with -rtscamshots — a scripted flight strip
# (theater altitude, a zoom over the Angle wall crossing down to the 5 m
# distance floor, a low-altitude rotate, a pan along the Union line, a
# terrain-clearance shot on the Culp's Hill slope, and a dedicated
# dynamic-near-clip-floor shot) — producing one screenshot per pose plus
# rts-camera-poses.json (the exact pivot/yaw/pitch/distance/clearance/
# near-clip flown at each step). Also runs the standard -benchmark perf
# sample so the FPS floor is captured alongside.
#
# Requires gitignored inputs restored (data/heightmap, data/landcover,
# app/Assets/Generated, SoldierView media) and the Unity editor CLOSED
# for THIS project path (worktree runs leave the owner's editor alone).
#
# Usage: scripts/rts-camera-captures.sh [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures/rts-camera}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Building Development standalone (this takes a few minutes) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/rtscam-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Running the flight-strip capture harness =="
"$BIN" -rtscamshots -rtscamOut "$OUT" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Running the standard perf sample =="
"$BIN" -benchmark -benchmarkOut "$OUT" \
  -screen-fullscreen 0 -screen-width 1440 -screen-height 900

echo "== Results =="
ls -la "$OUT"
cat "$OUT/rts-camera-poses.json"
cat "$OUT/p0-benchmark.json"
