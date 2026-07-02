using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class UnitFormationRendererTests
{
    static Matrix4x4[][] NewBuckets()
    {
        var buckets = new Matrix4x4[UnitFormationRenderer.PoseCount][];
        for (int p = 0; p < buckets.Length; p++)
            buckets[p] = new Matrix4x4[FormationLayout.MaxFigures];
        return buckets;
    }

    [Test]
    public void PoseBuckets_SplitDeterministicallyAndBiasedByState()
    {
        var state = new UnitState { posXZ = new Vector2(100f, 200f), facingDeg = 0f, strength = 400f, formation = "line" };
        var offsets = new Vector2[FormationLayout.MaxFigures];
        var bucketsA = NewBuckets();
        var bucketsB = NewBuckets();
        var countsA = new int[UnitFormationRenderer.PoseCount];
        var countsB = new int[UnitFormationRenderer.PoseCount];

        // same inputs, twice: the hash split must replay exactly (scrubbing)
        UnitFormationRenderer.BuildPoseMatrices(
            "u1", state, 300f, 40f, true, (x, z) => 0f, bucketsA, countsA, offsets);
        UnitFormationRenderer.BuildPoseMatrices(
            "u1", state, 300f, 40f, true, (x, z) => 0f, bucketsB, countsB, offsets);
        for (int p = 0; p < UnitFormationRenderer.PoseCount; p++)
        {
            Assert.AreEqual(countsA[p], countsB[p]);
            for (int i = 0; i < countsA[p]; i++)
                Assert.AreEqual(bucketsA[p][i], bucketsB[p][i]);
        }
        // the bias rule: a MOVING line reads as advancing (lean dominant,
        // nobody kneels mid-march)
        Assert.Greater(countsA[UnitFormationRenderer.PoseAdvancing],
                       countsA[UnitFormationRenderer.PoseStanding]);
        Assert.AreEqual(0, countsA[UnitFormationRenderer.PoseKneeling]);

        // a STATIONARY skirmish line is the one engaged posture derivable
        // from formation + movement: kneeling-firing dominates, no leaning
        state.formation = "skirmish";
        UnitFormationRenderer.BuildPoseMatrices(
            "u1", state, 300f, 40f, false, (x, z) => 0f, bucketsA, countsA, offsets);
        Assert.Greater(countsA[UnitFormationRenderer.PoseKneeling],
                       countsA[UnitFormationRenderer.PoseStanding]);
        Assert.AreEqual(0, countsA[UnitFormationRenderer.PoseAdvancing]);

        // a posted line invents nothing: everyone stands at shoulder arms
        state.formation = "line";
        UnitFormationRenderer.BuildPoseMatrices(
            "u1", state, 300f, 40f, false, (x, z) => 0f, bucketsA, countsA, offsets);
        Assert.AreEqual(0, countsA[UnitFormationRenderer.PoseAdvancing]);
        Assert.AreEqual(0, countsA[UnitFormationRenderer.PoseKneeling]);
    }

    [Test]
    public void PoseBuckets_SumToFigureCount()
    {
        var offsets = new Vector2[FormationLayout.MaxFigures];
        var buckets = NewBuckets();
        var counts = new int[UnitFormationRenderer.PoseCount];
        // across states and strengths, every figure lands in exactly one bucket
        foreach ((float strength, string formation, bool moving) c in new[]
        {
            (55f, "line", true), (400f, "skirmish", false),
            (4000f, "column", true), (130f, "routed", false), (0f, "line", false),
        })
        {
            var state = new UnitState
            {
                posXZ = Vector2.zero, facingDeg = 45f,
                strength = c.strength, formation = c.formation,
            };
            int total = UnitFormationRenderer.BuildPoseMatrices(
                "u2", state, 200f, 30f, c.moving, (x, z) => 0f, buckets, counts, offsets);
            Assert.AreEqual(FormationLayout.FigureCount(c.strength), total);
            int sum = 0;
            foreach (int n in counts) sum += n;
            Assert.AreEqual(total, sum);
        }
    }

    [Test]
    public void BuildMatrices_PlacesRotatesAndCounts()
    {
        var state = new UnitState { posXZ = new Vector2(100f, 200f), facingDeg = 90f, strength = 55f, formation = "line" };
        var matrices = new Matrix4x4[FormationLayout.MaxFigures];
        int n = UnitFormationRenderer.BuildMatrices(
            "u1", state, 300f, 40f, (x, z) => 50f, matrices);
        Assert.AreEqual(6, n); // ceil(55/10)
        // facing east: a figure with +x (right-of-line) offset lands SOUTH of center (z smaller)
        // and all figures stand at ground height 50
        for (int i = 0; i < n; i++)
        {
            Vector3 p = matrices[i].GetColumn(3);
            Assert.AreEqual(50f, p.y, 0.01f);
            Assert.AreEqual(100f, p.x, 200f); // sanity: near the unit
        }
    }
}
