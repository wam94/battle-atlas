using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class OrbitMathTests
{
    [Test]
    public void ClampPitch_StaysAboveHorizonAndBelowVertical()
    {
        Assert.AreEqual(OrbitMath.MinPitchDeg, OrbitMath.ClampPitch(-30f));
        Assert.AreEqual(OrbitMath.MaxPitchDeg, OrbitMath.ClampPitch(120f));
        Assert.AreEqual(45f, OrbitMath.ClampPitch(45f));
    }

    [Test]
    public void ClampDistance_StaysInRange()
    {
        Assert.AreEqual(OrbitMath.MinDistance, OrbitMath.ClampDistance(0f));
        Assert.AreEqual(OrbitMath.MaxDistance, OrbitMath.ClampDistance(1e9f));
    }

    [Test]
    public void CameraPosition_PitchNinetyIsStraightUp()
    {
        Vector3 p = OrbitMath.CameraPosition(Vector3.zero, 0f, 90f, 100f);
        Assert.AreEqual(0f, p.x, 1e-3f);
        Assert.AreEqual(100f, p.y, 1e-3f);
        Assert.AreEqual(0f, p.z, 1e-3f);
    }

    [Test]
    public void CameraPosition_PitchZeroSitsBehindPivotOnHorizon()
    {
        Vector3 p = OrbitMath.CameraPosition(new Vector3(10f, 0f, 10f), 0f, 0f, 100f);
        Assert.AreEqual(10f, p.x, 1e-3f);
        Assert.AreEqual(0f, p.y, 1e-3f);
        Assert.AreEqual(-90f, p.z, 1e-3f);
    }

    [Test]
    public void TwistDegrees_QuarterTurnCounterClockwise()
    {
        // finger pair rotates 90° CCW around their midpoint
        float twist = OrbitMath.TwistDegrees(
            new Vector2(0f, 0f), new Vector2(10f, 0f),
            new Vector2(5f, -5f), new Vector2(5f, 5f));
        Assert.AreEqual(90f, twist, 1e-3f);
    }

    [Test]
    public void TwistDegrees_WrapsAroundPi()
    {
        // from +175° to -175° is +10° of CCW twist, not -350°
        Vector2 a0 = Vector2.zero;
        Vector2 a1 = new Vector2(Mathf.Cos(175f * Mathf.Deg2Rad), Mathf.Sin(175f * Mathf.Deg2Rad));
        Vector2 b1 = new Vector2(Mathf.Cos(-175f * Mathf.Deg2Rad), Mathf.Sin(-175f * Mathf.Deg2Rad));
        float twist = OrbitMath.TwistDegrees(a0, a1, a0, b1);
        Assert.AreEqual(10f, twist, 1e-3f);
    }

    [Test]
    public void PanWorldDelta_DragRightMovesPivotWestAtYawZero()
    {
        // drag-the-map: finger right => terrain follows finger => camera/pivot move west (-x)
        Vector3 d = OrbitMath.PanWorldDelta(0f, new Vector2(100f, 0f), 1000f, 1f);
        Assert.Less(d.x, 0f);
        Assert.AreEqual(0f, d.y, 1e-3f);
        Assert.AreEqual(0f, d.z, 1e-3f);
    }

    [Test]
    public void PanWorldDelta_ScalesWithDistance()
    {
        Vector3 near = OrbitMath.PanWorldDelta(0f, new Vector2(0f, 100f), 100f, 1f);
        Vector3 far = OrbitMath.PanWorldDelta(0f, new Vector2(0f, 100f), 1000f, 1f);
        Assert.AreEqual(10f * near.z, far.z, 1e-3f);
    }

    [Test]
    public void PanWorldDelta_RotatesWithYaw()
    {
        // at yaw 90, screen-right maps to world +Z
        Vector3 d = OrbitMath.PanWorldDelta(90f, new Vector2(100f, 0f), 1000f, 1f);
        Assert.AreEqual(0f, d.x, 1e-3f);
        Assert.Greater(d.z, 0f);
    }
}
