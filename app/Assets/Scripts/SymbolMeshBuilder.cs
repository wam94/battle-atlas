using System;
using UnityEngine;

namespace BattleAtlas
{
    // Builds the draped map-symbol geometry (atlas-cartography plan, D1):
    // per unit, a segmented grid conforming to the terrain — every vertex's
    // Y sampled from an injected height func + a small display-meter lift,
    // so the ribbon hugs whatever relief the active terrain displays. A
    // rigid plane cannot lie on ~20 m of Culp's Hill; a sampled grid can,
    // and it stays correct under ANY vertical scale by construction (the
    // height func IS the displayed terrain). Pure statics writing into
    // caller-owned preallocated buffers (the BuildMatrices idiom): no
    // allocation, no growth at render time, deterministic — anything
    // jittered rides FormationLayout.Jitter, never Random.
    //
    // Buffer/submesh contract: verts/uvs fill [0, VertexCount); tris holds
    // the BODY (fill) indices in [0, BodyIndexCount) and the BORDER strip
    // indices in [BodyIndexCount, BodyIndexCount + BorderIndexCount) —
    // Task 4 maps the two ranges to submeshes 0/1 via
    // Mesh.SetTriangles(tris, start, length, submesh), one material for
    // both. UV convention the symbol shader reads: fill vertices carry
    // uv.y in [0, 1] (hatch/formation space, uv.x along the frontage);
    // border vertices carry uv.y in [2, 3] — the border band, where the
    // shader applies the echelon's border color/weight.
    public static class SymbolMeshBuilder
    {
        // grid density along the frontage: ~5 m per vertex column, clamped
        // so a battery frontage still drapes and a corps line stays inside
        // the rebuild budget (64 x 4 = 256 fill verts, plan D1)
        public const float MetersPerColumn = 5f;
        public const int MinColumns = 8;
        public const int MaxColumns = 64;
        public const int RowCount = 4; // vertex rows across the symbol depth

        // ground clearance in DISPLAY meters — hugs without z-fighting.
        // Reviewed at the Atlas's current 2.5x vertical exaggeration; an
        // eventual true-scale terrain (V2 DoD) re-tunes this ONE constant,
        // nothing else in the builder is scale-aware.
        public const float DefaultLiftM = 0.4f;

        public const float BorderWidthM = 2f;     // perimeter/stroke strip width
        public const float CavalryShearDeg = 30f; // the map-symbol slash, structural
        // gun-dot edge length: raised 6 -> 9 m for the cartography slice —
        // at theater zoom a battery must still read as a beaded row, and a
        // 6 m dot fell below one pixel before the field fit the frame
        public const float GunDotSizeM = 9f;
        public const float BaselineWidthM = 1.5f; // artillery baseline stroke
        public const float BaselineGapM = 2.5f;   // dot row -> baseline, baseline -> baseline
        // skirmish dotted line: matches FillSkirmish's frontage x 1.2 spread
        public const float SkirmishSpreadMul = 1.2f;
        public const float SkirmishDotLenM = 3f;
        public const float SkirmishDotDepthM = 1.5f;
        public const float SkirmishDotSpacingM = 10f;
        public const int MinSkirmishDots = 4;
        public const int MaxSkirmishDots = 32;
        // scattered/routed fragment gaps: the fraction of fill cells (and
        // dashed-border segments) dropped — a dissolving unit must not
        // read as an ordered one
        public const float FragmentGapFraction = 0.35f;
        const int FillGapSalt = 41;   // FormationLayout.Jitter salts; border
        const int BorderGapSalt = 43; // strips add their edge index

        // border band marker: border-strip uv.y runs [BorderBandUvY,
        // BorderBandUvY + 1] across the strip width
        public const float BorderBandUvY = 2f;

        // solid-ink band (cartography slice 3): uv.y at/above this renders
        // as unconditional border-shade ink — no echelon weight clip. The
        // facing chevron and the motion-trail dashes ride it: a brigade's
        // double-border rule would hollow a chevron out, and neither mark
        // is an echelon statement.
        public const float InkBandUvY = 4f;

