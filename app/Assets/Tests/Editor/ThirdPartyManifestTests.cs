using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unity-side enforcement of the third-party asset manifest (Angle
/// Reconstruction V2 plan §11). The authoritative validator (schema,
/// license allowlist, checksums, evidence) is
/// reconstruction/scripts/validate_assets.py, run by the reconstruction
/// pytest suite; these tests make the same structural violations fail the
/// Unity EditMode suite too, so a manifest break cannot hide from either
/// side of the project.
/// </summary>
public class ThirdPartyManifestTests
{
    [Serializable]
    private class Manifest
    {
        public Entry[] assets;
    }

    [Serializable]
    private class Entry
    {
        public string id;
        public string path;
        public string title;
        public string author;
        public string sourceUrl;
        public string license;
        public string licenseUrl;
        public string acquired;
        public bool redistributable;
        public bool modified;
        public string modifications;
        public string sha256;
        public string downloadUrl;
        public string downloadSha256;
        public string licenseEvidence;
    }

    private static readonly string[] AllowedLicenses = { "CC0-1.0", "CC-BY-3.0", "CC-BY-4.0" };
    private static readonly string[] ForbiddenUrlSubstrings = { "assetstore.unity.com", "mixamo.com" };

    private static string ThirdPartyDir =>
        Path.Combine(Application.dataPath, "ThirdParty");

    private static string ManifestPath =>
        Path.Combine(ThirdPartyDir, "manifest.json");

    private static Manifest Load()
    {
        Assert.IsTrue(File.Exists(ManifestPath),
            "Assets/ThirdParty/manifest.json is missing (plan §11)");
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
        Assert.IsNotNull(manifest.assets, "manifest has no assets array");
        return manifest;
    }

    private static IEnumerable<string> ContentFiles(string dir) =>
        Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta", StringComparison.Ordinal)
                        && Path.GetFileName(f) != ".DS_Store");

    [Test]
    public void Manifest_ParsesWithAllRequiredFields()
    {
        var manifest = Load();
        foreach (var a in manifest.assets)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(a.id), "asset id blank");
            foreach (var (name, value) in new[]
                     {
                         ("path", a.path), ("title", a.title), ("author", a.author),
                         ("sourceUrl", a.sourceUrl), ("license", a.license),
                         ("licenseUrl", a.licenseUrl), ("acquired", a.acquired),
                         ("sha256", a.sha256), ("downloadUrl", a.downloadUrl),
                         ("downloadSha256", a.downloadSha256),
                         ("licenseEvidence", a.licenseEvidence),
                     })
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(value),
                    $"{a.id}: required field {name} is blank");
            }
            Assert.IsTrue(a.redistributable,
                $"{a.id}: redistributable is false (locked decision 12)");
            if (a.modified)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(a.modifications),
                    $"{a.id}: modified is true but modifications is empty");
            }
        }
    }

    [Test]
    public void Manifest_LicensesAreOnTheAllowlist()
    {
        foreach (var a in Load().assets)
        {
            Assert.Contains(a.license, AllowedLicenses,
                $"{a.id}: license '{a.license}' is not CC0/CC-BY (plan §11)");
        }
    }

    [Test]
    public void Manifest_NoAssetStoreOrMixamoSources()
    {
        foreach (var a in Load().assets)
        {
            foreach (var url in new[] { a.sourceUrl, a.downloadUrl, a.licenseUrl })
            {
                foreach (var bad in ForbiddenUrlSubstrings)
                {
                    StringAssert.DoesNotContain(bad, url.ToLowerInvariant(),
                        $"{a.id}: {bad} may not be a required build input");
                }
            }
        }
    }

    [Test]
    public void EveryThirdPartyFile_HasExactlyOneManifestOwner()
    {
        var manifest = Load();
        var ownedDirs = manifest.assets
            .Select(a => Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", a.path)))
            .ToArray();
        foreach (var dir in ownedDirs)
        {
            Assert.IsTrue(Directory.Exists(dir), $"manifested path missing: {dir}");
        }

        foreach (var file in ContentFiles(ThirdPartyDir))
        {
            var full = Path.GetFullPath(file);
            if (full == Path.GetFullPath(ManifestPath)) continue;
            int owners = ownedDirs.Count(d =>
                full.StartsWith(d + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            Assert.AreEqual(1, owners,
                $"unmanifested or multiply-owned file under ThirdParty: {full}");
        }
    }
}
