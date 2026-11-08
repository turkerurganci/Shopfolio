using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Validation;
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

using FieldRejection = Shopfolio.Infrastructure.Validation.FieldRejection;

namespace Shopfolio.IntegrationTests.CorporateContent;

/// <summary>The collection of the announcement, trail, preview and deletion tests (T360): one database copied from the template.</summary>
[CollectionDefinition(AnnouncementAndPreviewDefinition.Name)]
public sealed class AnnouncementAndPreviewDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "AnnouncementAndPreview";
}

/// <summary>
/// The announcement strip, the status trail, the preview and the permanent deletion of the corporate content on a real PostgreSQL (T360 criteria 1, 3-8; KK1, KK3-KK7; 02 3.27.21-3.27.28;
/// P-30): the one announcement that shows only inside its dates, a status change that writes one trail row and a text edit that writes none, the draft that is not found by a visitor and is
/// marked for the administrator in the place it stands, and the deletion that takes the menu place, the home page mark and the product links with the record.
/// </summary>
[Collection(AnnouncementAndPreviewDefinition.Name)]
public sealed class AnnouncementAndPreviewTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private readonly MovableTime _time = new(Now);

    private ServiceProvider _provider = null!;

    private Admin? _admin;

    private int _counter;

    public AnnouncementAndPreviewTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE audit_logs, cart_items, carts, order_items, orders, variants, products, categories, admins, about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements, images, provinces CASCADE");
        await ExecuteAsync(
            "UPDATE company_settings SET homepage_layout = 'ContentFirst', menu_name_products = 'Ürünler', menu_name_offerings = 'Hizmetlerimiz', "
            + "menu_name_portfolio_items = 'Referanslarımız', menu_name_about_page = 'Hakkımızda', menu_name_faq_items = 'SSS', menu_name_contact = 'İletişim'");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Now);
        await SaveAsync(c => c.Admins.Add(_admin));
        _time.Set(Now);

        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_time);
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
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1: the announcement is one, plain, within P-30 and inside its dates ----

    [Fact]
    public async Task AnAnnouncement_IsBornADraft_AndOnlyTheAdministratorSeesItWithTheBand_KK1_KK5()
    {
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Bayram tatili", "https://example.com/duyuru", null, null), Ct));

        Assert.Equal(AuthoringOutcome.Created, created.Outcome);
        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM announcements"));
        Assert.Null(await AnnouncementAsync(asAdministrator: false));
        AnnouncementView preview = (await AnnouncementAsync(asAdministrator: true))!;
        Assert.Equal(new AnnouncementView("Bayram tatili", "https://example.com/duyuru", IsDraft: true), preview);
    }

    [Fact]
    public async Task APublishedAnnouncement_IsOnTheStrip_ForEveryone_WithoutTheBand_AndTheTextIsNotChanged_KK1()
    {
        const string typed = "<b>Yarın</b> kapalıyız & <i>açılış</i> 09:00";
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput(typed, null, null, null), Ct));
        await PublishAsync(ContentRecordKind.Announcement, created.Id!.Value, created.Version!.Value);

        AnnouncementView visitor = (await AnnouncementAsync(asAdministrator: false))!;
        AnnouncementView admin = (await AnnouncementAsync(asAdministrator: true))!;

        Assert.Equal(new AnnouncementView(typed, null, IsDraft: false), visitor);
        Assert.Equal(visitor, admin);
    }

    [Fact]
    public async Task ATextOverP30_OrALinkOfTheWrongForm_IsNotSaved_AndTheRowIsNotBorn_KK1()
    {
        AuthoringResult longText = await AuthorAsync((a, _) =>
            a.CreateAnnouncementAsync(new AnnouncementInput(new string('x', ContentRules.AnnouncementTextMaxLength + 1), null, null, null), Ct));
        AuthoringResult badLink = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", "javascript:alert(1)", null, null), Ct));
        AuthoringResult badRange = await AuthorAsync((a, _) =>
            a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", null, new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 11)), Ct));

        Assert.Equal([new FieldRejection(nameof(AnnouncementInput.Text), FieldCodes.TextTooLong)], longText.Rejections);
        Assert.Equal([new FieldRejection(nameof(AnnouncementInput.LinkUrl), FieldCodes.InvalidUrl)], badLink.Rejections);
        Assert.Equal([new FieldRejection(nameof(AnnouncementInput.EndDate), FieldCodes.InvalidDateRange)], badRange.Rejections);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM announcements"));
    }

    [Fact]
    public async Task ATextOfExactlyP30Characters_IsSaved_AndAnEditThatGoesOverIsNotSaved_KK1()
    {
        string atCeiling = new('ş', ContentRules.AnnouncementTextMaxLength);
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput(atCeiling, null, null, null), Ct));

        AuthoringResult over = await AuthorAsync((a, _) =>
            a.SaveAnnouncementAsync(new AnnouncementInput(atCeiling + "x", null, null, null), created.Version!.Value, Ct));

        Assert.Equal(AuthoringOutcome.Created, created.Outcome);
        Assert.Equal(AuthoringOutcome.Rejected, over.Outcome);
        Assert.Equal(atCeiling, await ScalarAsync<string>("SELECT text FROM announcements"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT version FROM announcements"));
    }

    [Fact]
    public async Task TheStrip_ShowsOnlyInsideTheDateRange_BothEndDaysInclusive_OnTheTurkeyDay_KK1()
    {
        DateOnly start = new(2026, 11, 10);
        DateOnly end = new(2026, 11, 12);
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Kampanya", null, start, end), Ct));
        await PublishAsync(ContentRecordKind.Announcement, created.Id!.Value, created.Version!.Value);
        TimeSpan turkey = TimeSpan.FromHours(3);

        _time.Set(new DateTimeOffset(2026, 11, 9, 23, 59, 0, turkey));
        Assert.Null(await AnnouncementAsync(false));
        Assert.Null(await AnnouncementAsync(true));

        _time.Set(new DateTimeOffset(2026, 11, 10, 0, 0, 0, turkey));
        Assert.NotNull(await AnnouncementAsync(false));

        _time.Set(new DateTimeOffset(2026, 11, 12, 23, 59, 0, turkey));
        Assert.NotNull(await AnnouncementAsync(false));

        _time.Set(new DateTimeOffset(2026, 11, 13, 0, 0, 0, turkey));
        Assert.Null(await AnnouncementAsync(false));
        Assert.Null(await AnnouncementAsync(true));
        Assert.Equal("Published", await ScalarAsync<string>("SELECT publish_status FROM announcements"));
    }

    [Fact]
    public async Task ADraftAnnouncement_IsPreviewedWhateverItsDates_AndAnEmptyOneHasNothingToShow_KK1_KK5()
    {
        await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Gelecek duyuru", null, new DateOnly(2027, 1, 1), null), Ct));

        Assert.Null(await AnnouncementAsync(false));
        Assert.True((await AnnouncementAsync(true))!.IsDraft);

        await AuthorAsync((a, _) => a.SaveAnnouncementAsync(new AnnouncementInput("  ", null, null, null), 1, Ct));
        Assert.Null(await AnnouncementAsync(true));
    }

    [Fact]
    public async Task TheAnnouncementIsOne_ASecondFirstSaveIsRefused_AndAfterTheDeletionANewOneMayBeOpened_KK1()
    {
        AuthoringResult first = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Birinci", null, null, null), Ct));
        AuthoringResult second = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("İkinci", null, null, null), Ct));
        ContentResult deleted = await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.Announcement, first.Id!.Value, true, Ct));
        AuthoringResult third = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Üçüncü", null, null, null), Ct));

        Assert.Equal(AuthoringOutcome.AlreadyExists, second.Outcome);
        Assert.Equal(ContentOutcome.Deleted, deleted.Outcome);
        Assert.Equal(AuthoringOutcome.Created, third.Outcome);
        Assert.Equal("Üçüncü", await ScalarAsync<string>("SELECT text FROM announcements"));
    }

    [Fact]
    public async Task TwoFirstSavesOfTheAnnouncementAtTheSameTime_LeaveOneRow_TheSingleRowIndexRefusesTheOther_KK1()
    {
        Task<Exception?>[] runs =
        [
            .. Enumerable.Range(0, 2).Select(async n =>
            {
                try
                {
                    await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput($"Duyuru {n}", null, null, null), Ct));
                    return (Exception?)null;
                }
                catch (DbUpdateException ex)
                {
                    return ex;
                }
            }),
        ];

        await Task.WhenAll(runs);

        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM announcements"));
    }

    // ---- KK3: the way back to draft; no second approval ----

    [Fact]
    public async Task AnyOneAdministrator_Publishes_AndTheWayBackIsToDraft_WithoutAnArchive_KK3()
    {
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", null, null, null), Ct));

        ContentResult published = await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Announcement, created.Id!.Value, 1, Ct));
        ContentResult back = await PublishingAsync((p, _) => p.UnpublishAsync(ContentRecordKind.Announcement, created.Id!.Value, 2, Ct));

        Assert.Equal(ContentOutcome.Changed, published.Outcome);
        Assert.Equal(ContentOutcome.Changed, back.Outcome);
        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM announcements"));
        Assert.Null(await AnnouncementAsync(false));
    }

    // ---- KK4: save as draft with little, publish asks the gate ----

    [Fact]
    public async Task AnEmptyAnnouncementAndAnEmptyAboutPage_AreSavedAsDrafts_AndPublishingThemIsRefusedWithTheMissingField_KK4()
    {
        AuthoringResult announcement = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput(null, null, null, null), Ct));
        AuthoringResult about = await AuthorAsync((a, _) => a.CreateAboutPageAsync(new AboutPageInput(null, null), Ct));

        ContentResult refusedAnnouncement = await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Announcement, announcement.Id!.Value, 1, Ct));
        ContentResult refusedAbout = await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.AboutPage, about.Id!.Value, 1, Ct));

        Assert.Equal([ContentGateField.Text], refusedAnnouncement.Missing);
        Assert.Equal([ContentGateField.ShortIntroduction], refusedAbout.Missing);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task ATitledReferenceDraft_AndASlugOnlyTitleDraft_AreSaved_AndPublishingAnEmptiedPublishedRecordIsNotSaved_KK4()
    {
        AuthoringResult reference = await AuthorAsync((a, _) => a.CreatePortfolioItemAsync(new PortfolioItemInput("Kurumsal site", null, null, false), Ct));
        AuthoringResult faq = await AuthorAsync((a, _) => a.CreateFaqItemAsync(new FaqItemInput("İade var mı?", null), Ct));
        await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.PortfolioItem, reference.Id!.Value, 1, Ct));

        AuthoringResult emptied = await AuthorAsync((a, _) => a.SavePortfolioItemAsync(reference.Id!.Value, new PortfolioItemInput("  ", null, null, false), 2, Ct));

        Assert.Equal(AuthoringOutcome.Created, faq.Outcome);
        Assert.Equal(AuthoringOutcome.Rejected, emptied.Outcome);
        Assert.Equal("Kurumsal site", await ScalarAsync<string>($"SELECT title FROM portfolio_items WHERE id = '{reference.Id}'"));
    }

    // ---- KK6: the trail ----

    [Fact]
    public async Task APublicationAndTheWayBack_WriteOneTrailRowEach_WithWhoAndTheOldAndNewStatus_KK6()
    {
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateOfferingAsync(new OfferingInput("Bakım", null, null, false), Ct));

        await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Offering, created.Id!.Value, 1, Ct));
        await PublishingAsync((p, _) => p.UnpublishAsync(ContentRecordKind.Offering, created.Id!.Value, 2, Ct));

        await using AppDbContext reader = NewContext();
        List<AuditLog> rows = await reader.AuditLogs.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id).ToListAsync(Ct);
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal(nameof(AuditOperation.ChangePublishStatus), row.Operation);
            Assert.Equal(nameof(Offering), row.TargetRecordType);
            Assert.Equal(created.Id, row.TargetRecordId);
            Assert.Equal(_admin!.Id, row.AdminId);
            Assert.Equal("Bakım", row.Details!.DisplayName);
        });
        Assert.Equal(("PublishStatus", "Draft", "Published"), Pair(rows[0]));
        Assert.Equal(("PublishStatus", "Published", "Draft"), Pair(rows[1]));
    }

    [Fact]
    public async Task EveryKindOfRecord_WritesItsStatusChangeIntoTheTrail_UnderItsOwnKindName_KK6()
    {
        AuthoringResult aboutResult = await AuthorAsync((a, _) => a.CreateAboutPageAsync(new AboutPageInput("Biz kimiz", null), Ct));
        Guid about = aboutResult.Id!.Value;
        (ContentRecordKind Kind, Guid Id)[] records =
        [
            (ContentRecordKind.AboutPage, about),
            (ContentRecordKind.Offering, (await AuthorAsync((a, _) => a.CreateOfferingAsync(new OfferingInput("Hizmet", null, null, false), Ct))).Id!.Value),
            (ContentRecordKind.PortfolioItem, (await AuthorAsync((a, _) => a.CreatePortfolioItemAsync(new PortfolioItemInput("Referans", null, null, false), Ct))).Id!.Value),
            (ContentRecordKind.FaqItem, (await AuthorAsync((a, _) => a.CreateFaqItemAsync(new FaqItemInput("Soru?", "<p>Cevap</p>"), Ct))).Id!.Value),
            (ContentRecordKind.CustomPage, (await AuthorAsync((a, _) => a.CreateCustomPageAsync(new CustomPageInput("Sayfa", null, false), Ct))).Id!.Value),
            (ContentRecordKind.Announcement, (await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", null, null, null), Ct))).Id!.Value),
        ];

        foreach ((ContentRecordKind kind, Guid id) in records)
        {
            ContentResult result = await PublishingAsync((p, _) => p.PublishAsync(kind, id, 1, Ct));
            Assert.Equal(ContentOutcome.Changed, result.Outcome);
        }

        await using AppDbContext reader = NewContext();
        List<AuditLog> rows = await reader.AuditLogs.ToListAsync(Ct);
        Assert.Equal(records.Select(r => r.Kind.ToString()).Order(), rows.Select(r => r.TargetRecordType!).Order());
        foreach ((ContentRecordKind kind, Guid id) in records)
        {
            Assert.Equal(id, Assert.Single(rows, r => r.TargetRecordType == kind.ToString()).TargetRecordId);
        }
    }

    [Fact]
    public async Task ATextEdit_OfAPublishedAndOfADraftRecord_WritesNoTrailRow_AndNeitherDoesTheDeletion_KK6()
    {
        AuthoringResult offering = await AuthorAsync((a, _) => a.CreateOfferingAsync(new OfferingInput("Bakım", null, null, false), Ct));
        AuthoringResult announcement = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", null, null, null), Ct));
        await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Offering, offering.Id!.Value, 1, Ct));
        long afterPublish = await ScalarAsync<long>("SELECT count(*) FROM audit_logs");

        AuthoringResult published = await AuthorAsync((a, _) =>
            a.SaveOfferingAsync(offering.Id!.Value, new OfferingInput("Bakım ve onarım", "kısa", "<p>uzun</p>", true), 2, Ct));
        AuthoringResult draft = await AuthorAsync((a, _) => a.SaveAnnouncementAsync(new AnnouncementInput("Yeni duyuru", null, null, null), 1, Ct));
        await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.Announcement, announcement.Id!.Value, true, Ct));

        Assert.Equal(AuthoringOutcome.Saved, published.Outcome);
        Assert.Equal(AuthoringOutcome.Saved, draft.Outcome);
        Assert.Equal(1L, afterPublish);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task ARepeatedPublication_AndARefusedOne_WriteNoTrailRow_KK6()
    {
        AuthoringResult complete = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", null, null, null), Ct));
        await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Announcement, complete.Id!.Value, 1, Ct));

        ContentResult again = await PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Announcement, complete.Id!.Value, 2, Ct));
        ContentResult stale = await PublishingAsync((p, _) => p.UnpublishAsync(ContentRecordKind.Announcement, complete.Id!.Value, 1, Ct));

        Assert.Equal(ContentOutcome.Unchanged, again.Outcome);
        Assert.Equal(ContentOutcome.VersionConflict, stale.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task APublicationWhoseTrailRowCannotBeWritten_IsRolledBack_TheRecordStaysADraft_KK6()
    {
        AuthoringResult created = await AuthorAsync((a, _) => a.CreateAnnouncementAsync(new AnnouncementInput("Duyuru", null, null, null), Ct));
        _admin = null;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => PublishingAsync((p, _) => p.PublishAsync(ContentRecordKind.Announcement, created.Id!.Value, 1, Ct)));

        Assert.Equal("Draft", await ScalarAsync<string>("SELECT publish_status FROM announcements"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT version FROM announcements"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM audit_logs"));
    }

    // ---- KK5: a draft is not found by a visitor and is marked for the administrator ----

    [Fact]
    public async Task ADraftServiceReferenceAndGeneralPage_AreNotFoundByAVisitor_AndAreMarkedForTheAdministrator_KK5()
    {
        await SeedOfferingAsync("Taslak hizmet", "taslak-hizmet", PublishStatus.Draft);
        await SeedOfferingAsync("Yayın hizmet", "yayin-hizmet", PublishStatus.Published);
        await SeedPortfolioAsync("Taslak referans", "taslak-referans", PublishStatus.Draft);
        await SeedCustomPageAsync("Taslak sayfa", "taslak-sayfa", PublishStatus.Draft);

        Assert.Null(await FindPageAsync(ContentRecordKind.Offering, "taslak-hizmet", ContentAudience.Visitor));
        Assert.Null(await FindPageAsync(ContentRecordKind.PortfolioItem, "taslak-referans", ContentAudience.Visitor));
        Assert.Null(await FindPageAsync(ContentRecordKind.CustomPage, "taslak-sayfa", ContentAudience.Visitor));
        Assert.True((await FindPageAsync(ContentRecordKind.Offering, "taslak-hizmet", ContentAudience.Administrator))!.IsDraft);
        Assert.True((await FindPageAsync(ContentRecordKind.PortfolioItem, "taslak-referans", ContentAudience.Administrator))!.IsDraft);
        Assert.True((await FindPageAsync(ContentRecordKind.CustomPage, "taslak-sayfa", ContentAudience.Administrator))!.IsDraft);
        Assert.False((await FindPageAsync(ContentRecordKind.Offering, "yayin-hizmet", ContentAudience.Visitor))!.IsDraft);
        Assert.False((await FindPageAsync(ContentRecordKind.Offering, "yayin-hizmet", ContentAudience.Administrator))!.IsDraft);
        Assert.Null(await FindPageAsync(ContentRecordKind.Offering, "olmayan-adres", ContentAudience.Administrator));
    }

    [Fact]
    public async Task ADraftAboutPage_IsNotFoundByAVisitor_AndIsMarkedForTheAdministrator_KK5()
    {
        await SeedAboutAsync("Biz kimiz", PublishStatus.Draft);

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentPageReader reader = scope.ServiceProvider.GetRequiredService<ContentPageReader>();
        Assert.Null(await reader.FindAboutPageAsync(ContentAudience.Visitor, Ct));
        ContentPageView preview = (await reader.FindAboutPageAsync(ContentAudience.Administrator, Ct))!;
        Assert.True(preview.IsDraft);
        Assert.Equal("Biz kimiz", preview.Summary);
    }

    [Fact]
    public async Task ADraftQuestionAndADraftBranch_StandInTheirPlace_MarkedForTheAdministrator_AndAreAbsentForAVisitor_KK5()
    {
        await SeedFaqAsync("Birinci soru", 1, PublishStatus.Published);
        await SeedFaqAsync("Taslak soru", 2, PublishStatus.Draft);
        await SeedFaqAsync("Üçüncü soru", 3, PublishStatus.Published);
        await SeedBranchAsync("Merkez", 1, PublishStatus.Published);
        await SeedBranchAsync("Taslak şube", 2, PublishStatus.Draft);

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentReaders readers = scope.ServiceProvider.GetRequiredService<ContentReaders>();
        IReadOnlyList<FaqView> visitorFaq = await readers.ListFaqAsync(Ct);
        IReadOnlyList<FaqView> adminFaq = await readers.ListFaqForAdministratorAsync(Ct);
        ContactPageContent visitorPage = await readers.ReadContactPageAsync(Ct);
        ContactPageContent adminPage = await readers.ReadContactPageForAdministratorAsync(Ct);

        Assert.Equal(["Birinci soru", "Üçüncü soru"], visitorFaq.Select(f => f.Question));
        Assert.All(visitorFaq, f => Assert.False(f.IsDraft));
        Assert.Equal(["Birinci soru", "Taslak soru", "Üçüncü soru"], adminFaq.Select(f => f.Question));
        Assert.Equal([false, true, false], adminFaq.Select(f => f.IsDraft));
        Assert.Equal(["Merkez"], visitorPage.Branches.Select(b => b.Name));
        Assert.Equal(["Merkez", "Taslak şube"], adminPage.Branches.Select(b => b.Name));
        Assert.Equal([false, true], adminPage.Branches.Select(b => b.IsDraft));
    }

    // ---- KK7: the permanent deletion takes the menu place, the home page mark and the product links ----

    [Fact]
    public async Task DeletingTheLastPublishedServiceIntroduction_TakesItsMenuPlace_ItsHomePageMark_AndItsProductLinks_KK7()
    {
        Guid product = await SeedProductAsync("Masa");
        Guid offering = await SeedOfferingAsync("Montaj", "montaj", PublishStatus.Published, flagged: true);
        await SaveAsync(c => c.Add(OfferingProduct.Create(offering, product, 1, Now)));
        Assert.True((await MenuAsync()).Items.Single(i => i.Kind == MenuItemKind.Offerings).IsVisible);
        Assert.Equal(["Montaj"], (await HomeAsync()).Offerings!.Cards.Select(c => c.Title));

        ContentResult result = await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.Offering, offering, true, Ct));

        Assert.Equal(ContentOutcome.Deleted, result.Outcome);
        Assert.False((await MenuAsync()).Items.Single(i => i.Kind == MenuItemKind.Offerings).IsVisible);
        Assert.Null((await HomeAsync()).Offerings);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM offering_products"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
        Assert.Null(await FindPageAsync(ContentRecordKind.Offering, "montaj", ContentAudience.Administrator));
        Assert.Null(await FindPageAsync(ContentRecordKind.Offering, "montaj", ContentAudience.Visitor));
    }

    [Fact]
    public async Task DeletingAReferenceWork_TakesItsHomePageMark_ItsProductLinks_AndItsPage_KK7()
    {
        Guid product = await SeedProductAsync("Sandalye");
        Guid first = await SeedPortfolioAsync("Birinci iş", "birinci-is", PublishStatus.Published, flagged: true);
        await SeedPortfolioAsync("İkinci iş", "ikinci-is", PublishStatus.Published, flagged: true);
        await SaveAsync(c => c.Add(PortfolioItemProduct.Create(first, product, 1, Now)));
        Assert.Equal(["Birinci iş", "İkinci iş"], (await HomeAsync()).PortfolioItems!.Cards.Select(c => c.Title).Order());

        await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.PortfolioItem, first, true, Ct));

        Assert.Equal(["İkinci iş"], (await HomeAsync()).PortfolioItems!.Cards.Select(c => c.Title));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM portfolio_item_products"));
        Assert.Null(await FindPageAsync(ContentRecordKind.PortfolioItem, "birinci-is", ContentAudience.Administrator));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM products"));
    }

    [Fact]
    public async Task DeletingAGeneralPage_TakesItOutOfTheFooterAndTheMenu_AndTheQuestionAndTheBranchOutOfTheirLists_KK7()
    {
        await SeedCustomPageAsync("Kargo bilgisi", "kargo-bilgisi", PublishStatus.Published, inMenu: true);
        await SeedFaqAsync("Kargo ücreti?", 1, PublishStatus.Published);
        await SeedBranchAsync("Merkez", 1, PublishStatus.Published);
        Guid page = await ScalarAsync<Guid>("SELECT id FROM custom_pages");
        Guid faq = await ScalarAsync<Guid>("SELECT id FROM faq_items");
        Guid branch = await ScalarAsync<Guid>("SELECT id FROM branches");
        Assert.Equal(["Kargo bilgisi"], (await MenuAsync()).FooterPages.Select(p => p.Title));

        await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.CustomPage, page, true, Ct));
        await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.FaqItem, faq, true, Ct));
        await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.Branch, branch, true, Ct));

        StorefrontMenu menu = await MenuAsync();
        Assert.Empty(menu.FooterPages);
        Assert.DoesNotContain(menu.VisibleItems, i => i.Kind == MenuItemKind.GeneralPage);
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentReaders readers = scope.ServiceProvider.GetRequiredService<ContentReaders>();
        Assert.Empty(await readers.ListFaqForAdministratorAsync(Ct));
        Assert.Empty((await readers.ReadContactPageForAdministratorAsync(Ct)).Branches);
    }

    [Fact]
    public async Task AWarningThatIsNotConfirmed_DeletesNothing_AndTheAboutPageIsNeverDeleted_KK7()
    {
        Guid offering = await SeedOfferingAsync("Montaj", "montaj", PublishStatus.Published, flagged: true);
        Guid about = await SeedAboutAsync("Biz kimiz", PublishStatus.Published);

        ContentResult unconfirmed = await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.Offering, offering, false, Ct));
        ContentResult aboutDelete = await PublishingAsync((p, _) => p.DeleteAsync(ContentRecordKind.AboutPage, about, true, Ct));

        Assert.Equal(ContentOutcome.ConfirmationRequired, unconfirmed.Outcome);
        Assert.Equal(ContentOutcome.Forbidden, aboutDelete.Outcome);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM offerings"));
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM about_pages"));
    }

    // ---- helpers: running ----

    private async Task<AuthoringResult> AuthorAsync(Func<ContentAuthoring, ContentPublishing, Task<AuthoringResult>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentAuthoring authoring = scope.ServiceProvider.GetRequiredService<ContentAuthoring>();
        ContentPublishing publishing = scope.ServiceProvider.GetRequiredService<ContentPublishing>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(authoring, publishing), Ct);
    }

    private async Task<ContentResult> PublishingAsync(Func<ContentPublishing, CancellationToken, Task<ContentResult>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentPublishing publishing = scope.ServiceProvider.GetRequiredService<ContentPublishing>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(publishing, token), Ct);
    }

    private async Task PublishAsync(ContentRecordKind kind, Guid id, int version)
    {
        ContentResult result = await PublishingAsync((p, token) => p.PublishAsync(kind, id, version, token));
        Assert.Equal(ContentOutcome.Changed, result.Outcome);
    }

    // ---- helpers: reading ----

    private async Task<AnnouncementView?> AnnouncementAsync(bool asAdministrator)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().GetAnnouncementAsync(asAdministrator, Ct);
    }

    private async Task<ContentPageView?> FindPageAsync(ContentRecordKind kind, string slug, ContentAudience audience)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ContentPageReader>().FindAsync(kind, slug, audience, Ct);
    }

    private async Task<HomepageView> HomeAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().GetHomepageAsync(Ct);
    }

    private async Task<StorefrontMenu> MenuAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICorporateContentModule>().GetMenuAsync(Ct);
    }

    private static (string Field, string? Old, string? New) Pair(AuditLog row)
    {
        AuditValueChange change = Assert.Single(row.Details!.Changes);
        return (change.Field, change.OldValue, change.NewValue);
    }

    // ---- helpers: seeding ----

    private async Task<Guid> SeedProductAsync(string name)
    {
        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create("Mobilya", $"kategori-{n:D3}", null, 1, Now.AddMinutes(n));
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        await SaveAsync(c =>
        {
            c.Categories.Add(category);
            c.Products.Add(product);
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
        });
        return product.Id;
    }

    private async Task<Guid> SeedAboutAsync(string shortIntroduction, PublishStatus status)
    {
        AboutPage page = AboutPage.Create(Now);
        page.ShortIntroduction = shortIntroduction;
        page.PublishStatus = status;
        await SaveAsync(c => c.AboutPages.Add(page));
        return page.Id;
    }

    private async Task<Guid> SeedOfferingAsync(string name, string slug, PublishStatus status, bool flagged = false)
    {
        Offering offering = Offering.Create(name, slug, Interlocked.Increment(ref _counter), Now);
        offering.PublishStatus = status;
        offering.IsShownOnHomepage = flagged;
        await SaveAsync(c => c.Offerings.Add(offering));
        return offering.Id;
    }

    private async Task<Guid> SeedPortfolioAsync(string title, string slug, PublishStatus status, bool flagged = false)
    {
        PortfolioItem item = PortfolioItem.Create(title, slug, Interlocked.Increment(ref _counter), Now);
        item.PublishStatus = status;
        item.IsShownOnHomepage = flagged;
        await SaveAsync(c => c.PortfolioItems.Add(item));
        return item.Id;
    }

    private async Task SeedCustomPageAsync(string title, string slug, PublishStatus status, bool inMenu = false)
    {
        CustomPage page = CustomPage.Create(title, slug, Interlocked.Increment(ref _counter), Now);
        page.PublishStatus = status;
        page.IsShownInMenu = inMenu;
        await SaveAsync(c => c.CustomPages.Add(page));
    }

    private async Task SeedFaqAsync(string question, int order, PublishStatus status)
    {
        FaqItem item = FaqItem.Create(question, order, Now);
        item.PublishStatus = status;
        await SaveAsync(c => c.FaqItems.Add(item));
    }

    private async Task SeedBranchAsync(string name, int order, PublishStatus status)
    {
        Branch branch = Branch.Create(name, order, Now);
        branch.PublishStatus = status;
        await SaveAsync(c => c.Branches.Add(branch));
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

    private sealed class FixedActor(Admin? admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) =>
            Task.FromResult(admin is null ? null : new AuditActor(admin.Id, admin.Name, admin.Email));
    }

    private sealed class MovableTime(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public void Set(DateTimeOffset value) => _now = value;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
