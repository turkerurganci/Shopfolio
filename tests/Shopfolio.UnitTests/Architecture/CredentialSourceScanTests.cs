using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the sign-in record and password scan over the application source and shows on a deliberate violation sample that each rule goes red in every call shape and that
/// each allowed file is allowed for its one rule only (T341 KK8, KK9; D-64; 09 15.1 item 5; 09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class CredentialSourceScanTests
{
    private const string Folder = "Architecture/ViolationSamples/Credentials/";
    private const string Elsewhere = "src/Shopfolio.Web/Pages/Account/Misuse.cs";

    private static string Sample(string name) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", Folder + name));

    private static (string Rule, int Line)[] Only(string rule, IEnumerable<(string Rule, int Line)> findings) => [.. findings.Where(f => f.Rule == rule)];

    private static string[] NamedFiles() =>
    [
        CredentialSourceScan.LoginRecordFile,
        CredentialSourceScan.LoginRecorderFile,
        CredentialSourceScan.PasswordServiceFile,
        CredentialSourceScan.DbContextFile,
    ];

    [Fact]
    public void ApplicationSource_WritesSignInRecordsInOnePlace_AndNamesThePasswordHasherInOnePlace()
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
            foreach ((string rule, int line) in CredentialSourceScan.Scan(relative, File.ReadAllText(file)))
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

    [Fact]
    public void NamedFiles_ExistInTheApplicationSource()
    {
        string root = RepositoryRoot.Find();

        Assert.All(NamedFiles(), file => Assert.True(File.Exists(Path.Combine(root, file)), file));
    }

    [Fact]
    public void Scan_OnTheViolationSample_ReportsEveryCallShapeOfEveryRule()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CredentialSourceScan.Scan(Elsewhere, Sample("Misuse.cs"));

        Assert.Equal(
            [
                (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 7),
                (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 8),
                (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 15),
                (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 16),
                (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 18),
                (CredentialSourceScan.PasswordHasherOutsideService, 22),
                (CredentialSourceScan.PasswordHasherOutsideService, 23),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheRecorderFile_MayCreateAndWriteButNotConstructTheRecord()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CredentialSourceScan.Scan(CredentialSourceScan.LoginRecorderFile, Sample("Misuse.cs"));

        // Lines 7, 8 and 18 are the recorder's own business; the two constructions (15, 16) stay the record class's.
        Assert.Equal(
            [(CredentialSourceScan.LoginRecordMadeOutsideRecorder, 15), (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 16)],
            Only(CredentialSourceScan.LoginRecordMadeOutsideRecorder, findings));
        Assert.NotEmpty(Only(CredentialSourceScan.PasswordHasherOutsideService, findings));
    }

    [Fact]
    public void Scan_TheRecordFile_MayConstructTheRecordButNotCreateOrWriteIt()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CredentialSourceScan.Scan(CredentialSourceScan.LoginRecordFile, Sample("Misuse.cs"));

        Assert.Equal(
            [(CredentialSourceScan.LoginRecordMadeOutsideRecorder, 7), (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 8), (CredentialSourceScan.LoginRecordMadeOutsideRecorder, 18)],
            Only(CredentialSourceScan.LoginRecordMadeOutsideRecorder, findings));
    }

    [Fact]
    public void Scan_ThePasswordServiceFile_IsAllowedTheHasherOnly()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CredentialSourceScan.Scan(CredentialSourceScan.PasswordServiceFile, Sample("Misuse.cs"));

        Assert.Empty(Only(CredentialSourceScan.PasswordHasherOutsideService, findings));
        Assert.NotEmpty(Only(CredentialSourceScan.LoginRecordMadeOutsideRecorder, findings));
    }

    [Fact]
    public void Scan_TheContextFile_MayDeclareTheSetButNotUseItToWrite()
    {
        const string declaration = "public DbSet<LoginRecord> LoginRecords => Set<LoginRecord>();";
        const string write = "void X() { LoginRecords.Add(row); this.LoginRecords\n.AddRange(rows); }";

        Assert.Empty(CredentialSourceScan.Scan(CredentialSourceScan.DbContextFile, declaration));
        Assert.NotEmpty(CredentialSourceScan.Scan(CredentialSourceScan.DbContextFile, write));
        Assert.NotEmpty(CredentialSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other.cs", declaration));
    }

    [Fact]
    public void Scan_GeneratedMigrations_AreNotScanned()
    {
        const string snapshot = "b.Entity(\"Shopfolio.Infrastructure.Security.LoginRecord\", b => { }); var x = new LoginRecord(); new PasswordHasher<object>();";

        Assert.Empty(CredentialSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Migrations/AppDbContextModelSnapshot.cs", snapshot));
        Assert.NotEmpty(CredentialSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Other/AppDbContextModelSnapshot.cs", snapshot));
    }

    [Fact]
    public void Scan_ACommentLineAndTheRecordsOwnHelperNames_AreNotFindings()
    {
        const string text = """
            // LoginRecord.Create(a), LoginRecords.Add(x), new LoginRecord { }, PasswordHasher<object>
            services.AddScoped<LoginRecorder>();
            new LoginRecordConfiguration();
            LoginRecordsGuard.Instance.ToString();
            """;

        Assert.Empty(CredentialSourceScan.Scan(Elsewhere, text));
    }
}
