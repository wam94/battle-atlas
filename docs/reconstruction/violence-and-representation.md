# Violence and representation

**Scope:** the Angle reconstruction's Soldier View and the Phase 8 action
scene it is rendered from (plan §9.2). These are the rules the
implementation follows before any graphic content ships. They bind every
later phase; changes here must be reviewed, not slipped in with art.

## Why explicit violence is shown at all

The subject is the killing at the Angle on July 3, 1863. Pickett's division
lost roughly half its men in under an hour. A reconstruction that sanitized
the casualties — lines that thin without anyone falling, smoke without
cause — would misrepresent the event more profoundly than any stylistic
choice could. The locked plan decision (ADR 0001): Soldier View contains
explicit falls, blood, wounds, and persistent bodies, presented soberly.

## The rules

### 1. No procedural casualty is an identified person

Victim selection is a deterministic hash over (battle seed, unit, slot,
casualty profile) — `CasualtySchedule.Compile`. No historical name is ever
attached to a procedural figure or its fate. Named officers whose wounds
are documented (Garnett, Armistead, Cushing, Kemper, Webb) are represented
only at unit level in this phase; if identified figures are added later,
their treatment requires specific sourcing and separate approval (locked
decision: no invented named-person wound details).

### 2. Aggregate evidence produces aggregate representation

Casualty counts, timing windows, intensity curves, and cause mixes come
from the compiled profiles (`angle.bundle.json`), which trace to claims —
mostly regimental/brigade loss totals. The reconstruction distributes those
totals over men and seconds by hash. Inspected anywhere in the product,
this layer must read as **reconstructed**, never documented.

### 3. A limited wound vocabulary, tied to broad cause classes

`CasualtySchedule.WoundCategory` is the complete vocabulary:

| Cause class (profile `causeMix`) | Wound category | Visual treatment |
| --- | --- | --- |
| `musketry` | Ball wound | fall by incoming direction; small dark wound patch (hero tier); modest blood pooling |
| `canister` | Canister strike | violent fall; larger wound patch; larger pooling |
| `shell` | Fragment wound | brace/flinch context; medium patch and pooling |
| `unknown` | Unspecified | the figure falls; **no wound or blood is staged** |

The `unknown` row is deliberate: where the evidence supports only "a man
was lost," we do not invent the manner. There is no dismemberment, no
per-figure gore variation beyond this table, and no wound detail that
outruns the cause class.

### 4. Bodies persist and shape the scene

Fallen figures hold their fall pose to the end of the slice (§6.4). They
are never cleaned up, pooled away, or faded for convenience; formation gaps
open where the schedule put the losses, dropped muskets lie where their
owners fell, and the repulse flows back over the field's accumulated dead.
A hash minority (~22%) of the fallen are wounded rather than dead and drag
themselves — slowly, prone — because casualty totals were mostly wounded
men, and a field of only corpses would be its own distortion.

### 5. No game grammar

- No slow motion, no kill confirmation, no hit markers, no score.
- The camera never rewards a death: no framing changes, no zoom, no
  emphasis when a figure falls.
- Falls, reactions, and crawls play at documentary speed with deterministic
  per-figure variation (rate ±10%, yaw ±9°) so no two deaths read as the
  same canned animation, and none reads as a highlight.
- Audio (Phase 9) follows the same rule: pain and panic are ambient
  human sound, never a reward cue.

### 6. Sober palette and staging

Blood is dark, matte, and ground-bound (pooling under bodies), not spray.
Wound patches are small dark stains on the uniform, sized by cause class
(0.10–0.17 m). The intent is that a viewer at eye level understands men are
being killed, not that the renderer enjoys it.

### 7. Content warning

A clear graphic-content warning precedes first entry into Soldier View
(Phase 11 UI). Until that UI ships, every rendered evidence sequence from
Phase 8 onward is documented as depicting explicit casualties (the P8
viewing guide carries the note).

### 8. Determinism is an ethical property here

Scrubbing to any battle second reconstructs the same living, falling, and
dead men (`SoldierActionResolverTests`, Gate P8 digests). Deaths are part
of the historical record's shape, not a slot-machine effect: replaying the
moment never re-rolls who dies.

## What inspection shows

- Casualty schedule totals equal the compiled profile totals exactly, per
  unit and per profile (EditMode `CasualtyScheduleTests`).
- The alive count reconciles with the bundle's per-second strength curve
  (exact at profile boundaries; within one man mid-window, a documented
  rounding-tie artifact).
- Cause classes are apportioned exactly per the profile `causeMix`.

## Review obligations

Any change that widens the wound vocabulary, attaches identity to a
casualty, adds gore detail, or alters the persistence rules must update
this document first and be called out for owner review at the next gate.
