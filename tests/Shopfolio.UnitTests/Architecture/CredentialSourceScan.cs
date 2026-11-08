using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the sign-in record and password rules (T341 KK8, KK9; D-64; 09 15.1 item 5, 9.4 item 1; 09 19.2 item 3). Two rules over the application source:
/// (1) a sign-in record is made and written only by <see cref="LoginRecorderFile"/> - <c>LoginRecord.Create</c> (also qualified by a namespace, split over lines or with named
/// arguments) and the <c>LoginRecords</c> set's writes are allowed there, and <c>new LoginRecord</c> only in the record's own class <see cref="LoginRecordFile"/>, so the
/// normalized address is made in one place; (2) the framework's password hasher (<c>PasswordHasher</c>, <c>IPasswordHasher</c>) is named only in <see cref="PasswordServiceFile"/>.
/// Each file is allowed for its one rule. The scan reads the whole text; full-line comments are blanked first and keep their line. Generated migrations
/// (<c>Infrastructure/Migrations/</c>) are not scanned and the test projects are not scanned.
/// </summary>
internal static partial class CredentialSourceScan
{
    public const string LoginRecordMadeOutsideRecorder = "makes or writes a sign-in record outside LoginRecorder (02 3.13.19; D-64: one writer, the address normalized in the record)";
    public const string PasswordHasherOutsideService = "names the framework's password hasher outside PasswordService (09 15.1 item 5: one wrapper)";

    public const string LoginRecordFile = "src/Shopfolio.Web/Infrastructure/Security/LoginRecord.cs";
    public const string LoginRecorderFile = "src/Shopfolio.Web/Infrastructure/Security/LoginRecorder.cs";
    public const string PasswordServiceFile = "src/Shopfolio.Web/Infrastructure/Security/PasswordService.cs";
    public const string DbContextFile = "src/Shopfolio.Web/Infrastructure/AppDbContext.cs";

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

        // new LoginRecord only in the record's class; Create and the set's writes only in the recorder.
        if (!string.Equals(path, LoginRecordFile, StringComparison.Ordinal))
        {
            Add(findings, LoginRecordMadeOutsideRecorder, NewRecord(), clean);
        }

        if (!string.Equals(path, LoginRecorderFile, StringComparison.Ordinal))
        {
            Add(findings, LoginRecordMadeOutsideRecorder, CreateRecord(), clean);
            Add(findings, LoginRecordMadeOutsideRecorder, RecordSetWrite(), clean);
            if (!string.Equals(path, DbContextFile, StringComparison.Ordinal) && !string.Equals(path, LoginRecordFile, StringComparison.Ordinal))
            {
                Add(findings, LoginRecordMadeOutsideRecorder, RecordSetGeneric(), clean);
            }
        }

        if (!string.Equals(path, PasswordServiceFile, StringComparison.Ordinal))
        {
            Add(findings, PasswordHasherOutsideService, Hasher(), clean);
        }

        findings.Sort((a, b) => a.Line != b.Line ? a.Line.CompareTo(b.Line) : string.CompareOrdinal(a.Rule, b.Rule));
        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string clean)
    {
        foreach (Match match in pattern.Matches(clean))
        {
            int line = 1 + clean.AsSpan(0, match.Index).Count('\n');
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

    // new LoginRecord, also qualified and split after "new"; not new LoginRecordConfiguration / LoginRecordsGuard.
    [GeneratedRegex(@"\bnew\s+(?:[A-Za-z_][A-Za-z0-9_]*\s*\.\s*)*LoginRecord\b(?!\w)")]
    private static partial Regex NewRecord();

    // LoginRecord.Create( with any qualification or spacing.
    [GeneratedRegex(@"\bLoginRecord\s*\.\s*Create\b")]
    private static partial Regex CreateRecord();

    // LoginRecords.Add / AddRange / AddAsync / AddRangeAsync, with or without a receiver, also split over lines.
    [GeneratedRegex(@"\bLoginRecords\s*\.\s*Add(?:Range)?(?:Async)?\b")]
    private static partial Regex RecordSetWrite();

    // Set<LoginRecord>() or any other generic use of the record type outside the context and the record's file.
    [GeneratedRegex(@"<\s*(?:[A-Za-z_][A-Za-z0-9_]*\s*\.\s*)*LoginRecord\s*>")]
    private static partial Regex RecordSetGeneric();

    // PasswordHasher<...> / IPasswordHasher<...> / PasswordHasherOptions, qualified or not.
    [GeneratedRegex(@"\bI?PasswordHasher(?:Options)?\b")]
    private static partial Regex Hasher();
}
