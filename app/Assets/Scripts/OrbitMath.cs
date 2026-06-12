using UnityEngine;

namespace BattleAtlas
{
    public static class OrbitMath
    {
        public const float MinPitchDeg = 10f;
        public const float MaxPitchDeg = 85f;
        public const float MinDistance = 50f;
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
    }
}
