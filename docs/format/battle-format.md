# Battle Track Format

The contract between the authoring tool (writer), the pipeline (future writer),
and the Unity app (reader). JSON, UTF-8. Machine-checkable schema:
[battle.schema.json](battle.schema.json). The Unity loader re-validates the
load-bearing rules at runtime (BattleData.cs).

## Conventions

- **Positions** are battlefield-local meters: `x` east, `z` north from the
  terrain's SW corner. Identical to Unity world XZ. The local frame is anchored
  to UTM zone 18N (EPSG:26918) by `origin_utm_e` / `origin_utm_n` in
  `data/heightmap/heightmap.json`; tools convert via
  `utm = origin + local`, then UTM→WGS84 for display on web maps.
- **Time**: `t` is seconds from battle start. `startTime` is seconds since
  local midnight of the battle day (e.g. 13:00 = 46800); display clock time is
  `startTime + t`. `endTime` is the battle duration in seconds (same axis as
  `t`), and must be ≥ every keyframe `t`.
- **Facing** is compass degrees: 0 = north (+Z in Unity), 90 = east.
- **Interpolation**: linear position/strength between keyframes, shortest-arc
  for facing; a segment carries its START keyframe's formation; state clamps
  before the first and after the last keyframe.

## Structure

| Field | Type | Rules |
|---|---|---|
| `name` | string | required |
| `startTime` | number | ≥ 0, seconds since local midnight |
| `endTime` | number | > 0, battle duration |
| `units[]` | array | ≥ 1 entry |
| `units[].id` | string | required, unique |
| `units[].name` | string | required |
| `units[].side` | string | `union` \| `confederate` |
| `units[].frontage_m` | number | > 0 |
| `units[].depth_m` | number | > 0 |
| `units[].regiments[]` | array | optional; ≥ 2 non-empty strings |
| `units[].keyframes[]` | array | ≥ 1, `t` strictly increasing |
| `keyframes[].t` | number | ≥ 0 |
| `keyframes[].x`, `.z` | number | battlefield-local meters |
| `keyframes[].facing` | number | compass degrees |
| `keyframes[].formation` | string | `column` \| `line` \| `skirmish` \| `scattered` \| `routed` |
| `keyframes[].strength` | number | ≥ 0, effectives at this moment |
| `keyframes[].confidence` | string | optional; `documented` \| `inferred` \| `unknown` (default `unknown`) |
| `keyframes[].citation` | string | optional; REQUIRED non-empty when `confidence == "documented"` |

## Regiment rosters

`units[].regiments` is an optional display subdivision roster: the named
regiments (or battalions) that make up the unit, in order. Order is a display
convention — right-to-left along the frontage as the unit faces — used when
the app renders regiment-level sub-blocks; it is attested where a source
gives it explicitly (e.g. Rawley Martin's regiment-by-regiment description of
Pickett's division) and otherwise follows the order the source lists the
regiments in, which may not reflect actual battlefield placement. The array
carries no per-regiment position, strength, or keyframe tracking — that's a
planned extension. Units without a `regiments` roster render as a single
block at every zoom level.

## The no-faking gate

The authoring tool refuses to export a keyframe claiming `documented`
confidence without a citation. Provenance will render in-app from Phase 4 (documented = solid,
inferred = ghosted, unknown = explicit "no reliable record"); today the
Unity loader parses but does not yet display it.

## Planned extensions (not yet valid)

- Per-keyframe `frontage_m`/`depth_m` (formation morphing)
- `engagement` flags (drives smoke/audio), path splines, comms/moments files
