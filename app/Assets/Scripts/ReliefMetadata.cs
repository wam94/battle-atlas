using System;

namespace BattleAtlas
{
    [Serializable]
    public class ReliefMetadata
    {
        // field names match the pipeline's relief.json keys (JsonUtility
        // maps by name) — the self-describing sidecar for relief.png /
        // relief_contours.png, same pattern as heightmap.json.
        public int resolution;
        public string row0;
        public float clamp;
        public float encode_min;
        public float encode_max;
        public string decode;
        public float contour_interval_display_m;
        public float contour_darken;
        public float vertical_exaggeration;
    }
}
