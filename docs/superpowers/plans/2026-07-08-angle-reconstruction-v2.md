# Battle Atlas — The Angle Reconstruction V2 Execution Plan

**Date:** 2026-07-08  
**Status:** Approved direction; implementation-ready  
**Target:** Desktop playback on Apple Silicon Mac  
**Audience:** Civil War and military-history enthusiasts  
**Historical stance:** One editorially chosen reconstruction, with disagreements and inference still disclosed  
**Detailed slice:** Pickett's Charge at the Emmitsburg Road and the Angle, approximately 15:14–15:30 on the project's canonical clock (`t=8040..9000`)  
**Visual target:** Credible museum-grade historical realism, not AAA photorealism  

> **Executor rule:** This plan intentionally changes the visual and data architecture. Preserve the current working implementation and its tests until each replacement passes the stated gate. Do not widen the battle, add more units, or author Days 1–2 while this plan is in progress.

## 1. Outcome

Deliver a desktop-first interactive reconstruction with two connected experiences:

1. **Atlas Mode** remains a real-time, scrubbable overview of the existing July 3 battlefield. It explains unit identity, movement, activity, terrain, sources, and uncertainty using documentary cartography rather than attempting literal realism at kilometer scale.
2. **Soldier View** enters a fixed, authored first- or close-third-person viewpoint inside the Angle slice. Nearby soldiers march, fire, reload, react, fall, and leave persistent bodies. The viewpoint is pre-rendered offline as seekable video and remains synchronized with the battle clock.

The user does not walk, steer, or rotate the Soldier View camera. The user may enter, exit, play, pause, and seek. This restriction is what makes substantially higher visual fidelity feasible on Apple Silicon while retaining a deterministic reconstruction.

## 2. Locked decisions

These are requirements, not open design questions:

- Keep Unity as the playback, staging, and final-render engine.
- Target Apple Silicon macOS first. The playback application must work on a base M1-class Mac; offline rendering may take longer on lower-end hardware.
- Defer iPhone support. Do not preserve mobile rendering constraints at the expense of the desktop visual target.
- Preserve the current 190-unit July 3 reconstruction as Atlas Mode context.
- Stop full-cast expansion until the Angle slice passes all final gates.
- Use true-scale terrain at `1.0×` vertical scale in all new work.
- Use a single canonical reconstruction. Do not build scenario switching.
- Show the basis and uncertainty of the canonical choice where sources disagree.
- Soldier View is a fixed camera-level experience, not locomotion or gameplay.
- Soldier View contains explicit falls, blood, wounds, and persistent bodies.
- Do not add dismemberment or invented named-person wound details unless separately approved and specifically sourced.
- The repository is open source. Every required asset must be redistributable under CC0, CC-BY, or a compatible project-owned license.
- Do not make Unity Asset Store EULA assets, Mixamo source files, or other non-redistributable content required for a clean build.
- Generated final video is a release artifact, not a normal Git object.
- Historical realism and intelligibility outrank spectacle.

## 3. The pre-render contract and its sacrifices

### 3.1 Hybrid architecture

The final camera image is not pre-rendered everywhere:

- Atlas Mode renders interactively in Unity.
- Simulation tracks, tactical actions, animation variants, terrain masks, lighting data, and effects seeds are baked before runtime.
- Soldier View is rendered offline from Unity at a fixed timestep to a high-quality image sequence, then encoded as seekable video.
- Playback maps `BattleClock.CurrentTime` to a video timestamp.
- Fixed cinematic moments may use the same offline-render pipeline.

### 3.2 Apple Silicon limitation

Unity HDRP supports macOS/Metal raster rendering, but Unity HDRP path tracing is not available on Metal. The final visual target must therefore use:

- HDRP raster rendering;
- physically based materials;
- baked or probe-based indirect lighting;
- direct sunlight and contact shadows;
- screen-space ambient occlusion and reflections where appropriate;
- volumetric fog and smoke;
- temporal anti-aliasing;
- offline accumulation for motion blur and anti-aliasing when supported by the Recorder configuration;
- high render settings unconstrained by real-time frame rate.

Do not make DX12 ray tracing or path tracing a dependency. Blender Cycles/Metal may be used to bake textures, normals, ambient occlusion, or asset turntables, but not as a second authoritative scene renderer in v1.

### 3.3 User-facing sacrifices

Document these in the product and do not accidentally promise beyond them:

- A user cannot enter any arbitrary soldier or arbitrary point on the battlefield.
- Soldier View exists only where an authored `ViewpointDefinition` and rendered media file exist.
- The camera cannot turn or move once inside a pre-rendered viewpoint.
- Atlas-to-Soldier transitions are authored cuts or short blends, not a physically continuous free camera.
- Every additional viewpoint increases rendering time and release size approximately linearly.
- Seeking compressed video is not frame-instantaneous. A low-resolution proxy and short seek-settle transition hide decoder latency.
- The final image can look convincingly realistic, but free/open assets and a small production pipeline will not match a bespoke AAA character production.

### 3.4 Initial viewpoint scope

Ship one required hero viewpoint and treat two others as post-gate extensions:

