# Battle Atlas — Design Spec

**Date:** 2026-06-11
**Status:** Approved design, pre-implementation
**One-liner:** A Google Maps for historical battles — an iPhone demo that lets you scrub through all three days of Gettysburg on real 3D terrain, watching every regiment move, with smoke, sound, couriers, and hand-crafted first-person moments. Fully on rails: a 3D movie you can move through.

## Goals & Non-Goals

**Goal:** The highest-quality demo possible of one battle (Gettysburg, July 1–3, 1863), built as a personal art project and a proof of whether the concept deserves productizing.

**Non-goals (v1):** App Store distribution, multiple battles, accessibility breadth, monetization, multiplayer/social, any network backend. The app is fully offline — the entire battle ships on disk.

## Decisions Log

| Decision | Choice |
|---|---|
| Ambition | Excellent single-battle demo / art project |
| Battle | Gettysburg, all 3 days |
| Visual style | "Commander's Table" (C): real LiDAR 3D terrain, naturalistic but lightly stylized ground cover, units as crisp readable formation blocks that resolve into instanced soldier ranks up close. Upgradeable toward realism later without rearchitecting. |
| Unit granularity | Regiment level (~650 units), via brigade-backbone derivation (see Data Model) |
| First-person | 3–6 hand-crafted scripted vignettes (not free-roam) |
| Narrative | Annotated timeline: free scrubbing + key-moment pins that fly the camera and caption the action |
| Engine | Unity (iOS export). Terrain tooling, GPU instancing, Cinemachine cameras. |
| Main screen layout | "The Window" (B): full-bleed battlefield, floating translucent UI, thin expandable scrubber |

## Core Principles

1. **On rails, not simulated.** The battle is data. The Simulation Core is a clock: given time T, interpolate every unit's state from baked keyframe tracks. Nothing is computed emergently; scrubbing is deterministic, instant, and bidirectional.
2. **Don't fake anything.** Every data element carries a provenance tag: `documented` / `inferred` / `unknown`. The UI renders uncertainty honestly (ghosted units, "reconstructed" notes, explicit "no reliable record"). Vignette content comes only from documented first-person accounts, quoted with attribution. The authoring tool refuses to export keyframes without a source citation.
3. **Environment is information, not decoration.** Smoke shows where the battle burns. Dust betrays marching columns. Sound locates fighting beyond ridgelines. Terrain difficulty explains why charges failed. Weather and time-of-day are part of the record.
4. **Two projects in one.** An offline desktop pipeline (where the hard slow work happens once) and the iPhone app (which only ever reads baked output). They share only the bundle format.

## Architecture

### Offline pipeline (desktop, build-time)

- **Battle Authoring Tool** — a web map app built for this project. Loads georeferenced scans of historical maps as overlays (Bachelder hour-by-hour maps, American Battlefield Trust map sets, NPS troop-movement maps). Author traces unit path splines and drops time keyframes; edits formation state, strength, engagement status; tags provenance + citation per keyframe cluster. Implements brigade-backbone editing with per-regiment overrides.
- **Terrain Builder** — scripts converting USGS 1 m LiDAR into tiled, LOD'd terrain meshes; 1863 land cover (period fields, woodlots, orchards) painted as splat maps from historical research; roads, fences, stone walls, and buildings placed as instanced props.
- **Validation suite** — runs at bake time; bad data fails the build: no teleports, march-rate speed limits by arm (infantry/cavalry/artillery), strength monotonically non-increasing through engagements, citation present on every authored cluster.

### Baked Battle Bundle (ships inside the app)

1. `battle.json` — order of battle: full hierarchy (Army → Corps → Division → Brigade → Regiment/Battery), per-unit identity, strengths, commanders w/ bios, time-keyed lore snippets.
2. `timeline.bin` — binary keyframe tracks per unit: time, position, facing, formation state (column / line / skirmish / scattered / routed), frontage/depth, strength, engagement status. Path splines for road-following interpolation.
3. Terrain tiles — meshes + LOD chain, splat maps, prop placements.
4. `moments.json` — annotated-timeline pins: time range, title, caption, camera instruction; vignette refs for the gold pins.
5. `comms.json` — message entities: sender, recipient, method (courier / signal flag / verbal / bugle), dispatch & arrival times, sourced content, link state over time (clear / delayed / broken / unknown). Courier route splines on the road network.
6. Environment track — time-of-day lighting curve, weather (July heat haze), wind direction over time; baked obscuration events.
7. Ambience — audio beds and event audio banks.

### iPhone app (Unity, runtime)

- **Simulation Core** — the clock. Pure interpolation over tracks; regiments without explicit tracks derive position from brigade track + formation slot.
- **Battlefield Renderer** — terrain tiles + instanced props; GPU-instanced unit rendering across the zoom ladder; smoke/dust particle systems driven by baked engagement events and wind.
- **Camera Director** — Cinemachine: free orbit/pan/pinch, tap-to-follow, fly-to-moment, scripted vignette rigs.
- **Timeline UI** — thin translucent scrubber with moment pins (gold = vignette); pull up to expand into filmstrip (day tabs, moment cards). Time-of-day lighting synced to clock.
- **Info & Layers UI** — floating unit cards (strength, commander, story-so-far, provenance badge); layer chips: elevation shading, 1863 vegetation, mobility/difficulty, comms, smoke, unit paths; conditions readout (temperature, wind).
- **Vignette Player** — scripted first-person hero moments; locally densified detail bubble; full audio mix; attributed captions; exit returns to map at the same battle-time.

## Data Model Details

