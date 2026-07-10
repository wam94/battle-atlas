using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 9: the hero viewpoint camera (plan §12 Phase 9, §6.5) is a pure
/// deterministic function of battle time — scrub-invariant like the
/// resolver it rides — with authored, conservative motion amplitudes
/// (comfort), a ReducedMotion accessibility variant, and the ED-22
/// observer-casualty exemption.
/// </summary>
public class HeroViewpointCameraTests
{
    const string Seed = "p9-test-seed";
    static AngleActionContext ctx;

    static AngleActionContext Ctx => ctx ??= AngleActionContext.Compile(
        AngleBundleLoader.Load(), Seed,
        SoldierActionResolverTests.SyntheticObstacles());

    static HeroCameraSettings Fp(float stabilization = 0.35f) =>
        new HeroCameraSettings
        {
            unitId = "csa-garnett",
            slot = 184,
            eyeHeightM = 1.66f,
            fovDeg = 68f,
            stabilization = stabilization,
            thirdPerson = false,
            profile = HeroMotionProfile.Standard,
        };

    static void AssertPosesEqual(HeroCameraPose a, HeroCameraPose b, string msg)
    {
        Assert.AreEqual(a.camX, b.camX, msg);
        Assert.AreEqual(a.camZ, b.camZ, msg);
        Assert.AreEqual(a.eyeAboveGroundM, b.eyeAboveGroundM, msg);
        Assert.AreEqual(a.headingDeg, b.headingDeg, msg);
        Assert.AreEqual(a.pitchDeg, b.pitchDeg, msg);
        Assert.AreEqual(a.rollDeg, b.rollDeg, msg);
        Assert.AreEqual(a.obsX, b.obsX, msg);
        Assert.AreEqual(a.obsZ, b.obsZ, msg);
        Assert.AreEqual(a.chaos01, b.chaos01, msg);
    }

    [Test]
    public void Pose_BitwiseIdentical_UnderAnyScrubOrder()
    {
        var settings = Fp();
        var times = new List<float>();
        for (float t = 8160f; t <= 8820f; t += 37.7f) times.Add(t);

        var forward = new List<HeroCameraPose>();
        foreach (float t in times)
            forward.Add(HeroViewpointCamera.Pose(Ctx, settings, t));

        // reversed and interleaved re-evaluation
        for (int i = times.Count - 1; i >= 0; i--)
        {
            HeroViewpointCamera.Pose(Ctx, settings, times[(i * 7) % times.Count]);
            var again = HeroViewpointCamera.Pose(Ctx, settings, times[i]);
            AssertPosesEqual(forward[i], again,
                $"pose at t={times[i]} must be scrub-invariant");
        }
    }

    [Test]
    public void Pose_PositionAndHeading_AreComfortBounded()
    {
        // Motion-comfort evidence (§12 P9): frame-to-frame camera speed
        // and angular velocity stay within conservative bounds through
        // the fence crossings, the canister climax, and the retreat turn.
        var settings = Fp();
        float dt = 1f / 30f;
        foreach (var (w0, w1) in new[]
            { (8270f, 8320f), (8580f, 8660f), (8690f, 8740f) })
        {
            var prev = HeroViewpointCamera.Pose(Ctx, settings, w0);
            for (float t = w0 + dt; t <= w1; t += dt)
            {
                var p = HeroViewpointCamera.Pose(Ctx, settings, t);
                float dx = p.camX - prev.camX;
                float dz = p.camZ - prev.camZ;
                // 8 m/s ceiling. Measured peak: ~6.8 m/s for ~2 s at the
                // east-fence redress wheel — slot 184 rides ~82 m out on
                // Garnett's left flank, so a small compiled line wheel
                // translates the flank fast (the whole file moves
                // TOGETHER, so relative on-screen motion stays small).
                // The compiled about-face at t≈8700 no longer spikes
                // here: formation placement unwraps half-turn facing
                // steps (UnitRuntime.frameFacing) so an about-face
                // leaves men standing where they stood. Anything past
                // 8 m/s means a broken path, not a wheel.
                float speed = Mathf.Sqrt(dx * dx + dz * dz) / dt;
                Assert.Less(speed, 8f,
                    $"camera plan speed at t={t} ({speed:F2} m/s)");
                float dEye = Mathf.Abs(p.eyeAboveGroundM - prev.eyeAboveGroundM) / dt;
                Assert.Less(dEye, 1.2f,
                    $"vertical eye rate at t={t} ({dEye:F2} m/s)");
                // 170 deg/s ceiling: the repulse about-turn (the observer
                // turns back with his line at t≈8700, a story moment) is
                // the measured peak at ~138 deg/s for under two seconds —
                // a natural human turn. Steady-state yaw (gait sway,
                // glances, handheld) stays far below.
                float dYaw = Mathf.Abs(
                    Mathf.DeltaAngle(prev.headingDeg, p.headingDeg)) / dt;
                Assert.Less(dYaw, 170f,
                    $"yaw rate at t={t} ({dYaw:F1} deg/s)");
                Assert.Less(Mathf.Abs(p.rollDeg), 3f, $"roll at t={t}");
                prev = p;
            }
        }
    }

