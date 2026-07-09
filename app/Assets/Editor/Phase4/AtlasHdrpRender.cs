using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Gate P4 determinism evidence: renders the ATLAS scene headlessly under
    // the OFFLINE HDRP profile at a fixed battle time, twice, and
    // byte-compares the frames (plan §12 Phase 4 / Gate P10's standard).
    //
    // The scene is driven the way the EditMode suite already drives
    // BattleDirector headlessly: lifecycle methods invoked directly (Start
    // once, then the per-frame Updates) with the BattleClock pinned — every
    // renderer poses purely from the clock, so two invocations at the same t
    // must produce identical draw lists. Nothing is saved: the scene is
    // reopened cleanly afterward and the pipeline binding is restored.
    //
    // Usage:
    //   Unity -batchmode -projectPath app -executeMethod
    //     BattleAtlas.EditorTools.AtlasHdrpRender.CaptureDeterminismPair
    // Output: docs/benchmarks/captures/hdrp-atlas-offline-t{T}-{a,b}.png and
    // hdrp-atlas-determinism.json (byte equality + pixel-diff stats).
    public static class AtlasHdrpRender
    {
        const int Width = 2560, Height = 1440; // §6.5 offline target
        const float CaptureT = 8700f;          // the Angle crisis frame
        const int WarmupFrames = 3;

        public static void CaptureDeterminismPair()
        {
            int exitCode = 0;
            try
            {
                RunInner();
            }
            catch (Exception e)
            {
                Debug.LogError($"AtlasHdrpRender failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        [Serializable]
        class Report
        {
            public string unityVersion;
            public string pipelineAsset;
            public float battleT;
            public int width;
            public int height;
            public bool bytesIdentical;
            public int differingPixels;
            public float differingPixelsPercent;
            public int maxChannelDelta;
            public string method;
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
                    $"{HdrpMigration.OfflineAssetPath} missing; run HdrpMigration first");
            var offlineVolume = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                HdrpMigration.OfflineVolumeProfilePath);
            if (offlineVolume == null)
                throw new InvalidOperationException(
                    $"{HdrpMigration.OfflineVolumeProfilePath} missing");

            RenderPipelineAsset prevDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset prevQuality = QualitySettings.renderPipeline;
            object prevGlobalSettings = CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            BindHdrpGlobalSettings();
            try
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Atlas.unity");

                // layer the offline volume over the scene's playback volume
                // (transient; the scene is never saved from this method)
                var volGo = new GameObject("Offline Render Volume (transient)");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                vol.priority = 100f;
                vol.sharedProfile = offlineVolume;

                var ctx = StageAtlas(CaptureT);

                var rt = new RenderTexture(Width, Height, 32);
                var texA = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                var texB = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

                for (int i = 0; i < WarmupFrames; i++)
                    RenderOnce(ctx, rt, null);
                RenderOnce(ctx, rt, texA);
                RenderOnce(ctx, rt, texB);

                string tag = ((int)CaptureT).ToString();
                string pathA = Path.Combine(outDir, $"hdrp-atlas-offline-t{tag}-a.png");
                string pathB = Path.Combine(outDir, $"hdrp-atlas-offline-t{tag}-b.png");
                File.WriteAllBytes(pathA, texA.EncodeToPNG());
                File.WriteAllBytes(pathB, texB.EncodeToPNG());

                var report = Compare(texA, texB);
                report.unityVersion = Application.unityVersion;
                report.pipelineAsset = HdrpMigration.OfflineAssetPath;
                report.battleT = CaptureT;
                report.width = Width;
                report.height = Height;
                report.method =
                    "editor batchmode; Atlas.unity lifecycle driven headlessly at fixed " +
                    "BattleClock t; offline HDRP asset + offline volume bound; " +
                    $"{WarmupFrames} warmup submissions, then two timed submissions " +
                    "readback-compared (raw RGBA32)";
                File.WriteAllText(
                    Path.Combine(outDir, "hdrp-atlas-determinism.json"),
                    JsonUtility.ToJson(report, true));
                Debug.Log($"AtlasHdrpRender: bytesIdentical={report.bytesIdentical} " +
                          $"differingPixels={report.differingPixels} " +
                          $"({report.differingPixelsPercent:F4}%) " +
                          $"maxChannelDelta={report.maxChannelDelta}");
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline = prevDefault;
                QualitySettings.renderPipeline = prevQuality;
                CurrentGlobalSettingsProp.SetValue(null, prevGlobalSettings);
                // drop every transient object this run created
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        class AtlasContext
        {
            public Camera camera;
            public MonoBehaviour[] updateOrder;
        }

        // Start once, then per-frame Updates in dependency order — the same
        // headless-lifecycle pattern the EditMode suite uses. AcousticField
        // and TimelineHud are skipped (audio/IMGUI: nothing to render).
        static AtlasContext StageAtlas(float t)
        {
            var clock = UnityEngine.Object.FindAnyObjectByType<BattleClock>();
            var director = UnityEngine.Object.FindAnyObjectByType<BattleDirector>();
            var sunDirector = UnityEngine.Object.FindAnyObjectByType<SunDirector>();
            var veg = UnityEngine.Object.FindAnyObjectByType<VegetationField>();
            var fence = UnityEngine.Object.FindAnyObjectByType<FenceField>();
            var crop = UnityEngine.Object.FindAnyObjectByType<CropField>();
            var camera = Camera.main;
            if (clock == null || director == null || sunDirector == null || camera == null)
                throw new InvalidOperationException(
                    "Atlas.unity missing BattleClock/BattleDirector/SunDirector/MainCamera");

            Call(director, "Start"); // also adds + inits ObscurationField/AcousticField
            Call(sunDirector, "Start");
            if (veg != null) Call(veg, "Start");
            if (fence != null) Call(fence, "Start");
            if (crop != null) Call(crop, "Start");

            clock.Playing = false;
            clock.CurrentTime = t;

            var obscuration = director.GetComponent<ObscurationField>();
            return new AtlasContext
            {
                camera = camera,
                updateOrder = new MonoBehaviour[]
                {
                    sunDirector, director, obscuration, veg, fence, crop,
                },
            };
        }

        static void RenderOnce(AtlasContext ctx, RenderTexture rt, Texture2D readInto)
        {
            // instanced draws are valid for the frame they are issued in:
            // re-run the clock-driven Updates, then submit synchronously
            foreach (MonoBehaviour b in ctx.updateOrder)
                if (b != null) Call(b, "Update");
            var request = new RenderPipeline.StandardRequest { destination = rt };
            if (!RenderPipeline.SupportsRenderRequest(ctx.camera, request))
                throw new InvalidOperationException(
                    "active render pipeline does not support StandardRequest");
            RenderPipeline.SubmitRenderRequest(ctx.camera, request);
            if (readInto != null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                readInto.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                readInto.Apply(false);
                RenderTexture.active = prev;
            }
        }

        static Report Compare(Texture2D a, Texture2D b)
        {
            byte[] rawA = a.GetRawTextureData();
            byte[] rawB = b.GetRawTextureData();
            if (rawA.Length != rawB.Length)
                throw new InvalidOperationException("frame size mismatch");
            int maxDelta = 0;
            int differingPixels = 0;
            for (int p = 0; p < rawA.Length; p += 4)
            {
                bool differs = false;
                for (int c = 0; c < 4; c++)
                {
                    int d = Math.Abs(rawA[p + c] - rawB[p + c]);
                    if (d > 0) differs = true;
                    if (d > maxDelta) maxDelta = d;
                }
                if (differs) differingPixels++;
            }
            int totalPixels = rawA.Length / 4;
            return new Report
            {
                bytesIdentical = differingPixels == 0,
                differingPixels = differingPixels,
                differingPixelsPercent = 100f * differingPixels / totalPixels,
                maxChannelDelta = maxDelta,
            };
        }

        // private lifecycle methods (Start/Update) invoked directly — the
        // EditMode headless pattern
        static void Call(MonoBehaviour target, string method)
        {
            var mi = target.GetType().GetMethod(
                method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (mi == null)
                throw new InvalidOperationException($"{target.GetType().Name}.{method} not found");
            mi.Invoke(target, null);
        }

        // Same headless global-settings binding the Phase 3 bake-off needed
        // (batchmode never runs the C++ loop that binds it): version-pinned
        // to Unity 6000.4, fails loudly if the property disappears.
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
