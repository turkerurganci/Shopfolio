using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The split of the accessibility pass in clusters that each fit one call (T604 D-1736; 12 12.2 (5)) and the one retry of a crashed page. No browser: the record of the criteria is written from made-up observations, so what each cluster
/// checks, and that the clusters together cover every part of the record, is shown without the 590 seconds a Firefox pass takes.
/// </summary>
public sealed class AccessibilityClusterTests
{
    private static readonly string[] Screens = ["E-01", "E-02", "E-03"];

    private static Observation Made(string row, string way, string step, bool ok = true) => new(row, way, "firefox", 1280, step, "beklenen", "gözlenen", ok, null);

    /// <summary>The observations of the audit's own parts: the five headings and the page facts of every screen.</summary>
    private static IEnumerable<Observation> AuditObservations(string way) =>
        Screens.SelectMany(screen => AccessibilityAuditTests.Headings.Select(h => Made(screen, way, $"elle denetim, {h}: durum")).Append(Made(screen, way, "sayfa olguları: durum")));

    /// <summary>The observations of the scan's part: the automatic scan of every screen.</summary>
    private static IEnumerable<Observation> ScanObservations(string way) => Screens.Select(screen => Made(screen, way, "otomatik tarama: durum"));

    /// <summary>The observations of the common part: the cross-screen tests' rows (the change of a value is scripted only, the others are made in each way).</summary>
    private static IEnumerable<Observation> CommonObservations(string way) =>
        [Made("VAL-A784", "betikli", "değer değişimi"), Made("VAL-A341", way, "yardım"), Made("VAL-A342", way, "tekrarlı giriş"), Made("VAL-A345", way, "klavye akışı")];

    private static (List<string> Gaps, List<string> NotMeasured) Write(AccessibilityCluster cluster, IEnumerable<Observation> record, string way = "betiksiz")
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("t604a-record-");
        try
        {
            return AccessibilityAuditTests.WriteRecordOfCriteria(directory.FullName, "firefox", way, Screens, [.. record], [], cluster);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("betikli")]
    [InlineData("betiksiz")]
    public void EachCluster_IsCompleteWithTheObservationsOfItsOwnParts_AndOnlyThose_D1736(string way)
    {
        (List<string> auditGaps, List<string> auditNotMeasured) = Write(AccessibilityCluster.Audit, AuditObservations(way), way);
        Assert.Empty(auditGaps);
        Assert.Empty(auditNotMeasured);

        (List<string> scanGaps, List<string> scanNotMeasured) = Write(AccessibilityCluster.Scan, ScanObservations(way).Concat(CommonObservations(way)), way);
        Assert.Empty(scanGaps);
        Assert.Empty(scanNotMeasured);

        (List<string> wholeGaps, List<string> wholeNotMeasured) = Write(AccessibilityCluster.Whole, AuditObservations(way).Concat(ScanObservations(way)).Concat(CommonObservations(way)), way);
        Assert.Empty(wholeGaps);
        Assert.Empty(wholeNotMeasured);
    }

    [Fact]
    public void ACluster_ThatMissesWhatItMeasures_IsIncomplete_AndTheWholePassNeedsEveryPart_D1736()
    {
        // The scan cluster run without the scan, or without the cross-screen tests, ends incomplete; so does the whole pass run with only the audit (what a run of the audit class alone used to end with).
        Assert.Contains(Write(AccessibilityCluster.Scan, CommonObservations("betiksiz")).NotMeasured, m => m.Contains(WcagCriteria.ScanStep, StringComparison.Ordinal));
        Assert.Contains(Write(AccessibilityCluster.Scan, ScanObservations("betiksiz")).NotMeasured, m => m.StartsWith("ortak /", StringComparison.Ordinal));
        Assert.NotEmpty(Write(AccessibilityCluster.Whole, AuditObservations("betiksiz")).NotMeasured);
        Assert.NotEmpty(Write(AccessibilityCluster.Whole, AuditObservations("betiksiz").Concat(ScanObservations("betiksiz"))).NotMeasured);

        // A run whose class filter does not match its cluster is caught: the audit's observations under the scan cluster measure nothing the scan cluster needs.
        Assert.NotEmpty(Write(AccessibilityCluster.Scan, AuditObservations("betiksiz")).NotMeasured);
    }

    [Fact]
    public void AnAuditCluster_LeavesTheScanAndTheCommonPartsToTheOtherCluster_WithoutFailingOnThem_D1736()
    {
        (List<string> gaps, List<string> notMeasured) = Write(AccessibilityCluster.Audit, AuditObservations("betiksiz"));
        Assert.Empty(gaps);
        Assert.Empty(notMeasured);

        // What the audit measures is still failed when it is missing or red: the cluster does not forgive its own part.
        Assert.NotEmpty(Write(AccessibilityCluster.Audit, []).Gaps);
        Assert.NotEmpty(Write(AccessibilityCluster.Audit, AuditObservations("betiksiz").Append(Made("E-02", "betiksiz", "elle denetim, klavye: durum", ok: false))).Gaps);
        Assert.NotEmpty(Write(AccessibilityCluster.Scan, ScanObservations("betiksiz").Concat(CommonObservations("betiksiz")).Append(Made("E-02", "betiksiz", "otomatik tarama: durum", ok: false))).Gaps);
    }

