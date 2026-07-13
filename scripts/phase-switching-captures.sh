#!/bin/bash
# Phase-switching slice evidence: builds the Development macOS standalone
# once, then runs it ONCE with -phaseswitchshots — a single session that
# visits all five reconstructed phases through the in-HUD switcher
# (AtlasHud.SwitchToPhase, the day panel's own code path): a distinct
# battle moment + the owning day panel (honest notes included) per phase,
# every switch's measured time, and steady-state FPS on the same July 3
# view before any switch and after the five-phase round trip
# (phase-switch-summary.json). No -battleFile: the build itself carries
# the phase battle files (BenchmarkBuild copies Assets/Battle/*.json into
# the bundle's StreamingAssets), which is the shipped mechanism.
#
# Requires gitignored inputs restored (data/heightmap, data/landcover,
# app/Assets/Generated, SoldierView media) and the Unity editor CLOSED
# for THIS project path (worktree runs leave the owner's editor alone).
#
# Usage: scripts/phase-switching-captures.sh [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
OUT="${1:-$REPO/docs/benchmarks/captures/phase-switching}"
mkdir -p "$OUT"

if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "ERROR: the Unity editor is open on $REPO/app — close it first." >&2
  exit 2
fi

echo "== Preparing the Atlas scene (heightmap + landcover import; saves the scene) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.CartographyStage.PrepareScene \
  -logFile "$OUT/phaseswitch-prepare.log"

echo "== Building Development standalone (a few minutes; copies media + battle files) =="
"$UNITY" -batchmode -quit -projectPath "$REPO/app" -buildTarget OSXUniversal \
  -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build \
  -logFile "$OUT/phaseswitch-build.log"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== One session, five phases (a window opens; it quits itself) =="
"$BIN" -phaseswitchshots -phaseswitchOut "$OUT" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Results =="
ls -la "$OUT"/phaseswitch-*.png "$OUT"/phase-switch-summary.json
cat "$OUT"/phase-switch-summary.json
