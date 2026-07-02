using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // JSON DTOs for battle track data. Conventions:
    // positions are battlefield-local meters (x east, z north from the terrain
    // SW corner = Unity world XZ); t is seconds from battle start; facing is
    // compass degrees (0 = north = Unity +Z).
    [Serializable]
    public class BattleDto
    {
        public string name;
        public float startTime;
        public float endTime;
        public List<UnitDto> units;
        public List<EventDto> events; // optional; JsonUtility leaves this null when absent = no events
        // optional; JsonUtility deserializes an absent block as a ZEROED
        // instance (never null) — windMps 0 means calm = no drift, which is
        // exactly the right fallback, so the quirk is load-bearing
        public EnvironmentDto environment;
    }

    // Engagement event: a provenance-gated fire window (battle-format.md
    // "Engagement events"). Exactly one emitter form; JsonUtility can't
    // express absent-vs-zero, so the form is keyed on non-empty unitId:
    // set = moving emitter (position from that unit's track at emission
    // time), empty = the x/z..x2/z2 fixed segment. Dust is never authored —
    // it derives from unit velocity (no "advance_dust" kind, by design).
    [Serializable]
    public class EventDto
    {
        public string id;
        public string kind; // "artillery_fire" | "musketry"
        public float t0;
        public float t1;
        public string unitId; // emitter form A; empty = segment form
        public float x;       // emitter form B: fixed segment, battlefield-local meters
        public float z;
        public float x2;
        public float z2;
        public string confidence; // "documented" | "inferred" | "unknown"; empty = unknown
        public string citation;
        public string note;
    }

    // Battlefield wind (battle-format.md "Environment"): windTowardDeg is
    // the compass bearing smoke drifts TOWARD (not the meteorological
    // from-direction).
    [Serializable]
    public class EnvironmentDto
    {
        public float windTowardDeg;
        public float windMps;
        public string confidence;
        public string citation;
        public string note;
    }

    [Serializable]
    public class UnitDto
    {
        public string id;
        public string name;
        public string side; // "union" | "confederate"
        public float frontage_m;
        public float depth_m;
        public List<string> regiments; // optional display roster; JsonUtility leaves this null when absent = no roster
        public string parent; // optional id of the unit this decomposes (depth 1); JsonUtility leaves this null/empty when absent
        public List<KeyframeDto> keyframes;
    }

    [Serializable]
    public class KeyframeDto
    {
        public float t;
        public float x;
        public float z;
        public float facing;
        public string formation;
        public float strength;
        public string confidence; // "documented" | "inferred" | "unknown"; empty = unknown
        public string citation;
    }

    public static class BattleLoader
    {
        public static BattleDto Parse(string json)
        {
            BattleDto battle = JsonUtility.FromJson<BattleDto>(json);
            if (battle.units == null || battle.units.Count == 0)
                throw new ArgumentException("battle has no units");
            foreach (UnitDto unit in battle.units)
            {
                if (unit.keyframes == null || unit.keyframes.Count == 0)
                    throw new ArgumentException($"unit '{unit.id}' has no keyframes");
                for (int i = 1; i < unit.keyframes.Count; i++)
                {
                    if (unit.keyframes[i].t <= unit.keyframes[i - 1].t)
                        throw new ArgumentException(
                            $"unit '{unit.id}' keyframe times must strictly increase " +
                            $"(index {i}: {unit.keyframes[i].t} <= {unit.keyframes[i - 1].t})");
                }
                if (unit.frontage_m <= 0f || unit.depth_m <= 0f)
                    throw new ArgumentException(
                        $"unit '{unit.id}' frontage/depth must be positive");
                KeyframeDto lastKf = unit.keyframes[unit.keyframes.Count - 1];
                if (lastKf.t > battle.endTime)
                    throw new ArgumentException(
                        $"unit '{unit.id}' keyframe t {lastKf.t} exceeds battle endTime {battle.endTime}");
            }
            // parent/children rules (battle-format.md "Parent / children"):
            // parent must exist, depth 1 only, and a unit with children must
            // not also carry a regiments roster (full decomposition or none)
            var byId = new Dictionary<string, UnitDto>();
            foreach (UnitDto unit in battle.units)
                byId[unit.id] = unit;
            foreach (UnitDto unit in battle.units)
            {
                if (string.IsNullOrEmpty(unit.parent))
                    continue;
                if (!byId.TryGetValue(unit.parent, out UnitDto parent))
                    throw new ArgumentException(
                        $"unit '{unit.id}' parent '{unit.parent}' does not exist");
                if (!string.IsNullOrEmpty(parent.parent))
                    throw new ArgumentException(
                        $"unit '{unit.id}' parent '{unit.parent}' has a parent itself (depth 1 only)");
                if (parent.regiments != null && parent.regiments.Count > 0)
                    throw new ArgumentException(
                        $"unit '{parent.id}' has children but still carries a regiments roster " +
                        "(full decomposition or none)");
            }
            // engagement event rules (battle-format.md "Engagement events"):
            // kind vocabulary, t0 < t1 <= endTime, and a non-empty unitId
            // must resolve — the load-bearing subset the renderer trusts
            if (battle.events != null)
            {
                foreach (EventDto ev in battle.events)
                {
                    if (ev.kind != "artillery_fire" && ev.kind != "musketry")
                        throw new ArgumentException(
                            $"event '{ev.id}' has unknown kind '{ev.kind}'");
                    if (ev.t0 >= ev.t1)
                        throw new ArgumentException(
                            $"event '{ev.id}' window must satisfy t0 < t1 ({ev.t0} >= {ev.t1})");
                    if (ev.t1 > battle.endTime)
                        throw new ArgumentException(
                            $"event '{ev.id}' t1 {ev.t1} exceeds battle endTime {battle.endTime}");
                    if (!string.IsNullOrEmpty(ev.unitId) && !byId.ContainsKey(ev.unitId))
                        throw new ArgumentException(
                            $"event '{ev.id}' unitId '{ev.unitId}' does not exist");
                }
            }
            return battle;
        }
    }
}
