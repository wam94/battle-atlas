using System;
using UnityEngine;

namespace BattleAtlas
{
    // The day/phase manifest (ADR 0005; docs/format/battle-manifest.md):
    // days > phases > (a battle file | an honest not-reconstructed note).
    // Parsed with the MomentSet discipline — structural lies throw, so a
    // manifest that invents content or hides emptiness never renders.
    // Authored at StreamingAssets/Atlas/battle-manifest.json; the tool-side
    // twin of these rules is validateManifest (tool/src/validate.ts).
    [Serializable]
    public class PhaseManifest
    {
        public string name;
        public DayDto[] days;

        public const string StatusReconstructed = "reconstructed";
        public const string StatusNotReconstructed = "not-reconstructed";

        public static PhaseManifest FromJson(string json)
        {
            var manifest = JsonUtility.FromJson<PhaseManifest>(json);
            if (manifest?.days == null || manifest.days.Length == 0)
                throw new ArgumentException("battle-manifest.json: missing 'days' array");
            var dayIds = new System.Collections.Generic.HashSet<string>();
            var phaseIds = new System.Collections.Generic.HashSet<string>();
            string prevDate = null;
            foreach (DayDto day in manifest.days)
            {
                if (string.IsNullOrEmpty(day.id) || string.IsNullOrEmpty(day.label)
                    || string.IsNullOrEmpty(day.date))
                    throw new ArgumentException("day: id, label, and date are required");
                if (!dayIds.Add(day.id))
                    throw new ArgumentException($"duplicate day id '{day.id}'");
                if (prevDate != null && string.CompareOrdinal(day.date, prevDate) <= 0)
                    throw new ArgumentException(
                        $"day '{day.id}': dates must strictly increase ({day.date} after {prevDate})");
                prevDate = day.date;
                if (day.phases == null || day.phases.Length == 0)
                    throw new ArgumentException($"day '{day.id}': at least one phase required");
                float prevEnd = float.NegativeInfinity;
                bool anyWindow = false;
                foreach (PhaseDto phase in day.phases)
                {
                    if (string.IsNullOrEmpty(phase.id) || string.IsNullOrEmpty(phase.label))
                        throw new ArgumentException($"day '{day.id}': phase id and label are required");
                    if (!phaseIds.Add(phase.id))
                        throw new ArgumentException($"duplicate phase id '{phase.id}'");
                    if (phase.status == StatusReconstructed)
                    {
                        if (string.IsNullOrEmpty(phase.battle))
                            throw new ArgumentException(
                                $"phase '{phase.id}': reconstructed requires a battle file");
                        if (phase.endTime <= 0f)
                            throw new ArgumentException(
                                $"phase '{phase.id}': reconstructed requires endTime > 0");
                        // no [startTime, startTime+endTime) overlap within a day
                        if (anyWindow && phase.startTime < prevEnd)
                            throw new ArgumentException(
                                $"day '{day.id}': reconstructed phase windows overlap at '{phase.id}'");
                        prevEnd = Mathf.Max(prevEnd, phase.startTime + phase.endTime);
                        anyWindow = true;
                    }
                    else if (phase.status == StatusNotReconstructed)
                    {
                        // the honesty rule: an empty phase must say so, and can
                        // never smuggle in content
                        if (string.IsNullOrEmpty(phase.note))
                            throw new ArgumentException(
                                $"phase '{phase.id}': not-reconstructed requires an honest note");
                        if (!string.IsNullOrEmpty(phase.battle))
                            throw new ArgumentException(
                                $"phase '{phase.id}': not-reconstructed may not reference a battle file");
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"phase '{phase.id}': unknown status '{phase.status}'");
                    }
                }
            }
            return manifest;
        }

        // The day owning the reconstructed phase whose battle file matches the
        // loaded battle asset (TextAsset name = filename sans extension);
        // -1 when none matches (e.g. a test fixture battle).
        public int ActiveDayIndex(string battleAssetName)
        {
            for (int i = 0; i < days.Length; i++)
                foreach (PhaseDto phase in days[i].phases)
                    if (phase.MatchesBattleAsset(battleAssetName))
                        return i;
            return -1;
        }

        public PhaseDto PhaseForBattle(string battleAssetName)
        {
            foreach (DayDto day in days)
                foreach (PhaseDto phase in day.phases)
                    if (phase.MatchesBattleAsset(battleAssetName))
                        return phase;
            return null;
        }

        // The manifest may never lie about a phase's clock
        // (battle-manifest.md "The honesty rules"): the active phase's echoed
        // startTime/endTime must equal the loaded battle's own values.
        // Returns null when they agree (or nothing matches); a human sentence
        // when the echo is broken — the caller warns loudly and shows it.
        public string ClockMismatch(string battleAssetName, float startTime, float endTime)
        {
            PhaseDto phase = PhaseForBattle(battleAssetName);
            if (phase == null) return null;
            if (Mathf.Approximately(phase.startTime, startTime)
                && Mathf.Approximately(phase.endTime, endTime))
                return null;
            return $"battle-manifest.json phase '{phase.id}' claims clock "
                + $"{phase.startTime}+{phase.endTime} but the loaded battle is "
                + $"{startTime}+{endTime} — the manifest's echo is stale; fix the manifest";
        }
    }

    [Serializable]
    public class DayDto
    {
        public string id;
        public string label; // the day tab's word ("July 1")
        public string date;  // YYYY-MM-DD
        public PhaseDto[] phases;
    }

    [Serializable]
    public class PhaseDto
    {
        public string id;
        public string label;
        public string status;    // reconstructed | not-reconstructed
        public string battle;    // battle-file name (reconstructed only)
        public float startTime;  // echo of the battle file's startTime
        public float endTime;    // echo of the battle file's endTime
        public string note;      // the honest empty state (not-reconstructed)

        public bool Reconstructed => status == PhaseManifest.StatusReconstructed;

        public bool MatchesBattleAsset(string battleAssetName)
            => Reconstructed && !string.IsNullOrEmpty(battleAssetName)
               && battle == battleAssetName + ".json";
    }
}
