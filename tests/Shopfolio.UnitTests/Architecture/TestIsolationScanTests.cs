using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the test isolation scan over the unit test project (T120 KK7, KK8; 09 19.2 item 2, 21.4 item 1) and shows on deliberate violation
/// samples that it goes red (09 19.2 item 3). The samples are assembled from parts so that the real scan does not see them as violations.
/// </summary>
public sealed class TestIsolationScanTests
{
    private const string ConsistencyFilter = "\"Consistency=true\"";

    [Fact]
    public void UnitTests_Sources_ApplyNoMigrationAndLookForNoPendingModelChanges()
    {
        List<string> findings = [];
        foreach (string file in UnitTestSources())
        {
            findings.AddRange(TestIsolationScan.Scan(File.ReadAllText(file), inConsistencyFolder: false).Select(f => $"{Path.GetRelativePath(RepositoryRoot.Find(), file)}:{f.Line} {f.Rule}"));
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void ConsistencyChecks_Sources_ReadNoConnectionStringOrEnvironmentAndOpenNoConnection()
    {
        string[] files = [.. ConsistencySources()];
        List<string> findings = [];
        foreach (string file in files)
        {
            findings.AddRange(TestIsolationScan.Scan(File.ReadAllText(file), inConsistencyFolder: true).Select(f => $"{Path.GetRelativePath(RepositoryRoot.Find(), file)}:{f.Line} {f.Rule}"));
        }

        Assert.NotEmpty(files);
        Assert.Empty(findings);
    }

    [Fact]
    public void ConsistencyChecks_Types_CarryTheConsistencyTrait_AndNoOtherTypeDoes()
    {
        Type[] types = [.. typeof(TestIsolationScanTests).Assembly.GetTypes().Where(t => t.DeclaringType is null)];

        Assert.Contains(types, t => InConsistencyFolder(t) && t.Name == "RelationshipConsistencyTests");
        Assert.Empty(TestIsolationScan.TraitProblems(types, InConsistencyFolder));
    }

    [Fact]
    public void Pipeline_RunsTheConsistencyChecksInLintOnly()
    {
        IReadOnlyDictionary<string, string> commands = CiCommands(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "ci-commands.env")));

        Assert.Empty(PipelineProblems(commands));
    }

    // ---- the scan must fail (T120 KK7, KK8; 09 19.2 item 3: on a sample, never on the source) ----

    [Theory]
    [InlineData("HasPending", "ModelChanges")]
    [InlineData("GetPending", "Migrations")]
    [InlineData("GetPending", "MigrationsAsync")]
    [InlineData("GetApplied", "Migrations")]
    public void Scan_AUnitTestThatLooksForModelChangesOrPendingMigrations_IsReported(string first, string second)
    {
        string source = $"public sealed class T\n{{\n    public void M(Context context)\n    {{\n        Assert.False(context.Database.{first}{second}().Any());\n    }}\n}}\n";

        IReadOnlyList<(string Rule, int Line)> findings = TestIsolationScan.Scan(source, inConsistencyFolder: false);

        (string Rule, int Line) finding = Assert.Single(findings);
        Assert.Equal(TestIsolationScan.ModelChangeCheck, finding.Rule);
        Assert.Equal(5, finding.Line);
    }

    [Theory]
    [InlineData("Migra", "te")]
    [InlineData("Migra", "teAsync")]
    public void Scan_AUnitTestThatAppliesMigrations_IsReported(string first, string second)
    {
        string source = $"public sealed class T\n{{\n    public void M(Context context)\n    {{\n        context.Database.{first}{second}();\n    }}\n}}\n";

        (string Rule, int Line) finding = Assert.Single(TestIsolationScan.Scan(source, inConsistencyFolder: false));

        Assert.Equal(TestIsolationScan.ModelChangeCheck, finding.Rule);
    }

    [Theory]
    [InlineData("Get", "ConnectionString", TestIsolationScan.ConfigurationRead)]
    [InlineData("Get", "EnvironmentVariable", TestIsolationScan.ConfigurationRead)]
    [InlineData("Npgsql", "Connection", TestIsolationScan.DatabaseOrNetwork)]
    [InlineData("Http", "Client", TestIsolationScan.DatabaseOrNetwork)]
    public void Scan_AConsistencyCheckThatReadsConfigurationOrOpensAConnection_IsReported(string first, string second, string rule)
    {
        string source = $"public sealed class T\n{{\n    private static object Read() => new {first}{second}();\n}}\n";

        (string Rule, int Line) finding = Assert.Single(TestIsolationScan.Scan(source, inConsistencyFolder: true));

        Assert.Equal(rule, finding.Rule);
    }

