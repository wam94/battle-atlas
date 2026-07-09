using System.IO;
using BattleAtlas;
using BattleAtlas.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.Tests
{
    // Phase 4 render-profile settings validation (plan §13 "Unity EditMode:
    // render-profile settings validation"). Pins the two committed HDRP
    // quality profiles, the exposure calibration, the material conversion,
    // and the deterministic sun contract the custom shaders read.
    public class HdrpMigrationTests
    {
        static HDRenderPipelineAsset Load(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(path);
            Assert.IsNotNull(asset, $"{path} missing");
            return asset;
        }

        [Test]
        public void ProjectDefaultPipeline_IsHdrpPlaybackProfile()
        {
            var playback = Load(HdrpMigration.PlaybackAssetPath);
            Assert.AreSame(playback, GraphicsSettings.defaultRenderPipeline,
                "the committed project default must be the HDRP PLAYBACK profile");
        }

        [Test]
        public void QualityLevels_DeferToGraphicsDefault()
        {
            // no per-level URP leftovers: every quality level must fall
            // through to the graphics default (the playback profile)
            for (int i = 0; i < QualitySettings.count; i++)
                Assert.IsNull(QualitySettings.GetRenderPipelineAssetAt(i),
                    $"quality level {i} overrides the render pipeline");
        }

        [Test]
        public void PlaybackProfile_IsRealTimeShaped()
        {
            var ps = Load(HdrpMigration.PlaybackAssetPath)
                .currentPlatformRenderPipelineSettings;
            Assert.IsFalse(ps.supportSSR, "playback: SSR off (real-time budget)");
            Assert.IsFalse(ps.supportVolumetrics, "playback: volumetrics off");
            Assert.AreEqual(RenderPipelineSettings.ColorBufferFormat.R11G11B10,
                ps.colorBufferFormat);
        }

        [Test]
        public void OfflineProfile_IsOfflineRenderShaped()
        {
            var ps = Load(HdrpMigration.OfflineAssetPath)
                .currentPlatformRenderPipelineSettings;
            Assert.IsTrue(ps.supportSSR, "offline: SSR on (§3.2)");
            Assert.IsTrue(ps.supportSSAO, "offline: SSAO on (§3.2)");
            Assert.IsTrue(ps.supportVolumetrics, "offline: volumetrics on (§3.2)");
            Assert.IsTrue(ps.supportMotionVectors,
                "offline: motion vectors on (Recorder accumulation)");
            Assert.AreEqual(RenderPipelineSettings.ColorBufferFormat.R16G16B16A16,
                ps.colorBufferFormat, "offline: 16-bit color buffer");
        }

        [Test]
        public void AtlasVolumeProfile_PinsTheExposureCalibration()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                HdrpMigration.AtlasVolumeProfilePath);
            Assert.IsNotNull(profile);

            Assert.IsTrue(profile.TryGet(
                out UnityEngine.Rendering.HighDefinition.Exposure exposure));
            Assert.AreEqual(ExposureMode.Fixed, exposure.mode.value,
                "exposure must be FIXED: automatic exposure would make offline " +
                "renders content-dependent and nondeterministic");
            Assert.AreEqual(HdrpMigration.FixedExposureEv100,
                exposure.fixedExposure.value, 1e-4f);

            Assert.IsTrue(profile.TryGet(out VisualEnvironment env));
            Assert.AreEqual((int)SkyType.PhysicallyBased, env.skyType.value);
            Assert.IsTrue(profile.TryGet(out PhysicallyBasedSky _));

            // the URP Atlas post chain carried over, not re-graded
            Assert.IsTrue(profile.TryGet(
                out UnityEngine.Rendering.HighDefinition.Tonemapping tone));
            Assert.AreEqual(UnityEngine.Rendering.HighDefinition.TonemappingMode.Neutral,
                tone.mode.value);
        }

        [Test]
        public void OfflineVolumeProfile_EnablesVolumetricFog()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                HdrpMigration.OfflineVolumeProfilePath);
            Assert.IsNotNull(profile);
            Assert.IsTrue(profile.TryGet(
                out UnityEngine.Rendering.HighDefinition.Fog fog));
            Assert.IsTrue(fog.enabled.value);
            Assert.IsTrue(fog.enableVolumetricFog.value);
        }

        [Test]
        public void BattleMaterials_ConvertedToHdrpLit_ColorsPreserved()
        {
            var hdrpLit = Shader.Find("HDRP/Lit");
            Assert.IsNotNull(hdrpLit);

            var marker = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/UnitMarker.mat");
            Assert.AreSame(hdrpLit, marker.shader);
            Assert.IsTrue(marker.enableInstancing);
            Assert.AreEqual(Color.white, marker.GetColor("_BaseColor"));

            var smoke = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/Smoke.mat");
            Assert.AreSame(hdrpLit, smoke.shader);
            Assert.AreEqual(1f, smoke.GetFloat("_SurfaceType"), "smoke stays transparent");
            AssertColor(new Color(0.93f, 0.93f, 0.9f, 1f), smoke.GetColor("_BaseColor"));

            var dust = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Battle/Dust.mat");
            Assert.AreSame(hdrpLit, dust.shader);
            Assert.AreEqual(1f, dust.GetFloat("_SurfaceType"), "dust stays transparent");
            AssertColor(new Color(0.72f, 0.62f, 0.44f, 1f), dust.GetColor("_BaseColor"));
        }

        [Test]
        public void CustomShaders_ActiveSubshaderIsHdrpForwardOnly()
        {
            // with HDRP active, Unity selects the SubShader whose
            // RenderPipeline tag matches — its single pass is ForwardOnly.
            foreach (string name in new[] { "BattleAtlas/SoldierVertexTint", "BattleAtlas/Flag" })
            {
                var shader = Shader.Find(name);
                Assert.IsNotNull(shader, $"{name} missing");
                var mat = new Material(shader);
                Assert.AreEqual(1, mat.passCount, $"{name}: one HDRP pass expected");
                Assert.AreEqual("ForwardOnly", mat.GetPassName(0),
                    $"{name}: the HDRP SubShader must be the active one");
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GeneratedTerrainMaterial_IsHdrpTerrainLit_WhenPresent()
        {
            // Assets/Generated is gitignored — on a checkout that ran the
            // land-cover import, the material must be HDRP now
            var mat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Generated/LandcoverTerrain.mat");
            if (mat == null)
                Assert.Ignore("no generated terrain on this checkout");
            Assert.AreEqual("HDRP/TerrainLit", mat.shader.name);
            Assert.IsTrue(mat.IsKeywordEnabled("_TERRAIN_INSTANCED_PERPIXEL_NORMAL"),
                "per-pixel normal keyword must survive the conversion");
        }

        [Test]
        public void SunDirector_PublishesDeterministicShaderGlobals()
        {
            var lightGo = new GameObject("test sun");
            var clockGo = new GameObject("test clock");
            try
            {
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 3f;
                var director = lightGo.AddComponent<SunDirector>();
                director.sun = light;
                director.clock = clockGo.AddComponent<BattleClock>();
                director.ReadingLight = true; // fixed NW raking light: deterministic
                Call(director, "Start");
                Call(director, "Update");

                var expectedRot = SunEphemeris.LightRotation(32f, 315f);
                Vector3 expectedDir = -(expectedRot * Vector3.forward);
                Vector4 dir = Shader.GetGlobalVector("_BattleSunDirWS");
                Assert.AreEqual(expectedDir.x, dir.x, 1e-4f);
                Assert.AreEqual(expectedDir.y, dir.y, 1e-4f);
                Assert.AreEqual(expectedDir.z, dir.z, 1e-4f);

                // reading light: white x the normalized shader intensity —
                // NOT the light's physical intensity (lux under HDRP)
                Vector4 color = Shader.GetGlobalVector("_BattleSunColor");
                Assert.AreEqual(director.ShaderSunIntensity, color.x, 1e-4f);
                Assert.AreEqual(director.ShaderSunIntensity, color.y, 1e-4f);
                Assert.AreEqual(director.ShaderSunIntensity, color.z, 1e-4f);
            }
            finally
            {
                Object.DestroyImmediate(lightGo);
                Object.DestroyImmediate(clockGo);
            }
        }

        [Test]
        public void AtlasScene_ReferencesHdrpComponentsAndProfile()
        {
            // text-level assert so the suite never has to open (and disturb)
            // the scene: the scene must reference the committed HDRP Atlas
            // volume profile and the HDRP additional-data scripts, and must
            // no longer reference the URP additional-data scripts.
            string scene = File.ReadAllText("Assets/Scenes/Atlas.unity");

            string profileGuid = AssetDatabase.AssetPathToGUID(
                HdrpMigration.AtlasVolumeProfilePath);
            StringAssert.Contains(profileGuid, scene,
                "Atlas volume must use the committed HDRP profile");

            StringAssert.Contains(ScriptGuid<HDAdditionalLightData>(), scene,
                "sun needs HDAdditionalLightData (physical light units)");
            StringAssert.Contains(ScriptGuid<HDAdditionalCameraData>(), scene,
                "camera needs HDAdditionalCameraData");
            StringAssert.DoesNotContain(
                ScriptGuid<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>(),
                scene, "URP light data must be gone");
            StringAssert.DoesNotContain(
                ScriptGuid<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(),
                scene, "URP camera data must be gone");
        }

        static string ScriptGuid<T>() where T : Component
        {
            var go = new GameObject("script guid probe");
            try
            {
                var comp = go.AddComponent<T>();
                var script = MonoScript.FromMonoBehaviour(comp as MonoBehaviour);
                string path = AssetDatabase.GetAssetPath(script);
                string guid = AssetDatabase.AssetPathToGUID(path);
                Assert.IsNotEmpty(guid, $"no GUID for {typeof(T).Name}");
                return guid;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        static void AssertColor(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 1e-3f);
            Assert.AreEqual(expected.g, actual.g, 1e-3f);
            Assert.AreEqual(expected.b, actual.b, 1e-3f);
        }

        static void Call(MonoBehaviour target, string method)
        {
            var mi = target.GetType().GetMethod(method,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(mi, $"{target.GetType().Name}.{method} not found");
            mi.Invoke(target, null);
        }
    }
}
