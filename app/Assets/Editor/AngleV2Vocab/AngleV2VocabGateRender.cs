using System;
using System.Collections.Generic;
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
    // Angle-v2 vocabulary gate evidence (FightProneGateRender pattern):
    // renders the staged demo bundle
    // (reconstruction/scripts/stage_angle_v2_vocab.py) on the REAL Angle
    // crop — the wall fight (P3 melee), the colors falling and passing
    // (P4, with the staff/flag staged as a scene prop), a mounted
    // officer falling (P5, horse + rider staged by this harness per the
    // owner-approved vocabulary), and a halt-and-fire line at the traced
    // post-and-rail fence (P6).
    //
    //   RenderSequenceWall   30 s, 30 fps, 2560x1440 at the wall/colors
    //   RenderSequenceFence  30 s at the halt-and-fire fence
    //   RenderStills         the committed still set
    //   RenderHorseSweep     per-clip frames for the horse rig
    //
    // Usage:
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.AngleV2VocabGateRender.RenderSequenceWall
    //     -logFile av2-wall.log
    public static class AngleV2VocabGateRender
    {
        const int Width = 2560, Height = 1440;
        const int Fps = 30;
        const int WarmupFrames = 3;

        const int TolMaxDelta = 12;
        const float TolDiffPct = 8f;

        // the mounted officer (P5) — harness-staged; ANONYMOUS by
        // construction (V&R §1; proposed ED-81)
        static MountedOfficerSpec OfficerSpec => new MountedOfficerSpec
        {
            officerId = "demo-officer-1",
            unitId = "demo-csa-colors",
            fallT = 14f,
            backOffsetM = 6f,
            alongOffsetM = 3f,
        };

        static string OutDir => Path.GetFullPath(Path.Combine(
            Application.dataPath, "../../docs/benchmarks/captures/angle-v2-vocab"));

        static string DemoBundlePath => Path.Combine(
            OutDir, "angle-v2-vocab-demo.bundle.json");

        public static void RenderSequenceWall() =>
            Run(() => SequenceInner("wall", 2f, 32f, WallCamera));
        public static void RenderSequenceFence() =>
            Run(() => SequenceInner("fence", 6f, 36f, FenceCamera));
        public static void RenderStills() => Run(StillsInner);
        public static void RenderHorseSweep() => Run(HorseSweepInner);

        static void Run(Action inner)
        {
            int exitCode = 0;
            try
            {
                inner();
            }
            catch (Exception e)
            {
                Debug.LogError($"AngleV2VocabGateRender failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        // ------------------------------------------------------------------
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

            // the REAL Angle crop (no override) — the point of the demo
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var scene = AngleActionScene.StageAll(DemoBundlePath);
            Debug.Log($"AngleV2VocabGateRender: staged {scene.ctx.TotalSlots} " +
                $"slots, bundle {scene.ctx.bundle.checksum.Substring(0, 12)}");
            var rt = new RenderTexture(Width, Height, 32);
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            scene.camera.nearClipPlane = 0.05f;
            return (scene, rt, tex);
        }

        static float Ground(AngleActionScene scene, Vector2 macro) =>
            scene.terrain.transform.position.y + scene.terrain.SampleHeight(
                new Vector3(macro.x - scene.cropX0, 0f, macro.y - scene.cropZ0));

        static Vector3 World(AngleActionScene scene, Vector2 macro, float up) =>
            new Vector3(macro.x - scene.cropX0,
                Ground(scene, macro) + up, macro.y - scene.cropZ0);

        static void AimCamera(
            AngleActionScene scene, Vector2 camPos, Vector2 lookAt,
            float eye = 1.66f, float lookUp = 1.1f, float fov = 55f)
        {
            var cam = scene.camera;
            cam.transform.position = World(scene, camPos, eye);
            cam.transform.rotation = Quaternion.LookRotation(
                World(scene, lookAt, lookUp) - cam.transform.position,
                Vector3.up);
            cam.fieldOfView = fov;
        }

        static void WallCamera(AngleActionScene scene) =>
            // south of the line, colors party frame-left, the wall fight
            // frame-right
            AimCamera(scene, new Vector2(4368f, 4838f),
                new Vector2(4392f, 4866f));

        static void FenceCamera(AngleActionScene scene) =>
            AimCamera(scene, new Vector2(4126f, 4882f),
                new Vector2(4143f, 4901f));

        // ------------------------------------------------------------------
        // Harness extras: the colors (staff+flag prop) and the mounted
        // officer (horse + rider). Deterministic functions of t.
        class Extras
        {
            public AngleActionScene scene;
            public UnitRuntime colorsUr;
            public GameObject staff;
            public GameObject horse, rider;
            public Dictionary<string, AnimationClip> horseClips, riderClips;

            public void Pose(float t)
            {
                PoseColors(t);
                PoseMounted(t);
            }

            void PoseColors(float t)
            {
                var cs = ColorGuard.StateAt(scene.ctx, colorsUr, t);
                var pos = new Vector2(cs.posX, cs.posZ);
                float facing = colorsUr.unit.FacingAt(t);
                float fr = facing * Mathf.Deg2Rad;
                var fwd3 = new Vector3(Mathf.Sin(fr), 0f, Mathf.Cos(fr));

                // tilt: 0 = staff vertical, 90 = on the ground
                float tilt;
                switch (cs.phase)
                {
                    case ColorGuard.Phase.Carried:
                        tilt = 4f;
                        break;
                    case ColorGuard.Phase.BearerFalling:
                        tilt = Mathf.Lerp(4f, 88f, Mathf.Clamp01(
                            (t - cs.sinceT) / ColorGuard.FallDur));
                        break;
                    case ColorGuard.Phase.Raising:
                        tilt = Mathf.Lerp(88f, 4f, Mathf.Clamp01(
                            (t - cs.sinceT) / ColorGuard.RaiseDur));
                        break;
                    default:   // Grounded / Down
                        tilt = 88f;
                        break;
                }
                staff.transform.position = World(scene, pos, 0.05f);
                staff.transform.rotation =
                    Quaternion.LookRotation(fwd3, Vector3.up) *
                    Quaternion.Euler(tilt, 0f, 0f);
            }

            void PoseMounted(float t)
            {
                var s = MountedOfficer.Resolve(scene.ctx, OfficerSpec, t);
                horse.SetActive(s.horseVisible);
                if (s.horseVisible)
                {
                    horse.transform.position = World(
                        scene, new Vector2(s.posX, s.posZ), 0f);
                    horse.transform.rotation =
                        Quaternion.Euler(0f, s.facingDeg, 0f);
                    var clip = horseClips[HorseClips.Name(s.horseClip)];
                    clip.SampleAnimation(horse,
                        Mathf.Min(s.horseClipTime, clip.length));
                }
                rider.transform.position = World(
                    scene, new Vector2(s.riderPosX, s.riderPosZ), 0f);
                rider.transform.rotation =
                    Quaternion.Euler(0f, s.riderFacingDeg, 0f);
                var rclip = riderClips[KitClips.Name(s.riderClip)];
                rclip.SampleAnimation(rider,
                    Mathf.Min(s.riderClipTime, rclip.length));
            }
        }

        static Extras BuildExtras(AngleActionScene scene)
        {
            var ex = new Extras
            {
                scene = scene,
                colorsUr = scene.ctx.Unit("demo-csa-colors"),
            };

            // staff + flag prop (project-owned primitives; the flag is a
            // plain dark-red cloth — no unit identification is implied)
            ex.staff = new GameObject("V2 Colors Prop");
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "staff";
            pole.transform.SetParent(ex.staff.transform, false);
            pole.transform.localScale = new Vector3(0.03f, 1.3f, 0.03f);
            pole.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            var cloth = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cloth.name = "flag";
            cloth.transform.SetParent(ex.staff.transform, false);
            cloth.transform.localPosition = new Vector3(0.55f, 2.15f, 0f);
            cloth.transform.localScale = new Vector3(1.1f, 0.85f, 1f);
            cloth.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var staffMat = new Material(Shader.Find("HDRP/Lit"));
            staffMat.color = new Color(0.35f, 0.24f, 0.13f);
            pole.GetComponent<MeshRenderer>().sharedMaterial = staffMat;
            var clothMat = new Material(Shader.Find("HDRP/Lit"));
            clothMat.color = new Color(0.45f, 0.10f, 0.10f);
            clothMat.SetFloat("_CullMode", 0f);
            cloth.GetComponent<MeshRenderer>().sharedMaterial = clothMat;

            // the horse and its rider
            ex.horse = GateP6Render.SpawnVariant("horse");
            ex.horseClips = GateP6Render.LoadClips("horse");
            ex.rider = GateP6Render.SpawnVariant("csa_b");
            ex.riderClips = GateP6Render.LoadClips("csa_b");
            return ex;
        }

        // ------------------------------------------------------------------
        static void SequenceInner(
            string tag, float t0, float t1, Action<AngleActionScene> placeCam)
        {
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var extras = BuildExtras(scene);
                placeCam(scene);
                int frames = (int)((t1 - t0) * Fps);
                string seqDir = Path.Combine(OutDir, $"seq-{tag}");
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
                    float t = t0 + f / (float)Fps;
                    scene.Pose(t);
                    extras.Pose(t);
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
                        Debug.Log($"AngleV2VocabGateRender[{tag}]: {f}/{frames} " +
                            $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                }
                float secondsPerFrame = (float)(sw.Elapsed.TotalSeconds / frames);

                foreach (int f in probeFrames)
                {
                    float t = t0 + f / (float)Fps;
                    float away = t0 + ((f + 450) % frames) / (float)Fps;
                    scene.Pose(away);
                    extras.Pose(away);
                    GateP6Render.RenderOnce(scene.camera, rt, null);
                    scene.Pose(t);
                    extras.Pose(t);
                    bool logical = scene.LogicalStateDigest(t) == digests[f];
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
                    probeStats.Add($"frame {f}: logicalBitwiseEqual={logical} " +
                        $"differingPixels={pct:F2}% maxChannelDelta={maxd}");
                    Debug.Log($"AngleV2VocabGateRender[{tag}]: probe {probeStats[^1]}");
                }

                // the colors timeline (P4 evidence: the succession as it
                // played on camera)
                var colorsLog = new StringBuilder();
                if (tag == "wall")
                {
                    var ur = scene.ctx.Unit("demo-csa-colors");
                    for (float t = t0; t <= t1; t += 1f)
                    {
                        var cs = ColorGuard.StateAt(scene.ctx, ur, t);
                        colorsLog.Append(
                            $"    \"t={t:F0} {cs.phase} bearer={cs.bearerSlot}\",\n");
                    }
                }

                string report =
                    "{\n" +
                    $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                    $"  \"bundleChecksum\": \"{scene.ctx.bundle.checksum}\",\n" +
                    $"  \"battleSeed\": \"{scene.ctx.seed}\",\n" +
                    $"  \"totalSlots\": {scene.ctx.TotalSlots},\n" +
                    $"  \"sequence\": {{\"tag\": \"{tag}\", \"t0\": {t0}, " +
                    $"\"t1\": {t1}, \"fps\": {Fps}, \"frames\": {frames}}},\n" +
                    $"  \"secondsPerFrame\": {secondsPerFrame:F2},\n" +
                    $"  \"probeFrames\": [{string.Join(",", probeFrames)}],\n" +
                    "  \"probesLogicalBitwiseEqual\": " +
                    $"[{string.Join(",", logicalEqual.ConvertAll(b => b ? "true" : "false"))}],\n" +
                    "  \"probesWithinPixelTolerance\": " +
                    $"[{string.Join(",", withinTol.ConvertAll(b => b ? "true" : "false"))}],\n" +
                    $"  \"probeStats\": [{string.Join(", ", probeStats.ConvertAll(s => $"\"{s}\""))}],\n" +
                    $"  \"pixelToleranceMaxChannelDelta\": {TolMaxDelta},\n" +
                    $"  \"pixelToleranceDifferingPct\": {TolDiffPct}" +
                    (colorsLog.Length > 0
                        ? ",\n  \"colorsTimeline\": [\n" +
                          colorsLog.ToString().TrimEnd('\n', ',') + "\n  ]\n"
                        : "\n") +
                    "}\n";
                File.WriteAllText(Path.Combine(
                    OutDir, $"angle-v2-vocab-gate-report-{tag}.json"), report);

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

        // ------------------------------------------------------------------
        static void StillsInner()
        {
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var extras = BuildExtras(scene);
                var shots = new (string name, float t, Action<AngleActionScene> cam)[]
                {
                    // the wall/colors arc
                    ("av2-wall-04-carry-and-clinch", 4f, WallCamera),
                    ("av2-wall-10-bearer-down", 10f, WallCamera),
                    ("av2-wall-16-colors-passing", 16f, WallCamera),
                    ("av2-wall-27-carried-on", 27f, WallCamera),
                    // the wall fight, close
                    ("av2-melee-20-closeup", 20f, s => AimCamera(s,
                        new Vector2(4394f, 4884f), new Vector2(4402f, 4868f))),
                    ("av2-melee-28-closeup", 28f, s => AimCamera(s,
                        new Vector2(4394f, 4884f), new Vector2(4402f, 4868f))),
                    // the mounted officer (falls at t=14)
                    ("av2-mounted-10-riding", 10f, s => AimCamera(s,
                        new Vector2(4342f, 4852f), new Vector2(4352f, 4869f))),
                    ("av2-mounted-15-rear", 15f, s => AimCamera(s,
                        new Vector2(4342f, 4852f), new Vector2(4352f, 4869f))),
                    ("av2-mounted-17-rider-down", 17f, s => AimCamera(s,
                        new Vector2(4342f, 4852f), new Vector2(4352f, 4869f))),
                    ("av2-mounted-24-riderless", 24f, s => AimCamera(s,
                        new Vector2(4342f, 4852f), new Vector2(4348f, 4866f),
                        fov: 62f)),
                    // the halt-and-fire fence
                    ("av2-fence-08-advancing", 8f, FenceCamera),
                    ("av2-fence-12-halt-dress", 12f, FenceCamera),
                    ("av2-fence-24-firing-at-the-fence", 24f, FenceCamera),
                    ("av2-fence-36-crossing-resumes", 36f, FenceCamera),
                };
                bool warmed = false;
                foreach (var shot in shots)
                {
                    shot.cam(scene);
                    scene.Pose(shot.t);
                    extras.Pose(shot.t);
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
                    Debug.Log($"AngleV2VocabGateRender: wrote {p}");
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        // ------------------------------------------------------------------
        // Horse-rig clip sweep (the human kit sweep lives in
        // ReloadStageDiag.RenderClipSweep).
        static void HorseSweepInner()
        {
            var (scene, rt, tex) = Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                string dir = Path.Combine(OutDir, "clip-sweep");
                Directory.CreateDirectory(dir);
                var horse = GateP6Render.SpawnVariant("horse");
                var clips = GateP6Render.LoadClips("horse");
                Vector2 spot = new Vector2(4352f, 4866f);
                horse.transform.position = World(scene, spot, 0f);
                horse.transform.rotation = Quaternion.Euler(0f, 130f, 0f);
                AimCamera(scene, spot + new Vector2(-4.5f, -5.5f), spot,
                    eye: 1.9f, lookUp: 1.2f);
                bool warmed = false;
                foreach (var kv in clips)
                {
                    var clip = kv.Value;
                    float[] phases = { 0f, 0.4f, 0.7f, 0.999f };
                    for (int i = 0; i < phases.Length; i++)
                    {
                        float t = clip.length * phases[i];
                        clip.SampleAnimation(horse, t);
                        if (!warmed)
                        {
                            for (int k = 0; k < WarmupFrames; k++)
                                GateP6Render.RenderOnce(scene.camera, rt, null);
                            warmed = true;
                        }
                        GateP6Render.RenderOnce(scene.camera, rt, null);
                        GateP6Render.RenderOnce(scene.camera, rt, tex);
                        string p = Path.Combine(dir,
                            $"sweep_horse_{kv.Key}_{i}_t{t:00.00}.png");
                        File.WriteAllBytes(p, tex.EncodeToPNG());
                        Debug.Log($"AngleV2VocabGateRender: wrote {p}");
                    }
                }
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }
    }
}
