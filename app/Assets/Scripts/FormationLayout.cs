using UnityEngine;

namespace BattleAtlas
{
    // Where each rendered soldier stands relative to the unit's center, in the
    // unit's local frame: x along the frontage (right positive), y along depth
    // (forward positive). Pure and deterministic — scrubbing the clock must
    // reproduce identical frames, so jitter derives from a hash, never Random.
    public static class FormationLayout
    {
        public const float MenPerFigure = 10f;
        public const int MaxFigures = 400;

        public static int FigureCount(float strength) =>
            Mathf.Min(MaxFigures, Mathf.CeilToInt(Mathf.Max(0f, strength) / MenPerFigure));

        public static Vector2[] Offsets(
            string unitId, string formation, int count, float frontage, float depth)
        {
            var offsets = new Vector2[count];
            if (count == 0) return offsets;
            switch (formation)
            {
                case "column":
                    FillRanks(offsets, frontage / 4f, depth * 4f);
                    break;
                case "skirmish":
                    FillSkirmish(unitId, offsets, frontage);
                    break;
                case "scattered":
                    FillScatter(unitId, offsets, frontage * 1.5f, depth * 1.5f, 0f);
                    break;
                case "routed":
                    // heavy scatter trailing rearward (negative y) of the facing
                    FillScatter(unitId, offsets, frontage, depth * 4f, -depth * 2f);
                    break;
                default: // "line" and anything unrecognized
                    FillRanks(offsets, frontage, depth);
                    break;
            }
            return offsets;
        }

        // even grid: as many ranks as needed to fit count across the width
        static void FillRanks(Vector2[] offsets, float width, float depth)
        {
            int count = offsets.Length;
            int perRank = Mathf.Max(1, Mathf.CeilToInt(count / Mathf.Max(1f, depth / 10f)));
            perRank = Mathf.Min(perRank, count);
            int ranks = Mathf.CeilToInt((float)count / perRank);
            // for the classic 2-rank line: prefer 2 ranks when they fit
            if (ranks < 2 && count >= 8) { ranks = 2; perRank = Mathf.CeilToInt(count / 2f); }
            for (int i = 0; i < count; i++)
            {
                int rank = i / perRank;
                int file = i % perRank;
                int filesInRank = Mathf.Min(perRank, count - rank * perRank);
                float x = filesInRank <= 1
                    ? 0f
                    : (file / (float)(filesInRank - 1) - 0.5f) * width;
                float y = ranks <= 1 ? 0f : (0.5f - rank / (float)(ranks - 1)) * (depth * 0.8f);
                offsets[i] = new Vector2(x, y);
            }
        }

        static void FillSkirmish(string unitId, Vector2[] offsets, float frontage)
        {
            int count = offsets.Length;
            for (int i = 0; i < count; i++)
            {
                float x = count <= 1 ? 0f : (i / (float)(count - 1) - 0.5f) * frontage * 1.2f;
                offsets[i] = new Vector2(
                    x + Jitter(unitId, i, 11) * 4f,
                    Jitter(unitId, i, 23) * 6f);
            }
        }

        static void FillScatter(
            string unitId, Vector2[] offsets, float width, float depth, float yBias)
        {
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = new Vector2(
                    Jitter(unitId, i, 7) * width * 0.5f,
                    yBias + Jitter(unitId, i, 13) * depth * 0.5f);
        }

        // deterministic pseudo-random in [-1, 1] from (unitId, index, salt)
        static float Jitter(string unitId, int index, int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in unitId) h = (h ^ c) * 16777619u;
                h = (h ^ (uint)index) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return ((h & 0xFFFFFF) / (float)0xFFFFFF) * 2f - 1f;
            }
        }
    }
}
