#!/bin/sh
# Iverson production render runner (the P10 pattern, site-parameterized).
#
# The render is chunked and resumable BY DESIGN: this machine SIGKILLs
# long Unity batch processes (exit 137, native leak meets jetsam — see
# the P10 postmortem). This loop re-invokes Unity until every chunk
# manifest exists; each invocation skips complete chunks (harvested
# chunk encodes count as complete) and resumes at the first incomplete
# one.
#
# Usage: UNITY=/path/to/Unity scripts/iverson-render-loop.sh
set -eu
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
MANIFESTS="$ROOT/app/RenderOutput/iverson/manifests"
CHUNKS=21   # ceil((7040 - 5830 + 0.5) / 60) sixty-second chunks
MAX_ATTEMPTS=24

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
    -executeMethod BattleAtlas.EditorTools.IversonProductionRender.RenderProduction \
    -logFile "$ROOT/iverson-production-a$attempt.log" || \
    echo "== attempt $attempt exited nonzero (kill/crash); resuming"
done
touch "$ROOT/app/RenderOutput/iverson/render-done"
echo "== render loop done"
