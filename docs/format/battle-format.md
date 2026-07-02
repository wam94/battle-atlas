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
| `events[]` | array | optional; see Engagement events |
| `environment` | object | optional; see Environment |
| `units[]` | array | ≥ 1 entry |
| `units[].id` | string | required, unique |
| `units[].name` | string | required |
| `units[].side` | string | `union` \| `confederate` |
| `units[].frontage_m` | number | > 0 |
| `units[].depth_m` | number | > 0 |
| `units[].regiments[]` | array | optional; ≥ 2 non-empty strings |
| `units[].parent` | string | optional; id of an existing unit, depth 1 only (see Parent / children) |
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
carries no per-regiment position, strength, or keyframe tracking — regiments
that CAN be tracked become first-class child units via `parent` (see Parent /
children below); rosters remain the display-LOD path for undecomposed
brigades. Units without a `regiments` roster render as a single block at
every zoom level.

## Parent / children

`units[].parent` — optional string; must reference an existing unit id; depth 1
only (a parent may not itself have a parent). Draft-07 can't express cross-unit
references — these are code rules in the tool's `validate.ts` and thrown errors
in the Unity loader's `BattleLoader.Parse`.

- **Full decomposition or none:** a unit with children must NOT carry a
  `regiments` roster (validator error; loader throws). A brigade either keeps
  its display-only roster exactly as today or decomposes completely into child
  units. This kills the residual-slot ambiguity (who renders the un-promoted
  regiments? where does their strength live?) before it exists. Children MAY
  carry rosters (e.g. a wing that rosters its regiment pair).
- **Parent keyframes are unchanged** — they keep whole-brigade strength and
  remain the far-tier record. Children carry per-regiment strengths. The tool
  warns (advisory, not error) when children's t=0 strengths sum outside ±15%
  of the parent's — known-short decompositions exist where a fragment stays
  unmodeled but the parent's strength keeps it counted.
- **Rendering contract by tier** (family-atomic, keyed on the PARENT's center
  distance so a family never half-swaps): Block tier (>4 km) — parent renders
  its block, children hidden; Regiments tier (1.5–4 km) — parent hidden,
  children render as their own block markers (a child with a roster may
  sub-block via the existing regiment-slot path); Soldiers tier (<1.5 km) —
  children render figures. Existing hysteresis bands unchanged.
- **Backward compatibility:** units with no `parent` and no children behave
  exactly as today, all three tiers, roster sub-blocks included. Batteries
  unchanged. Additive optional field; no format version bump.

## Engagement events

`events[]` — optional top-level array of fire windows, provenance-gated
exactly like keyframes. Events drive the app's obscuration (smoke) and
acoustic layers; they are authored by hand against the research docs (the
authoring tool validates and passes them through; an event authoring UI is a
later phase).

| Field | Type | Rules |
|---|---|---|
| `events[].id` | string | required, unique |
| `events[].kind` | string | `artillery_fire` \| `musketry` |
| `events[].t0`, `.t1` | number | required; `t0 < t1 ≤ endTime` |
| `events[].unitId` | string | emitter form A; id of an existing unit |
| `events[].x`, `.z`, `.x2`, `.z2` | number | emitter form B; fixed segment, battlefield-local meters |
| `events[].confidence` | string | optional; `documented` \| `inferred` \| `unknown` (default `unknown`) |
| `events[].citation` | string | optional; REQUIRED non-empty when `confidence == "documented"` |
| `events[].note` | string | optional free text (e.g. witness-time spread, position provenance) |

- **Exactly one emitter form** (schema `oneOf`; re-checked in the tool's
  `validate.ts` and thrown in the Unity loader): `unitId` is a moving
  emitter — the rendered position is read from that unit's own track at each
  emission time; the `x,z,x2,z2` segment is a fixed line emitter for gun
  lines not authored as units (e.g. the Confederate artillery along Seminary
  Ridge; when batteries are authored as units, their events migrate to the
  `unitId` form).
- **Attach level:** an event attaches at whichever level the SOURCE attests.
  A battery fires as a battery; a regiment's flank fire is regiment-level; a
  volley attested only at brigade grain stays on the parent — the format
  never pretends finer attestation than the record gives. **Never author the
  same fire at two family levels**: a regiment event plus a parent event over
  the same window doubles the smoke and the sound. A validator cannot read
  attestation grain, so this is authoring discipline — review every event
  batch against it. Family semantics live in Parent / children above; the
  rendering side resolves an event's emitter position from THAT unit's own
  track at time t (`UnitTrack.StateAt` — every unit, parent or child, keeps
  its own full track) regardless of which family tier the LOD ladder
  currently draws: the LOD switches what RENDERS, never what the data IS,
  and events inherit that rule.
- **Dust is derived, not authored — there is no `advance_dust` kind.** Units
  moving faster than a threshold shed dust puffs (central-difference velocity
  over the track, Δ=30 s battle time, threshold 0.5 m/s); provenance inherits
  from the keyframes that produced the motion. **Family rule: dust derives
  only from units WITHOUT children** — a decomposed brigade's parent track
  still moves as the far-tier record of the same men, so deriving from both
  parent and children would double-dust every family. Childless units
  (regiment children, undecomposed brigades, batteries) are the dust
  emitters.
- Events reference the battle timeline, not the tracks: if a track spine is
  re-authored, the event windows that cohere with that movement must move
  with it.

## Environment

`environment` — optional top-level object carrying battlefield wind for the
obscuration layer.

| Field | Type | Rules |
|---|---|---|
| `environment.windTowardDeg` | number | required; compass bearing smoke drifts TOWARD |
| `environment.windMps` | number | required, ≥ 0; meters per second |
| `environment.confidence` | string | optional; `documented` \| `inferred` \| `unknown` (default `unknown`) |
| `environment.citation` | string | optional; REQUIRED non-empty when `confidence == "documented"` |
| `environment.note` | string | optional free text |

`windTowardDeg` is the direction of drift, named to dodge the meteorological
from-direction ambiguity (a "southwest wind" blows FROM the southwest; here
`windTowardDeg: 45` means smoke drifts toward the northeast). `windMps: 0`
means calm — no drift. The Unity loader's JSON layer deserializes an absent
`environment` block as a zeroed instance, so absent = calm = no drift is the
built-in fallback, which is exactly right.

## The no-faking gate

The authoring tool refuses to export a keyframe, event, or environment block
claiming `documented` confidence without a citation. Provenance will render in-app from Phase 4 (documented = solid,
inferred = ghosted, unknown = explicit "no reliable record"); today the
Unity loader parses but does not yet display it.

## Planned extensions (not yet valid)

- Per-keyframe `frontage_m`/`depth_m` (formation morphing)
- Path splines, comms/moments files
