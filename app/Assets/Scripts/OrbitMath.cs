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
    }
}
