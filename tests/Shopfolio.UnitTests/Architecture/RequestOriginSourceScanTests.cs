using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the client address and attempt record scan over the application source and shows on deliberate violation samples that each rule goes red and that
/// each allowed file is allowed for its one rule only (T215 KK3, KK8; D-64, D-112; 09 15.1 item 8, 15.4 items 1 and 5; 09 19.2 item 3). The scan reads files
/// and never writes one.
/// </summary>
public sealed class RequestOriginSourceScanTests
{
    private const string Folder = "Architecture/ViolationSamples/RequestOrigin/";
    private const string Elsewhere = "src/Shopfolio.Web/Pages/Account/Misuse.cs";

    private static string Sample(string name) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", Folder + name));

    private static (string Rule, int Line)[] Only(string rule, IEnumerable<(string Rule, int Line)> findings) => [.. findings.Where(f => f.Rule == rule)];

    [Fact]
    public void ApplicationSource_ReadsTheAddressInOnePlace_TrustsTheProxyInOnePlace_AndWritesAttemptRecordsInOnePlace()
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
            foreach ((string rule, int line) in RequestOriginSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{path}:{line} {rule}");
            }
        }

        foreach (string named in NamedFiles())
        {
            Assert.Contains(named, scanned);
        }

        Assert.Empty(findings);
    }

    private static string[] NamedFiles() =>
    [
        RequestOriginSourceScan.ClientAddressFile,
        RequestOriginSourceScan.ForwardedHeadersFile,
        RequestOriginSourceScan.AttemptCounterFile,
        RequestOriginSourceScan.AttemptRecordFile,
        RequestOriginSourceScan.AttemptSubjectFile,
        RequestOriginSourceScan.DbContextFile,
        RequestOriginSourceScan.DisposalFile,
    ];

    [Fact]
    public void NamedFiles_ExistInTheApplicationSource()
    {
        string root = RepositoryRoot.Find();

        Assert.All(NamedFiles(), file => Assert.True(File.Exists(Path.Combine(root, file)), file));
    }

    [Fact]
    public void Scan_OnTheViolationSample_ReportsEveryCallShapeOfEveryRule()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(Elsewhere, Sample("Misuse.cs"));

        Assert.Equal(
            [
                (RequestOriginSourceScan.RawRemoteAddress, 5),
                (RequestOriginSourceScan.RawRemoteAddress, 6),
                (RequestOriginSourceScan.RawRemoteAddress, 8),
                (RequestOriginSourceScan.ForwardedHeadersOutsideSetup, 9),
                (RequestOriginSourceScan.ForwardedHeadersOutsideSetup, 10),
                (RequestOriginSourceScan.ForwardedHeadersOutsideSetup, 11),
                (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 14),
                (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 15),
                (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 19),
                (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 20),
                (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 22),
                (RequestOriginSourceScan.AttemptSubjectMadeOutsideItsType, 23),
                (RequestOriginSourceScan.AttemptSubjectMadeOutsideItsType, 24),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheClientAddressFile_IsAllowedTheRawAddressOnly()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(RequestOriginSourceScan.ClientAddressFile, Sample("Misuse.cs"));

        Assert.Empty(Only(RequestOriginSourceScan.RawRemoteAddress, findings));
        Assert.NotEmpty(Only(RequestOriginSourceScan.ForwardedHeadersOutsideSetup, findings));
        Assert.NotEmpty(Only(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, findings));
        Assert.NotEmpty(Only(RequestOriginSourceScan.AttemptSubjectMadeOutsideItsType, findings));
    }

    [Fact]
    public void Scan_TheForwardedHeadersFile_IsAllowedTheTrustOnly()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(RequestOriginSourceScan.ForwardedHeadersFile, Sample("Misuse.cs"));

        Assert.Empty(Only(RequestOriginSourceScan.ForwardedHeadersOutsideSetup, findings));
        Assert.NotEmpty(Only(RequestOriginSourceScan.RawRemoteAddress, findings));
        Assert.NotEmpty(Only(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, findings));
    }

    [Fact]
    public void Scan_TheCounterFile_MayCreateAndQueryRecordsButNotConstructThem()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(RequestOriginSourceScan.AttemptCounterFile, Sample("Misuse.cs"));

        // Lines 14, 15 and 22 are the counter's own business; the two constructions (19, 20) stay the record class's.
        Assert.Equal(
            [(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 19), (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 20)],
            Only(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, findings));
    }

    [Fact]
    public void Scan_TheRecordFile_MayConstructTheRecordButNotCreateOrWriteIt()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(RequestOriginSourceScan.AttemptRecordFile, Sample("Misuse.cs"));

        Assert.Equal(
            [(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 14), (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 15)],
            Only(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, findings));
    }

    [Fact]
    public void Scan_TheSubjectFile_IsAllowedTheConstructorOnly()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(RequestOriginSourceScan.AttemptSubjectFile, Sample("Misuse.cs"));

        Assert.Empty(Only(RequestOriginSourceScan.AttemptSubjectMadeOutsideItsType, findings));
        Assert.NotEmpty(Only(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, findings));
    }

    [Fact]
    public void Scan_TheContextFile_MayDeclareTheSetButNotUseIt()
    {
        const string declaration = "public DbSet<AttemptRecord> AttemptRecords => Set<AttemptRecord>();";
        const string use = "void X() { AttemptRecords.Add(row); Set<AttemptRecord>().Add(row); this.AttemptRecords\n.ExecuteDelete(); }";

        Assert.Empty(RequestOriginSourceScan.Scan(RequestOriginSourceScan.DbContextFile, declaration));
        Assert.NotEmpty(RequestOriginSourceScan.Scan(RequestOriginSourceScan.DbContextFile, use));
        Assert.NotEmpty(RequestOriginSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other.cs", declaration));
    }

    [Fact]
    public void Scan_TheDisposalFile_MayDeleteInTheAttemptPartAndDoesNothingElseWithRecords()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(RequestOriginSourceScan.DisposalFile, Sample("DisposalMisuse.cs"));

        Assert.Equal(
            [
                (RequestOriginSourceScan.AttemptRecordMutatedByDisposal, 10),
                (RequestOriginSourceScan.AttemptRecordMutatedByDisposal, 11),
                (RequestOriginSourceScan.AttemptRecordMutatedByDisposal, 12),
                (RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 20),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheDisposalFile_OutsideItsOwnPath_GetsNoDispensation()
    {
        IReadOnlyList<(string Rule, int Line)> findings = RequestOriginSourceScan.Scan(Elsewhere, Sample("DisposalMisuse.cs"));

        Assert.All(Only(RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, findings), f => Assert.True(f.Line is 9 or 11 or 12 or 20));
        Assert.Contains((RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 9), findings);
        Assert.Contains((RequestOriginSourceScan.AttemptRecordWriteOutsideCounter, 11), findings);
    }

    [Fact]
    public void Scan_GeneratedMigrations_AreNotScanned()
    {
        const string snapshot = "b.Entity(\"Shopfolio.Infrastructure.Security.AttemptRecord\", b => { }); var x = new AttemptRecord();";

        Assert.Empty(RequestOriginSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Migrations/AppDbContextModelSnapshot.cs", snapshot));
        Assert.NotEmpty(RequestOriginSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other/AppDbContextModelSnapshot.cs", snapshot));
    }

    [Fact]
    public void Scan_ACommentLineAndTheApplicationsOwnWrapperNames_AreNotFindings()
    {
        const string text = """
            // RemoteIpAddress, X-Forwarded-For, new AttemptRecord { }, AttemptRecords.Add(x), new AttemptSubject(a, b)
            services.AddForwardedHeadersRules();
            app.UseForwardedHeadersRules();
            AttemptRecordDisposalPart.WindowMinutes.ToString();
            new AttemptRecordConfiguration();
            """;

        Assert.Empty(RequestOriginSourceScan.Scan(Elsewhere, text));
    }
}
