# Soldier View media — cushing-canister

Pre-rendered Soldier View film `cushing-canister` (battle window t=8400..8760 + 0.5s media-contract pad, 30 fps).
Deterministic render of the compiled July 3 Angle bundle (settingsHash `2fbed41f3b38b7dc12508adbea05cfc65e4594833156505429e44d4e7f84305d`, git `bb79cc61e25b`).

| file | bytes | sha256 |
| --- | --- | --- |
| cushing-canister.full.mp4 | 431728399 | `69c6be7f56b0aa64ec7bfbebf4ce8d7c7d7fdb19fa4c264331e2dc66580909fd` |
| cushing-canister.proxy.mp4 | 103710405 | `75067d24dc21c1b6238582bd072c4e0ed3ebbac9b23bfbad1122744429bb7a3a` |

Publish (owner):
```sh
gh release create soldier-view-media-cushing-canister-v1 \
  app/RenderOutput/cushing-canister/cushing-canister.full.mp4 \
  app/RenderOutput/cushing-canister/cushing-canister.proxy.mp4 \
  docs/benchmarks/captures/cushing-canister/cushing-canister-release-manifest.json \
  --title "Soldier View media — cushing-canister" \
  --notes-file docs/benchmarks/captures/cushing-canister/cushing-canister-release-notes.md
```
