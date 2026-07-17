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
    // Generalized Soldier View production render harness (webb-wall +
    // cushing-canister slice; charge-intensity proposals OP-2/OP-3).
    //
    // Phase10Render is the proven pipeline but is hard-wired to the
    // garnett hero viewpoint; this class is the same pipeline —
    // GateP9Render.Boot staging, RenderPipeline.SubmitRenderRequest via
    // GateP6Render.RenderOnce, offline HDRP profile, chunked resumable
    // output, freeze records — parameterized by "-viewpointId <id>"
    // (falls back to the VIEWPOINT_ID environment variable). The
    // garnett entry points and their media are untouched (film safety):
    // this class only ADDS render capability for other committed
    // viewpoints of the SAME compiled Angle bundle.
    //
    //   DumpObserverProbe   no rendering: resolved observer-slot
    //                       geometry/facings at sample times, for slot
    //                       adjudication evidence before render hours.
    //   Preflight           the P10 global no-teleport gate over THIS
    //                       viewpoint's padded window (every unit, every
    //                       slot) + this viewpoint's camera sweep.
    //   RenderDeterminismPair
    //                       Gate P10 criterion for THIS viewpoint: 10 s
    //                       rendered twice from two independent stagings;
    //                       logical + camera-pose digests must be
    //                       bitwise identical; pixels within the
    //                       documented two-tier GPU tolerance.
    //   RenderProduction    the full chunked, resumable render for the
    //                       viewpoint's padded window.
    //   RenderStills        representative stills for the evidence dir.
    //   ExportAudioEvents   deterministic event export for the audio
    //                       stem builder, tagged with the observer's
    //                       unit and side so the mix maths can address a
    //                       defending (Union) observer.
    //
    // Usage (repo root; Unity editor closed):
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.ViewpointRender.<Method> \
    //     -viewpointId webb-wall -logFile <log>
    // ------------------------------------------------------------------
    public static class ViewpointRender
    {
        const int Width = 2560, Height = 1440, Fps = 30;
        const int WarmupFrames = 3;
        const float PadPastT1 = 0.5f;    // P1 media contract end-guard pad
        const float ChunkSeconds = 60f;

        // Documented GPU pixel tolerance (Phase 8 envelope + the P10
        // isolated-outlier tier; see Phase10Render).
        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;
        const int MaxOutlierPixels = 8;

        // No-teleport bounds (same as Phase10Render/NoTeleportTests).
        const float MaxDeltaPerFrameM = 8f / Fps;
        const float MaxCrossExitDeltaM = SoldierActionResolver.CrossTravelM + 8f / Fps;
        const float MaxCameraDeltaPerFrameM = 4.5f / Fps;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static string RepoRoot => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../.."));

        // ------------------------------------------------------------------
        // Viewpoint selection: -viewpointId <id> or VIEWPOINT_ID env var.
        // ------------------------------------------------------------------
        internal static string RequestedViewpointId()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-viewpointId") return args[i + 1];
            string env = Environment.GetEnvironmentVariable("VIEWPOINT_ID");
            if (!string.IsNullOrEmpty(env)) return env;
            throw new InvalidOperationException(
                "ViewpointRender: pass -viewpointId <id> (or VIEWPOINT_ID)");
        }

        internal static ViewpointDefinition LoadViewpoint(string id)
        {
            string path = Path.Combine(
                Application.streamingAssetsPath, "SoldierView/viewpoints.json");
            var set = ViewpointSet.FromJson(File.ReadAllText(path));
            foreach (var vp in set.viewpoints)
                if (vp.id == id) return vp;
            throw new InvalidOperationException(
                $"viewpoints.json has no viewpoint '{id}'");
        }

        static string OutRoot(string vpId) =>
            Path.Combine(RepoRoot, "app/RenderOutput", vpId);
        static string EvidenceDir(string vpId) => Path.Combine(
            RepoRoot, "docs/benchmarks/captures", vpId);

        // Per-viewpoint determinism-pair window starts (10 s each): chosen
        // inside each window at its smoke-heaviest stretch so the pair
        // stresses the hardest pixels (the garnett pair used t=8400).
        static float DeterminismT0(string vpId) => vpId switch
        {
            "webb-wall" => 8600f,        // wall crisis: canister + musketry smoke
            "cushing-canister" => 8520f, // rolling canister, guns hot
            _ => throw new InvalidOperationException(
                $"no determinism window recorded for '{vpId}'"),
        };

        public static void DumpObserverProbe() => Run(ObserverProbeInner);
        public static void Preflight() => Run(PreflightInner);
        public static void RenderDeterminismPair() => Run(DeterminismInner);
        public static void RenderProduction() => Run(ProductionInner);
        public static void RenderStills() => Run(StillsInner);
        public static void ExportAudioEvents() => Run(AudioEventsInner);

        static void Run(Action inner)
        {
            int exitCode = 0;
            try { inner(); }
            catch (Exception e)
            {
                Debug.LogError($"ViewpointRender failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        // ------------------------------------------------------------------
        // Observer probe: machine evidence for the slot adjudication.
        // ------------------------------------------------------------------
        static void ObserverProbeInner()
        {
            string vpId = RequestedViewpointId();
            var vp = LoadViewpoint(vpId);
            var ctx = Phase10TeleportProbe.CompileContext();
            var obs = ctx.Unit(vp.unitId);
            var settings = GateP9Render.Settings(vp, thirdPerson: false);

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append($"  \"viewpoint\": \"{vp.id}\",\n");
            sb.Append($"  \"unitId\": \"{vp.unitId}\", \"slotId\": {vp.slotId},\n");
            sb.Append($"  \"slotCount\": {obs.slotCount},\n");
            sb.Append($"  \"seed\": \"{ctx.seed}\",\n");
            sb.Append($"  \"bundleChecksum\": \"{ctx.bundle.checksum}\",\n");
            var cas = obs.casualties[vp.slotId];
            sb.Append("  \"observerFallT\": " +
                (float.IsInfinity(cas.fallT) ? "null" : cas.fallT.ToString(Inv)) +
                ",\n");
            sb.Append("  \"samples\": [");
            bool first = true;
            for (float t = (float)vp.t0; t <= (float)vp.t1 + 0.25f; t += 30f)
            {
                var st = SoldierActionResolver.Resolve(
                    ctx, obs.unitIndex, vp.slotId, t);
                var pose = HeroViewpointCamera.Pose(ctx, settings, t);
                if (!first) sb.Append(",");
                first = false;
                sb.Append(string.Format(Inv,
                    "\n    {{\"t\": {0}, \"x\": {1:F2}, \"z\": {2:F2}, " +
                    "\"facingDeg\": {3:F1}, \"clip\": \"{4}\", " +
                    "\"camHeadingDeg\": {5:F1}, \"chaos\": {6}}}",
                    t, st.posX, st.posZ, st.facingDeg, st.clip,
                    pose.headingDeg, pose.chaos01.ToString("F3", Inv)));
            }
            sb.Append("\n  ]\n}\n");
            Directory.CreateDirectory(EvidenceDir(vpId));
            string outPath = Path.Combine(
                EvidenceDir(vpId), $"{vpId}-observer-probe.json");
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"ViewpointRender: wrote {outPath}");
        }

        // ------------------------------------------------------------------
        // Preflight (the P10 gate): whole-window no-teleport over every
        // unit and slot, plus this viewpoint's camera sweep at render rate.
        // ------------------------------------------------------------------
        static void PreflightInner()
        {
            string vpId = RequestedViewpointId();
            var vp = LoadViewpoint(vpId);
            var sw = Stopwatch.StartNew();
            var ctx = Phase10TeleportProbe.CompileContext();
            var settings = GateP9Render.Settings(vp, thirdPerson: false);
            float t0 = (float)vp.t0;
            float tEnd = (float)vp.t1 + PadPastT1;

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
            Debug.Log($"ViewpointRender preflight [{vpId}]: coarse sweep " +
                $"{pairs} pairs, {suspects.Count} suspects, " +
                $"{sw.Elapsed.TotalSeconds:F0} s");

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

            Directory.CreateDirectory(EvidenceDir(vpId));
            var report = new StringBuilder();
            report.Append("{\n");
            report.Append($"  \"viewpoint\": \"{vpId}\",\n");
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
                Path.Combine(EvidenceDir(vpId), $"{vpId}-preflight.json"),
                report.ToString());
            Debug.Log($"ViewpointRender preflight [{vpId}]: " +
                $"{violations.Count} violations, {sw.Elapsed.TotalSeconds:F0} s");
            if (violations.Count > 0)
                throw new InvalidOperationException(
                    $"PREFLIGHT FAIL: {violations.Count} violations; see " +
                    $"{vpId}-preflight.json");
        }

        // ------------------------------------------------------------------
        // Shared frame loop (Phase10Render.RenderFrames, viewpoint-agnostic).
        // ------------------------------------------------------------------
        static float RenderFrames(
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
                    Debug.Log($"ViewpointRender: frame {frame} " +
                        $"({i}/{count} of range, " +
                        $"{sw.Elapsed.TotalSeconds / (i + 1):F2} s/frame)");
                }
            }
            peakManagedMB = Math.Max(peakManagedMB,
                GC.GetTotalMemory(false) / (1024 * 1024));
            return (float)(sw.Elapsed.TotalSeconds / count);
        }

        static string PoseDigest(
            AngleActionContext ctx, HeroCameraSettings settings, float t)
        {
            var p = HeroViewpointCamera.Pose(ctx, settings, t);
            var buf = new List<byte>(44);
            foreach (float v in new[] { p.camX, p.camZ, p.eyeAboveGroundM,
                p.headingDeg, p.pitchDeg, p.rollDeg, p.fovDeg,
                p.obsX, p.obsZ, p.chaos01 })
                buf.AddRange(BitConverter.GetBytes(v));
            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(buf.ToArray()))
                .Replace("-", "").ToLowerInvariant();
        }

        // ------------------------------------------------------------------
        // Determinism pair: two INDEPENDENT stagings of the same 10 s.
        // ------------------------------------------------------------------
        static void DeterminismInner()
        {
            string vpId = RequestedViewpointId();
            var vp = LoadViewpoint(vpId);
            float detT0 = DeterminismT0(vpId);
            int detFrames = 10 * Fps;
            if (detT0 < vp.t0 || detT0 + 10f > vp.t1)
                throw new InvalidOperationException(
                    "determinism window must sit inside the viewpoint window");
            string dirA = Path.Combine(OutRoot(vpId), "det-a");
            string dirB = Path.Combine(OutRoot(vpId), "det-b");
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
                    var freeze = Phase10Render.BuildFreeze(vp, scene.ctx);
                    freezes[pass] = freeze.ToJson();

                    bool warmed = false;
                    long peak = 0;
                    RenderFrames(scene, rt, tex, settings, detT0, 0, detFrames,
                        pass == 0 ? dirA : dirB, ref warmed, ref peak);

                    // logical + camera-pose digests at 10 probe times,
                    // re-posed AFTER scrubbing away (out-of-order probe)
                    digests[pass] = new string[detFrames / 30];
                    for (int i = 0; i < digests[pass].Length; i++)
                    {
                        float away = detT0 + ((i * 30 + 150) % detFrames) / (float)Fps;
                        scene.Pose(away);
                        float t = detT0 + i;
                        scene.Pose(t);
                        digests[pass][i] = scene.LogicalStateDigest(t) + "|" +
                            PoseDigest(scene.ctx, settings, t);
                    }
                }
                finally
                {
                    GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
                }
            }

            bool metadataIdentical = freezes[0] == freezes[1];
            bool digestsIdentical = true;
            for (int i = 0; i < digests[0].Length; i++)
                if (digests[0][i] != digests[1][i]) digestsIdentical = false;

            float worstPct = 0f;
            int worstDelta = 0;
            int worstOutliers = 0;
            var perFrame = new StringBuilder();
            var ta = new Texture2D(2, 2);
            var tb = new Texture2D(2, 2);
            for (int f = 0; f < detFrames; f++)
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

            string evidence = EvidenceDir(vpId);
            Directory.CreateDirectory(evidence);
            foreach (int f in new[] { 0, 150, 299 })
            {
                File.Copy(Path.Combine(dirA, $"frame_{f:D6}.png"),
                    Path.Combine(evidence, $"{vpId}-det-a-{f:D6}.png"), true);
                File.Copy(Path.Combine(dirB, $"frame_{f:D6}.png"),
                    Path.Combine(evidence, $"{vpId}-det-b-{f:D6}.png"), true);
            }
            var rep = new StringBuilder();
            rep.Append("{\n");
            rep.Append($"  \"viewpoint\": \"{vpId}\",\n");
            rep.Append(string.Format(Inv,
                "  \"range\": {{\"t0\": {0}, \"frames\": {1}}},\n",
                detT0, detFrames));
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
                Path.Combine(evidence, $"{vpId}-determinism.json"), rep.ToString());
            File.WriteAllText(
                Path.Combine(evidence, $"{vpId}-freeze.json"), freezes[0]);
            Debug.Log($"ViewpointRender determinism [{vpId}]: " +
                $"metadata={metadataIdentical} digests={digestsIdentical} " +
                $"pixels worst {worstPct:F2}%/{worstDelta}/{worstOutliers}");

            if (!metadataIdentical || !digestsIdentical)
                throw new InvalidOperationException(
                    "GATE FAIL: logical metadata not identical between passes");
            if (!pixelsWithin)
                throw new InvalidOperationException(
                    "GATE FAIL: pixel differences exceed the documented GPU tolerance");
        }

        // ------------------------------------------------------------------
        // Production render: chunked and resumable (Phase10Render pattern).
        // ------------------------------------------------------------------
        static void ProductionInner()
        {
            string vpId = RequestedViewpointId();
            var vp = LoadViewpoint(vpId);
            float t0 = (float)vp.t0;
            float tEnd = (float)vp.t1 + PadPastT1;
            int totalFrames = Mathf.RoundToInt((tEnd - t0) * Fps);
            int framesPerChunk = Mathf.RoundToInt(ChunkSeconds * Fps);
            int chunkCount = (totalFrames + framesPerChunk - 1) / framesPerChunk;

            string outRoot = OutRoot(vpId);
            string seqDir = Path.Combine(outRoot, "seq-full");
            string manifestDir = Path.Combine(outRoot, "manifests");
            Directory.CreateDirectory(seqDir);
            Directory.CreateDirectory(manifestDir);

            var (scene, rt, tex) = GateP9Render.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;
                var freeze = Phase10Render.BuildFreeze(vp, scene.ctx);
                File.WriteAllText(Path.Combine(outRoot, $"{vpId}-freeze.json"),
                    freeze.ToJson());
                Directory.CreateDirectory(EvidenceDir(vpId));
                File.WriteAllText(
                    Path.Combine(EvidenceDir(vpId), $"{vpId}-freeze.json"),
                    freeze.ToJson());

                bool warmed = false;
                var totalSw = Stopwatch.StartNew();
                int rendered = 0, skipped = 0;
                for (int c = 0; c < chunkCount; c++)
                {
                    int frame0 = c * framesPerChunk;
                    int count = Math.Min(framesPerChunk, totalFrames - frame0);
                    string manifestPath = Path.Combine(
                        manifestDir, $"chunk_{c:D3}.json");
                    if (ChunkComplete(outRoot, manifestPath, c, frame0, count))
                    {
                        skipped += count;
                        Debug.Log($"ViewpointRender: chunk {c} complete, skipping");
                        continue;
                    }
                    long peak = 0;
                    float spf = RenderFrames(scene, rt, tex, settings,
                        t0, frame0, count, seqDir, ref warmed, ref peak);
                    rendered += count;
                    WriteChunkManifest(manifestPath, c, frame0, count,
                        t0, freeze, spf, peak);
                    Debug.Log($"ViewpointRender [{vpId}]: chunk {c}/{chunkCount - 1} " +
                        $"done ({spf:F2} s/frame, peak {peak} MB managed)");
                    // native leak mitigation at chunk boundaries; the outer
                    // loop script resumes a SIGKILLed process regardless
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    GC.Collect();
                }
                Debug.Log($"ViewpointRender [{vpId}]: production render complete — " +
                    $"{rendered} rendered, {skipped} resumed-skipped, " +
                    $"{totalSw.Elapsed.TotalHours:F2} h wall");
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        static bool ChunkComplete(string outRoot, string manifestPath,
            int chunk, int frame0, int count)
        {
            if (!File.Exists(manifestPath)) return false;
            if (File.Exists(Path.Combine(
                    outRoot, "chunks", $"chunk_{chunk:D3}.mp4")))
                return true;
            for (int i = 0; i < count; i++)
                if (!File.Exists(Path.Combine(
                        outRoot, "seq-full", $"frame_{frame0 + i:D6}.png")))
                    return false;
            return true;
        }

        static void WriteChunkManifest(
            string path, int chunk, int frame0, int count, float viewT0,
            Phase10Render.Freeze freeze, float secondsPerFrame, long peakManagedMB)
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

        // ------------------------------------------------------------------
        // Representative stills for the evidence dir (force-added to git).
        // ------------------------------------------------------------------
        static (string name, float t)[] StillTimes(string vpId) => vpId switch
        {
            "webb-wall" => new (string, float)[]
            {
                ($"{vpId}-still-8165-line-at-wall", 8165f),
                ($"{vpId}-still-8460-first-fire", 8460f),
                ($"{vpId}-still-8575-charge-closing", 8575f),
                ($"{vpId}-still-8610-wall-crisis", 8610f),
                ($"{vpId}-still-8650-breach-left", 8650f),
                ($"{vpId}-still-8700-fall-back", 8700f),
                ($"{vpId}-still-8790-return", 8790f),
            },
            "cushing-canister" => new (string, float)[]
            {
                ($"{vpId}-still-8405-canister-opens", 8405f),
                ($"{vpId}-still-8520-serving-rhythm", 8520f),
                ($"{vpId}-still-8600-stall-in-front", 8600f),
                ($"{vpId}-still-8650-double-canister", 8650f),
                ($"{vpId}-still-8730-overrun", 8730f),
            },
            _ => throw new InvalidOperationException($"no stills for '{vpId}'"),
        };

        static void StillsInner()
        {
            string vpId = RequestedViewpointId();
            var vp = LoadViewpoint(vpId);
            var (scene, rt, tex) = GateP9Render.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;
                Directory.CreateDirectory(EvidenceDir(vpId));
                bool warmed = false;
                foreach (var (name, t) in StillTimes(vpId))
                {
                    GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                        scene.ctx, settings, t));
                    scene.Pose(t);
                    if (!warmed)
                    {
                        for (int i = 0; i < WarmupFrames; i++)
                            GateP6Render.RenderOnce(scene.camera, rt, null);
                        warmed = true;
                    }
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    string p = Path.Combine(EvidenceDir(vpId), $"{name}.png");
                    File.WriteAllBytes(p, tex.EncodeToPNG());
                    Debug.Log($"ViewpointRender: wrote {p}");
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // ------------------------------------------------------------------
        // Deterministic audio-event export (GateP9Render.AudioEventsInner
        // generalized). Adds observerUnit/observerSide so the stem builder
        // can address a defending observer: the whiz layer keys on ENEMY
        // fire relative to the observer's side, and the strike stem drops
        // its legacy own-unit filter when observerUnit is present.
        // ------------------------------------------------------------------
        static void AudioEventsInner()
        {
            string vpId = RequestedViewpointId();
            var vp = LoadViewpoint(vpId);
            float t0 = (float)vp.t0 - 6f, t1 = (float)vp.t1 + 1f;

            var ctx = Phase10TeleportProbe.CompileContext();
            var obs = ctx.Unit(vp.unitId);
            var settings = GateP9Render.Settings(vp, thirdPerson: false);

            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder(1 << 22);
            sb.Append("{\n");
            sb.Append($"  \"viewpoint\": \"{vp.id}\",\n");
            sb.Append($"  \"observerUnit\": \"{vp.unitId}\",\n");
            sb.Append($"  \"observerSide\": \"{obs.unit.side}\",\n");
            sb.Append($"  \"seed\": \"{ctx.seed}\",\n");
            sb.Append($"  \"bundleChecksum\": \"{ctx.bundle.checksum}\",\n");
            sb.Append(string.Format(inv,
                "  \"window\": {{\"t0\": {0}, \"t1\": {1}}},\n", t0, t1));

            // --- observer track at 10 Hz ---
            sb.Append("  \"observer\": [");
            var footfalls = new List<(float t, ClipId clip)>();
            float prevPhase = -1f;
            ClipId prevClip = ClipId.StandReady;
            bool first = true;
            for (float t = t0; t <= t1 + 1e-3f; t += 0.1f)
            {
                var pose = HeroViewpointCamera.Pose(ctx, settings, t);
                var st = SoldierActionResolver.Resolve(
                    ctx, obs.unitIndex, vp.slotId, t);
                if (!first) sb.Append(",");
                first = false;
                sb.Append(string.Format(inv,
                    "\n    {{\"t\": {0:F2}, \"x\": {1:F2}, \"z\": {2:F2}, " +
                    "\"headingDeg\": {3:F1}, \"chaos\": {4:F3}, " +
                    "\"clip\": \"{5}\", \"crossing\": {6}}}",
                    t, pose.obsX, pose.obsZ, pose.headingDeg, pose.chaos01,
                    st.clip, st.clip == ClipId.Cross ? "true" : "false"));

                if (KitClips.MetersPerCycle(st.clip) > 0f)
                {
                    float phase = st.clipTime / KitClips.Duration(st.clip);
                    if (st.clip == prevClip && prevPhase >= 0f)
                    {
                        foreach (float mark in new[] { 0.25f, 0.75f })
                        {
                            bool crossed = prevPhase < phase
                                ? (prevPhase < mark && phase >= mark)
                                : (prevPhase < mark || phase >= mark);
                            if (crossed) footfalls.Add((t, st.clip));
                        }
                    }
                    prevPhase = phase;
                }
                else prevPhase = -1f;
                prevClip = st.clip;
            }
            sb.Append("\n  ],\n");

            sb.Append("  \"footfalls\": [");
            first = true;
            foreach (var (t, clip) in footfalls)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append(string.Format(inv,
                    "\n    {{\"t\": {0:F2}, \"clip\": \"{1}\"}}", t, clip));
            }
            sb.Append("\n  ],\n");

            // observer + file-neighbor fence/wall crossings (rail rattle).
            // Artillery slots crew fixed positions (no file structure); the
            // neighbor set degenerates gracefully to nearby slot ids.
            sb.Append("  \"crossings\": [");
            first = true;
            int files = FormationRoster.Files(obs.slotCount);
            var neighborSlots = new List<int>();
            for (int d = -6; d <= 6; d++)
            {
                int sameRank = vp.slotId + d;
                if (sameRank >= 0 && sameRank < obs.slotCount)
                    neighborSlots.Add(sameRank);
                foreach (int other in new[]
                    { vp.slotId + files + d, vp.slotId - files + d })
                    if (other >= 0 && other < obs.slotCount)
                        neighborSlots.Add(other);
            }
            neighborSlots.Sort();
            neighborSlots = new List<int>(new SortedSet<int>(neighborSlots));
            foreach (int slot in neighborSlots)
            {
                foreach (float ct in obs.slotCrossings[slot])
                {
                    if (ct < t0 || ct > t1) continue;
                    var st = SoldierActionResolver.Resolve(
                        ctx, obs.unitIndex, slot, ct + 0.1f);
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append(string.Format(inv,
                        "\n    {{\"t\": {0:F2}, \"x\": {1:F2}, \"z\": {2:F2}, " +
                        "\"self\": {3}}}",
                        ct, st.posX, st.posZ,
                        slot == vp.slotId ? "true" : "false"));
                }
            }
            sb.Append("\n  ],\n");

            // --- musket discharges (all units, resolver-confirmed) ---
            sb.Append("  \"musketDischarges\": [");
            first = true;
            int discharges = 0;
            var times = new List<float>();
            foreach (var ur in ctx.units)
            {
                if (ur.isArtillery) continue;
                foreach (var seg in ur.unit.segments)
                {
                    bool fires = FireCycles.IsFireAction(seg.action) ||
                                 seg.action == "breach";
                    if (!fires) continue;
                    for (int slot = 0; slot < ur.slotCount; slot++)
                    {
                        float offset = FireCycles.Offset(
                            ctx.seed, ur.unit.unitId, seg, slot, ur.slotCount);
                        times.Clear();
                        FireCycles.DischargeTimes(
                            seg, offset, ur.casualties[slot].fallT,
                            Mathf.Max(seg.t0, t0 - 3f),
                            Mathf.Min(seg.t1, t1), times);
                        foreach (float ft in times)
                        {
                            var st = SoldierActionResolver.Resolve(
                                ctx, ur.unitIndex, slot, ft);
                            if (st.clip != ClipId.Fire || st.Fallen) continue;
                            if (!first) sb.Append(",");
                            first = false;
                            discharges++;
                            sb.Append(string.Format(inv,
                                "\n    {{\"t\": {0:F2}, \"x\": {1:F2}, " +
                                "\"z\": {2:F2}, \"side\": \"{3}\"}}",
                                ft, st.posX, st.posZ, ur.unit.side));
                        }
                    }
                }
            }
            sb.Append("\n  ],\n");

            // --- cannon discharges (staged batteries) ---
            sb.Append("  \"cannonDischarges\": [");
            first = true;
            foreach (var ur in ctx.units)
            {
                if (!ur.isArtillery || ur.cannonShots == null) continue;
                foreach (var shot in ur.cannonShots)
                {
                    if (shot.t < t0 - 3f || shot.t > t1) continue;
                    float facing = ur.unit.FacingAt(shot.t);
                    Vector2 pos = FormationRoster.ToWorld(
                        ur.unit.PositionAt(shot.t), facing,
                        FormationRoster.GunOffset(shot.gun));
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append(string.Format(inv,
                        "\n    {{\"t\": {0:F2}, \"x\": {1:F2}, \"z\": {2:F2}, " +
                        "\"side\": \"{3}\", \"unitId\": \"{4}\"}}",
                        shot.t, pos.x, pos.y, ur.unit.side, ur.unit.unitId));
                }
            }
            sb.Append("\n  ],\n");

            // --- projectile strikes (all units; python filters by range) ---
            sb.Append("  \"strikes\": [");
            first = true;
            foreach (var ur in ctx.units)
            {
                foreach (var strike in ur.strikes)
                {
                    if (strike.t < t0 || strike.t > t1) continue;
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append(string.Format(inv,
                        "\n    {{\"t\": {0:F2}, \"x\": {1:F2}, \"z\": {2:F2}, " +
                        "\"unitId\": \"{3}\"}}",
                        strike.t, strike.pos.x, strike.pos.y, ur.unit.unitId));
                }
            }
            sb.Append("\n  ],\n");

            // --- casualties within earshot (§9.2 sober) ---
            sb.Append("  \"casualties\": [");
            first = true;
            foreach (var ur in ctx.units)
            {
                for (int slot = 0; slot < ur.slotCount; slot++)
                {
                    var cas = ur.casualties[slot];
                    if (float.IsInfinity(cas.fallT)) continue;
                    if (cas.fallT < t0 || cas.fallT > t1) continue;
                    var st = SoldierActionResolver.Resolve(
                        ctx, ur.unitIndex, slot, cas.fallT);
                    var obsPose = HeroViewpointCamera.Pose(
                        ctx, settings, cas.fallT);
                    float dist = new Vector2(
                        st.posX - obsPose.obsX, st.posZ - obsPose.obsZ).magnitude;
                    if (dist > 120f) continue;
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append(string.Format(inv,
                        "\n    {{\"t\": {0:F2}, \"x\": {1:F2}, \"z\": {2:F2}, " +
                        "\"cause\": \"{3}\", \"crawls\": {4}, \"side\": \"{5}\"}}",
                        cas.fallT, st.posX, st.posZ, cas.cause,
                        cas.woundedCrawl ? "true" : "false", ur.unit.side));
                }
            }
            sb.Append("\n  ],\n");

            // --- observer-unit segments (order/unit-noise cues) ---
            sb.Append("  \"observerSegments\": [");
            first = true;
            foreach (var seg in obs.unit.segments)
            {
                if (seg.t1 < t0 || seg.t0 > t1) continue;
                if (!first) sb.Append(",");
                first = false;
                sb.Append(string.Format(inv,
                    "\n    {{\"id\": \"{0}\", \"t0\": {1}, \"t1\": {2}, " +
                    "\"action\": \"{3}\"}}",
                    seg.id, seg.t0, seg.t1, seg.action));
            }
            sb.Append("\n  ]\n}\n");

            Directory.CreateDirectory(EvidenceDir(vpId));
            string outPath = Path.Combine(
                EvidenceDir(vpId), $"{vpId}-audio-events.json");
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"ViewpointRender: wrote {outPath} " +
                $"({discharges} musket discharges, {footfalls.Count} footfalls)");
        }
    }
}
