# Smoke, Dust & Audio Implementation Plan (Battle Atlas, Phase 6)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
> **HARD RULE for every dispatch:** NEVER kill, quit, or terminate the Unity editor or any GUI process. If the Unity project is locked by an open editor, STOP and report BLOCKED. Unity CLI runs require exclusive access — coordinate through the controller.

**Goal:** Build Order item 5 — the environment starts carrying information: scrub to 13:07 and the Seminary Ridge gun line erupts in drifting smoke under a low rumble; the Union reply answers from Cemetery Ridge; dust trails Pickett's advancing brigades; musketry smoke and crackle mark the fence and the Angle at the climax. Obscuration and sound are functions of (battle data, t) — scrub backward and the smoke retreats exactly the way it came. (Spec: "Environment is information, not decoration"; obscuration/acoustic systems from engagement data.)

**Architecture:** Three legs. (1) **Format**: the battle file gains an optional `events[]` array (`artillery_fire` | `musketry` time windows, provenance-gated exactly like keyframes) and an optional `environment` block (wind). Tool passes them through with validation; authoring UI for events is a later phase — this phase authors them by hand against the research docs. (2) **Obscuration**: NO stateful particle simulation. Pure math (`ObscurationMath`) computes, for any t, the full set of live smoke/dust puffs from emission events: puff j of an event is emitted at `t0 + j·cadence` at the emitter's position at that moment (unit track or fixed gun-line segment), hash-jittered (FNV, never Random), advected linearly downwind with age, aged into 4 opacity buckets. A thin `ObscurationField` MonoBehaviour renders the buckets via `RenderMeshInstanced` of a procedural puff mesh into persistent buffers — zero per-frame allocation, deterministic in both scrub directions, same discipline as FormationLayout. Dust is **derived, not authored**: units moving faster than a threshold shed dust puffs; provenance inherits from the keyframes that produced the motion — nothing new to fake. (3) **Audio**: procedurally synthesized loops (pure, testable sample-fill functions — no licensing surface), played through a small pool of spatialized AudioSources whose positions and gains are recomputed every frame as functions of t and the active events. Loops never seek; only gain/position depend on t, so scrubbing is trivially safe.

**Format decisions locked here:**
- `events[]` (optional, top-level): `{ id, kind: "artillery_fire" | "musketry", t0, t1, unitId?, x?, z?, x2?, z2?, confidence?, citation?, note? }`. Exactly one emitter form (schema `oneOf`, belt-and-suspenders in validate.ts + loader): `unitId` = moving emitter, position read from that unit's track at emission time; the `x,z,x2,z2` segment = a fixed line emitter for gun lines not authored as units (the Confederate artillery along Seminary Ridge — when those batteries get authored later, events migrate to `unitId` form). Rules: unique ids; `t0 < t1 ≤ endTime`; `unitId` must reference an existing unit; documented ⇒ citation (the same no-faking gate, if/then in the schema).
- **No `advance_dust` event kind.** Dust derives from unit velocity (central difference over the track, Δ=30 s battle time, threshold 0.5 m/s) — zero authoring cost, automatically correct for all future authored movement, provenance inherited. The format doc says so explicitly.
- `environment` (optional, top-level): `{ windTowardDeg, windMps, confidence?, citation?, note? }`. `windTowardDeg` is the compass bearing smoke drifts TOWARD (named to dodge the meteorological from-direction ambiguity — document the convention). JsonUtility quirk: an absent object deserializes as a zeroed instance; `windMps == 0` means calm = no drift, which is exactly the right fallback — comment it.
- **Wind provenance:** the research corpus does NOT currently attest July 3 wind (checked: no wind/weather claims in the source-library or landcover docs). Author `windTowardDeg: 45, windMps: 2` (light SW breeze, hot day) as `inferred`, note: "unattested; likeliest attestation path is Michael Jacobs's meteorological register — he is already a verified, habitually precise source in the library (Notes on the Rebel Invasion, 1864) and kept regular thermometer/wind readings; a targeted research fetch would upgrade this to documented."

