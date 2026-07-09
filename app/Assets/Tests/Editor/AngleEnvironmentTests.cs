using System.Collections.Generic;
using System.IO;
using BattleAtlas;
using NUnit.Framework;
using UnityEngine;

namespace BattleAtlas.Tests
{
    // Phase 7 environment: the pure layout math is pinned unconditionally;
    // the baked-data contract tests skip (like the generated-terrain test)
    // when data/heightmap_angle/environment.json has not been baked.
    public class AngleEnvironmentTests
    {
        static string CropDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../data/heightmap_angle"));

        static AngleEnvironment LoadOrIgnore()
        {
            string p = Path.Combine(CropDir, "environment.json");
            if (!File.Exists(p))
                Assert.Ignore("environment.json not baked; run " +
                    "`uv run python -m terrain_pipeline.cli environment`");
            return AngleEnvironmentData.Parse(File.ReadAllText(p));
        }

        // --- pure layout math ---

        [Test]
        public void ClassWeightsAreAPartitionForEveryClass()
        {
            var w = new float[AngleEnvironmentLayout.LayerCount];
            for (byte cls = 0; cls <= 5; cls++)
            {
                foreach (float noise in new[] { 0f, 0.33f, 1f })
                {
                    AngleEnvironmentLayout.ClassWeights(cls, noise, w);
                    float sum = 0f;
                    foreach (float x in w)
                    {
                        Assert.GreaterOrEqual(x, 0f, $"class {cls} noise {noise}");
                        sum += x;
                    }
                    Assert.AreEqual(1f, sum, 1e-4f, $"class {cls} noise {noise}");
                }
            }
        }

        [Test]
        public void ClassWeightsRejectsUnknownClass()
        {
            var w = new float[AngleEnvironmentLayout.LayerCount];
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => AngleEnvironmentLayout.ClassWeights(6, 0.5f, w));
        }

        [Test]
        public void PostAndRailIsDeterministicAndOnTheLine()
        {
            var line = new List<Vector2>
            {
                new Vector2(10f, 10f), new Vector2(40f, 25f), new Vector2(60f, 70f),
            };
            var posts1 = new List<AngleEnvironmentLayout.Post>();
            var rails1 = new List<AngleEnvironmentLayout.RailSegment>();
            AngleEnvironmentLayout.PostAndRail("t", line, 3f, 1.5f, posts1, rails1);
            var posts2 = new List<AngleEnvironmentLayout.Post>();
            var rails2 = new List<AngleEnvironmentLayout.RailSegment>();
            AngleEnvironmentLayout.PostAndRail("t", line, 3f, 1.5f, posts2, rails2);

            Assert.AreEqual(posts1.Count, posts2.Count);
            Assert.AreEqual(rails1.Count, rails2.Count);
            Assert.Greater(posts1.Count, 20);
            Assert.AreEqual(5 * (posts1.Count - 2), rails1.Count,
                "five rails per panel (two polyline segments -> posts-1 panels each)");
            for (int i = 0; i < posts1.Count; i++)
            {
                Assert.AreEqual(posts1[i].pos, posts2[i].pos);
                Assert.Less(DistToPolyline(posts1[i].pos, line), 0.001f);
            }
            for (int i = 0; i < rails1.Count; i++)
            {
                Assert.AreEqual(rails1[i].a, rails2[i].a);
                Assert.AreEqual(rails1[i].height, rails2[i].height);
                Assert.Greater(rails1[i].height, 0.2f);
                Assert.Less(rails1[i].height, 1.6f);
            }
        }

        [Test]
        public void WormFenceZigzagsAroundTheTracedLine()
        {
            var line = new List<Vector2> { new Vector2(0f, 0f), new Vector2(34f, 0f) };
            var rails = new List<AngleEnvironmentLayout.RailSegment>();
            AngleEnvironmentLayout.Worm("t", line, 1.2f, rails);
            Assert.AreEqual(6 * 10, rails.Count, "ten 3.3 m panels, six rails each");
            foreach (var r in rails)
            {
                Assert.LessOrEqual(Mathf.Abs(r.a.y), 0.56f, "zig half-width bound");
                Assert.LessOrEqual(Mathf.Abs(r.b.y), 0.56f);
                Assert.AreNotEqual(Mathf.Sign(r.a.y), Mathf.Sign(r.b.y),
                    "each panel crosses the base line");
            }
        }