        // facing chevron: a draped "Λ" of two strips at the symbol's
        // leading edge, INSIDE the footprint (extents doctrine: the symbol
        // spans exactly frontage x depth). Ordered formations only —
        // scattered/routed men have no coherent facing to assert.
        public const float ChevronMaxWidthM = 36f;
        public const float ChevronMinWidthM = 8f;
        public const float ChevronWidthFraction = 0.2f;
        public const float ChevronMaxDepthM = 10f;
        public const float ChevronDepthFraction = 0.5f;
        public const float ChevronStrokeM = 2f;
        const int ChevronSegs = 3;

        // motion trail: a dashed wake strip from where the unit stood
        // TrailWindowS battle-seconds ago to its current center — moving
        // units read as moving at every zoom, holding units grow no tail.
        // Deterministic in t (track states), dashes deterministic per unit.
        public const float TrailWindowS = 180f;
        public const float TrailWidthM = 3f;
        public const float TrailSegLenM = 12f;
        public const int TrailMaxSegs = 32;
        const int TrailGapSalt = 53;

        // column formation (slice 3): the monolithic ribbon narrows to the
        // column footprint FormationLayout already uses for figures and
        // roster slots — frontage/4 wide, depth x4 deep — so a marching
        // column stops reading as a deployed line at every tier.
        public const float ColumnFrontageFraction = 0.25f;
        public const float ColumnDepthMultiplier = 4f;

        // The bar grammar's effective footprint for a formation: column
        // narrows and deepens (see above); everything else keeps the
        // attested frontage and strength depth. Artillery and the park
        // never reshape — a battery's dot row is its own grammar. Pure so
        // the picker and the builder can never disagree.
        public static (float frontage, float depth) EffectiveExtents(
            UnitSymbol.SymbolKind kind, string formation,
            float frontage, float displayDepth)
        {
            bool bar = kind == UnitSymbol.SymbolKind.Infantry
                || kind == UnitSymbol.SymbolKind.Cavalry;
            if (bar && formation == "column")
                return (frontage * ColumnFrontageFraction,
                    displayDepth * ColumnDepthMultiplier);
            return (frontage, displayDepth);
        }

        // rebuild-predicate epsilons: position mirrors the IsMovingAt
        // epsilon (BattleDirector.MovingEpsilonM — the same "has it really
        // moved" question at the same grain)
        public const float DirtyPosEpsilonM = 0.05f;
        public const float DirtyFacingEpsilonDeg = 0.5f;

        // Buffer capacity, audited worst case: 64x4 fill grid (256) +
        // border frame (2 long strips at 2x64 + 2 end strips at 2x4 = 272)
        // + cavalry stroke (8) totals 536 verts; the cartography slice adds
        // the facing chevron (2 strips at 2x4 = 16) and the motion trail
        // (<= 2x33 = 66) for <= 618; the park and an 8-dot battalion come
        // in under it. Headroom to 1024 covers the Task 4 skirt without a
        // resize. Indices worst case ~1944 (cavalry) + chevron 36 + trail
        // 192 ~= 2172; 6144 leaves the same headroom. Never grown at render
        // time — BuildRibbon asserts.
        public const int MaxSymbolVerts = 1024;
        public const int MaxSymbolIndices = 6144;

        // vertex-column count along the frontage
        public static int ColumnCount(float frontage) =>
            Mathf.Clamp(Mathf.RoundToInt(frontage / MetersPerColumn),
                MinColumns, MaxColumns);

        // What BuildRibbon wrote: vertex total plus the two index ranges in
        // the shared tris buffer (body first, border appended after it).
        public readonly struct SymbolCounts
        {
            public readonly int VertexCount;
            public readonly int BodyIndexCount;
            public readonly int BorderIndexCount;

            public SymbolCounts(int vertexCount, int bodyIndexCount, int borderIndexCount)
            {
                VertexCount = vertexCount;
                BodyIndexCount = bodyIndexCount;
                BorderIndexCount = borderIndexCount;
            }
        }

