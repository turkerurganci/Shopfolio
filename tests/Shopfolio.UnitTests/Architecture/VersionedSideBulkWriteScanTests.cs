using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Ordering;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the versioned-side bulk write scan over the application source, shows on a deliberate violation sample that each rule goes red and holds the
/// scan's sets and tables equal to the ones the two version guards cover (T208; D-95 (c); 09 10.2 items 2-4; 09 19.2 item 3). The scan reads files
/// and never writes one; the model is read without a database.
/// </summary>
public sealed class VersionedSideBulkWriteScanTests
{
    private const string PlaceholderConnectionString = "Host=localhost;Database=shopfolio_bulk_write_scan";

    private const string OtherModule = "src/Shopfolio.Web/Modules/Settings/Writer.cs";

    private const string SideModule = "src/Shopfolio.Web/Modules/Ordering/Writer.cs";

    [Fact]
    public void ApplicationSource_WritesNoRecordOfTheOrderSideOrTheProductFormByABulkStatement()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal) || !VersionedSideBulkWriteScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in VersionedSideBulkWriteScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains(scanned, f => f.EndsWith("Modules/Catalog/StockCounters.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("Modules/Ordering/OrderLocks.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void AllowedPlaces_AreFilesThatExist()
    {
        string root = RepositoryRoot.Find();

        Assert.All(VersionedSideBulkWriteScan.AllowedPlaces, place => Assert.True(File.Exists(Path.Combine(root, place)), place));
        Assert.All(VersionedSideBulkWriteScan.SideModules, module => Assert.True(Directory.Exists(Path.Combine(root, module)), module));
    }

    [Fact]
    public void AllowedPlaces_ExceptTheCountersFile_NameNoSetAndNoTableOfTheTwoSides()
    {
        string root = RepositoryRoot.Find();

        foreach (string place in VersionedSideBulkWriteScan.AllowedPlaces.Where(p => !p.EndsWith("/StockCounters.cs", StringComparison.Ordinal) && p != VersionedSideBulkWriteScan.GalleryMainMarkPlace && p != VersionedSideBulkWriteScan.DownloadCountersPlace))
        {
            // The same text under a path that is no allowed place: a closed place must not hide a write of a side's record.
            IReadOnlyList<(string Rule, int Line)> findings = VersionedSideBulkWriteScan.Scan(OtherModule, File.ReadAllText(Path.Combine(root, place)));

            Assert.True(findings.Count == 0, $"{place} writes a record of the order side or of the product form: {string.Join(", ", findings)}");
        }
    }

    [Fact]
    public void TheDownloadCountersPlace_HoldsExactlyTwoBulkStatements_BothOnlyOnTheDownloadCounter_AndNoDeleteAndNoRawWrite()
    {
        string place = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), VersionedSideBulkWriteScan.DownloadCountersPlace));
        System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.CultureInvariant;
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        Assert.Equal(2, System.Text.RegularExpressions.Regex.Count(place, "ExecuteUpdateAsync", options, timeout));
        Assert.Equal(0, System.Text.RegularExpressions.Regex.Count(place, "ExecuteDelete|ExecuteSql|SqlQuery|SaveChanges", options, timeout));
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Count(place, @"SetProperty\(i => i\.DownloadCount,", options, timeout));
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Count(place, "SetProperty", options, timeout));
        Assert.NotEmpty(VersionedSideBulkWriteScan.Scan(OtherModule, place));
    }

    [Fact]
    public void TheCartStatementsPlace_HoldsExactlyFourStatements_AllInsertsOnTheCartTables_AndNoUpdateNoDeleteAndNoSideTable()
    {
        string place = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), VersionedSideBulkWriteScan.CartStatementsPlace));
        System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.CultureInvariant;
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        Assert.Equal(4, System.Text.RegularExpressions.Regex.Count(place, @"ExecuteSql\w*\(", options, timeout));
        Assert.Equal(4, System.Text.RegularExpressions.Regex.Count(place, @"INSERT INTO (carts|cart_items) \(", options, timeout));
        Assert.Equal(0, System.Text.RegularExpressions.Regex.Count(place, @"ExecuteUpdate|ExecuteDelete|SqlQuery|SaveChanges|DELETE FROM|\bFROM\b", options, timeout));

        // The same text under a path that is no allowed place: no table of the two sides is named, and no set of them either.
        Assert.Empty(VersionedSideBulkWriteScan.Scan(OtherModule, place));
    }

    [Fact]
    public void TheGalleryMainMarkPlace_HoldsExactlyOneBulkStatement_OnlyOnTheProductImages_AndTheRestOfTheGalleryHoldsNone()
    {
        string root = RepositoryRoot.Find();
        string place = File.ReadAllText(Path.Combine(root, VersionedSideBulkWriteScan.GalleryMainMarkPlace));

        IReadOnlyList<(string Rule, int Line)> inPlace = VersionedSideBulkWriteScan.Scan(OtherModule, place);
        Assert.Equal(VersionedSideBulkWriteScan.NamedSetBulkWrite, Assert.Single(inPlace).Rule);
        Assert.Equal(1, System.Text.RegularExpressions.Regex.Count(place, "Execute(Update|Delete)Async", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Contains("db.ProductImages.Where", place, StringComparison.Ordinal);

        // The other files of the gallery are not places: a second bulk statement in any of them would be a finding.
        foreach (string file in new[] { "ProductGallery.cs", "GalleryRules.cs", "ImageAltText.cs" })
        {
            string path = "src/Shopfolio.Web/Modules/Catalog/" + file;
            Assert.DoesNotContain(path, VersionedSideBulkWriteScan.AllowedPlaces);
            Assert.Empty(VersionedSideBulkWriteScan.Scan(path, File.ReadAllText(Path.Combine(root, path))));
        }
    }

    [Fact]
    public void TheInboundNotificationInsertPlace_HoldsExactlyOneRawStatement_OnlyAnInsertOfTheInboundRecord_AndTheEndpointHoldsNone()
    {
        string root = RepositoryRoot.Find();
        string place = File.ReadAllText(Path.Combine(root, VersionedSideBulkWriteScan.InboundNotificationInsertPlace));

        Assert.Equal(1, System.Text.RegularExpressions.Regex.Count(place, @"\bExecute\w*\s*\(", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Equal(1, System.Text.RegularExpressions.Regex.Count(place, @"\b(?:INSERT|UPDATE|DELETE)\b", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Contains("INSERT INTO inbound_notification_records", place, StringComparison.Ordinal);
        Assert.Contains("ON CONFLICT DO NOTHING", place, StringComparison.Ordinal);

        // The endpoint and the processing job are not places: a bulk or raw write in either would be a finding.
        foreach (string file in new[] { "PaymentNotificationEndpoint.cs", "InboundNotificationJobHandler.cs", "PaymentNotificationRule.cs" })
        {
            string path = "src/Shopfolio.Web/Modules/Payment/" + file;
            Assert.DoesNotContain(path, VersionedSideBulkWriteScan.AllowedPlaces);
            Assert.Empty(VersionedSideBulkWriteScan.Scan(path, File.ReadAllText(Path.Combine(root, path))));
        }
    }

    [Fact]
    public void Scan_OnTheViolationSample_OutsideTheSideModules_ReportsTheNamedSetsAndTheRawStatementsOnly()
    {
        string sample = ReadSample();

        IReadOnlyList<(string Rule, int Line)> findings = VersionedSideBulkWriteScan.Scan(OtherModule, sample);

        Assert.Equal(
            [
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 7),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 8),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 11),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 12),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 13),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 18),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 19),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 21),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 44),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 45),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheViolationSample_InASideModule_ReportsEveryBulkWriteEvenThroughAVariable()
    {
        string sample = ReadSample();

        IReadOnlyList<(string Rule, int Line)> findings = VersionedSideBulkWriteScan.Scan(SideModule, sample);

        Assert.Equal(
            [
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 7),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 7),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 8),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 10),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 11),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 11),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 12),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 12),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 13),
                (VersionedSideBulkWriteScan.NamedSetBulkWrite, 13),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 18),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 18),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 19),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 19),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 20),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 21),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 23),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 28),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 29),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 38),
                (VersionedSideBulkWriteScan.BulkWriteInSideModule, 39),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 44),
                (VersionedSideBulkWriteScan.RawStatementOnSide, 45),
            ],
            findings);
    }

    [Fact]
    public void Scan_InAnAllowedPlace_IsQuiet_AndAFileOfTheSameNameElsewhereIsNotAnAllowedPlace()
    {
        string sample = ReadSample();

        Assert.Empty(VersionedSideBulkWriteScan.Scan("src/Shopfolio.Web/Modules/Catalog/StockCounters.cs", sample));
        Assert.NotEmpty(VersionedSideBulkWriteScan.Scan("src/Shopfolio.Web/Modules/Ordering/StockCounters.cs", sample));
        Assert.NotEmpty(VersionedSideBulkWriteScan.Scan("src/Shopfolio.Web/Infrastructure/StockCounters.cs", sample));
    }

    [Fact]
    public void Scan_OfAFullLineComment_IsQuiet()
    {
        Assert.Empty(VersionedSideBulkWriteScan.Scan(SideModule, "// await db.Orders.ExecuteUpdateAsync(s => s, ct); UPDATE orders SET x = 1;"));
    }

    [Theory]
    [MemberData(nameof(EverySet))]
    public void Scan_OfABulkWriteOnEachSetOfTheTwoSides_IsAFinding(string set)
    {
        string text = $"await _db.{set}.Where(x => x.Id == id).ExecuteDeleteAsync(ct);";

        Assert.Equal([(VersionedSideBulkWriteScan.NamedSetBulkWrite, 1)], VersionedSideBulkWriteScan.Scan(OtherModule, text));
    }

    [Theory]
    [MemberData(nameof(EveryTable))]
    public void Scan_OfARawWriteOnEachTableOfTheTwoSides_IsAFinding(string table)
    {
        string text = $"await db.Database.ExecuteSqlRawAsync(\"UPDATE {table} SET x = 1\");";

        Assert.Equal([(VersionedSideBulkWriteScan.RawStatementOnSide, 1)], VersionedSideBulkWriteScan.Scan(OtherModule, text));
    }

    [Theory]
    [InlineData("var rows = await db.Orders.Where(o => o.Id == id).ToListAsync(ct);")]
    [InlineData("db.Orders.Remove(order);")]
    [InlineData("await db.Sessions.Where(s => s.Id == id).ExecuteDeleteAsync(ct);")]
    [InlineData("await db.Database.SqlQuery<Guid>($\"SELECT id FROM orders FOR UPDATE\").ToListAsync(ct);")]
    [InlineData("await db.Database.ExecuteSqlAsync($\"UPDATE sessions SET x = 1\", ct);")]
    [InlineData("await db.Database.ExecuteSqlAsync($\"UPDATE order_items_view SET x = 1\", ct);")]
    public void Scan_OfAReadOfTheSidesOrAWriteOfAnotherSet_IsQuietOutsideTheSideModules(string text)
    {
        Assert.Empty(VersionedSideBulkWriteScan.Scan(OtherModule, text));
    }

    [Fact]
    public void ScansSetsAndTables_AreTheOnesTheTwoVersionGuardsCover()
    {
        IModel model = Model();
        IEntityType[] covered =
        [
            .. OrderVersionGuard.Scope.Members(model).Append(model.FindEntityType(OrderVersionGuard.Scope.RootType) ?? throw new Xunit.Sdk.XunitException("order")),
            .. ProductVersionGuard.Scope.Members(model).Append(model.FindEntityType(ProductVersionGuard.Scope.RootType) ?? throw new Xunit.Sdk.XunitException("product")),
        ];
        Dictionary<Type, string> setNames = typeof(AppDbContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .ToDictionary(p => p.PropertyType.GetGenericArguments()[0], p => p.Name);

        string[] sets = [.. covered.Select(t => setNames[t.ClrType]).Order(StringComparer.Ordinal)];
        string[] tables = [.. covered.Select(t => t.GetTableName() ?? throw new Xunit.Sdk.XunitException(t.Name)).Order(StringComparer.Ordinal)];

        Assert.Equal(sets, VersionedSideBulkWriteScan.OrderSideSets.Concat(VersionedSideBulkWriteScan.ProductFormSets).Order(StringComparer.Ordinal));
        Assert.Equal(tables, VersionedSideBulkWriteScan.Tables.Order(StringComparer.Ordinal));
        Assert.Contains(typeof(Order), covered.Select(t => t.ClrType));
        Assert.Contains(typeof(Product), covered.Select(t => t.ClrType));
    }

    public static TheoryData<string> EverySet() =>
        [.. VersionedSideBulkWriteScan.OrderSideSets.Concat(VersionedSideBulkWriteScan.ProductFormSets)];

    public static TheoryData<string> EveryTable() => [.. VersionedSideBulkWriteScan.Tables];

    private static string ReadSample() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "BulkWrites", "VersionedSides.cs"));

    private static IModel Model()
    {
        DbContextOptionsBuilder<AppDbContext> options = new();
        AppDbContext.Configure(options, PlaceholderConnectionString);
        using AppDbContext context = new(options.Options);
        return context.Model;
    }
}
