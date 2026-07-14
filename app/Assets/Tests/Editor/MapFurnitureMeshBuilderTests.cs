using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class MapFurnitureMeshBuilderTests
{
    static float Flat(float x, float z) => 0f;
    static float Sloped(float x, float z) => x * 0.1f; // 1:10 grade along +x

    [Test]
    public void Resample_PreservesOriginalVerticesAndCapsSegmentLength()
    {
        var points = new List<Vector2> { new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 10) };
        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(points, 30f);

        // original vertices survive exactly
        Assert.AreEqual(points[0], resampled[0]);
        Assert.Contains(points[1], resampled);
        Assert.AreEqual(points[2], resampled[resampled.Count - 1]);

        // no consecutive pair exceeds the cap (within float slop)
        for (int i = 0; i + 1 < resampled.Count; i++)
            Assert.LessOrEqual(Vector2.Distance(resampled[i], resampled[i + 1]), 30f + 1e-3f);
    }

    [Test]
    public void Resample_ShortSegmentIsUntouched()
    {
        var points = new List<Vector2> { new Vector2(0, 0), new Vector2(5, 0) };
        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(points, 30f);
        Assert.AreEqual(2, resampled.Count);
    }

    [Test]
    public void AppendPolyline_ProducesAQuadPerSegmentAtTheRequestedWidth()
    {
        var points = new List<Vector2> { new Vector2(0, 0), new Vector2(10, 0), new Vector2(20, 0) };
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();

        MapFurnitureMeshBuilder.AppendPolyline(points, 4f, 0.1f, dashed: false, Flat, verts, uvs, tris);

        // 2 segments * 4 verts/segment, 2 segments * 6 indices/segment
        Assert.AreEqual(8, verts.Count);
        Assert.AreEqual(12, tris.Count);

        // every vertex sits at the requested lift above flat ground, and
        // within half-width of the (straight, +x) centerline in z
        foreach (Vector3 v in verts)
        {
            Assert.AreEqual(0.1f, v.y, 1e-5f);
            Assert.LessOrEqual(Mathf.Abs(v.z), 2f + 1e-4f);
        }
    }

    [Test]
    public void AppendPolyline_DrapesOntoSlopedGround()
    {
        var points = new List<Vector2> { new Vector2(0, 0), new Vector2(50, 0) };
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolyline(points, 2f, 0.5f, dashed: false, Sloped, verts, uvs, tris);

        foreach (Vector3 v in verts)
            Assert.AreEqual(Sloped(v.x, v.z) + 0.5f, v.y, 1e-4f);
    }

    [Test]
    public void AppendPolyline_TooFewPointsEmitsNothing()
    {
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolyline(
            new List<Vector2> { Vector2.zero }, 2f, 0.1f, false, Flat, verts, uvs, tris);
        Assert.AreEqual(0, verts.Count);
        Assert.AreEqual(0, tris.Count);
    }

    [Test]
    public void AppendPolyline_DashedDropsSomeSegmentsButNotAll()
    {
        // a long straight line, resampled fine, so the dash on/off cycle
        // actually alternates several times
        var points = new List<Vector2> { new Vector2(0, 0), new Vector2(200, 0) };
        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(points, 2f);

        var solidVerts = new List<Vector3>(); var solidUvs = new List<Vector2>(); var solidTris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolyline(resampled, 2f, 0.1f, dashed: false, Flat, solidVerts, solidUvs, solidTris);

        var dashedVerts = new List<Vector3>(); var dashedUvs = new List<Vector2>(); var dashedTris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolyline(resampled, 2f, 0.1f, dashed: true, Flat, dashedVerts, dashedUvs, dashedTris);

        Assert.Greater(dashedVerts.Count, 0, "dashed line must still emit SOME geometry");
        Assert.Less(dashedVerts.Count, solidVerts.Count, "dashing must drop strictly fewer verts than solid");
    }

    [Test]
    public void AppendPolyline_IsDeterministic()
    {
        var points = new List<Vector2> { new Vector2(0, 0), new Vector2(37, 12), new Vector2(80, -5) };
        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(points, 15f);

        var v1 = new List<Vector3>(); var u1 = new List<Vector2>(); var t1 = new List<int>();
        MapFurnitureMeshBuilder.AppendPolyline(resampled, 3f, 0.1f, true, Flat, v1, u1, t1);
        var v2 = new List<Vector3>(); var u2 = new List<Vector2>(); var t2 = new List<int>();
        MapFurnitureMeshBuilder.AppendPolyline(resampled, 3f, 0.1f, true, Flat, v2, u2, t2);

        CollectionAssert.AreEqual(v1, v2);
        CollectionAssert.AreEqual(t1, t2);
    }

    [Test]
    public void AppendPolygonFill_FanTriangulatesEveryEdgeOnce()
    {
        var ring = new List<Vector2> {
            new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10),
        };
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolygonFill(ring, 0.2f, Flat, verts, uvs, tris);

        // 1 centroid + 4 rim verts; 4 triangles (one per edge), 12 indices
        Assert.AreEqual(5, verts.Count);
        Assert.AreEqual(12, tris.Count);
        foreach (Vector3 v in verts) Assert.AreEqual(0.2f, v.y, 1e-5f);

        // centroid vertex (index 0) is the polygon's average, at ground+lift
        Assert.AreEqual(new Vector3(5f, 0.2f, 5f), verts[0]);
    }

    [Test]
    public void AppendPolygonFill_TooFewPointsEmitsNothing()
    {
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolygonFill(
            new List<Vector2> { Vector2.zero, Vector2.one }, 0.2f, Flat, verts, uvs, tris);
        Assert.AreEqual(0, verts.Count);
    }

    [Test]
    public void AppendPolygonOutline_ClosesTheRingWithoutAGap()
    {
        var ring = new List<Vector2> {
            new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10),
        };
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        MapFurnitureMeshBuilder.AppendPolygonOutline(ring, 1f, 0.2f, Flat, verts, uvs, tris);

        // 4 edges when closed (last segment returns to the first point)
        // -> 4 segments * 4 verts, 4 * 6 indices (AppendPolyline's quad-per-segment shape)
        Assert.AreEqual(16, verts.Count);
        Assert.AreEqual(24, tris.Count);
    }

    [Test]
    public void InsetTowardCentroid_ShrinksASquareByTheRequestedMargin()
    {
        var ring = new List<Vector2> {
            new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100),
        };
        List<Vector2> inset = MapFurnitureMeshBuilder.InsetTowardCentroid(ring, 15f);

        Assert.AreEqual(4, inset.Count);
        // centroid (50,50) unchanged; each corner moves 15m toward it
        Assert.AreEqual(new Vector2(50f, 50f),
            (inset[0] + inset[1] + inset[2] + inset[3]) / 4f);
        float cornerDist = Vector2.Distance(ring[0], inset[0]);
        Assert.AreEqual(15f, cornerDist, 1e-3f);
    }

    [Test]
    public void InsetTowardCentroid_CapsAtFortyPercentSoASmallRingCannotInvert()
    {
        var tiny = new List<Vector2> {
            new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10),
        };
        var centroid = new Vector2(5f, 5f);
        float originalDist = Vector2.Distance(tiny[0], centroid); // ~7.07

        List<Vector2> inset = MapFurnitureMeshBuilder.InsetTowardCentroid(tiny, 15f);

        // requested inset (15) exceeds the corner's own distance to the
        // centroid, so the 40% cap must engage: the point ends up CLOSER
        // to the centroid but never past it (never collapses/inverts)
        float newDist = Vector2.Distance(inset[0], centroid);
        Assert.Less(newDist, originalDist);
        Assert.AreEqual(originalDist * 0.6f, newDist, 1e-3f);
        Assert.Greater(newDist, 0f);
    }

    [TestCase("pike", MapFurnitureMeshBuilder.PikeWidthM)]
    [TestCase("road", MapFurnitureMeshBuilder.RoadWidthM)]
    [TestCase("lane", MapFurnitureMeshBuilder.LaneWidthM)]
    [TestCase("unknown-class-falls-back-to-road", MapFurnitureMeshBuilder.RoadWidthM)]
    public void RoadWidthFor_MatchesTheClassGrammar(string cls, float expected)
    {
        Assert.AreEqual(expected, MapFurnitureMeshBuilder.RoadWidthFor(cls));
    }

    [TestCase("creek", MapFurnitureMeshBuilder.CreekWidthM)]
    [TestCase("run", MapFurnitureMeshBuilder.RunWidthM)]
    public void StreamWidthFor_MatchesTheClassGrammar(string cls, float expected)
    {
        Assert.AreEqual(expected, MapFurnitureMeshBuilder.StreamWidthFor(cls));
    }

    [Test]
    public void LiftOrdering_KeepsFurnitureBelowTheUnitSymbolDrapeHeight()
    {
        // declutter interplay: every map-furniture lift must sit strictly
        // below SymbolMeshBuilder.DefaultLiftM, and in the documented
        // fill < stream < rail < road painter's order
        Assert.Less(MapFurnitureMeshBuilder.TownBlockLiftM, MapFurnitureMeshBuilder.StreamLiftM);
        Assert.Less(MapFurnitureMeshBuilder.StreamLiftM, MapFurnitureMeshBuilder.RailLiftM);
        Assert.Less(MapFurnitureMeshBuilder.RailLiftM, MapFurnitureMeshBuilder.RoadLiftM);
        Assert.Less(MapFurnitureMeshBuilder.RoadLiftM, SymbolMeshBuilder.DefaultLiftM);
    }
}
