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
    public void SoldierPoseMeshes_CarryBandedVertexColors()
    {
        // the SoldierVertexTint shader multiplies mesh Color32 bands by the
        // side color — a mesh without distinct bands renders as the flat
        // monochrome slab this test exists to prevent (review follow-up)
        Mesh[] poses =
        {
            InstancedMeshes.BuildSoldier(),
            InstancedMeshes.BuildSoldierAdvancing(),
            InstancedMeshes.BuildSoldierKneeling(),
        };
        foreach (Mesh m in poses)
        {
            Color32[] colors = m.colors32;
            Assert.AreEqual(m.vertexCount, colors.Length, $"{m.name}: color per vertex");
            var distinct = new System.Collections.Generic.HashSet<uint>();
            foreach (Color32 c in colors)
                distinct.Add((uint)(c.r << 16 | c.g << 8 | c.b));
            Assert.GreaterOrEqual(distinct.Count, 4,
                $"{m.name}: trousers/coat/flesh/kepi bands must be distinct");
            Object.DestroyImmediate(m);
        }
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
    public void TreeMeshes_MeetNearAndFarVertBudgets()
    {
        // near tier: 6-sided trunk + canopy blobs + understory band, the
        // research doc's 120-200 vert budget (§2b); understory is submesh 1
        // so VegetationField can draw it darker with the same matrices
        Mesh near = InstancedMeshes.BuildTreeNear();
        Assert.GreaterOrEqual(near.vertexCount, 120);
        Assert.LessOrEqual(near.vertexCount, 200);
        Assert.AreEqual(2, near.subMeshCount);
        Assert.Greater(near.bounds.size.y, 6f); // tree-scale
        // far tier: the existing box-blob tree stays cheap — at >1 km a
        // tree is 2-8 px and geometry beats billboards
        Mesh far = InstancedMeshes.BuildTree();
        Assert.LessOrEqual(far.vertexCount, 40);
        Object.DestroyImmediate(near);
        Object.DestroyImmediate(far);
    }

    [Test]
    public void WallSegmentMesh_IsDeterministicLowIrregularStrip()
    {
        Mesh a = InstancedMeshes.BuildWallSegment();
        Mesh b = InstancedMeshes.BuildWallSegment();
        // hash-perturbed, not random: two builds are vertex-identical
        Assert.AreEqual(a.vertexCount, b.vertexCount);
        Vector3[] va = a.vertices, vb = b.vertices;
        for (int i = 0; i < va.Length; i++)
            Assert.AreEqual(va[i], vb[i]);
        // low (kneel-behind cover, not a building) and spanning the 3.0m
        // post spacing along +Z like the fence rails, with no rail-like
        // reach along +X
        Assert.Less(a.bounds.size.y, 1.5f);
        Assert.Greater(a.bounds.size.y, 0.5f);
        Assert.Greater(a.bounds.size.z, 2.5f);
        Assert.Less(a.bounds.size.x, 1f);
        // the top edge is irregular: block tops sit at differing heights
        float maxY = a.bounds.max.y;
        bool hasLowerTop = false;
        foreach (Vector3 v in va)
            if (v.y > 0.5f && v.y < maxY - 0.05f) hasLowerTop = true;
        Assert.IsTrue(hasLowerTop, "wall top edge should vary in height");
        Object.DestroyImmediate(a);
        Object.DestroyImmediate(b);
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

    [Test]
    public void FlagMesh_MeetsBudget()
    {
        // one 8x4-segment quad grid, exactly 45 verts (the report's flag
        // budget) — instanced across every unit in one call per side, so
        // the per-vertex wave cost is the whole bill
        Mesh m = InstancedMeshes.BuildFlag();
        Assert.AreEqual(45, m.vertexCount);
        // staff edge at x=0, 1.8m fly, hanging below the pole-top pivot
        Assert.AreEqual(0f, m.bounds.min.x, 1e-4f);
        Assert.AreEqual(1.8f, m.bounds.max.x, 1e-4f);
        Assert.LessOrEqual(m.bounds.max.y, 1e-4f);
        Object.DestroyImmediate(m);
    }

    [Test]
    public void PuffMesh_IsDeterministicLowPolyUnitBlob()
    {
        Mesh a = InstancedMeshes.BuildPuff();
        Mesh b = InstancedMeshes.BuildPuff();
        // hash-perturbed, not random: two builds are vertex-identical
        Assert.AreEqual(a.vertexCount, b.vertexCount);
        Vector3[] va = a.vertices, vb = b.vertices;
        for (int i = 0; i < va.Length; i++)
            Assert.AreEqual(va[i], vb[i]);
        // low-poly blob (three tapered boxes), roughly unit radius so the
        // renderer's TRS scale is the puff radius in meters
        Assert.LessOrEqual(a.vertexCount, 120);
        Assert.That(a.bounds.size.x, Is.InRange(0.8f, 3f));
        Assert.That(a.bounds.size.z, Is.InRange(0.8f, 3f));
        Assert.That(a.bounds.size.y, Is.InRange(0.5f, 2.5f));
        Object.DestroyImmediate(a);
        Object.DestroyImmediate(b);
    }
}
