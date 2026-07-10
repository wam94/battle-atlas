using System;
using UnityEngine;

namespace BattleAtlas
{
    // Runtime form of StreamingAssets/credits.json — the in-app
    // attribution/credits view's data (plan §11: generated from
    // app/Assets/ThirdParty/manifest.json by
    // reconstruction/scripts/generate_attribution.py, never hand-written;
    // the reconstruction suite fails when it is stale). The attribution
    // line format mirrors docs/assets/THIRD_PARTY_ASSETS.md's.
    [Serializable]
    public class CreditsManifest
    {
        public string generatedFrom;
        public string manifestSha256;
        public CreditEntry[] assets;

        public static CreditsManifest FromJson(string json)
        {
            var credits = JsonUtility.FromJson<CreditsManifest>(json);
            if (credits?.assets == null)
                throw new ArgumentException("credits.json: missing 'assets' array");
            foreach (CreditEntry e in credits.assets)
            {
                if (string.IsNullOrEmpty(e.title) || string.IsNullOrEmpty(e.author)
                    || string.IsNullOrEmpty(e.license))
                    throw new ArgumentException(
                        $"credits.json: entry '{e.id}' missing title/author/license");
            }
            return credits;
        }
    }

    [Serializable]
    public class CreditEntry
    {
        public string id;
        public string title;
        public string author;
        public string license;
        public string licenseUrl;
        public string sourceUrl;
        public bool modified;
        public string modifications;

        // One credit line, the generated document's sentence shape.
        public string AttributionLine()
        {
            string line = $"“{title}” by {author}, licensed under {license}.";
            if (!string.IsNullOrEmpty(sourceUrl)) line += $" Source: {sourceUrl}";
            if (modified && !string.IsNullOrEmpty(modifications))
                line += $" — modified: {modifications}";
            return line;
        }
    }
}
