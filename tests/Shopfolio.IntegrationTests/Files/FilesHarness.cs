using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>The collection of the file store and image tests (T223): one database copied from the template, nothing shared with another collection.</summary>
[CollectionDefinition(FilesDefinition.Name)]
public sealed class FilesDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "Files";
}

/// <summary>
/// The image side of the Catalog module as the application wires it - transaction runner, job queue, image processor, upload, derivative handler
/// and the two disposal parts - on the collection's database, a fake clock and a file store given by the test (the disk in a test directory, or
/// the in-memory fake that logs the order of its calls). No test waits for real time.
/// </summary>
internal sealed class FilesHarness : IAsyncDisposable
{


    private readonly ServiceProvider _provider;

    public FilesHarness(string dsn, Func<TimeProvider, IFileStore> files)
    {
        DataSource = NpgsqlDataSource.Create(dsn);
        Files = files(Time);
        ServiceCollection services = new();
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton(DataSource);
        services.AddSingleton(Files);
        services.AddSingleton<ImageProcessor>();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddScoped<ImageUploads>();
        services.AddScoped<ImageDerivativeJobHandler>();
        services.AddScoped<ImageDisposalPart>();
        services.AddScoped<OrphanImageFilePart>();
        _provider = services.BuildServiceProvider();
    }

    public NpgsqlDataSource DataSource { get; }

    public ManualTimeProvider Time { get; } = new(StartTime());

    public IFileStore Files { get; }

    public DateTimeOffset Now => Time.GetUtcNow();

    /// <summary>An hour ahead of the real clock, whole seconds: a file the test writes is never newer than the fake clock, and the database keeps the moment exactly.</summary>
    private static DateTimeOffset StartTime()
    {
        DateTimeOffset ahead = DateTimeOffset.UtcNow.AddHours(1);
        return new DateTimeOffset(ahead.Ticks - (ahead.Ticks % TimeSpan.TicksPerSecond), TimeSpan.Zero);
    }

    /// <summary>Runs a piece of work in a scope of its own, as a request or a job would.</summary>
    public async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await work(scope.ServiceProvider);
    }

    public Task<T> ReadAsync<T>(Func<AppDbContext, Task<T>> read) =>
        InScopeAsync(services => read(services.GetRequiredService<AppDbContext>()));

    public async Task SeedAsync(Action<AppDbContext> add)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        add(db);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public Task<ImageUploadResult> UploadAsync(byte[] content) =>
        InScopeAsync(services => services.GetRequiredService<ImageUploads>().UploadAsync(content, TestContext.Current.CancellationToken));

    public Task<JobOutcome> RunDerivativeJobAsync(Guid imageId) =>
        InScopeAsync(services => services.GetRequiredService<ImageDerivativeJobHandler>()
            .HandleAsync(new JobWork(Guid.NewGuid(), JobType.ImageDerivative, imageId, Now, 0), TestContext.Current.CancellationToken));

    public Task RunDisposalAsync<TPart>()
        where TPart : class, IDisposalPart =>
        InScopeAsync<int>(async services =>
        {
            await services.GetRequiredService<TPart>().RunAsync(new DisposalRun(services, Now), TestContext.Current.CancellationToken);
            return 0;
        });

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await DataSource.DisposeAsync();
    }
}
