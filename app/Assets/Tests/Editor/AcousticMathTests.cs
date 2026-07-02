using NUnit.Framework;
using BattleAtlas;

public class AcousticMathTests
{
    [Test]
    public void EventGain_FullInsideWindowZeroOutside()
    {
        // full gain once the attack completes, held through t1
        Assert.AreEqual(1f, AcousticMath.EventGain(100f, 200f, 103f), 1e-5f);
        Assert.AreEqual(1f, AcousticMath.EventGain(100f, 200f, 150f), 1e-5f);
        Assert.AreEqual(1f, AcousticMath.EventGain(100f, 200f, 200f), 1e-5f);
        // silent at t0 (the attack starts FROM zero), before it, and once
        // the release has fully run out
        Assert.AreEqual(0f, AcousticMath.EventGain(100f, 200f, 100f), 1e-5f);
        Assert.AreEqual(0f, AcousticMath.EventGain(100f, 200f, 50f), 1e-5f);
        Assert.AreEqual(0f, AcousticMath.EventGain(100f, 200f, 205f), 1e-5f);
        Assert.AreEqual(0f, AcousticMath.EventGain(100f, 200f, 9999f), 1e-5f);
    }

    [Test]
    public void EventGain_RampValuesExactAtEdgeOffsets()
    {
        // 3 s attack: linear from t0
        Assert.AreEqual(0.25f, AcousticMath.EventGain(100f, 200f, 100.75f), 1e-5f);
        Assert.AreEqual(0.5f, AcousticMath.EventGain(100f, 200f, 101.5f), 1e-5f);
        // 5 s release: linear after t1
        Assert.AreEqual(0.5f, AcousticMath.EventGain(100f, 200f, 202.5f), 1e-5f);
        Assert.AreEqual(0.2f, AcousticMath.EventGain(100f, 200f, 204f), 1e-5f);
    }
}
