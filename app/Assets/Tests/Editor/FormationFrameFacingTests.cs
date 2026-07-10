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
}
