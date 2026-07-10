using System;
using UnityEngine;

namespace BattleAtlas
{
    // Formation-slot identity for the Angle action scene (plan §7.4).
    //
    // Every man of a unit's start strength owns one PERMANENT slot id
    // (0..startStrength-1) for the whole slice. The slot never re-indexes:
    // its formation offset, kit variant, casualty fate, and behavior all
    // derive from (unitId, slotId) hashes, so identity is stable across
    // time, scrubbing, and crowd-tier changes (§13 slot-stability tests).
    // Casualties therefore leave visible gaps in the line (§6.4) instead
    // of the formation re-dressing over the dead.
    //
    // Offsets are in the unit-local frame: x along the frontage (right
    // positive), y along depth (forward positive), meters. Facing follows
    // the project convention: 0 = north (+z), 90 = east (+x).
    public static class FormationRoster
    {
        public const int Ranks = 2;               // two-rank line of battle
        public const float FileSpacingM = 0.5f;   // touch of elbows
        public const float RankSpacingM = 1.3f;

        // artillery crew staging (crew-response placeholder, §9.1)
        public const int GunsPerBattery = 6;
        public const int CrewPerGun = 8;
        public const float GunSpacingM = 14f;     // drill-manual interval

        public static int Files(int slotCount) =>
            (slotCount + Ranks - 1) / Ranks;

        public static float Frontage(int slotCount) =>
            Files(slotCount) * FileSpacingM;

        public static int RankOf(int slot, int slotCount) =>
            slot / Files(slotCount);

        // Infantry offset for one formation type. Deterministic jitter
        // comes from the shared FNV hash keyed by unit + slot.
        public static Vector2 Offset(
            string unitId, int slot, int slotCount, string formation)
        {
            int files = Files(slotCount);
            int rank = slot / files;
            int file = slot % files;
            float x0 = (file - (files - 1) * 0.5f) * FileSpacingM;
            float y0 = -rank * RankSpacingM;
            switch (formation)
            {
                case "line":
                    return new Vector2(
                        x0 + 0.06f * Jit(unitId, slot, 11),
                        y0 + 0.08f * Jit(unitId, slot, 13));
                case "line_disordered":
                    return new Vector2(
                        x0 + 0.6f * Jit(unitId, slot, 17),
                        y0 - 0.7f + 1.2f * Jit(unitId, slot, 19));
                case "scattered":
                    return new Vector2(
                        x0 * 0.85f + 2.6f * Jit(unitId, slot, 23),
                        y0 * 2f - 2.5f + 4.5f * Jit(unitId, slot, 29));
                case "routed":
                    // heavy scatter trailing rearward of the facing
                    return new Vector2(
                        x0 * 0.7f + 4.5f * Jit(unitId, slot, 31),
                        y0 - 5f - 20f * AngleEnvironmentLayout.Hash01(
                            unitId + "|rout", slot));
                default:
                    throw new InvalidOperationException(
                        $"{unitId}: unknown formation '{formation}'");
            }
        }

        // Segment-blended offset: formationFrom eases into formationTo over
        // the segment (smoothstep), continuous and pure in t.
        public static Vector2 BlendedOffset(
            string unitId, int slot, int slotCount,
            string from, string to, float progress)
        {
            var a = Offset(unitId, slot, slotCount, from);
            if (from == to) return a;
            var b = Offset(unitId, slot, slotCount, to);
            return Vector2.Lerp(a, b, Mathf.SmoothStep(0f, 1f, progress));
        }

        // Artillery: slots crew the battery instead of forming a line.
        // Slot -> gun cluster (first GunsPerBattery*CrewPerGun slots) or the
        // limber/horse-holder echelon 22 m to the rear. Guns sit on the
        // frontage line at drill spacing; crew in a serving arc behind each
        // piece. This is the plan's "crew response placeholder" (§9.1) —
        // no horses are modeled, and the rear echelon stands for drivers.
        public static Vector2 ArtilleryOffset(string unitId, int slot, int slotCount)
        {
            int crewSlots = GunsPerBattery * CrewPerGun;
            if (slot < crewSlots)
            {
                int gun = slot % GunsPerBattery;
                int member = slot / GunsPerBattery;
                float gunX = (gun - (GunsPerBattery - 1) * 0.5f) * GunSpacingM;
                // serving positions: pairs flanking the piece, then trail
                float side = (member % 2 == 0) ? 1f : -1f;
                float back = 1.6f + 1.5f * (member / 2);
                float lateral = side * (1.1f + 0.35f * (member / 2));
                return new Vector2(
                    gunX + lateral + 0.15f * Jit(unitId, slot, 37),
                    -back + 0.15f * Jit(unitId, slot, 41));
            }
            int rear = slot - crewSlots;
            int rearFiles = Mathf.Max(1, (slotCount - crewSlots + 1) / 2);
            int rr = rear / rearFiles;
            int rf = rear % rearFiles;
            return new Vector2(
                (rf - (rearFiles - 1) * 0.5f) * 1.4f + 0.3f * Jit(unitId, slot, 43),
                -22f - rr * 2.2f + 0.3f * Jit(unitId, slot, 47));
        }

        // Gun muzzle position (unit-local) for battery gun g.
        public static Vector2 GunOffset(int gun) =>
            new Vector2((gun - (GunsPerBattery - 1) * 0.5f) * GunSpacingM, 0f);

        // Unit-local -> world (macro meters). facingDeg 0 = +z, 90 = +x.
        public static Vector2 ToWorld(Vector2 centroid, float facingDeg, Vector2 offset)
        {
            float r = facingDeg * Mathf.Deg2Rad;
            var fwd = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
            var right = new Vector2(fwd.y, -fwd.x);
            return centroid + right * offset.x + fwd * offset.y;
        }

        static float Jit(string unitId, int slot, int salt) =>
            2f * AngleEnvironmentLayout.Hash01(unitId, slot * 97 + salt) - 1f;
    }
}
