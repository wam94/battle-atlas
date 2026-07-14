using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace BattleAtlas
{
    // JSON DTOs + parser for the pipeline's traced map-furniture output
    // (data/map-furniture/map-furniture.json, docs/format/map-furniture-
    // format.md -- roads, hydrology, town-block massing, railroad).
    //
    // JsonUtility cannot deserialize a jagged/nested array field like
    // "points": [[x,z],[x,z],...] (a well-known Unity limitation -- it
    // silently leaves such a field at its default). The `points` field
    // below is marked [NonSerialized] so JsonUtility parses every OTHER
    // feature field normally and simply skips the one it can't handle;
    // ParsePointsArrays then does a small bracket-depth scan for the
    // "points" arrays in document order (the same order JsonUtility
    // produces for `features`) and zips them back onto the parsed
    // features. This is a special-purpose extractor for this one known
    // shape (flat runs of "[number,number]" pairs), not a general JSON
    // parser.
    [Serializable]
    public class MapFurnitureFeatureDto
    {
        public string id;
        public string kind;   // "line" | "polygon"
        public string cls;
        public string source;
        public string confidence;
        public string note;

        [NonSerialized] public Vector2[] points;
    }

    [Serializable]
    public class MapFurnitureDto
    {
        public string name;
        public List<MapFurnitureFeatureDto> features;
    }

    public static class MapFurnitureData
    {
        public static MapFurnitureDto Parse(string json)
        {
            MapFurnitureDto dto = JsonUtility.FromJson<MapFurnitureDto>(json);
            if (dto == null || dto.features == null || dto.features.Count == 0)
                throw new ArgumentException("map-furniture.json has no features");

            List<Vector2[]> pointsPerFeature = ParsePointsArrays(json);
            if (pointsPerFeature.Count != dto.features.Count)
                throw new InvalidOperationException(
                    $"map-furniture.json: found {pointsPerFeature.Count} \"points\" arrays " +
                    $"but {dto.features.Count} features -- parser desync (unexpected JSON shape)");

            for (int i = 0; i < dto.features.Count; i++)
            {
                dto.features[i].points = pointsPerFeature[i];
                if (dto.features[i].points.Length == 0)
                    throw new InvalidOperationException(
                        $"map-furniture.json: feature '{dto.features[i].id}' has zero points");
            }
            return dto;
        }

        // Scans raw JSON text for every `"points": [ ... ]` block (bracket-
        // depth aware, so the nested per-point [x,z] arrays don't confuse
        // the outer-array boundary) and parses each into a flat Vector2[].
        // Public (not just called from Parse) so EditMode tests can probe
        // the extractor directly.
        public static List<Vector2[]> ParsePointsArrays(string json)
        {
            var result = new List<Vector2[]>();
            int i = 0;
            while (true)
            {
                int keyIdx = json.IndexOf("\"points\"", i, StringComparison.Ordinal);
                if (keyIdx < 0) break;
                int bracketStart = json.IndexOf('[', keyIdx);
                if (bracketStart < 0)
                    throw new InvalidOperationException("map-furniture.json: \"points\" with no '['");

                int depth = 0, end = -1;
                int j = bracketStart;
                for (; j < json.Length; j++)
                {
                    if (json[j] == '[') depth++;
                    else if (json[j] == ']')
                    {
                        depth--;
                        if (depth == 0) { end = j; break; }
                    }
                }
                if (end < 0)
                    throw new InvalidOperationException("map-furniture.json: unterminated \"points\" array");

                string inner = json.Substring(bracketStart + 1, end - bracketStart - 1);
                result.Add(ParseNumberPairs(inner));
                i = end + 1;
            }
            return result;
        }

        static Vector2[] ParseNumberPairs(string inner)
        {
            var nums = new List<float>();
            var sb = new StringBuilder();
            foreach (char c in inner)
            {
                if (char.IsDigit(c) || c == '-' || c == '.' || c == 'e' || c == 'E' || c == '+')
                {
                    sb.Append(c);
                }
                else if (sb.Length > 0)
                {
                    nums.Add(float.Parse(sb.ToString(), CultureInfo.InvariantCulture));
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
                nums.Add(float.Parse(sb.ToString(), CultureInfo.InvariantCulture));

            if (nums.Count % 2 != 0)
                throw new InvalidOperationException(
                    "map-furniture.json: a \"points\" array has an odd number of coordinates");

            var pts = new Vector2[nums.Count / 2];
            for (int k = 0; k < pts.Length; k++)
                pts[k] = new Vector2(nums[2 * k], nums[2 * k + 1]);
            return pts;
        }
    }
}
