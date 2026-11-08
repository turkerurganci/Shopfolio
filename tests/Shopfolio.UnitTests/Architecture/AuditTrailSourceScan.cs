using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the operation trail (T380; 09 9.2 items 1 and 5, 09 19.2 item 3; 05 4.7; 02 10.3.3, 12.2.9): (1) the <c>AuditLogs</c> set and the
/// <c>AuditLog</c> entity are touched only by the database context, the entity's own file and the module's <c>AuditTrail</c> class - no other code adds,
/// changes, removes or queries a row (any call form: the set, <c>Set&lt;AuditLog&gt;()</c>, <c>typeof</c>, a generic argument, <c>new AuditLog</c>); (2) the table
/// name <c>audit_logs</c> is written only by the migrations - the disposal function's name is another word; (3) the shared infrastructure (jobs, disposal,
/// security, logging, startup, installation settings) does not reach the trail's interface or entry: the system events and the installation commands write no row;
/// (4) the application has no data breach record module, no breach detection and no bulk breach notice (02 12.2.9; 10 KD-58): no identifier, file or route is
/// named after a breach. The scan reads whole texts (a call split over lines is found), blanks comments first and keeps their lines; the <c>.cs</c> and
/// <c>.cshtml</c> files of <c>src</c> are scanned, the test projects are not.
/// </summary>
internal static partial class AuditTrailSourceScan
{
    public const string TouchesTheTrailOutsideItsClass = "touches the AuditLogs set or the AuditLog entity outside the context, the entity file and AuditTrail (09 9.2 item 1: one writing and reading class; the trail is only added to and read)";
    public const string NamesTheTableOutsideTheMigrations = "names the table audit_logs outside the migrations (05 7.5.1: the application reaches the trail through AuditTrail; the expired rows leave through the database function only)";
    public const string SystemCodeReachesTheTrail = "reaches IAuditTrail or AuditEntry from the shared infrastructure (02 10.3.3, 05 4.7.4: system events and installation commands write no trail row)";
    public const string BreachModule = "names a data breach record, detection or bulk notice (02 12.2.9; 10 KD-58: the trail, the login record and the system logs are read for the scope; there is no such module)";

    /// <summary>
    /// The rollback check counts the trail like every order side table (T388; 05 8.5.11): its closed table list may hold the one entry <c>("audit_logs", string.Empty)</c>; any other
    /// form of the name in that file (a statement) and the entry anywhere else is still a finding.
    /// </summary>
    public const string RollbackCountFile = "src/Shopfolio.Web/Infrastructure/Backup/RollbackAudit.cs";

    /// <summary>The closed list of files that may touch the set and the entity.</summary>
    public static readonly string[] TrailFiles =
    [
        "src/Shopfolio.Web/Infrastructure/AppDbContext.cs",
        "src/Shopfolio.Web/Modules/Administration/AuditLog.cs",
        "src/Shopfolio.Web/Modules/Administration/AuditTrail.cs",
    ];

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return (path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal))
            && path.StartsWith("src/", StringComparison.Ordinal)
            && !path.Contains("/obj/", StringComparison.Ordinal)
            && !path.Contains("/bin/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        if (!TrailFiles.Contains(path, StringComparer.Ordinal))
        {
            Add(findings, TouchesTheTrailOutsideItsClass, TrailTouch(), clean);
        }

        if (!path.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal))
        {
            string tableText = path == RollbackCountFile ? RollbackListEntry().Replace(clean, string.Empty) : clean;
            Add(findings, NamesTheTableOutsideTheMigrations, TableName(), tableText);
        }

        if (path.Contains("/Infrastructure/", StringComparison.Ordinal))
        {
            Add(findings, SystemCodeReachesTheTrail, TrailInterface(), clean);
        }

        Add(findings, BreachModule, Breach(), clean);
        if (Breach().IsMatch(path))
        {
            findings.Add((BreachModule, 1));
        }

        return [.. findings.Distinct().OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    // Comments are blanked and keep their lines: block comments (and the Razor comment) by their newlines, full-line and trailing line comments by their text.
    private static string BlankComments(string text)
    {
        string withoutBlocks = BlockComment().Replace(text, m => new string('\n', m.Value.Count(c => c == '\n')));
        return string.Join('\n', withoutBlocks.Split('\n').Select(StripLineComment));
    }

    // A line comment starts at "//" that is not inside a string or character literal of the line (a URL such as "https://" is kept).
    private static string StripLineComment(string line)
    {
        bool inString = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inString)
            {
                if (c == '\\')
                {
                    i++;
                }
                else if (c == '"')
                {
                    inString = false;
                }
            }
            else if (c == '"')
            {
                inString = true;
            }
            else if (c == '\'' && i + 2 < line.Length)
            {
                i = line[i + 1] == '\\' ? line.IndexOf('\'', i + 2) : i + 2;
                if (i < 0)
                {
                    return line;
                }
            }
            else if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                return line[..i];
            }
        }

        return line;
    }

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    // The set, a generic argument or a type test of the entity, its factory, a constructor call: every way to reach the rows. The part name text "AuditLog" and the
    // disposal record type member are no such form (a member access is preceded by a dot, the name text is a string).
    [GeneratedRegex(@"\bAuditLogs\b|<\s*AuditLog\s*[>,]|\btypeof\s*\(\s*AuditLog\s*\)|\bnew\s+AuditLog\b|(?<![.\w""])AuditLog\s*\.\s*Create\b|\bnameof\s*\(\s*AuditLogs?\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TrailTouch();

    [GeneratedRegex(@"\(\s*""audit_logs""\s*,\s*string\.Empty\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RollbackListEntry();

    // The table as a word: dispose_expired_audit_logs is another word (the underscore joins it).
    [GeneratedRegex(@"(?<![A-Za-z0-9_])audit_logs(?![A-Za-z0-9_])", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TableName();

    [GeneratedRegex(@"\bIAuditTrail\b|\bAuditEntry\b|\bAuditDetails\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TrailInterface();

    [GeneratedRegex(@"breach|ihlal", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Breach();
}
