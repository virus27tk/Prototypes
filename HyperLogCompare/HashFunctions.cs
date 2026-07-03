using System.Text;

namespace HyperLogCompare;

public static class HashFunctions
{
    /// <summary>
    /// MurmurHash3 (x86, 32-bit). Different seeds behave as independent hash functions.
    /// </summary>
    public static uint Murmur3_32(string value, uint seed)
    {
        const uint c1 = 0xcc9e2d51;
        const uint c2 = 0x1b873593;

        byte[] data = Encoding.UTF8.GetBytes(value);
        uint h1 = seed;
        int length = data.Length;
        int roundedEnd = length & ~3;

        for (int i = 0; i < roundedEnd; i += 4)
        {
            uint k1 = (uint)(data[i] | data[i + 1] << 8 | data[i + 2] << 16 | data[i + 3] << 24);
            k1 *= c1;
            k1 = RotateLeft(k1, 15);
            k1 *= c2;

            h1 ^= k1;
            h1 = RotateLeft(h1, 13);
            h1 = h1 * 5 + 0xe6546b64;
        }

        uint tail = 0;
        int remaining = length & 3;
        if (remaining == 3) tail |= (uint)(data[roundedEnd + 2] << 16);
        if (remaining >= 2) tail |= (uint)(data[roundedEnd + 1] << 8);
        if (remaining >= 1)
        {
            tail |= data[roundedEnd];
            tail *= c1;
            tail = RotateLeft(tail, 15);
            tail *= c2;
            h1 ^= tail;
        }

        h1 ^= (uint)length;
        h1 = FMix(h1);
        return h1;
    }

    private static uint RotateLeft(uint x, int r) => (x << r) | (x >> (32 - r));

    private static uint FMix(uint h)
    {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return h;
    }
}
