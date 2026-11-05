using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// T201 KK3 and 09 19.2 item 3: the disposal code writes no retention number of its own - over the real files and on a deliberate violation sample.
/// </summary>
public sealed class DisposalNumberScanTests
{
    [Fact]
    public void DisposalFolder_WritesNoPeriodAsANumber()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file);
            if (!DisposalNumberSourceScan.AppliesTo(relative))
            {
                continue;
            }

            scanned.Add(relative);
            findings.AddRange(DisposalNumberSourceScan.Scan(File.ReadAllText(file)).Select(f => $"{relative}:{f.Line} {f.Rule}"));
        }

        Assert.Contains(scanned, f => f.EndsWith("TemporaryRecordParts.cs", StringComparison.Ordinal));
        Assert.Contains(scanned, f => f.EndsWith("LogFileDisposalPart.cs", StringComparison.Ordinal));
        Assert.DoesNotContain(scanned, f => f.EndsWith("DisposalTimings.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachNumberedPeriod_AndNotTheConstantsTheCommentsOrTheBlock()
    {
        string sample = File.ReadAllText(
            Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Disposal", "DisposalNumbers.cs"));

        IReadOnlyList<(string Rule, int Line)> findings = DisposalNumberSourceScan.Scan(sample);

        Assert.Equal(
            [
                (DisposalNumberSourceScan.PeriodLiteral, 7),
                (DisposalNumberSourceScan.PeriodLiteral, 8),
                (DisposalNumberSourceScan.PeriodLiteral, 9),
                (DisposalNumberSourceScan.PeriodLiteral, 10),
                (DisposalNumberSourceScan.PeriodLiteral, 11),
                (DisposalNumberSourceScan.PeriodLiteral, 12),
            ],
            findings);
    }

    [Fact]
    public void AppliesTo_TheDisposalFolderExceptTheTimingsFile()
    {
        Assert.True(DisposalNumberSourceScan.AppliesTo("src/Shopfolio.Web/Infrastructure/Disposal/DisposalStep.cs"));
        Assert.True(DisposalNumberSourceScan.AppliesTo(@"src\Shopfolio.Web\Infrastructure\Disposal\DisposalStep.cs"));
        Assert.False(DisposalNumberSourceScan.AppliesTo("src/Shopfolio.Web/Infrastructure/Disposal/DisposalTimings.cs"));
        Assert.False(DisposalNumberSourceScan.AppliesTo("src/Shopfolio.Web/Infrastructure/Jobs/SchedulerTimings.cs"));
    }
}
