using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas.EditorTools
{
    // Creates the transparent smoke/dust materials if absent and wires them
    // onto the scene's BattleDirector — idempotent, like Add Vegetation.
    // The materials are committed ASSETS (Assets/Battle/Smoke.mat, Dust.mat):
    // asset references keep their shader in device builds, where
    // runtime-created materials render magenta because the shader variant
    // was stripped (the Phase 2 lesson, re-confirmed by SoldierFigure/Flag).
    public static class ObscurationSetup
    {
        [MenuItem("BattleAtlas/Setup Obscuration")]
        public static void SetupObscuration()
        {
            // grey-white powder smoke / tan dust; per-age fade rides
            // ObscurationField's bucket MaterialPropertyBlocks, so the tint
            // here is what Task 7 tunes
            Material smoke = EnsureMaterial(
                "Assets/Battle/Smoke.mat", new Color(0.93f, 0.93f, 0.90f, 1f));
            Material dust = EnsureMaterial(
                "Assets/Battle/Dust.mat", new Color(0.72f, 0.62f, 0.44f, 1f));

            var director = Object.FindFirstObjectByType<BattleDirector>();
            if (director == null)
            {
                Debug.LogWarning(
                    "ObscurationSetup: no BattleDirector in the open scene; " +
                    "materials ensured but not wired. Open the battle scene and rerun.");
                return;
            }
            director.smokeMaterial = smoke;
            director.dustMaterial = dust;
            EditorSceneManager.MarkSceneDirty(director.gameObject.scene);
            Debug.Log("ObscurationSetup: Smoke.mat / Dust.mat wired on BattleDirector.");
        }

        // reuse the committed asset when present; create it only if absent
        // (safety net for a scene set up before the assets landed)
        static Material EnsureMaterial(string path, Color baseColor)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader);
            // URP Lit's Transparent surface preset, set explicitly: alpha
            // blend, no depth write, transparent queue
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f); // alpha blend mode
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetColor("_BaseColor", baseColor);
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log($"ObscurationSetup: created {path}");
            return mat;
        }
    }
}
