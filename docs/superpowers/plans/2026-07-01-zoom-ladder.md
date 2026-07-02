# Zoom Ladder Implementation Plan (Battle Atlas, Phase 4)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Zoom in and brigades resolve from blocks into ranks of instanced soldiers arranged by their formation state, thinning with strength — plus the battle-critical woods — at a fluid frame rate on iPhone.

**Architecture:** All math that determines where soldiers stand is pure C# (`FormationLayout`) and EditMode-tested, including deterministic per-soldier jitter (no `Random` — scrubbing must reproduce identical frames). A per-unit `UnitFormationRenderer` draws soldiers via `Graphics.RenderMeshInstanced` (one call per unit, per-unit color via `MaterialPropertyBlock`), with a procedurally built low-poly soldier mesh. `BattleDirector` gains a distance-based LOD switch with hysteresis: beyond the threshold the existing block renders; inside it, the block hides and ranks appear. Trees are instanced too, planted deterministically inside circles around three landmark anchors by an editor menu tool.

**Tech Stack:** Unity 6 C# (URP, GPU instancing), Unity Test Framework EditMode via MCP/CLI. No pipeline or tool changes.

**Design decisions locked here:**
- **LOD thresholds:** soldiers render when camera-to-unit distance < 1500 m; block when > 1650 m; both states latch (hysteresis band 1500–1650 prevents flicker). One LOD boundary this phase — the spec's four-altitude ladder refines later.
- **Men per figure:** 1 figure represents 10 men (a 1,400-man brigade ≈ 140 figures; whole assault fully zoomed ≈ 2,500 figures worst case — well inside instancing budgets). Figure count = `ceil(strength / 10)`, capped at 400/unit.
- **Formation layouts:** `line` = 2 ranks across full frontage; `column` = frontage/4 wide, correspondingly deep; `skirmish` = 1 rank, double spacing, light jitter; `scattered` = deterministic scatter over 1.5× footprint; `routed` = elongated trail stretching 2× depth rearward of facing with heavy jitter.
- **Determinism:** per-figure jitter from a hash of (unit id, figure index) — identical at any scrub position, forever.
- **Height sampling:** per-figure `Terrain.SampleHeight` each frame. Budget analysis: ≤2,500 figures × 1 call ≈ fine on A-series chips per Phase 1-3 experience; if device perf says otherwise, the documented fallback is per-unit plane fit (3 samples). Measure before optimizing.
- **Material:** reuse the committed `UnitMarker.mat` asset with `enableInstancing` set true in code (asset reference keeps the shader in device builds — the Phase 2 lesson; the flag itself is not stripped).
- **Trees:** cone-on-cylinder instanced mesh; Copse (r=40 m, ~40 trees), Spangler's Woods (r=220 m, ~350), Ziegler's Grove (r=120 m, ~120), centers from docs/research/2026-06-13-landmark-anchors.md. Comment in code: indicative vegetation, NOT surveyed 1863 extents (that's the future land-cover pipeline phase).

**Branch:** `zoom-ladder`. Unity MCP preferred for test runs (editor open); CLI batchmode as fallback (editor closed). Baseline Unity tests: 37.

---

### Task 1: FormationLayout math (TDD)

**Files:**
- Create: `app/Assets/Scripts/FormationLayout.cs`
- Test: `app/Assets/Tests/Editor/FormationLayoutTests.cs`

- [ ] **Step 1:** `git checkout -b zoom-ladder`

- [ ] **Step 2: Failing tests** — `app/Assets/Tests/Editor/FormationLayoutTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class FormationLayoutTests
{
    [Test]
    public void FigureCount_TenMenPerFigure_Capped()
    {
        Assert.AreEqual(140, FormationLayout.FigureCount(1400f));
        Assert.AreEqual(1, FormationLayout.FigureCount(5f));
        Assert.AreEqual(0, FormationLayout.FigureCount(0f));
        Assert.AreEqual(400, FormationLayout.FigureCount(99999f));
    }

    [Test]
    public void Line_TwoRanks_SpansFrontage()
    {
        var offsets = FormationLayout.Offsets("u1", "line", 40, 300f, 40f);
        Assert.AreEqual(40, offsets.Length);
        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var o in offsets)
        {
            if (o.x < minX) minX = o.x;
            if (o.x > maxX) maxX = o.x;
            Assert.LessOrEqual(Mathf.Abs(o.y), 20f + 1f); // within depth envelope
        }
        Assert.Greater(maxX - minX, 250f); // uses most of the 300m frontage
        Assert.LessOrEqual(maxX, 151f);
        Assert.GreaterOrEqual(minX, -151f);
    }

    [Test]
    public void Column_NarrowFront()
    {
        var offsets = FormationLayout.Offsets("u1", "column", 40, 300f, 40f);
        foreach (var o in offsets)
            Assert.LessOrEqual(Mathf.Abs(o.x), 300f / 8f + 1f); // half of frontage/4 + tolerance
    }

    [Test]
    public void Scattered_StaysWithinExpandedFootprint_AndIsDeterministic()
    {
        var a = FormationLayout.Offsets("u1", "scattered", 60, 200f, 30f);
        var b = FormationLayout.Offsets("u1", "scattered", 60, 200f, 30f);
        for (int i = 0; i < a.Length; i++)
        {
            Assert.AreEqual(a[i].x, b[i].x, 1e-5f); // deterministic
            Assert.LessOrEqual(Mathf.Abs(a[i].x), 150f + 1f); // 1.5x half-frontage
            Assert.LessOrEqual(Mathf.Abs(a[i].y), 22.5f + 1f);
        }
        var c = FormationLayout.Offsets("u2", "scattered", 60, 200f, 30f);
        Assert.AreNotEqual(a[0].x, c[0].x); // different unit, different scatter
    }

    [Test]
    public void Routed_TrailsRearward()
    {
        var offsets = FormationLayout.Offsets("u1", "routed", 50, 200f, 30f);
        int behind = 0;
        foreach (var o in offsets) if (o.y < 0f) behind++;
        Assert.Greater(behind, 35); // most figures trail behind the unit center
    }

    [Test]
    public void UnknownFormation_FallsBackToLine()
    {
        var line = FormationLayout.Offsets("u1", "line", 20, 100f, 20f);
        var unknown = FormationLayout.Offsets("u1", "banana", 20, 100f, 20f);
        for (int i = 0; i < line.Length; i++)
            Assert.AreEqual(line[i].x, unknown[i].x, 1e-5f);
    }
}
```

- [ ] **Step 3:** Run Unity tests (MCP `run_tests` if bridge up, else CLI batchmode) — compile failure expected.

- [ ] **Step 4: Implement** — `app/Assets/Scripts/FormationLayout.cs`:

```csharp
using UnityEngine;

namespace BattleAtlas
{
    // Where each rendered soldier stands relative to the unit's center, in the
    // unit's local frame: x along the frontage (right positive), y along depth
    // (forward positive). Pure and deterministic — scrubbing the clock must
    // reproduce identical frames, so jitter derives from a hash, never Random.
    public static class FormationLayout
    {
        public const float MenPerFigure = 10f;
        public const int MaxFigures = 400;

        public static int FigureCount(float strength) =>
            Mathf.Min(MaxFigures, Mathf.CeilToInt(Mathf.Max(0f, strength) / MenPerFigure));

        public static Vector2[] Offsets(
            string unitId, string formation, int count, float frontage, float depth)
        {
            var offsets = new Vector2[count];
            if (count == 0) return offsets;
            switch (formation)
            {
                case "column":
                    FillRanks(offsets, frontage / 4f, depth * 4f);
                    break;
                case "skirmish":
                    FillSkirmish(unitId, offsets, frontage);
                    break;
                case "scattered":
                    FillScatter(unitId, offsets, frontage * 1.5f, depth * 1.5f, 0f);
                    break;
                case "routed":
                    // heavy scatter trailing rearward (negative y) of the facing
                    FillScatter(unitId, offsets, frontage, depth * 4f, -depth * 2f);
                    break;
                default: // "line" and anything unrecognized
                    FillRanks(offsets, frontage, depth);
                    break;
            }
            return offsets;
        }

        // even grid: as many ranks as needed to fit count across the width
        static void FillRanks(Vector2[] offsets, float width, float depth)
        {
            int count = offsets.Length;
            int perRank = Mathf.Max(1, Mathf.CeilToInt(count / Mathf.Max(1f, depth / 10f)));
            perRank = Mathf.Min(perRank, count);
            int ranks = Mathf.CeilToInt((float)count / perRank);
            // for the classic 2-rank line: prefer 2 ranks when they fit
            if (ranks < 2 && count >= 8) { ranks = 2; perRank = Mathf.CeilToInt(count / 2f); }
            for (int i = 0; i < count; i++)
            {
                int rank = i / perRank;
                int file = i % perRank;
                int filesInRank = Mathf.Min(perRank, count - rank * perRank);
                float x = filesInRank <= 1
                    ? 0f
                    : (file / (float)(filesInRank - 1) - 0.5f) * width;
                float y = ranks <= 1 ? 0f : (0.5f - rank / (float)(ranks - 1)) * (depth * 0.8f);
                offsets[i] = new Vector2(x, y);
            }
        }

        static void FillSkirmish(string unitId, Vector2[] offsets, float frontage)
        {
            int count = offsets.Length;
            for (int i = 0; i < count; i++)
            {
                float x = count <= 1 ? 0f : (i / (float)(count - 1) - 0.5f) * frontage * 1.2f;
                offsets[i] = new Vector2(
                    x + Jitter(unitId, i, 11) * 4f,
                    Jitter(unitId, i, 23) * 6f);
            }
        }

        static void FillScatter(
            string unitId, Vector2[] offsets, float width, float depth, float yBias)
        {
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = new Vector2(
                    Jitter(unitId, i, 7) * width * 0.5f,
                    yBias + Jitter(unitId, i, 13) * depth * 0.5f);
        }

        // deterministic pseudo-random in [-1, 1] from (unitId, index, salt)
        static float Jitter(string unitId, int index, int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in unitId) h = (h ^ c) * 16777619u;
                h = (h ^ (uint)index) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return ((h & 0xFFFFFF) / (float)0xFFFFFF) * 2f - 1f;
            }
        }
    }
}
```

- [ ] **Step 5:** Tests pass (37 + 6 = 43). Commit: `feat: deterministic formation layouts for instanced soldiers`

**Worker note:** if `Routed_TrailsRearward` or the line-span assertions fail marginally, adjust the LAYOUT MATH to satisfy the tests' intent (span/trail/caps), not the assertions — the tests encode the visual contract.

---

### Task 2: Soldier + tree meshes (procedural, shared)

**Files:**
- Create: `app/Assets/Scripts/InstancedMeshes.cs`
- Test: `app/Assets/Tests/Editor/InstancedMeshesTests.cs`

- [ ] **Step 1: Failing tests:**

```csharp
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
}
```

- [ ] **Step 2: Implement** — `app/Assets/Scripts/InstancedMeshes.cs`: procedural low-poly meshes built from `CombineMeshes` of primitive-derived pieces is heavyweight; build vertices directly:

```csharp
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
```

- [ ] **Step 3:** Tests pass (45). Commit: `feat: procedural soldier and tree meshes for instancing`

**Worker note:** if triangle winding renders inside-out in the later visual check, flip the two `tris.Add` orderings — the tests don't pin winding, the visual check does.

---

### Task 3: UnitFormationRenderer (instanced ranks)

**Files:**
- Create: `app/Assets/Scripts/UnitFormationRenderer.cs`
- Test: `app/Assets/Tests/Editor/UnitFormationRendererTests.cs` (matrix math only)

- [ ] **Step 1: Failing test** (pure matrix-building helper):

```csharp
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class UnitFormationRendererTests
{
    [Test]
    public void BuildMatrices_PlacesRotatesAndCounts()
    {
        var state = new UnitState { posXZ = new Vector2(100f, 200f), facingDeg = 90f, strength = 55f, formation = "line" };
        var matrices = new Matrix4x4[FormationLayout.MaxFigures];
        int n = UnitFormationRenderer.BuildMatrices(
            "u1", state, 300f, 40f, (x, z) => 50f, matrices);
        Assert.AreEqual(6, n); // ceil(55/10)
        // facing east: a figure with +x (right-of-line) offset lands SOUTH of center (z smaller)
        // and all figures stand at ground height 50
        for (int i = 0; i < n; i++)
        {
            Vector3 p = matrices[i].GetColumn(3);
            Assert.AreEqual(50f, p.y, 0.01f);
            Assert.AreEqual(100f, p.x, 200f); // sanity: near the unit
        }
    }
}
```

- [ ] **Step 2: Implement** — `app/Assets/Scripts/UnitFormationRenderer.cs`:

```csharp
using System;
using UnityEngine;

namespace BattleAtlas
{
    // Draws one unit as instanced soldier figures arranged by formation state.
    // One RenderMeshInstanced call per unit per frame; per-unit color via a
    // MaterialPropertyBlock. Created and driven by BattleDirector.
    public class UnitFormationRenderer
    {
        readonly string unitId;
        readonly float frontage;
        readonly float depth;
        readonly Mesh mesh;
        readonly Material material;
        readonly MaterialPropertyBlock block;
        readonly Matrix4x4[] matrices = new Matrix4x4[FormationLayout.MaxFigures];
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public UnitFormationRenderer(
            string unitId, float frontage, float depth, Mesh mesh, Material material, Color color)
        {
            this.unitId = unitId;
            this.frontage = frontage;
            this.depth = depth;
            this.mesh = mesh;
            this.material = material;
            block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color);
        }

        // Pure: fills matrices, returns count. groundY samples world height at (x, z).
        public static int BuildMatrices(
            string unitId, UnitState state, float frontage, float depth,
            Func<float, float, float> groundY, Matrix4x4[] matrices)
        {
            int count = FormationLayout.FigureCount(state.strength);
            Vector2[] offsets = FormationLayout.Offsets(unitId, state.formation, count, frontage, depth);
            var rot = Quaternion.Euler(0f, state.facingDeg, 0f);
            for (int i = 0; i < count; i++)
            {
                // local (x=right-of-line, y=forward) -> world via facing
                Vector3 world = rot * new Vector3(offsets[i].x, 0f, offsets[i].y);
                float wx = state.posXZ.x + world.x;
                float wz = state.posXZ.y + world.z;
                matrices[i] = Matrix4x4.TRS(
                    new Vector3(wx, groundY(wx, wz), wz), rot, Vector3.one);
            }
            return count;
        }

        public void Render(UnitState state, Func<float, float, float> groundY)
        {
            int count = BuildMatrices(unitId, state, frontage, depth, groundY, matrices);
            if (count == 0) return;
            var rp = new RenderParams(material) { matProps = block };
            Graphics.RenderMeshInstanced(rp, mesh, 0, matrices, count);
        }
    }
}
```

- [ ] **Step 3:** Tests pass (46). Commit: `feat: instanced per-unit formation renderer`

---

### Task 4: LOD switch in BattleDirector

**Files:**
- Modify: `app/Assets/Scripts/BattleDirector.cs`

- [ ] **Step 1:** Read the current file, then:
  - Fields: `public Material soldierMaterial;` (wire UnitMarker.mat in scene — set `enableInstancing` true in `Start` via `soldierMaterial.enableInstancing = true;` and fall back to `unitMaterial` when unset), `const float SoldiersInDist = 1500f; const float BlocksOutDist = 1650f;` (hysteresis), a `Mesh soldierMesh` built once in `Start` from `InstancedMeshes.BuildSoldier()`.
  - Extend the per-unit tuple list to carry a `UnitFormationRenderer` (constructed in `Start` per unit with `SideColor(u.side)`) and a `bool soldiersVisible` latch.
  - In `Update`, per unit: compute `dist = Vector3.Distance(Camera.main.transform.position, markerPos)`; latch `soldiersVisible = dist < SoldiersInDist || (soldiersVisible && dist < BlocksOutDist)`. When soldiers visible: `marker.gameObject.SetActive(false)` and call `renderer.Render(state, (x,z) => terrain.SampleHeight(new Vector3(x,0,z)) + baseY)`; else `SetActive(true)` and pose the block as today. Cache `Camera.main` in `Start` (it's the orbit camera).
- [ ] **Step 2:** All 46 tests still pass (compile check). Commit: `feat: distance LOD — blocks resolve into soldier ranks`

**Worker note:** keep the block-posing code path byte-identical for the far case; the near case skips block posing entirely (no SampleHeight for hidden blocks — the soldier path does its own sampling).

---

### Task 5: Battle-critical woods

**Files:**
- Create: `app/Assets/Scripts/VegetationField.cs`
- Modify: `app/Assets/Editor/HeightmapImporter.cs` (or new menu item file `app/Assets/Editor/VegetationPlanter.cs` — prefer the separate file)
- Test: `app/Assets/Tests/Editor/VegetationFieldTests.cs`

- [ ] **Step 1: Failing test:**

```csharp
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class VegetationFieldTests
{
    [Test]
    public void Placements_DeterministicAndInsideRadius()
    {
        var a = VegetationField.Placements("copse", new Vector2(4407f, 4801f), 40f, 40);
        var b = VegetationField.Placements("copse", new Vector2(4407f, 4801f), 40f, 40);
        Assert.AreEqual(40, a.Length);
        for (int i = 0; i < a.Length; i++)
        {
            Assert.AreEqual(a[i].x, b[i].x, 1e-5f);
            Assert.LessOrEqual(Vector2.Distance(a[i], new Vector2(4407f, 4801f)), 40.01f);
        }
    }
}
```

- [ ] **Step 2: Implement** `VegetationField` (runtime): static `Placements(string id, Vector2 center, float radius, int count)` using `FormationLayout`-style hash jitter mapped into a disc (sqrt-radius for even density); a MonoBehaviour that holds `(center, radius, count)[]` groves — the three from the plan header, coordinates from the landmark anchors doc with a comment citing it — builds the tree mesh + matrices once in `Start` (sampling terrain height per tree), and issues one `RenderMeshInstanced` per grove per frame with a muted green MPB color. Field `public Material treeMaterial;` (UnitMarker.mat again).
- [ ] **Step 3:** `VegetationPlanter.cs` editor menu "BattleAtlas/Add Vegetation" adds the component to the terrain GameObject if absent and wires the material (LoadAssetAtPath UnitMarker.mat).
- [ ] **Step 4:** Tests pass (47). Commit: `feat: indicative woods at the Copse, Spangler's, Ziegler's`

---

### Task 6: Editor verification + scene save

Controller-inline (Unity MCP if bridge available; else user does it manually with controller guidance):
- [ ] Compile + run full EditMode suite (47).
- [ ] Run "BattleAtlas/Add Vegetation"; wire `soldierMaterial` on BattleDirector (UnitMarker.mat).
- [ ] Play mode: scrub to 15:10, fly camera to ~600 m from Garnett's brigade — blocks resolve into two-rank lines advancing; scrub to 15:30 near the Angle — Armistead's figures scattered past the wall; the Copse visible as trees. Screenshot evidence at each.
- [ ] Check `stats_get` draw calls sane (instancing = few calls per unit, not per figure).
- [ ] Save scene; commit scene + any settings churn.

### Task 7: iOS device verification + tag

- [ ] iOS export (MCP manage_build or user via Build Profiles), Xcode ⌘R (user).
- [ ] On-device checklist: zoom from theater height down into a marching brigade — LOD swap smooth, no flicker at the boundary; frame rate fluid at the Angle at 15:25 (the worst case: most units + trees in frustum); thermals acceptable after 5 min.
- [ ] `git tag phase4-zoom-ladder`.

## Done =

Pinch in from the full-map view and Pickett's division stops being geometry: two-rank battle lines of soldiers cross the Emmitsburg Road, Brockenbrough's figures scatter, the ranks thin as strengths fall, the Copse of Trees stands where every man was aiming — at 60 fps on the phone. The spec's "units are symbols" cartography now has its zoomed-in truth.

## Risks

- **RenderMeshInstanced + URP Lit on device:** the material's instancing flag is set in code on a committed asset; if device rendering still misbehaves, fallback is `Graphics.DrawMeshInstanced` (older API, same data) — swap is localized to `UnitFormationRenderer.Render` and `VegetationField`.
- **SampleHeight per figure per frame:** measured, not assumed — Task 7's checklist; documented plane-fit fallback.
- **Visual quality is subjective:** boxes-as-soldiers is the Commander's Table aesthetic on purpose; if it reads as crude at 200 m, the mesh builder is one function to upgrade without touching any other layer.
