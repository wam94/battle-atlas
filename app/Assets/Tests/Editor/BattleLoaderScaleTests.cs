using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

// Unit-count scaling sanity at the full-cast target (~210 units; plan
// 2026-07-02-full-cast.md Task 2). Retires the perf risk BEFORE the waves
// author ~127 real units: the synthetic fixture (tool/scripts/
// make-scale-fixture.ts, built with the Task 1 generator lib) is parsed,
// tracked, and driven through BattleDirector's real Start/Update headlessly.
// The fixture is committed and stays as the regression pin.
public class BattleLoaderScaleTests
{
    const string FixturePath = "Assets/Tests/Editor/Fixtures/scale-fixture-210.json";
    const int FixtureUnits = 210;
    const int FixtureEvents = 40;

    static BattleDto LoadFixture()
    {
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(FixturePath);
        Assert.IsNotNull(asset, FixturePath + " missing — regenerate with " +
            "`npx vite-node scripts/make-scale-fixture.ts` from tool/");
        return BattleLoader.Parse(asset.text);
    }

    [Test]
    public void Parse_ScaleFixture_LoadsAllUnitsAndEvents()
    {
        BattleDto b = LoadFixture(); // loader throws nothing at 210 units
        Assert.AreEqual(FixtureUnits, b.units.Count);
        Assert.AreEqual(FixtureEvents, b.events.Count);
        foreach (var u in b.units)
        {
            StringAssert.Contains("Test", u.name, "scale fixture must be obviously fake");
            foreach (var k in u.keyframes)
            {
                Assert.That(k.x, Is.InRange(500f, 8000f), u.id + " x out of range");
                Assert.That(k.z, Is.InRange(500f, 8000f), u.id + " z out of range");
            }
        }
    }

    [Test]
    public void StateAt_All210UnitsAt100SampledTs_AllocationSaneAndClampCorrect()
    {
        BattleDto b = LoadFixture();
        var tracks = new UnitTrack[b.units.Count];
        for (int i = 0; i < b.units.Count; i++) tracks[i] = new UnitTrack(b.units[i]);

        // clamp edges: before the first keyframe and after the last, StateAt
        // returns exactly that keyframe's pose (movers' tracks start at t=600
        // and end at t=10500, so both clamps are exercised inside the window)
        foreach (var track in tracks)
        {
            var first = track.Unit.keyframes[0];
            var last = track.Unit.keyframes[track.Unit.keyframes.Count - 1];
            UnitState before = track.StateAt(first.t - 100f);
            Assert.AreEqual(first.x, before.posXZ.x, 1e-4f, track.Unit.id);
            Assert.AreEqual(first.z, before.posXZ.y, 1e-4f, track.Unit.id);
            Assert.AreEqual(first.strength, before.strength, 1e-4f, track.Unit.id);
            UnitState after = track.StateAt(last.t + 100f);
            Assert.AreEqual(last.x, after.posXZ.x, 1e-4f, track.Unit.id);
            Assert.AreEqual(last.z, after.posXZ.y, 1e-4f, track.Unit.id);
            Assert.AreEqual(last.strength, after.strength, 1e-4f, track.Unit.id);
        }

        // allocation sanity over the per-frame access pattern: all 210 tracks
        // sampled at 100 ts spanning (and overshooting) the window. UnitState
        // is a struct and formation is a shared string reference, so the
        // expected heap traffic is ZERO after warmup.
        float sum = 0f;
        foreach (var track in tracks) sum += track.StateAt(5400f).strength; // JIT warmup
        long before2 = System.GC.GetAllocatedBytesForCurrentThread();
        for (int s = 0; s < 100; s++)
        {
            float t = -200f + s * (11200f / 99f); // sweeps past both clamp edges
            for (int i = 0; i < tracks.Length; i++) sum += tracks[i].StateAt(t).strength;
        }
        long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before2;
        Debug.Log($"[scale] StateAt 210x100: {allocated} bytes allocated (checksum {sum})");
        Assert.LessOrEqual(allocated, 4096,
            "StateAt allocated on the hot path — it must stay allocation-free");
    }

