using System;
using UnityEngine;

namespace BattleAtlas
{
    public static class OrbitMath
    {
        public const float MinPitchDeg = 10f;
        public const float MaxPitchDeg = 85f;
        // RTS camera slice: the floor drops from 50 m to 5 m (Total
        // War-style close flight); the ceiling is unchanged.
        public const float MinDistance = 5f;
        public const float MaxDistance = 12000f;

        public static float ClampPitch(float deg) =>
            Mathf.Clamp(deg, MinPitchDeg, MaxPitchDeg);

        public static float ClampDistance(float d) =>
            Mathf.Clamp(d, MinDistance, MaxDistance);

        public static Vector3 CameraPosition(
            Vector3 pivot, float yawDeg, float pitchDeg, float distance)
        {
            var rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);
            return pivot + rot * (Vector3.back * distance);
        }

        // Signed twist (degrees, CCW positive) of a two-finger pair from the
        // previous frame's positions to the current ones. Wraps correctly at ±180.
        public static float TwistDegrees(
            Vector2 prev0, Vector2 prev1, Vector2 curr0, Vector2 curr1)
        {
            float prevAng = Mathf.Atan2(prev1.y - prev0.y, prev1.x - prev0.x) * Mathf.Rad2Deg;
            float currAng = Mathf.Atan2(curr1.y - curr0.y, curr1.x - curr0.x) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(prevAng, currAng);
        }

        // World-space pivot offset for a drag-the-map pan: terrain follows the
        // finger, so the pivot moves opposite the screen delta, rotated by yaw
        // and scaled by zoom distance.
        public static Vector3 PanWorldDelta(
            float yawDeg, Vector2 screenDelta, float distance, float panSpeed)
        {
            Quaternion yawRot = Quaternion.Euler(0f, yawDeg, 0f);
            return -(yawRot * Vector3.right * screenDelta.x
                     + yawRot * Vector3.forward * screenDelta.y)
                   * panSpeed * distance * 0.001f;
        }

        // -------------------------------------------------- RTS camera slice

        // World-space pivot step for a keyboard pan (WASD/arrows): axis is
        // the yaw-relative move direction (x = right/left, y = forward/
        // back, each in [-1, 1]), scaled by real seconds so the rig feels
        // the same at any frame rate, by zoom distance (so a screen-width
        // pan takes the same number of keypresses whether you're at the
        // 5 m floor or theater altitude), and by Shift's fast multiplier.
        public static Vector3 KeyboardPanWorldDelta(
            float yawDeg, Vector2 axis, float distance, float panSpeed,
            float deltaTime, bool fast, float fastMultiplier)
        {
            float speed = panSpeed * distance * (fast ? fastMultiplier : 1f);
            Quaternion yawRot = Quaternion.Euler(0f, yawDeg, 0f);
            return (yawRot * Vector3.right * axis.x + yawRot * Vector3.forward * axis.y)
                   * speed * deltaTime;
        }

        // Progress from "at altitude" (0, unrestricted — at or above the
        // knee) to "at the floor" (1, fully eased — at minDistance), shared
        // by the descent-pitch curve and the dynamic near-clip curve so
        // both ease on the same shape (smoothstep — no jerk at the knee).
        static float DescentProgress(float distance, float minDistance, float kneeDistance)
        {
            if (distance >= kneeDistance) return 0f;
            float t = Mathf.InverseLerp(minDistance, kneeDistance, distance);
            return 1f - Mathf.SmoothStep(0f, 1f, t);
        }

        // The allowed pitch CEILING at the given zoom distance: unrestricted
        // (maxPitchAtAltitude) at or above kneeDistance, easing down to
        // maxPitchAtFloor as distance nears minDistance — Total War's
        // "zooming in tilts you toward the horizon." The floor (never
        // straight down past the horizon) is the fixed MinPitchDeg.
        public static float DescentMaxPitchDeg(
            float distance, float minDistance, float kneeDistance,
            float maxPitchAtAltitude, float maxPitchAtFloor)
        {
            float p = DescentProgress(distance, minDistance, kneeDistance);
            return Mathf.Lerp(maxPitchAtAltitude, maxPitchAtFloor, p);
        }