**Audio decisions locked here:**
- **Fully procedural synthesis, no samples:** `AudioSynth` generates three mono 22050 Hz ~8 s seamless loops in pure C# (deterministic FNV-seeded) — artillery rumble (low-passed noise with slow boom modulation), musketry crackle (sparse impulse train over band-passed noise), ambient bed (faint filtered hiss). Nothing to license; if PD/CC0 samples ever replace these, they must be recorded in a CREDITS file — noted in the format doc? No — noted here and in a code comment.
- **Pool of 8 AudioSources** created at runtime (no scene assets): 1 ambient 2D source always on at low gain; up to 7 spatialized 3D event sources. Per frame: active events at t are ranked by gain-at-camera; the top emitters get sources (artillery → rumble, musketry → crackle) positioned at the emitter (unit position at t, or segment midpoint — segment events may claim 2 sources at the third-points of long lines). `dopplerLevel = 0` (scrubbing at 60× would shriek). Distance tiers via per-kind rolloff: rumble audible to `maxDistance` 6000 m, crackle to 1400 m — the spec's theater-rumble vs tactical-crackle filtering for free.
- **Gain is a pure function of t:** `AcousticMath.EventGain(t0, t1, t)` — trapezoid with 3 s attack from t0 and 5 s release after t1, computed from t (not from playback state), so reverse scrubbing produces the identical soundscape. One honest exception: a ≤150 ms gain slew on source reassignment purely to avoid clicks — perceptual smoothing, not state; commented as such.
- **Paused clock keeps its soundscape** (rumble holds while paused mid-bombardment): audio reflects the state at t exactly like the visible smoke does — consistency of the "functions of t" rule. A mute toggle is one line later if it grates.

**Obscuration parameters (starting values, tuned in Task 7):** gun smoke — cadence 4 s, life 90 s, grows 4→14 m, grey-white; segment emitters emit `max(1, round(length/150m))` puffs per tick spread hash-jittered along the line; musketry smoke — cadence 3 s, life 30 s, 2→6 m, white, jittered along the unit frontage; dust — cadence 6 s, life 45 s, 2→8 m, tan, ground-hugging, trailing the unit. Worst case (bombardment peak: ~1,800 m Confederate line + 5 Union batteries + musketry + dust) ≈ 800–1,000 live puffs — persistent per-bucket `Matrix4x4[384]` buffers (4 buckets × 2 materials = ≤8 draw calls total); overflow clamps deterministically oldest-first with a one-time loud warning, never silently forever.

**Branch:** `smoke-audio`. Baselines: Unity 60 EditMode, tool 74, pipeline 29 (no pipeline changes in this phase). Unity CLI verification requires the editor closed — pure-math tasks verify via CLI; scene wiring and visual/acoustic verification ride the Task 7 editor session.

---

### Task 1: Format — events + environment (schema, tool passthrough, Unity loader) (TDD)

**Files:** `docs/format/battle-format.md`, `docs/format/battle.schema.json`, `tool/src/model.ts`, `tool/src/validate.ts`, `tool/src/io.ts`, `tool/tests/validate.test.ts` (or a new `events.test.ts`), `app/Assets/Scripts/BattleData.cs`, `app/Assets/Tests/Editor/BattleLoaderTests.cs`

