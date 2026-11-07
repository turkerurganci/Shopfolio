using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>The collection of the audit trail interface tests (T380): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(AuditTrailDefinition.Name)]
public sealed class AuditTrailDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AuditTrail";
}

/// <summary>
/// The audit trail interface on a real PostgreSQL, written and read as the application user (T380 criteria 1-10; 05 4.7; 09 9.2): the row is written in the caller's
/// transaction and goes with it (KK4); the application user can add and read but not change or remove (KK5); a correction is a new row (KK6); no administrator
/// and no transaction mean no row (KK7); the list is flat, newest first, with a Turkey date range and an administrator filter and pages that lose and repeat nothing
/// (KK8); the rows of a removed administrator stay with the name written, and the rows inside their ten years stay when the disposal runs (KK9).
/// </summary>
[Collection(AuditTrailDefinition.Name)]
public sealed class AuditTrailTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid Target = Guid.Parse("018f0000-0000-7000-8000-0000000000aa");

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _time = new(Now);

    public AuditTrailTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE audit_logs, admins CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: the eight areas; who, when, what ----

    [Fact]
    public async Task Write_InsideTheRunner_AddsOneRowWithTheAdministratorTheMomentTheAreaAndTheDetails()
    {
        Admin admin = await SeedAdminAsync("Ali Yılmaz", "ali@example.com");
        AuditDetails details = new("2026ABCD", [new AuditValueChange("TrackingNumber", "A1", "B2")]);

        await RunAsync(admin, trail => trail.WriteAsync(new AuditEntry(AuditOperation.CorrectShippingInfo, "Order", Target, details), Ct));

        AuditLog row = await ReadSingleAsync();
        Assert.Equal(admin.Id, row.AdminId);
        Assert.Equal("Ali Yılmaz", row.AdminName);
        Assert.Equal("ali@example.com", row.AdminEmail);
        Assert.Equal(Now, row.CreatedAt);
        Assert.Equal(AuditArea.Order, row.AuditArea);
        Assert.Equal("CorrectShippingInfo", row.Operation);
        Assert.Equal("Order", row.TargetRecordType);
        Assert.Equal(Target, row.TargetRecordId);
        Assert.Equal("2026ABCD", row.Details!.DisplayName);
        Assert.Equal(new AuditValueChange("TrackingNumber", "A1", "B2"), Assert.Single(row.Details.Changes));
    }

    [Fact]
    public async Task EveryOperation_IsWritten_AndAllEightAreasAreOnTheRows()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        AuditOperation[] operations = Enum.GetValues<AuditOperation>();

        await RunAsync(
            admin,
            async trail =>
            {
                foreach (AuditOperation operation in operations)
                {
                    await trail.WriteAsync(Valid(operation), Ct);
                }
            });

        Assert.Equal(operations.Length, (int)await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(
            Enum.GetNames<AuditArea>().Order(StringComparer.Ordinal),
            (await QueryAsync("SELECT DISTINCT audit_area FROM audit_logs")).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ASettingsChange_KeepsTheOldAndTheNewValue_AndTheListReturnsThem()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        AuditDetails details = new("Havale bilgisi", [new AuditValueChange("Iban", "TR33 0006 1005 1978 6457 8413 26", "TR12 0001 2009 4520 0058 0000 01")]);

        await RunAsync(admin, trail => trail.WriteAsync(new AuditEntry(AuditOperation.UpdateSettings, "CompanySettings", Target, details), Ct));

        AuditPage page = await ReadAsync(trail => trail.ListAsync(new AuditQuery(), Ct));
        AuditRow row = Assert.Single(page.Items);
        Assert.Equal("UpdateSettings", row.Operation);
        Assert.Equal(AuditArea.Settings, row.Area);
        AuditValueChange change = Assert.Single(row.Details!.Changes);
        Assert.Equal("TR33 0006 1005 1978 6457 8413 26", change.OldValue);
        Assert.Equal("TR12 0001 2009 4520 0058 0000 01", change.NewValue);
        Assert.Equal("Ali", row.AdminName);
        Assert.Equal("ali@example.com", row.AdminEmail);
    }

    // ---- KK3: nothing of the customer is written ----

    [Fact]
    public async Task AnOrderEntryWithTheCustomersData_IsRefused_AndNoRowIsWritten()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        string[] personalFields = ["Name", "Address", "ShippingAddress", "Phone", "Email", "ContactEmail", "Iban", "Note", "InternalNote"];
        string[] values = ["Ayşe Yılmaz", "Atatürk Mah. 12. Sk. No:3 Kadıköy", "0532 123 45 67", "ayse@example.com", "TR330006100519786457841326", "Müşteri şunu istedi"];

        // Every attempt is refused before it adds anything: one transaction holds them all and commits nothing.
        await RunAsync(
            admin,
            async trail =>
            {
                foreach (AuditOperation operation in Enum.GetValues<AuditOperation>().Where(o => AuditOperationText.AreaOf(o) == AuditArea.Order))
                {
                    foreach (string field in personalFields)
                    {
                        foreach (string value in values)
                        {
                            AuditEntry entry = new(operation, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue(field, value)]));
                            await Assert.ThrowsAsync<ArgumentException>(() => trail.WriteAsync(entry, Ct));
                        }
                    }

                    AuditEntry withEmail = new(operation, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue("Reason", "ayse@example.com")]));
                    await Assert.ThrowsAsync<ArgumentException>(() => trail.WriteAsync(withEmail, Ct));
                }
            });

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task TheRowOfAnAddressCorrection_NamesTheOrderTheFieldAndTheOperationOnly()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        const string oldAddress = "Eski Mah. 1. Sk. No:1";
        const string newAddress = "Yeni Mah. 2. Sk. No:2";

        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync(admin, trail => trail.WriteAsync(
            new AuditEntry(AuditOperation.CorrectAddress, "Order", Target, new AuditDetails("2026ABCD", [new AuditValueChange("Field", oldAddress, newAddress)])), Ct)));
        await RunAsync(admin, trail => trail.WriteAsync(
            new AuditEntry(AuditOperation.CorrectAddress, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue("Field", "ShippingAddress")])), Ct));

        string stored = await ScalarAsync<string>("SELECT details::text FROM audit_logs");
        Assert.DoesNotContain("Mah.", stored, StringComparison.Ordinal);
        Assert.Contains("ShippingAddress", stored, StringComparison.Ordinal);
    }

    // ---- KK4: the row is written in the operation's transaction ----

    [Fact]
    public async Task ARefusedResult_UndoesTheRow_AndAnExceptionUndoesIt()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");

        await RunAsync(admin, trail => trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct), rejected: true);
        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(
            admin,
            async trail =>
            {
                await trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct);
                throw new InvalidOperationException("The operation failed after the row was added.");
            }));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task ACommittedOperation_KeepsEveryRowItWrote()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");

        await RunAsync(
            admin,
            async trail =>
            {
                await trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct);
                await trail.WriteAsync(Valid(AuditOperation.MarkDelivered), Ct);
            });

        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task Write_OutsideATransaction_IsRefused_AndNothingIsWritten()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        await using AppDbContext db = NewContext();
        AuditTrail trail = new(db, new FixedActor(admin), _time);

        await Assert.ThrowsAsync<InvalidOperationException>(() => trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct));
        await db.SaveChangesAsync(Ct);

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK7: no administrator, no row ----

    [Fact]
    public async Task WithoutAnAdministrator_ANonAdminCallerGetsAnErrorAndNoRow()
    {
        await using AppDbContext db = NewContext();
        AuditTrail trail = new(db, new FixedActor(null), _time);
        TransactionRunner runner = NewRunner(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(
            async cancel =>
            {
                await trail.WriteAsync(Valid(AuditOperation.MarkShipped), cancel);
                return new Outcome(false);
            },
            Ct));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task TheRequestSource_HasNoActor_ForACodeOutsideARequestAndForARequestWithoutASession()
    {
        await using AppDbContext db = NewContext();

        Assert.Null(await new RequestAuditActorSource(new Microsoft.AspNetCore.Http.HttpContextAccessor(), db).CurrentAsync(Ct));
    }

    // ---- KK5, KK6: only added to; a correction is a new row ----

    [Fact]
    public async Task TheApplicationUser_CannotChangeOrRemoveARow_ThroughTheContext()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        await RunAsync(admin, trail => trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct));
        await using AppDbContext db = NewContext();

        PostgresException update = await Assert.ThrowsAsync<PostgresException>(() => db.AuditLogs.ExecuteUpdateAsync(s => s.SetProperty(l => l.AdminName, "x"), Ct));
        PostgresException delete = await Assert.ThrowsAsync<PostgresException>(() => db.AuditLogs.ExecuteDeleteAsync(Ct));

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, update.SqlState);
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, delete.SqlState);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal("Ali", await ScalarAsync<string>("SELECT admin_name FROM audit_logs"));
    }

    [Fact]
    public async Task ACorrectionOfAWrongTransition_IsANewRow_AndTheWrongOnesRowStaysAsItWas()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        await RunAsync(admin, trail => trail.WriteAsync(Valid(AuditOperation.MarkDelivered), Ct));
        AuditLog wrong = await ReadSingleAsync();
        _time.Advance(TimeSpan.FromMinutes(5));

        await RunAsync(admin, trail => trail.WriteAsync(new AuditEntry(AuditOperation.CorrectTransition, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue("TransitionCode", "S6")])), Ct));

        AuditPage page = await ReadAsync(trail => trail.ListAsync(new AuditQuery(), Ct));
        Assert.Equal(["CorrectTransition", "MarkDelivered"], page.Items.Select(r => r.Operation));
        AuditRow first = page.Items[1];
        Assert.Equal(wrong.Id, first.Id);
        Assert.Equal(wrong.CreatedAt, first.At);
    }

    // ---- KK9: the removed administrator and the ten years ----

    [Fact]
    public async Task TheRowsOfARemovedAdministrator_StayWithTheNameWritten_AndTheFilterListsHimRemoved()
    {
        Admin ali = await SeedAdminAsync("Ali Yılmaz", "ali@example.com");
        Admin veli = await SeedAdminAsync("Veli Kaya", "veli@example.com");
        await RunAsync(ali, trail => trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct));
        await RunAsync(veli, trail => trail.WriteAsync(Valid(AuditOperation.MarkDelivered), Ct));
        await ExecuteAsync($"DELETE FROM admins WHERE id = '{ali.Id}'");

        AuditPage page = await ReadAsync(trail => trail.ListAsync(new AuditQuery(), Ct));
        IReadOnlyList<AuditAdminOption> options = await ReadAsync(trail => trail.ListAdminsAsync(Ct));

        Assert.Equal(["Ali Yılmaz", "Veli Kaya"], page.Items.Select(r => r.AdminName).Order(StringComparer.Ordinal));
        Assert.Equal(
            [new AuditAdminOption(veli.Id, "Veli Kaya", "veli@example.com", false), new AuditAdminOption(ali.Id, "Ali Yılmaz", "ali@example.com", true)],
            options);
    }

    [Fact]
    public async Task TheFilter_ListsAnAdministratorOnce_AtHisCurrentNameWhenHeStillHasAnAccount()
    {
        Admin ali = await SeedAdminAsync("Ali", "ali@example.com");
        await RunAsync(ali, trail => trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct));
        await ExecuteAsync($"UPDATE admins SET name = 'Ali Yeni Ad' WHERE id = '{ali.Id}'");
        await RunAsync(await ReloadAsync(ali), trail => trail.WriteAsync(Valid(AuditOperation.MarkDelivered), Ct));

        IReadOnlyList<AuditAdminOption> options = await ReadAsync(trail => trail.ListAdminsAsync(Ct));

        Assert.Equal(new AuditAdminOption(ali.Id, "Ali Yeni Ad", "ali@example.com", false), Assert.Single(options));
    }

    [Fact]
    public async Task ARowNineYearsOld_StaysWhenTheDisposalRuns_AndTheRowsInsideTheirYearsStay()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        DateTimeOffset real = DateTimeOffset.UtcNow;
        await RunAsync(admin, trail => trail.WriteAsync(Valid(AuditOperation.MarkShipped), Ct));
        await SeedRowAsync(admin, real.AddYears(-9));
        await SeedRowAsync(admin, new DateTimeOffset(2010, 6, 1, 0, 0, 0, TimeSpan.Zero));

        int removed = await ScalarAsync<int>(AppDsn, $"SELECT public.dispose_expired_audit_logs(100, '{real.UtcDateTime:O}'::timestamptz)");

        Assert.Equal(1, removed);
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK8: the flat list ----

    [Fact]
    public async Task TheList_IsNewestFirst_AndRowsOfTheSameMomentKeepAStableOrder()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        DateTimeOffset at = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
        await SeedRowAsync(admin, at.AddHours(-2));
        await SeedRowAsync(admin, at.AddHours(1));
        await SeedRowAsync(admin, at);

        AuditPage page = await ReadAsync(trail => trail.ListAsync(new AuditQuery(), Ct));

        Assert.Equal([at.AddHours(1), at, at.AddHours(-2)], page.Items.Select(r => r.At));
        Assert.Equal(1, page.PageCount);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task TheDateRange_IsTurkeyDays_TheLastDayIncludedAndTheNextDaysMidnightExcluded()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        DateTimeOffset beforeStart = new(2026, 11, 3, 20, 59, 59, TimeSpan.Zero);
        DateTimeOffset atStart = new(2026, 11, 3, 21, 0, 0, TimeSpan.Zero);
        DateTimeOffset lastSecond = new(2026, 11, 4, 20, 59, 59, TimeSpan.Zero);
        DateTimeOffset nextMidnight = new(2026, 11, 4, 21, 0, 0, TimeSpan.Zero);
        foreach (DateTimeOffset at in new[] { beforeStart, atStart, lastSecond, nextMidnight })
        {
            await SeedRowAsync(admin, at);
        }

        DateOnly day = new(2026, 11, 4);
        AuditPage both = await ReadAsync(trail => trail.ListAsync(new AuditQuery(From: day, To: day), Ct));
        AuditPage fromOnly = await ReadAsync(trail => trail.ListAsync(new AuditQuery(From: day), Ct));
        AuditPage toOnly = await ReadAsync(trail => trail.ListAsync(new AuditQuery(To: day), Ct));

        Assert.Equal([lastSecond, atStart], both.Items.Select(r => r.At));
        Assert.Equal([nextMidnight, lastSecond, atStart], fromOnly.Items.Select(r => r.At));
        Assert.Equal([lastSecond, atStart, beforeStart], toOnly.Items.Select(r => r.At));
    }

    [Fact]
    public async Task TheAdministratorFilter_WorksOnTheIdWrittenInTheRows_AndTheTwoFiltersApplyTogether()
    {
        Admin ali = await SeedAdminAsync("Ali", "ali@example.com");
        Admin veli = await SeedAdminAsync("Veli", "veli@example.com");
        DateTimeOffset day1 = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
        DateTimeOffset day2 = new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);
        await SeedRowAsync(ali, day1);
        await SeedRowAsync(ali, day2);
        await SeedRowAsync(veli, day1);
        await ExecuteAsync($"UPDATE admins SET name = 'Ali Yeni Ad', email = 'yeni@example.com' WHERE id = '{ali.Id}'");

        AuditPage byAdmin = await ReadAsync(trail => trail.ListAsync(new AuditQuery(AdminId: ali.Id), Ct));
        AuditPage together = await ReadAsync(trail => trail.ListAsync(new AuditQuery(From: new DateOnly(2026, 11, 2), AdminId: veli.Id), Ct));

        Assert.Equal(2, byAdmin.TotalCount);
        Assert.All(byAdmin.Items, r => Assert.Equal(ali.Id, r.AdminId));
        Assert.Empty(together.Items);
        Assert.Equal(1, together.PageCount);
    }

    [Fact]
    public async Task ThePages_AreTwentyFiveRows_NeverRepeatNorLoseARowAtTheirBoundary_AndAPastTheEndPageShowsTheLast()
    {
        Admin admin = await SeedAdminAsync("Ali", "ali@example.com");
        DateTimeOffset same = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
        for (int i = 0; i < PageSizes.Panel * 2 + 1; i++)
        {
            await SeedRowAsync(admin, same);
        }

        AuditPage first = await ReadAsync(trail => trail.ListAsync(new AuditQuery(Page: 1), Ct));
        AuditPage second = await ReadAsync(trail => trail.ListAsync(new AuditQuery(Page: 2), Ct));
        AuditPage third = await ReadAsync(trail => trail.ListAsync(new AuditQuery(Page: 3), Ct));
        AuditPage past = await ReadAsync(trail => trail.ListAsync(new AuditQuery(Page: int.MaxValue), Ct));

        Assert.Equal([25, 25, 1], new[] { first.Items.Count, second.Items.Count, third.Items.Count });
        Assert.Equal(3, first.PageCount);
        Guid[] ids = [.. first.Items.Concat(second.Items).Concat(third.Items).Select(r => r.Id)];
        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.Equal(ids.Length, (int)await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(3, past.PageNumber);
        Assert.Single(past.Items);
    }

    [Fact]
    public async Task AnEmptyTrail_IsOneEmptyPage()
    {
        AuditPage page = await ReadAsync(trail => trail.ListAsync(new AuditQuery(), Ct));

        Assert.Empty(page.Items);
        Assert.Equal(1, page.PageCount);
        Assert.Equal(1, page.PageNumber);
        Assert.Empty(await ReadAsync(trail => trail.ListAdminsAsync(Ct)));
    }

    [Fact]
    public async Task ARowOfALaterVersion_IsListedWithItsCodeAndItsArea_NeverAsAnError()
    {
        await ExecuteAsync(
            "INSERT INTO audit_logs (id, created_at, admin_id, admin_name, admin_email, audit_area, operation, details) " +
            $"VALUES ('{EntityIds.New()}', now(), '{EntityIds.New()}', 'Ali', 'ali@example.com', 'Settings', 'OperationOfALaterVersion', '{{\"DisplayName\":\"x\",\"FromALaterVersion\":[1,2]}}')");

        AuditPage page = await ReadAsync(trail => trail.ListAsync(new AuditQuery(), Ct));

        AuditRow row = Assert.Single(page.Items);
        Assert.Equal("OperationOfALaterVersion", row.Operation);
        Assert.Equal("x", row.Details!.DisplayName);
    }

    // ---- helpers ----

    private static readonly Guid TargetId = Guid.Parse("018f0000-0000-7000-8000-000000000001");

    /// <summary>An entry that meets every rule of its operation.</summary>
    private static AuditEntry Valid(AuditOperation operation)
    {
        IReadOnlyList<string>? kinds = AuditEntryRules.TargetTypesOf(operation);
        string? type = kinds is null ? "Product" : kinds.Count == 0 ? null : kinds[0];
        Guid? id = type is null ? null : TargetId;
        AuditDetails? details = operation is AuditOperation.UpdateCompany or AuditOperation.UpdateBranding or AuditOperation.UpdateSettings
            ? new AuditDetails("Ayar", [new AuditValueChange("Field", "1", "2")])
            : null;
        return new AuditEntry(operation, type, id, details);
    }

    private AppDbContext NewContext()
    {
        Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private AppDbContext NewOwnerContext()
    {
        Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private static TransactionRunner NewRunner(AppDbContext db) => new(db, new JobEnqueueTracker(), new JobSignal(), new CacheInvalidations(db), new AppCache());

    /// <summary>Runs the work in the application user's transaction with the administrator as the actor; <paramref name="rejected"/> makes the result a refusal.</summary>
    private async Task RunAsync(Admin admin, Func<AuditTrail, Task> work, bool rejected = false)
    {
        await using AppDbContext db = NewContext();
        AuditTrail trail = new(db, new FixedActor(admin), _time);
        await NewRunner(db).RunAsync(
            async cancel =>
            {
                await work(trail);
                return new Outcome(rejected);
            },
            Ct);
    }

    private async Task<T> ReadAsync<T>(Func<AuditTrail, Task<T>> read)
    {
        await using AppDbContext db = NewContext();
        return await read(new AuditTrail(db, new FixedActor(null), _time));
    }

    private async Task<AuditLog> ReadSingleAsync()
    {
        await using AppDbContext db = NewOwnerContext();
        return await db.AuditLogs.AsNoTracking().SingleAsync(Ct);
    }

    private async Task<Admin> SeedAdminAsync(string name, string email)
    {
        await using AppDbContext db = NewOwnerContext();
        Admin admin = Admin.Create(name, email, "hash", Now);
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return admin;
    }

    private async Task<Admin> ReloadAsync(Admin admin)
    {
        await using AppDbContext db = NewOwnerContext();
        return await db.Admins.AsNoTracking().SingleAsync(a => a.Id == admin.Id, Ct);
    }

    /// <summary>Adds a row with a given moment as the schema owner's side of the test (the application user may insert as well).</summary>
    private async Task SeedRowAsync(Admin admin, DateTimeOffset at)
    {
        await using AppDbContext db = NewOwnerContext();
        db.AuditLogs.Add(AuditLog.Create(AuditOperation.MarkShipped, admin.Id, admin.Name, admin.Email, "Order", Target, null, at));
        await db.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
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

    private async Task<List<string>> QueryAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        List<string> rows = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }

    private sealed record Outcome(bool Rejected) : IOperationResult
    {
        public bool IsRejected => Rejected;
    }

    private sealed class FixedActor : IAuditActorSource
    {
        private readonly AuditActor? _actor;

        public FixedActor(Admin? admin)
        {
            _actor = admin is null ? null : new AuditActor(admin.Id, admin.Name, admin.Email);
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult(_actor);
    }
}
