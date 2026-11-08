using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

using FieldRejection = Shopfolio.Infrastructure.Validation.FieldRejection;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the corporate content entry tests (T358): one database copied from the template.</summary>
[CollectionDefinition(ContentAuthoringDefinition.Name)]
public sealed class ContentAuthoringDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContentAuthoring";
}

/// <summary>
/// The entry of the corporate content on a real PostgreSQL, written as the application user (T358 criteria 3, 7, 9, 10; KK3, KK7, KK9, KK10): the one about page, the long text
/// that is cleaned on save, the address made on the server, the branch's province from the closed list and its foreign key, the unlimited general pages, the one flat question
/// list, the contact page built from the parts and the social links with one row per platform.
/// </summary>
[Collection(ContentAuthoringDefinition.Name)]
public sealed class ContentAuthoringTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;

    public ContentAuthoringTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string AppDsn => _template.DsnFor(_collection.DatabaseName, _template.Users.App, _template.Users.AppPassword, pooling: false);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements, social_links, provinces CASCADE");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK3: the one about page ----

    [Fact]
    public async Task TheAboutPage_FirstSaveCreatesTheOneRow_AndASecondIsRefused_KK3()
    {
        AuthoringResult first = await RunAsync((authoring, _) => authoring.CreateAboutPageAsync(new AboutPageInput("Biz kimiz", null), Ct));
        AuthoringResult second = await RunAsync((authoring, _) => authoring.CreateAboutPageAsync(new AboutPageInput("Başka", null), Ct));

        Assert.Equal(AuthoringOutcome.Created, first.Outcome);
        Assert.Equal(AuthoringOutcome.AlreadyExists, second.Outcome);
        Assert.True(second.IsRejected);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
        Assert.Equal("Biz kimiz", await ScalarAsync<string>("SELECT short_introduction FROM about_pages"));
        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM about_pages"));
    }

    [Fact]
    public async Task TheAboutPage_ShortIntroduction_IsStoredAsTyped_AndTheLongTextIsCleaned_KK3()
    {
        await RunAsync((authoring, _) => authoring.CreateAboutPageAsync(new AboutPageInput("<b>x</b> & y", "<p>Merhaba</p><script>alert(1)</script>"), Ct));

        Assert.Equal("<b>x</b> & y", await ScalarAsync<string>("SELECT short_introduction FROM about_pages"));
        string longText = await ScalarAsync<string>("SELECT long_text FROM about_pages");
        Assert.Contains("Merhaba", longText, StringComparison.Ordinal);
        Assert.DoesNotContain("script", longText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", longText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheAboutPage_ShortIntroductionOverTheCeiling_IsRefused_AndWritesNothing_KK3()
    {
        AuthoringResult result = await RunAsync((authoring, _) =>
            authoring.CreateAboutPageAsync(new AboutPageInput(new string('a', ContentRules.ShortIntroductionMaxLength + 1), null), Ct));

        Assert.Equal(AuthoringOutcome.Rejected, result.Outcome);
        Assert.Equal([new FieldRejection(nameof(AboutPageInput.ShortIntroduction), FieldCodes.TextTooLong)], result.Rejections);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
    }

    [Fact]
    public async Task TheAboutPageEdit_RaisesTheVersion_AndAStaleFormIsAConflict_KK3()
    {
        AuthoringResult created = await RunAsync((authoring, _) => authoring.CreateAboutPageAsync(new AboutPageInput("Biz", null), Ct));

        AuthoringResult saved = await RunAsync((authoring, _) => authoring.SaveAboutPageAsync(new AboutPageInput("Biz kimiz", null), created.Version!.Value, Ct));
        AuthoringResult stale = await RunAsync((authoring, _) => authoring.SaveAboutPageAsync(new AboutPageInput("Eski", null), created.Version!.Value, Ct));

        Assert.Equal(AuthoringOutcome.Saved, saved.Outcome);
        Assert.Equal(AuthoringOutcome.VersionConflict, stale.Outcome);
        Assert.Equal("Biz kimiz", await ScalarAsync<string>("SELECT short_introduction FROM about_pages"));
    }

    [Fact]
    public async Task TheAboutPageEdit_WithoutARow_IsNotFound()
    {
        AuthoringResult result = await RunAsync((authoring, _) => authoring.SaveAboutPageAsync(new AboutPageInput("Biz", null), 1, Ct));

        Assert.Equal(AuthoringOutcome.NotFound, result.Outcome);
    }

    // ---- the address made on the server ----

    [Fact]
    public async Task TheAddressOfANewRecord_IsMadeFromItsName_AndANumberedOneOnAClash()
    {
        AuthoringResult first = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Web Tasarım", null, null, false), Ct));
        AuthoringResult second = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("web tasarim", null, null, false), Ct));
        AuthoringResult third = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("WEB TASARIM", null, null, false), Ct));

        Assert.Equal("web-tasarim", await SlugAsync("offerings", first.Id!.Value));
        Assert.Equal("web-tasarim-2", await SlugAsync("offerings", second.Id!.Value));
        Assert.Equal("web-tasarim-3", await SlugAsync("offerings", third.Id!.Value));
    }

    [Fact]
    public async Task ThreeRecordsOfTheSameNameMadeAtTheSameTime_AllGetADistinctAddress_WhenTheCallerRetriesOnTheSlugIndex()
    {
        // The retry on the unique index is the transaction owner's (09 9.4 item 3): the entry method reads the used addresses afresh on every attempt.
        string[] retryOn = ["ix_offerings_slug"];
        Task<AuthoringResult>[] runs =
        [
            .. Enumerable.Range(0, 3).Select(async _ =>
            {
                await using AppDbContext db = NewContext();
                CacheInvalidations invalidations = new(db);
                ContentAuthoring authoring = new(db, new ContentPublishing(db, invalidations, TimeProvider.System), new ProvinceList(db), TimeProvider.System);
                TransactionRunner runner = new(db, new JobEnqueueTracker(), new JobSignal(), invalidations, new AppCache());
                return await runner.RunAsync(cancel => authoring.CreateOfferingAsync(new OfferingInput("Çelik Kapı", null, null, false), cancel), retryOn, Ct);
            }),
        ];

        AuthoringResult[] results = await Task.WhenAll(runs);

        Assert.All(results, r => Assert.Equal(AuthoringOutcome.Created, r.Outcome));
        Assert.Equal(3L, await ScalarAsync<long>("SELECT count(DISTINCT slug) FROM offerings WHERE slug IN ('celik-kapi', 'celik-kapi-2', 'celik-kapi-3')"));
    }

    [Fact]
    public async Task AnOfferingAndAReferenceWork_AreBornDrafts_InTheFirmsOrder()
    {
        AuthoringResult a = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Bir", "kısa", "<p>m</p>", true), Ct));
        AuthoringResult b = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("İki", null, null, false), Ct));
        AuthoringResult p = await RunAsync((authoring, _) => authoring.CreatePortfolioItemAsync(new PortfolioItemInput("Proje", null, null, true), Ct));

        Assert.Equal(AuthoringOutcome.Created, a.Outcome);
        Assert.Equal(1, await ScalarAsync<int>($"SELECT sort_order FROM offerings WHERE id = '{a.Id}'"));
        Assert.Equal(2, await ScalarAsync<int>($"SELECT sort_order FROM offerings WHERE id = '{b.Id}'"));
        Assert.Equal("Draft", await ScalarAsync<string>($"SELECT publish_status FROM portfolio_items WHERE id = '{p.Id}'"));
        Assert.Equal("proje", await SlugAsync("portfolio_items", p.Id!.Value));
    }

    // ---- T363: the address follows the name until the first publication, then it is fixed ----

    [Fact]
    public async Task AnOfferingsAddress_FollowsTheNameOnEveryEdit_TakesANumberOnAClash_AndKeepsItsOwnWhenTheNameIsSavedAgain_T363()
    {
        AuthoringResult other = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Mobil Uygulama", null, null, false), Ct));
        AuthoringResult mine = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Web Tasarım", null, null, false), Ct));
        Assert.Equal("web-tasarim", await SlugAsync("offerings", mine.Id!.Value));

        AuthoringResult renamed = await RunAsync((authoring, _) => authoring.SaveOfferingAsync(mine.Id!.Value, new OfferingInput("Kurumsal Site", null, null, false), mine.Version!.Value, Ct));
        AuthoringResult same = await RunAsync((authoring, _) => authoring.SaveOfferingAsync(mine.Id!.Value, new OfferingInput("Kurumsal Site", "kısa", null, false), renamed.Version!.Value, Ct));
        AuthoringResult clash = await RunAsync((authoring, _) => authoring.SaveOfferingAsync(mine.Id!.Value, new OfferingInput("mobil uygulama", null, null, false), same.Version!.Value, Ct));

        Assert.Equal(AuthoringOutcome.Saved, renamed.Outcome);
        Assert.Equal(AuthoringOutcome.Saved, same.Outcome);
        Assert.Equal(AuthoringOutcome.Saved, clash.Outcome);
        Assert.Equal("mobil-uygulama", await SlugAsync("offerings", other.Id!.Value));
        Assert.Equal("mobil-uygulama-2", await SlugAsync("offerings", mine.Id!.Value));
    }

    [Fact]
    public async Task AnOfferingsFirstPublication_FixesTheAddress_AndNeitherAnEditNorTheWayBackToDraftChangesIt_T363()
    {
        AuthoringResult created = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Web Tasarım", null, null, false), Ct));
        Guid id = created.Id!.Value;
        Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM offerings WHERE id = '{id}' AND slug_fixed_at IS NOT NULL"));

        ContentResult published = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, id, created.Version!.Value, Ct));
        DateTime fixedAt = await ScalarAsync<DateTime>($"SELECT slug_fixed_at FROM offerings WHERE id = '{id}'");
        AuthoringResult edited = await RunAsync((authoring, _) => authoring.SaveOfferingAsync(id, new OfferingInput("Tamamen Başka Ad", null, null, false), published.Version!.Value, Ct));
        ContentResult draft = await RunAsync((_, publishing) => publishing.UnpublishAsync(ContentRecordKind.Offering, id, edited.Version!.Value, Ct));
        AuthoringResult editedAgain = await RunAsync((authoring, _) => authoring.SaveOfferingAsync(id, new OfferingInput("Bir Ad Daha", null, null, false), draft.Version!.Value, Ct));
        ContentResult again = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.Offering, id, editedAgain.Version!.Value, Ct));

        Assert.Equal(ContentOutcome.Changed, published.Outcome);
        Assert.Equal(AuthoringOutcome.Saved, edited.Outcome);
        Assert.Equal(ContentOutcome.Changed, draft.Outcome);
        Assert.Equal(AuthoringOutcome.Saved, editedAgain.Outcome);
        Assert.Equal(ContentOutcome.Changed, again.Outcome);
        Assert.Equal("web-tasarim", await SlugAsync("offerings", id));
        Assert.Equal(fixedAt, await ScalarAsync<DateTime>($"SELECT slug_fixed_at FROM offerings WHERE id = '{id}'"));
        Assert.Equal("Bir Ad Daha", await ScalarAsync<string>($"SELECT name FROM offerings WHERE id = '{id}'"));
    }

    [Fact]
    public async Task AReferenceWorkAndAGeneralPage_FollowTheirTitleUntilTheFirstPublication_ThenStay_T363()
    {
        AuthoringResult work = await RunAsync((authoring, _) => authoring.CreatePortfolioItemAsync(new PortfolioItemInput("Eski Proje", null, null, false), Ct));
        AuthoringResult page = await RunAsync((authoring, _) => authoring.CreateCustomPageAsync(new CustomPageInput("Eski Sayfa", null, false), Ct));
        AuthoringResult workRenamed = await RunAsync((authoring, _) => authoring.SavePortfolioItemAsync(work.Id!.Value, new PortfolioItemInput("Yeni Proje", null, null, false), work.Version!.Value, Ct));
        AuthoringResult pageRenamed = await RunAsync((authoring, _) => authoring.SaveCustomPageAsync(page.Id!.Value, new CustomPageInput("Yeni Sayfa", null, false), page.Version!.Value, Ct));

        Assert.Equal("yeni-proje", await SlugAsync("portfolio_items", work.Id!.Value));
        Assert.Equal("yeni-sayfa", await SlugAsync("custom_pages", page.Id!.Value));

        ContentResult workPublished = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.PortfolioItem, work.Id!.Value, workRenamed.Version!.Value, Ct));
        ContentResult pagePublished = await RunAsync((_, publishing) => publishing.PublishAsync(ContentRecordKind.CustomPage, page.Id!.Value, pageRenamed.Version!.Value, Ct));
        await RunAsync((authoring, _) => authoring.SavePortfolioItemAsync(work.Id!.Value, new PortfolioItemInput("Üçüncü Proje", null, null, false), workPublished.Version!.Value, Ct));
        await RunAsync((authoring, _) => authoring.SaveCustomPageAsync(page.Id!.Value, new CustomPageInput("Üçüncü Sayfa", null, false), pagePublished.Version!.Value, Ct));

        Assert.Equal("yeni-proje", await SlugAsync("portfolio_items", work.Id!.Value));
        Assert.Equal("yeni-sayfa", await SlugAsync("custom_pages", page.Id!.Value));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM portfolio_items WHERE id = '{work.Id}' AND slug_fixed_at IS NOT NULL"));
        Assert.Equal(1L, await ScalarAsync<long>($"SELECT count(*) FROM custom_pages WHERE id = '{page.Id}' AND slug_fixed_at IS NOT NULL"));
    }

    [Fact]
    public async Task ADeletedRecordsAddress_IsFreeAgain_AndADraftsAddressCountsAsUsed_T363()
    {
        AuthoringResult first = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Web Tasarım", null, null, false), Ct));
        AuthoringResult second = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Web Tasarım", null, null, false), Ct));
        Assert.Equal("web-tasarim-2", await SlugAsync("offerings", second.Id!.Value));

        ContentResult deleted = await RunAsync((_, publishing) => publishing.DeleteAsync(ContentRecordKind.Offering, first.Id!.Value, irreversibleWarningConfirmed: true, Ct));
        AuthoringResult third = await RunAsync((authoring, _) => authoring.CreateOfferingAsync(new OfferingInput("Web Tasarım", null, null, false), Ct));

        Assert.Equal(ContentOutcome.Deleted, deleted.Outcome);
        Assert.Equal("web-tasarim", await SlugAsync("offerings", third.Id!.Value));
    }

    // ---- KK7: the branch ----

    [Fact]
    public async Task ABranch_WithAProvinceNotOnTheList_IsRefused_AndWritesNothing_KK7()
    {
        AuthoringResult result = await RunAsync((authoring, _) =>
            authoring.CreateBranchAsync(new BranchInput("Merkez", Guid.CreateVersion7(), "Çankaya", "Atatürk Bulvarı 1", null, null, null), Ct));

        Assert.Equal(AuthoringOutcome.Rejected, result.Outcome);
        Assert.Equal([new FieldRejection(nameof(BranchInput.ProvinceId), FieldCodes.InvalidProvince)], result.Rejections);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM branches"));
    }

    [Fact]
    public async Task ABranch_WithAProvinceOnTheList_IsSaved_WithTheProvinceKey_AndTheNameAlone_IsADraft_KK7()
    {
        Guid province = await SeedProvinceAsync("06", "Ankara");

        AuthoringResult full = await RunAsync((authoring, _) =>
            authoring.CreateBranchAsync(new BranchInput(" Merkez ", province, "Çankaya", "Atatürk Bulvarı 1", "0 (532) 111 22 33", "09:00-18:00", "https://maps.example.com/?q=1"), Ct));
        AuthoringResult nameOnly = await RunAsync((authoring, _) =>
            authoring.CreateBranchAsync(new BranchInput("Taslak", Guid.Empty, null, null, null, null, null), Ct));

        Assert.Equal(AuthoringOutcome.Created, full.Outcome);
        Assert.Equal(AuthoringOutcome.Created, nameOnly.Outcome);
        Assert.Equal("Merkez", await ScalarAsync<string>($"SELECT name FROM branches WHERE id = '{full.Id}'"));
        Assert.Equal(province, await ScalarAsync<Guid>($"SELECT province_id FROM branches WHERE id = '{full.Id}'"));
        Assert.Equal("5321112233", await ScalarAsync<string>($"SELECT phone FROM branches WHERE id = '{full.Id}'"));
        Assert.Equal(DBNull.Value, await ScalarAsync<object>($"SELECT province_id FROM branches WHERE id = '{nameOnly.Id}'"));
    }

    [Fact]
    public async Task ABranchEdit_WithABadMapLinkOrPhone_IsRefused_AndKeepsTheRow_KK7()
    {
        AuthoringResult created = await RunAsync((authoring, _) =>
            authoring.CreateBranchAsync(new BranchInput("Merkez", null, null, null, null, null, "https://maps.example.com/a"), Ct));

        AuthoringResult result = await RunAsync((authoring, _) =>
            authoring.SaveBranchAsync(created.Id!.Value, new BranchInput("Merkez", null, null, null, "ara", null, "javascript:alert(1)"), created.Version!.Value, Ct));

        Assert.Equal(AuthoringOutcome.Rejected, result.Outcome);
        Assert.Equal(
            [new FieldRejection(nameof(BranchInput.Phone), FieldCodes.InvalidPhone), new FieldRejection(nameof(BranchInput.MapUrl), FieldCodes.InvalidUrl)],
            result.Rejections);
        Assert.Equal("https://maps.example.com/a", await ScalarAsync<string>($"SELECT map_url FROM branches WHERE id = '{created.Id}'"));
        Assert.Equal(1, await ScalarAsync<int>($"SELECT version FROM branches WHERE id = '{created.Id}'"));
    }

    // ---- KK8: the contact page is built from the parts ----

    [Fact]
    public async Task TheContactPage_ListsOnlyPublishedBranches_InTheFirmsOrder_WithTheProvinceNameAndTheMapLink_KK8()
    {
        Guid province = await SeedProvinceAsync("34", "İstanbul");
        AuthoringResult one = await RunAsync((authoring, _) =>
            authoring.CreateBranchAsync(new BranchInput("Birinci", province, "Kadıköy", "Bağdat Cd. 1", null, null, "https://maps.example.com/1"), Ct));
        AuthoringResult two = await RunAsync((authoring, _) =>
            authoring.CreateBranchAsync(new BranchInput("İkinci", province, "Beşiktaş", "Barbaros Blv. 2", null, null, null), Ct));
        await RunAsync((authoring, publishing) => publishing.PublishAsync(ContentRecordKind.Branch, two.Id!.Value, two.Version!.Value, Ct));
        await RunAsync((authoring, publishing) => publishing.PublishAsync(ContentRecordKind.Branch, one.Id!.Value, one.Version!.Value, Ct));
        AuthoringResult draft = await RunAsync((authoring, _) => authoring.CreateBranchAsync(new BranchInput("Taslak", null, null, null, null, null, null), Ct));

        await using AppDbContext db = NewContext();
        ContentReaders readers = new(db, FakeSettings(), new ProvinceList(db));
        ContactPageContent page = await readers.ReadContactPageAsync(Ct);

        Assert.True(page.HasContactForm);
        Assert.Equal(["Birinci", "İkinci"], page.Branches.Select(b => b.Name).ToArray());
        Assert.DoesNotContain(page.Branches, b => b.Id == draft.Id);
        Assert.All(page.Branches, b => Assert.Equal("İstanbul", b.ProvinceName));
        Assert.Equal(new MapLinkView("https://maps.example.com/1", "Haritada aç"), page.Branches[0].MapLink);
        Assert.Null(page.Branches[1].MapLink);
    }

    // ---- KK6: the one question list ----

    [Fact]
    public async Task TheFaq_IsOneFlatListOfPublishedQuestions_InTheFirmsOrder_KK6()
    {
        AuthoringResult q1 = await RunAsync((authoring, _) => authoring.CreateFaqItemAsync(new FaqItemInput("Birinci?", "<p>Evet</p>"), Ct));
        AuthoringResult q2 = await RunAsync((authoring, _) => authoring.CreateFaqItemAsync(new FaqItemInput("İkinci?", "<p>Hayır</p><script>x</script>"), Ct));
        await RunAsync((authoring, _) => authoring.CreateFaqItemAsync(new FaqItemInput("Taslak?", "<p>Belki</p>"), Ct));
        await RunAsync((authoring, publishing) => publishing.PublishAsync(ContentRecordKind.FaqItem, q2.Id!.Value, q2.Version!.Value, Ct));
        await RunAsync((authoring, publishing) => publishing.PublishAsync(ContentRecordKind.FaqItem, q1.Id!.Value, q1.Version!.Value, Ct));

        await using AppDbContext db = NewContext();
        ContentReaders readers = new(db, FakeSettings(), new ProvinceList(db));
        IReadOnlyList<FaqView> list = await readers.ListFaqAsync(Ct);

        Assert.Equal(["Birinci?", "İkinci?"], list.Select(f => f.Question).ToArray());
        Assert.DoesNotContain("script", list[1].AnswerHtml!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AQuestion_IsStoredAsTyped_AndNeedsOnlyTheQuestionToBeSaved_KK6()
    {
        AuthoringResult result = await RunAsync((authoring, _) => authoring.CreateFaqItemAsync(new FaqItemInput("  <i>İade?</i> ", null), Ct));

        Assert.Equal(AuthoringOutcome.Created, result.Outcome);
        Assert.Equal("<i>İade?</i>", await ScalarAsync<string>($"SELECT question FROM faq_items WHERE id = '{result.Id}'"));
        Assert.Equal(DBNull.Value, await ScalarAsync<object>($"SELECT answer FROM faq_items WHERE id = '{result.Id}'"));
    }

    // ---- KK9: the general page ----

    [Fact]
    public async Task TheGeneralPages_HaveNoCountLimit_AndEachGetsItsOwnAddress_KK9()
    {
        const int Count = 12;
        for (int i = 0; i < Count; i++)
        {
            AuthoringResult result = await RunAsync((authoring, _) => authoring.CreateCustomPageAsync(new CustomPageInput("Kampanya", null, i % 2 == 0), Ct));
            Assert.Equal(AuthoringOutcome.Created, result.Outcome);
        }

        Assert.Equal((long)Count, await ScalarAsync<long>("SELECT count(*) FROM custom_pages"));
        Assert.Equal((long)Count, await ScalarAsync<long>("SELECT count(DISTINCT slug) FROM custom_pages"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM custom_pages WHERE slug = 'kampanya'"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM custom_pages WHERE slug = 'kampanya-12'"));
    }

    [Fact]
    public async Task AGeneralPageWithoutATitle_IsRefused_KK9()
    {
        AuthoringResult result = await RunAsync((authoring, _) => authoring.CreateCustomPageAsync(new CustomPageInput("  ", "<p>m</p>", false), Ct));

        Assert.Equal(AuthoringOutcome.Rejected, result.Outcome);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM custom_pages"));
    }

    // ---- KK10: the social links ----

    [Fact]
    public async Task TheSocialLinks_AreAddedUpdatedAndDeleted_WithOneRowPerPlatform_KK10()
    {
        await SaveLinksAsync(new Dictionary<string, string?> { ["Instagram"] = "https://instagram.com/firma", ["X"] = "https://x.com/firma" });
        Assert.Equal(2L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));

        await SaveLinksAsync(new Dictionary<string, string?> { ["Instagram"] = "https://instagram.com/yeni", ["X"] = "  " });
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
        Assert.Equal("https://instagram.com/yeni", await ScalarAsync<string>("SELECT url FROM social_links"));

        await SaveLinksAsync(new Dictionary<string, string?> { ["Facebook"] = "https://facebook.com/firma" });
        await using AppDbContext db = NewContext();
        IReadOnlyList<SocialLinkView> list = await new SocialLinks(db, TimeProvider.System).ListAsync(Ct);
        Assert.Equal([SocialPlatform.Instagram, SocialPlatform.Facebook], list.Select(l => l.Platform).ToArray());
    }

    [Fact]
    public async Task ASocialSaveWithAnUnknownPlatformOrABadLink_IsRefusedWhole_AndWritesNothing_KK10()
    {
        SocialLinksResult result = await RunSocialAsync(links => links.SaveAsync(
            new Dictionary<string, string?> { ["Instagram"] = "https://instagram.com/firma", ["MySpace"] = "https://myspace.com/x", ["TikTok"] = "tiktok.com/x", ["1"] = "https://a.example.com" },
            Ct));

        Assert.True(result.IsRejected);
        Assert.Equal(
            [
                new FieldRejection("MySpace", FieldCodes.InvalidListValue),
                new FieldRejection("TikTok", FieldCodes.InvalidUrl),
                new FieldRejection("1", FieldCodes.InvalidListValue),
            ],
            result.Rejections);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM social_links"));
    }

    [Fact]
    public async Task TheSocialLinksTable_HoldsAtMostOneRowPerPlatform_AtTheDatabase_KK10()
    {
        await SaveLinksAsync(new Dictionary<string, string?> { ["YouTube"] = "https://youtube.com/@firma" });

        await Assert.ThrowsAsync<PostgresException>(() => ExecuteAppAsync(
            $"INSERT INTO social_links (id, created_at, social_platform, url) VALUES ('{Guid.CreateVersion7()}', now(), 'YouTube', 'https://youtube.com/@baska')"));
    }

    // ---- helpers ----

    /// <summary>The settings module as the contact page reads it: only the legal identity is answered; any other call is a test mistake.</summary>
    public class SettingsProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(ISettingsModule.GetLegalIdentityAsync))
            {
                return Task.FromResult(new LegalIdentity(null, "Firma", null, null, null, null, null, null, null, null, null, null, null, null, null));
            }

            throw new NotSupportedException(targetMethod?.Name);
        }
    }

    private static ISettingsModule FakeSettings() => DispatchProxy.Create<ISettingsModule, SettingsProxy>();

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, AppDsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<T> RunAsync<T>(Func<ContentAuthoring, ContentPublishing, Task<T>> work)
        where T : IOperationResult
    {
        await using AppDbContext db = NewContext();
        CacheInvalidations invalidations = new(db);
        ContentPublishing publishing = new(db, invalidations, TimeProvider.System);
        ContentAuthoring authoring = new(db, publishing, new ProvinceList(db), TimeProvider.System);
        TransactionRunner runner = new(db, new JobEnqueueTracker(), new JobSignal(), invalidations, new AppCache());
        return await runner.RunAsync(cancel => work(authoring, publishing), Ct);
    }

    private async Task<SocialLinksResult> RunSocialAsync(Func<SocialLinks, Task<SocialLinksResult>> work)
    {
        await using AppDbContext db = NewContext();
        CacheInvalidations invalidations = new(db);
        SocialLinks links = new(db, TimeProvider.System);
        TransactionRunner runner = new(db, new JobEnqueueTracker(), new JobSignal(), invalidations, new AppCache());
        return await runner.RunAsync(cancel => work(links), Ct);
    }

    private async Task SaveLinksAsync(Dictionary<string, string?> links)
    {
        SocialLinksResult result = await RunSocialAsync(s => s.SaveAsync(links, Ct));
        Assert.False(result.IsRejected);
    }

    private async Task<Guid> SeedProvinceAsync(string code, string name)
    {
        await using AppDbContext db = NewContext();
        Province province = Province.Create(code, name, Now);
        db.Provinces.Add(province);
        await db.SaveChangesAsync(Ct);
        return province.Id;
    }

    private Task<string> SlugAsync(string table, Guid id) => ScalarAsync<string>($"SELECT slug FROM {table} WHERE id = '{id}'");

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

    private async Task ExecuteAppAsync(string sql)
    {
        await using NpgsqlConnection connection = new(AppDsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
