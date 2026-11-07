using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the content status tests (T322): one database copied from the template.</summary>
[CollectionDefinition(ContentStatusDefinition.Name)]
public sealed class ContentStatusDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContentStatus";
}

/// <summary>
/// The publish rules of the corporate content on a real PostgreSQL, written as the application user (T322 criteria 1-6; KK1-KK6; 02 3.27.23, 5.2; 03 1.8.4):
/// the move from draft to published through the gate, the way back to draft with no archive, an edit that may not break the gate of a published record,
/// the draft that is not on the site, the about page that is never deleted and the deletion that asks for confirmation.
/// </summary>
[Collection(ContentStatusDefinition.Name)]
public sealed class ContentStatusTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly string[] ContentTables =
        ["about_pages", "offerings", "portfolio_items", "faq_items", "branches", "custom_pages", "announcements"];

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public ContentStatusTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK2: draft to published through the gate ----

    [Fact]
    public async Task Publish_ACompleteRecord_MovesItToPublished_AndRaisesTheVersionByOne_KK2()
    {
        FaqItem faq = FaqItem.Create("İade var mı?", 1, Now);
        faq.Answer = "<p>Evet.</p>";
        await SeedAsync(faq);

        ContentResult result = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.FaqItem, faq.Id, 1, Ct));

        Assert.Equal(ContentOutcome.Changed, result.Outcome);
        Assert.False(result.IsRejected);
        Assert.Equal("Published", await StatusAsync("faq_items", faq.Id));
        Assert.Equal(2, await VersionAsync("faq_items", faq.Id));
    }

    [Theory]
    [InlineData("AboutPage")]
    [InlineData("Offering")]
    [InlineData("PortfolioItem")]
    [InlineData("FaqItem")]
    [InlineData("Branch")]
    [InlineData("CustomPage")]
    [InlineData("Announcement")]
    public async Task Publish_ARecordWithAnEmptyGateField_IsRefusedWithTheMissingFields_AndWritesNothing_KK2(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        IContentRecord record = NewRecord(kind, complete: false);
        await SeedAsync(record);
        IReadOnlyList<ContentGateField> expected = record.MissingGateFields();
        Assert.NotEmpty(expected);

        ContentResult result = await RunAsync((_, publishing) => publishing.PublishAsync(kind, record.Id, 1, Ct));

        Assert.Equal(ContentOutcome.GateIncomplete, result.Outcome);
        Assert.True(result.IsRejected);
        Assert.Equal(expected, result.Missing);
        Assert.Equal("Draft", await StatusAsync(TableOf(kind), record.Id));
        Assert.Equal(1, await VersionAsync(TableOf(kind), record.Id));
    }

    [Theory]
    [InlineData("AboutPage")]
    [InlineData("Offering")]
    [InlineData("PortfolioItem")]
    [InlineData("FaqItem")]
    [InlineData("Branch")]
    [InlineData("CustomPage")]
    [InlineData("Announcement")]
    public async Task Publish_ACompleteRecordOfEveryKind_NeedsNoSecondApproval_KK2(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        IContentRecord record = NewRecord(kind, complete: true);
        await SeedAsync(record);

        ContentResult result = await RunAsync((_, publishing) => publishing.PublishAsync(kind, record.Id, 1, Ct));

        Assert.Equal(ContentOutcome.Changed, result.Outcome);
        Assert.Equal("Published", await StatusAsync(TableOf(kind), record.Id));
    }

    [Fact]
    public async Task Publish_ARecordAlreadyPublished_WritesNothing_AndKeepsTheVersion()
    {
        Offering offering = Offering.Create("Bakım", "bakim", 1, Now);
        await SeedAsync(offering);
        await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, offering.Id, 1, Ct));

        ContentResult again = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, offering.Id, 2, Ct));

        Assert.Equal(ContentOutcome.Unchanged, again.Outcome);
        Assert.Equal(2, await VersionAsync("offerings", offering.Id));
    }

    [Fact]
    public async Task APublishWithAStaleVersion_OrOfAMissingRecord_IsRefused()
    {
        Offering offering = Offering.Create("Bakım", "bakim", 1, Now);
        await SeedAsync(offering);

        ContentResult stale = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, offering.Id, 7, Ct));
        ContentResult missing = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, Guid.CreateVersion7(), 1, Ct));

        Assert.Equal(ContentOutcome.VersionConflict, stale.Outcome);
        Assert.Equal(ContentOutcome.NotFound, missing.Outcome);
        Assert.Equal("Draft", await StatusAsync("offerings", offering.Id));
    }

    // ---- KK1: published back to draft, no third status ----

    [Fact]
    public async Task Unpublish_PutsAPublishedRecordBackToDraft_NotToAnArchive_KK1()
    {
        CustomPage page = CustomPage.Create("Kampanya", "kampanya", 1, Now);
        await SeedAsync(page);
        await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.CustomPage, page.Id, 1, Ct));

        ContentResult result = await RunAsync((_, publishing) => publishing.UnpublishAsync(ContentRecordKind.CustomPage, page.Id, 2, Ct));

        Assert.Equal(ContentOutcome.Changed, result.Outcome);
        Assert.Equal("Draft", await StatusAsync("custom_pages", page.Id));
        Assert.Equal(3, await VersionAsync("custom_pages", page.Id));
    }

    [Fact]
    public async Task Unpublish_ADraft_WritesNothing_KK1()
    {
        Offering offering = Offering.Create("Bakım", "bakim", 1, Now);
        await SeedAsync(offering);

        ContentResult result = await RunAsync((_, publishing) => publishing.UnpublishAsync(ContentRecordKind.Offering, offering.Id, 1, Ct));

        Assert.Equal(ContentOutcome.Unchanged, result.Outcome);
        Assert.Equal(1, await VersionAsync("offerings", offering.Id));
    }

    [Fact]
    public async Task EverySevenContentTables_AcceptOnlyDraftAndPublished_AndNoVersionTableExists_KK1()
    {
        foreach (string table in ContentTables)
        {
            string definition = await ScalarAsync<string>(
                "SELECT pg_get_constraintdef(c.oid) FROM pg_constraint c JOIN pg_class t ON t.oid = c.conrelid " +
                $"WHERE t.relname = '{table}' AND c.contype = 'c' AND pg_get_constraintdef(c.oid) LIKE '%publish_status%'");

            Assert.Contains("'Draft'", definition, StringComparison.Ordinal);
            Assert.Contains("'Published'", definition, StringComparison.Ordinal);
            Assert.DoesNotContain("Archived", definition, StringComparison.Ordinal);
        }

        long historyTables = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public' AND " +
            "(table_name ~ '(about_page|offering|portfolio_item|faq_item|branch|custom_page|announcement)s?_(history|versions|revisions|archive)')");
        Assert.Equal(0L, historyTables);
    }

    // ---- KK3: an edit that breaks the gate of a published record is not saved ----

    [Fact]
    public async Task Edit_APublishedRecord_ThatEmptiesAGateField_IsNotSaved_AndTheRecordStaysAsItWas_KK3()
    {
        Branch branch = NewBranch(complete: true);
        branch.PublishStatus = PublishStatus.Published;
        await SeedAsync(branch);

        ContentResult result = await RunAsync(async (db, publishing) =>
        {
            Branch tracked = await db.Branches.SingleAsync(b => b.Id == branch.Id, Ct);
            return await publishing.SaveEditAsync(tracked, 1, b => b.AddressLine = "  ", Ct);
        });

        Assert.Equal(ContentOutcome.PublishedGateBroken, result.Outcome);
        Assert.True(result.IsRejected);
        Assert.Equal([ContentGateField.AddressLine], result.Missing);
        Assert.Equal("Atatürk Bulvarı 1", await ScalarAsync<string>($"SELECT address_line FROM branches WHERE id = '{branch.Id}'"));
        Assert.Equal(1, await VersionAsync("branches", branch.Id));
        Assert.Equal("Published", await StatusAsync("branches", branch.Id));
    }

    [Fact]
    public async Task Edit_APublishedRecord_ThatKeepsTheGate_IsSaved_AndIsOnTheSiteAtOnce_KK3()
    {
        FaqItem faq = FaqItem.Create("İade var mı?", 1, Now);
        faq.SetAnswer("<p>Evet.</p>");
        faq.PublishStatus = PublishStatus.Published;
        await SeedAsync(faq);

        ContentResult result = await RunAsync(async (db, publishing) =>
        {
            FaqItem tracked = await db.FaqItems.SingleAsync(f => f.Id == faq.Id, Ct);
            return await publishing.SaveEditAsync(tracked, 1, f => f.SetAnswer("<p>Evet, 14 gün içinde.</p>"), Ct);
        });

        Assert.Equal(ContentOutcome.Saved, result.Outcome);
        Assert.Equal("Published", await StatusAsync("faq_items", faq.Id));
        Assert.Equal(2, await VersionAsync("faq_items", faq.Id));
        Assert.Equal("<p>Evet, 14 gün içinde.</p>", await ScalarAsync<string>($"SELECT answer FROM faq_items WHERE id = '{faq.Id}'"));
    }

    [Fact]
    public async Task Edit_ADraft_MayLeaveAGateFieldEmpty_AndIsSaved_KK3()
    {
        FaqItem faq = FaqItem.Create("İade var mı?", 1, Now);
        faq.SetAnswer("<p>Evet.</p>");
        await SeedAsync(faq);

        ContentResult result = await RunAsync(async (db, publishing) =>
        {
            FaqItem tracked = await db.FaqItems.SingleAsync(f => f.Id == faq.Id, Ct);
            return await publishing.SaveEditAsync(tracked, 1, f => f.SetAnswer(null), Ct);
        });

        Assert.Equal(ContentOutcome.Saved, result.Outcome);
        Assert.Equal("Draft", await StatusAsync("faq_items", faq.Id));
    }

    [Fact]
    public async Task Edit_WithAStaleVersion_IsRefused_AndNothingIsWritten()
    {
        Offering offering = Offering.Create("Bakım", "bakim", 1, Now);
        await SeedAsync(offering);

        ContentResult result = await RunAsync(async (db, publishing) =>
        {
            Offering tracked = await db.Offerings.SingleAsync(o => o.Id == offering.Id, Ct);
            return await publishing.SaveEditAsync(tracked, 5, o => o.Name = "Yeni", Ct);
        });

        Assert.Equal(ContentOutcome.VersionConflict, result.Outcome);
        Assert.Equal("Bakım", await ScalarAsync<string>($"SELECT name FROM offerings WHERE id = '{offering.Id}'"));
    }

    // ---- KK4: a draft is not on the site ----

    [Fact]
    public async Task ADraft_IsNotOnTheSiteMap_APublishedRecordIs_AndTheWayBackTakesItOff_KK4()
    {
        Offering offering = Offering.Create("Bakım", "bakim", 1, Now);
        await SeedAsync(offering);
        Assert.DoesNotContain("/hizmet/bakim", await SitemapPathsAsync());

        await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, offering.Id, 1, Ct));
        Assert.Contains("/hizmet/bakim", await SitemapPathsAsync());

        await RunAsync((_, publishing) => publishing.UnpublishAsync(ContentRecordKind.Offering, offering.Id, 2, Ct));
        Assert.DoesNotContain("/hizmet/bakim", await SitemapPathsAsync());
    }

    // ---- KK6: the about page is never deleted; the others after the warning ----

    [Fact]
    public async Task TheAboutPage_IsNeverDeleted_EvenWhenConfirmed_AndCanBePutBackToDraft_KK6()
    {
        AboutPage about = AboutPage.Create(Now);
        about.ShortIntroduction = "Biz kimiz";
        await SeedAsync(about);
        await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.AboutPage, about.Id, 1, Ct));

        ContentResult refused = await RunAsync((_, publishing) => publishing.DeleteAsync(ContentRecordKind.AboutPage, about.Id, true, Ct));
        ContentResult back = await RunAsync((_, publishing) => publishing.UnpublishAsync(ContentRecordKind.AboutPage, about.Id, 2, Ct));

        Assert.Equal(ContentOutcome.Forbidden, refused.Outcome);
        Assert.True(refused.IsRejected);
        Assert.Equal(ContentOutcome.Changed, back.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
        Assert.Equal("Draft", await StatusAsync("about_pages", about.Id));
    }

    [Fact]
    public async Task ADeletion_WithoutTheIrreversibleWarningConfirmed_IsRefused_AndTheRecordStays_KK6()
    {
        FaqItem faq = FaqItem.Create("İade var mı?", 1, Now);
        await SeedAsync(faq);

        ContentResult result = await RunAsync((_, publishing) => publishing.DeleteAsync(ContentRecordKind.FaqItem, faq.Id, false, Ct));

        Assert.Equal(ContentOutcome.ConfirmationRequired, result.Outcome);
        Assert.True(result.IsRejected);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM faq_items"));
    }

    [Theory]
    [InlineData("Offering")]
    [InlineData("PortfolioItem")]
    [InlineData("FaqItem")]
    [InlineData("Branch")]
    [InlineData("CustomPage")]
    [InlineData("Announcement")]
    public async Task ADeletion_AfterTheWarningIsConfirmed_RemovesTheRecord_OfEveryKindButTheAboutPage_KK6(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        IContentRecord record = NewRecord(kind, complete: true);
        await SeedAsync(record);

        ContentResult result = await RunAsync((_, publishing) => publishing.DeleteAsync(kind, record.Id, true, Ct));

        Assert.Equal(ContentOutcome.Deleted, result.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {TableOf(kind)}"));
    }

    [Fact]
    public async Task ADeletion_OfAMissingRecord_IsNotFound()
    {
        ContentResult result = await RunAsync((_, publishing) => publishing.DeleteAsync(ContentRecordKind.Offering, Guid.CreateVersion7(), true, Ct));

        Assert.Equal(ContentOutcome.NotFound, result.Outcome);
    }

    // ---- helpers ----

    private static string TableOf(ContentRecordKind kind) => kind switch
    {
        ContentRecordKind.AboutPage => "about_pages",
        ContentRecordKind.Offering => "offerings",
        ContentRecordKind.PortfolioItem => "portfolio_items",
        ContentRecordKind.FaqItem => "faq_items",
        ContentRecordKind.Branch => "branches",
        ContentRecordKind.CustomPage => "custom_pages",
        ContentRecordKind.Announcement => "announcements",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static IContentRecord NewRecord(ContentRecordKind kind, bool complete)
    {
        switch (kind)
        {
            case ContentRecordKind.AboutPage:
                AboutPage about = AboutPage.Create(Now);
                about.ShortIntroduction = complete ? "Biz kimiz" : null;
                return about;
            case ContentRecordKind.Offering:
                return Offering.Create(complete ? "Bakım" : "   ", "bakim", 1, Now);
            case ContentRecordKind.PortfolioItem:
                return PortfolioItem.Create(complete ? "Proje" : "   ", "proje", 1, Now);
            case ContentRecordKind.FaqItem:
                FaqItem faq = FaqItem.Create("Soru?", 1, Now);
                faq.SetAnswer(complete ? "<p>Cevap</p>" : null);
                return faq;
            case ContentRecordKind.Branch:
                return NewBranch(complete);
            case ContentRecordKind.CustomPage:
                return CustomPage.Create(complete ? "Sayfa" : "   ", "sayfa", 1, Now);
            case ContentRecordKind.Announcement:
                Announcement announcement = Announcement.Create(Now);
                announcement.Text = complete ? "Bayram tatili" : null;
                return announcement;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static Branch NewBranch(bool complete)
    {
        Branch branch = Branch.Create("Merkez", 1, Now);
        if (complete)
        {
            branch.District = "Çankaya";
            branch.AddressLine = "Atatürk Bulvarı 1";
        }

        return branch;
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<T> RunAsync<T>(Func<AppDbContext, ContentPublishing, Task<T>> work)
        where T : IOperationResult
    {
        await using AppDbContext db = NewContext();
        CacheInvalidations invalidations = new(db);
        ContentPublishing publishing = new(db, invalidations);
        TransactionRunner runner = new(db, new JobEnqueueTracker(), new JobSignal(), invalidations, new AppCache());
        return await runner.RunAsync(cancel => work(db, publishing), Ct);
    }

    private async Task SeedAsync(object record)
    {
        await using AppDbContext db = NewContext();
        if (record is Branch branch && branch.District is not null)
        {
            branch.ProvinceId = await EnsureProvinceAsync(db);
        }

        db.Add(record);
        await db.SaveChangesAsync(Ct);
    }

    private static async Task<Guid> EnsureProvinceAsync(AppDbContext db)
    {
        Guid? existing = await db.Provinces.Select(p => (Guid?)p.Id).FirstOrDefaultAsync(Ct);
        if (existing is not null)
        {
            return existing.Value;
        }

        Modules.Settings.Province province = Modules.Settings.Province.Create("06", "Ankara", Now);
        db.Provinces.Add(province);
        await db.SaveChangesAsync(Ct);
        return province.Id;
    }

    private async Task<IReadOnlyList<string>> SitemapPathsAsync()
    {
        await using AppDbContext db = NewContext();
        return await new CorporateContentSitemapReader(db).ListAsync(Ct);
    }

    private Task<string> StatusAsync(string table, Guid id) => ScalarAsync<string>($"SELECT publish_status FROM {table} WHERE id = '{id}'");

    private Task<int> VersionAsync(string table, Guid id) => ScalarAsync<int>($"SELECT version FROM {table} WHERE id = '{id}'");

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
