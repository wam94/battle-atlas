using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Gate P9 evidence renderer (plan §12 Phase 9): the hero viewpoint
    // `garnett-road-to-angle` riding formation slot 184 of Garnett's
    // brigade through the full Phase 8 scene, camera pose from
    // HeroViewpointCamera (pure, deterministic, scrub-invariant).
    //
    //   RenderCandidates  two 30 s camera-style candidates over the SAME
    //                     window/slot: first-person and very tight
    //                     over-shoulder close-third-person (the owner
    //                     chooses at gate review, §3.4)
    //   RenderProof       60 s first-person at the wall (t=8610..8670,
    //                     the gate window) + scrub probes, for the mixed
    //                     audio-visual proof (audio is muxed by
    //                     scripts/p9-encode.sh from the stem pipeline)
    //   RenderStills      viewpoint stills along the route + a reduced-
    //                     motion comparison frame
    //   ExportAudioEvents deterministic event streams (fire, cannon,
    //                     strikes, casualties, observer track/footfalls)
    //                     for the synchronized audio stem builder
    //                     (reconstruction/scripts/build_viewpoint_audio.py)
    //
    // Usage:
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.GateP9Render.RenderCandidates
    //     -logFile p9cand.log
    // Output: docs/benchmarks/captures/p9-gate/
    public static class GateP9Render
    {
        const int Width = 2560, Height = 1440;
        const int Fps = 30;
        const int WarmupFrames = 3;

        // 30 s camera-style candidate window (§3.4: owner chooses style):
        // the observer's OWN road crossing. Slot 881 (rear rank, file 184) rides Garnett's left
        // flank, which trails the centroid — his resolved crossings are
        // west fence 8385.2, east fence 8403.5 (episode-window crossing
        // fix), so this take carries seven seconds of approach march,
        // two fence climbs, the road surface between them, and the
        // redress — the regimes a camera style must survive.
        public const float CandT0 = 8378f;
        public const float CandT1 = 8408f;

        // 60 s audio-visual proof window (gate suggestion): Garnett's
        // line under canister at the wall.
        public const float ProofT0 = 8610f;
        public const float ProofT1 = 8670f;

        // Documented GPU pixel tolerance: Phase 8's measured smoke-heavy
        // envelope (see GateP8Render).
        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;

        static string OutDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../docs/benchmarks/captures/p9-gate"));

        public static void RenderCandidates() => Run(CandidatesInner);
        public static void RenderProof() => Run(ProofInner);
        public static void RenderStills() => Run(StillsInner);
        public static void ExportAudioEvents() => Run(AudioEventsInner);

        static void Run(Action inner)
        {
            int exitCode = 0;
            try
            {
                inner();
            }
            catch (Exception e)
            {
                Debug.LogError($"GateP9Render failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        // ------------------------------------------------------------------
        static ViewpointDefinition LoadHeroViewpoint()
        {
            string path = Path.Combine(
                Application.streamingAssetsPath, "SoldierView/viewpoints.json");
            var set = ViewpointSet.FromJson(File.ReadAllText(path));
            foreach (var vp in set.viewpoints)
                if (vp.id == "garnett-road-to-angle") return vp;
            throw new InvalidOperationException(
                "viewpoints.json has no garnett-road-to-angle");
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
            Debug.Log($"GateP9Render: staged in {sw.Elapsed.TotalSeconds:F1} s");
            var rt = new RenderTexture(Width, Height, 32);
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            scene.camera.nearClipPlane = 0.05f;
            return (scene, rt, tex);
        }

        static void Restore(RenderPipelineAsset prevDefault,
            RenderPipelineAsset prevQuality, object prevGlobal)
        {
            var playback = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
                HdrpMigration.PlaybackAssetPath);
            GraphicsSettings.defaultRenderPipeline =
                playback != null ? playback : prevDefault;
            QualitySettings.renderPipeline = prevQuality;
            GateP6Render.CurrentGlobalSettingsProp.SetValue(null, prevGlobal);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // Apply a HeroCameraPose to the scene camera. Ground is sampled at
        // the observer's smoothed position (±0.75 m along heading averaged,
        // so a 1 m DEM texel edge cannot pop the eye line).
        static void ApplyHeroPose(AngleActionScene scene, HeroCameraPose pose)
        {
            float x0 = scene.cropX0, z0 = scene.cropZ0;
            var terrain = scene.terrain;
            float Ground(float mx, float mz) =>
                terrain.transform.position.y + terrain.SampleHeight(
                    new Vector3(mx - x0, 0f, mz - z0));
            float hr = pose.headingDeg * Mathf.Deg2Rad;
            float fx = Mathf.Sin(hr), fz = Mathf.Cos(hr);
            float g = (Ground(pose.groundRefX, pose.groundRefZ)
                + Ground(pose.groundRefX + 0.75f * fx, pose.groundRefZ + 0.75f * fz)
                + Ground(pose.groundRefX - 0.75f * fx, pose.groundRefZ - 0.75f * fz)) / 3f;
            scene.camera.transform.position = new Vector3(
                pose.camX - x0, g + pose.eyeAboveGroundM, pose.camZ - z0);
            scene.camera.transform.rotation = Quaternion.Euler(
                pose.pitchDeg, pose.headingDeg, pose.rollDeg);
            scene.camera.fieldOfView = pose.fovDeg;
        }

        static HeroCameraSettings Settings(
            ViewpointDefinition vp, bool thirdPerson) =>
            HeroCameraSettings.FromViewpoint(
                vp, thirdPerson, HeroMotionProfile.Standard);

        // ------------------------------------------------------------------
        static void CandidatesInner()
        {
            var vp = LoadHeroViewpoint();
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                foreach (var (name, thirdPerson) in new[]
                    { ("fp", false), ("c3p", true) })
                {
                    var settings = Settings(vp, thirdPerson);
                    // first person: the camera is the observer's eyes —
                    // his own figure is hidden (see AngleActionScene)
                    scene.hiddenUnitIndex = thirdPerson ? -1 : obs.unitIndex;
                    scene.hiddenSlot = thirdPerson ? -1 : vp.slotId;

                    string seqDir = Path.Combine(OutDir, $"seq-{name}");
                    Directory.CreateDirectory(seqDir);
                    int frames = (int)((CandT1 - CandT0) * Fps);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    bool warmed = false;
                    for (int f = 0; f < frames; f++)
                    {
                        float t = CandT0 + f / (float)Fps;
                        // camera BEFORE pose: smoke billboards face it
                        ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                            scene.ctx, settings, t));
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
                        if (f % 150 == 0)
                            Debug.Log($"GateP9Render: {name} {f}/{frames} " +
                                $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                    }
                    Debug.Log($"GateP9Render: candidate '{name}' complete " +
                        $"({sw.Elapsed.TotalSeconds / frames:F2} s/frame)");
                }
            }
            finally
            {
                Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // ------------------------------------------------------------------
        static void ProofInner()
        {
            var vp = LoadHeroViewpoint();
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;

                string seqDir = Path.Combine(OutDir, "seq-proof");
                Directory.CreateDirectory(seqDir);
                int frames = (int)((ProofT1 - ProofT0) * Fps);
                int[] probeFrames = { 1500, 300 };
                var digests = new Dictionary<int, string>();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long peakMem = 0;
                bool warmed = false;
                for (int f = 0; f < frames; f++)
                {
                    float t = ProofT0 + f / (float)Fps;
                    ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                        scene.ctx, settings, t));
                    scene.Pose(t);
                    if (!warmed)
                    {
                        for (int i = 0; i < WarmupFrames; i++)
                            GateP6Render.RenderOnce(scene.camera, rt, null);
                        warmed = true;
                    }
                    GateP6Render.RenderOnce(scene.camera, rt, tex);
                    File.WriteAllBytes(
                        Path.Combine(seqDir, $"frame_{f:D4}.png"),
                        tex.EncodeToPNG());
                    if (Array.IndexOf(probeFrames, f) >= 0)
                        digests[f] = scene.LogicalStateDigest(t) + "|" +
                            PoseDigest(scene.ctx, settings, t);
                    if (f % 150 == 0)
                    {
                        peakMem = Math.Max(peakMem, GC.GetTotalMemory(false));
                        Debug.Log($"GateP9Render: proof {f}/{frames} " +
                            $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                    }
                }
                float secondsPerFrame = (float)(sw.Elapsed.TotalSeconds / frames);

                // scrub probes: out-of-order re-pose (logical + camera pose
                // must be bitwise identical; pixels within GPU tolerance)
                var logicalEqual = new List<bool>();
                var withinTol = new List<bool>();
                var probeStats = new List<string>();
                foreach (int f in probeFrames)
                {
                    float t = ProofT0 + f / (float)Fps;
                    float away = ProofT0 + ((f + 900) % frames) / (float)Fps;
                    ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                        scene.ctx, settings, away));
                    scene.Pose(away);
                    GateP6Render.RenderOnce(scene.camera, rt, null);

                    ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                        scene.ctx, settings, t));
                    scene.Pose(t);
                    string digest = scene.LogicalStateDigest(t) + "|" +
                        PoseDigest(scene.ctx, settings, t);
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
                    probeStats.Add($"frame {f}: logicalAndPoseBitwiseEqual=" +
                        $"{logical} differingPixels={pct:F2}% " +
                        $"maxChannelDelta={maxd}");
                    Debug.Log($"GateP9Render: probe {probeStats[^1]} " +
                        $"withinTolerance={ok}");
                }

                string report =
                    "{\n" +
                    $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                    $"  \"pipelineAsset\": \"{HdrpMigration.OfflineAssetPath}\",\n" +
                    $"  \"bundleChecksum\": \"{scene.ctx.bundle.checksum}\",\n" +
                    $"  \"battleSeed\": \"{scene.ctx.seed}\",\n" +
                    $"  \"viewpoint\": \"{vp.id}\",\n" +
                    "  \"cameraStyle\": \"first_person (proof label; " +
                    "candidates pending owner choice)\",\n" +
                    $"  \"sequence\": {{\"t0\": {ProofT0}, \"t1\": {ProofT1}, " +
                    $"\"fps\": {Fps}, \"frames\": {frames}}},\n" +
                    $"  \"secondsPerFrame\": {secondsPerFrame:F2},\n" +
                    $"  \"peakManagedMemoryMB\": {peakMem / (1024 * 1024)},\n" +
                    $"  \"probeFrames\": [{string.Join(",", probeFrames)}],\n" +
                    "  \"probesLogicalAndPoseBitwiseEqual\": " +
                    $"[{string.Join(",", logicalEqual.ConvertAll(b => b ? "true" : "false"))}],\n" +
                    "  \"probesWithinPixelTolerance\": " +
                    $"[{string.Join(",", withinTol.ConvertAll(b => b ? "true" : "false"))}],\n" +
                    $"  \"probeStats\": [{string.Join(", ", probeStats.ConvertAll(s => $"\"{s}\""))}],\n" +
                    $"  \"pixelToleranceMaxChannelDelta\": {TolMaxDelta},\n" +
                    $"  \"pixelToleranceDifferingPct\": {TolDiffPct}\n" +
                    "}\n";
                File.WriteAllText(Path.Combine(OutDir, "p9-gate-report.json"),
                    report);

                if (logicalEqual.Contains(false))
                    throw new InvalidOperationException(
                        "GATE FAIL: logical/camera state not bitwise-identical under scrub");
                if (withinTol.Contains(false))
                    throw new InvalidOperationException(
                        "GATE FAIL: scrub probes exceeded the documented GPU tolerance");
            }
            finally
            {
                Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // Camera-pose digest: proves the hero camera itself reconstructs
        // bitwise under scrub, not just the soldier states behind it.
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
        static void StillsInner()
        {
            var vp = LoadHeroViewpoint();
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var shots = new (string name, float t, bool thirdPerson, bool reduced)[]
                {
                    ("p9-still-8165-advance-fp", 8165f, false, false),
                    ("p9-still-8387-west-fence-fp", 8387f, false, false),
                    ("p9-still-8405-east-fence-fp", 8405f, false, false),
                    ("p9-still-8500-advance-fp", 8500f, false, false),
                    ("p9-still-8620-canister-fp", 8620f, false, false),
                    ("p9-still-8760-repulse-fp", 8760f, false, false),
                    ("p9-still-8405-east-fence-c3p", 8405f, true, false),
                    ("p9-still-8620-canister-c3p", 8620f, true, false),
                    ("p9-still-8620-canister-fp-reduced", 8620f, false, true),
                };
                bool warmed = false;
                foreach (var shot in shots)
                {
                    var settings = HeroCameraSettings.FromViewpoint(
                        vp, shot.thirdPerson,
                        shot.reduced ? HeroMotionProfile.ReducedMotion
                                     : HeroMotionProfile.Standard);
                    scene.hiddenUnitIndex = shot.thirdPerson ? -1 : obs.unitIndex;
                    scene.hiddenSlot = shot.thirdPerson ? -1 : vp.slotId;
                    ApplyHeroPose(scene, HeroViewpointCamera.Pose(
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
                    Debug.Log($"GateP9Render: wrote {p}");
                }
            }
            finally
            {
                Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // ------------------------------------------------------------------
        // Deterministic event export for the audio stem builder. Everything
        // here is the SAME compiled data the visuals render from: fire
        // cycles, cannon schedules, strike scatter, casualty schedule, and
        // the hero camera's own smoothed track. JSON is gitignored,
        // regenerated by this method (documented in the audio runbook).
        static void AudioEventsInner()
        {
            var vp = LoadHeroViewpoint();
            // full viewpoint window + lead-in so distance-delayed reports
            // born before t0 still arrive correctly
            float t0 = (float)vp.t0 - 6f, t1 = (float)vp.t1 + 1f;

            // no rendering needed: compile the context exactly as the
            // stage does (same seed = bundle checksum, same obstacles)
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var env = AngleEnvironmentStage.StageAll();
            var bundle = AngleBundleLoader.Load();
            var obstacles = new Dictionary<string, List<Vector2>>();
            foreach (var fence in env.env.fences)
                obstacles[fence.featureId] =
                    AngleEnvironmentData.Points(fence.polylineFlat);
            obstacles[env.env.wall.featureId] =
                AngleEnvironmentData.Points(env.env.wall.polylineFlat);
            var ctx = AngleActionContext.Compile(
                bundle, bundle.checksum, obstacles);

            var obs = ctx.Unit(vp.unitId);
            var settings = Settings(vp, thirdPerson: false);

            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder(1 << 22);
            sb.Append("{\n");
            sb.Append($"  \"viewpoint\": \"{vp.id}\",\n");
            sb.Append($"  \"seed\": \"{ctx.seed}\",\n");
            sb.Append($"  \"bundleChecksum\": \"{bundle.checksum}\",\n");
            sb.Append(string.Format(inv,
                "  \"window\": {{\"t0\": {0}, \"t1\": {1}}},\n", t0, t1));

            // --- observer track at 10 Hz: position, heading, chaos, gait ---
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

                // footfalls: two per locomotion cycle at phase 0.25/0.75
                if (KitClips.MetersPerCycle(st.clip) > 0f)
                {
                    float phase = st.clipTime / KitClips.Duration(st.clip);
                    if (st.clip == prevClip && prevPhase >= 0f)
                    {
                        foreach (float mark in new[] { 0.25f, 0.75f })
                        {
                            bool crossed = prevPhase < phase
                                ? (prevPhase < mark && phase >= mark)
                                : (prevPhase < mark || phase >= mark); // wrap
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

            // observer + file-neighbor fence crossings (rail rattle)
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
            // a slot can appear twice if slotId±files overlap same-rank ids
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

            // --- cannon discharges (staged batteries; all Union here) ---
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

            // --- projectile strikes near the observer's unit ---
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

            // --- casualties within earshot (wounded voices, §9.2 sober) ---
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

            Directory.CreateDirectory(OutDir);
            string outPath = Path.Combine(OutDir, "p9-audio-events.json");
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"GateP9Render: wrote {outPath} " +
                $"({discharges} musket discharges, {footfalls.Count} footfalls)");
        }
    }
}
