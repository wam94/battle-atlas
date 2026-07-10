using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 8: SoldierActionResolver determinism, scrub invariance, formation
/// slot stability, and body-state reconstruction (plan §7.5 and §13). The
/// context is compiled from the REAL committed bundle; obstacle geometry
/// uses synthetic north-south lines (the traced environment bake is a
/// gitignored input — geometry integration is covered by the render
/// harness, logic is covered here).
/// </summary>
public class SoldierActionResolverTests
{
    const string Seed = "p8-test-seed";
    static AngleActionContext ctx;

    static AngleActionContext Ctx => ctx ??= AngleActionContext.Compile(
        AngleBundleLoader.Load(), Seed, SyntheticObstacles());

    // Synthetic north-south obstacle lines derived from the bundle itself:
    // each obstacle sits where the first unit that declares it has its
    // centroid at the middle of the declaring segment, so crossings are
    // guaranteed to happen where the choreography expects them. (The real
    // traced polylines live in the gitignored environment bake; the render
    // harness stages those.)
    internal static Dictionary<string, List<Vector2>> SyntheticObstacles()
    {
        var bundle = AngleBundleLoader.Load();
        var result = new Dictionary<string, List<Vector2>>();
        foreach (var u in bundle.units)
        {
            foreach (var seg in u.segments)
            {
                if (seg.action != "cross_obstacle" && seg.action != "breach")
                    continue;
                if (seg.obstacleIds == null) continue;
                for (int i = 0; i < seg.obstacleIds.Count; i++)
                {
                    string oid = seg.obstacleIds[i];
                    if (result.ContainsKey(oid)) continue;
                    // spread multiple obstacles of one segment a few
                    // meters apart along the advance
                    float frac = seg.obstacleIds.Count == 1 ? 0.5f
                        : 0.3f + 0.4f * i / (seg.obstacleIds.Count - 1);
                    float x = u.PositionAt(
                        seg.t0 + frac * (seg.t1 - seg.t0)).x;
                    result[oid] = new List<Vector2>
                    {
                        new Vector2(x, 4450f), new Vector2(x, 5250f),
                    };
                }
            }
        }
        return result;
    }

    [Test]
    public void ParentBrigade_IsExcluded_ChildrenAreStaged()
    {
        foreach (var ur in Ctx.units)
            Assert.AreNotEqual("us-webb", ur.unit.unitId,
                "parent/child double-count: us-webb must not be staged");
        Assert.DoesNotThrow(() => Ctx.Unit("us-69pa"));
        Assert.DoesNotThrow(() => Ctx.Unit("us-71pa"));
        Assert.DoesNotThrow(() => Ctx.Unit("us-72pa"));
        Assert.AreEqual(12, Ctx.units.Count, "13 bundle units minus the parent");
    }

    [Test]
    public void Resolve_IsRepeatable_Bitwise()
    {
        var times = new[] { 8040f, 8163.4f, 8400f, 8581.25f, 8700f, 8999f };
        foreach (var ur in Ctx.units)
        {
            int step = Mathf.Max(1, ur.slotCount / 23);
            for (int slot = 0; slot < ur.slotCount; slot += step)
            {
                foreach (float t in times)
                {
                    var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    var b = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    AssertStatesEqual(a, b, $"{ur.unit.unitId}/{slot}@{t}");
                }
            }
        }
    }

    [Test]
    public void Resolve_ScrubDirectionInvariant()
    {
        // forward sweep vs reverse sweep vs random-order queries must hash
        // to the identical logical-state digest
        var times = new List<float>();
        for (float t = 8040f; t <= 9000f; t += 37.7f) times.Add(t);

        string forward = DigestStates(times);
        times.Reverse();
        string backward = DigestStates(times);
        Assert.AreEqual(forward, backward,
            "scrubbing backward must reconstruct identical states");

        // deterministic pseudo-shuffle
        times.Sort((a, b) =>
            AngleEnvironmentLayout.Hash01("shuffle", (int)(a * 10f))
            .CompareTo(AngleEnvironmentLayout.Hash01("shuffle", (int)(b * 10f))));
        Assert.AreEqual(forward, DigestStates(times));
    }

