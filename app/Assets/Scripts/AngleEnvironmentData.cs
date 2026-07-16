using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // DTOs for data/heightmap_angle/environment.json — the Phase 7 sourced
    // environment bake (pipeline/terrain_pipeline/environment.py). All
    // coordinates are battlefield-local ("macro") meters; the stage converts
    // to crop-local via AngleTerrainFrame. Polylines/polygons are FLAT
    // [x0, z0, x1, z1, ...] arrays because JsonUtility cannot parse nested
    // arrays. Field names match the JSON keys.
    [Serializable]
    public class EnvRoadSample
    {
        public float x;
        public float z;
        public float halfWidth;
    }

    [Serializable]
    public class EnvRoad
    {
        public List<EnvRoadSample> centerline;
        public float depthM;
        public float shoulderM;
        public float wheelPathOffsetM;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvFence
    {
        public string featureId;
        public string style; // "post_and_rail" | "worm"
        public float heightM;
        public float postSpacingM;
        public List<float> polylineFlat;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvWall
    {
        public string featureId;
        public List<float> polylineFlat;
        public float heightM;
        public float widthM;
        public List<float> railsZRange;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvTree
    {
        public float x;
        public float z;
        public float heightM;
        public float yawDeg;
    }

    [Serializable]
    public class EnvGrove
    {
        public string featureId;
        public List<EnvTree> trees;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvBuilding
    {
        public string id;
        public string kind; // "house" | "barn"
        public float x;
        public float z;
        public float yawDeg;
        public List<float> sizeM;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvGun
    {
        public float x;
        public float z;
        public float yawDeg;
        public string state; // "intact" | "disabled" | "wrecked"
        public string at;    // "wall" | "crest"
    }

    [Serializable]
    public class EnvPlacement
    {
        public float x;
        public float z;
        public float yawDeg;
        public string kind;
    }

    [Serializable]
    public class EnvBattery
    {
        public string unitId;
        public List<EnvGun> guns;
        public List<EnvPlacement> limbers;
        public List<EnvPlacement> caissons;
        public List<EnvPlacement> detritus;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvWheat
    {
        public string featureId;
        public List<float> polygonFlat;
        public float clumpSpacingM;
        public string patchNoiseKey;
        public List<string> claimIds;
        public List<string> editorialIds;
    }

    [Serializable]
    public class EnvCrop
    {
        public float x0;
        public float z0;
        public float x1;
        public float z1;
    }

    [Serializable]
    public class EnvSplat
    {
        public string path;
        public int resolution;
        public string row0;
        public string sha256;
    }

    [Serializable]
    public class AngleEnvironment
    {
        public string name;
        public EnvCrop crop;
        public EnvRoad road;
        public List<EnvFence> fences;
        public EnvWall wall;
        public List<EnvGrove> groves;
        public List<EnvGrove> orchards;
        public List<EnvBuilding> buildings;
        public EnvBattery battery;
        public EnvWheat wheat;
        public EnvSplat splat;
    }

    public static class AngleEnvironmentData
    {
        public static AngleEnvironment Parse(string json)
        {
            var env = JsonUtility.FromJson<AngleEnvironment>(json);
            // An explicitly EMPTY centerline is a valid site statement
            // ("this crop has no road corridor" — the Oak Ridge bake);
            // a missing/null road block is still a malformed bake.
            if (env == null || env.road == null || env.road.centerline == null)
                throw new ArgumentException("environment.json has no road block");
            if (env.wall == null || env.wall.polylineFlat == null
                || env.wall.polylineFlat.Count < 4)
                throw new ArgumentException("environment.json has no wall polyline");
            if (env.splat == null || env.splat.resolution <= 0)
                throw new ArgumentException("environment.json has no splat descriptor");
            return env;
        }

        public static List<Vector2> Points(List<float> flat)
        {
            if (flat == null || flat.Count % 2 != 0)
                throw new ArgumentException("flat polyline must have even length");
            var pts = new List<Vector2>(flat.Count / 2);
            for (int i = 0; i < flat.Count; i += 2)
                pts.Add(new Vector2(flat[i], flat[i + 1]));
            return pts;
        }
    }
}
