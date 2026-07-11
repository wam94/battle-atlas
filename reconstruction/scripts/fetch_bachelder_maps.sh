#!/bin/sh
# Fetch the full-resolution map masters for the spatial-evidence program:
# the 28-sheet Bachelder timed troop-position set (23 main-field + 5 East
# Cavalry Field; Rumsey scans of the 1995 Morningside reproduction, hosted
# on Internet Archive) and Elliott's 1864 burial map (Library of Congress).
#
# data/maps/ is gitignored (data/* rule); this script + the manifest at
# reconstruction/spatial/bachelder-manifest.json are the committed record
# (per-sheet provenance, license basis, sha256, georeference).
# Pattern: characters/spike/fetch_base_meshes.sh (pinned URL + sha256).
#
# Credit (Rumsey scans): "David Rumsey Map Collection, David Rumsey Map
# Center, Stanford Libraries" - CC BY-NC-SA 3.0 formal license; see the
# manifest's license notes and docs/research/
# 2026-07-02-bachelder-timed-set-acquisition.md section 3 for the full
# rights analysis (Warren 1873 base is PD; Bachelder overlay 303(a)
# discussion). Elliott: published 1864, public domain (LOC).
#
# Usage: fetch_bachelder_maps.sh [dest]   (default: data/maps)
set -eu
DEST="${1:-data/maps}"
mkdir -p "$DEST/bachelder" "$DEST/elliott"

fetch() {
  url="$1"; sha="$2"; out="$DEST/$3"
  if [ ! -s "$out" ]; then
    echo "fetching $3"
    curl -fSL --retry 3 --retry-delay 5 -o "$out.part" "$url"
    mv "$out.part" "$out"
  fi
  echo "$sha  $out" | shasum -a 256 -c -
}

fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-1-pre-battle-july-1-1863-12440001/12440001.jp2" "5f05f32f75f093e1818f05e41112b7affdba5f357cdccb13f26f09552388af8a" "bachelder/12440001.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-2-pre-battle-july-1-1863-12440002/12440002.jp2" "99019849a4341c0162f3e1b5ba942e5db53a9d7247f8a4f1a09fee53a3729faf" "bachelder/12440002.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-3-start-of-battle-july-1-1863-12440003/12440003.jp2" "9f139aad6fc6a23e94a9045559dd70666b9bb820eac36ef24752e4c3521c9cbf" "bachelder/12440003.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-4-1130-am-july-1-1863-12440004/12440004.jp2" "08561cd05c4a7aef6e25a275cf826c7394e39da1b79b289be51d5bc4eb9abefe" "bachelder/12440004.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-5-12-noon-july-1-1863-12440005/12440005.jp2" "eb954bd8f5ef52c77cbfd5e77006ed27b7e7503ef2d8e5de1fd9c3d38955c0a5" "bachelder/12440005.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-6-1230-pm-july-1-1863-12440006/12440006.jp2" "04318d7558070a1b27210e9f767aecdb73d811f4a7040c5239933e10d81ade2d" "bachelder/12440006.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-7-1-pm-july-1-1863-12440007/12440007.jp2" "6322bf1cd546292c8810678ae8a31aa2f197818a0f5b1f638291e1380b16f6a9" "bachelder/12440007.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-8-130-pm-july-1-1863-12440008/12440008.jp2" "4cef1c27d247e5cfc55b89ee9f8a11d6b01915d10481f3184dc7ee4858dd3391" "bachelder/12440008.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-9-2-pm-july-1-1863-12440009/12440009.jp2" "85db4fd718ea466ccd0e604854c51f25e908a49c7d66db9687267e8558422db4" "bachelder/12440009.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-10-230-pm-july-1-1863-12440010/12440010.jp2" "8a0324c6f9d10fd4fa14c7a8b776d42a29832efb3f9f3b0b93247847ae43f9ae" "bachelder/12440010.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-11-3-pm-july-1-1863-12440011/12440011.jp2" "6ec26f84785b0b38d0d17f2a6dd70e2bfa13f6348c4a437a3f251fa79e060bf4" "bachelder/12440011.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-12-330-pm-july-1-1863-12440012/12440012.jp2" "a3fbb5a398bb827d222e7f6d7f602a85f44a3cbe2d0b40c389511d6baec3340e" "bachelder/12440012.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-13-4-pm-july-1-1863-12440013/12440013.jp2" "3c434b00e018f0fd8de77c3a767eeabdf2881cf569922fcca9ce12c7b3a5da13" "bachelder/12440013.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-14-430-pm-july-1-1863-12440014/12440014.jp2" "61157c5528f53f81d8cf5813b1236b2626792df6692f68394424675dfe9eeb9f" "bachelder/12440014.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-1-9-am-july-2-1863-12440015/12440015.jp2" "83e7a3c435fe1b45475cf3128f8280a16a69b45bb524f0507d605fd370d2d1a9" "bachelder/12440015.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-2-4-pm-july-2-1863-12440016/12440016.jp2" "df88b4e31be90533f63836502fa24c1b43745e8c551e9c7db46029a51443f24c" "bachelder/12440016.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-3-5-pm-july-2-1863-12440017/12440017.jp2" "96570d8be5908d45ab5840c1ace22398510e95765ed0ed651e081d6345a763b4" "bachelder/12440017.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-4-7-pm-july-2-1863-12440018/12440018.jp2" "c6cfbb642d4eb252ee84a8b98d093ff6b0f0de45cf282b28090e5e8a6b5039c3" "bachelder/12440018.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-5-8-9-pm-july-2-1863-12440019/12440019.jp2" "af0ad433c955e69d0207d0f0a33330fa2d26893184a4aea9f65b6928bdcc0d85" "bachelder/12440019.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-1-no-6-4-8-am-july-3-1863-12440020/12440020.jp2" "45035c19d3aca50c8a8ca143753fd6d818afcfa13f7739a2a96f6e9c39a90e88" "bachelder/12440020.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-2-no-7-8-11-am-july-3-1863-12440021/12440021.jp2" "7cf9dd373fde0f94ce9bc001f0318a5814861ee09e0e2bb38961e6e31d91cb2a" "bachelder/12440021.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-3-no-8-1-5-pm-july-3-1863-12440022/12440022.jp2" "747fef56b7ebab92deab7a6dff309587996d3c12766aa665a138c0c53dd4ed38" "bachelder/12440022.jp2"
fetch "https://archive.org/download/dr_battle-field-of-gettysburg-no-4-no-9-5-pm-july-3-1863-12440023/12440023.jp2" "46717b99e594e40d7e160c97c45f30cee2a5073ad6e48a80b1f14fe039eb9576" "bachelder/12440023.jp2"
fetch "https://archive.org/download/dr_map-of-the-field-of-operations-of-greggs-union--stuarts-confederate-12441001/12441001.jp2" "fb271861f46388b76ae38db75dafe381976e13a36ffe845c9b03115627660ac5" "bachelder/12441001.jp2"
fetch "https://archive.org/download/dr_map-of-the-field-of-operations-of-greggs-union--stuarts-confederate-12441002/12441002.jp2" "eec9f56695bc0fd004274664d572ef732cef4341f215c89db5e244e37327d2c0" "bachelder/12441002.jp2"
fetch "https://archive.org/download/dr_map-of-the-field-of-operations-of-greggs-union--stuarts-confederate-12441003/12441003.jp2" "5b3ab483a9377fa5ce3c2ff1601b35d09176d7e2795523e4e1c37f1e37826e70" "bachelder/12441003.jp2"
fetch "https://archive.org/download/dr_map-of-the-field-of-operations-of-greggs-union--stuarts-confederate-12441004/12441004.jp2" "70f95831ce48322b8eb1ed01a9845e1291ca0fe8488e770c72e04abaa86ec62e" "bachelder/12441004.jp2"
fetch "https://archive.org/download/dr_map-of-the-field-of-operations-of-greggs-union--stuarts-confederate-12441005/12441005.jp2" "a2969e76d1b9b16d2d40f2b52a4374db7a4561b7d3310405cea8d8a66fe902a7" "bachelder/12441005.jp2"

fetch "https://tile.loc.gov/storage-services/service/gmd/gmd382/g3824/g3824g/cw0332000.jp2" "6d4275a35b839b405aab54f62b584ce77ff18cdc68bb2d175bc5e1c21d1fa717" "elliott/cw0332000.jp2"

echo "all map masters verified in $DEST (~395 MB)"