    // Visit times in the CALLER's order (that is the scrub direction under
    // test), but canonicalize before hashing: one digest per battle time,
    // combined in time order — so only the resolved STATES matter, never
    // the visit order itself.
    static string DigestStates(List<float> times)
    {
        var perTime = new SortedDictionary<float, string>();
        foreach (float t in times)
        {
            using var sha = SHA256.Create();
            var buf = new List<byte>();
            foreach (var ur in Ctx.units)
            {
                int step = Mathf.Max(1, ur.slotCount / 17);
                for (int slot = 0; slot < ur.slotCount; slot += step)
                {
                    var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    buf.AddRange(BitConverter.GetBytes(s.posX));
                    buf.AddRange(BitConverter.GetBytes(s.posZ));
                    buf.AddRange(BitConverter.GetBytes(s.facingDeg));
                    buf.AddRange(BitConverter.GetBytes(s.clipTime));
                    buf.Add((byte)s.clip);
                    buf.Add(s.status);
                    buf.Add(s.cause);
                    buf.Add(s.variant);
                    buf.Add(s.equip);
                }
            }
            perTime[t] = BitConverter.ToString(sha.ComputeHash(buf.ToArray()));
        }
        using var total = SHA256.Create();
        var all = new List<byte>();
        foreach (var kv in perTime)
            all.AddRange(System.Text.Encoding.ASCII.GetBytes(kv.Value));
        return BitConverter.ToString(total.ComputeHash(all.ToArray()));
    }

    [Test]
    public void RecompiledContext_ProducesIdenticalStates()
    {
        var other = AngleActionContext.Compile(
            AngleBundleLoader.Load(), Seed, SyntheticObstacles());
        foreach (float t in new[] { 8160f, 8460f, 8700f })
        {
            foreach (var ur in Ctx.units)
            {
                int step = Mathf.Max(1, ur.slotCount / 11);
                for (int slot = 0; slot < ur.slotCount; slot += step)
                {
                    var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    var b = SoldierActionResolver.Resolve(other, ur.unitIndex, slot, t);
                    AssertStatesEqual(a, b, $"{ur.unit.unitId}/{slot}@{t}");
                }
            }
        }
    }

    static void AssertStatesEqual(SoldierState a, SoldierState b, string label)
    {
        Assert.AreEqual(a.posX, b.posX, $"{label} posX");
        Assert.AreEqual(a.posZ, b.posZ, $"{label} posZ");
        Assert.AreEqual(a.facingDeg, b.facingDeg, $"{label} facing");
        Assert.AreEqual(a.clip, b.clip, $"{label} clip");
        Assert.AreEqual(a.clipTime, b.clipTime, $"{label} clipTime");
        Assert.AreEqual(a.status, b.status, $"{label} status");
        Assert.AreEqual(a.cause, b.cause, $"{label} cause");
        Assert.AreEqual(a.variant, b.variant, $"{label} variant");
        Assert.AreEqual(a.equip, b.equip, $"{label} equip");
    }

