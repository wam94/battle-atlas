# Soldier View content warning and representative-viewpoint note

**Scope:** the user-facing text shown before first entry into Soldier View
(plan §9.2 guardrail: "add a clear content warning before first entry"),
plus the representative-observer explanation (plan §3.3, §6.5). Authored
in Phase 9; surfaced by the Phase 11 UI, which reads the committed asset
`app/Assets/StreamingAssets/SoldierView/content-warning.json`. The rules
this text implements live in `violence-and-representation.md`; edits to
either file must keep them consistent.

## The warning (shown before first entry, must be acknowledged once)

> **Before you enter Soldier View**
>
> You are about to watch a reconstruction of infantry combat at the Angle
> on July 3, 1863, from inside a Confederate line of battle. It depicts,
> explicitly and without relief: men shot at close range, artillery fire
> striking a formation, blood, the dying and the dead — who remain where
> they fall — and the sounds of battle, including the wounded.
>
> Nothing here is dramatized for effect. Casualties occur where and when
> the reconstruction's evidence places them, and the depiction is held
> deliberately sober. It is still a depiction of mass violence, and it is
> meant to be difficult.
>
> If you prefer not to watch this, the Atlas view presents the same
> events at map scale.

## The representative observer (available from the viewpoint's info panel)

> **Whose eyes are these?**
>
> No one's, and everyone's in that line. The viewpoint walks with
> formation slot 881 of Garnett's Virginia brigade — a rear-rank man on
> the brigade's left, a *representative, unnamed* soldier, not an identified person. No diary, letter, or
> service record places a specific man at this spot; the reconstruction
> does not pretend otherwise.
>
> The men around the camera march, fire, fall, and die according to the
> same aggregate evidence that drives the whole reconstruction — unit
> strengths, loss totals, timing windows — distributed to individual
> figures deterministically. None of them is an identified casualty.
>
> One editorial liberty is disclosed here: the observer himself is
> exempt from the casualty draw, so that the viewpoint can witness its
> full eleven minutes. Garnett's brigade lost roughly half its men in
> that hour; the odds this particular man walked away unhurt were real,
> but they were not good. His survival is a choice we made so you could
> see — not a claim about what happened to anyone.

## Presentation requirements (Phase 11)

- The warning appears before the FIRST entry into any Soldier View
  viewpoint and requires explicit acknowledgment; the acknowledgment
  persists (PlayMode `content-warning persistence` test, plan §13).
- The representative-observer note is reachable from inside the
  viewpoint (info/source drawer) at any time, and its first sentence
  ("representative, unnamed soldier, not an identified person") also
  appears in the viewpoint's entry UI, per §6.5 `editorialNote`.
- No celebratory framing: no scoring, no kill feedback, no slow-motion
  (violence-and-representation.md; plan §9.2).
- Accessibility (Phase 12): the warning text must be readable at the
  smallest supported text size, and the motion-reduction cut
  (`HeroMotionProfile.ReducedMotion`) is offered alongside it.