1. **Required — `garnett-road-to-angle`**: a Confederate infantry viewpoint moving with a deterministic formation slot from the Emmitsburg Road toward the Angle, approximately `t=8160..8820` (15:16–15:27). First-person or very tight over-shoulder, selected during the visual-target gate.
2. **Extension — `webb-wall`**: Union infantry behind the stone wall during the final approach and breach, approximately `t=8520..9000`.
3. **Extension — `cushing-canister`**: a fixed crew-level view at Cushing's battery, approximately `t=8400..8760`.

Do not begin extension viewpoints until the required viewpoint is integrated, source-visible, and accepted.

### 3.5 Render-time, storage, and quality budget

The required 11-minute viewpoint at 30 fps is **19,800 final frames**. Plan capacity before approving the full render:

- At 5 seconds per output frame: approximately 27.5 render-hours.
- At 30 seconds per output frame: approximately 6.9 render-days.
- At 60 seconds per output frame: approximately 13.8 render-days.
- 1440p PNG scratch at roughly 8–20 MB/frame: approximately 160–400 GB.
- Half-float EXR scratch at roughly 20–50 MB/frame: approximately 400 GB–1 TB.
- H.264 delivery at 20–40 Mbit/s: approximately 1.7–3.3 GB before audio/proxy overhead.

These are planning ranges, not promises. Phase 10 must replace them with measurements from the target Mac and selected scene.

If the full render exceeds seven days or 500 GB of scratch on available hardware, reduce cost in this order:

1. shorten the required viewpoint while preserving the road crossing and wall approach;
2. reduce accumulation/subframe samples;
3. lower output from 1440p to 1080p;
4. reduce hero/near crowd material and rig complexity;
5. reduce distant crowd density only after confirming smoke and formation silhouette remain credible.

Do not regain performance by restoring terrain exaggeration, removing historically important smoke, changing battlefield timing, or sanitizing casualty accumulation. Playback on a base M1-class Mac and offline rendering are different budgets: an 8 GB machine should play the application and encoded media, while production rendering may require a 16 GB-or-better Apple Silicon machine or more aggressive chunking.

## 4. Quality bar

The required viewpoint is successful only if a viewer can perceive all of the following without reading developer notes:

- the true scale and unevenness of the ground;
- the Emmitsburg Road and fence as physical obstacles;
- a line of men behaving as a unit rather than independent game NPCs;
- the repeated fire/reload rhythm of muzzle-loading infantry;
- artillery and musket smoke progressively reducing visibility;
- incoming fire changing unit order and behavior;
- casualties accumulating into visible gaps and bodies;
- the Union defensive line and artillery as an intelligible destination and threat;
- the difference between documented facts and reconstructed connective action;
- the ability to leave Soldier View and understand the same event in Atlas Mode.

Reject the visual target if it still reads as colored blocks, a game-engine terrain demo, or a generic nineteenth-century battle.

## 5. Target repository architecture

The final shape should be:

```text
app/
  Assets/
    Battle/
      gettysburg-july3.json          # current compiled macro artifact
      Angle/
        angle.bundle.json            # compiled tactical artifact
        viewpoints.json              # runtime viewpoint metadata
    Scenes/
      Atlas.unity                     # renamed/reworked current scene
      AngleVisualTarget.unity         # isolated quality-development scene
      AngleRender.unity               # deterministic offline render scene
    Scripts/
      Atlas/
      Reconstruction/
      SoldierView/
      Presentation/
    ThirdParty/
      manifest.json                   # redistributable asset inventory
      ...                             # license-compatible derived assets
    ProjectOwned/
      Characters/
      Environment/
      VFX/
      Audio/
  RenderOutput/                       # gitignored image sequences and encodes

reconstruction/
  schemas/
    source.schema.json
    claim.schema.json
    reconstruction.schema.json
    viewpoint.schema.json
    third-party-asset.schema.json
  sources/
    sources.json
  claims/
    angle.claims.json
  canonical/
    angle.reconstruction.json
    angle.viewpoints.json
  scripts/
    compile_angle.py
    validate_assets.py
    validate_reconstruction.py
    encode_viewpoint.sh
  tests/

docs/
  assets/
    THIRD_PARTY_ASSETS.md
  reconstruction/
    angle-editorial-decisions.md
    violence-and-representation.md
    render-runbook.md
```

Names may change to fit Unity conventions, but preserve the separation between evidence, canonical reconstruction, compiled playback data, source assets, and generated media.

## 6. Data model V2

Do not immediately replace the current macro `battle.json`. Compile the Angle V2 data into a tactical sidecar and integrate it progressively.

### 6.1 Sources

`sources.json` contains bibliographic identity once, referenced everywhere else:

```json
{
  "id": "haskell-1908",
  "kind": "memoir",
  "title": "The Battle of Gettysburg",
  "author": "Frank Aretas Haskell",
  "year": 1908,
  "url": "...",
  "editionNote": "..."
}
```

Source records do not themselves prove a claim. Page, plate, map region, quotation, and interpretation live on the claim reference.

### 6.2 Atomic claims

A claim must address one evidentiary proposition, not an entire keyframe:

```json
{
  "id": "claim-armistead-scaled-wall",
  "subjectId": "csa-armistead",
  "property": "action",
  "value": "crossed-stone-wall",
  "time": { "earliest": 8640, "preferred": 8700, "latest": 8760 },
  "geometry": {
    "kind": "point",
    "x": 4415,
    "z": 4855,
    "uncertaintyM": 30
  },
  "assessment": "documented",
  "references": [
    {
      "sourceId": "rawley-martin-shsp-v32",
      "locator": "pp. 183–195",
      "excerpt": "short rights-compliant research excerpt",
      "interpretation": "supports wall crossing, not exact clock time"
    }
  ],
  "note": "Preferred time is part of the canonical reconstruction."
}
```

