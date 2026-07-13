using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    // CLI entry point for the Phase 0 benchmark build:
    //   "$UNITY" -batchmode -quit -projectPath app -buildTarget OSXUniversal \
    //     -executeMethod BattleAtlas.EditorTools.BenchmarkBuild.Build -logFile -
    // Produces a Development macOS standalone at Builds/P0Benchmark/ (gitignored)
    // which scripts/p0-benchmark.sh then runs with -benchmark.
    public static class BenchmarkBuild
    {
        public static void Build()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled).Select(s => s.path).ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = BuildTarget.StandaloneOSX,
                locationPathName = "Builds/P0Benchmark/BattleAtlasBenchmark.app",
                options = BuildOptions.Development,
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError($"Benchmark build failed: {report.summary.result}, " +
                               $"{report.summary.totalErrors} errors");
                EditorApplication.Exit(1);
            }
            // In-HUD phase switching (ADR 0005): the switcher loads sibling
            // phase battle files at runtime. In the editor it reads
            // Assets/Battle directly; a standalone gets copies inside the
            // bundle's StreamingAssets so ONE launch can browse the whole
            // battle with no arguments. Post-build copy only — the source
            // assets and the project StreamingAssets stay byte-untouched.
            string battleDst = System.IO.Path.Combine(report.summary.outputPath,
                "Contents/Resources/Data/StreamingAssets/Battle");
            System.IO.Directory.CreateDirectory(battleDst);
            foreach (string src in System.IO.Directory.GetFiles("Assets/Battle", "*.json"))
                System.IO.File.Copy(src,
                    System.IO.Path.Combine(battleDst, System.IO.Path.GetFileName(src)),
                    overwrite: true);
            Debug.Log($"Benchmark build OK: {report.summary.outputPath} " +
                      $"({report.summary.totalSize / (1024 * 1024)} MB); " +
                      "battle files copied into StreamingAssets/Battle");
        }
    }
}