        // One unit's draped symbol. centerXZ/facingDeg/frontage are the
        // attested state; displayDepth is UnitSymbol.DisplayDepth's
        // strength thickness (ignored by the artillery grammar — there the
        // gun-dot count, precomputed by the caller via
        // UnitSymbol.GunDotCount, encodes strength); groundY samples the
        // DISPLAYED terrain height at world (x, z); lift is the
        // display-meter clearance (DefaultLiftM). unitId feeds the
        // deterministic fragment gaps. facingSpine adds the leading-edge
        // chevron (the director's monolithic symbols; roster ribbons skip
        // it — one arrow per brigade, not five); hasTrail appends the
        // dashed motion wake from world-XZ trailFromXZ to the center.
        public static SymbolCounts BuildRibbon(
            string unitId, Vector2 centerXZ, float facingDeg, float frontage,
            float displayDepth, UnitSymbol.SymbolKind kind, string formation,
            int gunDots, Func<float, float, float> groundY, float lift,
            Vector3[] verts, Vector2[] uvs, int[] tris,
            bool facingSpine = false, bool hasTrail = false,
            Vector2 trailFromXZ = default)
        {
            var rot = Quaternion.Euler(0f, facingDeg, 0f);
            int vertCount = 0, indexCount = 0;
            (float effF, float effD) = EffectiveExtents(
                kind, formation, frontage, displayDepth);
            float halfF = effF / 2f;
            float halfD = effD / 2f;
            int cols = ColumnCount(effF);
            bool fragmented = formation == "scattered" || formation == "routed";
            int bodyIndexCount;

            switch (kind)
            {
                case UnitSymbol.SymbolKind.Artillery:
                {
                    // gun-dot row (body): one small draped grid per gun,
                    // spread evenly along the frontage. Clamped defensively —
                    // the capacity audit assumes MaxGunDots.
                    int dots = Mathf.Clamp(gunDots,
                        UnitSymbol.MinGunDots, UnitSymbol.MaxGunDots);
                    float span = frontage - GunDotSizeM;
                    for (int d = 0; d < dots; d++)
                    {
                        float cx = (d / (float)(dots - 1) - 0.5f) * span;
                        EmitGrid(unitId, 0, false, 3, 3,
                            cx - GunDotSizeM / 2f, cx + GunDotSizeM / 2f,
                            -GunDotSizeM / 2f, GunDotSizeM / 2f, 0f, 0f, 1f,
                            centerXZ, rot, groundY, lift,
                            verts, uvs, tris, ref vertCount, ref indexCount);
                    }
                    bodyIndexCount = indexCount;
                    // baseline stroke(s) under the dots (border submesh):
                    // one for a battery, two for an artillery battalion —
                    // the approved mockup's echelon cue within the arm
                    float z1 = -GunDotSizeM / 2f - BaselineGapM;
                    EmitStrip(unitId, 0, false,
                        new Vector2(-halfF, z1), new Vector2(halfF, z1),
                        BaselineWidthM, cols - 1, centerXZ, rot, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount);
                    if (DoubleBaseline(unitId))
                    {
                        float z2 = z1 - BaselineGapM;
                        EmitStrip(unitId, 0, false,
                            new Vector2(-halfF, z2), new Vector2(halfF, z2),
                            BaselineWidthM, cols - 1, centerXZ, rot, groundY, lift,
                            verts, uvs, tris, ref vertCount, ref indexCount);
                    }
                    break;
                }
                case UnitSymbol.SymbolKind.ArtilleryPark:
                {
                    // the honest non-combat symbol: hollow outline + inset
                    // diagonal cross, NO interior fill tris — the fill's
                    // permanent mute rides the MPB (Task 4), the hollowness
                    // rides the geometry. A park is stores and teams; its
                    // formation string never fragments or shears it.
                    bodyIndexCount = 0;
                    EmitBarBorder(unitId, false, cols, halfF, halfD, 0f,
                        centerXZ, rot, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount);
                    float ix = halfF - BorderWidthM;
                    float iz = halfD - BorderWidthM;
                    EmitStrip(unitId, 0, false,
                        new Vector2(-ix, -iz), new Vector2(ix, iz),
                        BorderWidthM, cols - 1, centerXZ, rot, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount);
                    EmitStrip(unitId, 0, false,
                        new Vector2(-ix, iz), new Vector2(ix, -iz),
                        BorderWidthM, cols - 1, centerXZ, rot, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount);
                    break;
                }
                default: // Infantry, Cavalry — the bar grammar
                {
                    float shear = kind == UnitSymbol.SymbolKind.Cavalry
                        ? Mathf.Tan(CavalryShearDeg * Mathf.Deg2Rad) : 0f;
                    if (formation == "skirmish")
                    {
                        // a thin dotted line spanning frontage x 1.2
                        // (FillSkirmish's spread); a dotted line wears no
                        // border — the dots ARE the whole symbol
                        float span = frontage * SkirmishSpreadMul;
                        int dotN = Mathf.Clamp(
                            Mathf.RoundToInt(span / SkirmishDotSpacingM),
                            MinSkirmishDots, MaxSkirmishDots);
                        float reach = span - SkirmishDotLenM;
                        for (int d = 0; d < dotN; d++)
                        {
                            float cx = (d / (float)(dotN - 1) - 0.5f) * reach;
                            EmitGrid(unitId, 0, false, 2, 2,
                                cx - SkirmishDotLenM / 2f, cx + SkirmishDotLenM / 2f,
                                -SkirmishDotDepthM / 2f, SkirmishDotDepthM / 2f,
                                0f, 0f, 1f, centerXZ, rot, groundY, lift,
                                verts, uvs, tris, ref vertCount, ref indexCount);
                        }
                        bodyIndexCount = indexCount;
                        break;
                    }
                    // fill grid, fragment-gapped when the unit is
                    // dissolving (deterministic per unitId — scrubbing
                    // replays identical gaps)
                    EmitGrid(unitId, FillGapSalt, fragmented, cols, RowCount,
                        -halfF, halfF, -halfD, halfD, shear, 0f, 1f,
                        centerXZ, rot, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount);
                    bodyIndexCount = indexCount;
                    // border frame (dashed while fragmented — same honesty)
                    EmitBarBorder(unitId, fragmented, cols, halfF, halfD, shear,
                        centerXZ, rot, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount);
                    if (kind == UnitSymbol.SymbolKind.Cavalry)
                    {
                        // the center shear-stroke: parallel to the sheared
                        // ends, through the symbol center — the cavalry
                        // slash reads even where the ends are occluded
                        EmitStrip(unitId, 0, false,
                            new Vector2(shear * -halfD, -halfD),
                            new Vector2(shear * halfD, halfD),
                            BorderWidthM, RowCount - 1, centerXZ, rot, groundY, lift,
                            verts, uvs, tris, ref vertCount, ref indexCount);
                    }
                    break;
                }
            }

            // facing chevron (slice 3): bar grammar, ordered formations
            // only — a dotted skirmish line and a dissolving unit assert
            // no arrow. Rides the solid-ink band, so echelon border rules
            // can't hollow it out.
            if (facingSpine
                && (kind == UnitSymbol.SymbolKind.Infantry
                    || kind == UnitSymbol.SymbolKind.Cavalry)
                && (formation == "line" || formation == "column"))
            {
                float shear = kind == UnitSymbol.SymbolKind.Cavalry
                    ? Mathf.Tan(CavalryShearDeg * Mathf.Deg2Rad) : 0f;
                float w = Mathf.Clamp(effF * ChevronWidthFraction,
                    ChevronMinWidthM, ChevronMaxWidthM);
                float zTip = halfD - BorderWidthM * 1.5f;
                float z0 = zTip - Mathf.Min(
                    effD * ChevronDepthFraction, ChevronMaxDepthM);
                var tip = new Vector2(shear * zTip, zTip);
                EmitStrip(unitId, 0, false,
                    new Vector2(-w / 2f + shear * z0, z0), tip,
                    ChevronStrokeM, ChevronSegs, centerXZ, rot, groundY, lift,
                    verts, uvs, tris, ref vertCount, ref indexCount,
                    InkBandUvY);
                EmitStrip(unitId, 0, false,
                    tip, new Vector2(w / 2f + shear * z0, z0),
                    ChevronStrokeM, ChevronSegs, centerXZ, rot, groundY, lift,
                    verts, uvs, tris, ref vertCount, ref indexCount,
                    InkBandUvY);
            }

            // motion trail (slice 3): the dashed wake, any kind — a
            // displacing battery earns its tail exactly like a marching
            // brigade. World-frame endpoints, so no facing rotation.
            if (hasTrail)
            {
                Vector2 back = trailFromXZ - centerXZ;
                float len = back.magnitude;
                if (len > TrailSegLenM)
                {
                    int segs = Mathf.Clamp(
                        Mathf.CeilToInt(len / TrailSegLenM), 2, TrailMaxSegs);
                    EmitStrip(unitId, TrailGapSalt, true,
                        back, Vector2.zero, TrailWidthM, segs,
                        centerXZ, Quaternion.identity, groundY, lift,
                        verts, uvs, tris, ref vertCount, ref indexCount,
                        InkBandUvY);
                }
            }

            Debug.Assert(
                vertCount <= MaxSymbolVerts && indexCount <= MaxSymbolIndices,
                "symbol geometry exceeded its audited worst case");
            return new SymbolCounts(
                vertCount, bodyIndexCount, indexCount - bodyIndexCount);
        }

