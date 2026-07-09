using BattleAtlas;
using NUnit.Framework;
using UnityEngine;

namespace BattleAtlas.Tests
{
    // §8.1 acceptance: crop positions round-trip to the macro battlefield
    // frame, exaggeration is locked at 1.0, spacing <= ~1 m. The fixture
    // mirrors the real data/heightmap_angle/heightmap.json the pipeline's
    // `crop` command writes (values from the committed §8.1 default crop).
    public class AngleTerrainFrameTests
    {
        const string FixtureJson = @"{
            ""resolution"": 1025,
            ""width_m"": 800.0,
            ""depth_m"": 800.0,
            ""min_elev_m"": 166.58944702148438,
            ""max_elev_m"": 185.76446533203125,
            ""origin_utm_e"": 308108.0216360274,
            ""origin_utm_n"": 4408984.271124143,
            ""crs"": ""EPSG:26918"",
            ""row0"": ""north"",
            ""crop_x0_m"": 3900.0,
            ""crop_z0_m"": 4450.0,
            ""crop_x1_m"": 4700.0,
            ""crop_z1_m"": 5250.0,
            ""macro_origin_utm_e"": 304208.0216360274,
            ""macro_origin_utm_n"": 4404534.271124143,
            ""sample_spacing_m"": 0.78125,
            ""vertical_exaggeration"": 1.0,
            ""surface"": ""USGS 3DEP 1 m bare-earth DTM (no above-ground structures)""
        }";

        static AngleTerrainMetadata Meta() =>
            JsonUtility.FromJson<AngleTerrainMetadata>(FixtureJson);

        [Test]
        public void FixtureParses()
        {
            var m = Meta();
            Assert.AreEqual(1025, m.resolution);
            Assert.AreEqual(800f, m.width_m);
            Assert.AreEqual(3900f, m.crop_x0_m);
            Assert.AreEqual(1f, m.vertical_exaggeration);
            Assert.AreEqual("north", m.row0);
        }

        [Test]
        public void CropOriginMapsToCropWindowCorner()
        {
            var m = Meta();
            var macro = AngleTerrainFrame.CropLocalToMacro(Vector2.zero, m);
            Assert.AreEqual(3900f, macro.x);
            Assert.AreEqual(4450f, macro.y);
        }

        [Test]
        public void ArmisteadWallCrossingRoundTrips()
        {
            // claim-armistead-scaled-wall geometry from the plan: macro
            // (4415, 4855) must land inside the crop and round-trip exactly
            var m = Meta();
            var macro = new Vector2(4415f, 4855f);
            var local = AngleTerrainFrame.MacroToCropLocal(macro, m);
            Assert.AreEqual(515f, local.x);
            Assert.AreEqual(405f, local.y);
            var back = AngleTerrainFrame.CropLocalToMacro(local, m);
            Assert.AreEqual(macro.x, back.x);
            Assert.AreEqual(macro.y, back.y);
        }

        [Test]
        public void MacroUtmRoundTripStaysSubMillimeter()
        {
            var m = Meta();
            var macro = new Vector2(4415.25f, 4855.75f);
            var (e, n) = AngleTerrainFrame.MacroToUtm(macro, m);
            Assert.AreEqual(304208.0216360274 + 4415.25, e, 1e-6);
            Assert.AreEqual(4404534.271124143 + 4855.75, n, 1e-6);
            var back = AngleTerrainFrame.UtmToMacro(e, n, m);
            Assert.AreEqual(macro.x, back.x, 1e-3f);
            Assert.AreEqual(macro.y, back.y, 1e-3f);
        }

        [Test]
        public void CropTileUtmOriginAgreesWithMacroFrame()
        {
            // the crop's own UTM origin must equal macro origin + crop corner:
            // two independent paths to the same UTM point
            var m = Meta();
            var (e, n) = AngleTerrainFrame.MacroToUtm(
                new Vector2(m.crop_x0_m, m.crop_z0_m), m);
            Assert.AreEqual(m.origin_utm_e, e, 1e-6);
            Assert.AreEqual(m.origin_utm_n, n, 1e-6);
        }

        [Test]
        public void ValidateTrueScaleAcceptsTheContract()
        {
            Assert.DoesNotThrow(() => AngleTerrainFrame.ValidateTrueScale(Meta()));
        }

        [Test]
        public void ValidateTrueScaleRejectsExaggeration()
        {
            var m = Meta();
            m.vertical_exaggeration = 2.5f;
            Assert.Throws<System.InvalidOperationException>(
                () => AngleTerrainFrame.ValidateTrueScale(m));
        }

        [Test]
        public void ValidateTrueScaleRejectsCoarseSpacing()
        {
            var m = Meta();
            m.sample_spacing_m = 2.0f;
            Assert.Throws<System.InvalidOperationException>(
                () => AngleTerrainFrame.ValidateTrueScale(m));
        }

        [Test]
        public void ValidateTrueScaleRejectsWrongRowOrder()
        {
            var m = Meta();
            m.row0 = "south";
            Assert.Throws<System.InvalidOperationException>(
                () => AngleTerrainFrame.ValidateTrueScale(m));
        }
    }
}
