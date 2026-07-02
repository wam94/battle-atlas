using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class AudioSynthTests
{
    static readonly (string name, System.Action<float[]> fill)[] Fills =
    {
        ("rumble", AudioSynth.FillRumble),
        ("crackle", AudioSynth.FillCrackle),
        ("ambient", AudioSynth.FillAmbient),
    };

    static float[] Filled(System.Action<float[]> fill)
    {
        var s = new float[AudioSynth.LoopSamples];
        fill(s);
        return s;
    }

    [Test]
    public void Fills_AreDeterministic()
    {
        // FNV-seeded, never Random or time: two fills are bit-identical —
        // the same determinism contract as every other hash-jittered system
        foreach (var (name, fill) in Fills)
        {
            float[] a = Filled(fill);
            float[] b = Filled(fill);
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    Assert.Fail($"{name}: sample {i} differs ({a[i]} vs {b[i]})");
        }
    }

    [Test]
    public void Fills_StayWithinUnitRange()
    {
        foreach (var (name, fill) in Fills)
        {
            float[] s = Filled(fill);
            for (int i = 0; i < s.Length; i++)
                if (s[i] < -1f || s[i] > 1f)
                    Assert.Fail($"{name}: sample {i} out of range ({s[i]})");
        }
    }

    [Test]
    public void Fills_LandInSaneRmsBand()
    {
        // audible but headroom-safe; per-event gain and 3D distance
        // attenuation layer on top at runtime
        foreach (var (name, fill) in Fills)
        {
            float[] s = Filled(fill);
            double sum = 0;
            for (int i = 0; i < s.Length; i++) sum += (double)s[i] * s[i];
            float rms = Mathf.Sqrt((float)(sum / s.Length));
            Assert.That(rms, Is.InRange(0.05f, 0.5f), name);
        }
    }

    [Test]
    public void Rumble_HasFewerZeroCrossingsThanCrackle()
    {
        // cheap spectral proxy: the ~70 Hz rumble must cross zero far less
        // often than the pop-dense crackle (measured ~1.2k vs ~57k; the
        // 10x bar leaves a wide float-drift margin)
        int rumble = ZeroCrossings(Filled(AudioSynth.FillRumble));
        int crackle = ZeroCrossings(Filled(AudioSynth.FillCrackle));
        Assert.Less(rumble * 10, crackle);
    }

    [Test]
    public void Loops_AreSeamContinuous()
    {
        // every fill is periodic in the buffer (integer envelope cycles,
        // filters warmed over one full period) with its envelope trough —
        // or the crackle's pop guard band — at the wrap, so the first and
        // last samples are as close as any two neighbors in a lull
        foreach (var (name, fill) in Fills)
        {
            float[] s = Filled(fill);
            Assert.Less(Mathf.Abs(s[0] - s[s.Length - 1]), 0.06f, name);
        }
    }

    static int ZeroCrossings(float[] s)
    {
        int count = 0;
        for (int i = 1; i < s.Length; i++)
            if ((s[i - 1] < 0f) != (s[i] < 0f)) count++;
        return count;
    }
}
