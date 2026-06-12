using UnityEngine;

namespace BattleAtlas
{
    // Terrain datum + presentation parameters, attached to the terrain object
    // by the importer so runtime code can recover honest meters from the
    // exaggerated display terrain (LOS, slope, geo-referencing).
    public class BattlefieldInfo : MonoBehaviour
    {
        public float widthM;
        public float depthM;
        public float minElevM;
        public float maxElevM;
        public float originUtmE;
        public float originUtmN;
        public float verticalExaggeration = 1f;

        // displayedHeight: meters above the terrain object's base (what
        // Terrain.SampleHeight returns). Returns true elevation in meters ASL.
        public static float HonestElevation(
            float displayedHeight, float minElevM, float verticalExaggeration)
        {
            return minElevM + displayedHeight / verticalExaggeration;
        }

        public float HonestElevationAt(float displayedHeight) =>
            HonestElevation(displayedHeight, minElevM, verticalExaggeration);
    }
}
