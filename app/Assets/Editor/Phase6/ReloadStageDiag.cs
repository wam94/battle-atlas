using System;
using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Gate P6 defect-2 evidence tool: renders the 9-stage reload clip in
    // ISOLATION (one soldier, close camera, offline HDRP profile, staged
    // terrain light) — one still per drill stage, so a stranger can name
    // each stage or see exactly where it breaks. Also renders a full
    // clip-sweep of one hero variant (every authored clip x 4 phases) for
    // the defect-1 poke-through inspection.
    //
    //   Unity -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.ReloadStageDiag.RenderReloadStages
    //     -executeMethod BattleAtlas.EditorTools.ReloadStageDiag.RenderClipSweep
    public static class ReloadStageDiag
    {
        const int W = 1200, H = 1500;
        const int WarmupFrames = 3;

        // mirror of characters/kit/clips.py RELOAD_STAGES (+ a mid-stage
        // probe inside longer stages so two-beat stages show both beats)
        static readonly (string name, float t)[] Stages =
        {
            ("1_load",             0.0f),
            ("2_handle_cartridge", 2.9f),
            ("3_tear_cartridge",   4.8f),
            ("4_charge_cartridge", 7.4f),
            ("5_draw_rammer",     10.6f),
            ("6_ram_cartridge",   12.1f),
            ("7_return_rammer",   14.7f),
            ("8_prime",           16.9f),
            ("9_shoulder",        19.5f),
        };

        public static void RenderReloadStages()
        {
            RunIsolated((render, outDir) =>
            {
                var go = GateP6Render.SpawnVariant("csa_a");
                var clips = GateP6Render.LoadClips("csa_a");
                var reload = clips["Reload_Musket"];
                Debug.Log($"ReloadStageDiag: clip length={reload.length:F2}s " +
                          $"frameRate={reload.frameRate} legacy={reload.legacy} " +
                          $"empty={reload.empty}");
                foreach (var (name, t) in Stages)
                {
                    reload.SampleAnimation(go, Mathf.Min(t, reload.length));
                    render(go, Path.Combine(outDir, $"reload_{name}_t{t:00.0}.png"));
                }
                // context repro: exactly what the gate render asks of the
                // skirmisher at t=10 (Reload @ 1.67 s)
                var pose = GateP6Choreography.Resolve(84, 10f);
                clips[pose.clip].SampleAnimation(
                    go, Mathf.Min(pose.clipTime, clips[pose.clip].length));
                render(go, Path.Combine(outDir, "context_skirmisher_t10.png"));
            }, "reload-stages");
        }

        // Text-only diagnostic: are the imported clips' curves actually
        // bound and moving? Prints binding counts and sampled transform
        // positions for a handful of clips.
        public static void DumpClips()
        {
            int exitCode = 0;
            try
            {
                var go = GateP6Render.SpawnVariant("csa_a");
                var clips = GateP6Render.LoadClips("csa_a");
                Debug.Log("DumpClips: available: " + string.Join(", ",
                    System.Linq.Enumerable.Select(clips, kv =>
                        $"{kv.Key}({kv.Value.name}, {kv.Value.length:F2}s)")));
                var handR = FindDeep(go.transform, "hand_r");
                var musket = FindDeep(go.transform, "prop_musket");
                var root = FindDeep(go.transform, "Root");
                foreach (string key in new[]
                         { "March_ShoulderArms", "Reload_Musket",
                           "Stand_Ready", "Fall_Shot_Front_Back" })
                {
                    var clip = clips[key];
                    var bindings = AnimationUtility.GetCurveBindings(clip);
                    int moving = 0;
                    foreach (var b in bindings)
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, b);
                        if (curve == null) continue;
                        float lo = float.MaxValue, hi = float.MinValue;
                        foreach (var k in curve.keys)
                        { lo = Mathf.Min(lo, k.value); hi = Mathf.Max(hi, k.value); }
                        if (hi - lo > 1e-4f) moving++;
                    }
                    string samplePath = bindings.Length > 0 ? bindings[0].path : "-";
                    Debug.Log($"DumpClips: {key}: {bindings.Length} bindings, " +
                              $"{moving} moving, first path '{samplePath}'");
                    foreach (float t in new[] { 0f, 0.5f, 0.9f })
                    {
                        clip.SampleAnimation(go, clip.length * t);
                        Debug.Log($"DumpClips: {key} @{t:F1}: " +
                                  $"hand_r={Fmt(handR)} musket={Fmt(musket)} root={Fmt(root)}");
                    }
                }
                // head-vs-pelvis check against the Blender ground truth
                var head = FindDeep(go.transform, "head");
                var pelvis = FindDeep(go.transform, "pelvis");
                var reload2 = clips["Reload_Musket"];
                foreach (float tt in new[] { 1.67f, 10.6f })
                {
                    reload2.SampleAnimation(go, tt);
                    Debug.Log($"DumpClips: reload@{tt:F2} head={Fmt(head)} " +
                              $"pelvis={Fmt(pelvis)} hand_r={Fmt(handR)}");
                }
                // hierarchy root children for path comparison
                Debug.Log("DumpClips: instance children: " + string.Join(", ",
                    System.Linq.Enumerable.Select(
                        System.Linq.Enumerable.Range(0, go.transform.childCount),
                        i => go.transform.GetChild(i).name)));
            }
            catch (Exception e)
            {
                Debug.LogError($"DumpClips failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static string Fmt(Transform t) =>
            t == null ? "MISSING"
            : $"({t.position.x:F3},{t.position.y:F3},{t.position.z:F3})";

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var r = FindDeep(t.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }

        public static void RenderClipSweep()
        {
            RunIsolated((render, outDir) =>
            {
                foreach (string variant in new[] { "csa_a", "union_a" })
                {
                    var go = GateP6Render.SpawnVariant(variant);
                    var clips = GateP6Render.LoadClips(variant);
                    foreach (var kv in clips)
                    {
                        var clip = kv.Value;
                        float[] phases = { 0f, 0.33f, 0.66f, 0.999f };
                        for (int i = 0; i < phases.Length; i++)
                        {
                            float t = clip.length * phases[i];
                            clip.SampleAnimation(go, t);
                            render(go, Path.Combine(
                                outDir, $"sweep_{variant}_{kv.Key}_{i}_t{t:00.00}.png"));
                        }
                    }
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }, "clip-sweep");
        }

        static void RunIsolated(
            Action<Action<GameObject, string>, string> body, string subdir)
        {
            int exitCode = 0;
            try
            {
                Inner(body, subdir);
            }
            catch (Exception e)
            {
                Debug.LogError($"ReloadStageDiag failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void Inner(Action<Action<GameObject, string>, string> body, string subdir)
        {
            string outDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, $"../../docs/benchmarks/captures/p6-gate/{subdir}"));
            Directory.CreateDirectory(outDir);

            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(
                HdrpMigration.OfflineAssetPath);
            RenderPipelineAsset prevDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset prevQuality = QualitySettings.renderPipeline;
            object prevGlobal = GateP6Render.CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            GateP6Render.BindHdrpGlobalSettings();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var stage = AngleBakeoffStage.Stage(BakeoffPipeline.Hdrp);
                UnityEngine.Object.DestroyImmediate(stage.figures.gameObject);
                Terrain terrain = stage.terrain;
                var spot = GateP6Choreography.SkirmisherPos;
                float groundY = terrain.transform.position.y +
                    terrain.SampleHeight(new Vector3(spot.x, 0f, spot.y));
                var origin = new Vector3(spot.x, groundY, spot.y);

                Camera cam = stage.camera;
                cam.fieldOfView = 45f;
                cam.nearClipPlane = 0.05f;

                var rt = new RenderTexture(W, H, 32);
                var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
                bool warmed = false;

                void Render(GameObject go, string path)
                {
                    go.transform.position = origin;
                    // face the figure toward the camera (kit front is +Z
                    // at identity; camera sits at +X/-Z of the figure)
                    go.transform.rotation = Quaternion.Euler(0f, 130f, 0f);
                    // 3/4 front view from ~3.4 m, chest height
                    cam.transform.position = origin + new Vector3(2.6f, 1.35f, -2.1f);
                    cam.transform.LookAt(origin + new Vector3(0f, 0.95f, 0f));
                    if (!warmed)
                    {
                        for (int i = 0; i < WarmupFrames; i++)
                            GateP6Render.RenderOnce(cam, rt, null);
                        if (!(RenderPipelineManager.currentPipeline is HDRenderPipeline))
                            throw new InvalidOperationException(
                                "HDRP did not construct; run with -buildTarget OSXUniversal");
                        warmed = true;
                    }
                    var handR = FindDeep(go.transform, "hand_r");
                    var musketB = FindDeep(go.transform, "prop_musket");
                    var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
                    GateP6Render.RenderOnce(cam, rt, tex);
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    Debug.Log($"ReloadStageDiag: wrote {Path.GetFileName(path)} " +
                              $"hand_r={Fmt(handR)} musket={Fmt(musketB)} " +
                              $"smr={smr?.name} bounds={smr?.bounds.center}");
                }

                body(Render, outDir);
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
