using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans of the logging rules (T230; 09 18.1 item 1, 18.2 item 3): (1) nothing outside <c>Infrastructure/Logging/</c> names the logging
/// library — modules ask for <c>ILogger&lt;T&gt;</c> and for the audit line writer; (2) a log method that takes the event name as a parameter
/// (<c>string @event</c>) is called with a constant of <c>LogEvents</c>, never with a literal or a variable, so a free text cannot become an event
/// name. Comments are not scanned.
/// </summary>
internal static partial class LoggingSourceScan
{
    public const string LibraryOutsideLoggingFolder = "LoggingLibraryNamedOutsideTheLoggingFolder";

    public const string EventNameNotFromTheList = "EventNameNotFromTheClosedList";

    private const string LoggingFolder = "src/Shopfolio.Web/Infrastructure/Logging/";

    /// <summary>True when rule (1) applies to the file: every source file outside the logging folder.</summary>
    public static bool AppliesToLibraryRule(string relativePath) =>
        !relativePath.Replace('\\', '/').Contains(LoggingFolder, StringComparison.Ordinal);

    public static IReadOnlyList<(string Rule, int Line)> ScanLibraryUse(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string code = StripComment(lines[i]);
            if (LibraryName().IsMatch(code))
            {
                findings.Add((LibraryOutsideLoggingFolder, i + 1));
            }
        }

        return findings;
    }

    public static IReadOnlyList<(string Rule, int Line)> ScanEventNames(string text)
    {
        string normalized = text.ReplaceLineEndings("\n");
        HashSet<string> methods = [.. EventMethodDeclaration().Matches(normalized).Select(m => m.Groups["name"].Value)];
        List<(string Rule, int Line)> findings = [];
        if (methods.Count == 0)
        {
            return findings;
        }

        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string code = StripComment(lines[i]);
            foreach (Match call in Call().Matches(code))
            {
                if (methods.Contains(call.Groups["name"].Value) && !call.Groups["event"].Value.StartsWith("LogEvents.", StringComparison.Ordinal))
                {
                    findings.Add((EventNameNotFromTheList, i + 1));
                }
            }
        }

        return findings;
    }

    private static string StripComment(string line)
    {
        int comment = line.IndexOf("//", StringComparison.Ordinal);
        return comment < 0 ? line : line[..comment];
    }

    [GeneratedRegex(@"\bSerilog\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LibraryName();

    // static partial void LogFailure(ILogger logger, string @event, ...)
    [GeneratedRegex(@"static\s+partial\s+void\s+(?<name>\w+)\s*\(\s*ILogger\s+\w+\s*,\s*string\s+@event\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EventMethodDeclaration();

    // LogFailure(_logger, LogEvents.StartupSettingsChecked, failure)
    [GeneratedRegex(@"\b(?<name>\w+)\s*\(\s*[\w.]+\s*,\s*(?<event>[^,)]+?)\s*[,)]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Call();
}
