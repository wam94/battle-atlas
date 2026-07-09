using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Phase 6 feasibility spike (plan §12 Phase 6, first checklist item):
    // renders the headless-Blender-built spike soldier (CC0 base mesh +
    // scripted rig/garment/march clip) at hero distances (5/8/15 m) under
    // the OFFLINE HDRP profile. Output goes to docs/benchmarks/captures/
    // as p6-spike-*.png; the go/no-go note interprets them.
    //
    // Usage:
    //   Unity -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.SpikeHeroRender.RenderSpike
    public static class SpikeHeroRender
    {
        const int Width = 2560, Height = 1440;   // §6.5 offline target
        const string FbxPath = "Assets/ProjectOwned/Characters/Spike/SpikeSoldier.fbx";
        const int WarmupFrames = 3;

        public static void RenderSpike()
        {
            int exitCode = 0;
            try
            {
                RunInner();
            }
            catch (Exception e)
            {
                Debug.LogError($"SpikeHeroRender failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void RunInner()
        {
            string outDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "../../docs/benchmarks/captures"));
            Directory.CreateDirectory(outDir);

            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(
                HdrpMigration.OfflineAssetPath);
            if (offline == null)
                throw new InvalidOperationException(
                    $"{HdrpMigration.OfflineAssetPath} missing");

            RenderPipelineAsset prevDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset prevQuality = QualitySettings.renderPipeline;
            object prevGlobalSettings = CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            BindHdrpGlobalSettings();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                StageEnvironment();

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
                if (prefab == null)
                    throw new InvalidOperationException($"{FbxPath} not imported");
                var soldier = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                soldier.transform.position = Vector3.zero;
                soldier.transform.rotation = Quaternion.identity;

                foreach (var smr in soldier.GetComponentsInChildren<SkinnedMeshRenderer>())
                    Debug.Log($"spike mesh {smr.name}: verts={smr.sharedMesh.vertexCount} " +
                              $"tris={smr.sharedMesh.triangles.Length / 3} " +
                              $"bones={smr.bones.Length}");

                var clip = AssetDatabase.LoadAllAssetsAtPath(FbxPath)
                    .OfType<AnimationClip>()
                    .FirstOrDefault(c => c.name.Contains("March"));
                if (clip == null)
                    throw new InvalidOperationException(
                        "no March clip found in " + FbxPath + "; clips: " +
                        string.Join(", ", AssetDatabase.LoadAllAssetsAtPath(FbxPath)
                            .OfType<AnimationClip>().Select(c => c.name)));
                Debug.Log($"spike clip '{clip.name}': length={clip.length:F3}s " +
                          $"legacy={clip.legacy} frameRate={clip.frameRate}");

                var camGo = new GameObject("SpikeCamera");
                var cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<HDAdditionalCameraData>();
                cam.fieldOfView = 68f;   // §6.5 viewpoint fov
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 2000f;

                var rt = new RenderTexture(Width, Height, 32);
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

                // warm up pipeline construction before asserting on it
                clip.SampleAnimation(soldier, 0f);
                PlaceCamera(cam, 8f, 30f);
                for (int i = 0; i < WarmupFrames; i++) RenderOnce(cam, rt, null);
                if (!(RenderPipelineManager.currentPipeline is HDRenderPipeline))
                    throw new InvalidOperationException(
                        "HDRP did not construct; run with -buildTarget OSXUniversal");

                // hero-distance matrix: distance x march phase x azimuth
                float[] distances = { 5f, 8f, 15f };
                float[] phases = { 0f, 0.25f, 0.5f, 0.75f };
                foreach (float dist in distances)
                {
                    foreach (float phase in phases)
                    {
                        clip.SampleAnimation(soldier, phase * clip.length);
                        PlaceCamera(cam, dist, 30f);
                        RenderOnce(cam, rt, tex);
                        string path = Path.Combine(outDir,
                            $"p6-spike-{(int)dist}m-p{(int)(phase * 100):D2}.png");
                        File.WriteAllBytes(path, tex.EncodeToPNG());
                        Debug.Log($"spike: wrote {path}");
                    }
                }
                // orientation sweep at 8 m, phase 0 (front/side/back read)
                foreach (float az in new[] { 0f, 90f, 180f, 270f })
                {
                    clip.SampleAnimation(soldier, 0f);
                    PlaceCamera(cam, 8f, az);
                    RenderOnce(cam, rt, tex);
                    string path = Path.Combine(outDir, $"p6-spike-az{(int)az:D3}.png");
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    Debug.Log($"spike: wrote {path}");
                }
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline = prevDefault;
                QualitySettings.renderPipeline = prevQuality;
                CurrentGlobalSettingsProp.SetValue(null, prevGlobalSettings);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        static void PlaceCamera(Camera cam, float distance, float azimuthDeg)
        {
            // eye height 1.66 m (§6.5), orbiting the figure at chest focus
            float a = azimuthDeg * Mathf.Deg2Rad;
            cam.transform.position = new Vector3(
                distance * Mathf.Sin(a), 1.66f, distance * Mathf.Cos(a));
            cam.transform.LookAt(new Vector3(0f, 1.15f, 0f));
        }

        static void StageEnvironment()
        {
            // afternoon sun (bake-off convention: physical lux + fixed EV)
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(46f, 205f, 0f);
            var hd = sunGo.AddComponent<HDAdditionalLightData>();
            hd.SetIntensity(95000f, LightUnit.Lux);
            hd.angularDiameter = 0.53f;

            var groundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGo.name = "Ground";
            groundGo.transform.localScale = new Vector3(40f, 1f, 40f); // 400 m
            var mat = new Material(Shader.Find("HDRP/Lit"));
            mat.SetColor("_BaseColor", new Color(0.30f, 0.27f, 0.17f)); // dry field
            mat.SetFloat("_Smoothness", 0.05f);
            groundGo.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var volGo = new GameObject("Spike Volume");
            var vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var env = profile.Add<VisualEnvironment>(true);
            env.skyType.value = (int)SkyType.PhysicallyBased;
            env.skyAmbientMode.value = SkyAmbientMode.Dynamic;
            profile.Add<PhysicallyBasedSky>(true);

            var exposure = profile.Add<Exposure>(true);
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.value = 13.2f; // project sunny-afternoon EV100

            var shadows = profile.Add<HDShadowSettings>(true);
            shadows.maxShadowDistance.value = 150f;

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.value = TonemappingMode.ACES;

            vol.sharedProfile = profile;
        }

        static void RenderOnce(Camera cam, RenderTexture rt, Texture2D readInto)
        {
            var request = new RenderPipeline.StandardRequest { destination = rt };
            if (!RenderPipeline.SupportsRenderRequest(cam, request))
                throw new InvalidOperationException(
                    "active render pipeline does not support StandardRequest");
            RenderPipeline.SubmitRenderRequest(cam, request);
            if (readInto != null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                readInto.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                readInto.Apply(false);
                RenderTexture.active = prev;
            }
        }

        // Headless global-settings binding (see AtlasHdrpRender for the full
        // rationale); version-pinned to Unity 6000.4, fails loudly.
        static PropertyInfo CurrentGlobalSettingsProp =>
            typeof(GraphicsSettings).GetProperty(
                "currentRenderPipelineGlobalSettings",
                BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "GraphicsSettings.currentRenderPipelineGlobalSettings not found; " +
                "Unity version changed — revisit the headless binding mechanism");

        static void BindHdrpGlobalSettings()
        {
            var gs = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            if (gs == null)
                throw new InvalidOperationException(
                    "no HDRP global settings registered (Assets/HDRPDefaultResources)");
            CurrentGlobalSettingsProp.SetValue(null, gs);
        }
    }
}
