using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Fire/reload cycles compiled from the bundle's tactical actions
    // (plan §12 Phase 8: "compile fire/reload cycles from tactical actions
    // and fire events"). Pure functions of (segment, slot, hash) — the
    // same math drives both the resolver's clip phases and the VFX
    // emission times, so the smoke always leaves the musket that fired.
    //
    //   fire_by_rank      ranks alternate half-cycle volleys; small per-
    //                     slot raggedness inside a rank (§6.3 drill).
    //   fire_independent  each slot staggered across the cycle by hash.
    //
    // Cycle = aim + fire + full nine-stage reload + ready pause
    //       = 1.417 + 0.917 + 20 + 2 ≈ 24.3 s  (≈2.5 rounds/min).
    public static class FireCycles
    {
        public static readonly float AimDur = KitClips.Duration(ClipId.Aim);
        public static readonly float FireDur = KitClips.Duration(ClipId.Fire);
        public static readonly float ReloadDur = KitClips.Duration(ClipId.Reload);
        public const float ReadyDur = 2f;
        public static float Cycle => AimDur + FireDur + ReloadDur + ReadyDur;

        // discharge moment inside Fire_Recoil (muzzle flash/smoke birth)
        public const float MuzzleDelay = 0.10f;
        public const float RankRaggedness = 0.45f;   // volley spread, s

        public static bool IsFireAction(string action) =>
            action == "fire_by_rank" || action == "fire_independent";

        // Deterministic phase offset of `slot` inside a fire segment.
        public static float Offset(
            string seed, string unitId, AngleBundleSegment seg,
            int slot, int slotCount)
        {
            string key = seed + "|" + unitId + "|" + seg.id + "|fire";
            if (seg.action == "fire_by_rank")
            {
                int rank = FormationRoster.RankOf(slot, slotCount);
                return rank * (Cycle / 2f)
                    + RankRaggedness * AngleEnvironmentLayout.Hash01(key, slot);
            }
            return Cycle * AngleEnvironmentLayout.Hash01(key, slot);
        }

        public enum FirePhase : byte { Ready = 0, Aiming = 1, Firing = 2, Reloading = 3 }

        // Phase of the slot's cycle at battle time t (t within the segment).
        public static (FirePhase phase, float phaseTime) PhaseAt(
            AngleBundleSegment seg, float offset, float t)
        {
            float local = t - seg.t0 - offset;
            if (local < 0f) return (FirePhase.Ready, seg.t0 + offset - t);
            float c = local % Cycle;
            if (c < AimDur) return (FirePhase.Aiming, c);
            c -= AimDur;
            if (c < FireDur) return (FirePhase.Firing, c);
            c -= FireDur;
            if (c < ReloadDur) return (FirePhase.Reloading, c);
            return (FirePhase.Ready, c - ReloadDur);
        }

        // Enumerate this slot's musket discharge times inside [from, to)
        // clipped to the segment and to the slot's lifetime.
        public static void DischargeTimes(
            AngleBundleSegment seg, float offset, float aliveUntil,
            float from, float to, List<float> results)
        {
            float first = seg.t0 + offset + AimDur + MuzzleDelay;
            float stop = Mathf.Min(Mathf.Min(seg.t1, aliveUntil), to);
            for (float ft = first; ft < stop; ft += Cycle)
                if (ft >= from) results.Add(ft);
        }

        // --- artillery (per-gun, not per-slot) ---

        public const float CannonInterval = 16f;   // canister at close range
        public const float CannonJitter = 5f;

        public struct CannonShot
        {
            public float t;
            public int gun;
        }

        // Deterministic firing schedule for a battery's guns across all of
        // the unit's fire segments.
        public static List<CannonShot> CompileCannon(
            string seed, AngleBundleUnit unit, int gunCount)
        {
            var shots = new List<CannonShot>();
            string key = seed + "|" + unit.unitId + "|cannon";
            foreach (var seg in unit.segments)
            {
                if (!IsFireAction(seg.action)) continue;
                for (int g = 0; g < gunCount; g++)
                {
                    float t = seg.t0
                        + 6f * AngleEnvironmentLayout.Hash01(key, g * 131 + 7);
                    int k = 0;
                    while (t < seg.t1)
                    {
                        shots.Add(new CannonShot { t = t, gun = g });
                        t += CannonInterval + CannonJitter *
                            AngleEnvironmentLayout.Hash01(key, g * 131 + k * 17 + 29);
                        k++;
                    }
                }
            }
            shots.Sort((a, b) =>
            {
                int c = a.t.CompareTo(b.t);
                return c != 0 ? c : a.gun.CompareTo(b.gun);
            });
            return shots;
        }
    }
}
