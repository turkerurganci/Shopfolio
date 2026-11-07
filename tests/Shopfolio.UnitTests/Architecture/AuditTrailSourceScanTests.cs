using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The audit trail's rules on the application source (T380 KK5, KK7, KK10; 09 9.2 items 1 and 5; 09 19.2 item 3): only the context, the entity file and
/// <c>AuditTrail</c> touch the trail's set and entity, only the migrations name the table, the shared infrastructure does not reach the trail, and there is no
/// breach module. Each rule goes red on a deliberate violation sample, in every call form the scan claims to see.
/// </summary>
public sealed class AuditTrailSourceScanTests
{
    private const string SamplePath = "src/Shopfolio.Web/Infrastructure/Samples/AuditTrailMisuse.cs";

    private static string Sample() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture/ViolationSamples/AuditTrail", "AuditTrailMisuse.cs"));

    [Fact]
    public void ApplicationSource_HasOneTrailClass_NoOtherTableName_NoSystemCodeInTheTrail_AndNoBreachModule()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!AuditTrailSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in AuditTrailSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        // The scan reached the files it names and a migration (the table's home), so an empty result is not an empty scan.
        Assert.All(AuditTrailSourceScan.TrailFiles, f => Assert.Contains(f, scanned));
        Assert.Contains(scanned, f => f.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.StartsWith("src/Shopfolio.Web/Infrastructure/Jobs/", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void ViolationSample_EachRuleReportsItsLines_AndNotTheCommentsTheStringsOrTheDisposalFunction()
    {
        IReadOnlyList<(string Rule, int Line)> findings = AuditTrailSourceScan.Scan(SamplePath, Sample());

        Assert.Equal(
            [
                (AuditTrailSourceScan.TouchesTheTrailOutsideItsClass, 8),
                (AuditTrailSourceScan.TouchesTheTrailOutsideItsClass, 9),
                (AuditTrailSourceScan.TouchesTheTrailOutsideItsClass, 10),
                (AuditTrailSourceScan.TouchesTheTrailOutsideItsClass, 11),
                (AuditTrailSourceScan.TouchesTheTrailOutsideItsClass, 12),
                (AuditTrailSourceScan.TouchesTheTrailOutsideItsClass, 13),
                (AuditTrailSourceScan.NamesTheTableOutsideTheMigrations, 20),
                (AuditTrailSourceScan.SystemCodeReachesTheTrail, 25),
                (AuditTrailSourceScan.SystemCodeReachesTheTrail, 26),
                (AuditTrailSourceScan.BreachModule, 28),
                (AuditTrailSourceScan.BreachModule, 30),
            ],
            findings);
    }

    [Fact]
    public void TheTrailFiles_MayTouchTheTrail_ButNotTheirNeighbours()
    {
        const string touch = "var r = db.AuditLogs.Where(x => true); var e = new AuditLog(); var s = db.Set<AuditLog>();";

        foreach (string file in AuditTrailSourceScan.TrailFiles)
        {
            Assert.DoesNotContain(AuditTrailSourceScan.Scan(file, touch), f => f.Rule == AuditTrailSourceScan.TouchesTheTrailOutsideItsClass);
        }

        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Administration/AuditTrail2.cs", touch), f => f.Rule == AuditTrailSourceScan.TouchesTheTrailOutsideItsClass);
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Administration/AdminInvitation.cs", touch), f => f.Rule == AuditTrailSourceScan.TouchesTheTrailOutsideItsClass);
    }

    [Theory]
    [InlineData("db.AuditLogs\n    .Add(row);")]
    [InlineData("db . AuditLogs . ExecuteUpdateAsync(s => s)")]
    [InlineData("var q = from l in db.AuditLogs select l;")]
    [InlineData("db.Set< AuditLog >()")]
    [InlineData("typeof ( AuditLog )")]
    [InlineData("builder.HasKey<AuditLog>(x => x.Id)")]
    [InlineData("nameof(AuditLogs)")]
    [InlineData("new\n AuditLog\n{ }")]
    public void TheTouchRule_SeesEveryCallFormOfTheSetAndTheEntity(string code)
    {
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Other.cs", code), f => f.Rule == AuditTrailSourceScan.TouchesTheTrailOutsideItsClass);
    }

    [Theory]
    [InlineData("\"SELECT * FROM audit_logs\"")]
    [InlineData("$\"DELETE FROM public.audit_logs WHERE x = {y}\"")]
    [InlineData("\"TRUNCATE AUDIT_LOGS\"")]
    [InlineData("\"UPDATE\\n audit_logs SET operation = 'x'\"")]
    public void TheTableRule_SeesTheNameInEveryStatementForm_AndNotInTheMigrations(string code)
    {
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/Other.cs", code), f => f.Rule == AuditTrailSourceScan.NamesTheTableOutsideTheMigrations);
        Assert.DoesNotContain(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Migrations/20261104011340_X.cs", code), f => f.Rule == AuditTrailSourceScan.NamesTheTableOutsideTheMigrations);
    }

    [Theory]
    [InlineData("private readonly IAuditTrail _trail;")]
    [InlineData("var e = new AuditEntry(op, null, null, null);")]
    [InlineData("var d = new AuditDetails();")]
    public void TheSystemRule_SeesTheInterfaceTheEntryAndTheDetailsInTheSharedInfrastructure_AndNotInAModule(string code)
    {
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Jobs/JobHandlers.cs", code), f => f.Rule == AuditTrailSourceScan.SystemCodeReachesTheTrail);
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Disposal/X.cs", code), f => f.Rule == AuditTrailSourceScan.SystemCodeReachesTheTrail);
        Assert.DoesNotContain(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/OrderTransitions.cs", code), f => f.Rule == AuditTrailSourceScan.SystemCodeReachesTheTrail);
    }

    [Theory]
    [InlineData("class BreachRecord { }")]
    [InlineData("class DataBreachNotifier { }")]
    [InlineData("var route = \"/panel/ihlal-bildirimi\";")]
    [InlineData("@* x *@ <a asp-page=\"/Panel/Breach\">x</a>")]
    public void TheBreachRule_SeesTheNameInAnIdentifierARouteAndAView(string code)
    {
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Administration/X.cs", code), f => f.Rule == AuditTrailSourceScan.BreachModule);
    }

    [Fact]
    public void TheBreachRule_SeesAFileNamedAfterABreach()
    {
        Assert.Contains(AuditTrailSourceScan.Scan("src/Shopfolio.Web/Modules/Administration/BreachRegister.cs", "class A { }"), f => f.Rule == AuditTrailSourceScan.BreachModule);
    }
}
