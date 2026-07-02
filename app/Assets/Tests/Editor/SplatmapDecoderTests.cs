using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class SplatmapDecoderTests
{
    // 2x2 fixture, PNG row-major (row 0 = north):
    // NW = pure field (R=255), NE = all-zero (pure pasture),
    // SW = all-zero, SE = all-zero.
    static Color32[] Fixture()
    {
        return new Color32[]
        {
            new Color32(255, 0, 0, 0), new Color32(0, 0, 0, 0), // row 0 (north): NW, NE
            new Color32(0, 0, 0, 0),   new Color32(0, 0, 0, 0), // row 1 (south): SW, SE
        };
    }

    [Test]
    public void ToAlphamaps_FlipsRows_FieldPixelLandsAtFlippedY()
    {
        float[,,] a = SplatmapDecoder.ToAlphamaps(Fixture(), 2);
        // NW pixel (raw row 0, col 0) must land at Unity y=1 (north, since
        // y=0 is south) — same flip contract as HeightmapDecoder.
        Assert.AreEqual(1f, a[1, 0, SplatmapDecoder.LayerField], 1e-6f);
        Assert.AreEqual(0f, a[1, 0, SplatmapDecoder.LayerPasture], 1e-6f);
    }

    [Test]
    public void ToAlphamaps_AllZeroPixel_YieldsPastureBase()
    {
        float[,,] a = SplatmapDecoder.ToAlphamaps(Fixture(), 2);
        // SE pixel (raw row 1, col 1) is all-zero -> pasture base = 1.
        Assert.AreEqual(1f, a[0, 1, SplatmapDecoder.LayerPasture], 1e-6f);
        Assert.AreEqual(0f, a[0, 1, SplatmapDecoder.LayerField], 1e-6f);
        Assert.AreEqual(0f, a[0, 1, SplatmapDecoder.LayerWoods], 1e-6f);
        Assert.AreEqual(0f, a[0, 1, SplatmapDecoder.LayerOrchard], 1e-6f);
        Assert.AreEqual(0f, a[0, 1, SplatmapDecoder.LayerMarsh], 1e-6f);
    }

    [Test]
    public void ToAlphamaps_WeightsSumToOne_ForEveryCell()
    {
        float[,,] a = SplatmapDecoder.ToAlphamaps(Fixture(), 2);
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                float sum = 0f;
                for (int layer = 0; layer < SplatmapDecoder.LayerCount; layer++)
                    sum += a[y, x, layer];
                Assert.AreEqual(1f, sum, 1e-5f, $"cell ({y},{x}) weights should sum to 1");
            }
        }
    }
}
