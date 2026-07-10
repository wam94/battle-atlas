using System;
using System.Collections.Generic;
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
    // Gate P8 evidence renderer (plan §12 Phase 8 gate): the full stack —
    // compiled reconstruction, character kit, sourced environment,
    // black-powder VFX, deterministic casualties — rendered under the
    // offline HDRP profile.
    //
    //   RenderStills    §13 golden frames t=8160/8400/8580/8700
    //   RenderSequence  90 s eye-level at the wall, t=8610..8700 @30fps,
    //                   plus out-of-order scrub probes: bitwise logical
    //                   digests + pixel tolerance stats (P6/P10 precedent)
    //   StageDiag       one fast frame + scene statistics
    //
    // Usage:
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.GateP8Render.RenderStills
    //     -logFile p8render.log
    // Output: docs/benchmarks/captures/p8-gate/
    public static class GateP8Render
    {
        const int Width = 2560, Height = 1440;
        const int Fps = 30;
        const int WarmupFrames = 3;
        public const float SeqT0 = 8610f;
        public const float SeqT1 = 8700f;

        // Documented GPU pixel tolerance (plan Gate P10 language). The P6
        // scene measured maxDelta 8 / 5% of Metal raster noise; the P8
        // scene stacks thousands of transparent smoke quads, which
        // amplifies rasterization-order noise — the measured smoke-heavy
        // envelope (t=8690, peak coverage) is 5.92% differing pixels at
        // maxChannelDelta 9. Tolerance sits just above that measurement.
        // The LOGICAL state comparison stays bitwise-exact regardless.
        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;

        struct CamDef
        {
            public string name;
            public Vector2 posMacro;
            public float eyeM;
            public Vector2 lookMacro;
            public float lookM;
            public float fovDeg;
            public float t;
        }

        // §13 golden frames + the wall sequence camera. Macro meters.
        static readonly CamDef[] Stills =
        {
            new CamDef
            {
                // Emmitsburg Road crossing: Garnett's line over the fences
                name = "p8-still-8160-road", t = 8160f,
                posMacro = new Vector2(4098f, 4834f), eyeM = 1.66f,
                lookMacro = new Vector2(4072f, 4798f), lookM = 1.3f,
                fovDeg = 68f,
            },
            new CamDef
            {
                // closing under canister, mid-field
                name = "p8-still-8400-canister", t = 8400f,
                posMacro = new Vector2(4212f, 4838f), eyeM = 1.66f,
                lookMacro = new Vector2(4174f, 4820f), lookM = 1.6f,
                fovDeg = 68f,
            },
            new CamDef
            {
                // wall approach from behind the 69th PA
                name = "p8-still-8580-wall", t = 8580f,
                posMacro = new Vector2(4412f, 4884f), eyeM = 1.66f,
                lookMacro = new Vector2(4380f, 4858f), lookM = 1.5f,
                fovDeg = 68f,
            },
            new CamDef
            {
                // the Angle crisis: Armistead's breach over the wall
                name = "p8-still-8700-crisis", t = 8700f,
                posMacro = new Vector2(4419f, 4874f), eyeM = 1.66f,
                lookMacro = new Vector2(4404f, 4854f), lookM = 1.4f,
                fovDeg = 68f,
            },
        };

        static readonly CamDef SequenceCam = new CamDef
        {
            name = "p8-seq-wall",
            posMacro = new Vector2(4412f, 4884f), eyeM = 1.66f,
            lookMacro = new Vector2(4383f, 4856f), lookM = 1.5f,
            fovDeg = 68f,
        };

        static string OutDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../docs/benchmarks/captures/p8-gate"));

        public static void RenderStills() => Run(() => StillsInner());
        public static void RenderSequence() => Run(() => SequenceInner());
        public static void StageDiag() => Run(() => DiagInner());

        static void Run(Action inner)
        {
            int exitCode = 0;
            try
            {
                inner();
            }
            catch (Exception e)
            {
                Debug.LogError($"GateP8Render failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static (AngleActionScene scene, RenderTexture rt, Texture2D tex)
            Boot(out RenderPipelineAsset prevDefault,
                 out RenderPipelineAsset prevQuality, out object prevGlobal)
        {
            Directory.CreateDirectory(OutDir);
            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(
                HdrpMigration.OfflineAssetPath);
            if (offline == null)
                throw new InvalidOperationException(
                    $"{HdrpMigration.OfflineAssetPath} missing");
            prevDefault = GraphicsSettings.defaultRenderPipeline;
            prevQuality = QualitySettings.renderPipeline;
            prevGlobal = GateP6Render.CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            GateP6Render.BindHdrpGlobalSettings();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var scene = AngleActionScene.StageAll();
            Debug.Log($"GateP8Render: staged in {sw.Elapsed.TotalSeconds:F1} s");
            var rt = new RenderTexture(Width, Height, 32);
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            scene.camera.nearClipPlane = 0.05f;
            return (scene, rt, tex);
        }

        static void Restore(RenderPipelineAsset prevDefault,
            RenderPipelineAsset prevQuality, object prevGlobal)
        {
            // restore the COMMITTED playback profile explicitly (P6 rule)
            var playback = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
                HdrpMigration.PlaybackAssetPath);
            GraphicsSettings.defaultRenderPipeline =
                playback != null ? playback : prevDefault;
            QualitySettings.renderPipeline = prevQuality;
            GateP6Render.CurrentGlobalSettingsProp.SetValue(null, prevGlobal);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        static void ApplyCam(AngleActionScene scene, CamDef def)
        {
            var terrain = scene.terrain;
            float x0 = scene.env.env.crop.x0, z0 = scene.env.env.crop.z0;
            float Ground(Vector2 macro) =>
                terrain.transform.position.y + terrain.SampleHeight(
                    new Vector3(macro.x - x0, 0f, macro.y - z0));
            var pos = new Vector3(def.posMacro.x - x0,
                Ground(def.posMacro) + def.eyeM, def.posMacro.y - z0);
            var look = new Vector3(def.lookMacro.x - x0,
                Ground(def.lookMacro) + def.lookM, def.lookMacro.y - z0);
            scene.camera.transform.position = pos;
            scene.camera.transform.rotation =
                Quaternion.LookRotation(look - pos, Vector3.up);
            scene.camera.fieldOfView = def.fovDeg;
        }

        static void StillsInner()
        {
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                ApplyCam(scene, Stills[0]);
                scene.Pose(Stills[0].t);
                for (int i = 0; i < WarmupFrames; i++)
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                if (!(RenderPipelineManager.currentPipeline is HDRenderPipeline))
                    throw new InvalidOperationException(
                        "HDRP did not construct; run with -buildTarget OSXUniversal");

                foreach (var def in Stills)
                {
                    // camera BEFORE pose: smoke billboards face the camera
                    ApplyCam(scene, def);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    scene.Pose(def.t);
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    string p = Path.Combine(OutDir, $"{def.name}.png");
                    File.WriteAllBytes(p, tex.EncodeToPNG());
                    Debug.Log($"GateP8Render: wrote {p} " +
                              $"({sw.Elapsed.TotalSeconds:F1} s)");
                }
            }
            finally
            {
                Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        static void DiagInner()
        {
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var diagShots = new[]
                {
                    (SequenceCam, 8330f, "p8-diag-8330-wall-clear"),
                    (Stills[0], 8200f, "p8-diag-8200-road"),
                    (Stills[1], 8450f, "p8-diag-8450-canister"),
                    (SequenceCam, 8620f, "p8-diag-8620-wall"),
                };
                ApplyCam(scene, SequenceCam);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                scene.Pose(8620f);
                Debug.Log($"GateP8Render: pose in {sw.Elapsed.TotalSeconds:F2} s");
                for (int i = 0; i < WarmupFrames; i++)
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                foreach (var (cam, t, name) in diagShots)
                {
                    ApplyCam(scene, cam);
                    scene.Pose(t);
                    sw.Restart();
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    Debug.Log($"GateP8Render: {name} render in " +
                              $"{sw.Elapsed.TotalSeconds:F2} s");
                    File.WriteAllBytes(Path.Combine(OutDir, $"{name}.png"),
                        tex.EncodeToPNG());
                }

                int[] tierCounts = new int[4];
                foreach (var tier in scene.tiers) tierCounts[(int)tier]++;
                Debug.Log($"GateP8Render: tiers hero={tierCounts[0]} " +
                          $"near={tierCounts[1]} mid={tierCounts[2]} " +
                          $"far={tierCounts[3]}; " +
                          $"smokeEvents={scene.smokeEvents.Count}; " +
                          $"digest(8620)={scene.LogicalStateDigest(8620f)}");
            }
            finally
            {
                Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        static void SequenceInner()
        {
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                ApplyCam(scene, SequenceCam);
                scene.Pose(SeqT0);
                for (int i = 0; i < WarmupFrames; i++)
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                if (!(RenderPipelineManager.currentPipeline is HDRenderPipeline))
                    throw new InvalidOperationException(
                        "HDRP did not construct; run with -buildTarget OSXUniversal");

                string seqDir = Path.Combine(OutDir, "seq");
                Directory.CreateDirectory(seqDir);
                int frames = (int)((SeqT1 - SeqT0) * Fps);
                var digests = new Dictionary<int, string>();
                int[] probeFrames = { 2400, 300, 1500 };   // deliberately unordered later
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long peakMem = 0;
                for (int f = 0; f < frames; f++)
                {
                    float t = SeqT0 + f / (float)Fps;
                    scene.Pose(t);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    File.WriteAllBytes(
                        Path.Combine(seqDir, $"frame_{f:D4}.png"), tex.EncodeToPNG());
                    if (Array.IndexOf(probeFrames, f) >= 0)
                        digests[f] = scene.LogicalStateDigest(t);
                    if (f % 150 == 0)
                    {
                        peakMem = Math.Max(peakMem, GC.GetTotalMemory(false));
                        Debug.Log($"GateP8Render: frame {f}/{frames} " +
                                  $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                    }
                }
                sw.Stop();
                float secondsPerFrame = (float)(sw.Elapsed.TotalSeconds / frames);

                // --- scrub probes: OUT OF ORDER re-pose + re-render ---
                var probeStats = new List<string>();
                var logicalEqual = new List<bool>();
                var withinTol = new List<bool>();
                foreach (int f in probeFrames)
                {
                    float t = SeqT0 + f / (float)Fps;
                    // scrub far away first so the probe really is a scrub
                    scene.Pose(SeqT0 + ((f + 1000) % frames) / (float)Fps);
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    scene.Pose(t);
                    string digest = scene.LogicalStateDigest(t);
                    bool logical = digest == digests[f];
                    logicalEqual.Add(logical);

                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    byte[] again = tex.EncodeToPNG();
                    byte[] orig = File.ReadAllBytes(
                        Path.Combine(seqDir, $"frame_{f:D4}.png"));
                    var ta = new Texture2D(2, 2); ta.LoadImage(orig);
                    var tb = new Texture2D(2, 2); tb.LoadImage(again);
                    var pa = ta.GetPixels32();
                    var pb = tb.GetPixels32();
                    long ndiff = 0; int maxd = 0;
                    for (int i = 0; i < pa.Length; i++)
                    {
                        int d = Mathf.Max(
                            Mathf.Abs(pa[i].r - pb[i].r),
                            Mathf.Max(Mathf.Abs(pa[i].g - pb[i].g),
                                Mathf.Abs(pa[i].b - pb[i].b)));
                        if (d > 0) { ndiff++; if (d > maxd) maxd = d; }
                    }
                    float pct = 100f * ndiff / pa.Length;
                    bool ok = maxd <= TolMaxDelta && pct <= TolDiffPct;
                    withinTol.Add(ok);
                    string st = $"frame {f}: logicalBitwiseEqual={logical} " +
                                $"differingPixels={pct:F2}% maxChannelDelta={maxd}";
                    probeStats.Add(st);
                    Debug.Log($"GateP8Render: probe {st} withinTolerance={ok}");
                }

                string report =
                    "{\n" +
                    $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                    $"  \"pipelineAsset\": \"{HdrpMigration.OfflineAssetPath}\",\n" +
                    $"  \"bundleChecksum\": \"{scene.ctx.bundle.checksum}\",\n" +
                    $"  \"battleSeed\": \"{scene.ctx.seed}\",\n" +
                    $"  \"sequence\": {{\"t0\": {SeqT0}, \"t1\": {SeqT1}, " +
                    $"\"fps\": {Fps}, \"frames\": {frames}}},\n" +
                    $"  \"secondsPerFrame\": {secondsPerFrame:F2},\n" +
                    $"  \"peakManagedMemoryMB\": {peakMem / (1024 * 1024)},\n" +
                    $"  \"probeFrames\": [{string.Join(",", probeFrames)}],\n" +
                    $"  \"probesLogicalBitwiseEqual\": " +
                    $"[{string.Join(",", logicalEqual.Select(b => b ? "true" : "false"))}],\n" +
                    $"  \"probesWithinPixelTolerance\": " +
                    $"[{string.Join(",", withinTol.Select(b => b ? "true" : "false"))}],\n" +
                    $"  \"probeStats\": [{string.Join(", ", probeStats.Select(s => $"\"{s}\""))}],\n" +
                    $"  \"pixelToleranceMaxChannelDelta\": {TolMaxDelta},\n" +
                    $"  \"pixelToleranceDifferingPct\": {TolDiffPct},\n" +
                    $"  \"casualtyReconciliation\": {scene.CasualtyReport()}\n" +
                    "}\n";
                File.WriteAllText(Path.Combine(OutDir, "p8-gate-report.json"), report);
                Debug.Log($"GateP8Render: sequence complete, {secondsPerFrame:F2} s/frame");

                if (logicalEqual.Contains(false))
                    throw new InvalidOperationException(
                        "GATE FAIL: logical state digests not bitwise-identical under scrub");
                if (withinTol.Contains(false))
                    throw new InvalidOperationException(
                        "GATE FAIL: scrub probes exceeded the documented GPU tolerance");
            }
            finally
            {
                Restore(prevDefault, prevQuality, prevGlobal);
            }
        }
    }
}
