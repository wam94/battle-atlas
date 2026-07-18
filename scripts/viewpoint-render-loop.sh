#!/bin/sh
# Viewpoint production render runner (webb-cushing slice) — the
# p10-render-loop.sh pattern parameterized by viewpoint id.
#
# The render is chunked and resumable BY DESIGN: this machine SIGKILLs
# long Unity batch processes (exit 137, native leak meets jetsam; see
# docs/reconstruction/p10-gate-evidence.md). The loop re-invokes Unity
# until every chunk manifest exists; each invocation skips complete
# chunks (harvested chunk encodes count as complete).
#
# Usage: UNITY=/path/to/Unity scripts/viewpoint-render-loop.sh <viewpointId> <chunkCount>
set -eu
[ $# -eq 2 ] || { echo "usage: $0 <viewpointId> <chunkCount>" >&2; exit 2; }
VP="$1"
CHUNKS="$2"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app/Contents/MacOS/Unity}"
MANIFESTS="$ROOT/app/RenderOutput/$VP/manifests"
MAX_ATTEMPTS=16

attempt=0
while :; do
  n=$(ls "$MANIFESTS" 2>/dev/null | grep -c '^chunk_[0-9]\{3\}\.json$' || true)
  if [ "$n" -ge "$CHUNKS" ]; then
    echo "== all $CHUNKS chunk manifests present for $VP"
    break
  fi
  attempt=$((attempt + 1))
  [ "$attempt" -le "$MAX_ATTEMPTS" ] || {
    echo "FATAL: $MAX_ATTEMPTS attempts, still $n/$CHUNKS chunks" >&2
    exit 1
  }
  echo "== attempt $attempt: $n/$CHUNKS chunks complete, launching Unity"
  "$UNITY" -batchmode -projectPath "$ROOT/app" -buildTarget OSXUniversal \
    -executeMethod BattleAtlas.EditorTools.ViewpointRender.RenderProduction \
    -viewpointId "$VP" \
    -logFile "$ROOT/$VP-production-a$attempt.log" || \
    echo "== attempt $attempt exited nonzero (kill/crash); resuming"
done
touch "$ROOT/app/RenderOutput/$VP/render-done"
echo "== render loop done ($VP)"
