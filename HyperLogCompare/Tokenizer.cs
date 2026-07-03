using System.Text.RegularExpressions;

namespace HyperLogCompare;

public static class Tokenizer
{
    private static readonly Regex WordRegex = new(@"[A-Za-z0-9']+", RegexOptions.Compiled);

    public static List<string> Tokenize(string text)
    {
        var words = new List<string>();
        foreach (Match m in WordRegex.Matches(text))
        {
            words.Add(m.Value.ToLowerInvariant());
        }
        return words;
    }
}
