using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class BattleDirectorTests
{
    [Test]
    public void SideColor_DistinguishesSidesAndDefaultsNeutral()
    {
        Assert.AreNotEqual(BattleDirector.SideColor("union"),
                           BattleDirector.SideColor("confederate"));
        Assert.AreEqual(Color.gray, BattleDirector.SideColor("martian"));
    }

    [Test]
    public void MarkerPose_PlacesBottomOnGroundAndConvertsFacing()
    {
        var state = new UnitState
        {
            posXZ = new Vector2(100f, 200f),
            facingDeg = 90f,
            strength = 1f,
            formation = "line",
        };
        var (pos, rot) = BattleDirector.MarkerPose(state, groundY: 50f, markerHeight: 3f);
        Assert.AreEqual(new Vector3(100f, 51.5f, 200f), pos);
        // facing 90 = east: marker forward should be world +X
        Vector3 fwd = rot * Vector3.forward;
        Assert.AreEqual(1f, fwd.x, 1e-3f);
        Assert.AreEqual(0f, fwd.z, 1e-3f);
    }

    [Test]
    public void FootprintSamplePoints_UnrotatedCornersSpanFrontageAndDepth()
    {
        var buffer = new Vector2[9];
        BattleDirector.FootprintSamplePoints(new Vector2(100f, 200f), 0f, 300f, 40f, buffer);
        Assert.AreEqual(100f, buffer[0].x, 1e-3f); // center first
        Assert.AreEqual(200f, buffer[0].y, 1e-3f);
        Assert.AreEqual(-50f, buffer[1].x, 1e-3f); // 100 - 300/2
        Assert.AreEqual(180f, buffer[1].y, 1e-3f); // 200 - 40/2
        Assert.AreEqual(250f, buffer[4].x, 1e-3f); // 100 + 300/2
        Assert.AreEqual(220f, buffer[4].y, 1e-3f); // 200 + 40/2
        Assert.AreEqual(100f, buffer[5].x, 1e-3f); // south edge midpoint
        Assert.AreEqual(180f, buffer[5].y, 1e-3f);
        Assert.AreEqual(-50f, buffer[7].x, 1e-3f); // west edge midpoint
        Assert.AreEqual(200f, buffer[7].y, 1e-3f);
    }

    [Test]
    public void FootprintSamplePoints_RotateWithFacing()
    {
        var buffer = new Vector2[9];
        // facing east (90 deg): the frontage axis swings from east-west to north-south
        BattleDirector.FootprintSamplePoints(Vector2.zero, 90f, 300f, 40f, buffer);
        // local corner (-150 east, -20 north) rotates to (-20 east, +150 north)
        Assert.AreEqual(-20f, buffer[1].x, 1e-3f);
        Assert.AreEqual(150f, buffer[1].y, 1e-3f);
    }
}
