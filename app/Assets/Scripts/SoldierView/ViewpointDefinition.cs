using System;
using UnityEngine;

namespace BattleAtlas
{
    // Runtime form of reconstruction/schemas/viewpoint.schema.json (plan
    // section 6.5). Authoritative validation lives in the reconstruction
    // Python suite; Validate() re-checks only what playback depends on, so a
    // hand-edited StreamingAssets file fails loudly instead of desyncing.
    [Serializable]
    public class ViewpointSet
    {
        public ViewpointDefinition[] viewpoints;

        public static ViewpointSet FromJson(string json)
        {
            var set = JsonUtility.FromJson<ViewpointSet>(json);
            if (set?.viewpoints == null)
                throw new ArgumentException("viewpoints.json: missing 'viewpoints' array");
            foreach (var vp in set.viewpoints)
            {
                string err = vp.Validate();
                if (err != null)
                    throw new ArgumentException($"viewpoint '{vp.id}': {err}");
            }
            return set;
        }
    }

    [Serializable]
    public class ViewpointDefinition
    {
        public string id;
        public string title;
        public string unitId;
        public int slotId;
        public string viewKind;
        public double t0;
        public double t1;
        public ViewpointCamera camera;
        public ViewpointMedia media;
        public string[] claimIds;
        public string editorialNote;
        // Development fixtures (the Phase 1 timecode proxy) stay playable
        // for tests but never surface an Atlas entry marker (Phase 11);
        // JsonUtility defaults an absent field to false = product viewpoint.
        public bool development;

        // Cross-phase viewpoints (Iverson production slice): the battle
        // asset whose clock and cast this viewpoint's t0/t1 and media
        // address (e.g. "gettysburg-july1-afternoon"). Empty/absent = the
        // viewpoint set's home asset (the scene's serialized battle — the
        // pre-slice behavior, which every July 3 viewpoint keeps). Entry
        // markers, the timeline band, and entry itself exist only while
        // THIS phase is the loaded one (per-phase media honesty).
        public string battleAsset;

        // Null when playable; otherwise a human-readable reason.
        public string Validate()
        {
            if (string.IsNullOrEmpty(id)) return "missing id";
            if (!(t1 > t0)) return $"t1 ({t1}) must be > t0 ({t0})";
            if (media == null || string.IsNullOrEmpty(media.proxy)) return "missing media.proxy";
            if (media.fps <= 0) return $"bad media.fps ({media?.fps})";
            return null;
        }
    }

    [Serializable]
    public class ViewpointCamera
    {
        public float eyeHeightM;
        public float fovDeg;
        public float[] lookOffsetDeg;
        public float stabilization;
    }

    [Serializable]
    public class ViewpointMedia
    {
        public string proxy;
        public string full;   // null/empty until the full-resolution render exists
        public float fps;
        public int width;
        public int height;

        public bool HasFull => !string.IsNullOrEmpty(full);
    }
}
