#!/bin/sh
# Phase 6 kit toolchain — reproducible setup (no Blender application).
#
# The previous executor environment was wiped by a machine crash; this
# script encodes what its committed scripts assumed:
#   1. Python 3.11 venv with the bpy 4.5.11 wheel (the wheel requires 3.11);
#   2. MPFB 2.0.16 installed as a Blender USER extension so headless bpy
#      can `addon_utils.enable("bl_ext.user_default.mpfb")` (ADR 0004;
#      license evidence: docs/assets/licenses/makehuman-mpfb-core/).
#
# Usage: characters/kit/setup_toolchain.sh <venv-dir>
#   then: <venv-dir>/bin/python characters/kit/build_kit.py
set -eu
VENV="${1:?usage: setup_toolchain.sh <venv-dir>}"

# --- 1. bpy venv (uv provisions Python 3.11 if the system lacks it)
if command -v uv >/dev/null 2>&1; then
  uv venv --python 3.11 "$VENV"
  uv pip install --python "$VENV/bin/python" bpy==4.5.11
else
  python3.11 -m venv "$VENV"
  "$VENV/bin/pip" install bpy==4.5.11
fi

# --- 2. MPFB extension (pinned; extensions.blender.org content-addressed)
SHA256="b5cdc8b08147e0c6463e4faa01147491b13a0b062f73415363f029debd11c934"
URL="https://extensions.blender.org/download/sha256:$SHA256/add-on-mpfb-v2.0.16.zip"
case "$(uname)" in
  Darwin) EXT_DIR="$HOME/Library/Application Support/Blender/4.5/extensions/user_default" ;;
  *)      EXT_DIR="$HOME/.config/blender/4.5/extensions/user_default" ;;
esac
if [ -d "$EXT_DIR/mpfb" ]; then
  echo "== MPFB already installed at $EXT_DIR/mpfb"
else
  TMP="$(mktemp -d)"
  curl -fsSL -o "$TMP/mpfb.zip" "$URL"
  echo "$SHA256  $TMP/mpfb.zip" | shasum -a 256 -c -
  mkdir -p "$EXT_DIR"
  unzip -q "$TMP/mpfb.zip" -d "$EXT_DIR/mpfb"
  # the zip may nest a single top-level dir; flatten to mpfb/__init__.py
  if [ ! -f "$EXT_DIR/mpfb/__init__.py" ]; then
    inner="$(find "$EXT_DIR/mpfb" -mindepth 1 -maxdepth 1 -type d | head -1)"
    mv "$inner"/* "$EXT_DIR/mpfb/"
    rmdir "$inner"
  fi
  rm -rf "$TMP"
  echo "== MPFB installed at $EXT_DIR/mpfb"
fi

# --- smoke test
"$VENV/bin/python" - <<'EOF'
import bpy  # noqa  (bpy must be imported FIRST: it registers the bundled
#                    script paths that make addon_utils importable)
import addon_utils  # noqa
addon_utils.enable("bl_ext.user_default.mpfb", default_set=True, persistent=True)
from bl_ext.user_default.mpfb.services.humanservice import HumanService  # noqa
print("== toolchain OK: bpy", bpy.app.version_string, "+ MPFB importable")
EOF
