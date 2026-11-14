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
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the panel content editing tests (T534): one database copied from the template.</summary>
[CollectionDefinition(PanelContentEditingDefinition.Name)]
public sealed class PanelContentEditingDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PanelContentEditing";
}

/// <summary>
/// The panel's corporate content list, form, save, status move, deletion and manual order on a real PostgreSQL (T534; 07 UC-96, UC-158 to UC-161; 04 9.13): the seven groups in order, the form
/// that is not there, the save that creates a draft at the end of its group with its gallery and linked products, the refusals, the deletion that the transaction's owner saves (D-673) and the
/// product link that share-locks the product row (D-720).
/// </summary>
[Collection(PanelContentEditingDefinition.Name)]
public sealed class PanelContentEditingTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private ServiceProvider _provider = null!;

    private Admin _admin = null!;

    private int _counter;

    public PanelContentEditingTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE audit_logs, cart_items, carts, order_items, orders, variants, products, categories, admins, about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements, images CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));

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
        services.AddScoped<global::Shopfolio.Infrastructure.Security.AttemptCounter>();
        services.AddScoped<Shopfolio.Modules.Notifications.Contracts.INotificationsModule, UnusedNotifications>();
        services.AddCorporateContentModule();
        services.AddSettingsModule();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- the list and the form ----

    [Fact]
    public async Task TheList_HasTheSevenGroupsInOrder_AndTheRecordsInTheManualOrder_NeverPaged()
    {
        await SeedOfferingAsync("İkinci", 2);
        await SeedOfferingAsync("Birinci", 1);
        await SeedOfferingAsync("Üçüncü", 3);

        PanelContentList list = await ReadAsync(m => m.GetPanelContentListAsync(Ct));

        Assert.Equal(
            [PanelContentType.AboutPage, PanelContentType.Offering, PanelContentType.PortfolioItem, PanelContentType.FaqItem, PanelContentType.Branch, PanelContentType.CustomPage, PanelContentType.Announcement],
            list.Groups.Select(g => g.Type));
        PanelContentGroup offerings = list.Groups.Single(g => g.Type == PanelContentType.Offering);
        Assert.Equal(["Birinci", "İkinci", "Üçüncü"], offerings.Rows.Select(r => r.Title));
        Assert.Null(offerings.Rows[0].PreviousSortOrder);
        Assert.Equal(2, offerings.Rows[0].NextSortOrder);
        Assert.Equal(2, offerings.Rows[2].PreviousSortOrder);
        Assert.Null(offerings.Rows[2].NextSortOrder);
        Assert.All(list.Groups.Where(g => g.Type != PanelContentType.Offering), g => Assert.Empty(g.Rows));
    }

    [Fact]
    public async Task TheForm_OfARecordThatIsNotThere_OrIsAnotherTypes_IsNull_AndTheNewForm_IsEmptyAtVersionZero()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);

        PanelContentForm? missing = await ReadAsync(m => m.GetPanelContentFormAsync(PanelContentType.Offering, Guid.CreateVersion7(), Ct));
        PanelContentForm? wrongType = await ReadAsync(m => m.GetPanelContentFormAsync(PanelContentType.FaqItem, offering, Ct));
        PanelContentForm? blank = await ReadAsync(m => m.GetPanelContentFormAsync(PanelContentType.Offering, null, Ct));

        Assert.Null(missing);
        Assert.Null(wrongType);
        Assert.NotNull(blank);
        Assert.Null(blank.Id);
        Assert.Equal(0, blank.Version);
        Assert.Equal(PublishStatus.Draft, blank.PublishStatus);
    }

    // ---- the save ----

    [Fact]
    public async Task ASave_OfANewOffering_CreatesADraftAtTheEndOfItsGroup_WithItsGalleryAndLinkedProducts()
    {
        await SeedOfferingAsync("Eski", 1);
        Guid category = await SeedCategoryAsync();
        Guid productA = await SeedProductAsync("Masa", category);
        Guid productB = await SeedProductAsync("Sandalye", category);
        Guid image = await SeedImageAsync();

        PanelContentSaveResult saved = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Offering, null, null, new PanelContentValues { Name = "Yeni hizmet", IsShownOnHomepage = true }, [new GalleryEntry(image, null, true)], [productB, productA]), Ct));

        Assert.Equal(PanelContentSaveOutcome.Saved, saved.Outcome);
        PanelContentForm? form = await ReadAsync(m => m.GetPanelContentFormAsync(PanelContentType.Offering, saved.Id, Ct));
        Assert.NotNull(form);
        Assert.Equal(PublishStatus.Draft, form.PublishStatus);
        Assert.Equal("Yeni hizmet", form.Values.Name);
        Assert.Equal([productB, productA], form.ProductIds);
        Assert.Equal([image], form.Images.Select(i => i.ImageId));
        Assert.True(form.Images[0].IsMain);
        PanelContentList list = await ReadAsync(m => m.GetPanelContentListAsync(Ct));
        Assert.Equal(["Eski", "Yeni hizmet"], list.Groups.Single(g => g.Type == PanelContentType.Offering).Rows.Select(r => r.Title));
    }

    [Fact]
    public async Task ASave_WithAnEmptyRequiredField_IsRejected_AndWritesNothing()
    {
        PanelContentSaveResult result = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Offering, null, null, new PanelContentValues { Name = "   " }, [], []), Ct));

        Assert.Equal(PanelContentSaveOutcome.Rejected, result.Outcome);
        Assert.NotEmpty(result.Rejections);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
    }

    [Fact]
    public async Task ASave_WithAStaleVersion_IsAConflict_AndTheRecordIsNotChanged()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);

        PanelContentSaveResult result = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Offering, offering, 99, new PanelContentValues { Name = "Başka ad" }, [], []), Ct));

        Assert.Equal(PanelContentSaveOutcome.VersionConflict, result.Outcome);
        Assert.Equal("Montaj", await ScalarAsync<string>($"SELECT name FROM offerings WHERE id = '{offering}'"));
    }

    [Fact]
    public async Task ASecondAnnouncement_IsRefused_NamingTheOneThatIsThere()
    {
        PanelContentSaveResult first = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Announcement, null, null, new PanelContentValues { Text = "Bayram tatili" }, [], []), Ct));
        PanelContentSaveResult second = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Announcement, null, null, new PanelContentValues { Text = "Başka" }, [], []), Ct));

        Assert.Equal(PanelContentSaveOutcome.Saved, first.Outcome);
        Assert.Equal(PanelContentSaveOutcome.AlreadyExists, second.Outcome);
        Assert.Equal(first.Id, second.ExistingId);
        Assert.Equal(first.Id, await ReadAsync(m => m.FindSinglePanelContentAsync(PanelContentType.Announcement, Ct)));
    }

    [Fact]
    public async Task EditingAPublishedRecord_ToEmptyAGateField_IsRefused_NamingTheField()
    {
        Guid faq = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 1);
        PanelContentChangeResult published = await RunAsync(m => m.ChangePanelContentStatusAsync(PanelContentType.FaqItem, faq, PublishStatus.Published, Ct));
        Assert.Equal(PanelContentChangeOutcome.Changed, published.Outcome);
        int version = await ScalarAsync<int>($"SELECT version FROM faq_items WHERE id = '{faq}'");

        PanelContentSaveResult result = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.FaqItem, faq, version, new PanelContentValues { Question = "İade var mı?", Answer = "  " }, [], []), Ct));

        Assert.Equal(PanelContentSaveOutcome.PublishedGateBroken, result.Outcome);
        Assert.Contains(PanelContentField.Answer, result.Missing);
    }

    [Fact]
    public async Task ASave_WithMoreImagesThanTheCeiling_OrADeletedImage_IsRejected_AndTheRecordIsNotKept()
    {
        Guid gone = Guid.CreateVersion7();

        PanelContentSaveResult missingImage = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Offering, null, null, new PanelContentValues { Name = "Hizmet" }, [new GalleryEntry(gone, null, true)], []), Ct));

        List<GalleryEntry> many = [];
        for (int i = 0; i < 40; i++)
        {
            many.Add(new GalleryEntry(await SeedImageAsync(), null, i == 0));
        }

        PanelContentSaveResult tooMany = await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Offering, null, null, new PanelContentValues { Name = "Hizmet" }, many, []), Ct));

        Assert.Equal(PanelContentSaveOutcome.Rejected, missingImage.Outcome);
        Assert.Equal(PanelContentSaveOutcome.Rejected, tooMany.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
    }

    // ---- the status move ----

    [Fact]
    public async Task TheStatusMove_IsAbsolute_ChecksTheGate_WritesTheTrail_AndRaisesTheVersion_OnlyOnAChange()
    {
        Guid faq = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 1);
        Guid incomplete = await SeedFaqAsync("Boş cevap?", null, 2);

        PanelContentChangeResult refused = await RunAsync(m => m.ChangePanelContentStatusAsync(PanelContentType.FaqItem, incomplete, PublishStatus.Published, Ct));
        PanelContentChangeResult changed = await RunAsync(m => m.ChangePanelContentStatusAsync(PanelContentType.FaqItem, faq, PublishStatus.Published, Ct));
        int afterChange = await ScalarAsync<int>($"SELECT version FROM faq_items WHERE id = '{faq}'");
        long trailAfterChange = await ScalarAsync<long>("SELECT count(*) FROM audit_logs");
        PanelContentChangeResult again = await RunAsync(m => m.ChangePanelContentStatusAsync(PanelContentType.FaqItem, faq, PublishStatus.Published, Ct));
        PanelContentChangeResult missing = await RunAsync(m => m.ChangePanelContentStatusAsync(PanelContentType.FaqItem, Guid.CreateVersion7(), PublishStatus.Draft, Ct));

        Assert.Equal(PanelContentChangeOutcome.GateIncomplete, refused.Outcome);
        Assert.Contains(PanelContentField.Answer, refused.Missing);
        Assert.Equal(PanelContentChangeOutcome.Changed, changed.Outcome);
        Assert.Equal(2, afterChange);
        Assert.True(trailAfterChange >= 1);
        Assert.Equal(PanelContentChangeOutcome.Unchanged, again.Outcome);
        Assert.Equal(afterChange, await ScalarAsync<int>($"SELECT version FROM faq_items WHERE id = '{faq}'"));
        Assert.Equal(trailAfterChange, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
        Assert.Equal(PanelContentChangeOutcome.NotFound, missing.Outcome);
    }

    [Fact]
    public async Task ThePageAddress_OfAPublishedOffering_StaysFixed_WhenTheNameChangesAfterwards()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        string before = (await ReadAsync(m => m.GetPanelContentFormAsync(PanelContentType.Offering, offering, Ct)))!.PagePath!;
        await RunAsync(m => m.ChangePanelContentStatusAsync(PanelContentType.Offering, offering, PublishStatus.Published, Ct));
        int version = await ScalarAsync<int>($"SELECT version FROM offerings WHERE id = '{offering}'");

        await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Offering, offering, version, new PanelContentValues { Name = "Başka ad" }, [], []), Ct));

        PanelContentForm? after = await ReadAsync(m => m.GetPanelContentFormAsync(PanelContentType.Offering, offering, Ct));
        Assert.Equal(before, after!.PagePath);
        Assert.True(after.IsAddressFixed);
    }

    // ---- the deletion (D-673) ----

    [Fact]
    public async Task ADeletion_IsSavedByTheTransactionsOwner_AndTheAboutPageIsRefused_D673()
    {
        Guid faq = await SeedFaqAsync("İade var mı?", "<p>Evet.</p>", 1);
        AboutPage about = AboutPage.Create(Now);
        await SaveAsync(c => c.AboutPages.Add(about));

        PanelContentDeleteResult refused = await RunAsync(m => m.DeletePanelContentAsync(PanelContentType.AboutPage, about.Id, Ct));
        PanelContentDeleteResult deleted = await RunAsync(m => m.DeletePanelContentAsync(PanelContentType.FaqItem, faq, Ct));
        PanelContentDeleteResult again = await RunAsync(m => m.DeletePanelContentAsync(PanelContentType.FaqItem, faq, Ct));

        Assert.Equal(PanelContentDeleteOutcome.Forbidden, refused.Outcome);
        Assert.Equal(PanelContentDeleteOutcome.Deleted, deleted.Outcome);
        Assert.Equal(PanelContentDeleteOutcome.NotFound, again.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM faq_items"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
    }

    // ---- the manual order ----

    [Fact]
    public async Task AMove_OneOrderUp_SwapsTheNeighbours_AStaleScreenIsAConflict_AndTheEdgesAndSinglesAreRefused()
    {
        Guid first = await SeedOfferingAsync("Birinci", 1);
        Guid second = await SeedOfferingAsync("İkinci", 2);
        Guid announcement = (await RunAsync(m => m.SavePanelContentAsync(
            new PanelContentSaveCommand(PanelContentType.Announcement, null, null, new PanelContentValues { Text = "Duyuru" }, [], []), Ct))).Id!.Value;

        PanelContentReorderResult stale = await RunAsync(m => m.ReorderPanelContentAsync(PanelContentType.Offering, second, PanelContentMove.Up, 2, 7, Ct));
        PanelContentReorderResult edge = await RunAsync(m => m.ReorderPanelContentAsync(PanelContentType.Offering, first, PanelContentMove.Up, 1, 1, Ct));
        PanelContentReorderResult single = await RunAsync(m => m.ReorderPanelContentAsync(PanelContentType.Announcement, announcement, PanelContentMove.Up, 0, 0, Ct));
        PanelContentReorderResult moved = await RunAsync(m => m.ReorderPanelContentAsync(PanelContentType.Offering, second, PanelContentMove.Up, 2, 1, Ct));

        Assert.Equal(PanelContentReorderOutcome.OrderChanged, stale.Outcome);
        Assert.Equal(PanelContentReorderOutcome.AtEdge, edge.Outcome);
        Assert.Equal(PanelContentReorderOutcome.NotOrderable, single.Outcome);
        Assert.Equal(PanelContentReorderOutcome.Moved, moved.Outcome);
        PanelContentList list = await ReadAsync(m => m.GetPanelContentListAsync(Ct));
        Assert.Equal(["İkinci", "Birinci"], list.Groups.Single(g => g.Type == PanelContentType.Offering).Rows.Select(r => r.Title));
    }

    // ---- the product link lock (D-720) ----

    [Fact]
    public async Task ALinkedProduct_IsShareLockedUntilTheTransactionEnds_SoItsDeletionCannotSlipIn_D720()
    {
        Guid category = await SeedCategoryAsync();
        Guid product = await SeedProductAsync("Masa", category);
        bool lockedDuringTheSave = false;
        string? sqlState = null;

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ICorporateContentModule module = scope.ServiceProvider.GetRequiredService<ICorporateContentModule>();
        PanelContentSaveResult saved = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async cancel =>
            {
                PanelContentSaveResult result = await module.SavePanelContentAsync(
                    new PanelContentSaveCommand(PanelContentType.Offering, null, null, new PanelContentValues { Name = "Hizmet" }, [], [product]), cancel);
                await using NpgsqlConnection other = new(_collection.Dsn);
                await other.OpenAsync(cancel);
                await using NpgsqlTransaction tx = await other.BeginTransactionAsync(cancel);
                await using NpgsqlCommand probe = new($"SELECT id FROM products WHERE id = '{product}' FOR UPDATE NOWAIT", other, tx);
                try
                {
                    await probe.ExecuteScalarAsync(cancel);
                }
                catch (PostgresException ex)
                {
                    lockedDuringTheSave = true;
                    sqlState = ex.SqlState;
                }

                return result;
            },
            Ct);

        Assert.Equal(PanelContentSaveOutcome.Saved, saved.Outcome);
        Assert.True(lockedDuringTheSave);
        Assert.Equal("55P03", sqlState);
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM offering_products WHERE product_id = '{product}'"));
    }

    // ---- operations ----

    private async Task<T> RunAsync<T>(Func<ICorporateContentModule, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ICorporateContentModule module = scope.ServiceProvider.GetRequiredService<ICorporateContentModule>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(cancel => work(module), Ct);
    }

    private async Task<T> ReadAsync<T>(Func<ICorporateContentModule, Task<T>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<ICorporateContentModule>());
    }

    // ---- seeds ----

    private async Task<Guid> SeedOfferingAsync(string name, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        Offering offering = Offering.Create(name, $"hizmet-{n:D3}", order, Now.AddMinutes(n));
        await SaveAsync(c => c.Offerings.Add(offering));
        return offering.Id;
    }

    private async Task<Guid> SeedFaqAsync(string question, string? answer, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        FaqItem faq = FaqItem.Create(question, order, Now.AddMinutes(n));
        faq.SetAnswer(answer);
        await SaveAsync(c => c.FaqItems.Add(faq));
        return faq.Id;
    }

    private async Task<Guid> SeedCategoryAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create("Mobilya", $"kategori-{n:D3}", null, 1, Now.AddMinutes(n));
        await SaveAsync(c => c.Categories.Add(category));
        return category.Id;
    }

    private async Task<Guid> SeedProductAsync(string name, Guid category)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category, true, Now));
        });
        return product.Id;
    }

    private async Task<Guid> SeedImageAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Image image = Image.Create($"gorsel-{n:D4}", ImageFormat.Jpeg, 800, 600, Now);
        await SaveAsync(c => c.Images.Add(image));
        return image.Id;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
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

    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }
}
