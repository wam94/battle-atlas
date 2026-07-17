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
        // Fight-prone vocabulary (Iverson slice, claim-iv-lying-down:
        // "my line of battle still lying down in position")
        GoProne = 20,
        ProneIdle = 21,
        FightProneFire = 22,
        FightProneReload = 23,
        RiseFromProne = 24,
        ProneHitSettle = 25,
        // Angle-v2 vocabulary (P3 melee — Peyton: "hand to hand, and of
        // the most desperate character"; contact abstracted, sober per
        // violence-and-representation.md §5/§6)
        MeleeClubSwing = 26,
        MeleeBayonetThrust = 27,
        MeleeGrappleA = 28,
        MeleeGrappleB = 29,
        MeleeParry = 30,
        // Angle-v2 vocabulary (P4 colors — Shepard: "Every flag in the
        // brigade excepting one was captured at or within the works";
        // the staff/flag itself is a scene prop, not a kit prop bone)
        ColorsCarry = 31,
        ColorsBearerFall = 32,
        ColorsPickup = 33,
        // Angle-v2 vocabulary (P5 mounted officer — owner ruling
        // 2026-07-15 "ship p5 mounted officers falling"; the horse rig is
        // separate, HorseClips — these are the RIDER's clips)
        RideSeat = 34,
        RiderFall = 35,
    }

    public static class KitClips
    {
        public const int Count = 36;

        static readonly string[] Names =
        {
            "Stand_Ready", "March_ShoulderArms", "RouteStep_Advance",
            "DoubleQuick", "Routed_Run", "Aim_Musket", "Fire_Recoil",
            "Reload_Musket", "Cross_RailFence", "Hit_Nonfatal",
            "Fall_Shot_Front_Back", "Fall_Shot_Front_Crumple",
            "Fall_Shot_Left_Side", "Turn_Retreat", "Waver", "Kneel_Ready",
            "Brace_Artillery", "Flinch_Recover", "Halt_DressLine",
            "Prone_Wounded_Crawl", "Go_Prone", "Prone_Idle",
            "Fight_Prone_Fire", "Fight_Prone_Reload", "Rise_From_Prone",
            "Prone_Hit_Settle", "Melee_Club_Swing", "Melee_Bayonet_Thrust",
            "Melee_Grapple_A", "Melee_Grapple_B", "Melee_Parry",
            "Colors_Carry", "Colors_Bearer_Fall", "Colors_Pickup",
            "Ride_Seat", "Rider_Fall",
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
            1.8f,          // GoProne
            3f,            // ProneIdle (loop)
            2.4f,          // FightProneFire (discharge at 1.5)
            26f,           // FightProneReload (roll-to-load, 9 stages)
            2.2f,          // RiseFromProne
            1.8f,          // ProneHitSettle
            2.6f,          // MeleeClubSwing (contact abstracted at 1.3)
            2.0f,          // MeleeBayonetThrust (contact abstracted at 0.9)
            2.8f,          // MeleeGrappleA (loop, paired with B)
            2.8f,          // MeleeGrappleB (loop, anti-phase of A)
            2.0f,          // MeleeParry (catch at 0.6)
            2.0f,          // ColorsCarry (loop)
            2.2f,          // ColorsBearerFall (staff released at 0.9)
            2.0f,          // ColorsPickup (grasp at 0.9)
            2.4f,          // RideSeat (loop, root at saddle height)
            2.4f,          // RiderFall (leaves the saddle at 0.55)
        };

        static readonly bool[] Loops =
        {
            true, true, true, true, true,     // stand..routed run
            false, false, false, false, false, // aim..hit
            false, false, false, false, true,  // falls, retreat, waver
            false, false, false, false, true,  // kneel..crawl
            false, true, false, false, false,  // go-prone..rise
            false,                             // prone hit settle
            false, false, true, true, false,   // melee (grapples loop)
            true, false, false,                // colors (carry loops)
            true, false,                       // ride seat loops; fall no
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
