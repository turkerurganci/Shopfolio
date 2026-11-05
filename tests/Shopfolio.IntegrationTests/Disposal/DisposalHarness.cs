using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Ordering;
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
        DataSource = NpgsqlDataSource.Create(dsn);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton(DataSource);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddSingleton<NightlyJobRunner>();
        services.AddSingleton<INightlyStep, JobQueueCleanupStep>();
        services.AddDisposal(directories);
        services.AddDisposalPart<GuestCartDisposalPart>();
        services.AddDisposalPart<AdminInvitationDisposalPart>();
        _provider = services.BuildServiceProvider();
    }

    public string Dsn { get; }

    public NpgsqlDataSource DataSource { get; }

    public ManualTimeProvider Time { get; } = new(Start);

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
