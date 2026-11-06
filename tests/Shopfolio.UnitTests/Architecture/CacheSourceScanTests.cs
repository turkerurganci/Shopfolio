using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the cache scans over the application (T229 criteria 1 and 7, KK1 and KK7; 09 10.4 item 6, 20 item 9; 05 7.10.1; MK-34) and shows on deliberate
/// violation samples that each rule goes red, and that the allowed places are full paths per rule (09 19.2 item 3): the cache types only in their own
/// files, the drop only in the transaction runner, no other in-memory cache, no page output cache, no separate cache server or client in the dependencies;
/// and, on the compiled code, no type that reaches a cache library of the framework or caches a page's output. The scans read files and never write one.
/// </summary>
public sealed class CacheSourceScanTests
{
    // --- The application ---

    [Fact]
    public void ApplicationSource_KeepsTheCacheInItsPlaces_DropsAreasOnlyInTheRunner_AndCachesNoPage()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal) || !CacheSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in CacheSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        foreach (string place in CacheSourceScan.DefinitionFiles.Values.Append(CacheSourceScan.RunnerFile).Append(CacheSourceScan.BusinessCalendarFile))
        {
            Assert.Contains(place, scanned);
        }

        Assert.Contains(scanned, f => f.EndsWith(".cshtml", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void TheRunner_IsWhereTheAreasAreDropped_AfterTheCommit()
    {
        string runner = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), CacheSourceScan.RunnerFile));

        int commit = runner.IndexOf("await transaction.CommitAsync(ct);", StringComparison.Ordinal);
        int drop = runner.IndexOf("_cache.Invalidate(area);", StringComparison.Ordinal);

        Assert.True(commit > 0, "The runner's commit was not found.");
        Assert.True(drop > commit, "The runner drops the areas before the commit (09 10.4 item 3).");
    }

    [Fact]
    public void DependenciesAndDeploymentFiles_NameNoSeparateCacheServerOrClient()
    {
        string root = RepositoryRoot.Find();
        List<string> files =
        [
            Path.Combine(root, "Directory.Packages.props"),
            Path.Combine(root, "src", "Shopfolio.Web", "Shopfolio.Web.csproj"),
            Path.Combine(root, "src", "Shopfolio.Web", "packages.lock.json"),
            .. Directory.EnumerateFiles(Path.Combine(root, "deploy"), "compose*.yaml"),
            .. Directory.EnumerateFiles(Path.Combine(root, "deploy", "images"), "Dockerfile", SearchOption.AllDirectories),
            Path.Combine(root, "deploy", "caddy", "Caddyfile"),
        ];
        List<string> findings = [];
        foreach (string file in files)
        {
            Assert.True(File.Exists(file), $"{file} was not found.");
            findings.AddRange(CacheSourceScan.ScanDependencies(File.ReadAllText(file)).Select(f => $"{Path.GetRelativePath(root, file)}:{f.Line} {f.Rule}"));
        }

        Assert.True(files.Count >= 8, "The scan lost files.");
        Assert.Empty(findings);
    }

    [Fact]
    public void ApplicationTypes_ReachNoCacheLibraryAndCacheNoPageOutput()
    {
        Assert.Empty(Evaluate(_application.Value, CompiledCacheRule()));
    }

    [Fact]
    public void TheCacheTypes_AreDeclaredOnceEach_InTheInfrastructureNamespace()
    {
        Type[] types = [.. typeof(Program).Assembly.GetTypes().Where(t => t.Name is "AppCache" or "CacheArea" or "CacheInvalidations")];

        Assert.Equal(["AppCache", "CacheArea", "CacheInvalidations"], types.Select(t => t.Name).Order(StringComparer.Ordinal));
        Assert.All(types, t => Assert.Equal("Shopfolio.Infrastructure", t.Namespace));
        Assert.All(types, t => Assert.False(t.IsPublic));
        Assert.Equal(typeof(AppCache), types.Single(t => t.Name == "AppCache"));
    }

    // --- The violation samples (09 19.2 item 3) ---

    [Fact]
    public void Scan_OnTheCodeSample_ReportsEachRuleAndEveryCallForm_AndNotTheCommentOrTheContainedWords()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/Sample.cs", ReadSample("CacheViolations.cs"));

        Assert.Equal(
            [
                (CacheSourceScan.DefinitionText, 3),
                (CacheSourceScan.DefinitionText, 4),
                (CacheSourceScan.DefinitionText, 5),
                (CacheSourceScan.InvalidateText, 8),
                (CacheSourceScan.InvalidateText, 9),
                (CacheSourceScan.InvalidateText, 10),
                (CacheSourceScan.InvalidateText, 11),
                (CacheSourceScan.InvalidateText, 12),
                (CacheSourceScan.InvalidateText, 13),
                (CacheSourceScan.OtherCacheText, 16),
                (CacheSourceScan.OtherCacheText, 18),
                (CacheSourceScan.OtherCacheText, 19),
                (CacheSourceScan.OtherCacheText, 20),
                (CacheSourceScan.OtherCacheText, 21),
                (CacheSourceScan.OtherCacheText, 22),
                (CacheSourceScan.OtherCacheText, 23),
                (CacheSourceScan.OtherCacheText, 24),
                (CacheSourceScan.OtherCacheText, 25),
                (CacheSourceScan.PageOutputText, 27),
                (CacheSourceScan.PageOutputText, 29),
                (CacheSourceScan.PageOutputText, 33),
                (CacheSourceScan.PageOutputText, 34),
                (CacheSourceScan.PageOutputText, 35),
                (CacheSourceScan.PageOutputText, 36),
            ],
            findings);
    }

    [Fact]
    public void Scan_OnTheRazorSample_ReportsTheOutputCacheAttributesAndTheCacheTagHelpers_AndNotTheRazorComment()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.Scan("src/Shopfolio.Web/Pages/Storefront/Sample.cshtml", ReadSample("CacheViolations.cshtml"));

        Assert.Equal(
            [
                (CacheSourceScan.PageOutputText, 2),
                (CacheSourceScan.PageOutputText, 3),
                (CacheSourceScan.OtherCacheText, 4),
                (CacheSourceScan.OtherCacheText, 5),
            ],
            findings);
    }

    [Fact]
    public void Scan_TheTagHelperRule_ReadsOnlyMarkup_NotAGenericArgumentInCode()
    {
        Assert.Empty(CacheSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/Sample.cs", "internal sealed record Holder(List<Cache> Items);"));
        Assert.Single(CacheSourceScan.Scan("src/Shopfolio.Web/Pages/Sample.cshtml", "< cache vary-by=\"x\">y</cache>"));
    }

    [Fact]
    public void Scan_InEachCacheTypesFile_LeavesOutOnlyThatTypesDeclaration()
    {
        string sample = ReadSample("CacheViolations.cs");
        Dictionary<string, int> sampleLine = new(StringComparer.Ordinal) { ["AppCache"] = 3, ["CacheArea"] = 4, ["CacheInvalidations"] = 5 };
        Assert.Equal(sampleLine.Keys.Order(StringComparer.Ordinal), CacheSourceScan.DefinitionFiles.Keys.Order(StringComparer.Ordinal));
        foreach ((string name, string file) in CacheSourceScan.DefinitionFiles)
        {
            IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.Scan(file, sample);

            List<int> declarations = [.. findings.Where(f => f.Rule == CacheSourceScan.DefinitionText).Select(f => f.Line)];
            Assert.Equal(new[] { 3, 4, 5 }.Where(line => line != sampleLine[name]), declarations);
            Assert.Contains(findings, f => f.Rule == CacheSourceScan.OtherCacheText);
            Assert.Contains(findings, f => f.Rule == CacheSourceScan.PageOutputText);
        }
    }

    [Fact]
    public void Scan_InTheRunnerFile_LeavesOutOnlyTheDropAndTheTakingOfTheAreas()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.Scan(CacheSourceScan.RunnerFile, ReadSample("CacheViolations.cs"));

        Assert.DoesNotContain(findings, f => f.Rule == CacheSourceScan.InvalidateText);
        Assert.Contains(findings, f => f.Rule == CacheSourceScan.DefinitionText);
        Assert.Contains(findings, f => f.Rule == CacheSourceScan.OtherCacheText);
        Assert.Contains(findings, f => f.Rule == CacheSourceScan.PageOutputText);
    }

    [Fact]
    public void Scan_InTheCacheFiles_LeavesOutOnlyTheirOwnMember()
    {
        IReadOnlyList<(string Rule, int Line)> inCache = CacheSourceScan.Scan(CacheSourceScan.AppCacheFile, ReadSample("CacheViolations.cs"));
        IReadOnlyList<(string Rule, int Line)> inInvalidations = CacheSourceScan.Scan(CacheSourceScan.CacheInvalidationsFile, ReadSample("CacheViolations.cs"));

        // AppCache.cs defines Invalidate but not TakePendingAreas (line 13); CacheInvalidations.cs the other way round.
        Assert.Equal([13], inCache.Where(f => f.Rule == CacheSourceScan.InvalidateText).Select(f => f.Line));
        Assert.Equal([8, 9, 10, 11, 12], inInvalidations.Where(f => f.Rule == CacheSourceScan.InvalidateText).Select(f => f.Line));
    }

    [Fact]
    public void Scan_AFileOfTheSameNameAsAPlaceElsewhere_IsNotAnAllowedPlace()
    {
        string sample = ReadSample("CacheViolations.cs");

        Assert.Contains(CacheSourceScan.Scan("src/Shopfolio.Web/Modules/Ordering/TransactionRunner.cs", sample), f => f.Rule == CacheSourceScan.InvalidateText);
        Assert.Contains(CacheSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/AppCache.cs", sample), f => f.Rule == CacheSourceScan.DefinitionText && f.Line == 3);
        Assert.Contains(CacheSourceScan.Scan("src/Shopfolio.Web/Infrastructure/Caching/CacheArea.cs", sample), f => f.Rule == CacheSourceScan.DefinitionText && f.Line == 4);
        Assert.Contains(CacheSourceScan.Scan("src/Shopfolio.Web/Modules/Settings/BusinessCalendar.cs", sample), f => f.Rule == CacheSourceScan.OtherCacheText && f.Line == 25);
    }

    [Fact]
    public void Scan_InTheCalendarFile_SparesOnlyTheCalendarsOwnLazy()
    {
        IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.Scan(CacheSourceScan.BusinessCalendarFile, ReadSample("CacheViolations.cs"));

        List<int> otherCache = [.. findings.Where(f => f.Rule == CacheSourceScan.OtherCacheText).Select(f => f.Line)];
        Assert.DoesNotContain(25, otherCache);
        Assert.Contains(19, otherCache);
        Assert.Contains(20, otherCache);
        Assert.Contains(23, otherCache);
        Assert.Single(CacheSourceScan.Scan(CacheSourceScan.BusinessCalendarFile, "private static readonly Lazy<BusinessCalendar> _a = new(Load);\nprivate static readonly Lazy<string> _b = new(Text);"));
    }

    public static TheoryData<string> RefusedDependencyNames => [.. CacheSourceScan.DependencyNames];

    [Theory]
    [MemberData(nameof(RefusedDependencyNames))]
    public void DependencyScan_OnAPackageLineOfEachRefusedName_Reports(string name)
    {
        string line = $"    <PackageVersion Include=\"{name}\" Version=\"1.0.0\" />";

        IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.ScanDependencies("<Project>\n" + line + "\n</Project>");

        Assert.Equal([(CacheSourceScan.DependencyText, 2)], findings);
    }

    [Fact]
    public void DependencyScan_OnTheFrameworksCachePackagesAndOnComposeImages_Reports_AndNotOnCommentsOrContainedWords()
    {
        string text =
            "    <PackageVersion Include=\"Microsoft.Extensions.Caching.StackExchangeRedis\" Version=\"10.0.0\" />\n"
            + "      \"Microsoft.Extensions.Caching.Hybrid\": {\n"
            + "    image: valkey/valkey:8\n"
            + "    image: redis:7-alpine\n"
            + "# redis is not added\n"
            + "<!-- Memcached is not added -->\n"
            + "    <PackageVersion Include=\"Npgsql.EntityFrameworkCore.PostgreSQL\" Version=\"10.0.0\" />\n"
            + "    <PackageVersion Include=\"Gredisx\" Version=\"1\" />\n";

        IReadOnlyList<(string Rule, int Line)> findings = CacheSourceScan.ScanDependencies(text);

        Assert.Equal([1, 2, 3, 4], findings.Select(f => f.Line));
    }

    [Fact]
    public void CompiledRule_OnTheViolationSample_ReportsTheCacheLibraryUsersAndThePageOutputCache_AndNotThePlainType()
    {
        IReadOnlyList<string> violations = Evaluate(_sample.Value, CompiledCacheRule());

        Assert.Contains(violations, v => v.Contains("ForbiddenMemoryCacheUser", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.Contains("ForbiddenAliasedCacheUser", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.Contains("ForbiddenOutputCachedPage", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.Contains("ForbiddenResponseCachedPage", StringComparison.Ordinal));
        Assert.DoesNotContain(violations, v => v.Contains("AllowedPlainType", StringComparison.Ordinal));
    }

    private const string ForbiddenCacheTypes =
        @"^(Microsoft\.Extensions\.Caching|Microsoft\.AspNetCore\.OutputCaching|Microsoft\.AspNetCore\.ResponseCaching|System\.Runtime\.Caching)\."
        + @"|^Microsoft\.AspNetCore\.Mvc\.ResponseCacheAttribute$|^Microsoft\.AspNetCore\.Mvc\.TagHelpers\.(Distributed)?CacheTagHelper";

    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _application =
        new(() => new ArchLoader().LoadAssemblies(typeof(Program).Assembly).Build());

    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _sample =
        new(() => new ArchLoader().LoadAssemblies(System.Reflection.Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "CacheViolationSample.dll"))).Build());

    private static ArchRule<IType> CompiledCacheRule()
    {
        return Types().Should().NotDependOnAnyTypesThat().HaveFullNameMatching(ForbiddenCacheTypes).WithoutRequiringPositiveResults();
    }

    private static List<string> Evaluate(ArchUnitNET.Domain.Architecture architecture, ArchRule<IType> rule)
    {
        return rule.Evaluate(architecture).Where(result => !result.Passed).Select(result => result.Description).ToList();
    }

    private static string ReadSample(string name)
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Cache", name));
    }
}
