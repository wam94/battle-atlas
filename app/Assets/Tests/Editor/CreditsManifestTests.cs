using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using BattleAtlas;

// Pins the in-app credits view's data (Phase 11): the committed
// StreamingAssets/credits.json (generated from the third-party manifest by
// generate_attribution.py — the reconstruction suite enforces staleness)
// parses, carries attribution identity for every entry, and the runtime
// attribution line renders the generated document's sentence shape.
public class CreditsManifestTests
{
    [Test]
    public void CommittedCreditsJson_ParsesWithFullAttribution()
    {
        string path = UnityEngine.Application.dataPath + "/StreamingAssets/credits.json";
        CreditsManifest credits = CreditsManifest.FromJson(File.ReadAllText(path));
        Assert.AreEqual("app/Assets/ThirdParty/manifest.json", credits.generatedFrom);
        Assert.IsNotEmpty(credits.manifestSha256);
        Assert.Greater(credits.assets.Length, 0);
        foreach (CreditEntry e in credits.assets)
        {
            Assert.IsNotEmpty(e.title, e.id);
            Assert.IsNotEmpty(e.author, e.id);
            Assert.IsNotEmpty(e.license, e.id);
            Assert.IsNotEmpty(e.licenseUrl, e.id);
        }
        // only redistributable license families ever reach the credits view
        Assert.IsTrue(credits.assets.All(e =>
            e.license.StartsWith("CC0", StringComparison.Ordinal)
            || e.license.StartsWith("CC-BY", StringComparison.Ordinal)));
    }

    [Test]
    public void AttributionLine_SentenceShape()
    {
        var entry = new CreditEntry
        {
            id = "x", title = "Springfield 1861", author = "enKi",
            license = "CC-BY-4.0", sourceUrl = "https://example.test/m",
            modified = true, modifications = "retopology",
        };
        Assert.AreEqual(
            "“Springfield 1861” by enKi, licensed under CC-BY-4.0. "
            + "Source: https://example.test/m — modified: retopology",
            entry.AttributionLine());
        entry.modified = false;
        entry.sourceUrl = "";
        Assert.AreEqual("“Springfield 1861” by enKi, licensed under CC-BY-4.0.",
            entry.AttributionLine());
    }

    [Test]
    public void MalformedCredits_AreRejected()
    {
        Assert.Throws<ArgumentException>(() => CreditsManifest.FromJson("{}"));
        Assert.Throws<ArgumentException>(() => CreditsManifest.FromJson(
            "{\"assets\":[{\"id\":\"a\",\"title\":\"t\",\"author\":\"\"," +
            "\"license\":\"CC0-1.0\"}]}"));
    }
}
