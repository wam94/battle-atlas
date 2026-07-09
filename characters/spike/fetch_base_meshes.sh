#!/bin/sh
# Fetch the CC0 Blender Human Base Meshes bundle v1.4.1 (Blender Studio).
# The bundle .blend is 62 MB — over the ADR 0002 per-file commit limit — so
# it is fetch-reproducible rather than committed. License evidence:
# docs/assets/licenses/blender-human-base-meshes-v1.4.1/acquisition.json
set -eu
URL="https://download.blender.org/demo/asset-bundles/human-base-meshes/human-base-meshes-bundle-v1.4.1.zip"
SHA256="811f43accbb31a88266d932f8f5563b2d13586fca0ba2693aad1f5fe582b3515"
DEST="${1:-./downloads}"
mkdir -p "$DEST"
ZIP="$DEST/human-base-meshes-bundle-v1.4.1.zip"
[ -f "$ZIP" ] || curl -fsSL -o "$ZIP" "$URL"
echo "$SHA256  $ZIP" | shasum -a 256 -c -
unzip -o -q "$ZIP" -d "$DEST"
echo "bundle at $DEST/human-base-meshes-bundle-v1.4.1/human_base_meshes_bundle.blend"
