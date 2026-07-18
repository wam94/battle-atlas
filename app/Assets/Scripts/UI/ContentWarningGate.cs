using System;
using UnityEngine;

namespace BattleAtlas
{
    // First-launch graphic-content warning gate (plan §9.2 / §10): the
    // warning shows before the FIRST Soldier View entry and is then
    // remembered — per authored warning VERSION, so a future rewrite of the
    // warning text re-surfaces it. Storage is abstracted so the persistence
    // rule is testable without touching real PlayerPrefs.
    public interface IAcknowledgementStore
    {
        int GetInt(string key, int fallback);
        void SetInt(string key, int value);
        void Save();
    }

    public class PlayerPrefsStore : IAcknowledgementStore
    {
        public int GetInt(string key, int fallback) => PlayerPrefs.GetInt(key, fallback);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public void Save() => PlayerPrefs.Save();
    }

    public class ContentWarningGate
    {
        public const string PrefsKey = "battleatlas.soldierview.contentwarning.ackVersion";

        readonly IAcknowledgementStore store;
        readonly int currentVersion;
        readonly string key;

        // A per-viewpoint warning override acknowledges under its own key
        // (PrefsKey + "." + viewpointId): each film's warning must surface
        // before ITS first entry — acknowledging the Angle's warning says
        // nothing about having seen the Iverson field's. The default
        // (set-level) warning keeps the original key, so existing
        // acknowledgements carry over unchanged.
        public ContentWarningGate(IAcknowledgementStore store, int currentVersion,
            string key = PrefsKey)
        {
            if (currentVersion < 1)
                throw new ArgumentException(
                    $"content warning version must be >= 1 (got {currentVersion})");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("content warning key must be non-empty");
            this.store = store;
            this.currentVersion = currentVersion;
            this.key = key;
        }

        public static string KeyForViewpoint(string viewpointId) =>
            PrefsKey + "." + viewpointId;

        // Show the warning when the acknowledged version is older than the
        // authored one (0 = never acknowledged).
        public bool NeedsAcknowledgement =>
            store.GetInt(key, 0) < currentVersion;

        public void Acknowledge()
        {
            store.SetInt(key, currentVersion);
            store.Save();
        }
    }

    // Runtime form of StreamingAssets/SoldierView/content-warning.json (the
    // authored text lives there and in
    // docs/reconstruction/soldier-view-content-warning.md; this class only
    // carries it — wording is never composed in code).
    [Serializable]
    public class ContentWarningDoc
    {
        public string id;
        public int version;
        public WarningSection warning;
        public ObserverSection representativeObserver;
        public string docPath;

        // Per-viewpoint warnings (iverson-viewpoint-design.md §7): each
        // additional film ships its own authored warning + observer note
        // BESIDE (never replacing) the default. Same acknowledgment
        // mechanics, per override — its own version, its own ack key.
        public ViewpointOverride[] viewpointOverrides;

        [Serializable]
        public class ViewpointOverride
        {
            public string viewpointId;
            public int version;
            public WarningSection warning;
            public ObserverSection representativeObserver;
        }

        [Serializable]
        public class WarningSection
        {
            public string title;
            public string body;
            public string acknowledgeLabel;
            public string declineLabel;
        }

        [Serializable]
        public class ObserverSection
        {
            public string title;
            public string body;
            public string shortLine;
        }

        public static ContentWarningDoc FromJson(string json)
        {
            var doc = JsonUtility.FromJson<ContentWarningDoc>(json);
            if (doc == null || doc.warning == null
                || string.IsNullOrEmpty(doc.warning.body)
                || string.IsNullOrEmpty(doc.warning.acknowledgeLabel)
                || string.IsNullOrEmpty(doc.warning.declineLabel))
                throw new ArgumentException(
                    "content-warning.json: missing warning body/labels");
            if (doc.version < 1)
                throw new ArgumentException(
                    "content-warning.json: version must be >= 1");
            if (doc.viewpointOverrides != null)
                foreach (var ov in doc.viewpointOverrides)
                {
                    if (ov == null || string.IsNullOrEmpty(ov.viewpointId)
                        || ov.warning == null
                        || string.IsNullOrEmpty(ov.warning.body))
                        throw new ArgumentException(
                            "content-warning.json: viewpoint override needs "
                            + "viewpointId + warning body");
                    if (ov.version < 1)
                        throw new ArgumentException(
                            $"content-warning.json: override '{ov.viewpointId}' "
                            + "version must be >= 1");
                }
            return doc;
        }

        // The override for a viewpoint, or null (= use the default text and
        // the default acknowledgement key).
        public ViewpointOverride OverrideFor(string viewpointId)
        {
            if (viewpointOverrides == null || string.IsNullOrEmpty(viewpointId))
                return null;
            foreach (var ov in viewpointOverrides)
                if (ov != null && ov.viewpointId == viewpointId)
                    return ov;
            return null;
        }

        // Resolved text for a viewpoint: the override's warning/observer
        // sections where authored, the default's otherwise. Acknowledge and
        // decline BUTTON labels always fall back to the default's when the
        // override leaves them empty (mechanics are shared; wording of the
        // warning itself is per film).
        public WarningSection WarningFor(string viewpointId)
        {
            var ov = OverrideFor(viewpointId);
            if (ov == null) return warning;
            return new WarningSection
            {
                title = string.IsNullOrEmpty(ov.warning.title)
                    ? warning.title : ov.warning.title,
                body = ov.warning.body,
                acknowledgeLabel = string.IsNullOrEmpty(ov.warning.acknowledgeLabel)
                    ? warning.acknowledgeLabel : ov.warning.acknowledgeLabel,
                declineLabel = string.IsNullOrEmpty(ov.warning.declineLabel)
                    ? warning.declineLabel : ov.warning.declineLabel,
            };
        }

        public ObserverSection ObserverFor(string viewpointId)
        {
            var ov = OverrideFor(viewpointId);
            return ov?.representativeObserver ?? representativeObserver;
        }
    }
}
