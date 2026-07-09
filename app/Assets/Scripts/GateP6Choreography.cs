using UnityEngine;

namespace BattleAtlas
{
    // Gate P6 60-second evidence choreography (plan §12 Phase 6 gate).
    //
    // A PURE deterministic function of (slot, t) — no state, no Random, no
    // Time.* — so scrubbing to any t reconstructs the identical scene
    // (§6.4 discipline; the full SoldierActionResolver arrives in Phase 8,
    // driven by the compiled bundle instead of these staging tables).
    //
    // Staging (crop-local meters, same frame as AngleBakeoffLayout):
    //   * slots 0..63   CSA main line: 2 ranks x 32 files, quick-time east,
    //                    every file crosses the staged rail fence at x=480;
    //   * slots 64..83  USA line behind the wall (x=522), staggered
    //                    aim/fire/RELOAD cycles;
    //   * slot  84      CSA skirmisher 8 m from the camera: two complete
    //                    historically ordered reload cycles (hero evidence);
    //   * slots 85..99  CSA support echelon trailing the main line.
    // Hard-coded casualty/hit/retreat tables below are the deterministic
    // action variation; falls select their clip by incoming direction.
    public static class GateP6Choreography
    {
        public const float Duration = 60f;
        public const int SoldierCount = 100;

        // clip identity (names must exist in every kit FBX)
        public const string March = "March_ShoulderArms";
        public const string StandReady = "Stand_Ready";
        public const string Aim = "Aim_Musket";
        public const string Fire = "Fire_Recoil";
        public const string Reload = "Reload_Musket";
        public const string Cross = "Cross_RailFence";
        public const string Hit = "Hit_Nonfatal";
        public const string FallBack = "Fall_Shot_Front_Back";
        public const string FallCrumple = "Fall_Shot_Front_Crumple";
        public const string FallSide = "Fall_Shot_Left_Side";
        public const string Retreat = "Turn_Retreat";
        public const string Waver = "Waver";

        // authored durations (24 fps sources; harness clamps to clip.length)
        public const float MarchCycle = 26f / 24f;
        public const float StandCycle = 2f;
        public const float AimDur = 34f / 24f;
        public const float FireDur = 22f / 24f;
        public const float ReloadDur = 20f;
        public const float CrossDur = 4f;
        public const float HitDur = 1.25f;
        public const float FallBackDur = 2f;
        public const float FallCrumpleDur = 55f / 24f;
        public const float FallSideDur = 41f / 24f;
        public const float RetreatTurnDur = 1f;   // then walk loop [1..3)
        public const float RetreatLoopEnd = 3f;
        public const float WaverDur = 4f;

        // staging geometry
        public const float FenceX = 480f;         // AngleBakeoffLayout.ApproachFence
        public const float CrossStartX = 479.45f; // clip expects the rail 0.55 ahead
        public const float CrossTravel = 1.3f;    // clip root motion, forward
        public const float MarchSpeed = 1.2f;     // m/s, quick-ish time
        public const float RetreatSpeed = 0.9f;
        public const float HaltX = 510f;          // the advance stalls short
                                                  // of the wall and wavers
        public const float RetreatStopX = 481.5f; // routed men bunch at the
                                                  // fence rather than clip it
        public const float FallRateJitter = 0.10f;   // +-10% playback rate
        public const float FallYawJitterDeg = 9f;    // +- facing variation

        const float LineX0Front = 462f;
        const float LineZ0 = 388f;
        const float FileSpacing = 0.75f;
        const float RankSpacing = 1.5f;
        const float EchelonX0 = 442f;
        const float EchelonZ0 = 390f;
        const float EchelonSpacing = 1.5f;
        const float UsaX = 522f;
        const float UsaZ0 = 388f;
        const float UsaSpacing = 1.7f;
        public static readonly Vector2 SkirmisherPos = new Vector2(466f, 385f);

