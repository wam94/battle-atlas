using UnityEngine;

namespace BattleAtlas
{
    // The only serialized object in the committed AngleRender scene (plan
    // §5, §12 Phase 10): marks the deterministic offline render scene.
    // Everything rendered — terrain, environment, figures, VFX, sun — is
    // staged procedurally from code + data at render time
    // (Phase10Render.RenderProduction), so the scene stays thin,
    // deterministic, and free of gitignored asset references. The scene
    // exists so the offline render entry point has a committed,
    // documented home per the target repository layout.
    public class AngleRenderSceneMarker : MonoBehaviour
    {
        [TextArea(3, 8)]
        public string note =
            "Deterministic offline render scene. Do not add content here: " +
            "run BattleAtlas.EditorTools.Phase10Render methods (batchmode) " +
            "to stage and render. See docs/reconstruction/render-runbook.md.";
    }
}
