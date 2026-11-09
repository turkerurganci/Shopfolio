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

/// <summary>The collection of the content relation tests (T359): one database copied from the template.</summary>
[CollectionDefinition(ContentRelationsDefinition.Name)]
public sealed class ContentRelationsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ContentRelations";
}

/// <summary>
/// The relations, the manual order, the gate and the galleries of the corporate content on a real PostgreSQL (T359; KK1-KK6; 02 3.27.10, 3.27.11, 3.27.13-3.27.16): the products a service
/// introduction or a reference work links - drafts kept and hidden, deleted products gone, no variant - the move up and down with its stale-screen refusal and its equal-value renumbering,
/// the publish gate of every kind, the empty about page, and the galleries with their main mark, generated text and ceiling.
/// </summary>
[Collection(ContentRelationsDefinition.Name)]
public sealed class ContentRelationsTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _collection;

    private ServiceProvider _provider = null!;

    private Admin _admin = null!;

    private int _counter;

    public ContentRelationsTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE audit_logs, cart_items, carts, order_items, orders, variants, products, categories, admins, about_pages, offerings, portfolio_items, faq_items, branches, custom_pages, announcements, images CASCADE");
        await ExecuteAsync(
            "UPDATE company_settings SET homepage_layout = 'ContentFirst', menu_name_products = 'Ürünler', menu_name_offerings = 'Hizmetlerimiz', "
            + "menu_name_portfolio_items = 'Referanslarımız', menu_name_about_page = 'Hakkımızda', menu_name_faq_items = 'SSS', menu_name_contact = 'İletişim'");
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
        services.AddCorporateContentModule();
        services.AddSettingsModule();
        services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        services.AddScoped<IAuditTrail, AuditTrail>();
        _provider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1: the product link ----

    [Fact]
    public async Task ADraftProduct_StaysLinked_IsNotShown_AndComesBackWhenPublished_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya");
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid first = await SeedProductAsync("Masa", PublishStatus.Published, category);
        Guid draft = await SeedProductAsync("Sandalye", PublishStatus.Draft, category);
        Guid third = await SeedProductAsync("Dolap", PublishStatus.Published, category);

        ProductLinksResult saved = await LinkAsync(ContentLinkOwner.Offering, offering, [first, draft, third]);

        Assert.Equal(ProductLinksOutcome.Saved, saved.Outcome);
        Assert.Equal([first, draft, third], await LinkedIdsAsync(ContentLinkOwner.Offering, offering));
        Assert.Equal(["Masa", "Dolap"], (await RelatedAsync(ContentLinkOwner.Offering, offering)).Select(p => p.Name));

        await ChangeStatusAsync(draft, PublishStatus.Published, 1);
        Assert.Equal(["Masa", "Sandalye", "Dolap"], (await RelatedAsync(ContentLinkOwner.Offering, offering)).Select(p => p.Name));

        await ChangeStatusAsync(draft, PublishStatus.Draft, 2);
        Assert.Equal(["Masa", "Dolap"], (await RelatedAsync(ContentLinkOwner.Offering, offering)).Select(p => p.Name));
        Assert.Equal([first, draft, third], await LinkedIdsAsync(ContentLinkOwner.Offering, offering));
    }

    [Fact]
    public async Task AnArchivedProduct_StaysLinked_IsNotShown_AndTheReferenceWorkLinksTheSameWay_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya");
        Guid work = await SeedPortfolioAsync("Ofis", 1);
        Guid published = await SeedProductAsync("Masa", PublishStatus.Published, category);
        Guid archived = await SeedProductAsync("Eski masa", PublishStatus.Archived, category);

        ProductLinksResult saved = await LinkAsync(ContentLinkOwner.PortfolioItem, work, [archived, published]);

        Assert.Equal(ProductLinksOutcome.Saved, saved.Outcome);
        Assert.Equal([archived, published], await LinkedIdsAsync(ContentLinkOwner.PortfolioItem, work));
        Assert.Equal(["Masa"], (await RelatedAsync(ContentLinkOwner.PortfolioItem, work)).Select(p => p.Name));
    }

    [Fact]
    public async Task ADeletedProduct_GoesFromTheLink_AndFromTheRelatedList_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya");
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid work = await SeedPortfolioAsync("Ofis", 1);
        Guid table = await SeedProductAsync("Masa", PublishStatus.Published, category);
        Guid chair = await SeedProductAsync("Sandalye", PublishStatus.Published, category);
        await LinkAsync(ContentLinkOwner.Offering, offering, [table, chair]);
        await LinkAsync(ContentLinkOwner.PortfolioItem, work, [table, chair]);

        await DeleteProductAsync(table);

        Assert.Equal([chair], await LinkedIdsAsync(ContentLinkOwner.Offering, offering));
        Assert.Equal([chair], await LinkedIdsAsync(ContentLinkOwner.PortfolioItem, work));
        Assert.Equal(["Sandalye"], (await RelatedAsync(ContentLinkOwner.Offering, offering)).Select(p => p.Name));
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.OfferingProducts.CountAsync(Ct));
        Assert.Equal(1, await db.PortfolioItemProducts.CountAsync(Ct));
    }

    [Fact]
    public async Task TheSave_MakesTheLinksMatchTheList_TheOrderIsTheOrderOfTheChoice_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya");
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid a = await SeedProductAsync("A", PublishStatus.Published, category);
        Guid b = await SeedProductAsync("B", PublishStatus.Published, category);
        Guid c = await SeedProductAsync("C", PublishStatus.Published, category);

        await LinkAsync(ContentLinkOwner.Offering, offering, [a, b, c]);
        await LinkAsync(ContentLinkOwner.Offering, offering, [c, a]);

        Assert.Equal([c, a], await LinkedIdsAsync(ContentLinkOwner.Offering, offering));
        Assert.Equal(["C", "A"], (await RelatedAsync(ContentLinkOwner.Offering, offering)).Select(p => p.Name));

        await LinkAsync(ContentLinkOwner.Offering, offering, []);
        Assert.Empty(await LinkedIdsAsync(ContentLinkOwner.Offering, offering));
        Assert.Empty(await RelatedAsync(ContentLinkOwner.Offering, offering));
    }

    [Fact]
    public async Task ARefusedList_WritesNothing_ADuplicate_AnUnknownRecord_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya");
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid a = await SeedProductAsync("A", PublishStatus.Published, category);
        Guid b = await SeedProductAsync("B", PublishStatus.Published, category);
        await LinkAsync(ContentLinkOwner.Offering, offering, [a]);

        ProductLinksResult duplicate = await LinkAsync(ContentLinkOwner.Offering, offering, [b, b]);
        ProductLinksResult noOwner = await LinkAsync(ContentLinkOwner.Offering, Guid.CreateVersion7(), [a]);
        ProductLinksResult noWork = await LinkAsync(ContentLinkOwner.PortfolioItem, Guid.CreateVersion7(), [a]);

        Assert.Equal(ProductLinksOutcome.DuplicateProduct, duplicate.Outcome);
        Assert.Equal(ProductLinksOutcome.OwnerNotFound, noOwner.Outcome);
        Assert.Equal(ProductLinksOutcome.OwnerNotFound, noWork.Outcome);
        Assert.Equal([a], await LinkedIdsAsync(ContentLinkOwner.Offering, offering));
    }

    [Theory]
    [InlineData("Offering")]
    [InlineData("PortfolioItem")]
    public async Task ADeletedProductInTheList_IsSkipped_TheRestAreLinkedInTheOrderOfTheChoice_NotAnError_KK1(string ownerName)
    {
        ContentLinkOwner owner = Enum.Parse<ContentLinkOwner>(ownerName);
        Guid category = await SeedCategoryAsync("Mobilya");
        Guid record = owner == ContentLinkOwner.Offering ? await SeedOfferingAsync("Montaj", 1) : await SeedPortfolioAsync("Ofis", 1);
        Guid a = await SeedProductAsync("A", PublishStatus.Published, category);
        Guid b = await SeedProductAsync("B", PublishStatus.Published, category);
        Guid c = await SeedProductAsync("C", PublishStatus.Published, category);
        Guid gone = await SeedProductAsync("Gone", PublishStatus.Published, category);
        await DeleteProductAsync(gone);

        ProductLinksResult result = await LinkAsync(owner, record, [c, gone, a, Guid.CreateVersion7(), b]);

        Assert.Equal(ProductLinksOutcome.Saved, result.Outcome);
        Assert.Equal([c, a, b], await LinkedIdsAsync(owner, record));
        Assert.Equal(["C", "A", "B"], (await RelatedAsync(owner, record)).Select(p => p.Name));
    }

    [Fact]
    public async Task TheLinkTables_HaveNoVariantColumn_AndTheCatalogHoldsNoReverseLink_KK1()
    {
        foreach (string table in new[] { "offering_products", "portfolio_item_products" })
        {
            List<string> columns = await ColumnsOfAsync(table);
            Assert.NotEmpty(columns);
            Assert.DoesNotContain(columns, name => name.Contains("variant", StringComparison.Ordinal));
        }

        foreach (string table in new[] { "products", "variants" })
        {
            Assert.DoesNotContain(await ColumnsOfAsync(table), name => name.Contains("offering", StringComparison.Ordinal) || name.Contains("portfolio", StringComparison.Ordinal));
        }
    }

    // ---- KK2: the manual order ----

    [Theory]
    [InlineData("Offering")]
    [InlineData("PortfolioItem")]
    [InlineData("FaqItem")]
    [InlineData("Branch")]
    [InlineData("CustomPage")]
    public async Task AMoveUpAndDown_ChangesPlaces_OfTheTwoRecordsOnly_AndDoesNotRaiseTheVersion_KK2(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        Guid a = await SeedKindAsync(kind, "A", 1);
        Guid b = await SeedKindAsync(kind, "B", 2);
        Guid c = await SeedKindAsync(kind, "C", 3);
        int versionBefore = await VersionOfAsync(kind, c);

        OrderingResult up = await MoveAsync(kind, c, MoveDirection.Up, 3, 2);
        Assert.Equal(OrderingOutcome.Moved, up.Outcome);
        Assert.Equal(["A", "C", "B"], await NamesInOrderAsync(kind));
        Assert.Equal(versionBefore, await VersionOfAsync(kind, c));

        OrderingResult down = await MoveAsync(kind, a, MoveDirection.Down, 1, 2);
        Assert.Equal(OrderingOutcome.Moved, down.Outcome);
        Assert.Equal(["C", "A", "B"], await NamesInOrderAsync(kind));
        Assert.Equal("2, 3, 1", await OrdersAsync(kind, [a, b, c]));
    }

    [Fact]
    public async Task AStaleScreen_IsRefused_AndNothingIsWritten_TheSecondOfTwoMovesFromTheSameScreen_KK2()
    {
        Guid a = await SeedKindAsync(ContentRecordKind.Offering, "A", 1);
        Guid b = await SeedKindAsync(ContentRecordKind.Offering, "B", 2);
        await SeedKindAsync(ContentRecordKind.Offering, "C", 3);

        OrderingResult first = await MoveAsync(ContentRecordKind.Offering, b, MoveDirection.Up, 2, 1);
        OrderingResult second = await MoveAsync(ContentRecordKind.Offering, b, MoveDirection.Down, 2, 3);
        OrderingResult wrongNeighbour = await MoveAsync(ContentRecordKind.Offering, a, MoveDirection.Down, 2, 2);

        Assert.Equal(OrderingOutcome.Moved, first.Outcome);
        Assert.Equal(OrderingOutcome.OrderChanged, second.Outcome);
        Assert.Equal(OrderingOutcome.OrderChanged, wrongNeighbour.Outcome);
        Assert.Equal(["B", "A", "C"], await NamesInOrderAsync(ContentRecordKind.Offering));
        Assert.Equal("2, 1", await OrdersAsync(ContentRecordKind.Offering, [a, b]));
    }

    [Fact]
    public async Task TwoRecordsWithTheSameOrder_AreRenumberedOneByOne_AndTheMoveHoldsTheShownOrder_KK2()
    {
        Guid a = await SeedKindAsync(ContentRecordKind.PortfolioItem, "A", 5);
        Guid b = await SeedKindAsync(ContentRecordKind.PortfolioItem, "B", 5);
        Guid c = await SeedKindAsync(ContentRecordKind.PortfolioItem, "C", 5);

        // The list shows A, B, C (the order, then the time of creation); B moves up.
        OrderingResult result = await MoveAsync(ContentRecordKind.PortfolioItem, b, MoveDirection.Up, 5, 5);

        Assert.Equal(OrderingOutcome.Moved, result.Outcome);
        Assert.Equal(["B", "A", "C"], await NamesInOrderAsync(ContentRecordKind.PortfolioItem));
        Assert.Equal("2, 1, 3", await OrdersAsync(ContentRecordKind.PortfolioItem, [a, b, c]));
    }

    [Fact]
    public async Task TheFirstCannotGoUp_TheLastCannotGoDown_AnUnknownRecordIsNotFound_KK2()
    {
        Guid a = await SeedKindAsync(ContentRecordKind.FaqItem, "A", 1);
        Guid b = await SeedKindAsync(ContentRecordKind.FaqItem, "B", 2);

        Assert.Equal(OrderingOutcome.AtEdge, (await MoveAsync(ContentRecordKind.FaqItem, a, MoveDirection.Up, 1, 0)).Outcome);
        Assert.Equal(OrderingOutcome.AtEdge, (await MoveAsync(ContentRecordKind.FaqItem, b, MoveDirection.Down, 2, 3)).Outcome);
        Assert.Equal(OrderingOutcome.NotFound, (await MoveAsync(ContentRecordKind.FaqItem, Guid.CreateVersion7(), MoveDirection.Up, 1, 0)).Outcome);
        Assert.Equal(["A", "B"], await NamesInOrderAsync(ContentRecordKind.FaqItem));
    }

    [Fact]
    public async Task TheAboutPageAndTheAnnouncement_HaveNoOrder_AndAreRefused_KK2()
    {
        Guid about = await SeedAboutAsync("Kısa tanıtım", PublishStatus.Published);

        Assert.Equal(OrderingOutcome.NotOrderable, (await MoveAsync(ContentRecordKind.AboutPage, about, MoveDirection.Up, 1, 1)).Outcome);
        Assert.Equal(OrderingOutcome.NotOrderable, (await MoveAsync(ContentRecordKind.Announcement, Guid.CreateVersion7(), MoveDirection.Down, 1, 1)).Outcome);
    }

    [Fact]
    public async Task TheListPage_TheHomePageBlock_AndTheMenu_FollowTheSameOrder_AProductListIsNotTouched_KK2()
    {
        Guid category = await SeedCategoryAsync("Mobilya");
        await SeedProductAsync("Masa", PublishStatus.Published, category);
        await SeedProductAsync("Sandalye", PublishStatus.Published, category);
        Guid one = await SeedKindAsync(ContentRecordKind.Offering, "Bir", 1, flagged: true);
        await SeedKindAsync(ContentRecordKind.Offering, "İki", 2, flagged: true);
        Guid three = await SeedKindAsync(ContentRecordKind.Offering, "Üç", 3, flagged: true);
        Guid pageOne = await SeedKindAsync(ContentRecordKind.CustomPage, "Sayfa bir", 1, flagged: true);
        await SeedKindAsync(ContentRecordKind.CustomPage, "Sayfa iki", 2, flagged: true);

        HomepageView homeBefore = await HomeAsync();
        StorefrontMenu menuBefore = await MenuAsync();
        ContentCardPage listBefore = await ListOfferingsAsync();
        IReadOnlyList<Guid> showcaseBefore = [.. homeBefore.Showcase!.Products.Select(p => p.Id)];

        Assert.Equal(OrderingOutcome.Moved, (await MoveAsync(ContentRecordKind.Offering, three, MoveDirection.Up, 3, 2)).Outcome);
        Assert.Equal(OrderingOutcome.Moved, (await MoveAsync(ContentRecordKind.Offering, one, MoveDirection.Down, 1, 2)).Outcome);
        Assert.Equal(OrderingOutcome.Moved, (await MoveAsync(ContentRecordKind.CustomPage, pageOne, MoveDirection.Down, 1, 2)).Outcome);

        HomepageView homeAfter = await HomeAsync();
        StorefrontMenu menuAfter = await MenuAsync();
        ContentCardPage listAfter = await ListOfferingsAsync();

        Assert.Equal(["Bir", "İki", "Üç"], listBefore.Cards.Select(c => c.Title));
        Assert.Equal(["Üç", "Bir", "İki"], listAfter.Cards.Select(c => c.Title));
        Assert.Equal(["Üç", "Bir", "İki"], homeAfter.Offerings!.Cards.Select(c => c.Title));
        Assert.Equal(["Sayfa bir", "Sayfa iki"], menuBefore.FooterPages.Select(p => p.Title));
        Assert.Equal(["Sayfa iki", "Sayfa bir"], menuAfter.FooterPages.Select(p => p.Title));
        Assert.Equal(["Sayfa iki", "Sayfa bir", "İletişim"], menuAfter.VisibleItems.Where(i => i.Kind is MenuItemKind.GeneralPage or MenuItemKind.Contact).Select(i => i.Name));
        Assert.Equal(showcaseBefore, homeAfter.Showcase!.Products.Select(p => p.Id));
    }

    // ---- KK3: the gate of every kind ----

    [Theory]
    [InlineData("AboutPage")]
    [InlineData("Offering")]
    [InlineData("PortfolioItem")]
    [InlineData("FaqItem")]
    [InlineData("Branch")]
    [InlineData("CustomPage")]
    [InlineData("Announcement")]
    public async Task ARecordWithAnEmptyRequiredField_CannotBePublished_AndStaysADraft_KK3(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        Guid id = await SeedIncompleteAsync(kind);

        ContentResult result = await PublishAsync(kind, id);

        Assert.Equal(ContentOutcome.GateIncomplete, result.Outcome);
        Assert.NotEmpty(result.Missing);
        Assert.All(result.Missing, field => Assert.Contains(field, ContentGate.RequiredFields(kind)));
        Assert.Equal(PublishStatus.Draft, await StatusOfAsync(kind, id));
    }

    [Fact]
    public async Task NoTypeIsRequired_ASiteWithNoContentRecordHasNoGateToPass_KK3()
    {
        await using AppDbContext db = NewContext();

        Assert.Equal(0, await db.Offerings.CountAsync(Ct) + await db.PortfolioItems.CountAsync(Ct) + await db.FaqItems.CountAsync(Ct) + await db.Branches.CountAsync(Ct) + await db.CustomPages.CountAsync(Ct) + await db.AboutPages.CountAsync(Ct));
        HomepageView home = await HomeAsync();
        Assert.Equal([HomepageBlockKind.CorporateIntro], home.Blocks);
    }

    // ---- KK4: the empty about page ----

    [Theory]
    [InlineData("none")]
    [InlineData("draft")]
    [InlineData("empty")]
    public async Task AnAboutPage_NotThereDraftOrEmpty_GivesNoIntroduction_AndTheBlockStaysForTheBrand_KK4(string state)
    {
        switch (state)
        {
            case "draft":
                await SeedAboutAsync("Kısa tanıtım", PublishStatus.Draft);
                break;
            case "empty":
                await SeedAboutAsync(null, PublishStatus.Published);
                break;
        }

        HomepageView home = await HomeAsync();

        Assert.Null(home.Intro);
        Assert.Contains(HomepageBlockKind.CorporateIntro, home.Blocks);
    }

    // ---- KK5: the galleries ----

    [Fact]
    public async Task TheMainImage_IsAMarkOfItsOwn_NotTheFirst_AndExactlyOneStays_KK5()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid[] images = [await SeedImageAsync(), await SeedImageAsync(), await SeedImageAsync()];

        await GalleryAsync(ContentRecordKind.Offering, offering, [new(images[0], null, false), new(images[1], null, true), new(images[2], null, false)]);
        IReadOnlyList<GalleryImage> first = await ReadGalleryAsync(ContentRecordKind.Offering, offering);
        await GalleryAsync(ContentRecordKind.Offering, offering, [new(images[2], null, false), new(images[1], null, false), new(images[0], null, false)]);
        IReadOnlyList<GalleryImage> reordered = await ReadGalleryAsync(ContentRecordKind.Offering, offering);
        await GalleryAsync(ContentRecordKind.Offering, offering, [new(images[2], null, false), new(images[1], null, false), new(images[0], null, true)]);
        IReadOnlyList<GalleryImage> moved = await ReadGalleryAsync(ContentRecordKind.Offering, offering);

        Assert.Equal(images[1], first.Single(i => i.IsMain).ImageId);
        Assert.Equal(2, first.Single(i => i.IsMain).Position);
        Assert.Equal([images[2], images[1], images[0]], reordered.Select(i => i.ImageId));
        Assert.Equal(images[1], reordered.Single(i => i.IsMain).ImageId);
        Assert.Equal(images[0], moved.Single(i => i.IsMain).ImageId);
    }

    [Fact]
    public async Task AGallerySave_DropsTheHomePageBlocks_SoTheCardShowsTheNewMainImageAtOnce_KK5()
    {
        Guid offering = await SeedKindAsync(ContentRecordKind.Offering, "Montaj", 1, flagged: true);
        Guid[] images = [await SeedImageAsync(), await SeedImageAsync()];
        HomepageView before = await HomeAsync();

        await GalleryAsync(ContentRecordKind.Offering, offering, [new(images[0], null, false), new(images[1], null, true)]);
        HomepageView after = await HomeAsync();
        await GalleryAsync(ContentRecordKind.Offering, offering, [new(images[0], null, true), new(images[1], null, false)]);
        HomepageView swapped = await HomeAsync();

        Assert.Null(before.Offerings!.Cards.Single().MainImage);
        Assert.Equal(images[1], after.Offerings!.Cards.Single().MainImage?.ImageId);
        Assert.Equal(images[0], swapped.Offerings!.Cards.Single().MainImage?.ImageId);
    }

    [Fact]
    public async Task WithoutAnAltText_TheNameAndThePlaceAreUsed_WithTheFirmsTextTheFirmsTextIs_KK5()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid[] images = [await SeedImageAsync(), await SeedImageAsync(), await SeedImageAsync()];

        await GalleryAsync(ContentRecordKind.Offering, offering, [new(images[0], null, true), new(images[1], "  ", false), new(images[2], " Atölye girişi ", false)]);
        IReadOnlyList<GalleryImage> gallery = await ReadGalleryAsync(ContentRecordKind.Offering, offering);

        Assert.Equal(["Montaj — görsel 1 / 3", "Montaj — görsel 2 / 3", "Atölye girişi"], gallery.Select(i => i.EffectiveAltText));
        Assert.Equal([null, null, "Atölye girişi"], gallery.Select(i => i.AltText));
    }

    [Fact]
    public async Task TheAboutPageAndTheQuestion_NameTheirImages_ByTheTypesNameAndByTheQuestion_KK5()
    {
        Guid about = await SeedAboutAsync("Kısa tanıtım", PublishStatus.Published);
        Guid faq = await SeedKindAsync(ContentRecordKind.FaqItem, "İade var mı?", 1);
        Guid[] images = [await SeedImageAsync(), await SeedImageAsync()];

        await GalleryAsync(ContentRecordKind.AboutPage, about, [new(images[0], null, true), new(images[1], null, false)]);
        await GalleryAsync(ContentRecordKind.FaqItem, faq, [new(images[0], null, true)]);

        Assert.Equal(["Hakkımızda — görsel 1 / 2", "Hakkımızda — görsel 2 / 2"], (await ReadGalleryAsync(ContentRecordKind.AboutPage, about)).Select(i => i.EffectiveAltText));
        Assert.Equal(["İade var mı? — görsel 1 / 1"], (await ReadGalleryAsync(ContentRecordKind.FaqItem, faq)).Select(i => i.EffectiveAltText));
    }

    [Fact]
    public async Task TheTwentyFirstImage_IsRefused_AndNothingIsWritten_KK5()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        List<GalleryEntry> entries = [];
        for (int i = 0; i < ProductConstants.ImagesPerContentRecordMax; i++)
        {
            entries.Add(new GalleryEntry(await SeedImageAsync(), null, i == 0));
        }

        ContentGalleryResult full = await GalleryAsync(ContentRecordKind.Offering, offering, entries);
        ContentGalleryResult over = await GalleryAsync(ContentRecordKind.Offering, offering, [.. entries, new GalleryEntry(await SeedImageAsync(), null, false)]);

        Assert.Equal(ContentGalleryOutcome.Saved, full.Outcome);
        Assert.Equal(ContentGalleryOutcome.LimitExceeded, over.Outcome);
        Assert.Equal(ProductConstants.ImagesPerContentRecordMax, (await ReadGalleryAsync(ContentRecordKind.Offering, offering)).Count);
    }

    [Fact]
    public async Task ADuplicateImage_AnUnknownImage_AnUnknownRecord_AndAKindWithNoGallery_AreRefused_KK5()
    {
        Guid offering = await SeedOfferingAsync("Montaj", 1);
        Guid branch = await SeedKindAsync(ContentRecordKind.Branch, "Merkez", 1);
        Guid image = await SeedImageAsync();

        Assert.Equal(ContentGalleryOutcome.DuplicateImage, (await GalleryAsync(ContentRecordKind.Offering, offering, [new(image, null, true), new(image, null, false)])).Outcome);
        Assert.Equal(ContentGalleryOutcome.ImageNotFound, (await GalleryAsync(ContentRecordKind.Offering, offering, [new(Guid.CreateVersion7(), null, true)])).Outcome);
        Assert.Equal(ContentGalleryOutcome.OwnerNotFound, (await GalleryAsync(ContentRecordKind.Offering, Guid.CreateVersion7(), [new(image, null, true)])).Outcome);
        Assert.Equal(ContentGalleryOutcome.NotAGalleryKind, (await GalleryAsync(ContentRecordKind.Branch, branch, [new(image, null, true)])).Outcome);
        Assert.Equal(ContentGalleryOutcome.NotAGalleryKind, (await GalleryAsync(ContentRecordKind.Announcement, Guid.CreateVersion7(), [])).Outcome);
        Assert.Empty(await ReadGalleryAsync(ContentRecordKind.Offering, offering));
    }

    [Fact]
    public async Task TheBranch_HasOneImageBesideItsText_TheSaveCarriesTheVersion_AndTheTextIsGenerated_KK5()
    {
        Guid branch = await SeedKindAsync(ContentRecordKind.Branch, "Merkez", 1);
        Guid image = await SeedImageAsync();
        int version = await VersionOfAsync(ContentRecordKind.Branch, branch);

        ContentGalleryResult saved = await BranchImageAsync(branch, image, null, version);
        ContentGalleryResult stale = await BranchImageAsync(branch, null, null, version);
        ContentGalleryResult missing = await BranchImageAsync(branch, Guid.CreateVersion7(), null, version + 1);

        Assert.Equal(ContentGalleryOutcome.Saved, saved.Outcome);
        Assert.Equal(version + 1, saved.Version);
        Assert.Equal(ContentGalleryOutcome.VersionConflict, stale.Outcome);
        Assert.Equal(ContentGalleryOutcome.ImageNotFound, missing.Outcome);
        await using (AsyncServiceScope scope = _provider.CreateAsyncScope())
        {
            BranchImageView? view = await scope.ServiceProvider.GetRequiredService<ContentGallery>().ReadBranchImageAsync(branch, Ct);
            Assert.NotNull(view);
            Assert.Equal(image, view.ImageId);
            Assert.Equal("Merkez — görsel 1 / 1", view.EffectiveAltText);
        }

        ContentGalleryResult removed = await BranchImageAsync(branch, null, "Metin", version + 1);
        Assert.Equal(ContentGalleryOutcome.Saved, removed.Outcome);
        await using AsyncServiceScope after = _provider.CreateAsyncScope();
        Assert.Null(await after.ServiceProvider.GetRequiredService<ContentGallery>().ReadBranchImageAsync(branch, Ct));
    }

    // ---- KK6: the long text ----

    [Fact]
    public async Task TheLongText_OfARecord_IsKeptInTheFormatSet_NoFrameNoPlayer_TheVideoLinkStays_KK6()
    {
        Offering offering = Offering.Create("Montaj", "montaj", 1, Now);
        offering.SetText("<p>Anlatım</p><iframe src=\"https://video.example/e\"></iframe><video src=\"a.mp4\"></video><p><a href=\"https://www.youtube.com/watch?v=abc\">İzle</a></p>");
        await SaveAsync(c => c.Offerings.Add(offering));

        await using AppDbContext db = NewContext();
        Offering stored = await db.Offerings.AsNoTracking().SingleAsync(o => o.Id == offering.Id, Ct);

        Assert.NotNull(stored.Text);
        Assert.DoesNotContain("iframe", stored.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<video", stored.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"https://www.youtube.com/watch?v=abc\"", stored.Text, StringComparison.Ordinal);
        Assert.Contains("<p>Anlatım</p>", stored.Text, StringComparison.Ordinal);
    }

    // ---- page boundary: equal order and equal moments across the page edge (09 10.5 item 6, 25) ----

    [Fact]
    public async Task TheOfferingsPage_WithEqualOrderAndCreatedAtAcrossThePageBoundary_RepeatsNoCardAndLosesNone_DraftsNotCounted_TheLastPageIncluded()
    {
        (List<Guid> expected, List<Guid> walked) = await WalkListBoundaryAsync(ContentRecordKind.Offering);
        Assert.Equal(expected, walked);
    }

    [Fact]
    public async Task ThePortfolioPage_WithEqualOrderAndCreatedAtAcrossThePageBoundary_RepeatsNoCardAndLosesNone_DraftsNotCounted_TheLastPageIncluded()
    {
        (List<Guid> expected, List<Guid> walked) = await WalkListBoundaryAsync(ContentRecordKind.PortfolioItem);
        Assert.Equal(expected, walked);
    }

    /// <summary>Seeds published and draft records with one order and one creation moment, walks both pages, checks the page numbers, and returns the key-ordered published ids and the walked ids.</summary>
    private async Task<(List<Guid> Expected, List<Guid> Walked)> WalkListBoundaryAsync(ContentRecordKind kind)
    {
        int size = PageSizes.ContentList;
        int published = size + 10;
        for (int i = 0; i < published; i++)
        {
            await SeedEqualAsync(kind, PublishStatus.Published, $"Yayında {i:00}");
        }

        for (int i = 0; i < 5; i++)
        {
            await SeedEqualAsync(kind, PublishStatus.Draft, $"Taslak {i:00}");
        }

        List<Guid> expected = await PublishedIdsByKeyAsync(kind);
        Assert.Equal(published, expected.Count);

        ContentCardPage first = await ListContentAsync(kind, 1);
        Assert.Equal(published, first.TotalCount);
        Assert.Equal(2, first.PageCount);
        Assert.Equal(size, first.Cards.Count);

        ContentCardPage second = await ListContentAsync(kind, 2);
        Assert.Equal(2, second.PageNumber);
        Assert.Equal(published - size, second.Cards.Count);

        List<Guid> walked = [.. first.Cards.Select(c => c.Id), .. second.Cards.Select(c => c.Id)];
        Assert.Equal(published, walked.Distinct().Count());

        // A page past the last one gives the last page; a page below the first gives the first.
        ContentCardPage past = await ListContentAsync(kind, 9);
        Assert.Equal(2, past.PageNumber);
        Assert.Equal(second.Cards.Select(c => c.Id), past.Cards.Select(c => c.Id));
        Assert.Equal(1, (await ListContentAsync(kind, 0)).PageNumber);
        return (expected, walked);
    }

    // ---- operations ----

    private async Task<ProductLinksResult> LinkAsync(ContentLinkOwner owner, Guid ownerId, IReadOnlyList<Guid> productIds)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentProductLinks links = scope.ServiceProvider.GetRequiredService<ContentProductLinks>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => links.SaveAsync(owner, ownerId, productIds, token), Ct);
    }

    private async Task<IReadOnlyList<Guid>> LinkedIdsAsync(ContentLinkOwner owner, Guid ownerId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ContentProductLinks>().ListLinkedIdsAsync(owner, ownerId, Ct);
    }

    private async Task<IReadOnlyList<ShowcaseProduct>> RelatedAsync(ContentLinkOwner owner, Guid ownerId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ContentProductLinks>().ListRelatedAsync(owner, ownerId, Ct);
    }

    private async Task ChangeStatusAsync(Guid product, PublishStatus status, int version)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IProductTransitions transitions = scope.ServiceProvider.GetRequiredService<IProductTransitions>();
        PublishChangeResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
            .RunAsync(token => transitions.ChangeProductStatusAsync(product, status, version, token), Ct);
        Assert.True(result.IsChanged);
    }

    private async Task DeleteProductAsync(Guid productId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int version = (await db.Products.AsNoTracking().SingleAsync(p => p.Id == productId, Ct)).Version;
        IProductRemoval removal = scope.ServiceProvider.GetRequiredService<IProductRemoval>();
        RemovalResult result = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
            .RunAsync(token => removal.DeleteProductAsync(productId, version, token), Ct);
        Assert.True(result.IsDone);
    }

    private async Task<OrderingResult> MoveAsync(ContentRecordKind kind, Guid id, MoveDirection direction, int shownOrder, int shownNeighbourOrder)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentOrdering ordering = scope.ServiceProvider.GetRequiredService<ContentOrdering>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => ordering.MoveAsync(kind, id, direction, shownOrder, shownNeighbourOrder, token), Ct);
    }

    private async Task<ContentResult> PublishAsync(ContentRecordKind kind, Guid id)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentPublishing publishing = scope.ServiceProvider.GetRequiredService<ContentPublishing>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => publishing.PublishAsync(kind, id, 1, token), Ct);
    }

    private async Task<ContentGalleryResult> GalleryAsync(ContentRecordKind kind, Guid ownerId, IReadOnlyList<GalleryEntry> entries)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentGallery gallery = scope.ServiceProvider.GetRequiredService<ContentGallery>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => gallery.SaveAsync(kind, ownerId, entries, token), Ct);
    }

    private async Task<ContentGalleryResult> BranchImageAsync(Guid branchId, Guid? imageId, string? altText, int version)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentGallery gallery = scope.ServiceProvider.GetRequiredService<ContentGallery>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => gallery.SetBranchImageAsync(branchId, imageId, altText, version, token), Ct);
    }

    private async Task<IReadOnlyList<GalleryImage>> ReadGalleryAsync(ContentRecordKind kind, Guid ownerId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ContentGallery>().ReadAsync(kind, ownerId, Ct);
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

    private async Task<ContentCardPage> ListOfferingsAsync()
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ContentReaders>().ListOfferingsAsync(1, Ct);
    }

    private async Task<ContentCardPage> ListContentAsync(ContentRecordKind kind, int page)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        ContentReaders readers = scope.ServiceProvider.GetRequiredService<ContentReaders>();
        return kind == ContentRecordKind.Offering ? await readers.ListOfferingsAsync(page, Ct) : await readers.ListPortfolioItemsAsync(page, Ct);
    }

    // ---- reads of the stored state ----

    /// <summary>The ids of the published records of the kind in the order of the database's own key comparison (the last tie-break of the list).</summary>
    private async Task<List<Guid>> PublishedIdsByKeyAsync(ContentRecordKind kind)
    {
        await using AppDbContext db = NewContext();
        return kind == ContentRecordKind.Offering
            ? await db.Offerings.AsNoTracking().Where(r => r.PublishStatus == PublishStatus.Published).OrderBy(r => r.Id).Select(r => r.Id).ToListAsync(Ct)
            : await db.PortfolioItems.AsNoTracking().Where(r => r.PublishStatus == PublishStatus.Published).OrderBy(r => r.Id).Select(r => r.Id).ToListAsync(Ct);
    }

    private async Task<int> VersionOfAsync(ContentRecordKind kind, Guid id)
    {
        await using AppDbContext db = NewContext();
        return kind switch
        {
            ContentRecordKind.Offering => (await db.Offerings.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).Version,
            ContentRecordKind.PortfolioItem => (await db.PortfolioItems.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).Version,
            ContentRecordKind.FaqItem => (await db.FaqItems.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).Version,
            ContentRecordKind.Branch => (await db.Branches.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).Version,
            ContentRecordKind.CustomPage => (await db.CustomPages.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).Version,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private async Task<PublishStatus> StatusOfAsync(ContentRecordKind kind, Guid id)
    {
        await using AppDbContext db = NewContext();
        return kind switch
        {
            ContentRecordKind.AboutPage => (await db.AboutPages.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            ContentRecordKind.Offering => (await db.Offerings.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            ContentRecordKind.PortfolioItem => (await db.PortfolioItems.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            ContentRecordKind.FaqItem => (await db.FaqItems.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            ContentRecordKind.Branch => (await db.Branches.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            ContentRecordKind.CustomPage => (await db.CustomPages.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            ContentRecordKind.Announcement => (await db.Announcements.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).PublishStatus,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private async Task<List<string>> NamesInOrderAsync(ContentRecordKind kind)
    {
        await using AppDbContext db = NewContext();
        return kind switch
        {
            ContentRecordKind.Offering => await db.Offerings.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => r.Name).ToListAsync(Ct),
            ContentRecordKind.PortfolioItem => await db.PortfolioItems.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => r.Title).ToListAsync(Ct),
            ContentRecordKind.FaqItem => await db.FaqItems.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => r.Question).ToListAsync(Ct),
            ContentRecordKind.Branch => await db.Branches.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => r.Name).ToListAsync(Ct),
            ContentRecordKind.CustomPage => await db.CustomPages.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id).Select(r => r.Title).ToListAsync(Ct),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private async Task<string> OrdersAsync(ContentRecordKind kind, Guid[] ids)
    {
        await using AppDbContext db = NewContext();
        List<int> orders = [];
        foreach (Guid id in ids)
        {
            orders.Add(kind switch
            {
                ContentRecordKind.Offering => (await db.Offerings.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).SortOrder,
                ContentRecordKind.PortfolioItem => (await db.PortfolioItems.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).SortOrder,
                ContentRecordKind.FaqItem => (await db.FaqItems.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).SortOrder,
                ContentRecordKind.Branch => (await db.Branches.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).SortOrder,
                ContentRecordKind.CustomPage => (await db.CustomPages.AsNoTracking().SingleAsync(r => r.Id == id, Ct)).SortOrder,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            });
        }

        return string.Join(", ", orders);
    }

    private async Task<List<string>> ColumnsOfAsync(string table)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = @t ORDER BY column_name", connection);
        command.Parameters.AddWithValue("t", table);
        List<string> columns = [];
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    // ---- seeding ----

    private async Task<Guid> SeedCategoryAsync(string name)
    {
        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create(name, $"kategori-{n:D3}", null, 1, Now.AddMinutes(n));
        await SaveAsync(c => c.Categories.Add(category));
        return category.Id;
    }

    private async Task<Guid> SeedProductAsync(string name, PublishStatus status, Guid category)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = status;

        // The publish gate (T335) refuses a product without a published variant; a draft seed carries one so the test can publish it through the transition.
        Variant? variant = null;
        if (status == PublishStatus.Draft)
        {
            product.FulfilmentPeriod = 3;
            variant = Variant.Create(product.Id, $"SKU-{n:D4}", 100m, Now.AddMinutes(n));
            variant.PublishStatus = PublishStatus.Published;
            variant.StockQuantity = 10;
        }

        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.ProductCategories.Add(ProductCategory.Create(product.Id, category, true, Now));
            if (variant is not null)
            {
                c.Variants.Add(variant);
            }
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

    private async Task<Guid> SeedAboutAsync(string? shortIntroduction, PublishStatus status)
    {
        AboutPage page = AboutPage.Create(Now);
        page.ShortIntroduction = shortIntroduction;
        page.PublishStatus = status;
        await SaveAsync(c => c.AboutPages.Add(page));
        return page.Id;
    }

    private Task<Guid> SeedOfferingAsync(string name, int order) => SeedKindAsync(ContentRecordKind.Offering, name, order);

    private Task<Guid> SeedPortfolioAsync(string title, int order) => SeedKindAsync(ContentRecordKind.PortfolioItem, title, order);

    /// <summary>A published record of one of the five ordered kinds; <paramref name="flagged"/> shows it on the home page (a service or a reference) or in the menu (a general page).</summary>
    private async Task<Guid> SeedKindAsync(ContentRecordKind kind, string name, int order, bool flagged = false)
    {
        int n = Interlocked.Increment(ref _counter);
        DateTimeOffset created = Now.AddMinutes(n);
        switch (kind)
        {
            case ContentRecordKind.Offering:
                Offering offering = Offering.Create(name, $"hizmet-{n:D3}", order, created);
                offering.PublishStatus = PublishStatus.Published;
                offering.IsShownOnHomepage = flagged;
                await SaveAsync(c => c.Offerings.Add(offering));
                return offering.Id;
            case ContentRecordKind.PortfolioItem:
                PortfolioItem work = PortfolioItem.Create(name, $"referans-{n:D3}", order, created);
                work.PublishStatus = PublishStatus.Published;
                work.IsShownOnHomepage = flagged;
                await SaveAsync(c => c.PortfolioItems.Add(work));
                return work.Id;
            case ContentRecordKind.FaqItem:
                FaqItem faq = FaqItem.Create(name, order, created);
                faq.PublishStatus = PublishStatus.Published;
                await SaveAsync(c => c.FaqItems.Add(faq));
                return faq.Id;
            case ContentRecordKind.Branch:
                Branch branch = Branch.Create(name, order, created);
                await SaveAsync(c => c.Branches.Add(branch));
                return branch.Id;
            case ContentRecordKind.CustomPage:
                CustomPage page = CustomPage.Create(name, $"sayfa-{n:D3}", order, created);
                page.PublishStatus = PublishStatus.Published;
                page.IsShownInMenu = flagged;
                await SaveAsync(c => c.CustomPages.Add(page));
                return page.Id;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    /// <summary>A record of a list kind with the same order and the same creation moment as every other one the page-boundary test seeds.</summary>
    private async Task SeedEqualAsync(ContentRecordKind kind, PublishStatus status, string name)
    {
        int n = Interlocked.Increment(ref _counter);
        if (kind == ContentRecordKind.Offering)
        {
            Offering offering = Offering.Create(name, $"hizmet-{n:D3}", 1, Now);
            offering.PublishStatus = status;
            await SaveAsync(c => c.Offerings.Add(offering));
        }
        else
        {
            PortfolioItem work = PortfolioItem.Create(name, $"referans-{n:D3}", 1, Now);
            work.PublishStatus = status;
            await SaveAsync(c => c.PortfolioItems.Add(work));
        }
    }

    /// <summary>A draft record of the kind with one required field empty (the gate of 02 3.27.13).</summary>
    private async Task<Guid> SeedIncompleteAsync(ContentRecordKind kind)
    {
        int n = Interlocked.Increment(ref _counter);
        switch (kind)
        {
            case ContentRecordKind.AboutPage:
                AboutPage about = AboutPage.Create(Now);
                await SaveAsync(c => c.AboutPages.Add(about));
                return about.Id;
            case ContentRecordKind.Offering:
                Offering offering = Offering.Create("Montaj", $"hizmet-{n:D3}", 1, Now);
                offering.Name = "   ";
                await SaveAsync(c => c.Offerings.Add(offering));
                return offering.Id;
            case ContentRecordKind.PortfolioItem:
                PortfolioItem work = PortfolioItem.Create("Ofis", $"referans-{n:D3}", 1, Now);
                work.Title = "   ";
                await SaveAsync(c => c.PortfolioItems.Add(work));
                return work.Id;
            case ContentRecordKind.FaqItem:
                FaqItem faq = FaqItem.Create("İade var mı?", 1, Now);
                await SaveAsync(c => c.FaqItems.Add(faq));
                return faq.Id;
            case ContentRecordKind.Branch:
                Branch branch = Branch.Create("Merkez", 1, Now);
                await SaveAsync(c => c.Branches.Add(branch));
                return branch.Id;
            case ContentRecordKind.CustomPage:
                CustomPage page = CustomPage.Create("Sayfa", $"sayfa-{n:D3}", 1, Now);
                page.Title = "   ";
                await SaveAsync(c => c.CustomPages.Add(page));
                return page.Id;
            case ContentRecordKind.Announcement:
                Announcement announcement = Announcement.Create(Now);
                await SaveAsync(c => c.Announcements.Add(announcement));
                return announcement.Id;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
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

    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }
}
