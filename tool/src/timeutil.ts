// Where a new keyframe lands on the time axis: at the preview time if that's
// after the unit's last keyframe, else nudged past the end. Pure so the rule
// is testable and documented in one place.
export function nextKeyframeTime(draftTime: number, lastT: number | null, endTime: number): number {
  const t = lastT === null ? draftTime : Math.max(draftTime, lastT + 1);
  return Math.min(t, endTime);
}

export function clampDraftTime(draftTime: number, endTime: number): number {
  return Math.min(Math.max(0, draftTime), endTime);
}
