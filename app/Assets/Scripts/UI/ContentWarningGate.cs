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

        public ContentWarningGate(IAcknowledgementStore store, int currentVersion)
        {
            if (currentVersion < 1)
                throw new ArgumentException(
                    $"content warning version must be >= 1 (got {currentVersion})");
            this.store = store;
            this.currentVersion = currentVersion;
        }

        // Show the warning when the acknowledged version is older than the
        // authored one (0 = never acknowledged).
        public bool NeedsAcknowledgement =>
            store.GetInt(PrefsKey, 0) < currentVersion;

        public void Acknowledge()
        {
            store.SetInt(PrefsKey, currentVersion);
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
            return doc;
        }
    }
}