**Unit keyframe:** `{t, position, facing, formation, frontage, depth, strength, engagement, provenance, citation}`. Interpolation follows authored splines (columns march along roads, not through barns). Formation morphs animate between states; `scattered`/`routed` covered as first-class formation states.

**Brigade-backbone derivation:** brigades are the authored unit of record; regiments inherit position from their slot in the brigade's current formation layout. Hand-authored regiment tracks override derivation where history records independent movement (20th Maine, 1st Minnesota, etc.). This turns ~650 units × 3 days into ~120 authored tracks + targeted overrides.

**Obscuration system:** engagement data auto-generates emitters — battery fire → gun smoke plumes; massed musketry → drifting white smoke lines along the firing line; columns on dry roads → dust. Wind (from the environment track) advects it. Deterministic: scrub to any T and the smoke is exactly where the fighting put it.

**Acoustic model:** the same engagement data drives spatialized audio sources. Distance filtering by altitude/range: theater = directional low rumble; operational = distinct artillery + musketry crackle on correct bearings; tactical/eye = full mix. Sound is directional information at every zoom.

**Comms layer:** messages render as moving courier dots on real road splines, signal stations with sightlines, and command links with state coloring. Tap a delivered message for its text and journey. Only documented traffic is included.

## UX

### Main screen ("The Window")

Full-bleed battlefield. Floating translucent chips (layers, comms, conditions + clock). Thin bottom scrubber with moment pins; pull up for the filmstrip (day tabs July 1/2/3, tappable moment cards). Unit cards float adjacent to their unit.

### The Zoom Ladder

One continuous pinch zoom, four readable altitudes; detail streams in a bubble around the camera:

1. **Theater (~8 km)** — brigade chips colored by corps, front lines, smoke columns/dust plumes as strategic info, courier dots.
2. **Operational (~1.5 km)** — regiments as formation blocks with flags and facing; fences, woodlots, batteries; tap-to-follow lives here.
3. **Tactical (~250 m)** — blocks dissolve into ranks of instanced stylized soldiers (~2–4k figures near camera); muzzle smoke; main perf-engineering zone.
4. **Eye level (~2 m)** — vignettes and the bottom of follow-cam; full local detail and audio.

**Follow mode:** tap unit → follow; camera anchors to the unit through time scrubbing; card pinned with live strength and story-so-far. Follow + pinch down = ride along at eye level.

**Vignettes (v1 candidates):** dawn at Culp's Hill; the cannonade from the Union gun line; marching in Pickett's Charge; Warren on Little Round Top as the signal station works. 60–120 s each, scripted Cinemachine paths, captions verbatim from sourced accounts.

## Build Order (vertical slice first)

Target slice: **July 3, 1:00–4:00 PM** (bombardment → Pickett's Charge), excellent end-to-end, then widen.

1. **Terrain proof** — LiDAR → Unity → fly around Little Round Top on a real iPhone. De-risks the visual foundation immediately.
2. **Simulation core + scrubber** — placeholder units, interpolation math, on-device.
3. **Authoring tool MVP** — georeferenced overlays, path tracing, keyframes, provenance; author the slice.
4. **Zoom ladder** — LOD transitions, instanced soldier rendering, perf budget on-device.
5. **Smoke, dust, audio** — obscuration + acoustic systems from engagement data.
6. **Comms layer** — couriers, signal stations, link states for the slice period.
7. **First vignette** — marching in Pickett's Charge.
8. **Widen** — author remaining days (brigade resolution first, regiment overrides as research allows); can proceed in parallel with polish indefinitely.
9. **Polish** — lighting, transitions, sound mix, app icon, the love.

Every phase boundary leaves something running on a phone worth showing someone.

## Risks (ranked)

1. **Data authoring scale.** Hundreds of hours of historical work at full fidelity. Mitigations: brigade-backbone derivation, vertical slice first, authoring-tool investment, Days 1–2 may stay brigade-resolution longer — demo is real regardless.
2. **iPhone perf at tactical zoom.** Thousands of animated instances. Mitigations: GPU instancing, camera detail bubble, on-device testing from phase 1.
3. **Historical gaps.** Some movements are genuinely unrecorded. Mitigation: the provenance system makes gaps honest content rather than embarrassments.
4. **Scope gravity.** Comms, weather, and audio are each bottomless. Mitigation: each is a layer that can ship shallow and deepen later; explicit v1 depth limits below.

### V1 depth limits for the bottomless layers

- **Comms v1:** full documented message traffic for the vertical slice window; outside it, only the ~20 most famous messages of the battle (Warren's plea, Lee's "if practicable," etc.). Courier animation and link states at corps/division level only.
- **Weather v1:** time-of-day lighting curve, July heat haze, wind direction track (drives smoke). No precipitation or dynamic weather systems — none occurred during the battle's three days.
- **Audio v1:** the three-tier distance mix with positional sources per engagement cluster (not per regiment). Voices, shouts, and bugle calls appear only inside vignettes, where they're hand-placed.
- **Smoke/dust v1:** particle budget tuned for the slice; outside the slice, theater-level smoke columns may be billboard impostors rather than full particle systems.

## Testing

Proportionate to a demo: unit tests on the simulation core (interpolation, formation derivation — math that must never lie); the pipeline validation suite (teleports, speed limits, strength monotonicity, citation presence); frame-rate budget checked on-device at each phase boundary. No test theater beyond that.

## Out of Scope for v1

Other battles; Android/iPad layouts; free-roam first person; emergent simulation of any kind; counterfactuals ("what if Stuart arrived earlier") — the record only; localization; backend services of any kind.