Required claim properties for the slice include position, time, strength, formation, movement, firing, obstacle crossing, command/action, casualty total, casualty timing, land cover, structure, and weather.

### 6.3 Canonical reconstruction segments

Semantic segments replace unqualified straight-line interpolation:

```json
{
  "id": "seg-garnett-cross-road",
  "unitId": "csa-garnett",
  "t0": 8120,
  "t1": 8300,
  "action": "cross_obstacle",
  "route": [[4030, 4810], [4090, 4805], [4140, 4818]],
  "obstacleId": "emmitsburg-road-west-fence",
  "formationFrom": "line",
  "formationTo": "line-disordered",
  "paceProfile": "halt-compress-cross-redress",
  "claimIds": ["..."],
  "inferenceRules": [
    "route constrained to traced fence gaps and road geometry",
    "crossing duration reconstructed from frontage and drill behavior"
  ]
}
```

Supported actions for v1:

- `hold`
- `advance`
- `double_quick`
- `oblique`
- `halt`
- `dress_line`
- `cross_obstacle`
- `fire_by_rank`
- `fire_independent`
- `take_canister`
- `waver`
- `close_gap`
- `breach`
- `fall_back`
- `rout`

Every segment must cite at least one atomic claim or explicitly identify itself as editorial connective reconstruction.

### 6.4 Casualty profiles

Strength endpoints do not determine exact individual deaths. Add explicit aggregate profiles:

```json
{
  "id": "cas-garnett-angle-approach",
  "unitId": "csa-garnett",
  "t0": 8340,
  "t1": 8700,
  "count": 510,
  "intensityCurve": "rising",
  "causeMix": {
    "musketry": 0.45,
    "canister": 0.45,
    "unknown": 0.10
  },
  "assessment": "reconstructed",
  "claimIds": ["..."],
  "note": "Count/timing distribution compiled from aggregate loss and engagement claims."
}
```

Rules:

- Do not assign historical names to procedural casualties.
- Do not imply exact wound type when the evidence supports only aggregate loss.
- Victim selection, fall timing, animation, and body pose are deterministic from battle seed, unit ID, slot ID, and casualty profile.
- Bodies persist to the end of the viewpoint unless occlusion/performance policy requires an explicitly documented visual pooling substitute.
- Scrubbing to time `T` reconstructs the same living, falling, and dead figures every time.

### 6.5 Viewpoints

```json
{
  "id": "garnett-road-to-angle",
  "title": "With Garnett's Line",
  "unitId": "csa-garnett",
  "slotId": 184,
  "viewKind": "first_person",
  "t0": 8160,
  "t1": 8820,
  "camera": {
    "eyeHeightM": 1.66,
    "fovDeg": 68,
    "lookOffsetDeg": [0, 0],
    "stabilization": 0.35
  },
  "media": {
    "proxy": "...",
    "full": "...",
    "fps": 30,
    "width": 2560,
    "height": 1440
  },
  "claimIds": ["..."],
  "editorialNote": "Representative unnamed soldier viewpoint; not an identified witness."
}
```

## 7. Character and crowd architecture

### 7.1 Open-source character source

Use one of these after a one-task visual bake-off:

- MakeHuman/MPFB core CC0 human assets; or
- Blender Human Base Meshes CC0 bundle.

Prefer MakeHuman if its topology, rig, and deterministic body variation shorten production without compromising close-view quality. Community clothes or plugins are not automatically approved; validate each license separately.

Create project-owned modular Civil War clothing and equipment:

- kepi;
- slouch hat;
- forage cap variants;
- sack coat;
- shell jacket;
- trousers;
- brogans;
- cartridge box and sling;
- cap pouch;
- haversack;
- canteen;
- blanket roll;
- belt and buckle variants;
- bayonet and scabbard;
- musket and ramrod.

Uniform variation must be restrained and historically reviewed. Confederate variation does not mean random earth-tone costumes.

### 7.2 Weapon and artillery candidates

Provisional candidates, subject to file inspection and manifest approval:

- CC-BY Springfield 1861 model from Sketchfab (`enKi/Kien_Tran`), optimized and corrected as needed;
- CC-BY Civil War cannon by Zack Hawley, retopologized from the high-resolution source;
- Smithsonian CC0 scans for reference or usable components where an individual item's metadata permits it.

Do not commit any candidate until its downloaded file, embedded textures, author, license, and redistribution terms match the listing and the asset validator entry.

### 7.3 Animation vocabulary

Author or permissively source, then retarget and modify:

1. stand-ready variations;
2. shoulder-arms march;
3. route-step/disordered advance;
4. double-quick;
5. halt and dress line;
6. aim;
7. fire with recoil;
8. historically ordered multi-stage muzzle-loading reload;
9. kneel and rise;
10. cross/climb rail fence;
11. brace/react to nearby artillery;
12. flinch and recover;
13. nonfatal hit reaction;
14. multiple fatal falls by incoming direction;
15. prone/wounded movement;
16. waver;
17. turn and retreat;
18. routed run.

