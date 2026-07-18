using System.IO;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// The webb-wall and cushing-canister viewpoints (the V2 plan's two named
/// Angle extension viewpoints, charge-intensity proposals OP-2/OP-3):
/// design facts pinned against the committed bundle so a data or roster
/// change cannot silently invalidate the rendered films.
///
/// Both viewpoints RENDER THE EXISTING compiled July 3 Angle bundle —
/// zero data authoring. The bundle, its stagingSeed pin, and the shipped
/// garnett viewpoint are byte-untouched by this slice (film safety).
/// </summary>
public class WebbCushingViewpointTests
{
    const string Seed = "webb-cushing-test-seed";
    static AngleBundle bundle;
    static AngleBundle Bundle => bundle ??= AngleBundleLoader.Load();

    static ViewpointDefinition Load(string id)
    {
        string path = Path.Combine(
            Application.streamingAssetsPath, "SoldierView/viewpoints.json");
        var set = ViewpointSet.FromJson(File.ReadAllText(path));
        foreach (var vp in set.viewpoints)
            if (vp.id == id) return vp;
        throw new System.InvalidOperationException($"no {id} viewpoint");
    }

    static AngleBundleUnit Unit(string unitId)
    {
        foreach (var u in Bundle.units)
            if (u.unitId == unitId) return u;
        throw new System.InvalidOperationException($"no {unitId} in bundle");
    }

    [Test]
    public void FilmSafety_BundleStagingSeedPinUnchanged()
    {
        // The absolute constraint of this slice: the new viewpoints render
        // the EXISTING compiled states under the pinned staging seed.
        Assert.AreEqual(
            "d470c4691d0de414534c4ecce93efd3a2fac74373d472899af8465df7e2f7ac1",
            Bundle.StagingSeed);
    }

    [Test]
    public void WebbWall_ObserverSlot_IsRearRankInsideRoster()
    {
        var vp = Load("webb-wall");
        var u = Unit(vp.unitId);
        Assert.AreEqual("us-71pa", vp.unitId);
        int slots = u.startStrength;
        Assert.AreEqual(313, slots, "71st PA compiled start strength");
        Assert.Less(vp.slotId, slots, "observer slot must exist in the roster");
        // rear rank (the P9 lesson): rank 1 of the two-rank line
        Assert.AreEqual(1, FormationRoster.RankOf(vp.slotId, slots),
            "webb-wall observer must ride the REAR rank");
    }

    [Test]
    public void WebbWall_WindowIsTheExistingViewpointWindow()
    {
        var vp = Load("webb-wall");
        var hero = Load("garnett-road-to-angle");
        // OP-2 renders the existing July 3 Angle window: the SAME compiled
        // states as the shipped hero film, faced the other way.
        Assert.AreEqual(hero.t0, vp.t0, 1e-9);
        Assert.AreEqual(hero.t1, vp.t1, 1e-9);
        // and the compiled per-second table covers it (+ the media pad)
        var u = Unit(vp.unitId);
        Assert.LessOrEqual(u.perSecond.t0, vp.t0);
        Assert.GreaterOrEqual(
            u.perSecond.t0 + u.perSecond.strength.Count - 1, vp.t1 + 0.5);
    }

    [Test]
    public void CushingCanister_ObserverIsGunCrew_NeverCushingHimself()
    {
        var vp = Load("cushing-canister");
        var u = Unit(vp.unitId);
        Assert.AreEqual("us-btty-cushing", vp.unitId);
        Assert.AreEqual(97, u.startStrength);
        Assert.Less(vp.slotId, u.startStrength);
        // a serving crew position at the guns, not the limber echelon
        int crewSlots = FormationRoster.GunsPerBattery * FormationRoster.CrewPerGun;
        Assert.Less(vp.slotId, crewSlots,
            "the gun-crew view must ride a crew slot, not the rear echelon");
        // ED-3: the observer is representative and unnamed; the editorial
        // note must carry the Cushing disclaimer explicitly
        StringAssert.Contains("NOT Lieutenant Cushing", vp.editorialNote);
        StringAssert.Contains("claim-cushing-death", vp.editorialNote);
    }

    [Test]
    public void CushingCanister_WindowCoversTheCanisterPhase()
    {
        var vp = Load("cushing-canister");
        var u = Unit(vp.unitId);
        // OP-3's window t=8400..8760 opens exactly with the battery's
        // compiled fire_independent segment
        AngleBundleSegment fire = null;
        foreach (var seg in u.segments)
            if (seg.action == "fire_independent") fire = seg;
        Assert.IsNotNull(fire, "battery must have a canister segment");
        Assert.AreEqual(fire.t0, vp.t0, 1e-9,
            "window opens with the canister phase");
        Assert.LessOrEqual(vp.t0, fire.t1);
        // per-second coverage incl. the media pad
        Assert.GreaterOrEqual(
            u.perSecond.t0 + u.perSecond.strength.Count - 1, vp.t1 + 0.5);
    }

    [Test]
    public void Exemption_PreservesTotals_ForBothNewObserverUnits()
    {
        // ED-22 honesty: the receiving line and the battery both take
        // casualties; exempting the observer slots must not change either
        // unit's totals (another slot draws the fate).
        foreach (string unitId in new[] { "us-71pa", "us-btty-cushing" })
        {
            var u = Unit(unitId);
            var entries = CasualtySchedule.Compile(u, Seed);
            int expected = 0;
            foreach (var p in u.casualtyProfiles) expected += p.count;
            int scheduled = 0;
            foreach (var e in entries)
                if (!float.IsInfinity(e.fallT)) scheduled++;
            Assert.AreEqual(expected, scheduled,
                $"{unitId}: exemption changed the casualty total");
            // and the observer slot itself never falls
            int obsSlot = unitId == "us-71pa" ? 230 : 44;
            Assert.IsTrue(float.IsInfinity(entries[obsSlot].fallT),
                $"{unitId}: protected observer slot {obsSlot} drew a fate");
        }
    }
}
