using System;
using System.IO;
using System.Security.Cryptography;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Gate P7 evidence renderer (plan §12 Phase 7 gate: "the local scene is
    // recognizably the Angle without units or UI, at both tactical and eye
    // height"). Renders stills of the SOURCED environment — no figures —
    // under the offline HDRP profile and the 15:20 ephemeris sun:
    //
    //   p7-tactical    the assault corridor at drone height (bakeoff def)
    //   p7-eye-p3cam   THE Phase 3 gate camera, unchanged — the ADR 0003
    //                  deferred items (wall/road in frame, shadow
    //                  legibility) are judged against this frame
    //   p7-eye-road    eye height standing IN the Emmitsburg Road at the
    //                  crossing: fences, sunken roadbed, the grass slope,
    //                  wall, copse
    //   p7-eye-codori  eye height looking back west at the Codori farmyard,
    //                  orchard, and road
    //
    // Usage:
    //   Unity -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.GateP7Render.RenderStills
    //     -logFile p7render.log
    // Output: docs/benchmarks/captures/p7-gate/
    public static class GateP7Render
    {
        const int Width = 2560, Height = 1440;
        const int WarmupFrames = 3;

        static readonly AngleBakeoffLayout.CameraDef[] Cameras =
        {
            new AngleBakeoffLayout.CameraDef
            {
                name = "p7-tactical",
                posXZ = new Vector2(290f, 290f), heightAboveGround = 45f,
                lookXZ = new Vector2(515f, 415f), lookHeightAboveGround = 3f,
                fovDeg = 45f,
            },
            new AngleBakeoffLayout.CameraDef
            {
                // identical to AngleBakeoffLayout.Cameras[2] (Gate P3 eye)
                name = "p7-eye-p3cam",
                posXZ = new Vector2(417f, 360f), heightAboveGround = 1.66f,
                lookXZ = new Vector2(513f, 408f), lookHeightAboveGround = 1.5f,
                fovDeg = 68f,
            },
            new AngleBakeoffLayout.CameraDef
            {
                // standing in the roadbed at the crossing (macro ~4114,4838)
                name = "p7-eye-road",
                posXZ = new Vector2(214f, 388f), heightAboveGround = 1.66f,
                lookXZ = new Vector2(500f, 440f), lookHeightAboveGround = 2f,
                fovDeg = 68f,
            },
            new AngleBakeoffLayout.CameraDef
            {
                // looking back WSW at the Codori farmyard and orchard
                name = "p7-eye-codori",
                posXZ = new Vector2(258f, 262f), heightAboveGround = 1.66f,
                lookXZ = new Vector2(110f, 190f), lookHeightAboveGround = 4f,
                fovDeg = 68f,
            },
        };

        public static void RenderStills()
        {
            int exitCode = 0;
            try
            {
                RenderInner();
            }
            catch (Exception e)
            {
                Debug.LogError($"GateP7Render failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void RenderInner()
        {
            string outDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "../../docs/benchmarks/captures/p7-gate"));
            Directory.CreateDirectory(outDir);

            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(
                HdrpMigration.OfflineAssetPath);
            if (offline == null)
                throw new InvalidOperationException(
                    $"{HdrpMigration.OfflineAssetPath} missing");

            RenderPipelineAsset prevDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset prevQuality = QualitySettings.renderPipeline;
            object prevGlobal = GateP6Render.CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            GateP6Render.BindHdrpGlobalSettings();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var stage = AngleEnvironmentStage.StageAll();
                Debug.Log($"GateP7Render: staged in {sw.Elapsed.TotalSeconds:F1} s");

                var rt = new RenderTexture(Width, Height, 32);
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                var cam = stage.camera;

                AngleBakeoffStage.ApplyCamera(cam, stage.terrain, Cameras[0]);
                for (int i = 0; i < WarmupFrames; i++)
                    GateP6Render.RenderOnce(cam, rt, null);
                if (!(RenderPipelineManager.currentPipeline is HDRenderPipeline))
                    throw new InvalidOperationException(
                        "HDRP did not construct; run with -buildTarget OSXUniversal");

                foreach (var def in Cameras)
                {
                    AngleBakeoffStage.ApplyCamera(cam, stage.terrain, def);
                    var frameSw = System.Diagnostics.Stopwatch.StartNew();
                    GateP6Render.RenderOnce(cam, rt, null); // settle exposure
                    GateP6Render.RenderOnce(cam, rt, tex);
                    string p = Path.Combine(outDir, $"{def.name}.png");
                    File.WriteAllBytes(p, tex.EncodeToPNG());
                    Debug.Log($"GateP7Render: wrote {p} " +
                              $"({frameSw.Elapsed.TotalSeconds:F1} s)");
                }

                // provenance sidecar: what data produced these frames
                string envPath = Path.Combine(
                    AngleEnvironmentStage.CropDir, "environment.json");
                string envSha;
                using (var sha = SHA256.Create())
                    envSha = BitConverter.ToString(
                        sha.ComputeHash(File.ReadAllBytes(envPath)))
                        .Replace("-", "").ToLowerInvariant();
                string report =
                    "{\n" +
                    $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                    $"  \"pipelineAsset\": \"{HdrpMigration.OfflineAssetPath}\",\n" +
                    $"  \"sunBattleT\": {AngleBakeoffLayout.ComparisonBattleT},\n" +
                    $"  \"environmentJsonSha256\": \"{envSha}\",\n" +
                    $"  \"width\": {Width}, \"height\": {Height}\n" +
                    "}\n";
                File.WriteAllText(Path.Combine(outDir, "p7-gate-report.json"), report);
                Debug.Log("GateP7Render: done");
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
