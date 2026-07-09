using System.Linq;
using BattleAtlas;
using NUnit.Framework;
using UnityEngine;

namespace BattleAtlas.Tests
{
    // The bake-off's fairness contract: both pipelines must stage identical
    // content, which requires the layout to be deterministic, in-bounds, and
    // sized as the plan specifies (100 soldiers, smoke, road/wall/fence/
    // wheat/trees at the §8.1 crop).
    public class AngleBakeoffLayoutTests
    {
        const float CropSize = 800f;

        static void AssertInBounds(Vector2 p, string what)
        {
            Assert.That(p.x, Is.InRange(0f, CropSize), $"{what} x out of crop");
            Assert.That(p.y, Is.InRange(0f, CropSize), $"{what} z out of crop");
        }

        [Test]
        public void ComparisonMomentIs1520LocalMeanTime()
        {
            Assert.AreEqual(8400f, AngleBakeoffLayout.ComparisonBattleT);
            Assert.AreEqual(55200f, AngleBakeoffLayout.SecondsSinceMidnight); // 15:20
            var (elev, azim) = SunEphemeris.SunAngles(
                AngleBakeoffLayout.SecondsSinceMidnight);
            // mid-afternoon WSW sun, not a degenerate clamp value
            Assert.That(elev, Is.InRange(43f, 50f));
            Assert.That(azim, Is.InRange(259f, 265f));
        }

        [Test]
        public void ExactlyOneHundredSoldiers()
        {
            Assert.AreEqual(100,
                AngleBakeoffLayout.CsaSoldiers().Count +
                AngleBakeoffLayout.UsaSoldiers().Count);
        }

        [Test]
        public void SoldiersAreDeterministic()
        {
            var a = AngleBakeoffLayout.CsaSoldiers();
            var b = AngleBakeoffLayout.CsaSoldiers();
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].pos, b[i].pos);
                Assert.AreEqual(a[i].facingDeg, b[i].facingDeg);
            }
        }

        [Test]
        public void EverythingStaysInsideTheCrop()
        {
            foreach (var (pos, _) in AngleBakeoffLayout.CsaSoldiers())
                AssertInBounds(pos, "csa soldier");
            foreach (var (pos, _) in AngleBakeoffLayout.UsaSoldiers())
                AssertInBounds(pos, "usa soldier");
            foreach (var (pos, _) in AngleBakeoffLayout.WallSegments())
                AssertInBounds(pos, "wall segment");
            foreach (var (pos, _) in AngleBakeoffLayout.FencePosts())
                AssertInBounds(pos, "fence post");
            foreach (var (pos, _) in AngleBakeoffLayout.WheatClumps())
                AssertInBounds(pos, "wheat clump");
            foreach (var (pos, _) in AngleBakeoffLayout.CopseTrees(true))
                AssertInBounds(pos, "near tree");
            foreach (var (pos, _) in AngleBakeoffLayout.CopseTrees(false))
                AssertInBounds(pos, "far tree");
            foreach (var (pos, _) in AngleBakeoffLayout.SmokePuffs())
                AssertInBounds(pos, "smoke puff");
            foreach (var s in AngleBakeoffLayout.RoadSamples())
                AssertInBounds(s, "road sample");
        }

        [Test]
        public void SmokeIsPresentAndDeterministic()
        {
            var a = AngleBakeoffLayout.SmokePuffs();
            var b = AngleBakeoffLayout.SmokePuffs();
            Assert.That(a.Count, Is.GreaterThanOrEqualTo(20));
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].pos, b[i].pos);
                Assert.AreEqual(a[i].radius, b[i].radius);
            }
        }

        [Test]
        public void WallSegmentsFollowTheAnglePolyline()
        {
            var segments = AngleBakeoffLayout.WallSegments();
            // 220 m north-south run + 80 m west jog at 3 m spacing
            Assert.That(segments.Count, Is.InRange(95, 101));
            // the two legs have distinct bearings: ~0 (north) and ~270 (west)
            var bearings = segments.Select(s =>
                Mathf.Repeat(s.bearingDeg, 360f)).Distinct().ToArray();
            Assert.AreEqual(2, bearings.Length);
        }

        [Test]
        public void CamerasAreTheThreeCanonicalFrames()
        {
            var names = AngleBakeoffLayout.Cameras.Select(c => c.name).ToArray();
            CollectionAssert.AreEqual(
                new[] { "theater", "tactical", "eye" }, names);
            var eye = AngleBakeoffLayout.Cameras[2];
            Assert.AreEqual(1.66f, eye.heightAboveGround); // §6.5 eyeHeightM
            Assert.AreEqual(68f, eye.fovDeg);              // §6.5 fovDeg
            foreach (var cam in AngleBakeoffLayout.Cameras)
            {
                AssertInBounds(cam.posXZ, $"camera {cam.name}");
                AssertInBounds(cam.lookXZ, $"camera {cam.name} target");
            }
        }
    }
}
