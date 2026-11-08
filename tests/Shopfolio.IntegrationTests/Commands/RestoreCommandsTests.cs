using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Commands;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Commands;

/// <summary>The collection of the restore command tests (T388): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(RestoreCommandsDefinition.Name)]
public sealed class RestoreCommandsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "RestoreCommands";
}

/// <summary>
/// <c>rollback-check</c> and <c>reapply-deletions</c> on a real PostgreSQL (T388; 05 8.5.10, 8.5.11; 09 13.2): the count follows the commit moment and not the row's own time (KK3), a row
/// the restore point precedes by a change is counted whether it was added or changed (KK3, KK4), account and session writes are not the order side (KK3), the check writes nothing (KK1), a count
/// above zero ends with 3 (KK5); the replay deletes only the accounts of the lines from the restore moment (KK6), applies no gate and skips a missing record (KK7), and ends every session and every
/// unused link but the e-mail change revert link (KK8). The database is started with <c>track_commit_timestamp = on</c> (the CI service container and the local test database).
/// </summary>
[Collection(RestoreCommandsDefinition.Name)]
public sealed class RestoreCommandsTests : IAsyncLifetime
{
    // The tables the replay must leave alone (KK7): the trail, the sent records and the job tables (the first deletion wrote them; the replay writes none).
    private static readonly string[] UntouchedTables = ["audit_logs", "dispatch_records", "jobs"];

    private readonly CollectionDatabaseFixture _collection;
    private readonly string _logDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-t388-" + Guid.NewGuid().ToString("N"));