The reload cycle is a hero requirement. A generic modern-rifle reload is a release blocker.

### 7.4 Offline crowd tiers

Offline rendering removes the 60 FPS requirement but does not remove memory and draw-call limits. Use:

- **Hero tier, 0–25 m:** up to 64 full-quality skinned figures, full equipment, best animation, wound decals, facial variation.
- **Near tier, 25–100 m:** up to 400 reduced-rig or baked-animation figures, reduced material count.
- **Mid tier, 100–350 m:** up to 2,500 GPU-instanced vertex-animation or animation-texture figures.
- **Far tier, beyond 350 m:** formation-density meshes, flags, smoke, and impostors.

The renderer may take longer than real time. It must not exceed the render machine's memory. Record peak memory and render seconds per output frame in every quality report.

### 7.5 Deterministic action resolver

Do not create combat AI. Implement a pure function:

```text
SoldierAction Resolve(
  reconstructionSegment,
  battleTime,
  unitId,
  formationSlotId,
  casualtyProfiles,
  engagementEvents,
  battleSeed)
```

It chooses animation, phase, pose variation, target direction, formation offset, casualty status, and equipment variation. Tests must prove repeatability and invariance under scrubbing direction.

## 8. Environment architecture

### 8.1 Local terrain

Build a dedicated true-scale local terrain tile covering at least:

- the Emmitsburg Road crossing used by Pickett's division;
- the Codori-area approach needed for orientation;
- the stone wall and Angle;
- the Copse of Trees;
- Cushing's and nearby Union battery positions;
- enough east/west depth for artillery, smoke, and horizon composition.

Suggested first crop: approximately `x=3900..4700`, `z=4450..5250`, adjusted after camera blocking. Generate from the cached 1 m DEM rather than upscaling the current 4097-wide Unity terrain.

Acceptance:

- `verticalExaggeration == 1.0`;
- local terrain sample spacing no worse than approximately 1 m;
- positions round-trip to the macro battlefield frame;
- camera-height slopes and sight lines are reviewed against honest elevation;
- no modern above-ground structures are baked into the terrain surface.

### 8.2 Historical environment inventory

The local slice requires sourced geometry for:

- Emmitsburg Road surface and ditches;
- rail fences on both sides of the road;
- the stone wall and Angle geometry;
- fence gaps and damage relevant to crossings;
- Codori buildings visible from the viewpoint;
- the Copse of Trees;
- orchards, wheat, pasture, trampled crop transitions, and field boundaries;
- artillery positions, limbers, caissons, ammunition boxes, and abandoned equipment;
- period telegraph/utility absence—do not import modern landscape clutter;
- body and equipment accumulation during the viewpoint.

Each feature needs a source claim or a clearly marked reconstruction decision.

### 8.3 Materials and atmosphere

Use CC0 Poly Haven or equivalent redistributable PBR materials as ingredients, modified into a coherent battlefield palette. Required HDRP material families:

- pasture grass;
- dry summer grass;
- wheat/stubble;
- packed dirt road;
- disturbed/trampled soil;
- local stone;
- weathered timber;
- canvas;
- wool;
- leather;
- iron/bronze artillery surfaces;
- skin, blood, and uniform cloth.

Lighting must use the project's July 3 sun ephemeris at the rendered battle time. Add only historically supportable haze/cloud conditions. Presentation color grading may improve legibility but must be documented.

## 9. Effects, violence, and audio

### 9.1 Weapons and smoke

Required effects:

- per-musket muzzle flash, cap flash, and dense white/gray black-powder smoke;
- smoke accumulation along firing lines;
- cannon muzzle blast, smoke, recoil, and crew response;
- projectile strike effects appropriate to earth, wood, stone, and bodies;
- dust from marching and impacts;
- progressive visibility loss driven by wind and fire density;
- deterministic effect seeds and emission timing.

Do not use modern smokeless-weapon visual references.

### 9.2 Graphic casualty representation

The product may show explicit blood, wounds, falls, wounded movement, and bodies. Guardrails:

- Add a clear content warning before first entry into Soldier View.
- Avoid celebratory slow motion, scoring, kill feedback, or game-like hit markers.
- Do not assign exact graphic wounds to named historical people without evidence.
- Use a limited wound-category vocabulary tied to broad cause classes; label it reconstructed when inspected.
- Bodies and dropped equipment affect the scene composition and formation gaps.
- Keep visual treatment sober and documentary.
- Add `docs/reconstruction/violence-and-representation.md` explaining these rules.

### 9.3 Audio

Replace synthesized loops in Soldier View with a licensed multitrack soundscape:

- distant and near artillery reports;
- supersonic/subsonic projectile and impact layers as appropriate;
- individual and rolling musket fire;
- black-powder weapon handling and reload Foley;
- marching, running, clothing, equipment, and fence crossing;
- shouted orders and unit noise;
- breathing and exertion near the camera;
- pain, panic, and wounded voices;
- wind, insects, and rural ambience;
- dynamic muffling through smoke/distance only where acoustically justified.

Every source recording requires the same license manifest discipline as visual assets. CC0 is preferred; CC-BY is allowed with attribution. NC, ND, unclear, and sampling-restricted sources are forbidden.

Render final Soldier View audio as stems first, then a mastered synchronized track. Retain stems in release-source storage so the mix can be revised without rerendering images.

