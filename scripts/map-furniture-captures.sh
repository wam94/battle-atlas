#!/bin/bash
# Map-furniture slice evidence captures: runs the already-built Development
# standalone with -mapfurnshots (screenshot battery: theater/mid/tactical/
# rts-low, centered on the Diamond) and -benchmark (standard perf sample).
#
# Usage: scripts/map-furniture-captures.sh <prefix> [output-dir]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
PREFIX="${1:?usage: map-furniture-captures.sh <prefix> [output-dir]}"
OUT="${2:-$REPO/docs/benchmarks/captures/map-furniture}"
mkdir -p "$OUT"

APP="$REPO/app/Builds/P0Benchmark/BattleAtlasBenchmark.app"
BIN="$(ls "$APP/Contents/MacOS/"* | head -1)"

echo "== Screenshot battery =="
"$BIN" -mapfurnshots -mapfurnOut "$OUT" -mapfurnPrefix "$PREFIX" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Perf benchmark =="
"$BIN" -benchmark -benchmarkOut "$OUT" -benchmarkPrefix "$PREFIX" \
  -screen-fullscreen 0 -screen-width 1600 -screen-height 1000

echo "== Results =="
ls -la "$OUT"/$PREFIX-*
