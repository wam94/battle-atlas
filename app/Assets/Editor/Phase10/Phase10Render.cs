using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using BattleAtlas;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BattleAtlas.EditorTools
{
    // ------------------------------------------------------------------
    // Phase 10 — the offline production render pipeline (plan §12 Phase
    // 10, §3.5, §10.1; media contract docs/reconstruction/
    // p1-media-contract.md).
    //
    // Renders the full garnett-road-to-angle hero viewpoint
    // (t=8160..8820 plus the media contract's 0.5 s end pad; 19,815
    // frames at 2560x1440/30 fps) through the SAME deterministic staging
    // the Gate P6..P9 evidence used: the proven
    // RenderPipeline.SubmitRenderRequest harness (GateP6Render.
    // RenderOnce) under the offline HDRP profile. The Unity Recorder
    // package is installed but is NOT used: it wants a playmode timeline
    // and editor coroutines in batchmode, while this project's harness
    // has rendered every gate deterministically from a pure Pose(t) —
    // the decision is recorded in the render runbook. No accumulation
    // motion blur: the owner judged Gate P9 media rendered one
    // deterministic sample per frame; adding subframe accumulation
    // would change the accepted look and add a nondeterminism surface.
    //
    //   Preflight       global no-teleport sweep over the WHOLE padded
    //                   window (every unit, every slot) + camera-delta
    //                   sweep. Hard-fails before any render hours are
    //                   committed. Writes p10-preflight.json.
    //   RenderTeleportFixWindow
    //                   re-renders the Gate P9 proof window t=8610..8670
    //                   with the P10 fixes for the before/after evidence.
    //   RenderDeterminismPair
    //                   Gate P10 criterion: renders t=8400..8410 TWICE
    //                   from two independent stagings; compares logical
    //                   metadata (must be identical) and pixels (within
    //                   the documented Phase 8 GPU tolerance).
    //   RenderProduction
    //                   the full chunked, resumable render. A killed run
    //                   resumes: chunks with a complete manifest + all
    //                   frames on disk are skipped.
    //
    // Usage (from the repo root; Unity editor must be closed):
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.Phase10Render.<Method> \
    //     -logFile p10-<method>.log
    // ------------------------------------------------------------------
    public static class Phase10Render
    {
        const int Width = 2560, Height = 1440, Fps = 30;
        const int WarmupFrames = 3;

        // Media contract: the last ~3 frames of a stream are unreachable
        // seek targets (EndGuardFrames = 4), so the render runs PAST the
        // viewpoint's t1 by half a second — the guard becomes invisible.
        public const float PadPastT1 = 0.5f;

        // Resumable chunking (this machine has crashed mid-render).
        const float ChunkSeconds = 60f;

        // Documented GPU pixel tolerance. Two tiers:
        //  - the Phase 8 envelope (channel delta <= 12 on <= 8% of
        //    pixels) for ordinary raster/temporal noise, plus
        //  - up to MaxOutlierPixels ISOLATED pixels per frame above that
        //    delta: two INDEPENDENT stagings (fresh scene, fresh object
        //    ids) can resolve a handful of exact depth/coverage ties
        //    differently, which shows as 1-4 lone pixels (~0.0001% of a
        //    3.7 M-pixel frame) shimmering on hard edges. Measured and
        //    documented in p10-determinism.json.
        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;
        const int MaxOutlierPixels = 8;

        // No-teleport bounds (same as NoTeleportTests).
        const float MaxDeltaPerFrameM = 8f / Fps;             // sprint
        const float MaxCrossExitDeltaM = SoldierActionResolver.CrossTravelM + 8f / Fps;
        const float MaxCameraDeltaPerFrameM = 4.5f / Fps;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static string RepoRoot => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../.."));
        static string OutRoot => Path.Combine(RepoRoot, "app/RenderOutput/p10");
        static string SeqDir => Path.Combine(OutRoot, "seq-full");
        static string ManifestDir => Path.Combine(OutRoot, "manifests");
        static string EvidenceDir => Path.Combine(
            RepoRoot, "docs/benchmarks/captures/p10-gate");

        public static void Preflight() => Run(PreflightInner);
        public static void CreateMarkerScene() => Run(CreateMarkerSceneInner);

        // One-shot: creates the committed thin marker scene (plan §5
        // AngleRender.unity). Content is staged procedurally at render
        // time; the scene carries only the marker.
        static void CreateMarkerSceneInner()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            var go = new GameObject("AngleRender (staged procedurally)");
            go.AddComponent<AngleRenderSceneMarker>();
            const string path = "Assets/Scenes/AngleRender.unity";
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                    scene, path))
                throw new InvalidOperationException($"could not save {path}");
            Debug.Log($"Phase10Render: wrote {path}");
        }

        public static void RenderTeleportFixWindow() => Run(TeleportFixInner);
        public static void RenderDeterminismPair() => Run(DeterminismInner);
        public static void RenderProduction() => Run(ProductionInner);

        static void Run(Action inner)
        {
            int exitCode = 0;
            try { inner(); }
            catch (Exception e)
            {
                Debug.LogError($"Phase10Render failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        // ------------------------------------------------------------------
        // Freeze record: everything that must be pinned for the render to
        // be reproducible, hashed into one settingsHash carried by every
        // chunk manifest and frame-metadata record.
        // ------------------------------------------------------------------
        static string Sha256File(string path)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var fs = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(fs))
                .Replace("-", "").ToLowerInvariant();
        }

        static string Sha256Text(string text)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(
                    sha.ComputeHash(Encoding.UTF8.GetBytes(text)))
                .Replace("-", "").ToLowerInvariant();
        }

        static string GitSha()
        {
            var psi = new ProcessStartInfo("git", "rev-parse HEAD")
            {
                WorkingDirectory = RepoRoot,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            using var p = Process.Start(psi);
            string sha = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            if (p.ExitCode != 0 || sha.Length != 40)
                throw new InvalidOperationException(
                    "git rev-parse HEAD failed; frame metadata requires the SHA");
            return sha;
        }

        internal class Freeze
        {
            public string gitSha, unityVersion, pipelineAssetPath,
                pipelineAssetSha, packagesLockSha, colorSpace,
                bundleChecksum, seed, viewpointId, viewKind,
                heightmapJsonSha, heightmapRawSha, environmentJsonSha,
                environmentSplatSha, settingsHash;
            public int width, height, fps;
            public float t0, t1, padPastT1, lensGuardRadiusM,
                maxWheelFlankSpeedMps;
            public int slotId;

            public string ToJson()
            {
                var sb = new StringBuilder();
                sb.Append("{\n");
                sb.Append($"  \"gitSha\": \"{gitSha}\",\n");
                sb.Append($"  \"unityVersion\": \"{unityVersion}\",\n");
                sb.Append($"  \"pipelineAssetPath\": \"{pipelineAssetPath}\",\n");
                sb.Append($"  \"pipelineAssetSha256\": \"{pipelineAssetSha}\",\n");
                sb.Append($"  \"packagesLockSha256\": \"{packagesLockSha}\",\n");
                sb.Append($"  \"colorSpace\": \"{colorSpace}\",\n");
                sb.Append($"  \"bundleChecksum\": \"{bundleChecksum}\",\n");
                sb.Append($"  \"battleSeed\": \"{seed}\",\n");
                sb.Append($"  \"viewpointId\": \"{viewpointId}\",\n");
                sb.Append($"  \"viewKind\": \"{viewKind}\",\n");
                sb.Append($"  \"slotId\": {slotId},\n");
                sb.Append(string.Format(Inv,
                    "  \"window\": {{\"t0\": {0}, \"t1\": {1}, \"padPastT1\": {2}}},\n",
                    t0, t1, padPastT1));
                sb.Append($"  \"output\": {{\"width\": {width}, " +
                    $"\"height\": {height}, \"fps\": {fps}}},\n");
                sb.Append(string.Format(Inv,
                    "  \"lensGuardRadiusM\": {0}, \"maxWheelFlankSpeedMps\": {1},\n",
                    lensGuardRadiusM, maxWheelFlankSpeedMps));
                sb.Append("  \"inputChecksums\": {\n");
                sb.Append($"    \"heightmap.json\": \"{heightmapJsonSha}\",\n");
                sb.Append($"    \"heightmap.raw\": \"{heightmapRawSha}\",\n");
                sb.Append($"    \"environment.json\": \"{environmentJsonSha}\",\n");
                sb.Append($"    \"environment_splat.raw\": \"{environmentSplatSha}\"\n");
                sb.Append("  },\n");
                sb.Append($"  \"settingsHash\": \"{settingsHash}\"\n");
                sb.Append("}\n");
                return sb.ToString();
            }
        }

        internal static Freeze BuildFreeze(
            ViewpointDefinition vp, AngleActionContext ctx)
        {
            string cropDir = Path.Combine(RepoRoot, "data/heightmap_angle");
            var f = new Freeze
            {
                gitSha = GitSha(),
                unityVersion = Application.unityVersion,
                pipelineAssetPath = HdrpMigration.OfflineAssetPath,
                pipelineAssetSha = Sha256File(Path.Combine(
                    RepoRoot, "app", HdrpMigration.OfflineAssetPath)),
                packagesLockSha = Sha256File(Path.Combine(
                    RepoRoot, "app/Packages/packages-lock.json")),
                colorSpace = PlayerSettings.colorSpace.ToString(),
                bundleChecksum = ctx.bundle.checksum,
                seed = ctx.seed,
                viewpointId = vp.id,
                viewKind = vp.viewKind,
                slotId = vp.slotId,
                t0 = (float)vp.t0,
                t1 = (float)vp.t1,
                padPastT1 = PadPastT1,
                width = Width,
                height = Height,
                fps = Fps,
                lensGuardRadiusM = LensGuard.DefaultRadiusM,
                maxWheelFlankSpeedMps = AngleActionContext.MaxWheelFlankSpeedMps,
                heightmapJsonSha = Sha256File(Path.Combine(cropDir, "heightmap.json")),
                heightmapRawSha = Sha256File(Path.Combine(cropDir, "heightmap.raw")),
                environmentJsonSha = Sha256File(Path.Combine(cropDir, "environment.json")),
                environmentSplatSha = Sha256File(Path.Combine(cropDir, "environment_splat.raw")),
            };
            f.settingsHash = Sha256Text(string.Join("|", new[]
            {
                f.unityVersion, f.pipelineAssetPath, f.pipelineAssetSha,
                f.packagesLockSha, f.colorSpace, f.bundleChecksum, f.seed,
                f.viewpointId, f.viewKind, f.slotId.ToString(Inv),
                f.t0.ToString(Inv), f.t1.ToString(Inv), f.padPastT1.ToString(Inv),
                f.width.ToString(Inv), f.height.ToString(Inv), f.fps.ToString(Inv),
                f.lensGuardRadiusM.ToString(Inv),
                f.maxWheelFlankSpeedMps.ToString(Inv),
                f.heightmapJsonSha, f.heightmapRawSha,
                f.environmentJsonSha, f.environmentSplatSha,
            }));
            return f;
        }

        // ------------------------------------------------------------------
        // Preflight: the whole-window no-teleport assertion (the Gate P9
        // defect class must be provably absent from every slot before the
        // production render bakes 19,815 frames).
        //
        // Two passes: a 10 Hz sweep of every slot (any instantaneous pop
        // or sustained super-sprint movement shows in a 0.1 s delta), then
        // a 30 fps refinement of every suspicious pair to classify it
        // against the per-frame bounds (with the designed crossing-exit
        // exemption). The camera path is swept at full 30 fps.
        // ------------------------------------------------------------------
        static void PreflightInner()
        {
            var sw = Stopwatch.StartNew();
            var ctx = Phase10TeleportProbe.CompileContext();
            var vp = Phase10TeleportProbe.LoadHeroViewpoint();
            var settings = GateP9Render.Settings(vp, thirdPerson: false);
            float t0 = (float)vp.t0;
            float tEnd = (float)vp.t1 + PadPastT1;

            // pass 1: 10 Hz coarse sweep, every unit, every slot
            const float coarseDt = 0.1f;
            // a 0.1 s window can legitimately contain sprint motion plus
            // one crossing-exit hand-off pop
            float coarseBound = 8f * coarseDt + SoldierActionResolver.CrossTravelM;
            const float suspectBound = 8f * coarseDt; // refine above this
            int coarseSamples = (int)((tEnd - t0) / coarseDt) + 1;
            var suspects = new List<(int unit, int slot, float t)>();
            long pairs = 0;
            foreach (var ur in ctx.units)
            {
                for (int slot = 0; slot < ur.slotCount; slot++)
                {
                    Vector2 prev = Vector2.zero;
                    for (int i = 0; i < coarseSamples; i++)
                    {
                        float t = t0 + i * coarseDt;
                        var st = SoldierActionResolver.Resolve(
                            ctx, ur.unitIndex, slot, t);
                        var p = new Vector2(st.posX, st.posZ);
                        if (i > 0)
                        {
                            pairs++;
                            float d = (p - prev).magnitude;
                            if (d > coarseBound)
                                throw new InvalidOperationException(
                                    $"PREFLIGHT FAIL: {ur.unit.unitId} slot " +
                                    $"{slot} moved {d:F2} m in 0.1 s at t={t:F2}");
                            if (d > suspectBound)
                                suspects.Add((ur.unitIndex, slot, t));
                        }
                        prev = p;
                    }
                }
            }
            Debug.Log($"Phase10Render preflight: coarse sweep {pairs} pairs, " +
                $"{suspects.Count} suspect windows, {sw.Elapsed.TotalSeconds:F0} s");

            // pass 2: refine suspects at render rate
            var violations = new List<string>();
            int crossExits = 0;
            foreach (var (unit, slot, tc) in suspects)
            {
                var ur = ctx.units[unit];
                var prevSt = SoldierActionResolver.Resolve(
                    ctx, unit, slot, tc - coarseDt);
                var prev = new Vector2(prevSt.posX, prevSt.posZ);
                var prevClip = prevSt.clip;
                for (int k = 1; k <= 3; k++)
                {
                    float t = tc - coarseDt + k / (float)Fps;
                    var st = SoldierActionResolver.Resolve(ctx, unit, slot, t);
                    var p = new Vector2(st.posX, st.posZ);
                    float d = (p - prev).magnitude;
                    bool crossExit = prevClip == ClipId.Cross && st.clip != ClipId.Cross;
                    if (crossExit) crossExits++;
                    float bound = crossExit ? MaxCrossExitDeltaM : MaxDeltaPerFrameM;
                    if (d > bound)
                        violations.Add(string.Format(Inv,
                            "{0} slot {1} t={2:F3}: {3:F2} m/frame (clip {4})",
                            ur.unit.unitId, slot, t, d, st.clip));
                    prev = p;
                    prevClip = st.clip;
                }
            }

            // camera sweep at full render rate
            int frames = Mathf.RoundToInt((tEnd - t0) * Fps);
            Vector2 prevCam = Vector2.zero;
            float camMax = 0f;
            for (int f = 0; f < frames; f++)
            {
                float t = t0 + f / (float)Fps;
                var pose = HeroViewpointCamera.Pose(ctx, settings, t);
                var cam = new Vector2(pose.camX, pose.camZ);
                if (f > 0)
                {
                    float d = (cam - prevCam).magnitude;
                    camMax = Mathf.Max(camMax, d);
                    if (d > MaxCameraDeltaPerFrameM)
                        violations.Add(string.Format(Inv,
                            "CAMERA t={0:F3}: {1:F3} m/frame", t, d));
                }
                prevCam = cam;
            }

            Directory.CreateDirectory(EvidenceDir);
            var report = new StringBuilder();
            report.Append("{\n");
            report.Append(string.Format(Inv,
                "  \"window\": {{\"t0\": {0}, \"t1\": {1}}},\n", t0, tEnd));
            report.Append($"  \"coarsePairsChecked\": {pairs},\n");
            report.Append($"  \"suspectWindowsRefined\": {suspects.Count},\n");
            report.Append($"  \"crossingExitHandoffs\": {crossExits},\n");
            report.Append(string.Format(Inv,
                "  \"cameraFrames\": {0}, \"cameraMaxDeltaM\": {1:F4},\n",
                frames, camMax));
            report.Append(string.Format(Inv,
                "  \"boundPerFrameM\": {0:F3}, \"crossExitBoundM\": {1:F3}, " +
                "\"cameraBoundM\": {2:F3},\n",
                MaxDeltaPerFrameM, MaxCrossExitDeltaM, MaxCameraDeltaPerFrameM));
            report.Append($"  \"violations\": [");
            for (int i = 0; i < violations.Count; i++)
                report.Append((i > 0 ? "," : "") + "\n    \"" + violations[i] + "\"");
            report.Append(violations.Count > 0 ? "\n  ],\n" : "],\n");
            report.Append($"  \"pass\": {(violations.Count == 0 ? "true" : "false")},\n");
            report.Append(string.Format(Inv,
                "  \"sweepSeconds\": {0:F0}\n", sw.Elapsed.TotalSeconds));
            report.Append("}\n");
            File.WriteAllText(
                Path.Combine(EvidenceDir, "p10-preflight.json"),
                report.ToString());
            Debug.Log($"Phase10Render preflight: {violations.Count} violations, " +
                $"total {sw.Elapsed.TotalSeconds:F0} s");
            if (violations.Count > 0)
                throw new InvalidOperationException(
                    $"PREFLIGHT FAIL: {violations.Count} per-frame teleport " +
                    "violations; see p10-preflight.json");
        }

        // ------------------------------------------------------------------
        // Shared frame loop.
        // ------------------------------------------------------------------
        // internal: reused by AngleV2Render (the v2 data-wave harness) and
        // by IversonProductionRender, which drives the same loop over the
        // Oak Ridge staging (the site-parameterized P10 pattern)
        internal static float RenderFrames(
            AngleActionScene scene, RenderTexture rt, Texture2D tex,
            HeroCameraSettings settings, float viewT0,
            int frame0, int count, string dir, ref bool warmed,
            ref long peakManagedMB)
        {
            Directory.CreateDirectory(dir);
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                int frame = frame0 + i;
                float t = viewT0 + frame / (float)Fps;
                // camera BEFORE pose: smoke billboards and the lens guard
                // read the camera position
                GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                    scene.ctx, settings, t));
                scene.Pose(t);
                if (!warmed)
                {
                    for (int w = 0; w < WarmupFrames; w++)
                        GateP6Render.RenderOnce(scene.camera, rt, null);
                    if (!(UnityEngine.Rendering.RenderPipelineManager.currentPipeline
                        is UnityEngine.Rendering.HighDefinition.HDRenderPipeline))
                        throw new InvalidOperationException(
                            "HDRP did not construct; run with -buildTarget OSXUniversal");
                    warmed = true;
                }
                GateP6Render.RenderOnce(scene.camera, rt, tex);
                File.WriteAllBytes(
                    Path.Combine(dir, $"frame_{frame:D6}.png"),
                    tex.EncodeToPNG());
                if (i % 300 == 0)
                {
                    peakManagedMB = Math.Max(peakManagedMB,
                        GC.GetTotalMemory(false) / (1024 * 1024));
                    Debug.Log($"Phase10Render: frame {frame} " +
                        $"({i}/{count} of range, " +
                        $"{sw.Elapsed.TotalSeconds / (i + 1):F2} s/frame)");
                }
            }
            peakManagedMB = Math.Max(peakManagedMB,
                GC.GetTotalMemory(false) / (1024 * 1024));
            return (float)(sw.Elapsed.TotalSeconds / count);
        }

        // ------------------------------------------------------------------
        // Before/after evidence for the Gate P9 must-fix: the exact proof
        // window, re-rendered with the fixes.
        // ------------------------------------------------------------------
        static void TeleportFixInner()
        {
            var vp = Phase10TeleportProbe.LoadHeroViewpoint();
            var (scene, rt, tex) = GateP9Render.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;

                const float t0 = 8610f;
                int frames = 60 * Fps;
                string dir = Path.Combine(EvidenceDir, "seq-teleport-fix");
                bool warmed = false;
                long peak = 0;
                float spf = RenderFrames(scene, rt, tex, settings,
                    t0, 0, frames, dir, ref warmed, ref peak);
                Debug.Log($"Phase10Render: teleport-fix window done " +
                    $"({spf:F2} s/frame, peak {peak} MB)");
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // ------------------------------------------------------------------
        // Gate P10 determinism pair: two INDEPENDENT stagings of the same
        // ten seconds; logical metadata identical, pixels within the
        // documented GPU tolerance.
        // ------------------------------------------------------------------
        const float DetT0 = 8400f;
        const int DetFrames = 10 * Fps;

        static void DeterminismInner()
        {
            var vp = Phase10TeleportProbe.LoadHeroViewpoint();
            string dirA = Path.Combine(OutRoot, "det-a");
            string dirB = Path.Combine(OutRoot, "det-b");
            var digests = new string[2][];
            var freezes = new string[2];

            for (int pass = 0; pass < 2; pass++)
            {
                var (scene, rt, tex) = GateP9Render.Boot(
                    out var prevDefault, out var prevQuality, out var prevGlobal);
                try
                {
                    var obs = scene.ctx.Unit(vp.unitId);
                    var settings = GateP9Render.Settings(vp, thirdPerson: false);
                    scene.hiddenUnitIndex = obs.unitIndex;
                    scene.hiddenSlot = vp.slotId;
                    var freeze = BuildFreeze(vp, scene.ctx);
                    freezes[pass] = freeze.ToJson();

                    bool warmed = false;
                    long peak = 0;
                    RenderFrames(scene, rt, tex, settings, DetT0, 0, DetFrames,
                        pass == 0 ? dirA : dirB, ref warmed, ref peak);

                    // logical digests at probe frames (metadata identity)
                    digests[pass] = new string[DetFrames / 30];
                    for (int i = 0; i < digests[pass].Length; i++)
                    {
                        float t = DetT0 + i;
                        digests[pass][i] = scene.LogicalStateDigest(t);
                    }
                }
                finally
                {
                    GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
                }
            }

            // compare
            bool metadataIdentical = freezes[0] == freezes[1];
            if (!metadataIdentical)
            {
                Directory.CreateDirectory(EvidenceDir);
                File.WriteAllText(
                    Path.Combine(EvidenceDir, "p10-freeze-pass-a.json"), freezes[0]);
                File.WriteAllText(
                    Path.Combine(EvidenceDir, "p10-freeze-pass-b.json"), freezes[1]);
            }
            bool digestsIdentical = true;
            for (int i = 0; i < digests[0].Length; i++)
                if (digests[0][i] != digests[1][i]) digestsIdentical = false;

            float worstPct = 0f;
            int worstDelta = 0;
            int worstOutliers = 0;
            var perFrame = new StringBuilder();
            var ta = new Texture2D(2, 2);
            var tb = new Texture2D(2, 2);
            for (int f = 0; f < DetFrames; f++)
            {
                ta.LoadImage(File.ReadAllBytes(
                    Path.Combine(dirA, $"frame_{f:D6}.png")));
                tb.LoadImage(File.ReadAllBytes(
                    Path.Combine(dirB, $"frame_{f:D6}.png")));
                var pa = ta.GetPixels32();
                var pb = tb.GetPixels32();
                long ndiff = 0;
                int maxd = 0;
                int outliers = 0;
                for (int i = 0; i < pa.Length; i++)
                {
                    int d = Mathf.Max(
                        Mathf.Abs(pa[i].r - pb[i].r),
                        Mathf.Max(Mathf.Abs(pa[i].g - pb[i].g),
                            Mathf.Abs(pa[i].b - pb[i].b)));
                    if (d > 0) { ndiff++; if (d > maxd) maxd = d; }
                    if (d > TolMaxDelta) outliers++;
                }
                float pct = 100f * ndiff / pa.Length;
                worstPct = Mathf.Max(worstPct, pct);
                worstDelta = Mathf.Max(worstDelta, maxd);
                worstOutliers = Mathf.Max(worstOutliers, outliers);
                if (f % 60 == 0 || pct > TolDiffPct || outliers > MaxOutlierPixels)
                    perFrame.Append(string.Format(Inv,
                        (perFrame.Length > 0 ? "," : "") +
                        "\n    {{\"frame\": {0}, \"differingPct\": {1:F2}, " +
                        "\"maxChannelDelta\": {2}, \"outlierPixels\": {3}}}",
                        f, pct, maxd, outliers));
            }
            bool pixelsWithin = worstPct <= TolDiffPct &&
                worstOutliers <= MaxOutlierPixels;

            Directory.CreateDirectory(EvidenceDir);
            // keep three comparison frame pairs for the evidence dir
            foreach (int f in new[] { 0, 150, 299 })
            {
                File.Copy(Path.Combine(dirA, $"frame_{f:D6}.png"),
                    Path.Combine(EvidenceDir, $"p10-det-a-{f:D6}.png"), true);
                File.Copy(Path.Combine(dirB, $"frame_{f:D6}.png"),
                    Path.Combine(EvidenceDir, $"p10-det-b-{f:D6}.png"), true);
            }
            var rep = new StringBuilder();
            rep.Append("{\n");
            rep.Append(string.Format(Inv,
                "  \"range\": {{\"t0\": {0}, \"frames\": {1}}},\n",
                DetT0, DetFrames));
            rep.Append($"  \"freezeMetadataIdentical\": {(metadataIdentical ? "true" : "false")},\n");
            rep.Append($"  \"logicalDigestsIdentical\": {(digestsIdentical ? "true" : "false")},\n");
            rep.Append(string.Format(Inv,
                "  \"worstDifferingPct\": {0:F2}, \"worstMaxChannelDelta\": {1}, " +
                "\"worstOutlierPixels\": {2},\n",
                worstPct, worstDelta, worstOutliers));
            rep.Append(string.Format(Inv,
                "  \"toleranceMaxChannelDelta\": {0}, \"toleranceDifferingPct\": {1}, " +
                "\"toleranceOutlierPixels\": {2},\n",
                TolMaxDelta, TolDiffPct, MaxOutlierPixels));
            rep.Append($"  \"pixelsWithinTolerance\": {(pixelsWithin ? "true" : "false")},\n");
            rep.Append("  \"sampledFrames\": [" + perFrame + "\n  ]\n}\n");
            File.WriteAllText(Path.Combine(EvidenceDir, "p10-determinism.json"),
                rep.ToString());
            File.WriteAllText(Path.Combine(EvidenceDir, "p10-freeze.json"),
                freezes[0]);
            Debug.Log($"Phase10Render determinism: metadata={metadataIdentical} " +
                $"digests={digestsIdentical} pixels worst {worstPct:F2}%/{worstDelta}");

            if (!metadataIdentical || !digestsIdentical)
                throw new InvalidOperationException(
                    "GATE FAIL: logical metadata not identical between passes");
            if (!pixelsWithin)
                throw new InvalidOperationException(
                    "GATE FAIL: pixel differences exceed the documented GPU tolerance");
        }

        // ------------------------------------------------------------------
        // The production render: chunked and resumable.
        // ------------------------------------------------------------------
        static void ProductionInner()
        {
            var vp = Phase10TeleportProbe.LoadHeroViewpoint();
            float t0 = (float)vp.t0;
            float tEnd = (float)vp.t1 + PadPastT1;
            int totalFrames = Mathf.RoundToInt((tEnd - t0) * Fps);
            int framesPerChunk = Mathf.RoundToInt(ChunkSeconds * Fps);
            int chunkCount = (totalFrames + framesPerChunk - 1) / framesPerChunk;

            Directory.CreateDirectory(SeqDir);
            Directory.CreateDirectory(ManifestDir);

            var (scene, rt, tex) = GateP9Render.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;
                var freeze = BuildFreeze(vp, scene.ctx);
                File.WriteAllText(Path.Combine(OutRoot, "p10-freeze.json"),
                    freeze.ToJson());
                Directory.CreateDirectory(EvidenceDir);
                File.WriteAllText(Path.Combine(EvidenceDir, "p10-freeze.json"),
                    freeze.ToJson());

                bool warmed = false;
                var totalSw = Stopwatch.StartNew();
                int rendered = 0, skipped = 0;
                for (int c = 0; c < chunkCount; c++)
                {
                    int frame0 = c * framesPerChunk;
                    int count = Math.Min(framesPerChunk, totalFrames - frame0);
                    string manifestPath = Path.Combine(
                        ManifestDir, $"chunk_{c:D3}.json");
                    if (ChunkComplete(manifestPath, c, frame0, count))
                    {
                        skipped += count;
                        Debug.Log($"Phase10Render: chunk {c} complete, skipping");
                        continue;
                    }
                    long peak = 0;
                    float spf = RenderFrames(scene, rt, tex, settings,
                        t0, frame0, count, SeqDir, ref warmed, ref peak);
                    rendered += count;
                    WriteChunkManifest(manifestPath, c, frame0, count,
                        t0, freeze, spf, peak);
                    Debug.Log($"Phase10Render: chunk {c}/{chunkCount - 1} done " +
                        $"({spf:F2} s/frame, peak {peak} MB managed)");
                    // Long batch renders leak NATIVE memory (~both
                    // production attempts were jetsam-SIGKILLed after
                    // ~4,800 frames with managed memory flat at ~1 GB).
                    // Release what can be released at every chunk
                    // boundary; the outer runner loop
                    // (scripts/p10-render-loop.sh) resumes a killed
                    // process from the next incomplete chunk regardless.
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    GC.Collect();
                }
                Debug.Log($"Phase10Render: production render complete — " +
                    $"{rendered} rendered, {skipped} resumed-skipped, " +
                    $"{totalSw.Elapsed.TotalHours:F2} h wall");
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // A chunk is complete when its manifest exists AND its frames are
        // still on disk — OR the rolling chunk harvester
        // (scripts/p10-chunk-harvester.sh) has already encoded it and
        // reclaimed the PNGs (the render machine's free disk cannot hold
        // the full ~63 GB sequence; measured ~3.2 MB/frame).
        static bool ChunkComplete(string manifestPath, int chunk,
            int frame0, int count)
        {
            if (!File.Exists(manifestPath)) return false;
            if (File.Exists(Path.Combine(
                    OutRoot, "chunks", $"chunk_{chunk:D3}.mp4")))
                return true;
            for (int i = 0; i < count; i++)
                if (!File.Exists(Path.Combine(
                        SeqDir, $"frame_{frame0 + i:D6}.png")))
                    return false;
            return true;
        }

        // internal: reused by AngleV2Render (the v2 data-wave harness)
        internal static void WriteChunkManifest(
            string path, int chunk, int frame0, int count, float viewT0,
            Freeze freeze, float secondsPerFrame, long peakManagedMB)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append($"  \"chunk\": {chunk},\n");
            sb.Append($"  \"frame0\": {frame0},\n");
            sb.Append($"  \"frameCount\": {count},\n");
            sb.Append(string.Format(Inv,
                "  \"battleT0\": {0:F4},\n  \"battleT1\": {1:F4},\n",
                viewT0 + frame0 / (float)Fps,
                viewT0 + (frame0 + count - 1) / (float)Fps));
            sb.Append($"  \"gitSha\": \"{freeze.gitSha}\",\n");
            sb.Append($"  \"bundleChecksum\": \"{freeze.bundleChecksum}\",\n");
            sb.Append($"  \"viewpointId\": \"{freeze.viewpointId}\",\n");
            sb.Append($"  \"settingsHash\": \"{freeze.settingsHash}\",\n");
            sb.Append("  \"frameTimes\": \"t(frame) = " +
                string.Format(Inv, "{0} + frame/{1}", viewT0, Fps) + "\",\n");
            sb.Append("  \"measured\": " + string.Format(Inv,
                "{{\"secondsPerFrame\": {0:F3}, \"peakManagedMB\": {1}}}\n",
                secondsPerFrame, peakManagedMB));
            sb.Append("}\n");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