## 10. Atlas and transition UX

Replace the IMGUI placeholder with Unity UI Toolkit or another retained-mode Unity UI already supported by the project.

Minimum Atlas UI:

- play/pause and speeds `1×`, `10×`, `60×`;
- wall-clock time;
- day/slice context;
- moment markers;
- unit labels with decluttering;
- selection and follow;
- documented/reconstructed/contested visual status;
- source/provenance drawer;
- contours and relief shading;
- Soldier View entry markers only while the battle clock is within a viewpoint's time window;
- graphic-content warning on first entry;
- clear return to the exact Atlas time.

### 10.1 Video synchronization

Implement `SoldierViewPlayer` around Unity `VideoPlayer`:

```text
videoTime = battleClockTime - viewpoint.t0
battleClockTime = viewpoint.t0 + videoTime
```

Requirements:

- video never becomes an independent source of historical time;
- play/pause affects both clock and video;
- seeking pauses, shows the proxy frame/transition, seeks to the nearest keyframe, then settles to the requested time;
- leaving Soldier View restores Atlas at the exact battle time;
- entering outside `[t0,t1]` is impossible;
- media checksum and viewpoint metadata must agree;
- missing full-resolution media falls back to the proxy with a clear development warning;
- no network dependency at playback time.

Encode H.264 MP4 first for reliable macOS playback. Use a short GOP (target one keyframe per second or better) to support seeking. The full-resolution deliverable target is 2560×1440 at 30 fps; provide a 1280×720 proxy. Measure actual file size and decoder seek behavior before locking bitrate.

Generated media belongs in GitHub Releases or equivalent versioned release storage with checksums. Do not commit multi-gigabyte image sequences or final videos to normal Git history.

## 11. Asset licensing system

Create `app/Assets/ThirdParty/manifest.json` and validate it in tests. Each entry requires:

```json
{
  "id": "springfield-1861-enki",
  "path": "Assets/ThirdParty/Weapons/Springfield1861",
  "title": "springfield 1861",
  "author": "enKi / Kien Tran",
  "sourceUrl": "https://sketchfab.com/...",
  "license": "CC-BY-4.0",
  "licenseUrl": "https://creativecommons.org/licenses/by/4.0/",
  "acquired": "2026-07-08",
  "redistributable": true,
  "modified": true,
  "modifications": "retopology, texture packing, LODs, scale correction",
  "sha256": "..."
}
```

Validation fails when:

- a file exists under `ThirdParty` without a manifest owner;
- a required field is absent;
- the license is unknown, NC, ND, source-only, or otherwise incompatible;
- `redistributable` is false;
- attribution text cannot be generated for CC-BY assets;
- the checksum changes without a manifest update.

Generate `docs/assets/THIRD_PARTY_ASSETS.md` from the manifest. Do not hand-maintain duplicate attribution text.

## 12. Phased execution

### Phase 0 — Preserve, benchmark, and rename

**Files:** README, scene names/references, benchmark docs only.

- [ ] Record current Git SHA and create a durable tag or branch before render-pipeline migration.
- [ ] Run and record all existing suites: Python, TypeScript, Unity EditMode.
- [ ] Capture current Atlas screenshots at `t=0`, `t=8160`, `t=8700`, and `t=9000`.
- [ ] Record current real-time frame rate and memory on the target Mac. If a measurement requires an interactive editor/player session that cannot be run headlessly, document the exact blocker, package it for the owner's next verification session, and continue the phase.
- [ ] Record known pre-existing conditions so they are not later attributed to V2 work: the Phase 6 smoke/audio systems on `main` are code-complete but not yet owner-verified in editor or on device (open perf case: bombardment peak, ~30 simultaneous obscuration emitters; owner-unconfirmed fixes: muzzle bloom, pause silence). Last recorded suite baselines: tool 108 / pipeline 38 / Unity 119 (commit `af2022c`) — replace with measured values.
- [ ] Rename/copy the current scene to `Atlas.unity` without functional change.
- [ ] Add the locked decisions from this plan to an ADR or design document.

**Gate P0:** clean checkout reproduces all current tests and opens the preserved Atlas scene.

### Phase 1 — Prove the media contract before making art

**Files:** new Soldier View metadata, player, tests, and a generated ten-second proxy.

- [ ] Define and validate `viewpoint.schema.json`.
- [ ] Add one development `ViewpointDefinition` covering ten seconds.
- [ ] Render or generate a ten-second timecode proxy video. Generate it with a pinned, documented ffmpeg command (e.g. `testsrc2` plus a `drawtext` burn-in of battle time `t` and frame number); commit the command and its documentation, not the media — generated media stays gitignored per the plan's media policy.
- [ ] Implement `SoldierViewPlayer` with enter, exit, play, pause, and seek. Place runtime code under `Assets/Scripts/SoldierView/` per the target layout (§5).
- [ ] Add exact battle-time/video-time mapping tests. These sync tests establish this project's first PlayMode-or-headless-runtime testing pattern (the existing Unity suite is EditMode-only); document how to run them beside the existing suites.
- [ ] Measure seek latency on base Apple Silicon or the lowest available target.
- [ ] Choose proxy/full transition behavior from measured latency.

**Gate P1:** a tester can scrub Atlas time, enter the proxy, play/pause/seek, exit, and return to the same battle second without drift greater than one video frame after settle.

