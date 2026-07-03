namespace HyperLogCompare;

public interface ICardinalityEstimator
{
    void Add(string item);
    double Estimate();
}