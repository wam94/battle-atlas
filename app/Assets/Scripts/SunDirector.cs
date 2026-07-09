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
    //
    // HDRP (Phase 4): the Light itself carries physical units (lux; see the
    // Atlas scene's HDAdditionalLightData) and the same elevation-keyed ramp
    // scales it. Additionally, SunDirector publishes the deterministic sun
    // GLOBALS the custom BattleAtlas shaders read (_BattleSunDirWS,
    // _BattleSunColor): those shaders bypass HDRP's light loop/exposure and
    // want the pre-HDRP normalized intensity (ShaderSunIntensity, the URP
    // scene's authored 2.0), NOT lux — see SoldierFigure.shader's header.
    public class SunDirector : MonoBehaviour
    {
        public BattleClock clock;
        public Light sun;

        // Presentation toggle, driven by the TimelineHud chip.
        public bool ReadingLight;

        // Normalized sun intensity for the custom vertex-tint/flag shaders
        // (the pre-HDRP Atlas light's authored value). Serialized so the
        // scene stays the single source of the authored look.
        public float ShaderSunIntensity = 2f;

        static readonly int BattleSunDirWSId = Shader.PropertyToID("_BattleSunDirWS");
        static readonly int BattleSunColorId = Shader.PropertyToID("_BattleSunColor");

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
                PublishShaderGlobals(1f);
                return;
            }

            var (elevation, azimuth) = SunEphemeris.SunAngles(
                clock.StartTime + clock.CurrentTime);
            sun.transform.rotation = SunEphemeris.LightRotation(elevation, azimuth);
            float lateness = Mathf.InverseLerp(NoonElevationDeg, LateElevationDeg, elevation);
            sun.color = Color.Lerp(NoonColor, LateColor, lateness);
            float rampFactor = Mathf.Lerp(1f, LateIntensityFactor, lateness);
            sun.intensity = baseIntensity * rampFactor;
            PublishShaderGlobals(rampFactor);
        }

        // The custom-shader sun contract: everything derives from the battle
        // clock (via the light this component just posed), so scrubbing
        // replays identical shading. Direction points TOWARD the sun.
        void PublishShaderGlobals(float rampFactor)
        {
            Shader.SetGlobalVector(BattleSunDirWSId, -sun.transform.forward);
            Color c = sun.color * (ShaderSunIntensity * rampFactor);
            Shader.SetGlobalVector(BattleSunColorId, new Vector4(c.r, c.g, c.b, 1f));
        }
    }
}