        // camera (harness reads these; §6.5 eye height/fov)
        public static readonly Vector2 CameraPosXZ = new Vector2(455f, 382f);
        public const float CameraEyeHeight = 1.66f;
        public static readonly Vector2 CameraLookXZ = new Vector2(497f, 403f);
        public const float CameraLookHeight = 1.4f;
        public const float CameraFovDeg = 68f;

        // --- deterministic action variation tables (slot, time, clip) ---
        // Falls choose the clip by incoming direction: musketry/canister
        // from the front (east) throws men backward or crumples them;
        // oblique canister from the left knocks them sideways.
        static readonly (int slot, float t, string clip)[] Falls =
        {
            (2, 24.0f, FallBack), (11, 22.0f, FallCrumple), (19, 26.0f, FallSide),
            (27, 31.0f, FallBack), (30, 34.0f, FallCrumple), (38, 23.5f, FallBack),
            (47, 27.5f, FallCrumple), (55, 30.0f, FallBack), (62, 33.0f, FallSide),
            (33, 36.0f, FallCrumple), (87, 38.0f, FallBack), (91, 48.0f, FallCrumple),
            (95, 52.0f, FallSide), (7, 40.0f, FallBack), (23, 44.0f, FallCrumple),
        };
        static readonly (int slot, float t)[] Hits =
        {
            (5, 8.0f), (20, 26.0f), (44, 12.0f), (58, 31.0f), (89, 42.0f),
        };
        static readonly (int slot, float t)[] Retreats =
        {
            (13, 34.0f), (50, 45.0f), (59, 50.0f),
        };

        public struct SoldierPose
        {
            public Vector2 posLocal;     // crop-local XZ
            public float facingDeg;      // 0 = north (+z), 90 = east (+x)
            public string clip;
            public float clipTime;
            public bool dead;            // fallen (body persists)
            public bool usa;
        }

        public static int VariantIndex(int slot) => slot % 3;
        public static bool IsUsa(int slot) => slot >= 64 && slot <= 83;

        public static SoldierPose Resolve(int slot, float t)
        {
            t = Mathf.Clamp(t, 0f, Duration);
            if (IsUsa(slot)) return ResolveUsa(slot, t);
            if (slot == 84) return ResolveSkirmisher(t);
            return ResolveMarcher(slot, t);
        }

        // ------------------------------------------------------ CSA marchers
        static void MarcherStart(int slot, out float x0, out float z)
        {
            if (slot < 64)
            {
                int rank = slot / 32, file = slot % 32;
                x0 = LineX0Front - rank * RankSpacing;
                z = LineZ0 + file * FileSpacing;
            }
            else
            {
                int file = slot - 85;
                x0 = EchelonX0;
                z = EchelonZ0 + file * EchelonSpacing;
            }
        }

        static bool Fall(int slot, out float t, out string clip)
        {
            foreach (var f in Falls)
                if (f.slot == slot) { t = f.t; clip = f.clip; return true; }
            t = 0f; clip = null; return false;
        }

        static bool HitAt(int slot, out float t)
        {
            foreach (var h in Hits)
                if (h.slot == slot) { t = h.t; return true; }
            t = 0f; return false;
        }

        static bool RetreatAt(int slot, out float t)
        {
            foreach (var r in Retreats)
                if (r.slot == slot) { t = r.t; return true; }
            t = 0f; return false;
        }

        // Piecewise march kinematics. Timeline per slot:
        //   march [pause at hit 1.25 s] march
        //   halt at fence (deterministic stagger), cross (4 s, +1.3 m), march on
        // Fall/retreat interrupt wherever the soldier is (falls that would
        // land inside the crossing window are pushed just past it).
        struct MarchState
        {
            public float x;
            public string clip;
            public float clipTime;
        }

