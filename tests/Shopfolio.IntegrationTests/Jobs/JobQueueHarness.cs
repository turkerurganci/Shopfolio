using System.Collections.Concurrent;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Jobs;

/// <summary>The collection of the job queue tests (T220): one database copied from the template.</summary>
[CollectionDefinition(JobQueueDefinition.Name)]
public sealed class JobQueueDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "JobQueue";
}

/// <summary>What a scripted handler is given: the row, the services of the row's scope, and the token.</summary>
internal sealed record HandlerCall(JobWork Job, IServiceProvider Services, CancellationToken Ct);

/// <summary>
/// The behavior of the handlers of a test, by job type. Every call is recorded before the script runs, so a test can count how many times a row
/// was handed to a handler. A type without a script completes.
/// </summary>
internal sealed class HandlerScript
{
    private readonly ConcurrentQueue<JobWork> _calls = new();

    public ConcurrentDictionary<JobType, Func<HandlerCall, Task<JobOutcome>>> ByType { get; } = new();

    public Func<HandlerCall, Task>? Transactional { get; set; }

    public IReadOnlyList<JobWork> Calls => [.. _calls];

    public void Record(JobWork work) => _calls.Enqueue(work);
}

/// <summary>A handler of a job that calls outside, whose behavior is the test's script.</summary>
internal sealed class ScriptedHandler : IJobHandler
{
    private readonly HandlerScript _script;
    private readonly IServiceProvider _services;

    public ScriptedHandler(HandlerScript script, IServiceProvider services)
    {
        _script = script;
        _services = services;
    }

    public Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct)
    {
        _script.Record(job);
        return _script.ByType.TryGetValue(job.JobType, out Func<HandlerCall, Task<JobOutcome>>? script)
            ? script(new HandlerCall(job, _services, ct))
            : Task.FromResult(JobOutcome.Completed);
    }
}

/// <summary>A handler of a job that only writes to the database, whose behavior is the test's script.</summary>
internal sealed class ScriptedTransactionalHandler : ITransactionalJobHandler
{
    private readonly HandlerScript _script;
    private readonly IServiceProvider _services;

    public ScriptedTransactionalHandler(HandlerScript script, IServiceProvider services)
    {
        _script = script;
        _services = services;
    }

    public Task HandleAsync(JobWork job, CancellationToken ct)
    {
        _script.Record(job);
        return _script.Transactional is { } script ? script(new HandlerCall(job, _services, ct)) : Task.CompletedTask;
    }
}

/// <summary>A start gate that a test opens and closes.</summary>
internal sealed class SwitchGate : IJobStartGate
{
    public volatile bool IsOpen;

    public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(IsOpen);
}

/// <summary>The real signal, counting how many times it was notified.</summary>
internal sealed class RecordingSignal : IJobSignal
{
    private readonly JobSignal _inner = new();
    private int _notifications;

    public int Notifications => Volatile.Read(ref _notifications);

    public void Notify()
    {
        Interlocked.Increment(ref _notifications);
        _inner.Notify();
    }

    public Task WaitAsync(TimeSpan timeout, TimeProvider time, CancellationToken ct) => _inner.WaitAsync(timeout, time, ct);
}

/// <summary>A step result for the transaction runner: not refused by default.</summary>
internal sealed record StepResult(bool IsRejected = false) : IOperationResult;

/// <summary>
/// The job queue as the application wires it, on the collection's database and a fake clock: the queue writer, the transaction runner with its
/// wake-up, the handler registry and the runner service, with the handlers of the test's script. A test drives one round (<see cref="RunOneAsync"/>)
/// or the whole service (<see cref="NewRunner"/>); no test waits for real time to pass in the queue - the clock is moved.
/// </summary>
internal sealed class JobQueueHarness : IAsyncDisposable
{
    public static readonly DateTimeOffset Start = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private readonly ServiceProvider _provider;

    public JobQueueHarness(
        string dsn,
        IReadOnlyCollection<JobType> handled,
        JobType? transactional = null,
        IReadOnlyCollection<IJobStartGate>? gates = null,
        Action<IServiceCollection>? configure = null)
    {
        Dsn = dsn;
        DataSource = NpgsqlDataSource.Create(dsn);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton(Script);
        services.AddSingleton(DataSource);
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddScoped<JobEnqueueTracker>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddSingleton<IJobSignal>(Signal);
        services.AddLogging(builder => builder.AddProvider(Logs));
        foreach (JobType type in handled)
        {
            services.AddJobHandler<ScriptedHandler>(type);
        }

        if (transactional is { } transactionalType)
        {
            services.AddTransactionalJobHandler<ScriptedTransactionalHandler>(transactionalType);
        }

        foreach (IJobStartGate gate in gates ?? [])
        {
            services.AddSingleton(gate);
        }

        configure?.Invoke(services);
        services.AddSingleton(provider => new JobHandlerRegistry(provider.GetServices<JobHandlerRegistration>()));
        _provider = services.BuildServiceProvider();
    }

