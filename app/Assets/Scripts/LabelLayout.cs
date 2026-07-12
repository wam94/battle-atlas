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
        // aggregates (corps/division) > active brigades/park > inactive
        // brigades/park > active regiments/batteries > rest — and the FNV
        // fraction stays inside [0, 0.5) so hash luck can never promote a
        // label across a band.
        public const float SelectedPriority = -1f;
        const int PrioritySalt = 47;

        // ------------------------------------------------------------------
        // Altitude bands (cartography slice 1): WHICH GRAIN of name the map
        // offers is a function of how high the camera flies, independent of
        // the per-unit mesh LOD. Theater reads corps; Mid reads divisions
        // plus command-echelon units; Tactical reads everything the tier
        // rule already allowed. Thresholds are camera height above the
        // displayed terrain; each boundary carries a hysteresis gap so
        // orbiting at the boundary can't strobe the label set.
        public enum LabelBand { Theater, Mid, Tactical }

        public const float TheaterInAltM = 2400f;
        public const float TheaterOutAltM = 2150f;
        public const float TacticalInAltM = 950f;
        public const float TacticalOutAltM = 1075f;

        public static LabelBand BandFor(float altitudeM, LabelBand current)
        {
            switch (current)
            {
                case LabelBand.Theater:
                    if (altitudeM >= TheaterOutAltM) return LabelBand.Theater;
                    return altitudeM < TacticalInAltM
                        ? LabelBand.Tactical : LabelBand.Mid;
                case LabelBand.Tactical:
                    if (altitudeM <= TacticalOutAltM) return LabelBand.Tactical;
                    return altitudeM > TheaterInAltM
                        ? LabelBand.Theater : LabelBand.Mid;
                default:
                    if (altitudeM > TheaterInAltM) return LabelBand.Theater;
                    if (altitudeM < TacticalInAltM) return LabelBand.Tactical;
                    return LabelBand.Mid;
            }
        }

        // The band gate over UNIT labels (aggregate corps/division labels
        // are the director's own slots, stamped only in their band):
        // Theater — no unit names; a dozen corps words carry the whole
        // field, and a selected unit keeps its name (deliberate attention).
        // Mid — command echelon only (brigades and the park). Tactical —
        // exactly the tier rule that always governed close range.
        public static bool LabelsAtBand(
            LabelBand band, BattleDirector.LodTier tier,
            UnitSymbol.Echelon echelon, bool active, bool selected)
        {
            if (selected) return true;
            switch (band)
            {
                case LabelBand.Theater:
                    return false;
                case LabelBand.Mid:
                    return echelon == UnitSymbol.Echelon.Brigade
                        || echelon == UnitSymbol.Echelon.Park;
                default:
                    return LabelsAtTier(tier, echelon, active, selected);
            }
        }

        // Aggregate (corps/division) label priority: strictly between
        // selected (-1) and every unit rank (>= 0), FNV fraction inside
        // [-0.75, -0.55] so aggregates order deterministically among
        // themselves and can never collide with a band above or below.
        public const float AggregateBasePriority = -0.75f;

        public static float AggregatePriority(string key) =>
            AggregateBasePriority
            + (FormationLayout.Jitter(key, 0, PrioritySalt) + 1f) * 0.1f;

        // Constant-screen-size multipliers for the aggregate grains: a
        // corps word is the theater map's headline, a division word the mid
        // map's subhead — both must outrank a brigade name at a glance.
        public const float CorpsLabelScale = 1.8f;
        public const float DivisionLabelScale = 1.4f;

        // The map label for a unit: its name up to the qualifying tail —
        // trailing "(…)" groups (successions, corps chains, commander
        // notes) and ", X's Division"-style comma suffixes fall away until
        // the name is stable. The drawer keeps the full record; the MAP is
        // not the place for a unit's whole pedigree (P11: "the labels are
        // there but not easy to see"). Deterministic and pure.
        public static string ShortName(string displayName)
        {
            string s = displayName;
            bool changed = true;
            while (changed)
            {
                changed = false;
                if (s.EndsWith(")", System.StringComparison.Ordinal))
                {
                    int open = s.LastIndexOf(" (", System.StringComparison.Ordinal);
                    if (open > 0)
                    {
                        s = s.Substring(0, open);
                        changed = true;
                    }
                }
                int comma = s.IndexOf(", ", System.StringComparison.Ordinal);
                if (comma > 0)
                {
                    s = s.Substring(0, comma);
                    changed = true;
                }
            }
            return s.Length > 0 ? s : displayName;
        }

        // Label ink: the side color lifted toward paper white so the text
        // reads against the terrain palette over the dark halo the label
        // material carries — map-lettering contrast comes from the halo,
        // hue stays the side's.
        public const float InkLift = 0.42f;

        public static Color InkTint(Color sideColor) =>
            Color.Lerp(sideColor, Color.white, InkLift);

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
        // over-reserves slightly, which reads as breathing room. Widened
        // for the cartography slice: the P11/wave captures showed accepted
        // labels still brushing each other, so the estimate now buys real
        // air (10 px/glyph, 24 px line).
        public const float GlyphWidthPx = 10f;
        public const float LabelHeightPx = 24f;

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