        static MarchState MarchAt(int slot, float t, float x0)
        {
            // per-slot rhythm offset keeps the line in step but alive
            float step = 0.06f * FormationLayout.Jitter("p6-step", slot, 7);
            bool hasHit = HitAt(slot, out float hitT);

            // 1) time spent paused by the nonfatal hit before clock t
            float PauseBefore(float tt) =>
                hasHit ? Mathf.Clamp(tt - hitT, 0f, HitDur) : 0f;

            // 2) fence arrival solved against pre-fence pauses
            float arrive = (CrossStartX - x0) / MarchSpeed;
            if (hasHit && hitT < arrive + HitDur) arrive += HitDur;
            float wait = 0.6f + 0.6f * Mathf.Abs(FormationLayout.Jitter("p6-cross", slot, 13));
            float crossStart = arrive + wait;
            float crossEnd = crossStart + CrossDur;

            var s = new MarchState();
            if (t >= hitT && t < hitT + HitDur && hasHit && t < arrive)
            {
                s.x = x0 + MarchSpeed * (hitT - PauseBefore(hitT));
                s.clip = Hit;
                s.clipTime = t - hitT;
                return s;
            }
            if (t < arrive)
            {
                s.x = x0 + MarchSpeed * (t - PauseBefore(t));
                s.clip = March;
                s.clipTime = (t + step) % MarchCycle;
                return s;
            }
            if (t < crossStart)
            {
                s.x = CrossStartX;
                s.clip = StandReady;
                s.clipTime = (t - arrive) % StandCycle;
                return s;
            }
            if (t < crossEnd)
            {
                // harness holds the transform; the clip's root motion moves
                // the body over the rail
                s.x = CrossStartX;
                s.clip = Cross;
                s.clipTime = t - crossStart;
                return s;
            }
            // beyond the fence
            float hitAfter = hasHit && hitT >= crossEnd ? hitT : float.PositiveInfinity;
            if (t >= hitAfter && t < hitAfter + HitDur)
            {
                s.x = Mathf.Min(HaltX,
                    CrossStartX + CrossTravel + MarchSpeed * (hitAfter - crossEnd));
                s.clip = Hit;
                s.clipTime = t - hitAfter;
                return s;
            }
            float paused = t >= hitAfter ? HitDur : 0f;
            s.x = CrossStartX + CrossTravel + MarchSpeed * (t - crossEnd - paused);
            if (s.x >= HaltX)
            {
                // the advance stalls short of the wall and wavers under fire
                float tHalt = crossEnd + paused
                    + (HaltX - CrossStartX - CrossTravel) / MarchSpeed;
                s.x = HaltX;
                s.clip = Waver;
                s.clipTime = (t - tHalt) % WaverDur;
                return s;
            }
            s.clip = March;
            s.clipTime = (t + step) % MarchCycle;
            return s;
        }

        internal static float CrossWindowStart(int slot, float x0)
        {
            bool hasHit = HitAt(slot, out float hitT);
            float arrive = (CrossStartX - x0) / MarchSpeed;
            if (hasHit && hitT < arrive + HitDur) arrive += HitDur;
            float wait = 0.6f + 0.6f * Mathf.Abs(FormationLayout.Jitter("p6-cross", slot, 13));
            return arrive + wait;
        }

