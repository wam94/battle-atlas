# Soldier View media v2 — With the Twelfth North Carolina

Pre-rendered first-person Soldier View media for the Battle Atlas
`iverson-forney-field` viewpoint (the destruction of Iverson's brigade,
Forney field / Oak Ridge, July 1, 1863; July 1 afternoon battle clock
t=5830..7040, 20.2 minutes at 30 fps, plus a 0.5 s seek-guard pad).

| file | content | sha256 |
| --- | --- | --- |
| `iverson-forney-field.full.mp4` | full 2560x1440p30, 1.98 GB, ~13.1 Mbit/s | `aca0536d3e17cc67…` |
| `iverson-forney-field.proxy.mp4` | proxy 1280x720p30, 0.40 GB, ~2.63 Mbit/s | `1345d5b889e20e4b…` |

Rendered deterministically at commit `6dd419d6ce3d`
(settings hash `79251743299540c6…`, ED-21 production seed
pin `2f15dd2f4e5e…`); reproduction:
`docs/reconstruction/render-runbook.md` with the Iverson entry points
(`IversonProductionRender`, `scripts/iverson-*.sh`). Install: place
both files in `app/Assets/StreamingAssets/SoldierView/`.

Content warning: this reconstruction depicts the most concentrated
killing in the corpus, soberly and without gameplay framing; the film
ships behind its own first-entry warning (see
`docs/reconstruction/violence-and-representation.md` and the
`iverson-forney-field` entry in `content-warning.json`).
