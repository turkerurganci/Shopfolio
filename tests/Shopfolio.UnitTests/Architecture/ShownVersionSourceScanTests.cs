using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the shown version scan over the order-side modules and shows on a deliberate violation sample that each hand-written comparison goes red and
/// <c>ShownVersion.Matches</c>, a comment and another version name do not (T364 KK3, KK4; 09 10.2 item 3, 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class ShownVersionSourceScanTests
{
    [Fact]
    public void OrderSideModules_CompareTheShownVersionOnlyThroughShownVersionMatches()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!ShownVersionSourceScan.IsScanned(relative) || relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in ShownVersionSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        // The files that compare the administrator's shown version are among the scanned ones, and every scanned folder has files.
        Assert.Contains("src/Shopfolio.Web/Modules/Ordering/OrderTransitions.cs", scanned);
        Assert.Contains("src/Shopfolio.Web/Modules/Ordering/OrderCorrections.cs", scanned);
        Assert.Contains("src/Shopfolio.Web/Modules/Ordering/DownloadCounters.cs", scanned);
        Assert.All(ShownVersionSourceScan.ScannedFolders, folder => Assert.Contains(scanned, f => f.StartsWith(folder, StringComparison.Ordinal)));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnTheSample_ReportsEachHandWrittenComparison_AndNotMatchesACommentOrAnotherVersionName()
    {
        string sample = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Concurrency", "ShownVersionComparisons.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = ShownVersionSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Sample.cs", sample);

        Assert.Equal(
            [6, 7, 8, 9, 11, 12, 13, 14, 19, 20, 21, 22, 23, 24, 25, 26],
            findings.Where(f => f.Rule == ShownVersionSourceScan.HandWrittenComparison).Select(f => f.Line));

        // The comparison skipped on a missing version (T364 B1): each null-test shape goes red; ShownVersion.Check, a null test that refuses and a
        // null-conditional with no comparison do not.
        Assert.Equal(
            [30, 31, 32, 34, 35, 36, 37, 38, 39],
            findings.Where(f => f.Rule == ShownVersionSourceScan.SkippedComparison).Select(f => f.Line));
        Assert.Equal(25, findings.Count);
    }

    [Theory]
    [InlineData("if (expectedVersion is { } shown && !ShownVersion.Matches(order, shown))")]
    [InlineData("return expectedVersion is { } shown\n    && !ShownVersion.Matches(order, shown) ? conflict : ok;")]
    [InlineData("if (request.ExpectedVersion is null || ShownVersion.Matches(order, request.ExpectedVersion.Value))")]
    [InlineData("bool go = ShownVersion.Matches(order, request.ExpectedVersion ?? order.Version);")]
    public void Scan_ReportsTheComparisonSkippedOnAMissingVersion(string code)
    {
        Assert.Contains(ShownVersionSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Sample.cs", code), f => f.Rule == ShownVersionSourceScan.SkippedComparison);
    }

    [Theory]
    [InlineData("switch (ShownVersion.Check(order, expectedVersion, byAdministrator: true))")]
    [InlineData("if (request.ExpectedVersion is null || request.NoticeArrivalDate is not { } arrival)")]
    [InlineData("if (!ShownVersion.Matches(order, shown)) return refusal ?? fallback;")]
    [InlineData("if (!ShownVersion.Matches(order, mark.ExpectedVersion))")]
    public void Scan_LeavesTheCheckAndARefusingNullTestAlone(string code)
    {
        Assert.Empty(ShownVersionSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Sample.cs", code));
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/AfterSales/Sample.cs")]
    [InlineData(@"src\Shopfolio.Web\Modules\Payment\Sample.cs")]
    public void Scan_CoversEveryOrderSideModule(string path)
    {
        Assert.NotEmpty(ShownVersionSourceScan.Scan(path, "bool a = order.Version != shown;"));
    }

    [Theory]
    [InlineData("src/Shopfolio.Web/Modules/Catalog/ProductRemoval.cs")]
    [InlineData("src/Shopfolio.Web/Infrastructure/Forms/RecordSave.cs")]
    [InlineData("src/Shopfolio.Web/Modules/Ordering/Sample.cshtml")]
    public void Scan_LeavesTheRecordVariantAndTheHelperOutside(string path)
    {
        Assert.Empty(ShownVersionSourceScan.Scan(path, "bool a = record.Version != formVersion;"));
    }
}
