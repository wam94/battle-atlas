# Battle Manifest Format

The day/phase navigation contract between authored data and the Unity
Atlas (ADR 0005). JSON, UTF-8. Machine-checkable schema:
[battle-manifest.schema.json](battle-manifest.schema.json). The Unity
side re-validates the load-bearing rules at parse
(`PhaseManifest.FromJson`); the tool side validates in
`validateManifest` (tool/src/validate.ts).

The canonical instance lives at
`app/Assets/StreamingAssets/Atlas/battle-manifest.json` (the
runtime-readable channel, the `moments.json` precedent).

## Model

A battle spans **days**; a day contains **phases**; a phase is either
**reconstructed** (it IS a battle file — see
[battle-format.md](battle-format.md); one battle file = one phase =
one clock) or **not-reconstructed** (structure only, with an honest
note). The determinism contract is per-phase: everything rendered is
a pure function of (phase, one battle clock).

## Structure

| Field | Type | Rules |
|---|---|---|
| `name` | string | required |
| `days[]` | array | ≥ 1; `date` strictly increasing (code rule) |
| `days[].id` | string | required, unique (code rule) |
| `days[].label` | string | required (the day tab's word) |
| `days[].date` | string | required, `YYYY-MM-DD` |
| `days[].phases[]` | array | ≥ 1; `id` unique across the whole manifest (code rule) |
| `phases[].id` | string | required |
| `phases[].label` | string | required |
| `phases[].status` | string | `reconstructed` \| `not-reconstructed` |
| `phases[].battle` | string | REQUIRED iff reconstructed; battle-file name under `app/Assets/Battle/` |
| `phases[].startTime` | number | REQUIRED iff reconstructed; MUST equal the battle file's `startTime` (cross-file rule, test-enforced) |
| `phases[].endTime` | number | REQUIRED iff reconstructed; MUST equal the battle file's `endTime` (cross-file rule, test-enforced) |
| `phases[].note` | string | REQUIRED non-empty iff not-reconstructed (the honest empty state's displayed text); optional otherwise |

## The honesty rules

- **An empty day is inexpressible without saying so.** A
  `not-reconstructed` phase must carry a note; the Atlas shows that
  note verbatim as the day's state. No battle reference is allowed on
  it — an unreconstructed phase can never smuggle in content.
- **A phase cannot lie about its clock.** `startTime`/`endTime` on a
  reconstructed phase are an ECHO of the battle file's own values,
  and the echo is enforced (tool test + Unity cross-check against the
  loaded battle, loud warning on mismatch). The manifest adds
  navigation, never a second source of time.
- **Reconstructed phases within a day may not overlap** in
  `[startTime, startTime + endTime)` (code rule).

## Runtime consumption

`AtlasHud` loads the manifest (missing/rejected file = no day
navigation, warned, everything else keeps working — the moments.json
degradation pattern). Day tabs render in date order; the day owning
the phase whose `battle` matches the loaded battle asset is marked
active; selecting an unreconstructed day/phase opens the honest
empty-state panel with the phase's note. Battle hot-swapping between
multiple reconstructed phases is deferred until a second
reconstructed phase exists (ADR 0005 §Consequences).
