using UnityEngine;

namespace BattleAtlas
{
    // Swaps every terrain layer's diffuse texture between the base
    // relief-modulated albedo and the contour-line variant (the pipeline's
    // relief_contours.png — ~3 m display-scale elevation contours baked a
    // further 4% darker). Both variants are derivatives of the measured DEM,
    // so the toggle is honest in either position; it lives on a HUD chip
    // next to the reading light. Texture sets are generated and wired by
    // LandcoverImporter (index-aligned with terrainData.terrainLayers).
    public class ReliefContourToggle : MonoBehaviour
    {
        public Terrain terrain;
        public Texture2D[] baseAlbedos;
        public Texture2D[] contourAlbedos;

        public bool Contours { get; private set; }

        public void SetContours(bool contours)
        {
            if (contours == Contours) return;
            Contours = contours;
            if (terrain == null || terrain.terrainData == null) return;

            TerrainLayer[] layers = terrain.terrainData.terrainLayers;
            Texture2D[] set = contours ? contourAlbedos : baseAlbedos;
            if (set == null) return;
            // NOTE: TerrainLayer is an asset, so in the editor a play-mode
            // toggle left ON persists on the layer assets after exiting play;
            // the next toggle or land-cover re-import restores the base set.
            // Harmless (both variants are honest), but don't be surprised.
            for (int i = 0; i < layers.Length && i < set.Length; i++)
                layers[i].diffuseTexture = set[i];
        }
    }
}
