using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The three exports are the only ones (T384 KK9; 02 10.7.2, 10.7.3; 10 KD-1, KD-54) and the export code never holds a whole set (09 10.10 items 4, 9; 09 20 item 2): no name of the application
/// is an import, a bulk update, or an export of the catalog, the content, the trail or the sales summary; every name with an export word is on the closed list of the three exports' own names
/// (the list is exact names, not a pattern); and the code that reads a set for a file reads it by pages - no <c>ToList</c> or <c>ToArray</c>, and a <c>ToListAsync</c> only on a statement that takes a page.
/// Each form is shown to be found in a sample (09 19.2 item 3).
/// </summary>
public sealed partial class ExportSurfaceTests
{
    private static readonly Assembly Application = typeof(AppDbContext).Assembly;

    /// <summary>The exact names of the three exports and of what carries them: types, members, enum values and the fields and parameters that hold them.</summary>
    private static readonly HashSet<string> ExportNames = new(StringComparer.Ordinal)
    {
        // The writer, its ceilings and the temporary directory.
        "CsvExportWriter", "ExportCeilings", "ExportLimits", "ExportLimitExceededException", "ExportTempDirectory", "ExportDirectoryStartup",

        // The three reads and their columns.
        "OrderExport", "OrderExportRow", "OrderExportColumns", "CustomerExport", "CustomerExportRow", "CustomerExportColumns",
        "ContactRequestExport", "ContactRequestExportRow", "ContactRequestExportColumns",

        // The Administration face.
        "DataExport", "IDataExport", "ExportOutcome", "ExportRangeError", "ExportFileResult", "ExportOrdersAsync", "ExportCustomersAsync", "ExportContactRequestsAsync",

        // The modules' faces for the reads.
        "WriteOrderExportAsync", "WriteCustomerExportAsync", "WriteContactRequestExportAsync",

        // The trail, the error code and the area.
        "ExportOrders", "ExportCustomers", "ExportContactRequests", "ExportEmpty", "Export",

        // The fields and parameters that hold the reads and the directory.
        "_export", "_contactRequestExport", "contactRequestExport", "export",

        // The command-line reader of the log files has its own file export (a log is not one of the three; 09 18).
        "LogExportFile",
    };

    private static readonly string[] ExportWords = ["export", "exports", "exported", "exporter", "exporting"];

    private static readonly string[] ImportWords = ["import", "imports", "imported", "importer", "importing"];

    [Fact]
    public void NoNameOfTheApplication_IsAnImportABulkUpdateOrAnExportOutsideTheThreeExports_KK9()
    {
        List<string> findings = [.. SalesModelShape.NamesOf(Application).Where(n => IsOutsideTheThreeExports(n.Name)).Select(n => n.Owner)];

        Assert.True(findings.Count == 0, string.Join(Environment.NewLine, findings));
    }

    [Fact]
    public void TheScanReadsTheWholeApplication_AndSeesTheThreeExports()
    {
        List<string> names = [.. SalesModelShape.NamesOf(Application).Select(n => n.Name)];

        Assert.Contains(nameof(IDataExport.ExportOrdersAsync), names);
        Assert.Contains(nameof(IDataExport.ExportCustomersAsync), names);
        Assert.Contains(nameof(IDataExport.ExportContactRequestsAsync), names);
        Assert.Contains(nameof(AuditOperation.ExportOrders), names);
        Assert.True(names.Count > 5000, "the application surface has thousands of names; a scan that reads few is not reading the application");
    }

