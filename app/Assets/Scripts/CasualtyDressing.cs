using System;
using UnityEngine;

namespace BattleAtlas
{
    // Sober casualty dressing (plan §9.2): ground blood pooling under the
    // fallen, dropped equipment beside them, and a limited wound-category
    // vocabulary tied to broad cause classes. Pure functions of the
    // casualty schedule + hashes: scrubbing reconstructs the same pools
    // and dropped muskets every time. Documented in
    // docs/reconstruction/violence-and-representation.md.
    public static class CasualtyDressing
    {
        public struct BloodPool
        {
            public Vector2 pos;
            public float radius;
            public float alpha;
        }

        public struct DroppedItem
        {
            public Vector2 pos;
            public float yawDeg;
        }

        public const float PoolDelay = 2f;      // fall -> first visible blood
        public const float PoolGrowDur = 90f;

        // §9.2 rule: when the evidence supports only aggregate loss
        // (cause "unknown"), do NOT stage an invented wound — the figure
        // falls, but no blood vocabulary is attached.
        public static bool ShowsBlood(CasualtySchedule.WoundCategory wound) =>
            wound != CasualtySchedule.WoundCategory.Unspecified;

        public static bool Pool(
            in CasualtySchedule.Entry cas, Vector2 fallPos, float t,
            string unitKey, int slot, out BloodPool pool)
        {
            pool = default;
            if (float.IsInfinity(cas.fallT) || t < cas.fallT + PoolDelay)
                return false;
            var wound = CasualtySchedule.Wound(cas.cause);
            if (!ShowsBlood(wound)) return false;

            float h1 = AngleEnvironmentLayout.Hash01(unitKey + "|pool", slot);
            float h2 = AngleEnvironmentLayout.Hash01(unitKey + "|pool", slot * 5 + 1);
            float grow = Mathf.Sqrt(Mathf.Clamp01(
                (t - cas.fallT - PoolDelay) / PoolGrowDur));
            float rMax = wound == CasualtySchedule.WoundCategory.CanisterStrike
                ? 0.55f + 0.45f * h1
                : 0.30f + 0.30f * h1;
            // wounded who crawl away leave a smaller stain
            if (cas.woundedCrawl) rMax *= 0.55f;
            pool = new BloodPool
            {
                pos = fallPos + new Vector2(h2 - 0.5f, h1 - 0.5f) * 0.3f,
                radius = rMax * grow,
                alpha = 0.82f,
            };
            return pool.radius > 0.02f;
        }

        // A hash majority of the fallen drop their musket beside the body
        // (artillery crews carry none).
        public const float DropFraction = 0.65f;

        public static bool Dropped(
            in CasualtySchedule.Entry cas, bool isArtillery, Vector2 fallPos,
            float fallClipDur, float t, string unitKey, int slot,
            out DroppedItem item)
        {
            item = default;
            if (isArtillery || float.IsInfinity(cas.fallT)) return false;
            if (t < cas.fallT + fallClipDur) return false;
            float h = AngleEnvironmentLayout.Hash01(unitKey + "|drop", slot);
            if (h > DropFraction) return false;
            float h2 = AngleEnvironmentLayout.Hash01(unitKey + "|drop", slot * 7 + 1);
            float h3 = AngleEnvironmentLayout.Hash01(unitKey + "|drop", slot * 7 + 2);
            float ang = h2 * 2f * Mathf.PI;
            item = new DroppedItem
            {
                pos = fallPos + new Vector2(
                    Mathf.Cos(ang), Mathf.Sin(ang)) * (0.55f + 0.5f * h3),
                yawDeg = 360f * h,
            };
            return true;
        }

        // Hero-tier wound patch size by category (meters).
        public static float WoundPatchSize(CasualtySchedule.WoundCategory wound)
            => wound switch
            {
                CasualtySchedule.WoundCategory.CanisterStrike => 0.17f,
                CasualtySchedule.WoundCategory.FragmentWound => 0.14f,
                CasualtySchedule.WoundCategory.BallWound => 0.10f,
                _ => 0f,
            };
    }
}
