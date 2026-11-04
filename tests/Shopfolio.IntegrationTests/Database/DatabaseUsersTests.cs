using Npgsql;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// The collection of the database user and privilege tests (09 10.8 item 7; T10). It owns one database copied from the template, which the
/// setup script's SQL files and the schema owner's migrations made.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseUsersDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "DatabaseUsers";
}

/// <summary>
/// The users and privileges of 09 10.8, checked against a real PostgreSQL 18: what the setup script's SQL files made (the files an
/// installation runs, 09 19.1 item 4), as the application, read-only, backup status and schema owner users really connect.
/// </summary>
[Collection(DatabaseUsersDefinition.Name)]
public sealed class DatabaseUsersTests
{
    private const string Probe = "public.user_probe";

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public DatabaseUsersTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private TestDatabaseUsers Users => _template.Users;

    [Fact]
    public async Task FourUsers_AfterSetup_CarryNoneOfTheFiveDangerousAttributes()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        string[] names = [Users.Migrator, Users.App, Users.Reader, Users.BackupStatus];

        List<string> dangerous = await QueryStringsAsync(
            _collection.Dsn,
            "SELECT rolname FROM pg_roles WHERE rolname = ANY(@names) AND (rolsuper OR rolcreatedb OR rolcreaterole OR rolreplication OR rolbypassrls OR NOT rolcanlogin)",
            ct,
            ("names", names));
        List<string> present = await QueryStringsAsync(_collection.Dsn, "SELECT rolname FROM pg_roles WHERE rolname = ANY(@names)", ct, ("names", names));

        Assert.Empty(dangerous);
        Assert.Equal(names.Order(StringComparer.Ordinal), present.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task SchemaOwner_IsOwnerOfTheDatabaseAndThePublicSchema()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        List<string> databaseOwner = await QueryStringsAsync(_collection.Dsn, "SELECT pg_get_userbyid(datdba) FROM pg_database WHERE datname = current_database()", ct);
        List<string> schemaOwner = await QueryStringsAsync(_collection.Dsn, "SELECT pg_get_userbyid(nspowner) FROM pg_namespace WHERE nspname = 'public'", ct);

        Assert.Equal(Users.Migrator, Assert.Single(databaseOwner));
        Assert.Equal(Users.Migrator, Assert.Single(schemaOwner));
    }

    [Fact]
    public async Task Database_AfterSetup_IsUtf8WithIcuTurkishCollationAndHasTrigramCreatedByTheFounder()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        List<string> encoding = await QueryStringsAsync(_collection.Dsn, "SELECT pg_encoding_to_char(encoding) || '|' || datlocprovider::text || '|' || datlocale FROM pg_database WHERE datname = current_database()", ct);
        // The Turkish alphabet puts the dotless i before the dotted i; ICU's root order puts it after (05 2.8.3 of 06). The sort shows the setting at work.
        List<string> sorted = await QueryStringsAsync(_collection.Dsn, "SELECT string_agg(x, ',' ORDER BY x) FROM unnest(ARRAY['i', 'ı', 'h']) AS x", ct);
        List<string> trigram = await QueryStringsAsync(_collection.Dsn, "SELECT pg_get_userbyid(extowner) FROM pg_extension WHERE extname = 'pg_trgm'", ct);

