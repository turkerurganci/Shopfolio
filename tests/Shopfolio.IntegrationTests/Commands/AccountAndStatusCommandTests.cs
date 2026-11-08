using System.Diagnostics;
using System.Net;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Npgsql;

using Shopfolio.Commands;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Commands;

/// <summary>
/// <c>end-sessions</c>, <c>invalidate-password</c> and <c>status</c> on a real PostgreSQL (T389; 05 8.5.3, 8.5.4, 8.5.6; 09 13.2, 13.3 item 5): the sessions of exactly the named account
/// end (KK1), the password is unusable and the sessions end in one transaction (KK2), an unknown address ends with 3 and the "no target" line (KK3), the line is written before the work and
/// holds no personal value and a redirected input ends with 2 (KK4), the recovery order holds end to end (KK5), the status output holds the conditions and the job rows (KK6), the two
/// provider numbers follow the rule of 08 2.1.2 (KK7), every registered conversion's remaining count is written (KK8), and none of the three writes the operation trail or makes a
/// notification (KK9).
/// </summary>
[Collection(CreateAdminCommandDefinition.Name)]
public sealed class AccountAndStatusCommandTests : IAsyncLifetime
{
    private const string EmailA = "ayse.yilmaz@example.com";
    private const string EmailB = "mehmet.kaya@example.com";
    private const string NameA = "Ayşe Yılmaz";
    private const string NameB = "Mehmet Kaya";
    private const string Password = "orchid-lantern-4471-violet";
    private static readonly Guid _noTarget = Guid.Empty;
    private static readonly IPAddress _ip = IPAddress.Parse("203.0.113.5");

    // The tables none of the three commands may write (KK9): the operation trail, the sent records, the job rows and the sign-in records.
    private static readonly string[] _untouchedTables = ["audit_logs", "dispatch_records", "jobs", "login_records"];

    private static int _counter;

    private readonly CollectionDatabaseFixture _collection;
    private readonly string _logDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-t389-" + Guid.NewGuid().ToString("N"));

