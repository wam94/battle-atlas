using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Phase 5 (Reconstruction V2) EditMode smoke test: the compiled tactical
/// sidecar Assets/Battle/Angle/angle.bundle.json exists, parses, and covers
/// the Angle slice. The Unity runtime does not read the bundle yet (Phase 5
/// changes no macro battle behavior; BattleDirector is untouched) — this only
/// proves the artifact ships inside the project and is machine-readable from
/// C#. Full semantic validation lives in reconstruction/tests (pytest).
/// </summary>
public class AngleBundleTests
{
    [System.Serializable]
    class BundleSlice
    {
        public float t0;
        public float t1;
    }

    [System.Serializable]
    class BundleHeader
    {
        public string format;
        public BundleSlice slice;
        public string checksum;
    }

    static string BundlePath =>
        Path.Combine(Application.dataPath, "Battle", "Angle", "angle.bundle.json");

    [Test]
    public void Bundle_ExistsInProject()
    {
        Assert.IsTrue(File.Exists(BundlePath), $"missing {BundlePath}");
    }

    [Test]
    public void Bundle_HeaderParses_AndCoversTheAngleSlice()
    {
        var header = JsonUtility.FromJson<BundleHeader>(File.ReadAllText(BundlePath));
        Assert.AreEqual("angle-bundle/1", header.format);
        Assert.AreEqual(8040f, header.slice.t0);
        Assert.AreEqual(9000f, header.slice.t1);
        Assert.AreEqual(64, header.checksum.Length, "sha256 hex checksum expected");
    }

    [Test]
    public void Bundle_ContainsTheEightNamedAngleUnits()
    {
        string json = File.ReadAllText(BundlePath);
        foreach (string unitId in new[]
        {
            "csa-garnett", "csa-armistead", "csa-kemper",
            "us-webb", "us-69pa", "us-71pa", "us-72pa", "us-btty-cushing",
        })
        {
            StringAssert.Contains($"\"unitId\":\"{unitId}\"", json);
        }
    }

    [Test]
    public void Bundle_DoesNotTouchTheMacroBattleFile()
    {
        // Phase 5 guard: the macro artifact must not reference the sidecar.
        string macro = File.ReadAllText(
            Path.Combine(Application.dataPath, "Battle", "gettysburg-july3.json"));
        StringAssert.DoesNotContain("angle.bundle", macro);
    }
}
