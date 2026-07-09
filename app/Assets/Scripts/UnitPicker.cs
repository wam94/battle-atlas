using System;
using UnityEngine;

namespace BattleAtlas
{
    // Click-to-select math (atlas-cartography plan, Task 6): a fixed-step
    // raymarch to the displayed terrain plus point-in-oriented-footprint
    // picking over the rendered symbols. Pure statics, NO physics — no
    // colliders to resurrect, deterministic, EditMode-testable against a
    // synthetic height field exactly like the ribbon builder. The height
    // func IS the displayed terrain, so picking agrees with the draped
    // symbols under whatever vertical scale the scene shows.
    public static class UnitPicker
    {
        // march grain: fine enough that a theater-scale grazing ray cannot
        // step over the ~20 m relief that matters here; bisection refines
        // the bracketed crossing to centimeters
        public const float RayStepM = 4f;
        public const int BisectionSteps = 24;

        // the artillery pick band: gun-dots encode strength, not thickness,
        // so a battery's clickable footprint is the frontage x the band
        // from the dot row down past the (double) baseline —
        // GunDotSizeM + 2 x (BaselineGapM + gap-to-second) + baseline
        // width, symmetrized about the symbol center: 2 x (3 + 5 + 0.75)
        public const float ArtilleryPickBandM = 17.5f;

        // The clickable depth of a rendered symbol's footprint. Bars and
        // the park pick by their drawn thickness; artillery picks by the
        // dot/baseline band (its displayDepth never drew anything).
        public static float PickDepth(UnitSymbol.SymbolKind kind, float displayDepth) =>
            kind == UnitSymbol.SymbolKind.Artillery ? ArtilleryPickBandM : displayDepth;

        // Marches origin + t*dir against the height field until the ray
        // dips below ground, then bisection-refines the bracketed crossing.
        // An origin already at-or-under the surface is its own contact (the
        // camera can sit in a hollow at close zoom). Returns false when the
        // ray stays above ground through maxDist.
        public static bool RaycastTerrain(
            Vector3 origin, Vector3 dir, Func<float, float, float> groundY,
            float maxDist, out Vector3 hit)
        {
            dir = dir.normalized;
            if (origin.y - groundY(origin.x, origin.z) <= 0f)
            {
                hit = origin;
                return true;
            }
            float prevT = 0f;
            for (float t = RayStepM; ; t += RayStepM)
            {
                bool last = t >= maxDist;
                if (last) t = maxDist;
                Vector3 p = origin + dir * t;
                if (p.y - groundY(p.x, p.z) <= 0f)
                {
                    // crossing bracketed in [prevT, t]: bisect to the surface
                    float lo = prevT, hi = t;
                    for (int i = 0; i < BisectionSteps; i++)
                    {
                        float mid = (lo + hi) * 0.5f;
                        Vector3 m = origin + dir * mid;
                        if (m.y - groundY(m.x, m.z) > 0f) lo = mid;
                        else hi = mid;
                    }
                    hit = origin + dir * hi;
                    return true;
                }
                prevT = t;
                if (last)
                {
                    hit = default;
                    return false;
                }
            }
        }

        // Is the ground point inside the facing-rotated frontage x depth
        // rectangle? World -> unit-local via the inverse of the one facing
        // rotation every symbol uses (FootprintSamplePoints' convention).
        public static bool InFootprint(
            Vector2 pointXZ, Vector2 centerXZ, float facingDeg,
            float frontage, float depth)
        {
            Vector3 local = Quaternion.Euler(0f, -facingDeg, 0f)
                * new Vector3(pointXZ.x - centerXZ.x, 0f, pointXZ.y - centerXZ.y);
            return Mathf.Abs(local.x) <= frontage / 2f
                && Mathf.Abs(local.z) <= depth / 2f;
        }

        // The picked candidate index, or -1 for empty ground. Every
        // containing footprint competes and the SMALLEST AREA wins, so a
        // regiment beats the brigade ribbon overlapping it; an exact area
        // tie keeps the first candidate (fill order is authored-file order
        // — deterministic). Buffers are the caller's per-frame pick arrays,
        // count entries long.
        public static int PickUnit(
            Vector2 pointXZ, int count, Vector2[] centers, float[] facingsDeg,
            float[] frontages, float[] depths)
        {
            int best = -1;
            float bestArea = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (!InFootprint(pointXZ, centers[i], facingsDeg[i],
                        frontages[i], depths[i]))
                    continue;
                float area = frontages[i] * depths[i];
                if (area < bestArea)
                {
                    bestArea = area;
                    best = i;
                }
            }
            return best;
        }
    }
}
