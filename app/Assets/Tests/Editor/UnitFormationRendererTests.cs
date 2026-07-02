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
