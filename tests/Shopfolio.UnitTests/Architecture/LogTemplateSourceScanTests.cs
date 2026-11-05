using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The writing layer of the masking (T231 criteria 1 and 2; 09 18.2 items 2a, 2c, 7): the scans run over the application's source and are shown on
/// deliberate violation samples to go red (09 19.2 item 3). KK1: the example call writes the internal id, not the address; KK2: a sample template
/// with a forbidden name and a sample file with <c>EnableSensitiveDataLogging</c> break the scans.
/// </summary>
public sealed class LogTemplateSourceScanTests
{
    private static readonly string[] SourceExtensions = [".cs", ".json", ".csproj", ".props", ".targets", ".config", ".yaml", ".yml"];

    public static TheoryData<string> ForbiddenNames => [.. ApplicationLogFormatter.ForbiddenPropertyNames];

    [Fact]
    public void ApplicationSource_EveryLogTemplate_NamesNoPersonalDataAndNoRequestField()
    {
        (List<string> scanned, List<string> findings) = ScanSource(".cs", LogTemplateSourceScan.ScanTemplates);

        // The scan reached the files that hold the templates: the start-up refusal lines and the audit line shapes.
        Assert.Contains(scanned, f => f.EndsWith("StartupDatabaseCheck.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("AuditLine.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void ApplicationSource_NeverSwitchesOnValuesInExceptionMessages()
    {
        (List<string> scanned, List<string> findings) = ScanSource(null, LogTemplateSourceScan.ScanExceptionMessageSwitches);

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void ScanTemplates_OnViolationSample_ReportsEachForbiddenAndRequestFieldTemplateAndNotTheCleanOnesNorTheComment()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LogTemplateSourceScan.ScanTemplates(Sample("PersonalDataTemplates.cs"));

        Assert.Equal(
            [
                (LogTemplateSourceScan.ForbiddenTemplateName, 9),
                (LogTemplateSourceScan.ForbiddenTemplateName, 10),
                (LogTemplateSourceScan.RequestFieldInATemplate, 12),
                (LogTemplateSourceScan.ForbiddenTemplateName, 16),
                (LogTemplateSourceScan.RequestFieldInATemplate, 19),
            ],
            findings);
    }

    [Fact]
    public void ScanExceptionMessageSwitches_OnViolationSample_ReportsTheMethodAndBothSpellingsOfTheKeywordAndNotTheComment()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LogTemplateSourceScan.ScanExceptionMessageSwitches(Sample("SensitiveDataLogging.cs"));

        Assert.Equal(
            [
                (LogTemplateSourceScan.ValueInExceptionMessage, 7),
                (LogTemplateSourceScan.ValueInExceptionMessage, 8),
                (LogTemplateSourceScan.ValueInExceptionMessage, 9),
            ],
            findings);
    }

    [Theory]
    [MemberData(nameof(ForbiddenNames))]
    public void ScanTemplates_AnyForbiddenName_IsReportedPlainAndWithAPrefixAndWithAFormat(string name)
    {
        foreach (string placeholder in new[] { "{" + name + "}", "{Customer" + name + "}", "{@" + name + "}", "{" + name.ToUpperInvariant() + ":l}" })
        {
            string source = "_logger.LogWarning(\"" + placeholder + "\", value);";

            Assert.Equal([(LogTemplateSourceScan.ForbiddenTemplateName, 1)], LogTemplateSourceScan.ScanTemplates(source));
        }
    }

    [Fact]
    public void ScanTemplates_ANameThatOnlyContainsAForbiddenWord_IsNotReported()
    {
        // The rule is the formatter's own: a name that ends with a forbidden one. MissingKeys ends with "Keys" and is used by the start-up line.
        Assert.Empty(LogTemplateSourceScan.ScanTemplates("_logger.LogWarning(\"{Event} {MissingKeys} {KeyCount}\", a, b, c);"));
    }

    [Fact]
    public void ExampleCall_WritesTheInternalIdAndNotTheAddress()
    {
        const string address = "a@b.com";
        Guid orderId = Guid.NewGuid();
        const string template = "{Event} {OrderId}";

        // The call is the right form (a closed event, an internal id): the scan has nothing to report and the line carries the id only.
        Assert.Empty(LogTemplateSourceScan.ScanTemplates("_logger.LogWarning(\"" + template + "\", LogEvents.JobFinished, order.Id);"));
        string line = LogCapture.Line(LogCapture.One(logger => logger.Warning(template, LogEvents.JobFinished, orderId)));

        Assert.Contains(orderId.ToString(), line, StringComparison.Ordinal);
        Assert.DoesNotContain(address, line, StringComparison.Ordinal);
    }

    [Fact]
    public void AuditLineShapes_HaveTemplatesWithNoForbiddenName()
    {
        AuditLine[] lines = [AuditLine.InstantDeletion("order", Guid.NewGuid()), AuditLine.AdminCommand("create-admin", Guid.NewGuid())];

        foreach (AuditLine line in lines)
        {
            Assert.All(LogTemplateSourceScan.Placeholders(line.Template), name =>
            {
                Assert.False(LogTemplateSourceScan.IsPersonalDataName(name), name);
                Assert.False(LogTemplateSourceScan.IsRequestFieldName(name), name);
            });
        }
    }

    private static string Sample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Logging", name));

    // extension null: every source file type of the list.
    private static (List<string> Scanned, List<string> Findings) ScanSource(string? extension, Func<string, IReadOnlyList<(string Rule, int Line)>> scan)
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            string fileExtension = Path.GetExtension(file);
            bool wanted = extension is null ? SourceExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase) : string.Equals(fileExtension, extension, StringComparison.OrdinalIgnoreCase);
            if (!wanted || path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal))
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
}
