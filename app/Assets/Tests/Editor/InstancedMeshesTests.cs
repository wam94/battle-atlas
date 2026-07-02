using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class InstancedMeshesTests
{
    [Test]
    public void SoldierMesh_IsSmallAndValid()
    {
        Mesh m = InstancedMeshes.BuildSoldier();
        Assert.Greater(m.vertexCount, 0);
        Assert.LessOrEqual(m.vertexCount, 120); // stays low-poly
        Assert.Greater(m.bounds.size.y, 1.5f);  // roughly man-height
        Assert.Less(m.bounds.size.y, 2.5f);
        Object.DestroyImmediate(m);
    }

    [Test]
    public void TreeMesh_IsSmallAndValid()
    {
        Mesh m = InstancedMeshes.BuildTree();
        Assert.Greater(m.vertexCount, 0);
        Assert.LessOrEqual(m.vertexCount, 150);
        Assert.Greater(m.bounds.size.y, 6f); // tree-scale
        Object.DestroyImmediate(m);
    }

    [Test]
    public void FencePostMesh_IsSmallAndValid()
    {
        Mesh m = InstancedMeshes.BuildFencePost();
        Assert.Greater(m.vertexCount, 0);
        Assert.LessOrEqual(m.vertexCount, 36); // post + two rails, low-poly
        Assert.Greater(m.bounds.size.y, 1f);   // roughly post-height
        Assert.Less(m.bounds.size.y, 2f);
        Assert.Greater(m.bounds.size.x, 1f);   // rails extend well past the post
        Object.DestroyImmediate(m);
    }
}
