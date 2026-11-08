using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>The collection of the scheduler tests (T221): one database copied from the template.</summary>
[CollectionDefinition(SchedulerDefinition.Name)]
public sealed class SchedulerDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Scheduler";
}

/// <summary>A source of due records whose records and behavior are the test's script; every call is counted.</summary>
internal sealed class ScriptedSource : IDueRecordSource
{
    private readonly object _gate = new();
    private readonly List<DueRecord> _due = [];
    private readonly List<Guid> _processed = [];
    private int _findCalls;

    public ScriptedSource(string name, bool waitsForFreshHeartbeat = false)
    {
        Name = name;
        WaitsForFreshHeartbeat = waitsForFreshHeartbeat;
    }

    public string Name { get; }

    public bool WaitsForFreshHeartbeat { get; }

    /// <summary>When set, a look for due records waits for it: a scan that is slow.</summary>
    public TaskCompletionSource? BlockFind { get; set; }

    /// <summary>When set, processing the record runs it first; an exception from it fails the record.</summary>
    public Func<Guid, Task>? OnProcess { get; set; }

    public int FindCalls => Volatile.Read(ref _findCalls);

    public IReadOnlyList<Guid> Processed
    {
        get
        {
            lock (_gate)
            {
                return [.. _processed];
            }
        }
    }

    public void Add(Guid id, DateTimeOffset dueAt)
    {
        lock (_gate)
        {
            _due.Add(new DueRecord(id, dueAt));
        }
    }

    public async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        Interlocked.Increment(ref _findCalls);
        if (BlockFind is { } block)
        {
            await block.Task.WaitAsync(ct);
        }

        lock (_gate)
        {
            return [.. _due.Where(d => d.DueAt <= now).OrderBy(d => d.DueAt)];
        }
    }

    public async Task ProcessAsync(IServiceProvider services, Guid recordId, CancellationToken ct)
    {
        if (OnProcess is { } script)
        {
            await script(recordId);
        }

        lock (_gate)
        {
            _processed.Add(recordId);
            _due.RemoveAll(d => d.Id == recordId);
        }
    }
}

/// <summary>A step of the nightly job whose behavior is the test's script.</summary>
internal sealed class ScriptedStep : INightlyStep
{
    private int _runs;

    public ScriptedStep(string name = "Scripted")
    {
        Name = name;
    }

    public string Name { get; }

    public int Runs => Volatile.Read(ref _runs);

    public Func<Task>? Script { get; set; }

    public async Task RunAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        Interlocked.Increment(ref _runs);
        if (Script is { } script)
        {
            await script();
        }
    }
}

/// <summary>A view of a fake clock that counts the timers made through it and not yet disposed; the time and the timers are the inner clock's.</summary>
internal sealed class CountingTimeProvider : TimeProvider
{
    private readonly TimeProvider _inner;
    private int _openTimers;

    public CountingTimeProvider(TimeProvider inner) => _inner = inner;

    public int OpenTimers => Volatile.Read(ref _openTimers);

    public override DateTimeOffset GetUtcNow() => _inner.GetUtcNow();

    public override long GetTimestamp() => _inner.GetTimestamp();

    public override long TimestampFrequency => _inner.TimestampFrequency;

    public override TimeZoneInfo LocalTimeZone => _inner.LocalTimeZone;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        Interlocked.Increment(ref _openTimers);
        return new CountedTimer(_inner.CreateTimer(callback, state, dueTime, period), this);
    }

    private sealed class CountedTimer : ITimer
    {
        private readonly ITimer _timer;
        private readonly CountingTimeProvider _owner;
        private int _disposed;

        public CountedTimer(ITimer timer, CountingTimeProvider owner)
        {
            _timer = timer;
            _owner = owner;
        }

        public bool Change(TimeSpan dueTime, TimeSpan period) => _timer.Change(dueTime, period);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                Interlocked.Decrement(ref _owner._openTimers);
            }

            _timer.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}

/// <summary>The answers of the site's liveness endpoint, set by the test; every request is counted.</summary>
internal sealed class FakeLivenessHandler : HttpMessageHandler
{
    private int _requests;

    public volatile HttpStatusCode Status = HttpStatusCode.ServiceUnavailable;

    /// <summary>When true the request fails the way a refused connection does (<see cref="HttpRequestException"/>).</summary>
    public volatile bool Refuse;

    /// <summary>When true the request never answers; only the caller's limit ends it.</summary>
    public volatile bool Hang;

