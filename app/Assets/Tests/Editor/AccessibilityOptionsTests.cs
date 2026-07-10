using System.Collections.Generic;
using NUnit.Framework;
using BattleAtlas;

// Pins the Phase 12 accessibility options model (plan §12 P12): defaults
// (captions and reduced motion OFF, full volume), persistence through the
// int store, clamping, the master-times-mix rule for the Soldier View
// media gain, and the motion-profile selection.
public class AccessibilityOptionsTests
{
    class FakeStore : IAcknowledgementStore
    {
        public readonly Dictionary<string, int> values = new Dictionary<string, int>();
        public int saves;
        public int GetInt(string key, int fallback)
            => values.TryGetValue(key, out int v) ? v : fallback;
        public void SetInt(string key, int value) => values[key] = value;
        public void Save() => saves++;
    }

    [Test]
    public void Defaults_FullVolume_CaptionsOff_ReducedMotionOff()
    {
        var o = new AccessibilityOptions(new FakeStore());
        Assert.AreEqual(1f, o.MasterVolume01);
        Assert.AreEqual(1f, o.SoldierViewVolume01);
        Assert.IsFalse(o.CaptionsEnabled);   // opt-in by design
        Assert.IsFalse(o.ReducedMotion);
    }

    [Test]
    public void Volumes_PersistAsPercent_AndClamp()
    {
        var store = new FakeStore();
        var o = new AccessibilityOptions(store);
        o.MasterVolume01 = 0.35f;
        Assert.AreEqual(35, store.values[AccessibilityOptions.MasterVolumeKey]);
        Assert.AreEqual(0.35f, o.MasterVolume01, 1e-6f);
        o.SoldierViewVolume01 = 2.7f;   // clamps high
        Assert.AreEqual(1f, o.SoldierViewVolume01);
        o.MasterVolume01 = -1f;         // clamps low
        Assert.AreEqual(0f, o.MasterVolume01);
        Assert.AreEqual(3, store.saves); // every set persists
        // a hand-edited out-of-range pref clamps on read
        store.values[AccessibilityOptions.SoldierViewVolumeKey] = 400;
        Assert.AreEqual(1f, o.SoldierViewVolume01);
    }

    [Test]
    public void EffectiveSoldierViewVolume_IsMasterTimesMix()
    {
        // the VideoPlayer direct-audio path bypasses the AudioListener, so
        // the master fader must be composed into the media gain
        var o = new AccessibilityOptions(new FakeStore());
        o.MasterVolume01 = 0.5f;
        o.SoldierViewVolume01 = 0.4f;
        Assert.AreEqual(0.2f, o.EffectiveSoldierViewVolume01, 1e-6f);
    }

    [Test]
    public void Toggles_PersistAndSelectMotionProfile()
    {
        var store = new FakeStore();
        var o = new AccessibilityOptions(store);
        o.CaptionsEnabled = true;
        Assert.AreEqual(1, store.values[AccessibilityOptions.CaptionsKey]);
        Assert.IsTrue(new AccessibilityOptions(store).CaptionsEnabled);

        Assert.AreEqual(HeroMotionProfile.Standard.rollAmpDeg,
            o.MotionProfile.rollAmpDeg);
        o.ReducedMotion = true;
        Assert.AreEqual(0f, o.MotionProfile.rollAmpDeg); // ReducedMotion: no roll
        Assert.AreEqual(HeroMotionProfile.ReducedMotion.bobAmpM,
            o.MotionProfile.bobAmpM);
    }
}
