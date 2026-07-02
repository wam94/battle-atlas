using NUnit.Framework;
using UnityEngine;
using BattleAtlas;
using System.Collections.Generic;

public class SpatialBatcherTests
{
    const float Cell = 512f;

    static Matrix4x4 At(float x, float y, float z) =>
        Matrix4x4.TRS(new Vector3(x, y, z), Quaternion.identity, Vector3.one);

    static Vector3 Pos(Matrix4x4 m) => m.GetColumn(3);

    [Test]
    public void Build_GroupsInstancesIntoCorrectCells()
    {
        // two instances in cell (0,0), one in (1,0), one in (0,1)
        var matrices = new List<Matrix4x4>
        {
            At(10f, 0f, 10f),
            At(20f, 0f, 20f),
            At(1000f, 0f, 10f),
            At(10f, 0f, 1000f),
        };
        InstanceBatch[] batches = SpatialBatcher.Build(matrices, Cell, Vector3.zero, 1023);

        // cells sort by (cx, cz): (0,0) then (0,1) then (1,0)
        Assert.AreEqual(3, batches.Length);
        Assert.AreEqual(2, batches[0].matrices.Length);
        Assert.AreEqual(new Vector3(10f, 0f, 10f), Pos(batches[0].matrices[0]));
        Assert.AreEqual(new Vector3(20f, 0f, 20f), Pos(batches[0].matrices[1]));
        Assert.AreEqual(new Vector3(10f, 0f, 1000f), Pos(batches[1].matrices[0]));
        Assert.AreEqual(new Vector3(1000f, 0f, 10f), Pos(batches[2].matrices[0]));
    }

    [Test]
    public void Build_SplitsCellsAtMaxPerBatchKeepingFileOrder()
    {
        // 10 instances in one cell with a cap of 4 -> batches of 4, 4, 2,
        // file order preserved across the splits
        var matrices = new List<Matrix4x4>();
        for (int i = 0; i < 10; i++) matrices.Add(At(i, 0f, 0f));
        InstanceBatch[] batches = SpatialBatcher.Build(matrices, Cell, Vector3.zero, 4);

        Assert.AreEqual(3, batches.Length);
        Assert.AreEqual(4, batches[0].matrices.Length);
        Assert.AreEqual(4, batches[1].matrices.Length);
        Assert.AreEqual(2, batches[2].matrices.Length);
        int n = 0;
        foreach (InstanceBatch batch in batches)
            foreach (Matrix4x4 m in batch.matrices)
                Assert.AreEqual((float)n++, Pos(m).x, 1e-5f);
    }

    [Test]
    public void Build_BoundsContainMemberPositionsPlusMargin()
    {
        var margin = new Vector3(3f, 9f, 3f);
        var matrices = new List<Matrix4x4>
        {
            At(100f, 5f, 200f),
            At(140f, 12f, 260f),
            At(120f, 8f, 230f),
        };
        InstanceBatch[] batches = SpatialBatcher.Build(matrices, Cell, margin, 1023);

        Assert.AreEqual(1, batches.Length);
        Bounds b = batches[0].bounds;
        foreach (Matrix4x4 m in matrices) Assert.IsTrue(b.Contains(Pos(m)));
        // bounds are position min/max expanded by the mesh-size margin
        Assert.AreEqual(new Vector3(100f, 5f, 200f) - margin, b.min);
        Assert.AreEqual(new Vector3(140f, 12f, 260f) + margin, b.max);
    }

    [Test]
    public void Build_IsDeterministic()
    {
        var matrices = new List<Matrix4x4>();
        for (int i = 0; i < 300; i++)
        {
            // scatter across many cells, deterministic input
            float x = (i * 733) % 4096;
            float z = (i * 271) % 4096;
            matrices.Add(At(x, i % 7, z));
        }
        InstanceBatch[] a = SpatialBatcher.Build(matrices, Cell, Vector3.one, 64);
        InstanceBatch[] b = SpatialBatcher.Build(matrices, Cell, Vector3.one, 64);

        Assert.AreEqual(a.Length, b.Length);
        for (int i = 0; i < a.Length; i++)
        {
            Assert.AreEqual(a[i].bounds, b[i].bounds);
            Assert.AreEqual(a[i].matrices.Length, b[i].matrices.Length);
            for (int j = 0; j < a[i].matrices.Length; j++)
                Assert.AreEqual(a[i].matrices[j], b[i].matrices[j]);
        }
    }

    [Test]
    public void Build_CrossCellInstancesNeverShareABatch()
    {
        // file order alternates between two far-apart cells; grouping must
        // separate them, never merge across the cell boundary
        var matrices = new List<Matrix4x4>();
        for (int i = 0; i < 6; i++)
            matrices.Add(At(i % 2 == 0 ? 10f : 600f, 0f, 10f));
        InstanceBatch[] batches = SpatialBatcher.Build(matrices, Cell, Vector3.zero, 1023);

        Assert.AreEqual(2, batches.Length);
        foreach (InstanceBatch batch in batches)
        {
            Assert.AreEqual(3, batch.matrices.Length);
            float x0 = Pos(batch.matrices[0]).x;
            foreach (Matrix4x4 m in batch.matrices)
                Assert.AreEqual(x0, Pos(m).x, 1e-5f);
        }
    }
}
