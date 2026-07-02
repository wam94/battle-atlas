using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Procedural low-poly meshes for GPU instancing. Built in code so no asset
    // import pipeline is involved and vertex counts stay honest.
    public static class InstancedMeshes
    {
        // a soldier as a tapered box body + box head: ~1.8m tall, reads as a
        // human silhouette at 50m+, costs 48 vertices
        public static Mesh BuildSoldier()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            // body: 0.45m wide, 0.3m deep, 1.45m tall, slight taper
            AddBox(verts, tris, new Vector3(0f, 0.725f, 0f), new Vector3(0.45f, 1.45f, 0.3f), 0.8f);
            // head: 0.28m cube centered at 1.62m
            AddBox(verts, tris, new Vector3(0f, 1.62f, 0f), new Vector3(0.28f, 0.30f, 0.28f), 1f);
            return Build(verts, tris, "Soldier");
        }

        // tree: trunk box + two stacked foliage boxes, ~9m tall
        public static Mesh BuildTree()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            AddBox(verts, tris, new Vector3(0f, 1.5f, 0f), new Vector3(0.6f, 3f, 0.6f), 1f);
            AddBox(verts, tris, new Vector3(0f, 5.0f, 0f), new Vector3(4.5f, 4f, 4.5f), 0.55f);
            AddBox(verts, tris, new Vector3(0f, 7.8f, 0f), new Vector3(2.6f, 2.4f, 2.6f), 0.5f);
            return Build(verts, tris, "Tree");
        }

        // fence post: a vertical post box + two horizontal rail boxes
        // extending +x from the post, ~1.4m tall, 24 verts (3 boxes x 8).
        // Shared by both stone_wall and rail_fence renders (FenceField):
        // rail geometry reads fine as a low grey box row for stone walls too,
        // so one mesh serves both classes.
        public static Mesh BuildFencePost()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            // post: 0.15 x 1.4 x 0.15, centered so its base sits at y=0
            AddBox(verts, tris, new Vector3(0f, 0.7f, 0f), new Vector3(0.15f, 1.4f, 0.15f), 1f);
            // two rails, 1.5m long, extending +x from the post center
            AddBox(verts, tris, new Vector3(0.75f, 0.6f, 0f), new Vector3(1.5f, 0.08f, 0.08f), 1f);
            AddBox(verts, tris, new Vector3(0.75f, 1.1f, 0f), new Vector3(1.5f, 0.08f, 0.08f), 1f);
            return Build(verts, tris, "FencePost");
        }

        // axis-aligned box centered at c with size s; topScale tapers the top face
        static void AddBox(List<Vector3> verts, List<int> tris, Vector3 c, Vector3 s, float topScale)
        {
            int b = verts.Count;
            Vector3 h = s * 0.5f;
            float tx = h.x * topScale, tz = h.z * topScale;
            // bottom 4, top 4
            verts.Add(c + new Vector3(-h.x, -h.y, -h.z));
            verts.Add(c + new Vector3(h.x, -h.y, -h.z));
            verts.Add(c + new Vector3(h.x, -h.y, h.z));
            verts.Add(c + new Vector3(-h.x, -h.y, h.z));
            verts.Add(c + new Vector3(-tx, h.y, -tz));
            verts.Add(c + new Vector3(tx, h.y, -tz));
            verts.Add(c + new Vector3(tx, h.y, tz));
            verts.Add(c + new Vector3(-tx, h.y, tz));
            int[] quads = { 0,1,5,4, 1,2,6,5, 2,3,7,6, 3,0,4,7, 4,5,6,7, 3,2,1,0 };
            for (int q = 0; q < quads.Length; q += 4)
            {
                tris.Add(b + quads[q]); tris.Add(b + quads[q + 2]); tris.Add(b + quads[q + 1]);
                tris.Add(b + quads[q]); tris.Add(b + quads[q + 3]); tris.Add(b + quads[q + 2]);
            }
        }

        static Mesh Build(List<Vector3> verts, List<int> tris, string name)
        {
            var m = new Mesh { name = name };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
