using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // The Phase 11 source/provenance drawer's data build (plan §10): tap a
    // unit and see its identity, strength, current activity, and the
    // claims/citations behind its track. Pure static composition over the
    // battle JSON's own provenance fields (keyframe confidence + citation,
    // engagement-event citations) so the EditMode suite pins every line —
    // the drawer can only show what the record carries, and where the
    // record is silent it says so (the no-faking gate's display text,
    // inherited from the IMGUI citation-line seed this replaces).
    public static class ProvenanceDrawer
    {
        // Where the record is silent, the line says so — never invents.
        public const string NoReliableRecord = "no reliable record";

        // The citation carried by the bracketing START keyframe at t —
        // exactly UnitTrack's formation/confidence carry rule (a segment
        // cites its start; the clamps cite the track's ends). Moved verbatim
        // from the IMGUI TimelineHud seed (atlas-cartography Task 6).
        public static string CitationAt(List<KeyframeDto> keyframes, float t)
        {
            KeyframeDto bracketing = keyframes[0];
            for (int i = 1; i < keyframes.Count; i++)
            {
                if (keyframes[i].t > t) break;
                bracketing = keyframes[i];
            }
            return bracketing.citation;
        }

        public static string EchelonWord(UnitSymbol.Echelon echelon)
        {
            switch (echelon)
            {
                case UnitSymbol.Echelon.Regiment: return "regiment";
                case UnitSymbol.Echelon.Battery: return "battery";
                case UnitSymbol.Echelon.Park: return "park";
                default: return "brigade";
            }
        }

        public static string ArmWord(BattleDirector.UnitKind kind)
        {
            switch (kind)
            {
                case BattleDirector.UnitKind.Artillery: return "artillery";
                case BattleDirector.UnitKind.Cavalry: return "cavalry";
                default: return "infantry";
            }
        }

        public static string SideWord(bool isUnion) =>
            isUnion ? "Union" : "Confederate";

        // Confidence display: the 2026-07-09 ruling folds "unknown" into
        // inferred for this project — the drawer says which word the DATA
        // carries, because the drawer exists to be inspected. Contested is
        // V2-bundle vocabulary, passed through when it arrives.
        public static string ConfidenceWord(string confidence)
        {
            if (string.IsNullOrEmpty(confidence) || confidence == "unknown")
                return "inferred (unrecorded)";
            return confidence;
        }

        public static string StrengthLine(float strength) =>
            $"{Mathf.RoundToInt(strength):n0} men";

        // The activity line: movement + formation, plus any live authored
        // fire events. Deterministic in t (the moving flag is the caller's
        // symmetric-window sample, the same one the symbol salience uses).
        public static string ActivityLine(
            bool moving, string formation, IReadOnlyList<EventDto> events, float t)
        {
            string what = moving ? "moving" : "holding";
            switch (formation)
            {
                case "column": what += " in column"; break;
                case "line": what += " in line"; break;
                case "scattered": what = moving ? "scattered, moving" : "scattered"; break;
                case "routed": what = "routed"; break;
                default:
                    if (!string.IsNullOrEmpty(formation)) what += $" ({formation})";
                    break;
            }
            string fire = null;
            if (events != null)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    EventDto ev = events[i];
                    if (ev.t0 <= t && t <= ev.t1)
                    {
                        string kind = ev.kind == "artillery_fire"
                            ? "firing (artillery)" : "firing (musketry)";
                        // one fire verb is enough; two overlapping windows of
                        // the same kind must not read twice
                        if (fire == null) fire = kind;
                        else if (fire != kind) fire = "firing (artillery, musketry)";
                    }
                }
            }
            return fire == null ? what : $"{what} · {fire}";
        }

        // One drawer source entry: what kind of record, its display text,
        // and whether it is live at the inspected time (events) or the
        // bracketing segment (track).
        public struct SourceEntry
        {
            public string Heading; // e.g. "position/strength at 15:16 (documented)"
            public string Citation;
            public bool Live;
        }

        // The full source list for a unit at t: the bracketing keyframe's
        // citation first (the track segment the display currently shows),
        // then every authored engagement event attached to the unit — live
        // windows first, each with its confidence word and clock window.
        public static List<SourceEntry> SourceEntries(
            UnitDto unit, float t, float clockStartTime,
            IReadOnlyList<EventDto> events)
        {
            var result = new List<SourceEntry>();
            KeyframeDto bracketing = unit.keyframes[0];
            foreach (KeyframeDto kf in unit.keyframes)
            {
                if (kf.t > t) break;
                bracketing = kf;
            }
            result.Add(new SourceEntry
            {
                Heading = "track segment from "
                    + ClockMath.FormatClockTime(clockStartTime, bracketing.t)
                    + $" ({ConfidenceWord(bracketing.confidence)})",
                Citation = string.IsNullOrEmpty(bracketing.citation)
                    ? NoReliableRecord : bracketing.citation,
                Live = true,
            });
            if (events == null) return result;
            // live events first, then the rest in authored order
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    EventDto ev = events[i];
                    bool live = ev.t0 <= t && t <= ev.t1;
                    if (live != (pass == 0)) continue;
                    string kind = ev.kind == "artillery_fire" ? "artillery fire" : "musketry";
                    result.Add(new SourceEntry
                    {
                        Heading = $"{kind} "
                            + ClockMath.FormatClockTime(clockStartTime, ev.t0) + "–"
                            + ClockMath.FormatClockTime(clockStartTime, ev.t1)
                            + $" ({ConfidenceWord(ev.confidence)})"
                            + (live ? " — live" : ""),
                        Citation = string.IsNullOrEmpty(ev.citation)
                            ? NoReliableRecord : ev.citation,
                        Live = live,
                    });
                }
            }
            return result;
        }
    }
}
