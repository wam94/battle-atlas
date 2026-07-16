using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

// The Iverson's-field proof bundle (second Soldier View film, design
// stage): the committed compiled artifact loads, reconciles, and honors
// the ED-22 observer exemption — no staging or site data required.
public class IversonBundleTests
{
    static string BundlePath => Path.Combine(
        Application.dataPath, "Battle", "Iverson", "iverson.bundle.json");

    static AngleBundle Bundle => AngleBundleLoader.Load(BundlePath);

    [Test]
    public void CommittedBundle_LoadsAndValidates()
    {
        var b = Bundle;
        Assert.AreEqual("angle-bundle/1", b.format);
        Assert.AreEqual(6, b.units.Count);
        Assert.AreEqual(5820f, b.slice.t0);
        Assert.AreEqual(7050f, b.slice.t1);
        // July 1 afternoon phase clock
        Assert.AreEqual(46800, b.clock.startTimeSecondsSinceMidnight);
    }

    [Test]
    public void RegimentSums_MatchTheBrigadeShape()
    {
        var b = Bundle;
        // start: 4 x 319 = 1276 (the brigade macro interpolation at t0)
        int start = 0, end = 0;
        foreach (var uid in new[] { "csa-5nc", "csa-20nc", "csa-23nc", "csa-12nc" })
        {
            var u = b.Unit(uid);
            start += u.startStrength;
            end += (int)u.StrengthAt(b.slice.t1);
        }
        Assert.AreEqual(1276, start);
        // end: 725 (the brigade macro interpolation at t1) — the
        // destruction's in-slice loss is 551, of which 501 falls on the
        // left three regiments (the dress-parade 500)
        Assert.AreEqual(725, end);
        var twelfth = b.Unit("csa-12nc");
        int twelfthLosses = 0;
        foreach (var p in twelfth.casualtyProfiles) twelfthLosses += p.count;
        Assert.AreEqual(47, twelfthLosses,
            "the 12th NC is the documented survivor regiment; its in-slice "
            + "loss must stay light (claim-iv-return-rows)");
    }

    [Test]
    public void ObserverSlot_IsExempt_AndTotalsHold()
    {
        var twelfth = Bundle.Unit("csa-12nc");
        var entries = CasualtySchedule.Compile(twelfth, Bundle.StagingSeed);
        Assert.IsTrue(float.IsPositiveInfinity(entries[184].fallT),
            "observer slot 184 must survive the slice (ED-22)");
        int expected = 0;
        foreach (var p in twelfth.casualtyProfiles) expected += p.count;
        int scheduled = 0;
        foreach (var e in entries)
            if (!float.IsInfinity(e.fallT)) scheduled++;
        Assert.AreEqual(expected, scheduled,
            "the exemption must not change the unit's casualty totals");
    }

    [Test]
    public void ObserverSlot_IsRearRank_NearTheLeftFlank()
    {
        var twelfth = Bundle.Unit("csa-12nc");
        int files = FormationRoster.Files(twelfth.startStrength);
        Assert.AreEqual(160, files);
        Assert.AreEqual(1, FormationRoster.RankOf(184, twelfth.startStrength),
            "the P9 lesson: the observer rides the REAR rank so the front "
            + "rank reads 1.3 m ahead of a forward-facing camera");
        int file = 184 % files;
        Assert.AreEqual(24, file);
    }
}
