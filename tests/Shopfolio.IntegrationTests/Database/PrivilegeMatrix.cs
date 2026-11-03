using Npgsql;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// The privilege matrix of 09 10.8 item 7: for every table of the <c>public</c> schema, the privileges of the application, read-only and
/// backup status users are compared with what the table is expected to give them, and every difference is reported. The expectation is the
/// default of item 2 plus the three exceptions of item 3; a new table that differs from the default and is not in the exception list
/// makes <see cref="FindDeviationsAsync"/> report it, so the test that calls it goes red. The exception list is the one place that changes
/// when a table gets an exception (a table renamed in two releases is listed under both names, item 4).
/// </summary>
internal static class PrivilegeMatrix
{
    public const string AuditLogs = "audit_logs";

    public const string DataProtectionKeys = "data_protection_keys";

    public const string BackupStatusRecords = "backup_status_records";

    /// <summary>The columns of the one table the backup status user may update (09 10.8 item 3).</summary>
    public static readonly IReadOnlyList<string> BackupStatusWritableColumns = ["last_succeeded_at", "last_failed_at"];

    private static readonly string[] _columnCapablePrivileges = ["SELECT", "INSERT", "UPDATE", "REFERENCES"];

    private static readonly string[] _tableOnlyPrivileges = ["DELETE", "TRUNCATE", "TRIGGER"];

    /// <summary>
    /// What a table gives one user, privilege by privilege: <c>true</c> = the user has it on the whole table, <c>false</c> = the user has
    /// it neither on the table nor on any column. The backup status user's column privilege is checked separately.
    /// </summary>
    public static IReadOnlyDictionary<string, bool> Expected(string table, UserKind user)
    {
        bool app = user == UserKind.App;
        bool reader = user == UserKind.Reader;
        Dictionary<string, bool> expected = new()
        {
            ["SELECT"] = app || reader,
            ["INSERT"] = app,
            ["UPDATE"] = app,
            ["DELETE"] = app,
            ["TRUNCATE"] = false,
            ["REFERENCES"] = false,
            ["TRIGGER"] = false,
        };

        if (table == AuditLogs && app)
        {
            expected["UPDATE"] = false;
            expected["DELETE"] = false;
        }

        if (table == DataProtectionKeys && reader)
        {
            expected["SELECT"] = false;
        }

        // The migration history table (__EFMigrationsHistory) is closed to the application and read-only users by the first
        // migration (T103; 09 10.8 item 3); until that migration exists it has the default, and T103 adds the expectation here.
        return expected;
    }

    /// <summary>
    /// Compares every table of the <c>public</c> schema of the database with the expectation; returns one line per difference.
    /// </summary>
    public static async Task<IReadOnlyList<string>> FindDeviationsAsync(string connectionString, TestDatabaseUsers users, CancellationToken ct)
    {
        List<string> deviations = [];
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(ct);

        foreach (string table in await ReadAsync(connection, "SELECT c.relname FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace WHERE n.nspname = 'public' AND c.relkind IN ('r', 'p') ORDER BY 1", ct))
        {
            string owner = (await ReadAsync(connection, $"SELECT pg_get_userbyid(relowner) FROM pg_class WHERE oid = 'public.\"{table}\"'::regclass", ct))[0];
            if (owner != users.Migrator)
            {
                deviations.Add($"{table}: owned by '{owner}', expected the schema owner");
            }

            foreach ((UserKind kind, string user) in new[] { (UserKind.App, users.App), (UserKind.Reader, users.Reader), (UserKind.BackupStatus, users.BackupStatus) })
            {
                await CompareUserAsync(connection, table, kind, user, deviations, ct);
            }
        }

        return deviations;
    }

    private static async Task CompareUserAsync(NpgsqlConnection connection, string table, UserKind kind, string user, List<string> deviations, CancellationToken ct)
    {
        if (kind == UserKind.BackupStatus)
        {
            await CompareBackupStatusAsync(connection, table, user, deviations, ct);
            return;
        }

        foreach ((string privilege, bool allowed) in Expected(table, kind))
        {
            // Allowed: the user has the privilege on the whole table. Not allowed: it has it on no column either.
            bool granted = allowed || _tableOnlyPrivileges.Contains(privilege, StringComparer.Ordinal)
                ? await HasAsync(connection, "has_table_privilege", user, table, privilege, ct)
                : await HasAsync(connection, "has_any_column_privilege", user, table, privilege, ct);
            if (granted != allowed)
            {
                deviations.Add($"{table}: {user} {(granted ? "has" : "lacks")} {privilege}, expected {(allowed ? "to have" : "not to have")} it");
            }
        }
    }

    private static async Task CompareBackupStatusAsync(NpgsqlConnection connection, string table, string user, List<string> deviations, CancellationToken ct)
    {
        foreach (string privilege in _tableOnlyPrivileges.Concat(_columnCapablePrivileges))
        {
            if (table == BackupStatusRecords && privilege == "UPDATE")
            {
                continue;
            }

            string function = _tableOnlyPrivileges.Contains(privilege, StringComparer.Ordinal) ? "has_table_privilege" : "has_any_column_privilege";
            if (await HasAsync(connection, function, user, table, privilege, ct))
            {
                deviations.Add($"{table}: {user} has {privilege}, expected not to have it");
            }
        }

        if (table != BackupStatusRecords)
        {
            return;
        }

        foreach (string column in await ReadAsync(connection, $"SELECT attname FROM pg_attribute WHERE attrelid = 'public.\"{table}\"'::regclass AND attnum > 0 AND NOT attisdropped ORDER BY attnum", ct))
        {
            bool expected = BackupStatusWritableColumns.Contains(column, StringComparer.Ordinal);
            await using NpgsqlCommand command = new("SELECT has_column_privilege(@user, @table, @column, 'UPDATE')", connection);
            command.Parameters.AddWithValue("user", user);
            command.Parameters.AddWithValue("table", $"public.\"{table}\"");
            command.Parameters.AddWithValue("column", column);
            bool granted = (bool)(await command.ExecuteScalarAsync(ct))!; // '!': the query always returns one boolean row.
            if (granted != expected)
            {
                deviations.Add($"{table}.{column}: {user} {(granted ? "has" : "lacks")} UPDATE, expected {(expected ? "to have" : "not to have")} it");
            }
        }
    }

    private static async Task<bool> HasAsync(NpgsqlConnection connection, string function, string user, string table, string privilege, CancellationToken ct)
    {
        // The function name is one of two constants of this class, never a value from outside.
        await using NpgsqlCommand command = new($"SELECT {function}(@user, @table, @privilege)", connection);
        command.Parameters.AddWithValue("user", user);
        command.Parameters.AddWithValue("table", $"public.\"{table}\"");
        command.Parameters.AddWithValue("privilege", privilege);
        return (bool)(await command.ExecuteScalarAsync(ct))!; // '!': the query always returns one boolean row.
    }

    private static async Task<List<string>> ReadAsync(NpgsqlConnection connection, string sql, CancellationToken ct)
    {
        List<string> values = [];
        await using NpgsqlCommand command = new(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }
}

/// <summary>The three users whose privileges the matrix checks (the schema owner owns everything and the founding user is a super user).</summary>
internal enum UserKind
{
    App,
    Reader,
    BackupStatus,
}