        // The pure rebuild predicate (plan D1 cost honesty): most of the
        // 190 units are static most of the battle, so the mesh rebuilds
        // ONLY when something a viewer could see changed — position or
        // facing past epsilon, strength across a thickness-quantization
        // step (frontage converts strength to quantized depth), formation,
        // tier, or selection. Everything else keeps last frame's mesh.
        public static bool SymbolDirty(
            UnitState prev, UnitState cur, float frontage,
            BattleDirector.LodTier prevTier, BattleDirector.LodTier tier,
            bool prevSelected, bool selected)
        {
            if (tier != prevTier || selected != prevSelected) return true;
            if (Vector2.Distance(prev.posXZ, cur.posXZ) > DirtyPosEpsilonM) return true;
            if (Mathf.Abs(Mathf.DeltaAngle(prev.facingDeg, cur.facingDeg))
                > DirtyFacingEpsilonDeg) return true;
            if (prev.formation != cur.formation) return true;
            // quantized depths from identical math: exact compare is safe
            return UnitSymbol.DisplayDepth(prev.strength, frontage)
                != UnitSymbol.DisplayDepth(cur.strength, frontage);
        }

        // Artillery battalion baselines: the approved mockup draws a
        // battery's gun-dots over ONE baseline and a battalion's over TWO.
        // The only battalion-grade artillery ids are the csa-bn- battalions
        // — the same LOCKED prefix contract UnitSymbol.KindOf decodes.
        static bool DoubleBaseline(string unitId) =>
            unitId.StartsWith("csa-bn-", StringComparison.Ordinal);

