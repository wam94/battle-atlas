using UnityEngine;

namespace BattleAtlas
{
    // Label placement math (atlas-cartography plan, D3): which units label
    // at which zoom, in what order they claim screen space, and how the
    // greedy declutter resolves collisions. Pure statics writing into
    // caller-owned buffers — deterministic (FNV tie-breaks ride
    // FormationLayout.Jitter, never Random), zero allocation, EditMode-
    // testable without a scene. UnitLabelField is the thin MonoBehaviour
    // that feeds this from BattleDirector's per-frame candidate buffers.
    public static class LabelLayout
    {
        // priority is "lower wins". Ranks are integer bands — selected >
        // active brigades/park > inactive brigades/park > active
        // regiments/batteries > rest — and the FNV fraction stays inside
        // [0, 0.5) so hash luck can never promote a label across a band.
        public const float SelectedPriority = -1f;
        const int PrioritySalt = 47;

        // sticky hysteresis: a label shown last frame keeps its spot unless
        // a challenger beats it by MORE than this — one-sided, so two
        // near-equal labels settle instead of oscillating. 0.75 lets a
        // shown label defend against any same-rank hash (fraction < 0.5)
        // but never against a genuinely higher rank a full band away.
        public const float StickyBonus = 0.75f;

        // constant-screen-size: world scale grows linearly with camera
        // distance, floored so a fly-through to the anchor never shrinks
        // the text to nothing. Display tunings, reviewed in the owner's
        // editor session like the symbol lift.
        public const float ScalePerMeter = 0.006f;
        public const float MinScaleDistance = 50f;

        // screen-space AABB estimate for the declutter: TMP metrics are
        // not worth a layout pass per candidate — a fixed per-glyph width
        // over-reserves slightly, which reads as breathing room
        public const float GlyphWidthPx = 9f;
        public const float LabelHeightPx = 20f;

        // Which rendered symbols offer a label at this tier (plan D3):
        // Block — brigades/parents and the park always, everything else
        // earns it by acting or being selected; Regiments — every rendering
        // member is a candidate (budget + declutter trim, not the tier
        // rule); Soldiers — selected only, text spam would fight the
        // figures.
        public static bool LabelsAtTier(
            BattleDirector.LodTier tier, UnitSymbol.Echelon echelon,
            bool active, bool selected)
        {
            switch (tier)
            {
                case BattleDirector.LodTier.Soldiers:
                    return selected;
                case BattleDirector.LodTier.Regiments:
                    return true;
                default: // Block
                    if (selected || active) return true;
                    return echelon == UnitSymbol.Echelon.Brigade
                        || echelon == UnitSymbol.Echelon.Park;
            }
        }

        // Deterministic total order over label candidates — the same
        // inputs yield the identical float on every frame and every scrub
        // pass, so declutter outcomes replay exactly.
        public static float Priority(
            UnitSymbol.Echelon echelon, bool active, bool selected, string unitId)
        {
            if (selected) return SelectedPriority;
            bool command = echelon == UnitSymbol.Echelon.Brigade
                || echelon == UnitSymbol.Echelon.Park;
            int rank = command ? (active ? 0 : 1) : (active ? 2 : 3);
            // Jitter in [-1, 1] -> fraction in [0, 0.499]: strictly inside
            // the band, FNV-deterministic per unit
            return rank
                + (FormationLayout.Jitter(unitId, 0, PrioritySalt) + 1f) * 0.2495f;
        }

        // World scale that holds the label at constant screen size.
        public static float LabelScale(float distance) =>
            Mathf.Max(distance, MinScaleDistance) * ScalePerMeter;

        // The candidate's screen-space AABB around its projected anchor.
        public static Rect ScreenRect(Vector2 screenPos, int textLength)
        {
            float w = textLength * GlyphWidthPx;
            return new Rect(
                screenPos.x - w / 2f, screenPos.y - LabelHeightPx / 2f,
                w, LabelHeightPx);
        }

        // Greedy declutter over a FIXED slot layout (one slot per unit +
        // one per roster ribbon; a slot whose unit didn't render this frame
        // carries +inf priority and never shows — fixed slots keep the
        // sticky bonus keyed to the same unit across frames). Candidates
        // are visited in effective-priority order (priority minus the
        // sticky bonus for slots shown last frame; insertion sort is stable
        // so exact ties keep slot order — deterministic); each is accepted
        // unless it overlaps an already-accepted rect or the budget is
        // spent. `shownLastFrame` and `shown` MAY be the same array: last
        // frame's flags are consumed into effectiveScratch before shown is
        // cleared. Writes accepted slot indices into results in priority
        // order (the pool assignment contract) and returns the count. All
        // buffers are caller-owned and at least `count` long (results at
        // least `budget`) — nothing allocates.
        public static int Declutter(
            int count, float[] priorities, Rect[] rects, bool[] shownLastFrame,
            float stickyBonus, int budget,
            float[] effectiveScratch, int[] orderScratch, bool[] shown, int[] results)
        {
            for (int i = 0; i < count; i++)
            {
                effectiveScratch[i] =
                    shownLastFrame[i] && !float.IsPositiveInfinity(priorities[i])
                        ? priorities[i] - stickyBonus
                        : priorities[i];
                orderScratch[i] = i;
            }
            // insertion sort, ascending effective priority: n <= a few
            // hundred slots, allocation-free, stable on exact ties
            for (int i = 1; i < count; i++)
            {
                int idx = orderScratch[i];
                float p = effectiveScratch[idx];
                int j = i - 1;
                while (j >= 0 && effectiveScratch[orderScratch[j]] > p)
                {
                    orderScratch[j + 1] = orderScratch[j];
                    j--;
                }
                orderScratch[j + 1] = idx;
            }
            for (int i = 0; i < count; i++) shown[i] = false;
            int accepted = 0;
            for (int k = 0; k < count && accepted < budget; k++)
            {
                int i = orderScratch[k];
                // absent slots sort to the tail — nothing real remains
                if (float.IsPositiveInfinity(priorities[i])) break;
                bool overlaps = false;
                for (int a = 0; a < accepted && !overlaps; a++)
                    overlaps = rects[i].Overlaps(rects[results[a]]);
                if (overlaps) continue;
                shown[i] = true;
                results[accepted++] = i;
            }
            return accepted;
        }
    }
}
