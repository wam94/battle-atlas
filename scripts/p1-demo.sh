#!/bin/bash
# Gate P1 demo entry point (one command): ensures the dev timecode proxy
# exists, validates the viewpoint metadata, and opens the Unity editor on the
# project. Then follow docs/reconstruction/p1-owner-checklist.md.
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_APP="${UNITY_APP:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app}"

echo "== 1/3 dev proxy =="
PROXY="$REPO/app/Assets/StreamingAssets/SoldierView/dev-timecode.proxy.mp4"
if [ -f "$PROXY" ]; then
  echo "present: $PROXY"
else
  "$REPO/reconstruction/scripts/generate_dev_proxy.sh"
fi

echo "== 2/3 viewpoint metadata =="
(cd "$REPO/reconstruction" && uv run --quiet python scripts/validate_viewpoints.py \
  "$REPO/app/Assets/StreamingAssets/SoldierView/viewpoints.json")

echo "== 3/3 Unity editor =="
if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "The editor is already open on this project — switch to it."
else
  open -a "$UNITY_APP" --args -projectPath "$REPO/app"
  echo "Editor launching..."
fi

cat <<'EOF'

Next: in Unity open Assets/Scenes/Atlas.unity, press Play, and follow
docs/reconstruction/p1-owner-checklist.md (scrub to ~t=8160, i.e. 15:16).
EOF
