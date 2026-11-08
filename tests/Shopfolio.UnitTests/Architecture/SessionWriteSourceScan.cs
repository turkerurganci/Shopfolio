using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the session writes (T342; 09 15.1 item 3; D-133; 09 19.2 item 3): the code that writes and deletes a session, writes a recognized browser mark and gives or takes
/// the one-operation reauthentication permission is <c>SessionStore</c> - and nothing else. Four rules over the application source: (1) a write on the <c>Sessions</c> or
/// <c>RecognizedBrowserMarks</c> set (Add, AddRange, Remove, Attach, Update, ExecuteDelete, ExecuteUpdate), also when the call is split over lines or follows a long query chain;
/// (2) a session or mark set taken through <c>Set&lt;Session&gt;</c> or <c>Set&lt;RecognizedBrowserMark&gt;</c>, and a new row made with <c>new Session</c>,
/// <c>new RecognizedBrowserMark</c> or their <c>For…</c> factories; (3) <c>ReauthenticationValues</c> named, or <c>GrantReauthentication</c> called; (4) the permission's two fields
/// written with <c>SetProperty</c>. Each rule has a closed list of the files that may, each with its reason, below. The scan reads the whole text; full-line comments are blanked
/// first and keep their line. The test projects are not scanned.
/// </summary>
internal static partial class SessionWriteSourceScan
{
    public const string SetWrite = "writes the Sessions or RecognizedBrowserMarks set outside SessionStore (09 15.1 item 3: one code writes and deletes sessions and marks)";
    public const string RowCreation = "makes a session or a recognized browser mark row outside SessionStore (09 15.1 item 3)";
    public const string PermissionValues = "uses ReauthenticationValues or GrantReauthentication outside SessionStore (D-133: the permission is given and taken through the session store)";
    public const string PermissionFields = "writes the reauthentication permission's fields outside ReauthenticationValues (06 4.39: written together, one place)";

    private const string Store = "src/Shopfolio.Web/Infrastructure/Security/SessionStore.cs";
    // The command mode's service list only registers the permission's writer (the modules' constructors need it, as in ForgerySetup); the command mode grants no permission (T303).
    private const string CommandHostFile = "src/Shopfolio.Web/Commands/CommandHost.cs";
    private const string Values ="src/Shopfolio.Web/Infrastructure/Security/ReauthenticationValues.cs";

    /// <summary>
    /// The closed lists, rule by rule, each file with its reason: the store is the writer; <c>ReauthenticationValues</c> is the permission's own writer that the store calls;
    /// <c>SessionAuthenticationHandler</c> writes the last-activity moment of the session it reads (05 6.1.1); the two disposal parts of <c>TemporaryRecordParts</c> delete the
    /// expired rows (06 7.2); the two entities and the context hold the row factories and the set declarations; <c>ForgerySetup</c> registers the permission's writer.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> Allowed = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        [SetWrite] =
        [
            Store,
            Values,
            "src/Shopfolio.Web/Infrastructure/Security/SessionAuthenticationHandler.cs",
            "src/Shopfolio.Web/Infrastructure/Disposal/TemporaryRecordParts.cs",
        ],
        [RowCreation] =
        [
            Store,
            "src/Shopfolio.Web/Infrastructure/Security/Session.cs",
            "src/Shopfolio.Web/Infrastructure/Security/RecognizedBrowserMark.cs",
            "src/Shopfolio.Web/Infrastructure/AppDbContext.cs",
        ],
        [PermissionValues] = [Store, Values, "src/Shopfolio.Web/Infrastructure/Security/ForgerySetup.cs", "src/Shopfolio.Web/Infrastructure/Security/Session.cs", CommandHostFile],
        [PermissionFields] = [Values],
    };

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        Add(findings, path, SetWrite, SetWriteCall(), clean);
        Add(findings, path, RowCreation, CreationCall(), clean);
        Add(findings, path, PermissionValues, ValuesName(), clean);
        Add(findings, path, PermissionFields, FieldWrite(), clean);
        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string path, string rule, Regex pattern, string text)
    {
        if (Allowed[rule].Contains(path, StringComparer.Ordinal))
        {
            return;
        }

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
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // A write call after the set's name in the same statement (no semicolon between): a query chain split over any number of lines is found.
    [GeneratedRegex(
        @"\b(?:Sessions|RecognizedBrowserMarks)\b[^;]*?\.\s*(?:Add|AddRange|AddAsync|AddRangeAsync|Remove|RemoveRange|Attach|AttachRange|Update|UpdateRange|ExecuteDelete|ExecuteDeleteAsync|ExecuteUpdate|ExecuteUpdateAsync)\s*[(<]",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex SetWriteCall();

    // The set taken through the context's generic accessor, and a row made by constructor or factory (a named or positional constructor argument list does not matter).
    [GeneratedRegex(
        @"\bSet\s*<\s*(?:Session|RecognizedBrowserMark)\s*>|\bnew\s+(?:Session|RecognizedBrowserMark)\b|\b(?:Session|RecognizedBrowserMark)\s*\.\s*For(?:Customer|Admin)\b",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex CreationCall();

    [GeneratedRegex(@"\bReauthenticationValues\b|\bGrantReauthentication\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ValuesName();

    [GeneratedRegex(@"\bSetProperty\s*\([^;]*?\b(?:ReauthenticatedOperation|ReauthenticationTokenHash)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldWrite();
}
