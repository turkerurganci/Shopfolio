using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the migration files (T103; 09 10.6 item 11, 10.8 item 7): a migration is forward only (no <c>Down</c>), runs in one
/// transaction, carries no statement that cannot run in a transaction or that changes the server (CONCURRENTLY, VACUUM, ALTER SYSTEM,
/// CREATE DATABASE, CREATE EXTENSION), reads no clock inside SQL (<c>now()</c>; time comes from the application), writes no fixed user name
/// and grants or revokes only through the role settings (<c>current_setting</c>), never to PUBLIC. The scan is line based; comment lines
/// are dropped. It needs neither a database nor the network.
/// </summary>
internal static partial class MigrationSourceScan
{
    public const string DownMethod = "defines a Down method (a migration is forward only)";

    public const string SuppressedTransaction = "suppresses the transaction of the migration";

    public const string NonTransactionalStatement = "uses a statement that cannot run in the migration's transaction or changes the server";

    public const string DatabaseClock = "reads the clock in SQL (now(); time comes from the application)";

    public const string FixedUserName = "writes a fixed database user name (read it from the role settings)";

    public const string UnscopedPrivilege = "grants or revokes without reading the user name from current_setting";

    public const string PublicPrivilege = "grants or revokes for PUBLIC";

    /// <summary>The scan applies to a file under an infrastructure <c>Migrations</c> folder.</summary>
    public static bool Applies(string relativePath)
    {
        return relativePath.Replace('\\', '/').Contains("/Infrastructure/Migrations/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            Add(findings, DownMethod, DownDeclaration().IsMatch(line), i);
            Add(findings, SuppressedTransaction, SuppressTransactionWord().IsMatch(line), i);
            Add(findings, NonTransactionalStatement, NonTransactionalSql().IsMatch(line), i);
            Add(findings, DatabaseClock, ClockFunction().IsMatch(line), i);
            Add(findings, FixedUserName, FixedName().IsMatch(line), i);
            Add(findings, UnscopedPrivilege, PrivilegeStatement().IsMatch(line) && !line.Contains("current_setting", StringComparison.Ordinal), i);
            Add(findings, PublicPrivilege, PublicTarget().IsMatch(line), i);
        }

        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, bool matched, int index)
    {
        if (matched)
        {
            findings.Add((rule, index + 1));
        }
    }

    [GeneratedRegex(@"\bvoid\s+Down\s*\(")]
    private static partial Regex DownDeclaration();

    [GeneratedRegex(@"\bsuppressTransaction\b")]
    private static partial Regex SuppressTransactionWord();

    [GeneratedRegex(@"\b(CONCURRENTLY|VACUUM|ALTER\s+SYSTEM|CREATE\s+DATABASE|CREATE\s+EXTENSION)\b", RegexOptions.IgnoreCase)]
    private static partial Regex NonTransactionalSql();

    [GeneratedRegex(@"\bnow\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex ClockFunction();

    [GeneratedRegex(@"\bshopfolio_\w+")]
    private static partial Regex FixedName();

    [GeneratedRegex(@"\b(GRANT|REVOKE)\b")]
    private static partial Regex PrivilegeStatement();

    [GeneratedRegex(@"\b(TO|FROM)\s+PUBLIC\b", RegexOptions.IgnoreCase)]
    private static partial Regex PublicTarget();
}
