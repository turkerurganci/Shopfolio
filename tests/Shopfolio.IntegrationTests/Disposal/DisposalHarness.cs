using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Disposal;

/// <summary>The collection of the disposal tests (T201): one database copied from the template, nothing shared with another collection.</summary>
[CollectionDefinition(DisposalDefinition.Name)]
public sealed class DisposalDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Disposal";
}

/// <summary>
/// The disposal as the application wires it, on the collection's database and a fake clock: the transaction runner, the nightly job with the queue's
/// clean-up and the disposal step, and the log directories of the test. A test seeds rows relative to <see cref="Now"/>, moves the clock and runs
/// the step or the whole nightly job; no test waits for real time and none touches a directory but its own.
/// </summary>
internal sealed class DisposalHarness : IAsyncDisposable
{
    /// <summary>06:00 UTC, 09:00 in Turkey on a day in November: after the nightly hour.</summary>
    public static readonly DateTimeOffset Start = new(2026, 11, 5, 6, 0, 0, TimeSpan.Zero);

    private readonly ServiceProvider _provider;

    public DisposalHarness(string dsn, LogDirectories directories)
    {
        Dsn = dsn;
        Files = new InMemoryFileStore(Time);
        DataSource = NpgsqlDataSource.Create(dsn);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton(DataSource);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddSingleton<NightlyJobRunner>();
        services.AddSingleton<INightlyStep, JobQueueCleanupStep>();
        services.AddDisposal(directories);
        services.AddDisposalPart<GuestCartDisposalPart>();
        services.AddDisposalPart<AdminInvitationDisposalPart>();

        // The order's part (T301) and the module surfaces it reaches: the order row lock, the payment and after-sales modules.
        services.AddScoped<OrderLocks>();
        services.AddScoped<DownloadCounters>();
        services.AddScoped<OrderPaymentArms>();
        services.AddScoped<OrderAccess>();
        services.AddScoped<AdvisoryLocks>();
        services.AddCartServices();
        services.AddScoped<IOrderingModule, OrderingModule>();
        services.AddPaymentModule();
        services.AddAfterSalesModule();
        services.AddSettingsDependencies();
        services.AddDisposalPart<OrderDisposalPart>();

        // The other time-bound records' parts (T302): the dispatch record, the contact request and the file store they sweep; the digital files' parts come with the Catalog module's registration (AddCartServices).
        services.AddSingleton<IFileStore>(Files);
        services.AddDisposalPart<DispatchRecordDisposalPart>();
        services.AddDisposalPart<ContactRequestDisposalPart>();
        _provider = services.BuildServiceProvider();
    }

    public string Dsn { get; }

    public NpgsqlDataSource DataSource { get; }

    public ManualTimeProvider Time { get; } = new(Start);

    /// <summary>The file store the parts sweep: the in-memory fake, aged with the fake clock.</summary>
    public InMemoryFileStore Files { get; }

    public DateTimeOffset Now => Time.GetUtcNow();

    public NightlyJobRunner Nightly => _provider.GetRequiredService<NightlyJobRunner>();

    /// <summary>The step names in the order the nightly job runs them.</summary>
    public IReadOnlyList<string> StepNames => [.. _provider.GetServices<INightlyStep>().Select(s => s.Name)];

    /// <summary>The names of the parts in the order the step runs them.</summary>
    public IReadOnlyList<string> PartNames
    {
        get
        {
            using IServiceScope scope = _provider.CreateScope();
            return [.. scope.ServiceProvider.GetServices<IDisposalPart>().Select(p => p.Name)];
        }
    }

    /// <summary>Runs the disposal step once, as the nightly job does, in a scope of its own, at the clock's moment.</summary>
    public async Task RunStepAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        INightlyStep step = _provider.GetServices<INightlyStep>().Single(s => s is DisposalStep);
        await step.RunAsync(scope.ServiceProvider, Now, ct);
    }

    /// <summary>
    /// Runs only the order's part, in a scope of its own, at the clock's moment; <paramref name="afterPick"/> is the part's seam between the pick and
    /// the lock (06 9.2.1), where a test lets another transaction change an order.
    /// </summary>
    public async Task RunOrderPartAsync(Func<IReadOnlyList<Guid>, CancellationToken, Task>? afterPick, CancellationToken ct)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        OrderDisposalPart part = scope.ServiceProvider.GetServices<IDisposalPart>().OfType<OrderDisposalPart>().Single();
        part.AfterPick = afterPick;
        await part.RunAsync(new DisposalRun(scope.ServiceProvider, Now), ct);
    }

    /// <summary>Runs one part by its type, in a scope of its own, at the clock's moment (the part is the one the step would run).</summary>
    public async Task RunPartAsync<TPart>(CancellationToken ct)
        where TPart : class, IDisposalPart
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TPart part = scope.ServiceProvider.GetServices<IDisposalPart>().OfType<TPart>().Single();
        await part.RunAsync(new DisposalRun(scope.ServiceProvider, Now), ct);
    }

    /// <summary>Calls a module surface of the harness's scope, outside any transaction of the caller (a test of the surface's own guard).</summary>
    public async Task<T> WithServiceAsync<TService, T>(Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    /// <summary>Runs a piece of work with a scope's context and the part-writing helper, at the clock's moment.</summary>
    public async Task<T> WithRunAsync<T>(Func<DisposalRun, AppDbContext, Task<T>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await work(new DisposalRun(scope.ServiceProvider, Now), scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public async Task<T> ReadAsync<T>(Func<AppDbContext, Task<T>> read)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public async Task SeedAsync(Action<AppDbContext> add)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        add(db);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await DataSource.DisposeAsync();
    }
}