Gate P1's tester is the project owner. The phase ends with a prepared one-command demo entry point and a written checklist for that session; executor self-verification does not close the gate.

### Phase 2 — Asset/license infrastructure

- [ ] Add the third-party manifest schema and validator.
- [ ] Add generated attribution documentation.
- [ ] Add CI/test enforcement.
- [ ] Define Git LFS versus reproducible-fetch policy for each asset class.
- [ ] Exclude Unity Asset Store and Mixamo content from required build inputs.
- [ ] Import one CC0 material and one CC-BY model through the complete process.

**Gate P2:** a clean checkout can identify the source, license, checksum, and attribution of every third-party test asset, and the validator rejects a deliberately unmanifested fixture.

### Phase 3 — HDRP/URP visual-target bake-off

Work on a branch and isolated scene. Do not convert the full project first.

- [ ] Crop the true-scale local Angle terrain from the cached 1 m DEM.
- [ ] Create equivalent URP and HDRP visual-target scenes or preserve enough measurements for a fair comparison.
- [ ] Stage placeholder road, wall, fence, wheat, trees, 100 soldiers, smoke, and actual-time sunlight.
- [ ] Produce the same theater, tactical, and eye-level frames from each pipeline.
- [ ] Record image quality, setup complexity, memory, and offline render time.
- [ ] Decide pipeline in a short ADR.

**Default decision:** choose HDRP unless it fails on Apple Silicon, breaks required instancing without a tractable replacement, or produces no meaningful visual advantage in the target frames.

**Gate P3:** approved eye-level still frame at true scale with readable terrain, fence, wall, figures, atmosphere, and shadowing. No macro slabs are allowed in the frame.

### Phase 4 — Complete HDRP migration

Only if P3 selects HDRP:

- [ ] Install the compatible HDRP, Recorder, Cinemachine, and VFX Graph packages.
- [ ] Replace URP pipeline settings with versioned HDRP quality profiles.
- [ ] Convert terrain, unit, soldier, flag, vegetation, smoke, dust, crop, and fence materials.
- [ ] Replace custom URP-only shaders with HDRP-compatible Shader Graph/HLSL equivalents.
- [ ] Preserve deterministic battle-time inputs in animated shaders.
- [ ] Restore Atlas rendering and all tests.
- [ ] Add a low playback profile and a high offline-render profile.

**Gate P4:** Atlas works under the selected pipeline, current data loads, all tests pass, and the offline profile captures a deterministic frame on Apple Silicon.

### Phase 5 — Reconstruction V2 compiler

- [ ] Add schemas for sources, claims, reconstruction segments, casualty profiles, and viewpoints.
- [ ] Split the Angle research into atomic claims.
- [ ] Write `angle-editorial-decisions.md`, including the canonical timing choice and known witness disagreement.
- [ ] Author semantic segments for Garnett, Armistead, Kemper, Webb, 69th PA, 71st PA, 72nd PA, Cushing's battery, and directly visible supporting units.
- [ ] Implement route-constrained and action-aware compilation.
- [ ] Emit `angle.bundle.json` without changing macro battle behavior.
- [ ] Add referential integrity, speed, obstacle, strength, casualty, and source-coverage validation.

**Gate P5:** every compiled second from `t=8040..9000` has deterministic unit state and action; every output traces to claims and/or named inference rules; no straight segment crosses a traced wall/fence except through an authored crossing action.

### Phase 6 — Character kit and animation proof

- [ ] FIRST: run a short feasibility spike — one base mesh, one garment, one retargeted or hand-authored march clip, rendered at hero distance — and write a go/no-go note with the documented fallback (accept a lower-fidelity CC-BY rigged figure set) before building the full kit. Hand-authoring the reload cycle and modeled clothing without Mixamo is this plan's highest skilled-labor risk; surface it early, not in week three.
- [ ] Compare MakeHuman and Blender CC0 bases in a close-view test.
- [ ] Select one base and record the decision.
- [ ] Build one Union and one Confederate modular soldier.
- [ ] Build/import the approved musket.
- [ ] Rig and retarget.
- [ ] Author march, aim, fire, full reload, fence crossing, hit, fatal fall, and retreat first.
- [ ] Create body, equipment, and uniform variations.
- [ ] Create hero/near/mid LODs or baked-animation tiers.
- [ ] Review uniform and weapon silhouette against research references.

**Gate P6:** a 60-second eye-level test with 100 soldiers shows a credible formation, historically legible reload cycle, deterministic action variation, one fence crossing, explicit falls, and persistent bodies.

### Phase 7 — Environment completion

- [ ] Author sourced road, fence, wall, building, field, and tree geometry for the local crop.
- [ ] Create coherent PBR terrain and prop materials.
- [ ] Add local crop displacement/geometry, trampling masks, wheel/foot paths, and terrain blending.
- [ ] Add time-accurate sunlight, sky, haze, wind, and color grade.
- [ ] Build artillery, limber, ammunition, and battlefield-detritus props needed by the hero frame.
- [ ] Validate all geometry against macro coordinates and claims.

**Gate P7:** the local scene is recognizably the Angle without units or UI, at both tactical and eye height.

### Phase 8 — Deterministic action, casualty, and VFX scene

