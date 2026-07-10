# Phase 12 owner checklist — review and release

The executor closed the technical items of plan §12 Phase 12 (speed
designator, accessibility basics, license verification, golden frames,
playback verification on this machine, README/docs pass — evidence in
`docs/benchmarks/captures/p12-gate/` and `p12-gate-evidence.md`). Four
items are yours; this checklist packages them. A copy sits in
`docs/benchmarks/captures/p12-gate/` beside the evidence.

Session setup, same as P11: `./scripts/p11-demo.sh` stages media,
validates metadata/attribution, and opens the editor. New this phase:
the Options modal (volumes, captions, reduced motion), the CC button and
1×-designator inside Soldier View.

---

## 1. Historical review (§12 P12)

The claims corpus is `reconstruction/claims/angle.claims.json`
(93 claims: 68 documented / 23 inferred / 2 unknown, 42 sources in
`reconstruction/sources/sources.json`); editorial rulings are
`docs/reconstruction/angle-editorial-decisions.md` (ED-1…ED-23 plus the
named inference rules). Work each category against the claims that feed
it — the in-app source drawer shows the same data the renderer used, so
spot-check there, and use the golden frames
(`docs/benchmarks/captures/p12-gate/golden/`, five §13 times at 1440p +
1080p) or the full film for the visual half.

For each category: the question to answer, where the data is, and where
to look.

- [ ] **Positions** — do unit tracks and the final assault geometry
  match the sources? 16 `position` claims (per-unit: garnett, kemper,
  armistead, webb, 69/71/72 PA, Cushing, plus supporting units).
  Atlas Mode at 15:14→15:30 with the drawer open per unit; golden
  t=8160/8400/8580 for the approach axis; ED-4 (72nd PA crest-vs-wall),
  ED-5 (who reached the works), ED-8 (Armistead after the crossing).
