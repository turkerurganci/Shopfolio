using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the database session times (T233; 09 10.7 item 2): the three server-side times reach a connection only through the
/// connection string that <c>DatabaseConnection</c> writes, so no <c>SET statement_timeout</c>, <c>SET lock_timeout</c>,
/// <c>SET idle_in_transaction_session_timeout</c>, <c>SET LOCAL</c> or <c>set_config</c> of those names stands in the application source.
/// Two more rules of 09 10.7 item 4 ride on the same scan: the provider's automatic retry (<c>EnableRetryOnFailure</c>) is never turned on and
/// the error codes of the three times (<c>55P03</c>, <c>57014</c>, <c>25P03</c>) are named nowhere in the source, so no code catches or turns
/// them. The one exception is the <c>DatabaseConnection</c> file itself; the test projects are not scanned at all. The scan reads the whole text
/// (not line by line), so a statement split over several lines is found too; full-line comments are blanked first and keep their line.
/// </summary>
internal static partial class DatabaseSessionSourceScan
{
    public const string SessionTimeoutStatement = "changes a session time with SET (the times belong to the connection string that DatabaseConnection writes)";
    public const string SessionLocalStatement = "uses SET LOCAL (a session time is not changed inside a transaction)";
    public const string SessionTimeoutFunction = "changes a session time with set_config (the times belong to the connection string that DatabaseConnection writes)";

    public const string RetryStrategy = "turns on the provider's automatic retry (EnableRetryOnFailure; 09 10.7 item 4 keeps it off)";

    public const string SessionTimeoutHandled = "handles the error of a session time (lock wait, cancelled query, idle transaction); it goes up unchanged (09 10.7 item 4)";

    /// <summary>The one source file that may name the session settings.</summary>
    public const string AllowedFile = "src/Shopfolio.Web/Infrastructure/DatabaseConnection.cs";

    /// <summary>The scan applies to every application source file but <see cref="AllowedFile"/>.</summary>
    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return !string.Equals(path, AllowedFile, StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, SessionTimeoutStatement, SetTimeout(), clean);
        Add(findings, SessionLocalStatement, SetLocal(), clean);
        Add(findings, SessionTimeoutFunction, SetConfigTimeout(), clean);
        Add(findings, RetryStrategy, EnableRetry(), clean);
        Add(findings, SessionTimeoutHandled, SessionErrorCode(), clean);
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
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"\bSET\s+(?:SESSION\s+)?(?:statement_timeout|lock_timeout|idle_in_transaction_session_timeout)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetTimeout();

    [GeneratedRegex(@"\b(?:55P03|57014|25P03)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SessionErrorCode();

    [GeneratedRegex(@"\bEnableRetryOnFailure\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EnableRetry();

    [GeneratedRegex(@"\bSET\s+LOCAL\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetLocal();

    [GeneratedRegex(@"\bset_config\s*\(\s*['""]?(?:statement_timeout|lock_timeout|idle_in_transaction_session_timeout)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetConfigTimeout();
}