    [Test]
    public void SlotIdentity_StableAcrossTime_VariantNeverChanges()
    {
        var ur = Ctx.Unit("csa-garnett");
        for (int slot = 0; slot < ur.slotCount; slot += 97)
        {
            byte v0 = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, 8040f).variant;
            for (float t = 8100f; t <= 9000f; t += 120f)
                Assert.AreEqual(v0,
                    SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t).variant,
                    $"slot {slot} variant must be time-invariant");
        }
    }

    [Test]
    public void DeadStayDead_AndBodiesPersistInPlace()
    {
        var ur = Ctx.Unit("csa-garnett");
        int checkedCount = 0;
        for (int slot = 0; slot < ur.slotCount && checkedCount < 40; slot++)
        {
            var cas = ur.casualties[slot];
            if (float.IsInfinity(cas.fallT) || cas.woundedCrawl) continue;
            checkedCount++;
            var atFall = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, cas.fallT + 0.1f);
            Assert.AreNotEqual(SoldierState.StatusAlive, atFall.status);
            var later = SoldierActionResolver.Resolve(
                Ctx, ur.unitIndex, slot, cas.fallT + 60f);
            var end = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, 9000f);
            Assert.AreEqual(SoldierState.StatusDead, later.status);
            Assert.AreEqual(SoldierState.StatusDead, end.status);
            Assert.AreEqual(later.posX, end.posX, 1e-4f, "bodies do not move");
            Assert.AreEqual(later.posZ, end.posZ, 1e-4f);
            Assert.AreEqual(later.clip, end.clip, "fall pose persists");
        }
        Assert.Greater(checkedCount, 10);
    }

    [Test]
    public void CasualtyStatus_MatchesScheduleExactly()
    {
        var ur = Ctx.Unit("csa-armistead");
        foreach (float t in new[] { 8200f, 8500f, 8650f, 8900f })
        {
            for (int slot = 0; slot < ur.slotCount; slot += 61)
            {
                var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                bool fallen = t >= ur.casualties[slot].fallT;
                Assert.AreEqual(fallen, s.Fallen,
                    $"slot {slot}@{t}: status must mirror the schedule");
            }
        }
    }

    [Test]
    public void FireByRank_RanksAlternateVolleys()
    {
        var ur = Ctx.Unit("us-69pa");
        AngleBundleSegment seg = null;
        foreach (var sg in ur.unit.segments)
            if (sg.action == "fire_by_rank") { seg = sg; break; }
        Assert.IsNotNull(seg);

        int files = FormationRoster.Files(ur.slotCount);
        int frontSlot = 3, rearSlot = files + 3;
        float f0 = FireCycles.Offset(Seed, ur.unit.unitId, seg, frontSlot, ur.slotCount);
        float f1 = FireCycles.Offset(Seed, ur.unit.unitId, seg, rearSlot, ur.slotCount);
        Assert.AreEqual(FireCycles.Cycle / 2f, f1 - f0,
            FireCycles.RankRaggedness + 0.01f,
            "rear rank staggers half a cycle behind the front rank");
    }

    [Test]
    public void FireCycle_ShowsTheDrillVocabulary()
    {
        var ur = Ctx.Unit("us-69pa");
        AngleBundleSegment seg = null;
        foreach (var sg in ur.unit.segments)
            if (sg.action == "fire_by_rank") { seg = sg; break; }

        // one full cycle after the segment start: every slot must pass
        // through Aim -> Fire -> Reload
        var seen = new HashSet<ClipId>();
        int slot = 0;
        while (!float.IsInfinity(ur.casualties[slot].fallT) &&
               ur.casualties[slot].fallT < seg.t1) slot++;
        for (float t = seg.t0; t < seg.t0 + 2f * FireCycles.Cycle; t += 0.25f)
        {
            if (t >= seg.t1) break;
            seen.Add(SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t).clip);
        }
        Assert.Contains(ClipId.Aim, new List<ClipId>(seen));
        Assert.Contains(ClipId.Fire, new List<ClipId>(seen));
        Assert.Contains(ClipId.Reload, new List<ClipId>(seen));
    }

    [Test]
    public void CrossObstacle_SlotsCrossAtTheObstacleLine()
    {
        var ur = Ctx.Unit("csa-garnett");
        int slotsWithCrossings = 0;
        for (int slot = 0; slot < ur.slotCount; slot++)
        {
            var times = ur.slotCrossings[slot];
            if (times.Length == 0) continue;
            slotsWithCrossings++;
            foreach (float c in times)
            {
                var s = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, c + 1f);
                if (s.Fallen) continue;
                Assert.AreEqual(ClipId.Cross, s.clip,
                    $"slot {slot} must play the crossing clip at t={c + 1f}");
                Assert.AreEqual(1f, s.clipTime, 0.26f);
            }
        }
        Assert.Greater(slotsWithCrossings, ur.slotCount / 2,
            "most of the line crosses the road fences");
    }

    [Test]
    public void FallBack_FacesTheRetreatDirection()
    {
        var ur = Ctx.Unit("csa-garnett");
        int living = 0;
        for (int slot = 0; slot < ur.slotCount && living < 20; slot += 31)
        {
            if (ur.casualties[slot].fallT < 8860f) continue;
            var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, 8860f);
            if (s.clip != ClipId.RoutedRun && s.clip != ClipId.RouteStep) continue;
            living++;
            float rel = Mathf.Abs(Mathf.DeltaAngle(s.facingDeg, 262f));
            Assert.Less(rel, 60f,
                $"slot {slot}: routed men face roughly west, got {s.facingDeg}");
        }
        Assert.Greater(living, 5);
    }

    static bool IsLocomotion(ClipId c) =>
        c == ClipId.March || c == ClipId.RouteStep ||
        c == ClipId.DoubleQuick || c == ClipId.RoutedRun;

    // P8 locomotion review fix: any figure whose resolved track position
    // moves plays a locomotion clip (march / route step / double-quick per
    // paceProfile) — never a stationary clip gliding across the ground.
    [Test]
    public void MovingFigures_PlayLocomotionClips()
    {
        var ur = Ctx.Unit("csa-armistead");
        int checkedCount = 0;
        foreach (float t in new[] { 8100f, 8250f, 8670f, 8685f })
        {
            for (int slot = 0; slot < ur.slotCount; slot += 37)
            {
                var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                if (a.Fallen) continue;
                var b = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t + 0.5f);
                float speed = new Vector2(b.posX - a.posX, b.posZ - a.posZ)
                    .magnitude / 0.5f;
                if (speed < SoldierActionResolver.MoveThresholdMps + 0.15f)
                    continue;   // clearly above threshold only
                checkedCount++;
                bool ok = IsLocomotion(a.clip) || a.clip == ClipId.Cross ||
                          a.clip == ClipId.Flinch || a.clip == ClipId.Brace ||
                          a.clip == ClipId.TurnRetreat;
                Assert.IsTrue(ok,
                    $"slot {slot}@{t}: moving at {speed:F2} m/s but plays " +
                    $"{a.clip} — the Gate P8 'floating' defect");
            }
        }
        Assert.Greater(checkedCount, 15, "test must catch moving men");
    }

    // ... and the converse: a stalled line does not treadmill-march.
    [Test]
    public void StalledFigures_DoNotMarchInPlace()
    {
        var ur = Ctx.Unit("csa-garnett");   // take_canister, track stalled
        foreach (float t in new[] { 8655f, 8675f, 8695f })
        {
            for (int slot = 0; slot < ur.slotCount; slot += 41)
            {
                var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                if (a.Fallen) continue;
                var b = SoldierActionResolver.Resolve(
                    Ctx, ur.unitIndex, slot, t + 0.5f);
                float speed = new Vector2(b.posX - a.posX, b.posZ - a.posZ)
                    .magnitude / 0.5f;
                if (speed > SoldierActionResolver.MoveThresholdMps - 0.1f)
                    continue;   // clearly below threshold only
                Assert.IsFalse(IsLocomotion(a.clip),
                    $"slot {slot}@{t}: stalled ({speed:F2} m/s) but plays " +
                    $"{a.clip} — treadmill marching");
            }
        }
    }

    // Stride rate is keyed to ground DISTANCE: over a steady advance the
    // clip phase advances by (meters traveled / meters-per-cycle) cycles.
    [Test]
    public void StridePhase_ConsistentWithGroundSpeed()
    {
        var ur = Ctx.Unit("csa-garnett");   // steady advance 8340..8580
        int tested = 0;
        for (int slot = 0; slot < ur.slotCount && tested < 12; slot += 53)
        {
            if (ur.casualties[slot].fallT < 8520f) continue;
            const float t = 8480f, dt = 0.4f;
            var a = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
            var b = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t + dt);
            if (a.clip != b.clip || !IsLocomotion(a.clip)) continue;
            tested++;
            float meters = new Vector2(b.posX - a.posX, b.posZ - a.posZ).magnitude;
            float dur = KitClips.Duration(a.clip);
            float expected = meters / KitClips.MetersPerCycle(a.clip) * dur;
            float actual = b.clipTime - a.clipTime;
            if (actual < 0f) actual += dur;   // loop wrap
            Assert.AreEqual(expected, actual, 0.075f,
                $"slot {slot}: stride phase must track ground distance " +
                $"(clip {a.clip}, {meters:F2} m)");
        }
        Assert.Greater(tested, 5);
    }

    [Test]
    public void ClipTimes_AlwaysWithinClipDuration()
    {
        foreach (var ur in Ctx.units)
        {
            int step = Mathf.Max(1, ur.slotCount / 19);
            for (float t = 8040f; t <= 9000f; t += 53f)
            {
                for (int slot = 0; slot < ur.slotCount; slot += step)
                {
                    var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, slot, t);
                    Assert.GreaterOrEqual(s.clipTime, 0f,
                        $"{ur.unit.unitId}/{slot}@{t} clip {s.clip}");
                    Assert.LessOrEqual(s.clipTime, KitClips.Duration(s.clip) + 1e-3f,
                        $"{ur.unit.unitId}/{slot}@{t} clip {s.clip}");
                }
            }
        }
    }

    [Test]
    public void ArtilleryCrews_CarryNoMusket_AndBraceAtDischarges()
    {
        var ur = Ctx.Unit("us-btty-cushing");
        var s = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, 0, 8100f);
        Assert.AreEqual(0x80, s.equip & 0x80, "artillery equip flags no musket");

        Assert.IsNotNull(ur.cannonShots);
        Assert.Greater(ur.cannonShots.Count, 20,
            "Cushing's guns fire through the canister window");
        // crew of the firing gun braces at the discharge
        var shot = ur.cannonShots[ur.cannonShots.Count / 2];
        int crewSlot = shot.gun; // member 0 of that gun
        var atShot = SoldierActionResolver.Resolve(
            Ctx, ur.unitIndex, crewSlot, shot.t + 0.5f);
        if (!atShot.Fallen)
            Assert.AreEqual(ClipId.Brace, atShot.clip);
    }

    [Test]
    public void InvalidSlotOrUnit_ThrowLoudly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SoldierActionResolver.Resolve(Ctx, 0, -1, 8100f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SoldierActionResolver.Resolve(
                Ctx, 0, Ctx.units[0].slotCount, 8100f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SoldierActionResolver.Resolve(Ctx, 99, 0, 8100f));
        Assert.Throws<InvalidOperationException>(() =>
            SoldierActionResolver.Resolve(Ctx, "no-such-unit", 0, 8100f));
    }

    [Test]
    public void EngagementEvents_CompiledForTheAssault()
    {
        var garnett = Ctx.Unit("csa-garnett");
        Assert.Greater(garnett.incoming.Count, 0,
            "Garnett takes compiled incoming fire");
        Assert.Greater(garnett.strikes.Count, 10,
            "canister strikes land near Garnett");
        foreach (var st in garnett.strikes)
        {
            Assert.GreaterOrEqual(st.t, 8040f);
            Assert.LessOrEqual(st.t, 9001f);
        }
    }
}