    [Fact]
    public void Scan_TheSameSourceOutsideTheConsistencyFolder_IsNotAConsistencyFinding()
    {
        string source = "public sealed class T\n{\n    private static object Read() => new Http" + "Client();\n}\n";

        Assert.Empty(TestIsolationScan.Scan(source, inConsistencyFolder: false));
    }

    [Fact]
    public void Scan_OnACleanConsistencySource_ReportsNothing()
    {
        string source = "public sealed class T\n{\n    private static string Read() => File.ReadAllText(\"Docs/06_DATA_MODEL.md\");\n}\n";

        Assert.Empty(TestIsolationScan.Scan(source, inConsistencyFolder: true));
    }

    [Fact]
    public void TraitProblems_AnEqualityCheckWithoutTheTraitAndATaggedTypeOutsideTheFolder_AreReported()
    {
        // The samples are nested private types with their own test marker: the runner does not discover them and the real check skips nested types.
        Type[] samples = [typeof(UntaggedCheck), typeof(TaggedCheck), typeof(TaggedElsewhere)];

        IReadOnlyList<(string Rule, string Type)> problems = TestIsolationScan.TraitProblems(samples, t => t != typeof(TaggedElsewhere), typeof(SampleCheckAttribute));

        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.Rule == TestIsolationScan.TraitMissing && p.Type.EndsWith("UntaggedCheck", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Rule == TestIsolationScan.TraitMisplaced && p.Type.EndsWith("TaggedElsewhere", StringComparison.Ordinal));
    }

    [Fact]
    public void Pipeline_AUnitStepWithoutTheExclusion_OrALintStepWithoutTheSelection_IsReported()
    {
        Dictionary<string, string> real = CiCommands(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ".github", "ci-commands.env")));
        Dictionary<string, string> unitWithout = new(real) { ["UNIT_TEST_CMD"] = real["UNIT_TEST_CMD"].Replace("--filter-not-trait " + ConsistencyFilter, string.Empty, StringComparison.Ordinal) };
        Dictionary<string, string> lintWithout = new(real) { ["LINT_CMD"] = real["LINT_CMD"].Replace("--filter-trait " + ConsistencyFilter, string.Empty, StringComparison.Ordinal) };

        Assert.Single(PipelineProblems(unitWithout), p => p.Contains("UNIT_TEST_CMD", StringComparison.Ordinal));
        Assert.Single(PipelineProblems(lintWithout), p => p.Contains("LINT_CMD", StringComparison.Ordinal));
    }

    // ---- helpers ----

    private static bool InConsistencyFolder(Type type) => type.Namespace == "Shopfolio.UnitTests.Consistency";

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class SampleCheckAttribute : Attribute;

    private sealed class UntaggedCheck
    {
        [SampleCheck]
        public void Check() => throw new NotSupportedException();
    }

    [Trait("Consistency", "true")]
    private sealed class TaggedCheck
    {
        [SampleCheck]
        public void Check() => throw new NotSupportedException();
    }

    [Trait("Consistency", "true")]
    private sealed class TaggedElsewhere
    {
        [SampleCheck]
        public void Check() => throw new NotSupportedException();
    }

    private static List<string> PipelineProblems(IReadOnlyDictionary<string, string> commands)
    {
        List<string> problems = [];
        if (!commands["UNIT_TEST_CMD"].Contains("--filter-not-trait " + ConsistencyFilter, StringComparison.Ordinal))
        {
            problems.Add("UNIT_TEST_CMD does not exclude the Consistency trait (09 21.4 item 1).");
        }

        if (!commands["LINT_CMD"].Contains("--filter-trait " + ConsistencyFilter, StringComparison.Ordinal))
        {
            problems.Add("LINT_CMD does not select the Consistency trait (09 21.4 item 1).");
        }

        return problems;
    }

    private static Dictionary<string, string> CiCommands(string text)
    {
        return text.ReplaceLineEndings("\n").Split('\n')
            .Where(l => l.Length > 0 && !l.StartsWith('#') && l.Contains('='))
            .ToDictionary(l => l[..l.IndexOf('=')], l => l[(l.IndexOf('=') + 1)..], StringComparer.Ordinal);
    }

    private static IEnumerable<string> UnitTestSources()
    {
        string project = Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests");
        return Directory.EnumerateFiles(project, "*.cs", SearchOption.AllDirectories)
            .Where(file => !IsUnder(file, "bin") && !IsUnder(file, "obj") && !IsUnder(file, "ViolationSamples"));
    }

    private static IEnumerable<string> ConsistencySources()
    {
        return UnitTestSources().Where(file => IsUnder(file, "Consistency"));
    }

    private static bool IsUnder(string file, string directoryName)
    {
        return file.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains(directoryName, StringComparer.Ordinal);
    }
}
