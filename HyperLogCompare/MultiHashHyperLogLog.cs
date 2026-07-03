namespace HyperLogCompare;

/// <summary>
/// Runs several independent HyperLogLog sketches (each with its own seeded hash function)
/// over the same input and averages their estimates to reduce variance.
/// </summary>
public class MultiHashHyperLogLog : ICardinalityEstimator
{
    private readonly List<HyperLogLogSketch> _sketches;

    public int HashFunctionCount { get; }

    public MultiHashHyperLogLog(int hashFunctionCount, int precision)
    {
        HashFunctionCount = hashFunctionCount;
        _sketches = new List<HyperLogLogSketch>();
        for (int seed = 0; seed < hashFunctionCount; seed++)
        {
            _sketches.Add(new HyperLogLogSketch(precision, seed));
        }
    }

    public void Add(string item)
    {
        foreach (var sketch in _sketches)
        {
            sketch.Add(item);
        }
    }

    public double Estimate()
    {
        return _sketches.Average(s => s.Estimate());
    }
}