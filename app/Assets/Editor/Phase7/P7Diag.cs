using System;
using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BattleAtlas.EditorTools
{
    // Transient Phase 7 diagnostic: stage the environment and hunt for
    // degenerate geometry (edges far longer than any authored feature).
    public static class P7Diag
    {
        public static void Scan()
        {
            int exit = 0;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AngleEnvironmentStage.StageAll();
                foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>())
                {
                    var mesh = mf.sharedMesh;
                    if (mesh == null) continue;
                    var v = mesh.vertices;
                    var t = mesh.triangles;
                    float worst = 0f;
                    int worstIdx = -1;
                    for (int i = 0; i < t.Length; i += 3)
                    {
                        float e = Mathf.Max(
                            Vector3.Distance(v[t[i]], v[t[i + 1]]),
                            Mathf.Max(Vector3.Distance(v[t[i + 1]], v[t[i + 2]]),
                                      Vector3.Distance(v[t[i]], v[t[i + 2]])));
                        if (e > worst) { worst = e; worstIdx = i; }
                    }
                    if (worst > 12f && mesh.name != "Angle Terrain")
                    {
                        var a = v[t[worstIdx]]; var b = v[t[worstIdx + 1]]; var c = v[t[worstIdx + 2]];
                        Debug.Log($"P7Diag: {mf.gameObject.name}/{mesh.name}: worst edge {worst:F1} m " +
                                  $"tri=({a})({b})({c})");
                    }
                }
                Debug.Log("P7Diag: scan done");
            }
            catch (Exception e)
            {
                Debug.LogError($"P7Diag failed: {e}");
                exit = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exit);
        }
    }
}
