using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

public class TimelineHudTests
{
    [Test]
    public void IsTouchOverHud_TrueInsideBottomStrip()
    {
        // touch coords: y measured UP from screen bottom; HUD is the bottom strip
        Assert.IsTrue(TimelineHud.IsTouchOverHud(new Vector2(100f, 50f), hudHeightPx: 120f));
        Assert.IsFalse(TimelineHud.IsTouchOverHud(new Vector2(100f, 121f), hudHeightPx: 120f));
    }

    [Test]
    public void HudScale_NeverBelowOne()
    {
        Assert.AreEqual(1f, TimelineHud.HudScale(0f), 1e-3f);     // dpi unknown
        Assert.AreEqual(1f, TimelineHud.HudScale(120f), 1e-3f);   // low-dpi
        Assert.AreEqual(2.875f, TimelineHud.HudScale(460f), 1e-3f); // iPhone-class
    }
}
