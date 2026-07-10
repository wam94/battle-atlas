using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

/// <summary>
/// Phase 9 regression: a compiled about-face (Garnett's fall_back steps
/// the facing table 78.7° -> 264.0° at t=8700) must not point-reflect
/// the formation about its centroid. Before the frame-facing unwrap
/// (UnitRuntime.frameFacing) flank slot 184 teleported ~134 m in one
/// second there — fatal for the hero camera riding it, and wrong for
/// every rendered figure: in drill terms an about-face leaves each man
/// standing where he stood.
/// </summary>
public class FormationFrameFacingTests
{
    [Test]
    public void AboutFace_DoesNotPointReflectTheFormation()
    {
        var ctx = AngleActionContext.Compile(
            AngleBundleLoader.Load(), "p9-test-seed",
            SoldierActionResolverTests.SyntheticObstacles());
        var ur = ctx.Unit("csa-garnett");

        // raw compiled facing really does flip half a turn here — the
        // test guards the PLACEMENT response to that data, so first pin
        // the data premise
        float rawStep = Mathf.Abs(Mathf.DeltaAngle(
            ur.unit.FacingAt(8699f), ur.unit.FacingAt(8700f)));
        Assert.Greater(rawStep, 90f,
            "premise: the compiled repulse facing steps more than 90°");

        // flank slots must move at most a couple of meters per second
        // through the flip (residual wheel + retreat drift), never a
        // reflection to the opposite flank
        foreach (int slot in new[] { 0, 184, 696 })
        {
            var a = SoldierActionResolver.Resolve(ctx, ur.unitIndex, slot, 8698f);
            var b = SoldierActionResolver.Resolve(ctx, ur.unitIndex, slot, 8702f);
            if (a.Fallen || b.Fallen) continue;
            float d = new Vector2(b.posX - a.posX, b.posZ - a.posZ).magnitude;
            Assert.Less(d, 12f,
                $"slot {slot} moved {d:F1} m across the about-face");
        }

        // and the frame facing itself is continuous (no half-turn steps)
        for (int i = 1; i < ur.frameFacing.Length; i++)
            Assert.Less(
                Mathf.Abs(ur.frameFacing[i] - ur.frameFacing[i - 1]), 45f,
                $"frameFacing step at second {i}");
    }

    [Test]
    public void TrailingFlank_StillCrossesTheRoadFences()
    {
        // P9 episode-window fix: Garnett's left flank reaches the road
        // fences ~50 s after the last cross_obstacle segment ends; the
        // old per-segment ±30 s detection windows missed it and the men
        // walked through the rails. With the synthetic obstacle geometry
        // (lines through the declaring segments' centroid track) every
        // slot's path crosses both road fences — including the hero
        // viewpoint's slot 184, whose crossings the camera rides.
        var ctx = AngleActionContext.Compile(
            AngleBundleLoader.Load(), "p9-test-seed",
            SoldierActionResolverTests.SyntheticObstacles());
        var ur = ctx.Unit("csa-garnett");
        // slot 696 (extreme right flank) is excluded: the SYNTHETIC
        // vertical fence lines exaggerate the line's obliqueness so much
        // that the leading flank starts east of them and never crosses —
        // a fixture artifact; the real traced fences run along the road.
        // Real-geometry coverage is verified by the render harness's
        // audio-event export (observer crossings > 0).
        foreach (int slot in new[] { 0, 184, 348 })
        {
            int inCorridor = 0;
            foreach (float t in ur.slotCrossings[slot])
                if (t >= 8040f && t <= 8500f) inCorridor++;
            Assert.GreaterOrEqual(inCorridor, 2,
                $"slot {slot}: expected at least two road-corridor " +
                $"crossings, found {inCorridor}");
        }
    }
}
