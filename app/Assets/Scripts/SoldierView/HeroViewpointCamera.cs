using System;
using UnityEngine;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // Phase 9 hero viewpoint camera (plan §12 Phase 9, §6.5).
    //
    // The camera rides a deterministic formation slot of Garnett's
    // brigade through the SAME resolver pipeline as every rendered
    // figure: its base path is the slot's resolved position (formation
    // blending, obstacle-crossing holds, catch-up blends and all),
    // low-pass filtered with a symmetric window so the result is
    // scrub-invariant; its look direction is the slot's smoothed facing;
    // its high-frequency life (gait bob, sway, brace/duck on nearby
    // strikes, chaos handheld) is a pure function of battle time and the
    // same compiled event streams that drive the visuals. No state is
    // kept between calls — Pose(t) is deterministic and identical in any
    // scrub order, exactly like SoldierActionResolver.Resolve.
    //
    // Motion-comfort rule (§12 P9: "without inducing motion sickness"):
    // every amplitude below is an authored constant, kept conservative,
    // scaled DOWN by the viewpoint's stabilization parameter (§6.5,
    // 0.35 for garnett-road-to-angle), and the whole set has a
    // ReducedMotion variant for the Phase 12 accessibility cut.
    // ------------------------------------------------------------------

    // Camera pose in macro battlefield coordinates. The render harness
    // adds terrain height: worldY = ground(groundRefX, groundRefZ) +
    // eyeAboveGroundM. Ground is referenced at the OBSERVER, not under
    // the camera, so a close-third-person rig stays anchored to the man.
    public struct HeroCameraPose
    {
        public float camX, camZ;          // camera plan position (macro m)
        public float groundRefX, groundRefZ; // where to sample terrain
        public float eyeAboveGroundM;     // eye height + bob + duck (+ rig)
        public float headingDeg;          // look yaw (0 = +z north)
        public float pitchDeg;            // positive = looking down
        public float rollDeg;
        public float fovDeg;
        public float obsX, obsZ;          // the observer slot's smoothed pos
        public float chaos01;             // diagnostic: chaos level driving shake
    }

    // Authored motion constants (all meters/degrees at stabilization 0).
    // Standard is the shipping profile; ReducedMotion is the Phase 12
    // accessibility variant (motion-reduction cut): no roll, minimal bob,
    // no handheld noise — the path and events remain identical.
    [Serializable]
    public struct HeroMotionProfile
    {
        public float bobAmpM;         // vertical gait bob
        public float swayAmpM;        // lateral gait sway
        public float swayYawDeg;      // gait yaw oscillation
        public float rollAmpDeg;      // gait roll oscillation
        public float handheldDeg;     // baseline handheld noise (yaw/pitch)
        public float chaosShakeDeg;   // additional shake at full chaos
        public float duckMaxM;        // brace/duck depth on a near strike
        public float duckPitchDeg;    // pitch jolt at full duck
        public float headTurnDeg;     // glance toward a nearby fallen man
        public float crossPitchDeg;   // look-down while climbing a fence
        public float crossLiftM;      // step-up lift at mid fence-climb

        public static readonly HeroMotionProfile Standard = new HeroMotionProfile
        {
            bobAmpM = 0.032f,
            swayAmpM = 0.020f,
            swayYawDeg = 0.9f,
            rollAmpDeg = 0.55f,
            handheldDeg = 0.35f,
            chaosShakeDeg = 1.6f,
            duckMaxM = 0.09f,
            duckPitchDeg = 5f,
            headTurnDeg = 10f,
            crossPitchDeg = 9f,
            crossLiftM = 0.22f,
        };

        public static readonly HeroMotionProfile ReducedMotion = new HeroMotionProfile
        {
            bobAmpM = 0.008f,
            swayAmpM = 0.005f,
            swayYawDeg = 0.2f,
            rollAmpDeg = 0f,
            handheldDeg = 0f,
            chaosShakeDeg = 0.3f,
            duckMaxM = 0.03f,
            duckPitchDeg = 1.5f,
            headTurnDeg = 5f,
            crossPitchDeg = 6f,
            crossLiftM = 0.09f,
        };
    }

    public struct HeroCameraSettings
    {
        public string unitId;
        public int slot;
        public float eyeHeightM;      // §6.5: 1.66
        public float fovDeg;          // §6.5: 68
        public float lookYawOffsetDeg;
        public float lookPitchOffsetDeg;
        public float stabilization;   // §6.5: 0.35 (0 raw .. 1 locked-off)
        public bool thirdPerson;      // close third person (over-shoulder)
        public HeroMotionProfile profile;

        public static HeroCameraSettings FromViewpoint(
            ViewpointDefinition vp, bool thirdPerson, HeroMotionProfile profile)
        {
            return new HeroCameraSettings
            {
                unitId = vp.unitId,
                slot = vp.slotId,
                eyeHeightM = vp.camera.eyeHeightM,
                fovDeg = vp.camera.fovDeg,
                lookYawOffsetDeg = vp.camera.lookOffsetDeg[0],
                lookPitchOffsetDeg = vp.camera.lookOffsetDeg[1],
                stabilization = vp.camera.stabilization,
                thirdPerson = thirdPerson,
                profile = profile,
            };
        }
    }

    public static class HeroViewpointCamera
    {
        // --- path/heading smoothing (symmetric => scrub-invariant) ---
        // The ±2.6 s position window exists for comfort: the resolver's
        // fence-crossing hold + catch-up gives the raw slot path short
        // ~6-8 m/s bursts (fine on a rendered figure, nauseating on the
        // camera); a window comparable to the burst length glides the
        // eye through at under a run.
        const int PosTaps = 13;            // samples each side
        const float PosDt = 0.2f;          // window: ±2.6 s
        const int HeadTaps = 8;
        const float HeadDt = 0.25f;        // window: ±2.0 s

        // --- event-response tuning ---
        const float StrikeWindowS = 1.2f;  // duck envelope length
        const float StrikeRadiusM = 25f;
        const float FallGlanceRadiusM = 5f;
        const float FallGlanceWindowS = 2.5f;
        const float ChaosStrikeRadiusM = 30f;
        const float ChaosStrikeWindowS = 12f;
        const float ChaosFallRadiusM = 12f;
        const float ChaosFallWindowS = 10f;

        // --- close-third-person rig (over the right shoulder) ---
        const float C3pBackM = 1.35f;
        const float C3pRightM = 0.42f;
        const float C3pUpM = 0.35f;
        const float C3pBobScale = 0.45f;

        // Handheld/chaos noise band (Hz) — deterministic value noise.
        const float NoiseHz = 5f;

        public static HeroCameraPose Pose(
            AngleActionContext ctx, HeroCameraSettings s, float t)
        {
            var ur = ctx.Unit(s.unitId);
            int u = ur.unitIndex;
            float damp = 1f - Mathf.Clamp01(s.stabilization);

            // 1) smoothed base path (triangular window over the slot's
            //    fully resolved position — crossing holds included)
            Vector2 pos = Vector2.zero;
            float wSum = 0f;
            for (int k = -PosTaps; k <= PosTaps; k++)
            {
                float w = PosTaps + 1 - Mathf.Abs(k);
                var st = SoldierActionResolver.Resolve(ctx, u, s.slot, t + k * PosDt);
                pos += new Vector2(st.posX, st.posZ) * w;
                wSum += w;
            }
            pos /= wSum;

            // 2) smoothed heading (circular mean of resolved facing)
            Vector2 dir = Vector2.zero;
            for (int k = -HeadTaps; k <= HeadTaps; k++)
            {
                float w = HeadTaps + 1 - Mathf.Abs(k);
                var st = SoldierActionResolver.Resolve(ctx, u, s.slot, t + k * HeadDt);
                float r = st.facingDeg * Mathf.Deg2Rad;
                dir += new Vector2(Mathf.Sin(r), Mathf.Cos(r)) * w;
            }
            float heading = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

            // 3) state at t proper: gait phase, crossing, locomotion fade
            var now = SoldierActionResolver.Resolve(ctx, u, s.slot, t);
            float locoW = 0f, crossW = 0f;
            for (int k = -2; k <= 2; k++)
            {
                var st = k == 0 ? now
                    : SoldierActionResolver.Resolve(ctx, u, s.slot, t + k * 0.15f);
                if (KitClips.MetersPerCycle(st.clip) > 0f) locoW += 0.2f;
                if (st.clip == ClipId.Cross) crossW += 0.2f;
            }

            float bobY = 0f, swayX = 0f, gaitYaw = 0f, gaitRoll = 0f;
            if (locoW > 0f && KitClips.MetersPerCycle(now.clip) > 0f)
            {
                float phase01 = now.clipTime / KitClips.Duration(now.clip);
                float paceScale = now.clip == ClipId.DoubleQuick ? 1.4f
                    : now.clip == ClipId.RoutedRun ? 1.6f : 1f;
                float g = locoW * paceScale * damp;
                bobY = s.profile.bobAmpM * Mathf.Sin(4f * Mathf.PI * phase01) * g;
                swayX = s.profile.swayAmpM * Mathf.Sin(2f * Mathf.PI * phase01) * g;
                gaitYaw = s.profile.swayYawDeg * Mathf.Sin(2f * Mathf.PI * phase01) * g;
                gaitRoll = s.profile.rollAmpDeg * Mathf.Sin(2f * Mathf.PI * phase01) * g;
            }

            // fence climb: look down at the rails, step-up lift mid-clip;
            // gait bob is already faded out by locoW during the hold
            float crossPitch = s.profile.crossPitchDeg * crossW;
            float crossLift = 0f;
            if (now.clip == ClipId.Cross)
            {
                float cp = now.clipTime / KitClips.Duration(ClipId.Cross);
                crossLift = s.profile.crossLiftM * Mathf.Sin(Mathf.PI * cp) * crossW;
            }

            // 4) nearby strike -> brace/duck impulse (same compiled strike
            //    stream that drives figure flinches and strike dust)
            float duck = 0f;
            var strikes = ur.strikes;
            for (int i = strikes.Count - 1; i >= 0; i--)
            {
                float age = t - strikes[i].t;
                if (age < 0f) continue;
                if (age > ChaosStrikeWindowS) break; // sorted by t
                if (age <= StrikeWindowS)
                {
                    float d = (strikes[i].pos - pos).magnitude;
                    if (d < StrikeRadiusM)
                    {
                        float k = Mathf.Exp(-age * 4f) * (1f - d / StrikeRadiusM);
                        duck = Mathf.Max(duck, k);
                    }
                }
            }
            duck *= Mathf.Lerp(0.5f, 1f, damp); // even a stabilized cut ducks

            // 5) glance toward a man of the observer's own unit falling
            //    within arm's reach (deterministic, sober; no slow motion)
            float glanceYaw = 0f;
            {
                // envelope-weighted BLEND over all nearby falls: a best-pick
                // here snapped the head ~600 deg/s when two falls' envelopes
                // crossed (found by the comfort-bound test); the blend is
                // continuous in t because each envelope ramps from and
                // returns to zero.
                var cas = ur.casualties;
                float num = 0f, den = 0f;
                for (int slot = 0; slot < cas.Length; slot++)
                {
                    float fallT = cas[slot].fallT;
                    if (float.IsInfinity(fallT)) continue;
                    if (t < fallT || t > fallT + FallGlanceWindowS) continue;
                    var st = SoldierActionResolver.Resolve(ctx, u, slot, fallT);
                    var d = new Vector2(st.posX, st.posZ) - pos;
                    float dist = d.magnitude;
                    if (dist > FallGlanceRadiusM || dist < 1e-3f) continue;
                    float age = t - fallT;
                    // ramp 0.3 s, hold, release
                    float env = Mathf.Clamp01(age / 0.3f) *
                        Mathf.Clamp01((FallGlanceWindowS - age) / 1.2f);
                    if (env <= 0f) continue;
                    float toward = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
                    float rel = Mathf.DeltaAngle(heading, toward);
                    // never glance at what is behind: DeltaAngle is
                    // discontinuous at ±180°, and a man does not snap his
                    // head to a fall he cannot see — fade the weight to
                    // zero well before the wraparound (this was a real
                    // ~600 deg/s snap caught by the comfort-bound test)
                    float ang = Mathf.Abs(rel);
                    if (ang > 120f) continue;
                    float wAng = Mathf.Clamp01((120f - ang) / 30f);
                    num += Mathf.Clamp(rel, -60f, 60f) / 60f * env * wAng;
                    den += env * wAng;
                }
                if (den > 1e-5f)
                    glanceYaw = num / den * Mathf.Min(1f, den) *
                        s.profile.headTurnDeg;
            }

            // 6) chaos level: local casualty + strike density (the climax
            //    "fall/chaos" behavior, §12 P9) drives handheld shake
            float chaos = ChaosLevel(ctx, ur, pos, t);
            float noiseAmp = (s.profile.handheldDeg +
                s.profile.chaosShakeDeg * chaos) * damp;
            float nYaw = noiseAmp * Noise(ctx.seed, 0, t);
            float nPitch = 0.7f * noiseAmp * Noise(ctx.seed, 1, t);
            // rollAmpDeg == 0 (ReducedMotion) disables the roll channel
            // entirely — handheld noise included (§12 Phase 12 motion cut)
            float nRoll = s.profile.rollAmpDeg <= 0f
                ? 0f : 0.4f * noiseAmp * Noise(ctx.seed, 2, t);

            // 7) assemble
            float yaw = heading + s.lookYawOffsetDeg + gaitYaw + glanceYaw + nYaw;
            float pitch = s.lookPitchOffsetDeg + crossPitch + nPitch
                + duck * s.profile.duckPitchDeg;
            float roll = gaitRoll + nRoll;
            float eye = s.eyeHeightM + bobY + crossLift - duck * s.profile.duckMaxM;

            float hr = heading * Mathf.Deg2Rad;
            var fwd = new Vector2(Mathf.Sin(hr), Mathf.Cos(hr));
            var right = new Vector2(fwd.y, -fwd.x);
            Vector2 camXZ = pos + right * swayX;
            if (s.thirdPerson)
            {
                camXZ += -fwd * C3pBackM + right * C3pRightM
                    - right * swayX * (1f - C3pBobScale);
                eye = s.eyeHeightM + C3pUpM
                    + (bobY + crossLift - duck * s.profile.duckMaxM) * C3pBobScale;
            }

            return new HeroCameraPose
            {
                camX = camXZ.x,
                camZ = camXZ.y,
                groundRefX = pos.x,
                groundRefZ = pos.y,
                eyeAboveGroundM = eye,
                headingDeg = yaw,
                pitchDeg = pitch,
                rollDeg = roll,
                fovDeg = s.fovDeg,
                obsX = pos.x,
                obsZ = pos.y,
                chaos01 = chaos,
            };
        }

        // Local chaos in [0,1]: recent nearby strikes + own-unit falls.
        public static float ChaosLevel(
            AngleActionContext ctx, UnitRuntime ur, Vector2 pos, float t)
        {
            float level = 0f;
            var strikes = ur.strikes;
            for (int i = strikes.Count - 1; i >= 0; i--)
            {
                float age = t - strikes[i].t;
                if (age < 0f) continue;
                if (age > ChaosStrikeWindowS) break;
                if ((strikes[i].pos - pos).magnitude < ChaosStrikeRadiusM)
                    level += 0.25f * (1f - age / ChaosStrikeWindowS);
            }
            var cas = ur.casualties;
            for (int slot = 0; slot < cas.Length; slot++)
            {
                float fallT = cas[slot].fallT;
                if (float.IsInfinity(fallT)) continue;
                float age = t - fallT;
                if (age < 0f || age > ChaosFallWindowS) continue;
                var st = SoldierActionResolver.Resolve(
                    ctx, ur.unitIndex, slot, fallT);
                if ((new Vector2(st.posX, st.posZ) - pos).magnitude
                    < ChaosFallRadiusM)
                    level += 0.15f * (1f - age / ChaosFallWindowS);
            }
            return Mathf.Clamp01(level);
        }

        // Deterministic band-limited noise in [-1, 1]: cosine-interpolated
        // value noise on a NoiseHz lattice keyed by the battle seed.
        public static float Noise(string seed, int axis, float t)
        {
            float x = t * NoiseHz;
            int i = Mathf.FloorToInt(x);
            float f = x - i;
            string key = seed + "|p9-handheld|" + axis;
            float a = 2f * AngleEnvironmentLayout.Hash01(key, i) - 1f;
            float b = 2f * AngleEnvironmentLayout.Hash01(key, i + 1) - 1f;
            float c = 0.5f - 0.5f * Mathf.Cos(f * Mathf.PI);
            return a + (b - a) * c;
        }
    }
}
