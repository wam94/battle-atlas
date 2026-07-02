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
        // Rails must extend along local +Z, not +X: Quaternion.Euler(0, bearingDeg, 0)
        // maps local +Z to the compass bearing (unit-facing / FormationLayout
        // convention), so a fence post's rails need to run along +Z to align with
        // the fence line instead of sitting perpendicular to it.
        // Rails are 3.0m long (matching the pipeline's 3.0m post spacing) so
        // consecutive posts' rails meet rather than reading as disconnected
        // sawhorses; z extent must clear 2.5m to confirm that reach.
        Assert.Greater(m.bounds.size.z, 2.5f); // rails span the post-to-post spacing, along +Z
        Assert.Less(m.bounds.size.x, 0.5f);    // no extent along +X (perpendicular to bearing)
        Object.DestroyImmediate(m);
    }
}