    public RestoreCommandsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE legal_text_versions, sessions, link_records, customers, admins, dispatch_records, jobs, audit_logs CASCADE");
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_logDirectory))
        {
            Directory.Delete(_logDirectory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }

    // ---- rollback-check ----

    [Fact]
    public async Task RollbackCheck_WithNoWriteAfterThePoint_ExitsWithZero()
    {
        await AddLegalTextAsync(1, DateTimeOffset.UtcNow);
        DateTimeOffset point = await PointAfterAsync();
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "rollback-check", "--restore-point", point.ToString("o"));

        Assert.True(exit == 0, terminal.AllOutput);
        Assert.Contains("legal_text_versions: 0", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RollbackCheck_CountsARowAddedAfterThePoint_WhateverItsOwnTimeSays_AndExitsWithThree()
    {
        DateTimeOffset point = await PointAfterAsync();
        await AddLegalTextAsync(1, point.AddYears(-3));
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "rollback-check", "--restore-point", point.ToString("o"));

        Assert.True(exit == 3, terminal.AllOutput);
        Assert.Contains("legal_text_versions: 1", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("dönülmez", terminal.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RollbackCheck_CountsARowChangedAfterThePoint_TheEmptyingOfAnOrderRowIsAChange()
    {
        await AddLegalTextAsync(1, DateTimeOffset.UtcNow);
        DateTimeOffset point = await PointAfterAsync();
        await ExecuteAsync("UPDATE legal_text_versions SET text = 'yeni'");
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "rollback-check", "--restore-point", point.ToString("o"));

        Assert.True(exit == 3, terminal.AllOutput);
        Assert.Contains("legal_text_versions: 1", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RollbackCheck_CountsATransactionThatStartedBeforeThePointAndCommittedAfterIt()
    {
        await using AppDbContext context = NewContext();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Ct);
        context.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, DateTimeOffset.UtcNow, "metin", DateTimeOffset.UtcNow));
        await context.SaveChangesAsync(Ct);

        DateTimeOffset point = await PointAfterAsync();
        await transaction.CommitAsync(Ct);
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "rollback-check", "--restore-point", point.ToString("o"));

        Assert.True(exit == 3, terminal.AllOutput);
        Assert.Contains("legal_text_versions: 1", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RollbackCheck_DoesNotCountTheAccountSessionAndLinkWrites_WhichAreNotTheOrderSide()
    {
        DateTimeOffset point = await PointAfterAsync();
        await SeedAccountsAsync(point);
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "rollback-check", "--restore-point", point.ToString("o"));

        Assert.True(exit == 0, terminal.AllOutput);
    }

    [Fact]
    public async Task RollbackCheck_OnlyReads_NoTransactionIsStartedThatWrites()
    {
        await AddLegalTextAsync(1, DateTimeOffset.UtcNow);
        DateTimeOffset point = await PointAfterAsync();
        string before = await ScalarAsync<string>("SELECT pg_snapshot_xmax(pg_current_snapshot())::text");

        await RunAsync(new FakeTerminal(), "rollback-check", "--restore-point", point.ToString("o"));

        // A transaction that wrote would have been given a number; the next number is the one the check saw.
        Assert.Equal(before, await ScalarAsync<string>("SELECT pg_snapshot_xmax(pg_current_snapshot())::text"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM legal_text_versions"));
    }

    [Fact]
    public async Task CommandHost_ResolvesWhatTheReplayUses_TheTwoModulesTheLogReaderTheSessionStoreAndTheLinks()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:ConnectionString"] = _collection.Dsn, [LoggingOptions.DirectoryKey] = _logDirectory })
            .Build();
        await using ServiceProvider provider = CommandHost.CreateServices(configuration, new FakeTerminal()).BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        Assert.IsAssignableFrom<IMembershipModule>(scope.ServiceProvider.GetRequiredService<IMembershipModule>());
        Assert.IsAssignableFrom<IAdministrationModule>(scope.ServiceProvider.GetRequiredService<IAdministrationModule>());
        Assert.IsType<InstantDeletionLog>(provider.GetRequiredService<InstantDeletionLog>());
        Assert.IsAssignableFrom<SessionStore>(scope.ServiceProvider.GetRequiredService<SessionStore>());
        Assert.IsAssignableFrom<LinkRecords>(scope.ServiceProvider.GetRequiredService<LinkRecords>());
        Assert.IsAssignableFrom<RollbackAudit>(scope.ServiceProvider.GetRequiredService<RollbackAudit>());
    }

    // ---- reapply-deletions ----

    [Fact]
    public async Task Reapply_DeletesTheAccountsOfTheLinesFromTheRestoreMoment_AndLeavesTheOnesBefore()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset point = now.AddHours(-2);
        (Guid customerAfter, Guid customerBefore, Guid adminAfter, Guid adminKept) = await SeedForReplayAsync(now);
        WriteLine(point.AddMinutes(-30), InstantDeletionKinds.CustomerAccount, customerBefore);
        WriteLine(point.AddMinutes(30), InstantDeletionKinds.CustomerAccount, customerAfter);
        WriteLine(point.AddMinutes(40), InstantDeletionKinds.AdminAccount, adminAfter);
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "reapply-deletions", "--restore-point", point.ToString("o"));

        Assert.True(exit == 0, terminal.AllOutput);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{customerAfter}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{customerBefore}'"));
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM admins WHERE id = '{adminAfter}'"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM admins WHERE id = '{adminKept}'"));
        Assert.Contains("Yeniden uygulanan silme: 2", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reapply_AppliesNoGate_TheLastAdministratorIsRemovedAndAMissingRecordIsSkipped()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset point = now.AddHours(-2);
        await using (AppDbContext context = NewContext())
        {
            Admin only = Admin.Create("Tek", "tek@example.com", "digest-placeholder", now);
            context.Admins.Add(only);
            await context.SaveChangesAsync(Ct);
            WriteLine(point.AddMinutes(10), InstantDeletionKinds.AdminAccount, only.Id);
        }

        WriteLine(point.AddMinutes(11), InstantDeletionKinds.CustomerAccount, Guid.NewGuid());
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "reapply-deletions", "--restore-point", point.ToString("o"));

        Assert.True(exit == 0, terminal.AllOutput);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Contains("Yeniden uygulanan silme: 1; kaydı zaten olmayan, atlanan: 1", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reapply_WritesNoTrailSentOrJobRow_AndTheSecondRunIsHarmless()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset point = now.AddHours(-2);
        (Guid customerAfter, _, _, _) = await SeedForReplayAsync(now);
        WriteLine(point.AddMinutes(30), InstantDeletionKinds.CustomerAccount, customerAfter);

        FakeTerminal first = new();
        Assert.True(await RunAsync(first, "reapply-deletions", "--restore-point", point.ToString("o")) == 0, first.AllOutput);
        FakeTerminal second = new();
        int exit = await RunAsync(second, "reapply-deletions", "--restore-point", point.ToString("o"));

        Assert.True(exit == 0, second.AllOutput);
        Assert.Contains("Yeniden uygulanan silme: 0; kaydı zaten olmayan, atlanan: 1", second.Output, StringComparison.Ordinal);
        foreach (string table in UntouchedTables)
        {
            Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
        }
    }

    [Fact]
    public async Task Reapply_DeletesEverySession_AndEndsEveryUnusedLinkAtTheRestoreMoment_ExceptTheRevertLink()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset point = now.AddHours(-2);
        (_, Guid customerBefore, _, Guid adminKept) = await SeedForReplayAsync(now);
        DateTimeOffset later = now.AddDays(1);
        LinkRecord reset = LinkRecord.ForCustomer(LinkType.PasswordReset, customerBefore, "digest-reset", later, now);
        LinkRecord revert = LinkRecord.ForCustomer(LinkType.EmailChangeRevert, customerBefore, "digest-revert", later, now);
        LinkRecord used = LinkRecord.ForAdmin(LinkType.PasswordReset, adminKept, "digest-used", later, now);
        used.UsedAt = now;
        LinkRecord expired = LinkRecord.ForAdmin(LinkType.PasswordReset, adminKept, "digest-expired", point.AddHours(-5), point.AddHours(-6));
        await using (AppDbContext context = NewContext())
        {
            context.Sessions.AddRange(Session.ForCustomer(customerBefore, "hash-1", false, now), Session.ForAdmin(adminKept, "hash-2", true, now));
            context.LinkRecords.AddRange(reset, revert, used, expired);
            await context.SaveChangesAsync(Ct);
        }

        FakeTerminal terminal = new();
        int exit = await RunAsync(terminal, "reapply-deletions", "--restore-point", point.ToString("o"));

        Assert.True(exit == 0, terminal.AllOutput);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        await using AppDbContext read = NewContext();
        Dictionary<Guid, LinkRecord> links = await read.LinkRecords.AsNoTracking().ToDictionaryAsync(l => l.Id, Ct);
        Assert.Equal(point.ToUniversalTime(), links[reset.Id].ExpiresAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(later.ToUniversalTime(), links[revert.Id].ExpiresAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(later.ToUniversalTime(), links[used.Id].ExpiresAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(expired.ExpiresAt.ToUniversalTime(), links[expired.Id].ExpiresAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Reapply_WithAnUnreadableLine_ExitsWithThree_AndStillDoesTheRest()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset point = now.AddHours(-2);
        (Guid customerAfter, _, _, _) = await SeedForReplayAsync(now);
        WriteLine(point.AddMinutes(30), InstantDeletionKinds.CustomerAccount, customerAfter);
        string file = Directory.GetFiles(_logDirectory, "audit-*.jsonl").Single();
        File.AppendAllText(file, "this is not json\n");
        FakeTerminal terminal = new();

        int exit = await RunAsync(terminal, "reapply-deletions", "--restore-point", point.ToString("o"));

        Assert.True(exit == 3, terminal.AllOutput);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM customers WHERE id = '{customerAfter}'"));
        Assert.Contains("Okunamayan kayıt satırı: 1", terminal.Error, StringComparison.Ordinal);
    }

    // ---- seeds and helpers ----

    private async Task AddLegalTextAsync(int number, DateTimeOffset ownTime)
    {
        await using AppDbContext context = NewContext();
        context.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, number, ownTime, "metin", ownTime));
        await context.SaveChangesAsync(Ct);
    }

    private async Task SeedAccountsAsync(DateTimeOffset now)
    {
        await using AppDbContext context = NewContext();
        Customer customer = Customer.Create("Ali", "ali@example.com", "digest-placeholder", now, null, now);
        Admin admin = Admin.Create("Veli", "veli@example.com", "digest-placeholder", now);
        context.Customers.Add(customer);
        context.Admins.Add(admin);
        context.Sessions.Add(Session.ForCustomer(customer.Id, "hash-a", false, now));
        context.LinkRecords.Add(LinkRecord.ForAdmin(LinkType.PasswordReset, admin.Id, "digest-a", now.AddDays(1), now));
        await context.SaveChangesAsync(Ct);
    }

    private async Task<(Guid CustomerAfter, Guid CustomerBefore, Guid AdminAfter, Guid AdminKept)> SeedForReplayAsync(DateTimeOffset now)
    {
        await using AppDbContext context = NewContext();
        Customer after = Customer.Create("Ali", "ali@example.com", "digest-placeholder", now, null, now);
        Customer before = Customer.Create("Ayşe", "ayse@example.com", "digest-placeholder", now, null, now);
        Admin adminAfter = Admin.Create("Veli", "veli@example.com", "digest-placeholder", now);
        Admin adminKept = Admin.Create("Can", "can@example.com", "digest-placeholder", now);
        context.Customers.AddRange(after, before);
        context.Admins.AddRange(adminAfter, adminKept);
        await context.SaveChangesAsync(Ct);
        return (after.Id, before.Id, adminAfter.Id, adminKept.Id);
    }

    // A moment taken after the writes before it and before the writes after it: a short wait each side keeps the commit moments and the clock apart.
    private static async Task<DateTimeOffset> PointAfterAsync()
    {
        await Task.Delay(200, Ct);
        DateTimeOffset point = DateTimeOffset.UtcNow;
        await Task.Delay(200, Ct);
        return point;
    }

    private void WriteLine(DateTimeOffset at, string kind, Guid recordId)
    {
        LoggingSetup.CreateAuditLineWriter(new LoggingOptions { Directory = _logDirectory }, AuditFile.Audit, new ManualTimeProvider(at))
            .Write(AuditLine.InstantDeletion(kind, recordId));
    }

    private async Task<int> RunAsync(FakeTerminal terminal, params string[] arguments)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = _collection.Dsn,
                [LoggingOptions.DirectoryKey] = _logDirectory,
            })
            .Build();
        IServiceCollection services = CommandHost.CreateServices(configuration, terminal);
        CommandDispatcher dispatcher = new(AdminCommandCatalog.All, terminal, () => services.BuildServiceProvider());
        return await dispatcher.ExecuteAsync(arguments, Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }
}
