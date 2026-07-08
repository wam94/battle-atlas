using NUnit.Framework;
using BattleAtlas;

// Pins the plan section 10.1 contract exactly:
//   videoTime = battleClockTime - viewpoint.t0
//   battleClockTime = viewpoint.t0 + videoTime
public class SoldierViewMathTests
{
    const double T0 = 8160.0;
    const double T1 = 8170.0;
    const double Fps = 30.0;

    [Test]
    public void BattleToVideo_IsOffsetFromT0()
    {
        Assert.AreEqual(0.0, SoldierViewMath.BattleToVideo(8160.0, T0), 1e-9);
        Assert.AreEqual(3.5, SoldierViewMath.BattleToVideo(8163.5, T0), 1e-9);
        Assert.AreEqual(9.9, SoldierViewMath.BattleToVideo(8169.9, T0), 1e-9);
    }

    [Test]
    public void VideoToBattle_InvertsExactly()
    {
        for (double t = T0; t < T1; t += 0.253)
        {
            double roundTrip = SoldierViewMath.VideoToBattle(
                SoldierViewMath.BattleToVideo(t, T0), T0);
            Assert.AreEqual(t, roundTrip, 1e-9);
        }
    }

    [Test]
    public void WithinWindow_T0InclusiveT1Exclusive()
    {
        Assert.IsFalse(SoldierViewMath.WithinWindow(8159.999, T0, T1));
        Assert.IsTrue(SoldierViewMath.WithinWindow(8160.0, T0, T1));
        Assert.IsTrue(SoldierViewMath.WithinWindow(8169.999, T0, T1));
        Assert.IsFalse(SoldierViewMath.WithinWindow(8170.0, T0, T1));
        Assert.IsFalse(SoldierViewMath.WithinWindow(0.0, T0, T1));
    }

    [Test]
    public void ClampToWindow_LandsOnDecodableFrames()
    {
        // below window -> first frame; above -> last frame START (t1 - 1/fps)
        Assert.AreEqual(T0, SoldierViewMath.ClampToWindow(0, T0, T1, Fps), 1e-9);
        Assert.AreEqual(T1 - 1.0 / Fps,
            SoldierViewMath.ClampToWindow(9000, T0, T1, Fps), 1e-9);
        Assert.AreEqual(8165.0,
            SoldierViewMath.ClampToWindow(8165.0, T0, T1, Fps), 1e-9);
    }

    [Test]
    public void FrameForBattleTime_QuantizesToVideoFrames()
    {
        Assert.AreEqual(0, SoldierViewMath.FrameForBattleTime(8160.0, T0, Fps));
        // 1/30s later is exactly frame 1 (epsilon absorbs float noise)
        Assert.AreEqual(1, SoldierViewMath.FrameForBattleTime(8160.0 + 1.0 / 30.0, T0, Fps));
        Assert.AreEqual(15, SoldierViewMath.FrameForBattleTime(8160.5, T0, Fps));
        Assert.AreEqual(299, SoldierViewMath.FrameForBattleTime(8169.999, T0, Fps));
    }

    [Test]
    public void DriftFrames_SignedVideoMinusClock()
    {
        // video 100ms ahead of clock at 30fps = +3 frames
        Assert.AreEqual(3.0, SoldierViewMath.DriftFrames(3.1, 8163.0, T0, Fps), 1e-6);
        Assert.AreEqual(-3.0, SoldierViewMath.DriftFrames(2.9, 8163.0, T0, Fps), 1e-6);
        Assert.AreEqual(0.0, SoldierViewMath.DriftFrames(3.0, 8163.0, T0, Fps), 1e-6);
    }

    [Test]
    public void WithinOneFrame_IsTheGateP1Tolerance()
    {
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(3.0, 8163.0, T0, Fps));
        Assert.IsTrue(SoldierViewMath.WithinOneFrame(3.0 + 1.0 / 30.0, 8163.0, T0, Fps));
        Assert.IsFalse(SoldierViewMath.WithinOneFrame(3.0 + 1.5 / 30.0, 8163.0, T0, Fps));
        Assert.IsFalse(SoldierViewMath.WithinOneFrame(3.0 - 1.5 / 30.0, 8163.0, T0, Fps));
    }
}
