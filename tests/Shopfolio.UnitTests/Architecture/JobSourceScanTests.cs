using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Runs the job source scan over the application source (T220; 09 13.1 items 1, 2 and 10) and shows on a deliberate violation sample that each
/// rule goes red, and that the allowed places are full paths per rule (09 19.2 item 3). The scan reads files and never writes one.
/// </summary>
public sealed class JobSourceScanTests
{
    [Fact]
    public void ApplicationSource_StartsBackgroundWork_AndCatchesBroadly_AndTouchesTheJobTable_OnlyInTheirPlaces()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal) || !JobSourceScan.Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            foreach ((string rule, int line) in JobSourceScan.Scan(relative, File.ReadAllText(file)))
            {
                findings.Add($"{relative}:{line} {rule}");
            }
        }

        foreach (string place in JobSourceScan.ServiceFiles.Concat(JobSourceScan.BroadCatchFiles).Append(JobSourceScan.ContextFile))
        {
            Assert.Contains(place, scanned);
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnViolationSample_ReportsEachRule_AndNotTheCommentOrTheSpecificCatch()
    {
        IReadOnlyList<(string Rule, int Line)> findings = JobSourceScan.Scan("src/Shopfolio.Web/Modules/Payment/Sample.cs", ReadSample());

        Assert.Equal(
            [
                (JobSourceScan.ServiceText, 3),
                (JobSourceScan.UnownedWorkText, 5),
                (JobSourceScan.IgnoreText, 6),
                (JobSourceScan.UnownedWorkText, 7),
                (JobSourceScan.UnownedWorkText, 8),
                (JobSourceScan.BroadCatchText, 12),
                (JobSourceScan.BroadCatchText, 14),
                (JobSourceScan.BroadCatchText, 16),
                (JobSourceScan.BroadCatchText, 18),
                (JobSourceScan.JobTableText, 22),
                (JobSourceScan.JobTableText, 23),
                (JobSourceScan.JobTableText, 24),
                (JobSourceScan.JobTableText, 25),
            ],
            findings);
    }

    [Fact]
    public void Scan_InTheJobsFolder_LeavesOutOnlyThePlacesThatThatFolderOwns()
    {
        IReadOnlyList<(string Rule, int Line)> inJobs = JobSourceScan.Scan(JobSourceScan.JobsFolder + "Sample.cs", ReadSample());

        // Work and the table are the folder's; a service file and a broad catch are named files, not the folder.
        Assert.Equal([JobSourceScan.ServiceText, JobSourceScan.IgnoreText, JobSourceScan.BroadCatchText], inJobs.Select(f => f.Rule).Distinct());
    }

    [Fact]
    public void Scan_InTheNamedServiceFile_LeavesOutTheServiceRuleAndTheBroadCatch_OnlyThere()
    {
        string runner = JobSourceScan.ServiceFiles[0];

        IReadOnlyList<(string Rule, int Line)> findings = JobSourceScan.Scan(runner, ReadSample());

        Assert.DoesNotContain(findings, f => f.Rule == JobSourceScan.ServiceText);
        Assert.DoesNotContain(findings, f => f.Rule == JobSourceScan.BroadCatchText);
        Assert.Contains(findings, f => f.Rule == JobSourceScan.IgnoreText);
        Assert.DoesNotContain(findings, f => f.Rule == JobSourceScan.UnownedWorkText);
    }

    [Fact]
    public void Scan_InTheContextFile_LeavesOutOnlyTheJobTableRule()
    {
        IReadOnlyList<(string Rule, int Line)> findings = JobSourceScan.Scan(JobSourceScan.ContextFile, ReadSample());

        Assert.DoesNotContain(findings, f => f.Rule == JobSourceScan.JobTableText);
        Assert.Contains(findings, f => f.Rule == JobSourceScan.ServiceText);
        Assert.Contains(findings, f => f.Rule == JobSourceScan.UnownedWorkText);
        Assert.Contains(findings, f => f.Rule == JobSourceScan.BroadCatchText);
    }

    [Fact]
    public void Scan_AFileOfTheSameNameAsAPlaceElsewhere_IsNotAnAllowedPlace()
    {
        IReadOnlyList<(string Rule, int Line)> runner = JobSourceScan.Scan("src/Shopfolio.Web/Modules/Payment/JobRunnerService.cs", ReadSample());
        IReadOnlyList<(string Rule, int Line)> probe = JobSourceScan.Scan("src/Shopfolio.Web/Endpoints/LivenessProbe.cs", ReadSample());
        IReadOnlyList<(string Rule, int Line)> context = JobSourceScan.Scan("src/Shopfolio.Web/Modules/Payment/AppDbContext.cs", ReadSample());
        IReadOnlyList<(string Rule, int Line)> nearFolder = JobSourceScan.Scan("src/Shopfolio.Web/Modules/Jobs/Sample.cs", ReadSample());

        Assert.Contains(runner, f => f.Rule == JobSourceScan.ServiceText);
        Assert.Contains(probe, f => f.Rule == JobSourceScan.BroadCatchText);
        Assert.Contains(context, f => f.Rule == JobSourceScan.JobTableText);
        Assert.Contains(nearFolder, f => f.Rule == JobSourceScan.UnownedWorkText);
        Assert.Contains(nearFolder, f => f.Rule == JobSourceScan.JobTableText);
    }

    private static string ReadSample()
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "Jobs", "BackgroundWorkViolations.cs"));
    }
}
