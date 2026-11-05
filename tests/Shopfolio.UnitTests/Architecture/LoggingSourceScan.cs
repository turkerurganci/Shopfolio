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

    /// <summary>
    /// The general forms of an event name that the first scan cannot see (K3): a <c>[LoggerMessage]</c> whose message names <c>{Event}</c> but whose
    /// method has no <c>string @event</c> parameter (the name would come in under another parameter, so a text could become the event), and a direct
    /// call <c>logger.LogX("{Event}…", text)</c>, whose first argument after the template must be a constant of <c>LogEvents</c>.
    /// </summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanGeneralEventForms(string text)
    {
        string normalized = text.ReplaceLineEndings("\n");
        List<(string Rule, int Line)> findings = [];

        foreach (Match declaration in EventMessageDeclaration().Matches(normalized))
        {
            if (!EventParameter().IsMatch(declaration.Groups["params"].Value))
            {
                findings.Add((EventNameNotFromTheList, LineOf(normalized, declaration.Index)));
            }
        }

        foreach (Match call in DirectEventCall().Matches(normalized))
        {
            if (!call.Groups["event"].Value.StartsWith("LogEvents.", StringComparison.Ordinal))
            {
                findings.Add((EventNameNotFromTheList, LineOf(normalized, call.Index)));
            }
        }

        return findings;
    }

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;

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

    // [LoggerMessage(Level = ..., Message = "{Event}: ...")] static partial void LogX(ILogger logger, string text)
    [GeneratedRegex(@"\[LoggerMessage\([^\]]*Message\s*=\s*""[^""]*\{Event\}[^""]*""[^\]]*\]\s*(?:\w+\s+)*?partial\s+void\s+\w+\s*\((?<params>[^)]*)\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EventMessageDeclaration();

    [GeneratedRegex(@"\bstring\s+@event\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EventParameter();

    // _logger.LogWarning("{Event}: ...", text)
    [GeneratedRegex(@"\.Log\w*\(\s*""[^""]*\{Event\}[^""]*""\s*,\s*(?<event>[^,)]+?)\s*[,)]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DirectEventCall();

    // LogFailure(_logger, LogEvents.StartupSettingsChecked, failure)
    [GeneratedRegex(@"\b(?<name>\w+)\s*\(\s*[\w.]+\s*,\s*(?<event>[^,)]+?)\s*[,)]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Call();
}
