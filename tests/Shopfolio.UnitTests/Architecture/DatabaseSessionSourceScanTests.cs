using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the database session scan (T233; 09 10.7 item 2) and the transaction body scan (09 10.7 item 5) over the application source and
/// shows on a deliberate violation sample of each that it goes red (09 19.2 item 3). The scans read files and never write one.
/// </summary>
public sealed class DatabaseSessionSourceScanTests
{
    [Fact]
    public void ApplicationSource_ChangesNoSessionTimeoutOutsideDatabaseConnection()
    {
        (List<string> scanned, List<string> findings) = ScanApplication(
            DatabaseSessionSourceScan.Applies,
            text => DatabaseSessionSourceScan.Scan(text));

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.DoesNotContain(scanned, f => f.Replace('\\', '/') == DatabaseSessionSourceScan.AllowedFile);
        Assert.Empty(findings);
    }

    [Fact]
    public void Applies_LeavesOutOnlyTheDatabaseConnectionFile()
    {
        Assert.False(DatabaseSessionSourceScan.Applies("src/Shopfolio.Web/Infrastructure/DatabaseConnection.cs"));
        Assert.False(DatabaseSessionSourceScan.Applies(@"src\Shopfolio.Web\Infrastructure\DatabaseConnection.cs"));
        Assert.True(DatabaseSessionSourceScan.Applies("src/Shopfolio.Web/Infrastructure/DatabaseTimeouts.cs"));
        Assert.True(DatabaseSessionSourceScan.Applies("src/Shopfolio.Web/Infrastructure/Jobs/DatabaseConnection.cs"));
        Assert.True(DatabaseSessionSourceScan.Applies("src/Shopfolio.Web/Infrastructure/Migrations/20261104011340_Create.cs"));
        Assert.True(DatabaseSessionSourceScan.Applies("src/Shopfolio.Web/Modules/Ordering/OrderingModule.cs"));
    }

    [Fact]
    public void SessionScan_OnViolationSample_ReportsEachFormAndNotTheCommentOrTheOtherSettings()
    {
        string sample = ReadSample("DatabaseSession", "SessionTimeoutChange.cs");

        IReadOnlyList<(string Rule, int Line)> findings = DatabaseSessionSourceScan.Scan(sample);

        Assert.Equal(
            [
                (DatabaseSessionSourceScan.SessionTimeoutStatement, 5),
                (DatabaseSessionSourceScan.SessionTimeoutStatement, 6),
                (DatabaseSessionSourceScan.SessionLocalStatement, 7),
                (DatabaseSessionSourceScan.SessionTimeoutFunction, 8),
                (DatabaseSessionSourceScan.SessionTimeoutStatement, 9),
                (DatabaseSessionSourceScan.RetryStrategy, 14),
                (DatabaseSessionSourceScan.SessionTimeoutHandled, 15),
            ],
            findings);
    }

    [Fact]
    public void ApplicationSource_UsesNoOutsideServiceInsideATransactionBody()
    {
        (List<string> scanned, List<string> findings) = ScanApplication(
            TransactionBodySourceScan.Applies,
            text => TransactionBodySourceScan.Scan(text));

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void TransactionBodyScan_OnViolationSample_ReportsEachOutsideUseInsideTheBodyAndEachWorkNotALambda_AndNotTheOnesOutsideIt()
    {
        string sample = ReadSample("TransactionBody", "OutsideCallInTransaction.cs");

        IReadOnlyList<(string Rule, int Line)> findings = TransactionBodySourceScan.Scan(sample);

        Assert.Equal(
            [
                (TransactionBodySourceScan.OutsideCallInTransaction, 5),
                (TransactionBodySourceScan.OutsideCallInTransaction, 10),
                (TransactionBodySourceScan.OutsideCallInTransaction, 15),
                (TransactionBodySourceScan.WorkNotALambda, 25),
                (TransactionBodySourceScan.WorkNotALambda, 27),
            ],
            findings);
    }

    [Fact]
    public void TransactionBodyScan_ReadsTheRealRunnersCalls_AndNotItsDeclarations()
    {
        string declarations = "interface ITransactionRunner { Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct); }";
        string lambda = "class C(ITransactionRunner runner) { Task M(CancellationToken ct) => runner.RunAsync(async token => await Work(token), ct); }";

        Assert.Empty(TransactionBodySourceScan.Scan(declarations));
        Assert.Empty(TransactionBodySourceScan.Scan(lambda));
    }

    [Fact]
    public void TransactionBodyScan_WithoutTheRunnerNamed_FindsNothing()
    {
        Assert.Empty(TransactionBodySourceScan.Scan("class C(HttpClient http) { Task M() => other.RunAsync(t => http.GetAsync(\"x\", t)); }"));
    }

    private static (List<string> Scanned, List<string> Findings) ScanApplication(Func<string, bool> applies, Func<string, IReadOnlyList<(string Rule, int Line)>> scan)
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            if (IsBuildOutput(relative) || !applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in scan(File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        return (scanned, findings);
    }

    private static string ReadSample(string folder, string file)
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", folder, file));
    }

    private static bool IsBuildOutput(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal);
    }
}
