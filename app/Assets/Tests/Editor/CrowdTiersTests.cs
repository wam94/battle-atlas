using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: crowd-tier assignment (plan §7.4). Caps are enforced, nothing
/// is dropped (overflow demotes), and tier is presentation-only: it never
/// feeds back into resolved slot state.
/// </summary>
public class CrowdTiersTests
{
    static List<UnitRuntime> SyntheticUnits(params int[] slotCounts)
    {
        var list = new List<UnitRuntime>();
        foreach (int n in slotCounts)
            list.Add(new UnitRuntime
            {
                unit = new AngleBundleUnit { unitId = $"u{list.Count}" },
                unitIndex = list.Count,
                slotCount = n,
            });
        return list;
    }

    [Test]
    public void EverySlot_AssignedExactlyOnce_CapsRespected()
    {
        var units = SyntheticUnits(3000, 2000);
        // slots on a line marching away from the camera at 0.2 m spacing
        Vector2 Pos(int u, int s) => new Vector2(u * 2f + s * 0.2f, 0f);
        var tiers = CrowdTiers.Assign(Vector2.zero, units, Pos);

        Assert.AreEqual(5000, tiers.Length);
        int hero = 0, near = 0, mid = 0, far = 0;
        foreach (var t in tiers)
        {
            switch (t)
            {
                case CrowdTier.Hero: hero++; break;
                case CrowdTier.Near: near++; break;
                case CrowdTier.Mid: mid++; break;
                default: far++; break;
            }
        }
        Assert.LessOrEqual(hero, CrowdTiers.HeroCap);
        Assert.LessOrEqual(near, CrowdTiers.NearCap);
        Assert.LessOrEqual(mid, CrowdTiers.MidCap);
        Assert.AreEqual(5000, hero + near + mid + far, "nothing dropped");
    }

    [Test]
    public void Overflow_DemotesToNextTier_NeverDrops()
    {
        // 300 men all within 10 m of the camera: only 64 can be hero,
        // the rest demote to near (not disappear)
        var units = SyntheticUnits(300);
        Vector2 Pos(int u, int s) => new Vector2(
            5f + 4f * AngleEnvironmentLayout.Hash01("tier", s),
            4f * AngleEnvironmentLayout.Hash01("tier2", s));
        var tiers = CrowdTiers.Assign(Vector2.zero, units, Pos);
        int hero = 0, near = 0;
        foreach (var t in tiers)
        {
            if (t == CrowdTier.Hero) hero++;
            if (t == CrowdTier.Near) near++;
        }
        Assert.AreEqual(CrowdTiers.HeroCap, hero);
        Assert.AreEqual(300 - CrowdTiers.HeroCap, near);
    }

    [Test]
    public void NearestSlots_GetTheHeroTier()
    {
        var units = SyntheticUnits(100);
        Vector2 Pos(int u, int s) => new Vector2(1f + s, 0f);
        var tiers = CrowdTiers.Assign(Vector2.zero, units, Pos);
        for (int s = 0; s < 24; s++)
            Assert.AreEqual(CrowdTier.Hero, tiers[s], $"slot {s}");
        for (int s = 25; s < 99; s++)
            Assert.AreNotEqual(CrowdTier.Hero, tiers[s], $"slot {s}");
    }

    [Test]
    public void Assignment_IsDeterministic()
    {
        var units = SyntheticUnits(700, 700);
        Vector2 Pos(int u, int s) => new Vector2(
            400f * AngleEnvironmentLayout.Hash01("p", u * 1000 + s),
            400f * AngleEnvironmentLayout.Hash01("q", u * 1000 + s));
        var a = CrowdTiers.Assign(new Vector2(10f, 10f), units, Pos);
        var b = CrowdTiers.Assign(new Vector2(10f, 10f), units, Pos);
        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void MidPoses_CoverTheKitBake()
    {
        // every clip must map to a pose baked into the *_mid FBX
        var baked = new HashSet<string>
        {
            "pose_march_a", "pose_march_b", "pose_aim", "pose_fire",
            "pose_reload_rod", "pose_fallen_back", "pose_fallen_side",
        };
        for (int c = 0; c < KitClips.Count; c++)
        {
            string pose = CrowdTiers.MidPose((ClipId)c, 0.5f, 7, 8123.4f);
            Assert.IsTrue(baked.Contains(pose),
                $"{(ClipId)c} maps to unbaked pose '{pose}'");
        }
        Assert.AreEqual("pose_fallen_back",
            CrowdTiers.MidPose(ClipId.FallCrumple, 2f, 0, 8500f));
        Assert.AreEqual("pose_fallen_side",
            CrowdTiers.MidPose(ClipId.ProneCrawl, 0.5f, 0, 8500f));
    }

    [Test]
    public void MidMarch_AlternatesTheFlipbook()
    {
        // over one gait cycle of the resolver's distance-driven stride
        // phase (clipTime) the pose must change at least once — and a
        // figure whose stride phase is frozen must NOT step in place
        var seen = new HashSet<string>();
        float cycle = KitClips.Duration(ClipId.March);
        for (float ct = 0f; ct < cycle; ct += cycle / 12f)
            seen.Add(CrowdTiers.MidPose(ClipId.March, ct, 3, 8100f));
        Assert.AreEqual(2, seen.Count, "distant marchers must step");

        seen.Clear();
        for (float t = 8100f; t < 8101.2f; t += 0.1f)
            seen.Add(CrowdTiers.MidPose(ClipId.March, 0.4f, 3, t));
        Assert.AreEqual(1, seen.Count,
            "frozen stride phase must not flip poses (no treadmill)");
    }
}
