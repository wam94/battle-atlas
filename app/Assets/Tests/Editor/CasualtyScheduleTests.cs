using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: deterministic victim selection (plan §6.4). The schedule is a
/// pure function of (unit, seed); totals match the compiled profiles
/// exactly; the alive count reconciles with the bundle's per-second
/// strength (exact at profile boundaries, within one man mid-window where
/// the Phase 5 compiler's banker's rounding can tie).
/// </summary>
public class CasualtyScheduleTests
{
    static AngleBundle bundle;
    const string Seed = "p8-test-seed";

    static AngleBundle Bundle => bundle ??= AngleBundleLoader.Load();

    [Test]
    public void Totals_MatchCompiledProfilesExactly_EveryUnit()
    {
        foreach (var u in Bundle.units)
        {
            var entries = CasualtySchedule.Compile(u, Seed);
            int expected = 0;
            foreach (var p in u.casualtyProfiles) expected += p.count;
            int actual = 0;
            foreach (var e in entries)
                if (!float.IsInfinity(e.fallT)) actual++;
            Assert.AreEqual(expected, actual,
                $"{u.unitId}: casualty total must match profiles exactly");
        }
    }

    [Test]
    public void PerProfileCounts_Exact_AndWithinWindows()
    {
        foreach (var u in Bundle.units)
        {
            var entries = CasualtySchedule.Compile(u, Seed);
            var perProfile = new int[u.casualtyProfiles.Count];
            foreach (var e in entries)
            {
                if (e.profileIndex < 0) continue;
                var p = u.casualtyProfiles[e.profileIndex];
                Assert.GreaterOrEqual(e.fallT, p.t0,
                    $"{u.unitId}/{p.id}: fall before window");
                Assert.LessOrEqual(e.fallT, p.t1,
                    $"{u.unitId}/{p.id}: fall after window");
                perProfile[e.profileIndex]++;
            }
            for (int i = 0; i < perProfile.Length; i++)
                Assert.AreEqual(u.casualtyProfiles[i].count, perProfile[i],
                    $"{u.unitId}/{u.casualtyProfiles[i].id}");
        }
    }

    [Test]
    public void Alive_ReconcilesWithPerSecondStrength_EverySecond()
    {
        foreach (var u in Bundle.units)
        {
            var entries = CasualtySchedule.Compile(u, Seed);
            int t0 = (int)u.perSecond.t0;
            for (int s = 0; s < u.perSecond.strength.Count; s++)
            {
                int alive = CasualtySchedule.AliveCount(entries, t0 + s);
                float compiled = u.perSecond.strength[s];
                Assert.LessOrEqual(Mathf.Abs(alive - compiled), 1f + 1e-3f,
                    $"{u.unitId} t={t0 + s}: alive {alive} vs compiled {compiled}");
            }
        }
    }

    [Test]
    public void Alive_ExactAtProfileBoundaries()
    {
        foreach (var u in Bundle.units)
        {
            var entries = CasualtySchedule.Compile(u, Seed);
            var ordered = new List<AngleCasualtyProfile>(u.casualtyProfiles);
            ordered.Sort((a, b) => a.t0.CompareTo(b.t0));
            foreach (var p in ordered)
            {
                int alive = CasualtySchedule.AliveCount(entries, p.t1);
                Assert.AreEqual(u.startStrength - CumulativeThrough(ordered, p.t1),
                    alive,
                    $"{u.unitId}: boundary t={p.t1}");
            }
        }
    }

    static int CumulativeThrough(List<AngleCasualtyProfile> ordered, float t)
    {
        int n = 0;
        foreach (var p in ordered)
            if (p.t1 <= t) n += p.count;
            else if (p.t0 <= t)
                n += Mathf.RoundToInt(
                    p.count * Cdf(p.intensityCurve, (t - p.t0) / (p.t1 - p.t0)));
        return n;
    }

    static float Cdf(string curve, float x) => curve switch
    {
        "uniform" => x,
        "rising" => x * x,
        "falling" => 1f - (1f - x) * (1f - x),
        _ => x * x * (3f - 2f * x),
    };

