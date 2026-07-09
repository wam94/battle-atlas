using NUnit.Framework;
using UnityEngine;
using BattleAtlas;
using System.Collections.Generic;

public class UnitTrackTests
{
    static UnitDto MakeUnit(params (float t, float x, float z, float facing, float strength)[] kfs)
    {
        var u = new UnitDto
        {
            id = "u", name = "Test", side = "union",
            frontage_m = 100, depth_m = 20, keyframes = new List<KeyframeDto>()
        };
        foreach (var k in kfs)
            u.keyframes.Add(new KeyframeDto
            {
                t = k.t, x = k.x, z = k.z, facing = k.facing,
                formation = "line", strength = k.strength
            });
        return u;
    }

    [Test]
    public void StateAt_InterpolatesPositionAndStrengthLinearly()
    {
        var track = new UnitTrack(MakeUnit((0, 0, 0, 0, 1000), (10, 100, 200, 0, 800)));
        UnitState s = track.StateAt(5f);
        Assert.AreEqual(50f, s.posXZ.x, 1e-3f);
        Assert.AreEqual(100f, s.posXZ.y, 1e-3f); // Vector2.y carries z
        Assert.AreEqual(900f, s.strength, 1e-3f);
    }

    [Test]
    public void StateAt_ClampsBeforeFirstAndAfterLast()
    {
        var track = new UnitTrack(MakeUnit((10, 5, 6, 90, 100), (20, 50, 60, 90, 90)));
        Assert.AreEqual(5f, track.StateAt(0f).posXZ.x, 1e-3f);
        Assert.AreEqual(50f, track.StateAt(99f).posXZ.x, 1e-3f);
    }

    [Test]
    public void StateAt_InterpolatesFacingAcrossNorthWrap()
    {
        var track = new UnitTrack(MakeUnit((0, 0, 0, 350, 1), (10, 0, 0, 10, 1)));
        float facing = track.StateAt(5f).facingDeg;
        // shortest arc from 350° to 10° passes through 0°, not 180°
        Assert.AreEqual(0f, Mathf.DeltaAngle(0f, facing), 1e-3f);
    }

    [Test]
    public void StateAt_PicksCorrectSegmentAmongMany()
    {
        var track = new UnitTrack(MakeUnit(
            (0, 0, 0, 0, 1), (10, 10, 0, 0, 1), (20, 30, 0, 0, 1), (40, 70, 0, 0, 1)));
        Assert.AreEqual(20f, track.StateAt(15f).posXZ.x, 1e-3f); // mid 2nd segment
        Assert.AreEqual(50f, track.StateAt(30f).posXZ.x, 1e-3f); // mid 3rd segment
    }

    [Test]
    public void StateAt_FormationComesFromSegmentStart()
    {
        var u = MakeUnit((0, 0, 0, 0, 1), (10, 10, 0, 0, 1));
        u.keyframes[0].formation = "column";
        u.keyframes[1].formation = "line";
        var track = new UnitTrack(u);
        Assert.AreEqual("column", track.StateAt(5f).formation);
        Assert.AreEqual("line", track.StateAt(10f).formation);
    }

    [Test]
    public void StateAt_ConfidenceComesFromSegmentStart()
    {
        var u = MakeUnit((0, 0, 0, 0, 1), (10, 10, 0, 0, 1));
        u.keyframes[0].confidence = "documented";
        u.keyframes[1].confidence = "inferred";
        var track = new UnitTrack(u);
        // exactly the formation rule: a segment carries its START keyframe
        Assert.AreEqual("documented", track.StateAt(5f).confidence);
        Assert.AreEqual("inferred", track.StateAt(10f).confidence);
    }

    [Test]
    public void StateAt_EmptyConfidenceDefaultsToUnknown()
    {
        // MakeUnit leaves confidence unset — the format default is "unknown"
        // (which StyleOf renders as inferred per the 2026-07-09 user ruling)
        var track = new UnitTrack(MakeUnit((0, 0, 0, 0, 1), (10, 10, 0, 0, 1)));
        Assert.AreEqual("unknown", track.StateAt(5f).confidence);
        Assert.AreEqual("unknown", track.StateAt(10f).confidence);
    }

    [Test]
    public void StateAt_ExactInteriorKeyframeStartsNewSegment()
    {
        var u = MakeUnit((0, 0, 0, 0, 1), (10, 10, 0, 0, 1), (20, 30, 0, 0, 1));
        u.keyframes[0].formation = "column";
        u.keyframes[1].formation = "line";
        u.keyframes[2].formation = "march";
        var track = new UnitTrack(u);
        // t exactly on an interior keyframe belongs to the segment it STARTS
        Assert.AreEqual("line", track.StateAt(10f).formation);
        Assert.AreEqual(10f, track.StateAt(10f).posXZ.x, 1e-3f);
    }
}
