using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the migration files (T103, T106; 09 10.6 item 11, 10.8 item 7, 9.2 items 6-8): a migration is forward only (no <c>Down</c>),
/// runs in one transaction, carries no statement that cannot run in a transaction or that changes the server (CONCURRENTLY, VACUUM, ALTER
/// SYSTEM, CREATE DATABASE, CREATE EXTENSION), reads no clock inside SQL (the one exception is the disposal function's own clamp line; time
/// comes from the application), writes no fixed user name and grants or revokes only through the role settings (<c>current_setting</c>),
/// never to PUBLIC. The one grant form that names PUBLIC is the disposal function's <c>REVOKE ... ON FUNCTION ... FROM PUBLIC</c>.
/// <para>
/// Data is written only in tables whose volume does not grow with the business (the settings row and the fixed lists - 10.6 item 4), and the
/// audit trail is never updated, deleted from, or restructured by a migration (10.6 item 6); the one statement that deletes from it is the body
/// of <c>dispose_expired_audit_logs</c>. The scan is line based except for the audit trail's structure change through the migration builder,
/// which spans lines and is read from the whole text; comment lines are dropped. It needs neither a database nor the network.
/// </para>
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

    public const string DataWriteOutsideAllowedTables = "updates or deletes rows of a table whose volume grows with the business (move the work to a background job)";

    public const string AuditLogRowWrite = "updates or deletes audit_logs rows outside the disposal function's body (the trail is insert only)";

    public const string AuditLogStructureChange = "drops or renames a column or a table of audit_logs (a migration only adds to the trail)";

    /// <summary>The tables a migration may update or delete rows in: the settings row and the fixed lists (09 10.6 item 4).</summary>
    private static readonly HashSet<string> AllowedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "companies",
        "company_settings",
        "provinces",
        "countries",
    };

    /// <summary>The scan applies to a file under an infrastructure <c>Migrations</c> folder.</summary>
    public static bool Applies(string relativePath)
    {
        return relativePath.Replace('\\', '/').Contains("/Infrastructure/Migrations/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string normalized = text.ReplaceLineEndings("\n");
        string[] lines = normalized.Split('\n');
        bool inDisposalFunction = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (DisposalFunctionStart().IsMatch(line))
            {
                inDisposalFunction = true;
            }

            bool functionRevoke = FunctionRevokeFromPublic().IsMatch(line);
            Add(findings, DownMethod, DownDeclaration().IsMatch(line), i);
            Add(findings, SuppressedTransaction, SuppressTransactionWord().IsMatch(line), i);
            Add(findings, NonTransactionalStatement, NonTransactionalSql().IsMatch(line), i);
            Add(findings, DatabaseClock, ClockFunction().IsMatch(line) && !(inDisposalFunction && DisposalClamp().IsMatch(line)), i);
            Add(findings, FixedUserName, FixedName().IsMatch(line), i);
            Add(findings, UnscopedPrivilege, PrivilegeStatement().IsMatch(line) && !line.Contains("current_setting", StringComparison.Ordinal) && !functionRevoke, i);
            Add(findings, PublicPrivilege, ToPublic().IsMatch(line) || (FromPublic().IsMatch(line) && !functionRevoke), i);
            ScanRowWrites(findings, line, i, inDisposalFunction);
            Add(findings, AuditLogStructureChange, AuditLogSqlStructureChange().IsMatch(line), i);

            if (inDisposalFunction && FunctionBodyEnd().IsMatch(line))
            {
                inDisposalFunction = false;
            }
        }

        foreach (Match match in AuditLogBuilderStructureChange().Matches(normalized))
        {
            findings.Add((AuditLogStructureChange, normalized.AsSpan(0, match.Index).Count('\n') + 1));
        }

        return findings;
    }

    private static void ScanRowWrites(List<(string Rule, int Line)> findings, string line, int index, bool inDisposalFunction)
    {
        foreach (Match match in RowUpdate().Matches(line))
        {
            AddRowWrite(findings, match.Groups[1].Value, index, allowedInDisposalBody: false, inDisposalFunction);
        }

        foreach (Match match in RowDelete().Matches(line))
        {
            AddRowWrite(findings, match.Groups[1].Value, index, allowedInDisposalBody: true, inDisposalFunction);
        }
    }

    private static void AddRowWrite(List<(string Rule, int Line)> findings, string table, int index, bool allowedInDisposalBody, bool inDisposalFunction)
    {
        if (table.Equals("audit_logs", StringComparison.OrdinalIgnoreCase))
        {
            Add(findings, AuditLogRowWrite, !(allowedInDisposalBody && inDisposalFunction), index);
            return;
        }

        Add(findings, DataWriteOutsideAllowedTables, !AllowedTables.Contains(table), index);
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

    [GeneratedRegex(@"\b(now|clock_timestamp|statement_timestamp|transaction_timestamp)\s*\(|\b(current_timestamp|localtimestamp)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ClockFunction();

    /// <summary>The one clock read a migration may carry: the disposal function's upper bound on the given moment (09 9.2 item 6).</summary>
    [GeneratedRegex(@"^\s*as_of\s*:=\s*least\(as_of,\s*clock_timestamp\(\)\);\s*$")]
    private static partial Regex DisposalClamp();

    [GeneratedRegex(@"\bshopfolio_\w+")]
    private static partial Regex FixedName();

    [GeneratedRegex(@"\b(GRANT|REVOKE)\b")]
    private static partial Regex PrivilegeStatement();

    // The negative look-ahead keeps "FROM public.table" (the schema) from being read as the PUBLIC role.
    [GeneratedRegex(@"\bTO\s+PUBLIC\b(?![\w.])", RegexOptions.IgnoreCase)]
    private static partial Regex ToPublic();

    [GeneratedRegex(@"\bFROM\s+PUBLIC\b(?![\w.])", RegexOptions.IgnoreCase)]
    private static partial Regex FromPublic();

    /// <summary>The one statement that names PUBLIC: taking the call privilege of a function back from everyone (09 10.8 item 7; 9.2 item 7).</summary>
    [GeneratedRegex(@"\bREVOKE\s+(ALL|EXECUTE)(\s+PRIVILEGES)?\s+ON\s+FUNCTION\s+[\w.]+\s*\([^)]*\)\s+FROM\s+PUBLIC\b", RegexOptions.IgnoreCase)]
    private static partial Regex FunctionRevokeFromPublic();

    [GeneratedRegex(@"\bCREATE\s+(OR\s+REPLACE\s+)?FUNCTION\s+(public\.)?dispose_expired_audit_logs\b", RegexOptions.IgnoreCase)]
    private static partial Regex DisposalFunctionStart();

    [GeneratedRegex(@"^\s*END\s*\$\$")]
    private static partial Regex FunctionBodyEnd();

    // "ON UPDATE", "DO UPDATE SET" and "FOR UPDATE" are not row updates of a table.
    [GeneratedRegex(@"(?<!\bON\s)(?<!\bDO\s)(?<!\bFOR\s)\bUPDATE\s+(?:ONLY\s+)?(?:public\.)?""?(\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex RowUpdate();

    [GeneratedRegex(@"\bDELETE\s+FROM\s+(?:ONLY\s+)?(?:public\.)?""?(\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex RowDelete();

    [GeneratedRegex(@"\bALTER\s+TABLE\s+(?:ONLY\s+)?(?:IF\s+EXISTS\s+)?(?:public\.)?""?audit_logs\b.*\b(DROP\s+COLUMN|RENAME)\b|\bDROP\s+TABLE\b.*\baudit_logs\b", RegexOptions.IgnoreCase)]
    private static partial Regex AuditLogSqlStructureChange();

    // The builder's calls span lines; a call ends at its semicolon.
    [GeneratedRegex(@"\b(DropColumn|RenameColumn|RenameTable|RenameIndex|AlterColumn|DropTable)\s*\([^;]*?""audit_logs""")]
    private static partial Regex AuditLogBuilderStructureChange();
}
