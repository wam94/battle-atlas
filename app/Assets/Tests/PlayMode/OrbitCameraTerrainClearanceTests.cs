using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BattleAtlas;

// RTS camera slice: the terrain-clearance smoke test the plan calls for —
// a pivot forced below the terrain (the height sampler wired in AFTER the
// pivot was already set to a bad Y, exactly the ordering a scene bootstrap
// could hit) must resolve to a camera that ends up ABOVE the ground, not
// buried in it. The pure math (OrbitMath.ResolveTerrainClearance,
// RideTerrain's pivot correction) is EditMode-tested in OrbitMathTests;
// this pins the same guarantee through the real MonoBehaviour lifecycle
// (Awake + LateUpdate), which EditMode can't drive.
public class OrbitCameraTerrainClearanceTests
{
    const float GroundHeight = 100f;
    const float Clearance = 3f;

    GameObject cameraGo;

    [SetUp]
    public void SetUp()
    {
        cameraGo = new GameObject("TestOrbitCamera");
        cameraGo.AddComponent<Camera>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(cameraGo);
    }

    [UnityTest]
    public IEnumerator PivotForcedBelowTerrain_ResolvesCameraAboveIt()
    {
        var orbit = cameraGo.AddComponent<OrbitCameraController>();
        orbit.minTerrainClearanceM = Clearance;
        yield return null; // Awake ran

        // no height sampler yet: the pivot setter can't correct it, so this
        // assignment sticks exactly as given — deep below any real terrain
        orbit.distance = 200f;
        orbit.pitchDeg = 45f;
        orbit.yawDeg = 0f;
        orbit.pivot = new Vector3(50f, -1000f, 50f);

        // now the terrain apparatus comes online (the AtlasHud.Start /
        // phase-switch wiring timing this test reproduces) — flat ground
        // at GroundHeight everywhere
        orbit.heightSampler = (x, z) => GroundHeight;

        yield return null;
        yield return null;
        yield return null;

        Assert.GreaterOrEqual(cameraGo.transform.position.y, GroundHeight + Clearance - 0.05f,
            "the camera must never resolve below terrain + clearance");
        // and the pivot itself rides back onto the terrain, not just the
        // camera position's safety-net clamp
        Assert.AreEqual(GroundHeight, orbit.pivot.y, 0.5f,
            "the pivot must ride the terrain height once a sampler is wired");
    }
}
