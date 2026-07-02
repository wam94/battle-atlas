using NUnit.Framework;
using BattleAtlas;

public class SunEphemerisTests
{
    const float Hour = 3600f;

    [Test]
    public void SunAngles_TableKeys_ReturnExactReportValues()
    {
        // The research doc's NOAA/Meeus table (§0) verbatim at the battle
        // window's hourly keys — the ephemeris IS the record, so the keys
        // must come back exact, not merely close.
        var at13 = SunEphemeris.SunAngles(13f * Hour);
        Assert.AreEqual(69.4f, at13.elevationDeg, 1e-4f);
        Assert.AreEqual(219.4f, at13.azimuthDeg, 1e-4f);

        var at14 = SunEphemeris.SunAngles(14f * Hour);
        Assert.AreEqual(60.3f, at14.elevationDeg, 1e-4f);
        Assert.AreEqual(244.3f, at14.azimuthDeg, 1e-4f);

        var at15 = SunEphemeris.SunAngles(15f * Hour);
        Assert.AreEqual(49.3f, at15.elevationDeg, 1e-4f);
        Assert.AreEqual(259.0f, at15.azimuthDeg, 1e-4f);

        var at16 = SunEphemeris.SunAngles(16f * Hour);
        Assert.AreEqual(37.9f, at16.elevationDeg, 1e-4f);
        Assert.AreEqual(269.7f, at16.azimuthDeg, 1e-4f);
    }

    [Test]
    public void SunAngles_BetweenKeys_LerpsBothAngles()
    {
        // 15:15 is the midpoint of the 15:00 and 15:30 keys: elevation
        // (49.3 + 43.6) / 2, azimuth (259.0 + 264.7) / 2.
        var mid = SunEphemeris.SunAngles(15.25f * Hour);
        Assert.AreEqual(46.45f, mid.elevationDeg, 1e-3f);
        Assert.AreEqual(261.85f, mid.azimuthDeg, 1e-3f);
    }

    [Test]
    public void SunAngles_OutsideTable_ClampsToEndpoints()
    {
        // before 12:00 -> the 12:00 row; after 17:00 -> the 17:00 row
        var early = SunEphemeris.SunAngles(0f);
        Assert.AreEqual(73.2f, early.elevationDeg, 1e-4f);
        Assert.AreEqual(177.0f, early.azimuthDeg, 1e-4f);

        var late = SunEphemeris.SunAngles(23f * Hour);
        Assert.AreEqual(26.4f, late.elevationDeg, 1e-4f);
        Assert.AreEqual(278.8f, late.azimuthDeg, 1e-4f);
    }
}
