using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the start-up data boundary (T119 criteria 1 and 8; 06 8.1.1, 8.4.2): the start-up rows - the provinces, the countries, the
/// six single rows and the two drafts - are written by the start-up data migration, so the application code (every file outside the
/// <c>Migrations</c> folder) has no "create it when it is missing" path: no factory call, no constructor outside the entity's own file, no
/// <c>Add</c> on their sets and no hand-written <c>INSERT</c> into their tables. The delivery provinces are not in the list: the company adds
/// them from the panel. In the migration files the framework's key ring rows and the versions of the two generated texts are never written.
/// The scan is line based except for the generated text check, which reads one statement up to its semicolon; comment lines are dropped.
/// </summary>
internal static partial class StartupDataSourceScan
{
    public const string FactoryCall = "creates a start-up row in code (the start-up data migration writes it)";

    public const string Constructor = "constructs a start-up entity outside its own file";

    public const string SetAdd = "adds to the set of a start-up table (the start-up data migration writes its rows)";

    public const string SqlInsert = "inserts into a start-up table by hand outside the migrations";

    public const string KeyRingRow = "writes a data protection key in a migration (the framework makes the key ring)";

    public const string GeneratedTextVersion = "writes a version of a generated legal text in a migration (the first start writes it)";

    /// <summary>The scan's rules for application code apply outside an infrastructure <c>Migrations</c> folder.</summary>
    public static bool IsMigration(string relativePath)
    {
        return relativePath.Replace('\\', '/').Contains("/Infrastructure/Migrations/", StringComparison.Ordinal);
    }

    /// <summary>Scans one application source file; <paramref name="fileName"/> exempts an entity's own file from the constructor rule.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanCode(string fileName, string text)
    {
        List<(string Rule, int Line)> findings = [];
        string ownType = Path.GetFileNameWithoutExtension(fileName);
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (IsComment(line))
            {
                continue;
            }

            Add(findings, FactoryCall, StartupFactoryCall().IsMatch(line), i);
            Add(findings, Constructor, StartupConstructor().Matches(line).Any(m => m.Groups[1].Value != ownType), i);
            Add(findings, SetAdd, StartupSetAdd().IsMatch(line), i);
            Add(findings, SqlInsert, StartupTableInsert().IsMatch(line), i);
        }

        return findings;
    }

    /// <summary>Scans one migration file.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanMigration(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string normalized = string.Join("\n", text.ReplaceLineEndings("\n").Split('\n').Select(line => IsComment(line) ? string.Empty : line));
        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Add(findings, KeyRingRow, KeyRingInsert().IsMatch(lines[i]), i);
        }

        foreach (Match match in GeneratedTextInsert().Matches(normalized))
        {
            findings.Add((GeneratedTextVersion, normalized.AsSpan(0, match.Index).Count('\n') + 1));
        }

        return findings;
    }

    private static bool IsComment(string line)
    {
        string trimmed = line.TrimStart();
        return trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("--", StringComparison.Ordinal);
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, bool matched, int index)
    {
        if (matched)
        {
            findings.Add((rule, index + 1));
        }
    }

    [GeneratedRegex(@"\b(Company|CompanySettings|Branding|ChannelStateRecord|HeartbeatRecord|BackupStatusRecord|Province|Country)\s*\.\s*Create\s*\(|\bLegalTextVersion\s*\.\s*CreateDraft\s*\(")]
    private static partial Regex StartupFactoryCall();

    [GeneratedRegex(@"\bnew\s+(Company|CompanySettings|Branding|ChannelStateRecord|HeartbeatRecord|BackupStatusRecord|Province|Country)\s*[({]")]
    private static partial Regex StartupConstructor();

    [GeneratedRegex(@"\.\s*(Companies|CompanySettings|Brandings|ChannelStateRecords|HeartbeatRecords|BackupStatusRecords|Provinces|Countries)\s*\.\s*Add(Range)?(Async)?\s*\(")]
    private static partial Regex StartupSetAdd();

    [GeneratedRegex(@"\bINSERT\s+INTO\s+(?:public\.)?""?(companies|company_settings|brandings|channel_state_records|heartbeat_records|backup_status_records|provinces|countries)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StartupTableInsert();

    [GeneratedRegex(@"\bINSERT\s+INTO\s+(?:public\.)?""?data_protection_keys\b|\bInsertData\s*\(\s*(?:table:\s*)?""data_protection_keys""", RegexOptions.IgnoreCase)]
    private static partial Regex KeyRingInsert();

    // One statement: from the INSERT up to its semicolon.
    [GeneratedRegex(@"\bINSERT\s+INTO\s+(?:public\.)?""?legal_text_versions\b[^;]*?\b(PreInformationForm|DistanceSalesContract)\b", RegexOptions.IgnoreCase)]
    private static partial Regex GeneratedTextInsert();
}
