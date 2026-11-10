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
    public void ApplicationSource_HoldsEveryCallFormTheScanReads_SoTheScanIsNotBlindToOne()
    {
        string root = RepositoryRoot.Find();
        List<LogCallKind> kinds = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            kinds.AddRange(LogSourceText.Parse(File.ReadAllText(file)).CallSites().Select(s => s.Kind));
        }

        // The scope forms and Define are not used today (the rule keeps them out); the three that carry the application's templates must be seen.
        Assert.Contains(LogCallKind.Log, kinds);
        Assert.Contains(LogCallKind.LoggerMessage, kinds);
        Assert.Contains(LogCallKind.Audit, kinds);
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

    [Theory]
    [MemberData(nameof(ForbiddenNames))]
    public void ScanTemplates_AForbiddenWordInTheMiddleOrAtTheEndOfAName_IsReported(string name)
    {
        // T411 criterion 2 (D-89): the rule is "contains", not "ends with": {PhoneNumber}, {CardNumber}, {PasswordHash}, {TokenValue} are findings.
        foreach (string placeholder in new[] { "{" + name + "Number}", "{My" + name + "Hash}", "{" + name.ToLowerInvariant() + "s}" })
        {
            string source = "_logger.LogWarning(\"" + placeholder + "\", value);";

            Assert.Equal([(LogTemplateSourceScan.ForbiddenTemplateName, 1)], LogTemplateSourceScan.ScanTemplates(source));
        }
    }

    [Fact]
    public void ScanTemplates_EveryNameOfTheAllowList_IsNotReported_AndOnlyTheWholeNameIs()
    {
        // The allow list is open (ApplicationLogFormatter.AllowedPropertyNames) and exact: the names are spared, a longer name that contains one is not.
        Assert.NotEmpty(ApplicationLogFormatter.AllowedPropertyNames);
        foreach (string allowed in ApplicationLogFormatter.AllowedPropertyNames)
        {
            Assert.Empty(LogTemplateSourceScan.ScanTemplates("_logger.LogWarning(\"{Event} {" + allowed + "}\", a, b);"));
            Assert.Equal([(LogTemplateSourceScan.ForbiddenTemplateName, 1)], LogTemplateSourceScan.ScanTemplates("_logger.LogWarning(\"{Event} {" + allowed + "List}\", a, b);"));
        }
    }

    [Fact]
    public void AllowList_HoldsOnlyNamesThatContainAForbiddenWord_SoNoEntryIsDead()
    {
        Assert.All(
            ApplicationLogFormatter.AllowedPropertyNames,
            allowed => Assert.Contains(ApplicationLogFormatter.ForbiddenPropertyNames, forbidden => allowed.Contains(forbidden, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void ScanTemplates_AnIntentionalNameOfTheAllowList_IsUsedByTheApplication()
    {
        // Every allowed name is in the source: a name that nothing writes any more leaves the list.
        string root = RepositoryRoot.Find();
        string all = string.Join('\n', Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllText));

        Assert.All(ApplicationLogFormatter.AllowedPropertyNames, allowed => Assert.Contains("{" + allowed, all, StringComparison.Ordinal));
    }

    // --- T411 criterion 1 (KK1): the string forms and the comment forms, over a sample and one by one ---

    [Fact]
    public void ScanTemplates_OnTheFormsSample_ReportsEveryStringFormAndNotTheCleanLines()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LogTemplateSourceScan.ScanTemplates(Sample("TemplateForms.cs"));

        Assert.Equal(
            [
                (LogTemplateSourceScan.ForbiddenTemplateName, 9),
                (LogTemplateSourceScan.ForbiddenTemplateName, 10),
                (LogTemplateSourceScan.ForbiddenTemplateName, 11),
                (LogTemplateSourceScan.ForbiddenTemplateName, 12),
                (LogTemplateSourceScan.ForbiddenTemplateName, 13),
                (LogTemplateSourceScan.ForbiddenTemplateName, 14),
                (LogTemplateSourceScan.ForbiddenTemplateName, 15),
                (LogTemplateSourceScan.ForbiddenTemplateName, 16),
                (LogTemplateSourceScan.ForbiddenTemplateName, 17),
                (LogTemplateSourceScan.ForbiddenTemplateName, 22),
                (LogTemplateSourceScan.ForbiddenTemplateName, 25),
                (LogTemplateSourceScan.ForbiddenTemplateName, 28),
                (LogTemplateSourceScan.ForbiddenTemplateName, 36),
                (LogTemplateSourceScan.ForbiddenTemplateName, 40),
                (LogTemplateSourceScan.ForbiddenTemplateName, 42),
            ],
            findings);
    }

    [Theory]
    [InlineData("_logger.LogWarning(\"see https://x {Email}\", id);")]
    [InlineData("_logger.LogWarning(\"a // b\" + \" {Email}\", id);")]
    [InlineData("_logger.LogWarning($\"see {{Email}} {id}\", id);")]
    [InlineData("_logger.LogWarning($@\"see {{Email}} \"\"{id}\"\"\", id);")]
    [InlineData("_logger.LogWarning(@\"a \"\"quoted\"\" {Email}\", id);")]
    [InlineData("_logger.LogWarning(\"\"\"{Email}\"\"\", id);")]
    [InlineData("_logger.LogWarning(\"\"\"\"{Email} \"\"\" inside\"\"\"\", id);")]
    [InlineData("_logger.LogWarning($$\"\"\"{Email} {{id}}\"\"\", id);")]
    [InlineData("_logger.LogWarning(\"\\\" {Email}\", id);")]
    [InlineData("_logger.LogWarning(exception: GetEx(), message: \"{Email}\");")]
    [InlineData("_logger.LogWarning(GetEx(Describe(1)), \"{Email}\", id);")]
    [InlineData("LoggerMessage.DefineScope<string>(\"{Email}\");")]
    [InlineData("[LoggerMessageAttribute(LogLevel.Warning, \"{Email}\")]")]
    [InlineData("[global::Microsoft.Extensions.Logging.LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = \"{Email}\")]")]
    [InlineData("private const string T = \"{Event} {Email}\";")]
    public void ScanTemplates_EachStringForm_IsReportedOnItsLine(string source)
    {
        Assert.Equal([(LogTemplateSourceScan.ForbiddenTemplateName, 1)], LogTemplateSourceScan.ScanTemplates(source));
    }

    [Theory]
    [InlineData("// _logger.LogWarning(\"{Email}\", id);")]
    [InlineData("/* _logger.LogWarning(\"{Email}\", id); */")]
    [InlineData("_logger.LogWarning(\"{Event}\", id); // \"{Email}\"")]
    [InlineData("_logger.LogWarning($\"{order.Email}\", id);")]
    [InlineData("_logger.LogWarning(\"{Event} {OrderId}\", id); /* \" */ var x = \"{Email}\";")]
    [InlineData("var quote = '\"'; _logger.LogWarning(\"{Event}\", id);")]
    public void ScanTemplates_ACommentOrAHoleOrAStringOutsideALogCall_IsNotReported(string source)
    {
        Assert.Empty(LogTemplateSourceScan.ScanTemplates(source));
    }

    [Fact]
    public void ScanTemplates_ACommentMarkerInsideAString_HidesNothingAfterIt()
    {
        // The old comment remover cut the line at the first "//", also the one in the string: the Email after it was never read.
        const string source = "_logger.LogWarning(\"see https://example.test and {Email}\", id);";

        Assert.Equal([(LogTemplateSourceScan.ForbiddenTemplateName, 1)], LogTemplateSourceScan.ScanTemplates(source));
    }

    // --- T411 criterion 3 (KK3): the four new call forms ---

    [Fact]
    public void ScanTemplates_OnTheScopeSample_ReportsABeginScopeAForContextAPushPropertyAndAWithPropertyAndNotTheCleanOnes()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LogTemplateSourceScan.ScanTemplates(Sample("ScopeForms.cs"));

        Assert.Equal(
            [
                (LogTemplateSourceScan.ForbiddenTemplateName, 9),
                (LogTemplateSourceScan.ForbiddenTemplateName, 10),
                (LogTemplateSourceScan.RequestFieldInATemplate, 11),
                (LogTemplateSourceScan.ForbiddenTemplateName, 12),
                (LogTemplateSourceScan.ForbiddenTemplateName, 13),
            ],
            findings);
    }

    [Theory]
    [InlineData("Log.ForContext(\"Email\", value)")]
    [InlineData("Log.ForContext(propertyName: \"Email\", value: value)")]
    [InlineData("LogContext.PushProperty(\"PhoneNumber\", value)")]
    [InlineData("logger.BeginScope(\"{CardNumber}\", value)")]
    [InlineData("logger.BeginScope(new Dictionary<string, object> { [\"Token\"] = value })")]
    [InlineData("logger.BeginScope(new[] { KeyValuePair.Create(\"Secret\", value) })")]
    [InlineData("configuration.Enrich.WithProperty(\"Iban\", value)")]
    public void ScanTemplates_AScopeOrEnricherForm_NamingAForbiddenProperty_IsReported(string source)
    {
        Assert.Equal([(LogTemplateSourceScan.ForbiddenTemplateName, 1)], LogTemplateSourceScan.ScanTemplates(source));
    }

    [Theory]
    [InlineData("Log.ForContext(\"Path\", rawPath)")]
    [InlineData("logger.BeginScope(new Dictionary<string, object> { [\"method\"] = value })")]
    public void ScanTemplates_AScopeOrEnricherForm_NamingARequestField_IsReported(string source)
    {
        Assert.Equal([(LogTemplateSourceScan.RequestFieldInATemplate, 1)], LogTemplateSourceScan.ScanTemplates(source));
    }

    [Fact]
    public void ScanTemplates_APositionalLoggerMessage_IsReadLikeANamedOne()
    {
        const string positional = "[LoggerMessage(LogLevel.Warning, \"{Event} {Email}\")]";
        const string named = "[LoggerMessage(Level = LogLevel.Warning, Message = \"{Event} {Email}\")]";

        Assert.Equal(LogTemplateSourceScan.ScanTemplates(named), LogTemplateSourceScan.ScanTemplates(positional));
        Assert.Single(LogTemplateSourceScan.ScanTemplates(positional));
    }

    // --- T411 criterion 4 (KK4): an exception as a template argument ---

    [Fact]
    public void ScanTemplates_OnTheExceptionSample_ReportsTheExceptionAsAnArgumentInEveryFormAndNotTheSlot()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LogTemplateSourceScan.ScanTemplates(Sample("ExceptionArguments.cs"));

        Assert.Equal(
            [
                (LogTemplateSourceScan.ExceptionAsArgument, 16),
                (LogTemplateSourceScan.ExceptionAsArgument, 17),
                (LogTemplateSourceScan.ExceptionAsArgument, 18),
                (LogTemplateSourceScan.ExceptionAsArgument, 19),
                (LogTemplateSourceScan.ExceptionAsArgument, 26),
            ],
            findings);
    }

    [Theory]
    [InlineData("catch (Exception ex) { _logger.LogWarning(\"{Event} {Detail}\", e, ex); }")]
    [InlineData("catch (SqlException failure) { _logger.Warning(\"{Event} {Detail}\", e, failure.Message); }")]
    [InlineData("_logger.LogError(\"{Event} {Detail}\", e, ex.ToString());")]
    [InlineData("_logger.LogError(\"{Event} {Detail}\", e, result.InnerException);")]
    public void ScanTemplates_AnExceptionAfterTheTemplate_IsReported(string source)
    {
        Assert.Equal([(LogTemplateSourceScan.ExceptionAsArgument, 1)], LogTemplateSourceScan.ScanTemplates(source));
    }

    [Theory]
    [InlineData("catch (Exception ex) { _logger.LogWarning(ex, \"{Event} {JobId}\", e, id); }")]
    [InlineData("catch (Exception ex) { _logger.Warning(ex, \"{Event}\", e); }")]
    [InlineData("catch (Exception ex) { _logger.LogWarning(\"{Event}\", e); }")]
    [InlineData("_logger.LogWarning(\"{Event} {JobId}\", e, exampleId);")]
    public void ScanTemplates_TheExceptionInItsSlotOrNoException_IsNotReported(string source)
    {
        Assert.Empty(LogTemplateSourceScan.ScanTemplates(source));
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
