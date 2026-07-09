using System;
using System.Diagnostics;
using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.Universal;
using Debug = UnityEngine.Debug;

namespace BattleAtlas.EditorTools
{
    // Headless bake-off renderer (plan §12 Phase 3): produces the SAME
    // theater/tactical/eye-level frames from URP and HDRP so Gate P3 can be
    // judged on identical staged content.
    //
    // Pipeline-swap mechanism (documented per the phase instructions): the
    // project's persisted settings stay URP. For each run this script sets
    // GraphicsSettings.defaultRenderPipeline and QualitySettings.renderPipeline
    // to an IN-MEMORY pipeline asset — a copy of the project's PC_RPAsset with
    // a bake-off shadow distance for URP, or a default-settings
    // HDRenderPipelineAsset for HDRP — renders, and restores the previous
    // references in a finally block. Nothing is saved to ProjectSettings, so
    // the checked-in project remains URP-active regardless of how the run
    // ends. HDRP global settings (required once per project for HDRP to
    // render) are ensured on first use and committed as a normal asset.
    //
    // Usage:
    //   Unity -batchmode -projectPath app -executeMethod
    //     BattleAtlas.EditorTools.AngleBakeoffRender.RenderUrp   (or RenderHdrp)
    //   frames + measurements land in docs/benchmarks/captures/.
    public static class AngleBakeoffRender
    {
        const int Width = 2560, Height = 1440; // §6.5 full-media target
        const int WarmupFrames = 3, TimedFrames = 8;

        public static void RenderUrp() => Run(BakeoffPipeline.Urp);
        public static void RenderHdrp() => Run(BakeoffPipeline.Hdrp);

        static void Run(BakeoffPipeline p)
        {
            int exitCode = 0;
            try
            {
                RunInner(p);
            }
            catch (Exception e)
            {
                Debug.LogError($"bake-off render ({p}) failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        [Serializable]
        class CameraMeasure
        {
            public string camera;
            public float avgMs;
            public float minMs;
            public float maxMs;
        }

        [Serializable]
        class Report
        {
            public string pipeline;
            public string unityVersion;
            public int width;
            public int height;
            public float comparisonBattleT;
            public float secondsSinceMidnight;
            public int warmupFrames;
            public int timedFrames;
            public CameraMeasure[] cameras;
            public long allocatedMB;
            public long reservedMB;
            public string method;
        }

        static void RunInner(BakeoffPipeline p)
        {
            string outDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "../../docs/benchmarks/captures"));
            Directory.CreateDirectory(outDir);

            // new scene FIRST: creating one unloads unreferenced in-memory
            // assets, which would destroy the pipeline asset created below
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderPipelineAsset prevDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset prevQuality = QualitySettings.renderPipeline;
            object prevGlobalSettings = CurrentGlobalSettingsProp.GetValue(null);
            RenderPipelineAsset active = p == BakeoffPipeline.Urp
                ? MakeUrpAsset() : MakeHdrpAsset();
            active.hideFlags = HideFlags.HideAndDontSave;
            GraphicsSettings.defaultRenderPipeline = active;
            QualitySettings.renderPipeline = active;
            BindGlobalSettings(p);
            try
            {
                var ctx = AngleBakeoffStage.Stage(p);

                var rt = new RenderTexture(Width, Height, 32);
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                string tag = p == BakeoffPipeline.Urp ? "urp" : "hdrp";
                var measures = new CameraMeasure[AngleBakeoffLayout.Cameras.Length];

                for (int c = 0; c < AngleBakeoffLayout.Cameras.Length; c++)
                {
                    var def = AngleBakeoffLayout.Cameras[c];
                    AngleBakeoffStage.ApplyCamera(ctx.camera, ctx.terrain, def);

                    for (int i = 0; i < WarmupFrames; i++)
                        RenderOnce(ctx, rt);

                    float total = 0f, min = float.MaxValue, max = 0f;
                    var sw = new Stopwatch();
                    for (int i = 0; i < TimedFrames; i++)
                    {
                        sw.Restart();
                        RenderOnce(ctx, rt);
                        // full readback forces CPU/GPU sync: the timed cost is
                        // an honest offline seconds-per-saved-frame number
                        ReadBack(rt, tex);
                        sw.Stop();
                        float ms = (float)sw.Elapsed.TotalMilliseconds;
                        total += ms;
                        min = Mathf.Min(min, ms);
                        max = Mathf.Max(max, ms);
                    }
                    measures[c] = new CameraMeasure
                    {
                        camera = def.name,
                        avgMs = total / TimedFrames,
                        minMs = min,
                        maxMs = max,
                    };

                    string path = Path.Combine(outDir, $"p3-{def.name}-{tag}.png");
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    Debug.Log($"bake-off: wrote {path} " +
                              $"(avg {measures[c].avgMs:F0} ms/frame over {TimedFrames})");
                }

                var report = new Report
                {
                    pipeline = tag,
                    unityVersion = Application.unityVersion,
                    width = Width,
                    height = Height,
                    comparisonBattleT = AngleBakeoffLayout.ComparisonBattleT,
                    secondsSinceMidnight = AngleBakeoffLayout.SecondsSinceMidnight,
                    warmupFrames = WarmupFrames,
                    timedFrames = TimedFrames,
                    cameras = measures,
                    allocatedMB = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024),
                    reservedMB = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024),
                    method = "editor batchmode; RenderPipeline.SubmitRenderRequest " +
                             "(StandardRequest) + full ReadPixels sync per timed frame; " +
                             "instanced soldiers/smoke issued via Graphics.RenderMeshInstanced " +
                             "immediately before each submit",
                };
                File.WriteAllText(
                    Path.Combine(outDir, $"p3-measurements-{tag}.json"),
                    JsonUtility.ToJson(report, true));
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline = prevDefault;
                QualitySettings.renderPipeline = prevQuality;
                CurrentGlobalSettingsProp.SetValue(null, prevGlobalSettings);
            }
        }

