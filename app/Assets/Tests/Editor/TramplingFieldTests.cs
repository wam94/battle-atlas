using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: trampling driven by the compiled troop paths (the ED-17 debt
/// from Gate P7). First-trample times are a pure function of the tracks;
/// trampled area grows monotonically with battle time.
/// </summary>
public class TramplingFieldTests
{
    const string Seed = "p8-test-seed";
    static List<UnitRuntime> units;

    static List<UnitRuntime> Units
    {
        get
        {
            if (units != null) return units;
            var ctx = AngleActionContext.Compile(
                AngleBundleLoader.Load(), Seed,
                SoldierActionResolverTests.SyntheticObstacles());
            units = ctx.units;
            return units;
        }
    }

    [Test]
    public void Compile_IsDeterministic()
    {
        var a = TramplingField.Compile(Units, 3900f, 4450f, 800f, 128);
        var b = TramplingField.Compile(Units, 3900f, 4450f, 800f, 128);
        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void TrampledArea_GrowsMonotonicallyWithTime()
    {
        var firstT = TramplingField.Compile(Units, 3900f, 4450f, 800f, 128);
        int prev = -1;
        for (float t = 8040f; t <= 9000f; t += 120f)
        {
            int n = TramplingField.TrampledCount(firstT, t);
            Assert.GreaterOrEqual(n, prev, $"t={t}");
            prev = n;
        }
        Assert.Greater(prev, 0, "the assault corridor gets trampled");
    }

    [Test]
    public void AssaultCorridor_TramplesDuringTheCharge()
    {
        var firstT = TramplingField.Compile(Units, 3900f, 4450f, 800f, 128);
        // a cell in Garnett's path midway between road and wall
        var garnett = AngleBundleLoader.Load().Unit("csa-garnett");
        Vector2 mid = garnett.PositionAt(8450f);
        int cx = (int)((mid.x - 3900f) / (800f / 128));
        int cz = (int)((mid.y - 4450f) / (800f / 128));
        float cellT = firstT[cz * 128 + cx];
        Assert.IsFalse(float.IsInfinity(cellT),
            "the corridor under the advancing line must trample");
        Assert.LessOrEqual(cellT, 8700f);
    }

    [Test]
    public void GroundOffEveryTrack_NeverTramples()
    {
        var firstT = TramplingField.Compile(Units, 3900f, 4450f, 800f, 128);
        // far southwest corner of the crop: no compiled track goes there
        Assert.IsTrue(float.IsInfinity(firstT[2 * 128 + 2]),
            "untouched ground stays untouched");
    }
}
