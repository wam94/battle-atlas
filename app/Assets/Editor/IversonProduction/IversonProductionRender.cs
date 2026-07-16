using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BattleAtlas.EditorTools
{
    // ------------------------------------------------------------------
    // The SECOND Soldier View production render: `iverson-forney-field`
    // (Oak Ridge / Forney field, July 1 afternoon), the P10 pattern
    // site-parameterized per the design doc (iverson-viewpoint-design.md
    // §6 "Production-render plan"). Staging is IversonGateRender.Boot
    // (Oak Ridge crop + the Iverson bundle); the frame loop, chunking,
    // manifests, and tolerances are Phase10Render's, unchanged.
    //
    //   Preflight             global no-teleport sweep over the padded
    //                         window t=5830..7040.5 (every unit, every
    //                         slot) + camera sweep; hard-fails before any
    //                         render hours are committed.
    //   RenderDeterminismPair two INDEPENDENT stagings of t=6300..6310;
    //                         logical + pose digests must be bitwise
    //                         identical; pixels within the documented
    //                         GPU tolerance.
    //   RenderProduction      the full chunked resumable render (36,315
    //                         frames at 2560x1440/30 fps). A killed run
    //                         resumes; harvested chunk encodes count as
    //                         complete (scripts/iverson-chunk-harvester.sh).
    //   RenderStills          the five representative production stills
    //                         (advance / volley / destruction /
    //                         prone-fight / end) into the evidence dir.
    //   ExportAudioEvents     the deterministic event streams for the
    //                         9-stem mix, exported to the PRODUCTION
    //                         evidence dir (the gate export is design-
    //                         gate evidence and stays untouched).
    //
    // ED-21 production pin: every entry point refuses to run unless the
    // bundle's stagingSeed is the pinned reviewed checksum (2f15dd2f…) —
    // the same value IversonBundleTests and test_iverson_v2.py enforce.
    //
    // Usage (repo root; Unity editor closed, worktree Library):
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.IversonProductionRender.<Method> \
    //     -logFile iverson-<method>.log
    // ------------------------------------------------------------------
    public static class IversonProductionRender
    {
        const int Width = 2560, Height = 1440, Fps = 30;

        public const string ViewpointId = "iverson-forney-field";

        // ED-21 production pin (iverson-viewpoint-design.md §8.5): the
        // checksum of the bundle the owner reviewed at the design gate.
        public const string PinnedStagingSeed =
            "2f15dd2f4e5e399e9899de45e5606a5610309dfad503ff472edf5e2edb09bf43";

        // Media contract (p1-media-contract.md): the last ~3 frames of a
        // stream are unreachable seek targets (EndGuardFrames = 4), so the
        // render runs 0.5 s past t1 — the guard sits in padding.
        public const float PadPastT1 = 0.5f;

        // Resumable chunking (the P10 SIGKILL lessons).
        const float ChunkSeconds = 60f;

        // Documented GPU pixel tolerance (Phase 8 envelope + the P10
        // independent-staging outlier allowance).
        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;
        const int MaxOutlierPixels = 8;

        // No-teleport bounds (NoTeleportTests / Phase10Render.Preflight).
        const float MaxDeltaPerFrameM = 8f / Fps;             // sprint
        const float MaxCrossExitDeltaM =
            SoldierActionResolver.CrossTravelM + 8f / Fps;
        const float MaxCameraDeltaPerFrameM = 4.5f / Fps;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static string RepoRoot => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../.."));
        static string OutRoot => Path.Combine(
            RepoRoot, "app/RenderOutput/iverson");
        static string SeqDir => Path.Combine(OutRoot, "seq-full");
        static string ManifestDir => Path.Combine(OutRoot, "manifests");
        static string EvidenceDir => Path.Combine(
            RepoRoot, "docs/benchmarks/captures/iverson-production");
        static string OakridgeCropDir => Path.Combine(
            RepoRoot, "data/heightmap_oakridge");
        static string BundlePath => Path.Combine(
            Application.dataPath, "Battle", "Iverson", "iverson.bundle.json");

        public static void Preflight() => Run(PreflightInner);
        public static void RenderDeterminismPair() => Run(DeterminismInner);
        public static void RenderProduction() => Run(ProductionInner);
        public static void RenderStills() => Run(StillsInner);
        public static void ExportAudioEvents() => Run(() =>
        {
            AssertPin(AngleBundleLoader.Load(BundlePath));
            IversonGateRender.WriteAudioEvents(EvidenceDir);
        });

        static void Run(Action inner)
        {
            int exitCode = 0;
            try { inner(); }
            catch (Exception e)
            {
                Debug.LogError($"IversonProductionRender failed: {e}");
                exitCode = 1;
            }
            finally
            {
                AngleEnvironmentStage.CropDirOverride = null;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void AssertPin(AngleBundle bundle)
        {
            if (bundle.StagingSeed != PinnedStagingSeed)
                throw new InvalidOperationException(
                    "ED-21 PIN VIOLATION: the Iverson bundle's stagingSeed is "
                    + $"'{bundle.StagingSeed}', not the pinned reviewed checksum "
                    + $"'{PinnedStagingSeed.Substring(0, 12)}…' — the production "
                    + "render only runs from the reviewed choreography");
        }

        // Deterministic action context WITHOUT figure staging (preflight
        // needs positions, not pixels) — the IversonGateRender audio-export
        // compile path.
        static AngleActionContext CompileContext()
        {
            AngleEnvironmentStage.CropDirOverride = OakridgeCropDir;
            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var env = AngleEnvironmentStage.StageAll();
            var bundle = AngleBundleLoader.Load(BundlePath);
            AssertPin(bundle);
            var obstacles = new Dictionary<string, List<Vector2>>();
            foreach (var fence in env.env.fences)
                obstacles[fence.featureId] =
                    AngleEnvironmentData.Points(fence.polylineFlat);
            obstacles[env.env.wall.featureId] =
                AngleEnvironmentData.Points(env.env.wall.polylineFlat);
            return AngleActionContext.Compile(
                bundle, bundle.StagingSeed, obstacles);
        }

        // ------------------------------------------------------------------
        // Freeze record — Phase10Render's shape with this site's inputs.
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

        static Phase10Render.Freeze BuildFreeze(
            ViewpointDefinition vp, AngleActionContext ctx)
        {
            var f = new Phase10Render.Freeze
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
                heightmapJsonSha = Sha256File(
                    Path.Combine(OakridgeCropDir, "heightmap.json")),
                heightmapRawSha = Sha256File(
                    Path.Combine(OakridgeCropDir, "heightmap.raw")),
                environmentJsonSha = Sha256File(
                    Path.Combine(OakridgeCropDir, "environment.json")),
                environmentSplatSha = Sha256File(
                    Path.Combine(OakridgeCropDir, "environment_splat.raw")),
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
        // Preflight — the global no-teleport gate over THIS window (the
        // p10-teleport-postmortem discipline: LensGuard, the smoothed
        // formation frame, and the crossing chain all apply to this
        // geometry; prove the compiled tracks clean before 36,315 frames).
        // ------------------------------------------------------------------
        static void PreflightInner()
        {
            var sw = Stopwatch.StartNew();
            var ctx = CompileContext();
            var vp = IversonGateRender.LoadViewpoint();
            var settings = GateP9Render.Settings(vp, thirdPerson: false);
            float t0 = (float)vp.t0;
            float tEnd = (float)vp.t1 + PadPastT1;

            // pass 1: 10 Hz coarse sweep, every unit, every slot
            const float coarseDt = 0.1f;
            float coarseBound = 8f * coarseDt + SoldierActionResolver.CrossTravelM;
            const float suspectBound = 8f * coarseDt;
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
            Debug.Log($"IversonProductionRender preflight: coarse sweep {pairs} " +
                $"pairs, {suspects.Count} suspect windows, " +
                $"{sw.Elapsed.TotalSeconds:F0} s");

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
            report.Append($"  \"viewpointId\": \"{vp.id}\",\n");
            report.Append($"  \"bundleChecksum\": \"{ctx.bundle.checksum}\",\n");
            report.Append($"  \"battleSeed\": \"{ctx.seed}\",\n");
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
                Path.Combine(EvidenceDir, "iverson-preflight.json"),
                report.ToString());
            Debug.Log($"IversonProductionRender preflight: {violations.Count} " +
                $"violations, total {sw.Elapsed.TotalSeconds:F0} s");
            if (violations.Count > 0)
                throw new InvalidOperationException(
                    $"PREFLIGHT FAIL: {violations.Count} per-frame teleport " +
                    "violations; see iverson-preflight.json");
        }

        // ------------------------------------------------------------------
        // Determinism pair: two INDEPENDENT stagings of t=6300..6310 (the
        // fight at full intensity: three regiments prone-firing, Cutler's
        // changed-front keyframe, ~2,300 muskets of smoke).
        // ------------------------------------------------------------------
        const float DetT0 = 6300f;
        const int DetFrames = 10 * Fps;

        static void DeterminismInner()
        {
            var vp = IversonGateRender.LoadViewpoint();
            string dirA = Path.Combine(OutRoot, "det-a");
            string dirB = Path.Combine(OutRoot, "det-b");
            var digests = new string[2][];
            var freezes = new string[2];

            for (int pass = 0; pass < 2; pass++)
            {
                var (scene, rt, tex) = IversonGateRender.Boot(
                    out var prevDefault, out var prevQuality, out var prevGlobal);
                try
                {
                    AssertPin(scene.ctx.bundle);
                    var obs = scene.ctx.Unit(vp.unitId);
                    var settings = GateP9Render.Settings(vp, thirdPerson: false);
                    scene.hiddenUnitIndex = obs.unitIndex;
                    scene.hiddenSlot = vp.slotId;
                    var freeze = BuildFreeze(vp, scene.ctx);
                    freezes[pass] = freeze.ToJson();

                    bool warmed = false;
                    long peak = 0;
                    Phase10Render.RenderFrames(scene, rt, tex, settings,
                        DetT0, 0, DetFrames,
                        pass == 0 ? dirA : dirB, ref warmed, ref peak);

                    // logical + camera-pose digests at 1 Hz probes
                    digests[pass] = new string[DetFrames / 30];
                    for (int i = 0; i < digests[pass].Length; i++)
                    {
                        float t = DetT0 + i;
                        digests[pass][i] = scene.LogicalStateDigest(t) + "|" +
                            IversonGateRender.PoseDigest(scene.ctx, settings, t);
                    }
                }
                finally
                {
                    GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
                }
            }

            bool metadataIdentical = freezes[0] == freezes[1];
            if (!metadataIdentical)
            {
                Directory.CreateDirectory(EvidenceDir);
                File.WriteAllText(Path.Combine(
                    EvidenceDir, "iverson-freeze-pass-a.json"), freezes[0]);
                File.WriteAllText(Path.Combine(
                    EvidenceDir, "iverson-freeze-pass-b.json"), freezes[1]);
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
            foreach (int f in new[] { 0, 150, 299 })
            {
                File.Copy(Path.Combine(dirA, $"frame_{f:D6}.png"),
                    Path.Combine(EvidenceDir, $"iverson-det-a-{f:D6}.png"), true);
                File.Copy(Path.Combine(dirB, $"frame_{f:D6}.png"),
                    Path.Combine(EvidenceDir, $"iverson-det-b-{f:D6}.png"), true);
            }
            var rep = new StringBuilder();
            rep.Append("{\n");
            rep.Append(string.Format(Inv,
                "  \"range\": {{\"t0\": {0}, \"frames\": {1}}},\n",
                DetT0, DetFrames));
            rep.Append($"  \"freezeMetadataIdentical\": {(metadataIdentical ? "true" : "false")},\n");
            rep.Append($"  \"logicalAndPoseDigestsIdentical\": {(digestsIdentical ? "true" : "false")},\n");
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
            File.WriteAllText(
                Path.Combine(EvidenceDir, "iverson-determinism.json"),
                rep.ToString());
            File.WriteAllText(
                Path.Combine(EvidenceDir, "iverson-freeze.json"), freezes[0]);
            Debug.Log($"IversonProductionRender determinism: " +
                $"metadata={metadataIdentical} digests={digestsIdentical} " +
                $"pixels worst {worstPct:F2}%/{worstDelta}");

            if (!metadataIdentical || !digestsIdentical)
                throw new InvalidOperationException(
                    "GATE FAIL: logical metadata not identical between passes");
            if (!pixelsWithin)
                throw new InvalidOperationException(
                    "GATE FAIL: pixel differences exceed the documented GPU tolerance");
        }

        // ------------------------------------------------------------------
        // The production render: chunked and resumable (the P10 pattern).
        // ------------------------------------------------------------------
        static void ProductionInner()
        {
            var vp = IversonGateRender.LoadViewpoint();
            float t0 = (float)vp.t0;
            float tEnd = (float)vp.t1 + PadPastT1;
            int totalFrames = Mathf.RoundToInt((tEnd - t0) * Fps);
            int framesPerChunk = Mathf.RoundToInt(ChunkSeconds * Fps);
            int chunkCount = (totalFrames + framesPerChunk - 1) / framesPerChunk;

            Directory.CreateDirectory(SeqDir);
            Directory.CreateDirectory(ManifestDir);

            var (scene, rt, tex) = IversonGateRender.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                AssertPin(scene.ctx.bundle);
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;
                var freeze = BuildFreeze(vp, scene.ctx);
                File.WriteAllText(
                    Path.Combine(OutRoot, "iverson-freeze.json"), freeze.ToJson());
                Directory.CreateDirectory(EvidenceDir);
                File.WriteAllText(
                    Path.Combine(EvidenceDir, "iverson-freeze.json"),
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
                        Debug.Log($"IversonProductionRender: chunk {c} complete, skipping");
                        continue;
                    }
                    long peak = 0;
                    float spf = Phase10Render.RenderFrames(scene, rt, tex,
                        settings, t0, frame0, count, SeqDir, ref warmed, ref peak);
                    rendered += count;
                    Phase10Render.WriteChunkManifest(manifestPath, c, frame0,
                        count, t0, freeze, spf, peak);
                    Debug.Log($"IversonProductionRender: chunk {c}/{chunkCount - 1} " +
                        $"done ({spf:F2} s/frame, peak {peak} MB managed)");
                    // the P10 native-leak-meets-jetsam lesson: release what
                    // can be released at every chunk boundary; the outer
                    // loop (scripts/iverson-render-loop.sh) resumes a
                    // killed process from the next incomplete chunk
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    GC.Collect();
                }
                Debug.Log($"IversonProductionRender: production render complete — " +
                    $"{rendered} rendered, {skipped} resumed-skipped, " +
                    $"{totalSw.Elapsed.TotalHours:F2} h wall");
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

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

        // ------------------------------------------------------------------
        // The five representative production stills (task: advance /
        // volley / destruction / prone-fight / end), rendered through the
        // same staging + camera as the production frames.
        // ------------------------------------------------------------------
        static void StillsInner()
        {
            var vp = IversonGateRender.LoadViewpoint();
            var (scene, rt, tex) = IversonGateRender.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                AssertPin(scene.ctx.bundle);
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;
                var shots = new (string name, float t)[]
                {
                    ("ivp-still-5840-advance", 5840f),
                    ("ivp-still-5990-volley", 5990f),
                    ("ivp-still-6100-destruction", 6100f),
                    ("ivp-still-6600-prone-fight", 6600f),
                    ("ivp-still-7040-end", 7040f),
                };
                Directory.CreateDirectory(EvidenceDir);
                bool warmed = false;
                foreach (var shot in shots)
                {
                    GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                        scene.ctx, settings, shot.t));
                    scene.Pose(shot.t);
                    if (!warmed)
                    {
                        for (int i = 0; i < 3; i++)
                            GateP6Render.RenderOnce(scene.camera, rt, null);
                        warmed = true;
                    }
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    string p = Path.Combine(EvidenceDir, $"{shot.name}.png");
                    File.WriteAllBytes(p, tex.EncodeToPNG());
                    Debug.Log($"IversonProductionRender: wrote {p}");
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }
    }
}