- [ ] Implement formation-slot identities and crowd tiers.
- [ ] Implement `SoldierActionResolver` as pure deterministic logic.
- [ ] Compile fire/reload cycles from tactical actions and fire events.
- [ ] Implement casualty profiles and deterministic victim selection.
- [ ] Add wound decals, blood effects, dropped equipment, and persistent bodies.
- [ ] Replace generic puffs with black-powder musket and artillery VFX.
- [ ] Add smoke accumulation, wind drift, visibility reduction, dust, and impacts.
- [ ] Add render-time validation for missing animations, invalid slots, or overflow.

**Gate P8:** seeking to any tested time reconstructs bitwise-identical logical soldier states and visually equivalent deterministic frames; casualty totals match compiled profiles; no double-count occurs between parent and child units.

### Phase 9 — Hero viewpoint and audio

- [ ] Block the full `garnett-road-to-angle` camera path/attachment.
- [ ] Choose first-person versus close-third-person from side-by-side 30-second renders.
- [ ] Author camera stabilization, gait, recoil response, and fall/chaos behavior without inducing excessive motion sickness.
- [ ] Source and manifest audio.
- [ ] Build synchronized audio stems.
- [ ] Mix a 60-second proof, then the full viewpoint.
- [ ] Add content warning and representative-viewpoint explanation.

**Gate P9:** an enthusiast reviewer understands where they are, what their unit is doing, what threatens it, and that the observer is representative rather than an identified person.

### Phase 10 — Offline render pipeline

- [ ] Create `AngleRender.unity` containing only deterministic render dependencies.
- [ ] Configure Recorder for fixed 30 fps image-sequence output.
- [ ] Freeze seeds, quality settings, color management, resolution, and package versions.
- [ ] Add resumable chunk rendering by battle-time range.
- [ ] Write frame metadata containing Git SHA, input bundle checksum, viewpoint ID, battle time, frame number, and settings hash.
- [ ] Detect missing/duplicate frames before encode.
- [ ] Pin and document the `ffmpeg` encode command/version; fail clearly when it is unavailable.
- [ ] Encode 720p proxy and 1440p H.264 full media with short GOP.
- [ ] Generate media checksums and release manifest.
- [ ] Document render time, peak memory, and total output size.

**Gate P10:** two separate renders of the same 10-second range produce identical logical metadata and visually identical output within documented nondeterministic GPU tolerances; encoded video seeks acceptably on target hardware.

### Phase 11 — Atlas presentation and integration

- [ ] Replace IMGUI timeline with retained-mode production UI.
- [ ] Add unit selection, labels, activity hierarchy, and source drawer.
- [ ] Add uncertainty/contested styling.
- [ ] Replace tall unit slabs with map-like ribbons/footprints at theater scale.
- [ ] Add Soldier View marker and entry transition.
- [ ] Synchronize proxy/full video with the battle clock.
- [ ] Return to exact Atlas state on exit.
- [ ] Add first-launch graphic-content warning.
- [ ] Add attribution/credits view generated from the asset manifest.

**Gate P11:** a new user can locate Pickett's Charge, understand the approach, enter the hero viewpoint during its valid window, seek and play it, inspect why the reconstruction made its choices, and return without developer assistance.

### Phase 12 — Review and release

- [ ] Historical review: positions, timing, uniforms, drill, weapons, terrain, structures, casualty representation, and source claims.
- [ ] Enthusiast review with at least three people not involved in implementation.
- [ ] Visual review at 1440p and 1080p.
- [ ] Playback test on base M1 8 GB if available; otherwise document the lowest tested Apple Silicon configuration.
- [ ] Accessibility basics: readable text, subtitle/caption option for shouted orders, volume controls, motion-reduction cut if camera gait is uncomfortable.
- [ ] Verify open-source checkout and asset licenses.
- [ ] Publish generated media as versioned release artifacts.
- [ ] Update README to distinguish source checkout, required generated terrain, and optional/full media downloads.

**Final gate:** all Definition of Done items below pass. Only then may full-battle expansion resume.

## 13. Testing requirements

### Python/reconstruction

- schema validation for every new document;
- source and claim reference integrity;
- canonical segment overlap/gap rules;
- route stays within battlefield and honors obstacles;
- arm-appropriate speed limits;
- casualty counts never exceed available strength;
- compiled strength reconciles with casualty profiles;
- deterministic output checksum;
- media/viewpoint time-range consistency;
- third-party asset license and checksum validation.

### Unity EditMode

- `SoldierActionResolver` determinism;
- formation-slot identity stability;
- casualty victim/timing stability;
- body-state reconstruction before/during/after fall;
- battle/video time conversion;
- viewpoint availability boundaries;
- Atlas-to-Soldier return state;
- render-profile settings validation;
- true-scale terrain conversion and coordinate round-trip.

### Unity PlayMode/integration

- proxy and full media enter/exit;
- pause, resume, and seek behavior;
- missing media fallback;
- content-warning persistence;
- source drawer from a Soldier View;
- Atlas performance and memory smoke test;
- offline render scene loads without editor-only manual setup.

### Visual regression

Capture approved golden frames at:

- `t=8160`: Emmitsburg Road crossing;
- `t=8400`: closing under canister;
- `t=8580`: wall approach;
- `t=8700`: Angle crisis;
- `t=8820`: collapse/repulse transition.

Automated pixel equality is not required for transparent VFX, but the review tool must present current and approved frames side by side with settings and input hashes.

