using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 10 must-fix regressions (the Gate P9 "teleport" at ~25 s into
/// the proof, battle t~8636). Two real defects, both root-caused from
/// per-frame probes of the deterministic state (see
/// docs/reconstruction/p10-teleport-postmortem.md):
///
/// 1. FORMATION WHEEL: the compiled per-second facing stepped 16.7° in
///    one second at Armistead's advance->breach boundary (t=8639..8640)
///    — every CSA brigade carried a 17-20°/s step somewhere — wheeling
///    flank men at up to ~65 m/s. Fixed by SmoothFormationFrame
///    (placement-frame smoothing bounded by MaxWheelFlankSpeedMps).
///
/// 2. LENS PASS-THROUGH: Armistead's column walks through Garnett's
///    halted line, and slot 1014's track passed 0.06 m from the hero
///    camera at t=8635.97 — a body crossing the near plane at walking
///    pace reads as a materialization. Fixed by LensGuard (render-time
///    chord deflection around the camera).
/// </summary>
public class NoTeleportTests
{
    const string Seed = "p9-test-seed";
    static AngleActionContext ctx;

    static AngleActionContext Ctx => ctx ??= AngleActionContext.Compile(
        AngleBundleLoader.Load(), Seed,
        SoldierActionResolverTests.SyntheticObstacles());

    // A resolved slot may never move faster per frame than a sprint...
    const float Fps = 30f;
    const float MaxDeltaM = 8f / 30f;   // 8 m/s at 30 fps
    // ...except the single designed hand-off frame at the end of a fence/
    // wall crossing, where the logical position absorbs the crossing
    // clip's forward root motion (CrossTravelM; the rendered mesh is
    // continuous because the clip roots the body forward).
    const float MaxCrossExitDeltaM =
        SoldierActionResolver.CrossTravelM + 8f / 30f;

    [Test]
    public void FormationFrame_WheelRateIsBounded_ForEveryUnit()
    {
        foreach (var ur in Ctx.units)
        {
            float radius = ur.isArtillery
                ? FormationRoster.GunsPerBattery * FormationRoster.GunSpacingM / 2f
                : FormationRoster.Frontage(ur.slotCount) / 2f;
            for (int i = 1; i < ur.frameFacing.Length; i++)
            {
                float rate = Mathf.Abs(
                    ur.frameFacing[i] - ur.frameFacing[i - 1]); // deg/s
                float flankSpeed = rate * Mathf.Deg2Rad * radius;
                Assert.LessOrEqual(flankSpeed,
                    AngleActionContext.MaxWheelFlankSpeedMps + 0.25f,
                    $"{ur.unit.unitId}: placement frame wheels the flank " +
                    $"at {flankSpeed:F1} m/s at second {i}");
            }
        }
    }

    [Test]
    public void FormationFrame_SmoothingPreservesTheFacingEndToEnd()
    {
        // the kernel spreads wheels in time; it must not change WHERE the
        // frame ends up (long-run placement stays the compiled truth)
        foreach (var ur in Ctx.units)
        {
            var ps = ur.unit.perSecond;
            int n = ur.frameFacing.Length;
            // compare against the compiled facing at both stable ends
            // (frame facing is unwrapped modulo a half turn, so compare
            // mod 180)
            foreach (var (idx, label) in new[] { (0, "start"), (n - 1, "end") })
            {
                float residual = Mathf.Abs(Mathf.DeltaAngle(
                    ur.frameFacing[idx], ps.facingDeg[idx]));
                if (residual > 90f) residual = 180f - residual;
                Assert.LessOrEqual(residual, 0.6f,
                    $"{ur.unit.unitId}: frame facing drifted at slice " +
                    $"{label} ({ur.frameFacing[idx]:F1} vs compiled " +
                    $"{ps.facingDeg[idx]:F1})");
            }
        }
    }

    // The exact defect window the owner flagged (t~8634..8641 covers the
    // lens pass AND the breach wheel), every unit, every slot, at render
    // rate. Runs in a focused window to stay suite-friendly; the render
    // pipeline sweeps the FULL viewpoint window with the same bound
    // before committing render hours (Phase10Render preflight).
    [Test]
    public void DefectWindow_NoSlotTeleports_AtRenderRate()
    {
        const float t0 = 8630f, t1 = 8645f;
        int frames = (int)((t1 - t0) * Fps) + 1;
        var prev = new Vector2[Ctx.TotalSlots];
        var prevClip = new ClipId[Ctx.TotalSlots];
        for (int f = 0; f < frames; f++)
        {
            float t = t0 + f / Fps;
            int flat = 0;
            foreach (var ur in Ctx.units)
            {
                for (int s = 0; s < ur.slotCount; s++, flat++)
                {
                    var st = SoldierActionResolver.Resolve(
                        Ctx, ur.unitIndex, s, t);
                    var p = new Vector2(st.posX, st.posZ);
                    if (f > 0)
                    {
                        float d = (p - prev[flat]).magnitude;
                        float bound = prevClip[flat] == ClipId.Cross &&
                            st.clip != ClipId.Cross
                            ? MaxCrossExitDeltaM : MaxDeltaM;
                        if (d > bound)
                            Assert.Fail(
                                $"{ur.unit.unitId} slot {s} moved {d:F2} m " +
                                $"in one frame at t={t:F2} (clip {st.clip})");
                    }
                    prev[flat] = p;
                    prevClip[flat] = st.clip;
                }
            }
        }
    }

    [Test]
    public void LensGuard_NoFigureEntersTheLens_AndDeflectionIsSmooth()
    {
        // ride the real committed viewpoint camera through the defect
        // window; every hero-tier figure's GUARDED position must stay at
        // arm's length and move continuously
        var settings = new HeroCameraSettings
        {
            unitId = "csa-garnett",
            slot = 881,
            eyeHeightM = 1.66f,
            fovDeg = 68f,
            stabilization = 0.35f,
            thirdPerson = false,
            profile = HeroMotionProfile.Standard,
        };
        const float t0 = 8630f, t1 = 8642f;
        const float radius = LensGuard.DefaultRadiusM;

        // the known offenders from the probe + the observer's file
        var ur = Ctx.Unit("csa-armistead");
        var own = Ctx.Unit("csa-garnett");
        var watch = new List<(UnitRuntime u, int slot)>
        {
            (ur, 1014), (ur, 222), (ur, 223), (ur, 224), (ur, 225),
            (own, 880), (own, 882),
        };

        int frames = (int)((t1 - t0) * Fps) + 1;
        var prevGuarded = new Dictionary<int, Vector2>();
        bool anyDeflected = false;
        for (int f = 0; f < frames; f++)
        {
            float t = t0 + f / Fps;
            var pose = HeroViewpointCamera.Pose(Ctx, settings, t);
            var cam = new Vector2(pose.camX, pose.camZ);
            for (int i = 0; i < watch.Count; i++)
            {
                var (u, slot) = watch[i];
                var st = SoldierActionResolver.Resolve(Ctx, u.unitIndex, slot, t);
                var raw = new Vector2(st.posX, st.posZ);
                var guarded = LensGuard.Guarded(
                    Ctx, u.unitIndex, slot, t, raw, cam, radius);
                if ((guarded - raw).sqrMagnitude > 1e-8f) anyDeflected = true;
                Assert.GreaterOrEqual((guarded - cam).magnitude, radius - 1e-3f,
                    $"{u.unit.unitId} slot {slot} inside the lens guard at t={t:F2}");
                if (prevGuarded.TryGetValue(i, out var prev))
                {
                    float d = (guarded - prev).magnitude;
                    Assert.LessOrEqual(d, MaxDeltaM,
                        $"{u.unit.unitId} slot {slot}: guarded position " +
                        $"jumped {d:F2} m at t={t:F2}");
                }
                prevGuarded[i] = guarded;
            }
        }
        Assert.IsTrue(anyDeflected,
            "premise: the probe's lens offenders should have triggered " +
            "the guard in this window");
    }

    [Test]
    public void LensGuard_IsIdentity_OutsideTheRadius()
    {
        var ur = Ctx.Unit("csa-garnett");
        var st = SoldierActionResolver.Resolve(Ctx, ur.unitIndex, 184, 8500f);
        var raw = new Vector2(st.posX, st.posZ);
        var cam = raw + new Vector2(5f, 5f);
        var guarded = LensGuard.Guarded(
            Ctx, ur.unitIndex, 184, 8500f, raw, cam, LensGuard.DefaultRadiusM);
        Assert.AreEqual(raw, guarded);
    }

    [Test]
    public void HeroCamera_PathIsCalm_ThroughTheWheelBoundaries()
    {
        // the wheel smoothing also retires the P9 "flank translation"
        // limitation: the camera (which rides Garnett's far left flank)
        // must no longer take multi-m/s lateral rides at the compiled
        // wheel boundaries
        var settings = new HeroCameraSettings
        {
            unitId = "csa-garnett",
            slot = 881,
            eyeHeightM = 1.66f,
            fovDeg = 68f,
            stabilization = 0.35f,
            thirdPerson = false,
            profile = HeroMotionProfile.Standard,
        };
        foreach (var (w0, w1) in new[]
            { (8270f, 8290f), (8570f, 8590f), (8630f, 8650f) })
        {
            var prev = HeroViewpointCamera.Pose(Ctx, settings, w0);
            for (float t = w0 + 1f / Fps; t <= w1; t += 1f / Fps)
            {
                var p = HeroViewpointCamera.Pose(Ctx, settings, t);
                float speed = new Vector2(
                    p.camX - prev.camX, p.camZ - prev.camZ).magnitude * Fps;
                Assert.Less(speed, 4.5f,
                    $"camera plan speed {speed:F2} m/s at t={t:F2}");
                prev = p;
            }
        }
    }
}
