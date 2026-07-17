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
    // ANGLE-V2 DATA WAVE — the film-invalidating production render
    // (docs/reconstruction/angle-v2-data.md; owner authorization: "angle
    // film v2 authorized").
    //
    // Renders the SAME garnett-road-to-angle viewpoint (rear-rank slot
    // 881 — the owner has not chosen a new framing) over the same padded
    // window t=8160..8820.5, from the v2 bundle (checksum 03590b14…,
    // stagingSeed d470c469… held): in-window melee, colors succession,
    // the mounted-officer fall, and the strike-correlated casualty
    // bursts. The P10 pattern is followed verbatim (preflight sweep,
    // determinism pair, resumable 60 s chunks, chunk manifests, freeze
    // record) but EVERY output lands under app/RenderOutput/angle-v2/
    // and docs/benchmarks/captures/angle-v2-data/ — the shipped v1 media
    // and the p10 gate evidence stay untouched on disk.
    //
    //   Preflight              whole-window no-teleport sweep over the
    //                          NEW compiled states (melee step-ins, the
    //                          burst schedule, the split segments).
    //   RenderDeterminismPair  two independent stagings of t=8400..8410.
    //   RenderProduction       the full chunked resumable render.
    //   RenderFrontRankProof   P7 comparison: 30 s at the climax
    //                          (t=8640..8670) from front-rank slot 184
    //                          AND from the v1 rear-rank slot 881, so the
    //                          owner can choose rear vs front framing.
    //
    // Usage:
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.AngleV2Render.<Method> \
    //     -logFile <log>
    // ------------------------------------------------------------------
    public static class AngleV2Render
    {
        const int Width = 2560, Height = 1440, Fps = 30;
        public const float PadPastT1 = Phase10Render.PadPastT1;
        const float ChunkSeconds = 60f;

        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;
        const int MaxOutlierPixels = 8;

        const float MaxDeltaPerFrameM = 8f / Fps;
        const float MaxCrossExitDeltaM = SoldierActionResolver.CrossTravelM + 8f / Fps;
        const float MaxCameraDeltaPerFrameM = 4.5f / Fps;

        // P7 comparison-proof window: the climax (the melee/breach/burst
        // minute — cas spike peak, Armistead's crossing underway).
        const float ProofT0 = 8640f;
        const float ProofDur = 30f;
        const int FrontRankSlot = 184;   // front rank of file 184 (§6.5)

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static string RepoRoot => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../.."));
        static string OutRoot => Path.Combine(RepoRoot, "app/RenderOutput/angle-v2");
        static string SeqDir => Path.Combine(OutRoot, "seq-full");
        static string ManifestDir => Path.Combine(OutRoot, "manifests");
        static string EvidenceDir => Path.Combine(
            RepoRoot, "docs/benchmarks/captures/angle-v2-data");

        public static void Preflight() => Run(PreflightInner);
        public static void RenderDeterminismPair() => Run(DeterminismInner);
        public static void RenderProduction() => Run(ProductionInner);
        public static void RenderFrontRankProof() => Run(FrontRankProofInner);
        public static void DumpVocabEvidence() => Run(DumpVocabEvidenceInner);
        public static void ProbeCameraYaw() => Run(ProbeCameraYawInner);

        // Diagnostic: fine-grained camera pose components around a failing
        // comfort-bound window (writes angle-v2-yaw-probe.csv).
        static void ProbeCameraYawInner()
        {
            var ctx = Phase10TeleportProbe.CompileContext();
            var vp = Phase10TeleportProbe.LoadHeroViewpoint();
            var settings = GateP9Render.Settings(vp, thirdPerson: false);
            var sb = new StringBuilder("t,yaw,chaos,obsFacing\n");
            for (float t = 8684f; t <= 8702f; t += 0.05f)
            {
                var pose = HeroViewpointCamera.Pose(ctx, settings, t);
                var st = SoldierActionResolver.Resolve(
                    ctx, ctx.Unit(vp.unitId).unitIndex, vp.slotId, t);
                sb.Append(string.Format(Inv, "{0:F2},{1:F3},{2:F3},{3:F3}\n",
                    t, pose.headingDeg, pose.chaos01, st.facingDeg));
            }
            Directory.CreateDirectory(EvidenceDir);
            File.WriteAllText(
                Path.Combine(EvidenceDir, "angle-v2-yaw-probe.csv"),
                sb.ToString());
            Debug.Log("AngleV2Render: wrote angle-v2-yaw-probe.csv");
        }

        // ------------------------------------------------------------------
        // Machine evidence for the report: the C#-side strike times per CSA
        // unit (cross-checked against the compiler's Python replication),
        // the colors-succession timeline on the pinned seed, the mounted
        // officer's arc, and the melee pair census. No rendering.
        static void DumpVocabEvidenceInner()
        {
            var ctx = Phase10TeleportProbe.CompileContext();
            Directory.CreateDirectory(EvidenceDir);
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append($"  \"bundleChecksum\": \"{ctx.bundle.checksum}\",\n");
            sb.Append($"  \"seed\": \"{ctx.seed}\",\n");

            sb.Append("  \"strikeTimes\": {\n");
            bool firstU = true;
            foreach (var ur in ctx.units)
            {
                if (ur.strikes.Count == 0) continue;
                if (!firstU) sb.Append(",\n");
                firstU = false;
                sb.Append($"    \"{ur.unit.unitId}\": [");
                for (int i = 0; i < ur.strikes.Count; i++)
                    sb.Append((i > 0 ? "," : "") + string.Format(
                        Inv, "{0:R}", ur.strikes[i].t));
                sb.Append("]");
            }
            sb.Append("\n  },\n");

            var fry = ctx.Unit("csa-fry");
            sb.Append("  \"colorsTimeline\": [\n");
            ColorGuard.Phase lastPhase = (ColorGuard.Phase)255;
            int lastBearer = -2;
            bool firstC = true;
            for (float t = 8160f; t <= 8820f; t += 1f)
            {
                var cs = ColorGuard.StateAt(ctx, fry, t);
                if (cs.phase == lastPhase && cs.bearerSlot == lastBearer)
                    continue;
                lastPhase = cs.phase;
                lastBearer = cs.bearerSlot;
                if (!firstC) sb.Append(",\n");
                firstC = false;
                sb.Append(string.Format(Inv,
                    "    {{\"t\": {0}, \"phase\": \"{1}\", \"bearerSlot\": {2}}}",
                    t, cs.phase, cs.bearerSlot));
            }
            sb.Append("\n  ],\n");

            var garnett = ctx.Unit("csa-garnett");
            var spec = garnett.unit.mountedOfficers[0];
            sb.Append("  \"mountedOfficer\": [\n");
            bool firstM = true;
            foreach (float t in new[] { 8160f, 8400f, 8560f, 8579f, 8581f,
                8583f, 8586f, 8600f, 8620f, 8700f })
            {
                var ms = MountedOfficer.Resolve(ctx, spec, t);
                if (!firstM) sb.Append(",\n");
                firstM = false;
                sb.Append(string.Format(Inv,
                    "    {{\"t\": {0}, \"horseClip\": \"{1}\", \"riderClip\": " +
                    "\"{2}\", \"riderDown\": {3}, \"horseVisible\": {4}, " +
                    "\"x\": {5:F1}, \"z\": {6:F1}",
                    t, ms.horseClip, ms.riderClip,
                    ms.riderDown ? "true" : "false",
                    ms.horseVisible ? "true" : "false", ms.posX, ms.posZ));
                sb.Append("}");
            }
            sb.Append("\n  ],\n");

            // melee pair census at mid-melee
            var seg = garnett.unit.SegmentAt(8676f);
            int pairs = 0;
            float maxReach = 0f;
            for (int slot = 0; slot < FormationRoster.Files(garnett.slotCount); slot++)
            {
                if (!MeleeChoreo.TryPair(ctx, garnett, seg, slot, 8676f, out var pr))
                    continue;
                pairs++;
                // roster position an instant before the clinch locks
                var pre = SoldierActionResolver.Resolve(
                    ctx, garnett.unitIndex, slot, pr.t0 - 0.05f);
                maxReach = Mathf.Max(maxReach,
                    (pr.anchor - new Vector2(pre.posX, pre.posZ)).magnitude);
            }
            sb.Append(string.Format(Inv,
                "  \"meleePairs\": {{\"garnettPairsAt8676\": {0}, " +
                "\"maxStepInM\": {1:F2}", pairs, maxReach));
            sb.Append("},\n");

            // burst census: falls per second at the wall fight
            var falls = new Dictionary<int, int>();
            foreach (var e in garnett.casualties)
            {
                if (float.IsInfinity(e.fallT)) continue;
                if (e.fallT < 8580f || e.fallT > 8700f) continue;
                int s = (int)e.fallT;
                falls[s] = falls.TryGetValue(s, out int c) ? c + 1 : 1;
            }
            int maxPerSecond = 0;
            foreach (var kv in falls) maxPerSecond = Math.Max(maxPerSecond, kv.Value);
            sb.Append(string.Format(Inv,
                "  \"wallFightBursts\": {{\"maxFallsInOneSecond\": {0}}}\n",
                maxPerSecond));
            sb.Append("}\n");
            File.WriteAllText(
                Path.Combine(EvidenceDir, "angle-v2-vocab-evidence.json"),
                sb.ToString());
            Debug.Log("AngleV2Render: wrote angle-v2-vocab-evidence.json");
        }

        static void Run(Action inner)
        {
            int exitCode = 0;
            try { inner(); }
            catch (Exception e)
            {
                Debug.LogError($"AngleV2Render failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        // ------------------------------------------------------------------
        static void PreflightInner()
        {
            var sw = Stopwatch.StartNew();
            var ctx = Phase10TeleportProbe.CompileContext();
            var vp = Phase10TeleportProbe.LoadHeroViewpoint();
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
            Debug.Log($"AngleV2Render preflight: coarse sweep {pairs} pairs, " +
                $"{suspects.Count} suspect windows, {sw.Elapsed.TotalSeconds:F0} s");

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

            Directory.CreateDirectory(EvidenceDir);
            var report = new StringBuilder();
            report.Append("{\n");
            report.Append($"  \"bundleChecksum\": \"{ctx.bundle.checksum}\",\n");
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
            report.Append("  \"violations\": [");
            for (int i = 0; i < violations.Count; i++)
                report.Append((i > 0 ? "," : "") + "\n    \"" + violations[i] + "\"");
            report.Append(violations.Count > 0 ? "\n  ],\n" : "],\n");
            report.Append($"  \"pass\": {(violations.Count == 0 ? "true" : "false")},\n");
            report.Append(string.Format(Inv,
                "  \"sweepSeconds\": {0:F0}\n", sw.Elapsed.TotalSeconds));
            report.Append("}\n");
            File.WriteAllText(
                Path.Combine(EvidenceDir, "angle-v2-preflight.json"),
                report.ToString());
            Debug.Log($"AngleV2Render preflight: {violations.Count} violations, " +
                $"total {sw.Elapsed.TotalSeconds:F0} s");
            if (violations.Count > 0)
                throw new InvalidOperationException(
                    $"PREFLIGHT FAIL: {violations.Count} per-frame teleport " +
                    "violations; see angle-v2-preflight.json");
        }

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
                    var freeze = Phase10Render.BuildFreeze(vp, scene.ctx);
                    freezes[pass] = freeze.ToJson();

                    bool warmed = false;
                    long peak = 0;
                    Phase10Render.RenderFrames(scene, rt, tex, settings, DetT0,
                        0, DetFrames, pass == 0 ? dirA : dirB, ref warmed, ref peak);

                    digests[pass] = new string[DetFrames / 30];
                    for (int i = 0; i < digests[pass].Length; i++)
                        digests[pass][i] = scene.LogicalStateDigest(DetT0 + i);
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
            int worstDelta = 0, worstOutliers = 0;
            var ta = new Texture2D(2, 2);
            var tb = new Texture2D(2, 2);
            for (int f = 0; f < DetFrames; f++)
            {
                ta.LoadImage(File.ReadAllBytes(Path.Combine(dirA, $"frame_{f:D6}.png")));
                tb.LoadImage(File.ReadAllBytes(Path.Combine(dirB, $"frame_{f:D6}.png")));
                var pa = ta.GetPixels32();
                var pb = tb.GetPixels32();
                long ndiff = 0;
                int maxd = 0, outliers = 0;
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
            }
            bool pixelsWithin = worstPct <= TolDiffPct &&
                worstOutliers <= MaxOutlierPixels;

            Directory.CreateDirectory(EvidenceDir);
            foreach (int f in new[] { 0, 150, 299 })
            {
                File.Copy(Path.Combine(dirA, $"frame_{f:D6}.png"),
                    Path.Combine(EvidenceDir, $"av2-det-a-{f:D6}.png"), true);
                File.Copy(Path.Combine(dirB, $"frame_{f:D6}.png"),
                    Path.Combine(EvidenceDir, $"av2-det-b-{f:D6}.png"), true);
            }
            var rep = new StringBuilder();
            rep.Append("{\n");
            rep.Append(string.Format(Inv,
                "  \"range\": {{\"t0\": {0}, \"frames\": {1}}},\n", DetT0, DetFrames));
            rep.Append($"  \"freezeMetadataIdentical\": {(metadataIdentical ? "true" : "false")},\n");
            rep.Append($"  \"logicalDigestsIdentical\": {(digestsIdentical ? "true" : "false")},\n");
            rep.Append(string.Format(Inv,
                "  \"worstDifferingPct\": {0:F2}, \"worstMaxChannelDelta\": {1}, " +
                "\"worstOutlierPixels\": {2},\n", worstPct, worstDelta, worstOutliers));
            rep.Append($"  \"pixelsWithinTolerance\": {(pixelsWithin ? "true" : "false")}\n");
            rep.Append("}\n");
            File.WriteAllText(Path.Combine(EvidenceDir, "angle-v2-determinism.json"),
                rep.ToString());
            File.WriteAllText(Path.Combine(EvidenceDir, "angle-v2-freeze.json"),
                freezes[0]);
            Debug.Log($"AngleV2Render determinism: metadata={metadataIdentical} " +
                $"digests={digestsIdentical} pixels worst {worstPct:F2}%/{worstDelta}" +
                $"/{worstOutliers} outliers");

            if (!metadataIdentical || !digestsIdentical)
                throw new InvalidOperationException(
                    "GATE FAIL: logical metadata not identical between passes");
            if (!pixelsWithin)
                throw new InvalidOperationException(
                    "GATE FAIL: pixel differences exceed the documented GPU tolerance");
        }

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
                var freeze = Phase10Render.BuildFreeze(vp, scene.ctx);
                File.WriteAllText(Path.Combine(OutRoot, "angle-v2-freeze.json"),
                    freeze.ToJson());
                Directory.CreateDirectory(EvidenceDir);
                File.WriteAllText(Path.Combine(EvidenceDir, "angle-v2-freeze.json"),
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
                        Debug.Log($"AngleV2Render: chunk {c} complete, skipping");
                        continue;
                    }
                    long peak = 0;
                    float spf = Phase10Render.RenderFrames(scene, rt, tex, settings,
                        t0, frame0, count, SeqDir, ref warmed, ref peak);
                    rendered += count;
                    Phase10Render.WriteChunkManifest(manifestPath, c, frame0, count,
                        t0, freeze, spf, peak);
                    Debug.Log($"AngleV2Render: chunk {c}/{chunkCount - 1} done " +
                        $"({spf:F2} s/frame, peak {peak} MB managed)");
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    GC.Collect();
                }
                Debug.Log($"AngleV2Render: production render complete — " +
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
        // P7 comparison proof: the same 30 climax seconds from the front
        // rank (slot 184) and from the shipped rear rank (slot 881), so
        // the owner can choose the v2 framing with pixels, not prose.
        static void FrontRankProofInner()
        {
            var vpRear = Phase10TeleportProbe.LoadHeroViewpoint();
            var (scene, rt, tex) = GateP9Render.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vpRear.unitId);

                // ED-22 protects only committed observers (slot 881). The
                // front-rank PROOF slot is not protected: verify it lives
                // through the proof window; fail loudly if the schedule
                // fells it (pick a different file, do not silently move).
                float fallT = obs.casualties[FrontRankSlot].fallT;
                if (fallT <= ProofT0 + ProofDur + PadPastT1)
                    throw new InvalidOperationException(
                        $"front-rank proof slot {FrontRankSlot} falls at " +
                        $"t={fallT:F1} inside the proof window — choose a " +
                        "neighboring surviving front-rank file");

                foreach (var (tag, slot) in new[]
                    { ("front-184", FrontRankSlot), ("rear-881", vpRear.slotId) })
                {
                    // clone the committed viewpoint, re-slot it
                    var vp = JsonUtility.FromJson<ViewpointDefinition>(
                        JsonUtility.ToJson(vpRear));
                    vp.slotId = slot;
                    var settings = GateP9Render.Settings(vp, thirdPerson: false);
                    scene.hiddenUnitIndex = obs.unitIndex;
                    scene.hiddenSlot = slot;

                    string dir = Path.Combine(OutRoot, $"p7-{tag}");
                    bool warmed = false;
                    long peak = 0;
                    int frames = (int)(ProofDur * Fps);
                    float spf = Phase10Render.RenderFrames(scene, rt, tex,
                        settings, ProofT0, 0, frames, dir, ref warmed, ref peak);
                    Debug.Log($"AngleV2Render: P7 {tag} done ({spf:F2} s/frame)");

                    Directory.CreateDirectory(EvidenceDir);
                    foreach (int f in new[] { 60, 450, 840 })
                        File.Copy(Path.Combine(dir, $"frame_{f:D6}.png"),
                            Path.Combine(EvidenceDir,
                                $"av2-p7-{tag}-t{ProofT0 + f / (float)Fps:F0}.png"),
                            true);
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }
    }
}
