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
empty-state panel with the phase's note. Day-expansion slice 2 ships
the capture-path `-battleFile` override (BattleDirector loads a battle
JSON from disk; `BattleAssetName` follows it, so the day panel lights
the loaded phase) and per-phase timeline moments: the HUD first probes
`Atlas/moments-<battleAsset>.json`, falls back to `Atlas/moments.json`,
and drops any moments file whose own `battle` field names a different
phase — a moments file may never render against another phase's
clock.

**In-HUD phase switching** (the hot-swap ADR 0005 deferred, shipped by
the phase-switching slice): a reconstructed phase that is not the
loaded one carries a "Load this phase" button in the day panel
(`AtlasHud.SwitchToPhase`). A switch destroys the running battle's
components and spawns a fresh `BattleDirector` with the target phase's
battle text (`BattleDirector.SpawnSuccessor` — fresh-launch
equivalence by construction, PlayMode-pinned), re-bases the clock to
the target phase (a never-visited phase starts at its t=0; a revisited
one resumes at its session-remembered position), reloads the per-phase
moments, re-lights the day tabs, and **re-runs the manifest clock-echo
verification against the newly loaded battle** — a stale echo warns
loudly on every switch, not just at launch. The phase battle file
resolves from, in order: a `-battleDir <dir>` argument,
`StreamingAssets/Battle/` (standalone builds carry post-build copies),
`Assets/Battle/` (the editor and PlayMode tests), then the
`-battleFile` directory. Soldier View entry is per-phase honest: the
shipped viewpoints address the scene's serialized battle asset (July 3
afternoon), and entry markers/window band exist only while that phase
is loaded (`HudModel.ViewpointsApplyTo`). Not-reconstructed phases
remain notes — never a load control.