    [Test]
    public void EyeHeight_StaysWithinAuthoredEnvelope()
    {
        var settings = Fp();
        var prof = HeroMotionProfile.Standard;
        float envelope = prof.bobAmpM + prof.crossLiftM + prof.duckMaxM + 0.01f;
        for (float t = 8160f; t <= 8820f; t += 0.73f)
        {
            var p = HeroViewpointCamera.Pose(Ctx, settings, t);
            Assert.LessOrEqual(Mathf.Abs(p.eyeAboveGroundM - 1.66f), envelope,
                $"eye offset at t={t}");
        }
    }

    [Test]
    public void ReducedMotion_IsCalmerThanStandard()
    {
        var std = Fp();
        var red = Fp();
        red.profile = HeroMotionProfile.ReducedMotion;
        float stdMaxEye = 0f, redMaxEye = 0f, stdMaxRoll = 0f, redMaxRoll = 0f;
        for (float t = 8300f; t <= 8600f; t += 0.31f)
        {
            var a = HeroViewpointCamera.Pose(Ctx, std, t);
            var b = HeroViewpointCamera.Pose(Ctx, red, t);
            stdMaxEye = Mathf.Max(stdMaxEye, Mathf.Abs(a.eyeAboveGroundM - 1.66f));
            redMaxEye = Mathf.Max(redMaxEye, Mathf.Abs(b.eyeAboveGroundM - 1.66f));
            stdMaxRoll = Mathf.Max(stdMaxRoll, Mathf.Abs(a.rollDeg));
            redMaxRoll = Mathf.Max(redMaxRoll, Mathf.Abs(b.rollDeg));
        }
        Assert.Greater(stdMaxEye, 0f, "standard profile must actually bob");
        Assert.Less(redMaxEye, stdMaxEye * 0.5f,
            "reduced-motion bob must be well under half of standard");
        Assert.LessOrEqual(redMaxRoll, 1e-4f,
            "reduced-motion variant carries no roll");
        Assert.Greater(stdMaxRoll, redMaxRoll);
    }

    [Test]
    public void FullStabilization_RemovesGaitAndHandheld()
    {
        var locked = Fp(stabilization: 1f);
        for (float t = 8360f; t <= 8560f; t += 3.17f)
        {
            var p = HeroViewpointCamera.Pose(Ctx, locked, t);
            Assert.AreEqual(0f, p.rollDeg,
                $"stabilization 1 must zero gait/handheld roll (t={t})");
        }
    }

    [Test]
    public void FirstPerson_CameraRidesTheObserver()
    {
        var settings = Fp();
        for (float t = 8200f; t <= 8800f; t += 47.3f)
        {
            var p = HeroViewpointCamera.Pose(Ctx, settings, t);
            float d = new Vector2(p.camX - p.obsX, p.camZ - p.obsZ).magnitude;
            Assert.LessOrEqual(d, HeroMotionProfile.Standard.swayAmpM + 1e-4f,
                $"first-person camera must sit on the observer (t={t})");
        }
    }

    [Test]
    public void CloseThirdPerson_SitsBehindTheObserversShoulder()
    {
        var settings = Fp();
        settings.thirdPerson = true;
        for (float t = 8200f; t <= 8800f; t += 91.7f)
        {
            var p = HeroViewpointCamera.Pose(Ctx, settings, t);
            var offset = new Vector2(p.camX - p.obsX, p.camZ - p.obsZ);
            Assert.That(offset.magnitude, Is.InRange(1.1f, 1.7f),
                $"rig distance at t={t}");
            float hr = p.headingDeg * Mathf.Deg2Rad;
            var fwd = new Vector2(Mathf.Sin(hr), Mathf.Cos(hr));
            Assert.Less(Vector2.Dot(offset.normalized, fwd), -0.5f,
                $"rig must be behind the observer (t={t})");
            Assert.Greater(p.eyeAboveGroundM, 1.9f,
                "rig rides above eye height");
        }
    }

