# ADR 0005 — Multi-day model: a phase manifest over per-phase battle files

**Status:** accepted (day-expansion slice 1, 2026-07-12) ·
**Branch:** `day-expansion-1` (owner gate) ·
**Context:** V2 plan §10 (day tabs were always the design);
`docs/reconstruction/audit/authoring-wave-a2.md` §6.4 (the single-window
model's observed failures: per-day facts forced into t=0 strength
surgery; the 16:00 cut slicing attested activity).

## Decision

The battle data model becomes **a phase manifest over per-phase battle
files**, not phases embedded inside one battle file.

1. **The battle file format is unchanged.** A battle file remains what
   `docs/format/battle-format.md` says it is: one clock (`startTime`
   seconds since local midnight of ITS day, `t` seconds from phase
   start, `endTime` the phase duration), ≥1 units, optional events and
   environment. One battle file **is** one phase.
2. **A new document, the battle manifest**
   (`app/Assets/StreamingAssets/Atlas/battle-manifest.json`, schema
   `docs/format/battle-manifest.schema.json`), lists the battle's days
   in date order; each day lists its phases in clock order. A phase is
   either:
   - `status: "reconstructed"` — it MUST reference a battle file
     (`battle`, the filename under `Assets/Battle/`) and MUST echo that
     file's `startTime` and `endTime` (agreement is validator- and
     test-enforced, so the manifest cannot lie about a phase's clock);
     or
   - `status: "not-reconstructed"` — it MUST carry a non-empty `note`
     saying so honestly, and MUST NOT reference a battle file. Empty
     days/phases are structure, never content.
3. **The determinism contract is unchanged and per-phase**: everything
   rendered is a pure function of (phase, one battle clock). Switching
   phases swaps which battle file feeds the clock; it never introduces
   a second time source inside a phase.
4. **July 3 afternoon is the existing file.**
   `gettysburg-july3.json` becomes the manifest's
   `july3-afternoon` phase, `startTime` 46800 (13:00 LMT) unchanged,
   `endTime` widened 10800 → **23340** (sunset 19:29 LMT, ED-31). Its
   internal clock is IDENTICAL to the shipped one — t=8160..8820 is
   still 15:16–15:27 — so the shipped film's viewpoint windows and
   media mapping keep working byte-unchanged.
5. **July 1 and July 2 exist as empty days** (one `not-reconstructed`
   phase each), and July 3 additionally carries an empty
   `july3-morning` phase — the A2 §6.4 worklist's natural home
   (morning pins land as that phase's keyframes when it is authored,
   never as t=0 strength surgery on the afternoon file).
6. **Runtime**: the Atlas HUD reads the manifest and renders day
   navigation; unreconstructed days/phases present an honest
   "not yet reconstructed" state (the manifest's own note), never an
   empty battlefield pretending to be a quiet one. This slice ships
   navigation + honest empty states; battle-file hot-swapping between
   multiple reconstructed phases is deferred until a second
   reconstructed phase exists (the loader path is unchanged until
   then).

## Why not phases inside one battle file

- **The film contract.** The shipped Soldier View media is pinned to
  the July-3 file's clock (`viewpoints.json` t=8160..8820; angle
  bundle slice 8040..9000; stagingSeed ED-21). A format migration that
  rewrites that file's time semantics would put the shipped film's
  ground truth behind a format shim. Keeping "one file = one clock"
  keeps the film's ground truth byte-stable.
- **Authoring-wave isolation.** Every wave so far (W1–6, A1, A2) is a
  reviewable diff against ONE file. Day authoring stays that shape: a
  July-1 wave creates a July-1 file; it cannot accidentally touch
  July 3 (today's Angle-cast guard becomes a file boundary).
- **Scale.** 190 units × 105 events is already a 760 KB file; three
  days in one JSON would degrade both the authoring tool and loader,
  for no runtime gain (the Atlas renders one phase at a time anyway).
- **Honesty by construction.** The schema makes an empty day
  inexpressible without an explicit `not-reconstructed` note, and
  makes a phase clock inexpressible except as an echo of a real
  battle file's clock.

## Consequences

- New format doc + schema: `docs/format/battle-manifest.md`,
  `docs/format/battle-manifest.schema.json`; tool-side
  `validateManifest` (tool/src/validate.ts) with vitest coverage; the
  Unity side parses/validates in `PhaseManifest.FromJson` (throw on
  structural lies, the MomentSet discipline) with EditMode coverage.
- The July-3 widening (10800 → 23340) rides this slice; A2's deferred
  post-16:00 worklist becomes in-window content (see
  `docs/reconstruction/audit/day-expansion-slice-1.md`).
- `moments.json` stays per-phase (it addresses the July-3 afternoon
  clock); a future phase brings its own moments file — manifest
  linkage for that is deferred until a second reconstructed phase
  exists.
- The A2 §6.4 "per-day loss ledgers on the unit" observation is NOT
  taken this slice (it is a battle-format change); recorded as
  residual for slice 2+.
