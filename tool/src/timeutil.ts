// Where a new keyframe lands on the time axis: at the preview time if that's
// after the unit's last keyframe, else nudged past the end. Returns null when
// no valid slot exists (the unit's track already reaches endTime) — appending
// then would create equal-t keyframes, which the format forbids. Pure so the
// rule is testable and documented in one place.
export function nextKeyframeTime(
  draftTime: number, lastT: number | null, endTime: number,
): number | null {
  const t = lastT === null ? Math.min(draftTime, endTime) : Math.max(draftTime, lastT + 1);
  if (lastT !== null && (t > endTime || t <= lastT)) return null;
  return t;
}

export function clampDraftTime(draftTime: number, endTime: number): number {
  return Math.min(Math.max(0, draftTime), endTime);
}
