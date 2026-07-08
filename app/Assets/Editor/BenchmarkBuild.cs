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
            Debug.Log($"Benchmark build OK: {report.summary.outputPath} " +
                      $"({report.summary.totalSize / (1024 * 1024)} MB)");
        }
    }
}
