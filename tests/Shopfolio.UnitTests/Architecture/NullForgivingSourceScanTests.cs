using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the null-forgiving scan over the application source (T550; 09 3.1 item 5; D-778 decision (c); K-1613): every postfix
/// <c>!</c> under <c>src</c> is gone or carries its reason comment, so the count is zero; a deliberate violation sample shows the
/// scan red on the forms it must catch and green on the reasoned, removed and look-alike forms (09 19.2 item 3).
/// </summary>
public sealed class NullForgivingSourceScanTests
{
    [Fact]
    public void ApplicationSource_HasNoNullForgivingWithoutReason()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            bool isCode = file.EndsWith(".cs", StringComparison.Ordinal);
            bool isRazor = file.EndsWith(".cshtml", StringComparison.Ordinal) || file.EndsWith(".razor", StringComparison.Ordinal);
            if (IsBuildOutput(relative) || !(isCode || isRazor))
            {
                continue;
            }

            scanned.Add(relative);
            string text = File.ReadAllText(file);
            foreach ((string rule, int line) in isCode ? NullForgivingSourceScan.Scan(text) : NullForgivingSourceScan.ScanRazor(text))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith(".cshtml", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachUnexplainedUseAndNotTheReasonedOrLookAlikeForms()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "NullForgiving", "NullForgivingUses.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = NullForgivingSourceScan.Scan(sample);

        Assert.Equal([5, 6, 7, 8, 9, 10, 11, 12, 13], findings.Select(f => f.Line));
        Assert.All(findings, f => Assert.Equal(NullForgivingSourceScan.Unexplained, f.Rule));
    }

    [Theory]
    [InlineData("string s = x!;", 1)]
    [InlineData("string s = x!; // non-null: the caller checked it above", 0)]
    [InlineData("string s = x!; // non-null:", 1)]
    [InlineData("string s = x!; /* non-null: a block comment is not the reason form */", 1)]
    [InlineData("string s = $\"{a!.B} and {{c!}}\";", 1)]
    [InlineData("string s = $\"{{a!.B}}\";", 0)]
    [InlineData("string s = $$\"\"\"{{a!.B}} {c!}\"\"\";", 1)]
    [InlineData("string s = $$\"\"\"{c!} {{{d}}}\"\"\";", 0)]
    [InlineData("bool b = !(x is null) && y != z;", 0)]
    [InlineData("int n = list[0]!.Count;", 1)]
    [InlineData("T t = Get<T>()!;", 1)]
    [InlineData("bool b = x =>!y;", 0)]
    [InlineData("#if !DEBUG", 0)]
    [InlineData("string s = \"a\"!;", 1)]
    [InlineData("char c = 'a'!;", 1)]
    [InlineData("string s = $\"{a}\"!;", 1)]
    [InlineData("string s = \"\"\"raw\"\"\"!;", 1)]
    [InlineData("string s = x !;", 1)]
    [InlineData("int n = Call(x !, y);", 1)]
    [InlineData("string s = x ! .Trim();", 1)]
    [InlineData("return !flag;", 0)]
    [InlineData("bool b = a && ! (c);", 0)]
    [InlineData("bool b = \"a\" != x;", 0)]
    public void Scan_OnOneLine_FindsOnlyThePostfixOperatorInCode(string line, int expected)
    {
        Assert.Equal(expected, NullForgivingSourceScan.Scan(line).Count);
    }

    [Theory]
    [InlineData("<p>@(Model.Name!.Trim())</p>", 1)]
    [InlineData("@if (Model.Item!.IsShown)", 1)]
    [InlineData("<p>@(Model.Name!.Trim())</p> @* // non-null: the handler sets the name before the page renders *@", 0)]
    [InlineData("<p>Teşekkürler! Siparişiniz alındı.</p>", 0)]
    [InlineData("<p>Merhaba!</p>", 0)]
    public void ScanRazor_FindsTheMemberAccessFormAndNotTheSentence(string line, int expected)
    {
        Assert.Equal(expected, NullForgivingSourceScan.ScanRazor(line).Count);
    }

    private static bool IsBuildOutput(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal);
    }
}
