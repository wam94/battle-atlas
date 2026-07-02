using UnityEngine;

namespace BattleAtlas
{
    // Acoustic gain as PURE math: a function of (event window, t), never of
    // playback state — so scrubbing in either direction, or jump-cuts,
    // reproduces the identical soundscape (the same determinism contract as
    // ObscurationMath). CPU-side t is always an explicit parameter.
    public static class AcousticMath
    {
        public const float AttackSeconds = 3f;
        public const float ReleaseSeconds = 5f;

        // trapezoid envelope over the event window: 3 s attack from t0,
        // full gain through t1, 5 s release after t1. Multiplicative
        // (attack x release) so a window shorter than the attack still
        // rises and releases without a discontinuity.
        public static float EventGain(float t0, float t1, float t)
        {
            float attack = Mathf.Clamp01((t - t0) / AttackSeconds);
            float release = Mathf.Clamp01((t1 + ReleaseSeconds - t) / ReleaseSeconds);
            return attack * release;
        }
    }
}
