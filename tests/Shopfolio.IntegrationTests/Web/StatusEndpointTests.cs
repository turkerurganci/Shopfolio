using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Notifications;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// UC-82, the status endpoint (T314; 07 4.33.2; 05 9.4.1-9.4.18): 200 when no warning condition is open, 503 when one is, an empty body and
/// <c>no-store</c> either way, the open condition never named. The database conditions run against the collection database; the readings of the disk,
/// the log shipping and the file backup's status file are fakes the test sets, and the clock is a manual one so that no test waits. The real readings
/// have their own tests below.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class StatusEndpointTests
{
    private static readonly DateTimeOffset _start = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private static readonly Uri _statusUri = new(StatusEndpoint.Path, UriKind.Relative);

    private static readonly Uri _livenessUri = new(LivenessEndpoint.Path, UriKind.Relative);

    private readonly CollectionDatabaseFixture _database;

    public StatusEndpointTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>The names of the situations a test can start from; each opens exactly one condition (or none).</summary>
    public static TheoryData<string> OpeningScenarios =>
    [
        "disk-full",
        "log-files-waiting",
        "log-shipping-stalled",
        "full-backup-old",
        "file-backup-old",
        "heartbeat-old",
        "channel-warning",
        "errors-high",
        "payment-series",
        "certificate-near-end",
        "job-overdue",
        "job-failed",
        "nightly-old",
    ];

    // --- KK1, KK2: the statuses and the answer's shape ---

    [Fact]
    public async Task Get_WithNoOpenCondition_Is200_EmptyBody_NoStore_AndNeedsNoSignatureOrSession_KK1()
    {
        await using Scenario scenario = await OpenAsync();

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Theory]
    [MemberData(nameof(OpeningScenarios))]
    public async Task Get_WithOneConditionOpen_Is503_EmptyBody_NoStore_AndNamesNothing_KK1_KK2(string scenarioName)
    {
        await using Scenario scenario = await OpenAsync(scenarioName);

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(0, response.Content.Headers.ContentLength ?? 0);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.Single(await scenario.OpenNamesAsync());
    }

    [Fact]
    public async Task Get_WithSeveralConditionsOpen_Is503_AndTheAnswerIsTheSameAsWithOne_KK2()
    {
        await using Scenario scenario = await OpenAsync("disk-full");
        scenario.Disk.Fraction = 0.99;
        scenario.Certificate.Record(_start.AddDays(1));

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
        Assert.True((await scenario.OpenNamesAsync()).Count > 1);
    }

    // --- KK3, KK4: the boundaries of the disk, the log shipping and the two backups ---

    [Fact]
    public async Task TheDiskThreshold_IsTheOneInTheSettings_KK3_KK10()
    {
        Dictionary<string, string> looser = new() { ["StatusThreshold:DiskUsageFraction"] = "0.95" };
        await using Scenario scenario = await OpenAsync(settings: looser);
        scenario.Disk.Fraction = 0.90;

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("full-backup-old", "FullBackup")]
    [InlineData("file-backup-old", "FileBackup")]
    public async Task EachBackup_OpensTheStatusAlone_TheOtherOnesFreshnessDoesNotCloseIt_KK4(string scenarioName, string expectedCondition)
    {
        // The full backup's moment is the one in the database that only the outside repository's success writes (06 4.67): a local copy has no row.
        await using Scenario scenario = await OpenAsync(scenarioName);

        Assert.Equal([expectedCondition], await scenario.OpenNamesAsync());
    }

    // --- KK6: the channel warning closes at the first success ---

    [Fact]
    public async Task TheChannelWarning_ClosesWhenTheCountIsReset_KK6()
    {
        await using Scenario scenario = await OpenAsync("channel-warning");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);

        await scenario.ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = 0, first_channel_error_at = NULL");

        Assert.Equal(HttpStatusCode.OK, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);
    }

    // --- KK7: the unexpected errors, from the real catching layer ---

    [Fact]
    public async Task NineFailedRequestsAreClosed_TheTenthOpens_AndANotFoundIsNotCounted_KK7()
    {
        await using Scenario scenario = await OpenAsync();
        const string failing = "/odeme/hata-ornegi?fail=handler";
        int threshold = new Infrastructure.InstallationSettings.StatusThresholdOptions().UnexpectedErrorCount;

        for (int i = 0; i < threshold - 1; i++)
        {
            using HttpResponseMessage failed = await scenario.SampleClient.GetAsync(new Uri(failing, UriKind.Relative), Ct);
            Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
        }

        using HttpResponseMessage notFound = await scenario.SampleClient.GetAsync(new Uri("/olmayan-sayfa-t314", UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);

        using HttpResponseMessage last = await scenario.SampleClient.GetAsync(new Uri(failing, UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.InternalServerError, last.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);

        scenario.Time.Advance(TimeSpan.FromMinutes(new Infrastructure.InstallationSettings.StatusThresholdOptions().UnexpectedErrorWindowMinutes + 1));
        await scenario.KeepSourcesFreshAsync();
        Assert.Equal(HttpStatusCode.OK, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);
    }

    // --- KK8: the payment start's series ends at the first success; the certificate ---

    [Fact]
    public async Task ThePaymentSeries_IsClosedWhileYoung_OpensWhenOldEnough_AndTheFirstSuccessEndsIt_KK8()
    {
        await using Scenario scenario = await OpenAsync();
        Infrastructure.InstallationSettings.StatusThresholdOptions thresholds = new();

        for (int i = 0; i < thresholds.PaymentStartFailureCount; i++)
        {
            scenario.Series.RecordFailure();
        }

        Assert.Equal(HttpStatusCode.OK, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);

        scenario.Time.Advance(TimeSpan.FromMinutes(thresholds.PaymentStartFailureAgeMinutes));
        await scenario.KeepSourcesFreshAsync();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);

        scenario.Series.RecordSuccess();
        Assert.Equal(HttpStatusCode.OK, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);
    }

    // --- KK9: the job queue and the nightly job ---

    [Fact]
    public async Task AJobNotYetDue_AndAFreshDueJob_AreClosed_KK9()
    {
        await using Scenario scenario = await OpenAsync();
        Infrastructure.InstallationSettings.StatusThresholdOptions thresholds = new();
        await scenario.AddJobAsync(JobState.Pending, nextAttemptAt: _start.AddDays(2), createdAt: _start.AddDays(-2));
        await scenario.AddJobAsync(JobState.Completed, nextAttemptAt: null, createdAt: _start.AddDays(-2), type: JobType.CardRefund, related: Guid.NewGuid());
        await scenario.AddJobAsync(JobState.Pending, nextAttemptAt: null, createdAt: _start - TimeSpan.FromMinutes(thresholds.JobQueueAgeMinutes), type: JobType.CardRefund, related: Guid.NewGuid());

        Assert.Equal(HttpStatusCode.OK, (await scenario.Client.GetAsync(_statusUri, Ct)).StatusCode);
    }

    // --- KK10: the liveness endpoint does not carry the conditions; an unreadable source opens the status ---

    [Theory]
    [MemberData(nameof(OpeningScenarios))]
    public async Task TheLivenessEndpoint_StaysAt200_WhateverConditionIsOpen_KK10(string scenarioName)
    {
        await using Scenario scenario = await OpenAsync(scenarioName);

        using HttpResponseMessage response = await scenario.Client.GetAsync(_livenessUri, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ASourceThatCannotBeRead_IsAnOpenCondition_503_KK10()
    {
        await using Scenario scenario = await OpenAsync();
        scenario.FileBackup.Fails = true;

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(["FileBackup"], await scenario.OpenNamesAsync());
    }

    [Fact]
    public async Task ADatabaseConditionThatCannotBeRead_IsAnOpenCondition_503_KK10()
    {
        await using Scenario scenario = await OpenAsync();
        await scenario.ExecuteAsync("TRUNCATE heartbeat_records");

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("Heartbeat", await scenario.OpenNamesAsync());
    }

    // --- The real readings ---

    [Fact]
    public async Task ThePostgresLogShippingReader_ReadsTheArchiverStatistics_OfARealDatabase()
    {
        await using NpgsqlDataSource source = NpgsqlDataSource.Create(_database.Dsn);
        PostgresLogShippingReader reader = new(source);

        LogShippingState state = await reader.ReadAsync(Ct);

        // The test database archives nothing: no success, no failure and no waiting file.
        Assert.Null(state.LastArchivedAt);
        Assert.Null(state.LastFailedAt);
        Assert.Equal(0, state.WaitingFiles);
    }

    [Fact]
    public async Task AnUnknownFailureOfAReading_IsA500WithoutABody_TheCatchingLayerCountsIt_AndItIsNeverReadAsOpen()
    {
        await using Scenario scenario = await OpenAsync(throwingCondition: true);
        UnexpectedErrorCounter counter = scenario.Services.GetRequiredService<UnexpectedErrorCounter>();
        int before = counter.CountWithin(TimeSpan.FromMinutes(15));

        using HttpResponseMessage response = await scenario.Client.GetAsync(_statusUri, Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal(before + 1, counter.CountWithin(TimeSpan.FromMinutes(15)));
    }

    [Fact]
    public async Task TheFileBackupReader_ReadsTheStatusFileOfTheBackupTool_AndAMissingOrDamagedFileThrows()
    {
        string directory = Path.Combine(Path.GetTempPath(), "shopfolio-t314-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonFileBackupStatusReader reader = new(directory);
            await Assert.ThrowsAnyAsync<IOException>(() => reader.ReadLastSuccessAsync(Ct));

            string file = Path.Combine(directory, JsonFileBackupStatusReader.FileName);
            await File.WriteAllTextAsync(file, """{"result":"success","lastSuccessAt":"2026-11-06T03:10:00Z"}""", Ct);
            Assert.Equal(new DateTimeOffset(2026, 11, 6, 3, 10, 0, TimeSpan.Zero), await reader.ReadLastSuccessAsync(Ct));

            await File.WriteAllTextAsync(file, "{ damaged", Ct);
            await Assert.ThrowsAnyAsync<System.Text.Json.JsonException>(() => reader.ReadLastSuccessAsync(Ct));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void TheDiskReader_MeasuresTheDiskOfTheNearestExistingDirectory()
    {
        string missing = Path.Combine(Path.GetTempPath(), "shopfolio-t314-missing-" + Guid.NewGuid().ToString("N"), "images");
        DriveDiskUsageReader reader = new(new Infrastructure.Files.FileStoreDirectories(missing, missing));

        double used = reader.UsedFraction();

        Assert.InRange(used, 0.0, 1.0);
    }

    // --- The scenario ---

    private async Task<Scenario> OpenAsync(string? name = null, Dictionary<string, string>? settings = null, bool throwingCondition = false)
    {
        Scenario scenario = new(_database, settings) { ThrowingCondition = throwingCondition };
        await scenario.StartAsync(name);
        return scenario;
    }

    private sealed class ThrowingStatusCondition : IStatusCondition
    {
        public string Name => "Throwing";

        public Task<bool> IsOpenAsync(CancellationToken ct) => throw new NotImplementedException();
    }

    private sealed class FakeDisk : IDiskUsageReader
    {
        public double Fraction { get; set; } = 0.40;

        public double UsedFraction() => Fraction;
    }

    private sealed class FakeLogShipping : ILogShippingReader
    {
        public LogShippingState State { get; set; } = new(null, null, 0);

        public Task<LogShippingState> ReadAsync(CancellationToken ct) => Task.FromResult(State);
    }

    private sealed class FakeFileBackup : IFileBackupStatusReader
    {
        public DateTimeOffset? LastSuccess { get; set; }

        public bool Fails { get; set; }

        public Task<DateTimeOffset?> ReadLastSuccessAsync(CancellationToken ct) =>
            Fails ? throw new IOException("the status file is not readable") : Task.FromResult(LastSuccess);
    }

    private sealed class Scenario : IAsyncDisposable
    {
        private readonly CollectionDatabaseFixture _database;
        private readonly ShopfolioWebApplicationFactory _baseFactory;
        private WebApplicationFactory<Program>? _factory;

        public Scenario(CollectionDatabaseFixture database, Dictionary<string, string>? settings)
        {
            _database = database;
            _baseFactory = new ShopfolioWebApplicationFactory(database.Dsn, settings, withJobRunner: false);
            Time = new ManualTimeProvider(_start);
        }

        public ManualTimeProvider Time { get; }

        /// <summary>Registers one more condition whose reading throws a type the evaluator does not know (D-139).</summary>
        public bool ThrowingCondition { get; init; }

        public IServiceProvider Services => _factory!.Services;

        public FakeDisk Disk { get; } = new();

        public FakeLogShipping LogShipping { get; } = new();

        public FakeFileBackup FileBackup { get; } = new();

        public HttpClient Client { get; private set; } = null!;

        /// <summary>The client of the application with the test-only sample pages, whose handler throws on demand.</summary>
        public HttpClient SampleClient { get; private set; } = null!;

        public CertificateExpiryMemory Certificate => _factory!.Services.GetRequiredService<CertificateExpiryMemory>();

        public PaymentStartFailureSeries Series => _factory!.Services.GetRequiredService<PaymentStartFailureSeries>();

        public async Task StartAsync(string? name)
        {
            await SeedHealthyAsync();
            await ApplyDatabaseScenarioAsync(name);

            _factory = _baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Time);
                services.RemoveAll<IDiskUsageReader>();
                services.AddSingleton<IDiskUsageReader>(Disk);
                services.RemoveAll<ILogShippingReader>();
                services.AddSingleton<ILogShippingReader>(LogShipping);
                services.RemoveAll<IFileBackupStatusReader>();
                services.AddSingleton<IFileBackupStatusReader>(FileBackup);
                services.AddRazorPages().AddApplicationPart(typeof(ErrorSampleModel).Assembly);
                if (ThrowingCondition)
                {
                    services.AddSingleton<IStatusCondition, ThrowingStatusCondition>();
                }
            }));
            Client = ForgeryTokenHandler.CreateClient(_factory);
            SampleClient = Client;

            // The healthy start: a certificate with plenty of days left, a file backup of an hour ago, the rest from the database.
            Certificate.Record(_start.AddDays(90));
            FileBackup.LastSuccess = _start.AddHours(-1);
            await ApplyMemoryScenarioAsync(name);
        }

        /// <summary>Moves the freshness of the sources forward with the clock, as a running installation keeps them fresh.</summary>
        public async Task KeepSourcesFreshAsync()
        {
            FileBackup.LastSuccess = Time.GetUtcNow().AddHours(-1);
            await ExecuteAsync($"UPDATE heartbeat_records SET last_heartbeat_at = '{Time.GetUtcNow():O}', nightly_job_last_succeeded_at = '{Time.GetUtcNow():O}'");
            Certificate.Record(Time.GetUtcNow().AddDays(90));
        }

        public async Task<IReadOnlyList<string>> OpenNamesAsync()
        {
            await using AsyncServiceScope scope = _factory!.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<StatusEvaluator>().OpenConditionsAsync(Ct);
        }

        public async Task ExecuteAsync(string sql)
        {
            await using NpgsqlConnection connection = new(_database.Dsn);
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand command = new(sql, connection);
            await command.ExecuteNonQueryAsync(Ct);
        }

        public async Task AddJobAsync(JobState state, DateTimeOffset? nextAttemptAt, DateTimeOffset createdAt, JobType type = JobType.BounceQuery, Guid? related = null)
        {
            Job job = Job.Enqueue(type, type == JobType.BounceQuery ? null : related, nextAttemptAt, createdAt);
            job.JobState = state;
            job.CompletedAt = state == JobState.Completed ? createdAt : null;
            await using AppDbContext db = NewContext();
            db.Jobs.Add(job);
            await db.SaveChangesAsync(Ct);
        }

        public async ValueTask DisposeAsync()
        {
            Client?.Dispose();
            if (_factory is not null)
            {
                await _factory.DisposeAsync();
            }

            await _baseFactory.DisposeAsync();
        }

        private AppDbContext NewContext()
        {
            DbContextOptionsBuilder<AppDbContext> builder = new();
            AppDbContext.Configure(builder, _database.Dsn);
            return new AppDbContext(builder.Options);
        }

        private async Task SeedHealthyAsync()
        {
            await ExecuteAsync("TRUNCATE jobs, heartbeat_records, backup_status_records, channel_state_records CASCADE");
            await using AppDbContext db = NewContext();
            HeartbeatRecord heartbeat = HeartbeatRecord.Create(_start.AddDays(-30));
            heartbeat.LastHeartbeatAt = _start.AddSeconds(-30);
            heartbeat.NightlyJobLastSucceededAt = _start.AddHours(-9);
            heartbeat.NightlyJobLastAttemptedAt = _start.AddHours(-9);
            db.HeartbeatRecords.Add(heartbeat);
            BackupStatusRecord backup = BackupStatusRecord.Create(_start.AddDays(-30));
            backup.LastSucceededAt = _start.AddHours(-9);
            db.BackupStatusRecords.Add(backup);
            db.ChannelStateRecords.Add(ChannelStateRecord.Create(_start.AddDays(-30)));
            await db.SaveChangesAsync(Ct);
        }

        private async Task ApplyDatabaseScenarioAsync(string? name)
        {
            Infrastructure.InstallationSettings.StatusThresholdOptions t = new();
            switch (name)
            {
                case "full-backup-old":
                    await ExecuteAsync($"UPDATE backup_status_records SET last_succeeded_at = '{(_start - TimeSpan.FromHours(t.BackupAgeHours + 1)):O}'");
                    break;
                case "heartbeat-old":
                    await ExecuteAsync($"UPDATE heartbeat_records SET last_heartbeat_at = '{(_start - TimeSpan.FromMinutes(t.HeartbeatAgeMinutes + 1)):O}'");
                    break;
                case "nightly-old":
                    await ExecuteAsync($"UPDATE heartbeat_records SET nightly_job_last_succeeded_at = '{(_start - TimeSpan.FromHours(t.NightlyJobAgeHours + 1)):O}'");
                    break;
                case "channel-warning":
                    await ExecuteAsync(
                        $"UPDATE channel_state_records SET consecutive_channel_error_count = {ChannelWarningCondition.ErrorsInARow}, " +
                        $"first_channel_error_at = '{(_start - TimeSpan.FromMinutes(ChannelWarningCondition.FirstErrorAgeMinutes)):O}'");
                    break;
                case "job-overdue":
                    await AddJobAsync(JobState.Pending, nextAttemptAt: null, createdAt: _start - TimeSpan.FromMinutes(t.JobQueueAgeMinutes + 1));
                    break;
                case "job-failed":
                    await AddJobAsync(JobState.Failed, nextAttemptAt: null, createdAt: _start.AddHours(-1));
                    break;
                default:
                    break;
            }
        }

        private async Task ApplyMemoryScenarioAsync(string? name)
        {
            Infrastructure.InstallationSettings.StatusThresholdOptions t = new();
            switch (name)
            {
                case "disk-full":
                    Disk.Fraction = t.DiskUsageFraction;
                    break;
                case "log-files-waiting":
                    LogShipping.State = new LogShippingState(_start, null, t.LogFilesWaitingMax + 1);
                    break;
                case "log-shipping-stalled":
                    LogShipping.State = new LogShippingState(_start - TimeSpan.FromMinutes(t.LogShippingStallMinutes), _start, 1);
                    break;
                case "file-backup-old":
                    FileBackup.LastSuccess = _start - TimeSpan.FromHours(t.BackupAgeHours + 1);
                    break;
                case "errors-high":
                    UnexpectedErrorCounter counter = _factory!.Services.GetRequiredService<UnexpectedErrorCounter>();
                    for (int i = 0; i < t.UnexpectedErrorCount; i++)
                    {
                        counter.Record();
                    }

                    break;
                case "payment-series":
                    for (int i = 0; i < t.PaymentStartFailureCount; i++)
                    {
                        Series.RecordFailure();
                    }

                    Time.Advance(TimeSpan.FromMinutes(t.PaymentStartFailureAgeMinutes));
                    await KeepSourcesFreshAsync();
                    break;
                case "certificate-near-end":
                    Certificate.Record(_start.AddDays(t.CertificateDaysLeft).AddSeconds(-1));
                    break;
                default:
                    break;
            }
        }

        private static CancellationToken Ct => TestContext.Current.CancellationToken;
    }
}