        [Test]
        public void CarveDepthEasesFromFullToZero()
        {
            Assert.AreEqual(0.45f, AngleEnvironmentLayout.CarveDepth(0f, 5f, 0.45f), 1e-4f);
            Assert.AreEqual(0f, AngleEnvironmentLayout.CarveDepth(5f, 5f, 0.45f), 1e-4f);
            Assert.AreEqual(0f, AngleEnvironmentLayout.CarveDepth(9f, 5f, 0.45f), 1e-4f);
            float prev = float.MaxValue;
            for (float d = 0f; d <= 5f; d += 0.5f)
            {
                float v = AngleEnvironmentLayout.CarveDepth(d, 5f, 0.45f);
                Assert.LessOrEqual(v, prev, "monotone ease");
                prev = v;
            }
        }

        [Test]
        public void FlatPointsParserRejectsOddLengths()
        {
            Assert.Throws<System.ArgumentException>(
                () => AngleEnvironmentData.Points(new List<float> { 1f, 2f, 3f }));
            var pts = AngleEnvironmentData.Points(new List<float> { 1f, 2f, 3f, 4f });
            Assert.AreEqual(new Vector2(1f, 2f), pts[0]);
            Assert.AreEqual(new Vector2(3f, 4f), pts[1]);
        }

        // --- palette and prop assets exist where the stage looks ---

        [Test]
        public void PaletteTexturesArePresent()
        {
            string[] paths =
            {
                "Assets/ThirdParty/Materials/PolyHavenLeafyGrass/leafy_grass_diff_1k.jpg",
                "Assets/ThirdParty/Materials/PolyHavenSparseGrass/sparse_grass_diff_1k.jpg",
                "Assets/ThirdParty/Materials/PolyHavenWitheredGrass/withered_grass_diff_1k.jpg",
                "Assets/ThirdParty/Materials/PolyHavenStonyDirtPath/stony_dirt_path_diff_2k.jpg",
                "Assets/ThirdParty/Materials/PolyHavenBrownMudDry/brown_mud_dry_diff_2k.jpg",
                "Assets/ThirdParty/Materials/PolyHavenStackedStoneWall/stacked_stone_wall_diff_2k.jpg",
                "Assets/ThirdParty/Materials/PolyHavenWeatheredPlanks/weathered_planks_diff_2k.jpg",
                "Assets/ThirdParty/Models/PolyHavenIslandTree02/island_tree_02_decimated.fbx",
            };
            foreach (string p in paths)
                Assert.IsTrue(File.Exists(Path.GetFullPath(Path.Combine(
                    Application.dataPath, "..", p))), p);
        }

        [Test]
        public void EnvironmentPropsArePresent()
        {
            string[] props =
            {
                "ordnance_rifle", "ordnance_rifle_disabled", "ordnance_rifle_wrecked",
                "limber", "caisson", "ammo_chest", "wheel_wreck",
                "codori_house", "codori_barn",
            };
            foreach (string p in props)
                Assert.IsTrue(File.Exists(Path.GetFullPath(Path.Combine(
                    Application.dataPath, "ProjectOwned/Environment/Props", p + ".fbx"))),
                    p);
        }

        // --- baked-data contract (skips when not baked) ---

