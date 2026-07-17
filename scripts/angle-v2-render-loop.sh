#!/bin/sh
# ANGLE-V2 production render runner (adapted from p10-render-loop.sh;
# new output root — the shipped v1 media stays untouched).
#
# The render is chunked and resumable BY DESIGN (this machine kills
# long Unity batch processes: both production attempts were
# SIGKILLed — exit 137 — after ~4,800 frames with managed memory flat,
# i.e. a native-memory leak meets jetsam). This loop simply re-invokes
# Unity until every chunk manifest exists; each invocation skips
# complete chunks (harvested chunk encodes count as complete) and
# resumes at the first incomplete one.
#
# Usage: UNITY=/path/to/Unity scripts/angle-v2-render-loop.sh
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
MANIFESTS="$ROOT/app/RenderOutput/angle-v2/manifests"
CHUNKS=12
MAX_ATTEMPTS=12

attempt=0
while :; do
  n=$(ls "$MANIFESTS" 2>/dev/null | grep -c '^chunk_[0-9]\{3\}\.json$' || true)
  if [ "$n" -ge "$CHUNKS" ]; then
    echo "== all $CHUNKS chunk manifests present"
    break
  fi
  attempt=$((attempt + 1))
  [ "$attempt" -le "$MAX_ATTEMPTS" ] || {
    echo "FATAL: $MAX_ATTEMPTS attempts, still $n/$CHUNKS chunks" >&2
    exit 1
  }
  echo "== attempt $attempt: $n/$CHUNKS chunks complete, launching Unity"
  "$UNITY" -batchmode -projectPath "$ROOT/app" -buildTarget OSXUniversal \
    -executeMethod BattleAtlas.EditorTools.AngleV2Render.RenderProduction \
    -logFile "$ROOT/angle-v2-production-a$attempt.log" || \
    echo "== attempt $attempt exited nonzero (kill/crash); resuming"
done
touch "$ROOT/app/RenderOutput/angle-v2/render-done"
echo "== render loop done"
