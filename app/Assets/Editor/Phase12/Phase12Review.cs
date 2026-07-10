using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BattleAtlas;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BattleAtlas.EditorTools
{
    // ------------------------------------------------------------------
    // Phase 12 — review support (plan §12 Phase 12, §13).
    //
    //   RenderGoldenFrames   the §13 visual-regression golden frames at
    //                        BOTH review resolutions (2560x1440 and
    //                        1920x1080), five canonical times:
    //                          t=8160  Emmitsburg Road crossing
    //                          t=8400  closing under canister
    //                          t=8580  wall approach
    //                          t=8700  Angle crisis
    //                          t=8820  collapse/repulse transition
    //                        Rendered under the offline HDRP profile
    //                        through the same deterministic staging as
    //                        the production render (GateP9Render.Boot +
    //                        HeroViewpointCamera.Pose + GateP6Render.
    //                        RenderOnce), first-person, lens guard on,
    //                        observer figure hidden. Output to
    //                        docs/benchmarks/captures/p12-gate/golden/
    //                        with a manifest (git SHA, settings hash,
    //                        per-file sha256) so a reviewer can present
    //                        current-vs-approved side by side with the
    //                        input hashes (§13).
    //
    // Usage (repo root; Unity editor closed for this project path):
    //   "$UNITY" -batchmode -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.Phase12Review.RenderGoldenFrames \
    //     -logFile p12-golden.log
    // ------------------------------------------------------------------
    public static class Phase12Review
    {
        const int Fps = 30;
        const int WarmupFrames = 3;

        static readonly float[] GoldenTimes = { 8160f, 8400f, 8580f, 8700f, 8820f };
        static readonly string[] GoldenLabels =
        {
            "emmitsburg-road-crossing", "closing-under-canister",
            "wall-approach", "angle-crisis", "collapse-repulse",
        };
        static readonly (int w, int h)[] Resolutions =
            { (2560, 1440), (1920, 1080) };

        static string RepoRoot => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../.."));
        static string GoldenDir => Path.Combine(
            RepoRoot, "docs/benchmarks/captures/p12-gate/golden");

        public static void RenderGoldenFrames() => Run(GoldenFramesInner);

        // Headless equivalent of the README fresh-checkout step "run
        // BattleAtlas ▸ Import Heightmap and save the scene": the committed
        // Atlas.unity references the GITIGNORED generated terrain asset, so
        // a fresh checkout (or CI/worktree) has a dangling TerrainData until
        // the import runs INSIDE the Atlas scene and the scene is saved —
        // found by the P12 standalone probe ("Terrain has no valid
        // TerrainData!" spam, no macro ground). Leaves the scene modified on
        // disk (local GUID), exactly like the interactive flow; do not
        // commit the churn.
        public static void PrepareStandaloneScene() => Run(() =>
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                "Assets/Scenes/Atlas.unity",
                UnityEditor.SceneManagement.OpenSceneMode.Single);
            HeightmapImporter.Import();
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("could not save Atlas.unity");
            Debug.Log("Phase12Review: terrain imported into Atlas.unity and saved");
        });

        static void Run(Action inner)
        {
            int exitCode = 0;
            try { inner(); }
            catch (Exception e)
            {
                Debug.LogError($"Phase12Review failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        static void GoldenFramesInner()
        {
            var vp = GateP9Render.LoadHeroViewpoint();
            var (scene, rt1440, tex1440) = GateP9Render.Boot(
                out var prevDefault, out var prevQuality, out var prevGlobal);
            try
            {
                var obs = scene.ctx.Unit(vp.unitId);
                var settings = GateP9Render.Settings(vp, thirdPerson: false);
                scene.hiddenUnitIndex = obs.unitIndex;
                scene.hiddenSlot = vp.slotId;

                var freeze = Phase10Render.BuildFreeze(vp, scene.ctx);
                Directory.CreateDirectory(GoldenDir);
                var files = new List<(string name, float t, int w, int h, string sha)>();

                foreach ((int w, int h) in Resolutions)
                {
                    RenderTexture rt = w == 2560 ? rt1440
                        : new RenderTexture(w, h, 32);
                    Texture2D tex = w == 2560 ? tex1440
                        : new Texture2D(w, h, TextureFormat.RGBA32, false);
                    bool warmed = false;
                    for (int i = 0; i < GoldenTimes.Length; i++)
                    {
                        float t = GoldenTimes[i];
                        // camera BEFORE pose: smoke billboards and the
                        // lens guard read the camera position
                        GateP9Render.ApplyHeroPose(scene,
                            HeroViewpointCamera.Pose(scene.ctx, settings, t));
                        scene.Pose(t);
                        if (!warmed)
                        {
                            for (int k = 0; k < WarmupFrames; k++)
                                GateP6Render.RenderOnce(scene.camera, rt, null);
                            warmed = true;
                        }
                        GateP6Render.RenderOnce(scene.camera, rt, tex);
                        string name = string.Format(
                            "golden-t{0:0}-{1}-{2}p.png", t, GoldenLabels[i], h);
                        string path = Path.Combine(GoldenDir, name);
                        File.WriteAllBytes(path, tex.EncodeToPNG());
                        files.Add((name, t, w, h, Sha256File(path)));
                        Debug.Log($"Phase12Review: wrote {name}");
                    }
                    if (w != 2560)
                    {
                        rt.Release();
                        UnityEngine.Object.DestroyImmediate(tex);
                    }
                }

                WriteManifest(freeze, files);
                Debug.Log($"Phase12Review: {files.Count} golden frames -> {GoldenDir}");
            }
            finally
            {
                GateP9Render.Restore(prevDefault, prevQuality, prevGlobal);
            }
        }

        static void WriteManifest(Phase10Render.Freeze freeze,
            List<(string name, float t, int w, int h, string sha)> files)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"purpose\": \"plan §13 golden frames — visual-review reference set (Phase 12)\",\n");
            sb.Append($"  \"gitSha\": \"{freeze.gitSha}\",\n");
            sb.Append($"  \"settingsHash\": \"{freeze.settingsHash}\",\n");
            sb.Append($"  \"bundleChecksum\": \"{freeze.bundleChecksum}\",\n");
            sb.Append($"  \"unityVersion\": \"{freeze.unityVersion}\",\n");
            sb.Append("  \"pixelTolerance\": \"Phase 8 envelope: channel delta <= 12 on <= 8% of pixels, plus <= 8 isolated outlier pixels/frame (p10-determinism.json)\",\n");
            sb.Append("  \"frames\": [\n");
            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];
                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "    {{\"file\": \"{0}\", \"t\": {1}, \"width\": {2}, " +
                    "\"height\": {3}, \"sha256\": \"{4}\"}}{5}\n",
                    f.name, f.t, f.w, f.h, f.sha,
                    i < files.Count - 1 ? "," : ""));
            }
            sb.Append("  ]\n}\n");
            File.WriteAllText(
                Path.Combine(GoldenDir, "golden-manifest.json"), sb.ToString());
        }

        static string Sha256File(string path)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var fs = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(fs))
                .Replace("-", "").ToLowerInvariant();
        }
    }
}
