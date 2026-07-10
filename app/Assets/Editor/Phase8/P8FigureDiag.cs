using System;
using System.IO;
using System.Linq;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Phase 8 figure diagnostic: the six kit variants lined up at close
    // range, posed through the same clip path the action stage uses.
    // Chases the Gate P6 watch-item (doubled hat / bare feet = kit-build
    // bug) before the gate render.
    public static class P8FigureDiag
    {
        public static void Render()
        {
            int exitCode = 0;
            try { Inner(); }
            catch (Exception e)
            {
                Debug.LogError($"P8FigureDiag failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void Inner()
        {
            string outDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "../../docs/benchmarks/captures/p8-gate"));
            Directory.CreateDirectory(outDir);
            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(
                HdrpMigration.OfflineAssetPath);
            var prevDefault = GraphicsSettings.defaultRenderPipeline;
            var prevQuality = QualitySettings.renderPipeline;
            object prevGlobal = GateP6Render.CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            GateP6Render.BindHdrpGlobalSettings();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var sunGo = new GameObject("sun");
                var light = sunGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.shadows = LightShadows.Soft;
                sunGo.transform.rotation = Quaternion.Euler(45f, 200f, 0f);
                var hd = sunGo.AddComponent<HDAdditionalLightData>();
                hd.SetIntensity(80000f, LightUnit.Lux);
                var volGo = new GameObject("vol");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                var envv = profile.Add<VisualEnvironment>(true);
                envv.skyType.value = (int)SkyType.PhysicallyBased;
                profile.Add<PhysicallyBasedSky>(true);
                var exposure = profile.Add<UnityEngine.Rendering.HighDefinition.Exposure>(true);
                exposure.mode.value = ExposureMode.Fixed;
                exposure.fixedExposure.value = 13.2f;
                vol.sharedProfile = profile;
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.transform.localScale = new Vector3(10f, 1f, 10f);

                string[] variants = { "csa_a", "csa_b", "csa_c",
                                      "union_a", "union_b", "union_c",
                                      "csa_a_near", "union_a_near" };
                string[] clips = { "March_ShoulderArms", "Fire_Recoil" };
                var figures = new System.Collections.Generic.List<GameObject>();
                for (int i = 0; i < variants.Length; i++)
                {
                    string v = variants[i];
                    var go = GateP6Render.SpawnVariant(v);
                    figures.Add(go);
                    go.transform.position = new Vector3(i * 1.4f, 0f, 0f);
                    go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    string clipVariant = v.EndsWith("_near")
                        ? v.Substring(0, v.Length - 5) : v;
                    var lib = GateP6Render.LoadClips(clipVariant);
                    var clip = lib[clips[i % 2]];
                    clip.SampleAnimation(go, 0.45f);
                    // log renderer inventory of the first figure
                    if (i == 0 || i == 6)
                    {
                        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                            Debug.Log($"P8FigureDiag: {v} renderer '{r.gameObject.name}' " +
                                      $"enabled={r.enabled} bounds={r.bounds.size}");
                    }
                }

                // --- transparent-material shakedown: three candidate smoke
                // materials on big quads behind the lineup ---
                var tex0 = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                var px = new Color32[64 * 64];
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        float dx = x / 63f - 0.5f, dy = y / 63f - 0.5f;
                        float a = Mathf.Clamp01(1f - 2f * Mathf.Sqrt(dx * dx + dy * dy));
                        px[y * 64 + x] = new Color32(255, 255, 255,
                            (byte)(a * 255));
                    }
                tex0.SetPixels32(px);
                tex0.Apply();

                Material LitT()
                {
                    var m = new Material(Shader.Find("HDRP/Lit"));
                    m.SetColor("_BaseColor", new Color(0.9f, 0.9f, 0.9f, 0.6f));
                    m.SetTexture("_BaseColorMap", tex0);
                    m.SetFloat("_SurfaceType", 1f);
                    m.SetFloat("_BlendMode", 0f);
                    m.SetFloat("_ZWrite", 0f);
                    m.renderQueue = (int)RenderQueue.Transparent;
                    HDMaterial.ValidateMaterial(m);
                    return m;
                }
                Material UnlitT()
                {
                    var m = new Material(Shader.Find("HDRP/Unlit"));
                    m.SetColor("_UnlitColor", new Color(0.9f, 0.9f, 0.9f, 0.6f));
                    m.SetTexture("_UnlitColorMap", tex0);
                    m.SetFloat("_SurfaceType", 1f);
                    m.SetFloat("_BlendMode", 0f);
                    m.SetFloat("_ZWrite", 0f);
                    m.renderQueue = (int)RenderQueue.Transparent;
                    HDMaterial.ValidateMaterial(m);
                    return m;
                }
                var mats = new[] { LitT(), UnlitT() };
                for (int i = 0; i < mats.Length; i++)
                {
                    var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    q.name = $"smoketest_{i}";
                    q.transform.position = new Vector3(2f + i * 3f, 2f, -2.5f);
                    q.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    q.transform.localScale = Vector3.one * 2.5f;
                    q.GetComponent<MeshRenderer>().sharedMaterial = mats[i];
                    Debug.Log($"P8FigureDiag: smoketest_{i} shader=" +
                              $"{mats[i].shader.name} queue={mats[i].renderQueue} " +
                              $"keywords=[{string.Join(" ", mats[i].shaderKeywords)}]");
                }

                var camGo = new GameObject("cam");
                var cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<HDAdditionalCameraData>();
                cam.transform.position = new Vector3(4.9f, 1.5f, 6.5f);
                cam.transform.LookAt(new Vector3(4.9f, 1.0f, 0f));
                cam.fieldOfView = 55f;
                cam.nearClipPlane = 0.05f;

                var rt = new RenderTexture(2560, 1440, 32);
                var tex = new Texture2D(2560, 1440, TextureFormat.RGBA32, false);
                for (int i = 0; i < 3; i++) GateP6Render.RenderOnce(cam, rt, null);
                GateP6Render.RenderOnce(cam, rt, tex);
                File.WriteAllBytes(Path.Combine(outDir, "p8-figure-lineup.png"),
                    tex.EncodeToPNG());
                Debug.Log("P8FigureDiag: wrote p8-figure-lineup.png");

                // side profile of the same lineup (hat brim/visor and boot
                // silhouette read best in profile — the P8 review-fix
                // verification wants front AND side)
                foreach (var go in figures)
                    go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                GateP6Render.RenderOnce(cam, rt, null);
                GateP6Render.RenderOnce(cam, rt, tex);
                File.WriteAllBytes(
                    Path.Combine(outDir, "p8-figure-lineup-side.png"),
                    tex.EncodeToPNG());
                Debug.Log("P8FigureDiag: wrote p8-figure-lineup-side.png");
            }
            finally
            {
                var playback = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
                    HdrpMigration.PlaybackAssetPath);
                GraphicsSettings.defaultRenderPipeline =
                    playback != null ? playback : prevDefault;
                QualitySettings.renderPipeline = prevQuality;
                GateP6Render.CurrentGlobalSettingsProp.SetValue(null, prevGlobal);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }
}
