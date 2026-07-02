using UnityEngine;

namespace BattleAtlas
{
    // Drives the scene's directional light from the battle clock through
    // SunEphemeris every frame (two float lerps — free). Both modes are
    // display-honesty features: the ephemeris sun IS the record (computed,
    // source logged in the research doc); the ReadingLight toggle is a fixed
    // NW raking light — the cartographic hillshade standard — for when the
    // honest 49-69° afternoon sun flattens Gettysburg's relief like a noon
    // aerial photo. It is presentation and says so on its HUD chip
    // ("Reading light (presentation)"), the same labeled-not-smuggled
    // doctrine as HeightmapImporter.VerticalExaggeration's 2.5×.
    public class SunDirector : MonoBehaviour
    {
        public BattleClock clock;
        public Light sun;

        // Presentation toggle, driven by the TimelineHud chip.
        public bool ReadingLight;

        // NW raking light: azimuth 315°, elevation 32° (the ~30-35° band
        // hillshade cartography standardized on for relief legibility).
        const float ReadingAzimuthDeg = 315f;
        const float ReadingElevationDeg = 32f;

        // Warm/dim ramp keyed on sun elevation (physical: a lower sun crosses
        // more air). At 13:00 (el 69.4°) the light keeps the scene's authored
        // white/intensity; by 16:00 (el 37.9°) it has warmed and dimmed
        // slightly. Elevation-keyed (not time-keyed) so the ramp stays
        // sensible if the table ever grows past 16:00.
        static readonly Color NoonColor = Color.white;
        static readonly Color LateColor = new Color(1f, 0.89f, 0.76f);
        const float NoonElevationDeg = 69.4f; // 13:00 table key
        const float LateElevationDeg = 37.9f; // 16:00 table key
        const float LateIntensityFactor = 0.88f;

        float baseIntensity;

        void Start()
        {
            // the component lives on the light itself; tolerate an unset
            // reference the same way BattleDirector tolerates a lost terrain
            if (sun == null)
            {
                sun = GetComponent<Light>();
                Debug.LogWarning("SunDirector.sun was unset; using the local Light component");
            }
            baseIntensity = sun.intensity;
        }

        void Update()
        {
            if (ReadingLight)
            {
                sun.transform.rotation = SunEphemeris.LightRotation(
                    ReadingElevationDeg, ReadingAzimuthDeg);
                sun.color = NoonColor;
                sun.intensity = baseIntensity;
                return;
            }

            var (elevation, azimuth) = SunEphemeris.SunAngles(
                clock.StartTime + clock.CurrentTime);
            sun.transform.rotation = SunEphemeris.LightRotation(elevation, azimuth);
            float lateness = Mathf.InverseLerp(NoonElevationDeg, LateElevationDeg, elevation);
            sun.color = Color.Lerp(NoonColor, LateColor, lateness);
            sun.intensity = baseIntensity * Mathf.Lerp(1f, LateIntensityFactor, lateness);
        }
    }
}
