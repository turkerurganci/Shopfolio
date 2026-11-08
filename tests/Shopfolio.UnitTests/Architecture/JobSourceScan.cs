using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the background work's single places (T220; 09 13.1 items 1 and 10, 8.1 item 6): a type that derives from
/// <c>BackgroundService</c> stands only in the service files of <c>Infrastructure/Jobs/</c> (<c>JobRunnerService.cs</c> and, since T221,
/// <c>SchedulerService.cs</c>); <c>BackgroundServiceExceptionBehavior.Ignore</c> stands nowhere; <c>Task.Run(</c> and a new thread stand only under
/// <c>Infrastructure/Jobs/</c>; a broad catch (<c>catch (Exception</c>, filtered or not, and a typeless <c>catch</c>) stands only in the files of
/// the places that 09 8.1 item 6 names and that exist (the job queue's row loop, the scheduler's record loop, the nightly job's wrapper, the liveness probe and the single catching layer's two files); and the job table's set, the
/// <c>Jobs</c> member of the context, a new <c>Job</c> and <c>Job.Enqueue</c> stand only under <c>Infrastructure/Jobs/</c> and in
/// <c>AppDbContext.cs</c>, which maps the table. The places are full paths, not file names: a file of the same name elsewhere is scanned.
/// The scan reads the whole text; full-line comments are blanked first and keep their line.
/// </summary>
internal static partial class JobSourceScan
{
    public const string ServiceText = "derives from BackgroundService outside the job service files (09 13.1 items 1, 10)";

    public const string IgnoreText = "sets BackgroundServiceExceptionBehavior.Ignore (09 13.1 item 1)";

    public const string UnownedWorkText = "starts work with Task.Run or a new thread outside Infrastructure/Jobs/ (09 13.1 items 1, 10)";

    public const string BroadCatchText = "has a broad catch outside the places of 09 8.1 item 6 (09 13.1 item 10)";

    public const string JobTableText = "touches the job table's set or adds a job row outside Infrastructure/Jobs/ and AppDbContext.cs (09 13.1 items 2, 10)";

    public const string JobsFolder = "src/Shopfolio.Web/Infrastructure/Jobs/";

    public const string ContextFile = "src/Shopfolio.Web/Infrastructure/AppDbContext.cs";

    /// <summary>The files that may derive from <c>BackgroundService</c>: the job runner's and the scheduler's.</summary>
    public static readonly IReadOnlyList<string> ServiceFiles =
    [
        "src/Shopfolio.Web/Infrastructure/Jobs/JobRunnerService.cs",
        "src/Shopfolio.Web/Infrastructure/Jobs/SchedulerService.cs",
    ];

    /// <summary>The files of 09 8.1 item 6's places that exist: the job queue's row loop, the scheduler's record loop and the nightly job's wrapper, the liveness probe's database check, and the single catching layer (T212: the layer writing the answer and the middleware that calls it).</summary>
    public static readonly IReadOnlyList<string> BroadCatchFiles =
    [
        "src/Shopfolio.Web/Infrastructure/Jobs/JobRunnerService.cs",
        "src/Shopfolio.Web/Infrastructure/Jobs/SchedulerScan.cs",
        "src/Shopfolio.Web/Infrastructure/Jobs/NightlyJobRunner.cs",
        "src/Shopfolio.Web/Infrastructure/LivenessProbe.cs",
        "src/Shopfolio.Web/Infrastructure/Errors/UnexpectedErrorLayer.cs",
        "src/Shopfolio.Web/Infrastructure/Errors/UnexpectedErrorSetup.cs",

        // The management commands' dispatcher (T303; 09 13.2 item 2): its top-level catch ends an unexpected error with exit code 1 and writes the error's type only.
        "src/Shopfolio.Web/Commands/CommandDispatcher.cs",
    ];

    public static bool Applies(string relativePath)
    {
        return relativePath.Replace('\\', '/').EndsWith(".cs", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (!ServiceFiles.Contains(path))
        {
            Add(findings, ServiceText, ServiceBase(), clean);
        }

        Add(findings, IgnoreText, IgnoreBehavior(), clean);
        bool inJobs = path.StartsWith(JobsFolder, StringComparison.Ordinal);
        if (!inJobs)
        {
            Add(findings, UnownedWorkText, TaskRun(), clean);
            Add(findings, UnownedWorkText, NewThread(), clean);
        }

        if (!BroadCatchFiles.Contains(path))
        {
            Add(findings, BroadCatchText, BroadCatch(), clean);
            Add(findings, BroadCatchText, TypelessCatch(), clean);
        }

        if (!inJobs && path != ContextFile)
        {
            Add(findings, JobTableText, JobTable(), clean);
        }

        findings.Sort((a, b) => a.Line.CompareTo(b.Line));
        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            int line = text.AsSpan(0, match.Index).Count('\n') + 1;
            if (!findings.Contains((rule, line)))
            {
                findings.Add((rule, line));
            }
        }
    }

    private static string BlankCommentLines(string text)
    {
        StringBuilder builder = new();
        foreach (string line in text.Split('\n'))
        {
            builder.Append(line.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : line).Append('\n');
        }

        return builder.ToString(0, builder.Length - 1);
    }

    [GeneratedRegex(@":\s*(?:[\w.]+\.)?BackgroundService\b|,\s*(?:[\w.]+\.)?BackgroundService\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ServiceBase();

    [GeneratedRegex(@"\bBackgroundServiceExceptionBehavior\s*\.\s*Ignore\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex IgnoreBehavior();

    [GeneratedRegex(@"\bTask\s*\.\s*Run\s*[(<]|\bTask\s*\.\s*Factory\s*\.\s*StartNew\s*[(<]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TaskRun();

    [GeneratedRegex(@"\bnew\s+(?:System\s*\.\s*Threading\s*\.\s*)?Thread\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NewThread();

    [GeneratedRegex(@"\bcatch\s*\(\s*(?:System\s*\.\s*)?Exception\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BroadCatch();

    [GeneratedRegex(@"\bcatch\s*(?:\{|when\b)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TypelessCatch();

    [GeneratedRegex(@"\bDbSet\s*<\s*Job\s*>|\bSet\s*<\s*Job\s*>|(?<!\bInfrastructure)\.\s*Jobs\b|\bnew\s+Job\s*\(|\bJob\s*\.\s*Enqueue\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex JobTable();
}
