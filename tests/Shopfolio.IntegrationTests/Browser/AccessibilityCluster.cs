using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>What a cell of the criteria record is measured by: the manual audit's own headings and page facts, the automatic scan of each screen, the criteria answered for all screens at once (source scan, cross-screen tests).</summary>
internal enum RecordPart
{
    AuditOwn,
    Scan,
    Common,
}

/// <summary>
/// A set of test classes of the accessibility pass that is run in one call, and the parts of the criteria record it measures (T604 D-1736). The Firefox pass of one way does not fit the 590 second limit of a call in one run, so it is
/// split in two clusters that each fit; each cluster's run checks the completeness of its own parts only (BrowserHost.DisposeAsync), and the union of the clusters covers every part - <see cref="AccessibilityRecordTests"/> breaks
/// when it does not. The cluster of a run is chosen by <see cref="VariableName"/>; unset, the run is the whole pass in one call (every part measured, as before). This is the one place the clusters are defined: the classes a
/// cluster's call filters on (the CI job's command, the way of the worker) are the ones named here, and a test compares the command with this list.
/// </summary>
internal sealed record AccessibilityCluster(string Name, Type[] Classes, RecordPart[] Parts)
{
    /// <summary>The environment variable that names the cluster of the run: <c>audit</c>, <c>scan</c>; unset or empty is the whole pass.</summary>
    internal const string VariableName = "SHOPFOLIO_BROWSER_CLUSTER";

    /// <summary>The whole pass in one call: every class, every part of the record.</summary>
    internal static readonly AccessibilityCluster Whole = new(string.Empty, [typeof(AccessibilityAuditTests), typeof(AccessibilityScanTests), typeof(AccessibilityCriteriaTests)], [RecordPart.AuditOwn, RecordPart.Scan, RecordPart.Common]);

    /// <summary>The manual audit alone: the five headings and the page facts of every screen (the longest class, ~5 minutes in Firefox).</summary>
    internal static readonly AccessibilityCluster Audit = new("audit", [typeof(AccessibilityAuditTests)], [RecordPart.AuditOwn]);

    /// <summary>The automatic scan of every screen and the cross-screen criteria tests (and the source scan of the criteria record, which needs no browser).</summary>
    internal static readonly AccessibilityCluster Scan = new("scan", [typeof(AccessibilityScanTests), typeof(AccessibilityCriteriaTests)], [RecordPart.Scan, RecordPart.Common]);

    /// <summary>The clusters the pass is split in.</summary>
    internal static readonly IReadOnlyList<AccessibilityCluster> Split = [Audit, Scan];

    /// <summary>The cluster of this run, from <see cref="VariableName"/>; a name that is not a cluster fails the run (a misspelt name must not fall back to the whole pass silently).</summary>
    internal static readonly AccessibilityCluster Current = Named(Environment.GetEnvironmentVariable(VariableName));

    internal static AccessibilityCluster Named(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? Whole
            : Split.FirstOrDefault(c => c.Name == name.Trim()) ?? throw new InvalidOperationException($"{VariableName}={name} is not a cluster of the accessibility pass: {string.Join(", ", Split.Select(c => c.Name))}.");

    internal bool Measures(RecordPart part) => Parts.Contains(part);

    /// <summary>The part of the record an evidence of a criterion belongs to (<see cref="WcagCriteria.Criterion.Evidence"/>).</summary>
    internal static RecordPart PartOf(string evidence) =>
        WcagCriteria.ScreenEvidence.Contains(evidence) && evidence != WcagCriteria.ScanStep ? RecordPart.AuditOwn : evidence == WcagCriteria.ScanStep ? RecordPart.Scan : RecordPart.Common;

    /// <summary>The folder of the cluster's record under the run's output folder: the clusters are separate runs and each flushes its own observation files, so they must not share one.</summary>
    internal string FolderIn(string output) => Name.Length == 0 ? output : System.IO.Path.Combine(output, Name);
}

/// <summary>
/// The one retry the browser tool allows (T604 D-1736): Firefox under load closes a page with "Page crashed". Only that error is retried, once per piece of work (a stop of a context); the retry is counted in the record, and a
/// second crash fails the run. Any other error is not retried.
/// </summary>
internal static class CrashRetry
{
    internal const string Message = "Page crashed";

    internal static bool IsCrash(Exception exception) => exception is PlaywrightException && exception.Message.Contains(Message, StringComparison.Ordinal);

    /// <summary>Runs the work; when it ends in a page crash, <paramref name="noted"/> is told what was retried and the work runs once more (its result, or its second crash, stands).</summary>
    internal static async Task RunAsync(string what, Action<string> noted, Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (PlaywrightException e) when (IsCrash(e))
        {
            noted(what);
            await work();
        }
    }
}
