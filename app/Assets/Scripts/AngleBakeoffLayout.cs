using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Deterministic placeholder staging layout for the Phase 3 URP/HDRP
    // visual-target bake-off (plan §12 Phase 3). Everything is expressed in
    // CROP-LOCAL meters (x east 0..800, z north 0..800 over the §8.1 Angle
    // crop: battlefield-local x=3900..4700, z=4450..5250), and everything is
    // a pure function of constants + the shared FNV jitter — no Random, so
    // both pipelines stage byte-identical content.
    //
    // Positions are BLOCKING-QUALITY placeholders for a lighting/atmosphere
    // comparison, not sourced geometry (that is Phase 5/7 work): the road,
    // wall, copse, and unit lines sit in roughly correct relation to each
    // other; the mid-field "approach fence" exists so the eye-level frame
    // can contain fence, wall, figures, and smoke at readable distances.
    public static class AngleBakeoffLayout
    {
        // Canonical comparison moment: t=8400 on the battle clock. With the
        // July 3 battle's startTime 46800 (13:00 LMT) that is 15:20 LMT —
        // Pickett's division closing on the wall under canister.
        public const float ComparisonBattleT = 8400f;
        public const float BattleStartTime = 46800f;
        public static float SecondsSinceMidnight => BattleStartTime + ComparisonBattleT;

        // --- Static geometry (crop-local meters) ---

        // Emmitsburg Road placeholder: straight NNE strip through the west
        // third of the crop (macro x ~4070..4130).
        public static readonly Vector2 RoadSouth = new Vector2(170f, 0f);
        public static readonly Vector2 RoadNorth = new Vector2(230f, 800f);
        public const float RoadWidth = 9f;
        public const float RoadFenceOffset = 7f; // rail fences flank both sides

        // The Angle stone wall placeholder: N-S run with the 90-degree jog
        // west at its north end (corner at macro ~(4420, 4920)).
        public static readonly Vector2[] WallPolyline =
        {
            new Vector2(520f, 250f),
            new Vector2(520f, 470f),
            new Vector2(440f, 470f),
        };

        // Mid-field rail fence remnant on the approach — placeholder staging
        // so fence + wall + figures share the eye-level frame (Gate P3 frame
        // content), not a sourced feature.
        public static readonly Vector2[] ApproachFence =
        {
            new Vector2(480f, 300f),
            new Vector2(480f, 500f),
        };

        // Copse of Trees placeholder: cluster just east of the wall.
        public static readonly Vector2 CopseCenter = new Vector2(560f, 375f);
        public const int CopseNearTrees = 14;
        public const int CopseFarTrees = 8;

        // Wheat: a band between road and approach (July wheat, standing).
        const float WheatX0 = 250f, WheatX1 = 410f;
        const float WheatZ0 = 290f, WheatZ1 = 510f;
        const float WheatSpacing = 2.2f;

        // --- Sampling helpers ---

        public static Vector2[] RoadSamples(float step = 10f) =>
            SampleLine(RoadSouth, RoadNorth, step);

        static Vector2[] SampleLine(Vector2 a, Vector2 b, float step)
        {
            float len = Vector2.Distance(a, b);
            int n = Mathf.Max(2, Mathf.CeilToInt(len / step) + 1);
            var pts = new Vector2[n];
            for (int i = 0; i < n; i++)
                pts[i] = Vector2.Lerp(a, b, i / (float)(n - 1));
            return pts;
        }

        // Posts (or wall-segment anchors) every `spacing` meters along a
        // polyline, each with the compass bearing of its segment — the
        // convention InstancedMeshes rails/segments extend along local +Z
        // rotated by Quaternion.Euler(0, bearing, 0).
        public static List<(Vector2 pos, float bearingDeg)> PostsAlong(
            Vector2[] polyline, float spacing)
        {
            var result = new List<(Vector2, float)>();
            for (int s = 0; s < polyline.Length - 1; s++)
            {
                Vector2 a = polyline[s], b = polyline[s + 1];
                float len = Vector2.Distance(a, b);
                float bearing = Mathf.Atan2(b.x - a.x, b.y - a.y) * Mathf.Rad2Deg;
                int count = Mathf.FloorToInt(len / spacing);
                for (int i = 0; i < count; i++)
                    result.Add((Vector2.Lerp(a, b, i * spacing / len), bearing));
            }
            return result;
        }

        public static List<(Vector2 pos, float bearingDeg)> WallSegments() =>
            PostsAlong(WallPolyline, 3f);

        public static List<(Vector2 pos, float bearingDeg)> FencePosts()
        {
            var posts = new List<(Vector2, float)>();
            // both sides of the road
            Vector2 dir = (RoadNorth - RoadSouth).normalized;
            Vector2 normal = new Vector2(dir.y, -dir.x); // right of travel (east)
            // inset from the tile edges so the perpendicular offset cannot
            // push endpoint posts outside the crop
            Vector2 south = Vector2.Lerp(RoadSouth, RoadNorth, 0.02f);
            Vector2 north = Vector2.Lerp(RoadSouth, RoadNorth, 0.98f);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector2 off = normal * (RoadWidth / 2f + RoadFenceOffset) * side;
                posts.AddRange(PostsAlong(
                    new[] { south + off, north + off }, 3f));
            }
            posts.AddRange(PostsAlong(ApproachFence, 3f));
            return posts;
        }

        public static List<(Vector2 pos, float yawDeg)> CopseTrees(bool near)
        {
            int count = near ? CopseNearTrees : CopseFarTrees;
            float radius = near ? 28f : 40f;
            string key = near ? "copse-near" : "copse-far";
            var trees = new List<(Vector2, float)>();
            for (int i = 0; i < count; i++)
            {
                var p = CopseCenter + new Vector2(
                    radius * FormationLayout.Jitter(key, i, 17),
                    radius * FormationLayout.Jitter(key, i, 29));
                float yaw = 180f * FormationLayout.Jitter(key, i, 43);
                trees.Add((p, yaw));
            }
            return trees;
        }

        public static List<(Vector2 pos, float yawDeg)> WheatClumps()
        {
            var clumps = new List<(Vector2, float)>();
            int i = 0;
            for (float x = WheatX0; x <= WheatX1; x += WheatSpacing)
                for (float z = WheatZ0; z <= WheatZ1; z += WheatSpacing, i++)
                {
                    var p = new Vector2(
                        x + 0.8f * FormationLayout.Jitter("wheat", i, 17),
                        z + 0.8f * FormationLayout.Jitter("wheat", i, 29));
                    float yaw = 90f * FormationLayout.Jitter("wheat", i, 43);
                    clumps.Add((p, yaw));
                }
            return clumps;
        }

        // --- Figures (100 soldiers total: 80 CSA advancing, 20 USA at wall) ---

        public const int CsaCount = 80;
        public const int UsaCount = 20;

        // tight double-rank line (~0.9 m per file) so the formation reads as
        // a unit rather than a scatter of individuals (plan §4 quality bar)
        static readonly Vector2 CsaCenter = new Vector2(430f, 400f);
        const float CsaFrontage = 72f, CsaDepth = 2.4f, CsaFacingDeg = 90f; // east

        static readonly Vector2 UsaCenter = new Vector2(528f, 400f);
        const float UsaFrontage = 36f, UsaDepth = 1.6f, UsaFacingDeg = 270f; // west

        public static List<(Vector2 pos, float facingDeg)> CsaSoldiers() =>
            LineSoldiers("bakeoff-csa", CsaCenter, CsaFacingDeg, CsaCount, CsaFrontage, CsaDepth);

        public static List<(Vector2 pos, float facingDeg)> UsaSoldiers() =>
            LineSoldiers("bakeoff-usa", UsaCenter, UsaFacingDeg, UsaCount, UsaFrontage, UsaDepth);

        static List<(Vector2, float)> LineSoldiers(
            string unitId, Vector2 center, float facingDeg,
            int count, float frontage, float depth)
        {
            var offsets = FormationLayout.Offsets(unitId, "line", count, frontage, depth);
            var rot = Quaternion.Euler(0f, facingDeg, 0f);
            var result = new List<(Vector2, float)>(count);
            foreach (var off in offsets)
            {
                var world = rot * new Vector3(off.x, 0f, off.y);
                result.Add((center + new Vector2(world.x, world.z), facingDeg));
            }
            return result;
        }

        // --- Smoke (deterministic puff field at the comparison moment) ---

        public static List<(Vector2 pos, float radius)> SmokePuffs()
        {
            var puffs = new List<(Vector2, float)>();
            // musketry bank along the wall's defended face
            for (int i = 0; i < 14; i++)
            {
                var p = new Vector2(
                    512f + 5f * FormationLayout.Jitter("smoke-wall", i, 17),
                    300f + i * 12f + 4f * FormationLayout.Jitter("smoke-wall", i, 29));
                puffs.Add((p, 2.8f + 0.9f * FormationLayout.Jitter("smoke-wall", i, 43)));
            }
            // battery smoke behind the wall (Cushing's section placeholder)
            for (int i = 0; i < 4; i++)
            {
                var p = new Vector2(
                    545f + 8f * FormationLayout.Jitter("smoke-battery", i, 17),
                    428f + 10f * FormationLayout.Jitter("smoke-battery", i, 29));
                puffs.Add((p, 5.5f + 1.5f * FormationLayout.Jitter("smoke-battery", i, 43)));
            }
            // drifting mid-field haze between fence and wall
            for (int i = 0; i < 6; i++)
            {
                var p = new Vector2(
                    478f + 18f * FormationLayout.Jitter("smoke-drift", i, 17),
                    390f + 55f * FormationLayout.Jitter("smoke-drift", i, 29));
                puffs.Add((p, 3.5f + 1.2f * FormationLayout.Jitter("smoke-drift", i, 43)));
            }
            return puffs;
        }

        // --- Cameras (crop-local; heights are meters above sampled ground) ---

        public struct CameraDef
        {
            public string name;
            public Vector2 posXZ;
            public float heightAboveGround;
            public Vector2 lookXZ;
            public float lookHeightAboveGround;
            public float fovDeg;
        }

        // Theater: the whole crop from the SW, high oblique — the macro
        // context frame. Tactical: the assault corridor at drone height.
        // Eye: in the Confederate line at viewpoint eye height (§6.5
        // eyeHeightM 1.66, fovDeg 68) looking at fence, wall, copse, smoke.
        public static readonly CameraDef[] Cameras =
        {
            new CameraDef
            {
                name = "theater",
                posXZ = new Vector2(110f, 130f), heightAboveGround = 430f,
                lookXZ = new Vector2(470f, 400f), lookHeightAboveGround = 5f,
                fovDeg = 50f,
            },
            new CameraDef
            {
                name = "tactical",
                posXZ = new Vector2(290f, 290f), heightAboveGround = 45f,
                lookXZ = new Vector2(515f, 415f), lookHeightAboveGround = 3f,
                fovDeg = 45f,
            },
            new CameraDef
            {
                // rear-left flank of the CSA line (center x=430, z 364..436):
                // the line crosses the frame obliquely while fence, wall
                // corner, smoke bank, and copse stay visible mid-frame
                name = "eye",
                posXZ = new Vector2(417f, 360f), heightAboveGround = 1.66f,
                lookXZ = new Vector2(513f, 408f), lookHeightAboveGround = 1.5f,
                fovDeg = 68f,
            },
        };
    }
}
