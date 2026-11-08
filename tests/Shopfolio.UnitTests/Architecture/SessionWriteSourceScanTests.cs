using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the session write scan over the application source and shows on a deliberate violation sample that each rule goes red (T342; 09 15.1 item 3; D-133; 09 19.2 item 3).
/// The scan reads files and never writes one.
/// </summary>
public sealed class SessionWriteSourceScanTests
{
    [Fact]
    public void ApplicationSource_WritesSessionsAndMarksOnlyInTheSessionStore()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(path);
            foreach ((string rule, int line) in SessionWriteSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        Assert.Contains("src/Shopfolio.Web/Infrastructure/Security/SessionStore.cs", scanned);
        Assert.Contains("src/Shopfolio.Web/Infrastructure/Security/SessionAuthenticationHandler.cs", scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachRuleAndNotTheCommentOrTheRead()
    {
        string sample = File.ReadAllText(Path.Combine(
            RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Sessions", "SessionMisuse.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = SessionWriteSourceScan.Scan("src/Shopfolio.Web/Modules/Membership/Misuse.cs", sample);

        Assert.Equal(
            [
                (SessionWriteSourceScan.SetWrite, 5),
                (SessionWriteSourceScan.SetWrite, 6),
                (SessionWriteSourceScan.SetWrite, 9),
                (SessionWriteSourceScan.RowCreation, 10),
                (SessionWriteSourceScan.RowCreation, 11),
                (SessionWriteSourceScan.RowCreation, 12),
                (SessionWriteSourceScan.PermissionValues, 13),
                (SessionWriteSourceScan.SetWrite, 14),
                (SessionWriteSourceScan.PermissionFields, 16),
            ],
            findings);
    }

    [Fact]
    public void Scan_InEachAllowedFile_AllowsItsOwnRulesAndNothingElse()
    {
        const string text = "db.Sessions.Add(s); var r = new Session(); new ReauthenticationValues(db); db.Sessions.ExecuteUpdateAsync(s => s.SetProperty(x => x.ReauthenticationTokenHash, h));";

        Assert.Equal([(SessionWriteSourceScan.PermissionFields, 1)], SessionWriteSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Security/SessionStore.cs", text));
        Assert.Equal(
            [(SessionWriteSourceScan.RowCreation, 1), (SessionWriteSourceScan.PermissionValues, 1), (SessionWriteSourceScan.PermissionFields, 1)],
            SessionWriteSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Security/SessionAuthenticationHandler.cs", text));
        Assert.Equal(
            [(SessionWriteSourceScan.RowCreation, 1), (SessionWriteSourceScan.PermissionValues, 1), (SessionWriteSourceScan.SetWrite, 1), (SessionWriteSourceScan.PermissionFields, 1)],
            SessionWriteSourceScan.Scan("src/Shopfolio.Web/Modules/Membership/SessionService.cs", text));
    }

    [Fact]
    public void AllowedFiles_Exist()
    {
        string root = RepositoryRoot.Find();
        foreach (string path in SessionWriteSourceScan.Allowed.Values.SelectMany(p => p).Distinct(StringComparer.Ordinal))
        {
            Assert.True(File.Exists(Path.Combine(root, path)), path);
        }
    }
}
