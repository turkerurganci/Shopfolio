using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The scheduler in the opened application (T221 criterion 1; 09 13.1 items 1 and 10): one scheduler service, registered after the job queue's
/// runner, and the Ordering module's outage effect in the container. Every test works on its own copy of the template database.
/// </summary>
public sealed class SchedulerRegistrationTests : IAsyncLifetime
{
    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private string _dsn = string.Empty;

    public SchedulerRegistrationTests(TemplateDatabaseFixture template)
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
    public async Task Application_HasOneSchedulerService_StartedAfterTheJobRunner()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn, withScheduler: true);
        using HttpClient client = factory.CreateClient();

        List<Type> order = [.. factory.Services.GetServices<IHostedService>().Select(s => s.GetType())];

        Assert.Single(order, t => t == typeof(SchedulerService));
        Assert.True(order.IndexOf(typeof(JobRunnerService)) < order.IndexOf(typeof(SchedulerService)));
    }

    [Fact]
    public async Task Application_ResolvesTheSchedulerParts_AndTheOrderingModulesOutageEffect()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn);
        using HttpClient client = factory.CreateClient();

        Assert.Same(factory.Services.GetRequiredService<SchedulerScan>(), factory.Services.GetRequiredService<SchedulerScan>());
        Assert.Same(factory.Services.GetRequiredService<NightlyJobRunner>(), factory.Services.GetRequiredService<NightlyJobRunner>());
        Assert.Single(factory.Services.GetServices<INightlyStep>(), s => s is JobQueueCleanupStep);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        Assert.Single(scope.ServiceProvider.GetServices<IOutageEffect>(), e => e is TransferWindowDeferral);
    }

    [Fact]
    public async Task TestHost_DoesNotRunTheScheduler_UnlessAskedTo()
    {
        await using ShopfolioWebApplicationFactory factory = new(_dsn);
        using HttpClient client = factory.CreateClient();

        Assert.DoesNotContain(factory.Services.GetServices<IHostedService>(), s => s is SchedulerService);
    }
}
