using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class VegetationFieldTests
{
    [Test]
    public void NearTier_LatchesWithHysteresis()
    {
        // in-band below 800, sticky until 1200 (the BattleDirector idiom):
        // a cell that was far stays far inside the band, a cell that was
        // near stays near — so hovering at the boundary never flickers
        Assert.IsTrue(VegetationField.NearTier(wasNear: false, distance: 700f));
        Assert.IsFalse(VegetationField.NearTier(wasNear: false, distance: 1000f));
        Assert.IsTrue(VegetationField.NearTier(wasNear: true, distance: 1000f));
        Assert.IsFalse(VegetationField.NearTier(wasNear: true, distance: 1300f));
    }

    [Test]
    public void BucketFor_SelectsOrchardVariantAndSpreadsWoodlotHues()
    {
        // orchard-class trees always take the orchard bucket (their own
        // near mesh + brighter tint), regardless of index
        Assert.AreEqual(VegetationField.OrchardBucket, VegetationField.BucketFor("orchard", 0));
        Assert.AreEqual(VegetationField.OrchardBucket, VegetationField.BucketFor("orchard", 999));
        // woodlot trees hash deterministically into the hue buckets, and a
        // realistic stand actually uses all of them
        var seen = new bool[VegetationField.HueBucketCount];
        for (int i = 0; i < 200; i++)
        {
            int bucket = VegetationField.BucketFor("woodlot", i);
            Assert.AreEqual(bucket, VegetationField.BucketFor("woodlot", i));
            Assert.GreaterOrEqual(bucket, 0);
            Assert.Less(bucket, VegetationField.HueBucketCount);
            seen[bucket] = true;
        }
        foreach (bool hit in seen) Assert.IsTrue(hit);
    }

    [Test]
    public void Placements_DeterministicAndInsideRadius()
    {
        var a = VegetationField.Placements("copse", new Vector2(4407f, 4801f), 40f, 40);
        var b = VegetationField.Placements("copse", new Vector2(4407f, 4801f), 40f, 40);
        Assert.AreEqual(40, a.Length);
        for (int i = 0; i < a.Length; i++)
        {
            Assert.AreEqual(a[i].x, b[i].x, 1e-5f);
            Assert.LessOrEqual(Vector2.Distance(a[i], new Vector2(4407f, 4801f)), 40.01f);
        }
    }
}
