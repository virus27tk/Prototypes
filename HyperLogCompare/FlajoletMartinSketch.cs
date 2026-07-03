namespace HyperLogCompare;

/// <summary>
/// A simple Flajolet-Martin sketch backed by one seeded hash function.
/// Unlike HyperLogLogSketch (which buckets into registers and tracks a max rank per
/// bucket), this keeps a single bitmap: for each item, the bit at the position of the
/// first set bit (from the LSB) of its hash is turned on. The cardinality estimate is
/// 2^b / phi, where b is the position of the lowest still-unset bit in the bitmap and
/// phi is the Flajolet-Martin bias-correction constant.
/// </summary>
public class FlajoletMartinSketch
{
    private const int BitWidth = 32;
    private const double Phi = 0.77351;

    private readonly int _seed;
    private readonly bool[] _bitmap;

    public FlajoletMartinSketch(int seed)
    {
        _seed = seed;
        _bitmap = new bool[BitWidth];
    }

    public void Add(string item)
    {
        var hash = HashFunctions.Murmur3_32(item, (uint)_seed);
        var position = FirstSetBitFromLsb(hash);
        if (position < BitWidth)
        {
            _bitmap[position] = true;
        }
    }

    public double Estimate()
    {
        int b = 0;
        while (b < BitWidth && _bitmap[b])
        {
            b++;
        }
        return Math.Pow(2, b) / Phi;
    }

    private static int FirstSetBitFromLsb(uint value)
    {
        if (value == 0) return BitWidth;

        int position = 0;
        while ((value & 1) == 0)
        {
            position++;
            value >>= 1;
        }
        return position;
    }
}