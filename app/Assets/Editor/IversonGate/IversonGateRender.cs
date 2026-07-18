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
    // Owner-gate evidence renderer for the SECOND Soldier View film:
    // `iverson-forney-field` (Oak Ridge, July 1 afternoon), riding
    // formation slot 184 of the 12th North Carolina through the staged
    // Iverson bundle (Assets/Battle/Iverson/iverson.bundle.json) on the
    // Oak Ridge crop (data/heightmap_oakridge). Reuses the Gate P9
    // machinery (GateP9Render internals) — only the site, bundle, and
    // windows differ.
    //
    //   RenderCandidates  two 30 s camera-style candidates over the SAME
    //                     window/slot (FP vs close-3P, the P9 owner-choice
    //                     pattern), plus scrub probes + gate report
    //   RenderStills      route stills (advance, the volley, the fight,
    //                     the end state) in both styles at key moments
    //   ExportAudioEvents deterministic event streams for the audio stem
    //                     builder (same script as P9/P10)
    //
    // Usage:
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.IversonGateRender.RenderCandidates
    //     -logFile iv-cand.log
    // Output: docs/benchmarks/captures/iverson-viewpoint/
    public static class IversonGateRender
    {
        const int Width = 2560, Height = 1440;
        const int Fps = 30;
        const int WarmupFrames = 3;

        public const string ViewpointId = "iverson-forney-field";

        // 30 s candidate window at the destruction's peak: the halted line
        // under the wall's massed fire (the falling-curve volley profile,
        // t=6000..6240, is at full intensity; the observer's regiment is
        // firing back; men of the 23rd fall down-line to frame-left).
        public const float CandT0 = 6090f;
        public const float CandT1 = 6120f;

        // Documented GPU pixel tolerance: the Phase 8 measured envelope.
        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;

        static string OutDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../docs/benchmarks/captures/iverson-viewpoint"));

        static string OakridgeCropDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../data/heightmap_oakridge"));

        static string BundlePath => Path.Combine(
            Application.dataPath, "Battle", "Iverson", "iverson.bundle.json");

        public static void RenderCandidates() => Run(CandidatesInner);
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
                Debug.LogError($"IversonGateRender failed: {e}");
                exitCode = 1;
            }
            finally
            {
                AngleEnvironmentStage.CropDirOverride = null;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        internal static ViewpointDefinition LoadViewpoint()
        {
            string path = Path.Combine(
                Application.streamingAssetsPath, "SoldierView/viewpoints.json");
            var set = ViewpointSet.FromJson(File.ReadAllText(path));
            foreach (var vp in set.viewpoints)
                if (vp.id == ViewpointId) return vp;
            throw new InvalidOperationException(
                $"viewpoints.json has no {ViewpointId}");
        }

        // internal: IversonProductionRender boots the same Oak Ridge
        // staging (site + bundle are what distinguish this film's harness)
        internal static (AngleActionScene scene, RenderTexture rt, Texture2D tex)
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

            AngleEnvironmentStage.CropDirOverride = OakridgeCropDir;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var scene = AngleActionScene.StageAll(BundlePath);
            Debug.Log($"IversonGateRender: staged in {sw.Elapsed.TotalSeconds:F1} s " +
                $"({scene.ctx.TotalSlots} slots, bundle {scene.ctx.bundle.checksum.Substring(0, 12)})");
            var rt = new RenderTexture(Width, Height, 32);
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            scene.camera.nearClipPlane = 0.05f;
            scene.lensGuardRadiusM = LensGuard.DefaultRadiusM;
            return (scene, rt, tex);
        }

        // ------------------------------------------------------------------
        static void CandidatesInner()
        {
            var vp = LoadViewpoint();
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                int frames = (int)((CandT1 - CandT0) * Fps);
                int[] probeFrames = { 600, 150 };
                var digests = new Dictionary<int, string>();
                var logicalEqual = new List<bool>();
                var withinTol = new List<bool>();
                var probeStats = new List<string>();
                float fpSecondsPerFrame = 0f;
                long peakMem = 0;

                foreach (var (name, thirdPerson) in new[]
                    { ("fp", false), ("c3p", true) })
                {
                    var settings = GateP9Render.Settings(vp, thirdPerson);
                    scene.hiddenUnitIndex = thirdPerson ? -1 : obs.unitIndex;
                    scene.hiddenSlot = thirdPerson ? -1 : vp.slotId;

                    string seqDir = Path.Combine(OutDir, $"seq-{name}");
                    Directory.CreateDirectory(seqDir);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    bool warmed = false;
                    for (int f = 0; f < frames; f++)
                    {
                        float t = CandT0 + f / (float)Fps;
                        GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
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
                        if (!thirdPerson && Array.IndexOf(probeFrames, f) >= 0)
                            digests[f] = scene.LogicalStateDigest(t) + "|" +
                                PoseDigest(scene.ctx, settings, t);
                        if (f % 150 == 0)
                        {
                            peakMem = Math.Max(peakMem, GC.GetTotalMemory(false));
                            Debug.Log($"IversonGateRender: {name} {f}/{frames} " +
                                $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                        }
                    }
                    if (!thirdPerson)
                        fpSecondsPerFrame = (float)(sw.Elapsed.TotalSeconds / frames);
                    Debug.Log($"IversonGateRender: candidate '{name}' complete " +
                        $"({sw.Elapsed.TotalSeconds / frames:F2} s/frame)");
                }

                // scrub probes against the FP pass (out-of-order re-pose)
                {
                    var settings = GateP9Render.Settings(vp, thirdPerson: false);
                    scene.hiddenUnitIndex = obs.unitIndex;
                    scene.hiddenSlot = vp.slotId;
                    foreach (int f in probeFrames)
                    {
                        float t = CandT0 + f / (float)Fps;
                        float away = CandT0 + ((f + 450) % frames) / (float)Fps;
                        GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                            scene.ctx, settings, away));
                        scene.Pose(away);
                        GateP6Render.RenderOnce(scene.camera, rt, null);

                        GateP9Render.ApplyHeroPose(scene, HeroViewpointCamera.Pose(
                            scene.ctx, settings, t));
                        scene.Pose(t);
                        string digest = scene.LogicalStateDigest(t) + "|" +
                            PoseDigest(scene.ctx, settings, t);
                        bool logical = digest == digests[f];
                        logicalEqual.Add(logical);

                        GateP6Render.RenderOnce(scene.camera, rt, tex);
                        byte[] again = tex.EncodeToPNG();
                        byte[] orig = File.ReadAllBytes(
                            Path.Combine(OutDir, "seq-fp", $"frame_{f:D4}.png"));
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
                        Debug.Log($"IversonGateRender: probe {probeStats[^1]} " +
                            $"withinTolerance={ok}");
                    }
                }

                string report =
                    "{\n" +
                    $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                    $"  \"pipelineAsset\": \"{HdrpMigration.OfflineAssetPath}\",\n" +
                    $"  \"bundleChecksum\": \"{scene.ctx.bundle.checksum}\",\n" +
                    $"  \"battleSeed\": \"{scene.ctx.seed}\",\n" +
                    $"  \"viewpoint\": \"{vp.id}\",\n" +
                    $"  \"totalSlots\": {scene.ctx.TotalSlots},\n" +
                    "  \"cameraStyle\": \"fp + c3p candidates (owner chooses at the gate)\",\n" +
                    $"  \"sequence\": {{\"t0\": {CandT0}, \"t1\": {CandT1}, " +
                    $"\"fps\": {Fps}, \"frames\": {frames}}},\n" +
                    $"  \"fpSecondsPerFrame\": {fpSecondsPerFrame:F2},\n" +
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
                File.WriteAllText(Path.Combine(OutDir, "iverson-gate-report.json"),
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
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // ------------------------------------------------------------------
        static void StillsInner()
        {
            var vp = LoadViewpoint();
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var shots = new (string name, float t, bool thirdPerson)[]
                {
                    ("iv-still-5840-advance-fp", 5840f, false),
                    ("iv-still-5990-volley-opens-fp", 5990f, false),
                    ("iv-still-6100-destruction-fp", 6100f, false),
                    ("iv-still-6100-destruction-c3p", 6100f, true),
                    ("iv-still-6600-fight-fp", 6600f, false),
                    ("iv-still-7040-endstate-fp", 7040f, false),
                    ("iv-still-7040-endstate-c3p", 7040f, true),
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
                    Debug.Log($"IversonGateRender: wrote {p}");
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        internal static string PoseDigest(
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
        // Deterministic event export for the audio stem builder — the same
        // streams GateP9Render exports for the Angle, from the Iverson
        // bundle/site (no rendering needed).
        static void AudioEventsInner() => WriteAudioEvents(OutDir);

        // internal: the production render exports the same streams to its
        // own evidence dir (the gate file stays the gate's evidence)
        internal static void WriteAudioEvents(string outDir)
        {
            var vp = LoadViewpoint();
            float t0 = (float)vp.t0 - 6f, t1 = (float)vp.t1 + 1f;

            AngleEnvironmentStage.CropDirOverride = OakridgeCropDir;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var env = AngleEnvironmentStage.StageAll();
            var bundle = AngleBundleLoader.Load(BundlePath);
            var obstacles = new Dictionary<string, List<Vector2>>();
            foreach (var fence in env.env.fences)
                obstacles[fence.featureId] =
                    AngleEnvironmentData.Points(fence.polylineFlat);
            obstacles[env.env.wall.featureId] =
                AngleEnvironmentData.Points(env.env.wall.polylineFlat);
            var ctx = AngleActionContext.Compile(
                bundle, bundle.StagingSeed, obstacles);

            var obs = ctx.Unit(vp.unitId);
            var settings = GateP9Render.Settings(vp, thirdPerson: false);

            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder(1 << 22);
            sb.Append("{\n");
            sb.Append($"  \"viewpoint\": \"{vp.id}\",\n");
            sb.Append($"  \"seed\": \"{ctx.seed}\",\n");
            sb.Append($"  \"bundleChecksum\": \"{bundle.checksum}\",\n");
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

            // observer + file-neighbor fence crossings (none expected on
            // this site — no fences in the bake; kept for stream parity)
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
                        times.Clear();
                        FireCycles.SegmentDischargeTimes(
                            ctx.seed, ur.unit.unitId, seg, slot, ur.slotCount,
                            ur.casualties[slot].fallT,
                            Mathf.Max(seg.t0, t0 - 3f),
                            Mathf.Min(seg.t1, t1), times);
                        foreach (float ft in times)
                        {
                            var st = SoldierActionResolver.Resolve(
                                ctx, ur.unitIndex, slot, ft);
                            if ((st.clip != ClipId.Fire &&
                                 st.clip != ClipId.FightProneFire) ||
                                st.Fallen) continue;
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

            // --- cannon discharges (none staged on this site; parity) ---
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

            // --- projectile strikes near units ---
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

            // --- casualties within earshot ---
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

            // --- observer-unit segments ---
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

            Directory.CreateDirectory(outDir);
            string outPath = Path.Combine(outDir, "iverson-audio-events.json");
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"IversonGateRender: wrote {outPath} " +
                $"({discharges} musket discharges, {footfalls.Count} footfalls)");
        }
    }
}