        static SoldierPose ResolveMarcher(int slot, float t)
        {
            MarcherStart(slot, out float x0, out float z);
            var pose = new SoldierPose { facingDeg = 90f, usa = false };

            if (Fall(slot, out float fallT, out string fallClip))
            {
                // never fall mid-crossing: push past the window
                float cs = CrossWindowStart(slot, x0);
                if (fallT > cs - 0.5f && fallT < cs + CrossDur + 0.3f)
                    fallT = cs + CrossDur + 0.3f;
                if (t >= fallT)
                {
                    var at = MarchAt(slot, fallT, x0);
                    float dur = fallClip == FallBack ? FallBackDur
                        : fallClip == FallCrumple ? FallCrumpleDur : FallSideDur;
                    pose.posLocal = new Vector2(at.x, z);
                    pose.clip = fallClip;
                    // deterministic per-soldier variation from the shared
                    // FNV hash so near-simultaneous falls never run in
                    // lockstep: playback rate +-10%, facing +-9 degrees
                    // (small enough to keep the by-incoming-direction
                    // clip choice legible)
                    float rate = 1f + FallRateJitter *
                        FormationLayout.Jitter("p6-fall-rate", slot, 17);
                    pose.facingDeg += FallYawJitterDeg *
                        FormationLayout.Jitter("p6-fall-yaw", slot, 29);
                    pose.clipTime = Mathf.Min((t - fallT) * rate, dur - 1f / 48f);
                    pose.dead = true;
                    return pose;
                }
            }
            if (RetreatAt(slot, out float rt) && t >= rt)
            {
                var at = MarchAt(slot, rt, x0);
                float back = t < rt + RetreatTurnDur ? 0f
                    : RetreatSpeed * (t - rt - RetreatTurnDur);
                float x = at.x - back;
                if (x <= RetreatStopX)
                {
                    // bunched against the fence: no re-crossing clip is
                    // staged, so he goes to ground state there
                    float tStop = rt + RetreatTurnDur + (at.x - RetreatStopX) / RetreatSpeed;
                    pose.posLocal = new Vector2(RetreatStopX, z);
                    pose.clip = Waver;
                    pose.clipTime = (t - tStop) % WaverDur;
                    return pose;
                }
                pose.posLocal = new Vector2(x, z);
                pose.clip = Retreat;
                pose.clipTime = t - rt < RetreatTurnDur
                    ? t - rt
                    : RetreatTurnDur + (t - rt - RetreatTurnDur)
                        % (RetreatLoopEnd - RetreatTurnDur);
                return pose;
            }
            var m = MarchAt(slot, t, x0);
            pose.posLocal = new Vector2(m.x, z);
            pose.clip = m.clip;
            pose.clipTime = m.clipTime;
            return pose;
        }

        // ------------------------------------------------------------- USA
        // aim -> fire -> full muzzle-loading reload -> ready, staggered per
        // file so the wall shows every reload stage simultaneously.
        const float UsaCycle = AimDur + FireDur + ReloadDur + 2f;

        static SoldierPose ResolveUsa(int slot, float t)
        {
            int file = slot - 64;
            float tt = (t + file * 1.9f) % UsaCycle;
            var pose = new SoldierPose
            {
                posLocal = new Vector2(UsaX, UsaZ0 + file * UsaSpacing),
                facingDeg = 270f,
                usa = true,
            };
            if (tt < AimDur) { pose.clip = Aim; pose.clipTime = tt; }
            else if (tt < AimDur + FireDur)
            { pose.clip = Fire; pose.clipTime = tt - AimDur; }
            else if (tt < AimDur + FireDur + ReloadDur)
            { pose.clip = Reload; pose.clipTime = tt - AimDur - FireDur; }
            else
            { pose.clip = StandReady; pose.clipTime = (tt - AimDur - FireDur - ReloadDur) % StandCycle; }
            return pose;
        }

        // ------------------------------------------------- hero skirmisher
        // Two complete reload cycles 8 m from the camera — the Gate P6
        // "historically legible reload" evidence.
        static readonly (float start, string clip, float dur, bool loop)[] SkirmisherScript =
        {
            (0f, StandReady, StandCycle, true),
            (6f, Aim, AimDur, false),
            (7.42f, Fire, FireDur, false),
            (8.33f, Reload, ReloadDur, false),
            (28.33f, Aim, AimDur, false),
            (29.75f, Fire, FireDur, false),
            (30.67f, Reload, ReloadDur, false),
            (50.67f, Aim, AimDur, false),
            (52.08f, Fire, FireDur, false),
            (53.0f, StandReady, StandCycle, true),
        };

        static SoldierPose ResolveSkirmisher(float t)
        {
            var pose = new SoldierPose
            {
                posLocal = SkirmisherPos,
                facingDeg = 90f,
                usa = false,
            };
            int idx = 0;
            for (int i = 0; i < SkirmisherScript.Length; i++)
                if (t >= SkirmisherScript[i].start) idx = i;
            var seg = SkirmisherScript[idx];
            float local = t - seg.start;
            pose.clip = seg.clip;
            pose.clipTime = seg.loop
                ? local % seg.dur
                : Mathf.Min(local, seg.dur - 1f / 48f);
            return pose;
        }
    }
}
