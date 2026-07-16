using System;
using System.Collections.Generic;
using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Fight-prone vocabulary gate evidence (P6-style pattern): renders the
    // staged demo bundle (reconstruction/scripts/stage_fight_prone.py ->
    // docs/benchmarks/captures/fight-prone/fight-prone-demo.bundle.json)
    // on the Oak Ridge crop — a line standing under fire, going prone,
    // fighting prone (fire + roll-to-load + idle), taking prone
    // casualties, and rising — from a fixed eye-level flank camera.
    //
    //   RenderSequence     30 s, 30 fps, 2560x1440 + scrub probes + report
    //   RenderStills       the arc in six stills (committed evidence)
    //   RenderIversonStill Iverson window stills at the prone segment
    //                      (t=6600), tagged via FIGHT_PRONE_TAG env
    //                      (before/after bundle comparison)
    //
    // Usage:
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.FightProneGateRender.RenderSequence
    //     -logFile fp-seq.log
    public static class FightProneGateRender
    {
        const int Width = 2560, Height = 1440;
        const int Fps = 30;
        const int WarmupFrames = 3;

        // 30 s window over the demo slice (hold 0-6, fight_prone 6-24,
        // hold/rise 24-40): the drop, the prone fight, and the rise all
        // land inside t=2..32.
        const float SeqT0 = 2f, SeqT1 = 32f;

        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;

        static string OutDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../docs/benchmarks/captures/fight-prone"));

        static string OakridgeCropDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../data/heightmap_oakridge"));

        static string DemoBundlePath => Path.Combine(
            OutDir, "fight-prone-demo.bundle.json");

        static string IversonBundlePath => Path.Combine(
            Application.dataPath, "Battle", "Iverson", "iverson.bundle.json");

        public static void RenderSequence() => Run(SequenceInner);
        public static void RenderStills() => Run(StillsInner);
        public static void RenderIversonStill() => Run(IversonStillInner);

        static void Run(Action inner)
        {
            int exitCode = 0;
            try
            {
                inner();
            }
            catch (Exception e)
            {
                Debug.LogError($"FightProneGateRender failed: {e}");
                exitCode = 1;
            }
            finally
            {
                AngleEnvironmentStage.CropDirOverride = null;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static (AngleActionScene scene, RenderTexture rt, Texture2D tex)
            Boot(string bundlePath,
                 out RenderPipelineAsset prevDefault,
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

            AngleEnvironmentStage.CropDirOverride = OakridgeCropDir;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var scene = AngleActionScene.StageAll(bundlePath);
            Debug.Log($"FightProneGateRender: staged {scene.ctx.TotalSlots} slots, " +
                $"bundle {scene.ctx.bundle.checksum.Substring(0, 12)}");
            var rt = new RenderTexture(Width, Height, 32);
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            scene.camera.nearClipPlane = 0.05f;
            return (scene, rt, tex);
        }

        // Fixed eye-level camera off the demo line's left flank, looking
        // down the line with the opposing fire beyond — the P6 gate
        // viewing grammar.
        static void PlaceCamera(AngleActionScene scene)
        {
            var ur = scene.ctx.Unit("demo-csa-line");
            Vector2 center = ur.unit.PositionAt(SeqT0);
            float facing = ur.unit.FacingAt(SeqT0) * Mathf.Deg2Rad;
            var fwd = new Vector2(Mathf.Sin(facing), Mathf.Cos(facing));
            var left = new Vector2(-fwd.y, fwd.x);   // left flank of the facing
            float halfFrontage =
                FormationRoster.Frontage(ur.slotCount) / 2f;
            Vector2 camPos = center + left * (halfFrontage + 8f) - fwd * 3f;
            Vector2 lookAt = center + fwd * 10f;

            var cam = scene.camera;
            float cropX0 = scene.cropX0, cropZ0 = scene.cropZ0;
            var terrain = scene.terrain;
            float groundY = terrain.transform.position.y + terrain.SampleHeight(
                new Vector3(camPos.x - cropX0, 0f, camPos.y - cropZ0));
            var camWorld = new Vector3(
                camPos.x - cropX0, groundY + 1.66f, camPos.y - cropZ0);
            float lookY = terrain.transform.position.y + terrain.SampleHeight(
                new Vector3(lookAt.x - cropX0, 0f, lookAt.y - cropZ0));
            var lookWorld = new Vector3(
                lookAt.x - cropX0, lookY + 1.1f, lookAt.y - cropZ0);
            cam.transform.position = camWorld;
            cam.transform.rotation = Quaternion.LookRotation(
                lookWorld - camWorld, Vector3.up);
            cam.fieldOfView = 55f;
        }

        static void SequenceInner()
        {
            var (scene, rt, tex) = Boot(DemoBundlePath,
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                PlaceCamera(scene);
                int frames = (int)((SeqT1 - SeqT0) * Fps);
                string seqDir = Path.Combine(OutDir, "seq");
                Directory.CreateDirectory(seqDir);
                int[] probeFrames = { 450, 150 };
                var digests = new Dictionary<int, string>();
                var logicalEqual = new List<bool>();
                var withinTol = new List<bool>();
                var probeStats = new List<string>();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                bool warmed = false;
                for (int f = 0; f < frames; f++)
                {
                    float t = SeqT0 + f / (float)Fps;
                    scene.Pose(t);
                    if (!warmed)
                    {
                        for (int i = 0; i < WarmupFrames; i++)
                            GateP6Render.RenderOnce(scene.camera, rt, null);
                        if (!(RenderPipelineManager.currentPipeline
                            is HDRenderPipeline))
                            throw new InvalidOperationException(
                                "HDRP did not construct; run with " +
                                "-buildTarget OSXUniversal");
                        warmed = true;
                    }
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    File.WriteAllBytes(
                        Path.Combine(seqDir, $"frame_{f:D4}.png"),
                        tex.EncodeToPNG());
                    if (Array.IndexOf(probeFrames, f) >= 0)
                        digests[f] = scene.LogicalStateDigest(t);
                    if (f % 150 == 0)
                        Debug.Log($"FightProneGateRender: {f}/{frames} " +
                            $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                }
                float secondsPerFrame = (float)(sw.Elapsed.TotalSeconds / frames);

                // out-of-order scrub probes (the P6/P8 determinism grammar)
                foreach (int f in probeFrames)
                {
                    float t = SeqT0 + f / (float)Fps;
                    float away = SeqT0 + ((f + 450) % frames) / (float)Fps;
                    scene.Pose(away);
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    scene.Pose(t);
                    bool logical = scene.LogicalStateDigest(t) == digests[f];
                    logicalEqual.Add(logical);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    byte[] again = tex.EncodeToPNG();
                    byte[] orig = File.ReadAllBytes(
                        Path.Combine(OutDir, "seq", $"frame_{f:D4}.png"));
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
                    probeStats.Add($"frame {f}: logicalBitwiseEqual={logical} " +
                        $"differingPixels={pct:F2}% maxChannelDelta={maxd}");
                    Debug.Log($"FightProneGateRender: probe {probeStats[^1]}");
                }

                string report =
                    "{\n" +
                    $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                    $"  \"bundleChecksum\": \"{scene.ctx.bundle.checksum}\",\n" +
                    $"  \"battleSeed\": \"{scene.ctx.seed}\",\n" +
                    $"  \"totalSlots\": {scene.ctx.TotalSlots},\n" +
                    $"  \"sequence\": {{\"t0\": {SeqT0}, \"t1\": {SeqT1}, " +
                    $"\"fps\": {Fps}, \"frames\": {frames}}},\n" +
                    $"  \"secondsPerFrame\": {secondsPerFrame:F2},\n" +
                    $"  \"probeFrames\": [{string.Join(",", probeFrames)}],\n" +
                    "  \"probesLogicalBitwiseEqual\": " +
                    $"[{string.Join(",", logicalEqual.ConvertAll(b => b ? "true" : "false"))}],\n" +
                    "  \"probesWithinPixelTolerance\": " +
                    $"[{string.Join(",", withinTol.ConvertAll(b => b ? "true" : "false"))}],\n" +
                    $"  \"probeStats\": [{string.Join(", ", probeStats.ConvertAll(s => $"\"{s}\""))}],\n" +
                    $"  \"pixelToleranceMaxChannelDelta\": {TolMaxDelta},\n" +
                    $"  \"pixelToleranceDifferingPct\": {TolDiffPct}\n" +
                    "}\n";
                File.WriteAllText(
                    Path.Combine(OutDir, "fight-prone-gate-report.json"), report);

                if (logicalEqual.Contains(false))
                    throw new InvalidOperationException(
                        "GATE FAIL: logical state not bitwise-identical under scrub");
                if (withinTol.Contains(false))
                    throw new InvalidOperationException(
                        "GATE FAIL: scrub probes exceeded the documented GPU tolerance");
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        static void StillsInner()
        {
            var (scene, rt, tex) = Boot(DemoBundlePath,
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                PlaceCamera(scene);
                var shots = new (string name, float t)[]
                {
                    ("fp-still-04-standing-under-fire", 4f),
                    ("fp-still-08-going-prone", 8f),
                    ("fp-still-13-prone-fire", 13f),
                    ("fp-still-19-roll-to-load", 19f),
                    ("fp-still-25-rising", 25.2f),
                    ("fp-still-30-recovered", 30f),
                };
                bool warmed = false;
                foreach (var shot in shots)
                {
                    scene.Pose(shot.t);
                    if (!warmed)
                    {
                        for (int i = 0; i < WarmupFrames; i++)
                            GateP6Render.RenderOnce(scene.camera, rt, null);
                        warmed = true;
                    }
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    string p = Path.Combine(OutDir, $"{shot.name}.png");
                    File.WriteAllBytes(p, tex.EncodeToPNG());
                    Debug.Log($"FightProneGateRender: wrote {p}");
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // Iverson window before/after stills at the prone segment: the
        // observer's own first-person view at t=6600 (the fight at full
        // depth) — run once on the pre-slice bundle (tag "before") and
        // once on the regenerated bundle (tag "after").
        static void IversonStillInner()
        {
            string tag = Environment.GetEnvironmentVariable("FIGHT_PRONE_TAG");
            if (string.IsNullOrEmpty(tag)) tag = "after";

            string path = Path.Combine(
                Application.streamingAssetsPath, "SoldierView/viewpoints.json");
            var set = ViewpointSet.FromJson(File.ReadAllText(path));
            ViewpointDefinition vp = null;
            foreach (var v in set.viewpoints)
                if (v.id == "iverson-forney-field") vp = v;
            if (vp == null)
                throw new InvalidOperationException("no iverson viewpoint");

            var (scene, rt, tex) = Boot(IversonBundlePath,
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                scene.lensGuardRadiusM = LensGuard.DefaultRadiusM;
                var obs = scene.ctx.Unit(vp.unitId);
                var shots = new (string name, float t, bool thirdPerson)[]
                {
                    ($"iv-6600-prone-fight-fp-{tag}", 6600f, false),
                    ($"iv-6620-prone-fight-fp-{tag}", 6620f, false),
                    ($"iv-6600-prone-fight-c3p-{tag}", 6600f, true),
                };
                bool warmed = false;
                foreach (var shot in shots)
                {
                    var settings = GateP9Render.Settings(vp, shot.thirdPerson);
                    scene.hiddenUnitIndex = shot.thirdPerson ? -1 : obs.unitIndex;
                    scene.hiddenSlot = shot.thirdPerson ? -1 : vp.slotId;
                    GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                        scene.ctx, settings, shot.t));
                    scene.Pose(shot.t);
                    if (!warmed)
                    {
                        for (int i = 0; i < WarmupFrames; i++)
                            GateP6Render.RenderOnce(scene.camera, rt, null);
                        warmed = true;
                    }
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    string p = Path.Combine(OutDir, $"{shot.name}.png");
                    File.WriteAllBytes(p, tex.EncodeToPNG());
                    Debug.Log($"FightProneGateRender: wrote {p}");
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }
    }
}