    [Fact]
    public void TheClusters_TogetherCoverEveryPartOfTheRecord_AndEveryEvidenceOfEveryCriterion_D1736()
    {
        foreach (RecordPart part in Enum.GetValues<RecordPart>())
        {
            Assert.True(AccessibilityCluster.Split.Any(c => c.Measures(part)), $"No cluster measures the record part {part}.");
            Assert.True(AccessibilityCluster.Whole.Measures(part), $"The whole pass does not measure the record part {part}.");
        }

        foreach (WcagCriteria.Criterion criterion in WcagCriteria.All.Where(c => c.Applies))
        {
            foreach (string evidence in criterion.Evidence)
            {
                Assert.True(AccessibilityCluster.Split.Any(c => c.Measures(AccessibilityCluster.PartOf(evidence))), $"The evidence '{evidence}' of {criterion.Id} is measured by no cluster.");
            }
        }

        // The parts are split, not repeated: a part measured by two clusters would be paid for twice.
        foreach (RecordPart part in Enum.GetValues<RecordPart>())
        {
            Assert.Single(AccessibilityCluster.Split, c => c.Measures(part));
        }
    }

    [Fact]
    public void TheClasses_OfTheClusters_AreTheClassesOfTheWholePass_EachInOneCluster_D1736()
    {
        Type[] split = [.. AccessibilityCluster.Split.SelectMany(c => c.Classes)];
        Assert.Equal(split.Length, split.Distinct().Count());
        Assert.Equal([.. AccessibilityCluster.Whole.Classes.OrderBy(t => t.Name, StringComparer.Ordinal)], [.. split.OrderBy(t => t.Name, StringComparer.Ordinal)]);
        foreach (AccessibilityCluster cluster in AccessibilityCluster.Split)
        {
            Assert.All(cluster.Classes, t => Assert.True(t.IsSubclassOf(typeof(BrowserTestBase)), $"{t.Name} is not a browser test class."));

            // The completeness check of a cluster is registered by the audit class or the scan class (AccessibilityScanTests.RegisterCompleteness): a cluster with neither would end with no check.
            Assert.True(cluster.Classes.Contains(typeof(AccessibilityAuditTests)) || cluster.Classes.Contains(typeof(AccessibilityScanTests)), $"The cluster {cluster.Name} has no class that registers its completeness check.");
        }
    }

    [Fact]
    public void TheCluster_IsChosenByName_AndAnUnknownNameFailsTheRun_D1736()
    {
        Assert.Same(AccessibilityCluster.Whole, AccessibilityCluster.Named(null));
        Assert.Same(AccessibilityCluster.Whole, AccessibilityCluster.Named("  "));
        Assert.Same(AccessibilityCluster.Audit, AccessibilityCluster.Named("audit"));
        Assert.Same(AccessibilityCluster.Scan, AccessibilityCluster.Named("scan"));
        Assert.Throws<InvalidOperationException>(() => AccessibilityCluster.Named("adit"));
        Assert.Equal("SHOPFOLIO_BROWSER_CLUSTER", AccessibilityCluster.VariableName);

        // Each cluster flushes its own observation files, so each has its own folder under the output folder; the whole pass keeps the output folder itself.
        Assert.Equal("out", AccessibilityCluster.Whole.FolderIn("out"));
        Assert.Equal(System.IO.Path.Combine("out", "audit"), AccessibilityCluster.Audit.FolderIn("out"));
        Assert.NotEqual(AccessibilityCluster.Audit.FolderIn("out"), AccessibilityCluster.Scan.FolderIn("out"));
    }

