using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // One RenderMeshInstanced call's worth of instances plus the explicit
    // world bounds Unity culls that call against (RenderParams.worldBounds).
    public struct InstanceBatch
    {
        public Matrix4x4[] matrices;
        public Bounds bounds;
    }

    // Groups static instance matrices into spatial grid cells before
    // batching. Graphics.RenderMeshInstanced frustum-culls per CALL, not per
    // instance — "Unity uses the bounds to cull and sort all the instances
    // of this Mesh as a single entity" — so batches built in file order span
    // the whole 8.5 km map and never cull (see
    // docs/research/2026-07-02-descriptive-graphics-techniques.md §2a).
    // Cell-local batches with tight explicit bounds let off-screen cells
    // drop before any vertex work. Runs once at build time; Update() paths
    // stay allocation-free.
    public static class SpatialBatcher
    {
        // 512 m cells over the 8.5 km map -> ~280 cells, per the research
        // report's recommendation. Shared by VegetationField and FenceField.
        public const float CellSize = 512f;

        // Pure and deterministic: instances bucket by floor(x/cellSize),
        // floor(z/cellSize) of their matrix translation; cells emit in
        // (cx, cz) order; file order is preserved within a cell; cells with
        // more than maxPerBatch members split into consecutive chunks. Each
        // batch's bounds are the min/max of member positions expanded by
        // `margin` on every side — pass a conservative mesh-extent margin so
        // geometry hanging off the pivot never pops at the frustum edge.
        public static InstanceBatch[] Build(
            IReadOnlyList<Matrix4x4> matrices, float cellSize, Vector3 margin, int maxPerBatch)
        {
            var cells = new Dictionary<(int cx, int cz), List<int>>();
            for (int i = 0; i < matrices.Count; i++)
            {
                Vector3 p = matrices[i].GetColumn(3);
                var key = (Mathf.FloorToInt(p.x / cellSize), Mathf.FloorToInt(p.z / cellSize));
                if (!cells.TryGetValue(key, out List<int> members))
                {
                    members = new List<int>();
                    cells.Add(key, members);
                }
                members.Add(i);
            }

            // Dictionary iteration order is not contractual, so sort cell
            // keys (ValueTuple compares lexicographically: cx then cz) to
            // keep batch order deterministic across runs.
            var keys = new List<(int cx, int cz)>(cells.Keys);
            keys.Sort();

            var batches = new List<InstanceBatch>();
            foreach ((int cx, int cz) key in keys)
            {
                List<int> members = cells[key];
                for (int start = 0; start < members.Count; start += maxPerBatch)
                {
                    int count = Mathf.Min(maxPerBatch, members.Count - start);
                    var batch = new Matrix4x4[count];
                    Vector3 min = matrices[members[start]].GetColumn(3);
                    Vector3 max = min;
                    for (int j = 0; j < count; j++)
                    {
                        Matrix4x4 m = matrices[members[start + j]];
                        batch[j] = m;
                        Vector3 p = m.GetColumn(3);
                        min = Vector3.Min(min, p);
                        max = Vector3.Max(max, p);
                    }
                    var bounds = new Bounds();
                    bounds.SetMinMax(min - margin, max + margin);
                    batches.Add(new InstanceBatch { matrices = batch, bounds = bounds });
                }
            }
            return batches.ToArray();
        }
    }
}