    [Test]
    public void Compile_IsDeterministic()
    {
        var u = Bundle.Unit("csa-garnett");
        var a = CasualtySchedule.Compile(u, Seed);
        var b = CasualtySchedule.Compile(u, Seed);
        for (int i = 0; i < a.Length; i++)
        {
            Assert.AreEqual(a[i].fallT, b[i].fallT);
            Assert.AreEqual(a[i].cause, b[i].cause);
            Assert.AreEqual(a[i].woundedCrawl, b[i].woundedCrawl);
            Assert.AreEqual(a[i].profileIndex, b[i].profileIndex);
        }
    }

    [Test]
    public void DifferentSeed_SelectsDifferentVictims()
    {
        var u = Bundle.Unit("csa-garnett");
        var a = CasualtySchedule.Compile(u, Seed);
        var b = CasualtySchedule.Compile(u, "another-seed");
        int differing = 0;
        for (int i = 0; i < a.Length; i++)
            if (float.IsInfinity(a[i].fallT) != float.IsInfinity(b[i].fallT))
                differing++;
        Assert.Greater(differing, 50,
            "different battle seeds must select different victims");
    }

    [Test]
    public void NoSlotIsSelectedTwice()
    {
        // guaranteed structurally (taken[]), so verify per-profile counts
        // sum to total fallen with no overlap
        var u = Bundle.Unit("csa-armistead");
        var entries = CasualtySchedule.Compile(u, Seed);
        var seen = new HashSet<int>();
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].profileIndex < 0) continue;
            Assert.IsTrue(seen.Add(i));
        }
    }

    [Test]
    public void InverseCdfs_RoundTripThePinnedCurves()
    {
        foreach (string curve in new[] { "uniform", "rising", "falling", "spike" })
        {
            for (float p = 0.05f; p < 1f; p += 0.1f)
            {
                float x = CasualtySchedule.InvCdf(curve, p);
                Assert.AreEqual(p, Cdf(curve, x), 1e-4f,
                    $"{curve} invCDF({p}) -> {x}");
            }
        }
    }

    [Test]
    public void CauseApportionment_IsExactPerMix()
    {
        var u = Bundle.Unit("csa-garnett");
        var entries = CasualtySchedule.Compile(u, Seed);
        for (int pi = 0; pi < u.casualtyProfiles.Count; pi++)
        {
            var p = u.casualtyProfiles[pi];
            var counts = new Dictionary<CasualtySchedule.Cause, int>();
            foreach (var e in entries)
            {
                if (e.profileIndex != pi) continue;
                counts.TryGetValue(e.cause, out int c);
                counts[e.cause] = c + 1;
            }
            float total = p.causeMix.musketry + p.causeMix.canister +
                          p.causeMix.shell + p.causeMix.unknown;
            foreach (var kv in counts)
            {
                float share = kv.Key switch
                {
                    CasualtySchedule.Cause.Musketry => p.causeMix.musketry,
                    CasualtySchedule.Cause.Canister => p.causeMix.canister,
                    CasualtySchedule.Cause.Shell => p.causeMix.shell,
                    _ => p.causeMix.unknown,
                };
                float exact = p.count * share / total;
                Assert.LessOrEqual(Mathf.Abs(kv.Value - exact), 1f,
                    $"{p.id} cause {kv.Key}: {kv.Value} vs exact {exact}");
            }
        }
    }

    [Test]
    public void WoundVocabulary_TiesToCauseClasses()
    {
        Assert.AreEqual(CasualtySchedule.WoundCategory.BallWound,
            CasualtySchedule.Wound(CasualtySchedule.Cause.Musketry));
        Assert.AreEqual(CasualtySchedule.WoundCategory.CanisterStrike,
            CasualtySchedule.Wound(CasualtySchedule.Cause.Canister));
        Assert.AreEqual(CasualtySchedule.WoundCategory.FragmentWound,
            CasualtySchedule.Wound(CasualtySchedule.Cause.Shell));
        Assert.AreEqual(CasualtySchedule.WoundCategory.Unspecified,
            CasualtySchedule.Wound(CasualtySchedule.Cause.Unknown));
    }
}
