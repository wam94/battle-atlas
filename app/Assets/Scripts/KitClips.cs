using System;

namespace BattleAtlas
{
    // The Phase 6 character-kit animation vocabulary (§7.3), addressed by
    // stable integer ids so resolved soldier states are compact and
    // bitwise-comparable. Names must match the actions inside every kit
    // FBX (characters/kit/clips.py authors them; the P6 render harness
    // indexes clips by the "<rig>|<action>" suffix).
    public enum ClipId : byte
    {
        StandReady = 0,
        March = 1,
        RouteStep = 2,
        DoubleQuick = 3,
        RoutedRun = 4,
        Aim = 5,
        Fire = 6,
        Reload = 7,
        Cross = 8,
        HitNonfatal = 9,
        FallBack = 10,
        FallCrumple = 11,
        FallSide = 12,
        TurnRetreat = 13,
        Waver = 14,
        KneelReady = 15,
        Brace = 16,
        Flinch = 17,
        HaltDress = 18,
        ProneCrawl = 19,
    }

    public static class KitClips
    {
        public const int Count = 20;

        static readonly string[] Names =
        {
            "Stand_Ready", "March_ShoulderArms", "RouteStep_Advance",
            "DoubleQuick", "Routed_Run", "Aim_Musket", "Fire_Recoil",
            "Reload_Musket", "Cross_RailFence", "Hit_Nonfatal",
            "Fall_Shot_Front_Back", "Fall_Shot_Front_Crumple",
            "Fall_Shot_Left_Side", "Turn_Retreat", "Waver", "Kneel_Ready",
            "Brace_Artillery", "Flinch_Recover", "Halt_DressLine",
            "Prone_Wounded_Crawl",
        };

        // Authored durations in seconds (24 fps sources; loop clips list the
        // loop cycle). Render harnesses clamp to the imported clip.length,
        // so these only need to be correct for phase math.
        static readonly float[] Durations =
        {
            2f,            // StandReady (loop)
            26f / 24f,     // March (loop)
            30f / 24f,     // RouteStep (loop)
            18f / 24f,     // DoubleQuick (loop)
            16f / 24f,     // RoutedRun (loop)
            34f / 24f,     // Aim
            22f / 24f,     // Fire
            20f,           // Reload (nine historically ordered stages)
            4f,            // Cross
            1.25f,         // HitNonfatal
            2f,            // FallBack
            55f / 24f,     // FallCrumple
            41f / 24f,     // FallSide
            3f,            // TurnRetreat (turn 0..1, walk loop 1..3)
            4f,            // Waver (loop)
            1.5f,          // KneelReady
            2f,            // Brace
            0.85f,         // Flinch
            2.4f,          // HaltDress
            1.6f,          // ProneCrawl (loop)
        };

        static readonly bool[] Loops =
        {
            true, true, true, true, true,     // stand..routed run
            false, false, false, false, false, // aim..hit
            false, false, false, false, true,  // falls, retreat, waver
            false, false, false, false, true,  // kneel..crawl
        };

        public static string Name(ClipId id) => Names[(int)id];
        public static float Duration(ClipId id) => Durations[(int)id];
        public static bool IsLoop(ClipId id) => Loops[(int)id];

        // Loop-or-clamp phase for a clip started at t0, evaluated at t.
        public static float Phase(ClipId id, float sinceStart)
        {
            float d = Durations[(int)id];
            if (Loops[(int)id])
                return sinceStart - d * (float)Math.Floor(sinceStart / d);
            return Math.Min(Math.Max(sinceStart, 0f), d - 1f / 48f);
        }

        // Ground meters one loop of a locomotion clip covers (drill-manual
        // paces: quick time 110 x 28 in, double-quick 165 x 33 in). The P8
        // locomotion review fix keys stride PHASE to track DISTANCE through
        // these, so stride rate always matches ground speed (no skating,
        // no treadmill marching).
        public static float MetersPerCycle(ClipId id) => id switch
        {
            ClipId.March => 1.42f,
            ClipId.RouteStep => 1.30f,
            ClipId.DoubleQuick => 1.68f,
            ClipId.RoutedRun => 1.90f,
            _ => 0f,
        };

        // Distance-driven loop phase: clip time as a function of meters
        // traveled along the track (pure in its inputs).
        public static float DistancePhase(ClipId id, float meters)
        {
            float mpc = MetersPerCycle(id);
            if (mpc <= 0f) return 0f;
            float cycles = meters / mpc;
            float frac = cycles - (float)Math.Floor(cycles);
            return frac * Durations[(int)id];
        }
    }
}
