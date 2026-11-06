using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the client address and attempt record rules (T215 KK3, KK8; D-64, D-112; 09 15.1 item 8, 15.4 items 1 and 5; 09 19.2 item 3). Four rules over
/// the application source: (1) the connection's <c>RemoteIpAddress</c> is read only in <see cref="ClientAddressFile"/> - every other code takes the address
/// or its axis from <c>ClientAddress</c>; (2) the forwarded headers - the middleware, its options, the <c>X-Forwarded-*</c> header names and the
/// <c>ForwardedHeaders</c> flags - are named only in <see cref="ForwardedHeadersFile"/>; (3) an attempt record is made and written only in
/// <see cref="AttemptCounterFile"/> (<c>AttemptRecord.Create</c>, the <c>AttemptRecords</c> set, any <c>&lt;AttemptRecord&gt;</c> generic), with <c>new AttemptRecord</c> in
/// its own class <see cref="AttemptRecordFile"/>, the context's set declaration in <see cref="DbContextFile"/>, and the clean-up's delete in
/// <see cref="DisposalFile"/> (inside <c>AttemptRecordDisposalPart</c> only, and only <c>ExecuteDelete</c>); (4) <c>new AttemptSubject</c> only in
/// <see cref="AttemptSubjectFile"/>, so the value of a row is made in one place. Each file is allowed for its one rule. The scan reads the whole text, so a
/// call split over several lines or carrying an extra or named argument is found; full-line comments are blanked first and keep their line. Generated
/// migrations (<c>Infrastructure/Migrations/</c>) are not scanned - they are the model's snapshot - and the test projects are not scanned.
/// </summary>
internal static partial class RequestOriginSourceScan
{
    public const string RawRemoteAddress = "reads RemoteIpAddress outside ClientAddress (09 15.1 item 8: the one reader of the connection's address)";
    public const string ForwardedHeadersOutsideSetup = "names the forwarded headers middleware, its options or an X-Forwarded header outside ForwardedHeadersSetup (09 15.1 item 8: trust is set in one place)";
    public const string AttemptRecordWriteOutsideCounter = "makes, writes or queries an attempt record outside AttemptCounter (09 15.4 item 5; D-64: one writer)";
    public const string AttemptRecordMutatedByDisposal = "changes an attempt record in the disposal part other than by ExecuteDelete (09 15.4 item 5: the clean-up only deletes)";
    public const string AttemptSubjectMadeOutsideItsType = "makes an AttemptSubject outside AttemptSubject (09 15.4 item 5; D-64: the value is made in one place)";

    public const string ClientAddressFile = "src/Shopfolio.Web/Infrastructure/Security/ClientAddress.cs";
    public const string ForwardedHeadersFile = "src/Shopfolio.Web/Infrastructure/Security/ForwardedHeadersSetup.cs";
    public const string AttemptCounterFile = "src/Shopfolio.Web/Infrastructure/Security/AttemptCounter.cs";
    public const string AttemptRecordFile = "src/Shopfolio.Web/Infrastructure/Security/AttemptRecord.cs";
    public const string AttemptSubjectFile = "src/Shopfolio.Web/Infrastructure/Security/AttemptSubject.cs";
    public const string DbContextFile = "src/Shopfolio.Web/Infrastructure/AppDbContext.cs";
    public const string DisposalFile = "src/Shopfolio.Web/Infrastructure/Disposal/TemporaryRecordParts.cs";

    private const string MigrationsFolder = "src/Shopfolio.Web/Infrastructure/Migrations/";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        if (path.StartsWith(MigrationsFolder, StringComparison.Ordinal))
        {
            return [];
        }

        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        if (!string.Equals(path, ClientAddressFile, StringComparison.Ordinal))
        {
            Add(findings, RawRemoteAddress, RemoteAddress(), clean);
        }

        if (!string.Equals(path, ForwardedHeadersFile, StringComparison.Ordinal))
        {
            Add(findings, ForwardedHeadersOutsideSetup, ForwardedHeaders(), clean);
        }

        // Making a record: new AttemptRecord only in its class; Create only in the counter.
        if (!string.Equals(path, AttemptRecordFile, StringComparison.Ordinal))
        {
            Add(findings, AttemptRecordWriteOutsideCounter, NewRecord(), clean);
        }

        if (!string.Equals(path, AttemptCounterFile, StringComparison.Ordinal))
        {
            Add(findings, AttemptRecordWriteOutsideCounter, CreateRecord(), clean);
            bool declaration = string.Equals(path, DbContextFile, StringComparison.Ordinal);
            bool disposal = string.Equals(path, DisposalFile, StringComparison.Ordinal);
            if (string.Equals(path, AttemptRecordFile, StringComparison.Ordinal))
            {
                // The record's own file holds its class and its model configuration (IEntityTypeConfiguration<AttemptRecord>); nothing there writes a row.
            }
            else if (!declaration && !disposal)
            {
                Add(findings, AttemptRecordWriteOutsideCounter, RecordSet(), clean);
            }
            else if (declaration)
            {
                // The context declares the set and nothing more: the set is not used for a query or a write here.
                Add(findings, AttemptRecordWriteOutsideCounter, RecordSetUse(), clean);
            }
            else
            {
                CheckDisposal(findings, clean);
            }
        }