        static bool FragmentGap(string unitId, int cellIndex, int salt) =>
            (FormationLayout.Jitter(unitId, cellIndex, salt) + 1f) * 0.5f
                < FragmentGapFraction;

        // Emits one draped grid: cols x rows vertices spanning [x0,x1] x
        // [z0,z1] in the unit-local frame (x along the frontage, z along
        // depth — FormationLayout's axes), sheared by shearXPerZ (local x
        // offset per unit of local z, the cavalry parallelogram). When
        // fragmented, whole CELL COLUMNS drop deterministically — vertices
        // stay (fixed layout, stable counts), their triangles go.
        static void EmitGrid(string unitId, int gapSalt, bool fragmented,
            int cols, int rows, float x0, float x1, float z0, float z1,
            float shearXPerZ, float uvY0, float uvY1,
            Vector2 centerXZ, Quaternion rot, Func<float, float, float> groundY,
            float lift, Vector3[] verts, Vector2[] uvs, int[] tris,
            ref int vertCount, ref int indexCount)
        {
            int baseVert = vertCount;
            for (int j = 0; j < rows; j++)
            {
                float v = j / (float)(rows - 1);
                float lz = Mathf.Lerp(z0, z1, v);
                for (int i = 0; i < cols; i++)
                {
                    float u = i / (float)(cols - 1);
                    float lx = Mathf.Lerp(x0, x1, u) + lz * shearXPerZ;
                    // local (x=right-of-line, z=forward) -> world via facing
                    Vector3 world = rot * new Vector3(lx, 0f, lz);
                    float wx = centerXZ.x + world.x;
                    float wz = centerXZ.y + world.z;
                    verts[vertCount] = new Vector3(wx, groundY(wx, wz) + lift, wz);
                    uvs[vertCount] = new Vector2(u, Mathf.Lerp(uvY0, uvY1, v));
                    vertCount++;
                }
            }
            for (int j = 0; j < rows - 1; j++)
                for (int i = 0; i < cols - 1; i++)
                {
                    if (fragmented && FragmentGap(unitId, i, gapSalt)) continue;
                    int a = baseVert + j * cols + i;
                    int c = a + cols;
                    tris[indexCount++] = a; tris[indexCount++] = c;
                    tris[indexCount++] = a + 1;
                    tris[indexCount++] = a + 1; tris[indexCount++] = c;
                    tris[indexCount++] = c + 1;
                }
        }

