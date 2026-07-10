#!/bin/bash
# Gate P11 demo entry point (one command): stages the production Soldier
# View media into StreamingAssets, validates the runtime metadata and the
# generated attribution outputs, and opens the Unity editor on the project.
# Then follow docs/reconstruction/p11-owner-checklist.md.
#
# Media staging (media is gitignored — plan §10.1): the P10 encodes are
# looked for, in order, at
#   1. $P11_MEDIA_DIR (if set),
#   2. docs/benchmarks/captures/p10-gate/ (the gate-evidence copies),
# and copied into app/Assets/StreamingAssets/SoldierView/ if missing there.
# Checksums are verified against docs/benchmarks/captures/p10-gate/
# p10-media.sha256 (the P10 release manifest's values). Without the
# encodes the app still runs and degrades per the P1 contract (clear
# refusal / proxy fallback), but the Gate P11 loop needs them.
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_APP="${UNITY_APP:-/Applications/Unity/Hub/Editor/6000.4.11f1/Unity.app}"
DEST="$REPO/app/Assets/StreamingAssets/SoldierView"
GATE="$REPO/docs/benchmarks/captures/p10-gate"

echo "== 1/4 Soldier View media =="
stage_one() {
  local name="$1" found=""
  if [ -f "$DEST/$name" ]; then
    echo "present: $DEST/$name"
    return 0
  fi
  for src in "${P11_MEDIA_DIR:-}" "$GATE"; do
    [ -n "$src" ] && [ -f "$src/$name" ] && { found="$src/$name"; break; }
  done
  if [ -z "$found" ]; then
    echo "WARNING: $name not found (set P11_MEDIA_DIR or restore $GATE/$name)."
    echo "         The app will degrade gracefully, but the P11 loop needs it."
    return 0
  fi
  echo "staging $found -> $DEST/"
  cp "$found" "$DEST/$name"
}
stage_one garnett-road-to-angle.proxy.mp4
stage_one garnett-road-to-angle.full.mp4
if [ ! -f "$DEST/dev-timecode.proxy.mp4" ]; then
  "$REPO/reconstruction/scripts/generate_dev_proxy.sh"
fi

echo "== 2/4 media checksums =="
if [ -f "$GATE/p10-media.sha256" ]; then
  # manifest lines may carry render-workspace paths; match by basename
  (cd "$DEST" && while read -r sum path; do
      name="$(basename "$path")"
      case "$name" in garnett-road-to-angle.*.mp4) ;; *) continue ;; esac
      if [ -f "$name" ]; then
        echo "$sum  $name" | shasum -a 256 -c - ||
          { echo "ERROR: $name checksum mismatch — restage from the P10 release"; exit 1; }
      fi
    done < "$GATE/p10-media.sha256")
else
  echo "NOTE: $GATE/p10-media.sha256 missing; skipping checksum verification."
fi

echo "== 3/4 metadata + attribution =="
(cd "$REPO/reconstruction" && uv run --quiet python scripts/validate_viewpoints.py \
  "$DEST/viewpoints.json")
(cd "$REPO/reconstruction" && uv run --quiet python scripts/generate_attribution.py \
  --check "$REPO")

echo "== 4/4 Unity editor =="
if pgrep -f "MacOS/Unity -projectpath $REPO/app" >/dev/null 2>&1 ||
   pgrep -f "MacOS/Unity.*-projectPath $REPO/app" >/dev/null 2>&1; then
  echo "The editor is already open on this project — switch to it."
else
  open -a "$UNITY_APP" --args -projectPath "$REPO/app"
  echo "Editor launching..."
fi

cat <<'EOF'

Next: in Unity open Assets/Scenes/Atlas.unity, press Play, and follow
docs/reconstruction/p11-owner-checklist.md (the full loop: find the charge,
enter With Garnett's Line inside 15:16-15:27, seek, inspect sources, exit).
EOF
