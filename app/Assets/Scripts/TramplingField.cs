using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Trampling driven by the compiled troop paths (owed from ED-17 /
    // Gate P7: "Phase 8 owes trampling driven by the compiled tracks").
    //
    // Pure compile: accumulate man-seconds-per-square-meter under every
    // unit's per-second footprint; a cell becomes trampled at the first
    // battle second its cumulative traffic crosses the threshold. The
    // stage then re-splats trampled cells to the ED-17 trampled ground
    // class as a pure function of battle time — scrubbing backward
    // un-tramples, exactly as determinism requires.
    public static class TramplingField
    {
        public const float ThresholdManSecPerM2 = 8f;
        public const float FootprintDepthM = 6f;

        // Returns first-trampled battle time per cell (+inf = never),
        // row-major with row 0 at MINIMUM z (south), matching Unity
        // terrain alphamap orientation.
        public static float[] Compile(
            IReadOnlyList<UnitRuntime> units,
            float x0, float z0, float sizeM, int res)
        {
            float cell = sizeM / res;
            var traffic = new float[res * res];
            var firstT = new float[res * res];
            for (int i = 0; i < firstT.Length; i++)
                firstT[i] = float.PositiveInfinity;

            foreach (var ur in units)
            {
                var u = ur.unit;
                float frontage = ur.isArtillery
                    ? FormationRoster.GunsPerBattery * FormationRoster.GunSpacingM
                    : FormationRoster.Frontage(ur.slotCount);
                float depth = ur.isArtillery ? 26f : FootprintDepthM;
                int t0 = (int)u.perSecond.t0;
                int seconds = u.perSecond.x.Count;
                for (int s = 0; s < seconds - 1; s++)
                {
                    float t = t0 + s;
                    float strength = u.perSecond.strength[s];
                    if (strength <= 0f) continue;
                    Vector2 c = new Vector2(u.perSecond.x[s], u.perSecond.z[s]);
                    float facing = u.perSecond.facingDeg[s];
                    float density = strength / (frontage * depth); // men/m2

                    float r = facing * Mathf.Deg2Rad;
                    var fwd = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
                    var right = new Vector2(fwd.y, -fwd.x);
                    float halfW = frontage / 2f, halfD = depth / 2f;
                    // AABB of the rotated footprint
                    float ext = Mathf.Abs(right.x) * halfW + Mathf.Abs(fwd.x) * halfD;
                    float extZ = Mathf.Abs(right.y) * halfW + Mathf.Abs(fwd.y) * halfD;
                    int cx0 = Mathf.Max(0, (int)((c.x - ext - x0) / cell));
                    int cx1 = Mathf.Min(res - 1, (int)((c.x + ext - x0) / cell));
                    int cz0 = Mathf.Max(0, (int)((c.y - extZ - z0) / cell));
                    int cz1 = Mathf.Min(res - 1, (int)((c.y + extZ - z0) / cell));
                    for (int cz = cz0; cz <= cz1; cz++)
                    {
                        for (int cx = cx0; cx <= cx1; cx++)
                        {
                            var p = new Vector2(
                                x0 + (cx + 0.5f) * cell,
                                z0 + (cz + 0.5f) * cell) - c;
                            float lx = Vector2.Dot(p, right);
                            float ly = Vector2.Dot(p, fwd);
                            if (Mathf.Abs(lx) > halfW || Mathf.Abs(ly) > halfD)
                                continue;
                            int idx = cz * res + cx;
                            if (!float.IsInfinity(firstT[idx])) continue;
                            traffic[idx] += density;   // man-seconds per m2
                            if (traffic[idx] >= ThresholdManSecPerM2)
                                firstT[idx] = t;
                        }
                    }
                }
            }
            return firstT;
        }

        public static bool TrampledAt(float[] firstT, int idx, float t) =>
            firstT[idx] <= t;

        public static int TrampledCount(float[] firstT, float t)
        {
            int n = 0;
            for (int i = 0; i < firstT.Length; i++)
                if (firstT[i] <= t) n++;
            return n;
        }
    }
}