    [Fact]
    public void EveryNameOnTheClosedList_IsInTheApplication_SoTheListHoldsNothingBroader()
    {
        HashSet<string> names = [.. SalesModelShape.NamesOf(Application).Select(n => n.Name)];

        Assert.Empty(ExportNames.Where(n => !names.Contains(n)).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheOperationsOfTheTrail_HaveExactlyThreeExports_AndNoImport()
    {
        string[] operations = [.. Enum.GetNames<AuditOperation>()];

        Assert.Equal(["ExportContactRequests", "ExportCustomers", "ExportOrders"], operations.Where(o => SalesModelShape.Words(o).Intersect(ExportWords).Any()).Order(StringComparer.Ordinal));
        Assert.DoesNotContain(operations, o => SalesModelShape.Words(o).Intersect(ImportWords).Any());
    }

    [Theory]
    [InlineData("ExportCatalogAsync")]
    [InlineData("ExportProducts")]
    [InlineData("ExportAuditTrail")]
    [InlineData("ExportTrail")]
    [InlineData("ExportSalesSummary")]
    [InlineData("ExportContent")]
    [InlineData("export_catalog")]
    [InlineData("Pages_Admin_Products_Export")]
    [InlineData("ImportProducts")]
    [InlineData("ProductImport")]
    [InlineData("CsvImporter")]
    [InlineData("BulkUpdateProducts")]
    [InlineData("BulkPriceUpdate")]
    [InlineData("ExportOrdersAsync2")]
    public void TheScan_FindsEachFormOfASecondExportAnImportOrABulkUpdate(string name)
    {
        Assert.True(IsOutsideTheThreeExports(name), name);
    }

    [Theory]
    [InlineData("ExportOrdersAsync")]
    [InlineData("ExportCustomersAsync")]
    [InlineData("WriteContactRequestExportAsync")]
    [InlineData("CsvExportWriter")]
    [InlineData("ExportOrders")]
    [InlineData("Important")]
    [InlineData("UpdateStock")]
    [InlineData("BulkReadOnly")]
    public void TheScan_LeavesTheThreeExportsAndUnrelatedNamesAlone(string name)
    {
        Assert.False(IsOutsideTheThreeExports(name), name);
    }

    // ---- the export code reads a set by pages ----

    [Fact]
    public void TheExportCode_NeverHoldsAWholeSet_NoToListNoToArray_AndAToListAsyncOnlyOnAPage()
    {
        string root = RepositoryRoot.Find();
        string[] files = ExportSourceFiles(root);

        Assert.True(files.Length >= 6, "the scan must find the writer, the directory and the four reads");
        foreach (string file in files)
        {
            Assert.Empty(Violations(File.ReadAllText(file)).Select(v => $"{Path.GetFileName(file)}: {v}"));
        }
    }

    [Fact]
    public void TheExportSourceFiles_AreTheSixOfTheExports()
    {
        string[] names = [.. ExportSourceFiles(RepositoryRoot.Find()).Select(Path.GetFileName).Order(StringComparer.Ordinal)!];

        Assert.Equal(
            ["ContactRequestExport.cs", "CsvExportWriter.cs", "CustomerExport.cs", "DataExport.cs", "ExportTempDirectory.cs", "OrderExport.cs"],
            names);
    }

    [Theory]
    [InlineData("var all = rows.ToList();")]
    [InlineData("var all = rows.ToArray();")]
    [InlineData("var all = rows.Select(r => r.Id).ToList();")]
    [InlineData("var f = rows.Select(r => r.Id).ToList;")]
    [InlineData("var all = Enumerable.ToArray(rows);")]
    [InlineData("var all = await _db.Orders.ToListAsync(ct);")]
    [InlineData("var all = await _db.Orders.Where(o => o.Id != last)\n    .OrderBy(o => o.Id)\n    .ToArrayAsync(ct);")]
    [InlineData("var all = await query.ToListAsync(ct);\nvar page = await query.Take(5).ToListAsync(ct);")]
    [InlineData("var bytes = File.ReadAllBytes(path);")]
    [InlineData("var bytes = await File.ReadAllBytesAsync(path, ct);")]
    [InlineData("var copy = new MemoryStream();")]
    public void TheScan_FindsEachFormOfHoldingAWholeSet(string source)
    {
        Assert.NotEmpty(Violations(source));
    }

    [Theory]
    [InlineData("var page = await query.OrderBy(o => o.Id).Take(ExportLimits.PageSize).ToListAsync(ct);")]
    [InlineData("var page = await _db.Orders.AsNoTracking()\n    .Where(o => o.Id > last)\n    .OrderBy(o => o.Id)\n    .Take(500)\n    .ToListAsync(ct);")]
    [InlineData("Dictionary<Guid, string> names = [.. page.Select(p => p.Name)];")]
    [InlineData("// rows.ToList() is the rule's own example, in a comment\nint a = 1;")]
    [InlineData("string text = \"ToList is a word in a string\";")]
    public void TheScan_LeavesAPagedReadAndCommentsAlone(string source)
    {
        Assert.Empty(Violations(source));
    }

    private static bool IsOutsideTheThreeExports(string name)
    {
        List<string> words = SalesModelShape.Words(name);
        bool exports = words.Intersect(ExportWords).Any();
        bool imports = words.Intersect(ImportWords).Any();
        bool bulkUpdate = Enumerable.Range(0, Math.Max(0, words.Count - 1)).Any(i => words[i] == "bulk" && words[i + 1] is "update" or "updates" or "import" or "edit" or "price");
        return (exports && !ExportNames.Contains(name)) || imports || bulkUpdate;
    }

    private static string[] ExportSourceFiles(string root)
    {
        string source = Path.Combine(root, "src", "Shopfolio.Web");
        string sep = Path.DirectorySeparatorChar.ToString();
        return
        [
            .. Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{sep}obj{sep}", StringComparison.Ordinal) && !f.Contains($"{sep}bin{sep}", StringComparison.Ordinal))
                .Where(f => Path.GetFileName(f) is "CsvExportWriter.cs" or "ExportTempDirectory.cs" or "DataExport.cs" or "OrderExport.cs" or "CustomerExport.cs" or "ContactRequestExport.cs"),
        ];
    }

    // The violations of the export code's own rule in one source text: comments and string literals are dropped first, then the whole-set forms are looked for.
    private static List<string> Violations(string source)
    {
        string code = StringLiteral().Replace(Comment().Replace(source, string.Empty), "\"\"");
        List<string> found = [];
        foreach (Match m in SyncMaterializer().Matches(code))
        {
            found.Add(m.Value);
        }

        foreach (Match m in AsyncMaterializer().Matches(code))
        {
            int start = Math.Max(code.LastIndexOf(';', m.Index) + 1, 0);
            if (!code.AsSpan(start, m.Index - start).Contains(".Take(", StringComparison.Ordinal))
            {
                found.Add(m.Value + " without a page");
            }
        }

        foreach (Match m in WholeFile().Matches(code))
        {
            found.Add(m.Value);
        }

        return found;
    }

    [GeneratedRegex(@"//[^\n]*|/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Comment();

    [GeneratedRegex("\"(?:[^\"\\\\\\n]|\\\\.)*\"", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StringLiteral();

    [GeneratedRegex(@"\b(?:ToList|ToArray)\b(?!Async)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SyncMaterializer();

    [GeneratedRegex(@"\b(?:ToListAsync|ToArrayAsync)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AsyncMaterializer();

    [GeneratedRegex(@"\bReadAllBytes(?:Async)?\b|\bnew\s+MemoryStream\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WholeFile();
}