        // Emits one draped strip from local point a to local point b,
        // `width` across, segs cells along — the border-band primitive
        // (uv.y rides [bandUvY, bandUvY+1]; BorderBandUvY unless the caller
        // asks for the solid-ink band). When dashed, whole segments drop
        // via the same deterministic gap rule as the fill.
        static void EmitStrip(string unitId, int gapSalt, bool dashed,
            Vector2 a, Vector2 b, float width, int segs,
            Vector2 centerXZ, Quaternion rot, Func<float, float, float> groundY,
            float lift, Vector3[] verts, Vector2[] uvs, int[] tris,
            ref int vertCount, ref int indexCount, float bandUvY = BorderBandUvY)
        {
            int baseVert = vertCount;
            Vector2 dir = (b - a).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x) * (width / 2f);
            int stripCols = segs + 1;
            for (int r = 0; r < 2; r++)
            {
                float side = r == 0 ? -1f : 1f;
                for (int c = 0; c < stripCols; c++)
                {
                    Vector2 p = Vector2.Lerp(a, b, c / (float)segs) + perp * side;
                    Vector3 world = rot * new Vector3(p.x, 0f, p.y);
                    float wx = centerXZ.x + world.x;
                    float wz = centerXZ.y + world.z;
                    verts[vertCount] = new Vector3(wx, groundY(wx, wz) + lift, wz);
                    uvs[vertCount] = new Vector2(
                        c / (float)segs, bandUvY + r);
                    vertCount++;
                }
            }
            for (int c = 0; c < segs; c++)
            {
                if (dashed && FragmentGap(unitId, c, gapSalt)) continue;
                int v0 = baseVert + c;
                int v1 = baseVert + stripCols + c;
                tris[indexCount++] = v0; tris[indexCount++] = v1;
                tris[indexCount++] = v0 + 1;
                tris[indexCount++] = v0 + 1; tris[indexCount++] = v1;
                tris[indexCount++] = v1 + 1;
            }
        }

        // The bar outline's four strips, inset INSIDE the footprint so the
        // symbol's extents stay exactly frontage x displayDepth: long
        // strips hug the front/rear edges, end strips stop short of them
        // (no coincident corner tris to shimmer), all four following the
        // shear so a cavalry frame is the same parallelogram as its fill.
        static void EmitBarBorder(string unitId, bool dashed, int cols,
            float halfF, float halfD, float shear,
            Vector2 centerXZ, Quaternion rot, Func<float, float, float> groundY,
            float lift, Vector3[] verts, Vector2[] uvs, int[] tris,
            ref int vertCount, ref int indexCount)
        {
            float w = BorderWidthM;
            float zf = halfD - w / 2f; // front/rear strip centerlines
            float zi = halfD - w;      // end strips span between the long ones
            EmitStrip(unitId, BorderGapSalt, dashed,
                new Vector2(-halfF + w + shear * zf, zf),
                new Vector2(halfF - w + shear * zf, zf),
                w, cols - 1, centerXZ, rot, groundY, lift,
                verts, uvs, tris, ref vertCount, ref indexCount);
            EmitStrip(unitId, BorderGapSalt + 1, dashed,
                new Vector2(-halfF + w - shear * zf, -zf),
                new Vector2(halfF - w - shear * zf, -zf),
                w, cols - 1, centerXZ, rot, groundY, lift,
                verts, uvs, tris, ref vertCount, ref indexCount);
            EmitStrip(unitId, BorderGapSalt + 2, dashed,
                new Vector2(-halfF + w / 2f - shear * zi, -zi),
                new Vector2(-halfF + w / 2f + shear * zi, zi),
                w, RowCount - 1, centerXZ, rot, groundY, lift,
                verts, uvs, tris, ref vertCount, ref indexCount);
            EmitStrip(unitId, BorderGapSalt + 3, dashed,
                new Vector2(halfF - w / 2f - shear * zi, -zi),
                new Vector2(halfF - w / 2f + shear * zi, zi),
                w, RowCount - 1, centerXZ, rot, groundY, lift,
                verts, uvs, tris, ref vertCount, ref indexCount);
        }
    }
}