        Assert.Equal("UTF8|i|tr-TR", Assert.Single(encoding));
        Assert.Equal("h,ı,i", Assert.Single(sorted));
        // The extension is the founding user's: the schema owner got no new privilege for it (05 7.1.3).
        Assert.NotEqual(Users.Migrator, Assert.Single(trigram));
    }

    [Fact]
    public async Task SchemaOwner_AfterSetup_IsNotASuperUserAndCannotCreateRolesOrDatabases()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        string migrator = _template.DsnFor(_collection.DatabaseName, Users.Migrator, Users.MigratorPassword, pooling: false);

        List<string> flags = await QueryStringsAsync(migrator, "SELECT rolsuper::text || rolcreaterole::text || rolcreatedb::text FROM pg_roles WHERE rolname = current_user", ct);
        PostgresException createRole = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(migrator, $"CREATE ROLE {Users.Migrator}_other", ct));
        PostgresException createDatabase = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(migrator, $"CREATE DATABASE {_collection.DatabaseName}_other", ct));
        // The schema owner uses the trigram operator class in an index without any privilege of its own for the extension.
        await ExecuteAsync(migrator, $"CREATE TABLE {Probe} (id integer PRIMARY KEY, name text); CREATE INDEX ix_user_probe_name ON {Probe} USING gin (name gin_trgm_ops)", ct);

        Assert.Equal("falsefalsefalse", Assert.Single(flags));
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, createRole.SqlState);
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, createDatabase.SqlState);
        await DropProbeAsync(ct);
    }

    [Fact]
    public async Task ApplicationUser_OnTableOfTheSchemaOwner_UsesDataButCannotChangeTheSchema()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await CreateProbeAsync(ct);
        string app = AppDsn();

        PostgresException create = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(app, "CREATE TABLE public.app_creates (id integer)", ct));
        PostgresException drop = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(app, $"DROP TABLE {Probe}", ct));
        PostgresException alter = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(app, $"ALTER TABLE {Probe} ADD COLUMN extra integer", ct));
        PostgresException truncate = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(app, $"TRUNCATE {Probe}", ct));
        await ExecuteAsync(app, $"INSERT INTO {Probe} (id, name) VALUES (1, 'a')", ct);
        await ExecuteAsync(app, $"UPDATE {Probe} SET name = 'b' WHERE id = 1", ct);
        List<string> read = await QueryStringsAsync(app, $"SELECT name FROM {Probe}", ct);
        await ExecuteAsync(app, $"DELETE FROM {Probe} WHERE id = 1", ct);
        // The status command lists the pending log files with this function (05 9.4.10).
        List<string> listing = await QueryStringsAsync(app, "SELECT count(*)::text FROM pg_ls_archive_statusdir()", ct);

        Assert.All([create, drop, alter, truncate], e => Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, e.SqlState));
        Assert.Equal("b", Assert.Single(read));
        Assert.Single(listing);
        await DropProbeAsync(ct);
    }

    [Fact]
    public async Task ReaderUser_OnTableOfTheSchemaOwner_OnlyReadsAndItsSessionIsReadOnly()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await CreateProbeAsync(ct);
        await ExecuteAsync(AppDsn(), $"INSERT INTO {Probe} (id, name) VALUES (1, 'a')", ct);
        string reader = _template.DsnFor(_collection.DatabaseName, Users.Reader, Users.ReaderPassword, pooling: false);

        List<string> read = await QueryStringsAsync(reader, $"SELECT name FROM {Probe}", ct);
        List<string> sessionSetting = await QueryStringsAsync(reader, "SHOW default_transaction_read_only", ct);
        PostgresException insert = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(reader, $"INSERT INTO {Probe} (id, name) VALUES (2, 'x')", ct));
        // The session setting is no security boundary (09 10.8 item 2): with it switched off, the missing privilege is what refuses the write.
        PostgresException insertWithoutSetting = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteSequenceAsync(reader, ct, "SET default_transaction_read_only = off", $"INSERT INTO {Probe} (id, name) VALUES (2, 'x')"));
        PostgresException update = await Assert.ThrowsAsync<PostgresException>(() => ExecuteSequenceAsync(reader, ct, "SET default_transaction_read_only = off", $"UPDATE {Probe} SET name = 'x'"));
        PostgresException delete = await Assert.ThrowsAsync<PostgresException>(() => ExecuteSequenceAsync(reader, ct, "SET default_transaction_read_only = off", $"DELETE FROM {Probe}"));

        Assert.Equal("a", Assert.Single(read));
        Assert.Equal("on", Assert.Single(sessionSetting));
        Assert.Contains(insert.SqlState, new[] { PostgresErrorCodes.ReadOnlySqlTransaction, PostgresErrorCodes.InsufficientPrivilege });
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, insertWithoutSetting.SqlState);
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, update.SqlState);
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, delete.SqlState);
        await DropProbeAsync(ct);
    }

    [Fact]
    public async Task BackupStatusUser_OnTableOfTheSchemaOwner_GetsNoPrivilegeByDefault()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await CreateProbeAsync(ct);
        // The user has no password and connects only through the local socket (pg_hba.conf), so the test acts as it with SET ROLE.
        string asStatus = _collection.Dsn;

        string setRole = $"SET ROLE {Users.BackupStatus}";
        PostgresException select = await Assert.ThrowsAsync<PostgresException>(() => ExecuteSequenceAsync(asStatus, ct, setRole, $"SELECT * FROM {Probe}"));
        PostgresException update = await Assert.ThrowsAsync<PostgresException>(() => ExecuteSequenceAsync(asStatus, ct, setRole, $"UPDATE {Probe} SET name = 'x'"));
        // Control: the same way of acting as the user succeeds once a column privilege exists, so the refusals above come from the missing privilege.
        await ExecuteAsync(SchemaOwnerDsn(), $"GRANT UPDATE (name) ON {Probe} TO {Users.BackupStatus}", ct);
        await ExecuteSequenceAsync(asStatus, ct, setRole, $"UPDATE {Probe} SET name = 'x'");
        PostgresException insert = await Assert.ThrowsAsync<PostgresException>(() => ExecuteSequenceAsync(asStatus, ct, setRole, $"INSERT INTO {Probe} (id) VALUES (3)"));
        PostgresException delete = await Assert.ThrowsAsync<PostgresException>(() => ExecuteSequenceAsync(asStatus, ct, setRole, $"DELETE FROM {Probe}"));

        Assert.All([select, update, insert, delete], e => Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, e.SqlState));
        await DropProbeAsync(ct);
    }

    [Fact]
    public async Task BackupStatusUser_HasNoPassword()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        List<string> passwords = await QueryStringsAsync(_collection.Dsn, "SELECT (rolpassword IS NULL)::text FROM pg_authid WHERE rolname = @name", ct, ("name", Users.BackupStatus));

        Assert.Equal("true", Assert.Single(passwords));
    }

    [Fact]
    public async Task SchemaOwnerRoleSettings_InTheTemplateAndInACopy_AreTheThreeUserNames()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        string migrator = _template.DsnFor(_collection.DatabaseName, Users.Migrator, Users.MigratorPassword, pooling: false);

        List<string> inCopy = await QueryStringsAsync(
            migrator,
            "SELECT current_setting('shopfolio.application_user') || '|' || current_setting('shopfolio.reader_user') || '|' || current_setting('shopfolio.backup_status_user')",
            ct);
        List<string> inTemplate = await QueryStringsAsync(
            _template.DsnFor(TemplateDatabaseFixture.TemplateName, Users.Migrator, Users.MigratorPassword, pooling: false),
            "SELECT current_setting('shopfolio.application_user') || '|' || current_setting('shopfolio.reader_user') || '|' || current_setting('shopfolio.backup_status_user')",
            ct);

        string expected = $"{Users.App}|{Users.Reader}|{Users.BackupStatus}";
        Assert.Equal(expected, Assert.Single(inCopy));
        Assert.Equal(expected, Assert.Single(inTemplate));
    }

    [Fact]
    public async Task Public_HasNoPrivilegeOnTheDatabaseOrTheSchema()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        string other = $"{Users.Migrator}_nobody";
        await ExecuteAsync(_template.DsnFor(_collection.DatabaseName), $"DROP ROLE IF EXISTS {other}; CREATE ROLE {other} LOGIN", ct);
        try
        {
            List<string> publicGrants = await QueryStringsAsync(
                _collection.Dsn,
                "SELECT 'database' FROM pg_database, aclexplode(datacl) a WHERE datname = current_database() AND a.grantee = 0 UNION ALL SELECT 'schema' FROM pg_namespace, aclexplode(nspacl) a WHERE nspname = 'public' AND a.grantee = 0",
                ct);
            List<string> nobody = await QueryStringsAsync(
                _collection.Dsn,
                "SELECT has_database_privilege(@name, current_database(), 'CONNECT')::text || has_database_privilege(@name, current_database(), 'TEMPORARY')::text || has_schema_privilege(@name, 'public', 'USAGE')::text || has_schema_privilege(@name, 'public', 'CREATE')::text",
                ct,
                ("name", other));

            Assert.Empty(publicGrants);
            Assert.Equal("falsefalsefalsefalse", Assert.Single(nobody));
        }
        finally
        {
            await ExecuteAsync(_template.DsnFor(_collection.DatabaseName), $"DROP ROLE IF EXISTS {other}", CancellationToken.None);
        }
    }

    [Fact]
    public async Task PrivilegeMatrix_OnTheTablesOfTheDatabase_HasNoDeviation()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await CreateProbeAsync(ct);
        // The three tables of 09 10.8 item 3 are the real ones, each with the privilege split of the migration that made it: the audit trail
        // (T106), the key ring and the backup status record (T118). The probe is an ordinary table with the default privileges.
        List<string> exceptions = await QueryStringsAsync(
            _collection.Dsn,
            "SELECT relname FROM pg_class WHERE relnamespace = 'public'::regnamespace AND relname = ANY(@names) ORDER BY 1",
            ct,
            ("names", new[] { PrivilegeMatrix.AuditLogs, PrivilegeMatrix.DataProtectionKeys, PrivilegeMatrix.BackupStatusRecords }));

        IReadOnlyList<string> deviations = await PrivilegeMatrix.FindDeviationsAsync(_collection.Dsn, Users, ct);

        Assert.Equal([PrivilegeMatrix.AuditLogs, PrivilegeMatrix.BackupStatusRecords, PrivilegeMatrix.DataProtectionKeys], exceptions);
        Assert.Empty(deviations);
        await DropProbeAsync(ct);
    }

    [Theory]
    [InlineData("GRANT TRUNCATE ON public.user_probe TO {app}", "user_probe: {app} has TRUNCATE")]
    [InlineData("GRANT SELECT ON public.user_probe TO {status}", "user_probe: {status} has SELECT")]
    [InlineData("GRANT UPDATE (name) ON public.user_probe TO {status}", "user_probe: {status} has UPDATE")]
    [InlineData("REVOKE DELETE ON public.user_probe FROM {app}", "user_probe: {app} lacks DELETE")]
    [InlineData("GRANT INSERT ON public.user_probe TO {reader}", "user_probe: {reader} has INSERT")]
    [InlineData("ALTER TABLE public.user_probe OWNER TO {app}", "user_probe: owned by")]
    public async Task PrivilegeMatrix_OnATableThatDiffersFromTheDefaultWithoutBeingAnException_ReportsIt(string statement, string expectedLine)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        await CreateProbeAsync(ct);
        string Fill(string text) => text.Replace("{app}", Users.App, StringComparison.Ordinal).Replace("{reader}", Users.Reader, StringComparison.Ordinal).Replace("{status}", Users.BackupStatus, StringComparison.Ordinal);
        // The owner change needs a super user; the grants are the schema owner's own.
        await ExecuteAsync(statement.StartsWith("ALTER", StringComparison.Ordinal) ? _collection.Dsn : SchemaOwnerDsn(), Fill(statement), ct);

        IReadOnlyList<string> deviations = await PrivilegeMatrix.FindDeviationsAsync(_collection.Dsn, Users, ct);

        Assert.Contains(deviations, line => line.StartsWith(Fill(expectedLine), StringComparison.Ordinal));
        // Back to a table the schema owner owns, so the drop below is the schema owner's.
        await ExecuteAsync(_collection.Dsn, $"ALTER TABLE {Probe} OWNER TO {Users.Migrator}", ct);
        await DropProbeAsync(ct);
    }

    private string SchemaOwnerDsn()
    {
        return _template.DsnFor(_collection.DatabaseName, Users.Migrator, Users.MigratorPassword, pooling: false);
    }

    private string AppDsn()
    {
        return _template.DsnFor(_collection.DatabaseName, Users.App, Users.AppPassword, pooling: false);
    }

    private Task CreateProbeAsync(CancellationToken ct)
    {
        return ExecuteAsync(SchemaOwnerDsn(), $"DROP TABLE IF EXISTS {Probe}; CREATE TABLE {Probe} (id integer PRIMARY KEY, name text)", ct);
    }

    private Task DropProbeAsync(CancellationToken ct)
    {
        return ExecuteAsync(SchemaOwnerDsn(), $"DROP TABLE IF EXISTS {Probe}", ct);
    }

    private static async Task ExecuteAsync(string connectionString, string sql, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Runs the statements one after the other on one connection, each in its own command (so a setting changed by one applies to the next).</summary>
    private static async Task ExecuteSequenceAsync(string connectionString, CancellationToken ct, params string[] statements)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(ct);
        foreach (string statement in statements)
        {
            await using NpgsqlCommand command = new(statement, connection);
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task<List<string>> QueryStringsAsync(string connectionString, string sql, CancellationToken ct, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(Convert.ToString(reader.GetValue(0), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
        }

        return rows;
    }
}
