using System;
using UnityEngine;

namespace BattleAtlas
{
    // Moment markers for the Atlas timeline (plan §10 "moment markers"):
    // a small curated set of clock anchors, each carrying the citation (or
    // the named editorial decision) that places it — the same no-faking
    // rule as every other displayed time. Authored in
    // StreamingAssets/Atlas/moments.json; every entry's provenance traces
    // to the reconstruction corpus (see the file's _comment field).
    [Serializable]
    public class MomentSet
    {
        // optional: the battle ASSET this file's clock addresses (ADR 0005
        // consequence — moments are per-phase; a file naming a battle must
        // never render against another phase's clock). Empty = ungated
        // (pre-slice-2 files).
        public string battle;
        public MomentDto[] moments;

        // May this file's moments render against the given loaded battle
        // asset? (null asset = fixture rigs: gated files do NOT apply.)
        public bool AppliesTo(string battleAssetName) =>
            string.IsNullOrEmpty(battle) || battle == battleAssetName;

        public static MomentSet FromJson(string json)
        {
            var set = JsonUtility.FromJson<MomentSet>(json);
            if (set?.moments == null || set.moments.Length == 0)
                throw new ArgumentException("moments.json: missing 'moments' array");
            for (int i = 0; i < set.moments.Length; i++)
            {
                MomentDto m = set.moments[i];
                if (string.IsNullOrEmpty(m.label))
                    throw new ArgumentException($"moments[{i}]: missing label");
                if (string.IsNullOrEmpty(m.citation))
                    throw new ArgumentException(
                        $"moment '{m.label}': missing citation — a marker without "
                        + "provenance is an invented time");
                if (i > 0 && m.t <= set.moments[i - 1].t)
                    throw new ArgumentException(
                        $"moment '{m.label}': times must strictly increase");
            }
            return set;
        }

        // The phase readout: the label of the last moment at or before t
        // (empty before the first — the masthead shows nothing rather than
        // a phase that hasn't begun).
        public string PhaseAt(float t)
        {
            string phase = "";
            foreach (MomentDto m in moments)
            {
                if (m.t > t) break;
                phase = m.label;
            }
            return phase;
        }
    }

    [Serializable]
    public class MomentDto
    {
        public float t;        // canonical battle-clock seconds
        public string label;   // short marker label ("The charge steps off")
        public string detail;  // one-line hover detail
        public string citation; // claim id / editorial decision that places t
    }
}