        if (!string.Equals(path, AttemptSubjectFile, StringComparison.Ordinal))
        {
            Add(findings, AttemptSubjectMadeOutsideItsType, NewSubject(), clean);
        }

        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    // In the disposal file the attempt record's part may delete and nothing else: inside the class of AttemptRecordDisposalPart no member other than
    // ExecuteDelete(Async) is called on the set or on a query built from it, and the set is not named in any other class of the file.
    private static void CheckDisposal(List<(string Rule, int Line)> findings, string text)
    {
        Match part = DisposalPartClass().Match(text);
        int start = part.Success ? part.Index : -1;
        int end = start < 0 ? -1 : NextTypeStart(text, start + part.Length);
        foreach (Match use in RecordSet().Matches(text))
        {
            if (start < 0 || use.Index < start || use.Index >= end)
            {
                Record(findings, AttemptRecordWriteOutsideCounter, text, use.Index);
            }
        }

        if (start >= 0)
        {
            string body = text[start..end];
            foreach (Match change in Mutation().Matches(body))
            {
                Record(findings, AttemptRecordMutatedByDisposal, text, start + change.Index);
            }
        }
    }

    private static int NextTypeStart(string text, int from)
    {
        Match next = TypeStart().Match(text, from);
        return next.Success ? next.Index : text.Length;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            Record(findings, rule, text, match.Index);
        }
    }

    private static void Record(List<(string Rule, int Line)> findings, string rule, string text, int index)
    {
        int line = text.AsSpan(0, index).Count('\n') + 1;
        if (!findings.Contains((rule, line)))
        {
            findings.Add((rule, line));
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

    // RemoteIpAddress in any form: .Connection.RemoteIpAddress, ?.RemoteIpAddress, a pattern, a line break before the dot, a string "RemoteIpAddress".
    [GeneratedRegex(@"\bRemoteIpAddress\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RemoteAddress();

    // The middleware, its options, the flags, the header names and the extension (not the application's own UseForwardedHeadersRules wrapper).
    [GeneratedRegex(@"\bForwardedHeaders(?:Options|Middleware|Defaults)?\b|\bUseForwardedHeaders\b|\bXForwarded\w*|X-Forwarded|\bForwardedFor\b|\bKnownProxies\b|\bKnownIPNetworks\b|\bKnownNetworks\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ForwardedHeaders();

    // new AttemptRecord { ... }, new AttemptRecord(), with spacing or a line break after new.
    [GeneratedRegex(@"\bnew\s+(?:\w+\s*\.\s*)*AttemptRecord(?:Attribute)?\b(?!\w)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NewRecord();

    // AttemptRecord.Create(...) with any arguments, also qualified by a namespace.
    [GeneratedRegex(@"\bAttemptRecord\s*\.\s*Create\w*\s*[(<]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CreateRecord();

    // The set by name, by the generic Set<AttemptRecord>() and any other generic over the record type (IQueryable<AttemptRecord>, DbSet<AttemptRecord>, List<AttemptRecord>).
    [GeneratedRegex(@"\bAttemptRecords\b|<\s*(?:\w+\s*\.\s*)*AttemptRecord\s*>|typeof\s*\(\s*(?:\w+\s*\.\s*)*AttemptRecord\s*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RecordSet();

    // In the context file the declaration names the set once as the property type and name; any call on it is a use.
    [GeneratedRegex(@"\bAttemptRecords\s*(?:\.|\?\.|\[)|\bSet\s*<\s*AttemptRecord\s*>\s*\(\s*\)\s*\.", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RecordSetUse();

    [GeneratedRegex(@"\bclass\s+AttemptRecordDisposalPart\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DisposalPartClass();

    [GeneratedRegex(@"\b(?:internal|public|private)\s+(?:sealed\s+|static\s+|abstract\s+|partial\s+)*(?:class|record|struct|interface|enum)\s+\w+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TypeStart();

    // Anything that changes rows other than ExecuteDelete in the disposal part's class.
    [GeneratedRegex(@"\.\s*(?:ExecuteUpdate\w*|Add\w*|Remove\w*|Update\w*|Attach\w*|SaveChanges\w*|FromSql\w*|ExecuteSql\w*|Entry)\s*[(<]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Mutation();

    [GeneratedRegex(@"\bnew\s+(?:\w+\s*\.\s*)*AttemptSubject\b(?!\w)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NewSubject();
}