    [Test]
    public void Chaos_PeaksAtTheWallNotInTheRoad()
    {
        var settings = Fp();
        float roadMax = 0f, wallMax = 0f;
        for (float t = 8170f; t <= 8260f; t += 1.1f)
            roadMax = Mathf.Max(roadMax,
                HeroViewpointCamera.Pose(Ctx, settings, t).chaos01);
        for (float t = 8580f; t <= 8700f; t += 1.1f)
            wallMax = Mathf.Max(wallMax,
                HeroViewpointCamera.Pose(Ctx, settings, t).chaos01);
        Assert.Greater(wallMax, roadMax,
            "the canister climax must out-chaos the road crossing");
        Assert.Greater(wallMax, 0.15f, "the climax must register at all");
    }

    [Test]
    public void Noise_IsDeterministic_AndBounded()
    {
        for (float t = 0f; t < 20f; t += 0.037f)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                float a = HeroViewpointCamera.Noise("seed-a", axis, t);
                float b = HeroViewpointCamera.Noise("seed-a", axis, t);
                Assert.AreEqual(a, b, "noise must be pure");
                Assert.That(a, Is.InRange(-1f, 1f));
            }
        }
        Assert.AreNotEqual(
            HeroViewpointCamera.Noise("seed-a", 0, 3.3f),
            HeroViewpointCamera.Noise("seed-b", 0, 3.3f),
            "noise must be keyed by seed");
    }
}

/// <summary>
/// Phase 9 observer-casualty policy (ED-22): committed viewpoint observer
/// slots are exempt from victim selection; totals and reconciliation are
/// unchanged; the exemption table matches the committed viewpoints.json.
/// </summary>
public class ViewpointObserversTests
{
    static AngleBundle Bundle => AngleBundleLoader.Load();

    [Test]
    public void ObserverSlot_NeverDrawsACasualtyFate_AnySeed()
    {
        var garnett = Array.Find(
            Bundle.units.ToArray(), u => u.unitId == "csa-garnett");
        foreach (string seed in new[]
            { "p8-test-seed", "p9-test-seed", Bundle.checksum })
        {
            var entries = CasualtySchedule.Compile(garnett, seed);
            Assert.IsTrue(float.IsPositiveInfinity(entries[184].fallT),
                $"observer slot 184 must survive the slice (seed {seed})");
        }
    }

    [Test]
    public void Exemption_PreservesProfileTotalsExactly()
    {
        var garnett = Array.Find(
            Bundle.units.ToArray(), u => u.unitId == "csa-garnett");
        var entries = CasualtySchedule.Compile(garnett, "p9-test-seed");
        int expected = 0;
        foreach (var p in garnett.casualtyProfiles) expected += p.count;
        int scheduled = 0;
        foreach (var e in entries)
            if (!float.IsInfinity(e.fallT)) scheduled++;
        Assert.AreEqual(expected, scheduled,
            "exempting the observer must not change unit casualty totals");
    }

    [Test]
    public void ProtectionTable_MatchesCommittedViewpoints()
    {
        string path = Path.Combine(
            Application.streamingAssetsPath, "SoldierView/viewpoints.json");
        var set = ViewpointSet.FromJson(File.ReadAllText(path));
        foreach (var vp in set.viewpoints)
        {
            if (vp.id.StartsWith("dev-"))
                continue; // synthetic media-contract fixtures: no observer
            Assert.IsTrue(ViewpointObservers.IsProtected(vp.unitId, vp.slotId),
                $"viewpoint {vp.id}: observer {vp.unitId}/{vp.slotId} " +
                "must be in ViewpointObservers.ProtectedSlots (ED-22)");
        }
        // and nothing is protected that no committed viewpoint claims
        foreach (var kv in ViewpointObservers.ProtectedSlots)
        {
            foreach (int slot in kv.Value)
            {
                bool claimed = false;
                foreach (var vp in set.viewpoints)
                    if (!vp.id.StartsWith("dev-") &&
                        vp.unitId == kv.Key && vp.slotId == slot)
                        claimed = true;
                Assert.IsTrue(claimed,
                    $"{kv.Key}/{slot} is protected but no committed " +
                    "viewpoint rides it");
            }
        }
    }

    [Test]
    public void UnprotectedUnits_AreUntouchedByThePolicy()
    {
        Assert.IsFalse(ViewpointObservers.IsProtected("us-69pa", 0));
        Assert.IsFalse(ViewpointObservers.IsProtected("csa-garnett", 183));
        Assert.IsTrue(ViewpointObservers.IsProtected("csa-garnett", 184));
    }
}