    public int Requests => Volatile.Read(ref _requests);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requests);
        if (Refuse)
        {
            throw new HttpRequestException("the connection was refused");
        }

        if (Hang)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        return new HttpResponseMessage(Status);
    }
}

/// <summary>
/// An order source of the test: an order is due when its payment window has ended and the marker (the reminder field) is empty. Processing is
/// the base's: lock, load again, ask again, apply. The apply step records whether a second connection could take the order's row lock while
/// the first holds it, and writes the marker.
/// </summary>
internal sealed class TestOrderSource : ScheduledOrderTransition
{
    private int _applied;
    private bool? _secondLockRefused;

    public override string Name => "TestOrders";

    public override bool WaitsForFreshHeartbeat => false;

    public int Applied => Volatile.Read(ref _applied);

    public bool? SecondLockRefused => _secondLockRefused;

    public override async Task<IReadOnlyList<DueRecord>> FindDueAsync(IServiceProvider services, DateTimeOffset now, CancellationToken ct)
    {
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        return await db.Orders.AsNoTracking()
            .Where(o => o.PaymentReminderAt == null && o.PaymentWindowEndsAt <= now)
            .OrderBy(o => o.PaymentWindowEndsAt)
            .Select(o => new DueRecord(o.Id, o.PaymentWindowEndsAt))
            .ToListAsync(ct);
    }

    protected override bool IsDue(Order order, DateTimeOffset now) => order.PaymentReminderAt is null && order.PaymentWindowEndsAt <= now;

    protected override async Task Apply(IServiceProvider services, Order order, DateTimeOffset now, CancellationToken ct)
    {
        NpgsqlDataSource dataSource = services.GetRequiredService<NpgsqlDataSource>();
        await using (NpgsqlConnection other = await dataSource.OpenConnectionAsync(ct))
        await using (NpgsqlTransaction transaction = await other.BeginTransactionAsync(ct))
        {
            await using NpgsqlCommand command = new("SELECT id FROM orders WHERE id = $1 FOR UPDATE NOWAIT", other, transaction);
            command.Parameters.Add(new NpgsqlParameter { Value = order.Id });
            try
            {
                await command.ExecuteScalarAsync(ct);
                _secondLockRefused = false;
            }
            catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.LockNotAvailable)
            {
                _secondLockRefused = true;
            }
        }

        order.PaymentReminderAt = now;
        Interlocked.Increment(ref _applied);
    }
}

/// <summary>
/// The scheduler as the application wires it, on the collection's database and a fake clock and a fake liveness endpoint: the transaction
/// runner, the Ordering module's outage effect, the scan, the nightly job and the heartbeat. A test drives one scan, one nightly check or one
/// beat, or the whole service; no test waits for real time to pass in the scheduler - the clock is moved.
/// </summary>
internal sealed class SchedulerHarness : IAsyncDisposable
{
    /// <summary>09:00 UTC, noon in Turkey: after the nightly hour of the day.</summary>
    public static readonly DateTimeOffset Start = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private readonly ServiceProvider _provider;
    private CountingTimeProvider? _pulseClock;

    public SchedulerHarness(string dsn, IEnumerable<IDueRecordSource>? sources = null, IEnumerable<INightlyStep>? steps = null)
    {
        Dsn = dsn;
        DataSource = NpgsqlDataSource.Create(dsn);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton(DataSource);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<OrderLocks>();
        services.AddScoped<DownloadCounters>();
        services.AddOutageEffect<TransferWindowDeferral>();
        services.AddLogging(builder => builder.AddProvider(Logs));
        services.AddHttpClient(HeartbeatPulse.ClientName, client => client.BaseAddress = new Uri("https://localhost/"))
            .ConfigurePrimaryHttpMessageHandler(() => Liveness);
        services.AddSingleton(sp => new HeartbeatPulse(sp.GetRequiredService<IHttpClientFactory>(), sp.GetRequiredService<IServiceScopeFactory>(), PulseClock));
        services.AddSingleton<SchedulerScan>();
        services.AddSingleton<NightlyJobRunner>();
        services.AddSingleton<INightlyStep, JobQueueCleanupStep>();
        foreach (IDueRecordSource source in sources ?? [])
        {
            services.AddSingleton(source);
        }

        foreach (INightlyStep step in steps ?? [])
        {
            services.AddSingleton(step);
        }

        _provider = services.BuildServiceProvider();
    }

    public string Dsn { get; }

    public NpgsqlDataSource DataSource { get; }

    public ManualTimeProvider Time { get; } = new(Start);

