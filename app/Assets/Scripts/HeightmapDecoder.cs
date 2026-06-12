namespace BattleAtlas
{
    public static class HeightmapDecoder
    {
        // RAW convention (see pipeline export.py): 16-bit little-endian, row 0 = north.
        // Unity heights[0, *] is the SOUTH edge, so rows flip here.
        public static float[,] Decode(byte[] raw, int resolution)
        {
            if (raw.Length != resolution * resolution * 2)
                throw new System.ArgumentException(
                    $"raw length {raw.Length} != 2 * {resolution}^2");

            var heights = new float[resolution, resolution];
            for (int row = 0; row < resolution; row++)
            {
                int dstY = resolution - 1 - row;
                for (int x = 0; x < resolution; x++)
                {
                    int i = (row * resolution + x) * 2;
                    ushort v = (ushort)(raw[i] | (raw[i + 1] << 8));
                    heights[dstY, x] = v / 65535f;
                }
            }
            return heights;
        }
    }
}
