using UnityEngine;

namespace BattleAtlas
{
    // Pure presentation math for the Phase 11 retained-mode Atlas HUD
    // (plan §10 minimum UI). Everything the UI Toolkit controller
    // (AtlasHud) displays that could lie lives here as static functions so
    // the EditMode suite can pin it without a panel.
    public static class HudModel
    {
        // Plan §10: play/pause and speeds 1x, 10x, 60x. (The IMGUI
        // placeholder's 300x convenience speed retires with it: 60x already
        // crosses the 3-hour slice in three minutes.)
        public static readonly float[] Speeds = { 1f, 10f, 60f };

        // Timeline fraction for the scrubber and marker layout: t as a
        // 0..1 position along [0, endTime], clamped.
        public static float TimelineFraction(float t, float endTime)
        {
            if (endTime <= 0f) return 0f;
            return Mathf.Clamp01(t / endTime);
        }

        // Day/slice context line from the battle name: the display name up
        // to the first parenthetical qualifier — "Pickett's Charge — July 3,
        // 1863 (brigade level, reconstruction)" reads as the title without
        // losing the qualifier from the data (the drawer keeps honesty; the
        // masthead keeps legibility).
        public static string DayContext(string battleName)
        {
            if (string.IsNullOrEmpty(battleName)) return "";
            int cut = battleName.IndexOf('(');
            return (cut < 0 ? battleName : battleName.Substring(0, cut)).Trim();
        }

        // Conditions readout (plan: "can stay minimal"): wind as a
        // compass-word drift with its provenance word. windTowardDeg is the
        // bearing smoke drifts TOWARD (battle-format.md "Environment").
        public static string ConditionsLine(EnvironmentDto env)
        {
            if (env == null || env.windMps <= 0f) return "calm";
            string conf = string.IsNullOrEmpty(env.confidence)
                ? "unknown" : env.confidence;
            return $"wind {env.windMps:0.#} m/s toward {CompassWord(env.windTowardDeg)}"
                + $" ({conf})";
        }

        // Eight-point compass word for a bearing in degrees (0 = north).
        public static string CompassWord(float bearingDeg)
        {
            string[] words = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int i = Mathf.RoundToInt(Mathf.Repeat(bearingDeg, 360f) / 45f) % 8;
            return words[i];
        }

        // A viewpoint's entry marker exists ONLY while the battle clock sits
        // inside its window (plan §10) — and never for development
        // viewpoints (the Phase 1 media-contract fixture is a test rig, not
        // a product surface).
        public static bool EntryMarkerVisible(ViewpointDefinition vp, double battleTime)
        {
            if (vp == null || vp.development) return false;
            return SoldierViewMath.WithinWindow(battleTime, vp.t0, vp.t1);
        }

        // The Soldier View window band drawn on the timeline so a new user
        // can FIND the hero viewpoint (Gate P11: "locate Pickett's Charge
        // ... enter the hero viewpoint during its valid window"): left edge
        // and width as timeline fractions.
        public static (float left, float width) WindowBand(
            double t0, double t1, float endTime)
        {
            float a = TimelineFraction((float)t0, endTime);
            float b = TimelineFraction((float)t1, endTime);
            return (a, Mathf.Max(0f, b - a));
        }

        // Soldier View play-speed designator (Gate P11 punchlist, assigned
        // to Phase 12): entering Soldier View forces 1× — the media is
        // pre-rendered real time — but the Atlas bar's speed buttons are
        // hidden inside, so the forced state was invisible. The designator
        // says the speed AND why it cannot change, and carries the paused
        // state so a still frame reads as "paused", not "broken".
        public static string SoldierViewSpeedLabel(bool playing)
            => playing ? "1× real time" : "1× real time — paused";

        // Seek-settle indicator policy (Gate P10 measurement: median 33.9 ms,
        // worst 107 ms on this hardware): a hold this short reads as a held
        // frame, not a defect, so the UI shows nothing for it. Only a stall
        // beyond this threshold (a slow disk, a cold decoder) surfaces the
        // "settling" badge. Decision documented in
        // docs/reconstruction/p11-gate-evidence.md.
        public const double SettleIndicatorAfterSeconds = 0.15;

        public static bool ShowSettleIndicator(bool seekInProgress, double elapsedSeconds)
            => seekInProgress && elapsedSeconds > SettleIndicatorAfterSeconds;
    }
}