        // Pitch clamped to both the fixed horizon floor and the
        // descent-eased ceiling — user pitch input still works freely
        // inside whatever window the current zoom distance allows.
        public static float ClampPitchForDistance(
            float pitchDeg, float distance, float minDistance, float kneeDistance,
            float maxPitchAtAltitude, float maxPitchAtFloor)
        {
            float ceiling = DescentMaxPitchDeg(
                distance, minDistance, kneeDistance, maxPitchAtAltitude, maxPitchAtFloor);
            return Mathf.Clamp(pitchDeg, MinPitchDeg, ceiling);
        }

        // Near-clip plane at the given zoom distance: nearAtAltitude at or
        // above the knee, easing toward nearAtFloor at minDistance — depth
        // precision (reversed-Z, 20 km far plane) holds at theater range,
        // and close flight over the 5 m floor doesn't clip the ground.
        // Same ease shape as the descent-pitch curve (DescentProgress).
        public static float DynamicNearClip(
            float distance, float minDistance, float kneeDistance,
            float nearAtAltitude, float nearAtFloor)
        {
            float p = DescentProgress(distance, minDistance, kneeDistance);
            return Mathf.Lerp(nearAtAltitude, nearAtFloor, p);
        }

        // Lifts a candidate camera position straight up so it never dips
        // within minClearance of the terrain sampled under it — the rig
        // can graze a slope but never clip through it. groundY is the SAME
        // sampler the pivot and the click-picker use, so clearance agrees
        // with the displayed terrain.
        public static Vector3 ResolveTerrainClearance(
            Vector3 camPos, Func<float, float, float> groundY, float minClearance)
        {
            float floor = groundY(camPos.x, camPos.z) + minClearance;
            if (camPos.y < floor) camPos.y = floor;
            return camPos;
        }

        // Clamps a pivot's XZ to the battlefield rectangle (terrain
        // origin..origin+size) — panning/following can't walk the camera
        // off the edge of the map.
        public static Vector3 ClampPivotToBounds(Vector3 pivot, Vector2 minXZ, Vector2 maxXZ)
        {
            pivot.x = Mathf.Clamp(pivot.x, minXZ.x, maxXZ.x);
            pivot.z = Mathf.Clamp(pivot.z, minXZ.y, maxXZ.y);
            return pivot;
        }

        // Is a viewport point (0..1, Camera.ScreenToViewportPoint's frame)
        // actually over the rendered viewport? Off-viewport cursors
        // (multi-monitor overflow, a stale cached position) can't anchor a
        // cursor-relative zoom.
        public static bool CursorInViewport(Vector2 viewportPoint01) =>
            viewportPoint01.x >= 0f && viewportPoint01.x <= 1f
            && viewportPoint01.y >= 0f && viewportPoint01.y <= 1f;

        // The ground point a cursor-anchored zoom should pull the pivot
        // toward: the cursor's terrain contact when the cursor is over the
        // viewport and its ray finds ground within maxDist, else
        // pivotFallback (off-viewport cursor, or a ray that sails past the
        // terrain edge into the sky) — the pivot-anchored fallback.
        public static Vector3 ZoomAnchorPoint(
            Vector2 viewportPoint01, Vector3 rayOrigin, Vector3 rayDir,
            Func<float, float, float> groundY, float maxDist, Vector3 pivotFallback)
        {
            if (!CursorInViewport(viewportPoint01)) return pivotFallback;
            if (!UnitPicker.RaycastTerrain(rayOrigin, rayDir, groundY, maxDist, out Vector3 hit))
                return pivotFallback;
            return hit;
        }

        // Blends the pivot toward the zoom anchor by the fraction distance
        // just shrank (zoomFrac = 0 leaves the pivot untouched — used when
        // zooming OUT, so pulling back doesn't chase the cursor toward an
        // arbitrary ground point).
        public static Vector3 ZoomAnchorPivot(Vector3 pivot, Vector3 anchor, float zoomFrac) =>
            Vector3.Lerp(pivot, anchor, Mathf.Clamp01(zoomFrac));
    }
}
