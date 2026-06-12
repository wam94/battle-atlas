using NUnit.Framework;
using BattleAtlas;

public class ClockMathTests
{
    [Test]
    public void Advance_ScalesBySpeed()
    {
        var (t, playing) = ClockMath.Advance(t: 100f, dt: 0.5f, speed: 60f, endTime: 3600f);
        Assert.AreEqual(130f, t, 1e-3f);
        Assert.IsTrue(playing);
    }

    [Test]
    public void Advance_ClampsAtEndAndStops()
    {
        var (t, playing) = ClockMath.Advance(t: 3599f, dt: 1f, speed: 60f, endTime: 3600f);
        Assert.AreEqual(3600f, t, 1e-3f);
        Assert.IsFalse(playing);
    }

    [Test]
    public void FormatTime_HoursMinutesSeconds()
    {
        Assert.AreEqual("01:02:03", ClockMath.FormatTime(3723f));
        Assert.AreEqual("00:00:00", ClockMath.FormatTime(0f));
    }
}
