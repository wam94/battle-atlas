using System;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: formation-slot identity math (plan §7.4/§13). Offsets are
/// pure functions of (unitId, slotId, formation): stable, deterministic,
/// and continuous under segment formation blending.
/// </summary>
public class FormationRosterTests
{
    [Test]
    public void Offsets_AreDeterministic()
    {
        for (int slot = 0; slot < 500; slot += 37)
        {
            var a = FormationRoster.Offset("csa-test", slot, 1400, "line");
            var b = FormationRoster.Offset("csa-test", slot, 1400, "line");
            Assert.AreEqual(a, b);
        }
    }

    [Test]
    public void Line_HasTwoRanksAtDrillSpacing()
    {
        int count = 1400;
        int files = FormationRoster.Files(count);
        Assert.AreEqual(700, files);
        var frontMid = FormationRoster.Offset("u", files / 2, count, "line");
        var rearMid = FormationRoster.Offset("u", files + files / 2, count, "line");
        Assert.AreEqual(FormationRoster.RankSpacingM,
            frontMid.y - rearMid.y, 0.2f, "rank spacing");
        var a = FormationRoster.Offset("u", 10, count, "line");
        var b = FormationRoster.Offset("u", 11, count, "line");
        Assert.AreEqual(FormationRoster.FileSpacingM, b.x - a.x, 0.15f,
            "file spacing");
        Assert.AreEqual(350f, FormationRoster.Frontage(count), 1f,
            "1400 men in two ranks at 0.5 m: 350 m of front");
    }

    [Test]
    public void BlendedOffset_MatchesEndpointsAndIsMonotone()
    {
        var from = FormationRoster.Offset("u", 42, 1000, "line");
        var to = FormationRoster.Offset("u", 42, 1000, "scattered");
        var at0 = FormationRoster.BlendedOffset("u", 42, 1000, "line", "scattered", 0f);
        var at1 = FormationRoster.BlendedOffset("u", 42, 1000, "line", "scattered", 1f);
        Assert.AreEqual(from, at0);
        Assert.AreEqual(to, at1);
        var mid = FormationRoster.BlendedOffset("u", 42, 1000, "line", "scattered", 0.5f);
        Assert.AreEqual(Vector2.Lerp(from, to, 0.5f).x, mid.x, 1e-4f);
    }

    [Test]
    public void Routed_TrailsRearward()
    {
        int rearward = 0;
        for (int slot = 0; slot < 200; slot++)
        {
            var line = FormationRoster.Offset("u", slot, 200, "line");
            var rout = FormationRoster.Offset("u", slot, 200, "routed");
            if (rout.y < line.y) rearward++;
        }
        Assert.Greater(rearward, 190, "routed men trail behind the facing");
    }

    [Test]
    public void UnknownFormation_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            FormationRoster.Offset("u", 0, 100, "testudo"));
    }

    [Test]
    public void ToWorld_RotatesByFacing()
    {
        var c = new Vector2(100f, 200f);
        // facing east (90): forward = +x, right = -z
        var w = FormationRoster.ToWorld(c, 90f, new Vector2(2f, 3f));
        Assert.AreEqual(103f, w.x, 1e-3f);
        Assert.AreEqual(198f, w.y, 1e-3f);
        // facing north (0): forward = +z, right = +x
        w = FormationRoster.ToWorld(c, 0f, new Vector2(2f, 3f));
        Assert.AreEqual(102f, w.x, 1e-3f);
        Assert.AreEqual(203f, w.y, 1e-3f);
    }

    [Test]
    public void ArtilleryCrews_ClusterAtTheirGuns()
    {
        // members 0..7 of gun 2 stay within a serving radius of the piece
        var gun = FormationRoster.GunOffset(2);
        for (int member = 0; member < FormationRoster.CrewPerGun; member++)
        {
            int slot = member * FormationRoster.GunsPerBattery + 2;
            var p = FormationRoster.ArtilleryOffset("btty", slot, 100);
            Assert.Less((p - gun).magnitude, 9f,
                $"crew member {member} serves gun 2");
        }
        // rear echelon well behind the gun line
        var rear = FormationRoster.ArtilleryOffset(
            "btty", FormationRoster.GunsPerBattery * FormationRoster.CrewPerGun + 3,
            100);
        Assert.Less(rear.y, -20f);
    }
}