    /// <summary>
    /// The clock of the heartbeat request: the same fake clock, but its timers (the request's limit) are counted apart, so that a request
    /// still in flight is not mistaken for the loop's wait.
    /// </summary>
    public CountingTimeProvider PulseClock => _pulseClock ??= new CountingTimeProvider(Time);

    public FakeLivenessHandler Liveness { get; } = new();

    public CollectingLoggerProvider Logs { get; } = new();

    public IServiceProvider Services => _provider;

    public SchedulerScan Scan => _provider.GetRequiredService<SchedulerScan>();

    public NightlyJobRunner Nightly => _provider.GetRequiredService<NightlyJobRunner>();

    public HeartbeatPulse Pulse => _provider.GetRequiredService<HeartbeatPulse>();

    public SchedulerService NewService() => ActivatorUtilities.CreateInstance<SchedulerService>(_provider);

    public async Task<HeartbeatRecord> HeartbeatAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().HeartbeatRecords.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
    }

    public async Task SetHeartbeatAsync(DateTimeOffset? lastBeat, DateTimeOffset? succeeded = null, DateTimeOffset? attempted = null)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.HeartbeatRecords.ExecuteUpdateAsync(
            s => s.SetProperty(h => h.LastHeartbeatAt, lastBeat)
                .SetProperty(h => h.NightlyJobLastSucceededAt, succeeded)
                .SetProperty(h => h.NightlyJobLastAttemptedAt, attempted),
            TestContext.Current.CancellationToken);
    }

    public async Task<IReadOnlyList<OutageRecord>> OutagesAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().OutageRecords.AsNoTracking().OrderBy(o => o.StartedAt)
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    public async Task<Order> OrderAsync(Guid id)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Orders.AsNoTracking().SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken);
    }

    /// <summary>Waits until the code under test is waiting on the fake clock for at least <paramref name="count"/> timers (the heartbeat request's limit not counted).</summary>
    public Task WaitForTimersAsync(int count) =>
        JobQueueHarness.EventuallyAsync(() => Task.FromResult(Time.ActiveTimerCount - PulseClock.OpenTimers >= count), $"{count} waits on the fake clock");

    /// <summary>Waits until the heartbeat request has its limit timer on the fake clock.</summary>
    public Task WaitForPulseLimitAsync() =>
        JobQueueHarness.EventuallyAsync(() => Task.FromResult(PulseClock.OpenTimers >= 1), "the heartbeat request waits for its limit on the fake clock");

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await DataSource.DisposeAsync();
    }
}

/// <summary>The shared parts of the scheduler test classes: the collection's database, raw statements, a clean start with one heartbeat row.</summary>
public abstract class SchedulerTestBase : IAsyncLifetime
{
    private static int _counter;

    protected SchedulerTestBase(CollectionDatabaseFixture collection)
    {
        Collection = collection;
    }

    protected CollectionDatabaseFixture Collection { get; }

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    protected static DateTimeOffset Now => SchedulerHarness.Start;

    public virtual async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE jobs, heartbeat_records, outage_records, cancellations, internal_notes, transition_records, order_items, orders, legal_text_versions, provinces CASCADE");
        await using AppDbContext db = NewContext();
        db.HeartbeatRecords.Add(HeartbeatRecord.Create(Now));
        await db.SaveChangesAsync(Ct);
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    internal SchedulerHarness NewHarness(IEnumerable<IDueRecordSource>? sources = null, IEnumerable<INightlyStep>? steps = null)
    {
        return new SchedulerHarness(Collection.Dsn, sources, steps);
    }

    internal AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, Collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    protected async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(Collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    /// <summary>A bank transfer order in the state the deferral looks for: placed, payment pending, the window ending at <paramref name="windowEndsAt"/>.</summary>
    internal async Task<Order> SeedTransferOrderAsync(DateTimeOffset windowEndsAt, DateTimeOffset? reminderAt = null)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"Il {n}", Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "text", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "text", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "text", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.BankTransfer, true, "Ali@Example.com", 120m, 0m, windowEndsAt, Now);
        order.BankTransferIban = "TR330006100519786457841326";
        order.PaymentReminderAt = reminderAt;
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Cankaya";
        order.BillingAddressLine = "Ataturk Bulvari 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Ornek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Cankaya";
        order.CompanyAddressLine = "Kizilay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("pre-information", "distance sales");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Product", 1, 100m, 20m, 16.67m, Now);
        await using AppDbContext db = NewContext();
        db.Provinces.Add(province);
        db.LegalTextVersions.AddRange(form, contract, notice);
        db.Orders.Add(order);
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        return order;
    }
}
