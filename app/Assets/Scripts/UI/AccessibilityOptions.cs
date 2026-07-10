using UnityEngine;

namespace BattleAtlas
{
    // Phase 12 accessibility options (plan §12 P12: "readable text,
    // subtitle/caption option for shouted orders, volume controls,
    // motion-reduction"). Pure, store-backed model — the same
    // IAcknowledgementStore abstraction the content-warning gate uses, so
    // the persistence rules are testable without PlayerPrefs. Volumes are
    // stored as integer percent (the store is int-only by design).
    //
    // Defaults: full volume, captions OFF (the task's locked default —
    // captions are opt-in), reduced motion OFF.
    //
    // Reduced motion is an Atlas-side setting: it selects
    // HeroMotionProfile.ReducedMotion for any real-time hero camera work
    // and for future media variants rendered with that profile. The
    // SHIPPED Soldier View media was rendered with the standard profile —
    // a reduced-motion cut is a separate offline render (documented in
    // docs/reconstruction/render-runbook.md); the HUD says so while the
    // setting is on rather than pretending the pixels changed.
    public class AccessibilityOptions
    {
        public const string MasterVolumeKey = "battleatlas.audio.master";
        public const string SoldierViewVolumeKey = "battleatlas.audio.soldierview";
        public const string CaptionsKey = "battleatlas.captions.enabled";
        public const string ReducedMotionKey = "battleatlas.motion.reduced";

        readonly IAcknowledgementStore store;

        public AccessibilityOptions(IAcknowledgementStore store)
        {
            this.store = store;
        }

        // ------------------------------------------------- volumes (0..1)

        // Stored 0..100; a value outside the range (hand-edited prefs)
        // clamps rather than blowing the mix.
        public float MasterVolume01
        {
            get => Percent01(store.GetInt(MasterVolumeKey, 100));
            set { store.SetInt(MasterVolumeKey, ToPercent(value)); store.Save(); }
        }

        public float SoldierViewVolume01
        {
            get => Percent01(store.GetInt(SoldierViewVolumeKey, 100));
            set { store.SetInt(SoldierViewVolumeKey, ToPercent(value)); store.Save(); }
        }

        // The gain the Soldier View media mix actually plays at: the
        // VideoPlayer's direct audio path bypasses the AudioListener, so
        // the master fader must be applied here explicitly.
        public float EffectiveSoldierViewVolume01
            => MasterVolume01 * SoldierViewVolume01;

        // ------------------------------------------------------- toggles

        public bool CaptionsEnabled
        {
            get => store.GetInt(CaptionsKey, 0) != 0;
            set { store.SetInt(CaptionsKey, value ? 1 : 0); store.Save(); }
        }

        public bool ReducedMotion
        {
            get => store.GetInt(ReducedMotionKey, 0) != 0;
            set { store.SetInt(ReducedMotionKey, value ? 1 : 0); store.Save(); }
        }

        // The motion profile the current setting selects (for real-time
        // hero camera work and future reduced-motion renders).
        public HeroMotionProfile MotionProfile => ReducedMotion
            ? HeroMotionProfile.ReducedMotion : HeroMotionProfile.Standard;

        static int ToPercent(float v01)
            => Mathf.RoundToInt(Mathf.Clamp01(v01) * 100f);

        static float Percent01(int percent)
            => Mathf.Clamp(percent, 0, 100) / 100f;
    }
}
