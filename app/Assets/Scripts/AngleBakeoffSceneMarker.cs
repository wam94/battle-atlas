using UnityEngine;

namespace BattleAtlas
{
    // The only serialized object in the committed AngleVisualTarget scenes:
    // names the pipeline the scene compares, so the editor menu item
    // (BattleAtlas ▸ Bakeoff ▸ Stage Content For Open Scene Marker) knows
    // which staging variant to build. Content itself is staged from code +
    // data at use time — the scenes stay thin, deterministic, and free of
    // gitignored asset references.
    public class AngleBakeoffSceneMarker : MonoBehaviour
    {
        public string pipeline = "Urp"; // "Urp" or "Hdrp"
    }
}