    public AccountAndStatusCommandTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE admins, sessions, login_records, attempt_records, dispatch_records, jobs, audit_logs, refund_records, refunds, payment_records, orders, provinces, legal_text_versions CASCADE");
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_logDirectory))
        {
            Directory.Delete(_logDirectory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }

    // ---- KK1: the account's sessions end ----

    [Fact]
    public async Task EndSessions_DeletesEverySessionOfTheNamedAccount_AndKeepsTheOtherAccountsAndTheAccountRow()
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        Guid b = await SeedAdminAsync(NameB, EmailB);
        await OpenSessionAsync(a);
        await OpenSessionAsync(a);
        Guid kept = await OpenSessionAsync(b);
        string accounts = await ScalarAsync<string>("SELECT string_agg(to_jsonb(t)::text, '|' ORDER BY t.id) FROM admins t");

        FakeTerminal terminal = Interactive(EmailA);
        int exit = await RunAsync(terminal, "end-sessions");

        Assert.Equal(0, exit);
        Assert.Equal(0L, await SessionCountAsync(a));
        Assert.Equal([kept], await SessionIdsAsync(b));
        Assert.Equal(accounts, await ScalarAsync<string>("SELECT string_agg(to_jsonb(t)::text, '|' ORDER BY t.id) FROM admins t"));
        Assert.DoesNotContain(EmailA, terminal.AllOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(NameA, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EndSessions_TheEmailIsMatchedByTheNormalizedForm_NotTheTypedCase()
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        await OpenSessionAsync(a);

        int exit = await RunAsync(Interactive("  " + EmailA.ToUpperInvariant() + " "), "end-sessions");

        Assert.Equal(0, exit);
        Assert.Equal(0L, await SessionCountAsync(a));
    }

    // ---- KK2: the password is unusable and the sessions end in the same transaction ----

    [Fact]
    public async Task InvalidatePassword_EmptiesTheDigest_DeletesTheSessions_AndRefusesTheOldPassword_ButTheResetPathStaysOpen()
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        Guid b = await SeedAdminAsync(NameB, EmailB);
        await OpenSessionAsync(a);
        await OpenSessionAsync(a);
        Guid kept = await OpenSessionAsync(b);
        Assert.True((await CheckCredentialsAsync(EmailA, Password)).Succeeded);

        int exit = await RunAsync(Interactive(EmailA), "invalidate-password");

        Assert.Equal(0, exit);
        Assert.Null(await NullableScalarAsync<string>($"SELECT password_hash FROM admins WHERE id = '{a}'"));
        Assert.NotNull(await NullableScalarAsync<string>($"SELECT password_hash FROM admins WHERE id = '{b}'"));
        Assert.Equal(0L, await SessionCountAsync(a));
        Assert.Equal([kept], await SessionIdsAsync(b));
        Assert.False((await CheckCredentialsAsync(EmailA, Password)).Succeeded);
        Assert.True((await CheckCredentialsAsync(EmailB, Password)).Succeeded);

        // The account opens again only through the reset: the request for a link still reaches the account.
        ResetRequested requested = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().RequestPasswordResetAsync(EmailA, _ip, Ct));
        Assert.True(requested.IsIssued);
        Assert.Equal(a, requested.AccountId);
    }

    [Fact]
    public async Task InvalidatePassword_WhenTheSessionsCannotBeDeleted_TheDigestStays_BothAreOneTransaction()
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        await OpenSessionAsync(a);
        string digest = await ScalarAsync<string>($"SELECT password_hash FROM admins WHERE id = '{a}'");
        await ExecuteAsync("CREATE FUNCTION t389_refuse_session_delete() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 't389 refuses'; END $$");
        await ExecuteAsync("CREATE TRIGGER t389_refuse BEFORE DELETE ON sessions FOR EACH ROW EXECUTE FUNCTION t389_refuse_session_delete()");
        try
        {
            int exit = await RunAsync(Interactive(EmailA), "invalidate-password");

            Assert.Equal(1, exit);
            Assert.Equal(digest, await ScalarAsync<string>($"SELECT password_hash FROM admins WHERE id = '{a}'"));
            Assert.Equal(1L, await SessionCountAsync(a));
        }
        finally
        {
            await ExecuteAsync("DROP TRIGGER t389_refuse ON sessions");
            await ExecuteAsync("DROP FUNCTION t389_refuse_session_delete()");
        }
    }

    // ---- KK3: an unknown address ----

    [Theory]
    [InlineData("end-sessions", "yok.kimse@example.com")]
    [InlineData("invalidate-password", "yok.kimse@example.com")]
    [InlineData("end-sessions", "")]
    [InlineData("invalidate-password", "   ")]
    public async Task UnknownOrEmptyEmail_ExitsWithThree_ChangesNothing_AndTheLineHoldsTheEmptyId(string command, string email)
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        await OpenSessionAsync(a);
        string before = await AccountsAndSessionsAsync();
        FakeTerminal terminal = Interactive(email);

        int exit = await RunAsync(terminal, command);

        Assert.Equal(3, exit);
        Assert.Equal(before, await AccountsAndSessionsAsync());
        string file = Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"));
        string line = Assert.Single(ReadShared(file).Split('\n', StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains(command, line, StringComparison.Ordinal);
        Assert.Contains(_noTarget.ToString(), line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(a.ToString(), line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("kimse", line + terminal.AllOutput, StringComparison.OrdinalIgnoreCase);
    }

    // ---- KK4: the order, the content of the line, the redirected input ----

    [Theory]
    [InlineData("end-sessions")]
    [InlineData("invalidate-password")]
    public async Task TheLine_IsWrittenBeforeTheWork_CarriesTheAccountId_AndHoldsNoEmailOrName(string command)
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        await OpenSessionAsync(a);
        await OpenSessionAsync(a);
        List<(AuditLine Line, long Sessions, bool HasDigest)> seen = [];
        string dsn = _collection.Dsn;

        int exit = await RunAsync(
            Interactive(EmailA),
            services => services.Replace(ServiceDescriptor.Singleton<IAuditLineWriter>(new SpyWriter(line =>
            {
                using NpgsqlConnection connection = new(dsn);
                connection.Open();
                using NpgsqlCommand count = new($"SELECT count(*) FROM sessions WHERE admin_id = '{a}'", connection);
                using NpgsqlCommand digest = new($"SELECT password_hash IS NOT NULL FROM admins WHERE id = '{a}'", connection);
                seen.Add((line, (long)count.ExecuteScalar()!, (bool)digest.ExecuteScalar()!));
            }))),
            command);

        (AuditLine written, long sessionsAtWriteTime, bool hasDigestAtWriteTime) = Assert.Single(seen);
        Assert.Equal(0, exit);
        Assert.Equal(a, written.Id);
        Assert.Equal(command, written.Name);
        Assert.Equal(2L, sessionsAtWriteTime);
        Assert.True(hasDigestAtWriteTime);
        Assert.Equal(0L, await SessionCountAsync(a));
    }

    [Theory]
    [InlineData("end-sessions")]
    [InlineData("invalidate-password")]
    public async Task TheCommandFile_HoldsOneLineWithTheCommandAndTheAccountId_AndNoEmailOrName(string command)
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);

        int exit = await RunAsync(Interactive(EmailA), command);

        Assert.Equal(0, exit);
        string file = Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"));
        Assert.Empty(Directory.GetFiles(_logDirectory, "audit-*.jsonl"));
        string text = ReadShared(file);
        string line = Assert.Single(text.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        using JsonDocument json = JsonDocument.Parse(line);
        Assert.True(json.RootElement.ValueKind == JsonValueKind.Object);
        Assert.Contains(LogEvents.AdminCommand, line, StringComparison.Ordinal);
        Assert.Contains(command, line, StringComparison.Ordinal);
        Assert.Contains(a.ToString(), line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ayşe", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Yılmaz", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ayse", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("example.com", text, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("end-sessions")]
    [InlineData("invalidate-password")]
    public async Task RealProcess_WithARedirectedInput_ExitsWithTwo_WritesNoLine_AndChangesNoRow(string command)
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        await OpenSessionAsync(a);
        string before = await AccountsAndSessionsAsync();

        (int exit, string output) = await RunProcessAsync(["admin", command], standardInput: EmailA + "\n");

        Assert.Equal(2, exit);
        Assert.Equal(before, await AccountsAndSessionsAsync());
        Assert.True(!Directory.Exists(_logDirectory) || Directory.GetFiles(_logDirectory).Length == 0);
        Assert.DoesNotContain(EmailA, output, StringComparison.Ordinal);
        Assert.Equal(1L, await SessionCountAsync(a));
    }

    // ---- KK5: the recovery order, end to end ----

    [Fact]
    public async Task Recovery_EndSessionsThenInvalidatePassword_ASessionOpenedBetweenTheTwoFallsToo()
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        Guid other = await SeedAdminAsync(NameB, EmailB);
        await OpenSessionAsync(a);
        Guid kept = await OpenSessionAsync(other);

        Assert.Equal(0, await RunAsync(Interactive(EmailA), "end-sessions"));
        Assert.Equal(0L, await SessionCountAsync(a));

        // The intruder, holding the old password, signs in between the two commands.
        Assert.True((await CheckCredentialsAsync(EmailA, Password)).Succeeded);
        Guid between = await OpenSessionAsync(a);
        Assert.Equal([between], await SessionIdsAsync(a));

        Assert.Equal(0, await RunAsync(Interactive(EmailA), "invalidate-password"));

        Assert.Equal(0L, await SessionCountAsync(a));
        Assert.Null(await NullableScalarAsync<string>($"SELECT password_hash FROM admins WHERE id = '{a}'"));
        Assert.False((await CheckCredentialsAsync(EmailA, Password)).Succeeded);
        Assert.Equal([kept], await SessionIdsAsync(other));
    }

    // ---- KK6: the status output ----

    [Fact]
    public async Task Status_WritesTheVersion_EveryConditionByName_TheJobQueueAndTheUnmeasurableOnes()
    {
        FakeTerminal terminal = new() { IsInputRedirected = false };

        int exit = await RunAsync(terminal, "status");

        Assert.Equal(0, exit);
        string output = terminal.Output;
        Assert.Contains("Sürüm: ", output, StringComparison.Ordinal);
        Assert.Contains("Koşullar:", output, StringComparison.Ordinal);
        using ServiceProvider provider = CommandHost.CreateServices(Configuration(), terminal).BuildServiceProvider();
        IStatusCondition[] conditions = [.. provider.GetServices<IStatusCondition>()];
        Assert.NotEmpty(conditions);
        Assert.All(conditions, c => Assert.Contains($"  {c.Name}: ", output, StringComparison.Ordinal));
        Assert.Equal(conditions.Count(c => c.ReadsProcessMemory), CountOf(output, "ölçülemez (web sürecinin belleğinde)"));
        Assert.Equal(3, conditions.Count(c => c.ReadsProcessMemory));
        Assert.Contains("  JobQueue: kapalı", output, StringComparison.Ordinal);
        Assert.Contains("Başarısız iş satırı: 0", output, StringComparison.Ordinal);
        Assert.Contains("Zamanı gelmiş iş satırı: 0", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_AFailedJobRow_OpensTheJobQueueCondition_AndIsCounted()
    {
        await using (AppDbContext db = NewContext())
        {
            Job failed = Job.Enqueue(JobType.CardRefund, EntityIds.New(), null, DateTimeOffset.UtcNow.AddHours(-2));
            failed.JobState = JobState.Failed;
            failed.AttemptCount = JobTimings.DefaultRetrySchedule.Count + 1;
            failed.LastErrorSummary = nameof(InvalidOperationException);
            db.Jobs.Add(failed);
            await db.SaveChangesAsync(Ct);
        }

        FakeTerminal terminal = new() { IsInputRedirected = false };
        int exit = await RunAsync(terminal, "status");

        Assert.Equal(0, exit);
        Assert.Contains("  JobQueue: AÇIK", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("Başarısız iş satırı: 1", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("Durum ucu: 503", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_TheCommandsOwnLine_HoldsTheEmptyId_AndNoPersonalValue()
    {
        int exit = await RunAsync(new FakeTerminal { IsInputRedirected = false }, "status");

        Assert.Equal(0, exit);
        string file = Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"));
        string line = Assert.Single(ReadShared(file).Split('\n', StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("status", line, StringComparison.Ordinal);
        Assert.Contains(_noTarget.ToString(), line, StringComparison.OrdinalIgnoreCase);
    }

    // ---- KK7: the two provider numbers ----

    [Fact]
    public async Task Status_TheFirstNumberCountsTheWaitingCardOrdersAndTheUnfinishedCardRefunds_NotAFailedRefund()
    {
        await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, PaymentMethod.BankTransfer);
        Guid paid = await SeedCardOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        Guid failedRefund = await SeedRefundAsync(paid, "tx-failed");
        await AddRecordAsync(failedRefund, RefundRecordState.Failed);
        await SeedRefundAsync(paid, "tx-open");
        Guid sendingRefund = await SeedRefundAsync(paid, "tx-sending");
        await AddRecordAsync(sendingRefund, RefundRecordState.Sending);
        Guid unknownRefund = await SeedRefundAsync(paid, "tx-unknown");
        await AddRecordAsync(unknownRefund, RefundRecordState.OutcomePending);

        FakeTerminal terminal = new() { IsInputRedirected = false };
        Assert.Equal(0, await RunAsync(terminal, "status"));

        Assert.Contains("Açık kart işi: 4 (ödemesi bekleyen kart siparişi 1, bitmemiş kart iadesi 3)", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_TheSecondNumberCountsAWithinWithdrawalWindowCardOrder_AndNotAnExpiredOneOrATransferOne()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        await SeedCardOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, itemDeliveryDate: today.AddDays(-2));
        await SeedCardOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, itemDeliveryDate: today.AddDays(-90));
        await SeedCardOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid);
        await SeedCardOrderAsync(OrderStatus.Preparing, PaymentStatus.Paid, PaymentMethod.BankTransfer);
        await SeedCardOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.BankTransfer, itemDeliveryDate: today.AddDays(-2));
        await SeedCardOrderAsync(OrderStatus.Placed, PaymentStatus.Pending);
        await SeedCardOrderAsync(OrderStatus.Delivered, PaymentStatus.Refunded, itemDeliveryDate: today.AddDays(-2));
        string before = await OrdersSnapshotAsync();

        FakeTerminal terminal = new() { IsInputRedirected = false };
        Assert.Equal(0, await RunAsync(terminal, "status"));

        // In the window (delivered two days ago) and the undelivered paid one; not the expired, the transfer ones, the waiting one or the fully refunded one.
        Assert.Contains("Geri ödemesi doğabilecek kart siparişi: 2;", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("ödemesi bekleyen kart siparişi 1,", terminal.Output, StringComparison.Ordinal);
        Assert.Equal(before, await OrdersSnapshotAsync());
    }

    [Fact]
    public async Task Status_WithNoCardOrderAtAll_WritesTwoZeros()
    {
        FakeTerminal terminal = new() { IsInputRedirected = false };

        Assert.Equal(0, await RunAsync(terminal, "status"));

        Assert.Contains("Açık kart işi: 0 (ödemesi bekleyen kart siparişi 0, bitmemiş kart iadesi 0)", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("Geri ödemesi doğabilecek kart siparişi: 0;", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_WritesTheConditionalLines_TheSecretOnlyKeyAsNotMeasurable_AndAMissingSettingsFileKeyAsClosed()
    {
        FakeTerminal terminal = new() { IsInputRedirected = false };

        int exit = await RunAsync(terminal, "status");

        Assert.True(exit == 0, terminal.AllOutput);
        string output = terminal.Output;
        Assert.Contains($"  {ConditionalLine.CardPayment}: ölçülemez (eksik: {PaymentOptions.ProviderKeysKey};", output, StringComparison.Ordinal);
        Assert.Contains($"  {ConditionalLine.GoogleSignIn}: KAPALI (eksik: {GoogleOptions.ClientIdKey}, {GoogleOptions.ClientSecretKey})", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_AConditionalLineWhoseSettingsArePresent_IsWrittenOpen_AndNoValueIsWritten()
    {
        FakeTerminal terminal = new() { IsInputRedirected = false };
        const string firstSample = "first-sample-value";
        const string secondSample = "second-sample-value";
        const string thirdSample = "third-sample-value";

        int exit = await RunAsync(terminal, services =>
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddConfiguration(Configuration())
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PaymentOptions.ProviderKeysKey] = firstSample,
                    [GoogleOptions.ClientIdKey] = secondSample,
                    [GoogleOptions.ClientSecretKey] = thirdSample,
                })
                .Build();
            services.AddSingleton(configuration);
        }, "status");

        Assert.True(exit == 0, terminal.AllOutput);
        string output = terminal.Output;
        Assert.Contains($"  {ConditionalLine.CardPayment}: AÇIK", output, StringComparison.Ordinal);
        Assert.Contains($"  {ConditionalLine.GoogleSignIn}: AÇIK", output, StringComparison.Ordinal);
        Assert.DoesNotContain(firstSample, output, StringComparison.Ordinal);
        Assert.DoesNotContain(secondSample, output, StringComparison.Ordinal);
        Assert.DoesNotContain(thirdSample, output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheCommandHost_ResolvesEveryModule_SoACommandThatReachesAnyOfThemDoesNotBreak()
    {
        await using ServiceProvider provider = CommandHost.CreateServices(Configuration(), new FakeTerminal()).BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = false });
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;

        Assert.IsAssignableFrom<IAdministrationModule>(services.GetRequiredService<IAdministrationModule>());
        Assert.IsAssignableFrom<IAfterSalesModule>(services.GetRequiredService<IAfterSalesModule>());
        Assert.IsAssignableFrom<ICatalogModule>(services.GetRequiredService<ICatalogModule>());
        Assert.IsAssignableFrom<ICorporateContentModule>(services.GetRequiredService<ICorporateContentModule>());
        Assert.IsAssignableFrom<IMembershipModule>(services.GetRequiredService<IMembershipModule>());
        Assert.IsAssignableFrom<INotificationsModule>(services.GetRequiredService<INotificationsModule>());
        Assert.IsAssignableFrom<IOrderingModule>(services.GetRequiredService<IOrderingModule>());
        Assert.IsAssignableFrom<IPaymentModule>(services.GetRequiredService<IPaymentModule>());
        Assert.IsAssignableFrom<ISettingsModule>(services.GetRequiredService<ISettingsModule>());
    }

    // ---- KK8: the data conversions ----

    [Fact]
    public async Task Status_WritesTheRemainingRowsOfEveryRegisteredConversion()
    {
        FakeTerminal terminal = new() { IsInputRedirected = false };

        int exit = await RunAsync(terminal, services =>
        {
            services.AddSingleton<IDataConversion>(new FixedConversion(JobType.BounceQuery, 42));
            services.AddSingleton<IDataConversion>(new FixedConversion(JobType.ImageDerivative, 0));
        }, "status");

        Assert.Equal(0, exit);
        Assert.Contains($"  {JobType.BounceQuery}: kalan 42 satır", terminal.Output, StringComparison.Ordinal);
        Assert.Contains($"  {JobType.ImageDerivative}: kalan 0 satır", terminal.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Kayıtlı dönüşüm yok.", terminal.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_WithNoRegisteredConversion_SaysSo()
    {
        FakeTerminal terminal = new() { IsInputRedirected = false };

        int statusExit = await RunAsync(terminal, "status");
        Assert.True(statusExit == 0, terminal.AllOutput);

        Assert.Contains("Kayıtlı dönüşüm yok.", terminal.Output, StringComparison.Ordinal);
    }

    // ---- KK9: no trail, no notification, no job ----

    [Theory]
    [InlineData("end-sessions", true)]
    [InlineData("invalidate-password", true)]
    [InlineData("status", false)]
    public async Task TheCommand_WritesNothingToTheTrailTheSentRecordsOrTheJobTables(string command, bool takesAnEmail)
    {
        Guid a = await SeedAdminAsync(NameA, EmailA);
        await OpenSessionAsync(a);
        string before = await SnapshotAsync();

        int exit = await RunAsync(takesAnEmail ? Interactive(EmailA) : new FakeTerminal { IsInputRedirected = false }, command);

        Assert.Equal(0, exit);
        Assert.Equal(before, await SnapshotAsync());
        foreach (string table in new[] { "audit_logs", "dispatch_records", "jobs" })
        {
            Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
        }
    }

    // ---- helpers ----

    private static int CountOf(string text, string part)
    {
        int count = 0;
        for (int at = text.IndexOf(part, StringComparison.Ordinal); at >= 0; at = text.IndexOf(part, at + part.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    private static FakeTerminal Interactive(string email) => new FakeTerminal { IsInputRedirected = false }.Answer(email);

    private static string ReadShared(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private IConfiguration Configuration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = _collection.Dsn,
                [LoggingOptions.DirectoryKey] = _logDirectory,
            })
            .Build();
    }

    private Task<int> RunAsync(FakeTerminal terminal, params string[] arguments) => RunAsync(terminal, _ => { }, arguments);

    private async Task<int> RunAsync(FakeTerminal terminal, Action<IServiceCollection> adjust, params string[] arguments)
    {
        IServiceCollection services = CommandHost.CreateServices(Configuration(), terminal);

        // The disk condition reads the disk of the file volumes; the default directories are the container's, so the test points them at a directory that exists on any host.
        services.RemoveAll<FileStoreDirectories>();
        services.AddSingleton(new FileStoreDirectories(Path.GetTempPath(), Path.GetTempPath()));
        adjust(services);

        CommandDispatcher dispatcher = new(AdminCommandCatalog.All, terminal, () => services.BuildServiceProvider());
        return await dispatcher.ExecuteAsync(arguments, Ct);
    }

    // A scope of the command host's own services, for the set-up and the checks that go through the modules the way the web does.
    private async Task<T> InTransactionAsync<T>(Func<IServiceProvider, Task<T>> call)
    {
        await using ServiceProvider provider = CommandHost.CreateServices(Configuration(), new FakeTerminal()).BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        Held<T> held = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(async _ => new Held<T>(await call(scope.ServiceProvider)), Ct);
        return held.Value;
    }

    private sealed record Held<T>(T Value) : IOperationResult
    {
        public bool IsRejected => false;
    }

    private async Task<Guid> SeedAdminAsync(string name, string email)
    {
        Guid id = EntityIds.New();
        AdminCreationResult result = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().CreateAdminAsync(id, name, email, Password, Ct));
        Assert.True(result.IsCreated);
        return id;
    }

    private async Task<Guid> OpenSessionAsync(Guid adminId)
    {
        SessionOpening opening = await InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().SignInAsync(adminId, false, null, Ct));
        return opening.SessionId;
    }

    private Task<AdminCredentialCheckResult> CheckCredentialsAsync(string email, string password) =>
        InTransactionAsync(s => s.GetRequiredService<IAdministrationModule>().CheckCredentialsAsync(email, password, _ip, null, Ct));

    private Task<long> SessionCountAsync(Guid adminId) => ScalarAsync<long>($"SELECT count(*) FROM sessions WHERE admin_id = '{adminId}'");

    private async Task<List<Guid>> SessionIdsAsync(Guid adminId)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT id FROM sessions WHERE admin_id = '{adminId}' ORDER BY id", connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        List<Guid> ids = [];
        while (await reader.ReadAsync(Ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids;
    }

    private Task<string> AccountsAndSessionsAsync() =>
        ScalarAsync<string>("SELECT coalesce((SELECT string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id) FROM admins t), '') || '#' || coalesce((SELECT string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id) FROM sessions t), '')");

    private Task<string> OrdersSnapshotAsync() =>
        ScalarAsync<string>("SELECT coalesce((SELECT string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id) FROM orders t), '') || '#' || coalesce((SELECT string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id) FROM order_items t), '') || '#' || coalesce((SELECT string_agg(t.xmin::text || to_jsonb(t)::text, '|' ORDER BY t.id) FROM refunds t), '')");

    private async Task<string> SnapshotAsync()
    {
        List<string> parts = [];
        foreach (string table in _untouchedTables)
        {
            parts.Add(table + ":" + await ScalarAsync<string>($"SELECT coalesce(string_agg(t.xmin::text || to_jsonb(t)::text, '|'), '') FROM {table} t"));
        }

        return string.Join('\n', parts);
    }

    private async Task<Guid> SeedCardOrderAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, PaymentMethod method = PaymentMethod.Card, DateOnly? itemDeliveryDate = null)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, now, "metin", now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, now, "metin", now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, now, "metin", now).WithHosting();
        Order order = Order.Create($"2026-{n + 389000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "ali@example.com", 120m, 0m, now.AddHours(1), now.AddDays(-5));
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, now.AddDays(-5));
        await using (AppDbContext db = NewContext())
        {
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(form, contract, notice);
            db.Orders.Add(order);
            db.OrderItems.Add(item);
            await db.SaveChangesAsync(Ct);
        }

        if (orderStatus != OrderStatus.Placed || paymentStatus != PaymentStatus.Pending)
        {
            await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        }

        if (itemDeliveryDate is { } delivered)
        {
            await ExecuteAsync($"UPDATE order_items SET delivery_date = '{delivered:yyyy-MM-dd}', delivery_marked_at = now() WHERE id = '{item.Id}'");
        }

        return order.Id;
    }

    private async Task<Guid> SeedRefundAsync(Guid orderId, string transactionId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using AppDbContext db = NewContext();
        PaymentRecord payment = PaymentRecord.ForCardPayment(orderId, transactionId, null, now);
        db.PaymentRecords.Add(payment);
        Refund refund = Refund.ForLatePayment(orderId, payment.Id, 120m, now);
        db.Refunds.Add(refund);
        await db.SaveChangesAsync(Ct);
        return refund.Id;
    }

    private async Task AddRecordAsync(Guid refundId, RefundRecordState state)
    {
        RefundRecord record = RefundRecord.Sending(refundId, DateTimeOffset.UtcNow);
        record.State = state;
        if (state is RefundRecordState.Succeeded or RefundRecordState.Failed)
        {
            record.OutcomeAt = record.CreatedAt;
        }

        await using AppDbContext db = NewContext();
        db.RefundRecords.Add(record);
        await db.SaveChangesAsync(Ct);
    }

    private async Task<(int Exit, string Output)> RunProcessAsync(string[] arguments, string standardInput)
    {
        string application = Path.Combine(AppContext.BaseDirectory, "Shopfolio.Web.dll");
        ProcessStartInfo start = new("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = AppContext.BaseDirectory,
        };
        start.ArgumentList.Add(application);
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["Database__ConnectionString"] = _collection.Dsn;
        start.Environment[LoggingOptions.DirectoryKey.Replace(":", "__", StringComparison.Ordinal)] = _logDirectory;

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("The process did not start.");
        await process.StandardInput.WriteAsync(standardInput.AsMemory(), Ct);
        process.StandardInput.Close();

        Task<string> output = process.StandardOutput.ReadToEndAsync(Ct);
        Task<string> error = process.StandardError.ReadToEndAsync(Ct);
        try
        {
            await process.WaitForExitAsync(Ct).WaitAsync(TimeSpan.FromSeconds(90), Ct);
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return (process.ExitCode, await output + await error);
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

    private async Task<T?> NullableScalarAsync<T>(string sql)
        where T : class
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        object? value = await command.ExecuteScalarAsync(Ct);
        return value is null or DBNull ? null : (T)value;
    }

    private sealed class SpyWriter : IAuditLineWriter
    {
        private readonly Action<AuditLine> _onWrite;

        public SpyWriter(Action<AuditLine> onWrite)
        {
            _onWrite = onWrite;
        }

        public void Write(AuditLine line) => _onWrite(line);
    }

    // A conversion with a fixed remaining count: the status command reads it as it reads a real one (09 13.3 item 5).
    private sealed class FixedConversion : IDataConversion
    {
        private readonly long _remaining;

        public FixedConversion(JobType type, long remaining)
        {
            Type = type;
            _remaining = remaining;
        }

        public JobType Type { get; }

        public string RemainingPredicateSql => "false";

        public Task<int> ConvertBatchAsync(int batchSize, CancellationToken ct) => Task.FromResult(0);

        public Task<long> CountRemainingAsync(CancellationToken ct) => Task.FromResult(_remaining);
    }
}
