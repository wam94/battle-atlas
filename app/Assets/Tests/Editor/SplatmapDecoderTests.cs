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

    [Test]
    public void ToAlphamaps_FourLayers_MarshOnBlue_PaintedAlphaThrows()
    {
        // The 4-layer hard constraint (URP Terrain Lit packs 4 layers per
        // pass; a 5th re-rasterizes the whole terrain and silently disables
        // height blending): exactly four layers, marsh riding B per the
        // landcover-format.md channel layout.
        Assert.AreEqual(4, SplatmapDecoder.LayerCount);
        var pixels = new Color32[]
        {
            new Color32(0, 0, 255, 0), new Color32(0, 0, 0, 0), // NW = marsh
            new Color32(0, 0, 0, 0),   new Color32(0, 0, 0, 0),
        };
        float[,,] a = SplatmapDecoder.ToAlphamaps(pixels, 2);
        Assert.AreEqual(4, a.GetLength(2));
        Assert.AreEqual(1f, a[1, 0, SplatmapDecoder.LayerMarsh], 1e-6f);
        Assert.AreEqual(0f, a[1, 0, SplatmapDecoder.LayerWoods], 1e-6f);

        // a painted alpha is the pre-merge 5-layer layout (A carried marsh);
        // decoding it would mislabel the ground — must fail loudly instead
        var stale = Fixture();
        stale[3] = new Color32(0, 0, 0, 255);
        Assert.Throws<System.InvalidOperationException>(
            () => SplatmapDecoder.ToAlphamaps(stale, 2));
    }

    [Test]
    public void ModulatedAlbedo_IsTintTimesRelief_AtProbedTexel()
    {
        // relief.json's encode constants: byte b -> 0.85 + b/255 * 0.3
        const float encodeMin = 0.85f;
        const float encodeMax = 1.15f;
        var tint = new Color32(200, 100, 60, 255);
        Color32[] relief =
        {
            new Color32(255, 255, 255, 255), new Color32(0, 0, 0, 255),   // row 0 (north)
            new Color32(51, 51, 51, 255),    new Color32(102, 102, 102, 255),
        };
        Color32[] albedo = ReliefDecoder.ModulatedAlbedo(tint, relief, 2, encodeMin, encodeMax);

        // North-west texel (input index 0, byte 255 -> x1.15) lands at the
        // flipped output row (index 2) — same flip contract as ToAlphamaps.
        Assert.AreEqual(230, albedo[2].r); // 200 * 1.15
        Assert.AreEqual(115, albedo[2].g); // 100 * 1.15
        Assert.AreEqual(69, albedo[2].b);  //  60 * 1.15
        // South-west texel (input index 2, byte 51 -> x0.91) lands at index 0.
        Assert.AreEqual(182, albedo[0].r); // 200 * 0.91
        Assert.AreEqual(91, albedo[0].g);  // 100 * 0.91
        Assert.AreEqual(55, albedo[0].b);  //  60 * 0.91, rounded
        // alpha stays 0 everywhere: the anti-gloss trick must survive
        foreach (Color32 texel in albedo)
            Assert.AreEqual(0, texel.a);
    }
}
