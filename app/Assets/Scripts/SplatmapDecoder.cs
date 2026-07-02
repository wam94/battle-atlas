using UnityEngine;

namespace BattleAtlas
{
    public static class SplatmapDecoder
    {
        // Layer order in the output alphamap's 3rd axis. EXACTLY four:
        // URP's Terrain Lit shader packs 4 layers per pass (one RGBA
        // control map); a 5th layer triggers an add pass that re-rasterizes
        // the ENTIRE terrain and silently disables height-based blending
        // (docs/research/2026-07-02-descriptive-graphics-techniques.md §1a).
        // Orchard is merged into the pasture base at the splat level —
        // orchards read from their baked tree rows, not ground tint.
        public const int LayerPasture = 0;
        public const int LayerField = 1;
        public const int LayerWoods = 2;
        public const int LayerMarsh = 3;
        public const int LayerCount = 4;

        // PNG convention (docs/format/landcover-format.md "Baked splat
        // channels" / pipeline/terrain_pipeline/landcover.py docstring):
        // row 0 = north, channels R=field G=woodlot(woods) B=marsh, A unused
        // (always 0), pasture is the implied base (1 - sum of the three).
        // SAME row-flip contract as HeightmapDecoder: Unity
        // alphamaps[0, *, *] is the SOUTH edge, so rows flip here exactly
        // as they do there.
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
                    // a painted alpha means a pre-4-layer splatmap (A used
                    // to carry marsh) — decoding it would silently mislabel
                    // the ground, so fail loudly on contract drift
                    if (p.a != 0)
                        throw new System.InvalidOperationException(
                            $"splatmap alpha is painted at ({row},{x}); this decoder expects " +
                            "the 4-layer channel layout R=field G=woods B=marsh with A unused " +
                            "(landcover-format.md) — stale splatmap.png? regenerate it with " +
                            "`terrain_pipeline.cli landcover`");
                    float field = p.r / 255f;
                    float woods = p.g / 255f;
                    float marsh = p.b / 255f;

                    float sum = field + woods + marsh;
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
                    alphamaps[dstY, x, LayerMarsh] = Mathf.Clamp01(marsh);
                }
            }
            return alphamaps;
        }
    }
}
