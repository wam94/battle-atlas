using System;

namespace BattleAtlas
{
    [Serializable]
    public class HeightmapMetadata
    {
        // field names match the pipeline's heightmap.json keys (JsonUtility maps by name;
        // extra JSON keys like origin_utm_e are ignored)
        public int resolution;
        public float width_m;
        public float depth_m;
        public float min_elev_m;
        public float max_elev_m;
        public string crs;
        public string row0;
    }
}
