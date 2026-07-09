using System;
using UnityEngine;

namespace BattleAtlas
{
    // Sidecar metadata for a true-scale local terrain crop (pipeline `crop`
    // command, data/heightmap_angle/heightmap.json). Field names match the
    // JSON keys for JsonUtility.
    [Serializable]
    public class AngleTerrainMetadata
    {
        public int resolution;
        public float width_m;
        public float depth_m;
        public float min_elev_m;
        public float max_elev_m;
        public double origin_utm_e;
        public double origin_utm_n;
        public string crs;
        public string row0;
        public float crop_x0_m;
        public float crop_z0_m;
        public float crop_x1_m;
        public float crop_z1_m;
        public double macro_origin_utm_e;
        public double macro_origin_utm_n;
        public float sample_spacing_m;
        public float vertical_exaggeration;
        public string surface;
    }

    // Coordinate frame of a local crop, mirroring pipeline/terrain_pipeline/
    // crop.py: battlefield-local ("macro") x/z are meters east/north of the
    // macro heightmap's UTM origin — the Unity Atlas world frame — and
    // crop-local x/z are meters east/north of the crop window's own origin
    // (x0, z0). The crop terrain sits at the crop scene's world origin, so
    // crop-local coordinates ARE world coordinates there.
    public static class AngleTerrainFrame
    {
        public static Vector2 CropLocalToMacro(Vector2 cropLocal, AngleTerrainMetadata m) =>
            new Vector2(m.crop_x0_m + cropLocal.x, m.crop_z0_m + cropLocal.y);

        public static Vector2 MacroToCropLocal(Vector2 macro, AngleTerrainMetadata m) =>
            new Vector2(macro.x - m.crop_x0_m, macro.y - m.crop_z0_m);

        // UTM easting/northing in doubles: metre-scale UTM coordinates burn
        // float precision (~0.25 m at 4.4e6), and the round-trip contract is
        // exactness within a millimeter.
        public static (double e, double n) MacroToUtm(Vector2 macro, AngleTerrainMetadata m) =>
            (m.macro_origin_utm_e + macro.x, m.macro_origin_utm_n + macro.y);

        public static Vector2 UtmToMacro(double e, double n, AngleTerrainMetadata m) =>
            new Vector2((float)(e - m.macro_origin_utm_e), (float)(n - m.macro_origin_utm_n));

        // Fail loudly on contract drift (plan §8.1: all V2 work is 1.0x).
        public static void ValidateTrueScale(AngleTerrainMetadata m)
        {
            if (m.vertical_exaggeration != 1f)
                throw new InvalidOperationException(
                    $"crop metadata vertical_exaggeration {m.vertical_exaggeration} != 1.0; " +
                    "true-scale terrain is a locked V2 decision");
            if (m.row0 != "north")
                throw new InvalidOperationException(
                    $"crop metadata row0 '{m.row0}' != 'north'; decoder assumes north-first rows");
            if (m.sample_spacing_m > 1.0001f)
                throw new InvalidOperationException(
                    $"crop sample spacing {m.sample_spacing_m} m exceeds the ~1 m acceptance");
        }
    }
}
