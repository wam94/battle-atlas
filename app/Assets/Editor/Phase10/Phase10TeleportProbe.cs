using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    // Phase 10 must-fix probe (Gate P9 carry-over): the owner saw a visible
    // TELEPORT at ~25 s into p9-proof-60s-av-fp-1440p.mp4 (battle t~8635).
    // This probe compiles the deterministic action context (no rendering)
    // and dumps per-frame slot motion around the defect window so the
    // root cause is machine evidence, not pixels.
    //
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.Phase10TeleportProbe.Probe
    //     -logFile p10probe.log
    public static class Phase10TeleportProbe
    {
        const int Fps = 30;
        // Focused window around the reported defect.
        const float T0 = 8615f;
        const float T1 = 8655f;
        // Global bound: a per-frame planar delta above this is faster than
        // a sprint (8 m/s / 30 fps = 0.27 m; headroom for catch-up blends).
        const float SpikeM = 0.34f;
        // Near the camera even a fast walk reads as a teleport: flag lower.
        const float NearSpikeM = 0.12f;   // 3.6 m/s
        const float NearRadiusM = 40f;
        // Any slot that passes this close to the lens gets a full trace.
        const float LensRadiusM = 3f;

        static string OutDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../docs/benchmarks/captures/p10-gate"));

        public static void Probe()
        {
            int exit = 0;
            try { ProbeInner(); }
            catch (Exception e) { Debug.LogError($"Phase10TeleportProbe: {e}"); exit = 1; }
            if (Application.isBatchMode) EditorApplication.Exit(exit);
        }

        internal static AngleActionContext CompileContext()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var env = AngleEnvironmentStage.StageAll();
            var bundle = AngleBundleLoader.Load();
            var obstacles = new Dictionary<string, List<Vector2>>();
            foreach (var fence in env.env.fences)
                obstacles[fence.featureId] =
                    AngleEnvironmentData.Points(fence.polylineFlat);
            obstacles[env.env.wall.featureId] =
                AngleEnvironmentData.Points(env.env.wall.polylineFlat);
            return AngleActionContext.Compile(bundle, bundle.checksum, obstacles);
        }

        internal static ViewpointDefinition LoadHeroViewpoint()
        {
            string path = Path.Combine(
                Application.streamingAssetsPath, "SoldierView/viewpoints.json");
            var set = ViewpointSet.FromJson(File.ReadAllText(path));
            foreach (var vp in set.viewpoints)
                if (vp.id == "garnett-road-to-angle") return vp;
            throw new InvalidOperationException("no garnett-road-to-angle");
        }

        static string F(float v, int digits = 3) =>
            v.ToString("F" + digits, CultureInfo.InvariantCulture);

        static void ProbeInner()
        {
            var ctx = CompileContext();
            var vp = LoadHeroViewpoint();
            var settings = HeroCameraSettings.FromViewpoint(
                vp, thirdPerson: false, HeroMotionProfile.Standard);

            int frames = (int)((T1 - T0) * Fps) + 1;

            // camera track (also reused for distance tests below)
            var camXZ = new Vector2[frames];
            var sb = new StringBuilder(1 << 22);
            sb.Append("{\n");
            sb.Append($"  \"window\": {{\"t0\": {F(T0, 1)}, \"t1\": {F(T1, 1)}, \"fps\": {Fps}}},\n");
            sb.Append("  \"camera\": [");
            float camMax = 0f;
            for (int f = 0; f < frames; f++)
            {
                float t = T0 + f / (float)Fps;
                var pose = HeroViewpointCamera.Pose(ctx, settings, t);
                camXZ[f] = new Vector2(pose.camX, pose.camZ);
                float d = f == 0 ? 0f : (camXZ[f] - camXZ[f - 1]).magnitude;
                camMax = Mathf.Max(camMax, d);
                if (f > 0) sb.Append(",");
                sb.Append("\n    {\"t\": " + F(t) + ", \"x\": " + F(pose.camX) +
                    ", \"z\": " + F(pose.camZ) + ", \"eye\": " + F(pose.eyeAboveGroundM) +
                    ", \"heading\": " + F(pose.headingDeg, 2) + ", \"delta\": " + F(d) + "}");
            }
            sb.Append("\n  ],\n");
            sb.Append("  \"cameraMaxDeltaM\": " + F(camMax, 4) + ",\n");

            // --- sweep every slot of every unit ---
            var spikes = new List<(UnitRuntime ur, int slot, float t, float d, float dCam)>();
            var lensSlots = new List<(UnitRuntime ur, int slot, float minDist)>();
            foreach (var ur in ctx.units)
            {
                for (int slot = 0; slot < ur.slotCount; slot++)
                {
                    Vector2 prev = Vector2.zero;
                    float minLens = float.MaxValue;
                    for (int f = 0; f < frames; f++)
                    {
                        float t = T0 + f / (float)Fps;
                        var st = SoldierActionResolver.Resolve(
                            ctx, ur.unitIndex, slot, t);
                        var p = new Vector2(st.posX, st.posZ);
                        float dCam = (p - camXZ[f]).magnitude;
                        minLens = Mathf.Min(minLens, dCam);
                        if (f > 0)
                        {
                            float d = (p - prev).magnitude;
                            float bound = dCam < NearRadiusM ? NearSpikeM : SpikeM;
                            if (d > bound)
                                spikes.Add((ur, slot, t, d, dCam));
                        }
                        prev = p;
                    }
                    if (minLens < LensRadiusM &&
                        !(ur.unit.unitId == vp.unitId && slot == vp.slotId))
                        lensSlots.Add((ur, slot, minLens));
                }
            }
            spikes.Sort((a, b) => b.d.CompareTo(a.d));
            lensSlots.Sort((a, b) => a.minDist.CompareTo(b.minDist));
            Debug.Log($"Phase10TeleportProbe: {spikes.Count} spike pairs, " +
                $"{lensSlots.Count} lens-proximity slots in t={T0}..{T1}");

            sb.Append("  \"spikes\": [");
            bool first = true;
            int emitted = 0;
            foreach (var (ur, slot, t, d, dCam) in spikes)
            {
                if (emitted++ >= 400) break;
                if (!first) sb.Append(",");
                first = false;
                var st = SoldierActionResolver.Resolve(ctx, ur.unitIndex, slot, t);
                var cross = ur.slotCrossings[slot];
                var crossStr = new StringBuilder("[");
                for (int i = 0; i < cross.Length; i++)
                {
                    if (i > 0) crossStr.Append(",");
                    crossStr.Append(F(cross[i], 1));
                }
                crossStr.Append("]");
                float fallT = ur.casualties[slot].fallT;
                sb.Append("\n    {\"unit\": \"" + ur.unit.unitId + "\", \"slot\": " +
                    slot + ", \"t\": " + F(t) + ", \"deltaM\": " + F(d) +
                    ", \"clip\": \"" + st.clip + "\", \"status\": " + st.status +
                    ", \"distToCameraM\": " + F(dCam, 2) +
                    ", \"crossings\": " + crossStr +
                    ", \"fallT\": " + (float.IsInfinity(fallT) ? "null" : F(fallT, 1)) + "}");
            }
            sb.Append("\n  ],\n");
            sb.Append($"  \"spikeCount\": {spikes.Count},\n");

            // --- full traces: lens-proximity slots + observer file column ---
            var traced = new HashSet<string>();
            var traceList = new List<(UnitRuntime ur, int slot, string why)>();
            foreach (var (ur, slot, minDist) in lensSlots)
            {
                if (traceList.Count >= 8) break;
                if (traced.Add(ur.unit.unitId + "#" + slot))
                    traceList.Add((ur, slot, "lens " + F(minDist, 2) + " m"));
            }
            var obsUr = ctx.Unit(vp.unitId);
            int files = FormationRoster.Files(obsUr.slotCount);
            foreach (int s in new[] { vp.slotId, vp.slotId - files, vp.slotId + files })
                if (s >= 0 && s < obsUr.slotCount &&
                    traced.Add(obsUr.unit.unitId + "#" + s))
                    traceList.Add((obsUr, s, "observer file"));

            sb.Append("  \"traces\": [");
            first = true;
            foreach (var (ur, slot, why) in traceList)
            {
                if (!first) sb.Append(",");
                first = false;
                float fallT = ur.casualties[slot].fallT;
                var cross = ur.slotCrossings[slot];
                var crossStr = new StringBuilder("[");
                for (int i = 0; i < cross.Length; i++)
                {
                    if (i > 0) crossStr.Append(",");
                    crossStr.Append(F(cross[i], 1));
                }
                crossStr.Append("]");
                sb.Append("\n    {\"unit\": \"" + ur.unit.unitId + "\", \"slot\": " +
                    slot + ", \"why\": \"" + why + "\", \"fallT\": " +
                    (float.IsInfinity(fallT) ? "null" : F(fallT, 1)) +
                    ", \"crossings\": " + crossStr + ", \"frames\": [");
                Vector2 prev = Vector2.zero;
                for (int f = 0; f < frames; f++)
                {
                    float t = T0 + f / (float)Fps;
                    var st = SoldierActionResolver.Resolve(
                        ctx, ur.unitIndex, slot, t);
                    var p = new Vector2(st.posX, st.posZ);
                    float d = f == 0 ? 0f : (p - prev).magnitude;
                    float dCam = (p - camXZ[f]).magnitude;
                    prev = p;
                    if (f > 0) sb.Append(",");
                    sb.Append("\n      {\"t\": " + F(t) + ", \"x\": " + F(st.posX) +
                        ", \"z\": " + F(st.posZ) + ", \"d\": " + F(d) +
                        ", \"dCam\": " + F(dCam, 2) + ", \"clip\": \"" + st.clip +
                        "\", \"ct\": " + F(st.clipTime, 2) + "}");
                }
                sb.Append("\n    ]}");
            }
            sb.Append("\n  ]\n}\n");

            Directory.CreateDirectory(OutDir);
            string outPath = Path.Combine(OutDir, "teleport-probe.json");
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log($"Phase10TeleportProbe: wrote {outPath}");
        }
    }
}
