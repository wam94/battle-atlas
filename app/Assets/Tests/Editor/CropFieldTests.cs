using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class CropFieldTests
{
    const float TexelSize = 8f;
    const int Resolution = 64;

    // uniform Field weight over the whole grid
    static float[] AllField(float weight = 1f)
    {
        var weights = new float[Resolution * Resolution];
        for (int i = 0; i < weights.Length; i++) weights[i] = weight;
        return weights;
    }

    static float Flat(float x, float z) => 0f;

    [Test]
    public void Rebuild_PlacementIsDeterministicAndOnFieldTexelsOnly()
    {
        // field on exactly one texel: clumps must land inside it, and two
        // rebuilds must be matrix-identical (FNV hash, never Random)
        var weights = new float[Resolution * Resolution];
        weights[10 * Resolution + 12] = 1f; // texel (12, 10)
        var cam = new Vector3(12.5f * TexelSize, 0f, 10.5f * TexelSize);
        var a = new Matrix4x4[CropField.MaxClumps];
        var b = new Matrix4x4[CropField.MaxClumps];

        int na = CropField.Rebuild(weights, Resolution, TexelSize, cam, 250f, Flat, a);
        int nb = CropField.Rebuild(weights, Resolution, TexelSize, cam, 250f, Flat, b);

        Assert.Greater(na, 0);
        Assert.AreEqual(na, nb);
        for (int i = 0; i < na; i++)
        {
            Assert.AreEqual(a[i], b[i]);
            Vector3 p = a[i].GetColumn(3);
            Assert.AreEqual(12, Mathf.FloorToInt(p.x / TexelSize));
            Assert.AreEqual(10, Mathf.FloorToInt(p.z / TexelSize));
        }
        // and a zero-weight map grows no wheat at all
        Assert.AreEqual(0, CropField.Rebuild(
            new float[Resolution * Resolution], Resolution, TexelSize, cam, 250f, Flat, a));
    }

    [Test]
    public void Rebuild_RingMembershipUpdatesOnCellCrossing()
    {
        float[] weights = AllField();
        var buffer = new Matrix4x4[CropField.MaxClumps];
        var cam = new Vector3(32f * TexelSize, 50f, 32f * TexelSize);
        const float ring = 100f;

        // every clump lives inside the ring (XZ distance, camera height
        // irrelevant to membership)
        int n = CropField.Rebuild(weights, Resolution, TexelSize, cam, ring, Flat, buffer);
        Assert.Greater(n, 0);
        for (int i = 0; i < n; i++)
        {
            Vector3 p = buffer[i].GetColumn(3);
            float dist = Vector2.Distance(
                new Vector2(p.x, p.z), new Vector2(cam.x, cam.z));
            Assert.LessOrEqual(dist, ring + 1e-3f);
        }

        // moving within a texel keeps the same rebuild trigger cell;
        // crossing the boundary changes it — that's the only rebuild moment
        Assert.AreEqual(CropField.CameraCell(cam, TexelSize),
                        CropField.CameraCell(cam + new Vector3(TexelSize * 0.4f, 0f, 0f), TexelSize));
        var crossed = cam + new Vector3(TexelSize, 0f, 0f);
        Assert.AreNotEqual(CropField.CameraCell(cam, TexelSize),
                           CropField.CameraCell(crossed, TexelSize));

        // and the rebuilt ring follows the camera: membership holds around
        // the NEW position, so texels behind dropped and texels ahead joined
        int n2 = CropField.Rebuild(weights, Resolution, TexelSize, crossed, ring, Flat, buffer);
        Assert.Greater(n2, 0);
        bool joined = false;
        for (int i = 0; i < n2; i++)
        {
            Vector3 p = buffer[i].GetColumn(3);
            var xz = new Vector2(p.x, p.z);
            Assert.LessOrEqual(Vector2.Distance(xz, new Vector2(crossed.x, crossed.z)),
                               ring + 1e-3f);
            if (Vector2.Distance(xz, new Vector2(cam.x, cam.z)) > ring) joined = true;
        }
        Assert.IsTrue(joined, "crossing a cell should pull new texels into the ring");
    }

    [Test]
    public void Rebuild_StopsAtTheBufferCap()
    {
        // saturated field, generous ring, tiny buffer: the fill must stop
        // exactly at capacity instead of overrunning (the batch cap — the
        // component's buffer is MaxClumps, drawn in ≤1023-instance chunks)
        float[] weights = AllField();
        var small = new Matrix4x4[100];
        var cam = new Vector3(32f * TexelSize, 0f, 32f * TexelSize);
        int n = CropField.Rebuild(weights, Resolution, TexelSize, cam, 250f, Flat, small);
        Assert.AreEqual(small.Length, n);
    }
}
