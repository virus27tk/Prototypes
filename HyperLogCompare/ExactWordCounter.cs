namespace HyperLogCompare;

public static class ExactWordCounter
{
    public static int CountUnique(IEnumerable<string> words)
    {
        return new HashSet<string>(words).Count;
    }
}
