using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Builds the draped map-furniture geometry (roads, hydrology, railroad,
    // town-block massing) for the Atlas cartography layer: terrain-draped
    // polyline ribbons and flat-fill polygon footprints, in the same
    // "sample the displayed terrain, drape a grid onto it" idiom as
    // SymbolMeshBuilder -- but built ONCE at load (this layer is static;
    // period roads and streams do not move), not rebuilt per frame, so
    // this class works against growable Lists rather than SymbolMeshBuilder's
    // caller-owned fixed buffers.
    //
    // Layer order (declutter interplay, docs/reconstruction/cartography-
    // slice.md's grammar): map furniture must read UNDER unit ribbons and
    // labels. Unit symbols drape at SymbolMeshBuilder.DefaultLiftM (0.4 m);
    // every lift constant here sits well below that, in a fixed painter's
    // order (fills lowest, then hydrology, then rail, then roads):
    // TownBlockLiftM(0.05) < StreamLiftM(0.08) < RailLiftM(0.11) <
    // RoadLiftM(0.12) -- so ordinary depth testing keeps furniture ink
    // beneath the unit/label layer at every altitude without any render-
    // queue tricks, matching the drape-height convention already
    // established by DefaultLiftM.
    public static class MapFurnitureMeshBuilder
    {
        public const float TownBlockLiftM = 0.05f;
        public const float StreamLiftM = 0.08f;
        public const float RailLiftM = 0.11f;
        public const float RoadLiftM = 0.12f;

        // Road-class line weights (pike widest, lane narrowest -- the
        // period map-grammar convention carried into ribbon width).
        public const float PikeWidthM = 5f;
        public const float RoadWidthM = 3.5f;
        public const float LaneWidthM = 2f;

        // Stream-class widths (named "Creek" reads wider than named "Run").
        public const float CreekWidthM = 5f;
        public const float RunWidthM = 2.5f;

        public const float RailWidthM = 4f;
        public const float TownBlockOutlineWidthM = 1.5f;

        // Adjacent traced block-cluster quads share an edge (the ledger's
        // clusters were read off the sheet's own street-grid lines, so a
        // cluster's boundary IS the next cluster's boundary) -- rendered
        // edge-to-edge with no gap, that reads as one solid mass instead of
        // "blocks separated by streets." InsetTowardCentroid pulls each
        // fill/outline in by this many meters (a rough half-street-width)
        // so the traced DATA stays the honest full-extent quad (to the
        // street centerline, the standard block-boundary convention) while
        // the RENDERED footprint leaves a visible street gap between
        // neighbors.
        public const float TownBlockInsetM = 15f;

        // Adaptive resample: no source segment is trusted to drape straight
        // over more than this many meters of relief before an extra sample
        // is inserted -- Culp's Hill/Cemetery Ridge read correctly under
        // the ribbon instead of the mesh cutting through the hill.
        public const float MaxSegmentLengthM = 30f;

        // rail_unfinished dash pattern: a graded-not-railed cut reads as
        // dashed ink, distinct from the finished branch's solid line --
        // same honesty convention as SymbolMeshBuilder's fragmented/dashed
        // borders, but a fixed arc-length pattern (not per-unit hashed --
        // there is exactly one unfinished-rail feature, not many units).
        public const float DashOnM = 8f;
        public const float DashOffM = 5f;

        // Resamples a traced polyline so no segment exceeds
        // MaxSegmentLengthM, preserving every original vertex (bend/
        // junction fidelity) and inserting evenly-spaced points along
        // longer runs (drape fidelity).
        public static List<Vector2> Resample(IReadOnlyList<Vector2> points, float maxSegLen)
        {
            var result = new List<Vector2> { points[0] };
            for (int i = 0; i + 1 < points.Count; i++)
            {
                Vector2 a = points[i], b = points[i + 1];
                float len = Vector2.Distance(a, b);
                int steps = Mathf.Max(1, Mathf.CeilToInt(len / maxSegLen));
                for (int s = 1; s <= steps; s++)
                    result.Add(Vector2.Lerp(a, b, s / (float)steps));
            }
            return result;
        }

        // Appends a draped ribbon following `points` (already resampled by
        // the caller if drape fidelity matters), `widthM` across, into the
        // growable buffers. `dashed`: whole resampled segments are dropped
        // in a fixed on/off arc-length pattern (rail_unfinished); solid
        // otherwise. Butt joints at each sample (no miter) -- acceptable at
        // these widths (2-5 m) against the sheet's own ~31 m uncertainty.
        public static void AppendPolyline(
            IReadOnlyList<Vector2> points, float widthM, float liftM, bool dashed,
            Func<float, float, float> groundY,
            List<Vector3> verts, List<Vector2> uvs, List<int> tris)
        {
            if (points.Count < 2) return;
            float halfW = widthM / 2f;
            float dashPos = 0f;
            bool dashOn = true;

            for (int i = 0; i + 1 < points.Count; i++)
            {
                Vector2 a = points[i], b = points[i + 1];
                float segLen = Vector2.Distance(a, b);
                bool emit = true;
                if (dashed)
                {
                    emit = dashOn;
                    dashPos += segLen;
                    float period = DashOnM + DashOffM;
                    float phase = dashPos % period;
                    dashOn = phase < DashOnM;
                }
                if (!emit || segLen < 1e-6f) continue;

                Vector2 dir = (b - a).normalized;
                Vector2 perp = new Vector2(-dir.y, dir.x) * halfW;

                int baseVert = verts.Count;
                Vector2 a0 = a - perp, a1 = a + perp;
                Vector2 b0 = b - perp, b1 = b + perp;
                AddVert(verts, uvs, a0, 0f, groundY, liftM);
                AddVert(verts, uvs, a1, 1f, groundY, liftM);
                AddVert(verts, uvs, b0, 0f, groundY, liftM);
                AddVert(verts, uvs, b1, 1f, groundY, liftM);

                tris.Add(baseVert); tris.Add(baseVert + 1); tris.Add(baseVert + 2);
                tris.Add(baseVert + 2); tris.Add(baseVert + 1); tris.Add(baseVert + 3);
            }
        }

        static void AddVert(List<Vector3> verts, List<Vector2> uvs, Vector2 xz,
            float u, Func<float, float, float> groundY, float liftM)
        {
            verts.Add(new Vector3(xz.x, groundY(xz.x, xz.y) + liftM, xz.y));
            uvs.Add(new Vector2(u, 0f));
        }

        // Draped fill for a town-block footprint: fan triangulation from
        // the polygon's centroid. Traced block quads are convex by
        // construction (docs/reconstruction/map-furniture-slice.md "town
        // massing simplification"); a fan is exact for a convex ring and
        // is the same low-cost primitive CropField-adjacent code favors
        // over a general ear-clipping triangulator this layer doesn't need.
        public static void AppendPolygonFill(
            IReadOnlyList<Vector2> ring, float liftM,
            Func<float, float, float> groundY,
            List<Vector3> verts, List<Vector2> uvs, List<int> tris)
        {
            if (ring.Count < 3) return;
            Vector2 centroid = Vector2.zero;
            foreach (Vector2 p in ring) centroid += p;
            centroid /= ring.Count;

            int centerIdx = verts.Count;
            AddVert(verts, uvs, centroid, 0.5f, groundY, liftM);
            int firstRim = verts.Count;
            for (int i = 0; i < ring.Count; i++)
                AddVert(verts, uvs, ring[i], 0f, groundY, liftM);

            for (int i = 0; i < ring.Count; i++)
            {
                int a = firstRim + i;
                int b = firstRim + (i + 1) % ring.Count;
                tris.Add(centerIdx); tris.Add(a); tris.Add(b);
            }
        }

        // Closed-ring outline stroke for a town block: reuses the polyline
        // ribbon primitive on the ring + its own first point appended at
        // the end, so the stroke closes without a gap.
        public static void AppendPolygonOutline(
            IReadOnlyList<Vector2> ring, float widthM, float liftM,
            Func<float, float, float> groundY,
            List<Vector3> verts, List<Vector2> uvs, List<int> tris)
        {
            if (ring.Count < 3) return;
            var closed = new List<Vector2>(ring.Count + 1);
            closed.AddRange(ring);
            closed.Add(ring[0]);
            List<Vector2> resampled = Resample(closed, MaxSegmentLengthM);
            AppendPolyline(resampled, widthM, liftM, false, groundY, verts, uvs, tris);
        }

        // Pulls each ring vertex toward the polygon's centroid by up to
        // insetM meters (capped at 40% of that vertex's own distance to
        // the centroid, so a small block can't invert/collapse) -- a
        // simple radial inset, not a true polygon offset, but exact enough
        // for the traced blocks' near-convex quads. See TownBlockInsetM.
        public static List<Vector2> InsetTowardCentroid(IReadOnlyList<Vector2> ring, float insetM)
        {
            Vector2 centroid = Vector2.zero;
            foreach (Vector2 p in ring) centroid += p;
            centroid /= ring.Count;

            var result = new List<Vector2>(ring.Count);
            foreach (Vector2 p in ring)
            {
                Vector2 toCenter = centroid - p;
                float dist = toCenter.magnitude;
                if (dist < 1e-4f) { result.Add(p); continue; }
                float shrink = Mathf.Min(insetM, dist * 0.4f);
                result.Add(p + toCenter / dist * shrink);
            }
            return result;
        }

        public static float RoadWidthFor(string cls) => cls switch
        {
            "pike" => PikeWidthM,
            "road" => RoadWidthM,
            "lane" => LaneWidthM,
            _ => RoadWidthM,
        };

        public static float StreamWidthFor(string cls) => cls switch
        {
            "creek" => CreekWidthM,
            "run" => RunWidthM,
            _ => RunWidthM,
        };
    }
}
