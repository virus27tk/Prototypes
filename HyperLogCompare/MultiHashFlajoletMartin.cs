namespace HyperLogCompare;

/// <summary>
/// Runs several independent Flajolet-Martin sketches (each with its own seeded hash
/// function) over the same input and averages their estimates to reduce variance.
/// </summary>
public class MultiHashFlajoletMartin
{
    private readonly List<FlajoletMartinSketch> _sketches;

    public int HashFunctionCount { get; }

    public MultiHashFlajoletMartin(int hashFunctionCount)
    {
        HashFunctionCount = hashFunctionCount;
        _sketches = new List<FlajoletMartinSketch>();
        for (int seed = 0; seed < hashFunctionCount; seed++)
        {
            _sketches.Add(new FlajoletMartinSketch(seed));
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