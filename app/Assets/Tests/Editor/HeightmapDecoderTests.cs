using NUnit.Framework;
using BattleAtlas;

public class HeightmapDecoderTests
{
    [Test]
    public void Decode_FlipsRowsAndScalesTo01()
    {
        // 2x2, little-endian. RAW row 0 (north): [0, 65535]; row 1 (south): [32768, 16384]
        byte[] raw = {
            0x00, 0x00, 0xFF, 0xFF,
            0x00, 0x80, 0x00, 0x40,
        };
        float[,] h = HeightmapDecoder.Decode(raw, 2);
        // Unity heights row 0 = south edge => RAW row 1 lands there
        Assert.AreEqual(32768f / 65535f, h[0, 0], 1e-4f);
        Assert.AreEqual(16384f / 65535f, h[0, 1], 1e-4f);
        Assert.AreEqual(0f, h[1, 0], 1e-4f);
        Assert.AreEqual(1f, h[1, 1], 1e-4f);
    }

    [Test]
    public void Decode_RejectsWrongLength()
    {
        Assert.Throws<System.ArgumentException>(() => HeightmapDecoder.Decode(new byte[3], 2));
    }
}