        // GraphicsSettings.currentRenderPipelineGlobalSettings is the native
        // pointer TryGetRenderPipelineSettings reads. Interactively the C++
        // render loop binds it when the active pipeline changes; that loop
        // never runs in batchmode before our first render, so URP/HDRP
        // construction NREs on unresolvable runtime-shader settings. The
        // property is internal — reflection is the only binding path from
        // script. Version-pinned to Unity 6000.4 (verified in
        // UnityCsReference); fails loudly if the property disappears.
        static System.Reflection.PropertyInfo CurrentGlobalSettingsProp =>
            typeof(GraphicsSettings).GetProperty(
                "currentRenderPipelineGlobalSettings",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "GraphicsSettings.currentRenderPipelineGlobalSettings not found; " +
                "Unity version changed — revisit the bake-off binding mechanism");

        static void BindGlobalSettings(BakeoffPipeline p)
        {
            RenderPipelineGlobalSettings gs;
            if (p == BakeoffPipeline.Urp)
            {
                gs = GraphicsSettings.GetSettingsForRenderPipeline<
                    UnityEngine.Rendering.Universal.UniversalRenderPipeline>();
            }
            else
            {
                gs = EnsureHdrpGlobalSettings();
            }
            if (gs == null)
                throw new InvalidOperationException(
                    $"no render pipeline global settings available for {p}");
            CurrentGlobalSettingsProp.SetValue(null, gs);
        }

        // HDRenderPipelineGlobalSettings and its Ensure() are internal to
        // HDRP; Ensure() creates + registers the global settings asset
        // (Assets/HDRPDefaultResources/...) exactly as interactive HDRP
        // activation would. The created asset is committed on this branch.
        static RenderPipelineGlobalSettings EnsureHdrpGlobalSettings()
        {
            var existing = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            if (existing != null) return existing;

            var type = typeof(HDRenderPipeline).Assembly.GetType(
                "UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings");
            var ensure = type?.GetMethod(
                "Ensure",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);
            if (ensure == null)
                throw new InvalidOperationException(
                    "HDRenderPipelineGlobalSettings.Ensure not found; HDRP version changed");
            var args = new object[ensure.GetParameters().Length];
            for (int i = 0; i < args.Length; i++)
            {
                var param = ensure.GetParameters()[i];
                args[i] = param.HasDefaultValue ? param.DefaultValue : null;
            }
            var result = ensure.Invoke(null, args) as RenderPipelineGlobalSettings;
            return result != null
                ? result
                : GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
        }

