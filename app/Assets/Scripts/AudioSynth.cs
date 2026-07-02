using UnityEngine;

namespace BattleAtlas
{
    // Fully procedural battle-sound loops — no samples, nothing to license.
    // If PD/CC0 recordings ever replace these fills, they must be recorded
    // in a CREDITS file (phase plan, audio decisions). Three mono 22050 Hz
    // ~8 s seamless loops, each a PURE deterministic sample-fill (FNV-seeded
    // hash noise, never Random or time): artillery rumble (double-low-passed
    // noise under slow boom modulation), musketry crackle (sparse low-passed
    // impulse bursts over a quiet band-passed bed), ambient bed (faint
    // filtered hiss). Every component is periodic in the buffer length —
    // integer envelope cycles, filters warmed over one full period before
    // the write pass — and each texture's envelope reaches its trough at
    // i = 0, so the loop seam lands in a lull and stays inaudible.
    public static class AudioSynth
    {
        public const int SampleRate = 22050;
        public const int LoopSamples = SampleRate * 8;

        // target loudness per loop; AcousticField layers per-event gain and
        // Unity's 3D distance attenuation on top of these
        const float RumbleRms = 0.22f;
        const float CrackleRms = 0.15f;
        const float AmbientRms = 0.08f;

        // artillery rumble: white hash noise through two cascaded one-pole
        // low-passes (~70 Hz knee), amplitude-modulated by 5 boom swells and
        // one slow heave per loop — a gun LINE heard from distance, not
        // individual reports
        public static void FillRumble(float[] s)
        {
            int n = s.Length;
            float lp1 = 0f, lp2 = 0f;
            const float a = 0.02f;
            // first pass warms the filter state over one full period, second
            // pass writes: the state entering sample 0 equals the state
            // leaving sample n-1, so the filtered loop is exactly periodic
            for (int p = 0; p < 2 * n; p++)
            {
                float x = Noise(101, p % n);
                lp1 += a * (x - lp1);
                lp2 += a * (lp1 - lp2);
                if (p >= n) s[p - n] = lp2;
            }
            for (int i = 0; i < n; i++)
            {
                float u = i / (float)n;
                float boom = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * 5f * u);
                boom *= boom; // sharpen the swells
                float swell = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * u);
                // both cosines bottom out at i = 0: the seam sits in the lull
                s[i] *= (0.25f + 0.75f * boom) * (0.6f + 0.4f * swell);
            }
            Normalize(s, RumbleRms);
        }

        // musketry crackle: ~10 sparse pops per second — short low-passed
        // noise bursts with a fast attack ramp and exponential decay,
        // hash-placed inside a seam guard band — over a quiet band-passed
        // bed so the texture never falls fully silent between pops
        public static void FillCrackle(float[] s)
        {
            int n = s.Length;
            float lpFast = 0f, lpSlow = 0f;
            const float aFast = 0.35f, aSlow = 0.06f;
            for (int p = 0; p < 2 * n; p++)
            {
                float x = Noise(202, p % n);
                lpFast += aFast * (x - lpFast);
                lpSlow += aSlow * (x - lpSlow);
                if (p >= n) s[p - n] = 0.10f * (lpFast - lpSlow);
            }
            // pops stay clear of the wrap by `guard` samples on each side,
            // so the seam crosses only the quiet bed — the crackle
            // equivalent of the rumble's envelope trough
            const int guard = 800;
            const int maxLen = 900;
            int count = n / 2205;
            for (int b = 0; b < count; b++)
            {
                float h1 = (Noise(303, b) + 1f) * 0.5f;
                float h2 = (Noise(404, b) + 1f) * 0.5f;
                float h3 = (Noise(505, b) + 1f) * 0.5f;
                int start = guard + (int)(h1 * (n - 2 * guard - maxLen));
                int length = 250 + (int)(h2 * (maxLen - 250));
                float amp = 0.5f + 0.5f * h3;
                float lp = 0f;
                for (int k = 0; k < length; k++)
                {
                    lp += 0.6f * (Noise(606 + b, k) - lp);
                    float env = Mathf.Min(k / 24f, 1f) * Mathf.Exp(-6f * k / length);
                    s[start + k] += amp * env * lp;
                }
            }
            Normalize(s, CrackleRms);
        }

        // ambient bed: faint double-low-passed hiss with a gentle two-cycle
        // wobble — the "air" under everything, so silence never reads as a
        // muted bug
        public static void FillAmbient(float[] s)
        {
            int n = s.Length;
            float lp1 = 0f, lp2 = 0f;
            const float a = 0.10f;
            for (int p = 0; p < 2 * n; p++)
            {
                float x = Noise(707, p % n);
                lp1 += a * (x - lp1);
                lp2 += a * (lp1 - lp2);
                if (p >= n) s[p - n] = lp2;
            }
            for (int i = 0; i < n; i++)
            {
                float u = i / (float)n;
                s[i] *= 0.85f + 0.15f * (0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * 2f * u));
            }
            Normalize(s, AmbientRms);
        }

        // runtime-only convenience (not under test): one loopable mono clip
        // from a pure fill. AudioClip.Create touches the audio engine, so
        // the testable math stays in the Fill* functions above.
        public static AudioClip CreateClip(string name, System.Action<float[]> fill)
        {
            var samples = new float[LoopSamples];
            fill(samples);
            var clip = AudioClip.Create(name, LoopSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // scale to a target RMS, then clamp to valid sample range — loud
        // burst peaks soft-clip rather than wrapping. Deterministic: pure
        // arithmetic over the buffer.
        static void Normalize(float[] s, float targetRms)
        {
            double sum = 0;
            for (int i = 0; i < s.Length; i++) sum += (double)s[i] * s[i];
            float rms = Mathf.Sqrt((float)(sum / s.Length));
            if (rms <= 0f) return;
            float k = targetRms / rms;
            for (int i = 0; i < s.Length; i++)
                s[i] = Mathf.Clamp(s[i] * k, -1f, 1f);
        }

        // deterministic white noise in [-1, 1] from (seed, index): the same
        // FNV-1a-plus-avalanche recipe as FormationLayout.Jitter, over ints
        // instead of a string id — per-sample hashing must not walk a string
        static float Noise(int seed, int i)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)seed) * 16777619u;
                h = (h ^ (uint)i) * 16777619u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return ((h & 0xFFFFFF) / (float)0xFFFFFF) * 2f - 1f;
            }
        }
    }
}