- [ ] **Timing** — 6 `time` claims + the canonical clock choices:
  ED-1 (step-off and the slice clock frame), ED-2 (Armistead's wall
  crossing, preferred 8700), ED-7 (Arnold's withdrawal). The moment
  markers on the timeline carry their citations on hover.
- [ ] **Uniforms** — the kit silhouette against research references:
  `docs/reconstruction/p6-kit-silhouette-review.md` recorded the P6
  review; confirm nothing regressed at the golden frames (kepi/slouch
  hat mix, sack coat vs shell jacket, equipment set, restrained CSA
  variation — plan §7.1).
- [ ] **Drill** — the muzzle-loading reload read (hero requirement,
  §7.3), fire-by-rank cadence (ED-20), formation frontage/dress,
  crossing behavior at the fences (known punchlist item: the climb
  CLIP reads poorly — judged deferred at P10; the drill LOGIC is what
  this review checks). Film 3:20–4:10 (the observer's own crossing) and
  7:00–9:00 (canister at the wall).
- [ ] **Weapons** — Springfield 1861 and the cannon against period
  references: `app/Assets/ThirdParty/manifest.json` entries, silhouettes
  at golden t=8580/8700; no modern-weapon reads (§9.1).
- [ ] **Terrain** — true-scale (1.0×) local crop against honest
  elevation: the road cut and swale reads, sight lines to the clump of
  trees and the crest (ED-11; the §8.1 acceptance was measured at P7 —
  this is the judgment pass).
- [ ] **Structures** — 13 `structure` claims: both Emmitsburg Road
  fences and their gaps (ED-6, ED-12), the stone wall and Angle
  geometry, Codori buildings (ED-13), the Copse (ED-14), Cushing's
  battery layout (ED-16).
- [ ] **Casualty representation** — aggregate-only discipline: counts
  and timing from the 5 `casualty_total` + 3 `casualty_timing` claims;
  deterministic victim selection with no named-person fates (ED-21);
  the observer exemption disclosure (ED-22, shown in the in-view
  drawer); sober treatment per
  `docs/reconstruction/violence-and-representation.md`.
- [ ] **Source claims** — spot-check the drawer against the cited
  sources: pick 5 units at 3 times each; every displayed line must
  trace (segment citation, engagement events, or "no reliable record" —
  the honest-silence text). The viewpoint's own claim ids are listed in
  the in-view Sources drawer.

Record outcomes inline here or in a review note; anything failed
becomes a punchlist entry or a data fix before release.

## 2. Enthusiast review — three uninvolved people (§12 P12, §14)

Protocol (per person, ~30 minutes, unassisted):

1. Fresh session (`scripts/p11-demo.sh`; delete the content-warning
   PlayerPrefs key or use a fresh machine account so they see the
   first-entry warning). Give them ONLY this brief: *"This is an
   interactive reconstruction of Pickett's Charge on the afternoon of
   July 3, 1863. Explore it however you like; find your way inside the
   charge if you can."* Do not demonstrate, do not name the controls.
2. Observe silently (screen recording recommended). Note whether they:
   locate the charge on the timeline, enter Soldier View inside the
   window, play/seek, open the source drawer, exit.
3. Afterwards, the §14 comprehension test — they should be able to
   explain, in their own words:
   - **the action**: what the Confederate line did between the road and
     the wall, and how it ended;
   - **the terrain**: why the road, its fences, and the stone wall
     mattered to men on foot;
   - **documented versus reconstructed**: that the product
     distinguishes sourced facts from editorial reconstruction, and
     where they saw that distinction (drawer wording, moment-marker
     citations, the representative-observer note).
4. Also collect: the content warning (did it read as honest or as
   drama?), motion comfort (offer the reduced-motion setting if they
   report discomfort — note that the shipped media itself is
   standard-motion), caption legibility if they enable CC.

**Pass (§14):** all three can explain action, terrain, and the
documented/reconstructed distinction. Record each session's notes under
`docs/benchmarks/captures/p12-gate/enthusiast-<n>.md`.

- [ ] Enthusiast 1
- [ ] Enthusiast 2
- [ ] Enthusiast 3

## 3. Base-M1 8 GB playback test (§12 P12)

The plan's requirement is base-M1 "if available; otherwise document the
lowest tested configuration". **Lowest tested so far: Apple M4, 24 GB
(this machine)** — full evidence at
`docs/benchmarks/captures/p12-gate/p12-playback.json` (Atlas 60×
playback, enter on production media, 1440p playback, six-seek battery,
exact-return exit, FPS/memory per stage). The M1 pass stays open until
run on real hardware:

1. On the M1: clone, regenerate terrain (README "Fresh-clone setup"),
   stage the release media, then
   `scripts/p12-playback.sh` — it builds the Development standalone and
   runs the same automated probe; or run the app and do the loop by
   hand (enter at 15:20, play 30 s, three long seeks, exit).
2. Watch for: memory pressure (8 GB machine — Activity Monitor memory
   pressure staying out of red), decoder stutter on 1440p (if present,
   confirm the 720p proxy path by renaming the full file away), seek
   feel (worst measured here: ~101–107 ms — the punchlist's proxy-frame
   transition question).
3. Drop the probe's JSON + screenshots into
   `docs/benchmarks/captures/p12-gate/base-m1/` and note pass/fail here.

- [ ] Base-M1 8 GB run (or explicitly accept M4/24 GB as the documented
  lowest tested configuration for v1)

## 4. Publish the media release (§12 P12, plan §10.1)

The P10 release manifest's original command pointed at
`app/RenderOutput/p10/` in the PHASE-10 WORKTREE, which may be cleaned
at any time; the durable copies are the gate-evidence files in THIS
checkout — same bytes, verified against the release manifest's sha256s
at Phase 12. From the main checkout root:

```sh
# 1. verify the artifacts against the release manifest (expected:
#    full  50c4725e6a90451183af2fe50f4a42c8fc00c72a03da4f12f42ee4d2df975ed1
#    proxy 57e164bd7c4255c6fc0c54c9f73e848e0530907a97e9c7c11918cf8b1b08d642)
shasum -a 256 \
  docs/benchmarks/captures/p10-gate/garnett-road-to-angle.full.mp4 \
  docs/benchmarks/captures/p10-gate/garnett-road-to-angle.proxy.mp4

# 2. publish (repo: wam94/battle-atlas)
gh release create soldier-view-media-v1 \
  docs/benchmarks/captures/p10-gate/garnett-road-to-angle.full.mp4 \
  docs/benchmarks/captures/p10-gate/garnett-road-to-angle.proxy.mp4 \
  docs/benchmarks/captures/p10-gate/p10-release-manifest.json \
  --title "Soldier View media v1 — garnett-road-to-angle" \
  --notes-file docs/benchmarks/captures/p10-gate/p10-release-notes.md
```

After publishing:

- [ ] Replace the README's "forthcoming" wording for the
  `soldier-view-media-v1` release with the release URL.
- [ ] Spot-check a fresh download's sha256 against the manifest.

## 5. Definition of Done (§14) — status at executor handoff

| §14 item | status |
| --- | --- |
| Macro battle + authoring data remain available | done (P0/P4 preserved; suites green) |
| Atlas true-scale or clearly separate relief; Soldier View true scale | Soldier View true scale (done); Atlas keeps 2.5× sand-table relief — decide/document the "clearly separate non-geometric relief presentation" wording (punchlist) |
| No tall debug slabs as final unit language | done (P11 ribbons/footprints) |
| Angle terrain + environment sourced and recognizable | done (Gate P7); species-accurate trees deferred (punchlist) |
| Hero viewpoint covers t=8160..8820 continuously | done (P10, 19,815 frames) |
| 1440p30 full + 720p30 proxy encodes | done (P10; publish = item 4 above) |
| Play/pause/seek/enter/exit sync within one frame after settle | done (P10/P11 measured; re-verified in the P12 standalone probe) |
| Soldiers march/fire/reload/cross/react/fall/retreat, persistent bodies | done (Gates P6–P9) |
| Historically credible muzzle-loading reload | done (P6 gate); drill review = item 1 |
| Smoke materially affects visibility | done (P8) |
| Explicit violence sober + content warning | done (P9/P11; warning version-persisted) |
| No procedural casualty presented as identified person | done (ED-21/ED-22; casualty-representation review = item 1) |
| Every tactical output traces to claims/displayed rules | done (P5 compiler validation; drawer spot-check = item 1) |
| Every required external asset redistributable + attributed | done (36-asset manifest validates; TMP/UCL carve-out documented at P12) |
| No Mixamo/Asset Store source required | done (validator-enforced) |
| Playback succeeds on lowest documented Apple Silicon config | M4/24 GB verified (P12 probe); base-M1 = item 3 |
| All Python/TS/EditMode/required PlayMode tests pass | done (P12: pipeline 59, tool 108, reconstruction 99, EditMode 356+1 skip, PlayMode 16) |
| Three uninvolved enthusiasts pass the comprehension test | **open — item 2** |

**Final gate:** items 1–4 above close it. Only then may full-battle
expansion resume (plan §12).
