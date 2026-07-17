# Iverson production render — `iverson-forney-field` (second Soldier View film)

**Status:** executor evidence for the `iverson-production` slice.
**Authorization:** the design gate PASSED (owner: 12th NC observer,
first person, window t=5830..7040, smoke AS-IS) and the named blocker
(`fight_prone` vocabulary) merged — `iverson-viewpoint-design.md` §8.5's
production path runs: the ED-21 seed re-pin plus the P10 pattern.
**Branch:** `iverson-production`. Owner publishes the media release;
this slice never publishes.

---

## 1. The ED-21 production seed pin

`compile_iverson.py`'s `STAGING_SEED` moved from the gate-stage proof
pin (`iverson-proof-seed/1`) to the **checksum of the bundle the owner
reviewed at the gate**: `2f15dd2f4e5e399e9899de45e5606a5610309dfad503ff472edf5e2edb09bf43`
(the fight-prone merge's reviewed bundle) — the Angle ED-21 convention.
The bundle was regenerated deterministically; **only the `stagingSeed`
and `checksum` fields changed** (every other top-level key verified
byte-identical; new bundle checksum `ebb9e3b6095f…`). Enforcement, like
the Angle's `d470c469…` pin:

- reconstruction: `test_staging_seed_is_the_ed21_production_pin`
- EditMode: `IversonBundleTests.Bundle_StagingSeed_IsPinnedAtTheReviewedChecksum`
- render time: every `IversonProductionRender` entry point hard-fails
  on any other seed (`AssertPin`)

Consequence (inherent in the re-pin the design doc ordered): the
hash-drawn choreography (victim draws, stagger phases, yaw) re-rolled
once, from the proof seed to the production seed; the preflight and
determinism gates below validate the production draws. Future
provenance-only recompiles can never re-roll the shipped film again.

## 2. Harness

`IversonProductionRender` (app/Assets/Editor/IversonProduction/) — the
P10 pattern site-parameterized per the design doc: staging is
`IversonGateRender.Boot` (Oak Ridge crop + Iverson bundle, offline HDRP
profile), the frame loop / chunk manifests / tolerances are
`Phase10Render`'s own members. Window t=5830..7040 + 0.5 s media-
contract pad = **36,315 frames** at 2560×1440/30 fps in **21 resumable
60 s chunks**. Scripts: `iverson-render-loop.sh` (SIGKILL-resilient
runner), `iverson-chunk-harvester.sh` (the full PNG scratch is ~116 GB
against ~44 GB free — the rolling harvester is mandatory here),
`iverson-encode.sh` (pinned libx264 slow CRF 18, GOP 30, +faststart,
AAC 192k mix), `iverson-release-manifest.py`.

## 3. Preflight (no-teleport gate)

TBD (report: `docs/benchmarks/captures/iverson-production/iverson-preflight.json`)

## 4. Determinism pair (t=6300..6310, two independent stagings)

TBD (report: `iverson-determinism.json`)

## 5. Render stats

TBD

## 6. Audio (9-stem deterministic mix)

TBD

## 7. Encode + media

TBD (sha256: `app/RenderOutput/iverson/iverson-media.sha256`,
release manifest: `iverson-release-manifest.json`)

## 8. Seek measurements (media contract)

TBD

## 9. Content warning + cross-phase entry

The design slice's §7 text ships as a **per-viewpoint override** in
`content-warning.json` (`viewpointOverrides[0]`, viewpointId
`iverson-forney-field`, version 1) with its own acknowledgement key
(`…contentwarning.ackVersion.iverson-forney-field`): acknowledging the
Angle's warning never suppresses this film's — each film's warning
surfaces before ITS first entry (same mechanics, per override).
PlayMode-verified in the real entry flow
(`IversonWarning_RendersInEntryFlow_WithItsOwnAcknowledgement`).

**Owner check (wording change):** the design-gate draft of the observer
note disclosed TWO unshown record facts (the lying-down fight and the
surrender). The `fight_prone` slice shipped the lying-down fight, so
that clause became false; the shipped text now discloses only the
surrender gap ("One thing the record describes is not yet shown…").
Test-pinned (`test_iverson_content_warning_override_ships` asserts the
prone claim is gone). If the owner prefers different wording, it is one
JSON edit + version bump.

Cross-phase entry: `ViewpointDefinition.battleAsset` (additive; empty =
set home) — `iverson-forney-field` declares
`gettysburg-july1-afternoon`, so its entry marker, timeline band, and
entry exist only while that phase is loaded (per-phase media honesty,
now per viewpoint). The July 3 viewpoints are bit-untouched by the
gate (no battleAsset = the set's home asset, exactly the old behavior).
PlayMode: `CrossPhaseViewpoint_IsRefusedOffItsOwnPhase`.

## 10. Suites

TBD

## 11. Film-safety verdict

TBD

## 12. Stills index

TBD
