using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.StartupDatabase;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Notifications;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The job queue in the opened application (T220 criterion 1; 09 13.1 items 1 and 10): one runner service, registered after the start-up checks and
/// the digest rewrite so that the queue starts last, the queue writer and the wake-up signal in the container, and the rewrite's gate among the
/// start gates (D-96 K1). Every test works on its own copy of the template database.
/// </summary>
public sealed class JobQueueRegistrationTests : IAsyncLifetime
{
    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private string _dsn = string.Empty;

    public JobQueueRegistrationTests(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _databaseName = await _template.CreateDatabaseFromTemplateAsync(Ct);
        _dsn = _template.DsnFor(_databaseName);
    }

    public async ValueTask DisposeAsync()
    {
        using NpgsqlConnection pooled = new(_dsn);
        NpgsqlConnection.ClearPool(pooled);
        await _template.DropDatabaseAsync(_databaseName, CancellationToken.None);
    }

    [Fact]
    public async Task Application_HasOneRunnerService_StartedAfterTheStartupChecksAndTheDigestRewrite()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn);
        using HttpClient client = factory.CreateClient();

        List<Type> order = [.. factory.Services.GetServices<IHostedService>().Select(s => s.GetType())];

        Assert.Single(order, t => t == typeof(JobRunnerService));
        int runner = order.IndexOf(typeof(JobRunnerService));
        Assert.True(order.IndexOf(typeof(StartupDatabaseCheck)) < runner);
        Assert.True(order.IndexOf(typeof(RevertLinkDigestRewriteStartup)) < runner);
    }

    [Fact]
    public async Task Application_ResolvesTheQueueWriterPerScope_AndOneSignalAndTheRewriteGate()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn);
        using HttpClient client = factory.CreateClient();

        await using (AsyncServiceScope first = factory.Services.CreateAsyncScope())
        await using (AsyncServiceScope second = factory.Services.CreateAsyncScope())
        {
            Assert.NotSame(first.ServiceProvider.GetRequiredService<IJobQueue>(), second.ServiceProvider.GetRequiredService<IJobQueue>());
            Assert.NotSame(first.ServiceProvider.GetRequiredService<JobEnqueueTracker>(), second.ServiceProvider.GetRequiredService<JobEnqueueTracker>());
        }

        Assert.Same(factory.Services.GetRequiredService<IJobSignal>(), factory.Services.GetRequiredService<IJobSignal>());
        Assert.Single(factory.Services.GetServices<IJobStartGate>(), g => g is RevertLinkDigestRewriteGate);
        Assert.Same(factory.Services.GetRequiredService<JobHandlerRegistry>(), factory.Services.GetRequiredService<JobHandlerRegistry>());
    }
}
