using UnityEngine;

namespace BattleAtlas
{
    // Sun position over Gettysburg on 1863-07-03, computed with the
    // NOAA/Meeus solar position algorithm for lat 39.81°N lon 77.23°W
    // (docs/research/2026-07-02-descriptive-graphics-techniques.md §0).
    // Time basis is Local Mean Time — 1863 predates US standard time
    // (1883) — which is the SAME axis as the battle format's startTime
    // (seconds since local midnight), so BattleClock.StartTime +
    // CurrentTime feeds straight in with no zone conversion.
    public static class SunEphemeris
    {
        // (seconds since local midnight, elevation°, compass azimuth°).
        // Azimuth is compass convention (0 = north, 90 = east) — the same
        // mapping to Unity yaw as the battle format's facingDeg.
        static readonly (float t, float elevationDeg, float azimuthDeg)[] Table =
        {
            (12.0f * 3600f, 73.2f, 177.0f),
            (13.0f * 3600f, 69.4f, 219.4f), // bombardment begins ~13:00
            (13.5f * 3600f, 65.2f, 233.7f),
            (14.0f * 3600f, 60.3f, 244.3f), // infantry advance ~14:00-15:00
            (14.5f * 3600f, 54.9f, 252.4f),
            (15.0f * 3600f, 49.3f, 259.0f),
            (15.5f * 3600f, 43.6f, 264.7f),
            (16.0f * 3600f, 37.9f, 269.7f), // sun due west
            (17.0f * 3600f, 26.4f, 278.8f),
        };

        // Piecewise-linear interpolation between table keys; clamped to the
        // first/last entry outside the table (the battle window is 13:00-16:00,
        // so the clamp only guards degenerate scrubs, not real play).
        public static (float elevationDeg, float azimuthDeg) SunAngles(
            float secondsSinceMidnight)
        {
            var first = Table[0];
            if (secondsSinceMidnight <= first.t)
                return (first.elevationDeg, first.azimuthDeg);
            var last = Table[Table.Length - 1];
            if (secondsSinceMidnight >= last.t)
                return (last.elevationDeg, last.azimuthDeg);

            for (int i = 1; i < Table.Length; i++)
            {
                if (secondsSinceMidnight > Table[i].t) continue;
                var a = Table[i - 1];
                var b = Table[i];
                float f = Mathf.InverseLerp(a.t, b.t, secondsSinceMidnight);
                return (Mathf.Lerp(a.elevationDeg, b.elevationDeg, f),
                        Mathf.Lerp(a.azimuthDeg, b.azimuthDeg, f));
            }
            return (last.elevationDeg, last.azimuthDeg); // unreachable
        }

        // A directional light points FROM the sun TOWARD the ground: pitch
        // down by the sun's elevation, yaw to azimuth + 180°.
        public static Quaternion LightRotation(float elevationDeg, float azimuthDeg) =>
            Quaternion.Euler(elevationDeg, azimuthDeg + 180f, 0f);
    }
}
