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
            action == "fire_by_rank" || action == "fire_independent" ||
            action == "fight_prone";

        // ------------------------------------------------------------------
        // fight_prone (Iverson slice, claim-iv-lying-down): the line goes
        // prone under fire and fights from the ground. Per-slot timeline:
        //
        //   seg.t0 + stagger(hash)      -> Go_Prone (the line drops file
        //                                  by file over a few seconds)
        //   + GoProne duration          -> prone ready
        //   + offset(hash)              -> prone fire cycles:
        //       Fight_Prone_Fire (level the piece, discharge at
        //       ProneMuzzleDelay) + Fight_Prone_Reload (the roll-to-load
        //       compromise) + prone ready pause
        //
        // Cycle = 2.4 + 26 + 3 ≈ 31.4 s (~1.9 rounds/min) — deliberately
        // slower than the standing cycle's ~2.5: loading a muzzle-loader
        // lying down costs time; that cost is the point of depicting it.
        public static readonly float ProneFireDur =
            KitClips.Duration(ClipId.FightProneFire);
        public static readonly float ProneReloadDur =
            KitClips.Duration(ClipId.FightProneReload);
        public const float ProneReadyDur = 3f;
        public static float ProneCycle =>
            ProneFireDur + ProneReloadDur + ProneReadyDur;

        // discharge moment inside Fight_Prone_Fire
        public const float ProneMuzzleDelay = 1.5f;
        // muzzle height above ground for a prone discharge (VFX/audio)
        public const float ProneMuzzleHeightM = 0.35f;
        public const float GoProneStagger = 4f;   // line drops over 0..4 s

        public static bool IsProneFightAction(string action) =>
            action == "fight_prone";

        // When this slot drops (start of Go_Prone). Pure.
        public static float ProneDropTime(
            string seed, string unitId, AngleBundleSegment seg, int slot)
        {
            string key = seed + "|" + unitId + "|" + seg.id + "|drop";
            return seg.t0 + GoProneStagger *
                AngleEnvironmentLayout.Hash01(key, slot);
        }

        // When this slot's first prone fire cycle begins (after the drop,
        // staggered across the cycle so the line's fire is independent).
        public static float ProneCycleStart(
            string seed, string unitId, AngleBundleSegment seg, int slot)
        {
            string key = seed + "|" + unitId + "|" + seg.id + "|fire";
            return ProneDropTime(seed, unitId, seg, slot)
                + KitClips.Duration(ClipId.GoProne)
                + ProneCycle * AngleEnvironmentLayout.Hash01(key, slot);
        }

        // Phase of the slot's prone cycle at battle time t.
        public static (FirePhase phase, float phaseTime) PhaseAtProne(
            float cycleStart, float t)
        {
            float local = t - cycleStart;
            if (local < 0f) return (FirePhase.Ready, cycleStart - t);
            float c = local % ProneCycle;
            if (c < ProneFireDur) return (FirePhase.Firing, c);
            c -= ProneFireDur;
            if (c < ProneReloadDur) return (FirePhase.Reloading, c);
            return (FirePhase.Ready, c - ProneReloadDur);
        }

        // Prone discharge times inside [from, to), clipped to the segment
        // and the slot's lifetime.
        public static void ProneDischargeTimes(
            string seed, string unitId, AngleBundleSegment seg, int slot,
            float aliveUntil, float from, float to, List<float> results)
        {
            float first = ProneCycleStart(seed, unitId, seg, slot)
                + ProneMuzzleDelay;
            float stop = Mathf.Min(Mathf.Min(seg.t1, aliveUntil), to);
            for (float ft = first; ft < stop; ft += ProneCycle)
                if (ft >= from) results.Add(ft);
        }

        // Unified per-slot discharge enumeration for any firing segment
        // (standing cycles vs the prone cycle) — VFX and audio exports
        // must call this so the emission math always matches the resolver.
        public static void SegmentDischargeTimes(
            string seed, string unitId, AngleBundleSegment seg,
            int slot, int slotCount, float aliveUntil,
            float from, float to, List<float> results)
        {
            if (IsProneFightAction(seg.action))
            {
                ProneDischargeTimes(
                    seed, unitId, seg, slot, aliveUntil, from, to, results);
                return;
            }
            float offset = Offset(seed, unitId, seg, slot, slotCount);
            DischargeTimes(seg, offset, aliveUntil, from, to, results);
        }

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
