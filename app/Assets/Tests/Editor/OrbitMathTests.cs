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

    // ---------------------------------------------------- RTS camera slice

    [Test]
    public void KeyboardPanWorldDelta_ScalesWithDistanceAndDeltaTime()
    {
        Vector3 near = OrbitMath.KeyboardPanWorldDelta(
            0f, new Vector2(0f, 1f), 100f, 1f, 1f, false, 3f);
        Vector3 far = OrbitMath.KeyboardPanWorldDelta(
            0f, new Vector2(0f, 1f), 1000f, 1f, 1f, false, 3f);
        // 10x the distance = 10x the world-space step for the same axis
        // input and the same real second
        Assert.AreEqual(10f * near.z, far.z, 1e-3f);
        Vector3 halfTime = OrbitMath.KeyboardPanWorldDelta(
            0f, new Vector2(0f, 1f), 100f, 1f, 0.5f, false, 3f);
        Assert.AreEqual(0.5f * near.z, halfTime.z, 1e-3f);
    }

    [Test]
    public void KeyboardPanWorldDelta_ShiftMultipliesSpeed()
    {
        Vector3 normal = OrbitMath.KeyboardPanWorldDelta(
            0f, new Vector2(0f, 1f), 500f, 1f, 1f, false, 3f);
        Vector3 fast = OrbitMath.KeyboardPanWorldDelta(
            0f, new Vector2(0f, 1f), 500f, 1f, 1f, true, 3f);
        Assert.AreEqual(3f * normal.z, fast.z, 1e-3f);
    }

    [Test]
    public void KeyboardPanWorldDelta_RotatesWithYaw()
    {
        // at yaw 90, forward (+z axis input) maps to world -x (right-handed
        // yaw rotation of +Vector3.forward)
        Vector3 d = OrbitMath.KeyboardPanWorldDelta(
            90f, new Vector2(0f, 1f), 1000f, 1f, 1f, false, 3f);
        Assert.AreEqual(0f, d.z, 1e-3f);
        Assert.AreNotEqual(0f, d.x);
    }

    [Test]
    public void DescentMaxPitchDeg_AtOrAboveKnee_IsUnrestricted()
    {
        Assert.AreEqual(85f, OrbitMath.DescentMaxPitchDeg(400f, 5f, 400f, 85f, 35f), 1e-3f);
        Assert.AreEqual(85f, OrbitMath.DescentMaxPitchDeg(4000f, 5f, 400f, 85f, 35f), 1e-3f);
    }

    [Test]
    public void DescentMaxPitchDeg_AtFloor_IsFullyEasedToTheObliqueCap()
    {
        Assert.AreEqual(35f, OrbitMath.DescentMaxPitchDeg(5f, 5f, 400f, 85f, 35f), 1e-3f);
    }

    [Test]
    public void DescentMaxPitchDeg_MidwayIsBetweenTheTwoCeilings()
    {
        float mid = OrbitMath.DescentMaxPitchDeg(200f, 5f, 400f, 85f, 35f);
        Assert.Greater(mid, 35f);
        Assert.Less(mid, 85f);
    }

    [Test]
    public void ClampPitchForDistance_NeverBelowTheHorizonFloor()
    {
        Assert.AreEqual(OrbitMath.MinPitchDeg,
            OrbitMath.ClampPitchForDistance(-10f, 5f, 5f, 400f, 85f, 35f), 1e-3f);
    }

    [Test]
    public void ClampPitchForDistance_ClampsToTheDescentCeilingNearTheFloor()
    {
        // 70° is a legal pitch at altitude but must be eased down near the
        // 5 m zoom floor
        float clamped = OrbitMath.ClampPitchForDistance(70f, 5f, 5f, 400f, 85f, 35f);
        Assert.AreEqual(35f, clamped, 1e-3f);
    }

    [Test]
    public void DynamicNearClip_AtOrAboveKnee_IsTheAltitudeValue()
    {
        Assert.AreEqual(10f, OrbitMath.DynamicNearClip(400f, 5f, 400f, 10f, 0.3f), 1e-3f);
        Assert.AreEqual(10f, OrbitMath.DynamicNearClip(9000f, 5f, 400f, 10f, 0.3f), 1e-3f);
    }

    [Test]
    public void DynamicNearClip_AtTheFloor_IsTheFloorValue()
    {
        Assert.AreEqual(0.3f, OrbitMath.DynamicNearClip(5f, 5f, 400f, 10f, 0.3f), 1e-3f);
    }

    [Test]
    public void DynamicNearClip_ShapeMatchesTheDescentPitchCurve()
    {
        // both curves share the same smoothstep progress — a point 3/4 of
        // the way down orders the same as pitch's ceiling would
        float near = OrbitMath.DynamicNearClip(105f, 5f, 400f, 10f, 0.3f);
        Assert.Greater(near, 0.3f);
        Assert.Less(near, 10f);
    }

    [Test]
    public void ResolveTerrainClearance_LiftsAPositionThatDipsUnderClearance()
    {
        System.Func<float, float, float> ground = (x, z) => 50f;
        Vector3 lifted = OrbitMath.ResolveTerrainClearance(
            new Vector3(10f, 51f, 20f), ground, 3f);
        Assert.AreEqual(53f, lifted.y, 1e-3f);
        Assert.AreEqual(10f, lifted.x, 1e-3f);
        Assert.AreEqual(20f, lifted.z, 1e-3f);
    }

    [Test]
    public void ResolveTerrainClearance_LeavesAnAlreadyClearPositionUntouched()
    {
        System.Func<float, float, float> ground = (x, z) => 50f;
        Vector3 pos = new Vector3(10f, 500f, 20f);
        Vector3 result = OrbitMath.ResolveTerrainClearance(pos, ground, 3f);
        Assert.AreEqual(pos, result);
    }

    [Test]
    public void ClampPivotToBounds_ClampsXAndZIndependently()
    {
        Vector3 clamped = OrbitMath.ClampPivotToBounds(
            new Vector3(-50f, 12f, 9000f), new Vector2(0f, 0f), new Vector2(8500f, 8500f));
        Assert.AreEqual(0f, clamped.x, 1e-3f);
        Assert.AreEqual(12f, clamped.y, 1e-3f); // y untouched — bounds are XZ only
        Assert.AreEqual(8500f, clamped.z, 1e-3f);
    }

    [Test]
    public void ClampPivotToBounds_InsideTheRectIsUnchanged()
    {
        Vector3 p = new Vector3(4000f, 0f, 4000f);
        Assert.AreEqual(p, OrbitMath.ClampPivotToBounds(p, Vector2.zero, new Vector2(8500f, 8500f)));
    }

    [Test]
    public void CursorInViewport_TrueInsideTheUnitSquareFalseOutside()
    {
        Assert.IsTrue(OrbitMath.CursorInViewport(new Vector2(0.5f, 0.5f)));
        Assert.IsTrue(OrbitMath.CursorInViewport(new Vector2(0f, 0f)));
        Assert.IsTrue(OrbitMath.CursorInViewport(new Vector2(1f, 1f)));
        Assert.IsFalse(OrbitMath.CursorInViewport(new Vector2(-0.01f, 0.5f)));
        Assert.IsFalse(OrbitMath.CursorInViewport(new Vector2(0.5f, 1.2f)));
    }

    [Test]
    public void ZoomAnchorPoint_OffViewportFallsBackToThePivot()
    {
        System.Func<float, float, float> ground = (x, z) => 0f;
        Vector3 fallback = new Vector3(1f, 2f, 3f);
        Vector3 anchor = OrbitMath.ZoomAnchorPoint(
            new Vector2(1.5f, 0.5f), Vector3.up * 10f, Vector3.down, ground, 1000f, fallback);
        Assert.AreEqual(fallback, anchor);
    }

    [Test]
    public void ZoomAnchorPoint_RayMissingGroundFallsBackToThePivot()
    {
        System.Func<float, float, float> ground = (x, z) => 0f;
        Vector3 fallback = new Vector3(1f, 2f, 3f);
        // pointing away from the ground: never lands within maxDist
        Vector3 anchor = OrbitMath.ZoomAnchorPoint(
            new Vector2(0.5f, 0.5f), new Vector3(0f, 5f, 0f), Vector3.up, ground, 500f, fallback);
        Assert.AreEqual(fallback, anchor);
    }

    [Test]
    public void ZoomAnchorPoint_OnViewportHittingGroundReturnsTheGroundContact()
    {
        System.Func<float, float, float> ground = (x, z) => 0f;
        Vector3 fallback = new Vector3(1f, 2f, 3f);
        Vector3 anchor = OrbitMath.ZoomAnchorPoint(
            new Vector2(0.5f, 0.5f), new Vector3(40f, 12f, 0f), Vector3.down,
            ground, 1000f, fallback);
        Assert.AreEqual(40f, anchor.x, 0.05f);
        Assert.AreEqual(0f, anchor.y, 0.05f);
        Assert.AreEqual(0f, anchor.z, 0.05f);
    }

    [Test]
    public void ZoomAnchorPivot_ZeroFracLeavesThePivotUntouched()
    {
        Vector3 pivot = new Vector3(10f, 0f, 10f);
        Vector3 anchor = new Vector3(500f, 0f, 500f);
        Assert.AreEqual(pivot, OrbitMath.ZoomAnchorPivot(pivot, anchor, 0f));
    }

    [Test]
    public void ZoomAnchorPivot_FullFracMovesAllTheWayToTheAnchor()
    {
        Vector3 pivot = new Vector3(10f, 0f, 10f);
        Vector3 anchor = new Vector3(500f, 0f, 500f);
        Vector3 result = OrbitMath.ZoomAnchorPivot(pivot, anchor, 1f);
        Assert.AreEqual(anchor.x, result.x, 1e-3f);
        Assert.AreEqual(anchor.z, result.z, 1e-3f);
    }

    [Test]
    public void ZoomAnchorPivot_HalfFracIsTheMidpoint()
    {
        Vector3 pivot = new Vector3(0f, 0f, 0f);
        Vector3 anchor = new Vector3(100f, 0f, 100f);
        Vector3 result = OrbitMath.ZoomAnchorPivot(pivot, anchor, 0.5f);
        Assert.AreEqual(50f, result.x, 1e-3f);
        Assert.AreEqual(50f, result.z, 1e-3f);
    }
}
