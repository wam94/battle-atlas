using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    public struct UnitState
    {
        public Vector2 posXZ;      // battlefield-local meters; .x = east, .y = north(z)
        public float facingDeg;    // compass degrees, 0 = north
        public float strength;
        public string formation;
        // provenance of the bracketing START keyframe (exactly the
        // formation carry rule); an empty/absent keyframe confidence reads
        // as "unknown" per the format default — UnitSymbol.StyleOf renders
        // that as inferred (hatched) per the 2026-07-09 user ruling
        public string confidence;
    }

    // Deterministic interpolation over one unit's keyframes. Pure C# (no
    // MonoBehaviour) so the math that must never lie is EditMode-testable.
    public class UnitTrack
    {
        public readonly UnitDto Unit;
        readonly List<KeyframeDto> kfs;

        public UnitTrack(UnitDto unit)
        {
            Unit = unit;
            kfs = unit.keyframes;
        }

        public UnitState StateAt(float t)
        {
            if (t <= kfs[0].t) return FromKeyframe(kfs[0]);
            KeyframeDto last = kfs[kfs.Count - 1];
            if (t >= last.t) return FromKeyframe(last);

            int hi = UpperBound(t); // first keyframe with kf.t > t
            KeyframeDto a = kfs[hi - 1];
            KeyframeDto b = kfs[hi];
            float u = (t - a.t) / (b.t - a.t);
            return new UnitState
            {
                posXZ = new Vector2(Mathf.Lerp(a.x, b.x, u), Mathf.Lerp(a.z, b.z, u)),
                facingDeg = Mathf.LerpAngle(a.facing, b.facing, u),
                strength = Mathf.Lerp(a.strength, b.strength, u),
                formation = a.formation, // segment carries its start formation
                confidence = ConfidenceOrDefault(a.confidence), // ...and its start confidence
            };
        }

        static UnitState FromKeyframe(KeyframeDto k) => new UnitState
        {
            posXZ = new Vector2(k.x, k.z),
            facingDeg = k.facing,
            strength = k.strength,
            formation = k.formation,
            confidence = ConfidenceOrDefault(k.confidence),
        };

        // format default (battle-format.md): an absent/empty confidence is
        // "unknown" — normalized here so every UnitState carries a word
        static string ConfidenceOrDefault(string confidence) =>
            string.IsNullOrEmpty(confidence) ? "unknown" : confidence;

        int UpperBound(float t)
        {
            int lo = 0, hi = kfs.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (kfs[mid].t <= t) lo = mid + 1; else hi = mid;
            }
            return lo;
        }
    }
}
