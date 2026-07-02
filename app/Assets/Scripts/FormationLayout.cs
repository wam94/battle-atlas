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
            Offsets(unitId, formation, count, frontage, depth, offsets);
            return offsets;
        }

        // Buffer-reuse overload: fills the first `count` entries of a
        // caller-provided buffer instead of allocating. Used by the per-frame
        // render path (UnitFormationRenderer) to avoid GC pressure.
        public static void Offsets(
            string unitId, string formation, int count, float frontage, float depth,
            Vector2[] buffer)
        {
            if (count == 0) return;
            switch (formation)
            {
                case "column":
                    FillRanks(buffer, count, frontage / 4f, depth * 4f);
                    break;
                case "skirmish":
                    FillSkirmish(unitId, buffer, count, frontage);
                    break;
                case "scattered":
                    FillScatter(unitId, buffer, count, frontage * 1.5f, depth * 1.5f, 0f);
                    break;
                case "routed":
                    // heavy scatter trailing rearward (negative y) of the facing
                    FillScatter(unitId, buffer, count, frontage, depth * 4f, -depth * 2f);
                    break;
                default: // "line" and anything unrecognized
                    FillRanks(buffer, count, frontage, depth);
                    break;
            }
        }

        // interval between regiment sub-blocks, meters (drill-manual spacing)
        const float RegimentGap = 6f;

        // Where each regiment sub-block sits inside the unit's footprint, in the
        // unit-local frame (same axes as Offsets): center + size per sub-block.
        // Line: count blocks side by side along the frontage, equal widths, 6m
        // gaps, ordered +x (right) to -x (left) to match the roster's
        // right-to-left convention. Column: the blocks stack front-to-back
        // inside the column footprint (frontage/4 wide, depth*4 deep — the same
        // footprint FillRanks gives column figures), slot 0 leading. Count 1
        // degenerates to the formation's full block. Scattered/routed
        // formations never call this — the middle LOD tier renders those as
        // the monolithic block (a dissolving unit has no ordered sub-blocks).
        public static (Vector2 center, Vector2 size)[] RegimentSlots(
            string formation, int count, float frontage, float depth)
        {
            var slots = new (Vector2 center, Vector2 size)[count];
            RegimentSlots(formation, count, frontage, depth, slots);
            return slots;
        }

        // Buffer-reuse overload: fills the first `count` entries of a
        // caller-provided buffer instead of allocating (per-frame render path).
        public static void RegimentSlots(
            string formation, int count, float frontage, float depth,
            (Vector2 center, Vector2 size)[] buffer)
        {
            if (count <= 0) return;
            if (formation == "column")
            {
                float width = frontage / 4f;
                float totalDepth = depth * 4f;
                float subDepth = (totalDepth - RegimentGap * (count - 1)) / count;
                for (int i = 0; i < count; i++)
                {
                    float y = totalDepth / 2f - subDepth / 2f - i * (subDepth + RegimentGap);
                    buffer[i] = (new Vector2(0f, y), new Vector2(width, subDepth));
                }
            }
            else // "line" (and anything unrecognized, matching Offsets' fallback)
            {
                float width = (frontage - RegimentGap * (count - 1)) / count;
                for (int i = 0; i < count; i++)
                {
                    float x = frontage / 2f - width / 2f - i * (width + RegimentGap);
                    buffer[i] = (new Vector2(x, 0f), new Vector2(width, depth));
                }
            }
        }

        // even grid: as many ranks as needed to fit count across the width
        static void FillRanks(Vector2[] offsets, int count, float width, float depth)
        {
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

        static void FillSkirmish(string unitId, Vector2[] offsets, int count, float frontage)
        {
            for (int i = 0; i < count; i++)
            {
                float x = count <= 1 ? 0f : (i / (float)(count - 1) - 0.5f) * frontage * 1.2f;
                offsets[i] = new Vector2(
                    x + Jitter(unitId, i, 11) * 4f,
                    Jitter(unitId, i, 23) * 6f);
            }
        }

        static void FillScatter(
            string unitId, Vector2[] offsets, int count, float width, float depth, float yBias)
        {
            for (int i = 0; i < count; i++)
                offsets[i] = new Vector2(
                    Jitter(unitId, i, 7) * width * 0.5f,
                    yBias + Jitter(unitId, i, 13) * depth * 0.5f);
        }

        // deterministic pseudo-random in [-1, 1] from (unitId, index, salt).
        // Internal (not private) so other deterministic placement code in this
        // assembly — e.g. VegetationField — shares the same hash-jitter approach
        // instead of duplicating it.
        internal static float Jitter(string unitId, int index, int salt)
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
