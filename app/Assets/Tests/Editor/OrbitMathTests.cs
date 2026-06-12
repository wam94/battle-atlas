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
}
