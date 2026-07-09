using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Gate P6 evidence renderer (plan §12 Phase 6 gate): a 60-second
    // eye-level sequence with 100 kit soldiers over the staged Angle
    // terrain, under the OFFLINE HDRP profile.
    //
    // Everything on screen is a pure function of the frame's battle time:
    // GateP6Choreography.Resolve(slot, t) picks clip + clip time, and
    // AnimationClip.SampleAnimation applies it — no Animator playback, no
    // _Time-driven shaders, no unseeded Random. Determinism evidence: after
    // the forward pass, probe frames are re-rendered OUT OF ORDER and
    // byte-compared against the sequence frames (scrub invariance).
    //
    // Usage (Unity closed on this checkout; worktree runs are fine):
    //   Unity -batchmode -projectPath app -buildTarget OSXUniversal
    //     -executeMethod BattleAtlas.EditorTools.GateP6Render.RenderStills
    //     -executeMethod BattleAtlas.EditorTools.GateP6Render.RenderSequence
    // Output: docs/benchmarks/captures/p6-gate/...
    public static class GateP6Render
    {
        const int Width = 2560, Height = 1440;   // §6.5 offline target
        const int Fps = 30;
        const int WarmupFrames = 3;
        const string KitDir = "Assets/ProjectOwned/Characters/Kit";

        // The kit figures import facing Unity -Z at identity (Blender -Y
        // forward through the default exporter axes); choreography facing 0
        // means +Z north, so +180 yaw — verified in the stills pass (faces
        // showed toward a camera that should have seen backs at fixup 0).
        const float FacingFixup = 180f;

        static readonly string[] CsaVariants = { "csa_a", "csa_b", "csa_c" };
        static readonly string[] UnionVariants = { "union_a", "union_b", "union_c" };

        public static void RenderStills()
        {
            Run(stillsOnly: true);
        }

        public static void RenderSequence()
        {
            Run(stillsOnly: false);
        }

        static void Run(bool stillsOnly)
        {
            int exitCode = 0;
            try
            {
                RunInner(stillsOnly);
            }
            catch (Exception e)
            {
                Debug.LogError($"GateP6Render failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        class Trooper
        {
            public GameObject go;
            public Dictionary<string, AnimationClip> clips;
        }

        static void RunInner(bool stillsOnly)
        {
            string outDir = Path.GetFullPath(Path.Combine(
                Application.dataPath, "../../docs/benchmarks/captures/p6-gate"));
            Directory.CreateDirectory(outDir);

            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(
                HdrpMigration.OfflineAssetPath);
            if (offline == null)
                throw new InvalidOperationException(
                    $"{HdrpMigration.OfflineAssetPath} missing");

            RenderPipelineAsset prevDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset prevQuality = QualitySettings.renderPipeline;
            object prevGlobal = CurrentGlobalSettingsProp.GetValue(null);
            GraphicsSettings.defaultRenderPipeline = offline;
            QualitySettings.renderPipeline = offline;
            BindHdrpGlobalSettings();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var stage = AngleBakeoffStage.Stage(BakeoffPipeline.Hdrp);
                // the bake-off's instanced capsule figures give way to the kit
                UnityEngine.Object.DestroyImmediate(stage.figures.gameObject);

                Terrain terrain = stage.terrain;
                float Ground(Vector2 xz) =>
                    terrain.transform.position.y +
                    terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y));

                // camera per choreography constants
                Camera cam = stage.camera;
                var camPos = GateP6Choreography.CameraPosXZ;
                var camLook = GateP6Choreography.CameraLookXZ;
                cam.transform.position = new Vector3(
                    camPos.x, Ground(camPos) + GateP6Choreography.CameraEyeHeight, camPos.y);
                cam.transform.LookAt(new Vector3(
                    camLook.x, Ground(camLook) + GateP6Choreography.CameraLookHeight, camLook.y));
                cam.fieldOfView = GateP6Choreography.CameraFovDeg;
                cam.nearClipPlane = 0.05f;

                var troopers = Spawn();

                var rt = new RenderTexture(Width, Height, 32);
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

                PoseAll(troopers, 0f, Ground);
                for (int i = 0; i < WarmupFrames; i++) RenderOnce(cam, rt, null);
                if (!(RenderPipelineManager.currentPipeline is HDRenderPipeline))
                    throw new InvalidOperationException(
                        "HDRP did not construct; run with -buildTarget OSXUniversal");

                if (stillsOnly)
                {
                    float[] stillTimes = { 0f, 10f, 21f, 30f, 38f, 42f, 47f, 52f, 58f };
                    foreach (float t in stillTimes)
                    {
                        PoseAll(troopers, t, Ground);
                        RenderOnce(cam, rt, tex);
                        string p = Path.Combine(outDir, $"still_t{t:00.0}.png");
                        File.WriteAllBytes(p, tex.EncodeToPNG());
                        Debug.Log($"GateP6Render: wrote {p}");
                    }
                    return;
                }

                string seqDir = Path.Combine(outDir, "seq");
                Directory.CreateDirectory(seqDir);
                int frames = (int)(GateP6Choreography.Duration * Fps);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                for (int f = 0; f < frames; f++)
                {
                    float t = f / (float)Fps;
                    PoseAll(troopers, t, Ground);
                    RenderOnce(cam, rt, tex);
                    File.WriteAllBytes(
                        Path.Combine(seqDir, $"frame_{f:D4}.png"), tex.EncodeToPNG());
                    if (f % 150 == 0)
                        Debug.Log($"GateP6Render: frame {f}/{frames} " +
                                  $"({sw.Elapsed.TotalSeconds / (f + 1):F2} s/frame)");
                }
                sw.Stop();

                // determinism probes: re-render OUT OF ORDER, byte-compare
                var report = new Report
                {
                    unityVersion = Application.unityVersion,
                    pipelineAsset = HdrpMigration.OfflineAssetPath,
                    fps = Fps,
                    frames = frames,
                    secondsPerFrame = (float)(sw.Elapsed.TotalSeconds / frames),
                    peakMemoryMB = SystemInfo.systemMemorySize,
                };
                int[] probes = { 1500, 300, 900 };   // deliberately unordered
                var equal = new List<bool>();
                foreach (int f in probes)
                {
                    PoseAll(troopers, f / (float)Fps, Ground);
                    RenderOnce(cam, rt, tex);
                    byte[] again = tex.EncodeToPNG();
                    byte[] orig = File.ReadAllBytes(
                        Path.Combine(seqDir, $"frame_{f:D4}.png"));
                    bool eq = again.SequenceEqual(orig);
                    equal.Add(eq);
                    Debug.Log($"GateP6Render: probe frame {f} scrub-equal={eq} " +
                              $"sha_orig={Sha(orig).Substring(0, 12)} sha_again={Sha(again).Substring(0, 12)}");
                }
                report.probeFrames = probes;
                report.probesEqual = equal.ToArray();
                File.WriteAllText(Path.Combine(outDir, "p6-gate-report.json"),
                    JsonUtility.ToJson(report, true));
                Debug.Log($"GateP6Render: sequence complete, " +
                          $"{report.secondsPerFrame:F2} s/frame, probes equal: " +
                          string.Join(",", equal));
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline = prevDefault;
                QualitySettings.renderPipeline = prevQuality;
                CurrentGlobalSettingsProp.SetValue(null, prevGlobal);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        [Serializable]
        class Report
        {
            public string unityVersion;
            public string pipelineAsset;
            public int fps;
            public int frames;
            public float secondsPerFrame;
            public int peakMemoryMB;
            public int[] probeFrames;
            public bool[] probesEqual;
        }

        static List<Trooper> Spawn()
        {
            var prefabs = new Dictionary<string, (GameObject prefab, Dictionary<string, AnimationClip> clips)>();
            foreach (var name in CsaVariants.Concat(UnionVariants))
            {
                string path = $"{KitDir}/{name}.fbx";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    throw new InvalidOperationException($"{path} not imported");
                // Blender FBX takes import as "<rig>|<action>" — index by
                // the action name
                var clips = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .Where(c => !c.name.StartsWith("__preview"))
                    .GroupBy(c => c.name.Contains("|")
                        ? c.name.Substring(c.name.LastIndexOf('|') + 1)
                        : c.name)
                    .ToDictionary(g => g.Key, g => g.First());
                prefabs[name] = (prefab, clips);
            }

            var troopers = new List<Trooper>();
            for (int slot = 0; slot < GateP6Choreography.SoldierCount; slot++)
            {
                bool usa = GateP6Choreography.IsUsa(slot);
                string name = (usa ? UnionVariants : CsaVariants)
                    [GateP6Choreography.VariantIndex(slot)];
                var (prefab, clips) = prefabs[name];
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = $"soldier_{slot:D3}_{name}";
                var animator = go.GetComponent<Animator>();
                if (animator != null) animator.enabled = false;
                RemapMaterials(go);
                troopers.Add(new Trooper { go = go, clips = clips });
            }
            return troopers;
        }

        static void PoseAll(List<Trooper> troopers, float t, Func<Vector2, float> ground)
        {
            for (int slot = 0; slot < troopers.Count; slot++)
            {
                var pose = GateP6Choreography.Resolve(slot, t);
                var tr = troopers[slot];
                if (!tr.clips.TryGetValue(pose.clip, out var clip))
                    throw new InvalidOperationException(
                        $"slot {slot}: clip '{pose.clip}' missing from {tr.go.name}");
                tr.go.transform.position = new Vector3(
                    pose.posLocal.x, ground(pose.posLocal), pose.posLocal.y);
                tr.go.transform.rotation = Quaternion.Euler(
                    0f, pose.facingDeg + FacingFixup, 0f);
                clip.SampleAnimation(tr.go, Mathf.Min(pose.clipTime, clip.length));
            }
        }

        // FBX-imported materials arrive on the built-in Standard shader
        // (white under HDRP) but keep their authored diffuse (as sRGB).
        // Rebuild each as HDRP/Lit with the linearized color and a
        // material-family smoothness. Cached by name so instances share.
        static readonly Dictionary<string, Material> RemappedMats = new();

        static void RemapMaterials(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    string key = mats[i].name;
                    if (!RemappedMats.TryGetValue(key, out var m))
                    {
                        Color c = mats[i].HasProperty("_BaseColor")
                            ? mats[i].GetColor("_BaseColor")
                            : mats[i].HasProperty("_Color") ? mats[i].color : Color.gray;
                        m = new Material(Shader.Find("HDRP/Lit"));
                        m.name = $"p6_{key}";
                        m.SetColor("_BaseColor", c.linear);
                        float smooth = 0.06f;
                        float metal = 0f;
                        if (key.Contains("leather")) smooth = 0.30f;
                        else if (key.Contains("steel")) { smooth = 0.45f; metal = 0.85f; }
                        else if (key.Contains("brass")) { smooth = 0.40f; metal = 0.80f; }
                        else if (key.Contains("skin")) smooth = 0.25f;
                        else if (key.Contains("walnut")) smooth = 0.35f;
                        else if (key.Contains("canteen")) smooth = 0.20f;
                        m.SetFloat("_Smoothness", smooth);
                        m.SetFloat("_Metallic", metal);
                        RemappedMats[key] = m;
                    }
                    mats[i] = m;
                }
                r.sharedMaterials = mats;
            }
        }

        static string Sha(byte[] b)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(b)).Replace("-", "").ToLowerInvariant();
        }

        static void RenderOnce(Camera cam, RenderTexture rt, Texture2D readInto)
        {
            var request = new RenderPipeline.StandardRequest { destination = rt };
            if (!RenderPipeline.SupportsRenderRequest(cam, request))
                throw new InvalidOperationException(
                    "active render pipeline does not support StandardRequest");
            RenderPipeline.SubmitRenderRequest(cam, request);
            if (readInto != null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                readInto.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                readInto.Apply(false);
                RenderTexture.active = prev;
            }
        }

        static System.Reflection.PropertyInfo CurrentGlobalSettingsProp =>
            typeof(GraphicsSettings).GetProperty(
                "currentRenderPipelineGlobalSettings",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "GraphicsSettings.currentRenderPipelineGlobalSettings not found");

        static void BindHdrpGlobalSettings()
        {
            var gs = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            if (gs == null)
                throw new InvalidOperationException(
                    "no HDRP global settings registered");
            CurrentGlobalSettingsProp.SetValue(null, gs);
        }
    }
}
