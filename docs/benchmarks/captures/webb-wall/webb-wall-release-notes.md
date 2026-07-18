# Soldier View media — webb-wall

Pre-rendered Soldier View film `webb-wall` (battle window t=8160..8820 + 0.5s media-contract pad, 30 fps).
Deterministic render of the compiled July 3 Angle bundle (settingsHash `11e751d0e39ee4aad2e837cc31770782c983b8d2ff609fd6647514ab7c03f3c1`, git `c2408cc1e4b1`).

| file | bytes | sha256 |
| --- | --- | --- |
| webb-wall.full.mp4 | 1014939176 | `908634fc64ee4583d832e649f0c8f5560d8edb80fe3bdea28bf2620653285847` |
| webb-wall.proxy.mp4 | 201227997 | `1f454e2ec45081ab23d77a98c8695484c649849801d47c29539ab12ebdd88334` |

Publish (owner):
```sh
gh release create soldier-view-media-webb-wall-v1 \
  app/RenderOutput/webb-wall/webb-wall.full.mp4 \
  app/RenderOutput/webb-wall/webb-wall.proxy.mp4 \
  docs/benchmarks/captures/webb-wall/webb-wall-release-manifest.json \
  --title "Soldier View media — webb-wall" \
  --notes-file docs/benchmarks/captures/webb-wall/webb-wall-release-notes.md
```