        [Test]
        public void BakedEnvironmentMatchesTheCropWindow()
        {
            var env = LoadOrIgnore();
            Assert.AreEqual(3900f, env.crop.x0);
            Assert.AreEqual(4450f, env.crop.z0);
            Assert.AreEqual(4700f, env.crop.x1);
            Assert.AreEqual(5250f, env.crop.z1);

            var wall = AngleEnvironmentData.Points(env.wall.polylineFlat);
            foreach (var p in wall)
            {
                Assert.GreaterOrEqual(p.x, env.crop.x0);
                Assert.LessOrEqual(p.x, env.crop.x1);
                Assert.GreaterOrEqual(p.y, env.crop.z0);
                Assert.LessOrEqual(p.y, env.crop.z1);
            }
            foreach (var f in env.fences)
                foreach (var p in AngleEnvironmentData.Points(f.polylineFlat))
                {
                    Assert.GreaterOrEqual(p.x, env.crop.x0 - 0.01f, f.featureId);
                    Assert.LessOrEqual(p.x, env.crop.x1 + 0.01f, f.featureId);
                }
            // road samples may extend past the crop by the bake's carve pad
            foreach (var s in env.road.centerline)
            {
                Assert.GreaterOrEqual(s.x, env.crop.x0 - 61f);
                Assert.LessOrEqual(s.x, env.crop.x1 + 61f);
                Assert.Greater(s.halfWidth, 3f);
                Assert.Less(s.halfWidth, 10f);
            }
        }

        [Test]
        public void BakedSplatRasterMatchesItsDescriptor()
        {
            var env = LoadOrIgnore();
            string p = Path.Combine(CropDir, env.splat.path);
            Assert.IsTrue(File.Exists(p), p);
            byte[] bytes = File.ReadAllBytes(p);
            Assert.AreEqual(env.splat.resolution * env.splat.resolution, bytes.Length);
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                string hex = System.BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", "").ToLowerInvariant();
                Assert.AreEqual(env.splat.sha256, hex,
                    "raster on disk must be the one the bake described");
            }
            foreach (byte b in bytes)
                Assert.LessOrEqual(b, (byte)5);
        }

        [Test]
        public void BakedGeometryConvertsToCropLocalInsideTheTile()
        {
            var env = LoadOrIgnore();
            foreach (var b in env.buildings)
            {
                Vector2 local = AngleEnvironmentLayout.ToCropLocal(
                    new Vector2(b.x, b.z), env.crop);
                Assert.GreaterOrEqual(local.x, 0f, b.id);
                Assert.LessOrEqual(local.x, 800f, b.id);
                Assert.GreaterOrEqual(local.y, 0f, b.id);
                Assert.LessOrEqual(local.y, 800f, b.id);
            }
            foreach (var g in env.battery.guns)
            {
                Vector2 local = AngleEnvironmentLayout.ToCropLocal(
                    new Vector2(g.x, g.z), env.crop);
                Assert.GreaterOrEqual(local.x, 0f);
                Assert.LessOrEqual(local.x, 800f);
            }
        }

        [Test]
        public void BakedWheatClumpsStayInsideTheirPolygonAndClass()
        {
            var env = LoadOrIgnore();
            string p = Path.Combine(CropDir, env.splat.path);
            byte[] classes = File.ReadAllBytes(p);
            int res = env.splat.resolution;
            float size = env.crop.x1 - env.crop.x0;
            byte ClassAt(Vector2 macro)
            {
                int c = Mathf.Clamp((int)((macro.x - env.crop.x0) / size * res), 0, res - 1);
                int r = Mathf.Clamp((int)((env.crop.z1 - macro.y) / size * res), 0, res - 1);
                return classes[r * res + c];
            }
            var clumps = AngleEnvironmentLayout.WheatClumps(env.wheat, env.crop, ClassAt);
            var clumps2 = AngleEnvironmentLayout.WheatClumps(env.wheat, env.crop, ClassAt);
            Assert.AreEqual(clumps.Count, clumps2.Count, "deterministic");
            Assert.Greater(clumps.Count, 100, "standing wheat exists west of the road");
            var poly = AngleEnvironmentData.Points(env.wheat.polygonFlat);
            foreach (var (pos, _) in clumps)
            {
                Assert.IsTrue(AngleEnvironmentLayout.PointInPolygon(pos, poly));
                Assert.AreEqual(2, ClassAt(pos), "clumps only on stubble ground");
            }
        }

        static float DistToPolyline(Vector2 p, List<Vector2> line)
        {
            float best = float.MaxValue;
            for (int i = 0; i < line.Count - 1; i++)
            {
                Vector2 a = line[i], b = line[i + 1];
                Vector2 v = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, v) / v.sqrMagnitude);
                best = Mathf.Min(best, Vector2.Distance(p, a + t * v));
            }
            return best;
        }
    }
}
