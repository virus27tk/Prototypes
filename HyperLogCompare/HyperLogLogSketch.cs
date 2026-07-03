namespace HyperLogCompare;

/// <summary>
/// A single HyperLogLog sketch backed by one seeded hash function.
/// </summary>
public class HyperLogLogSketch
{
    private readonly int _precision;
    private readonly int _m;
    private readonly int _seed;
    private readonly byte[] _registers;
    private readonly double _alpha;

    public HyperLogLogSketch(int precision, int seed)
    {
        _precision = precision;
        _m = 1 << precision;
        _seed = seed;
        _registers = new byte[_m];
        _alpha = ComputeAlpha(_m);
    }

    public void Add(string item)
    {
        uint hash = HashFunctions.Murmur3_32(item, (uint)_seed);
        int index = (int)(hash >> (32 - _precision));
        uint remainder = hash & ((1u << (32 - _precision)) - 1);
        int rank = LeadingZeroCount(remainder, 32 - _precision) + 1;

        if (rank > _registers[index])
        {
            _registers[index] = (byte)rank;
        }
    }

    public double Estimate()
    {
        double sumOfInverses = 0;
        int zeroRegisters = 0;
        foreach (byte r in _registers)
        {
            sumOfInverses += 1.0 / (1 << r);
            if (r == 0) zeroRegisters++;
        }

        double rawEstimate = _alpha * _m * _m / sumOfInverses;

        if (rawEstimate <= 2.5 * _m && zeroRegisters > 0)
        {
            return _m * Math.Log((double)_m / zeroRegisters);
        }

        const double twoPow32 = 4294967296.0;
        if (rawEstimate > twoPow32 / 30.0)
        {
            return -twoPow32 * Math.Log(1 - rawEstimate / twoPow32);
        }

        return rawEstimate;
    }

    private static int LeadingZeroCount(uint value, int bitWidth)
    {
        if (value == 0) return bitWidth;

        int count = 0;
        uint mask = 1u << (bitWidth - 1);
        while ((value & mask) == 0)
        {
            count++;
            mask >>= 1;
        }
        return count;
    }

    private static double ComputeAlpha(int m)
    {
        return m switch
        {
            16 => 0.673,
            32 => 0.697,
            64 => 0.709,
            _ => 0.7213 / (1 + 1.079 / m)
        };
    }
}