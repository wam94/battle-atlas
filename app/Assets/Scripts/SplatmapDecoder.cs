using UnityEngine;

namespace BattleAtlas
{
    public static class SplatmapDecoder
    {
        // Layer order in the output alphamap's 3rd axis.
        public const int LayerPasture = 0;
        public const int LayerField = 1;
        public const int LayerWoods = 2;
        public const int LayerOrchard = 3;
        public const int LayerMarsh = 4;
        public const int LayerCount = 5;

        // PNG convention (see pipeline/terrain_pipeline/landcover.py docstring):
        // row 0 = north, channels R=field G=woodlot(woods) B=orchard A=marsh,
        // pasture is the implied base (1 - sum of the four). SAME row-flip
        // contract as HeightmapDecoder: Unity alphamaps[0, *, *] is the SOUTH
        // edge, so rows flip here exactly as they do there.
        public static float[,,] ToAlphamaps(Color32[] pixels, int resolution)
        {
            if (pixels.Length != resolution * resolution)
                throw new System.ArgumentException(
                    $"pixels length {pixels.Length} != {resolution}^2");

            var alphamaps = new float[resolution, resolution, LayerCount];
            for (int row = 0; row < resolution; row++)
            {
                int dstY = resolution - 1 - row;
                for (int x = 0; x < resolution; x++)
                {
                    Color32 p = pixels[row * resolution + x];
                    float field = p.r / 255f;
                    float woods = p.g / 255f;
                    float orchard = p.b / 255f;
                    float marsh = p.a / 255f;

                    float sum = field + woods + orchard + marsh;
                    // Defensive normalization: if painted weights exceed 1
                    // (shouldn't happen given the pipeline's non-overlapping
                    // "later wins" rasterization, but Unity requires
                    // per-cell weights to sum to 1), scale them down and let
                    // pasture take whatever's left (0 if weights summed >= 1).
                    float pasture;
                    if (sum > 1f)
                    {
                        field /= sum;
                        woods /= sum;
                        orchard /= sum;
                        marsh /= sum;
                        pasture = 0f;
                    }
                    else
                    {
                        pasture = 1f - sum;
                    }

                    alphamaps[dstY, x, LayerPasture] = Mathf.Clamp01(pasture);
                    alphamaps[dstY, x, LayerField] = Mathf.Clamp01(field);
                    alphamaps[dstY, x, LayerWoods] = Mathf.Clamp01(woods);
                    alphamaps[dstY, x, LayerOrchard] = Mathf.Clamp01(orchard);
                    alphamaps[dstY, x, LayerMarsh] = Mathf.Clamp01(marsh);
                }
            }
            return alphamaps;
        }
    }
}
