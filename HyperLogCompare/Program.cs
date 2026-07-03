using HyperLogCompare;

int[] hashFunctionCounts = { 1, 2, 3 };
const int precision = 10; // 2^10 = 1024 registers per sketch, used only by the bucket-based HyperLogLog

string[] files = Directory.GetFiles(AppContext.BaseDirectory, "sample_*_words.txt")
    .OrderBy(f => new FileInfo(f).Length)
    .ToArray();

if (files.Length == 0)
{
    Console.WriteLine($"No sample_*_words.txt files found in {AppContext.BaseDirectory}");
    return;
}

var fileStats = files.Select(file =>
{
    string text = File.ReadAllText(file);
    List<string> words = Tokenizer.Tokenize(text);
    return (
        FileName: Path.GetFileName(file),
        Words: words,
        ExactUnique: ExactWordCounter.CountUnique(words)
    );
}).ToList();

string FormatRow(IReadOnlyList<string> values) =>
    $"{values[0],-24}" + string.Join("", values.Skip(1).Select(v => $"{v,-16}"));

void RunTable(string title, string columnPrefix, Func<int, ICardinalityEstimator> createEstimator)
{
    var columns = new List<string> { "File", "TotalWords", "ExactUnique" };
    foreach (int h in hashFunctionCounts)
    {
        columns.Add($"{columnPrefix}_{h}hash");
        columns.Add($"Error%_{h}hash");
    }

    Console.WriteLine(title);
    Console.WriteLine(FormatRow(columns));

    foreach (var (fileName, words, exactUnique) in fileStats)
    {
        var rowValues = new List<string> { fileName, words.Count.ToString(), exactUnique.ToString() };
        foreach (int hashFunctionCount in hashFunctionCounts)
        {
            ICardinalityEstimator estimator = createEstimator(hashFunctionCount);
            foreach (string word in words)
            {
                estimator.Add(word);
            }

            double estimate = estimator.Estimate();
            double errorPercent = exactUnique == 0 ? 0 : Math.Abs(estimate - exactUnique) / exactUnique * 100;
            rowValues.Add(estimate.ToString("F1"));
            rowValues.Add(errorPercent.ToString("F2"));
        }

        Console.WriteLine(FormatRow(rowValues));
    }

    Console.WriteLine();
}

RunTable("Flajolet-Martin (bitmap, first-set-bit)", "FM", h => new MultiHashFlajoletMartin(h));
RunTable("HyperLogLog (bucket / register max-rank)", "HLL", h => new MultiHashHyperLogLog(h, precision));