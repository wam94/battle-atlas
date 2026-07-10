using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Black-powder battle VFX math (plan §9.1). PURE: every puff attribute
    // is a closed-form function of (emission event, battle time, seed) —
    // no ParticleSystem, no _Time, no Random — so scrubbing to any frame
    // reconstructs the identical smoke field (the ADR 0003 "smoke reads as
    // tents" deferred item is retired by the Phase 8 gate evidence).
    //
    // Emission timing is clock-driven: musket smoke births exactly at the
    // resolver's Fire_Recoil discharge instant; cannon smoke at the
    // compiled battery shot times; dust at compiled strike/march events.
    //
    // Wind: no direct observation survives for the charge hour
    // (claim-wind-unknown). ED-19 reconstructs a light SW breeze —
    // witnesses describe smoke banks hanging over the field, so the drift
    // is deliberately weak (1.1 m/s toward the NE).
    public enum SmokeKind : byte { Musket = 0, Cannon = 1, StrikeDust = 2, MarchDust = 3 }

    public struct SmokeEvent
    {
        public float t;
        public Vector2 pos;         // macro meters
        public float heightM;       // emission height above ground
        public float dirDeg;        // muzzle bearing (0=N, 90=E)
        public SmokeKind kind;
        public int seedIndex;       // deterministic per-event hash index
    }

    public struct PuffInstance
    {
        public Vector2 pos;         // macro meters
        public float heightM;       // center above ground
        public float radius;
        public float alpha;         // 0..1
        public float shade;         // 0=dust brown family, 1=powder white family (blend)
        public byte texVariant;     // 0..2
        public float rollDeg;       // in-plane billboard roll
    }

    public static class BlackPowderVfx
    {
        // ED-19 canonical wind (reconstructed; claim-wind-unknown)
        public static readonly Vector2 WindMps = new Vector2(0.78f, 0.78f);

        public const float MusketLife = 24f;
        public const float CannonLife = 55f;
        public const float StrikeDustLife = 3.5f;
        public const float MarchDustLife = 7f;
        public const float FlashDur = 0.09f;

        public static float Life(SmokeKind kind) => kind switch
        {
            SmokeKind.Musket => MusketLife,
            SmokeKind.Cannon => CannonLife,
            SmokeKind.StrikeDust => StrikeDustLife,
            _ => MarchDustLife,
        };

        public static int SubPuffs(SmokeKind kind) => kind switch
        {
            SmokeKind.Musket => 3,
            SmokeKind.Cannon => 7,
            SmokeKind.StrikeDust => 2,
            _ => 2,
        };

        // Appends the live puff instances of one event at battle time t.
        // Returns the number appended (0 when outside the event's life).
        public static int Emit(
            in SmokeEvent e, float t, string seed, List<PuffInstance> outList)
        {
            float age = t - e.t;
            float life = Life(e.kind);
            if (age < 0f || age >= life) return 0;

            int n = SubPuffs(e.kind);
            string key = seed + "|puff" + (int)e.kind;
            float r = e.dirDeg * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
            int emitted = 0;
            for (int i = 0; i < n; i++)
            {
                int h = e.seedIndex * 31 + i;
                float h1 = AngleEnvironmentLayout.Hash01(key, h);
                float h2 = AngleEnvironmentLayout.Hash01(key, h * 7 + 1);
                float h3 = AngleEnvironmentLayout.Hash01(key, h * 7 + 2);
                float h4 = AngleEnvironmentLayout.Hash01(key, h * 7 + 3);

                // sub-puffs trail the jet: later ones sit closer to the muzzle
                float along = (i + 0.6f) / n;
                PuffInstance p;
                switch (e.kind)
                {
                    case SmokeKind.Musket:
                    {
                        float jet = 2.4f * along * (1f - Mathf.Exp(-age / 0.9f));
                        float grow = Mathf.Sqrt(Mathf.Min(age, 14f) / 14f);
                        p = new PuffInstance
                        {
                            pos = e.pos + dir * jet + WindMps * age
                                + new Vector2(h1 - 0.5f, h2 - 0.5f) * (0.5f + age * 0.10f),
                            heightM = e.heightM + 0.3f + 1.9f *
                                (1f - Mathf.Exp(-age / 11f)) + 0.5f * (h3 - 0.5f),
                            radius = 0.45f + 3.1f * grow * (0.75f + 0.5f * h4),
                            alpha = Fade(age, life, 0.35f, 7f) * 0.62f,
                            shade = Mathf.Lerp(0.98f, 0.80f, Mathf.Min(age / life, 1f)),
                            texVariant = (byte)(AngleEnvironmentLayout.Hash01(key, h * 7 + 4) * 2.999f),
                            rollDeg = 360f * h2 + age * (h3 > 0.5f ? 2.6f : -2.6f),
                        };
                        break;
                    }
                    case SmokeKind.Cannon:
                    {
                        float jet = 8.5f * along * (1f - Mathf.Exp(-age / 1.6f));
                        float grow = Mathf.Sqrt(Mathf.Min(age, 22f) / 22f);
                        p = new PuffInstance
                        {
                            pos = e.pos + dir * jet + WindMps * age
                                + new Vector2(h1 - 0.5f, h2 - 0.5f) * (1.2f + age * 0.12f),
                            heightM = e.heightM + 0.4f + 3.2f *
                                (1f - Mathf.Exp(-age / 14f)) + 1.2f * (h3 - 0.5f),
                            radius = 1.1f + 6.5f * grow * (0.7f + 0.6f * h4),
                            alpha = Fade(age, life, 0.25f, 14f) * 0.72f,
                            shade = Mathf.Lerp(1f, 0.78f, Mathf.Min(age / life, 1f)),
                            texVariant = (byte)(AngleEnvironmentLayout.Hash01(key, h * 7 + 4) * 2.999f),
                            rollDeg = 360f * h2 + age * (h3 > 0.5f ? 1.8f : -1.8f),
                        };
                        break;
                    }
                    case SmokeKind.StrikeDust:
                    {
                        p = new PuffInstance
                        {
                            pos = e.pos + WindMps * age * 0.4f
                                + new Vector2(h1 - 0.5f, h2 - 0.5f) * 1.1f,
                            heightM = 0.4f + 2.6f * (age / StrikeDustLife)
                                * (0.6f + 0.6f * h3),
                            radius = 0.6f + 1.6f * (age / StrikeDustLife)
                                * (0.7f + 0.6f * h4),
                            alpha = 0.62f * (1f - age / StrikeDustLife),
                            shade = 0.12f,      // thrown earth
                            texVariant = (byte)(AngleEnvironmentLayout.Hash01(key, h * 7 + 4) * 2.999f),
                            rollDeg = 360f * h2,
                        };
                        break;
                    }
                    default: // MarchDust
                    {
                        p = new PuffInstance
                        {
                            pos = e.pos + WindMps * age * 0.5f
                                + new Vector2(h1 - 0.5f, h2 - 0.5f) * 2.2f,
                            heightM = 0.35f + 0.7f * (age / MarchDustLife),
                            radius = 1.3f + 2.4f * (age / MarchDustLife),
                            alpha = 0.16f * Fade(age, MarchDustLife, 0.8f, 2.5f),
                            shade = 0.18f,
                            texVariant = (byte)(AngleEnvironmentLayout.Hash01(key, h * 7 + 4) * 2.999f),
                            rollDeg = 360f * h2,
                        };
                        break;
                    }
                }
                outList.Add(p);
                emitted++;
            }
            return emitted;
        }

        // ramp in over `up`, hold, decay after `hold` to 0 at life
        static float Fade(float age, float life, float up, float hold)
        {
            float a = Mathf.Clamp01(age / up);
            float b = age <= hold ? 1f
                : 1f - Mathf.Clamp01((age - hold) / (life - hold));
            return a * b * b;
        }

        // Muzzle flash window (short emissive quad at the muzzle).
        public static bool FlashActive(float eventT, float t) =>
            t >= eventT && t < eventT + FlashDur;

        // Progressive visibility loss (§9.1): the volumetric fog mean free
        // path shortens as smoke mass accumulates. The billboard banks
        // carry the LOCAL density; the global fog term is deliberately
        // gentle so the scene degrades without drowning (a climax-scale
        // alpha sum of ~2500 puts the mean free path near 280 m).
        public const float ClearMeanFreePathM = 900f;
        public const float MinMeanFreePathM = 240f;

        public static float FogMeanFreePath(float activeAlphaSum)
        {
            float mfp = ClearMeanFreePathM / (1f + activeAlphaSum / 1100f);
            return Mathf.Max(MinMeanFreePathM, mfp);
        }

        // Marching-dust emission times for a unit: an event every
        // `interval` seconds per 25 m of frontage while the unit moves
        // faster than a walk threshold. Pure function of the track.
        public static void CompileMarchDust(
            AngleBundleUnit unit, int slotCount, string seed,
            List<SmokeEvent> outEvents, ref int seedIndex)
        {
            const float interval = 2.5f;
            const float minSpeed = 0.35f;
            float frontage = FormationRoster.Frontage(slotCount);
            int chunks = Mathf.Max(1, Mathf.CeilToInt(frontage / 25f));
            string key = seed + "|" + unit.unitId + "|dust";
            float t0 = unit.perSecond.t0;
            float t1 = t0 + unit.perSecond.x.Count - 1;
            for (float t = t0; t < t1; t += interval)
            {
                Vector2 a = unit.PositionAt(t);
                Vector2 b = unit.PositionAt(t + 1f);
                if ((b - a).magnitude < minSpeed) continue;
                float facing = unit.FacingAt(t);
                for (int c = 0; c < chunks; c++)
                {
                    float x = (c + 0.5f) / chunks - 0.5f;
                    float jx = AngleEnvironmentLayout.Hash01(key, seedIndex * 3 + 1) - 0.5f;
                    float jy = AngleEnvironmentLayout.Hash01(key, seedIndex * 3 + 2) - 0.5f;
                    Vector2 pos = FormationRoster.ToWorld(
                        a, facing,
                        new Vector2(x * frontage + jx * 12f, -1.5f + jy * 3f));
                    outEvents.Add(new SmokeEvent
                    {
                        t = t + interval * AngleEnvironmentLayout.Hash01(
                            key, seedIndex * 3),
                        pos = pos,
                        heightM = 0.3f,
                        dirDeg = facing,
                        kind = SmokeKind.MarchDust,
                        seedIndex = seedIndex++,
                    });
                }
            }
        }
    }
}
