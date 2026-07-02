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