        static void RenderOnce(AngleBakeoffStage.StageResult ctx, RenderTexture rt)
        {
            // instanced draws are valid for the frame they are issued in;
            // issue then render synchronously within this editor frame.
            // Camera.Render() with a targetTexture routes through the active
            // SRP (and constructs it on first use) — unlike
            // RenderPipeline.SubmitRenderRequest, which silently no-ops in
            // batchmode before any pipeline instance exists.
            ctx.figures.IssueDraws();
            var request = new RenderPipeline.StandardRequest { destination = rt };
            // SupportsRenderRequest constructs the pipeline if needed (it
            // calls TryPrepareRenderPipeline internally) — with global
            // settings bound, this is the documented render-request path
            if (!RenderPipeline.SupportsRenderRequest(ctx.camera, request))
                throw new InvalidOperationException(
                    "active render pipeline does not support StandardRequest");
            RenderPipeline.SubmitRenderRequest(ctx.camera, request);
        }

        static void ReadBack(RenderTexture rt, Texture2D tex)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply(false);
            RenderTexture.active = prev;
        }

        static RenderPipelineAsset MakeUrpAsset()
        {
            var src = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                "Assets/Settings/PC_RPAsset.asset");
            if (src == null)
                throw new InvalidOperationException("Assets/Settings/PC_RPAsset.asset not found");
            var copy = UnityEngine.Object.Instantiate(src);
            copy.name = "PC_RPAsset (bakeoff copy)";
            // eye-level frames need long shadows; the committed asset is
            // tuned for the macro Atlas
            copy.shadowDistance = 1200f;
            copy.msaaSampleCount = 1; // parity with HDRP (no MSAA in deferred)
            return copy;
        }

        static RenderPipelineAsset MakeHdrpAsset()
        {
            // HDRP's editor-side pipeline constructor ensures the global
            // settings asset (Assets/HDRPDefaultResources/...) on first
            // activation; that generated asset is committed on this branch
            var asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
            asset.name = "HDRP Bakeoff (in-memory)";
            return asset;
        }

        // --- Committed thin scenes (named entry points for the owner) ---

        public static void BuildScenes()
        {
            int exitCode = 0;
            try
            {
                BuildScene(BakeoffPipeline.Urp, "Assets/Scenes/AngleVisualTarget-URP.unity");
                BuildScene(BakeoffPipeline.Hdrp, "Assets/Scenes/AngleVisualTarget-HDRP.unity");
                AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                Debug.LogError($"BuildScenes failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void BuildScene(BakeoffPipeline p, string path)
        {
            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var marker = new GameObject(
                $"AngleVisualTarget ({p}) — stage via BattleAtlas ▸ Bakeoff menu");
            var note = marker.AddComponent<AngleBakeoffSceneMarker>();
            note.pipeline = p.ToString();
            EditorSceneManager.SaveScene(scene, path);
        }

        [MenuItem("BattleAtlas/Bakeoff/Stage Content For Open Scene Marker")]
        public static void StageForMarker()
        {
            var marker = UnityEngine.Object.FindFirstObjectByType<AngleBakeoffSceneMarker>();
            if (marker == null)
            {
                Debug.LogError("no AngleBakeoffSceneMarker in the open scene; " +
                               "open AngleVisualTarget-URP/-HDRP first");
                return;
            }
            AngleBakeoffStage.Stage(marker.pipeline == "Hdrp"
                ? BakeoffPipeline.Hdrp : BakeoffPipeline.Urp);
        }

        // Session-only pipeline switch for interactive HDRP viewing: nothing
        // is saved; restarting the editor (or Restore URP) returns to the
        // project's committed URP settings.
        [MenuItem("BattleAtlas/Bakeoff/Use HDRP For This Session")]
        public static void UseHdrpSession()
        {
            var asset = (RenderPipelineAsset)MakeHdrpAsset();
            GraphicsSettings.defaultRenderPipeline = asset;
            QualitySettings.renderPipeline = asset;
            Debug.Log("HDRP active for this session (not saved). " +
                      "Use 'Restore URP (Session)' to switch back.");
        }

        [MenuItem("BattleAtlas/Bakeoff/Restore URP (Session)")]
        public static void RestoreUrpSession()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                "Assets/Settings/PC_RPAsset.asset");
            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = null;
            Debug.Log("URP restored for this session.");
        }
    }
}