    /// <summary>The CI job runs the same clusters (ci-commands.env E2E_CMD): one call per cluster with its name and its classes, and one more call for every other browser class.</summary>
    [Fact]
    public void TheCiE2eCommand_RunsTheSameClusters_WithTheirClasses_AndTheOtherBrowserClassesOnce_D1736()
    {
        string root = AppContext.BaseDirectory;
        while (!File.Exists(System.IO.Path.Combine(root, ".github", "ci-commands.env")))
        {
            root = System.IO.Path.GetDirectoryName(root) ?? throw new InvalidOperationException(".github/ci-commands.env was not found above the test's folder.");
        }

        string line = File.ReadAllLines(System.IO.Path.Combine(root, ".github", "ci-commands.env")).Last(l => l.StartsWith("E2E_CMD=", StringComparison.Ordinal));
        string[] calls = [.. line.Split(" || rc=1;", StringSplitOptions.TrimEntries).Where(c => c.Contains("dotnet test", StringComparison.Ordinal))];
        Assert.Equal(AccessibilityCluster.Split.Count + 1, calls.Length);
        foreach (AccessibilityCluster cluster in AccessibilityCluster.Split)
        {
            string call = Assert.Single(calls, c => c.Contains($"{AccessibilityCluster.VariableName}={cluster.Name} ", StringComparison.Ordinal));
            Assert.Contains("--filter-class", call, StringComparison.Ordinal);
            Assert.DoesNotContain("--filter-not-class", call, StringComparison.Ordinal);
            foreach (Type type in AccessibilityCluster.Whole.Classes)
            {
                Assert.Equal(cluster.Classes.Contains(type), call.Contains($"\"*{type.Name}\"", StringComparison.Ordinal));
            }
        }

        string others = Assert.Single(calls, c => !c.Contains(AccessibilityCluster.VariableName, StringComparison.Ordinal));
        Assert.Contains("--filter-not-class", others, StringComparison.Ordinal);
        foreach (Type type in AccessibilityCluster.Whole.Classes)
        {
            Assert.Contains($"\"*{type.Name}\"", others, StringComparison.Ordinal);
        }
    }

    // ---- the one retry of a crashed page ----

    [Fact]
    public async Task APageCrash_IsRetriedOnce_AndCounted_D1736()
    {
        List<string> noted = [];
        int runs = 0;
        await CrashRetry.RunAsync("E-01 /sepet", noted.Add, () =>
        {
            runs++;
            return runs == 1 ? throw new PlaywrightException("Page.EvaluateAsync: Page crashed") : Task.CompletedTask;
        });
        Assert.Equal(2, runs);
        Assert.Equal(["E-01 /sepet"], noted);

        runs = 0;
        noted.Clear();
        await CrashRetry.RunAsync("E-02", noted.Add, () =>
        {
            runs++;
            return Task.CompletedTask;
        });
        Assert.Equal(1, runs);
        Assert.Empty(noted);
    }

    [Fact]
    public async Task ASecondCrash_FailsTheRun_AndAnyOtherErrorIsNotRetried_D1736()
    {
        List<string> noted = [];
        int runs = 0;
        PlaywrightException second = await Assert.ThrowsAsync<PlaywrightException>(() => CrashRetry.RunAsync("E-03", noted.Add, () =>
        {
            runs++;
            throw new PlaywrightException("Page crashed");
        }));
        Assert.Contains("crashed", second.Message, StringComparison.Ordinal);
        Assert.Equal(2, runs);
        Assert.Equal(["E-03"], noted);

        foreach (Exception other in new Exception[] { new PlaywrightException("Target page, context or browser has been closed"), new PlaywrightException("Timeout 30000ms exceeded"), new InvalidOperationException("Page crashed") })
        {
            runs = 0;
            noted.Clear();
            await Assert.ThrowsAsync(other.GetType(), () => CrashRetry.RunAsync("E-04", noted.Add, () =>
            {
                runs++;
                throw other;
            }));
            Assert.Equal(1, runs);
            Assert.Empty(noted);
        }
    }

    [Fact]
    public void ARetry_IsWrittenWithItsCount_ToTheObservationFile_AndToTheRecordOfTheCriteria_D1736()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("t604a-retry-");
        try
        {
            ObservationRecord record = new(directory.FullName);
            record.NoteCrashRetry("E-01 /sepet");
            record.NoteCrashRetry("E-07 /panel");
            Assert.Equal(["E-01 /sepet", "E-07 /panel"], record.CrashRetries);
            record.Flush();
            string table = File.ReadAllText(System.IO.Path.Combine(directory.FullName, "observations.md"));
            Assert.Contains("yeniden denemesi (Firefox yük altında; yalnız 'Page crashed', iş başına bir): 2 - E-01 /sepet; E-07 /panel", table, StringComparison.Ordinal);
            Assert.Contains("\"crashRetries\":[\"E-01 /sepet\",\"E-07 /panel\"]", File.ReadAllText(System.IO.Path.Combine(directory.FullName, "observations.jsonl")), StringComparison.Ordinal);

            _ = AccessibilityAuditTests.WriteRecordOfCriteria(directory.FullName, "firefox", "betikli", Screens, [], record.CrashRetries, AccessibilityCluster.Audit);
            string criteria = File.ReadAllText(System.IO.Path.Combine(directory.FullName, "accessibility-record-firefox-betikli.md"));
            Assert.Contains("yeniden denemesi", criteria, StringComparison.Ordinal);
            Assert.Contains("iş başına bir; ikinci çökme koşuyu düşürür): 2 - E-01 /sepet; E-07 /panel", criteria, StringComparison.Ordinal);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
