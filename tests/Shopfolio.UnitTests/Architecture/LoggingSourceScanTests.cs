using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the logging scans over the application's source (T230; 09 18.1 item 1, 18.2 item 3) and shows on deliberate violation samples that each
/// goes red (09 19.2 item 3). KK1: the configuration is in one place and no module names the library. KK7: an event name outside the closed list is
/// refused (the scan for the call sites; the formatter's refusal is in <c>ApplicationLogFormatterTests</c>).
/// </summary>
public sealed class LoggingSourceScanTests
{
    [Fact]
    public void ApplicationSource_OutsideTheLoggingFolder_NamesNoLoggingLibrary()
    {
        (List<string> scanned, List<string> findings) = ScanSource(LoggingSourceScan.AppliesToLibraryRule, LoggingSourceScan.ScanLibraryUse);

        Assert.Contains(scanned, f => f.EndsWith("Program.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.Contains("Modules", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void ApplicationSource_EveryEventNameArgument_IsAConstantOfTheClosedList()
    {
        (List<string> scanned, List<string> findings) = ScanSource(_ => true, LoggingSourceScan.ScanEventNames);

        Assert.NotEmpty(scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void AppliesToLibraryRule_LeavesOnlyTheLoggingFolderOut()
    {
        Assert.False(LoggingSourceScan.AppliesToLibraryRule("src/Shopfolio.Web/Infrastructure/Logging/LoggingSetup.cs"));
        Assert.False(LoggingSourceScan.AppliesToLibraryRule(@"src\Shopfolio.Web\Infrastructure\Logging\LoggingSetup.cs"));
        Assert.True(LoggingSourceScan.AppliesToLibraryRule("src/Shopfolio.Web/Program.cs"));
        Assert.True(LoggingSourceScan.AppliesToLibraryRule("src/Shopfolio.Web/Infrastructure/InstallationSettings/LoggingNotes.cs"));
    }

    [Fact]
    public void ScanLibraryUse_OnViolationSample_ReportsTheUsingAndTheCallAndNotTheComment()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LoggingSourceScan.ScanLibraryUse(Sample("LibraryInAModule.cs"));

        Assert.Equal([(LoggingSourceScan.LibraryOutsideLoggingFolder, 3)], findings);
    }

    [Fact]
    public void ScanEventNames_OnViolationSample_ReportsTheLiteralAndTheVariableAndNotTheConstant()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LoggingSourceScan.ScanEventNames(Sample("FreeTextEvent.cs"));

        Assert.Equal([(LoggingSourceScan.EventNameNotFromTheList, 9), (LoggingSourceScan.EventNameNotFromTheList, 10)], findings);
    }

    [Fact]
    public void ApplicationSource_EveryGeneralEventForm_NamesAConstantOfTheClosedList()
    {
        (List<string> scanned, List<string> findings) = ScanSource(_ => true, LoggingSourceScan.ScanGeneralEventForms);

        Assert.NotEmpty(scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void ScanGeneralEventForms_OnViolationSample_ReportsTheVariableTheLiteralAndTheMethodWithoutAnEventParameter()
    {
        IReadOnlyList<(string Rule, int Line)> findings = LoggingSourceScan.ScanGeneralEventForms(Sample("GeneralEventForms.cs"));

        Assert.Equal(
            [
                (LoggingSourceScan.EventNameNotFromTheList, 14),
                (LoggingSourceScan.EventNameNotFromTheList, 8),
                (LoggingSourceScan.EventNameNotFromTheList, 9),
            ],
            findings);
    }

    private static string Sample(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Logging", name));

    private static (List<string> Scanned, List<string> Findings) ScanSource(Func<string, bool> applies, Func<string, IReadOnlyList<(string Rule, int Line)>> scan)
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            string path = relative.Replace('\\', '/');
            if (path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal) || !applies(relative))
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