- [ ] Format doc: new "Engagement events" and "Environment" sections per the locked decisions above (field tables, emitter exclusivity, the derived-dust note, wind convention + calm fallback, documented⇒citation); move `engagement` out of "Planned extensions".
- [ ] Schema: optional `events` array (unique-id rule stays code-side) — required `id/kind/t0/t1`; kind enum; emitter `oneOf` (`unitId` XOR all four segment numbers); documented⇒citation if/then mirroring keyframes. Optional `environment` object (`windTowardDeg`, `windMps` ≥ 0, provenance fields).
- [ ] `tool/src/model.ts`: `EngagementEvent`, `Environment` types on `Battle` (optional). `tool/src/validate.ts` code rules: unique event ids; `t0 < t1 ≤ endTime`; `unitId` exists; documented⇒citation. `tool/src/io.ts`: canonical export order (events sorted by `t0`, then id), validation-gated — passthrough only, no UI (event authoring UI is a later phase; note it in the format doc).
- [ ] Tool tests (~6): valid events fixture accepts; duplicate event id rejects; `t0 ≥ t1` rejects; unknown `unitId` rejects; documented-without-citation rejects; both-emitter-forms rejects. `cd tool && npm test` → **74 + 6 = 80** + typecheck.
- [ ] Unity: `EventDto` / `EnvironmentDto` in `BattleData.cs` (flat fields — JsonUtility can't nest arrays; emitter form keyed on non-empty `unitId`); `BattleLoader.Parse` re-validates the load-bearing rules (t0<t1≤endTime, unitId resolves, kind vocabulary) — throw, same posture as existing loader checks.
- [ ] Loader tests (+4): events parse from a valid fixture; bad unitId throws; t0≥t1 throws; unknown kind throws. CLI verify (BLOCKED if locked): **60 + 4 = 64**. Commit.

### Task 2: Author the slice's engagement events + wind (data)

**Files:** `app/Assets/Battle/gettysburg-july3.json`, `tool/tests/gettysburg.test.ts`

- [ ] **Read the authored tracks first and stay on their spine.** The keyframes already commit to a modern-reconstruction timeline (step-off ~15:05, Emmitsburg Road ~15:16, the wall ~15:23, repulse 15:30–15:40, citing ABT maps/Haskell); event windows must cohere with the movement they explain. Where witnesses disagree (step-off attested anywhere 13:50–15:00 across Alexander/Jacobs/Haskell — see docs/research/2026-06-13-july3-source-library.md §3), geometry follows the track spine and the citation carries the disagreement — never a reconciled "true" time.
- [ ] Confederate bombardment: one `artillery_fire` segment event along the Seminary Ridge gun line (endpoints read off the draped Bachelder sheet 3 in the tool; position provenance `inferred` in `note` — the fact and window are documented, the traced line is a reconstruction). Window t0=420 (13:07 Jacobs 1864, contemporaneous and habitually precise; note carries Alexander's "just 1 P.M." and Haskell's ~13:00 — a 0–7 min spread) → t1≈7200 (cannonade ends as the infantry steps off on the track spine; Haskell "two mortal hours" / ends "at three o'clock almost precisely"). `documented` + citations.
- [ ] Union counter-battery: `artillery_fire` per authored battery unit (Cushing, Rorty, Arnold, Cowan, Woodruff), t0≈600–900 (reply opens minutes after the signal — Waitt 1906: "then every rebel gun... opened," reply from the ridge; Hunt's conservation order caps the reply at ~80 guns — cite), tapering per Hunt's deliberate slackening + genuine withdrawals near the copse ("not purely a ruse: encode as mixed" — pass 1). Second window for the canister climax at the wall, ~15:20–15:30, Cushing's documented ("We continued to fire double and treble charges of our canister" — Fuger via NPS/Hartwig, JMSI 41 p. 408), the rest `inferred`.
- [ ] Musketry: unit events for the climax — Union brigades (Webb/Hall/Harrow/Smyth/Sherrill from ~15:16 as the column crosses the road; Stannard's flank fire ~15:20+, documented via Sturtevant 1910 pp. 304–305), Confederate return fire windows on the same spine. Pass 1 flags the back half of the window time-poor — tag `inferred` where no witness time exists, and say so in notes.
- [ ] Wind: the `environment` block per the locked decision (`inferred`, Jacobs-register note).
- [ ] `tool/tests/gettysburg.test.ts` +1: the battle file validates, has ≥1 event of each kind, and every `documented` event carries a citation. `cd tool && npm test` → **81**. Commit.

### Task 3: ObscurationMath (TDD, Unity CLI)

**Files:** `app/Assets/Scripts/ObscurationMath.cs`, `app/Assets/Tests/Editor/ObscurationMathTests.cs`

- [ ] Pure static core: `int LivePuffs(EventDto e, System.Func<float, Vector2> emitterPosAt, Vector2 windMps, float t, Puff[] buffer)` — `Puff { Vector2 posXZ; float age01; float radius; }`. Emission times `t0 + j·cadence` while `< t1`; live while `t − tEmit ∈ [0, life)`; position = emit position + FNV jitter (eventId, j, salt — reuse the FormationLayout hash pattern) + wind·age; radius/bucket from `age01`. Per-kind params in a small struct (`PuffParams.For(kind)`); segment emitters spread `round(len/150)` puffs per tick along the line by hash. Plus `DustSpeedAt` lives here or Task 4 — keep emission math together: `LivePuffs` also serves dust with dust params and the unit-track position function.
- [ ] `AgeBucket(float age01)` → 0..3 (opacity buckets, render-side contract).
- [ ] Tests (~7): no puffs before t0 or after t1+life; puff drifts linearly downwind with age (two ts, exact delta); deterministic — same t twice AND after querying a different t returns identical puffs (the reverse-scrub guarantee); different event ids jitter differently; segment event spreads puffs along the segment (all within the segment's jitter envelope, spread > half its length); buffer cap clamps oldest-first; bucket mapping monotonic with age.
- [ ] CLI verify (BLOCKED if locked): **64 + 7 = 71**. Commit.

### Task 4: ObscurationField rendering + derived dust (Unity CLI)

**Files:** `app/Assets/Scripts/ObscurationField.cs`, `app/Assets/Scripts/InstancedMeshes.cs` (+`BuildPuff`), `app/Assets/Scripts/DustMath.cs`, `app/Assets/Scripts/BattleDirector.cs` (wiring), `app/Assets/Editor/ObscurationSetup.cs`, tests

- [ ] `InstancedMeshes.BuildPuff()`: irregular low-poly blob — three overlapping offset tapered boxes, ≤120 verts (the Commander's Table aesthetic; no billboarding, no custom shader). 1 mesh test.
- [ ] `DustMath.SpeedAt(UnitTrack, t)` — central difference Δ=30 s, clamped at track ends; pure. 2 tests (moving segment ≈ known speed; static unit = 0).
- [ ] `ObscurationField` (MonoBehaviour, created/driven by BattleDirector like the formation renderers): holds events + tracks + environment + clock reference; per frame fills 4 per-bucket persistent matrix buffers (smoke) + 4 (dust) from `ObscurationMath.LivePuffs` across all events, plus derived dust for every unit with `SpeedAt > 0.5 m/s`; one `RenderMeshInstanced` per non-empty bucket with a per-bucket-alpha `MaterialPropertyBlock`. Zero per-frame allocations (persistent buffers, cached delegates — the groundYFunc pattern). Smoke puffs float at emitter ground + ~4 m rising slightly with age; dust hugs ground.
- [ ] Materials: committed transparent URP assets `Smoke.mat` / `Dust.mat` (asset references keep shaders in device builds — the Phase 2 lesson). `ObscurationSetup.cs` menu item "BattleAtlas/Setup Obscuration" creates the materials if absent and wires them onto BattleDirector fields (idempotent, like Add Vegetation). Runtime: if materials unset, log a loud warning once and render nothing — never a silent or magenta fake.
- [ ] BattleDirector: parse events/environment via the loader, AddComponent + hand over refs in `Start`.
- [ ] CLI verify: **71 + 3 = 74** (compile check covers the MonoBehaviour). Commit.

### Task 5: AudioSynth (TDD, Unity CLI)

**Files:** `app/Assets/Scripts/AudioSynth.cs`, `app/Assets/Tests/Editor/AudioSynthTests.cs`

- [ ] Pure static sample-fill: `FillRumble(float[] s)`, `FillCrackle(float[] s)`, `FillAmbient(float[] s)` — FNV-seeded noise shaped per the locked decisions; loops generated with periodic phase so the seam is inaudible. A `CreateClip(name, fill)` helper wraps `AudioClip.Create` (runtime-only path, not under test).
- [ ] Tests (~5): deterministic (two fills identical); all samples in [−1, 1]; RMS within a sane band (0.05–0.5) for each; rumble has fewer zero-crossings than crackle (cheap spectral proxy for "low rumble vs crackle"); loop seam continuity (|s[0] − s[last]| small).
- [ ] CLI verify: **74 + 5 = 79**. Commit.

### Task 6: AcousticField (Unity CLI)

**Files:** `app/Assets/Scripts/AcousticField.cs`, `app/Assets/Scripts/AcousticMath.cs`, `app/Assets/Tests/Editor/AcousticMathTests.cs`, `app/Assets/Scripts/BattleDirector.cs` (wiring)

- [ ] `AcousticMath.EventGain(t0, t1, t)` — trapezoid (3 s attack, 5 s release), pure. 2 tests: full gain mid-window + zero outside; ramp values at edge offsets exact.
- [ ] `AcousticField` (MonoBehaviour, created by BattleDirector like ObscurationField): builds the three clips in `Start` via AudioSynth; pool of 8 AudioSources (1 ambient 2D, 7 spatial 3D, `dopplerLevel = 0`, per-kind `maxDistance` 6000/1400, logarithmic rolloff); per frame ranks active events by `EventGain × ` distance attenuation at the camera, assigns the top emitters to sources (stable order — sort by gain then event id to prevent thrash), positions unit emitters from their track at t, applies the ≤150 ms anti-click slew (commented: perceptual, not state). Audio reflects t while paused, by design.
- [ ] Confirm the scene camera carries the AudioListener (Unity default — verify, don't assume, in Task 7).
- [ ] CLI verify: **79 + 2 = 81**. Commit.

### Task 7: Editor + device verification, tag

- [ ] Editor session (user or MCP; the ONLY task that needs the editor): run "BattleAtlas/Setup Obscuration", save scene. Then the scrub script: 13:05 — silence, clear air; 13:07 (t=420) — the Seminary Ridge line blooms smoke, rumble rises; 13:20 — Union ridge answers, both gun lines smoking, drift toward NE visible; 15:10 — dust trails the advancing brigades between the ridges; 15:20–15:30 — musketry smoke and crackle at the fence and the Angle, canister smoke at Cushing's position; **scrub BACKWARD from 15:30 to 13:05 — the field cleans itself in perfect reverse; jump-cut scrubs land on identical states.** Screenshots at each beat; listen at theater height (rumble only) vs 300 m (crackle joins) for the distance tiers.
- [ ] Tune the parameter block (cadence/life/sizes/gains) by eye and ear; keep the values in the two params structs, one commit.
- [ ] iOS build; device checklist: bombardment peak (13:30, both gun lines + full smoke) and the Angle at 15:25 (soldiers + trees + fences + smoke + audio — the new worst case) hold frame rate; transparent overdraw acceptable; thermals after 5 min; audio doesn't stutter on scrub. Tag `phase6-smoke-audio`.

## Done =

Scrub to 13:07 and the war stops being silent geometry: the Confederate gun line erupts along Seminary Ridge in a two-mile ribbon of powder smoke drifting northeast, the Union ridge thunders back, and when Pickett's brigades step off, dust betrays the columns and musketry crackles exactly where — and exactly when — the record puts the fighting. Drag the scrubber backward and every puff retraces its drift. Smoke shows where the battle burns; sound locates it beyond the ridgelines; none of it is faked — every emission window can answer "says who."

## Risks

- **Transparent overdraw on device** is the real perf threat (not instance count — ~1k blobs is nothing, but stacked alpha layers at tactical zoom are). Mitigations: 4 discrete opacity buckets (≤8 draw calls), Task 7 measures the bombardment peak on-device first; fallbacks are smaller puffs, shorter life, or opaque-dithered material — all parameter/material-local.
- **Witness time disagreement:** the step-off is attested anywhere 13:50–15:00; events follow the already-authored track spine (ABT reconstruction) so smoke never contradicts movement, and citations carry the spread. If the tracks' spine is ever re-authored, events must move with it — they reference the same timeline, not each other (noted in the format doc).
- **Procedural audio taste:** synthesized rumble may read as synthetic. All character lives in three pure fill functions — swappable for PD/CC0 samples later (CREDITS entry required) without touching AcousticField.
- **Wind is unattested** — carried as `inferred` with an explicit attestation path (Jacobs's meteorological register); at `windMps: 0` the system degrades to no drift, honestly.
- **Source-pool thrash** at event boundaries could pop; stable ranking + the slew should cover it — if not, hysteresis on source assignment mirrors the LOD-latch pattern.