    [Test]
    public void DirectorStart_ScaleFixture_BuffersScaleWithUnitCountAndFrameLoopIsCheap()
    {
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(FixturePath);
        Assert.IsNotNull(asset);

        var terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
        terrainData.size = new Vector3(8507f, 30f, 8507f);
        GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        var directorGO = new GameObject("scale test director");
        var material = new Material(Shader.Find("Standard"));
        try
        {
            var clock = directorGO.AddComponent<BattleClock>();
            var director = directorGO.AddComponent<BattleDirector>();
            director.battleJson = asset;
            director.terrain = terrainGO.GetComponent<Terrain>();
            director.clock = clock;
            director.unitMaterial = material;
            // flag/smoke/dust/symbol materials stay null: RenderFlags warns
            // once and skips, symbols warn once and fall back to
            // unitMaterial, obscuration never Updates in edit mode — the
            // measured loop is exactly the symbol path (StateAt + tier eval
            // + dirty check + ribbon rebuild/draw for every parentless unit)

            const BindingFlags priv = BindingFlags.Instance | BindingFlags.NonPublic;
            var start = (System.Action)System.Delegate.CreateDelegate(typeof(System.Action),
                director, typeof(BattleDirector).GetMethod("Start", priv));
            var update = (System.Action)System.Delegate.CreateDelegate(typeof(System.Action),
                director, typeof(BattleDirector).GetMethod("Update", priv));

            var sw = System.Diagnostics.Stopwatch.StartNew();
            start();
            sw.Stop();
            double startMs = sw.Elapsed.TotalMilliseconds;

            // entry count == unit count; symbol meshes are LAZY (built on
            // first render at a symbol tier), so none exist yet — the
            // post-loop assertion below is the one that proves they appear
            var unitsList = (System.Collections.IList)
                typeof(BattleDirector).GetField("units", priv).GetValue(director);
            Assert.AreEqual(FixtureUnits, unitsList.Count);
            FieldInfo symbolMeshField = unitsList[0].GetType().GetField("SymbolMesh");
            foreach (object entry in unitsList)
                Assert.IsNull((Mesh)symbolMeshField.GetValue(entry),
                    "symbol meshes must be lazy — Start builds none");

            // flag matrix capacity == unit count — the regression pin: the
            // buffers are sized from units.Count in Start and must keep
            // auto-scaling; nobody gets to hardcode a capacity later
            var unionFlags = (Matrix4x4[])
                typeof(BattleDirector).GetField("unionFlagMatrices", priv).GetValue(director);
            var csaFlags = (Matrix4x4[])
                typeof(BattleDirector).GetField("csaFlagMatrices", priv).GetValue(director);
            Assert.AreEqual(FixtureUnits, unionFlags.Length);
            Assert.AreEqual(FixtureUnits, csaFlags.Length);

            // per-frame loop: 100 frames sweeping the whole window at Block
            // tier (no camera => far tier — the 210-symbol strategic-zoom
            // worst case; every mover crosses the dirty epsilon each sweep
            // step, so this measures REBUILD frames, not the static idle)
            clock.CurrentTime = 0f;
            update(); // warmup frame (JIT, lazy mesh creation, one-time warns)
            long allocBefore = System.GC.GetAllocatedBytesForCurrentThread();
            sw.Restart();
            for (int f = 0; f < 100; f++)
            {
                clock.CurrentTime = f * (10800f / 99f);
                update();
            }
            sw.Stop();
            long allocated = System.GC.GetAllocatedBytesForCurrentThread() - allocBefore;
            double frameMs = sw.Elapsed.TotalMilliseconds / 100.0;
            Debug.Log($"[scale] Start({FixtureUnits} units): {startMs:F1} ms; " +
                $"Update avg over 100 frames: {frameMs:F3} ms/frame; " +
                $"alloc {allocated / 100} bytes/frame");

            // every unit rendered at Block tier, so every entry now owns a
            // built persistent symbol mesh. The fixture has no families or
            // rosters — all 210 are monolithic ribbons. ACKNOWLEDGED GAP
            // (review): the per-frame roster label-anchor path
            // (RosterSymbolSpecs at the Regiments tier) is hand-verified
            // allocation-free but not pinned by this guard; pinning it needs
            // a roster family in the fixture AND a camera inside the
            // Regiments band, both out of this headless test's reach.
            foreach (object entry in unitsList)
            {
                var mesh = (Mesh)symbolMeshField.GetValue(entry);
                Assert.IsNotNull(mesh, "rendered unit never built its symbol mesh");
                Assert.Greater(mesh.vertexCount, 0);
            }

            // budgets, deliberately loose for CI machines: the point is
            // catching a scaling cliff (quadratic loop, per-frame allocation
            // storm), not benchmarking. 8 ms is half a 60 fps frame for the
            // CPU symbol loop alone — and this sweep is the REBUILD worst
            // case (every mover redraws its draped ribbon every step), not
            // the static battle the dirty predicate optimizes for.
            Assert.Less(startMs, 5000.0, "Start cost scaled past the budget");
            Assert.Less(frameMs, 8.0, "per-frame symbol loop scaled past the budget");
            // the rebuild path writes into preallocated scratch and persistent
            // meshes; the loose ceiling catches a reintroduced steady leak
            // that the frame budget would hide
            Assert.LessOrEqual(allocated / 100, 1024,
                "per-frame Update allocation crept in — the symbol loop must stay allocation-free");
        }
        finally
        {
            // no marker GameObjects to sweep anymore — but the per-unit
            // symbol meshes are runtime objects, and edit mode never runs
            // the lifecycle callbacks, so invoke the director's OnDestroy
            // cleanup by hand before tearing the rig down
            var directorComponent = directorGO.GetComponent<BattleDirector>();
            if (directorComponent != null)
                typeof(BattleDirector).GetMethod("OnDestroy",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(directorComponent, null);
            Object.DestroyImmediate(directorGO);
            Object.DestroyImmediate(terrainGO);
            Object.DestroyImmediate(terrainData);
            Object.DestroyImmediate(material);
        }
    }
}