    public string Dsn { get; }

    public NpgsqlDataSource DataSource { get; }

    public ManualTimeProvider Time { get; } = new(Start);

    public HandlerScript Script { get; } = new();

    public RecordingSignal Signal { get; } = new();

    public CollectingLoggerProvider Logs { get; } = new();

    public IServiceProvider Services => _provider;

    /// <summary>A runner service with the signal of the harness, or with its own signal when one is given.</summary>
    public JobRunnerService NewRunner(IJobSignal? signal = null)
    {
        return signal is null
            ? ActivatorUtilities.CreateInstance<JobRunnerService>(_provider)
            : ActivatorUtilities.CreateInstance<JobRunnerService>(_provider, signal);
    }

    public Task<bool> RunOneAsync(CancellationToken ct) => NewRunner().RunOneAsync(ct);

    /// <summary>Adds a row in the transaction of one scope and commits it, as a transition would.</summary>
    public async Task EnqueueAsync(JobType type, Guid? relatedRecordId, DateTimeOffset? notBefore = null)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IJobQueue queue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
        await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async ct =>
            {
                await queue.EnqueueAsync(type, relatedRecordId, notBefore, ct);
                return new StepResult();
            },
            TestContext.Current.CancellationToken);
    }

    public async Task<IReadOnlyList<Job>> RowsAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Jobs.AsNoTracking().OrderBy(j => j.CreatedAt).ThenBy(j => j.Id).ToListAsync(TestContext.Current.CancellationToken);
    }

    public async Task<Job> RowAsync(Guid id)
    {
        return (await RowsAsync()).Single(j => j.Id == id);
    }

    /// <summary>Waits, in real time and only as a guard against a hang, until the condition holds; the queue's own waits are on the fake clock.</summary>
    public static async Task EventuallyAsync(Func<Task<bool>> condition, string what)
    {
        DateTime until = DateTime.UtcNow.AddSeconds(60);
        while (!await condition())
        {
            if (DateTime.UtcNow > until)
            {
                Assert.Fail("Did not happen in time: " + what);
            }

            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>Waits until the code under test is waiting on the fake clock (it has registered a timer).</summary>
    public Task WaitForTimerAsync() => EventuallyAsync(() => Task.FromResult(Time.ActiveTimerCount > 0), "the runner waits on the fake clock");

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await DataSource.DisposeAsync();
    }
}

/// <summary>The shared parts of the job queue test classes: the collection's database, raw statements and a clean start.</summary>
public abstract class JobQueueTestBase : IAsyncLifetime
{
    protected JobQueueTestBase(CollectionDatabaseFixture collection)
    {
        Collection = collection;
    }

    protected CollectionDatabaseFixture Collection { get; }

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    public virtual async ValueTask InitializeAsync()
    {
        await ExecuteAsync("CREATE TABLE IF NOT EXISTS job_test_effects (job_id uuid PRIMARY KEY, note text NOT NULL DEFAULT '')");
        await ExecuteAsync("TRUNCATE jobs, job_test_effects, refund_records, dispatch_records, link_records, sessions, customers, admins CASCADE");
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    internal JobQueueHarness NewHarness(
        IReadOnlyCollection<JobType> handled,
        JobType? transactional = null,
        IReadOnlyCollection<IJobStartGate>? gates = null,
        Action<IServiceCollection>? configure = null)
    {
        return new JobQueueHarness(Collection.Dsn, handled, transactional, gates, configure);
    }

    protected async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(Collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    protected async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(Collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)Convert.ChangeType((await command.ExecuteScalarAsync(Ct))!, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    protected Task<long> EffectCountAsync() => ScalarAsync<long>("SELECT count(*) FROM job_test_effects");

    internal AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, Collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    /// <summary>Writes the idempotent effect of a row inside the open transaction of the given scope: one row per job, however many times it is written.</summary>
    internal static Task<int> WriteEffectAsync(AppDbContext db, Guid jobId, CancellationToken ct)
    {
        return db.Database.ExecuteSqlAsync($"INSERT INTO job_test_effects (job_id) VALUES ({jobId}) ON CONFLICT DO NOTHING", ct);
    }
}
