using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Deterministic victim selection from compiled casualty profiles
    // (plan §6.4). Pure function of (unit, battle seed):
    //
    //   * victims per profile are the hash-ranked eligible slots — never
    //     historical names, never Random;
    //   * victim k of a profile falls at t0 + dur * invCDF((k+0.5)/count),
    //     using the SAME intensity-curve CDFs the Phase 5 compiler pinned
    //     (reconstruction/scripts/validate_reconstruction.py), so the
    //     schedule's alive count reconciles with the bundle's per-second
    //     strength (exactly at profile boundaries; within one man mid-
    //     window, where the compiler's banker's rounding can differ at
    //     exact rounding ties);
    //   * cause classes are apportioned exactly per causeMix (largest
    //     remainder), and drive the wound-category vocabulary (§9.2);
    //   * scrubbing to any T reconstructs the same living/falling/dead.
    public static class CasualtySchedule
    {
        public enum Cause : byte { Musketry = 0, Canister = 1, Shell = 2, Unknown = 3 }

        // §9.2: limited wound-category vocabulary tied to broad cause
        // classes. No named-person wounds; inspected wounds read as
        // "reconstructed".
        public enum WoundCategory : byte
        {
            BallWound = 0,       // musketry: small-arms ball, torso/limb
            CanisterStrike = 1,  // close artillery: massive trauma
            FragmentWound = 2,   // shell fragment
            Unspecified = 3,     // aggregate-only evidence: no wound shown
        }

        public struct Entry
        {
            public float fallT;        // +inf: survives the slice
            public Cause cause;
            public bool woundedCrawl;  // survivor-of-fall subset that crawls
            public short profileIndex; // -1 when never a casualty
        }

        // fraction of casualties that read as wounded (fall, then crawl)
        // rather than lying still — sober reminder that "casualties" are
        // killed AND wounded; kept a minority so bodies still accumulate.
        public const float WoundedCrawlFraction = 0.22f;

        public static Entry[] Compile(AngleBundleUnit unit, string seed)
        {
            int n = unit.startStrength;
            var entries = new Entry[n];
            for (int i = 0; i < n; i++)
            {
                entries[i].fallT = float.PositiveInfinity;
                entries[i].profileIndex = -1;
            }

            // profiles in canonical order: by t0, then id (stable + pure)
            var order = new List<int>();
            for (int i = 0; i < unit.casualtyProfiles.Count; i++) order.Add(i);
            order.Sort((a, b) =>
            {
                var pa = unit.casualtyProfiles[a];
                var pb = unit.casualtyProfiles[b];
                int c = pa.t0.CompareTo(pb.t0);
                return c != 0 ? c : string.CompareOrdinal(pa.id, pb.id);
            });

            var taken = new bool[n];
            foreach (int pi in order)
            {
                var p = unit.casualtyProfiles[pi];
                if (p.count <= 0) continue;

                // hash-ranked eligible slots (untaken)
                string key = seed + "|" + unit.unitId + "|" + p.id;
                var cand = new List<int>();
                for (int s = 0; s < n; s++) if (!taken[s]) cand.Add(s);
                if (cand.Count < p.count)
                    throw new InvalidOperationException(
                        $"{unit.unitId}: profile {p.id} needs {p.count} victims, " +
                        $"only {cand.Count} men remain");
                cand.Sort((a, b) =>
                {
                    float ha = AngleEnvironmentLayout.Hash01(key, a);
                    float hb = AngleEnvironmentLayout.Hash01(key, b);
                    int c = ha.CompareTo(hb);
                    return c != 0 ? c : a.CompareTo(b);
                });

                var causes = ApportionCauses(p);
                float dur = p.t1 - p.t0;
                for (int k = 0; k < p.count; k++)
                {
                    int slot = cand[k];
                    taken[slot] = true;
                    entries[slot].fallT = p.t0 + dur *
                        InvCdf(p.intensityCurve, (k + 0.5f) / p.count);
                    entries[slot].cause = causes[k];
                    entries[slot].profileIndex = (short)pi;
                    entries[slot].woundedCrawl =
                        AngleEnvironmentLayout.Hash01(key + "|wound", slot)
                        < WoundedCrawlFraction;
                }
            }
            return entries;
        }

        // Exact integer cause apportionment (largest remainder), assigned
        // to victims k=0..count-1 by a second hash order so cause does not
        // correlate with fall time.
        static Cause[] ApportionCauses(AngleCasualtyProfile p)
        {
            float[] mix =
            {
                p.causeMix.musketry, p.causeMix.canister,
                p.causeMix.shell, p.causeMix.unknown,
            };
            float total = mix[0] + mix[1] + mix[2] + mix[3];
            if (total <= 0f) { mix[3] = 1f; total = 1f; }

            var counts = new int[4];
            var rem = new float[4];
            int assigned = 0;
            for (int c = 0; c < 4; c++)
            {
                float exact = p.count * mix[c] / total;
                counts[c] = (int)exact;
                rem[c] = exact - counts[c];
                assigned += counts[c];
            }
            while (assigned < p.count)
            {
                int best = 0;
                for (int c = 1; c < 4; c++) if (rem[c] > rem[best]) best = c;
                counts[best]++;
                rem[best] = -1f;
                assigned++;
            }

            var causes = new Cause[p.count];
            int idx = 0;
            for (int c = 0; c < 4; c++)
                for (int i = 0; i < counts[c]; i++)
                    causes[idx++] = (Cause)c;
            // deterministic shuffle: sort positions by hash
            var perm = new int[p.count];
            for (int i = 0; i < p.count; i++) perm[i] = i;
            string key = p.id + "|causeperm";
            Array.Sort(perm, (a, b) =>
            {
                float ha = AngleEnvironmentLayout.Hash01(key, a);
                float hb = AngleEnvironmentLayout.Hash01(key, b);
                int c = ha.CompareTo(hb);
                return c != 0 ? c : a.CompareTo(b);
            });
            var shuffled = new Cause[p.count];
            for (int i = 0; i < p.count; i++) shuffled[i] = causes[perm[i]];
            return shuffled;
        }

        // Inverse CDFs of the pinned intensity curves.
        public static float InvCdf(string curve, float p)
        {
            p = Mathf.Clamp01(p);
            switch (curve)
            {
                case "uniform": return p;
                case "rising": return Mathf.Sqrt(p);
                case "falling": return 1f - Mathf.Sqrt(1f - p);
                case "spike":
                    // inverse smoothstep (closed form)
                    return 0.5f - Mathf.Sin(Mathf.Asin(1f - 2f * p) / 3f);
                default:
                    throw new InvalidOperationException(
                        $"unknown intensityCurve '{curve}'");
            }
        }

        public static WoundCategory Wound(Cause cause) => cause switch
        {
            Cause.Musketry => WoundCategory.BallWound,
            Cause.Canister => WoundCategory.CanisterStrike,
            Cause.Shell => WoundCategory.FragmentWound,
            _ => WoundCategory.Unspecified,
        };

        public static int AliveCount(Entry[] entries, float t)
        {
            int alive = 0;
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].fallT > t) alive++;
            return alive;
        }
    }
}
