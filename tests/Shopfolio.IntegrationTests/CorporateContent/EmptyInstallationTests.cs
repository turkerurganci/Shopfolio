using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the empty installation test (T360 KK2): one database copied from the template, with no truncation and no content seeding.</summary>
[CollectionDefinition(EmptyInstallationDefinition.Name)]
public sealed class EmptyInstallationDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "EmptyInstallation";
}

/// <summary>
/// The site is installed empty (T360 criterion 2; KK2; 02 3.27.22): the database copied from the template (the migrations write only the single company and settings rows)
/// holds no content, catalog, image or announcement row, and the content module answers "nothing" everywhere instead of showing a sample record.
/// </summary>
[Collection(EmptyInstallationDefinition.Name)]
public sealed class EmptyInstallationTests : IAsyncLifetime
{
    private static readonly string[] SampleBearingTables =
    [
        "about_pages", "offerings", "portfolio_items", "faq_items", "branches", "custom_pages", "announcements",
        "offering_products", "portfolio_item_products", "products", "variants", "categories", "product_categories", "images",
    ];

    private readonly CollectionDatabaseFixture _collection;
    private ServiceProvider _provider = null!;

    public EmptyInstallationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddPaymentModule();
        services.AddOrderingModule();
        services.AddCorporateContentModule();
        services.AddSettingsModule();
        services.AddScoped<IAuditActorSource>(_ => new NoActor());
        services.AddScoped<IAuditTrail, AuditTrail>();
        _provider = services.BuildServiceProvider();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    [Fact]
    public async Task ANewInstallation_CarriesNoContentCatalogImageOrAnnouncementRow_KK2()
    {
        foreach (string table in SampleBearingTables)
        {
            Assert.True(await CountAsync(table) == 0, $"The table {table} must be empty on a new installation.");
        }
    }

    [Fact]
    public async Task OnANewInstallation_TheContentModuleAnswersNothing_ForBothAudiences_KK2()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ICorporateContentModule module = scope.ServiceProvider.GetRequiredService<ICorporateContentModule>();
        ContentReaders readers = scope.ServiceProvider.GetRequiredService<ContentReaders>();

        Assert.Null(await module.GetAnnouncementAsync(asAdministrator: false, Ct));
        Assert.Null(await module.GetAnnouncementAsync(asAdministrator: true, Ct));

        HomepageView home = await module.GetHomepageAsync(Ct);
        Assert.Null(home.Intro);
        Assert.Null(home.Offerings);
        Assert.Null(home.PortfolioItems);
        Assert.Null(home.Showcase);

        Assert.Empty(await module.ListSitemapPathsAsync(Ct));
        Assert.Empty(await readers.ListFaqAsync(Ct));
        Assert.Empty(await readers.ListFaqForAdministratorAsync(Ct));
        Assert.Empty((await readers.ReadContactPageAsync(Ct)).Branches);
        Assert.Empty((await readers.ReadContactPageForAdministratorAsync(Ct)).Branches);

        StorefrontMenu menu = await module.GetMenuAsync(Ct);
        Assert.Empty(menu.FooterPages);
        Assert.Empty(menu.Categories);
        Assert.DoesNotContain(menu.VisibleItems, i => i.Kind is MenuItemKind.Offerings or MenuItemKind.PortfolioItems or MenuItemKind.AboutPage or MenuItemKind.FaqItems);
    }

    private sealed class NoActor : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(null);
    }

    private async Task<long> CountAsync(string table)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT count(*) FROM {table}", connection);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }
}
