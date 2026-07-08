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
