using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class UnitPickerTests
{
    [Test]
    public void RaycastTerrain_HitsASyntheticHillFromAGrazingAngle()
    {
        // flat plain rising into a 10%-grade hill after x = 200; a grazing
        // ray (falling half a centimeter per meter) from y = 12 meets it at
        // x* = 32 / 0.105 — the theater-camera worst case for a raymarch
        System.Func<float, float, float> ground =
            (x, z) => Mathf.Max(0f, (x - 200f) * 0.1f);
        Vector3 origin = new Vector3(0f, 12f, 0f);
        Vector3 dir = new Vector3(1f, -0.005f, 0f).normalized;
        Assert.IsTrue(UnitPicker.RaycastTerrain(
            origin, dir, ground, 1000f, out Vector3 hit));
        float expectedX = 32f / 0.105f;
        Assert.AreEqual(expectedX, hit.x, 0.05f);
        // the hit sits on the terrain surface within refine tolerance...
        Assert.AreEqual(ground(hit.x, hit.z), hit.y, 0.01f);
        // ...and on the ray, not somewhere the bisection invented
        float t = hit.x / dir.x;
        Assert.AreEqual(origin.y + dir.y * t, hit.y, 0.01f);
        Assert.AreEqual(0f, hit.z, 1e-3f);
    }

    [Test]
    public void RaycastTerrain_MissesAndUndergroundStartsResolveHonestly()
    {
        System.Func<float, float, float> flat = (x, z) => 0f;
        // climbing away from flat ground: no hit inside maxDist
        Assert.IsFalse(UnitPicker.RaycastTerrain(
            new Vector3(0f, 5f, 0f), new Vector3(1f, 0.01f, 0f).normalized,
            flat, 500f, out _));
        // parallel above the ground: never lands
        Assert.IsFalse(UnitPicker.RaycastTerrain(
            new Vector3(0f, 5f, 0f), Vector3.right, flat, 500f, out _));
        // starting at-or-under the surface: the origin IS the contact
        Assert.IsTrue(UnitPicker.RaycastTerrain(
            new Vector3(3f, -1f, 4f), Vector3.right, flat, 500f, out Vector3 hit));
        Assert.AreEqual(new Vector3(3f, -1f, 4f), hit);
    }

    [Test]
    public void PickUnit_PointInOrientedFootprint()
    {
        // one unit facing east (90 deg): the frontage axis swings from
        // east-west to north-south, exactly the symbol rotation convention
        var centers = new[] { new Vector2(0f, 0f) };
        var facings = new[] { 90f };
        var frontages = new[] { 300f };
        var depths = new[] { 40f };
        // inside: 140 m along the (rotated) frontage, 10 m across the depth
        Assert.AreEqual(0, UnitPicker.PickUnit(
            new Vector2(-10f, 140f), 1, centers, facings, frontages, depths));
        // just past the depth edge
        Assert.AreEqual(-1, UnitPicker.PickUnit(
            new Vector2(-30f, 140f), 1, centers, facings, frontages, depths));
        // just past the frontage end
        Assert.AreEqual(-1, UnitPicker.PickUnit(
            new Vector2(0f, 160f), 1, centers, facings, frontages, depths));
    }

    [Test]
    public void PickUnit_SmallestAreaWinsTheOverlapTie()
    {
        // a regiment ribbon overlapped by its brigade's: the click lands in
        // both footprints, and the smaller area (the regiment) must win —
        // brigade listed FIRST so order can't be doing the work
        var centers = new[] { new Vector2(0f, 0f), new Vector2(50f, 0f) };
        var facings = new[] { 0f, 0f };
        var frontages = new[] { 300f, 90f };
        var depths = new[] { 40f, 10f };
        Assert.AreEqual(1, UnitPicker.PickUnit(
            new Vector2(50f, 0f), 2, centers, facings, frontages, depths));
        // outside the regiment, still inside the brigade
        Assert.AreEqual(0, UnitPicker.PickUnit(
            new Vector2(-100f, 0f), 2, centers, facings, frontages, depths));
    }

    [Test]
    public void PickUnit_TracksTheUnitAcrossTime()
    {
        // footprints are rebuilt from the track's state at t, so the same
        // click hits the unit only where the unit actually IS at that t
        var track = new UnitTrack(new UnitDto
        {
            id = "test-mover",
            frontage_m = 50f,
            depth_m = 10f,
            keyframes = new List<KeyframeDto>
            {
                new KeyframeDto { t = 0f, x = 0f, z = 0f, formation = "line", strength = 300f },
                new KeyframeDto { t = 100f, x = 100f, z = 0f, formation = "line", strength = 300f },
            },
        });
        var centers = new Vector2[1];
        var facings = new[] { 0f };
        var frontages = new[] { 50f };
        var depths = new[] { 10f };

        centers[0] = track.StateAt(0f).posXZ;
        Assert.AreEqual(0, UnitPicker.PickUnit(
            new Vector2(0f, 0f), 1, centers, facings, frontages, depths));

        centers[0] = track.StateAt(100f).posXZ;
        Assert.AreEqual(-1, UnitPicker.PickUnit(
            new Vector2(0f, 0f), 1, centers, facings, frontages, depths));
        Assert.AreEqual(0, UnitPicker.PickUnit(
            new Vector2(100f, 0f), 1, centers, facings, frontages, depths));
    }
}
