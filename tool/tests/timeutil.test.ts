import { describe, expect, it } from "vitest";
import { clampDraftTime, nextKeyframeTime } from "../src/timeutil";

describe("nextKeyframeTime", () => {
  it("first keyframe lands at the preview time", () => {
    expect(nextKeyframeTime(120, null, 3600)).toBe(120);
  });
  it("later keyframes never go backwards", () => {
    expect(nextKeyframeTime(100, 200, 3600)).toBe(201);
    expect(nextKeyframeTime(500, 200, 3600)).toBe(500);
  });
  it("returns null when the track already reaches endTime", () => {
    expect(nextKeyframeTime(4000, 3999, 3600)).toBeNull();
    expect(nextKeyframeTime(3600, 3600, 3600)).toBeNull();
  });
  it("first keyframe clamps to endTime instead of refusing", () => {
    expect(nextKeyframeTime(4000, null, 3600)).toBe(3600);
  });
});

describe("clampDraftTime", () => {
  it("bounds both ends", () => {
    expect(clampDraftTime(-5, 3600)).toBe(0);
    expect(clampDraftTime(9999, 3600)).toBe(3600);
    expect(clampDraftTime(50, 3600)).toBe(50);
  });
});
