using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Pure, deterministic geometry math for the Phase 7 environment stage —
    // everything here is a function of the baked environment data plus the
    // shared FNV jitter, so EditMode tests can pin behavior without a scene.
    // Frames: inputs are battlefield-local (macro) meters as baked;
    // ToCropLocal converts using the crop origin from environment.json.
    public static class AngleEnvironmentLayout
    {
        public static Vector2 ToCropLocal(Vector2 macro, EnvCrop crop) =>
            new Vector2(macro.x - crop.x0, macro.y - crop.z0);

        // --- fences ---

        public struct RailSegment
        {
            public Vector2 a;      // crop-local meters
            public Vector2 b;
            public float height;   // rail center height above ground
        }

        public struct Post
        {
            public Vector2 pos;
            public float bearingDeg;
        }

        // Five-rail post-and-rail (ED-12): posts on the traced line, five
        // rails between consecutive posts with deterministic sag/offset.
        public static void PostAndRail(
            string key, List<Vector2> line, float postSpacing, float height,
            List<Post> posts, List<RailSegment> rails)
        {
            const int railCount = 5;
            for (int s = 0; s < line.Count - 1; s++)
            {
                Vector2 a = line[s], b = line[s + 1];
                float len = Vector2.Distance(a, b);
                if (len < 0.01f) continue;
                float bearing = Mathf.Atan2(b.x - a.x, b.y - a.y) * Mathf.Rad2Deg;
                int panels = Mathf.Max(1, Mathf.FloorToInt(len / postSpacing));
                for (int i = 0; i <= panels; i++)
                {
                    Vector2 p = Vector2.Lerp(a, b, Mathf.Min(1f, i * postSpacing / len));
                    posts.Add(new Post { pos = p, bearingDeg = bearing });
                    if (i == panels) continue;
                    Vector2 q = Vector2.Lerp(a, b, Mathf.Min(1f, (i + 1) * postSpacing / len));
                    for (int r = 0; r < railCount; r++)
                    {
                        float h = height * (0.22f + 0.78f * r / (railCount - 1))
                            + 0.03f * FormationLayout.Jitter(key, s * 1000 + i * 8 + r, 17);
                        rails.Add(new RailSegment { a = p, b = q, height = h });
                    }
                }
            }
        }

        // Worm (zigzag) fence (ED-12): panels alternate around the traced
        // base line; six stacked rails per panel.
        public static void Worm(
            string key, List<Vector2> line, float height,
            List<RailSegment> rails)
        {
            const float panelLen = 3.3f;
            const float zigHalfWidth = 0.55f;
            const int railCount = 6;
            for (int s = 0; s < line.Count - 1; s++)
            {
                Vector2 a = line[s], b = line[s + 1];
                float len = Vector2.Distance(a, b);
                if (len < 0.01f) continue;
                Vector2 dir = (b - a) / len;
                Vector2 normal = new Vector2(dir.y, -dir.x);
                int panels = Mathf.Max(1, Mathf.FloorToInt(len / panelLen));
                for (int i = 0; i < panels; i++)
                {
                    float side = (i % 2 == 0) ? 1f : -1f;
                    Vector2 p = a + dir * (i * panelLen) + normal * (zigHalfWidth * side);
                    Vector2 q = a + dir * Mathf.Min(len, (i + 1) * panelLen)
                        + normal * (-zigHalfWidth * side);
                    for (int r = 0; r < railCount; r++)
                    {
                        float h = height * (0.10f + 0.90f * r / (railCount - 1))
                            + 0.02f * FormationLayout.Jitter(key, s * 1000 + i * 8 + r, 29);
                        rails.Add(new RailSegment { a = p, b = q, height = h });
                    }
                }
            }
        }

        // --- splat classes -> terrain layer weights ---

        // Layer order the stage builds: 0 pasture (leafy_grass),
        // 1 dry summer grass (sparse_grass), 2 wheat stubble
        // (withered_grass, warm remap), 3 packed dirt road
        // (stony_dirt_path), 4 disturbed soil (brown_mud_dry).
        public const int LayerCount = 5;

        // Deterministic per-texel blend (ED-15/ED-17): classes map to layer
        // mixes, not hard indices, so fields read as ground cover rather
        // than paint. `noise` in [0,1] varies the mix texel to texel.
        public static void ClassWeights(byte cls, float noise, float[] w)
        {
            for (int i = 0; i < LayerCount; i++) w[i] = 0f;
            switch (cls)
            {
                case 0: // pasture: leafy with dry patches
                    w[0] = 0.72f + 0.18f * noise;
                    w[1] = 1f - w[0];
                    break;
                case 1: // dry summer grass
                    w[1] = 0.80f + 0.15f * noise;
                    w[0] = 1f - w[1];
                    break;
                case 2: // wheat stubble: withered gold over dry
                    w[2] = 0.68f + 0.22f * noise;
                    w[1] = 1f - w[2];
                    break;
                case 3: // packed dirt road
                    w[3] = 0.88f + 0.10f * noise;
                    w[1] = 1f - w[3];
                    break;
                case 4: // disturbed soil (wheel paths, battery ground)
                    w[4] = 0.75f + 0.20f * noise;
                    w[3] = 1f - w[4];
                    break;
                case 5: // trampled corridor: dry grass ground down to soil
                    w[1] = 0.52f + 0.20f * noise;
                    w[4] = 0.30f - 0.12f * noise;
                    w[0] = 1f - w[1] - w[4];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cls), $"unknown ground class {cls}");
            }
        }

        // --- road carve (ED-11) ---

        // Depth of the roadbed depression at `dist` meters from the
        // centerline where the local bed half-width is `bedHalf`:
        // full depth in the bed center, smooth ease to 0 at the fence line.
        public static float CarveDepth(float dist, float bedHalf, float depth)
        {
            float t = Mathf.Clamp01(Mathf.Abs(dist) / Mathf.Max(bedHalf, 0.01f));
            float ease = 1f - t * t * (3f - 2f * t); // 1 - smoothstep
            return depth * ease;
        }

        // --- standing wheat patches (ED-15) ---

        public static List<(Vector2 pos, float yawDeg)> WheatClumps(
            EnvWheat wheat, EnvCrop crop, Func<Vector2, byte> classAt)
        {
            var poly = AngleEnvironmentData.Points(wheat.polygonFlat);
            float minX = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxZ = float.MinValue;
            foreach (var p in poly)
            {
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.y); maxZ = Mathf.Max(maxZ, p.y);
            }
            var clumps = new List<(Vector2, float)>();
            float step = wheat.clumpSpacingM;
            int i = 0;
            for (float x = minX; x <= maxX; x += step)
                for (float z = minZ; z <= maxZ; z += step, i++)
                {
                    var p = new Vector2(
                        x + 0.8f * FormationLayout.Jitter(wheat.patchNoiseKey, i, 17),
                        z + 0.8f * FormationLayout.Jitter(wheat.patchNoiseKey, i, 29));
                    if (!PointInPolygon(p, poly)) continue;
                    // patchiness: standing wheat survives in hash-picked
                    // patches; the rest is stubble (splat class 2)
                    float patch = Hash01(wheat.patchNoiseKey,
                        (int)(x / 12f) * 731 + (int)(z / 12f) * 137);
                    if (patch < 0.45f) continue;
                    if (classAt != null && classAt(p) != 2) continue;
                    clumps.Add((p, 90f * FormationLayout.Jitter(wheat.patchNoiseKey, i, 43)));
                }
            return clumps;
        }

        public static bool PointInPolygon(Vector2 p, List<Vector2> poly)
        {
            bool inside = false;
            int j = poly.Count - 1;
            for (int i = 0; i < poly.Count; i++)
            {
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y)
                        / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
                j = i;
            }
            return inside;
        }

        // FNV-family hash, the project's shared deterministic noise.
        public static float Hash01(string key, int index)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in key) h = (h ^ c) * 16777619u;
                h = (h ^ (uint)index) * 16777619u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }
    }
}
