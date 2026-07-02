using UnityEngine;

namespace BattleAtlas
{
    // Decodes the pipeline's relief bake (data/landcover/relief.png, byte ->
    // luminance multiplier per relief.json's encode constants — see
    // pipeline/terrain_pipeline/relief.py) and multiplies it into a terrain
    // layer's tint. Every band in the bake is a derivative of the measured
    // DEM (sky-view factor, curvature, contour level sets), so the darkening
    // this produces is a rendering of the record, not paint.
    public static class ReliefDecoder
    {
        // relief.json's "decode" contract:
        // multiplier = encode_min + byte / 255 * (encode_max - encode_min)
        public static float DecodeMultiplier(byte value, float encodeMin, float encodeMax) =>
            encodeMin + value / 255f * (encodeMax - encodeMin);

        // Layer albedo = tint x relief, at the bake's (= splatmap's)
        // resolution. Row-flip contract: relief.png (row 0 = north) arrives
        // through the same LoadImage/GetPixels32 path as splatmap.png, so
        // rows flip here EXACTLY as in SplatmapDecoder.ToAlphamaps — the
        // output array is ready for SetPixels32, and with the layer tiled
        // once over the full terrain extent its texels sit on the same
        // ground points as the splat cells they modulate.
        public static Color32[] ModulatedAlbedo(
            Color32 tint, Color32[] reliefPixels, int resolution,
            float encodeMin, float encodeMax)
        {
            if (reliefPixels.Length != resolution * resolution)
                throw new System.ArgumentException(
                    $"relief pixels length {reliefPixels.Length} != {resolution}^2");

            var albedo = new Color32[resolution * resolution];
            for (int row = 0; row < resolution; row++)
            {
                int dstRow = resolution - 1 - row;
                for (int x = 0; x < resolution; x++)
                {
                    // 8-bit grayscale bake: r == g == b, read r
                    float m = DecodeMultiplier(
                        reliefPixels[row * resolution + x].r, encodeMin, encodeMax);
                    // alpha stays 0: URP Terrain Lit reads smoothness from
                    // the diffuse alpha (LandcoverImporter's anti-gloss
                    // trick) — the relief bake must not resurrect wet-look
                    // terrain
                    albedo[dstRow * resolution + x] = new Color32(
                        MulByte(tint.r, m), MulByte(tint.g, m), MulByte(tint.b, m), 0);
                }
            }
            return albedo;
        }

        static byte MulByte(byte channel, float multiplier) =>
            (byte)Mathf.Min(255, Mathf.RoundToInt(channel * multiplier));
    }
}
