using System.Globalization;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Migrations;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the administrator, invitation and audit trail tests (T106): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(AdminAndAuditTablesDefinition.Name)]
public sealed class AdminAndAuditTablesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AdminAndAuditTables";
}

/// <summary>
/// The administrator account, the invitation and the audit trail on a real PostgreSQL (T106 criteria 1-7; 06 4.37, 4.38, 4.63; 09 9.2 items 6-8;
/// 09 10.6; 09 10.8 item 3): the migration made the three tables; the database itself refuses what the rules refuse; the application user can
/// insert into and read the trail but not update, delete or truncate it; the disposal function removes exactly the rows whose retention is over
/// and is callable by the application user only; and a missing role setting rolls the migration back.
/// </summary>
[Collection(AdminAndAuditTablesDefinition.Name)]
public sealed class AdminAndAuditTablesTests : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
    private const string InsufficientPrivilege = PostgresErrorCodes.InsufficientPrivilege;

    private const string DisposeFunction = "public.dispose_expired_audit_logs";

    private static readonly string[] ThreeTables = ["admin_invitations", "admins", "audit_logs"];

    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public AdminAndAuditTablesTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TestDatabaseUsers Users => _template.Users;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, Users.App, Users.AppPassword, pooling: false);

    private string ReaderDsn => _template.DsnFor(_collection.DatabaseName, Users.Reader, Users.ReaderPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(_collection.Dsn, "TRUNCATE admin_invitations, admins, audit_logs CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the migration ----

    [Fact]
    public async Task Migration_MakesTheThreeTables()
    {
        List<string> tables = await QueryAsync(
            _collection.Dsn,
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(ThreeTables, tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Columns_HaveTheTypesAndNullabilityOfTheFieldTables()
    {
        // 06 4.37, 4.38, 4.63 and 2.5.1 (the common Id and CreatedAt of the 4 introduction included): text, uuid, timestamptz, the details as jsonb.
        string[] expected =
        [
            "admin_invitations.accepted_at timestamp with time zone",
            "admin_invitations.created_at timestamp with time zone not null",
            "admin_invitations.email text not null",
            "admin_invitations.id uuid not null",
            "admin_invitations.invalidated_at timestamp with time zone",
            "admin_invitations.normalized_email text not null",
            "admin_invitations.sender_admin_id uuid",
            "admin_invitations.sender_admin_name text not null",
            "admin_invitations.withdrawn_at timestamp with time zone",
            "admins.created_at timestamp with time zone not null",
            "admins.email text not null",
            "admins.id uuid not null",
            "admins.name text not null",
            "admins.normalized_email text not null",
            "admins.password_hash text",
            "audit_logs.admin_email text not null",
            "audit_logs.admin_id uuid not null",
            "audit_logs.admin_name text not null",
            "audit_logs.audit_area text not null",
            "audit_logs.created_at timestamp with time zone not null",
            "audit_logs.details jsonb",
            "audit_logs.id uuid not null",
            "audit_logs.operation text not null",
            "audit_logs.target_record_id uuid",
            "audit_logs.target_record_type text",
        ];

        List<string> columns = await QueryAsync(
            _collection.Dsn,
            """
            SELECT c.relname || '.' || a.attname || ' ' || format_type(a.atttypid, a.atttypmod) || CASE WHEN a.attnotnull THEN ' not null' ELSE '' END
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND c.relname = ANY(@names) AND a.attnum > 0 AND NOT a.attisdropped
            """,
            ("names", ThreeTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), columns.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ThreeTables_HaveNoColumnDefault_BecauseTheFieldTablesNameNone()
    {
        // 06 4.37, 4.38, 4.63: the "Default" column is "-" in every row.
        List<string> defaults = await QueryAsync(
            _collection.Dsn,
            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = ANY(@names) AND column_default IS NOT NULL",
            ("names", ThreeTables));

        Assert.Empty(defaults);
    }

    [Fact]
    public async Task Indexes_AreTheDocumentedOnes()
    {
        // 06 4.37-4.38 (unique normalized email, filtered unique invitation address, sender link index), 4.63 and 7.5 (CreatedAt; AdminId + CreatedAt).
        string[] expected =
        [
            "admin_invitations.ix_admin_invitations_normalized_email: CREATE UNIQUE INDEX ix_admin_invitations_normalized_email ON public.admin_invitations USING btree (normalized_email) WHERE ((withdrawn_at IS NULL) AND (invalidated_at IS NULL) AND (accepted_at IS NULL))",
            "admin_invitations.ix_admin_invitations_sender_admin_id: CREATE INDEX ix_admin_invitations_sender_admin_id ON public.admin_invitations USING btree (sender_admin_id)",
            "admin_invitations.pk_admin_invitations: CREATE UNIQUE INDEX pk_admin_invitations ON public.admin_invitations USING btree (id)",
            "admins.ix_admins_normalized_email: CREATE UNIQUE INDEX ix_admins_normalized_email ON public.admins USING btree (normalized_email)",
            "admins.pk_admins: CREATE UNIQUE INDEX pk_admins ON public.admins USING btree (id)",
            "audit_logs.ix_audit_logs_admin_id_created_at: CREATE INDEX ix_audit_logs_admin_id_created_at ON public.audit_logs USING btree (admin_id, created_at)",
            "audit_logs.ix_audit_logs_created_at: CREATE INDEX ix_audit_logs_created_at ON public.audit_logs USING btree (created_at)",
            "audit_logs.pk_audit_logs: CREATE UNIQUE INDEX pk_audit_logs ON public.audit_logs USING btree (id)",
        ];

        List<string> indexes = await QueryAsync(
            _collection.Dsn,
            "SELECT tablename || '.' || indexname || ': ' || indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = ANY(@names)",
            ("names", ThreeTables));

        Assert.Equal(expected.Order(StringComparer.Ordinal), indexes.Order(StringComparer.Ordinal));
    }

    // ---- KK2: the administrator's email ----

    [Fact]
    public async Task SecondAdminWithTheSameEmail_InAnotherCase_IsRefused()
    {
        await SaveAsync(c => c.Admins.Add(Admin.Create("Ali", "Ali@Example.com", "hash-1", Now)));
        Admin second = Admin.Create("Veli", "ALI@EXAMPLE.COM", null, Now);

        Assert.Equal((UniqueViolation, "ix_admins_normalized_email"), await ViolationOfAsync(c => c.Admins.Add(second)));
    }

    [Fact]
    public async Task SecondAdminWithTheSameEmail_WithTurkishDottedAndDotlessLetters_IsRefused()
    {
        await SaveAsync(c => c.Admins.Add(Admin.Create("Ali", "ISIK@example.com", "hash-1", Now)));
        Admin second = Admin.Create("Veli", "ısık@example.com", "hash-2", Now);

        Assert.Equal((UniqueViolation, "ix_admins_normalized_email"), await ViolationOfAsync(c => c.Admins.Add(second)));
    }

    [Fact]
    public async Task AdminWithNoPasswordHash_IsAccepted_BecauseThePasswordMayBeInvalidated()
    {
        await SaveAsync(c => c.Admins.Add(Admin.Create("Ali", "ali@example.com", null, Now)));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins WHERE password_hash IS NULL"));
    }

    // ---- KK3: the invitation ----

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task InvitationWithTwoOrMoreOutcomeMoments_IsRefused(bool withdrawn, bool invalidated, bool accepted)
    {
        AdminInvitation invitation = AdminInvitation.Create("yeni@example.com", null, "Ali", Now);
        invitation.WithdrawnAt = withdrawn ? Now : null;
        invitation.InvalidatedAt = invalidated ? Now : null;
        invitation.AcceptedAt = accepted ? Now : null;

        Assert.Equal((CheckViolation, "ck_admin_invitations_at_most_one_outcome"), await ViolationOfAsync(c => c.AdminInvitations.Add(invitation)));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task InvitationWithAtMostOneOutcomeMoment_IsAccepted(bool withdrawn, bool invalidated, bool accepted)
    {
        AdminInvitation invitation = AdminInvitation.Create("yeni@example.com", null, "Ali", Now);
        invitation.WithdrawnAt = withdrawn ? Now : null;
        invitation.InvalidatedAt = invalidated ? Now : null;
        invitation.AcceptedAt = accepted ? Now : null;

        await SaveAsync(c => c.AdminInvitations.Add(invitation));

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations"));
    }

    [Fact]
    public async Task SecondValidInvitationToTheSameAddress_InAnotherCase_IsRefused()
    {
        await SaveAsync(c => c.AdminInvitations.Add(AdminInvitation.Create("Yeni@Example.com", null, "Ali", Now)));
        AdminInvitation second = AdminInvitation.Create("YENI@EXAMPLE.COM", null, "Veli", Now);

        Assert.Equal((UniqueViolation, "ix_admin_invitations_normalized_email"), await ViolationOfAsync(c => c.AdminInvitations.Add(second)));
    }

    [Fact]
    public async Task NewInvitationToTheSameAddress_IsAccepted_WhenTheEarlierOnesAreWithdrawnInvalidatedOrAccepted()
    {
        AdminInvitation withdrawn = AdminInvitation.Create("yeni@example.com", null, "Ali", Now);
        withdrawn.WithdrawnAt = Now;
        AdminInvitation invalidated = AdminInvitation.Create("yeni@example.com", null, "Ali", Now);
        invalidated.InvalidatedAt = Now;
        AdminInvitation accepted = AdminInvitation.Create("yeni@example.com", null, "Ali", Now);
        accepted.AcceptedAt = Now;
        AdminInvitation current = AdminInvitation.Create("Yeni@example.com", null, "Ali", Now);

        await SaveAsync(c => c.AdminInvitations.AddRange(withdrawn, invalidated, accepted, current));

        Assert.Equal(4L, await ScalarAsync<long>("SELECT count(*) FROM admin_invitations WHERE normalized_email = 'yeni@example.com'"));
    }

    // ---- KK6: the sender's removal ----

    [Fact]
    public async Task RemovingTheSender_EmptiesTheLink_AndKeepsTheName()
    {
        Admin sender = Admin.Create("Ali Yılmaz", "ali@example.com", "hash-1", Now);
        AdminInvitation invitation = AdminInvitation.Create("yeni@example.com", sender.Id, sender.Name, Now);
        await SaveAsync(c => c.Admins.Add(sender));
        await SaveAsync(c => c.AdminInvitations.Add(invitation));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM admin_invitations WHERE sender_admin_id = '{sender.Id}'"));

        await ExecuteAsync(_collection.Dsn, $"DELETE FROM admins WHERE id = '{sender.Id}'");

        Assert.Equal("|Ali Yılmaz", await ScalarAsync<string>("SELECT coalesce(sender_admin_id::text, '') || '|' || sender_admin_name FROM admin_invitations"));
    }

    [Fact]
    public async Task SenderLink_IsAnEmptyingForeignKey_WithItsOwnIndex()
    {
        List<string> links = await QueryAsync(
            _collection.Dsn,
            "SELECT conname || ' ' || confdeltype::text FROM pg_constraint WHERE contype = 'f' AND conrelid = 'public.admin_invitations'::regclass");

        Assert.Equal("fk_admin_invitations_admins_sender_admin_id n", Assert.Single(links));
    }

    // ---- KK5: the audit trail's values ----

    [Fact]
    public async Task AuditRow_CarriesTheAdministratorsNameAndEmailAsValues_AndTheAreaOfItsOperation()
    {
        Admin admin = Admin.Create("Ali Yılmaz", "ali@example.com", "hash-1", Now);
        Guid target = EntityIds.New();
        await SaveAsync(c =>
        {
            c.Admins.Add(admin);
            c.AuditLogs.Add(AuditLog.Create(AuditOperation.SendAdminInvitation, admin.Id, admin.Name, admin.Email, "AdminInvitation", target, "{\"InvitedAddress\":\"yeni@example.com\"}", Now));
        });

        // The administrator is removed: the row stays as it was written (06 2.2.5, 4.63).
        await ExecuteAsync(_collection.Dsn, $"DELETE FROM admins WHERE id = '{admin.Id}'");

        await using AppDbContext context = NewContext();
        AuditLog row = await context.AuditLogs.SingleAsync(Ct);
        Assert.Equal(admin.Id, row.AdminId);
        Assert.Equal("Ali Yılmaz", row.AdminName);
        Assert.Equal("ali@example.com", row.AdminEmail);
        Assert.Equal(AuditArea.AdminAccounts, row.AuditArea);
        Assert.Equal("SendAdminInvitation", row.Operation);
        Assert.Equal("AdminInvitation", row.TargetRecordType);
        Assert.Equal(target, row.TargetRecordId);
        Assert.Equal(Now, row.CreatedAt);
        Assert.Contains("yeni@example.com", row.Details, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuditLogs_HasNoForeignKey_AndNoTableRefersToIt()
    {
        List<string> links = await QueryAsync(
            _collection.Dsn,
            "SELECT conname FROM pg_constraint WHERE contype = 'f' AND (conrelid = 'public.audit_logs'::regclass OR confrelid = 'public.audit_logs'::regclass)");

        Assert.Empty(links);
    }

    [Fact]
    public async Task AuditLogs_HasNoCheckConstraintOnTheOperation_ButOnTheArea()
    {
        // D-48 a (09 9.2 item 2): the operation column takes any code; only the area is a closed list.
        List<string> checks = await QueryAsync(
            _collection.Dsn,
            "SELECT conname FROM pg_constraint WHERE contype = 'c' AND conrelid = 'public.audit_logs'::regclass");

        Assert.Equal("ck_audit_logs_audit_area", Assert.Single(checks));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM pg_constraint WHERE conname = 'ck_audit_logs_operation'"));
    }

    [Fact]
    public async Task OperationOfALaterVersion_IsStoredAndReadAsItIs()
    {
        await ExecuteAsync(
            _collection.Dsn,
            "INSERT INTO audit_logs (id, created_at, admin_id, admin_name, admin_email, audit_area, operation) " +
            $"VALUES ('{EntityIds.New()}', now(), '{EntityIds.New()}', 'Ali', 'ali@example.com', 'Settings', 'OperationOfALaterVersion')");

        await using AppDbContext context = NewContext();
        AuditLog row = await context.AuditLogs.SingleAsync(Ct);

        Assert.Equal("OperationOfALaterVersion", row.Operation);
    }

    [Fact]
    public async Task AreaOutsideTheClosedList_IsRefused()
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            _collection.Dsn,
            "INSERT INTO audit_logs (id, created_at, admin_id, admin_name, admin_email, audit_area, operation) " +
            $"VALUES ('{EntityIds.New()}', now(), '{EntityIds.New()}', 'Ali', 'ali@example.com', 'NotAnArea', 'UpdateSettings')"));

        Assert.Equal((CheckViolation, "ck_audit_logs_audit_area"), (error.SqlState, error.ConstraintName));
    }

    [Fact]
    public async Task EveryAuditOperation_HasAnAreaThatTheColumnAccepts()
    {
        // The operation-to-area mapping (09 9.2 item 3) never produces a value the area check constraint refuses.
        await SaveAsync(c =>
        {
            foreach (AuditOperation operation in Enum.GetValues<AuditOperation>())
            {
                c.AuditLogs.Add(AuditLog.Create(operation, EntityIds.New(), "Ali", "ali@example.com", null, null, null, Now));
            }
        });

        Assert.Equal(Enum.GetValues<AuditOperation>().Length, (int)await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task Details_IsAJsonbColumn_ThatRefusesText_ThatIsNotJson()
    {
        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            _collection.Dsn,
            "INSERT INTO audit_logs (id, created_at, admin_id, admin_name, admin_email, audit_area, operation, details) " +
            $"VALUES ('{EntityIds.New()}', now(), '{EntityIds.New()}', 'Ali', 'ali@example.com', 'Settings', 'UpdateSettings', 'not json')"));

        Assert.Equal(PostgresErrorCodes.InvalidTextRepresentation, error.SqlState);
    }

    // ---- KK4: the privileges of the trail ----

    [Fact]
    public async Task ApplicationUser_CanInsertIntoAndReadTheTrail()
    {
        Guid id = EntityIds.New();

        await ExecuteAsync(
            AppDsn,
            "INSERT INTO audit_logs (id, created_at, admin_id, admin_name, admin_email, audit_area, operation) " +
            $"VALUES ('{id}', now(), '{EntityIds.New()}', 'Ali', 'ali@example.com', 'Settings', 'UpdateSettings')");

        Assert.Equal(1L, await ScalarAsync<long>(AppDsn, "SELECT count(*) FROM audit_logs"));
    }

    [Theory]
    [InlineData("UPDATE audit_logs SET admin_name = 'x'")]
    [InlineData("DELETE FROM audit_logs")]
    [InlineData("TRUNCATE audit_logs")]
    public async Task ApplicationUser_CannotUpdateDeleteOrTruncateTheTrail(string statement)
    {
        await SeedExpiredRowsAsync(1, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(AppDsn, statement));

        Assert.Equal(InsufficientPrivilege, error.SqlState);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task ReadOnlyUser_CanReadTheTrail_ButNotWriteIt()
    {
        await SeedExpiredRowsAsync(1, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(1L, await ScalarAsync<long>(ReaderDsn, "SELECT count(*) FROM audit_logs"));
        // The reader's session is read only (25006) and, beyond that setting, it has no write privilege (09 10.8 item 1: the setting is no boundary).
        PostgresException error = await Assert.ThrowsAnyAsync<PostgresException>(() => ExecuteAsync(ReaderDsn, "DELETE FROM audit_logs"));
        Assert.Equal(PostgresErrorCodes.ReadOnlySqlTransaction, error.SqlState);
        Assert.Equal(
            "false|false|false",
            await ScalarAsync<string>(
                $"SELECT has_table_privilege('{Users.Reader}', 'public.audit_logs', 'INSERT')::text || '|' || has_table_privilege('{Users.Reader}', 'public.audit_logs', 'UPDATE')::text || '|' || has_table_privilege('{Users.Reader}', 'public.audit_logs', 'DELETE')::text"));
    }

    [Fact]
    public async Task PrivilegeMatrix_OnTheTablesOfTheMigratedDatabase_HasNoDeviation()
    {
        IReadOnlyList<string> deviations = await PrivilegeMatrix.FindDeviationsAsync(_collection.Dsn, Users, Ct);

        Assert.Empty(deviations);
    }

    // ---- KK4: the disposal function ----

    [Fact]
    public async Task DisposalFunction_IsSecurityDefiner_WithAFixedSearchPath_OwnedByTheSchemaOwner()
    {
        List<string> rows = await QueryAsync(
            _collection.Dsn,
            "SELECT p.prosecdef::text || '|' || coalesce(array_to_string(p.proconfig, ','), '') || '|' || pg_get_userbyid(p.proowner) || '|' || pg_get_function_result(p.oid) " +
            "FROM pg_proc p WHERE p.oid = 'public.dispose_expired_audit_logs(integer, timestamptz)'::regprocedure");

        Assert.Equal($"true|search_path=pg_catalog, public|{Users.Migrator}|integer", Assert.Single(rows));
    }

    [Fact]
    public async Task DisposalFunction_IsGrantedToTheApplicationUserOnly_NotToPublicNorTheReadOnlyUser()
    {
        List<string> grantees = await QueryAsync(
            _collection.Dsn,
            "SELECT CASE WHEN a.grantee = 0 THEN 'PUBLIC' ELSE pg_get_userbyid(a.grantee) END || ' ' || a.privilege_type " +
            "FROM pg_proc p, aclexplode(p.proacl) a WHERE p.oid = 'public.dispose_expired_audit_logs(integer, timestamptz)'::regprocedure");

        Assert.Equal([$"{Users.App} EXECUTE", $"{Users.Migrator} EXECUTE"], grantees.Order(StringComparer.Ordinal));

        PostgresException reader = await Assert.ThrowsAnyAsync<PostgresException>(() => ScalarAsync<int>(ReaderDsn, $"SELECT {DisposeFunction}(1, now())"));
        Assert.Equal(InsufficientPrivilege, reader.SqlState);
        Assert.Equal(
            "false",
            await ScalarAsync<string>($"SELECT has_function_privilege('{Users.BackupStatus}', '{DisposeFunction}(integer, timestamptz)', 'EXECUTE')::text"));
    }

    [Fact]
    public async Task DisposalFunction_DeletesTheRowAtTheBoundary_AndNotOneSecondBefore()
    {
        // Created 2014-12-31T20:59:59Z = 23:59:59 in Turkey: the last second of 2014 there; retention ends with the year 2024 (Z-30: ten years).
        DateTimeOffset created = new(2014, 12, 31, 20, 59, 59, TimeSpan.Zero);
        await SeedExpiredRowsAsync(1, created);

        int before = await DisposeAsync(100, new DateTimeOffset(2024, 12, 31, 20, 59, 59, TimeSpan.Zero));
        long left = await ScalarAsync<long>("SELECT count(*) FROM audit_logs");
        int at = await DisposeAsync(100, new DateTimeOffset(2024, 12, 31, 21, 0, 0, TimeSpan.Zero));

        Assert.Equal(0, before);
        Assert.Equal(1L, left);
        Assert.Equal(1, at);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task DisposalFunction_DoesNotTouchARowWhoseRetentionIsNotOver()
    {
        // A row of five years ago and one of last year are inside the ten years of the real clock; a row of 2010 is outside them.
        await SeedExpiredRowsAsync(1, DateTimeOffset.UtcNow.AddYears(-5));
        await SeedExpiredRowsAsync(1, DateTimeOffset.UtcNow.AddYears(-1));
        await SeedExpiredRowsAsync(1, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));

        int removed = await DisposeAsync(100, DateTimeOffset.UtcNow);

        Assert.Equal(1, removed);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE created_at < '2011-01-01'"));
    }

    [Fact]
    public async Task DisposalFunction_WithAMomentInTheFuture_CannotDeleteARowWhoseRetentionIsNotOver()
    {
        // The given moment is capped by the database clock: a hundred years ahead removes what is expired today and nothing more.
        await SeedExpiredRowsAsync(1, DateTimeOffset.UtcNow.AddYears(-5));
        await SeedExpiredRowsAsync(1, DateTimeOffset.UtcNow.AddMonths(-1));
        await SeedExpiredRowsAsync(1, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));

        int removed = await DisposeAsync(100, DateTimeOffset.UtcNow.AddYears(100));

        Assert.Equal(1, removed);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task DisposalFunction_RemovesAtMostMaxRows_AndReturnsTheCount()
    {
        await SeedExpiredRowsAsync(5, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));
        await SeedExpiredRowsAsync(1, DateTimeOffset.UtcNow);

        int first = await DisposeAsync(3, DateTimeOffset.UtcNow);
        long afterFirst = await ScalarAsync<long>("SELECT count(*) FROM audit_logs");
        int second = await DisposeAsync(10, DateTimeOffset.UtcNow);

        Assert.Equal(3, first);
        Assert.Equal(3L, afterFirst);
        Assert.Equal(2, second);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DisposalFunction_WithANonPositiveMaxRows_RaisesAnError(int maxRows)
    {
        await SeedExpiredRowsAsync(1, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(() => DisposeAsync(maxRows, DateTimeOffset.UtcNow));

        Assert.Equal(PostgresErrorCodes.RaiseException, error.SqlState);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK4: the role settings ----

    [Fact]
    public async Task Migration_WithoutTheApplicationUserRoleSetting_IsRolledBack_AndWithItGrantsTheRightUser()
    {
        string database = "shopfolio_test_" + Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(6));
        string admin = _template.DsnFor(_collection.DatabaseName);
        Dictionary<string, string> variables = Users.Variables(database);
        string migratorDsn = _template.DsnFor(database, Users.Migrator, Users.MigratorPassword, pooling: false);
        string previous = typeof(CreatePriceCouponAndDigitalFileTables).GetCustomAttribute<MigrationAttribute>()!.Id;
        try
        {
            await BootstrapScript.RunAsync(admin, BootstrapScript.DatabaseFile, variables, Ct);
            await BootstrapScript.RunAsync(admin, BootstrapScript.DatabaseAccessFile, variables, Ct);
            await BootstrapScript.RunAsync(_template.DsnFor(database), BootstrapScript.SchemaFile, variables, Ct);
            await MigrateAsync(migratorDsn, previous);

            // The role setting of the application user is gone: the new migration fails in its first privilege statement and leaves nothing.
            await ExecuteAsync(admin, $"ALTER ROLE {Users.Migrator} IN DATABASE {database} RESET shopfolio.application_user");
            PostgresException failure = await Assert.ThrowsAnyAsync<PostgresException>(() => MigrateAsync(migratorDsn, null));
            Assert.Equal(PostgresErrorCodes.UndefinedObject, failure.SqlState);
            string dsn = _template.DsnFor(database);
            Assert.Equal(0L, await ScalarAsync<long>(dsn, "SELECT count(*) FROM pg_tables WHERE schemaname = 'public' AND tablename IN ('admins', 'admin_invitations', 'audit_logs')"));
            Assert.Equal(0L, await ScalarAsync<long>(dsn, "SELECT count(*) FROM pg_proc WHERE proname = 'dispose_expired_audit_logs'"));
            Assert.Equal(previous, await ScalarAsync<string>(dsn, "SELECT max(migration_id) FROM \"__EFMigrationsHistory\""));

            // The setting is back: the migration runs and the privileges are the right user's.
            await ExecuteAsync(admin, $"ALTER ROLE {Users.Migrator} IN DATABASE {database} SET shopfolio.application_user = '{Users.App}'");
            await MigrateAsync(migratorDsn, null);
            Assert.Equal(
                "true|false|false|false",
                await ScalarAsync<string>(
                    dsn,
                    $"SELECT has_function_privilege('{Users.App}', '{DisposeFunction}(integer, timestamptz)', 'EXECUTE')::text || '|' || " +
                    $"has_function_privilege('{Users.Reader}', '{DisposeFunction}(integer, timestamptz)', 'EXECUTE')::text || '|' || " +
                    $"has_table_privilege('{Users.App}', 'public.audit_logs', 'UPDATE')::text || '|' || has_table_privilege('{Users.App}', 'public.audit_logs', 'DELETE')::text"));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS {database} WITH (FORCE)");
        }
    }

    // ---- KK7: one topic, no Down ----

    [Fact]
    public void Migration_HasNoDownMethod()
    {
        MethodInfo? down = typeof(CreateAdminInvitationAndAuditLogTables).GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Null(down);
    }

    // ---- helpers ----

    private static async Task MigrateAsync(string dsn, string? target)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, dsn);
        await using AppDbContext context = new(builder.Options);
        IMigrator migrator = ((IInfrastructure<IServiceProvider>)context).Instance.GetService(typeof(IMigrator)) as IMigrator
            ?? throw new InvalidOperationException("The migrator service is not available.");
        await migrator.MigrateAsync(target, Ct);
    }

    /// <summary>Calls the disposal function as the application user and returns the count it returns.</summary>
    private async Task<int> DisposeAsync(int maxRows, DateTimeOffset asOf)
    {
        await using NpgsqlConnection connection = new(AppDsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT {DisposeFunction}(@max, @asOf)", connection);
        command.Parameters.AddWithValue("max", maxRows);
        command.Parameters.AddWithValue("asOf", asOf.ToUniversalTime());
        return (int)(await command.ExecuteScalarAsync(Ct))!;
    }

    /// <summary>Inserts rows with a given creation moment (as the schema owner's side of the test; the application user may insert as well).</summary>
    private async Task SeedExpiredRowsAsync(int count, DateTimeOffset createdAt)
    {
        for (int i = 0; i < count; i++)
        {
            await using NpgsqlConnection connection = new(_collection.Dsn);
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand command = new(
                "INSERT INTO audit_logs (id, created_at, admin_id, admin_name, admin_email, audit_area, operation) VALUES (@id, @createdAt, @adminId, 'Ali', 'ali@example.com', 'Settings', 'UpdateSettings')",
                connection);
            command.Parameters.AddWithValue("id", EntityIds.New());
            command.Parameters.AddWithValue("createdAt", createdAt.ToUniversalTime());
            command.Parameters.AddWithValue("adminId", EntityIds.New());
            await command.ExecuteNonQueryAsync(Ct);
        }
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    /// <summary>Saves the change and returns the SQLSTATE and the constraint name of the database error it raises.</summary>
    private async Task<(string SqlState, string? Constraint)> ViolationOfAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        PostgresException inner = Assert.IsType<PostgresException>(error.InnerException);
        return (inner.SqlState, inner.ConstraintName);
    }

    private static async Task ExecuteAsync(string dsn, string sql)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private Task<T> ScalarAsync<T>(string sql) => ScalarAsync<T>(_collection.Dsn, sql);

    private static async Task<T> ScalarAsync<T>(string dsn, string sql)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private static async Task<List<string>> QueryAsync(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture)!);
        }

        return rows;
    }
}
