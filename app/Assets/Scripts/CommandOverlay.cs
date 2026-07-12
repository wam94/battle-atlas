using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // The command overlay (cartography slice 1): unit id -> corps /
    // division words, GENERATED from the audited OOB register by
    // scripts/gen-command-overlay.py (display names verbatim from the
    // register's parentChain — never invented here). The altitude-driven
    // label policy reads corps at theater height and divisions at mid
    // height; the battle file itself carries no such records, so this
    // sidecar rides Resources (Battle/command-overlay) and its absence
    // degrades gracefully — no aggregate labels, everything else intact.

    [Serializable]
    public class CommandOverlayUnitDto
    {
        public string id;
        public string corps;
        public string division; // optional; empty = corps-direct unit
    }

    [Serializable]
    public class CommandOverlayDoc
    {
        public List<CommandOverlayUnitDto> units;
    }

    // The parsed overlay folded into flat group tables: every distinct
    // corps/division becomes one aggregate-label group in first-appearance
    // order (the generator sorts units by id, so the order is stable), and
    // each known unit id resolves to its group indices. Labels are stored
    // UPPERCASED — the cartographic register for command grains, applied
    // once here so the per-frame stamp never allocates. Pure — EditMode-
    // testable without a scene.
    public sealed class CommandGroups
    {
        public readonly List<string> CorpsLabels = new();
        public readonly List<bool> CorpsIsUnion = new();
        public readonly List<string> DivisionLabels = new();
        public readonly List<bool> DivisionIsUnion = new();
        readonly Dictionary<string, (int corps, int division)> byUnit = new();

        public int CorpsCount => CorpsLabels.Count;
        public int DivisionCount => DivisionLabels.Count;

        // (-1, -1) for a unit the overlay doesn't know — it simply never
        // joins an aggregate label.
        public (int corps, int division) GroupsOf(string unitId) =>
            byUnit.TryGetValue(unitId ?? "", out var g) ? g : (-1, -1);

        // isUnionById: the battle data's side per unit (the overlay never
        // re-states side; the battle file owns it). Aggregate side = the
        // side of its first member — corps never mix sides at this grain.
        public static CommandGroups Build(
            CommandOverlayDoc doc, Func<string, bool?> isUnionById)
        {
            var result = new CommandGroups();
            if (doc?.units == null) return result;
            var corpsIndex = new Dictionary<string, int>();
            var divisionIndex = new Dictionary<string, int>();
            foreach (CommandOverlayUnitDto u in doc.units)
            {
                if (string.IsNullOrEmpty(u.id)) continue;
                bool? side = isUnionById(u.id);
                if (side == null) continue; // overlay knows a unit the battle doesn't
                int ci = -1, di = -1;
                if (!string.IsNullOrEmpty(u.corps))
                {
                    if (!corpsIndex.TryGetValue(u.corps, out ci))
                    {
                        ci = result.CorpsLabels.Count;
                        corpsIndex[u.corps] = ci;
                        result.CorpsLabels.Add(u.corps.ToUpperInvariant());
                        result.CorpsIsUnion.Add(side.Value);
                    }
                }
                if (!string.IsNullOrEmpty(u.division))
                {
                    if (!divisionIndex.TryGetValue(u.division, out di))
                    {
                        di = result.DivisionLabels.Count;
                        divisionIndex[u.division] = di;
                        result.DivisionLabels.Add(u.division.ToUpperInvariant());
                        result.DivisionIsUnion.Add(side.Value);
                    }
                }
                result.byUnit[u.id] = (ci, di);
            }
            return result;
        }
    }
}
