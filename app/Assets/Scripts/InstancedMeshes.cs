using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Procedural low-poly meshes for GPU instancing. Built in code so no asset
    // import pipeline is involved and vertex counts stay honest.
    public static class InstancedMeshes
    {
        // Soldier figures carry vertex colors in three uniform bands —
        // trousers / coat / flesh — multiplied against the per-side
        // _BaseColor by the SoldierVertexTint shader (Assets/Battle): the
        // coat band is white so it takes the side color straight, trousers
        // darken it, the head band warms it. One mesh, same batching,
        // soldiers stop being monochrome boxes (research doc §5). Coat is
        // white=side color; a coat-band multiplier tints BOTH sides'
        // uniforms consistently with the existing block/marker palette.
        // band contrast tuned for legibility at 50-300m: the coat is the
        // side color at full strength, trousers drop to ~40% so the two-tone
        // reads at distance, and the dark kepi over the flesh head is the
        // strongest "this is a person" cue a 30-vert figure can carry
        static readonly Color32 TrouserColor = new Color32(100, 102, 128, 255);
        static readonly Color32 CoatColor = new Color32(255, 255, 255, 255);
        static readonly Color32 FleshColor = new Color32(255, 205, 165, 255);
        static readonly Color32 HatColor = new Color32(58, 58, 66, 255);

        // standing / shoulder-arms pose: trouser legs + tapered coat torso +
        // head + kepi, ~1.8m tall, reads as a human silhouette at 50m+, 32 verts
        public static Mesh BuildSoldier()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var colors = new List<Color32>();
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 0.325f, 0f), new Vector3(0.36f, 0.65f, 0.26f), 1f, 0f, TrouserColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.04f, 0f), new Vector3(0.42f, 0.78f, 0.28f), 0.8f, 0f, CoatColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.55f, 0f), new Vector3(0.24f, 0.24f, 0.24f), 1f, 0f, FleshColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.73f, 0f), new Vector3(0.27f, 0.12f, 0.27f), 1f, 0f, HatColor);
            return BuildColored(verts, tris, colors, "Soldier");
        }

        // advancing pose: the whole figure shears forward along local +Z —
        // the unit-facing convention (BuildMatrices maps formation-forward
        // to +Z), so a moving line visibly leans into its advance. 24 verts.
        public static Mesh BuildSoldierAdvancing()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var colors = new List<Color32>();
            // striding legs: deeper stance, top pushed forward
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 0.325f, 0.02f), new Vector3(0.36f, 0.65f, 0.32f), 1f, 0.12f, TrouserColor);
            // torso leans: sheared toward +Z, carried by the leg shift
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.02f, 0.10f), new Vector3(0.42f, 0.76f, 0.28f), 0.8f, 0.22f, CoatColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.52f, 0.34f), new Vector3(0.24f, 0.24f, 0.24f), 1f, 0f, FleshColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.70f, 0.36f), new Vector3(0.27f, 0.12f, 0.27f), 1f, 0f, HatColor);
            return BuildColored(verts, tris, colors, "SoldierAdvancing");
        }

        // kneeling-firing pose: folded legs, low torso, head at ~1.2m —
        // clearly shorter than the 1.8m standing figure at any distance the
        // soldier tier renders. 24 verts.
        public static Mesh BuildSoldierKneeling()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var colors = new List<Color32>();
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 0.25f, 0.05f), new Vector3(0.40f, 0.50f, 0.48f), 0.9f, 0f, TrouserColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 0.76f, 0f), new Vector3(0.42f, 0.58f, 0.28f), 0.85f, 0f, CoatColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.16f, 0.02f), new Vector3(0.23f, 0.22f, 0.23f), 1f, 0f, FleshColor);
            AddColoredBox(verts, tris, colors,
                new Vector3(0f, 1.32f, 0.03f), new Vector3(0.26f, 0.11f, 0.26f), 1f, 0f, HatColor);
            return BuildColored(verts, tris, colors, "SoldierKneeling");
        }

        // regiment flag: an 8x4-segment quad grid, exactly 45 verts — enough
        // for the Flag shader's vertex wave to bend smoothly, nothing more.
        // Staff edge at x=0 (the shader pins wave amplitude there), 1.8m fly,
        // 1.2m drop hanging below the pole-top pivot. Single-sided geometry;
        // the flag material draws Cull Off, so no duplicated backside verts.
        public static Mesh BuildFlag()
        {
            const int segX = 8, segY = 4;
            const float width = 1.8f, height = 1.2f;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int y = 0; y <= segY; y++)
                for (int x = 0; x <= segX; x++)
                    verts.Add(new Vector3(width * x / segX, -height * y / segY, 0f));
            for (int y = 0; y < segY; y++)
                for (int x = 0; x < segX; x++)
                {
                    int i = y * (segX + 1) + x;
                    int below = i + segX + 1;
                    tris.Add(i); tris.Add(i + 1); tris.Add(below + 1);
                    tris.Add(i); tris.Add(below + 1); tris.Add(below);
                }
            return Build(verts, tris, "Flag");
        }

        // tree, FAR tier: trunk box + two stacked foliage boxes, ~9m tall,
        // 24 verts. Beyond ~1 km a tree is 2-8 px on a phone screen, where
        // opaque geometry beats billboards on TBDR (no alpha, no sorting —
        // research doc 2026-07-02-descriptive-graphics-techniques.md §2b).
        public static Mesh BuildTree()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            AddBox(verts, tris, new Vector3(0f, 1.5f, 0f), new Vector3(0.6f, 3f, 0.6f), 1f);
            AddBox(verts, tris, new Vector3(0f, 5.0f, 0f), new Vector3(4.5f, 4f, 4.5f), 0.55f);
            AddBox(verts, tris, new Vector3(0f, 7.8f, 0f), new Vector3(2.6f, 2.4f, 2.6f), 0.5f);
            return Build(verts, tris, "Tree");
        }

        // tree, NEAR tier (research doc §2b): 6-sided trunk + two squashed
        // canopy blobs in submesh 0, plus a darker UNDERSTORY blob as
        // submesh 1 so VegetationField can draw it with its own darker
        // MaterialPropertyBlock — canopy + understory density is the
        // concealment cue the near tier exists to show. ~140 verts; the
        // deciduous woodlot read is a wide low canopy over a shaded band.
        public static Mesh BuildTreeNear()
        {
            var verts = new List<Vector3>();
            var canopyTris = new List<int>();
            var understoryTris = new List<int>();
            // trunk: 6-sided prism, 0.3m radius tapering to 0.22m at y=3.2
            AddPrism(verts, canopyTris, 0.30f, 0.22f, 0f, 3.2f, 6);
            // main canopy: wide and low, the woodlot silhouette
            AddBlob(verts, canopyTris, new Vector3(0f, 4.8f, 0f), new Vector3(3.4f, 1.9f, 3.4f), 10, 5);
            // upper canopy: smaller crown blob offset upward
            AddBlob(verts, canopyTris, new Vector3(0f, 6.9f, 0f), new Vector3(2.3f, 1.5f, 2.3f), 10, 5);
            // understory: a low shaded band under the canopy (submesh 1)
            AddBlob(verts, understoryTris, new Vector3(0f, 2.9f, 0f), new Vector3(2.6f, 1.1f, 2.6f), 10, 5);
            return BuildTwoSubmeshes(verts, canopyTris, understoryTris, "TreeNear");
        }

        // orchard tree, NEAR tier: rounded lollipop — planted stock, not
        // mature woods. Rendered at OrchardScale by VegetationField's
        // matrices; row legibility comes from the pipeline's regular 8m
        // planting grid, already baked into trees.json. 54 verts.
        public static Mesh BuildOrchardTreeNear()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            AddPrism(verts, tris, 0.18f, 0.14f, 0f, 2.2f, 6);
            AddBlob(verts, tris, new Vector3(0f, 3.6f, 0f), new Vector3(1.8f, 1.6f, 1.8f), 10, 5);
            return Build(verts, tris, "OrchardTreeNear");
        }

        // stone wall segment: a low irregular strip of blocks extending along
        // local +Z from the post position (same 3.0m pipeline spacing and +Z
        // bearing convention as BuildFencePost's rails, so consecutive
        // segments meet). Block heights and lateral offsets are perturbed by
        // the shared FNV hash (FormationLayout.Jitter) — deterministic, so
        // every build yields byte-identical vertices — giving the top edge
        // the broken coursed-stone read instead of a machined rail. The
        // Angle's wall is cover the viewer must read as cover. 32 verts.
        public static Mesh BuildWallSegment()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            const int blocks = 4;
            const float length = 3.0f;
            const float step = length / blocks;
            for (int i = 0; i < blocks; i++)
            {
                // heights 0.55-1.05m (a field wall you'd kneel behind), with
                // small lateral offset and per-block depth wobble
                float h = 0.80f + 0.25f * FormationLayout.Jitter("stone-wall", i, 17);
                float dx = 0.06f * FormationLayout.Jitter("stone-wall", i, 29);
                float w = 0.50f + 0.08f * FormationLayout.Jitter("stone-wall", i, 43);
                // depth slightly over step so blocks overlap instead of gapping
                AddBox(verts, tris,
                    new Vector3(dx, h / 2f, (i + 0.5f) * step),
                    new Vector3(w, h, step + 0.1f), 0.85f);
            }
            return Build(verts, tris, "WallSegment");
        }

        // fence post: a vertical post box + two horizontal rail boxes
        // extending along local +Z from the post, ~1.4m tall, 24 verts (3 boxes x 8).
        // Shared by both stone_wall and rail_fence renders (FenceField):
        // rail geometry reads fine as a low grey box row for stone walls too,
        // so one mesh serves both classes.
        public static Mesh BuildFencePost()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            // post: 0.15 x 1.4 x 0.15, centered so its base sits at y=0
            AddBox(verts, tris, new Vector3(0f, 0.7f, 0f), new Vector3(0.15f, 1.4f, 0.15f), 1f);
            // two rails, 3.0m long, extending along local +Z from the post center.
            // Matches the pipeline's 3.0m post spacing so each post's rails reach
            // the next post and the fence reads as continuous rather than as
            // disconnected sawhorses. Quaternion.Euler(0, bearingDeg, 0) maps
            // local +Z to the compass bearing (the unit-facing / FormationLayout
            // convention), so the rails must run along +Z to align with the fence
            // line rather than perpendicular to it. Note: the last post of each
            // fence run has no following post, so its rails overhang past the run's
            // end by ~1.5m — acceptable for now.
            AddBox(verts, tris, new Vector3(0f, 0.6f, 1.5f), new Vector3(0.08f, 0.08f, 3.0f), 1f);
            AddBox(verts, tris, new Vector3(0f, 1.1f, 1.5f), new Vector3(0.08f, 0.08f, 3.0f), 1f);
            return Build(verts, tris, "FencePost");
        }

        // wheat clump for the soldier-zoom crop ring (CropField): two opaque
        // crossed quads ("X" cards), 0.9m tall — July wheat, ripe/being cut —
        // slightly tapered so the silhouette reads as standing grain. Each
        // card is emitted twice with opposite winding so it shows from both
        // sides without alpha or Cull Off (alpha-tested vegetation defeats
        // TBDR hidden-surface removal — research doc §6 trap #4). 16 verts.
        public static Mesh BuildCropClump()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            AddCropCard(verts, tris, 0f);
            AddCropCard(verts, tris, 90f);
            return Build(verts, tris, "CropClump");
        }

        // one tapered card of the crossed-quad clump, rotated yawDeg about Y,
        // double-sided via duplicated verts (RecalculateNormals would zero
        // out shared verts with opposing faces)
        static void AddCropCard(List<Vector3> verts, List<int> tris, float yawDeg)
        {
            var rot = Quaternion.Euler(0f, yawDeg, 0f);
            Vector3 bl = rot * new Vector3(-0.35f, 0f, 0f);
            Vector3 br = rot * new Vector3(0.35f, 0f, 0f);
            Vector3 tr = rot * new Vector3(0.25f, 0.9f, 0f);
            Vector3 tl = rot * new Vector3(-0.25f, 0.9f, 0f);
            int b = verts.Count;
            verts.Add(bl); verts.Add(br); verts.Add(tr); verts.Add(tl);
            tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
            tris.Add(b); tris.Add(b + 2); tris.Add(b + 3);
            b = verts.Count;
            verts.Add(bl); verts.Add(br); verts.Add(tr); verts.Add(tl);
            tris.Add(b); tris.Add(b + 2); tris.Add(b + 1);
            tris.Add(b); tris.Add(b + 3); tris.Add(b + 2);
        }

        // smoke/dust puff for ObscurationField: an irregular low-poly blob —
        // three overlapping tapered boxes, hash-offset and hash-sized via the
        // shared FNV jitter (deterministic like BuildWallSegment, never
        // Random). Box-composed on purpose: no billboarding, no
        // ParticleSystem, no custom shader — the Commander's Table aesthetic,
        // and plain geometry stays swappable for an opaque-dithered fallback
        // if transparent overdraw bites on device (phase plan, Risks).
        // Unit-ish radius so the renderer's TRS scale IS the puff radius in
        // meters. 24 verts.
        public static Mesh BuildPuff()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                var c = new Vector3(
                    0.5f * FormationLayout.Jitter("puff", i, 17),
                    0.22f * FormationLayout.Jitter("puff", i, 29),
                    0.5f * FormationLayout.Jitter("puff", i, 43));
                float w = 1.1f + 0.3f * FormationLayout.Jitter("puff", i, 61);
                float h = 0.85f + 0.25f * FormationLayout.Jitter("puff", i, 71);
                // per-box taper keeps every silhouette edge off-axis-parallel
                // enough that the blob never reads as a stack of crates
                float taper = 0.55f + 0.2f * FormationLayout.Jitter("puff", i, 83);
                AddBox(verts, tris, c, new Vector3(w, h, w), taper);
            }
            return Build(verts, tris, "Puff");
        }

        // unit box for regiment sub-blocks at the middle LOD tier: 1 x 1 x 1,
        // base at y=0 (centered at y +0.5) so a TRS with position = ground and
        // scale = (width, height, depth) stands the block on the terrain the
        // same way the monolithic unit marker does. 8 verts.
        public static Mesh BuildUnitBox()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            AddBox(verts, tris, new Vector3(0f, 0.5f, 0f), Vector3.one, 1f);
            return Build(verts, tris, "UnitBox");
        }

        // AddBox plus a uniform vertex color for the whole box and an
        // optional shear: topShiftZ pushes the top face along +Z, which is
        // how the pose meshes lean without any extra geometry
        static void AddColoredBox(
            List<Vector3> verts, List<int> tris, List<Color32> colors,
            Vector3 c, Vector3 s, float topScale, float topShiftZ, Color32 color)
        {
            int start = verts.Count;
            AddBox(verts, tris, c, s, topScale);
            if (topShiftZ != 0f)
                for (int i = start + 4; i < start + 8; i++) // AddBox: bottom 4, top 4
                    verts[i] += new Vector3(0f, 0f, topShiftZ);
            for (int i = 0; i < 8; i++) colors.Add(color);
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

        // n-sided open cylinder (no caps — the trunk top hides in the canopy
        // and the base sits in the ground), radius rBottom tapering to rTop
        // between y0 and y1. Winding matches AddBox: for triangle (p0,p1,p2)
        // Unity's normal is cross(p1-p0, p2-p0), so quads emit outward.
        static void AddPrism(
            List<Vector3> verts, List<int> tris,
            float rBottom, float rTop, float y0, float y1, int sides)
        {
            int b = verts.Count;
            for (int s = 0; s < sides; s++)
            {
                float a = Mathf.PI * 2f * s / sides;
                float cos = Mathf.Cos(a), sin = Mathf.Sin(a);
                verts.Add(new Vector3(cos * rBottom, y0, sin * rBottom));
                verts.Add(new Vector3(cos * rTop, y1, sin * rTop));
            }
            for (int s = 0; s < sides; s++)
            {
                int s1 = (s + 1) % sides;
                int lo = b + s * 2, hi = b + s * 2 + 1;
                int lo1 = b + s1 * 2, hi1 = b + s1 * 2 + 1;
                // band quad (upper s, upper s+1, lower s+1, lower s), outward
                tris.Add(hi); tris.Add(lo1); tris.Add(lo);
                tris.Add(hi); tris.Add(hi1); tris.Add(lo1);
            }
        }

        // squashed low-poly UV sphere ("canopy blob") centered at c with
        // per-axis radii: `segments` around, `rings` pole-to-pole bands.
        // verts = segments * (rings - 1) + 2 poles (seg 10 / rings 5 = 42).
        static void AddBlob(
            List<Vector3> verts, List<int> tris,
            Vector3 c, Vector3 radii, int segments, int rings)
        {
            int top = verts.Count;
            verts.Add(c + new Vector3(0f, radii.y, 0f));
            for (int r = 1; r < rings; r++)
            {
                float polar = Mathf.PI * r / rings; // from +Y pole
                float y = Mathf.Cos(polar), ring = Mathf.Sin(polar);
                for (int s = 0; s < segments; s++)
                {
                    float a = Mathf.PI * 2f * s / segments;
                    verts.Add(c + new Vector3(
                        Mathf.Cos(a) * ring * radii.x,
                        y * radii.y,
                        Mathf.Sin(a) * ring * radii.z));
                }
            }
            int bottom = verts.Count;
            verts.Add(c + new Vector3(0f, -radii.y, 0f));

            int Ring(int r, int s) => top + 1 + (r - 1) * segments + s % segments;
            for (int s = 0; s < segments; s++)
            {
                // top fan, band quads, bottom fan — all wound outward per
                // the cross(p1-p0, p2-p0) convention above
                tris.Add(top); tris.Add(Ring(1, s + 1)); tris.Add(Ring(1, s));
                for (int r = 1; r < rings - 1; r++)
                {
                    tris.Add(Ring(r, s)); tris.Add(Ring(r + 1, s + 1)); tris.Add(Ring(r + 1, s));
                    tris.Add(Ring(r, s)); tris.Add(Ring(r, s + 1)); tris.Add(Ring(r + 1, s + 1));
                }
                tris.Add(bottom); tris.Add(Ring(rings - 1, s)); tris.Add(Ring(rings - 1, s + 1));
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

        // Build plus the Color32 channel the SoldierVertexTint shader reads
        static Mesh BuildColored(
            List<Vector3> verts, List<int> tris, List<Color32> colors, string name)
        {
            Mesh m = Build(verts, tris, name);
            m.SetColors(colors);
            return m;
        }

        // two-submesh variant: RenderMeshInstanced draws one submesh per
        // call, so a caller can draw submesh 0 and 1 with the same matrices
        // but different MaterialPropertyBlocks (the understory band).
        static Mesh BuildTwoSubmeshes(
            List<Vector3> verts, List<int> tris0, List<int> tris1, string name)
        {
            var m = new Mesh { name = name };
            m.SetVertices(verts);
            m.subMeshCount = 2;
            m.SetTriangles(tris0, 0);
            m.SetTriangles(tris1, 1);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
