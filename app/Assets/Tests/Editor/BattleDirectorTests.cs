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
}
