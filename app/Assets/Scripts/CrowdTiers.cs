using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Crowd tiers for the offline Angle scene (plan §7.4):
    //
    //   Hero  0-25 m    <=64    full kit figures, best clips, wound decals
    //   Near  25-100 m  <=400   decimated *_near figures, same skeleton
    //   Mid   100-350 m <=2500  instanced *_mid static pose meshes
    //   Far   beyond    -       formation-density impostors
    //
    // Tier assignment is PRESENTATION ONLY: a slot's identity, behavior,
    // casualty fate, and variant are functions of (unitId, slotId) and
    // never of tier, so a slot keeps its identity as the camera or the
    // formation moves it across tier boundaries (§13 stability tests).
    //
    // Band overflow DEMOTES the farthest overflow slots to the next tier;
    // nothing is ever silently dropped. A bookkeeping violation throws
    // (render-time validation, §12 Phase 8).
    public enum CrowdTier : byte { Hero = 0, Near = 1, Mid = 2, Far = 3 }

    public static class CrowdTiers
    {
        public const float HeroRangeM = 25f;
        public const float NearRangeM = 100f;
        public const float MidRangeM = 350f;
        public const int HeroCap = 64;
        public const int NearCap = 400;
        public const int MidCap = 2500;

        public struct SlotRef
        {
            public int unitIndex;
            public int slot;
            public float dist;
        }

        // Assigns a tier to every (unitIndex, slot) given each slot's
        // current world position. `positions` is indexed by flat slot id
        // (unit-major order); `flatBase[u]` is unit u's first flat index.
        public static CrowdTier[] Assign(
            Vector2 camXZ, IReadOnlyList<UnitRuntime> units,
            Func<int, int, Vector2> positionOf)
        {
            var refs = new List<SlotRef>();
            int total = 0;
            for (int u = 0; u < units.Count; u++) total += units[u].slotCount;
            refs.Capacity = total;
            for (int u = 0; u < units.Count; u++)
                for (int s = 0; s < units[u].slotCount; s++)
                    refs.Add(new SlotRef
                    {
                        unitIndex = u,
                        slot = s,
                        dist = (positionOf(u, s) - camXZ).magnitude,
                    });

            // deterministic order: distance, then unit, then slot
            refs.Sort((a, b) =>
            {
                int c = a.dist.CompareTo(b.dist);
                if (c != 0) return c;
                c = a.unitIndex.CompareTo(b.unitIndex);
                return c != 0 ? c : a.slot.CompareTo(b.slot);
            });

            var result = new CrowdTier[total];
            var assigned = new bool[total];
            var flatBase = new int[units.Count];
            for (int u = 1; u < units.Count; u++)
                flatBase[u] = flatBase[u - 1] + units[u - 1].slotCount;

            int hero = 0, near = 0, mid = 0;
            foreach (var r in refs)
            {
                int flat = flatBase[r.unitIndex] + r.slot;
                if (assigned[flat])
                    throw new InvalidOperationException(
                        $"tier bookkeeping: slot {r.unitIndex}/{r.slot} " +
                        "assigned twice");
                assigned[flat] = true;
                CrowdTier tier;
                if (r.dist <= HeroRangeM && hero < HeroCap)
                { tier = CrowdTier.Hero; hero++; }
                else if (r.dist <= NearRangeM && near < NearCap)
                { tier = CrowdTier.Near; near++; }
                else if (r.dist <= MidRangeM && mid < MidCap)
                { tier = CrowdTier.Mid; mid++; }
                else tier = CrowdTier.Far;
                result[flat] = tier;
            }

            for (int i = 0; i < total; i++)
                if (!assigned[i])
                    throw new InvalidOperationException(
                        $"tier bookkeeping: flat slot {i} never assigned");
            if (hero > HeroCap || near > NearCap || mid > MidCap)
                throw new InvalidOperationException(
                    $"tier overflow: hero={hero} near={near} mid={mid}");
            return result;
        }

        // Mid-tier pose vocabulary baked by the Phase 6 kit
        // (characters/kit/build_kit.py MID_POSES).
        public static string MidPose(ClipId clip, float clipTime, int slot, float t)
        {
            switch (clip)
            {
                case ClipId.Aim: return "pose_aim";
                case ClipId.Fire: return "pose_fire";
                case ClipId.Reload: return "pose_reload_rod";
                case ClipId.FallBack:
                case ClipId.FallCrumple: return "pose_fallen_back";
                case ClipId.FallSide:
                case ClipId.ProneCrawl: return "pose_fallen_side";
                case ClipId.March:
                case ClipId.RouteStep:
                case ClipId.DoubleQuick:
                case ClipId.RoutedRun:
                {
                    // two-frame flipbook keyed to the resolver's DISTANCE-
                    // driven stride phase (clipTime), so distant lines step
                    // at exactly the rate the ground passes underneath
                    float cycle = KitClips.Duration(clip);
                    float phase = (clipTime / cycle +
                        AngleEnvironmentLayout.Hash01("mid-step", slot)) % 1f;
                    return phase < 0.5f ? "pose_march_a" : "pose_march_b";
                }
                case ClipId.Cross:
                case ClipId.TurnRetreat:
                {
                    // fixed-duration transition clips keep a time flipbook
                    float cycle = KitClips.Duration(ClipId.March);
                    float phase = (t / cycle +
                        AngleEnvironmentLayout.Hash01("mid-step", slot)) % 1f;
                    return phase < 0.5f ? "pose_march_a" : "pose_march_b";
                }
                default:
                    return "pose_march_a";   // stand-ish at 100 m+
            }
        }
    }
}
