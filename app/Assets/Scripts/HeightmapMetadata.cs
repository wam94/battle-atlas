using System;

namespace BattleAtlas
{
    [Serializable]
    public class HeightmapMetadata
    {
        // field names match the pipeline's heightmap.json keys (JsonUtility maps by name;
        // all keys including origin_utm_e/n are mapped and consumed by BattlefieldInfo)
        public int resolution;
        public float width_m;
        public float depth_m;
        public float min_elev_m;
        public float max_elev_m;
        public float origin_utm_e;
        public float origin_utm_n;
        public string crs;
        public string row0;
    }
}