## 14. Definition of Done

The plan is complete when:

- [ ] Existing macro battle and authoring data remain available.
- [ ] Atlas uses true-scale terrain or a clearly separate non-geometric relief presentation; Soldier View is always true scale.
- [ ] Atlas no longer presents tall debug slabs as its final unit language.
- [ ] The Angle local terrain and visible historical environment are sourced and recognizable.
- [ ] The hero viewpoint covers at least `t=8160..8820` continuously.
- [ ] The hero media is available as 1440p30 full and 720p30 proxy encodes.
- [ ] Play, pause, seek, enter, and exit remain synchronized to within one frame after decoder settle.
- [ ] Nearby soldiers march, fire, reload, cross an obstacle, react, fall, retreat, and leave bodies according to deterministic aggregate reconstruction.
- [ ] The muzzle-loading reload is historically credible.
- [ ] Smoke materially affects visibility.
- [ ] Explicit violence is presented soberly with a content warning.
- [ ] No procedural casualty is presented as an identified historical person's exact fate.
- [ ] Every tactical output traces to source claims and/or displayed reconstruction rules.
- [ ] Every required external asset is redistributable and present in the generated attribution document.
- [ ] No Mixamo or Unity Asset Store source file is required for an open-source build.
- [ ] Playback succeeds on the lowest documented Apple Silicon configuration.
- [ ] All Python, TypeScript, Unity EditMode, and required PlayMode tests pass.
- [ ] Three uninvolved enthusiasts can explain the action, terrain, and documented-versus-reconstructed distinction after using the slice.

## 15. Explicit non-goals

- all three Gettysburg days at Soldier View fidelity;
- arbitrary first-person entry anywhere;
- camera rotation or locomotion inside pre-rendered Soldier View;
- combat AI or counterfactual outcomes;
- historically identified anonymous soldiers;
- exact per-person casualty claims from aggregate records;
- mobile/iPhone optimization;
- VR;
- multiplayer;
- path-traced final rendering on Apple Silicon Unity;
- AAA facial animation;
- extension viewpoints before the required viewpoint passes its gate.

## 16. Executor operating instructions

- Work gate by gate. Do not start the next phase while the current gate has known failures.
- Make small, reviewable commits named by phase and task.
- Preserve unrelated user changes and never replace the historical corpus wholesale.
- Run the narrow test first, then the full relevant suite before each gate.
- Record all external downloads before importing them.
- Never commit generated image sequences, caches, or unlicensed source packages.
- Never silently substitute spectacle for missing evidence. Add a claim, an editorial decision, or omit the detail.
- When historical sources conflict, implement the locked canonical choice and expose the disagreement; do not invent scenario switching.
- When visual quality and arbitrary interactivity conflict, preserve the fixed-view pre-render contract for v1.
- When visual quality and historical legibility conflict, prefer historical legibility.

### 16.1 Project-specific operating constraints

- Unity batch/CLI work requires the Unity editor to be closed; there is exactly one Unity-CLI owner at a time. The Unity MCP bridge is historically unreliable — "Session Active" in the editor window does not mean the bridge is reachable; prefer the CLI (`"$UNITY" -batchmode ...`, Unity 6000.4.11f1), as all of Phase 5C was run.
- Never kill GUI applications (Unity, browsers, simulators). If an open app blocks CLI work, report it and wait for the owner.
- Use a git worktree for any work that could run in parallel with another agent; a single sequential executor works on a phase branch cut from the Phase 0 preservation tag.
- When a checklist item needs an interactive session no agent can run, document the blocker, package it for the owner's next verification session, and continue with what remains.

## 17. Recommended first executor dispatch

The first implementation agent should receive only Phases 0 and 1:

> Preserve and benchmark the current project, add the V2 viewpoint schema, create a ten-second timecode proxy, and implement/test exact battle-clock synchronization through a fixed Soldier View video. Do not migrate render pipelines or import production assets yet. Stop at Gate P1 with measurements and a clean handoff.

This ordering proves the architectural consequence of pre-rendering before expensive art, data migration, or HDRP work begins.

## 18. External implementation references

These links establish capability or license only; they are not blanket approval of every asset on a platform:

- Unity HDRP platform and feature overview: <https://unity.com/features/srp/high-definition-render-pipeline>
- Unity Recorder package documentation: <https://docs.unity3d.com/Packages/com.unity.recorder@latest>
- Unity Recorder image sequence and accumulation concepts: <https://docs.unity3d.com/Packages/com.unity.recorder@latest/manual/RecorderImage.html>
- Poly Haven CC0 license: <https://polyhaven.com/license>
- MakeHuman/MPFB core asset license: <https://static.makehumancommunity.org/about/license.html>
- Blender Human Base Meshes download index: <https://download.blender.org/demo/bundles/bundles-3.6/>
- Smithsonian Open Access: <https://www.si.edu/OpenAccess>
- Provisional Springfield 1861 candidate: <https://sketchfab.com/3d-models/springfield-1861-bc19a986c2064ae69e5d1f105e5b21ac>
- Provisional Civil War cannon candidate: <https://sketchfab.com/3d-models/civil-war-cannon-77642765da02400cbc8db58b5532ff9e>

The executor must archive the actual license metadata present on acquisition day because marketplace listings and availability can change.
