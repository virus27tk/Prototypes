using HyperLogCompare;
int[] hashFunctionCounts = { 1, 2, 3 };

string[] files = Directory.GetFiles(AppContext.BaseDirectory, "sample_*_words.txt")
    .OrderBy(f => new FileInfo(f).Length)
    .ToArray();

if (files.Length == 0)
{
    Console.WriteLine($"No sample_*_words.txt files found in {AppContext.BaseDirectory}");
    return;
}

var columns = new List<string> { "File", "TotalWords", "ExactUnique" };
foreach (int h in hashFunctionCounts)
{
    columns.Add($"HLL_{h}hash");
    columns.Add($"Error%_{h}hash");
}

string FormatRow(IReadOnlyList<string> values) =>
    $"{values[0],-24}" + string.Join("", values.Skip(1).Select(v => $"{v,-16}"));

Console.WriteLine(FormatRow(columns));

var csvLines = new List<string> { string.Join(",", columns) };

foreach (var file in files)
{
    var text = File.ReadAllText(file);
    var words = Tokenizer.Tokenize(text);
    var exactUnique = ExactWordCounter.CountUnique(words);

    var fileName = Path.GetFileName(file);
    var rowValues = new List<string> { fileName, words.Count.ToString(), exactUnique.ToString() };
    foreach (var hashFunctionCount in hashFunctionCounts)
    {
        var hll = new MultiHashFlajoletMartin(hashFunctionCount);
        foreach (var word in words)
        {
            hll.Add(word);
        }

        var estimate = hll.Estimate();
        var errorPercent = exactUnique == 0 ? 0 : Math.Abs(estimate - exactUnique) / exactUnique * 100;
        rowValues.Add(estimate.ToString("F1"));
        rowValues.Add(errorPercent.ToString("F2"));
    }

    Console.WriteLine(FormatRow(rowValues));
}
Console.WriteLine();