using NUnit.Framework;
using BattleAtlas;

public class BattlefieldInfoTests
{
    [Test]
    public void HonestElevation_DividesOutExaggeration()
    {
        // displayed height 250 m above terrain base at 2.5x over a 72.1 m datum
        // = 100 m of real relief above 72.1 m
        float honest = BattlefieldInfo.HonestElevation(
            displayedHeight: 250f, minElevM: 72.1f, verticalExaggeration: 2.5f);
        Assert.AreEqual(172.1f, honest, 1e-3f);
    }

    [Test]
    public void HonestElevation_IdentityAtNoExaggeration()
    {
        Assert.AreEqual(100f,
            BattlefieldInfo.HonestElevation(50f, 50f, 1f), 1e-3f);
    }
}
